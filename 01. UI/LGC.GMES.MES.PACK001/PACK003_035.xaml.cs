/*************************************************************************************
 Created Date : 2022.07.05
      Creator : 주동석 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.19  주동석 : Initial Created.
  2022.11.10  김진수   WO기준 MMD 등록 안되어있는 자재가 없을시 POPUP창 안뜨게 수정
  2022.11.24  김진수   MMD등록 유무 POPUP 라인 변경 조회시 뜨게 수정
  2022.12.13  김진수   Box정보 조회할수 있는 POPUP 추가
  2022.12.15  김진수   자동 공급 유무 Biz BR로 수정
  2023.07.19  장만철   자재반품요청 버튼 추가(POPUP)
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.PACK001.Class;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_035 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        CommonCombo _combo = new CommonCombo();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        private PACK003_035_DataHelper dataHelper = new PACK003_035_DataHelper();
        string[] sParam = null;
        Color FColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//ForegroundColor
        Color BColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//BackgroundColor
        Color White = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
        Color Green = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
        Color Yellow = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
        Color Red = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000");//빨간색
        Color Orange = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//주황색
        Color Gray = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#8C8C8C");//회색
        Color Black = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
        Color Blue = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색
        Color Purple = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#B95AFF");//보라색
        Color Transparent = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00ff0000"); // 투명색
        Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#5F00FF");//바탕색
        int cboMtrlRackCount = 0;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string sMtrlPortID = string.Empty;
        public string MtrlPortID
        {
            get
            {
                return sMtrlPortID;
            }

            set
            {
                sMtrlPortID = value;
            }
        }

        private string sEqsgID = string.Empty;
        public string EqsgID
        {
            get
            {
                return sEqsgID;
            }

            set
            {
                sEqsgID = value;
            }
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_035()
        {
            InitializeComponent();
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //이하 코드 추가
                this.Initialize();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
                //add start
                this.Unloaded += PACK003_035_Unloaded;
                //add end

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void PACK003_035_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherMainTimer.Stop();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.cboSnapEqsg.SelectedValue.ToString()))
            {
                //SFU1223 : 라인을 선택하세요.
                Util.MessageInfo("SFU1223");
                return;
            }

            //if (string.IsNullOrEmpty(Convert.ToString(this.cboMtrlRack.SelectedItemsToString)))
            if (!this.EqsgID.Equals(this.cboSnapEqsg.SelectedValue.ToString()))
            {
                this.EqsgID = this.cboSnapEqsg.SelectedValue.ToString();
                DataTable dt2 = SetPortDetailCheck();

                if (dt2.Rows.Count > 0 && Int32.Parse(dt2.Rows[0]["MTRLCOUNT"].ToString()) > 0)
                {
                    Util.MessageInfo("SUF9007", dt2.Rows[0]["MTRLCOUNT"].ToString(), dt2.Rows[0]["MTRLIDS"].ToString());
                }
            }
            if (!string.IsNullOrEmpty(Convert.ToString(this.cboMtrlRack.SelectedItemsToString)))
            {
                sParam = new string[] { this.cboMtrlRack.SelectedItemsToString, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, cboSnapEqsg.SelectedValue.ToString() };

                SetGrdDetail(sParam);
            }
            else
            {
                PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, 0);
                Util.gridClear(grdDetail);
                sParam = null;
            }

            this.SearchProcess();
        }

        private void grdDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "BOX1" || e.Cell.Column.Name == "BOX2" || e.Cell.Column.Name == "BOX3" || e.Cell.Column.Name == "BOX4" || e.Cell.Column.Name == "BOX5"
                    || e.Cell.Column.Name == "BOX6" || e.Cell.Column.Name == "BOX7" || e.Cell.Column.Name == "BOX8" || e.Cell.Column.Name == "BOX9" || e.Cell.Column.Name == "BOX10"
                    || e.Cell.Column.Name == "BOX11" || e.Cell.Column.Name == "BOX12" || e.Cell.Column.Name == "BOX13" || e.Cell.Column.Name == "BOX14" || e.Cell.Column.Name == "BOX15"
                    || e.Cell.Column.Name == "BOX16" || e.Cell.Column.Name == "BOX17" || e.Cell.Column.Name == "BOX18" || e.Cell.Column.Name == "BOX19" || e.Cell.Column.Name == "BOX20"
                    )
                    {
                        SetCellColor(dataGrid, e);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region #. Member Function Lists...
        private void Initialize()
        {
            iniCombo();
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

            //PackCommon.SearchRowCount(ref 0, 0);
        }

        private void iniCombo()
        {
            SetCboEQSG(cboSnapEqsg);

            string sTemp = Util.NVC(cboSnapEqsg.SelectedValue);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRack, ref this.cboMtrlRackCount, null);

            setUseYN();

            cboSnapEqsg.SelectedValueChanged += cboSnapEqsg_SelectedValueChanged;
        }

        #region [ WPF Event ]

        #region ( 전산재고 Header )

        #region < 동 >
        private void cboSnapArea_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEQSG(cboSnapEqsg);
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >
        private void cboSnapEqsg_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                string sTemp = Util.NVC(cboSnapEqsg.SelectedValue);

                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRack, ref this.cboMtrlRackCount, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #endregion

        #region < 콤보 생성 >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, AREAID
        /// Event : cboSnapArea_SelectedValueChanged
        /// </summary>
        private void SetCboEQSG(C1ComboBox cboEqsg)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drn = RQSTDT.NewRow();
                drn["LANGID"] = LoginInfo.LANGID;
                drn["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(drn);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_CODE"] = "";
                dRow["CBO_NAME"] = "-SELECT-";
                dtResult.Rows.InsertAt(dRow, 0);

                cboEqsg.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboEqsg.IsEnabled = false;
                if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
                {
                    cboEqsg.SelectedValue = LoginInfo.CFG_EQSG_ID;
                }
                else if (dtResult.Rows.Count > 0)
                {
                    cboEqsg.SelectedIndex = 0;
                }
                else
                {
                    cboSnapEqsg_SelectedValueChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable SetcboMtrlRack_CBO()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = cboSnapEqsg.SelectedValue ?? null;

                RQSTDT.Rows.Add(dr);

                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = "-SELECT-";
                RQSTDT.Rows.InsertAt(dr, 0);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MTRL_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

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

        public DataTable initTable2()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LINE_INFOS", typeof(string));
            return dt;
        }

        public DataTable SetDataToGrind2()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("P_LANGID", typeof(string));
            RQSTDT.Columns.Add("P_SHOPID", typeof(string));
            RQSTDT.Columns.Add("P_AREAID", typeof(string));
            RQSTDT.Columns.Add("P_EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["P_LANGID"] = LoginInfo.LANGID;
            dr["P_SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["P_AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["P_EQSGID"] = cboSnapEqsg.SelectedValue ?? null;

            RQSTDT.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_LINE_MTRL_PORT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

            return dt;
        }

        private void SetCellColor(C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Row.DataItem != null)//  if (e.Cell.Row.Index >= 0)
            {
                if (dataGrid.Name.Equals("grdDetail"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                string sCOL36 = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name.Replace("BOX", "COLOR")));
                                int number;
                                bool success = int.TryParse(Util.NVC(grdDetail.GetCell(e.Cell.Row.Index, grdDetail.Columns[e.Cell.Column.Index].Index).Value), out number);

                                switch (sCOL36)
                                {
                                    case "GREEN":
                                        e.Cell.Presenter.Background = new SolidColorBrush(Green);
                                        break;
                                    case "ORANGE":
                                        e.Cell.Presenter.Background = new SolidColorBrush(Orange);
                                        break;
                                    case "RED":
                                        e.Cell.Presenter.Background = new SolidColorBrush(Red);
                                        break;
                                    case "WHITE":
                                        e.Cell.Presenter.Background = new SolidColorBrush(White);
                                        break;
                                    default:
                                        e.Cell.Presenter.Background = new SolidColorBrush(Gray);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        #region #. Search
        // 조회
        private void SearchProcess()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                #region #. validatoin and initialize
                grdMain.ColumnDefinitions.Clear();
                grdMain.RowDefinitions.Clear();
                grdMain.Children.Clear();
                #endregion
                CreateColunmsRuntime();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region #. CreateColunmsRuntime
        /// <summary>
        /// PORT|YELLOW|A,W-P8-M22-S01-A01|G481|P8|P8Q30
        /// Type/색상 / 표기 문자 / 자재ID / Area / Shop / Line(EQSG)
        /// </summary>
        public void CreateColunmsRuntime()
        {
            #region #. Data Retreive and Convert
            DataTable dt = SetDataToGrind2();
            #endregion

            #region #. define color and number
            double screenWidth = grbMain.ActualWidth - 5;
            double screenHeight = grbMain.ActualHeight - 5;
            int i_cnt = dt.Rows.Count;
            int j_cnt = dt.Columns.Count;
            int columnsize = j_cnt;
            int rowsize = i_cnt;

            #endregion

            #region #. add control with text and style to grdMain

            for (int i = 0; i < rowsize; i++)
            {
                grdMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(screenHeight / rowsize) });

                columnsize = Convert.ToString(dt.Rows[i][1]).Split(new char[] { '#' }).Length;


                string[] sText = null;
                string[] sTexts = null;
                string sTag = null;
                string sSTN_NO = "";

                sTexts = Convert.ToString(dt.Rows[i][1]).Split(new char[] { '#' });

                if (columnsize == 0)
                    break;

                for (int j = 0; j < columnsize; j++)
                {
                    grdMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(screenWidth / columnsize) });

                    #region #. make data for control
                    if (sTexts[j] != null)
                    {
                        sText = sTexts[j].Split(new char[] { ',' })[0].Split(new char[] { '|' });

                        switch (sText[1].ToUpper())
                        {
                            case "PURPLE": BColor = Purple; FColor = Black; sSTN_NO = sText[2]; break;
                            case "ORANGE": BColor = Orange; FColor = Black; sSTN_NO = sText[2]; break;
                            case "RED": BColor = Red; FColor = Black; sSTN_NO = sText[2]; break;
                            case "GREEN": BColor = Green; FColor = Black; sSTN_NO = sText[2]; break;
                            case "WHITE": BColor = White; FColor = Black; sSTN_NO = sText[2]; break;
                            case "TRANSPARENT": BColor = Transparent; FColor = Black; sSTN_NO = sText[2]; break;
                            case "GRAY": BColor = Gray; FColor = Black; sSTN_NO = sText[2]; break;
                        }

                        sTag = sTexts[j].Split(new char[] { ',' })[1];
                    }
                    #endregion

                    #region #. make control with style
                    System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));

                    if (j > 0 || j < rowsize - 1 || i > 0 || i < columnsize - 1)
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(Black)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(BColor == Transparent ? 0 : 1)));
                    }
                    else
                    {
                        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
                        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
                        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
                        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
                    }

                    //Label lbl = new Label() { Content = new TextBlock() { Text = sSTN_NO, TextWrapping = TextWrapping.Wrap, FontSize = 11  }, Width = (screenWidth / columnsize) - 4, Height = ((screenHeight / 2) / rowsize) + 15 };
                    //Label lbl = new Label() { Content = new TextBlock() { Text = sSTN_NO, Tag = sTag, TextWrapping = TextWrapping.Wrap, FontSize = (screenWidth / columnsize) / 4 }, Width = (screenWidth / columnsize) - 5, Height = ((screenHeight) / rowsize) - 5 };
                    Label lbl = new Label() { Content = new TextBlock() { Text = sSTN_NO, Tag = sTag, TextWrapping = TextWrapping.Wrap, FontSize = 13 }, Width = (screenWidth / columnsize) - 5, Height = ((screenHeight) / rowsize) - 5 };
                    lbl.Style = style;
                    lbl.SetValue(Grid.ColumnProperty, j);
                    lbl.SetValue(Grid.RowProperty, i);
                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
                    ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
                    ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);
                    lbl.MouseDown += Lbl_MouseDown;

                    #endregion

                    if (sTexts[0] != "")
                    {
                        grdMain.Children.Add(lbl);
                        Grid.SetColumn(lbl, j);
                        Grid.SetRow(lbl, i);
                    }
                }
            }
            #endregion

        }

        private DataTable SetPortDetailCheck()
        {
            // P_SHOPID String
            // P_AREAID	String
            // P_EQSGID String

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = cboSnapEqsg.SelectedValue ?? null;

            RQSTDT.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MMD_PORT_DETL_CHECK", "RQSTDT", "RSLTDT", RQSTDT);

            return dt;
        }

        /// <summary>
        /// Main Grid Click
        /// </summary>
        /// <param name= "sender"></param>
        /// <param name= "e"></param>
        private void Lbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchGrdDetail(sender);
            }));

            if (sender == null)
            {
                return;
            }
        }

        private void SearchGrdDetail(object sender)
        {
            if (sender == null)
            {
                return;
            }

            Label lbl = (Label)sender;

            if (((System.Windows.Controls.TextBlock)lbl.Content).Text == "")
                return;

            sParam = ((TextBlock)lbl.Content).Tag.ToString().Replace("\r\n", "|").Split(new char[] { '|' });

            string sTemp = Util.NVC(cboSnapEqsg.SelectedValue);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRack, ref this.cboMtrlRackCount, null);
            
            SetGrdDetail(sParam);
            #endregion
        }

        private void SetGrdDetail(string[] sParam)
        {
            if (sParam != null && sParam.Length > 0 && sParam[0] != "")
            {
                #region #. lot info
                string bizRuleName = "BR_MTRL_SEL_LINE_MTRL_PORT_BOX_INFO";
                DataTable dtINDATA = new DataTable("INDATA");
                DataTable dtOUTDATA = new DataTable("OUTDATA");

                //dtRQSTDT.Columns.Add("WH_ID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtINDATA.Columns.Add("AUTO_SPLY_FLAG", typeof(string));

                DataRow drRQSTDT = dtINDATA.NewRow();
                //drRQSTDT["WH_ID"] = sParam[0].Split(new char[] { ' ' })[0];

                drRQSTDT["AREAID"] = sParam[2];
                drRQSTDT["SHOPID"] = sParam[1];
                drRQSTDT["EQSGID"] = sParam[3];
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["MTRL_PORT_ID"] = sParam[0];
                drRQSTDT["AUTO_SPLY_FLAG"] = cboAutoSPLYYN.SelectedValue.Equals("ALL") ? null : cboAutoSPLYYN.SelectedValue;
                dtINDATA.Rows.Add(drRQSTDT);

                dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);
                //if (CommonVerify.HasTableRow(dtRSLTDT))
                //{
                PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, dtOUTDATA.Rows.Count);
                Util.GridSetData(this.grdDetail, dtOUTDATA, FrameOperation);

            }
        }

        #endregion

        #region #. Alert Popup

        private void grdDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            if (cell.Column.Name == "MTRLID")
            {
                ShowPopup(Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRL_PORT_ID"].Index).Value), Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRLID"].Index).Value));
            }

            if(cell.Column.Name == "ON_HAND_QTY")
            {
                btnBoxList_Click(Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRL_PORT_ID"].Index).Value), Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRLID"].Index).Value), "COMPLETE");
            }
            if (cell.Column.Name == "IN_TRANSIT_QTY")
            {
                btnBoxList_Click(Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRL_PORT_ID"].Index).Value), Util.NVC(grdDetail.GetCell(datagrid.CurrentRow.Index, grdDetail.Columns["MTRLID"].Index).Value), "DELIVERING");
            }

            if (cell.Column.Name.Substring(0, 3) == "BOX")
            {
                if (cell.Value == null)
                    return;

                if (cell.Value.Equals("READY"))
                {
                    // 작업대기 제품일 때 자재요청
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SUF9000"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetMaterialRequest(datagrid.CurrentRow.Index, cell.Column.Name, "BR_MTRL_INS_RACK_MTRL_BOX_STCK_INS");
                        }
                    });
                }
                else if (cell.Value.ToString().Contains("REQUEST") || cell.Value.Equals("CANCEL_FAIL"))
                {
                    // 자재요청중일때 요청 취소
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SUF9001"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetMaterialRequest(datagrid.CurrentRow.Index, cell.Column.Name, "BR_MTRL_UPD_RACK_MTRL_BOX_STCK_CNCL");
                        }
                    });
                }

            }
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

        #endregion

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;
                bool bSply = (bool)((System.Windows.Controls.Primitives.ToggleButton)e.OriginalSource).IsChecked;

                string sMSG = bSply == true ? "SUF9005" : "SUF9006";

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = grdDetail.Rows[idx].DataItem;

                if (objRowIdx != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun(sMSG), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(grdDetail.Rows[idx].DataItem, "AUTO_SPLY_FLAG", bSply);

                            SetAutoSPLYFlag(idx, bSply);
                        }
                        else
                        {
                            DataTableConverter.SetValue(grdDetail.Rows[idx].DataItem, "AUTO_SPLY_FLAG", bSply == true ? false : true);
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                    SetGrdDetail(sParam);
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
        
        /// <summary>
        /// 자재 반품 요청 버튼
        /// </summary>
        /// <param name= "sender"></param>
        /// <param name= "e"></param>
        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (sParam == null)
                //{
                //    ms.AlertInfo("SUF9002"); //정상처리되었습니다.
                //    return;
                //}

                PACK003_035_REMAIN_POPUP popup = new PACK003_035_REMAIN_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    //object[] Parameters = null;
                    //Parameters = new object[] { sParam[3], sParam[0] };

                    //C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.Closed += Popup_Closed;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 사전 자재 요청 버튼
        /// </summary>
        /// <param name= "sender"></param>
        /// <param name= "e"></param>
        private void btnBtr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sParam == null)
                {
                    ms.AlertInfo("SUF9002"); //정상처리되었습니다.
                    return;
                }

                PACK003_035_POPUP popup = new PACK003_035_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = null;
                    Parameters = new object[] { sParam[3], sParam[0] };

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.Closed += Popup_Closed;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnBoxList_Click(string MTEL_PORT_ID,string BOX_ID, string STAT_CODE)
        {
            try
            {
                if (sParam == null)
                {
                    ms.AlertInfo("SUF9002"); //정상처리되었습니다.
                    return;
                }

                PACK003_035_BOXINFO_POPUP popup = new PACK003_035_BOXINFO_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = null;
                    Parameters = new object[] { MTEL_PORT_ID, BOX_ID, STAT_CODE};

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 팝업 닫을때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_Closed(object sender, EventArgs e)
        {
            this.SearchProcess();
            SetGrdDetail(sParam);
        }

        // Validation Check
        private bool ValidationCheckRequest()
        {
            return true;
        }

        private void SetAutoSPLYFlag(int idx, bool apy)
        {
            try
            {
                if (grdDetail == null)
                {
                    return;
                }

                string sMTRL_PORT_ID = string.Empty;
                string sMTRLID = string.Empty;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("AUTO_SPLY_FLAG", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                sMTRL_PORT_ID = Util.NVC(grdDetail.GetCell(idx, grdDetail.Columns["MTRL_PORT_ID"].Index).Value);
                sMTRLID = Util.NVC(grdDetail.GetCell(idx, grdDetail.Columns["MTRLID"].Index).Value);

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["MTRL_PORT_ID"] = sMTRL_PORT_ID;
                dr["MTRLID"] = sMTRLID;
                dr["AUTO_SPLY_FLAG"] = apy == true ? "Y" : "N";
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_MTRL_INS_MTRL_PORT_AUTO_INFO", "INDATA", null, INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_WO_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    if (apy)
                        ms.AlertInfo("SUF9016"); //해당 랙/자재는 소진시 자동요청 되도록 설정 되었습니다.
                    else
                        ms.AlertInfo("SUF9004"); //해당랙/자재는 소진시 자동요청 되지 않도록 설정 되었습니다.
                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자재요청/취소
        /// </summary>
        /// <param name="idx"></param>
        private void SetMaterialRequest(int idx, string colName, string bizName)
        {
            try
            {
                if (grdDetail == null)
                {
                    return;
                }

                string sMTRL_PORT_ID = string.Empty;
                string sMTRLID = string.Empty;
                string sREQSTATCODE = string.Empty;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("REQ_STAT_CODE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                sMTRL_PORT_ID = Util.NVC(grdDetail.GetCell(idx, grdDetail.Columns["MTRL_PORT_ID"].Index).Value);
                sMTRLID = Util.NVC(grdDetail.GetCell(idx, grdDetail.Columns["MTRLID"].Index).Value);
                sREQSTATCODE = Util.NVC(grdDetail.GetCell(idx, grdDetail.Columns[colName].Index).Value);

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRL_PORT_ID"] = sMTRL_PORT_ID;
                dr["MTRLID"] = sMTRLID;
                dr["REQ_STAT_CODE"] = sREQSTATCODE;
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                if (dr == null)
                    return;

                new ClientProxy().ExecuteService(bizName, "INDATA", null, INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ms.AlertInfo("SFU1275"); //정상처리되었습니다.

                    this.SearchProcess();

                    SetGrdDetail(sParam);
                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowPopup(string sPortid, string sMtrlid)
        {
            try
            {
                this.popupAlert.Tag = null;
                this.popupAlert.Tag = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = sPortid;
                dr["MTRLID"] = sMtrlid;

                RQSTDT.Rows.Add(dr);

                DataRow drData = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MTRL_PORT_DETL_INFO", "RQSTDT", "RSLTDT", RQSTDT).Rows[0];

                txtMessageAlert1.Text = drData["MTRL_PORT_ID"].ToString();
                txtMessageAlert2.Text = drData["MTRLNAME"].ToString();
                txtMessageAlert3.Text = drData["REPACK_WH_NM"].ToString();
                txtMessageAlert4.Text = drData["KEP_BOX_QTY"].ToString();
                txtMessageAlert5.Text = drData["CATN_BOX_QTY"].ToString();
                txtMessageAlert6.Text = drData["DNGR_BOX_QTY"].ToString();
                txtMessageAlert7.Text = drData["NOTE"].ToString();

                this.popupAlert.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                this.popupAlert.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //사용여부 Combo 설정
        private void setUseYN()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("자동 공급");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("수동 공급");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboAutoSPLYYN.ItemsSource = DataTableConverter.Convert(dt);
                cboAutoSPLYYN.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    #region #999. DataHelper (Biz 호출)
    internal class PACK003_035_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_035_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists...
        
        internal DataTable GetRackData(string EqsgID)
        {
            string bizRuleName = "DA_MTRL_SEL_MTRL_PORT_CBO";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(EqsgID) || EqsgID.Equals("ALL") ? null : EqsgID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }
        
        #endregion #999-3. Member Function Lists...
    }
    #endregion #999. DataHelper (Biz 호출)
}
