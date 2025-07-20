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
    public partial class Report_Gogoro_Tag : C1Window, IWorkArea
    {

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PALLET_ID
        {
            get;
            set;
        }

        C1.C1Report.C1Report cr = null;
        private DataTable dtOutData = new DataTable();
        public Report_Gogoro_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            PALLET_ID = tmps[0] as string;

            this.Loaded -= Window_Loaded;

            txtPrintQty.Value = 1;

            PrintView(PALLET_ID);
            setReport();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty.Value.ToString());

                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
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

        private void setReport()
        {
            try
            {
                string filename = string.Empty;
                string reportname = string.Empty;

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report.Report_Gogoro_Tag.xml"))
                {
                    cr.Load(stream, "Report_Gogoro_Tag");

                    if (cr.Fields.Contains("PRT_DTTM"))
                    {
                        cr.Fields["PRT_DTTM"].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    if (dtOutData != null && dtOutData.Rows.Count > 0)
                    {
                        for (int col = 0; col < dtOutData.Columns.Count; col++)
                        {
                            string strColName = dtOutData.Columns[col].ColumnName;

                            if(cr.Fields.Contains(strColName))
                                cr.Fields[strColName].Text = dtOutData.Rows[0][strColName].ToString();
                        }
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }
        private void PrintView(string sPalletID)
        {

            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "INDATA";
                dtRqstDt.Columns.Add("BOXID", typeof(string));
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("AREAID", typeof(string));

                DataRow inDataRow = dtRqstDt.NewRow();
                inDataRow["BOXID"] = sPalletID;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstDt.Rows.Add(inDataRow);

                if(LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    dtOutData = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_GOGORO_TAG_PALLET_NJ", "RQSTDT", "RSLTDT", dtRqstDt);
                else
                    dtOutData = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_GOGORO_TAG_PALLET_FM", "RQSTDT", "RSLTDT", dtRqstDt);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }
    }
}
