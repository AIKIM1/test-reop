/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : 와인딩 이력카드 팝업
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
   2017.03.02   신광희C : UI 변경 수정 및 BizRule 수정
   2018.03.30   이상훈  [C20180308_28552] 이력카드 오투입 방지를 위한 이력카드 수정, 중복발행 방지 및 재발행 팝업창 발생 요청건
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

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_HISTORYCARD : C1Window, IWorkArea
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
        private string _selectEquipmentCode = string.Empty;
        private bool _isSmallType;
        private bool _isLoaded = false;

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

        public CMM_ASSY_HISTORYCARD()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            cboEquipment.ItemsSource = DataTableConverter.Convert(_dtequipment);
            if (!string.IsNullOrEmpty(_selectEquipmentCode))
            {
                cboEquipment.SelectedValue = _selectEquipmentCode;
            }
            else
            {
                cboEquipment.SelectedIndex = _selectdindex;
            }

            cboEquipmentSegmentAssy.ItemsSource = DataTableConverter.Convert(_dtEquipmentSegment);
            cboEquipmentSegmentAssy.SelectedIndex = _equipmentSegmentselectedIndex;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _dtequipment = tmps[0] as DataTable;
            _selectdindex = string.IsNullOrEmpty(tmps[1].ToString()) ? 0 : Convert.ToInt16(tmps[1].ToString());
            _dtEquipmentSegment = tmps[2] as DataTable;
            _equipmentSegmentselectedIndex = Convert.ToInt16(tmps[3]);
            _processCode = tmps[4].GetString();
            _isSmallType = (bool) tmps[5];

            if(tmps.Length > 6)
                _selectEquipmentCode = tmps[6].GetString();

            InitCombo();
            GetWindingList();
            //dtpProdDate.SelectedDateTime = System.DateTime.Now;
            this.BringToFront();
            _isLoaded = true;
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
                //C20180308_28552 발행 수 메시지 출력
                //Util.MessageValidation("SFU4905", Convert.ToString(DataTableConverter.GetValue(gdWindingList.Rows[rowIndex].DataItem, "PRT_COUNT")));
                Util.MessageInfo("SFU4905", (msgResult) =>
                {
                    if (msgResult == MessageBoxResult.OK )
                    {
                        //const string bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WN";
                        //string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN";

                        string outPutData = _isSmallType ? "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA,OUT_TRAY" : "OUT_DATA,OUT_ELEC";
                        string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN_V01";

                        if (_processCode.Equals(Process.WINDING_POUCH) || _processCode.Equals(Process.TAPING_POUCH))
                        {
                            outPutData = "OUT_DATA,OUT_ELEC";
                            bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WN_V01_WP";
                        }

                        DataSet ds = _bizRule.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                        DataTable indataTable = ds.Tables["IN_DATA"];
                        DataRow indata = indataTable.NewRow();

                        indata["LANGID"] = LoginInfo.LANGID;
                        indata["LOTID"] = Convert.ToString(DataTableConverter.GetValue(gdWindingList.Rows[rowIndex].DataItem, "LOTID"));

                        indataTable.Rows.Add(indata);

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", outPutData, (result, exception) =>
                        {
                            try
                            {
                                if (exception != null)
                                {
                                    Util.MessageException(exception);
                                    return;
                                }
                                else
                                {
                                    CMM_ASSY_WINDERCARD_PRINT poopupHistoryCard = new CMM_ASSY_WINDERCARD_PRINT { FrameOperation = this.FrameOperation };
                                    object[] parameters = new object[5];
                                    parameters[0] = result;
                                    parameters[1] = _isSmallType;
                                    parameters[2] = Convert.ToString(DataTableConverter.GetValue(gdWindingList.Rows[rowIndex].DataItem, "LOTID"));
                                    parameters[3] = _processCode;
                                    parameters[4] = false;
                                    C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                                    poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);
                                    this.Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));

                                    //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                                    //{
                                    //    if (tmp.Name == "grdMain")
                                    //    {
                                    //        tmp.Children.Add(poopupHistoryCard);
                                    //        poopupHistoryCard.BringToFront();
                                    //        break;
                                    //    }
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, ds);
                    }
                }, Convert.ToString(DataTableConverter.GetValue(gdWindingList.Rows[rowIndex].DataItem, "PRT_COUNT")));

                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDERCARD_PRINT popup = sender as CMM_ASSY_WINDERCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetWindingList();
            }
            //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (tmp.Name == "grdMain")
            //    {
            //        tmp.Children.Remove(popup);
            //    }
            //}
            //this.grdMain.Children.Remove(popup);
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

                string bizRuleName = _isSmallType ? "DA_BAS_SEL_WINDING_RUNCARD_LIST_WNS" : "DA_BAS_SEL_WINDING_RUNCARD_LIST_WN";

                DataTable indataTable = _bizRule.GetDA_BAS_SEL_WINDING_RUNCARD_LIST();
                DataRow indata = indataTable.NewRow();

                if (cboEquipment.SelectedIndex > 0)
                    indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                if(cboEquipmentSegmentAssy.SelectedIndex > 0)
                    indata["EQSGID"] = Util.NVC(cboEquipmentSegmentAssy.SelectedValue);

                if (string.IsNullOrEmpty(txtLotId.Text.Trim()) && string.IsNullOrEmpty(txtRuncardId.Text.Trim()))
                {
                    indata["FRCALDATE"] = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                    indata["TOCALDATE"] = DateTime.Today.ToString("yyyy-MM-dd");
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

        private void cboEquipmentSegmentAssy_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(_isLoaded)
                SetEquipmentCombo(cboEquipment);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            //원각 : CR , 초소형 : CS
            //string gubun = _isSmallType ? "CS" : "CR";

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegmentAssy.SelectedValue.GetString(), _processCode, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }
    }

}
