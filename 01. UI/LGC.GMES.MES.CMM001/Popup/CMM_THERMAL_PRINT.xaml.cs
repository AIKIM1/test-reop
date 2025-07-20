using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT : C1Window, IWorkArea
    {
        private int iCnt = 1;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT()
        {
            InitializeComponent();
            object[] tmps = C1WindowExtension.GetParameters(this);
            dicParam = tmps[0] as Dictionary<string, string>;

        }

        public CMM_THERMAL_PRINT(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam["reportName"].Equals("Fold"))
                {
                    if (dicParam.ContainsKey("LOTID")) LOTID_FOLD.Text = dicParam["LOTID"];
                    if (dicParam.ContainsKey("QTY")) QTY_FOLD.Text = dicParam["QTY"];
                    if (dicParam.ContainsKey("MAGID")) MAGID_FOLD.Text = dicParam["MAGID"];
                    if (dicParam.ContainsKey("MAGID")) BARCODE_FOLD.Text = "*" + dicParam["MAGID"] + "*";
                    if (dicParam.ContainsKey("LARGELOT")) LARGELOT_FOLD.Text = dicParam["LARGELOT"];
                    if (dicParam.ContainsKey("MODEL")) MODEL_FOLD.Text = dicParam["MODEL"];
                    if (dicParam.ContainsKey("REGDATE")) REGDATE_FOLD.Text = dicParam["REGDATE"];
                    if (dicParam.ContainsKey("EQPTNO")) EQPTNO_FOLD.Text = dicParam["EQPTNO"];
                    if (dicParam.ContainsKey("TITLEX")) TITLEX_FOLD.Text = dicParam["TITLEX"].Equals("BASKET ID") ? "BASKET\r\nID" : dicParam["TITLEX"];
                    grFoldPrint.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    if (dicParam.ContainsKey("LOTID")) LOTID_LAMI.Text = dicParam["LOTID"];
                    if (dicParam.ContainsKey("QTY")) QTY_LAMI.Text = dicParam["QTY"];
                    if (dicParam.ContainsKey("MAGID")) MAGID_LAMI.Text = dicParam["MAGID"];
                    if (dicParam.ContainsKey("MAGID")) BARCODE_LAMI.Text = "*" + dicParam["MAGID"] + "*";
                    if (dicParam.ContainsKey("CASTID")) CASTID_LAMI.Text = dicParam["CASTID"];
                    if (dicParam.ContainsKey("MODEL")) MODEL_LAMI.Text = dicParam["MODEL"];
                    if (dicParam.ContainsKey("REGDATE")) REGDATE_LAMI.Text = dicParam["REGDATE"];
                    if (dicParam.ContainsKey("EQPTNO")) EQPTNO_LAMI.Text = dicParam["EQPTNO"];
                    if (dicParam.ContainsKey("CELLTYPE")) CELLTYPE_LAMI.Text = dicParam["CELLTYPE"];
                    if (dicParam.ContainsKey("TITLEX")) TITLEX_LAMI.Text = dicParam["TITLEX"].Equals("MAGAZINE ID") ? "MAGAZINE\r\nID" : dicParam["TITLEX"];
                    grLamiPrint.Margin = new Thickness(0, 0, 0, 0);
                }

                if (dicParam.ContainsKey("PRINTQTY"))
                    iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                PrintDialog dialog = new PrintDialog();

                this.Width = 242 + 2;
                this.Height = 227 + 30;


                //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 240);
                dialog.PrintVisual(grFoldPrint, "GMES PRINT");

            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                this.Width = 242 + 2;
                this.Height = 257 + 30;

                grLamiPrint.Margin = new Thickness(0);

                //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 270);

                //dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //for (int i = 0; i < iCnt; i++)
                //{
                dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //}
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                if (dicParam["reportName"].Equals("Fold"))
                {
                    this.Width = 242 + 2;
                    this.Height = 227 + 30;
                    //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 240);

                    //dialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                    //dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                    for (int i = 0; i < iCnt; i++)
                    {
                        dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                    }
                }
                else
                {
                    this.Width = 242 + 2;
                    this.Height = 257 + 30;

                    grLamiPrint.Margin = new Thickness(0);
                    //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 270);

                    //dialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                    //dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                    for (int i = 0; i < iCnt; i++)
                    {
                        dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                    }
                }


                this.Close();
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
