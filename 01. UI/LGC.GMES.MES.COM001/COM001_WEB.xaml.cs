/*************************************************************************************
 Created Date : 2019.01.31
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.31  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_WEB : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        //Util _Util = new Util();

        //string sSeqNo = null;
        //string parameter = string.Empty;

        public COM001_WEB()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            string sUrl = "http://gportal.lgensol.com/collpack/collaboration/board/boardItem/listBoardItemView.do?workspaceId=110143298868&boardId=110175075887";
            System.Diagnostics.Process.Start(sUrl);
            this.Close();
        }
        #endregion
    }
}