/*************************************************************************************
 Created Date : 2020.10.08
      Creator : 신광희
   Decription : STK 재고실사
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.08  신광희 차장 : Initial Created. 
  2020.11.16  안인효
  2020.12.22  안인효      : (이영준 선임 요청)
                            실사대상조회 탭에서 
                            창고 적재 현황에서 전체 컬럼을 선택하여 
                            우측 그리드에 조회한 후 우측 그리드에서 체크박스 전체 선택한 경우
                            BR_MCS_REG_STK_STCK_CNT_SNAP 비즈 INDATA 파라미터 NOTE = ‘ALL’ 로 저장
  2024.05.24  윤지해      : (수정사항 없음) NERP 대응 프로젝트-차수마감 취소 등 개발 범위에서 제외(대상테이블 아님)
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_048.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_048 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //private readonly 
        private readonly Util _util = new Util();
        private string _selectedEquipmentCode;
        private string _selectedLotElectrodeTypeCode;
        private string _selectedEquipmentCodeReflash;
        private string _selectedCarrierStat;
        private string _selectedPart;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private bool _isGradeJudgmentDisplay;
        private int _maxRowCount;
        private int _maxColumnCount;

        CommonCombo _combo = new CommonCombo();

        public MCS001_048()
        {
            InitializeComponent();
        }
        #endregion


        #region Initialize

        /// <summary>
        /// 창고유형
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO_STK";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        /// <summary>
        /// 창고
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.Empty;
            string electrodeType = string.Empty;

            if ((cbo as C1ComboBox).Name == "cboStocker1")
            {
                stockerType = string.IsNullOrEmpty(cboStockerType1.SelectedValue.GetString()) ? null : cboStockerType1.SelectedValue.GetString();
                electrodeType = string.IsNullOrEmpty(cboElectrodeType1.SelectedValue.GetString()) ? null : cboElectrodeType1.SelectedValue.GetString();
            }
            else if ((cbo as C1ComboBox).Name == "cboStocker2")
            {
                stockerType = string.IsNullOrEmpty(cboStockerType2.SelectedValue.GetString()) ? null : cboStockerType2.SelectedValue.GetString();
                electrodeType = string.IsNullOrEmpty(cboElectrodeType2.SelectedValue.GetString()) ? null : cboElectrodeType2.SelectedValue.GetString();
            }
            else if ((cbo as C1ComboBox).Name == "cboStocker3")
            {
                stockerType = string.IsNullOrEmpty(cboStockerType3.SelectedValue.GetString()) ? null : cboStockerType3.SelectedValue.GetString();
                electrodeType = string.IsNullOrEmpty(cboElectrodeType3.SelectedValue.GetString()) ? null : cboElectrodeType3.SelectedValue.GetString();
            }


            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType, electrodeType };

            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            if ((cbo as C1ComboBox).Name != "cboStocker3")
            {
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
            }
            else
            {
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
            }
        }

        /// <summary>
        /// 극성
        /// </summary>
        /// <param name="cbo"></param>
        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion


        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch1, btnDegreeAdd, btnSearch2, btnForcedClose, btnRefresh, btnSearch3 };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeCombo();

            Loaded -= UserControl_Loaded;
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            //_scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
        }


        #region ComboBox

        /// <summary>
        /// 창고유형 선택 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if ((sender as C1ComboBox).Name == "cboStockerType1")
                {
                    ClearControl("1");
                    cboStocker1.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                    SetStockerCombo(cboStocker1);
                    cboStocker1.SelectedValueChanged += cboStocker_SelectedValueChanged;
                }
                else if ((sender as C1ComboBox).Name == "cboStockerType2")
                {
                    ClearControl("2");
                    cboStocker2.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                    SetStockerCombo(cboStocker2);
                    cboStocker2.SelectedValueChanged += cboStocker_SelectedValueChanged;
                }
                else if ((sender as C1ComboBox).Name == "cboStockerType3")
                {
                    ClearControl("3");
                    //cboStocker3.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                    SetStockerCombo(cboStocker3);
                    //cboStocker3.SelectedValueChanged += cboStocker_SelectedValueChanged;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }           
        }

        /// <summary>
        /// 극성 선택 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if ((sender as C1ComboBox).Name == "cboElectrodeType1")
            {
                cboStocker1.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                SetStockerCombo(cboStocker1);
                cboStocker1.SelectedValueChanged += cboStocker_SelectedValueChanged;
            }
            else if ((sender as C1ComboBox).Name == "cboElectrodeType2")
            {
                cboStocker2.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                SetStockerCombo(cboStocker2);
                cboStocker2.SelectedValueChanged += cboStocker_SelectedValueChanged;
            }
            else if ((sender as C1ComboBox).Name == "cboElectrodeType3")
            {
                cboStocker3.SelectedValueChanged -= cboStocker_SelectedValueChanged;
                SetStockerCombo(cboStocker3);
                cboStocker3.SelectedValueChanged += cboStocker_SelectedValueChanged;
            }
        }

        /// <summary>
        /// 창고 선택 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string strTapStep = string.Empty;

            switch ((sender as C1ComboBox).Name)
            {
                case "cboStocker1": strTapStep = "1"; break;
                case "cboStocker2": strTapStep = "2"; break;
                case "cboStocker3": strTapStep = "3"; break;
                default: break;
            }

            ClearControl(strTapStep);

            // 실사이력탭에만 적용
            if (strTapStep == "3")
            {
                if (cboStocker3.SelectedValue != null)
                {
                    if (cboStocker3.SelectedValue.ToString() != "SELECT")
                    {
                        SetStockSeqShotCombo(cboStockSeqShot);
                    }
                    else
                    {
                        if (cboStockSeqShot.Items.Count > 0)
                        {
                            SetStockSeqShotCombo(cboStockSeqShot);
                        }
                    }
                }
            }
        }

        #endregion


        #region dgCapacitySummary

        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "RACK_QTY") || string.Equals(e.Cell.Column.Name, "BBN_U_QTY") || string.Equals(e.Cell.Column.Name, "BBN_E_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "XXXXXXXXXX") && !string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Aqua");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }


        private void dgCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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


        private void dgCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        Count = g.Count()
                    }).ToList();

                    string previewElectrodeTypeCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode && previewElectrodeTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ELTR_TYPE_NAME"].Index)));
                                }
                            }
                        }
                        previewElectrodeTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 신규 차수를 생성할 창고 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                chkHeaderAll.IsChecked = false;
                _selectedPart = string.Empty;

                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedLotElectrodeTypeCode = null;

                if (cell.Column.Name.Equals("BBN_U_QTY") || cell.Column.Name.Equals("BBN_E_QTY") || cell.Column.Name.Equals("RACK_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                        if (cell.Column.Name.Equals("BBN_U_QTY"))       // 실Carrier
                        {
                            _selectedCarrierStat = "U";
                        }
                        else if (cell.Column.Name.Equals("BBN_E_QTY"))      // 공Carrier
                        {
                            _selectedCarrierStat = "E";
                        }
                        else if (cell.Column.Name.Equals("RACK_QTY"))
                        {
                            _selectedCarrierStat = null;
                            _selectedPart = "T";
                        }
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    Util.gridClear(dgProduct);

                    //차수가 진행중인 창고는 선택 불가
                    if (DataTableConverter.GetValue(drv, "STCK_CNT_CMPL_FLAG").GetString() == "N")
                    {
                        Util.MessageInfo("SFU5163");  // 선택항목의 현 진행상태코드를 확인해주세요.   (진행중인 차수는 선택할 수 없습니다 )
                        return;
                    }

                    SelectWareHouseProductList(dgProduct);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region dgProduct

        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                    if (_isscrollToHorizontalOffset)
                    {
                        dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("LOTID"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        #endregion


        /// <summary>
        /// 조회 - 창고 적재 현황 (좌측 Grid)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sWorkType = string.Empty;
                if ((sender as Button).Name == "btnSearch1")
                {
                    sWorkType = "S";

                    ClearControl("1");
                }
                else if ((sender as Button).Name == "btnSearch2")
                {
                    sWorkType = "C";

                    ClearControl("2");
                }
                else if ((sender as Button).Name == "btnSearch3")
                {
                    if (cboStockSeqShot.Items.Count == 0)
                    {
                        Util.MessageInfo("SFU2958"); //차수는 필수입니다.
                        return;
                    }
                    ClearControl("3");
                }


                //if (cboStockerType1.SelectedValue.GetString() == "LWW")
                //{
                //    SelectWareHouseLamiSummary();
                //}
                //else
                //{
                if ((sender as Button).Name != "btnSearch3")
                {
                    SelectWareHouseCapacitySummary(sWorkType);
                }
                else
                {
                    dgChoice_Checked();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 차수생성 - 실사시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
        {
            const string bizRuleName = "BR_MCS_REG_STK_STCK_CNT_SNAP";
            try
            {
                if (dgProduct.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU2961"); //창고를 먼저 선택해주세요..
                    return;
                }

                //차수를 추가하시겠습니까??
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2959"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("AREAID", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("NOTE", typeof(string));

                        DataTable inOutCst = inDataSet.Tables.Add("OUT_CST");
                        inOutCst.Columns.Add("CSTID", typeof(string));

                        DataRow dr = inTable.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["EQPTID"] = _selectedEquipmentCode;
                        dr["USERID"] = LoginInfo.USERID;
                        if ((_selectedPart == "T") && (chkHeaderAll.IsChecked == true))
                        {
                            dr["NOTE"] = "ALL";
                        }
                        else
                        {
                            dr["NOTE"] = "";
                        }
                        inTable.Rows.Add(dr);

                        for (int _iRow = 0; _iRow < dgProduct.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgProduct.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                dr = inOutCst.NewRow();
                                dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProduct.Rows[_iRow].DataItem, "OUTER_CSTID"));
                                inOutCst.Rows.Add(dr);
                            }
                        }

                        if (inOutCst.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,OUT_CST", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    HiddenLoadingIndicator();

                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    //this.DialogResult = MessageBoxResult.OK;
                                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                                    btnSearch_Click(btnSearch1, null);
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                }
                            }, inDataSet);
                        }
                        else
                        {
                            HiddenLoadingIndicator();
                            Util.Alert("SFU1645");  //선택된 작업대상이 없습니다..
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = dgCapacitySummary;
                double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                if (ContentsRow.ColumnDefinitions[0].Width.Value > sumWidth)
                {
                    ContentsRow.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 실사대상조회 탭의 차수 생성 datagrid 전체선택 지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgProduct);
        }


        /// <summary>
        /// 실사대상조회 탭의 차수 생성 datagrid 전체선택 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProduct);
        }


        /// <summary>
        /// [실사 마감] 새로고침
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgProduct2) || !CommonVerify.HasDataGridRow(dgInspectionList))
                {
                    Util.MessageInfo("SFU2951"); //조회결과가 없습니다.
                    return;
                }

                _selectedEquipmentCodeReflash = string.Empty;

                for (int _iRow = 0; _iRow < dgProduct2.Rows.Count; _iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                    {
                        _selectedEquipmentCodeReflash = Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "EQPTID"));
                        break;
                    }
                }

                btnSearch_Click(btnSearch2, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// [실사 마감] 창고별 실사 현황 그리드의 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            const string bizRuleName = "DA_MCS_SEL_STK_STCK_CNT_RSLT_COMPARE";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("STCK_CNT_YM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("WHID", typeof(string));
                inTable.Columns.Add("STCK_CNT_SEQNO", typeof(Int32));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;


                Util.gridClear(dgInspectionList);

                dr["STCK_CNT_YM"] = DateTime.Now.ToString("yyyyMM");

                for (int _iRow = 0; _iRow < dgProduct2.Rows.Count; _iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                    {
                        dr["WHID"] = Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "EQPTID"));
                        dr["STCK_CNT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "STCK_CNT_SEQNO"));
                        inTable.Rows.Add(dr);
                        break;
                    }
                }

                if (inTable.Rows.Count != 0)
                {
                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgInspectionList, bizResult, null, true);
                    });
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 실사이력 조회 버튼
        /// </summary>
        private void dgChoice_Checked()
        {

            const string bizRuleName = "DA_MCS_SEL_STK_STCK_CNT_RSLT_COMPARE";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("STCK_CNT_YM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("WHID", typeof(string));
                inTable.Columns.Add("STCK_CNT_SEQNO", typeof(Int32));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;


                Util.gridClear(dgProduct3);

                dr["STCK_CNT_YM"] = ldpMonthShot.SelectedDateTime.ToString("yyyyMM");
                dr["WHID"] = cboStocker3.SelectedValue.GetString();
                dr["STCK_CNT_SEQNO"] = cboStockSeqShot.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProduct3, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// [실사 마감] 실사취소 (강제 종료처리)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnForcedClose_Click(object sender, RoutedEventArgs e)
        {
            const string bizRuleName = "DA_MCS_UPD_STK_STCK_CNT_ORD_U";

            try
            {
                //작업을 취소하시겠습니까? - 강제종료 (FORCED_CLOSE)
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1168"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable inTable = new DataTable("RQSTDT");
                        inTable.Columns.Add("STCK_CNT_YM", typeof(string));
                        inTable.Columns.Add("AREAID", typeof(string));
                        inTable.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));
                        inTable.Columns.Add("STCK_CNT_CMPL_TYPE", typeof(string));
                        inTable.Columns.Add("STCK_CNT_CMPL_DTTM", typeof(DateTime));
                        inTable.Columns.Add("UPDUSER", typeof(string));
                        inTable.Columns.Add("UPDDTTM", typeof(DateTime));
                        inTable.Columns.Add("STCK_CNT_NOTE", typeof(string));
                        inTable.Columns.Add("WHID", typeof(string));
                        inTable.Columns.Add("STCK_CNT_SEQNO", typeof(Int32));

                        DataRow dr = inTable.NewRow();
                        //dr["LANGID"] = LoginInfo.LANGID;
                        dr["STCK_CNT_YM"] = DateTime.Now.ToString("yyyyMM");
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["STCK_CNT_CMPL_FLAG"] = "Y";
                        dr["STCK_CNT_CMPL_TYPE"] = "FORCED_CLOSE";
                        dr["STCK_CNT_CMPL_DTTM"] = System.DateTime.Now;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        //dr["STCK_CNT_NOTE"] = "";

                        for (int _iRow = 0; _iRow < dgProduct2.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                if (string.Compare(Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "STCK_CNT_CMPL_FLAG")), "Y") == 0)
                                {
                                    Util.MessageInfo("SFU1172"); //이미 마감처리 되었습니다.
                                    return;
                                }
                                dr["WHID"] = Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "EQPTID"));
                                dr["STCK_CNT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgProduct2.Rows[_iRow].DataItem, "STCK_CNT_SEQNO"));
                                inTable.Rows.Add(dr);
                                break;
                            }
                        }

                        if (inTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                            {
                                try
                                {
                                    HiddenLoadingIndicator();

                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    Util.MessageInfo("SFU1275"); // 정상처리되었습니다.
                                    btnSearch_Click(btnSearch2, null);
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                }
                            });
                        }
                        else
                        {
                            Util.MessageInfo("SFU2961"); //창고를 먼저 선택해주세요..
                            return;
                        }
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
        /// [실사 마감] 실사 결과 그리드의 비교 결과에 대한 background color 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgInspectionList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Presenter != null)
                    {
                        //if (!(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUTER_CSTID")) == Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SCAN_OUTER_CSTID"))
                        //   && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTID")) == Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SCAN_CSTID"))))
                        if(!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STCK_CNT_RSLT_VALUE")).Equals("OK"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 달력변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Util.gridClear(dgProduct3);

                if (cboStocker3.SelectedValue != null)
                {
                    if (cboStocker3.SelectedValue.ToString() != "SELECT")
                    {
                        SetStockSeqShotCombo(cboStockSeqShot);
                    }
                    else
                    {
                        if (cboStockSeqShot.Items.Count > 0)
                        {
                            SetStockSeqShotCombo(cboStockSeqShot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgProduct))
                {
                    _isscrollToHorizontalOffset = true;
                    _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
                }

                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 차수변경시 그리드 Clear
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                Util.gridClear(dgProduct3);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 그리드 개별 선택에 따른 전체 선택 CheckBox를 Checked or Unchecked 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bool bExistFalse = false;

                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                //int rowIdx = cell.Row.Index;
                //DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                //if (drv == null) return;

                if (cell.Column.Name.Equals("CHK"))
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").GetString(), "False"))
                        {
                            chkHeaderAll.Unchecked -= chkHeaderAll_Unchecked;
                            chkHeaderAll.IsChecked = false;
                            chkHeaderAll.Unchecked += chkHeaderAll_Unchecked;
                            break;
                        }
                        else if (string.Equals(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").GetString(), "FALSE"))
                        {
                            bExistFalse = true;
                        }

                        if ((i == dg.Rows.Count - 1) && (bExistFalse == false))
                        {
                            chkHeaderAll.Checked -= chkHeaderAll_Checked;
                            chkHeaderAll.IsChecked = true;
                            chkHeaderAll.Checked += chkHeaderAll_Checked;
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


        #region Method

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsGradeJudgmentDisplay()
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ClearControl(string strTapStep)
        {
            try
            {
                _selectedEquipmentCode = string.Empty;
                _selectedCarrierStat = string.Empty;
                _selectedLotElectrodeTypeCode = string.Empty;

                if (strTapStep == "1")
                {
                    Util.gridClear(dgCapacitySummary);
                    Util.gridClear(dgProduct);

                    chkHeaderAll.IsChecked = false;
                }
                else if (strTapStep == "2")
                {
                    Util.gridClear(dgProduct2);
                    Util.gridClear(dgInspectionList);
                }
                else if (strTapStep == "3")
                {
                    Util.gridClear(dgProduct3);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeGrid()
        {
            //dgProductSummary.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            //dgProductSummary.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

            if (_isGradeJudgmentDisplay)
            {
                //dgProduct.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
            }
        }

        private void InitializeCombo()
        {
            // Area 콤보박스
            //SetAreaCombo(cboArea);

            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerType1);
            SetStockerTypeCombo(cboStockerType2);
            SetStockerTypeCombo(cboStockerType3);

            // Stocker 콤보박스
            //SetStockerCombo(cboStocker1);
            //SetStockerCombo(cboStocker2);
            //SetStockerCombo(cboStocker3);

            // 극성 콤보박스
            //SetElectrodeTypeCombo(cboElectrodeType1);
            //SetElectrodeTypeCombo(cboElectrodeType2);
            //SetElectrodeTypeCombo(cboElectrodeType3);
        }


        /// <summary>
        /// [창고 적재 현황]
        /// </summary>
        private void SelectWareHouseCapacitySummary(string sWorkType)
        {
            string bizRuleName = string.Empty;
            if (sWorkType == "S")
            {
                bizRuleName = "DA_MCS_SEL_WAREHOUSE_CAPACITY_SUMMARY_STK";
            }
            else
            {
                bizRuleName = "DA_MCS_SEL_WAREHOUSE_CAPACITY_SUMMARY_STK_LAST";
            }

            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();


                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //cboArea.SelectedValue;
                dr["EQGRID"] = (sWorkType == "S") ? cboStockerType1.SelectedValue : cboStockerType2.SelectedValue;
                //dr["ELTR_TYPE_CODE"] = (sWorkType == "S") ? cboElectrodeType1.SelectedValue : cboElectrodeType2.SelectedValue;
                dr["EQPTID"] = (sWorkType == "S") ? cboStocker1.SelectedValue : cboStocker2.SelectedValue;
                //dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    dtResult = bizResult;
                    if (sWorkType == "C")
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (dtResult.Rows[i]["EQPTID"].ToString() == _selectedEquipmentCodeReflash)
                            {
                                dtResult.Rows[i]["CHK"] = "True";
                                break;
                            }
                        }
                    }

                    if (sWorkType == "S")
                    {
                        Util.GridSetData(dgCapacitySummary, dtResult, null, true);
                    }
                    else
                    {
                        Util.GridSetData(dgProduct2, dtResult, null, true);

                        _selectedEquipmentCodeReflash = string.Empty;
                    }

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// [창고 적재 현황] 창고의 차수 선택
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseProductList(C1DataGrid dg)
        {
            const string bizRuleName = "DA_MCS_SEL_STK_STCK_CNT_SNAP";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CSTSTAT"] = _selectedCarrierStat;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 실사이력 차수 콤보
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockSeqShotCombo(C1ComboBox cbo)
        {
            try
            {
                string stocker = string.Empty;
                stocker = cboStocker3.SelectedValue.GetString();

                const string bizRuleName = "DA_MCS_SEL_STK_STCK_CNT_ORD_CLOSE_CBO";
                string[] arrColumn = { "LANGID", "STCK_CNT_YM", "AREAID", "WHID" };
                string[] arrCondition = { LoginInfo.LANGID, ldpMonthShot.SelectedDateTime.ToString("yyyyMM"), LoginInfo.CFG_AREA_ID, stocker };

                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private double GetRackRate(decimal x, decimal y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private double GetRackRate(int x, int y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
        }

        private void SelectMaxxyz()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _selectedEquipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    if (string.IsNullOrEmpty(searchResult.Rows[0][2].GetString()) || string.IsNullOrEmpty(searchResult.Rows[0][1].GetString()))
                    {
                        _maxRowCount = 0;
                        _maxColumnCount = 0;
                        return;
                    }

                    _maxRowCount = Convert.ToInt32(searchResult.Rows[0][2].GetString());
                    _maxColumnCount = Convert.ToInt32(searchResult.Rows[0][1].GetString());
                }
                else
                {
                    _maxRowCount = 0;
                    _maxColumnCount = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ActualWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex) * averageScrollWidth);
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        #endregion

    }
}