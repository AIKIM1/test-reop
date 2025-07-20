/*************************************************************************************
 Created Date : 2018.09.04
      Creator : 오화백
   Decription : FCS 셀 측정 데이터 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.04  오화백 : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_251 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public COM001_251()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                //K-VALUE 등급판성
                String[] sFilterExclFlag = { "", "KVAL_SPEC_GRD_CODE" };
                _combo.SetCombo(cboKGrade, CommonCombo.ComboStatus.ALL, sFilter: sFilterExclFlag, sCase: "COMMCODES");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        #region 조회 - 버튼 btnSearch()
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        #endregion

        #region 조회 - CellID  txtCellID_KeyDown()
        /// <summary>
        /// Cell ID 엔터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SearchData();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 조회 - TrayID txtTrayID_KeyDown()
        /// <summary>
        /// TrayID 엔터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTrayID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SearchData();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 조회 - LOTID txtLotId_KeyDown()
        /// <summary>
        /// LOTID 엔터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SearchData();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 조회 SearchData();
        private void SearchData()
        {
            try
            {
                if (txtCellID.Text.Trim() == string.Empty && txtTrayID.Text.Trim() == string.Empty && txtLotId.Text.Trim() == string.Empty && cboKGrade.SelectedIndex == 0)
                {
                    //조회조건이 하나라도 있어야 합니다.
                    Util.MessageValidation("SFU5018");
                    return;
                }
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("CELLID", typeof(string));
                IndataTable.Columns.Add("TRAYID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("KVALUE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["CELLID"] = txtCellID.Text.Trim() == "" ? null : txtCellID.Text.Trim();
                Indata["TRAYID"] = txtTrayID.Text.Trim() == "" ? null : txtTrayID.Text.Trim();
                Indata["LOTID"] = txtLotId.Text.Trim() == "" ? null : txtLotId.Text.Trim();

                Indata["KVALUE"] = Util.GetCondition(cboKGrade, bAllNull: true);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_INF_CCT1_SEL_CELL_MEASR_DATA_UI", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgFCSCell, result, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        #endregion

        #endregion
    
    }
}
