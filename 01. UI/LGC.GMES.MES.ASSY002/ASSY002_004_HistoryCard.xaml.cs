/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : 반제품 투입 팝업
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.

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
    public partial class ASSY002_004_HistoryCard : C1Window, IWorkArea
    {
        #region Declaration

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizRule = new BizDataSet();

        //private DataTable selectDt = null;
        private DataTable _dtequipment = null;
        private int _selectdindex = 0;

        //ComboEquipmentSegment
        private DataTable _dtEquipmentSegment = null;
        private int _equipmentSegmentselectedIndex = 0;

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

        public ASSY002_004_HistoryCard()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            cboEquipment.ItemsSource = DataTableConverter.Convert(_dtequipment);
            cboEquipment.SelectedIndex = _selectdindex;

            cboEquipmentSegmentAssy.ItemsSource = DataTableConverter.Convert(_dtEquipmentSegment);
            cboEquipmentSegmentAssy.SelectedIndex = _equipmentSegmentselectedIndex;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_004_INPUT_PRODUCT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _dtequipment = tmps[0] as DataTable;
            _selectdindex = string.IsNullOrEmpty(tmps[1].ToString()) ? 0 : Convert.ToInt16(tmps[1].ToString());
            _dtEquipmentSegment = tmps[2] as DataTable;
            _equipmentSegmentselectedIndex = Convert.ToInt16(tmps[3]);
            _processCode = tmps[4].GetString();
            InitCombo();

            //dtpProdDate.SelectedDateTime = System.DateTime.Now;
            this.BringToFront();
        }
        #endregion

        #region [DataGrid] - gdWindingList_CurrentCellChanged
        private void gdWindingList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell?.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             !(bool)checkBox.IsChecked))
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
                                                    if (chk != null) chk.IsChecked = false;
                                                }
                                            }
                                        }
                                  
                                    }
                                    else
                                    {
                                        var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                            box.IsChecked.HasValue &&
                                                            (bool)box.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                        }
                                    }
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
            GetWindingList();
        }
        #endregion

        #region [출력]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(gdWindingList, "CHK");
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
                //ds.Tables.Add(indataTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_DATA", "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA", ds);

                if (dsResult.Tables["OUT_DATA"].Rows.Count == 0)
                {
                    //Util.AlertInfo("Lot 정보가 없습니다.");
                    Util.MessageValidation("SFU1195");
                    return;
                }

                ////LGC.GMES.MES.ASSY002.ASSY002_PRINT _print = new LGC.GMES.MES.ASSY002.ASSY002_PRINT();
                CMM001.CMM_ASSY_WINDERCARD_PRINT popupWinderCardPrint = new CMM001.CMM_ASSY_WINDERCARD_PRINT{ FrameOperation = this.FrameOperation };

                object[] parameters = new object[1];
                parameters[0] = dsResult;

                C1WindowExtension.SetParameters(popupWinderCardPrint, parameters);

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
            try
            {
                ShowLoadingIndicator();
                Util.gridClear(gdWindingList);

                const string bizRuleName = "DA_BAS_SEL_WINDING_RUNCARD_LIST";

                DataTable indataTable = _bizRule.GetDA_BAS_SEL_WINDING_RUNCARD_LIST();
                DataRow indata = indataTable.NewRow();

                if (cboEquipment.SelectedIndex > 0)
                    indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                if (string.IsNullOrEmpty(txtLotId.Text.Trim()) && string.IsNullOrEmpty(txtRuncardId.Text.Trim()))
                {
                    indata["FRCALDATE"] = DateTime.Today.AddDays(-1).ToShortDateString();
                    indata["TOCALDATE"] = DateTime.Today.ToShortDateString();
                }
                else
                {
                    indata["FRCALDATE"] = null;
                    indata["TOCALDATE"] = null;
                }

                indata["LOTID"] = Util.NVC(txtLotId.Text.Trim());
                indata["WINDING_RUNCARD_ID"] = Util.NVC(txtRuncardId.Text.Trim());
                indata["PROCID"] = _processCode;

                indataTable.Rows.Add(indata);
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", indataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (!CommonVerify.HasTableRow(bizResult))
                    {
                        Util.MessageValidation("SFU1195");
                        return;
                    }

                    Util.GridSetData(gdWindingList, bizResult, FrameOperation, true);

                    if (gdWindingList.GetRowCount() > 0)
                        gdWindingList.CurrentCell = gdWindingList.GetCell(0, 1);

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region[[Validation]
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
