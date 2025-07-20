/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.08.05  조영대    : 출고 처리 분할 작업 - 멀티 Page View 추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using C1.C1Preview;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.BOX001
{
    public partial class Report_Packing_Tag : C1Window, IWorkArea
    {
        C1.C1Report.C1Report cr = null;
        C1.C1Report.C1Report cr2 = null;

        object[] arguments;
        string reportName = string.Empty;
        DataTable reportData;
        bool _bAddFlag = false;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_Packing_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Loaded -= Window_Loaded;

                arguments = C1WindowExtension.GetParameters(this);
                reportName = arguments[0] as string;
                _bAddFlag = Convert.ToBoolean(arguments[2]);

                // Print 수량 셋팅하기
                if (arguments.Length > 3)
                {
                    txtPrintQty.Value = Convert.ToInt32(arguments[3] as string);
                }
                else
                {
                    txtPrintQty.Value = 1;
                }

                if (arguments.Length > 4 && arguments[4].Equals("MultiPage"))
                {
                    cr = new C1.C1Report.C1Report();
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                    cr.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                    cr.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                    cr.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";

                    List<RenderImage> reportImages = arguments[1] as List<RenderImage>;
                    foreach (RenderImage image in reportImages)
                    {
                        cr.C1Document.Body.Children.Add(image);
                        cr.C1Document.Reflow();
                    }
                }
                else
                {
                    reportData = arguments[1] as DataTable; //dtPackingCard

                    cr = new C1.C1Report.C1Report();
                    cr2 = new C1.C1Report.C1Report();

                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                    cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    cr.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                    cr.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                    cr.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";

                    string fileName = reportName + ".xml";

                    System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                    using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + fileName))
                    {
                        cr.Load(stream, reportName);

                        for (int col = 0; col < reportData.Columns.Count; col++)
                        {
                            string strColName = reportData.Columns[col].ColumnName;
                            if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = reportData.Rows[0][strColName].ToString();
                        }
                    }

                    if (_bAddFlag)
                    {
                        using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + fileName))
                        {
                            cr2.Load(stream, reportName);
                        }

                        for (int col = 0; col < reportData.Columns.Count; col++)
                        {
                            string strColName = reportData.Columns[col].ColumnName;
                            if (cr2.Fields.Contains(strColName)) cr2.Fields[strColName].Text = reportData.Rows[1][strColName].ToString();
                        }
                    }

                    cr.Render();

                    System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[_bAddFlag ? 2 : 1];
                    int index = 0;

                    if (_bAddFlag) cr2.Render();

                    foreach (System.Drawing.Imaging.Metafile Page in cr.GetPageImages())
                    {
                        Pages.SetValue(Page, index);
                        index++;
                    }
                    if (_bAddFlag)
                    {
                        foreach (System.Drawing.Imaging.Metafile Page in this.cr2.GetPageImages())
                        {
                            Pages.SetValue(Page, index);
                            index++;
                        }
                    }

                    for (int i = 0; i <= (Pages.Length - 1); i++)
                    {
                        RenderImage PageImage = new RenderImage(Pages[i]);
                        cr.C1Document.Body.Children.Add(PageImage);
                        cr.C1Document.Reflow();
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();


                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                    pm.Print(ps);
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
                //    Util.MessageInfo("SFU1275");  //정상처리되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
