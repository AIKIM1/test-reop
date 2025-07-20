/*************************************************************************************
 Created Date : 2023.01.26
      Creator : 김선준
   Decription : 반품사유팝업
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2023.01.26      김선준 :                           Initial Created.
***************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_038_POPUP_HIS : C1Window, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        DataTable dtMain = new DataTable(); 
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_038_POPUP_HIS()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {            

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            object[] tmps = C1WindowExtension.GetParameters(this);
            dtMain = (DataTable)tmps[0];
            grdMain.ItemsSource = dtMain.DefaultView;
        }        
        #endregion 
    }
}