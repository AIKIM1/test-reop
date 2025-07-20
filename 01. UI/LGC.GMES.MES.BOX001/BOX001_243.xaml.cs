/*************************************************************************************
 Created Date : 2020.04.01
      Creator : 비즈테크 이동우S
   Decription : CELL SCRAP 처리 (VISION NG, DIMENSION NG) AUTO SCRAP
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.UserControls;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_243 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private DataTable _scarpTable = new DataTable();
        private DataTable _unscarpTable = new DataTable();
        private DataTable _cellInfoTable = new DataTable();

        private string _emptyLot = string.Empty;
        private string _emptyScrapCell = string.Empty;
        private string _emptyUnScrapCell = string.Empty;
        CommonCombo _combo = new CMM001.Class.CommonCombo();


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }


        public BOX001_243()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {

        }
        private void SetControl()
        {
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            String[] sFilter = { LoginInfo.CFG_AREA_ID };    // Area
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

           // String[] sFilter = { LoginInfo.CFG_AREA_ID };    // Area
            _combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnDelete);
            //listAuth.Add(btnSave);
            //listAuth.Add(btnSearchLot);
            //listAuth.Add(btnSearchHis);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            txtScanQtySrap.Value = 0;
            txtScanQtyUnScrap.Value = 0;
            txtCellIDtoScrap.Focus();

            this.Loaded -= UserControl_Loaded;
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgScrapInfo);
            txtScanQtySrap.Focus();
            _scarpTable = DataTableConverter.Convert(dgScrapInfo.GetCurrentItems());

            txtScanQtySrap.Value = _scarpTable.Rows.Count;
            txtScrapNote.Text = string.Empty;
        }
        private void btnClear2_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgUnScrapInfo);
            txtCellIDtoScrap.Focus();
            _unscarpTable = DataTableConverter.Convert(dgUnScrapInfo.GetCurrentItems());

            txtScanQtyUnScrap.Value = _unscarpTable.Rows.Count;
        }

        private void btnSearchCellInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoEvents();
                Util.gridClear(dgCellInfo);

                const string bizRuleName = "BR_PRD_GET_CELL_SCRAP_INFO";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SUBLOTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["SUBLOTID"] = txtCellInfo.Text;
                inTable.Rows.Add(dr);

                System.Windows.Forms.Application.DoEvents();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCellInfo, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            e.Handled = true;
        }

        private void txtUnScrapCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();
                    //    DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    Util.gridClear(dgUnScrapCellHistory);

                    bool bLot = false;

                    _emptyUnScrapCell = Clipboard.GetText();

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("ACTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = String.IsNullOrEmpty(_emptyUnScrapCell) ? null : _emptyUnScrapCell;
                    dr["ACTID"] = "CANCEL_SCRAP_SUBLOT";
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromRealese);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToRealese);

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgUnScrapCellHistory, dtRslt, FrameOperation);
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
        }
        private void txtCellIDScrap_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();
                    // DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    Util.gridClear(dgScrapCellHistory);

                    _emptyScrapCell = Clipboard.GetText();

                    bool bLot = false;

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("ACTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = String.IsNullOrEmpty(_emptyScrapCell) ? null : _emptyScrapCell;
                    dr["ACTID"] = "SCRAP_SUBLOT";
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgScrapCellHistory, dtRslt, FrameOperation);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
                e.Handled = true;

            }
        }

        private void txtCellIDScrap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearchCellScrap_Click(null, null);
            }
        }

        private void txtCellInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    DoEvents();
                    Util.gridClear(dgCellInfo);

                    const string bizRuleName = "BR_PRD_GET_CELL_SCRAP_INFO";

                    ShowLoadingIndicator();

                    DataTable inTable = new DataTable("INDATA");
                    inTable.Columns.Add("SUBLOTID", typeof(string));
                    DataRow dr = inTable.NewRow();
                    dr["SUBLOTID"] = txtCellInfo.Text;
                    inTable.Rows.Add(dr);

                    System.Windows.Forms.Application.DoEvents();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgCellInfo, dtResult, FrameOperation);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void txtCellInfo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    DoEvents();
                    Util.gridClear(dgCellInfo);

                    const string bizRuleName = "BR_PRD_GET_CELL_SCRAP_INFO";

                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    DataTable inTable = new DataTable("INDATA");
                    inTable.Columns.Add("SUBLOTID", typeof(string));


                    foreach (string item in sPasteStrings)
                    {
                        DataRow dr = inTable.NewRow();
                        dr["SUBLOTID"] = item;
                        inTable.Rows.Add(dr);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgCellInfo, dtResult, FrameOperation);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
            txtCellInfo.Focus();
        }
        private void txtCellIDtoScrap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList("scrap");
            }
            txtCellIDtoScrap.Focus();
        }

        private void btnUserUc_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindowUn();
        }

        private void txtCellIDtoScrap_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Cell(item, "scrap") == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (!string.IsNullOrEmpty(_emptyLot))
                    {
                        Util.MessageValidation("SFU3588", _emptyLot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        _emptyLot = string.Empty;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
            txtCellIDtoScrap.Focus();
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void txtDefectCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Cell(item,"unscrap") == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (!string.IsNullOrEmpty(_emptyLot))
                    {
                        Util.MessageValidation("SFU3588", _emptyLot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        _emptyLot = string.Empty;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
            txtDefectCell.Focus();
        }

        private void txtDefectCell_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList("unscrap");
            }
            txtDefectCell.Focus();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((Button)sender).Parent).Row.Index;

                    dgScrapInfo.IsReadOnly = false;
                    dgScrapInfo.RemoveRow(index);
                    dgScrapInfo.IsReadOnly = true;
                    _scarpTable = DataTableConverter.Convert(dgScrapInfo.GetCurrentItems());

                    txtCellIDtoScrap.Focus();
                    txtScanQtySrap.Value = _scarpTable.Rows.Count;

                }
            });
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((Button)sender).Parent).Row.Index;

                    dgUnScrapInfo.IsReadOnly = false;
                    dgUnScrapInfo.RemoveRow(index);
                    dgUnScrapInfo.IsReadOnly = true;
                    _unscarpTable = DataTableConverter.Convert(dgUnScrapInfo.GetCurrentItems());

                    txtCellIDtoScrap.Focus();
                    txtScanQtyUnScrap.Value = _unscarpTable.Rows.Count;

                }
            });
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave("scrap"))
                    return;

            // 생성 하시겠습니까?
            Util.MessageConfirm("SFU8202", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save_Scrap();
                }
                txtScrapNote.Text = string.Empty;
            });
        }
        private void btnUnscrap_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave("unscarp"))
                return;

            // 생성 하시겠습니까?
            Util.MessageConfirm("SFU8203", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save_UnScrap();
                }
                txtUnScrapNote.Text = string.Empty;
            });
        }
        private void btnSearchCellScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgScrapCellHistory);

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = String.IsNullOrEmpty(txtCellIDScrap.Text) ? null : txtCellIDScrap.Text;
                dr["ACTID"] = "SCRAP_SUBLOT";
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["EQSGID"] = cboEquipmentSegment.SelectedIndex == 0 ? null : cboEquipmentSegment.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgScrapCellHistory, dtRslt, FrameOperation);
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
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void txtUserNameUc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindowUn();
            }
        }
        private void btnSearchCellUnScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgUnScrapCellHistory);

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = String.IsNullOrEmpty(txtUnScrapCell.Text) ? null : txtUnScrapCell.Text;
                dr["ACTID"] = "CANCEL_SCRAP_SUBLOT";
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFromRealese);
                dr["TO_DATE"] = Util.GetCondition(dtpDateToRealese);
                dr["EQSGID"] = cboEquipmentSegment2.SelectedIndex == 0 ? null : cboEquipmentSegment2.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgUnScrapCellHistory, dtRslt, FrameOperation);
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

        private void txtUnScrapCell_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            { 
                btnSearchCellUnScrap_Click(null, null);
            }
        }


        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if((e.Cell.Column.Name.Equals("SUBLOTSCRAP") || e.Cell.Column.Name.Equals("VISION_NG_JUDGE") || e.Cell.Column.Name.Equals("DIMENSION_NG_JUDGE")))
                    //{

                    //}
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SUBLOTSCRAP")), "Y") && (e.Cell.Column.Name.Equals("SUBLOTSCRAP")))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VISION_NG_JUDGE")), "NG") && e.Cell.Column.Name.Equals("VISION_NG_JUDGE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIMENSION_NG_JUDGE")), "NG") && e.Cell.Column.Name.Equals("DIMENSION_NG_JUDGE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));

        }
        private void dgCellInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = null;
                    }
                }
            }));

        }
        #endregion

        #region Methode
        private void Save_Scrap()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_CHK_AUTO_SCRAP_CELL";

                _scarpTable = DataTableConverter.Convert(dgScrapInfo.GetCurrentItems());

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = txtUserNameSc.Tag.ToString();
                row["IFMODE"] = "OFF";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("IN_SUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));
                inLot.Columns.Add("NOTE", typeof(string));
                DataRow row2 = inLot.NewRow();
                for (int i = 0; i < _scarpTable.Rows.Count; i++)
                {
                    row2 = inLot.NewRow();
                    row2["SUBLOTID"] = Util.NVC(_scarpTable.Rows[i]["SUBLOTID"]);
                    row2["NOTE"] = txtScrapNote.Text;
                    inLot.Rows.Add(row2);
                }

                DataSet dtRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_SUBLOT", "OUT_SUBLOT", inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", _scarpTable.Rows.Count);
                Util.gridClear(dgScrapInfo);
                _emptyLot = string.Empty;
                _scarpTable = new DataTable();

                txtScanQtySrap.Value = _scarpTable.Rows.Count;

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
        private void Save_UnScrap()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_REG_CANCEL_SCRAP_CELL";

                _unscarpTable = DataTableConverter.Convert(dgUnScrapInfo.GetCurrentItems());

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = txtUserNameUc.Tag.ToString();
                row["IFMODE"] = "OFF";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("IN_SUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));
                inLot.Columns.Add("NOTE", typeof(string));

                DataRow row2 = inLot.NewRow();
                for (int i = 0; i < _unscarpTable.Rows.Count; i++)
                {
                    row2 = inLot.NewRow();
                    row2["SUBLOTID"] = Util.NVC(_unscarpTable.Rows[i]["SUBLOTID"]);
                    row2["NOTE"] = txtUnScrapNote.Text;
                    inLot.Rows.Add(row2);
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_SUBLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", _unscarpTable.Rows.Count);
                Util.gridClear(dgUnScrapInfo);
                _emptyLot = string.Empty;
                _unscarpTable = new DataTable();
                
                txtScanQtyUnScrap.Value = _unscarpTable.Rows.Count;

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
        private bool CanSave(string scrap_chk)
        {
            if(scrap_chk == "scrap")
            {
                if (dgScrapInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2052");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtScrapNote.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrEmpty(txtUserNameSc.Text) || string.IsNullOrEmpty(txtUserNameSc.Tag?.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }

            }
            else
            {
                if (dgUnScrapInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2052");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUnScrapNote.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrEmpty(txtUserNameUc.Text) || string.IsNullOrEmpty(txtUserNameUc.Tag?.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }
            return true;
        }
        public void GetLotList(string scrap_state)
        {
            try
            {
                TextBox tb = txtCellIDtoScrap;
                TextBox utb = txtDefectCell;

                if (scrap_state == "scrap")
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        // CELLID를 스캔 또는 입력하세요.
                        Util.MessageInfo("SFU1323");
                        return;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(utb.Text))
                    {
                        // CELLID를 스캔 또는 입력하세요.
                        Util.MessageInfo("SFU1323");
                        return;
                    }
                }

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_MDLLOTID_BY_SUBLOT";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                if (scrap_state == "scrap")
                {
                    dr["SUBLOTID"] = txtCellIDtoScrap.Text;
                    inTable.Rows.Add(dr);
                }
                else
                {
                    dr["SUBLOTID"] = txtDefectCell.Text;
                    inTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if(dtResult.Rows.Count == 0)
                {
                    if (scrap_state == "scrap")
                    {
                        Util.MessageValidation("SFU8201", txtCellIDtoScrap.Text);
                        return;
                    }
                    else
                    {
                        Util.MessageValidation("SFU8201", txtDefectCell.Text);
                        return;
                    }
                }

                if (dgScrapInfo.GetRowCount() == 0)
                {
                    if(scrap_state == "scrap")
                    { 
                         Util.GridSetData(dgScrapInfo, dtResult, FrameOperation);
                    }
                    else
                    {
                        Util.GridSetData(dgUnScrapInfo, dtResult, FrameOperation);
                    }
                }
                else
                {
                    for(int i = 0; i < dgScrapInfo.GetRowCount(); i++)
                    {
                       if( Util.NVC(DataTableConverter.GetValue(dgScrapInfo.Rows[i].DataItem, "SUBLOTID")) == txtCellIDtoScrap.Text)
                        {
                            Util.MessageInfo("SFU3164");
                            return;
                        }
                    }
                    if (scrap_state == "scrap")
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgScrapInfo.ItemsSource);
                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgScrapInfo, dtInfo, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgUnScrapInfo.ItemsSource);
                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgUnScrapInfo, dtInfo, FrameOperation);
                    }
                }
                if (scrap_state == "scrap")
                {
                    _scarpTable = DataTableConverter.Convert(dgScrapInfo.GetCurrentItems());
                    txtScanQtySrap.Value = _scarpTable.Rows.Count;
                }
                else
                {
                    _unscarpTable = DataTableConverter.Convert(dgUnScrapInfo.GetCurrentItems());
                    txtScanQtyUnScrap.Value = _unscarpTable.Rows.Count;
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
        }
        public void GetCellInfo()
        {
            try
            {
                TextBox tb = txtCellInfo;

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // CELLID를 스캔 또는 입력하세요.
                    Util.MessageInfo("SFU1323");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_GET_CELL_SCRAP_INFO";

                //  DataSet inData = new DataSet("INDATA");

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SUBLOTID"] = txtCellInfo.Text;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dgCellInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgCellInfo, dtResult, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID")) == txtCellInfo.Text)
                        {
                            Util.MessageInfo("SFU3164");
                            return;
                        }
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgCellInfo.ItemsSource);
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgCellInfo, dtInfo, FrameOperation);
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        bool Multi_Cell(string cell_ID, string scarp_chk)
        {
            try
            {
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_MDLLOTID_BY_SUBLOT";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SUBLOTID"] = cell_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);


                if (dtResult.Rows.Count == 0)
                {
                    if (string.IsNullOrEmpty(_emptyLot))
                        _emptyLot += cell_ID;
                    else
                        _emptyLot = _emptyLot + ", " + cell_ID;
                }

                if(scarp_chk == "scrap")
                {
                    if (dgScrapInfo.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgScrapInfo, dtResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgScrapInfo.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgScrapInfo, dtInfo, FrameOperation);
                    }

                    _scarpTable = DataTableConverter.Convert(dgScrapInfo.GetCurrentItems());
                    txtScanQtySrap.Value = _scarpTable.Rows.Count;
                }
                else
                {
                    if (dgUnScrapInfo.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgUnScrapInfo, dtResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgUnScrapInfo.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgUnScrapInfo, dtInfo, FrameOperation);
                    }

                    _unscarpTable = DataTableConverter.Convert(dgUnScrapInfo.GetCurrentItems());
                    txtScanQtyUnScrap.Value = _unscarpTable.Rows.Count;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName = txtUserNameSc.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += wndUser_Closed;
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }
        private void GetUserWindowUn()
        {
            CMM_PERSON wndPersonUn = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName = txtUserNameUc.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPersonUn, parameters);

            wndPersonUn.Closed += wndUserUn_Closed;
            grdMain.Children.Add(wndPersonUn);
            wndPersonUn.BringToFront();
        }
        #endregion

        #region [Func]
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameSc.Text = wndPerson.USERNAME;
                txtUserNameSc.Tag = wndPerson.USERID;
            }
        }
        private void wndUserUn_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPersonUn = sender as CMM_PERSON;
            if (wndPersonUn != null && wndPersonUn.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameUc.Text = wndPersonUn.USERNAME;
                txtUserNameUc.Tag = wndPersonUn.USERID;
            }
        }



        #endregion

    }
}
