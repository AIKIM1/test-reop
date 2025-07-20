/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class Report_Plant_Move_Return : C1Window, IWorkArea
    {
        #region Declaration & Constructor   

        C1.C1Report.C1Report cr1 = null;
        C1.C1Report.C1Report cr2 = null;

        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;
        DataTable tmmp03;

        int iMAX_PAGE = 0;
        int _iCount = 0;
        int _iMaxRow = 37;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_Plant_Move_Return()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable;
            tmmp03 = tmps[2] as DataTable;

            this.Loaded -= Window_Loaded;

            txtPage.Text = "1";
            txtPrintQty.Value = 1;

            Open();
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

                cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;                

                string filename = string.Empty;
                string reportname = string.Empty;


                reportname = tmmp01;
                //reportname = "Report_SKID";
                filename = tmmp01 + ".xml";

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.COM001.Report." + filename))
                {
                    cr1.Load(stream, reportname);
                }

                for (int row = 0; row < tmmp02.Rows.Count; row++)
                {
                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        string strFieldName = tmmp02.Columns[col].ColumnName;

                        if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();
                    }
                }

                for (int row = 0; row < tmmp03.Rows.Count; row++)
                {
                    for (int col = 0; col < tmmp03.Columns.Count; col++)
                    {
                        string strColName = tmmp03.Columns[col].ColumnName;
                        string strFieldName = tmmp03.Columns[col].ColumnName + row;

                        if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp03.Rows[row][strColName].ToString();

                    }
                }

                c1DocumentViewer.Document = cr1.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
    }
}
