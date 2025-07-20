/*************************************************************************************
 Created Date : 2019.10.17
      Creator : 
   Decription : 설비별 계획조정
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.17  : Initial Created. 
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
    /// COM001_276.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_276 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sPlanID = string.Empty;
        private string sCutOffYMD = string.Empty;
        private string sCutOffHour = string.Empty;
        private int rIdx = 0;
        DataTable dtResource = new DataTable();

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_276()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
            getPlanID();
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    Initialize();
            //    getPlanID();
            //}));
        }

        private void Initialize()
        {
            InitCombo();
            //ApplyPermissions();
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
            dtResource = setEquipment();
            SearchPlanListdDataEquipment();
        }
        #endregion
        
        #region # 계획조정
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        amendPlanning();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }
        #endregion

        #region # 계획추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.GetRowCount() > 0)
            {
                rIdx = dgList.SelectedIndex;
                if (rIdx < 0) { Util.MessageValidation("SFU8109"); return; }

                COM001_276_ADD popWOMerge = new COM001_276_ADD();
                popWOMerge.FrameOperation = FrameOperation;

                object[] Parameters = new object[9];

                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "PJT")); 
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "ITEM_ID")); 
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "WO")); 
                Parameters[3] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "TO_PLAN_QTY")); 
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "TO_RESOURCE")); 
                Parameters[5] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "ORDER_SEQ"));
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rIdx].DataItem, "FACTORY_ID"));
                Parameters[7] = Util.GetCondition(cboProcess);
                C1WindowExtension.SetParameters(popWOMerge, Parameters);

                popWOMerge.Closed += new EventHandler(popWOMerge_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => popWOMerge.ShowModal()));
            }
        }
        private void popWOMerge_Closed(object sender, EventArgs e)
        {
            COM001_276_ADD popup = sender as COM001_276_ADD;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = ((DataView)dgList.ItemsSource).Table;

                if (dt.Select("WO = '" + popup._GetWO + "' AND TXN_FLAG = 'A'").Length > 0) //중복조건 체크
                {
                    Util.MessageValidation("SFU8124");
                    return;
                }

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["PJT"] = popup._GetPjt;
                dr["ITEM_ID"] = popup._GetITEMID;
                dr["WO"] = popup._GetWO;
                dr["ROUTE_ID"] = popup._GetOPERATION;
                dr["TO_RESOURCE"] = popup._GetRESOURCE;
                dr["TO_PLAN_QTY"] = popup._GetPLANQTY;
                dr["ORDER_SEQ"] = popup._GetORDER;
                dr["PRE_RESOURCE"] = popup._GetRESOURCE;
                dr["PRE_ORDER_SEQ"] = popup._GetPREORDER;
                dr["PRE_PLAN_QTY"] = popup._GetPLANQTY;
                dr["TXN_FLAG"] = "A";
                dt.Rows.InsertAt(dr, rIdx + 1);

                dgList.ScrollIntoView(rIdx + 1, 0);
            }
            rIdx = 0;
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
                        if (e.Cell.Column.Name.Equals("ITEM_ID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        if (Util.NVC(e.Cell.Column.Name).Equals("TO_RESOURCE") || Util.NVC(e.Cell.Column.Name).Equals("ORDER_SEQ") || Util.NVC(e.Cell.Column.Name).Equals("TO_PLAN_QTY"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        }

                        string sRunningWip = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RUNNING_WIP_YN"));
                        if (sRunningWip.Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
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
                        if (e.Cell.Column.Name.Equals("ITEM_ID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        if (Util.NVC(e.Cell.Column.Name).Equals("TO_RESOURCE") || Util.NVC(e.Cell.Column.Name).Equals("ORDER_SEQ") || Util.NVC(e.Cell.Column.Name).Equals("TO_PLAN_QTY"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        }

                        string sRunningWip = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RUNNING_WIP_YN"));
                        if (sRunningWip.Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            if (cbo != null)
            {
                if (e.Column.Name == "TO_RESOURCE")
                {
                    string sITEMID = (string)DataTableConverter.GetValue(e.Row.DataItem, "ITEM_ID");
                    cbo.ItemsSource = DataTableConverter.Convert(setCboAssignEquipment(sITEMID));
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void dgList_KeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (!char.IsNumber((char)e.Key.ToString()[(e.Key.ToString().Length - 1)])) { return; }

                if (string.Equals(dataGrid.CurrentCell.Column.Name, "ORDER_SEQ") || string.Equals(dataGrid.CurrentCell.Column.Name, "TO_PLAN_QTY"))
                {
                    (dgList.GetCell(dgList.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;
                }
            }
        }

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name.Equals("TO_RESOURCE"))
                {
                    string sResource = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRE_RESOURCE"));
                    string tResource = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "TO_RESOURCE"));

                    if (sResource.Equals(tResource))
                        (dgList.GetCell(dgList.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = false;
                    else
                        (dgList.GetCell(dgList.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;
                }
            }
            catch (Exception ex){ }
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

                    getPlan(_ITEMID);
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "CHK")).Equals("0") ||
                Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "CHK")).Equals("False"))
            {
                string sResource = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRE_RESOURCE"));
                decimal sOrder = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRE_ORDER_SEQ"));
                decimal sPlan = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRE_PLAN_QTY"));
                DataTableConverter.SetValue(dgList.Rows[rowIndex].DataItem, "TO_RESOURCE", sResource);
                DataTableConverter.SetValue(dgList.Rows[rowIndex].DataItem, "ORDER_SEQ", sOrder);
                DataTableConverter.SetValue(dgList.Rows[rowIndex].DataItem, "TO_PLAN_QTY", sPlan);
                dgList.Refresh(false);
                dgList.EndEdit(true);
            }
        }
        #endregion

        #region # Combo Event
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
            Util.gridClear(dgPlanList);
        }
        #endregion
        #region # Grid Head
        private void setGridHead()
        {
            SetGridDate(dgPlanList, 6);
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

        private DataTable setCboAssignEquipment(string sITEMID)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ITEM_ID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                drnewrow["ELTR_TYPE_CODE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                drnewrow["ITEM_ID"] = sITEMID;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_ASSIGN_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (result != null && result.Rows.Count > 0)
                    return result;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable setEquipment()
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
                drnewrow["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (result != null && result.Rows.Count > 0)
                    return result;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
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

                    DateTime fromPlanTime = Util.StringToDateTime(txtMSS.Text);
                    DateTime toPlanTime = fromPlanTime.AddHours(48);
                    txtPlanPeriod.Text = fromPlanTime.ToString("yyyy-MM-dd HH:mm") + "~" + toPlanTime.ToString("yyyy-MM-dd HH:mm");

                    setGridHead();
                }
            }
            catch (Exception ex) { loadingIndicator.Visibility = Visibility.Collapsed; }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region # 설비별 계획조회
        private void SearchPlanListdDataEquipment()
        {
            try
            {
                getPlanID();

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("PLAN_ID", typeof(string));
                IndataTable.Columns.Add("ROUTE_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
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
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PSI_PLAN_AMEND_LIST", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation, false);

                    //string[] sColumnName = new string[] { "ENG_RESOURCE", "PJT", "ELTR_TYPE_NAME", "ITEM_ID" };
                    //_Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                    (dgList.Columns["TO_RESOURCE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResource);
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

        #region # 계획조정
        private bool CanSave()
        {
            try
            {
                bool bRet = false;

                if (_Util.GetDataGridCheckCnt(dgList, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1651");
                    return bRet;
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void amendPlanning()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("TO_EQUIPMENT_ID", typeof(string));
                inDataTable.Columns.Add("TO_WORK_ORDER_SEQ", typeof(decimal));
                inDataTable.Columns.Add("TO_PLAN_QTY", typeof(decimal));
                inDataTable.Columns.Add("PLAN_ID", typeof(string));
                inDataTable.Columns.Add("OPERATION_ID", typeof(string));
                inDataTable.Columns.Add("CUTOFF_YYYYMMDD", typeof(string));
                inDataTable.Columns.Add("CUTOFF_HOUR", typeof(string));
                inDataTable.Columns.Add("FROM_EQUIPMENT_ID", typeof(string));
                inDataTable.Columns.Add("WORK_ORDER_ID_ENG", typeof(string));
                inDataTable.Columns.Add("FROM_WORK_ORDER_SEQ", typeof(decimal));
                inDataTable.Columns.Add("TXN_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["TO_EQUIPMENT_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "TO_RESOURCE"));
                    newRow["TO_WORK_ORDER_SEQ"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ORDER_SEQ"));
                    newRow["TO_PLAN_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "TO_PLAN_QTY"));
                    newRow["PLAN_ID"] = sPlanID;
                    newRow["OPERATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ROUTE_ID"));
                    newRow["CUTOFF_YYYYMMDD"] = sCutOffYMD;
                    newRow["CUTOFF_HOUR"] = sCutOffHour;
                    newRow["FROM_EQUIPMENT_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ENG_RESOURCE"));
                    newRow["WORK_ORDER_ID_ENG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WO"));
                    newRow["FROM_WORK_ORDER_SEQ"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PRE_ORDER_SEQ"));
                    newRow["TXN_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "TXN_FLAG"));
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_RTS_REG_AMEND", "IN_DATA", null, inDataTable);
                
                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 상세계획 조회
        private void getPlan(string sITEM_ID)
        {
            try
            {
                Util.gridClear(dgPlanList);

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
                    Util.GridSetData(dgPlanList, dtResult, FrameOperation, false);
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
