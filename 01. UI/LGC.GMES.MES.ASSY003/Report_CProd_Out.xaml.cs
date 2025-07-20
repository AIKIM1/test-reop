/*************************************************************************************
 Created Date : 2017.12.07
      Creator : CNS 고현영S
   Decription : C생산 Folding BOX 출고시 인쇄
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.06  CNS 고현영S : Initial Created.
**************************************************************************************/

using C1.C1Preview;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;

namespace LGC.GMES.MES.ASSY003
{
    public partial class Report_CProd_Out : C1Window, IWorkArea
    {
        #region Declaration & Constructor   

        C1.C1Report.C1Report cr1 = null;
        C1.C1Report.C1Report cr2 = null;
        //C1.C1Report.C1Report cr3 = null;

        object[] tmps;
        string _reportName = string.Empty;
        DataSet _dataSet = null;
        string _MoveID = string.Empty;

        int iMAX_PAGE = 0;
        int _iCount = 0;
        int _iMaxRow = 15;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_CProd_Out()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);
            _reportName = Util.NVC(tmps[0]);
            _dataSet = tmps[1] as DataSet;

            //_dtTmp = (tmps[1] as DataTable);
            //_Header = (tmps[2] as DataTable);

            this.Loaded -= Window_Loaded;

            txtPage.Text = "1";
            txtPrintQty.Value = 1;

            Open();
        }

        private void SetData()
        {
            
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

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
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
                //cr3 = new C1.C1Report.C1Report();

                cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                //cr3.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                string fileName = _reportName + ".xml";

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.ASSY003.Report." + fileName))
                {
                    cr1.Load(stream, _reportName);

                    // 다국어 처리 및 CLEAR
                    for (int cnt = 0; cnt < cr1.Fields.Count; cnt++)
                    {
                        // Title
                        if (cr1.Fields[cnt].Name.IndexOf("txtTitle", StringComparison.Ordinal) > -1)
                        {
                            cr1.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr1.Fields[cnt].Text));
                        }
                    }


                    // 1. Header Data
                    DataTable dt1 = _dataSet.Tables["OUT_HEADER"];
                    for (int idx = 0; idx < dt1.Columns.Count; idx++)
                    {
                        string strColName = dt1.Columns[idx].ColumnName;

                        double dValue = 0;

                        if (strColName.Equals("SUMQTY"))
                        {
                            if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                            {
                                if (dValue > 0)
                                    cr1.Fields[strColName].Text = " " + dValue.ToString("N0");
                            }
                        }
                        else
                        {
                            if (cr1.Fields.Contains(strColName))
                            {
                                cr1.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                            }

                            if (cr1.Fields.Contains(strColName + "_BCD"))
                            {
                                cr1.Fields[strColName + "_BCD"].Text = dt1.Rows[0][strColName].ToString();
                            }
                        }
                    }

                    // 2 DATA LIST
                    DataTable dt2 = _dataSet.Tables["OUT_DATA"];
                    for (int col = 0; col < dt2.Columns.Count; col++)
                    {
                        string strColName = dt2.Columns[col].ColumnName;

                        //for (int row = 0; row < dt2.Rows.Count; row++)
                        for (int row = 0; row < 15; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr1.Fields.Contains(strColName + strRowName))
                            {
                                cr1.Fields[strColName + strRowName].Text = "";

                                if (row < dt2.Rows.Count)
                                {
                                    cr1.Fields[strColName + strRowName].Visible = true;

                                    double dValue = 0;

                                    if (strColName.Equals("WIPQTY"))
                                    {
                                        if (double.TryParse(Util.NVC(dt2.Rows[row][strColName]), out dValue))
                                        {
                                            if (dValue > 0)
                                                cr1.Fields[strColName + strRowName].Text = " " + dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr1.Fields[strColName + strRowName].Text = dt2.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }

                    //_iCount = tmmp03.Rows.Count;

                    //if (_iCount >= _iMaxRow) cr2.CopyFrom(cr1);

                    //for (int row = 0; row < tmmp02.Rows.Count; row++)
                    //{
                    //    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    //    {
                    //        string strColName = tmmp02.Columns[col].ColumnName;
                    //        string strFieldName = tmmp02.Columns[col].ColumnName;
                    //        if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();

                    //    }
                    //}

                    //if (_iCount >= _iMaxRow)
                    //{
                    //    for (int row = 0; row < tmmp02.Rows.Count; row++)
                    //    {
                    //        for (int col = 0; col < tmmp02.Columns.Count; col++)
                    //        {
                    //            string strColName = tmmp02.Columns[col].ColumnName;
                    //            string strFieldName = tmmp02.Columns[col].ColumnName;
                    //            if (cr2.Fields.Contains(strFieldName)) cr2.Fields[strFieldName].Text = tmmp02.Rows[row][strColName].ToString();
                    //        }
                    //    }
                    //}

                    //for (int row = 0; row < tmmp03.Rows.Count; row++)
                    //{
                    //    for (int col = 0; col < tmmp03.Columns.Count; col++)
                    //    {
                    //        string strColName = tmmp03.Columns[col].ColumnName;
                    //        string strFieldName = tmmp03.Columns[col].ColumnName + row;
                    //        //if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp03.Rows[row][strColName].ToString();
                    //        if (row < _iMaxRow)
                    //        {
                    //            if (cr1.Fields.Contains(strFieldName)) cr1.Fields[strFieldName].Text = tmmp03.Rows[row][strColName].ToString();
                    //        }
                    //        else
                    //        {
                    //            strFieldName = tmmp03.Columns[col].ColumnName + (row - _iMaxRow);
                    //            if (cr2.Fields.Contains(strFieldName)) cr2.Fields[strFieldName].Text = tmmp03.Rows[row][strColName].ToString();
                    //        }
                    //    }
                    //}

                    cr1.Render();
                    if (_iCount >= _iMaxRow) cr2.Render();
                    //if (_iCount >= _iMaxRow) cr3.Render();

                    cr1.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                    cr1.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                    cr1.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";


                    //iMAX_PAGE = cr1.GetPageCount() + cr2.GetPageCount() + cr3.GetPageCount();
                    iMAX_PAGE = cr1.GetPageCount() + cr2.GetPageCount();
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

                    //foreach (System.Drawing.Imaging.Metafile Page in this.cr3.GetPageImages())
                    //{
                    //    Pages.SetValue(Page, index);
                    //    index++;
                    //}

                    for (int i = 0; i <= (Pages.Length - 1); i++)
                    {
                        RenderImage PageImage = new RenderImage(Pages[i]);
                        cr1.C1Document.Body.Children.Add(PageImage);
                        cr1.C1Document.Reflow();
                    }
                }
                    c1DocumentViewer.Document = cr1.FixedDocumentSequence;

                    c1DocumentViewer.FitToWidth();
                    c1DocumentViewer.FitToHeight();
            }

            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }






    }
}
