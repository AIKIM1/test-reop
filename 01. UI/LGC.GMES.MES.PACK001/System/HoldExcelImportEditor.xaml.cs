/*************************************************************************************
 Created Date : 2020.01.27
      Creator : 염규범 S
   Decription : 엑셀 Upload 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.27  염규범 : Initial Created.
  2022.09.20  정용석 : 오류 수정
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class HoldExcelImportEditor : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private DataTable dtParentHoldLOT = new DataTable();
        private DataTable dtResult = new DataTable();
        #endregion

        #region Constructor...
        public HoldExcelImportEditor()
        {
            InitializeComponent();
        }

        public HoldExcelImportEditor(DataTable dtHoldLOT, bool generateUI = false)
        {
            InitializeComponent();
            this.dtParentHoldLOT = dtHoldLOT;
        }
        #endregion

        #region Properties...
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable ImportHoldData
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
        #endregion

        #region Member Function Lists
        private DataTable GetLOTIDValidation(DataTable dtLoadedData)
        {
            string bizRuleName = "DA_BAS_SEL_CELLID_VALI";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = dtLoadedData.AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["HOLD_FLAG"] = "N";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
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

                string tempFilekey = "HOLD_TEMPLATE";

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

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] obj = C1WindowExtension.GetParameters(this);

            if (obj != null && obj.Length >= 1)
            {
                this.dtParentHoldLOT.Clear();
                this.dtParentHoldLOT = (DataTable)obj[0];
            }
        }

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
                dtTemp.Columns.Add("INPUT_LOTID", typeof(string));
                dtTemp.Columns.Add("LOTID", typeof(string));
                dtTemp.Columns.Add("PRODID", typeof(string));
                dtTemp.Columns.Add("WOID", typeof(string));
                dtTemp.Columns.Add("PRODNAME", typeof(string));
                dtTemp.Columns.Add("EQSGID", typeof(string));
                dtTemp.Columns.Add("EQSGNAME", typeof(string));
                dtTemp.Columns.Add("PROCNAME", typeof(string));
                dtTemp.Columns.Add("WIPSNAME", typeof(string));
                dtTemp.Columns.Add("BOXID", typeof(string));

                using (Stream stream = openDialog.OpenFile())
                {
                    DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0, true);

                    // Excel 제목줄은 Import 대상 아님.
                    if (dtExcelData.Rows[0][0].ToString().ToUpper().Contains("LOT ID"))
                    {
                        dtExcelData.Rows[0].Delete();
                    }
                    dtExcelData.AcceptChanges();

                    if (dtExcelData.Rows.Count > 400)
                    {
                        Util.MessageInfo("SFU8164");
                        Util.gridClear(this.c1DataGrid);
                        return;
                    }

                    if (CommonVerify.HasTableRow(dtExcelData))
                    {
                        foreach (DataRow dr in dtExcelData.Rows)
                        {
                            DataRow drTemp = dtTemp.NewRow();
                            drTemp["INPUT_LOTID"] = dr.ItemArray[0].ToString();
                            drTemp["LOTID"] = dr.ItemArray[0].ToString();
                            drTemp["EQSGNAME"] = dr.ItemArray[1].ToString();
                            drTemp["PROCNAME"] = dr.ItemArray[2].ToString();
                            drTemp["WIPSNAME"] = dr.ItemArray[3].ToString();
                            drTemp["PRODID"] = dr.ItemArray[4].ToString();
                            drTemp["WOID"] = dr.ItemArray[5].ToString();
                            drTemp["BOXID"] = dr.ItemArray[6].ToString();
                            dtTemp.Rows.Add(drTemp);
                        }

                        this.c1DataGrid.ItemsSource = DataTableConverter.Convert(dtTemp);
                        this.txtLoadedRowCount.Text = dtTemp.Rows.Count.ToString() + "/1000";
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
                if (dtLoadedData.AsEnumerable().GroupBy(x => x.Field<string>("LOTID")).Where(grp => grp.Count() > 1).Select(x => x.Key).Count() > 0)
                {
                    Util.MessageInfo("SFU8166");
                    return;
                }

                // 순서도 호출
                DataTable dtRSLTDT = GetLOTIDValidation(dtLoadedData);

                // PACK001_060의 Validation Check 내용이랑 비슷함.
                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtLoadedData.AsEnumerable().Select(x => new
                    {
                        CHK = "True",
                        INPUT_LOTID = x.Field<string>("LOTID"),
                        LOTID = x.Field<string>("LOTID"),
                        EQSGNAME = string.Empty,
                        EQSGID = string.Empty,
                        PROCNAME = string.Empty,
                        WIPSTAT = string.Empty,
                        WIPSNAME = string.Empty,
                        PRODID = string.Empty,
                        WOID = string.Empty,
                        PRODNAME = string.Empty,
                        BOXID = string.Empty,
                        NOTE = MessageDic.Instance.GetMessage("SFU1905")  // SFU1905 : 조회된 Data가 없습니다.
                    });

                    Util.GridSetData(this.c1DataGrid, PackCommon.queryToDataTable(query.ToList()), FrameOperation);
                    return;
                }

                // 불건전 Data 1호 : SFU1905 : 조회된 Data가 없습니다.
                var queryUnwholesome1 = dtLoadedData.AsEnumerable().Where(x => !dtRSLTDT.AsEnumerable().Where(y => x.Field<string>("LOTID") == y.Field<string>("INPUT_LOTID")).Any()).Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    EQSGNAME = string.Empty,
                    EQSGID = string.Empty,
                    PROCNAME = string.Empty,
                    WIPSTAT = string.Empty,
                    WIPSNAME = string.Empty,
                    PRODID = string.Empty,
                    WOID = string.Empty,
                    PRODNAME = string.Empty,
                    BOXID = string.Empty,
                    OCOP_RTN_FLAG = string.Empty,
                    NOTE = MessageDic.Instance.GetMessage("SFU1905")
                });

                // 불건전 Data 2호 : SFU3335 : 입력오류 : LOGIN 사용자의 동정보와 LOT의 동정보가 다릅니다.
                var queryUnwholesome2 = dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("AREAID") != LoginInfo.CFG_AREA_ID).Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    EQSGNAME = x.Field<string>("EQSGNAME"),
                    EQSGID = x.Field<string>("EQSGID"),
                    PROCNAME = x.Field<string>("PROCNAME"),
                    WIPSTAT = x.Field<string>("WIPSTAT"),
                    WIPSNAME = x.Field<string>("WIPSNAME"),
                    PRODID = x.Field<string>("PRODID"),
                    WOID = x.Field<string>("WOID"),
                    PRODNAME = x.Field<string>("PRODNAME"),
                    BOXID = x.Field<string>("BOXID"),
                    OCOP_RTN_FLAG = x.Field<string>("OCOP_RTN_FLAG"),
                    NOTE = MessageDic.Instance.GetMessage("SFU3335")
                });

                // 불건전 Data 3호 : SFU8367 : HOLD 할수 없는 WIP 상태입니다.
                var queryUnwholesome3 = dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("WIPSTAT") == "TERM" || x.Field<string>("WIPSTAT") == "MOVING").Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    EQSGNAME = x.Field<string>("EQSGNAME"),
                    EQSGID = x.Field<string>("EQSGID"),
                    PROCNAME = x.Field<string>("PROCNAME"),
                    WIPSTAT = x.Field<string>("WIPSTAT"),
                    WIPSNAME = x.Field<string>("WIPSNAME"),
                    PRODID = x.Field<string>("PRODID"),
                    WOID = x.Field<string>("WOID"),
                    PRODNAME = x.Field<string>("PRODNAME"),
                    BOXID = x.Field<string>("BOXID"),
                    OCOP_RTN_FLAG = x.Field<string>("OCOP_RTN_FLAG"),
                    NOTE = MessageDic.Instance.GetMessage("SFU8367")
                });

                // 불건전 Data 4호 : SFU2835 : Grid에 중복된 ID가 있습니다.
                var queryUnwholesome4 = dtRSLTDT.AsEnumerable().Where(x => this.dtParentHoldLOT.AsEnumerable().Where(y => y.Field<string>("LOTID") == x.Field<string>("LOTID")).Any()).Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    EQSGNAME = x.Field<string>("EQSGNAME"),
                    EQSGID = x.Field<string>("EQSGID"),
                    PROCNAME = x.Field<string>("PROCNAME"),
                    WIPSTAT = x.Field<string>("WIPSTAT"),
                    WIPSNAME = x.Field<string>("WIPSNAME"),
                    PRODID = x.Field<string>("PRODID"),
                    WOID = x.Field<string>("WOID"),
                    PRODNAME = x.Field<string>("PRODNAME"),
                    BOXID = x.Field<string>("BOXID"),
                    OCOP_RTN_FLAG = x.Field<string>("OCOP_RTN_FLAG"),
                    NOTE = MessageDic.Instance.GetMessage("SFU2835")
                });

                // 건전 Data와 불건전 Data 분리 (CHK Column으로 Upload 가능, Upload 불가 여부 판단함.)
                // 부모 Grid의 CHK Column이 False가 기본 Setting이므로 Upload 가능 데이터의 CHK Field값은 False임
                var result = (from d1 in dtRSLTDT.AsEnumerable()
                              join d2 in queryUnwholesome1.Union(queryUnwholesome2).Union(queryUnwholesome3).Union(queryUnwholesome4) on d1.Field<string>("INPUT_LOTID") equals d2.INPUT_LOTID into outerJoin
                              from d3 in outerJoin.DefaultIfEmpty()
                              select new
                              {
                                  BASEDATA = d1,
                                  SELECTEDDATA = d3
                              }).Select(x => new
                              {
                                  CHK = x.SELECTEDDATA == null ? "False" : "True",
                                  INPUT_LOTID = x.SELECTEDDATA == null ? x.BASEDATA.Field<string>("INPUT_LOTID") : x.SELECTEDDATA.INPUT_LOTID,
                                  LOTID = x.BASEDATA.Field<string>("LOTID"),
                                  EQSGNAME = x.BASEDATA.Field<string>("EQSGNAME"),
                                  EQSGID = x.BASEDATA.Field<string>("EQSGID"),
                                  PROCNAME = x.BASEDATA.Field<string>("PROCNAME"),
                                  WIPSTAT = x.BASEDATA.Field<string>("WIPSTAT"),
                                  WIPSNAME = x.BASEDATA.Field<string>("WIPSNAME"),
                                  PRODID = x.BASEDATA.Field<string>("PRODID"),
                                  WOID = x.BASEDATA.Field<string>("WOID"),
                                  PRODNAME = x.BASEDATA.Field<string>("PRODNAME"),
                                  BOXID = x.BASEDATA.Field<string>("BOXID"),
                                  OCOP_RTN_FLAG = x.BASEDATA.Field<string>("OCOP_RTN_FLAG"),
                                  NOTE = x.SELECTEDDATA == null ? ObjectDic.Instance.GetObjectName("정상") : x.SELECTEDDATA.NOTE
                              });

                Util.GridSetData(this.c1DataGrid, PackCommon.queryToDataTable(result.ToList()), FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper() == "FALSE");
                if (query.Count() <= 0)
                {
                    Util.MessageInfo("SFU8519");    // Upload가 가능한 데이터가 존재하지 않습니다.
                    return;
                }

                this.dtResult = query.CopyToDataTable();
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
