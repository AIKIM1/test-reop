/*************************************************************************************
 Created Date : 2019.08.13
      Creator : 정문교
   Decription : 원자재관리 - Foil 공급 요청 현황 조회 
--------------------------------------------------------------------------------------
 [Change History]


 
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
    public partial class MTRL001_101 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private System.Windows.Threading.DispatcherTimer _timer = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_101()
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
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent);

            //극성
            string[] sFilter = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboEltrTypeCode, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);

            //설비
            SetEquipmentCombo(cboEquipment);
        }

        private void SetControl()
        {
            numRefresh.ValueChanged += numRefresh_ValueChanged;

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboEltrTypeCode.SelectedValueChanged += cboEltrTypeCode_SelectedValueChanged;
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
            listAuth.Add(btnChange);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void cboEltrTypeCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
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

                //Grid Data Binding 이용한 Foreground 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_ELTR_TYPE_NAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
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
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
        /// 공급대상지정
        /// </summary>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChange())
                return;

            ChangeProcess();
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 설비 콤보
        /// </summary>
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID
                                    , string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString()
                                    , Process.COATING
                                    , cboEltrTypeCode.SelectedValue.ToString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 요청 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                if (cboEltrTypeCode.SelectedValue == null) return;

                Nullable<DateTime> dRequest = null;
                if (dgRequest.Rows.Count > 0)
                    dRequest = DateTime.Parse(DataTableConverter.GetValue(dgRequest.Rows[dgRequest.Rows.Count-1].DataItem, "MTRL_SPLY_REQ_DTTM").ToString());

                // Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Request;
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue?? null;
                newRow["MTRL_ELTR_TYPE_CODE"] = cboEltrTypeCode.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST_FOIL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
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

                        // 첫번쨰 선택
                        if (bizResult.Rows.Count > 0)
                        {
                            dgRequest.SelectedIndex = 0;
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
        /// 공급 대상 지정
        /// </summary>
        private void ChangeProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CHANGE_YN", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                // INDATA SET
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgRequest, "CHK");

                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString());
                newRow["ELTR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "MTRL_ELTR_TYPE_CODE").ToString());
                newRow["CHANGE_YN"] = "Y";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_MTR_REG_MATERIAL_REQUEST_FOIL_SPLY_TRGT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 재조회
                        SearchProcess();
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

            if (cboEltrTypeCode.SelectedValue == null || cboEltrTypeCode.SelectedValue.ToString().Equals("SELECT"))
            {
                // 극성을 선택 하세요.
                Util.MessageValidation("SFU1467");
                return false;
            }

            return true;
        }

        private bool ValidationChange()
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
