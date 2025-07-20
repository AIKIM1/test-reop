/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리 - HOLD 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_245_HOLD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_245_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _holdTrgtCode = string.Empty;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_245_HOLD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            //// 시장유형 MKT_TYPE_CODE
            //DataTable dtMrk = dtTypeCombo("MKT_TYPE_CODE", ComboStatus.ALL);
            //(dgHold.Columns["MKT_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMrk);

            //// 보류범위 HOLD_TRGT_CODE  
            //DataTable dtHold = dtTypeCombo("HOLD_TRGT_CODE",ComboStatus.NONE);
            //(dgHold.Columns["HOLD_TRGT_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtHold);

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _holdTrgtCode = tmps[0].ToString();

            DataTable dt = new DataTable();
            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                dt.Columns.Add(dgHold.Columns[i].Name);
            }

            //DataRow dr = dt.NewRow();
            //dt.Rows.Add(dr);
            Util.GridSetData(dgHold, dt, FrameOperation);

            if (_holdTrgtCode == "LOT")
            {
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
            }
        }

        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region Validation
        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Column.Name == "HOLD_REG_QTY")
                {
                    string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index).Value);
                    int iHold_req_qty;

                    if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                    {
                        //SFU3435	숫자만 입력해주세요
                        Util.MessageInfo("SFU3435");
                    }
                }
                else if (e.Cell.Column.Name.Equals("ASSY_LOTID") || e.Cell.Column.Name.Equals("STRT_SUBLOTID"))
                {
                    DataTable inDataTable = new DataTable("RQSTDT");

                    inDataTable.Columns.Add("PRODID", typeof(string));
                    inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                    inDataTable.Columns.Add("LOTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["PRODID"] = dataGrid.Columns.Contains("PRODID") && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRODID")).Equals("") ? "" : Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRODID"));
                    dr["HOLD_TRGT_CODE"] = _holdTrgtCode.Equals("LOT") ? "LOT" : "CELL";
                    dr["LOTID"] = dataGrid.Columns.Contains(e.Cell.Column.Name) && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name)).Equals("") ? "EMPTY" : Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name));
                    inDataTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VW_PRODUCT_BY_PRODID", "RQSTDT", "RSLTDT", inDataTable);

                    if (dtResult?.Rows.Count < 1)
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRODID", "");
                    }
                }

                //if (e.Cell.Column.Name == "HOLD_TRGT_CODE"
                //      && Util.NVC(e.Cell.Value) == "LOT")
                //{
                //    DataTableConverter.SetValue(e.Cell.Row.DataItem, "STRT_SUBLOTID", string.Empty);
                //    DataTableConverter.SetValue(e.Cell.Row.DataItem, "END_SUBLOTID", string.Empty);

                //    dataGrid.Refresh();
                //}

                //else if (e.Cell.Column.Name == "STRT_SUBLOTID" || e.Cell.Column.Name == "END_SUBLOTID")
                //{
                //    //string sLotType = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);
                //    string sStart = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["STRT_SUBLOTID"].Index).Value);
                //    string sEnd = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["END_SUBLOTID"].Index).Value);

                //    int iStart;
                //    int iEnd;

                //    int cnt = 10;
                //    if (Util.NVC(e.Cell.Value).Length < cnt)
                //    {
                //        //SFU4342	[%1] 자리수 이상 입력하세요.
                //        Util.MessageInfo("SFU4342", new object[] { cnt });
                //    }

                //    //else if (!string.IsNullOrWhiteSpace(sStart) && !int.TryParse(sStart, out iStart))
                //    //{
                //    //    //SFU3435	숫자만 입력해주세요
                //    //    Util.MessageInfo("SFU3435");
                //    //}

                //    //else if(!string.IsNullOrWhiteSpace(sEnd) && !int.TryParse(sEnd, out iEnd))
                //    //{
                //    //    Util.MessageInfo("SFU3435");
                //    //}

                //    //else if (sStart.Length != 0 && sEnd.Length != 0 && sStart.Length != sEnd.Length)
                //    //{
                //    //    //SFU4341	시작값과 종료값의 자리수가 동일해야 합니다.	
                //    //    Util.MessageInfo("SFU4341");
                //    //}


                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Hold 리스트 추가/제거
        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Hold_Lot_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    if (_holdTrgtCode == "LOT")
                    {
                        sheet[0, 0].Value = "LOTID";
                        sheet[1, 0].Value = "ABCRB01L";
                        sheet[2, 0].Value = "ABCRD237";

                        //sheet[0, 1].Value = "QTY";
                        //sheet[1, 1].Value = "1234";
                        //sheet[2, 1].Value = "12000";

                        sheet[0, 1].Value = "PRODID";
                        sheet[1, 1].Value = "ACHP1006I-A1";
                        sheet[2, 1].Value = "ACHP1006I-A1";

                        sheet[0, 0].Style = sheet[0, 1].Style = sheet[0, 2].Style = styel;
                        sheet.Columns[0].Width = sheet.Columns[1].Width = sheet.Columns[2].Width = 1500;
                    }
                    else
                    {
                        sheet[0, 0].Value = "CELLID";
                        sheet[1, 0].Value = "PB23K1B264";
                        sheet[2, 0].Value = "G86AI051019494";

                        sheet[0, 1].Value = "PRODID";
                        sheet[1, 1].Value = "ACEN1063I-A1";
                        sheet[2, 1].Value = "ACEN1063I-A1";

                        sheet[0, 0].Style = sheet[0, 1].Style = styel;
                        sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;
                    }

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

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

                        if (_holdTrgtCode == "LOT")
                        {
                            if (sheet.GetCell(0, 0).Text != "LOTID"
                                //|| sheet.GetCell(0, 1).Text != "QTY"
                                || sheet.GetCell(0, 1).Text != "PRODID")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // LOTID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                DataRow dr = dtInfo.NewRow();
                                dr["CHK"] = true;
                                dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                dr["HOLD_REG_QTY"] = "0";// sheet.GetCell(rowInx, 1).Text;
                                dr["PRODID"] = sheet.GetCell(rowInx, 1).Text;
                                dtInfo.Rows.Add(dr);
                            }
                        }
                        else
                        {
                            if (sheet.GetCell(0, 0).Text != "CELLID"
                                || sheet.GetCell(0, 1).Text != "PRODID")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // LOTID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                DataRow dr = dtInfo.NewRow();
                                dr["CHK"] = true;
                                //dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text;
                                dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                dr["HOLD_REG_QTY"] = "1";
                                dr["PRODID"] = sheet.GetCell(rowInx, 1).Text;
                                dtInfo.Rows.Add(dr);
                            }
                        }

                        if (dtInfo.Rows.Count > 0)
                            dtInfo = dtInfo.DefaultView.ToTable(true);

                        Util.GridSetData(dgHold, dtInfo, FrameOperation);
                    }

                    if (dgHold?.Rows?.Count > 0)
                    {
                        string sColName = string.Empty;
                        if (_holdTrgtCode == "LOT")
                            sColName = "ASSY_LOTID";
                        else
                            sColName = "STRT_SUBLOTID";

                        for (int i = 0; i < dgHold.GetRowCount(); i++)
                        {
                            DataTable inDataTable = new DataTable("RQSTDT");

                            inDataTable.Columns.Add("PRODID", typeof(string));
                            inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                            inDataTable.Columns.Add("LOTID", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["PRODID"] = dgHold.Columns.Contains("PRODID") && Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "PRODID")).Equals("") ? "" : Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "PRODID"));
                            dr["HOLD_TRGT_CODE"] = _holdTrgtCode.Equals("LOT") ? "LOT" : "CELL";
                            dr["LOTID"] = dgHold.Columns.Contains(sColName) && Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, sColName)).Equals("") ? "EMPTY" : Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, sColName));
                            inDataTable.Rows.Add(dr);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VW_PRODUCT_BY_PRODID", "RQSTDT", "RSLTDT", inDataTable);

                            if (dtResult?.Rows.Count < 1)
                            {
                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "PRODID", "");
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

        /// <summary>
        /// HOLD 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);

                Util.GridSetData(dgHold, dt, FrameOperation);
                dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// HOLD 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgHold, dt, FrameOperation);
                chkAll.IsChecked = false;
            }
        }
        #endregion

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 등록
        /// BIZ : BR_PRD_REG_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dgHold.EndEdit();

            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }

            if (_holdTrgtCode == "LOT" && dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("ASSY_LOTID"))).ToList().Count > 0)
            {
                //SFU4351		미입력된 항목이 존재합니다.	
                Util.MessageValidation("SFU4351");
                return;
            }

            //if (_holdTrgtCode == "LOT" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("HOLD_REG_QTY"))
            //                            )).ToList().Count > 0)
            //{
            //    //SFU1209		수량을 입력하세요.	
            //    Util.MessageValidation("SFU1154");
            //    return;
            //}

            if (_holdTrgtCode == "SUBLOT" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("STRT_SUBLOTID"))
                                        )).ToList().Count > 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }

            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                // SFU1740  오늘 이후 날짜만 지정 가능합니다.
                Util.MessageValidation("SFU1740");
            }

            if (string.IsNullOrEmpty(txtUser.Text))
            {
                //SFU4350 해제 예정 담당자를 선택하세요.
                Util.MessageValidation("SFU4350");
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4300 Hold 사유를 입력하세요.
                Util.MessageValidation("SFU4300");
                return;
            }

            if (dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("PRODID"))
                                        )).ToList().Count > 0)
            {
                //SFU2949 제품ID를 입력하세요.
                Util.MessageValidation("SFU2949");
                return;
            }

            //SFU1345	HOLD 하시겠습니까?
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Biz

        /// <summary>
        /// Hold 등록 
        /// </summary>
        private void Save()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_SCHD_DATE");
                inDataTable.Columns.Add("UNHOLD_CHARGE_USERID");
                inDataTable.Columns.Add("HOLD_NOTE");
                inDataTable.Columns.Add("HOLD_TRGT_CODE");
                inDataTable.Columns.Add("GUBUN");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID");
                inHoldTable.Columns.Add("HOLD_TRGT_CODE");
                //inHoldTable.Columns.Add("MKT_TYPE_CODE");
                inHoldTable.Columns.Add("STRT_SUBLOTID");
                inHoldTable.Columns.Add("END_SUBLOTID");
                inHoldTable.Columns.Add("HOLD_REG_QTY");
                inHoldTable.Columns.Add("PRODID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["UNHOLD_CHARGE_USERID"] = txtUser.Tag;
                newRow["HOLD_NOTE"] = txtNote.Text;
                newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;
                newRow["GUBUN"] = "AUTO";
                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    newRow = inHoldTable.NewRow();
                    newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];
                    //newRow["MKT_TYPE_CODE"] = dtInfo.Rows[row]["MKT_TYPE_CODE"];
                    newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                    newRow["END_SUBLOTID"] = dtInfo.Rows[row]["END_SUBLOTID"];
                    newRow["HOLD_REG_QTY"] = Util.NVC(dtInfo.Rows[row]["HOLD_REG_QTY"]).Equals("") ? "0" : dtInfo.Rows[row]["HOLD_REG_QTY"];
                    newRow["PRODID"] = dtInfo.Rows[row]["PRODID"];
                    inHoldTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_HOLD_MOBILE", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 타입으로 CommonCode 조회
        /// Biz : DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE
        /// </summary>
        /// <param name="sFilter"></param>
        /// <returns></returns>
        private DataTable dtTypeCombo(string sFilter, ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                return dtResult;
            }
            catch (Exception ex)
            {
                return null;
                Util.MessageException(ex);
            }
        }

        private string GetProdID(string sLot)
        {
            try
            {
                string sRet = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID");

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                }

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        #region Method
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (e.Column.Name == "STRT_SUBLOTID" || e.Column.Name == "END_SUBLOTID")
            //{
            //    string sLotType = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);               

            //    if (string.IsNullOrWhiteSpace(sLotType))
            //    {
            //        //	SFU4349		보류범위 먼저 선택하세요.	
            //        Util.MessageValidation("SFU4349");
            //        e.Cancel = true;
            //    }
            //    else if (sLotType == "LOT")
            //        e.Cancel = true;               
            //}
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dtpSchdDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //SFU1740  오늘이후날짜만지정가능합니다.
                Util.MessageValidation("SFU1740");
                dtpSchdDate.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                GetUserWindow();

                e.Handled = true;
            }
        }

        private void dgHold_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                if (e == null || e.Column == null) return;

                if (e.Column.Name.Equals("PRODID"))
                {
                    var combo = e.EditingElement as C1ComboBox;

                    string sColName = string.Empty;
                    if (_holdTrgtCode == "LOT")
                        sColName = "ASSY_LOTID";
                    else
                        sColName = "STRT_SUBLOTID";

                    if (dgHold?.Rows?.Count > 0)
                    {
                        string sLot = dgHold.Columns.Contains(sColName) ? Util.NVC(DataTableConverter.GetValue(dgHold.Rows[e.Row.Index].DataItem, sColName)) : "";

                        DataTable inDataTable = new DataTable("RQSTDT");

                        inDataTable.Columns.Add("PRODID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["PRODID"] = "";
                        dr["LOTID"] = sLot.Equals("") ? "EMPTY" : sLot;
                        dr["HOLD_TRGT_CODE"] = _holdTrgtCode.Equals("LOT") ? "LOT" : "CELL";

                        inDataTable.Rows.Add(dr);


                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VW_PRODUCT_BY_PRODID", "RQSTDT", "RSLTDT", inDataTable);

                        // restrict combo collection
                        combo.ItemsSource = DataTableConverter.Convert(dtResult.Copy());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
