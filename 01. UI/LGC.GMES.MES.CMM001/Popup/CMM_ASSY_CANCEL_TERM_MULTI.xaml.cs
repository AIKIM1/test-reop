/*************************************************************************************
 Created Date : 2019.02.18
      Creator : 이진선
   Decription : 다중 투입 LOT 종료 취소 + 발행
--------------------------------------------------------------------------------------
 [Change History]

  2020.05.27  김동일 : C20200513-000349 재고 및 수율 정합성 향상을 위한 투입Lot 종료 취소에 대한 기능변경

**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CANCEL_TERM_MULTI : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private int iCnt = 1;
        private string _ProcID = string.Empty;

        private bool bViewMsg = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        Dictionary<string, string> dicParam;

        string _sPGM_ID = "CMM_ASSY_CANCEL_TERM_MULTI";

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSY_CANCEL_TERM_MULTI()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {                    
                    _ProcID = Util.NVC(tmps[0]);
                }
                else
                {                    
                    _ProcID = "";
                }

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetTermLotInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bViewMsg) return;

                if (!CanCancelTerm())
                    return;
                
                Util.MessageConfirm("SFU1887", (result) =>// 종료취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelTermLot();
                    }
                });                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetTermLotInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = _Biz.GetDA_PRD_SEL_CANCEL_TERMINATE();

                DataRow newRow = inTable.NewRow();                
                newRow["PROCID"] = _ProcID;
                newRow["LOTID"] = txtLotID.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_TERMINATE_CMM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Util.GridSetData(dgLotInfo, searchResult, null, false);

                        if (searchResult != null && searchResult.Rows.Count < 1)
                            Util.MessageValidation("SFU2885", txtLotID.Text);    // {0} 은 해당 공정에 투입LOT 중 종료된 정보가 없습니다.
                        else
                        {
                            if (!dgLotInfo.Columns.Contains("WIPQTY2_ST"))
                                return;



                            if (dgLotInfo.GetRowCount() == 0)
                            {
                                Util.GridSetData(dgLotInfo, searchResult, FrameOperation, true);
                            }
                            else
                            {
                                DataTable dtSrc = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                                dtSrc.PrimaryKey = new DataColumn[] { dtSrc.Columns["LOTID"] };
                                searchResult.PrimaryKey = new DataColumn[] { searchResult.Columns["LOTID"] };

                                dtSrc.Merge(searchResult);

                                Util.GridSetData(dgLotInfo, dtSrc, FrameOperation, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }

        private DataSet GetBR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("IFTYPE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));


            DataTable in_DATA = indataSet.Tables.Add("INLOT");
            in_DATA.Columns.Add("LOTID", typeof(string));
            in_DATA.Columns.Add("LOTSTAT", typeof(string));
            in_DATA.Columns.Add("WIPQTY", typeof(int));
            in_DATA.Columns.Add("WIPQTY2", typeof(int));
            in_DATA.Columns.Add("INPUT_SEQNO", typeof(string));

            return indataSet;
        }

        private void SetLabelPrtHist()
        {
            try
            {
                string sBizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST";

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
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

        private void CancelTermLot()
        {
            loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                DataSet indataSet = GetBR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;

                newRow["PROCID"] = _ProcID;
                newRow["IFTYPE"] = "NORMAL"; //임의로 설정한 값임 Biz 확인
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable in_DATA = indataSet.Tables["INLOT"];

                for (int inx = 0; inx < dgLotInfo.GetRowCount(); inx++)
                {
                    newRow = in_DATA.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[inx].DataItem, "LOTID"));
                    newRow["LOTSTAT"] = "RELEASED";
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[inx].DataItem, "WIPQTY2_ST")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[inx].DataItem, "WIPQTY2_ST")));
                    newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[inx].DataItem, "WIPQTY2_ST")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[inx].DataItem, "WIPQTY2_ST")));
                    in_DATA.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK_MULTI", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable result = bizResult.Tables["OUTDATA"];

                        if (result.Rows.Count != 0)
                        {
                            for (int jnx = 0; jnx < dgLotInfo.GetRowCount(); jnx++)
                            {
                                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                                PrintDialog dialog = new PrintDialog();
                                dicParam = new Dictionary<string, string>();

                                dicParam.Add("reportName", "Fold");
                                dicParam.Add("LOTID", Util.NVC(result.Rows[jnx]["PRT_ITEM05"]));
                                dicParam.Add("QTY", Convert.ToDouble(Util.NVC(result.Rows[jnx]["PRT_ITEM08"])).ToString());
                                dicParam.Add("MAGID", Util.NVC(result.Rows[jnx]["PRT_ITEM01"]));
                                dicParam.Add("MAGIDBARCODE", Util.NVC(result.Rows[jnx]["PRT_ITEM01"]));
                                dicParam.Add("LARGELOT", Util.NVC(result.Rows[jnx]["PRT_ITEM07"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                                dicParam.Add("MODEL", Util.NVC(result.Rows[jnx]["PRT_ITEM09"]));
                                dicParam.Add("REGDATE", Util.NVC(result.Rows[jnx]["PRT_ITEM10"]));
                                dicParam.Add("EQPTNO", Util.NVC(result.Rows[jnx]["PRT_ITEM11"]));
                                dicParam.Add("TITLEX", "BASKET ID");
                                dicParam.Add("PRINTQTY", "2");  // 발행 수
                                dicParam.Add("B_LOTID", Util.NVC(result.Rows[jnx]["PRT_ITEM01"]));
                                dicParam.Add("B_WIPSEQ", Util.NVC(result.Rows[jnx]["PRT_ITEM02"]));
                                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.
                                dicList.Add(dicParam);

                                if (dicList != null && dicList.Count > 0)
                                {
                                    foreach (Dictionary<string, string> dic in dicList)
                                    {
                                        dicParam = dic;

                                        if (dicParam.ContainsKey("PRINTQTY"))
                                            iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));

                                        var fd = (FlowDocument)null;


                                        fd = CreateFlowDocument(dicParam);

                                        if (fd != null)
                                        {
                                            fd.PagePadding = new Thickness(5, 0, 5, 0);

                                            dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                                            ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);


                                            var paginatorSource = (IDocumentPaginatorSource)fd;
                                            var paginator = paginatorSource.DocumentPaginator;

                                            for (int j = 0; j < iCnt; j++)
                                            {
                                                dialog.PrintDocument(paginator, "GMES PRINT");
                                            }

                                            SetLabelPrtHist();

                                        }
                                    }
                                }
                                else
                                {
                                    if (dicParam != null)
                                    {
                                        // 바구니 생성 후 자동 프린트 처리.
                                        var fd = (FlowDocument)null;

                                        fd = CreateFlowDocument(dicParam);

                                        if (fd != null)
                                        {
                                            fd.PagePadding = new Thickness(5, 0, 5, 0);

                                            dialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(fd.PageWidth, fd.PageHeight);
                                            ((IDocumentPaginatorSource)fd).DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);


                                            var paginatorSource = (IDocumentPaginatorSource)fd;
                                            var paginator = paginatorSource.DocumentPaginator;

                                            for (int j = 0; j < iCnt; j++)
                                            {
                                                dialog.PrintDocument(paginator, "GMES PRINT");
                                            }

                                            SetLabelPrtHist();
                                        }
                                    }
                                }
                            }
                        }

                        Util.gridClear(dgLotInfo);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Validation]

        private bool CanCancelTerm()
        {
            bool bRet = false;

            if (dgLotInfo.ItemsSource == null || dgLotInfo.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2953");    // 종료 취소 할 항목이 없습니다.
                return bRet;
            }

            for (int idx = 0; idx < dgLotInfo.GetRowCount(); idx++)
            {
                // 수량 체크.
                double dQty = 0;
                double.TryParse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPQTY2_ST")), out dQty);
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPQTY2_ST")).Equals("") ||
                    dQty < 1)
                {
                    Util.MessageValidation("SFU1683");  // 수량은 0보다 커야 합니다.
                    return bRet;
                }
            }            

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #endregion



        private void dgLotInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                if (!Util.NVC(e.Cell.Column.Name).Equals("WIPQTY2_ST"))
                    return;

                C1DataGrid grd = sender as C1DataGrid;

                grd.EndEdit();
                grd.EndEditRow(true);

                string sTmp = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY2_ST"));
                string sMax = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY_IN"));

                double dMax = 0;
                double dNow = 0;

                double.TryParse(sMax, out dMax);
                double.TryParse(sTmp, out dNow);

                if (dMax >= 0 && dMax < dNow)
                {
                    bViewMsg = true;
                    Util.MessageValidation("SFU3107", (actin) => { bViewMsg = false; });   // 수량이 이전 수량보다 많이 입력 되었습니다.
                    DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "WIPQTY2_ST", dMax == 0 ? 1 : dMax);

                    grd.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name).Equals("WIPQTY2_ST"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgLotInfo_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }
    }
}
