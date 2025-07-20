/*************************************************************************************
  Created Date : 2020.10.07
      Creator : 안인효
   Decription : 수동 대기재공 이동
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  안인효 : Initial Created.  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY004
{
    public partial class ASSY004_008_MANUAL_WAIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public ASSY004_008_MANUAL_WAIT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize

        #endregion


        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        #endregion


        #region Method

        #endregion
    }
}