/*************************************************************************************
 Created Date : 2021.06.30
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - HOLD 재공 현황 : HOLD LOT LIST 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.06.30  조영대 : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_344_HOLD_LOT_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtData =null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_344_HOLD_LOT_LIST()
        {
            InitializeComponent();
        }

        #endregion


        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length < 1)
            {
                return;
            }

            dtData = parameters[0] as DataTable;
            if (dtData == null) return;

            InitControl();
        }

        private void InitControl()
        {
            dgLotList.SetItemsSource(dtData, FrameOperation, true);
        }

        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region Method

        #endregion

       



       


       
    }
}
