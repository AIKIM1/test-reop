/*************************************************************************************
 Created Date : 2020.06.11
      Creator : 김준겸 A
   Decription : 재고실사 - 엑셀 Upload 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.11  김준겸  CSR ID : C20200602-000006 Initial Created.
  2020.06.26  김준겸  CSR ID : C20200602-000006 재고 실사(Pack) Excel Upload 템플릿  변경 및 에러 메시지 로직 수정.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Collections.Generic;
using Microsoft.Win32;
using LGC.GMES.MES.CMM001.Extensions;
using System.IO;
using C1.WPF.Excel;
using System.Configuration;
using C1.WPF.DataGrid.Excel;
using System.Linq;
using System.Net;

namespace LGC.GMES.MES.PACK001
{
    public partial class TabStckCntExcelImportEditor : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public delegate void EventFormClosed();
        public event EventFormClosed FormClosed;
        DataTable DATA_GRID;
        DataTable dtChildIf = new DataTable();

        private object lockObject = new object();
        bool GENERATE_UI = false;

        int iChkCnt;
        int iInputLotCnt;
        private int iChkLotCnt;
        int iGridRowCount;

        #endregion

        #region Initialize
        public TabStckCntExcelImportEditor(DataTable dataGrid, bool generateUI = false)
        {
            InitializeComponent();

            this.DATA_GRID = dataGrid;
            this.GENERATE_UI = generateUI;
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region Event

        void InitDataGrid()
        {
            DataTable dtSource = this.DATA_GRID.Clone();

            if (dtSource.Columns.Count == 0)
            {
                Util.MessageInfo("SFU8163");
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
                return;
            }


            foreach (DataColumn dColumn in dtSource.Columns)
            {
                Util.SetGridColumnTextName(dataGrid, null, null, dColumn.ToString(), dColumn.ToString(), false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), 40, HorizontalAlignment.Center, Visibility.Visible);
            }


        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.FormClosed != null) FormClosed();
            /*
            Util.MessageInfo("SFU1937");
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
            */
            Util.MessageInfo("SFU1937", (msgResult) =>
            {
                if (msgResult == MessageBoxResult.OK)
                {
                    this.DialogResult = MessageBoxResult.Cancel;
                    this.Close();
                }
            });
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Export(dataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //DataGridVisible();
        }

        public void SaveStream(string filekey, Stream stream, object userState, EventHandler<EventArgs> uploadCompleteHandler)
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
                //new UploadCompletedEventArgs(true, false, null, null, null, userState)
                );
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region [ 자식창 I/F 테이블 만들고 던지기 ]

        public DataTable dtIfMethod
        {
            get { return dtChildIf; }
            set { this.dtChildIf = value; }
        }

        #endregion

        #region Mehod
        #endregion

        private void btnStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                iGridRowCount = 0;
                numStckCntSeqno.Value = 1;

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }


                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (fd.ShowDialog() == true)
                {
                    iChkLotCnt = 0; // 변수 초기화 선언.

                    // 데이터 넣기위함 테이블
                    DataTable dtTemp = new DataTable();
                    dtTemp.TableName = "RQSTDT";
                    dtTemp.Columns.Add("LOTID", typeof(string));
                    dtTemp.Columns.Add("NOTE", typeof(string));

                    

                    using (Stream stream = fd.OpenFile())
                    {
                        lock (lockObject)
                        {
                            DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                            if (dtExcelData.Rows[0][0].ToString().Replace("[#]", "").Trim().Equals("Lot ID"))
                            {
                                dtExcelData.Rows[0].Delete();
                            }

                            if (dtExcelData.Rows.Count > 400)
                            {
                                Util.MessageInfo("SFU8164");
                                dataGrid.ItemsSource = null;
                                return;
                            }

                            GetGridData(false);

                            if (dtExcelData != null)
                            {
                                foreach (DataRow dr in dtExcelData.Rows)
                                {
                                    if (!(dr.RowState.ToString().Equals("Deleted")))
                                    {
                                        DataRow drTemp = dtTemp.NewRow();
                                        drTemp["LOTID"] = dr.ItemArray[0].ToString();
                                        drTemp["NOTE"] = dr.ItemArray[1].ToString() == null ? null : dr.ItemArray[1].ToString();
                                        
                                        dtTemp.Rows.Add(drTemp);
                                        iInputLotCnt++;
                                    }
                                }                                                                
                                iChkCnt = 1;                                                                
                            }
                            if (iInputLotCnt != 0)
                            {
                                iGridRowCount = Convert.ToInt32(dataGrid.Rows.Count) - 1; // 헤더값 제거 한 실제 데이터 수량.
                                dataGrid.ItemsSource = DataTableConverter.Convert(dtTemp);
                                //xTextBlock.Text = Convert.ToString(iGridRowCount) + "/400";
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

        private void btnStep2_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                iChkLotCnt = dataGrid.Rows.Count-1;
                iInputLotCnt = 0;

                if (dataGrid.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU8165");
                    return;
                }

                //조회
                DataTable dt = new DataTable();
                dt = Util.MakeDataTable(dataGrid, true);

                DataView dv = dt.DefaultView;
                //중복 확인 테이블
                DataTable dtDistinct = new DataTable();
                dtDistinct = dv.ToTable(true, new string[] { "LOTID" });

                if (dt.Rows.Count != dtDistinct.Rows.Count)
                {
                    Util.MessageInfo("SFU8166");
                    return;
                }                

                DataTable INDATA = new DataTable();
                DataTable OUTDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));                
                INDATA.Columns.Add("NOTE", typeof(string));
                //INDATA.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow drInData = null;

                foreach (DataRow dr in dt.Rows)
                {
                    drInData = INDATA.NewRow();
                    drInData["LANGID"] = LoginInfo.LANGID;
                    drInData["LOTID"] = dr.ItemArray[0].ToString(); // "LOT";                    
                    drInData["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drInData["NOTE"] = dr.ItemArray[1].ToString() == null ? null : dr.ItemArray[1].ToString();
                    //drInData["HOLD_FLAG"] = "N";

                    INDATA.Rows.Add(drInData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_PACK_STOCK_EXCEL_CHECK", "INDATA", "OUTDATA", INDATA, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }                    

                    GetGridData(true);
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (dtResult.Rows.Count != 0)
                    {
                        iChkCnt = 2;


                        dataGrid.ItemsSource = null;
                        dataGrid.ItemsSource = DataTableConverter.Convert(dtResult);

                        for (int i = 0; i < dataGrid.Rows.Count; i++)
                        {
                            if (Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["CHK"].Index).Value) == "True")
                            {
                                iInputLotCnt++;
                            }
                        }

                        Util.MessageValidation("SFU8167", iChkLotCnt, iInputLotCnt);

                        dataGrid.Refresh();
                    }
                    else if (dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU8222");
                        dataGrid.ItemsSource = null;
                    }
                    else
                    {
                        Util.MessageInfo("SFU8168");
                    }
                });
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetGridData(Boolean boolean)
        {
            if (boolean == true)
            {
                CHK.Visibility = Visibility.Visible;                
                PRODID.Visibility = Visibility.Visible;
                WIPSTAT.Visibility = Visibility.Visible;
                PRJT_NAME.Visibility = Visibility.Visible;
                STCK_CNT_SEQNO.Visibility = Visibility.Visible;
                AREAID.Visibility = Visibility.Visible;
                PROCNAME.Visibility = Visibility.Visible;
                EQSGNAME.Visibility = Visibility.Visible;
            }
            else
            {
                CHK.Visibility = Visibility.Collapsed;                
                PRODID.Visibility = Visibility.Collapsed;
                WIPSTAT.Visibility = Visibility.Collapsed;
                PRJT_NAME.Visibility = Visibility.Collapsed;
                STCK_CNT_SEQNO.Visibility = Visibility.Collapsed;
                AREAID.Visibility = Visibility.Collapsed;
                PROCNAME.Visibility = Visibility.Collapsed;
                EQSGNAME.Visibility = Visibility.Collapsed;
            }
        }

        private void btnStep3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(iChkCnt.Equals(2)))
                {
                    Util.MessageInfo("SFU8168");
                    return;
                }

                iChkCnt = 0;
                iGridRowCount = 0;

                DataTable RQSTDT = new DataTable();
                //DataSet dsInput = new DataSet();

                DataTable dtResult = null;
                try
                {
                    iGridRowCount = dataGrid.Rows.Count - 1;


                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));                    
                    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                    RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                    RQSTDT.Columns.Add("SCAN_DTTM", typeof(string));
                    RQSTDT.Columns.Add("NOTE", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    RQSTDT.Columns.Add("WIPQTY", typeof(decimal));
                    RQSTDT.Columns.Add("WIPQTY2", typeof(decimal));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                    RQSTDT.Columns.Add("BOXID", typeof(string));                    
                    RQSTDT.Columns.Add("MKT_TYPE_CODE", typeof(string));                    

                    for (int i = 0; i < iGridRowCount; i++)
                    {
                        DataRow dr = RQSTDT.NewRow();

                        if (Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["CHK"].Index).Value) == "True")
                        {
                            dr["AREAID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["AREAID"].Index).Value);
                            dr["LOTID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["LOTID"].Index).Value);
                            dr["STCK_CNT_YM"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["STCK_CNT_YM"].Index).Value);
                            dr["STCK_CNT_SEQNO"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["STCK_CNT_SEQNO"].Index).Value) == "" ? "1" : Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["STCK_CNT_SEQNO"].Index).Value);
                            dr["SCAN_DTTM"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["SCAN_DTTM"].Index).Value);
                            dr["NOTE"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["NOTE"].Index).Value) == "" ? null : Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["NOTE"].Index).Value);
                            dr["USERID"] = LoginInfo.USERID;
                            dr["WIPQTY"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["WIPQTY"].Index).Value) == "" ? "1" : Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["WIPQTY"].Index).Value);
                            dr["WIPQTY2"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["WIPQTY2"].Index).Value) == "" ? "1" : Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["WIPQTY2"].Index).Value);
                            dr["PROCID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["PROCID"].Index).Value);
                            dr["PRODID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["PRODID"].Index).Value);
                            dr["EQSGID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["EQSGID"].Index).Value);
                            dr["WIPSTAT"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["WIPSTAT"].Index).Value);
                            dr["BOXID"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["BOXID"].Index).Value);
                            dr["MKT_TYPE_CODE"] = Util.NVC(dataGrid.GetCell(i, dataGrid.Columns["MKT_TYPE_CODE"].Index).Value);
                            RQSTDT.Rows.Add(dr);
                        }
                    }
                    
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCK_RSLT_EXCEL", "INDATA", "OUTDATA", RQSTDT);

                    this.DialogResult = MessageBoxResult.OK;
                    Util.MessageInfo("SFU1270");
                    FormClosed();
                    this.Close();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }


                this.DialogResult = MessageBoxResult.OK;
                FormClosed();
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void Export(C1DataGrid dataGrid)
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

                        string columnName = this.dataGrid.Columns[c].Name;


                        if (columnName.ContainsValue("LOTID"))
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

                string tempFilekey = "TABSTCKCNT_TEMPLATE";

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
    }
}
