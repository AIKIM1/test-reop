/************************************************************************************
  Created Date : 2022.04.18
       Creator : 이태규
   Description : 충방전 공정 설비 현황 조회
 ------------------------------------------------------------------------------------
  [Change History]
    2022.04.18  이태규 : Initial Created.
    2022.05.13  이태규 : Cell text 짤림현상 변경 & Cell size 해상도에 최적화 적용
    2022.06.13  이태규 : SUMMARY 추가
    2022.06.17  이태규 : Main grid 정부 추가
 ************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_032 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_032()
        {
            InitializeComponent();
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }
        #endregion

        #region #. Member Function Lists...
        private void Initialize()
        {
            SetProcess(cboProcess);// SetCombo(공정)
            SetAutoSearchCombo(cboAutoSearch);// SetCombo(자동조회)            
            if (_dispatcherMainTimer != null)// 자동조회 적용
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.ToString()) && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        #region #. Set combo
        /// <summary>
        /// 공정
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetProcess(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "PACK_MONITORING_PROCESS" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }
        /// <summary>
        /// 자동조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region #. Search
        // 조회
        private void SearchProcess()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                this.SearchGrdMain();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회 - Stocker Summary 현황
        private void SearchGrdMain()
        {
            #region #. validatoin and initialize
            grdMain.ColumnDefinitions.Clear();
            grdMain.RowDefinitions.Clear();
            grdMain.Children.Clear();
            grdTotal.ColumnDefinitions.Clear();
            grdTotal.RowDefinitions.Clear();
            grdTotal.Children.Clear();
            string PROCID = Util.GetCondition(cboProcess);
            if (string.IsNullOrWhiteSpace(PROCID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("공정")); // %1(을)를 선택하세요.
                this.cboProcess.Focus();
                return;
            }
            #endregion

            CreateColunmsRuntime();
            CreateColunmsRuntimeTotal();
        }

        #region #. CreateColunmsRuntime
        public void CreateColunmsRuntime()
        {
            #region #. Data Retreive and Convert: CMA 충전(P5270) & CMA 방전(P5430) 
            string PROCID = Util.GetCondition(cboProcess);
            string bizRuleName = "DA_PRD_SEL_LOGIS_CHARGE_EQPT_STAT";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["PROCID"] = PROCID;
            drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

            int i_cnt = dtRSLTDT.Rows.Count;
            int j_cnt = Convert.ToString(dtRSLTDT.Rows[0]["EQPT_SET"]).Split(new char[] { ',' }).Length;
            DataTable dt = new DataTable();
            if (i_cnt > 0 && j_cnt > 0)
            {
                for (int j = 0; j < j_cnt; j++)
                {
                    dt.Columns.Add(j.ToString(), typeof(string));
                }

                for (int i = 0; i < i_cnt; i++)
                {
                    DataRow row = dt.NewRow();
                    string[] sValue = Convert.ToString(dtRSLTDT.Rows[i]["EQPT_SET"]).Split(new char[] { ',' });
                    for (int j = 0; j < j_cnt; j++)
                    {
                        if (sValue[j] == "") sValue[j] = "|||";
                        row[j] = sValue[j];
                    }
                    dt.Rows.Add(row);
                }
            }
            #endregion

            #region #. define color and number
            double screenWidth = grbMain.ActualWidth - 5;
            double screenHeight = grbMain.ActualHeight - 5;

            int columnsize = j_cnt;
            int rowsize = i_cnt;
            Color FColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//ForegroundColor
            Color BColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//BackgroundColor
            Color C = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
            Color R = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
            Color W = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
            Color T = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//빨간색
            Color U = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD0CECE");//회색
            Color G = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD0CECE");//회색
            Color F = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
            Color B = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색
            Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEFF1F3");//바탕색
            #endregion

            #region #. add control with text and style to grdMain

            for (int j= 0; j < columnsize; j++)
            {
                grdMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(screenWidth / columnsize) });

                for (int i = 0; i < rowsize; i++)
                {
                    grdMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(screenHeight / rowsize) });

                    string sText = "";
                    string[] sTexts = null;
                    string sState = "";

                    #region #. make data for control
                    if (dt.Rows[i][j] != null)
                    {
                        sTexts = Convert.ToString(dt.Rows[i][j]).Split(new char[] { '|' });
                        if (sTexts[3] != "") sTexts[3] = sTexts[3] + "%";
                        sText = /*"Name:" + */sTexts[0] + "\r\n"
                            /*+ "State:" + sTexts[1] + "\r\n"*/
                            /*+ "LotID:"*/ + sTexts[2] + "\r\n"
                            /*+ "Rate:"*/ + sTexts[3];
                        if (sTexts[3] != "") sTexts[3] = sTexts[3].Replace("%", "");
                        int number;
                        bool success = int.TryParse(sTexts[3], out number);
                        if (success)
                        {
                            switch (number == 0 ? "red" : number < 95 ? "green" : "yellow")
                            {
                                case "red": BColor = T; FColor = F; sState = "Empty"; break;
                                case "green": BColor = R; FColor = F; sState = "In-progress"; break;
                                case "yellow": BColor = W; FColor = F; sState = "In-progress"; break;
                            }
                        }
                        else
                        {
                            BColor = C; FColor = F;
                        }
                        if (sTexts[1] == "M")
                        {
                            //case "gray": 
                            BColor = G; FColor = F; sState = "Maintenance";
                        }
                    }
                    #endregion

                    #region #. make control with style
                    System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));

                    if (j > 0 || j < rowsize - 1 || i > 0 || i < columnsize - 1)
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(F)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
                    }
                    else
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
                    }
                    
                    Label lbl = new Label() { Content = new TextBlock() { Text = sTexts[0], TextWrapping = TextWrapping.Wrap, FontSize = 9 }, Width = (((screenWidth / 2) / columnsize))-3, Height = ((screenHeight / 3) / rowsize)-3 };
                    lbl.Style = style;
                    lbl.SetValue(Grid.ColumnProperty, j);
                    lbl.SetValue(Grid.RowProperty, i);
                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);
                    
                    Label lbl2 = new Label() { Content = new TextBlock() { Text = sState, TextWrapping = TextWrapping.Wrap, FontSize = 9 }, Width = (((screenWidth / 2) / columnsize))-6, Height = ((screenHeight / 3) / rowsize)-3 };
                    lbl2.Style = style;
                    lbl2.SetValue(Grid.ColumnProperty, j);
                    lbl2.SetValue(Grid.RowProperty, i);
                    lbl2.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl2.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl2.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl2.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl2.Content).Margin = new Thickness(0, 0, 0, 0);

                    Label lbl3 = new Label() { Content = new TextBlock() { Text = sTexts[3], TextWrapping = TextWrapping.Wrap, FontSize = 9 }, Width = (screenWidth / columnsize) - 11, Height = ((screenHeight / 3) / rowsize) + 6};
                    lbl3.Style = style;
                    lbl3.SetValue(Grid.ColumnProperty, j);
                    lbl3.SetValue(Grid.RowProperty, i);
                    lbl3.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl3.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl3.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl3.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl3.Content).Margin = new Thickness(0, 0, 0, 0);

                    Label lbl5 = new Label() { Content = new TextBlock() { Text = sTexts[2], TextWrapping = TextWrapping.Wrap, FontSize = 9 }, Width = (screenWidth / columnsize)-11, Height = ((screenHeight / 3) / rowsize)-4 };
                    lbl5.Style = style;
                    lbl5.SetValue(Grid.ColumnProperty, j);
                    lbl5.SetValue(Grid.RowProperty, i);
                    lbl5.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl5.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl5.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl5.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl5.Content).Margin = new Thickness(0, 0, 0, 0);

                    ProgressBar bar = new ProgressBar();
                    if (sTexts[3] == "") sTexts[3] = "0";
                    bar.Value = int.Parse(sTexts[3]);
                    bar.Foreground = new SolidColorBrush(B);
                    bar.Background = new SolidColorBrush(BColor);
                    bar.VerticalAlignment = VerticalAlignment.Center;
                    bar.Height = ((screenHeight / 3) / rowsize) - 2;
                    bar.Width = (screenWidth / columnsize) - 13;
                    TextBlock tbBar = new TextBlock();
                    tbBar.Text = sTexts[3] + "%";
                    tbBar.TextAlignment = TextAlignment.Center;
                    tbBar.HorizontalAlignment = HorizontalAlignment.Center;
                    tbBar.VerticalAlignment = VerticalAlignment.Center;
                    #endregion

                    #region #. add grid to grid
                    Grid grd = new Grid();
                    grd.Background = new SolidColorBrush(F);
                    grd.Margin = new Thickness(5);
                    grd.ColumnDefinitions.Add(new ColumnDefinition());
                    grd.ColumnDefinitions.Add(new ColumnDefinition());
                    grd.RowDefinitions.Add(new RowDefinition());
                    grd.RowDefinitions.Add(new RowDefinition());
                    grd.RowDefinitions.Add(new RowDefinition());
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, 0);
                    grd.Children.Add(lbl);
                    Grid grdBar = new Grid();
                    grdBar.Children.Add(bar);
                    grdBar.Children.Add(tbBar);
                    Grid.SetColumn(grdBar, 0);
                    Grid.SetColumnSpan(grdBar, 2);
                    Grid.SetRow(grdBar, 1);
                    grd.Children.Add(grdBar);
                    if (sTexts[1] == "M" || sTexts[3] == "0")
                    {
                        Grid.SetColumn(lbl3, 0);
                        Grid.SetColumnSpan(lbl3, 2);
                        Grid.SetRow(lbl3, 1);
                        grd.Children.Add(lbl3);
                    }
                    Grid.SetColumn(lbl2, 1);
                    Grid.SetRow(lbl2, 0);
                    grd.Children.Add(lbl2);
                    Grid.SetColumn(lbl5, 0);
                    Grid.SetColumnSpan(lbl5, 2);
                    Grid.SetRow(lbl5, 2);
                    grd.Children.Add(lbl5);
                    if (sTexts[0] != "")
                    {
                        grdMain.Children.Add(grd);
                        Grid.SetColumn(grd, j);
                        Grid.SetRow(grd, i);
                        if (int.Parse(sTexts[3]) >= 95 && sTexts[1] != "M" && sTexts[1] != "U")
                        {
                            int second = 60 * 60 * 24;
                            if (_dispatcherMainTimer != null)// 자동조회 적용
                            {
                                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.ToString()) && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());
                            }
                            BlinkGrid(grd, 1000, second);
                        }
                    }
                    #endregion
                }
            }
            #endregion
            
        }

        public void CreateColunmsRuntimeTotal()
        {
            #region #. Data Retreive and Convert
            string PROCID = Util.GetCondition(cboProcess);
            string bizRuleName = "DA_PRD_SEL_LOGIS_CHARGE_EQPT_STAT_SUMMARY";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["PROCID"] = PROCID;
            drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            if (dtRSLTDT.Rows.Count == 0) return;

            DataTable dt = new DataTable();
            dt.Columns.Add("COL0", typeof(string));
            dt.Columns.Add("COL1", typeof(string));
            dt.Columns.Add("COL2", typeof(string));
            dt.Columns.Add("COL3", typeof(string));
            DataRow row = dt.NewRow();
            row["COL0"] = dtRSLTDT.Rows[0]["CASE1"];
            row["COL1"] = dtRSLTDT.Rows[0]["CASE2"];
            row["COL2"] = dtRSLTDT.Rows[0]["CASE3"];
            row["COL3"] = dtRSLTDT.Rows[0]["CASE4"];
            dt.Rows.Add(row);
            #endregion

            #region #. define color and number
            double screenWidth = grbTotal.ActualWidth - 5;
            double screenHeight = grbTotal.ActualHeight - 5;

            int columnsize = dt.Columns.Count;
            int rowsize = dt.Rows.Count;
            Color FColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//ForegroundColor
            Color BColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//BackgroundColor
            Color C = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
            Color R = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
            Color W = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
            Color T = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//빨간색
            Color U = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD0CECE");//회색
            Color F = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
            Color B = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색
            Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEFF1F3");//바탕색            
            #endregion

            #region #. add control with text and style to grdTotal
            for (int j = 0; j < columnsize; j++)
            {
                grdTotal.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(screenWidth / columnsize) });
                for (int i = 0; i < rowsize; i++)
                {
                    grdTotal.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(screenHeight / rowsize) });

                    string[] sTexts = null;

                    #region #. make data for control
                    sTexts = Convert.ToString(dt.Rows[i][j]).Split(new char[] { '|' });
                    #endregion

                    #region #. make control with style
                    System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));
                    if (j == 0)
                    {
                        FColor = F;
                        BColor = R;
                    }
                    else if (j == 1)
                    {
                        FColor = F;
                        BColor = W;
                    }
                    else if (j == 2)
                    {
                        FColor = F;
                        BColor = T;
                    }
                    else if (j == 3)
                    {
                        FColor = F;
                        BColor = U;
                    }

                    if (j > 0 || j < rowsize - 1 || i > 0 || i < columnsize - 1)
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(F)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
                    }
                    else
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
                    }
                    Label lbl = null;
                    lbl = new Label() { Content = new TextBlock() { Text = sTexts[0], TextWrapping = TextWrapping.Wrap, FontSize = 12 }, Width = ((screenWidth / 2) / columnsize)-3, Height = (screenHeight / rowsize)-32};
                    lbl.Style = style;
                    lbl.SetValue(Grid.ColumnProperty, j);
                    lbl.SetValue(Grid.RowProperty, i);
                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);

                    Label lbl2 = null;
                    lbl2 = new Label() { Content = new TextBlock() { Text = sTexts[1], TextWrapping = TextWrapping.Wrap, FontSize = 12 }, Width = ((screenWidth / 2) / columnsize)-5, Height = (screenHeight / rowsize)-32 };
                    lbl2.Style = style;
                    lbl2.SetValue(Grid.ColumnProperty, j);
                    lbl2.SetValue(Grid.RowProperty, i);
                    lbl2.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl2.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl2.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl2.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl2.Content).Margin = new Thickness(0, 0, 0, 0);

                    Label lbl3 = null;
                    lbl3 = new Label() { Content = new TextBlock() { Text = sTexts[2], TextWrapping = TextWrapping.Wrap, FontSize = 12 }, Width = (screenWidth / columnsize)-10, Height = (screenHeight / rowsize)-37 };
                    lbl3.Style = style;
                    lbl3.SetValue(Grid.ColumnProperty, j);
                    lbl3.SetValue(Grid.RowProperty, i);
                    lbl3.HorizontalContentAlignment = HorizontalAlignment.Center;
                    ((TextBlock)lbl3.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl3.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl3.Content).Margin = new Thickness(0, 0, 0, 0);
                    #endregion

                    #region #. add grid to grid
                    Grid grd = new Grid();
                    grd.Background = new SolidColorBrush(F);
                    grd.Margin = new Thickness(5);
                    grd.ColumnDefinitions.Add(new ColumnDefinition());
                    grd.ColumnDefinitions.Add(new ColumnDefinition());
                    grd.RowDefinitions.Add(new RowDefinition());
                    grd.RowDefinitions.Add(new RowDefinition());
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, 0);
                    grd.Children.Add(lbl);
                    Grid.SetColumn(lbl2, 1);
                    Grid.SetRow(lbl2, 0);
                    grd.Children.Add(lbl2);
                    Grid.SetColumn(lbl3, 0);
                    Grid.SetColumnSpan(lbl3, 2);
                    Grid.SetRow(lbl3, 1);
                    grd.Children.Add(lbl3);
                    grdTotal.Children.Add(grd);
                    Grid.SetColumn(grd, j);
                    Grid.SetRow(grd, i);
                    #endregion
                }

            }
            #endregion
        }

        #endregion

        #endregion

        #region #. auto search

        private bool _isAutoSelectTime = false;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;
                    // data조회
                    SearchProcess();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals("") && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // 자동조회  %1초로 변경 되었습니다.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            SearchProcess();
        }
        #endregion

        #region #. Blink Grid
        public void BlinkGrid(Grid grid, int length, double repetation)
        {
            DoubleAnimation opacityAnimaiton = new DoubleAnimation
            {
                From = 1.0,
                //To = 0.0,
                To = 0.5,
                Duration = new Duration(TimeSpan.FromMilliseconds(length)),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(repetation)
            };
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimaiton);
            Storyboard.SetTarget(opacityAnimaiton, grid);
            Storyboard.SetTargetProperty(opacityAnimaiton, new PropertyPath("Opacity"));
            storyboard.Begin(grid);
        }
        #endregion

        #endregion

    }
}