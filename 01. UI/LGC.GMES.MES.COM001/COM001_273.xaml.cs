/*************************************************************************************
 Created Date : 2019.09.20
      Creator : 
   Decription : RTS 공정별 재고 과부족
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.20  : Initial Created.  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_273.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_273 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sCompany = string.Empty;
        private string sDivision = string.Empty;
        private string sSite = string.Empty;
        private string sPlant = string.Empty;

        private string sPlanID = string.Empty;
        private string sCutOffYMD = string.Empty;
        private string sCutOffHour = string.Empty;

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_273()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize();
                getPlanID();
            }));
        }

        private void Initialize()
        {
            InitCombo();
        }

        private void InitCombo()
        {
            C1ComboBox[] cboAreaChild = { cboProcess };
            SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            C1ComboBox[] cboProcessParent = { cboArea };
            SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            C1ComboBox[] cboLineParent = { cboArea, cboProcess };
            SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);

            String[] sFilter3 = { "ELEC_TYPE" };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);

            cboProcess.SelectedValue = Process.SLITTING;
        }
        #endregion

        #region Event
        #region # 과부족 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            initGrid();
            SearchOverListdData();
        }
        #endregion

        #region # 재고 상세조회
        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {
            if (dgT2List.GetRowCount() > 0)
            {
                COM001_274 wndInventory = new COM001.COM001_274();
                wndInventory.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = sCutOffYMD;
                Parameters[1] = sCutOffHour;
                Parameters[2] = Util.GetCondition(cboProcess);
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgT2List.CurrentRow.DataItem, "ITEM_ID"));
                Parameters[4] = Util.GetCondition(txtMSS);

                this.FrameOperation.OpenMenu("SFU010010303", true, Parameters);
            }
        }
        #endregion

        #region # 계획 상세조회
        private void btnPlanList_Click(object sender, RoutedEventArgs e)
        {
            if (dgT3List.GetRowCount() > 0)
            {
                COM001_275 wndInventory = new COM001.COM001_275();
                wndInventory.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = Util.GetCondition(cboProcess);
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgT3List.CurrentRow.DataItem, "ITEM_ID"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgT3List.CurrentRow.DataItem, "FACTORY_ID"));

                this.FrameOperation.OpenMenu("SFU050010305", true, Parameters);
            }
        }
        #endregion

        #region #Grid Event
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MEASURE"));
                        string sProdYN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_YN"));
                        if (sDataType.Equals("BALANCE"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);

                            if (Util.NVC(e.Cell.Column.Name).StartsWith("T_"))
                            {
                                if (Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) < 0)
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                            }
                            else
                            {
                                if (e.Cell.Column.Name.Equals("ITEM_ID"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    if (sProdYN.Equals("N"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                    else
                                        e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        else if (sDataType.Equals("WIP_HOLD"))
                        {
                            if (Util.NVC(e.Cell.Column.Name).StartsWith("T_"))
                            {
                                if (Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) > 0)
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                if (e.Cell.Column.Name.Equals("ITEM_ID"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    if (sProdYN.Equals("N"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                    else
                                        e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        else
                        {
                            if (e.Cell.Column.Name.Equals("ITEM_ID"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                if (sProdYN.Equals("N"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                else
                                    e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MEASURE"));
                        string sProdYN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_YN"));
                        if (sDataType.Equals("BALANCE"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);

                            if (Util.NVC(e.Cell.Column.Name).StartsWith("T_"))
                            {
                                if (Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) < 0)
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                            }
                            else
                            {
                                if (e.Cell.Column.Name.Equals("ITEM_ID"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    if (sProdYN.Equals("N"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                    else
                                        e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        else if (sDataType.Equals("WIP_HOLD"))
                        {
                            if (Util.NVC(e.Cell.Column.Name).StartsWith("T_"))
                            {
                                if (Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)) > 0)
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                if (e.Cell.Column.Name.Equals("ITEM_ID"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                    if (sProdYN.Equals("N"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                    else
                                        e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        else
                        {
                            if (e.Cell.Column.Name.Equals("ITEM_ID"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                if (sProdYN.Equals("N"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                                else
                                    e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("ITEM_ID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)  // 
                {
                    string _ITEMID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "ITEM_ID"));

                    getReqment(_ITEMID);
                    getWIP(_ITEMID);
                    getPlan(_ITEMID);
                    getCoaterWIP(_ITEMID);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion
        #endregion

        #region Mehod

        #region # Init Grid
        private void initGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgT1List);
            Util.gridClear(dgT2List);
            Util.gridClear(dgT3List);
            Util.gridClear(dgT4List);
        }
        #endregion

        #region # Grid Head
        private void setGridHead()
        {
            SetGridDate(dgList, 7);
            SetGridDate(dgT1List, 5);
            SetGridDate(dgT3List, 7);
        }
        #endregion

        #region # Combo
        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    SetCombo(cbChild);
                }
            }
        }
        public void SetCombo(C1ComboBox cb, CommonCombo.ComboStatus cs, C1ComboBox[] cbChild = null, C1ComboBox[] cbParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_cbo", cbParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {
                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
            }
        }

        private void SetCombo(C1ComboBox cb)
        {
            try
            {
                Hashtable hashTag = cb.Tag as Hashtable;
                CommonCombo.ComboStatus cs = (CommonCombo.ComboStatus)Enum.Parse(typeof(CommonCombo.ComboStatus), hashTag["all_status"].ToString());
                String[] sFilter = new String[10];
                

                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    C1ComboBox[] cbParentArray = hashTag["parent_cbo"] as C1ComboBox[];
                    int i = 0;
                    for (i = 0; i < cbParentArray.Length; i++)
                    {
                        if (cbParentArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                        }
                        else
                        {
                            sFilter[i] = "";
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1) {
                            sFilter[i] = s;
                            i++;
                        }
                        
                    }
                }                
                else if (hashTag.Contains("filter") && hashTag["filter"] != null)
                {
                    sFilter = hashTag["filter"] as String[];
                }
                else
                {
                    sFilter[0] = "";
                }

                switch (hashTag["combo_case"].ToString())
                {
                    case "cboArea":
                        SetComboArea(cb, cs, sFilter);
                        break;
                    case "cboProcess":
                        SetComboProcess(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegment":
                        SetComboEquipmentSegmant(cb, cs, sFilter);
                        break;
                    case "cboElecType":
                        SetComboEltr(cb, cs, sFilter);
                        break;
                    case "cboEquipment":
                        SetComboEquipment(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }        

        private void SetComboArea(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboProcess(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0]; ;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataTable result = result2.Clone();
                var drTmp = result2?.Select("CBO_CODE IN ('" + Process.ROLL_PRESSING + "','" + Process.SLITTING + "','" + Process.NOTCHING + "')");
                if (drTmp.Length > 0)
                    result = drTmp.CopyToDataTable();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipmentSegmant(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0];
                drnewrow["PROCID"] = sFilter[1];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEltr(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipment(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string)); 

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = sFilter[2];
                drnewrow["PROCID"] = sFilter[0]; ;
                drnewrow["ELTR_TYPE_CODE"] = Util.NVC(sFilter[1]).Equals("") ? null : sFilter[1];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetDefaultCbo(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new DataTable();

                dtResult.Columns.Add("CBO_CODE", typeof(string));
                dtResult.Columns.Add("CBO_NAME", typeof(string));

                DataRow newRow = dtResult.NewRow();

                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "NA", "구현안됨" };
                dtResult.Rows.Add(newRow);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region # Plan ID
        private void getPlanID()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PLANID", null, "RSLTDT", null);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    sPlanID = Util.NVC(dtMain.Rows[0]["PLAN_ID"]);
                    sCutOffYMD = Util.NVC(dtMain.Rows[0]["CUTOFF_YYYYMMDD"]);
                    sCutOffHour = Util.NVC(dtMain.Rows[0]["CUTOFF_HOUR"]);

                    txtMSS.Text = sCutOffYMD.Substring(0, 4) + '-' + sCutOffYMD.Substring(4, 2) + '-' + sCutOffYMD.Substring(6, 2) + ' ' + sCutOffHour + ":00"; //ObjectDic.Instance.GetObjectName("시"); 

                    setGridHead();
                }
            }
            catch (Exception ex) { loadingIndicator.Visibility = Visibility.Collapsed; }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region # 생산 과부족
        private void SearchOverListdData()
        {
            try
            {
                getPlanID();

                //if (loadingIndicator != null)
                //    loadingIndicator.Visibility = Visibility.Visible;
                //DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));
                IndataTable.Columns.Add("OUTBOUND_CHK", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["PLAN_ID"] = sPlanID;
                dtRow["ROUTE_ID"] = Util.GetCondition(cboProcess);
                dtRow["ELTR_TYPE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);
                dtRow["OUTBOUND_CHK"] = chkOutBoundFlag.IsChecked == true ? null : "Y";

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_SUM", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    getOverQty(dtResult);
                    DataTable dtResultMain = dtResult.Select("MEASURE <> 'WIP'").CopyToDataTable();
                    dtResultMain.DefaultView.Sort = "PJT ASC, ELTR_TYPE ASC, ITEM_ID ASC, ROW_SEQ ASC";
                    dtResultMain.AcceptChanges();
                    DataTable dt = dtResultMain.DefaultView.ToTable();
                    Util.GridSetData(dgList, dt, FrameOperation, false);

                    string[] sColumnName = new string[] { "PJT", "POLARITY", "ELTR_TYPE", "ITEM_ID" };
                    _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void getOverQty(DataTable dt)
        {
            int countRow = dt.Rows.Count;
            int countCol = dt.Columns.Count;
            int colIdx = dt.Columns.IndexOf("T_01");
            decimal _REQ = 0;
            decimal _WIP = 0;
            decimal _PLAN = 0;
            decimal _BALANCE = 0;

            for (int iCol = colIdx; iCol < countCol; iCol++)
            {
                for (int iRow = 0; iRow < countRow; iRow += 5)
                {
                    _REQ = Util.NVC_Int(dt.Rows[iRow].ItemArray[iCol]);
                    _WIP = Util.NVC_Int(dt.Rows[iRow + 1].ItemArray[iCol]);
                    _PLAN = Util.NVC_Int(dt.Rows[iRow + 3].ItemArray[iCol]);

                    if (iCol.Equals(colIdx))
                        _BALANCE = Util.NVC_Int(dt.Rows[iRow + 4].ItemArray[iCol]);
                    else
                        _BALANCE = Util.NVC_Int(dt.Rows[iRow + 4].ItemArray[iCol - 1]);

                    dt.Rows[iRow + 4][iCol] = (_BALANCE + _WIP + _PLAN) - _REQ;
                    dt.AcceptChanges();
                }
            }
        }
        #endregion

        #region # 소요량
        private void getReqment(string sITEM_ID)
        {
            try
            {
                Util.gridClear(dgT1List);

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["PLAN_ID"] = sPlanID;
                dtRow["ROUTE_ID"] = Util.GetCondition(cboProcess);
                dtRow["ITEM_ID"] = sITEM_ID;

                IndataTable.Rows.Add(dtRow);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_REQ_DETAIL", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgT1List, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 재고
        private void getWIP(string sITEM_ID)
        {
            try
            {
                Util.gridClear(dgT2List);

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUTOFF_YYYYMMDD", typeof(string));
                IndataTable.Columns.Add("CUTOFF_HOUR", typeof(string));
                IndataTable.Columns.Add("PSI_OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CUTOFF_YYYYMMDD"] = sCutOffYMD;
                dtRow["CUTOFF_HOUR"] = sCutOffHour;
                dtRow["PSI_OPERATION_ID"] = Util.GetCondition(cboProcess);
                dtRow["ITEM_ID"] = sITEM_ID;

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_WIP_DETAIL", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgT2List, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 계획
        private void getPlan(string sITEM_ID)
        {
            try
            {
                Util.gridClear(dgT3List);

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["PLAN_ID"] = sPlanID;
                dtRow["ROUTE_ID"] = Util.GetCondition(cboProcess);
                dtRow["ITEM_ID"] = sITEM_ID;

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_PLAN_DETAIL", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgT3List, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 재고
        private void getCoaterWIP(string sITEM_ID)
        {
            try
            {
                Util.gridClear(dgT4List);

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUTOFF_YYYYMMDD", typeof(string));
                IndataTable.Columns.Add("CUTOFF_HOUR", typeof(string));
                IndataTable.Columns.Add("PSI_OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CUTOFF_YYYYMMDD"] = sCutOffYMD;
                dtRow["CUTOFF_HOUR"] = sCutOffHour;
                dtRow["PSI_OPERATION_ID"] = Process.COATING;
                dtRow["ITEM_ID"] = sITEM_ID;

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_WIP_DETAIL", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgT4List, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # Grid
        private void SetGridDate(C1DataGrid dg, int col)
        {
            try
            {
                for (int iCol = dg.Columns.Count; iCol-- > col;)
                    dg.Columns.RemoveAt(iCol);

                int i = 0;
                int ValueToHour = 0;
                int tmpHour = 0;
                for (i = 1; i <= 24; i++)
                {
                    string dayColumnName = string.Empty;

                    if (i < 10)
                    {
                        dayColumnName = "T_0" + i.ToString();
                    }
                    else
                    {
                        dayColumnName = "T_" + i.ToString();
                    }

                    if (i == 1)
                        tmpHour = Util.StringToInt(sCutOffHour);
                    else
                        tmpHour = (Util.StringToInt(sCutOffHour) + i) - 1;

                    if (tmpHour > 24)
                        ValueToHour = tmpHour - 24;
                    else
                        ValueToHour = tmpHour;

                    string sHeader = string.Empty;
                    sHeader = ValueToHour < 10 ? "T_0" + Convert.ToString(ValueToHour) : "T_" + Convert.ToString(ValueToHour); // + ObjectDic.Instance.GetObjectName("시");

                    Util.SetGridColumnNumeric(dg, dayColumnName, null, sHeader, true, false, false, true, 80, HorizontalAlignment.Right, Visibility.Visible, "#,##0");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

    }
}
