/************************************************************************************
  Created Date : 2022.04.27
       Creator : 이태규
   Description : MEB 충방전 창고/렉 위치별 재공 현황 조회
 ------------------------------------------------------------------------------------
  [Change History]
    2022.04.27  이태규 : Initial Created.
    2022.06.22  이태규 : Group정보 추가(Detail Grid)
 ************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_033 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_033()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupAlert, this.pnlTitleTransferConfirm); //생성자에 추가
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

        private void Lbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchGrdDetail(sender);
            }));
        }
        #endregion

        #region #. Member Function Lists...
        private void Initialize()
        {
            SetAreaCombo();
            SetWHCombo();
            SetAutoSearchCombo(cboAutoSearch);// 자동조회
            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.ToString()) && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
        }

        #region #. Set Combo
        private void SetAreaCombo()
        {
            String[] sFiltercboArea = { Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFiltercboArea, sCase: "AREA_PACK");
        }
        private void SetWHCombo()
        {     
            C1ComboBox[] cboWHIDRightParent = { cboArea };
            _combo.SetCombo(cboWhId, CommonCombo.ComboStatus.SELECT, cbParent: cboWHIDRightParent, sCase: "cboWHID");
        }

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

        private void SearchGrdMain()
        {
            CreateColunmsRuntime();
        }

        private void SearchGrdDetail(object sender)
        {
            if (sender == null)
            {
                return;
            }
            Label lbl = (Label)sender;
            //string[] sTexts = ((TextBlock)lbl.Content).Text.Replace("\r\n", "|").Split(new char[] { '|' });
            string[] sTexts = ((TextBlock)lbl.Content).Tag.ToString().Replace("\r\n", "|").Split(new char[] { '|' });
            if (sTexts != null && sTexts.Length > 0)
            {
                #region #. lot info
                string bizRuleName = "DA_PRD_SEL_LOGIS_WH_RACK_STAT_DETAIL";
                DataTable dtRQSTDT = new DataTable();

               // dtRQSTDT.Columns.Add("WH_ID", typeof(string));
                dtRQSTDT.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
               // dr["WH_ID"] = cboWhId.SelectedValue;                
                dr["RACK_ID"] = sTexts[0];
                dtRQSTDT.Rows.Add(dr);

                //dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT);

                PackCommon.SearchRowCount(ref this.txtGridRowCount, dtResult.Rows.Count);
                Util.GridSetData(this.grdDetail, dtResult, FrameOperation);

                #endregion
            }
        }

        #endregion

        #region #. CreateColunmsRuntime
        public void CreateColunmsRuntime()
        {
            #region #. validatoin and initialize
            string AREAID = Util.GetCondition(cboArea);
            if (cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("동")); // %1(을)를 선택하세요.
                this.cboArea.Focus();
                return;
            }
            string WHID = Util.GetCondition(cboWhId);
            if (cboWhId.SelectedValue.Equals("SELECT"))
            {
                ms.AlertWarning("SFU2961"); //창고를 먼저 선택해주세요.
                return;
            }
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
            Util.gridClear(this.grdDetail);
            grdMain.ColumnDefinitions.Clear();
            grdMain.RowDefinitions.Clear();
            grdMain.Children.Clear();
            #endregion

            try   
            {
                #region #. Data Retreive and Convert:
                string bizRuleName = "DA_PRD_SEL_LOGIS_WH_RACK_STAT";
                string[] sValue = { };
                int y_cnt = 0;
                
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");                

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("WH_ID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                //drRQSTDT["WH_ID"] = "P8F160";//추후 combo로 변경 예정!!
                drRQSTDT["WH_ID"] = WHID;
                drRQSTDT["AREAID"] = AREAID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

       
                //_maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);
                if (dtRSLTDT.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU8548");
                    return;
                }
                int i_cnt = dtRSLTDT.Rows.Count;
                int j_cnt = Convert.ToString(dtRSLTDT.Rows[0]["WH_SET"]).Split(new char[] { ',' }).Length;
                y_cnt = dtRSLTDT.AsEnumerable().ToList().Max(r => (int)r["SCRN_Y_POSITN_VALUE"]);
                //int j_cnt = 0;
                //int i_cnt = 0;
                //j_cnt = dtRSLTDT.AsEnumerable().ToList().Max(r => (int)r["SCRN_X_POSITN_VALUE"]);
                //i_cnt = dtRSLTDT.AsEnumerable().ToList().Max(r => (int)r["SCRN_Y_POSITN_VALUE"]);


                DataTable dt = new DataTable();
                dt.Columns.Add("0", typeof(string));
                if (i_cnt > 0 && j_cnt > 0)
                {
                    for (int j = 0; j < j_cnt; j++)
                    {
                        dt.Columns.Add((j + 1).ToString(), typeof(string));
                    }
                    DataRow rowHeader = dt.NewRow();
                    dt.Rows.Add(rowHeader);
                    for (int i = 0; i < i_cnt; i++)
                    {                        
                        DataRow row = dt.NewRow();
                        sValue = Convert.ToString(dtRSLTDT.Rows[i]["WH_SET"]).Split(new char[] { ',' });

                        if (j_cnt == sValue.Length && Convert.ToInt16(dtRSLTDT.Rows[i]["MAX_SCRN_X_POSITN_VALUE"]) == sValue.Length && dtRSLTDT.Rows.Count == y_cnt)
                        {
                            

                            for (int j = 0; j < j_cnt; j++)
                            {
                                if (sValue[j] == "") sValue[j] = "|||";
                                //row[j] = sValue[j];
                                row[j + 1] = sValue[j];
                            }
                            dt.Rows.Add(row);
                        }
                        else
                        {
                            Util.MessageInfo("SFU8546");
                            return;
                        }                                                                       
                    }
                }
                #endregion                

                #region #. define color and number                
                double screenWidth = grbMain.ActualWidth - 40;
                double screenHeight = grbMain.ActualHeight - 30;
                int columnsize = j_cnt;
                int rowsize = i_cnt;
                Color FColor = Colors.White;//ForegroundColor
                Color BColor = Colors.White;//BackgroundColor
                Color C = Colors.White;//하얀색
                Color R = Colors.YellowGreen;//초록색
                Color W = Colors.Yellow;//노란색
                Color T = Colors.Red;//빨간색
                Color U = Colors.LightGray;//회색
                Color F = Colors.Black;//검정색
                Color B = Colors.Blue;//파란색
                Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEFF1F3");//바탕색
                                                                                                              //Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//바탕색
                #endregion

                #region #. add label with text and style to grdMain
                for (int i = 0; i < rowsize + 1; i++)
                {
                    for (int j = 0; j < columnsize + 1; j++)
                    {
                        if (i == 0)//Grid.RowDefinitions
                        {
                            if (j == 0)
                            {
                                grdMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30) });
                            }
                            else
                            {
                                double with = Math.Truncate(screenWidth / columnsize);
                                grdMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(with) });
                            }
                        }
                        string sText = "";
                        string[] sTexts = null;
                        string[] sTexts2 = new string[5];
                        if (dt.Rows[i][j] != null)
                        {
                            if (i == 0)
                            {
                                if (j == 0)
                                    sText = "";
                                else
                                    sText = (j).ToString();
                            }
                            else
                            {
                                if (j == 0)
                                {
                                    sText = (i).ToString();
                                }
                                else
                                {


                                    sTexts = Convert.ToString(dt.Rows[i][j]).Split(new char[] { '|' });
                                    int sTextsCnt = sTexts.Length;
                                    for (int k = 0; k < sTextsCnt; k++)
                                    {
                                        if (k == 0) { sText = /*"WH:" +*/ sTexts[0] + "\r\n"; continue; }
                                        else if (k == 1) { sText += /*"Rack:" +*/ sTexts[k] + "\r\n"; continue; }
                                        else if (k == sTextsCnt - 1) { sText += "TotalCnt:" + sTexts[k]; break; }
                                        else { sText += sTexts[k] + "\r\n"; continue; }
                                    }
                                    int number;

                                    bool success = int.TryParse(sTexts[sTextsCnt - 2].Replace("%", ""), out number);
                                    if (success)
                                    {
                                        switch (number == 0 ? "red" : number < 95 ? "green" : "yellow")
                                        {
                                            case "red": BColor = T; FColor = C; break;
                                            case "green": BColor = R; FColor = F; break;
                                            case "yellow": BColor = W; FColor = F; break;
                                        }

                                        if (sText.ToString().Contains("DUMMY"))
                                        {
                                            BColor = U;
                                            FColor = F;
                                        }
                                    }
                                    else
                                    {
                                        BColor = C; FColor = F;
                                    }
                                }
                            }
                        }

                        if (i == 0 && j == 0)
                        {
                            grdMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                        }
                        else
                        {
                            grdMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(screenHeight / rowsize) });                            
                        }

                        System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));

                        if (j > 0 || j < rowsize - 1 || i > 0 || i < columnsize - 1)
                        {
                            if (i != 0 && j != 0)
                            {
                                style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                                style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                                style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
                                style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(4)));
                            }
                            else
                            {
                                style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(F)));
                                style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(U)));
                                style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
                                style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0.5)));

                            }
                        }
                        Label lbl = null;
                        if (i != 0 && j != 0)
                        {
                            lbl = new Label() { Content = new TextBlock() { Tag = sText/*, Text = sText*/, TextWrapping = TextWrapping.Wrap, FontSize = 12 }/*, Width = (screenWidth / columnsize), Height = (screenHeight / rowsize)*/ };
                        }
                        else
                        {
                            if (j == 0)
                            {
                                lbl = new Label() { Content = new TextBlock() { Tag = sText, Text = sText, TextWrapping = TextWrapping.Wrap, FontSize = 8, Width = 10, Height = (screenHeight / rowsize) } }; //Y축
                            }
                            else
                            {
                                lbl = new Label() { Content = new TextBlock() { Tag = sText, Text = sText, TextWrapping = TextWrapping.Wrap, FontSize = 8, Width = (screenWidth / columnsize), Height = (screenHeight / rowsize) } }; //X축
                            }
                        }
                        lbl.Style = style;
                        lbl.SetValue(Grid.ColumnProperty, j);
                        lbl.SetValue(Grid.RowProperty, i);
                        ((TextBlock)lbl.Content).HorizontalAlignment = HorizontalAlignment.Center;
                        ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
                        ((TextBlock)lbl.Content).TextAlignment = TextAlignment.Center;
                        ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
                        ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);

                        if (i != 0 && j != 0)
                        {
                            lbl.MouseDown += Lbl_MouseDown;
                            lbl.MouseEnter += Lbl_MouseEnter;
                            lbl.MouseLeave += Lbl_MouseLeave;
                        }
                        if (j == 0) lbl.Width = 30;
                        if (i == 0) lbl.Height = 24;
                        grdMain.Children.Add(lbl);
                    }
                }
                #endregion
        }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion      
           
        #region #. Auto Search
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

        #region #. Alert Popup
        private void Lbl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            ShowPopup(sender);
        }

        private void Lbl_MouseLeave(object sender, MouseEventArgs e)
        {
            HidePopUp();
        }

        private void btnHideConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }

        // Popup - Show Popup
        private void ShowPopup(object sender)
        {
            try
            {
                this.popupAlert.Tag = null;
                this.popupAlert.Tag = null;

                Label lbl = (Label)sender;
                string sText = ((TextBlock)lbl.Content).Tag.ToString();
                txtMessageAlert.Text = sText;
                this.popupAlert.PlacementTarget = lbl;
                this.popupAlert.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

    }
}