/*************************************************************************************
 Created Date : 2021.06.04
      Creator : 박수미
   Decription : 폐기 셀 등록
--------------------------------------------------------------------------------------
 [Change History]
  2021.06.04  DEVELOPER : 박수미
  2022.09.14  최도훈 : 폐기대기처리 기능 추가

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS002_113 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private int cnt_sum;
        private int cnt_error;
        private int iRegCount; //2025.01.20 폐기처리 동시 처리 Count 전역변수

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();

        public FCS002_113()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            InitCombo();
            iRegCount = 100; // Default 100개

            //2025.01.17 - 폐기처리 동시 처리 수량 동별 공통코드로 수량 조절 가능하도록 변경
            DataTable dtResult = _Util.GetAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_113_SCARP_REG_COUNT_OPTION"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                int iRet = iRegCount; // 초기값
                bool bRet = Int32.TryParse(Convert.ToString(dtResult.Rows[0]["ATTR2"]), out iRet);

                if (bRet)
                {
                    iRegCount = iRet;
                }
            }

            this.Loaded -= UserControl_Loaded;
        }

        #region Initialize
        private void InitCombo()
        {
            string[] sFilterSearchShift = { "STM", null, null, null, null, null };
            string[] sFilterOp = { "CLO", "A,B,C,D" };
            string[] sFilterEqpInput = { "EQP_INPUT_YN" };

            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            _combo.SetCombo(cboSearchLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");
            _combo.SetCombo(cboSearchModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL");
        }

        #endregion

        #region [Method]

        #region [등록]
        private void GetCellData(string sCellID = "")
        {
            try
            {
                cnt_sum = 0;
                cnt_error = 0;

                txtInsertCellCnt.Text = string.Empty;
                txtBadInsertRow.Text = string.Empty;

                if (string.IsNullOrEmpty(sCellID))
                {
                    //SUBLOTID - XADDA14223,XADDA14224,XADDA14225 ...
                    for (int iRow = 0; iRow < dgInputList.Rows.Count; iRow++)
                    {
                       // string sublot = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "SUBLOTID"));
                        string sublot = Util.Convert_CellID(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "SUBLOTID")));

                        if (string.IsNullOrEmpty(sublot))
                            continue;
                        sCellID += sublot;
                        sCellID += ",";
                    }
                }
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_FOR_SCRAP", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                dtRslt.Columns.Add("CHK", typeof(bool));
                cnt_sum = dtRslt.Rows.Count;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (!Util.NVC(dtRslt.Rows[i]["AVAIL_YN"]).Equals("Y"))
                        cnt_error++;
                }

                //TRAY LOT ID 별로 정렬
                DataView dv = new DataView(dtRslt);
                dv.Sort = "LOTID";
                DataTable dtSort = dv.ToTable();

                Util.GridSetData(dgInputList, dtSort, this.FrameOperation);

                txtInsertCellCnt.Text = cnt_sum.ToString();
                txtBadInsertRow.Text = cnt_error.ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private string LoadExcel()
        {

            DataTable dtInfo = DataTableConverter.Convert(dgInputList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            string sColData = string.Empty;

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return sColData;
                        sColData += Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        sColData += ",";
                    }
                }
            }

            return sColData;
        }

        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("CHK2", typeof(string));
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        //string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        string CELL_ID = Util.Convert_CellID(Util.NVC(sheet.GetCell(rowInx, 0).Text));

                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dataGrid, dataTable, FrameOperation);
                }
            }
        }

        private void SetCellScrap()
        {
            try
            {
                int iProcessingCnt = iRegCount; //2025.01.20 폐기처리 동시 처리 수량 동별 공통코드로 수량 조절 가능하도록 변경
                double dNumberOfProcessingCnt = 0.0;
                bool bIsOK = true;

                Util.MessageConfirm("SFU4191", (result) => //폐기하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    if (!dgInputList.IsCheckedRow("CHK"))
                    {
                        Util.AlertInfo("FM_ME_0419"); // 폐기 대상이 없습니다.
                        return;
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("MENUID", typeof(string));
                    inData.Columns.Add("USER_IP", typeof(string));
                    inData.Columns.Add("PC_NAME", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));

                    DataRow dr = inData.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                    dr["USER_IP"] = LoginInfo.USER_IP;
                    dr["PC_NAME"] = LoginInfo.PC_NAME;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inData.Rows.Add(dr);

                    DataTable inSublot = inDataSet.Tables.Add("INSUBLOT");
                    inSublot.Columns.Add("SUBLOTID", typeof(string));

                    dNumberOfProcessingCnt = Math.Ceiling(dgInputList.Rows.Count / (double)iProcessingCnt);//처리수량

                    for(int i = 0; i < dNumberOfProcessingCnt; i++)
                    {
                        inSublot.Clear();

                        for (int k = (i * Convert.ToInt32(iProcessingCnt)); k < (i * iProcessingCnt + iProcessingCnt); k++)
                        {
                            if (k >= dgInputList.Rows.Count) break;

                            if (Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[k].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr2 = inSublot.NewRow();
                                dr2["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[k].DataItem, "SUBLOTID"));
                                inSublot.Rows.Add(dr2);
                            }
                        }

                        try
                        {
                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SCRAP_BY_SUBLOT_LINE", "INDATA,INSUBLOT", "OUTDATA", inDataSet);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            Util.MessageValidation("SFU9200", (i * (int)iProcessingCnt).ToString("#,##0"), (i * iProcessingCnt + iProcessingCnt - 1).ToString("#,##0"));
                            bIsOK = false;
                            return;
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }

                    if (bIsOK)
                    {
                        Util.MessageValidation("FM_ME_0425");   // Cell 실물 폐기 완료되었습니다.
                        GetCellData();
                    }
                    else
                    {
                        Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                    }



                    //foreach (DataRow inRow in dgInputList.GetCheckedDataRow("CHK"))
                    //{
                    //    DataRow dr2 = inSublot.NewRow();
                    //    dr2["SUBLOTID"] = Util.Convert_CellID(Util.NVC(inRow["SUBLOTID"]));
                    //   // dr2["SUBLOTID"] = Util.NVC(inRow["SUBLOTID"]);
                    //    inSublot.Rows.Add(dr2);
                    //}

                    //DataSet dsrslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SCRAP_BY_SUBLOT_LINE", "INDATA,INSUBLOT", "OUTDATA", inDataSet);
                    //Util.MessageInfo("FM_ME_0425"); // Cell 실물 폐기 완료되었습니다.
                    //GetCellData();
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [조회]

        #endregion

        #endregion

        #region [Event]
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

        #region [등록 tab]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCellData();
        }
        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")).Equals("N"))
                    {
                        e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                    }
                }
            }));
        }               

        private void dgInputList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            dgInputList2.ClearRows();
            //txtInsertCellCnt2.Text = "";
            //txtBadInsertRow2.Text = "";
            DataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }

        private void btnScrapStandbyProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgInputList2.IsCheckedRow("CHK2"))
                {
                    Util.AlertInfo("FM_ME_0419"); // 폐기 대상이 없습니다.
                    return;
                }

                FCS002_113_SCRAP_STANDBY_PROC FCS002_113_ScrapStandbyProc = new FCS002_113_SCRAP_STANDBY_PROC();
                FCS002_113_ScrapStandbyProc.FrameOperation = FrameOperation;

                if (FCS002_113_ScrapStandbyProc != null)
                {
                    string sCellList = string.Empty;
                    string sCheckList = string.Empty;

                    for (int i = 0; i < dgInputList2.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "CHK2")).ToUpper().Equals("TRUE"))
                        {
                            sCellList += Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")) + "|";
                            sCheckList += Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "CHK2")) + "|";
                        }
                    }

                    if (string.IsNullOrEmpty(sCellList))
                    {
                        Util.MessageInfo("SFU8243");  //CELL을 선택해야합니다.
                        return;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = sCellList;
                    Parameters[1] = sCheckList;

                    C1WindowExtension.SetParameters(FCS002_113_ScrapStandbyProc, Parameters);

                    FCS002_113_ScrapStandbyProc.Closed += new EventHandler(ScrapStandbyProc_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => FCS002_113_ScrapStandbyProc.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ScrapStandbyProc_Closed(object sender, EventArgs e)
        {
            FCS002_113_SCRAP_STANDBY_PROC window = sender as FCS002_113_SCRAP_STANDBY_PROC;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnRefresh2_Click(null, null);
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetCellScrap();
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList.Rows.Count; i++)
            {
                string availYN = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "AVAIL_YN"));
                if (availYN.Equals("Y"))
                {
                    DataTableConverter.SetValue(dgInputList.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList.Rows[i].DataItem, "CHK", false);
            }
        }


        private void chkHeaderAll2_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList2.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList2.Rows[i].DataItem, "CHK2", true);
            }
        }

        private void chkHeaderAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList2.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList2.Rows[i].DataItem, "CHK2", false);
            }
        }


        private void dgInputList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //폐기 대기 Lot 이 아닌 Cell 선택할 수 없음
            if (e.Column.Name.Equals("CHK"))
            {
                if (!Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "AVAIL_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgInputList2_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("CHK2"))
            {
                e.Cancel = false;
            }
        }

        private void dgInputList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        #endregion

        #region [조회 tab]
        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                dr["EQSGID"] = Util.GetCondition(cboSearchLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboSearchModel, bAllNull: true);
                if (!string.IsNullOrEmpty(Util.NVC(txtSearchCellId.Text)))
                {
                    dr["SUBLOTID"] = Util.Convert_CellID(Util.NVC(txtSearchCellId.Text));
                    //dr["SUBLOTID"] = Util.NVC(txtSearchCellId.Text);
                }


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SCRAP_CELL", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                Util.GridSetData(dgSearch, dtRslt, this.FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            string sCellID = LoadExcel();
            GetCellData(sCellID);
        }

        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgInputList2);
        }
        #endregion

        #endregion

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }



        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

    }
}
