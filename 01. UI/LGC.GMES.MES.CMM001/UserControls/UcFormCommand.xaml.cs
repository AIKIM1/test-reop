/*************************************************************************************
 Created Date : 2017.10.20
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2019.01.02  0.2   이상훈   C20181121_50523   불량 pallet 생성 버튼 추가 (공통모듈로 기본 Collapsed 설정
                                              오창소형 && F5300 이면 보이도록 개선
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
    public partial class UcFormCommand : UserControl
    {
        #region Properties
        public C1DropDownButton ButtonExtra { get; set; }
        public Button ButtonInspectionNew { get; set; }

        public Button ButtonStart { get; set; }
        public Button ButtonCancel { get; set; }
        public Button ButtonCompletion { get; set; }
        public Button ButtonInspection { get; set; }
        public Button ButtonDefect { get; set; }
        public Button ButtonInboxType { get; set; }
        public Button ButtonDefectPalletCreate { get; set; } // C20181121_50523 버튼 추가
        public string ProcessCode { get; set; }

        #endregion Properties

        #region Constructor
        public UcFormCommand()
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
            ButtonExtra = btnExtra;
            ButtonInspectionNew = btnInspectionNew;

            ButtonStart = btnStart;
            ButtonCancel = btnCancel;
            ButtonCompletion = btnCompletion;
            ButtonInspection = btnInspection;
            ButtonDefect = btnDefect;
            ButtonInboxType = btnInboxType;

            ButtonDefectPalletCreate = btnDefectPalletCreate; // C20181121_50523 버튼 추가
        }

        public void SetButtonVisibility()
        {
            if (string.Equals(ProcessCode, Process.CircularGrader) || string.Equals(ProcessCode, Process.CircularVoltage))
            {
                btnDefect.Visibility = Visibility.Collapsed;
            }

            if (!string.Equals(ProcessCode, Process.SmallExternalTab) && !string.Equals(ProcessCode, Process.CircularCharacteristic))
            {
                // 외부탭 용접, 원형 특성 공정 에서만 사용
                btnInspection.Visibility = Visibility.Collapsed;

                // 추가기능
                btnExtra.Visibility = Visibility.Collapsed;
                btnInspectionNew.Visibility = Visibility.Collapsed;
            }

            if (LoginInfo.CFG_SHOP_ID.Equals("A010") && ProcessCode.Equals(Process.CircularCharacteristicGrader))
            {
                btnDefectPalletCreate.Visibility = Visibility.Visible;
                btnDefect.Content = ObjectDic.Instance.GetObjectName("특성 불량 등록");
            }

            // 원각 자주검사 입력방식 문제로 일단 안보이게
            btnInspectionNew.Visibility = Visibility.Collapsed;

        }

        #endregion Methods
    }
}