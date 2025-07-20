/*************************************************************************************
 Created Date : 2017.12.06
      Creator : 
   Decription : 활성화 후공정 - 폴리머 Tag Print
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Windows;
using System.IO;
using System.Linq.Expressions;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_POLYMER_FORM_TEMP_TAG_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        C1.C1Report.C1Report cr = null;
        private string _SettingPrintName = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public CMM_POLYMER_FORM_TEMP_TAG_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Window_Loaded;

            SetControl();
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            /*
            if (LoginInfo.CFG_GENERAL_PRINTER != null
                && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0
                && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null
                && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
            {
                _SettingPrintName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
            }
            //else
            //{
            //    // Setting에서 기본 프린트 설정을 먼저 하세요.
            //    Util.MessageValidation("SFU4428");
            //    this.DialogResult = MessageBoxResult.Cancel;
            //}
            */

            PrintView();
        }

        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        #region 공통 : 미리보기
        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintView()
        {
            try
            {
                string reportNamePath;
                string reportName;

                #region Print 태그 설정
                //ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart.xml";
                //ReportName = "POLYMERFORM_GoodCart";

                string paperSize = string.Empty;
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
                {
                    paperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
                }

                if (string.IsNullOrWhiteSpace(paperSize) || paperSize.Equals("A4"))
                {
                    reportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart.xml";
                    reportName = "POLYMERFORM_GoodCart";
                }
                else
                {
                    reportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart_A5.xml";
                    reportName = "POLYMERFORM_GoodCart_A5";
                }

                #endregion

                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream(reportNamePath))
                {
                    cr.Load(stream, reportName);

                    // 다국어 처리 및 Clear
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        // Title
                        if (cr.Fields[cnt].Name.IndexOf("txTitle", StringComparison.Ordinal) > -1)
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
                c1DocumentViewer.FitToWidth();
                c1DocumentViewer.FitToHeight();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 출력
        /// </summary>
        private void TagPrint()
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
            pm.Print(ps, ps.DefaultPageSettings);

            //ps.PrinterName = _SettingPrintName;
            //pm.Print(ps);

            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion



    }
}
