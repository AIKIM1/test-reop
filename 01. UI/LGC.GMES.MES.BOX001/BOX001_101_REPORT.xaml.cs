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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_101_REPORT : C1Window, IWorkArea
    {
        #region Declaration
        DataSet runcard = null;
        C1.C1Report.C1Report cr = null;
        string _USERNAME = string.Empty;
        string _REMARK = string.Empty;
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
        public BOX001_101_REPORT()
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
            _USERNAME = tmps[1] as string;
            _REMARK = tmps[2] as string;
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

            if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

            pm.Print(ps);

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

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report.InPallet_Label.xml"))
                {
                    cr.Load(stream, "InPallet_Label");

                    // 임시 다국어 처리
                    //for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    //{
                    //    if (cr.Fields[cnt].Name.ToString().IndexOf("Text") > -1)
                    //    {
                    //        cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                    //    }
                    //}
                    
                    // 2.OUT_ELEC
                    DataTable dt = runcard.Tables["OUTDATA"];
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        string strColName = dt.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dt.Rows[0][strColName].ToString();
                    }

                    if (cr.Fields.Contains("PACKDTTM") && !string.IsNullOrWhiteSpace(cr.Fields["PACKDTTM"].Text))
                    {
                        DateTime dEndDttm = Convert.ToDateTime(cr.Fields["PACKDTTM"].Text);
                        cr.Fields["PACKDTTM"].Text = dEndDttm.ToShortDateString();
                    }

                    if (cr.Fields.Contains("BOXID") && !string.IsNullOrWhiteSpace(cr.Fields["BOXID"].Text))
                    {
                        if (cr.Fields.Contains("BOXID_BC")) cr.Fields["BOXID_BC"].Text = cr.Fields["BOXID"].Text; // "*" + cr.Fields["BOXID"].Text + "*";
                    }

                    if (cr.Fields.Contains("ACTUSER")) cr.Fields["ACTUSER"].Text = _USERNAME;

                    if (cr.Fields.Contains("PACK_NOTE")) cr.Fields["PACK_NOTE"].Text = _REMARK;

                    if (cr.Fields.Contains("TOTAL_QTY"))
                    {
                        int iQTY = Util.NVC_Int(cr.Fields["TOTAL_QTY"].Text);
                        cr.Fields["TOTAL_QTY"].Text = String.Format("{0:#,###0}", iQTY);
                    }

                    if (cr.Fields.Contains("TOTAL_BOXQTY"))
                    {
                        int iQTY = Util.NVC_Int(cr.Fields["TOTAL_BOXQTY"].Text);
                        cr.Fields["TOTAL_BOXQTY"].Text = String.Format("{0:#,###0}", iQTY);
                    }

                    if (cr.Fields.Contains("PRINT_DTTM")) cr.Fields["PRINT_DTTM"].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //DataTable dt2 = runcard.Tables["OUTLOT"];

                    foreach (C1.C1Report.Field field in cr.Fields)
                    {
                        if (dt.Rows.Count > 2 && field.Name.Contains("ROW2"))
                        {
                            field.Visible = true;
                        }
                        if (dt.Rows.Count > 4 && field.Name.Contains("ROW3"))
                        {
                            field.Visible = true;
                        }
                    }

                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        string strColName = dt.Columns[col].ColumnName;
                       
                        for (int row = 0; row < dt.Rows.Count; row++)
                        {
                            string strRowName = (row).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                cr.Fields[strColName + strRowName].Visible = true;

                                string sValue = dt.Rows[row][strColName].ToString();
                                if (strColName.Contains("WIPQTY"))
                                {
                                    int iQTY = Util.NVC_Int(sValue);
                                    sValue = String.Format("{0:#,###0}", iQTY); 
                                }
                                if (row < dt.Rows.Count)
                                    cr.Fields[strColName + strRowName].Text = sValue;
                                else
                                    cr.Fields[strColName + strRowName].Text = string.Empty;

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
