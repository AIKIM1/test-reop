/*************************************************************************************
 Created Date : 2019.10.11
      Creator : 
   Decription : 계획 상세조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.11  : Initial Created. 
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
    /// COM001_275.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_275 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sPlanID = string.Empty;
        private string sCutOffYMD = string.Empty;
        private string sCutOffHour = string.Empty;
        private string sArea = string.Empty;
        private string sOperation = string.Empty;
        private string sItem = string.Empty;

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_275()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
            object[] parameters = this.FrameOperation.Parameters;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                getPlanID();
                setGridHeader(dgList, (bool)rdoEquipment.IsChecked ? "RESOURCE" : "ITEMID");

                if (parameters.Length > 0)
                {
                    sOperation = Util.NVC(parameters[0]);
                    sItem = Util.NVC(parameters[1]);
                    sArea = Util.NVC(parameters[2]);

                    cboArea.SelectedValue = sArea;
                    cboProcess.SelectedValue = sOperation;
                    txtITEM.Text = sItem;

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
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
            SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter3 = { "ELEC_TYPE" };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }
        #endregion

        #region Event
        #region # 계획 상세조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboEquipment.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return;
            }

            initGrid();

            if ((bool)rdoEquipment.IsChecked)
                SearchPlanListdDataEquipment();
            else
                SearchPlanListdDataItemID();
        }
        #endregion

        #region #Grid Event
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                string sDataTypeName = (bool)rdoEquipment.IsChecked ? "Total Load(%)" : "Total Qty";
                string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, (bool)rdoEquipment.IsChecked ? "PJT" : "RESOURCE_NAME"));

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (sDataType.Equals(sDataTypeName))
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
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

                string sDataTypeName = (bool)rdoEquipment.IsChecked ? "Total Load(%)" : "Total Qty";
                string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, (bool)rdoEquipment.IsChecked ? "PJT" : "RESOURCE_NAME"));

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (sDataType.Equals(sDataTypeName))
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            setGridHeader(dgList, (bool)rdoEquipment.IsChecked ? "RESOURCE" : "ITEMID");
        }

        #endregion

        #region Mehod

        #region # Init Grid
        private void initGrid()
        {
            Util.gridClear(dgList);
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

        private void cboProcess_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboElecType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
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

        private void SetCboEquipment()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                drnewrow["ELTR_TYPE_CODE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboEquipment.ItemsSource = DataTableConverter.Convert(result);
                cboEquipment.CheckAll();
            }
            catch (Exception ex)
            {

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
                }
            }
            catch (Exception ex) { loadingIndicator.Visibility = Visibility.Collapsed; }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region # 계획 상세조회
        private void SearchPlanListdDataEquipment()
        {
            try
            {
                getPlanID();
                setGridHeader(dgList, (bool)rdoEquipment.IsChecked ? "RESOURCE" : "ITEMID");

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("LINE_ID", typeof(string));
                IndataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["PLAN_ID"] = sPlanID;
                dtRow["ROUTE_ID"] = Util.GetCondition(cboProcess);
                dtRow["ELTR_TYPE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["EQUIPMENT_ID"] = cboEquipment.SelectedItemsToString;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                dtRow["LINE_ID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment); 
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_PLAN_SUM_BY_EQUIPMENT", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation, false);

                    string[] sColumnName = new string[] { "RESOURCE_NAME", "PJT" };
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

        private void SearchPlanListdDataItemID()
        {
            try
            {
                getPlanID();
                setGridHeader(dgList, (bool)rdoEquipment.IsChecked ? "RESOURCE" : "ITEMID");

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("LINE_ID", typeof(string));
                IndataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["PLAN_ID"] = sPlanID;
                dtRow["ROUTE_ID"] = Util.GetCondition(cboProcess);
                dtRow["ELTR_TYPE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["EQUIPMENT_ID"] = cboEquipment.SelectedItemsToString;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                dtRow["LINE_ID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment); 
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_PLAN_SUM_BY_ITEMID", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation, false);

                    string[] sColumnName = new string[] { "PJT", "ELTR_TYPE_NAME", "ITEM_ID", "RESOURCE_NAME" };
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
        #endregion

        #region # Grid
        private void setGridHeader(C1DataGrid dg, string ValueToSort)
        {
            initGrid();

            for (int i = dg.Columns.Count; i-- > 0;) 
                dgList.Columns.RemoveAt(i);

            SetGridHead(dgList, ValueToSort);
            SetGridDate(dgList);
        }
        private void SetGridHead(C1DataGrid dg, string ValueToSort)
        {
            try
            {
                if (ValueToSort.Equals("RESOURCE"))
                {
                    dgList.FrozenColumnCount = 5;
                    Util.SetGridColumnText(dg, "RESOURCE_NAME", null, ObjectDic.Instance.GetObjectName("설비"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "PJT", null, ObjectDic.Instance.GetObjectName("PJT"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "ELTR_TYPE_NAME", null, ObjectDic.Instance.GetObjectName("극성"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "ITEM_ID", null, ObjectDic.Instance.GetObjectName("제품ID"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnNumeric(dg, "TACT_TIME", null, ObjectDic.Instance.GetObjectName("TACT_TIME"), true, false, false, true, 70, HorizontalAlignment.Right, Visibility.Visible, "#,##0.##");
                }
                else
                {
                    dgList.FrozenColumnCount = 4;
                    Util.SetGridColumnText(dg, "PJT", null, ObjectDic.Instance.GetObjectName("PJT"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "ELTR_TYPE_NAME", null, ObjectDic.Instance.GetObjectName("극성"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "ITEM_ID", null, ObjectDic.Instance.GetObjectName("제품ID"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dg, "RESOURCE_NAME", null, ObjectDic.Instance.GetObjectName("설비"), true, false, false, true, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetGridDate(C1DataGrid dg)
        {
            try
            {
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

                    //Util.SetGridColumnNumeric(dg, dayColumnName, null, sHeader, true, false, false, true, 70, HorizontalAlignment.Right, Visibility.Visible, "#,##0");
                    Util.SetGridColumnText(dg, dayColumnName, null, sHeader, true, false, false, true, 70, HorizontalAlignment.Right, Visibility.Visible);
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
