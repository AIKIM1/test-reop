/*************************************************************************************
 Created Date : 2016.08.16
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 공정진척화면의 작업지시 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.16  INS 김동일K : Initial Created.
**************************************************************************************/

using C1.WPF;
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// UC_WORKORDER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WORKORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor        
        private string _ShopID = string.Empty;
        private string _AreaID = string.Empty;
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _FP_REF_PROCID = string.Empty;
        private string _Process_ErpUseYN = string.Empty;        // Workorder 사용 공정 여부.
        private string _Process_Plan_Level_Code = string.Empty; // 계획 Level 코드. (EQPT, PROC .. )
        private string _Now_Workorder = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private string _WOID = string.Empty;
        private string _WOIDDETAIL = string.Empty;

        public string WOID
        {
            get { return _WOID; }
        }

        public string WOIDDETAIL
        {
            get { return _WOIDDETAIL; }
        }

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_WORKORDER()
        {
            InitializeComponent();            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                _ShopID = Util.NVC(tmps[0]);
                _AreaID = Util.NVC(tmps[1]);
                _EqptSegment = Util.NVC(tmps[2]);
                _ProcID = Util.NVC(tmps[3]);
                _EqptID = Util.NVC(tmps[4]);
                _Now_Workorder = Util.NVC(tmps[5]);
            }
            else
            {
                _ShopID = "";
                _AreaID = "";
                _EqptSegment = "";
                _ProcID = "";
                _EqptID = "";
                _Now_Workorder = "";
            }

            GetWorkOrder();
        }
        
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            ApplyPermissions();

            //InitializeWorkorderQuantityInfo();

            //GetWorkOrder();

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //ApplyPermissions();
            
            //GetWorkOrder();
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;
                
                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = idx;

                // 선택 작지 수량 설정
                //SetWorkOrderQtyInfo(dtRow);

                // 실적 조회 호출..
                //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                //SearchParentProductInfo(dtRow);
            }
            
        }
        
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
                    //if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EIO_WO_DETL_ID")).Equals(""))
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#56BE1C"));
                    //}
                }
            }));
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
            {
                Util.MessageInfo("SFU1275");    //선택된 항목이 없습니다.
                return;
            }

            _WOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID"));
            _WOIDDETAIL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //GetParentSearchConditions();

                // 자동 조회 처리....
                //if (!CanSearch())
                //    return;

                if (_EqptSegment.Equals("") || _EqptSegment.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                if (_EqptID.Equals("") || _EqptID.ToString().Trim().Equals("SELECT"))
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

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (sender == null)
            //        return;

            //    LGCDatePicker dtPik = (sender as LGCDatePicker);

            //    if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            //    {
            //        dtPik.Text = System.DateTime.Now.ToLongDateString();
            //        dtPik.SelectedDateTime = System.DateTime.Now;
            //        Util.Alert("SFU1738");      //오늘 이전 날짜는 선택할 수 없습니다.
            //        //e.Handled = false;
            //        return;
            //    }

            //    btnSearch_Click(null, null);
            //}));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (sender == null)
            //        return;

            //    LGCDatePicker dtPik = (sender as LGCDatePicker);

            //    if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            //    {
            //        dtPik.Text = System.DateTime.Now.ToLongDateString();
            //        dtPik.SelectedDateTime = System.DateTime.Now;
            //        Util.Alert("SFU1698");      //시작일자 이전 날짜는 선택할 수 없습니다.
            //        //e.Handled = false;
            //        return;
            //    }

            //    btnSearch_Click(null, null);
            //}));
        }
        #endregion

        #region Method

        #region [BizCall]
        private void GetProcessFPInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["PROCID"] = _ProcID;

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

                if (_Process_Plan_Level_Code.Equals("PROC"))//if (!_Process_ErpUseYN.Equals("Y") && _Process_Plan_Level_Code.Equals("PROC")) // PROCESS 인 경우 공정 자동 체크 및 disable.
                {
                    _FP_REF_PROCID = "";

                    chkProc.IsChecked = true;
                    chkProc.IsEnabled = false;
                }
                else
                {
                    _FP_REF_PROCID = "";

                    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    {
                        chkProc.IsChecked = true;
                        chkProc.IsEnabled = true;
                    }
                    else
                    {
                        chkProc.IsChecked = false;
                        chkProc.IsEnabled = true;
                    }
                }


                // Reference 공정인 경우는 REF 공정 정보 설정.
                if (!_Process_ErpUseYN.Equals("Y") && Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                {
                    _FP_REF_PROCID = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);

                    chkProc.IsChecked = true;
                    chkProc.IsEnabled = false;
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWOInfo(string sWOID, out string sRet, out string sMsg)
        {
            try
            {
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
                        sMsg = "SFU3058";  //선택 가능한 상태의 작업지시가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";//존재하지 않습니다.
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
            }
        }

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="drWOInfo">선택 한 작업지시 정보 DataRow</param>
        private void SetWorkOrderSelect(DataRow drWOInfo)
        {
            if (drWOInfo == null)
                return;

            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = _EqptID;
                newRow["WO_DETL_ID"] = Util.NVC(drWOInfo["WO_DETL_ID"]);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        public void GetWorkOrder()
        {
            if (_ProcID.Length < 1 || _EqptID.Length < 1 || _EqptSegment.Length < 1)
                return;

            // Process 정보 조회
            GetProcessFPInfo();

            // 현 작지 정보 조회.
            //string sWODetl = GetEIOInfo();

            string sPrvWODTL = string.Empty;

            //if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
            //{
            //    int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            //    if (idx >= 0)
            //        sPrvWODTL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
            //}

            sPrvWODTL = _Now_Workorder;

            ClearWorkOrderInfo();
            //ParentDataClear();  // Caller 화면 Data Clear.

            DataTable searchResult = null;

            if (_Process_ErpUseYN.Equals("Y"))  // ERP 실적 전송인 경우는 Workorder Inner Join..
            {
                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    searchResult = GetEquipmentWorkOrderByProcWithInnerJoin();
                else
                    searchResult = GetEquipmentWorkOrderWithInnerJoin();
            }
            else
            {
                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    searchResult = GetEquipmentWorkOrderByProc();
                else
                    searchResult = GetEquipmentWorkOrder();
            }

            if (searchResult == null)
                return;

            dgWorkOrder.ItemsSource = DataTableConverter.Convert(searchResult);

            // 현 작업지시 정보 Top Row 처리 및 고정..
            if (searchResult.Rows.Count > 0)
            {
                if (!Util.NVC(searchResult.Rows[0]["EIO_WO_DETL_ID"]).Equals(""))
                    dgWorkOrder.FrozenTopRowsCount = 1;
                else
                    dgWorkOrder.FrozenTopRowsCount = 0;
            }

            // 이전 선택 작지 선택
            if (!sPrvWODTL.Equals(""))
            {
                int idx = _Util.GetDataGridRowIndex(dgWorkOrder, "WO_DETL_ID", sPrvWODTL);

                if (idx >= 0)
                {
                    //searchResult.Rows[idx]["CHK"] = true;

                    //dgWorkOrder.BeginEdit();
                    //dgWorkOrder.ItemsSource = DataTableConverter.Convert(searchResult);
                    //dgWorkOrder.EndEdit();

                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);
                    }

                    DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                    // 재조회 처리.
                    //ReChecked(idx);
                }
            }
            else // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
            {
                for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        dgWorkOrder.SelectedIndex = i;
                        break;
                    }
                }
            }

            // 공정 조회인 경우 설비 정보 Visible 처리.
            if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            else
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
            
        }

        /// <summary>
        /// 설비별 작업지시 리스트 조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetEquipmentWorkOrder()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = _EqptSegment;
                searchCondition["EQPTID"] = _EqptID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = _EqptSegment;
                searchCondition["EQPTID"] = _EqptID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
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
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = _EqptSegment;
                searchCondition["EQPTID"] = _EqptID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderByProcWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = _EqptSegment;
                searchCondition["EQPTID"] = _EqptID;
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
        }
        
        public DataTable GetWorkOrderSummaryInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AreaID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfoWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AreaID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_WITH_FP", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        public DataTable GetWorkOrderSummaryInfoByProcID()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO_BY_PROCID();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AreaID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_BY_PROCID", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfoByProcIDWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO_BY_PROCID();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AreaID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? _ProcID : _FP_REF_PROCID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        private string GetEIOInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_PLAN_DETAIL_BYEQPTID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = _EqptID;

                inTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORER_PLAN_DETAIL_BYEQPTID", "INDATA", "OUTDATA", inTable);

                if (dtResult == null || dtResult.Rows.Count < 1)
                    return "";

                //txtWOID.Text = Util.NVC(dtResult.Rows[0]["WO_DETL_ID"]);

                return Util.NVC(dtResult.Rows[0]["WO_DETL_ID"]);
                //dgWorkOrder.TopRows.RemoveAt(0);
                //dgWorkOrder.FrozenTopRowsCount = 0;

                //// Top Row 설정...
                //C1.WPF.DataGrid.DataGridRow item = new C1.WPF.DataGrid.DataGridRow();
                //dgWorkOrder.TopRows.Add(item);

                //dgWorkOrder.FrozenTopRowsCount = 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        #region [Validation]
        //private bool CanSearch()
        //{
        //    bool bRet = false;

        //    if (PROCID.Length < 1)
        //    {
        //        Util.Alert("SFU1456");      //공정 정보가 없습니다.
        //        return bRet;
        //    }

        //    if (EQPTSEGMENT.Equals("") || EQPTSEGMENT.ToString().Trim().Equals("SELECT"))
        //    {
        //        Util.Alert("SFU1223");  //라인을 선택하세요.
        //        return bRet;
        //    }

        //    if (EQPTID.Equals("") || EQPTID.ToString().Trim().Equals("SELECT"))
        //    {
        //        Util.Alert("SFU1673");      //설비를 선택하세요.
        //        return bRet;
        //    }

        //    bRet = true;
        //    return bRet;
        //}
        #endregion

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            if (FrameOperation != null)
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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

            if (_EqptID.Trim().Equals("") || _ProcID.Trim().Equals("") || _EqptSegment.Trim().Equals(""))
                return bRet;

            if (Util.NVC(dtRow["EIO_WO_SEL_STAT"]).Equals("Y"))
            {
                Util.MessageValidation("SFU3061");
                return bRet;
            }

            // 선택 가능한 작업지시 인지..
            //if (!Util.NVC(dtRow["WO_STAT_CODE"]).Equals("REL") &&
            //    !Util.NVC(dtRow["WO_STAT_CODE"]).Equals("PDLV"))
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작이 가능한 상태의 작업지시가 아닙니다.", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return bRet;
            //}

            // 현 작업중인 실적 중 확정 처리 안된 실적 존재 확인.

            //// 계획수량 >= 생산수량 인지..
            //if (double.Parse(txtBlockPlanQty.Text) < double.Parse(txtBlockOutQty.Text))
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("생산수량이 계획수량보다 많아 선택할 수 없습니다.", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

            bRet = true;

            return bRet;
        }
        
        /// <summary>
        /// 작업지시 리스트에서 해당 작지 정보 조회
        /// </summary>
        /// <param name="sWorkOrder">작업지시 번호</param>
        /// <param name="bFindRunning">생산중인 작업지시를 찾을지 여부</param>
        /// <returns>조회 결과</returns>
        private DataRow[] GetWorkOrderInfo(string sWorkOrder, bool bFindRunning = false)
        {
            string expression = string.Empty;

            DataTable dtTemp;
            DataRow[] foundRows;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count < 1)
                return null;

            dtTemp = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

            // Running 중인 Workorder 찾기.
            if (bFindRunning)
                sWorkOrder = FindRunningWorkOrderNumber();

            expression = "WOID = '" + sWorkOrder + "'";

            foundRows = dtTemp.Select(expression);

            return foundRows;
        }

        /// <summary>
        /// 생산 중인 작업지시 정보 조회
        /// </summary>
        /// <returns>작업지시 번호</returns>
        private string FindRunningWorkOrderNumber()
        {
            string sRet = string.Empty;
            DataTable dtTemp;
            DataRow[] foundRows;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count < 1)
                return "";

            dtTemp = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

            foundRows = dtTemp.Select("WO_STAT_CODE = '작업중'");  // 하드코딩 삭제 필요....

            if (foundRows.Length > 0)
                sRet = foundRows[0]["WOID"].ToString();

            return sRet;
        }

        /// <summary>
        /// 작업지시 선택 처리
        /// </summary>
        /// <param name="sWorkOrder">작업지시 번호</param>
        private void WorkOrderChange(DataRow dtRow)
        {
            //DataRow[] fndRow = GetWorkOrderInfo(sWorkOrder);

            if (dtRow == null)
                return;

            // Biz Call...
            SetWorkOrderSelect(dtRow);

            // 재조회
            GetWorkOrder();

            //SearchProductLot(GetWorkOrderInfo(sWorkOrder));
        }
        
        /// <summary>
        /// 작업지시 Datagrid Clear
        /// </summary>
        public void ClearWorkOrderInfo()
        {
            Util.gridClear(dgWorkOrder);

            //txtWOID.Text = "";
        }
        
        #endregion

        #endregion

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void dgWorkOrder_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;

                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content is RadioButton)
                            {
                                RadioButton rdoButton = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

                                if (rdoButton != null)
                                {
                                    if (rdoButton.DataContext == null)
                                        return;

                                    // 부모 조회 없으므로 로직 수정..
                                    if (!(bool)rdoButton.IsChecked && (rdoButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                                    {
                                        DataRow dtRow = (rdoButton.DataContext as DataRowView).Row;

                                        for (int i = 0; i < dg.Rows.Count; i++)
                                            if (e.Cell.Row.Index == i)   // Mode = OneWay 이므로 Set 처리.
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                                            else
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                        //row 색 바꾸기
                                        dgWorkOrder.SelectedIndex = e.Cell.Row.Index;

                                        // 선택 작지 수량 설정
                                        //SetWorkOrderQtyInfo(dtRow);

                                        // 실적 조회 호출..
                                        //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                                        //SearchParentProductInfo(dtRow);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}