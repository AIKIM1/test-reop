/*************************************************************************************
 Created Date : 2019.05.08
      Creator : 정문교
   Decription : Carrier 조회
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ASSY_CARRIER_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CARRIER_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_CARRIER_INFO()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetControl();
            Loaded -= C1Window_Loaded;
        }

        private void InitializeControls()
        {
            Util.gridClear(dgCell);
        }

        private void SetParameters()
        {
        }

        private void SetControl()
        {
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// 그리드 선택
        /// </summary>
        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                // row 색 바꾸기
                dgList.SelectedIndex = idx;
                // cell 정보 조회
                SearchCell(DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "LOTID").ToString(),
                           DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "CSTID").ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Carrier 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                InitializeControls();

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_CELLMAP_OUTLOT_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(String));
                dtRqst.Columns.Add("LOTID", typeof(String));
                dtRqst.Columns.Add("CSTID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                dr["CSTID"] = string.IsNullOrWhiteSpace(txtCarrierID.Text) ? null : txtCarrierID.Text;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 1)
                    {
                        bizResult.Rows[0]["CHK"] = 1;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation);

                    if (bizResult.Rows.Count == 1)
                    {
                        SearchCell(bizResult.Rows[0]["LOTID"].ToString(), bizResult.Rows[0]["CSTID"].ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell 조회
        /// </summary>
        private void SearchCell(string LotID, string CarrierID)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_CELLMAP_CELL_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(String));
                dtRqst.Columns.Add("CSTID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                dr["CSTID"] = CarrierID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgCell, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

        private bool ValidationSearch()
        {
            if (string.IsNullOrWhiteSpace(txtCarrierID.Text) && string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                if (string.IsNullOrWhiteSpace(txtCarrierID.Text))
                {
                    //  % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", tbCarrier.Text);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    //  % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", tbLot.Text);
                    return false;
                }
            }

            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}