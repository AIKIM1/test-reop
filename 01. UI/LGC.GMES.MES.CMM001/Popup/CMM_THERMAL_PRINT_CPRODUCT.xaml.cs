/*************************************************************************************
 Created Date : 2017.08.13
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - C 생산 감열지 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.13  INS 김동일K : Initial Created.
  
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_THERMAL_PRINT_CPRODUCT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_CPRODUCT : C1Window, IWorkArea
    {
        private int iCnt = 1;

        private List<Dictionary<string, string>> _dicList;
        private string _ProcID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ShowMsg = string.Empty;
        private string _PrintType = string.Empty;

        BizDataSet _Biz = new BizDataSet();        

        Dictionary<string, string> dicParam;

        string _sPGM_ID = "CMM_THERMAL_PRINT_CPRODUCT";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_CPRODUCT()
        {
            InitializeComponent();
        }

        public CMM_THERMAL_PRINT_CPRODUCT(Dictionary<string, string> dic)
        {
            InitializeComponent();

            dicParam = dic;
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
                    _PrintType = Util.NVC(tmps[5]);
                }
                else
                {
                    _dicList = null;
                    _ProcID = "";
                    _EqsgID = "";
                    _EqptID = "";
                    _ShowMsg = "Y";
                    _PrintType = "M";
                }
                
                PrintUseFlowdocument();
                
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
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
                    if ( _ShowMsg.Equals("Y") )
                    {
                        Util.MessageInfo("SFU1236");
                    }
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
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(90, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150, GridUnitType.Pixel) });
                
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
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
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 1, 0.5) };
                br2.SetValue(Grid.RowProperty, 1);
                br2.SetValue(Grid.ColumnSpanProperty, 2);
                br2.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br2);

                // Row 3
                var br3 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br3.SetValue(Grid.RowProperty, 2);
                br3.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br3);

                // Row 4
                var br4 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br4.SetValue(Grid.RowProperty, 3);
                br4.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br4);

                // Row 5
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br5.SetValue(Grid.RowProperty, 4);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);
                

                /* Column 2 */
                // Row 1
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 0);
                br8.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br8);
                
                // Row 3
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 2);
                br10.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br10);

                // Row 4
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br11.SetValue(Grid.RowProperty, 3);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);
                                
                // Row 5
                var br14 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br14.SetValue(Grid.RowProperty, 4);
                br14.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br14);

                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("C생산LOTID")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                g.Children.Add(t1);
                

                var t3 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("라인")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 2);
                g.Children.Add(t3);

                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("수량")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t5);
                
                #endregion

                #region 3. Create TextBlock For Setting Value

                string sCProdLot = "";
                string sLine = "";
                string sEqpt = "";
                string sQty = "";
                
                if (dic.ContainsKey("LOTID")) sCProdLot = dic["LOTID"];
                if (dic.ContainsKey("QTY")) sQty = dic["QTY"];
                if (dic.ContainsKey("EQSGNAME")) sLine = dic["EQSGNAME"];
                if (dic.ContainsKey("EQPTNAME")) sEqpt = dic["EQPTNAME"];
                

                // C 생산 Lot
                var tbCProdLot = new TextBlock() { Text = sCProdLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbCProdLot.SetValue(Grid.ColumnProperty, 1);
                tbCProdLot.SetValue(Grid.RowProperty, 0);
                g.Children.Add(tbCProdLot);

                // Barcode            
                var tbBCD = new TextBlock() { Text = "*" + sCProdLot + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 0);
                tbBCD.SetValue(Grid.RowProperty, 1);
                tbBCD.SetValue(Grid.ColumnSpanProperty, 2);
                g.Children.Add(tbBCD);

                // Line
                var tbLine = new TextBlock() { Text = sLine, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9 };
                tbLine.SetValue(Grid.ColumnProperty, 1);
                tbLine.SetValue(Grid.RowProperty, 2);
                g.Children.Add(tbLine);

                // Qty
                var tbQty = new TextBlock() { Text = sQty, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbQty.SetValue(Grid.ColumnProperty, 1);
                tbQty.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbQty);

                // 설비
                var tbEqpt = new TextBlock() { Text = sEqpt, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9 };
                tbEqpt.SetValue(Grid.ColumnProperty, 1);
                tbEqpt.SetValue(Grid.RowProperty, 3);
                g.Children.Add(tbEqpt);
                
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                fd.PageHeight = 165;

                return fd;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        private void SetLabelPrtHist()
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = iCnt.ToString();                                                    // 발행 수량
                newRow["PRT_ITEM01"] = dicParam.ContainsKey("LOTID") ? Util.NVC(dicParam["LOTID"]) : "";
                newRow["PRT_ITEM02"] = "1";
                newRow["PRT_ITEM03"] = _PrintType;
                newRow["PRT_ITEM04"] = dicParam.ContainsKey("RE_PRT_YN") ? Util.NVC(dicParam["RE_PRT_YN"]) : ""; // 재발행 여부
                newRow["PRT_ITEM05"] = dicParam.ContainsKey("LOTID") ? Util.NVC(dicParam["LOTID"]) : "";
                newRow["PRT_ITEM06"] = dicParam.ContainsKey("EQSGNAME") ? Util.NVC(dicParam["EQSGNAME"]) : "";
                newRow["PRT_ITEM07"] = dicParam.ContainsKey("EQPTNAME") ? Util.NVC(dicParam["EQPTNAME"]) : "";
                newRow["PRT_ITEM08"] = dicParam.ContainsKey("QTY") ? Util.NVC(dicParam["QTY"]) : "";            // Qty
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
                newRow["LOTID"] = dicParam.ContainsKey("LOTID") ? Util.NVC(dicParam["LOTID"]) : "";
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = "BR_PRD_REG_LABEL_PRINT_HIST";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
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

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Focus();
        }
    }
}
