using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

using System.Printing;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_THERMAL_PRINT_Winding.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_Winding : C1Window, IWorkArea
    {

        DataSet runcard = null;
        string _printName = "";

        //BizDataSet _Biz = new BizDataSet();

        //Dictionary<string, string> dicParam;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_Winding()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Window_Loaded;
            this.DialogResult = MessageBoxResult.OK;
            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_THERMAL_PRINT.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_THERMAL_PRINT.Rows[0]?[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString()))
                {
                    _printName = LoginInfo.CFG_THERMAL_PRINT.Rows[0]?[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString();
                }

                if (_printName.Equals(""))
                {
                    Util.MessageValidation("SFU3615");  //프린터 환경설정에 설비 정보를 확인하세요.
                }
                else
                {
                    object[] tmps = C1WindowExtension.GetParameters(this);
                    // Runcard 발행 DataSet
                    runcard = tmps[0] as DataSet;
                    printDataSet(); //값 세팅
                    printThermal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
            }
        }

        #endregion

        public CMM_THERMAL_PRINT_Winding(Dictionary<string, string> dic)
        {
            InitializeComponent();

           // SetParameters(dic);
        }

        private void printDataSet()
        {
            //전체 초기화
            printDataInit();

            double dValue = 0;
            int nValue = 0;
            string sTemp = "*";

            DataTable dt = runcard.Tables["OUT_DATA"];
            this.EQPTSHORTNAME.Text = dt.Rows[0]["EQPTSHORTNAME"].ToString();
            this.PRJT_NAME2.Text = dt.Rows[0]["PRJT_NAME2"].ToString();
            this.WOTYPEDESC2.Text = dt.Rows[0]["WOTYPEDESC2"].ToString();
            this.PRJT_NAME1.Text = dt.Rows[0]["PRJT_NAME1"].ToString() + " / " + dt.Rows[0]["WOTYPEDESC1"].ToString() + " / " + dt.Rows[0]["MKT_TYPE_DESC"].ToString();
            this.MODLNAME.Text = dt.Rows[0]["MODLNAME"].ToString();

            
            this.BARCODE1.Text = dt.Rows[0]["BARCODE1"].ToString();
            this.BARCODE1.Text = string.IsNullOrEmpty(this.BARCODE1.Text) ? "" : sTemp + this.BARCODE1.Text + sTemp;
            this.BARCODE1_TXT.Text = dt.Rows[0]["BARCODE1"].ToString();
            this.BARCODE2.Text = string.IsNullOrEmpty(this.BARCODE2.Text) ? "" : sTemp + this.BARCODE2.Text + sTemp;
            this.BARCODE2_TXT.Text = dt.Rows[0]["BARCODE2"].ToString();

            this.PRINT_DATE.Text = dt.Rows[0]["PRINT_DATE"].ToString();
            this.LOTID.Text = dt.Rows[0]["LOTID"].ToString();
            this.WINDING_RUNCARD_ID.Text = dt.Rows[0]["WINDING_RUNCARD_ID"].ToString();
            this.WORKER.Text = dt.Rows[0]["WORKER"].ToString();
            this.VLD_DATE.Text = dt.Rows[0]["VLD_DATE"].ToString();
            this.PRD_DATE.Text = dt.Rows[0]["PRD_DATE"].ToString();

            this.WIP_NOTE.Text = dt.Rows[0]["WIP_NOTE"].ToString();
            this.WIP_TRAY.Text = dt.Rows[0]["WIP_TRAY"].ToString();

            if (double.TryParse(Util.NVC(dt.Rows[0]["INPUT_QTY"]), out dValue))
                this.INPUT_QTY.Text = dValue.ToString("N0");
            if (double.TryParse(Util.NVC(dt.Rows[0]["DFCT_QTY"]), out dValue))
                this.DFCT_QTY.Text = dValue.ToString("N0");

            if (double.TryParse(Util.NVC(dt.Rows[0]["PRD_QTY"]), out dValue))
                this.PRD_QTY.Text = dValue.ToString("N0");

            if (int.TryParse(Util.NVC(dt.Rows[0]["PRD_TRAY"]), out nValue))
            {
                if (nValue > 0)
                    this.PRD_QTY.Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
            }

            if (double.TryParse(Util.NVC(dt.Rows[0]["WIP_QTY"]), out dValue))
                this.WIP_QTY.Text = dValue.ToString("N0");

            if (int.TryParse(Util.NVC(dt.Rows[0]["WIP_TRAY"]), out nValue))
            {
                if (nValue > 0)
                    this.WIP_QTY.Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
            }

            DataTable dt1 = runcard.Tables["OUT_ELEC"];
            this.CTYPE_LOTID.Text = dt1.Rows[0]["CTYPE_LOTID"].ToString();
            this.CTYPE_VER.Text = dt1.Rows[0]["CTYPE_VER"].ToString();
            this.ATYPE_LOTID.Text = dt1.Rows[0]["ATYPE_LOTID"].ToString();
            this.ATYPE_VER.Text = dt1.Rows[0]["ATYPE_VER"].ToString();
        }

        private void printDataInit()
        {
            this.EQPTSHORTNAME.Text = "";
            this.PRJT_NAME2.Text = "";
            this.WOTYPEDESC2.Text = "";
            this.PRJT_NAME1.Text = "";
            this.MODLNAME.Text = "";
            this.BARCODE1.Text = "";
            this.BARCODE1_TXT.Text = "";
            this.BARCODE2.Text = "";
            this.BARCODE2_TXT.Text = "";
            this.PRINT_DATE.Text = "";
            this.LOTID.Text = "";
            this.WINDING_RUNCARD_ID.Text = "";
            this.WORKER.Text = "";
            this.CTYPE_LOTID.Text = "";
            this.CTYPE_VER.Text = "";
            this.PRD_DATE.Text = "";
            this.ATYPE_LOTID.Text = "";
            this.ATYPE_VER.Text = "";
            this.VLD_DATE.Text = "";
            this.INPUT_QTY.Text = "";
            this.PRD_QTY.Text = "";
            this.WIP_QTY.Text = "";
            this.DFCT_QTY.Text = "";
            this.WIP_NOTE.Text = "";
            this.WIP_TRAY.Text = "";
        }


        private void printThermal()
        {

            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
            dialog.PrintQueue = new PrintQueue(new PrintServer(), _printName);
            dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight);

            grFoldPrint.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
            grFoldPrint.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight))); //grFoldPrint.Arrange(new Rect(new Point(5, 5), grFoldPrint.DesiredSize));
            grFoldPrint.UpdateLayout();


            FixedDocument fixedDoc = GetFixedDocument(grFoldPrint, dialog, new Thickness(1, 1, 1, 1));

            //dialog.PrintVisual(grFoldPrint, "GMES PRINT");
            dialog.PrintDocument(fixedDoc.DocumentPaginator, "GMES PRINT");//the error of width and height must be nonnegative。
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
             
        }
        
        public static FixedDocument GetFixedDocument(FrameworkElement toPrint, PrintDialog printDialog, Thickness margin)
        {
            System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            Size visibleSize = new Size(capabilities.PageImageableArea.ExtentWidth - margin.Left - margin.Right, capabilities.PageImageableArea.ExtentHeight - margin.Top - margin.Bottom);
            FixedDocument fixedDoc = new FixedDocument();
            toPrint.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
            toPrint.Arrange(new Rect(new Size(toPrint.ActualWidth, toPrint.ActualHeight)));

            //   
            Size size = toPrint.DesiredSize;
            //Will assume for simplicity the control fits horizontally on the page   
            double yOffset = 0;
            while (yOffset < size.Height)
            {
                VisualBrush vb = new VisualBrush(toPrint);
                vb.Stretch = Stretch.None;
                vb.AlignmentX = AlignmentX.Left;
                vb.AlignmentY = AlignmentY.Top;
                vb.ViewboxUnits = BrushMappingMode.Absolute;
                vb.TileMode = TileMode.None;
                vb.Viewbox = new Rect(0, yOffset, visibleSize.Width, visibleSize.Height);

                PageContent pageContent = new PageContent();
                FixedPage page = new FixedPage();
                ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
                fixedDoc.Pages.Add(pageContent);
                page.Width = pageSize.Width;
                page.Height = pageSize.Height;
                Canvas canvas = new Canvas();
                FixedPage.SetLeft(canvas, capabilities.PageImageableArea.OriginWidth);
                FixedPage.SetTop(canvas, capabilities.PageImageableArea.OriginHeight);
                canvas.Width = visibleSize.Width;
                canvas.Height = visibleSize.Height;
                canvas.Background = vb;
                canvas.Margin = margin;

                page.Children.Add(canvas);
                yOffset += visibleSize.Height;
            }
            return fixedDoc;
        }

 

    }
}
