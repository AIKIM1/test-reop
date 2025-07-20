/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Net;
using System.Reflection;

namespace LGC.GMES.MES.ProtoType01
{
    public partial class ProtoType0102Windows01 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        //public string Parameter
        //{
        //    get;
        //    set;
        //}

        //public object[] Parameters
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ProtoType0102Windows01()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string tmp = C1WindowExtension.GetParameter(this);
            object[] tmps = C1WindowExtension.GetParameters(this);
            string tmmp01 = tmps[0] as string;
            DataTable tmmp02 = tmps[1] as DataTable;
        }

        #endregion

        #region Mehod

        #endregion
    }
}
