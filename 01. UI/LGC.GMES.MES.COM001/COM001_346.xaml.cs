/*************************************************************************************
 Created Date : 2020.12.01
      Creator : 오화백
   Decription : NND 투입 대기 재고 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.01  오화백 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_346.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_346 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;



        #endregion

        #region Initialize
        /// <summary>
        /// 생성자
        /// </summary>
        public COM001_346()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IsInitialized가 true설정될 때마다 호출되는 메소드. override하여 base의 것도 호출되도록 함
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        /// <summary>
        /// 화면로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControl();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 요소 기본 세팅
        /// </summary>
        private void InitializeControl()
        {
            InitializeCombo();
        }

        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitializeCombo()
        {
            // 동 콤보박스
            SetAreaCombo(cboArea);
        }

        /// <summary>
        /// 타이머 셋팅
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            //default 60초 세팅
            if (cboTimer != null && cboTimer.Items.Count > 0)
            {
                for(int i= cboTimer.Items.Count-1; i >= 0; i--)
                {
                    DataRowView item = cboTimer.Items[i] as DataRowView;
                    if(Util.NVC_Int(DataTableConverter.GetValue(item, "CBO_CODE")) == 60)
                    {
                        cboTimer.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                if(second > 0)
                {
                    _monitorTimer.Start();
                }
            }
        }

        /// <summary>
        /// 동정보 가져오는 콤보박스 내용 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetAreaCombo(C1ComboBox cbo)
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_BLDG_CODE_CBO", "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (LoginInfo.CFG_AREA_ID == ((DataRowView)cbo.Items[i]).Row.ItemArray[3].ToString())
                {
                    cbo.SelectedIndex = i;
                    break;
                }
            }
        }

        #endregion

        #region Event

        #region 조회버튼   [btnSearch_Click()]
        /// <summary>
        /// 조회버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            ClearControl();
            SelectNNDInputDataList();
        }
        #endregion

        #region 타이머 콤보이벤트 [cboTimer_SelectedValueChanged()]
        /// <summary>
        /// 타이머 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;

            if (string.IsNullOrEmpty(cboArea.SelectedValue.ToString()))
            {
                //동을 선택해주세요
                Util.MessageValidation("SFU3206");
                return;
            }

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
                        //자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        //자동조회  %1초로 변경 되었습니다.
                        if (cboTimer != null)
                            Util.MessageInfo("SFU5127", cboTimer.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region NND 투입 대기 가용 스프레드 이벤트  [dgStatusbyWorkorder_LoadedCellPresenter(), dgStatusbyWorkorder_UnloadedCellPresenter()]
        /// <summary>
        /// 화면 로딩후 색깔 셋팅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStatusbyWorkorder_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "EQSGNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME") || string.Equals(e.Cell.Column.Name, "COATING_LINE"))
                {
                    return;
                }
                else
                {
                    if (e.Cell.Row.DataItem == null) return;

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 0 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 6)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        /// <summary>
        /// 스크롤 내릴때  색깔 오류 대비 강제적으로 색지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStatusbyWorkorder_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region Storage Rate 스프레드 이벤트 [dgStorageRate_LoadedCellPresenter(), dgStorageRate_UnloadedCellPresenter()]
        /// <summary>
        /// 색깔 셋팅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStorageRate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "ELTR_TYPE_CODE") || string.Equals(e.Cell.Column.Name, "EQPTNAME"))
                {
                    return;
                }
                else
                {
                    if (e.Cell.Row.DataItem == null) return;

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 90)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 85 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 90)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        /// <summary>
        /// 스크롤 내릴때  색깔 오류 대비 강제적으로 색지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStorageRate_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region 타이머에 따른 조회 [_dispatcherTimer_Tick()]
        /// <summary>
        /// 타이머에 따른 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            if (string.IsNullOrEmpty(cboArea.SelectedValue.ToString()))
            {
                //Util.MessageValidation("설비유형을 선택해 주세요.");
                Util.MessageValidation("SFU3206");
                return;
            }


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

        #region Method

        #region Empty Carrier List  [SelectEmptyCarrierList]
        /// <summary>
        /// Empty Carrier List
        /// </summary>
        private void SelectEmptyCarrierList()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_SUMMARY";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);

                    SelectStorageRateList();

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK Storage Rate List  [SelectStorageRateList()]
        /// <summary>
        /// STK Storage Rate List
        /// </summary>
        private void SelectStorageRateList()
        {

            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_RATE_ELEC";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgStorageRate, bizResult, null, true);

                    HiddenLoadingIndicator();

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region NND 투입 대기 가용 재고 현황  [SelectNNDInputDataList()]
        /// <summary>
        /// NND 투입 대기 가용 재고 현황
        /// </summary>
        private void SelectNNDInputDataList()
        {
            ShowLoadingIndicator();
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_ELEC_NND";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                inTable.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("DISPLAY", typeof(bool));
                    Util.GridSetData(dgStatusbyWorkorder, bizResult, null, true);
                    SelectEmptyCarrierList();

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 초기화 [ClearControl()]
        /// <summary>
        /// 초기화
        /// </summary>
        private void ClearControl()
        {
            Util.gridClear(dgStatusbyWorkorder);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgStorageRate);
        }


        #endregion

        #region 조회 Validation  [ValidationSearch()]
        /// <summary>
        /// 조회 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationSearch()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue == null)
            {
                //동을 선택해주세요
                Util.MessageValidation("SFU3206");
                return false;
            }

            return true;
        }
        #endregion

        #region 프로그래스 바 셋팅 [ShowLoadingIndicator(),HiddenLoadingIndicator()]
        /// <summary>
        /// 프로그래스바 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 프로그래스바 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion

        private void dgStatusbyWorkorder_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            //pjt와 pancake을 합쳐야 함
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.ItemsSource == null || dg.Rows == null || dg.Rows.Count == dg.TopRows.Count)
                {
                    return;
                }

                int startIdx = dg.TopRows.Count;
                int prevIdx = startIdx;
                int currIdx = 0;
                String prevPrjtName = Util.NVC(DataTableConverter.GetValue(dg.Rows[prevIdx].DataItem, "PRJT_NAME"));

                for (currIdx = startIdx + 1; currIdx <= dg.Rows.Count - dg.BottomRows.Count - 1; currIdx++)
                {
                    String currPrjtName = Util.NVC(DataTableConverter.GetValue(dg.Rows[currIdx].DataItem, "PRJT_NAME"));
                    if (!prevPrjtName.Equals(currPrjtName))
                    {
                        //prevIdx에서 i-1까지 merge
                        e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(currIdx - 1, dg.Columns["PRJT_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["AN_QTY_EL_ALL"].Index), dg.GetCell(currIdx - 1, dg.Columns["AN_QTY_EL_ALL"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["CA_QTY_EL_ALL"].Index), dg.GetCell(currIdx - 1, dg.Columns["CA_QTY_EL_ALL"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["AN_QTY_EL"].Index), dg.GetCell(currIdx - 1, dg.Columns["AN_QTY_EL"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["CA_QTY_EL"].Index), dg.GetCell(currIdx - 1, dg.Columns["CA_QTY_EL"].Index)));
                        prevIdx = currIdx;
                        prevPrjtName = currPrjtName;
                    }
                }
                e.Merge(new DataGridCellsRange(dg.GetCell(prevIdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(currIdx - 1, dg.Columns["PRJT_NAME"].Index)));
                //다 돌고 나서 prevIdx랑 i랑 병합하고 끝
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            //{
            //    if (dgInput != null && dgInput.Rows.Count > 0 && dgInput.Columns.Contains("EQPT_MOUNT_PSTN_NAME"))
            //    {
            //        e.Merge(new DataGridCellsRange(dgInput.GetCell(0, dgInput.Columns["EQPT_MOUNT_PSTN_NAME"].Index), dgInput.GetCell(dgInput.Rows.Count - 1, dgInput.Columns["EQPT_MOUNT_PSTN_NAME"].Index)));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }
    }
}
