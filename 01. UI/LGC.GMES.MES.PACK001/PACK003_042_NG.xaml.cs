/*************************************************************************************
 Created Date : 2023.05.04
      Creator : 김선준
   Decription : Partial ILT 입/출고 NG메시지
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.04  김선준 : Initial Created.
  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_042_NG : C1Window 
    {
        #region #. Member Variable Lists...
        public IFrameOperation FrameOperation { get; internal set; }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_042_NG()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {            

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null & tmps.Length != 0 && null != tmps[0])
            {
                DataTable dt = (DataTable)tmps[0];
                DataView dv = new DataView(dt);
                dv.Sort = "RSLT_MSG ASC";

                //dt = dv.ToTable();
                this.grdMain.ItemsSource = dv;
            }
        }        
        #endregion 
    }
}