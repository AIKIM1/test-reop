/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.05.19  장희만  C20220509-000185 PackingList에 Mix Lot 출력 추가 
  2023.03.13  LEEHJ   SI               소형활성화 MES 복사
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_Report_2nd_Boxing : C1Window, IWorkArea
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
        public string PACK_VIRT_LOTID
        {
            get;
            set;
        }
        C1.C1Report.C1Report cr = null;
        private DataTable dtOutData;
        private DataTable dtOutLot;
        private string ShipType;
        int iMAX_PAGE = 0;
        int Standard = 20;

        public FCS002_Report_2nd_Boxing()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length > 1)
            {
                PALLET_ID = tmps[0] as string;
                PACK_VIRT_LOTID = tmps[1] as string;
            }
            else
            { 
                PALLET_ID = tmps[0] as string;
            }
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

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.FCS002.Report.Pallet_2nd_Boxing.xml"))
                {
                    cr.Load(stream, "Pallet_2nd_Boxing");

                    if (cr.Fields.Contains("PRT_DTTM"))
                    {
                        cr.Fields["PRT_DTTM"].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    if (dtOutData != null && dtOutData.Rows.Count > 0)
                    {
                        for (int col = 0; col < dtOutData.Columns.Count; col++)
                        {
                            string strColName = dtOutData.Columns[col].ColumnName;

                            if (strColName.Equals("MODEL_NAME"))
                            {
                                cr.Fields[strColName + "_BCD"].Text = cr.Fields[strColName].Text = dtOutData.Rows[0][strColName].ToString();
                            }

                            if (strColName.Equals("BOXID"))
                            {
                                cr.Fields[strColName + "_BCD"].Text = cr.Fields[strColName + "_TXT"].Text = dtOutData.Rows[0][strColName].ToString();
                            }
                            else if (strColName.Equals("TOTAL_QTY"))
                            {
                                cr.Fields[strColName + "_BCD"].Text = dtOutData.Rows[0][strColName].ToString();
                                cr.Fields[strColName + "_TXT"].Text = Util.NVC_Int(dtOutData.Rows[0][strColName]).ToString("#,###");
                            }
                            else if (strColName.Equals("ISS_DTTM"))
                            {
                                cr.Fields[strColName + "_BCD"].Text = cr.Fields[strColName + "_TXT"].Text = dtOutData.Rows[0][strColName].ToString();
                            }
                            else if (strColName.Equals("CUSTPRODID"))
                            {
                                cr.Fields[strColName + "_BCD"].Text = cr.Fields[strColName + "_TXT"].Text = dtOutData.Rows[0][strColName].ToString();
                            }

                            if (cr.Fields.Contains(strColName))
                            {
                                cr.Fields[strColName].Text = dtOutData.Rows[0][strColName].ToString();
                            }

                            if (ShipType == "SMP")
                            {
                                cr.Fields["PRODUCT"].Visible = false;
                                cr.Fields["PRODNAME"].Visible = false;
                                cr.Fields["SMP_PN"].Visible = true;
                                cr.Fields["CUSTPRODID_TXT"].Visible = true;
                                cr.Fields["CUSTPRODID_BCD"].Visible = true;

                            }
                            else
                            {
                                cr.Fields["PRODUCT"].Visible = true;
                                cr.Fields["PRODNAME"].Visible = true;
                                cr.Fields["SMP_PN"].Visible = false;
                                cr.Fields["CUSTPRODID_TXT"].Visible = false;
                                cr.Fields["CUSTPRODID_BCD"].Visible = false;
                            }
                        }
                    }

                    if (dtOutLot != null && dtOutLot.Rows.Count > 0)
                    {
                        int start = Standard * iPage;
                        int end = Standard * (iPage + 1) < dtOutLot.Rows.Count ? Standard * (iPage + 1) : dtOutLot.Rows.Count;

                        for (int row = 0; row < end - start; row++)
                        {
                            if (cr.Fields.Contains("LOTNO" + (row + 1).ToString()))
                                cr.Fields["LOTNO" + (row + 1).ToString()].Text = Util.NVC(dtOutLot.Rows[start + row]["LOT_PRODWEEK"]).ToString();
                            cr.Fields["LOTNO" + (row + 1) + "_BCD".ToString()].Text = Util.NVC(dtOutLot.Rows[start + row]["PKG_LOTID"]).ToString();

                            if (cr.Fields.Contains("QTY" + (row + 1).ToString()))
                                cr.Fields["QTY" + (row + 1).ToString()].Text = Util.NVC_Int(dtOutLot.Rows[start + row]["LOTQTY"]).ToString("#,###");

                            if (cr.Fields.Contains("GRD" + (row + 1).ToString()))
                                cr.Fields["GRD" + (row + 1).ToString()].Text = Util.NVC(dtOutLot.Rows[start + row]["PRDT_GRD_CODE"]).ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(PACK_VIRT_LOTID))
                {
                    cr.Fields["MIX_LOTID"].Text = ObjectDic.Instance.GetObjectName("잔량") + " : " + Util.NVC(PACK_VIRT_LOTID);
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

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_PALLET_MB", "INDATA", "OUTDATA,OUTBOX,OUTLOT", inDataSet);

                dtOutData = ds.Tables["OUTDATA"];
                dtOutLot = ds.Tables["OUTLOT"];

                PALLET_ID = dtOutData.Rows[0]["BOXID"].ToString();
                if (!string.IsNullOrWhiteSpace(Util.NVC(dtOutData.Rows[0]["SHIP_LIST_TYPE_CODE"])))
                    ShipType = Util.NVC(dtOutData.Rows[0]["SHIP_LIST_TYPE_CODE"]);

                iMAX_PAGE = (dtOutLot.Rows.Count - 1) / Standard + 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
