/*************************************************************************************
 Created Date : 2023.07.12
      Creator : 강성묵
   Decription : 공정설비 반송요청 진행현황(통합)
--------------------------------------------------------------------------------------
 [Change History]
  2023.07.12  강성묵 : Initial Created.
  2023.07.21  강성묵 : 라인 ALL 조건 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_089 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private readonly Util _util = new Util();

        public MCS001_089()
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
        
        private void InitializeControl()
        {
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            // 라인 콥보박스
            SetEquipmentSegmentCombo(cboEquipmentSegment);
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);

            // 공정 콤보
            SetProcess(cboProcess);
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                SetEquipment();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControl();
            TimerSetting();
            Loaded -= UserControl_Loaded;

            _isLoaded = true;
        }
        #endregion

        #region Event
        
        #region 조회 버튼 : btnSearch_Click()
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationSearch() == false)
            {
                return;
            }

            GetDataList();
        }
        #endregion

        #region cboEquipmentSegment_SelectedValueChanged()
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            //if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null)
            //{
            SetProcess(cboProcess);
            //}
        }
        #endregion

        #region  공정 콤보 이벤트  : cboProcess_SelectedItemChanged()
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                SetEquipment();
            }
        }
        #endregion

        #region 극성 콤보 이벤트 : cboElectrodeType_SelectedValueChanged()
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
            {
                return;
            }

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
            if (sender == null)
            {
                return;
            }

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
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded)
            {
                return;
            }

            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int iSecond = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        iSecond = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (iSecond == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, iSecond);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                        {
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
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

        #region Timer에 따라 조회  : _dispatcherTimer_Tick()
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1)
                    {
                        return;
                    }

                    btnSearch_Click(null, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                    {
                        dpcTmr.Start();
                    }
                }
            }));
        }
        #endregion

        #region 모니터링 리스트 화면 머지  : dgList_MergingCells()
        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                // 설비기준으로 머지
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
                            {
                                bStrt = false;
                            }
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
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EQPTNAME"].Index)));

                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOIFMODE_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOIFMODE_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOSTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOSTNAME"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EQPTNAME"].Index)));

                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOIFMODE_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOIFMODE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["EIOSTNAME"].Index), dgList.GetCell(idxE, dgList.Columns["EIOSTNAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                {
                                    bStrt = false;
                                }
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
                            {
                                bStrt = false;
                            }
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
                                {
                                    bStrt = false;
                                }
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

        #region Method

        #region 라인 조회 : SetEquipmentSegmentCombo()
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            string sBizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string sSelectedValueText = "CBO_CODE";
            string sDisplayMemberText = "CBO_NAME";

            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            CommonCombo.CommonBaseCombo(sBizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, sSelectedValueText, sDisplayMemberText, LoginInfo.CFG_EQSG_ID);
        }
        #endregion

        #region 극성 조회 : SetElectrodeTypeCombo()
        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            string sBizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string sSelectedValueText = cbo.SelectedValuePath;
            string sDisplayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(sBizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, sSelectedValueText, sDisplayMemberText, string.Empty);
        }
        #endregion

        #region 공정 조회 : SetProcess()
        private void SetProcess(C1ComboBox cbo)
        {
            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            //string sBizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string sBizRuleName = "DA_BAS_SEL_PROCESS_BY_AREA_CBO";

            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString() };

            string sSelectedValueText = "CBO_CODE";
            string sDisplayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(sBizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, sSelectedValueText, sDisplayMemberText, LoginInfo.CFG_PROC_ID);
        }
        #endregion

        #region 설비 조회 : SetEquipment()
        private void SetEquipment()
        {
            try
            {
                DataTable dtRQST = new DataTable();
                dtRQST.TableName = "RQSTDT";
                dtRQST.Columns.Add("LANGID", typeof(string));
                dtRQST.Columns.Add("AREAID", typeof(string));
                dtRQST.Columns.Add("EQSGID", typeof(string));
                dtRQST.Columns.Add("PROCID", typeof(string));
                dtRQST.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drRQST = dtRQST.NewRow();
                drRQST["LANGID"] = LoginInfo.LANGID;
                drRQST["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (cboEquipmentSegment.SelectedValue != null)
                {
                    drRQST["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                }

                if (cboProcess.SelectedValue != null)
                {
                    drRQST["PROCID"] = cboProcess.SelectedValue.ToString();
                }

                if (cboElectrodeType.SelectedValue != null)
                {
                    drRQST["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue.ToString();
                }

                dtRQST.Rows.Add(drRQST);

                string sBizRuleName = "";
                if(LoginInfo.CFG_SYSTEM_TYPE_CODE == "E")
                {
                    // 전극동 Main 조회
                    sBizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_ELEC_ASSY_CBO";
                }
                else
                {
                    // 조립동 Machine 조회
                    sBizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ASSY_LOADER_CBO";
                }
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "RQSTDT", "RSLTDT", dtRQST);
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
        private void GetDataList()
        {
            ShowLoadingIndicator();

            string bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_ELEC_ASSY";
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

                    //if (bizResult.Rows.Count < 11)
                    //{
                    //    dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                    //    dgList.FontSize = 14;
                    //}
                    //else if (bizResult.Rows.Count > 6 && bizResult.Rows.Count < 15)
                    //{
                    //    dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                    //    dgList.FontSize = 13;
                    //}
                    //else
                    //{
                    //dgList.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                    //dgList.FontSize = 12;
                    //}

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

        #region ValidationSearch()
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

        #region ShowLoadingIndicator()
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region HiddenLoadingIndicator()
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        
        #region 초기화  : ClearControl()
        private void ClearControl()
        {
            Util.gridClear(dgList);
        }
        #endregion

        #endregion

        #region Timer 셋팅 : TimerSetting()
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_DDA" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
            {
                cboTimer.SelectedIndex = 3;
            }

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                {
                    second = int.Parse(cboTimer.SelectedValue.ToString());
                }

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }
        #endregion

        #endregion

    }
}