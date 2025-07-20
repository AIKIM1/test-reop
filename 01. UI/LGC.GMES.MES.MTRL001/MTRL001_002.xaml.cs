/*************************************************************************************
 Created Date : 2019.07.11
      Creator : 정문교
   Decription : 원자재관리 - 원자재 출고 요청 현황 조회 
--------------------------------------------------------------------------------------
 [Change History]

  2019.11.08  정문교 : 동 콤보 조회시 Setting에 설정된 동 정보가 아닌 CNB에 해당하는 동정보 전부 갖고오게 수정
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private System.Windows.Threading.DispatcherTimer _timer = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_002()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgRequest);
            Util.gridClear(dgRequestMtrl);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "cboAreaAll");

            //라인
            EquipmentSegmentCombo(cboEquipmentSegment);
            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void SetControl()
        {
            cboEquipmentSegment.SelectedIndex = 0;

            numRefresh.ValueChanged += numRefresh_ValueChanged;
        }

        private void TimerSetting()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            _timer = new System.Windows.Threading.DispatcherTimer();
            int interval = Convert.ToInt32(numRefresh.Value) * 60; 

            _timer.Interval = TimeSpan.FromSeconds(interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            // 조회
            SearchProcess();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 갱신주기 사용
        /// </summary>
        private void chkInterval_Checked(object sender, RoutedEventArgs e)
        {
            numRefresh.IsEnabled = true;
            TimerSetting();
        }

        private void chkInterval_Unchecked(object sender, RoutedEventArgs e)
        {
            numRefresh.IsEnabled = false;

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null) return;

            //라인
            EquipmentSegmentCombo(cboEquipmentSegment);
            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);

            // 조회
            SearchProcess();

            // 주기 재설정
            TimerSetting();
        }
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedValue == null) return;

            //공정
            ProcessCombo(cboProcess);
            //설비
            EquipmentCombo(cboEquipment);
        }
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null) return;

            //설비
            EquipmentCombo(cboEquipment);
        }

        private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                ////Grid Data Binding 이용한 Background 색 변경
                //if (e.Cell.Row.Type == DataGridRowType.Item)
                //{
                //    if (e.Cell.Row.Index == 0)
                //    {
                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                //    }
                //    else
                //    {
                //        e.Cell.Presenter.Background = null;
                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                //    }
                //}
            }));
        }

        private void dgRequestChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgRequest.SelectedIndex = idx;

                        //if (idx == 0)
                        //{
                        //    dgRequest.Rows[idx].Presenter.Background = new SolidColorBrush(Colors.Red);
                        //    dgRequest.Rows[idx].Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        //    dgRequest.Rows[idx].Presenter.FontWeight = FontWeights.Bold;
                        //}

                        // 요청 자재 조회
                        SearchMTRL();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRequestMtrl_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_SPLY_REQ_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void numRefresh_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            _timer.Tick -= new EventHandler(timer_Tick);
            TimerSetting();
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// 요청 리스트 출력
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint())
                return;

            PopupPrint();
        }

        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 라인
        /// </summary>
        private void EquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void ProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 설비
        /// </summary>
        private void EquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(),
                                                        cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString(), null };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 요청 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                Nullable<DateTime> dRequest = null;
                if (dgRequest.Rows.Count > 0)
                    dRequest = DateTime.Parse(DataTableConverter.GetValue(dgRequest.Rows[dgRequest.Rows.Count - 1].DataItem, "MTRL_SPLY_REQ_DTTM").ToString());

                // Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue == null  ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue== null ? null : cboEquipment.SelectedValue.ToString();
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Request;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgRequest, bizResult, FrameOperation, true);

                        // 신규 요청이 있으면 딩동 Sound
                        if (bizResult.Rows.Count > 0)
                        {
                            if (dRequest != null && dRequest < DateTime.Parse(bizResult.Rows[bizResult.Rows.Count - 1]["MTRL_SPLY_REQ_DTTM"].ToString()))
                            {
                                Util.DingDongPlayer();
                            }
                        }

                        // 첫번쨰 선택및 요청 자재 조회
                        if (bizResult.Rows.Count > 0)
                        {
                            DataTableConverter.SetValue(dgRequest.Rows[0].DataItem, "CHK", true);
                            dgRequest.SelectedIndex = 0;

                            SearchMTRL();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 지재 LOT 조회
        /// </summary>
        private void SearchMTRL()
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST_MTRL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgRequestMtrl, bizResult, null);

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
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

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            //if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    // 라인을 선택해주세요
            //    Util.MessageValidation("SFU4050");
            //    return false;
            //}

            return true;
        }

        private bool ValidationPrint()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]

        /// <summary>
        /// 요청리스트 출력 팝업
        /// </summary>
        private void PopupPrint()
        {
            CMM_MTRL_REQUEST_PRINT popupPrint = new CMM_MTRL_REQUEST_PRINT();
            popupPrint.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupPrint.Name.ToString()) == false)
                return;

            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

            object[] parameters = new object[2];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
            parameters[1] = "N";      // Direct 출력 여부

            C1WindowExtension.SetParameters(popupPrint, parameters);

            popupPrint.Closed += new EventHandler(popupPrint_Closed);
            grdMain.Children.Add(popupPrint);
            popupPrint.BringToFront();
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            CMM_MTRL_REQUEST_PRINT popup = sender as CMM_MTRL_REQUEST_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            grdMain.Children.Remove(popup);
        }

        #endregion

        #region [Func]

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

        private bool ValidationGridAdd(string popName)
        {
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
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                SearchProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        #endregion

    }
}
