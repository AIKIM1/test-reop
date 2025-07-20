/*********************************************************************************************************************************************************************************
 Created Date : 2023.12.26
      Creator : INS 전상진
   Decription : 수동 Cell정보, 공Tray 생성
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.12.26  DEVELOPER : INS 전상진
**********************************************************************************************************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS001_170 : UserControl, IWorkArea
    {
        public string xAxis = string.Empty;
        public string yAxis = string.Empty;
        public string barcodeXY = string.Empty;
        public string barcodeOption = string.Empty;
        public string barTextXY = string.Empty;
        public string barTextSize = string.Empty;

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_170()
        {
            InitializeComponent();
            InitCombo();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            
            this.Loaded -= UserControl_Loaded;
        }

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
        }

        #endregion

        #region [Method]
        
        private Boolean LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid, string AddDatacol)
        {
            Boolean rslt = false;

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

                    if (!dtInfo.Columns.Contains(AddDatacol))
                    {
                        dtInfo.Columns.Add(AddDatacol, typeof(string));
                    }
                    
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        //if (AddDatacol == "SUBLOTID")
                        //{
                        if (sheet.GetCell(rowInx, 0) != null)
                        {
                            dtInfo.Rows.Add(Util.NVC(sheet.GetCell(rowInx, 0).Text));
                        }
                        //}
                        //else
                        //{
                        //    if (sheet.GetCell(rowInx, 1) != null)
                        //    {
                        //        dtInfo.Rows.Add(Util.NVC(sheet.GetCell(rowInx, 1).Text));
                        //    }
                        //}
                    }


                    if (dtInfo.Rows.Count > 0)
                    {
                        if (!dtInfo.Columns.Contains("FLAG"))
                        {
                            dtInfo.Columns.Add("FLAG", typeof(string));
                        }

                        for (int i = 0; i < dtInfo.Rows.Count; i++)
                        {
                            dtInfo.Rows[i]["FLAG"] = "N";
                        }
                    }
                    Util.GridSetData(dataGrid, dtInfo, FrameOperation, true);

                    //dataGrid.ItemsSource = dtInfo.DefaultView;

                    rslt = true;
                }
            }
            return rslt;
        }

        private void CreateEmptyTrayInfomaion()
        {
            try
            {
                Util.MessageConfirm("SFU1621", (result) => //생성 하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    // Tray ID 없음
                    if (dgInputList.Rows.Count == 0)
                    {
                        Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("TRAY_ID") });
                        return;
                    }
                    else
                    {
                        string chkTrayid = string.Empty;

                        for (int i = 0; i < dgInputList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "TRAYID")) != "")
                            {
                                chkTrayid = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "TRAYID"));
                            }
                        }
                        if (chkTrayid == "")
                        {
                            Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("TRAY_ID") });
                            return;
                        }
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("CSTID", typeof(string));
                    
                    for (int i = 0; i < dgInputList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "TRAYID")) != "")
                        {
                            DataRow dr = inData.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["USERID"] = LoginInfo.USERID;
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "TRAYID"));
                            inData.Rows.Add(dr);
                        }
                    }
                    
                    new ClientProxy().ExecuteService_Multi("BR_SET_CREATE_EMPTY_TRAY", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //생성완료하였습니다.
                                Util.MessageInfo("FM_ME_0160");

                                // 생성 완료후 Grid Data Clear & 초기화
                                this.ClearValidation();
                                dgInputList.ClearRows();
                                //DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
                                EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
                                DataTableConverter.SetValue(dgInputList.Rows[dgInputList.Rows.Count - 1].DataItem, "FLAG", "Y");
                            }
                            else
                            {
                                if (bizResult.Tables["OUTDATA"].Rows[0]["ERR_CODE"].ToString() == "NG_DUP")
                                {
                                    // 중복된 Tray 정보가 존재합니다.
                                    Util.MessageInfo("SFU3682");
                                }
                                //생성실패하였습니다. (Result Code : {0})
                                Util.MessageInfo("ME_0159");
                                return;
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
                    }, inDataSet);
                    
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateCellInformation()
        {
            try
            {
                Util.MessageConfirm("SFU1621", (result) => //생성 하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    // Lot Type 미선택
                    if (Util.GetCondition(cboLotType, bAllNull: true) == null)
                    {
                        Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("LOTTYPE") });
                        return;
                    }

                    // 제품ID 미기입
                    if (txtProdID.Text == "")
                    {
                        Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("PRODID") });
                        return;
                    }

                    // Cell ID 없음
                    if (dgInputList2.Rows.Count == 0)
                    {
                        Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("CELL_ID") });
                        return;
                    }
                    else
                    {
                        string chkCellid = string.Empty;

                        for (int i = 0; i < dgInputList2.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")) != "")
                            {
                                chkCellid = Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID"));
                            }
                        }
                        if (chkCellid == "")
                        {
                            Util.MessageInfo("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("CELL_ID") });
                            return;
                        }
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("EQPID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("PRODID", typeof(string));
                    inData.Columns.Add("LOTID", typeof(string));
                    inData.Columns.Add("LOTTYPE", typeof(string));
                    inData.Columns.Add("PROD_LOTID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("EQSGID", typeof(string));

                    DataTable inSUBLOTDATA = inDataSet.Tables.Add("IN_SUBLOT");
                    inSUBLOTDATA.Columns.Add("SUBLOTID", typeof(string));

                    string regdate = DateTime.Now.ToString("yyMMddhhmmss");
                    string lotid = "OCHV" + regdate;

                    DataRow dr = inData.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PRODID"] = txtProdID.Text;                    
                    dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                    dr["EQSGID"] = "AFHVF";
                    dr["LOTID"] = lotid;
                    dr["PROD_LOTID"] = lotid;

                    inData.Rows.Add(dr);

                    for (int i = 0; i < dgInputList2.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")) != "")
                        {
                            DataRow drSub = inSUBLOTDATA.NewRow();
                            drSub["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID"));
                            inSUBLOTDATA.Rows.Add(drSub);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_SET_CREATE_CELL_INFOMAION", "INDATA, IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //생성완료하였습니다.
                                Util.MessageInfo("FM_ME_0160");

                                // 생성 완료후 Grid Data Clear & 초기화
                                this.ClearValidation();
                                dgInputList2.ClearRows();
                                //DataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell.Text));
                                EqpDataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell2.Text));
                                DataTableConverter.SetValue(dgInputList2.Rows[dgInputList2.Rows.Count - 1].DataItem, "FLAG", "Y");
                            }
                            else
                            {
                                if (bizResult.Tables["OUTDATA"].Rows[0]["ERR_CODE"].ToString() == "NG_DUP")
                                {
                                    // 중복된 Cell 정보가 존재합니다.
                                    Util.MessageInfo("SFU4384");
                                    return;
                                }
                                else if (bizResult.Tables["OUTDATA"].Rows[0]["ERR_CODE"].ToString() == "NO_INPUT")
                                {
                                    // 입력된 항목이 없습니다.
                                    Util.MessageInfo("SFU2052");
                                    return;
                                }
                                else if (bizResult.Tables["OUTDATA"].Rows[0]["ERR_CODE"].ToString() == "NO_ROUT")
                                {
                                    // 공정경로 정보가 존재하지않습니다.
                                    Util.MessageInfo("ME_0100");
                                    return;
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
                    }, inDataSet);

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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

        public static void ExcelExport(C1DataGrid dg, string AddDatacol, List<int> collapsedcols = null, bool bVisiblecols = false)
        {

            if (collapsedcols == null && bVisiblecols == false)
            {
                collapsedcols = new List<int>() { 0 };

                if (dg.Resources.Contains("ExportRemove"))
                {
                    dg.Resources.Remove("ExportRemove");
                }

                dg.Resources.Add("ExportRemove", collapsedcols);
            }

            int iMultLangColCnt = 0;
            DataTable dtGrid = DataTableConverter.Convert(dg.ItemsSource);
            DataTable dtGridCopy = dtGrid.Copy();

            //다국어 처리.
            for (int dgCol = 0; dgCol < dg.Columns.Count; dgCol++)
            {
                if (dg.Columns[dgCol] is ControlsLibrary.DataGridMultiLangColumn)
                {
                    string sMultiColName = dg.Columns[dgCol].Name;

                    const string sDefaultLangId = "ko-KR";

                    for (int iRowCnt = 0; iRowCnt < dtGrid.Rows.Count; iRowCnt++)
                    {
                        string sMultLangValue = dtGrid.Rows[iRowCnt][sMultiColName].ToString();
                        string sDefaultLangValue = GetTextFromMultiLangText(sMultLangValue, sDefaultLangId);
                        string sLangValue = GetTextFromMultiLangText(sMultLangValue, LoginInfo.LANGID);

                        dtGrid.Rows[iRowCnt][sMultiColName] = string.IsNullOrEmpty(sLangValue) ? sDefaultLangValue : sLangValue;
                    }

                    iMultLangColCnt++;
                }
            }

            if (iMultLangColCnt > 0)
            {
                Util.gridClear(dg);
                dtGrid.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dtGrid);
                new ExcelExporter().Export(dg);

                Util.gridClear(dg);
                dtGridCopy.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dtGridCopy);
            }
            else
            {
                new ExcelExporter().Export(dg);
            }
        }

        public static void MessageError(Exception ex)
        {
            ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }

        public static string multiPattern = string.Empty;

        public static string GetTextFromMultiLangText(string multiLangText, string langID)
        {
            string result = string.Empty;
            string defaultResult = string.Empty;

            if (multiPattern == string.Empty)
            {
                int check = 0;
                multiPattern = "(";
                for (int i = 0; i < Common.LoginInfo.SUPPORTEDLANGLIST.Length; i++)
                {
                    string s = Common.LoginInfo.SUPPORTEDLANGLIST[i];
                    if (multiLangText.IndexOf(s) >= 0) check++;

                    multiPattern += s;
                    multiPattern += @"\\";
                    if (i + 1 != Common.LoginInfo.SUPPORTEDLANGLIST.Length)
                    {
                        multiPattern += @"|";
                    }
                }

                multiPattern += ")(.+)";
            }


            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(multiPattern);
            char[] chrArray = new char[] { '|' };
            string[] strArrays = multiLangText.Split(chrArray);
            bool matched = false;

            foreach (string str in strArrays)
            {
                System.Text.RegularExpressions.Match match = reg.Match(str);

                if (match.Success)
                {
                    matched = true;
                    string[] ss = str.Split(new char[] { '\\' });
                    if (langID == ss[0])
                    {
                        result = ss[1];
                    }
                    if (ss[0] == "ko-KR")
                    {
                        defaultResult = ss[1];
                    }
                }
            }

            if (matched == false)
            {
                return multiLangText;
            }

            if (result.Equals(string.Empty))
            {
                return defaultResult;
            }

            return result;

        }

        private void EqpDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                //if (dg.Rows.Count == 0)
                //    dt.Columns.Add("FLAG", typeof(string));

                if (!dt.Columns.Contains("FLAG"))
                {
                    dt.Columns.Add("FLAG", typeof(string));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        //if (dg.Rows.Count == 0)
                        //    dt.Columns.Add("FLAG", typeof(string));

                        if (!dt.Columns.Contains("FLAG"))
                        {
                            dt.Columns.Add("FLAG", typeof(string));
                        }

                        DataRow dr = dt.NewRow();
                        //dr["FLAG"] = "Y";
                        dt.Rows.Add(dr);
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
                        dr["FLAG"] = "Y";
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

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - dg.TopRows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

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

        #region [공 Tray 생성]
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

                    if (e.Column.Name.Equals("LOTID"))
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
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name.ToString().Equals("CHK"))
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")).Equals("N"))
                        {
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }));
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
        private void dgInputList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            try
            {

                if (e.Cell.Column.Name.ToString().Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")) != "N")
                    {
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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList.ClearRows();
            //DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
            EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            CreateEmptyTrayInfomaion();
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            bool rslt = LoadExcel(dgInputList, "TRAYID");

            if (!rslt)
            {
                Util.MessageValidation("SFU1497");
            }
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelExport(dgInputList, "TRAYID");
            }
            catch (Exception ex)
            {
                MessageError(ex);
            }
        }
        private void Loc_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList.Rows.Count > 0)
            {
                EqpDataGridRowAdd(dgInputList, 1);
                DataTableConverter.SetValue(dgInputList.Rows[dgInputList.Rows.Count - 1].DataItem, "FLAG", "Y");
                //dgInputList.ScrollIntoView(dgInputList.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }
        private void Loc_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList.Rows.Count > 0)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[dgInputList.Rows.Count - 1].DataItem, "FLAG"));
                //if (flag.Equals("Y")) DataGridRowRemove(dgInputList);
                DataGridRowRemove(dgInputList);
                //dgInputList.ScrollIntoView(dgInputList.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }
        #endregion

        #region [수동 Cell 생성]
        private void dgInputList2_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("CHK2"))
            {
                e.Cancel = false;
            }
        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList2.ClearRows();
            //DataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell2.Text));
            EqpDataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }
        private void btnSave2_Click(object sender, RoutedEventArgs e)
        {
            CreateCellInformation();
        }
        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            bool rslt = LoadExcel(dgInputList2, "SUBLOTID");

            if (!rslt)
            {
                Util.MessageValidation("SFU1497");
            }
        }
        private void btnExport2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelExport(dgInputList2, "SUBLOTID");
            }
            catch (Exception ex)
            {
                MessageError(ex);
            }
        }
        private void Loc_btnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList2.Rows.Count > 0)
            {
                EqpDataGridRowAdd(dgInputList2, 1);
                DataTableConverter.SetValue(dgInputList2.Rows[dgInputList2.Rows.Count - 1].DataItem, "FLAG", "Y");
            }
        }
        private void Loc_btnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList2.Rows.Count > 0)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[dgInputList2.Rows.Count - 1].DataItem, "FLAG"));
                DataGridRowRemove(dgInputList2);
            }
        }

        #region Cell ID Print

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            string[] CellList = new string[dgInputList2.Rows.Count];

            for (int i = 0; i < dgInputList2.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")) != "")
                {
                    PrintLabel(new object[] { Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")), "1" });
                    System.Threading.Thread.Sleep(100);
                }
            }

            Util.MessageInfo("FM_ME_0126");  //라벨 발행을 완료하였습니다.
        }

        private void PrintLabel(object[] PrintData)
        {
            try
            {
                string sCellID = PrintData[0] as string;
                
                String PrintCode;
                string ZplCode = string.Empty;

                ZplCode += "^XA";
                ZplCode += "^LH45,0";       // X축,Y축
                ZplCode += "^MD15";         // 바코드 진하기
                ZplCode += "^FO90,80";      // 바코드위치
                ZplCode += "^BQN,2,10";     // 2D 바코드 정보
                ZplCode += "^FD{0}";        // 2D 바코드 정보
                ZplCode += "^FS";           // 2D 바코드 정보
                ZplCode += "^FO90,20";      // 글씨위치
                ZplCode += "^ADN,56,45";    // 글씨크기
                ZplCode += "^FD{2}";        // Cell ID
                ZplCode += "^FS";
                ZplCode += "^PQ1";          // 바코드 발행 수량
                ZplCode += "^XZ";

                PrintCode = string.Format(ZplCode, "   " + sCellID  // 0 Cell ID
                                                           );

                Util.PrintLabel(FrameOperation, loadingIndicator, PrintCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
