/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 활성화 공정진척 - 버튼
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
**************************************************************************************/using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.FCS002.Controls
{
    /// <summary>
    /// UcFCSCommand.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFCSCommand : UserControl
    {
        #region Properties
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Button ButtonOperResult { get; set; }
        public Button ButtonCreateTrayInfo { get; set; }
        public Button ButtonProductList { get; set; }

        public string ProcessCode { get; set; }

        #endregion Properties

        public UcFCSCommand()
        {
            InitializeComponent();
            SetButtons();
        }

        private void SetButtons()
        {
            ButtonOperResult = btnOperResult;                    // 공정별 실적조회
            ButtonCreateTrayInfo = btnCreateTrayInfo;            // Tray정보 생성
            ButtonProductList = btnProductList;                  // 공정 홈 화면전환
        }

        public void SetButtonExtraVisibility()
        {

        }

        public void SetButtonVisibility(bool Main)
        {
            // 단선추가 => 일단 주석 처리
            btnOperResult.Visibility = Visibility.Visible;
            btnCreateTrayInfo.Visibility = Visibility.Visible;
            if (Main)
            {
                btnProductList.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnProductList.Visibility = Visibility.Visible;
            }
        }

        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

    }
}