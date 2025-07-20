/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.10.25  장영철 : 자재투입요청서 QR 추가 (GM2 Pjt) 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Printing;
using System.Windows;

using QRCoder;
using System.Drawing;

namespace LGC.GMES.MES.ELEC001
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

        C1.C1Report.C1Report cr = null;

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;
        DataTable tmmp03;
        DataTable tmmp04;
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
            tmmp03 = tmps[2] as DataTable;
            tmmp04 = tmps[3] as DataTable;

            this.Loaded -= Window_Loaded;

            //Page설정
            txtPage.Text = "1";
            iMAX_PAGE = tmmp02.Rows.Count;
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < iMAX_PAGE; i++)
                {
                    txtPage.Text = (i + 1).ToString();
                    txtPage_ValueChanged(i);

                    //c1DocumentViewer.Print();

                    /*
                    var pq = LocalPrintServer.GetDefaultPrintQueue();
                    var writer = PrintQueue.CreateXpsDocumentWriter(pq);
                    var paginator = c1DocumentViewer.Document.DocumentPaginator;
                    writer.Write(paginator);
                    */

                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    pm.Print(ps, ps.DefaultPageSettings);
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                // C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.ELEC001.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();

                        //투입요청서 QR코드 추가 GM2 (컬럼명이 QR_ 로 시작하는 데이터는 QR로 변환)
                        if (reportname.Equals("MReqList_Print") && strColName.Substring(0,3) == "QR_")
                        {
                            cr.Fields[strColName].Text = "";
                            ImageConverter imgcvt = new ImageConverter();
                            System.Drawing.Image img = (System.Drawing.Image)imgcvt.ConvertFrom(GetQRCode(tmmp02.Rows[iPage][strColName].ToString()));
                            cr.Fields[strColName].Picture = img == null ? (System.Drawing.Image)imgcvt.ConvertFrom("") : img;
                        }

                    }
                    for (int col = 0; col < tmmp03.Columns.Count; col++)
                    {
                        string strColName = tmmp03.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp03.Rows[iPage][strColName].ToString();
                    }
                    for (int col = 0; col < tmmp04.Columns.Count; col++)//다국어처리
                    {
                        string strColName = tmmp04.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp04.Rows[iPage][strColName].ToString();
                    }


                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);

            System.Drawing.Image fullsizeImage = qr.GetGraphic(50);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(100, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
        }

    }
}
