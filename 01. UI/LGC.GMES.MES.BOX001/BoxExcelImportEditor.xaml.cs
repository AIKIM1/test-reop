/*************************************************************************************
 Created Date : 2024.02.26
      Creator : 박나연
   Decription : 포장 PALLET 구성(개별 CELL) 엑셀 Upload 화면
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.26  박나연 : Initial Created.
  2024.07.19  최석준 : 반품정보 표시 추가 (2025년 적용예정 - 수정 필요시 연락 부탁드립니다)
**************************************************************************************/
using C1.WPF;
using C1.WPF.Excel;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;


namespace LGC.GMES.MES.BOX001
{
    public partial class BoxExcelImportEditor : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private string ScanQty = "";
        private string ChkSkip = "";
        private string Chk2D = "";
        private string MdllotID = "";
        private string EqsgID = "";
        private string ProdID = "";
        private string sSHOPID = LoginInfo.CFG_SHOP_ID;
        private string sAREAID = LoginInfo.CFG_AREA_ID;
        private string EqptID = "";
        private string IrDFCTCode = "";
        private DataTable isLotTable = new DataTable();
        private string ChkLotTerm = "";
        private string Go_LotTerm = "";
        private double BoxCellQTY = 0;

        // lot 편차 구하기 위한 변수 선언.
        string sMINLOT = "";
        string sMAXLOT = "";
        string sMINDATE = "";
        string sMAXDATE = "";
        string tLOTTERM = "";

        private DataTable dtResult = new DataTable();
        private DataTable dtLotResult = new DataTable();
        private object[] paramResult = new object[4];
        #endregion

        #region Constructor...
        public BoxExcelImportEditor()
        {
            InitializeComponent();
        }

        public BoxExcelImportEditor(string scanqty, string chkskip, string chk2D, string mdllot_id, string eqsgid, string prodid, string eqptid, string IRDefectCode, string chkLot_Term, string lblPackOut_Go_LotTerm, double txtBoxCellQty)
        {
            InitializeComponent();
            this.ScanQty = scanqty;
            this.ChkSkip = chkskip;
            this.Chk2D = chk2D;
            this.MdllotID = mdllot_id;
            this.EqsgID = eqsgid;
            this.ProdID = prodid;
            this.EqptID = eqptid;
            this.IrDFCTCode = IRDefectCode;
            this.ChkLotTerm = chkLot_Term;
            this.Go_LotTerm = lblPackOut_Go_LotTerm;
            this.BoxCellQTY = txtBoxCellQty;
        }
        #endregion

        #region Properties...
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable ImportBoxData
        {
            get
            {
                return this.dtResult;
            }
            set
            {
                this.dtResult = value;
            }
        }

        public DataTable ImportBoxData2
        {
            get
            {
                return this.dtLotResult;
            }
            set
            {
                this.dtLotResult = value;
            }
        }

        public object[] ImportParam
        {
            get
            {
                return this.paramResult;
            }
            set
            {
                this.paramResult = value;
            }
        }
        #endregion

        #region Member Function Lists
        private bool GetSublotValidation(DataTable dtLoadedData)
        {
            string cellID = "";

            int[] pack_seq_cnt = new int[c1DataGrid.GetRowCount()];
            int tmp_cnt = 0;
            
            double nBoxCellQty = BoxCellQTY;   //CSR:2640443 - Box내 Cell 수량

            for (int i = 0; i < dtLoadedData.Rows.Count; i++)
            {
                cellID = dtLoadedData.Rows[i][3].ToString();

                // CELL ID 공백 or 널값 여부 확인
                if (string.IsNullOrWhiteSpace(cellID))
                {
                    // CELLID를 스캔 또는 입력하세요.
                     Util.MessageValidation("SFU1323");

                    return false;
                }

                if (cellID.Length < 10)
                {
                    // CELL ID 길이가 잘못 되었습니다.
                    Util.MessageValidation("SFU1318");
                    return false;
                }

                // 현재 스캔된 수량이 설정치보다 클 경우
                if (dtLoadedData.Rows.Count > int.Parse(ScanQty))
                {
                    //설정치를 초과하였습니다. 설정치를 변경하시거나 스캔을 중지해주세요.
                    Util.MessageValidation("SFU3152");
                  
                    return false;
                }

                try
                {
                    string msg = "";
                    string skip = "";
                    string sRtnFlag = "";

                    // CELL ID 의 조립 LOTID 가져옴과 동시에 활성화 특성치 데이터 조회 함수 호출
                    DataTable LOT_RSTDT = new DataTable();
                    LOT_RSTDT = SelectAssyLotID(cellID, skip, out msg);
                    DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "LOTID", Convert.ToString(LOT_RSTDT.Rows[0]["LOTID"]));

                    if (!Check2LotTerm(DataTableConverter.GetValue(c1DataGrid.Rows[i].DataItem, "LOTID").ToString(), cellID))
                        return false;

                    tmp_cnt = int.Parse(DataTableConverter.GetValue(c1DataGrid.Rows[i].DataItem, "PACK_SEQ").ToString());
                    if (string.IsNullOrEmpty(pack_seq_cnt[tmp_cnt].ToString()))
                    {
                        pack_seq_cnt[tmp_cnt] = 0; //수량  초기화
                    }

                    pack_seq_cnt[tmp_cnt] = pack_seq_cnt[tmp_cnt] + 1;

                    if (pack_seq_cnt[tmp_cnt] > BoxCellQTY) // 순번 변경 시 BOX 내 CELL 수량보다 많아지는 경우 알림
                    {
                        Util.MessageValidation("SFU3152");
                        return false;
                    }

                    DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "BOX_SEQ", Convert.ToString(pack_seq_cnt[tmp_cnt]));                    
                    DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "CHK", "True");

