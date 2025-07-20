/*************************************************************************************
 Created Date : 2017.05.17
      Creator : 신광희C
   Decription : [조립 - 원각 및 초소형 상단 버튼 영역 UserControl]
--------------------------------------------------------------------------------------
 [Change History]
   2017.05.17   신광희C   : 최초생성
   2019.10.21   이상준C   : Last Cell No 조회 버튼  추가 
   2019.10.31   이상준C   : Cell ID 정보조회 버튼  추가 
   2023.02.07   성민식    : C20230109-000394 설비 투입 수량 변경 이력 조회 버튼 추가
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_IMPLEMENT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyCommand : UserControl
    {
        #region Properties
        public C1DropDownButton ButtonExtra { get; set; }
        public Button ButtonWaitLot { get; set; }
        public Button ButtonFinalCut { get; set; }
        public Button ButtonCleanLot { get; set; }
        public Button ButtonCancelFCut { get; set; }
        public Button ButtonCut { get; set; }
        public Button ButtonInvoiceMaterial { get; set; }
        public Button ButtonEqptCond { get; set; }
        public Button ButtonMixConfirm { get; set; }
        public Button ButtonWaitPancake { get; set; }
        public Button ButtonCancelTerm { get; set; }
        public Button ButtonCancelTermSepa { get; set; }
        public Button ButtonWindingTrayLocation { get; set; }
        public Button ButtonTestMode { get; set; }
        public Button ButtonAutoRsltCnfmMode { get; set; }// C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
        public Button ButtonQualityInput { get; set; }
        public Button ButtonEqptIssue { get; set; }
        public Button ButtonStart { get; set; }
        public Button ButtonCancel { get; set; }
        public Button ButtonEqptEnd { get; set; }
        public Button ButtonEqptEndCancel { get; set; }
        public Button ButtonConfirm { get; set; }
        public Button ButtonBarcodeLabel { get; set; }
        public Button ButtonPrintLabel { get; set; }
        public Button ButtonHistoryCard { get; set; }
        public Button ButtonRemarkHist { get; set; }
        public Button ButtonEditEqptQty { get; set; }
        public Button ButtonQualitySearch { get; set; }
        public Button ButtonBoxPrint { get; set; }
        public Button ButtonWindingLot { get; set; }
        public Button ButtonScheduledShutdown { get; set; }
        public Button ButtonSelfInspection { get; set; }
        public Button ButtonSelfInspectionNew { get; set; }
        public Button ButtonElectrodeInputEnd { get; set; }
        public Button ButtonTrayLotChange { get; set; }
        public Button ButtonLastCellNo { get; set; }
        public Button ButtonEqptQtyHist { get; set; }
        public TextBox TextEndLotId { get; set; }
        public string ProcessCode { get; set; }
        public string InlineFlag { get; set; }
        public bool IsSmallType { get; set; }
        public bool IsReWork { get; set; }
        public Button ButtonCellDetailInfo { get; set; }

        public Button ButtonModlChgCheck { get; set; }

        #endregion Properties

        #region Constructor
        public UcAssyCommand()
        {
            InitializeComponent();
            SetButtons();
        }
        #endregion Constructor

        #region Events
        #endregion Events

        #region Methods

        public void SetInlineBtn()
        {
            btnWindingTrayLocation.Visibility = Visibility.Collapsed;
        }

        private void SetButtons()
        {
            ButtonExtra = btnExtra;
            ButtonWaitLot = btnWaitLot;
            ButtonFinalCut = btnFinalCut;
            ButtonCleanLot = btnCleanLot;
            ButtonCancelFCut = btnCancelFCut;
            ButtonCut = btnCut;
            ButtonInvoiceMaterial = btnInvoiceMaterial;
            ButtonEqptCond = btnEqptCond;
            ButtonMixConfirm = btnMixConfirm;
            ButtonWaitPancake = btnWaitPancake;
            ButtonCancelTerm = btnCancelTerm;
            ButtonCancelTermSepa = btnCancelTermSepa;
            ButtonWindingTrayLocation = btnWindingTrayLocation;
            ButtonTestMode = btnTestMode;
            ButtonAutoRsltCnfmMode = btnAutoRsltCnfm; // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
            ButtonQualityInput = btnQualityInput;
            ButtonEqptIssue = btnEqptIssue;
            ButtonStart = btnStart;
            ButtonCancel = btnCancel;
            ButtonEqptEnd = btnEqptEnd;
            ButtonEqptEndCancel = btnEqptEndCancel;
            ButtonConfirm = btnConfirm;
            ButtonBarcodeLabel = btnBarcodeLabel;
            ButtonPrintLabel = btnPrintLabel;
            ButtonHistoryCard = btnHistoryCard;
            ButtonRemarkHist = btnRemarkHist;
            ButtonEditEqptQty = btnEditEqptQty;
            ButtonQualitySearch = btnQualitySearch;
            ButtonBoxPrint = btnBoxPrint;
            ButtonWindingLot = btnWindingLot;
            ButtonScheduledShutdown = btnScheduledShutdown;
            ButtonSelfInspection = btnSelfInspection;
            ButtonSelfInspectionNew = btnSelfInspectionNew;
            ButtonElectrodeInputEnd = btnElectrodeInputEnd;
            ButtonTrayLotChange = btnTrayLotChange;
            ButtonLastCellNo = btnLastCellNo;
            ButtonCellDetailInfo = btnCellDetailInfo;
            ButtonModlChgCheck = btnModlChgCheck;
            ButtonEqptQtyHist = btnEqptQtyHist;
        }

        public void SetButtonVisibility()
        {
            btnFinalCut.Visibility = Visibility.Collapsed;
            btnCleanLot.Visibility = Visibility.Collapsed;
            btnCancelFCut.Visibility = Visibility.Collapsed;
            btnCut.Visibility = Visibility.Collapsed;
            btnInvoiceMaterial.Visibility = Visibility.Collapsed;
            btnMixConfirm.Visibility = Visibility.Collapsed;
            btnPrintLabel.Visibility = Visibility.Collapsed;
            btnBarcodeLabel.Visibility = Visibility.Collapsed;
            btnWaitLot.Visibility = Visibility.Collapsed;
            //btnTestMode.Visibility = Visibility.Collapsed;
            //btnEqptCond.Visibility = Visibility.Collapsed;
            btnBoxPrint.Visibility = Visibility.Collapsed;
            btnWindingLot.Visibility = Visibility.Collapsed;
            btnElectrodeInputEnd.Visibility = Visibility.Collapsed;
            btnTrayLotChange.Visibility = Visibility.Collapsed;
            btnLastCellNo.Visibility = Visibility.Collapsed;
            btnCellDetailInfo.Visibility = Visibility.Collapsed;
            // 자주검사 등록 버튼 숨김 처리
            //btnSelfInspection.Visibility = Visibility.Collapsed;
            btnModlChgCheck.Visibility = Visibility.Collapsed;
            if(LoginInfo.CFG_SHOP_ID != "A010")
            {
                //ESOC 아닐 경우 숨김 처리
                btnEqptQtyHist.Visibility = Visibility.Collapsed;
            }
            //=====================================================================================================================
            // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
            //=====================================================================================================================
            // 자동실적확정 버튼 숨김 처리
            btnAutoRsltCnfm.Visibility = Visibility.Collapsed;
            if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
            {
                btnWaitPancake.Visibility = Visibility.Visible;
                //btnElectrodeInputEnd.Visibility = Visibility.Visible;
                if (!IsSmallType)
                {
                    btnWindingLot.Visibility = Visibility.Visible;
                    btnWindingTrayLocation.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnWindingLot.Visibility = Visibility.Collapsed;
                }

                if (ProcessCode.Equals(Process.WINDING_POUCH))
                {
                    btnSelfInspection.Visibility = Visibility.Collapsed;
                    btnSelfInspectionNew.Visibility = Visibility.Collapsed;
                }

                if (ProcessCode.Equals(Process.WINDING) && !IsSmallType && LoginInfo.CFG_SHOP_ID == "A010")
                {
                    btnModlChgCheck.Visibility = Visibility.Visible;
                }
                //=====================================================================================================================
                // C20220822-000365 원통형 9,10호 실적 자동 확정 기능 구현
                //=====================================================================================================================
                // 자동실적확정 버튼 숨김 처리 (OC소형조립 - OC소형조립2동 - 조립원통형#09,#10 호)
                if (LoginInfo.CFG_SHOP_ID.Equals("A010") && LoginInfo.CFG_AREA_ID.Equals("M2")   
                    &&(LoginInfo.CFG_EQSG_ID.Equals("M2C09") || LoginInfo.CFG_EQSG_ID.Equals("M2C10")) 
                    &&LoginInfo.CFG_PROC_ID.Equals("A2000") && IsSmallType )
                {
                    btnAutoRsltCnfm.Visibility = Visibility.Visible; 
                }
                else
                {
                    btnAutoRsltCnfm.Visibility = Visibility.Collapsed;
                }
            }
            else if (ProcessCode.Equals(Process.ASSEMBLY))
            {
                btnWaitPancake.Visibility = Visibility.Collapsed;
                btnHistoryCard.Visibility = Visibility.Collapsed;
                btnRemarkHist.Visibility = Visibility.Collapsed;
                btnEditEqptQty.Visibility = Visibility.Collapsed;
                btnEqptQtyHist.Visibility = Visibility.Collapsed;

                if (!IsSmallType)
                {
                    btnWindingLot.Visibility = Visibility.Visible;
                    btnWindingTrayLocation.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnWindingLot.Visibility = Visibility.Collapsed;
                }
            }
            else if (ProcessCode.Equals(Process.WASHING))
            {
                btnWaitPancake.Visibility = Visibility.Collapsed;
                btnHistoryCard.Visibility = Visibility.Collapsed;
                btnRemarkHist.Visibility = Visibility.Collapsed;
                btnEditEqptQty.Visibility = Visibility.Collapsed;
                btnWindingTrayLocation.Visibility = Visibility.Collapsed;
                btnCancelTerm.Visibility = Visibility.Collapsed;
                btnCancelTermSepa.Visibility = Visibility.Collapsed;
                btnLastCellNo.Visibility = Visibility.Visible;
                btnCellDetailInfo.Visibility = Visibility.Visible;
                btnEqptQtyHist.Visibility = Visibility.Collapsed;

                if (!IsSmallType)
                {
                    btnBoxPrint.Visibility = Visibility.Visible;
                    btnTrayLotChange.Visibility = Visibility.Visible;
                }
                if (IsReWork)
                {
                    btnTrayLotChange.Visibility = Visibility.Collapsed;
                    btnTestMode.Visibility = Visibility.Collapsed;
                    btnScheduledShutdown.Visibility = Visibility.Collapsed;
                    btnBoxPrint.Visibility = Visibility.Collapsed;
                    btnCancelTerm.Visibility = Visibility.Visible;
                    btnCancelTermSepa.Visibility = Visibility.Visible;
                    btnWindingTrayLocation.Visibility = Visibility.Collapsed;
                }
            }
            else if (ProcessCode.Equals(Process.XRAY_REWORK))
            {
                btnExtra.Visibility = Visibility.Collapsed;
                btnEqptIssue.Visibility = Visibility.Collapsed;
                btnEqptEnd.Visibility = Visibility.Collapsed;
                btnEqptEndCancel.Visibility = Visibility.Collapsed;
                btnHistoryCard.Visibility = Visibility.Collapsed;

                btnStart.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;
                btnConfirm.Content = ObjectDic.Instance.GetObjectName("작업완료");
                btnEqptCond.Visibility = Visibility.Collapsed;
                btnSelfInspection.Visibility = Visibility.Collapsed;
                btnSelfInspectionNew.Visibility = Visibility.Collapsed;
            }
        }

        public void SetButtonInline()
        {
            if (InlineFlag.Equals("Y"))
            {
                btnWindingTrayLocation.Visibility = Visibility.Collapsed;
            }            
        }

            #endregion Methods
        }
    }