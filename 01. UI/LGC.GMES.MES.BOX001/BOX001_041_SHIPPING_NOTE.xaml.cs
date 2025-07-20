/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Printing;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_041_SHIPPING_NOTE : C1Window, IWorkArea
    {
        C1.C1Report.C1Report cr = null;

        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_041_SHIPPING_NOTE()
        {
            InitializeComponent();
            this.Loaded += C1Window_Loaded;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            object[] tmps = C1WindowExtension.GetParameters(this);

            txtFrom.Text = "LG Chem., Ltd";
            txtLocation.Text = "Kor-Ochang";
            txtWorker.Text = "J.B.Kim";
            txtShipTo.Text = "BOSCH";
            txtCustPartNo.Text = Util.NVC(tmps[0]);
            txtPONo.Text = Util.NVC(tmps[1]);
            txtShippingNoteNo.Text = Util.NVC(tmps[2]);
            txtSupplierID.Text = "97121026";
            txtQty.Text = Util.NVC(tmps[3]);
            txtBatchNo.Text = Util.NVC(tmps[2]);
            txtExpDate.Text = Util.NVC(tmps[6]);

            this.Loaded -= C1Window_Loaded;

            SetPreview();

        }
        private void SetPreview()
        {
            prv_From.Text = txtFrom.Text;
            prv_Loc.Text = txtLocation.Text;
            prv_Worker.Text = txtWorker.Text;
            prv_shipTo.Text = txtShipTo.Text;
            prv_CustPartNo.Text = txtCustPartNo.Text;
            prv_PONo.Text = txtPONo.Text;
            prv_ShipNo.Text = txtShippingNoteNo.Text;
            prv_SupplierID.Text = txtSupplierID.Text;
            prv_Qty.Text = txtQty.Text;
            prv_BatchNo.Text = txtBatchNo.Text;
            prv_ExpDate.Text = txtExpDate.Text;
            bcPallet.Text = "[)>@06" + "@K" + prv_PONo.Text + "@16K" + prv_ShipNo.Text + "@@";
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //int iCopies = int.Parse(nbPrintQty.Value.ToString());

                //for (int iPrint = 0; iPrint < iCopies; iPrint++)
                //{
                //    var pm = new C1.C1Preview.C1PrintManager();
                //    pm.Document = cr;
                //    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                //    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                //        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                //    pm.Print(ps);

                //}
                //this.DialogResult = MessageBoxResult.OK;
                //this.Close();
                PrintPallet();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void PrintPallet()
        {
            try
            {
                
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    PrintTicket pt = printDlg.PrintTicket;
                    pt.PageOrientation = PageOrientation.Portrait;
                    pt.PageMediaSize = new PageMediaSize(PageMediaSizeName.NorthAmericaLetter);
                    // get selected printer capabilities 
                    PrintCapabilities capabilities = printDlg.PrintQueue.GetPrintCapabilities(pt);

                    //get scale of the print wrt to screen of WPF visual 
                    double Scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / (this.grdPreview.ActualWidth +40),
                        capabilities.PageImageableArea.ExtentHeight / (this.grdPreview.ActualHeight +40));

                    // Transform the visual to scale 
                    System.Windows.Media.MatrixTransform mt = this.grdPreview.LayoutTransform as MatrixTransform;
                    this.grdPreview.LayoutTransform = new ScaleTransform(Scale, Scale);
                    
                    // get the size of the printer page 
                    System.Windows.Size sz = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth,
                        capabilities.PageImageableArea.ExtentHeight);

                    //// update the layout of the visual to the printer page size 
                    //this.grdPreview.Measure(sz);

                    this.grdPreview.Arrange(new Rect(new System.Windows.Point(20 + txtXPoint.Value,
                        txtYPoint.Value), sz));


                    // now print the visual to printer to fit the page 
                    printDlg.PrintVisual(this.grdPreview,"Bosch Pallet Print");
                    this.grdPreview.LayoutTransform = mt;

                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                chkFinal.IsChecked = false;
            }

        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            ////listAuth.Add(btnOutDel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            SetPreview();
        }

        private void btnPrintFinal_Click(object sender, RoutedEventArgs e)
        {
            chkFinal.IsChecked = true;
            PrintPallet();
        }

        private void chkFinal_Checked(object sender, RoutedEventArgs e)
        {
            prv_QtyTitle.Text = "Total quantity:";
        }

        private void chkFinal_Unchecked(object sender, RoutedEventArgs e)
        {
            prv_QtyTitle.Text = "Total quantity in Pallet:";

        }
    }
}
