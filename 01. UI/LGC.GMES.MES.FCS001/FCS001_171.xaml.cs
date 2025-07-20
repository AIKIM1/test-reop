/*************************************************************************************
 Created Date : 2024.03.14 
      Creator : 
   Decription : CDC CDLL HOLD 등록/해제 및 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.14  전민식 : Initial Created
  2024.03.20  조영대 : 조회 및 등록 수정
**************************************************************************************/

using C1.WPF; 
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_171 : UserControl, IWorkArea
    {
        public FCS001_171()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            dtpSearchDate.SelectedFromDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpSearchDate.SelectedToDateTime = (DateTime)System.DateTime.Now;

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE");

            cboHoldYN.SetCommonCode("YORN", CommonCombo.ComboStatus.ALL, false);
        }

        #endregion

        #region Event

        #region 등록

        private void btnInsertClear_Click(object sender, RoutedEventArgs e)
        {
            dgInsert.ClearValidation();
            dgInsert.ClearRows();
            txtInsertCellCnt.Text = "0";
        }

        private void Delete_Insert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null || presenter.Row == null) return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = dgInsert.GetDataTable(false);
                dt.Rows.RemoveAt(clickedIndex);

                CellCountDisplay(dgInsert);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInsertSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgInsert.ClearValidation();

                if (dgInsert.GetRowCount() == 0) return;

                DataTable dtInsert = dgInsert.GetDataTable(false);
                int dataCount = dtInsert.AsEnumerable().Where(w => !w.Field<string>("SUBLOTID").Nvc().Equals(string.Empty)).Count();
                if (dataCount == 0)
                {
                    //등록할 대상이 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                    });
                    return;
                }

                // 빈값 삭제
                dtInsert.AsEnumerable().Where(w => w.Field<string>("SUBLOTID").Nvc().Equals(string.Empty)).ToList().ForEach(w => { w.Delete(); });
                dtInsert.AcceptChanges();

                CellCountDisplay(dgInsert);

                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0124", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string BizRuleID = "BR_SET_SUBLOT_HOLD_CDC"; 

                        int iProcessingCnt = 100; //한번에 처리하는 Tray 건수
                        double dNumberOfProcessingCnt = 0.0;
                        bool bIsOK = true;

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("WORK_TYPE", typeof(string));
                        inDataTable.Columns.Add("AREAID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                        inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                        inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                        inDataTable.Columns.Add("SHOPID", typeof(string));
                        inDataTable.Columns.Add("HOLD_GR_ID", typeof(string));

                        DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                        inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                        inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                        DataTable inTable = inDataSet.Tables["INDATA"];
                        DataRow newRow = inDataTable.NewRow();
                        newRow["WORK_TYPE"] = "REGISTER";
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["HOLD_NOTE"] = "CDC CELL HOLD";
                        newRow["HOLD_TRGT_CODE"] = "SUBLOT";
                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        inDataTable.Rows.Add(newRow);

                        ShowLoadingIndicator();
                        dNumberOfProcessingCnt = Math.Ceiling(dgInsert.Rows.Count / (double)iProcessingCnt);//처리수량

                        for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                        {
                            inHoldTable.Clear();

                            for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                            {
                                if (i >= dgInsert.Rows.Count) break;

                                string sublotID = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID"));
                                if (string.IsNullOrEmpty(sublotID)) continue;

                                DataRow RowCell = inHoldTable.NewRow();
                                RowCell["ASSY_LOTID"] = null;
                                RowCell["STRT_SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID"));
                                RowCell["END_SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID"));
                                RowCell["HOLD_REG_QTY"] = 1;
                                inHoldTable.Rows.Add(RowCell);
                            }

                            try
                            {
                                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INHOLD", "OUTDATA,OUTHOLD", inDataSet);
                                if (dsResult != null && dsResult.Tables.Count > 0)
                                {
                                    // 메세지 업데이트
                                    dtInsert.AsEnumerable().ToList().ForEach(w =>
                                    {
                                        DataRow dr = dsResult.Tables["OUTDATA"].AsEnumerable().Where(x => x.Field<string>("SUBLOTID").Nvc().Equals(w.Field<string>("SUBLOTID").Nvc())).FirstOrDefault();
                                        if (dr != null)
                                        {
                                            switch (dr["RESULT_CODE"].Nvc())
                                            {
                                                case "001":
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU5118");
                                                    w["ERROR_CHK"] = "ERROR";
                                                    break;
                                                case "002":
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU8429");
                                                    w["ERROR_CHK"] = "ERROR";
                                                    break;
                                                case "003":
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU8429");
                                                    w["ERROR_CHK"] = "ERROR";
                                                    break;
                                                case "004":
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("FM_ME_0534");
                                                    w["ERROR_CHK"] = "ERROR";
                                                    break;
                                                default:
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU1889");
                                                    w["ERROR_CHK"] = null;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU1889");
                                            w["ERROR_CHK"] = null;
                                        }
                                    });
                                    dtInsert.AcceptChanges();
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                                bIsOK = false;
                                HiddenLoadingIndicator();
                                return;
                            }
                        }

                        if (bIsOK)
                        {
                            //저장하였습니다.
                            Util.MessageInfo("FM_ME_0215");
                        }
                        else
                        {
                            //저장실패하였습니다.
                            Util.MessageInfo("FM_ME_0213");
                        }

                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInsert_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ERROR_CHK")).Equals("ERROR"))
                        {
                            e.Cell.Presenter.Foreground = Brushes.Red;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = Brushes.Black;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtAddCellId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetCellList(dgInsert, txtAddCellId.Text);
                    txtAddCellId.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtAddCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            try
            {
                string[] stringSeparators = new string[] { "," };
                string[] sPasteStrings = text.Split(stringSeparators, StringSplitOptions.None);

                SetCellList(dgInsert, text);
                txtAddCellId.Clear();

                e.Handled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            e.Handled = true;
        }
        #endregion

        #region 복구

        private void btnReleaseClear_Click(object sender, RoutedEventArgs e)
        {
            dgRelease.ClearValidation();
            dgRelease.ClearRows();
            txtReleaseCellCnt.Text = "0";
        }

        private void Delete_Release_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null || presenter.Row == null) return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = dgRelease.GetDataTable(false);
                dt.Rows.RemoveAt(clickedIndex);

                CellCountDisplay(dgRelease);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReleaseSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgRelease.ClearValidation();
                if (dgRelease.GetRowCount() == 0) return;

                DataTable dtRelease = dgRelease.GetDataTable(false);
                int dataCount = dgRelease.AsEnumerable().Where(w => !w.Field<string>("SUBLOTID").Nvc().Equals(string.Empty)).Count();
                if (dataCount == 0)
                {
                    //등록할 대상이 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                    });
                    return;
                }

                // 빈값 삭제
                dtRelease.AsEnumerable().Where(w => w.Field<string>("SUBLOTID").Nvc().Equals(string.Empty)).ToList().ForEach(w => { w.Delete(); });
                dtRelease.AcceptChanges();

                CellCountDisplay(dgRelease);

                //복구하시겠습니까?
                Util.MessageConfirm("FM_ME_0141", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string BizRuleID = "BR_SET_SUBLOT_HOLD_CDC";

                        int iProcessingCnt = 100; //한번에 처리하는 Tray 건수
                        double dNumberOfProcessingCnt = 0.0;
                        bool bIsOK = true;

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("WORK_TYPE", typeof(string));
                        inDataTable.Columns.Add("AREAID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                        inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                        inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                        inDataTable.Columns.Add("SHOPID", typeof(string));
                        inDataTable.Columns.Add("HOLD_GR_ID", typeof(string));

                        DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                        inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                        inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                        DataTable inTable = inDataSet.Tables["INDATA"];
                        DataRow newRow = inDataTable.NewRow();
                        newRow["WORK_TYPE"] = "RELEASE";
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["HOLD_NOTE"] = "CDC CELL HOLD";
                        newRow["HOLD_TRGT_CODE"] = "SUBLOT";
                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        inDataTable.Rows.Add(newRow);

                        ShowLoadingIndicator();
                        dNumberOfProcessingCnt = Math.Ceiling(dgRelease.Rows.Count / (double)iProcessingCnt);//처리수량

                        for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                        {
                            inHoldTable.Clear();

                            for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                            {
                                if (i >= dgRelease.Rows.Count) break;

                                string sublotID = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "SUBLOTID"));
                                if (string.IsNullOrEmpty(sublotID)) continue;

                                DataRow RowCell = inHoldTable.NewRow();
                                RowCell["ASSY_LOTID"] = null;
                                RowCell["STRT_SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "SUBLOTID"));
                                RowCell["END_SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "SUBLOTID"));
                                RowCell["HOLD_REG_QTY"] = 1;
                                inHoldTable.Rows.Add(RowCell);
                            }

                            try
                            {
                                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INHOLD", "OUTDATA,OUTHOLD", inDataSet);
                                if (dsResult != null && dsResult.Tables.Count > 0)
                                {
                                    // 메세지 업데이트
                                    dtRelease.AsEnumerable().ToList().ForEach(w =>
                                    {
                                        DataRow dr = dsResult.Tables["OUTDATA"].AsEnumerable().Where(x => x.Field<string>("SUBLOTID").Nvc().Equals(w.Field<string>("SUBLOTID").Nvc())).FirstOrDefault();
                                        if (dr != null)
                                        {
                                            switch (dr["RESULT_CODE"].Nvc())
                                            {
                                                case "101":
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("1175", dr["SUBLOTID"].Nvc());
                                                    w["ERROR_CHK"] = "ERROR";
                                                    break;
                                                default:
                                                    w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU1889");
                                                    w["ERROR_CHK"] = null;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            w["MESSAGE"] = MessageDic.Instance.GetMessage("SFU1889");
                                            w["ERROR_CHK"] = null;
                                        }
                                    });
                                    dtRelease.AcceptChanges();
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                                bIsOK = false;
                                HiddenLoadingIndicator();
                                return;
                            }
                        }

                        if (bIsOK)
                        {
                            //저장하였습니다.
                            Util.MessageInfo("FM_ME_0215");
                        }
                        else
                        {
                            //저장실패하였습니다.
                            Util.MessageInfo("FM_ME_0213");
                        }

                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }         
        }

        private void dgRelease_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ERROR_CHK")).Equals("ERROR"))
                        {
                            e.Cell.Presenter.Foreground = Brushes.Red;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = Brushes.Black;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtReleaseCellId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetCellList(dgRelease, txtReleaseCellId.Text);
                    txtReleaseCellId.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtReleaseCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            try
            {
                string[] stringSeparators = new string[] { "," };
                string[] sPasteStrings = text.Split(stringSeparators, StringSplitOptions.None);

                SetCellList(dgRelease, text);
                txtReleaseCellId.Clear();

                e.Handled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            e.Handled = true;
        }

        #endregion

        #region 조회

        private void dtpSearchDate_DateTimeChanged(object sender, UcBaseDateTimePicker.DateTimeChangedEventArgs e)
        {
            if (new TimeSpan(e.SelectedToDate.Ticks).Days - new TimeSpan(e.SelectedFromDate.Ticks).Days > 30)
            {
                //조회기간은 30일을 초과 할 수 없습니다.
                dtpSearchDate.SetValidation("SFU4466");

                if (e.SelectedTarget == UcBaseDateTimePicker.SelectedTargetType.FromDate)
                {
                    dtpSearchDate.SelectedToDateTime = dtpSearchDate.SelectedFromDateTime.AddDays(30);
                }
                else if (e.SelectedTarget == UcBaseDateTimePicker.SelectedTargetType.ToDate)
                {
                    dtpSearchDate.SelectedFromDateTime = dtpSearchDate.SelectedToDateTime.AddDays(-30);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null) return;

                if (datagrid.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS001_021 wndRunStart = new FCS001_021();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cell.Text;

                        this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            dgInsert.ClearValidation();
            dgInsert.AddRowData(numAddRow.Value.NvcInt());

            CellCountDisplay(dgInsert);
        }

        private void btnReleaseAddRow_Click(object sender, RoutedEventArgs e)
        {
            dgRelease.ClearValidation();
            dgRelease.AddRowData(numReleaseAddRow.Value.NvcInt());

            CellCountDisplay(dgRelease);
        }

        #endregion

        #endregion

        #region Method
        private void SetCellList(UcBaseDataGrid dataGrid, string cellID)
        {
            try
            {
                dataGrid.ClearValidation();

                List<string> CellList = null;

                if (string.IsNullOrEmpty(cellID)) return;

                string[] stringSeparators = new string[] { "," };
                CellList = cellID.Split(stringSeparators, StringSplitOptions.None).ToList<string>();

                //스프레드에 있는지 확인
                bool isDuplicate = false;
                for (int inx = CellList.Count - 1; inx >= 0; inx--)
                {
                    for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    {
                        if (dataGrid.GetValue(i, "SUBLOTID").Equals(CellList[inx]))
                        {
                            //목록에 기존재하는 Cell 입니다. 
                            dataGrid.SetCellValidation(i, "SUBLOTID", "FM_ME_0132", dataGrid.GetValue(i, "SUBLOTID").Nvc());
                            isDuplicate = true;
                            CellList.RemoveAt(inx);
                            break;
                        }
                    }
                }
                if (isDuplicate && CellList.Count == 0) return;

                foreach (string cell in CellList)
                {
                    dataGrid.AddRowData(1);
                    dataGrid.SetValue(dataGrid.Rows.Count - 1, "SUBLOTID", cell);
                }

                CellCountDisplay(dataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_HOLD_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_HOLD_DTTM", typeof(string));                
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("HOLD_FLAG", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                // PKG Lot 혹은 Cell 입력시 날짜 무시
                if (txtSearchPkglotID.GetBindValue() == null && txtSearchCellId.GetBindValue() == null)
                {
                    dr["FROM_HOLD_DTTM"] = dtpSearchDate.SelectedFromDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["TO_HOLD_DTTM"] = dtpSearchDate.SelectedToDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                dr["EQSGID"] = cboLine.GetBindValue();
                dr["MDLLOT_ID"] = cboModel.GetBindValue();
                dr["LOTTYPE"] = cboLotType.GetBindValue();
                dr["HOLD_FLAG"] = cboHoldYN.GetBindValue();
                dr["PROD_LOTID"] = txtSearchPkglotID.GetBindValue();
                dr["SUBLOTID"] = txtSearchCellId.GetBindValue();
                dtRqst.Rows.Add(dr);

                dgSearch.ExecuteService("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_CDC", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CellCountDisplay(UcBaseDataGrid dataGrid)
        {
            switch (dataGrid.Name)
            {
                case "dgInsert":
                    txtInsertCellCnt.Text = dgInsert.GetRowCount().Nvc();
                    break;
                case "dgRelease":
                    txtReleaseCellCnt.Text = dgRelease.GetRowCount().Nvc();
                    break;
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
    }
}
