/*************************************************************************************
 Created Date : 2020.11.18
      Creator : 박준규
   Decription : Lot별 Cell Data 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.18  DEVELOPER : 박준규
  2021.02.23  박수미 
  2022.11.09  조영대    : 조건 날짜 오류 수정 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;
using System.Linq;
using Microsoft.Win32;
using System.Configuration;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// Lot별 Cell Data 조회
    /// </summary>
    public partial class FCS002_046 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string sLotList = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FCS002_046()
        {
            InitializeComponent();
            InitCombo();
            InitControl();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            string[] sFilter = { null };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent, cbChild: cboModelChild, sFilter: sFilter);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboWorkOP };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE", cbParent: cboRouteParent, cbChild:cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            ComCombo.SetCombo(cboWorkOP, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOperParent);

        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            // 로딩 창 
            //StartLoader();

            SetCellDataNew();
        }

        private void btnAssyCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iLotCnt = 0;

                //선택한 lot tray 조회
                string sLot = "";
                string sFileName = "";
                DataSet dsDown = new DataSet();

                for (int i = 0; i < dgCellList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        sLot += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sLot += ",";
                        sFileName += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sFileName += "_";
                        iLotCnt++;
                    }
                }
                
                if (string.IsNullOrEmpty(sLot))
                {
                    //Util.AlertMsg("선택된 LOT이 없습니다.");
                    Util.MessageValidation("SFU1261");
                    return;
                }

                if (iLotCnt > 1)
                {
                    //Util.AlertMsg("하나의 LOT만 선택 가능합니다.");
                    Util.MessageValidation("FM_ME_0469");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = sLot;
                dr["ROUTID"] = Util.GetCondition(cboRoute);
                if (string.IsNullOrEmpty(dr["ROUTID"].ToString())) { Util.MessageValidation("FM_ME_0411"); return; }
                dtRqst.Rows.Add(dr);

                DataTable dtCellInfo = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_FROM_ASSY_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtCellInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1498");
                    return;
                }
                else
                {
                    Dictionary<string, string> dicHeader = new Dictionary<string, string>();

                    dicHeader.Add("PROD_LOTID", " Pakage Lot ID");
                    dicHeader.Add("LOTID", "Lot ID");
                    dicHeader.Add("WIPDTTM_ST", "Job Start Time");
                    dicHeader.Add("CSTID", "Tray ID");
                    dicHeader.Add("SUBLOTID", "Cell ID");
                    dicHeader.Add("CSTSLOT", "Cell No");

                    new ExcelExporter().DtToExcel(dtCellInfo, sLot, dicHeader);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            } 
        }

        private void SetCellDataNew()
        {
            try
            {
                int iLotCnt = 0;

                //선택한 lot tray 조회
                string sLot = "";
                string sFileName = "";
                string sBiz = "";
                string sRout = "";
                DataSet dsDown = new DataSet();
                string sFirstRout = string.Empty;

                for (int i = 0; i < dgCellList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        sLot += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sLot += ",";
                        sFileName += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sFileName += "_";
                        sRout = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "ROUTID"));

                        if (string.IsNullOrEmpty(sFirstRout))
                            sFirstRout = sRout;
                        else if (sFirstRout != sRout)
                        {
                            // 같은 route만 선택해주세요
                            Util.MessageValidation("FM_ME_0518");
                            return;
                        }
                        iLotCnt++;
                    }
                }
                if (string.IsNullOrEmpty(sLot))
                {
                    //Util.AlertMsg("선택된 LOT이 없습니다.");
                    Util.MessageValidation("SFU1261");
                    return;
                }


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("WIPDTTM_ST", typeof(string));
                dtRqst.Columns.Add("WIPDTTM_ED", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTIDLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = sLot;
                //dr["WIPDTTM_ST"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:00");
                //dr["WIPDTTM_ED"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:00");
                dr["WIPDTTM_ST"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00.000");
                dr["WIPDTTM_ED"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59.997");
                dr["ROUTID"] = sRout;
                if (string.IsNullOrEmpty(dr["ROUTID"].ToString())) { Util.MessageValidation("FM_ME_0411"); return; }
                dtRqst.Rows.Add(dr);
                dr["LOTIDLIST"] = sLotList;
                DataTable dtCellInfo = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_NEW_MB", "RQSTDT", "RSLTDT", dtRqst);
                //여기까지 선택한 lot tray 조회
                if (dtCellInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1498");
                    return;
                }

               

                dtCellInfo.PrimaryKey = new DataColumn[] { dtCellInfo.Columns["LOTID"], dtCellInfo.Columns["CSTSLOT"] };

                for (int i = 0; i < dtCellInfo.Columns.Count; i++)
                {
                    string sColName = ObjectDic.Instance.GetObjectName(dtCellInfo.Columns[i].ColumnName);
                    dtCellInfo.Columns[i].ColumnName = sColName;
                }
                DataTable dtOp = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_ROUTE_OP_MB", "RQSTDT", "RSLTDT", dtRqst);
                for (int i = 0; i < dtOp.Rows.Count; i++)
                {
                    dtRqst.Rows[0]["PROCID"] = dtOp.Rows[i]["CBO_CODE"];
                    switch (dtOp.Rows[i]["S27"].ToString())
                    {
                        case "1A":
                        case "1B":
                        case "1C":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_LCI_NEW_MB";
                            break;
                        case "11":
                        case "J1":
                        case "12":
                        case "J2":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_NEW_MB";
                            break;

                        case "13":
                        case "J3":
                        case "81":
                        case "A1":
                        case "A2":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_OCV_NEW_MB";
                            break;

                        case "I1":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_IROCV_NEW_MB";
                            break;
                        case "17":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_MEGA_CHG_NEW_MB";
                            break;
                        case "31":
                        case "41":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_AGING_NEW_MB";
                            break;
                        default: continue; //위 경우를 제외하고 처리 안함
                    }
                    DataTable dtRsltMeas = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRsltMeas.Rows.Count > 0)
                    {
                        if (!dtOp.Rows[i]["S27"].ToString().Equals("12")) //DISCHARGE 에서만  FITCAPA 존재
                        {
                            if (dtRsltMeas.Columns.IndexOf("FITCAPA_VAL") > -1)
                            {
                                dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("FITCAPA_VAL"));
                            }
                        }

                        // 02/19 파워 추가.
                        //if (!dtOp.Rows[i]["S27"].ToString().Equals("J1")) //JIG CHARGE 에서만  POWER 존재
                        //{
                        //    if (dtRsltMeas.Columns.IndexOf("POWER") > -1)
                        //    {
                        //        dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("POWER"));
                        //    }
                        //}

                        if (!dtOp.Rows[i]["S27"].ToString().Equals("J")) //JIG 에서 검색 안하는 경우에는 JIG 설비가 나올수 없음 JIG 끝나고 트레이 갈아탐
                        {
                            if (dtRsltMeas.Columns.IndexOf("EQP") > -1)
                            {
                                if (chkAddEQPName.IsChecked == false)
                                {
                                    dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("EQP"));
                                }
                            }
                        }

                        if (dtRsltMeas.Columns.IndexOf("EQP") > -1 && string.IsNullOrEmpty(dtRsltMeas.Rows[0]["EQP"].ToString()))// 첫번째 row 의 eqp_id 가 없는 경우 제거, JIG 에서 검색 안하는 경우에는 JIG 설비가 나올수 없슴 JIG 끝나고 트레이 갈아탐
                        {
                            //설비명 추가 체크박스
                            if (chkAddEQPName.IsChecked == false)
                            {
                                dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("EQP"));
                            }
                        }
                        LeftOuterJoin(ref dtCellInfo, dtRsltMeas, dtOp.Rows[i]["CBO_NAME"].ToString());
                    }
                    //온도정보 추가 체크박스
                    if ((chkAddTemp.IsChecked == true) && sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_NEW_MB")
                    {
                        DataTable dtRsltTemp = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_TEMP_NEW_MB", "RQSTDT", "RSLTDT", dtRqst);
                        LeftOuterJoin(ref dtCellInfo, dtRsltTemp, dtOp.Rows[i]["CBO_NAME"].ToString());
                    }
                    else if ((chkAddTemp.IsChecked == true) && 
                             (sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_LCI_NEW_MB" || sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_OCV_NEW_MB" || sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_IROCV_NEW_MB")
                             )
                    {
                        DataTable dtRsltTemp = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_PROC_TEMP_NEW_MB", "RQSTDT", "RSLTDT", dtRqst);
                        LeftOuterJoin(ref dtCellInfo, dtRsltTemp, dtOp.Rows[i]["CBO_NAME"].ToString());
                    }
                }
                C1DataGrid dgCellInfo = new C1DataGrid();
                dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCellInfo);
                Export(dgCellInfo);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #region[ExcelExport]
        public void Export(C1DataGrid dataGrid, string defaultFileName = null, bool bOpen = false)
        {
            MemoryStream ms = new MemoryStream();
            dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = ExcelFileFormat.Xlsx, KeepColumnWidths = true, KeepRowHeights = true });
            ms.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(ms);
            List<int> deleteIndex = new List<int>();
            foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
            {
                if (row.Visibility == System.Windows.Visibility.Collapsed)
                {
                    deleteIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
                }
            }

            foreach (int index in (from i in deleteIndex orderby i descending select i))
            {
                if (index < book.Sheets[0].Rows.Count)
                    book.Sheets[0].Rows.RemoveAt(index);
            }
            for (int rowinx = 0; rowinx < book.Sheets[0].Rows.Count; rowinx++)
            {
                for (int colinx = 0; colinx < book.Sheets[0].Columns.Count; colinx++)
                {
                    // 2017.03.30 이슬아
                    // DB에서 Header를 조회하여 다국어처리된 값을 업무테이블 동적으로 가져오나 
                    // 다국어테이블에는 등록이 안되서 [#]이 붙는 경우가 발생하여 replace 처리하고 싶었으나, 일단 주석처리함
                    if (rowinx == 0)
                    {
                        // book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text.Replace("[#]", string.Empty), book.Sheets[0].GetCell(rowinx, colinx).Style);
                        book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text, book.Sheets[0].GetCell(rowinx, colinx).Style);
                    }

                    if (book.Sheets[0].GetCell(rowinx, colinx).Style != null)
                        book.Sheets[0].GetCell(rowinx, colinx).Style.Font = new XLFont("Arial", 12);
                }
            }

            MemoryStream editedms = new MemoryStream();
            //===================================================================================================================
            if (dataGrid.Resources.Contains("ExportRemove"))
            {
                List<int> removecol = dataGrid.Resources["ExportRemove"] as List<int>;
                for (int idx = removecol.Count; idx > 0; idx--)
                {
                    book.Sheets[0].Columns.RemoveAt(removecol[idx - 1]);
                }
            }
            //===================================================================================================================

            //   AutoSizeColumns(book.Sheets[0]);

            book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
            editedms.Seek(0, SeekOrigin.Begin);
            string tempFilekey = defaultFileName + DateTime.Now.ToString("yyyyMMddHHmmss");

            //string tempFilekey = Guid.NewGuid().ToString("N");
            new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            {
                ms.Close();
                editedms.Close();
                if (arg.Success)
                {
                    //new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
                }
            }, bOpen);
        }
        internal class StreamUploader
        {
            internal void uploadTempStream(string filekey, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler, bool bOpen = false)
            {
                int blockSize = 1024 * 1024 * 100;
                //int blockSize = 1024 * 1024 * 3;
                int readedTotal = 0;
                int partNumber = 1;
                int uploadedNumber = 0;
                int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

                try
                {
                    SaveFileDialog od = new SaveFileDialog();
                    if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                    {
                        od.InitialDirectory = @"\\Client\C$\Users";
                    }
                    od.Filter = "Excel Files (.xlsx)|*.xlsx";
                    od.FileName = filekey + "_" + partNumber.ToString() + ".xlsx";

                    if (od.ShowDialog() == true)
                    {
                        while (stream.Length > readedTotal)
                        {
                            byte[] buffer = new byte[blockSize];
                            int readed = stream.Read(buffer, 0, blockSize);
                            readedTotal += readed;

                            //FileInfo tempFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + filekey + "_" + partNumber.ToString() + ".xlsx");
                            FileInfo tempFile = new FileInfo(od.FileName);

                            using (FileStream fs = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                fs.Write(buffer, 0, readed);
                                fs.Flush();
                                fs.Close();
                            }
                            partNumber++;
                        }
                        //  WebClient client = new WebClient();
                        // uploadCompleteHandler(client, new UploadCompletedEventArgs(true, false, null, null, null, userState));
                        // if (bOpen)
                        // System.Diagnostics.Process.Start(od.FileName);
                    }
                }
                catch { }
            }
        }
        #endregion
        private void GetList()
        {
            try
            {
                sLotList = string.Empty;

                Util.gridClear(dgCellList);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("START_TIME", typeof(string));
                dtRqst.Columns.Add("END_TIME", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);

                //dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);

                dr["ROUTID"] = Util.GetCondition(cboRoute, sMsg: "FM_ME_0411");  //공정경로을 선택해주세요.
                if (string.IsNullOrEmpty(dr["ROUTID"].ToString())) return;

                dr["PROCID"] = Util.GetCondition(cboWorkOP, bAllNull: true);
                dr["START_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00.000");
                dr["END_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59.997");

                if (!string.IsNullOrEmpty(txtLotID.Text.Trim()))
                    dr["PROD_LOTID"] = txtLotID.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_LOT_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_TRAY_LOT_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);
                }
                Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);

                if (dtRslt1.Rows.Count > 0)
                {
                    for(int i = 0; i<dtRslt1.Rows.Count; i++)
                    {
                        if(i==0)
                            sLotList += dtRslt1.Rows[i]["LOTID"];
                        else
                            sLotList += ","+dtRslt1.Rows[i]["LOTID"];
                    }
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                //if (dg.CurrentColumn.Name.Equals("TRAY_ID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                //    || dg.CurrentColumn.Name.Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                //{

                //}
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

        private void LeftOuterJoin(ref DataTable dtCellInfo, DataTable dtRslt1, string sOpName)
        {
            for (int i = 0; i < dtRslt1.Columns.Count; i++)
            {
                if (dtRslt1.Columns[i].ColumnName != "LOTID" && dtRslt1.Columns[i].ColumnName != "CSTSLOT")
                {
                    if (dtRslt1.Columns[i].ColumnName.Equals("EQP") || dtRslt1.Columns[i].ColumnName.Equals("WIPDTTM_ST") || dtRslt1.Columns[i].ColumnName.Equals("WIPDTTM_ED"))
                    {//ObjectDic.Instance.GetObjectName("TROUBLE")
                        dtCellInfo.Columns.Add(sOpName + " " + ObjectDic.Instance.GetObjectName(dtRslt1.Columns[i].ColumnName), typeof(string));
                        //dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(string));
                    }
                    else
                    {
                        dtCellInfo.Columns.Add(sOpName + " " + ObjectDic.Instance.GetObjectName(dtRslt1.Columns[i].ColumnName), typeof(string));
                        //dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(string));
                    }


                    for (int j = 0; j < dtRslt1.Rows.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString()))
                        {

                            if (dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), dtRslt1.Rows[j]["CSTSLOT"].ToString() }) != null)
                            {
                                dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), dtRslt1.Rows[j]["CSTSLOT"].ToString() })[sOpName + " " + ObjectDic.Instance.GetObjectName(dtRslt1.Columns[i].ColumnName)] = dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString();
                          //      dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), dtRslt1.Rows[j]["CSTSLOT"].ToString() })[sOpName + " " + dtRslt1.Columns[i].ColumnName] = dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString();
                            }
                        }
                    }
                }
            }
        }

        private void LeftOuterJoinTemp(ref DataTable dtCellInfo, DataTable dtRslt1, string sOpName)
        {
            for (int i = 0; i < dtRslt1.Columns.Count; i++)
            {
                if (dtRslt1.Columns[i].ColumnName != "LOTID")
                {
                    dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(Single));

                    for (int j = 0; j < dtRslt1.Rows.Count; j++)
                    {
                        DataRow[] drArray = dtCellInfo.Select("LOTID='" + dtRslt1.Rows[j]["LOTID"].ToString() + "'");

                        for (int k = 0; k < drArray.Length; k++) //동일 트레이는 모두 같은 값으로 셋팅하도록 처리
                        {
                            if (!string.IsNullOrEmpty(dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString()))
                            {
                                dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), drArray[k]["CSTSLOT"].ToString() })[sOpName + " " + dtRslt1.Columns[i].ColumnName] = dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString();
                            }
                        }
                    }
                }
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgCellList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgCellList);
        }

        private void dgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        
    }
}
