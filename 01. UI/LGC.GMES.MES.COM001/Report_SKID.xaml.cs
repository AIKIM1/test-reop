/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.C1Preview;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using QRCoder;
using System.Drawing;

namespace LGC.GMES.MES.COM001
{
    public partial class Report_SKID : C1Window, IWorkArea
    {
        #region Declaration & Constructor   

        C1.C1Report.C1Report cr1 = null;
        C1.C1Report.C1Report cr2 = null;
        
        object[] tmps;
        string tmmp01 = string.Empty;
        string tmmp02 = string.Empty;
        DataTable tmmp03;

        DataTable dtSkidInfo;

        int iMAX_PAGE = 0;
        int _iCount = 0;
        int _iMaxRow = 32;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_SKID()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as string;
            tmmp03 = tmps[2] as DataTable;

            this.Loaded -= Window_Loaded;

            txtPage.Text = "1";
            txtPrintQty.Value = 1;

            SearchData();
            Open();
        }

        private void SearchData()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("RQSTDT");

               // dtRqst.Columns.Add("WIPSTAT", typeof(string));
               // dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                //dr["WIPSTAT"] = Wip_State.WAIT;
                //dr["PROCID"] = Process.ELEC_STORAGE;
                dr["CSTID"] = tmmp02;

                dtRqst.Rows.Add(dr);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_SKID_PRINT_INFO_FOR_SKID", "RQSTDT", "RSLTDT", inData);

                dtSkidInfo = dsRslt.Tables["RSLTDT"];

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void Open()
        {
            try
            {
                cr1 = new C1.C1Report.C1Report();
                cr2 = new C1.C1Report.C1Report();

                cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                string filename = string.Empty;
                string reportname = string.Empty;
                double lSumCell = 0;
                string sSumLot = string.Empty;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.COM001.Report." + filename))
                {
                    cr1.Load(stream, reportname);
                }

                _iCount = dtSkidInfo.Rows.Count;

                for (int row = 0; row < tmmp03.Rows.Count; row++)
                {
                    for (int col = 0; col < tmmp03.Columns.Count; col++)
                    {
                        string strColName = tmmp03.Columns[col].ColumnName;
                        string strFieldName = tmmp03.Columns[col].ColumnName;
                        if (cr1.Fields.Contains(strFieldName))
                            cr1.Fields[strFieldName].Text = tmmp03.Rows[row][strColName].ToString();

                    }
                }

                if (_iCount > _iMaxRow)
                    cr2.CopyFrom(cr1);

                for (int row = 0; row < dtSkidInfo.Rows.Count; row++)
                {
                    for (int col = 0; col < dtSkidInfo.Columns.Count; col++)
                    {
                        string strColName = dtSkidInfo.Columns[col].ColumnName;
                        string strFieldName = dtSkidInfo.Columns[col].ColumnName + row;
                        if (row < _iMaxRow)
                        {
                            if (cr1.Fields.Contains(strFieldName))
                                cr1.Fields[strFieldName].Text = dtSkidInfo.Rows[row][strColName].ToString();
                        }
                        else
                        {
                            strFieldName = dtSkidInfo.Columns[col].ColumnName + (row - _iMaxRow);
                            if (cr2.Fields.Contains(strFieldName))
                                cr2.Fields[strFieldName].Text = dtSkidInfo.Rows[row][strColName].ToString();
                        }
                    }
                    lSumCell = lSumCell + Convert.ToDouble(dtSkidInfo.Rows[row]["QTY"].ToString());
                    sSumLot = sSumLot + "/" + dtSkidInfo.Rows[row]["LOTID"].ToString();
                }
                cr1.Fields["TOTAL_QTY"].Text = lSumCell.ToString("#,#.000");

                //C20220625-000035 SKID Card increase QR Code 2022.08.31 강호운
                if (IsAreaCommonCodeUse("ELEC_SKID_QRCODE_USE"))
                {
                    ImageConverter imgcvt = new ImageConverter();
                    System.Drawing.Image img = (System.Drawing.Image)imgcvt.ConvertFrom(GetQRCode(tmmp02 + sSumLot));
                    cr1.Fields["QRCODE"].Picture = img == null ? (System.Drawing.Image)imgcvt.ConvertFrom("") : img;
                }

                cr1.Render();
                if (_iCount >= _iMaxRow) cr2.Render();

                cr1.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                cr1.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                cr1.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";

                iMAX_PAGE = cr1.GetPageCount() + cr2.GetPageCount();
                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[iMAX_PAGE];
                int index = 0;

                // get pageimages of report1
                foreach (System.Drawing.Imaging.Metafile Page in cr1.GetPageImages())
                {
                    Pages.SetValue(Page, index);
                    index++;
                }

                // get pageimages of report2
                foreach (System.Drawing.Imaging.Metafile Page in this.cr2.GetPageImages())
                {
                    Pages.SetValue(Page, index);
                    index++;
                }

                for (int i = 0; i <= (Pages.Length - 1); i++)
                {
                    RenderImage PageImage = new RenderImage(Pages[i]);
                    cr1.C1Document.Body.Children.Add(PageImage);
                    cr1.C1Document.Reflow();
                }

                c1DocumentViewer.Document = cr1.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr1;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    pm.Print(ps, ps.DefaultPageSettings);

                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                c1DocumentViewer.NextPage();
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                c1DocumentViewer.PreviousPage();
            }
        }

        private void c1DocumentViewer_PageViewsChanged(object sender, EventArgs e)
        {
            if (c1DocumentViewer.PageViews.Count > 0)
                txtPage.Text = (c1DocumentViewer.PageViews[0].PageNumber + 1).ToString();
        }


        //C20220625-000035 SKID Card increase QR Code 2022.08.31 강호운
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
        private bool IsAreaCommonCodeUse(string sCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
               // RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }
    }
}
