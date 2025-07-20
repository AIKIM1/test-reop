/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.10.26  김도형    : [E20231017-000661] Adding Polarity Menu in Process Monitoring





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_031 : UserControl
    {
        #region Declaration & Constructor 

        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        COM001_031_TESTMODE wndTestMode;
         
        public COM001_031()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            TimerSetting();

            InitCombo();

            SetEltrTypeSearchConditionFlag(); // [E20231017-000661] Adding Polarity Menu in Process Monitoring

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void TimerSetting()
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 5);
            _timer.Tick += new EventHandler(timer_Tick);
            
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                ClearGrid();
                getEquipmentStatus();
                //Util.Alert("Timer");
            }
            catch (Exception ex)
            {
                Util.Alert("Timer_Err" + ex.ToString());
            }
        }

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            //공정군
            C1ComboBox[] cboProcessSegmentParent = { cboArea };
            _combo.SetCombo(cboProcessSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessSegmentParent);

            string[] sFilter = { "REFRESH_TERM" };
            _combo.SetCombo(cboTerm, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            // 극성 유형
            cboElectrodeType.SetCommonCode("ELTR_TYPE_CODE", CommonCombo.ComboStatus.ALL);  // [E20231017-000661] Adding Polarity Menu in Process Monitoring

        }

        // [E20231017-000661] Adding Polarity Menu in Process Monitoring
        private void SetEltrTypeSearchConditionFlag()
        {
            string sEltrTypeSearchConditionFlag ="N";

            sEltrTypeSearchConditionFlag = GetEltrTypeSearchConditionFlag(); 

            if (!string.Equals(sEltrTypeSearchConditionFlag, "Y")) 
            {
                lblElectrodeType.Visibility = Visibility.Collapsed;
                cboElectrodeType.Visibility =Visibility.Collapsed; ;
            }
        }

        /// <summary>
        /// 색지도초기화
        /// </summary>
        private void ClearGrid()
        {
            try
            {
                foreach (Label _label in _grid.Children.OfType<Label>())
                {
                    _grid.UnregisterName(_label.Name);
                }

                foreach (StackPanel _stack in _grid.Children.OfType<StackPanel>())
                {
                    _grid.UnregisterName(_stack.Name);
                }
                

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            ClearGrid();
            getEquipmentStatus();

            if (!Util.GetCondition(cboTerm).Equals(""))
            {
                _timer.Interval = new TimeSpan(0, 0, 0, Convert.ToInt16(Util.GetCondition(cboTerm)));
                _timer.Start();
            }
        }
        #endregion

        #region Mehod
        private void getEquipmentStatus()
        { 
            try
            {   
                DataTable dtRslt = new DataTable();
                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MAIN_FLAG", typeof(string));
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string)); // [E20231017-000661] Adding Polarity Menu in Process Monitoring

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수항목입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["MAIN_FLAG"] = (chkMain.IsChecked.Equals(true)) ? "M" : "A";
                dr["PCSGID"] = Util.GetCondition(cboProcessSegment, bAllNull: true);
                                  
                dr["ELTR_TYPE_CODE"] = (string)cboElectrodeType.GetBindValue(); // [E20231017-000661] Adding Polarity Menu in Process Monitoring  

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_MONITORING", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dtCol = dtRslt.DefaultView.ToTable(true, "PROCID", "PROCNAME");

                    DataTable dtRow = dtRslt.DefaultView.ToTable(true, "EQSGID", "EQSGNAME");

                    int iRowCnt = dtRow.Rows.Count + 1;
                    int iColCnt = dtCol.Rows.Count + 1;

                    for (int i = 0; i <= iColCnt; i++)
                    {
                        ColumnDefinition gridCol1 = new ColumnDefinition();
                        gridCol1.Width = GridLength.Auto;

                        _grid.ColumnDefinitions.Add(gridCol1);

                    }

                    for (int i = 0; i <= iRowCnt; i++)
                    {
                        RowDefinition gridRow1 = new RowDefinition();
                        //gridRow1.Height = new GridLength(50);
                        gridRow1.Height = GridLength.Auto;
                        _grid.RowDefinitions.Add(gridRow1);

                    }

                    //헤더 셋팅 공정
                    for (int iCol = 1; iCol < iColCnt; iCol++)
                    {
                        Label _lable = new Label();
                        _lable.Content = dtCol.Rows[iCol -1]["PROCNAME"];
                        _lable.Style = (Style)FindResource("Local_LabelStyle");
                        _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        _lable.Tag = iCol;

                        _lable.Name = dtCol.Rows[iCol - 1]["PROCID"].ToString();

                        _lable.Background = new SolidColorBrush(Colors.LightGray);

                        //_lable.Background = new SolidColorBrush(Colors.Red);
                        Grid.SetColumn(_lable, iCol);
                        Grid.SetRow(_lable, 0);
                        _grid.Children.Add(_lable);

                        _grid.RegisterName(_lable.Name, _lable);
                    }

                    //헤더 셋팅 라인
                    for (int iRow = 1; iRow < iRowCnt; iRow++)
                    {
                        Label _lable = new Label();
                        _lable.Content = dtRow.Rows[iRow - 1]["EQSGNAME"];
                        _lable.Style = (Style)FindResource("Local_LabelStyle");
                        _lable.Width = 190;
                        _lable.Margin = new Thickness(5, 5, 5, 5);
                        //_lable.BorderThickness = new Thickness(1, 1, 1, 1);
                        //_lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        _lable.Tag = iRow;

                        _lable.Name = dtRow.Rows[iRow - 1]["EQSGID"].ToString();

                        _lable.Background = new SolidColorBrush(Colors.LightGray);

                        //_lable.Background = new SolidColorBrush(Colors.Red);

                        Border _border = new Border();
                        _border.Background = new SolidColorBrush(Colors.LightGray);
                        _border.BorderBrush = new SolidColorBrush(Colors.Gray);
                        _border.BorderThickness = new Thickness(1, 1, 1, 1);
                        Grid.SetColumn(_border, 0);
                        Grid.SetRow(_border, iRow);
                        _grid.Children.Add(_border);

                        Grid.SetColumn(_lable, 0);
                        Grid.SetRow(_lable, iRow);
                        _grid.Children.Add(_lable);

                        _grid.RegisterName(_lable.Name, _lable);
                    }

                    //각 CELL 에 stackpanel 추가
                    //호기가 여러개인경우 처리 위해서
                    for (int iRow = 1; iRow < iRowCnt; iRow++)
                    {
                        for (int iCol = 1; iCol < iColCnt; iCol++)
                        {
                            StackPanel _stack = new StackPanel();
                            _stack.Margin = new Thickness(0, 0, 0, 0);
                            _stack.Orientation = Orientation.Vertical;
                            _stack.Name = "ROW"+iRow+"COL"+iCol;
                            Grid.SetColumn(_stack, iCol);
                            Grid.SetRow(_stack, iRow);
                            _grid.Children.Add(_stack);
                            _grid.RegisterName(_stack.Name, _stack);
                        }
                    }

                    //데이타 셋팅
                    for (int iCnt = 0; iCnt < dtRslt.Rows.Count; iCnt++)
                    {
                        Label _lable = new Label();
                        _lable.Content = dtRslt.Rows[iCnt]["EQPTNAME"];
                        //_lable.FontSize = 11;
                        _lable.Style = (Style)FindResource("Local_LabelStyle");
                        _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                        //_lable.BorderBrush = new SolidColorBrush(getColor(dtRslt.Rows[iCnt]["EIOSTAT"].ToString()));
                        _lable.BorderBrush = new SolidColorBrush(Colors.White);

                        Label lblCol = _grid.FindName(dtRslt.Rows[iCnt]["PROCID"].ToString()) as Label;

                        Label lblRow = _grid.FindName(dtRslt.Rows[iCnt]["EQSGID"].ToString()) as Label;

                        StackPanel sp = _grid.FindName("ROW" + lblRow.Tag.ToString() + "COL" + lblCol.Tag.ToString()) as StackPanel;

                        _lable.Background = new SolidColorBrush(getColor(dtRslt.Rows[iCnt]["EIOSTAT"].ToString()));

                        _lable.Foreground = new SolidColorBrush(getFontColor(dtRslt.Rows[iCnt]["EIOSTAT"].ToString()));

                        ////_lable.Background = new SolidColorBrush(Colors.Red);
                        //Grid.SetColumn(_lable, Convert.ToInt16(lblCol.Tag));
                        //Grid.SetRow(_lable, Convert.ToInt16(lblRow.Tag));
                        sp.Children.Add(_lable);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private Color getColor(string sStatus)
        {
            Color cReturn = Colors.White;
            switch (sStatus)
            {
                case "IDLE":
                case "W":
                    cReturn = Colors.Yellow;
                    break;
                case "RUN":
                case "R":
                    cReturn = Colors.Green;
                    break;
                case "TROUBLE":
                case "T":
                    cReturn = Colors.Red;
                    break;
                case "USERSTOP":
                case "U":
                    cReturn = Colors.Gray;
                    break;
                case "F":
                    cReturn = Colors.Black;
                    break;
            }

            return cReturn;

        }

        private Color getFontColor(string sStatus)
        {
            Color cReturn = Colors.White;
            switch (sStatus)
            {   case "F":
                    cReturn = Colors.White;
                    break;
                default:
                    cReturn = Colors.Black;
                    break;
            }

            return cReturn;

        }

        // [E20231017-000661] Adding Polarity Menu in Process Monitoring
        private string GetEltrTypeSearchConditionFlag()
        {
            string sEltrTypeSearchConditionFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "COM_TYPE_CODE";
            sCmCode = "ELTR_TYPE_SEARCH_CONDITION_FLAG"; // 비고 HOLD 사용여부 

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrTypeSearchConditionFlag = "Y";

                }
                else
                {
                    sEltrTypeSearchConditionFlag = "N";
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                sEltrTypeSearchConditionFlag = "N";
            }

            return sEltrTypeSearchConditionFlag;

        }

        #endregion

        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (wndTestMode != null)
                wndTestMode = null;

            wndTestMode = new COM001_031_TESTMODE();
            wndTestMode.FrameOperation = FrameOperation;

            if (wndTestMode != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = cboArea.SelectedValue;
                C1WindowExtension.SetParameters(wndTestMode, Parameters);

                wndTestMode.Closed += new EventHandler(wndTestMode_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndTestMode.ShowModal()));
            }
        }

        private void wndTestMode_Closed(object sender, EventArgs e)
        {
            wndTestMode = null;
            COM001_031_TESTMODE window = sender as COM001_031_TESTMODE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
    }
}
