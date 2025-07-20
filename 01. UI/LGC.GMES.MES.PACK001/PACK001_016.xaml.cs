/*************************************************************************************
 Created Date : 2016.06.16
      Creator :
   Decription : Pallet 포장 및 라벨 발행, 재발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.05.28  손우석    : 이력조회 조건에 시간 추가
  2018.06.20  손우석    : BOX포장시 투입되는 LOT과 선택된 W/O 제품의 BOM 내용에 포함되는지 체크 로직 반영으로 W/O 선택 기능 Disable
  2018.07.03  손우석    : WO조회 기능 오픈
  2018.10.02  손우석    : 같은 제품 다른 라인인경우 포장 W/O 구분을위해 EQSGID 파라메터 추가
  2018.10.12  손우석    : 같은 제품 다른 라인인경우 포장 W/O 구분을위해 EQSGID 파라메터 추가
  2019.04.02  손우석    : 포장중 에러 메시지 출력후 초기화 제거
  2019.06.10  손우석    : 작업초기화시 WORKORDER 버튼 숨기기, 라인정보 추가
  2019.06.19  손우석    : 불필요 메시지 제거
  2019.06.21  손우석    : 작업 초기화시 Pallet ID 자동생성 체크 해제 방지 수정
  2019.06.26  손우석    : 메시지 표시 조건 수정
  2019.09.05  김도형    : CMI CUST_ID로 투입 가능 여부 판단 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252
  2019.09.20  손우석    : CMI CUST_ID로 투입 가능 여부 판단 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252
  2019.09.26  김도형    : CMI CUST_ID로 투입 가능 여부 판단 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252
  2019.10.08  손우석    : [CSR ID:4097644] [요청] 출하검사 Hold 건 Box & Pallet 포장 및 해제 Blocking 건 | [요청번호]C20191002_97644
  2019.11.18  손우석    : 제품코드 혼입 오류 수정
  2020.01.06  손우석    : 제품코드 혼입 오류 수정(BOX 단위 구성)
  2020.01.28  손우석    : CSR ID 25026 시스템 개선을 통해 수동 포장시 타 모델 W/O로 오 포장 방지 [요청번호] C20200123-000214
  2020.05.14  손우석    : CSR ID 60447 [생산PI팀] 포장 W/O 설정 및 처리에 대한 인터락 기능 개선 [요청번호] C20200513-000399
  2020.10.22  염규범    : PACK PALLET 포장 Transaction 분산 처리
  2020.10.22  강호운    : 2nd OOCV 공정분리(P5200,P5400) 공정추가
  2021.11.16  김길용    : Pack3동 Carrier - Pallet 구성으로 인한 조회 컬럼추가
  2022.09.21  임성운    : 양식 선택 콤보박스 추가(TAGLIST) 및 Pallet_Tag 프로그램 호출 시 Parameter 전달
  2023.07.17  김건식    : UNPACK (포장해제) 시 불필요 VALIDATION 삭제
  2023.10.25  김민석    : 이력 조회 시 PRODID가 NULL로 들어가던 것을 cboProduct 값을 가져오는 것으로 수정 [요청번호] E20231018-000854
  2024.01.23  김선준    : 포장취소 로직수정. Grid(OQC_INSP_RESULT)컬럼추가 -> 포장취소메시지 -> 포장취소(BIZ통합)
  2024.08.07  정용석    : 포장처리시 포장개수 설정과 입력한 ID의 개수가 다르면 무조건 Interlock [요청번호] E20240717-000988
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_016 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        DataTable dtSearchResult; //조회 결과를 담기위한 DataTable
        DataTable dtOrderResult;  //lot scan시 포장 오더 정보 담기 위한 DataTable
        DataTable dtWoProdResult; //cma의 포장오더에 물려있는 prod와 pcsgid 담기 위한 TAble
        DataTable dtBoxWoResult;
        DataTable dtUnpackBoxs;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        //flag 변수
        bool boxingYn = false;   //true - box 구성중, 미완료 / false - 박스포장 가능
        bool unPackYn = false;   //리스트에서 포장 해재 가능 여부 : true-포장해제 가능 / false-포장해제 불가능
        bool rePrint = false;    //리스트에서 발행 할것인지 포장완료 후 발행 할것인지 : true-재발행 / false-최초발행
        bool reBoxing = false;   //재포장 여부
        bool processYN = false;  //출하검사 체크 후 JUGD_VALUE가 'F' 이면 UNPACK 가능 / 아니면 UNPACK 불가능
        bool cust_palletidYN = false; //고객사 palletid를 관리하는 라인인지 체크

        //수량 관련 변수
        int boxLotmax_cnt = 0;   //PALLET에 담길 BOX 수량 OR LOT 수량의 최대
        int boxingBox_idx = 0;   //PALLET에 담긴 BOX 및 LOT 수량
        int lotCountReverse = 0; //PALLET에 담길 LOT의 남은 수량

        //pallet관련 변수
        string boxing_lot;                 //포장중인 palletid
        string selectrcv = string.Empty;   //출하여부 확인을 위한 변수
        string pallet_prod = string.Empty; //pallet의 prod : 투입되는 box나 lot의 prod와 비교하기 위한 변수
        string pallet_eqsg = string.Empty;
        string pack_wotype;

        //Pallet 선택시 메시지 저장
        string _Msg_OkNg = string.Empty;

        #region lot OR box 투입시 이전 투입한 lot OR box와 비교를 위한 변수
        //투입되는 lot관련 변수
        string lot_prod = string.Empty;       //이전에 투입한 lot의 prodid
        string lot_proc = string.Empty;       //이전에 투입한 lot의 procid
        string lot_eqsgid = string.Empty;     //이전에 투입한 lot의  eqsgid
        string lot_class_old = string.Empty;  //이전에 투입한 lot의 class

        //투입되는 box관련 변수
        string boxID = string.Empty; //내부적으로 생성한 box
        string box_prod = string.Empty; //이전에 투입한 box의 prodid
        string box_proc = string.Empty; //이전에 투입한 box의 procid
        string box_eqsg = string.Empty; //이전에 투입한 box의 eqsgid
        string box_class = string.Empty;//이전에 투입한 box의 class
        #endregion

        #region 재포장을 위한 포장해제 변수
        string seleted_Pallet_Prod = string.Empty;    // 재포장을 위한 prodid
        string seleted_Pallet_Procid = string.Empty;  // 재포장을 위한 procid
        string seleted_Pallet_Eqptid = string.Empty;  // 재포장을 위한 eqptid
        string seleted_Pallet_Eqsgid = string.Empty;  // 재포장을 위한 eqsgid
        string seleted_Pallet_PrdClass = string.Empty;// 재포장을 위한 prdclass
        string seleted_oqc_insp_id = string.Empty;    // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string seleted_judg_value = string.Empty;     // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string seleted_insp_name = string.Empty;      // 재포장을 위해 포장해제시 OQC_INSP_NAME를 초기화 시키기 위한 변수
        string seleted_Pallet_pcsgid = string.Empty;
        string seleted_Pallet_Model = string.Empty;
        string seleted_Pallet_Woid = string.Empty;
        string seleted_Pallet_Prod_C = string.Empty;
        string seleted_Pallet_Pcsgid_C = string.Empty;
        string seleted_Pallet_Eqsgname = string.Empty;
        #endregion

        #region 리스트에서 포장해제를 위한 변수
        string unPack_ProdID = string.Empty;       // 포장해제를 위한 prodid
        string unPack_EqsgID = string.Empty;       // 포장해제를 위한 eqsgid
        string unPack_EqptID = string.Empty;       // 포장해제를 위한 eqptid
        string unPack_ProcID = string.Empty;       // 포장해제를 위한 procid
        string unPack_PrdClasee = string.Empty;    // 포장해제를 위한 prdclass
        string unPack_oqc_insp_id = string.Empty;  // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string unPack_judg_value = string.Empty;   // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string unPack_insp_name = string.Empty;    // 재포장을 위해 포장해제시 OQC_INSP_NAME를 초기화 시키기 위한 변수
        string unPack_lotQty = string.Empty;    // 재포장 리스트에서 포장해제 수량
        #endregion

        #region LOT 최초 스캔시 LOT 정보 담아두기 위한 변수(LOT정보)
        string First_eqsgid = string.Empty;
        string First_prodid = string.Empty;
        string First_routid = string.Empty;
        string First_flowid = string.Empty;
        string First_pcsgid = string.Empty;
        string First_class = string.Empty;
        string First_model = string.Empty;

        // 2019.09.05 그리드에 처음 추가되는 정보 담기 위한 변수 (cust_id 비교용)
        string First_custid = string.Empty;

        #endregion

        #region BOX 최초 스캔시 BOX 정보 담아두기 위한 변수(BOX정보)
        string First_eqsgid_b = string.Empty;
        string First_prodid_b = string.Empty;
        string First_routid_b = string.Empty;
        string First_flowid_b = string.Empty;
        string First_pcsgid_b = string.Empty;
        string First_class_b = string.Empty;
        string First_model_b = string.Empty;
        string First_custid_b = string.Empty;
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
        string scan_custid = string.Empty;
        string scan_ocopyn = string.Empty;
        #endregion

        #region LOT/BOX 최초 스캔시 또는 PALLET 스캔시 WO 정보 담아두기 위한 변수(포장정보)
        string wo_prodid = string.Empty;
        string wo_eqsgid = string.Empty;
        string wo_modlid = string.Empty;
        string wo_class = string.Empty;
        string wo_conf_prodid = string.Empty;
        string wo_woid = string.Empty;
        string wo_pcsgid = string.Empty;
        string wo_procid = string.Empty;
        string wo_model = string.Empty;
        #endregion

        #region Box 스캔시 Box정보를 담아두기 위한 변수(BOX정보)
        string scan_eqsgid_b = string.Empty;
        string scan_prodid_b = string.Empty;
        string scan_routid_b = string.Empty;
        string scan_flowid_b = string.Empty;
        string scan_pcsgid_b = string.Empty;
        string scan_class_b = string.Empty;
        string scan_model_b = string.Empty;
        string scan_procid_b = string.Empty;
        string scan_woid_b = string.Empty;
        string scan_custid_b = string.Empty;
        string scan_eqsgname_b = string.Empty;
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_016()
        {
            InitializeComponent();

            this.Loaded += PACK001_016_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //날자 초기값 세팅
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            //2018.05.28
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbBoxHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            tbLotInform.Text = "SCAN LOT" + ObjectDic.Instance.GetObjectName("정보");

            InitCombo();

            setGubunCbo();
            //Pack3동 Carrier - Pallet 추가 2021.11.16
            if (LoginInfo.CFG_AREA_ID == "PA") dgPallethistory.Columns["CARRIERID"].Visibility = Visibility.Visible;
            //setCustPalletidChk(LoginInfo.CFG_EQSG_ID.Trim()); //고객사 palletid를 관리하는 라인인지 체크 후 고객사palletid 입력 란 오픈
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

            //동
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //모델
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //제품
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            /* ESWA 일부인원 TimeOut 이슈로 Holding  - limsw
            //TAG LIST
            //            _combo.SetCombo(cboTAGLIST, CommonCombo.ComboStatus.SELECT,sCase: "PACK_PALLETTAG_LIST");

            String[] sFilter = { "PACK_PALLETAG_LIST" };
            String[] sFilter1 = { "PACK_PALLETAG_LIST" };
            _combo.SetCombo(cboTAGLIST, CommonCombo.ComboStatus.NA, sFilter: sFilter1, sCase: "COMMCODE");
            */
        }

        private void control_Init()
        {
            if (!boxingYn) //포장완료 인 경우
            {
                txtPalletId.Text = string.Empty;
                txtBoxLOTID.Text = string.Empty;
                boxing_lot = string.Empty;
                boxingBox_idx = 0;

                lot_prod = string.Empty;
                lot_proc = string.Empty;
                lot_eqsgid = string.Empty;
                lot_class_old = string.Empty;
                txtBoxInfo.Text = string.Empty;

                box_prod = string.Empty;
                box_proc = string.Empty;
                box_eqsg = string.Empty;
                box_class = string.Empty;

                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                chkPalletId.IsChecked = false;
                txtPalletId.IsReadOnly = false;

                nbBoxingCnt.Value = 1;
                boxingBox_idx = 0;
                //boxLotmax_cnt = 0;
                Util.gridClear(dgPalletBox); //그리드 clear
            }
        }

        #endregion

        #region Event

        #region 메인 EVENT
        private void PACK001_016_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();
                chkPalletId.IsEnabled = true; // 2024.11.21. 김영국 - Check 박스 무조건 사용하도록 수정함.
                //search();

                this.Loaded -= PACK001_016_Loaded;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }
        #endregion

        #region BUTTON EVENT
        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            if (dgPalletBox.ItemsSource != null)
            {
                for (int i = dgPalletBox.Rows.Count; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgPalletBox.Rows[i - 1].DataItem, "CHK");
                    var box_id = DataTableConverter.GetValue(dgPalletBox.Rows[i - 1].DataItem, "BOXID");

                    if (chkYn == null)
                    {
                        dgPalletBox.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgPalletBox.EndNewRow(true);
                        dgPalletBox.RemoveRow(i - 1);
                        boxingBox_idx--;
                        lotCountReverse++;

                        setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgPalletBox.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            //전체 취소
            try
            {
                if (dgPalletBox.GetRowCount() == 0)
                {
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3406"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                //정말 포장리스트를 삭제 하시겠습니까?
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        dgPalletBox.ItemsSource = null;
                        boxingBox_idx = 0;
                        lotCountReverse = boxLotmax_cnt;
                        //boxingYn = false;

                        tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                        if (txtPalletId.Text.ToString() == "")
                        {
                            boxLotmax_cnt = 0;
                        }

                        setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
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
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            string confirmMessage = string.Empty;
            try
            {
                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    txtcnt.Visibility = Visibility.Visible;
                    btnUnPack.Visibility = Visibility.Hidden;

                    btnSelectCacel.Visibility = Visibility.Visible;
                    btncancel.Visibility = Visibility.Visible;
                    chkPalletId.IsChecked = true;
                    chkPalletId.Visibility = Visibility.Visible;
                    //2019.06.21
                    //chkPalletId.IsEnabled = true;
                    chkPalletId.IsEnabled = false;
                    txtPalletId.IsEnabled = false;

                    txtBoxLOTID.IsEnabled = true;

                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

                    boxingBox_idx = 0;
                    boxLotmax_cnt = 0; // 박싱 가능 수량 세팅 - 정해지면 수정
                    nbBoxingCnt.Value = 1;
                    lotCountReverse = Convert.ToInt32(nbBoxingCnt.Value);
                    tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                    dgPalletBox.ItemsSource = null;
                    //(dgPalletBox.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
                    // (dgPalletBox.Columns["OQC_INSP_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Width = new C1.WPACKPF.DataGrid.DataGridLength(100);
                    //(dgPalletBox.Columns["JUDG_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Width = new C1.WPF.DataGrid.DataGridLength(100);
                    txtBoxLOTID.IsEnabled = true;

                    boxingYn = false;
                    reBoxing = false;

                    setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "대기중");

                    txtPalletId.Text = "";
                    txtBoxLOTID.Text = "";

                    lot_prod = "";
                    lot_proc = "";
                    lot_eqsgid = "";
                    lot_class_old = "";
                    txtBoxInfo.Text = "";

                    box_prod = "";
                    box_proc = "";
                    box_eqsg = "";
                    box_class = "";

                    cust_palletidYN = false;
                    btnSave.Visibility = Visibility.Hidden;
                    btnExcelLoad.Visibility = Visibility.Hidden;
                    setCustPalletidChk("");

                    //search();
                }
                else
                {
                    if (txtPalletId.Text == "")
                    {
                        return;
                    }

                    if (dgPalletBox.GetRowCount() == 0)
                    {
                        return;
                    }

                    if (Util.GetCondition(cboGubun) != "Box ID")  //LOT일경우
                    {
                        if (Util.GetCondition(txtProdClass) == "CMA")
                        {
                            if (Util.GetCondition(txtReworkWOID) == "")
                            {
                                ms.AlertWarning("SFU3452"); // 선택된 W/O가 없습니다.
                                return;
                            }

                            //if (!woDateDiff()) //w/o의 날짜와 현재 날짜 비교(월비교) : 당월이 아니면 포장 못함.
                            //{
                            //    ms.AlertWarning("SFU3573");//포장하려는 W/O가 당월 W/O가 아닙니다.
                            //    return;
                            //}
                        }

                        //PALLET의 포장된 BOX의 취소 실적이 전송됐는지 확인하는 로직
                        if (!reBoxingCanYn()) //재포장시 해제실적이 전송 됐는지 확인
                        {
                            ms.AlertWarning("SFU3453");//포장해제 실적이 전송되지 않았습니다. 잠시후 다시 시도하세요
                            return;
                        }


                        //히스토리에서[적용] 클릭시 W/ O의 타입을 가져와서 비교문 와성해야 됨 #########################################
                        if (pack_wotype == "PPRW") //재작업 WO인데 포장하려는 LOT이 실적이 없는 경우 포장 못함.
                        {
                            bool reworkChk = true;
                            for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgPalletBox.Rows[i].DataItem, "WOID").ToString() == "" ||
                                    DataTableConverter.GetValue(dgPalletBox.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString() == "")
                                {
                                    reworkChk = false; //실적처리 안된 LOT이라서 포장할수 없음.
                                }
                            }

                            if (!reworkChk)
                            {
                                ms.AlertWarning("SFU3572");//재작업 WO로 포장할때 실적처리 안된 LOT은 포장할 수 없습니다.
                                return;
                            }
                        }
                    }
                    /*
                                        if(cust_palletidYN)
                                        {
                                            if(Util.GetCondition(txtConfPalletId) == "")
                                            {
                                                ms.AlertWarning("SFU3612");//GTL(Global Transport Label)을 입력하세요.
                                                txtConfPalletId.Focus();
                                                return;
                                            }
                                        }
                    */
                    if (boxingYn == true && (boxLotmax_cnt == boxingBox_idx))
                    {
                        confirmMessage = ms.AlertRetun("SFU3385"); //포장완료 하시겠습니까?
                    }
                    else if (boxLotmax_cnt != boxingBox_idx)
                    {
                        ms.AlertWarning("SFU3392");
                        return;
                        //confirmMessage = ms.AlertRetun("SFU3392"); //PALLET 포장 수량과 BOX(LOT)의 수량이 일치하지 않습니다.\n포장 완료 하시겠습니까 ?"
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(confirmMessage, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            //cmi의 경우 함수 만들어서 cust_id 하고 lot_id하고 일치하나 확인

                            boxingEnd(); //포장 완료 함수

                            reSet();

                            //search();
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
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgPallethistory);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    ms.AlertWarning("SFU1499"); //동을 선택하세요.
                    return;
                }

                if (!dtDateCompare())
                {
                    return;
                }

                search();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnConfigCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPalleyIdR.Text.Length == 0)
                {
                    return;
                }

                unPack_Process(sender, txtPalleyIdR.Text, unPackYn);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reBoxing = true; //재포장
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnPalletLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rePrint = true; //재발행
                if (txtPalleyIdR.Text.Length > 0)
                {
                    //labelPrint(sender); //pallet는 라벨 발행 안함

                    setTagReport();
                }

                rePrint = false;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPalletId.Text.Length == 0)
                {
                    return;
                }

                bool UNPACKYN = string.IsNullOrEmpty(selectrcv); //false;


                unPack_Process(sender, txtPalletId.Text, UNPACKYN);

                btnSave.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet();
                    //Util.AlertInfo("작업이 초기화 됐습니다.");
                    ms.AlertInfo("SFU3377"); //작업이 초기화 됐습니다.
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3282"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    //작업오류 : 포장중인 작업이 있습니다. 정말 [작업초기화] 하시겠습니까?
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet();
                            //Util.AlertInfo("작업이 초기화 됐습니다.");
                            ms.AlertInfo("SFU3377"); //작업이 초기화 됐습니다.
                        }
                        else
                        {
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnWorkOroderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2019.06.19
                //2019.06.26
                if (scan_prodid == "" && txtBoxingProd.Text == "")
                {
                    ms.AlertWarning("SFU7008");
                    return;
                }

                PACK001_015_WORKORDERSELECT popup = new PACK001_015_WORKORDERSELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("PRODID", typeof(string));
                    dtData.Columns.Add("PRODID_LOT", typeof(string));
                    dtData.Columns.Add("PROD_CLSS_CODE", typeof(string));
                    //2018.10.12
                    dtData.Columns.Add("EQSGID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["PRODID"] = Util.GetCondition(txtBoxingProd);
                    newRow["PRODID_LOT"] = scan_prodid;
                    newRow["PROD_CLSS_CODE"] = scan_class;
                    //2018.10.12
                    newRow["EQSGID"] = scan_eqsgid;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Util.GetCondition(txtPalletId) == "" || Util.GetCondition(txtPalletId).Length == 0)
            {
                return;
            }

            updateCustPalletID(Util.GetCondition(txtPalletId), Util.GetCondition(txtConfPalletId));
        }

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        #endregion

        #region GRID EVENT
        private void dgPallethistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPallethistory.Rows.Count == 0 || dgPallethistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPallethistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                string selectPallet = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PALLETID").ToString();

                txtPalleyIdR.Text = selectPallet;
                #region OQC검사결과
                //포장해체시 검사결과에 따라 Msg다르게 보여주기 위한 설정(2024.01.23 seonjun9)
                object oqc_insp_req_id = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_REQ_ID");
                object oqc_insp_result = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_RESULT");
                if (null != oqc_insp_req_id && oqc_insp_req_id.ToString().Length > 0)
                {
                    if (null != oqc_insp_result && oqc_insp_result.ToString().Equals("F"))
                    {
                        //_Msg_OkNg = "정말 포장취소 하시겠습니까?";
                        _Msg_OkNg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                    }
                    else
                    {
                        //okNg_msg = "출하검사 결과가 [불합격]이 아닙니다.\n계속 포장 취소 하시겠습니까?";
                        _Msg_OkNg = ms.AlertRetun("SFU3322"); //작업오류 : 선택한 PALLET의 출하 검사 결과가 [불합격]이 아닙니다. 포장취소 하시겠습니까?
                    }
                }
                else
                {
                    //_Msg_OkNg = "정말 포장취소 하시겠습니까?";
                    _Msg_OkNg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                }
                #endregion

                object rcv_iss_id = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "RCV_ISS_ID");
                selectrcv = (null == rcv_iss_id) ? string.Empty : rcv_iss_id.ToString();

                //출고 된 박스는 해제 불가능
                unPackYn = string.IsNullOrEmpty(selectrcv);

                unPack_ProdID = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PRODID").ToString();
                unPack_ProcID = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PROCID").ToString();
                unPack_EqptID = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "EQPTID") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "EQPTID").ToString();
                unPack_EqsgID = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "EQSGID") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "EQSGID").ToString();
                unPack_PrdClasee = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "PRDCLASS").ToString();

                unPack_oqc_insp_id = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_REQ_ID") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_REQ_ID").ToString();
                unPack_judg_value = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_RESULT") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "OQC_INSP_RESULT").ToString();
                unPack_lotQty = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "TOTAL_QTY") == null ? null : DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "TOTAL_QTY").ToString();

                setCustPalletidChk(unPack_EqsgID); //고객사 palletid를 관리하는 라인인지 체크 후 고객사palletid 입력 란 오픈

                if (cust_palletidYN)
                {
                    tbConfPalletId.Visibility = Visibility.Hidden;
                    txtConfPalletId.Visibility = Visibility.Hidden;
                    btnSave.Visibility = Visibility.Hidden;
                    btnExcelLoad.Visibility = Visibility.Visible;
                }
                else
                {
                    btnExcelLoad.Visibility = Visibility.Hidden;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //2018.05.28
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPallethistory.GetCellFromPoint(pnt);
                gridDoubleClickProcess(cell, "INFO_BOX");
                //C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;
                //gridDoubleClickProcess(sender, e, grid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void gridDoubleClickProcess(C1.WPF.DataGrid.DataGridCell cell, string sPopUp_Flag)
        {
            try
            {
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }

                        if (cell.Column.Name == "BOXID" || cell.Column.Name == "PALLETID")
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

                                //========================================================================
                                object[] Parameters = new object[1];
                                Parameters[0] = dtData;
                                C1WindowExtension.SetParameters(popup, Parameters);
                                //========================================================================

                                popup.ShowModal();
                                popup.CenterOnScreen();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.05.28
        private void dgPallethistory_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "PALLETID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgPalletBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
                        try
                        {
                            Point pnt = e.GetPosition(null);
                            C1.WPF.DataGrid.DataGridCell cell = dgPalletBox.GetCellFromPoint(pnt);

                            if (cell != null)
                            {
                                if (cell.Column.Name == "LOTID")
                                {
                                    this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                                }

                                if (cell.Column.Name == "BOXID" || cell.Column.Name == "PALLETID")
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

                                        //========================================================================
                                        object[] Parameters = new object[1];
                                        Parameters[0] = dtData;
                                        C1WindowExtension.SetParameters(popup, Parameters);
                                        //========================================================================

                                        //popup.Closed -= popup_Closed;
                                        //popup.Closed += popup_Closed;
                                        popup.ShowModal();
                                        popup.CenterOnScreen();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Alert(ex.Message);
                        }
              */
        }
        #endregion

        #region CHECKBOX EVENT
        private void chkPalletId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkPalletId.IsChecked)
                {
                    if (boxingYn)
                    {
                        chkPalletId.IsChecked = false;
                        txtPalletId.Text = boxing_lot;
                        txtBoxLOTID.Text = "";
                        return;
                    }

                    txtPalletId.IsReadOnly = true;
                    txtPalletId.IsEnabled = false;
                    txtPalletId.Text = "";
                }
                else
                {
                    if (boxingYn)
                    {
                        chkPalletId.IsChecked = true;
                        return;
                    }

                    txtPalletId.IsReadOnly = false;
                    txtPalletId.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }
        #endregion

        #region TEXTBOX EVETN
        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            /*        PALLET는 재활용 안하기 때문에 PALLET 투입은 없음.
            if (e.Key == Key.Enter)
            {
                try
                {
                    SetPalletkeyDown(txtPalletId.Text);
                }
                catch (Exception ex)
                {
                    //Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }
            }
            */
        }

        private void txtBoxLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtPalletId.Text.Trim())) //PALLET id 가 입력 되지 않았고
                    {
                        if ((bool)chkPalletId.IsChecked) // PALLET 체크 박스가 체크 있을경우 PALLETID 자동 생성
                        {
                            #region boxid일경우
                            if (Util.GetCondition(cboGubun) == "Box ID")  //if ((bool)rdoBoxId.IsChecked)
                            {
                                //boxid 입력시 처리 로직
                                //입력된 box validation
                                if (!boxValidation_BR("BOX")) //boxValidation("BOX")
                                {
                                    txtPalletId.Text = "";
                                    return;
                                }

                                txtReworkWOID.Text = scan_woid_b;
                                txtBoxingModel.Text = scan_model_b;
                                txtBoxingPcsg.Text = scan_pcsgid_b;
                                txtBoxingProd.Text = scan_prodid_b;
                                txtProdClass.Text = scan_class_b;
                                //2020.09.30 KIM MIN SEOK 추가
                                txtEqsgID.Text = scan_eqsgid_b;
                                txtEqsgname.Text = scan_eqsgname_b;

                                wo_woid = scan_woid_b;
                                wo_class = scan_class_b;
                                wo_eqsgid = scan_eqsgid_b;
                                wo_modlid = scan_model_b;
                                wo_pcsgid = scan_pcsgid_b;
                                wo_prodid = scan_prodid_b;

                                First_eqsgid_b = scan_eqsgid_b;
                                First_prodid_b = scan_prodid_b;
                                First_routid_b = scan_routid_b;
                                First_flowid_b = scan_flowid_b;
                                First_pcsgid_b = scan_pcsgid_b;
                                First_class_b = scan_class_b;
                                First_model_b = scan_model_b;
                                First_custid_b = scan_custid_b;

                                //PALLETID 자동 생성
                                autoPalettIdCreate(txtBoxLOTID.Text, "BOX");

                                //palletValidation("PLT", txtPalletId.Text);

                                Util.gridClear(dgPalletBox); //그리드 clear


                                //BOX를 PALLET,BOX 그리드(dgPalletBox)에 추가
                                addGridLot();

                                // BOX Label 자동발행
                                //labelPrint();

                                //boxing 상태 초기화
                                //boxingStatInit();

                                //txtBoxLotID.Text = "";
                                txtBoxLOTID.SelectAll();

                                boxingYn = true; //포장중

                                setCustPalletidChk(scan_eqsgid_b); //고객사 palletid(GTLID) 관리 라인이지 확인
                            }
                            #endregion

                            #region lotid일 경우
                            else
                            {
                                //1.lot check
                                if (!lotValidation_BR()) //if (!lotValidation())
                                {
                                    txtBoxLOTID.Text = "";
                                    return;
                                }

                                //2. lot의 정보가져오기
                                getLotInform();

                                // CMI인 경우 분기 처리
                                //2019.09.26 김도형 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252 | [서비스번호]4033252
                                if (LoginInfo.CFG_AREA_ID.Equals("P5"))
                                {
                                    if (scan_eqsgid == "P5Q01" || scan_eqsgid == "P5Q02")
                                    {
                                        if (dgPalletBox.GetRowCount() != 0)
                                        {
                                            string first_custid = DataTableConverter.GetValue(dgPalletBox.Rows[0].DataItem, "CUST_LOTID").ToString();
                                            if (scan_custid != first_custid)
                                            {
                                                Util.Alert("SFU8107");  //투입하는 고객제품정보가 잘 못 되었습니다.
                                                return;
                                            }
                                        }
                                    }
                                }

                                txtEqsgID.Text = scan_eqsgid;
                                txtLotProd.Text = scan_prodid;
                                First_routid = scan_routid;
                                First_flowid = scan_flowid;
                                txtBoxingPcsg.Text = scan_pcsgid;
                                txtLotingPcsg.Text = scan_pcsgid;
                                txtProdClass.Text = scan_class;
                                First_model = scan_model;

                                txtBoxingModel.Text = scan_model;
                                txtProdClass.Text = scan_class;

                                box_eqsg = scan_eqsgid;
                                wo_eqsgid = scan_eqsgid;

                                //2018.07.03
                                //2018.06.20 ================
                                if (scan_class == "CMA")
                                {
                                    btnWorkOroderSearch.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    btnWorkOroderSearch.Visibility = Visibility.Hidden;
                                }
                                //2018.06.20 ================
                                //2018.07.03

                                //포장관련 정보 찾기
                                if (txtReworkWOID.Text == null || txtReworkWOID.Text == "") //첫번째 lot 선택시
                                {
                                    if (scan_class == "BMA")
                                    {
                                        txtBoxingProd.Text = scan_prodid;
                                        txtBoxingPcsg.Text = scan_pcsgid;

                                        wo_prodid = scan_prodid;
                                        box_prod = wo_prodid;

                                        txtReworkWOID.Text = scan_woid;      //lot의 w/o

                                        //if (!orderCheck()) //등록된 order check
                                        //{
                                        //    ms.AlertWarning("SFU3454");////포장오더를 선택한 후 다시 스캔하세요
                                        //    txtReworkWOID.Text = "";
                                        //    return;
                                        //}
                                    }
                                    else if (scan_class == "CMA")
                                    {
                                        if (findWoProd())//등록된 포장오더에서 포장제품 찾기
                                        {
                                            if (!ckhCmaBoxingYn(scan_prodid)) //CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                                            {
                                                ms.AlertWarning("SFU3571"); //CMA 포장을 할수 없는 LOT입니다.
                                                //2019.04.02
                                                //reSet();

                                                //2019.06.26
                                                btnWorkOroderSearch.Visibility = Visibility.Hidden; ;
                                                return;
                                            }

                                            //WO 자동변경 로직 수행 : 첫 lot일 경우만 해당 로직 실행
                                            //woAutoChangeProcess(Util.GetCondition(txtBoxLotID));

                                            int idx = 100;

                                            //2020.05.14
                                            //if (dtWoProdResult.Rows.Count == 2)
                                            //{
                                            //    if (dtWoProdResult.Rows[0]["PCSGID"].ToString() == "P")
                                            //    {
                                            //        idx = 1;
                                            //    }
                                            //}

                                            for (int nCnt = 0; dtWoProdResult.Rows.Count > nCnt; nCnt++)
                                            {
                                                if (dtWoProdResult.Rows[nCnt]["PCSGID"].ToString() == "B")
                                                {
                                                    idx = nCnt;
                                                }
                                            }

                                            if (idx == 100)
                                            {
                                                ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                                txtReworkWOID.Text = "";
                                                return;
                                            }
                                            else
                                            {
                                                txtReworkWOID.Text = dtWoProdResult.Rows[idx]["WOID"].ToString();
                                                txtBoxingProd.Text = dtWoProdResult.Rows[idx]["PRODID"].ToString();
                                                txtBoxingPcsg.Text = dtWoProdResult.Rows[idx]["PCSGID"].ToString();
                                                txtEqsgname.Text = dtWoProdResult.Rows[idx]["EQSGNAME"].ToString();

                                                wo_prodid = Util.GetCondition(txtBoxingProd);
                                                box_prod = wo_prodid;
                                            }
                                        }
                                        else
                                        {
                                            ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                            txtReworkWOID.Text = "";
                                            return;
                                        }
                                    }
                                }

                                //PALLETID 생성
                                autoPalettIdCreate(txtBoxLOTID.Text, "LOT");

                                //포장 가능 수량 세팅
                                setBoxOfPalletCnt();

                                //BOX를 PALLET,BOX 그리드(dgPalletBox)에 추가
                                addGridLot();

                                //txtBoxLotID.Text = "";
                                txtBoxLOTID.SelectAll();
                                setCustPalletidChk(scan_eqsgid); //고객사 palletid(GTLID) 관리 라인이지 확인
                            }

                            //    boxingYn = true; //박싱중.
                            //    boxingBox_idx++; //PALLET에 담긴 BOX 수량 체크
                            //lotCountReverse--;

                            setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");

                        }
                        #endregion

                        else
                        {
                            //Util.AlertInfo("PALLETID를 먼저 입력 OR PALLETID CHECK 한후 진행하세요");
                            ms.AlertWarning("SFU3318"); //선택오류 : PALLETID 자동생성 CHECKBOX를 체크 하시거나 PALLETID를 먼저 입력하세요
                        }
                    }
                    else //2번째 부터 BOX OR LOT Scan시 / 조회결과에서 [적용] 클릭한후 포장 해제 한 후 lot scan / PALLET 스캔 후 LOT SCAN시
                    {
                        //box의 담길 lot의 수량체크
                        if (boxLotmax_cnt > boxingBox_idx)
                        {
                            if (CheckScanIDDuplicate("BOX")) //그리드 중복 체크
                            {
                                if (cboGubun.SelectedValue.ToString() == "Box ID")  // Box
                                {
                                    //입력된 box validation
                                    if (!boxValidation_BR("BOX"))
                                    {
                                        txtBoxLOTID.Text = "";
                                        return;
                                    }

                                    txtBoxInfo.Text = scan_class_b + " : " + scan_eqsgid_b + " : " + scan_prodid_b;

                                    if (dgPalletBox.GetRowCount() > 0) //두번부터 BOX 스캔시 처리
                                    {

                                        if (scan_class_b != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3574"); //포장대기중인 BOX의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        if (scan_prodid_b != txtBoxingProd.Text)
                                        {
                                            ms.AlertWarning("SFU3575"); //포장대기중인 BOX의 제품과 스캔한 BOX의 제품이 다릅니다.
                                            return;
                                        }

                                        if (scan_eqsgid_b != txtEqsgID.Text)
                                        {
                                            ms.AlertWarning("SFU3576"); //포장대기중인 BOX의 라인과 스캔한 BOX의 라인이 다릅니다.
                                            return;
                                        }

                                        if (scan_pcsgid_b != txtBoxingPcsg.Text)
                                        {
                                            ms.AlertWarning("SFU3577"); //포장대기중인 BOX의 공정군과 스캔한 BOX의 공정군이 다릅니다.
                                            return;
                                        }
                                    }
                                    else //pallet먼저 스캔하고 lot스캔한 경우 - pallet 정보와lot의 정보 비교
                                    {
                                        if (scan_class_b != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3578"); //포장하려는 PALLET의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        if (scan_eqsgid_b != wo_eqsgid)
                                        {
                                            ms.AlertWarning("SFU3579"); //포장하려는 PALLET의 라인과 스캔한 LOT의 라인이 다릅니다.
                                            return;
                                        }
                                    }
                                }

                                #region lotid
                                else
                                {
                                    //1.lot check
                                    if (!lotValidation_BR()) //if (!lotValidation())
                                    {
                                        txtBoxLOTID.Text = "";
                                        return;
                                    }

                                    //lot의 wip 정보가져오기
                                    getLotInform();

                                    //2019.09.26  CMI인 경우 분기 처리
                                    if (LoginInfo.CFG_AREA_ID.Equals("P5"))
                                    {
                                        if (scan_eqsgid == "P5Q01" || scan_eqsgid == "P5Q02")
                                        {
                                            if (dgPalletBox.GetRowCount() != 0)
                                            {
                                                string first_custid = DataTableConverter.GetValue(dgPalletBox.Rows[0].DataItem, "CUST_LOTID").ToString();
                                                if (scan_custid != first_custid)
                                                {
                                                    Util.Alert("SFU8107");  //투입하는 고객제품정보가 잘 못 되었습니다.
                                                    return;
                                                }
                                            }
                                        }
                                    }

                                    txtBoxInfo.Text = scan_class + " : " + scan_eqsgid + " : " + scan_prodid;

                                    if (dgPalletBox.GetRowCount() > 0)
                                    {
                                        if (scan_class == "CMA")
                                        {
                                            if (!ckhCmaBoxingYn(scan_prodid)) //CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                                            {
                                                ms.AlertWarning("SFU3571"); //CMA 포장을 할수 없는 LOT입니다.
                                                return;
                                            }
                                        }

                                        if (scan_class != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3456"); //작업오류 : 포장대기중인 LOT의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        if (scan_prodid != txtLotProd.Text)
                                        {
                                            ms.AlertWarning("SFU3457"); //작업오류 : 포장대기중인 LOT의 제품과 스캔한 LOT의 제품이 다릅니다.
                                            return;
                                        }

                                        if (scan_eqsgid != txtEqsgID.Text)
                                        {
                                            ms.AlertWarning("SFU3458"); //작업오류 : 포장대기중인 LOT의 라인과 스캔한 LOT의 라인이 다릅니다.
                                            return;
                                        }

                                        if (scan_pcsgid != txtLotingPcsg.Text)
                                        {
                                            ms.AlertWarning("SFU3459"); //작업오류 : 포장대기중인 LOT의 공정군과 스캔한 LOT의 공정군이 다릅니다.
                                            return;
                                        }
                                    }
                                    else //pallet먼저 스캔하고 lot스캔한 경우 - pallet 정보와lot의 정보 비교
                                    {
                                        if (scan_class != Util.GetCondition(txtProdClass))
                                        {
                                            ms.AlertWarning("SFU3528"); //포장하려는 PALLET의 제품타입과 스캔한 제품타입이 다릅니다.
                                            return;
                                        }

                                        if (scan_eqsgid != wo_eqsgid)
                                        {
                                            ms.AlertWarning("SFU3579"); //포장하려는 PALLET의 라인과 스캔한 LOT의 라인이 다릅니다.
                                            return;
                                        }
                                    }

                                }
                                #endregion

                                //입력된 lotid validation
                                //2019.11.18
                                //lotValidation();

                                ////2020.01.06
                                //2022.07.14     0.9        이은영     359658       BMW12V Change Paletizing Method (C20220713-000594)
                                //if (Util.GetCondition(cboGubun) == "Lot ID")
                                //{
                                //lotValidation();
                                //}

                                addGridLot();
                                //txtBoxLotID.Text = "";
                                txtBoxLOTID.SelectAll();
                                setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
                            }
                        }
                        else
                        {
                            //Util.AlertInfo("포장 가능 수량( " + boxLotmax_cnt.ToString() + " )을 넘습니다.");
                            ms.AlertWarning("SFU3319", boxLotmax_cnt.ToString()); // 입력오류 : PALLET의 포장가능 수량 %1을 넘었습니다. [포장수량 수정 후 LOT 입력]
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
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
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["EQSGID"] = eqsgid; //cma lot의 EQSGID
                //dr["PRODID"] = prodid; //cma lot의 PROD
                dr["LOTID"] = lotID; //cma lot의 LOLTID

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
                dr["PRODID"] = scanProdid; //cma lot의 PROD
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; //cma lot의 PROD

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

        private void getLotInform(string strBoxLotId = null)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = string.IsNullOrWhiteSpace(strBoxLotId) ? Util.GetCondition(txtBoxLOTID) : strBoxLotId;

                RQSTDT.Rows.Add(dr);

                DataTable dtLotInformResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SCAN_INFORM", "INDATA", "OUTDATA", RQSTDT); //DA_PRD_SEL_LOT_SCAN_INFORM

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
                    //2019.09.05
                    scan_custid = dtLotInformResult.Rows[0]["CUST_LOTID"].ToString();
                    scan_ocopyn = dtLotInformResult.Rows[0]["OCOP_RTN_FLAG"].ToString();
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

                if (dtOrderResult.Rows.Count > 0) //해당 제품으로 설정된 W/O가 있을 경우 설정된 정보를 UI에 뿌림
                {
                    orderOK = true;

                    txtReworkWOID.Text = dtOrderResult.Rows[0]["WOID"].ToString();      //등록된 wo
                    wo_prodid = dtOrderResult.Rows[0]["PRODID"].ToString();             //등록된 wo의 제품
                    wo_eqsgid = dtOrderResult.Rows[0]["EQSGID"].ToString();             //등록된 wo의 공정군
                    wo_modlid = dtOrderResult.Rows[0]["MODLID"].ToString();             //등록된 wo의 모델
                    wo_class = dtOrderResult.Rows[0]["PRODTYPE"].ToString();         //등록된 wo의 제품타입(CMA,BMA)
                    wo_conf_prodid = dtOrderResult.Rows[0]["CONF_PRODID"].ToString();   //등록된 wo의 ASSY제품
                }
                return orderOK;
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
                //2018.10.02
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = scan_prodid; //cma lot의 PROD
                //2018.10.02
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

        private void setBoxOfPalletCnt(string strKind = null)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = wo_eqsgid;
                dr["SHIPTO_ID"] = null;
                dr["PRODID"] = wo_prodid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXTOTAL_LOTTOTAL_CNT_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (string.IsNullOrWhiteSpace(strKind))
                    {
                        if (dtResult.Rows[0]["BOXCNT"] == null || dtResult.Rows[0]["BOXCNT"].ToString() == "")
                        {
                            ms.AlertInfo("SFU3607");
                            //PALLET의 [포장 수량]이 정의 되지 않았습니다. Defalut : 1로 세팅됩니다. \nMMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요.
                            nbBoxingCnt.Value = 1;
                        }
                        else
                        {
                            if (Convert.ToInt32(dtResult.Rows[0]["BOXCNT"]) != 0)
                            {
                                nbBoxingCnt.Value = Convert.ToInt32(dtResult.Rows[0]["BOXCNT"]);
                            }
                            else
                            {
                                nbBoxingCnt.Value = 1;
                            }
                        }
                    }
                    else
                    {
                        if (dtResult.Rows[0]["BOXCNT"] == null || dtResult.Rows[0]["BOXCNT"].ToString() == "")
                        {
                            nbBoxingCnt.Value = 1;
                        }
                        else
                        {
                            if (Convert.ToInt32(dtResult.Rows[0]["BOXCNT"]) != 0)
                            {
                                nbBoxingCnt.Value = Convert.ToInt32(dtResult.Rows[0]["BOXCNT"]);
                            }
                            else
                            {
                                nbBoxingCnt.Value = 1;
                            }
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(strKind))
                    {
                        ms.AlertInfo("SFU3607");
                        //PALLET의 [포장 수량]이 정의 되지 않았습니다. Defalut : 1로 세팅됩니다. \nMMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요.
                        nbBoxingCnt.Value = 1;
                    }
                    else
                    {
                        nbBoxingCnt.Value = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
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
                    //Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }
            }
        }
        #endregion

        #region 기타 이벤트 : 콤보, numberic
        private void cboGubun_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboGubun) == "Box ID")
            {
                tbBoxLotID.Text = "Box ID";
                tbGubun.Text = ObjectDic.Instance.GetObjectName("포장단위:BOX");
                //2018.07.03
                //2018.06.20
                btnWorkOroderSearch.IsEnabled = false;
            }
            else
            {
                tbBoxLotID.Text = "Lot ID";
                tbGubun.Text = ObjectDic.Instance.GetObjectName("포장단위:LOT");
                //2018.07.03
                //2018.06.20
                btnWorkOroderSearch.IsEnabled = true;
            }
        }

        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            //if (tbCount != null && tbCount.Text.ToString() == "0")//남은 수량 o일때
            //{
            //    if(e.OldValue > e.NewValue) //줄임수
            //    {
            //        nbBoxingCnt.ValueChanged -= nbBoxingCnt_ValueChanged;
            //        nbBoxingCnt.Value = e.OldValue;
            //        nbBoxingCnt.ValueChanged += nbBoxingCnt_ValueChanged;
            //        return;
            //    }
            //}

            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt - boxingBox_idx;
            string stat = string.Empty;

            if (boxingYn)
            {
                stat = ObjectDic.Instance.GetObjectName("포장중");
            }
            else
            {
                stat = ObjectDic.Instance.GetObjectName("대기중");
            }

            setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, stat);
        }
        #endregion

        #endregion Event

        #region Method
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

        private bool reBoxingCanYn()
        {
            try
            {
                bool Yn = true;

                DataTable dt = DataTableConverter.Convert(dgPalletBox.ItemsSource); //재포장할 grid의 정보를 DataTable 담아둠

                for (int i = 0; i < dt.Rows.Count; i++) //unpack
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("BOXID", typeof(string));
                    dtRqst.Columns.Add("BOXTYPE", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dtRqst.Rows.Add(dr);
                    dtRqst.Rows[0]["BOXID"] = dt.Rows[i]["BOXID"].ToString();
                    dtRqst.Rows[0]["BOXTYPE"] = "BOX";

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Rows[0]["ERP_IF_FLAG"].ToString() == "C")
                        {
                            Yn = false;
                        }
                    }
                }
                return Yn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setCustPalletidChk(string eqsgid)
        {
            try
            {
                cust_palletidYN = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "CUST_PALLETID_EQSGID";

                RQSTDT.Rows.Add(dr);

                DataTable dtCUSTPALLETIDEQSGID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUST_PALLETID_EQSGID_FIND", "INDATA", "OUTDATA", RQSTDT);

                string eqsgid_codes = string.Empty;

                if (dtCUSTPALLETIDEQSGID != null && dtCUSTPALLETIDEQSGID.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCUSTPALLETIDEQSGID.Columns.Count; i++)
                    {
                        if (eqsgid.Length != 0 && eqsgid == dtCUSTPALLETIDEQSGID.Rows[0][i].ToString())
                        {
                            cust_palletidYN = true;
                        }
                    }
                }

                if (cust_palletidYN) //고객사 palletid를 관리하는 라인이면 //cust_palletidYN || LoginInfo.USERID.Trim() == "cnswkdakscjf"
                {
                    tbConfPalletId.Visibility = Visibility.Visible;
                    txtConfPalletId.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                }
                else
                {
                    tbConfPalletId.Visibility = Visibility.Hidden;
                    txtConfPalletId.Visibility = Visibility.Hidden;
                    btnSave.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void unPack_Process(object sender, string palletID, bool unPackYn)
        {
            try
            {
                if (!unPackYn)
                {
                    //Util.AlertInfo("출고된 PALLET입니다. 포장취소 할수 없습니다.");
                    ms.AlertWarning("SFU3393"); //출고된 PALLET입니다. 포장취소 할수 없습니다.
                    return;
                }

                #region 주석처리 - 체크로직을 Biz에 통합  (2024.01.23 seonjun9)
                /*
                string okNg_msg = string.Empty;

                //oqc 검사 의뢰 됐는지 한번더 detail하게 체크
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string)); //PALLETID

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = palletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_INSP_PALLET_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0) //결과 값이 없으면 아직 출하검사 의뢰를 하지 않은 경우이므로 바로 '포장해제'
                {
                    if (dtResult.Rows[0]["INSP_PROG_CODE"] != null && dtResult.Rows[0]["INSP_PROG_CODE"].ToString() == "REQUEST") //검사요청중
                    {
                        //Util.AlertInfo("출하검사 의뢰중인 PALLET입니다. 포장취소 할수 없습니다.");
                        ms.AlertWarning("SFU3321"); //작업오류 : 출하검사 의뢰중인 PALLET입니다. 포장취소 할수 없습니다.
                        return;
                    }

                    //2019.10.08 QMS Hold
                    //C20221114 - 000372  QMS 신규 인터락으로 인한 QMS 불량 코드만 확인 HOLD 제외처리 요청의 건
                    if (dtResult.Rows[0]["QMS_HOLD_FLAG"] != null && dtResult.Rows[0]["QMS_HOLD_FLAG"].ToString() == "Y")
                    {
                        ms.AlertWarning("SFU8345"); //QMS 판정 불합격으로 인한 OQC_HOLD PALLET입니다.
                        return;
                    }

                    /* 2023.07.17 포장해제 시 필요없는 Validation 으로 주석 처리
                //    //2019.10.08 QMS Hold
                //    //C20221114 - 000372  QMS 신규 인터락으로 인한 QMS 불량 코드만 확인 HOLD 제외처리 요청의 건
                //    if (dtResult.Rows[0]["JUDG_VALUE"] != null && dtResult.Rows[0]["JUDG_VALUE"].ToString() == "F")
                //    {
                //        ms.AlertWarning("SFU9013"); //QMS 판정 불합격으로 인한 OQC_HOLD PALLET입니다.
                //        return;
                //    }

                //    //2022.12.12 염규범 선임
                //    //C20221114 - 000372  QMS 신규 인터락으로 인한 QMS 불량 코드만 확인 HOLD 제외처리 요청의 건
                //    if (dtResult.Rows[0]["FAIL_CNT"] != null && Convert.ToInt32(dtResult.Rows[0]["FAIL_CNT"].ToString()) > 0)
                //    {
                //        ms.AlertWarning("SFU9012"); //QMS 판정 불합격으로 LOT이 존재하는 PALLET입니다.
                //        return;
                //    }

                //if (oqcInspID_Init_chk(palletID))
                //    {
                //        //okNg_msg = "정말 포장취소 하시겠습니까?";
                //        okNg_msg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                //    }
                //    else
                //    {
                //        //okNg_msg = "출하검사 결과가 [불합격]이 아닙니다.\n계속 포장 취소 하시겠습니까?";
                //        okNg_msg = ms.AlertRetun("SFU3322"); //작업오류 : 선택한 PALLET의 출하 검사 결과가 [불합격]이 아닙니다. 포장취소 하시겠습니까?
                //    }
                //}
                //else
                //{
                //    //okNg_msg = "정말 포장취소 하시겠습니까?";
                //    okNg_msg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                //}

                */
                #endregion

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(_Msg_OkNg, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        //oqcInspID_Init(palletID);  //통합시킴

                        //UNPACK 로직
                        pack_unPack_init(sender);
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
                throw ex;
            }

        }

        private void buttonAccess(object sender)
        {
            try
            {
                Button btn = sender as Button;

                string grid_name = "dgPallethistory";
                int seleted_row = 0;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;

                        seleted_row = row.Index;

                        grid_name = presenter.DataGrid.Name;
                    }
                }

                DataRowView drv = row.DataItem as DataRowView;

                //[적용] 클릭 후 선택한 pallet가 box로 포장됐는지 lot으로 포장 됐는지 확인
                string boxcnt = DataTableConverter.GetValue(dgPallethistory.Rows[seleted_row].DataItem, "PALLETBOXCNT").ToString();
                string lotcnt = DataTableConverter.GetValue(dgPallethistory.Rows[seleted_row].DataItem, "TOTAL_QTY").ToString();

                if (boxcnt != lotcnt)
                {
                    cboGubun.SelectedValue = "Box ID";
                }
                else
                {
                    cboGubun.SelectedValue = "Lot ID";
                }

                string selectPallet = drv["PALLETID"].ToString();

                //txtBoxIdR
                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgPallethistory.ItemsSource);

                dt.AcceptChanges();

                foreach (DataRow drDel in dt.Rows)
                {
                    if (drDel["PALLETID"].ToString() == selectPallet)
                    {
                        drDel.Delete();
                        break;
                    }
                }

                dt.AcceptChanges();

                Util.GridSetData(dgPallethistory, dt, FrameOperation);

                if (!boxingYn)
                {
                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장작업전환");
                    SetPalletkeyDown(selectPallet);
                }
                else
                {
                    //Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                    ms.AlertWarning("SFU3308"); //작업오류 : 포장중인 작업이 있습니다. [포장완료 후 처리]
                    reBoxing = false;
                    return;
                }

                txtPalletId.Text = selectPallet;
                #region OQC검사결과
                //포장해체시 검사결과에 따라 Msg다르게 보여주기 위한 설정(2024.01.23 seonjun9)
                if (null != drv["OQC_INSP_REQ_ID"] && drv["OQC_INSP_REQ_ID"].ToString().Length > 0)
                {
                    if (null != drv["OQC_INSP_RESULT"] && drv["OQC_INSP_RESULT"].ToString().Equals("F"))
                    {
                        //_Msg_OkNg = "정말 포장취소 하시겠습니까?";
                        _Msg_OkNg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                    }
                    else
                    {
                        //okNg_msg = "출하검사 결과가 [불합격]이 아닙니다.\n계속 포장 취소 하시겠습니까?";
                        _Msg_OkNg = ms.AlertRetun("SFU3322"); //작업오류 : 선택한 PALLET의 출하 검사 결과가 [불합격]이 아닙니다. 포장취소 하시겠습니까?
                    }
                }
                else
                {
                    //_Msg_OkNg = "정말 포장취소 하시겠습니까?";
                    _Msg_OkNg = ms.AlertRetun("SFU3135"); //포장취소 하시겠습니까?
                }
                #endregion

                selectrcv = (null == drv["RCV_ISS_ID"]) ? string.Empty : drv["RCV_ISS_ID"].ToString();

                seleted_Pallet_Prod = drv["PRODID"].ToString(); //PALLET의 제품
                seleted_Pallet_Procid = drv["PROCID"] == null ? null : drv["PROCID"].ToString(); //PALLET의 공정
                seleted_Pallet_Eqptid = drv["EQPTID"].ToString(); //PALLET의 설비
                seleted_Pallet_Eqsgid = drv["EQSGID"].ToString(); //PALLET의 라인
                seleted_Pallet_PrdClass = drv["PRDCLASS"].ToString(); //PALLET의 제품군

                seleted_Pallet_pcsgid = drv["PCSGID"].ToString(); //PALLET의 공정군
                seleted_Pallet_Model = drv["MODEL"].ToString();   //PALLET의 모델
                //seleted_Pallet_Woid = drv["WOID"].ToString();

                seleted_oqc_insp_id = drv["OQC_INSP_REQ_ID"] == null ? null : drv["OQC_INSP_REQ_ID"].ToString(); //PALLET의 검사의뢰 ID
                seleted_judg_value = drv["OQC_INSP_RESULT"] == null ? null : drv["OQC_INSP_RESULT"].ToString(); //PALLET의 검사결과
                seleted_insp_name = drv["OQC_INSP_NAME"] == null ? null : drv["OQC_INSP_NAME"].ToString(); //PALLET의 검사이름
                seleted_Pallet_Prod_C = drv["PRODID_C"].ToString();
                seleted_Pallet_Pcsgid_C = drv["PCSGID_C"].ToString();
                seleted_Pallet_Eqsgname = drv["EQSGNAME"].ToString();


                boxLotmax_cnt = Convert.ToInt32(drv["PALLETBOXCNT"]); //포장가능수량
                boxingBox_idx = Convert.ToInt32(drv["PALLETBOXCNT"]); //포장된 수량

                txtBoxingProd.Text = seleted_Pallet_Prod;
                txtBoxingPcsg.Text = seleted_Pallet_pcsgid;
                txtBoxingModel.Text = seleted_Pallet_Model;
                txtLotingPcsg.Text = seleted_Pallet_Pcsgid_C;
                txtLotProd.Text = seleted_Pallet_Prod_C;
                txtEqsgID.Text = seleted_Pallet_Eqsgid;
                txtEqsgname.Text = seleted_Pallet_Eqsgname;

                txtProdClass.Text = seleted_Pallet_PrdClass;

                lotCountReverse = boxLotmax_cnt - boxingBox_idx; //남은수량

                nbBoxingCnt.Value = boxLotmax_cnt;

                txtBoxInfo.Text = seleted_Pallet_PrdClass + " : " + seleted_Pallet_Eqsgid + " : " + seleted_Pallet_Prod;

                setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");

                setCustPalletidChk(seleted_Pallet_Eqsgid); //고객사 palletid를 관리하는 라인인지 체크 후 고객사palletid 입력 란 오픈

                if (cust_palletidYN)
                {
                    txtConfPalletId.Text = drv["OUTER_BOXID2"] == null ? null : drv["OUTER_BOXID2"].ToString(); //고객사 palletid 입력

                    btnSave.Visibility = Visibility.Visible;
                    btnExcelLoad.Visibility = Visibility.Visible;
                }
                else
                {
                    btnSave.Visibility = Visibility.Hidden;
                    btnExcelLoad.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool oqcInspID_Init_chk(string palletID)
        {
            try
            {
                //oqc 검사 의뢰 됐는지 한번더 detail하게 체크
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string)); //PALLETID

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = palletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_INSP_PALLET_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows[0]["OQC_INSP_REQ_ID"] != null && dtResult.Rows[0]["OQC_INSP_REQ_ID"].ToString().Length > 0)
                    {
                        if (dtResult.Rows[0]["JUDG_VALUE"].ToString() == "F")
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
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void oqcInspID_Init(string palletID)
        {
            try
            {
                //oqc 검사 의뢰 ID : null 값으로 UPDATE
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("OQC_INSP_REQ_ID", typeof(string)); //PALLETID

                DataRow drUPD = INDATA.NewRow();
                drUPD["BOXID"] = palletID;
                drUPD["OQC_INSP_REQ_ID"] = seleted_oqc_insp_id;

                INDATA.Rows.Add(drUPD);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_OQC_INSP_SEQ_ID", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTagReport()
        {
            PackCommon.ShowPalletTag(this.GetType().Name, txtPalleyIdR.Text.ToString(), unPack_EqsgID, string.Empty);
        }

        private void setGubunCbo()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();
            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "Box ID", "Box ID" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "Lot ID", "Lot ID" };
            dtResult.Rows.Add(newRow);


            cboGubun.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                //RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                //RQSTDT.Columns.Add("USERID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                //2023.10.25 - 기존에 PRODID가 NULL로 들어가던 것을 cboProduct 값을 가져오는 것으로 수정 - KIM MIN SEOK
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                //2018.05.28
                //dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                //dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                dr["PALLETID"] = null;
                //dr["SYSTEM_ID"] = LoginInfo.SYSID;
                //dr["USERID"] = LoginInfo.USERID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLETHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgPallethistory.ItemsSource = null;
                txtPalleyIdR.Text = string.Empty;
                _Msg_OkNg = string.Empty;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgPallethistory, dtResult, FrameOperation);
                    txtSearchBox.Text = string.Empty;
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));

                btnExcelLoad.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void boxingEnd()
        {
            try
            {
                string palletID = Util.GetCondition(txtPalletId);
                string lot_total_qty;
                string rebox_lotbox_ckh = string.Empty;
                string gubun = string.Empty; //box포장인지 lot포장인지 구분

                string eqsg = string.Empty;
                string prod = string.Empty;
                string proc = string.Empty;

                //string cust_palletid = string.Empty; //고객사 palletid 관리하는 라인일때만

                if (reBoxing) //재포장
                {
                    eqsg = seleted_Pallet_Eqsgid;
                    prod = seleted_Pallet_Prod;
                    proc = seleted_Pallet_Procid;

                    rebox_lotbox_ckh = getLotBoxGubun();

                    if (rebox_lotbox_ckh == null || rebox_lotbox_ckh == "") //box로 pallet구성된 경우
                    {
                        gubun = "Box ID";
                        lot_total_qty = getLotTotal_qty();
                    }
                    else
                    {
                        gubun = "LotID";
                        lot_total_qty = (dgPalletBox.GetRowCount()).ToString();
                    }

                }
                else//최초포장
                {
                    gubun = Util.GetCondition(cboGubun);

                    if (gubun == "Box ID")
                    {
                        eqsg = wo_eqsgid;
                        prod = wo_prodid;
                        lot_total_qty = getLotTotal_qty();
                    }
                    else
                    {
                        eqsg = wo_eqsgid;
                        prod = wo_prodid;
                        lot_total_qty = (dgPalletBox.GetRowCount()).ToString();
                    }

                    if (Util.GetCondition(txtProdClass) == "CMA")
                    {
                        proc = "P5500";
                    }
                    else
                    {
                        proc = "P9500";
                    }
                }

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                INDATA.Columns.Add("BOXID", typeof(string));    //palletid
                //INDATA.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("PACK_EQSGID", typeof(string));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("PACK_EQPTID", typeof(string));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("PRODID", typeof(string));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("BOXLAYER", typeof(Int32));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("BOX_QTY", typeof(Int32));   //투입 총수량
                INDATA.Columns.Add("BOX_QTY2", typeof(Int32));   //투입 총수량
                INDATA.Columns.Add("USERID", typeof(string));   //사용자ID
                INDATA.Columns.Add("NOTE", typeof(string));   //사용자ID
                INDATA.Columns.Add("PROCID", typeof(string));   //사용자ID
                INDATA.Columns.Add("WOID", typeof(string));   //사용자ID
                INDATA.Columns.Add("CUST_PALLETID", typeof(string));   //사용자ID
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["BOXID"] = palletID; //PALLETID
                //dr["PROCID"] = "P5000";
                dr["PACK_EQSGID"] = eqsg;
                dr["PACK_EQPTID"] = "NOEQPT(H)";
                dr["PRODID"] = prod;
                dr["BOXLAYER"] = 2;
                dr["BOX_QTY"] = lot_total_qty; // dgPalletBox.GetRowCount().ToString(); //실제 포장되는 수량
                dr["BOX_QTY2"] = lot_total_qty;// dgPalletBox.GetRowCount().ToString(); //실제 포장되는 수량
                dr["USERID"] = LoginInfo.USERID;
                dr["NOTE"] = "pallet " + ObjectDic.Instance.GetObjectName("포장중");
                dr["PROCID"] = proc;
                dr["WOID"] = Util.GetCondition(txtReworkWOID);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (cust_palletidYN)
                {
                    dr["CUST_PALLETID"] = Util.GetCondition(txtConfPalletId);
                }
                else
                {
                    dr["CUST_PALLETID"] = ""; //재포장test
                }

                INDATA.Rows.Add(dr);

                DataTable ININNERBOX = indataSet.Tables.Add("ININNERBOX");
                ININNERBOX.Columns.Add("BOXID", typeof(string));

                DataTable LOTDATA = indataSet.Tables.Add("LOTDATA");
                LOTDATA.Columns.Add("LOTID", typeof(string));

                //LOTID를 팔렛에 구성할지 BOXID를 팔렛에 구성할지 판단
                if (gubun == "Box ID")
                {
                    for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
                    {
                        string sBoxId = Util.NVC(dgPalletBox.GetCell(i, dgPalletBox.Columns["BOXID"].Index).Value);

                        DataRow inDataDtl = ININNERBOX.NewRow();
                        inDataDtl["BOXID"] = sBoxId;
                        ININNERBOX.Rows.Add(inDataDtl);
                    }
                }
                else
                {
                    for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
                    {
                        string sLotId = Util.NVC(dgPalletBox.GetCell(i, dgPalletBox.Columns["BOXID"].Index).Value);

                        DataRow inDataDtl = LOTDATA.NewRow();
                        inDataDtl["LOTID"] = sLotId;
                        LOTDATA.Rows.Add(inDataDtl);
                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING_PALLET", "INDATA,ININNERBOX,LOTDATA", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    boxingYn = false; //포장완료 또는 포장 대기 flag

                    reBoxing = false; // 신규포장으로 변경

                    boxingStatInit(); //box 상태 초기화

                    control_Init(); //control 초기화

                    setBoxCnt(5, 0, 5, "대기중");

                    rePrint = false;
                    //신규발행, 재발행
                    //labelPrint(sender);

                    //Util.AlertInfo("포장 완료");
                    ms.AlertInfo("SFU3386"); //포장이 완료되었습니다.

                    SearchBox(palletID, false);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private string getLotTotal_qty()
        {
            try
            {
                int roof_cnt = dgPalletBox.GetRowCount();
                int lotTotal_cnt = 0;

                for (int i = 0; i < roof_cnt; i++)
                {
                    string inboxid = DataTableConverter.GetValue(dgPalletBox.Rows[i].DataItem, "BOXID").ToString();

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("BOXID", typeof(string));

                    DataRow dr = INDATA.NewRow();
                    dr["BOXID"] = inboxid;

                    INDATA.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PALLET_OF_TOTAL_LOT_QTY", "INDATA", "OUTDATA", INDATA);

                    lotTotal_cnt += Convert.ToInt32(dtResult.Rows[0]["TOTAL_QTY"]);

                }

                return lotTotal_cnt.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getLotBoxGubun()
        {
            try
            {
                string boxid = string.Empty;

                for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
                {
                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("BOXID", typeof(string));

                    DataRow dr = INDATA.NewRow();
                    dr["BOXID"] = DataTableConverter.GetValue(dgPalletBox.Rows[i].DataItem, "BOXID").ToString();

                    INDATA.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTBOX_BOXING_GUBUN", "INDATA", "OUTDATA", INDATA);

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        boxid = null;
                    }
                    else
                    {
                        boxid = dtResult.Rows[0]["BOXID"].ToString();
                    }
                }

                return boxid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetPalletkeyDown(string palletID)
        {
            try
            {
                if (chkPalletId.IsChecked == true && btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장")) //boxid가 체크 되어 있으면 biz에서 validaiton 수행 후  boxid 생성 하므로 로직에서 validation 필요없음.
                {
                    //Util.AlertInfo("CHECKBOX의 CHECK를 풀고 PALLETID를 입력하세요");
                    ms.AlertWarning("SFU3324"); //선택오류 : PALLETID 자동생성 CHECKBOX를 풀고 PALLETID를 입력하세요
                }
                else
                {
                    if (boxingYn)
                    {
                        txtPalletId.Text = boxing_lot;
                        //Util.AlertInfo("이전 포장작업이 완료 되지 않았습니다.");
                        ms.AlertWarning("SFU3308"); //작업오류 : 포장중인 작업이 있습니다. [포장완료 후 처리]
                        reBoxing = false;
                        return;
                    }

                    //입력된 boxid validation
                    palletValidation("PLT", palletID);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void palletValidation(string boxtype, string palletID)
        {
            //이전 포장 작업 유무
            if (boxingYn)
            {
                //Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                ms.AlertWarning("SFU3308"); //작업오류 : 포장중인 작업이 있습니다. [포장완료 후 처리]
                return;
            }

            //입력된 PALLETid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletID;
                dr["BOXTYPE"] = boxtype; //boxtype;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", RQSTDT); //박스가 있는지 확인

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    //Util.AlertInfo("입력한 PALLET가 존재하지 않습니다.");
                    ms.AlertWarning("SFU3394"); //입력한 PALLET가 존재하지 않습니다.
                    return;
                }

                wo_eqsgid = dtResult.Rows[0]["PACK_EQSGID"].ToString();
                wo_prodid = dtResult.Rows[0]["PRODID"].ToString();
                wo_pcsgid = dtResult.Rows[0]["PCSGID"].ToString();
                wo_class = dtResult.Rows[0]["PRDT_CLSS_CODE"].ToString();
                wo_model = dtResult.Rows[0]["PRJT_ABBR_NAME"].ToString();

                txtReworkWOID.Text = dtResult.Rows[0]["WOID"].ToString();

                //히스토리 그리드에서 링크 타고 올 경우 처리
                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환")) // && boxtype == "PLT")
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            chkPalletId.IsChecked = true;

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("BOXID", typeof(string));
                            RQSTDT1.Columns.Add("LANGID", typeof(string));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["BOXID"] = palletID;//"P6A1200001"; //Util.GetCondition(txtPalletId);
                            dr1["LANGID"] = LoginInfo.LANGID;//

                            RQSTDT1.Rows.Add(dr1);

                            DataTable dtPalletLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLETBOXS_SEARCH", "INDATA", "OUTDATA", RQSTDT1); //BOXID(PALLETID)에 물려 있는 BOXID SEARCH

                            txtcnt.Visibility = Visibility.Hidden;
                            btnUnPack.Visibility = Visibility.Visible;

                            btnSelectCacel.Visibility = Visibility.Hidden;
                            btncancel.Visibility = Visibility.Hidden;

                            txtBoxLOTID.IsEnabled = false;
                            chkPalletId.IsChecked = false;
                            chkPalletId.IsEnabled = false;
                            chkPalletId.Visibility = Visibility.Hidden;
                            txtPalletId.IsEnabled = false;

                            btnPack.Content = ObjectDic.Instance.GetObjectName("포장작업전환");

                            dgPalletBox.ItemsSource = null;

                            if (dtPalletLots.Rows.Count == 0)
                            {
                                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtPalletLots.Rows.Count));

                                int current_idx = dgPallethistory.CurrentRow.Index;

                                First_eqsgid = DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "EQSGID").ToString();
                                First_prodid = DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "PRODID").ToString();
                                First_routid = "";
                                First_flowid = "";
                                First_pcsgid = DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "PCSGID").ToString();
                                First_class = DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "PRDCLASS").ToString();
                                First_model = DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "MODEL").ToString();

                                boxLotmax_cnt = Convert.ToInt32(DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "TOTAL_QTY"));
                                boxingBox_idx = Convert.ToInt32(DataTableConverter.GetValue(dgPallethistory.Rows[current_idx].DataItem, "PALLETBOXCNT"));
                            }
                            else
                            {
                                dgPalletBox.ItemsSource = DataTableConverter.Convert(dtPalletLots);
                                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtPalletLots.Rows.Count));

                                First_eqsgid = dtPalletLots.Rows[0]["EQSGID"].ToString();
                                First_prodid = dtPalletLots.Rows[0]["PRODID"].ToString();
                                First_routid = "";
                                First_flowid = "";
                                First_pcsgid = dtPalletLots.Rows[0]["PCSGID"].ToString();
                                First_class = dtPalletLots.Rows[0]["CLASS"].ToString();
                                First_model = dtPalletLots.Rows[0]["MODEL"].ToString();

                                boxLotmax_cnt = dtPalletLots.Rows.Count;
                                boxingBox_idx = dtPalletLots.Rows.Count;
                            }

                            // (dgPalletBox.Columns["OQC_INSP_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Width = new C1.WPF.DataGrid.DataGridLength(100);
                            //(dgPalletBox.Columns["JUDG_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Width = new C1.WPF.DataGrid.DataGridLength(100);

                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("작업불가! 이미포장된BOXID", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            //return;
                        }
                    }
                }
                else if (btnPack.Content.ToString() != ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            // Util.AlertInfo("이미 포장된PALLET입니다.");
                            ms.AlertWarning("SFU3326"); //입력오류 : 입력한 PALLET가 이미 포장 됐습니다.[PALLET의 정보 확인]
                            return;
                        }
                    }

                    if (BoxWoInform())
                    {
                        txtReworkWOID.Text = dtBoxWoResult.Rows[0]["WOID"].ToString();
                        txtBoxingProd.Text = dtBoxWoResult.Rows[0]["PRODID"].ToString();
                        txtBoxingPcsg.Text = dtBoxWoResult.Rows[0]["PCSGID"].ToString();

                        wo_prodid = Util.GetCondition(txtBoxingProd);
                        box_prod = wo_prodid;
                    }
                    else
                    {
                        ms.AlertWarning("SFU3454"); //포장오더를 선택한 후 다시 스캔하세요
                        txtReworkWOID.Text = "";
                        return;
                    }

                    boxingYn = true;
                    boxing_lot = txtPalletId.Text.ToString();
                    pallet_prod = dtResult.Rows[0]["PRODID"].ToString();

                    //boxing 가능 수량 세팅 필요
                    setBoxOfPalletCnt();
                    boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                    lotCountReverse = boxLotmax_cnt;
                }

                setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
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
                dr["BOXID"] = txtPalletId.Text;
                dr["BOXTYPE"] = "PLT"; //"BOX";

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

        private bool boxValidation(string boxtype)
        {
            //입력된 BOX id 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = Util.GetCondition(txtBoxLOTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_EQSGID_FIND", "INDATA", "OUTDATA", RQSTDT); //박스가 있는지 확인

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    //Util.AlertInfo("입력한 BOX가 존재하지 않거나 빈 BOX입니다.");
                    ms.AlertWarning("SFU3327"); //입력오류 : 입력한 BOX가 존재하지 않거나 빈 BOX입니다.[BOX 정보 확인]
                    return false;
                }
                else
                {
                    string procid = dtResult.Rows[0]["PROCID"].ToString();
                    string prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string eqsgid = dtResult.Rows[0]["PACK_EQSGID"].ToString();
                    string _class = dtResult.Rows[0]["CLASS"].ToString();
                    string outerbox = dtResult.Rows[0]["OUTER_BOXID"] == null ? "" : dtResult.Rows[0]["OUTER_BOXID"].ToString();

                    if (box_prod != null && box_prod != "")
                    {
                        if (box_prod != prodid)
                        {
                            //Util.AlertInfo("이전에 투입한 BOX의 제품과 다릅니다.");
                            ms.AlertWarning("SFU3328"); //입력오류 : 포장 대기중인 BOX들의 제품과 현재 투입한 BOX의 제품이 다릅니다.
                            txtBoxLOTID.Text = "";
                            return false;
                        }
                    }

                    if (outerbox != "") //이미 팔렛에 포장된 경우
                    {
                        //Util.AlertInfo("이미 Pallet(" + outerbox + "에 포장된 BOX 입니다.");
                        ms.AlertWarning("SFU3395", outerbox); // 이미 PALLET({0})에 포장된 BOX입니다.

                        txtBoxLOTID.Text = "";
                        return false;
                    }

                    box_prod = prodid;
                    box_proc = procid;
                    box_eqsg = eqsgid;
                    box_class = _class;

                    txtBoxInfo.Text = box_class + " : " + box_eqsg + " : " + box_prod + " : " + box_proc;
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool boxValidation_BR(string boxtype, string strBoxLotId = null)
        {
            //입력된 BOX id 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("GQMS_INTERLOCK_USE_CALL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = string.IsNullOrWhiteSpace(strBoxLotId) ? Util.GetCondition(txtBoxLOTID) : strBoxLotId; //BOXID
                dr["BOXTYPE"] = "BOX";
                dr["BOX_PRODID"] = box_prod == "" ? null : box_prod;
                dr["PRDT_CLSS"] = box_class == "" ? null : box_class;
                dr["EQSGID"] = box_eqsg == "" ? null : box_eqsg;

                //BR_PRD_CHK_BOXLOT_PALLET 에 팔레트 출하시에만 Validation 해야 하는데 다른 화면에서도 이 비즈 사용해
                //이 화면에서 호출했을때만 Validation 하기 위해 파라미터 추가함
                dr["GQMS_INTERLOCK_USE_CALL"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT); //박스가 있는지 확인

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    //Util.AlertInfo("입력한 BOX가 존재하지 않거나 빈 BOX입니다.");
                    ms.AlertWarning("SFU3327"); //입력오류 : 입력한 BOX가 존재하지 않거나 빈 BOX입니다.[BOX 정보 확인]
                    return false;
                }
                else
                {
                    string procid = dtResult.Rows[0]["PROCID"].ToString();
                    string prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string eqsgid = dtResult.Rows[0]["EQSGID"].ToString();
                    string _class = dtResult.Rows[0]["CLASS"].ToString();
                    string woid = dtResult.Rows[0]["WOID"].ToString();
                    string model = dtResult.Rows[0]["MODEL"].ToString();
                    string pcsgid = dtResult.Rows[0]["PCSGID"].ToString();
                    string wotype = dtResult.Rows[0]["WOTYPE"].ToString();

                    //string outerbox = dtResult.Rows[0]["OUTER_BOXID"] == null ? "" : dtResult.Rows[0]["OUTER_BOXID"].ToString();
                    scan_woid_b = woid;
                    scan_class_b = _class;
                    scan_eqsgid_b = eqsgid;
                    scan_model_b = model;
                    scan_pcsgid_b = pcsgid;
                    scan_prodid_b = prodid;
                    scan_procid_b = procid;
                    pack_wotype = wotype;
                    scan_eqsgname_b = dtResult.Rows[0]["EQSGNAME"].ToString();
                    scan_ocopyn = dtResult.Rows[0]["OCOP_RTN_FLAG"].ToString();

                    txtBoxInfo.Text = scan_class_b + " : " + scan_eqsgid_b + " : " + scan_prodid_b + " : " + scan_procid_b;
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void autoPalettIdCreate(string box_lot_id, string gubun)
        {
            try
            {
                string setProcId = string.Empty;
                string prodId = string.Empty;

                if (Util.GetCondition(cboGubun) == "Box ID")
                {
                    setProcId = scan_procid_b;
                }
                else
                {
                    if (scan_class == "CMA")
                    {
                        setProcId = "P5500";
                    }
                    else if (scan_class == "BMA")
                    {
                        setProcId = "P9500";
                    }
                    else
                    {
                        return;
                    }
                }

                prodId = wo_prodid;

                DataTable dtPALLET = new DataTable(); //plletid 생성후 return

                //pallet 생성 로직
                dtPALLET.TableName = "dtPALLET";
                dtPALLET.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                dtPALLET.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID
                dtPALLET.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정)
                dtPALLET.Columns.Add("BOXQTY", typeof(Int32));    //투입 총수량
                dtPALLET.Columns.Add("EQSGID", typeof(string));   //라인ID
                dtPALLET.Columns.Add("USERID", typeof(string));   //사용자ID
                dtPALLET.Columns.Add("LOTID", typeof(string));    //LOTID 또는 BOXID
                dtPALLET.Columns.Add("GUBUN", typeof(string));    //LOTID 또는 BOXID
                dtPALLET.Columns.Add("PRODID", typeof(string));    //LOTID 또는 BOXID

                DataRow drPALLET = dtPALLET.NewRow();
                drPALLET["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drPALLET["LANGID"] = LoginInfo.LANGID;
                drPALLET["PROCID"] = setProcId; // Util.GetCondition(cboGubun) == "Box ID" ? box_proc : lot_proc;
                drPALLET["BOXQTY"] = Convert.ToInt32(nbBoxingCnt.Value);
                drPALLET["EQSGID"] = wo_eqsgid;
                drPALLET["USERID"] = LoginInfo.USERID;
                drPALLET["LOTID"] = box_lot_id;
                drPALLET["GUBUN"] = gubun;
                drPALLET["PRODID"] = prodId;

                dtPALLET.Rows.Add(drPALLET);

                DataTable dtPalletResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_PALLET", "INDATA", "OUTDATA", dtPALLET);

                if (dtPalletResult != null && dtPalletResult.Rows.Count != 0)
                {
                    txtPalletId.Text = dtPalletResult.Rows[0][0].ToString();
                    boxing_lot = txtPalletId.Text.ToString();

                    DataTable dtPalletInfo = new DataTable();
                    dtPalletInfo.TableName = "RQSTDT";
                    dtPalletInfo.Columns.Add("BOXID", typeof(string));

                    DataRow dr = dtPalletInfo.NewRow();
                    dr["BOXID"] = boxing_lot;  //palletid

                    dtPalletInfo.Rows.Add(dr);

                    DataTable dtPalletInfoResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", dtPalletInfo); //박스가 있는지 확인

                    pallet_prod = dtPalletInfoResult.Rows[0]["PRODID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CheckScanIDDuplicate(string gubun, string scanID = null)
        {
            if (gubun == "PALLET")
            {
                return false;
            }

            DataRowView dataRowView = null;
            string boxLOTID = string.IsNullOrEmpty(scanID) ? this.txtBoxLOTID.Text.ToString() : scanID;
            foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in this.dgPalletBox.Rows)
            {
                if (dataGridRow.DataItem != null)
                {
                    dataRowView = dataGridRow.DataItem as DataRowView;

                    if (dataRowView["BOXID"].ToString() == boxLOTID)
                    {
                        ms.AlertWarning("SFU1197"); // BOX 가 이미 추가되었습니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private void addGridLot(string strBoxLotId = null)
        {
            DataTable dtErpTran = getErpTrnaInform(Util.GetCondition(txtBoxLOTID));

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

            if (boxingBox_idx == 0)
            {
                DataTable dtdgPalletBox = new DataTable();
                dtdgPalletBox.Columns.Add("CHK", typeof(bool));
                dtdgPalletBox.Columns.Add("PALLETID", typeof(string));
                dtdgPalletBox.Columns.Add("BOXID", typeof(string));
                dtdgPalletBox.Columns.Add("PACKDTTM", typeof(string));
                dtdgPalletBox.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                dtdgPalletBox.Columns.Add("WOID", typeof(string));
                dtdgPalletBox.Columns.Add("CUST_LOTID", typeof(string));
                dtdgPalletBox.Columns.Add("OCOP_RTN_FLAG", typeof(string));

                DataRow dr = dtdgPalletBox.NewRow();
                dr["CHK"] = false;
                dr["PALLETID"] = Util.GetCondition(txtPalletId);
                dr["BOXID"] = string.IsNullOrWhiteSpace(strBoxLotId) ? Util.GetCondition(txtBoxLOTID) : strBoxLotId;
                dr["PACKDTTM"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                dr["ERP_TRNF_SEQNO"] = erp_trnf_seqno;
                dr["WOID"] = woid;
                dr["CUST_LOTID"] = scan_custid;
                dr["OCOP_RTN_FLAG"] = scan_ocopyn;

                dtdgPalletBox.Rows.Add(dr);

                dgPalletBox.ItemsSource = DataTableConverter.Convert(dtdgPalletBox);

                boxingYn = true;
            }
            else
            {
                int TotalRow = dgPalletBox.Rows.Count - 1; //헤더제거

                dgPalletBox.EndNewRow(true);
                DataGridRowAdd(dgPalletBox);
                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "PALLETID", Util.GetCondition(txtPalletId));

                if (string.IsNullOrWhiteSpace(strBoxLotId))
                {
                    DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "BOXID", Util.GetCondition(txtBoxLOTID));
                }
                else
                {
                    DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "BOXID", strBoxLotId);
                }

                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "PACKDTTM", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "ERP_TRNF_SEQNO", erp_trnf_seqno);
                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "WOID", woid);
                //DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "CUST_LOTID", string.IsNullOrWhiteSpace(scan_custid) ? null : scan_custid);
                if (!scan_custid_b.ToString().Equals(""))
                {
                    DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "CUST_LOTID", scan_custid);
                }
                DataTableConverter.SetValue(dgPalletBox.Rows[TotalRow].DataItem, "OCOP_RTN_FLAG", scan_ocopyn);


            }
            boxingBox_idx++;
            lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgPalletBox.ItemsSource);

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

        private void labelPrint(object sender)
        {
            try
            {
                string print_cnt = string.Empty;
                string print_yn = string.Empty;
                string prodID = string.Empty;
                string palletID = string.Empty;
                DataTable dtzpl;

                //재발행 신규 발행 구분 : PRODID 구분해서 뿌려줌.
                if (rePrint) //재발행
                {
                    prodID = seleted_Pallet_Prod;
                    palletID = txtPalleyIdR.Text;
                }
                else
                {
                    prodID = "";
                    palletID = txtPalletId.Text;
                }

                //zpl 가져오기
                //string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY,string sPRODID
                //dtzpl = Util.getZPL_Pack(palletID, null, null, null, "PACK_INBOX", "LBL0020", "N", "1", prodID , null);

                dtzpl = Util.getZPL_Pack(sLOTID: palletID
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: "LBL0019"
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: prodID
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);
                //라벨 발행
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                wndPopup.Show();

                //Util.printLabel_Pack(FrameOperation, loadingIndicator, txtPalleyIdR.Text, "PACK", "N", "1");

                if (!rePrint)
                {
                    return;
                }

                //재발행 일 경우 처리 : 기존 발행 정보 확인
                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print 여부 N인경우 Y로 update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }
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
                //DA_PRD_SEL_BOX_LIST_FOR_LABEL

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
                dr["BOXID"] = Util.GetCondition(txtPalleyIdR) == "" ? null : Util.GetCondition(txtPalleyIdR);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
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

        private void boxingStatInit()
        {
            boxingYn = false; //대기나 완료 상태로.
            boxingBox_idx = 0;
            boxLotmax_cnt = 1; // boxid에 수량 정보 담고 있음. 현재는 미정의 되어 있어서 테스트용으로 5를 세팅
            box_prod = "";
        }

        private void pack_unPack_init()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;

            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

            boxingBox_idx = 0;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);

            dgPalletBox.ItemsSource = null;
            txtBoxLOTID.IsEnabled = true;

            boxingYn = true;

            setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
        }

        //내부적으로 inner box 포장해 주는 로직 : 현재 안씀(BIZ에서 처리)
        private void innerBoxingProcess()
        {
            //1.create boxid
            autoBoxIdCreate();

            //2.boxing
            innerBoxingEnd();

        }

        private void innerBoxingEnd()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID
                INDATA.Columns.Add("BOXID", typeof(string));    //투이LOT(처음 LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정)
                INDATA.Columns.Add("BOXQTY", typeof(string));   //투입 총수량
                INDATA.Columns.Add("EQSGID", typeof(string));   //라인ID
                INDATA.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = boxID;
                dr["PROCID"] = "";
                dr["BOXQTY"] = 1;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                DataRow inDataDtl = IN_LOTID.NewRow();
                inDataDtl["LOTID"] = Util.GetCondition(txtBoxLOTID);
                IN_LOTID.Rows.Add(inDataDtl);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    //Util.AlertInfo("포장이 완료 되었습니다.");
                }
                else
                {
                    throw new Exception();
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
                lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투입LOT(처음 LOT)

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = Util.GetCondition(txtBoxLOTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTWIP_INFO", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_prodtype = dtResult.Rows[0]["PRODTYPE"].ToString();
                    string lot_wiphold = dtResult.Rows[0]["WIPHOLD"].ToString();
                    string lot_wipstat = dtResult.Rows[0]["WIPSTAT"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();
                    string lot_route = dtResult.Rows[0]["ROUTID"].ToString();
                    string lot_boxid = dtResult.Rows[0]["BOXID"] == null ? "" : dtResult.Rows[0]["BOXID"].ToString();

                    // 필요 없으나, BIZ에서는 에러 메세지가 표기가 잘 안됨
                    // 다만 그전 lotValidation_BR 에서 에러나면 해당 부분까지 내려오지 않기 때문에 필요 없음
                    if (lot_boxid != "") // 페기된 lot인지 체크
                    {
                        //2020.01.28
                        //Util.AlertInfo("이미 BOX(" + lot_boxid + ")에 포장된 LOT입니다.");
                        ms.AlertWarning("SFU8149", lot_boxid);
                        return false;
                    }

                    // 필요 없으나, BIZ에서는 에러 메세지가 표기가 잘 안됨
                    // 다만 그전 lotValidation_BR 에서 에러나면 해당 부분까지 내려오지 않기 때문에 필요 없음
                    if (lot_wipstat == "TERM") // 페기된 lot인지 체크
                    {
                        //2020.01.28
                        //Util.AlertInfo("폐기된 LOT입니다.");
                        ms.AlertWarning("SFU3290");
                        return false;
                    }

                    // lotValidation_BR의 하위중에서 QMS, 자재 결합등 HOLD 체크하는 로직을 BR_PRD_CHK_LOTHOLD 로 사용중이며 제외
                    if (lot_wiphold == "Y") //hold 상태인지 체크
                    {
                        //20201.01.28
                        //Util.AlertInfo("HOLD LOT입니다.");
                        ms.AlertWarning("SFU1340");
                        return false;
                    }

                    // lotValidation_BR 처리중이니 제외 처리
                    if (lot_class_old != null && lot_class_old != "") //이전 투입 제품 타입과 같은지 비교
                    {
                        if (lot_class_old != lot_class)
                        {
                            //2020.01.28
                            //Util.AlertInfo("이전에 투입한 LOT의 제품 타입과 다릅니다.");
                            ms.AlertWarning("SFU8148");
                            return false;
                        }
                    }

                    // BR_PRD_CHK_BOXLOT_PALLET  Step 58 으로 제거 가능
                    if (lot_class == "CMA") //제품타입별 포장 가능 공정 체크
                    {
                        if (lot_procid == "P5000" || lot_procid == "P5500" || lot_procid == "P5200" || lot_procid == "P5400")
                        {

                        }
                        else
                        {
                            //2020.01.28
                            //Util.AlertInfo("포장 불가능한 공정입니다.");
                            ms.AlertWarning("SFU3388");
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
                            //2020.01.28
                            //Util.AlertInfo("포장 불가능한 공정입니다.");
                            ms.AlertWarning("SFU3388");
                            return false;
                        }
                    }
                    else
                    {
                        //2020.01.28
                        //Util.AlertInfo("포장가능한 제품타입이 아닙니다.");
                        ms.AlertWarning("SFU3382");
                        return false;
                    }

                    if (lot_prod != null && lot_prod != "") //이전 투입 lot의 제품과 같은지 비교
                    {
                        if (lot_prod != lot_prodid)
                        {
                            //2020.01.28
                            //Util.AlertInfo("이전에 투입한 LOT의 제품과 다릅니다.");
                            ms.AlertWarning("SFU8148");
                            return false;
                        }
                    }

                    if (lot_eqsgid != null && lot_eqsgid != "") //이전 투입 lot의 라인과 같은지 비교
                    {
                        if (lot_eqsgid != lot_eqsg)
                        {
                            //2020.01.28
                            //Util.AlertInfo("이전에 투입한 LOT의 라인과 다릅니다.");
                            ms.AlertWarning("SFU3383");
                            return false;
                        }
                    }


                    //BR_PRD_CHK_BOXLOT_PALLET 의 Stp 24 이 더 상위 이므로 필요 없음
                    //포장공정의 공정 id 찾기
                    DataTable dtPROC = new DataTable();
                    dtPROC.TableName = "RQSTDT";
                    dtPROC.Columns.Add("ROUTID", typeof(string));
                    //dtPROC.Columns.Add("PROCTYPE", typeof(string));

                    DataRow drPROC = dtPROC.NewRow();
                    drPROC["ROUTID"] = lot_route;
                    //drPROC["PROCTYPE"] = "B"; //포장공정 타입

                    dtPROC.Rows.Add(drPROC);

                    DataTable dtdtPROCResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ENDPROCID_PACK", "INDATA", "OUTDATA", dtPROC);

                    if (dtdtPROCResult == null || dtdtPROCResult.Rows.Count == 0)
                    {
                        //2020.01.28
                        //Util.AlertInfo("포장공정을 찾을수 없습니다.");
                        ms.AlertWarning("SFU3384");
                        return false;
                    }

                    lot_proc = dtdtPROCResult.Rows[0]["PROCID"].ToString();
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;

                    txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    //2020.01.28
                    //Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                    ms.AlertWarning("SFU3379");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private bool lotValidation_BR(string strBoxLotId = null)
        {
            try
            {
                lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    //투입LOT(처음 LOT)
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("GQMS_INTERLOCK_USE_CALL", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = string.IsNullOrWhiteSpace(strBoxLotId) ? Util.GetCondition(txtBoxLOTID) : strBoxLotId; //LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = lot_prod == "" ? null : lot_prod;
                dr["PRDT_CLSS"] = lot_class_old == "" ? null : lot_class_old;
                dr["EQSGID"] = lot_eqsgid == "" ? null : lot_eqsgid;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                //BR_PRD_CHK_BOXLOT_PALLET 에 팔레트 출하시에만 Validation 해야 하는데 다른 화면에서도 이 비즈 사용해
                //이 화면에서 호출했을때만 Validation 하기 위해 파라미터 추가함
                dr["GQMS_INTERLOCK_USE_CALL"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();

                    lot_proc = lot_procid;
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;

                    txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    //Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                    ms.AlertWarning("SFU3379"); //LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.
                    return false;
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
                //lot 정보 로직
                DataTable RQSTDT_LOT = new DataTable();
                RQSTDT_LOT.TableName = "RQSTDT_LOT";
                RQSTDT_LOT.Columns.Add("LOTID", typeof(string));  //INPUT TYPE (UI OR EQ)

                DataRow dt_lot = RQSTDT_LOT.NewRow();
                dt_lot["LOTID"] = txtBoxLOTID.Text.ToString();//"UI";
                RQSTDT_LOT.Rows.Add(dt_lot);
                DataTable dtLotInform = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPLOT", "INDATA", "OUTDATA", RQSTDT_LOT);

                if (dtLotInform.Rows.Count == 0) //입력 lot이 존재 하지 않을때
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("존재하지 않는 LOT 입니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU1137"); //선택된 LOT 이 존재하지 않습니다.
                    return;
                }

                //boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정)
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtBoxLOTID.Text;

                foreach (DataRow drv in dtLotInform.Rows)
                {
                    dr["PROCID"] = drv["PROCID"].ToString(); // "POA2010"; // drv["PROCID"].ToString();
                    dr["LOTQTY"] = Convert.ToInt32(drv["WIPQTY"]); // "10000";   // drv["WIPQTY"].ToString();
                }

                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_BMA", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    boxID = dtResult.Rows[0][0].ToString();
                    //boxing_lot = txtBoxId.Text.ToString();
                }
                //else
                //{
                //    boxID = txtBoxLotID.Text;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable getZPL_Pack(string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY)
        {
            DataTable dtResult = null;

            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                //INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("PRN_QTY", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                dr["PROCID"] = sPROCID;
                //dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["PRN_QTY"] = sPRN_QTY;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;

                INDATA.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
                //new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void pack_unPack_init(object sender)
        {
            try
            {
                dtUnpackBoxs = DataTableConverter.Convert(dgPalletBox.ItemsSource);
                //pallet unpak\
                pallet_unpack(sender);

                //box_unpack(sender);

                //search();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void pallet_unpack(object sender)
        {
            try
            {
                Button btn = sender as Button;
                string unpack_boxid = string.Empty;

                string lot_total_qty = getLotTotal_qty();
                string prod = string.Empty;
                string eqsg = string.Empty;
                string eqpt = string.Empty;
                string proc = string.Empty;
                string unqty = string.Empty;

                if (btn.Name == "btnUnPack") //재포장을 위한 포장해제
                {
                    unpack_boxid = Util.GetCondition(txtPalletId);
                    prod = seleted_Pallet_Prod;
                    eqsg = seleted_Pallet_Eqsgid;
                    eqpt = seleted_Pallet_Eqptid;
                    proc = seleted_Pallet_Procid;
                    unqty = lot_total_qty;
                }
                else // 리스트에서 포장해제
                {
                    unpack_boxid = Util.GetCondition(txtPalleyIdR);
                    prod = unPack_ProdID;
                    eqsg = unPack_EqsgID;
                    eqpt = unPack_EqptID;
                    proc = unPack_ProcID;
                    unqty = unPack_lotQty;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WORKTYPE", typeof(string));
                INDATA.Columns.Add("OQC_INSP_REQ_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = unpack_boxid;
                dr["PRODID"] = prod;
                dr["UNPACK_QTY"] = unqty;
                dr["UNPACK_QTY2"] = unqty;

                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;
                dr["EQSGID"] = eqsg;
                dr["EQPTID"] = eqpt;
                dr["PROCID"] = proc;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WORKTYPE"] = string.Empty;
                dr["OQC_INSP_REQ_ID"] = seleted_oqc_insp_id;

                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_PALLET", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    if (btn.Name == "btnUnPack") //재포장시만 처리
                    {
                        txtcnt.Visibility = Visibility.Visible;
                        btnUnPack.Visibility = Visibility.Hidden;

                        btnSelectCacel.Visibility = Visibility.Visible;
                        btncancel.Visibility = Visibility.Visible;
                        btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

                        boxingBox_idx = dgPalletBox.GetRowCount();
                        boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                        lotCountReverse = boxLotmax_cnt - boxingBox_idx;

                        txtBoxLOTID.IsEnabled = true;

                        boxingYn = true;

                        setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");

                        ms.AlertWarning("SFU3389"); //포장이 해제 되어 재포장 가능합니다.
                    }
                    else
                    {
                        ms.AlertInfo("SFU3390"); //UNPACK되었습니다.
                    }

                    DataTable dt = new DataTable();
                    dt = DataTableConverter.Convert(dgPallethistory.ItemsSource);

                    dt.AcceptChanges();

                    foreach (DataRow dgBoxHistoryDr in dt.Rows)
                    {
                        if (dgBoxHistoryDr["PALLETID"].ToString() == txtPalleyIdR.Text.ToString())
                        {
                            dgBoxHistoryDr.Delete();
                            break;
                        }
                    }

                    dt.AcceptChanges();

                    Util.GridSetData(dgPallethistory, dt, FrameOperation);

                    Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dt.Rows.Count));

                    txtPalleyIdR.Text = string.Empty;
                    _Msg_OkNg = string.Empty;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void box_unpack(object sender)
        {
            try
            {
                //box table의 boxid와 lot table의 lotid가 같으면 inner box unpack // 다르면 pallet만 unpack
                if (!innerBoxLotCheck())
                {
                    return;
                }

                Button btn = sender as Button;
                string unpack_boxid = string.Empty;

                if (btn.Name == "btnUnPack")
                {
                    unpack_boxid = boxID;
                }
                else
                {
                    unpack_boxid = boxID;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("BOXID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = unpack_boxid;
                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    txtcnt.Visibility = Visibility.Visible;
                    btnUnPack.Visibility = Visibility.Hidden;

                    btnSelectCacel.Visibility = Visibility.Visible;
                    btncancel.Visibility = Visibility.Visible;
                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

                    boxingBox_idx = 0;
                    boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                    lotCountReverse = boxLotmax_cnt;

                    dgPalletBox.ItemsSource = null;
                    txtBoxLOTID.IsEnabled = true;
                    (dgPalletBox.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;

                    boxingYn = true;

                    setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool innerBoxLotCheck()
        {
            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("BOXID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["BOXID"] = boxID;
            INDATA.Rows.Add(dr);

            DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);

            return false;
        }

        private void reSet()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            chkPalletId.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");

            boxingBox_idx = 0;
            nbBoxingCnt.Value = 1;
            //nbBoxingCnt.Value = Convert.ToDouble(1);
            boxLotmax_cnt = 1;//Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            dgPalletBox.ItemsSource = null;
            txtBoxLOTID.IsEnabled = true;
            //(dgPalletBox.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            // (dgPalletBox.Columns["OQC_INSP_REQ_ID"] as C1.WPF.txtPalletId.IsEnabled = true;
            //(dgPalletBox.Columns["JUDG_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Width = new C1.WPF.DataGrid.DataGridLength(100);
            chkPalletId.IsChecked = true;
            chkPalletId.Visibility = Visibility.Visible;
            //2019.06.21
            //chkPalletId.IsEnabled = true;
            chkPalletId.IsEnabled = true;
            txtPalletId.IsEnabled = true;

            boxingYn = false;

            setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "대기중");

            txtPalletId.Text = "";
            txtBoxLOTID.Text = "";

            reBoxing = false;

            boxing_lot = "";
            lot_prod = "";
            lot_proc = "";
            lot_eqsgid = "";
            txtBoxInfo.Text = "";

            lot_class_old = "";

            box_prod = "";
            box_proc = "";
            box_eqsg = "";
            box_class = "";

            scan_eqsgid = string.Empty;
            scan_prodid = string.Empty;
            scan_routid = string.Empty;
            scan_flowid = string.Empty;
            scan_pcsgid = string.Empty;
            scan_class = string.Empty;
            scan_model = string.Empty;

            txtBoxingProd.Text = "";
            txtBoxingPcsg.Text = "";
            txtBoxingModel.Text = "";
            txtReworkWOID.Text = "";
            txtProdClass.Text = "";
            txtEqsgname.Text = "";
            txtLotProd.Text = "";
            txtLotingPcsg.Text = "";

            _Msg_OkNg = string.Empty;

            cust_palletidYN = false;
            btnSave.Visibility = Visibility.Hidden;
            btnExcelLoad.Visibility = Visibility.Hidden;

            //2019.06.10
            btnWorkOroderSearch.Visibility = Visibility.Hidden;

            setCustPalletidChk("");
        }

        private void setBoxCnt(int max_cnt, int lot_cnt, int lotCountReverse, string boxing_stat)
        {
            if (txtcnt == null)
            {
                return;
            }

            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " - " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            tbCount.Text = lotCountReverse.ToString();
        }

        private void SearchBox(string strPalletId, Boolean bGridClear)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgPallethistory.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("PALLETID = '" + strPalletId + "'").Count() > 0 && !bGridClear)
                    {
                        Util.MessageInfo("SFU8251");
                        return;
                    }
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("FROMDATE", typeof(string));
                INDATA.Columns.Add("TODATE", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PRODID"] = null;
                dr["MODLID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;
                dr["PALLETID"] = strPalletId;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_PALLETHISTORY_SEARCH", "INDATA", "OUTDATA", INDATA);

                dgPallethistory.ItemsSource = null;
                txtPalleyIdR.Text = string.Empty;
                _Msg_OkNg = string.Empty;

                if (dtResult.Rows.Count != 0)
                {
                    if (bGridClear)
                    {
                        txtSearchBox.Text = "";
                        Util.GridSetData(dgPallethistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        txtSearchBox.Text = "";

                        Util.gridClear(dgPallethistory);

                        dt.AsEnumerable().CopyToDataTable(dtResult, LoadOption.Upsert);

                        Util.GridSetData(dgPallethistory, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
                    }
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_015_WORKORDERSELECT popup = sender as PACK001_015_WORKORDERSELECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    //2020.01.28
                    string sPCSGWOID = TB_SFC_PCSG_WO(popup.EQSGID);

                    if (LoginInfo.CFG_SHOP_ID == "A040")
                    {
                        if (sPCSGWOID != popup.PRODID)
                        {
                            //선택한 작업지시의 제품코드와 다릅니다.
                            ms.AlertInfo("SFU2905");
                            return;
                        }
                        else
                        {
                            txtReworkWOID.Text = popup.WOID;
                            txtBoxingPcsg.Text = popup.PCSGID;
                            txtBoxingProd.Text = popup.PRODID;
                            txtEqsgID.Text = popup.EQSGID;
                            txtEqsgname.Text = popup.EQSGNAME;

                            pack_wotype = popup.WOTYPE;
                            wo_prodid = popup.PRODID;
                            box_prod = wo_prodid;
                        }
                    }
                    else
                    {
                        txtReworkWOID.Text = popup.WOID;
                        txtBoxingPcsg.Text = popup.PCSGID;
                        txtBoxingProd.Text = popup.PRODID;
                        //2018.10.12
                        txtEqsgID.Text = popup.EQSGID;
                        txtEqsgname.Text = popup.EQSGNAME;

                        pack_wotype = popup.WOTYPE;
                        wo_prodid = popup.PRODID;
                        box_prod = wo_prodid;
                    }

                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (dtExcelData != null)
                        {
                            ReturnChkAndReturnCellCreate_ExcelOpen(dtExcelData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnChkAndReturnCellCreate_ExcelOpen(DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][0] != null && dt.Rows[i][0].ToString() != "") // PalletID가 존재
                    {
                        //if(dt.Rows[i][1] != null && dt.Rows[i][1].ToString() != "") // 고객사 PalletID가 존재
                        //{
                        updateCustPalletID(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void updateCustPalletID(string palletid, string cust_palletid)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("CUST_PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletid;
                dr["CUST_PALLETID"] = cust_palletid;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_CUSTPALLETID", "RQSTDT", "", RQSTDT);

                SearchBox(palletid, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2020.01.28
        private string TB_SFC_PCSG_WO(string sEQSGID)
        {
            try
            {
                String sProdid = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sEQSGID;
                dr["PCSGID"] = "B";  // B 포장  M CMA  P BMA

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_PCSG_WO", "INDATA", "OUTDATA", RQSTDT);



                if (dtResult.Rows.Count > 0)
                {

                    sProdid = dtResult.Rows[0]["PRODID"].ToString();
                }
                else
                {
                    sProdid = null;
                }

                return sProdid;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 30)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    Util.MessageValidation("SFU4466");
                    return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxLOTID.SelectedText.ToString());
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
                Clipboard.SetText(txtBoxLOTID.SelectedText.ToString());
                txtBoxLOTID.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreatePallet(string clipboardData)
        {
            try
            {
                switch (LoginInfo.CFG_SHOP_ID)
                {
                    case "G481":
                        this.CreatePalletG481(clipboardData);
                        break;
                    case "A040":
                        this.CreatePalletA040(clipboardData);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreatePalletG481(string clipboardData)
        {
            try
            {
                if (!AuthCopyAndPasteChk())
                {

                    if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                    {
                        Util.MessageInfo("SFU3180");
                        txtBoxLOTID.Clear();
                        return;
                    }

                    txtBoxLOTID.Focus();
                    txtBoxLOTID.SelectAll();
                }
                else
                {
                    string[] seperator = new string[] { "\r\n", "\n", "," };
                    string[] arrObjectID = clipboardData.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string obj in arrObjectID)
                    {
                        if (string.IsNullOrWhiteSpace(obj)) break;

                        if (txtPalletId.Text == "" || !boxingYn) //PALLET id 가 입력 되지 않았고
                        {
                            if ((bool)chkPalletId.IsChecked) // PALLET 체크 박스가 체크 있을경우 PALLETID 자동 생성
                            {
                                if (Util.GetCondition(cboGubun) == "Box ID")
                                {
                                    if (!boxValidation_BR("BOX", obj))
                                    {
                                        txtPalletId.Text = "";
                                        return;
                                    }

                                    txtReworkWOID.Text = scan_woid_b;
                                    txtBoxingModel.Text = scan_model_b;
                                    txtBoxingPcsg.Text = scan_pcsgid_b;
                                    txtBoxingProd.Text = scan_prodid_b;
                                    txtProdClass.Text = scan_class_b;
                                    txtEqsgID.Text = scan_eqsgid_b;
                                    txtEqsgname.Text = scan_eqsgname_b;

                                    wo_woid = scan_woid_b;
                                    wo_class = scan_class_b;
                                    wo_eqsgid = scan_eqsgid_b;
                                    wo_modlid = scan_model_b;
                                    wo_pcsgid = scan_pcsgid_b;
                                    wo_prodid = scan_prodid_b;

                                    First_eqsgid_b = scan_eqsgid_b;
                                    First_prodid_b = scan_prodid_b;
                                    First_routid_b = scan_routid_b;
                                    First_flowid_b = scan_flowid_b;
                                    First_pcsgid_b = scan_pcsgid_b;
                                    First_class_b = scan_class_b;
                                    First_model_b = scan_model_b;
                                    First_custid_b = scan_custid_b;

                                    //PALLETID 자동 생성
                                    autoPalettIdCreate(obj, "BOX");

                                    Util.gridClear(dgPalletBox); //그리드 clear

                                    addGridLot();

                                    boxingYn = true; //포장중

                                    setCustPalletidChk(scan_eqsgid_b);
                                }
                                else
                                {
                                    //1.lot check
                                    if (!lotValidation_BR(obj)) //if (!lotValidation())
                                    {
                                        txtBoxLOTID.Text = "";
                                        return;
                                    }

                                    //2. lot의 정보가져오기
                                    getLotInform(obj);

                                    // CMI인 경우 분기 처리
                                    //2019.09.26 김도형 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252 | [서비스번호]4033252
                                    if (LoginInfo.CFG_AREA_ID.Equals("P5"))
                                    {
                                        if (scan_eqsgid == "P5Q01" || scan_eqsgid == "P5Q02")
                                        {
                                            if (dgPalletBox.GetRowCount() != 0)
                                            {
                                                string first_custid = DataTableConverter.GetValue(dgPalletBox.Rows[0].DataItem, "CUST_LOTID").ToString();
                                                if (scan_custid != first_custid)
                                                {
                                                    Util.Alert("SFU8107");  //투입하는 고객제품정보가 잘 못 되었습니다.
                                                    return;
                                                }
                                            }
                                        }
                                    }

                                    txtEqsgID.Text = scan_eqsgid;
                                    txtLotProd.Text = scan_prodid;
                                    First_routid = scan_routid;
                                    First_flowid = scan_flowid;
                                    txtBoxingPcsg.Text = scan_pcsgid;
                                    txtLotingPcsg.Text = scan_pcsgid;
                                    txtProdClass.Text = scan_class;
                                    First_model = scan_model;

                                    txtBoxingModel.Text = scan_model;
                                    txtProdClass.Text = scan_class;

                                    box_eqsg = scan_eqsgid;
                                    wo_eqsgid = scan_eqsgid;

                                    if (scan_class == "CMA")
                                    {
                                        btnWorkOroderSearch.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        btnWorkOroderSearch.Visibility = Visibility.Hidden;
                                    }

                                    if (txtReworkWOID.Text == null || txtReworkWOID.Text == "") //첫번째 lot 선택시
                                    {
                                        if (scan_class == "BMA")
                                        {
                                            txtBoxingProd.Text = scan_prodid;
                                            txtBoxingPcsg.Text = scan_pcsgid;

                                            wo_prodid = scan_prodid;
                                            box_prod = wo_prodid;

                                            txtReworkWOID.Text = scan_woid;      //lot의 w/o

                                        }
                                        else if (scan_class == "CMA")
                                        {
                                            if (findWoProd())//등록된 포장오더에서 포장제품 찾기
                                            {
                                                if (!ckhCmaBoxingYn(scan_prodid)) //CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                                                {
                                                    ms.AlertWarning("SFU3571"); //CMA 포장을 할수 없는 LOT입니다.
                                                                                //2019.04.02
                                                                                //2019.06.26
                                                    btnWorkOroderSearch.Visibility = Visibility.Hidden; ;
                                                    return;
                                                }

                                                int idx = 100;

                                                for (int nCnt = 0; dtWoProdResult.Rows.Count > nCnt; nCnt++)
                                                {
                                                    if (dtWoProdResult.Rows[nCnt]["PCSGID"].ToString() == "B")
                                                    {
                                                        idx = nCnt;
                                                    }
                                                }

                                                if (idx == 100)
                                                {
                                                    ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                                    txtReworkWOID.Text = "";
                                                    return;
                                                }
                                                else
                                                {
                                                    txtReworkWOID.Text = dtWoProdResult.Rows[idx]["WOID"].ToString();
                                                    txtBoxingProd.Text = dtWoProdResult.Rows[idx]["PRODID"].ToString();
                                                    txtBoxingPcsg.Text = dtWoProdResult.Rows[idx]["PCSGID"].ToString();
                                                    txtEqsgname.Text = dtWoProdResult.Rows[idx]["EQSGNAME"].ToString();

                                                    wo_prodid = Util.GetCondition(txtBoxingProd);
                                                    box_prod = wo_prodid;
                                                }
                                            }
                                            else
                                            {
                                                ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                                                txtReworkWOID.Text = "";
                                                return;
                                            }
                                        }
                                    }

                                    autoPalettIdCreate(obj, "LOT");

                                    setBoxOfPalletCnt("SM");


                                    //BOX를 PALLET,BOX 그리드(dgPalletBox)에 추가
                                    addGridLot(obj);

                                    txtBoxLOTID.SelectAll();
                                    setCustPalletidChk(scan_eqsgid); //고객사 palletid(GTLID) 관리 라인이지 확인
                                }

                                setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
                            }
                            else
                            {
                                Util.MessageInfo("SFU3318");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // LOTID만 Copy & Paste 가능하도록 하게 함.
        private void CreatePalletA040(string clipboardData)
        {
            bool isInterlock = true;
            string[] seperator = new string[] { "\r\n", "\n", "," };
            string[] arrObjectID = clipboardData.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < arrObjectID.Length; i++)
            {
                arrObjectID[i] = arrObjectID[i].Trim();
            }

            if ((arrObjectID.Length + this.dgPalletBox.GetRowCount()) > Convert.ToInt32(this.nbBoxingCnt.Value))
            {
                ms.AlertWarning("SFU3319", boxLotmax_cnt.ToString()); // 입력오류 : PALLET의 포장가능 수량 %1을 넘었습니다. [포장수량 수정 후 LOT 입력]
                return;
            }

            foreach (string obj in arrObjectID)
            {
                if (string.IsNullOrWhiteSpace(obj))
                {
                    break;
                }

                // Pallet ID가 입력되지 않았고, Pallet CheckBox가 Checked인 경우 PalletID 자동 생성
                if ((string.IsNullOrEmpty(this.txtPalletId.Text) || !this.boxingYn) && this.chkPalletId.IsChecked == true)
                {
                    switch (Util.GetCondition(this.cboGubun))
                    {
                        case "Box ID":
                            //this.CreatePalletWithBoxID(obj);
                            break;
                        case "Lot ID":
                            isInterlock = this.CreatePalletWithLOTID(obj);
                            break;
                        default:
                            break;
                    }
                    setBoxCnt(Convert.ToInt32(this.nbBoxingCnt.Value), arrObjectID.Length, lotCountReverse, "포장중");
                }

                // 2번째 부터 BOX OR LOT Scan시 / 조회결과에서 [적용] 클릭한후 포장 해제 한 후 lot scan / PALLET 스캔 후 LOT SCAN시
                else
                {
                    switch (Util.GetCondition(this.cboGubun))
                    {
                        case "Box ID":
                            //this.AddPalletBoxID(obj);
                            break;
                        case "Lot ID":
                            isInterlock = this.AddPalletLOTID(obj);
                            break;
                        default:
                            break;
                    }
                    //setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
                    setBoxCnt(Convert.ToInt32(this.nbBoxingCnt.Value), arrObjectID.Length, lotCountReverse, "포장중");
                }

                if (!isInterlock)
                {
                    break;
                }
            }
        }

        private bool CreatePalletWithLOTID(string LOTID)
        {
            bool returnValue = true;
            // 1.lot check
            if (!lotValidation_BR(LOTID))
            {
                txtBoxLOTID.Text = "";
                returnValue = false;
                return returnValue;
            }

            // 2. lot의 정보가져오기
            getLotInform(LOTID);

            // CMI인 경우 분기 처리
            //2019.09.26 김도형 [CSR ID:4033252] Shipment Pallet Revision Check | [요청번호]C20190704_33252 | [서비스번호]4033252
            if (LoginInfo.CFG_AREA_ID.Equals("P5"))
            {
                if (scan_eqsgid == "P5Q01" || scan_eqsgid == "P5Q02")
                {
                    if (dgPalletBox.GetRowCount() != 0)
                    {
                        string first_custid = DataTableConverter.GetValue(dgPalletBox.Rows[0].DataItem, "CUST_LOTID").ToString();
                        if (scan_custid != first_custid)
                        {
                            Util.Alert("SFU8107");  //투입하는 고객제품정보가 잘 못 되었습니다.
                            returnValue = false;
                            return returnValue;
                        }
                    }
                }
            }

            txtEqsgID.Text = scan_eqsgid;
            txtLotProd.Text = scan_prodid;
            First_routid = scan_routid;
            First_flowid = scan_flowid;
            txtBoxingPcsg.Text = scan_pcsgid;
            txtLotingPcsg.Text = scan_pcsgid;
            txtProdClass.Text = scan_class;
            First_model = scan_model;

            txtBoxingModel.Text = scan_model;
            txtProdClass.Text = scan_class;

            box_eqsg = scan_eqsgid;
            wo_eqsgid = scan_eqsgid;

            if (scan_class == "CMA")
            {
                btnWorkOroderSearch.Visibility = Visibility.Visible;
            }
            else
            {
                btnWorkOroderSearch.Visibility = Visibility.Hidden;
            }

            if (txtReworkWOID.Text == null || txtReworkWOID.Text == "") //첫번째 lot 선택시
            {
                if (scan_class == "BMA")
                {
                    txtBoxingProd.Text = scan_prodid;
                    txtBoxingPcsg.Text = scan_pcsgid;

                    wo_prodid = scan_prodid;
                    box_prod = wo_prodid;

                    txtReworkWOID.Text = scan_woid;      //lot의 w/o

                }
                else if (scan_class == "CMA")
                {
                    if (findWoProd())//등록된 포장오더에서 포장제품 찾기
                    {
                        if (!ckhCmaBoxingYn(scan_prodid)) //CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                        {
                            ms.AlertWarning("SFU3571"); //CMA 포장을 할수 없는 LOT입니다.
                                                        //2019.04.02
                                                        //2019.06.26
                            btnWorkOroderSearch.Visibility = Visibility.Hidden;
                            returnValue = false;
                            return returnValue;
                        }

                        int idx = 100;

                        for (int nCnt = 0; dtWoProdResult.Rows.Count > nCnt; nCnt++)
                        {
                            if (dtWoProdResult.Rows[nCnt]["PCSGID"].ToString() == "B")
                            {
                                idx = nCnt;
                            }
                        }

                        if (idx == 100)
                        {
                            ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                            txtReworkWOID.Text = "";
                            returnValue = false;
                            return returnValue;
                        }
                        else
                        {
                            txtReworkWOID.Text = dtWoProdResult.Rows[idx]["WOID"].ToString();
                            txtBoxingProd.Text = dtWoProdResult.Rows[idx]["PRODID"].ToString();
                            txtBoxingPcsg.Text = dtWoProdResult.Rows[idx]["PCSGID"].ToString();
                            txtEqsgname.Text = dtWoProdResult.Rows[idx]["EQSGNAME"].ToString();

                            wo_prodid = Util.GetCondition(txtBoxingProd);
                            box_prod = wo_prodid;
                        }
                    }
                    else
                    {
                        ms.AlertWarning("SFU3657"); //W/O 설정화면에서 W/O를 설정후 다시 작업하세요.
                        txtReworkWOID.Text = "";
                        returnValue = false;
                        return returnValue;
                    }
                }
            }

            autoPalettIdCreate(LOTID, "LOT");
            //setBoxOfPalletCnt("SM");

            //BOX를 PALLET,BOX 그리드(dgPalletBox)에 추가
            addGridLot(LOTID);

            txtBoxLOTID.SelectAll();
            setCustPalletidChk(scan_eqsgid); //고객사 palletid(GTLID) 관리 라인이지 확인

            return returnValue;
        }

        private bool AddPalletLOTID(string LOTID)
        {
            bool returnValue = true;
            try
            {
                if (!this.CheckScanIDDuplicate("BOX", LOTID))
                {
                    returnValue = false;
                    return returnValue;
                }

                // 1.lot check
                if (!lotValidation_BR(LOTID)) //if (!lotValidation())
                {
                    txtBoxLOTID.Text = "";
                    returnValue = false;
                    return returnValue;
                }

                //lot의 wip 정보가져오기
                getLotInform(LOTID);

                //2019.09.26  CMI인 경우 분기 처리
                if (LoginInfo.CFG_AREA_ID.Equals("P5"))
                {
                    if (scan_eqsgid == "P5Q01" || scan_eqsgid == "P5Q02")
                    {
                        if (dgPalletBox.GetRowCount() != 0)
                        {
                            string first_custid = DataTableConverter.GetValue(dgPalletBox.Rows[0].DataItem, "CUST_LOTID").ToString();
                            if (scan_custid != first_custid)
                            {
                                Util.Alert("SFU8107");  //투입하는 고객제품정보가 잘못 되었습니다.
                                returnValue = false;
                                return returnValue;
                            }
                        }
                    }
                }

                txtBoxInfo.Text = scan_class + " : " + scan_eqsgid + " : " + scan_prodid;

                if (dgPalletBox.GetRowCount() > 0)
                {
                    if (scan_class == "CMA")
                    {
                        if (!ckhCmaBoxingYn(scan_prodid)) //CMA 포장을 할수 없는 제품 체크 : CMA 포장이 없는 라인의 제품 찾음.
                        {
                            ms.AlertWarning("SFU3571"); //CMA 포장을 할수 없는 LOT입니다.
                            returnValue = false;
                            return returnValue;
                        }
                    }

                    if (scan_class != Util.GetCondition(txtProdClass))
                    {
                        ms.AlertWarning("SFU3456"); //작업오류 : 포장대기중인 LOT의 제품타입과 스캔한 제품타입이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }

                    if (scan_prodid != txtLotProd.Text)
                    {
                        ms.AlertWarning("SFU3457"); //작업오류 : 포장대기중인 LOT의 제품과 스캔한 LOT의 제품이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }

                    if (scan_eqsgid != txtEqsgID.Text)
                    {
                        ms.AlertWarning("SFU3458"); //작업오류 : 포장대기중인 LOT의 라인과 스캔한 LOT의 라인이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }

                    if (scan_pcsgid != txtLotingPcsg.Text)
                    {
                        ms.AlertWarning("SFU3459"); //작업오류 : 포장대기중인 LOT의 공정군과 스캔한 LOT의 공정군이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }
                }
                else //pallet먼저 스캔하고 lot스캔한 경우 - pallet 정보와lot의 정보 비교
                {
                    if (scan_class != Util.GetCondition(txtProdClass))
                    {
                        ms.AlertWarning("SFU3528"); //포장하려는 PALLET의 제품타입과 스캔한 제품타입이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }

                    if (scan_eqsgid != wo_eqsgid)
                    {
                        ms.AlertWarning("SFU3579"); //포장하려는 PALLET의 라인과 스캔한 LOT의 라인이 다릅니다.
                        returnValue = false;
                        return returnValue;
                    }
                }

                addGridLot(LOTID);
                txtBoxLOTID.SelectAll();
                //setBoxCnt(boxLotmax_cnt, boxingBox_idx, lotCountReverse, "포장중");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        private void CreatePalletWithBoxID(string BoxID)
        {
            if (Util.GetCondition(cboGubun) != "Box ID")  //if ((bool)rdoBoxId.IsChecked)
            {
                return;
            }

            //boxid 입력시 처리 로직
            //입력된 box validation
            if (!boxValidation_BR("BOX", BoxID))
            {
                txtPalletId.Text = string.Empty;
                return;
            }

            txtReworkWOID.Text = scan_woid_b;
            txtBoxingModel.Text = scan_model_b;
            txtBoxingPcsg.Text = scan_pcsgid_b;
            txtBoxingProd.Text = scan_prodid_b;
            txtProdClass.Text = scan_class_b;
            //2020.09.30 KIM MIN SEOK 추가
            txtEqsgID.Text = scan_eqsgid_b;
            txtEqsgname.Text = scan_eqsgname_b;

            wo_woid = scan_woid_b;
            wo_class = scan_class_b;
            wo_eqsgid = scan_eqsgid_b;
            wo_modlid = scan_model_b;
            wo_pcsgid = scan_pcsgid_b;
            wo_prodid = scan_prodid_b;

            First_eqsgid_b = scan_eqsgid_b;
            First_prodid_b = scan_prodid_b;
            First_routid_b = scan_routid_b;
            First_flowid_b = scan_flowid_b;
            First_pcsgid_b = scan_pcsgid_b;
            First_class_b = scan_class_b;
            First_model_b = scan_model_b;
            First_custid_b = scan_custid_b;

            // PALLETID 자동 생성
            autoPalettIdCreate(txtBoxLOTID.Text, "BOX");
            Util.gridClear(dgPalletBox); //그리드 clear


            // BOX를 PALLET,BOX 그리드(dgPalletBox)에 추가
            addGridLot(BoxID);

            txtBoxLOTID.SelectAll();

            boxingYn = true; //포장중

            setCustPalletidChk(scan_eqsgid_b); //고객사 palletid(GTLID) 관리 라인이지 확인
        }

        private void AddPalletBoxID(string BoxID)
        {
            if (boxLotmax_cnt > boxingBox_idx)
            {
                ms.AlertWarning("SFU3319", boxLotmax_cnt.ToString()); // 입력오류 : PALLET의 포장가능 수량 %1을 넘었습니다. [포장수량 수정 후 LOT 입력]
            }

            //입력된 box validation
            if (!boxValidation_BR("BOX", BoxID))
            {
                txtBoxLOTID.Text = "";
                return;
            }

            txtBoxInfo.Text = scan_class_b + " : " + scan_eqsgid_b + " : " + scan_prodid_b;

            if (dgPalletBox.GetRowCount() > 0) //두번부터 BOX 스캔시 처리
            {

                if (scan_class_b != Util.GetCondition(txtProdClass))
                {
                    ms.AlertWarning("SFU3574"); //포장대기중인 BOX의 제품타입과 스캔한 제품타입이 다릅니다.
                    return;
                }

                if (scan_prodid_b != txtBoxingProd.Text)
                {
                    ms.AlertWarning("SFU3575"); //포장대기중인 BOX의 제품과 스캔한 BOX의 제품이 다릅니다.
                    return;
                }

                if (scan_eqsgid_b != txtEqsgID.Text)
                {
                    ms.AlertWarning("SFU3576"); //포장대기중인 BOX의 라인과 스캔한 BOX의 라인이 다릅니다.
                    return;
                }

                if (scan_pcsgid_b != txtBoxingPcsg.Text)
                {
                    ms.AlertWarning("SFU3577"); //포장대기중인 BOX의 공정군과 스캔한 BOX의 공정군이 다릅니다.
                    return;
                }
            }
            else // pallet먼저 스캔하고 lot스캔한 경우 - pallet 정보와lot의 정보 비교
            {
                if (scan_class_b != Util.GetCondition(txtProdClass))
                {
                    ms.AlertWarning("SFU3578"); //포장하려는 PALLET의 제품타입과 스캔한 제품타입이 다릅니다.
                    return;
                }

                if (scan_eqsgid_b != wo_eqsgid)
                {
                    ms.AlertWarning("SFU3579"); //포장하려는 PALLET의 라인과 스캔한 LOT의 라인이 다릅니다.
                    return;
                }
            }
        }

        private void txtBoxLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.V || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            string clipboardData = Clipboard.GetText();
            if(string.IsNullOrEmpty(clipboardData))
            {
                return;
            }

            this.CreatePallet(clipboardData);
        }

        private void txtPalletId_PreviewKeyDown(object sender, KeyEventArgs e)
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
                             txtPalletId.Clear();
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

        private void MenuItem_PltId_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtPalletId.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_PltId_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtPalletId.SelectedText.ToString());
                txtPalletId.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean AuthCopyAndPasteChk()
        {
            Boolean bChk = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));
                RQSTDT.Columns.Add("APPR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERNAME"] = LoginInfo.USERID;
                dr["APPR_CODE"] = "GMES_PACK_SM_PLPLT";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPR_PERSON_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    bChk = true;
                }
                return bChk;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bChk;
            }
        }
    }
}
