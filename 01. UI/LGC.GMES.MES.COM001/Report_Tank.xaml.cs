/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.21  김대현      라벨 출력후 LOTATTR의 VLD_DATE 컬럼 업데이트 추가
 





**************************************************************************************/

using C1.C1Preview;
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
    public partial class Report_Tank : C1Window, IWorkArea
    {
        #region Declaration & Constructor   

        C1.C1Report.C1Report cr1 = null;
        C1.C1Report.C1Report cr2 = null;
        C1.C1Report.C1Report cr3 = null;

        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;

        int iMAX_PAGE = 0;
        int _iCount = 0;
        //int _iMaxRow = 3;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_Tank()
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

                    pm.Print(ps, ps.DefaultPageSettings);

                }


                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("IN_LOT");
                dt.Columns.Add("LOTID", typeof(string));
                //2023.02.21 김대현
                dt.Columns.Add("VALID_DATE", typeof(string));


                DataRow row = dt.NewRow();
                row["LOTID"] = Convert.ToString(tmmp02.Rows[0]["TANK"]);
                //2023.02.21 김대현
                row["VALID_DATE"] = Convert.ToString(tmmp02.Rows[0]["VALID_DATE"]).Replace("-", string.Empty);
                dt.Rows.Add(row);


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LABEL_PRNT_FLAG_MX", "IN_LOT", null, ds);

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
                cr2 = new C1.C1Report.C1Report();
                cr3 = new C1.C1Report.C1Report();

                cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr3.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                string filename = string.Empty;
                string reportname = string.Empty;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.COM001.Report." + filename))
                {
                    cr1.Load(stream, reportname);
                }

                _iCount = tmmp02.Rows.Count;

                for (int i = 0; i < _iCount; i++)
                {
                    if (i == 1)
                    {
                        cr2.CopyFrom(cr1);
                    }
                    else if (i == 2 )
                    {
                        cr3.CopyFrom(cr1);
                    }
                }

                for (int row = 0; row < tmmp02.Rows.Count; row++)
                {
                    if (row == 0)
                    {
                        for (int col = 0; col < tmmp02.Columns.Count; col++)
                        {
                            string strColName = tmmp02.Columns[col].ColumnName;
                            string strFieldName = tmmp02.Columns[col].ColumnName;
                            if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();

                        }
                    }
                    else if (row == 1)
                    {
                        for (int col = 0; col < tmmp02.Columns.Count; col++)
                        {
                            string strColName = tmmp02.Columns[col].ColumnName;
                            string strFieldName = tmmp02.Columns[col].ColumnName;
                            if (cr2.Fields.Contains(strFieldName)) cr2.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();

                        }
                    }
                    else if (row == 2)
                    {
                        for (int col = 0; col < tmmp02.Columns.Count; col++)
                        {
                            string strColName = tmmp02.Columns[col].ColumnName;
                            string strFieldName = tmmp02.Columns[col].ColumnName;
                            if (cr3.Fields.Contains(strFieldName)) cr3.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();

                        }
                    }
                }

                cr1.Render();

                for (int i = 0; i < _iCount; i++)
                {
                    if (i == 1)
                    {
                        cr2.Render();
                    }
                    else if (i == 2)
                    {
                        cr3.Render();
                    }
                }

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
            }
        }






    }
}
