/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

using LGC.GMES.MES.MCS001.Controls;
using C1.WPF;

using System.Windows.Media.Animation;
using System.Reflection;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_001 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer monitorTimer = new System.Windows.Threading.DispatcherTimer();
        //private int refeshInterval = 60;
        //private int refeshTo = 60;
        private string refeshAfter = "{0}";

        private bool IsFirstLoading = true;


        private int ColumnCount;

        // 창고 ID - 폴란드 Skid 창고
        private const string WH_ID = "A5A102";

        private bool bSetAutoSelTime = false;

        private string EmgPORT_ID;

        private DataTable dtCheck;

        private bool bIsManualIssueOK = false;

        private DataView dvRank;

        public MCS001_001()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // get column count(22)
            GetColumnCount();

            MakeColumnDefinition();


            InitStoreGrid();
            InitQAGrid();
            InitInputRankGrid();
            InitPancakeInfoGrid();

            TimerSetting();
        }

        /// <summary>
        /// MCS001_001 Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfig);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);



            // 단콤보 초기화
            InitStairCombo();
            // 출고포트 초기화
            ReloadOutPortCombo();
            // 입고일별 색상 설정
            InitRackColors();

            InitdtCheck();

            InitCombo();

            // 최초한번 로딩
            Refresh();

            CommonCombo _combo = new CommonCombo();

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;


        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            monitorTimer.Stop();
        }

        private void OnMonitorTimer(object sender, EventArgs e)
        {
            try
            {
                //this.InitCombo();
                this.ReInitCombo();
                this.Refresh();

            }
            catch (Exception ex)
            {
                Util.Alert("Timer_Err" + ex.ToString());
            }
        }

        #endregion

        #region Event
        /// <summary>
        /// 자동 새로고침 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChkAutoRefreshClick(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 새로고침 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnRefresh(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 조회 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnSearch(object sender, RoutedEventArgs e)
        {
            //this.InitCombo();
            this.ReInitCombo();
            Refresh();
        }


        /// <summary>
        /// SKID/LOT ID 조회
        /// 화면에서 검색 후 없는 경우 DB에서 검색
        /// </summary>
        private void SearchSKIDLOTID()
        {
            /// animation
            /// 
            DoubleAnimation da = new DoubleAnimation();

            string sLOTID = txtLOTID.Text.Trim();

            if (sLOTID.Length < 3)
            {
                //SFU4147
                Util.AlertInfo("SFU3624");   // 다시 생성 필요
            }
            else
            {
                for (int r = 0; r < rackStair.RowCount; r++)
                {
                    for (int c = 0; c < rackStair.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair[r, c];

                        da.From = sr.Height;
                        da.To = 0;
                        da.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); // 500ms == 0.5s
                        da.AutoReverse = true;

                        if ((sr.PancakeID1).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);

                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }

                        if ((sr.PancakeID2).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);

                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }


                        if ((sr.SkidID).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                    }
                }

                for (int r = 0; r < rackStair2.RowCount; r++)
                {
                    for (int c = 0; c < rackStair2.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair2[r, c];
                        if ((sr.PancakeID1).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }

                        if ((sr.PancakeID2).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                        if ((sr.PancakeID3).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                        if ((sr.PancakeID4).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                        if ((sr.PancakeID5).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                        if ((sr.PancakeID6).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }

                        if ((sr.SkidID).ToUpper() == (sLOTID).ToUpper())
                        {
                            if (!sr.IsChecked)
                            {
                                sr.IsChecked = true;
                                this.CheckSkidRack(sr);
                                sr.BeginAnimation(SkidRack.HeightProperty, da);
                            }
                            return;
                        }
                    }
                }

                // 없는 경우 message box 100119
                // DB에서 검색
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SKIDID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SKIDID"] = sLOTID;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_PSTN_2", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    Util.AlertInfo("100119");
                }
                else
                {
                    ControlsLibrary.MessageBox.Show(dtResult.Rows[0][0].ToString(), "", "Info");
                }
            }
        }
        private void OnbtnLOTIDSearchClick(object sender, RoutedEventArgs e)
        {
            SearchSKIDLOTID();
        }

        /// <summary>
        /// 미입고 LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnNotInput(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_NOT_INPUT_LOT wndPopup = new MCS001_001_NOT_INPUT_LOT();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 재입고 LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnReInput(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_REWORKSKID wndPopup = new MCS001_001_REWORKSKID();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 특이 LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnLotSearch(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_UNUSUAL_LOT wndPopup = new MCS001_001_UNUSUAL_LOT();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 양극/음극 선택 라디오 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRdoAnodeCathodeChecked(object sender, RoutedEventArgs e)
        {
            chkNG.IsChecked = false;

            // 출고포트 초기화
            ReloadOutPortCombo();

            ReloadSkidRack();

            //if (rdoAnode.IsChecked.Value)
            //{
            //    chkArr.IsChecked = false;
            //    this.ArrPos();
            //}
            //else
            //{
            //    chkArr.IsChecked = true;
            //    this.ArrPosX();
            //}
        }

        /// <summary>
        /// 단 콤보 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCboStairSelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

            chkNG.IsChecked = false;

            if (!IsFirstLoading)
            {
                ReloadSkidRack();
            }

            IsFirstLoading = false;

        }

        /// <summary>
        /// POS-EX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChkScrollChecked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 불량 체크박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChkNGChecked(object sender, RoutedEventArgs e)
        {
            // 출고포트 초기화
            ReloadOutPortCombo();

            // check 해제
            rackStair.UncheckRack();
            rackStair2.UncheckRack();

            if (chkNG.IsChecked.Value)
            {
                // 불량 check
                lblEqpt.Visibility = Visibility.Hidden;
                txtEqpt.Visibility = Visibility.Hidden;
                lblModel.Visibility = Visibility.Hidden;
                txtProd.Visibility = Visibility.Hidden;
                lblAutoDeliveryStatus.Visibility = Visibility.Hidden;
                txtAutoDeliveryStatus.Visibility = Visibility.Hidden;
                // 불량 cell만 enable
                for (int r = 0; r < rackStair.RowCount; r++)
                {
                    for (int c = 0; c < rackStair.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair[r, c];
                        if ((sr.UserData.ContainsKey("JUDG_VALUE_1") && sr.UserData["JUDG_VALUE_1"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_2") && sr.UserData["JUDG_VALUE_2"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_3") && sr.UserData["JUDG_VALUE_3"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_4") && sr.UserData["JUDG_VALUE_4"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_5") && sr.UserData["JUDG_VALUE_5"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_6") && sr.UserData["JUDG_VALUE_6"].ToString().Equals("R"))
                        )
                        {
                            // 둘중 하나라도 불량이 존재하면 enable
                            sr.IsEnabled = true;
                            sr.Background = new SolidColorBrush(Colors.White);
                        }
                        else
                        {
                            sr.IsEnabled = false;
                        }
                    }
                }

                for (int r = 0; r < rackStair2.RowCount; r++)
                {
                    for (int c = 0; c < rackStair2.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair2[r, c];
                        if ((sr.UserData.ContainsKey("JUDG_VALUE_1") && sr.UserData["JUDG_VALUE_1"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_2") && sr.UserData["JUDG_VALUE_2"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_3") && sr.UserData["JUDG_VALUE_3"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_4") && sr.UserData["JUDG_VALUE_4"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_5") && sr.UserData["JUDG_VALUE_5"].ToString().Equals("R")) ||
                            (sr.UserData.ContainsKey("JUDG_VALUE_6") && sr.UserData["JUDG_VALUE_6"].ToString().Equals("R"))
                        )
                        {
                            // 둘중 하나라도 불량이 존재하면 enable
                            sr.IsEnabled = true;
                            sr.Background = new SolidColorBrush(Colors.White);
                        }
                        else
                        {
                            sr.IsEnabled = false;
                        }
                    }
                }

            }
            else
            {
                lblEqpt.Visibility = Visibility.Visible;
                txtEqpt.Visibility = Visibility.Visible;
                lblModel.Visibility = Visibility.Visible;
                txtProd.Visibility = Visibility.Visible;
                lblAutoDeliveryStatus.Visibility = Visibility.Visible;
                txtAutoDeliveryStatus.Visibility = Visibility.Visible;
                // 전체 enable
                for (int r = 0; r < rackStair.RowCount; r++)
                {
                    for (int c = 0; c < rackStair.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair[r, c];
                        sr.IsEnabled = true;
                    }
                }

                for (int r = 0; r < rackStair2.RowCount; r++)
                {
                    for (int c = 0; c < rackStair2.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair2[r, c];
                        sr.IsEnabled = true;
                    }
                }


                ReloadSkidRack();

                //if (rdoAnode.IsChecked.Value)
                //{
                //    chkArr.IsChecked = false;
                //    this.ArrPos();
                //}
                //else
                //{
                //    chkArr.IsChecked = true;
                //    this.ArrPosX();
                //}
            }
        }

        /// <summary>
        /// Port combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCboPortSelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {
            DataRowView port = (DataRowView)cboPort.SelectedItem;

            if (port == null)
            {
                txtAutoDeliveryStatus.Text = "";
                txtEqpt.Text = "";
                txtProd.Text = "";
                return;
            }
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PORT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PORT_ID"] = port.Row["CBO_CODE"].ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PROD_FOR_ISS_REV", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtAutoDeliveryStatus.Text = dtResult.Rows[0]["AUTO_ISS_REQ"].ToString();
                    txtEqpt.Text = dtResult.Rows[0]["EQPTNAME"].ToString();
                    txtProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                }
                else
                {
                    txtAutoDeliveryStatus.Text = "";
                    txtEqpt.Text = "";
                    txtProd.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 수동 출고 버튼 EventArgs e
        /// </summary> RoutedEventArgs
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnManualIssue(object sender, RoutedEventArgs e)
        {

            ManualIssue();
        }

        private void dgStoreLoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputRankLoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgQALoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("ELTR_TYPE_CODE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        private void dgRackInfoLoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        /// <summary>
        /// SkidRack Check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkidRackChecked(object sender, Controls.SkidRackEventArgs e)
        {
            try
            {
                SkidRack rack = e.SkidRack;

                if (rack != null && rack.IsChecked)
                {

                    int maxSeq;
                    if (dtCheck.Rows.Count == 0)
                    {
                        maxSeq = 1;
                    }
                    else
                    {
                        maxSeq = Convert.ToInt32(dtCheck.Compute("max([SEQ])", string.Empty)) + 1;
                    }

                    DataRow dr;



                    if (rack.UserData.ContainsKey("WOID_1"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_1") ? rack.UserData["RACK_ID_1"] : "");
                        dr["LOTID"] = rack.PancakeID1;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_1") ? rack.UserData["VLD_DATE_1"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_1") ? rack.UserData["WOID_1"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_1") ? rack.UserData["PRODID_1"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_1") ? rack.UserData["PRODDESC_1"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_1") ? rack.UserData["PRODNAME_1"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_1") ? rack.UserData["MODLID_1"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_1") ? rack.UserData["WH_RCV_DTTM_1"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_1") ? rack.UserData["WIP_QTY_1"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA1;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD1;
                        dtCheck.Rows.Add(dr);
                    }

                    if (rack.UserData.ContainsKey("WOID_2"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_2") ? rack.UserData["RACK_ID_2"] : "");
                        dr["LOTID"] = rack.PancakeID2;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_2") ? rack.UserData["VLD_DATE_2"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_2") ? rack.UserData["WOID_2"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_2") ? rack.UserData["PRODID_2"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_2") ? rack.UserData["PRODDESC_2"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_2") ? rack.UserData["PRODNAME_2"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_2") ? rack.UserData["MODLID_2"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_2") ? rack.UserData["WH_RCV_DTTM_2"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_2") ? rack.UserData["WIP_QTY_2"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA2;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD2;
                        dtCheck.Rows.Add(dr);
                    }
                    if (rack.UserData.ContainsKey("WOID_3"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_3") ? rack.UserData["RACK_ID_3"] : "");
                        dr["LOTID"] = rack.PancakeID3;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_3") ? rack.UserData["VLD_DATE_3"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_3") ? rack.UserData["WOID_3"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_3") ? rack.UserData["PRODID_3"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_3") ? rack.UserData["PRODDESC_3"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_3") ? rack.UserData["PRODNAME_3"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_3") ? rack.UserData["MODLID_3"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_3") ? rack.UserData["WH_RCV_DTTM_3"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_3") ? rack.UserData["WIP_QTY_3"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA3;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD3;
                        dtCheck.Rows.Add(dr);
                    }
                    if (rack.UserData.ContainsKey("WOID_4"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_4") ? rack.UserData["RACK_ID_4"] : "");
                        dr["LOTID"] = rack.PancakeID4;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_4") ? rack.UserData["VLD_DATE_4"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_4") ? rack.UserData["WOID_4"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_4") ? rack.UserData["PRODID_4"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_4") ? rack.UserData["PRODDESC_4"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_4") ? rack.UserData["PRODNAME_4"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_4") ? rack.UserData["MODLID_4"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_4") ? rack.UserData["WH_RCV_DTTM_4"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_4") ? rack.UserData["WIP_QTY_4"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA4;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD4;
                        dtCheck.Rows.Add(dr);
                    }
                    if (rack.UserData.ContainsKey("WOID_5"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_5") ? rack.UserData["RACK_ID_5"] : "");
                        dr["LOTID"] = rack.PancakeID5;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_5") ? rack.UserData["VLD_DATE_5"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_5") ? rack.UserData["WOID_5"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_5") ? rack.UserData["PRODID_5"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_5") ? rack.UserData["PRODDESC_5"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_5") ? rack.UserData["PRODNAME_5"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_5") ? rack.UserData["MODLID_5"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_5") ? rack.UserData["WH_RCV_DTTM_5"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_5") ? rack.UserData["WIP_QTY_5"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA5;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD5;
                        dtCheck.Rows.Add(dr);
                    }
                    if (rack.UserData.ContainsKey("WOID_6"))
                    {
                        dr = dtCheck.NewRow();
                        dr["CHK"] = true;
                        dr["SEQ"] = maxSeq;
                        dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_6") ? rack.UserData["RACK_ID_6"] : "");
                        dr["LOTID"] = rack.PancakeID6;
                        dr["PRJT_NAME"] = rack.ProjectName;
                        dr["SPCL_FLAG"] = rack.Spcl_Flag;
                        dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                        dr["WIP_REMARKS"] = rack.Wip_Remarks;
                        dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                        dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_6") ? rack.UserData["VLD_DATE_6"] : "");
                        dr["WOID"] = (rack.UserData.ContainsKey("WOID_6") ? rack.UserData["WOID_6"] : "");
                        dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_6") ? rack.UserData["PRODID_6"] : "");
                        dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_6") ? rack.UserData["PRODDESC_6"] : "");
                        dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_6") ? rack.UserData["PRODNAME_6"] : "");

                        dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_6") ? rack.UserData["MODLID_6"] : "");
                        dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_6") ? rack.UserData["WH_RCV_DTTM_6"] : DBNull.Value);
                        dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_6") ? rack.UserData["WIP_QTY_6"] : DBNull.Value);
                        dr["JUDG_VALUE"] = rack.PancakeQA6;
                        dr["MCS_CST_ID"] = rack.SkidID;
                        dr["ELAPSE"] = rack.ElapseDay;
                        dr["WIPHOLD"] = rack.WIPHOLD6;
                        dtCheck.Rows.Add(dr);
                    }
                    txtZone.Text = rack.ZoneId.ToString();
                    txtPancakeRow.Text = rack.Row.ToString();
                    txtPancakeColumn.Text = rack.Col.ToString();
                    txtPancakeStair.Text = rack.Stair.ToString();

                    Util.GridSetData(this.dgRackInfo, dtCheck, null, false);


                    // find and check  dgRackInfo
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgInputRank.Rows)
                    {
                        DataRowView drv = (row.DataItem as DataRowView);


                        if (drv != null)
                        {
                            if (drv.Row["MCS_CST_ID"].ToString() == rack.SkidID.ToString())
                            {

                            }
                        }
                    }

                }
                else
                {
                    txtZone.Text = "";
                    txtPancakeRow.Text = "";
                    txtPancakeColumn.Text = "";
                    txtPancakeStair.Text = "";
                    DataRow[] selectedRow = null;
                    selectedRow = dtCheck.Select("RACK_ID='" + rack.RackId + "'");

                    int seqno = 0;

                    foreach (DataRow row in selectedRow)
                    {
                        seqno = Convert.ToInt16(row["SEQ"]);
                        dtCheck.Rows.Remove(row);
                    }
                    //seq 다시 처리
                    foreach (DataRow row in dtCheck.Rows)
                    {
                        if (Convert.ToInt16(row["SEQ"]) > seqno)
                        {
                            row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                        }
                    }

                    Util.GridSetData(this.dgRackInfo, dtCheck, null, false);

                    // find and uncheck
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// SkidRack Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkidRackClick(object sender, Controls.SkidRackEventArgs e)
        {

        }

        /// <summary>
        /// SkidRack DoubleClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkidRackDoubleClick(object sender, Controls.SkidRackEventArgs e)
        {
            //
            try
            {

                string sZONE = String.Empty;

                if ((bool)rdoAnode.IsChecked)
                {
                    sZONE = "A";
                }
                else
                {
                    sZONE = "B";
                }

                if (e.SkidRack.SkidID == "PORT")
                {
                    //Popup창 생성
                    MCS001_001_PORT_INFO wndPopup = new MCS001_001_PORT_INFO();
                    wndPopup.FrameOperation = FrameOperation;
                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[] { e.SkidRack.RackId, WH_ID, e.SkidRack };
                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += OnPancakeInfoPopupClosed;

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                else
                {
                    //Popup창 생성
                    MCS001_001_PANCAKE_INFO wndPopup = new MCS001_001_PANCAKE_INFO();
                    wndPopup.FrameOperation = FrameOperation;
                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[] { e.SkidRack, 'A' };
                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += OnPancakeInfoPopupClosed;

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                        //this.InitCombo();
                        //Refresh();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void OnPancakeInfoPopupClosed(object sender, EventArgs e)
        {
            MCS001_001_PANCAKE_INFO popup = sender as MCS001_001_PANCAKE_INFO;
            if (popup != null)
            {
                // 새로고침여부 판단.
                if (popup.IsUpdated)
                {
                    // 정보수정되었으므로, 새로고침
                    //this.InitCombo();
                    // OPTIMIZE 필요함 전체 REFRESH 말고 하나만 .......
                    // DA_MCS_SEL_SKID_BUFFER_BY_RACK_ID
                    // ((LGC.GMES.MES.MCS001.MCS001_001_PANCAKE_INFO)sender).sRACK_ID

                    string sRack_Id = ((LGC.GMES.MES.MCS001.MCS001_001_PANCAKE_INFO)sender).sRACK_ID;


                    DataTable RQDT = new DataTable("RQSTDT");
                    RQDT.Columns.Add("LANGID", typeof(string));
                    RQDT.Columns.Add("WH_ID", typeof(string));
                    RQDT.Columns.Add("RACK_ID", typeof(string));

                    DataRow drColor = RQDT.NewRow();
                    drColor["LANGID"] = LoginInfo.LANGID;
                    drColor["WH_ID"] = WH_ID;
                    drColor["RACK_ID"] = sRack_Id;

                    RQDT.Rows.Add(drColor);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_BUFFER_BY_RACK_ID", "RQSTDT", "RSLTDT", RQDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        //DataRow row = dtResult.Rows[0];
                        try
                        {
                            this.dtCheck.Clear();
                        }
                        catch
                        {
                        }

                        Util.gridClear(this.dgRackInfo);


                        for (int r = 0; r < rackStair.RowCount; r++)
                        {
                            for (int c = 0; c < rackStair.ColumnCount; c++)
                            {
                                SkidRack sr = rackStair[r, c];


                                if ((sr.RackId).ToUpper() == sRack_Id.ToUpper())
                                {
                                    sr.IsEnabled = true;

                                    sr.UserData.Clear();

                                    sr.PancakeQA1 = "";
                                    sr.PancakeQA2 = "";
                                    sr.PancakeQA3 = "";
                                    sr.PancakeQA4 = "";
                                    sr.PancakeQA5 = "";
                                    sr.PancakeQA6 = "";

                                    foreach (DataRow row in dtResult.Rows)
                                    {
                                        sr.ZoneId = row["ZONE_ID"].ToString();
                                        sr.RackId = row["RACK_ID"].ToString();
                                        sr.Row = int.Parse(row["X_PSTN"].ToString()); // row index 아님
                                        sr.Col = int.Parse(row["Y_PSTN"].ToString()); // col index 아님
                                        sr.Stair = int.Parse(row["Z_PSTN"].ToString());
                                        sr.PRDT_CLSS_CODE = row["PRDT_CLSS_CODE"].ToString();

                                        sr.ElapseDay = (row["ELAPSE"] == DBNull.Value ? 0 : (int)row["ELAPSE"]); //순서바뀜

                                        sr.ProjectName = row["PRJT_NAME"].ToString();

                                        sr.SkidID = row["MCS_CST_ID"].ToString();

                                        sr.Spcl_Flag = row["SPCL_FLAG"].ToString();
                                        sr.Spcl_RsnCode = row["SPCL_RSNCODE"].ToString();
                                        sr.Wip_Remarks = row["WIP_REMARKS"].ToString();
                                        sr.Wipdttm_ED = row["WIPDTTM_ED"].ToString();




                                        string qa = "";
                                        switch (row["JUDG_VALUE"].ToString())
                                        {
                                            case "TERM":
                                                qa = ObjectDic.Instance.GetObjectName("TERM");
                                                break;

                                            case "Y":
                                                qa = ObjectDic.Instance.GetObjectName("합격");
                                                break;
                                            case "F":
                                                qa = ObjectDic.Instance.GetObjectName("불합격");
                                                break;
                                            case "W":
                                                qa = ObjectDic.Instance.GetObjectName("대기");
                                                break;
                                            case "I":
                                                qa = ObjectDic.Instance.GetObjectName("검사중");
                                                break;
                                            case "R":
                                                qa = ObjectDic.Instance.GetObjectName("재작업");
                                                break;
                                            case "PR":
                                                qa = "In V/D";
                                                break;
                                            default:
                                                qa = "";
                                                break;
                                        }

                                        if (row["ID_SEQ"].ToString().Equals("1"))
                                        {
                                            // 1번 pancake
                                            sr.PancakeID1 = row["LOTID"].ToString();
                                            sr.PancakeQA1 = qa;
                                            sr.WIPHOLD1 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_1", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_1", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_1", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_1", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_1", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_1", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_1", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_1", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_1", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_1", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("2"))
                                        {
                                            // 2번 pancake
                                            sr.PancakeID2 = row["LOTID"].ToString();
                                            sr.PancakeQA2 = qa;
                                            sr.WIPHOLD2 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_2", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_2", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_2", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_2", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_2", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_2", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_2", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_2", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_2", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_2", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("3"))
                                        {
                                            // 3번 pancake
                                            sr.PancakeID3 = row["LOTID"].ToString();
                                            sr.PancakeQA3 = qa;
                                            sr.WIPHOLD3 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_3", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_3", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_3", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_3", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_3", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_3", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_3", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_3", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_3", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_3", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("4"))
                                        {
                                            // 4번 pancake
                                            sr.PancakeID4 = row["LOTID"].ToString();
                                            sr.PancakeQA4 = qa;
                                            sr.WIPHOLD4 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_4", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_4", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_4", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_4", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_4", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_4", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_4", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_4", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_4", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_4", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("5"))
                                        {
                                            // 5번 pancake
                                            sr.PancakeID5 = row["LOTID"].ToString();
                                            sr.PancakeQA5 = qa;
                                            sr.WIPHOLD5 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_5", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_5", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_5", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_5", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_5", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_5", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_5", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_5", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_5", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_5", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("6"))
                                        {
                                            // 6번 pancake
                                            sr.PancakeID6 = row["LOTID"].ToString();
                                            sr.PancakeQA6 = qa;
                                            sr.WIPHOLD6 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_6", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_6", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_6", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_6", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_6", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_6", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_6", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_6", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_6", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_6", row["VLD_DATE"]);
                                        }



                                    }

                                    ReloadStore();
                                    ReloadQA();
                                    ReloadInputRank();
                                    return;
                                }

                            }
                        }

                        for (int r = 0; r < rackStair2.RowCount; r++)
                        {
                            for (int c = 0; c < rackStair2.ColumnCount; c++)
                            {
                                SkidRack sr = rackStair2[r, c];

                                if ((sr.RackId).ToUpper() == sRack_Id.ToUpper())
                                {
                                    sr.UserData.Clear();
                                    sr.IsEnabled = true;
                                    sr.PancakeQA1 = "";
                                    sr.PancakeQA2 = "";


                                    foreach (DataRow row in dtResult.Rows)
                                    {
                                        sr.ZoneId = row["ZONE_ID"].ToString();
                                        sr.RackId = row["RACK_ID"].ToString();
                                        sr.Row = int.Parse(row["X_PSTN"].ToString()); // row index 아님
                                        sr.Col = int.Parse(row["Y_PSTN"].ToString()); // col index 아님
                                        sr.Stair = int.Parse(row["Z_PSTN"].ToString());
                                        sr.PRDT_CLSS_CODE = row["PRDT_CLSS_CODE"].ToString();

                                        sr.ElapseDay = (row["ELAPSE"] == DBNull.Value ? 0 : (int)row["ELAPSE"]); //순서바뀜

                                        sr.ProjectName = row["PRJT_NAME"].ToString();

                                        sr.SkidID = row["MCS_CST_ID"].ToString();

                                        sr.Spcl_Flag = row["SPCL_FLAG"].ToString();
                                        sr.Spcl_RsnCode = row["SPCL_RSNCODE"].ToString();
                                        sr.Wip_Remarks = row["WIP_REMARKS"].ToString();
                                        sr.Wipdttm_ED = row["WIPDTTM_ED"].ToString();




                                        string qa = "";
                                        switch (row["JUDG_VALUE"].ToString())
                                        {
                                            case "TERM":
                                                qa = ObjectDic.Instance.GetObjectName("TERM");
                                                break;

                                            case "Y":
                                                qa = ObjectDic.Instance.GetObjectName("합격");
                                                break;
                                            case "F":
                                                qa = ObjectDic.Instance.GetObjectName("불합격");
                                                break;
                                            case "W":
                                                qa = ObjectDic.Instance.GetObjectName("대기");
                                                break;
                                            case "I":
                                                qa = ObjectDic.Instance.GetObjectName("검사중");
                                                break;
                                            case "R":
                                                qa = ObjectDic.Instance.GetObjectName("재작업");
                                                break;
                                            case "PR":
                                                qa = "In V/D";
                                                break;
                                            default:
                                                qa = "";
                                                break;
                                        }
                                        if (row["ID_SEQ"].ToString().Equals("1"))
                                        {
                                            // 1번 pancake
                                            sr.PancakeID1 = row["LOTID"].ToString();
                                            sr.PancakeQA1 = qa;
                                            sr.WIPHOLD1 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_1", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_1", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_1", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_1", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_1", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_1", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_1", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_1", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_1", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_1", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("2"))
                                        {
                                            // 2번 pancake
                                            sr.PancakeID2 = row["LOTID"].ToString();
                                            sr.PancakeQA2 = qa;
                                            sr.WIPHOLD2 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_2", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_2", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_2", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_2", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_2", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_2", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_2", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_2", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_2", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_2", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("3"))
                                        {
                                            // 3번 pancake
                                            sr.PancakeID3 = row["LOTID"].ToString();
                                            sr.PancakeQA3 = qa;
                                            sr.WIPHOLD3 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_3", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_3", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_3", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_3", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_3", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_3", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_3", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_3", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_3", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_3", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("4"))
                                        {
                                            // 4번 pancake
                                            sr.PancakeID4 = row["LOTID"].ToString();
                                            sr.PancakeQA4 = qa;
                                            sr.WIPHOLD4 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_4", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_4", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_4", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_4", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_4", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_4", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_4", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_4", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_4", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_4", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("5"))
                                        {
                                            // 5번 pancake
                                            sr.PancakeID5 = row["LOTID"].ToString();
                                            sr.PancakeQA5 = qa;
                                            sr.WIPHOLD5 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_5", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_5", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_5", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_5", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_5", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_5", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_5", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_5", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_5", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_5", row["VLD_DATE"]);
                                        }
                                        else if (row["ID_SEQ"].ToString().Equals("6"))
                                        {
                                            // 6번 pancake
                                            sr.PancakeID6 = row["LOTID"].ToString();
                                            sr.PancakeQA6 = qa;
                                            sr.WIPHOLD6 = row["WIPHOLD"].ToString();

                                            sr.UserData.Add("RACK_ID_6", row["RACK_ID"]);
                                            sr.UserData.Add("JUDG_VALUE_6", row["JUDG_VALUE"]);
                                            sr.UserData.Add("WOID_6", row["WOID"]);
                                            sr.UserData.Add("WIP_QTY_6", row["WIP_QTY"]);
                                            sr.UserData.Add("PRODID_6", row["PRODID"]);
                                            sr.UserData.Add("PRODDESC_6", row["PRODDESC"]);
                                            sr.UserData.Add("PRODNAME_6", row["PRODNAME"]);
                                            sr.UserData.Add("MODLID_6", row["MODLID"]);
                                            sr.UserData.Add("WH_RCV_DTTM_6", row["WH_RCV_DTTM"]);
                                            sr.UserData.Add("VLD_DATE_6", row["VLD_DATE"]);
                                        }
                                    }

                                    ReloadStore();
                                    ReloadQA();
                                    ReloadInputRank();
                                    return;
                                }
                            }
                        }
                    }

                    //Refresh();
                }
            }
        }

        /// <summary>
        /// 화면확대
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HMIAera_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var matTrans = HMIAera.RenderTransform as MatrixTransform;
            //var pos1 = e.GetPosition(HMIAera);

            //var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

            //var mat = matTrans.Matrix;
            //mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
            //matTrans.Matrix = mat;
            //e.Handled = true;

        }

        private void btnExpandFrame2_Checked(object sender, RoutedEventArgs e)
        {
            RightArea.RowDefinitions[3].Height = new GridLength(280);
        }

        private void btnExpandFrame2_Unchecked(object sender, RoutedEventArgs e)
        {
            RightArea.RowDefinitions[3].Height = new GridLength(30);
        }

        private void dgQA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgQA.SelectedItem == null) return;

            //((System.Data.DataRowView)dgQA.SelectedItem).Row["CODE"]

            //

            DataRowView prjt_name = (DataRowView)cboProdId2.SelectedItem;
            string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();


            string sCode = ((System.Data.DataRowView)dgQA.SelectedItem).Row["CODE"].ToString();

            if (sCode == "V")
            {
                try
                {
                    //Popup창 생성
                    MCS001_001_VD_DETAIL wndPopup = new MCS001_001_VD_DETAIL();
                    wndPopup.FrameOperation = FrameOperation;
                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[] { ((System.Data.DataRowView)dgQA.SelectedItem).Row["CODE"].ToString(), ((System.Data.DataRowView)dgQA.SelectedItem).Row["ELTR_TYPE_CODE"].ToString(), PRJT_NAME };
                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += ConfigPopUpClosed;
                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }
            }
            else
            {
                try
                {
                    //Popup창 생성
                    MCS001_001_QADETAIL wndPopup = new MCS001_001_QADETAIL();
                    wndPopup.FrameOperation = FrameOperation;
                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[] { ((System.Data.DataRowView)dgQA.SelectedItem).Row["CODE"].ToString(), ((System.Data.DataRowView)dgQA.SelectedItem).Row["ELTR_TYPE_CODE"].ToString(), PRJT_NAME };
                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += ConfigPopUpClosed;
                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }
            }
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                //e.Handled = false;
                e.Handled = true;
                SearchSKIDLOTID();
            }
        }

        private void chkArr_Click(object sender, RoutedEventArgs e)
        {
            //if (chkArr.IsChecked.Value)
            //{
            //    this.ArrPosX();
            //}
            //else
            //{
            //    this.ArrPos();
            //}
        }

        private void OnBtnConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_CONFIG wndPopup = new MCS001_001_CONFIG();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += ConfigPopUpClosed;
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {

            }
        }

        private void ConfigPopUpClosed(object sender, EventArgs e)
        {
            MCS001_001_CONFIG popup = sender as MCS001_001_CONFIG;
            if (popup != null)
            {
                // 새로고침여부 판단.
                if (popup.IsUpdated)
                {
                    // 정보수정되었으므로, 새로고침
                    //rackStair
                    //this.InitCombo();
                    this.ReInitCombo();
                    Refresh();
                }
            }
        }

        private void WndPopup_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnBtnEmptySKID(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_EmptySKID wndPopup = new MCS001_001_EmptySKID();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            // 확장
            ContentsRow.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[2].Width = new GridLength(10.0, GridUnitType.Star);

            this.btnExpandFrame2.IsChecked = false;
            RightArea.RowDefinitions[3].Height = new GridLength(30);
        }

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            try
            {
                if (monitorTimer != null)
                {
                    monitorTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    {
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());
                        bSetAutoSelTime = true;
                    }
                    else
                    {
                        bSetAutoSelTime = false;
                    }


                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        Util.MessageValidation("SFU1606");
                        return;
                    }
                    monitorTimer.Interval = new TimeSpan(0, 0, iSec);
                    monitorTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //  SKID BUFFER 모니터링 자동조회  %1초로 변경 되었습니다.                                     
                        Util.MessageInfo("SFU4537", cboAutoSearchOut.SelectedValue.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void OnBtnSkidErrorList(object sender, RoutedEventArgs e)
        {
            // SKIDERRORLIST 조회
            try
            {
                //Popup창 생성
                MCS001_001_ERRORSKID wndPopup = new MCS001_001_ERRORSKID();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 200;

                da.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); // 500ms == 0.5s
                                                                             //da.AutoReverse = true;
                                                                             // da.RepeatBehavior = RepeatBehavior.Forever;
                                                                             //Imagelayout.BeginAnimation(Canvas.ZIndexProperty, da);
                                                                             //this.BeginAnimation(SkidRack.WidthProperty, da);
            }
        }

        private void layMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                //Imagelayout.Visibility = Visibility.Visible;

                //DoubleAnimation da = new DoubleAnimation();
                //da.From = 0;
                //da.To = 200;

                //da.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); // 500ms == 0.5s
                //                                                             //da.AutoReverse = true;
                //                                                             // da.RepeatBehavior = RepeatBehavior.Forever;
                //Imagelayout.BeginAnimation(Canvas.LeftProperty, da);
                ////this.BeginAnimation(SkidRack.WidthProperty, da);
            }
        }

        private void RankingDataView()
        {
            try
            {
                // 선택 지우기

                foreach (C1.WPF.DataGrid.DataGridRow row in dgInputRank.Rows)
                {
                    DataRowView drv = (row.DataItem as DataRowView);


                    if (drv != null)
                    {
                        if (drv.Row[0].ToString() == "1")
                        {
                            string sMCS_CST_ID = drv.Row["MCS_CST_ID"].ToString();
                            SearchSKIDFromRankToUncheck(sMCS_CST_ID);
                        }
                    }
                }



                DataRowView eltrtypeview = (DataRowView)cboELTRTYPE.SelectedItem;
                string CMCODE = eltrtypeview.Row["CMCODE"].ToString();
                string CMCDNAME = eltrtypeview.Row["CMCDNAME"].ToString();

                DataRowView prjt_name_view = (DataRowView)cboProdId3.SelectedItem;
                string PRJT_CODE = prjt_name_view.Row["CBO_CODE"].ToString();
                string PRJT_NAME = prjt_name_view.Row["CBO_NAME"].ToString();

                if (PRJT_CODE == "0" && CMCODE == "0")
                {
                    dvRank.RowFilter = "";
                    Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
                }
                else if (PRJT_CODE == "0" && CMCODE != "0")
                {
                    dvRank.RowFilter = "ELTR_TYPE_CODE = '" + CMCDNAME + "'";

                    Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
                }
                else if (PRJT_CODE != "0" && CMCODE == "0")
                {
                    dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "'";

                    Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
                }

                else
                {
                    dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "' AND ELTR_TYPE_CODE = '" + CMCDNAME + "' ";

                    Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
                }
            }
            catch
            {
            }
        }

        private void OnCboProdId03SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            RankingDataView();

            //try
            //{
            //    // 선택 지우기

            //    foreach (C1.WPF.DataGrid.DataGridRow row in dgInputRank.Rows)
            //    {
            //        DataRowView drv = (row.DataItem as DataRowView);


            //        if (drv != null)
            //        {
            //            if (drv.Row[0].ToString() == "1")
            //            {
            //                string sMCS_CST_ID = drv.Row["MCS_CST_ID"].ToString();
            //                SearchSKIDFromRankToUncheck(sMCS_CST_ID);
            //            }
            //        }
            //    }



            //    DataRowView eltrtypeview = (DataRowView)cboELTRTYPE.SelectedItem;
            //    string CMCODE = eltrtypeview.Row["CMCODE"].ToString();
            //    string CMCDNAME = eltrtypeview.Row["CMCDNAME"].ToString();

            //    DataRowView prjt_name_view = (DataRowView)cboProdId3.SelectedItem;
            //    string PRJT_CODE = prjt_name_view.Row["CBO_CODE"].ToString();
            //    string PRJT_NAME = prjt_name_view.Row["CBO_NAME"].ToString();

            //    if (PRJT_CODE == "0" && CMCODE == "0")
            //    {
            //        dvRank.RowFilter = "";
            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //    else if (PRJT_CODE == "0" && CMCODE != "0")
            //    {
            //        dvRank.RowFilter = "ELTR_TYPE_CODE = '" + CMCDNAME + "'";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //    else if (PRJT_CODE != "0" && CMCODE == "0")
            //    {
            //        dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "'";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }

            //    else
            //    {
            //        dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "' AND ELTR_TYPE_CODE = '" + CMCDNAME + "' ";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //}
            //catch
            //{
            //}

        }

        private void OnCboELTRTYPESelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

            RankingDataView();
            //try
            //{
            //    // find and check  dgRackInfo
            //    foreach (C1.WPF.DataGrid.DataGridRow row in dgInputRank.Rows)
            //    {
            //        DataRowView drv = (row.DataItem as DataRowView);


            //        if (drv != null)
            //        {
            //            if (drv.Row[0].ToString() == "1")
            //            {
            //                string sMCS_CST_ID = drv.Row["MCS_CST_ID"].ToString();
            //                SearchSKIDFromRankToUncheck(sMCS_CST_ID);
            //            }
            //        }
            //    }


            //    DataRowView eltrtypeview = (DataRowView)cboELTRTYPE.SelectedItem;
            //    string CMCODE = eltrtypeview.Row["CMCODE"].ToString();
            //    string CMCDNAME = eltrtypeview.Row["CMCDNAME"].ToString();

            //    DataRowView prjt_name_view = (DataRowView)cboProdId3.SelectedItem;
            //    string PRJT_CODE = prjt_name_view.Row["CBO_CODE"].ToString();
            //    string PRJT_NAME = prjt_name_view.Row["CBO_NAME"].ToString();

            //    if (PRJT_CODE == "0" && CMCODE == "0")
            //    {
            //        dvRank.RowFilter = "";
            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //    else if (PRJT_CODE == "0" && CMCODE != "0")
            //    {
            //        dvRank.RowFilter = "ELTR_TYPE_CODE = '" + CMCDNAME + "'";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //    else if (PRJT_CODE != "0" && CMCODE == "0")
            //    {
            //        dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "'";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }

            //    else
            //    {
            //        dvRank.RowFilter = "PRJT_NAME = '" + PRJT_NAME + "' AND ELTR_TYPE_CODE = '" + CMCDNAME + "' ";

            //        Util.GridSetData(this.dgInputRank, dvRank.ToTable(), null, false);
            //    }
            //}
            //catch { }

        }

        private void onBtnLayoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_Layout wndPopup = new MCS001_001_Layout();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    //object[] Parameters = new object[] { WH_ID };
                    //C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void chPOSChecked(object sender, RoutedEventArgs e)
        {
            this.ArrPosX();
        }

        private void chPOSUncheck(object sender, RoutedEventArgs e)
        {
            this.ArrPos();
        }


        private void dgStore_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            for (int i = 0; i <= dgStore.GetRowCount(); i++)
            {
                if ((Util.NVC(DataTableConverter.GetValue(dgStore.Rows[i + 1].DataItem, "CODE"))) == "1")
                {
                    e.Merge(new DataGridCellsRange(dgStore.GetCell(i + 1, 1), dgStore.GetCell(i + 1, 4)));
                }
            }
        }

        private void OnCboProdId2SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

            this.ReloadQA();
            //DataRowView prjt_name = (DataRowView)cboProdId2.SelectedItem;
            //string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();
            //if (PRJT_NAME == "0")
            //{
            //    ReloadQA();
            //}
            //else
            //{
            //    try
            //    {
            //        DataTable inTable = new DataTable();
            //        inTable.Columns.Add("WH_ID", typeof(string));
            //        inTable.Columns.Add("PRJT_NAME", typeof(string));

            //        DataRow newRow = inTable.NewRow();
            //        newRow["WH_ID"] = WH_ID;
            //        newRow["PRJT_NAME"] = PRJT_NAME;

            //        inTable.Rows.Add(newRow);

            //        new ClientProxy().ExecuteService("DA_MCS_SEL_QA_BY_PRJT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
            //        {
            //            try
            //            {
            //                if (searchException != null)
            //                {
            //                    Util.MessageException(searchException);
            //                    return;
            //                }
            //                DataTable GridData = new DataTable();

            //                GridData.Columns.Add("ELTR_TYPE_CODE", typeof(string)); // 
            //                GridData.Columns.Add("CC", typeof(int)); // 양극
            //                GridData.Columns.Add("AC", typeof(int)); // 음극
            //                GridData.Columns.Add("CODE", typeof(string));

            //                GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("합격"), 0, 0, 'Y' });
            //                GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("검사중"), 0, 0, 'I' });
            //                GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("불합격"), 0, 0, 'F' });
            //                GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("재작업"), 0, 0, 'R' });
            //                GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("검사대기"), 0, 0, 'W' });

            //                foreach (DataRow dr in searchResult.Rows)
            //                {
            //                    int idx = 0;
            //                    if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
            //                    {
            //                        // 양극
            //                        idx = 1;
            //                    }
            //                    else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
            //                    {
            //                        // 음극
            //                        idx = 2;
            //                    }
            //                    if (idx > 0)
            //                    {
            //                        GridData.Rows[0][idx] = dr["QA_Y"];
            //                        GridData.Rows[1][idx] = dr["QA_I"];
            //                        GridData.Rows[2][idx] = dr["QA_F"];
            //                        GridData.Rows[3][idx] = dr["QA_R"];
            //                        GridData.Rows[4][idx] = dr["QA_W"];
            //                    }
            //                }
            //                Util.GridSetData(this.dgQA, GridData, null, false);
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.MessageException(ex);
            //            }
            //        }
            //        );
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //    }
            //}
        }

        private void OnCboProdId1SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            ReloadStore();


            //DataRowView prjt_name = (DataRowView)cboProdId1.SelectedItem;
            //string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();

            //if (PRJT_NAME == "0")
            //{
            //    ReloadStore();
            //}
            //else
            //{
            //    DataTable inTable = new DataTable();
            //    inTable.Columns.Add("LANGID", typeof(string));
            //    inTable.Columns.Add("WH_ID", typeof(string));
            //    inTable.Columns.Add("PRJT_NAME", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["LANGID"] = LoginInfo.LANGID;
            //    newRow["WH_ID"] = WH_ID;
            //    newRow["PRJT_NAME"] = PRJT_NAME;

            //    inTable.Rows.Add(newRow);

            //    try
            //    {
            //        new ClientProxy().ExecuteService("DA_MCS_SEL_WH_QTY_BY_PRJT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
            //        {
            //            try
            //            {
            //                if (searchException != null)
            //                {
            //                    Util.MessageException(searchException);
            //                    return;
            //                }


            //                if (searchResult != null && searchResult.Rows.Count > 0)
            //                {
            //                    foreach (DataRow row in searchResult.Rows)
            //                    {
            //                        if (row["PRJT_NAME"].ToString() == "적재가능수량(총수량)")
            //                            row["PRJT_NAME"] = ObjectDic.Instance.GetObjectName("적재가능수량(총수량)");
            //                    }
            //                }
            //                Util.GridSetData(this.dgStore, searchResult, null, false);
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.MessageException(ex);
            //            }
            //        }
            //       );
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //    }
            //}

        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;

                if (cb.IsChecked != null)
                {
                    int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;
                    DataRow dtRow = (cb.DataContext as DataRowView).Row;

                    string sMCS_CST_ID = dtRow["MCS_CST_ID"].ToString();

                    SearchSKIDFromRank(sMCS_CST_ID);
                }
            }
            catch
            {
            }
        }

        private void Chk_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;

                if (cb.IsChecked != null)
                {
                    int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;
                    DataRow dtRow = (cb.DataContext as DataRowView).Row;

                    string sMCS_CST_ID = dtRow["MCS_CST_ID"].ToString();

                    SearchSKIDFromRankToUncheck(sMCS_CST_ID);
                }
            }
            catch
            {
            }
        }




        private void btnInSKID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Popup창 생성
                MCS001_001_InSKID wndPopup = new MCS001_001_InSKID();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void OnBtnISSRSV(object sender, RoutedEventArgs e)
        {
            // 출고예약SKID조회
            try
            {
                //Popup창 생성
                MCS001_001_ISSRSV wndPopup = new MCS001_001_ISSRSV();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { WH_ID };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region Method

        private void CheckSkidRack(SkidRack sr)
        {
            int maxSeq;
            if (dtCheck.Rows.Count == 0)
            {
                maxSeq = 1;
            }
            else
            {
                maxSeq = Convert.ToInt32(dtCheck.Compute("max([SEQ])", string.Empty)) + 1;
            }

            DataRow dr;
            if (sr.UserData.ContainsKey("WOID_1"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_1") ? sr.UserData["RACK_ID_1"] : "");
                dr["LOTID"] = sr.PancakeID1;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_1") ? sr.UserData["WOID_1"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_1") ? sr.UserData["PRODID_1"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_1") ? sr.UserData["PRODDESC_1"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_1") ? sr.UserData["PRODNAME_1"] : "");

                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_1") ? sr.UserData["MODLID_1"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_1") ? sr.UserData["WH_RCV_DTTM_1"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_1") ? sr.UserData["WIP_QTY_1"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA1;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD1;
                dtCheck.Rows.Add(dr);
            }

            if (sr.UserData.ContainsKey("WOID_2"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_2") ? sr.UserData["RACK_ID_2"] : "");
                dr["LOTID"] = sr.PancakeID2;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_2") ? sr.UserData["WOID_2"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_2") ? sr.UserData["PRODID_2"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_2") ? sr.UserData["PRODDESC_2"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_2") ? sr.UserData["PRODNAME_2"] : "");
                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_2") ? sr.UserData["MODLID_2"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_2") ? sr.UserData["WH_RCV_DTTM_2"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_2") ? sr.UserData["WIP_QTY_2"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA2;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD2;
                dtCheck.Rows.Add(dr);
            }
            if (sr.UserData.ContainsKey("WOID_3"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_3") ? sr.UserData["RACK_ID_3"] : "");
                dr["LOTID"] = sr.PancakeID3;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_3") ? sr.UserData["WOID_3"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_3") ? sr.UserData["PRODID_3"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_3") ? sr.UserData["PRODDESC_3"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_3") ? sr.UserData["PRODNAME_3"] : "");
                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_3") ? sr.UserData["MODLID_3"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_3") ? sr.UserData["WH_RCV_DTTM_3"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_3") ? sr.UserData["WIP_QTY_3"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA3;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD3;
                dtCheck.Rows.Add(dr);
            }
            if (sr.UserData.ContainsKey("WOID_4"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_4") ? sr.UserData["RACK_ID_4"] : "");
                dr["LOTID"] = sr.PancakeID4;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_4") ? sr.UserData["WOID_4"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_4") ? sr.UserData["PRODID_4"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_4") ? sr.UserData["PRODDESC_4"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_4") ? sr.UserData["PRODNAME_4"] : "");
                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_4") ? sr.UserData["MODLID_4"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_4") ? sr.UserData["WH_RCV_DTTM_4"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_4") ? sr.UserData["WIP_QTY_4"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA4;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD4;
                dtCheck.Rows.Add(dr);
            }
            if (sr.UserData.ContainsKey("WOID_5"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_5") ? sr.UserData["RACK_ID_5"] : "");
                dr["LOTID"] = sr.PancakeID5;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_5") ? sr.UserData["WOID_5"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_5") ? sr.UserData["PRODID_5"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_5") ? sr.UserData["PRODDESC_5"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_5") ? sr.UserData["PRODNAME_5"] : "");
                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_5") ? sr.UserData["MODLID_5"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_5") ? sr.UserData["WH_RCV_DTTM_5"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_5") ? sr.UserData["WIP_QTY_5"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA5;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD5;
                dtCheck.Rows.Add(dr);
            }
            if (sr.UserData.ContainsKey("WOID_6"))
            {
                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = (sr.UserData.ContainsKey("RACK_ID_6") ? sr.UserData["RACK_ID_6"] : "");
                dr["LOTID"] = sr.PancakeID6;
                dr["PRJT_NAME"] = sr.ProjectName;
                dr["SPCL_FLAG"] = sr.Spcl_Flag;
                dr["SPCL_RSNCODE"] = sr.Spcl_RsnCode;
                dr["WIP_REMARKS"] = sr.Wip_Remarks;
                dr["WIPDTTM_ED"] = sr.Wipdttm_ED;
                dr["WOID"] = (sr.UserData.ContainsKey("WOID_6") ? sr.UserData["WOID_6"] : "");
                dr["PRODID"] = (sr.UserData.ContainsKey("PRODID_6") ? sr.UserData["PRODID_6"] : "");
                dr["PRODDESC"] = (sr.UserData.ContainsKey("PRODDESC_6") ? sr.UserData["PRODDESC_6"] : "");
                dr["PRODNAME"] = (sr.UserData.ContainsKey("PRODNAME_6") ? sr.UserData["PRODNAME_6"] : "");
                dr["MODLID"] = (sr.UserData.ContainsKey("MODLID_6") ? sr.UserData["MODLID_6"] : "");
                dr["WH_RCV_DTTM"] = (sr.UserData.ContainsKey("WH_RCV_DTTM_6") ? sr.UserData["WH_RCV_DTTM_6"] : DBNull.Value);
                dr["WIP_QTY"] = (sr.UserData.ContainsKey("WIP_QTY_6") ? sr.UserData["WIP_QTY_6"] : DBNull.Value);
                dr["JUDG_VALUE"] = sr.PancakeQA6;
                dr["MCS_CST_ID"] = sr.SkidID;
                dr["ELAPSE"] = sr.ElapseDay;
                dr["WIPHOLD"] = sr.WIPHOLD6;
                dtCheck.Rows.Add(dr);
            }
            txtZone.Text = sr.ZoneId.ToString();
            txtPancakeRow.Text = sr.Row.ToString();
            txtPancakeColumn.Text = sr.Col.ToString();
            txtPancakeStair.Text = sr.Stair.ToString();
            Util.GridSetData(this.dgRackInfo, dtCheck, null, false);
        }

        private void UnCheckSkidRack(SkidRack sr)
        {
            txtZone.Text = "";
            txtPancakeRow.Text = "";
            txtPancakeColumn.Text = "";
            txtPancakeStair.Text = "";
            DataRow[] selectedRow = null;
            selectedRow = dtCheck.Select("RACK_ID='" + sr.RackId + "'");

            int seqno = 0;

            foreach (DataRow row in selectedRow)
            {
                seqno = Convert.ToInt16(row["SEQ"]);
                dtCheck.Rows.Remove(row);
            }
            //seq 다시 처리
            foreach (DataRow row in dtCheck.Rows)
            {
                if (Convert.ToInt16(row["SEQ"]) > seqno)
                {
                    row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                }
            }

            Util.GridSetData(this.dgRackInfo, dtCheck, null, false);
        }

        private void UnCheckSkidRackBySkid(string sRackId)
        {
            txtZone.Text = "";
            txtPancakeRow.Text = "";
            txtPancakeColumn.Text = "";
            txtPancakeStair.Text = "";
            DataRow[] selectedRow = null;
            selectedRow = dtCheck.Select("RACK_ID='" + sRackId + "'");

            int seqno = 0;

            foreach (DataRow row in selectedRow)
            {
                seqno = Convert.ToInt16(row["SEQ"]);
                dtCheck.Rows.Remove(row);
            }
            //seq 다시 처리
            foreach (DataRow row in dtCheck.Rows)
            {
                if (Convert.ToInt16(row["SEQ"]) > seqno)
                {
                    row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                }
            }

            Util.GridSetData(this.dgRackInfo, dtCheck, null, false);
        }

        private void UnCheckSkidRackBySkidId(string sSkidId)
        {
            txtZone.Text = "";
            txtPancakeRow.Text = "";
            txtPancakeColumn.Text = "";
            txtPancakeStair.Text = "";
            DataRow[] selectedRow = null;
            selectedRow = dtCheck.Select("MCS_CST_ID='" + sSkidId + "'");

            int seqno = 0;

            foreach (DataRow row in selectedRow)
            {
                seqno = Convert.ToInt16(row["SEQ"]);
                dtCheck.Rows.Remove(row);
            }
            //seq 다시 처리
            foreach (DataRow row in dtCheck.Rows)
            {
                if (Convert.ToInt16(row["SEQ"]) > seqno)
                {
                    row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                }
            }

            Util.GridSetData(this.dgRackInfo, dtCheck, null, false);
        }


        /// <summary>
        /// 화면 역방향 디스플레이
        /// </summary>
        private void ArrPosX()
        {
            try
            {
                for (int i = 1; i <= ColumnCount; i++)
                {

                    //string sN = "n1" + (23 - i).ToString("D" + 2);
                    //TextBlock tb = (TextBlock)this.FindName(sN);
                    //Grid.SetColumn(tb, i);

                    string sN2 = "n2" + (23 - i).ToString("D" + 2);
                    TextBlock tb2 = (TextBlock)this.FindName(sN2);
                    //tb2.Foreground = new SolidColorBrush(Colors.Blue);
                    Grid.SetColumn(tb2, i);

                    //Grid.SetColumn(rackStair[0, i - 1], 25 - i);
                    //Grid.SetColumn(rackStair[1, i - 1], 25 - i);
                    //Grid.SetColumn(rackStair[2, i - 1], 25 - i);

                    Grid.SetColumn(rackStair2[0, i - 1], 25 - i);
                    Grid.SetColumn(rackStair2[1, i - 1], 25 - i);
                    Grid.SetColumn(rackStair2[2, i - 1], 25 - i);
                }
            }
            catch
            {
            }

            #region 폐기예정
            //Grid.SetColumn(n122, 1);
            //Grid.SetColumn(n121, 2);
            //Grid.SetColumn(n120, 3);
            //Grid.SetColumn(n119, 4);
            //Grid.SetColumn(n118, 5);
            //Grid.SetColumn(n117, 6);
            //Grid.SetColumn(n116, 7);
            //Grid.SetColumn(n115, 8);
            //Grid.SetColumn(n114, 9);
            //Grid.SetColumn(n113, 10);
            //Grid.SetColumn(n112, 11);
            //Grid.SetColumn(n111, 12);
            //Grid.SetColumn(n110, 13);
            //Grid.SetColumn(n109, 14);
            //Grid.SetColumn(n108, 15);
            //Grid.SetColumn(n107, 16);
            //Grid.SetColumn(n106, 17);
            //Grid.SetColumn(n105, 18);
            //Grid.SetColumn(n104, 19);
            //Grid.SetColumn(n103, 20);
            //Grid.SetColumn(n102, 21);
            //Grid.SetColumn(n101, 22);

            //Grid.SetColumn(n222, 1);
            //Grid.SetColumn(n221, 2);
            //Grid.SetColumn(n220, 3);
            //Grid.SetColumn(n219, 4);
            //Grid.SetColumn(n218, 5);
            //Grid.SetColumn(n217, 6);
            //Grid.SetColumn(n216, 7);
            //Grid.SetColumn(n215, 8);
            //Grid.SetColumn(n214, 9);
            //Grid.SetColumn(n213, 10);
            //Grid.SetColumn(n212, 11);
            //Grid.SetColumn(n211, 12);
            //Grid.SetColumn(n210, 13);
            //Grid.SetColumn(n209, 14);
            //Grid.SetColumn(n208, 15);
            //Grid.SetColumn(n207, 16);
            //Grid.SetColumn(n206, 17);
            //Grid.SetColumn(n205, 18);
            //Grid.SetColumn(n204, 19);
            //Grid.SetColumn(n203, 20);
            //Grid.SetColumn(n202, 21);
            //Grid.SetColumn(n201, 22);


            //// 1
            //Grid.SetColumn(rackStair[0, 0], 24);
            //Grid.SetColumn(rackStair[1, 0], 24);
            //Grid.SetColumn(rackStair[2, 0], 24);

            //Grid.SetColumn(rackStair[0, 1], 23);
            //Grid.SetColumn(rackStair[1, 1], 23);
            //Grid.SetColumn(rackStair[2, 1], 23);

            //Grid.SetColumn(rackStair[0, 2], 22);
            //Grid.SetColumn(rackStair[1, 2], 22);
            //Grid.SetColumn(rackStair[2, 2], 22);

            //Grid.SetColumn(rackStair[0, 3], 21);
            //Grid.SetColumn(rackStair[1, 3], 21);
            //Grid.SetColumn(rackStair[2, 3], 21);

            //Grid.SetColumn(rackStair[0, 4], 20);
            //Grid.SetColumn(rackStair[1, 4], 20);
            //Grid.SetColumn(rackStair[2, 4], 20);

            //Grid.SetColumn(rackStair[0, 5], 19);
            //Grid.SetColumn(rackStair[1, 5], 19);
            //Grid.SetColumn(rackStair[2, 5], 19);

            //Grid.SetColumn(rackStair[0, 6], 18);
            //Grid.SetColumn(rackStair[1, 6], 18);
            //Grid.SetColumn(rackStair[2, 6], 18);

            //Grid.SetColumn(rackStair[0, 7], 17);
            //Grid.SetColumn(rackStair[1, 7], 17);
            //Grid.SetColumn(rackStair[2, 7], 17);

            //Grid.SetColumn(rackStair[0, 8], 16);
            //Grid.SetColumn(rackStair[1, 8], 16);
            //Grid.SetColumn(rackStair[2, 8], 16);

            //Grid.SetColumn(rackStair[0, 9], 15);
            //Grid.SetColumn(rackStair[1, 9], 15);
            //Grid.SetColumn(rackStair[2, 9], 15);

            //Grid.SetColumn(rackStair[0, 10], 14);
            //Grid.SetColumn(rackStair[1, 10], 14);
            //Grid.SetColumn(rackStair[2, 10], 14);

            //Grid.SetColumn(rackStair[0, 11], 13);
            //Grid.SetColumn(rackStair[1, 11], 13);
            //Grid.SetColumn(rackStair[2, 11], 13);

            //Grid.SetColumn(rackStair[0, 12], 12);
            //Grid.SetColumn(rackStair[1, 12], 12);
            //Grid.SetColumn(rackStair[2, 12], 12);

            //Grid.SetColumn(rackStair[0, 13], 11);
            //Grid.SetColumn(rackStair[1, 13], 11);
            //Grid.SetColumn(rackStair[2, 13], 11);

            //Grid.SetColumn(rackStair[0, 14], 10);
            //Grid.SetColumn(rackStair[1, 14], 10);
            //Grid.SetColumn(rackStair[2, 14], 10);

            //Grid.SetColumn(rackStair[0, 15], 9);
            //Grid.SetColumn(rackStair[1, 15], 9);
            //Grid.SetColumn(rackStair[2, 15], 9);

            //Grid.SetColumn(rackStair[0, 16], 8);
            //Grid.SetColumn(rackStair[1, 16], 8);
            //Grid.SetColumn(rackStair[2, 16], 8);

            //Grid.SetColumn(rackStair[0, 17], 7);
            //Grid.SetColumn(rackStair[1, 17], 7);
            //Grid.SetColumn(rackStair[2, 17], 7);

            //Grid.SetColumn(rackStair[0, 18], 6);
            //Grid.SetColumn(rackStair[1, 18], 6);
            //Grid.SetColumn(rackStair[2, 18], 6);

            //Grid.SetColumn(rackStair[0, 19], 5);
            //Grid.SetColumn(rackStair[1, 19], 5);
            //Grid.SetColumn(rackStair[2, 19], 5);

            //Grid.SetColumn(rackStair[0, 20], 4);
            //Grid.SetColumn(rackStair[1, 20], 4);
            //Grid.SetColumn(rackStair[2, 20], 4);

            //Grid.SetColumn(rackStair[0, 21], 3);
            //Grid.SetColumn(rackStair[1, 21], 3);
            //Grid.SetColumn(rackStair[2, 21], 3);



            //// 2
            //Grid.SetColumn(rackStair2[0, 0], 24);
            //Grid.SetColumn(rackStair2[1, 0], 24);
            //Grid.SetColumn(rackStair2[2, 0], 24);

            //Grid.SetColumn(rackStair2[0, 1], 23);
            //Grid.SetColumn(rackStair2[1, 1], 23);
            //Grid.SetColumn(rackStair2[2, 1], 23);

            //Grid.SetColumn(rackStair2[0, 2], 22);
            //Grid.SetColumn(rackStair2[1, 2], 22);
            //Grid.SetColumn(rackStair2[2, 2], 22);

            //Grid.SetColumn(rackStair2[0, 3], 21);
            //Grid.SetColumn(rackStair2[1, 3], 21);
            //Grid.SetColumn(rackStair2[2, 3], 21);

            //Grid.SetColumn(rackStair2[0, 4], 20);
            //Grid.SetColumn(rackStair2[1, 4], 20);
            //Grid.SetColumn(rackStair2[2, 4], 20);

            //Grid.SetColumn(rackStair2[0, 5], 19);
            //Grid.SetColumn(rackStair2[1, 5], 19);
            //Grid.SetColumn(rackStair2[2, 5], 19);

            //Grid.SetColumn(rackStair2[0, 6], 18);
            //Grid.SetColumn(rackStair2[1, 6], 18);
            //Grid.SetColumn(rackStair2[2, 6], 18);

            //Grid.SetColumn(rackStair2[0, 7], 17);
            //Grid.SetColumn(rackStair2[1, 7], 17);
            //Grid.SetColumn(rackStair2[2, 7], 17);

            //Grid.SetColumn(rackStair2[0, 8], 16);
            //Grid.SetColumn(rackStair2[1, 8], 16);
            //Grid.SetColumn(rackStair2[2, 8], 16);

            //Grid.SetColumn(rackStair2[0, 9], 15);
            //Grid.SetColumn(rackStair2[1, 9], 15);
            //Grid.SetColumn(rackStair2[2, 9], 15);

            //Grid.SetColumn(rackStair2[0, 10], 14);
            //Grid.SetColumn(rackStair2[1, 10], 14);
            //Grid.SetColumn(rackStair2[2, 10], 14);

            //Grid.SetColumn(rackStair2[0, 11], 13);
            //Grid.SetColumn(rackStair2[1, 11], 13);
            //Grid.SetColumn(rackStair2[2, 11], 13);

            //Grid.SetColumn(rackStair2[0, 12], 12);
            //Grid.SetColumn(rackStair2[1, 12], 12);
            //Grid.SetColumn(rackStair2[2, 12], 12);

            //Grid.SetColumn(rackStair2[0, 13], 11);
            //Grid.SetColumn(rackStair2[1, 13], 11);
            //Grid.SetColumn(rackStair2[2, 13], 11);

            //Grid.SetColumn(rackStair2[0, 14], 10);
            //Grid.SetColumn(rackStair2[1, 14], 10);
            //Grid.SetColumn(rackStair2[2, 14], 10);

            //Grid.SetColumn(rackStair2[0, 15], 9);
            //Grid.SetColumn(rackStair2[1, 15], 9);
            //Grid.SetColumn(rackStair2[2, 15], 9);

            //Grid.SetColumn(rackStair2[0, 16], 8);
            //Grid.SetColumn(rackStair2[1, 16], 8);
            //Grid.SetColumn(rackStair2[2, 16], 8);

            //Grid.SetColumn(rackStair2[0, 17], 7);
            //Grid.SetColumn(rackStair2[1, 17], 7);
            //Grid.SetColumn(rackStair2[2, 17], 7);

            //Grid.SetColumn(rackStair2[0, 18], 6);
            //Grid.SetColumn(rackStair2[1, 18], 6);
            //Grid.SetColumn(rackStair2[2, 18], 6);

            //Grid.SetColumn(rackStair2[0, 19], 5);
            //Grid.SetColumn(rackStair2[1, 19], 5);
            //Grid.SetColumn(rackStair2[2, 19], 5);

            //Grid.SetColumn(rackStair2[0, 20], 4);
            //Grid.SetColumn(rackStair2[1, 20], 4);
            //Grid.SetColumn(rackStair2[2, 20], 4);

            //Grid.SetColumn(rackStair2[0, 21], 3);
            //Grid.SetColumn(rackStair2[1, 21], 3);
            //Grid.SetColumn(rackStair2[2, 21], 3);
            #endregion

        }

        /// <summary>
        /// 화면 순방향 디스플레이
        /// </summary>
        private void ArrPos()
        {
            try
            {
                for (int i = 1; i <= ColumnCount; i++)
                {

                    //string sN = "n1" + i.ToString("D" + 2);
                    //TextBlock tb = (TextBlock)this.FindName(sN);
                    //Grid.SetColumn(tb, i);

                    string sN2 = "n2" + i.ToString("D" + 2);
                    TextBlock tb2 = (TextBlock)this.FindName(sN2);

                    tb2.Foreground = new SolidColorBrush(Colors.Black);
                    Grid.SetColumn(tb2, i);

                    //Grid.SetColumn(rackStair[0, i - 1], i + 2);
                    //Grid.SetColumn(rackStair[1, i - 1], i + 2);
                    //Grid.SetColumn(rackStair[2, i - 1], i + 2);

                    Grid.SetColumn(rackStair2[0, i - 1], i + 2);
                    Grid.SetColumn(rackStair2[1, i - 1], i + 2);
                    Grid.SetColumn(rackStair2[2, i - 1], i + 2);
                }
            }
            catch
            {
            }

            #region 폐기
            //Grid.SetColumn(n101, 1);
            //Grid.SetColumn(n102, 2);
            //Grid.SetColumn(n103, 3);
            //Grid.SetColumn(n104, 4);
            //Grid.SetColumn(n105, 5);
            //Grid.SetColumn(n106, 6);
            //Grid.SetColumn(n107, 7);
            //Grid.SetColumn(n108, 8);
            //Grid.SetColumn(n109, 9);
            //Grid.SetColumn(n110, 10);
            //Grid.SetColumn(n111, 11);
            //Grid.SetColumn(n112, 12);
            //Grid.SetColumn(n113, 13);
            //Grid.SetColumn(n114, 14);
            //Grid.SetColumn(n115, 15);
            //Grid.SetColumn(n116, 16);
            //Grid.SetColumn(n117, 17);
            //Grid.SetColumn(n118, 18);
            //Grid.SetColumn(n119, 19);
            //Grid.SetColumn(n120, 20);
            //Grid.SetColumn(n121, 21);
            //Grid.SetColumn(n122, 22);

            //Grid.SetColumn(n201, 1);
            //Grid.SetColumn(n202, 2);
            //Grid.SetColumn(n203, 3);
            //Grid.SetColumn(n204, 4);
            //Grid.SetColumn(n205, 5);
            //Grid.SetColumn(n206, 6);
            //Grid.SetColumn(n207, 7);
            //Grid.SetColumn(n208, 8);
            //Grid.SetColumn(n209, 9);
            //Grid.SetColumn(n210, 10);
            //Grid.SetColumn(n211, 11);
            //Grid.SetColumn(n212, 12);
            //Grid.SetColumn(n213, 13);
            //Grid.SetColumn(n214, 14);
            //Grid.SetColumn(n215, 15);
            //Grid.SetColumn(n216, 16);
            //Grid.SetColumn(n217, 17);
            //Grid.SetColumn(n218, 18);
            //Grid.SetColumn(n219, 19);
            //Grid.SetColumn(n220, 20);
            //Grid.SetColumn(n221, 21);
            //Grid.SetColumn(n222, 22);



            // 1
            //Grid.SetColumn(rackStair[0, 0], 3);
            //Grid.SetColumn(rackStair[1, 0], 3);
            //Grid.SetColumn(rackStair[2, 0], 3);

            //Grid.SetColumn(rackStair[0, 1], 4);
            //Grid.SetColumn(rackStair[1, 1], 4);
            //Grid.SetColumn(rackStair[2, 1], 4);

            //Grid.SetColumn(rackStair[0, 2], 5);
            //Grid.SetColumn(rackStair[1, 2], 5);
            //Grid.SetColumn(rackStair[2, 2], 5);

            //Grid.SetColumn(rackStair[0, 3], 6);
            //Grid.SetColumn(rackStair[1, 3], 6);
            //Grid.SetColumn(rackStair[2, 3], 6);

            //Grid.SetColumn(rackStair[0, 4], 7);
            //Grid.SetColumn(rackStair[1, 4], 7);
            //Grid.SetColumn(rackStair[2, 4], 7);

            //Grid.SetColumn(rackStair[0, 5], 8);
            //Grid.SetColumn(rackStair[1, 5], 8);
            //Grid.SetColumn(rackStair[2, 5], 8);

            //Grid.SetColumn(rackStair[0, 6], 9);
            //Grid.SetColumn(rackStair[1, 6], 9);
            //Grid.SetColumn(rackStair[2, 6], 9);

            //Grid.SetColumn(rackStair[0, 7], 10);
            //Grid.SetColumn(rackStair[1, 7], 10);
            //Grid.SetColumn(rackStair[2, 7], 10);

            //Grid.SetColumn(rackStair[0, 8], 11);
            //Grid.SetColumn(rackStair[1, 8], 11);
            //Grid.SetColumn(rackStair[2, 8], 11);

            //Grid.SetColumn(rackStair[0, 9], 12);
            //Grid.SetColumn(rackStair[1, 9], 12);
            //Grid.SetColumn(rackStair[2, 9], 12);

            //Grid.SetColumn(rackStair[0, 10], 13);
            //Grid.SetColumn(rackStair[1, 10], 13);
            //Grid.SetColumn(rackStair[2, 10], 13);

            //Grid.SetColumn(rackStair[0, 11], 14);
            //Grid.SetColumn(rackStair[1, 11], 14);
            //Grid.SetColumn(rackStair[2, 11], 14);

            //Grid.SetColumn(rackStair[0, 12], 15);
            //Grid.SetColumn(rackStair[1, 12], 15);
            //Grid.SetColumn(rackStair[2, 12], 15);

            //Grid.SetColumn(rackStair[0, 13], 16);
            //Grid.SetColumn(rackStair[1, 13], 16);
            //Grid.SetColumn(rackStair[2, 13], 16);

            //Grid.SetColumn(rackStair[0, 14], 17);
            //Grid.SetColumn(rackStair[1, 14], 17);
            //Grid.SetColumn(rackStair[2, 14], 17);

            //Grid.SetColumn(rackStair[0, 15], 18);
            //Grid.SetColumn(rackStair[1, 15], 18);
            //Grid.SetColumn(rackStair[2, 15], 18);

            //Grid.SetColumn(rackStair[0, 16], 19);
            //Grid.SetColumn(rackStair[1, 16], 19);
            //Grid.SetColumn(rackStair[2, 16], 19);

            //Grid.SetColumn(rackStair[0, 17], 20);
            //Grid.SetColumn(rackStair[1, 17], 20);
            //Grid.SetColumn(rackStair[2, 17], 20);

            //Grid.SetColumn(rackStair[0, 18], 21);
            //Grid.SetColumn(rackStair[1, 18], 21);
            //Grid.SetColumn(rackStair[2, 18], 21);

            //Grid.SetColumn(rackStair[0, 19], 22);
            //Grid.SetColumn(rackStair[1, 19], 22);
            //Grid.SetColumn(rackStair[2, 19], 22);

            //Grid.SetColumn(rackStair[0, 20], 23);
            //Grid.SetColumn(rackStair[1, 20], 23);
            //Grid.SetColumn(rackStair[2, 20], 23);

            //Grid.SetColumn(rackStair[0, 21], 24);
            //Grid.SetColumn(rackStair[1, 21], 24);
            //Grid.SetColumn(rackStair[2, 21], 24);



            // 2
            //Grid.SetColumn(rackStair2[0, 0], 3);
            //Grid.SetColumn(rackStair2[1, 0], 3);
            //Grid.SetColumn(rackStair2[2, 0], 3);

            //Grid.SetColumn(rackStair2[0, 1], 4);
            //Grid.SetColumn(rackStair2[1, 1], 4);
            //Grid.SetColumn(rackStair2[2, 1], 4);

            //Grid.SetColumn(rackStair2[0, 2], 5);
            //Grid.SetColumn(rackStair2[1, 2], 5);
            //Grid.SetColumn(rackStair2[2, 2], 5);

            //Grid.SetColumn(rackStair2[0, 3], 6);
            //Grid.SetColumn(rackStair2[1, 3], 6);
            //Grid.SetColumn(rackStair2[2, 3], 6);

            //Grid.SetColumn(rackStair2[0, 4], 7);
            //Grid.SetColumn(rackStair2[1, 4], 7);
            //Grid.SetColumn(rackStair2[2, 4], 7);

            //Grid.SetColumn(rackStair2[0, 5], 8);
            //Grid.SetColumn(rackStair2[1, 5], 8);
            //Grid.SetColumn(rackStair2[2, 5], 8);

            //Grid.SetColumn(rackStair2[0, 6], 9);
            //Grid.SetColumn(rackStair2[1, 6], 9);
            //Grid.SetColumn(rackStair2[2, 6], 9);

            //Grid.SetColumn(rackStair2[0, 7], 10);
            //Grid.SetColumn(rackStair2[1, 7], 10);
            //Grid.SetColumn(rackStair2[2, 7], 10);

            //Grid.SetColumn(rackStair2[0, 8], 11);
            //Grid.SetColumn(rackStair2[1, 8], 11);
            //Grid.SetColumn(rackStair2[2, 8], 11);

            //Grid.SetColumn(rackStair2[0, 9], 12);
            //Grid.SetColumn(rackStair2[1, 9], 12);
            //Grid.SetColumn(rackStair2[2, 9], 12);

            //Grid.SetColumn(rackStair2[0, 10], 13);
            //Grid.SetColumn(rackStair2[1, 10], 13);
            //Grid.SetColumn(rackStair2[2, 10], 13);

            //Grid.SetColumn(rackStair2[0, 11], 14);
            //Grid.SetColumn(rackStair2[1, 11], 14);
            //Grid.SetColumn(rackStair2[2, 11], 14);

            //Grid.SetColumn(rackStair2[0, 12], 15);
            //Grid.SetColumn(rackStair2[1, 12], 15);
            //Grid.SetColumn(rackStair2[2, 12], 15);

            //Grid.SetColumn(rackStair2[0, 13], 16);
            //Grid.SetColumn(rackStair2[1, 13], 16);
            //Grid.SetColumn(rackStair2[2, 13], 16);

            //Grid.SetColumn(rackStair2[0, 14], 17);
            //Grid.SetColumn(rackStair2[1, 14], 17);
            //Grid.SetColumn(rackStair2[2, 14], 17);

            //Grid.SetColumn(rackStair2[0, 15], 18);
            //Grid.SetColumn(rackStair2[1, 15], 18);
            //Grid.SetColumn(rackStair2[2, 15], 18);

            //Grid.SetColumn(rackStair2[0, 16], 19);
            //Grid.SetColumn(rackStair2[1, 16], 19);
            //Grid.SetColumn(rackStair2[2, 16], 19);

            //Grid.SetColumn(rackStair2[0, 17], 20);
            //Grid.SetColumn(rackStair2[1, 17], 20);
            //Grid.SetColumn(rackStair2[2, 17], 20);

            //Grid.SetColumn(rackStair2[0, 18], 21);
            //Grid.SetColumn(rackStair2[1, 18], 21);
            //Grid.SetColumn(rackStair2[2, 18], 21);

            //Grid.SetColumn(rackStair2[0, 19], 22);
            //Grid.SetColumn(rackStair2[1, 19], 22);
            //Grid.SetColumn(rackStair2[2, 19], 22);

            //Grid.SetColumn(rackStair2[0, 20], 23);
            //Grid.SetColumn(rackStair2[1, 20], 23);
            //Grid.SetColumn(rackStair2[2, 20], 23);

            //Grid.SetColumn(rackStair2[0, 21], 24);
            //Grid.SetColumn(rackStair2[1, 21], 24);
            //Grid.SetColumn(rackStair2[2, 21], 24);

            #endregion


        }

        /// <summary>
        /// 수동 출고 예약
        /// </summary>
        private void ManualIssue()
        {
            string port = (string)cboPort.SelectedValue;

            if (dtCheck.Rows.Count == 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4531"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                return;
            }


            if (port.Equals("SELECT"))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4532"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                return;
            }

            bool bIsOK = false;


            int iCountN = 0;

            foreach (DataRow row in dtCheck.Rows)
            {
                if (row["JUDG_VALUE"].ToString() != ObjectDic.Instance.GetObjectName("합격"))
                {
                    iCountN++;
                }
            }
            int iAnswerCount = 0;


            if (iCountN > 0)
            {
                //"수동출고 하시겠습니까?"
                Util.MessageConfirm("SFU4538", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            string sPrvRACK_ID = "";
                            foreach (DataRow row in dtCheck.Rows)
                            {
                                DataTable dtTime = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SYSDATE", "RQSTDT", "OUTDATA", null);

                                DateTime dtSysTime = Convert.ToDateTime(dtTime.Rows[0][0]);
                                string sRACK_ID = row["RACK_ID"].ToString();
                                if (sPrvRACK_ID == sRACK_ID)
                                {
                                    continue;
                                }
                                else
                                {
                                    sPrvRACK_ID = sRACK_ID;
                                }

                                string sJUDG_VALUE = row["JUDG_VALUE"].ToString();

                                bIsManualIssueOK = true;

                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("LANGID", typeof(string));
                                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                                RQSTDT.Columns.Add("RSV_TO_PORT_ID", typeof(string));
                                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                                RQSTDT.Columns.Add("DTTM", typeof(DateTime));

                                DataRow dr = RQSTDT.NewRow();
                                dr["LANGID"] = LoginInfo.LANGID;
                                dr["RACK_ID"] = sRACK_ID;
                                dr["RSV_TO_PORT_ID"] = port;
                                dr["UPDUSER"] = LoginInfo.USERID;
                                dr["DTTM"] = dtSysTime;
                                RQSTDT.Rows.Add(dr);
                                //DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_MAUAL_ISS_RSV_DTTM", "RQSTDT", "OUTDATA", RQSTDT);

                                DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("BR_MCS_UPD_MANUAL_ISS_RSV", "RQSTDT", "OUTDATA", RQSTDT);



                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (bIsManualIssueOK && iAnswerCount == 0)
                            {
                                iAnswerCount++;
                                Util.AlertInfo("SFU1275");

                            }

                            cboPort.SelectedIndex = 0;

                            // check 해제
                            rackStair.UncheckRack();
                            rackStair2.UncheckRack();

                            Util.gridClear(this.dgRackInfo);
                            //this.dtCheck.Rows.Clear();
                        }


                    }
                });




            }
            else
            {
                //"수동출고 하시겠습니까?"
                Util.MessageConfirm("SFU4539", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            string sPrvRACK_ID = "";
                            foreach (DataRow row in dtCheck.Rows)
                            {
                                DataTable dtTime = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SYSDATE", "RQSTDT", "OUTDATA", null);

                                DateTime dtSysTime = Convert.ToDateTime(dtTime.Rows[0][0]);
                                string sRACK_ID = row["RACK_ID"].ToString();
                                if (sPrvRACK_ID == sRACK_ID)
                                {
                                    continue;
                                }
                                else
                                {
                                    sPrvRACK_ID = sRACK_ID;
                                }

                                string sJUDG_VALUE = row["JUDG_VALUE"].ToString();

                                bIsManualIssueOK = true;

                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("LANGID", typeof(string));
                                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                                RQSTDT.Columns.Add("RSV_TO_PORT_ID", typeof(string));
                                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                                RQSTDT.Columns.Add("DTTM", typeof(DateTime));

                                DataRow dr = RQSTDT.NewRow();
                                dr["LANGID"] = LoginInfo.LANGID;
                                dr["RACK_ID"] = sRACK_ID;
                                dr["RSV_TO_PORT_ID"] = port;
                                dr["UPDUSER"] = LoginInfo.USERID;
                                dr["DTTM"] = dtSysTime;
                                RQSTDT.Rows.Add(dr);
                                //DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_MAUAL_ISS_RSV_DTTM", "RQSTDT", "OUTDATA", RQSTDT);

                                DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("BR_MCS_UPD_MANUAL_ISS_RSV", "RQSTDT", "OUTDATA", RQSTDT);

                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (bIsManualIssueOK && iAnswerCount == 0)
                            {
                                iAnswerCount++;
                                Util.AlertInfo("SFU1275");
                            }

                            cboPort.SelectedIndex = 0;
                        }
                    }
                });
            }
        }

        private void InitdtCheck()
        {
            dtCheck = new DataTable();
            dtCheck.Columns.Add("CHK", typeof(bool));
            dtCheck.Columns.Add("SEQ", typeof(int));
            dtCheck.Columns.Add("RACK_ID", typeof(string));
            dtCheck.Columns.Add("PRJT_NAME", typeof(string));
            dtCheck.Columns.Add("WOID", typeof(string));
            dtCheck.Columns.Add("PRODID", typeof(string));
            dtCheck.Columns.Add("PRODDESC", typeof(string));
            dtCheck.Columns.Add("PRODNAME", typeof(string));
            dtCheck.Columns.Add("MODLID", typeof(string));
            dtCheck.Columns.Add("WH_RCV_DTTM", typeof(DateTime));
            dtCheck.Columns.Add("ELAPSE", typeof(int));
            dtCheck.Columns.Add("LOTID", typeof(string));
            dtCheck.Columns.Add("WIP_QTY", typeof(decimal));
            dtCheck.Columns.Add("JUDG_VALUE", typeof(string));
            dtCheck.Columns.Add("MCS_CST_ID", typeof(string));
            dtCheck.Columns.Add("WIPHOLD", typeof(string));
            dtCheck.Columns.Add("SPCL_FLAG", typeof(string));
            dtCheck.Columns.Add("SPCL_RSNCODE", typeof(string));
            dtCheck.Columns.Add("WIP_REMARKS", typeof(string));
            dtCheck.Columns.Add("WIPDTTM_ED", typeof(string));
            dtCheck.Columns.Add("VLD_DATE", typeof(string));
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            cboColor.Items.Clear();

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboColor.Items.Add(cbItemTitle);

            // DB에서 가져오기

            DataTable RQDT = new DataTable("RQSTDT");
            RQDT.Columns.Add("LANGID", typeof(string));

            DataRow drColor = RQDT.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;

            RQDT.Rows.Add(drColor);

            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COLOR_LEGEND", "RQSTDT", "RSLTDT", RQDT);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem();
                cbItem1.Content = row["KEYNAME"].ToString();
                cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;

                cboColor.Items.Add(cbItem1);
            }



            cboColor.SelectedIndex = 0;

            //cboColor.Height = 300;
            cboColor.DropDownHeight = 400;


            //// 자동 조회 시간 Combo
            //String[] sFilter4 = { "SECOND_INTERVAL" };
            //_combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            //if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
            //    cboAutoSearchOut.SelectedIndex = 0;



            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = WH_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PRJT_FOR_COMBO", "RQSTDT", "RSLTDT", RQSTDT);


            cboProdId1.DisplayMemberPath = "CBO_NAME";
            cboProdId1.SelectedValuePath = "CBO_CODE";
            cboProdId1.ItemsSource = dtResult.Copy().AsDataView();

            cboProdId1.SelectedIndex = 0;


            cboProdId2.DisplayMemberPath = "CBO_NAME";
            cboProdId2.SelectedValuePath = "CBO_CODE";
            cboProdId2.ItemsSource = dtResult.Copy().AsDataView();

            cboProdId2.SelectedIndex = 0;

            cboProdId3.DisplayMemberPath = "CBO_NAME";
            cboProdId3.SelectedValuePath = "CBO_CODE";
            cboProdId3.ItemsSource = dtResult.Copy().AsDataView();

            cboProdId3.SelectedIndex = 0;

            DataTable RQSTDTELTR = new DataTable();
            RQSTDTELTR.TableName = "LANGID";
            RQSTDTELTR.Columns.Add("LANGID", typeof(string));

            DataRow drELTR = RQSTDTELTR.NewRow();
            drELTR["LANGID"] = LoginInfo.LANGID;
            RQSTDTELTR.Rows.Add(drELTR);

            DataTable dtELTRResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_ELTR_TYPE_FOR_COMBO", "RQSTDT", "RSLTDT", RQSTDTELTR);

            if (dtELTRResult.Rows.Count > 0)
            {
                cboELTRTYPE.DisplayMemberPath = "CMCDNAME";
                cboELTRTYPE.SelectedValuePath = "CMCODE";
                cboELTRTYPE.ItemsSource = dtELTRResult.Copy().AsDataView();

                cboELTRTYPE.SelectedIndex = 0;
            }


        }

        private void ReInitCombo()
        {
            string cbo1Code = this.cboProdId1.SelectedValue.ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = WH_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PRJT_FOR_COMBO", "RQSTDT", "RSLTDT", RQSTDT);


            cboProdId1.DisplayMemberPath = "CBO_NAME";
            cboProdId1.SelectedValuePath = "CBO_CODE";
            cboProdId1.ItemsSource = dtResult.Copy().AsDataView();

            //cboProdId1.SelectedIndex = 0;

            cboProdId1.SelectedValue = cbo1Code;


            string cbo2Code = this.cboProdId2.SelectedValue.ToString();
            cboProdId2.DisplayMemberPath = "CBO_NAME";
            cboProdId2.SelectedValuePath = "CBO_CODE";
            cboProdId2.ItemsSource = dtResult.Copy().AsDataView();

            //cboProdId2.SelectedIndex = 0;

            cboProdId2.SelectedValue = cbo2Code;


            string cbo3Code = this.cboProdId3.SelectedValue.ToString();

            cboProdId3.DisplayMemberPath = "CBO_NAME";
            cboProdId3.SelectedValuePath = "CBO_CODE";
            cboProdId3.ItemsSource = dtResult.Copy().AsDataView();

            //cboProdId3.SelectedIndex = 0;

            cboProdId3.SelectedValue = cbo2Code;



            string cboELTRTYPECode = this.cboELTRTYPE.SelectedValue.ToString();
            DataTable RQSTDTELTR = new DataTable();
            RQSTDTELTR.TableName = "LANGID";
            RQSTDTELTR.Columns.Add("LANGID", typeof(string));

            DataRow drELTR = RQSTDTELTR.NewRow();
            drELTR["LANGID"] = LoginInfo.LANGID;
            RQSTDTELTR.Rows.Add(drELTR);

            DataTable dtELTRResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_ELTR_TYPE_FOR_COMBO", "RQSTDT", "RSLTDT", RQSTDTELTR);

            if (dtELTRResult.Rows.Count > 0)
            {
                cboELTRTYPE.DisplayMemberPath = "CMCDNAME";
                cboELTRTYPE.SelectedValuePath = "CMCODE";
                cboELTRTYPE.ItemsSource = dtELTRResult.Copy().AsDataView();

                cboELTRTYPE.SelectedValue = cboELTRTYPECode;
                // cboELTRTYPE.SelectedIndex = 0;
            }
        }



        /// <summary>
        /// 창고재고 Grid 초기화
        /// </summary>
        private void InitStoreGrid()
        {
            DataTable GridData = new DataTable();
            GridData.Columns.Add("PRJT_NAME", typeof(string)); // 프로젝트명
            GridData.Columns.Add("CC", typeof(string)); // 양극
            GridData.Columns.Add("AC", typeof(string)); // 음극

            dgStore.ItemsSource = DataTableConverter.Convert(GridData);
        }

        /// <summary>
        /// QA검사 Grid 초기화
        /// </summary>
        private void InitQAGrid()
        {
            DataTable GridData = new DataTable();

            GridData.Columns.Add("ELTR_TYPE", typeof(string)); // 
            GridData.Columns.Add("CC", typeof(string)); // 양극
            GridData.Columns.Add("AC", typeof(string)); // 음극

            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("합격"), 0, 0 });
            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("검사중"), 0, 0 });
            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("불합격"), 0, 0 });
            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("재작업"), 0, 0 });
            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("대기"), 0, 0 });
            GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("V/D PROC"), 0, 0 });

            this.dgQA.ItemsSource = DataTableConverter.Convert(GridData);
        }

        /// <summary>
        /// 입고순위 Grid 초기화
        /// </summary>
        private void InitInputRankGrid()
        {
            DataTable GridData = new DataTable();

            GridData.Columns.Add("SEQ", typeof(int));
            GridData.Columns.Add("LOC", typeof(string)); // 
            GridData.Columns.Add("PRJT_NAME", typeof(string)); //
            GridData.Columns.Add("MCS_CST_ID", typeof(string)); //
            GridData.Columns.Add("WH_RCV_DTTM", typeof(string)); //
            GridData.Columns.Add("ELAPSE", typeof(string)); //

            this.dgInputRank.ItemsSource = DataTableConverter.Convert(GridData);
        }

        /// <summary>
        /// 출고대상 Pancake 정보 Grid 초기화
        /// </summary>
        private void InitPancakeInfoGrid()
        {
            DataTable GridData = new DataTable();

            GridData.Columns.Add("CHK", typeof(bool));
            GridData.Columns.Add("SEQ", typeof(int));
            GridData.Columns.Add("RACK", typeof(string)); // 
            GridData.Columns.Add("PROJECT", typeof(string)); //
            GridData.Columns.Add("ITEM", typeof(string)); //
            GridData.Columns.Add("MODEL", typeof(string)); //
            GridData.Columns.Add("DATE", typeof(string)); //
            GridData.Columns.Add("DURATION", typeof(string)); //
            GridData.Columns.Add("PANCAKE", typeof(string)); //
            GridData.Columns.Add("QTY", typeof(string)); //
            GridData.Columns.Add("JUDG_VALUE", typeof(string)); //

            GridData.Rows.Add(new object[] { false, 1, "", "", "", "", "", "", "", "", "" });
            GridData.Rows.Add(new object[] { false, 1, "", "", "", "", "", "", "", "", "" });
        }

        /// <summary>
        /// 단콤보 초기화
        /// </summary>
        private void InitStairCombo()
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WH_ID"] = WH_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_WH_STAIR_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cboStair.DisplayMemberPath = "CBO_NAME";
                cboStair.SelectedValuePath = "CBO_CODE";
                cboStair.ItemsSource = dtResult.Copy().AsDataView();

                cboStair.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReloadOutPortCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;

                if (chkNG.IsChecked.Value)
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RETURN_OUT_PORT", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_OUT_PORT", "RQSTDT", "RSLTDT", RQSTDT);
                }

                DataRow drSel = dtResult.NewRow();

                drSel["CBO_CODE"] = "SELECT";
                drSel["CBO_NAME"] = "-SELECT-";
                dtResult.Rows.InsertAt(drSel, 0);

                cboPort.DisplayMemberPath = "CBO_NAME";
                cboPort.SelectedValuePath = "CBO_CODE";
                cboPort.ItemsSource = dtResult.Copy().AsDataView();

                cboPort.SelectedValue = "SELECT";


                // cboPort.MinHeight = 1000;
                //cboPort.
                cboPort.MaxDropDownHeight = 1000;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitRackColors()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "DISP_COLR_CODE";
                RQSTDT.Rows.Add(dr);

                // 미리정의된 색상 가져오기
                DataTable dtColors = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                RQSTDT.Rows[0]["CMCDTYPE"] = "MCS_ISS_DATE_COLR_TYPE";
                // 입고일별 색상 가져오기
                DataTable dtIssueDateColor = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow drc in dtIssueDateColor.Rows)
                {
                    string clrcomponent = drc["ATTRIBUTE3"].ToString();
                    Color backcolor = Common.CommonCodeColor.GetCommonColor(clrcomponent, Colors.White);
                    Color forecolor = Colors.Black;
                    int day = 3;
                    Int32.TryParse(drc["ATTRIBUTE1"].ToString(), out day);

                    if (drc["CMCODE"].Equals("TYPE_A"))
                    {
                        // ex) 3일 이내
                        string str = string.Format(ObjectDic.Instance.GetObjectName("입고 {0}일 이내"), drc["ATTRIBUTE1"]);
                        lblColorTypeA.Text = str;
                        lblColorTypeA.Foreground = new SolidColorBrush(forecolor);
                        bdrColorTypeA.Background = new SolidColorBrush(backcolor);
                        rackStair.IssueDayTypeAForeColor = forecolor;
                        rackStair.IssueDayTypeABackColor = backcolor;
                        rackStair.IssueDayTypeADay = day;

                    }
                    else if (drc["CMCODE"].Equals("TYPE_B"))
                    {
                        // ex) 7일 이내
                        string str = string.Format(ObjectDic.Instance.GetObjectName("입고 {0}일 이내"), drc["ATTRIBUTE1"]);
                        lblColorTypeB.Text = str;
                        lblColorTypeB.Foreground = new SolidColorBrush(forecolor);
                        bdrColorTypeB.Background = new SolidColorBrush(backcolor);
                        rackStair.IssueDayTypeBForeColor = forecolor;
                        rackStair.IssueDayTypeBBackColor = backcolor;
                        rackStair.IssueDayTypeBDay = day;

                    }
                    else if (drc["CMCODE"].Equals("TYPE_C"))
                    {
                        // ex) 30일 이내
                        string str = string.Format(ObjectDic.Instance.GetObjectName("입고 {0}일 이내"), drc["ATTRIBUTE1"]);
                        lblColorTypeC.Text = str;
                        lblColorTypeC.Foreground = new SolidColorBrush(forecolor);
                        bdrColorTypeC.Background = new SolidColorBrush(backcolor);
                        rackStair.IssueDayTypeCForeColor = forecolor;
                        rackStair.IssueDayTypeCBackColor = backcolor;
                        rackStair.IssueDayTypeCDay = day;

                    }
                    else if (drc["CMCODE"].Equals("TYPE_D"))
                    {
                        // ex) 30일 이상
                        string str = string.Format(ObjectDic.Instance.GetObjectName("입고 {0}일 이상"), drc["ATTRIBUTE1"]);
                        lblColorTypeD.Text = str;
                        lblColorTypeD.Foreground = new SolidColorBrush(forecolor);
                        bdrColorTypeD.Background = new SolidColorBrush(backcolor);
                        rackStair.IssueDayTypeDForeColor = forecolor;
                        rackStair.IssueDayTypeDBackColor = backcolor;
                        rackStair.IssueDayTypeDDay = day;
                    }
                } // foreach( DataRow drc in dtIssueDateColor.Rows )

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Skid Rack 정보 새로고침
        /// </summary>
        private void ReloadSkidRack()
        {
            // 전체 enable
            for (int r = 0; r < rackStair.RowCount; r++)
            {
                for (int c = 0; c < rackStair.ColumnCount; c++)
                {
                    SkidRack sr = rackStair[r, c];
                    sr.IsEnabled = true;
                    sr.SetRackBackcolor();
                }
            }

            for (int r = 0; r < rackStair2.RowCount; r++)
            {
                for (int c = 0; c < rackStair2.ColumnCount; c++)
                {
                    SkidRack sr = rackStair2[r, c];
                    sr.IsEnabled = true;
                    sr.SetRackBackcolor();
                }

                try
                {
                    this.dtCheck.Clear();
                }
                catch (Exception ex)
                {
                    Util.MessageInfo(ex.Message.ToString());
                }

                lblEqpt.Visibility = Visibility.Visible;
                txtEqpt.Visibility = Visibility.Visible;
                lblModel.Visibility = Visibility.Visible;
                txtProd.Visibility = Visibility.Visible;
                lblAutoDeliveryStatus.Visibility = Visibility.Visible;
                txtAutoDeliveryStatus.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("Z_PSTN", typeof(int));
                RQSTDT.Columns.Add("X_PSTN", typeof(string));

                DataRow Indata = RQSTDT.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["WH_ID"] = WH_ID;
                Indata["Z_PSTN"] = cboStair.SelectedValue;
                //if ((bool)rdoAnode.IsChecked)
                //{
                //    Indata["ZONE_ID"] = "A";
                //}
                //else
                //{
                //    Indata["ZONE_ID"] = "B";
                //}

                if ((bool)rdoAnode.IsChecked)
                {
                    Indata["X_PSTN"] = "1";
                }
                else
                {
                    Indata["X_PSTN"] = "2";
                }

                RQSTDT.Rows.Add(Indata);

                try
                {
                    // 전체, 단 사용율
                    DataTable dtAllUseRate = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_ALL_USE_RATE", "RQSTDT", "RSLTDT", RQSTDT);
                    string sAllUseRate = ((decimal)dtAllUseRate.Rows[0][0]).ToString("#,##0.00");

                    txtTotalUsageRate.Text = sAllUseRate;

                    txtStairUsageRate.Text = "";


                    if (cboStair.SelectedValue.ToString() != "0")
                    {
                        DataTable dtUseRate = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_USE_RATE", "RQSTDT", "RSLTDT", RQSTDT);
                        string sUseRate = ((decimal)dtUseRate.Rows[0][0]).ToString("#,##0.00");
                        txtStairUsageRate.Text = sUseRate;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageInfo(ex.Message.ToString());
                }

                try
                {
                    this.rackStair.HideAndClearAllRack();

                    this.rackStair2.HideAndClearAllRack();

                    Util.gridClear(this.dgRackInfo);

                    DataTable dtMaxAB = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAXAB", "RQSTDT", "RSLTDT", null);

                    int iMaxA = Convert.ToInt16(dtMaxAB.Rows[0][0]);
                    int iMaxB = Convert.ToInt16(dtMaxAB.Rows[0][1]);
                    int iMaxT = Convert.ToInt16(dtMaxAB.Rows[0][2]);
                    for (int rr = 1; rr <= iMaxA; rr++)
                    {
                        string sN = "n2" + rr.ToString("D" + 2);
                        TextBlock tb = (TextBlock)this.FindName(sN);
                        tb.Visibility = Visibility.Visible;
                    }
                    for (int rr = 1; rr <= iMaxB; rr++)
                    {
                        string sN = "n1" + rr.ToString("D" + 2);
                        TextBlock tb = (TextBlock)this.FindName(sN);
                        tb.Visibility = Visibility.Visible;
                    }

                    for (int rr = iMaxA + 1; rr <= iMaxT; rr++)
                    {
                        string sN = "n1" + rr.ToString("D" + 2);
                        TextBlock tb = (TextBlock)this.FindName(sN);
                        tb.Visibility = Visibility.Hidden;
                    }

                    for (int rr = iMaxB + 1; rr <= iMaxT; rr++)
                    {
                        string sN = "n1" + rr.ToString("D" + 2);
                        TextBlock tb = (TextBlock)this.FindName(sN);
                        tb.Visibility = Visibility.Hidden;
                    }

                    DataTable RSLTDT = null;

                    if (cboStair.SelectedValue.ToString() == "0")
                    {
                        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_BUFFER_ALL_X_PSTN", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    else
                    {
                        RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_BUFFER_X_PSTN", "RQSTDT", "RSLTDT", RQSTDT);
                    }

                    SkidRack sr = null;


                    foreach (DataRow dr in RSLTDT.Rows)
                    {
                        int x = 0;
                        int y = 0;

                        if (dr["ZONE_ID"].ToString() == "A")
                        {
                            if (dr["Z_PSTN"].ToString() == "1")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 2;
                                    y = 0;
                                }
                                else
                                {
                                    x = 2;
                                    y = 1;
                                }
                            }
                            else if (dr["Z_PSTN"].ToString() == "2")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 1;
                                    y = 0;
                                }
                                else
                                {
                                    x = 1;
                                    y = 1;
                                }
                            }
                            else if (dr["Z_PSTN"].ToString() == "3")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 0;
                                    y = 0;
                                }
                                else
                                {
                                    x = 0;
                                    y = 1;
                                }
                            }

                            sr = rackStair[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            //////////SkidRack sr = rackStair[string.Format("r{0:0}c{1:00}", int.Parse(dr["X_PSTN"].ToString()) - 1, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////if (y < 1)
                            ////////{
                            ////////    sr = rackStair[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////}
                            ////////else
                            ////////{
                            ////////    sr = rackStair2[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////}
                            //SkidRack sr = rackStair[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            if (sr == null)
                            {
                                continue;
                            }

                            sr.ZoneId = dr["ZONE_ID"].ToString();
                            sr.RackId = dr["RACK_ID"].ToString();
                            sr.Row = int.Parse(dr["X_PSTN"].ToString()); // row index 아님
                            sr.Col = int.Parse(dr["Y_PSTN"].ToString()); // col index 아님
                            sr.Stair = int.Parse(dr["Z_PSTN"].ToString());
                            sr.PRDT_CLSS_CODE = dr["PRDT_CLSS_CODE"].ToString();

                            sr.ElapseDay = (dr["ELAPSE"] == DBNull.Value ? 0 : (int)dr["ELAPSE"]); //순서바뀜

                            sr.ProjectName = dr["PRJT_NAME"].ToString();

                            sr.SkidID = dr["MCS_CST_ID"].ToString();

                            sr.Spcl_Flag = dr["SPCL_FLAG"].ToString();
                            sr.Spcl_RsnCode = dr["SPCL_RSNCODE"].ToString();
                            sr.Wip_Remarks = dr["WIP_REMARKS"].ToString();
                            sr.Wipdttm_ED = dr["WIPDTTM_ED"].ToString();
                          

                            string qa = "";
                            switch (dr["JUDG_VALUE"].ToString())
                            {
                                case "TERM":
                                    qa = ObjectDic.Instance.GetObjectName("TERM");
                                    break;

                                case "Y":
                                    qa = ObjectDic.Instance.GetObjectName("합격");
                                    break;
                                case "F":
                                    qa = ObjectDic.Instance.GetObjectName("불합격");
                                    break;
                                case "W":
                                    qa = ObjectDic.Instance.GetObjectName("대기");
                                    break;
                                case "I":
                                    qa = ObjectDic.Instance.GetObjectName("검사중");
                                    break;
                                case "R":
                                    qa = ObjectDic.Instance.GetObjectName("재작업");
                                    break;
                                case "PR":
                                    qa = "In V/D";
                                    break;
                                default:
                                    qa = "";
                                    break;
                            }

                            if (dr["ID_SEQ"].ToString().Equals("1"))
                            {
                                // 1번 pancake
                                sr.PancakeID1 = dr["LOTID"].ToString();
                                sr.PancakeQA1 = qa;
                                sr.WIPHOLD1 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_1", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_1", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_1", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_1", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_1", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_1", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_1", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_1", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_1", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_1", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("2"))
                            {
                                // 2번 pancake
                                sr.PancakeID2 = dr["LOTID"].ToString();
                                sr.PancakeQA2 = qa;
                                sr.WIPHOLD2 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_2", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_2", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_2", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_2", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_2", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_2", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_2", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_2", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_2", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_2", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("3"))
                            {
                                // 3번 pancake
                                sr.PancakeID3 = dr["LOTID"].ToString();
                                sr.PancakeQA3 = qa;
                                sr.WIPHOLD3 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_3", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_3", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_3", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_3", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_3", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_3", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_3", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_3", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_3", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_3", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("4"))
                            {
                                // 4번 pancake
                                sr.PancakeID4 = dr["LOTID"].ToString();
                                sr.PancakeQA4 = qa;
                                sr.WIPHOLD4 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_4", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_4", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_4", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_4", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_4", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_4", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_4", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_4", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_4", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_4", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("5"))
                            {
                                // 5번 pancake
                                sr.PancakeID5 = dr["LOTID"].ToString();
                                sr.PancakeQA5 = qa;
                                sr.WIPHOLD5 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_5", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_5", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_5", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_5", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_5", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_5", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_5", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_5", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_5", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_5", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("6"))
                            {
                                // 6번 pancake
                                sr.PancakeID6 = dr["LOTID"].ToString();
                                sr.PancakeQA6 = qa;
                                sr.WIPHOLD6 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_6", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_6", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_6", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_6", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_6", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_6", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_6", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_6", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_6", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_6", dr["VLD_DATE"]);
                            }
                            sr.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (dr["Z_PSTN"].ToString() == "1")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 2;
                                    y = 0;
                                }
                                else
                                {
                                    x = 2;
                                    y = 1;
                                }
                            }
                            else if (dr["Z_PSTN"].ToString() == "2")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 1;
                                    y = 0;
                                }
                                else
                                {
                                    x = 1;
                                    y = 1;
                                }
                            }
                            else if (dr["Z_PSTN"].ToString() == "3")
                            {
                                if (dr["X_PSTN"].ToString() == "1")
                                {
                                    x = 0;
                                    y = 0;
                                }
                                else
                                {
                                    x = 0;
                                    y = 1;
                                }
                            }

                            sr = rackStair2[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            //////////SkidRack sr = rackStair[string.Format("r{0:0}c{1:00}", int.Parse(dr["X_PSTN"].ToString()) - 1, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////if (y < 1)
                            ////////{
                            ////////    sr = rackStair[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////}
                            ////////else
                            ////////{
                            ////////    sr = rackStair2[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            ////////}
                            //SkidRack sr = rackStair[string.Format("r{0:0}c{1:00}", x, int.Parse(dr["Y_PSTN"].ToString()) - 1)];
                            if (sr == null)
                            {
                                continue;
                            }

                            sr.ZoneId = dr["ZONE_ID"].ToString();
                            sr.RackId = dr["RACK_ID"].ToString();
                            sr.Row = int.Parse(dr["X_PSTN"].ToString()); // row index 아님
                            sr.Col = int.Parse(dr["Y_PSTN"].ToString()); // col index 아님
                            sr.Stair = int.Parse(dr["Z_PSTN"].ToString());
                            sr.PRDT_CLSS_CODE = dr["PRDT_CLSS_CODE"].ToString();

                            sr.ElapseDay = (dr["ELAPSE"] == DBNull.Value ? 0 : (int)dr["ELAPSE"]); //순서바뀜

                            sr.ProjectName = dr["PRJT_NAME"].ToString();

                            sr.SkidID = dr["MCS_CST_ID"].ToString();

                            sr.Spcl_Flag = dr["SPCL_FLAG"].ToString();
                            sr.Spcl_RsnCode = dr["SPCL_RSNCODE"].ToString();
                            sr.Wip_Remarks = dr["WIP_REMARKS"].ToString();
                            sr.Wipdttm_ED = dr["WIPDTTM_ED"].ToString();


                            string qa = "";
                            switch (dr["JUDG_VALUE"].ToString())
                            {
                                case "TERM":
                                    qa = ObjectDic.Instance.GetObjectName("TERM");
                                    break;

                                case "Y":
                                    qa = ObjectDic.Instance.GetObjectName("합격");
                                    break;
                                case "F":
                                    qa = ObjectDic.Instance.GetObjectName("불합격");
                                    break;
                                case "W":
                                    qa = ObjectDic.Instance.GetObjectName("대기");
                                    break;
                                case "I":
                                    qa = ObjectDic.Instance.GetObjectName("검사중");
                                    break;
                                case "R":
                                    qa = ObjectDic.Instance.GetObjectName("재작업");
                                    break;
                                case "PR":
                                    qa = "In V/D";
                                    break;
                                default:
                                    qa = "";
                                    break;
                            }
                            if (dr["ID_SEQ"].ToString().Equals("1"))
                            {
                                // 1번 pancake
                                sr.PancakeID1 = dr["LOTID"].ToString();
                                sr.PancakeQA1 = qa;
                                sr.WIPHOLD1 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_1", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_1", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_1", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_1", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_1", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_1", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_1", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_1", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_1", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_1", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("2"))
                            {
                                // 2번 pancake
                                sr.PancakeID2 = dr["LOTID"].ToString();
                                sr.PancakeQA2 = qa;
                                sr.WIPHOLD2 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_2", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_2", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_2", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_2", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_2", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_2", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_2", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_2", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_2", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_2", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("3"))
                            {
                                // 3번 pancake
                                sr.PancakeID3 = dr["LOTID"].ToString();
                                sr.PancakeQA3 = qa;
                                sr.WIPHOLD3 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_3", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_3", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_3", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_3", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_3", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_3", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_3", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_3", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_3", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_3", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("4"))
                            {
                                // 4번 pancake
                                sr.PancakeID4 = dr["LOTID"].ToString();
                                sr.PancakeQA4 = qa;
                                sr.WIPHOLD4 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_4", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_4", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_4", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_4", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_4", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_4", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_4", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_4", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_4", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_4", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("5"))
                            {
                                // 5번 pancake
                                sr.PancakeID5 = dr["LOTID"].ToString();
                                sr.PancakeQA5 = qa;
                                sr.WIPHOLD5 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_5", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_5", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_5", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_5", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_5", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_5", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_5", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_5", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_5", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_5", dr["VLD_DATE"]);
                            }
                            else if (dr["ID_SEQ"].ToString().Equals("6"))
                            {
                                // 6번 pancake
                                sr.PancakeID6 = dr["LOTID"].ToString();
                                sr.PancakeQA6 = qa;
                                sr.WIPHOLD6 = dr["WIPHOLD"].ToString();

                                sr.UserData.Add("RACK_ID_6", dr["RACK_ID"]);
                                sr.UserData.Add("JUDG_VALUE_6", dr["JUDG_VALUE"]);
                                sr.UserData.Add("WOID_6", dr["WOID"]);
                                sr.UserData.Add("WIP_QTY_6", dr["WIP_QTY"]);
                                sr.UserData.Add("PRODID_6", dr["PRODID"]);
                                sr.UserData.Add("PRODDESC_6", dr["PRODDESC"]);
                                sr.UserData.Add("PRODNAME_6", dr["PRODNAME"]);
                                sr.UserData.Add("MODLID_6", dr["MODLID"]);
                                sr.UserData.Add("WH_RCV_DTTM_6", dr["WH_RCV_DTTM"]);
                                sr.UserData.Add("VLD_DATE_6", dr["VLD_DATE"]);
                            }
                            sr.Visibility = Visibility.Visible;
                        }


                    }
                }
                catch (Exception ex)
                {
                    Util.MessageInfo(ex.Message.ToString());
                }
                // 비상취출포드
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.Columns.Add("LANGID", typeof(string));
                RQSTDT2.Columns.Add("WH_ID", typeof(string));
                RQSTDT2.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow Indata2 = RQSTDT2.NewRow();
                Indata2["LANGID"] = LoginInfo.LANGID;
                Indata2["WH_ID"] = WH_ID;
                if ((bool)rdoAnode.IsChecked)
                {
                    Indata2["ELTR_TYPE_CODE"] = "C";
                }
                else
                {
                    Indata2["ELTR_TYPE_CODE"] = "A";
                }
                RQSTDT2.Rows.Add(Indata2);

                DataTable RSLTDT2 = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EMERGENCY_PORT", "RQSTDT", "RSLTDT2", RQSTDT2);

                if (RSLTDT2.Rows.Count > 0)
                {

                    string sPort_Name = RSLTDT2.Rows[0]["PORT_NAME"].ToString();
                    string sFull_Flag = RSLTDT2.Rows[0]["FULL_FLAG"].ToString();
                    EmgPORT_ID = RSLTDT2.Rows[0]["PORT_ID"].ToString();
                    string sCST_QTY = RSLTDT2.Rows[0]["CST_QTY"].ToString();

                    string sKEYVALUE = RSLTDT2.Rows[0]["KEYVALUE"].ToString();

                    if (sKEYVALUE == "")
                    {
                        EmegPORTGrid1.Background = new SolidColorBrush(Colors.White);
                        EmegPORTGrid2.Background = new SolidColorBrush(Colors.White);
                        EmegPORTGrid3.Background = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        EmegPORTGrid1.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(sKEYVALUE));
                        EmegPORTGrid2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(sKEYVALUE));
                        EmegPORTGrid3.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(sKEYVALUE));
                    }


                    lblEmegPORT.Text = sPort_Name;


                    if (sCST_QTY == "1")
                    {
                        EmegPORTGrid1.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da = new ColorAnimation();
                        da.From = Colors.White;
                        da.To = Colors.Red;
                        da.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da.AutoReverse = true;
                        da.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid1.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                        try
                        {
                            EmegPORTGrid2.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                            EmegPORTGrid3.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageInfo(ex.Message.ToString());
                        }
                    }
                    else if (sCST_QTY == "2")
                    {
                        EmegPORTGrid1.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da = new ColorAnimation();
                        da.From = Colors.White;
                        da.To = Colors.Red;
                        da.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da.AutoReverse = true;
                        da.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid1.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                        EmegPORTGrid2.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da2 = new ColorAnimation();
                        da2.From = Colors.White;
                        da2.To = Colors.Red;
                        da2.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da2.AutoReverse = true;
                        da2.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid2.Background.BeginAnimation(SolidColorBrush.ColorProperty, da2);

                        try
                        {
                            EmegPORTGrid3.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageInfo(ex.Message.ToString());
                        }
                    }
                    else if (sCST_QTY == "3")
                    {
                        EmegPORTGrid1.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da = new ColorAnimation();
                        da.From = Colors.White;
                        da.To = Colors.Red;
                        da.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da.AutoReverse = true;
                        da.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid1.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);

                        EmegPORTGrid2.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da2 = new ColorAnimation();
                        da2.From = Colors.White;
                        da2.To = Colors.Red;
                        da2.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da2.AutoReverse = true;
                        da2.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid2.Background.BeginAnimation(SolidColorBrush.ColorProperty, da2);

                        EmegPORTGrid3.Background = new SolidColorBrush(Colors.White);
                        ColorAnimation da3 = new ColorAnimation();
                        da3.From = Colors.White;
                        da3.To = Colors.Red;
                        da3.Duration = new Duration(TimeSpan.FromSeconds(1));
                        da3.AutoReverse = true;
                        da3.RepeatBehavior = RepeatBehavior.Forever;
                        EmegPORTGrid3.Background.BeginAnimation(SolidColorBrush.ColorProperty, da3);
                    }
                    else
                    {
                        EmegPORTGrid1.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        EmegPORTGrid2.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                        EmegPORTGrid3.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                }
            }
            this.ArrPosX();
        }

        private void ReloadStore()
        {

            DataRowView prjt_name = (DataRowView)cboProdId1.SelectedItem;
            string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();


            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("WH_ID", typeof(string));
            if (PRJT_NAME != "0")
            {
                inTable.Columns.Add("PRJT_NAME", typeof(string));
            }


            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["WH_ID"] = WH_ID;
            if (PRJT_NAME != "0")
            {
                newRow["PRJT_NAME"] = PRJT_NAME;
            }
            inTable.Rows.Add(newRow);


            string sBizName = String.Empty;

            if (PRJT_NAME != "0")
            {
                sBizName = "DA_MCS_SEL_WH_QTY_BY_PRJT";
            }
            else
            {
                sBizName = "DA_MCS_SEL_WH_QTY";
            }

            try
            {
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }


                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            foreach (DataRow row in searchResult.Rows)
                            {
                                if (row["PRJT_NAME"].ToString() == "적재가능수량(총수량)")
                                    row["PRJT_NAME"] = ObjectDic.Instance.GetObjectName("적재가능수량(총수량)");
                            }
                        }
                        Util.GridSetData(this.dgStore, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
               );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReloadQA()
        {
            try
            {

                DataRowView prjt_name = (DataRowView)cboProdId2.SelectedItem;
                string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();


                DataTable inTable = new DataTable();
                inTable.Columns.Add("WH_ID", typeof(string));
                if (PRJT_NAME != "0")
                {
                    inTable.Columns.Add("PRJT_NAME", typeof(string));
                }

                DataRow newRow = inTable.NewRow();
                newRow["WH_ID"] = WH_ID;
                if (PRJT_NAME != "0")
                {
                    newRow["PRJT_NAME"] = PRJT_NAME;
                }

                inTable.Rows.Add(newRow);

                string sBiz_Name = String.Empty;
                if (PRJT_NAME != "0")
                {
                    sBiz_Name = "DA_MCS_SEL_QA_BY_PRJT";
                }
                else
                {
                    sBiz_Name = "DA_MCS_SEL_QA";
                }

                new ClientProxy().ExecuteService(sBiz_Name, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        DataTable GridData = new DataTable();

                        GridData.Columns.Add("ELTR_TYPE_CODE", typeof(string)); // 
                        GridData.Columns.Add("CC", typeof(int)); // 양극
                        GridData.Columns.Add("AC", typeof(int)); // 음극
                        GridData.Columns.Add("CODE", typeof(string));

                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("합격"), 0, 0, 'Y' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("검사중"), 0, 0, 'I' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("불합격"), 0, 0, 'F' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("재작업"), 0, 0, 'R' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("대기"), 0, 0, 'W' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("V/D PROC"), 0, 0, 'V' });

                        foreach (DataRow dr in searchResult.Rows)
                        {
                            int idx = 0;
                            if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                            {
                                // 양극
                                idx = 1;
                            }
                            else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                            {
                                // 음극
                                idx = 2;
                            }
                            if (idx > 0)
                            {
                                GridData.Rows[0][idx] = dr["QA_Y"];
                                GridData.Rows[1][idx] = dr["QA_I"];
                                GridData.Rows[2][idx] = dr["QA_F"];
                                GridData.Rows[3][idx] = dr["QA_R"];
                                GridData.Rows[4][idx] = dr["QA_W"];
                                GridData.Rows[5][idx] = dr["QA_V"];
                            }
                        }
                        Util.GridSetData(this.dgQA, GridData, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReloadInputRank()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WH_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WH_ID"] = WH_ID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_MCS_SEL_RANK", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dvRank = searchResult.DefaultView;

                        Util.GridSetData(this.dgInputRank, searchResult, null, false);

                        RankingDataView();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Refresh()
        {
            try
            {
                monitorTimer.Stop();

                chkNG.IsChecked = false;

                lblEqpt.Visibility = Visibility.Visible;
                txtEqpt.Visibility = Visibility.Visible;
                lblModel.Visibility = Visibility.Visible;
                txtProd.Visibility = Visibility.Visible;
                lblAutoDeliveryStatus.Visibility = Visibility.Visible;
                txtAutoDeliveryStatus.Visibility = Visibility.Visible;
                // 전체 enable
                for (int r = 0; r < rackStair.RowCount; r++)
                {
                    for (int c = 0; c < rackStair.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair[r, c];
                        sr.IsEnabled = true;

                    }
                }

                for (int r = 0; r < rackStair2.RowCount; r++)
                {
                    for (int c = 0; c < rackStair2.ColumnCount; c++)
                    {
                        SkidRack sr = rackStair2[r, c];
                        sr.IsEnabled = true;
                    }
                }


                loadingIndicator.Visibility = Visibility.Visible;
                ReloadStore();
                ReloadQA();
                ReloadInputRank();
                ReloadSkidRack();

            }
            finally
            {
                // 새로 고침 시간 update
                //refeshTo = refeshInterval;
                if (bSetAutoSelTime)
                {
                    monitorTimer.Start();
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// RACK MAX 열 COUNT
        /// </summary>
        private void GetColumnCount()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = WH_ID;

            RQSTDT.Rows.Add(dr);
            DataTable dtResult = null;
            dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAXY", "RQSTDT", "RSLTDT", RQSTDT);

            ColumnCount = Convert.ToInt16(dtResult.Rows[0][0]);
        }

        /// <summary>
        /// 비상취출포트 더블클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmegPORTGrid_mouse_down(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                //Popup창 생성
                MCS001_001_PORT_INFO wndPopup = new MCS001_001_PORT_INFO();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    object[] Parameters = new object[] { EmgPORT_ID, "A5A101", null };
                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += OnPancakeInfoPopupClosed;

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void MakeColumnDefinition()
        {
            ColGrid1.Children.Clear();
            for (int i = 1; i <= ColumnCount; i++)
            {
                ColumnDefinition coldef;
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(1, GridUnitType.Star);
                ColGrid1.ColumnDefinitions.Add(coldef);

                TextBlock tb;
                tb = new TextBlock();
                RegisterName("n1" + i.ToString("D" + 2), tb);
                string sText = i.ToString() + "연";
                tb.SetBinding(TextBlock.TextProperty, new Binding() { ConverterParameter = sText, Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                tb.VerticalAlignment = VerticalAlignment.Bottom;

                Grid.SetColumn(tb, i);
                ColGrid1.Children.Add(tb);



                ColumnDefinition coldef2;
                coldef2 = new ColumnDefinition();
                coldef2.Width = new GridLength(1, GridUnitType.Star);
                ColGrid2.ColumnDefinitions.Add(coldef2);

                TextBlock tb2;
                tb2 = new TextBlock();
                RegisterName("n2" + i.ToString("D" + 2), tb2);
                string sText2 = i.ToString() + "연";
                tb2.SetBinding(TextBlock.TextProperty, new Binding() { ConverterParameter = sText, Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                tb2.VerticalAlignment = VerticalAlignment.Bottom;

                Grid.SetColumn(tb2, i);
                ColGrid2.Children.Add(tb2);
            }
        }

        private void SearchSKIDFromRank(string sSKIDID)
        {
            for (int r = 0; r < rackStair.RowCount; r++)
            {
                for (int c = 0; c < rackStair.ColumnCount; c++)
                {
                    SkidRack sr = rackStair[r, c];

                    if ((sr.PancakeID1).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.PancakeID2).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.SkidID).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }
                }
            }

            for (int r = 0; r < rackStair2.RowCount; r++)
            {
                for (int c = 0; c < rackStair2.ColumnCount; c++)
                {
                    SkidRack sr = rackStair2[r, c];
                    if ((sr.PancakeID1).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.PancakeID2).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.SkidID).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (!sr.IsChecked)
                        {
                            sr.IsChecked = true;
                            this.CheckSkidRack(sr);
                        }
                        return;
                    }
                }
            }
            //// 없는 경우 message box 100119
            //// DB에서 검색
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("WH_ID", typeof(string));
            RQSTDT.Columns.Add("MCS_CST_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["WH_ID"] = WH_ID;
            dr["MCS_CST_ID"] = sSKIDID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = null;
            dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_BUFFER_BY_SKID", "RQSTDT", "RSLTDT", RQSTDT);

            int maxSeq;
            if (dtCheck.Rows.Count == 0)
            {
                maxSeq = 1;
            }
            else
            {
                maxSeq = Convert.ToInt32(dtCheck.Compute("max([SEQ])", string.Empty)) + 1;
            }

            foreach (DataRow row in dtResult.Rows)
            {

                dr = dtCheck.NewRow();
                dr["CHK"] = true;
                dr["SEQ"] = maxSeq;
                dr["RACK_ID"] = row["RACK_ID"];
                dr["LOTID"] = row["LOTID"];
                dr["PRJT_NAME"] = row["PRJT_NAME"];
                dr["SPCL_FLAG"] = row["SPCL_FLAG"];
                dr["SPCL_RSNCODE"] = row["SPCL_RSNCODE"];
                dr["WIP_REMARKS"] = row["WIP_REMARKS"];
                dr["WIPDTTM_ED"] = row["WIPDTTM_ED"];
                dr["VLD_DATE"] = row["VLD_DATE"];
                dr["WOID"] = row["WOID"];
                dr["PRODID"] = row["PRODID"];
                dr["PRODDESC"] = row["PRODDESC"];
                dr["PRODNAME"] = row["PRODNAME"];
                dr["MODLID"] = row["MODLID"];
                dr["WH_RCV_DTTM"] = row["WH_RCV_DTTM"];
                dr["WIP_QTY"] = row["WIP_QTY"];
                dr["JUDG_VALUE"] = row["JUDG_VALUE"];
                dr["MCS_CST_ID"] = row["MCS_CST_ID"];
                dr["ELAPSE"] = row["ELAPSE"];
                dr["WIPHOLD"] = row["WIPHOLD"];
                dtCheck.Rows.Add(dr);
            }
            Util.GridSetData(this.dgRackInfo, dtCheck, null, false);

            //if (dtResult.Rows.Count == 0)
            //{
            //    Util.AlertInfo("100119");
            //}
            //else
            //{
            //    string sZONE = String.Empty;
            //    string sPSTN = String.Empty;

            //    sZONE = dtResult.Rows[0][0].ToString();
            //    sPSTN = dtResult.Rows[0][1].ToString();
            //    //Util.AlertInfo("ZONE : " + sZONE + "     RACK : " + sPSTN + "에 존재합니다");
            //    Util.AlertInfo("SFU4541", new object[] { sZONE, sPSTN });
            //}
        }

        private void SearchSKIDFromRankToUncheck(string sSKIDID)
        {
            for (int r = 0; r < rackStair.RowCount; r++)
            {
                for (int c = 0; c < rackStair.ColumnCount; c++)
                {
                    SkidRack sr = rackStair[r, c];

                    if ((sr.PancakeID1).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (sr.IsChecked)
                        {
                            sr.IsChecked = false;
                            this.UnCheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.PancakeID2).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (sr.IsChecked)
                        {
                            sr.IsChecked = false;
                            this.UnCheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.SkidID).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (sr.IsChecked)
                        {
                            sr.IsChecked = false;
                            this.UnCheckSkidRack(sr);
                        }
                        return;
                    }
                }
            }

            for (int r = 0; r < rackStair2.RowCount; r++)
            {
                for (int c = 0; c < rackStair2.ColumnCount; c++)
                {
                    SkidRack sr = rackStair2[r, c];
                    if ((sr.PancakeID1).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (sr.IsChecked)
                        {
                            sr.IsChecked = false;
                            this.UnCheckSkidRack(sr);
                        }
                        return;
                    }

                    if ((sr.SkidID).ToUpper() == (sSKIDID).ToUpper())
                    {
                        if (sr.IsChecked)
                        {
                            sr.IsChecked = false;
                            this.UnCheckSkidRack(sr);
                        }
                        return;
                    }
                }
            }
            try
            {
                this.UnCheckSkidRackBySkidId(sSKIDID);
            }
            catch
            {
            }
        }

        private void TimerSetting()
        {
            //monitorTimer.Interval = new TimeSpan( 0, 0, 0, 60 );    // 60초에 한번
            monitorTimer.Interval = new TimeSpan(0, 0, 0, 1);
            monitorTimer.Tick += new EventHandler(OnMonitorTimer);
            refeshAfter = ObjectDic.Instance.GetObjectName("{0}초 후");
            if (bSetAutoSelTime)
            {
                OnChkAutoRefreshClick(null, null);
            }
        }
        #endregion

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color WH = System.Drawing.Color.White;

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);
        }

    }
}
