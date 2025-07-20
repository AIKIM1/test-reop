/*************************************************************************************
 Created Date : 2017.10.11
      Creator : 이슬아
   Decription : 반품 셀 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.11  이슬아 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Globalization;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_214 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();        
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_214()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }
        
        #endregion

        #region Event
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion        

        #region Mehod

        #region [BizCall]

    
        #endregion        


        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        #endregion

        #endregion
    }
}
