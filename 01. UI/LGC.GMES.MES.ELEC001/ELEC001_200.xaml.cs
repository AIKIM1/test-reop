/*************************************************************************************
 Created Date : 2019.08.10
      Creator : 이동우S
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2019. 08. 20  오화백  화면 개발
 2019. 09. 24  오화백  러시아어로 변환시 숫자가 나오지 않는 부분 수정
 2020. 07. 22  오화백  바인더, CMC, 선분산, WETTING 믹서에도 확산 적용하기 위하여 공정팝업 추가
 2022. 10. 11  정재홍  [C20220919-000515] - 공정진척 특이사항 추가
 2023. 08. 10  연현정  [ESHM] 슬러리 이송탱크 저장 시, RPT_ITEM 하드코딩 제거. (HM 은 SLURRY_MOVE 가 아닌 SLURRY_MOVE_01, SLURRY_MOVE_02 사용중) 
 2023. 10. 13  양영재  [E20230907-001465] - 현재 믹싱 중인 배치 작업에 대해 특이사항 내용 저장 활성화 및 믹싱 차주 정보값 자동 입력
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Documents;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.UserControls;
using System.Globalization;



namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_200 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        // 모니터링용 타이머 관련
        private System.Windows.Threading.DispatcherTimer _monitorTimer = new System.Windows.Threading.DispatcherTimer();
        //private string refeshAfter = "{0}";
        private bool _isSetAutoSelectTime = false;

        string _BatchID = string.Empty;
        string _EqptId = string.Empty;
        string _Stat = string.Empty;
        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        private Util _Util = new Util();
        public ELEC001_200()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            TimerSetting();

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            InitCombo();
            //TimerSetting();
            this.Loaded -= UserControl_Loaded;

        }
        private void InitCombo()
        {

            ProcessComb();
            EqptComb(cboProcess.SelectedValue.ToString());
            if(_EqptId != "") // 2024.10.14. 김영국 - eqpid가 없는 경우는 Hopper를 조회하지 않는다.
                HopperComb();

        }

        /// <summary>
        /// 공정정보
        /// </summary>
        public void ProcessComb()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_CWA_MIXING", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";
                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProcess.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 설비정보
        /// </summary>
        public void EqptComb(string process)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = process;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";
                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);

                cboEquipment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 호퍼 콤보 조회 호출
        /// </summary>
        public void HopperComb()
        {
            //호퍼정보
            C1.WPF.DataGrid.DataGridComboBoxColumn HopperColumn = dgMtrlGrdReady.Columns["RPT_ITEM_VALUE07"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (HopperColumn != null)
                HopperColumn.ItemsSource = DataTableConverter.Convert(dtHopperInfo(cboEquipment.SelectedValue.ToString()));

        }

        /// <summary>
        /// 호퍼 콤보 조회 
        /// </summary>
        /// <param name="Eqsgid"></param>
        /// <returns></returns>
        DataTable dtHopperInfo(string Eqsgid)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQPTID"] = _EqptId;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIX_HOPPER_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            DataRow drIns = dtResult.NewRow();
            drIns["CBO_NAME"] = "-ALL-";
            drIns["CBO_CODE"] = "";
            dtResult.Rows.InsertAt(drIns, 0);

            return dtResult;
        }


        #endregion

        #region Event

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            if (txtBatchID.Text.Trim() == string.Empty)
            {
                if (cboEquipment.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU1673"); //설비정보를 선택하세요
                    return;
                }
            }
            Clear();
            GetBatchInfo();
        }

        #endregion

        #region 배치ID로 조회 : txtBatchID_KeyDown()
        /// <summary>
        /// 배치ID로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBatchID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtBatchID.Text)) return;

                Clear();
                GetBatchInfo();
            }

        }
        /// <summary>
        /// 포커스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBatchID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        #endregion

        #region 현재진행배치 조회 : btnNowBatch_Click()

        /// <summary>
        /// 현재진행배치
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNowBatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipment.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU1673"); //설비정보를 선택하세요
                    Util.gridClear(dgBatchInfo);
                    Clear();

                    return;
                }
                Clear();
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_PROC_BATCH";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.

                        return;
                    }
                    else
                    {
                        //문서 정보 조회
                        GetDocInfo(bizResult.Rows[0]["LOTID"].ToString());

                        ////원재료 계량
                        //GetRptCheck(bizResult.Rows[0]["LOTID"].ToString());

                        //믹싱
                        GetMixing(bizResult.Rows[0]["LOTID"].ToString());

                        //자주검사
                        GetSelfInsp(bizResult.Rows[0]["LOTID"].ToString());

                        //체크 스케일
                        GetCheckScale(bizResult.Rows[0]["LOTID"].ToString());

                        //체크 스케일 배출
                        GetCheckScaleOut(bizResult.Rows[0]["LOTID"].ToString());

                        //총 투입량
                        GetAllInputQty(bizResult.Rows[0]["LOTID"].ToString());

                        //슬러리 이송
                        GetSlurryMove(bizResult.Rows[0]["LOTID"].ToString());

                        //공정진척 특이사항
                        GetWipRemark(bizResult.Rows[0]["LOTID"].ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 좌측축소 : btnLeftExpandFrame_Checked()
        /// <summary>
        ///  좌측축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            grid1.ColumnDefinitions[0].Width = new GridLength(260);
            grid1.ColumnDefinitions[1].Width = new GridLength(4);

        }
        #endregion 

        #region 좌측확장 : btnLeftExpandFrame_Unchecked()
        /// <summary>
        /// 좌측확장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            // 확장
            grid1.ColumnDefinitions[0].Width = new GridLength(0);
            grid1.ColumnDefinitions[1].Width = new GridLength(0);
        }

        #endregion

        #region Batch ID 선택 : dgBatchInfo_MouseDoubleClick()
        /// <summary>
        /// 배치ID 더블클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgBatchInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBatchInfo.GetCellFromPoint(pnt);

                //초기화
                Clear();
                _Stat = Util.NVC(DataTableConverter.GetValue(dgBatchInfo.Rows[cell.Row.Index].DataItem, "WIPSTAT"));
                _BatchID = Util.NVC(DataTableConverter.GetValue(dgBatchInfo.Rows[cell.Row.Index].DataItem, "BATCH_ID"));
                _EqptId = Util.NVC(DataTableConverter.GetValue(dgBatchInfo.Rows[cell.Row.Index].DataItem, "EQPTID"));
                GetAll_Item(_BatchID, _EqptId);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        #endregion

        #region 배치ID 정보  - 그리드 내 색깔 설정 : dgBatchInfo_LoadedCellPresenter(), dgBatchInfo_UnloadedCellPresenter()
        /// <summary>
        /// 그리드 내 글자 색 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgBatchInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgBatchInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("BATCH_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                }

            }));
        }
        /// <summary>
        /// 스크롤 이동시 글자색 유지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgBatchInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //link 색변경
                        if (e.Cell.Column.Name.Equals("BATCH_ID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }
            }));
        }


        #endregion

        #region 문서정보 - 현 배치/계획 저장 : btnSaveBatchCount_Click()
        /// <summary>
        /// 현 배치/계획 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveBatchCount_Click(object sender, RoutedEventArgs e)
        {

            if (dgDocument.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905");
                return;
            }

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveMixRptBatch();
                }
            });
        }
        #endregion

        #region 문서정보 - 그리드 설정 : dgDocument_LoadedCellPresenter()

        /// <summary>
        /// 현 배치/계획 컬럼 색변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDocument_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Equals("BATCH_COUNT"))
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["BATCH_COUNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 원재료 계량 - 합계 Row 색깔 설정 : dgMtrlGrd_LoadedCellPresenter(), dgMtrlGrd_UnloadedCellPresenter()
        /// <summary>
        /// 그리드 내 배경색 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlGrd_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgBatchInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RPT_ITEM_VALUE02")) == ObjectDic.Instance.GetObjectName("총합계"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#e6b387"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }

            }));
        }
        /// <summary>
        /// 그리드 내 배경색 유지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlGrd_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Background = null;

                }
            }
        }

        #endregion

        #region 믹싱  - 합계 Row 색깔 설정 : dgMix_LoadedCellPresenter(),  dgMix_UnloadedCellPresenter()
        /// <summary>
        /// 그리드 내 배경색 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMix_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgBatchInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RPT_ITEM_VALUE01"))))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#e6b387"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }

            }));
        }
        /// <summary>
        /// 그리드 내 배경색 유지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMix_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Background = null;

                }
            }
        }

        #endregion

        #region 원자재 재료 - 자재정보요청 : btnReqMtrl_Click()
        /// <summary>
        /// 자재 정보 요청
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReqMtrl_Click(object sender, RoutedEventArgs e)
        {
            GetMtrlGrdReady_Mtrl();
        }

        #endregion

        #region 원자재 재료 - 저장 : btnSaveGrdReady_Click()
        /// <summary>
        /// 원자재 재료 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveGrdReady_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation_GrdReady()) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveGrdReady();
                }
            });

        }
        #endregion

        #region 원자재 재료 - 그리드 설정 : DataGridTemplateColumn_GettingCellValue(), dgMtrlGrdReady_LoadedCellPresenter()
        /// <summary>
        /// 원자재 재료 - 그리드 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.DataGridTemplateColumn dgtc = sender as C1.WPF.DataGrid.DataGridTemplateColumn;
                System.Data.DataRowView drv = e.Row.DataItem as System.Data.DataRowView;

                if (dgtc != null && drv != null && dgtc.Name != null)
                {
                    e.Value = drv[dgtc.Name].ToString();
                }
            }
            catch (Exception ex)
            {
                //오류 발생할 경우 아무 동작도 하지 않음. try catch 없으면 이 로직에서 오류날 경우 복사 자체가 안됨
            }
        }

        /// <summary>
        /// 배경색 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlGrdReady_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE03"))
                    {
                        if (e.Cell.Presenter != null)
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE03"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE04"))
                    {
                        if (e.Cell.Presenter != null)
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE04"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE05"))
                    {
                        if (e.Cell.Presenter != null)
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE05"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE06"))
                    {
                        if (e.Cell.Presenter != null)
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE06"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE07"))
                    {
                        if (e.Cell.Presenter != null)
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE07"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 슬러리 이송 - 이송시간 : btnTime_Click()
        /// <summary>
        /// 슬러리 이송 - 이송시간
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTime_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtTmp = DataTableConverter.Convert(dgSlurry.ItemsSource);

            if (dtTmp.Rows.Count > 0)
            {
                for (int i = 0; i < dtTmp.Rows.Count; i++)
                {
                    if (dtTmp.Rows[i]["RPT_ITEM_VALUE01"].ToString() == ObjectDic.Instance.GetObjectName("이송탱크")
                        //2023.08.10 [ESHM] SLURRY_MOVE_01, SLURRY_MOVE_02 로 저장되어야 함. RPT_ITEM 이 세분화됨
                        || (dtTmp.Columns.Contains("ORG_RPT_ITEM_VALUE01") && Util.NVC(dtTmp.Rows[i]["ORG_RPT_ITEM_VALUE01"]).Equals("MOVE_TANK"))
                        )
                    {
                        dtTmp.Rows[i]["RPT_ITEM_VALUE02"] = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
                    }
                }
                Util.GridSetData(dgSlurry, dtTmp, FrameOperation, true);

            }


        }

        #endregion

        #region 슬러리 이송 - 저장 : btnSaveSlurry_Click()
        /// <summary>
        /// 슬러리 이송 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveSlurry_Click(object sender, RoutedEventArgs e)
        {

            if (dgSlurry.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905");
                return;
            }
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSlurryMove();
                }
            });


        }
        #endregion

        #region 슬러리 이송 - 그리드 설정 : dgSlurry_BeginningEdit(), dgSlurry_LoadedCellPresenter()

        /// <summary>
        /// 저장탱크 Row의 시작시간은 수정 불가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSlurry_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                string sTank = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RPT_ITEM_VALUE01"));
                string sOrgTank = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ORG_RPT_ITEM_VALUE01"));

                if (sTank == ObjectDic.Instance.GetObjectName("저장탱크")
                    //2023.08.10 [ESHM] SLURRY_MOVE_01, SLURRY_MOVE_02 로 저장되어야 함. RPT_ITEM 이 세분화됨
                    || sOrgTank.Equals("SAVE_TANK")
                    )
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 슬러리 이송 수정가능 컬럼 색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSlurry_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Equals("RPT_ITEM_VALUE02"))
                    {
                        string sTank = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RPT_ITEM_VALUE01"));
                        string sOrgTank = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ORG_RPT_ITEM_VALUE01"));

                        if (e.Cell.Presenter != null &&
                          (sTank.Equals(ObjectDic.Instance.GetObjectName("이송탱크"))
                          //2023.08.10 [ESHM] SLURRY_MOVE_01, SLURRY_MOVE_02 로 저장되어야 함. RPT_ITEM 이 세분화됨
                          || sOrgTank.Equals("MOVE_TANK"))
                          )
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["RPT_ITEM_VALUE02"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 특이사항 - 저장 : btnSaveRemark_Click()

        /// <summary>
        /// 특이사항 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            string remark = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text.Trim(); // [E20230907-001465] - 현재 믹싱 중인 배치 작업에 대해 특이사항 내용 저장 활성화 및 믹싱 차주 정보값 자동 입력
            if (remark.Length == 0)  //if (dgDocument.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905");
                return;
            }
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveRemark();
                }
            });

        }

        #endregion

        #region 인쇄 - 저장 : btnPrint_Click()
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PopupPrint();
        }

        #endregion

        #region 자동리셋 콤보박스 이벤트 : cboReset_SelectedValueChanged()
        private void cboReset_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboReset?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboReset.SelectedValue.ToString());
                        _isSetAutoSelectTime = true;
                    }
                    else
                    {
                        _isSetAutoSelectTime = false;
                    }


                    if (second == 0 && _isSetAutoSelectTime)
                    {
                        Util.MessageValidation("SFU1606");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSetAutoSelectTime)
                    {
                        //자동조회  %1초로 변경 되었습니다.
                        if (cboReset != null)
                            Util.MessageInfo("SFU5127", cboReset.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공정변경시 설비정보 조회 : cboProcess_SelectedValueChanged()
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Clear();
            //배치정보 정보 초기화
            Util.gridClear(dgBatchInfo);
            EqptComb(cboProcess.SelectedValue.ToString());
        }
        #endregion

        #region 설비변경시 초기화 : cboEquipment_SelectedValueChanged()
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //배치정보 정보 초기화
            Util.gridClear(dgBatchInfo);
            Clear();
        }
        #endregion

        #endregion

        #region Mehod

        #region 배치ID 정보 조회 : GetBatchInfo()
        /// <summary>
        /// 배치ID 정보 조회
        /// </summary>
        public void GetBatchInfo()
        {
            // [E20230907-001465] - 현재 믹싱 중인 배치 작업에 대해 특이사항 내용 저장 활성화 및 믹싱 차주 정보값 자동 입력
            string[] wip_note = null;
            int next_batch = 0;
            double next_batch_d = 0.0;
            string plan = string.Empty;
            bool is_double = false;

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_BATCH_INFO";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("BATCH_ID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("DATE_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();

                if (txtBatchID.Text.Trim() == string.Empty)
                {
                    dr["DATE_FR"] = Util.GetCondition(dtpDateFrom);
                    dr["DATE_TO"] = Util.GetCondition(dtpDateTo);
                    dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    dr["PJT"] = txtPjt.Text;
                    dr["DATE_YN"] = "Y";
                }
                else
                {
                    dr["BATCH_ID"] = txtBatchID.Text;
                    dr["DATE_YN"] = null;
                }

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgBatchInfo.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgBatchInfo.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

                   // [E20230907-001465] - 현재 믹싱 중인 배치 작업에 대해 특이사항 내용 저장 활성화 및 믹싱 차주 정보값 자동 입력
                   // if (bizResult.Rows.Count > 1)
                   // {
                   //     for (int i = 0; i < bizResult.Rows.Count; i++)
                   //     {
                   //         if (i == bizResult.Rows.Count - 2)
                   //         {
                   //             string batch_plan = bizResult.Rows[i]["WIP_NOTE"].ToString();
                   // 
                   //             wip_note = batch_plan.Split('/');
                   //             if(double.TryParse(wip_note[0], out next_batch_d))
                   //             {
                   //                 next_batch_d += 1.0;
                   //                 is_double = true;
                   //             }
                   //             if (int.TryParse(wip_note[0], out next_batch))
                   //             {
                   //                 next_batch = int.Parse(wip_note[0]) + 1;
                   //             }
                   //             plan = wip_note[1];
                   //         }
                   //         else if (i == bizResult.Rows.Count - 1 && bizResult.Rows[i]["WIP_NOTE"].ToString().IsNullOrEmpty())
                   //         {
                   //             if (bizResult.Rows[i]["WIP_NOTE"].IsNullOrEmpty() && is_double)
                   //             {
                   //                 bizResult.Rows[i]["WIP_NOTE"] = Convert.ToString(next_batch_d) + "/" + wip_note[1];
                   //             }
                   //             else
                   //             {
                   //                 bizResult.Rows[i]["WIP_NOTE"] = Convert.ToString(next_batch) + "/" + wip_note[1];
                   //             }
                   //         }
                   //     }
                   // }
                    Util.GridSetData(dgBatchInfo, bizResult, FrameOperation, true);



                    if (txtBatchID.Text.Trim() != string.Empty)
                    {
                        GetAll_Item(bizResult.Rows[0]["BATCH_ID"].ToString(), bizResult.Rows[0]["EQPTID"].ToString());
                        txtBatchID.Text = string.Empty;
                        txtBatchID.Focus();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 각 항목 조회 : GetAll_Item()
        public void GetAll_Item(string BatchInfo, string Eqptid)
        {

            try
            {
                _monitorTimer.Stop();
                //문서 정보 조회
                GetDocInfo(BatchInfo);

                //원재료 계량
                //GetRptCheck(BatchInfo);

                //믹싱
                GetMixing(BatchInfo);

                //자주검사
                GetSelfInsp(BatchInfo);

                //체크 스케일
                GetCheckScale(BatchInfo);

                //체크 스케일 배출
                GetCheckScaleOut(BatchInfo);

                //총 투입량
                GetAllInputQty(BatchInfo);

                //원재료 준비
                GetMtrlGrdReady(BatchInfo);

                //슬러리 이송
                GetSlurryMove(BatchInfo);
                //비고
                GetRemark(BatchInfo);

                //공정진척 특이사항
                GetWipRemark(BatchInfo);
            }
            finally
            {
                if (_isSetAutoSelectTime)
                {
                    _monitorTimer.Start();
                }
            }


        }
        #endregion

        #region 문서정보 -  조회 : GetDocInfo()
        //문서정보
        public void GetDocInfo(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_DOC_INFO";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = BatchInfo;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0 && !string.IsNullOrEmpty(bizResult.Rows[0]["WRK_TIME"].ToString()))
                    {
                        Double F_Min = Convert.ToDouble(bizResult.Rows[0]["WRK_TIME"].ToString().Substring(0, bizResult.Rows[0]["WRK_TIME"].ToString().IndexOf(':')), CultureInfo.InvariantCulture) * 60;
                        Double E_Min = Convert.ToDouble(bizResult.Rows[0]["WRK_TIME"].ToString().Substring(bizResult.Rows[0]["WRK_TIME"].ToString().IndexOf(':') + 1, bizResult.Rows[0]["WRK_TIME"].ToString().Length - (bizResult.Rows[0]["WRK_TIME"].ToString().IndexOf(':') + 1)), CultureInfo.InvariantCulture);
                        bizResult.Rows[0]["WRK_TIME_MIN"] = String.Format(nfi, "{0:#,##0}", F_Min + E_Min, CultureInfo.InvariantCulture);
                    }


                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgDocument.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgDocument.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

                    Util.GridSetData(dgDocument, bizResult, FrameOperation, true);

                    GetRptCheck(BatchInfo);

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 문서정보 -  현 배치/계획 저장 : SaveMixRptBatch()
        public void SaveMixRptBatch()
        {
            try
            {

                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
                inDataTable.Columns.Add("LOTID", typeof(string));   //배치ID
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EqptId;
                row["LOTID"] = _BatchID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                //아이템 정보
                DataTable inItem = inData.Tables.Add("IN_ITEM");
                inItem.Columns.Add("RPT_ITEM", typeof(string));//작업일지 항목
                inItem.Columns.Add("MIX_SEQS", typeof(string)); //차수
                inItem.Columns.Add("RPT_ITEM_VALUE01", typeof(string));

                row = inItem.NewRow();
                row["RPT_ITEM"] = "ADD_INFO";
                row["MIX_SEQS"] = "BATCH_COUNT";
                row["RPT_ITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgDocument.Rows[0].DataItem, "BATCH_COUNT"));

                inItem.Rows.Add(row);

                string bizRuleName = "BR_PRD_REG_MIXER_RPT_ITEM";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_ITEM", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetDocInfo(_BatchID);
                        GetBatchInfo();
                    });

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 원재료 계량 조회 : GetRptCheck()
        /// <summary>
        /// 원재료 계량 조회
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetRptCheck(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "MTRL_MEASR";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgMtrlGrd.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgMtrlGrd.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);



                    if (bizResult.Rows.Count > 0)
                    {

                        if (_Stat != "PROC")
                        {
                            DataRow drInput = bizResult.NewRow();
                            drInput["RPT_ITEM_VALUE02"] = ObjectDic.Instance.GetObjectName("수동투입");   //수동투입
                            drInput["RPT_ITEM_VALUE08"] = String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(bizResult.Rows[0]["RPT_ITEM_VALUE20"].ToString(), CultureInfo.InvariantCulture) - Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgDocument.Rows[0].DataItem, "OUT_QTY")), CultureInfo.InvariantCulture));
                            bizResult.Rows.Add(drInput);
                        }
                        DataRow dr1 = bizResult.NewRow();
                        dr1["RPT_ITEM_VALUE02"] = ObjectDic.Instance.GetObjectName("총합계");   //총계
                        dr1["RPT_ITEM_VALUE06"] = bizResult.Rows[0]["RPT_ITEM_VALUE19"];   //총 설정값
                        dr1["RPT_ITEM_VALUE08"] = bizResult.Rows[0]["RPT_ITEM_VALUE20"];   //총 계량값
                        dr1["RPT_ITEM_VALUE12"] = bizResult.Rows[0]["RPT_ITEM_VALUE21"];   //총 계량시간
                        dr1["RPT_ITEM_VALUE18"] = bizResult.Rows[0]["RPT_ITEM_VALUE22"];   //총 대기시간
                        bizResult.Rows.Add(dr1);
                    }

                    Util.GridSetData(dgMtrlGrd, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 믹싱 : GetMixing()
        /// <summary>
        /// 믹싱
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetMixing(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "MIXING";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        DataRow dr1 = bizResult.NewRow();
                        dr1["RPT_ITEM_VALUE02"] = ObjectDic.Instance.GetObjectName("총합계");   //총계
                        dr1["RPT_ITEM_VALUE10"] = bizResult.Rows[0]["RPT_ITEM_VALUE12"];   //총 믹싱시간
                        dr1["RPT_ITEM_VALUE11"] = bizResult.Rows[0]["RPT_ITEM_VALUE13"];   //총 대기시간
                        bizResult.Rows.Add(dr1);
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgMix.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgMix.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgMix, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 자주검사 : GetSelfInsp()
        /// <summary>
        /// 자주검사
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetSelfInsp(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_SELF_INSP";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Process.MIXING;
                dr["LOTID"] = BatchInfo;

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgSelfInsp.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgSelfInsp.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgSelfInsp, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 체크스케일 : GetCheckScale()
        /// <summary>
        /// 체크스케일
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetCheckScale(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "CHECK_SCALE";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgChkScale.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgChkScale.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgChkScale, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 총 투입량 : GetAllInputQty()
        /// <summary>
        /// 총 투입량
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetAllInputQty(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_ALL_INPUT_QTY";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgInputQty.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgInputQty.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgInputQty, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 원재료 준비 - 조회 : GetMtrlGrdReady()
        /// <summary>
        /// 원재료 준비 조회
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetMtrlGrdReady(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "MTRL_NEXT_BATCH";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgMtrlGrdReady.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgMtrlGrdReady.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgMtrlGrdReady, bizResult, FrameOperation, true);
                    HopperComb();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 원재료 준비 - 저장 : SaveGrdReady()
        /// <summary>
        /// 원재료 준비 저장
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void SaveGrdReady()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
                inDataTable.Columns.Add("LOTID", typeof(string));   //배치ID
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = cboEquipment.SelectedValue.ToString();
                row["LOTID"] = _BatchID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                //아이템 정보
                DataTable inItem = inData.Tables.Add("IN_ITEM");
                inItem.Columns.Add("RPT_ITEM", typeof(string));//작업일지 항목
                inItem.Columns.Add("MIX_SEQS", typeof(string)); //차수
                inItem.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE07", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE08", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE09", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE010", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE011", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE012", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE013", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE014", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE015", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE016", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE017", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE018", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE019", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE020", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE021", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE022", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE023", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE024", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE025", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE026", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE027", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE028", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE029", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE030", typeof(string));
                inItem.Columns.Add("NOTE", typeof(string));

                for (int i = 0; i < dgMtrlGrdReady.Rows.Count; i++)
                {
                    row = inItem.NewRow();
                    row["RPT_ITEM"] = "MTRL_NEXT_BATCH";
                    row["MIX_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE01"));
                    row["RPT_ITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE01"));
                    row["RPT_ITEM_VALUE02"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE02"));
                    row["RPT_ITEM_VALUE03"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE03")).Replace(",", "");
                    row["RPT_ITEM_VALUE04"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE04")).Replace(",", "");
                    row["RPT_ITEM_VALUE05"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE05")).Replace(",", "");
                    row["RPT_ITEM_VALUE06"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE06")).Replace(",", "");
                    row["RPT_ITEM_VALUE07"] = Util.NVC(DataTableConverter.GetValue(dgMtrlGrdReady.Rows[i].DataItem, "RPT_ITEM_VALUE07")).Replace(",", "");
                    inItem.Rows.Add(row);
                }
                string bizRuleName = "BR_PRD_REG_MIXER_RPT_ITEM";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_ITEM", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetMtrlGrdReady(_BatchID);
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 원재료 준비 - 요청정보자재 조회 : GetMtrlGrdReady_Mtrl()
        /// <summary>
        /// 요청 정보자재 조회
        /// </summary>
        public void GetMtrlGrdReady_Mtrl()
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_NEXT_MTRL_READY";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.gridClear(dgMtrlGrdReady);
                    Util.GridSetData(dgMtrlGrdReady, bizResult, FrameOperation, true);
                    HopperComb();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 원재료 준비 - 저장 Validation : Validation_GrdReady()
        /// <summary>
        /// 원자재 재료 Validation
        /// </summary>
        /// <returns></returns>
        private bool Validation_GrdReady()
        {
            if (dgMtrlGrdReady.Rows.Count == 0)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }
            return true;
        }

        #endregion

        #region 체크 스케일 배출 - 조회 : GetCheckScaleOut()
        /// <summary>
        /// 체크 스케일 배출 조회
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetCheckScaleOut(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "CHECK_SCALE_OUT";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgChkScaleOut.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgChkScaleOut.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    Util.GridSetData(dgChkScaleOut, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 슬러리 이송 - 조회 : GetSlurryMove()

        /// <summary>
        /// 슬러리 이송 조회 
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetSlurryMove(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "SLURRY_MOVE";

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        dgSlurry.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                    else
                        dgSlurry.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                    if (bizResult.Rows.Count == 1)
                    {

                        DataRow drMove_Tank = bizResult.NewRow();
                        drMove_Tank["RPT_ITEM_VALUE01"] = ObjectDic.Instance.GetObjectName("이송탱크");
                        bizResult.Rows.Add(drMove_Tank);
                        Util.GridSetData(dgSlurry, bizResult, FrameOperation, true);

                    }
                    else
                    {

                        DataView inputVm = bizResult.DefaultView;
                        inputVm.Sort = "CLCT_DTTM";
                        //bizResult.DefaultView.Sort = "CLCT_DTTM";
                        //bizResult.AcceptChanges();
                        Util.GridSetData(dgSlurry, inputVm.ToTable(), FrameOperation, true);
                    }


                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 슬러리 이송 - 저장 : SaveSlurryMove()
        /// <summary>
        /// 슬러리 이송 저장
        /// </summary>
        public void SaveSlurryMove()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
                inDataTable.Columns.Add("LOTID", typeof(string));   //배치ID
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = cboEquipment.SelectedValue.ToString();
                row["LOTID"] = _BatchID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                //아이템 정보
                DataTable inItem = inData.Tables.Add("IN_ITEM");
                inItem.Columns.Add("RPT_ITEM", typeof(string));//작업일지 항목
                inItem.Columns.Add("MIX_SEQS", typeof(string)); //차수
                inItem.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                inItem.Columns.Add("RPT_ITEM_VALUE02", typeof(string));


                for (int i = 0; i < dgSlurry.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[i].DataItem, "RPT_ITEM_VALUE01")) == ObjectDic.Instance.GetObjectName("이송탱크")
                        //2023.08.10 [ESHM] SLURRY_MOVE_01, SLURRY_MOVE_02 로 저장되어야 함. RPT_ITEM 이 세분화됨
                        || (dgSlurry.Columns.Contains("ORG_RPT_ITEM_VALUE01") && Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[i].DataItem, "ORG_RPT_ITEM_VALUE01")).Equals("MOVE_TANK"))
                        )
                    {
                        row = inItem.NewRow();

                        //2023.08.10 [ESHM] SLURRY_MOVE_01, SLURRY_MOVE_02 로 저장되어야 함. RPT_ITEM 이 세분화됨
                        //row["RPT_ITEM"] = "SLURRY_MOVE";
                        row["RPT_ITEM"] = Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[i].DataItem, "RPT_ITEM"));

                        row["MIX_SEQS"] = "MOVE_TANK";
                        row["RPT_ITEM_VALUE01"] = "MOVE_TANK";
                        row["RPT_ITEM_VALUE02"] = Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[i].DataItem, "RPT_ITEM_VALUE02"));
                        inItem.Rows.Add(row);
                    }

                }

                string bizRuleName = "BR_PRD_REG_MIXER_RPT_ITEM";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_ITEM", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetSlurryMove(_BatchID);
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region 특이사항 - 조회 : GetRemark()

        /// <summary>
        /// 특이사항 조회 
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetRemark(string BatchInfo)
        {
            try
            {

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["RPT_ITEM"] = "ADD_INFO";
                dr["MIX_SEQS"] = "REMARK";
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text = bizResult.Rows[0]["NOTE"].ToString();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 특이사항 - 저장 : SaveRemark()

        /// <summary>
        /// 특이사항 저장
        /// </summary>
        public void SaveRemark()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
                inDataTable.Columns.Add("LOTID", typeof(string));   //배치ID
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = cboEquipment.SelectedValue.ToString();
                row["LOTID"] = _BatchID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                //아이템 정보
                DataTable inItem = inData.Tables.Add("IN_ITEM");
                inItem.Columns.Add("RPT_ITEM", typeof(string));//작업일지 항목
                inItem.Columns.Add("MIX_SEQS", typeof(string)); //차수
                inItem.Columns.Add("NOTE", typeof(string));

                row = inItem.NewRow();
                row["RPT_ITEM"] = "ADD_INFO";
                row["MIX_SEQS"] = "REMARK";
                row["NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                inItem.Rows.Add(row);
                string bizRuleName = "BR_PRD_REG_MIXER_RPT_ITEM";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_ITEM", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetRemark(_BatchID);
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 공정진척 특이사항 - 조회 : GetWipRemark()

        /// <summary>
        /// 공정진척 특이사항 조회 
        /// </summary>
        /// <param name="BatchInfo"></param>
        public void GetWipRemark(string BatchInfo)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_MIX_PROC_WIP_NOTE";

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = BatchInfo;
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        new TextRange(WIPRemark.Document.ContentStart, WIPRemark.Document.ContentEnd).Text = bizResult.Rows[0]["WIP_NOTE"].ToString();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 로딩 설정 : ShowLoadingIndicator(), HiddenLoadingIndicator()

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

        #region  초기화 :  Clear()
        /// <summary>
        /// 초기화
        /// </summary>
        private void Clear()
        {

            //문서번호 정보 초기화
            Util.gridClear(dgDocument);
            //원재료 계량 초기화
            Util.gridClear(dgMtrlGrd);
            //믹싱 초기화
            Util.gridClear(dgMix);
            //자주검사 초기화
            Util.gridClear(dgSelfInsp);
            //체크스케일 초기화
            Util.gridClear(dgChkScale);
            //총투입량 초기화
            Util.gridClear(dgInputQty);
            //원재료준비 초기화
            Util.gridClear(dgMtrlGrdReady);
            //체크스케일배출
            Util.gridClear(dgChkScaleOut);
            //슬러리 이송
            Util.gridClear(dgSlurry);

            //비고
            rtxRemark.Document.Blocks.Clear();

            //공정진척 특이사항
            WIPRemark.Document.Blocks.Clear();

            //전역변수 
            _BatchID = string.Empty;
            _EqptId = string.Empty;
            _Stat = string.Empty;

        }

        #endregion

        #region  인쇄 팝업 열기 닫기 :  PopupPrint(),popupPrint_Closed()
        /// <summary>
        /// 작업일지 출력 팝업
        /// </summary>
        private void PopupPrint()
        {
            CMM_MIX_JOB_SHEET_PRINT popupPrint = new CMM_MIX_JOB_SHEET_PRINT();
            popupPrint.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupPrint.Name.ToString()) == false)
                return;

            object[] parameters = new object[10];

            parameters[0] = DataTableConverter.Convert(dgDocument.ItemsSource);     //배치정보
            parameters[1] = DataTableConverter.Convert(dgMtrlGrd.ItemsSource);      //원재료계량
            parameters[2] = DataTableConverter.Convert(dgMix.ItemsSource);          // 믹싱
            parameters[3] = DataTableConverter.Convert(dgSelfInsp.ItemsSource);     //자주검사
            parameters[4] = DataTableConverter.Convert(dgChkScale.ItemsSource);     //체크스케일
            parameters[5] = DataTableConverter.Convert(dgInputQty.ItemsSource);     //총투입량
            parameters[6] = DataTableConverter.Convert(dgMtrlGrdReady.ItemsSource); //원재료 준비
            parameters[7] = DataTableConverter.Convert(dgChkScaleOut.ItemsSource);  //체크스케일 배출
            parameters[8] = DataTableConverter.Convert(dgSlurry.ItemsSource);       //슬러리 이송
            parameters[9] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text; //특이사항

            C1WindowExtension.SetParameters(popupPrint, parameters);

            popupPrint.Closed += new EventHandler(popupPrint_Closed);
            grdMain.Children.Add(popupPrint);
            popupPrint.BringToFront();
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            CMM_MIX_JOB_SHEET_PRINT popup = sender as CMM_MIX_JOB_SHEET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            grdMain.Children.Remove(popup);
        }
        #endregion

        #region  인쇄 Validation :  ValidationGridAdd()

        private bool ValidationGridAdd(string popName)
        {

            if (dgDocument.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905");
                return false;
            }

            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Time 셋팅 : TimerSetting()
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            // 자동 조회 시간 Combo
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboReset, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboReset != null && cboReset.Items.Count > 0)
                cboReset.SelectedIndex = 0;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboReset?.SelectedValue?.ToString()))
                    second = int.Parse(cboReset.SelectedValue.ToString());

                _monitorTimer.Tick += _monitorTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        private void _monitorTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;
            if (_BatchID == string.Empty) return;


            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;
                    GetAll_Item(_BatchID, _EqptId);


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


    }
}
