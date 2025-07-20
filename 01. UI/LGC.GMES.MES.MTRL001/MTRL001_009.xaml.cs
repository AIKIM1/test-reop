/*************************************************************************************
 Created Date : 2019.07.19
      Creator : 정문교
   Decription : 원자재관리 - 원자재 요청 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.07.05  정문교 : Initial Created.
  2020.02.03  정문교 : IWMS <-> GMES I/F 변경에 따른 수정, 요청 상태 변경 및 적재 IWMS로 변경
                       - 요청 상태 코드 MTRL_SPLY_REQ_STAT_CODE -> MTRL_SPLY_REQ_STAT_CODE_ASSY 변경

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_009 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_009()
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
            Util.gridClear(dgHistory);
            Util.gridClear(dgHistoryMlot);

            //if (!(bool)chkMlot.IsChecked)
            //    dgHistory.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
            //else
                dgHistory.AlternatingRowBackground = null;

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
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //상태
            string[] sFilter = { "MTRL_SPLY_REQ_STAT_CODE_ASSY" };
            _combo.SetCombo(cboReqStatCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
        }

        private void SetControl(bool isVisibility = false)
        {
            dgHistory.AlternatingRowBackground = null;
            GridColumnVisibility(false);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 자재 Lot Visibility
        /// </summary>
        private void chkMlot_Checked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(true);
        }
        private void chkMlot_Unchecked(object sender, RoutedEventArgs e)
        {
            //GridColumnVisibility(false);
        }

        /// <summary>
        /// dgHistory Event
        /// </summary>
        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("DIFF_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));

        }

        private void dgHistory_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgHistory.GetCellFromPoint(pnt);

            if (cell != null)
            {
                //if (Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[cell.Row.Index].DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString()).Equals(Mtrl_Request_StatCode.Completed))
                //{
                //    dgHistoryMlot.Columns["RCPT_CMPL_FLAG"].Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    dgHistoryMlot.Columns["RCPT_CMPL_FLAG"].Visibility = Visibility.Collapsed;
                //}

                SearchMLOT(cell.Row.Index);
            }
        }

        private void dgHistoryMlot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "RCPT_CMPL_FLAG") == null ||
                        string.IsNullOrWhiteSpace(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "RCPT_CMPL_FLAG").ToString()))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));

        }

        /// <summary>
        /// 요청 이력 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// 엑셀
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcel())
                    return;

                new ExcelExporter().Export(dgHistory);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 요청 이력 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                string bizRuleName = (bool)chkMlot.IsChecked ? "DA_MTR_SEL_MATERIAL_REQUEST_HISTORY_MLOT" : "DA_MTR_SEL_MATERIAL_REQUEST_HISTORY";

                // Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("DIFF_QTY", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcess.SelectedValue.ToString()) ? null : cboProcess.SelectedValue.ToString();
                newRow["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = string.IsNullOrWhiteSpace(cboReqStatCode.SelectedValue.ToString()) ? null : cboReqStatCode.SelectedValue.ToString();
                newRow["DIFF_QTY"] = (bool)chkDiffQty.IsChecked ? "Y" : null;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Request;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);
                        GridColumnVisibility((bool)chkMlot.IsChecked);
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
        private void SearchMLOT(int row)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgHistory.Rows[row].DataItem, "MTRL_SPLY_REQ_ID").ToString());
                newRow["DEL_FLAG"] = "N";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_LOADING_MLOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryMlot, bizResult, null);

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
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationExcel()
        {
            if (dgHistory.Rows.Count - dgHistory.TopRows.Count - dgHistory.BottomRows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
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

        private void GridColumnVisibility(bool isVisibility = false)
        {
            if (isVisibility)
            {
                dgHistory.Columns["MLOTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgHistory.Columns["MLOTID"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
