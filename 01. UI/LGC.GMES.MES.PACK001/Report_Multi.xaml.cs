/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.IO;
using System.Printing;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
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


        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty; //report 이름
        DataTable tmmp02; //report date
        int iMAX_PAGE = 0;


        public Report_Multi()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable;

            this.Loaded -= Window_Loaded;

            //Page설정
            txtPage.Text = "1";
            iMAX_PAGE = tmmp02.Rows.Count;
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);

            //Print();

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                Print();
                
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        private void Print()
        {
            try
            {
                for (int i = 0; i < iMAX_PAGE; i++)
                {
                    txtPage.Text = (i + 1).ToString();
                    txtPage_ValueChanged(i);
                    //c1DocumentViewer.Print();

                    var pq = LocalPrintServer.GetDefaultPrintQueue();
                    var writer = PrintQueue.CreateXpsDocumentWriter(pq);
                    var paginator = c1DocumentViewer.Document.DocumentPaginator;
                    writer.Write(paginator);

                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                throw ex;
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

                C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        if(col == 300)
                        {

                        }

                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();
                    }

                    //Language Binding
                    for (int fields_iex = 0; fields_iex < cr.Fields.Count; fields_iex++)
                    {
                        if(fields_iex == 400)
                        {

                        }

                        if (cr.Fields[fields_iex].Text != null)
                        {                           
                            if (cr.Fields[fields_iex].Name.Contains("Line") || cr.Fields[fields_iex].Name.Contains("Qty") ||
                                cr.Fields[fields_iex].Name.Contains("P_ID") || cr.Fields[fields_iex].Name.Contains("P_Qty") ||
                                cr.Fields[fields_iex].Name.Contains("L_ID") || cr.Fields[fields_iex].Name.Contains("L_Qty"))
                            {                 
                                        
                            }
                            else
                            {
                                if (cr.Fields[fields_iex].Name == "Pack_ID" ||
                                    cr.Fields[fields_iex].Name == "HEAD_BARCODE"||
                                    cr.Fields[fields_iex].Name == "Model" ||
                                    cr.Fields[fields_iex].Name == "Prod_ID" ||
                                    cr.Fields[fields_iex].Name == "Prod_Date" ||
                                    cr.Fields[fields_iex].Name == "Out_WH" ||
                                    cr.Fields[fields_iex].Name == "In_WH" ||
                                    cr.Fields[fields_iex].Text == "Pallet ID" ||
                                    cr.Fields[fields_iex].Name == "User"
                                    )
                                {

                                }
                                else
                                {
                                    cr.Fields[fields_iex].Text = ObjectDic.Instance.GetObjectName(cr.Fields[fields_iex].Text);
                                }
                            }
                        }

                        cr.Fields[fields_iex].Font.Name = "돋움체";
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
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);
            }

        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);
            }
        }

    }
}
