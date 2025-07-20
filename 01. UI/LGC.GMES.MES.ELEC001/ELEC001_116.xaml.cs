/*************************************************************************************
 Created Date : 2016.11.17
      Creator : 이슬아
   Decription : 믹서원자재 라벨 발행
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.17  이슬아 : 최초 생성





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_116 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private const string _BizRule = "";

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_116()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();           
        }

        #endregion

        #region Event
      
        #endregion

        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {          
        }

        #endregion

        private void chkResult_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
