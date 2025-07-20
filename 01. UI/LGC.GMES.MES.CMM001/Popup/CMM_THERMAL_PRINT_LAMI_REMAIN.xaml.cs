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
    /// CMM_THERMAL_PRINT_LAMI_REMAIN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_LAMI_REMAIN : C1Window, IWorkArea
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

        string _sPGM_ID = "CMM_THERMAL_PRINT_LAMI_REMAIN";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_LAMI_REMAIN()
        {
            InitializeComponent();
        }

        public CMM_THERMAL_PRINT_LAMI_REMAIN(Dictionary<string, string> dic)
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

                PrintUseFlowdocument();

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

                        grLamiRemain.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grLamiRemain.Arrange(new Rect(new Size(grLamiRemain.ActualWidth, grLamiRemain.ActualHeight)));     //grLamiRemain.Arrange(new Rect(new Point(0, 0), grLamiRemain.DesiredSize));
                        grLamiRemain.UpdateLayout();

                        //this.Measure(new Size(grLamiRemain.ActualWidth, grLamiRemain.ActualHeight));
                        //this.Arrange(new Rect(new Size(grLamiRemain.ActualWidth, grLamiRemain.ActualHeight)));
                        //this.UpdateLayout();

                        //System.Threading.Thread.Sleep(200);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintVisual(grLamiRemain, "GMES Remain PRINT");
                            }

                            SetLabelPrtHist();

                            ////인쇄 완료 되었습니다.
                            //if (_ShowMsg.Equals("Y"))
                            //    Util.MessageInfo("SFU1236");
                        }
                        ), DispatcherPriority.ContextIdle, null);                        
                    }

                    //인쇄 완료 되었습니다.
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfo("SFU1236");
                }
                else
                {
                    if (dicParam != null)
                    {
                        grLamiRemain.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grLamiRemain.Arrange(new Rect(new Size(grLamiRemain.ActualWidth, grLamiRemain.ActualHeight)));     //grLamiRemain.Arrange(new Rect(new Point(0, 0), grLamiRemain.DesiredSize));
                        grLamiRemain.UpdateLayout();
                        
                        Dispatcher.Invoke(new Action(() =>
                        {
                            for (int i = 0; i < iCnt; i++)
                            {
                                dialog.PrintVisual(grLamiRemain, "GMES Remain PRINT");
                            }

                            SetLabelPrtHist();

                            ////인쇄 완료 되었습니다.
                            //if (_ShowMsg.Equals("Y"))
                            //    Util.MessageInfo("SFU1236");
                        }
                        ), DispatcherPriority.ContextIdle, null);

                        //인쇄 완료 되었습니다.
                        if (_ShowMsg.Equals("Y"))
                            Util.MessageInfo("SFU1236");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

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

                        var fd = CreateFlowDocument(dicParam);

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

                            SetLabelPrtHist();                            
                        }
                    }

                    //인쇄 완료 되었습니다.
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfo("SFU1236");
                }
                else
                {
                    if (dicParam != null)
                    {
                        var fd = CreateFlowDocument(dicParam);

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

                            SetLabelPrtHist();
                            
                            //인쇄 완료 되었습니다.
                            if (_ShowMsg.Equals("Y"))
                                Util.MessageInfo("SFU1236");
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
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(60, GridUnitType.Pixel) });
                
                /* Column 1 */
                // Row 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                br1.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(br1);

                // Row 3
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 3 -1
                var br17 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br17.SetValue(Grid.RowProperty, 3);
                br17.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br17);

                // Row 4
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 4);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 5
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);

                // Row 6
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br4.SetValue(Grid.RowProperty, 6);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);
                

                /* Column 2 */
                // Row 1
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 0);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br8);

                // Row 2
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 1);
                br9.SetValue(Grid.ColumnProperty, 1);
                br9.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br9);

                // Row 3
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 2);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Row 3-1
                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br16.SetValue(Grid.RowProperty, 3);
                br16.SetValue(Grid.ColumnProperty, 1);
                br16.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br16);

                // Row 4
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                br11.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br11);

                // Row 4
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br15.SetValue(Grid.RowProperty, 5);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br15);

                // Row 5
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br14.SetValue(Grid.RowProperty, 6);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);
                
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                t1.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t1);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("PJT(모델명)")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.WrapWithOverflow };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 2);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("극성")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.WrapWithOverflow };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t6);                

                var t2 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("총수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.WrapWithOverflow };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 4);                
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("잔량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.WrapWithOverflow };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("교체이유/처리방법")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.WrapWithOverflow };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t4);
                
                #endregion

                #region 3. Create TextBlock For Setting Value

                string sLot = "";
                string sTotQty = "";                
                string sRemainQty = "";
                string sNote = "";
                string sModel = "";
                string sCodeName = "";

                if (dic.ContainsKey("PANCAKEID")) sLot = Util.NVC(dic["PANCAKEID"]);
                if (dic.ContainsKey("TOT_QTY")) sTotQty = Util.NVC(dic["TOT_QTY"]);
                if (dic.ContainsKey("REMAIN_QTY")) sRemainQty = Util.NVC(dic["REMAIN_QTY"]);
                if (dic.ContainsKey("NOTE")) sNote = Util.NVC(dic["NOTE"]);
                if (dic.ContainsKey("MODEL")) sModel = Util.NVC(dic["MODEL"]);
                if (dic.ContainsKey("PRDT_CLSS_NAME")) sCodeName = Util.NVC(dic["PRDT_CLSS_NAME"]);

                // Lot
                var tbFoldLot = new TextBlock() { Text = sLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 1);
                tbModel.SetValue(Grid.RowProperty, 2);
                tbModel.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbModel);

                // PRDT_CLSS_CODE
                var tbClass = new TextBlock() { Text = sCodeName, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbClass.SetValue(Grid.ColumnProperty, 1);
                tbClass.SetValue(Grid.RowProperty, 3);
                tbClass.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbClass);                

                // Total Qty
                var tbTotQty = new TextBlock() { Text = sTotQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbTotQty.SetValue(Grid.ColumnProperty, 1);
                tbTotQty.SetValue(Grid.RowProperty, 4);
                tbTotQty.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbTotQty);

                // Remain Qty
                var tbQty = new TextBlock() { Text = sRemainQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 5);
                tbQty.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbQty);

                // Note
                var tbNote = new TextBlock() { Text = sNote, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10, TextWrapping = TextWrapping.WrapWithOverflow };
                tbNote.SetValue(Grid.ColumnProperty, 1);
                tbNote.SetValue(Grid.RowProperty, 6);
                tbNote.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbNote);
                
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 235;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetParameters(Dictionary<string, string> dic)
        {
            try
            {
                dicParam = dic;

                if (dicParam == null) return;

                if (dicParam.ContainsKey("PANCAKEID")) PANCAKEID.Text = dicParam["PANCAKEID"];
                if (dicParam.ContainsKey("PANCAKEID")) BARCODE_PANCAKEID.Text = "*" + dicParam["PANCAKEID"] + "*";
                if (dicParam.ContainsKey("TOT_QTY")) TOT_QTY.Text = dicParam["TOT_QTY"];                
                if (dicParam.ContainsKey("REMAIN_QTY")) REMAIN_QTY.Text = dicParam["REMAIN_QTY"];
                if (dicParam.ContainsKey("NOTE")) NOTE.Text = dicParam["NOTE"];

                grLamiRemain.Margin = new Thickness(0, 0, 0, 0);

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
                PANCAKEID.Text = "";
                BARCODE_PANCAKEID.Text = "";
                TOT_QTY.Text = "";
                REMAIN_QTY.Text = "";
                NOTE.Text = "";
                
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
                string sNote = dicParam.ContainsKey("NOTE") ? Util.NVC(dicParam["NOTE"]) : "";

                if (sNote.Length > 98)
                {
                    sNote = sNote.Substring(0, 98);
                }

                string sBizRule = "BR_PRD_REG_LABEL_PRINT_HIST";

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = iCnt.ToString();                                                        // 발행수
                newRow["PRT_ITEM01"] = dicParam.ContainsKey("B_LOTID") ? Util.NVC(dicParam["B_LOTID"]) : "";
                newRow["PRT_ITEM02"] = dicParam.ContainsKey("B_WIPSEQ") ? Util.NVC(dicParam["B_WIPSEQ"]) : "";
                newRow["PRT_ITEM03"] = "PANCAKE_REMAIN";                                                            // Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)
                newRow["PRT_ITEM04"] = "N";                                                                         // 재발행 여부
                newRow["PRT_ITEM05"] = dicParam.ContainsKey("PANCAKEID") ? Util.NVC(dicParam["PANCAKEID"]) : "";    // Pancake ID
                newRow["PRT_ITEM06"] = dicParam.ContainsKey("TOT_QTY") ? Util.NVC(dicParam["TOT_QTY"]) : "";        // 총수량
                newRow["PRT_ITEM07"] = dicParam.ContainsKey("REMAIN_QTY") ? Util.NVC(dicParam["REMAIN_QTY"]) : "";  // 잔량
                newRow["PRT_ITEM08"] = sNote;                                                                       // 교체이유/처리방법
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
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
                newRow["BZRULE_ID"] = sBizRule;

                inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                new ClientProxy().ExecuteService(sBizRule, "INDATA", null, inTable, (result, searchException) =>
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
                
    }
}
