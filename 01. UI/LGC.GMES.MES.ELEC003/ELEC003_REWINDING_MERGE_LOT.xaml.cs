/*************************************************************************************
 Created Date : 2021.01.26
      Creator : 정문교
   Decription : 재와인딩 MERGE LOT 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.26  정문교 : Initial Created.
    
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// ELEC003_REWINDING_MERGE_LOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC003_REWINDING_MERGE_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _lotID = string.Empty; 

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

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

        public ELEC003_REWINDING_MERGE_LOT()
        {
            InitializeComponent();
        }

        private void InitializeUserControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _lotID = tmps[0] as string;
        }

        private void SetControl()
        {
        }

        private void SetCombo()
        {
        }

        private void SetDataGridColumnVisibility()
        {
        }

        #endregion

        #region Event

        /// <summary>
        /// Form Load
        /// </summary>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetParameters();
            SetControl();
            SetCombo();
            SetDataGridColumnVisibility();

            // 조회
            SelectMergeList();
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string Wipstat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT"));

                    if (Wipstat == Wip_State.WAIT)
                    {
                        e.Cell.Presenter.Background = dg.SelectedBackground;
                    }
                }
            }));
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SelectMergeList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _lotID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_WIP_RW_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]

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