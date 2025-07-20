/*************************************************************************************
 Created Date : 2021.03.26
      Creator : 조영대
   Decription : 인쇄 공통 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.26  조영대 : Initial Created.
**************************************************************************************/
using System;
using System.Windows;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_COMMON_PRINT : C1Window, IWorkArea
    {
        #region Declaration
        C1.C1Report.C1Report reportDesign = null;

        string targetFile = string.Empty;
        string targetName = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        DataTable dtReport = null;

        #endregion

        #region Initialize
        public CMM_COMMON_PRINT(DataTable reportData, string reportFile, string reportName)
        {
            InitializeComponent();

            dtReport = reportData;

            targetFile = reportFile;
            targetName = reportName;

            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            this.Loaded -= Window_Loaded;
            
            // 미리보기
            PrintView();
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GC.Collect();
        }
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            ReportPrint();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region SizeChanged
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();

        }
        #endregion

        #region User Method
        private void PrintView()
        {
            try
            {      
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[dtReport.Rows.Count];
                                
                int index = 0;
                C1.C1Report.C1Report reportPage = null;
                foreach (DataRow drPrint in dtReport.Rows)
                {
                    reportPage = new C1.C1Report.C1Report();

                    using (Stream stream = assembly.GetManifestResourceStream(targetFile))
                    {
                        reportPage.Load(stream, targetName);

                        if (reportDesign == null)
                        {
                            reportDesign = reportPage;                           
                        }

                        // 다국어 처리
                        for (int cnt = 0; cnt < reportPage.Fields.Count; cnt++)
                        {
                            if (reportPage.Fields[cnt].Name == null) continue;
                            if (reportPage.Fields[cnt].Name.Substring(0, 4).Equals("Text"))
                            {
                                reportPage.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(reportPage.Fields[cnt].Text));
                            }
                        }

                        for (int col = 0; col < dtReport.Columns.Count; col++)
                        {
                            string strColName = dtReport.Columns[col].ColumnName;

                            if (reportPage.Fields.Contains(strColName))
                            {
                                reportPage.Fields[strColName].Text = Util.NVC(drPrint[strColName]);
                            }
                        }
                    }
                    
                    reportPage.Render();
                    foreach (System.Drawing.Imaging.Metafile Page in reportPage.GetPageImages())
                    {
                        Pages.SetValue(Page, index);
                        index++;
                    }
                }

                reportDesign.C1Document.Body.Children.Clear();
                for (int i = 0; i <= (Pages.Length - 1); i++)
                {
                    C1.C1Preview.RenderImage PageImage = new C1.C1Preview.RenderImage(Pages[i]);
                    reportDesign.C1Document.Body.Children.Add(PageImage);
                    reportDesign.C1Document.Reflow();
                }

                c1DocumentViewer.Document = reportDesign.FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReportPrint()
        {
            try
            {
                var pm = new C1.C1Preview.C1PrintManager {Document = reportDesign};
                System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                pm.Print(ps, ps.DefaultPageSettings);
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

     
    }
}
