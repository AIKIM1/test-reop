/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  
[요청번호]C20171122_37527  조용수사원

  2024.07.04  김도형    : [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건
  2024.07.11  유명환    : [E20240717-000844] [생산 PI] 포장전극(전극Biz) 화면 추가
  
  
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_025 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public Boolean ReFlash = false;

        bool isPackElecUse = false;
        string sShiptoIds = "";

        public class ResultElement
        {
            public RadioButton radButton = null;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }

        public C1ComboBox DATE_COMBO { get; set; }

        public int DATECharge = 30;
        public int igetstdRate = 0;

        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        // [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건
        private string _CheckRollPressAlarmUseYn = string.Empty;
        private bool _isTimerPopupRollPress = false;
        private string _RollPressEndIntervalMinutes = string.Empty;

        public COM001_025()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        //컬럼 합치기 추가
        public C1DataGrid QUALITY2_GRID { get; set; }

        private void TimerSetting()
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 60);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                //btnRefresh_Click(null, null);
                Search_Store();
                Search_Status();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Search_Store();
            Search_Status();
            //ClearGrid();
            //Grid_Draw();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            chkPolEx.IsChecked = true;
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            _timer.Start();
        }

        private void dgList_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
            TimerSetting();
        }
        Util _Util = new Util();

        #endregion

        #region Initialize
        private void Initialize()
        {
            #region Declaration
            rdoALL.Tag = string.Empty;
            rdoJumbo.Tag = "J";
            rdoPancake.Tag = "P";
            //rdoSRS.Tag = "S";
            #endregion

            rdoALL.IsChecked = true;
            Set_RadioButton_GR();
            Set_COMBO();

            DataTable dtPackElec = GetAreaCommonCode("IS_MONITERING_PACKELEC_USE", "USE_YN");

            if (dtPackElec != null && dtPackElec.Rows.Count > 0)
            {
                isPackElecUse = true;
                sShiptoIds = dtPackElec.Rows[0]["ATTR1"].ToString();
                igetstdRate = Int32.Parse(dtPackElec.Rows[0]["ATTR2"].ToString());
            }
            

            if (!isPackElecUse)
            {
                rdoPackElec.Visibility = Visibility.Collapsed;
            }
        }

        private void Set_COMBO()
        {
            cboDate.Items.Clear();
            for (int i = 1; i <= 3; i++)
            {
                cboDate.Items.Add(new C1ComboBoxItem() { Content = (i + ObjectDic.Instance.GetObjectName("개월이내")).ToString(), Tag = (i * 30) });
            }
            cboDate.SelectedIndex = 0;
        }

        public static List<ResultElement> RadioButtonList(DataTable dt)
        {
            List<ResultElement> lst = new List<ResultElement>();
            DataTable dt2 = dt as DataTable;
            for (int row = 0; row < dt2.Rows.Count; row++)
            {
                lst.Add(new ResultElement { radButton = new RadioButton() { GroupName = "RadioButton_" + LoginInfo.CFG_AREA_ID, Name = dt2.Rows[row][2].ToString(), Tag = dt2.Rows[row][3].ToString(), Content = dt2.Rows[row][1].ToString() + " ", VerticalAlignment = VerticalAlignment.Center } });
            }
            return lst;
        }

        private void Set_RadioButton_GR()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_WH_MONITORING_RADIO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        List<ResultElement> elemList;
                        elemList = RadioButtonList(result);
                        if (elemList.Count > 0)
                        {
                            SetResult(elemList, Area);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void SetResult(List<ResultElement> elementList, Grid grid)
        {
            int elementCol = 0;
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            int colIndex = 0;

            foreach (ResultElement re in elementList)
            {
                if (re.radButton != null)
                {
                    re.radButton.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    re.radButton.Margin = new Thickness(10, 0, 5, 0);
                    if (elementCol == 0)
                        re.radButton.IsChecked = true;
                    elementCol++;
                    re.radButton.SetValue(Grid.ColumnProperty, elementCol);
                    re.radButton.Checked += RadioButton_Checked;
                    Area.Children.Add(re.radButton);
                }
                colIndex += re.SpaceInCharge;
            }
            Search_Status();
        }
        #endregion

        #region Event

        /// <summary>
        /// 좌상단 라디오버튼 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Data_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rdo = sender as RadioButton;
            /*
            if (rdo.IsChecked.Value)
            {
                if (rdoALL.IsChecked.Value || rdoJumbo.IsChecked.Value || rdoPancake.IsChecked.Value || rdoSRS.IsChecked.Value)
                {
                    Search_Store();
                }
            }
            */

            DataTable dtPackElec = GetAreaCommonCode("IS_MONITERING_PACKELEC_USE", "USE_YN");

            if (dtPackElec != null && dtPackElec.Rows.Count > 0)
            {
                isPackElecUse = true;
                sShiptoIds = dtPackElec.Rows[0]["ATTR1"].ToString();
                igetstdRate = Int32.Parse(dtPackElec.Rows[0]["ATTR2"].ToString());
            }

            if (rdoPackElec.IsChecked.Value)
            {
                chkPolEx.Visibility = Visibility.Collapsed;
                chkOldEx.Visibility = Visibility.Collapsed;
                chkHold.Visibility = Visibility.Collapsed;

                dgStore.Columns["AC"].Visibility = Visibility.Collapsed;
                dgProcess.Columns["AC"].Visibility = Visibility.Collapsed;
            }
            else
            {
                chkPolEx.Visibility = Visibility.Visible;
                chkOldEx.Visibility = Visibility.Visible;
                chkHold.Visibility = Visibility.Visible;

                dgStore.Columns["AC"].Visibility = Visibility.Visible;
                dgProcess.Columns["AC"].Visibility = Visibility.Visible;
            }

            if (rdo.IsChecked.Value)
            {
                if (rdoALL.IsChecked.Value || rdoJumbo.IsChecked.Value || rdoPancake.IsChecked.Value || rdoPackElec.IsChecked.Value)
                {
                    Search_Store();
                }
            }
        }

        /// <summary>
        /// 우상단 라디오버튼 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rdo = sender as RadioButton;

            if (rdo.IsChecked.Value)
            {
                Search_Status();
            }
        }

        private void _lable2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lblName = (Label)sender;

            Label lblRack = dgList.FindName(lblName.Name.ToString().Replace("VALUE", "RACK")) as Label;

            foreach (Grid gd in pnlRadioButton.Children)
            {
                if (gd.Visibility == Visibility.Visible)
                {
                    foreach (RadioButton rb in gd.Children)
                    {
                        if (rb.IsChecked.Value)
                            if (rb.Name.IndexOf("SRS") == -1)
                            {
                                string sText = "COMM_CONV";
                                if (rb.Name.Equals("EPINSPWait") || rb.Name.Equals("EPJMBOC"))
                                {
                                    sText = "PackElec";
                                }
                                COM001_025_RACK wndPopup = new COM001_025_RACK(lblRack.Content.ToString(), sText, sShiptoIds);


                                wndPopup.FrameOperation = FrameOperation;
                                wndPopup.Show();
                            }
                            else
                            {
                                COM001_025_SRS_RACK wndPopup = new COM001_025_SRS_RACK(lblRack.Content.ToString(), "COMM_CONV");


                                wndPopup.FrameOperation = FrameOperation;
                                wndPopup.Show();
                            }

                    }
                }
            }
            //Popup창 생성
            //COM001_025_RACK wndPopup = new COM001_025_RACK(lblRack.Content.ToString(), "COMM_CONV");


            //wndPopup.FrameOperation = FrameOperation;
            //wndPopup.Show();
        }

        private void _lable3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lbName = (Label)sender;

            string sCheck = "";
            sCheck = lbName.Content.ToString();

            if (sCheck == "0")
            {
                return;
            }

            string[] sRoWCol = lbName.Tag.ToString().Split(new char[] { '-' });
            Label lblRack = dgList.FindName("RACK" + lbName.Tag) as Label;
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(lblRack.Content.ToString());

            //Popup창 생성
            //   COM001_025_RACK wndPopup = new COM001_025_RACK(lblRack.Content.ToString(), "D_Click");
            //   wndPopup.FrameOperation = FrameOperation;
            //   wndPopup.Show();

            //if (wndPopup != null)
            //{
            //    wndPopup.Closed += new EventHandler(wndPopup_Closed);
            //}

        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            //COM001_025_RACK window = sender as COM001_025_RACK;
            //if (window.DialogResult == MessageBoxResult.OK)
            //{
            //    //GetList();
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            if (!ReFlash)
            {
                Search_Status();
                //if (!string.IsNullOrWhiteSpace(txtModel.Text))
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1d);
                timer.Tick += (timeSender, arg) =>
                {
                    timer.Stop();
                    SearchData();
                };
                timer.Start();
            }
            else
            {
                SearchData();
            }
            //if (!string.IsNullOrWhiteSpace(txtLotid.Text)&& string.IsNullOrWhiteSpace(txtModel.Text))
            //    SearchData2();
            _timer.Start();
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {
            //특이Lot조회
            COM001_025_UNUSUAL wndPopup = new COM001_025_UNUSUAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[1];
                C1WindowExtension.SetParameters(wndPopup, Parameters);
                Parameters[0] = GetWarehouseID();
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnReInput_Click(object sender, RoutedEventArgs e)
        {
            //노칭에서 다시 재입고된 LOT_List
            COM001_025_REIN_RACK wndPopup = new COM001_025_REIN_RACK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnNotInput_Click(object sender, RoutedEventArgs e)
        {
            //공정후 or 공장,Shop 이동되어 창고에 미입고된 LOT_List
            try
            {
                //Popup창 생성
                COM001_025_NOTIN_RACK wndPopup = new COM001_025_NOTIN_RACK();
                wndPopup.FrameOperation = FrameOperation;
                wndPopup.Show();

                if (wndPopup != null)
                {
                    wndPopup.Closed += new EventHandler(wndPopup_Closed);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOutLotid.Text))
                {
                    //출고 처리할 LOT 정보가 존재하지 않습니다.
                    Util.MessageInfo("SFU3217", (Result) =>
                    {
                        return;
                    });
                    return;
                }

                #region INDATA
                DataSet inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);
                #endregion

                #region INLOT
                DataTable InLot = inDataSet.Tables.Add("INLOT");
                InLot.Columns.Add("LOTID", typeof(string));

                DataRow drInlot = InLot.NewRow();

                foreach (Grid gd in pnlRadioButton.Children)
                {
                    if (gd.Visibility == Visibility.Visible)
                    {
                        foreach (RadioButton rb in gd.Children)
                        {
                            if (rb.IsChecked.Value)
                                if (rb.Name.IndexOf("SRS") == -1)
                                {
                                    //SRS 아니면 기존 방식대로 LOTID 하나입력
                                    drInlot = InLot.NewRow();
                                    drInlot["LOTID"] = txtOutLotid.Text;
                                    InLot.Rows.Add(drInlot);
                                }
                                else
                                {
                                    //SRS 이면 파렛트에 속해있는 LOTID들 추가 
                                    DataSet dsInput = new DataSet();
                                    DataTable dtResult = null;
                                    DataTable INDATA2 = new DataTable();
                                    INDATA2.TableName = "INDATA";
                                    INDATA2.Columns.Add("LANGID", typeof(string));
                                    INDATA2.Columns.Add("LOTID", typeof(string));
                                    INDATA2.Columns.Add("AREAID", typeof(string));
                                    INDATA2.Columns.Add("EQSGID", typeof(string));
                                    INDATA2.Columns.Add("RESNFLAG", typeof(string));
                                    INDATA2.Columns.Add("TO_PROCID", typeof(string));
                                    INDATA2.Columns.Add("FLAG", typeof(string));
                                    INDATA2.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                                    INDATA2.Columns.Add("BR_TYPE", typeof(string));

                                    
                                    DataRow dr2 = INDATA2.NewRow();
                                    dr2["LANGID"] = LoginInfo.LANGID;
                                    dr2["LOTID"] = txtOutLotid.Text;
                                    dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                                    dr2["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                                    //dr2["RESNFLAG"] = "";
                                    //dr2["TO_PROCID"] = "E7000";// 아직 미사용 
                                    dr2["FLAG"] = "SRS";
                                    dr2["BLOCK_TYPE_CODE"] = "STOCK_OUT_STORAGE";    //NEW HOLD Type 변수
                                    dr2["BR_TYPE"] = "P_ELEC";                       //OLD BR Search 변수

                                    INDATA2.Rows.Add(dr2);
                                    dsInput.Tables.Add(INDATA2);

                                    //BR_PRD_CHK_OUT_ELEC_LOT -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                                    //신규 HOLD 적용을 위해 변경 작업
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", dsInput, null);

                                    if (dsResult != null && dsResult.Tables.Count > 0)
                                    {
                                        if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                        {
                                            dtResult = dsResult.Tables["OUTDATA"]; 
                                            drInlot = null;
                                            for (int i = 0; i < dtResult.Rows.Count; i++)
                                            {
                                                drInlot = InLot.NewRow();
                                                drInlot["LOTID"] = Util.NVC(dtResult.Rows[i]["LOTID"]);
                                                InLot.Rows.Add(drInlot);
                                            }
                                        }
                                    }
                                }
                        }
                    }
                }

                #endregion

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT,INRESN", null, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        return;
                    }
                    else
                    {
                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (Result) =>
                        {
                            Search_Status();
                        });
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

        }

        private void dgStore_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                int istdRate = 80;

                if (rdoPackElec.IsChecked.Value)
                {
                    istdRate = igetstdRate; // 포장전극시 별도 기준적용
                }

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    int index = e.Cell.Row.Index;
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "GUBUN1" || e.Cell.Column.Name == "GUBUN2")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#eaeaea"));
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "GUBUN2") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN2").ToString().Equals("점유율"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF00"));
                                }

                                if (string.Equals(e.Cell.Column.Name, "CC") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN2").ToString().Equals("점유율"))
                                {
                                    if (Int32.Parse(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CC").ToString().Replace("%", "")) >= istdRate)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFB2D9"));
                                    }
                                }
                                if (string.Equals(e.Cell.Column.Name, "AC") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN2").ToString().Equals("점유율"))
                                {
                                    if (Int32.Parse(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AC").ToString().Replace("%", "")) >= istdRate)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFB2D9"));
                                    }
                                }
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProcess_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "GUBUN1" || e.Cell.Column.Name == "GUBUN2")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#eaeaea"));

                    }
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                
                                if (string.Equals(e.Cell.Column.Name, "GUBUN1") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN1").ToString().Equals("압연률") )
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF00"));
                                }
                                if (string.Equals(e.Cell.Column.Name, "CC") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN1").ToString().Equals("압연률"))
                                {
                                    if (Int32.Parse(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CC").ToString().Replace("%", "")) <= 20)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFB2D9"));
                                    }
                                }
                                if (string.Equals(e.Cell.Column.Name, "AC") && DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "GUBUN1").ToString().Equals("압연률"))
                                {
                                    if (Int32.Parse(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AC").ToString().Replace("%", "")) <= 20)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFB2D9"));
                                    }
                                }
                            }
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }
        private void txtModel_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLotid.Text = string.Empty;
        }

        private void txtLotid_GotFocus(object sender, RoutedEventArgs e)
        {
            txtModel.Text = string.Empty;
        }
        #endregion

        #region Mehod

        /// <summary>
        /// [창고재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchStore()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREA", typeof(string));
                IndataTable.Columns.Add("JUMBOROLL", typeof(string));
                IndataTable.Columns.Add("PANCAKE", typeof(string));
                if (rdoPackElec.IsChecked.Value)
                {
                    IndataTable.Columns.Add("SHIPTOID", typeof(string));
                }
                //IndataTable.Columns.Add("SRS", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREA"] = LoginInfo.CFG_AREA_ID;
                Indata["JUMBOROLL"] = rdoJumbo.Tag;
                Indata["PANCAKE"] = rdoPancake.Tag;
                if (rdoPackElec.IsChecked.Value)
                {
                    Indata["SHIPTOID"] = sShiptoIds;
                }
                //Indata["SRS"] = rdoSRS.Tag;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(GetBizRuleName_V02(), "INDATA", "RSLTDT", IndataTable);

                #region Customizing 창고재고

                string sGbunName = "";
                if (rdoPackElec.IsChecked.Value)
                {
                    sGbunName = "포장전극(BOX)";
                }
                else
                {
                    sGbunName = "팬케익(CUT)";
                }

                if (dtMain.Rows.Count >= 1)
                {
                    DataTable GridData = new DataTable();
                    GridData.Columns.Add("GUBUN1", typeof(string));
                    GridData.Columns.Add("GUBUN2", typeof(string));
                    GridData.Columns.Add("CC", typeof(string));
                    GridData.Columns.Add("CM", typeof(string));
                    GridData.Columns.Add("AC", typeof(string));
                    GridData.Columns.Add("AM", typeof(string));

                    //GridData.Columns.Add("SS", typeof(string));

                    for (int c = 0; c < dtMain.Columns.Count; c++)
                    {
                        if (c % 2 == 0)
                        {
                            string sGubun1 = "";
                            string sGubun2 = "";

                            if (c == 0)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("점보롤(Roll)"); sGubun2 = ObjectDic.Instance.GetObjectName("Capa"); }
                            if (c == 2)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("점보롤(Roll)"); sGubun2 = ObjectDic.Instance.GetObjectName("현수량"); }
                            if (c == 4)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("점보롤(Roll)"); sGubun2 = ObjectDic.Instance.GetObjectName("점유율"); }
                            if (c == 6)
                            { sGubun1 = ObjectDic.Instance.GetObjectName(sGbunName); sGubun2 = ObjectDic.Instance.GetObjectName("Capa"); }
                            if (c == 8)
                            { sGubun1 = ObjectDic.Instance.GetObjectName(sGbunName); sGubun2 = ObjectDic.Instance.GetObjectName("현수량"); }
                            if (c == 10)
                            { sGubun1 = ObjectDic.Instance.GetObjectName(sGbunName); sGubun2 = ObjectDic.Instance.GetObjectName("점유율"); }


                            DataRow setData = GridData.NewRow();
                            setData["GUBUN1"] = sGubun1;
                            setData["GUBUN2"] = sGubun2;
                            setData["CC"] = dtMain.Rows[0][c].ToString();
                            setData["AC"] = dtMain.Rows[0][c + 1].ToString();
                            //setData["SS"] = dtMain.Rows[0][c + 2].ToString();
                            if (sGubun1 != "")
                            {
                                GridData.Rows.Add(setData);
                            }

                        }
                    }
                    dgStore.ItemsSource = DataTableConverter.Convert(GridData);
                }
                #endregion
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        string GetBizRuleName_V02()
        {
            if (rdoPackElec.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_FOR_STORE_V02";
            }
            else if (chkPolEx.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_FOR_STORE_V01";
            }


            return "DA_PRD_SEL_STOCK_FOR_STORE_POL";
        }

        /// <summary>
        /// [공정재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                string sGbunName = "";
                if (rdoPackElec.IsChecked.Value)
                {
                    sGbunName = "점보롤(Roll)";
                }
                else
                {
                    sGbunName = "점보롤";
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREA", typeof(string));
                //IndataTable.Columns.Add("OLDEX", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREA"] = LoginInfo.CFG_AREA_ID;

                //if ((bool)chkOldEx.IsChecked) Indata["OLDEX"] = "N";

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(GetBizRuleName_V01(), "INDATA", "RSLTDT", IndataTable);

                #region Customizing 공정재고
                if (dtMain.Rows.Count >= 1)
                {
                    DataTable GridData = new DataTable();
                    GridData.Columns.Add("GUBUN1", typeof(string));
                    GridData.Columns.Add("GUBUN2", typeof(string));
                    GridData.Columns.Add("CC", typeof(string));
                    GridData.Columns.Add("CM", typeof(string));
                    GridData.Columns.Add("AC", typeof(string));
                    GridData.Columns.Add("AM", typeof(string));
                   //GridData.Columns.Add("SS", typeof(string));



                    for (int c = 0; c < dtMain.Columns.Count; c++)
                    {
                        if (c % 2 == 0)
                        {
                            string sGubun1 = "";
                            string sGubun2 = "";
                            //_Util.SetDataGridMergeExtensionCol(QUALITY2_GRID, new string[] { sGubun1, sGubun2 }, DataGridMergeMode.VERTICALHIERARCHI);
                            // _Util.SetDataGridMergeExtensionCol(dgProcess, columsName, DataGridMergeMode.VERTICAL);

                            if (c == 0)
                            { sGubun1 = ObjectDic.Instance.GetObjectName(sGbunName); sGubun2 = ObjectDic.Instance.GetObjectName("압연전"); }
                            if (c == 2)
                            { sGubun1 = ObjectDic.Instance.GetObjectName(sGbunName); sGubun2 = ObjectDic.Instance.GetObjectName("압연후"); }
                            if (c == 4)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("홀드 재고"); sGubun2 = ObjectDic.Instance.GetObjectName("압연전"); }
                            if (c == 6)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("홀드 재고"); sGubun2 = ObjectDic.Instance.GetObjectName("압연후"); }
                            if (c == 8 && !rdoPackElec.IsChecked.Value)
                            { sGubun1 = ObjectDic.Instance.GetObjectName("홀드 재고"); sGubun2 = ObjectDic.Instance.GetObjectName("팬케익"); }


                            DataRow setData = GridData.NewRow();
                            setData["GUBUN1"] = sGubun1;
                            setData["GUBUN2"] = sGubun2;
                            setData["CC"] = dtMain.Rows[0][c].ToString();
                            setData["AC"] = dtMain.Rows[0][c + 1].ToString();
                            // setData["SS"] = dtMain.Rows[0][c + 2].ToString();

                            if (c == 10)
                            {
                                sGubun1 = ObjectDic.Instance.GetObjectName("압연률"); sGubun2 = ObjectDic.Instance.GetObjectName("압연률");
                                string[] columsName = new string[] {"GUBUN1", "GUBUN2"};                                
                                _Util.SetDataGridMergeExtensionCol(dgProcess, columsName, DataGridMergeMode.VERTICALHIERARCHI);                                
                                setData["GUBUN1"] = sGubun1;
                                setData["GUBUN2"] = sGubun2;
                            }

                            if (sGubun1 != "")
                            {
                                GridData.Rows.Add(setData);
                            }
                        }

                    }
                    dgProcess.ItemsSource = DataTableConverter.Convert(GridData);
                }
                #endregion
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        string GetBizRuleName_V01()
        {
            if (rdoPackElec.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_BY_ALL_PROCESS_V01";
            }
            else if (chkOldEx.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_BY_ALL_PROCESS_V02";
            }
            

            return "DA_PRD_SEL_STOCK_BY_ALL_PROCESS_V01";
        }


        /// <summary>
        /// [유효기간 임박재고] 데이터를 조회합니다.
        /// </summary>
        private void SearchVLDate()
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREA", typeof(string));
                IndataTable.Columns.Add("WIPHOLD", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREA"] = LoginInfo.CFG_AREA_ID;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = DATECharge;
                if ((bool)chkHold.IsChecked) Indata["WIPHOLD"] = "N";
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(GetBizRuleName(), "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count >= 1)
                {
                    dgMonth.ItemsSource = DataTableConverter.Convert(dtMain);
                }
                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        string GetBizRuleName()
        {
            if (rdoJumbo.IsChecked.Value || rdoPackElec.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_BY_JUMBOROLL_VLDATE";
            }
            else if (rdoPancake.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_BY_PANCAKE_VLDATE";
            }
            /*else if (rdoSRS.IsChecked.Value)
            {
                return "DA_PRD_SEL_STOCK_BY_SRS_VLDATE";
            }*/

            return "DA_PRD_SEL_STOCK_BY_ALL_VLDATE";
        }

        private void Clear_Grid()
        {
            Util.gridClear(dgStore);
            Util.gridClear(dgProcess);
            Util.gridClear(dgMonth);

        }

        private void ClearGrid()
        {
            
            try
            {
                if (dgList != null)
                {
                    foreach (Label _label in dgList.Children.OfType<Label>())
                    {
                        dgList.UnregisterName(_label.Name);
                    }

                    foreach (StackPanel _stack in dgList.Children.OfType<StackPanel>())
                    {
                        dgList.UnregisterName(_stack.Name);
                    }
                }


                NameScope.SetNameScope(dgList, new NameScope());

                dgList.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void Search_Store()
        {
            Clear_Grid();

            SearchStore();
            SearchProcess();
            SearchVLDate();

            GetCheckRollPressAlarm();// [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건

        }

        private void Search_Status()
        {
            ClearGrid();
            Grid_Draw();
            ReFlash = true;
        }

        private string GetWarehouseID()
        {
            /* AREAID별로 라디오버튼을 구성하여 Grid로 관리
             * 해당 Grid는 pnlRadioButton의 자식
             * Visible상태인 Grid의 라디오버튼중 체크된 것을 가져옴
             */
            string sWH_ID = string.Empty;

            foreach (Grid gd in pnlRadioButton.Children)
            {
                if (gd.Visibility == Visibility.Visible)
                {
                    foreach (RadioButton rb in gd.Children)
                    {
                        if (rb.IsChecked.Value)
                            sWH_ID = (string)rb.Tag;
                    }
                }
            }
            return sWH_ID;
        }
        private void Grid_Draw()
        {
            try
            {
                string sWH_ID = GetWarehouseID();
                if (string.IsNullOrWhiteSpace(sWH_ID)) return;

                string sBizRuleName = GetDrawBizRuleName();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                if(sBizRuleName.Equals("DA_PRD_SEL_STOCK_TO_DRAW_V01"))
                    RQSTDT.Columns.Add("SHIPTOID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = sWH_ID;
                if (sBizRuleName.Equals("DA_PRD_SEL_STOCK_TO_DRAW_V01"))
                    dr["SHIPTOID"] = sShiptoIds;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService(sBizRuleName, "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        try
                        {
                            //헤더 셋팅 (가로)
                            for (int i = 0; i <= 100; i++)
                            {
                                ColumnDefinition gridCol1 = new ColumnDefinition();
                                gridCol1.Width = GridLength.Auto;
                                dgList.ColumnDefinitions.Add(gridCol1);
                            }

                            for (int iCol = 0; iCol <= 100; iCol++)
                            {
                                Label _lable = new Label();
                                _lable.Content = iCol.ToString();
                                _lable.FontSize = 11;
                                _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable.Margin = new Thickness(0, 0, 0, 0);
                                _lable.Padding = new Thickness(0, 0, 0, 0);
                                _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                                _lable.Width = 35;
                                _lable.Height = 5;
                                _lable.Tag = iCol;
                                _lable.Name = "Col" + iCol.ToString(); //요놈
                                _lable.Background = new SolidColorBrush(Colors.LightGray);
                                _lable.Visibility = Visibility.Hidden;

                                Grid.SetColumn(_lable, iCol);
                                Grid.SetRow(_lable, 0);
                                dgList.Children.Add(_lable);
                                dgList.RegisterName(_lable.Name, _lable);
                            }

                            //헤더 셋팅 (세로)
                            for (int i = 0; i <= 100; i++)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                dgList.RowDefinitions.Add(gridRow1);
                            }

                            for (int iRow = 0; iRow <= 100; iRow++)
                            {
                                Label _lable = new Label();
                                _lable.Content = iRow.ToString();
                                _lable.FontSize = 11;
                                _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable.Margin = new Thickness(0, 0, 0, 0);
                                _lable.Padding = new Thickness(0, 0, 0, 0);
                                _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                                _lable.Width = 5;
                                _lable.Height = 15;
                                _lable.Tag = iRow;
                                _lable.Name = "Row" + iRow.ToString();
                                _lable.Background = new SolidColorBrush(Colors.LightGray);
                                _lable.Visibility = Visibility.Hidden;

                                Grid.SetColumn(_lable, 0);
                                Grid.SetRow(_lable, iRow);
                                dgList.Children.Add(_lable);
                                dgList.RegisterName(_lable.Name, _lable);
                            }

                            //데이타 셋팅
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                string sTag = "";
                                string sCol = "";
                                string sRow = "";

                                sTag = result.Rows[i]["X_POSITION"].ToString() + "-" + result.Rows[i]["Y_POSITION"].ToString();
                                sCol = result.Rows[i]["X_POSITION"].ToString();
                                sRow = result.Rows[i]["Y_POSITION"].ToString();

                                Label _lable = new Label();
                                _lable.Content = result.Rows[i]["RACK_ID"].ToString(); //확인
                                _lable.FontSize = 12;
                                _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable.Margin = new Thickness(0, 0, 0, 0);
                                _lable.Padding = new Thickness(0, 0, 0, 0);
                                _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable.Width = 58;
                                _lable.Height = 22;
                                _lable.Tag = result.Rows[i]["RACK_ID"].ToString();
                                _lable.Name = "RACK" + result.Rows[i]["RACK_ID"].ToString();
                                _lable.Background = new SolidColorBrush(Colors.White); //NavajoWhite, Ivory, White

                                if (!sCol.Equals(""))
                                    Grid.SetColumn(_lable, int.Parse(sCol));
                                if (!sRow.Equals(""))
                                    Grid.SetRow(_lable, int.Parse(sRow) + 1);
                                dgList.Children.Add(_lable);
                                dgList.RegisterName(_lable.Name, _lable);

                                Label _lable2 = new Label();
                                _lable2.Content = result.Rows[i]["RACK_CNT"].ToString(); //랙 수량/
                                _lable2.FontSize = 12;
                                _lable2.HorizontalContentAlignment = HorizontalAlignment.Center;
                                _lable2.VerticalContentAlignment = VerticalAlignment.Center;
                                _lable2.Margin = new Thickness(0, 0, 0, 0);
                                _lable2.Padding = new Thickness(0, 0, 0, 0);
                                _lable2.BorderThickness = new Thickness(1, 1, 1, 1);
                                _lable2.Width = 58;
                                _lable2.Height = 22;
                                _lable2.Tag = result.Rows[i]["RACK_CNT"].ToString();
                                _lable2.Name = "VALUE" + result.Rows[i]["RACK_ID"].ToString();
                                _lable2.Background = new SolidColorBrush(getColor(result.Rows[i]["RACK_CNT"].ToString(), result.Rows[i]["MAX_QTY"].ToString()));
                                //상세데이터 조회
                                _lable2.MouseDoubleClick += _lable2_MouseDoubleClick;
                                if (!sCol.Equals(""))
                                    Grid.SetColumn(_lable2, int.Parse(sCol) + 1);
                                if (!sRow.Equals(""))
                                    Grid.SetRow(_lable2, int.Parse(sRow) + 1);
                                dgList.Children.Add(_lable2);
                                dgList.RegisterName(_lable2.Name, _lable2);
                            }
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                    //요청에 의한 텍스트 추가건
                    AddText();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        string GetDrawBizRuleName()
        {

            string sBizRuleName = string.Empty;

            foreach (Grid gd in pnlRadioButton.Children)
            {
                if (gd.Visibility == Visibility.Visible)
                {
                    foreach (RadioButton rb in gd.Children)
                    {
                        if (rb.IsChecked.Value)
                        {
                            if (rb.Name.IndexOf("SRS") == -1)
                            {
                                if (rb.Name.Equals("EPINSPWait") || rb.Name.Equals("EPJMBOC"))
                                {
                                    sBizRuleName = "DA_PRD_SEL_STOCK_TO_DRAW_V01";
                                }
                                else
                                {
                                    sBizRuleName = "DA_PRD_SEL_STOCK_TO_DRAW";
                                }
                            }
                            else
                            {
                                sBizRuleName = "DA_PRD_SEL_STOCK_TO_SRS_DRAW";
                            }
                        }
                    }
                }
            }
            return sBizRuleName;
        }



        /// <summary>
        /// 요청에 의한 텍스트를 추가합니다.
        /// </summary>
        private void AddText()
        {
            //텍스트 추가건 (적용은 아직 미정임?)
            //2022-12-29 오화백  동 :EP 추가 
            if (LoginInfo.CFG_AREA_ID == "E5" || LoginInfo.CFG_AREA_ID == "EP")
            {
                //if (rdoRoll1.IsChecked.Value)
                //{
                //    MakeText("양대차(+)", 0, 7);
                //    MakeText("양대차(-)", 0, 11);
                //    MakeText("[코터(+)", 0, 20);
                //    MakeText("복도]", 0, 21);
                //}
                //else if (rdoRoll3.IsChecked.Value)
                //{
                //    MakeText("양대차(+)", 18, 11);
                //    MakeText("양대차(-)", 20, 11);
                //}
                //else if (rdoPancake1A.IsChecked.Value)
                //{
                //    MakeText("[팬케익", 7, 0);
                //    MakeText("보관대]", 7, 1);
                //}
            }
            else if (LoginInfo.CFG_AREA_ID == "E6")
            {
                //if (rdoRoll2.IsChecked.Value)
                //{
                //    MakeText("양대차(+)", 8, 14);
                //    MakeText("양대차(-)", 12, 14);
                //}
                //else if (rdoPanOut.IsChecked.Value)
                //{
                //    MakeText("복도", 1, 1);
                //    MakeText("점보롤실+", 11, 1);
                //    MakeText("슬리터실", 11, 2);
                //    MakeText("2층", 29, 1);
                //    MakeText("3단대차", 11, 16);
                //    MakeText("[조립]", 11, 17);
                //    MakeText("3단대차", 14, 16);
                //    MakeText("[전극]", 14, 17);

                //    MakeText("보관가대", 17, 16);

                //    MakeText("대기가대", 20, 16);

                //    MakeText("입고중", 24, 16);
                //    MakeText("출고중", 25, 16);
                //    MakeText("[COMM]", 23, 17);
                //    MakeText("[CONV]", 23, 18);

                //입출고중[COMM,CONV] 수량 추가 BIZ
                //DataTable RQSTDT = new DataTable("RQSTDT");
                //RQSTDT.Columns.Add("AREAID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_IN_MOVING", "RQSTDT", "RSLTDT", RQSTDT);

                //if (dtResult.Rows.Count > 0)
                //{
                //    MakeText2(dtResult.Rows[0]["FROMCOMM"].ToString(), 24, 17);
                //    MakeText2(dtResult.Rows[0]["FROMCONV"].ToString(), 24, 18);
                //    MakeText2(dtResult.Rows[0]["TOCOMM"].ToString(), 25, 17);
                //    MakeText2(dtResult.Rows[0]["TOCONV"].ToString(), 25, 18);

                //}
                //}
            }
        }

        private void MakeText(string sText, int iRow, int iCol)
        {
            string sRow = "";
            string sCol = "";

            sRow = iRow.ToString();
            sCol = iCol.ToString();

            Label _lable = new Label();
            _lable.Content = sText;
            _lable.FontSize = 12;
            _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
            _lable.VerticalContentAlignment = VerticalAlignment.Center;
            _lable.Margin = new Thickness(0, 0, 0, 0);
            _lable.Padding = new Thickness(0, 0, 0, 0);
            _lable.BorderThickness = new Thickness(1, 1, 1, 1);
            _lable.Width = 58;
            _lable.Height = 22;
            _lable.Tag = sRow + "-" + sCol;
            _lable.Name = "TEXT" + "ROW" + sRow + "COL" + sCol;
            _lable.Background = new SolidColorBrush(Colors.White);
            _lable.Foreground = new SolidColorBrush(Colors.Blue);
            _lable.FontWeight = FontWeights.Bold;

            Grid.SetColumn(_lable, iCol);
            Grid.SetRow(_lable, iRow);
            dgList.Children.Add(_lable);
            dgList.RegisterName(_lable.Name, _lable);

        }

        private void MakeText2(string sText, int iRow, int iCol)
        {
            string sRow = "";
            string sCol = "";

            sRow = iRow.ToString();
            sCol = iCol.ToString();

            Label _lable2 = new Label();
            _lable2.Content = sText;
            _lable2.FontSize = 12;
            _lable2.HorizontalContentAlignment = HorizontalAlignment.Center;
            _lable2.VerticalContentAlignment = VerticalAlignment.Center;
            _lable2.Margin = new Thickness(0, 0, 0, 0);
            _lable2.Padding = new Thickness(0, 0, 0, 0);
            _lable2.BorderThickness = new Thickness(1, 1, 1, 1);
            _lable2.Width = 58;
            _lable2.Height = 22;
            _lable2.Tag = sRow + "-" + sCol;
            _lable2.Name = "TEXT2" + "ROW" + sRow + "COL" + sCol;
            _lable2.Background = new SolidColorBrush(getColor(sText, "1"));

            //상세데이터 조회
            _lable2.MouseDoubleClick += _lable3_MouseDoubleClick;

            Grid.SetColumn(_lable2, int.Parse(sCol));
            Grid.SetRow(_lable2, int.Parse(sRow));
            dgList.Children.Add(_lable2);
            dgList.RegisterName(_lable2.Name, _lable2);
        }

        private Color getColor(string sCnt, string sMax)
        {
            double Yield = 0;
            double iCnt = 0;
            double iMax = 0;
            Color cReturn = Colors.White;
            iCnt = double.Parse(sCnt);
            iMax = double.Parse(sMax);

            //Rack 에 실물이 차지하고 있는 퍼센트 비율 계산 처리
            Yield = Math.Round((iCnt / iMax) * 100, 2);

            if (Yield >= 100)
            {
                cReturn = Colors.LightCoral;
            }
            else if (Yield < 100 && Yield > 66)
            {
                cReturn = Colors.Yellow;
            }
            else if (Yield < 66 && Yield > 33)
            {
                cReturn = Colors.LightSkyBlue; //Aqua   LightSkyBlue
            }
            else if (Yield < 33 && Yield > 0)
            {
                cReturn = Colors.LightGreen;  //Lime  LawnGreen
            }
            else if (Yield <= 0)
            {
                cReturn = Colors.WhiteSmoke;
            }
            else
            {
                cReturn = Colors.WhiteSmoke;
            }

            return cReturn;

        }

        private void SearchData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            if (!string.IsNullOrWhiteSpace(txtModel.Text)) dr["PRJT_NAME"] = txtModel.Text;
            if (!string.IsNullOrWhiteSpace(txtLotid.Text)) dr["LOTID"] = txtLotid.Text;
            RQSTDT.Rows.Add(dr);
            new ClientProxy().ExecuteService(GetDrawBiz2RuleName(), "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
            {
                try
                {
                    if (result == null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            SetGridColor(result.Rows[i]["RACK_ID"].ToString(), result.Rows[i]["LOTCNT"].ToString(), Colors.Bisque); // LightSkyBlue NavajoWhite
                        }
                        ReFlash = false;
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                }
                finally
                {
                }

            });
        }

        string GetDrawBiz2RuleName()
        {

            string sBizRuleName = string.Empty;

            foreach (Grid gd in pnlRadioButton.Children)
            {
                if (gd.Visibility == Visibility.Visible)
                {
                    foreach (RadioButton rb in gd.Children)
                    {
                        if (rb.IsChecked.Value)
                            if (rb.Name.IndexOf("SRS") == -1)
                            {
                                sBizRuleName = "DA_PRD_SEL_STOCK_BY_SERCH_LOT";
                            }
                            else
                            {
                                sBizRuleName = "DA_PRD_SEL_STOCK_BY_SERCH_LOT_SRS";
                            }
                    }
                }
            }
            return sBizRuleName;
        }

        private void SetGridColor(string sRackId, string sCNT, Color color)
        {
            try
            {
                Label lblRack = dgList.FindName("RACK" + sRackId) as Label;
                if (lblRack != null) lblRack.Background = new SolidColorBrush(Colors.Bisque);

                Label lblValue = dgList.FindName("VALUE" + sRackId) as Label;
                if (lblValue != null)
                {
                    lblValue.Content = lblValue.Tag + "(" + sCNT + ")";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void cboDate_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBoxItem cbo = cboDate.SelectedItem as C1ComboBoxItem;
            if (cbo != null)
                DATECharge = Util.StringToInt(cbo.Tag.ToString());
            SearchVLDate();
        }

        private void chkOldEx_Checked(object sender, RoutedEventArgs e)
        {

                Search_Store();

        }

        private void chkOldEx_Unchecked(object sender, RoutedEventArgs e)
        {

                Search_Store();

        }

        private void chkPolEx_Checked(object sender, RoutedEventArgs e)
        {
                Search_Store();
        }

        private void chkPolEx_Unchecked(object sender, RoutedEventArgs e)
        {
                Search_Store();
        }

        // [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건
        private void SetRollPressAlarmUseYn()
        {

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_ROLLPRESS_INNER_PACKING_CHK";
            sCmCode = "USE_YN";

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
                    _RollPressEndIntervalMinutes = Util.NVC(dtResult.Rows[0]["ATTR2"].ToString());
                    _CheckRollPressAlarmUseYn = "Y";
                }
                else
                {
                    _RollPressEndIntervalMinutes = "";
                    _CheckRollPressAlarmUseYn = "N";
                }

                return ;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
                _RollPressEndIntervalMinutes = "";
                _CheckRollPressAlarmUseYn = "N";
                return ;
            }
        }

        // [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건
        private void GetCheckRollPressAlarm()
        {

            SetRollPressAlarmUseYn();

            if (_CheckRollPressAlarmUseYn.Equals("N"))
            {
                return; 
            }
            
            string delayLotIDs = "";

            //메시지 팝업 이중 처리 방지
            if (_isTimerPopupRollPress == true)
            {
                return;
            }

            try
            { 
                delayLotIDs = GetValidateRollPressAlarm();

                if (!delayLotIDs.Equals(""))
                {
                    Util.MessageInfo("101614", (result) =>
                    {
                        _isTimerPopupRollPress = false;

                    }, new object[] { _RollPressEndIntervalMinutes, delayLotIDs, _isTimerPopupRollPress = true });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            } 
        }

        // [E20240605-001165] [생산 PI] 자동차1동(소형) IM 4680 전극 롤프레스 착공 후 알람 기능 추가 요청의 건
        private string GetValidateRollPressAlarm()
        {
            string rtnLotIDs = "";

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
           // IndataTable.Columns.Add("EQPTID", typeof(string));  -- 미사용

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["PROCID"] = Process.ROLL_PRESSING.ToString(); 
            IndataTable.Rows.Add(Indata);

            try
            {
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_INNER_PACKING_RP_FOR_EQSGID", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null &&  dtResult.Rows.Count > 0)
                {
                    int roofcnt = 1;  

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        switch (roofcnt)
                        {
                            case 1:
                                if(i == 0)
                                {
                                    rtnLotIDs = rtnLotIDs + dtResult.Rows[i]["LOTID"].ToString() + ",";
                                }
                                else
                                {
                                    rtnLotIDs = rtnLotIDs + "       " + dtResult.Rows[i]["LOTID"].ToString() + ",";
                                }                                   
                                break;

                            case 3:
                                if(i == (dtResult.Rows.Count-1))
                                {
                                    rtnLotIDs = rtnLotIDs + " " + dtResult.Rows[i]["LOTID"].ToString() + "," ;
                                }
                                else
                                {
                                    rtnLotIDs = rtnLotIDs + " " + dtResult.Rows[i]["LOTID"].ToString() + "," + "\r\n";
                                }
                                roofcnt = 0 ;
                                break;

                            default:
                                   rtnLotIDs = rtnLotIDs + " " + dtResult.Rows[i]["LOTID"].ToString() + ",";
                                break;
                        }
                           
                         roofcnt = roofcnt+1;

                    }

                    rtnLotIDs = rtnLotIDs.Substring(0, rtnLotIDs.Length - 1);
 
                }

            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
                rtnLotIDs = "";
            }

            return rtnLotIDs;
        }

        private DataTable GetAreaCommonCode(string sComTypeCode, string sComCode)
        {
            DataTable dtResult = null;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComTypeCode;
                dr["COM_CODE"] = sComCode;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtResult;
        }
    }
}
