/*************************************************************************************
 Created Date : 2020.01.27
      Creator : 염규범 S
   Decription : 엑셀 Upload 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.27  염규범  : Initial Created.
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
    public partial class UnHoldExcelImportEditor : C1Window, IWorkArea
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
        int iChkLotCnt;

        #endregion

        #region Initialize
        public UnHoldExcelImportEditor(DataTable dataGrid, bool generateUI = false)
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

            Util.MessageInfo("SFU1937");
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                /*
                Dictionary<string, string> dicHeader = new Dictionary<string, string>();
                DataTable dtTemp = new DataTable();
                dtTemp.TableName = "dtTable";
                dtTemp.Columns.Add("LOTID", typeof(string));
                dicHeader.Add("LOTID", "LOTID");

                new ExcelExporter().DtToExcel(dtTemp, "HOLD 템플릿", dicHeader);
                */

                Export(dataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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


        public void Export(C1DataGrid dataGrid, string defaultFileName = null)
        {

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
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                iInputLotCnt = 0;

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (fd.ShowDialog() == true)
                {
                    // 데이터 넣기위함 테이블
                    DataTable dtTemp = new DataTable();
                    dtTemp.TableName = "RQSTDT";
                    dtTemp.Columns.Add("CHK", typeof(string));
                    dtTemp.Columns.Add("LOTID", typeof(string));
                    dtTemp.Columns.Add("PRODID", typeof(string));
                    dtTemp.Columns.Add("WOID", typeof(string));
                    dtTemp.Columns.Add("PRODNAME", typeof(string));
                    dtTemp.Columns.Add("EQSGID", typeof(string));
                    dtTemp.Columns.Add("EQSGNAME", typeof(string));
                    dtTemp.Columns.Add("PROCNAME", typeof(string));
                    dtTemp.Columns.Add("WIPSNAME", typeof(string));
                    dtTemp.Columns.Add("BOXID", typeof(string));

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

                            if (dtExcelData != null)
                            {
                                foreach (DataRow dr in dtExcelData.Rows)
                                {
                                    if (!(dr.RowState.ToString().Equals("Deleted")))
                                    {
                                        DataRow drTemp = dtTemp.NewRow();
                                        drTemp["LOTID"] = dr.ItemArray[0].ToString();
                                        dtTemp.Rows.Add(drTemp);
                                        iInputLotCnt++;
                                    }
                                }
                                iChkLotCnt = 0;
                                iChkCnt = 1;
                            }
                            if (iInputLotCnt != 0)
                            {
                                dataGrid.ItemsSource = DataTableConverter.Convert(dtTemp);
                                xTextBlock.Text = dtExcelData.Rows.Count.ToString() + "/1000";
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

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow drInData = null;

                foreach (DataRow dr in dt.Rows)
                {
                    drInData = INDATA.NewRow();
                    drInData["LANGID"] = LoginInfo.LANGID;
                    drInData["LOTID"] = dr.ItemArray[1].ToString(); // "LOT";
                    drInData["HOLD_FLAG"] = "Y";

                    INDATA.Rows.Add(drInData);
                }
                //DataTable dtResult = new DataTable();

                //dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELLID_VALI", "INDATA", "OUTDATA", INDATA);


                new ClientProxy().ExecuteService("DA_BAS_SEL_CELLID_VALI", "INDATA", "OUTDATA", INDATA, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    iChkLotCnt = dtResult.Rows.Count;

                    if (iChkLotCnt != 0)
                    {
                        if (dtResult.Rows[0]["AREAID"].ToString() != LoginInfo.CFG_AREA_ID.ToString())
                        {
                            Util.MessageInfo("SFU3335"); //입력오류 : LOGIN 사용자의 동정보와 LOT의 동정보가 다릅니다.
                            return;
                        }

                        iChkCnt = 2;

                        Util.gridClear(dataGrid);
                        Util.GridSetData(dataGrid, dtResult, FrameOperation, true);
                        //dataGrid.ItemsSource = null;
                        //dataGrid.ItemsSource = DataTableConverter.Convert(dtResult);

                        //Util.MessageInfo("총 : " + iInputLotCnt + " 개중, Hold 가능한 LOT의 갯수 : " + iChkLotCnt );
                        Util.MessageValidation("SFU8167", iInputLotCnt, iChkLotCnt);
                    }
                    else
                    {
                        Util.MessageInfo("SFU8168");
                    }


                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
                //string info_step = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                iInputLotCnt = 0;
                iChkLotCnt = 0;

                dtChildIf = Util.MakeDataTable(dataGrid, true);
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
                //List<int> deleteColIndex = new List<int>();

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
                //yellowCell.SetBorderStyle(XLLineStyleEnum.Thin);


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

                string tempFilekey = "UNHOLD_TEMPLATE";

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
