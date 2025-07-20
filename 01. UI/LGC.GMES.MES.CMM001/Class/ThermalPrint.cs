/*************************************************************************************
 Created Date : 2018.02.27
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 감열지 발행 Class 생성
--------------------------------------------------------------------------------------
 [Change History]
  2018.02.27  INS 김동일K : Initial Created.
  2018.10.02  INS 김동일K : 매거진 발행 기능 추가
  2020.03.18  CNS 김대근  : CarrierID가 바코드로 출력되는 유형 추가(CMI용)
**************************************************************************************/

using System;
using System.Data;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Class
{
    public class ThermalPrint : IDisposable
    {
        private static string PROCID = string.Empty;
        private static string EQSGID = string.Empty;
        private static string EQPTID = string.Empty;
        private static bool SAVE_PRT_HIST = false;
        private static bool DISPATCH = false;
        private static int PRT_CNT = 1;
        
        bool disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                
            }
            
            disposed = true;
        }

        ~ThermalPrint()
        {
            Dispose(false);
        }


        public bool Print(DataTable inData, string sProcID, string sEqsgID, string sEqptID, THERMAL_PRT_TYPE iType, int iPrtCnt, bool bSavePrtHist, bool bDispatch)
        {
            try
            {
                PROCID = sProcID;
                EQSGID = sEqsgID;
                EQPTID = sEqptID;
                SAVE_PRT_HIST = bSavePrtHist;
                DISPATCH = bDispatch;
                PRT_CNT = iPrtCnt;

                switch (iType)
                {
                    case THERMAL_PRT_TYPE.COM_OUT_RFID_GRP:
                        PrintGroupList(inData, iType);
                        break;
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD:
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET:
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD:
                    case THERMAL_PRT_TYPE.LAM_STK_RWK_GOOD_CELL:
                        PrintBasket(inData, iType);
                        break;
                    case THERMAL_PRT_TYPE.FOL_IN_REMAIN_MAGAZINE:
                    case THERMAL_PRT_TYPE.LAM_OUT_MAGAZINE:
                        PrintMagazine(inData, iType);
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void PrintGroupList(DataTable inData, THERMAL_PRT_TYPE iType)
        {
            try
            {
                var fd = (FlowDocument)null;

                fd = CreateFlowDocument_GRP_RFID(inData);

                if (fd != null)
                {
                    fd.PagePadding = new Thickness(5, 0, 5, 0);

                    PrintDialog dialog = new PrintDialog();

                    //dialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                    ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);


                    var paginatorSource = (IDocumentPaginatorSource)fd;
                    var paginator = paginatorSource.DocumentPaginator;

                    for (int i = 0; i < PRT_CNT; i++)
                    {
                        dialog.PrintDocument(paginator, "GMES PRINT");
                    }

                    if (SAVE_PRT_HIST)
                        SetLabelPrtHist(inData, iType);

                    if (DISPATCH)
                        SetDispatch(inData, iType);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintBasket(DataTable inData, THERMAL_PRT_TYPE iType)
        {
            try
            {
                if (inData == null || inData.Rows.Count < 1) return;

                for (int i = 0; i < inData.Rows.Count; i++)
                {
                    var fd = (FlowDocument)null;

                    switch (iType)
                    {
                        case THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD:
                            fd = CreateFlowDocument_BOX_NoBCD(inData.Rows[i]);
                            break;
                        case THERMAL_PRT_TYPE.LAM_STK_RWK_GOOD_CELL:
                            fd = CreateFlowDocument_RWK_BOX(inData.Rows[i]);
                            break;
                        case THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD:
                            fd = CreateFlowDocument_BOX_MI(inData.Rows[i]);
                            break;
                        default:
                            if (Common.LoginInfo.CFG_SHOP_ID.Equals("G182"))
                            {
                                if (PROCID.Equals(Process.STP))
                                    fd = CreateFlowDocument_BOX_STP(inData.Rows[i], b3DModel: GetWoProdInfo());
                                else
                                    fd = CreateFlowDocument_BOX_NJ(inData.Rows[i]);
                            }
                            else
                            {
                                fd = CreateFlowDocument_BOX(inData.Rows[i]);
                            }
                            break;
                    }

                    if (fd != null)
                    {
                        fd.PagePadding = new Thickness(5, 0, 5, 0);

                        PrintDialog dialog = new PrintDialog();

                        //dialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                        dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                        ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);


                        var paginatorSource = (IDocumentPaginatorSource)fd;
                        var paginator = paginatorSource.DocumentPaginator;

                        for (int j = 0; j < PRT_CNT; j++)
                        {
                            dialog.PrintDocument(paginator, "GMES PRINT");
                        }
                        
                    }
                }

                if (SAVE_PRT_HIST)
                    SetLabelPrtHist(inData, iType);

                if (DISPATCH)
                    SetDispatch(inData, iType);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintMagazine(DataTable inData, THERMAL_PRT_TYPE iType)
        {
            try
            {
                if (inData == null || inData.Rows.Count < 1) return;

                for (int i = 0; i < inData.Rows.Count; i++)
                {
                    var fd = (FlowDocument)null;

                    fd = CreateFlowDocument_MG(inData.Rows[i]);

                    if (fd != null)
                    {
                        fd.PagePadding = new Thickness(5, 0, 5, 0);

                        PrintDialog dialog = new PrintDialog();

                        //dialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                        dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                        ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);


                        var paginatorSource = (IDocumentPaginatorSource)fd;
                        var paginator = paginatorSource.DocumentPaginator;

                        for (int j = 0; j < PRT_CNT; j++)
                        {
                            dialog.PrintDocument(paginator, "GMES PRINT");
                        }

                    }
                }

                if (SAVE_PRT_HIST)
                    SetLabelPrtHist(inData, iType);

                if (DISPATCH)
                    SetDispatch(inData, iType);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Group 라벨 양식
        /// </summary>
        /// <param name="inData"></param>
        /// <returns></returns>
        private static FlowDocument CreateFlowDocument_GRP_RFID(DataTable inData)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(48, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });


                /* Row 1 */
                // Col 1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Col 2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br2.SetValue(Grid.RowProperty, 0);
                br2.SetValue(Grid.ColumnProperty, 1);
                br2.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(br2);

                /* Row 2 */
                // Col 1
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 1);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Col 2
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 1);
                br4.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br4);

                // Col 3
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 1);
                br5.SetValue(Grid.ColumnProperty, 2);
                br5.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br5);

                // Col 4
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br6.SetValue(Grid.RowProperty, 1);
                br6.SetValue(Grid.ColumnProperty, 4);
                g.Children.Add(br6);


                /* Row 3 */
                // Col 1
                var br20 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br20.SetValue(Grid.RowProperty, 2);
                br20.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br20);

                // Col 2
                var br21 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br21.SetValue(Grid.RowProperty, 2);
                br21.SetValue(Grid.ColumnProperty, 1);
                br21.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(br21);


                /* Row 4 */
                // Col 1
                var br22 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br22.SetValue(Grid.RowProperty, 3);
                br22.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br22);

                // Col 2
                var br23 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br23.SetValue(Grid.RowProperty, 3);
                br23.SetValue(Grid.ColumnProperty, 1);
                br23.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(br23);

                /* Row 5 */
                // Col 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br7.SetValue(Grid.RowProperty, 4);
                br7.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br7);

                // Col 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 4);
                br8.SetValue(Grid.ColumnProperty, 1);
                br8.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(br8);

                /* Row 6 */
                // Col 1
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br9.SetValue(Grid.RowProperty, 5);
                br9.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br9);

                // Col 2
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br10.SetValue(Grid.RowProperty, 5);
                br10.SetValue(Grid.ColumnProperty, 1);
                br10.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br10);

                // Col 3
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br11.SetValue(Grid.RowProperty, 5);
                br11.SetValue(Grid.ColumnProperty, 4);
                g.Children.Add(br11);


                /* Row 7 */
                // Col 1
                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 6);
                br12.SetValue(Grid.ColumnProperty, 0);
                br12.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br12);

                // Col 2
                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br13.SetValue(Grid.RowProperty, 6);
                br13.SetValue(Grid.ColumnProperty, 2);
                br13.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(br13);

                // Col 3
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 6);
                br14.SetValue(Grid.ColumnProperty, 4);
                g.Children.Add(br14);


                // 가변 항목.
                for (int i = 0; i < inData.Rows.Count; i++)
                {
                    g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                    // Col 1
                    var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                    br15.SetValue(Grid.RowProperty, i + 7);
                    br15.SetValue(Grid.ColumnProperty, 0);
                    br15.SetValue(Grid.ColumnSpanProperty, 2);
                    g.Children.Add(br15);

                    // Col 2
                    var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                    br16.SetValue(Grid.RowProperty, i + 7);
                    br16.SetValue(Grid.ColumnProperty, 2);
                    br16.SetValue(Grid.ColumnSpanProperty, 2);
                    g.Children.Add(br16);

                    // Col 3
                    var br17 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                    br17.SetValue(Grid.RowProperty, i + 7);
                    br17.SetValue(Grid.ColumnProperty, 4);
                    g.Children.Add(br17);

                    /* Value Set */
                    // CST ID
                    var tbCstID = new TextBlock() { Text = Util.NVC(inData.Rows[i]["CSTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                    tbCstID.SetValue(Grid.RowProperty, i + 7);
                    tbCstID.SetValue(Grid.ColumnProperty, 0);
                    tbCstID.SetValue(Grid.ColumnSpanProperty, 2);
                    g.Children.Add(tbCstID);

                    // LOT ID
                    var tbLotID = new TextBlock() { Text = Util.NVC(inData.Rows[i]["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                    tbLotID.SetValue(Grid.RowProperty, i + 7);
                    tbLotID.SetValue(Grid.ColumnProperty, 2);
                    tbLotID.SetValue(Grid.ColumnSpanProperty, 2);
                    g.Children.Add(tbLotID);

                    // Qty
                    var tbQty = new TextBlock() { Text = Util.NVC(inData.Rows[i]["WIPQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(inData.Rows[i]["WIPQTY"])).ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                    tbQty.SetValue(Grid.RowProperty, i + 7);
                    tbQty.SetValue(Grid.ColumnProperty, 4);
                    g.Children.Add(tbQty);
                }
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = PROCID.Equals(Process.STACKING_FOLDING) ? "Folding\r\nLot" : "Lami\r\nLot", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOT갯수")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("전체수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 2);
                t3.SetValue(Grid.RowProperty, 1);
                t3.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(t3);

                var t20 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("제품ID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t20.SetValue(Grid.ColumnProperty, 0);
                t20.SetValue(Grid.RowProperty, 2);
                g.Children.Add(t20);

                var t21 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("PJT명")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t21.SetValue(Grid.ColumnProperty, 0);
                t21.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t21);

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CSTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 6);
                t6.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 2);
                t7.SetValue(Grid.RowProperty, 6);
                t7.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(t7);

                var t8 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t8.SetValue(Grid.ColumnProperty, 4);
                t8.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t8);
                #endregion

                #region 3. Create TextBlock For Setting Value

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["PR_LOTID"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(tbFoldLot);

                // LOT 갯수
                var tbLotCount = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["LOT_CNT"]) : "0", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12, FontWeight = FontWeights.Bold };
                tbLotCount.SetValue(Grid.ColumnProperty, 1);
                tbLotCount.SetValue(Grid.RowProperty, 1);
                g.Children.Add(tbLotCount);

                // 총 수량
                var tbTotQty = new TextBlock() { Text = inData?.Rows.Count > 0 ? String.Format("{0:0,0}", Util.NVC(inData.Rows[0]["TOT_QTY"]).Equals("") ? 0 : double.Parse(Util.NVC(inData.Rows[0]["TOT_QTY"]))) : "0", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbTotQty.SetValue(Grid.ColumnProperty, 4);
                tbTotQty.SetValue(Grid.RowProperty, 1);
                g.Children.Add(tbTotQty);

                // 제품ID
                var tbProdID = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["PRODID"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbProdID.SetValue(Grid.ColumnProperty, 1);
                tbProdID.SetValue(Grid.RowProperty, 2);
                tbProdID.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(tbProdID);

                // PRJT
                var tbPRJT = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["PRJT_NAME"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbPRJT.SetValue(Grid.ColumnProperty, 1);
                tbPRJT.SetValue(Grid.RowProperty, 3);
                tbPRJT.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(tbPRJT);

                // 발행시간
                var tbCaldate = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["NOW_DTTM"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 4);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 4);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["EQPTSHORTNAME"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 5);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);

                // MK Type
                var tbMkType = new TextBlock() { Text = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["MKT_TYPE_NAME"]) : "", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbMkType.SetValue(Grid.ColumnProperty, 4);
                tbMkType.SetValue(Grid.RowProperty, 5);
                g.Children.Add(tbMkType);

                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 210 + ((double)inData?.Rows.Count * 30);

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 바구니 감열지 양식 (바구니 바코드 ID 없음.)
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <returns></returns>
        private static FlowDocument CreateFlowDocument_BOX_NoBCD(DataRow inDataRow)
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
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
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
                //br2.SetValue(Grid.RowSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 3
                var br31 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br31.SetValue(Grid.RowProperty, 2);
                br31.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br31);

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

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CSTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                //t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t31 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t31.SetValue(Grid.ColumnProperty, 0);
                t31.SetValue(Grid.RowProperty, 2);
                g.Children.Add(t31);

                var t3 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("작업일자")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("생성시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value

                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // 카세트ID            
                var tbBCD = new TextBlock() { Text = Util.NVC(inDataRow["CSTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                var tbLagreLot = new TextBlock() { Text = Util.NVC(inDataRow["CAL_DATE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                double dTmp = 0;
                double.TryParse(Util.NVC(inDataRow["WIPQTY"]), out dTmp);
                var tbQty = new TextBlock() { Text = dTmp.ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 6);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 210;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 바구니 감열지 양식
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <returns></returns>
        public static FlowDocument CreateFlowDocument_BOX(DataRow inDataRow)
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

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
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

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value
                
                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + Util.NVC(inDataRow["LOTID"]) + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                var tbLagreLot = new TextBlock() { Text = Util.NVC(inDataRow["CAL_DATE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = Util.NVC(inDataRow["WIPQTY"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
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

        /// <summary>
        /// STP 바구니 감열지 양식 (기존 소스 로직만 카피함. 정상 여부 테스트 필요)
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <param name="b3DModel"></param>
        /// <returns></returns>
        public static FlowDocument CreateFlowDocument_BOX_STP(DataRow inDataRow, bool b3DModel = false)
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

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
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

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 7);
                g.Children.Add(t7);

                var t8 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CELLTYPE")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t8.SetValue(Grid.ColumnProperty, 0);
                t8.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t8);

                #endregion

                #region 3. Create TextBlock For Setting Value
                
                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + Util.NVC(inDataRow["LOTID"]) + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                //var tbLagreLot = new TextBlock() { Text = sLagreLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                var tbLagreLot = new TextBlock() { Text = Util.NVC(inDataRow["CSTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = Util.NVC(inDataRow["WIPQTY"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 6);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 7);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);

                // Cell Type
                var tbCellType = new TextBlock() { Text = Util.NVC(inDataRow["CELLTYPE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbCellType.SetValue(Grid.ColumnProperty, 1);
                tbCellType.SetValue(Grid.RowProperty, 5);
                tbCellType.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbCellType);

                // 시장 유형
                var tbMktTypeCode = new TextBlock() { Text = Util.NVC(inDataRow["MKT_TYPE_CODE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10 };
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

        /// <summary>
        /// 남경 바구니 감열지 양식 (기존 소스 로직만 카피함. 정상 여부 테스트 필요)
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <returns></returns>
        public static FlowDocument CreateFlowDocument_BOX_NJ(DataRow inDataRow)
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

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
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

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value
                
                // Fold Lot
                var tbFoldLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + Util.NVC(inDataRow["LOTID"]) + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Basket ID
                var tbBasketID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Large Lot
                var tbLagreLot = new TextBlock() { Text = Util.NVC(inDataRow["CAL_DATE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbLagreLot.SetValue(Grid.ColumnProperty, 1);
                tbLagreLot.SetValue(Grid.RowProperty, 3);
                tbLagreLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLagreLot);

                // Qty
                var tbQty = new TextBlock() { Text = Util.NVC(inDataRow["WIPQTY"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 6);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbEqptNo);

                // 시장 유형
                var tbMktTypeCode = new TextBlock() { Text = Util.NVC(inDataRow["MKT_TYPE_CODE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 10 };
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

        /// <summary>
        /// CMI 바구니 감열지 양식 (고정식 바코드 적용, 바코드화된 CSTID 추가)
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <returns></returns>
        public static FlowDocument CreateFlowDocument_BOX_MI(DataRow inDataRow)
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

                // Row 2~3
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
                br15.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br15);
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CSTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = "BASKET\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 2);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                //MMD에 Created\r\nTime으로 등록했으나 ObjectDic을 통해 가져온 후에는 \\r, \\n으로 변하는 문제가 발생함.
                string CR_TIME_FOR_PRT = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CR_TIME_FOR_PRT"));
                CR_TIME_FOR_PRT = CR_TIME_FOR_PRT.Replace("\\r", "\r");
                CR_TIME_FOR_PRT = CR_TIME_FOR_PRT.Replace("\\n", "\n");
                var t6 = new TextBlock() { Text = CR_TIME_FOR_PRT, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                #endregion

                #region 3. Create TextBlock For Setting Value

                // Group Lot
                var tbGroupLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbGroupLot.SetValue(Grid.ColumnProperty, 1);
                tbGroupLot.SetValue(Grid.RowProperty, 0);
                tbGroupLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbGroupLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + Util.NVC(inDataRow["CSTID"]) + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // CSTID
                var tbCSTID = new TextBlock() { Text = Util.NVC(inDataRow["CSTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbCSTID.SetValue(Grid.ColumnProperty, 1);
                tbCSTID.SetValue(Grid.RowProperty, 2);
                tbCSTID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCSTID);

                // LOTID
                var tbLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbLot.SetValue(Grid.ColumnProperty, 1);
                tbLot.SetValue(Grid.RowProperty, 3);
                tbLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbLot);

                // Qty
                var tbQty = new TextBlock() { Text = Util.NVC(Util.NVC_Int(inDataRow["WIPQTY"])), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 3);
                tbModel.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbModel);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 5);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
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

        /// <summary>
        /// 매거진 감열지 양식
        /// </summary>
        /// <param name="inDataRow"></param>
        /// <returns></returns>
        public static FlowDocument CreateFlowDocument_MG(DataRow inDataRow)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(46, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(89, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
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
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br6.SetValue(Grid.RowProperty, 6);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);

                // Row 8
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br7.SetValue(Grid.RowProperty, 7);
                br7.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br7);


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

                // Row 4
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br11.SetValue(Grid.RowProperty, 3);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);

                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 0.5, 0.5) };
                br12.SetValue(Grid.RowProperty, 3);
                br12.SetValue(Grid.ColumnProperty, 2);
                g.Children.Add(br12);

                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 3);
                br13.SetValue(Grid.ColumnProperty, 3);
                g.Children.Add(br13);


                // Row 5
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 4);
                br14.SetValue(Grid.ColumnProperty, 1);
                br14.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br14);


                // Row 6
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br15.SetValue(Grid.RowProperty, 5);
                br15.SetValue(Grid.ColumnProperty, 1);
                br15.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br15);

                // Row 7
                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br16.SetValue(Grid.RowProperty, 6);
                br16.SetValue(Grid.ColumnProperty, 1);
                br16.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br16);

                // Row 8
                var br17 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br17.SetValue(Grid.RowProperty, 7);
                br17.SetValue(Grid.ColumnProperty, 1);
                br17.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(br17);
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = "MAGAZINE\r\nID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                t2.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("카세트ID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 2);
                t4.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("CELLTYPE")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                var t8 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("설비번호")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t8.SetValue(Grid.ColumnProperty, 0);
                t8.SetValue(Grid.RowProperty, 7);
                g.Children.Add(t8);
                #endregion

                #region 3. Create TextBlock For Setting Value
                
                // Lami Lot
                var tbFoldLot = new TextBlock() { Text = Util.NVC(inDataRow["LOTID_RT"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbFoldLot.SetValue(Grid.ColumnProperty, 1);
                tbFoldLot.SetValue(Grid.RowProperty, 0);
                tbFoldLot.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbFoldLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + Util.NVC(inDataRow["LOTID"]) + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBCD);

                // Magazine ID
                var tbBasketID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbBasketID.SetValue(Grid.ColumnProperty, 1);
                tbBasketID.SetValue(Grid.RowProperty, 2);
                tbBasketID.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbBasketID);

                // Qty
                var tbQty = new TextBlock() { Text = Convert.ToDouble(Util.NVC(inDataRow["WIPQTY"])).ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 3);
                g.Children.Add(tbQty);

                // Cst
                var tbCst = new TextBlock() { Text = Util.NVC(inDataRow["CSTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbCst.SetValue(Grid.ColumnProperty, 3);
                tbCst.SetValue(Grid.RowProperty, 3);
                g.Children.Add(tbCst);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 1);
                tbModel.SetValue(Grid.RowProperty, 4);
                tbModel.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbModel);

                // Cell Type
                var tbCellType = new TextBlock() { Text = Util.NVC(inDataRow["PRODUCT_LEVEL3_CODE"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbCellType.SetValue(Grid.ColumnProperty, 1);
                tbCellType.SetValue(Grid.RowProperty, 5);
                tbCellType.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCellType);

                // CalDate
                var tbCaldate = new TextBlock() { Text = Util.NVC(inDataRow["LOTDTTM_CR"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbCaldate.SetValue(Grid.ColumnProperty, 1);
                tbCaldate.SetValue(Grid.RowProperty, 6);
                tbCaldate.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbCaldate);

                // Eqpt No.
                var tbEqptNo = new TextBlock() { Text = Util.NVC(inDataRow["EQPTSHORTNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptNo.SetValue(Grid.ColumnProperty, 1);
                tbEqptNo.SetValue(Grid.RowProperty, 7);
                tbEqptNo.SetValue(Grid.ColumnSpanProperty, 3);
                g.Children.Add(tbEqptNo);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 255;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        public static FlowDocument CreateFlowDocument_RWK_BOX(DataRow inDataRow)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(195, GridUnitType.Pixel) });

                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });

                #region Column1
                #region Row1
                var br1 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 1, 0.5, 0.5) };
                br1.SetValue(Grid.RowProperty, 0);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);
                #endregion

                #region Row2
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);
                #endregion

                #region Row3
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 2);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);
                #endregion

                #region Row4
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 3);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);
                #endregion

                #region Row5
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br5.SetValue(Grid.RowProperty, 4);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);
                #endregion

                #region Row6
                var br6 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br6.SetValue(Grid.RowProperty, 5);
                br6.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br6);
                #endregion

                #region Row7
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br7.SetValue(Grid.RowProperty, 6);
                br7.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br7);
                #endregion

                #region Row8
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br8.SetValue(Grid.RowProperty, 7);
                br8.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br8);
                #endregion
                #endregion

                #region Column2
                #region Row1
                var br15 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br15.SetValue(Grid.RowProperty, 0);
                br15.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br15);
                #endregion

                #region Row2
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 1);
                br9.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br9);
                #endregion

                #region Row3
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 2);
                br10.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br10);
                #endregion

                #region Row4
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br11.SetValue(Grid.RowProperty, 3);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);
                #endregion

                #region Row5
                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br12.SetValue(Grid.RowProperty, 4);
                br12.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br12);
                #endregion

                #region Row6
                var br13 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br13.SetValue(Grid.RowProperty, 5);
                br13.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br13);
                #endregion

                #region Row7
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br14.SetValue(Grid.RowProperty, 6);
                br14.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br14);
                #endregion

                #region Row8
                var br16 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br16.SetValue(Grid.RowProperty, 7);
                br16.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br16);
                #endregion
                #endregion

                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("제품ID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 1);
                g.Children.Add(t2);

                var t3 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("EQSGNAME")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 2);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("PJT명")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);

                var t6 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t6.SetValue(Grid.ColumnProperty, 0);
                t6.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t6);

                var t7 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("생성시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t7.SetValue(Grid.ColumnProperty, 0);
                t7.SetValue(Grid.RowProperty, 6);
                g.Children.Add(t7);

                var t8 = new TextBlock() { Text = Util.NVC(Common.ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t8.SetValue(Grid.ColumnProperty, 0);
                t8.SetValue(Grid.RowProperty, 7);
                g.Children.Add(t8);
                #endregion

                #region 3. Create TextBlock For Setting Value

                // Lami Lot
                var tbLotID = new TextBlock() { Text = Util.NVC(inDataRow["LOTID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbLotID.SetValue(Grid.ColumnProperty, 1);
                tbLotID.SetValue(Grid.RowProperty, 0);
                g.Children.Add(tbLotID);

                // Barcode            
                var tbProdID = new TextBlock() { Text = Util.NVC(inDataRow["PRODID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbProdID.SetValue(Grid.ColumnProperty, 1);
                tbProdID.SetValue(Grid.RowProperty, 1);
                g.Children.Add(tbProdID);

                // Magazine ID
                var tbEqsgName = new TextBlock() { Text = Util.NVC(inDataRow["EQSGNAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbEqsgName.SetValue(Grid.ColumnProperty, 1);
                tbEqsgName.SetValue(Grid.RowProperty, 2);
                g.Children.Add(tbEqsgName);

                // Qty
                var tbQty = new TextBlock() { Text = Convert.ToDouble(Util.NVC(inDataRow["WIPQTY"])).ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 3);
                g.Children.Add(tbQty);

                // Cst
                var tbPrjtName = new TextBlock() { Text = Util.NVC(inDataRow["PRJT_NAME"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbPrjtName.SetValue(Grid.ColumnProperty, 1);
                tbPrjtName.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbPrjtName);

                // Model
                var tbModel = new TextBlock() { Text = Util.NVC(inDataRow["MODLID"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 1);
                tbModel.SetValue(Grid.RowProperty, 5);
                g.Children.Add(tbModel);

                // Cell Type
                var tbWipTime = new TextBlock() { Text = Util.NVC(inDataRow["WIPDTTM_IN"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbWipTime.SetValue(Grid.ColumnProperty, 1);
                tbWipTime.SetValue(Grid.RowProperty, 6);
                g.Children.Add(tbWipTime);

                // CalDate
                var tbPrntTime = new TextBlock() { Text = Util.NVC(inDataRow["PRNT_DTTM"]), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbPrntTime.SetValue(Grid.ColumnProperty, 1);
                tbPrntTime.SetValue(Grid.RowProperty, 7);
                g.Children.Add(tbPrntTime);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 255;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetLabelPrtHist(DataTable inData, THERMAL_PRT_TYPE iType)
        {
            try
            {
                if (inData == null || inData.Rows.Count < 1) return;

                string sBizName = "BR_PRD_REG_LABEL_PRINT_HIST";

                switch (iType)
                {                    
                    case THERMAL_PRT_TYPE.COM_OUT_RFID_GRP:
                        #region COM_OUT_RFID_GRP
                        sBizName = "BR_PRD_REG_GROUP_LABEL_PRINT_HIST";

                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("LABEL_CODE", typeof(string));
                        inTable.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                        inTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                        inTable.Columns.Add("PRT_ITEM01", typeof(string));
                        inTable.Columns.Add("PRT_ITEM02", typeof(string));
                        inTable.Columns.Add("PRT_ITEM03", typeof(string));
                        inTable.Columns.Add("PRT_ITEM04", typeof(string));
                        inTable.Columns.Add("PRT_ITEM05", typeof(string));
                        inTable.Columns.Add("PRT_ITEM06", typeof(string));
                        inTable.Columns.Add("PRT_ITEM07", typeof(string));
                        inTable.Columns.Add("PRT_ITEM08", typeof(string));
                        inTable.Columns.Add("PRT_ITEM09", typeof(string));
                        inTable.Columns.Add("PRT_ITEM10", typeof(string));
                        inTable.Columns.Add("PRT_ITEM11", typeof(string));
                        inTable.Columns.Add("PRT_ITEM12", typeof(string));
                        inTable.Columns.Add("PRT_ITEM13", typeof(string));
                        inTable.Columns.Add("PRT_ITEM14", typeof(string));
                        inTable.Columns.Add("PRT_ITEM15", typeof(string));
                        inTable.Columns.Add("PRT_ITEM16", typeof(string));
                        inTable.Columns.Add("PRT_ITEM17", typeof(string));
                        inTable.Columns.Add("PRT_ITEM18", typeof(string));
                        inTable.Columns.Add("PRT_ITEM19", typeof(string));
                        inTable.Columns.Add("PRT_ITEM20", typeof(string));
                        inTable.Columns.Add("PRT_ITEM21", typeof(string));
                        inTable.Columns.Add("PRT_ITEM22", typeof(string));
                        inTable.Columns.Add("PRT_ITEM23", typeof(string));
                        inTable.Columns.Add("PRT_ITEM24", typeof(string));
                        inTable.Columns.Add("PRT_ITEM25", typeof(string));
                        inTable.Columns.Add("PRT_ITEM26", typeof(string));
                        inTable.Columns.Add("PRT_ITEM27", typeof(string));
                        inTable.Columns.Add("PRT_ITEM28", typeof(string));
                        inTable.Columns.Add("PRT_ITEM29", typeof(string));
                        inTable.Columns.Add("PRT_ITEM30", typeof(string));
                        inTable.Columns.Add("INSUSER", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));

                        DataTable inLotTable = inDataSet.Tables.Add("IN_LOT");
                        inLotTable.Columns.Add("LOTID", typeof(string));
                        inLotTable.Columns.Add("WIPSEQ", typeof(string));
                        inLotTable.Columns.Add("CSTID", typeof(string));



                        DataRow newRow = null;

                        string sCstID = string.Empty;
                        string sLotID = string.Empty;
                        string sWipSeq = string.Empty;
                        string sQty = string.Empty;
                        for (int i = 0; i < inData.Rows.Count; i++)
                        {
                            if (sCstID.Length < 1)
                                sCstID = Util.NVC(inData.Rows[i]["CSTID"]);
                            else
                                sCstID = sCstID + "," + Util.NVC(inData.Rows[i]["CSTID"]);

                            if (sLotID.Length < 1)
                                sLotID = Util.NVC(inData.Rows[i]["LOTID"]);
                            else
                                sLotID = sLotID + "," + Util.NVC(inData.Rows[i]["LOTID"]);

                            if (sWipSeq.Length < 1)
                                sWipSeq = Util.NVC(inData.Rows[i]["WIPSEQ"]);
                            else
                                sWipSeq = sWipSeq + "," + Util.NVC(inData.Rows[i]["WIPSEQ"]);

                            if (sQty.Length < 1)
                                sQty = Util.NVC(inData.Rows[i]["WIPQTY"]);
                            else
                                sQty = sQty + "," + Util.NVC(inData.Rows[i]["WIPQTY"]);
                            

                            newRow = inLotTable.NewRow();
                            newRow["LOTID"] = Util.NVC(inData.Rows[i]["LOTID"]);
                            newRow["CSTID"] = Util.NVC(inData.Rows[i]["CSTID"]);
                            newRow["WIPSEQ"] = Util.NVC(inData.Rows[i]["WIPSEQ"]);

                            inLotTable.Rows.Add(newRow);
                        }

                        for (int i = 0; i < inData.Rows.Count; i++)
                        {
                            newRow = inTable.NewRow();

                            newRow["EQPTID"] = EQPTID;
                            //newRow["LABEL_CODE"] = "LBL0001";
                            //newRow["LABEL_ZPL_CNTT"] = sZPL;
                            newRow["LABEL_PRT_COUNT"] = PRT_CNT.ToString();                                                    // 발행수량
                            newRow["PRT_ITEM01"] = sLotID.Length >= 200 ? sLotID.Substring(0, 199) : sLotID;  // LOT 목록
                            newRow["PRT_ITEM02"] = 0;    // wipseq 목록
                            newRow["PRT_ITEM03"] = "GROUPING";                                                                   // Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)
                            newRow["PRT_ITEM04"] = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["LOT_CNT"]) : ""; // LOT 갯수
                            newRow["PRT_ITEM05"] = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["TOT_QTY"]) : "";        // 전체수량
                            newRow["PRT_ITEM06"] = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["NOW_DTTM"]) : "";        // 발행시간
                            newRow["PRT_ITEM07"] = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["EQPTSHORTNAME"]) : "";  // 설비번호
                            newRow["PRT_ITEM08"] = inData?.Rows.Count > 0 ? Util.NVC(inData.Rows[0]["MKT_TYPE_NAME"]) : "";            // 내/외수


                            newRow["PRT_ITEM09"] = sCstID.Length >= 200 ? sCstID.Substring(0, 199) : sCstID;    // CSTID
                            newRow["PRT_ITEM10"] = sLotID.Length >= 200 ? sLotID.Substring(0, 199) : sLotID;    // LOTID
                            newRow["PRT_ITEM11"] = sQty.Length >= 200 ? sQty.Substring(0, 199) : sQty;      // 수량
                            newRow["PRT_ITEM12"] = PROCID;      // 공정                                                                                                                     
                            newRow["PRT_ITEM13"] = EQSGID;      // 라인                                                                                                                        
                            newRow["PRT_ITEM14"] = EQPTID;      // 설비
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
                            newRow["INSUSER"] = Common.LoginInfo.USERID;
                            newRow["LOTID"] = Util.NVC(inData.Rows[i]["LOTID"]);

                            inTable.Rows.Add(newRow);
                        }
                                                
                        new Common.ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,IN_LOT", null, inDataSet);
                        #endregion
                        break;                    
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD:
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET:
                    case THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD:
                        #region FOL_OUT_BASKET
                        inTable = new DataTable();

                        inTable.Columns.Add("LABEL_CODE", typeof(string));
                        inTable.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                        inTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                        inTable.Columns.Add("PRT_ITEM01", typeof(string));
                        inTable.Columns.Add("PRT_ITEM02", typeof(string));
                        inTable.Columns.Add("PRT_ITEM03", typeof(string));
                        inTable.Columns.Add("PRT_ITEM04", typeof(string));
                        inTable.Columns.Add("PRT_ITEM05", typeof(string));
                        inTable.Columns.Add("PRT_ITEM06", typeof(string));
                        inTable.Columns.Add("PRT_ITEM07", typeof(string));
                        inTable.Columns.Add("PRT_ITEM08", typeof(string));
                        inTable.Columns.Add("PRT_ITEM09", typeof(string));
                        inTable.Columns.Add("PRT_ITEM10", typeof(string));
                        inTable.Columns.Add("PRT_ITEM11", typeof(string));
                        inTable.Columns.Add("PRT_ITEM12", typeof(string));
                        inTable.Columns.Add("PRT_ITEM13", typeof(string));
                        inTable.Columns.Add("PRT_ITEM14", typeof(string));
                        inTable.Columns.Add("PRT_ITEM15", typeof(string));
                        inTable.Columns.Add("PRT_ITEM16", typeof(string));
                        inTable.Columns.Add("PRT_ITEM17", typeof(string));
                        inTable.Columns.Add("PRT_ITEM18", typeof(string));
                        inTable.Columns.Add("PRT_ITEM19", typeof(string));
                        inTable.Columns.Add("PRT_ITEM20", typeof(string));
                        inTable.Columns.Add("PRT_ITEM21", typeof(string));
                        inTable.Columns.Add("PRT_ITEM22", typeof(string));
                        inTable.Columns.Add("PRT_ITEM23", typeof(string));
                        inTable.Columns.Add("PRT_ITEM24", typeof(string));
                        inTable.Columns.Add("PRT_ITEM25", typeof(string));
                        inTable.Columns.Add("PRT_ITEM26", typeof(string));
                        inTable.Columns.Add("PRT_ITEM27", typeof(string));
                        inTable.Columns.Add("PRT_ITEM28", typeof(string));
                        inTable.Columns.Add("PRT_ITEM29", typeof(string));
                        inTable.Columns.Add("PRT_ITEM30", typeof(string));
                        inTable.Columns.Add("INSUSER", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < inData.Rows.Count; i++)
                        {
                            newRow = inTable.NewRow();
                            //newRow["LABEL_CODE"] = "LBL0001";
                            //newRow["LABEL_ZPL_CNTT"] = sZPL;
                            newRow["LABEL_PRT_COUNT"] = PRT_CNT.ToString();                                                    // 발행수량
                            newRow["PRT_ITEM01"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";
                            newRow["PRT_ITEM02"] = inData.Columns.Contains("WIPSEQ") ? Util.NVC(inData.Rows[i]["WIPSEQ"]) : "";
                            newRow["PRT_ITEM03"] = "BOX";                                                                   // Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)
                            newRow["PRT_ITEM04"] = inData.Columns.Contains("RE_PRT_YN") ? Util.NVC(inData.Rows[i]["RE_PRT_YN"]) : "N"; // 재발행 여부
                            newRow["PRT_ITEM05"] = inData.Columns.Contains("LOTID_RT") ? Util.NVC(inData.Rows[i]["LOTID_RT"]) : "";        // Folding Lot
                            newRow["PRT_ITEM06"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";        // Box ID
                            newRow["PRT_ITEM07"] = inData.Columns.Contains("CAL_DATE") ? Util.NVC(inData.Rows[i]["CAL_DATE"]) : "";  // 폴딩 LOT의 생성시간(공장시간기준)
                            newRow["PRT_ITEM08"] = inData.Columns.Contains("WIPQTY") ? Util.NVC(inData.Rows[i]["WIPQTY"]) : "";            // 수량
                            newRow["PRT_ITEM09"] = inData.Columns.Contains("MODLID") ? Util.NVC(inData.Rows[i]["MODLID"]) : "";        // 모델
                            newRow["PRT_ITEM10"] = inData.Columns.Contains("LOTDTTM_CR") ? Util.NVC(inData.Rows[i]["LOTDTTM_CR"]) : "";    // 발행시간
                            newRow["PRT_ITEM11"] = inData.Columns.Contains("EQPTSHORTNAME") ? Util.NVC(inData.Rows[i]["EQPTSHORTNAME"]) : "";      // 설비번호
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
                            newRow["INSUSER"] = Common.LoginInfo.USERID;
                            newRow["LOTID"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";

                            inTable.Rows.Add(newRow);
                        }
                        
                        new Common.ClientProxy().ExecuteServiceSync(sBizName, "INDATA", null, inTable);
                        #endregion
                        break;
                    case THERMAL_PRT_TYPE.LAM_OUT_MAGAZINE:
                    case THERMAL_PRT_TYPE.FOL_IN_REMAIN_MAGAZINE:
                        #region LAM_OUT_MAGAZINE
                        inTable = new DataTable();

                        inTable.Columns.Add("LABEL_CODE", typeof(string));
                        inTable.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                        inTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                        inTable.Columns.Add("PRT_ITEM01", typeof(string));
                        inTable.Columns.Add("PRT_ITEM02", typeof(string));
                        inTable.Columns.Add("PRT_ITEM03", typeof(string));
                        inTable.Columns.Add("PRT_ITEM04", typeof(string));
                        inTable.Columns.Add("PRT_ITEM05", typeof(string));
                        inTable.Columns.Add("PRT_ITEM06", typeof(string));
                        inTable.Columns.Add("PRT_ITEM07", typeof(string));
                        inTable.Columns.Add("PRT_ITEM08", typeof(string));
                        inTable.Columns.Add("PRT_ITEM09", typeof(string));
                        inTable.Columns.Add("PRT_ITEM10", typeof(string));
                        inTable.Columns.Add("PRT_ITEM11", typeof(string));
                        inTable.Columns.Add("PRT_ITEM12", typeof(string));
                        inTable.Columns.Add("PRT_ITEM13", typeof(string));
                        inTable.Columns.Add("PRT_ITEM14", typeof(string));
                        inTable.Columns.Add("PRT_ITEM15", typeof(string));
                        inTable.Columns.Add("PRT_ITEM16", typeof(string));
                        inTable.Columns.Add("PRT_ITEM17", typeof(string));
                        inTable.Columns.Add("PRT_ITEM18", typeof(string));
                        inTable.Columns.Add("PRT_ITEM19", typeof(string));
                        inTable.Columns.Add("PRT_ITEM20", typeof(string));
                        inTable.Columns.Add("PRT_ITEM21", typeof(string));
                        inTable.Columns.Add("PRT_ITEM22", typeof(string));
                        inTable.Columns.Add("PRT_ITEM23", typeof(string));
                        inTable.Columns.Add("PRT_ITEM24", typeof(string));
                        inTable.Columns.Add("PRT_ITEM25", typeof(string));
                        inTable.Columns.Add("PRT_ITEM26", typeof(string));
                        inTable.Columns.Add("PRT_ITEM27", typeof(string));
                        inTable.Columns.Add("PRT_ITEM28", typeof(string));
                        inTable.Columns.Add("PRT_ITEM29", typeof(string));
                        inTable.Columns.Add("PRT_ITEM30", typeof(string));
                        inTable.Columns.Add("INSUSER", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < inData.Rows.Count; i++)
                        {
                            newRow = inTable.NewRow();
                            //newRow["LABEL_CODE"] = "LBL0001";
                            //newRow["LABEL_ZPL_CNTT"] = sZPL;
                            newRow["LABEL_PRT_COUNT"] = PRT_CNT.ToString();                                                    // 발행수량
                            newRow["PRT_ITEM01"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";
                            newRow["PRT_ITEM02"] = inData.Columns.Contains("WIPSEQ") ? Util.NVC(inData.Rows[i]["WIPSEQ"]) : "";
                            newRow["PRT_ITEM03"] = iType == THERMAL_PRT_TYPE.FOL_IN_REMAIN_MAGAZINE ? "MAGAZINE_REMAIN" : "MAGAZINE";                                                                   // Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)
                            newRow["PRT_ITEM04"] = inData.Columns.Contains("RE_PRT_YN") ? Util.NVC(inData.Rows[i]["RE_PRT_YN"]) : ""; // 재발행 여부
                            newRow["PRT_ITEM05"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";        // Lami Lot
                            newRow["PRT_ITEM06"] = inData.Columns.Contains("MAGID") ? Util.NVC(inData.Rows[i]["MAGID"]) : "";        // Magazine ID    
                            newRow["PRT_ITEM07"] = inData.Columns.Contains("QTY") ? Util.NVC(inData.Rows[i]["QTY"]) : "";            // Qty
                            newRow["PRT_ITEM08"] = inData.Columns.Contains("CASTID") ? Util.NVC(inData.Rows[i]["CASTID"]) : "";      // 카세트 ID
                            newRow["PRT_ITEM09"] = inData.Columns.Contains("MODEL") ? Util.NVC(inData.Rows[i]["MODEL"]) : "";        // 모델
                            newRow["PRT_ITEM10"] = inData.Columns.Contains("CELLTYPE") ? Util.NVC(inData.Rows[i]["CELLTYPE"]) : "";  // BICELL TYPE
                            newRow["PRT_ITEM11"] = inData.Columns.Contains("REGDATE") ? Util.NVC(inData.Rows[i]["REGDATE"]) : "";    // 발행시간
                            newRow["PRT_ITEM12"] = inData.Columns.Contains("EQPTNO") ? Util.NVC(inData.Rows[i]["EQPTNO"]) : "";      // 설비 번호

                            if (Common.LoginInfo.CFG_SHOP_ID.Equals("G182"))
                            {
                                newRow["PRT_ITEM13"] = inData.Columns.Contains("MKT_TYPE_CODE") ? Util.NVC(inData.Rows[i]["MKT_TYPE_CODE"]) : "";      // 설비 번호
                            }
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
                            newRow["INSUSER"] = Common.LoginInfo.USERID;
                            newRow["LOTID"] = inData.Columns.Contains("LOTID") ? Util.NVC(inData.Rows[i]["LOTID"]) : "";

                            inTable.Rows.Add(newRow);
                        }
                        
                        new Common.ClientProxy().ExecuteServiceSync(sBizName, "INDATA", null, inTable);
                        
                        #endregion
                        break;
                    default:
                        sBizName = "BR_PRD_REG_LABEL_PRINT_HIST";
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
        }

        private void SetDispatch(DataTable inData, THERMAL_PRT_TYPE iType)
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                //inDataTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("REWORK", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable input_LOT = indataSet.Tables.Add("INLOT");
                input_LOT.Columns.Add("LOTID", typeof(string));
                input_LOT.Columns.Add("ACTQTY", typeof(double));
                input_LOT.Columns.Add("ACTUQTY", typeof(double));
                input_LOT.Columns.Add("WIPNOTE", typeof(string));
                

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["EQSGID"] = EQSGID;
                newRow["REWORK"] = "N";
                newRow["USERID"] = Common.LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < inData.Rows.Count; i++)
                {
                    if (Util.NVC(inData.Rows[i]["DISPATCH_YN"]).Equals("Y")) continue;

                    newRow = input_LOT.NewRow();
                    newRow["LOTID"] = Util.NVC(inData.Rows[i]["LOTID"]);
                    newRow["ACTQTY"] = Util.NVC(inData.Rows[i]["WIPQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(inData.Rows[i]["WIPQTY"]));
                    newRow["ACTUQTY"] = 0;
                    newRow["WIPNOTE"] = "";

                    input_LOT.Rows.Add(newRow);
                }

                if (input_LOT.Rows.Count < 1)
                    return;

                new Common.ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetWoProdInfo()
        {
            try
            {
                bool b3DModel = false;

                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new Common.ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_INFO_BY_WO", "INDATA", "OUTDATA", inTable);

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

    public enum THERMAL_PRT_TYPE
    {
        COM_OUT_RFID_GRP,   // RFID 그룹 발행 
        FOL_OUT_BASKET,     // 기본 폴딩 BOX 발행
        FOL_OUT_BASKET_NO_BCD,  // 기본 폴딩 Box 발행 (LOT ID 바코드 없음.)
        LAM_OUT_MAGAZINE,       // 기본 라미 Maz 발행
        LAM_IN_REMAIN_PANCAKE,  // 기본 라미 잔량 Pancake 발행
        FOL_IN_REMAIN_MAGAZINE,  // 폴딩 매거진 잔량 발행
        LAM_STK_RWK_GOOD_CELL, //L&S재작업을 위한 추가작업
        FOL_OUT_BASKET_CARRIER_BCD //기본 폴딩 BOX 발행 (Carrier ID 바코드만 있음.)
    }
}
