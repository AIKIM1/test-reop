using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcPolymerFormCommand.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPolymerFormCommand : UserControl
    {
        #region Properties
        public C1DropDownButton ButtonExtra { get; set; }
        public Button ButtonTakeOver { get; set; }
        public Button ButtonCartDefect { get; set; }
        public Button ButtonInspectionNew { get; set; }
        public Button ButtonModeChange { get; set; }
        public Button ButtonChangeRoute { get; set; }
        public Button ButtonSublotDefect { get; set; }
        public Button ButtonChangeAommGrade { get; set; }


        public Button ButtonStart { get; set; }
        public Button ButtonCancel { get; set; }
        public Button ButtonInspection { get; set; }
        public Button ButtonEqptEnd { get; set; }
        public Button ButtonInboxType { get; set; }
        public Button ButtonConfirm { get; set; }
        public Button ButtonCartMove { get; set; }
        public Button ButtonCartStorage { get; set; }

        public string ProcessCode { get; set; }
        public string DivisionCode { get; set; }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Properties

        #region Constructor
        public UcPolymerFormCommand()
        {
            InitializeComponent();
            SetButtons();
        }
        #endregion Constructor

        #region Events
        #endregion Events

        #region Methods

        private void SetButtons()
        {
            // 추가기능
            ButtonExtra = btnExtra;
            ButtonTakeOver = btnTakeOver;
            ButtonCartDefect = btnCartDefect;
            ButtonInspectionNew = btnInspectionNew;
            ButtonModeChange = btnModeChange;
            ButtonChangeRoute = btnChangeRoute;
            ButtonSublotDefect = btnSublotDefect;
            ButtonChangeAommGrade = btnChangeAommGrade;

            // 작업시작.. 실적확정
            ButtonStart = btnStart;
            ButtonCancel = btnCancel;
            ButtonInspection = btnInspection;
            ButtonInboxType = btnInboxType;
            ButtonEqptEnd = btnEqptEnd;
            ButtonConfirm = btnConfirm;

            // 대차이동, 대차보관
            ButtonCartMove = btnCartMove;
            ButtonCartStorage = btnCartStorage;
        }

        public void SetButtonVisibility()
        {
            btnEqptEnd.Visibility = Visibility.Collapsed;
            btnCartMove.Visibility = Visibility.Collapsed;
            btnCartStorage.Visibility = Visibility.Collapsed;
            btnChangeAommGrade.Visibility = Visibility.Collapsed;
        

            // Degas(Tray)
            if (string.Equals(ProcessCode, Process.PolymerDegas) && DivisionCode == "Tray")
            {
                btnEqptEnd.Visibility = Visibility.Collapsed;
                btnInboxType.Visibility = Visibility.Collapsed;
                btnChangeRoute.Visibility = Visibility.Collapsed;
            }
            // Degas(Pallet), 특성/Grading, Grading 공정진척
            else if (string.Equals(ProcessCode, Process.PolymerDegas) && DivisionCode == "Pallet"
                     || string.Equals(ProcessCode, Process.PolymerGrading)
                     || string.Equals(ProcessCode, Process.PolymerCharacteristicGrader))
            {
                btnChangeRoute.Visibility = Visibility.Collapsed;

            }
            // DSF, Taping 공정진척
            else if (string.Equals(ProcessCode, Process.PolymerDSF)
                     || string.Equals(ProcessCode, Process.PolymerTaping))
            {
                //btnEqptEnd.Visibility = Visibility.Visible;
                btnChangeRoute.Visibility = Visibility.Collapsed;

                if(string.Equals(ProcessCode, Process.PolymerTaping))
                    btnChangeAommGrade.Visibility = Visibility.Visible;
            }
            // Side Taping, TCO 공정진척
            else if (string.Equals(ProcessCode, Process.PolymerSideTaping)
                     || string.Equals(ProcessCode, Process.PolymerTCO))
            {
                btnChangeRoute.Visibility = Visibility.Collapsed;

            }
            // 최종외관검사(DSF, 특성), Offline 특성 공정진척
            else if (string.Equals(ProcessCode, Process.PolymerFinalExternal) 
                    || string.Equals(ProcessCode, Process.PolymerFinalExternalDSF)
                    || string.Equals(ProcessCode, Process.PolymerOffLineCharacteristic))
            {
                btnInboxType.Visibility = Visibility.Collapsed;
                btnInspection.Visibility = Visibility.Collapsed;
                btnInspectionNew.Visibility = Visibility.Collapsed;
                btnCartMove.Visibility = Visibility.Visible;
                btnCartStorage.Visibility = Visibility.Visible;

                btnChangeRoute.Visibility = Visibility.Collapsed;

                btnStart.Content = ObjectDic.Instance.GetObjectName("대차작업시작");
                btnCancel.Content = ObjectDic.Instance.GetObjectName("대차작업취소");
                btnConfirm.Content = ObjectDic.Instance.GetObjectName("조립LOT완료");
            }
            // 양품화 공정 대차 구성
            else if (string.Equals(ProcessCode, Process.PolymerFairQuality))
            {
                btnInboxType.Visibility = Visibility.Visible;
                //btnExtra.Visibility = Visibility.Collapsed;
                btnInspection.Visibility = Visibility.Collapsed;
                btnInspectionNew.Visibility = Visibility.Collapsed;
                //btnStart.Visibility = Visibility.Collapsed;
                //btnCancel.Visibility = Visibility.Collapsed;
                //btnConfirm.Visibility = Visibility.Collapsed;
                //btnChangeRoute.Visibility = Visibility.Collapsed;
            }
            // CELL 포장 , 물류반품 , RMA 반품
            else if (string.Equals(ProcessCode, Process.CELL_BOXING)
               || string.Equals(ProcessCode, Process.CELL_BOXING_RETURN)
               || string.Equals(ProcessCode, Process.CELL_BOXING_RETURN_RMA))
            {
                //btnExtra.Visibility = Visibility.Collapsed;
                btnInspection.Visibility = Visibility.Collapsed;
                btnInspectionNew.Visibility = Visibility.Collapsed;
                btnEqptEnd.Visibility = Visibility.Collapsed;

                btnChangeRoute.Visibility = Visibility.Visible;
            }

            // DSF 공정 생산Lot 작업 모드 변경 팝업
            if (!string.Equals(ProcessCode, Process.PolymerDSF))
                btnModeChange.Visibility = Visibility.Collapsed;

            // 원각 자주검사 입력방식 문제로 일단 안보이게
            btnInspectionNew.Visibility = Visibility.Collapsed;

        }

        #endregion Methods

        private void btnTestPrint_Click(object sender, RoutedEventArgs e)
        {
            //CMM_FORM_TEST_PRINT
            CMM_FORM_TEST_PRINT popupTestPrint = new CMM_FORM_TEST_PRINT { FrameOperation = FrameOperation };
            C1WindowExtension.SetParameters(popupTestPrint, null);
            popupTestPrint.Closed += popupTestPrint_Closed;
            this.Dispatcher.BeginInvoke(new Action(() => popupTestPrint.ShowModal()));
        }

        private void popupTestPrint_Closed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }




    }
}