                    //CSR:2640443 - BoxID 계산하기
                    string sBoxID = tmp_cnt.ToString() + "-" + Convert.ToString(pack_seq_cnt[tmp_cnt]);
                    if (sBoxID == "") sBoxID = string.Empty;

                    DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "BOXID", sBoxID);

                    sRtnFlag = GetReturnFlagInfo(cellID);
                    DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "RTN_FLAG", sRtnFlag);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    Util.WarningPlayer();
                    return false;
                }
                finally
                {
                    System.Windows.Forms.Application.DoEvents();
                }

            }
            return true;
        }

        private void Export(C1DataGrid dataGrid)
        {
            try
            {
                MemoryStream ms = new MemoryStream();

                dataGrid.Save(ms, new ExcelSaveOptions()
                {
                    FileFormat = ExcelFileFormat.Xlsx,
                    KeepColumnWidths = true,
                    KeepRowHeights = true,
                });
                ms.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(ms);


                List<int> deleteRowIndex = new List<int>();

                foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                {
                    if (row.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        deleteRowIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
                    }
                }

                foreach (int index in (from i in deleteRowIndex orderby i descending select i))
                {
                    if (index < book.Sheets[0].Rows.Count)
                        book.Sheets[0].Rows.RemoveAt(index);

                    if (book.Sheets[0].Rows.ToString().Equals("LOTID"))
                        book.Sheets[0].Rows.RemoveAt(index);
                }

                XLFont defaultFont = book.DefaultFont;

                XLStyle defaultCell = new XLStyle(book);
                defaultCell.Font = defaultFont;
                defaultCell.AlignHorz = XLAlignHorzEnum.Center;
                defaultCell.SetBorderStyle(XLLineStyleEnum.Thin);

                XLStyle yellowCell = new XLStyle(book);

                yellowCell.BackColor = new System.Windows.Media.Color() { R = 255, G = 255, B = 0, A = 100 };
                yellowCell.Font = defaultFont;
                yellowCell.AlignHorz = XLAlignHorzEnum.Center;
                yellowCell.SetBorderStyle(XLLineStyleEnum.Medium);

                XLStyle fieldCell = new XLStyle(book);
                fieldCell.BackColor = new System.Windows.Media.Color() { R = 0, G = 176, B = 80, A = 100 };
                fieldCell.Font = defaultFont;
                fieldCell.AlignVert = XLAlignVertEnum.Center;
                fieldCell.AlignHorz = XLAlignHorzEnum.Center;
                fieldCell.SetBorderStyle(XLLineStyleEnum.Thin);

                XLStyle firstRow = new XLStyle(book);
                firstRow.BackColor = new System.Windows.Media.Color() { R = 255, G = 255, B = 0, A = 100 };
                firstRow.AlignVert = XLAlignVertEnum.Center;
                firstRow.AlignHorz = XLAlignHorzEnum.Center;
                firstRow.Font = defaultFont;
                firstRow.SetBorderStyle(XLLineStyleEnum.Thin);

                for (int r = 0; r < book.Sheets[0].Rows.Count; r++)
                {
                    for (int c = 0; c < book.Sheets[0].Columns.Count; c++)
                    {
                        if (book.Sheets[0].GetCell(r, c).Style != null) book.Sheets[0][r, c].Style = defaultCell;

                        string columnName = this.c1DataGrid.Columns[c].Name;

                        if (columnName.ContainsValue("BOX_SEQ") )
                        {
                            if (book.Sheets[0].GetCell(r, c).Style != null)
                            {
                                book.Sheets[0][r, c - 1].Style = yellowCell;
                            }
                        }
                    }
                }

                MemoryStream editedms = new MemoryStream();
                book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
                editedms.Seek(0, SeekOrigin.Begin);

                string tempFilekey = "PALLET_PACKING_TEMPLATE";

                SaveStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
                {
                    ms.Close();
                    editedms.Close();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveStream(string filekey, Stream stream, object userState, EventHandler<EventArgs> uploadCompleteHandler)
        {
            try
            {
                int blockSize = 1024 * 1024 * 3;
                int readedTotal = 0;
                int partNumber = 1;
                int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

                SaveFileDialog od = new SaveFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = filekey + ".xlsx";
                if (od.ShowDialog() == true)
                {
                    while (stream.Length > readedTotal)
                    {
                        byte[] buffer = new byte[blockSize];
                        int readed = stream.Read(buffer, 0, blockSize);
                        readedTotal += readed;

                        FileInfo tempFile = new FileInfo(od.FileName);
                        using (FileStream fs = tempFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            fs.Write(buffer, 0, readed);
                            fs.Flush();
                            fs.Close();
                        }

                        partNumber++;
                    }
                    WebClient client = new WebClient();
                    uploadCompleteHandler(client, EventArgs.Empty
                );
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageInfo("SFU1937");
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Export(c1DataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 가져오기 눌렀을 때
        private void btnStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openDialog.InitialDirectory = @"\\Client\C$";
                }

                openDialog.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (openDialog.ShowDialog() != true)
                {
                    return;
                }

                DataTable dtTemp = new DataTable();
                dtTemp.TableName = "RQSTDT";
                dtTemp.Columns.Add("CHK", typeof(string));
                dtTemp.Columns.Add("PACK_SEQ", typeof(string));
                dtTemp.Columns.Add("BOX_SEQ", typeof(string));
                dtTemp.Columns.Add("SUBLOTID", typeof(string));
                dtTemp.Columns.Add("LOTID", typeof(string));
                dtTemp.Columns.Add("BOXID", typeof(string));
                dtTemp.Columns.Add("RTN_FLAG", typeof(string));

                using (Stream stream = openDialog.OpenFile())
                {
                    DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                    // Excel 제목줄은 Import 대상 아님.
                    if (dtExcelData.Rows[0][1].ToString().ToUpper().Contains("SEQ"))
                    {
                        dtExcelData.Rows[0].Delete();
                    }

                    dtExcelData.AcceptChanges();


                    if (dtExcelData.Rows.Count > 999)
                    {
                        Util.MessageInfo("SFU8164");
                        Util.gridClear(this.c1DataGrid);
                        Util.gridClear(this.dgLotInfo);
                        return;
                    }

                    if (CommonVerify.HasTableRow(dtExcelData))
                    {
                        foreach (DataRow dr in dtExcelData.Rows)
                        {
                            DataRow drTemp = dtTemp.NewRow();
                            drTemp["PACK_SEQ"] = dr.ItemArray[0].ToString();
                            drTemp["BOX_SEQ"] = dr.ItemArray[1].ToString();
                            drTemp["SUBLOTID"] = dr.ItemArray[2].ToString();
                            drTemp["LOTID"] = dr.ItemArray[3].ToString();
                            dtTemp.Rows.Add(drTemp);
                        }

                        this.c1DataGrid.ItemsSource = DataTableConverter.Convert(dtTemp);
                        this.txtLoadedRowCount.Text = dtTemp.Rows.Count.ToString() + "/999";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 데이터 검증 눌렀을 때
        private void btnStep2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation Check Series...
                if (this.c1DataGrid == null || this.c1DataGrid.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU8165");    // Data 가져오기를 진행해 주세요.
                    return;
                }

                DataTable dtLoadedData = Util.MakeDataTable(this.c1DataGrid, true);
                if (!CommonVerify.HasTableRow(dtLoadedData))
                {
                    Util.MessageInfo("SFU8165");    // Data 가져오기를 진행해 주세요.
                    return;
                }

                // Loaded Data의 LOTID 중복 여부
                if (dtLoadedData.AsEnumerable().GroupBy(x => x.Field<string>("SUBLOTID")).Where(grp => grp.Count() > 1).Select(x => x.Key).Count() > 0)
                {
                    Util.MessageInfo("SFU8166");
                    return;
                }

                //초기화
                sMINLOT = "";
                sMAXLOT = "";
                sMINDATE = "";
                sMAXDATE = "";
                tLOTTERM = "";

                dgLotInfo.ItemsSource = null;
                isLotTable = new DataTable();

                // cell validation 진행 (BOX001_301 : Add_Scan_Cellid)
                GetSublotValidation(dtLoadedData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private DataTable SelectAssyLotID(string cellID, string skip, out string msg)
        {
            // SET_SHIPMENT_CELL_INFO_v02
            try
            {
                msg = "";
                string sBCR_Check = Chk2D;
                string sModelID = MdllotID;
                string sLineID = EqsgID;
                string sPRODID = ProdID;

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = cellID;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sLineID;
                dr["PRODID"] = sPRODID;
                dr["MDLLOT_ID"] = sModelID;
                dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                dr["INSP_SKIP_FLAG"] = ChkSkip;
                dr["2D_BCR_SKIP_FLAG"] = Chk2D;
                dr["USERID"] = LoginInfo.USERID;
                dr["EQPTID"] = EqptID;

                RQSTDT.Rows.Add(dr);

                // ClientProxy2007
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_CHK_FORM_DATA_VALIDATION_BX", "INDATA", "OUTDATA", indataSet);
                
                return dsResult.Tables["OUTDATA"];

            }
            catch (Exception ex)
            {
                // IR NG 발생 시,
                if (ex.Data["CODE"].ToString() == "100000089")
                {
                    SetSublotDefect(cellID);
                }
                throw ex;
            }
        }

        private void SetSublotDefect(string sCellID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IrDFCTCode))
                {
                    // SFU1578 불량 항목이 없습니다.
                    Util.MessageValidation("SFU1578");
                    return;
                }

                DataSet dsInDataSet = new DataSet();

                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("IFMODE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("REMARKS_CNTT", typeof(string));
                dtINDATA.Columns.Add("CALDATE", typeof(DateTime));
                dsInDataSet.Tables.Add(dtINDATA);

                DataRow drInData = dtINDATA.NewRow();
                drInData["SRCTYPE"] = "UI";
                drInData["IFMODE"] = "OFF";
                drInData["USERID"] = LoginInfo.USERID;
                drInData["LOT_DETL_TYPE_CODE"] = 'N';
                drInData["REMARKS_CNTT"] = "IR NG - Packing";
                drInData["CALDATE"] = DateTime.Now;
                dtINDATA.Rows.Add(drInData);

                DataTable dtIN_SUBLOT = new DataTable();
                dtIN_SUBLOT.TableName = "IN_SUBLOT";
                dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                dsInDataSet.Tables.Add(dtIN_SUBLOT);

                DataRow drInSublot = dtIN_SUBLOT.NewRow();
                drInSublot["SUBLOTID"] = sCellID;
                drInSublot["DFCT_GR_TYPE_CODE"] = "5";
                drInSublot["DFCT_CODE"] = IrDFCTCode;
                dtIN_SUBLOT.Rows.Add(drInSublot);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet);

                if (dsResult.Tables[0].Rows[0]["RETVAL"].ToString() != "0")
                {
                    // SFU1583 불량정보 저장 오류 발생
                    Util.MessageInfo("SFU1583");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool Check2LotTerm(string sLotID, string sCellID) //Lot 편차 구하는 함수
        {
            try
            {
                string sTmpDate = GetLotCreatDateBySubLot(sCellID);
                
                if (string.IsNullOrEmpty(sMINLOT) || string.IsNullOrEmpty(sMAXLOT))
                {
                    sMINLOT = sLotID;
                    sMAXLOT = sLotID;
                    Search2LotTerm(sMINLOT, sMAXLOT);

                    return true;
                }
                else
                {
                    int iMin = string.Compare(sMINDATE, sTmpDate);
                    int iMax = string.Compare(sTmpDate, sMAXDATE);

                    sMINLOT = (iMin > 0 ? sLotID : sMINLOT);
                    sMAXLOT = (iMax > 0 ? sLotID : sMAXLOT);

                    sMINDATE = (iMin > 0 ? sTmpDate : sMINDATE);
                    sMAXDATE = (iMax > 0 ? sTmpDate : sMAXDATE);

                    if (iMin > 0 || iMax > 0)    // min, max값이 변동 된 경우 
                    {
                        return Search2LotTerm(sMINLOT, sMAXLOT);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private string GetLotCreatDateBySubLot(string sCellid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = sCellid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CALDATE_BY_SUBLOT_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (isLotTable.Columns.Contains("LOTID") == false)
                    {
                        //데이터 컬럼 정의
                        isLotTable.Columns.Add("EQSGNAME", typeof(string));
                        isLotTable.Columns.Add("LOTID", typeof(string));
                        isLotTable.Columns.Add("QTY", typeof(string));
                    }

                    DataRow row1 = isLotTable.NewRow();
                    Boolean chk = false;
                    if (dgLotInfo.GetRowCount() > 0)
                    {
                        // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                        for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                        {
                            if (Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value) == dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8))
                            {
                                int lotqty = Util.NVC_Int(dgLotInfo.GetCell(i, dgLotInfo.Columns["QTY"].Index).Value) + 1;
                                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "QTY", lotqty.ToString());

                                //화면에 반영된 내용을 DataTable에 다시 반영함.
                                isLotTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());
                                chk = true;
                            }
                        }
                        if (chk == false)   // 없는 경우 추가
                        {
                            // 스프레드에 데이터 바인딩                        
                            isLotTable.Rows.Add(row1);
                            row1["EQSGNAME"] = dtResult.Rows[0]["EQSGNAME"].ToString();
                            row1["LOTID"] = dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8);
                            row1["QTY"] = "1";
                        }
                    }
                    else   // 처음인 경우 추가
                    {
                        // 스프레드에 데이터 바인딩                        
                        isLotTable.Rows.Add(row1);
                        row1["EQSGNAME"] = dtResult.Rows[0]["EQSGNAME"].ToString();
                        row1["LOTID"] = dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8);
                        row1["QTY"] = "1";
                    }
                    
                    Util.GridSetData(dgLotInfo, isLotTable, FrameOperation, true);
                }

                return Util.NVC(dtResult.Rows[0]["CALDATE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private bool Search2LotTerm(string sMinLot, string sMaxLot)   //Lot편차 조회 함수
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("MIN_LOTID", typeof(string));
                RQSTDT.Columns.Add("MAX_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MIN_LOTID"] = sMinLot;
                dr["MAX_LOTID"] = sMaxLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VAL_LOT_TERM_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    string sLotTerm = Util.NVC_Int(dtResult.Rows[0]["LOTTERM"]).ToString();
                    
                    if (ChkLotTerm == "Y")
                    {
                        int iLotTerm = Util.NVC_Int(Go_LotTerm);

                        if (int.Parse(sLotTerm) > iLotTerm)
                        {
                            //Lot편차 값이 설정된 값 {%1}일 보다 큽니다
                            Util.MessageInfo("SFU3247", iLotTerm.ToString());
                            return false;
                        }
                    }

                    sMINLOT = sMinLot;
                    sMAXLOT = sMaxLot;                   
                    sMINDATE = dtResult.Rows[0]["MIN_PRODDATE"].ToString();
                    sMAXDATE = dtResult.Rows[0]["MAX_PRODDATE"].ToString();
                    tLOTTERM = sLotTerm;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // Upload 눌렀을 때
        private void btnStep3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation Check Series...
                // Loaded Data Check
                if (this.c1DataGrid == null || this.c1DataGrid.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU8165");    // Data 가져오기를 진행해 주세요.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(this.c1DataGrid.ItemsSource);
                if (!CommonVerify.HasTableRow(dt))
                {
                    Util.MessageInfo("SFU8165");    // Data 가져오기를 진행해 주세요.
                    return;
                }

                // 데이터 검증 안한거 있으면 데이터 검증하라는 Interlock 띄움.
                if (dt.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("CHK"))).Count() > 0)
                {
                    Util.MessageInfo("SFU8168");    // 데이터 검증 부터 진행해주세요.
                    return;
                }

                var query_true = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper() == "TRUE");
                var query_false = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper() == "FALSE");

                if (query_false.Count() > 0)
                {
                    Util.MessageInfo("SFU8519");    // Upload가 가능한 데이터가 존재하지 않습니다.
                    return;
                }

                this.dtResult = query_true.CopyToDataTable();
                this.dtLotResult = isLotTable;
                this.paramResult[0] = query_true.Count();
                this.paramResult[1] = sMINLOT;
                this.paramResult[2] = sMAXLOT;
                this.paramResult[3] = tLOTTERM;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetReturnFlagInfo(string sCellID)
        {
            try
            {
                string sRtnFlag = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SUBLOTID"] = sCellID;

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RTN_FLAG_BY_SUBLOTID", "RQSTDT", "RSLTDT", inTable);

                if (!dtRslt.IsNullOrEmpty() && dtRslt.Rows.Count > 0)
                {
                    sRtnFlag = Util.NVC(dtRslt.Rows[0]["RTN_FLAG"]);
                }

                return sRtnFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        #endregion
    }
}
