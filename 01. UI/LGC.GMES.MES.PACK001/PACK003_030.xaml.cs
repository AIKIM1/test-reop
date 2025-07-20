/************************************************************************************
  Created Date : 2022.01.20
       Creator : 정용석
   Description : Group별 Pallet 라벨 발행
 ------------------------------------------------------------------------------------
  [Change History]
    2022.01.20  정용석 : Initial Created.
 ************************************************************************************/
using C1.C1Preview;
using C1.C1Report;
using C1.WPF.C1Report;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_030 : UserControl, IWorkArea, IDisposable
    {
        #region Member Variable Lists...
        private List<C1Report> lstC1Report = new List<C1Report>();
        C1Report targetReport = new C1Report();
        C1DocumentViewer c1DocumentViewerLocal = new C1DocumentViewer();
        bool previewFlag = false;
        private PACK003_030_DataHelper dataHelper = new PACK003_030_DataHelper();
        private DataTable dtOutDataDetail = new DataTable();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK003_030()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            PackCommon.SetC1ComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboPackEquipmentID, "ALL");

            // Group ID Label Set
            this.lblGroupID.Text = ObjectDic.Instance.GetObjectName("GROUPID") + "/" + ObjectDic.Instance.GetObjectName("LOTID"); 

            // Button
            this.btnPreView.IsEnabled = false;
            this.btnPrint.IsEnabled = false;
            this.IsPreviewClick(false);
        }

        // 조회 - Group ID 또는 LOTID 붙여넣고 조회
        private void SearchClipboardData()
        {
            try
            {
                this.txtGroupID.Text = string.Empty;
                string[] separators = new string[] { "\r\n" };
                string clipboardText = Clipboard.GetText();
                string[] arrLOTIDList = clipboardText.Split(separators, StringSplitOptions.None);

                if (arrLOTIDList.Count() > 10)
                {
                    Util.MessageValidation("SFU4643");   // 최대 10개 까지 가능합니다.
                    return;
                }

                for (int i = 0; i < arrLOTIDList.Length; i++)
                {
                    if (string.IsNullOrEmpty(arrLOTIDList[i]))
                    {
                        Util.MessageInfo("SFU1190", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                return;
                            }
                        });
                    }
                }

                this.txtGroupID.Text = string.Join(",", arrLOTIDList);
                this.SearchProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        // 조회
        private void SearchProcess()
        {
            // 재발행 CheckBox가 Checked인 경우 GroupID / LOTID TextBox에 데이터가 없으면 Interlock
            if (this.chkReprint.IsChecked == true && string.IsNullOrEmpty(this.txtGroupID.Text))
            {
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("GROUPID") + " OR " + ObjectDic.Instance.GetObjectName("LOTID")); // %1이 입력되지 않았습니다.
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            this.btnPreView.IsEnabled = false;
            this.btnPrint.IsEnabled = false;
            this.IsPreviewClick(false);
            this.ClearProcess();
            PackCommon.SearchRowCount(ref this.txtGroupIDCount, 0);
            PackCommon.SearchRowCount(ref this.txtLOTIDCount, 0);
            Util.gridClear(this.dgGrid1);
            Util.gridClear(this.dgGrid2);

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();


            try
            {
                string logisPackType = "TRU";                                                           // 물류포장유형(TRU: 트럭, CHG: 충방전)
                string labelPrintFlag = this.chkReprint.IsChecked == true ? "Y" : "N";                  // 라벨발행여부
                //if (!string.IsNullOrEmpty(this.txtGroupID.Text))
                //{
                //    labelPrintFlag = string.Empty;
                //}

                DataSet ds = this.dataHelper.GetPalletLabelData( LoginInfo.LANGID                       // (*)Language ID
                                                               , LoginInfo.CFG_AREA_ID                  // (*)Area ID
                                                               , this.txtGroupID.Text                   // Group ID(빠레뜨 ID)
                                                               , string.Empty                           // Group ID 생성일자(From)
                                                               , string.Empty                           // Group ID 생성일자(To)
                                                               , logisPackType                          // 물류포장유형(TRU: 트럭, CHG: 충방전)
                                                               , labelPrintFlag                         // 라벨발행여부(Default: 없음)
                                                               , this.cboPackEquipmentID.SelectedValue  // 포장기 ID                
                                                               );
                if (CommonVerify.HasTableInDataSet(ds))
                {
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_SUMMARY"]))
                    {
                        PackCommon.SearchRowCount(ref this.txtGroupIDCount, ds.Tables["OUTDATA_SUMMARY"].Rows.Count);
                        Util.GridSetData(this.dgGrid1, ds.Tables["OUTDATA_SUMMARY"], FrameOperation);
                    }

                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_DETAIL"]))
                    {
                        this.dtOutDataDetail.Clear();
                        this.dtOutDataDetail = ds.Tables["OUTDATA_DETAIL"].Copy();
                        PackCommon.SearchRowCount(ref this.txtLOTIDCount, ds.Tables["OUTDATA_DETAIL"].Rows.Count);
                        Util.GridSetData(this.dgGrid2, ds.Tables["OUTDATA_DETAIL"], FrameOperation);
                    }

                    this.btnPreView.IsEnabled = true;
                    this.btnPrint.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 라벨 Print - 라벨과 매핑되어 있는 Data는 조회시에 다 가지고 온다, 별도 LABEL DATA를 가져오는 Action은 생략함.
        private void PrintProcess()
        {
            // Validation Check...
            if (this.dgGrid1 == null || this.dgGrid1.ItemsSource == null || this.dgGrid1.Rows.Count < 0 ||
                this.dgGrid2 == null || this.dgGrid2.ItemsSource == null || this.dgGrid2.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return;
            }

            List<string> lstGroupID = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Select(x => x.Field<string>("GR_ID")).ToList();
            this.GroupLabelPrint(lstGroupID);
        }

        // 라벨 Print
        private void GroupLabelPrint(List<string> lstGroupID, bool isSearchFlag = false)
        {
            DataSet ds = new DataSet();
            if (isSearchFlag)
            {
                ds = this.dataHelper.GetPalletLabelData(LoginInfo.LANGID                // (*)Language ID
                                                      , LoginInfo.CFG_AREA_ID           // (*)Area ID
                                                      , string.Join(",", lstGroupID)    // Group ID(빠레뜨 ID)
                                                      , string.Empty                    // Group ID 생성일자(From)
                                                      , string.Empty                    // Group ID 생성일자(To)
                                                      , "TRU"                           // 물류포장유형(TRU: 트럭, CHG: 충방전)
                                                      , string.Empty                    // 라벨발행여부(Default: 없음)
                                                      , "ALL"                           // 포장기 ID                
                                                      );

                if (!CommonVerify.HasTableInDataSet(ds))
                {
                    return;
                }

                if (!CommonVerify.HasTableRow(ds.Tables["OUTDATA_DETAIL"]))
                {
                    return;
                }

                if (this.targetReport == null || this.targetReport.C1Document.Body.Children.Count <= 0)
                {
                    this.MakeReport(lstGroupID, ds.Tables["OUTDATA_DETAIL"]);
                }
            }
            else
            {
                if (!CommonVerify.HasTableRow(this.dtOutDataDetail))
                {
                    return;
                }

                if (this.targetReport == null || this.targetReport.C1Document.Body.Children.Count <= 0)
                {
                    this.MakeReport(lstGroupID, this.dtOutDataDetail);
                }
            }


            // 그리드에 데이터가 있으면 Print
            if (this.targetReport != null && this.targetReport.C1Document.Body.Children.Count > 0)
            {
                this.c1DocumentViewerLocal.Document = null;
                this.c1DocumentViewerLocal.Document = this.targetReport.FixedDocumentSequence;
                this.c1DocumentViewerLocal.Print();

                // 최초발행 Flag Set
                foreach (string groupID in lstGroupID)
                {
                    if (!string.IsNullOrEmpty(groupID))
                    {
                        this.dataHelper.SetGroupBoxPrintYN(groupID);
                    }
                }
            }
        }

        private void ClearProcess()
        {
            this.c1DocumentViewerLocal.Document = null;
            this.c1DocumentViewer1.Document = null;
            this.lstC1Report.Clear();
            this.targetReport.C1Document.Body.Children.Clear();
        }

        // 미리보기 View 보이기 숨기기 Type 1
        private void PreViewClick()
        {
            this.previewFlag = !this.previewFlag;
            if (this.previewFlag == true)
            {
                this.col0.Width = new GridLength(20, GridUnitType.Star);
                this.col1.Width = new GridLength(30, GridUnitType.Star);
                this.col2.Width = new GridLength(50, GridUnitType.Star);
            }
            else
            {
                this.col0.Width = new GridLength(40, GridUnitType.Star);
                this.col1.Width = new GridLength(60, GridUnitType.Star);
                this.col2.Width = new GridLength(0, GridUnitType.Star);
            }
        }

        // 미리보기 View 보이기 숨기기 Type 2
        private void IsPreviewClick(bool previewFlag)
        {
            this.previewFlag = previewFlag;
            if (this.previewFlag == true)
            {
                this.col0.Width = new GridLength(20, GridUnitType.Star);
                this.col1.Width = new GridLength(30, GridUnitType.Star);
                this.col2.Width = new GridLength(50, GridUnitType.Star);
            }
            else
            {
                this.col0.Width = new GridLength(40, GridUnitType.Star);
                this.col1.Width = new GridLength(60, GridUnitType.Star);
                this.col2.Width = new GridLength(0, GridUnitType.Star);
            }
        }

        // 라벨 미리보기
        private void PreviewProcess()
        {
            this.IsPreviewClick(true);
            this.ClearProcess();

            if (this.dgGrid1 == null || this.dgGrid1.ItemsSource == null || this.dgGrid1.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                //this.IsPreviewClick(false);
                return;
            }

            List<string> lstPalletID = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Select(x => x.Field<string>("GR_ID")).ToList();
            if (lstPalletID.Count <= 0)
            {
                return;
            }

            //// For Test
            //List<string> lstPalletID = new List<string>();
            //lstPalletID.Add("PP8Q03UJ23MD001");
            //lstPalletID.Add("PP8Q03UJ23MD002");
            //lstPalletID.Add("PP8Q03UJ23MD003");
            //lstPalletID.Add("PP8Q03UJ23MD004");
            //lstPalletID.Add("PP8Q03UJ23MD005");
            //lstPalletID.Add("PP8Q03UJ23MD006");
            //lstPalletID.Add("PP8Q03UJ23MD007");

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            //DataTable dtPalletDataDetail = DataTableConverter.Convert(this.dgGrid2.ItemsSource);
            this.MakeReport(lstPalletID, this.dtOutDataDetail);
            if (this.targetReport != null && this.targetReport.C1Document.Body.Children.Count > 0)
            {
                this.c1DocumentViewer1.Document = this.targetReport.FixedDocumentSequence;
            }
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 라벨 데이터 만들기
        private void MakeReport(List<string> lstGroupID, DataTable dtPalletDataDetail)
        {
            DataSet dsPallet = new DataSet();
            DataSet dsReportBinding = new DataSet();

            if (!CommonVerify.HasTableRow(dtPalletDataDetail))
            {
                return;
            }

            // 라벨 데이터 만들기
            // Step 1 : 라벨 데이터 만들기 (PalletID 1개당 1장이 출력되므로, DataTable을 PalletID 단위로 구성
            foreach (string palletID in lstGroupID)
            {
                DataTable dt = dtPalletDataDetail.AsEnumerable().Where(x => x.Field<string>("GR_ID").Equals(palletID)).CopyToDataTable();
                dt.TableName = palletID;
                dsPallet.Tables.Add(dt);
            }

            // Step 2 : 라벨 Design XML의 Control과 Step 1에서 만든 Data Entity의 Binding
            foreach (DataTable dt in dsPallet.Tables.OfType<DataTable>())
            {
                dsReportBinding.Tables.Add(this.BindingLabel_MEBCMA(dt));
            }

            // Step 3 : 라벨 데이터와 Report Binding
            foreach (DataTable dtReportBinding in dsReportBinding.Tables.OfType<DataTable>())
            {
                C1Report c1Report = this.ReportDataBinding(dtReportBinding);
                this.lstC1Report.Add(c1Report);
            }

            // Step 4 : Report 데이터를 가지고 작업하기
            this.targetReport.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
            this.targetReport.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
            this.targetReport.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";

            this.targetReport.C1Document.Body.Children.Clear();
            foreach (C1Report c1Report in lstC1Report)
            {
                if (c1Report == null)
                {
                    continue;
                }

                c1Report.Render();
                foreach (Metafile metafile in c1Report.GetPageImages())
                {
                    RenderImage PageImage = new RenderImage(metafile);
                    this.targetReport.C1Document.Body.Children.Add(PageImage);
                }
            }

            this.targetReport.C1Document.Reflow();
        }

        // 라벨 데이터 만들기
        private DataTable BindingLabel_MEBCMA(DataTable dt)
        {
            DataTable dtReturn = new DataTable();
            dtReturn.TableName = dt.TableName;
            dtReturn.Columns.Add("BARCODE", typeof(string));
            dtReturn.Columns.Add("PALLETID", typeof(string));
            dtReturn.Columns.Add("DATE", typeof(string));
            dtReturn.Columns.Add("BOXSEQ", typeof(string));             // 포장 단수
            dtReturn.Columns.Add("BOXCNT", typeof(string));             // Hold 여부
            dtReturn.Columns.Add("LOTTOTALCNT", typeof(string));        // LOT의 총 갯수
            dtReturn.Columns.Add("USER", typeof(string));
            dtReturn.Columns.Add("PRODID", typeof(string));
            dtReturn.Columns.Add("PRODCNT", typeof(string));

            if (!CommonVerify.HasTableRow(dt))
            {
                return dtReturn;
            }

            try
            {
                int boxcnt = dt.Rows.Count;
                int index = 2;
                string prefixBoxID = "BOXID";
                string prefixBoxLOTCount = "BOXLOTCNT";
                string prefixGBTID = "GBT";
                int i_GBT = 0;
                int i_MBOM = 0;

                for (int j = 1; j <= boxcnt; j++)
                {
                    dtReturn.Columns.Add(prefixBoxID + j.ToString(), typeof(string));
                    dtReturn.Columns.Add(prefixBoxLOTCount + j.ToString(), typeof(string));     // Hold 여부
                    dtReturn.Columns.Add(prefixGBTID + j, typeof(string));
                }


                DataRow drBindData = dtReturn.NewRow();
                drBindData["BARCODE"] = dt.Rows[0]["GR_ID"].ToString();
                drBindData["PALLETID"] = dt.Rows[0]["GR_ID"].ToString();
                drBindData["DATE"] = dt.Rows[0]["GR_SET_DTTM"].ToString();
                drBindData["BOXSEQ"] = dt.Rows[0]["LOAD_SEQ"].ToString();
                drBindData["BOXCNT"] = dt.Rows.Count;
                drBindData["LOTTOTALCNT"] = dt.Rows.Count;
                drBindData["USER"] = dt.Rows[0]["INSUSER"].ToString();
                drBindData["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = dt.Rows.Count;


                drBindData["BOXID1"] = dt.Rows[0]["LOTID"].ToString();
                drBindData["BOXLOTCNT1"] = dt.Rows[0]["HOLD_FLAG"].ToString();
                i_GBT = Convert.ToInt32(dt.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(dt.Rows[0]["MBOM_CNT"].ToString());

                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[prefixBoxID + index.ToString()] = dt.Rows[i]["LOTID"].ToString();
                        drBindData[prefixBoxLOTCount + index.ToString()] = dt.Rows[i]["HOLD_FLAG"].ToString();
                        i_GBT = Convert.ToInt32(dt.Rows[i]["GBT_CNT"].ToString());
                        i_MBOM = Convert.ToInt32(dt.Rows[i]["MBOM_CNT"].ToString());

                        if (i_GBT > 0)
                        {
                            if (i_GBT == i_MBOM)
                            {
                                drBindData[prefixGBTID + index.ToString()] = "GB/T";
                            }
                        }
                        index++;
                    }
                }

                dtReturn.Rows.Add(drBindData);
                dtReturn.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 라벨 데이터 만들기
        private C1Report ReportDataBinding(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }

            C1Report c1Report = new C1Report();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string reportName = "Pallet_Tag_MEBCMA";
            string reportFileName = "LGC.GMES.MES.PACK001.Report.Pallet_Tag_MEBCMA.xml";

            c1Report.Layout.PaperSize = PaperKind.A4;
            using (Stream stream = assembly.GetManifestResourceStream(reportFileName))
            {
                c1Report.Load(stream, reportName);
            }

            // Report Item in Bind Data
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string columnName = dt.Columns[i].ColumnName;
                if (c1Report.Fields.Contains(columnName))
                {
                    c1Report.Fields[columnName].Text = dt.Rows[0][columnName] == null ? "" : dt.Rows[0][columnName].ToString();
                }
            }

            // 하양색 바탕에 검은색 글자 색깔로 단표시 박스 깔고, BOXSEQ에 해당하는 박스만 음영처리
            for (int i = 1; i <= 7; i++)
            {
                string columnName = "txtRow" + i.ToString();
                c1Report.Fields[columnName].BackColor = Colors.White;
                c1Report.Fields[columnName].ForeColor = Colors.Black;
            }

            foreach (var item in dt.AsEnumerable().Take(1))
            {
                if (!string.IsNullOrEmpty(item.Field<string>("BOXSEQ")))
                {
                    string reverseColorColumnName = "txtRow" + item.Field<string>("BOXSEQ");
                    c1Report.Fields[reverseColorColumnName].BackColor = Colors.Black;
                    c1Report.Fields[reverseColorColumnName].ForeColor = Colors.White;
                }
            }

            // 트럭공정 라벨의 경우 박스ID 제목 옆에 수량 제목표시를 Hold로 변경
            c1Report.Fields["LabelHoldOrQty1"].Text = "Hold";
            c1Report.Fields["LabelHoldOrQty2"].Text = "Hold";

            // 트럭공정 라벨 출력시 Hold가 Y인 경우에는 폰트를 굵게 표시해달라고 함.
            for (int i = 1; i < 30; i++)
            {
                string columnName = "BOXLOTCNT" + i.ToString();
                c1Report.Fields[columnName].Font.Bold = true;
            }

            // Language Binding : 다국어 처리
            for (int i = 0; i < c1Report.Fields.Count; i++)
            {
                if (c1Report.Fields[i].Text == null)
                {
                    continue;
                }

                if (c1Report.Fields[i].Name.ToUpper().Contains("LABEL"))
                {
                    string strResult = string.Empty;
                    switch (c1Report.Fields[i].Name.ToUpper())
                    {
                        case "LABEL1":
                            string[] temp = c1Report.Fields[i].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                            for (int j = 0; j < temp.Length; j++)
                            {
                                string item = temp[j];
                                strResult = strResult + ObjectDic.Instance.GetObjectName(item) + ";";
                            }
                            c1Report.Fields[i].Text = strResult.Substring(0, strResult.Length - 1).Replace(";", Environment.NewLine);
                            break;
                        default:
                            strResult = c1Report.Fields[i].Text;
                            c1Report.Fields[i].Text = ObjectDic.Instance.GetObjectName(strResult);
                            break;
                    }
                }

                c1Report.Fields[i].Font.Name = "돋움체";
            }

            return c1Report;
        }

        // 재구성 완료후 라벨 Print
        public void GroupLabelPrintBatch(List<string> lstGroupID)
        {
            if (lstGroupID == null || lstGroupID.Count <= 0)
            {
                return;
            }

            this.GroupLabelPrint(lstGroupID, true);
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnPreView_Click(object sender, RoutedEventArgs e)
        {
            this.PreviewProcess();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            this.PrintProcess();
        }

        private void txtGroupID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.SearchClipboardData();
                e.Handled = true;
            }
        }

        private void txtGroupID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchProcess();
            }
        }

        private void chkReprint_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void chkReprint_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void dgGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Declareations...
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            try
            {
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }

                DataRowView dataRowView = (DataRowView)c1DataGrid.Rows[dataGridCell.Row.Index].DataItem;
                if (!CommonVerify.HasTableRow(this.dtOutDataDetail))
                {
                    return;
                }

                string groupID = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "GR_ID")?.ToString();
                DataTable dtSelectedData = this.dtOutDataDetail.AsEnumerable().Where(x => x.Field<string>("GR_ID").Equals(groupID)).CopyToDataTable();
                Util.GridSetData(this.dgGrid2, dtSelectedData, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~PACK003_030() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class PACK003_030_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public PACK003_030_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 라벨 데이터 조회 (트럭공정대기에 있는 Module)
        public DataSet GetPalletLabelData(params object[] obj)
        {
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_SEL_LOGIS_GROUP_LABEL_DATA";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA_SUMMARY,OUTDATA_DETAIL";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));                 // (*)Language ID
                dtINDATA.Columns.Add("AREAID", typeof(string));                 // (*)Area ID
                dtINDATA.Columns.Add("GR_ID", typeof(string));                  // Group ID(빠레뜨 ID)
                dtINDATA.Columns.Add("FROM_GR_SET_DTTM", typeof(string));       // Group ID 생성일자(From)
                dtINDATA.Columns.Add("TO_GR_SET_DTTM", typeof(string));         // Group ID 생성일자(To)
                dtINDATA.Columns.Add("LOGIS_PACK_TYPE", typeof(string));        // 물류포장유형(TRU: 트럭, CHG: 충방전)
                dtINDATA.Columns.Add("LABEL_PRT_FLAG", typeof(string));         // 라벨발행여부(Default: 없음)
                dtINDATA.Columns.Add("PACK_EQPTID", typeof(string));            // 포장기 ID

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = string.IsNullOrEmpty(obj[0].ToString()) ? null : obj[0].ToString();                                            // (*)Language ID
                drINDATA["AREAID"] = string.IsNullOrEmpty(obj[1].ToString()) ? null : obj[1].ToString();                                            // (*)Area ID
                drINDATA["GR_ID"] = string.IsNullOrEmpty(obj[2].ToString()) ? null : obj[2].ToString();                                             // Group ID(빠레뜨 ID)
                drINDATA["FROM_GR_SET_DTTM"] = string.IsNullOrEmpty(obj[3].ToString()) ? null : obj[3].ToString();                                  // Group ID 생성일자(From)
                drINDATA["TO_GR_SET_DTTM"] = string.IsNullOrEmpty(obj[4].ToString()) ? null : obj[4].ToString();                                    // Group ID 생성일자(To)
                drINDATA["LOGIS_PACK_TYPE"] = string.IsNullOrEmpty(obj[5].ToString()) ? null : obj[5].ToString();                                   // 물류포장유형(TRU: 트럭, CHG: 충방전)
                drINDATA["LABEL_PRT_FLAG"] = string.IsNullOrEmpty(obj[6].ToString()) ? null : obj[6].ToString();                                    // 라벨발행여부(Default: 없음)
                drINDATA["PACK_EQPTID"] = (string.IsNullOrEmpty(obj[7].ToString()) || obj[7].ToString().Equals("ALL")) ? null : obj[7].ToString();  // 포장기 ID
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (dsOUTDATA == null)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }

        // 순서도 호출 - 포장기
        public DataTable GetPackEquipmentInfo()
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtReturn;
            }

            return dtReturn;
        }

        // 순서도 호출 - 라벨 발행 여부 Update
        public void SetGroupBoxPrintYN(string groupID)
        {
            try
            {
                string bizRuleName = "DA_PRD_UPD_TB_SFC_LOGIS_MOD_GR";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("GR_ID", typeof(string));              // 그룹 ID
                dtRQSTDT.Columns.Add("LOAD_GR_ID", typeof(string));         // 적재 그룹 ID
                dtRQSTDT.Columns.Add("LOAD_SEQ", typeof(string));           // 적재 순번
                dtRQSTDT.Columns.Add("LOGIS_PACK_TYPE", typeof(string));    // 물류 포장 유형
                dtRQSTDT.Columns.Add("CSTID", typeof(string));              // 캐리어 ID
                dtRQSTDT.Columns.Add("GR_SET_DTTM", typeof(string));        // 그룹 설정 시간
                dtRQSTDT.Columns.Add("GR_CNCL_DTTM", typeof(string));       // 그룹 해제 시간
                dtRQSTDT.Columns.Add("NOTE", typeof(string));               // 비고
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));           // 사용여부
                dtRQSTDT.Columns.Add("LABEL_PRT_FLAG", typeof(string));     // 라벨 발행 여부
                dtRQSTDT.Columns.Add("USERID", typeof(string));             // 사용자 ID
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));             // 포장기 ID

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["GR_ID"] = groupID;                                // 그룹 ID
                drRQSTDT["LOAD_GR_ID"] = null;                              // 적재 그룹 ID
                drRQSTDT["LOAD_SEQ"] = null;                                // 적재 순번
                drRQSTDT["LOGIS_PACK_TYPE"] = null;                         // 물류 포장 유형
                drRQSTDT["CSTID"] = null;                                   // 캐리어 ID
                drRQSTDT["GR_SET_DTTM"] = null;                             // 그룹 설정 시간
                drRQSTDT["GR_CNCL_DTTM"] = null;                            // 그룹 해제 시간
                drRQSTDT["NOTE"] = null;                                    // 비고
                drRQSTDT["USE_FLAG"] = null;                                // 사용여부
                drRQSTDT["LABEL_PRT_FLAG"] = "Y";                           // 라벨 발행 여부
                drRQSTDT["USERID"] = LoginInfo.USERID;                      // 사용자 ID
                drRQSTDT["EQPTID"] = null;                                  // 포장기 ID
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}