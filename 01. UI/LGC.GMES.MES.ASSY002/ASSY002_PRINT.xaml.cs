using System;
using System.Windows;
using System.Data;
using System.IO;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_PRINT : C1Window, IWorkArea
    {
        #region Declaration
        DataSet runcard = null;
        C1.C1Report.C1Report cr = null;

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
        public ASSY002_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            // Runcard 발행 DataSet
            runcard = tmps[0] as DataSet;

            this.Loaded -= Window_Loaded;

            // 미리보기
            PrintView();
        }
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
            pm.Print(ps, ps.DefaultPageSettings);

            this.Close();
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
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.ASSY002.Report.Winder_HistoryCard.xml"))
                {
                    cr.Load(stream, "Winder_HistoryCard");

                    // 임시 다국어 처리
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        if (cr.Fields[cnt].Name.ToString().IndexOf("Text") > -1)
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }
                    }

                    // 1.OUT_DATA
                    DataTable dt1 = runcard.Tables["OUT_DATA"];
                    for (int col = 0; col < dt1.Columns.Count; col++)
                    {
                        string strColName = dt1.Columns[col].ColumnName;

                        if (cr.Fields.Contains(strColName))
                        {
                            double dValue = 0;
                            int nValue = 0;

                            if (strColName.Equals("INPUT_QTY") || strColName.Equals("DFCT_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");
                            }
                            else if (strColName.Equals("PRD_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["PRD_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.Equals("WIP_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["WIP_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.IndexOf("BARCODE") > -1)
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                                cr.Fields[strColName + "_TXT"].Text = dt1.Rows[0][strColName].ToString();
                            }
                            else
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                            }

                        }
                    }

                    // 2.OUT_ELEC
                    DataTable dt2 = runcard.Tables["OUT_ELEC"];
                    for (int col = 0; col < dt2.Columns.Count; col++)
                    {
                        string strColName = dt2.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dt2.Rows[0][strColName].ToString();
                    }

                    // 3.OUT_DFCT
                    DataTable dt3 = runcard.Tables["OUT_DFCT"];
                    for (int col = 0; col < dt3.Columns.Count; col++)
                    {
                        string strColName = dt3.Columns[col].ColumnName;

                        //for (int row = 0; row < dt3.Rows.Count; row++)
                        for (int row = 0; row < 30; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                cr.Fields[strColName + strRowName].Text = "";

                                if (row < dt3.Rows.Count)
                                {
                                    double dValue = 0;

                                    if (strColName.Equals("DFCT_QTY"))
                                    {
                                        if (double.TryParse(Util.NVC(dt3.Rows[row][strColName]), out dValue))
                                        {
                                            if (dValue > 0)
                                                cr.Fields[strColName + strRowName].Text = " " + dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr.Fields[strColName + strRowName].Text = dt3.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }

                    // 4.OUT_SEPA
                    DataTable dt4 = runcard.Tables["OUT_SEPA"];
                    for (int col = 0; col < dt4.Columns.Count; col++)
                    {
                        string strColName = dt4.Columns[col].ColumnName;

                        //for (int row = 0; row < dt4.Rows.Count; row++)
                        for (int row = 0; row < 8; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                if (row < dt4.Rows.Count)
                                    cr.Fields[strColName + strRowName].Text = dt4.Rows[row][strColName].ToString();
                                else
                                    cr.Fields[strColName + strRowName].Text = "";

                            }
                        }
                    }

                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
                //c1DocumentViewer.FitToWidth();
                //c1DocumentViewer.FitToHeight();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



    }
}
