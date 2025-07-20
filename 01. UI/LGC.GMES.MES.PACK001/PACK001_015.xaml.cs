/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 장만철
   Decription : BOX 포장및 라벨 발행, 재발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.02.26  손우석    : Cust LOT ID 스캔하여 BOX 포장 기능
  2018.06.20  손우석    : BOX포장시 투입되는 LOT과 선택된 W/O 제품의 BOM 내용에 포함되는지 체크 로직 반영으로 W/O 선택 기능 Disable
  2018.07.03  손우석    : WO조회 기능 오픈
  2018.10.02  손우석    : 같은 제품 다른 라인인경우 포장 W/O 구분을위해 EQSGID 파라메터 추가
  2018.10.12  손우석    : 같은 제품 다른 라인인경우 포장 W/O 구분을위해 EQSGID 파라메터 추가
  2019.04.02  손우석    : 포장중 에러 메시지 출력후 초기화 제거
  2020.05.14  손우석    : CSR ID 60447 [생산PI팀] 포장 W/O 설정 및 처리에 대한 인터락 기능 개선 [요청번호] C20200513-000399
  2020.10.22  염규범    : PACK BOX 포장 Transaction 분산 처리
  2020.10.22  강호운    : 2nd OOCV 공정분리(P5200,P5400) 공정추가
  2022.09.02  김길용    : 사외반품 프로세스적용
  2023.10.25  김민석    : 이력 조회 시 PRODID가 NULL로 들어가던 것을 cboProduct 값을 가져오는 것으로 수정 [요청번호] E20231018-000854
  2024.08.07  정용석    : 포장처리시 포장개수 설정과 입력한 ID의 개수가 다르면 무조건 Interlock [요청번호] E20240717-000988
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtOrderResult;  // lot scan시 포장 오더 정보 담기 위한 DataTable
        DataTable dtWoProdResult; // cma의 포장오더에 물려있는 prod와 pcsgid 담기 위한 TAble
        DataTable dtBoxWoResult;
        DataTable dtLabelCodes;
        DataTable dtBoxLabelResult;

        // flag 변수
        bool boxingYn = false;   // true - box 구성중, 미완료 / false - 박스포장 가능
        bool unPackYn = false;   // 리스트에서 포장 해재 가능 여부 : true-포장해제 가능 / false-포장해제 불가능
        bool rePrint = false;    // 리스트에서 발행 할것인지 포장완료 후 발행 할것인지 : true-재발행 / false-최초발행
        bool reBoxing = false;   // 재포장 여부
        bool bResetYN = false;   // btnUnPack 버튼으로 포장해제시 전체 포장 했을 경우 Reset() 함수 사용함

        // 수량 관련 변수
        int boxLotmax_cnt = 0;   // 박스에 담길 lot의 최대 수량
        int boxingLot_idx = 0;   // 박스에 담긴 Lot의 수량
        int lotCountReverse = 0; // 박스에 담길 lot의 남은 수량

        // box 관련 변수
        string seleted_boxid;
        string boxing_lot;    // 포장중인 boxid
        string box_prod = ""; // 박스의prod : 투입되는 lot의 prod와 비교하기 위한 변수
        string box_eqsg = "";
        string seleted_palletID;
        string register_wo; // 등록된 wo
        string pack_wotype;

        // 투입되는 lot 관련 변수
        string lot_prod = string.Empty;      // 이전에 투입한 lot의 prodid
        string lot_proc = string.Empty;      // 이전에 투입한 lot의 procid
        string lot_eqsgid = string.Empty;    // 이전에 투입한 lot의  eqsgid
        string lot_class_old = string.Empty; // 이전에 투입한 lot의 class
        string lot_wo = string.Empty;

        #region 재포장을 위한 포장해제 변수
        string seleted_Box_Prod = string.Empty;    // 재포장을 위한 prodid
        string seleted_Box_Procid = string.Empty;  // 재포장을 위한 procid
        string seleted_Box_Eqptid = string.Empty;  // 재포장을 위한 eqptid
        string seleted_Box_Eqsgid = string.Empty;  // 재포장을 위한 eqsgid
        string seleted_Box_PrdClass = string.Empty;// 재포장을 위한 prdclass
        string seleted_oqc_insp_id = string.Empty; // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string seleted_judg_value = string.Empty;  // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string seleted_Box_pcsgid = string.Empty;
        string seleted_Box_Model = string.Empty;
        string seleted_Box_Woid = string.Empty;
        #endregion

        #region 리스트에서 포장해제를 위한 변수
        string unPack_ProdID = string.Empty;       // 포장해제를 위한 prodid
        string unPack_EqsgID = string.Empty;       // 포장해제를 위한 eqsgid
        string unPack_EqptID = string.Empty;       // 포장해제를 위한 eqptid
        string unPack_ProcID = string.Empty;       // 포장해제를 위한 procid
        string unPack_PrdClasee = string.Empty;    // 포장해제를 위한 prdclass
        string unPack_oqc_insp_id = string.Empty;  // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string unPack_judg_value = string.Empty;   // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string unPack_woID = string.Empty;         // 포장해제를 위한 woid : 실제로 상용하지는 않음.
        string unPack_Conf_Prodid = string.Empty;  // 라벨발행을 위해 box 테이블의 CONF_PRODID
        string shipTo_ID = string.Empty;           // 라벨코드 선택시 출하처정보
        #endregion

        #region LOT 최초 스캔시 LOT 정보 담아두기 위한 변수(LOT정보)
        string First_eqsgid = string.Empty;
        string First_prodid = string.Empty;
        string First_routid = string.Empty;
        string First_flowid = string.Empty;
        string First_pcsgid = string.Empty;
        string First_class = string.Empty;
        string First_model = string.Empty;
        #endregion

        #region 스캔시 LOT 정보 담아두기 위한 변수(LOT정보)
        string scan_eqsgid = string.Empty;
        string scan_prodid = string.Empty;
        string scan_routid = string.Empty;
        string scan_flowid = string.Empty;
        string scan_pcsgid = string.Empty;
        string scan_class = string.Empty;
        string scan_model = string.Empty;
        string scan_woid = string.Empty;
        string scan_ocopyn = string.Empty;
        // 2018.02.26
        string scan_lotid = string.Empty;
        #endregion

        #region LOT 최초 스캔시 WO 정보 담아두기 위한 변수(포장정보)
        string wo_prodid = string.Empty;
        string wo_eqsgid = string.Empty;
        string wo_modlid = string.Empty;
        string wo_prodtype = string.Empty;
        string wo_conf_prodid = string.Empty;
        #endregion

        #region Box 스캔시 Box정보를 담아두기 위한 변수(BOX정보)
        string scan_eqsgid_b = string.Empty;
        string scan_prodid_b = string.Empty;
        string scan_routid_b = string.Empty;
        string scan_flowid_b = string.Empty;
        string scan_pcsgid_b = string.Empty;
        string scan_class_b = string.Empty;
        string scan_model_b = string.Empty;
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_015()
        {
            InitializeComponent();
            this.Loaded += PACK001_015_Loaded;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                // 날자 초기값 세팅
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); // 일주일 전 날짜
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;// 오늘 날짜
                this.chkBoxIDAutoCreate.IsChecked = true;
                this.SetBoxCreateAreaControl();
                InitCombo();

                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbBoxHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                tbLotInform.Text = "SCAN LOT" + ObjectDic.Instance.GetObjectName("정보");

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    txtregisterWO.Visibility = Visibility.Visible;
                    txtseletedWO.Visibility = Visibility.Visible;
                }

                // 로그인 User가 중국 NA 소속인 경우
                if (LoginInfo.CFG_SHOP_ID.Equals("G451") && LoginInfo.CFG_AREA_ID.Equals("P4"))
                {
                    this.lblShippingStatus.Visibility = Visibility.Visible;
                    this.cboShippingStatus.Visibility = Visibility.Visible;
                    this.dgBoxhistory.Columns["SHIP_STATNAME"].Visibility = Visibility.Visible;
                    this.dtpIssueScheduleDate.Visibility = Visibility.Visible;
                    this.lblIssueScheduleDate.Visibility = Visibility.Visible;
                }
                else
                {
                    this.lblShippingStatus.Visibility = Visibility.Collapsed;
                    this.cboShippingStatus.Visibility = Visibility.Collapsed;
                    this.dgBoxhistory.Columns["SHIP_STATNAME"].Visibility = Visibility.Collapsed;
                    this.dtpIssueScheduleDate.Visibility = Visibility.Collapsed;
                    this.lblIssueScheduleDate.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void setBoxLotCnt()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = box_eqsg == "" ? scan_eqsgid : box_eqsg;
                dr["SHIPTO_ID"] = null;
                dr["PRODID"] = wo_prodid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXTOTAL_LOTTOTAL_CNT_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows[0]["LOTCNT"] == null || dtResult.Rows[0]["LOTCNT"].ToString() == "")
                    {
                        ms.AlertInfo("SFU3594");
                        // BOX의 [포장 수량]이 정의 되지 않았습니다. Defalut : 5로 세팅됩니다. \nMMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요.
                        nbBoxingCnt.Value = 5;
                    }
                    else
                    {
                        if (Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]) != 0)
                        {
                            nbBoxingCnt.Value = Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]);
                        }
                        else
                        {
                            nbBoxingCnt.Value = 5;
                        }
                    }
                }
                else
                {
                    ms.AlertInfo("SFU3594");
                    // BOX의 [포장 수량]이 정의 되지 않았습니다. Defalut : 5로 세팅됩니다. \nMMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요.
                    nbBoxingCnt.Value = 5;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #region Event

        #region MAIN EVENT
        private void PACK001_015_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();
                this.Loaded -= PACK001_015_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region BUTTON EVENT
        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.rePrint = true;        // 재발행
                if (this.txtBoxIdR.Text.Length <= 0)
                {
                    this.rePrint = false;
                    return;
                }

                // 중국 NA의 경우 Box Label Print시에 출고예정일을 외포장 정보에 Update시키고 Label Print
                if (LoginInfo.CFG_SHOP_ID == "G451" && LoginInfo.CFG_AREA_ID == "P4")
                {
                    this.SetShippingDate();
                }
                this.labelPrint();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxIdR.Text.Length == 0)
                {
                    return;
                }

                if (!unPackYn)
                {
                    // Util.AlertInfo("이미 PALLET에 담긴 BOX 입니다. UPNPACK 할수 없습니다.");
                    ms.AlertWarning("SFU3288"); // 입력오류 : 입력한 BOX가 이미 PALLET에 포장 됐습니다.[BOX의 정보 확인]
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3405"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                // 정말 포장취소 하시겠습니까?
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("재발행");

                        // 2020.10.01 염규범S
                        // 조회 조건에서, 제거 처리로 변경 속도 문제
                        // search();
                    }
                    else
                    {
                        return;
                    }
                }
                   );
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgBoxhistory);
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    ms.AlertWarning("SFU1499"); // 동을 선택하세요.
                    return;
                }

                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;
                if (timeSpan.Days < 0)
                {
                    // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return;
                }

                if (timeSpan.Days > 30)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    Util.MessageValidation("SFU4466");
                    return;
                }

                this.SearchBoxHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            if (dgBoxLot.ItemsSource != null)
            {
                for (int i = dgBoxLot.Rows.Count; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgBoxLot.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgBoxLot.EndNewRow(true);
                        dgBoxLot.RemoveRow(i - 1);
                        boxingLot_idx--;
                        lotCountReverse++;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgBoxLot.GetRowCount() == 0)
                {
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3406"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                // 정말 포장리스트를 삭제 하시겠습니까?
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        dgBoxLot.ItemsSource = null;
                        boxingLot_idx = 0;
                        lotCountReverse = boxLotmax_cnt;
                        // boxingYn = false;

                        tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                        if (txtBOXID.Text.ToString() == "")
                        {
                            boxLotmax_cnt = 0;
                        }

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                    }
                    else
                    {
                        return;
                    }
                }
                  );
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            string confirmMessage = string.Empty;

            try
            {
                Button btn = sender as Button;

                if (btn.Name == "btnPack" && btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    txtcnt.Visibility = Visibility.Visible;
                    btnUnPack.Visibility = Visibility.Hidden;
                    btnRePack.Visibility = Visibility.Hidden;

                    btnSelectCacel.Visibility = Visibility.Visible;
                    btncancel.Visibility = Visibility.Visible;
                    chkBoxIDAutoCreate.IsChecked = false;
                    txtBOXID.IsEnabled = true;
                    txtLOTID.IsEnabled = true;
                    chkBoxIDAutoCreate.Visibility = Visibility.Visible;
                    txtBOXID.IsReadOnly = false;

                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
                    btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");

                    boxingLot_idx = 0;
                    boxLotmax_cnt = 0; // 박싱 가능 수량 세팅 - 정해지면 수정
                    nbBoxingCnt.Value = 5;
                    lotCountReverse = Convert.ToInt32(nbBoxingCnt.Value);
                    tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                    reBoxing = false;

                    dgBoxLot.ItemsSource = null;
                    (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
                    txtLOTID.IsEnabled = true;

                    boxingYn = false;

                    setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "대기중");

                    txtBOXID.Text = "";
                    txtLOTID.Text = "";

                    // 2020.10.01 염규범S
                    // 속도 문제로, 조회 대신, ROW 추가 형식으로 변경의건
                    // search();
                }
                else
                {
                    if (txtBOXID.Text == "")
                    {
                        return;
                    }

                    if (dgBoxLot.GetRowCount() == 0)
                    {
                        return;
                    }

                    if (Util.GetCondition(txtProdClass) == "CMA")
                    {
                        if (Util.GetCondition(txtReworkWOID) == "")
                        {
                            ms.AlertWarning("SFU3452"); // 선택된 W/O가 없습니다.

                            return;
                        }
                    }

                    if (!reBoxingCanYn()) // 재포장시 해제실적이 전송 됐는지 확인
                    {
                        ms.AlertWarning("SFU3453");// 포장해제 실적이 전송되지 않았습니다. 잠시후 다시 시도하세요
                        return;
                    }
                    /* 임시로 풀어줌
                                        if (Util.GetCondition(txtProdClass) == "CMA")
                                        {
                                            if (!woDateDiff()) // w/o의 날짜와 현재 날짜 비교(월비교) : 당월이 아니면 포장 못함.
                                            {
                                                ms.AlertWarning("SFU3573");// 포장하려는 W/O가 당월 W/O가 아닙니다. // SFU3573
                                                return;
                                            }
                                        }
                    */
                    if (pack_wotype == "PPRW") // 재작업 WO인데 포장하려는 LOT의 실적이 없는 경우 포장 못함.
                    {
                        bool reworkChk = true;
                        for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "WOID").ToString() == "" ||
                                DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString() == "")
                            {
                                reworkChk = false; // 실적처리 안된 LOT이라서 포장할수 없음.
                            }
                        }

                        if (!reworkChk)
                        {
                            ms.AlertWarning("SFU3572");// 재작업 WO로 포장할때 실적처리 안된 LOT은 포장할 수 없습니다.
                            return;
                        }
                    }

                    if ((boxingYn || reBoxing) && (boxLotmax_cnt == boxingLot_idx))
                    {
                        confirmMessage = ms.AlertRetun("SFU3385"); // 포장완료 하시겠습니까?"
                    }
                    else if (boxLotmax_cnt != boxingLot_idx)
                    {
                        ms.AlertWarning("SFU3302");
                        //confirmMessage = ms.AlertRetun("SFU3302"); // 작업오류 : BOX의 포장가능 수량과 포장하려는 LOT들의 수량이 다릅니다. 포장 하시겠습니까?
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(confirmMessage, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            boxingEnd(); // 포장 완료 함수

                            Reset();

                            // boxingYn = false; // 포장완료 또는 포장 대기 flag

                            // control_Init(); // control 초기화

                            // setBoxCnt(5, 0, 5, "대기중");

                            // 라벨발행
                            // 신규발행, 재발행
                            // labelPrint(sender);

                            // Util.AlertInfo("포장완료");
                            // ms.AlertInfo("SFU3386"); // 포장이 완료되었습니다.

                            // 2020.10.01 염규범S
                            // 포장 완료후, ROW 추가로 변경 조회 속도 문제로 인하여
                            // search();
                        }
                        else
                        {
                            return;
                        }
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private bool woDateDiff()
        {
            try
            {
                bool pack_yn = true;
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WOID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["WOID"] = Util.GetCondition(txtReworkWOID);

                RQSTDT.Rows.Add(dr);

                DataTable dtWoDateDiff = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WODATE_NODATE_DIFF", "INDATA", "OUTDATA", RQSTDT);

                if (dtWoDateDiff.Rows.Count > 0)
                {
                    string wo_date = dtWoDateDiff.Rows[0]["WO_DATE"].ToString();
                    string now_date = dtWoDateDiff.Rows[0]["NOW_DATE"].ToString();

                    if (wo_date != now_date)
                    {
                        pack_yn = false;
                    }
                }

                return pack_yn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBOXID.Text.Length == 0)
                {
                    return;
                }

                bool UNPACKYN = false;

                if (seleted_palletID != null && seleted_palletID != "")
                {
                    UNPACKYN = false;

                    // box_eqsg = seleted_Pallet_Eqsgid;
                    // box_prod = seleted_Pallet_Prod;

                }
                else
                {
                    UNPACKYN = true;
                }

                if (!UNPACKYN)
                {
                    // Util.AlertInfo("PALLET에 담긴 BOX는 포장취소 할수 없습니다.");
                    ms.AlertWarning("SFU3303"); // 작업오류 : PALLET에 포장된 BOX는 포장취소 할수 없습니다.
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3405"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                // 정말 포장취소 하시겠습니까?
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        // UNPACK 로직
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("재발행");

                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show("포장이 취소 되었습니다.", null, "완료", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        // 2020.10.01 염규범S
                        // 포장 완료후, ROW  추가로 변경의건, 속도 문제로
                        // search();
                        if (!bResetYN) boxValidation();

                    }
                    else
                    {
                        return;
                    }
                }
                   );

                // 초기화
                bResetYN = false;
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    Reset();
                    ms.AlertInfo("SFU3377"); // 작업이 초기화 됐습니다.
                }
                else
                {

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3282"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    // 포장중인 내용이 있습니다. 정말 [작업초기화] 하시겠습니까?
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            Reset();
                            ms.AlertInfo("SFU3377");    // 작업이 초기화 됐습니다.
                        }
                        else
                        {
                            return;
                        }
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reBoxing = true; // 재포장
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }
        #endregion

        #region GRID EVENT
        private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgBoxhistory.Rows.Count == 0 || dgBoxhistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxhistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgBoxhistory.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string selectBox = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXID").ToString();


                string selectPallet = string.Empty;

                if (DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID") != null)
                {
                    selectPallet = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID").ToString();
                }
                else
                {
                    selectPallet = "";
                }

                // PALLET에 담긴 BOX는 해제 불가능
                if (selectPallet == null || selectPallet == "")
                {
                    unPackYn = true;
                }
                else
                {
                    unPackYn = false;
                }

                txtBoxIdR.Text = selectBox;
                txtPltId.Text = selectPallet;

                unPack_ProdID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRODID").ToString(); // BOX 제품
                unPack_ProcID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID").ToString(); // BOX 공정
                unPack_EqptID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQPTID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQPTID").ToString(); // BOX 설비
                unPack_EqsgID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQSGID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQSGID").ToString(); // BOX 라인
                unPack_PrdClasee = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRDCLASS") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRDCLASS").ToString(); // BOX CLASS (CMA,BMA)
                unPack_woID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "WOID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "WOID").ToString(); // BOX WO
                unPack_Conf_Prodid = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "CONF_PRODID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "CONF_PRODID").ToString(); // BOX WO

                txtseletedWO.Text = unPack_woID;
                // 2020.08.11 | 김건식S | 불필요 주석 처리
                // boxLotmax_cnt = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); // 포장된 수량
                // boxingLot_idx = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); // 포장된 수량

                unPack_orderCheck(unPack_PrdClasee, unPack_EqsgID);

                getLabelCode();  // COMMONCODE TABLE에서 해당 화면에서 발행할 LABEL_CODE 가져오기 (CMCDTYPE = "PACK_LABEL_CODE")

                setComboPrint(); // 포장라벨 세팅

            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void setComboPrint()
        {
            try
            {
                string label_codes = string.Empty;

                if (dtLabelCodes != null && dtLabelCodes.Rows.Count > 0)
                {
                    label_codes = dtLabelCodes.Rows[0]["LABEL_CODE1"].ToString();

                    if (dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString();
                    }
                }
                else
                {
                    label_codes = "LBL0020,LBL0067";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = unPack_Conf_Prodid; // "APVCCCMA0-A2"; // unPack_Conf_Prodid;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_CODE"] = label_codes;

                RQSTDT.Rows.Add(dr);

                dtBoxLabelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_BOX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboLabelCode.DisplayMemberPath = "CBO_NAME";
                cboLabelCode.SelectedValuePath = "CBO_CODE";
                cboLabelCode.ItemsSource = DataTableConverter.Convert(dtBoxLabelResult); // AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboLabelCode.SelectedIndex = 0;



                // CommonCombo _combo = new CommonCombo();
                // // 라벨 세팅
                // String[] sFilter = { unPack_ProdID, LoginInfo.CFG_SHOP_ID, label_codes};
                // _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD_BOX");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private void getLabelCode()
        {
            try
            {
                string contry = string.Empty;

                if (LoginInfo.CFG_SHOP_ID == "A040")
                {
                    contry = "KOR";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G451")
                {
                    contry = "CNA";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G382")
                {
                    contry = "CMI";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G481")
                {
                    contry = "POL";
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = contry;

                RQSTDT.Rows.Add(dr);

                dtLabelCodes = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void gridDoubleClickProcess(object sender, MouseButtonEventArgs e, C1.WPF.DataGrid.C1DataGrid grid)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = grid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }

                    if (cell.Column.Name == "BOXID")
                    {
                        PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("BOXID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["BOXID"] = cell.Text;

                            dtData.Rows.Add(newRow);

                            // ========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            // ========================================================================

                            // popup.Closed -= popup_Closed;
                            // popup.Closed += popup_Closed;
                            popup.ShowModal();
                            //popup.CenterOnScreen();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgBoxLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;

                gridDoubleClickProcess(sender, e, grid);

            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }
        #endregion

        #region TEXTBOX EVENT
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SetBoxkeyDown();
                }
                catch (Exception ex)
                {
                    // Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl && e.Key == Key.V)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                if (txtBOXID.Text == "") // boxid 가 입력 되지 않았고
                {
                    if ((bool)chkBoxIDAutoCreate.IsChecked) // BOXID 체크 박스가 체크 있을경우 BOXID 자동 생성
                    {
                        try
                        {
                            // 입력된 lot validation
                            if (!lotValidation_BR()) // lot 체크 BR에서 LOT의 공정 상태를 완공처리함.
                            {
                                txtLOTID.Text = "";
                                return;
                            }

                            // lot의 정보가져오기
                            getLotInform();

                            First_eqsgid = scan_eqsgid;
                            First_prodid = scan_prodid;
                            First_routid = scan_routid;
                            First_flowid = scan_flowid;
                            First_pcsgid = scan_pcsgid;
                            First_class = scan_class;
                            First_model = scan_model;

                            txtBoxingModel.Text = scan_model;
                            txtProdClass.Text = scan_class;

                            box_eqsg = scan_eqsgid;
                            wo_eqsgid = scan_eqsgid;

                            // 2018.07.03
                            // 2018.06.20 ================
                            if (scan_class == "CMA")
                            {
                                btnWorkOroderSearch.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                btnWorkOroderSearch.Visibility = Visibility.Hidden;
                            }
                            // 2018.06.20 ================
                            // 2018.07.03

                            if (txtReworkWOID.Text == null || txtReworkWOID.Text == "") // 첫번째 lot 선택시
                            {
                                if (scan_class == "BMA")
                                {
                                    txtBoxingProd.Text = scan_prodid;
                                    txtBoxingPcsg.Text = scan_pcsgid;

                                    wo_prodid = scan_prodid;
                                    box_prod = wo_prodid;

                                    txtReworkWOID.Text = scan_woid;      // lot의 w/o
                                                                         // wo_prodid = dtOrderResult.Rows[0]["PRODID"].ToString();             // 등록된 wo의 제품

                                    // wo_modlid = dtOrderResult.Rows[0]["MODLID"].ToString();             // 등록된 wo의 모델
                                    // wo_prodtype = dtOrderResult.Rows[0]["PRODTYPE"].ToString();         // 등록된 wo의 제품타입(CMA,BMA)
                                    // wo_conf_prodid = dtOrderResult.Rows[0]["CONF_PRODID"].ToString();   // 등록된 wo의 ASSY제품

                                    // if (!orderCheck()) // 등록된 order check
                                    // {
                                    // ms.AlertWarning("SFU3454"); // 포장오더를 선택한 후 다시 스캔하세요
                                    // txtReworkWOID.Text = "";
                                    // return;
                                    // }
                                }
                                else if (scan_class == "CMA")
                                {
                                    if (findWoProd())// 등록된 포장오더에서 포장제품 찾기
                                    {
                                        if (!ckhCmaBoxingYn(scan_prodid)) // CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                                        {
                                            ms.AlertWarning("SFU3571"); // CMA 포장을 할수 없는 LOT입니다.
                                            // 2019.04.02
                                            // reSet();
                                            return;
                                        }

                                        // WO 자동변경 로직 수행 : 첫 lot일 경우만 해당 로직 실행           : CMA W/O 자동변경에 대한 컨셉이 아직 정해지지 않아서 잠시 보류
                                        // woAutoChangeProcess(Util.GetCondition(txtLotId));

                                        int idx = 0;

                                        // 2020.05.14
                                        // if (dtWoProdResult.Rows.Count == 2)
                                        // {
                                        // if (dtWoProdResult.Rows[0]["PCSGID"].ToString() == "P")
                                        // {
                                        //     idx = 1;
                                        // }
                                        // }

                                        for (int nCnt = 0; dtWoProdResult.Rows.Count > nCnt; nCnt++)
                                        {
                                            if (dtWoProdResult.Rows[nCnt]["PCSGID"].ToString() == "B")
                                            {
                                                idx = nCnt;
                                            }
                                        }

                                        if (idx == 100)
                                        {
                                            ms.AlertWarning("SFU3657"); // W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                            txtReworkWOID.Text = "";
                                            return;
                                        }
                                        else
                                        {
                                            txtReworkWOID.Text = dtWoProdResult.Rows[idx]["WOID"].ToString();
                                            txtBoxingProd.Text = dtWoProdResult.Rows[idx]["PRODID"].ToString();
                                            txtBoxingPcsg.Text = dtWoProdResult.Rows[idx]["PCSGID"].ToString();

                                            wo_prodid = Util.GetCondition(txtBoxingProd);
                                            box_prod = wo_prodid;
                                        }
                                    }
                                    else
                                    {
                                        ms.AlertWarning("SFU3657"); // W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                        txtReworkWOID.Text = "";
                                        return;
                                    }
                                }
                            }

                            // BOXID 자동 생성
                            // 2018.06.21
                            if (LoginInfo.CFG_SHOP_ID == "G481")
                            {
                                // 2018.02.26
                                autoBoxIdCreate_CWA(scan_lotid);
                            }
                            else
                            {
                                autoBoxIdCreate();
                            }

                            // 생성된 boxid의 prod 가져옴.
                            // getBoxProd(); 위에서 w/o의 제품 가져오게 수정됐으므로 불필요한 함수

                            // if(lot_prod != box_prod)
                            // {
                            // // Util.AlertInfo("BOX의 PROD와 LOT의 PROD가 다릅니다.");
                            // ms.AlertWarning("SFU3283"); // 입력오류 : 포장중인 BOX의 제품과 입력한 LOT의 제품이 다릅니다.
                            // return;
                            // }

                            Util.gridClear(dgBoxLot); // 그리드 clear

                            // 포장 가능 수량 세팅
                            setBoxLotCnt();

                            // lot을 그리드(dgBoxLot)에 추가
                            // 2018.06.21
                            if (LoginInfo.CFG_SHOP_ID == "G481")
                            {
                                // 2018.02.26
                                addGridLot_CWA(scan_lotid);
                            }
                            else
                            {
                                addGridLot();
                            }

                            // BOX Label 자동발행
                            // labelPrint();

                            // boxing 상태 초기화
                            // boxingStatInit();

                            txtLOTID.Text = "";

                            boxingYn = true; // 박싱중.
                            // boxingLot_idx++; // box에 담긴 lot 수량 체크
                            // lotCountReverse--;

                            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                        }
                        catch (Exception ex)
                        {
                            // Util.AlertInfo(ex.Message);
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        // Util.AlertInfo("BoxId 먼저 입력하세요");
                        ms.AlertWarning("SFU3387"); // BoxId 먼저 입력하세요
                    }
                }
                else // 2번째 부터 lot Scan시 / 조회결과에서 [적용] 클릭한후 포장 해제 한 후 lot scan
                {
                    try
                    {
                        if (boxingYn || reBoxing) // boxid 입력후 엔터 안친경우(즉 box에 대한 validation을 하지 않은 경우
                        {
                            // box의 담길 lot의 수량체크
                            if (boxLotmax_cnt > boxingLot_idx)
                            {
                                if (gridDistinctCheck("lot")) // 그리드 중복 체크
                                {
                                    // 입력된 lotid validation
                                    if (!lotValidation_BR()) // if (!lotValidation())
                                    {
                                        txtLOTID.Text = "";
                                        return;
                                    }

                                    // lot의 wip 정보가져오기
                                    getLotInform();

                                    txtBoxInfo.Text = scan_class + " : " + scan_eqsgid + " : " + scan_prodid;

                                    if (dgBoxLot.GetRowCount() > 0) // lot이 두번째부터 비교
                                    {
                                        if (scan_class == "CMA")
                                        {
                                            if (!ckhCmaBoxingYn(scan_prodid)) // CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                                            {
                                                ms.AlertWarning("SFU3571"); // CMA 포장을 할수 없는 LOT입니다.
                                                return;
                                            }
                                        }

                                        if (scan_class != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3456"); // 포장대기중인 LOT의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        if (scan_prodid != First_prodid)
                                        {
                                            ms.AlertWarning("SFU3457"); // 포장대기중인 LOT의 제품과 스캔한 LOT의 제품이 다릅니다.
                                            return;
                                        }

                                        if (scan_eqsgid != First_eqsgid)
                                        {
                                            ms.AlertWarning("SFU3458"); // 포장대기중인 LOT의 라인과 스캔한 LOT의 라인이 다릅니다.
                                            return;
                                        }

                                        if (scan_pcsgid != First_pcsgid)
                                        {
                                            ms.AlertWarning("SFU3459"); // 포장대기중인 LOT의 공정군과 스캔한 LOT의 공정군이 다릅니다.
                                            return;
                                        }
                                    }
                                    else // boxid 스캔 후 lotid 스캔한 경우 - box의 정보와 비교
                                    {
                                        First_eqsgid = scan_eqsgid;
                                        First_prodid = scan_prodid;
                                        First_routid = scan_routid;
                                        First_flowid = scan_flowid;
                                        First_pcsgid = scan_pcsgid;
                                        First_class = scan_class;
                                        First_model = scan_model;

                                        if (scan_class != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3460"); // 포장하려는 BOX의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        // if (scan_eqsgid != scan_eqsgid_b)
                                        // {
                                        // ms.AlertWarning("SFU3461"); // 포장하려는 BOX의 라인과 스캔한 LOT의 라인이 다릅니다.
                                        // return;
                                        // }
                                    }

                                    // if (lot_prod != box_prod)
                                    // {
                                    // // Util.AlertInfo("BOX의 PROD와 LOT의 PROD가 다릅니다.");
                                    // ms.AlertWarning("SFU3283"); // 입력오류 : 포장중인 BOX의 제품과 입력한 LOT의 제품이 다릅니다.
                                    // return;
                                    // }

                                    // 2018.06.21
                                    if (LoginInfo.CFG_SHOP_ID == "G481")
                                    {
                                        // 2018.02.26
                                        addGridLot_CWA(scan_lotid);
                                    }
                                    else
                                    {
                                        addGridLot();
                                    }

                                    setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");

                                    txtLOTID.Text = "";
                                }
                            }
                            else
                            {
                                // Util.AlertInfo("포장 가능 수량( " + boxLotmax_cnt.ToString() + " )을 넘습니다.");
                                ms.AlertWarning("SFU3306", boxLotmax_cnt.ToString()); // 입력오류 : BOX의 포장가능 수량 %1을 넘었습니다. [포장수량 수정 후 LOT 입력]
                            }
                        }
                        else
                        {
                            ms.AlertWarning("SFU3455"); // Boxid TextBox에서 엔터를 치세요
                            txtBOXID.Focus();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        txtLOTID.Text = "";
                        // Util.AlertInfo(ex.Message);
                        Util.MessageException(ex);
                    }
                }
            }
        }

        private void woAutoChangeProcess(string lotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                // RQSTDT.Columns.Add("EQSGID", typeof(string));
                // RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                // dr["EQSGID"] = eqsgid; // cma lot의 EQSGID
                // dr["PRODID"] = prodid; // cma lot의 PROD
                dr["LOTID"] = lotID; // cma lot의 LOLTID

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_WO_AUTO_CHANGE_BOX", "INDATA", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ckhCmaBoxingYn(string scanProdid)
        {
            try
            {
                bool boxingYn = true;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = scanProdid; // cma lot의 PROD
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; // cma lot의 PROD

                RQSTDT.Rows.Add(dr);

                DataTable dtCMABoxingYn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CMA_BOXING_YN", "INDATA", "OUTDATA", RQSTDT);

                if (dtCMABoxingYn == null || dtCMABoxingYn.Rows.Count == 0)
                {
                    boxingYn = false;
                }

                return boxingYn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool findWoProd()
        {
            try
            {
                bool find = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                // 2018.10.02
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = scan_prodid; // cma lot의 PROD
                // 2018.10.02
                dr["EQSGID"] = scan_eqsgid;

                RQSTDT.Rows.Add(dr);

                dtWoProdResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CMA_BOX_ORDER_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtWoProdResult.Rows.Count > 0)
                {
                    find = true;
                }
                return find;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void getLotInform()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = Util.GetCondition(txtLOTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtLotInformResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SCAN_INFORM", "INDATA", "OUTDATA", RQSTDT); // DA_PRD_SEL_LOT_SCAN_INFORM

                if (dtLotInformResult.Rows.Count > 0)
                {
                    scan_eqsgid = dtLotInformResult.Rows[0]["EQSGID"].ToString();
                    scan_prodid = dtLotInformResult.Rows[0]["PRODID"].ToString();
                    scan_routid = dtLotInformResult.Rows[0]["ROUTID"].ToString();
                    scan_flowid = dtLotInformResult.Rows[0]["FLOWID"].ToString();
                    scan_pcsgid = dtLotInformResult.Rows[0]["PCSGID"].ToString();
                    scan_class = dtLotInformResult.Rows[0]["CLASS"].ToString();
                    scan_model = dtLotInformResult.Rows[0]["MODEL"].ToString();
                    scan_woid = dtLotInformResult.Rows[0]["WOID"].ToString();
                    scan_ocopyn = dtLotInformResult.Rows[0]["OCOP_RTN_FLAG"].ToString();
                    // 2018.02.26
                    // scan_lotid = dtLotInformResult.Rows[0]["LOTID"].ToString();
                    // 2018.06.22
                    scan_lotid = Util.GetCondition(txtLOTID);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool orderCheck()
        {
            try
            {
                string sProcid = string.Empty;
                bool orderOK = false;

                if (scan_class == "CMA")
                {
                    sProcid = "P5500";
                }
                else if (scan_class == "BMA")
                {
                    sProcid = "P9500";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = scan_eqsgid == "" ? LoginInfo.CFG_EQSG_ID : scan_eqsgid;
                dr["PROCID"] = sProcid;
                dr["PRODID"] = scan_prodid;

                RQSTDT.Rows.Add(dr);

                dtOrderResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQSGID_PROCID_TO_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtOrderResult.Rows.Count > 0) // 해당 제품으로 설정된 W/O가 있을 경우 설정된 정보를 UI에 뿌림
                {
                    orderOK = true;

                    // string set_woid = dtOrderResult.Rows[0]["WOID"].ToString();      // 등록된 wo
                    // string set_wo_prodid = dtOrderResult.Rows[0]["PRODID"].ToString();             // 등록된 wo의 제품
                    // string set_wo_eqsgid = dtOrderResult.Rows[0]["EQSGID"].ToString();             // 등록된 wo의 공정군
                    // string set_wo_modlid = dtOrderResult.Rows[0]["MODLID"].ToString();             // 등록된 wo의 모델
                    // string set_wo_prodtype = dtOrderResult.Rows[0]["PRODTYPE"].ToString();         // 등록된 wo의 제품타입(CMA,BMA)
                    // string set_wo_conf_prodid = dtOrderResult.Rows[0]["CONF_PRODID"].ToString();   // 등록된 wo의 ASSY제품

                    // // 설정된 w/o 가 없을 경우 처리
                    // // 설정된 w/o가 있을 경우 처리
                    // if (set_woid)
                    // {

                    // }

                    txtReworkWOID.Text = dtOrderResult.Rows[0]["WOID"].ToString();      // 등록된 wo
                    wo_prodid = dtOrderResult.Rows[0]["PRODID"].ToString();             // 등록된 wo의 제품
                    wo_eqsgid = dtOrderResult.Rows[0]["EQSGID"].ToString();             // 등록된 wo의 공정군
                    wo_modlid = dtOrderResult.Rows[0]["MODLID"].ToString();             // 등록된 wo의 모델
                    wo_prodtype = dtOrderResult.Rows[0]["PRODTYPE"].ToString();         // 등록된 wo의 제품타입(CMA,BMA)
                    wo_conf_prodid = dtOrderResult.Rows[0]["CONF_PRODID"].ToString();   // 등록된 wo의 ASSY제품
                }
                return orderOK;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unPack_orderCheck(string prd_class, string eqsgid)
        {
            try
            {
                string sProcid = string.Empty;

                if (prd_class == "CMA")
                {
                    sProcid = "P5500";
                }
                else if (prd_class == "BMA")
                {
                    sProcid = "P9500";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = eqsgid;
                dr["PROCID"] = sProcid;

                RQSTDT.Rows.Add(dr);

                dtOrderResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQSGID_PROCID_TO_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtOrderResult.Rows.Count > 0)
                {
                    register_wo = dtOrderResult.Rows[0]["WOID"].ToString();      // 등록된 wo
                    txtregisterWO.Text = register_wo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtSearchBox.Text.Length == 0)
                    {
                        return;
                    }

                    SearchBox(Util.GetCondition(txtSearchBox), true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

            // if(e.Key == Key.LeftCtrl && e.Key == Key.V)
            // {
            // try
            // {
            //     if (txtSearchBox.Text.Length == 0)
            //     {
            //         return;
            //     }

            //     string[] stringSeparators = new string[] { "\r\n", "\n", "," };
            //     string sPasteString = Clipboard.GetText();
            //     string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);


            //     for (int i = 0; i < sPasteStrings.Length; i++)
            //     {
            //         if (string.IsNullOrEmpty(sPasteStrings[i]))
            //         {
            //             Util.MessageInfo("SFU1190", (result) =>
            //             {
            //                 if (result == MessageBoxResult.OK)
            //                 {
            //                     HiddenLoadingIndicator();
            //                     return;
            //                 }
            //             });

            //         }
            //         SearchBox(sPasteStrings[i]);

            //         System.Windows.Forms.Application.DoEvents();
            //     }

            // }
            // catch (Exception ex)
            // {
            //     Util.MessageException(ex);
            // }
            // }
        }
        #endregion

        #region CHECKBOX EVENT

        private void chkBoxIDAutoCreate_Click(object sender, RoutedEventArgs e)
        {
            this.SetBoxCreateAreaControl();
        }

        private void SetBoxCreateAreaControl()
        {
            if ((bool)chkBoxIDAutoCreate.IsChecked)
            {
                if (boxingYn)
                {
                    chkBoxIDAutoCreate.IsChecked = false;
                    txtBOXID.Text = boxing_lot;
                    txtLOTID.Text = string.Empty;
                    return;
                }
                txtBOXID.IsReadOnly = true;
                txtBOXID.IsEnabled = false;
                txtBOXID.Text = string.Empty;
            }
            else
            {
                if (boxingYn)
                {
                    chkBoxIDAutoCreate.IsChecked = true;
                    return;
                }
                txtBOXID.IsReadOnly = false;
                txtBOXID.IsEnabled = true;
            }
        }
        #endregion

        #region 기타 이벤트
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_015_WORKORDERSELECT popup = sender as PACK001_015_WORKORDERSELECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtReworkWOID.Text = popup.WOID;
                    txtBoxingPcsg.Text = popup.PCSGID;
                    txtBoxingProd.Text = popup.PRODID;
                    // 2018.10.12
                    txtEqsgID.Text = popup.EQSGID;
                    txtEqsgname.Text = popup.EQSGNAME;

                    pack_wotype = popup.WOTYPE;
                    wo_prodid = popup.PRODID;
                    box_prod = wo_prodid;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt - boxingLot_idx;
            string stat = string.Empty;

            if (boxingYn || reBoxing)
            {
                stat = ObjectDic.Instance.GetObjectName("포장중");
                if (lotCountReverse > 0)
                {
                    txtLOTID.IsEnabled = true;
                }
                else
                {
                    txtLOTID.IsEnabled = false;
                }
            }
            else
            {
                stat = ObjectDic.Instance.GetObjectName("대기중");
            }

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, stat);

        }
        #endregion

        #endregion

        #region Mehod
        private void SearchBoxHistory()
        {
            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SYSTEM_ID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("BOXID", typeof(string));
                dtINDATA.Columns.Add("FROMDATE", typeof(string));
                dtINDATA.Columns.Add("TODATE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("SHIP_STATCD", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SYSTEM_ID"] = LoginInfo.SYSID;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = Util.GetCondition(cboArea);
                //2023.10.26 - 기존 null로 들어가던 PRODOD 정보를 cboProduct로 조회하도록 수정 - KIM MIN
                drINDATA["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                drINDATA["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                drINDATA["BOXID"] = null;
                drINDATA["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                drINDATA["TODATE"] = Util.GetCondition(dtpDateTo);
                drINDATA["USERID"] = LoginInfo.USERID;
                if (LoginInfo.CFG_SHOP_ID.Equals("G451") && LoginInfo.CFG_AREA_ID.Equals("P4"))
                {
                    drINDATA["SHIP_STATCD"] = string.IsNullOrEmpty(Util.GetCondition(this.cboShippingStatus)) ? null : Util.GetCondition(this.cboShippingStatus);
                }
                else
                {
                    drINDATA["SHIP_STATCD"] = null;
                }
                dtINDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", dtINDATA);

                dgBoxhistory.ItemsSource = null;
                txtBoxIdR.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    txtSearchBox.Text = "";
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null; // LoginInfo.CFG_EQSG_ID;
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            // 동
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            // 모델
            // C1ComboBox[] cboProductModelParent = { cboSHOPID, cboArea, cboEquipmentSegment };
            // C1ComboBox[] cboProductModelChild = { cboProduct };
            // _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild);

            // 모델
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            // 제품
            // C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
            // _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);

            // 제품코드
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            // getProduct(cboProduct);
            // Shipping Status
            PackCommon.SetC1ComboBox(this.GetCommonCodeInfo("SHIP_BOX_RCV_ISS_STAT_CODE"), this.cboShippingStatus, true, "ALL");
        }

        private DataTable GetCommonCodeInfo(string cmcdType)
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("CBO_CODE") == "SHIPPING" ||
                                                      x.Field<string>("CBO_CODE") == "CANCEL_SHIP" ||
                                                      x.Field<string>("CBO_CODE") == "END_RECEIVE" ||
                                                      x.Field<string>("CBO_CODE") == "SHIPPED").CopyToDataTable();
        }

        private void getBoxProd()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["BOXID"] = boxing_lot;


                RQSTDT.Rows.Add(searchCondition);

                DataTable dtBoxProd = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOXCHECK_PROD", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtBoxProd.Rows.Count > 0)
                {
                    box_prod = dtBoxProd.Rows[0]["PRODID"].ToString(); // assy PROD
                    box_eqsg = dtBoxProd.Rows[0]["EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtBoxProd.Rows[0]["EQSGID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Boolean getBoxInfo(string strBoxId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["BOXID"] = strBoxId;


                RQSTDT.Rows.Add(searchCondition);

                DataTable dtBoxProd = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                if (string.IsNullOrEmpty(dtBoxProd.Rows[0]["OUTER_BOXID"].ToString()))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private void SetBoxkeyDown()
        {
            try
            {
                if (chkBoxIDAutoCreate.IsChecked == true && btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장")) // boxid가 체크 되어 있으면 biz에서 validaiton 수행 후  boxid 생성 하므로 로직에서 validation 필요없음.
                {
                    // Util.AlertInfo("CHECKBOX의 CHECK를 풀고 BOXID를 입력하세요");
                    ms.AlertWarning("SFU3307"); // 선택오류 : BOXID 자동생성 CHECKBOX를 풀고 BOXID를 입력하세요
                }
                else
                {
                    if (boxingYn)
                    {
                        // Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                        ms.AlertWarning("SFU3380"); // 이전 포장 작업이 완료 되지 않았습니다.

                        txtBOXID.Text = boxing_lot;
                        return;
                    }

                    // 입력된 boxid validation
                    boxValidation();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBoxCnt(int max_cnt, int lot_cnt, int lotCountReverse, string boxing_stat)
        {
            if (txtcnt == null)
            {
                return;
            }

            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " : " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            tbCount.Text = lotCountReverse.ToString();
        }

        // CHECK BOX(chkBoxId ) 가 체크 됐고 LOTID 입력(KEYIN, SCAN)시 BOXID 생성하는 함수
        // 2018.06.21
        private void autoBoxIdCreate_CWA(string sLOT)
        {
            try
            {
                string setProcid = string.Empty;

                if (lot_class_old == "CMA")
                {
                    setProcid = "P5500";
                }
                else if (lot_class_old == "BMA")
                {
                    setProcid = "P9500";
                }
                else
                {
                    return;
                }

                // boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  // INPUT TYPE (UI OR EQ)
                RQSTDT.Columns.Add("LANGID", typeof(string));   // LANGUAGE ID
                RQSTDT.Columns.Add("LOTID", typeof(string));    // 투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   // ㅍ장공정 ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   // lot의 제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   // 투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   // 라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   // 사용자ID
                RQSTDT.Columns.Add("WOID", typeof(string));   // 사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                // 2018.02.26
                // dr["LOTID"] = txtLotId.Text;
                dr["LOTID"] = sLOT;
                dr["PROCID"] = setProcid;// lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod; // wo가 있으면 설정된 포장제품, wo가 없으면 포장제품 찾아서 box table에 넣어줌(이 변수는 큰 의미 없음).
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString();// 화면에서 선택한 수량....나중에 포장시 실제 담긴 수량으로 변경됨.
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = txtReworkWOID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBOXID.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot = txtBOXID.Text.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void autoBoxIdCreate()
        {
            try
            {
                string setProcid = string.Empty;

                if (lot_class_old == "CMA")
                {
                    setProcid = "P5500";
                }
                else if (lot_class_old == "BMA")
                {
                    setProcid = "P9500";
                }
                else
                {
                    return;
                }

                // boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  // INPUT TYPE (UI OR EQ)
                RQSTDT.Columns.Add("LANGID", typeof(string));   // LANGUAGE ID
                RQSTDT.Columns.Add("LOTID", typeof(string));    // 투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   // ㅍ장공정 ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   // lot의 제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   // 투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   // 라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   // 사용자ID
                RQSTDT.Columns.Add("WOID", typeof(string));   // 사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLOTID.Text;
                dr["PROCID"] = setProcid;// lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod; // wo가 있으면 설정된 포장제품, wo가 없으면 포장제품 찾아서 box table에 넣어줌(이 변수는 큰 의미 없음).
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString();// 화면에서 선택한 수량....나중에 포장시 실제 담긴 수량으로 변경됨.
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = txtReworkWOID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBOXID.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot = txtBOXID.Text.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  // INPUT TYPE (UI OR EQ)
                RQSTDT.Columns.Add("LANGID", typeof(string));   // LANGUAGE ID
                RQSTDT.Columns.Add("LOTID", typeof(string));    // 투이LOT(처음 LOT)

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLOTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT", "INDATA", "OUTDATA", RQSTDT);

                // LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    lot_proc = string.Empty;

                    DataTable dtLOTINFO = new DataTable();
                    dtLOTINFO.TableName = "RQSTDT";
                    dtLOTINFO.Columns.Add("LOTID", typeof(string));    // 투입LOT(처음 LOT)

                    DataRow drLOTINFO = dtLOTINFO.NewRow();
                    drLOTINFO["LOTID"] = Util.GetCondition(txtLOTID);

                    dtLOTINFO.Rows.Add(drLOTINFO);

                    DataTable dtLOTINFOResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTWIP_INFO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtLOTINFOResult.Rows.Count > 0)
                    {
                        string lot_class = dtLOTINFOResult.Rows[0]["CLASS"].ToString();
                        string lot_prodtype = dtLOTINFOResult.Rows[0]["PRODTYPE"].ToString();
                        string lot_wiphold = dtLOTINFOResult.Rows[0]["WIPHOLD"].ToString();
                        string lot_wipstat = dtLOTINFOResult.Rows[0]["WIPSTAT"].ToString();
                        string lot_procid = dtLOTINFOResult.Rows[0]["PROCID"].ToString();
                        string lot_prodid = dtLOTINFOResult.Rows[0]["PRODID"].ToString();
                        string lot_eqsg = dtLOTINFOResult.Rows[0]["EQSGID"].ToString();
                        string lot_route = dtLOTINFOResult.Rows[0]["ROUTID"].ToString();

                        if (lot_wipstat == "TERM") // 페기된 lot인지 체크
                        {
                            // Util.AlertInfo("폐기된 LOT입니다.");
                            ms.AlertWarning("SFU3290"); // 입력오류 : 폐기된 LOT입니다. [LOT 이력 확인]
                            return false;
                        }

                        if (lot_wiphold == "Y") // hold 상태인지 체크
                        {
                            ms.AlertWarning("SFU1340"); // HOLD LOT입니다.
                            return false;
                        }

                        if (lot_class_old != null && lot_class_old != "") // 이전 투입 제품 타입과 같은지 비교
                        {
                            if (lot_class_old != lot_class)
                            {
                                // Util.AlertInfo("이전에 투입한 LOT의 제품 타입과 다릅니다.");
                                ms.AlertWarning("SFU3291"); // 입력오류 : 포장 대기중인 LOT들의 제품타입과 현재 투입한 LOT의 제품타입이 다릅니다.
                                return false;
                            }
                        }

                        if (lot_class == "CMA") // 제품타입별 포장 가능 공정 체크
                        {
                            if (lot_procid == "P5000" || lot_procid == "P5500" || lot_procid == "P5200" || lot_procid == "P5400")
                            {

                            }
                            else
                            {
                                // Util.AlertInfo("포장 불가능한 공정입니다.");
                                ms.AlertWarning("SFU3388"); // 포장 불가능한 공정입니다.
                                return false;
                            }
                        }
                        else if (lot_class == "BMA")
                        {
                            if (lot_procid == "P9000" || lot_procid == "P9500")
                            {

                            }
                            else
                            {
                                // Util.AlertInfo("포장 불가능한 공정입니다.");
                                ms.AlertWarning("SFU3388"); // 포장 불가능한 공정입니다.
                                return false;
                            }
                        }
                        else
                        {
                            // Util.AlertInfo("포장가능한 제품타입이 아닙니다.");
                            ms.AlertWarning("SFU3382"); // 포장가능한 제품타입(CMA,BMA)이 아닙니다.
                            return false;
                        }

                        if (lot_prod != null && lot_prod != "") // 이전 투입 lot의 제품과 같은지 비교
                        {
                            if (lot_prod != lot_prodid)
                            {
                                // Util.AlertInfo("이전에 투입한 LOT의 제품과 다릅니다.");
                                ms.AlertWarning("SFU3291"); // 이전에 투입한 LOT의 제품과 다릅니다.
                                return false;
                            }
                        }

                        if (lot_eqsgid != null && lot_eqsgid != "") // 이전 투입 lot의 라인과 같은지 비교
                        {
                            if (lot_eqsgid != lot_eqsg)
                            {
                                // Util.AlertInfo("이전에 투입한 LOT의 라인과 다릅니다.");
                                ms.AlertWarning("SFU3279", lot_eqsgid, lot_eqsg); // 입력오류 : 입력한 LOT의 %1 라인이 작업 대기중인 LOT의 %1 라인과 다릅니다.
                                return false;
                            }
                        }

                        // 포장공정의 공정 id 찾기
                        DataTable dtPROC = new DataTable();
                        dtPROC.TableName = "RQSTDT";
                        dtPROC.Columns.Add("ROUTID", typeof(string));
                        // dtPROC.Columns.Add("PROCTYPE", typeof(string));

                        DataRow drPROC = dtPROC.NewRow();
                        drPROC["ROUTID"] = lot_route;
                        // drPROC["PROCTYPE"] = "B"; // 포장공정 타입

                        dtPROC.Rows.Add(drPROC);

                        DataTable dtdtPROCResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ENDPROCID_PACK", "INDATA", "OUTDATA", dtPROC);

                        if (dtdtPROCResult == null || dtdtPROCResult.Rows.Count == 0)
                        {
                            // Util.AlertInfo("포장공정을 찾을수 없습니다.");
                            ms.AlertWarning("SFU3384"); // 포장공정을 찾을수 없습니다.
                            return false;
                        }

                        lot_proc = dtdtPROCResult.Rows[0]["PROCID"].ToString();
                        lot_prod = lot_prodid;
                        lot_eqsgid = lot_eqsg;
                        lot_class_old = lot_class;

                        txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid + " : " + lot_proc;

                        return true;
                    }
                    else
                    {
                        // Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                        ms.AlertWarning("SFU3295"); // 입력오류 : LOT의 상태가 'TERM'이거나 이미 포장됐습니다. [LOT의 상태 확인]
                        return false;
                    }
                }
                else
                {
                    // Util.AlertInfo("포장 불가능한 LOT입니다.");
                    ms.AlertWarning("SFU3295"); // 입력오류 : LOT의 상태가 'TERM'이거나 이미 포장됐습니다. [LOT의 상태 확인]
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation_BR()
        {
            try
            {
                // lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    // 투입LOT(처음 LOT)
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtLOTID); // LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = Util.GetCondition(txtBoxingProd);
                dr["PRDT_CLSS"] = scan_class == "" ? null : scan_class;
                dr["EQSGID"] = scan_eqsgid == "" ? null : scan_eqsgid;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                // LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();
                    string lot_woid = dtResult.Rows[0]["WOID"].ToString();

                    lot_proc = lot_procid;
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;
                    lot_wo = lot_woid;

                    txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    // Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                    ms.AlertWarning("SFU3295"); // 입력오류 : LOT의 상태가 'TERM'이거나 이미 포장됐습니다. [LOT의 상태 확인].
                    return false;
                }
            }
            catch (Exception ex)
            {
                // reSet();

                // chkBoxId.IsChecked = true;
                // chkBoxId.Visibility = Visibility.Visible;
                // txtBoxId.IsEnabled = false;
                // txtBoxId.IsReadOnly = true;

                throw ex;
            }
        }

        private void boxValidation()
        {
            // 이전 포장 작업 유무
            if (boxingYn)
            {
                // Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                ms.AlertWarning("SFU3308"); // 작업오류 : 포장중인 작업이 있습니다. [포장완료 후 처리]
                return;
            }

            // 입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBOXID.Text;
                dr["BOXTYPE"] = "BOX"; // "BOX";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    ms.AlertWarning("SFU1180"); // BOX 정보가 없습니다.
                    return;
                }

                // box_prod = dtResult.Rows[0]["CONF_PRODID"].ToString(); // dtResult.Rows[0]["PRODID"].ToString(); // box의 prod 담아둠.
                // box_eqsg = dtResult.Rows[0]["PACK_EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtResult.Rows[0]["PACK_EQSGID"].ToString(); // box의 eqsgid 담아둠.

                scan_eqsgid_b = dtResult.Rows[0]["PACK_EQSGID"].ToString();
                scan_prodid_b = dtResult.Rows[0]["PRODID"].ToString();
                scan_pcsgid_b = dtResult.Rows[0]["PCSGID"].ToString();
                scan_class_b = dtResult.Rows[0]["PRDT_CLSS_CODE"].ToString();
                scan_model_b = dtResult.Rows[0]["PRJT_ABBR_NAME"].ToString();
                pack_wotype = dtResult.Rows[0]["WOTYPE"].ToString();

                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환")) // 히스토리 클릭해서 온 경우
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") // if (drw["BOXSTAT"].ToString() == "PACKED") // BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            chkBoxIDAutoCreate.IsChecked = true;

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("BOXID", typeof(string));
                            RQSTDT1.Columns.Add("LANGID", typeof(string));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["BOXID"] = txtBOXID.Text;
                            dr1["LANGID"] = LoginInfo.LANGID;

                            RQSTDT1.Rows.Add(dr1);

                            DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                            txtcnt.Visibility = Visibility.Hidden;
                            btnUnPack.Visibility = Visibility.Visible;
                            btnRePack.Visibility = Visibility.Visible;
                            btnSelectCacel.Visibility = Visibility.Collapsed;
                            btncancel.Visibility = Visibility.Collapsed;
                            txtLOTID.IsEnabled = false;
                            chkBoxIDAutoCreate.IsChecked = false;
                            chkBoxIDAutoCreate.Visibility = Visibility.Hidden;

                            dgBoxLot.ItemsSource = null;
                            dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLots);

                            First_eqsgid = dtBoxLots.Rows[0]["EQSGID"].ToString();
                            First_prodid = dtBoxLots.Rows[0]["PRODID"].ToString();
                            First_routid = "";
                            First_flowid = "";
                            First_pcsgid = dtBoxLots.Rows[0]["PCSGID"].ToString();
                            First_class = dtBoxLots.Rows[0]["CLASS"].ToString();
                            First_model = dtBoxLots.Rows[0]["MODEL"].ToString();

                            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtBoxLots.Rows.Count));

                            // (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 0;

                        }
                    }
                }
                else if (btnPack.Content.ToString() != ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED") // if (drw["BOXSTAT"].ToString() == "PACKED") // BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            // Util.AlertInfo("작업불가! 이미포장된BOX입니다.");
                            ms.AlertWarning("SFU3315"); // 입력오류 : 입력한 BOX는 포장완료 된 BOX입니다.[BOX 정보 확인].
                            return;
                        }
                    }

                    // BOX의 오더와 제품이 등록됐는지 확인.
                    if (BoxWoInform())
                    {
                        txtReworkWOID.Text = dtBoxWoResult.Rows[0]["WOID"].ToString();
                        txtBoxingProd.Text = dtBoxWoResult.Rows[0]["PRODID"].ToString();
                        txtBoxingPcsg.Text = dtBoxWoResult.Rows[0]["PCSGID"].ToString();

                        txtregisterWO.Text = txtReworkWOID.Text;

                        wo_prodid = Util.GetCondition(txtBoxingProd);
                        box_prod = wo_prodid;
                    }
                    else
                    {
                        ms.AlertWarning("SFU3454"); // 포장오더를 선택한 후 다시 스캔하세요
                        txtReworkWOID.Text = "";
                        return;
                    }

                    txtBoxingModel.Text = scan_model_b;
                    txtProdClass.Text = scan_class_b;

                    boxingYn = true;
                    boxing_lot = txtBOXID.Text.ToString();

                    // boxing 가능 수량 세팅 필요
                    setBoxLotCnt();
                    boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                    lotCountReverse = boxLotmax_cnt;

                }

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool BoxWoInform()
        {
            try
            {
                bool woYn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBOXID.Text;
                dr["BOXTYPE"] = "BOX"; // "BOX";

                RQSTDT.Rows.Add(dr);

                dtBoxWoResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_PROD_WO_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtBoxWoResult.Rows.Count > 0)
                {
                    woYn = true;
                }

                return woYn;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool boxValidation_accept(string boxid)
        {
            // 입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxid;
                dr["BOXTYPE"] = "BOX"; // "BOX";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    ms.AlertWarning("SFU1180"); // BOX 정보가 없습니다.
                    return false;
                }

                foreach (DataRow drw in dtResult.Rows)
                {
                    if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX")
                    {
                        // Util.AlertInfo("이미 포장해제된 BOX입니다.");
                        ms.AlertWarning("SFU3316"); // 작업오류 : 선택한 BOX는 이미 포장해제된 BOX입니다.[BOX 정보 확인]
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck(string gubun)
        {
            if (gubun == "box")
            {
                return false;
            }
            else
            {
                DataRowView rowview = null;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxLot.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLOTID.Text.ToString())
                        {
                            // Util.AlertInfo("이미 포장 리스트에 있는 LOTID입니다.");
                            ms.AlertWarning("SFU2014"); // 해? LOT이 이미 존재합니다.
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        private void addGridLot_CWA(string sLOTID)
        {
            // 2018.02.26
            DataTable dtErpTran = getErpTrnaInform(Util.GetCondition(txtLOTID));

            string erp_trnf_seqno = string.Empty;
            string woid = string.Empty;
            string rwk_flag = string.Empty;

            if (dtErpTran == null || dtErpTran.Rows.Count == 0)
            {
                erp_trnf_seqno = "";
                woid = "";
                rwk_flag = "N";
            }
            else
            {
                if (dtErpTran.Rows[0]["UNPACK_FLAG"].ToString() == "Y" || dtErpTran.Rows[0]["UNPACK_FLAG"].ToString() == "")
                {
                    erp_trnf_seqno = "";
                    woid = "";
                }
                else
                {
                    erp_trnf_seqno = dtErpTran == null ? "" : dtErpTran.Rows[0]["RESULT_TRNF_SEQ"].ToString();
                    woid = dtErpTran == null ? "" : dtErpTran.Rows[0]["RESULT_WOID"].ToString();
                }

                if (dtErpTran.Rows[0]["RESULT_RWK"].ToString() == "Y")
                {
                    rwk_flag = "Y";
                }
                else
                {
                    if (dtErpTran.Rows[0]["RCV_LOT"] != null && dtErpTran.Rows[0]["RCV_LOT"].ToString() != "")
                    {
                        rwk_flag = "Y";
                    }
                    else
                    {
                        rwk_flag = "N";
                    }
                }
            }

            if (boxingLot_idx == 0)
            {
                // 그리드에 추가하기 전에 LOT의 실적전송 유무 정보를 가져옴.

                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("CHK", typeof(bool));
                dtBoxLot.Columns.Add("BOXID", typeof(string));
                dtBoxLot.Columns.Add("LOTID", typeof(string));
                dtBoxLot.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                dtBoxLot.Columns.Add("WOID", typeof(string));
                dtBoxLot.Columns.Add("RWK_FLAG", typeof(string));
                dtBoxLot.Columns.Add("OCOP_RTN_FLAG", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["CHK"] = true;
                dr["BOXID"] = Util.GetCondition(txtBOXID);
                // 2018.02.26
                // dr["LOTID"] = Util.GetCondition(txtLotId);
                dr["LOTID"] = sLOTID;
                dr["ERP_TRNF_SEQNO"] = erp_trnf_seqno;
                dr["WOID"] = woid;
                dr["RWK_FLAG"] = rwk_flag;
                dr["OCOP_RTN_FLAG"] = scan_ocopyn;

                dtBoxLot.Rows.Add(dr);

                dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                boxingYn = true;
            }
            else
            {
                int TotalRow = dgBoxLot.Rows.Count - 1; // 헤더제거

                dgBoxLot.EndNewRow(true);
                DataGridRowAdd(dgBoxLot);

                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "CHK", "true");
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "BOXID", Util.GetCondition(txtBOXID));
                // 2018.02.26
                // DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId));
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "LOTID", sLOTID);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "ERP_TRNF_SEQNO", erp_trnf_seqno);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "WOID", woid);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "RWK_FLAG", rwk_flag);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "OCOP_RTN_FLAG", scan_ocopyn);
            }
            boxingLot_idx++;
            lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
        }

        private void addGridLot()
        {
            DataTable dtErpTran = getErpTrnaInform(Util.GetCondition(txtLOTID));

            string erp_trnf_seqno = string.Empty;
            string woid = string.Empty;
            string rwk_flag = string.Empty;

            if (dtErpTran == null || dtErpTran.Rows.Count == 0)
            {
                erp_trnf_seqno = "";
                woid = "";
                rwk_flag = "N";
            }
            else
            {
                if (dtErpTran.Rows[0]["UNPACK_FLAG"].ToString() == "Y" || dtErpTran.Rows[0]["UNPACK_FLAG"].ToString() == "")
                {
                    erp_trnf_seqno = "";
                    woid = "";
                }
                else
                {
                    erp_trnf_seqno = dtErpTran == null ? "" : dtErpTran.Rows[0]["RESULT_TRNF_SEQ"].ToString();
                    woid = dtErpTran == null ? "" : dtErpTran.Rows[0]["RESULT_WOID"].ToString();
                }

                if (dtErpTran.Rows[0]["RESULT_RWK"].ToString() == "Y")
                {
                    rwk_flag = "Y";
                }
                else
                {
                    if (dtErpTran.Rows[0]["RCV_LOT"] != null && dtErpTran.Rows[0]["RCV_LOT"].ToString() != "")
                    {
                        rwk_flag = "Y";
                    }
                    else
                    {
                        rwk_flag = "N";
                    }
                }
            }

            if (boxingLot_idx == 0)
            {
                // 그리드에 추가하기 전에 LOT의 실적전송 유무 정보를 가져옴.

                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("CHK", typeof(bool));
                dtBoxLot.Columns.Add("BOXID", typeof(string));
                dtBoxLot.Columns.Add("LOTID", typeof(string));
                dtBoxLot.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                dtBoxLot.Columns.Add("WOID", typeof(string));
                dtBoxLot.Columns.Add("RWK_FLAG", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["CHK"] = true;
                dr["BOXID"] = Util.GetCondition(txtBOXID);
                dr["LOTID"] = Util.GetCondition(txtLOTID);
                dr["ERP_TRNF_SEQNO"] = erp_trnf_seqno;
                dr["WOID"] = woid;
                dr["RWK_FLAG"] = rwk_flag;

                dtBoxLot.Rows.Add(dr);

                dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                boxingYn = true;
            }
            else
            {
                int TotalRow = dgBoxLot.Rows.Count - 1; // 헤더제거

                dgBoxLot.EndNewRow(true);
                DataGridRowAdd(dgBoxLot);

                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "CHK", "true");
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "BOXID", Util.GetCondition(txtBOXID));
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLOTID));
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "ERP_TRNF_SEQNO", erp_trnf_seqno);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "WOID", woid);
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "RWK_FLAG", rwk_flag);
            }
            boxingLot_idx++;
            lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
        }

        private DataTable getErpTrnaInform(string lotid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LOTID", typeof(string));     //

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = lotid;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_TRAN_INFO", "INDATA", "OUTDATA", RQSTDT);

            return dtResult;
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void boxingEnd()
        {
            try
            {
                string boxID = Util.GetCondition(txtBOXID);
                int lot_total_qty;
                string rebox_lotbox_ckh = string.Empty;
                string gubun = string.Empty; // box포장인지 lot포장인지 구분

                string eqsg = string.Empty;
                string proc = string.Empty;

                if (reBoxing) // 재포장
                {
                    eqsg = seleted_Box_Eqsgid;
                    proc = seleted_Box_Procid;

                    // if(!reBoxingCanYn())
                    // {
                    // ms.AlertWarning("포장해제 실적이 전송되지 않았습니다. 잠시후 다시 시도하세요");// 추가
                    // return;
                    // }
                }
                else// 최초포장
                {
                    eqsg = lot_eqsgid;

                    if (Util.GetCondition(txtProdClass) == "CMA")
                    {
                        proc = "P5500";
                    }
                    else if (Util.GetCondition(txtProdClass) == "BMA")
                    {
                        proc = "9500";
                    }

                }

                lot_total_qty = dgBoxLot.GetRowCount();

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  // INPUT TYPE (UI OR EQ)
                INDATA.Columns.Add("LANGID", typeof(string));   // LANGUAGE ID
                INDATA.Columns.Add("BOXID", typeof(string));    // 투이LOT(처음 LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   // 공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("BOXQTY", typeof(string));   // 투입 총수량
                INDATA.Columns.Add("EQSGID", typeof(string));   // 라인ID
                INDATA.Columns.Add("USERID", typeof(string));   // 사용자ID
                INDATA.Columns.Add("WOID", typeof(string));   // 사용자ID
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = boxID;
                dr["PROCID"] = proc;
                dr["BOXQTY"] = lot_total_qty;
                dr["EQSGID"] = eqsg;
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = Util.GetCondition(txtReworkWOID);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                if (reBoxing) // 재포장중이면 추가된 LOT(체크 박스 선택된) 파라미터 생성
                {
                    foreach (object added in dgBoxLot.GetAddedItems())
                    {
                        if (DataTableConverter.GetValue(added, "CHK").Equals("true"))
                        {
                            DataRow inDataDtl = IN_LOTID.NewRow();

                            inDataDtl["LOTID"] = DataTableConverter.GetValue(added, "LOTID");
                            IN_LOTID.Rows.Add(inDataDtl);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                    {
                        string sLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["LOTID"].Index).Value);

                        DataRow inDataDtl = IN_LOTID.NewRow();
                        inDataDtl["LOTID"] = sLotId;
                        IN_LOTID.Rows.Add(inDataDtl);
                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING_WO", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show("포장이 완료 되었습니다.", null, "완료", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageInfo("SFU3386");// 포장이 완료되었습니다.
                    // ms.AlertInfo("SFU3386"); // 포장이 완료되었습니다.
                    // boxID

                    SearchBox(boxID, false);

                }
                else
                {
                    throw new Exception("SFU3462"); // 포장 작업 실패 BOXING BIZ 확인 하세요.
                }

            }
            catch (Exception ex)
            {
                // Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private bool reBoxingCanYn()
        {
            try
            {
                bool Yn = true;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("BOXID", typeof(string));
                dtRqst.Columns.Add("BOXTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);
                dtRqst.Rows[0]["BOXID"] = Util.GetCondition(txtBOXID);
                dtRqst.Rows[0]["BOXTYPE"] = "BOX";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["ERP_IF_FLAG"].ToString() == "C") // 재포장인 경우 ER_IF_FLAG가 G인 것만 재포장 가능(ERP 포장해제 실적이 전송된 경우임)
                    {
                        Yn = false;
                    }
                }

                return Yn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void labelTest()
        {
            try
            {
                // x,y 가져오기
                DataTable dt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (dt.Rows.Count > 0)
                {
                    startX = dt.Rows[0]["X"].ToString();
                    startY = dt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                // for (int i = 0; i < inData.Rows.Count; i++)
                // {

                dtRqst.Rows[0]["LBCD"] = "LBL0017";
                dtRqst.Rows[0]["PRMK"] = "Z";
                dtRqst.Rows[0]["RESO"] = "203";
                dtRqst.Rows[0]["PRCN"] = "1";
                dtRqst.Rows[0]["MARH"] = startX;
                dtRqst.Rows[0]["MARV"] = startY;
                dtRqst.Rows[0]["ATTVAL001"] = "REF : 295B93949R__C";// inData.Rows[i]["MODELID"].ToString();
                dtRqst.Rows[0]["ATTVAL002"] = "966Wh";// inData.Rows[i]["LOTID"].ToString();
                dtRqst.Rows[0]["ATTVAL003"] = "LOT0000001";// inData.Rows[i]["WIPQTY"].ToString();
                dtRqst.Rows[0]["ATTVAL004"] = "11111111";// inData.Rows[i]["RESNNAME"].ToString();
                dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL006"] = "";// dtExpected.SelectedDateTime.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL007"] = "";// inData.Rows[i]["PERSON"].ToString();
                dtRqst.Rows[0]["ATTVAL008"] = "";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                try
                {
                    CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(dtRslt.Rows[0]["LABELCD"].ToString());
                    wndPopup.Show();
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
                // }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // 외포장 Box 출하예정일 Update (중국 NA만 적용)
        private void SetShippingDate()
        {
            if (!LoginInfo.CFG_SHOP_ID.Equals("G451") || !LoginInfo.CFG_AREA_ID.Equals("P4"))
            {
                return;
            }

            string boxLabelCode = Util.NVC(cboLabelCode.SelectedValue).Length > 0 ? Util.NVC(cboLabelCode.SelectedValue) : "LBL0021";

            // Validation 염규범거 참조
            if (boxLabelCode == "LBL0206" && txtPltId.Text.Length <= 0)
            {
                return;
            }

            // 외포장 Box 출하예정일 Update
            string bizRuleName = "BR_PRD_REG_PALLET_ISS_SCHD_DATE";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("BOXID", typeof(string));
                dtINDATA.Columns.Add("PALLETID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("ISS_SCHD_DATE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["BOXID"] = this.txtBoxIdR.Text;
                drINDATA["PALLETID"] = this.txtPltId.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["ISS_SCHD_DATE"] = this.dtpIssueScheduleDate.SelectedDateTime.ToString("yyyyMMdd");
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 라벨 발행
        private void labelPrint()
        {
            try
            {
                string print_cnt = string.Empty;
                string print_yn = string.Empty;
                string prodID = string.Empty;
                string boxID = string.Empty;
                DataTable dtzpl;

                // 2022.11.24 염규범
                // ASN 정보 OWMS 전송건으로 인한 라벨 코드 선 조회형식으로 변경
                // zpl 가져오기
                // string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY,string sPRODID
                // dtzpl = Util.getZPL_Pack(boxID, null, null, null, "PACK_INBOX", "LBL0021", "N", "1", prodID, null);
                string boxLabelCode = Util.NVC(cboLabelCode.SelectedValue).Length > 0 ? Util.NVC(cboLabelCode.SelectedValue) : "LBL0021";

                // NA 염규범
                if (boxLabelCode == "LBL0206" && LoginInfo.CFG_AREA_ID.Equals("P4") && txtPltId.Text.Length <= 0)
                {
                    Util.MessageInfo("SFU9011");
                    return;
                }

                // 재발행 신규 발행 구분 : PRODID 구분해서 뿌려줌.
                if (rePrint) // 재발행
                {
                    if (boxLabelCode == "LBL0206" && LoginInfo.CFG_AREA_ID.Equals("P4"))
                    {
                        prodID = string.IsNullOrWhiteSpace(unPack_Conf_Prodid) ? unPack_ProdID : unPack_Conf_Prodid;
                        boxID = txtPltId.Text;
                    }
                    else
                    {
                        prodID = unPack_ProdID;
                        boxID = txtBoxIdR.Text;
                    }



                }
                else
                {
                    if (boxLabelCode == "LBL0206" && LoginInfo.CFG_AREA_ID.Equals("P4"))
                    {
                        prodID = string.IsNullOrWhiteSpace(unPack_Conf_Prodid) ? unPack_ProdID : unPack_Conf_Prodid;
                        boxID = txtPltId.Text;
                    }
                    else
                    {
                        prodID = "";
                        boxID = txtBOXID.Text;
                    }

                }



                dtzpl = Util.getZPL_Pack(sLOTID: boxID
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: boxLabelCode// null /*"LBL0020"*/
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: prodID
                                        , sSHIPTO_ID: shipTo_ID
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);
                // 라벨 발행
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                // CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                // wndPopup.Show();

                if (!rePrint)
                {
                    return;
                }

                // 재발행 일 경우 처리 : 기존 발행 정보 확인
                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")// print 여부 N인경우 Y로 update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }

                // Button btn = sender as Button;

                // if (btn.Name == "btnBoxLabel") // 재발행
                // {
                // DataTable dtWipList = getWipList();
                // string lotId = Util.GetCondition(txtBoxIdR);

                // Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1");

                // updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString());

                // showLabelPrintInfoPopup(lotId, lotId);
                // }
                // else
                // {
                // string lotId = Util.GetCondition(txtBoxId);

                // Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1"); // 라벨 출력

                // if (btn.Name == "btnPack" && btn.Content.ToString() == "포장")
                // {
                //     DataTable dtWipList = getWipList();
                //     updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString()); // 이력테이블 update

                //     showLabelPrintInfoPopup(lotId, lotId);
                // }

                // }


                /* REAL 버전
                                string print_cnt = string.Empty;
                                string print_yn = string.Empty;

                                Util.printLabel_Pack(FrameOperation, loadingIndicator, txtBoxIdR.Text, "PACK", "N", "1");

                                // 재발행 일 경우 처리 : 기존 발행 정보 확인
                                DataTable dtBoxPrintHistory = setBoxResultList();

                                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                                {
                                    return;
                                }

                                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")// print 여부 N인경우 Y로 update
                                {
                                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                                }
                  */

                // 테스트 버전
                // DataTable dtResult = getZPL_Pack(txtBoxIdR.Text, null, null, null, "", "", "N", "1");
                // DataTable dtResult = getZPL_Pack(null, null, null, null, "PACK", "LBL0017", "N", "1"); // AMDAU0068A

                // for (int i = 0; i < dtResult.Rows.Count; i++)
                // {
                // string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                // CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                // wndPopup.Show();

                // Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                // }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable getZPL_Pack(string sBOXID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY)
        {
            DataTable dtResult = null;
            try
            {

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("PRN_QTY", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBOXID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["PRN_QTY"] = sPRN_QTY;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;

                INDATA.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
                // new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void control_Init()
        {
            if (!boxingYn) // 포장완료 인 경우
            {
                txtBOXID.Text = "";
                txtLOTID.Text = "";
                boxing_lot = "";

                lot_prod = "";
                lot_proc = "";
                lot_eqsgid = "";
                lot_class_old = "";
                txtBoxInfo.Text = "";

                box_prod = "";

                boxingLot_idx = 0;
                btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");
                nbBoxingCnt.Value = 5;
                lotCountReverse = 5;

                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                chkBoxIDAutoCreate.IsChecked = false;
                txtBOXID.IsReadOnly = false;
                // boxLotmax_cnt = 0;
                Util.gridClear(dgBoxLot); // 그리드 clear
            }
        }

        private void boxingStatInit()
        {
            boxingYn = false; // 대기나 완료 상태로.
            boxingLot_idx = 0;
            boxLotmax_cnt = 5; // boxid에 수량 정보 담고 있음. 현재는 미정의 되어 있어서 테스트용으로 5를 세팅
        }

        private void pack_unPack_init(object sender)
        {
            try
            {
                Button btn = sender as Button;
                string unpack_boxid = string.Empty;

                string boxid = Util.GetCondition(txtBOXID);
                string prod = string.Empty;
                string eqsg = string.Empty;
                string eqpt = string.Empty;
                string proc = string.Empty;
                string prd_class = string.Empty;
                string selected_wo = string.Empty;
                int iCheckcnt = 0; // 체크 박스 선택된 수량
                DataSet dsResult = null;


                if (btn.Name == "btnUnPack")
                {
                    unpack_boxid = Util.GetCondition(txtBOXID);

                    prod = seleted_Box_Prod;
                    eqsg = seleted_Box_Eqsgid;
                    eqpt = seleted_Box_Eqptid;
                    proc = seleted_Box_Procid;
                    prd_class = seleted_Box_PrdClass;
                    selected_wo = seleted_Box_Woid;
                }
                else // 이력조회에서 선택된 포장 전체 포장 해제
                {
                    unpack_boxid = Util.GetCondition(txtBoxIdR);
                    prod = unPack_ProdID;
                    eqsg = unPack_EqsgID;
                    eqpt = unPack_EqptID;
                    proc = unPack_ProcID;
                    prd_class = unPack_PrdClasee;
                    selected_wo = unPack_woID;
                }

                unPack_orderCheck(prd_class, eqsg);

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ERP_IF_FLAG", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = unpack_boxid;
                dr["PRODID"] = prod;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                dr["WOID"] = register_wo; // 등록된 wo


                DataTable INLOT = indataSet.Tables.Add("INLOT");
                INLOT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        string sLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["LOTID"].Index).Value);
                        iCheckcnt++;

                        DataRow dr2 = INLOT.NewRow();
                        dr2["LOTID"] = sLotId;
                        INLOT.Rows.Add(dr2);
                    }
                }

                if (btn.Name == "btnUnPack" && iCheckcnt == 0)
                {
                    ms.AlertInfo("PSS9073"); // 선택된 LOT이 없습니다.
                    return;
                }

                dr["UNPACK_QTY"] = btn.Name == "btnPacCancel" ? DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT") : iCheckcnt;
                dr["UNPACK_QTY2"] = btn.Name == "btnPacCancel" ? DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT") : iCheckcnt;
                INDATA.Rows.Add(dr);

                // 전체 포장 해제시 "INLOT" 없이 포장 해제 biz 실행
                if (dgBoxLot.GetRowCount() == iCheckcnt
                     || btn.Name == "btnPacCancel")
                {
                    indataSet.Tables.Remove("INLOT");
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", indataSet);
                }
                else
                {
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX", "INDATA,INLOT", "OUTDATA", indataSet);
                }

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    // txtBoxIdR
                    DataTable dt = new DataTable();
                    dt = DataTableConverter.Convert(dgBoxhistory.ItemsSource);

                    dt.AcceptChanges();

                    foreach (DataRow drDel in dt.Rows)
                    {
                        if (drDel["BOXID"].ToString() == txtBoxIdR.Text.ToString())
                        {
                            drDel.Delete();
                            break;
                        }
                    }

                    dt.AcceptChanges();

                    Util.GridSetData(dgBoxhistory, dt, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dt.Rows.Count));


                    if (btn.Name == "btnUnPack")
                    {
                        /* 20200721 | 김건식S | 포장 해제시 재포장 기능 추가 생성으로 포장 버튼 활성화 로직 주석 처리함.
                         *
                        txtcnt.Visibility = Visibility.Visible;
                        btnUnPack.Visibility = Visibility.Hidden;
                        btnRePack.Visibility = Visibility.Hidden;

                        btnSelectCacel.Visibility = Visibility.Visible;
                        btncancel.Visibility = Visibility.Visible;
                        btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

                        // boxingLot_idx = 0;
                        boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                        lotCountReverse = boxLotmax_cnt - boxingLot_idx;

                        // dgBoxLot.ItemsSource = null;
                        txtLotId.IsEnabled = true;
                        (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;

                        boxingYn = true;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                        */


                        ms.AlertInfo("SFU3389"); // 포장이 해제 되어 재포장 가능합니다.

                        // 부분 포장 해제가 끝난후 LOT 수량 관련 재 설정
                        boxLotmax_cnt = dgBoxLot.GetRowCount() - iCheckcnt;
                        boxingLot_idx = dgBoxLot.GetRowCount() - iCheckcnt;
                        lotCountReverse = boxLotmax_cnt - boxingLot_idx;
                        nbBoxingCnt.Value = boxLotmax_cnt;
                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");

                    }
                    else
                    {
                        ms.AlertInfo("SFU3390"); // UNPACK되었습니다.
                    }

                    // 전체 포장일경우에는 작업자 입력 화면 초기화
                    if (dgBoxLot.GetRowCount() == iCheckcnt
                        || btn.Name == "btnPacCancel")
                    {
                        bResetYN = true;
                        Reset();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private DataTable getWipList()
        {
            try
            {
                // DA_PRD_SEL_WIP_PACK_ROUTE
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                dr["TOPCNT"] = 10;
                RQSTDT.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYLOT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void showLabelPrintInfoPopup(string sLotid, string sMLotid)
        {
            try
            {
                PACK001_002_PRINTINFOMATION popup = new PACK001_002_PRINTINFOMATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("MLOTID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["MLOTID"] = sMLotid;
                    dtData.Rows.Add(newRow);

                    // ========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    // ========================================================================

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // / <summary>
        // / TB_SFC_LABEL_PRT_REQ_HIST
        // / PRT_FLAG = 'Y' 로 UPDATE
        // / </summary>
        // / <param name="sScanid"></param>
        // / <param name="sPRT_SEQ"></param>
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
        {
            try
            {
                // DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = Util.NVC_Int(sPRT_SEQ);
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBoxResultList()
        {
            try
            {
                // DA_PRD_SEL_BOX_LIST_FOR_LABEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = null;
                dr["EQPTID"] = null;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["BOXID"] = Util.GetCondition(txtBoxIdR) == "" ? null : Util.GetCondition(txtBoxIdR);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Reset()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnRePack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");

            boxingLot_idx = 0;
            nbBoxingCnt.Value = 5;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            dgBoxLot.ItemsSource = null;
            (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            txtLOTID.IsEnabled = true;
            chkBoxIDAutoCreate.IsChecked = true;
            chkBoxIDAutoCreate.Visibility = Visibility.Visible;
            txtBOXID.IsEnabled = false;

            boxingYn = false;
            reBoxing = false;
            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "대기중");
            txtBOXID.Text = string.Empty;
            txtLOTID.Text = string.Empty;
            txtBoxIdR.Text = string.Empty;

            lot_prod = string.Empty;
            lot_proc = string.Empty;
            lot_eqsgid = string.Empty;
            lot_class_old = string.Empty;
            txtBoxInfo.Text = string.Empty;

            boxing_lot = string.Empty;
            box_prod = string.Empty;

            scan_eqsgid = string.Empty;
            scan_prodid = string.Empty;
            scan_routid = string.Empty;
            scan_flowid = string.Empty;
            scan_pcsgid = string.Empty;
            scan_class = string.Empty;
            scan_model = string.Empty;

            txtBoxingProd.Text = string.Empty;
            txtBoxingPcsg.Text = string.Empty;
            txtBoxingModel.Text = string.Empty;
            txtReworkWOID.Text = string.Empty;
            txtProdClass.Text = string.Empty;
        }

        private void buttonAccess(object sender)
        {
            try
            {
                Button btn = sender as Button;

                string grid_name = "dgBoxhistory";

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;

                        grid_name = presenter.DataGrid.Name;
                    }
                }

                DataRowView drv = row.DataItem as DataRowView;

                seleted_boxid = drv["BOXID"].ToString();


                // txtBoxIdR
                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgBoxhistory.ItemsSource);

                dt.AcceptChanges();

                foreach (DataRow dgBoxHistoryDr in dt.Rows)
                {
                    if (dgBoxHistoryDr["BOXID"].ToString() == seleted_boxid)
                    {
                        dgBoxHistoryDr.Delete();
                        break;
                    }
                }
                dt.AcceptChanges();

                Util.GridSetData(dgBoxhistory, dt, FrameOperation);


                // 그리드에서 적용 버튼 클릭해서 넘어온 경우 : 이미 포장 해제 된 box 일 수 있으므로 확인해줌.
                if (btn.Name == "btnAccept")
                {
                    if (!boxValidation_accept(seleted_boxid))
                    {
                        return;
                    }
                }

                if (!boxingYn)
                {
                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장작업전환");
                    txtBOXID.Text = seleted_boxid;
                    SetBoxkeyDown();
                }
                else
                {
                    // Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                    ms.AlertWarning("SFU3308"); // 작업오류 : 포장중인 작업이 있습니다. [포장완료 후 처리]
                }

                if (drv["PALLETID"] != null)
                {
                    seleted_palletID = drv["PALLETID"].ToString();
                }

                seleted_Box_Prod = drv["PRODID"].ToString(); // BOX의 제품
                seleted_Box_Procid = drv["PROCID"] == null ? null : drv["PROCID"].ToString(); // BOX의 공정
                seleted_Box_Eqptid = drv["EQPTID"].ToString(); // BOX의 설비
                seleted_Box_Eqsgid = drv["EQSGID"].ToString(); // BOX의 라인
                seleted_Box_PrdClass = drv["PRDCLASS"].ToString(); // BOX 제품군

                seleted_Box_pcsgid = drv["PCSGID"].ToString(); // BOX의 공정군
                seleted_Box_Model = drv["MODEL"].ToString(); // BOX의 모델
                seleted_Box_Woid = drv["WOID"].ToString(); // BOX의 WO

                boxLotmax_cnt = Convert.ToInt32(drv["BOXLOTCNT"]); // 포장가능수량
                boxingLot_idx = Convert.ToInt32(drv["BOXLOTCNT"]); // 포장된 수량

                txtBoxingProd.Text = seleted_Box_Prod;
                txtBoxingPcsg.Text = seleted_Box_pcsgid;
                txtBoxingModel.Text = seleted_Box_Model;
                txtReworkWOID.Text = seleted_Box_Woid;
                txtProdClass.Text = seleted_Box_PrdClass;

                lotCountReverse = boxLotmax_cnt - boxingLot_idx; // 남은수량
                nbBoxingCnt.Value = boxLotmax_cnt;

                txtBoxInfo.Text = seleted_Box_PrdClass + " : " + seleted_Box_Eqsgid + " : " + seleted_Box_Prod;

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");

                unPack_orderCheck(seleted_Box_PrdClass, seleted_Box_Eqsgid);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SearchBox(string strBoxId, Boolean bGridClear)
        {
            try
            {
                //
                DataTable dt = DataTableConverter.Convert(dgBoxhistory.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("BOXID = '" + strBoxId + "'").Count() > 0 && !bGridClear)
                    {
                        Util.MessageInfo("SFU8251");
                        return;
                    }
                }


                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SYSTEM_ID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("BOXID", typeof(string));
                dtINDATA.Columns.Add("FROMDATE", typeof(string));
                dtINDATA.Columns.Add("TODATE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("SHIP_STATCD", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SYSTEM_ID"] = LoginInfo.SYSID;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["PRODID"] = null;
                drINDATA["MODLID"] = null;
                drINDATA["BOXID"] = strBoxId;
                drINDATA["FROMDATE"] = null;
                drINDATA["TODATE"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["SHIP_STATCD"] = null;
                dtINDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", dtINDATA);

                if (dtResult.Rows.Count != 0)
                {
                    if (bGridClear)
                    {
                        txtSearchBox.Text = "";
                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        txtSearchBox.Text = "";

                        Util.gridClear(dgBoxhistory);

                        // dt.Rows.InsertAt(dtResult.Rows[0], 0);

                        // dtResult.AsEnumerable().CopyToDataTable(dt, LoadOption.Upsert);
                        dt.AsEnumerable().CopyToDataTable(dtResult, LoadOption.Upsert);

                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));

                    }

                }
                else
                {
                    Util.MessageInfo("SFU1179");
                }


                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private void btnWorkOroderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if (Util.GetCondition(txtBoxingProd) == "")
                // {
                // return;
                // }

                PACK001_015_WORKORDERSELECT popup = new PACK001_015_WORKORDERSELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("PRODID", typeof(string));
                    dtData.Columns.Add("PRODID_LOT", typeof(string));
                    dtData.Columns.Add("PROD_CLSS_CODE", typeof(string));
                    // 2018.10.12
                    dtData.Columns.Add("EQSGID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["PRODID"] = Util.GetCondition(txtBoxingProd);
                    newRow["PRODID_LOT"] = scan_prodid;
                    newRow["PROD_CLSS_CODE"] = scan_class;
                    // 2018.10.12
                    newRow["EQSGID"] = scan_eqsgid;

                    dtData.Rows.Add(newRow);

                    // ========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    // ========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                // Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void cboLabelCode_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dtBoxLabelResult != null && dtBoxLabelResult.Rows.Count > 0)
            {
                shipTo_ID = dtBoxLabelResult.Rows[cboLabelCode.SelectedIndex]["SHIPTO_ID"].ToString();
            }
            else
            {
                shipTo_ID = null;
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxLot.Rows)
            {

                DataTableConverter.SetValue(row.DataItem, "CHK", true);

            }
            dgBoxLot.EndEdit();
            dgBoxLot.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxLot.Rows)
            {

                DataTableConverter.SetValue(row.DataItem, "CHK", false);

            }
            dgBoxLot.EndEdit();
            dgBoxLot.EndEditRow(true);
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtLOTID.Clear();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLOTID.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLOTID.SelectedText.ToString());
                txtLOTID.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBoxId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtBOXID.Clear();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Box_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBOXID.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Box_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBOXID.SelectedText.ToString());
                txtBOXID.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
