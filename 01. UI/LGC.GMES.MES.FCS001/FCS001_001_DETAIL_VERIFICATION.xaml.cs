/*************************************************************************************
 Created Date : 2024.03.05
      Creator : 복현수
   Decription : Verification 측정값 조회
   --------------------------------------------------------------------------------------
 [Change History]
  2024.03.05 복현수 : Initial Created.
  2024.05.29 복현수 : 사용횟수 초기화 탭에 현재 횟수 추가
  2024.07.18 조영대 : 검사 진행 현황 탭 추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_001_DETAIL_VERIFICATION : UserControl, IWorkArea
    {
        #region Internal Class
        protected class CellInfo
        {
            public CellInfo()
            {
                IsProgress = false;
            }

            public CellInfo(Brush forground, Brush background)
            {
                Forground = forground;
                Background = background;
                IsProgress = false;
            }

            public Brush Forground { get; set; }
            public Brush Background { get; set; }
            public bool IsProgress { get; set; }
            public string VerifEqptId { get; set; }
            public string LocationCode { get; set; }
        }
        #endregion

        #region Declaration & Constructor

        //외부 화면에서 받아온 파라미터
        private string _sEqptID;
        private string _sLaneID;
        private string _sRow;
        private string _sCol;
        private string _sStg;
        private string _sCstID;

        //검사공정 레시피 조회값
        //private string _sCH_VLTG_ULMT;
        //private string _sCH_VLTG_LLMT;
        //private string _sCRNT_LINE_RATE_ULMT;
        //private string _sCRNT_LINE_RATE_LLMT;
        //private string _sVLTG_LINE_RATE_ULMT;
        //private string _sVLTG_LINE_RATE_LLMT;

        //판정 결과 종류
        //<0, 정상>
        //<1, 전압 상한불량>
        //<2, 전압 하한불량>
        //<3, 라인 전류 평균 상한불량>
        //<4, 라인 전류 평균 하한불량>
        //<5, 라인 전압 평균 상한불량>
        //<6, 라인 전압 평균 하한불량>
        //<7, 비접촉 불량>
        //private Dictionary<string, string> _dicJudgKind = new Dictionary<string, string>();

        Util _Util = new Util();

        //기본 설정시간 parameter 추가
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        private string langYul = string.Empty;
        private string langYun = string.Empty;
        private string langDan = string.Empty;
        private string NON_TARGET = string.Empty;

        private Dictionary<Point, CellInfo> cellInfos = null;
        private DataTable dtScanProgress = null;
        private Brush nonTargetBrushForColor = Brushes.Black;
        private Brush nonTargetBrushBackColor = Brushes.Silver;
        private Brush lineBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        public FCS001_001_DETAIL_VERIFICATION()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            string[] arrColumn = { "LANGID", "AREAID", "S70" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "1" };
            cboLaneUnit.SetDataComboItem("DA_SEL_FORMATION_UNIT_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);

            //#region tab: 검사 이력 조회

            //CommonCombo_Form _combo = new CommonCombo_Form();

            ////C1ComboBox[] cboLaneChild = { cboRow, cboCol, cboStg };
            //_combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE"); //, cbChild: cboLaneChild);

            //string sLaneID = Util.GetCondition(cboLane, bAllNull: true);
            //string[] sFilterEqptypeLane1 = { "1", sLaneID };
            ////C1ComboBox[] cboRowColStgParent = { cboLane };
            //_combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.ALL, sCase: "ROW", sFilter: sFilterEqptypeLane1); //, cbParent: cboRowColStgParent);
            //_combo.SetCombo(cboCol, CommonCombo_Form.ComboStatus.ALL, sCase: "COL", sFilter: sFilterEqptypeLane1); //, cbParent: cboRowColStgParent);
            //_combo.SetCombo(cboStg, CommonCombo_Form.ComboStatus.ALL, sCase: "STG", sFilter: sFilterEqptypeLane1); //, cbParent: cboRowColStgParent);

            //#endregion

            //#region tab: 검사값 조회

            ////C1ComboBox[] cboLaneChild = { cboRow, cboCol, cboStg };
            //_combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE"); //, cbChild: cboLaneChild);

            //string sLaneID = Util.GetCondition(cboLane, bAllNull: true);
            //string[] sFilterEqptypeLane = { "1", sLaneID };
            ////C1ComboBox[] cboRowColStgParent = { cboLane };
            //_combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROW", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);
            //_combo.SetCombo(cboCol, CommonCombo_Form.ComboStatus.SELECT, sCase: "COL", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);
            //_combo.SetCombo(cboStg, CommonCombo_Form.ComboStatus.SELECT, sCase: "STG", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);

            //#endregion
        }

        private void InitControl()
        {
            btnUnloadReq.IsEnabled = false;
            btnCntReset.IsEnabled = false;

            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();

            // 시간 제거
            dtpFromDate.SelectedDateTime = new DateTime(dtpFromDate.SelectedDateTime.Year, dtpFromDate.SelectedDateTime.Month, dtpFromDate.SelectedDateTime.Day);
            dtpToDate.SelectedDateTime = new DateTime(dtpToDate.SelectedDateTime.Year, dtpToDate.SelectedDateTime.Month, dtpToDate.SelectedDateTime.Day);

            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
            dtpToTime.DateTime = DateTime.Now;

            langYun = ObjectDic.Instance.GetObjectName("연");
            langYul = ObjectDic.Instance.GetObjectName("열");
            langDan = ObjectDic.Instance.GetObjectName("단");
            NON_TARGET = ObjectDic.Instance.GetObjectName("NON_TARGET");

            GetNonTargetColor();
        }
        #endregion

        #region Event

        #region tab: 공용
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
                SetWorkResetTime();
                InitControl();

                object[] parameters = this.FrameOperation.Parameters;

                if (parameters != null && parameters.Length >= 1)
                {
                    _sEqptID = parameters[0] as string;
                    _sLaneID = parameters[1] as string;
                    _sRow = parameters[2] as string;
                    _sCol = parameters[3] as string;
                    _sStg = parameters[4] as string;
                    _sCstID = parameters[5] as string;

                    if (!string.IsNullOrWhiteSpace(_sCstID))
                    {
                        txtTrayID_Hist.Text = _sCstID;
                        //GetHistList();
                    }

                    GetValueList(null);

                    tabMain.SelectedIndex = 1; //'측정값 조회' 탭으로 이동
                }

                this.Loaded -= UserControl_Loaded; //화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region tab1: Verification Kit 정보 조회
        private void txtTrayID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtTrayID_Hist.Text.Trim() == string.Empty) return;

                btnHistSearch.PerformClick();
            }
        }

        private void btnUnloadReq_Click(object sender, RoutedEventArgs e)
        {
            SetUnloadReq();

            //GetHistList();
        }

        private void btnHistExport1_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgVerificationHist1);
        }

        private void btnHistExport2_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgVerificationHist2);
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetHistList();
        }

        private void dgVerificationHist1_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            System.Windows.Point pnt = e.GetPosition(null);
            if (pnt == null)
                return;

            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null)
                return;

            C1.WPF.DataGrid.DataGridRow gridRow = cell.Row;

            object[] internal_para = new object[7];

            if (gridRow != null)
            {
                int row = cell.Row.Index;

                string sEqptID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQPTID"), null);
                string sLaneID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LANEID"), null);
                string sRow = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "ROW"), null);
                string sCol = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "COL"), null);
                string sStg = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "STG"), null);
                string sVERIF_DTTM_ST = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "VERIF_DTTM_ST"), null);
                string sVERIF_DTTM_ED = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "VERIF_DTTM_ED"), null);

                internal_para[0] = sEqptID;
                internal_para[1] = sLaneID;
                internal_para[2] = sRow;
                internal_para[3] = sCol;
                internal_para[4] = sStg;
                internal_para[5] = sVERIF_DTTM_ST;
                internal_para[6] = sVERIF_DTTM_ED;
            }

            GetValueList(internal_para);

            tabMain.SelectedIndex = 1; //'측정값 조회' 탭으로 이동
        }

        private void dgVerificationHist2_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            System.Windows.Point pnt = e.GetPosition(null);
            if (pnt == null)
                return;

            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null)
                return;

            C1.WPF.DataGrid.DataGridRow gridRow = cell.Row;

            object[] internal_para = new object[7];

            if (gridRow != null)
            {
                int row = cell.Row.Index;

                string sEqptID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQPTID"), null);
                string sLaneID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "LANEID"), null);
                string sRow = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "ROW"), null);
                string sCol = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "COL"), null);
                string sStg = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "STG"), null);
                string sVERIF_DTTM_ST = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "VERIF_DTTM_ST"), null);
                string sVERIF_DTTM_ED = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "VERIF_DTTM_ED"), null);

                internal_para[0] = sEqptID;
                internal_para[1] = sLaneID;
                internal_para[2] = sRow;
                internal_para[3] = sCol;
                internal_para[4] = sStg;
                internal_para[5] = sVERIF_DTTM_ST;
                internal_para[6] = sVERIF_DTTM_ED;
            }

            GetValueList(internal_para);

            tabMain.SelectedIndex = 1; //'측정값 조회' 탭으로 이동
        }

        private void dgVerificationHist1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //SetProcessEnd();
        }

        private void dgVerificationHist2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //SetProcessEnd();
        }

        private void dgVerificationHist1_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            //try
            //{
            //    if (e.Row.HeaderPresenter == null)
            //    {
            //        return;
            //    }

            //    e.Row.HeaderPresenter.Content = null;

            //    TextBlock tb = new TextBlock();

            //    int num = e.Row.Index + 1 - dgVerificationHist1.TopRows.Count;
            //    if (num > 0)
            //    {
            //        tb.Text = num.ToString();
            //        tb.VerticalAlignment = VerticalAlignment.Center;
            //        tb.HorizontalAlignment = HorizontalAlignment.Center;
            //        e.Row.HeaderPresenter.Content = tb;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgVerificationHist1_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 1);

            dgVerificationHist1?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell == null || e.Cell.Presenter == null) return;

                //e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                if (e.Cell.Column != null && e.Cell.Column.Name.ToString().Equals("PASS_YN"))
                {
                    if (e.Cell.Value != null && string.Equals(e.Cell.Value.ToString(), "FAIL")) //ObjectDic.Instance.GetObjectName("FAIL")))
                    {
                        e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                    }
                }
            }));
        }

        private void dgVerificationHist2_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            //try
            //{
            //    if (e.Row.HeaderPresenter == null)
            //    {
            //        return;
            //    }

            //    e.Row.HeaderPresenter.Content = null;

            //    TextBlock tb = new TextBlock();

            //    int num = e.Row.Index + 1 - dgVerificationHist2.TopRows.Count;
            //    if (num > 0)
            //    {
            //        tb.Text = num.ToString();
            //        tb.VerticalAlignment = VerticalAlignment.Center;
            //        tb.HorizontalAlignment = HorizontalAlignment.Center;
            //        e.Row.HeaderPresenter.Content = tb;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgVerificationHist2_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 1);

            dgVerificationHist2?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell == null || e.Cell.Presenter == null) return;

                //e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                if (e.Cell.Column != null && e.Cell.Column.Name.ToString().Equals("PASS_YN"))
                {
                    if (e.Cell.Value != null && string.Equals(e.Cell.Value.ToString(), "FAIL")) //ObjectDic.Instance.GetObjectName("FAIL")))
                    {
                        e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                    }
                }
            }));
        }
        #endregion

        #region tab2: 측정값 조회
        private void btnValueExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgVerification);
        }

        private void dgVerification_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            //try
            //{
            //    if (e.Row.HeaderPresenter == null)
            //    {
            //        return;
            //    }

            //    e.Row.HeaderPresenter.Content = null;

            //    TextBlock tb = new TextBlock();

            //    int num = e.Row.Index + 1 - dgVerification.TopRows.Count;
            //    if (num > 0)
            //    {
            //        tb.Text = num.ToString();
            //        tb.VerticalAlignment = VerticalAlignment.Center;
            //        tb.HorizontalAlignment = HorizontalAlignment.Center;
            //        e.Row.HeaderPresenter.Content = tb;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgVerification_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 1);

            string sJudg_Rslt = string.Empty;

            dgVerification?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell == null || e.Cell.Presenter == null)
                    return;

                //e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                if (e.Cell.Column != null && e.Cell.Row.Index > dgVerification.TopRows.Count - 1)
                {
                    //같은 행의 판정종류 값 저장
                    sJudg_Rslt = Util.NVC(DataTableConverter.GetValue(e.Cell.DataGrid.Rows[e.Cell.Row.Index].DataItem, "JUDG_RSLT_VALUE"));

                    switch (e.Cell.Column.Name.ToString())
                    {
                        case "JUDG_RSLT_VALUE":
                            if (!e.Cell.Value.ToString().Equals(ObjectDic.Instance.GetObjectName("NORMAL"))) //양품 코드가 아니면
                                e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                            break;

                        //case "LINE_CURNT_VALUE": //전류 상하한 불량은 없는듯?
                        //    break;

                        case "LINE_VLTG_VALUE":
                            if (sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_VLTG_ULMT_DEFECT")) || sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_VLTG_LLMT_DEFECT")))
                                e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;

                            //if (Convert.ToDecimal(e.Cell.Value) < Convert.ToDecimal(_sCH_VLTG_LLMT) || Convert.ToDecimal(e.Cell.Value) > Convert.ToDecimal(_sCH_VLTG_ULMT))
                            //    e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                            break;

                        case "LINE_CURNT_AVG_VALUE":
                            if (sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_CURNT_AVG_ULMT_DEFECT")) || sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_CURNT_AVG_LLMT_DEFECT")))
                                e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;

                            //if (Convert.ToDecimal(e.Cell.Value) < Convert.ToDecimal(_sCRNT_LINE_RATE_LLMT) || Convert.ToDecimal(e.Cell.Value) > Convert.ToDecimal(_sCRNT_LINE_RATE_ULMT))
                            //    e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                            break;

                        case "LINE_VLTG_AVG_VALUE":
                            if (sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_VLTG_AVG_ULMT_DEFECT")) || sJudg_Rslt.Equals(ObjectDic.Instance.GetObjectName("LINE_VLTG_AVG_LLMT_DEFECT")))
                                e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;

                            //if (Convert.ToDecimal(e.Cell.Value) < Convert.ToDecimal(_sVLTG_LINE_RATE_LLMT) || Convert.ToDecimal(e.Cell.Value) > Convert.ToDecimal(_sVLTG_LINE_RATE_ULMT))
                            //    e.Cell.Presenter.Background = System.Windows.Media.Brushes.Yellow;
                            break;

                        default:
                            break;
                    }
                }
            }));
        }

        //private void cboLaneSelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //string sLaneID = Util.GetCondition(cboLane, bAllNull: true);

        //if (string.IsNullOrWhiteSpace(sLaneID))
        //    return;
        //else
        //{
        //    CommonCombo_Form _combo = new CommonCombo_Form();

        //    string[] sFilterEqptypeLane = { "1", sLaneID };
        //    //C1ComboBox[] cboRowColStgParent = { cboLane };
        //    _combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROW", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);
        //    _combo.SetCombo(cboCol, CommonCombo_Form.ComboStatus.SELECT, sCase: "COL", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);
        //    _combo.SetCombo(cboStg, CommonCombo_Form.ComboStatus.SELECT, sCase: "STG", sFilter: sFilterEqptypeLane); //, cbParent: cboRowColStgParent);
        //}
        //}
        #endregion

        #region tab3: 측정횟수 초기화
        private void txtTrayID_MeasrCnt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtTrayID_MeasrCnt.Text.Trim() == string.Empty) return;

                btnMeasrCntSearch.PerformClick();
            }
        }

        private void btnCntReset_Click(object sender, RoutedEventArgs e)
        {
            SetMeasrCntReset();

            //GetMeasrCntList();
        }

        private void btnMeasrCntSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMeasrCntList();
        }

        private void dgMeasrCnt_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            //try
            //{
            //    if (e.Row.HeaderPresenter == null)
            //    {
            //        return;
            //    }

            //    e.Row.HeaderPresenter.Content = null;

            //    TextBlock tb = new TextBlock();

            //    int num = e.Row.Index + 1 - dgMeasrCnt.TopRows.Count;
            //    if (num > 0)
            //    {
            //        tb.Text = num.ToString();
            //        tb.VerticalAlignment = VerticalAlignment.Center;
            //        tb.HorizontalAlignment = HorizontalAlignment.Center;
            //        e.Row.HeaderPresenter.Content = tb;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgMeasrCnt_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 1);
        }
        #endregion

        #region 검사 진행 현황

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cellInfos == null || dgScanProgress.ItemsSource == null) return;

                dgScanProgress.Selection.Clear();
                foreach (KeyValuePair<Point, CellInfo> cellInfo in cellInfos)
                {
                    dgScanProgress.Selection.Add(dgScanProgress.GetCell((int)cellInfo.Key.X, (int)cellInfo.Key.Y));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMeasrSkip_Click(object sender, RoutedEventArgs e)
        {
            SkipProcess();
        }

        private void btnMeasrReset_Click(object sender, RoutedEventArgs e)
        {
            ResetProcess();
        }

        private void btnScanProgSearch_Click(object sender, RoutedEventArgs e)
        {
            ScanProgressSearch();
        }

        private void dgScanProgress_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                List<C1.WPF.DataGrid.DataGridCell> deleteCell = new List<C1.WPF.DataGrid.DataGridCell>();
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgScanProgress.Selection.SelectedCells)
                {
                    Point cellPoint = new Point(cell.Row.Index, cell.Column.Index);
                    if (!cellInfos.ContainsKey(cellPoint))
                    {
                        deleteCell.Add(cell);
                    }
                }

                if (deleteCell.Count > 0)
                {
                    dgScanProgress?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        deleteCell.ForEach(f => dgScanProgress.Selection.Remove(f));
                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgScanProgress_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Row.Index == 3 && e.Cell.Column.Index == 3)
                {
                    e.Cell.Presenter.LeftLineBrush = System.Windows.Media.Brushes.Red;
                    e.Cell.Presenter.TopLineBrush = System.Windows.Media.Brushes.Red;
                }

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Presenter == null) return;

                if (dtScanProgress == null) return;

                if (!dtScanProgress.Columns.Contains(Util.NVC(e.Cell.Column.Index + 1))) return;

                int rowIndex = e.Cell.Row.Index;
                int colIndex = e.Cell.Column.Index;

                bool isHeader = dtScanProgress.Rows[rowIndex]["ROW_TYPE"].Equals("HEADER");
                if (e.Cell.Column.Index == 0) isHeader = true;

                if (isHeader)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                    //e.Cell.Presenter.Padding = new Thickness(0);
                    //e.Cell.Presenter.Margin = new Thickness(0);
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;

                    Point position = new Point(rowIndex, colIndex);

                    if (cellInfos != null && cellInfos.Count > 0 && cellInfos.ContainsKey(position))
                    {
                        e.Cell.Presenter.Foreground = cellInfos[position].Forground;
                        e.Cell.Presenter.Background = cellInfos[position].Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = nonTargetBrushForColor;
                        e.Cell.Presenter.Background = nonTargetBrushBackColor;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgScanProgress_ExecuteCustomBinding(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable _dtDATA = e.ResultData as DataTable;

            SetVerifFormation(_dtDATA);
        }

        private void dgScanProgress_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            // 조회 완료시 실행
        }

        #endregion

        #endregion


        #region Method

        #region tab: 공용
        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetBtnEnable(int tab_No, bool flag)
        {
            switch (tab_No)
            {
                case 1: //tab1: Verification Kit 정보 조회
                    btnUnloadReq.IsEnabled = flag;
                    btnHistExport1.IsEnabled = flag;
                    btnHistExport2.IsEnabled = flag;
                    btnHistSearch.IsEnabled = flag;
                    break;

                case 2: //tab2: 측정값 조회
                    btnValueExport.IsEnabled = flag;
                    break;

                case 3: //tab3: 측정횟수 초기화
                    btnCntReset.IsEnabled = flag;
                    btnMeasrCntSearch.IsEnabled = flag;
                    break;

                default:
                    break;
            }

            //if (!CheckFormAuthority("FORM_FORMATION_MANA") || !FrameOperation.AUTHORITY.Equals("W"))
            //{
            //    btnUnloadReq.IsEnabled = false;
            //    btnCntReset.IsEnabled = false;
            //}
        }

        private bool CheckFormAuthority(string sAuthID)
        {
            bool isEditable = false;

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("AUTHID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["AUTHID"] = sAuthID;
            dr["USE_FLAG"] = 'Y';
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
                isEditable = true;

            return isEditable;
        }

        //private void ShowLoadingIndicator()
        //{
        //    if (loadingIndicator != null)
        //        loadingIndicator.Visibility = Visibility.Visible;
        //}

        //private void HiddenLoadingIndicator()
        //{
        //    if (loadingIndicator != null)
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //}
        #endregion

        #region tab1: Verification Kit 정보 조회
        private void GetHistList()
        {
            try
            {
                SetBtnEnable(1, false);

                dgVerificationHist1.ClearRows();
                dgVerificationHist2.ClearRows();

                txtEqpID_UR.Text = null;
                txtTrayID1_UR.Text = null;
                txtTrayID2_UR.Text = null;

                txtTrayOpStatus1.Tag = null;
                txtTrayOpStatus1.Text = null;
                txtTrayOpStatus2.Tag = null;
                txtTrayOpStatus2.Text = null;

                string sTrayID = Util.NVC(txtTrayID_Hist.Text);

                if (string.IsNullOrWhiteSpace(sTrayID) || sTrayID.Length > 10)
                {
                    //Tray ID를 입력해주세요.
                    Util.Alert("FM_ME_0070");
                    return;
                }

                string sFromDTTM = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00.000");
                string sToDTTM = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00.000"); //("HH:mm:59.997");

                DataTable dtRqst = new DataTable();
                //dtRqst_MeasrCnt.TableName = "INDATA";
                //dtRqst.Columns.Add("SRCTYPE", typeof(string));
                //dtRqst.Columns.Add("IFMODE", typeof(string));
                //dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                //dtRqst.Columns.Add("BOX_PSTN_NO", typeof(string));
                dtRqst.Columns.Add("FROM_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                //dr["SRCTYPE"] = "UI";
                //dr["IFMODE"] = "OFF";
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = sTrayID;
                //dr["BOX_PSTN_NO"] = string.Empty; //BR에서 상하단 각각 조회하도록 구성함
                dr["FROM_DTTM"] = sFromDTTM;
                dr["TO_DTTM"] = sToDTTM;
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_FORM_BOX_VERIF_HIST", "INDATA", "OUTDATA_LOWER,OUTDATA_UPPER", dsRqst);

                if (dsRslt == null || dsRslt.Tables["OUTDATA_LOWER"].Rows.Count == 0)
                {
                    //조회된 값이 없습니다.
                    Util.Alert("FM_ME_0232");
                    return;
                }
                else
                {
                    #region 강제출고요청에서 사용하기 위한 데이터 임시저장, 버튼 활성화 여부
                    string sEqpID = Util.NVC(dsRslt.Tables["OUTDATA_LOWER"].Rows[0]["EQPTID"].ToString());
                    string sLaneID = Util.NVC(dsRslt.Tables["OUTDATA_LOWER"].Rows[0]["LANEID"].ToString());

                    DataTable dtRqst_UR = new DataTable();
                    //dtRqst_UR.TableName = "RQSTDT";
                    dtRqst_UR.Columns.Add("LANGID", typeof(string));
                    dtRqst_UR.Columns.Add("LANE_ID", typeof(string));
                    dtRqst_UR.Columns.Add("EQPTID", typeof(string));

                    DataRow dr_UR = dtRqst_UR.NewRow();
                    dr_UR["LANGID"] = LoginInfo.LANGID;
                    dr_UR["LANE_ID"] = sLaneID;
                    dr_UR["EQPTID"] = sEqpID;
                    dtRqst_UR.Rows.Add(dr_UR);

                    DataTable dtRslt_UR = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_LOAD_EQPT", "RQSTDT", "RSLTDT", dtRqst_UR);

                    if (dtRslt_UR != null && dtRslt_UR.Rows.Count > 0)
                    {
                        txtEqpID_UR.Text = sEqpID;
                        txtTrayID1_UR.Text = dtRslt_UR.Rows[0]["CSTID"].ToString();

                        if (dtRslt_UR.Rows.Count == 2)
                            txtTrayID2_UR.Text = dtRslt_UR.Rows[1]["CSTID"].ToString();

                        if (!dtRslt_UR.Rows[0]["EQPT_CTRL_STAT_CODE"].ToString().Equals("F"))
                        {
                            //강제출고요청 버튼 활성화 여부
                            if (CheckFormAuthority("FORM_FORMATION_MANA")) // && FrameOperation.AUTHORITY.Equals("W"))
                            {
                                if (dtRslt_UR.Rows[0]["STATUS"].ToString().Equals("4") || dtRslt_UR.Rows[0]["STATUS"].ToString().Equals("7")) //4(T):Trouble, 7(S):일시정지
                                    btnUnloadReq.IsEnabled = true;
                            }
                        }
                    }
                    #endregion

                    #region 정보 표시 (하단)
                    if (dsRslt.Tables["OUTDATA_LOWER"] != null && dsRslt.Tables["OUTDATA_LOWER"].Rows.Count > 0)
                    {
                        //측정 이력 표시 (하단)
                        Util.GridSetData(dgVerificationHist1, dsRslt.Tables["OUTDATA_LOWER"], FrameOperation);

                        //공정 상태 표시 (하단)
                        switch (dsRslt.Tables["OUTDATA_LOWER"].Rows[0]["WIPSTAT"].ToString())
                        {
                            case "PROC":
                                txtTrayOpStatus1.Tag = "S";
                                txtTrayOpStatus1.Text = ObjectDic.Instance.GetObjectName("WORKING"); //작업중
                                break;
                            case "END":
                                txtTrayOpStatus1.Tag = "END";
                                txtTrayOpStatus1.Text = ObjectDic.Instance.GetObjectName("완공"); //완공
                                break;
                            case "WAIT":
                                txtTrayOpStatus1.Tag = "S";
                                txtTrayOpStatus1.Text = ObjectDic.Instance.GetObjectName("대기"); //대기
                                break;
                            case "TERM":
                                txtTrayOpStatus1.Tag = "TERM";
                                txtTrayOpStatus1.Text = ObjectDic.Instance.GetObjectName("재공종료"); //재공종료
                                break;
                            default:
                                txtTrayOpStatus1.Tag = string.Empty;
                                txtTrayOpStatus1.Text = ObjectDic.Instance.GetObjectName("INFO_ERR"); //정보이상
                                break;
                        }
                    }
                    #endregion

                    #region 정보 표시 (상단)
                    if (dsRslt.Tables["OUTDATA_UPPER"] != null && dsRslt.Tables["OUTDATA_UPPER"].Rows.Count > 0)
                    {
                        //측정 이력 표시 (상단)
                        Util.GridSetData(dgVerificationHist2, dsRslt.Tables["OUTDATA_UPPER"], FrameOperation);

                        //공정 상태 표시 (상단)
                        switch (dsRslt.Tables["OUTDATA_UPPER"].Rows[0]["WIPSTAT"].ToString())
                        {
                            case "PROC":
                                txtTrayOpStatus2.Tag = "S";
                                txtTrayOpStatus2.Text = ObjectDic.Instance.GetObjectName("WORKING"); //작업중
                                break;
                            case "END":
                                txtTrayOpStatus2.Tag = "END";
                                txtTrayOpStatus2.Text = ObjectDic.Instance.GetObjectName("완공"); //완공
                                break;
                            case "WAIT":
                                txtTrayOpStatus2.Tag = "S";
                                txtTrayOpStatus2.Text = ObjectDic.Instance.GetObjectName("대기"); //대기
                                break;
                            case "TERM":
                                txtTrayOpStatus2.Tag = "TERM";
                                txtTrayOpStatus2.Text = ObjectDic.Instance.GetObjectName("재공종료"); //재공종료
                                break;
                            default:
                                txtTrayOpStatus2.Tag = string.Empty;
                                txtTrayOpStatus2.Text = ObjectDic.Instance.GetObjectName("INFO_ERR"); //정보이상
                                break;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //SetBtnEnable(1, true);
                btnHistExport1.IsEnabled = true;
                btnHistExport2.IsEnabled = true;
                btnHistSearch.IsEnabled = true;
            }
        }

        private void SetUnloadReq()
        {
            try
            {
                SetBtnEnable(1, false);

                string sEqpID = Util.NVC(txtEqpID_UR.Text);
                string sTrayID1 = Util.NVC(txtTrayID1_UR.Text);
                string sTrayID2 = Util.NVC(txtTrayID2_UR.Text);

                if (string.IsNullOrWhiteSpace(sTrayID1))
                {
                    //설비내에 Tray가 존재하지 않습니다.
                    Util.Alert("FM_ME_0170");
                    return;
                }
                else
                {
                    #region 강제출고요청
                    //강제출고요청을 하시겠습니까?
                    Util.MessageConfirm("FM_ME_0094", result =>
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            //return;
                        }
                        else
                        {
                            try
                            {
                                DataSet indataSet = new DataSet();
                                DataTable inData = indataSet.Tables.Add("INDATA");
                                inData.Columns.Add("SRCTYPE", typeof(string));
                                inData.Columns.Add("IFMODE", typeof(string));
                                inData.Columns.Add("INIT_FIRE_FLAG", typeof(string));
                                inData.Columns.Add("EQPT_CTRL_STAT_CODE", typeof(string));
                                inData.Columns.Add("EQPTID", typeof(string));
                                inData.Columns.Add("USERID", typeof(string));
                                inData.Columns.Add("USER_IP", typeof(string));
                                inData.Columns.Add("PC_NAME", typeof(string));
                                inData.Columns.Add("MENU_ID", typeof(string));

                                DataRow dr = inData.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["INIT_FIRE_FLAG"] = string.Empty;
                                dr["EQPT_CTRL_STAT_CODE"] = "F";
                                dr["EQPTID"] = sEqpID;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["USER_IP"] = LoginInfo.USER_IP;
                                dr["PC_NAME"] = LoginInfo.PC_NAME;
                                dr["MENU_ID"] = LoginInfo.CFG_MENUID;
                                inData.Rows.Add(dr);

                                //ShowLoadingIndicator();
                                new ClientProxy().ExecuteService_Multi("BR_SET_EQP_CTR_STATUS_MANUAL", "INDATA", "OUTDATA", (dsRslt, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        if (dsRslt != null && dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                        {
                                            //강제출고이력 저장
                                            SaveForcedOutHist(sEqpID, sTrayID1, sTrayID2);

                                            Util.MessageInfo("FM_ME_0186"); //요청 완료하였습니다.
                                            //C1Window_Loaded(null, null);
                                        }
                                        else
                                            Util.Alert("FM_ME_0185"); //요청 실패하였습니다.
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                        //HiddenLoadingIndicator();
                                    }
                                    //finally
                                    //{
                                    //    loadingIndicator.Visibility = Visibility.Collapsed;
                                    //}
                                }, indataSet);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                        GetHistList();

                    });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //GetHistList();

                //SetBtnEnable(1, true);

                btnHistExport1.IsEnabled = true;
                btnHistExport2.IsEnabled = true;
                btnHistSearch.IsEnabled = true;
            }
        }

        private void SaveForcedOutHist(string sEqpID, string sTrayID1, string sTrayID2)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("ACTID", typeof(string));

                DataRow dr = inData.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["EQPTID"] = sEqpID;
                dr["USERID"] = LoginInfo.USERID;
                dr["ACTID"] = "UI_REQ_FORMEQP_FORC_ISS";
                inData.Rows.Add(dr);

                DataTable inCSTID = indataSet.Tables.Add("INCSTID");
                inCSTID.Columns.Add("CSTID", typeof(string));

                if (!string.IsNullOrWhiteSpace(sTrayID1))
                {
                    DataRow dr_Tray1 = inCSTID.NewRow();
                    dr_Tray1["CSTID"] = sTrayID1;
                    inCSTID.Rows.Add(dr_Tray1);
                }

                if (!string.IsNullOrWhiteSpace(sTrayID2))
                {
                    DataRow dr_Tray2 = inCSTID.NewRow();
                    dr_Tray2["CSTID"] = sTrayID2;
                    inCSTID.Rows.Add(dr_Tray2);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_EQPT_CONTROL_HIST", "INDATA,INCSTID", "OUTDATA", indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region tab2: 측정값 조회
        private void GetValueList(object[] internal_para)
        {
            try
            {
                SetBtnEnable(2, false);

                dgVerification.ClearRows();

                txtLane.Text = string.Empty;
                txtRow.Text = string.Empty;
                txtCol.Text = string.Empty;
                txtStg.Text = string.Empty;

                string sEqptID = string.Empty;
                string sVERIF_DTTM_ST = string.Empty;
                string sVERIF_DTTM_ED = string.Empty;

                DataTable dtRqst = null;
                DataRow dr = null;
                DataTable dtRslt = null;

                //_sCH_VLTG_ULMT = string.Empty;
                //_sCH_VLTG_LLMT = string.Empty;
                //_sCRNT_LINE_RATE_ULMT = string.Empty;
                //_sCRNT_LINE_RATE_LLMT = string.Empty;
                //_sVLTG_LINE_RATE_ULMT = string.Empty;
                //_sVLTG_LINE_RATE_LLMT = string.Empty;

                #region 내부 파라미터 사용 (Verification Kit 정보 조회 탭에서 이력 더블 클릭시)
                if (internal_para != null)
                {
                    sEqptID = Util.NVC(internal_para[0], null);

                    txtLane.Text = Util.NVC(internal_para[1]);
                    txtRow.Text = Util.NVC(internal_para[2]);
                    txtCol.Text = Util.NVC(internal_para[3]);
                    txtStg.Text = Util.NVC(internal_para[4]);

                    sVERIF_DTTM_ST = Util.NVC(internal_para[5], null);
                    sVERIF_DTTM_ED = Util.NVC(internal_para[6], null);

                    if (string.IsNullOrWhiteSpace(sEqptID))
                    {
                        //입력 데이터가 존재하지 않습니다.
                        Util.Alert("SFU1801");
                        return;
                    }

                    dtRqst = new DataTable();
                    //dtRqst.TableName = "INDATA";
                    //dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    //dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("VERIF_DTTM_ST", typeof(string));
                    dtRqst.Columns.Add("VERIF_DTTM_ED", typeof(string));

                    dr = dtRqst.NewRow();
                    //dr["SRCTYPE"] = "UI";
                    //dr["IFMODE"] = "OFF";
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = sEqptID;
                    dr["VERIF_DTTM_ST"] = sVERIF_DTTM_ST;
                    dr["VERIF_DTTM_ED"] = sVERIF_DTTM_ED;
                    dtRqst.Rows.Add(dr);

                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_BOX_VERIF_MEASR_HIST", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt == null || dtRslt.Rows.Count == 0)
                    {
                        //조회된 값이 없습니다.
                        Util.Alert("FM_ME_0232");
                        return;
                    }
                    else
                    {
                        Util.GridSetData(dgVerification, dtRslt, FrameOperation);

                        string[] sColumnName = new string[] { "EQPTID", "LOCATION_INFO", "LOTID" };
                        _Util.SetDataGridMergeExtensionCol(dgVerification, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
                #endregion
                #region 외부 파라미터 사용 (충방전기 세부현황에서 '측정값 조회' 버튼 클릭으로 화면 오픈시)
                else
                {
                    sEqptID = _sEqptID;

                    if (string.IsNullOrWhiteSpace(sEqptID))
                    {
                        //입력 데이터가 존재하지 않습니다.
                        Util.Alert("SFU1801");
                        return;
                    }

                    txtLane.Text = _sLaneID;
                    txtRow.Text = _sRow;
                    txtCol.Text = _sCol;
                    txtStg.Text = _sStg;

                    dtRqst = new DataTable();
                    //dtRqst.TableName = "INDATA";
                    //dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    //dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));

                    dr = dtRqst.NewRow();
                    //dr["SRCTYPE"] = "UI";
                    //dr["IFMODE"] = "OFF";
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = sEqptID;
                    dtRqst.Rows.Add(dr);

                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_BOX_VERIF_MEASR", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt == null || dtRslt.Rows.Count == 0)
                    {
                        //조회된 값이 없습니다.
                        Util.Alert("FM_ME_0232");
                        return;
                    }
                    else
                    {
                        Util.GridSetData(dgVerification, dtRslt, FrameOperation);

                        string[] sColumnName = new string[] { "EQPTID", "LOCATION_INFO", "LOTID" };
                        _Util.SetDataGridMergeExtensionCol(dgVerification, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
                #endregion

                #region '측정값 조회' 탭의 셀 로드 완료시 색상 변경을 위해 Recipe 값 임시저장
                //if (dtRslt != null && dtRslt.Rows.Count > 0)
                //{
                //    string sLotID = dtRslt.Rows[0]["LOTID"].ToString();

                //    DataTable dtRqst_Recipe = new DataTable();
                //    //dtRqst.TableName = "INDATA";
                //    //dtRqst_Recipe.Columns.Add("SRCTYPE", typeof(string));
                //    //dtRqst_Recipe.Columns.Add("IFMODE", typeof(string));
                //    //dtRqst_Recipe.Columns.Add("AREAID", typeof(string));
                //    //dtRqst_Recipe.Columns.Add("LANGID", typeof(string));
                //    dtRqst_Recipe.Columns.Add("LOTID", typeof(string));

                //    DataRow dr_Recipe = dtRqst_Recipe.NewRow();
                //    //dr_Recipe["SRCTYPE"] = "UI";
                //    //dr_Recipe["IFMODE"] = "OFF";
                //    //dr_Recipe["AREAID"] = LoginInfo.CFG_AREA_ID;
                //    //dr_Recipe["LANGID"] = LoginInfo.LANGID;
                //    dr_Recipe["LOTID"] = sLotID;
                //    dtRqst_Recipe.Rows.Add(dr_Recipe);

                //    DataTable dtRslt_Recipe = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_BOX_VERIF_RECIPE", "RQSTDT", "RSLTDT", dtRqst_Recipe);

                //    if (dtRslt_Recipe != null && dtRslt_Recipe.Rows.Count > 0)
                //    {
                //        _sCH_VLTG_ULMT = dtRslt_Recipe.Rows[0]["CH_VLTG_ULMT_VALUE"].ToString();
                //        _sCH_VLTG_LLMT = dtRslt_Recipe.Rows[0]["CH_VLTG_LLMT_VALUE"].ToString();
                //        _sCRNT_LINE_RATE_ULMT = dtRslt_Recipe.Rows[0]["CURNT_LINE_RATE_ULMT_VALUE"].ToString();
                //        _sCRNT_LINE_RATE_LLMT = dtRslt_Recipe.Rows[0]["CURNT_LINE_RATE_LLMT_VALUE"].ToString();
                //        _sVLTG_LINE_RATE_ULMT = dtRslt_Recipe.Rows[0]["VLTG_LINE_RATE_ULMT_VALUE"].ToString();
                //        _sVLTG_LINE_RATE_LLMT = dtRslt_Recipe.Rows[0]["VLTG_LINE_RATE_LLMT_VALUE"].ToString();
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                SetBtnEnable(2, true);
            }
        }
        #endregion

        #region tab3: 측정횟수 초기화
        private void GetMeasrCntList()
        {
            try
            {
                SetBtnEnable(3, false);

                dgMeasrCnt.ClearRows();

                txtTrayID_CntReset.Text = string.Empty;
                txtUseCnt_CntReset.Text = string.Empty;

                string sTrayID = txtTrayID_MeasrCnt.Text.ToString();

                if (string.IsNullOrWhiteSpace(sTrayID) || sTrayID.Length > 10)
                {
                    //Tray ID를 입력해주세요.
                    Util.Alert("FM_ME_0070");
                    return;
                }

                DataTable dtRqst_Cst = new DataTable();
                dtRqst_Cst.Columns.Add("AREAID", typeof(string));
                dtRqst_Cst.Columns.Add("CSTID", typeof(string));

                DataRow dr_Cst = dtRqst_Cst.NewRow();
                dr_Cst["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr_Cst["CSTID"] = sTrayID;
                dtRqst_Cst.Rows.Add(dr_Cst);

                DataTable dtResult_Cst = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_BOX_VERIF_CARRIER_USE_COUNT", "RQSTDT", "RSLTDT", dtRqst_Cst);

                if (dtResult_Cst == null || dtResult_Cst.Rows.Count == 0)
                {
                    //조회된 값이 없습니다.
                    Util.Alert("FM_ME_0232");
                    return;
                }
                else
                {
                    txtTrayID_CntReset.Text = dtResult_Cst.Rows[0]["CSTID"].ToString();
                    txtUseCnt_CntReset.Text = dtResult_Cst.Rows[0]["CURR_VERIF_CNT"].ToString();

                    //초기화 버튼 활성화 여부
                    if (CheckFormAuthority("FORM_FORMATION_MANA"))// && FrameOperation.AUTHORITY.Equals("W"))
                        btnCntReset.IsEnabled = true;
                    else
                        btnCntReset.IsEnabled = false;

                    DataTable dtRqst = new DataTable();
                    //dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["CSTID"] = sTrayID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_BOX_VERIF_CNT_HIST", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        //조회된 값이 없습니다.
                        //Util.Alert("FM_ME_0232");
                        //return;
                    }
                    else
                    {
                        Util.GridSetData(dgMeasrCnt, dtResult, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //SetBtnEnable(3, true);
                btnMeasrCntSearch.IsEnabled = true;
            }
        }

        private void SetMeasrCntReset()
        {
            try
            {
                SetBtnEnable(3, false);

                string sTrayID = txtTrayID_CntReset.Text;

                if (string.IsNullOrWhiteSpace(sTrayID))
                {
                    //조회를 먼저 해주세요.
                    Util.Alert("SFU8537");
                    return;
                }
                else
                {
                    #region 측정횟수 초기화
                    //초기화 하시겠습니까?
                    Util.MessageConfirm("SFU3440", result_confirm =>
                    {
                        if (result_confirm != MessageBoxResult.OK)
                        {
                            //GetMeasrCntList();

                            //return;
                        }
                        else
                        {
                            try
                            {
                                DataTable inDataTable = new DataTable();
                                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                                inDataTable.Columns.Add("IFMODE", typeof(string));
                                //inDataTable.Columns.Add("AREAID", typeof(string));
                                inDataTable.Columns.Add("USERID", typeof(string));
                                inDataTable.Columns.Add("ACTID", typeof(string));
                                inDataTable.Columns.Add("CSTID", typeof(string));

                                DataRow dr = inDataTable.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["ACTID"] = "USE_COUNT_RESET";
                                dr["CSTID"] = sTrayID;
                                inDataTable.Rows.Add(dr);

                                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_UPD_FORM_BOX_VERIF_CNT", "INDATA", "OUTDATA", inDataTable);

                                //if (dtRslt != null && dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                //{
                                //    Util.MessageInfo("FM_ME_0294"); //초기화가 완료되었습니다.
                                //}
                                //else
                                //{
                                //    Util.Alert("FM_ME_0295"); //초기화 도중 문제가 발생하였습니다.
                                //    //return;
                                //}

                                new ClientProxy().ExecuteService("BR_UPD_FORM_BOX_VERIF_CNT", "INDATA", "OUTDATA", inDataTable, (dtRslt, ex) =>
                                {
                                    if (dtRslt != null && dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                        Util.MessageInfo("FM_ME_0294"); //초기화가 완료되었습니다.
                                    else
                                        Util.Alert("FM_ME_0295"); //초기화 도중 문제가 발생하였습니다.

                                    //GetMeasrCntList();
                                });

                                //GetMeasrCntList();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                        GetMeasrCntList();

                    });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnMeasrCntSearch.IsEnabled = true;

                //SetBtnEnable(3, true);
            }
        }
        #endregion

        #region tab4: 검사 진행 현황
        private void GetNonTargetColor()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "VERIF_JUDG_RSLT";
                dr["CBO_CODE"] = "N";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    System.Drawing.Color forColor = System.Drawing.ColorTranslator.FromHtml(dtResult.Rows[0]["ATTRIBUTE2"].Nvc());
                    nonTargetBrushForColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(forColor.A, forColor.R, forColor.G, forColor.B));
                    System.Drawing.Color backColor = System.Drawing.ColorTranslator.FromHtml(dtResult.Rows[0]["ATTRIBUTE1"].Nvc());
                    nonTargetBrushBackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth, VerticalAlignment vertical)
        {
            try
            {
                if (dg.Columns.Contains(sColName)) return;

                dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Header = sColName,
                    Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = vertical,
                    TextWrapping = TextWrapping.NoWrap,
                    Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ScanProgressSearch()
        {
            try
            {
                cboLaneUnit.ClearValidation();
                if (cboLaneUnit.GetBindValue().Nvc().Equals(""))
                {
                    cboLaneUnit.SetValidation("SFU1651");
                    return;
                }
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = cboLaneUnit.GetBindValue("LANE_ID");
                dtRqst.Rows.Add(dr);

                // 멀티 스레드 실행, 다음은 순서대로 dgScanProgress_ExecuteCustomBinding, dgScanProgress_ExecuteDataCompleted 실행 됨.
                dgScanProgress.ExecuteService("DA_SEL_FORMATION_VERIF_VIEW", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetVerifFormation(DataTable dtRslt)
        {
            try
            {
                if (cellInfos == null) cellInfos = new Dictionary<Point, CellInfo>();
                cellInfos.Clear();

                dgScanProgress.ItemsSource = null;
                dgScanProgress.Columns.Clear();
                dgScanProgress.ClearBorderLine();
                dgScanProgress.AlternatingRowBackground = System.Windows.Media.Brushes.White;

                List<string> rowList = dtRslt.AsEnumerable().Select(s => s.Field<string>("ROW")).Distinct().ToList();
                int trayLoadingStep = dtRslt.AsEnumerable().Select(s => s.Field<string>("CST_LOAD_LOCATION_CODE")).Max().NvcInt();
                int danCount = dtRslt.AsEnumerable().Select(s => s.Field<string>("STG")).Max().NvcInt();
                int danRowCount = trayLoadingStep == 2 ? danCount * 2 : danCount;
                int yunColumnCount = dtRslt.AsEnumerable().Select(s => s.Field<string>("COL")).Max().NvcInt();
                int columnCount = yunColumnCount + 1;
                if (rowList.Count == 1 && yunColumnCount > 14)
                {
                    columnCount = yunColumnCount / 2 + (yunColumnCount % 2 == 0 ? 0 : 1) + 1;
                }

                double columnWidth = (dgScanProgress.ActualWidth - 50) / (columnCount - 1);
                columnWidth = columnWidth - (columnWidth % 1);
                double deltaWidth = (dgScanProgress.ActualWidth - 50 - (columnWidth * (columnCount - 1))) - 1;
                deltaWidth = deltaWidth - (deltaWidth % 1);

                dtScanProgress = new DataTable("ScanProgress");

                for (int i = 0; i < columnCount; i++)
                {
                    if (i == 0)
                    {
                        SetGridHeaderSingle((i + 1).ToString(), dgScanProgress, 50 + deltaWidth, VerticalAlignment.Center);
                    }
                    else
                    {
                        SetGridHeaderSingle((i + 1).ToString(), dgScanProgress, columnWidth, VerticalAlignment.Center);
                    }

                    dtScanProgress.Columns.Add((i + 1).ToString(), typeof(string));
                }

                dtScanProgress.Columns.Add("ROW_TYPE", typeof(string));

                dtScanProgress.Rows.Clear();

                int rowGroupCount = rowList.Count;
                if (rowList.Count == 1) rowGroupCount = yunColumnCount / columnCount + (yunColumnCount % columnCount == 0 ? 0 : 1);

                for (int level = 0; level < rowGroupCount; level++)
                {
                    int rowHeader = 0;
                    int rowDan = danRowCount;
                    int rowListIndex = rowList.Count == 1 ? 0 : level;
                    string cstLocation = string.Empty;
                    for (int row = 0; row < danRowCount + 1; row++)
                    {
                        DataRow drRow = dtScanProgress.NewRow();
                        for (int col = 0; col < columnCount; col++)
                        {
                            if (row == 0 && col == 0)
                            {
                                if (rowList.Count > 1) drRow[col] = rowList[rowListIndex].Nvc() + langYul;
                                drRow["ROW_TYPE"] = "HEADER";
                            }
                            else if (row == 0)
                            {
                                drRow[col] = ((columnCount * level) + (col - level)).ToString() + langYun;
                                if (rowList.Count > 1) drRow[col] = col.ToString() + langYun;
                            }
                            else if (row > 0 && col == 0)
                            {
                                rowHeader = (rowDan + trayLoadingStep - 1) / trayLoadingStep;
                                drRow[col] = (rowHeader).ToString() + langDan;
                                cstLocation = ((rowDan + trayLoadingStep - 1) % 2 + 1).Nvc();
                                rowDan--;
                            }
                            else
                            {
                                Brush forground = Brushes.Black;
                                Brush background = Brushes.White;

                                string yul = rowList[rowListIndex];
                                string yun = ((columnCount * level) + (col - level)).ToString("00");
                                if (rowList.Count > 1) yun = col.ToString("00");
                                string dan = rowHeader.ToString("00");

                                DataRow drSelect = dtRslt.AsEnumerable().Where(w => w.Field<string>("ROW").Equals(yul) &&
                                        w.Field<string>("COL").Equals(yun) && w.Field<string>("STG").Equals(dan) &&
                                        w.Field<string>("CST_LOAD_LOCATION_CODE").Equals(cstLocation)).FirstOrDefault();

                                if (drSelect != null)
                                {
                                    drRow[col] = drSelect["VERIF_RSLT_NAME"].Nvc();

                                    System.Drawing.Color forColor = System.Drawing.ColorTranslator.FromHtml(drSelect["VERIF_RSLT_FORCOLOR"].Nvc());
                                    forground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(forColor.A, forColor.R, forColor.G, forColor.B));
                                    System.Drawing.Color backColor = System.Drawing.ColorTranslator.FromHtml(drSelect["VERIF_RSLT_BACKCOLOR"].Nvc());
                                    background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B));

                                    CellInfo cellInfo = new CellInfo(forground, background);
                                    cellInfo.LocationCode = drSelect["CST_LOAD_LOCATION_CODE"].Nvc();

                                    if (!drSelect["VERIF_RSLT_CODE"].Nvc().Equals("V") && !string.IsNullOrWhiteSpace(drSelect["VERIF_EQPTID"].Nvc()))
                                    {
                                        cellInfo.VerifEqptId = drSelect["VERIF_EQPTID"].Nvc();
                                    }

                                    if (drSelect["VERIF_RSLT_CODE"].Nvc().Equals("V"))
                                    {
                                        cellInfo.IsProgress = true;
                                        cellInfos.Add(new Point(dtScanProgress.Rows.Count, col), cellInfo);
                                    }
                                    else
                                    {
                                        cellInfos.Add(new Point(dtScanProgress.Rows.Count, col), cellInfo);
                                    }
                                }
                                else
                                {
                                    drRow[col] = ObjectDic.Instance.GetObjectName("NON_TARGET");

                                    forground = nonTargetBrushForColor;
                                    background = nonTargetBrushBackColor;

                                    CellInfo cellInfo = new CellInfo(forground, background);
                                    cellInfos.Add(new Point(dtScanProgress.Rows.Count, col), cellInfo);
                                }

                                #region Border Setting
                                CellBorderLineInfo cellBorder = new CellBorderLineInfo();
                                cellBorder.LeftBorderLineInfo = new BorderLineInfo(1, lineBrush);
                                cellBorder.RightBorderLineInfo = new BorderLineInfo(1, lineBrush);

                                if (cstLocation.Equals("1"))
                                {
                                    cellBorder.BottomBorderLineInfo = new BorderLineInfo(1, lineBrush);
                                }
                                else
                                {
                                    cellBorder.TopBorderLineInfo = new BorderLineInfo(1, lineBrush);
                                    cellBorder.BottomBorderLineInfo = new BorderLineInfo(1, lineBrush, BorderLineStyle.Dot);
                                }

                                if (row == 1) cellBorder.TopBorderLineInfo = new BorderLineInfo(2, lineBrush);
                                if (row == danRowCount) cellBorder.BottomBorderLineInfo = new BorderLineInfo(2, lineBrush);
                                if (col == 1) cellBorder.LeftBorderLineInfo = new BorderLineInfo(2, lineBrush);
                                if (col == columnCount - 1) cellBorder.RightBorderLineInfo = new BorderLineInfo(2, lineBrush);

                                dgScanProgress.SetBorderLine(dtScanProgress.Rows.Count, col, cellBorder);
                                #endregion
                            }
                        }
                        dtScanProgress.Rows.Add(drRow);
                    }
                }

                dgScanProgress.ItemsSource = DataTableConverter.Convert(dtScanProgress);

                double rowHeight = dgScanProgress.ActualHeight / ((danRowCount + 1) * rowGroupCount);
                rowHeight = rowHeight - (rowHeight % 1);
                double deltaHeight = (dgScanProgress.ActualHeight - (rowHeight * ((danRowCount + 1) * rowGroupCount))) / rowGroupCount - 1;
                deltaHeight = deltaHeight - (deltaHeight % 1);

                for (int row = 0; row < dgScanProgress.Rows.Count; row++)
                {
                    if (dgScanProgress.GetValue(row, "ROW_TYPE").Nvc().Equals("HEADER"))
                    {
                        dgScanProgress.Rows[row].Height = new C1.WPF.DataGrid.DataGridLength(rowHeight + deltaHeight, DataGridUnitType.Pixel);
                    }
                    else
                    {
                        dgScanProgress.Rows[row].Height = new C1.WPF.DataGrid.DataGridLength(rowHeight, DataGridUnitType.Pixel);
                    }
                }

                string[] sColumnName = new string[] { "1" };
                _Util.SetDataGridMergeExtensionCol((C1DataGrid)dgScanProgress, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SkipProcess()
        {
            try
            {
                List<string> processCell = new List<string>();
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgScanProgress.Selection.SelectedCells)
                {
                    Point cellPoint = new Point(cell.Row.Index, cell.Column.Index);
                    if (cellInfos.ContainsKey(cellPoint))
                    {
                        if (!string.IsNullOrWhiteSpace(cellInfos[cellPoint].VerifEqptId)) processCell.Add(cellInfos[cellPoint].VerifEqptId);
                    }
                }

                processCell = processCell.Distinct().ToList();
                if (processCell.Count == 0)
                {
                    Util.AlertInfo("FM_ME_0240");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow drNew = dtRqst.NewRow();
                drNew["EQPTID"] = string.Join(",", processCell);
                drNew["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_UPD_FORMATION_VERIF_SKIP", "RQSTDT", null, dtRqst);
                if (dtResult != null)
                {
                    ScanProgressSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ResetProcess()
        {
            try
            {
                List<string> processCell = new List<string>();
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgScanProgress.Selection.SelectedCells)
                {
                    Point cellPoint = new Point(cell.Row.Index, cell.Column.Index);
                    if (cellInfos.ContainsKey(cellPoint))
                    {
                        if (!string.IsNullOrWhiteSpace(cellInfos[cellPoint].VerifEqptId)) processCell.Add(cellInfos[cellPoint].VerifEqptId);
                    }
                }

                processCell = processCell.Distinct().ToList();
                if (processCell.Count == 0)
                {
                    Util.AlertInfo("FM_ME_0240");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow drNew = dtRqst.NewRow();
                drNew["EQPTID"] = string.Join(",", processCell);
                drNew["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_UPD_FORMATION_VERIF_RESET", "RQSTDT", null, dtRqst);
                if (dtResult != null)
                {
                    ScanProgressSearch();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

    }
}
