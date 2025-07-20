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
    public partial class Report_Box_List : C1Window, IWorkArea
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
        private DataTable dtInfo;        
        int iMAX_PAGE = 0;
        int BoxQty = 0;
        int TotalQty = 0;
        int Standard = 90;

        public Report_Box_List()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            PALLET_ID = tmps[0] as string;

            this.Loaded -= Window_Loaded;

            txtPage.Text = "1";
            txtPrintQty.Value = 1;

            PrintView(PALLET_ID);
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //for (int i = 0; i < iMAX_PAGE; i++)
                //{
                //    txtPage.Text = (i + 1).ToString();
                //    txtPage_ValueChanged(i);

                //    var pq = LocalPrintServer.GetDefaultPrintQueue();
                //    var writer = PrintQueue.CreateXpsDocumentWriter(pq);
                //    var paginator = c1DocumentViewer.Document.DocumentPaginator;
                //    writer.Write(paginator);
                //}

                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    for (int i = 0; i < iMAX_PAGE; i++)
                    {
                        txtPage.Text = (i + 1).ToString();
                        txtPage_ValueChanged(i);

                        var pm = new C1.C1Preview.C1PrintManager();
                        pm.Document = cr;
                        System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                        if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                            ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                        //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
                        pm.Print(ps, ps.DefaultPageSettings);

                        //var pq = LocalPrintServer.GetDefaultPrintQueue();
                        //var writer = PrintQueue.CreateXpsDocumentWriter(pq);
                        //var paginator = c1DocumentViewer.Document.DocumentPaginator;
                        //writer.Write(paginator);

                    }
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
        
        private void txtPage_ValueChanged(int iPage)
        {
            try
            {
                string filename = string.Empty;
                string reportname = string.Empty;

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report.Box_List.xml"))
                {
                    cr.Load(stream, "BoxList");
                    if (cr.Fields.Contains("PALLETID"))
                        cr.Fields["PALLETID"].Text = PALLET_ID;

                    if (cr.Fields.Contains("QTY"))
                        cr.Fields["QTY"].Text = TotalQty.ToString();

                    for (int row = 1; row <= dtInfo.Rows.Count; row++)
                    {
                        if (cr.Fields.Contains("BOXID"+row.ToString("D2")))
                            cr.Fields["BOXID" + row.ToString("D2")].Text = dtInfo.Rows[Standard * iPage + row - 1]["BOXID"].ToString();

                        if (cr.Fields.Contains("LOT_PRODWEEK" + row.ToString("D2")))
                            cr.Fields["LOT_PRODWEEK" + row.ToString("D2")].Text = dtInfo.Rows[Standard * iPage + row - 1]["LOT_PRODWEEK"].ToString();

                        if (cr.Fields.Contains("TOTAL_QTY" + row.ToString("D2")))
                            cr.Fields["TOTAL_QTY" + row.ToString("D2")].Text = dtInfo.Rows[Standard * iPage + row - 1]["TOTAL_QTY"].ToString();
                        if (iPage > 0
                            && cr.Fields.Contains(row.ToString()))
                            cr.Fields[row.ToString()].Text = (Standard * iPage + row).ToString();
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }          
        }

        private void PrintView(string sPalletID)
        {
            try
            {
                // 데이터 조회
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = sPalletID;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_PALLET_NJ", "INDATA", "OUTDATA,OUTBOX,OUTLOT", inDataSet);

                PALLET_ID = (String)ds.Tables["OUTDATA"].Rows[0]["BOXID"];
                BoxQty = Util.NVC_Int(ds.Tables["OUTDATA"].Rows[0]["BOXQTY"]);
                TotalQty = Util.NVC_Int(ds.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"]);

                dtInfo = ds.Tables["OUTBOX"];

                iMAX_PAGE = (dtInfo.Rows.Count - 1) / Standard + 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
