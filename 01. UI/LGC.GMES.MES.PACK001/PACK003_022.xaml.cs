/************************************************************************************
  Created Date : 2021.08.12
       Creator : 정용석
   Description : 포장기 Pallet 라벨 발행
 ------------------------------------------------------------------------------------
  [Change History]
    2021.08.12  정용석 : Initial Created.
    2022.01.26  김길용 : Pack2동 Preview 기능 활성화(Pack3동은 비활성)
    2025.04.28  윤주일 : CAT_UP_0454 (GMES PACK 자동화 물류 UI 및 기능 및 MICA 공정 신규 로직)
 ************************************************************************************/
using C1.C1Preview;
using C1.C1Report;
using C1.WPF.C1Report;
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
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_022 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        string currentPackEQPTID = string.Empty;
        string CurrentPalletID = string.Empty;
        private List<C1Report> lstC1Report = new List<C1Report>();
        C1Report targetReport = new C1Report();
        C1DocumentViewer c1DocumentViewerLocal = new C1DocumentViewer();
        bool previewFlag = false;
        private PACK003_022_DataHelper dataHelper = new PACK003_022_DataHelper();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Declaration & Constructor
        public PACK003_022()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            if (LoginInfo.CFG_AREA_ID == "PA") this.btnPreView.Visibility = Visibility.Collapsed;
            PackCommon.SetC1ComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboPackEquipmentID);
            this.btnPreView.IsEnabled = false;
            this.btnPrint.IsEnabled = false;
            this.IsPreviewClick(false);
        }

        // 조회
        private void SearchProcess()
        {
            this.btnPreView.IsEnabled = false;
            this.btnPrint.IsEnabled = false;
            this.IsPreviewClick(false);
            this.ClearProcess();
            PackCommon.SearchRowCount(ref this.txtBoxSeqCount, 0);
            PackCommon.SearchRowCount(ref this.txtLOTCount, 0);
            Util.gridClear(this.dgGrid1);
            Util.gridClear(this.dgGrid2);

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                string packEquipmentID = this.cboPackEquipmentID.SelectedValue.ToString();
                string palletID = string.Empty;
                this.CurrentPalletID = string.Empty;
                string LOTID = string.Empty;
                DataSet ds = this.dataHelper.GetPalletLabelData(packEquipmentID, palletID, LOTID);
                if (CommonVerify.HasTableInDataSet(ds))
                {
                    this.currentPackEQPTID = packEquipmentID;
                    
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_SUM"]))
                    {
                        PackCommon.SearchRowCount(ref this.txtBoxSeqCount, ds.Tables["OUTDATA_SUM"].Rows.Count);
                        Util.GridSetData(this.dgGrid1, ds.Tables["OUTDATA_SUM"], FrameOperation);
                        this.btnPreView.IsEnabled = true;
                        this.btnPrint.IsEnabled = true;
                        this.CurrentPalletID = ds.Tables["OUTDATA_SUM"].Rows[0]["BOXID"].ToString();
                    }

                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_DETAIL"]))
                    {
                        PackCommon.SearchRowCount(ref this.txtLOTCount, ds.Tables["OUTDATA_DETAIL"].Rows.Count);
                        Util.GridSetData(this.dgGrid2, ds.Tables["OUTDATA_DETAIL"], FrameOperation);
                        this.btnPreView.IsEnabled = true;
                        this.btnPrint.IsEnabled = true;
                    }
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 라벨 Print
        private void PrintProcess()
        {
            if (this.dgGrid1 == null || this.dgGrid1.ItemsSource == null || this.dgGrid1.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return;
            }

            // 출력했는지 체크
            string packEquipmentID = this.currentPackEQPTID;
            string palletID = string.Empty;
            string LOTID = string.Empty;
            DataSet ds = this.dataHelper.GetPalletLabelData(packEquipmentID, palletID, LOTID);
            if (CommonVerify.HasTableInDataSet(ds))
            {
                if (!CommonVerify.HasTableRow(ds.Tables["OUTDATA_SUM"]))
                {
                    Util.Alert("9059");     // 데이터를 조회 하십시오.
                    return;
                }
            }


            if (this.targetReport != null && this.targetReport.C1Document.Body.Children.Count > 0)
            {
                this.c1DocumentViewerLocal.Document = this.targetReport.FixedDocumentSequence;
                this.c1DocumentViewerLocal.Print();

                if (!string.IsNullOrEmpty(this.currentPackEQPTID))
                {
                    this.dataHelper.SetBoxPrintYN(currentPackEQPTID);
                }
                return;
            }

            // 그리드에 데이터가 있으면 Print
            List<string> lstBoxID = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BOXID")).ToList();
            //// For Test
            //List<string> lstBoxID = new List<string>();
            //lstBoxID.Add("PP8Q03UJ23MD001");
            //lstBoxID.Add("PP8Q03UJ23MD002");
            //lstBoxID.Add("PP8Q03UJ23MD003");
            //lstBoxID.Add("PP8Q03UJ23MD004");
            //lstBoxID.Add("PP8Q03UJ23MD005");
            //lstBoxID.Add("PP8Q03UJ23MD006");
            //lstBoxID.Add("PP8Q03UJ23MD007");

            this.MakeReport(lstBoxID);
            if (this.targetReport != null && this.targetReport.C1Document.Body.Children.Count > 0)
            {
                this.c1DocumentViewerLocal.Document = this.targetReport.FixedDocumentSequence;
                this.c1DocumentViewerLocal.Print();

                if (!string.IsNullOrEmpty(this.currentPackEQPTID))
                {
                    this.dataHelper.SetBoxPrintYN(currentPackEQPTID);
                }
            }
        }

        private void ClearProcess()
        {
            this.c1DocumentViewerLocal.Document = null;
            this.c1DocumentViewer1.Document = null;
            this.lstC1Report.Clear();
            //this.targetReport = new C1Report();
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

            List<string> lstBoxID = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BOXID")).ToList();
            if (lstBoxID.Count <= 0)
            {
                return;
            }

            //// For Test
            //List<string> lstBoxID = new List<string>();
            //lstBoxID.Add("PP8Q03UJ23MD001");
            //lstBoxID.Add("PP8Q03UJ23MD002");
            //lstBoxID.Add("PP8Q03UJ23MD003");
            //lstBoxID.Add("PP8Q03UJ23MD004");
            //lstBoxID.Add("PP8Q03UJ23MD005");
            //lstBoxID.Add("PP8Q03UJ23MD006");
            //lstBoxID.Add("PP8Q03UJ23MD007");

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            this.MakeReport(lstBoxID);
            if (this.targetReport != null && this.targetReport.C1Document.Body.Children.Count > 0)
            {
                this.c1DocumentViewer1.Document = this.targetReport.FixedDocumentSequence;
            }
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 라벨 데이터 만들기
        private void MakeReport(List<string> lstPalletID)
        {
            DataSet dsPallet = new DataSet();
            DataSet dsReportBinding = new DataSet();

            if (LoginInfo.CFG_AREA_ID == "P8")
            {
                // Data 가져오기
                foreach (string palletID in lstPalletID)
                {
                    DataTable dt = this.dataHelper.GetPalletLabelInfo(palletID, "MEBCMA");
                    dt.TableName = palletID;
                    dsPallet.Tables.Add(dt);
                }
            }
            else
            {
                // Data 가져오기
                foreach (string palletID in lstPalletID)
                {
                    DataTable dt = this.dataHelper.GetPalletLabelInfo_v2(palletID, "CMAEV2020");
                    dt.TableName = palletID;
                    dsPallet.Tables.Add(dt);
                }
            }

            if (LoginInfo.CFG_AREA_ID == "P8")
            {
                // 라벨 데이터 만들기
                foreach (DataTable dt in dsPallet.Tables.OfType<DataTable>())
                {
                    dsReportBinding.Tables.Add(this.SetBindData(dt));
                }
            }
            else
            {
                // 라벨 데이터 만들기
                foreach (DataTable dt in dsPallet.Tables.OfType<DataTable>())
                {
                    dsReportBinding.Tables.Add(this.SetBindData_CMAEV2020(dt));
                }
            }
            
            // 라벨 데이터와 Report Binding
            foreach (DataTable dtReportBinding in dsReportBinding.Tables.OfType<DataTable>())
            {
                C1Report report = this.setReport(dtReportBinding);
                this.lstC1Report.Add(report);
            }

            // Report 데이터를 가지고 작업하기
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
        private DataTable SetBindData(DataTable dt)
        {
            DataTable dtReturn = new DataTable();
            dtReturn.TableName = dt.TableName;
            dtReturn.Columns.Add("BARCODE", typeof(string));
            dtReturn.Columns.Add("PALLETID", typeof(string));
            dtReturn.Columns.Add("DATE", typeof(string));
            dtReturn.Columns.Add("BOXSEQ", typeof(string)); //포장 단수
            dtReturn.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
            dtReturn.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
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
                    dtReturn.Columns.Add(prefixBoxLOTCount + j.ToString(), typeof(string));
                    dtReturn.Columns.Add(prefixGBTID + j, typeof(string));
                }


                DataRow drBindData = dtReturn.NewRow();
                drBindData["BARCODE"] = dt.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = dt.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = dt.Rows[0]["DATE"].ToString();
                drBindData["BOXSEQ"] = dt.Rows[0]["BOXSEQ"].ToString();
                drBindData["BOXCNT"] = dt.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(dt.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = dt.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = Convert.ToInt32(dt.Rows[0]["PALLETCNT"]).ToString();

                drBindData["BOXID1"] = dt.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(dt.Rows[0]["BOXCNT"]).ToString();
                i_GBT = Convert.ToInt32(dt.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(dt.Rows[0]["MBOM_CNT"].ToString());

                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[prefixBoxID + index.ToString()] = dt.Rows[i]["BOXID"].ToString();
                        drBindData[prefixBoxLOTCount + index.ToString()] = Convert.ToInt32(dt.Rows[i]["BOXCNT"]).ToString();
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
        private DataTable SetBindData_CMAEV2020(DataTable dt)
        {
            DataTable dtReturn = new DataTable();
            dtReturn.TableName = dt.TableName;
            dtReturn.Columns.Add("BARCODE", typeof(string));
            dtReturn.Columns.Add("CARRIERID", typeof(string));
            dtReturn.Columns.Add("PALLETID", typeof(string));
            dtReturn.Columns.Add("DATE", typeof(string));
            dtReturn.Columns.Add("BOXSEQ", typeof(string)); //포장 단수
            dtReturn.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
            dtReturn.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
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

                for (int j = 1; j <= boxcnt; j++)
                {
                    dtReturn.Columns.Add(prefixBoxID + j.ToString(), typeof(string));
                    dtReturn.Columns.Add(prefixBoxLOTCount + j.ToString(), typeof(string));
                }


                DataRow drBindData = dtReturn.NewRow();
                drBindData["BARCODE"] = dt.Rows[0]["PALLETID"].ToString();
                drBindData["CARRIERID"] = dt.Rows[0]["CARRIERID"].ToString();
                drBindData["PALLETID"] = dt.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = dt.Rows[0]["DATE"].ToString();
                drBindData["BOXSEQ"] = dt.Rows[0]["BOXSEQ"].ToString();
                drBindData["BOXCNT"] = dt.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(dt.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = dt.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = dt.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = Convert.ToInt32(dt.Rows[0]["PALLETCNT"]).ToString();

                drBindData["BOXID1"] = dt.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(dt.Rows[0]["BOXCNT"]).ToString();

                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[prefixBoxID + index.ToString()] = dt.Rows[i]["BOXID"].ToString();
                        drBindData[prefixBoxLOTCount + index.ToString()] = Convert.ToInt32(dt.Rows[i]["BOXCNT"]).ToString();
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
        private C1Report setReport(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }
            C1Report c1Report = new C1Report();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string reportName = string.Empty;
            string reportFileName = string.Empty;
            if (LoginInfo.CFG_AREA_ID =="P8")
            {
                reportName = "Pallet_Tag_MEBCMA";
                reportFileName = "LGC.GMES.MES.PACK001.Report.Pallet_Tag_MEBCMA.xml";
            }
            else
            {
                reportName = "Pallet_Tag_CMAEV2020";
                reportFileName = "LGC.GMES.MES.PACK001.Report.Pallet_Tag_CMAEV2020.xml";
            }

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
            if (LoginInfo.CFG_AREA_ID == "P8")
            {
                // 2021-07-26 MEBCMA 7단포장위치 음영표시
                // 하양색 바탕에 검은색 글자 색깔로 단표시 박스 깔고, BOXSEQ에 해당하는 박스만 음영처리
                for (int i = 1; i <= 7; i++)
                {
                    string columnName = "txtRow" + i.ToString();
                    c1Report.Fields[columnName].BackColor = Colors.White;
                    c1Report.Fields[columnName].ForeColor = Colors.Black;
                }

                // select top 1 * from dtBindData;
                foreach (var item in dt.AsEnumerable().Take(1))
                {
                    if (!string.IsNullOrEmpty(item.Field<string>("BOXSEQ")))
                    {
                        string reverseColorColumnName = "txtRow" + item.Field<string>("BOXSEQ");
                        c1Report.Fields[reverseColorColumnName].BackColor = Colors.Black;
                        c1Report.Fields[reverseColorColumnName].ForeColor = Colors.White;
                    }
                }
            }
            // Language Binding : 다국어 처리
            for (int idx = 0; idx < c1Report.Fields.Count; idx++)
            {
                if (c1Report.Fields[idx].Text == null)
                {
                    continue;
                }

                if (c1Report.Fields[idx].Name.ToUpper().Contains("LABEL"))
                {
                    string strResult = string.Empty;
                    switch (c1Report.Fields[idx].Name.ToUpper())
                    {
                        case "LABEL1":
                            string[] temp = c1Report.Fields[idx].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                            for (int jdx = 0; jdx < temp.Length; jdx++)
                            {
                                string item = temp[jdx];
                                strResult = strResult + ObjectDic.Instance.GetObjectName(item) + ";";
                            }
                            c1Report.Fields[idx].Text = strResult.Substring(0, strResult.Length - 1).Replace(";", Environment.NewLine);
                            break;
                        default:
                            strResult = c1Report.Fields[idx].Text;
                            c1Report.Fields[idx].Text = ObjectDic.Instance.GetObjectName(strResult);
                            break;
                    }
                }
                c1Report.Fields[idx].Font.Name = "돋움체";
            }

            return c1Report;
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
            if(LoginInfo.CFG_AREA_ID == "P8")
            {
                this.PrintProcess();
            }
            else
            {
                setTagReport_PA();
                if (!string.IsNullOrEmpty(this.currentPackEQPTID))
                {
                    this.dataHelper.SetBoxPrintYN(currentPackEQPTID);
                }
            }
            
        }
        private void setTagReport_PA()
        {
            try
            {
                LGC.GMES.MES.PACK001.Pallet_CST_Tag rs = new LGC.GMES.MES.PACK001.Pallet_CST_Tag();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "Pallet_Tag";
                    Parameters[1] = CurrentPalletID;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(printPopUp_PA_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.Show()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void printPopUp_PA_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Pallet_CST_Tag printPopUp = sender as LGC.GMES.MES.PACK001.Pallet_CST_Tag;
                if (Convert.ToBoolean(printPopUp.DialogResult))
                {

                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }
        #endregion
    }
    

    public class PACK003_022_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public PACK003_022_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // BizRule 호출 - Pallet Label 데이터 가져오기
        public DataTable GetPalletLabelInfo(string palletID, string reportName)
        {
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_GET_PLT_TAG";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            //string outDataSetName = "CMA,X09CMA,PORSCHE12V,MBW12V,FORD48V,C727EOL,CMAEV,MEBCMA,BT6,ISUZU"; //20250428
            string outDataSetName = "CMA,X09CMA,PORSCHE12V,MBW12V,FORD48V,C727EOL,CMAEV,MEBCMA,BT6,ISUZU,ST";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("PALLETID", typeof(string));
                dtINDATA.Columns.Add("PLT_TYPE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["PALLETID"] = palletID;
                drINDATA["PLT_TYPE"] = reportName;
                dtINDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (dsOUTDATA == null)
                {
                    return null;
                }

                var query = dsOUTDATA.Tables.OfType<DataTable>().Where(x => x.TableName.Equals(reportName));
                foreach (DataTable dt in query)
                {
                    dtReturn = dt.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }
        public DataTable GetPalletLabelInfo_v2(string palletID, string reportName)
        {


            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_GET_PLT_TAG_V2";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("PALLETID", typeof(string));
                dtINDATA.Columns.Add("PLT_TYPE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["PALLETID"] = palletID;
                drINDATA["PLT_TYPE"] = reportName;
                dtINDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (dsOUTDATA == null)
                {
                    return null;
                }

                var query = dsOUTDATA.Tables.OfType<DataTable>().Where(x => x.TableName.Equals(reportName));
                foreach (DataTable dt in query)
                {
                    dtReturn = dt.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;

        }


        // BizRule 호출 - 포장기
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

        // BizRule 호출 - 라벨 데이터 조회
        public DataSet GetPalletLabelData(string packEquipmentID, string palletID, string LOTID)
        {
            string bizRuleName = "BR_PRD_SEL_BOX_GROUP_INFO_FOR_PRINT";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA_SUM,OUTDATA_DETAIL";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PACK_EQPTID", typeof(string));
                dtINDATA.Columns.Add("BOXID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["PACK_EQPTID"] = string.IsNullOrEmpty(packEquipmentID) ? null : packEquipmentID;
                drINDATA["BOXID"] = string.IsNullOrEmpty(palletID) ? String.Empty : palletID;
                drINDATA["LOTID"] = string.IsNullOrEmpty(LOTID) ? string.Empty : LOTID;
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                if (!CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

            return dsOUTDATA;
        }

        // BizRule 호출 - 라벨 발행 여부 Update
        public void SetBoxPrintYN(string packEquipmentID)
        {
            try
            {
                string bizRuleName = "DA_PRD_UPD_BOX_PRINTYN";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("PRINTYN", typeof(string));
                dtRQSTDT.Columns.Add("LOGIS_BOX_GR_ID", typeof(string));
                dtRQSTDT.Columns.Add("UPDUSER", typeof(string));
                dtRQSTDT.Columns.Add("UPDDTTM", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["PACK_EQPTID"] = packEquipmentID;
                drRQSTDT["PRINTYN"] = "Y";
                drRQSTDT["LOGIS_BOX_GR_ID"] = DBNull.Value;
                drRQSTDT["UPDUSER"] = LoginInfo.USERID;
                drRQSTDT["UPDDTTM"] = DateTime.Now;
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