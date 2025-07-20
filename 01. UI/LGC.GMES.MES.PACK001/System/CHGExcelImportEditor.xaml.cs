/*************************************************************************************
 Created Date : 2020.06.09
      Creator : 김길용 A
   Decription : 엑셀 Upload 화면 (변경이력조회)
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.09  김길용  : Initial Created.
  2024.03.13  김민석 변경 데이터 입력 시 Excel 업로드 검증 시 데이터 중복 조회 건 수정 [요청번호] E20240312-000566
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
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;
using System.Windows.Media;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class CHGExcelImportEditor : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public delegate void EventFormClosed();
        public event EventFormClosed FormClosed;
        DataTable DATA_GRID;
        DataTable dtChildIf = new DataTable();
        private DataTable isCreateTable = new DataTable();

        private object lockObject = new object();
        bool GENERATE_UI = false;

        int checkStep = 0;
        int LoadedRows;
        int iChkLotCnt;

        #endregion

        #region Initialize
        public CHGExcelImportEditor(DataTable dataGrid, bool generateUI = false)
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
                Util.SetGridColumnTextName(c1DataGrid, null, null, dColumn.ToString(), dColumn.ToString(), false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), 40, HorizontalAlignment.Center, Visibility.Visible);
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

                Export(c1DataGrid);
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

                LoadedRows = 0;

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                if (fd.ShowDialog() == true)
                {
                    // 데이터 넣기위함 테이블
                    DataTable dtTemp = new DataTable();
                    dtTemp.TableName = "RQSTDT";
                    dtTemp.Columns.Add("CHK", typeof(string));
                    dtTemp.Columns.Add("LOTID", typeof(string));
                    dtTemp.Columns.Add("CHG_TYPE", typeof(string));
                    dtTemp.Columns.Add("NOTE", typeof(string));
                    dtTemp.Columns.Add("PRE_VALUE", typeof(string));
                    dtTemp.Columns.Add("AFTER_VALUE", typeof(string));
                    dtTemp.Columns.Add("INSDTTM", typeof(string));
                    dtTemp.Columns.Add("INSUSER", typeof(string));
                    dtTemp.Columns.Add("UPDDTTM", typeof(string));
                    dtTemp.Columns.Add("UPDUSER", typeof(string));

                    using (Stream stream = fd.OpenFile())
                    {
                        lock (lockObject)
                        {
                            DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                            if (dtExcelData.Rows[0][0].ToString().Replace("[#]", "").Trim().Equals("Lot ID"))
                            {
                                dtExcelData.Rows[0].Delete();
                            }

                            if (dtExcelData.Rows.Count > 1001)
                            {
                                Util.MessageInfo("SFU8164");
                                c1DataGrid.ItemsSource = null;
                                return;
                            }

                            if (dtExcelData != null)
                            {
                                foreach (DataRow dr in dtExcelData.Rows)
                                {
                                    if (!dr.RowState.ToString().Equals("Deleted"))
                                    {
                                        DataRow drTemp = dtTemp.NewRow();
                                        drTemp["LOTID"] = dr.ItemArray[0].ToString();
                                        drTemp["CHG_TYPE"] = dr.ItemArray[1].ToString();
                                        drTemp["NOTE"] = dr.ItemArray[2].ToString();
                                        drTemp["PRE_VALUE"] = dr.ItemArray[3].ToString();
                                        drTemp["AFTER_VALUE"] = dr.ItemArray[4].ToString();
                                        drTemp["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        drTemp["INSUSER"] = LoginInfo.USERID;
                                        drTemp["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        drTemp["UPDUSER"] = LoginInfo.USERID;

                                        dtTemp.Rows.Add(drTemp);
                                        LoadedRows++;
                                    }
                                }
                                iChkLotCnt = 0;
                                this.checkStep = 1;
                            }
                            if (LoadedRows != 0)
                            {
                                c1DataGrid.ItemsSource = DataTableConverter.Convert(dtTemp);
                                xTextBlock.Text = LoadedRows.ToString() + " / 1000";
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
            this.ValidationCheckUploadData();
        }

        private void ValidationCheckUploadData()
        {
            // 2022-04-11 Validation Check가 왜 들어 있는지 모르겠음.
            // 단지 LOTID만 존재하는지만 체크하고 있음.
            try
            {
                if (this.c1DataGrid.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU8165");
                    return;
                }
                DataTable dt = DataTableConverter.Convert(this.c1DataGrid.ItemsSource);
                var lstLOT = dt.AsEnumerable().Select(x => x.Field<string>("LOTID")).ToList();
                var lstChangeType = dt.AsEnumerable().Select(x => x.Field<string>("CHG_TYPE")).ToList();

                string bizRuleName = "DA_PRD_SEL_TB_SFC_AFTER_SHIP_BY_VALI";

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                // RQSTDT
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("CHG_TYPE", typeof(string));
                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = string.Join(",", lstLOT);
                drRQSTDT["CHG_TYPE"] = string.Join(",", lstChangeType);
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    Util.MessageInfo("SFU8168");
                    return;
                }

                // 입력데이터와 검증데이터 Join후에 Grid Binding
                
                var query = from d1 in dtRSLTDT.AsEnumerable()
                            join d2 in dt.AsEnumerable() on new { LOTID = d1.Field<string>("LOTID"), CHG_TYPE = d1.Field<string>("CHG_TYPE") } equals new { LOTID = d2.Field<string>("LOTID"), CHG_TYPE = d2.Field<string>("CHG_TYPE") } into dtJoin
                            from joined in dtJoin
                            select new
                            {
                                CHK = d1.Field<int>("CHK"),
                                LOTID = d1.Field<string>("LOTID"),
                                CHG_TYPE = d1.Field<string>("CHG_TYPE"),
                                NOTE = joined.Field<string>("NOTE"),
                                PRE_VALUE = joined.Field<string>("PRE_VALUE"),
                                AFTER_VALUE = joined.Field<string>("AFTER_VALUE"),
                                INSDTTM = joined.Field<string>("INSDTTM"),
                                INSUSER = joined.Field<string>("INSUSER"),
                                UPDDTTM = joined.Field<string>("UPDDTTM"),
                                UPDUSER = joined.Field<string>("UPDUSER")
                            };
                


                /*
                var query = from d1 in dtRSLTDT.AsEnumerable()
                            join d2 in dt.AsEnumerable() on new { LOTID = d1.Field<string>("LOTID"), CHG_TYPE = d1.Field<string>("CHG_TYPE") } equals new { LOTID = d2.Field<string>("LOTID"), CHG_TYPE = d2.Field<string>("CHG_TYPE") } into dtJoin
                            from joined in dtJoin
                            group new { d1, joined } by new { CHK = d1.Field<int>("CHK"), LOTID = d1.Field<string>("LOTID"), CHG_TYPE = d1.Field<string>("CHG_TYPE"), NOTE = joined.Field<string>("NOTE"), PRE_VALUE = joined.Field<string>("PRE_VALUE") } into grouped
                            select new
                            {
                                CHK = grouped.Key.CHK,
                                LOTID = grouped.Key.LOTID,
                                CHG_TYPE = grouped.Key.CHG_TYPE,
                                NOTE = grouped.Select(x => x.joined.Field<string>("NOTE")).FirstOrDefault(),
                                PRE_VALUE = grouped.Select(x => x.joined.Field<string>("PRE_VALUE")).FirstOrDefault(),
                                AFTER_VALUE = grouped.Select(x => x.joined.Field<string>("AFTER_VALUE")).FirstOrDefault(),
                                INSDTTM = grouped.Select(x => x.joined.Field<string>("INSDTTM")).FirstOrDefault(),
                                INSUSER = grouped.Select(x => x.joined.Field<string>("INSUSER")).FirstOrDefault(),
                                UPDDTTM = grouped.Select(x => x.joined.Field<string>("UPDDTTM")).FirstOrDefault(),
                                UPDUSER = grouped.Select(x => x.joined.Field<string>("UPDUSER")).FirstOrDefault()
                            };
                */

                DataTable dtResult = PackCommon.queryToDataTable(query.ToList());

                Util.GridSetData(this.c1DataGrid, dtResult, FrameOperation);

                Util.MessageValidation("SFU8167", dt.Rows.Count, dtResult.Rows.Count);

                //Util.MessageValidation("SFU2056", dtRSLTDT.Rows.Count);
                this.checkStep = 2;
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
                if (!this.checkStep.Equals(2))
                {
                    Util.MessageInfo("SFU8168");
                    return;
                }

                Save();
                this.checkStep = 0;
                this.LoadedRows = 0;
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

                        string columnName = this.c1DataGrid.Columns[c].Name;


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

                string tempFilekey = "CHGCODE_TEMPLATE";

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
        private void Save()
        {

            try
            {
                string bizRuleName = "BR_PRD_REG_TB_SFC_LOT_CHG_HIST";

                isCreateTable = DataTableConverter.Convert(c1DataGrid.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(c1DataGrid)) return;

                this.c1DataGrid.EndEdit();
                this.c1DataGrid.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("INPUT_SEQNO", typeof(decimal));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("CHG_TYPE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("PRE_VALUE", typeof(string));
                inDataTable.Columns.Add("AFTER_VALUE", typeof(string));
                inDataTable.Columns.Add("CHG_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("INSUSER", typeof(string));
                inDataTable.Columns.Add("INSDTTM", typeof(DateTime));
                inDataTable.Columns.Add("UPDUSER", typeof(string));
                inDataTable.Columns.Add("UPDDTTM", typeof(DateTime));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));


                foreach (object added in c1DataGrid.GetCurrentItems())
                {
                    if (DataTableConverter.GetValue(added, "CHK").Equals("True") || DataTableConverter.GetValue(added, "CHK").Equals(1))
                    {
                        DataRow param = inDataTable.NewRow();

                        param["LOTID"] = DataTableConverter.GetValue(added, "LOTID");
                        param["CHG_TYPE"] = DataTableConverter.GetValue(added, "CHG_TYPE");
                        param["NOTE"] = DataTableConverter.GetValue(added, "NOTE");
                        param["PRE_VALUE"] = DataTableConverter.GetValue(added, "PRE_VALUE");
                        param["AFTER_VALUE"] = DataTableConverter.GetValue(added, "AFTER_VALUE");
                        //param["CHG_DTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("CHG_DTTM"));
                        param["INSUSER"] = DataTableConverter.GetValue(added, "INSUSER");
                        //param["INSDTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        param["UPDUSER"] = DataTableConverter.GetValue(added, "UPDUSER");
                        //param["UPDDTTM"] = DataTableConverter.GetValue(added, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        param["SQLTYPE"] = "I";
                        inDataTable.Rows.Add(param);
                    }
                }
                //foreach (object added in dataGrid.GetCurrentItems())
                //{
                //    if (DataTableConverter.GetValue(added, "CHK").Equals("False"))
                //    {
                //        Util.MessageValidation("SFU4287");
                //        return;
                //    }
                //}

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                Util.gridClear(c1DataGrid);

                inDataTable = new DataTable();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
