/*************************************************************************************
 Created Date : 2020.10.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29  DEVELOPER : Initial Created.





 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{

    public partial class FCS001_021_RELATIVE_RJUDGE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        
        public FCS001_021_RELATIVE_RJUDGE()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                DataTable dtRJudge = tmps[0] as DataTable;

                dgTrayList.SetItemsSource(dtRJudge, FrameOperation, false);
            }
        }
        #endregion

        #region [Method]
        
        #endregion

        #region [Event]

        #endregion

    }
}
