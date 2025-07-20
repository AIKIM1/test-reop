/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.05.17  김도형    : [E20240408-000359] 电极包装card改善 전극포장card improvement

  



 
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
    public partial class Report_Multi : C1Window, IWorkArea
    {

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        //string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;
        int iMAX_PAGE = 0;


        public Report_Multi()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable;

            this.Loaded -= Window_Loaded;

            //Page설정
            //if (tmps.Length > 2)
            //{
            //    txtPage.Text = tmps[2] as string;
            //}
            //else
            //{
                txtPage.Text = "1";
            //}


            // Print 수량 셋팅하기
            if (tmps.Length > 2)
            {
                txtPrintQty.Value = Convert.ToInt32(tmps[2] as string);
            }
            else
            {
                txtPrintQty.Value = 1;
            }

            // [E20240408-000359] 电极包装card改善 전극포장card improvement
            string sReportname = string.Empty;
            sReportname = tmmp01;

            if (sReportname.Equals("PackingCard_New") || sReportname.Equals("PackingCard_New_NJ") || sReportname.Equals("PackingCard_2CRT") || sReportname.Equals("PackingCard_2CRT_NJ"))
            {
                if (tmmp02 != null && tmmp02.Rows.Count > 0)
                {
                    SetDemandTypeForSkidID();
                }
            }
            
            iMAX_PAGE = tmmp02.Rows.Count;
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);

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

        C1.C1Report.C1Report cr = null;

        private void txtPage_ValueChanged(int iPage)
        {

            try
            {
                string filename = string.Empty;
                string reportname = string.Empty;

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();
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
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }

            //int iCurPage = Util.NVC_Int(txtPrintQty.Text);
            //if (iCurPage > 1)
            //{
            //    txtPrintQty.Text = (iCurPage - 1).ToString();
            //}

        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }

            //int iCurPage = Util.NVC_Int(txtPrintQty.Text);
            //if (iCurPage >= 1)
            //{
            //    txtPrintQty.Text = (iCurPage + 1).ToString();
            //    //txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            //}
        }

        // [E20240408-000359] 电极包装card改善 전극포장card improvement
        private void SetDemandTypeForSkidID()
        {
            try
            {
                tmmp02.Columns.Add("DEMAND_TYPE1", typeof(string));
                tmmp02.Columns.Add("DEMAND_TYPE2", typeof(string));

                for (int i = 0; i < tmmp02.Rows.Count; i++)
                {
                    if (!tmmp02.Rows[i]["Lot1"].ToString().Equals(""))
                    {
                        tmmp02.Rows[i]["DEMAND_TYPE1"] = GetDemandTypeForSkidID(tmmp02.Rows[i]["Lot1"].ToString());
                    }
                    if (!tmmp02.Rows[i]["Lot2"].ToString().Equals(""))
                    {
                        tmmp02.Rows[i]["DEMAND_TYPE2"] = GetDemandTypeForSkidID(tmmp02.Rows[i]["Lot2"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return;
        } 

        // [E20240408-000359] 电极包装card改善 전극포장card improvement
        private string GetDemandTypeForSkidID(string sSkidID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SKIDID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SKIDID"] = sSkidID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEMAND_TYPE_FOR_SKIDID", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    return result.Rows[0]["DEMAND_TYPE"].ToString();  //DEMAND_TYPE
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
                return "";
            }

            return "";
        }
    }
}
