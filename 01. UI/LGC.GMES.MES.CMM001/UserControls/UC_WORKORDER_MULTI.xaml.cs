/*************************************************************************************
 Created Date : 2017.01.23
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - 공정진척화면의 작업지시 공통 화면(2개까지 선택 가능)           
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.23  INS 정문교C : Initial Created.
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_WORKORDER_MULTI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_WORKORDER_MULTI : UserControl, IWorkArea
    {
        #region Declaration & Constructor       
         
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _FP_REF_PROCID = string.Empty;
        private string _Process_ErpUseYN = string.Empty;        // Workorder 사용 공정 여부.
        private string _Process_Plan_Level_Code = string.Empty; // 계획 Level 코드. (EQPT, PROC .. )
        private string _Process_Plan_Mngt_Type_Code = string.Empty; // 계획 관리 유형 (WO, MO, REF..)
        private string _OtherEqsgID_OLD = string.Empty; // 타라인 이전 선택 값.

        public UserControl _UCParent; //Caller
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        public string EQPTSEGMENT
        {
            get { return _EqptSegment; }
            set { _EqptSegment = value; }
        }

        public string EQPTID
        {
            get { return _EqptID; }
            set { _EqptID = value; }
        }

        public string PROCID
        {
            get { return _ProcID; }
            set { _ProcID = value; }
        }

        ////public string PRODID { get; set; }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM wndBOM;
        #endregion

        #region Initialize     

        public UC_WORKORDER_MULTI()
        {
            InitializeComponent();

            this.Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Input, (System.Threading.ThreadStart)(() =>
                {
                    SetChangeDatePlan();
                }
            ));
        }

        private void InitializeCombo()
        {
            SetDataGridZoneCombo(dgWorkOrder.Columns["MOUNT_PSTN_GR_CODE"], CommonCombo.ComboStatus.ALL);

            
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = PROCID;
            dr["EQPTID"] = EQPTID;
            RQSTDT.Rows.Add(dr);
            
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);
            
            DataTable dtTemp = null;

            if (dtResult.Rows.Count > 0)
            {
                dtTemp = dtResult;
            }
            else
            {
                dtTemp = new DataTable();

                dtTemp.Columns.Add("CBO_NAME", typeof(string));
                dtTemp.Columns.Add("CBO_CODE", typeof(string));
            }


            cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
            cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
            cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

            DataRow dr2 = dtTemp.NewRow();
            dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
            dr2["CBO_CODE"] = "";
            dtTemp.Rows.InsertAt(dr2, 0);

            cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();
            cboEquipmentSegment.SelectedIndex = 0;
            if (!Util.NVC(_OtherEqsgID_OLD).Equals("") && dtTemp?.Select("CBO_CODE = '" + _OtherEqsgID_OLD + "'").Length > 0)
                cboEquipmentSegment.SelectedValue = _OtherEqsgID_OLD;
            else
                cboEquipmentSegment.SelectedIndex = 0;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            
        }

        private void ReSetCombo()
        {
            string sPrvEqsg = "";

            if (cboEquipmentSegment != null) // && cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
            {
                if (!Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    sPrvEqsg = Util.NVC(cboEquipmentSegment.SelectedValue);
                }
            }

            //CommonCombo _combo = new CommonCombo();

            //String[] sFilter = { LoginInfo.CFG_AREA_ID, EQPTSEGMENT };

            //cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_EXCLUDE_LINE");

            //if (!sPrvEqsg.Equals(""))
            //{
            //    cboEquipmentSegment.SelectedValue = sPrvEqsg;
            //}

            //cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;


            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("LANGID", typeof(string));
            //RQSTDT.Columns.Add("AREAID", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["LANGID"] = LoginInfo.LANGID;
            //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //RQSTDT.Rows.Add(dr);

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //DataTable dtTemp = dtResult.Select("CBO_CODE <> '" + EQPTSEGMENT + "'").CopyToDataTable();

            //cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
            //cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
            //cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

            //DataRow dr2 = dtTemp.NewRow();
            //dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
            //dr2["CBO_CODE"] = "";
            //dtTemp.Rows.InsertAt(dr2, 0);

            //cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();

            //if (!sPrvEqsg.Equals("") && !EQPTSEGMENT.Equals(sPrvEqsg))
            //{
            //    cboEquipmentSegment.SelectedValue = sPrvEqsg;
            //}
            //else
            //{
            //    cboEquipmentSegment.SelectedIndex = 0;
            //}

            //cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
        }

        ////private void InitializeWorkorderQuantityInfo()
        ////{
        ////    txtBlockPlanQty.Text = "0";
        ////    txtBlockOutQty.Text = "0";
        ////    txtBlockRemainQty.Text = "0";
        ////}

        #endregion

        #region Event

        #region [Form Load]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (wndBOM != null)
                wndBOM.BringToFront();

            ApplyPermissions();

            InitializeCombo();

            //btnSelectCancel.IsEnabled = false;
            dtpDateFrom.Tag = "CHANGE";
            dtpDateTo.Tag = "CHANGE";

            if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                SetGridData(dgWorkOrder);
            }

            //if (chkLine != null && chkLine.IsChecked.HasValue)
            //    chkLine.IsChecked = false;

            //if (cboEquipmentSegment != null)
            //    cboEquipmentSegment.IsEnabled = false;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            ////InitializeWorkorderQuantityInfo();
            //GetWorkOrder();
        }
        #endregion

        #region [W/O 그리드]
        private void dgWorkOrder_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EIO_WO_DETL_ID")).Equals(""))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFD0DA"));
                    }
                }

                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    SetGridData(dataGrid);
                }
            }));
        }

        public void SetGridData(C1DataGrid dg)
        {
            dg.Columns["CHK"].DisplayIndex = 0;
            dg.Columns["EIO_WO_SEL_STAT"].DisplayIndex = 1;
            dg.Columns["MOUNT_PSTN_GR_CODE"].DisplayIndex = 2;
            dg.Columns["PRJT_NAME"].DisplayIndex = 3;
            dg.Columns["WOID"].DisplayIndex = 4;
            dg.Columns["PRODID"].DisplayIndex = 5;
            dg.Columns["MKT_TYPE_NAME"].DisplayIndex = 6;
            dg.Columns["CELL_3DTYPE"].DisplayIndex = 7;
            dg.Columns["CLSS_NAME"].DisplayIndex = 8;
            dg.Columns["MODLID"].DisplayIndex = 9;
            dg.Columns["LOTYNAME"].DisplayIndex = 10;

            dg.Columns["INPUT_QTY"].DisplayIndex = 11;
            dg.Columns["OUTQTY"].DisplayIndex = 12;
            dg.Columns["STRT_DTTM"].DisplayIndex = 13;
            dg.Columns["END_DTTM"].DisplayIndex = 14;
            dg.Columns["WO_STAT_NAME"].DisplayIndex = 15;

            dg.Columns["EQSGID"].DisplayIndex = 16;
            dg.Columns["EQSGNAME"].DisplayIndex = 17;
            dg.Columns["PROD_VER_CODE"].DisplayIndex = 18;
            dg.Columns["WO_DETL_ID"].DisplayIndex = 19;
            dg.Columns["EQPTID"].DisplayIndex = 20;

            dg.Columns["EQPTNAME"].DisplayIndex = 21;
            dg.Columns["WO_STAT_CODE"].DisplayIndex = 22;
            dg.Columns["PLAN_TYPE_NAME"].DisplayIndex = 23;
            dg.Columns["PLAN_TYPE"].DisplayIndex = 24;
            dg.Columns["WOTYPE"].DisplayIndex = 25;
            dg.Columns["EIO_WO_DETL_ID"].DisplayIndex = 26;
            dg.Columns["EIO_WO_DETL_ID2"].DisplayIndex = 27;
            dg.Columns["PRDT_CLSS_CODE"].DisplayIndex = 28;
            dg.Columns["DEMAND_TYPE"].DisplayIndex = 29;

            dg.FrozenColumnCount = 5;

        }

        private void dgWorkOrder_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgWorkOrder_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            //DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            //DataRow[] dr1 = dt.Select("(EIO_WO_SEL_STAT = 'Y') OR (EIO_WO_SEL_STAT <> 'Y' And CHK = '1')");
            //DataRow[] dr2 = dt.Select("EIO_WO_SEL_STAT = 'Y'");                      // 선택된 W/O
            //DataRow[] dr3 = dt.Select("EIO_WO_SEL_STAT <> 'Y' And CHK = '1'");
            //string sProdID = string.Empty;

            //// 선택된 W/O 를 체크시 return
            //if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.SelectedItem, "EIO_WO_SEL_STAT")).Equals("Y"))
            //    return;

            //// 체크해제시
            //if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(0))
            //{
            //    dgWorkOrder.SelectedIndex = -1;
            //    return;
            //}

            //// W/O는 2건까지만(선택된거와 추가선택)
            //if (dr1.Length > 2)
            //{
            //    //Util.Alert("W/O 선택은 2개까지만 선택할 수 있습니다.");
            //    Util.MessageValidation("W/O 선택은 2개까지만 선택할 수 있습니다.");
            //    // 체크 해제
            //    DataTableConverter.SetValue(dgWorkOrder.Rows[e.Row.Index].DataItem, "CHK", false);
            //    dgWorkOrder.SelectedIndex = -1;
            //    return;
            //}

            //// 선택된 W/O의 ZONE 구분이 ALL인 경우는 추가 W/O 선택 불가
            //if (dr2.Length > 0)
            //{
            //    if (dr2[0]["MOUNT_PSTN_GR_CODE"].ToString().Equals("ALL"))
            //    {
            //        //Util.Alert("이미 ALL로 선택된 W/O가 존재 합니다.");
            //        Util.MessageValidation("이미 ALL로 선택된 W/O가 존재 합니다.");
            //        DataTableConverter.SetValue(dgWorkOrder.Rows[e.Row.Index].DataItem, "CHK", false);
            //        dgWorkOrder.SelectedIndex = -1;
            //        return;
            //    }
            //}

            //// 추가선택은 1건 
            //if (dr3.Length > 1)
            //{
            //    //Util.Alert("W/O 선택은 1개만 선택할 수 있습니다.");
            //    Util.MessageValidation("W/O 선택은 1개만 선택할 수 있습니다.");
            //    // 체크 해제
            //    DataTableConverter.SetValue(dgWorkOrder.Rows[e.Row.Index].DataItem, "CHK", false);
            //    dgWorkOrder.SelectedIndex = -1;
            //    return;
            //}

            ////// 작업중인 LOT 여부 체크
            ////if (!SelectWorkInProcessStatus())
            ////{
            ////    DataTableConverter.SetValue(dgWorkOrder.Rows[e.Row.Index].DataItem, "CHK", false);
            ////    dgWorkOrder.SelectedIndex = -1;
            ////    return;
            ////}

            //// W/O의 품목은 서로 틀린 품목
            //for (int nrow = 0; nrow < dr1.Length; nrow++)
            //{
            //    if (nrow == 0)
            //    {
            //        sProdID = dr1[nrow]["PRODID"].ToString();
            //    }
            //    else
            //    {
            //        if (sProdID == dr1[nrow]["PRODID"].ToString())
            //        {
            //            //Util.Alert("W/O의 품목이 동일한 품목 입니다.");
            //            Util.MessageValidation("W/O의 품목이 동일한 품목 입니다.");
            //            // 체크 해제
            //            DataTableConverter.SetValue(dgWorkOrder.Rows[e.Row.Index].DataItem, "CHK", false);
            //            dgWorkOrder.SelectedIndex = -1;
            //            return;
            //        }
            //    }
            //}

        }

        private void dgWorkOrder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (wndBOM != null)
                        return;

                    //if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                    //{
                        C1DataGrid dg = sender as C1DataGrid;

                        if (dg == null) return;

                        C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                        if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                        switch (Convert.ToString(currCell.Column.Name))
                        {
                            case "WOID":

                                wndBOM = new LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM();
                                wndBOM.FrameOperation = FrameOperation;

                                if (wndBOM != null)
                                {
                                    object[] Parameters = new object[7];
                                    Parameters[0] = currCell.Text;

                                    C1WindowExtension.SetParameters(wndBOM, Parameters);

                                    wndBOM.Closed += new EventHandler(wndBOM_Closed);
                                    this.Dispatcher.BeginInvoke(new Action(() => wndBOM.ShowModal()));                                    
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    //}
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void dgWorkOrder_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("MOUNT_PSTN_GR_CODE") && 
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EIO_WO_SEL_STAT")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndBOM_Closed(object sender, EventArgs e)
        {
            wndBOM = null;
            LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM window = sender as LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        #endregion

        #region [W/O 선택]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
                return;

            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr1 = dt.Select("EIO_WO_SEL_STAT = 'Y' And CHK = '1'");       // 선택된 W/O에 체크된 넘
            DataRow[] dr2 = dt.Select("EIO_WO_SEL_STAT = 'Y'");                     // 선택된 W/O
            DataRow[] dr3 = dt.Select("EIO_WO_SEL_STAT <> 'Y' And CHK = '1'");      // 선택 W/O
            DataRow[] dr4 = dt.Select("CHK = '1'");                                 // 선택한 모든 W/O  
            DataRow[] dr5 = dt.Select("(EIO_WO_SEL_STAT = 'Y') OR (EIO_WO_SEL_STAT <> 'Y' And CHK = '1')");

            if (dr5.Length > 2)
            {
                // W/O 선택은 2개까지만 선택할 수 있습니다.
                Util.MessageValidation("SFU3502");
                return;
            }

            if (dr3.Length < 1 && dr4.Length > 0)
            {
                Util.MessageValidation("SFU3061");  //이미 선택된 작업지시 입니다.
                return;
            }
            else if (dr3.Length < 1)
                return;

            // 선택이 안되고 CheckBox 로 선택한(실제 신규 선택 Row)만 Validation.
            foreach (DataRow drTmp in dr3)
            {
                if (!CanChangeWorkOrder(drTmp))
                    return;
            }

            // Zone Validation
            bool bMgzAll = false;
            string sBeforeZone = string.Empty;
            string sBeforeProdID = string.Empty;

            foreach (DataRow drTmp in dr5)
            {
                C1.WPF.DataGrid.DataGridCell dgcTmp = dgWorkOrder[dt.Rows.IndexOf(drTmp), dgWorkOrder.Columns["MOUNT_PSTN_GR_CODE"].Index];
                C1.WPF.DataGrid.DataGridCell dgcPrdTmp = dgWorkOrder[dt.Rows.IndexOf(drTmp), dgWorkOrder.Columns["PRODID"].Index];

                if (dgcTmp != null)
                {
                    if (Util.NVC(dgcTmp.Value).IndexOf("ALL") >= 0)
                    {
                        if (bMgzAll || !Util.NVC(sBeforeZone).Equals(""))
                        {
                            // ALL로 선택된 W/O가 존재 합니다.
                            Util.MessageValidation("SFU3503");
                            return;
                        }

                        bMgzAll = true;
                    }
                    else
                    {
                        if (bMgzAll)
                        {
                            // ALL로 선택된 W/O가 존재 합니다.
                            Util.MessageValidation("SFU3503");
                            return;
                        }

                        // Zone
                        if (Util.NVC(sBeforeZone).Equals(""))
                        {
                            sBeforeZone = Util.NVC(dgcTmp.Value);
                        }
                        else
                        {
                            if (sBeforeZone.Equals(Util.NVC(dgcTmp.Value)))
                            {
                                //동일한 ZONE을 선택 하였습니다.
                                Util.MessageValidation("SFU3504");
                                return;
                            }
                        }
                    }
                }

                if (dgcPrdTmp != null)
                {
                    // 제품코드
                    if (Util.NVC(sBeforeProdID).Equals(""))
                    {
                        sBeforeProdID = Util.NVC(dgcPrdTmp.Value);
                    }
                    else
                    {
                        if (sBeforeProdID.Equals(Util.NVC(dgcPrdTmp.Value)))
                        {
                            // 선택한 W/O의 제품이 동일합니다.
                            Util.MessageValidation("SFU3505");
                            return;
                        }
                    }
                }
            }
            
            // 작업지시를 변경하시겠습니까?
            Util.MessageConfirm("SFU2943", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sWoDetlID1 = string.Empty;
                    string sWoDetlID2 = string.Empty;

                    foreach (DataRow drTmp in dr5)
                    {
                        //if (!CanChangeWorkOrder(drTmp))
                        //    return;
                        
                        C1.WPF.DataGrid.DataGridCell dgcTmp = dgWorkOrder[dt.Rows.IndexOf(drTmp), dgWorkOrder.Columns["MOUNT_PSTN_GR_CODE"].Index];
                        
                        if (dgcTmp != null)
                        {
                            if (Util.NVC(dgcTmp.Value).IndexOf("ALL") >= 0)
                            {
                                sWoDetlID1 = drTmp["WO_DETL_ID"].ToString();
                                sWoDetlID2 = drTmp["WO_DETL_ID"].ToString();
                            }
                            else if (Util.NVC(dgcTmp.Value).Equals("1"))
                            {
                                sWoDetlID1 = drTmp["WO_DETL_ID"].ToString();
                                //sWoDetlID2 = string.Empty;
                            }
                            else
                            {
                                //sWoDetlID1 = string.Empty;
                                sWoDetlID2 = drTmp["WO_DETL_ID"].ToString();
                            }
                        }                        
                    }

                    WorkOrderChange(sWoDetlID1, sWoDetlID2);
                }
            });
        }

        private void wozone_Closed(object sender, EventArgs e)
        {
            // HALF PRODUCT RESULT
            string sSelectZone;
            string sWoDetlID1 = string.Empty;
            string sWoDetlID2 = string.Empty;

            LGC.GMES.MES.CMM001.Popup.CMM_WO_ZONE _wozone = sender as LGC.GMES.MES.CMM001.Popup.CMM_WO_ZONE;

            if (_wozone.DialogResult == MessageBoxResult.OK)
            {
                sSelectZone = _wozone.SELWOZONE;
                DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
                DataRow[] dr1 = dt.Select("EIO_WO_SEL_STAT = 'Y'");
                DataRow[] dr2 = dt.Select("EIO_WO_SEL_STAT <> 'Y' And CHK = '1'");

                if (_wozone.SELWOZONE.Equals("ALL"))
                {
                    sWoDetlID1 = dr2[0]["WO_DETL_ID"].ToString();
                    sWoDetlID2 = dr2[0]["WO_DETL_ID"].ToString();
                }
                else if (_wozone.SELWOZONE.Equals("1"))
                {
                    sWoDetlID1 = dr2[0]["WO_DETL_ID"].ToString();
                    sWoDetlID2 = dr1.Length > 0 ? dr1[0]["WO_DETL_ID"].ToString() : string.Empty;
                }
                else
                {
                    sWoDetlID1 = dr1.Length > 0 ? dr1[0]["WO_DETL_ID"].ToString() : string.Empty;
                    sWoDetlID2 = dr2[0]["WO_DETL_ID"].ToString(); 
                }

                // 작업지시를 변경하시겠습니까?
                Util.MessageConfirm("SFU2943", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (SelectWorkInProcessStatus())
                            WorkOrderChange(sWoDetlID1, sWoDetlID2);
                    }
                });
            }
                
        }
        #endregion

        #region [W/O 선택해제]
        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EQPTID) || string.IsNullOrEmpty(PROCID) || string.IsNullOrEmpty(EQPTSEGMENT) || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;
                
                // 2건 이상 선택 불가 
                DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
                DataRow[] dr1 = dt.Select("CHK = '1'");
                DataRow[] dr2 = dt.Select("EIO_WO_SEL_STAT = 'Y' And CHK =  '1'");
                DataRow[] dr3 = dt.Select("EIO_WO_SEL_STAT = 'Y' And CHK <> '1'");
                DataRow[] dr4 = dt.Select("EIO_WO_SEL_STAT = 'Y'");
                DataRow[] dr5 = dt.Select("(EIO_WO_SEL_STAT = 'Y') OR (EIO_WO_SEL_STAT <> 'Y' And CHK = '1')");

                string sWoDetlID1 = string.Empty;
                string sWoDetlID2 = string.Empty;

                if (dr1.Length < 1)
                    return;

                // 선택한 전체 Validation
                foreach (DataRow drTmp in dr1)
                {
                    if (!CanCancelWorkOrder(drTmp))
                        return;
                }

                // 이전 값 조회
                foreach (DataRow drTmp in dr4)
                {
                    C1.WPF.DataGrid.DataGridCell dgcTmp = dgWorkOrder[dt.Rows.IndexOf(drTmp), dgWorkOrder.Columns["MOUNT_PSTN_GR_CODE"].Index];

                    if (dgcTmp != null)
                    {
                        if (Util.NVC(dgcTmp.Value).Equals("1"))
                        {
                            sWoDetlID1 = Util.NVC(drTmp["EIO_WO_DETL_ID"]);
                        }
                        else if (Util.NVC(dgcTmp.Value).Equals("2"))
                        {
                            sWoDetlID2 = Util.NVC(drTmp["EIO_WO_DETL_ID2"]);
                        }
                    }
                }

                string sParam1 = string.Empty;
                string sParam2 = string.Empty;

                // 2개 모두 취소.
                if (dr1.Length == 2)
                {
                    sParam1 = string.Empty;
                    sParam2 = string.Empty;
                }
                else
                {
                    if (Util.NVC(dr1[0]["MOUNT_PSTN_GR_CODE"]).Equals("1"))
                    {
                        sParam1 = string.Empty;
                        sParam2 = sWoDetlID2;
                    }
                    else if (Util.NVC(dr1[0]["MOUNT_PSTN_GR_CODE"]).Equals("2"))
                    {
                        sParam1 = sWoDetlID1;
                        sParam2 = string.Empty;
                    }
                }


                // 작업지시를 선택취소 하시겠습니까?
                Util.MessageConfirm("SFU2944", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        WorkOrderChange(sParam1, sParam2, false);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [W/O 조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetParentSearchConditions();

                if (EQPTSEGMENT.Equals("") || EQPTSEGMENT.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                if (EQPTID.Equals("") || EQPTID.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [계획일자]
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                // FP안정화 때까지 이전날짜 VALIDATION 무효화
                /*DateTime dtCaldate;
                string sCalDateYMD = "";
                string sCalDateYYYY = "";
                string sCalDateMM = "";
                string sCalDateDD = "";

                CheckCalDateByMonth(dtPik, out dtCaldate, out sCalDateYMD, out sCalDateYYYY, out sCalDateMM, out sCalDateDD);

                if (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;
                    //Util.Alert("SFU1738");      //오늘 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1738");
                    //e.Handled = false;
                    return;
                }*/

                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                // W/O 공정인 경우에만 체크.
                if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                {
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    //if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }
                else
                {
                    //if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }

                //if (Convert.ToDecimal(currDate.ToString("yyyyMM")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                //{
                //    dtPik.Text = baseDate.ToLongDateString();
                //    dtPik.SelectedDateTime = baseDate;
                //    Util.MessageValidation("SFU3448");  //이달 이후 날짜는 선택할 수 없습니다.
                //    return;
                //}

                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
                    //e.Handled = false;
                    return;
                }

                btnSearch_Click(null, null);
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                /*
                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = System.DateTime.Now.ToLongDateString();
                    dtPik.SelectedDateTime = System.DateTime.Now;
                    //Util.Alert("SFU1698");      //시작일자 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1698");
                    //e.Handled = false;
                    return;
                }
                */

                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    baseDate = dtpDateFrom.SelectedDateTime;

                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }

                //if (Convert.ToDecimal(currDate.ToString("yyyyMM")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                //{
                //    if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(baseDate.ToString("yyyyMMdd")))
                //        baseDate = dtpDateFrom.SelectedDateTime;

                //    dtPik.Text = baseDate.ToLongDateString();
                //    dtPik.SelectedDateTime = baseDate;
                //    Util.MessageValidation("SFU3448");  //이달 이후 날짜는 선택할 수 없습니다.
                //    return;
                //}

                btnSearch_Click(null, null);
            }));
        }
        #endregion

        #region [공정체크]
        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }
        #endregion

        #endregion

        #region Method

        #region [BizCall]
        /// <summary>
        /// 공정정보 biz 호출 처리
        /// </summary>
        private void GetProcessFPInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["PROCID"] = PROCID;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    _FP_REF_PROCID = "";
                    _Process_ErpUseYN = "";
                    _Process_Plan_Level_Code = "";
                    return;
                }

                // WorkOrder 사용여부, 계획LEVEL 코드.
                _Process_ErpUseYN = Util.NVC(dtRslt.Rows[0]["ERPRPTIUSE"]);
                _Process_Plan_Level_Code = Util.NVC(dtRslt.Rows[0]["PLAN_LEVEL_CODE"]);
                _Process_Plan_Mngt_Type_Code = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);

                if (_Process_Plan_Level_Code.Equals("PROC"))//if (!_Process_ErpUseYN.Equals("Y") && _Process_Plan_Level_Code.Equals("PROC")) // PROCESS 인 경우 공정 자동 체크 및 disable.
                {
                    _FP_REF_PROCID = "";

                    //chkProc.IsChecked = true;
                    //chkProc.IsEnabled = false;
                }
                else
                {
                    _FP_REF_PROCID = "";

                    //if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    //{
                    //    chkProc.IsChecked = true;
                    //    chkProc.IsEnabled = true;
                    //}
                    //else
                    //{
                    //    chkProc.IsChecked = false;
                    //    chkProc.IsEnabled = true;
                    //}
                }

                // Reference 공정인 경우는 REF 공정 정보 설정.
                if (!_Process_ErpUseYN.Equals("Y") && Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                {
                    _FP_REF_PROCID = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);

                    //chkProc.IsChecked = true;
                    //chkProc.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// W/O정보 biz 호출 처리
        /// </summary>
        private void GetWOInfo(string sWOID, out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WO();

                DataRow newRow = inTable.NewRow();
                newRow["WOID"] = sWOID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKORDER", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("20") ||
                        Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("40"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        //sMsg = "선택 가능한 상태의 작업지시가 아닙니다.";
                        sMsg = "SFU3058";
                    }
                }
                else
                {
                    sRet = "NG";
                    //sMsg = "존재하지 않습니다.";
                    sMsg = "SFU2881";
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="dr">선택 한 작업지시 정보 DataRow</param>
        /// <param name="isSelectFlag">선택 처리:true 선택 취소:fals</param>
        private void SetWorkOrderSelect(string sWoDetlid, string sWoDetlid2, bool isSelectFlag = true)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID_SRC();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = sWoDetlid;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WO_DETL_ID2"] = sWoDetlid2;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID_SRC", "INDATA", "", inTable);

                if (isSelectFlag)
                    Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.
                else
                    Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 설비별 작업지시 리스트 조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetEquipmentWorkOrder()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                //searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                //searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode(); //"LINE"; // chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked ? "PROC" : "EQPT";

                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0 &&
                    !Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    searchCondition["OTHER_EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    searchCondition["EQSGID"] = "";
                    searchCondition["PROC_EQPT_FLAG"] = "LINE";
                }
                else
                {
                    searchCondition["OTHER_EQSGID"] = "";
                    searchCondition["EQSGID"] = EQPTSEGMENT;
                    searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();
                }

                inTable.Rows.Add(searchCondition); 

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE_SRC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 공정별 작업지시 리스트 조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetEquipmentWorkOrderByProc()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// W/O 선택, 선택 취소시 설비 작업중 체크
        /// </summary>
        private bool SelectWorkInProcessStatus()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = EQPTID;
                inData["PROCID"] = PROCID;
                inData["EQSGID"] = EQPTSEGMENT;
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_STATUS", "INDATA", "OUTDATA", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["WIPSTAT"].ToString().Equals("PROC"))
                    {
                        //ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("진행중인 LOT 이 존재 합니다."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        Util.MessageValidation("SFU1290", EQPTID);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool ChkFPDtlInfoByMonth(string sWODtl, string sCalDateYMD, out string sOutMsg)
        {
            sOutMsg = "";
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = sWODtl;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FP_DETL_PLAN", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("STRT_DTTM") && dtResult.Columns.Contains("END_DTTM"))
                {
                    DateTime dtStrtDate;
                    DateTime dtEndDate;
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["STRT_DTTM"]), out dtStrtDate);
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["END_DTTM"]), out dtEndDate);

                    if (dtEndDate != null)
                    {
                        // W/O 공정인 경우에만 체크.
                        if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                        {
                            if (Util.NVC_Int(dtEndDate.ToString("yyyyMMdd")) >= Util.NVC_Int(sCalDateYMD))
                            {
                                bRet = true;
                            }
                            else
                                sOutMsg = "SFU3517";    // 계획일자가 이미 지난 WO는 선택할 수 없습니다.
                        }
                        else
                        {
                            bRet = true;
                        }
                    }
                }

                // Plan date 기준..
                //if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("PLAN_DATE"))
                //{
                //    string sPlanDate = Util.NVC(dtResult.Rows[0]["PLAN_DATE"]);
                //    if (sPlanDate.Length >= 6 && sCalDateYMD.Length >= 6)
                //    {
                //        //if (sPlanDate.Substring(0, 6).Equals(sCalDateYMD.Substring(0, 6)))  // 동일 월인 경우.
                //        //{
                //            // W/O 공정인 경우에만 체크.
                //            if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                //            {
                //                if (Util.NVC_Int(sPlanDate) >= Util.NVC_Int(sCalDateYMD))  // Today ~ 해당 월의 W/O만 선택 가능.
                //                {
                //                    bRet = true;
                //                }
                //                else
                //                    sOutMsg = "SFU3517";    // 계획일자가 이미 지난 WO는 선택할 수 없습니다.
                //            }
                //            else
                //            {
                //                bRet = true;
                //            }
                //        //}
                //        //else
                //        //    sOutMsg = "SFU3443";    // 해당월의 WO만 선택 가능합니다.
                //    }
                //}
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckCalDateByMonth(LGCDatePicker dtPik, out DateTime dtCaldate, out string sCalDateYMD, out string sCalDateYYYY, out string sCalDateMM, out string sCalDateDD)
        {
            try
            {
                bool bRet = false;

                dtCaldate = System.DateTime.Now;
                sCalDateYMD = "";
                sCalDateYYYY = "";
                sCalDateMM = "";
                sCalDateDD = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("CALDATE"))
                    {
                        if (Util.NVC(dtResult.Rows[0]["CALDATE"]).Equals(""))
                            return bRet;


                        DateTime.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE"]), out dtCaldate);
                        //dtCaldate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                    }
                    if (dtResult.Columns.Contains("CALDATE_YMD"))
                        sCalDateYMD = Util.NVC(dtResult.Rows[0]["CALDATE_YMD"]);
                    if (dtResult.Columns.Contains("CALDATE_YYYY"))
                        sCalDateYYYY = Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]);
                    if (dtResult.Columns.Contains("CALDATE_MM"))
                        sCalDateMM = Util.NVC(dtResult.Rows[0]["CALDATE_MM"]);
                    if (dtResult.Columns.Contains("CALDATE_DD"))
                        sCalDateDD = Util.NVC(dtResult.Rows[0]["CALDATE_DD"]);

                    if (dtResult.Columns.Contains("CALDATE_YYYY") && dtResult.Columns.Contains("CALDATE_MM"))
                    {
                        int iYM = 0;
                        int.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]) + Util.NVC(dtResult.Rows[0]["CALDATE_MM"]), out iYM);
                        if (dtPik != null && iYM != Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                        {
                            bRet = true;
                        }
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                dtCaldate = System.DateTime.Now;
                sCalDateYMD = "";
                sCalDateYYYY = "";
                sCalDateMM = "";
                sCalDateDD = "";

                Util.MessageException(ex);
                return false;
            }
        }

        private bool SelectProcStateLot(string sWOID = "")
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID
                inDataTable.Columns.Add("WOID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = EQPTID;
                inData["PROCID"] = PROCID;
                inData["EQSGID"] = EQPTSEGMENT;

                if(!sWOID.Equals(""))
                    inData["WOID"] = sWOID;

                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("WIPSTAT"))
                {
                    if (dtRslt.Rows[0]["WIPSTAT"].Equals("PROC"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        private void SetChangeDatePlan(bool isInitFlag = true)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (isInitFlag)
            {
                if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateFrom.Tag = "CHANGE";

                    dtpDateTo.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateTo.Tag = "CHANGE";
                }
            }
            else
            {
                if (Util.NVC_Decimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate;
                    dtpDateFrom.Tag = "CHANGE";
                }

                if (Util.NVC_Decimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateTo.SelectedDateTime = currDate;
                    dtpDateTo.Tag = "CHANGE";
                }
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private void GetChangeDatePlan(out DateTime currDate, out string sCurrTime, out string sBaseTime)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            currDate = GetCurrentTime();
            sCurrTime = currDate.ToString("HHmmss");
            sBaseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);
        }

        private string GetFpPlanGnrtBasCode()
        {
            try
            {
                string sPlanType = "";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = PROCID;
                dr["EQSGID"] = EQPTSEGMENT;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_PLAN_GNRT_BAS_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("FP_PLAN_GNRT_BAS_CODE"))
                {
                    if (Util.NVC(dtResult.Rows[0]["FP_PLAN_GNRT_BAS_CODE"]).Equals("E"))
                        sPlanType = "EQPT";
                    else
                        sPlanType = "LINE";
                }

                return sPlanType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "LINE";
            }
        }
        #endregion

        #region [Validation]

        /// <summary>
        /// 작업지시 선택 가능 Validation 처리
        /// </summary>
        /// <param name="iRow">선택한 작업지시 정보 Row Number</param>
        /// <returns></returns>
        private bool CanChangeWorkOrder(DataRow dtRow)
        {
            bool bRet = false;

            if (dtRow == null)
                return bRet;

            if (EQPTID.Trim().Equals("") || PROCID.Trim().Equals("") || EQPTSEGMENT.Trim().Equals(""))
                return bRet;

            // Zone 선택 여부 체크
            if (Util.NVC(dtRow["MOUNT_PSTN_GR_CODE"]).Equals(""))
            {
                // 매거진 Zone을 선택 하세요.
                Util.MessageValidation("SFU3506");
                return bRet;
            }

            //// 해당 설비에 진행중인 LOT 체크
            //if (SelectProcStateLot())
            //{
            //    Util.MessageValidation("SFU1917");
            //    return bRet;
            //}

            // Workorder 내려오는 공정만 체크 필요.
            if (_Process_ErpUseYN.Equals("Y"))
            {
                // 선택 가능한 작지 여부 확인.
                string sRet = string.Empty;
                string sMsg = string.Empty;

                GetWOInfo(Util.NVC(dtRow["WOID"]), out sRet, out sMsg);
                if (sRet.Equals("NG"))
                {
                    Util.MessageValidation(sMsg);
                    return bRet;
                }
            }


            // 해당 월의 W/O만 선택 가능
            DateTime dtCaldate;
            string sCalDateYMD = "";
            string sCalDateYYYY = "";
            string sCalDateMM = "";
            string sCalDateDD = "";
            string sOutMsg = "";

            CheckCalDateByMonth(null, out dtCaldate, out sCalDateYMD, out sCalDateYYYY, out sCalDateMM, out sCalDateDD);
            if (!ChkFPDtlInfoByMonth(Util.NVC(dtRow["WO_DETL_ID"]), sCalDateYMD, out sOutMsg))
            {
                Util.MessageValidation(sOutMsg);
                return bRet;
            }

            // 자동 Rolling에 따라 순차적 WO 처리를 위한 Validation
            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr = dt?.Select("EIO_WO_SEL_STAT = 'Y'");
            if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("PRODID") && dt.Columns.Contains("MKT_TYPE_CODE"))
            {
                foreach (DataRow drTmp in dr)
                {
                    if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && 
                        Util.NVC(dtRow["PRODID"]).Equals(Util.NVC(drTmp["PRODID"])) &&
                        Util.NVC(dtRow["MKT_TYPE_CODE"]).Equals(Util.NVC(drTmp["MKT_TYPE_CODE"])))
                    {
                        Util.MessageValidation("SFU4117"); // 동일한 모델, WO Type의 WO가 이미 선택되어 있습니다.
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        private bool CanCancelWorkOrder(DataRow dtRow)
        {
            bool bRet = false;

            if (dtRow == null)
                return bRet;

            if (EQPTID.Trim().Equals("") || PROCID.Trim().Equals("") || EQPTSEGMENT.Trim().Equals(""))
                return bRet;

            // 선택 여부 확인
            if (!Util.NVC(dtRow["EIO_WO_SEL_STAT"]).Equals("Y"))
            {
                // 선택된 W/O만 취소 가능 합니다.
                Util.MessageValidation("SFU3507");
                return bRet;
            }

            // 해당 설비에 진행중인 LOT 체크
            if (SelectProcStateLot(Util.NVC(dtRow["WOID"])))
            {
                Util.MessageValidation("SFU1917");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        #endregion

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSelectCancel);

            if (FrameOperation != null)
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 작업지시 선택 처리
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isSelectFlag"> 선택 처리:true 선택 취소:false </param>
        private void WorkOrderChange(string sWoDetlid1, string sWoDetlid2, bool isSelectFlag = true)
        {
            SetWorkOrderSelect(sWoDetlid1, sWoDetlid2, isSelectFlag);

            GetWorkOrder();
        }

        /// <summary>
        /// 호출 Form의 W/O 조회조건 처리
        /// </summary>
        private void GetParentSearchConditions()
        {
            // call form에 정의 필요--------------------------------------------------------------------------
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetSearchConditions");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                    parameterArrys[i] = null;

                object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                if ((bool)result)
                {
                    PROCID = parameterArrys[0].ToString();
                    EQPTSEGMENT = parameterArrys[1].ToString();
                    EQPTID = parameterArrys[2].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업지시 Datagrid Clear
        /// </summary>
        public void ClearWorkOrderInfo()
        {
            Util.gridClear(dgWorkOrder);
            ////InitializeWorkorderQuantityInfo();
            ReSetCombo();
        }

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        public void GetWorkOrder()
        {
            InitializeCombo();

            //SetChangeDatePlan(false);

            if (PROCID.Length < 1 || EQPTID.Length < 1 || EQPTSEGMENT.Length < 1)
                return;

            // 일자 설정이 안된경우 RETURN.
            if (dtpDateFrom.SelectedDateTime.Year < 2000 || dtpDateTo.SelectedDateTime.Year < 2000)
                return;

            // Process 정보 조회  
            GetProcessFPInfo();

            ClearWorkOrderInfo();

            DataTable searchResult = new DataTable();

            //// ERP 실적 공정 기준 WO 조회
            //if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
            //    searchResult = GetEquipmentWorkOrderByProc();
            //else
                searchResult = GetEquipmentWorkOrder();

            if (searchResult == null)
                return;

            Util.GridSetData(dgWorkOrder, searchResult, FrameOperation, true);

            // 3D 제품이 존재하는 경우
            if (dgWorkOrder.Columns.Contains("CELL_3DTYPE"))
            {
                if (searchResult.Columns.Contains("CELL_3DYN"))
                {
                    DataRow[] drTmp = searchResult.Select("CELL_3DYN = 'Y'");
                    if (drTmp.Length > 0)
                    {
                        dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                }
            }

            // 현 작업지시 정보 Top Row 처리 및 고정..
            DataRow[] dr = searchResult.Select("EIO_WO_SEL_STAT = 'Y'");

            dgWorkOrder.FrozenTopRowsCount = dr.Length;

            //if (dr.Length > 0)
            //{
            //    btnSelectCancel.IsEnabled = true;
            //}
            //else
            //{
            //    btnSelectCancel.IsEnabled = false;
            //}

            // 공정 조회인 경우 설비 정보 Visible 처리.
            //if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
            //    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            //else
            //dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

            //if (chkLine.IsChecked.HasValue && (bool)chkLine.IsChecked)
            if (cboEquipmentSegment != null && Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Collapsed;
            else
                dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Visible;

            SetUnCheckAll();
        }

        /// <summary>
        /// 선택 W/O 정보
        /// </summary>
        public DataRow GetSelectWorkOrderRow()
        {
            DataRow row = null;

            try
            {
                DataRow[] dr = Util.gridGetChecked(ref dgWorkOrder, "CHK");

                if (dr == null || dr.Length < 1)
                    row = null;
                else
                    row = dr[0];

                return row;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void SetDataGridZoneCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            //const string bizRuleName = "DA_BAS_SEL_COMMONCODE_BY_ATTR1";
            //string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            //string[] arrCondition = { LoginInfo.LANGID, "MOUNT_PSTN_GR_CODE", Process.SRC };
            string selectedValueText = ((C1.WPF.DataGrid.DataGridComboBoxColumn)dgcol).SelectedValuePath;
            string displayMemberText = ((C1.WPF.DataGrid.DataGridComboBoxColumn)dgcol).DisplayMemberPath;
            //CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MOUNT_PSTN_GR_CODE";
            dr["ATTRIBUTE1"] = "SRC";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1", "RQSTDT", "RSLTDT", RQSTDT);
            
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });

            DataRow drInst = dtBinding.NewRow();

            drInst[selectedValueText] = "ALL";
            drInst[displayMemberText] = "ALL";
            dtBinding.Rows.InsertAt(drInst, 0);

            C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (dataGridComboBoxColumn != null)
                dataGridComboBoxColumn.ItemsSource = dtBinding.Copy().AsDataView();
        }

        private void SetUnCheckAll()
        {
            try
            {
                if (dgWorkOrder == null || dgWorkOrder.ItemsSource == null)
                    return;

                for (int i = 0; i < dgWorkOrder.Rows.Count - dgWorkOrder.TopRows.Count - dgWorkOrder.BottomRows.Count; i++)
                {
                    DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);
                    dgWorkOrder.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void chkLine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cboEquipmentSegment.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkLine_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0)
                    cboEquipmentSegment.SelectedIndex = 0;

                cboEquipmentSegment.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboEquipmentSegment?.SelectedValue != null)
                    _OtherEqsgID_OLD = cboEquipmentSegment.SelectedValue.ToString();
                else
                    _OtherEqsgID_OLD = "";

                btnSearch_Click(null, null);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }

}