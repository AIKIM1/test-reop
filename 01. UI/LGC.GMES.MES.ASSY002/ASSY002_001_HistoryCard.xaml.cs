/*************************************************************************************
 Created Date : 2016.11.21
      Creator : JEONG JONGWON
   Decription : 반제품 투입 팝업
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_001_HistoryCard.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_001_HistoryCard : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        //private DataTable selectDt = null;
        private DataTable equipment = null;
        private int selectdindex = 0;
        private string _processCode = string.Empty;

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
        public ASSY002_001_HistoryCard()
        {
            InitializeComponent();
        }
        private void InitCombo()
        {
            cboEquipment.ItemsSource = DataTableConverter.Convert(equipment);
            cboEquipment.SelectedIndex = selectdindex;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_001_INPUT_PRODUCT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            // 설비
            equipment = tmps[0] as DataTable;

            // 선택된 설비
            if (string.IsNullOrEmpty(tmps[1].ToString()))
                selectdindex = 0;
            else
                selectdindex = Convert.ToInt16(tmps[1].ToString());

            _processCode = tmps[2].GetString();

            // INIT COMBO
            InitCombo();

            // SET DATETIME 
            dtpProdDate.SelectedDateTime = System.DateTime.Now;

            ////// SET DATA
            ////GetWindingList();

            this.BringToFront();
        }
        #endregion

        #region [DataGrid] - gdWindingList_CurrentCellChanged
        private void gdWindingList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            // 작업대상 CHECK CELL CLICK
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                chk.IsChecked = false;
                                            }
                                        }
                                    }

                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    // INIT 
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;

                        }

                    }
                }
            }));
        }

        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // SEARCH
            GetWindingList();
        }
        #endregion

        #region [출력]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexByCheck(gdWindingList, "CHK");
                if (rowIndex < 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                const string bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WN";
                DataSet ds = _bizRule.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable indataTable = ds.Tables["IN_DATA"];
                DataRow indata = indataTable.NewRow();

                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = Convert.ToString(DataTableConverter.GetValue(gdWindingList.Rows[rowIndex].DataItem, "LOTID"));

                indataTable.Rows.Add(indata);
                ds.Tables.Add(indataTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_DATA", "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA", ds);

                if (dsResult.Tables["OUT_DATA"].Rows.Count == 0)
                {
                    //Util.AlertInfo("Lot 정보가 없습니다.");
                    Util.MessageValidation("SFU1195");
                    return;
                }

                ////LGC.GMES.MES.ASSY002.ASSY002_PRINT _print = new LGC.GMES.MES.ASSY002.ASSY002_PRINT();
                LGC.GMES.MES.CMM001.CMM_ASSY_WINDERCARD_PRINT popupWinderCardPrint = new LGC.GMES.MES.CMM001.CMM_ASSY_WINDERCARD_PRINT();
                popupWinderCardPrint.FrameOperation = this.FrameOperation;

                // SET PARAMETER
                object[] parameters = new object[1];
                parameters[0] = dsResult;

                C1WindowExtension.SetParameters(popupWinderCardPrint, parameters);

                //_print.ShowModal();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupWinderCardPrint);
                        popupWinderCardPrint.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        private void GetWindingList()
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(gdWindingList);

                DataTable indataTable = _bizRule.GetDA_BAS_SEL_WINDING_RUNCARD_LIST();
                DataRow indata = indataTable.NewRow();

                if (cboEquipment.SelectedIndex > 0)
                    indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                indata["CALDATE"] = dtpProdDate.SelectedDateTime.ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(Util.NVC(txtLotId.Text.Trim())) && string.IsNullOrEmpty(Util.NVC(txtRuncardId.Text.Trim())))
                {
                    indata["CALDATE"] = dtpProdDate.SelectedDateTime.ToString("yyyy-MM-dd");
                }
                else
                {
                    indata["CALDATE"] = DBNull.Value;
                }
                indata["LOTID"] = Util.NVC(txtLotId.Text.Trim());
                indata["WINDING_RUNCARD_ID"] = Util.NVC(txtRuncardId.Text.Trim());
                indata["PROCID"] = _processCode;

                indataTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WINDING_RUNCARD_LIST", "INDATA", "RSLTDT", indataTable);

                if (dt.Rows.Count == 0)
                {
                    //Util.AlertInfo("Lot 정보가 없습니다.");
                    Util.MessageValidation("SFU1195");
                    return;
                }

                ////gdWindingList.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(gdWindingList, dt, FrameOperation, true);

                if (gdWindingList.GetRowCount() > 0)
                    gdWindingList.CurrentCell = gdWindingList.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]
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


        #endregion

    }
}
