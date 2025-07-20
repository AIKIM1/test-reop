/*************************************************************************************
 Created Date : 2021.09.30
      Creator : 오화백
   Decription : DDA 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.30  오화백 : Initial Created.   
  2021.11.18  정재홍 : [C20221021-000451] - DDA monitoring history
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;


namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_070 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        public MCS001_070()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();
 
            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);

            
            //공정
            SetProcess(cboProcess);
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            
            SetProcess(cboProcess2);
            cboProcess2.SelectedItemChanged += cboProcess2_SelectedItemChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControls();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;

            dtpFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
        }

        #endregion

        #region Event


        #region 조회 버튼 : btnSearch_Click()

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetDataList();
        }


        #endregion

        #region ACS 조회버튼 : btnSearchACS_Click()
        private void btnSearchACS_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgBuffer);
            GetMonitoringData();
        }

        #endregion

        #region 공정 콤보 이벤트  : cboProcess_SelectedItemChanged()
        /// <summary>
        /// 공정 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                SetEquipment();

            }
        }
        #endregion

        #region 극성 콤보 이벤트 : cboElectrodeType_SelectedValueChanged()
        /// <summary>
        /// 극성 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            SetEquipment();
        }

        #endregion

        #region 조회 내용 색표시 : dgList_LoadedCellPresenter(), dgList_UnloadedCellPresenter()

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "REQ_TYPE")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "LR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "UR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "-"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            
                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "JOB_RESULT")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JOB_RESULT")), "OK"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        //else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JOB_RESULT")), "NG"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //}
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    if (Convert.ToString(e.Cell.Column.Name) == "JOB_DELAY_TIME")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISP_COLOR")), "R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #region Timer 콤보 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// Timer 콤보 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region Timer에 따라 조회  : _dispatcherTimer_Tick()
        /// <summary>
        /// Timer에 따라 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(null, null);
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
        #endregion

        #endregion

        #region Mehod

        #region 극성 조회 : SetElectrodeTypeCombo()
        /// <summary>
        /// 극성 참조 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region 공정 조회 : SetProcess()
        /// <summary>
        /// 공정
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetProcess(C1ComboBox cbo)
        {
            
            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "DDA_PROCESS", LoginInfo.CFG_SYSTEM_TYPE_CODE };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODEATTRS");
        }

        #endregion

        #region 설비 조회 : SetEquipment()
        /// <summary>
        /// 설비
        /// </summary>
        private void SetEquipment()
        {
            try
            {
                string processCode = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue == null ? null : cboElectrodeType.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count != 0)
                {
                    cboEquipment.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipment.Check(-1);
                    }
                    else
                    {
                        cboEquipment.isAllUsed = true;
                        cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipment.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 모니터링 데이터 조회 : GetDataList()
        /// <summary>
        ///  모니터링 데이터 조회
        /// </summary>
        private void GetDataList()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "BR_MCS_GET_DDA_MONITORING";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedItemsToString;

                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

               
                    //if(bizResult.Rows.Count > 0)
                    //{
                    //    for(int i=0; i<bizResult.Rows.Count; i++)
                    //    {
                    //        bizResult.Rows[i]["JOB_POS"] = "ELEC Lv0 CATHODE JUMBO ROLL STK #3 SHELF #100101";
                    //        bizResult.Rows[i]["EQPT_LOCID"] = "CATHODE ROL 6-2 UW PORT #01";
                    //    }
                      


                    //}
                    Util.GridSetData(dgList, bizResult, null, false);

                    if (bizResult.Rows.Count < 11)
                    {
                        dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                        //dgList.ColumnHeaderHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                        dgList.FontSize = 14;
                    }
                    else if (bizResult.Rows.Count > 6 && bizResult.Rows.Count < 15)
                    {
                        dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                        dgList.FontSize = 13;
                    }
                    else
                    {
                        dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                        dgList.FontSize = 12;
                    }



                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region ACS STATE 데이터 조회  : GetMonitoringData()
        public void GetMonitoringData()
        {
            const string bizRuleName = "BR_MCS_GET_DDA_STAT";
            try
            {
                ShowLoadingIndicator();
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                inDataTable.Rows.Add(newRow);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService_Multi(bizRuleName, "INDATA", "COT_RW_STAT,RP_UW_STAT,RP_RW_STAT,SLT_UW_STAT,BUFFER_STAT,RP_WO_STAT,SLT_WO_STAT,REPLACE_TRANSFER,ACS_STAT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //BUFFER_STAT
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["BUFFER_STAT"] != null)
                        {
                            Util.GridSetData(dgBuffer, bizResult.Tables["BUFFER_STAT"], null, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion
     
        #endregion

        #region Function

        #region 조회 Validation : ValidationSearch()

        private bool ValidationSearch()
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }
            return true;
        }

        #endregion

        #region 프로그래스 바  : ShowLoadingIndicator(), HiddenLoadingIndicator()

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

        #endregion

        #region 초기화  : ClearControl()

        private void ClearControl()
        {
            Util.gridClear(dgList);
        }

        #endregion
   
        #region MCS 비즈 접속 정보 : GetBizActorServerInfo()
        /// <summary>
        /// MCS 비즈 접속 정보
        /// </summary>
        private void GetBizActorServerInfo()
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

        }

        #endregion

        #region Timer 셋팅 : TimerSetting()
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_DDA" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }




        #endregion

        #endregion

        #region DDA 모니터링 이력 [C20221021-000451]

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            if (cboProcess2.SelectedValue == null || cboProcess2.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return;
            }

            if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 3)
            {
                Util.MessageValidation("SFU2042", "3");
                return;
            }

            GetDataListHist();
        }

        private void cboProcess2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess2.Items.Count > 0 && cboProcess2.SelectedValue != null)
            {
                SetEquipment2();

            }
        }

        private void dgListhist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "REQ_TYPE")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "LR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "UR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TYPE")), "-"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "JOB_RESULT")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JOB_RESULT")), "OK"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }
            }));
        }

        private void dgListhist_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void SetEquipment2()
        {
            try
            {
                string processCode = Util.GetCondition(cboProcess2);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment2.ItemsSource = null;
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = cboProcess2.SelectedValue.ToString();
                //dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue == null ? null : cboElectrodeType.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count != 0)
                {
                    cboEquipment2.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipment2.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipment2.Check(-1);
                    }
                    else
                    {
                        cboEquipment2.isAllUsed = true;
                        cboEquipment2.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipment2.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipment2.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDataListHist()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "DA_SEL_MCS_DDA_MONITORING_HIST_GUI";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment2.SelectedItemsToString;
                dr["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");

                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.gridClear(dgListhist);
                    Util.GridSetData(dgListhist, bizResult, this.FrameOperation);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}