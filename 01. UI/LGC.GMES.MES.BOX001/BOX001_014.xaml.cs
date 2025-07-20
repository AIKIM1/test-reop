/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public BOX001_014()
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
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            // ComboBox 추가 필요
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "PACK_LOTTYPE" };

            _combo.SetCombo(cboType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");
        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
