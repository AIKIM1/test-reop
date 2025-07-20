/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_042_PALLET : C1Window, IWorkArea
    {

        C1.C1Report.C1Report cr = null;

        string tmp = string.Empty;
        object[] tmps;
        DataTable dtPallet;
        int totalPage = 0;
        int startPageNum = 0;
        int endPageNum = 0;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_042_PALLET()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            dtPallet = tmps[0] as DataTable;
            totalPage = int.Parse(tmps[1] as string); //총수량
            startPageNum = int.Parse(tmps[2] as string); //시작 페이지
            endPageNum = int.Parse(tmps[3] as string); //끝 페이지

            this.Loaded -= Window_Loaded;

            cr = new C1.C1Report.C1Report();
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A5;

            // string filename = string.Empty;
            // string reportname = string.Empty;

            //// filename = "Pallet_TTi.xml";
            // reportname = "Pallet_TTi";
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report.Pallet_TTi.xml"))
            {
                cr.Load(stream, "Pallet_TTi");

                for (int col = 0; col < dtPallet.Columns.Count; col++)
                {
                    string strColName = dtPallet.Columns[col].ColumnName;
                    if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dtPallet.Rows[0][strColName].ToString();
                    //if (cr.Fields.Contains("Pallet_No"))
                    //{
                    //    if(tmp03.Equals("ALL"))
                    //        cr.Fields["Pallet_No"].Text = 1.ToString() + "/" + tmp02.ToString();
                    //    else
                    //        cr.Fields["Pallet_No"].Text = tmp03 + "/" + tmp02.ToString();

                    //}
                }
            }

            c1DocumentViewer.Document = cr.FixedDocumentSequence;

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                for (int iPrint = startPageNum; iPrint <= endPageNum; iPrint++)
                {
                    cr.Fields["Pallet_No"].Text = Convert.ToString(iPrint) + "/" + Convert.ToString(totalPage);
                    c1DocumentViewer.Document = cr.FixedDocumentSequence;

                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                    pm.Print(ps);
                    System.Threading.Thread.Sleep(2000);
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
    }
}
