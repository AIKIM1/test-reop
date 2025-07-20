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
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_053 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        #region Declaration & Constructor 
        public PGM_GUI_053()
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
        }
        #endregion

        #region Mehod
        //private Boolean SelectOutHistToExcelFile(string sLot_ID)
        //{

        //}
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string sLotID = string.Empty;
            Boolean bResult = true;

            if (dgOutHist.Rows.Count > 0)
            {
                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") > 0)
                {
                    if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                    {
                        sLotID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "LOT_ID").ToString();

                        //SelectOutHistToExcelFile(sLotID)
                    }
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 이력을 조회 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPalletID.Text.ToString()))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Pallet ID 가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}