/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
  2018.12.27  황기근    : C20181227_79985. SetDispatch() 와 SetLabelPrtHist() 순서 변경 - 디스패치 후 발행되도록 수정
  2019.01.15  황기근    : C20181227_79985. SetDispatch() 의 catch 문 Util.MessageException(ex)을 throw ex 로 변경.
  2019.12.06  이상준    :                . ZZS 공정추가 대응 신규 출력양식 추가  
**************************************************************************************/

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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_THERMAL_PRINT_FOLD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_FOLD : C1Window, IWorkArea
    {
        private int iCnt = 1;
        
        private List<Dictionary<string, string>> _dicList;
        private string _ProcID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ShowMsg = string.Empty;
        private string _Dispatch = string.Empty;

        BizDataSet _Biz = new BizDataSet();

        Dictionary<string, string> dicParam;

        string _sPGM_ID = "CMM_THERMAL_PRINT_FOLD";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_FOLD()
        {
            InitializeComponent();
        }

        public CMM_THERMAL_PRINT_FOLD(Dictionary<string, string> dic)
        {
            InitializeComponent();

            SetParameters(dic);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 6)
                {
                    _dicList = tmps[0] as List<Dictionary<string, string>>;
                    _ProcID = Util.NVC(tmps[1]);
                    _EqsgID = Util.NVC(tmps[2]);
                    _EqptID = Util.NVC(tmps[3]);
                    _ShowMsg = Util.NVC(tmps[4]);
                    _Dispatch = Util.NVC(tmps[5]);
                }
                else
                {
                    _dicList = null;
                    _ProcID = "";
                    _EqsgID = "";
                    _EqptID = "";
                    _ShowMsg = "Y";
                    _Dispatch = "Y"; // 기본 디스패치 처리.
                }

                //this.Hide();

                //PrintProcess();
                //PrintProcessFixedDocument();
                
                PrintUseFlowdocument();

                #region "...."
                //grdEtc.Visibility = Visibility.Collapsed;

                //PrintDialog dialog = new PrintDialog();

                //if (_dicList != null && _dicList.Count > 0)
                //{
                //    if (_ProcID.Equals(Process.STACKING_FOLDING))
                //    {
                //        grLamiPrint.Visibility = Visibility.Collapsed;

                //        for (int x = 0; x < _dicList.Count; x++)
                //        {
                //            ClearParameters();

                //            SetParameters(_dicList[x]);

                //            this.Measure(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight));
                //            this.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight)));
                //            this.UpdateLayout();

                //            System.Threading.Thread.Sleep(400);

                //            //dialog.PrintTicket.CopyCount = iCnt < 1 ? 1 : iCnt;
                //            //dialog.PrintVisual(grFoldPrint, "GMES PRINT");

                //            for (int i = 0; i < iCnt; i++)
                //            {
                //                dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                //            }

                //            SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                //            if (_Dispatch.Equals("Y"))
                //                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));
                //        }

                //        //this.UpdateLayout();

                //        //Dispatcher.BeginInvoke(new Action(() =>
                //        //{
                //        //    for (int x = 0; x < _dicList.Count; x++)
                //        //    {
                //        //        ClearParameters();

                //        //        SetParameters(_dicList[x]);

                //        //        for (int i = 0; i < iCnt; i++)
                //        //        {
                //        //            dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                //        //        }
                //        //    }

                //        //    this.DialogResult = MessageBoxResult.OK;

                //        //}), DispatcherPriority.ContextIdle, null);
                //    }
                //    else
                //    {
                //        grFoldPrint.Visibility = Visibility.Collapsed;

                //        for (int x = 0; x < _dicList.Count; x++)
                //        {
                //            ClearParameters();

                //            SetParameters(_dicList[x]);

                //            this.Measure(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight));
                //            this.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight)));
                //            this.UpdateLayout();

                //            System.Threading.Thread.Sleep(200);

                //            for (int i = 0; i < iCnt; i++)
                //            {
                //                dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //            }

                //            SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                //            if (_Dispatch.Equals("Y"))
                //                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));
                //        }
                //    }

                //    //인쇄 완료 되었습니다.
                //    if (_ShowMsg.Equals("Y"))
                //        Util.MessageInfo("SFU1236");
                //}
                #endregion

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
            }
        }

        private void PrintProcess()
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                if (_dicList != null && _dicList.Count > 0)
                {
                    for (int x = 0; x < _dicList.Count; x++)
                    {
                        ClearParameters();

                        SetParameters(_dicList[x]);

                        grFoldPrint.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grFoldPrint.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight))); //grFoldPrint.Arrange(new Rect(new Point(5, 5), grFoldPrint.DesiredSize));
                        grFoldPrint.UpdateLayout();

                        Dispatcher.Invoke(new Action(() =>
                        {
                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                            }

                            //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();
                            
                            ////인쇄 완료 되었습니다.
                            //if (_ShowMsg.Equals("Y"))
                            //    Util.MessageInfo("SFU1236");

                        }), DispatcherPriority.ContextIdle, null);
                    }

                    //인쇄 완료 되었습니다.
                    //INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfoAutoClosing("SFU1236");
                }
                else
                {
                    if (dicParam != null)
                    {
                        // 바구니 생성 후 자동 프린트 처리.

                        grFoldPrint.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grFoldPrint.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight))); //grFoldPrint.Arrange(new Rect(new Point(5, 5), grFoldPrint.DesiredSize));
                        grFoldPrint.UpdateLayout();

                        System.Threading.Thread.Sleep(200);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                            }

                                                        //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();

                            ////인쇄 완료 되었습니다.
                            //if (_ShowMsg.Equals("Y"))
                            //    Util.MessageInfo("SFU1236");

                        }), DispatcherPriority.ContextIdle, null);

                        //인쇄 완료 되었습니다.
                        //INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
                        if (_ShowMsg.Equals("Y"))
                            Util.MessageInfoAutoClosing("SFU1236");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void PrintBgThread()
        //{
        //    var printDialog = new PrintDialog();
        //    if (false == printDialog.ShowDialog())
        //    {
        //        return;
        //    }

        //    var selectedPrinter = printDialog.PrintQueue.Name;

        //    var action = new Action((SelectedPrinter) =>
        //    { 
        //        var doc = new FixedDocument();
        //        var page1 = new PageContent();
        //        var fixedPage1 = new FixedPage();
        //        fixedPage1.Children.Add(new Label() { Content = "Test Page1" }); //Adding user control populated with data
        //        page1.Child = fixedPage1;
        //        doc.Pages.Add(page1);

        //        var page2 = new PageContent();
        //        var fixedPage2 = new FixedPage();
        //        fixedPage2.Children.Add(new Label() { Content = "Test Page2" });
        //        page2.Child = fixedPage2;
        //        doc.Pages.Add(page2);
        //        var printDupDialog = new PrintDialog();
        //        printDupDialog.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), SelectedPrinter.ToString());
        //        printDupDialog.PrintDocument(doc.DocumentPaginator, SelectedPrinter.ToString());
        //    });

        //    var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(action));
        //    thread.SetApartmentState(System.Threading.ApartmentState.STA);
        //    thread.IsBackground = true;
        //    thread.Start(selectedPrinter);

        //}

        private void PrintUseFlowdocument()
        {
            try
            {
                PrintDialog dialog = new PrintDialog();
                
                if (_dicList != null && _dicList.Count > 0)
                {
                    foreach (Dictionary<string, string> dic in _dicList)
                    {
                        dicParam = dic;
                        
                        if (dicParam.ContainsKey("PRINTQTY"))
                            iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));

                        var fd = (FlowDocument)null;

                        if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                        {
                            if (_ProcID.Equals(Process.STP))
                                fd = CreateFlowDocumentSTP(dicParam, b3DModel: GetWoProdInfo());
                            else if (_ProcID.Equals(Process.ZZS))
                                fd = CreateFlowDocumentZZS(dicParam);
                            else
                                fd = CreateFlowDocumentNJ(dicParam);
                        }
                        else
                        {
                            fd = CreateFlowDocument(dicParam);
                        }


                        if (fd != null)
                        {
                            fd.PagePadding = new Thickness(5, 0, 5, 0);

                            //dialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                            dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                            ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                            
                                                        
                            var paginatorSource = (IDocumentPaginatorSource)fd;
                            var paginator = paginatorSource.DocumentPaginator;

                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintDocument(paginator, "GMES PRINT");
                            }

                            //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();
                        }
                    }

                    //인쇄 완료 되었습니다.
                    //INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfoAutoClosing("SFU1236");
                }
                else
                {
                    if (dicParam != null)
                    {
                        // 바구니 생성 후 자동 프린트 처리.
                        var fd = (FlowDocument)null;

                        if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                        {
                            if (_ProcID.Equals(Process.STP))
                                fd = CreateFlowDocumentSTP(dicParam, b3DModel: GetWoProdInfo());
                            else if (_ProcID.Equals(Process.ZZS))
                                fd = CreateFlowDocumentZZS(dicParam);
                            else
                                fd = CreateFlowDocumentNJ(dicParam);
                        }
                        else
                        {
                            fd = CreateFlowDocument(dicParam);
                        }

                        if (fd != null)
                        {
                            fd.PagePadding = new Thickness(5, 0, 5, 0);

                            //dialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                            dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                            ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                            

                            var paginatorSource = (IDocumentPaginatorSource)fd;
                            var paginator = paginatorSource.DocumentPaginator;

                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintDocument(paginator, "GMES PRINT");
                            }

                            //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();

                            //인쇄 완료 되었습니다.
                            //INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
                            if (_ShowMsg.Equals("Y"))
                                Util.MessageInfoAutoClosing("SFU1236");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static FlowDocument CreateFlowDocument(Dictionary<string, string> dic)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(65, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(46, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(84, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                /* Column 1 */
                // Row 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Row 2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.RowSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 4
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 3);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 5
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 4);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);

                // Row 6
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);

                // Row 7
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br6.SetValue(Grid.RowProperty, 6);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);



                /* Column 2 */
                // Row 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br7.SetValue(Grid.RowProperty, 0);
                br7.SetValue(Grid.ColumnProperty, 1);
                br7.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br7);

                // Row 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 1);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br8);

                // Row 3
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 2);
                br9.SetValue(Grid.ColumnProperty, 1);
                br9.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br9);

                // Row 4
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 3);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Row 5
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);

                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 4);
                br12.SetValue(Grid.ColumnProperty, 2);
                g.Children.Add(br12);

                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 4);
                br13.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br13);

                // Row 6
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 5);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);

                // Row 7
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br15.SetValue(Grid.RowProperty, 6);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br15);
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = "Folding\r\nLot", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value

                string sFoldLot = string.Empty;
                string sBasketLot = string.Empty;
                string sLagreLot = string.Empty;
                string sQty = string.Empty;
                string sModel = string.Empty;
                string sCaldate = string.Empty;
                string sEqptNo = string.Empty;

                if (dic.ContainsKey("LOTID")) sFoldLot = dic["LOTID"];
                if (dic.ContainsKey("QTY")) sQty = dic["QTY"];
                if (dic.ContainsKey("MAGID")) sBasketLot = dic["MAGID"];
                if (dic.ContainsKey("LARGELOT")) sLagreLot = dic["LARGELOT"];
                if (dic.ContainsKey("MODEL")) sModel = dic["MODEL"];
                if (dic.ContainsKey("REGDATE")) sCaldate = dic["REGDATE"];
                if (dic.ContainsKey("EQPTNO")) sEqptNo = dic["EQPTNO"];

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = sFoldLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sBasketLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = sBasketLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                var tbLagreLot = new TextBlock() { Text = sLagreLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = sQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = sCaldate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = sEqptNo, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 6);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 225;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static FlowDocument CreateFlowDocumentSTP(Dictionary<string, string> dic, bool b3DModel = false)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(65, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(46, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(84, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                //if(b3DModel)
                //    g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                //else
                //    g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                /* Column 1 */
                // Row 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Row 2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.RowSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 4
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 3);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 5
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 4);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);

                // Row 6
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);

                // Row 7
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br6.SetValue(Grid.RowProperty, 6);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);

                // Row 8
                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br16.SetValue(Grid.RowProperty, 7);
                br16.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br16);


                /* Column 2 */
                // Row 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br7.SetValue(Grid.RowProperty, 0);
                br7.SetValue(Grid.ColumnProperty, 1);
                br7.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br7);

                // Row 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 1);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br8);

                // Row 3
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 2);
                br9.SetValue(Grid.ColumnProperty, 1);
                br9.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br9);

                // Row 4
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 3);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Row 5
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);

                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 4);
                br12.SetValue(Grid.ColumnProperty, 2);
                g.Children.Add(br12);

                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 4);
                br13.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br13);

                // Row 6
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 5);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);
                
                // Row 7
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br15.SetValue(Grid.RowProperty, 6);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br15);

                // Row 8
                var br17 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 1) };
                br17.SetValue(Grid.RowProperty, 7);
                br17.SetValue(Grid.ColumnProperty, 1);
                br17.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br17);

                var br18 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br18.SetValue(Grid.RowProperty, 7);
                br18.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br18);

                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = "Folding\r\nLot", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);
                
                var t6 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 7);
                g.Children.Add(t7);

                var t8 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("CELLTYPE")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t8.SetValue(Grid.ColumnProperty, 0);
                t8.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t8);

                #endregion

                #region 3. Create TextBlock For Setting Value

                string sFoldLot = string.Empty;
                string sBasketLot = string.Empty;
                string sLagreLot = string.Empty;
                string sQty = string.Empty;
                string sModel = string.Empty;
                string sCaldate = string.Empty;
                string sEqptNo = string.Empty;
                string sCellType = string.Empty;
                string sMkt_type_code = "";
                string sCSTID = "";

                if (dic.ContainsKey("LOTID")) sFoldLot = dic["LOTID"];
                if (dic.ContainsKey("QTY")) sQty = dic["QTY"];
                if (dic.ContainsKey("MAGID")) sBasketLot = dic["MAGID"];
                if (dic.ContainsKey("LARGELOT")) sLagreLot = dic["LARGELOT"];
                if (dic.ContainsKey("MODEL")) sModel = dic["MODEL"];
                if (dic.ContainsKey("REGDATE")) sCaldate = dic["REGDATE"];
                if (dic.ContainsKey("EQPTNO")) sEqptNo = dic["EQPTNO"];
                if (dic.ContainsKey("CELLTYPE")) sCellType = dic["CELLTYPE"];
                if (dic.ContainsKey("MKT_TYPE_CODE")) sMkt_type_code = dic["MKT_TYPE_CODE"];
                if (dic.ContainsKey("CSTID")) sCSTID = dic["CSTID"];

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = sFoldLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sBasketLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = sBasketLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                //var tbLagreLot = new TextBlock() { Text = sLagreLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                var tbLagreLot = new TextBlock() { Text = sCSTID, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = sQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = sCaldate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 6);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = sEqptNo, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 7);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);

                // Cell Type
                var tbCellType = new TextBlock() { Text = sCellType, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbCellType.SetValue(Grid.ColumnProperty, 1);
                tbCellType.SetValue(Grid.RowProperty, 5);
                tbCellType.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbCellType);

                // 시장 유형
                var tbMktTypeCode = new TextBlock() { Text = sMkt_type_code, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10 };
                tbMktTypeCode.SetValue(Grid.ColumnProperty, 3);
                tbMktTypeCode.SetValue(Grid.RowProperty, 7);
                //tbCellType.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbMktTypeCode);

                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                //fd.PageHeight = b3DModel ? 255 : 235;
                fd.PageHeight = 255;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static FlowDocument CreateFlowDocumentZZS(Dictionary<string, string> dic)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(65, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(46, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(84, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                /* Column 1 */
                // Row 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Row 2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.RowSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 4
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 3);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 5
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 4);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);

                // Row 6
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);

                // Row 7
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br6.SetValue(Grid.RowProperty, 6);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);



                /* Column 2 */
                // Row 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br7.SetValue(Grid.RowProperty, 0);
                br7.SetValue(Grid.ColumnProperty, 1);
                br7.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br7);

                // Row 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 1);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br8);

                // Row 3
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 2);
                br9.SetValue(Grid.ColumnProperty, 1);
                br9.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br9);

                // Row 4
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 3);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Row 5
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);

                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 4);
                br12.SetValue(Grid.ColumnProperty, 2);
                g.Children.Add(br12);

                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 4);
                br13.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br13);

                // Row 6
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 5);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);

                // Row 7
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 1) };
                br15.SetValue(Grid.RowProperty, 6);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br15);

                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br16.SetValue(Grid.RowProperty, 6);
                br16.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br16);
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = "Box ID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value

                string sFoldLot = string.Empty;
                string sBasketLot = string.Empty;
                string sLagreLot = string.Empty;
                string sQty = string.Empty;
                string sModel = string.Empty;
                string sCaldate = string.Empty;
                string sEqptNo = string.Empty;
                string sMkt_type_code = string.Empty;
                string sCSTID = string.Empty;

                if (dic.ContainsKey("LOTID")) sFoldLot = dic["LOTID"];
                if (dic.ContainsKey("QTY")) sQty = dic["QTY"];
                if (dic.ContainsKey("MAGID")) sBasketLot = dic["MAGID"];
                if (dic.ContainsKey("LARGELOT")) sLagreLot = dic["LARGELOT"];
                if (dic.ContainsKey("MODEL")) sModel = dic["MODEL"];
                if (dic.ContainsKey("REGDATE")) sCaldate = dic["REGDATE"];
                if (dic.ContainsKey("EQPTNO")) sEqptNo = dic["EQPTNO"];
                if (dic.ContainsKey("MKT_TYPE_CODE")) sMkt_type_code = dic["MKT_TYPE_CODE"];
                if (dic.ContainsKey("CSTID")) sCSTID = dic["CSTID"];

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = sFoldLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sBasketLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = sBasketLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // CST ID (Box ID)
                var tbLagreLot = new TextBlock() { Text = sCSTID, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = sQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = sCaldate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = sEqptNo, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 6);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbEqptNo);

                // 시장 유형
                var tbMktTypeCode = new TextBlock() { Text = sMkt_type_code, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10 };
                tbMktTypeCode.SetValue(Grid.ColumnProperty, 3);
                tbMktTypeCode.SetValue(Grid.RowProperty, 6);
                //tbCellType.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbMktTypeCode);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 225;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static FlowDocument CreateFlowDocumentNJ(Dictionary<string, string> dic)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(65, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(46, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(84, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                /* Column 1 */
                // Row 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Row 2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.RowSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 4
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 3);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 5
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 4);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);

                // Row 6
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);

                // Row 7
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br6.SetValue(Grid.RowProperty, 6);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);



                /* Column 2 */
                // Row 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br7.SetValue(Grid.RowProperty, 0);
                br7.SetValue(Grid.ColumnProperty, 1);
                br7.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br7);

                // Row 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 1);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br8);

                // Row 3
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 2);
                br9.SetValue(Grid.ColumnProperty, 1);
                br9.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br9);

                // Row 4
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 3);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Row 5
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);

                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 4);
                br12.SetValue(Grid.ColumnProperty, 2);
                g.Children.Add(br12);

                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 4);
                br13.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br13);

                // Row 6
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 5);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);

                // Row 7
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 1) };
                br15.SetValue(Grid.RowProperty, 6);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br15);

                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br16.SetValue(Grid.RowProperty, 6);
                br16.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br16);
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = "Folding\r\nLot", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value

                string sFoldLot = string.Empty;
                string sBasketLot = string.Empty;
                string sLagreLot = string.Empty;
                string sQty = string.Empty;
                string sModel = string.Empty;
                string sCaldate = string.Empty;
                string sEqptNo = string.Empty;
                string sMkt_type_code = string.Empty;

                if (dic.ContainsKey("LOTID")) sFoldLot = dic["LOTID"];
                if (dic.ContainsKey("QTY")) sQty = dic["QTY"];
                if (dic.ContainsKey("MAGID")) sBasketLot = dic["MAGID"];
                if (dic.ContainsKey("LARGELOT")) sLagreLot = dic["LARGELOT"];
                if (dic.ContainsKey("MODEL")) sModel = dic["MODEL"];
                if (dic.ContainsKey("REGDATE")) sCaldate = dic["REGDATE"];
                if (dic.ContainsKey("EQPTNO")) sEqptNo = dic["EQPTNO"];
                if (dic.ContainsKey("MKT_TYPE_CODE")) sMkt_type_code = dic["MKT_TYPE_CODE"];

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = sFoldLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sBasketLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = sBasketLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                var tbLagreLot = new TextBlock() { Text = sLagreLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = sQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = sCaldate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = sEqptNo, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 6);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbEqptNo);

                // 시장 유형
                var tbMktTypeCode = new TextBlock() { Text = sMkt_type_code, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10 };
                tbMktTypeCode.SetValue(Grid.ColumnProperty, 3);
                tbMktTypeCode.SetValue(Grid.RowProperty, 6);
                //tbCellType.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbMktTypeCode);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 225;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private void PrintProcessFixedDocument()
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                if (_dicList != null && _dicList.Count > 0)
                {
                    for (int x = 0; x < _dicList.Count; x++)
                    {
                        ClearParameters();

                        SetParameters(_dicList[x]);

                        grFoldPrint.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grFoldPrint.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight))); //grFoldPrint.Arrange(new Rect(new Point(5, 5), grFoldPrint.DesiredSize));
                        grFoldPrint.UpdateLayout();

                        System.Threading.Thread.Sleep(200);

                        Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate ()
                        {
                            FixedDocument fixedDoc = GetFixedDocument(grFoldPrint, dialog, new Thickness(10 / 2, 10 / 2, 10 / 2, 10 / 2));
                            for (int i = 0; i < iCnt; i++)
                            {
                                //dialog.PrintVisual(grFoldPrint, "GMES PRINT");

                                dialog.PrintDocument(fixedDoc.DocumentPaginator, "GMES PRINT");//the error of width and height must be nonnegative。
                            }

                            //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();


                            ////인쇄 완료 되었습니다.
                            //if (_ShowMsg.Equals("Y"))
                            //    Util.MessageInfo("SFU1236");

                            
                        }));
                    }

                    //인쇄 완료 되었습니다.
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfo("SFU1236");                    
                }
                else
                {
                    if (dicParam != null)
                    {
                        // 바구니 생성 후 자동 프린트 처리.
                        grFoldPrint.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grFoldPrint.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight))); //grFoldPrint.Arrange(new Rect(new Point(5, 5), grFoldPrint.DesiredSize));
                        grFoldPrint.UpdateLayout();

                        System.Threading.Thread.Sleep(200);

                        //dialog.ShowDialog();
                        
                        Dispatcher.Invoke(DispatcherPriority.ApplicationIdle,    new Action(delegate () 
                        {
                            FixedDocument fixedDoc = GetFixedDocument(grFoldPrint, dialog, new Thickness(10 / 2, 10 / 2, 10 / 2, 10 / 2));
                            for (int i = 0; i < iCnt; i++)
                            {
                                //dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                                
                                dialog.PrintDocument(fixedDoc.DocumentPaginator, "GMES PRINT");//the error of width and height must be nonnegative。
                            }

                            //SetLabelPrtHist();

                            //if (_Dispatch.Equals("Y"))
                            //    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            if (_Dispatch.Equals("Y"))
                                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                            SetLabelPrtHist();

                            //인쇄 완료 되었습니다.
                            if (_ShowMsg.Equals("Y"))
                                Util.MessageInfo("SFU1236");
                                                        
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static FixedDocument GetFixedDocument(FrameworkElement toPrint, PrintDialog printDialog, Thickness margin)
        {
            System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            Size visibleSize = new Size(capabilities.PageImageableArea.ExtentWidth - margin.Left - margin.Right, capabilities.PageImageableArea.ExtentHeight - margin.Top - margin.Bottom);
            FixedDocument fixedDoc = new FixedDocument();
            //If the toPrint visual is not displayed on screen we neeed to measure and arrange it   
            //toPrint.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //toPrint.Arrange(new Rect(new Point(0, 0), toPrint.DesiredSize));
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

        private void SetParameters(Dictionary<string, string> dic)
        {
            try
            {
                dicParam = dic;

                if (dicParam == null) return;

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

                if (dicParam.ContainsKey("PRINTQTY"))
                    iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearParameters()
        {
            try
            {
                // Folding
                LOTID_FOLD.Text = "";
                QTY_FOLD.Text = "";
                MAGID_FOLD.Text = "";
                BARCODE_FOLD.Text = "";
                LARGELOT_FOLD.Text = "";
                MODEL_FOLD.Text = "";
                REGDATE_FOLD.Text = "";
                EQPTNO_FOLD.Text = "";
                TITLEX_FOLD.Text = "";
                
                iCnt = 1;

                dicParam = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLabelPrtHist()
        {
            try
            {
                string sBizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST";

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = iCnt.ToString();                                                    // 발행수량
                newRow["PRT_ITEM01"] = dicParam.ContainsKey("B_LOTID") ? Util.NVC(dicParam["B_LOTID"]) : "";
                newRow["PRT_ITEM02"] = dicParam.ContainsKey("B_WIPSEQ") ? Util.NVC(dicParam["B_WIPSEQ"]) : "";
                newRow["PRT_ITEM03"] = "BOX";                                                                   // Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)
                newRow["PRT_ITEM04"] = dicParam.ContainsKey("RE_PRT_YN") ? Util.NVC(dicParam["RE_PRT_YN"]) : ""; // 재발행 여부
                newRow["PRT_ITEM05"] = dicParam.ContainsKey("LOTID") ? Util.NVC(dicParam["LOTID"]) : "";        // Folding Lot
                newRow["PRT_ITEM06"] = dicParam.ContainsKey("MAGID") ? Util.NVC(dicParam["MAGID"]) : "";        // Box ID
                newRow["PRT_ITEM07"] = dicParam.ContainsKey("LARGELOT") ? Util.NVC(dicParam["LARGELOT"]) : "";  // 폴딩 LOT의 생성시간(공장시간기준)
                newRow["PRT_ITEM08"] = dicParam.ContainsKey("QTY") ? Util.NVC(dicParam["QTY"]) : "";            // 수량
                newRow["PRT_ITEM09"] = dicParam.ContainsKey("MODEL") ? Util.NVC(dicParam["MODEL"]) : "";        // 모델
                newRow["PRT_ITEM10"] = dicParam.ContainsKey("REGDATE") ? Util.NVC(dicParam["REGDATE"]) : "";    // 발행시간
                newRow["PRT_ITEM11"] = dicParam.ContainsKey("EQPTNO") ? Util.NVC(dicParam["EQPTNO"]) : "";      // 설비번호
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = dicParam.ContainsKey("B_LOTID") ? Util.NVC(dicParam["B_LOTID"]) : "";
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRuleName;

                inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                new ClientProxy().ExecuteService(sBizRuleName, "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
        }

        private void SetDispatch(string sBoxID, decimal dQty)
        {
            try
            {

                DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _EqsgID;
                newRow["REWORK"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                newRow = inDataTable.NewRow();
                newRow["LOTID"] = sBoxID;
                newRow["ACTQTY"] = dQty;
                newRow["ACTUQTY"] = 0;
                newRow["WIPNOTE"] = "";

                inDataTable.Rows.Add(newRow);


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                throw ex;
            }
        }

        private bool GetWoProdInfo()
        {
            try
            {
                bool b3DModel = false;

                //ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO_SRC();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_INFO_BY_WO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if ((dtRslt.Columns.Contains("PRDT_SIZE") && Util.NVC(dtRslt.Rows[0]["PRDT_SIZE"]).Trim().Length > 0) ||
                        (dtRslt.Columns.Contains("PRDT_SIZE2") && Util.NVC(dtRslt.Rows[0]["PRDT_SIZE2"]).Trim().Length > 0))
                    {
                        b3DModel = true;
                    }
                }

                return b3DModel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                //HiddenParentLoadingIndicator();
            }
        }
    }
}
