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
    public partial class Report_1st_Boxing : C1Window, IWorkArea
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
        public bool bRemainder
        {
            get;
            set;
        }

        C1.C1Report.C1Report cr = null;
        private DataTable dtOutData;
        private DataTable dtOutBox;
        int iMAX_PAGE = 0;      
        int Standard = 60;

        public Report_1st_Boxing()
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
                btnPrint.IsEnabled = false;

                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    for (int i = 0; i < iMAX_PAGE; i++)
                    {
                        if (!(txtPage.Text == "1" && i == 0))
                            txtPage_ValueChanged(i);
                        txtPage.Text = (i + 1).ToString();

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
            finally
            {
                btnPrint.IsEnabled = true;
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

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report.Pallet_1st_Boxing.xml"))
                {
                    cr.Load(stream, "Pallet_1st_Boxing");

                    cr.Fields["PAGE"].Text = String.Format("({0}/{1})", txtPage.Text, iMAX_PAGE);

                    for (int col = 0; col < dtOutData.Columns.Count; col++)
                    {
                        string strColName = dtOutData.Columns[col].ColumnName;

                        if (strColName.Contains("QTY")
                            && cr.Fields.Contains(strColName))
                        {
                            cr.Fields[strColName].Text = Util.NVC_Int(dtOutData.Rows[0][strColName]).ToString("#,###");
                        }
                        else if (strColName == "EQPTNAME"
                            && cr.Fields.Contains("TagTitle"))
                        {
                            cr.Fields["TagTitle"].Text = dtOutData.Rows[0]["EQPTNAME"].ToString() + cr.Fields["TagTitle"].Text;
                        }

                        else if (cr.Fields.Contains(strColName))
                        {
                            cr.Fields[strColName].Text = dtOutData.Rows[0][strColName].ToString();
                        }

                       
                    }

                    if (dtOutBox.Rows.Count > 0)
                    {
                        dtOutBox = dtOutBox.Select("1=1", "BOXSEQ").CopyToDataTable();

                        int start = Standard * iPage;
                        int end = Standard * (iPage+1) < dtOutBox.Rows.Count ? Standard * (iPage + 1) : dtOutBox.Rows.Count;
                        int cellQty = 0;

                        for (int row = 0; row < end - start; row++)
                        {
                            if (cr.Fields.Contains("BOXID_BC" + (row+1).ToString()))
                                cr.Fields["BOXID_BC" + (row + 1).ToString()].Text = cr.Fields["BOXID_TXT" + (row + 1).ToString()].Text = dtOutBox.Rows[start + row]["BOXID"].ToString();

                            if (cr.Fields.Contains("BOX_QTY" + (row + 1).ToString()))
                            {
                                int qty = Util.NVC_Int(dtOutBox.Rows[start + row]["TOTAL_QTY"]);
                                cellQty += qty;
                                cr.Fields["BOX_QTY" + (row + 1).ToString()].Text = qty.ToString("#,###");
                            }
                        }
                        cr.Fields["CELLQTY"].Text = cellQty.ToString("#,###");
                    }
                    if (cr.Fields.Contains("PLLTID_BC")) cr.Fields["PLLTID_BC"].Text = dtOutData.Rows[0]["BOXID"].ToString();
                    if (cr.Fields.Contains("PLLTID_TXT")) cr.Fields["PLLTID_TXT"].Text = dtOutData.Rows[0]["BOXID"].ToString();
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
                string bizRule = string.Empty;
                // 데이터 조회
                if (bRemainder == true)
                    bizRule = "BR_PRD_GET_TAG_INPALLET_NJ";
                else
                    bizRule = "BR_PRD_GET_TAG_PALLET_NJ";
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = sPalletID;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi(bizRule , "INDATA", "OUTDATA,OUTBOX,OUTLOT", inDataSet);
                
                dtOutData = ds.Tables["OUTDATA"];
                dtOutBox = ds.Tables["OUTBOX"];

                PALLET_ID = dtOutData.Rows[0]["BOXID"].ToString();

                iMAX_PAGE = (dtOutBox.Rows.Count - 1) / Standard + 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
