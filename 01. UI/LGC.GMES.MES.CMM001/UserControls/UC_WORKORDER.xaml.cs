/*************************************************************************************
 Created Date : 2016.08.16
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 공정진척화면의 작업지시 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.16  INS 김동일K : Initial Created.
  2017.01.18  신광희 : 선택 취소 버튼 기능 추가(진행중인 LOT이 없을 경우 작업지시 선택 취소 처리,  dgWorkOrder 의 선택 컬럼이 Y 인경우에 한하여 버튼 Enable 속성 설정)
  2022.11.04  강호운 : C20221107-000542 - LASER_ABLATION 공정추가
  2024.09.09  김도형 : [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善 
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
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_WORKORDER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_WORKORDER : UserControl, IWorkArea
    {
        #region Declaration & Constructor        
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _CoatSideType = string.Empty;
        private string _FP_REF_PROCID = string.Empty;
        private string _Process_ErpUseYN = string.Empty;        // Workorder 사용 공정 여부.
        private string _Process_Plan_Level_Code = string.Empty; // 계획 Level 코드. (EQPT, PROC .. )
        private string _Process_Plan_Mngt_Type_Code = string.Empty; // 계획 관리 유형 (WO, MO, REF..)

        // Old 제품 코드 와 New 제품코드 
        private string _NewProdID = string.Empty;
        private string _OldProdID = string.Empty;

        private string _CURR_WOID = string.Empty;          // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        private string _AFTER_WOID = string.Empty;         // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善 
        private DataRow _DtWoChgRow = null;                // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善 

        public UserControl _UCParent; //Caller
        public UcBaseElec _UCElec;
        public UcBaseElec_CWA _UCElec_CWA;
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

        public string COATSIDETYPE
        {
            get { return _CoatSideType; }
            set { _CoatSideType = value; }
        }

        public string PRODID { get; set; }
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public UC_WORKORDER()
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
        
        private void InitializeGridColumns()
        {
            if (dgWorkOrder == null)
                return;

            /*
             * C/Roll, S/Roll, Lane수 적용 공정.
             *     C/ROLL = PLAN_QTY(S/ROLL) / LANE_QTY
             * E2000  - TOP_COATING
             * E2300  - INS_COATING
             * S2000  - SRS_COATING
             * E2500  - HALF_SLITTING
             * E3000  - ROLL_PRESSING
             * E3500  - TAPING
             * E3800  - REWINDER
             * E3300  - LASER_ABLATION [C20221107-000542]
             * E3900  - BACK_WINDER
             */
            if (_ProcID.Equals(Process.TOP_COATING) ||
                _ProcID.Equals(Process.INS_COATING) ||
                _ProcID.Equals(Process.SRS_COATING) ||
                _ProcID.Equals(Process.HALF_SLITTING) ||
                _ProcID.Equals(Process.ROLL_PRESSING) ||
                _ProcID.Equals(Process.TAPING) ||
                _ProcID.Equals(Process.REWINDER) || 
                _ProcID.Equals(Process.LASER_ABLATION) ||
                _ProcID.Equals(Process.BACK_WINDER))
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                }
            }

            // 조립 공정인 경우 버전 컬럼 HIDDEN.
            if (_ProcID.Equals(Process.NOTCHING) ||
                _ProcID.Equals(Process.LAMINATION) ||
                _ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.PACKAGING))
            {
                if (dgWorkOrder.Columns.Contains("PROD_VER_CODE"))
                {
                    dgWorkOrder.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
            }

            // 패키지 공정인 경우만 모델랏 정보 표시
            if (dgWorkOrder.Columns.Contains("MDLLOT_ID"))
            {
                if (_ProcID.Equals(Process.PACKAGING))
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                }
            }

            // 라미 공정일 경우 Cell Type (CLSS_NAME : 분류명) 컬럼 표시 -> 극성 컬럼 Hidden
            if(_ProcID.Equals(Process.LAMINATION))
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility  = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility   = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility  = Visibility.Visible;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility   = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            
            InitializeGridColumns();

            btnSelectCancel.IsEnabled = false;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            GetWorkOrder();
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
                    // 양극 설비 LTO 모델 음영표시
                    if (!string.Equals(PROCID, Process.PRE_MIXING) && !string.Equals(PROCID, Process.MIXING) && !string.Equals(PROCID, Process.COATING))
                    {
                        if ( string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LTO_FLAG"), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }

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
            if (sender == null)
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
                return;

            DataRow dtRow = (dgWorkOrder.Rows[idx].DataItem as DataRowView).Row;
            PRODID = dtRow["PRODID"].ToString();

            if (!CanChangeWorkOrder(dtRow))
                return;

            _AFTER_WOID = dtRow["WOID"].ToString();     // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善 
            _DtWoChgRow = dtRow;                        // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善

            string sElecProcTankWashCheckUseYn = string.Empty;
            sElecProcTankWashCheckUseYn = GetElecProcTankWashCheckUseYn();  // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善

            if (sElecProcTankWashCheckUseYn.Equals("Y"))  //동별 공통코드 등록여부에 따라 실행 ELEC_PROC_TANK_WASH_CHECK
            {
                if (isTankWashCheck())   // DA_PRE_SEL_TB_SFC_ELTR_MIXER_TANK_CLEAN_MNGT	SFC 전극 믹서 탱크 세정 관리 체크
                {
                    LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_TANK_WASH_CHECK_YN wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_TANK_WASH_CHECK_YN();
                    wndPopup.FrameOperation = FrameOperation;
                    object[] parameters = new object[2];
                    // parameters[0] = sLotid;
                    C1WindowExtension.SetParameters(wndPopup, parameters);

                    wndPopup.Closed += new EventHandler(wndTankWashCheckYn_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                    return;
                }
            }

            // 작업지시를 변경하시겠습니까?
            Util.MessageConfirm("SFU2943", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(dtRow);
                    //SetWorkOrderQtyInfo(sWorkOrder);
                }
            });
        }

        // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        private void WorkOrderChangeWashCheckRun()
        {
            //작업지시를 변경하시겠습니까?
            Util.MessageConfirm("SFU2943", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_DtWoChgRow != null)  //&& _DtWoChgRow.co > 0
                    {
                        WorkOrderChange(_DtWoChgRow);
                        //SetWorkOrderQtyInfo(sWorkOrder);
                    }
                }
            });
        }

        // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        private void wndTankWashCheckYn_Closed(object sender, EventArgs e)
        {
            string sTankWashCheckYn = string.Empty;

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_TANK_WASH_CHECK_YN window = sender as LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_TANK_WASH_CHECK_YN;

            sTankWashCheckYn = window.TANK_WASH_CHECK_YN.ToString();

            if (sTankWashCheckYn.Equals("Y"))
            {
                WorkOrderChangeWashCheckRun();
            }

            return;
        }

        // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        private string GetElecProcTankWashCheckUseYn()
        {
            string sElecProcTankWashCheckUseYn = string.Empty;

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_PROC_TANK_WASH_CHECK";  // 전극 공정 TANK WASH CHECK
            sCmCode = PROCID;                         // 공정 

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sElecProcTankWashCheckUseYn = "Y";
                }
                else
                {
                    sElecProcTankWashCheckUseYn = "N";
                }

                return sElecProcTankWashCheckUseYn;
            }
            catch (Exception ex)
            {
                sElecProcTankWashCheckUseYn = "N";
                //Util.MessageException(ex);
                return sElecProcTankWashCheckUseYn;
            }
        }

        // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        private bool isTankWashCheck()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRE_WO", typeof(string));
                inTable.Columns.Add("AFTER_WO", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PRE_WO"] = _CURR_WOID;
                newRow["AFTER_WO"] = _AFTER_WOID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = PROCID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRE_SEL_TB_SFC_ELTR_MIXER_TANK_CLEAN_MNGT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
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

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EQPTID) || string.IsNullOrEmpty(PROCID) || string.IsNullOrEmpty(EQPTSEGMENT) || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;

                DataRowView drv = dgWorkOrder.Rows[idx].DataItem as DataRowView;
                if (drv != null)
                {
                    DataRow dr = drv.Row;
                    PRODID = dr["PRODID"].ToString();

                    // 작업지시를 선택취소 하시겠습니까?
                    Util.MessageConfirm("SFU2944", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SelectWorkInProcessStatus(EQPTID, PROCID, EQPTSEGMENT, (table, ex) =>
                            {
                                if (CommonVerify.HasTableRow(table))
                                {
                                    if (table.Rows[0]["WIPSTAT"].GetString() == "PROC")
                                    {
                                        //ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1917"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);  //진행중인 LOT이 있습니다.
                                        Util.MessageValidation("SFU1917");
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetParentSearchConditions();

                // 자동 조회 처리....
                //if (!CanSearch())
                //    return;

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

        #region Method

        #region [BizCall]
        private void GetProcessFPInfo()
        {
            try
            {
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
                        sMsg = "SFU3058";    // 선택 가능한 상태의 작업지시가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
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
                        //if (sPlanDate.Substring(0, 6).Equals(sCalDateYMD.Substring(0, 6)))  // 동일 월인 경우.
                        //{
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
                        //}
                        //else
                        //    sOutMsg = "SFU3443";    // 해당월의 WO만 선택 가능합니다.
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

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="dr">선택 한 작업지시 정보 DataRow</param>
        /// <param name="isSelectFlag">선택 처리:true 선택 취소:fals</param>
        private void SetWorkOrderSelect(DataRow dr, bool isSelectFlag = true)
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(dr["WO_DETL_ID"]) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                //Util.Alert(isSelectFlag ? "작업지시가 변경 되었습니다." : "작업지시가 선택취소 되었습니다.");
                //Util.MessageInfo(isSelectFlag ? "작업지시가 변경 되었습니다." : "작업지시가 선택취소 되었습니다.");
                if (isSelectFlag)
                    Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.
                else
                    Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        public void GetWorkOrder(bool isSelectFlag = true)
        {
            //SetChangeDatePlan(false);

            if (PROCID.Length < 1 || EQPTID.Length < 1 || EQPTSEGMENT.Length < 1)
                return;

            // 일자 설정이 안된경우 RETURN.
            if (dtpDateFrom.SelectedDateTime.Year < 2000 || dtpDateTo.SelectedDateTime.Year < 2000)
                return;

            // Process 정보 조회
            GetProcessFPInfo();

            // 현 작지 정보 조회.
            //string sWODetl = GetEIOInfo();

            string sPrvWODTL = string.Empty;

            if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                if (idx >= 0)
                {
                    sPrvWODTL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

                    //if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                    //    btnSelectCancel.IsEnabled = true;
                }
            }

            // 취소인 경우에는 선택 없애도록..
            if (!isSelectFlag)
                sPrvWODTL = "";

            InitializeGridColumns();

            ClearWorkOrderInfo();
            //ParentDataClear();  // Caller 화면 Data Clear.

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

            //searchResult = GetWorkOrderListEltrAssy();  // W/O 조회 1개 BIZ로 통합 관련 수정.

            if (searchResult == null)
                return;


            //dgWorkOrder.ItemsSource = DataTableConverter.Convert(searchResult);
            Util.GridSetData(dgWorkOrder, searchResult, FrameOperation, true);


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
                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);

                    DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                    // 재조회 처리.
                    ReChecked(idx);

                    PRODID = DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID").ToString();
                }
            }
            else // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
            {
                for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        dgWorkOrder.SelectedIndex = i;
                        PRODID = DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PRODID").ToString();
                        _CURR_WOID = DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "WOID").ToString(); // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
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

            // Summary 조회.
            //SetWorkOrderQtyInfo();
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
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
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
                //DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(_CoatSideType))
                    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

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
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
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
                //DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(_CoatSideType))
                    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

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
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
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
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
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
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                newRow["EQPTID"] = _EqptID;

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
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                newRow["EQPTID"] = _EqptID;

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
                searchCondition["EQPTID"] = EQPTID;

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

        private DataTable GetWorkOrderListEltrAssy()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_ELTR_ASSY();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["PROC_EQPT_FLAG"] = chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked ? "PROC" : "EQPT";

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_ELTR_ASSY", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;

            if (PROCID.Length < 1)
            {
                Util.MessageValidation("SFU1456");      //공정 정보가 없습니다.
                return bRet;
            }

            if (EQPTSEGMENT.Equals("") || EQPTSEGMENT.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return bRet;
            }

            if (EQPTID.Equals("") || EQPTID.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
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

            if (Util.NVC(dtRow["EIO_WO_SEL_STAT"]).Equals("Y"))
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("이미 선택된 작업지시 입니다.", null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageValidation("SFU3061");  //이미 선택된 작업지시 입니다.
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

            // 진행중인 Lot 존재 여부 체크. => PACKAGING 공정 제외 2017-04-05 
            ////if (_ProcID.Equals(Process.NOTCHING) ||
            ////    _ProcID.Equals(Process.LAMINATION) ||
            ////    _ProcID.Equals(Process.STACKING_FOLDING) ||
            ////    _ProcID.Equals(Process.PACKAGING))

            if (_ProcID.Equals(Process.LAMINATION) ||
                _ProcID.Equals(Process.STACKING_FOLDING))
            {
                if (SelectProcStateLot())
                {
                    Util.MessageValidation("SFU1917");
                    return bRet;
                }
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
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

            // W/O Rolling에 따라 자동 Over Completion을 위하여 하기 Validation 적용 [2017-09-18]
            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr = dt?.Select("EIO_WO_SEL_STAT = 'Y'");
            if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("MODLID"))
            {
                foreach (DataRow drTmp in dr)
                {
                    if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && Util.NVC(dtRow["MODLID"]).Equals(Util.NVC(drTmp["MODLID"])))
                    {
                        Util.MessageValidation("SFU4117"); // 동일한 모델, WO Type의 WO가 이미 선택되어 있습니다.
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

            // CSR : C20220622-000589 - Coater Process Progress(New)
            if (string.Equals(_ProcID, Process.COATING) && isSelectFlag)
            {
                _OldProdID = string.Empty;

                // 이전 선택한 제품코드
                DataTable dtOld = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
                DataRow[] drOld = dtOld?.Select("EIO_WO_SEL_STAT = 'Y'");

                if (drOld?.Length > 0)
                {
                    foreach (DataRow drTmp in drOld)
                    {
                        _OldProdID = Util.NVC(drTmp["PRODID"]);
                    }
                }

                // 신규 선택한 제품코드 및 탈착 실행
                if (!string.IsNullOrEmpty(EQPTID))
                {
                    _NewProdID = Util.NVC(dr["PRODID"]);
                    GetSlurryLotList(EQPTID, PROCID);
                }
            }

            SetWorkOrderSelect(dr, isSelectFlag);

            GetWorkOrder(isSelectFlag);

            if (_UCParent != null)
            {
                ParentDataClear();  // Caller 화면 Data Clear.

                SearchParentAll();  // Caller 화면 Data 모두 조회
            }
            else if ( _UCElec != null)
            {
                _UCElec.REFRESH = true;
            }
            _CURR_WOID = dr["WOID"].ToString(); // [E20240729-000085] [소형_전극]mixer wash clean improvement mixer清洗改善
        }
        
        /// <summary>
        /// Main 화면 실적 실적 조회 Call
        /// </summary>
        /// <param name="drSelWorkOrder">선택한 작지 정보</param>
        private void SearchParentProductInfo(DataRow dataRow)
        {
            //if (_UCParent == null)
            //    return;

            //if (dataRow != null)
            //{
            //    try
            //    {   
            //        Type type = _UCParent.GetType();
            //        MethodInfo methodInfo = type.GetMethod("GetProductLot");
            //        ParameterInfo[] parameters = methodInfo.GetParameters();

            //        object[] parameterArrys = new object[parameters.Length];
            //        parameterArrys[0] = dataRow;
            //        if(parameterArrys.Length > 1)
            //            parameterArrys[1] = true;  //  이전 선택 Lot 선택 여부. (true:이전선택 Lot 자동 선택, false:첫번째 PROC 상태 Lot 선택)

            //        methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            //    }
            //    catch(Exception ex)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //    }
            //}
        }

        private void SearchParentAll()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetAllInfoFromChild");
                if (methodInfo == null)
                    return;
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                
                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                // 전극 조립 모두 사용 하므로 없는 공정 존재. 하여 exception  처리 안함.
            }
        }

        /// <summary>
        /// Main 화면 Data Clear 처리
        /// </summary>
        private void ParentDataClear()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("ClearControls");
                methodInfo.Invoke(_UCParent, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetParentSearchConditions()
        {
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
            //InitializeWorkorderQuantityInfo();
            //txtWOID.Text = "";
        }

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

                // 선택 작지 수량 설정
                //SetWorkOrderQtyInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);

                // 실적 조회 호출..
                //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                //SearchParentProductInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);
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

        private void dgWorkOrder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                        (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                        (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt &&
                        (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (dg == null) return;
                        
                        C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                        if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                        switch (Convert.ToString(currCell.Column.Name))
                        {
                            case "WOID":

                                LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM wndBOM = new LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM();
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
                    }
                    else if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                    {
                        TextBlock textBlock = e.OriginalSource as TextBlock;

                        if (textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
                        {
                            DataGridCellPresenter cellPresenter = textBlock.Parent as DataGridCellPresenter;

                            if (cellPresenter != null && string.Equals(cellPresenter.Column.Name, "PRJT_NAME"))
                            {
                                LGC.GMES.MES.CMM001.CMM_ELEC_PRDT_GPLM window = new CMM_ELEC_PRDT_GPLM();
                                window.FrameOperation = FrameOperation;
                                if (window != null)
                                {
                                    object[] Parameters = new object[2];
                                    Parameters[0] = DataTableConverter.GetValue(cellPresenter.Row.DataItem, "PRODID");

                                    if (string.Equals(_ProcID, Process.COATING) || string.Equals(_ProcID, Process.SRS_COATING))
                                        Parameters[1] = Gplm_Process_Type.COATING;
                                    else
                                        Parameters[1] = Gplm_Process_Type.RTS;

                                    C1WindowExtension.SetParameters(window, Parameters);

                                    this.Dispatcher.BeginInvoke(new Action(() => window.ShowModal()));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void wndBOM_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM window = sender as LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
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
                        Util.MessageException(ex);
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

        private bool SelectProcStateLot()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = EQPTID;
                inData["PROCID"] = PROCID;
                inData["EQSGID"] = EQPTSEGMENT;
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

        #region W/O 변경시 SLURRY 자동 탈착 
        private void GetSlurryLotList(string sEqptID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_WIP_MTRL_AUTO", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        string sPstn = dtMain.Rows[i]["EQPT_MOUNT_PSTN_ID"].ToString();
                        string sProd = dtMain.Rows[i]["PRODID"].ToString();
                        string sINPUT_LOTID = dtMain.Rows[i]["INPUT_LOTID"].ToString();

                        if (string.IsNullOrEmpty(_OldProdID))
                            _OldProdID = sProd;

                        // 선택한 제품코드와 슬러리 제품코드와 상이시 진행
                        if (_OldProdID != _NewProdID && !string.IsNullOrEmpty(sINPUT_LOTID) && !string.IsNullOrEmpty(_OldProdID))
                        {
                            SetPreSelectedMixerDesorption(sPstn, sEqptID, sProcID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPreSelectedMixerDesorption(string sPstn, string sEqptID, string sProcID)
        {
            // SET
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = "UI";
            Indata["EQPTID"] = sEqptID;
            Indata["EQPT_MOUNT_PSTN_ID"] = sPstn;
            Indata["PROCID"] = sProcID;
            Indata["USERID"] = LoginInfo.USERID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("BR_PRD_REG_UNMOUNT_AUTO_SLURRY_CT", "INDATA", null, IndataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
            });
        }
        #endregion
    }
}
 