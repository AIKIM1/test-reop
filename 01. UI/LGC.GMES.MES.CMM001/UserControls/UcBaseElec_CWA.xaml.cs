/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 전극 공정진척(ESWA)
--------------------------------------------------------------------------------------
 [Change History]
                    : Initial Created.
 2023.05.09  정재홍 : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
 2023.05.11  김도형 : E20230127-000272 - 코터 공정진척 2번컷 특이사항 자동 입력 건
 2023.07.25  김태우 : DAM 믹서(E0430) 추가 
 2023.08.31  신광희 : 롤프레스 공정 수불 미적용 롤맵 설비 인 경우 HOLD 조건 부합 시 롤맵 홀드 팝업 호출 적용
 2023.09.05  임재형 : 롤맵 홀드 조건 및 노트 추가 
 2023.09.07  김도형 : [E20230807-000061] speical work-LOT MERGE improvement
 2023.09.14  양영재 : [E20230829-000685] Adding Fast track function to the Roll Press Process Progress Menu
 2023.09.16  김도형 : [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization 
 2024.03.14  김영택 : 선분산믹서, 믹서, BS, CMC, 롤프레스, 슬리터 공정 실적확정시 물청/Loss/불량 초과 사유 입력 기능 추가 
 2024.03.18  김영택 : 절연액 MIXING 공정 실적확정시 물청/Loss/불량 초과 사유 입력 기능 추가 
 2024.05.30  백상우 : [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
 2024.07.25  이원열 : [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제) 
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DateTimeEditors;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Models;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBoxIcon = LGC.GMES.MES.ControlsLibrary.MessageBoxIcon;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Data;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcBaseElec_CWA.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcBaseElec_CWA
    {
        #region Properties

        string procId;
        //2021-03-10 오화백 QA 샘플링 검사여부 체크
        string Qa_Insp_Trgt_Flag = string.Empty;


        C1DataGrid dgLotInfo;

        string _RemarkHoldUseFlag = "N"; // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization

        public string PROCID
        {
            get
            {
                return procId;
            }
            set
            {
                procId = value;

                if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING) ))
                {
                    //_RemarkHoldUseFlag = GetProcRemarkHoldUseFlag(); // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
                    _RemarkHoldUseFlag = "Y";  // 비고 HOLD 사용여부 (REMARK_HOLD_USE_FLAG) 미적용 - 전법인 일괄 적용(2023.09.22)
                }

                SetComboBox();
                SetGrids();
                SetTabItems();
                SetResultDetail();
                SetTextBox();
                SetEvents();
                SetStatus();
                SetSingleCoaterControl();
                SetVisible();
                WipReasonGridColumnAdd();
                SetIdentInfo();
                SetRollMapEquipment();

                if (procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.MIXING))
                {
                    (grdCommand.Children[0] as UcCommand).btnBarcodeLabel.Visibility = Visibility.Collapsed;
                    (grdCommand.Children[0] as UcCommand).btnHistoryCard.Visibility = Visibility.Collapsed;
                    (grdCommand.Children[0] as UcCommand).btnMixConfirm.Visibility = Visibility.Visible;
                    CHECK_WAIT.Visibility = Visibility.Collapsed;
                    grdWorkOrder.Children.Clear();
                    grdWorkOrder.Children.Add(new UC_WORKORDER_MX_CWA());
                }

                if (PROCID.Equals(Process.BS) || PROCID.Equals(Process.CMC) || PROCID.Equals(Process.InsulationMixing))
                {
                    (grdCommand.Children[0] as UcCommand).btnBarcodeLabel.Visibility = Visibility.Collapsed;
                    (grdCommand.Children[0] as UcCommand).btnHistoryCard.Visibility = Visibility.Collapsed;
                    (grdCommand.Children[0] as UcCommand).btnMixConfirm.Visibility = Visibility.Visible;
                    CHECK_WAIT.Visibility = Visibility.Collapsed;
                    grdWorkOrder.Children.Clear();
                    grdWorkOrder.Children.Add(new UC_WORKORDER_CWA());
                }

                if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING))
                {
                    (grdCommand.Children[0] as UcCommand).btnReservation.Visibility = Visibility.Visible;
                }

                if (string.Equals(procId, Process.COATING))
                {
                    (grdCommand.Children[0] as UcCommand).btnCleanLot.Visibility = Visibility.Visible;
                    (grdCommand.Children[0] as UcCommand).btnFoil.Visibility = Visibility.Visible;
                    (grdCommand.Children[0] as UcCommand).btnLogisStat.Visibility = Visibility.Visible;

                    if (string.Equals(LoginInfo.CFG_AREA_ID, "EB") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                        (grdCommand.Children[0] as UcCommand).btnMixConfirm.Visibility = Visibility.Visible;
                }

                if (string.Equals(procId, Process.HALF_SLITTING) || string.Equals(procId, Process.ROLL_PRESSING))   /* HALF SLITTING, ROLL PRESS CUT메뉴 추가 */
                    (grdCommand.Children[0] as UcCommand).btnCut.Visibility = Visibility.Visible;


                // 슬리터 공정 Port별 Skid Type 설정 팝업추가
                if (string.Equals(procId, Process.SLITTING))
                {
                    // 적용대상 동은 CNB 전극1동, CWA 전극2동
                    if (LoginInfo.CFG_AREA_ID.Equals("EC") || LoginInfo.CFG_AREA_ID.Equals("ED"))
                    {
                        (grdCommand.Children[0] as UcCommand).btnSkidTypeSettingByPort.Visibility = Visibility.Visible;
                    }

                    //무지부방향, 슬리터에도 보이도록 설정. 21.03.05
                    (grdCommand.Children[0] as UcCommand).btnWorkHalfSlitSide.Visibility = Visibility.Visible;
                }

                // R/P공정 이동 기능 추가 [2017-08-04]
                if (string.Equals(procId, Process.ROLL_PRESSING))
                {
                    (grdCommand.Children[0] as UcCommand).btnProcReturn.Visibility = Visibility.Visible;
                    (grdCommand.Children[0] as UcCommand).btnWorkHalfSlitSide.Visibility = Visibility.Visible;

                    //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
                    if (GetCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID))
                        (grdCommand.Children[0] as UcCommand).btnReturnCondition.Visibility = Visibility.Visible;

                    if (string.Equals(LoginInfo.CFG_AREA_ID, "EB") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                        (grdCommand.Children[0] as UcCommand).btnMixConfirm.Visibility = Visibility.Visible;
                }


                // 하프슬리터도 변경 못하게 추가 [2017-05-23]
                // 해당 건 CWA요청으로 주석 [2018-12-20]
                //if (string.Equals(procId, Process.HALF_SLITTING))                    
                //    txtInputQty.IsReadOnly = true;

                if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)) && (string.Equals(LoginInfo.CFG_AREA_ID, "EB")))
                    (grdCommand.Children[0] as UcCommand).btnSamplingProdT1.Visibility = Visibility.Visible;

                if (string.Equals(procId, Process.HALF_SLITTING))   /* HALF SLITTING 하프슬리터 이동 메뉴 추가 */
                    (grdCommand.Children[0] as UcCommand).btnMovetoHalf.Visibility = Visibility.Visible;

                if (procId.Equals(Process.COATING))
                    CHECK_WAIT.Visibility = Visibility.Collapsed;

                CHECK_WOPRODUCT.Visibility = Visibility.Collapsed;

                // 투입량의 초과입력률 체크하기 위하여 추가
                string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
                inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));

                // 변환률 기본값 설정
                convRate = 1;

                // 롤프레스 평균값
                if (!string.Equals(procId, Process.ROLL_PRESSING))
                    (grdDataCollect.Children[0] as UcDataCollect).dgQuality.BottomRows.Clear();

                // 슬리터 기본 설정 (소형 체크로직을 Setting에서 불러와서 사용)
                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                    if (LoginInfo.CFG_ETC != null && LoginInfo.CFG_ETC.Rows.Count > 0)
                        (grdDataCollect.Children[0] as UcDataCollect).chkSum.IsChecked = Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_SMALLTYPE]);

                // 코터 공정인 경우 슬러리 정보 팝업 기능 추가.
                if (procId.Equals(Process.COATING))
                    (grdCommand.Children[0] as UcCommand).btnMixerTankInfo.Visibility = Visibility.Visible;

                //2020-07-10 오화백 (롤프레스, 슬리터, 하프슬리터 고객인증그룹조회 팝업 추가) CWA 전극2동만 적용

                if (string.Equals(LoginInfo.CFG_AREA_ID, "ED") && (procId.Equals(Process.ROLL_PRESSING) || procId.Equals(Process.SLITTING) || procId.Equals(Process.HALF_SLITTING)))
                {
                    (grdCommand.Children[0] as UcCommand).btnCustomer.Visibility = Visibility.Visible;
                }

                // 믹스 실적 확정 메시지 Timer CSR[C20201014-000233]
                if (string.Equals(procId, Process.MIXING))
                {
                    Mix_Timer.Interval = TimeSpan.FromMinutes(5d); //5분 간격으로 설정
                    Mix_Timer.Tick += MixTimer_Tick;
                }
            }
        }

        private void WipReasonGridColumnAdd()
        {
            // 동적으로 AREA_RESN_CLSS_NAME1,2,3 컬럼 생성
            WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "AREA_RESN_CLSS_NAME1",
                Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME1"),
                Binding = new System.Windows.Data.Binding()
                {
                    Path = new PropertyPath("AREA_RESN_CLSS_NAME1"),
                    Mode = BindingMode.TwoWay
                },
                IsReadOnly = true,
                Visibility = Visibility.Collapsed
            });

            WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "AREA_RESN_CLSS_NAME2",
                Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME2"),
                Binding = new System.Windows.Data.Binding()
                {
                    Path = new PropertyPath("AREA_RESN_CLSS_NAME2"),
                    Mode = BindingMode.TwoWay
                },
                IsReadOnly = true,
                Visibility = Visibility.Collapsed
            });

            WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "AREA_RESN_CLSS_NAME3",
                Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME3"),
                Binding = new System.Windows.Data.Binding()
                {
                    Path = new PropertyPath("AREA_RESN_CLSS_NAME3"),
                    Mode = BindingMode.TwoWay
                },
                IsReadOnly = true,
                Visibility = Visibility.Collapsed
            });

            WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME1"].DisplayIndex = 7;
            WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME2"].DisplayIndex = 8;
            WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME3"].DisplayIndex = 9;

        }
        bool isRefresh;
        public bool REFRESH
        {
            get
            {
                return isRefresh;
            }
            set
            {
                isRefresh = value;

                if (isRefresh)
                {
                    if (isConfirm && LOTINFO_GRID.GetRowCount() > 0)
                    {
                        txtEndLotId.Text = Util.NVC((LOTINFO_GRID.ItemsSource as DataView).Table.Rows[0]["LOTID"]);
                        txtEndLotId.Tag = _CUTID;

                        // INOUT LOT이 수량 연계되는 문제가 발생할 여지가 있어서 저장된 수량 초기화 [2017-04-22]
                        SetInitProductQty();

                        // W/O 자동 변경을 위한 BIZ 호출 [MO등록해서 쓰는것들은 이제 사용 안함 2017-10-13]
                        string confirmMsg = string.Empty;   // 실적확정 INFORM MESSAGE
                        if (string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.SRS_MIXING) ||
                            string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING))
                            confirmMsg = IsAutoChangeWorkOrder();

                        confirmMsg += string.IsNullOrEmpty(confirmMsg) ? MessageDic.Instance.GetMessage("SFU1275") : ("\n\n" + MessageDic.Instance.GetMessage("SFU1275"));

                        #region [샘플링 출하거래처 추가]
                        // SAMPLING용 발행 매수 추가
                        int iSamplingCount;

                        DataTable LabelDT = new DataTable();
                        LabelDT = (LOTINFO_GRID.ItemsSource as DataView).Table;

                        if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                        {
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                foreach (DataRow _iRow in LabelDT.Rows)
                                {
                                    iSamplingCount = 0;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), procId, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            foreach (DataRow _iRow in LabelDT.Rows)
                                Util.UpdatePrintExecCount(Util.NVC(_iRow["LOTID"]), procId);


                        }
                        #endregion                        

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(confirmMsg, null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                if (!string.Equals(procId, Process.PRE_MIXING))
                                {
                                    if (string.Equals(LoginInfo.CFG_CARD_POPUP, "Y") || string.Equals(LoginInfo.CFG_CARD_AUTO, "Y"))
                                        OnClickHistoryCard(null, null);
                                }
                            }
                        });
                        isConfirm = false;
                    }
                    if (CHECK_WAIT.Visibility == Visibility.Collapsed || WIPSTATUS.Equals(Wip_State.WAIT))
                        CHECK_RUN.IsChecked = true;

                    Thread.Sleep(500);

                    if (LARGELOT_GRID.Visibility == Visibility.Visible)
                    {
                        GetLargeLot();

                        if (LARGELOT_GRID.Rows.Count > 0)
                        {
                            DataTableConverter.SetValue(LARGELOT_GRID.Rows[0].DataItem, "CHK", true);
                            LARGELOT_GRID.SelectedIndex = 0;
                            LARGELOTID = Util.NVC(DataTableConverter.GetValue(LARGELOT_GRID.Rows[0].DataItem, "LOTID_LARGE"));
                        }
                        else
                        {
                            // 대lot Clear 2016.12.06 **********
                            LARGELOTID = string.Empty;
                            //**********************************
                        }
                    }

                    if (LARGELOT_GRID.Visibility == Visibility.Visible && LARGELOT_GRID.Rows.Count > 0 && LARGELOT_GRID.SelectedIndex > -1)
                        GetProductLot(LARGELOT_GRID.Rows[LARGELOT_GRID.SelectedIndex].DataItem as DataRowView);
                    else if (LARGELOT_GRID.Visibility != Visibility.Visible)
                        GetProductLot();

                    // SHIFT_CODE, SHIFT_USER 자동 SET
                    GetWrkShftUser();

                    // COATER공정에서 CORE, SLURRY 조회, SRS코터에서 BATCH ID 자동 조회 및 SETUP
                    if (string.Equals(procId, Process.COATING))
                    {
                        if (isSingleCoater)
                            GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));
                        else
                            GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), "");
                    }
                    else if (string.Equals(procId, Process.SRS_COATING))
                    {
                        GetSlurry();
                    }
                }
            }
        }

        bool isSingleCoater;
        public bool SINGLECOATER
        {
            get
            {
                return isSingleCoater;
            }
            set
            {
                isSingleCoater = value;
            }
        }
        public string LOTID { get; set; }
        public string LARGELOTID { get; set; }
        public string PRLOTID { get; set; }
        public string WO_DETL_ID { get; set; }
        public Button STARTBUTTON { get; set; }
        public Button SEARCHBUTTON { get; set; }
        public Button INVOICEBUTTON { get; set; }
        public string WIPSTATUS { get; set; }
        public C1ComboBox EQUIPMENT_COMBO { get; set; }
        public C1ComboBox EQUIPMENTSEGMENT_COMBO { get; set; }
        public C1ComboBox COATTYPE_COMBO { get; set; }
        public C1ComboBox LANENUM_COMBO { get; set; }
        public C1ComboBox COLOR_COMBO { get; set; }
        public CheckBox CHECK_WAIT { get; set; }
        public CheckBox CHECK_RUN { get; set; }
        public CheckBox CHECK_END { get; set; }
        public CheckBox CHECK_WOPRODUCT { get; set; }
        public C1DataGrid LARGELOT_GRID { get; set; }
        public C1DataGrid PRODLOT_GRID { get; set; }
        public C1DataGrid LOTINFO_GRID
        {
            get { return dgLotInfo; }
            set { dgLotInfo = value; }
        }
        public C1DataGrid WIPREASON_GRID { get; set; }
        public C1DataGrid WIPREASON2_GRID { get; set; }
        public C1DataGrid QUALITY_GRID { get; set; }
        public C1DataGrid QUALITY2_GRID { get; set; }
        public C1DataGrid INPUTMTRL_GRID { get; set; }
        public C1DataGrid SLURRY_GRID { get; set; }
        public C1DataGrid REMARK_GRID { get; set; }
        public C1DataGrid REMARK_HIST_GRID { get; set; }
        public C1DataGrid COLORTAG_GRID { get; set; }
        public C1DataGrid COLOR_GRID { get; set; }
        public C1DataGrid DEFECT_TAG_GRID { get; set; }
        public C1DataGrid MERGE_GRID { get; set; }
        public C1DataGrid MERGE2_GRID { get; set; }
        public C1DataGrid WORKORDER_GRID { get; set; }
        public C1DataGrid COTTON_GRID { get; set; }
        public string _LDR_LOT_IDENT_BAS_CODE { get; set; }
        public string _UNLDR_LOT_IDENT_BAS_CODE { get; set; }

        #region [POSTACTION]
        public C1DataGrid POSTHOLD_GRID { get; set; }
        #endregion

        #region [Cancel Delete Lot]
        public Button CANCELDELETE { get; set; }
        #endregion

        #region Coater Slurry
        public LGCDatePicker DTPFROM { get; set; }
        public LGCDatePicker DTPTO { get; set; }
        public C1DataGrid SLURRY_INPUT_GRID { get; set; }
        #endregion
        //[CR-128] MIXER Consume Material (2017.01.18)
        public C1DataGrid INPUTMTRLSUMMARY_GRID { get; set; }
        public DataTable WIPCOLORLEGEND { get; private set; }

        private const string ITEM_CODE_LEN_LACK = "LENGTH_LACK";
        private const string ITEM_CODE_LEN_EXCEED = "LENGTH_EXCEED";

        Util _Util = new Util();

        string _ValueWOID = string.Empty;
        string _LargeLOTID = string.Empty;
        string _LOTID = string.Empty;
        string _EQPTID = string.Empty;
        string _LOTIDPR = string.Empty;
        string _CUTID = string.Empty;
        string _WIPSTAT = string.Empty;
        string _INPUTQTY = string.Empty;
        string _OUTPUTQTY = string.Empty;
        string _CTRLQTY = string.Empty;
        string _GOODQTY = string.Empty;
        string _LOSSQTY = string.Empty;
        string _CHARGEQTY = string.Empty;
        string _WORKORDER = string.Empty;
        string _WORKDATE = string.Empty;
        string _VERSION = string.Empty;
        string _PRODID = string.Empty;
        string _WIPDTTM_ST = string.Empty;
        string _WIPDTTM_ED = string.Empty;
        string _REMARK = string.Empty;
        string _CONFIRMUSER = string.Empty;
        string _FINALCUT = string.Empty;
        string _WIPSTAT_NAME = string.Empty;
        string _LANEQTY = string.Empty;
        string sEQPTID = string.Empty;
        string cut = string.Empty;
        string _PTNQTY = string.Empty;
        string _WIPSEQ = string.Empty;
        string _CLCTSEQ = string.Empty;
        string _WRKDTTM_ST = string.Empty;
        string _WRKDTTM_ED = string.Empty;
        string _HOLDYN_CHK = string.Empty;
        string _CSTID = string.Empty;
        string _CSTID_CHK = string.Empty;
        string _LABEL_PASS_HOLD_FLAG = string.Empty;

        string _ProNameMerge = string.Empty;              

        private string _FastTrackLot = string.Empty;      // 2023-09-12 [E20230829-000685] Fast Track 추가

        DataTable _dtDEFECTLANENOT = null; // 전수불량 Lane
        DataRow[] _DEFECTLANELOT = null;  // 전수불량 Lane

        int dgLVIndex1 = 0;
        int dgLVIndex2 = 0;
        int dgLVIndex3 = 0;

        TextBox txtVersion;
        C1NumericBox txtLaneQty = new C1NumericBox();
        #region #전수불량 Lane 등록
        C1NumericBox txtCurLaneQty = new C1NumericBox();
        #endregion
        C1NumericBox txtLanePatternQty = new C1NumericBox();
        C1NumericBox txtGoodQty;
        C1NumericBox txtLossQty;
        TextBox txtShift;
        TextBox txtWorker;
        TextBox txtShiftDateTime;
        TextBox txtShiftStartTime;
        TextBox txtShiftEndTime;
        TextBox txtStartDateTime;
        TextBox txtEndDateTime;

        C1NumericBox txtWorkTime;
        TextBox txtWorkDate;
        TextBox txtWipNote;
        TextBox txtUnit;
        TextBox txtMergeInputLot;
        C1NumericBox txtInputQty;
        C1NumericBox txtParentQty;
        C1NumericBox txtRemainQty;
        C1NumericBox txtBeadMillCount;
        CheckBox chkFinalCut;
        CheckBox chkExtraPress;
        CheckBox chkFastTrack; // 2023-09-12 [E20230829-000685] Fast Track 추가
        TextBox txtEndLotId;
        C1NumericBox txtSrs1Qty;
        C1NumericBox txtSrs2Qty;
        C1NumericBox txtSrs3Qty;
        C1ComboBox cboPet;
        TextBox txtBatchId;
        TextBox txtTank;

        TextBox txtCore1;
        TextBox txtCore2;
        TextBox txtSlurry1;
        TextBox txtSlurry2;
        Button btnMtrlChange;
        Button btnMtrlChange2;

        CheckBox chkInOut;

        DataTable dtWoAll;
        DataTable dtWoCheck;
        private DataTable dtWipReasonBak;       // WIPREASONCOLLECT의 이전값을 보관하기 위한 DataTable(C20190404_67447) [2019-04-11]

        public System.Windows.Threading.DispatcherTimer Mix_Timer = new System.Windows.Threading.DispatcherTimer();  //CSR[C20201014-000233] 믹스 공정 Timer 메시지 팝업

        string eqptMountPositionCode;
        decimal inputOverrate;
        decimal exceedLengthQty;
        decimal convRate;

        GridLength ExpandFrame;

        DataSet inDataSet;
        DataTable procResnDt;

        bool isChangeWipReason = false;
        bool isChangeQuality = false;
        bool isChangeMaterial = false;
        bool isChangeColorTag = false;
        bool isChagneDefectTag = false;
        bool isChangeRemark = false;
        bool isCoaterAfterProcess = false;
        bool isConfirm = false;
        bool isChangeInputFocus = false;
        bool isDupplicatePopup = false;
        bool isChangeCotton = false;
        bool isChangePostAction = false;
        bool isResnCountUse = false;
        bool isDefectLevel = false;

        //2020-04-21 믹서 투입수량을 설비 수량으로. BUT 생산수량을 입력을 했을 경우는 적용.
        bool _isProdQtyInput = false;

        //2020.11.27 Timer
        bool isTimerCheck = false;
        bool isTimerPopup = false;
        bool isTimerTabCk = false;

        // RollMap 관련 변수 선언
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;  // 동별 공정별 롤맵 실적 연계 여부
        private bool _isOriginRollMapEquipment = false;

        //허용비율 초과 사유 처리 관련 변수
        DataTable dtPermitRateReturn = new DataTable();
        string permitRateUerReturn = string.Empty;
        string permitRateDeptReturn = string.Empty;

        // 코터 롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////
        Dictionary<string, string> holdLotClassCode = new Dictionary<string, string>();

        // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        private bool _isELEC_MTRL_LOT_VALID_YN = false;

        #endregion Properties


        #region Constructor

        public UcBaseElec_CWA()
        {
            InitializeComponent();
            InitControls();
        }

        private void SetIdentInfo()
        {
            try
            {
                if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 0 || EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    _LDR_LOT_IDENT_BAS_CODE = string.Empty;
                    _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

                    (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    (grdResult.Children[0] as UcResult).dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    (grdResult.Children[0] as UcResult).dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = PROCID;
                row["EQSGID"] = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _UNLDR_LOT_IDENT_BAS_CODE = result.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();

                    switch (_LDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                            (grdResult.Children[0] as UcResult).dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                            break;
                        default:
                            (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                            (grdResult.Children[0] as UcResult).dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                            break;
                    }

                    switch (_UNLDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                            (grdResult.Children[0] as UcResult).dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Visible;

                            if (string.Equals(procId, Process.COATING))
                            {
                                (grdResult.Children[0] as UcResult).dgLotInfo.Columns["OUT_CSTID"].IsReadOnly = false;
                                (grdResult.Children[0] as UcResult).btnSaveCarrier.Visibility = Visibility.Visible;
                            }

                            break;
                        default:
                            (grdProdLot.Children[0] as UcProdLot).dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
                            (grdResult.Children[0] as UcResult).dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
                            break;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }


        #endregion Constructor


        #region Events

        protected virtual void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (isCoaterAfterProcess == false)
            {
                e.Handled = false;
                CHECK_RUN.IsChecked = true;
                CHECK_END.IsChecked = true;
                return;
            }

            CheckBox cb = sender as CheckBox;

            switch (cb.Name)
            {
                case "chkWait":
                    if (cb.IsChecked == true)
                    {
                        CHECK_WOPRODUCT.Visibility = Visibility.Visible;

                        CHECK_RUN.IsChecked = false;
                        CHECK_END.IsChecked = false;
                    }
                    else
                    {
                        CHECK_WOPRODUCT.Visibility = Visibility.Collapsed;

                        CHECK_RUN.IsChecked = true;
                        CHECK_END.IsChecked = true;
                    }
                    break;

                case "chkRun":
                case "chkEqpEnd":
                    if (cb.IsChecked == true)
                    {
                        CHECK_WOPRODUCT.Visibility = Visibility.Collapsed;

                        CHECK_WAIT.IsChecked = false;
                        CHECK_RUN.IsChecked = true;
                        CHECK_END.IsChecked = true;
                    }
                    else
                    {
                        CHECK_WOPRODUCT.Visibility = Visibility.Visible;

                        CHECK_WAIT.IsChecked = true;
                        CHECK_RUN.IsChecked = false;
                        CHECK_END.IsChecked = false;
                    }
                    break;

                case "chkWoProduct":
                    if (cb.IsChecked == true)
                    {
                        if (LARGELOT_GRID.Visibility == Visibility.Visible)
                        {
                            if (LARGELOT_GRID.Rows.Count < 1)
                                return;
                        }
                        else
                        {
                            if (PRODLOT_GRID.Rows.Count < 1)
                                return;
                        }

                        try
                        {
                            string prodId = procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing) || procId.Equals(Process.MIXING) ? (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA).PRODID : (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).PRODID;

                            if (LARGELOT_GRID.Visibility == Visibility.Visible)
                            {
                                dtWoAll = (LARGELOT_GRID.ItemsSource as DataView).Table;
                                dtWoCheck = (LARGELOT_GRID.ItemsSource as DataView).Table.Select("PRODID='" + prodId + "'").CopyToDataTable();
                                Util.GridSetData(LARGELOT_GRID, dtWoCheck, FrameOperation, true);
                            }
                            else
                            {
                                dtWoAll = (PRODLOT_GRID.ItemsSource as DataView).Table;
                                dtWoCheck = (PRODLOT_GRID.ItemsSource as DataView).Table.Select("PRODID='" + prodId + "'").CopyToDataTable();
                                Util.GridSetData(PRODLOT_GRID, dtWoCheck, FrameOperation, true);
                            }
                        }
                        catch
                        {
                            if (LARGELOT_GRID.Visibility == Visibility.Visible)
                                LARGELOT_GRID.ItemsSource = null;
                            else
                                PRODLOT_GRID.ItemsSource = null;

                            return;
                        }
                    }
                    else
                    {
                        if (dtWoAll != null)
                        {
                            if (LARGELOT_GRID.Visibility == Visibility.Visible)
                                Util.GridSetData(LARGELOT_GRID, dtWoAll, FrameOperation, true);
                            else
                                Util.GridSetData(PRODLOT_GRID, dtWoAll, FrameOperation, true);
                        }
                    }
                    break;
            }

            if (!cb.Name.Equals("chkWoProduct"))
            {
                SetStatus();

                if (string.Equals(cb.Name, "chkEqpEnd"))    // PROC, EQPT_END가 동시에 체크되기 떄문에 하나만 체크하도록 변경
                    return;

                if (((string.Equals(WIPSTATUS, Wip_State.WAIT) || string.Equals(WIPSTATUS, Wip_State.PROC + "," + Wip_State.EQPT_END)) && cb.IsChecked == true))
                {
                    OnClickSearch(SEARCHBUTTON, null);
                }
            }

            if (cb.Name.Equals("chkWait"))
                CHECK_WOPRODUCT.IsChecked = true;
        }

        private void SetExtraPress()
        {
            try
            {
                int roll_count; //압연 횟수
                int roll_Seq;   //압연 차수

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = _LOTID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLPRESS_COUNT", "INDATA", "RSLTDT", dt);// 압연 횟수 차수 가져오기

                if (result.Rows.Count > 0)
                {
                    roll_count = Int32.Parse(result.Rows[0]["ROLLPRESS_COUNT"].ToString().Equals("") ? 0.ToString() : result.Rows[0]["ROLLPRESS_COUNT"].ToString());
                    roll_Seq = Int32.Parse(result.Rows[0]["ROLLPRESS_SEQNO"].ToString().Equals("") ? 0.ToString() : result.Rows[0]["ROLLPRESS_SEQNO"].ToString());

                    if (roll_count <= roll_Seq)
                    {
                        chkExtraPress.IsEnabled = true;
                        chkExtraPress.IsChecked = false;
                    }
                    else
                    {
                        chkExtraPress.IsEnabled = false;
                        chkExtraPress.IsChecked = true;
                    }
                }
                else
                {
                    roll_count = 0;
                    roll_Seq = 0;

                    chkExtraPress.IsEnabled = true;
                    chkExtraPress.IsChecked = false;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 2023-09-12 [E20230829-000685] Fast Track 추가
        private void chkFastTrack_Click(object sender, RoutedEventArgs e)
        {
            if (chkFastTrack.IsChecked == true)
            {
                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7354", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(true);
                        chkFastTrack.IsChecked = true;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = false;
                    }

                });
            }
            else
            {

                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7355", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(false);
                        chkFastTrack.IsChecked = false;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = true;
                    }


                });
            }
        }

        ///2023-09-12  FastTrack 설정
        private void SetFastTrace(bool fasttrackFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FAST_TRACK_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LOTID"] = _FastTrackLot;
                if (fasttrackFlag == true)
                {
                    dr["FAST_TRACK_FLAG"] = "Y";
                }
                else
                {
                    dr["FAST_TRACK_FLAG"] = string.Empty;
                }
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_FAST_TRACK_LOT", "INDATA", null, dtRqst);

                if (fasttrackFlag)
                {
                    Util.MessageInfo("SFU1518");
                }
                else
                {
                    Util.MessageInfo("SFU1937");
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        void SetStatus()
        {
            var status = new List<string>();

            if (CHECK_WAIT.IsChecked == true)
                status.Add(CHECK_WAIT.Tag.ToString());

            if (CHECK_RUN.IsChecked == true)
                status.Add(CHECK_RUN.Tag.ToString());

            if (CHECK_END.IsChecked == true)
                status.Add(CHECK_END.Tag.ToString());

            WIPSTATUS = string.Join(",", status);
        }

        protected virtual void OnClickSearch(object sender, RoutedEventArgs e)
        {

            if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(WIPSTATUS))
            {
                Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                return;
            }

            InitClear();
            InitWorkOrderCheck();

            //C20210527-000015, ESWA Mixing 공정 Work Order 자동 변경 로직 적용 요청의 건
            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
                Util.gridClear(((UC_WORKORDER_CWA)grdWorkOrder.Children[0]).dgWorkOrder);
            else
                Util.gridClear(((UC_WORKORDER_MX_CWA)grdWorkOrder.Children[0]).dgWorkOrder);

            GetWorkOrder();

            if (LARGELOT_GRID.Visibility == Visibility.Visible)
                GetLargeLot();
            else
                GetProductLot();

            // WAIT 상태 일때는 수량 입력 금지
            if (string.Equals(WIPSTATUS, Wip_State.WAIT))
            {
                CHECK_WOPRODUCT.IsChecked = true;
                txtInputQty.IsEnabled = false;
            }
            else
            {
                txtInputQty.IsEnabled = true;
            }

            // SHIFT_CODE, SHIFT_USER 자동 SET
            GetWrkShftUser();

            // COATER공정에서 CORE, SLURRY 조회, SRS코터에서 BATCH ID 자동 조회 및 SETUP
            if (string.Equals(procId, Process.COATING))
            {
                if (isSingleCoater)
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));
                else
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), "");
            }
            else if (string.Equals(procId, Process.SRS_COATING))
            {
                GetSlurry();
            }



            // 자동선택 추가
            SetLotAutoSelected();


        }

        protected virtual void OnClickStartCancel(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = PRODLOT_GRID;

            DataRow[] dr = Util.gridGetChecked(ref dg, "CHK");

            if (dr == null || dr.Length < 1 || dg == null)
            {
                Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                return;
            }

            StartCancelProcess();
        }

        protected virtual void OnClickEqptEndCancel(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = PRODLOT_GRID;

            DataRow[] dr = Util.gridGetChecked(ref dg, "CHK");

            if (dr == null || dr.Length < 1 || dg == null)
            {
                Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                return;
            }

            EqptEndCancelProcess();
        }

        protected virtual void OnClickConfirm(object sender, RoutedEventArgs e)
        {
            if (txtInputQty.Value > 0)
                return;

            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count)
            {
                Util.MessageValidation("SFU1707");  //실적 확정할 대상이 없습니다.
                return;
            }
            
            // 설비완공 상태에서만 실적확정 가능하도록 변경 [2017-03-02]
            if (!string.Equals(_WIPSTAT, "EQPT_END"))
            {
                Util.MessageValidation("SFU3194");  //실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return;
            }

            // 1. 해당 동 적용 여부 체크
            // 2. 적용 동 버튼 적용 여부 체크
            string[] sAttrbute = { null, procId, "CONFIRM_W" };

            if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_DRB", "", sAttrbute))
                if (!CheckButtonPermissionGroupByBtnGroupID("CONFIRM_W")) return;

            if (procId.Equals(Process.SLITTING))
            {
                if (!ValidateConfirmSlitter())
                    return;
            }
            else
            {

                // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부 Start
                if (procId == Process.MIXING ||
                    procId == Process.PRE_MIXING ||
                    procId == Process.BS ||
                    procId == Process.CMC ||
                    procId == Process.InsulationMixing ||
                    procId == Process.DAM_MIXING)
                {
                    // 원재료 Lot 누락 Validation 기능 적용 Check
                    CheckUseElecMtrlLotValidation();

                    if (_isELEC_MTRL_LOT_VALID_YN)
                    {
                        //미 투입자재 존재 유무 Check
                        if (CheckMissedElecMtrlLot())
                        {
                            //미투입 자재 PopUp
                            PopupInputMaterial();
                            return;
                        }
                    }
                }
                // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부 End

                if (!ValidateConfirm())
                    return;
            }

            CheckLabelPassHold(() => {
                CheckAuthValidation(() => {
                    CheckSpecOutHold(() => {
                        ConfirmCheck();
                    });
                });
            });
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
                CheckSpecOutHold(() => {
                    ConfirmCheck();
                });
        }

        private void OnCloseAuthConfirm2(object sender, EventArgs e)
        {
            // R/P는 해당 부분에서 제외하지 않았기 때문에 인증 후 처리에 대한 Process를 2개로 구분함
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
                ConfirmProcess();
        }

        private void OnCloseMixerConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_MIXER_BATCH window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_MIXER_BATCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                string sRemark = Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "REMARK"));
                DataTableConverter.SetValue(REMARK_GRID.Rows[1].DataItem, "REMARK", window._ConfirmMsg + sRemark);
                ConfirmProcess();
            }
        }

        protected virtual void OnClickEqptEnd(object sender, RoutedEventArgs e)
        {

            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            if (!Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                return;
            }

            // 1. 해당 동 적용 여부 체크
            // 2. 적용 동 버튼 적용 여부 체크
            string[] sAttrbute = { null, procId, "EQPT_END_W" };

            if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_DRB", "", sAttrbute))
                if (!CheckButtonPermissionGroupByBtnGroupID("EQPT_END_W")) return;

            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPT_END();
            wndEqpComment.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                if (dgLotInfo.Rows[i].DataItem != null)
                {
                    if (!DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID").Equals(""))  //_Shift
                    {
                        endLotID = DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID").ToString() + "," + endLotID;
                    }
                }
            }

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = EQUIPMENT_COMBO.SelectedValue.ToString();
                Parameters[1] = procId;
                Parameters[2] = endLotID; /*iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));*/
                Parameters[3] = txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(txtStartDateTime.Text);    // 시작시간 추가
                Parameters[5] = _CUTID;
                Parameters[6] = Util.NVC(txtParentQty.Value);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(OnCloseEqptEnd);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        private void OnCloseEqptEnd(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END window = sender as LGC.GMES.MES.CMM001.CMM_COM_EQPT_END;
            if (window.DialogResult == MessageBoxResult.OK)
                REFRESH = true;
        }

        protected virtual void OnClickBarcode(object sender, RoutedEventArgs e)
        {
            // 상태 관계없이 OUTLOT 기준으로 BARCODE 발행 [2017-01-31]
            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1559");  //발행할 LOT을 선택하십시오.
                return;
            }

            // 코터설비의 경우 공정 라벨 발행 시 버전 저장 안되었을 경우 버전 저장 후 바코드 발행 할 수 있게 변경 처리 [2018-03-08]
            if (string.Equals(procId, Process.COATING))
            {
                if (string.IsNullOrEmpty(GetLotProdVerCode(_LOTID)))
                {
                    Util.MessageValidation("SFU4561"); // 생산실적 화면의 저장버튼 클릭 후(버전 정보 저장) 바코드 출력 하시기 바랍니다.
                    return;
                }
            }

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                return;
            }

            DataTable printDT = GetPrintCount(_LOTID, procId);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            #region [샘플링 출하거래처 추가]
                            // SAMPLING용 발행 매수 추가
                            int iSamplingCount;

                            DataTable LabelDT = new DataTable();
                            LabelDT = (LOTINFO_GRID.ItemsSource as DataView).Table;

                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                foreach (DataRow _iRow in LabelDT.Rows)
                                {
                                    iSamplingCount = 0;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), procId, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            foreach (DataRow _iRow in LabelDT.Rows)
                                Util.UpdatePrintExecCount(Util.NVC(_iRow["LOTID"]), procId);
                            #endregion
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    DataTable LabelDT = new DataTable();
                    LabelDT = (LOTINFO_GRID.ItemsSource as DataView).Table;

                    // S/L공정은 Default 라벨 발행 매수 포함해서 출력해달라고 자동차2동 요청 [2018-07-10]
                    if (string.Equals(procId, Process.SLITTING))
                    {
                        #region [샘플링 출하거래처 추가]
                        // SAMPLING용 발행 매수 추가
                        int iSamplingCount;

                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            foreach (DataRow _iRow in LabelDT.Rows)
                            {
                                iSamplingCount = 0;
                                string[] sCompany = null;
                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                {
                                    iSamplingCount = Util.NVC_Int(items.Key);
                                    sCompany = Util.NVC(items.Value).Split(',');
                                }

                                for (int i = 0; i < iSamplingCount; i++)
                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), procId, i > sCompany.Length - 1 ? "" : sCompany[i]);
                            }
                        foreach (DataRow _iRow in LabelDT.Rows)
                            Util.UpdatePrintExecCount(Util.NVC(_iRow["LOTID"]), procId);
                        #endregion
                    }
                    else
                    {
                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            foreach (DataRow _iRow in LabelDT.Rows)
                                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), procId);

                        foreach (DataRow _iRow in LabelDT.Rows)
                            Util.UpdatePrintExecCount(Util.NVC(_iRow["LOTID"]), procId);

                    }
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        protected virtual void OnClickHistoryCard(object sender, RoutedEventArgs e)
        {
            if (string.Equals(procId, Process.MIXING))
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3 wndPopup1 = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3();
                wndPopup1.FrameOperation = FrameOperation;

                if (wndPopup1 != null)
                {
                    (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[3];
                    Parameters[0] = txtEndLotId.Text; //LOT ID
                    Parameters[1] = procId; //PROCESS ID
                    Parameters[2] = "Y";    // 실적확정 여부

                    C1WindowExtension.SetParameters(wndPopup1, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup1.ShowModal()));
                }
            }
            else
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup2 = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
                wndPopup2.FrameOperation = FrameOperation;

                if (wndPopup2 != null)
                {
                    (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[4];
                    Parameters[0] = txtEndLotId.Text; //LOT ID
                    Parameters[1] = procId; //PROCESS ID

                    // SKIDID
                    if (string.Equals(procId, Process.SLITTING) && !string.IsNullOrEmpty(Util.NVC(txtEndLotId.Tag)))
                        Parameters[2] = Util.NVC(txtEndLotId.Tag);
                    else
                        Parameters[2] = string.Empty;

                    Parameters[3] = "Y";    // 실적확정 여부

                    C1WindowExtension.SetParameters(wndPopup2, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup2.ShowModal()));
                }
            }
        }

        protected virtual void OnClickCleanLot(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LARGELOTID))
                return;

            string ValuetoLotID = string.Empty;

            DataTable dt = DataTableConverter.Convert(LARGELOT_GRID.ItemsSource);

            foreach (DataRow dRow in dt.Rows)
                if (Convert.ToBoolean(dRow["CHK"]) == true)
                    ValuetoLotID = dRow["LOTID_LARGE"].ToString();

            if (string.IsNullOrEmpty(ValuetoLotID))
            {
                Util.MessageValidation("SFU1490");  //대LOT을 선택하십시오.
                return;
            }
            SetDeleteLargeLot(ValuetoLotID);
        }

        protected virtual void OnClickCancelFLot(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_FCUT wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_FCUT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[2];
                Parameters[0] = procId; //PROCESS ID
                Parameters[1] = isSingleCoater ? "Y" : "N";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickEqptIssue(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                Parameters[1] = EQUIPMENT_COMBO.SelectedValue.ToString();
                Parameters[2] = procId;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = EQUIPMENT_COMBO.Text;
                Parameters[6] = Util.NVC(txtShift.Text);
                Parameters[7] = Util.NVC(txtShift.Tag);
                Parameters[8] = Util.NVC(txtWorker.Text);
                Parameters[9] = Util.NVC(txtWorker.Tag);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        protected virtual void OnClickLotCut(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LOTCUT wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LOTCUT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = procId;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickEqptCond(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            CMM_ELEC_EQPT_COND wndPopup = new CMM_ELEC_EQPT_COND();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = EQUIPMENT_COMBO.SelectedValue;
                Parameters[1] = EQUIPMENT_COMBO.Text;
                Parameters[2] = procId;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickPrintLabel(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            // CSR : C20210226-000004
            // ESWA 전극1동 Roll Press, Slitter 프린트 기능 개선
            // BARCODE_PRINT_PWD를 통해 동과 공정 관리
            if (LoginInfo.CFG_LABEL_AUTO == "Y" && IsAreaCommonCodeUse("BARCODE_PRINT_PWD", procId))
            {
                CMM_ELEC_BARCODE_AUTH authConfirm = new CMM_ELEC_BARCODE_AUTH();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = procId;

                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Delete);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[3];
                    Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue;
                    Parameters[1] = procId;
                    Parameters[2] = EQUIPMENT_COMBO.SelectedValue;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void OnCloseAuthConfirm_Delete(object sender, EventArgs e)
        {
            CMM_ELEC_BARCODE_AUTH window = sender as CMM_ELEC_BARCODE_AUTH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[3];
                    Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue;
                    Parameters[1] = procId;
                    Parameters[2] = EQUIPMENT_COMBO.SelectedValue;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        protected virtual void OnClickMixConfirm(object sender, RoutedEventArgs e)
        {
            if (!(string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING)))
            {
                if (EQUIPMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673"); //설비를 선택하세요.
                    return;
                }
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM wndLotIssue = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM();
            wndLotIssue.FrameOperation = FrameOperation;

            if (wndLotIssue != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                Parameters[1] = EQUIPMENT_COMBO.SelectedValue.ToString();
                Parameters[2] = procId;
                Parameters[3] = ""; // iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = ""; //iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = EQUIPMENT_COMBO.Text;

                C1WindowExtension.SetParameters(wndLotIssue, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndLotIssue.ShowModal()));
            }
        }



        private void BtnReservation_Click(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION wndReservation = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION();
            wndReservation.FrameOperation = FrameOperation;

            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
            object[] Parameters = new object[10];

            if (WORKORDER_GRID != null && WORKORDER_GRID.Rows.Count > 0)
            {
                //(grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                //Parameters = new object[10];                
                Parameters[0] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "PRJT_NAME");
                Parameters[1] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "PROD_VER_CODE");
                Parameters[2] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "WOID");
                Parameters[3] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "PRODID");
                Parameters[4] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "ELECTYPE");
                Parameters[5] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "LOTYNAME");
                Parameters[6] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "EQPTID");
                Parameters[7] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[8] = Util.NVC(procId);
                Parameters[9] = (WORKORDER_GRID.ItemsSource as DataView).ToTable();
            }

            C1WindowExtension.SetParameters(wndReservation, Parameters);
            wndReservation.Closed += WndReservation_Closed;

            this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
        }

        private void btnFoil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.CMM001.CMM_COM_FOIL wndReservation = new LGC.GMES.MES.CMM001.CMM_COM_FOIL();
                wndReservation.FrameOperation = FrameOperation;

                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMovetoHalf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_MOVE_HALF wndReservation = new LGC.GMES.MES.CMM001.CMM_ELEC_MOVE_HALF();
                wndReservation.FrameOperation = FrameOperation;

                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WndReservation_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION window = sender as LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                OnClickSearch(null, null);
            }
        }

        private void wndHoldChk_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_HOLD_YN window = sender as CMM_ELEC_HOLD_YN;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                _HOLDYN_CHK = window.HOLDYNCHK;
                //ConfirmProcess();
                ValidateCarrierCTUnloader();
            }
        }

        private void popRollMapHold_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_HOLD window = sender as CMM_ROLLMAP_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _HOLDYN_CHK = window.HoldCheck;
                ConfirmProcess();
            }
        }

        private void wndCst_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_CST_RELATION window = sender as CMM_ELEC_CST_RELATION;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                //_CSTID_CHK = "OK";
                ConfirmProcess();
            }
        }

        #region #전수불량 Lane 등록
        private void wndDefectLaneChk_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_DFCT_ACTION_CHK window = sender as CMM_ELEC_DFCT_ACTION_CHK;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                ConfirmProcess();
            }
        }
        #endregion
        protected virtual void OnClickSamplingProd(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_RP_SAMPLING wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_RP_SAMPLING();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                C1WindowExtension.SetParameters(wndPopup, null);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickSamplingProdT1(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = procId;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickProcReturn(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_RP_RETURN wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_RP_RETURN();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                C1WindowExtension.SetParameters(wndPopup, null);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        //2020-07-10 오화백 (롤프레스, 슬리터, 하프슬리터 고객인증그룹조회 팝업 추가) CWA 전극2동만 적용
        protected virtual void OnClickbtnCustomer(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }


            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_CUSTOMER_GROUP_SEARCH wndMixerTankInfo = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_CUSTOMER_GROUP_SEARCH();
            wndMixerTankInfo.FrameOperation = FrameOperation;

            if (wndMixerTankInfo != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                Parameters[1] = EQUIPMENT_COMBO.SelectedValue.ToString();
                Parameters[2] = procId;

                C1WindowExtension.SetParameters(wndMixerTankInfo, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndMixerTankInfo.ShowModal()));
            }
        }


        protected virtual void OnExtraMouseLeave(object sender, MouseEventArgs e)
        {
            (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
        }

        protected virtual void OnClickMixerTankInfo(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MIXER_TANK_INFO wndMixerTankInfo = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MIXER_TANK_INFO();
            wndMixerTankInfo.FrameOperation = FrameOperation;

            if (wndMixerTankInfo != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                Parameters[1] = EQUIPMENT_COMBO.SelectedValue.ToString();
                Parameters[2] = procId;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndMixerTankInfo, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndMixerTankInfo.ShowModal()));
            }
        }

        protected virtual void OnClickWorkHalfSlitSide(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
            if (GetCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(procId, Process.ROLL_PRESSING))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ESWA popupWorkHalfSlitting = new CMM_ELEC_WORK_HALF_SLITTING_ESWA();
                popupWorkHalfSlitting.FrameOperation = FrameOperation;
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[2];
                parameters[0] = EQUIPMENT_COMBO.SelectedValue.ToString();
                parameters[1] = (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag;   // 무지부 방향

                C1WindowExtension.SetParameters(popupWorkHalfSlitting, parameters);
                popupWorkHalfSlitting.Closed += popupWorkHalfSlitting_Closed;

                this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitting.ShowModal()));
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popupWorkHalfSlitting = new CMM_ELEC_WORK_HALF_SLITTING();
                popupWorkHalfSlitting.FrameOperation = FrameOperation;
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[2];
                parameters[0] = EQUIPMENT_COMBO.SelectedValue.ToString();
                parameters[1] = (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag;   // 무지부 방향

                C1WindowExtension.SetParameters(popupWorkHalfSlitting, parameters);
                popupWorkHalfSlitting.Closed += popupWorkHalfSlitting_Closed;

                this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitting.ShowModal()));
            }

        }

        protected virtual void OnClickReturnCondition(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_CONDITION_RETURN_ESWA popCondition = new CMM_CONDITION_RETURN_ESWA();

            popCondition.FrameOperation = FrameOperation;
            (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;

            object[] parameters = new object[7];
            parameters[0] = EQUIPMENT_COMBO.SelectedValue.ToString(); ; //설비코드
            parameters[1] = EQUIPMENT_COMBO.Text; // 설비코드명
            parameters[2] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "PRJT_NAME");  //PJT
            parameters[3] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "PROD_VER_CODE"); //Version
            parameters[4] = procId; //공정
            parameters[5] = "E"; //전극 화면에서 호출
            parameters[6] = DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "WOID"); //WOID

            C1WindowExtension.SetParameters(popCondition, parameters);
            popCondition.Closed += popCondition_Closed;

            this.Dispatcher.BeginInvoke(new Action(() => popCondition.ShowModal()));

        }

        protected virtual void OnClickLogisStat(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_ELEC_LOGIS_STAT popupElecLogisStat = new CMM_ELEC_LOGIS_STAT();
            popupElecLogisStat.FrameOperation = FrameOperation;
            (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
            object[] parameters = new object[1];
            parameters[0] = EQUIPMENT_COMBO.SelectedValue.ToString();
            C1WindowExtension.SetParameters(popupElecLogisStat, parameters);
            this.Dispatcher.BeginInvoke(new Action(() => popupElecLogisStat.ShowModal()));

        }


        private void popupWorkHalfSlitting_Closed(object sender, EventArgs e)
        {
            //CMM_ELEC_LOGIS_STAT popup = sender as CMM_ELEC_LOGIS_STAT;
            //2022 03 30  오화백  무지부 팝업닫기 버그 수정

            //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
            if (GetCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(procId, Process.ROLL_PRESSING))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ESWA popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ESWA;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popup = sender as CMM_ELEC_WORK_HALF_SLITTING;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                }
            }
        }

        private void popCondition_Closed(object sender, EventArgs e)
        {
            CMM_CONDITION_RETURN_ESWA popup = sender as CMM_CONDITION_RETURN_ESWA;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetWorkHalfSlittingSide();
            }
        }

        protected virtual void OnClickSkidTypeSettingByPort(object sender, RoutedEventArgs e)
        {
            CMM_ELEC_SKIDTYPE_SETTING_PORT popSkidTypeSettingByPort = new CMM_ELEC_SKIDTYPE_SETTING_PORT { FrameOperation = FrameOperation };
            object[] parameters = new object[6];
            parameters[0] = LoginInfo.CFG_AREA_ID;
            C1WindowExtension.SetParameters(popSkidTypeSettingByPort, parameters);

            popSkidTypeSettingByPort.Closed += popSkidTypeSettingByPort_Closed;
            Dispatcher.BeginInvoke(new Action(() => popSkidTypeSettingByPort.ShowModal()));
        }

        private void popSkidTypeSettingByPort_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_SKIDTYPE_SETTING_PORT popup = sender as CMM_ELEC_SKIDTYPE_SETTING_PORT;
            if (popup != null && popup.IsUpdated)
            {

            }
        }


        private void SetDeleteLargeLot(string LargeLotId)
        {
            //대LOT 정리를 하시겠습니까?
            Util.MessageConfirm("SFU1488", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));

                    DataRow indata = inTable.NewRow();
                    indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    indata["IFMODE"] = IFMODE.IFMODE_OFF;
                    indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                    indata["LOTID"] = LargeLotId;
                    indata["USERID"] = LoginInfo.USERID;
                    indata["COATING_SIDE_TYPE_CODE"] = COATTYPE_COMBO.Visibility == Visibility.Visible ? COATTYPE_COMBO.SelectedValue : null;
                    inTable.Rows.Add(indata);

                    new ClientProxy().ExecuteService("BR_PRD_REG_TERM_LARGE_LOT", "INDATA", null, inTable, (result, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                            OnClickSearch(SEARCHBUTTON, null);
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    });
                    LARGELOTID = string.Empty;
                }
            });
        }

        protected virtual void OnClickSaveWipReason(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDefect(WIPREASON_GRID);

                if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                    SaveDefect(WIPREASON2_GRID);

                if (_isRollMapEquipment) SaveDefectForRollMap();

                isChangeWipReason = false;
                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        protected virtual void OnClickProcReason(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[1] = procId;
                Parameters[2] = _CUTID;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[4] = GetUnitFormatted();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseProcReason);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseProcReason(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN window = sender as LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN;
            if (window.DialogResult == MessageBoxResult.OK)
                SetProcWipReasonData();
        }

        protected virtual void OnSumReasonGridChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb != null && (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count)
            {
                WIPREASON_GRID.Refresh(false);

                if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count + 1)
                    GetDefectList();
                else if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1)
                    GetDefectListMultiLane();
            }
        }

        protected virtual void OnLaneSelectionItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            LANENUM_COMBO.Text = ObjectDic.Instance.GetObjectName("Lane선택");

        }

        protected virtual void OnLaneChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                if (string.Equals(checkBox.Tag, "ALL"))
                {
                    foreach (CheckBox _checkBox in LANENUM_COMBO.Items)
                        _checkBox.IsChecked = checkBox.IsChecked;
                }
                else
                {
                    SetVisibilityWipReasonGrid(Util.NVC(checkBox.Tag), checkBox.IsChecked);
                }
            }
        }

        protected virtual void OnCancelDeleteLot(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = procId; //PROCESS ID
                Parameters[1] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[2] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(OnCloseCancelDeleteLot);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseCancelDeleteLot(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
                REFRESH = true;
        }

        private void SetVisibilityWipReasonGrid(string sLotID, bool? isVisibility)
        {
            for (int i = WIPREASON_GRID.Columns["TAG_CONV_RATE"].Index; i < WIPREASON_GRID.Columns.Count; i++)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                if (string.Equals(sLotID, WIPREASON_GRID.Columns[i].Name) ||
                    ((grdDataCollect.Children[0] as UcDataCollect).chkSum.IsChecked == false &&
                        (string.Equals(sLotID + "NUM", WIPREASON_GRID.Columns[i].Name) || string.Equals(sLotID + "CNT", WIPREASON_GRID.Columns[i].Name))))
                    WIPREASON_GRID.Columns[i].Visibility = isVisibility == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #region #전수불량 Lane 등록
        private DataTable getDefectLaneLotList(string sCUTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUT_ID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CUT_ID"] = sCUTID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DFCT_LANE_SL", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetDisableWipReasonGrid(DataRow[] laneRow)
        {
            for (int j = 0; j < _DEFECTLANELOT.Length; j++)
            {
                for (int i = WIPREASON_GRID.Columns["TAG_CONV_RATE"].Index; i < WIPREASON_GRID.Columns.Count; i++)
                {
                    if (string.Equals(laneRow[j]["LOTID"], WIPREASON_GRID.Columns[i].Name) || string.Equals(laneRow[j]["LOTID"] + "NUM", WIPREASON_GRID.Columns[i].Name) || string.Equals(laneRow[j]["LOTID"] + "CNT", WIPREASON_GRID.Columns[i].Name))
                    {
                        WIPREASON_GRID.Columns[i].IsReadOnly = true;
                    }
                }
            }
            // 전체불량의 경우 ALL Column ReadOnly
            if (_dtDEFECTLANENOT.Rows.Count == _DEFECTLANELOT.Length)
                WIPREASON_GRID.Columns[WIPREASON_GRID.Columns["ALL"].Index].IsReadOnly = true;

            WIPREASON_GRID.LoadedCellPresenter -= OnLoadedDefectLaneCellPresenter;
            WIPREASON_GRID.LoadedCellPresenter += OnLoadedDefectLaneCellPresenter;
        }

        protected virtual void OnLoadedDefectLaneCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            var _defectLane = new List<string>();
            foreach (DataRow row in _DEFECTLANELOT)
            {
                _defectLane.Add(Util.NVC(row["LOTID"]));
                _defectLane.Add(Util.NVC(row["LOTID"]) + "NUM");
                _defectLane.Add(Util.NVC(row["LOTID"]) + "CNT");
            }
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {

                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                    //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                }
                                else if (_defectLane.Contains(e.Cell.Column.Name) || _defectLane.Contains(e.Cell.Column.Name + "NUM") || _defectLane.Contains(e.Cell.Column.Name + "CNT"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);

                                if (e.Cell.Column.Name.Length == 13 && e.Cell.Column.Name.Contains("NUM") == true && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);

                            }
                            // 전체불량의 경우 ALL Column ReadOnly
                            if (_dtDEFECTLANENOT.Rows.Count == _DEFECTLANELOT.Length)
                                if (string.Equals(e.Cell.Column.Name, "ALL"))
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }));
            }
        }

        private bool IsAbnormalLot()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            DataRow dr = dt.NewRow();
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "LOTID_PR"));
            dt.Rows.Add(dr);
            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", dt);

            if (!string.IsNullOrEmpty(result.Rows[0]["ABNORM_FLAG"].ToString()) && result.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
            {
                return true;
            }
            else
                return false;


        }

        #endregion

        private void SaveCotton(C1DataGrid dg)
        {
            string[] SLIT_CHK = new string[5];
            SLIT_CHK[0] = "SLIT_CHK_001";
            SLIT_CHK[1] = "SLIT_CHK_002";
            SLIT_CHK[2] = "SLIT_CHK_003";
            SLIT_CHK[3] = "SLIT_CHK_004";
            SLIT_CHK[4] = "SLIT_CHK_005";

            DataSet inDataSet = new DataSet();
            DataTable dtCotton = (dg.ItemsSource as DataView).Table;

            DataRow inDataRow = null;
            DataTable IndataTable = inDataSet.Tables.Add("INDATA");
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("NOTE", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            inDataRow = IndataTable.NewRow();
            inDataRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[0].DataItem, "WIPSEQ"));
            inDataRow["NOTE"] = "";
            inDataRow["USERID"] = LoginInfo.USERID;

            IndataTable.Rows.Add(inDataRow);

            DataRow inDataRow2 = null;
            DataTable IndataTable2 = inDataSet.Tables.Add("IN_LOT");
            IndataTable2.Columns.Add("LOTID", typeof(string));
            IndataTable2.Columns.Add("SLIT_CHK_ITEM_CODE", typeof(string));
            IndataTable2.Columns.Add("CHK_RSLT", typeof(string));

            for (int k = 0; k < dtCotton.Rows.Count; k++)
            {
                for (int i = 0; i < SLIT_CHK.Length; i++)
                {
                    inDataRow2 = IndataTable2.NewRow();
                    string schk = dtCotton.Rows[k][SLIT_CHK[i]].ToString();
                    if (schk.Equals("True"))
                    {
                        schk = "Y";
                    }
                    else
                    {
                        if (schk.Equals("False"))
                        {
                            schk = "";
                        }
                    }

                    inDataRow2["LOTID"] = dtCotton.Rows[k]["LOTID"].ToString();
                    inDataRow2["SLIT_CHK_ITEM_CODE"] = SLIT_CHK[i].ToString();
                    inDataRow2["CHK_RSLT"] = schk;
                    IndataTable2.Rows.Add(inDataRow2);
                }
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SLIT_CHK_DATA", "INDATA,IN_LOT", null, inDataSet);
                COTTON_GRID.EndEdit(true);
                isChangeCotton = false;
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region[POSTACTION]
        private void SavePostHold()
        {
            if (POSTHOLD_GRID.GetRowCount() < 1)
                return;
            DataTable dt = ((DataView)POSTHOLD_GRID.ItemsSource).Table;

            int idx = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");
            string wipseq = string.Empty;
            if (idx >= 0)
                wipseq = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[idx].DataItem, "WIPSEQ"));
            else
                return;
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow inData = null;
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    inData = inTable.NewRow();

                    inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                    if (POSTHOLD_GRID.Rows[0].Visibility == Visibility.Visible)
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                    else
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                    inData["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(inData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    string[] HOLD_CHK = new string[1];
                    HOLD_CHK[0] = "POST_HOLD_001";

                    DataSet inDataSet = new DataSet();
                    DataTable dtpostHold = (POSTHOLD_GRID.ItemsSource as DataView).Table;

                    DataRow inDataRow = null;
                    DataTable IndataTable = inDataSet.Tables.Add("INDATA");
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                    IndataTable.Columns.Add("USERID", typeof(string));

                    inDataRow = IndataTable.NewRow();
                    inDataRow["WIPSEQ"] = wipseq;
                    inDataRow["USERID"] = LoginInfo.USERID;

                    IndataTable.Rows.Add(inDataRow);

                    DataRow inDataRow2 = null;
                    DataTable IndataTable2 = inDataSet.Tables.Add("IN_LOT");
                    IndataTable2.Columns.Add("LOTID", typeof(string));
                    IndataTable2.Columns.Add("HOLD_CHK_ITEM_CODE", typeof(string));
                    IndataTable2.Columns.Add("CHK_RSLT", typeof(string));
                    IndataTable2.Columns.Add("HOLD", typeof(string));
                    IndataTable2.Columns.Add("NOTE", typeof(string));
                    for (int k = 0; k < dtpostHold.Rows.Count; k++)
                    {
                        if (!string.Equals(Util.NVC(dtpostHold.Rows[k]["LOTID"]), ObjectDic.Instance.GetObjectName("공통특이사항")))
                        {
                            for (int i = 0; i < HOLD_CHK.Length; i++)
                            {
                                inDataRow2 = IndataTable2.NewRow();
                                inDataRow2["LOTID"] = Util.NVC(dtpostHold.Rows[k]["LOTID"]);
                                inDataRow2["HOLD_CHK_ITEM_CODE"] = HOLD_CHK[i].ToString();
                                inDataRow2["CHK_RSLT"] = "N";
                                inDataRow2["HOLD"] = string.Equals(Util.NVC(dtpostHold.Rows[k]["POST_HOLD"]), "True") ? "Y" : "N";

                                if (POSTHOLD_GRID.Rows[0].Visibility == Visibility.Visible)
                                    inDataRow2["NOTE"] = Util.NVC(dtpostHold.Rows[k]["REMARK"]) + "|" + Util.NVC(dtpostHold.Rows[0]["REMARK"]);
                                else
                                    inDataRow2["NOTE"] = Util.NVC(dtpostHold.Rows[k]["REMARK"]);

                                IndataTable2.Rows.Add(inDataRow2);
                            }
                        }
                    }
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_POST_HOLD", "INDATA,IN_LOT", null, inDataSet);
                    POSTHOLD_GRID.EndEdit(true);
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    isChangePostAction = false;

                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #endregion

        private DataTable dtDataCollectOfChildDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            // if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
            if (isResnCountUse == true && (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING)))
                IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            DataRow inDataRow = null;
            DataTable dtTop = (dg.ItemsSource as DataView).Table;

            foreach (DataRow _iRow in dtTop.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = _LOTID;
                inDataRow["WIPSEQ"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "WIPSEQ");
                inDataRow["ACTID"] = _iRow["ACTID"];
                inDataRow["RESNCODE"] = _iRow["RESNCODE"];
                inDataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                inDataRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(_iRow["DFCT_TAG_QTY"])) ? 0 : _iRow["DFCT_TAG_QTY"];

                // SLITTING 공정에서 Deffec 저장 시 LANE수량 1로 변경 요청 [2017-01-17]
                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                {
                    inDataRow["LANE_QTY"] = 1; //txtLaneQty.Value;
                    inDataRow["LANE_PTN_QTY"] = 1; //txtLanePatternQty.Value;
                }
                else
                {
                    inDataRow["LANE_QTY"] = txtLaneQty == null ? 0 : txtLaneQty.Value;
                    inDataRow["LANE_PTN_QTY"] = txtLanePatternQty == null ? 0 : txtLanePatternQty.Value;
                }
                inDataRow["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

                //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
                if (isResnCountUse == true && (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING)))
                    inDataRow["WRK_COUNT"] = _iRow["COUNTQTY"].ToString() == "" ? DBNull.Value : _iRow["COUNTQTY"];

                IndataTable.Rows.Add(inDataRow);
            }
            return IndataTable;
        }

        private DataTable dtDataCollectOfChildrenDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            if (isResnCountUse == true)
                IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            int iCount = isResnCountUse == true ? 1 : 0;

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;
            int iLotCount = 0;

            for (int iCol = dg.Columns["ALL"].Index + (2 + iCount); iCol < dg.Columns["COSTCENTERID"].Index; iCol += (2 + iCount))
            {
                string sublotid = dg.Columns[iCol].Name;

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["LOTID"] = sublotid;
                    inData["WIPSEQ"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "WIPSEQ");
                    inData["ACTID"] = _iRow["ACTID"];
                    inData["RESNCODE"] = _iRow["RESNCODE"];
                    inData["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["DFCT_TAG_QTY"] = _iRow[sublotid + "CNT"].ToString().Equals("") ? 0 : _iRow[sublotid + "CNT"];

                    // SLITTING 공정에서 Deffec 저장 시 LANE수량 1로 변경 요청 [2017-01-17]
                    if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                    {
                        inData["LANE_QTY"] = 1; //txtLaneQty.Value;
                        inData["LANE_PTN_QTY"] = 1; //txtLanePatternQty.Value;
                    }
                    else if (string.Equals(procId, Process.HALF_SLITTING))
                    {
                        inData["LANE_QTY"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count + iLotCount].DataItem, "LANE_QTY");
                        inData["LANE_PTN_QTY"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count + iLotCount].DataItem, "LANE_PTN_QTY");
                    }
                    else
                    {
                        inData["LANE_QTY"] = txtLaneQty.Value;
                        inData["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                    }
                    inData["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

                    //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                    //    inData["WRK_COUNT"] = _iRow["COUNTQTY"].ToString() == "" ? DBNull.Value : _iRow["COUNTQTY"];
                    if (isResnCountUse == true)
                        inData["WRK_COUNT"] = string.IsNullOrEmpty(_iRow[sublotid + "NUM"].ToString()) ? 0 : _iRow[sublotid + "NUM"];

                    IndataTable.Rows.Add(inData);
                }
                iLotCount++;
            }
            return IndataTable;
        }

        /// <summary>
        /// 다중 자LOT용
        /// </summary>
        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            #region SAVE DEFECT
            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = procId;
            inDataTable.Rows.Add(inDataRow);

            DataTable inDefectLot = (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1 ? dtDataCollectOfChildrenDefect(dg) : dtDataCollectOfChildDefect(dg);
            #endregion
            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        protected virtual void OnClickSaveQuality(object sender, RoutedEventArgs e)
        {
            // 품질정보 SLITTER, SRS SLITTER는 CUT단위로 관리 [2017-02-01]
            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
            {
                SaveCutQuality(QUALITY_GRID);
            }
            else
            {
                SaveQuality(QUALITY_GRID);
            }
        }

        //OnClickSaveCarrier
        protected virtual void OnClickSaveCarrier(object sender, RoutedEventArgs e)
        {
            if (string.Equals(procId, Process.COATING))
            {
                if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    if (LOTINFO_GRID.GetRowCount() == 0)
                    {
                        Util.MessageValidation("SFU3552");  //저장 할 DATA가 없습니다.
                        return;
                    }

                    //if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "OUT_CSTID"))))
                    //{
                    //    Util.MessageValidation("SFU6051");  //입력오류 : Carrier ID를 입력 하세요.
                    //    return;
                    //}

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("SRCTYPE", typeof(string));

                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID").ToString();
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "OUT_CSTID"));
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                        inTable.Rows.Add(newRow);

                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "OUT_CSTID"))))
                        {
                            Util.MessageValidation("SFU6051");  //입력오류 : Carrier ID를 입력 하세요.
                            return;
                        }

                        if (!CheckCstID(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID").ToString(), Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "OUT_CSTID"))))
                        {
                            return;
                        }
                        else
                        {
                            if (!CheckLotID(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID").ToString(), Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "OUT_CSTID"))))
                                return;
                        }
                    }

                    //저장하시겠습니까?
                    Util.MessageConfirm("SFU1241", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //SaveMappingInfo(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID").ToString(), Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "OUT_CSTID")));
                            new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                            {
                                try
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }

                                    Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                }
                            });
                        }
                    });

                }
            }
        }

        protected virtual void OnClickWIPHistory(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EQUIPMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                if (LOTINFO_GRID.GetRowCount() > 0)
                {
                    if (txtInputQty.Value <= 0)
                    {
                        if (IsCoaterProdVersion() == true && !string.Equals(GetCoaterMaxVersion(), txtVersion.Text))
                        {
                            // 작업지시 최신 Version과 상이합니다! 그래도 저장하시겠습니까?
                            Util.MessageConfirm("SFU4462", (sResult) =>
                            {
                                if (sResult == MessageBoxResult.OK)
                                {
                                    SaveWIPHistory();
                                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                                }
                            }, new object[] { GetCoaterMaxVersion(), txtVersion.Text });
                        }
                        else
                        {
                            SaveWIPHistory();
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                            Util.MessageInfo("SFU1270");    //저장되었습니다.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region # 전수불량Lane 등록
        protected virtual void OnClickRegDefectLane(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (EQUIPMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                if (LOTINFO_GRID.GetRowCount() < 1)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                    return;
                }

                if (!string.Equals(_WIPSTAT, Wip_State.EQPT_END))
                {
                    Util.MessageValidation("SFU1269");  //장비완료 상태가 아닙니다.
                    return;
                }
                if (string.Equals(procId, Process.COATING))
                {
                    LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                        object[] Parameters = new object[6];

                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "LOTID"));
                        Parameters[1] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                        Parameters[2] = procId;
                        Parameters[3] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "PRODID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));

                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                else
                {
                    LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        (grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
                        object[] Parameters = new object[6];

                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "CUT_ID"));

                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            if (string.Equals(procId, Process.COATING))
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE;
                if (window.DialogResult == MessageBoxResult.OK)
                    REFRESH = true;
            }
            else
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE;
                if (window.DialogResult == MessageBoxResult.OK)
                    REFRESH = true;
            }



        }
        #endregion

        private bool IsCoaterProdVersion()
        {
            // 1. 공정체크
            if (!string.Equals(procId, Process.COATING))
                return false;

            // 2. 입력된 VERSION 체크
            if (string.IsNullOrEmpty(txtVersion.Text))
                return false;

            // 3. 양산버전 이외는 체크 안함
            System.Text.RegularExpressions.Regex engRegex = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]");
            if (engRegex.IsMatch(txtVersion.Text.Substring(0, 1)) == true)
                return false;

            // 4. 1번 CUT인지 확인
            string sCut = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "CUT"));
            if (string.IsNullOrEmpty(sCut) || !string.Equals(sCut, "1"))
                return false;

            return true;
        }

        private string GetCoaterMaxVersion()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "PRODID"));
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_MAX_VERSION", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return Util.NVC(dtMain.Rows[0][0]);

            }
            catch (Exception ex) { }

            return "";
        }

        private void SaveWIPHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS1QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS2QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS3QTY", typeof(decimal));
            inDataTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            //RFID 관련 추가
            inDataTable.Columns.Add("OUT_CSTID", typeof(string));

            for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
            {
                DataRow inLotDetailDataRow = null;
                inLotDetailDataRow = inDataTable.NewRow();
                inLotDetailDataRow["LOTID"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID").ToString();
                inLotDetailDataRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                inLotDetailDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                inLotDetailDataRow["WIPNOTE"] = txtWipNote != null ? Util.NVC(txtWipNote.Text) : null;
                inLotDetailDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                inLotDetailDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);

                if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING))
                {
                    inLotDetailDataRow["LANE_PTN_QTY"] = Util.NVC_Decimal(txtLanePatternQty.Value);
                    inLotDetailDataRow["LANE_QTY"] = Util.NVC_Decimal(txtLaneQty.Value);
                }

                if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                    inLotDetailDataRow["PROD_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY"));
                else
                    inLotDetailDataRow["PROD_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY"));

                inLotDetailDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inLotDetailDataRow);

            }

            new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inDataTable, (result, Returnex) =>
            {
                try
                {
                    if (Returnex != null)
                    {
                        Util.MessageException(Returnex);
                        return;
                    }

                    // 코터 공정에서 Product Lot 버전 정보 갱신
                    if (string.Equals(procId, Process.COATING) && !string.IsNullOrEmpty(txtVersion.Text))
                    {
                        int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");
                        if (iRow >= 0)
                            DataTableConverter.SetValue(PRODLOT_GRID.Rows[iRow].DataItem, "COATERVER", Util.NVC(txtVersion.Text));
                    }

                    SaveDefect(WIPREASON_GRID);

                    if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                        SaveDefect(WIPREASON2_GRID);

                    isChangeWipReason = false;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private bool CheckCstID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = sCstID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    //CSTID[%1]에 해당하는 CST가 없습니다.
                    Util.MessageValidation("SFU7001", sCstID);
                    return false;
                }

                //캐리어 상태 Check
                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("U"))
                {
                    if (Util.NVC(searchResult.Rows[0]["CURR_LOTID"]) == sLotID)
                    {
                        //Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                        Util.MessageValidation("SFU5126", sCstID, sLotID);
                        return false;
                    }
                    else
                    {
                        //CSTID[%1] 이 상태가 %2 입니다.
                        Util.MessageValidation("SFU7002", Util.NVC(searchResult.Rows[0]["CSTID"]), Util.NVC(searchResult.Rows[0]["CSTSNAME"]));
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;

        }

        private bool CheckLotID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    //LOTID[%1]에 해당하는 LOT이 없습니다.
                    Util.MessageValidation("SFU7000", sLotID);
                    return false;
                }

                if (!string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CSTID"])))
                {
                    //Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                    Util.MessageValidation("SFU5126", Util.NVC(searchResult.Rows[0]["CSTID"]), sLotID);
                    return false;
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;

        }

        private void SaveMappingInfo(string sLotID, string sCstID)
        {
            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID;
                newRow["CSTID"] = sCstID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                inTable.Rows.Add(newRow);


                new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

            // TOP/BACK을 동시 적용하기 위하여 해당 방식으로 변경 (SPLUNK문제로 CMI요청사항) [2018-05-24]
            if (QUALITY2_GRID.Visibility == Visibility.Visible)
                inEDCLot.Merge(dtDataCollectOfChildQuality(QUALITY2_GRID));

            new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (!dg.Name.Equals("dgColor") && !dg.Name.Equals("dgDefectTag"))
                    isChangeQuality = false;

                if (dg.Name.Equals("dgColor"))
                    Util.MessageInfo("SFU3272");    //색지정보가 저장되었습니다.
                else if (dg.Name.Equals("dgDefectTag"))
                    Util.MessageInfo("SFU3271");    //불량태그정보가 저장되었습니다.
                else
                    Util.MessageInfo("SFU1998");    //품질 정보가 저장되었습니다.                
            });
        }

        private void SavePublicQuality(C1DataGrid dg)
        {
            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

            // TOP/BACK을 동시 적용하기 위하여 해당 방식으로 변경 (SPLUNK문제로 CMI요청사항) [2018-05-24]
            if (QUALITY2_GRID.Visibility == Visibility.Visible)
                inEDCLot.Merge(dtDataCollectOfChildQuality(QUALITY2_GRID));

            new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (!dg.Name.Equals("dgColor") && !dg.Name.Equals("dgDefectTag"))
                    isChangeQuality = false;
            });
        }

        private void SaveCutQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataSet inCollectLot = dtDataCollectOfChildrenQuality(dg);

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPDATACOLLECT_CUTID", "INDATA,IN_DATA", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1998");    //품질 정보가 저장되었습니다.
                isChangeQuality = false;
            }, inCollectLot);
        }

        private void SavePublicCutQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataSet inCollectLot = dtDataCollectOfChildrenQuality(dg);

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPDATACOLLECT_CUTID", "INDATA,IN_DATA", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                isChangeQuality = false;
            }, inCollectLot);
        }

        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            //20211228입력된 값이 숫자인지 확인하기 위한 변수 
            int j = 0;
            double p1 = 0F;

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;

            if (dg.Name.Equals("dgColor"))
            {
                GetWipSeq(_LOTID, string.Empty);

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = _LOTID;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];
                    inData["CLCTVAL01"] = inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(_WIPSEQ) ? null : _WIPSEQ;
                    inData["CLCTSEQ"] = 1;
                    IndataTable.Rows.Add(inData);
                }
            }
            else
            {
                GetWipSeq(_LOTID, string.Empty);

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = _LOTID;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];

                    //20211228이전 숫자입력 방지를 위한 validation
                    //decimal tmp;
                    //if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    //    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    //else
                    //    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(_WIPSEQ) ? null : _WIPSEQ;
                    inData["CLCTSEQ"] = 1;
                    //IndataTable.Rows.Add(inData);
                    if (_iRow["INSP_VALUE_TYPE_CODE"].ToString().Equals("NUM"))
                    {
                        bool canConvert = double.TryParse(_iRow["CLCTVAL01"].ToString(), out p1);
                        if (!canConvert || double.IsNaN(Convert.ToDouble(_iRow["CLCTVAL01"])))
                        {
                            inData["CLCTVAL01"] = "";
                            DataTableConverter.SetValue(dg.Rows[j].DataItem, "CLCTVAL01", "");
                        }
                        else inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]).Trim();
                    }
                    else inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                    IndataTable.Rows.Add(inData);
                    j++;
                }
                dg.UpdateLayout();
            }
            return IndataTable;
        }

        private DataSet dtDataCollectOfChildrenQuality(C1DataGrid dg)
        {
            //20211228입력된 값이 숫자인지 확인하기 위한 변수 
            int j = 0;
            double p1 = 0F;

            // 사용안하는 메서드라 SLITTER CUT의 BINDING메서드로 용도 변경 사용 [2017-02-01]
            DataSet inDataSet = new DataSet();

            DataTable IndataTable = inDataSet.Tables.Add("INDATA");
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("CUT_ID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = IndataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["PROCID"] = procId;
            inDataRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
            inDataRow["CUT_ID"] = Util.NVC(_CUTID);
            inDataRow["USERID"] = LoginInfo.USERID;
            IndataTable.Rows.Add(inDataRow);

            DataTable IndataDetailTable = inDataSet.Tables.Add("IN_DATA");
            IndataDetailTable.Columns.Add("CLCTITEM", typeof(string));
            IndataDetailTable.Columns.Add("VERSION", typeof(string));
            IndataDetailTable.Columns.Add("CLCTVAL01", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            foreach (DataRow _iRow in dt.Rows)
            {
                DataRow inDetailDataRow = null;
                inDetailDataRow = IndataDetailTable.NewRow();
                inDetailDataRow["CLCTITEM"] = _iRow["CLCTITEM"];
                inDetailDataRow["VERSION"] = 0;

                //20211228이전 숫자입력 방지를 위한 validation
                //decimal tmp;
                //if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                //    inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                //else
                //    inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                ////inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]);

                //IndataDetailTable.Rows.Add(inDetailDataRow);

                if (_iRow["INSP_VALUE_TYPE_CODE"].ToString().Equals("NUM"))
                {
                    bool canConvert = double.TryParse(_iRow["CLCTVAL01"].ToString(), out p1);
                    if (!canConvert || double.IsNaN(Convert.ToDouble(_iRow["CLCTVAL01"])))
                    {
                        inDetailDataRow["CLCTVAL01"] = "";
                        DataTableConverter.SetValue(dg.Rows[j].DataItem, "CLCTVAL01", "");
                    }
                    else inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]).Trim();
                }
                else inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                IndataDetailTable.Rows.Add(inDetailDataRow);
                j++;
            }
            dg.UpdateLayout();
            return inDataSet;
        }

        private void GetWipSeq(string sLotID, string sCLCTITEM)
        {
            _WIPSEQ = string.Empty;
            _CLCTSEQ = string.Empty;

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = procId;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_SL", "INDATA", "RSLTDT", IndataTable);

            if (dtResult.Rows.Count == 0)
            {
                _WIPSEQ = string.Empty;
                _CLCTSEQ = string.Empty;
            }
            else
            {
                _WIPSEQ = dtResult.Rows[0]["WIPSEQ"].ToString();
                _CLCTSEQ = dtResult.Rows[0]["CLCTSEQ"].ToString();
            }
        }

        protected virtual void OnClickAddMaterial(object sender, RoutedEventArgs e)
        {
            if (INPUTMTRL_GRID.ItemsSource == null || INPUTMTRL_GRID.Rows.Count < 0)
                return;

            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            DataTable dt = ((DataView)INPUTMTRL_GRID.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["CHK"] = true;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        protected virtual void OnClickRemoveMaterial(object sender, RoutedEventArgs e)
        {
            if (INPUTMTRL_GRID.ItemsSource == null || INPUTMTRL_GRID.Rows.Count < 0)
                return;

            DataTable dt = (INPUTMTRL_GRID.ItemsSource as DataView).Table;

            dt.Rows[INPUTMTRL_GRID.CurrentRow.Index].Delete();

            Util.GridSetData(INPUTMTRL_GRID, dt, FrameOperation, true);
        }

        protected virtual void OnClickDeleteMaterial(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = (grdDataCollect.Children[0] as UcDataCollect).dgMaterial;
            DataRow[] drs = Util.gridGetChecked(ref dg, "CHK");

            if (drs != null)
            {
                //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                Util.MessageConfirm("SFU1815", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                        SetMaterial(_LOTID, "D");
                });
            }
        }

        protected virtual void OnClickSaveMaterial(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = INPUTMTRL_GRID;
            DataRow[] drs = Util.gridGetChecked(ref dg, "CHK");

            if (drs == null)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }

            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU1984");  //투입자재 LOT ID를 입력하세요.
                    return;
                }

                if (!string.Equals(procId, Process.SRS_COATING))
                {
                    if (string.IsNullOrEmpty(dr["INPUT_QTY"].ToString()) || dr["INPUT_QTY"].ToString().Equals("0.00000"))
                    {
                        Util.MessageValidation("SFU1953");  //투입 수량을 입력하세요.
                        return;
                    }
                }
            }
            SetMaterial(_LOTID, "A");
        }

        protected virtual void OnClickCottonSave(object sender, RoutedEventArgs e)
        {
            SaveCotton(COTTON_GRID);
        }

        protected virtual void OnClickSolidContRate(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }
            CMM_HOPR_SCLE_SOLD_CONT_RATE _popUpSolid = new CMM_HOPR_SCLE_SOLD_CONT_RATE();
            _popUpSolid.FrameOperation = FrameOperation;

            if (_popUpSolid != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[1] = procId;

                C1WindowExtension.SetParameters(_popUpSolid, Parameters);

                _popUpSolid.Closed += new EventHandler(popUpSolid_Closed);
                _popUpSolid.ShowModal();
                _popUpSolid.CenterOnScreen();
            }
        }
        private void popUpSolid_Closed(object sender, EventArgs e)
        {
            CMM_HOPR_SCLE_SOLD_CONT_RATE runStartWindow = sender as CMM_HOPR_SCLE_SOLD_CONT_RATE;
            if (runStartWindow.DialogResult == MessageBoxResult.Cancel)
            {
                OnClickSearch(null, null);
            }
        }

        #region[POSTACTION]
        protected virtual void OnClickPostHoldSave(object sender, RoutedEventArgs e)
        {
            if (!ValidPostAction())
                return;

            DataTable postHold = DataTableConverter.Convert(POSTHOLD_GRID.ItemsSource);
            if (postHold.Select("POST_HOLD = 'True'").Length > 0 && postHold != null)
            {
                Util.MessageConfirm("SFU1345", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                        SavePostHold();
                    else
                        return;
                });
            }
            else
            {
                SavePostHold();
            }

        }
        #endregion

        protected virtual void OnClickDefetectFilter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            Grid grdDefectLVFilter = (grdDataCollect.Children[0] as UcDataCollect).grdDefectLVFilter as Grid;

            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                //CWA 불량등록 필터 그리드
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }
        }

        protected virtual void OnClickPublicSave(object sender, RoutedEventArgs e)
        {

            try
            {
                SaveDefect(WIPREASON_GRID);

                if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                    SaveDefect(WIPREASON2_GRID);

                if (_isRollMapEquipment) SaveDefectForRollMap();

                isChangeWipReason = false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
            {
                SavePublicCutQuality(QUALITY_GRID);
            }
            else
            {
                string status = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "WIPSTAT"));
                if (string.Equals(procId, Process.ROLL_PRESSING))
                {
                    if (status.Equals(Wip_State.PROC) || status.Equals(Wip_State.EQPT_END))
                    {
                        SavePublicQuality(COLOR_GRID);
                        isChangeColorTag = false;
                    }

                }
                SavePublicQuality(QUALITY_GRID);
            }
            //SetWipNote();
            if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)))
            {
                if (!ConfirmdPostAction())
                    return;
                SavePostHold();
            }
            else
            {
                SetWipNote();
            }
        }

        protected virtual void OnClickSaveRemark(object sender, RoutedEventArgs e)
        {
            SetWipNote();
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

        private void GetRemarkHistory(int iRow)
        {
            try
            {
                Util.gridClear(REMARK_HIST_GRID);

                String sLotID = String.Empty;
                if (Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID_PR")).Equals(""))
                    sLotID = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));
                else
                    sLotID = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID_PR"));

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["LOTID"] = sLotID;
                inDataTable.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_HISTORY_WIPNOTE", "INDATA", "RSLTDT", inDataTable);

                // 필요정보 변환
                System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                foreach (DataRow row in dtResult.Rows)
                {
                    strBuilder.Clear();
                    string[] wipNotes = Util.NVC(row["WIPNOTE"]).Split('|');

                    for (int i = 0; i < wipNotes.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(wipNotes[i]))
                        {
                            if (i == 0)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                            else if (i == 1)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                            else if (i == 2)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                            else if (i == 3)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                            else if (i == 4)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                            else if (i == 5)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                            strBuilder.Append("\n");
                        }
                    }
                    row["WIPNOTE"] = strBuilder.ToString();
                }
                Util.GridSetData(REMARK_HIST_GRID, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        protected virtual void OnClickEqptMaterialList(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM001.Popup.CMM_EQPT_MATERIAL wndPopup = new CMM001.Popup.CMM_EQPT_MATERIAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _LOTID;
                Parameters[1] = _WORKORDER;
                Parameters[2] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[3] = Util.NVC(EQUIPMENT_COMBO.Text);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnClickInputMaterial(object sender, RoutedEventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1702");  //실적 Lot을 선택 해 주세요.
                return;
            }

            if (!(string.Equals(procId, Process.SRS_MIXING) || string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.BS) || string.Equals(procId, Process.CMC) || string.Equals(procId, Process.InsulationMixing)) && string.IsNullOrEmpty(txtVersion.Text))
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return;
            }

            CMM001.Popup.CMM_INPUT_MATERIAL wndPopup = new CMM001.Popup.CMM_INPUT_MATERIAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = _LOTID;
                Parameters[1] = _WORKORDER;
                Parameters[2] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[3] = Util.NVC(EQUIPMENT_COMBO.Text);
                Parameters[4] = procId;
                Parameters[5] = _PRODID;
                Parameters[6] = Util.NVC(txtVersion.Text);
                Parameters[7] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(Material_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void Material_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_INPUT_MATERIAL window = sender as CMM001.Popup.CMM_INPUT_MATERIAL;

            if (procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing))
            {
                GetMaterialList(_LOTID, _PRODID);
            }
            else
            {
                GetMaterialSummary(_LOTID, _WORKORDER);
            }
        }

        private void SetWipNote()
        {
            try
            {
                if (REMARK_GRID.GetRowCount() < 1)
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable dt = ((DataView)REMARK_GRID.ItemsSource).Table;
                DataRow inData = null;
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    inData = inTable.NewRow();

                    inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                    if (REMARK_GRID.Rows[0].Visibility == Visibility.Visible)
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                    else
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                    inData["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(inData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    isChangeRemark = false;
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        protected virtual void OnClickSearchSlurry(object sender, RoutedEventArgs e)
        {
            GetSlurryData();
        }

        protected virtual void OnClickDeleteSlurry(object sender, RoutedEventArgs e)
        {
            SetSlurry(_LOTID, "D");
        }

        protected virtual void OnClickSaveSlurry(object sender, RoutedEventArgs e)
        {
            SetSlurry(_LOTID, "A");
        }

        protected virtual void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string vLotID = Util.NVC(DataTableConverter.GetValue(SLURRY_GRID.CurrentRow.DataItem, "LOTID"));
            string vPRODID = Util.NVC(DataTableConverter.GetValue(SLURRY_GRID.CurrentRow.DataItem, "PRODID"));

            if (SLURRY_INPUT_GRID.GetRowCount() > 1)
            {
                Util.MessageValidation("SFU1634");  //선택된 Slurry가 있습니다. 취소/삭제 후 작업하십시오
                return;
            }

            DataTable dt = ((DataView)SLURRY_INPUT_GRID.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_LOTID"] = vLotID;
            dr["PRODID"] = vPRODID;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        protected virtual void OnClickCellMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.Equals(procId, Process.COATING))
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.Equals(dataGrid.CurrentColumn.Name, "COUNTQTY"))
                    {
                        string sLinKCode = Util.NVC(DataTableConverter.GetValue(dataGrid.CurrentRow.DataItem, "LINK_DETL_RSN_CODE_TYPE"));

                        if (!string.IsNullOrEmpty(sLinKCode))
                        {
                            // POPUP 생성
                            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

                            if (iRow < 0)
                            {
                                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                                return;
                            }

                            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_COATING wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_COATING();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[8];
                                Parameters[0] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                                Parameters[1] = procId;
                                Parameters[2] = _LOTID;
                                Parameters[3] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));
                                Parameters[4] = string.Equals(dataGrid.Name, "dgWipReason") ? "DEFECT_TOP" : "DEFECT_BACK";
                                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dataGrid.CurrentRow.DataItem, "LINK_DETL_RSN_CODE_TYPE"));
                                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dataGrid.CurrentRow.DataItem, "RESNCODE"));
                                Parameters[7] = GetUnitFormatted();

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(OnCloseProcReasonCoating);
                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                    }
                }));
            }
        }

        private void OnCloseProcReasonCoating(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_COATING window = sender as LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_COATING;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!string.IsNullOrEmpty(window._ReturnResnCode) && !string.IsNullOrEmpty(window._ReturnPosition))
                {
                    C1DataGrid dataGrid = null;
                    if (string.Equals(window._ReturnPosition, "DEFECT_TOP"))
                        dataGrid = WIPREASON_GRID;
                    else if (string.Equals(window._ReturnPosition, "DEFECT_BACK"))
                        dataGrid = WIPREASON2_GRID;

                    if (dataGrid != null)
                    {
                        for (int i = 0; i < dataGrid.GetRowCount(); i++)
                        {
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNCODE"), window._ReturnResnCode))
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "COUNTQTY", window._ReturnDefectCount);
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", window._ReturnSumQty);
                                break;
                            }
                        }
                        GetSumDefectQty();
                        dataGrid.Refresh(false);
                        LOTINFO_GRID.Refresh(false);
                    }
                }
            }
        }

        private void OnDataCollectGridLoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (!_isRollMapEquipment) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e == null || e.Cell == null || e.Cell.Presenter == null) return;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9");

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals((string)DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    {
                        if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") ||
                         string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") ||
                         string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") ||
                         string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                        (string.Equals((string)DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals((string)DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    {
                        if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals((string)DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    {
                        if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    #region # RollMap ActivityReason
                    // RollMap용 수량 변경 금지 처리 [2021-07-27]
                    if (string.Equals(e.Cell.Column.Name, "RESNQTY") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                        if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    #endregion
                }

            }));
        }

        private void OnDataCollectGridUnLoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (!_isRollMapEquipment) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e != null && e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgWipReason2_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (!_isRollMapEquipment) return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = ((C1DataGrid)sender).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                        }
                                    }
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESN_TOT_CHK", false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                        }
                    }
                }
            }));
        }

        protected virtual void OnClickbtnExpandFrame(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualHeight != 0)
                ExpandFrame = Content.RowDefinitions[1].Height;
            if ((grdDataCollect.Children[0] as UcDataCollect).btnExpandFrame.IsChecked == true)
            {
                Content.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                Content.RowDefinitions[1].Height = ExpandFrame;
            }
        }

        protected virtual void OnClickbtnLeftExpandFrame(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualWidth != 0)
                ExpandFrame = Content.ColumnDefinitions[0].Width;
            if ((grdDataCollect.Children[0] as UcDataCollect).btnLeftExpandFrame.IsChecked == true)
            {
                Content.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Content.ColumnDefinitions[0].Width = ExpandFrame;
            }
        }

        protected virtual void OnClickSearchMergeData(object sender, RoutedEventArgs e)
        {
            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1381");    //Lot을 선택 하세요
                return;
            }

            GetMergeList();
            GetMergeEndList();
        }

        protected virtual void OnClickSaveMergeData(object sender, RoutedEventArgs e)
        {
            string sLotid = string.Empty ;              // [E20230807-000061] speical work-LOT MERGE improvement
            string sProceChkMsg = string.Empty;         // [E20230807-000061] speical work-LOT MERGE improvement  

            if (MERGE_GRID.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3628");   //합권취 진행할 대상 Lot들이 선택되지 않았습니다.
                return;
            }

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));
            dt2.Columns.Add("PR_LOTID", typeof(string));

            DataRow dataRow2 = dt2.NewRow();
            dataRow2["LOTID"] = Util.NVC(txtMergeInputLot.Text);

            dt2.Rows.Add(dataRow2);

            DataTable prodLotresult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt2);



            DataTable dt = ((DataView)MERGE_GRID.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sProceChkMsg = string.Empty;                                       // [E20230807-000061] speical work-LOT MERGE improvement 

                if (dt.Rows[i]["CHK"].ToString().Equals("1"))
                {
                    if (prodLotresult.Rows[0]["LANE_QTY"].ToString() != dt.Rows[i]["LANE_QTY"].ToString())
                    {
                        Util.MessageInfo("SFU5081");
                        return;
                    }
                    if (prodLotresult.Rows[0]["MKT_TYPE_CODE"].ToString() != dt.Rows[i]["MKT_TYPE_CODE"].ToString())
                    {
                        Util.MessageInfo("SFU4271");
                        return;
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[i]["WH_ID"].ToString()))
                    {
                        Util.MessageInfo("SFU2963");

                        if (!string.IsNullOrEmpty(dt.Rows[i]["ABNORM_FLAG"].ToString()) && dt.Rows[i]["ABNORM_FLAG"].ToString().Equals("Y"))
                        {
                            Util.MessageInfo("SFU7029");

                            return;
                        }

                    }

                    sLotid = Util.NVC(txtMergeInputLot.Text);                          // [E20230807-000061] speical work-LOT MERGE improvement 
                    sLotid = sLotid + "," + Util.NVC(dt.Rows[i]["LOTID"].ToString());  // [E20230807-000061] speical work-LOT MERGE improvement 
                }

                DataTable prodLotresult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt2);
                if (!string.IsNullOrEmpty(prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString()) && prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                {
                    Util.MessageValidation("SFU7029");  //전수불량레인이 존재하여 합권취 불가합니다.
                    return;
                }

                if (LOTINFO_GRID.GetRowCount() < 1)
                {
                    Util.MessageValidation("SFU1381");   //Lot을 선택 하세요
                    return;
                }

                if (!string.Equals(_WIPSTAT, "PROC"))
                {
                    Util.MessageValidation("SFU3627");  //합권취는 진행 상태에서만 가능합니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtMergeInputLot.Text))
                {
                    Util.MessageValidation("SFU1945");  //투입 LOT이 없습니다.
                    return;
                }

                C1DataGrid dg = MERGE_GRID;
                if (Util.gridGetChecked(ref dg, "CHK").Length <= 0)
                {
                    Util.MessageValidation("SFU3628");  //합권취 진행할 대상 Lot들이 선택되지 않았습니다.
                    return;
                }

                // [E20230807-000061] speical work-LOT MERGE improvement
                if (!string.IsNullOrEmpty(sLotid))
                {
                    sProceChkMsg = GetMergeLotProcIDChk(procId, sLotid);
                    if (!Util.NVC(sProceChkMsg).Equals("Y"))
                    {
                        if (!Util.NVC(sProceChkMsg).Equals("E"))
                        {
                            Util.MessageValidation("SFU9205", _ProNameMerge, sProceChkMsg); //  동일공정의 LOT이 아닙니다.
                            GetMergeList();
                        }
                        return;
                    }
                }
                 
            }

            SaveMergeData();
        }

        protected virtual void OnTabSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiWipReason"))
                    {
                        WIPREASON_GRID.EndEdit(true);
                        if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                            WIPREASON2_GRID.EndEdit(true);
                    }
                }
                #region [POSTACTION]
                // if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)) && (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EA") || string.Equals(LoginInfo.CFG_AREA_ID, "ED")))
                // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
                if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)) && (string.Equals(_RemarkHoldUseFlag, "Y")))
                {
                    if (!isChagneDefectTag || !isChangeWipReason || !isChangeQuality)
                    {
                        C1TabItem selitem = e.AddedItems[0] as C1TabItem;
                        if (string.Equals(selitem.Name, "tiPostHold"))
                        {
                            if (PRODLOT_GRID.ItemsSource != null && PRODLOT_GRID.Rows.Count > 0)
                            {
                                int idx = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

                                if (idx >= 0)
                                    BindPostHold(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[idx].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[idx].DataItem, "CUT_ID")));
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private void GetMergeList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                dataRow["PROCID"] = procId;
                dataRow["PRODID"] = _PRODID;
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_LIST_V01", "INDATA", "RSLTDT", dt);

                Util.GridSetData(MERGE_GRID, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetMergeEndList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                dataRow["PROCID"] = procId;
                dataRow["LOTID"] = Util.NVC(txtMergeInputLot.Text); 
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_END_LIST", "INDATA", "RSLTDT", dt);

                Util.GridSetData(MERGE2_GRID, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // [E20230807-000061] speical work-LOT MERGE improvement
        private string GetMergeLotProcIDChk(string sProcid, string sLotid)
        {
            string sChkResult = "Y";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID_MERGE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID_MERGE"] = sProcid;
                dr["LOTID"] = sLotid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_MERGE_LOT_PROCID_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ProNameMerge = Util.NVC(dtResult.Rows[0]["PROCNAME_MERGE"].ToString());

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (Util.NVC(sChkResult).Equals("Y"))
                        {
                            sChkResult = Util.NVC(dtResult.Rows[i]["CHK_MSG"].ToString());
                        }
                        else
                        {
                            sChkResult = sChkResult + "\r\n" + Util.NVC(dtResult.Rows[i]["CHK_MSG"].ToString());
                        }
                    }
                }
                else
                {
                    sChkResult = "Y";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sChkResult = "E";
            }

            return sChkResult;
        }

        private void SaveMergeData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["LOTID"] = Util.NVC(txtMergeInputLot.Text);
                        row["NOTE"] = string.Empty;
                        row["USERID"] = LoginInfo.USERID;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable formLotID = indataSet.Tables.Add("IN_FROMLOT");
                        formLotID.Columns.Add("FROM_LOTID", typeof(string));

                        DataTable dt = ((DataView)MERGE_GRID.ItemsSource).Table;
                        decimal iAddSumQty = 0;

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                row = formLotID.NewRow();

                                iAddSumQty += Util.NVC_Decimal(inRow["WIPQTY"]);
                                row["FROM_LOTID"] = Util.NVC(inRow["LOTID"]);
                                indataSet.Tables["IN_FROMLOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MERGE_LOT", "INDATA,IN_FROMLOT", null, indataSet);

                        Util.MessageInfo("SFU2009");    //합권되었습니다.
                        REFRESH = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void GetSlurryData()
        {
            try
            {
                DataTable _SlurryDT = new DataTable();
                Util.gridClear(SLURRY_GRID);

                DataTable topTable = new DataTable();
                topTable.Columns.Add("LANGID", typeof(string));
                topTable.Columns.Add("PROCID", typeof(string));
                topTable.Columns.Add("EQSGID", typeof(string));
                topTable.Columns.Add("WO_DETL_ID", typeof(string));
                topTable.Columns.Add("STDT", typeof(string));
                topTable.Columns.Add("EDDT", typeof(string));

                DataRow topdata = topTable.NewRow();
                topdata["LANGID"] = LoginInfo.LANGID;
                topdata["PROCID"] = Process.COATING;
                topdata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                topdata["WO_DETL_ID"] = WO_DETL_ID;
                topdata["STDT"] = DTPFROM.SelectedDateTime.ToString("yyyyMMdd");
                topdata["EDDT"] = DTPTO.SelectedDateTime.ToString("yyyyMMdd");
                topTable.Rows.Add(topdata);

                _SlurryDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_WIP", "INDATA", "RSLTDT", topTable);

                Util.GridSetData(SLURRY_GRID, _SlurryDT, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetInputSlurry(string LotID)
        {
            try
            {
                DataTable _SlurryDT = new DataTable();
                Util.gridClear(SLURRY_INPUT_GRID);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = LotID;
                inTable.Rows.Add(indata);

                _SlurryDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPMTRL_CT", "INDATA", "RSLTDT", inTable);

                Util.GridSetData(SLURRY_INPUT_GRID, _SlurryDT, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
        private string GetProcRemarkHoldUseFlag()
        {
            string sRemarkHoldUseFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "COM_TYPE_CODE";
            sCmCode = "REMARK_HOLD_USE_FLAG"; // 비고 HOLD 사용여부 

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
                    sRemarkHoldUseFlag = "Y";

                }
                else
                {
                    sRemarkHoldUseFlag = "N";
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                sRemarkHoldUseFlag = "N";
            }

            return sRemarkHoldUseFlag;

        }

        private void SetSlurry(string LotID, string PROC_TYPE)
        {
            if (SLURRY_GRID.Rows.Count < 1)
                return;

            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROC_TYPE", typeof(string));
                inDataTable.Columns.Add("INPUT_SEQNO", typeof(Int64));

                DataRow inData = null;

                DataTable dt = ((DataView)SLURRY_INPUT_GRID.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        inData = inDataTable.NewRow();
                        inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inData["EQPTID"] = _EQPTID;
                        inData["LOTID"] = LotID;
                        inData["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                        inData["MTRLID"] = Util.NVC(row["PRODID"]);
                        inData["PROD_LOTID"] = LotID;
                        inData["INPUT_QTY"] = 0;
                        inData["USERID"] = LoginInfo.USERID;
                        inData["PROC_TYPE"] = PROC_TYPE;
                        inData["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                        inDataTable.Rows.Add(inData);
                    }
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_IN_LOT", "INDATA", null, inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                    GetInputSlurry(LotID);
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        protected virtual void OnSelectedItemChangedEquipmentCombo(object sender, PropertyChangedEventArgs<object> e)
        {
            DataRowView drv = e.NewValue as DataRowView;

            if (drv != null)
            {
                // 코터 설비 선택 시 대lot Clear  [2016.12.06]
                if (procId.Equals(Process.SRS_COATING) || procId.Equals(Process.COATING))
                    LARGELOTID = string.Empty;

                // 요청으로 설비 선택 시 초기화 추가 [2017-03-17]
                ClearControls();
                InitWorkOrderCheck();

                if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
                    Util.gridClear(((UC_WORKORDER_CWA)grdWorkOrder.Children[0]).dgWorkOrder);
                else
                    Util.gridClear(((UC_WORKORDER_MX_CWA)grdWorkOrder.Children[0]).dgWorkOrder);

                Util.gridClear(LARGELOT_GRID);
                Util.gridClear(PRODLOT_GRID);

                // ON SEARCH
                GetWorkOrder();

                if (LARGELOT_GRID.Visibility == Visibility.Visible)
                    GetLargeLot();
                else
                    GetProductLot();

                // SHIFT_CODE, SHIFT_USER 자동 SET
                GetWrkShftUser();

                // COATER공정에서 CORE, SLURRY 조회, SRS코터에서 BATCH ID 자동 조회 및 SETUP
                if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                {
                    if (isSingleCoater)
                        GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));
                    else
                        GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), "");
                }

                if (string.Equals(procId, Process.ROLL_PRESSING))
                {
                    (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Text = string.Empty;
                    (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag = string.Empty;

                    if ((EQUIPMENT_COMBO.SelectedValue != null && EQUIPMENT_COMBO.SelectedValue.GetString() != "SELECT"))
                    {
                        GetWorkHalfSlittingSide();
                    }
                }
                //2022-03-30 오화백 : 슬리팅 공정에도 무지부 방향 보이도록 수정
                if (string.Equals(procId, Process.SLITTING))
                {
                    (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Text = string.Empty;
                    (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag = string.Empty;

                    if ((EQUIPMENT_COMBO.SelectedValue != null && EQUIPMENT_COMBO.SelectedValue.GetString() != "SELECT"))
                    {
                        GetWorkHalfSlittingSide();
                    }
                }
                SetIdentInfo();

                // 자동선택 추가
                SetLotAutoSelected();

                // Roll Map 대상 설비에 버튼 컨트롤 속성 설정
                SetRollMapEquipment();
            }
        }

        private void GetSlurry()
        {
            try
            {
                txtTank.Text = string.Empty;
                txtTank.Tag = string.Empty;
                txtBatchId.Text = string.Empty;
                txtBatchId.Tag = string.Empty;

                DataTable SlurryTable = new DataTable();
                SlurryTable.Columns.Add("EQPTID", typeof(string));

                DataRow Slurrydata = SlurryTable.NewRow();
                Slurrydata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                SlurryTable.Rows.Add(Slurrydata);

                DataTable _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_SRSTANK_CURR_MOUNT", "INDATA", "RSLTDT", SlurryTable);

                if (_dt.Rows.Count > 0)
                {
                    txtTank.Text = _dt.Rows[0]["EQPTNAME"].ToString();
                    txtTank.Tag = _dt.Rows[0]["EQPTID"].ToString();
                    txtBatchId.Text = _dt.Rows[0]["INPUT_LOTID"].ToString();
                }
                else
                {
                    txtTank.Text = string.Empty;
                    txtTank.Tag = string.Empty;
                    txtBatchId.Text = string.Empty;
                    txtBatchId.Tag = string.Empty;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        void SetSingleCoaterControl()
        {
            if (isSingleCoater)
            {
                if (COATTYPE_COMBO.SelectedValue.Equals("B"))
                {
                    txtInputQty = (grdResult.Children[0] as UcResult).txtInputQty;

                    for (int i = 0; i < (grdResult.Children[0] as UcResult).ResultDetail2.Children.Count; i++)
                    {
                        Panel p = (grdResult.Children[0] as UcResult).ResultDetail2.Children[i] as Panel;

                        if (p == null)  // Border추가
                            continue;

                        if (p.Children[1] is C1NumericBox)
                        {
                            C1NumericBox t = p.Children[1] as C1NumericBox;

                            if (t.Name.Equals("txtInputQty"))   // 추가
                                txtInputQty = t;
                            else if (t.Name.Equals("txtGoodQty"))   // 추가
                                txtGoodQty = t;
                            else if (t.Name.Equals("txtLossQty"))   // 추가
                                txtLossQty = t;

                            else if (t.Name.Equals("txtLaneQty"))
                                txtLaneQty = t;
                            else if (t.Name.Equals("txtWorkTime"))
                                txtWorkTime = t;
                        }
                        else if (p.Children[1] is TextBox)
                        {
                            TextBox t = p.Children[1] as TextBox;
                            if (t.Name.Equals("txtVersion"))
                                txtVersion = t;
                            else if (t.Name.Equals("txtWorkDate"))
                                txtWorkDate = t;
                            else if (t.Name.Equals("txtStartDateTime"))
                                txtStartDateTime = t;
                            else if (t.Name.Equals("txtEndDateTime"))
                                txtEndDateTime = t;
                        }
                        else if (p.Children[1] is CheckBox)
                        {
                            CheckBox t = p.Children[1] as CheckBox;
                            if (t.Name.Equals("chkFinalCut"))
                                chkFinalCut = t;
                        }
                    }

                    // 하단으로 추가로 인하여 위치 변경 [2017-01-19]
                    for (int i = 0; i < (grdResult.Children[0] as UcResult).CoaterSlurry2.Children.Count; i++)
                    {
                        Panel p = (grdResult.Children[0] as UcResult).CoaterSlurry2.Children[i] as Panel;

                        if (p == null)  // Border추가
                            continue;

                        if (p.Children[1] is TextBox)
                        {
                            TextBox t = p.Children[1] as TextBox;

                            if (t.Name.Equals("txtCore1")) // Core, Slurry 추가 [2017-01-12]
                                txtCore1 = t;
                            else if (t.Name.Equals("txtCore2"))
                                txtCore2 = t;
                            else if (t.Name.Equals("txtSlurry1"))
                            {
                                txtSlurry1 = t;

                                ((TextBlock)p.Children[0]).Text = "Slurry";
                            }
                            else if (t.Name.Equals("txtSlurry2"))
                            {
                                txtSlurry2 = t;
                                txtSlurry2.Visibility = Visibility.Collapsed;

                                ((TextBlock)p.Children[0]).Visibility = Visibility.Collapsed;
                                ((Button)p.Children[2]).Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < (grdResult.Children[0] as UcResult).ResultDetail.Children.Count; i++)
                    {
                        Panel p = (grdResult.Children[0] as UcResult).ResultDetail.Children[i] as Panel;

                        if (p == null)  // Border추가
                            continue;

                        if (p.Children[1] is C1NumericBox)
                        {
                            C1NumericBox t = p.Children[1] as C1NumericBox;

                            if (t.Name.Equals("txtInputQty"))
                                txtInputQty = t;
                            else if (t.Name.Equals("txtGoodQty"))   // 추가
                                txtGoodQty = t;
                            else if (t.Name.Equals("txtLossQty"))   // 추가
                                txtLossQty = t;
                            else if (t.Name.Equals("txtLaneQty"))
                                txtLaneQty = t;
                            else if (t.Name.Equals("txtWorkTime"))
                                txtWorkTime = t;
                        }
                        else if (p.Children[1] is TextBox)
                        {
                            TextBox t = p.Children[1] as TextBox;
                            if (t.Name.Equals("txtVersion"))
                                txtVersion = t;
                            else if (t.Name.Equals("txtWorkDate"))
                                txtWorkDate = t;
                            else if (t.Name.Equals("txtStartDateTime"))
                                txtStartDateTime = t;
                            else if (t.Name.Equals("txtEndDateTime"))
                                txtEndDateTime = t;
                        }
                        else if (p.Children[1] is CheckBox)
                        {
                            CheckBox t = p.Children[1] as CheckBox;
                            if (t.Name.Equals("chkFinalCut"))
                                chkFinalCut = t;
                        }
                    }

                    // 하단으로 추가로 인하여 위치 변경 [2017-01-19]
                    for (int i = 0; i < (grdResult.Children[0] as UcResult).CoaterSlurry.Children.Count; i++)
                    {
                        Panel p = (grdResult.Children[0] as UcResult).CoaterSlurry.Children[i] as Panel;

                        if (p == null)  // Border추가
                            continue;

                        if (p.Children[1] is TextBox)
                        {
                            TextBox t = p.Children[1] as TextBox;

                            if (t.Name.Equals("txtCore1")) // Core, Slurry 추가 [2017-01-12]
                                txtCore1 = t;
                            else if (t.Name.Equals("txtCore2"))
                                txtCore2 = t;
                            else if (t.Name.Equals("txtSlurry1"))
                            {
                                txtSlurry1 = t;

                                ((TextBlock)p.Children[0]).Text = "Slurry";
                            }
                            else if (t.Name.Equals("txtSlurry2"))
                            {
                                txtSlurry2 = t;
                                txtSlurry2.Visibility = Visibility.Collapsed;

                                ((TextBlock)p.Children[0]).Visibility = Visibility.Collapsed;
                                ((Button)p.Children[2]).Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnSelectedItemChangedCoatSideTypeCombo(object sender, PropertyChangedEventArgs<object> e)
        {
            if (isSingleCoater)
            {
                // 단면코터 조회 시 COAT SIDE 선택으로 나누어짐
                GetWorkOrder();

                DataRowView drv = e.NewValue as DataRowView;
                Grid grdWipReason = (grdDataCollect.Children[0] as UcDataCollect).grdWipReason as Grid;
                TextBlock tbWipReasonBack = (grdDataCollect.Children[0] as UcDataCollect).lblBack;
                grdWipReason.ColumnDefinitions.Clear();
                LARGELOTID = string.Empty;

                if (Util.NVC(DataTableConverter.GetValue(drv, "CBO_CODE")).Equals("B"))
                {
                    (grdResult.Children[0] as UcResult).ResultDetail.Visibility = Visibility.Collapsed;
                    (grdResult.Children[0] as UcResult).ResultDetail2.Visibility = Visibility.Visible;

                    // 단면코터 TOP이 아닐시 2개로 구분 (2017-01-09)
                    WIPREASON2_GRID.Visibility = Visibility.Visible;
                    tbWipReasonBack.Visibility = Visibility.Visible;

                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);

                    // 하단부로 변경되면서 Visible 처리 [2017-01-19]
                    (grdResult.Children[0] as UcResult).CoaterSlurry.Visibility = Visibility.Collapsed;
                    (grdResult.Children[0] as UcResult).CoaterSlurry2.Visibility = Visibility.Visible;

                    SetSingleCoaterControl();
                }
                else
                {
                    (grdResult.Children[0] as UcResult).ResultDetail.Visibility = Visibility.Visible;
                    (grdResult.Children[0] as UcResult).ResultDetail2.Visibility = Visibility.Collapsed;

                    // 단면코터 TOP일시 초기화 및 Lable 표시안함 [2017-01-09]
                    WIPREASON2_GRID.Visibility = Visibility.Collapsed;
                    tbWipReasonBack.Visibility = Visibility.Collapsed;

                    // 하단부로 변경되면서 Visible 처리 [2017-01-19]
                    (grdResult.Children[0] as UcResult).CoaterSlurry.Visibility = Visibility.Visible;
                    (grdResult.Children[0] as UcResult).CoaterSlurry2.Visibility = Visibility.Collapsed;
                    (grdDataCollect.Children[0] as UcDataCollect).tiRemarkHistory.Visibility = Visibility.Collapsed;

                    SetSingleCoaterControl();
                }
            }
            else
            {
                DataRowView drv = e.NewValue as DataRowView;
                Grid grdWipReason = (grdDataCollect.Children[0] as UcDataCollect).grdWipReason as Grid;
                TextBlock tbWipReasonBack = (grdDataCollect.Children[0] as UcDataCollect).lblBack;
                grdWipReason.ColumnDefinitions.Clear();

                if (Util.NVC(DataTableConverter.GetValue(drv, "CBO_CODE")).Equals("B"))
                {
                    WIPREASON2_GRID.Visibility = Visibility.Visible;
                    tbWipReasonBack.Visibility = Visibility.Visible;

                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);
                }
                else
                {
                    WIPREASON2_GRID.Visibility = Visibility.Collapsed;
                    tbWipReasonBack.Visibility = Visibility.Collapsed;
                }
            }
            ClearDetailControls();

            // Foil, Slurry 추가 [2017-01-12]
            if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            {
                txtCore1.Text = string.Empty;
                txtCore1.Tag = null;
                txtCore2.Text = string.Empty;
                txtCore2.Tag = null;
                txtSlurry1.Text = string.Empty;
                txtSlurry1.Tag = null;
                txtSlurry2.Text = string.Empty;
                txtSlurry2.Tag = null;
            }
            LARGELOT_GRID.ItemsSource = null;
            PRODLOT_GRID.ItemsSource = null;

            if (string.Equals(procId, Process.COATING))
                GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));

            // 자동선택 추가
            if (LARGELOT_GRID.Visibility == Visibility.Visible)
                GetLargeLot();
            else
                GetProductLot();

            SetLotAutoSelected();

        }

        protected virtual void On_rdoTop_Checked(object sender, RoutedEventArgs e)
        {
            COATTYPE_COMBO.SelectedValue = "T";
        }

        protected virtual void On_rdoBack_Checked(object sender, RoutedEventArgs e)
        {
            COATTYPE_COMBO.SelectedValue = "B";
        }

        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {
            // 버전정보 체크
            if (!ValidVersion()) return;

            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName = string.Empty;
            string processCode = string.Empty;
            string equipmentCode = string.Empty;
            string equipmentName = string.Empty;
            string lotCode = string.Empty;
            string wipSeq = string.Empty;

            if (string.Equals(procId, Process.ROLL_PRESSING))
            {
                if (string.Equals(WIPSTATUS, Wip_State.PROC))
                {
                    mainFormName = "COM001_RM_CHART_CT";
                    lotCode = string.Empty;
                    wipSeq = "1";
                    processCode = Process.COATING;
                    equipmentName = string.Empty;

                    DataTable dtRollMapInputLot = GetRollMapInputLotCode(_LOTID);

                    if (CommonVerify.HasTableRow(dtRollMapInputLot))
                    {
                        lotCode = dtRollMapInputLot.Rows[0]["INPUT_LOTID"].GetString();

                        DataTable dtEquipment = GetEquipmentCode(lotCode, wipSeq);
                        equipmentCode = CommonVerify.HasTableRow(dtEquipment) ? dtEquipment.Rows[0]["EQPTID"].GetString() : string.Empty;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    mainFormName = "COM001_RM_CHART_RP";
                    lotCode = _LOTID;
                    wipSeq = _WIPSEQ;
                    processCode = procId;
                    equipmentCode = _EQPTID;
                    equipmentName = EQUIPMENT_COMBO.Text;
                }
            }
            else if (string.Equals(procId, Process.COATING))
            {
                //CWA 전극 코터공정진척의 경우 ELEC002_002 파일을 사용함. 
                mainFormName = "COM001_RM_CHART_CT";
                lotCode = _LOTID;
                wipSeq = _WIPSEQ;
                processCode = procId;
                equipmentCode = _EQPTID;
                equipmentName = EQUIPMENT_COMBO.Text;
            }
            else if (string.Equals(procId, Process.SLITTING))
            {
                mainFormName = "COM001_RM_CHART_SL";
                lotCode = _LOTID;
                wipSeq = _WIPSEQ;
                processCode = procId;
                equipmentCode = _EQPTID;
                equipmentName = EQUIPMENT_COMBO.Text;
            }
            else if (string.Equals(procId, Process.SLIT_REWINDING) || string.Equals(procId, Process.REWINDING))
            {
                mainFormName = "COM001_RM_CHART_RW";
                lotCode = _LOTID;
                wipSeq = _WIPSEQ;
                processCode = procId;
                equipmentCode = _EQPTID;
                equipmentName = EQUIPMENT_COMBO.Text;
            }
            else
            {
                return;
                //mainFormName = "COM001_ROLLMAP_COMMON";
                //lotCode = _LOTID;
                //wipSeq = _WIPSEQ;
                //processCode = procId;
                //equipmentCode = _EQPTID;
                //equipmentName = EQUIPMENT_COMBO.Text;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = processCode;
            parameters[1] = EQUIPMENTSEGMENT_COMBO.SelectedValue;
            parameters[2] = equipmentCode;
            parameters[3] = lotCode;
            parameters[4] = wipSeq;
            parameters[5] = txtLaneQty.Value;
            parameters[6] = equipmentName;
            parameters[7] = txtVersion.Text.Trim();

            C1Window popupRollMap = obj as C1Window;
            popupRollMap.Closed += new EventHandler(popupRollMap_Closed);
            C1WindowExtension.SetParameters(popupRollMap, parameters);
            if (popupRollMap != null)
            {
                popupRollMap.ShowModal();
                popupRollMap.CenterOnScreen();
            }
        }

        private void popupRollMap_Closed(object sender, EventArgs e)
        {

        }

        protected virtual void OnGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLargeLot":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;

                                            LARGELOTID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID_LARGE"));
                                            WO_DETL_ID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WO_DETL_ID"));

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }

                                            ClearDetailControls();

                                            GetProductLot(LARGELOT_GRID.Rows[e.Cell.Row.Index].DataItem as DataRowView);

                                            ClearDefectLV();
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            ClearDetailControls();
                                            PRODLOT_GRID.ItemsSource = null;

                                            LARGELOTID = string.Empty;
                                            WO_DETL_ID = string.Empty;

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            ClearDefectLV();

                                        }

                                        break;
                                }

                                if (LARGELOT_GRID.CurrentCell != null)
                                    LARGELOT_GRID.CurrentCell = LARGELOT_GRID.GetCell(LARGELOT_GRID.CurrentCell.Row.Index, LARGELOT_GRID.Columns.Count - 1);
                                else if (LARGELOT_GRID.Rows.Count > 0)
                                    LARGELOT_GRID.CurrentCell = LARGELOT_GRID.GetCell(LARGELOT_GRID.Rows.Count, LARGELOT_GRID.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgProductLot":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;

                                            ClearDetailControls();

                                            if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                                return;

                                            WIPSTATUS = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT"));
                                            LOTID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID"));


                                            //2021-03-10 오화백 QA 샘플링 검사여부 체크
                                            if (procId.Equals(Process.ROLL_PRESSING) || procId.Equals(Process.SLITTING))
                                                Qa_Insp_Trgt_Flag = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "QA_INSP_TRGT_FLAG"));


                                            if (procId.Equals(Process.HALF_SLITTING))
                                                PRLOTID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID_PR"));

                                            PRODLOT_GRID.SelectedIndex = e.Cell.Row.Index;

                                            // LOT별 ROLLMAP 속성 구분 (2021-08-18)
                                            SetRollMapLotAttribute(LOTID);

                                            if (!WIPSTATUS.Equals("WAIT"))
                                            {
                                                GetLotInfo(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem);

                                                if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count + 1)
                                                    GetDefectList();
                                                else if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1)
                                                    GetDefectListMultiLane();

                                                GetQualityList(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem);

                                                if (procId.Equals(Process.SLITTING))
                                                {
                                                    GetCottonList(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem);
                                                }
                                                // 색지정보 T/P공정 추가 [2017-07-11]
                                                if (procId.Equals(Process.ROLL_PRESSING) || string.Equals(procId, Process.TAPING))
                                                    GetColorList(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem);

                                                // 코터 이후 설비에서는 BOTTOM ROW 추가 [2017-06-13]
                                                if (!string.Equals(procId, Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !string.Equals(procId, Process.MIXING) && !string.Equals(procId, Process.SRS_MIXING))
                                                    if (string.Equals(txtUnit.Text, "EA") && LOTINFO_GRID.BottomRows.Count == 0)
                                                        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                                                            if (LOTINFO_GRID.Rows[i].Visibility == Visibility.Visible)
                                                                LOTINFO_GRID.BottomRows.Add(new C1.WPF.DataGrid.DataGridRow());
                                            }

                                            if (procId.Equals(Process.COATING))
                                            {
                                                GetMaterial(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem, "LOTID")));
                                            }
                                            else if (procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing))
                                            {
                                                GetMaterialList(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem, "LOTID"))
                                                              , Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem, "PRODID")));
                                            }
                                            else if (procId.Equals(Process.MIXING) || procId.Equals(Process.PRE_MIXING))//MIXER전체공정 적용 확인 
                                            {
                                                GetMaterialSummary(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem, "WOID")));
                                            }

                                            if (!procId.Equals(Process.MIXING) && !procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !procId.Equals(Process.SRS_MIXING))
                                                GetRemarkHistory(e.Cell.Row.Index);

                                            if (string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING))
                                            {
                                                if (string.Equals(WIPSTATUS, Wip_State.EQPT_END) || string.Equals(WIPSTATUS, Wip_State.PROC))
                                                {
                                                    (grdSearch.Children[0] as UcSearch).btnRollMap.IsEnabled = true;
                                                }
                                                else
                                                {
                                                    (grdSearch.Children[0] as UcSearch).btnRollMap.IsEnabled = false;
                                                }
                                            }
                                            else if (string.Equals(procId, Process.COATING))
                                            {
                                                if (string.Equals(WIPSTATUS, Wip_State.EQPT_END))
                                                    (grdSearch.Children[0] as UcSearch).btnRollMap.IsEnabled = true;
                                                else
                                                    (grdSearch.Children[0] as UcSearch).btnRollMap.IsEnabled = false;
                                            }

                                            PRODLOT_GRID.SelectedIndex = e.Cell.Row.Index;

                                            ClearDefectLV();

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            ClearDetailControls();

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);

                                            ClearDefectLV();

                                        }
                                        break;
                                }

                                if (PRODLOT_GRID.CurrentCell != null)
                                    PRODLOT_GRID.CurrentCell = PRODLOT_GRID.GetCell(PRODLOT_GRID.CurrentCell.Row.Index, PRODLOT_GRID.Columns.Count - 1);
                                else if (PRODLOT_GRID.Rows.Count > 0)
                                    PRODLOT_GRID.CurrentCell = PRODLOT_GRID.GetCell(PRODLOT_GRID.Rows.Count, PRODLOT_GRID.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }
        protected void OnDefectCurrentChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex1 = e.Cell.Row.Index;

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                            {
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }
                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 1, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }

                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 1, true);
                                                        }
                                                    }

                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex1 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex1 = e.Cell.Row.Index;
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.CurrentCell != null)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.CurrentCell.Row.Index, (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Columns.Count - 1);
                                else if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows.Count > 0)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows.Count, (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex2 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }
                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                    Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 2, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();

                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }

                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 2, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex2 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex2 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.CurrentCell != null)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.CurrentCell.Row.Index, (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Columns.Count - 1);
                                else if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows.Count > 0)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows.Count, (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex3 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                    Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 3, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                    if (procId == Process.COATING)
                                                    {
                                                        if (WIPREASON2_GRID.ItemsSource != null)
                                                        {
                                                            DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 3, true);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex3 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex3 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (WIPREASON_GRID.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.CurrentCell != null)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.CurrentCell.Row.Index, (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Columns.Count - 1);
                                else if ((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows.Count > 0)
                                    (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.CurrentCell = (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.GetCell((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows.Count, (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }
        protected virtual void OnDataCollectGridCottonTagCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, COTTON_GRID.Columns[e.Cell.Column.Index].Name)).Equals(""))
            {
                isChangeCotton = true;
            }
        }

        protected virtual void OnDataCollectGridQualityCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02"));
                //sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-').Count() == 3)
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0] + "-" +
                        Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[1];
                }
                else
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                }
                sCLCNAME = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
            }
            else
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            }
            string sCode = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                    isChangeQuality = true;

                    // RP공정에서 범위 알림 기능 추가 [2017-06-06]
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                    {
                        DataTable dataCollect = DataTableConverter.Convert(caller.ItemsSource);
                        int iHGCount1 = 0;  // H/G
                        int iHGCount2 = 0;  // M/S
                        int iHGCount3 = 0;  // 1차 H/G
                        int iHGCount4 = 0;  // 1차 M/S
                        decimal sumValue1 = 0;
                        decimal sumValue2 = 0;
                        decimal sumValue3 = 0;
                        decimal sumValue4 = 0;
                        foreach (DataRow row in dataCollect.Rows)
                        {
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount3++;
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount4++;
                                }
                            }
                        }

                        if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 4)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                        else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 4)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                        else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 2)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 2)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 4)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                        else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 4)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                        else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 2)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 2)
                            Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                    }
                }

                if (string.Equals(e.Cell.Column.Name, "CLCTVAL02") && (procId.Equals(Process.COATING) || procId.Equals(Process.INS_COATING) || procId.Equals(Process.SRS_COATING)))
                {
                    if (QUALITY_GRID.Visibility == Visibility.Visible)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", null);
                        for (int i = 0; i < QUALITY_GRID.Rows.Count; i++)
                        {
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            string nCLCTITEM = DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString().Split('-')[0] + "-" +
                                DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString().Split('-')[1];

                            if (nCLCTITEM.Equals("E2000-0001") || nCLCTITEM.Equals("E2000-0002"))
                            {
                                if ((DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME1").ToString() + DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME2").ToString() + DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME3").ToString()).Equals(sCLCNAME))
                                {
                                    if (DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01").Equals(""))
                                        DataTableConverter.SetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01", "0");

                                    if (Convert.ToDouble(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01")) == 0)
                                    {
                                        double input = Convert.ToDouble(GetUnitFormatted(sValue)) * 0.5;
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", sValue);
                                        for (int j = 0; j < QUALITY_GRID.Rows.Count; j++)
                                        {
                                            if (DataTableConverter.GetValue(QUALITY_GRID.Rows[j].DataItem, "CLCTITEM").ToString().Equals(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString()))
                                            {
                                                Util.SetDataGridCurrentCell(QUALITY_GRID, QUALITY_GRID[j, 1]);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        double input = Convert.ToDouble(GetUnitFormatted(sValue)) - Convert.ToDouble(GetUnitFormatted(Convert.ToDouble(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01"))));
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", sValue);
                                        for (int j = 0; j < QUALITY_GRID.Rows.Count; j++)
                                        {
                                            if (DataTableConverter.GetValue(QUALITY_GRID.Rows[j].DataItem, "CLCTITEM").ToString().Equals(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString()))
                                            {
                                                Util.SetDataGridCurrentCell(QUALITY_GRID, QUALITY_GRID[j, 1]);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            else if (DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString().Split('-')[0].Equals("SI016") || DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString().Split('-')[0].Equals("SI017"))
                            {
                                if ((DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME1").ToString() + DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME2").ToString() + DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLSS_NAME3").ToString()).Equals(sCLCNAME))
                                {
                                    if (DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01").Equals(""))
                                        DataTableConverter.SetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01", "0");

                                    if (Convert.ToDouble(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01")) == 0)
                                    {
                                        double input = Convert.ToDouble(GetUnitFormatted(sValue)) * 0.5;
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", sValue);
                                        for (int j = 0; j < QUALITY_GRID.Rows.Count; j++)
                                        {
                                            if (DataTableConverter.GetValue(QUALITY_GRID.Rows[j].DataItem, "CLCTITEM").ToString().Equals(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString()))
                                            {
                                                Util.SetDataGridCurrentCell(QUALITY_GRID, QUALITY_GRID[j, 1]);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        double input = Convert.ToDouble(GetUnitFormatted(sValue)) - Convert.ToDouble(GetUnitFormatted(Convert.ToDouble(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01"))));
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", sValue);
                                        for (int j = 0; j < QUALITY_GRID.Rows.Count; j++)
                                        {
                                            if (DataTableConverter.GetValue(QUALITY_GRID.Rows[j].DataItem, "CLCTITEM").ToString().Equals(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTITEM").ToString()))
                                            {
                                                Util.SetDataGridCurrentCell(QUALITY_GRID, QUALITY_GRID[j, 1]);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                double input = Convert.ToDouble(GetUnitFormatted(sValue)) * 0.5;
                                DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);
                            }
                        }
                    }
                }
            }
            else if (e.Cell.Presenter != null)
            {
            }
        }

        protected virtual void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line);

                            iRowIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                isDupplicatePopup = true;
                int iRowIdx = 0;
                int iColIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;

                    if (p.Cell == null)
                        return;

                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;

                    grid = p.DataGrid;

                    C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);

                    // 자주검사 자동입력 기능 추가 [2020-12-19]
                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()) && currentCell != null)
                    {
                        /*
                        DataRow[] searchRow = DataTableConverter.Convert(grid.ItemsSource).Select(string.Format("TARGET_INSP_ID = '{0}' AND INSP_PRDT_TYPE_CODE = '{1}' AND INQ_LANE_NO = {2}",
                            new object[] { DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_ITEM_ID"),
                                DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_PRDT_TYPE_CODE"),
                                DataTableConverter.GetValue(currentCell.Row.DataItem, "INQ_LANE_NO") }));

                        if (searchRow != null && searchRow.Length > 0)
                        {
                            string sTargetID = Util.NVC(searchRow[0]["TARGET_INSP_ID"]);
                            string sExpression = Util.NVC(searchRow[0]["FUNC_EXP"]);
                            string sCondition = Util.NVC(searchRow[0]["FUNC_COND"]);

                            DataRow[] targetRow = DataTableConverter.Convert(grid.ItemsSource).Select(string.Format("INSP_ITEM_ID = '{0}' AND INSP_PRDT_TYPE_CODE = '{1}' AND INQ_LANE_NO = {2}",
                                new object[] { sTargetID, DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_PRDT_TYPE_CODE"), DataTableConverter.GetValue(currentCell.Row.DataItem, "INQ_LANE_NO") }));

                            if (targetRow.Length > 0)
                                DataTableConverter.SetValue(grid.Rows[Util.getGridDataRowIndex(grid, searchRow[0])].DataItem, "CLCTVAL01", Util.GetSelfInspectionValue(targetRow, sExpression, sCondition));
                        }
                        */
                        //[2021-11-12 김대근 수정 / TARGET_INSP_ID가 쉼표로 구분되어 등록되는 것에 대응]
                        DataRow[] allRows = DataTableConverter.Convert(grid.ItemsSource).Select();
                        List<DataRow> searchRow = new List<DataRow>();
                        foreach (DataRow allRowsDr in allRows)
                        {
                            string targetInspId = allRowsDr["TARGET_INSP_ID"].ToString();
                            string inspPrdtTypeCode = allRowsDr["INSP_PRDT_TYPE_CODE"].ToString();
                            string inqLaneNo = allRowsDr["INQ_LANE_NO"].ToString();
                            string[] targetInspIdSplit = targetInspId.Split(new char[] { ',' });
                            if (targetInspIdSplit.Contains(Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_ITEM_ID"))) &&
                                inspPrdtTypeCode.Equals(Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_PRDT_TYPE_CODE"))) &&
                                inqLaneNo.Equals(Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "INQ_LANE_NO"))))
                            {
                                searchRow.Add(allRowsDr);
                            }
                        }

                        if (searchRow != null && searchRow.Count > 0)
                        {
                            string sTargetID = Util.NVC(searchRow[0]["TARGET_INSP_ID"]);
                            string sExpression = Util.NVC(searchRow[0]["FUNC_EXP"]);
                            string sCondition = Util.NVC(searchRow[0]["FUNC_COND"]);

                            string sTargetIDForInClause = string.Empty;
                            string[] sTargetIDSplit = sTargetID.Split(new char[] { ',' });
                            if (sTargetIDSplit.Length > 1)
                            {
                                for (int i = 0; i < sTargetIDSplit.Length; i++)
                                {
                                    sTargetIDSplit[i] = "'" + sTargetIDSplit[i] + "'";
                                }
                                sTargetIDForInClause = string.Join(",", sTargetIDSplit);
                            }
                            else
                            {
                                sTargetIDForInClause = sTargetID;
                            }

                            DataRow[] targetRow = DataTableConverter.Convert(grid.ItemsSource).Select(string.Format("INSP_ITEM_ID IN ({0}) AND INSP_PRDT_TYPE_CODE = '{1}' AND INQ_LANE_NO = {2}",
                                new object[] { sTargetIDForInClause, DataTableConverter.GetValue(currentCell.Row.DataItem, "INSP_PRDT_TYPE_CODE"), DataTableConverter.GetValue(currentCell.Row.DataItem, "INQ_LANE_NO") }));

                            if (targetRow.Length > 0)
                                DataTableConverter.SetValue(grid.Rows[Util.getGridDataRowIndex(grid, searchRow[0])].DataItem, "CLCTVAL01", Util.GetSelfInspectionValue(targetRow, sExpression, sCondition));
                        }
                    }

                    string sLSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "LSL"));
                    string sUSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "USL"));

                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        if (sLSL != "" && Util.NVC_Decimal(item.Value) < Util.NVC_Decimal(sLSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        else if (sUSL != "" && Util.NVC_Decimal(item.Value) > Util.NVC_Decimal(sUSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            currentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        isChangeQuality = true;
                    }

                    if (string.Equals(procId, Process.ROLL_PRESSING) && !string.IsNullOrEmpty(Util.NVC(item.Value)) && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        DataTable dataCollect = DataTableConverter.Convert(grid.ItemsSource);
                        int iHGCount1 = 0;  // H/G
                        int iHGCount2 = 0;  // M/S
                        int iHGCount3 = 0;  // 1차 H/G
                        int iHGCount4 = 0;  // 1차 M/S
                        decimal sumValue1 = 0;
                        decimal sumValue2 = 0;
                        decimal sumValue3 = 0;
                        decimal sumValue4 = 0;
                        foreach (DataRow row in dataCollect.Rows)
                        {
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount3++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount4++;
                                    }
                                }
                            }
                        }

                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount3 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount4 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }

                        if (grid.BottomRows.Count > 0)
                            grid.BottomRows[0].Refresh(false);
                    }
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    isChangeQuality = true;
                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                isDupplicatePopup = false;
            }
        }

        protected virtual void OnDataCollectGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (_isRollMapEquipment && (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)))
                    {
                        if (dataGrid.Name.Contains("WipReason") && string.Equals(DataTableConverter.GetValue(dataGrid.CurrentCell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                            return;
                    }

                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        if (!_isRollMapEquipment)
                            DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }

        private void DefectChange(C1DataGrid dg, int iRow, int iCol)
        {
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = isResnCountUse == true ? 1 : 0;

            if (iCol == dg.Columns["ALL"].Index)
            {
                // 소형 전극은 SUM으로 분배 [2017-01-24]
                int iLaneQty = 0;

                for (int i = 1; i < LANENUM_COMBO.Items.Count; i++)
                    if (((CheckBox)LANENUM_COMBO.Items[i]).IsChecked == true)
                        iLaneQty++;

                if (iLaneQty == 0)
                    return;

                decimal iTarget = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                decimal iLastCol = iTarget % iLaneQty;

                if (dg.Columns.Contains("PERMIT_RATE"))
                    iCol++;

                for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i++)
                {
                    if ((i + iCol) % (2 + iCount) == 0) // ytkim29 수정  (15 + 13) % (2 + 15) 
                    {
                        if (dg.Columns[i].Visibility == Visibility.Collapsed)
                            continue;

                        string _ValueToFind = string.Empty;

                        // 길이초과, 길이부족의 경우는 SUM도 분배가 아닌 일괄 등록
                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK") ||
                            string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                        {
                            _ValueToFind = Util.NVC(iTarget);
                        }
                        else
                        {
                            if (string.Equals(dg.Columns["ALL"].GetColumnText(), ObjectDic.Instance.GetObjectName("SUM")))
                                _ValueToFind = Util.NVC(Math.Truncate(iTarget / iLaneQty) + (iLastCol > 1 ? 1 : iLastCol));
                            else
                                _ValueToFind = Util.NVC(iTarget);
                        }

                        // 공정 진행 중 불량은 ALL/SUM SETUP시 포함하여 저장 [2017-07-21]
                        if (procResnDt != null && procResnDt.Rows.Count > 0)
                        {
                            DataRow[] rows = procResnDt.Select("LOTID = '" + dg.Columns[i].Name + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "RESNCODE")) + "'");
                            if (rows != null && rows.Length > 0)
                                _ValueToFind = Util.NVC(Util.NVC_Decimal(_ValueToFind) + Util.NVC_Decimal(rows[0]["RESNQTY"]));
                        }
                        #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                        if (!dg.Columns[i].IsReadOnly)
                        {
                            DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name, _ValueToFind);
                        }
                            
                        #endregion
                        bool isDefectValid = true;
                        if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                            isDefectValid = GetSumCutDefectQty(dg, iRow, i);
                        else
                            isDefectValid = GetSumDefectQty(dg, iRow, i);

                        if (isDefectValid == false)
                            continue;

                        iLastCol = iLastCol > 1 ? iLastCol - 1 : 0;
                    }
                }
            }
            //else if (iCol >= dg.Columns["ALL"].Index + 1 && iCol % 2 != 0)
            else if (iCol >= dg.Columns["ALL"].Index + 1 && (iCol - (dg.Columns["ALL"].Index - 1)) % (2 + iCount) == 1)
            {
                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                    GetSumCutDefectQty(dg, iRow, iCol);
                else
                    GetSumDefectQty(dg, iRow, iCol);
            }
            //else if (iCol >= dg.Columns["ALL"].Index + 1  && iCol % 2 == 0)
            else if (iCol >= dg.Columns["ALL"].Index + 1 && (iCol - (dg.Columns["ALL"].Index - 1)) % (2 + iCount) == 0)
            {
                // 태그 수 불량/LOSS 자동 반영 로직 적용(C20190404_67447) [2019-04-11]
                if (string.Equals(procId, Process.SLITTING))
                {
                    decimal dConvertValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_CONV_RATE"));
                    if (dConvertValue > 0)
                    {
                        decimal dInputTagValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                        decimal dInputDefectValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol + 1].Name));
                        decimal dPreInputDefectValue = Util.NVC_Decimal(dtWipReasonBak.Rows[iRow][dg.Columns[iCol].Name]) * dConvertValue;
                        //bool isSumValue = dInputDefectValue == 0 ? true : false;
                        bool isSumValue = true;

                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_ALL_APPLY_FLAG"), "Y"))
                        {
                            for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                            {
                                if (Util.NVC_Decimal(Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name))) != 0 &&
                                    Util.NVC_Decimal(Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name))) != dPreInputDefectValue)
                                {
                                    isSumValue = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dInputDefectValue != 0 && dInputDefectValue != dPreInputDefectValue)
                                isSumValue = false;
                        }

                        /*
                        if ( string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_ALL_APPLY_FLAG"), "Y") && isSumValue == false)
                        {                           
                            for (int i = dg.Columns["ALL"].Index + 2; i < dg.Columns["COSTCENTERID"].Index; i +=2)
                            {
                                if (Convert.ToDouble(Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name))) != 0)
                                {
                                    isSumValue = false;
                                    break;
                                }
                            }
                        }
                        */

                        if (isSumValue == true)
                        {
                            if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_ALL_APPLY_FLAG"), "Y"))
                            {
                                for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                                {
                                    #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                                    if (!dg.Columns[i].IsReadOnly)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name, (dInputTagValue * dConvertValue));
                                        DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i - 2].Name, (dInputTagValue));

                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                                if (!dg.Columns[iCol + 1].IsReadOnly)
                                    DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol + 1].Name, (dInputTagValue * dConvertValue));
                                #endregion
                            }
                            GetSumCutDefectQty(dg, iRow, iCol + 1);
                        }
                    }
                }

                /*
                double dTagValue = Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                double dPitch = Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_CONV_RATE"));

                DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol + 1].Name, dTagValue * dPitch);

                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                    GetSumCutDefectQty(dg, iRow, iCol + 1);
                else
                    GetSumDefectQty(dg, iRow, iCol + 1);
                */
            }
            //SetParentRemainQty();

            // 값 변환때마다 갱신(C20190404_67447) [2019-04-11]
            if (string.Equals(procId, Process.SLITTING))
                dtWipReasonBak = DataTableConverter.Convert(dg.ItemsSource);
        }

        private void GetSumDefectQty()
        {
            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count + 1)
            {
                defectQty = SumDefectQty("DEFECT_LOT");
                LossQty = SumDefectQty("LOSS_LOT");
                chargeQty = SumDefectQty("CHARGE_PROD_LOT");

                totalSum = defectQty + LossQty + chargeQty;

                DataTable dt = (LOTINFO_GRID.ItemsSource as DataView).Table;

                for (int i = LOTINFO_GRID.TopRows.Count; i < dt.Rows.Count + LOTINFO_GRID.TopRows.Count; i++)
                {
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_LOSS", LossQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY", totalSum);

                    if (_isRollMapEquipment)
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                    }
                    else
                    {
                        //이병렬책임 요청 [2019-07-09]
                        if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                        else if (Util.NVC(txtInputQty.Tag).Equals("C"))
                            //DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) + totalSum);
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                        else
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) + totalSum);

                        if (txtLaneQty != null && !string.Equals(procId, Process.HALF_SLITTING))
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * txtLaneQty.Value);
                        else
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * laneqty);
                    }
                }

                if (isCoaterAfterProcess)
                    SetParentRemainQty();
            }
        }

        private void GetSumDefectQty(C1DataGrid dg)
        {
            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count + 1)
            {
                defectQty = SumDefectQty("DEFECT_LOT");
                LossQty = SumDefectQty("LOSS_LOT");
                chargeQty = SumDefectQty("CHARGE_PROD_LOT");

                totalSum = defectQty + LossQty + chargeQty;

                DataTable dt = (LOTINFO_GRID.ItemsSource as DataView).Table;

                for (int i = LOTINFO_GRID.TopRows.Count; i < dt.Rows.Count + LOTINFO_GRID.TopRows.Count; i++)
                {
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_LOSS", LossQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY", totalSum);

                    if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                    else
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) + totalSum);

                    laneqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY"));

                    if (txtLaneQty != null && !string.Equals(procId, Process.HALF_SLITTING))
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * txtLaneQty.Value);
                    else
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * laneqty);
                }
            }
        }

        private bool GetSumDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            string subLotId = dg.Columns[colIdx].Name;
            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));
            double inputQty = 0;
            double actSum = 0;
            double rowSum = 0;
            double totalSum = 0;
            int iLotIdx = Util.gridFindDataRow(ref dgLotInfo, "LOTID", subLotId, false);

            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = isResnCountUse == true ? 1 : 0;

            inputQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "INPUTQTY"));

            for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));

            actSum = SumDefectQty(dg, colIdx, subLotId, actId);

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "DTL_DEFECT"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "DTL_LOSS"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "DTL_CHARGEPRD"));

            /*
            if (inputQty < totalSum)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1608"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //생산량 보다 불량이 크게 입력될 수 없습니다.
                DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, dg.Columns[colIdx].Name, null);
                return false;
            }
            */

            DataTableConverter.SetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DTL_DEFECT" : string.Equals(actId, "LOSS_LOT") ? "DTL_LOSS" : "DTL_CHARGEPRD", actSum);
            DataTableConverter.SetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "LOSSQTY", totalSum);
            DataTableConverter.SetValue(LOTINFO_GRID.Rows[iLotIdx].DataItem, "GOODQTY", inputQty - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            return true;
        }

        private bool GetSumCutDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));
            double inputQty = 0;
            double actSum = 0;
            double totalSum = 0;
            double rowSum = 0;
            double laneQty = 0;

            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = isResnCountUse == true ? 1 : 0;

            laneQty = (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) - LOTINFO_GRID.TopRows.Count;
            inputQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

            for (int i = dg.Columns["ALL"].Index + (2 + iCount); i < dg.Columns["COSTCENTERID"].Index; i += (2 + iCount))
            {
                /*
                if (inputQty < SumDefectQty(dg, dg.Columns[i].Name))
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1608"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //생산량 보다 불량이 크게 입력될 수 없습니다.
                    DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, dg.Columns[colIdx].Name, null);
                    return false;
                }
                */

                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));
                actSum += SumDefectQty(dg, i, dg.Columns[i].Name, actId);
            }

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_DEFECT"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_LOSS"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_CHARGEPRD"));

            DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DTL_DEFECT" : string.Equals(actId, "LOSS_LOT") ? "DTL_LOSS" : "DTL_CHARGEPRD", actSum);
            DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY", totalSum);
            DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY", (inputQty * laneQty) - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            return true;
        }

        private void SetCauseTitle()
        {
            int causeqty = 0;

            if (WIPREASON_GRID.ItemsSource != null)
            {
                DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;
                for (int i = WIPREASON_GRID.TopRows.Count; i < dt.Rows.Count + WIPREASON_GRID.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }
                if (causeqty > 0)
                {
                    if (procId.Equals("E1000"))
                    {
                        TextBlock tbTop = (grdDataCollect.Children[0] as UcDataCollect).lblTop;
                        tbTop.Margin = new Thickness(300, 0, 8, 0);
                        tbTop.Visibility = Visibility.Collapsed;
                    }
                    if ((grdDataCollect.Children[0] as UcDataCollect).lblTop.Visibility == Visibility.Visible)
                    {
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Text = ObjectDic.Instance.GetObjectName("Top(*는 타공정 귀속)");
                    }
                    else
                    {
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Visibility = Visibility.Visible;
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Text = ObjectDic.Instance.GetObjectName("(*는 타공정 귀속)");
                    }
                }
            }
            if (WIPREASON2_GRID.ItemsSource != null)
            {
                DataTable dt = (WIPREASON2_GRID.ItemsSource as DataView).Table;
                for (int i = WIPREASON2_GRID.TopRows.Count; i < dt.Rows.Count + WIPREASON2_GRID.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }
                if (causeqty > 0)
                    (grdDataCollect.Children[0] as UcDataCollect).lblBack.Text = ObjectDic.Instance.GetObjectName("Back(*는 타공정 귀속)");
            }
        }

        private bool ValidateDefect(C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return false;
            }

            // 길이초과 입력 시 반영 안해줌
            if (isCoaterAfterProcess)
            {
                if (string.Equals(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                {
                    // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                    int iCount = isResnCountUse == true ? 1 : 0;

                    decimal inputQty = 0;
                    decimal inputLengthQty = 0;

                    if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                        inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "ALL"));
                    else
                        inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "RESNQTY"));

                    if (inputLengthQty > 0)
                    {
                        inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

                        if (Util.NVC_Decimal(txtParentQty.Value) > inputQty)
                        {
                            Util.MessageValidation("SFU3424");  // FINAL CUT이 아닌 경우 길이초과 입력 불가
                            DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);

                            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                                for (int i = WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount); i < WIPREASON_GRID.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                                    DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.Columns[i].Name, 0);

                            exceedLengthQty = 0;
                            return false;
                        }

                        if (inputLengthQty > (inputQty - Util.NVC_Decimal(txtParentQty.Value)))
                        {
                            Util.MessageValidation("SFU3422", (inputQty - Util.NVC_Decimal(txtParentQty.Value)) + txtUnit.Text);    // 길이초과수량을 초과하였습니다.[현재 실적에서 길이초과는 %1까지 입력 가능합니다.] 
                            DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);

                            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                                for (int i = WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount); i < WIPREASON_GRID.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                                    DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.Columns[i].Name, 0);

                            exceedLengthQty = 0;
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        protected virtual void OnKeyDownInputQty(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (LOTINFO_GRID.GetRowCount() < 1)
                        return;

                    if (string.Equals(procId, Process.COATING))
                    {
                        txtInputQty.Tag = "C,GoodQty input";


                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("OUTPUTQTY", typeof(string));

                        DataRow inDataRow = null;
                        inDataRow = inDataTable.NewRow();
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["OUTPUTQTY"] = txtInputQty.Value;
                        inDataTable.Rows.Add(inDataRow);

                        DataSet dsRlt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MIN_OUTQTY", "INDATA", null, inDataSet);

                        if (txtInputQty.Value == 0)
                        {
                            //양품량이 0으로 입력하시겠습니까?
                            Util.MessageConfirm("SFU6033", (Cresult) =>
                            {
                                if (Cresult != MessageBoxResult.OK)
                                    return;
                                else
                                {
                                    SetResultInputQTY();
                                }
                            }
                            );
                        }
                        else
                        {
                            SetResultInputQTY();
                        }
                    }
                    else
                    {
                        _isProdQtyInput = true;
                        SetResultInputQTY();
                        _isProdQtyInput = false;
                    }
                }
            }
            catch (Exception ex)
            {
                txtInputQty.Value = 0;
                Util.MessageException(ex);
            }
        }

        protected virtual void OnKeyLostInputQty(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isChangeInputFocus == false && txtInputQty.Value > 0)
                OnKeyDownInputQty(txtInputQty, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        protected virtual void OnKeyDownMesualQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (LOTINFO_GRID.GetRowCount() < 1)
                    return;

                C1NumericBox numericBox = sender as C1NumericBox;
                if (numericBox != null)
                {
                    for (int i = 0; i < QUALITY_GRID.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(QUALITY_GRID.Rows[i].DataItem, "CLCTVAL01", numericBox.Value);

                        decimal iUSL = Util.NVC_Decimal(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "USL"));
                        decimal iLSL = Util.NVC_Decimal(DataTableConverter.GetValue(QUALITY_GRID.Rows[i].DataItem, "LSL"));

                        if (iLSL == 0 && iUSL == 0)
                            continue;

                        if (QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter != null)
                        {
                            if (iLSL > Util.NVC_Decimal(numericBox.Value) || (iUSL > 0 && iUSL < Util.NVC_Decimal(numericBox.Value)))
                            {
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                QUALITY_GRID.GetCell(i, QUALITY_GRID.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    isChangeQuality = true;
                    numericBox.Value = 0;
                }
            }
        }

        protected virtual void OnKeyLostMesualQty(object sender, System.Windows.RoutedEventArgs e)
        {
            C1NumericBox numericBox = sender as C1NumericBox;

            if (numericBox != null)
                if (numericBox.Value > 0)
                    OnKeyDownMesualQty(numericBox, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        protected virtual void OnKeyDownOutputQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInputQty.Value <= 0)
                {
                    Util.MessageValidation("SFU1611");  //생산량을 확인하십시오.
                    txtInputQty.Value = 0;
                    txtInputQty.Focus();
                    return;
                }
                txtGoodQty.Value = txtInputQty.Value - txtLossQty.Value;
                SetInputQty();
            }
        }

        protected virtual void OnKeyDownInOutQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                C1NumericBox numericBox = sender as C1NumericBox;

                if (numericBox.Value > 0)
                {
                    txtInputQty.Value = txtSrs1Qty.Value + txtSrs2Qty.Value + txtSrs3Qty.Value;
                    OnKeyDownInputQty(txtInputQty, e);
                }
            }
        }

        private void OnKeyLostFocusInOutQty(object sender, System.Windows.RoutedEventArgs e)
        {
            C1NumericBox numericBox = sender as C1NumericBox;

            if (numericBox.Value > 0)
            {
                txtInputQty.Value = txtSrs1Qty.Value + txtSrs2Qty.Value + txtSrs3Qty.Value;
                OnKeyDownInputQty(txtInputQty, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
            }
        }

        protected virtual void OnKeyDownGoodQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtVersion.Text) || string.IsNullOrEmpty(txtLaneQty.Value.ToString()) || string.IsNullOrEmpty(txtLanePatternQty.Value.ToString()))
                {
                    Util.MessageValidation("SFU1563");  //버전 정보를 지정하세요.
                    txtGoodQty.Value = 0;
                    return;
                }

                if (txtGoodQty.Value <= 0)
                {
                    Util.MessageValidation("SFU1611");  //생산량을 확인하십시오.
                    txtGoodQty.Value = 0;
                    txtGoodQty.Focus();
                    return;
                }
                txtInputQty.Value = txtGoodQty.Value + txtLossQty.Value;
                SetGoodQty();
            }
        }

        protected virtual void OnKeyDownLaneQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (LOTINFO_GRID.GetRowCount() > 0)
                {
                    if (Util.NVC_Decimal(_LANEQTY) != Util.NVC_Decimal(txtLaneQty.Value))
                    {
                        if (WIPREASON_GRID.Rows.Count > 0)
                            SaveDefect(WIPREASON_GRID);

                        if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Rows.Count > 0)
                            SaveDefect(WIPREASON2_GRID);

                        _LANEQTY = Util.NVC(txtLaneQty.Value);

                        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY", txtLaneQty.Value);

                        GetSumDefectQty();
                    }
                }
            }
        }

        protected virtual void OnKeyLostFocusLaneQty(object sender, RoutedEventArgs e)
        {
            if (LOTINFO_GRID.GetRowCount() > 0)
            {
                if (Util.NVC_Decimal(_LANEQTY) != Util.NVC_Decimal(txtLaneQty.Value))
                {
                    if (WIPREASON_GRID.Rows.Count > 0)
                        SaveDefect(WIPREASON_GRID);

                    if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Rows.Count > 0)
                        SaveDefect(WIPREASON2_GRID);

                    _LANEQTY = Util.NVC(txtLaneQty.Value);

                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY", txtLaneQty.Value);

                    GetSumDefectQty();
                }
            }
        }

        protected virtual void OnClickVersion(object sender, EventArgs e)
        {
            CMM_ELECRECIPE wndPopup = new CMM_ELECRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null && PRODLOT_GRID.SelectedIndex > -1 && (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "PRODID"));
                Parameters[1] = procId;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "LOTID"));
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseVersion);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnCloseVersion(object sender, EventArgs e)
        {
            CMM_ELECRECIPE window = sender as CMM_ELECRECIPE;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (Util.NVC_Decimal(txtLaneQty.Value) != Util.NVC_Decimal(window._ReturnLaneQty))
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(window._ReturnLaneQty);
                    txtLanePatternQty.Value = Convert.ToDouble(window._ReturnPtnQty);

                    if (WIPREASON_GRID.Rows.Count > 0)
                        SaveDefect(WIPREASON_GRID);

                    if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Rows.Count > 0)
                        SaveDefect(WIPREASON2_GRID);

                    _LANEQTY = Util.NVC(txtLaneQty.Value);

                    #region #전수불량 Lane등록
                    txtCurLaneQty.Value = getCurrLaneQty(_LOTID, procId);
                    #endregion
                }
                else
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(window._ReturnLaneQty);
                    txtLanePatternQty.Value = Convert.ToDouble(window._ReturnPtnQty);
                }

                if (LOTINFO_GRID.GetRowCount() > 0)
                {
                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY", txtLaneQty.Value);
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_PTN_QTY", txtLanePatternQty.Value);
                    }
                }
                GetSumDefectQty();
            }
        }

        protected virtual void OnClickShift(object sender, EventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[3] = procId;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }

        protected virtual void OnClickBatchId(object sender, EventArgs e)
        {
            CMM_SRSTANK _Tank = new CMM_SRSTANK();
            _Tank.FrameOperation = FrameOperation;

            if (_Tank != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Util.NVC(EQUIPMENT_COMBO.SelectedValue.ToString());
                C1WindowExtension.SetParameters(_Tank, Parameters);

                _Tank.Closed += new EventHandler(OnCloseBatchId);
                _Tank.ShowModal();
                _Tank.CenterOnScreen();
            }
        }

        protected virtual void OnCloseBatchId(object sender, EventArgs e)
        {
            CMM_SRSTANK window = sender as CMM_SRSTANK;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtTank.Text = window._ReturnTankName;
                txtTank.Tag = window._ReturnTank;
                txtBatchId.Text = window._ReturnLotID;
            }
        }

        protected virtual void OnClickSaveColorTag(object sender, EventArgs e)
        {
            string status = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "WIPSTAT"));

            if (status.Equals(Wip_State.PROC) || status.Equals(Wip_State.EQPT_END))
            {
                SaveQuality(COLOR_GRID);
                isChangeColorTag = false;
            }
        }

        protected virtual void OnClickSaveDefectTag(object sender, EventArgs e)
        {
            if (LOTINFO_GRID.GetRowCount() < 1 || DEFECT_TAG_GRID.Rows.Count == 0)
                return;

            SaveQuality(DEFECT_TAG_GRID);
            isChagneDefectTag = false;
        }

        // FOIL ACTIVE CORE 한 쪽만 표시하기 위하여 설정 [ 2017-02-14 ]
        protected virtual void OnCheckFoilChecked(object sender, EventArgs e)
        {
            RadioButton radBtn = sender as RadioButton;

            if (radBtn != null)
            {
                if (EQUIPMENT_COMBO.SelectedIndex < 1)
                {
                    radBtn.IsChecked = false;
                    return;
                }

                Grid grid = null;
                if (string.Equals(radBtn.Name, "radFoil1"))
                    grid = txtCore2.Parent as Grid;
                else if (string.Equals(radBtn.Name, "radFoil2"))
                    grid = txtCore1.Parent as Grid;

                if (grid == null)
                    return;

                RadioButton unFoilBtn = grid.Children[grid.Children.Count - 1] as RadioButton;

                if (unFoilBtn != null)
                    unFoilBtn.IsChecked = false;
            }
        }

        // SLURRY 장착 팝업 화면 호출 [ 2017-01-12 ]
        protected virtual void OnClickSlurryOpen(object sender, EventArgs e)
        {
            string sWOID = string.Empty;

            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WOID"));

            if (string.IsNullOrEmpty(sWOID))
            {
                Util.MessageValidation("SFU1635");  //선택된 W/O가 없습니다.
                return;
            }

            CMM_ELEC_SLURRY popup = new CMM_ELEC_SLURRY();
            popup.FrameOperation = FrameOperation;

            if (EQUIPMENT_COMBO.SelectedIndex > 0 && !string.IsNullOrEmpty(sWOID))
            {
                object[] Parameters = new object[8];
                Parameters[0] = procId;
                Parameters[1] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[2] = Util.NVC(sWOID);
                Parameters[3] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[4] = ((Button)sender).Name == "btnSlurry1" ? 0 : 1;
                Parameters[5] = ((Button)sender).Name == "btnSlurry1" ? txtSlurry1.Text : txtSlurry2.Text;
                Parameters[6] = isSingleCoater == true ? "Y" : "N";
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(EQUIPMENT_COMBO.SelectedItem, "WIDE_ROLL_FLAG"));

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(OnClickSlurryClose);
                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        protected void OnClickSlurryClose(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY popup = sender as CMM_ELEC_SLURRY;
            Button btn = sender as Button;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup._ReturnPosition == 0)
                {
                    txtSlurry1.Text = popup._ReturnLotID;
                    txtSlurry1.Tag = popup._ReturnPRODID;

                    if (popup._IsAllConfirm == true)
                    {
                        txtSlurry2.Text = popup._ReturnLotID;
                        txtSlurry2.Tag = popup._ReturnPRODID;
                    }
                }
                else
                {
                    txtSlurry2.Text = popup._ReturnLotID;
                    txtSlurry2.Tag = popup._ReturnPRODID;

                    if (popup._IsAllConfirm == true)
                    {
                        txtSlurry1.Text = popup._ReturnLotID;
                        txtSlurry1.Tag = popup._ReturnPRODID;
                    }
                }

                // Slurry만 장착 처리
                if (popup._IsAllConfirm == true)
                    SaveMountChange(false, popup._IsSlurryTerm);
                else
                    SaveMountChange(false, popup._IsSlurryTerm, false, popup._ReturnPosition == 0 ? true : false, popup._ReturnPosition == 1 ? true : false);
            }
        }

        protected virtual void OnClickTopLotOpen(object sender, EventArgs e)
        {
            string sWOID = string.Empty;

            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WOID"));

            if (string.IsNullOrEmpty(sWOID))
            {
                Util.MessageValidation("SFU1635");  //선택된 W/O가 없습니다.
                return;
            }

            CMM_ELEC_TOPLOT popup = new CMM_ELEC_TOPLOT();
            popup.FrameOperation = FrameOperation;

            if (EQUIPMENT_COMBO.SelectedIndex > 0 && !string.IsNullOrEmpty(sWOID))
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[1] = procId;
                Parameters[2] = Util.NVC(sWOID);
                Parameters[3] = ((Button)sender).Name == "btnFoil1" ? 0 : 1;
                Parameters[4] = ((Button)sender).Name == "btnFoil1" ? txtCore1.Text : txtCore2.Text;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(OnClickTopLotClose);
                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        protected void OnClickTopLotClose(object sender, EventArgs e)
        {
            CMM_ELEC_TOPLOT popup = sender as CMM_ELEC_TOPLOT;
            Button btn = sender as Button;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Grid grid = null;
                if (popup._ReturnPosition == 0)
                {
                    txtCore1.Text = popup._ReturnLotID;
                    grid = txtCore1.Parent as Grid;
                }
                else
                {
                    txtCore2.Text = popup._ReturnLotID;
                    grid = txtCore2.Parent as Grid;
                }

                // 자동선택처리
                if (grid != null)
                    ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked = true;

                // 자동탈착처리
                if (string.Equals(txtCore1.Text, txtCore2.Text))
                    SetUnMountCore(popup._ReturnPosition);

                SaveMountChange(true, false, popup._IsCoreTerm, false, false);
            }
        }

        protected virtual void OnClickMtrlChange(object sender, EventArgs e)
        {
            string sWOID = WO_DETL_ID;

            if (string.IsNullOrEmpty(WO_DETL_ID))
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;
                int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");
                sWOID = idx > -1 ? Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID")) : string.Empty;
            }

            if (EQUIPMENT_COMBO.SelectedIndex > 0 && !string.IsNullOrEmpty(sWOID))
            {
                SetMountChange();
            }
        }

        protected virtual void OnClickMtrlChange2(object sender, EventArgs e)
        {
            string sWOID = string.Empty;

            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

            CMM_ELEC_SLURRY_TERM popup = new CMM_ELEC_SLURRY_TERM();
            popup.FrameOperation = FrameOperation;

            if (EQUIPMENT_COMBO.SelectedIndex > 0)
            {
                object[] Parameters = new object[7];
                Parameters[0] = procId;
                Parameters[1] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[2] = Util.NVC(sWOID);
                Parameters[3] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[4] = ((Button)sender).Name == "btnSlurry1" ? 0 : 1;
                Parameters[5] = ((Button)sender).Name == "btnSlurry1" ? txtSlurry1.Text : txtSlurry2.Text;
                Parameters[6] = isSingleCoater == true ? "Y" : "N";

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(OnClickMtrlChange2Close);
                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        protected void OnClickMtrlChange2Close(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY_TERM popup = sender as CMM_ELEC_SLURRY_TERM;
            Button btn = sender as Button;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (isSingleCoater)
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));
                else
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), "");
            }
        }

        #endregion Events


        #region Methods

        void InitControls()
        {
            SetButtons();
            SetCheckBox();
        }

        void SetComboBox()
        {
            EQUIPMENT_COMBO = (grdSearch.Children[0] as UcSearch).cboEquipment;
            EQUIPMENTSEGMENT_COMBO = (grdSearch.Children[0] as UcSearch).cboEquipmentSegment;
            COATTYPE_COMBO = (grdSearch.Children[0] as UcSearch).cboCoatType;
            LANENUM_COMBO = (grdDataCollect.Children[0] as UcDataCollect).cboLaneNum;
            COLOR_COMBO = (grdProdLot.Children[0] as UcProdLot).cboColor;

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { EQUIPMENT_COMBO };
            _combo.SetCombo(EQUIPMENTSEGMENT_COMBO, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { procId.Equals(Process.INS_COATING) ? "E2300,E4500" : procId, isSingleCoater ? "SSC" : (procId.Equals(Process.COATING) ? "DSC" : null) };
            C1ComboBox[] cboEquipmentParent = { EQUIPMENTSEGMENT_COMBO };
            _combo.SetCombo(EQUIPMENT_COMBO, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

            if (isSingleCoater || procId.Equals(Process.INS_COATING))
            {
                if (procId.Equals(Process.INS_COATING) && LoginInfo.CFG_AREA_ID.Equals("E1"))
                {
                    (grdSearch.Children[0] as UcSearch).rdoTop.Visibility = Visibility.Visible;
                    (grdSearch.Children[0] as UcSearch).rdoBack.Visibility = Visibility.Visible;
                    (grdSearch.Children[0] as UcSearch).cboCoatType.Visibility = Visibility.Collapsed;
                    (grdSearch.Children[0] as UcSearch).rdoTop.IsChecked = true;
                }
                String[] sFilter3 = { "COAT_SIDE_TYPE" };
                _combo.SetCombo(COATTYPE_COMBO, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
            }
            else
            {
                (grdSearch.Children[0] as UcSearch).spSingleCoater.Visibility = Visibility.Collapsed;
                (grdSearch.Children[0] as UcSearch).cboCoatType.Visibility = Visibility.Collapsed;
            }

            // 2020.06.30 공정 Interlock - 명암 속성에 대한 범레 추가
            COLOR_COMBO.Visibility = Visibility.Collapsed;

            if (procId.Equals(Process.COATING) || procId.Equals(Process.ROLL_PRESSING) || procId.Equals(Process.SLITTING))
            {
                // 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
                SetWipColorLegendCombo();
            }
        }

        void SetButtons()
        {
            STARTBUTTON = (grdCommand.Children[0] as UcCommand).btnStart;
            INVOICEBUTTON = (grdCommand.Children[0] as UcCommand).btnInvoiceMaterial;

            #region [Cancel Delete Lot]
            CANCELDELETE = (grdProdLot.Children[0] as UcProdLot).btnCancelDelete;
            #endregion
        }

        void SetCheckBox()
        {
            CHECK_WAIT = (grdSearch.Children[0] as UcSearch).chkWait;
            CHECK_WAIT.Checked += OnCheckBoxChecked;
            CHECK_WAIT.Unchecked += OnCheckBoxChecked;

            CHECK_RUN = (grdSearch.Children[0] as UcSearch).chkRun;
            CHECK_RUN.Checked += OnCheckBoxChecked;
            CHECK_RUN.Unchecked += OnCheckBoxChecked;

            CHECK_END = (grdSearch.Children[0] as UcSearch).chkEqpEnd;
            CHECK_END.Checked += OnCheckBoxChecked;
            CHECK_END.Unchecked += OnCheckBoxChecked;
            CHECK_WOPRODUCT = (grdProdLot.Children[0] as UcProdLot).chkWoProduct;
            CHECK_WOPRODUCT.Checked += OnCheckBoxChecked;
            CHECK_WOPRODUCT.Unchecked += OnCheckBoxChecked;
        }

        void SetTextBox()
        {
            //믹서와 코터는 상단 생산량/모LOT투입량/잔량을 보여줄 필요가 없다.
            if (PROCID.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing) || PROCID.Equals(Process.MIXING) || PROCID.Equals(Process.COATING) || PROCID.Equals(Process.SRS_MIXING) || PROCID.Equals(Process.SRS_COATING))
            {
                isCoaterAfterProcess = false;
            }
            else
            {
                isCoaterAfterProcess = true;
            }

            txtInputQty = (grdResult.Children[0] as UcResult).txtInputQty;

            if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING))
            {
                (grdResult.Children[0] as UcResult).lblInputQty.Text = ObjectDic.Instance.GetObjectName("양품량");
                txtInputQty.Tag = "C";
            }

            txtParentQty = (grdResult.Children[0] as UcResult).txtParentQty;
            txtRemainQty = (grdResult.Children[0] as UcResult).txtRemainQty;

            chkInOut = (grdResult.Children[0] as UcResult).chkInOut;    //체크박스이지만 이쪽에서 처리
            txtUnit = (grdResult.Children[0] as UcResult).txtUnit;
            txtEndLotId = (grdCommand.Children[0] as UcCommand).txtEndLotId;

            // SHIFT USER, WOREKER 하단에 추가
            txtShift = (grdShift.Children[0] as UcShift).txtShift;
            txtWorker = (grdShift.Children[0] as UcShift).txtWorker;
            txtShiftDateTime = (grdShift.Children[0] as UcShift).txtShiftDateTime;
            txtShiftStartTime = (grdShift.Children[0] as UcShift).txtShiftStartTime;
            txtShiftEndTime = (grdShift.Children[0] as UcShift).txtShiftEndTime;

            // 합권취용 투입 LOT ID
            txtMergeInputLot = (grdDataCollect.Children[0] as UcDataCollect).txtMergeInputLot;

            if (procId == Process.COATING || procId == Process.ROLL_PRESSING || procId == Process.HALF_SLITTING || procId == Process.SLITTING)
            {
                (grdDataCollect.Children[0] as UcDataCollect).chkDefectFilter.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).txtLVFilter.Visibility = Visibility.Visible;
            }
        }

        void SetEvents()
        {
            (grdSearch.Children[0] as UcSearch).btnSearch.Click += OnClickSearch;
            (grdCommand.Children[0] as UcCommand).btnBarcodeLabel.Click += OnClickBarcode;
            (grdCommand.Children[0] as UcCommand).btnCancel.Click += OnClickStartCancel;
            (grdCommand.Children[0] as UcCommand).btnEqptEnd.Click += OnClickEqptEnd;
            (grdCommand.Children[0] as UcCommand).btnEqptEndCancel.Click += OnClickEqptEndCancel;
            (grdCommand.Children[0] as UcCommand).btnConfirm.Click += OnClickConfirm;
            (grdCommand.Children[0] as UcCommand).btnHistoryCard.Click += OnClickHistoryCard;
            (grdCommand.Children[0] as UcCommand).btnCleanLot.Click += OnClickCleanLot;
            (grdCommand.Children[0] as UcCommand).btnCancelFCut.Click += OnClickCancelFLot;
            (grdCommand.Children[0] as UcCommand).btnEqptIssue.Click += OnClickEqptIssue;
            (grdCommand.Children[0] as UcCommand).btnCut.Click += OnClickLotCut;
            (grdCommand.Children[0] as UcCommand).btnEqptCond.Click += OnClickEqptCond;
            (grdCommand.Children[0] as UcCommand).btnPrintLabel.Click += OnClickPrintLabel;
            (grdCommand.Children[0] as UcCommand).btnMixConfirm.Click += OnClickMixConfirm;
            (grdCommand.Children[0] as UcCommand).btnSamplingProd.Click += OnClickSamplingProd;
            (grdCommand.Children[0] as UcCommand).btnReservation.Click += BtnReservation_Click;
            (grdCommand.Children[0] as UcCommand).btnFoil.Click += btnFoil_Click;
            (grdCommand.Children[0] as UcCommand).btnMovetoHalf.Click += btnMovetoHalf_Click;
            #region [Sampling]
            (grdCommand.Children[0] as UcCommand).btnSamplingProdT1.Click += OnClickSamplingProdT1;
            #endregion
            (grdCommand.Children[0] as UcCommand).btnLogisStat.Click += OnClickLogisStat;

            (grdCommand.Children[0] as UcCommand).btnProcReturn.Click += OnClickProcReturn;
            (grdCommand.Children[0] as UcCommand).btnExtra.MouseLeave += OnExtraMouseLeave;

            // Mixer Slurry 정보 조회 Event 추가. 2018.06.01
            (grdCommand.Children[0] as UcCommand).btnMixerTankInfo.Click += OnClickMixerTankInfo;
            (grdCommand.Children[0] as UcCommand).btnWorkHalfSlitSide.Click += OnClickWorkHalfSlitSide;
            // Port별 Skid Type 설정 Evnet
            (grdCommand.Children[0] as UcCommand).btnSkidTypeSettingByPort.Click += OnClickSkidTypeSettingByPort;

            //2020-07-10 오화백 (롤프레스, 슬리터, 하프슬리터 고객인증그룹조회 팝업 추가) CWA 전극2동만 적용
            (grdCommand.Children[0] as UcCommand).btnCustomer.Click += OnClickbtnCustomer;

            //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
            (grdCommand.Children[0] as UcCommand).btnReturnCondition.Click += OnClickReturnCondition;


            (grdSearch.Children[0] as UcSearch).cboEquipment.SelectedItemChanged += OnSelectedItemChangedEquipmentCombo;
            (grdSearch.Children[0] as UcSearch).cboCoatType.SelectedItemChanged += OnSelectedItemChangedCoatSideTypeCombo;
            (grdSearch.Children[0] as UcSearch).rdoTop.Checked += On_rdoTop_Checked;
            (grdSearch.Children[0] as UcSearch).rdoBack.Checked += On_rdoBack_Checked;
            (grdSearch.Children[0] as UcSearch).btnRollMap.Click += btnRollMap_Click;
            (grdProdLot.Children[0] as UcProdLot).dgLargeLot.CurrentCellChanged += OnGridCurrentCellChanged;
            (grdProdLot.Children[0] as UcProdLot).dgProductLot.CurrentCellChanged += OnGridCurrentCellChanged;
            (grdProdLot.Children[0] as UcProdLot).dgProductLot.LoadedCellPresenter += OnLoadedProdLotCellPresenter;
            (grdProdLot.Children[0] as UcProdLot).dgProductLot.UnloadedCellPresenter += OnUnloadedProdLotCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.CurrentCellChanged += OnDefectCurrentChanged;
            (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.CurrentCellChanged += OnDefectCurrentChanged;
            (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.CurrentCellChanged += OnDefectCurrentChanged;

            (grdDataCollect.Children[0] as UcDataCollect).dgLevel1.LoadedCellPresenter += OnLoadedDefectCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgLevel2.LoadedCellPresenter += OnLoadedDefectCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgLevel3.LoadedCellPresenter += OnLoadedDefectCellPresenter;

            #region [Cancel Delete Lot]
            (grdProdLot.Children[0] as UcProdLot).btnCancelDelete.Click += OnCancelDeleteLot;
            #endregion

            (grdResult.Children[0] as UcResult).txtInputQty.KeyDown += OnKeyDownInputQty;
            (grdResult.Children[0] as UcResult).txtInputQty.LostFocus += OnKeyLostInputQty;
            (grdResult.Children[0] as UcResult).btnSaveWipHistory.Click += OnClickWIPHistory;
            (grdResult.Children[0] as UcResult).btnSaveCarrier.Click += OnClickSaveCarrier;

            #region # 전수불량Lane등록
            (grdResult.Children[0] as UcResult).btnSaveRegDefectLane.Click += OnClickRegDefectLane;
            #endregion

            (grdResult.Children[0] as UcResult).dgLotInfo.LoadedCellPresenter += OnLoadedLotInfoCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.CommittedEdit += OnDataCollectGridCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.BeginningEdit += OnDataCollectGridBeginningEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.MouseDoubleClick += OnClickCellMouseDoubleClick;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.LoadedCellPresenter += OnDataCollectGridLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.UnloadedCellPresenter += OnDataCollectGridUnLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.CommittedEdit += OnDataCollectGridCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.MouseDoubleClick += OnClickCellMouseDoubleClick;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.LoadedCellPresenter += OnDataCollectGridLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.UnloadedCellPresenter += OnDataCollectGridUnLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.BeganEdit += dgWipReason2_BeganEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgRemark.LoadedCellPresenter += OnLoadedRemarkCellPresenter;

            #region [POSTACTION]
            (grdDataCollect.Children[0] as UcDataCollect).dgPostHold.LoadedCellPresenter += OnLoadedPostHoldCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgPostHold.CommittedEdit += OnDataCollectGridPostHoldCommittedEdit;
            #endregion

            (grdDataCollect.Children[0] as UcDataCollect).dgQuality.CommittedEdit += OnDataCollectGridQualityCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality.LoadedCellPresenter += OnLoadedDgQualitynCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality.UnloadedCellPresenter += OnUnLoadedDgQualitynCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality2.CommittedEdit += OnDataCollectGridQualityCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality2.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality2.LoadedCellPresenter += OnLoadedDgQualitynCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgQuality2.UnloadedCellPresenter += OnUnLoadedDgQualitynCellPresenter;
            (grdDataCollect.Children[0] as UcDataCollect).dgDefectTag.CommittedEdit += OnDataCollectGridDefectTagComittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgColor.CommittedEdit += OnDataCollectGridColorTagCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgCotton.CommittedEdit += OnDataCollectGridCottonTagCommittedEdit;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveWipReason.Click += OnClickSaveWipReason;
            (grdDataCollect.Children[0] as UcDataCollect).btnProcResn.Click += OnClickProcReason;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveQuality.Click += OnClickSaveQuality;
            (grdDataCollect.Children[0] as UcDataCollect).btnRemoveMaterial.Visibility = Visibility.Hidden;

            (grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial.Click += OnClickSaveMaterial;
            (grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial.Click += OnClickDeleteMaterial;
            (grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial.Click += OnClickAddMaterial;
            (grdDataCollect.Children[0] as UcDataCollect).btnRemoveMaterial.Click += OnClickRemoveMaterial;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveRemark.Click += OnClickSaveRemark;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveColor.Click += OnClickSaveColorTag;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveTag.Click += OnClickSaveDefectTag;
            //전체저장 2018-01-09
            (grdDataCollect.Children[0] as UcDataCollect).btnPublicWipSave.Click += OnClickPublicSave;
            (grdDataCollect.Children[0] as UcDataCollect).btnSearchSlurry.Click += OnClickSearchSlurry;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveSlurry.Click += OnClickSaveSlurry;
            (grdDataCollect.Children[0] as UcDataCollect).btnDelSlurry.Click += OnClickDeleteSlurry;
            (grdDataCollect.Children[0] as UcDataCollect).dgSearchSlurry.MouseDoubleClick += OnGridMouseDoubleClick;
            (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.CommittedEdit += OnMaterialCommittedEdit;
            //면상태일지 2018-04-06
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveCotton.Click += OnClickCottonSave;
            //고형분 비율 2023-03-27
            (grdDataCollect.Children[0] as UcDataCollect).btnSolidContRate.Click += OnClickSolidContRate;

            #region[POSTACTION]
            (grdDataCollect.Children[0] as UcDataCollect).btnSavePostHold.Click += OnClickPostHoldSave;
            #endregion

            // COATER전용 이벤트 추가 ( 2017-01-09 )
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.BeginningEdit += OnDataGridBeginningEdit;
            (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2.BeginningEdit += OnDataGridBeginningEdit;

            //[CR-128] MIXER Consume Materail(2017.01.18)
            (grdDataCollect.Children[0] as UcDataCollect).btnEqptMaterial.Click += OnClickEqptMaterialList;
            (grdDataCollect.Children[0] as UcDataCollect).btnInputMaterial.Click += OnClickInputMaterial;

            // MULTI LANE 소형/자동차를 나눠서 기능을 사용하기 위하여 추가 [2017-01-24] -CR-56-
            (grdDataCollect.Children[0] as UcDataCollect).chkSum.Click += OnSumReasonGridChecked;
            (grdDataCollect.Children[0] as UcDataCollect).cboLaneNum.SelectedItemChanged += OnLaneSelectionItemChanged;

            // 해당 SUMMARYROW의 CELL표시 및 양품량 계산을 위하여 해당 EVENT 추가 [2017-02-17]
            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
            {
                (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.LoadedCellPresenter += OnLoadedWipReasonCellPresenter;
                (grdDataCollect.Children[0] as UcDataCollect).txtMesualQty.KeyDown += OnKeyDownMesualQty;
                (grdDataCollect.Children[0] as UcDataCollect).txtMesualQty.LostFocus += OnKeyLostMesualQty;
            }

            // SHIFT USER, WORKER 정보를 사용하기 위하여 BUTTON 이벤트 추가 [2017-02-24]
            (grdShift.Children[0] as UcShift).btnShift.Click += OnClickShift;

            // 공정진척 창확장 버튼 EVENT추가 [2017-03-24]
            (grdDataCollect.Children[0] as UcDataCollect).btnExpandFrame.Click += OnClickbtnExpandFrame;
            (grdDataCollect.Children[0] as UcDataCollect).btnLeftExpandFrame.Click += OnClickbtnLeftExpandFrame;

            // 합권취 버튼 이벤트 추가 [2017-07-06]
            (grdDataCollect.Children[0] as UcDataCollect).btnSearchMerge.Click += OnClickSearchMergeData;
            (grdDataCollect.Children[0] as UcDataCollect).btnSaveMerge.Click += OnClickSaveMergeData;

            // CNA에서 불량 실적 반영안되는 괴현상 발생으로 불량값 입력 하다 TAB이동 후 다시 돌아오면 소수점 클리어되는 현상으로 하기 이벤트 추가 [2017-09-03]
            (grdDataCollect.Children[0] as UcDataCollect).tcDataCollect.SelectionChanged += OnTabSelectionChange;

            //CWA 불량정보 필터링 이벤트 추가
            (grdDataCollect.Children[0] as UcDataCollect).chkDefectFilter.Click += OnClickDefetectFilter;
        }

        private void OnLoadedProdLotCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e == null || sender == null)
                    return;

                C1DataGrid dg = sender as C1DataGrid;
                C1.WPF.DataGrid.DataGridCell cell = e.Cell as C1.WPF.DataGrid.DataGridCell;

                if (dg == null || cell == null || cell.Presenter == null)
                    return;

                if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                    return;

                // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완 
                if (WIPCOLORLEGEND == null)
                    return;

                // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                SolidColorBrush scbZVersionBack = new SolidColorBrush();
                SolidColorBrush scbZVersionFore = new SolidColorBrush();

                SolidColorBrush scbCutBack = new SolidColorBrush();
                SolidColorBrush scbCutFore = new SolidColorBrush();

                foreach (DataRow dr in WIPCOLORLEGEND.Rows)
                {
                    if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (dr["CODE"].ToString().Equals("Z_VER"))
                    {
                        scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                    else if (dr["CODE"].ToString().Equals("CUT"))
                    {
                        scbCutBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbCutFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                }

                // 2020.06.30 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                if (string.Equals(procId, Process.COATING))
                {
                    if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "CUT")).Equals("1")
                    || Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "CUT")).Equals("L1")
                    || Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "CUT")).Equals("R1"))
                    {
                        e.Cell.Presenter.SelectedBackground = scbCutBack;
                        e.Cell.Presenter.Background = scbCutBack;
                        e.Cell.Presenter.Foreground = scbCutFore;
                    }
                    else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["COATERVER"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Substring(0, 1).Equals("Z"))
                    {
                        e.Cell.Presenter.Background = scbZVersionBack;
                        e.Cell.Presenter.Foreground = scbZVersionFore;
                    }
                    else
                    {
                        e.Cell.Presenter.SelectedBackground = dg.SelectedBackground;
                        e.Cell.Presenter.Background = dg.Background;
                        e.Cell.Presenter.Foreground = dg.Foreground;
                    }
                }
                else if (string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.TAPING))
                {
                    if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).IsNullOrEmpty() &&
                            Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.SelectedBackground = scbCutBack;
                        e.Cell.Presenter.Background = scbCutBack;
                        e.Cell.Presenter.Foreground = scbCutFore;
                    }
                    else if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                            ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["COATERVER"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Length >= 1 &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Substring(0, 1).Equals("Z"))
                    {
                        e.Cell.Presenter.Background = scbZVersionBack;
                        e.Cell.Presenter.Foreground = scbZVersionFore;
                    }
                    else if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                            ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["COATERVER"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Length >= 1 &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Substring(0, 1).Equals("Z"))
                    {
                        e.Cell.Presenter.Background = scbZVersionBack;
                        e.Cell.Presenter.Foreground = scbZVersionFore;
                    }
                    else
                    {
                        e.Cell.Presenter.SelectedBackground = dg.SelectedBackground;
                        e.Cell.Presenter.Background = dg.Background;
                        e.Cell.Presenter.Foreground = dg.Foreground;
                    }

                }
                else if (string.Equals(procId, Process.SLITTING))
                {
                    // 20201221 슬리터 샘플링 컷 범례 표시
                    if (e.Cell.Column.Name.Equals("CUT_ID") && !Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).IsNullOrEmpty() &&
                               Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.SelectedBackground = scbCutBack;
                        e.Cell.Presenter.Background = scbCutBack;
                        e.Cell.Presenter.Foreground = scbCutFore;
                    }
                    else if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["COATERVER"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Substring(0, 1).Equals("Z"))
                    {
                        e.Cell.Presenter.Background = scbZVersionBack;
                        e.Cell.Presenter.Foreground = scbZVersionFore;
                    }
                    else if (e.Cell.Column.Name.Equals("CUT_ID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["COATERVER"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COATERVER")).Substring(0, 1).Equals("Z"))
                    {
                        e.Cell.Presenter.Background = scbZVersionBack;
                        e.Cell.Presenter.Foreground = scbZVersionFore;
                    }
                    else
                    {
                        e.Cell.Presenter.SelectedBackground = dg.SelectedBackground;
                        e.Cell.Presenter.Background = dg.Background;
                        e.Cell.Presenter.Foreground = dg.Foreground;
                    }
                }

            }));
        }
        protected virtual void OnUnloadedProdLotCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e != null && e.Cell != null && e.Cell.Presenter != null)
                        {
                            if (string.Equals(procId, Process.SLITTING))
                            {
                                if (!string.Equals(e.Cell.Column.Name, "CUT_ID"))
                                {
                                    e.Cell.Presenter.SelectedBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 208, 218));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                            else
                            {
                                if (dataGrid.Columns["LOTID"].Index != e.Cell.Column.Index)
                                {
                                    e.Cell.Presenter.SelectedBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 208, 218));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                    }
                }
            }
        }
        void SetVisible()
        {
            // GRID LOT
            if (procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing) || procId.Equals(Process.MIXING))
            {
                PRODLOT_GRID.Columns["LOTID_PR"].Visibility = Visibility.Collapsed;
                PRODLOT_GRID.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
                PRODLOT_GRID.Columns["COATERVER"].Visibility = Visibility.Collapsed;
                PRODLOT_GRID.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

                if (procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing))
                {
                    PRODLOT_GRID.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                    PRODLOT_GRID.Columns["PRODDESC"].DisplayIndex = PRODLOT_GRID.Columns["PRJT_NAME"].DisplayIndex;
                }

                LOTINFO_GRID.Columns["GOODQTY"].Header = "양품수량";
                LOTINFO_GRID.Columns["GOODPTNQTY"].Header = "양품수량";
                LOTINFO_GRID.Columns["GOODPTNQTY"].Visibility = Visibility.Collapsed;
            }
            else if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
            {
                LOTINFO_GRID.Columns["GOODQTY"].Header = "양품수량" + "(S/Roll)";
                LOTINFO_GRID.Columns["GOODPTNQTY"].Visibility = Visibility.Collapsed;

                string sHeader = "불량수량" + "(S/Roll)";
                LOTINFO_GRID.Columns["LOSSQTY"].Header = new List<string>() { sHeader, "합계" };
                LOTINFO_GRID.Columns["DTL_DEFECT"].Header = new List<string>() { sHeader, "LOT불량" };
                LOTINFO_GRID.Columns["DTL_LOSS"].Header = new List<string>() { sHeader, "LOSS" };
                LOTINFO_GRID.Columns["DTL_CHARGEPRD"].Header = new List<string>() { sHeader, "물품청구" };
            }
            else
            {
                List<string> sHeader = new List<string>() { "양품수량", "C/Roll" };
                LOTINFO_GRID.Columns["GOODQTY"].Header = sHeader;
                LOTINFO_GRID.Columns["GOODPTNQTY"].Visibility = Visibility.Visible;

                // HALF SLITTING 공정에서 LANE수 LOT_GRID에 추가 [2017-01-05]
                if (procId.Equals(Process.HALF_SLITTING))
                {
                    LOTINFO_GRID.Columns["LANE_QTY"].Visibility = Visibility.Visible;
                }

                // COATER 공정에서 횟수 입력 추가 [2017-01-09]
                /*
                if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                {
                    WIPREASON_GRID.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                    WIPREASON2_GRID.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                }
                */

                if (procId.Equals(Process.ROLL_PRESSING))
                    PRODLOT_GRID.Columns["PRESSCOUNT"].Visibility = Visibility.Visible;
            }

            // 횟수를 COMMONCODE로 관리하도록 변경 (C20190416_75868) [2019-04-16]
            isResnCountUse = IsAreaCommonCodeUse("RESN_COUNT_USE_YN", procId);
            if (isResnCountUse == true && (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING)))
            {
                WIPREASON_GRID.Columns["COUNTQTY"].Visibility = Visibility.Visible;

                if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                    WIPREASON2_GRID.Columns["COUNTQTY"].Visibility = Visibility.Visible;
            }

            //[CR-128] MIXER Consume Material(2017.01.18)
            if (procId.Equals(Process.MIXING))
            {
                (grdDataCollect.Children[0] as UcDataCollect).btnEqptMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).btnInputMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterialList.Visibility = Visibility.Visible;

                (grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnRemoveMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Visibility = Visibility.Collapsed;
            }
            else if (procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing) || procId.Equals(Process.SRS_MIXING))
            {
                (grdDataCollect.Children[0] as UcDataCollect).btnInputMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterialList.Visibility = Visibility.Visible;

                (grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnRemoveMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Visibility = Visibility.Collapsed;
            }
            else if (procId.Equals(Process.BACK_WINDER))
            {
                (grdCommand.Children[0] as UcCommand).btnCancel.Visibility = Visibility.Collapsed;
            }

            // MULTI LANE은 수동으로 ALL/SUM 구분하여 사용할 수 있게 변경 [2017-01-21] -CR-56-
            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
            {
                WIPREASON_GRID.Columns["RESNTOTQTY"].Visibility = Visibility.Visible;

                (grdDataCollect.Children[0] as UcDataCollect).chkSum.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).cboLaneNum.Visibility = Visibility.Visible;

                (grdDataCollect.Children[0] as UcDataCollect).lblMesualQty.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).txtMesualQty.Visibility = Visibility.Visible;
            }

            if (string.Equals(procId, Process.ROLL_PRESSING))
            {
                WIPREASON_GRID.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                DataTableConverter.SetValue(QUALITY_GRID, "FrozenBottomRowsCount", 1);
            }

            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                DataTableConverter.SetValue(WIPREASON_GRID, "FrozenBottomRowsCount", 2);
            else
                (grdDataCollect.Children[0] as UcDataCollect).dgWipReason.BottomRows.Clear();

            if (string.Equals(LoginInfo.CFG_AREA_ID, "E1") && string.Equals(procId, Process.COATING))
            {
                WIPREASON_GRID.Columns["CONVRESNQTY"].Visibility = Visibility.Visible;
                WIPREASON2_GRID.Columns["CONVRESNQTY"].Visibility = Visibility.Visible;
            }

            // SRS코터 투입자재란에 실제수량이 아닌 1BATCH수량이라 입력값 보여주지 않음 [2017-05-23]
            if (string.Equals(procId, Process.SRS_COATING))
                INPUTMTRL_GRID.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;

            // SRS SLITTER에는 체크박스 보여줌 [2017-06-07]
            if (string.Equals(procId, Process.SRS_SLITTING) && chkInOut != null)
                chkInOut.Visibility = Visibility.Visible;

            // CNA 코터공정 CUT 표기 요청 [2017-07-12]
            if (string.Equals(procId, Process.COATING))
                PRODLOT_GRID.Columns["CUT"].Visibility = Visibility.Visible;

            // SLITTING 공정만 생산 중 불량 기능 추가 [2017-07-18]
            if (string.Equals(procId, Process.SLITTING))
                (grdDataCollect.Children[0] as UcDataCollect).btnProcResn.Visibility = Visibility.Visible;

            if (IsAreaCommonCodeUse("SOLID_CONT_AUTO_CALC_PROC", procId))
                (grdDataCollect.Children[0] as UcDataCollect).btnSolidContRate.Visibility = Visibility.Visible;
            else
                (grdDataCollect.Children[0] as UcDataCollect).btnSolidContRate.Visibility = Visibility.Collapsed;

        }

        public void SetApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add((grdCommand.Children[0] as UcCommand).btnStart);
            listAuth.Add((grdCommand.Children[0] as UcCommand).btnCancel);
            listAuth.Add((grdCommand.Children[0] as UcCommand).btnEqptEnd);
            listAuth.Add((grdCommand.Children[0] as UcCommand).btnConfirm);

            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnProcResn);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnSaveWipReason);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnSaveQuality);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnRemoveMaterial);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnSaveRemark);
            listAuth.Add((grdDataCollect.Children[0] as UcDataCollect).btnSaveColor);
            listAuth.Add((grdResult.Children[0] as UcResult).btnSaveWipHistory);
            listAuth.Add((grdShift.Children[0] as UcShift).btnShift);

            listAuth.Add((grdProdLot.Children[0] as UcProdLot).btnCancelDelete);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        void SetGrids()
        {
            LARGELOT_GRID = (grdProdLot.Children[0] as UcProdLot).dgLargeLot;
            PRODLOT_GRID = (grdProdLot.Children[0] as UcProdLot).dgProductLot;
            LOTINFO_GRID = (grdResult.Children[0] as UcResult).dgLotInfo;
            WIPREASON_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgWipReason;
            WIPREASON2_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgWipReason2;
            QUALITY_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgQuality;
            QUALITY2_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgQuality2;
            COLOR_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgColor;
            DEFECT_TAG_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgDefectTag;
            INPUTMTRL_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgMaterial;
            SLURRY_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgSearchSlurry;
            SLURRY_INPUT_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgSlurry;
            REMARK_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgRemark;
            REMARK_HIST_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgRemarkHistory;
            //[CR-128] MIXER Consume Material (2017.01.18)
            INPUTMTRLSUMMARY_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgMaterialList;
            MERGE_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgWipMerge;
            MERGE2_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgWipMerge2;
            COTTON_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgCotton;
            #region[POSTACTION]
            POSTHOLD_GRID = (grdDataCollect.Children[0] as UcDataCollect).dgPostHold;
            #endregion
            switch (procId)
            {
                case "E2000":
                case "E2300":
                    if (procId.Equals(Process.COATING))
                        LARGELOT_GRID.Visibility = Visibility.Visible;
                    else
                        LARGELOT_GRID.Visibility = Visibility.Collapsed;

                    WIPREASON2_GRID.Visibility = Visibility.Visible;

                    // 단면코터 TOP일시 TOP만 표시 (2017-01-09)
                    if ((isSingleCoater && string.Equals(COATTYPE_COMBO.SelectedValue, "T")) || (string.Equals(procId, Process.INS_COATING) && string.Equals(COATTYPE_COMBO.SelectedValue, "T")))
                        WIPREASON2_GRID.Visibility = Visibility.Collapsed;

                    QUALITY2_GRID.Visibility = Visibility.Visible;

                    Grid grdWipReason = (grdDataCollect.Children[0] as UcDataCollect).grdWipReason as Grid;
                    Grid grdQuality = (grdDataCollect.Children[0] as UcDataCollect).grdQualityGrid as Grid;

                    grdWipReason.ColumnDefinitions.Clear();
                    grdQuality.ColumnDefinitions.Clear();

                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdWipReason.ColumnDefinitions.Add(cd);

                    // 단면코터 TOP일시 초기화 및 Lable 표시안함 (2017-01-09)
                    if ((isSingleCoater && string.Equals(COATTYPE_COMBO.SelectedValue, "T")) || (string.Equals(procId, Process.INS_COATING) && string.Equals(COATTYPE_COMBO.SelectedValue, "T")))
                        grdWipReason.ColumnDefinitions.Clear();

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdQuality.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    grdQuality.ColumnDefinitions.Add(cd);
                    break;

                case "E3000":
                    LARGELOT_GRID.Visibility = Visibility.Collapsed;
                    WIPREASON2_GRID.Visibility = Visibility.Collapsed;
                    QUALITY_GRID.Visibility = Visibility.Visible;
                    QUALITY2_GRID.Visibility = Visibility.Collapsed;
                    COLOR_GRID.Visibility = Visibility.Visible;

                    grdQuality = (grdDataCollect.Children[0] as UcDataCollect).grdQualityGrid as Grid;
                    grdQuality.ColumnDefinitions.Clear();
                    (grdSearch.Children[0] as UcSearch).gridWorkHalfSlittingSide.Visibility = Visibility.Visible;
                    break;
                //2020-03-30 오화백 : 슬리터 공정에서도 무지부방향 보이도록 추가
                case "E4000":
                    LARGELOT_GRID.Visibility = Visibility.Collapsed;
                    WIPREASON2_GRID.Visibility = Visibility.Collapsed;
                    QUALITY2_GRID.Visibility = Visibility.Collapsed;
                    (grdSearch.Children[0] as UcSearch).gridWorkHalfSlittingSide.Visibility = Visibility.Visible;
                    break;

                default:
                    LARGELOT_GRID.Visibility = Visibility.Collapsed;
                    WIPREASON2_GRID.Visibility = Visibility.Collapsed;
                    QUALITY2_GRID.Visibility = Visibility.Collapsed;
                    (grdSearch.Children[0] as UcSearch).gridWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
                    break;
            }


        }

        void SetMixLossClean()
        {
            RadioButton rbManual = new RadioButton();
            rbManual.Content = ObjectDic.Instance.GetObjectName("수동");
            rbManual.Name = "rbManual";
            rbManual.Click += new RoutedEventHandler(OnClickMixLoss);

            RadioButton rbSimpClean = new RadioButton();
            rbSimpClean.Content = ObjectDic.Instance.GetObjectName("간이세정");
            rbSimpClean.Name = "rbSimpClean";
            rbSimpClean.Click += new RoutedEventHandler(OnClickMixLoss);

            RadioButton rbFullClean = new RadioButton();
            rbFullClean.Content = ObjectDic.Instance.GetObjectName("Full세정");
            rbFullClean.Name = "rbFullClean";
            rbFullClean.Click += new RoutedEventHandler(OnClickMixLoss);

            rbManual.IsChecked = true;
            rbSimpClean.IsChecked = false;
            rbFullClean.IsChecked = false;

            Grid wGrid = (grdDataCollect.Children[0] as UcDataCollect).grdWipReasonHeader;
            wGrid.ColumnDefinitions.Clear();

            wGrid.ColumnDefinitions.Add(new ColumnDefinition());
            StackPanel sPanel = new StackPanel();
            Border sBoard = new Border();
            sBoard.Width = 50;
            sPanel.Orientation = Orientation.Horizontal;
            sPanel.HorizontalAlignment = HorizontalAlignment.Left;
            sPanel.Children.Add(sBoard);
            sPanel.Children.Add(rbManual);
            sBoard = new Border();
            sBoard.Width = 8;
            sPanel.Children.Add(sBoard);
            sPanel.Children.Add(rbSimpClean);
            sBoard = new Border();
            sBoard.Width = 8;
            sPanel.Children.Add(sBoard);
            sPanel.Children.Add(rbFullClean);

            wGrid.Children.Add(sPanel);
        }

        protected virtual void OnClickMixLoss(object sender, RoutedEventArgs e)
        {
            if (LOTINFO_GRID.GetRowCount() < 1)
                return;

            RadioButton rb = sender as RadioButton;
            string sCode = string.Empty;

            switch (rb.Name)
            {
                case "rbSimpClean":
                    sCode = "SIMP";
                    break;

                case "rbFullClean":
                    sCode = "FULL";
                    break;

                default:
                    sCode = "BAS";
                    break;
            }

            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LOTID;
                Indata["CODE"] = sCode;
                IndataTable.Rows.Add(Indata);

                //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(WIPREASON_GRID, dtResult, FrameOperation, true);
                GetSumDefectQty();
                SetCauseTitle();
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        void SetTabItems()
        {
            C1TabItem tabDefect = (grdDataCollect.Children[0] as UcDataCollect).tiDefect;
            C1TabItem tabLoss = (grdDataCollect.Children[0] as UcDataCollect).tiLoss;
            C1TabItem tabCharge = (grdDataCollect.Children[0] as UcDataCollect).tiCharge;
            C1TabItem tabQuality = (grdDataCollect.Children[0] as UcDataCollect).tiQuality;
            C1TabItem tabColor = (grdDataCollect.Children[0] as UcDataCollect).tiColor;
            C1TabItem tabDefectTag = (grdDataCollect.Children[0] as UcDataCollect).tiDefectTag;
            C1TabItem tabInputMaterial = (grdDataCollect.Children[0] as UcDataCollect).tiInputMaterial;
            C1TabItem tabSlurry = (grdDataCollect.Children[0] as UcDataCollect).tiSlurry;
            C1TabItem tabRemark = (grdDataCollect.Children[0] as UcDataCollect).tiRemark;
            C1TabItem tabRemarkHistory = (grdDataCollect.Children[0] as UcDataCollect).tiRemarkHistory;
            C1TabItem tabMerge = (grdDataCollect.Children[0] as UcDataCollect).tiMerge;
            C1TabItem tabCotton = (grdDataCollect.Children[0] as UcDataCollect).tiCotton;
            TextBlock tbTop = (grdDataCollect.Children[0] as UcDataCollect).lblTop;
            TextBlock tbBack = (grdDataCollect.Children[0] as UcDataCollect).lblBack;
            TextBlock tbQualityTop = (grdDataCollect.Children[0] as UcDataCollect).lblQualityTop;
            TextBlock tbQualityBack = (grdDataCollect.Children[0] as UcDataCollect).lblQualityBack;
            Button btnPublicWipSave = (grdDataCollect.Children[0] as UcDataCollect).btnPublicWipSave;
            Button btnSaveCotton = (grdDataCollect.Children[0] as UcDataCollect).btnSaveCotton;

            #region [POSTACTION]
            C1TabItem tabPostHold = (grdDataCollect.Children[0] as UcDataCollect).tiPostHold;
            #endregion
                        
            switch (procId)
            {
                case "E0500":
                case "E0400":
                case "E0410":
                case "E0420":
                case "E0430":
                case "E1000":
                case "S1000":
                    tabDefect.Visibility = Visibility.Collapsed;
                    tabSlurry.Visibility = Visibility.Collapsed;
                    tabRemark.Visibility = Visibility.Visible;
                    tbTop.Visibility = Visibility.Collapsed;
                    tbBack.Visibility = Visibility.Collapsed;
                    tbQualityTop.Visibility = Visibility.Collapsed;
                    tbQualityBack.Visibility = Visibility.Collapsed;
                    tabRemarkHistory.Visibility = Visibility.Collapsed;
                    tabMerge.Visibility = Visibility.Collapsed;

                    if (procId.Equals(Process.MIXING))
                        SetMixLossClean();
                    break;

                case "E2000":
                    tabRemark.Visibility = Visibility.Visible;
                    tabMerge.Visibility = Visibility.Collapsed;
                    tbTop.Visibility = Visibility.Visible;
                    tbBack.Visibility = Visibility.Visible;

                    // 단면코터 TOP일시 초기화 및 Lable 표시안함 (2017-01-09)
                    if (isSingleCoater && string.Equals(COATTYPE_COMBO.SelectedValue, "T"))
                        tbBack.Visibility = Visibility.Collapsed;

                    // 단면코터일 경우만 합권취 표시 [2017-07-06]
                    if (isSingleCoater)
                        tabMerge.Visibility = Visibility.Visible;

                    tbQualityTop.Visibility = Visibility.Visible;
                    tbQualityBack.Visibility = Visibility.Visible;
                    tabSlurry.Visibility = Visibility.Collapsed;
                    tabRemarkHistory.Visibility = Visibility.Collapsed;
                    
                    break;

                case "E3000":
                    tabInputMaterial.Visibility = Visibility.Collapsed;
                    tabSlurry.Visibility = Visibility.Collapsed;
                    tabRemark.Visibility = Visibility.Visible;
                    tbTop.Visibility = Visibility.Collapsed;
                    tbBack.Visibility = Visibility.Collapsed;
                    tbQualityTop.Visibility = Visibility.Collapsed;
                    tbQualityBack.Visibility = Visibility.Collapsed;
                    tabColor.Visibility = Visibility.Visible;
                    #region [POSTACTION]
                    // if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EA") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                    // {
                    //     tabRemark.Visibility = Visibility.Collapsed;
                    //     tabPostHold.Visibility = Visibility.Visible;
                    //     POSTHOLD_GRID.Columns[1].Visibility = Visibility.Collapsed;
                    // }
                    

                    // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
                    if (string.Equals(_RemarkHoldUseFlag, "Y"))
                    {
                        tabRemark.Visibility = Visibility.Collapsed;
                        tabPostHold.Visibility = Visibility.Visible;
                        POSTHOLD_GRID.Columns[1].Visibility = Visibility.Collapsed;
                    }

                    #endregion
                    break;

                case "E4000":
                    tabSlurry.Visibility = Visibility.Collapsed;
                    tabInputMaterial.Visibility = Visibility.Collapsed;
                    tbTop.Visibility = Visibility.Collapsed;
                    tbBack.Visibility = Visibility.Collapsed;
                    tbQualityTop.Visibility = Visibility.Collapsed;
                    tbQualityBack.Visibility = Visibility.Collapsed;
                    if (string.Equals(LoginInfo.CFG_AREA_ID, "E5"))
                    {
                        tabCotton.Visibility = Visibility.Visible;
                    }

                    #region [POSTACTION]
                    //if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EA") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                    //{
                    //    tabRemark.Visibility = Visibility.Collapsed;
                    //    tabPostHold.Visibility = Visibility.Visible;
                    //    POSTHOLD_GRID.Columns[1].Visibility = Visibility.Visible;
                    //}

                    // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
                    if (string.Equals(_RemarkHoldUseFlag, "Y"))
                    {
                        tabRemark.Visibility = Visibility.Collapsed;
                        tabPostHold.Visibility = Visibility.Visible;
                        POSTHOLD_GRID.Columns[1].Visibility = Visibility.Visible;
                    }

                    #endregion
                    break;

                default:
                    tabInputMaterial.Visibility = Visibility.Collapsed;
                    tabSlurry.Visibility = Visibility.Collapsed;
                    tbTop.Visibility = Visibility.Collapsed;
                    tbBack.Visibility = Visibility.Collapsed;
                    tbQualityTop.Visibility = Visibility.Collapsed;
                    tbQualityBack.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(PRODLOT_GRID);
                ClearDetailControls();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private void ClearDetailControls()
        {
            Util.gridClear(LOTINFO_GRID);
            Util.gridClear(WIPREASON_GRID);
            Util.gridClear(WIPREASON2_GRID);
            Util.gridClear(QUALITY_GRID);
            Util.gridClear(QUALITY2_GRID);
            Util.gridClear(INPUTMTRL_GRID);
            Util.gridClear(SLURRY_GRID);
            Util.gridClear(SLURRY_INPUT_GRID);
            Util.gridClear(COLOR_GRID);
            Util.gridClear(DEFECT_TAG_GRID);
            Util.gridClear(INPUTMTRLSUMMARY_GRID);
            Util.gridClear(REMARK_GRID);
            Util.gridClear(REMARK_HIST_GRID);
            Util.gridClear(MERGE_GRID);
            Util.gridClear(MERGE2_GRID);
            Util.gridClear(COTTON_GRID);
            #region[POSTACTION]
            Util.gridClear(POSTHOLD_GRID);
            #endregion
            LOTINFO_GRID.BottomRows.Clear();

            if (procResnDt != null)
                procResnDt.Clear();

            InitClear();
        }

        private void InitClear()
        {
            // INIT VARIABLE
            _ValueWOID = string.Empty;
            _LargeLOTID = string.Empty;
            _LOTID = string.Empty;
            _EQPTID = string.Empty;
            _LOTIDPR = string.Empty;
            _CUTID = string.Empty;
            _WIPSTAT = string.Empty;
            _INPUTQTY = string.Empty;
            _OUTPUTQTY = string.Empty;
            _CTRLQTY = string.Empty;
            _GOODQTY = string.Empty;
            _LOSSQTY = string.Empty;
            _WORKORDER = string.Empty;
            _WORKDATE = string.Empty;
            _VERSION = string.Empty;
            _PRODID = string.Empty;
            _WIPDTTM_ST = string.Empty;
            _WIPDTTM_ED = string.Empty;
            _REMARK = string.Empty;
            _CONFIRMUSER = string.Empty;
            sEQPTID = string.Empty;
            _FINALCUT = string.Empty;
            _WIPSTAT_NAME = string.Empty;
            _LANEQTY = string.Empty;
            _PTNQTY = string.Empty;
            exceedLengthQty = 0;
            convRate = 1;
            cut = "Y";  // Cut

            InitTextBox();

            isChangeQuality = false;
            isChangeMaterial = false;
            isChangeColorTag = false;
            isChagneDefectTag = false;
            isChangeRemark = false;
            isChangeCotton = false;

            LANENUM_COMBO.Items.Clear();
        }

        void InitWorkOrderCheck()
        {
            CHECK_WOPRODUCT.IsChecked = false;
            dtWoAll = null;
            dtWoCheck = null;
        }

        void InitTextBox()
        {
            txtUnit.Text = string.Empty;
            txtInputQty.Value = 0;

            if (txtGoodQty != null)
                txtGoodQty.Value = 0;

            if (txtLossQty != null)
                txtLossQty.Value = 0;

            if (txtParentQty != null)
            {
                txtParentQty.Value = 0;
                txtRemainQty.Value = 0;
            }

            if (txtVersion != null)
                txtVersion.Text = string.Empty;

            if (txtLaneQty != null)
                txtLaneQty.Value = 0;
            #region # 전수불량 Lane  등록
            if (txtCurLaneQty != null)
                txtCurLaneQty.Value = 0;
            #endregion
            if (txtLanePatternQty != null)
                txtLanePatternQty.Value = 0;

            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;

            if (txtWorkDate != null)
                txtWorkDate.Text = string.Empty;

            if (txtWipNote != null)
                txtWipNote.Text = string.Empty;

            if (txtWorkTime != null)
                txtWorkTime.Value = 0;

            if (txtSrs1Qty != null)
                txtSrs1Qty.Value = 0;

            if (txtSrs2Qty != null)
                txtSrs2Qty.Value = 0;

            if (txtSrs3Qty != null)
                txtSrs3Qty.Value = 0;

            if (chkFinalCut != null)
                chkFinalCut.IsChecked = false;

            txtMergeInputLot.Text = string.Empty;

            if (txtBeadMillCount != null)
                txtBeadMillCount.Value = 0;

            if (cboPet != null)
                cboPet.SelectedValue = string.Empty;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }
        }

        private void SetWipColorLegendCombo()
        {
            COLOR_COMBO.Visibility = Visibility.Visible;

            COLOR_COMBO.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            COLOR_COMBO.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow inRow = inTable.NewRow();
            inRow["LANGID"] = LoginInfo.LANGID;
            inRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            inRow["PROCID"] = procId;

            inTable.Rows.Add(inRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                COLOR_COMBO.Items.Add(cbItem);
            }
            COLOR_COMBO.SelectedIndex = 0;

            WIPCOLORLEGEND = dtResult;
        }

        void SetResultDetail()
        {
            int elementCountPerRow = 3;
            int totalRowCount = 0;
            int rowIndex = 0;
            int colIndex = 0;
            int rowIndex2 = 0;
            int colIndex2 = 0;

            List<ResultElement> elemList = new List<ResultElement>();
            List<ResultElement> elemList2 = new List<ResultElement>();
            List<ResultElement> elemList3 = new List<ResultElement>();
            List<ResultElement> elemList4 = new List<ResultElement>();

            switch (procId)
            {
                case "E0500": //PRE MIX                
                    elemList = ResultElementList.PreMixerList(elementCountPerRow);
                    break;

                case "E0400": // Binder Solution
                case "E0410": // CMC
                case "E0420": // InsulationMixing
                case "E0430": // DAM Mixing
                    elemList = ResultElementList.BinderSolution(elementCountPerRow);
                    break;

                case "E1000": //MIX
                    elemList = ResultElementList.MixerList(elementCountPerRow);
                    break;

                case "E2000": //COAT
                    if (isSingleCoater)
                    {
                        elemList = ResultElementList.SingleCoaterTopList(elementCountPerRow);
                        elemList2 = ResultElementList.SingleCoaterBackList(elementCountPerRow);
                        elemList3 = ResultElementList.SlurryAndCoreList(elementCountPerRow);
                        elemList4 = ResultElementList.BackSlurryAndCoreList(elementCountPerRow);
                    }
                    else
                    {
                        #region #전수불량 Lane 등록
                        //elemList = ResultElementList.CoaterList(elementCountPerRow);
                        elemList = ResultElementList.DefectLaneCoaterList(elementCountPerRow);
                        #endregion

                        elemList3 = ResultElementList.SlurryAndCoreList(elementCountPerRow);
                    }
                    break;

                case "E3000": //ROLL PRESS
                    #region #전수불량 Lane등록
                    //elemList = ResultElementList.RollPressList(elementCountPerRow);
                    elemList = ResultElementList.DefectLaneRollPressList(elementCountPerRow);
                    #endregion
                    break;

                case "E2500": //HALF SLITTING
                case "E4000": //SLIT
                    #region #전수불량 Lane 등록
                    //elemList = ResultElementList.SlitterList(elementCountPerRow);
                    elemList = ResultElementList.DefectLaneSlitterList(elementCountPerRow);
                    #endregion
                    break;
            }
            totalRowCount = elemList.Count + 2;

            Grid grdElement = (grdResult.Children[0] as UcResult).ResultDetail;
            grdElement.Children.Clear();

            int mod = totalRowCount / elementCountPerRow;
            int rem = totalRowCount % elementCountPerRow;

            for (int i = 0; i < mod; i++)
            {
                var rowDef = new RowDefinition();

                if (elemList[elemList.Count - 2].Control.Name.Equals("dgColorTag") || elemList[elemList.Count - 1].Control.Name.Equals("dgColorTag") || elemList[elemList.Count - 1].Control.Name.Equals("txtNote"))
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                else
                    rowDef.Height = new GridLength(30);

                grdElement.RowDefinitions.Add(rowDef);
            }

            if (rem > 0 && elemList[elemList.Count - 1].SpaceInCharge > 1)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = elemList[elemList.Count - 1].SpaceInCharge > 1 ? new GridLength(1, GridUnitType.Star) : new GridLength(30);
                grdElement.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < elementCountPerRow; i++)
            {
                var colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                grdElement.ColumnDefinitions.Add(colDef);
            }

            foreach (ResultElement re in elemList)
            {
                if (re.Control.Name.Equals("txtVersion"))
                {
                    txtVersion = re.Control as TextBox;

                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickVersion;
                }
                else if (re.Control.Name.Equals("txtLaneQty"))
                {
                    txtLaneQty = re.Control as C1NumericBox;

                    if (txtLaneQty.IsEnabled)
                    {
                        txtLaneQty.KeyDown += OnKeyDownLaneQty;
                        txtLaneQty.LostFocus += OnKeyLostFocusLaneQty;
                    }
                }
                #region #전수불량 Lane 등록
                else if (re.Control.Name.Equals("txtCurLaneQty"))
                {
                    txtCurLaneQty = re.Control as C1NumericBox;
                }
                #endregion
                else if (re.Control.Name.Equals("txtLanePatternQty"))
                {
                    txtLanePatternQty = re.Control as C1NumericBox;
                }
                else if (re.Control.Name.Equals("txtInputQty"))
                {
                    txtInputQty = re.Control as C1NumericBox;
                    txtInputQty.KeyDown += OnKeyDownOutputQty;
                }
                else if (re.Control.Name.Equals("txtGoodQty"))
                {
                    txtGoodQty = re.Control as C1NumericBox;
                    txtGoodQty.KeyDown += OnKeyDownGoodQty;
                }
                else if (re.Control.Name.Equals("txtLossQty"))
                {
                    txtLossQty = re.Control as C1NumericBox;
                }
                else if (re.Control.Name.Equals("txtWorkTime"))
                {
                    txtWorkTime = re.Control as C1NumericBox;
                }
                else if (re.Control.Name.Equals("txtWorkDate"))
                {
                    txtWorkDate = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtNote"))
                {
                    txtWipNote = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtStartDateTime"))
                {
                    txtStartDateTime = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtEndDateTime"))
                {
                    txtEndDateTime = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("chkFinalCut"))
                {
                    chkFinalCut = re.Control as CheckBox;
                }
                else if (re.Control.Name.Equals("chkExtraPress"))
                {
                    chkExtraPress = re.Control as CheckBox;
                }
                else if (re.Control.Name.Equals("chkFastTrack"))
                {
                    chkFastTrack = re.Control as CheckBox; // 2023-09-12
                    chkFastTrack.Click += chkFastTrack_Click;
                }
                else if (re.Control.Name.Equals("txtSrs1Qty"))
                {
                    txtSrs1Qty = re.Control as C1NumericBox;
                    if (txtSrs1Qty.IsEnabled)
                    {
                        txtSrs1Qty.KeyDown += OnKeyDownInOutQty;
                        txtSrs1Qty.LostFocus += OnKeyLostFocusInOutQty;
                    }
                }
                else if (re.Control.Name.Equals("txtSrs2Qty"))
                {
                    txtSrs2Qty = re.Control as C1NumericBox;
                    if (txtSrs2Qty.IsEnabled)
                    {
                        txtSrs2Qty.KeyDown += OnKeyDownInOutQty;
                        txtSrs2Qty.LostFocus += OnKeyLostFocusInOutQty;
                    }
                }
                else if (re.Control.Name.Equals("txtSrs3Qty"))
                {
                    txtSrs3Qty = re.Control as C1NumericBox;
                    if (txtSrs3Qty.IsEnabled)
                    {
                        txtSrs3Qty.KeyDown += OnKeyDownInOutQty;
                        txtSrs3Qty.LostFocus += OnKeyLostFocusInOutQty;
                    }
                }
                else if (re.Control.Name.Equals("cboPet"))
                {
                    cboPet = re.Control as C1ComboBox;
                    GetPet();
                }
                else if (re.Control.Name.Equals("txtBatchId"))
                {
                    txtBatchId = re.Control as TextBox;
                    re.PopupButton.Click += OnClickBatchId;
                }
                else if (re.Control.Name.Equals("txtTank"))
                {
                    txtTank = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtBeadMillCount"))
                {
                    txtBeadMillCount = re.Control as C1NumericBox;
                }

                if (re.Control is C1NumericBox)
                    re.Control.Style = Application.Current.Resources["C1NumericBoxStyle"] as Style;

                if (re.PopupButton != null)
                {
                    if (re.Control.Name.Equals("dgColorTag"))
                    {
                        COLORTAG_GRID = re.Control as C1DataGrid;
                        re.PopupButton.Click += OnClickSaveColorTag;
                    }

                    re.PopupButton.Style = re.Control.Name.Equals("dgColorTag") ? Application.Current.Resources["Content_SaveButtonStyle"] as Style : Application.Current.Resources["Content_SearchButtonStyle"] as Style;
                    re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                }

                var panel = new Grid();

                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(70);
                panel.ColumnDefinitions.Add(cd);

                cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                panel.ColumnDefinitions.Add(cd);

                cd = new ColumnDefinition();
                cd.Width = new GridLength(23);
                panel.ColumnDefinitions.Add(cd);

                TextBlock title = new TextBlock { Text = ObjectDic.Instance.GetObjectName(re.Title), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                title.SetValue(Grid.ColumnProperty, 0);
                panel.Children.Add(title);

                re.Control.SetValue(Grid.ColumnProperty, 1);
                panel.Children.Add(re.Control);

                if (re.PopupButton != null)
                    panel.Children.Add(re.PopupButton);

                if (re.Control.Name.Equals("dgColorTag"))
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    panel.RowDefinitions.Add(rd);

                    (re.Control as C1DataGrid).Height = 60;
                    (re.Control as C1DataGrid).VerticalAlignment = VerticalAlignment.Top;
                    (re.Control as C1DataGrid).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
                else if (re.Control.Name.Equals("txtNote"))
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    panel.RowDefinitions.Add(rd);

                    (re.Control as TextBox).TextWrapping = TextWrapping.WrapWithOverflow;
                    (re.Control as TextBox).Height = 60;
                    (re.Control as TextBox).AcceptsReturn = true;
                    (re.Control as TextBox).VerticalAlignment = VerticalAlignment.Top;
                    (re.Control as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                if (re.Control is C1DateTimePicker)
                {
                    (re.Control as C1DateTimePicker).TimeFormat = C1TimeEditorFormat.Custom;
                    (re.Control as C1DateTimePicker).DateFormat = C1DatePickerFormat.Short;
                    (re.Control as C1DateTimePicker).CustomDateFormat = "yyyy-MM-dd";
                    (re.Control as C1DateTimePicker).CustomTimeFormat = "HH:mm";
                    (re.Control as C1DateTimePicker).EditMode = C1DateTimePickerEditMode.Date;
                    (re.Control as C1DateTimePicker).MinDate = DateTime.Now.AddDays(-1);
                    (re.Control as C1DateTimePicker).MaxDate = DateTime.Now.AddDays(1);
                    (re.Control as C1DateTimePicker).Margin = new Thickness(3, 0, 0, 0);
                }

                if (re.Control is C1NumericBox || re.Control is CheckBox)
                    re.Control.Margin = new Thickness(3, 0, 0, 0);

                if (re.SpaceInCharge.Equals(elementCountPerRow))
                {
                    if (!colIndex.Equals(0))
                        rowIndex++;

                    colIndex = 0;
                    panel.SetValue(Grid.ColumnSpanProperty, grdElement.ColumnDefinitions.Count);
                }

                panel.SetValue(Grid.RowProperty, rowIndex);
                panel.SetValue(Grid.ColumnProperty, colIndex);

                grdElement.Children.Add(panel);

                colIndex += re.SpaceInCharge;

                if (colIndex % elementCountPerRow == 0)
                    rowIndex++;

                colIndex = colIndex % elementCountPerRow == 0 ? 0 : colIndex;
            }

            if (elemList2.Count > 0)
            {
                totalRowCount = elemList2.Count + 2;

                grdElement = (grdResult.Children[0] as UcResult).ResultDetail2;
                grdElement.Children.Clear();

                mod = totalRowCount / elementCountPerRow;
                rem = totalRowCount % elementCountPerRow;

                for (int i = 0; i < mod; i++)
                {
                    var rowDef = new RowDefinition();

                    if (elemList2[elemList2.Count - 1].Control.Name.Equals("txtNote"))
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                    else
                        rowDef.Height = new GridLength(30);

                    grdElement.RowDefinitions.Add(rowDef);
                }

                if (rem > 0 && elemList2[elemList2.Count - 1].SpaceInCharge > 1)
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = elemList2[elemList2.Count - 1].SpaceInCharge > 1 ? new GridLength(90) : new GridLength(30);
                    grdElement.RowDefinitions.Add(rowDef);
                }

                for (int i = 0; i < elementCountPerRow; i++)
                {
                    var colDef = new ColumnDefinition();
                    colDef.Width = new GridLength(1, GridUnitType.Star);
                    grdElement.ColumnDefinitions.Add(colDef);
                }

                foreach (ResultElement re in elemList2)
                {
                    if (re.Control.Name.Equals("txtVersion"))
                    {
                        txtVersion = re.Control as TextBox;

                        if (re.PopupButton != null)
                            re.PopupButton.Click += OnClickVersion;
                    }
                    else if (re.Control.Name.Equals("txtLaneQty"))
                    {
                        txtLaneQty = re.Control as C1NumericBox;

                        if (txtLaneQty.IsEnabled)
                        {
                            txtLaneQty.KeyDown += OnKeyDownLaneQty;
                            txtLaneQty.LostFocus += OnKeyLostFocusLaneQty;
                        }
                    }
                    else if (re.Control.Name.Equals("txtLanePatternQty"))
                    {
                        txtLanePatternQty = re.Control as C1NumericBox;
                    }
                    else if (re.Control.Name.Equals("txtInputQty"))
                    {
                        txtInputQty = re.Control as C1NumericBox;
                        txtInputQty.KeyDown += OnKeyDownOutputQty;
                    }
                    else if (re.Control.Name.Equals("txtGoodQty"))
                    {
                        txtGoodQty = re.Control as C1NumericBox;
                        txtGoodQty.KeyDown += OnKeyDownGoodQty;
                    }
                    else if (re.Control.Name.Equals("txtLossQty"))
                    {
                        txtLossQty = re.Control as C1NumericBox;
                    }
                    else if (re.Control.Name.Equals("txtWorkTime"))
                    {
                        txtWorkTime = re.Control as C1NumericBox;
                    }
                    else if (re.Control.Name.Equals("txtWorkDate"))
                    {
                        txtWorkDate = re.Control as TextBox;
                    }
                    else if (re.Control.Name.Equals("txtNote"))
                    {
                        txtWipNote = re.Control as TextBox;
                    }
                    else if (re.Control.Name.Equals("txtStartDateTime"))
                    {
                        txtStartDateTime = re.Control as TextBox;
                    }
                    else if (re.Control.Name.Equals("txtEndDateTime"))
                    {
                        txtEndDateTime = re.Control as TextBox;
                    }
                    else if (re.Control.Name.Equals("chkFinalCut"))
                    {
                        chkFinalCut = re.Control as CheckBox;
                    }
                    else if (re.Control.Name.Equals("chkExtraPress"))
                    {
                        chkExtraPress = re.Control as CheckBox;
                    }
                    else if (re.Control.Name.Equals("txtSrs1Qty"))
                    {
                        txtSrs1Qty = re.Control as C1NumericBox;
                        if (txtSrs1Qty.IsEnabled)
                        {
                            txtSrs1Qty.KeyDown += OnKeyDownInOutQty;
                            txtSrs1Qty.LostFocus += OnKeyLostFocusInOutQty;
                        }
                    }
                    else if (re.Control.Name.Equals("txtSrs2Qty"))
                    {
                        txtSrs2Qty = re.Control as C1NumericBox;
                        if (txtSrs2Qty.IsEnabled)
                        {
                            txtSrs2Qty.KeyDown += OnKeyDownInOutQty;
                            txtSrs2Qty.LostFocus += OnKeyLostFocusInOutQty;
                        }
                    }
                    else if (re.Control.Name.Equals("txtSrs3Qty"))
                    {
                        txtSrs3Qty = re.Control as C1NumericBox;
                        if (txtSrs3Qty.IsEnabled)
                        {
                            txtSrs3Qty.KeyDown += OnKeyDownInOutQty;
                            txtSrs3Qty.LostFocus += OnKeyLostFocusInOutQty;
                        }
                    }
                    else if (re.Control.Name.Equals("cboPet"))
                    {
                        cboPet = re.Control as C1ComboBox;
                        GetPet();
                    }
                    else if (re.Control.Name.Equals("txtBatchId"))
                    {
                        txtBatchId = re.Control as TextBox;
                        re.PopupButton.Click += OnClickBatchId;
                    }
                    else if (re.Control.Name.Equals("txtTank"))
                    {
                        txtTank = re.Control as TextBox;
                    }

                    if (re.Control is C1NumericBox)
                        re.Control.Style = Application.Current.Resources["C1NumericBoxStyle"] as Style;

                    if (re.PopupButton != null)
                    {
                        if (re.Control.Name.Equals("dgColorTag"))
                        {
                            COLORTAG_GRID = re.Control as C1DataGrid;
                            re.PopupButton.Click += OnClickSaveColorTag;
                        }

                        re.PopupButton.Style = re.Control.Name.Equals("dgColorTag") ? Application.Current.Resources["Content_SaveButtonStyle"] as Style : Application.Current.Resources["Content_SearchButtonStyle"] as Style;
                        re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                    }

                    var panel = new Grid();

                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(70);
                    panel.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(1, GridUnitType.Star);
                    panel.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(23);
                    panel.ColumnDefinitions.Add(cd);

                    TextBlock title = new TextBlock { Text = ObjectDic.Instance.GetObjectName(re.Title), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                    title.SetValue(Grid.ColumnProperty, 0);
                    panel.Children.Add(title);

                    re.Control.SetValue(Grid.ColumnProperty, 1);
                    panel.Children.Add(re.Control);

                    if (re.PopupButton != null)
                        panel.Children.Add(re.PopupButton);

                    if (re.Control.Name.Equals("dgColorTag"))
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength(1, GridUnitType.Star);
                        panel.RowDefinitions.Add(rd);

                        (re.Control as C1DataGrid).Height = 60;
                        (re.Control as C1DataGrid).VerticalAlignment = VerticalAlignment.Top;
                        (re.Control as C1DataGrid).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    }
                    else if (re.Control.Name.Equals("txtNote"))
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength(1, GridUnitType.Star);
                        panel.RowDefinitions.Add(rd);

                        (re.Control as TextBox).TextWrapping = TextWrapping.WrapWithOverflow;
                        (re.Control as TextBox).Height = 60;
                        (re.Control as TextBox).AcceptsReturn = true;
                        (re.Control as TextBox).VerticalAlignment = VerticalAlignment.Top;
                        (re.Control as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    }

                    if (re.Control is C1DateTimePicker)
                    {
                        (re.Control as C1DateTimePicker).TimeFormat = C1TimeEditorFormat.Custom;
                        (re.Control as C1DateTimePicker).DateFormat = C1DatePickerFormat.Custom;
                        (re.Control as C1DateTimePicker).CustomDateFormat = "yyyy-MM-dd";
                        (re.Control as C1DateTimePicker).CustomTimeFormat = "HH:mm";
                        (re.Control as C1DateTimePicker).EditMode = C1DateTimePickerEditMode.Date;
                        (re.Control as C1DateTimePicker).MinDate = DateTime.Now.AddDays(-1);
                        (re.Control as C1DateTimePicker).MaxDate = DateTime.Now.AddDays(1);
                        (re.Control as C1DateTimePicker).Margin = new Thickness(3, 0, 0, 0);
                    }

                    if (re.Control is C1NumericBox || re.Control is CheckBox)
                        re.Control.Margin = new Thickness(3, 0, 0, 0);

                    if (re.SpaceInCharge.Equals(elementCountPerRow))
                    {
                        if (!colIndex2.Equals(0))
                            rowIndex2++;

                        colIndex2 = 0;
                        panel.SetValue(Grid.ColumnSpanProperty, grdElement.ColumnDefinitions.Count);
                    }

                    panel.SetValue(Grid.RowProperty, rowIndex2);
                    panel.SetValue(Grid.ColumnProperty, colIndex2);

                    grdElement.Children.Add(panel);

                    colIndex2 += re.SpaceInCharge;

                    if (colIndex2 % elementCountPerRow == 0)
                        rowIndex2++;

                    colIndex2 = colIndex2 % elementCountPerRow == 0 ? 0 : colIndex2;
                }

                (grdResult.Children[0] as UcResult).ResultDetail2.Visibility = Visibility.Collapsed;
            }

            // 좌측 하단 Grid에 Coater Foil, Slurry 추가 (2017-01-11) CR-42
            if (elemList3.Count > 0)
            {
                SetCoaterResult(elemList3, (grdResult.Children[0] as UcResult).CoaterSlurry, elementCountPerRow);

                if (elemList2.Count > 0)
                {
                    SetCoaterResult(elemList4, (grdResult.Children[0] as UcResult).CoaterSlurry2, elementCountPerRow);
                    (grdResult.Children[0] as UcResult).CoaterSlurry2.Visibility = Visibility.Collapsed;
                }
            }
        }

        void SetCoaterResult(List<ResultElement> elementList, Grid grid, int elementCountRow)
        {
            // 좌측 하단 Grid에 Coater Foil, Slurry 추가 (2017-01-11) CR-42
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);

            int rowIndex = 0;
            int colIndex = 0;
            int totalRowCount = elementList.Count;

            int mod = (totalRowCount / elementCountRow) + 1;

            for (int i = 0; i < mod; i++)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = new GridLength(30);
                grid.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < elementCountRow; i++)
            {
                var colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(colDef);
            }

            // BOARDER 추가
            Border border = new Border();
            border.SetValue(Grid.RowProperty, rowIndex);
            border.SetValue(Grid.ColumnProperty, colIndex);
            border.SetValue(Grid.ColumnSpanProperty, elementCountRow);
            border.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Top);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 2, 0, 0));
            BrushConverter converter = new BrushConverter();
            border.BorderBrush = converter.ConvertFromString("#ee5283") as Brush;
            grid.Children.Add(border);

            foreach (ResultElement re in elementList)
            {
                if (re.Control.Name.Equals("txtCore1"))
                {
                    txtCore1 = re.Control as TextBox;

                    if (re.radButton != null)
                        re.radButton.Checked += OnCheckFoilChecked;
                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickTopLotOpen;
                }
                else if (re.Control.Name.Equals("txtCore2"))
                {
                    txtCore2 = re.Control as TextBox;

                    if (re.radButton != null)
                        re.radButton.Checked += OnCheckFoilChecked;
                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickTopLotOpen;
                }
                else if (re.Control.Name.Equals("txtSlurry1"))
                {
                    txtSlurry1 = re.Control as TextBox;

                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickSlurryOpen;

                }
                else if (re.Control.Name.Equals("txtSlurry2"))
                {
                    txtSlurry2 = re.Control as TextBox;

                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickSlurryOpen;
                }
                else if (re.Control.Name.Equals("btnMtrlChange"))
                {
                    btnMtrlChange = re.Control as Button;
                    btnMtrlChange.Click += OnClickMtrlChange;
                }
                else if (re.Control.Name.Equals("btnMtrlChange2"))
                {
                    btnMtrlChange2 = re.Control as Button;
                    btnMtrlChange2.Click += OnClickMtrlChange2;
                }

                if (re.PopupButton != null)
                {
                    re.PopupButton.Style = Application.Current.Resources["Content_SearchButtonStyle"] as Style;
                    re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                }
                if (re.radButton != null)
                {
                    re.radButton.SetValue(Grid.ColumnProperty, 2);
                }

                var panel = new Grid();

                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(70);
                panel.ColumnDefinitions.Add(cd);

                cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                panel.ColumnDefinitions.Add(cd);

                if (!re.Control.GetType().Name.Equals("Button"))
                {
                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(23);
                    panel.ColumnDefinitions.Add(cd);

                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(23);
                    panel.ColumnDefinitions.Add(cd);
                }
                else
                {
                    re.Control.Style = Application.Current.Resources["Content_MainButtonNoMinWidthSpecialStyle"] as Style;
                }

                TextBlock title = new TextBlock { Text = re.Title, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                title.SetValue(Grid.ColumnProperty, 0);
                panel.Children.Add(title);

                re.Control.SetValue(Grid.ColumnProperty, 1);
                panel.Children.Add(re.Control);

                if (re.PopupButton != null && re.radButton != null)
                {
                    re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                    panel.Children.Add(re.PopupButton);

                    re.radButton.SetValue(Grid.ColumnProperty, 3);
                    panel.Children.Add(re.radButton);
                }
                else
                {
                    if (re.PopupButton != null)
                    {
                        re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                        re.PopupButton.SetValue(Grid.ColumnSpanProperty, 2);
                        panel.Children.Add(re.PopupButton);
                    }
                    else if (re.radButton != null)
                    {
                        re.radButton.SetValue(Grid.ColumnProperty, 2);
                        re.radButton.SetValue(Grid.ColumnSpanProperty, 2);
                        panel.Children.Add(re.radButton);
                    }
                }

                if (re.IsNewLine)
                {
                    rowIndex++;
                    colIndex = 0;
                }

                panel.SetValue(Grid.RowProperty, rowIndex);
                panel.SetValue(Grid.ColumnProperty, colIndex);

                grid.Children.Add(panel);

                colIndex += re.SpaceInCharge;
            }
        }

        private void GetWorkOrder()
        {
            UC_WORKORDER_CWA wo1;
            UC_WORKORDER_MX_CWA wo2;

            ClearControls();

            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                wo1 = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                wo1.FrameOperation = FrameOperation;
                wo1._UCElec_CWA = this;
                wo1.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                wo1.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
                wo1.PROCID = procId;

                if (string.Equals(procId, Process.COATING) && isSingleCoater == true)
                    wo1.COATSIDETYPE = Util.NVC(COATTYPE_COMBO.SelectedValue);

                wo1.GetWorkOrder();
                if (string.Equals(procId, Process.COATING))
                {
                    chkLANEqtyAble2();
                }
            }
            else
            {
                wo2 = grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA;

                wo2.FrameOperation = FrameOperation;
                wo2._UCElec_CWA = this;
                wo2.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                wo2.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
                wo2.PROCID = procId;
                wo2.GetWorkOrder();
            }

            if ((string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.BS) || string.Equals(procId, Process.CMC) || string.Equals(procId, Process.InsulationMixing)) && (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA) != null)
            {
                WORKORDER_GRID = (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA).dgWorkOrder;
            }
            else
            {
                WORKORDER_GRID = (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).dgWorkOrder;
            }
        }

        private void GetWorkOrderNotClear()
        {
            UC_WORKORDER_CWA wo1;
            UC_WORKORDER_MX_CWA wo2;

            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                wo1 = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                wo1.FrameOperation = FrameOperation;
                wo1._UCElec_CWA = this;
                wo1.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                wo1.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
                wo1.PROCID = procId;

                if (string.Equals(procId, Process.COATING) && isSingleCoater == true)
                    wo1.COATSIDETYPE = Util.NVC(COATTYPE_COMBO.SelectedValue);

                wo1.GetWorkOrder();
            }
            else
            {
                wo2 = grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA;

                wo2.FrameOperation = FrameOperation;
                wo2._UCElec_CWA = this;
                wo2.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
                wo2.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
                wo2.PROCID = procId;
                wo2.GetWorkOrder();
            }

            if ((string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.SRS_MIXING)) && (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA) != null)
            {
                WORKORDER_GRID = (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA).dgWorkOrder;
            }
            else
            {
                WORKORDER_GRID = (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).dgWorkOrder;
            }
        }

        private void GetLargeLot()
        {
            try
            {
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                    return;

                string sPrvLot = string.Empty;

                if (PRODLOT_GRID.ItemsSource != null && PRODLOT_GRID.Rows.Count > 0)
                {
                    int idx = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = procId;
                Indata["EQPTID"] = EQUIPMENT_COMBO.SelectedValue.ToString();

                if (isSingleCoater)
                    Indata["COAT_SIDE_TYPE"] = Util.NVC(COATTYPE_COMBO.SelectedValue);

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(GetLargeLotBizRuleName(), "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(LARGELOT_GRID, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectLevel()
        {
            try
            {
                string[] Level = { "LV1", "LV2", "LV3" };

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LV_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();


                DataTable dtAddAll = new DataTable();
                dtAddAll.Columns.Add("CHK", typeof(string));
                dtAddAll.Columns.Add("LV_NAME", typeof(string));
                dtAddAll.Columns.Add("LV_CODE", typeof(string));

                DataRow AddData = dtAddAll.NewRow();

                for (int i = 0; i < Level.Count(); i++)
                {
                    AddData["CHK"] = 0;
                    AddData["LV_NAME"] = "ALL";
                    AddData["LV_CODE"] = "ALL";
                    dtAddAll.Rows.Add(AddData);

                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = procId;
                    Indata["LV_CODE"] = Level[i];

                    IndataTable.Rows.Add(Indata);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC_LEVEL", "RQSTDT", "RSLTDT", IndataTable);

                    dtAddAll.Merge(dtResult);

                    if (i == 0)
                        Util.GridSetData((grdDataCollect.Children[0] as UcDataCollect).dgLevel1, dtAddAll, FrameOperation, true);
                    else if (i == 1)
                        Util.GridSetData((grdDataCollect.Children[0] as UcDataCollect).dgLevel2, dtAddAll, FrameOperation, true);
                    else if (i == 2)
                        Util.GridSetData((grdDataCollect.Children[0] as UcDataCollect).dgLevel3, dtAddAll, FrameOperation, true);

                    IndataTable.Clear();
                    dtAddAll.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetProductLot(DataRowView drv = null)
        {
            try
            {
                string ValueToCondition = string.Empty;
                var sCond = new List<string>();

                SetStatus();

                if (string.IsNullOrEmpty(WIPSTATUS))
                {
                    Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                if (procId.Equals(Process.ROLL_PRESSING))
                    chkExtraPress.Visibility = Visibility.Visible;

                // SLITTER CUT기준으로 변경으로 인하여 LOT ID -> CUT ID로 표기 ( 2017-01-21 ) CR-53
                if ((string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING)) && !string.Equals(WIPSTATUS, Wip_State.WAIT))
                {
                    PRODLOT_GRID.Columns["CUT_ID"].Visibility = Visibility.Visible;
                    PRODLOT_GRID.Columns["LOTID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    PRODLOT_GRID.Columns["CUT_ID"].Visibility = Visibility.Collapsed;
                    PRODLOT_GRID.Columns["LOTID"].Visibility = Visibility.Visible;
                }

                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                    return;

                string sPrvLot = string.Empty;

                if (PRODLOT_GRID.ItemsSource != null && PRODLOT_GRID.Rows.Count > 0)
                {
                    int idx = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                Util.gridClear(PRODLOT_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID_LARGE", typeof(string));
                IndataTable.Columns.Add("WIPSTAT", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("COATSIDE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = procId;
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);

                if (drv != null)
                    Indata["LOTID_LARGE"] = drv["LOTID_LARGE"].ToString();

                Indata["WIPSTAT"] = WIPSTATUS;
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);  // 자동차 전극은 EQSG가 2개라서 변경 가능하기 때문에 선택된 걸로 변경 ( 2017-01-20 )

                if (isSingleCoater || procId.Equals(Process.INS_COATING) || procId.Equals(Process.INS_SLIT_COATING))
                    Indata["COATSIDE"] = COATTYPE_COMBO.SelectedValue;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(GetProdLotBizRuleName(), "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(PRODLOT_GRID, dtResult, FrameOperation, true);

                // R/P공정에서 QA 샘플링 대상 LOT일시 LOTID 붉은색으로 표시 [2017-04-23]
                // T/P공정도 COATER 첫 번째 CUT일 경우 붉은색으로 표시
                if (string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.TAPING))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (string.Equals(dtResult.Rows[i]["QA_INSP_TRGT_FLAG"], "Y") && PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter != null)
                        {
                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                }

                // 코터 공정에서 1번 CUT일 경우 붉은색으로 표시하도록 설정
                if (string.Equals(procId, Process.COATING))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (string.Equals(dtResult.Rows[i]["CUT"], "1") && PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter != null)
                        {
                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }

                        // [E20230228-000007] CSR - Marking logic in GMES
                        // 속도 문제 로 인해 OnLoadedProdLotCellPresenter 구현하지 않음
                        if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["LOTID"].Index).Presenter != null)
                        {
                            if (string.Equals(GetCutLotSampleQAFalg(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[i].DataItem, "LOTID")), Process.COATING), "Y"))
                            {
                                for (int k = 0; k < PRODLOT_GRID.Columns.Count; k++)
                                {
                                    if (PRODLOT_GRID.Columns[k].Visibility == Visibility.Visible)
                                    {
                                        if (PRODLOT_GRID.GetCell(i, k).Presenter != null)
                                        {
                                            PRODLOT_GRID.GetCell(i, k).Presenter.Background = new SolidColorBrush(Colors.Red);
                                        }

                                        if (PRODLOT_GRID.GetCell(i, k).Presenter != null)
                                        {
                                            PRODLOT_GRID.GetCell(i, k).Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                //LOTID 고정
                if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING))
                    PRODLOT_GRID.FrozenColumnCount = 3;

                // 믹스 실적 확정 메시지 Timer CSR[C20201014-000233]
                if (string.Equals(procId, Process.MIXING) && dtResult.Rows.Count > 0)
                {
                    MixTimer_Start();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MixTimer_Start()
        {
            //팝업 설정 Area 조회
            DataTable InData = new DataTable();
            InData.TableName = "RQSTDT";
            InData.Columns.Add("LANGID", typeof(string));
            InData.Columns.Add("CMCDTYPE", typeof(string));
            InData.Columns.Add("CMCODE", typeof(string));

            DataRow dr = InData.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MIX_TIMER_POPUP";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            InData.Rows.Add(dr);

            DataTable dtMsgArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", InData);

            if (dtMsgArea.Rows.Count == 0)
                return;

            //Timer 중복 Start 방지위함
            if (isTimerCheck == false)
            {
                Mix_Timer.Start();
                isTimerCheck = true;
            }
        }

        private void MixTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            if (PRODLOT_GRID.ItemsSource == null || PRODLOT_GRID.Rows.Count <= 0)
                return;

            isTimerTabCk = false;
            //Menuid 값 가져오기
            string MENUID = Convert.ToString(DataTableConverter.GetValue(DataContext, "MENUID"));
            //Tab중 믹서 공정 UI화면 존재 확인
            foreach (Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    foreach (UIElement ui in tmp.Children)
                    {
                        C1TabControl tabCtrl = ui as C1TabControl;

                        if (tabCtrl != null)
                        {
                            for (int i = 0; i < tabCtrl.Items.Count; i++)
                            {
                                C1TabItem tabItem = tabCtrl.Items[i] as C1TabItem;

                                if (MENUID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "MENUID")))
                                {
                                    isTimerTabCk = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //믹서 공정진척 UI 종료되면 Timer Stop처리
            if (isTimerTabCk == false)
            {
                Mix_Timer.Stop();
                return;
            }

            // DB 서버 시간 조회
            DataTable dtResult = new ClientProxy().ExecuteServiceSync_Multi("BR_CUS_GET_SYSTIME", "", "OUTDATA", new DataSet()).Tables["OUTDATA"];
            string cutTime = Convert.ToDateTime(dtResult.Rows[0]["SYSTIME"]).ToString("HH:mm").Replace(":", "");


            //Area별 CutOff 시간 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult2.Rows.Count == 0)
                return;

            //CutOff 시간보다 15분 전 부터 시작
            string sTime = Convert.ToString(Convert.ToInt16(Util.NVC(dtResult2.Rows[0]["S02"]).Substring(0, 4)) - 17);
            string eTime = Util.NVC(dtResult2.Rows[0]["S02"]).Substring(0, 4);


            //Cutoff 시간 15분전 부터 메시지 실행
            if (Convert.ToInt16(cutTime) >= Convert.ToInt32(sTime) && Convert.ToInt16(cutTime) <= Convert.ToInt32(eTime))
            {
                //믹스 공정UI 존재하지만 비활성화 이면 Tab 활성화 처리
                foreach (Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        foreach (UIElement ui in tmp.Children)
                        {
                            C1TabControl tabCtrl = ui as C1TabControl;

                            if (tabCtrl != null)
                            {
                                for (int i = 0; i < tabCtrl.Items.Count; i++)
                                {
                                    C1TabItem tabItem = tabCtrl.Items[i] as C1TabItem;

                                    if (MENUID.Equals(DataTableConverter.GetValue(tabItem.DataContext, "MENUID")))
                                    {
                                        tabCtrl.SelectedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int j = 0; j < PRODLOT_GRID.Rows.Count; j++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[j].DataItem, "WIPSTAT")).Equals("EQPT_END"))
                    {
                        //메시지 팝업 이중 처리 방지
                        if (isTimerPopup == true)
                            return;

                        Util.MessageInfo("SFU8293", (result) =>     //실적 확정 요청 메시지
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                isTimerPopup = false;
                            }
                        }, isTimerPopup = true);
                        break;
                    }
                }
            }
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 0 || EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택 하세요.
                return bRet;
            }

            if (EQUIPMENT_COMBO.SelectedIndex < 0 || EQUIPMENT_COMBO.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택 하세요.
                return bRet;
            }
            bRet = true;
            return bRet;
        }

        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            DataRowView drv = dataitem.DataItem as DataRowView;
            string sInputLot;
            string sChildSeq;
            string sLot;

            try
            {
                sInputLot = drv["LOTID_PR"].ToString().Equals(string.Empty) ? drv["LOTID"].ToString() : drv["LOTID_PR"].ToString();
            }
            catch
            {
                sInputLot = string.Empty;
            }

            try
            {
                sChildSeq = string.IsNullOrEmpty(drv["CUT_ID"].ToString()) ? "1" : drv["CUT_ID"].ToString();
            }
            catch
            {
                sChildSeq = "1";
            }

            try
            {
                sLot = drv["LOTID"].ToString();
            }
            catch
            {
                sLot = string.Empty;
            }

            if (!string.IsNullOrEmpty(sInputLot) && !string.IsNullOrEmpty(sChildSeq))
            {
                // 모두 Uncheck 처리 및 동일 자LOT의 경우는 Check 처리.
                for (int i = 0; i < PRODLOT_GRID.Rows.Count; i++)
                {
                    if (dataitem.Index != i)
                    {
                        if (sInputLot == Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[i].DataItem, "LOTID_PR")) &&
                            sChildSeq == Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[i].DataItem, "CUT_ID")))
                        {
                            if (sInputLot.Equals(""))
                            {
                                if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter != null &&
                                    PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content != null &&
                                    (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                    (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                DataTableConverter.SetValue(PRODLOT_GRID.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (bUncheckAll)
                                {
                                    if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter != null &&
                                    PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content != null &&
                                    (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(PRODLOT_GRID.Rows[i].DataItem, "CHK", false);
                                }
                                else
                                {
                                    // CNA에 같은 대LOT에 같은 GR_SEQ로 COATER LOT이 2개씩 생성되어 SLITTER계열만 동일 선택 처리 [2017-07-05]
                                    if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                                    {
                                        if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter != null &&
                                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content != null &&
                                            (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                            (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;

                                        DataTableConverter.SetValue(PRODLOT_GRID.Rows[i].DataItem, "CHK", true);
                                    }
                                    else if (!string.Equals(sLot, Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[i].DataItem, "LOTID"))))
                                    {
                                        if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter != null &&
                                            PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content != null &&
                                            (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                            (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                        DataTableConverter.SetValue(PRODLOT_GRID.Rows[i].DataItem, "CHK", false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter != null &&
                                PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content != null &&
                                (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                (PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                            DataTableConverter.SetValue(PRODLOT_GRID.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            return true;
        }

        public void RunProcess(string ValueToLotID)
        {
            if (procId.Equals(Process.HALF_SLITTING))
            {
                try
                {
                    Util.MessageConfirm("SFU1240", (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            DataTable eqpt_mount = new DataTable();
                            eqpt_mount.Columns.Add("EQPTID", typeof(string));

                            DataRow eqpt_mount_row = eqpt_mount.NewRow();
                            eqpt_mount_row["EQPTID"] = EQUIPMENT_COMBO.SelectedValue.ToString();
                            eqpt_mount.Rows.Add(eqpt_mount_row);

                            DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                            if (EQPT_PSTN_ID.Rows.Count > 0)
                            {
                                eqptMountPositionCode = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                            }
                            else
                            {
                                Util.MessageValidation("SFU1397");  //MMD에 설비 투입 위치를 입력해 주세요.
                                return;
                            }

                            DataSet inDataSet = new DataSet();
                            DataRow inLotDataRow;
                            DataRow inMtrlDataRow;

                            #region MESSAGE SET
                            DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");

                            inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inLotDataTable.Columns.Add("IFMODE", typeof(string));
                            inLotDataTable.Columns.Add("EQPTID", typeof(string));
                            inLotDataTable.Columns.Add("USERID", typeof(string));

                            inLotDataRow = inLotDataTable.NewRow();

                            inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inLotDataRow["EQPTID"] = EQUIPMENT_COMBO.SelectedValue.ToString();
                            inLotDataRow["USERID"] = LoginInfo.USERID;

                            inLotDataTable.Rows.Add(inLotDataRow);

                            DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                            InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                            inMtrlDataRow = InMtrldataTable.NewRow();
                            inMtrlDataRow["INPUT_LOTID"] = ValueToLotID;
                            inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = eqptMountPositionCode;
                            inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                            InMtrldataTable.Rows.Add(inMtrlDataRow);
                            #endregion

                            new ClientProxy().ExecuteService_Multi(GetStartLotBizRuleName(), "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                REFRESH = true;

                            }, inDataSet);
                        }
                    });
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
            else if (procId.Equals(Process.SLITTING))
            {
                try
                {
                    Util.MessageConfirm("SFU1240", (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            DataSet inDataSet = new DataSet();

                            #region MESSAGE SET
                            DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");
                            inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inLotDataTable.Columns.Add("IFMODE", typeof(string));
                            inLotDataTable.Columns.Add("EQPTID", typeof(string));
                            inLotDataTable.Columns.Add("USERID", typeof(string));

                            DataRow inLotDataRow = null;
                            inLotDataRow = inLotDataTable.NewRow();
                            inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inLotDataRow["EQPTID"] = EQUIPMENT_COMBO.SelectedValue.ToString();
                            inLotDataRow["USERID"] = LoginInfo.USERID;
                            inLotDataTable.Rows.Add(inLotDataRow);

                            DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                            InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                            DataRow inMtrlDataRow = null;

                            inMtrlDataRow = InMtrldataTable.NewRow();
                            inMtrlDataRow["INPUT_LOTID"] = ValueToLotID;
                            inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(EQUIPMENT_COMBO.SelectedValue.ToString());
                            inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                            InMtrldataTable.Rows.Add(inMtrlDataRow);
                            #endregion

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_SL", "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                #region QC 샘플 PopUp
                                DataTable dtCutTable = result.Tables["RSLTDT"];

                                string strCutid = dtCutTable.Rows[0]["CUT_ID"].ToString();

                                //2020.12.23 Slitter QC 샘플 화면 POPUP 
                                //PROCESSEQUIPMENTSEGMENT.GQMS_SMPLG_POPUP_APPLY_FLAG 값이 'Y'이면 실행
                                DataTable InTable = new DataTable();
                                InTable.Columns.Add("PROCID", typeof(string));
                                InTable.Columns.Add("EQSGID", typeof(string));

                                DataRow dtRow = InTable.NewRow();
                                dtRow["PROCID"] = Process.SLITTING;
                                dtRow["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                                InTable.Rows.Add(dtRow);

                                DataTable dtProcEqst = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", InTable);

                                if (Convert.ToString(dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"]) == "Y")
                                {
                                    DataSet inDataSet2 = new DataSet();

                                    //Slitter QC 샘플 POPUP 실행 비즈 호출
                                    DataTable InEqpTable = inDataSet2.Tables.Add("IN_EQP");
                                    InEqpTable.Columns.Add("SRCTYPE", typeof(string));
                                    InEqpTable.Columns.Add("IFMODE", typeof(string));
                                    InEqpTable.Columns.Add("EQPTID", typeof(string));
                                    InEqpTable.Columns.Add("USERID", typeof(string));
                                    InEqpTable.Columns.Add("SMPL_REG_YN", typeof(string));

                                    DataRow drEqp = null;
                                    drEqp = InEqpTable.NewRow();
                                    drEqp["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    drEqp["IFMODE"] = IFMODE.IFMODE_OFF;
                                    drEqp["EQPTID"] = EQUIPMENT_COMBO.SelectedValue.ToString();
                                    drEqp["USERID"] = LoginInfo.USERID;
                                    drEqp["SMPL_REG_YN"] = dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"].ToString();
                                    InEqpTable.Rows.Add(drEqp);

                                    DataTable dtLotTable = inDataSet2.Tables.Add("IN_LOT");
                                    dtLotTable.Columns.Add("LOTID", typeof(string));

                                    DataRow drLot = null;
                                    drLot = dtLotTable.NewRow();
                                    drLot["LOTID"] = strCutid;

                                    dtLotTable.Rows.Add(drLot);

                                    DataSet dsInfoRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHK_LOT_SAMPLING_SL", "IN_EQP,IN_LOT", "OUTDATA", inDataSet2);

                                    DataTable dtInfo = dsInfoRslt.Tables["OUTDATA"];

                                    if (dtInfo.Rows[0]["MSGTYPE"].Equals("INFO"))
                                    {
                                        //Message 등록 여부 확인
                                        DataTable dtCode = new DataTable();
                                        dtCode.Columns.Add("LANGID", typeof(string));
                                        dtCode.Columns.Add("MSGID", typeof(string));

                                        DataRow dtRowMsg = dtCode.NewRow();
                                        dtRowMsg["LANGID"] = LoginInfo.LANGID;
                                        dtRowMsg["MSGID"] = "1" + dtInfo.Rows[0]["MSGCODE"].ToString(); //기존 메시지 변경 불가로 신규 메시지로 처리

                                        dtCode.Rows.Add(dtRowMsg);

                                        DataTable dtMessage = new ClientProxy().ExecuteServiceSync("DA_SEL_MESSAGE_LOT_START_RP", "INDATA", "RSLTDT", dtCode);

                                        if (dtMessage == null || dtMessage.Rows.Count == 0)
                                        {
                                            Util.MessageValidation("SFU1392");  //MESSAGE 테이블에 메세지를 등록해주세요
                                            return;
                                        }

                                        if (string.Equals(dtInfo.Rows[0]["MSGCODE"], "95000")) // 해당BIZ에서 95000번을 샘플링으로 리턴
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2960", new object[] { Convert.ToString(dtMessage.Rows[0]["MSGNAME"]) })
                                                , true, true, ObjectDic.Instance.GetObjectName("바코드발행"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Barresult, isBarCode) =>
                                                {
                                                    if (isBarCode == true)
                                                    {
                                                        if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                                                        {
                                                            Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                                                            return;
                                                        }

                                                        DataTable LabelDT = new DataTable();
                                                        LabelDT = (LOTINFO_GRID.ItemsSource as DataView).Table;

                                                        #region [샘플링 출하거래처 추가]
                                                        // SAMPLING용 발행 매수 추가
                                                        int iSamplingCount;

                                                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                                            foreach (DataRow _iRow in LabelDT.Rows)
                                                            {
                                                                iSamplingCount = 0;
                                                                string[] sCompany = null;
                                                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                                                {
                                                                    iSamplingCount = Util.NVC_Int(items.Key);
                                                                    sCompany = Util.NVC(items.Value).Split(',');
                                                                }

                                                                for (int i = 0; i < iSamplingCount; i++)
                                                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), procId, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                                            }
                                                        foreach (DataRow _iRow in LabelDT.Rows)
                                                            Util.UpdatePrintExecCount(Util.NVC(_iRow["LOTID"]), procId);
                                                        #endregion
                                                    }
                                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                                });
                                        }
                                        else
                                        {
                                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                        }
                                    }
                                    else
                                    {
                                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                    }

                                }
                                else
                                {
                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                }
                                #endregion

                                //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                REFRESH = true;
                            }, inDataSet);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            LOTID = Util.NVC(rowview["LOTID"]);
            _LOTID = Util.NVC(rowview["LOTID"]);
            _WIPSEQ = Util.NVC(rowview["WIPSEQ"]);

            if (!procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !procId.Equals(Process.MIXING) && !procId.Equals(Process.SRS_MIXING))
            {
                if (LARGELOT_GRID.Visibility == Visibility.Visible)
                    _LOTIDPR = Util.NVC(DataTableConverter.GetValue(rowview, "LOTID_LARGE"));
                else
                    _LOTIDPR = Util.NVC(rowview["LOTID_PR"]);
            }

            _WIPSTAT = Util.NVC(rowview["WIPSTAT"]);
            _WIPDTTM_ST = Util.NVC(rowview["WIPDTTM_ST"]);
            _WIPDTTM_ED = Util.NVC(rowview["WIPDTTM_ED"]);
            _REMARK = Util.NVC(rowview["REMARK"]);
            _PRODID = Util.NVC(rowview["PRODID"]);
            _EQPTID = Util.NVC(rowview["EQPTID"]);
            _WORKORDER = Util.NVC(rowview["WOID"]);
            _LANEQTY = "0"; // LANE수 디폴트 0 SET
            _PTNQTY = "1"; // 패턴 사용을 안하지만 다른곳에서 패턴을 사용할 것을 대비하여 1로 SET
            _CSTID = (DataTableConverter.Convert(rowview.DataView).Columns["CSTID"] == null) ? "" : Util.NVC(rowview["CSTID"]);
            _FastTrackLot = Util.NVC(rowview["LOTID"]); // 2023-09-12 [E20230829-000685] Fast Track 추가

            if (isCoaterAfterProcess)
                _CUTID = (procId.Equals(Process.INS_COATING) || procId.Equals(Process.INS_SLIT_COATING)) ? "1" : Util.NVC(rowview["CUT_ID"]);

            // 버전 공통으로 입력 하는 부분 추가 [2017-02-17]
            DataTable versionDt = GetProcessVersion(_LOTID, _PRODID);
            if (versionDt.Rows.Count > 0)
            {
                _VERSION = Util.NVC(versionDt.Rows[0]["PROD_VER_CODE"]);
                _LANEQTY = string.IsNullOrEmpty(Util.NVC(versionDt.Rows[0]["LANE_QTY"])) ? "0" : Util.NVC(versionDt.Rows[0]["LANE_QTY"]);
            }

            // MIXER공정에서 CONVRATE에 버전정보 1개만 존재 시 해당 버전 SETUP [2017-05-10]
            if ((string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.SRS_MIXING)) && string.IsNullOrEmpty(_VERSION))
                _VERSION = GetProdVerCode(_PRODID);

            if (_WIPSTAT != "WAIT")
            {
                txtVersion.Text = _VERSION;
                txtLaneQty.Value = string.IsNullOrEmpty(_LANEQTY) ? 0 : Convert.ToDouble(_LANEQTY);
                txtLanePatternQty.Value = string.IsNullOrEmpty(_PTNQTY) ? 0 : Convert.ToDouble(_PTNQTY);

                txtStartDateTime.Text = Convert.ToDateTime(_WIPDTTM_ST).ToString("yyyy-MM-dd HH:mm");

                if (string.IsNullOrEmpty(_WIPDTTM_ED))
                    txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                else
                    txtEndDateTime.Text = Convert.ToDateTime(_WIPDTTM_ED).ToString("yyyy-MM-dd HH:mm");

                if (txtWorkTime != null && (string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.SRS_MIXING)))
                    txtWorkTime.Value = (Convert.ToDateTime(txtEndDateTime.Text) - Convert.ToDateTime(txtStartDateTime.Text)).TotalMinutes;

                WIPSTATUS = _WIPSTAT;
                _WORKDATE = Util.NVC(rowview["WORKDATE"]);

                if (txtWorkDate != null)
                    SetCalDate();

                if (!procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !procId.Equals(Process.MIXING) && !procId.Equals(Process.SRS_MIXING))
                    LOTID = WIPSTATUS == "EQPT_END" ? _LOTIDPR : _LOTID;

                txtUnit.Text = rowview["UNIT"].ToString();

                // 합권취용 투입 LOT SET
                if (LARGELOT_GRID.Visibility == Visibility.Visible)
                    txtMergeInputLot.Text = _LOTID;
                else
                    txtMergeInputLot.Text = string.IsNullOrEmpty(_LOTIDPR) ? _LOTID : _LOTIDPR;

                // 청주 소형전극에서만 패턴에서만 변환해서 값 입력 [COATER에서만 사용]                
                if (string.Equals(txtUnit.Text, "EA") && (!string.Equals(procId, Process.PRE_MIXING) && !string.Equals(procId, Process.MIXING) && !string.Equals(procId, Process.SRS_MIXING)))
                    convRate = GetPatternLength(_PRODID);

                // 코터 & SRS코터 자동 FINAL CUT 처리 [2017-05-30]
                if ((string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING)) && chkFinalCut != null && string.Equals(rowview["FINAL_CUT_FLAG"], "Y"))
                    chkFinalCut.IsChecked = true;
                else if ((string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING)) && chkFinalCut != null && !string.Equals(rowview["FINAL_CUT_FLAG"], "Y"))
                    chkFinalCut.IsChecked = false;

                SetUnitFormatted(); // UNIT별로 FORMAT을 별도로 해달라는 요청이 있어서 해당 기능 적용 [2017-02-21]
                GetResultInfo();

                #region # 전수불량 Lane
                //현재 Lane 수량
                txtCurLaneQty.Value = getCurrLaneQty(_LOTID, procId);

                if (getDefectLane(LoginInfo.CFG_EQSG_ID, procId))
                    (grdResult.Children[0] as UcResult).btnSaveRegDefectLane.Visibility = Visibility.Visible;

                if (string.Equals(procId, Process.SLITTING) && (txtCurLaneQty.Value == txtLaneQty.Value))
                    (grdResult.Children[0] as UcResult).btnSaveRegDefectLane.IsEnabled = false;
                else
                    (grdResult.Children[0] as UcResult).btnSaveRegDefectLane.IsEnabled = true;
                #endregion
            }
        }

        private void SetUnitFormatted()
        {
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                string sFormatted = string.Empty;
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }

                // 단면코터 BACK작업 시 패턴일 경우 생산량 소수점 조절을 위해 F1 고정
                if (string.Equals(txtUnit.Text, "EA") && isSingleCoater == true && string.Equals(COATTYPE_COMBO.SelectedValue, "B"))
                    txtInputQty.Format = "F1";
                else
                    txtInputQty.Format = sFormatted;

                txtParentQty.Format = sFormatted;
                txtRemainQty.Format = sFormatted;

                if (string.Equals(procId, Process.SRS_COATING))
                {
                    txtSrs1Qty.Format = sFormatted;
                    txtSrs2Qty.Format = sFormatted;
                    txtSrs3Qty.Format = sFormatted;
                }

                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                {
                    if (string.Equals(txtUnit.Text, "EA"))
                        (grdDataCollect.Children[0] as UcDataCollect).txtMesualQty.Format = "F1";
                    else
                        (grdDataCollect.Children[0] as UcDataCollect).txtMesualQty.Format = sFormatted;
                }

                if (LOTINFO_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < LOTINFO_GRID.Columns.Count; i++)
                        if (LOTINFO_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(LOTINFO_GRID.Columns[i].Tag, "N"))
                            // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                            if (string.Equals(txtUnit.Text, "EA") && string.Equals(procId, Process.COATING))
                                ((DataGridNumericColumn)LOTINFO_GRID.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)LOTINFO_GRID.Columns[i]).Format = sFormatted;

                if (WIPREASON_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < WIPREASON_GRID.Columns.Count; i++)
                        if (WIPREASON_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(WIPREASON_GRID.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)WIPREASON_GRID.Columns[i]).Format = sFormatted;

                if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < WIPREASON2_GRID.Columns.Count; i++)
                        if (WIPREASON2_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(WIPREASON2_GRID.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)WIPREASON2_GRID.Columns[i]).Format = sFormatted;

                if (QUALITY_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < QUALITY_GRID.Columns.Count; i++)
                        if (QUALITY_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(QUALITY_GRID.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)QUALITY_GRID.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)QUALITY_GRID.Columns[i]).Format = sFormatted;

                if (QUALITY2_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < QUALITY2_GRID.Columns.Count; i++)
                        if (QUALITY2_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(QUALITY2_GRID.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)QUALITY2_GRID.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)QUALITY2_GRID.Columns[i]).Format = sFormatted;

                if (INPUTMTRL_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < INPUTMTRL_GRID.Columns.Count; i++)
                        if (INPUTMTRL_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(INPUTMTRL_GRID.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)INPUTMTRL_GRID.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)INPUTMTRL_GRID.Columns[i]).Format = sFormatted;

                if (INPUTMTRLSUMMARY_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < INPUTMTRLSUMMARY_GRID.Columns.Count; i++)
                        if (INPUTMTRLSUMMARY_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(INPUTMTRLSUMMARY_GRID.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)INPUTMTRLSUMMARY_GRID.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)INPUTMTRLSUMMARY_GRID.Columns[i]).Format = sFormatted;

                // NISSAN용 입력용도로 추가
                if (DEFECT_TAG_GRID.Visibility == Visibility.Visible)
                    for (int i = 0; i < DEFECT_TAG_GRID.Columns.Count; i++)
                        if (DEFECT_TAG_GRID.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(DEFECT_TAG_GRID.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)DEFECT_TAG_GRID.Columns[i]).Format = sFormatted;
            }
        }

        private string GetUnitFormatted()
        {
            string sFormatted = "0";
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        private string GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (txtUnit.Text)
            {
                case "KG":
                    sFormatted = "{0:#,##0.000}";
                    break;

                case "M":
                    sFormatted = "{0:#,##0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:#,##0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private string GetUnitFormatted(object obj, string pattern)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (pattern)
            {
                case "KG":
                    sFormatted = "{0:###0.000}";
                    break;

                case "M":
                    sFormatted = "{0:###0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:###0.0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private string GetIntFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = "{0:#,##0}";
            double dFormat = 0;

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        void GetWrkShftUser()
        {
            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
            Indata["PROCID"] = procId;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                        {
                            txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                        }
                        else
                        {
                            txtShiftStartTime.Text = string.Empty;
                        }

                        if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                        {
                            txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            txtShiftEndTime.Text = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                        {
                            txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                        }
                        else
                        {
                            txtShiftDateTime.Text = string.Empty;
                        }

                        if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                        else
                        {
                            txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                            txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                        }

                        if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                        {
                            txtShift.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                        }
                        else
                        {
                            txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                            txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                        }
                    }
                    else
                    {
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        void GetResultInfo()
        {
            try
            {
                Util.gridClear(LOTINFO_GRID);

                DataRowView rowView = PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem as DataRowView;

                if (isCoaterAfterProcess == false || isSingleCoater == true)
                {
                    DataTable _dtLotInfo = new DataTable();
                    _dtLotInfo.Columns.Add("LOTID", typeof(string));
                    _dtLotInfo.Columns.Add("WIPSEQ", typeof(Int32));
                    _dtLotInfo.Columns.Add("INPUTQTY", typeof(double));
                    _dtLotInfo.Columns.Add("GOODQTY", typeof(double));
                    _dtLotInfo.Columns.Add("GOODPTNQTY", typeof(double));
                    _dtLotInfo.Columns.Add("LOSSQTY", typeof(double));
                    _dtLotInfo.Columns.Add("DTL_DEFECT", typeof(double));
                    _dtLotInfo.Columns.Add("DTL_LOSS", typeof(double));
                    _dtLotInfo.Columns.Add("DTL_CHARGEPRD", typeof(double));

                    _dtLotInfo.Columns.Add("EQPT_END_QTY", typeof(double)); //장비 완료 수량 추가
                    _dtLotInfo.Columns.Add("LANE_QTY", typeof(Int32)).DefaultValue = 1;
                    _dtLotInfo.Columns.Add("LANE_PTN_QTY", typeof(Int32)).DefaultValue = 1;

                    DataRow dRow = _dtLotInfo.NewRow();
                    dRow["LOTID"] = Util.NVC(rowView["LOTID"]);
                    dRow["WIPSEQ"] = Util.NVC(rowView["WIPSEQ"]);
                    dRow["EQPT_END_QTY"] = Util.NVC(rowView["EQPT_END_QTY"]);   //장비 수량 추가

                    if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                    {
                        _dtLotInfo.Columns.Add("CSTID", typeof(string));
                        dRow["CSTID"] = Util.NVC(rowView["CSTID"]);
                    }
                    if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                    {
                        _dtLotInfo.Columns.Add("OUT_CSTID", typeof(string));
                        dRow["OUT_CSTID"] = Util.NVC(rowView["OUT_CSTID"]);
                    }

                    _dtLotInfo.Rows.Add(dRow);

                    Util.GridSetData(LOTINFO_GRID, _dtLotInfo, FrameOperation);

                    SetParentQty(_LOTID, WIPSTATUS);

                    // BEACMILL 횟수 SET
                    if (string.Equals(procId, Process.PRE_MIXING))
                        txtBeadMillCount.Value = Util.NVC_Int(rowView["MILL_COUNT"]);

                    // 특이사항 SET
                    DataTable dtCopy = _dtLotInfo.Copy();
                    BindingWipNote(dtCopy);
                }
                else
                {
                    string lotId = Util.NVC(DataTableConverter.GetValue(rowView, "LOTID"));

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LOTID_PR", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                    inTable.Columns.Add("WIPSTAT", typeof(string));

                    // Add : 2016.12.14, Slitting, SRS Slitting Cut ID 추가 ********************
                    if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
                        inTable.Columns.Add("CUT_ID", typeof(string));
                    //**************************************************************************

                    DataRow indata = inTable.NewRow();

                    if (WIPSTATUS == "EQPT_END")
                    {
                        indata["LOTID_PR"] = null;
                        indata["LOTID"] = lotId;
                    }
                    else
                    {
                        indata["LOTID_PR"] = lotId;
                        indata["LOTID"] = null;
                    }

                    // Add : 2016.12.14, Slitting, SRS Slitting Cut ID 추가 ******************** // CUTID PROC, EQTP_END 동일하게 입력하게 처리
                    if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
                        indata["CUT_ID"] = _CUTID;
                    //**************************************************************************

                    if (procId.Equals(Process.ROLL_PRESSING) || procId.Equals(Process.REWINDER) || procId.Equals(Process.TAPING) || PROCID.Equals(Process.BACK_WINDER) || PROCID.Equals(Process.HALF_SLITTING) || procId.Equals(Process.INS_COATING) || procId.Equals(Process.INS_SLIT_COATING) || procId.Equals(Process.HEAT_TREATMENT))
                    {
                        indata["LOTID"] = lotId;
                    }
                    else if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
                    {
                        indata["LOTID_PR"] = _LOTIDPR;
                        indata["LOTID"] = null;
                    }

                    // R/P 공정에서 추가 압연은 자기 차수에서는 체크 가능하게 하고, 그 외는 자동 체크로 진행 [2017-02-16]
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                        SetExtraPress();

                    // 수정 2016.12.21: 단면코터 Back *******************
                    if (isSingleCoater)
                    {
                        if (COATTYPE_COMBO.SelectedValue.Equals("B"))
                        {
                            indata["LOTID_PR"] = _LOTIDPR;
                            indata["LOTID"] = null;
                        }
                    }
                    //***************************************************

                    // SLITTING CUTID 기준 변경으로 LOTID 추가에 SLITTER 추가 ( 2017-01-21 ) CR-53
                    if ((string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING)) && !string.Equals(WIPSTATUS, Wip_State.WAIT))
                    {
                        LOTINFO_GRID.Columns["CUT_ID"].Visibility = Visibility.Visible;
                        LOTINFO_GRID.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LOTINFO_GRID.Columns["CUT_ID"].Visibility = Visibility.Collapsed;
                        LOTINFO_GRID.Columns["LOTID"].Visibility = Visibility.Visible;
                    }

                    indata["WIPSTAT"] = WIPSTATUS;
                    indata["LANGID"] = LoginInfo.LANGID;
                    indata["PROCID"] = procId;

                    inTable.Rows.Add(indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync(GetResultLotBizRuleName(), "INDATA", "RSLTDT", inTable);

                    Util.GridSetData(LOTINFO_GRID, dt, FrameOperation);

                    // Modify : 2016.12.13, Slitter | SRS Slitter 모투입수량 설정 추가 ******************** 
                    if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
                    {
                        // 첫 번째 CUT만 VISIBLE하고 그 외는 숨김 ( 2017-01-21 ) CR-53
                        for (int i = LOTINFO_GRID.TopRows.Count + 1; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                            LOTINFO_GRID.Rows[i].Visibility = Visibility.Collapsed;

                        SetParentQty(_LOTIDPR, WIPSTATUS); // OUTLOT 기준으로 변경 (CR54) 

                    }
                    else
                    {
                        SetParentQty(_LOTID, WIPSTATUS);
                    }
                    //*************************************************************************************
                    // Roll Press, Slitter 특이사항 Setting
                    DataTable dtCopy = dt.Copy();
                    BindingWipNote(dtCopy);

                    #region[POSTACTION]
                    BindPostHold(_LOTID, _CUTID);
                    #endregion
                }

                // 해당 설비 완공 시점에서는 설비완공 시점에서 투입량을 수량으로 변경한다 [2017-02-14]
                if (string.Equals(WIPSTATUS, Wip_State.EQPT_END) && txtInputQty.IsReadOnly == false)
                    txtInputQty.Value = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "EQPT_END_QTY"));

                // 절연코터, BACK WINDER는 자동 입력 후 수정 못하게 변경 (믹서는 투입자재 총수량 = 생산량)
                // 믹서 공정 다시 설비완공수량을 생산량으로 자동입력하게 변경, 또한 표면검사는 투입량 -> 생산량 자동입력 및 수정 가능하게 변경 요청
                // 백와인더, INS슬리터 코터는 모LOT 투입 기준 수정X, 나머지 공정들은 모LOT 투입 기준 수정 O                
                //if (txtInputQty.IsReadOnly == true || string.Equals(procId, Process.REWINDER) || string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.INS_COATING) ||
                //    string.Equals(procId, Process.HALF_SLITTING) || string.Equals(procId, Process.TAPING) || string.Equals(procId, Process.SLITTING))
                if (txtInputQty.IsReadOnly == true || string.Equals(procId, Process.REWINDER) || string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.INS_COATING) ||
                    string.Equals(procId, Process.HALF_SLITTING) || string.Equals(procId, Process.TAPING))
                {
                    if (!_isRollMapEquipment) txtInputQty.Value = txtParentQty.Value;
                }


                // 저장되어 있는 수량이 있으면 그 수량을 최선책으로 지정 [2017-04-21]
                SetSaveProductQty();

                // Rollmap 적용 설비의 경우 투입량을 Input
                if (_isRollMapEquipment == true && isCoaterAfterProcess == true)
                    txtInputQty.Value = txtParentQty.Value + Convert.ToDouble(exceedLengthQty);

                SetInputQty();
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // [E20230228-000007] CSR - Marking logic in GMES
        private string GetCutLotSampleQAFalg(string sLotID, string sProcID)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SAMPLE_TARGET_CT", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["SAMPLE_FLAG"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }
        #region  # 전수불량 Lane
        private bool getDefectLane(string sEQSGID, string sPROCID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "SCRIBE_DEFECT_LANE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(Util.NVC(row["CBO_CODE"]), sEQSGID + ":" + sPROCID))
                        return true;
                }
            }
            return false;
        }

        #endregion

        private void GetPet()
        {
            try
            {
                DataTable _dt = new DataTable();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["CMCDTYPE"] = "PROTECT_FILM_TYPE";
                inTable.Rows.Add(indata);

                _dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", inTable);

                if (_dt.Rows.Count > 0)
                {
                    cboPet.DisplayMemberPath = "CBO_NAME";
                    cboPet.SelectedValuePath = "CBO_CODE";
                    cboPet.ItemsSource = DataTableConverter.Convert(_dt);
                    cboPet.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetDefectList()
        {
            try
            {
                Util.gridClear(WIPREASON_GRID);
                Util.gridClear(WIPREASON2_GRID);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("RESNPOSITION", typeof(string));
                inDataTable.Columns.Add("CODE", typeof(string));

                //Modify 2016.12.19 물청 TOP/BACK 구분 없음(CHARGE2_GRID 삭제) *************************************************************
                List<C1DataGrid> lst = new List<C1DataGrid> { WIPREASON_GRID, WIPREASON2_GRID };
                //**************************************************************************************************************************
                foreach (C1DataGrid dg in lst)
                {
                    DataRow Indata = inDataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = procId;
                    Indata["LOTID"] = _LOTID; // LOTID -> _LOTID로 변경, 아니면 위에 MIXER일때 EQPT_END시점에서 PR_LOTID를 LOTID로 넣어주는것때문에 제대로 조회 불가 ( 2017-01-26 )

                    // DEFECT 통합으로 아래와  같이 변경
                    if (procId.Equals(Process.COATING) || procId.Equals(Process.INS_COATING) || procId.Equals(Process.INS_SLIT_COATING))
                        Indata["RESNPOSITION"] = string.Equals(dg.Name, "dgWipReason") ? "DEFECT_TOP" : "DEFECT_BACK";
                    else
                        Indata["RESNPOSITION"] = null;
                    //***********************************************************************************************

                    if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                        Indata["CODE"] = "BAS";

                    inDataTable.Rows.Clear();
                    inDataTable.Rows.Add(Indata);

                    //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                    DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                    if (dg.Visibility == Visibility.Visible)
                        Util.GridSetData(dg, dt, FrameOperation, true);

                    Util.SetGridColumnText(WIPREASON_GRID, "DFCT_QTY_CHG_BLOCK_FLAG", null, "DFCT_QTY_CHG_BLOCK_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Index + 1, dt.Rows[i]["DFCT_QTY_CHG_BLOCK_FLAG"]);
                    }
                    if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING) || string.Equals(procId, Process.ROLL_PRESSING))
                    {
                        dg.LoadedCellPresenter -= OnLoadedCellPresenter;
                        dg.LoadedCellPresenter += OnLoadedCellPresenter;
                    }
                }

                GetSumDefectQty();
                SetCauseTitle();
                SetExceedLength();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectListMultiLane()
        {
            try
            {
                string childLotId;

                // 소형/자동차 전극 사용을 체크박스로 분리 [2017-01-23] CR-55
                bool isElecProdType = (grdDataCollect.Children[0] as UcDataCollect).chkSum.IsChecked == true ? true : false;

                Util.gridClear(WIPREASON_GRID);

                // SET LANE COMBO 설정
                LANENUM_COMBO.Items.Clear();

                CheckBox allCheck = new CheckBox();
                allCheck.IsChecked = true;
                allCheck.Content = Util.NVC("-ALL-");
                allCheck.Tag = "ALL";
                allCheck.Checked += OnLaneChecked;
                allCheck.Unchecked += OnLaneChecked;
                LANENUM_COMBO.Items.Add(allCheck);

                for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.IsChecked = true;
                    checkBox.Content = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_NUM"));
                    checkBox.Tag = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID"));
                    checkBox.Checked += OnLaneChecked;
                    checkBox.Unchecked += OnLaneChecked;
                    LANENUM_COMBO.Items.Add(checkBox);
                }
                LANENUM_COMBO.Text = ObjectDic.Instance.GetObjectName("Lane선택");

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = procId;
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                IndataTable.Rows.Add(Indata);

                //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_DEFECT -> BR_PRD_SEL_DEFECT 변경
                new ClientProxy().ExecuteService("BR_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable totalWipReason = searchResult.DataSet.Tables["OUTDATA"];
                        DataTable dsWipReason = totalWipReason.DefaultView.ToTable(false, "RESNCODE", "ACTID", "ACTNAME", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "TAG_CONV_RATE", "AREA_RESN_CLSS_NAME1", "AREA_RESN_CLSS_NAME2", "AREA_RESN_CLSS_NAME3", "PERMIT_RATE"); // 2024-03-13 PERMIT_RATE 추가 

                        Util.GridSetData(WIPREASON_GRID, dsWipReason, FrameOperation);

                        if (WIPREASON_GRID.Rows.Count > 0)
                        {
                            if (LOTINFO_GRID.GetRowCount() > 0)
                            {
                                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                                int iCount = isResnCountUse == true ? 1 : 0;

                                // CONV_RATE기준으로 반영된 컬럼 삭제 [2017-02-15]
                                //for (int i = WIPREASON_GRID.Columns.Count - 1; i > WIPREASON_GRID.Columns["TAG_CONV_RATE"].Index; i--)
                                //    WIPREASON_GRID.Columns.RemoveAt(i);

                                // 2024-03-13 허용비율 처리 수정 
                                //string[] basecol = new string[] { "ACTID", "ACTNAME", "RESNCODE", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "AREA_RESN_CLSS_NAME1", "AREA_RESN_CLSS_NAME2", "AREA_RESN_CLSS_NAME3", "TAG_CONV_RATE" };
                                //for (int i = WIPREASON_GRID.Columns.Count - 1; i > WIPREASON_GRID.Columns["ACTID"].Index; i--)
                                //{
                                //    if (!basecol.Contains(WIPREASON_GRID.Columns[i].Name))
                                //    {
                                //        WIPREASON_GRID.Columns.RemoveAt(i);
                                //    }
                                //}
                                //  for (int i = WIPREASON_GRID.Columns.Count - 1; i > WIPREASON_GRID.Columns["TAG_CONV_RATE"].Index; i--)
                                string[] basecol = new string[] { "ACTID", "ACTNAME", "RESNCODE", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "AREA_RESN_CLSS_NAME1", "AREA_RESN_CLSS_NAME2", "AREA_RESN_CLSS_NAME3", "TAG_CONV_RATE", "PERMIT_RATE" };
                                for (int i = WIPREASON_GRID.Columns.Count - 1; i > WIPREASON_GRID.Columns["ACTID"].Index; i--)
                                {
                                    if (!basecol.Contains(WIPREASON_GRID.Columns[i].Name) /*&& !WIPREASON_GRID.Columns[i].Name.Equals("PERMIT_RATE") */)
                                    {
                                        WIPREASON_GRID.Columns.RemoveAt(i);
                                    }
                                }




                                //WIPREASON_GRID.Refresh();
                                string colcount = WIPREASON_GRID.Columns.Count.ToString();

                                if (LOTINFO_GRID.GetRowCount() > 0)
                                {

                                    string sMessageDic = ObjectDic.Instance.GetObjectName("태그수");
                                    string sMessageNum = ObjectDic.Instance.GetObjectName("횟수");
                                    double defectQty = 0;
                                    double lossQty = 0;
                                    double chargeQty = 0;

                                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                                    {
                                        childLotId = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID"));

                                        // 소형 ( SUM, 태그수 X ), 자동차 ( ALL, 태그수 O ) (2017-01-23) CR-54
                                        if (i == LOTINFO_GRID.TopRows.Count)
                                        {
                                            if (isElecProdType)
                                                Util.SetGridColumnNumeric(WIPREASON_GRID, "ALL", null, "SUM", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                            else
                                                Util.SetGridColumnNumeric(WIPREASON_GRID, "ALL", null, "ALL", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                        }

                                        if (isResnCountUse == true)
                                        {
                                            Util.SetGridColumnNumeric(WIPREASON_GRID, childLotId + "NUM", null, sMessageNum,
                                                true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);
                                        }

                                        Util.SetGridColumnNumeric(WIPREASON_GRID, childLotId + "CNT", null, sMessageDic,
                                            true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);

                                        // HALF SLITTING의 경우 8번째 자리 값으로 표시해달라는 요청으로 추가 [2017-05-08]
                                        if (string.Equals(procId, Process.HALF_SLITTING))
                                        {
                                            Util.SetGridColumnNumeric(WIPREASON_GRID, childLotId, null, childLotId.Length > 8 ? childLotId.Substring(7, 1) : Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_NUM")),
                                                true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                        }
                                        else
                                        {
                                            Util.SetGridColumnNumeric(WIPREASON_GRID, childLotId, null, Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_NUM")),
                                                true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                        }
                                        DataGridAggregate.SetAggregateFunctions(WIPREASON_GRID.Columns[childLotId], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});

                                        if (WIPREASON_GRID.Rows.Count == 0)
                                            continue;

                                        DataTable dt = GetDefectDataByLot(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID")));

                                        if (dt != null)
                                        {
                                            if (isResnCountUse == true)
                                            {
                                                for (int j = 0; j < dt.Rows.Count; j++)
                                                {
                                                    if (dt.Rows[j]["COUNTQTY"].ToString().Equals(""))
                                                        BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count - 2, null);
                                                    else
                                                        BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count - 2, dt.Rows[j]["COUNTQTY"]);
                                                }
                                            }

                                            for (int j = 0; j < dt.Rows.Count; j++)
                                            {
                                                if (dt.Rows[j]["DFCT_TAG_QTY"].ToString().Equals(""))
                                                    BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count - 1, 0);
                                                else
                                                    BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count - 1, dt.Rows[j]["DFCT_TAG_QTY"]);
                                            }

                                            for (int j = 0; j < dt.Rows.Count; j++)
                                            {
                                                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                                    BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count, 0);
                                                else
                                                    BindingDataGrid(WIPREASON_GRID, j, WIPREASON_GRID.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                            }
                                        }

                                        // SLITTER는 CUT단위로 변경되면서 CUT단위는 합산수량, 그 외는 개별로 처리 [2017-01-21] CR-53
                                        if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
                                        {
                                            DataTable distinctDt = DataTableConverter.Convert(WIPREASON_GRID.ItemsSource).DefaultView.ToTable(true, "ACTID");
                                            foreach (DataRow _row in distinctDt.Rows)
                                            {
                                                if (string.Equals(_row["ACTID"], "DEFECT_LOT"))
                                                    defectQty += SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                                else if (string.Equals(_row["ACTID"], "LOSS_LOT"))
                                                    lossQty += SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                                else if (string.Equals(_row["ACTID"], "CHARGE_PROD_LOT"))
                                                    chargeQty += SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                            }

                                            if (i == ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) - 1))
                                            {
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_DEFECT", defectQty);
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_LOSS", lossQty);
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_CHARGEPRD", chargeQty);
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY", (defectQty + lossQty + chargeQty));
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY", (Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) * LOTINFO_GRID.GetRowCount()) - Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY")));
                                                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY")) * Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LANE_QTY")));
                                            }
                                        }
                                        else
                                        {
                                            defectQty = 0;
                                            lossQty = 0;
                                            chargeQty = 0;

                                            DataTable distinctDt = DataTableConverter.Convert(WIPREASON_GRID.ItemsSource).DefaultView.ToTable(true, "ACTID");
                                            foreach (DataRow _row in distinctDt.Rows)
                                            {
                                                if (string.Equals(_row["ACTID"], "DEFECT_LOT"))
                                                    defectQty = SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                                else if (string.Equals(_row["ACTID"], "LOSS_LOT"))
                                                    lossQty = SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                                else if (string.Equals(_row["ACTID"], "CHARGE_PROD_LOT"))
                                                    chargeQty = SumDefectQty(WIPREASON_GRID, i, childLotId, Util.NVC(_row["ACTID"]));
                                            }

                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_LOSS", lossQty);
                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY", (defectQty + lossQty + chargeQty));
                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY")));
                                            DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY")));
                                        }
                                    }
                                }

                                // COST CENTER 컬럼 맨 뒤로 오게하는 요청으로 해당 컬럼 제일 하위에서 생성 [2017-02-07]
                                // 전 공정 횟수 관리를 위하여 컬럼 추가 (C20190416_75868 ) [2019-04-17]
                                Util.SetGridColumnText(WIPREASON_GRID, "COSTCENTERID", null, "COSTCENTERID", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(WIPREASON_GRID, "COSTCENTER", null, "COSTCENTER", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(WIPREASON_GRID, "TAG_ALL_APPLY_FLAG", null, "TAG_ALL_APPLY_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(WIPREASON_GRID, "DFCT_QTY_CHG_BLOCK_FLAG", null, "DFCT_QTY_CHG_BLOCK_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);

                                if (isResnCountUse == true)
                                {
                                    Util.SetGridColumnText(WIPREASON_GRID, "WRK_COUNT_MNGT_FLAG", null, "WRK_COUNT_MNGT_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                    Util.SetGridColumnText(WIPREASON_GRID, "LINK_DETL_RSN_CODE_TYPE", null, "LINK_DETL_RSN_CODE_TYPE", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                }

                                for (int i = 0; i < totalWipReason.Rows.Count; i++)
                                {
                                    BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["COSTCENTERID"].Index + 1, totalWipReason.Rows[i]["COSTCENTERID"]);
                                    BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["COSTCENTER"].Index + 1, totalWipReason.Rows[i]["COSTCENTER"]);
                                    BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["TAG_ALL_APPLY_FLAG"].Index + 1, totalWipReason.Rows[i]["TAG_ALL_APPLY_FLAG"]);
                                    BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Index + 1, totalWipReason.Rows[i]["DFCT_QTY_CHG_BLOCK_FLAG"]);

                                    if (isResnCountUse == true)
                                    {
                                        BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["WRK_COUNT_MNGT_FLAG"].Index + 1, totalWipReason.Rows[i]["WRK_COUNT_MNGT_FLAG"]);
                                        BindingDataGrid(WIPREASON_GRID, i, WIPREASON_GRID.Columns["LINK_DETL_RSN_CODE_TYPE"].Index + 1, totalWipReason.Rows[i]["LINK_DETL_RSN_CODE_TYPE"]);
                                    }
                                }

                                // ROW별 합산 수량 반영 [2017-02-15]
                                double rowSum = 0;
                                for (int i = 0; i < WIPREASON_GRID.Rows.Count; i++)
                                {
                                    rowSum = 0;
                                    for (int j = WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount); j < WIPREASON_GRID.Columns["COSTCENTERID"].Index; j += (2 + iCount))
                                        rowSum += Convert.ToDouble(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, WIPREASON_GRID.Columns[j].Name));

                                    DataTableConverter.SetValue(WIPREASON_GRID.Rows[i].DataItem, "RESNTOTQTY", rowSum);
                                }
                                Util.GridSetData(WIPREASON_GRID, DataTableConverter.Convert(WIPREASON_GRID.ItemsSource), FrameOperation, true);
                                SetExceedLength();
                                WIPREASON_GRID.Refresh(false);
                                WIPREASON_GRID.FrozenColumnCount = WIPREASON_GRID.Columns["ALL"].Index + 1;

                                // S/L공정의 경우 TAG 불량/LOSS 자동 반영을 위하여 하기와 같이 이전 값을 DataTable로 보관(C20190404_67447)  [2019-04-11]
                                dtWipReasonBak = DataTableConverter.Convert(WIPREASON_GRID.ItemsSource);
                            }

                            #region #전수불량 Lane 등록
                            _dtDEFECTLANENOT = getDefectLaneLotList(_CUTID);
                            _DEFECTLANELOT = _dtDEFECTLANENOT.Select("CHK = 1");
                            SetDisableWipReasonGrid(_DEFECTLANELOT);
                            #endregion
                        }
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetWorkHalfSlittingSide()
        {
            try
            {
                //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
                if (GetCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(procId, Process.ROLL_PRESSING))
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = EQUIPMENT_COMBO.SelectedValue;
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_WRK_HALF_SLIT_SIDE_RP_ESWA", "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Text = bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"].GetString();
                            (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag = bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"].GetString();
                        }
                    });
                }
                else
                {
                    const string bizRuleName = "DA_PRD_SEL_WRK_HALF_SLIT_SIDE";

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = EQUIPMENT_COMBO.SelectedValue;
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Text = bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"].GetString();
                            (grdSearch.Children[0] as UcSearch).txtWorkHalfSlittingSide.Tag = bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"].GetString();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private double SumDefectQty(string actId)
        {
            double sum = 0;

            if (WIPREASON_GRID.Rows.Count > 0)
                for (int i = 0; i < WIPREASON_GRID.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "RESNQTY")) *
                                    (WIPREASON2_GRID.Visibility == Visibility.Visible && (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING)) ? 0.5 : 1));

            if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Rows.Count > 0)
                for (int i = 0; i < WIPREASON2_GRID.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += Convert.ToDouble(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "RESNQTY"));

            return sum;
        }

        private double SumDefectQty(C1DataGrid dg, string lotId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)) *
                                ((string.Equals(dg.Name, "dgWipReason") && WIPREASON2_GRID.Visibility == Visibility.Visible && (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING))) ? 0.5 : 1));

            return sum;
        }

        private double SumDefectQty(C1DataGrid dg, int index, string lotId, string actId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)) *
                                    ((string.Equals(dg.Name, "dgWipReason") && WIPREASON2_GRID.Visibility == Visibility.Visible && (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.SRS_COATING))) ? 0.5 : 1));

            return sum;
        }

        private void BindingDataGrid(C1DataGrid dg, int iRow, int iCol, object sValue)
        {
            try
            {
                if (dg.ItemsSource == null)
                {
                    return;
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                    if (dt.Columns.Count < dg.Columns.Count)
                        for (int i = dt.Columns.Count; i < dg.Columns.Count; i++)
                            dt.Columns.Add(dg.Columns[i].Name);

                    if (sValue.Equals("") || sValue.Equals(null))
                        sValue = 0;

                    dt.Rows[iRow][iCol - 1] = sValue;

                    dg.BeginEdit();
                    Util.GridSetData(dg, dt, FrameOperation, false);
                    dg.EndEdit();
                }
            }
            catch { }
        }

        void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL2", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void SetMaterial(string LotID, string PROC_TYPE)
        {
            if (INPUTMTRL_GRID.Rows.Count < 1)
                return;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["LOTID"] = _LOTID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("IN_INPUT");
                InLotdataTable.Columns.Add("INPUT_LOTID", typeof(string));
                InLotdataTable.Columns.Add("MTRLID", typeof(string));
                InLotdataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                InLotdataTable.Columns.Add("PROC_TYPE", typeof(string));
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));

                DataTable dt = ((DataView)INPUTMTRL_GRID.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals(""))
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                            inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                            InLotdataTable.Rows.Add(inLotDataRow);
                        }
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                Thread.Sleep(500);

                GetMaterial(LotID);
                isChangeMaterial = false;
                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        string GetStartCancelBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E0500":
                case "E0400":
                case "E0410":
                case "E0420":
                case "E0430":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_PM";
                    break;

                case "E1000":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_MX";
                    break;

                case "E2000":
                case "S2000":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_CT";
                    break;

                case "E2500":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_HS";
                    break;

                case "E3000":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_RP";
                    break;


                case "E4000":
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_SL";
                    break;

                default:
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_CT";
                    break;
            }

            return bizRuleName;
        }

        private void StartCancelProcess()
        {
            try
            {
                if (_WIPSTAT.Equals("WAIT"))
                    return;

                if (_WIPSTAT.Equals("EQPT_END"))
                {
                    Util.MessageValidation("SFU1671");  //설비 완공 LOT은 취소할 수 없습니다.
                    return;
                }

                // CSR : C20220322-000662 - ESWA 전극 2동 롤프레스 GMES 공정 진척 Cancel Start 버튼 권한 부여 후 사용으로 변경 요청의 건
                // 1. 해당 동 적용 여부 체크
                // 2. 적용 동 버튼 적용 여부 체크
                string[] sAttrbute = { null, procId, "CANCEL_LOTSTART_W"  };

                if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_DRB", "", sAttrbute))
                    if (!CheckButtonPermissionGroupByBtnGroupID("CANCEL_LOTSTART_W")) return;

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        #region Lot Info
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow inDataRow = null;

                        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                        {
                            inDataRow = null;
                            inDataRow = inDataTable.NewRow();

                            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inDataRow["EQPTID"] = _EQPTID;
                            inDataRow["LOTID"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID").ToString();
                            if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                if (LOTINFO_GRID.Columns["OUT_CSTID"] != null)
                                {
                                    inDataRow["OUT_CSTID"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "OUT_CSTID").ToString();
                                }
                            }

                            inDataRow["USERID"] = LoginInfo.USERID;
                            inDataRow["INPUT_LOTID"] = _LOTIDPR;
                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                if (LOTINFO_GRID.Columns["CSTID"] != null)
                                {
                                    inDataRow["CSTID"] = DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "CSTID").ToString();
                                }
                            }
                            inDataTable.Rows.Add(inDataRow);
                        }
                        #endregion

                        // Add : 2016.12.10, Slitter, SRS Slitter 작업시작 취소 추가 *******************************
                        if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING))
                        {
                            #region Parent Lot
                            DataRow inLotDataRow = null;

                            DataTable InLotdataTable = inDataSet.Tables.Add("IN_PRLOT");
                            InLotdataTable.Columns.Add("PR_LOTID", typeof(string));
                            InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                            InLotdataTable.Columns.Add("CSTID", typeof(string));

                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["PR_LOTID"] = _LOTIDPR;
                            inLotDataRow["CUT_ID"] = _CUTID;

                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                inLotDataRow["CSTID"] = _CSTID;
                            }
                            InLotdataTable.Rows.Add(inLotDataRow);
                            #endregion
                        }

                        new ClientProxy().ExecuteService_Multi(GetStartCancelBizRuleName(), procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING) ? "INDATA,IN_PRLOT" : "INDATA", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void EqptEndCancelProcess()
        {
            try
            {
                if (_WIPSTAT.Equals("WAIT"))
                    return;

                if (_WIPSTAT.Equals("PROC"))
                {
                    Util.MessageValidation("SFU3464");  //진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                    return;
                }
                
                // 1. 해당 동 적용 여부 체크
                // 2. 적용 동 버튼 적용 여부 체크
                string[] sAttrbute = { null, procId, "CANCEL_EQPT_END_W" };

                if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_DRB", "", sAttrbute))
                    if (!CheckButtonPermissionGroupByBtnGroupID("CANCEL_EQPT_END_W")) return;

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["PROCID"] = procId;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                        InLotdataTable.Columns.Add("WIPNOTE", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();

                        if (procId.Equals(Process.SLITTING) || procId.Equals(Process.SRS_SLITTING) || procId.Equals(Process.HALF_SLITTING))
                            inLotDataRow["CUT_ID"] = _CUTID;
                        else
                            inLotDataRow["LOTID"] = _LOTID;

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        string GetParentQtyBizRuleName()
        {
            string bizRuleName;

            if (isSingleCoater)
            {
                if (COATTYPE_COMBO.SelectedValue.Equals("B"))
                    bizRuleName = "DA_PRD_SEL_QTY";
                else
                    bizRuleName = "DA_PRD_SEL_PRLOT_QTY";
            }
            else
            {
                switch (procId)
                {
                    case "E2300":
                    case "E2500":
                    case "E3000":
                    case "E3500":
                    case "E3800":
                    case "E3900":
                        bizRuleName = _isRollMapEquipment ? "DA_PRD_SEL_QTY_RM" : "DA_PRD_SEL_QTY";
                        break;

                    default:
                        bizRuleName = "DA_PRD_SEL_PRLOT_QTY";
                        break;
                }
            }
            return bizRuleName;
        }

        private void SetParentQty(string lotid, string status)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = lotid;
                indata["WIPSTAT"] = status;

                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(GetParentQtyBizRuleName(), "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    if (status.Equals(Wip_State.EQPT_END))
                        txtInputQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_OUT"]);

                    txtParentQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_IN"].ToString());
                    SetParentRemainQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        void SetInputQty()
        {
            if (LOTINFO_GRID.GetRowCount() < 1)
                return;

            decimal inputQty = Util.NVC_Decimal(txtInputQty.Value);
            decimal lossQty = 0;
            int laneqty = 0;

            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING))
            {
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                laneqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LANE_QTY"));

                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY", inputQty);
                DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY", (inputQty * LOTINFO_GRID.GetRowCount()) - lossQty);
            }
            if (string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.CMC) || string.Equals(procId, Process.BS))
            {
                for (int i = 0 + LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                {
                    lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY"));
                    laneqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY"));

                    if (!_isProdQtyInput)
                    {
                        //2020-05-15 CWA 전극 1동은 투입 수량을 설비 완공 수량으로 안한다고 함.(기준정보 등록으로 변경)
                        if (GetCommonCodeUse("MIXER_EQPTQTY_AREA", LoginInfo.CFG_AREA_ID))
                        {
                            //--------------------------------------------------------------------------------------------------------
                            //2020-04-15 CSRID : 아직 없음. CWA는 투입 수량을 설비 완공 수량으로 한다고 함.
                            inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "EQPT_END_QTY"));
                            //--------------------------------------------------------------------------------------------------------
                        }
                    }

                    if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", inputQty);
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", inputQty + lossQty);
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", inputQty);

                    }

                    if (txtLaneQty != null && !string.Equals(procId, Process.HALF_SLITTING))
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * Util.NVC_Decimal(txtLaneQty.Value));
                    else
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * laneqty);
                }
            }
            else
            {
                for (int i = 0 + LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                {
                    lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY"));
                    laneqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY"));

                    if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", inputQty);
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", inputQty + lossQty);
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", inputQty);

                    }

                    if (txtLaneQty != null && !string.Equals(procId, Process.HALF_SLITTING))
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * Util.NVC_Decimal(txtLaneQty.Value));
                    else
                        DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) * laneqty);
                }
            }

            if (isCoaterAfterProcess)
                SetParentRemainQty();
            else
                txtInputQty.Value = 0;
        }

        void SetGoodQty()
        {
            if (LOTINFO_GRID.GetRowCount() < 1)
                return;

            double goodQty = txtGoodQty.Value;
            double lossQty = 0;
            int laneqty = 1;
            int laneptnqty = 1;

            if ((grdResult.Children[0] as UcResult).grdSummary.Visibility == Visibility.Visible)
            {
                if (txtParentQty != null)
                {
                    if (goodQty > double.Parse(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString()))
                    {
                        Util.MessageValidation("SFU1612");  //생산량이 모LOT 투입량 보다 클 수 없습니다.
                        return;
                    }
                }
            }

            for (int i = 0 + LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
            {
                lossQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY"));
                laneqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY"));
                laneptnqty = Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_PTN_QTY"));

                DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", goodQty);
                DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", goodQty + lossQty);

                DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", laneqty * laneptnqty * goodQty);

                if (txtLaneQty != null)
                    DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODPTNQTY", txtLaneQty.Value * txtLanePatternQty.Value * goodQty);
            }

            if ((grdResult.Children[0] as UcResult).grdSummary.Visibility == Visibility.Visible)
                SetParentRemainQty();
        }

        private void SetParentRemainQty()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

            // 믹서공정 기준으로는 투입량이 따로 없어 0으로 표시
            if (isCoaterAfterProcess == false)
                txtRemainQty.Value = 0;
            else
                txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty - exceedLengthQty));

            txtInputQty.Value = 0;
        }

        private void SetLotAutoSelected()
        {
            if (LARGELOT_GRID.Visibility == Visibility.Visible)
            {
                if (LARGELOT_GRID != null && LARGELOT_GRID.Rows.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridCell currCell = LARGELOT_GRID.GetCell(0, LARGELOT_GRID.Columns["CHK"].Index);

                    if (currCell != null && currCell.Presenter != null && currCell.Presenter.Content != null)
                    {
                        LARGELOT_GRID.SelectedIndex = currCell.Row.Index;
                        LARGELOT_GRID.CurrentCell = currCell;
                    }
                }
            }

            // 롤플레스 대기, 설비완공 자동으로 선택해주게 변경 2018-01-06
            if (PRODLOT_GRID.Visibility == Visibility.Visible)
            {
                if (PRODLOT_GRID != null && PRODLOT_GRID.Rows.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridCell currCell = PRODLOT_GRID.GetCell(0, PRODLOT_GRID.Columns["CHK"].Index);

                    if (currCell != null && currCell.Presenter != null && currCell.Presenter.Content != null)
                    {
                        PRODLOT_GRID.SelectedIndex = currCell.Row.Index;
                        PRODLOT_GRID.CurrentCell = currCell;

                    }
                }
            }

            if (PRODLOT_GRID != null && PRODLOT_GRID.Rows.Count > 0)
            {
                for (int i = 0; i < PRODLOT_GRID.Rows.Count; i++)
                {
                    if (string.Equals(DataTableConverter.GetValue(PRODLOT_GRID.Rows[i].DataItem, "WIPSTAT"), Wip_State.PROC))
                    {
                        C1.WPF.DataGrid.DataGridCell currCell = PRODLOT_GRID.GetCell(i, PRODLOT_GRID.Columns["CHK"].Index);

                        if (currCell != null && currCell.Presenter != null && currCell.Presenter.Content != null)
                        {
                            PRODLOT_GRID.SelectedIndex = currCell.Row.Index;
                            PRODLOT_GRID.CurrentCell = currCell;
                        }
                        break;
                    }
                }
            }
        }

        private void ConfirmCheck()
        {
            // LOSS는 자동 등록 한번 해줌
            SaveDefect(WIPREASON_GRID);

            if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                SaveDefect(WIPREASON2_GRID);

            // RP공정에서 범위 알림 기능 추가 [2017-06-06]
            if (string.Equals(procId, Process.ROLL_PRESSING))
            {
                // 2023.10.10 조성근- 롤맵 홀드는 수동 선택이 아닌, 자동 선택 ( E20231005-000782 )
                //DataTable dtHold = GetRollMapHold(procId);

                //if (IsRollMapEquipment() && CommonVerify.HasTableRow(GetRollMapHold(procId)))
                //{
                //    //롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////
                //    holdLotClassCode.Clear();
                //    if (dtHold.Columns.Contains("ADJ_LOTID") && dtHold.Columns.Contains("HOLD_CLASS_CODE"))
                //    {
                //        holdLotClassCode.Add(dtHold.Rows[0]["ADJ_LOTID"].ToString(), "E3000");
                //    }

                //    CMM_ROLLMAP_HOLD popRollMapHold = new CMM_ROLLMAP_HOLD();
                //    popRollMapHold.FrameOperation = FrameOperation;

                //    object[] parameters = new object[10];
                //    parameters[0] = procId;
                //    parameters[1] = _EQPTID;
                //    parameters[2] = _LOTID;
                //    parameters[3] = _WIPSEQ;
                //    parameters[4] = Util.NVC(EQUIPMENT_COMBO.Text);
                //    parameters[5] = txtVersion.Text.Trim();
                //    C1WindowExtension.SetParameters(popRollMapHold, parameters);

                //    popRollMapHold.Closed += new EventHandler(popRollMapHold_Closed);
                //    this.Dispatcher.BeginInvoke(new Action(() => popRollMapHold.ShowModal()));
                //}
                //else
                {
                    DataTable dataCollect = DataTableConverter.Convert(QUALITY_GRID.ItemsSource);
                    int iHGCount1 = 0;  // H/G
                    int iHGCount2 = 0;  // M/S
                    int iHGCount3 = 0;  // 1차 H/G
                    int iHGCount4 = 0;  // 1차 M/S
                    decimal sumValue1 = 0;
                    decimal sumValue2 = 0;
                    decimal sumValue3 = 0;
                    decimal sumValue4 = 0;
                    bool isAuthConfirm = false;

                    foreach (DataRow row in dataCollect.Rows)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount3++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount4++;
                                }
                            }
                        }
                    }

                    ConfirmProcess();
                }
            }
            else if (string.Equals(procId, Process.MIXING))
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_MIXER_BATCH mixerConfirm = new CMM_ELEC_MIXER_BATCH();
                mixerConfirm.FrameOperation = FrameOperation;
                if (mixerConfirm != null)
                {
                    int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");
                    if (iRow < 0)
                        iRow = 0;

                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "PRJT_NAME"));
                    Parameters[1] = Util.NVC(txtVersion.Text);
                    // 2019-08-28 오화백  LOTID, EQPTID 정보 파라미터 추가
                    Parameters[2] = _LOTID;
                    Parameters[3] = _EQPTID;

                    C1WindowExtension.SetParameters(mixerConfirm, Parameters);

                    mixerConfirm.Closed += new EventHandler(OnCloseMixerConfirm);
                    this.Dispatcher.BeginInvoke(new Action(() => mixerConfirm.ShowModal()));
                }
            }
            else if (procId.Equals(Process.COATING))
            {
                // 2023.10.10 조성근- 롤맵 홀드는 수동 선택이 아닌, 자동 선택 ( E20231005-000782 )
                //DataTable dtHold = GetRollMapHold(procId);

                //if (IsRollMapEquipment() && CommonVerify.HasTableRow(dtHold))
                //{

                //    //코터 롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////
                //    holdLotClassCode.Clear();
                //    if (dtHold.Columns.Contains("ADJ_LOTID") && dtHold.Columns.Contains("HOLD_CLASS_CODE"))
                //    {
                //        holdLotClassCode.Add(dtHold.Rows[0]["ADJ_LOTID"].ToString(), dtHold.Rows[0]["HOLD_CLASS_CODE"].ToString());
                //    }


                //    CMM_ROLLMAP_HOLD popRollMapHold = new CMM_ROLLMAP_HOLD();
                //    popRollMapHold.FrameOperation = FrameOperation;

                //    object[] parameters = new object[10];
                //    parameters[0] = procId;
                //    parameters[1] = _EQPTID;
                //    parameters[2] = _LOTID;
                //    parameters[3] = _WIPSEQ;
                //    parameters[4] = Util.NVC(EQUIPMENT_COMBO.Text);
                //    parameters[5] = txtVersion.Text.Trim();
                //    C1WindowExtension.SetParameters(popRollMapHold, parameters);

                //    popRollMapHold.Closed += new EventHandler(popRollMapHold_Closed);
                //    Dispatcher.BeginInvoke(new Action(() => popRollMapHold.ShowModal()));
                //}
                //else
                {
                    CMM_ELEC_HOLD_YN wndHChk = new CMM_ELEC_HOLD_YN();
                    wndHChk.FrameOperation = FrameOperation;

                    if (wndHChk != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = _LOTID;

                        C1WindowExtension.SetParameters(wndHChk, Parameters);

                        wndHChk.Closed += new EventHandler(wndHoldChk_Closed);
                        Dispatcher.BeginInvoke(new Action(() => wndHChk.ShowModal()));
                    }
                }

            }
            else if (PROCID.Equals(Process.SLITTING) && IsAbnormalLot())
            {

                #region #전수불량 Lane 등록
                CMM_ELEC_DFCT_ACTION_CHK wndDefectLaneChk = new CMM_ELEC_DFCT_ACTION_CHK();
                wndDefectLaneChk.FrameOperation = FrameOperation;

                if (wndDefectLaneChk != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "CUT_ID"));

                    C1WindowExtension.SetParameters(wndDefectLaneChk, Parameters);

                    wndDefectLaneChk.Closed += new EventHandler(wndDefectLaneChk_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndDefectLaneChk.ShowModal()));

                }
                #endregion
            }
            else
            {
                ConfirmProcess();
            }

        }

        private void ValidateCarrierCTUnloader()
        {
            try
            {
                if (procId.Equals(Process.COATING))
                {
                    if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                    {

                        //20200313 전량 LOSS 처리 하는 경우 RFID 연계 여부 체크 하지 않음
                        if (Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY")) == 0)
                        {
                            ConfirmProcess();
                            return;
                        }

                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("LOTID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOTID"));
                        IndataTable.Rows.Add(Indata);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                        if (result.Rows.Count > 0)
                        {
                            if (string.IsNullOrEmpty(result.Rows[0]["CSTID"].ToString()))
                            {
                                CMM_ELEC_CST_RELATION cst = new CMM_ELEC_CST_RELATION();
                                cst.FrameOperation = FrameOperation;

                                if (cst != null)
                                {
                                    object[] Parameters = new object[2];
                                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOTID"));
                                    Parameters[1] = _EQPTID;

                                    C1WindowExtension.SetParameters(cst, Parameters);

                                    cst.Closed += new EventHandler(wndCst_Closed);
                                    this.Dispatcher.BeginInvoke(new Action(() => cst.ShowModal()));
                                }
                            }
                            else
                            {
                                ConfirmProcess();
                            }
                        }

                    }
                    else
                    {
                        ConfirmProcess();
                    }
                }
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                return;
            }

            return;

        }

        bool ValidateConfirm()
        {
            if (!ValidateInputQtyLimit())
                return false;

            if (!ValidateProductQty())
                return false;

            if (!ValidWorkTime())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (!ValidOverProdQty())
                return false;

            if (!ValidConfirmQty())
                return false;

            if (!procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing))
            {
                if (!ValidVersion())
                    return false;
            }

            if (!procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !procId.Equals(Process.MIXING) && !procId.Equals(Process.SRS_MIXING))
                if (!ValidLaneQty())
                    return false;

            if (string.Equals(procId, Process.SRS_COATING))
                if (!ValidInOutQty())
                    return false;

            if (Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY")) != 0) // 양품량이 0이 아닌 경우만 자주검사 필수 입력 항목 체크함 2020.02.22
                if (!ValidQualityRequired())
                    return false;

            if (!ValidQualitySpecRequired())
                return false;

            if (!ValidDataCollect())
                return false;

            if (procId.Equals(Process.COATING)) //Slurry Input Lot 확인 인터락
            {
                string[] sAttrbute = { procId };
                if (IsAreaCommoncodeAttrUse("EQPT_CURR_MOUNT_INPUT_LOT", "USE_YN", sAttrbute))
                {
                    if (!IsChickSlurryInEQPT())
                    {
                        return false;
                    }
                }
            }

            if (procId.Equals(Process.ROLL_PRESSING)) //대LOT의 첫번째 롤프레스 일 경우
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = _LOTID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPATTR_QAFLAG", "INDATA", "RSLTDT", dt);

                string wipseq = Convert.ToString(result.Rows[0]["WIPSEQ"]);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1382");  //LOT이 WIPATTR테이블에 없습니다. 
                    return false;
                }

                if (Convert.ToString(result.Rows[0]["QA_INSP_TRGT_FLAG"]).Equals("Y"))
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("AREAID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                    IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = procId;
                    Indata["LOTID"] = _LOTID;
                    Indata["WIPSEQ"] = wipseq;

                    Indata["CLCT_PONT_CODE"] = null;
                    IndataTable.Rows.Add(Indata);

                    result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count == 0)
                        result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count > 0)
                    {
                        DataRow[] inspRows;

                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0)
                        {
                            inspRows = result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'E3000-0001'") : result.Select("INSP_ITEM_ID = 'SI516'");
                        }
                        else
                        {
                            inspRows = result.Select("INSP_ITEM_ID = 'SI022'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'SI022'") : result.Select("INSP_ITEM_ID = 'SI516'");
                        }

                        foreach (DataRow inspRow in inspRows)
                        {
                            if (inspRow["CLCTVAL01"].Equals("") && !Util.NVC(inspRow["CLSS_NAME1"]).Contains("HG"))
                            {
                                Util.MessageValidation("SFU1603");  //샘플링 진행/마우저 두께를 모두 기입 바랍니다.
                                return false;
                            }
                        }
                    }
                }

                if (!ValidateSpec())
                    return false;
            }

            return true;
        }

        private bool IsChickSlurryInEQPT()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable OutdataTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_SLURRY", "INDATA", "OUTDATA", IndataTable);

                for (int i = 0; i < OutdataTable.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(OutdataTable.Rows[i]["INPUT_LOTID"].ToString()))
                    {
                        Util.MessageValidation("SFU2988", Util.NVC(EQUIPMENT_COMBO.SelectedValue));
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }

        bool ValidateSpec()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["LOTID"] = _LOTID;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INSP_ITEM_SPEC", "INDATA", "RSLTDT", dt);
            if (result.Rows.Count != 0)
            {
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    if (Convert.ToString(result.Rows[i]["CLCTVAL01"]).Equals("") || Util.NVC_Decimal(result.Rows[i]["CLCTVAL01"]) == 0)
                    {
                        Util.MessageValidation("SFU2886", new object[] { Util.NVC(result.Rows[i]["INSP_CLCTNAME"]) });  //{%1} 품질 값을 넣어주세요
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidateConfirmSlitter()
        {
            if (LOTINFO_GRID.GetRowCount() < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1702"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);    //실적 Lot을 선택 해 주세요.
                return false;
            }

            if (!ValidateProductQty())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (!ValidVersion())
                return false;

            if (!ValidLaneQty())
                return false;

            if (!ValidCutOverProdQty())
                return false;

            if (!ValidConfirmQty())
                return false;

            if (Util.NVC_Int(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY")) != 0) // 양품량이 0이 아닌 경우만 자주검사 필수 입력 항목 체크함 2020.02.22
                if (!ValidQualityRequired())
                    return false;

            if (!ValidQualitySpecRequired())
                return false;

            if (!ValidDataCollect())
                return false;

            #region [POSTACTION]
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EA") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
            {
                if (!ConfirmdPostAction())
                    return false;
            }
            #endregion

            return true;
        }

        bool ValidateProductQty()
        {
            DataTable dt = DataTableConverter.Convert(LOTINFO_GRID.ItemsSource);

            if (string.Equals(procId, Process.SLITTING))
            {
                if (Util.NVC_Decimal(dt.Rows[0]["INPUTQTY"]) <= 0)
                {
                    Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Util.NVC_Decimal(dt.Rows[i]["INPUTQTY"]) <= 0)
                    {
                        Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                        return false;
                    }
                }
            }
            return true;
        }

        private string IsAutoChangeWorkOrder()
        {
            string rtnMsg = string.Empty;
            try
            {
                C1DataGrid workOrder = null;
                string woID = string.Empty;

                if (string.Equals(procId, Process.MIXING) || string.Equals(procId, Process.PRE_MIXING) || string.Equals(procId, Process.SRS_MIXING))
                    workOrder = (grdWorkOrder.Children[0] as UC_WORKORDER_MX_CWA).dgWorkOrder;
                else
                    workOrder = (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).dgWorkOrder;

                for (int i = 0; i < workOrder.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(workOrder.Rows[i].DataItem, "CHK")))
                    {
                        woID = Util.NVC(DataTableConverter.GetValue(workOrder.Rows[i].DataItem, "WO_DETL_ID"));
                        break;
                    }
                }

                if (string.IsNullOrEmpty(woID))
                    return rtnMsg;

                DataSet indataSet = new DataSet();

                DataTable inEqpData = indataSet.Tables.Add("IN_EQP");
                inEqpData.Columns.Add("SRCTYPE", typeof(string));
                inEqpData.Columns.Add("IFMODE", typeof(string));
                inEqpData.Columns.Add("EQPTID", typeof(string));
                inEqpData.Columns.Add("USERID", typeof(string));
                inEqpData.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow inEqpDataRow = inEqpData.NewRow();
                inEqpDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inEqpDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inEqpDataRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                inEqpDataRow["USERID"] = LoginInfo.USERID;
                inEqpDataRow["WO_DETL_ID"] = woID;
                inEqpData.Rows.Add(inEqpDataRow);

                GetWorkOrderNotClear(); // 작업지시 재 조회
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(Util.NVC(ex.Data["DATA"])) && !string.IsNullOrEmpty(Util.NVC(ex.Data["PARA"])))
                    rtnMsg = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]), Util.NVC(ex.Data["PARA"]).Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries));
                else
                    rtnMsg = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));
            }
            return rtnMsg;
        }

        private void SetUnMountCore(int position)
        {
            try
            {
                string sEqpt_Pstn_ID = string.Empty;
                DataTable dt = GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue));

                if (dt.Rows.Count > 0)
                {
                    int iIdx = 0;
                    foreach (DataRow _iRow in dt.Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'"))
                    {
                        if (iIdx != position)
                        {
                            sEqpt_Pstn_ID = Convert.ToString(_iRow["EQPT_MOUNT_PSTN_ID"]);
                            break;
                        }
                        iIdx++;
                    }
                }

                if (string.IsNullOrEmpty(sEqpt_Pstn_ID))
                    return;

                DataSet indataSet = new DataSet();

                DataTable IndataTable = indataSet.Tables.Add("IN_EQP");
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = IndataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                inDataRow["EQPT_MOUNT_PSTN_ID"] = sEqpt_Pstn_ID;
                inDataRow["PROCID"] = procId;
                inDataRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(inDataRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNMOUNT_SLURRY_CT", "INDATA", null, indataSet);
            }
            catch (Exception ex) { };
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = prodID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                        if (string.Equals(row["PROD_VER_CODE"], txtVersion.Text) && !string.IsNullOrEmpty(Util.NVC(row["PTN_LEN"])))
                            return Util.NVC_Decimal(row["PTN_LEN"]);

                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private string GetLotProdVerCode(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PROD_VER_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }
        #region #전수불량 Lane 등록
        private Int32 getCurrLaneQty(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { }

            return 0;
        }
        #endregion

        private DataTable GetMergeInfo(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_HIST", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private string GetProdVerCode(string sProdID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = sProdID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(result.Rows[0]["PROD_VER_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private bool ValidateInputQtyLimit()
        {
            //코터 이후 공정도 Validation 체크해줌.
            //if (isCoaterAfterProcess == true)
            //    return true;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = EQUIPMENT_COMBO.SelectedValue;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER_INPUT_VALID", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {

                    decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["LOT_PROD_MAX_QTY"])) && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["LOT_PROD_MIN_QTY"])))
                    {
                        if (inputQty == 0)
                        {
                            return true;
                        }
                        else if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MAX_QTY"]) < inputQty || Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MIN_QTY"]) > inputQty)
                        {
                            Util.MessageValidation("SFU3359");    //입력 가능한 생산량 범위를 벗어났습니다.
                            return false;
                        }
                    }

                    else if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["LOT_PROD_MAX_QTY"])))
                    {
                        if (inputQty == 0)
                        {
                            return true;
                        }
                        if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MAX_QTY"]) < inputQty)
                        {
                            Util.MessageValidation("SFU3359");
                            return false;
                        }
                    }
                    else if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["LOT_PROD_MIN_QTY"])))
                    {
                        if (inputQty == 0)
                        {
                            return true;
                        }
                        if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MIN_QTY"]) > inputQty)
                        {
                            Util.MessageValidation("SFU3359");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private DataTable GetMixerPreData()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Indata["PROCID"] = procId;
                Indata["EQPTID"] = _EQPTID;
                Indata["PRODID"] = _PRODID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MIXER_BEFORE_QTY", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetBackCoaterInputMaterialValid(string sLotID, string sCoatingSideType)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("OUT_FLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;

                if (string.Equals(sCoatingSideType, "B"))
                    Indata["OUT_FLAG"] = "Y";

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SINGLE_CT_INPUT_VALID", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetProcWipReasonData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CUTID", typeof(string));
                dt.Columns.Add("WIPSEQ", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["CUTID"] = _CUTID;

                int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

                if (iRow < 0)
                    dataRow["WIPSEQ"] = 1;
                else
                    dataRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "WIPSEQ"));

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_WIPREASON", "INDATA", "RSLTDT", dt);

                if (procResnDt == null)
                    procResnDt = new DataTable();

                procResnDt.Clear();
                procResnDt = result.Copy();

                if (result != null && result.Rows.Count > 0)
                {
                    // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                    int iCount = isResnCountUse == true ? 1 : 0;

                    for (int i = WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount); i < WIPREASON_GRID.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                    {
                        DataRow[] rows = result.Select("LOTID = '" + WIPREASON_GRID.Columns[i].Name + "'");
                        for (int j = 0; j < WIPREASON_GRID.Rows.Count; j++)
                        {
                            if (rows.Length == 0)
                                break;

                            foreach (DataRow row in rows)
                            {
                                if (string.Equals(WIPREASON_GRID.Columns[i].Name, row["LOTID"]) && string.Equals(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "RESNCODE"), row["RESNCODE"]))
                                {
                                    DataTableConverter.SetValue(WIPREASON_GRID.Rows[j].DataItem, WIPREASON_GRID.Columns[i].Name, row["RESNQTY"]);
                                    GetSumCutDefectQty(WIPREASON_GRID, j, i);
                                    continue;
                                }
                            }
                        }
                    }
                    WIPREASON_GRID.Refresh(false);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetCalDate()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = EQUIPMENT_COMBO.SelectedValue;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", dt);

                if (result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["CALDATE"])))
                {
                    txtWorkDate.Text = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtWorkDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool ValidDataCollect()
        {
            if (isChangeQuality)
            {
                Util.MessageValidation("SFU1999");  //품질 정보를 저장하세요.
                return false;
            }

            if (isChangeMaterial)
            {
                Util.MessageValidation("SFU1818");  //자재 정보를 저장하세요.
                return false;
            }

            if (isChagneDefectTag)
            {
                Util.MessageValidation("SFU3409");  //불량태그 정보를 저장하세요.
                return false;
            }

            if (isChangeColorTag)
            {
                Util.MessageValidation("SFU3410");  //색지 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }

            if (isChangeCotton)
            {
                Util.MessageValidation("SFU4913");  //면상태일지 정보를 저장하세요.
                return false;
            }

            #region [POSTACTION]
            if (isChangePostAction)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }

            //if (!ValidPostAction())
            //{
            //    Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
            //    return false;
            //}
            #endregion
            return true;
        }

        #region [POSTACTION]
        private bool ValidPostAction()
        {
            DataTable dt = DataTableConverter.Convert(POSTHOLD_GRID.ItemsSource);
            DataRow[] dr = dt.Select("LANENUM <> 'ALL' AND POST_HOLD = 'True'");

            foreach (DataRow dRow in dr)
            {
                if (string.IsNullOrEmpty(Util.NVC(dRow["REMARK"]).Split('|')[0]))
                {
                    Util.MessageValidation("SFU1993");  //특이사항을 입력하세요
                    return false;
                }
            }
            return true;
        }

        private bool ConfirmdPostAction()
        {
            DataTable postHold = DataTableConverter.Convert(POSTHOLD_GRID.ItemsSource);
            if (!ValidPostAction())
                return false;

            if (string.Equals(procId, Process.SLITTING))
            {
                DataTable savedposthold = getSavedPostAction(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "CUT_ID")));
                DataTable preposthold = getPostAction(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "CUT_ID")));

                DataRow[] dr = preposthold.Select("POST_HOLD = 'True'");

                if (dr.Length > 0 && savedposthold.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2977"); // 특이사항 정보를 저장하세요.
                    return false;
                }

                foreach (DataRow dRow in dr)
                {
                    foreach (DataRow dRow1 in savedposthold.Rows)
                    {
                        if (string.Equals(Util.NVC(dRow["LOTID"]), Util.NVC(dRow1["LOTID"])) && string.IsNullOrEmpty(Util.NVC(dRow1["NOTE"]).Split('|')[0]))
                        {
                            Util.MessageValidation("SFU1993"); // 특이사항을 입력하세요.
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        private bool ValidQualityRequired()
        {
            //if (QUALITY_GRID.Visibility == Visibility.Visible && QUALITY2_GRID.Visibility != Visibility.Visible && QUALITY_GRID.Rows.Count > 0)
            if (QUALITY_GRID.Visibility == Visibility.Visible && QUALITY_GRID.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(QUALITY_GRID.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    //bool isValid = false;

                    DataRow[] filterRows = DataTableConverter.Convert(QUALITY_GRID.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            //필수 항목 만큼 LOOP 처리를 못함 주석처리함(2021.03.11)
                            //isValid = true;
                            //break;

                            //2021.03.11 추가
                            //필수 항목 만큼 LOOP 처리하기 위함
                            Util.MessageValidation("SFU3601", sItemName);   ///해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                            return false;
                        }

                    }

                    //2021.03.11 주석처리
                    //if (isValid == false)
                    //{
                    //    Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                    //    return false;
                    //}
                }
            }

            if (QUALITY2_GRID.Visibility == Visibility.Visible && QUALITY2_GRID.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    //bool isValid = false;

                    DataRow[] filterRows = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            //필수 항목 만큼 LOOP 처리를 못함
                            //주석처리함(2021.03.11)
                            //isValid = true;
                            //break;

                            //2021.03.11 추가
                            //필수 항목 만큼 LOOP 처리하기 위함
                            Util.MessageValidation("SFU3601", sItemName);   ///해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                            return false;
                        }
                    }

                    //2021.03.11 주석처리
                    //if (isValid == false)
                    //{
                    //    Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                    //    return false;
                    //}
                }
            }
            return true;
        }

        // 상/하한값 필수 입력 체크 Validation 추가 [2018-07-18]
        private bool ValidQualitySpecRequired()
        {
            //if (QUALITY_GRID.Visibility == Visibility.Visible && QUALITY2_GRID.Visibility != Visibility.Visible && QUALITY_GRID.Rows.Count > 0)
            if (QUALITY_GRID.Visibility == Visibility.Visible && QUALITY_GRID.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(QUALITY_GRID.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(QUALITY_GRID.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }

            if (QUALITY2_GRID.Visibility == Visibility.Visible && QUALITY2_GRID.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    string itemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidWorkTime()
        {
            if (string.IsNullOrEmpty(txtStartDateTime.Text))
            {
                Util.MessageValidation("SFU1696");  //시작시간 정보가 없습니다.
                return false;
            }

            double totalMin = (Convert.ToDateTime(txtEndDateTime.Text) - Convert.ToDateTime(txtStartDateTime.Text)).TotalMinutes;

            if (totalMin < 0)
            {
                Util.MessageValidation("SFU1219");  //가동시간을 확인 하세요.
                return false;
            }
            return true;
        }

        private bool ValidShift()
        {
            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력하세요.
                return false;
            }

            return true;
        }

        private bool ValidOperator()
        {
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }

            return true;
        }

        private bool ValidVersion()
        {
            if (string.IsNullOrEmpty(txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
            return true;
        }

        private bool ValidLaneQty()
        {
            if (string.IsNullOrEmpty(Util.NVC(txtLaneQty.Value)) || txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");  //Lane 정보가 없습니다
                return false;
            }
            return true;
        }

        private bool ValidLaneWrongQty()
        {
            if (!procId.Equals(Process.PRE_MIXING) && !procId.Equals(Process.BS) && !procId.Equals(Process.CMC) && !procId.Equals(Process.InsulationMixing) && !procId.Equals(Process.MIXING) && !procId.Equals(Process.SLITTING))
            {
                if (procId.Equals(Process.HALF_SLITTING))
                {
                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY")) == 1)
                            return false;
                }
                else
                {
                    if (txtLaneQty.Value == 1)
                        return false;
                }
            }
            return true;
        }

        private bool ValidOverProdQty()
        {
            double inputQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
            double lossQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));

            if (inputQty < lossQty)
            {
                Util.MessageValidation("SFU3236");
                return false;
            }
            return true;
        }

        private bool ValidConfirmQty()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

            if (isCoaterAfterProcess)
            {
                if (exceedLengthQty > 0)
                {
                    // 투입량 <> (생산량 - 길이초과) 여야 확정 가능
                    if (Util.NVC_Decimal(txtParentQty.Value) != (inputQty - exceedLengthQty))
                    {
                        Util.MessageValidation("SFU3417");  //길이초과 입력 시 잔량이 0이어야 합니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidCutOverProdQty()
        {
            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                int iCount = isResnCountUse == true ? 1 : 0;

                double inputQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

                for (int i = WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount); i < WIPREASON_GRID.Columns["COSTCENTERID"].Index; i += (2 + iCount))
                {
                    if (inputQty < SumDefectQty(WIPREASON_GRID, WIPREASON_GRID.Columns[i].Name))
                    {
                        Util.MessageValidation("SFU3236");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidInOutQty()
        {
            if (txtSrs3Qty.Value > 0 && txtSrs2Qty.Value == 0)
            {
                Util.MessageValidation("SFU3725");  //Mid수량이 입력된 상태에서 Out수량을 입력히지 않으면 실적확정이 불가능 합니다. (In/Out수량 확인 후 진행)
                return false;
            }
            return true;
        }

        private bool ValidLabelPass()
        {
            bool bRet = true;
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID");
            dt.Columns.Add("EQPTID");

            DataRow dr = dt.NewRow();
            dr["LOTID"] = _CUTID;
            dr["EQPTID"] = _EQPTID;
            dt.Rows.Add(dr);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DATA_ECLCT", "INDATA", "OUTDATA", dt);

            if (rslt != null && rslt.Rows.Count > 0)
            {
                bRet = false;
            }
            else
            {
                bRet = true;
            }
            return bRet;
        }

        private bool ValidQualitySpec(string validType)
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = QUALITY_GRID.ItemsSource == null ? null : ((DataView)QUALITY_GRID.ItemsSource).ToTable();
                DataTable qualityList2 = QUALITY2_GRID.ItemsSource == null ? null : ((DataView)QUALITY2_GRID.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                        if (!qualityList.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList.Rows[i]["CLCTVAL01"].ToString();
                        }

                        if (!String.IsNullOrWhiteSpace(LSL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            //validType이 Hold면 자동보류여부를 체크하고
                            //아니면 체크 안하고
                            if (Util.NVC_Decimal(LSL) > 0 && Util.NVC_Decimal(LSL) > Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(USL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (Util.NVC_Decimal(USL) > 0 && Util.NVC_Decimal(USL) < Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                    }
                }

                if (qualityList2 != null && qualityList2.Rows.Count > 0)
                {
                    for (int j = 0; j < qualityList2.Rows.Count; j++)
                    {
                        LSL = qualityList2.Rows[j]["LSL"].ToString();
                        USL = qualityList2.Rows[j]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList2.Rows[j]["AUTO_HOLD_FLAG"].ToString();

                        if (!qualityList2.Rows[j]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList2.Rows[j]["CLCTVAL01"].ToString();
                        }

                        if (!String.IsNullOrWhiteSpace(LSL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (Util.NVC_Decimal(LSL) > 0 && Util.NVC_Decimal(LSL) > Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(USL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (Util.NVC_Decimal(USL) > 0 && Util.NVC_Decimal(USL) < Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        private bool ValidQualitySpecExists()
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = QUALITY_GRID.ItemsSource == null ? null : ((DataView)QUALITY_GRID.ItemsSource).ToTable();
                DataTable qualityList2 = QUALITY2_GRID.ItemsSource == null ? null : ((DataView)QUALITY2_GRID.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;
                string SPEC_TYPE_CODE = string.Empty;  //201.03.11 추가

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();
                        SPEC_TYPE_CODE = qualityList.Rows[i]["SPEC_TYPE_CODE"].ToString();  //2021.03.11 추가

                        if (!qualityList.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList.Rows[i]["CLCTVAL01"].ToString();
                        }

                        if (AUTO_HOLD_FLAG.Equals("Y") && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (SPEC_TYPE_CODE == "B")
                            {
                                if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                            else if (SPEC_TYPE_CODE == "U")
                            {
                                if (string.IsNullOrWhiteSpace(USL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                            else if (SPEC_TYPE_CODE == "L")
                            {
                                if (string.IsNullOrWhiteSpace(LSL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (qualityList2 != null && qualityList2.Rows.Count > 0)
                {
                    for (int j = 0; j < qualityList2.Rows.Count; j++)
                    {
                        LSL = qualityList2.Rows[j]["LSL"].ToString();
                        USL = qualityList2.Rows[j]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList2.Rows[j]["AUTO_HOLD_FLAG"].ToString();
                        SPEC_TYPE_CODE = qualityList2.Rows[j]["SPEC_TYPE_CODE"].ToString(); //2021.03.11 추가

                        if (!qualityList2.Rows[j]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList2.Rows[j]["CLCTVAL01"].ToString();
                        }

                        if (AUTO_HOLD_FLAG.Equals("Y") && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (SPEC_TYPE_CODE == "B")
                            {
                                if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                            else if (SPEC_TYPE_CODE == "U")
                            {
                                if (string.IsNullOrWhiteSpace(USL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                            else if (SPEC_TYPE_CODE == "L")
                            {
                                if (string.IsNullOrWhiteSpace(LSL))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        private void CheckLabelPassHold(Action callback)
        {
            try
            {
                //라벨링 패스 기능은 기존에도 활성화 되어 있었음
                _LABEL_PASS_HOLD_FLAG = "Y";

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID");
                dt.Columns.Add("CMCDTYPE");
                dt.Columns.Add("CMCODE");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LABEL_PASS_HOLD_CHECK";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", dt);

                if (rslt != null && rslt.Rows.Count > 0)
                {
                    if (!ValidLabelPass())
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8218"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            //홀드
                            //전극의 모든 실적확정 로직에 플래그를 넣어서 Y면 자주검사스펙 홀드를 통과하도록하고 N이면 넘긴다
                            //MMD 동별 자주검사에서 자동홀드여부도 Y로 바꿔야 한다.
                            //또한 홀드를 걸 항목에 대해서만 LSL USL을 등록한다.
                            if (result == MessageBoxResult.OK)
                            {
                                _LABEL_PASS_HOLD_FLAG = "Y";
                            }
                            else
                            {
                                _LABEL_PASS_HOLD_FLAG = "N";
                            }
                            callback();
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckAuthValidation(Action callback)
        {
            try
            {
                // AD 인증 기능 추가 [2019-08-21]
                DataTable confirmDt = GetConfirmAuthVaildation();

                if (confirmDt != null && confirmDt.Rows.Count > 0)
                {
                    // 강제 인터락 체크 (이거는 공용 메세지로 공유하니 필요 시 MES MESSAGE 코드 별도 추가 필요)
                    if (string.Equals(confirmDt.Rows[0]["VALIDATION_FLAG"], "Y"))
                    {
                        // 실적확정은 자동 Interlock 기능에 의하여 보류 되었습니다. [%1]
                        Util.MessageValidation("SFU5125", new object[] { Util.NVC(confirmDt.Rows[0]["RSLT_CNFM_TYPE_CODE"]) });
                        return;
                    }

                    // AD 인증 체크
                    if (string.Equals(confirmDt.Rows[0]["AD_CHK_FLAG"], "Y"))
                    {
                        LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                        authConfirm.FrameOperation = FrameOperation;
                        authConfirm.sContents = Util.NVC(confirmDt.Rows[0]["DISP_MSG"]);
                        if (authConfirm != null)
                        {
                            // SBC AD 인증
                            if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "SBC_AD"))
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);

                                C1WindowExtension.SetParameters(authConfirm, Parameters);

                            }
                            else if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "LGCHEM_AD"))
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);
                                Parameters[1] = "lgchem.com";

                                C1WindowExtension.SetParameters(authConfirm, Parameters);
                            }
                            authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                            this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                        }
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void CheckSpecOutHold(Action callback)
        {
            try
            {
                //자동보류여부에 체크되어 있으면 무조건 홀드를 건다.
                if (!ValidQualitySpec("Hold"))
                {
                    //자동HOLD되도록 설정된 품질검사 결과가 기준치를 만족하지 못했습니다. 완성랏이 홀드됩니다. 계속하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8185"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            callback();
                        }
                    });
                }
                else
                {
                    if (!ValidQualitySpecExists())
                    {
                        //LSL, USL 미설정되어 계속 진행할 경우 완성LOT이 HOLD처리 됩니다. 계속하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8186"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                callback();
                            }
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void CoatorConfirmProcess()
        {

        }

        #region 실적확정시 허용비율 초과 팝업 실행 (2024-03-12)
        private DataTable GetPermitRateOverData()
        {
            DataTable dtLotInfo = DataTableConverter.Convert(LOTINFO_GRID.ItemsSource);
            decimal eqpEndQty = Util.NVC_Decimal(dtLotInfo.Rows[0]["EQPT_END_QTY"]); // 장비수량

            // 코터 공정의 경우 불량/LOSS/물품청구 TOP, BACK 자료 모두 존재할 경우, 입력 수량의 1/2로 처리함 
            //decimal halfDevide = 2.0m;
            //bool isHalfTopInCoater = false;
            //if ((procId.Equals(Process.COATING) || procId.Equals(Process.INS_COATING)) // 코터, 단면코터, 절연코더...
            //    && WIPREASON2_GRID.ItemsSource != null)
            //{
            //    isHalfTopInCoater = true;
            //}

            DataTable dtPermitRate = new DataTable();

            foreach (DataRow rowLot in dtLotInfo.Rows)
            {
                DataTable dtTemp1 = new DataTable();
                DataTable dtTemp2 = new DataTable();

                string lotID = rowLot["LOTID"].ToString();
                //GetWipSeq(lotID, string.Empty); // 전역변수: _WIPSEQ 세팅 
                string wipSeq = String.IsNullOrEmpty(_WIPSEQ) ? null : _WIPSEQ;

                string resnQtyName = "RESNQTY"; // 수량 컬럼명 

                // 슬리팅 공정의 경우, 각 태그 (실제 LOTID별) 수량 체크 
                if (procId.Equals(Process.SLITTING) || procId.Equals(Process.TWO_SLITTING) || procId.Equals(Process.HALF_SLITTING))
                {
                    resnQtyName = lotID;
                }

                List<DataRow> permitRateDataRow = DataTableConverter.Convert(WIPREASON_GRID.ItemsSource)
                .AsEnumerable()
                .Where(row => Util.NVC_Decimal(row["PERMIT_RATE"]) > 0 && Math.Truncate(eqpEndQty * (Util.NVC_Decimal(row["PERMIT_RATE"]) / 100)) < (Util.NVC_Decimal(row[resnQtyName])))
                .ToList<DataRow>();

                List<DataRow> permitRateDataRow2 = DataTableConverter.Convert(WIPREASON2_GRID.ItemsSource)
                    .AsEnumerable().Where(row => Util.NVC_Decimal(row["PERMIT_RATE"]) > 0 && Math.Truncate(eqpEndQty * (Util.NVC_Decimal(row["PERMIT_RATE"]) / 100)) < Util.NVC_Decimal(row[resnQtyName]))
                    .ToList<DataRow>();


                //List<DataRow> permitRateDataRow = DataTableConverter.Convert(WIPREASON_GRID.ItemsSource)
                //.AsEnumerable()
                //.Where(row => Util.NVC_Decimal(row["PERMIT_RATE"]) > 0 && Math.Truncate(eqpEndQty * (Util.NVC_Decimal(row["PERMIT_RATE"]) / 100)) < (isHalfTopInCoater ? Util.NVC_Decimal(row[resnQtyName]) / halfDevide : Util.NVC_Decimal(row[resnQtyName])))
                //.ToList<DataRow>();

                //List<DataRow> permitRateDataRow2 = DataTableConverter.Convert(WIPREASON2_GRID.ItemsSource)
                //    .AsEnumerable().Where(row => Util.NVC_Decimal(row["PERMIT_RATE"]) > 0 && Math.Truncate(eqpEndQty * (Util.NVC_Decimal(row["PERMIT_RATE"]) / 100)) < Util.NVC_Decimal(row[resnQtyName]))
                //    .ToList<DataRow>();

                dtTemp1 = permitRateDataRow.Any() ? permitRateDataRow.CopyToDataTable() : DataTableConverter.Convert(WIPREASON_GRID.ItemsSource).Clone();
                dtTemp2 = permitRateDataRow2.Any() ? permitRateDataRow2.CopyToDataTable() : DataTableConverter.Convert(WIPREASON2_GRID.ItemsSource).Clone();

                if (!dtTemp1.Columns.Contains("LOTID")) { dtTemp1.Columns.Add(new DataColumn { ColumnName = "LOTID", DefaultValue = lotID }); }
                if (!dtTemp2.Columns.Contains("LOTID")) { dtTemp2.Columns.Add(new DataColumn { ColumnName = "LOTID", DefaultValue = lotID }); }

                if (!dtTemp1.Columns.Contains("WIPSEQ")) { dtTemp1.Columns.Add(new DataColumn { ColumnName = "WIPSEQ", DefaultValue = wipSeq }); }
                if (!dtTemp2.Columns.Contains("WIPSEQ")) { dtTemp2.Columns.Add(new DataColumn { ColumnName = "WIPSEQ", DefaultValue = wipSeq }); }

                dtPermitRate.Merge(dtTemp1);
                dtPermitRate.Merge(dtTemp2);

            }
            return dtPermitRate;
        }

        private void SetPermitRateOverHis(DataTable dtPermitRate, DataSet inDataSet, string bizRuleName, string inDataTableName)
        {
            DataTable dtLotInfo = DataTableConverter.Convert(LOTINFO_GRID.ItemsSource);
            decimal eqpEndQty = Util.NVC_Decimal(dtLotInfo.Rows[0]["EQPT_END_QTY"]); // 장비수량 
            string lotID = dtLotInfo.Rows[0]["LOTID"].ToString(); // 의미 없음 
            //GetWipSeq(lotID, string.Empty); // 전역변수: _WIPSEQ 세팅 // 의미 없음 

            // 불량발생 수량 컬럼명 
            string resnQtyName = "RESNQTY";

            if (procId.Equals(Process.SLITTING) /* || procId.Equals(Process.TWO_SLITTING) || procId.Equals(Process.HALF_SLITTING) */)
            {
                resnQtyName = "RESNTOTQTY";
            }

            // 허용 불량수량 초과 사유 입력 창 실행 
            CMM_PERMIT_RATE permitRatePopup = new CMM_PERMIT_RATE();
            permitRatePopup.FrameOperation = this.FrameOperation;

            DataTable input = new DataTable();
            input.Columns.Add("LOTID", typeof(string));
            input.Columns.Add("WIPSEQ", typeof(string));

            input.Columns.Add("ACTID", typeof(string));
            input.Columns.Add("ACTNAME", typeof(string));
            input.Columns.Add("RESNCODE", typeof(string));

            input.Columns.Add("RESNNAME", typeof(string));
            input.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
            input.Columns.Add("RESNQTY", typeof(string));
            input.Columns.Add("PERMIT_RATE", typeof(string));
            input.Columns.Add("OVER_QTY", typeof(string));
            input.Columns.Add("SPCL_RSNCODE", typeof(string));
            input.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
            input.Columns.Add("RESNNOTE", typeof(string));

            foreach (DataRow row in dtPermitRate.Rows)
            {
                if (procId.Equals(Process.SLITTING)/* || procId.Equals(Process.TWO_SLITTING) || procId.Equals(Process.HALF_SLITTING) */)
                {
                    resnQtyName = row["LOTID"].ToString();
                }

                // 초과 수량 계산 : 소수점 3자리까지 올림해서 보여준다. ex) 0.6723 => 0.673
                decimal d1 = Util.NVC_Decimal(row[resnQtyName]); // 장비수량 
                decimal d2 = Util.NVC_Decimal(row["PERMIT_RATE"]); // 허용비율 
                decimal d3 = d1 - ((eqpEndQty * d2) / 100); // 초과 수량 (소수점 그대로) 

                DataRow newRow = input.NewRow();
                newRow["LOTID"] = row["LOTID"];
                newRow["WIPSEQ"] = row["WIPSEQ"];
                newRow["ACTID"] = row["ACTID"];
                newRow["ACTNAME"] = row["ACTNAME"];
                newRow["RESNCODE"] = row["RESNCODE"];
                newRow["RESNNAME"] = row["RESNNAME"];
                newRow["DFCT_CODE_DETL_NAME"] = null; // 필수 x
                newRow["RESNQTY"] = String.Format("{0:0.000}", Util.NVC_Decimal(row[resnQtyName]));
                newRow["PERMIT_RATE"] = String.Format("{0:0.00}", row["PERMIT_RATE"]);
                //newRow["OVER_QTY"] = String.Format("{0:0.000}", Util.NVC_Decimal(row[resnQtyName]) - Math.Truncate(eqpEndQty * (Util.NVC_Decimal(row["PERMIT_RATE"]) / 100)));
                newRow["OVER_QTY"] = Math.Ceiling(d3 * 1000) / 1000; // 초과수량 소수점 4자리에서 3자리로 반올림 
                input.Rows.Add(newRow);
            }

            object[] parameters = new object[2];
            parameters[0] = lotID;
            parameters[1] = input;
            C1WindowExtension.SetParameters(permitRatePopup, parameters);

            permitRatePopup.Closed += (sender, e) => { permitRatePopup_Closed(sender, inDataSet, bizRuleName, inDataTableName); };
            Dispatcher.BeginInvoke(new Action(() => permitRatePopup.ShowModal()));
        }

        private void permitRatePopup_Closed(object sender, DataSet inDataSet, string bizRuleName, string inDataTableName)
        {
            try
            {
                CMM_PERMIT_RATE popup = sender as CMM_PERMIT_RATE;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 허용비율 사유 팝업 데이터 리턴 
                    dtPermitRateReturn.Clear();
                    dtPermitRateReturn = popup.PERMIT_RATE.Copy();
                    permitRateUerReturn = popup.UserID;
                    permitRateDeptReturn = popup.DeptID;

                    // 실적 확정 프로세스 
                    
                    ConfirmAfterLeavePermitRateReason(inDataSet, bizRuleName, inDataTableName);

                    // 불량 초과 사유 입력 
                    BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 불량비율 초과 사유 입력 Biz 
        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable dtLotInfo = dtPermitRateReturn.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                //string lotID = dtPermitRateReturn.Rows[0]["LOTID"].ToString();
                //GetWipSeq(lotID, string.Empty); // 전역변수: _WIPSEQ 세팅 
                string rLotID = "";

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));

                foreach (DataRow lotRow in dtLotInfo.Rows)
                {
                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();

                    rLotID = lotRow["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = null;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = rLotID;
                    newRow["WIPSEQ"] = lotRow["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;

                    foreach (DataRow retRow in dtPermitRateReturn.Rows)
                    {

                        if (rLotID.Equals(retRow["LOTID"]))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Util.NVC_Decimal(retRow["PERMIT_RATE"]);
                            newRow["ACTID"] = retRow["ACTID"].ToString();
                            newRow["RESNCODE"] = retRow["RESNCODE"].ToString();
                            newRow["RESNQTY"] = Util.NVC_Decimal(retRow["RESNQTY"]);
                            newRow["OVER_QTY"] = Util.NVC_Decimal(retRow["OVER_QTY"]);
                            newRow["REQ_USERID"] = permitRateUerReturn;
                            newRow["REQ_DEPTID"] = permitRateDeptReturn;
                            newRow["DIFF_RSN_CODE"] = retRow["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = retRow["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    string inTableNames = String.Join(",", indataSet.Tables.Cast<DataTable>().ToArray().Select(i => i.TableName));

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", inTableNames, null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            //Util.AlertInfo("정상 처리 되었습니다.");
                            //Util.MessageInfo("SFU1275");
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                    // 연속 호출시 선행 서비스 파라미터 덮어씀 방지 (ytkim29. 2024-02-14) 
                    System.Threading.Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 실적확정시 불량 초과 사유 팝업 미실행 (아직 반영전 공정에 쓰임) 
        private DataTable GetPermitRateEmptyData()
        {
            return new DataTable();
        }


        // 불량 허용비율 초과 사유 입력 후 실적 확정 
        private void ConfirmAfterLeavePermitRateReason(DataSet inDataSet, string bizRuleName, string inDataTableName)
        {

            new ClientProxy().ExecuteService_Multi(bizRuleName, inDataTableName, null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                isConfirm = true;
                REFRESH = true;
            }, inDataSet);
        }

        #endregion


        void ConfirmProcess(bool bRealWorkerSelFlag = false)
        {
            string listData = string.Empty;

            // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
            string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

            // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴
            string topInfo = string.Empty;

            if (!string.Equals(procId, Process.SRS_COATING) && !string.Equals(procId, Process.SRS_SLITTING))
            {
                decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                decimal eqptEndQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "EQPT_END_QTY"));

                if (Math.Abs((prodQty - eqptEndQty)) >= 30)
                    topInfo = MessageDic.Instance.GetMessage("SFU3468", txtUnit.Text);  // 설비 양품량과 생산량 차이가 30M이상입니다.\n실물 수량이 정말 맞습니까?
            }

            // LANE수 정보 표시 (믹서는 이전 확정 수량 VALIDATION)
            string addMessage = string.Empty;

            // 설비 LOSS 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(Util.NVC(EQUIPMENT_COMBO.SelectedValue), procId);

            if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0)
            {
                int iLossCnt = Util.NVC_Int(dtEqpLossInfo.Rows[0]["CNT"]);

                if (iLossCnt > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }
                    addMessage = MessageDic.Instance.GetMessage("SFU3501", new object[] { sLossInfo });
                }
            }

            if (procId.Equals(Process.PRE_MIXING) || procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing) || procId.Equals(Process.MIXING))
            {
                DataTable dt = GetMixerPreData();

                if (dt.Rows.Count > 0)
                {
                    decimal goodQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                    decimal mixOutQty = string.IsNullOrEmpty(Util.NVC(dt.Rows[0]["WIPQTY_ED"])) ? 0 : Util.NVC_Decimal(dt.Rows[0]["WIPQTY_ED"]);

                    if (Math.Abs(goodQty - mixOutQty) > 10)
                    {
                        if (string.IsNullOrEmpty(addMessage))
                            addMessage = MessageDic.Instance.GetMessage("SFU3602", Util.NVC(dt.Rows[0]["LOTID"])); // 이전 확정 생산LOT[%1]의 수량보다 ±10KG 초과하였습니다.
                        else
                            addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3602", Util.NVC(dt.Rows[0]["LOTID"])); // 이전 확정 생산LOT[%1]의 수량보다 ±10KG 초과하였습니다.
                    }
                }
            }
            else
            {
                if (ValidLaneWrongQty() == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // 단면코터에 한해서는 이상 있는 부분에 대해서는 추가 인폼 추가 [2017-09-19] ==> 소형 1동 투입자재 엉망으로 하는 문제로 인하여 구현
                if (string.Equals(procId, Process.COATING) && isSingleCoater == true && string.Equals(COATTYPE_COMBO.SelectedValue, "T"))
                {
                    // 단면 TOP에 FOIL 자재로 LOT이 존재하면 안됨
                    string sLot = string.Empty;
                    DataTable inputValid = GetBackCoaterInputMaterialValid(_LOTID, Util.NVC(COATTYPE_COMBO.SelectedValue));
                    if (inputValid != null && inputValid.Rows.Count > 0)
                    {
                        foreach (DataRow row in inputValid.Rows)
                            sLot += Util.NVC(row["INPUT_LOTID"]) + ",";

                        addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4118", string.IsNullOrEmpty(sLot) ? sLot : sLot.Substring(0, (sLot.Length - 1)))
                            : "\n" + MessageDic.Instance.GetMessage("SFU4118", string.IsNullOrEmpty(sLot) ? sLot : sLot.Substring(0, (sLot.Length - 1)));   //해당 Top코터에서는 Top Lot[%1]을 Core에 장착 할 수 없습니다. 투입자재 탭에서 투입된 Foil삭제 후 수동으로 추가 바랍니다.
                    }
                }
                else if (string.Equals(procId, Process.COATING) && isSingleCoater == true && string.Equals(COATTYPE_COMBO.SelectedValue, "B"))
                {
                    // 단면 BACK에 TOP LOT이 하나도 존재하지 않음면 안됨
                    DataTable inputValid = GetBackCoaterInputMaterialValid(_LOTID, Util.NVC(COATTYPE_COMBO.SelectedValue));
                    if (inputValid == null || inputValid.Rows.Count == 0)
                        addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4119") : "\n" + MessageDic.Instance.GetMessage("SFU4119");  //해당 Back코터에서 투입처리 된 Top Lot이 없습니다. 투입자재 탭에서 수동으로 사용하실 Top Lot를 저장하시고 진행 바랍니다.

                    // 첫 번째 CUT일 경우 TOP LOT의 버전과 비교하여 INFORM 처리 [2018-05-24]
                    string sCutNo = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "CUT"));
                    if (inputValid != null && inputValid.Rows.Count > 0 && string.Equals(sCutNo, "1"))
                    {
                        foreach (DataRow inputRow in inputValid.Rows)
                        {
                            if (!string.Equals(txtVersion.Text, Util.NVC(inputRow["PROD_VER_CODE"])))
                            {
                                addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4936", new object[] { Util.NVC(inputRow["INPUT_LOTID"]), Util.NVC(inputRow["PROD_VER_CODE"]), txtVersion.Text }) :
                                    "\n" + MessageDic.Instance.GetMessage("SFU4936", new object[] { Util.NVC(inputRow["INPUT_LOTID"]), Util.NVC(inputRow["PROD_VER_CODE"]), txtVersion.Text });  //투입된 Top Lot[%1]의 버전정보[%2]가 실적확정 할 버전[%3]과 다르니 확인하시고 실적확정 하시기 바랍니다.
                                break;
                            }
                        }
                    }

                }
                else if (string.Equals(procId, Process.HALF_SLITTING))
                {
                    // H/S공정 설비양품량 > 확정양품량 일 경우 인폼 추가 [2018-01-17]
                    double eqptQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "EQPT_END_QTY"));
                    double goodQty = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                    if (eqptQty > 0 && eqptQty > goodQty)
                        addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty }) : "\n" + MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty });  //설비수량[%1]이 양품량[%2]보다 많습니다. 불량/LOSS수량등 입력되지 않았는지 확인 바랍니다.
                }
            }

            // REMARK 필요 정보 취합 [2017-05-24]
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU3484"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

            // HOLD 특이사항 정보 취합 [2019-04-24]
            Dictionary<string, string> postremarkInfo = GetPostRemarkConvert();
            if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)))
            {
                if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                {
                    if (postremarkInfo.Count == 0)
                    {
                        Util.MessageValidation("SFU3484"); // 특이사항 정보를 확인 바랍니다.
                        return;
                    }
                }
            }

            // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
            Dictionary<int, string> finalCutInfo = null;
            if (isCoaterAfterProcess)
            {
                finalCutInfo = CheckFinalCutLot();
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;
            }

            #region 작업자 실명관리 기능 추가
            if (!bRealWorkerSelFlag && CheckRealWorkerCheckFlag())
            {
                CMM001.CMM_COM_INPUT_USER wndRealWorker = new CMM001.CMM_COM_INPUT_USER();

                wndRealWorker.FrameOperation = FrameOperation;
                object[] Parameters2 = new object[0];
                //Parameters2[0] = "";

                C1WindowExtension.SetParameters(wndRealWorker, Parameters2);

                wndRealWorker.Closed -= new EventHandler(wndRealWorker_Closed);
                wndRealWorker.Closed += new EventHandler(wndRealWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndRealWorker.ShowModal()));

                return;
            }
            #endregion

            if (procId.Equals(Process.PRE_MIXING))
            {
                //실적 확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inDataTable.Columns.Add("SHIFT", typeof(string));
                        inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inDataTable.Columns.Add("MILL_COUNT", typeof(Int32));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inDataRow = null;

                        inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inDataRow["SHIFT"] = txtShift.Tag;
                        inDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        inDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inDataRow["MILL_COUNT"] = Util.NVC_Int(txtBeadMillCount.Value);
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inDataTable.Rows.Add(inDataRow);

                        DataRow inLotDataRow = null;

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _LOTID;
                        inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                        InLotdataTable.Rows.Add(inLotDataRow);
                        #endregion CONFIRM MESSAGE

                        string bizRuleName = "BR_PRD_REG_END_LOT_PM";
                        string inDataTableName = "INDATA,INLOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_PM", "INDATA,INLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }


                    }
                }, false, false, topInfo);
            }
            else if (procId.Equals(Process.BS))
            {
                //실적 확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inDataTable.Columns.Add("SHIFT", typeof(string));
                        inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inDataRow = null;
                        inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inDataRow["SHIFT"] = txtShift.Tag;
                        inDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        inDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

                        DataRow inLotDataRow = null;
                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _LOTID;
                        inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                        InLotdataTable.Rows.Add(inLotDataRow);
                        #endregion CONFIRM MESSAGE

                        string bizRuleName = "BR_PRD_REG_END_LOT_BS";
                        string inDataTableName = "INDATA,INLOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_BS", "INDATA,INLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }

                        
                    }
                }, false, false, topInfo);
            }
            else if (procId.Equals(Process.CMC))
            {
                //실적 확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inDataTable.Columns.Add("SHIFT", typeof(string));
                        inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inDataRow = null;
                        inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inDataRow["SHIFT"] = txtShift.Tag;
                        inDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        inDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

                        DataRow inLotDataRow = null;
                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _LOTID;
                        inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                        InLotdataTable.Rows.Add(inLotDataRow);
                        #endregion CONFIRM MESSAGE

                        string bizRuleName = "BR_PRD_REG_END_LOT_CMC";
                        string inDataTableName = "INDATA,INLOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CMC", "INDATA,INLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }

                        
                    }
                }, false, false, topInfo);
            }
            else if (procId.Equals(Process.InsulationMixing))
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inDataTable.Columns.Add("SHIFT", typeof(string));
                        inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inDataRow = null;
                        inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inDataRow["SHIFT"] = txtShift.Tag;
                        inDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        inDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

                        DataRow inLotDataRow = null;
                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _LOTID;
                        inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                        InLotdataTable.Rows.Add(inLotDataRow);

                        string bizRuleName = "BR_PRD_REG_END_LOT_INSULT_MX";
                        string inDataTableName = "INDATA,INLOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_INSULT_MX", "INDATA,INLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }
                    }
                }, false, false, topInfo);
            }

            else if (procId.Equals(Process.MIXING))
            {
                //실적 확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inDataTable.Columns.Add("SHIFT", typeof(string));
                        inDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inDataRow = null;

                        inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inDataRow["SHIFT"] = txtShift.Tag;
                        inDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        inDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);

                        inDataTable.Rows.Add(inDataRow);

                        DataRow inLotDataRow = null;

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("INPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLotdataTable.Columns.Add("RESNQTY", typeof(decimal));

                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _LOTID;
                        inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                        InLotdataTable.Rows.Add(inLotDataRow);
                        #endregion CONFIRM MESSAGE

                        string bizRuleName = "BR_PRD_REG_END_LOT_MX";
                        string inDataTableName = "INDATA,INLOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_MX", "INDATA,INLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }

                        
                    }
                }, false, false, topInfo);
            }
            else if (procId.Equals(Process.COATING))
            {
                if (isSingleCoater)
                {
                    string ListData = string.Empty;

                    //실적확정 FIFO : 이전 Cut 장비완료 Validation 
                    DataTable inTable = new DataTable();
                    inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));

                    DataRow indata = inTable.NewRow();
                    indata = inTable.NewRow();
                    indata["LOTID"] = _LOTID;
                    inTable.Rows.Add(indata);

                    DataTable Fifodt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTEND_LOT_CT", "INDATA", "RSLTDT", inTable);

                    if (Fifodt.Rows.Count > 0)
                    {
                        if (!Fifodt.Rows[0]["LOTID"].ToString().Equals(_LOTID))
                        {
                            Util.MessageValidation("SFU2046", new object[] { Fifodt.Rows[0]["LOTID"].ToString() });
                            return;
                        }
                    }

                    // 단면코터 BACK은 공통 CONFIRM 분기 ( 2017-01-17 )
                    if (string.Equals(COATTYPE_COMBO.SelectedValue, "B"))
                    {
                        string _ValueToMessage = string.Empty;

                        if (chkFinalCut.IsChecked.Value)
                        {
                            inTable = new DataTable();
                            inTable.Columns.Add("LOTID", typeof(string));

                            indata = inTable.NewRow();
                            indata["LOTID"] = _LOTID;
                            inTable.Rows.Add(indata);

                            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", inTable);

                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; ++i)
                                    if (i == 0)
                                        ListData = string.Concat(ListData, " ", dt.Rows[i]["LOTID"].ToString());
                                    else
                                        ListData = string.Concat(ListData, ", ", dt.Rows[i]["LOTID"].ToString());
                            }

                            _ValueToMessage = MessageDic.Instance.GetMessage("SFU2043", new object[] { _LOTID });    //확정 하려는{0} LOT은 FINAL CUT 입니다.\r\n확정 하시면 대랏은 종결처리되며, LOT 추가/삭제는 불가합니다.\r\n저장 하시겠습니까?
                        }
                        else
                        {
                            _ValueToMessage = MessageDic.Instance.GetMessage("SFU1241");    //저장하시겠습니까?
                        }

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(_ValueToMessage, addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                #region CONFIRM MESSAGE                                
                                //실적확정
                                DataSet inDataSet = null;
                                inDataSet = new DataSet();

                                DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");

                                inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                                inLotDataTable.Columns.Add("IFMODE", typeof(string));
                                inLotDataTable.Columns.Add("EQPTID", typeof(string));
                                inLotDataTable.Columns.Add("LOTID", typeof(string));
                                inLotDataTable.Columns.Add("INPUTQTY", typeof(string));
                                inLotDataTable.Columns.Add("OUTPUTQTY", typeof(string));
                                inLotDataTable.Columns.Add("RESNQTY", typeof(string));
                                inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                                inLotDataTable.Columns.Add("SHIFT", typeof(string));
                                inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                                inLotDataTable.Columns.Add("WIPNOTE", typeof(string));
                                inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                                inLotDataTable.Columns.Add("WRK_USERID", typeof(string));
                                inLotDataTable.Columns.Add("LAST_FLAG", typeof(string));
                                inLotDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
                                inLotDataTable.Columns.Add("USERID", typeof(string));
                                inLotDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                                inLotDataTable.Columns.Add("LANE_QTY", typeof(decimal));
                                inLotDataTable.Columns.Add("CALDATE", typeof(DateTime));

                                DataRow inLotDataRow = null;
                                inLotDataRow = inLotDataTable.NewRow();
                                inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                inLotDataRow["EQPTID"] = _EQPTID;
                                inLotDataRow["LOTID"] = _LOTID;
                                inLotDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                                inLotDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                                inLotDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                                inLotDataRow["PROD_VER_CODE"] = Util.NVC(txtVersion.Text);
                                inLotDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                                inLotDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                                inLotDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                                inLotDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                                inLotDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                                inLotDataRow["LAST_FLAG"] = chkFinalCut.IsChecked.Value == true ? "Y" : "N";
                                inLotDataRow["COAT_SIDE_TYPE"] = COATTYPE_COMBO.SelectedValue;
                                inLotDataRow["USERID"] = LoginInfo.USERID;
                                inLotDataRow["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                                inLotDataRow["LANE_QTY"] = txtLaneQty.Value;
                                inLotDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                                inLotDataTable.Rows.Add(inLotDataRow);
                                #endregion CONFIRM MESSAGE

                                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CT_SINGLE", "INDATA, ", null, (result, searchException) =>
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }
                                    isConfirm = true;
                                    REFRESH = true;

                                }, inDataSet);
                            }
                        }, false, false, topInfo);
                    }
                    else
                    {
                        string _ValueToMessage = string.Empty;

                        if (chkFinalCut.IsChecked.Value)
                        {
                            inTable = new DataTable();
                            inTable.Columns.Add("LOTID", typeof(string));

                            indata = inTable.NewRow();
                            indata["LOTID"] = _LOTID;
                            inTable.Rows.Add(indata);

                            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", inTable);

                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; ++i)
                                    if (i == 0)
                                        ListData = string.Concat(ListData, " ", dt.Rows[i]["LOTID"].ToString());
                                    else
                                        ListData = string.Concat(ListData, ", ", dt.Rows[i]["LOTID"].ToString());
                            }

                            _ValueToMessage = MessageDic.Instance.GetMessage("SFU2043", new object[] { _LOTID });    //확정 하려는{0} LOT은 FINAL CUT 입니다.\r\n확정 하시면 대랏은 종결처리되며, LOT 추가/삭제는 불가합니다.\r\n저장 하시겠습니까?
                        }
                        else
                        {
                            _ValueToMessage = MessageDic.Instance.GetMessage("SFU1241");    //저장하시겠습니까?
                        }

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(_ValueToMessage, addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                #region CONFIRM MESSAGE                                
                                DataSet inDataSet = null;
                                inDataSet = new DataSet();

                                DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                                inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                                inLotDataTable.Columns.Add("IFMODE", typeof(string));
                                inLotDataTable.Columns.Add("EQPTID", typeof(string));
                                inLotDataTable.Columns.Add("LOTID", typeof(string));
                                inLotDataTable.Columns.Add("INPUTQTY", typeof(string));
                                inLotDataTable.Columns.Add("OUTPUTQTY", typeof(string));
                                inLotDataTable.Columns.Add("RESNQTY", typeof(string));
                                inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                                inLotDataTable.Columns.Add("SHIFT", typeof(string));
                                inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                                inLotDataTable.Columns.Add("WIPNOTE", typeof(string));
                                inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                                inLotDataTable.Columns.Add("WRK_USERID", typeof(string));
                                inLotDataTable.Columns.Add("LAST_FLAG", typeof(string));
                                inLotDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
                                inLotDataTable.Columns.Add("USERID", typeof(string));
                                inLotDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                                inLotDataTable.Columns.Add("LANE_QTY", typeof(decimal));
                                inLotDataTable.Columns.Add("CALDATE", typeof(DateTime));

                                indata = inLotDataTable.NewRow();
                                indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                indata["IFMODE"] = IFMODE.IFMODE_OFF;
                                indata["EQPTID"] = _EQPTID;
                                indata["LOTID"] = _LOTID;
                                indata["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                                indata["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                                indata["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                                indata["PROD_VER_CODE"] = Util.NVC(txtVersion.Text);
                                indata["SHIFT"] = Util.NVC(txtShift.Tag);
                                indata["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                                indata["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                                indata["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                                indata["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                                indata["LAST_FLAG"] = chkFinalCut.IsChecked.Value == true ? "Y" : "N";
                                indata["COAT_SIDE_TYPE"] = COATTYPE_COMBO.SelectedValue;
                                indata["USERID"] = LoginInfo.USERID;
                                indata["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                                indata["LANE_QTY"] = txtLaneQty.Value;
                                indata["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);

                                inLotDataTable.Rows.Add(indata);
                                #endregion CONFIRM MESSAGE

                                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CT_SINGLE", "INDATA", null, (result, searchException) =>
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }
                                    isConfirm = true;
                                    REFRESH = true;

                                }, inDataSet);
                            }
                        }, false, false, topInfo);
                    }
                }
                else
                {
                    string ListData = string.Empty;

                    //실적확정 FIFO : 이전 Cut 장비완료 Validation 
                    DataTable inTable = new DataTable();
                    inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));

                    DataRow indata = inTable.NewRow();
                    indata = inTable.NewRow();
                    indata["LOTID"] = _LOTID;
                    inTable.Rows.Add(indata);

                    DataTable Fifodt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTEND_LOT_CT", "INDATA", "RSLTDT", inTable);

                    if (Fifodt.Rows.Count > 0)
                    {
                        if (!Fifodt.Rows[0]["LOTID"].ToString().Equals(_LOTID))
                        {
                            Util.MessageValidation("SFU2046", new object[] { Fifodt.Rows[0]["LOTID"].ToString() });
                            return;
                        }
                    }

                    string _ValueToMessage = string.Empty;

                    if (chkFinalCut.IsChecked.Value)
                    {
                        inTable = new DataTable();
                        inTable.Columns.Add("LOTID", typeof(string));

                        indata = inTable.NewRow();
                        indata["LOTID"] = _LOTID;
                        inTable.Rows.Add(indata);

                        DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", inTable);

                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; ++i)
                                if (i == 0)
                                    ListData = string.Concat(ListData, " ", dt.Rows[i]["LOTID"].ToString());
                                else
                                    ListData = string.Concat(ListData, ", ", dt.Rows[i]["LOTID"].ToString());
                        }

                        _ValueToMessage = MessageDic.Instance.GetMessage("SFU2043", new object[] { _LOTID });    //확정 하려는{0} LOT은 FINAL CUT 입니다.\r\n확정 하시면 대랏은 종결처리되며, LOT 추가/삭제는 불가합니다.\r\n저장 하시겠습니까?
                    }
                    else
                    {
                        _ValueToMessage = MessageDic.Instance.GetMessage("SFU1241");    //저장하시겠습니까?
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(_ValueToMessage, addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                    {
                        if (vResult == MessageBoxResult.OK)
                        {
                            #region CONFIRM MESSAGE                            
                            DataSet inDataSet = null;
                            inDataSet = new DataSet();

                            DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                            inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inLotDataTable.Columns.Add("IFMODE", typeof(string));
                            inLotDataTable.Columns.Add("EQPTID", typeof(string));
                            inLotDataTable.Columns.Add("LOTID", typeof(string));
                            inLotDataTable.Columns.Add("INPUTQTY", typeof(string));
                            inLotDataTable.Columns.Add("OUTPUTQTY", typeof(string));
                            inLotDataTable.Columns.Add("RESNQTY", typeof(string));
                            inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                            inLotDataTable.Columns.Add("SHIFT", typeof(string));
                            inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                            inLotDataTable.Columns.Add("WIPNOTE", typeof(string));
                            inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                            inLotDataTable.Columns.Add("WRK_USERID", typeof(string));
                            inLotDataTable.Columns.Add("USERID", typeof(string));
                            inLotDataTable.Columns.Add("LAST_FLAG", typeof(string));
                            inLotDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                            inLotDataTable.Columns.Add("LANE_QTY", typeof(decimal));
                            inLotDataTable.Columns.Add("CALDATE", typeof(DateTime));
                            inLotDataTable.Columns.Add("HOLD_YN", typeof(string));

                            indata = inLotDataTable.NewRow();
                            indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            indata["IFMODE"] = IFMODE.IFMODE_OFF;
                            indata["EQPTID"] = _EQPTID;
                            indata["LOTID"] = _LOTID;
                            indata["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                            indata["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                            indata["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LOSSQTY"));
                            indata["PROD_VER_CODE"] = Util.NVC(txtVersion.Text);
                            indata["SHIFT"] = Util.NVC(txtShift.Tag);
                            indata["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                            indata["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                            indata["LAST_FLAG"] = chkFinalCut.IsChecked.Value == true ? "Y" : "N";
                            indata["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                            indata["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                            indata["USERID"] = LoginInfo.USERID;
                            indata["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                            indata["LANE_QTY"] = txtLaneQty.Value;
                            indata["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                            indata["HOLD_YN"] = _HOLDYN_CHK;


                            inLotDataTable.Rows.Add(indata);
                            #endregion CONFIRM MESSAGE

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CT", "INDATA", null, (result, searchException) =>
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;
                            }, inDataSet);
                        }
                    }, false, false, topInfo);
                }
            }
            else if (procId.Equals(Process.HALF_SLITTING))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult, isHold) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        // 자동 Loss 처리 여부
                        if (bool.Parse(finalCutInfo[2]) == true)
                        {
                            if (SetLossLot(WIPREASON_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                return;

                            SaveDefect(WIPREASON_GRID);

                            if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                            {
                                if (SetLossLot(WIPREASON2_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                    return;

                                SaveDefect(WIPREASON2_GRID);
                            }
                        }

                        DataSet inDataSet = null;
                        inDataSet = new DataSet();

                        DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                        inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inLotDataTable.Columns.Add("IFMODE", typeof(string));
                        inLotDataTable.Columns.Add("EQPTID", typeof(string));
                        inLotDataTable.Columns.Add("USERID", typeof(string));
                        inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inLotDataTable.Columns.Add("SHIFT", typeof(string));
                        inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inLotDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inLotDataTable.Columns.Add("WIPNOTE", typeof(string));
                        inLotDataTable.Columns.Add("CUTYN", typeof(string));
                        inLotDataTable.Columns.Add("REMAINQTY", typeof(string));
                        inLotDataTable.Columns.Add("LANE_QTY", typeof(double));
                        inLotDataTable.Columns.Add("LANE_PTN_QTY", typeof(double));
                        inLotDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inLotDataRow = null;
                        inLotDataRow = inLotDataTable.NewRow();
                        inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inLotDataRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        inLotDataRow["USERID"] = LoginInfo.USERID;
                        inLotDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inLotDataRow["SHIFT"] = txtShift.Tag;
                        inLotDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inLotDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inLotDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inLotDataRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        inLotDataRow["REMAINQTY"] = finalCutInfo[6];
                        inLotDataRow["LANE_QTY"] = txtLaneQty.Value;
                        inLotDataRow["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                        inLotDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inLotDataTable.Rows.Add(inLotDataRow);

                        DataTable inPLotDataTable = inDataSet.Tables.Add("IN_INPUT");
                        inPLotDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inPLotDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inPLotDataTable.Columns.Add("INPUT_LOTID", typeof(string));

                        DataRow inPLotDataRow = null;
                        inPLotDataRow = inPLotDataTable.NewRow();
                        inPLotDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(EQUIPMENT_COMBO.SelectedValue.ToString());
                        inPLotDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        inPLotDataRow["INPUT_LOTID"] = _LOTIDPR;
                        inPLotDataTable.Rows.Add(inPLotDataRow);

                        DataTable inLotDetail = inDataSet.Tables.Add("IN_LOT");
                        inLotDetail.Columns.Add("LOTID", typeof(string));
                        inLotDetail.Columns.Add("INPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("RESNQTY", typeof(decimal));
                        inLotDetail.Columns.Add("WIPNOTE", typeof(string));

                        string sLotID = string.Empty;
                        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                        {
                            DataRow inLotDetailDataRow = null;
                            inLotDetailDataRow = inLotDetail.NewRow();

                            sLotID = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID"));
                            inLotDetailDataRow["LOTID"] = sLotID;
                            inLotDetailDataRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY"));
                            inLotDetailDataRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY"));
                            inLotDetailDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY"));
                            inLotDetailDataRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[i - 1].DataItem, "LOTID"))];
                            inLotDetail.Rows.Add(inLotDetailDataRow);
                        }
                        #endregion CONFIRM MESSAGE

                        new ClientProxy().ExecuteService_Multi(GetEndLotBizRuleName(), "INDATA,IN_INPUT,IN_LOT,IN_OUTLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            isConfirm = true;
                            REFRESH = true;

                        }, inDataSet);
                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            else if (procId.Equals(Process.ROLL_PRESSING)) 
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE                        
                        if (bool.Parse(finalCutInfo[2]) == true)
                        {
                            if (SetLossLot(WIPREASON_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                return;

                            SaveDefect(WIPREASON_GRID);

                            if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                            {
                                if (SetLossLot(WIPREASON2_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                    return;

                                SaveDefect(WIPREASON2_GRID);
                            }
                            isChangeWipReason = false;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("PROD_VER_CODE", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WIPNOTE", typeof(string));
                        inData.Columns.Add("CUTYN", typeof(string));
                        inData.Columns.Add("REMAINQTY", typeof(string));
                        inData.Columns.Add("REPROC_YN", typeof(string));
                        inData.Columns.Add("LANE_QTY", typeof(string));
                        inData.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inData.Columns.Add("CALDATE", typeof(DateTime));
                        inData.Columns.Add("HOLD_YN", typeof(string));   // # RollMap Hold 처리

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = _EQPTID;
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_VER_CODE"] = txtVersion.Text;
                        row["SHIFT"] = txtShift.Tag;
                        row["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        row["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        row["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        //row["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];

                        // 특이사항 저장 오류 수정 [2019-04-24]
                        if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                            row["WIPNOTE"] = postremarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];
                        else
                            row["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))];

                        row["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        row["REMAINQTY"] = finalCutInfo[6];
                        if (chkExtraPress.IsChecked == true)
                            row["REPROC_YN"] = "Y";
                        else
                            row["REPROC_YN"] = "N";
                        row["LANE_QTY"] = txtLaneQty.Value;
                        row["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                        row["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        row["HOLD_YN"] = _HOLDYN_CHK;   // # RollMap Hold 처리
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable input = indataSet.Tables.Add("IN_INPUT");
                        input.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        input.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        input.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _EQPTID;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");  //MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        row = input.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["INPUT_LOTID"] = DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "LOTID_PR");
                        input.Rows.Add(row);

                        DataTable InLot = indataSet.Tables.Add("INLOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(string));
                        InLot.Columns.Add("OUTPUTQTY", typeof(string));
                        InLot.Columns.Add("RESNQTY", typeof(string));

                        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                        {
                            row = InLot.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID"));
                            row["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY"));
                            row["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY"));
                            row["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOSSQTY"));
                            indataSet.Tables["INLOT"].Rows.Add(row);
                        }
                        #endregion CONFIRM MESSAGE 

                        string bizRuleName = "BR_PRD_REG_END_LOT_RP_UI";
                        string inDataTableName = "INDATA,IN_INPUT,IN_LOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RP_UI", "INDATA,IN_INPUT,IN_LOT", null, (sResult, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, indataSet);
                        }

                        
                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            else if (procId.Equals(Process.SLITTING))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult, isHold) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE                        
                        if (bool.Parse(finalCutInfo[2]) == true)
                        {
                            if (SetLossLot(WIPREASON_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                return;

                            SaveDefect(WIPREASON_GRID);

                            if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                            {
                                if (SetLossLot(WIPREASON2_GRID, ITEM_CODE_LEN_LACK, Util.NVC_Decimal(finalCutInfo[5])) == false)
                                    return;

                                SaveDefect(WIPREASON2_GRID);
                            }
                            isChangeWipReason = false;
                        }

                        // SLITTER 실적 확정 CUT기준으로 변경 ( 2017-01-21 ) CR-53
                        DataSet inDataSet = null;
                        inDataSet = new DataSet();

                        DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                        inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inLotDataTable.Columns.Add("IFMODE", typeof(string));
                        inLotDataTable.Columns.Add("EQPTID", typeof(string));
                        inLotDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inLotDataTable.Columns.Add("SHIFT", typeof(string));
                        inLotDataTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inLotDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inLotDataTable.Columns.Add("WRK_USERID", typeof(string));
                        inLotDataTable.Columns.Add("USERID", typeof(string));
                        inLotDataTable.Columns.Add("LANE_QTY", typeof(decimal));
                        inLotDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                        inLotDataTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow inLotDataRow = null;
                        inLotDataRow = inLotDataTable.NewRow();
                        inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inLotDataRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        inLotDataRow["PROD_VER_CODE"] = txtVersion.Text;
                        inLotDataRow["SHIFT"] = txtShift.Tag;
                        inLotDataRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        inLotDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text.ToString());
                        inLotDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag.ToString());
                        inLotDataRow["USERID"] = LoginInfo.USERID;
                        inLotDataRow["LANE_QTY"] = 1;
                        inLotDataRow["LANE_PTN_QTY"] = 1;
                        inLotDataRow["CALDATE"] = Convert.ToDateTime(txtWorkDate.Tag);
                        inLotDataTable.Rows.Add(inLotDataRow);

                        DataTable inLotDetail = inDataSet.Tables.Add("IN_CUTLOT");
                        inLotDetail.Columns.Add("CUT_ID", typeof(string));
                        inLotDetail.Columns.Add("INPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("LABEL_PASS_HOLD_FLAG", typeof(string));

                        DataRow inLotDetailDataRow = null;
                        inLotDetailDataRow = inLotDetail.NewRow();
                        inLotDetailDataRow["CUT_ID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "CUT_ID"));
                        if (bool.Parse(finalCutInfo[2]) == true)
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) + Convert.ToDouble(finalCutInfo[5]);
                        else
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDetailDataRow["LABEL_PASS_HOLD_FLAG"] = string.IsNullOrEmpty(_LABEL_PASS_HOLD_FLAG) ? "Y" : _LABEL_PASS_HOLD_FLAG;
                        inLotDetail.Rows.Add(inLotDetailDataRow);

                        DataTable inPLotDataTable = inDataSet.Tables.Add("IN_PRLOT");
                        inPLotDataTable.Columns.Add("LOTID", typeof(string));
                        inPLotDataTable.Columns.Add("OUTQTY", typeof(string));
                        inPLotDataTable.Columns.Add("CUTYN", typeof(string));

                        DataRow inPLotDataRow = null;
                        inPLotDataRow = inPLotDataTable.NewRow();

                        DataRowView prodRowView = PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem as DataRowView;

                        inPLotDataRow["LOTID"] = _LOTIDPR;
                        inPLotDataRow["OUTQTY"] = finalCutInfo[6];
                        inPLotDataRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부                        
                        inPLotDataTable.Rows.Add(inPLotDataRow);

                        DataTable inMtrlDataTable = inDataSet.Tables.Add("IN_INPUT");
                        inMtrlDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inMtrlDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inMtrlDataTable.Columns.Add("INPUT_LOTID", typeof(string));

                        DataRow inMtrlDataRow = null;
                        inMtrlDataRow = inMtrlDataTable.NewRow();
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(Util.NVC(EQUIPMENT_COMBO.SelectedValue));
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        inMtrlDataRow["INPUT_LOTID"] = _LOTIDPR;
                        inMtrlDataTable.Rows.Add(inMtrlDataRow);

                        DataTable inRemartTable = inDataSet.Tables.Add("IN_OUTLOT");
                        inRemartTable.Columns.Add("OUT_LOTID", typeof(string));
                        inRemartTable.Columns.Add("WIPNOTE", typeof(string));

                        DataTable dt = ((DataView)REMARK_GRID.ItemsSource).Table;
                        DataRow row = null;
                        for (int i = 1; i < dt.Rows.Count; i++)
                        {
                            row = inRemartTable.NewRow();
                            row["OUT_LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                            //row["WIPNOTE"] = Util.NVC(_iRow["REMARK"]);
                            // 특이사항 저장 오류 수정 [2019-04-24]
                            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                                row["WIPNOTE"] = postremarkInfo[Util.NVC(dt.Rows[i]["LOTID"])];
                            else
                                row["WIPNOTE"] = remarkInfo[Util.NVC(dt.Rows[i]["LOTID"])];
                            inDataSet.Tables["IN_OUTLOT"].Rows.Add(row);
                        }
                        #endregion CONFIRM MESSAGE

                        string bizRuleName = "BR_PRD_REG_END_LOT_RP_UI";
                        string inDataTableName = "INDATA,IN_INPUT,IN_LOT";

                        DataTable dtPermitRateOverData = GetPermitRateOverData(); // 허용비율 초과목록 
                        if (dtPermitRateOverData.Rows.Count > 0)
                        {
                            SetPermitRateOverHis(dtPermitRateOverData, inDataSet, bizRuleName, inDataTableName);
                        }
                        else
                        {

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CUTID_SL", "INDATA,IN_CUTLOT,IN_PRLOT,IN_INPUT,IN_OUTLOT", null, (result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                isConfirm = true;
                                REFRESH = true;

                            }, inDataSet);
                        }
                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
        }

        #region [POSTACTION]
        private DataTable getPostAction(string CUT_ID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUT_ID", typeof(string));
                //IndataTable.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CUT_ID"] = CUT_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_POST_HOLD_ELTR", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private DataTable getSavedPostAction(string CUT_ID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("CUT_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["CUT_ID"] = CUT_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_POST_HOLD_CUT_ID", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        string GetEqptCurrentMtrl(string sEqptID)
        {
            try
            {
                string MountMTRL = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                    return "";

                MountMTRL = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                return MountMTRL;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void DefectVisibleLV(DataTable dt, int LV, bool chk)
        {
            if (LV == 1)
            {
                DefectVisibleLV1(dt, chk);
            }
            else if (LV == 2)
            {
                DefectVisibleLV2(dt, chk);
            }
            else if (LV == 3)
            {
                DefectVisibleLV3(dt, chk);
            }
        }
        private void DefectVisibleLVAll()
        {
            DataTable dt = (WIPREASON_GRID.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
            }

            if (procId == Process.COATING)
            {
                DataTable dt2 = (WIPREASON2_GRID.ItemsSource as DataView).Table;

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                }
            }
        }
        private void DefectVisibleLV1(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        private void DefectVisibleLV2(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV3(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            WIPREASON_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            WIPREASON2_GRID.Rows[i].Visibility == Visibility.Visible)
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            WIPREASON2_GRID.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }

        }

        private void ClearDefectLV()
        {
            if ((grdDataCollect.Children[0] as UcDataCollect).chkDefectFilter.IsChecked == true)
            {
                isDefectLevel = true;
                OnClickDefetectFilter((grdDataCollect.Children[0] as UcDataCollect).chkDefectFilter, null);
                isDefectLevel = false;
            }
        }


        #region # Roll Map
        private void SetRollMapEquipment()
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(Util.NVC(EQUIPMENT_COMBO.SelectedValue));
            _isOriginRollMapEquipment = _isRollMapEquipment;

            // # Roll Map 대상설비에 따른 컨트롤 정의
            VisibleRollMapMode();

            // 롤맵 설비는 롤 방향 Tab 안보이도록 해달라는 요청 반영 [2021-10-18]
            if (_isRollMapEquipment)
                (grdDataCollect.Children[0] as UcDataCollect).tiRollDirection.Visibility = Visibility.Collapsed;
            else
                (grdDataCollect.Children[0] as UcDataCollect).tiRollDirection.Visibility = Visibility.Visible;
        }

        private bool IsEquipmentAttr(string equipmentCode)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(equipmentCode).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return false;
        }

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = procId;
                dr["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                    if (Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return false;
        }

        private void SetRollMapLotAttribute(string lotId)
        {
            try
            {
                bool isRollMapModeChange = false;

                if (_isOriginRollMapEquipment == true)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    DataRow row = dt.NewRow();
                    row["LOTID"] = lotId;
                    dt.Rows.Add(row);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                    if (CommonVerify.HasTableRow(result))
                    {
                        // 실적 연계 여부에 따라 처리 변경 [2021-10-18]
                        if (_isRollMapResultLink == true)
                        {
                            if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                            {
                                if (!_isRollMapEquipment) isRollMapModeChange = true;
                                _isRollMapEquipment = true;
                            }
                            else
                            {
                                if (_isRollMapEquipment) isRollMapModeChange = true;
                                _isRollMapEquipment = false;
                            }

                            if (isRollMapModeChange)
                                VisibleRollMapMode();
                        }
                        else
                        {
                            _isRollMapEquipment = false;
                            VisibleRollMapMode();

                            if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                                (grdSearch.Children[0] as UcSearch).btnRollMap.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEquipmentCode(string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable GetRollMapInputLotCode(string lotId)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = 1;
            dr["EQPT_MEASR_PSTN_ID"] = "UW";
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_COLLECT_INFO", "RQSTDT", "RSLTDT", inTable);
        }

        private void VisibleRollMapMode()
        {

            if (_isRollMapEquipment)
            {
                (grdSearch.Children[0] as UcSearch).btnRollMap.Visibility = Visibility.Visible;

                if (string.Equals(procId, Process.COATING))
                {
                    (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;  // 숨김처리 06.06
                    (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["STRT_PSTN"].Visibility = Visibility.Collapsed;   // 숨김처리 06.06
                    (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["END_PSTN"].Visibility = Visibility.Collapsed;    // 숨김처리 06.06
                    (grdDataCollect.Children[0] as UcDataCollect).tiInputMaterialHistory.Visibility = Visibility.Collapsed;

                    (grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial.Visibility = Visibility.Collapsed;
                    (grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial.Visibility = Visibility.Collapsed;
                    (grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial.Visibility = Visibility.Collapsed;
                    (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.IsReadOnly = true;
                    (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["CHK"].Visibility = Visibility.Collapsed;

                    (grdCommand.Children[0] as UcCommand).btnRollMapInputMaterial.Visibility = Visibility.Visible; // 2021.08.10 Visible 처리

                }
                else if (string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING))
                {
                    (grdDataCollect.Children[0] as UcDataCollect).tiInputMaterialHistory.Visibility = Visibility.Visible;
                }
            }
            else
            {
                (grdSearch.Children[0] as UcSearch).btnRollMap.Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["STRT_PSTN"].Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["END_PSTN"].Visibility = Visibility.Collapsed;
                (grdDataCollect.Children[0] as UcDataCollect).btnAddMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).btnDeleteMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).btnSaveMaterial.Visibility = Visibility.Visible;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.IsReadOnly = false;
                (grdDataCollect.Children[0] as UcDataCollect).dgMaterial.Columns["CHK"].Visibility = Visibility.Visible;

                (grdDataCollect.Children[0] as UcDataCollect).tiInputMaterialHistory.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveDefectForRollMap()
        {
            string bizRuleName = string.Equals(procId, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);

            DataTable IndataTable = inDataSet.Tables.Add("IN_LOT");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));

            inDataRow = IndataTable.NewRow();
            inDataRow["LOTID"] = _LOTID;
            inDataRow["WIPSEQ"] = _WIPSEQ;
            IndataTable.Rows.Add(inDataRow);

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetRollMapHold(string processCode)
        {
            string bizRuleName = string.Equals(processCode, Process.ROLL_PRESSING) ? "DA_PRD_SEL_ROLLMAP_RP_HOLD" : "DA_PRD_SEL_ROLLMAP_CT_HOLD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));          

            decimal dcmWIPSEQ;
            decimal.TryParse(_WIPSEQ, out dcmWIPSEQ);
            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = _LOTID;
            dr["WIPSEQ"] = dcmWIPSEQ;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private bool IsRollMapEquipment()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_LOTATTR";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow row = inTable.NewRow();
                row["LOTID"] = _LOTID;
                inTable.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
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
            catch (Exception)
            {
                return false;
            }
        }
        #endregion


        #endregion Methods

        #region AD 공통 체크 로직 추가
        // AD 인증 체크 기능 [2019-08-21]
        private DataTable GetConfirmAuthVaildation()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
            Indata["PROCID"] = procId;
            Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CNFM_AUTH", "INDATA", "RSLTDT", IndataTable);

            if (dt != null && dt.Rows.Count > 0)
            {
                // 인증 여부
                bool isAuthConfirm = true;

                // Input용 데이터 테이블 ( INPUTVALUE : 비교할 대상 값, CHK_VALUE1 : SPEC1, CHK_VALUE2 : SPEC2 )
                DataTable inputTable = new DataTable();
                inputTable.Columns.Add("CHK_VALUE1", typeof(decimal));
                inputTable.Columns.Add("CHK_VALUE2", typeof(decimal));
                inputTable.Columns.Add("INPUTVALUE", typeof(decimal));

                foreach (DataRow row in dt.Rows)
                {
                    inputTable.Clear();

                    if (!string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE1"])) || !string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE2"])))
                    {
                        DataRow dataRow = inputTable.NewRow();
                        dataRow["CHK_VALUE1"] = row["CHK_VALUE1"];
                        dataRow["CHK_VALUE2"] = row["CHK_VALUE2"];
                        inputTable.Rows.Add(dataRow);
                    }

                    if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_GOOD_QTY_LIMIT"))
                    {
                        // 양품량 기준 체크
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY"));
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);

                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_PROD_QTY_LIMIT"))
                    {
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_SPEC_LIMIT"))
                    {
                        isAuthConfirm = ValidQualitySpec("Auth");
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_AVG_LIMIT"))
                    {
                        // 품질평균값 제한 체크

                    }

                    // 인증이 필요한 경우 전체 정보 전달
                    if (isAuthConfirm == false)
                    {
                        DataTable outTable = dt.Clone();
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //outTable.Rows.Add(row.ItemArray);
                        outTable.AddDataRow(row);
                        return outTable;
                    }
                }
            }
            return new DataTable();
        }

        private bool CheckLimitValue(string sCheckType, DataTable inputTable)
        {
            foreach (DataRow row in inputTable.Rows)
            {
                switch (sCheckType)
                {
                    case "LOWER":           // SPEC LOWER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "UPPER":           // SPEC UPPER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "BOTH":            // SPEC IN
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;

                    case "NOT_BOTH":        // SPEC OUT
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;
                    case "VALUE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) == Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "NOT_VAULE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) != Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    default:
                        break;
                }
            }
            return true;
        }
        #endregion

        #region [Defect / DataCollect / Material / Slurry  / Wip Note ]

        #region Defect
        protected virtual void OnDataCollectGridCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count)
            {
                C1DataGrid caller = sender as C1DataGrid;

                if (ValidateDefect(sender as C1DataGrid))
                {
                    //2020-07-02 오화백 
                    //폴란드 롤프레스 공정 태그수 입력 시 TAG_CONV_RATE 컬럼 값을 곱하여 수량에 자동 입력.
                    //(수량 = 태그수 * TAG_CONV_RATE)

                    //2021-04-15 김대근
                    //위치만 변경. 불량태그수 입력 후 반영된 불량수량이 양품량 계산에 반영되지 않기에 단순 위치 변경
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                    {
                        if (e.Cell.Column.Name.Equals("DFCT_TAG_QTY"))
                        {
                            string sTagQty = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY"));
                            string sTagRate = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAG_CONV_RATE"));

                            double dTagQty = 0;
                            double dTagRate = 0;

                            double.TryParse(sTagQty, out dTagQty);
                            double.TryParse(sTagRate, out dTagRate);
                            DataTableConverter.SetValue(caller.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", dTagQty * dTagRate);
                            caller.UpdateLayout();
                        }
                    }

                    if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1)
                    {
                        DefectChange(caller, caller.CurrentCell.Row.Index, caller.CurrentCell.Column.Index);
                    }
                    else
                    {
                        // 청주1동 특화 M -> P 변환 로직 [2017-05-15]
                        if (string.Equals(caller.CurrentCell.Column.Name, "CONVRESNQTY") && string.Equals(txtUnit.Text, "EA"))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY",
                                Convert.ToDouble(GetIntFormatted(Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CONVRESNQTY")) / convRate)));

                        if (_isRollMapEquipment && (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)))
                        {
                            SetWipReasonCommittedEdit(sender, e);
                        }

                        GetSumDefectQty();
                    }

                    switch (caller.Name)
                    {
                        case "dgWipReason":
                        case "dgWipReason2":
                            isChangeWipReason = true;
                            break;

                        case "dgQuality":
                            isChangeQuality = true;
                            break;

                        case "dgRemark":
                            isChangeRemark = true;
                            break;
                    }

                    SetExceedLength();
                    LOTINFO_GRID.Refresh(false);
                    _LOSSQTY = string.Empty;
                    _GOODQTY = string.Empty;
                    _CHARGEQTY = string.Empty;
                }
            }
        }

        private void SetWipReasonCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    // 전체 체크가 없어서 주석 처리
                    /* for (int i = 0; i < dataGrid.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                            if (e.Cell.Row.Index != i)
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    } */

                    // 재 조건 조정 배분 로직 추가 [2021-07-27]
                    DataRow[] row = DataTableConverter.Convert(dataGrid.ItemsSource).Select("DFCT_CODE='" +
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_CODE")) + "' AND PRCS_ITEM_CODE='GRP_QTY_DIST'");

                    if (row.Length > 0)
                    {
                        decimal iCurrQty = 0;
                        decimal iResQty = 0;
                        decimal iInitQty = row.Sum(g => g.Field<decimal>("FRST_AUTO_RSLT_RESNQTY"));

                        decimal iDistQty = DataTableConverter.Convert(dataGrid.ItemsSource).AsEnumerable()
                                                .Where(g => g.Field<string>("DFCT_CODE") == Util.NVC(row[0]["DFCT_CODE"]) &&
                                                                   g.Field<string>("PRCS_ITEM_CODE") != "GRP_QTY_DIST" &&
                                                                   g.Field<string>("RESNCODE") != Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE")))
                                                .Sum(g => Util.NVC_Decimal(g.Field<string>("RESNQTY")));

                        if (iInitQty < (iDistQty + Util.NVC_Decimal(e.Cell.Value)))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", iInitQty - iDistQty);

                        for (int i = 0; i < dataGrid.Rows.Count; i++)
                        {
                            iCurrQty = 0;
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "DFCT_CODE"), row[0]["DFCT_CODE"]) &&
                                string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST"))
                            {
                                iCurrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                if (iCurrQty <= (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty))
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                    iResQty += iCurrQty;
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", iCurrQty - (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty));
                                    iResQty = iDistQty + Util.NVC_Decimal(e.Cell.Value);
                                }
                            }
                        }
                    }

                    #region # Coater Back

                    #endregion

                    #region # RollPress

                    #endregion
                }

                dataGrid.EndEdit();
            }
        }

        protected virtual void OnDataCollectGridBeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                {
                    e.Cancel = true;
                }
                if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                {
                    if (dataGrid.Columns["ALL"].Index < e.Column.Index && dataGrid.Columns["COSTCENTERID"].Index > e.Column.Index)
                    {
                        if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK") ||
                            string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                        {
                            e.Cancel = true;
                            dataGrid.BeginEdit(e.Row.Index, dataGrid.Columns["ALL"].Index);
                        }
                    }
                }
            }
        }

        #region[POSTACTION]
        protected virtual void OnDataCollectGridPostHoldCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null)
                isChangePostAction = true;

            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell.Column.Name.Contains("POST_HOLD"))
                {
                    if (e.Cell.Row.Index.Equals(0))
                    {
                        DataTable dt = ((DataView)POSTHOLD_GRID.ItemsSource).Table;
                        DataRow dr = dt.Select().FirstOrDefault();
                        string sColumnName = e.Cell.Column.Name;
                        bool hold = Convert.ToBoolean(dr[sColumnName]);
                        if (dr != null)
                        {
                            foreach (DataRow dRow in dt.Rows)
                            {
                                dRow[sColumnName] = hold;
                            }
                        }
                        Util.GridSetData(POSTHOLD_GRID, dt, FrameOperation);
                    }
                }
            }
        }
        #endregion

        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));
                IndataTable.Columns.Add("CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;

                if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                    Indata["CODE"] = "BAS";

                IndataTable.Rows.Add(Indata);

                //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region Quality
        void GetQualityList(object SelectedItem)
        {
            try
            {
                DataTable _topDT = new DataTable();
                DataTable _backDT = new DataTable();

                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(QUALITY_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = procId;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
                Indata["CLCT_PONT_CODE"] = procId.Equals(Process.COATING) || procId.Equals(Process.INS_COATING) ? "T" : null;

                if (!string.IsNullOrEmpty(txtVersion.Text))
                    Indata["VER_CODE"] = txtVersion.Text;

                if (_LANEQTY != null && Convert.ToInt16(_LANEQTY) > 0)
                    Indata["LANEQTY"] = _LANEQTY;
                IndataTable.Rows.Add(Indata);

                _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                if (_topDT.Rows.Count == 0)
                {
                    _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(QUALITY_GRID, _topDT, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(QUALITY_GRID, _topDT, FrameOperation, true);
                }
                _Util.SetDataGridMergeExtensionCol(QUALITY_GRID, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);

                if (QUALITY2_GRID.Visibility == Visibility.Visible)
                {
                    Util.gridClear(QUALITY2_GRID);

                    DataTable backTable = new DataTable();
                    backTable.Columns.Add("LANGID", typeof(string));
                    backTable.Columns.Add("AREAID", typeof(string));
                    backTable.Columns.Add("PROCID", typeof(string));
                    backTable.Columns.Add("LOTID", typeof(string));
                    backTable.Columns.Add("WIPSEQ", typeof(string));
                    backTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    backTable.Columns.Add("VER_CODE", typeof(string));
                    backTable.Columns.Add("LANEQTY", typeof(Int16));

                    DataRow backdata = backTable.NewRow();
                    backdata["LANGID"] = LoginInfo.LANGID;
                    backdata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    backdata["PROCID"] = procId;
                    backdata["LOTID"] = rowview["LOTID"].ToString();
                    backdata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
                    backdata["CLCT_PONT_CODE"] = procId.Equals(Process.COATING) || procId.Equals(Process.INS_COATING) ? "B" : null;

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                        backdata["VER_CODE"] = txtVersion.Text;

                    if (_LANEQTY != null && Convert.ToInt16(_LANEQTY) > 0)
                        backdata["LANEQTY"] = _LANEQTY;

                    backTable.Rows.Add(backdata);

                    _backDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", backTable);

                    if (_backDT.Rows.Count == 0)
                    {
                        _backDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", backTable);
                    }

                    if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.INS_COATING) || string.Equals(procId, Process.SRS_COATING))
                    {
                        if (string.Equals(QUALITY2_GRID.Columns[QUALITY2_GRID.Columns.Count - 1].Name, "CLCTVAL02"))
                            QUALITY2_GRID.Columns.RemoveAt(QUALITY2_GRID.Columns.Count - 1);

                        _backDT.Columns.Add("CLCTVAL02", typeof(double));
                        Util.SetGridColumnNumeric(QUALITY2_GRID, "CLCTVAL02", null, ObjectDic.Instance.GetObjectName("입력값"), true, true, true, false, -1, HorizontalAlignment.Center, Visibility.Visible, GetUnitFormatted(), false, false);
                        QUALITY2_GRID.LoadedCellPresenter += OnLoaded2CellPresenter;

                    }
                    Util.GridSetData(QUALITY2_GRID, _backDT, FrameOperation, true);
                    _Util.SetDataGridMergeExtensionCol(QUALITY2_GRID, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        protected virtual void OnLoaded2CellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.INS_COATING) || string.Equals(procId, Process.SRS_COATING))
            {
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                string nCLCTITEM = DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CLCTITEM").ToString().Split('-')[0] + "-" +
                    DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CLCTITEM").ToString().Split('-')[1];

                if (string.Equals(e.Cell.Column.Name, "CLCTVAL02") && !nCLCTITEM.Equals("E2000-0001") || nCLCTITEM.Equals("E2000-0002"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                }
                else if (string.Equals(e.Cell.Column.Name, "CLCTVAL02") && !(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CLCTITEM").ToString().Split('-')[0].Equals("SI016") || DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CLCTITEM").ToString().Split('-')[0].Equals("SI017")))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        //20180405 조용수 면상태일지
        private void GetCottonList(object SelectedItem)
        {
            DataTable _cottonDT = new DataTable();

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(COTTON_GRID);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("CUT_ID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["CUT_ID"] = rowview["CUT_ID"].ToString();
            Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
            IndataTable.Rows.Add(Indata);

            _cottonDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_CHK_DATA", "INDATA", "RSLTDT", IndataTable);
            Util.GridSetData(COTTON_GRID, _cottonDT, FrameOperation, true);
        }

        private void GetColorList(object SelectedItem)
        {
            try
            {
                DataTable _colorDT = new DataTable();

                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(COLOR_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                Indata["PROCID"] = procId;
                Indata["EQPTID"] = null;
                Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
                IndataTable.Rows.Add(Indata);

                _colorDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "RSLTDT", IndataTable);

                if (_colorDT.Rows.Count == 0)
                {
                    Indata["EQPTID"] = _EQPTID;

                    _colorDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(COLOR_GRID, _colorDT, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(COLOR_GRID, _colorDT, FrameOperation, true);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetDefectTagList(string sLotID)
        {
            try
            {
                if (string.IsNullOrEmpty(sLotID))
                    return;

                Util.gridClear(DEFECT_TAG_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = sLotID;
                Indata["EQPTID"] = null;
                Indata["PROCID"] = procId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_DEFECT_TAG_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    Indata["LOTID"] = _LOTIDPR;

                    dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_DEFECT_TAG_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (dt.Rows.Count == 0)
                    {
                        Indata["EQPTID"] = _EQPTID;

                        dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_DEFECT_TAG", "INDATA", "RSLTDT", IndataTable);
                        Util.GridSetData(DEFECT_TAG_GRID, dt, FrameOperation, true);
                    }
                    else
                    {
                        Util.GridSetData(DEFECT_TAG_GRID, dt, FrameOperation, true);
                    }
                }
                else
                {
                    Util.GridSetData(DEFECT_TAG_GRID, dt, FrameOperation, true);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 장착 자재 FOIL, SLURRY 정보 조회 (ACTIVE CORE는 FOIL의 RADIO BUTTON으로 표시)
        private void GetCurrentMount(string sEqptID, string sCoatSide)
        {
            try
            {
                txtCore1.Text = string.Empty;
                txtCore1.Tag = null;
                txtCore2.Text = string.Empty;
                txtCore2.Tag = null;
                txtSlurry1.Text = string.Empty;
                txtSlurry1.Tag = null;
                txtSlurry2.Text = string.Empty;
                txtSlurry2.Tag = null;

                DataTable dtMain = GetCurrentMount(sEqptID);
                Grid grid = null;

                if (dtMain.Rows.Count > 0)
                {
                    int iIdx = 0;

                    // SET FOIL
                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'"))
                    {
                        if (iIdx == 0)
                        {
                            txtCore1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                            txtCore1.Tag = "DUMMY";

                            grid = txtCore1.Parent as Grid;
                            if (string.Equals(_iRow["INPUT_STATE_CODE"], "A"))
                                ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked = true;
                            else
                                ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked = false;

                            iIdx++;
                        }
                        else if (iIdx == 1)
                        {
                            txtCore2.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                            txtCore2.Tag = "DUMMY";

                            grid = txtCore2.Parent as Grid;
                            if (string.Equals(_iRow["INPUT_STATE_CODE"], "A"))
                                ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked = true;
                            else
                                ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked = false;

                            break;
                        }
                    }

                    // SET SLURRY
                    iIdx = 0;

                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE = 'ASL'"))
                    {
                        if (iIdx == 0)
                        {
                            txtSlurry1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                            txtSlurry1.Tag = Convert.ToString(_iRow["MTRLID"]);

                            iIdx++;
                        }
                        else if (iIdx == 1)
                        {
                            txtSlurry2.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                            txtSlurry2.Tag = Convert.ToString(_iRow["MTRLID"]);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetCurrentMount(string sEqptID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                if (isSingleCoater)
                    IndataTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEqptID;

                if (isSingleCoater)
                    Indata["COATING_SIDE_TYPE_CODE"] = null;

                IndataTable.Rows.Add(Indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "INDATA", "RSLTDT", IndataTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private void SetMountChange()
        {
            // DATA SET
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 자재를 변경하시겠습니까?"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    SaveMountChange();
                }
            });
        }

        private void SaveMountChange(bool IsCurrentFoil = true, bool IsSlurryTerm = false, bool IsCoreTerm = false, bool IsTopSlurryChange = true, bool IsBackSlurryChange = true,
            bool IsAcoreChange = true, bool IsBcoreChange = true)
        {
            DataTable mountDt = GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue));

            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("INPUT_LOT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("MTRLID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("TERM_FLAG", typeof(string));

            DataTable inTable = indataSet.Tables["INDATA"];
            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
            newRow["USERID"] = LoginInfo.USERID;

            inTable.Rows.Add(newRow);
            Grid grid = null;

            DataTable inMaterial = indataSet.Tables["INPUT_LOT"];

            // PSTN ID QUERY해서 변경하여 변경된것만 체크 후 저장하게 변경 ( 2017-01-23 )
            DataRow[] rows = mountDt.Copy().Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2987", new object[] { EQUIPMENT_COMBO.SelectedValue });  //해당 설비({%1})의 등록된 Foil정보가 존재하지 않습니다.
                return;
            }

            // SET CORE
            if (txtCore1.Visibility == Visibility.Visible && rows.Length > 0 && IsCurrentFoil == true && IsAcoreChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                grid = txtCore1.Parent as Grid;

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked == true ? !string.IsNullOrEmpty(txtCore1.Text.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = txtCore1.Tag;
                newRow["INPUT_LOTID"] = txtCore1.Text;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;
                inMaterial.Rows.Add(newRow);
            }

            if (txtCore2.Visibility == Visibility.Visible && rows.Length > 1 && IsCurrentFoil == true && IsBcoreChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                grid = txtCore2.Parent as Grid;

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = ((RadioButton)grid.Children[grid.Children.Count - 1]).IsChecked == true ? !string.IsNullOrEmpty(txtCore2.Text.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = txtCore2.Tag;
                newRow["INPUT_LOTID"] = txtCore2.Text;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            // SET SLURRY
            rows = null;
            rows = mountDt.Copy().Select("PRDT_CLSS_CODE = 'ASL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2988", new object[] { EQUIPMENT_COMBO.SelectedValue });  //해당 설비({%1})의 등록된 Slurry정보가 존재하지 않습니다.
                return;
            }

            if (txtSlurry1.Visibility == Visibility.Visible && rows.Length > 0 && IsTopSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(txtSlurry1.Text) ? "A" : "S";
                newRow["MTRLID"] = txtSlurry1.Tag;
                newRow["INPUT_LOTID"] = txtSlurry1.Text;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (txtSlurry2.Visibility == Visibility.Visible && rows.Length > 1 && IsBackSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(txtSlurry2.Text) ? "A" : "S";
                newRow["MTRLID"] = txtSlurry2.Tag;
                newRow["INPUT_LOTID"] = txtSlurry2.Text;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (inMaterial.Rows.Count == 0)
                return;

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_USE_MTRL_LOT_CT", "INDATA,INPUT_LOT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                if (isSingleCoater)
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), Util.NVC(COATTYPE_COMBO.SelectedValue));
                else
                    GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue), "");

            }, indataSet);
        }

        private DataTable GetProcessVersion(string sLotID, string sModlID)
        {
            // VERSION, LANE수를 룰에 따라 가져옴 [2017-02-17]
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MODLID", typeof(string));
                inTable.Columns.Add("PROCSTATE", typeof(string));
                inTable.Columns.Add("TOPLOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                indata["PROCID"] = procId;
                indata["EQPTID"] = _EQPTID;
                indata["LOTID"] = sLotID;
                indata["MODLID"] = sModlID;

                if (isCoaterAfterProcess)
                    indata["PROCSTATE"] = "Y";

                if (string.Equals(procId, Process.COATING) && isSingleCoater && string.Equals(COATTYPE_COMBO.SelectedValue, "B"))
                {
                    DataTable mountDt = GetCurrentMount(Util.NVC(EQUIPMENT_COMBO.SelectedValue));

                    if (mountDt.Rows.Count > 0)
                    {
                        DataRow[] rows = mountDt.Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL' AND INPUT_STATE_CODE = 'A'");
                        if (rows.Length > 0)
                            indata["TOPLOTID"] = Util.NVC(rows[0]["INPUT_LOTID"]);
                    }
                }

                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private string GetCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return Util.NVC(row["ATTRIBUTE1"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private bool GetCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
        {
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
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID)
        {
            try
            {
                // CSR : C20220322-000662 - ESWA 전극 2동 롤프레스 GMES 공정 진척 Cancel Start 버튼 권한 부여 후 사용으로 변경 요청의 건
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = procId;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "CANCEL_LOTSTART_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("시작취소");
                    else if (sBtnGrpID == "CANCEL_EQPT_END_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("장비완료취소");
                    else if (sBtnGrpID == "CONFIRM_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("실적확정");
                    else if (sBtnGrpID == "EQPT_END_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("장비완료");

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region Material
        private void GetMaterial(string sLotID)
        {
            try
            {
                Util.gridClear(INPUTMTRL_GRID);

                string bizRuleName;
                if (_isRollMapEquipment && string.Equals(procId, Process.COATING))
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2_RM";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2";
                    SetGridComboItem(INPUTMTRL_GRID.Columns["MTRLID"], _WORKORDER);
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                if (_isRollMapEquipment && string.Equals(procId, Process.COATING))
                {
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                }

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                if (_isRollMapEquipment && string.Equals(procId, Process.COATING))
                {
                    Indata["EQPTID"] = _EQPTID;
                    Indata["WIPSEQ"] = _WIPSEQ;
                }
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(INPUTMTRL_GRID, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetMaterialSummary(string sLotID, string sWOID)
        {
            try
            {
                Util.gridClear(INPUTMTRLSUMMARY_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                Indata["WOID"] = sWOID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(INPUTMTRLSUMMARY_GRID, dtResult, FrameOperation, true);

                // 믹서공정은 투입자재 총사용량 = 생산량 [2017-03-02]
                double inputMtrlSumQty = 0;
                foreach (DataRow row in dtResult.Rows)
                {
                    inputMtrlSumQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));
                }

                if (inputMtrlSumQty > 0)
                {
                    txtInputQty.Value = inputMtrlSumQty;
                    SetInputQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        /// <summary>
        /// CWA - BindingSolution,CMC 투입자재 그리드용
        /// </summary>
        /// <param name="sLotID"></param>
        /// <param name="sWOID"></param>
        private void GetMaterialList(string sLotID, string sProdID)
        {
            try
            {
                Util.gridClear(INPUTMTRLSUMMARY_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                Indata["PRODID"] = sProdID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY_SOL", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(INPUTMTRLSUMMARY_GRID, dtResult, FrameOperation, true);

                // 믹서공정은 투입자재 총사용량 = 생산량 [2017-03-02]
                double inputMtrlSumQty = 0;
                foreach (DataRow row in dtResult.Rows)
                {
                    inputMtrlSumQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));
                }

                if (inputMtrlSumQty > 0)
                {
                    txtInputQty.Value = inputMtrlSumQty;
                    SetInputQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        //20170109 유관수 자재표시 규격 추가
        protected virtual void OnMaterialCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!INPUTMTRL_GRID.CurrentCell.IsEditing)
                {
                    if (INPUTMTRL_GRID.CurrentCell.Column.Name.Equals("MTRLID"))
                    {
                        string sMTRLNAME;
                        string vMTRLID = Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.CurrentRow.DataItem, "MTRLID"));

                        if (vMTRLID.Equals(""))
                        {
                            return;
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("MTRLID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["MTRLID"] = vMTRLID;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_MTRLDESC", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count > 0)
                            {
                                sMTRLNAME = result.Rows[0]["MTRLDESC"].ToString();
                                DataTableConverter.SetValue(INPUTMTRL_GRID.Rows[INPUTMTRL_GRID.SelectedIndex].DataItem, "MTRLDESC", sMTRLNAME);

                                DataTable dt2 = (INPUTMTRL_GRID.ItemsSource as DataView).Table;
                                Util.GridSetData(INPUTMTRL_GRID, dt2, FrameOperation, true);

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // LOSS 횟수 지정을 위해서 하단의 2개 이벤트 추가 (2017-01-09)
        protected virtual void OnDataGridBeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
            if (isResnCountUse == true && (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING)))
            {
                //if (string.Equals(dataGrid.Name, "dgLoss") || string.Equals(dataGrid.Name, "dgLoss2") ||
                //    string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "LOSS_LOT"))
                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "DEFECT_LOT") ||
                    string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "LOSS_LOT"))
                {
                    if (string.Equals(e.Column.Name, "COUNTQTY") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y") &&
                        string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "LINK_DETL_RSN_CODE_TYPE"))))
                        return;
                }

                if (string.Equals(e.Column.Name, "COUNTQTY"))
                    e.Cancel = true;
            }
            else if (isResnCountUse == true && (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING)))
            {
                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "DEFECT_LOT") ||
                    string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "LOSS_LOT"))
                {
                    if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y") &&
                        string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "LINK_DETL_RSN_CODE_TYPE"))))
                        return;
                }

                if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true)
                    e.Cancel = true;
            }
        }

        protected virtual void OnLoadedLotInfoCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (!string.Equals(procId, Process.PRE_MIXING) && !string.Equals(procId, Process.MIXING) && !string.Equals(procId, Process.SRS_MIXING))
                                {
                                    if (LOTINFO_GRID.GetRowCount() > 0)
                                    {
                                        if (e.Cell.Column.Visibility == Visibility.Visible)
                                        {
                                            TextBlock sContents = e.Cell.Presenter.Content as TextBlock;

                                            int iSourceIdx = e.Cell.Row.Index - (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) + LOTINFO_GRID.TopRows.Count;
                                            if (DataTableConverter.Convert(LOTINFO_GRID.ItemsSource).Columns.Contains(e.Cell.Column.Name))
                                            {

                                                string sValue = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[iSourceIdx].DataItem, e.Cell.Column.Name));

                                                if (string.Equals(e.Cell.Column.Name, "LANE_QTY"))
                                                {
                                                    sContents.Text = sValue;
                                                }
                                                else
                                                {
                                                    if (e.Cell.Column.GetType() == typeof(DataGridNumericColumn))
                                                        sContents.Text = GetUnitFormatted(Convert.ToDouble(Util.NVC_Decimal(string.IsNullOrEmpty(sValue) ? "0" : sValue) * convRate), "EA");
                                                    else
                                                        sContents.Text = sValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }
        protected virtual void OnLoadedDefectCellPresenter(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {

                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex1 = 0;
                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel1.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex2 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel2.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex3 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue((grdDataCollect.Children[0] as UcDataCollect).dgLevel3.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;
            }
        }
        protected virtual void OnLoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                                // 횟수 전 공정 추가로 수정 (C20190416_75868)  [2019-04-16]
                                if (isResnCountUse == true && (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING)))
                                {
                                    if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                                else if (isResnCountUse == true && (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING)))
                                {
                                    if (e.Cell.Column.Name.Length == 13 && e.Cell.Column.Name.Contains("NUM") == true && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                            }
                        }
                    }
                }));
            }
        }

        protected virtual void OnLoadedWipReasonCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    if (e.Cell.Column.Index == dataGrid.Columns["ACTNAME"].Index)
                    {
                        if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                            presenter.Content = ObjectDic.Instance.GetObjectName("불량수량");
                        else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                            presenter.Content = ObjectDic.Instance.GetObjectName("양품수량");
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["RESNTOTQTY"].Index)
                    {
                        if (LOTINFO_GRID.GetRowCount() > 0)
                            if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                                //    presenter.Content = GetUnitFormatted((Util.NVC_Decimal(presenter.Content) - (exceedLengthQty * LOTINFO_GRID.GetRowCount())));
                                presenter.Content = GetUnitFormatted((Convert.ToDecimal(presenter.Content) - (exceedLengthQty * LOTINFO_GRID.GetRowCount())));

                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                //  presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) * LOTINFO_GRID.GetRowCount()) - (Util.NVC_Decimal(presenter.Content) - (exceedLengthQty * LOTINFO_GRID.GetRowCount())));
                                presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) * LOTINFO_GRID.GetRowCount()) - (Convert.ToDecimal(presenter.Content) - (exceedLengthQty * LOTINFO_GRID.GetRowCount())));

                    }
                    else
                    {
                        if (LOTINFO_GRID.GetRowCount() > 0)
                        {
                            if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                                // presenter.Content = GetUnitFormatted(Util.NVC_Decimal(presenter.Content) - exceedLengthQty);
                                presenter.Content = GetUnitFormatted(Convert.ToDecimal(presenter.Content) - exceedLengthQty);
                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                //  presenter.Content = GetUnitFormatted(Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) - (Util.NVC_Decimal(presenter.Content) - exceedLengthQty));
                                presenter.Content = GetUnitFormatted(Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) - (Convert.ToDecimal(presenter.Content) - exceedLengthQty));
                        }
                    }
                }
            }
        }

        protected virtual void OnLoadedDgQualitynCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
                                string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));
                                string isEnable = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ISENABLE"));

                                if (panel != null)
                                {
                                    if (string.Equals(sCode, "NUM"))
                                    {
                                        C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                        // 재설정
                                        if (string.Equals(txtUnit.Text, "EA"))
                                            numeric.Format = "F1";
                                        else
                                            numeric.Format = GetUnitFormatted();

                                        // SRS요청으로 동별 LIMIT값 설정 [2017-11-30]
                                        if (!string.IsNullOrEmpty(sLSL_Limit) && Util.NVC_Decimal(sLSL_Limit) > 0)
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit, CultureInfo.InvariantCulture);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit, CultureInfo.InvariantCulture);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrEmpty(Util.NVC(numeric.Value)) && numeric.Value != 0 && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            if (!string.IsNullOrEmpty(sValue) && !string.Equals(sValue, "NaN"))
                                            //numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture);
                                            {
                                                //소수점Separator에 따라 분기(우크라이나 언어)
                                                if (sValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.CurrentCulture.NumberFormat);
                                                else
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture.NumberFormat);
                                            }

                                            if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }

                                            else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }
                                            else
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                            }
                                        }

                                        if (isEnable.Equals("False"))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#EBEBEB"));
                                        }

                                        numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.LostKeyboardFocus -= OnDataCollectGridGotKeyboardLost;
                                        numeric.LostKeyboardFocus += OnDataCollectGridGotKeyboardLost;
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }

                        if (string.Equals(procId, Process.ROLL_PRESSING))
                        {
                            if (e.Cell.Row.Type == DataGridRowType.Bottom)
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                                if (e.Cell.Column.Index == dataGrid.Columns["CLSS_NAME1"].Index)
                                {
                                    if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                        presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                                }
                                else if (e.Cell.Column.Index == dataGrid.Columns["CLCTVAL01"].Index) // 측정값
                                {
                                    decimal sumValue = 0;
                                    if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                        if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非?字"))
                                        {
                                            foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                                                if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                    sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));


                                            if (sumValue == 0)
                                                presenter.Content = 0;
                                            else
                                                presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dataGrid.Rows.Count - dataGrid.BottomRows.Count), "EA"));
                                        }
                                }
                            }
                        }
                    }
                }));
            }
        }

        protected virtual void OnUnLoadedDgQualitynCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                e.Cell.Presenter.Background = null;

                                if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }));
            }
        }

        #endregion

        #region Color Tag, Defect Tag
        protected virtual void OnDataCollectGridDefectTagComittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, DEFECT_TAG_GRID.Columns[e.Cell.Column.Index].Name)).Equals("") &&
                    Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, DEFECT_TAG_GRID.Columns[e.Cell.Column.Index].Name)) > 0)
                isChagneDefectTag = true;
        }

        protected virtual void OnDataCollectGridColorTagCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, COLOR_GRID.Columns[e.Cell.Column.Index].Name)).Equals("") &&
                 Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, COLOR_GRID.Columns[e.Cell.Column.Index].Name)) > 0)
                isChangeColorTag = true;
        }
        #endregion

        #region WIP Note
        protected virtual void OnLoadedRemarkCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkLostKeyboardFocus;
                        }
                    }
                }
                else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;
                        }
                    }
                }
            }
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (REMARK_GRID.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
            {
                isChangeRemark = true;
            }
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (REMARK_GRID.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }

        #region [POSTACTION]
        protected virtual void OnLoadedPostHoldCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 3)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnPostHoldLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnPostHoldLostKeyboardFocus;
                        }
                    }
                }
                else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 3)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnPostHoldChildLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnPostHoldChildLostKeyboardFocus;
                        }
                    }
                }
            }
        }

        private void OnPostHoldLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (POSTHOLD_GRID.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
            {
                isChangePostAction = true;

            }
        }

        private void OnPostHoldChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (POSTHOLD_GRID.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangePostAction = true;
        }
        #endregion

        private void BindingWipNote(DataTable dt)
        {
            if (REMARK_GRID.GetRowCount() > 0)
                return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

            if (dt.Rows.Count > 0)
            {
                string[] sWipNote = GetWIPNOTE(Util.NVC(dt.Rows[0]["LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    inDataRow["REMARK"] = sWipNote[1];
            }
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                inDataRow["REMARK"] = GetWIPNOTE(Util.NVC(_row["LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(REMARK_GRID, dtRemark, FrameOperation);

            // SLITTER가 아닌 경우 공통특이사항은 숨김
            if (!string.Equals(procId, Process.SLITTING) && !string.Equals(procId, Process.SRS_SLITTING) && !string.Equals(procId, Process.HALF_SLITTING))
                REMARK_GRID.Rows[0].Visibility = Visibility.Collapsed;

            //[E20230127-000272] 코터 공정진척 2번컷 특이사항 자동 입력 건
            if (string.Equals(procId, Process.COATING))
            {
                if (REMARK_GRID != null && REMARK_GRID.GetRowCount() > 1)  //REMARK_GRID.Rows[0] :공통특이사항,REMARK_GRID.Rows[1] : LOTID
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "REMARK")), ""))
                    {
                        DataTableConverter.SetValue(REMARK_GRID.Rows[1].DataItem, "REMARK", GetUnstblSectionWupNote(LoginInfo.CFG_AREA_ID, procId, Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[1].DataItem, "LOTID"))));

                    }
                }
            }
        }

        // [E20230127-000272] 코터 공정진척 2번컷 특이사항 자동 입력 건
        private string GetUnstblSectionWupNote(string sAreaID, string sProcID, string sLotID)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = sAreaID;
                Indata["PROCID"] = sProcID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE_UNSTBL_WIPNOTE", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["ATTR4"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }
        #region[POSTACTION]
        private void BindPostHold(string LOTID, string CUT_ID)
        {
            // if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)) && (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EA") || string.Equals(LoginInfo.CFG_AREA_ID, "ED")))
            // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
            if ((string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING)) && (string.Equals(_RemarkHoldUseFlag, "Y")))
            {
                DataTable _postholdDT = new DataTable();

                Util.gridClear(POSTHOLD_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("CUT_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = LOTID;
                Indata["CUT_ID"] = CUT_ID;
                IndataTable.Rows.Add(Indata);

                _postholdDT = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_POST_HOLD_ELTR", "INDATA", "RSLTDT", IndataTable);
                 
                if (_postholdDT == null || _postholdDT.Rows.Count == 0) // [E20230906-001181] [LGESWA PI Team] HOLD check box - UI Standarization
                {
                   // Util.MessageInfo("SFU9207"); // 기준정보가 없습니다.
                    return;
                }

                DataTable _dt = new DataTable();
                _dt.Columns.Add("LOTID", typeof(string));
                _dt.Columns.Add("LANENUM", typeof(string));
                _dt.Columns.Add("POST_HOLD", typeof(string));
                _dt.Columns.Add("REMARK", typeof(string));

                var remark = new List<string>();
                DataRow dRow = null;
                for (int i = 0; i < _postholdDT.Rows.Count; i++)
                {
                    dRow = _dt.NewRow();
                    dRow["LOTID"] = Util.NVC(_postholdDT.Rows[i]["LOTID"]);
                    dRow["LANENUM"] = Util.NVC(_postholdDT.Rows[i]["LANENUM"]);
                    dRow["POST_HOLD"] = Util.NVC(_postholdDT.Rows[i]["POST_HOLD"]);

                    if (!string.IsNullOrEmpty(Util.NVC(_postholdDT.Rows[i]["HOLD_DESC"]).Split('|')[0]))
                        remark.Add(Util.NVC(_postholdDT.Rows[i]["HOLD_DESC"]).Split('|')[0]);
                    else
                        remark.Add(GetWIPNOTE(Util.NVC(_postholdDT.Rows[i]["LOTID"])).Split('|')[0]);

                    dRow["REMARK"] = remark[0];
                    _dt.Rows.Add(dRow);
                    remark.Clear();
                }

                DataRow dr = _dt.NewRow();

                dr[0] = ObjectDic.Instance.GetObjectName("공통특이사항");
                dr[1] = "ALL";
                string[] sWipNote = GetWIPNOTE(Util.NVC(_postholdDT.Rows[0]["LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    dr["REMARK"] = sWipNote[1];

                _dt.Rows.InsertAt(dr, 0);

                Util.GridSetData(POSTHOLD_GRID, _dt, FrameOperation);

                // SLITTER가 아닌 경우 공통특이사항은 숨김
                if (string.Equals(procId, Process.ROLL_PRESSING))
                    POSTHOLD_GRID.Rows[0].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private string GetWIPNOTE(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region Final Cut Method
        private bool IsFinalProcess()
        {
            // 현재 작업중인 LOT이 마지막 공정인지 체크 [2017-02-16]
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = _LOTIDPR;
            indata["LOTID"] = _LOTID;
            indata["PROCID"] = procId;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                return true;

            return false;
        }

        private Dictionary<string, string> GetRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (REMARK_GRID.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < REMARK_GRID.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[0].DataItem, "REMARK")));
                    sRemark.Append("|");

                    if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.ROLL_PRESSING))
                    {
                        // 롤맵 Hold 기능 추가 Start//////////////////////////////////////////////////////////////////////
                        if (!holdLotClassCode.IsNullOrEmpty() && holdLotClassCode.Count > 0)
                        {
                            string lotId = _LOTID;
                            string holdClassCode = holdLotClassCode[lotId];
                            string holdMessage = string.Empty;

                            //if ("E2000_01".Equals(holdClassCode))
                            //{
                            //    // Tag Section Hold Cond Core LOT[%1] SFU9951 
                            //    holdMessage = GetMessageWithSubstitution("SFU9951", lotId);
                            //}
                            //else
                            //{
                            //    // Tag Section Hold Cond Outmost LOT[%1] SFU9952
                            //    holdMessage = GetMessageWithSubstitution("SFU9952", lotId);
                            //}

                            holdMessage = GetMessageWithSubstitution(holdClassCode.Trim(), lotId);

                            sRemark.Append(holdMessage);
                            sRemark.Append("|");
                        }
                        // 롤맵 Hold 기능 추가 End//////////////////////////////////////////////////////////////////////

                    }

                    // 3. 조정횟수
                    if (WIPREASON_GRID.Visibility == Visibility.Visible && WIPREASON_GRID.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < WIPREASON_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < WIPREASON2_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 4. 압연횟수
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "PRESSCOUNT")));
                    sRemark.Append("|");

                    // 5.색지정보
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                        for (int j = 0; j < COLOR_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01"))) &&
                                Util.NVC_Decimal(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[i].DataItem, "LOTID")), procId);
                    if (mergeInfo.Rows.Count > 0)
                        foreach (DataRow row in mergeInfo.Rows)
                            sRemark.Append(Util.NVC(row["LOTID"]) + " : " + GetUnitFormatted(row["LOT_QTY"]) + txtUnit.Text + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(REMARK_GRID.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        private Dictionary<string, string> GetPostRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (POSTHOLD_GRID.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < POSTHOLD_GRID.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(POSTHOLD_GRID.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    if (string.Equals(procId, Process.SLITTING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(POSTHOLD_GRID.Rows[0].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 3. 조정횟수
                    if (WIPREASON_GRID.Visibility == Visibility.Visible && WIPREASON_GRID.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < WIPREASON_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(WIPREASON_GRID.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (WIPREASON2_GRID.Visibility == Visibility.Visible && WIPREASON2_GRID.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < WIPREASON2_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(WIPREASON2_GRID.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 4. 압연횟수
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[PRODLOT_GRID.SelectedIndex].DataItem, "PRESSCOUNT")));
                    sRemark.Append("|");

                    // 5.색지정보
                    if (string.Equals(procId, Process.ROLL_PRESSING))
                        for (int j = 0; j < COLOR_GRID.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01"))) &&
                                Util.NVC_Decimal(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(COLOR_GRID.Rows[j].DataItem, "CLCTVAL01")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(POSTHOLD_GRID.Rows[i].DataItem, "LOTID")), procId);
                    if (mergeInfo.Rows.Count > 0)
                        foreach (DataRow row in mergeInfo.Rows)
                            sRemark.Append(Util.NVC(row["LOTID"]) + " : " + GetUnitFormatted(row["LOT_QTY"]) + txtUnit.Text + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(POSTHOLD_GRID.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        private Dictionary<int, string> CheckFinalCutLot()
        {
            // 자동 FINAL CUT 여부 [2017-01-13]
            // 0 : Confirm 여부, 1 : Final Cut 여부, 2  : Loss 처리 여부, 3 : Hold 표시 여부, 4 : Confirm Message, 5 : 잔량[팝업], 6 : 잔량[길이부족차감] 
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = _LOTIDPR;
            indata["LOTID"] = _LOTID;
            indata["PROCID"] = procId;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Rows.Count <= 1)
            {
                Util.MessageValidation("SFU1707");  //실적 확정할 대상이 없습니다.
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            string sLotID = string.Empty;
            decimal iOutQty = 0;         // 생산 수량
            decimal iTotQty = 0;         // 투입 LOT 수량
            decimal iResQty = 0;         // 투입 LOT 처리 이후 최종 수량
            bool isFinalOper = false;   // 마지막 CUT 여부

            // 투입 Lot 수량 저장
            iOutQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));   // 생산수량
            iTotQty = Util.NVC_Decimal(dt.Rows[0]["WIPQTY"]);

            // 실적확정해야 할 1번째 LOT이 처음 확정할 LOT인지 체크
            DataRow[] rows = dt.Select("CUT_SEQNO = 1");
            if (rows.Length == ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) - LOTINFO_GRID.TopRows.Count))
            {
                bool isDupplicate = false;
                for (int i = 0 + LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                {

                    for (int j = 0; j < rows.Length; j++)
                    {
                        if (string.Equals(rows[j]["LOTID"], DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID")))
                        {
                            isDupplicate = true;
                            break;
                        }
                    }
                }
                if (isDupplicate == false)
                {
                    Util.MessageValidation("SFU2898", new object[] { rows[0]["LOTID"] });   //LOT[{%1}]을 먼저 실적 확정 하세요.
                    return new Dictionary<int, string> { { 0, bool.FalseString } };
                }
            }
            else
            {
                Util.MessageValidation("SFU2898", new object[] { rows[0]["LOTID"] });   //LOT[{%1}]을 먼저 실적 확정 하세요.
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            // 마지막 실적 확정인지 체크
            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                isFinalOper = true;

            iResQty = Util.NVC_Decimal(GetUnitFormatted(iTotQty - (iOutQty - exceedLengthQty)));

            // 길이초과로 등록된 수량 감안해서 -수량인 경우 체크
            if (iResQty < 0)
            {
                Util.MessageValidation("SFU1614");
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            if (isFinalOper)
            {
                if (iResQty > 0)
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.FalseString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1964", new object[] { iResQty + txtUnit.Text }) }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };   //투입LOT 잔량 {0}가 대기됩니다.\n실적확정 하시겠습니까?
                }
                else
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.TrueString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1965") }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     //투입LOT 잔량이 없습니다.\r\n실적확정하시겠습니까?
                }
            }
            else
            {
                if (iResQty > 0)
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.FalseString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1963", new object[] { iResQty + txtUnit.Text }) }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };   //투입LOT 잔량 {0}이 남게 됩니다.\r\n실적확정하시겠습니까?
                }
                else
                {
                    Util.MessageValidation("SFU1483");  //다음 완성LOT이 존재하여 실적확정 불가합니다.
                    return new Dictionary<int, string> { { 0, bool.FalseString } };
                }
            }
        }

        private bool SetLossLot(C1DataGrid dg, string sItemCode, decimal iLossQty)
        {
            bool isLossValid = false;
            DataTable dt = (dg.ItemsSource as DataView).Table;
            if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) > LOTINFO_GRID.TopRows.Count + 1)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                int iCount = isResnCountUse == true ? 1 : 0;

                for (int iCol = dg.Columns["ALL"].Index + (1 + iCount); iCol < dg.Columns["COSTCENTERID"].Index; iCol += (2 + iCount))
                {
                    string sublotid = dg.Columns[iCol + 1].Name;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                        {
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, sublotid, iLossQty);
                            DefectChange(dg, i, iCol + 1);
                            isLossValid = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "RESNQTY", iLossQty);
                        GetSumDefectQty(dg);
                        isLossValid = true;
                        break;
                    }
                }
            }

            if (isLossValid == false)
                Util.MessageValidation("SFU3196", new object[] { string.Equals(sItemCode, ITEM_CODE_LEN_LACK) ?
                    ObjectDic.Instance.GetObjectName("길이부족") : ObjectDic.Instance.GetObjectName("길이초과") }); //해당 MMD에 {%1}에 관련된 속성이 지정되지 않아 자동Loss를 등록할 수 없습니다.

            return isLossValid;
        }

        private void SetExceedLength()
        {
            if (isCoaterAfterProcess)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                int iCount = isResnCountUse == true ? 1 : 0;

                for (int i = 0; i < WIPREASON_GRID.Rows.Count; i++)
                {
                    if (string.Equals(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
                    {
                        if ((LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count) == LOTINFO_GRID.TopRows.Count + 1)
                            exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, "RESNQTY"));
                        else
                            exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(WIPREASON_GRID.Rows[i].DataItem, WIPREASON_GRID.Columns[WIPREASON_GRID.Columns["ALL"].Index + (2 + iCount)].Name));
                        break;
                    }
                }

                if (exceedLengthQty >= 0)
                {
                    // 롤맵 설비의 경우에는 길이초과 존재 시 포함하여 투입량 계산하도록 수정 [2021-11-04]
                    if (_isRollMapEquipment)
                    {
                        txtInputQty.Value = txtParentQty.Value + Convert.ToDouble(exceedLengthQty);
                        SetInputQty();
                    }

                    decimal inputQty = Util.NVC_Decimal(txtParentQty.Value);
                    decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY"));

                    if (prodQty > 0)
                        txtRemainQty.Value = Convert.ToDouble(inputQty - (prodQty - Util.NVC_Decimal(exceedLengthQty)));
                }
            }
        }

        // 샘플링 라벨발행 수량 / 출하처(자동차 롤프레스,슬리터)
        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            if ((string.Equals(procId, Process.ROLL_PRESSING) || (string.Equals(procId, Process.SLITTING))) && string.Equals(getQAInspectFlag(sLotID), "Y"))
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };
            }

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private string getQAInspectFlag(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(result.Rows[0]["QA_INSP_TRGT_FLAG"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private void SetSaveProductQty()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = _LOTID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR_FOR_PROD_QTY", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                    if (Util.NVC_Decimal(result.Rows[0]["PROD_QTY"]) > 0)
                        txtInputQty.Value = Convert.ToDouble(result.Rows[0]["PROD_QTY"]);
            }
            catch (Exception ex) { }
        }

        private void SetInitProductQty()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));

            DataRow inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["LOTID"] = _LOTID;
            inLotDetailDataRow["PROD_QTY"] = DBNull.Value;
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("DA_BAS_UPD_WIPATTR_FOR_PROD_QTY", "INDATA", null, inDataTable, (result, Returnex) =>
            {
                try
                {
                    if (Returnex != null)
                        return;
                }
                catch (Exception ex) { }
            });
        }

        public void chkLANEqtyAble()
        {
            try
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                if (wo.dgWorkOrder.Rows.Count() <= 0)
                {
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LANE_QTY_EDITABLE_CT_MODEL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count <= 0)
                {
                    txtLaneQty.IsEnabled = false;

                    return;
                }
                else
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRODID"), row["CBO_CODE"]))
                        {
                            txtLaneQty.IsEnabled = true;
                            break;
                        }
                        else
                        {
                            txtLaneQty.IsEnabled = false;
                        }
                    }
                }
                OnClickSearch(SEARCHBUTTON, null);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void chkLANEqtyAble2()
        {
            try
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                if (wo.dgWorkOrder.Rows.Count() <= 0)
                {
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LANE_QTY_EDITABLE_CT_MODEL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count <= 0)
                {
                    txtLaneQty.IsEnabled = false;

                    return;
                }
                else
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRODID"), row["CBO_CODE"]))
                        {
                            txtLaneQty.IsEnabled = true;
                            break;
                        }
                        else
                        {
                            txtLaneQty.IsEnabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetResultInputQTY()
        {
            // 청주 소형전극에서만 패턴에서만 변환해서 값 입력 [COATER에서만 사용]
            if (string.Equals(txtUnit.Text, "EA") && string.Equals(LoginInfo.CFG_AREA_ID, "E1") && string.Equals(procId, Process.COATING))
                txtInputQty.Value = Convert.ToDouble(GetIntFormatted(Util.NVC_Decimal(txtInputQty.Value) / GetPatternLength(_PRODID)));


            if (isCoaterAfterProcess)
            {
                isChangeInputFocus = true;
                if (IsFinalProcess())
                {
                    if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                    {
                        decimal diffQty = Math.Abs(Util.NVC_Decimal(txtParentQty.Value) - Util.NVC_Decimal(txtInputQty.Value));

                        // 투입량의 제한률 이상 초과하면 입력 금지, 단 INPUT_OVER_RATE가 등록되어있지 않으면 SKIP [2017-03-02]
                        decimal inputRateQty = Util.NVC_Decimal(Util.NVC_Decimal(txtParentQty.Value) * inputOverrate);

                        if (inputRateQty > 0 && diffQty > inputRateQty)
                        {
                            Util.MessageValidation("SFU3195", new object[] { Util.NVC(inputOverrate * 100) + "%" });    //투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
                            return;
                        }

                        //  차이수량(생산량-투입량) %1 만큼 길이초과로 등록 하시겠습니까?
                        Util.MessageConfirm("SFU1921", (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                if (SetLossLot(WIPREASON_GRID, ITEM_CODE_LEN_EXCEED, diffQty) == false)
                                    return;

                                exceedLengthQty = diffQty;

                                if (WIPREASON2_GRID.Visibility == Visibility.Visible)
                                {
                                    if (SetLossLot(WIPREASON2_GRID, ITEM_CODE_LEN_EXCEED, diffQty) == false)
                                        return;
                                }
                                isChangeWipReason = true;
                                SetInputQty();
                                WIPREASON_GRID.Refresh();
                                LOTINFO_GRID.Refresh(false);
                            }
                        }, new object[] { diffQty + txtUnit.Text });
                    }
                    else
                    {
                        SetInputQty();
                    }
                }
                else
                {
                    if (_isRollMapEquipment)
                    {
                        if (Convert.ToDouble(txtInputQty.Value) > (Convert.ToDouble(txtParentQty.Value) + Convert.ToDouble(exceedLengthQty)))
                        {
                            Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                            return;
                        }
                        else
                        {
                            SetInputQty();
                        }
                    }
                    else
                    {
                        if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                        {
                            Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                            return;
                        }
                        else
                        {
                            SetInputQty();
                        }
                    }
                }
                isChangeInputFocus = false; // FOCUS 초기화
            }
            else
            {
                SetInputQty();
            }

            if (string.Equals(procId, Process.SLITTING) || string.Equals(procId, Process.SRS_SLITTING) || string.Equals(procId, Process.HALF_SLITTING))
                WIPREASON_GRID.Refresh(false);

            LOTINFO_GRID.Refresh(false);
        }

        #endregion

        #endregion [Defect / DataCollect / Material / Slurry  / Wip Note ]


        #region Biz Rule

        string GetStartLotBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E2500":
                default:
                    bizRuleName = "BR_PRD_REG_START_LOT_HS_EIF";
                    break;
            }

            return bizRuleName;
        }

        string GetLargeLotBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E2000":
                case "S2000":
                default:
                    bizRuleName = "DA_PRD_SEL_COATMLOT_LOT";
                    break;
            }

            return bizRuleName;
        }

        string GetProdLotBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E0500":
                case "E0400":
                case "E0410":
                case "E0420":
                case "E0430":
                case "E1000":
                    bizRuleName = "DA_PRD_SEL_WIP_MX";
                    break;

                case "E2000":
                    bizRuleName = "DA_PRD_SEL_WIP_CT";
                    break;

                case "E2500":
                case "E3000":
                    if (WIPSTATUS.Equals("WAIT"))
                        bizRuleName = "DA_PRD_SEL_PRODUCTLOT_RP_WIP_WAIT";
                    else
                        bizRuleName = "DA_PRD_SEL_PRODUCTLOT_RP";
                    break;

                case "E4000":
                    bizRuleName = "DA_PRD_SEL_WIP_CUT_SL";
                    break;



                default:
                    bizRuleName = "DA_PRD_SEL_WIP_HS";
                    break;
            }

            return bizRuleName;
        }

        string GetResultLotBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E2500":
                case "E3000":
                    bizRuleName = "DA_PRD_SEL_CHILDLOT_INFO_RP";
                    break;


                case "E2000":
                case "E4000":
                    bizRuleName = "DA_PRD_SEL_RUNLOT_SL"; //Add : 2016.12.13, Slitter | SRS Slitter Biz 추가
                    break;

                default:
                    bizRuleName = "DA_PRD_SEL_RUNLOT_HS";
                    break;
            }

            return bizRuleName;
        }

        string GetEndLotBizRuleName()
        {
            string bizRuleName;

            switch (procId)
            {
                case "E2500":
                default:
                    bizRuleName = "BR_PRD_REG_END_LOT_HS_UI";
                    break;
            }

            return bizRuleName;
        }

        #endregion


        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = procId;
                dtRow["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                    //2021-03-10 오화백 QA 샘플링 검사여부 : 롤프레스, 슬리터 공정만
                    if (string.Equals(procId, Process.ROLL_PRESSING) || string.Equals(procId, Process.SLITTING))
                    {
                        // QA 샘플링 검사대상여부
                        if (Qa_Insp_Trgt_Flag == "Y")
                        {

                            CMM001.CMM_COM_QA_INSP_TAGET_YN wndQaYN = new CMM001.CMM_COM_QA_INSP_TAGET_YN();
                            wndQaYN.FrameOperation = FrameOperation;
                            object[] Parameters2 = new object[1];
                            Parameters2[0] = _LOTID;
                            C1WindowExtension.SetParameters(wndQaYN, Parameters2);
                            wndQaYN.Closed -= new EventHandler(wndQaYN_Closed);
                            wndQaYN.Closed += new EventHandler(wndQaYN_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndQaYN.ShowModal()));

                        }
                        else
                        {
                            ConfirmProcess(true);
                        }

                    }
                    else
                    {
                        ConfirmProcess(true);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                if (procId.Equals(Process.HALF_SLITTING) || procId.Equals(Process.ROLL_PRESSING))
                {
                    for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LOTID"));
                        //newRow["WIPSEQ"] = null;
                        newRow["WORKER_NAME"] = sWrokerName;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                    }
                }
                else if (procId.Equals(Process.SLITTING))
                {
                    DataTable dt = ((DataView)REMARK_GRID.ItemsSource).Table;

                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                        //newRow["WIPSEQ"] = null;
                        newRow["WORKER_NAME"] = sWrokerName;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                    }
                }
                else
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = _LOTID;
                    //newRow["WIPSEQ"] = null;
                    newRow["WORKER_NAME"] = sWrokerName;
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        private void wndQaYN_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_QA_INSP_TAGET_YN window = sender as CMM001.CMM_COM_QA_INSP_TAGET_YN;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveSampleYN("Y");
                }
                else
                {
                    SaveSampleYN("N");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveSampleYN(string YN)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SMPL_CMPL_YN", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = procId;
                newRow["SMPL_CMPL_YN"] = YN;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inLot.NewRow();
                newRow["LOTID"] = _LOTID;
                inLot.Rows.Add(newRow);
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SAMPLE_CMPL_YN", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ConfirmProcess(true);

                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        // 롤맵 Hold 기능 추가 Start//////////////////////////////////////////////////////////////////////
        private string GetMessageWithSubstitution(string messageId, params object[] parameters)
        {
            DataTable dtMessage = GetMessageFromCommonCode(messageId);
            string message = string.Empty;

            if (CommonVerify.HasTableRow(dtMessage))
            {
                message = dtMessage.Rows[0]["CMCDNAME"].ToString();
            }

            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }

            return message;
        }

        private DataTable GetMessageFromCommonCode(string messageId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_HOLD_CONDITION_MSG";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                var resultCmcode = (from t in dtResult.AsEnumerable()
                                    where messageId.Equals(t.Field<string>("CMCODE"))
                                    orderby t.Field<decimal>("CMCDSEQ") ascending
                                    select t);
                if (resultCmcode.Any())
                {
                    return resultCmcode.CopyToDataTable();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        //롤맵 Hold 기능 추가 End//////////////////////////////////////////////////////////////////////

        #region [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        /// <summary>
        /// Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        /// </summary>
        private void CheckUseElecMtrlLotValidation()
        {
            try
            {
                _isELEC_MTRL_LOT_VALID_YN = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ELEC_MTRL_LOT_VALID_YN";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (dtResult.Rows[0]["ATTRIBUTE1"].ToString().Equals("Y"))
                            _isELEC_MTRL_LOT_VALID_YN = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// Mixer 원재료 Lot 누락 존재 유무 Check
        /// </summary>
        /// <returns></returns>
        private bool CheckMissedElecMtrlLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = Util.NVC(_LOTID);
                dr["WOID"] = Util.NVC(_WORKORDER);
                dr["EQPTID"] = Util.NVC(_EQPTID);
                dr["PRODID"] = Util.NVC(_PRODID);
                dr["PROCID"] = Util.NVC(procId);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_MISSED", "RQSTDT", "RSLTDT", RQSTDT);

                //미 투입자재 존재하면
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
        private void PopupInputMaterial()
        {

            CMM_ELEC_MISSED_MTRL_INPUT_LOT popupInputMaterial = new CMM_ELEC_MISSED_MTRL_INPUT_LOT { FrameOperation = FrameOperation };

            if (popupInputMaterial != null)
            {
                // [E20240712-001591]
                //object[] Parameters = new object[5];
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(_LOTID);
                Parameters[1] = Util.NVC(_WORKORDER);
                Parameters[2] = Util.NVC(_EQPTID);
                Parameters[3] = Util.NVC(procId);
                Parameters[4] = Util.NVC(_PRODID);

                // [E20240712-001591]
                Parameters[5] = "D";

                C1WindowExtension.SetParameters(popupInputMaterial, Parameters);

                popupInputMaterial.Closed += new EventHandler(PopupInputMaterial_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupInputMaterial.ShowModal()));
                popupInputMaterial.BringToFront();
            }
        }

        private void PopupInputMaterial_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_MISSED_MTRL_INPUT_LOT popup = sender as CMM_ELEC_MISSED_MTRL_INPUT_LOT;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup.bSaveConfirm == true) // PopUp에서 저장을 했을 경우 투입자재 재조회
                {

                    if (procId.Equals(Process.BS) || procId.Equals(Process.CMC) || procId.Equals(Process.InsulationMixing))
                    {
                        GetMaterialList(_LOTID, _PRODID);
                    }
                    else
                    {
                        GetMaterialSummary(_LOTID, _WORKORDER);
                    }
                }
            }
        }

        #endregion

    }
}
