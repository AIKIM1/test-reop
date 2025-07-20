/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

//using C1.C1Preview;
//using C1.WPF;
//using LGC.GMES.MES.CMM001.Class;
//using LGC.GMES.MES.Common;
//using LGC.GMES.MES.ControlsLibrary;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Windows;

using C1.C1Preview;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class Report_Gate_Tag : C1Window, IWorkArea
    {
        #region Declaration & Constructor   

        C1.C1Report.C1Report cr1 = null;
        C1.C1Report.C1Report cr2 = null;
        C1.C1Report.C1Report cr3 = null;
        DataTable dtSummary;
        DataTable dtDetail;
        object[] tmps;
        bool _bNew;
        string _sAPPRV_PASS_NO = string.Empty;
        string _sDetailReport = string.Empty;
        string _sMainReport = string.Empty;
        string _sORD_LIST;
        int iMAX_PAGE = 0;
        int _iCount = 0;
        int _iMaxRow = 25;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_Gate_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);
            _bNew = (bool)tmps[0];
            _sAPPRV_PASS_NO = tmps[1] as string;
            _sMainReport = tmps[2] as string;
            _sDetailReport = tmps[3] as string;
            _sORD_LIST = tmps[4] as string;
            this.Loaded -= Window_Loaded;

            txtPage.Text = "1";
            txtPrintQty.Value = 1;

            if (string.Equals(_sDetailReport, "Report_MTRL"))
                btnPdf.Visibility = Visibility.Visible;

            SearchData();
            Open();
        }      

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                c1DocumentViewer.NextPage();
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                c1DocumentViewer.PreviousPage();
            }
        }

        private void c1DocumentViewer_PageViewsChanged(object sender, EventArgs e)
        {
            if (c1DocumentViewer.PageViews.Count > 0)
                txtPage.Text = (c1DocumentViewer.PageViews[0].PageNumber + 1).ToString();
        }

        private void Open()
        {
            try
            {
                cr1 = new C1.C1Report.C1Report();
                cr2 = new C1.C1Report.C1Report();
                cr3 = new C1.C1Report.C1Report();

                cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr3.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                string mReportName = _sMainReport;
                string mFileName = _sMainReport + ".xml";

                string dReportName = _sDetailReport;
                string dFileName = _sDetailReport + ".xml";

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + mFileName))
                {
                    cr1.Load(stream, mReportName);
                }
                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + dFileName))
                {
                    cr2.Load(stream, dReportName);
                }

                if (_iCount >= _iMaxRow) cr3.CopyFrom(cr2);


                MappingData();

                cr1.Render();
                cr2.Render();
                if (_iCount >= _iMaxRow) cr3.Render();

                cr1.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                cr1.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                cr1.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";


                iMAX_PAGE = cr1.GetPageCount() + cr2.GetPageCount() + cr3.GetPageCount();
                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[iMAX_PAGE];
                int index = 0;

                // get pageimages of report1
                foreach (System.Drawing.Imaging.Metafile Page in cr1.GetPageImages())
                {
                    Pages.SetValue(Page, index);
                    index++;
                }

                // get pageimages of report2
                foreach (System.Drawing.Imaging.Metafile Page in this.cr2.GetPageImages())
                {
                    Pages.SetValue(Page, index);
                    index++;
                }

                foreach (System.Drawing.Imaging.Metafile Page in this.cr3.GetPageImages())
                {
                    Pages.SetValue(Page, index);
                    index++;
                }

                for (int i = 0; i <= (Pages.Length - 1); i++)
                {
                    RenderImage PageImage = new RenderImage(Pages[i]);
                    cr1.C1Document.Body.Children.Add(PageImage);
                    cr1.C1Document.Reflow();
                }

                c1DocumentViewer.Document = cr1.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void SearchData()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("APPRV_PASS_NO", typeof(string));
                dtRqst.Columns.Add("ORDID_LIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (_bNew) dr["ORDID_LIST"] = _sORD_LIST;
                else dr["APPRV_PASS_NO"] = _sAPPRV_PASS_NO;

                dtRqst.Rows.Add(dr);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPRV_PRINT_INFO", "INDATA", "SUMMARYDATA,DETAILDATA", inData);

                dtSummary = dsRslt.Tables["SUMMARYDATA"];
                dtDetail = dsRslt.Tables["DETAILDATA"];
                _iCount = dtDetail.Rows.Count;

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void MappingData()
        {
            cr1.Fields["P_ID1"].Text = DateTime.Today.ToShortDateString();

            if (_bNew)
            {
                cr1.Fields["P_ID3"].Text = RegisterPassNo();
            }
            else
            {
                cr1.Fields["P_ID3"].Text = _sAPPRV_PASS_NO;
            }

            for (int row = 0; row < dtSummary.Rows.Count; row++)
            {
                for (int col = 0; col < dtSummary.Columns.Count; col++)
                {
                    string strColName = dtSummary.Columns[col].ColumnName;
                    string strFieldName = dtSummary.Columns[col].ColumnName + row;
                    if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = dtSummary.Rows[row][strColName].ToString();
                }
            }

            for (int row = 0; row < dtDetail.Rows.Count; row++)
            {
                for (int col = 0; col < dtDetail.Columns.Count; col++)
                {
                    string strColName = dtDetail.Columns[col].ColumnName;
                    string strFieldName = dtDetail.Columns[col].ColumnName + row;
                    if (row < _iMaxRow)
                    {
                        if (cr2.Fields.Contains(strFieldName)) cr2.Fields[strFieldName].Text = dtDetail.Rows[row][strColName].ToString();
                    }
                    else
                    {
                        strFieldName = dtDetail.Columns[col].ColumnName + (row - _iMaxRow);
                        if (cr3.Fields.Contains(strFieldName)) cr3.Fields[strFieldName].Text = dtDetail.Rows[row][strColName].ToString();
                    }
                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (_bNew)
                //{
                //    cr1.Fields["P_ID3"].Text = RegisterPassNo();
                //    //cr2.Fields["Text40"].Text = "";

                //    cr1.Render();
                //    cr2.Render();
                //}


                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr1;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString(); 

                    pm.Print(ps, ps.DefaultPageSettings);
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private string RegisterPassNo()
        {
            string value = string.Empty;
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");                
                inTable.Columns.Add("USERID", typeof(String));
                inTable.Columns.Add("SHOPID", typeof(String));

                DataRow inRow = inTable.NewRow();                
                inRow["USERID"] = LoginInfo.USERID;
                inRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(inRow);

                DataTable ordTable = indataSet.Tables.Add("INRCV");
                ordTable.Columns.Add("RCV_ISS_ID", typeof(String));

                List<string> list = _sORD_LIST.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string id in list)
                {
                    DataRow ordRow = ordTable.NewRow();
                    ordRow["RCV_ISS_ID"] = id;
                    ordTable.Rows.Add(ordRow);
                }
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPRV_PASS_NO_FOR_SHIP", "INDATA,INRCV", "OUTDATA", indataSet);
                value = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["APPRV_PASS_NO"]);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_APPRV_PASS_NO_FOR_SHIP", "INDATA,INRCV", "OUTDATA", (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.AlertByBiz("BR_PRD_REG_APPRV_PASS_NO_FOR_SHIP", bizException.Message, bizException.ToString());
                //            return;
                //        }

                //        value = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["APPRV_PASS_NO"]);
                //    }
                //    catch (Exception ex)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        return;
                //    }

                //}, indataSet);

                return value;
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return value;
            }
        }

        private void btnPdfOut_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_sAPPRV_PASS_NO))
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                    saveFileDialog.Filter = "Pdf Files|*.pdf";
                    saveFileDialog.Title = "Save an Pdf File";
                    saveFileDialog.FileName = _sAPPRV_PASS_NO;

                    if (System.Configuration.ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                    {
                        saveFileDialog.InitialDirectory = @"\\Client\C$";
                    }

                    else
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        Logger.Instance.WriteLine("PDF LOCAL PATH", saveFileDialog.FileName);

                        C1.C1Preview.Export.PdfExporter ex = new C1.C1Preview.Export.PdfExporter();
                        ex.Document = cr1.Document;
                        ex.Export(saveFileDialog.FileName);
                    }
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }
    }
}
