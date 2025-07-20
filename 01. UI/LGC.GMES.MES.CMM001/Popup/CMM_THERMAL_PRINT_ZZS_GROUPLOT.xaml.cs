/*************************************************************************************
 Created Date : 2021.07.21
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.21  이형대    : Initial Created.  (reference CMM_THERMAL_PRINT_FOLD.xaml)
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
    /// CMM_THERMAL_PRINT_ZZS_GROUPLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_ZZS_GROUPLOT : C1Window, IWorkArea
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

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_ZZS_GROUPLOT()
        {
            InitializeComponent();
        }

        public CMM_THERMAL_PRINT_ZZS_GROUPLOT(Dictionary<string, string> dic)
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
                    _Dispatch = "N"; // 기본 디스패치 처리 안함.
                }

                //this.Hide();
                
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

                        var fd = (FlowDocument)null;

                        
                        //ZZS의 GROUPLOT 프린트 양식 제작코드
                        fd = CreateFlowDocumentZZS_GROUPLOT(dicParam);


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
                            
                            // 20210721 Dispatch와 발행이력은 관리 하지 않음
                            ////if (_Dispatch.Equals("Y"))
                            ////    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));
                            ////SetLabelPrtHist();
                        }
                    }

                    //인쇄 완료 되었습니다.
                    //INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfoAutoClosing("SFU1236");
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public static FlowDocument CreateFlowDocumentZZS_GROUPLOT(Dictionary<string, string> dic)
        {
            try
            {
                FlowDocument fd = new FlowDocument();

                Grid g = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

                #region 1. Make Grid Format
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(195, GridUnitType.Pixel) });
                
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
                br1.SetValue(Grid.RowSpanProperty, 2);
                br1.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br1);

                // Row 3
                var br2 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 0.5) };
                br2.SetValue(Grid.RowProperty, 2);                
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
                var br5 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(1, 0.5, 0.5, 1) };
                br5.SetValue(Grid.RowProperty, 5);
                br5.SetValue(Grid.ColumnProperty, 0);
                g.Children.Add(br5);
                
                /* Column 2 */
                // Row 1
                var br7 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 1, 1, 0.5) };
                br7.SetValue(Grid.RowProperty, 0);
                br7.SetValue(Grid.ColumnProperty, 1);                
                g.Children.Add(br7);

                // Row 2
                var br8 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br8.SetValue(Grid.RowProperty, 1);
                br8.SetValue(Grid.ColumnProperty, 1);                
                g.Children.Add(br8);

                // Row 3
                var br9 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br9.SetValue(Grid.RowProperty, 2);
                br9.SetValue(Grid.ColumnProperty, 1);                
                g.Children.Add(br9);

                // Row 4
                var br10 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br10.SetValue(Grid.RowProperty, 3);
                br10.SetValue(Grid.ColumnProperty, 1);                
                g.Children.Add(br10);

                // Row 5
                var br11 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 0.5) };
                br11.SetValue(Grid.RowProperty, 4);
                br11.SetValue(Grid.ColumnProperty, 1);
                g.Children.Add(br11);                

                // Row 6
                var br12 = new Border() { Background = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0.5, 0.5, 1, 1) };
                br12.SetValue(Grid.RowProperty, 5);
                br12.SetValue(Grid.ColumnProperty, 1);                
                g.Children.Add(br12);
                                
                #endregion

                #region 2. Set Item Title

                var t1 = new TextBlock() { Text = "GROUP\r\nLOT ID", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t1.SetValue(Grid.ColumnProperty, 0);
                t1.SetValue(Grid.RowProperty, 0);
                t1.SetValue(Grid.RowSpanProperty, 2);
                g.Children.Add(t1);

                var t2 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("작업일")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t2.SetValue(Grid.ColumnProperty, 0);
                t2.SetValue(Grid.RowProperty, 2);                
                g.Children.Add(t2);
                
                var t3 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("모델")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t3.SetValue(Grid.ColumnProperty, 0);
                t3.SetValue(Grid.RowProperty, 3);
                g.Children.Add(t3);
                
                var t4 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("발행시간")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t4.SetValue(Grid.ColumnProperty, 0);
                t4.SetValue(Grid.RowProperty, 4);
                g.Children.Add(t4);

                var t5 = new TextBlock() { Text = Util.NVC(ObjectDic.Instance.GetObjectName("설비")), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 9, FontWeight = FontWeights.Bold };
                t5.SetValue(Grid.ColumnProperty, 0);
                t5.SetValue(Grid.RowProperty, 5);
                g.Children.Add(t5);

                #endregion

                #region 3. Create TextBlock For Setting Value

                string sGroupLot = string.Empty;
                string sGroupLotBCD = string.Empty;
                string sCalDate = string.Empty;
                string sModel = string.Empty;
                string sRegDate = string.Empty;
                string sEqptName = string.Empty;

                if (dic.ContainsKey("GROUPLOTID")) sGroupLot = dic["GROUPLOTID"];
                if (dic.ContainsKey("GROUPLOTIDBARCODE")) sGroupLotBCD = dic["GROUPLOTIDBARCODE"];
                if (dic.ContainsKey("CALDATE")) sCalDate = dic["CALDATE"];
                if (dic.ContainsKey("MODEL")) sModel = dic["MODEL"];
                if (dic.ContainsKey("REGDATE")) sRegDate = dic["REGDATE"];
                if (dic.ContainsKey("EQPTNAME")) sEqptName = dic["EQPTNAME"];


                // Barcode ( 폰트명칭 주의 )
                var tbBCD = new TextBlock() { Text = "*" + sGroupLotBCD + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Bar-Code 39"), FontSize = 24 };
                //var tbBCD = new TextBlock() { Text = "*" + sGroupLotBCD + "*", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("3 of 9 Barcode"), FontSize = 24 };
                tbBCD.SetValue(Grid.ColumnProperty, 1);
                tbBCD.SetValue(Grid.RowProperty, 0);                
                g.Children.Add(tbBCD);

                // Group LotID
                var tbGroupLotID = new TextBlock() { Text = sGroupLot, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 16, FontWeight = FontWeights.Bold };
                tbGroupLotID.SetValue(Grid.ColumnProperty, 1);
                tbGroupLotID.SetValue(Grid.RowProperty, 1);                
                g.Children.Add(tbGroupLotID);

                // Insert DTTM
                var tbCalDate = new TextBlock() { Text = sCalDate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbCalDate.SetValue(Grid.ColumnProperty, 1);
                tbCalDate.SetValue(Grid.RowProperty, 2);                
                g.Children.Add(tbCalDate);
                                
                // Model
                var tbModel = new TextBlock() { Text = sModel, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 12 };
                tbModel.SetValue(Grid.ColumnProperty, 1);
                tbModel.SetValue(Grid.RowProperty, 3);
                g.Children.Add(tbModel);

                // RegDate
                var tbRegDate = new TextBlock() { Text = sRegDate, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbRegDate.SetValue(Grid.ColumnProperty, 1);
                tbRegDate.SetValue(Grid.RowProperty, 4);
                g.Children.Add(tbRegDate);

                // Eqpt Name
                var tbEqptName = new TextBlock() { Text = sEqptName, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontFamily = new FontFamily("Gulim"), FontSize = 12 };
                tbEqptName.SetValue(Grid.ColumnProperty, 1);
                tbEqptName.SetValue(Grid.RowProperty, 5);
                g.Children.Add(tbEqptName);
                #endregion

                fd.Blocks.Add(new BlockUIContainer(g));

                fd.PageWidth = 260;
                //fd.PageHeight = 225;
                fd.PageHeight = 195;

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

                if (dicParam.ContainsKey("GROUPLOTID")) GROUPLOTID.Text = dicParam["GROUPLOTID"];
                if (dicParam.ContainsKey("GROUPLOTIDBARCODE")) GROUPLOTIDBCD.Text = dicParam["GROUPLOTIDBARCODE"];
                if (dicParam.ContainsKey("CALDATE")) CALDATE.Text = dicParam["CALDATE"];                
                if (dicParam.ContainsKey("MODEL")) MODEL.Text = dicParam["MODEL"];
                if (dicParam.ContainsKey("REGDATE")) REGDATE.Text = dicParam["REGDATE"];
                if (dicParam.ContainsKey("EQPTNAME")) EQPTNAME.Text = dicParam["EQPTNAME"];
                
                grFoldPrint.Margin = new Thickness(0, 0, 0, 0);

                if (dicParam.ContainsKey("PRINTQTY"))
                    iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
    }
}
