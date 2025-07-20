/*************************************************************************************
 Created Date : 2017.11.03
      Creator : 
   Decription : 엑셀 업로드 형식
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.03  정규환 : Initial Created.


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_119_UPLOAD_FORMAT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public COM001_119_UPLOAD_FORMAT()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        #region 닫기 버턴
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

    }
}
