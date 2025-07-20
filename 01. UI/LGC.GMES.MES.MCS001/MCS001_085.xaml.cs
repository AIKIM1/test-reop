/*************************************************************************************
 Created Date : 2022.08.30
      Creator : 오화백
   Decription : 공정설비 반송요청 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.30  오화백 : Initial Created.
  2023.06.07  오화백 : Carrier ID 마지막 위치, 마지막 위치명 컬럼 추가   
  2023.09.08  주동석 : 슬리팅 공정일 때 대표 LOT 로 조회될수 있도록 추가
  2023,10.10  김태우 : dic용어 변경. REP_LOTID
  2024.05.31  배현우 : 컬럼 사용자 설정 기능 추가 및 조회 컬럼 사이즈 고정 (스크롤시 컬럼 사이즈 변경되어 고정요청, 대표랏 사용동) 
  2024.12.12  오화백 : NFF WMS 전환으로 NFF 일경우 설비 컬럼 숨김처리
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
    public partial class MCS001_085 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private bool _LotConf = false;
        private bool RepLotUseArea = false;
        public MCS001_085()
        {
          
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Columns_Hide();
            InitializeCombo();
            InitializeControls();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
            GetRepLotUseArea();
            if(RepLotUseArea)
                ColumnSizeInit();
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

        #region 공정 콤보 이벤트  : cboProcess_SelectedItemChanged()
        /// <summary>
        /// 공정 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    if (cboProcess.SelectedValue.ToString() == dtCommon.Rows[0]["ATTRIBUTE1"].ToString())
                    {
                        _LotConf = true;
                        dgList.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("REP_LOTID");
                    }
                    else
                    {
                        _LotConf = false;
                        dgList.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                    }
                }
            }

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
                    if (Convert.ToString(e.Cell.Column.Name) == "PORT_STAT_CODE")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "LR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "UR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "PL"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "-"))
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

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_REQ")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "R"))
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

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_TRF")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "R"))
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

        #region 모니터링 리스트 화면 머지  : dgList_MergingCells()
        /// <summary>
        /// LIST 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //설비기준으로 머지
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count; i++)
                {

                    if (dgList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQPTDESC"].Index), dgList.GetCell(idxE, dgList.Columns["EQPTDESC"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["PRJT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["DEMAND_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["DEMAND_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOIFMODE_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOIFMODE_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOSTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOSTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQP_DIRCTN"].Index), dgList.GetCell(idxE, dgList.Columns["EQP_DIRCTN"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQPTDESC"].Index), dgList.GetCell(idxE, dgList.Columns["EQPTDESC"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["PRJT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["DEMAND_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["DEMAND_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOIFMODE_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOIFMODE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOSTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOSTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQP_DIRCTN"].Index), dgList.GetCell(idxE, dgList.Columns["EQP_DIRCTN"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }

                // 포트 기준으로 머지
                idxS = 0;
                idxE = 0;
                bStrt = false;
                sTmpLvCd = string.Empty;
                sTmpTOTALQTY = string.Empty;

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count; i++)
                {

                    if (dgList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PORT_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PORT_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_ID"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_STAT_CODE"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_STAT_CODE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TRF_PROC_STATUS"].Index), dgList.GetCell(idxE, dgList.Columns["TRF_PROC_STATUS"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CSTID"].Index), dgList.GetCell(idxE, dgList.Columns["CSTID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CSTSTAT"].Index), dgList.GetCell(idxE, dgList.Columns["CSTSTAT"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CURR_LOTID"].Index), dgList.GetCell(idxE, dgList.Columns["CURR_LOTID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["LOT_DIRCTN"].Index), dgList.GetCell(idxE, dgList.Columns["LOT_DIRCTN"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EMPTY_RSLT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EMPTY_RSLT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TIME_OF_TRF"].Index), dgList.GetCell(idxE, dgList.Columns["TIME_OF_TRF"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TIME_OF_REQ"].Index), dgList.GetCell(idxE, dgList.Columns["TIME_OF_REQ"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["REQ_DTTM"].Index), dgList.GetCell(idxE, dgList.Columns["REQ_DTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CMD_CR_DTTM"].Index), dgList.GetCell(idxE, dgList.Columns["CMD_CR_DTTM"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_ID"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["PORT_STAT_CODE"].Index), dgList.GetCell(idxE, dgList.Columns["PORT_STAT_CODE"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TRF_PROC_STATUS"].Index), dgList.GetCell(idxE, dgList.Columns["TRF_PROC_STATUS"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CSTID"].Index), dgList.GetCell(idxE, dgList.Columns["CSTID"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CSTSTAT"].Index), dgList.GetCell(idxE, dgList.Columns["CSTSTAT"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CURR_LOTID"].Index), dgList.GetCell(idxE, dgList.Columns["CURR_LOTID"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["LOT_DIRCTN"].Index), dgList.GetCell(idxE, dgList.Columns["LOT_DIRCTN"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EMPTY_RSLT_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EMPTY_RSLT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TIME_OF_TRF"].Index), dgList.GetCell(idxE, dgList.Columns["TIME_OF_TRF"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["TIME_OF_REQ"].Index), dgList.GetCell(idxE, dgList.Columns["TIME_OF_REQ"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["REQ_DTTM"].Index), dgList.GetCell(idxE, dgList.Columns["REQ_DTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["CMD_CR_DTTM"].Index), dgList.GetCell(idxE, dgList.Columns["CMD_CR_DTTM"].Index)));


                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PORT_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }





            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //믹스 공정일 경우 하위 공정의 설비 전체 정보 가져오기
                if (cboProcess.SelectedValue.ToString() == "E1000")
                {
                    dr["PROCID"] = "E0400,E0410,E0430,E0500,E1000";
                }
                else
                {
                    dr["PROCID"] = cboProcess.SelectedValue.ToString();
                }
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue == null ? null : cboElectrodeType.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_ELEC_ASSY_CBO", "RQSTDT", "RSLTDT", RQSTDT);
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

            string bizRuleName = string.Empty;

            if (_LotConf == true)
                bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LOT_CONF_LIST";
            else
                bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST";

            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedItemsToString;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {

                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, false);

                    if (bizResult.Rows.Count < 11)
                    {
                        dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
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

                    dgList.MergingCells -= dgList_MergingCells;
                    dgList.MergingCells += dgList_MergingCells;

                });



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

        private void ColumnSizeInit()
        {
            //NFF 양산라인의 경우 컬럼 사이즈 고정 요청 (스크롤시 컬럼크기가 변경되어 사이즈 고정)
            dgList.Columns["EQPTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["EQPTDESC"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["PRJT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList.Columns["DEMAND_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList.Columns["EIOIFMODE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList.Columns["EIOSTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList.Columns["EQP_DIRCTN"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["PORT_ID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["PORT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["PORT_STAT_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList.Columns["TRF_PROC_STATUS"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["CSTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["CSTSTAT"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["CURR_LOCID"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["CURR_LOC_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["CURR_LOTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["LOT_DIRCTN"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList.Columns["EMPTY_RSLT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(300);
            dgList.Columns["TIME_OF_TRF"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["TIME_OF_REQ"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["REQ_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["CMD_CR_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["STO_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["STO_EIOSTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList.Columns["STO_EIOIFMODE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);


        }

        private void GetRepLotUseArea()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    RepLotUseArea = true;
                }
                else
                {
                    RepLotUseArea = false;
                }
            }
        }


        private void Columns_Hide()
        {
            try
            {
                if(LoginInfo.CFG_AREA_ID == "ER")
                {
                    dgList.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DEMAND_TYPE"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DEMAND_NAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["EIOIFMODE"].Visibility = Visibility.Collapsed;
                    dgList.Columns["EIOIFMODE_NAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["EIOSTAT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["EIOSTNAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["EQP_DIRCTN"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgList.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
                    dgList.Columns["DEMAND_TYPE"].Visibility = Visibility.Visible;
                    dgList.Columns["DEMAND_NAME"].Visibility = Visibility.Visible;
                    dgList.Columns["EIOIFMODE"].Visibility = Visibility.Visible;
                    dgList.Columns["EIOIFMODE_NAME"].Visibility = Visibility.Visible;
                    dgList.Columns["EIOSTAT"].Visibility = Visibility.Visible;
                    dgList.Columns["EIOSTNAME"].Visibility = Visibility.Visible;
                    dgList.Columns["EQP_DIRCTN"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


    }
}