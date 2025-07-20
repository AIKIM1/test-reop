/*************************************************************************************
 Created Date : 2020.10.13
      Creator : 정문교
   Decription : 작업지시 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  정문교 : Initial Created.   
                       UC_WORKORDER_MX Copy CMM_WORKORDER_MX_DRB 생성
  2021.02.11  정문교 : 계획일자 칼럼 추가.   
  2021.05.24  정문교 : W/O 선택시 직전 완공된 반제품과 선택 W/O 반제품이 틀린경우 해당 설비의 믹서 자재 설정 Clear.   
  2023.07.13  김태우 : 오창2산단 NFF. DAM MIXING 추가.E0430
  2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경
  2024.12.23  이동주 : E20240904-000991 [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)  



  2025.07.09  이주원 : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_WORKORDER_MX_DRB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WORKORDER_MX_DRB : C1Window, IWorkArea
    {
        #region Declaration & Constructor        
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;

        private string _FP_REF_PROCID = string.Empty;
        private string _Process_ErpUseYN = string.Empty;        // Workorder 사용 공정 여부.
        private string _Process_Plan_Level_Code = string.Empty; // 계획 Level 코드. (EQPT, PROC .. )
        private string _Process_Plan_Mngt_Type_Code = string.Empty; // 계획 관리 유형 (WO, MO, REF..)

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

        public CMM_WORKORDER_MX_DRB()
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

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _EqptSegment = Util.NVC(tmps[0]);
            _ProcID = Util.NVC(tmps[1]);
            _EqptID = Util.NVC(tmps[2]);
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetParameters();
            GetWorkOrder();

            // 선택된 행이 있으면 WO BOM 조회
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx > -1)
            {
                //2025.06.05  양영재 : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회             
                SelectWorkOrderBOM(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID").GetString(),
                    DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID").GetString(),                    
                    DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "CP_VER_CODE").GetString());

                //SelectWorkOrderBOM(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID").GetString(),
                //    DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID").GetString());
            }

            //btnSelectCancel.IsEnabled = false;

            chkProc.Checked += chkProc_Checked;
            chkProc.Unchecked += chkProc_Unchecked;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

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

                btnSearch_Click(null, null);
            }));
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

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "EIO_WO_SEL_STAT").GetString();
                    btnSelectCancel.IsEnabled = workState == "Y";
                }

                // WO BOM 조회
                //2025.06.05  양영재 : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회   
                SelectWorkOrderBOM(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "WOID").GetString(),
                    DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "PRODID").GetString(),
                    DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CP_VER_CODE").GetString());

                //SelectWorkOrderBOM(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "WOID").GetString(),
                //    DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "PRODID").GetString());
            }

            //2024.12.13 DJLEE E20240904-000991
            if (dgWorkOrder.SelectedIndex > -1)
            {                
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                SelectCPVersionSolutioMixer(dgWorkOrder.GetCell(dgWorkOrder.SelectedIndex, dgWorkOrder.Columns["CP_VER_CODE"].Index));

                SelectWorkOrderBOM(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "WOID").GetString(),
                    DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "PRODID").GetString(),
                    DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CP_VER_CODE").GetString());
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

        //2025.06.05  양영재  : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회  
        private void dgWorkOrder_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.Columns.Contains("CP_VER_CODE"))
            {
                SelectWorkOrderBOM(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WOID").GetString(),
                    DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID").GetString(),
                    DataTableConverter.GetValue(e.Cell.Row.DataItem, "CP_VER_CODE").GetString());
            }
        }

        private void dgWorkOrder_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    // WO BOM 조회
                    //2025.06.05  양영재  : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회             
                    SelectWorkOrderBOM(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WOID").GetString(),
                        DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID").GetString(),
                        DataTableConverter.GetValue(e.Cell.Row.DataItem, "CP_VER_CODE").GetString());

                    //SelectWorkOrderBOM(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WOID").GetString(),
                    //    DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID").GetString());

                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;

                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            RadioButton rb = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

                            if (rb != null)
                            {
                                if (rb.DataContext == null)
                                    return;

                                // 부모 조회 없으므로 로직 수정..
                                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                                for (int i = 0; i < dg.Rows.Count; i++)
                                    if (e.Cell.Row.Index == i)   // Mode = OneWay 이므로 Set 처리.
                                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                                    else
                                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);

                                //row 색 바꾸기
                                dgWorkOrder.SelectedIndex = e.Cell.Row.Index;

                                //선택 취소 버튼 Enabled 속성 설정
                                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                                {
                                    string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[e.Cell.Row.Index].DataItem, "EIO_WO_SEL_STAT").GetString();
                                    btnSelectCancel.IsEnabled = workState == "Y";
                                }

                            }

                            break;
                    }

                    //2024.12.13 DJLEE E20240904-000991
                    SelectCPVersionSolutioMixer(e.Cell);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
                return;

            DataRow dtRow = (dgWorkOrder.Rows[idx].DataItem as DataRowView).Row;

            if (!CanChangeWorkOrder(dtRow))
                return;

            //작업지시를 변경하시겠습니까?
            Util.MessageConfirm("SFU2943", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(dtRow);
                }
            });
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_EqptID) || string.IsNullOrEmpty(_ProcID) || string.IsNullOrEmpty(_EqptSegment) || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;

                DataRowView drv = dgWorkOrder.Rows[idx].DataItem as DataRowView;
                if (drv != null)
                {
                    DataRow dr = drv.Row;

                    // 작업지시를 선택취소 하시겠습니까?
                    Util.MessageConfirm("SFU2944", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SelectWorkInProcessStatus(_EqptID, _ProcID, _EqptSegment, (table, ex) =>
                            {
                                if (CommonVerify.HasTableRow(table))
                                {
                                    if (table.Rows[0]["WIPSTAT"].GetString() == "PROC")
                                    {
                                        Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                                        return;
                                    }
                                    else
                                    {
                                        WorkOrderChange(dr, false);
                                    }
                                }
                                else
                                {
                                    WorkOrderChange(dr, false);
                                }
                            });
                        }
                    });
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
                _Process_Plan_Mngt_Type_Code = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);

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
                        sMsg = "선택 가능한 상태의 작업지시가 아닙니다.";
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "존재하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
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

                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("PLAN_DATE"))
                {
                    string sPlanDate = Util.NVC(dtResult.Rows[0]["PLAN_DATE"]);
                    if (sPlanDate.Length >= 6 && sCalDateYMD.Length >= 6)
                    {
                        // W/O 공정인 경우에만 체크.
                        if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                        {
                            if (Util.NVC_Int(sPlanDate) >= Util.NVC_Int(sCalDateYMD))  // Today ~ 해당 월의 W/O만 선택 가능.
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
                newRow["EQPTID"] = _EqptID;

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

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="drWOInfo">선택 한 작업지시 정보 DataRow</param>
        private void SetWorkOrderSelect(DataRow drWOInfo, bool isSelectFlag = true)
        {
            if (drWOInfo == null)
                return;

            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = _EqptID;
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(drWOInfo["WO_DETL_ID"]) : string.Empty;
                newRow["CP_VER_CODE"] = (_ProcID.Equals(Process.BS) || _ProcID.Equals(Process.CMC)) && isSelectFlag ? Util.NVC(drWOInfo["CP_VER_CODE"]) : string.Empty; //2024.12.13 DJLEE E20240904-000991
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                if (isSelectFlag)
                {
                    Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.

                    DataTable inClear = new DataTable();
                    inClear.Columns.Add("EQPTID", typeof(string));
                    inClear.Columns.Add("PRODID", typeof(string));
                    inClear.Columns.Add("USERID", typeof(string));

                    // CSR : E20250328-001618
                    inClear.Columns.Add("WOID", typeof(string));
                    inClear.Columns.Add("AREAID", typeof(string));
                    inClear.Columns.Add("PROCID", typeof(string));
                    inClear.Columns.Add("LOTTYPE", typeof(string));
                    inClear.Columns.Add("ALL_FLAG", typeof(string));

                    newRow = inClear.NewRow();
                    newRow["EQPTID"] = _EqptID;
                    newRow["PRODID"] = Util.NVC(drWOInfo["PRODID"]);
                    newRow["USERID"] = LoginInfo.USERID;

                    // CSR : E20250328-001618
                    newRow["WOID"] = Util.NVC(drWOInfo["WOID"]);
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["PROCID"] = _ProcID;
                    newRow["LOTTYPE"] = Util.NVC(drWOInfo["LOTTYPE"]);
                    newRow["ALL_FLAG"] = (Util.NVC(drWOInfo["PRODID"]).Substring(3, 2).Equals("CA")) ? newRow["ALL_FLAG"] = "Y" : newRow["ALL_FLAG"] = null;

                    inClear.Rows.Add(newRow);

                    DataTable dtClear = new ClientProxy().ExecuteServiceSync("BR_MAT_REG_MIXER_TANK_MTRL_SET_CLEAR", "INDATA", null, inClear);
                }
                else
                    Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        private void GetWorkOrder(bool isSelectFlag = true)
        {
            if (_ProcID.Length < 1 || _EqptID.Length < 1 || _EqptSegment.Length < 1)
                return;

            // 선분산믹서 MODLID -> PRODDESC 표시
            if (string.Equals(_ProcID, Process.PRE_MIXING))
                dgWorkOrder.Columns["PRODDESC"].Visibility = Visibility.Visible;
            else
            {
                // 2023.10.09 강성묵 E20230927-000880 전극 Nickname 표기 변경
                //dgWorkOrder.Columns["MODLID"].Visibility = Visibility.Visible;
                dgWorkOrder.Columns["MODELID_NEW"].Visibility = Visibility.Visible;
            }

            // Process 정보 조회
            GetProcessFPInfo();

            // 현 작지 정보 조회.
            //string sWODetl = GetEIOInfo();

            string sPrvWODTL = string.Empty;

            if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                if (idx >= 0)
                    sPrvWODTL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
            }

            // 취소인 경우에는 선택 없애도록..
            if (!isSelectFlag)
                sPrvWODTL = "";

            ClearWorkOrderInfo();

            btnSelectCancel.IsEnabled = false;

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

            Util.GridSetData(dgWorkOrder, searchResult, null, true);

            // 현 작업지시 정보 Top Row 처리 및 고정..
            if (searchResult.Rows.Count > 0)
            {
                //if (!Util.NVC(searchResult.Rows[0]["EIO_WO_DETL_ID"]).Equals(""))
                //    dgWorkOrder.FrozenTopRowsCount = 1;
                //else
                //    dgWorkOrder.FrozenTopRowsCount = 0;
            }

            // 이전 선택 작지 선택
            if (!sPrvWODTL.Equals(""))
            {
                int idx = _Util.GetDataGridRowIndex(dgWorkOrder, "WO_DETL_ID", sPrvWODTL);

                if (idx >= 0)
                {
                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);
                    }

                    DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                    // 재조회 처리.
                    ReChecked(idx);
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

            //선택 취소 버튼 Enabled 속성 설정
            if (CommonVerify.HasDataGridRow(dgWorkOrder))
            {
                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx != -1)
                {
                    string workState = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"));
                    btnSelectCancel.IsEnabled = workState == "Y";
                }
            }

            // 공정 조회인 경우 설비 정보 Visible 처리.
            if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            else
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

            // 선분산MIXER의 경우 PRJT 제외하고 그 자리에 PRODDESC보여줌
            if (string.Equals(_ProcID, Process.PRE_MIXING))
            {
                dgWorkOrder.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                dgWorkOrder.Columns["PRODDESC"].DisplayIndex = dgWorkOrder.Columns["PRJT_NAME"].DisplayIndex;

                // 2023.10.09  강성묵: E20230927 - 000880 전극 Nickname 표기 변경
                dgWorkOrder.Columns["PRJT_NAME_NEW"].Visibility = Visibility.Collapsed;

                // E20250108-001229  FP 모델의 PRJT_NAME 출력
                dgWorkOrder.Columns["CURR_WO_APPLY_MODLID_PRJT_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                // 2023.10.09  강성묵: E20230927 - 000880 전극 Nickname 표기 변경
                dgWorkOrder.Columns["NEXT_PRJT_NAME"].Visibility = Visibility.Collapsed;
            }

            // E20250108-001229
            if (_ProcID.Equals(Process.BS) || _ProcID.Equals(Process.CMC))
                dgWorkOrder.Columns["CP_VER_CODE"].Visibility = Visibility.Visible;
            else
                dgWorkOrder.Columns["CP_VER_CODE_TXT"].Visibility = Visibility.Visible;
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

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_MX", "INDATA", "OUTDATA", inTable);
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

        private static void SelectWorkInProcessStatus(string eqptCode, string processCode, string eqptSegmentCode, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = eqptCode;
                inData["PROCID"] = processCode;
                inData["EQSGID"] = eqptSegmentCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.AlertByBiz(bizRuleName, ex.Message, ex.ToString());
                        return;
                    }

                    actionCompleted?.Invoke(result, null);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        //2025.06.05  양영재  : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회             
        private void SelectWorkOrderBOM(string WOID, string sPRODID, string sCP_VER_CODE)
        //private void SelectWorkOrderBOM(string WOID, string sPRODID)
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                //2025.06.05  양영재  : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회             
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CP_VER_CODE", typeof(string));
                inTable.Columns.Add("ALL_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WOID"] = WOID;
                newRow["PROCID"] = _ProcID;
                newRow["PRODID"] = sPRODID;
                //2025.06.05  양영재  : E20250502-001161 [품질DX팀] 솔루션ID 설비 CP버전을 키값으로 BInder 및 CMC 공정 진척 WO 팝업 소요량 조회             
                newRow["EQPTID"] = _EqptID;
                newRow["CP_VER_CODE"] = sCP_VER_CODE;
                if (sPRODID.Substring(3, 2).Equals("CA"))
                {
                    newRow["ALL_FLAG"] = "Y";
                }
                else
                {
                    newRow["ALL_FLAG"] = null;
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_COM_SEL_TB_SFC_WO_MTRL_ALL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListMaterial, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectCPVersionSolutioMixer(C1.WPF.DataGrid.DataGridCell currCell)
        {
            if (currCell == null || currCell.Row == null)
            {
                return;
            }

            dgWorkOrder.IsReadOnly = Convert.ToString(currCell.Column.Name).Equals("CP_VER_CODE") ? false : true;
            C1.WPF.DataGrid.DataGridComboBoxColumn dgcolz = dgWorkOrder.Columns["CP_VER_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;

            if (_ProcID.Equals(Process.BS) || _ProcID.Equals(Process.CMC))
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = DataTableConverter.GetValue(currCell.Row.DataItem, "PRODID").GetString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_PROD_RESTRCT_VER_INFO_SOLUTION", "INDATA", "OUTDATA", inTable);

                dgcolz.IsReadOnly = false;
                dgcolz.ItemsSource = dtResult.Copy().AsDataView();
                dgcolz.DisplayMemberPath = "CP_VER_CODE";
                dgcolz.SelectedValuePath = "CP_VER_CODE";

                foreach (C1.WPF.DataGrid.DataGridRow row in dgWorkOrder.Rows)
                {
                    //if (!dgWorkOrder.GetValue(row.Index, "EIO_WO_SEL_STAT").Equals("Y"))
                    if (!"Y".Equals(dgWorkOrder.GetValue(row.Index, "EIO_WO_SEL_STAT")))
                    {
                        DataTableConverter.SetValue(row.DataItem, "CP_VER_CODE", string.Empty);
                    }
                }

                if (dtResult.Rows.Count == 1)
                {
                    dgcolz.SetCellValue(currCell.Row, dtResult.Rows[0]["CP_VER_CODE"].ToString());
                }
                else
                {
                    // E20250401-000355 [전극기술표준화팀]UC1 PDS 고도화에 따른, MES CP 버전 운용 기능 개선 요청의
                    dgcolz.SetCellValue(currCell.Row, SelectMaxCPVersionSolutioMixer(DataTableConverter.GetValue(currCell.Row.DataItem, "PRODID").GetString()));
                }
            }
            else
            {
                dgcolz.IsReadOnly = true;
            }
        }


        #endregion

        #region [Validation]
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
                Util.MessageValidation("SFU3061");  // 이미 선택된 작업지시 입니다.
                return bRet;
            }

            // Workorder 내려오는 공정만 체크 필요.
            if (_Process_ErpUseYN.Equals("Y"))
            {
                // 선택 가능한 작지 여부 확인.
                string sRet = string.Empty;
                string sMsg = string.Empty;

                GetWOInfo(Util.NVC(dtRow["WOID"]), out sRet, out sMsg);
                if (sRet.Equals("NG"))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

            // W/O Rolling에 따라 자동 Over Completion을 위하여 하기 Validation 적용 [2017-09-18]
            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr = dt?.Select("EIO_WO_SEL_STAT = 'Y'");

            // 2023.10.09 강성묵 E20230927-000880 전극 Nickname 표기 변경
            // if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("MODLID"))
            if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("MODELID_NEW"))
            {
                foreach (DataRow drTmp in dr)
                {
                    // 2023.10.09 강성묵 E20230927-000880 전극 Nickname 표기 변경
                    // if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && Util.NVC(dtRow["MODLID"]).Equals(Util.NVC(drTmp["MODLID"])))
                    if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && Util.NVC(dtRow["MODELID_NEW"]).Equals(Util.NVC(drTmp["MODELID_NEW"])))
                    {
                        Util.MessageValidation("SFU4117"); // 동일한 모델, WO Type의 WO가 이미 선택되어 있습니다.
                        return bRet;
                    }
                }
            }

            // 버전 체크
            if (string.IsNullOrWhiteSpace(dtRow["PROD_VER_CODE"].ToString()))
            {
                if (_ProcID == Process.PRE_MIXING ||
                    _ProcID == Process.BS ||
                    _ProcID == Process.CMC ||
                    _ProcID == Process.InsulationMixing ||
                    _ProcID == Process.DAM_MIXING
                    )
                    ;
                else
                {
                    Util.MessageValidation("SFU7350"); // 생산계획에서 전극 버전이 설정된 W/O만 선택 가능 합니다.
                    return bRet;
                }
            }

            //2024.12.13 DJLEE E20240904-000991 CP 버전 선택 알림
            if (_ProcID.Equals(Process.BS) || _ProcID.Equals(Process.CMC))
            {
                if (dgWorkOrder.SelectedIndex > -1)
                {
                    string cpVerCode = DataTableConverter.GetValue(dgWorkOrder.GetCell(dgWorkOrder.SelectedIndex, dgWorkOrder.Columns["CP_VER_CODE"].Index).Row.DataItem, "CP_VER_CODE").GetString();
                    C1.WPF.DataGrid.DataGridComboBoxColumn dgcolz = dgWorkOrder.Columns["CP_VER_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                    if (string.IsNullOrEmpty(cpVerCode) && ((System.Data.DataView)dgcolz.ItemsSource).Count > 0)
                    {
                        Util.MessageValidation("SFU4925", "CP VERSION"); // %1(을)를 선택하세요.
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        /// <summary>
        /// 작업지시 선택 처리
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isSelectFlag"> 선택 처리:true 선택 취소:false </param>
        private void WorkOrderChange(DataRow dr, bool isSelectFlag = true)
        {
            if (dr == null) return;

            SetWorkOrderSelect(dr, isSelectFlag);

            if (isSelectFlag)
            {
                // WO 선택
                _WOID = Util.NVC(dr["WOID"]);
                _WOIDDETAIL = Util.NVC(dr["WO_DETL_ID"]);

                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                // WO 선택 취소
                GetWorkOrder(isSelectFlag);
            }
        }

        /// <summary>
        /// 작업지시 Datagrid Clear
        /// </summary>
        private void ClearWorkOrderInfo()
        {
            Util.gridClear(dgWorkOrder);

            for (int i = 0; i < dgWorkOrder.FilteredColumns.Length; i++)
            {
                dgWorkOrder.FilteredColumns[i].FilterState.FilterInfo.Clear();
            }
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = iRow;
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

        private bool IsPdsCpVersionConditionVisibility()
        {

            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("ATTR1", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "PDS_CP_VER_APPLY_AREA";
                dr["COM_CODE"] = "PDS_CP_VER_APPLY_AREA_YN";
                dr["USE_FLAG"] = "Y";
                dr["ATTR1"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    return true;
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

        // E20250401-000355 [전극기술표준화팀]UC1 PDS 고도화에 따른, MES CP 버전 운용 기능 개선 요청의 : PDS 적용 공장 및 BINDER, CMC 공정인 경우 4자리 M으로 시작하는 CP 버전 중 가장 최신버전을 가져옴
        private string SelectMaxCPVersionSolutioMixer(string _ProdID)
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = _ProdID;
                inTable.Rows.Add(newRow);

                // PDS 적용 공장이면서 CP MAX 버전 FLAG가 Y어야 함 -> PDS_CP_VER_APPLY_PROC의 속성 2가 Y
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_PROD_RESTRCT_VER_INFO_NEW_MAX_CP", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["CP_VER_CODE"].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }
        #endregion

        #endregion



    }

}