/*************************************************************************************
 Created Date : 2022.11.15
      Creator : 조영대
   Decription : Master Sample Cell 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.15  조영대 : Initial Created
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_120 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        public FCS002_120()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded;
        }
       

        private void InitControl()
        {
        }


        #endregion

        #region [Event]
        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {

        }
        #endregion

        #region [Method]

        #endregion


    }
}


