/*************************************************************************************
 Created Date : 2023.01.26
      Creator : KANG DONG HEE
   Decription : 전체 Aging Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2023.01.26  DEVELOPER : Initial Created.
**************************************************************************************/
#define SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_012_ALL : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS002_012_ALL()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            object[] objParent = { "FORM_AGING_TYPE_CODE", "N" };
            ComCombo.SetComboObjParent(cboAgingType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", objParent: objParent);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void chkOnlyAll_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkOnlyAll_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void dgAgingStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    string sROW = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW"));
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!chkOnlyAll.IsChecked.Equals(true) && sROW.Equals("ALL"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgAgingStatus_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AGING_TYPE", typeof(string));
                dtRqst.Columns.Add("ONLY_ALL_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AGING_TYPE"] = Util.GetCondition(cboAgingType, bAllNull: true);
                dr["ONLY_ALL_YN"] = ((bool)chkOnlyAll.IsChecked) ? "Y" : "N";
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_GET_AGING_RACK_STATUS_MB", "INDATA", "OUTDATA,OUTDATA_ALL", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTDATA"].Rows.Count > 0 && bizResult.Tables["OUTDATA_ALL"].Rows.Count > 0)
                        {
                            if (!chkOnlyAll.IsChecked.Equals(true))
                            {
                                dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Visible;

                                Util _Util = new Util();
                                string[] sColumnName = new string[] { "AGING_TYPE_NAME", "EQPT_NAME" };
                                _Util.SetDataGridMergeExtensionCol(dgAgingStatus, sColumnName, DataGridMergeMode.VERTICAL);

                                Util.GridSetData(dgAgingStatus, bizResult.Tables["OUTDATA"], FrameOperation, true);
                            }
                            else
                            {
                                dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Collapsed;
                                Util.GridSetData(dgAgingStatus, bizResult.Tables["OUTDATA_ALL"], FrameOperation, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, dsRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void dgAgingStatus_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //C1DataGrid grid = sender as C1DataGrid;

                //if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                //{
                //    var _mergeList = new List<DataGridCellsRange>();

                //    string saveCmdSeq = Util.NVC(grid.GetValue(2, "EQPT_NAME"));
                //    int mergeStart = 0, mergeEnd = 0;
                //    for (int row = 0; row < grid.Rows.Count; row++)
                //    {
                //        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) &&
                //            Util.NVC(grid.GetValue(row - 1, "EQPT_NAME")) != Util.NVC(grid.GetValue(row, "EQPT_NAME")))
                //        {
                //            break;
                //        }

                //        if (!saveCmdSeq.Equals(Util.NVC(grid.GetValue(row, "EQPT_NAME"))) ||
                //            row.Equals(grid.Rows.Count - 1))
                //        {
                //            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                //            if (mergeStart < mergeEnd)
                //            {
                //                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                //                {
                //                    switch (dgCol.Name)
                //                    {
                //                        case "EQP_RACKIBGO":
                //                        case "EQPT_NAME":
                //                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                //                            break;
                //                        default:
                //                            break;
                //                    }

                //                }
                //            }
                //            mergeStart = row;
                //            saveCmdSeq = Util.NVC(grid.GetValue(row, "EQPT_NAME"));
                //        }
                //    }
                //    foreach (var range in _mergeList)
                //    {
                //        e.Merge(range);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
