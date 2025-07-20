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
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_155 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_155()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom2.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;
        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
