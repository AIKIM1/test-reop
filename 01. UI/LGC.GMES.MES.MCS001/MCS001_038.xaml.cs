/*************************************************************************************
 Created Date : 2020.01.08
      Creator : 정문교
   Decription : STK 재공현황 Summary
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.08  정문교 : Initial Created.
  2020.01.31  정문교 : 오류Carrier 컬럼을 추가   
  2020.02.06  정문교 : Carrier가 없는 창고도 조회되게 수정  
  2020.02.12  정문교 : 1.조회 조건에 조회구분 추가
                       2.라미대기창고 제외 : 조회구분 활성화 (Default는 권취별)
                         라미대기창고      : 조회구분 비활성화 (가용구분별 표시로 고정)
                       3.라미대기창고 제외 가용구분별 조회 그리드 추가 
  
      
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_038.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_038 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private const string _LamiWaitWarehouse = "LWW";            // 라미대기창고


        public MCS001_038()
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
            dgStockListL.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgStockListL.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStockListU.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgStockListU.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStockListE.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgStockListE.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStockListE.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);
            dgModelListL.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgModelListL.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgModelListU.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgModelListU.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgModelListE.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
            dgModelListE.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgModelListE.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

            if (((FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbStock"))
            {
                Util.gridClear(dgStockListE);
                Util.gridClear(dgStockListU);
                Util.gridClear(dgStockListL);
            }
            else
            {
                Util.gridClear(dgModelListE);
                Util.gridClear(dgModelListU);
                Util.gridClear(dgModelListL);
            }
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();
            ////////////////////////////////////////////////// 창고
            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            // 창고 유형 콤보박스
            SetStockerTypeCombo(cboStockerType, cboArea);
            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);
            // Stocker 콤보박스
            SetStockerCombo(cboStocker, cboArea);
            // 조회구분
            string[] sFilter = new string[] { "WAREHOUSE_STOCK_SEARCH_TYPE" };
            _combo.SetCombo(cboSearchType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);

            ////////////////////////////////////////////////// 모델
            //동
            _combo.SetCombo(cboAreaModel, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            // 창고 유형 콤보박스
            SetStockerTypeCombo(cboStockerTypeModel, cboAreaModel);
            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeTypeModel);
            // Stocker 콤보박스
            cboStockerModel.ApplyTemplate();
            SetStockerCombo(cboStockerModel, cboAreaModel);
            // 조회구분
            sFilter = new string[] { "WAREHOUSE_STOCK_SEARCH_TYPE" };
            _combo.SetCombo(cboSearchTypeModel, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
        }

        private void SetControl(bool isVisibility = false)
        {
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboStockerType.SelectedValueChanged += cboStockerType_SelectedValueChanged;
            cboElectrodeType.SelectedValueChanged += cboElectrodeType_SelectedValueChanged;
            cboSearchType.SelectedValueChanged += cboSearchType_SelectedValueChanged;

            cboAreaModel.SelectedValueChanged += cboAreaModel_SelectedValueChanged;
            cboStockerTypeModel.SelectedValueChanged += cboStockerTypeModel_SelectedValueChanged;
            cboElectrodeTypeModel.SelectedValueChanged += cboElectrodeTypeModel_SelectedValueChanged;
            cboSearchTypeModel.SelectedValueChanged += cboSearchTypeModel_SelectedValueChanged;

            cboSearchType.SelectedValue = "M";
            cboSearchTypeModel.SelectedValue = "M";
        }

        #endregion


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            InitializeGrid();
            InitializeCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        #region 창고
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStockerType.SelectedValueChanged -= cboStockerType_SelectedValueChanged;
            SetStockerTypeCombo(cboStockerType, cboArea);
            SetStockerCombo(cboStocker, cboArea);
            cboStockerType.SelectedValueChanged += cboStockerType_SelectedValueChanged;
        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchType();
            SetGridVisibility();
            SetStockerCombo(cboStocker, cboArea);
        }
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetStockerCombo(cboStocker, cboArea);
        }
        private void cboSearchType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetGridVisibility();
        }

        private void dgStockList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    SetGridColumnWidth(dataGrid, e.Cell.Column.Name);

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "1" ||
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "2")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFF5F4F4");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "3" ||
                             Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "4")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE8E8E8");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgStockList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null) return;

            //C1DataGrid dataGrid = sender as C1DataGrid;
            //dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //            e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //        }
            //    }
            //}));
        }

        /// <summary>
        /// 조회
        /// </summary>

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            SearchStock();
        }

        #endregion

        #region 모델
        private void cboAreaModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStockerTypeModel.SelectedValueChanged -= cboStockerTypeModel_SelectedValueChanged;
            SetStockerTypeCombo(cboStockerTypeModel, cboAreaModel);
            SetStockerCombo(cboStockerModel, cboAreaModel);
            cboStockerTypeModel.SelectedValueChanged += cboStockerTypeModel_SelectedValueChanged;
        }

        private void cboStockerTypeModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchType();
            SetGridVisibility();
            SetStockerCombo(cboStockerModel, cboAreaModel);
        }
        private void cboElectrodeTypeModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetStockerCombo(cboStockerModel, cboAreaModel);
        }
        private void cboSearchTypeModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetGridVisibility();
        }

        private void dgModelList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "1")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFF5F4F4");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_SORT_NO")) == "2")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE8E8E8");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgModelList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null) return;

            //C1DataGrid dataGrid = sender as C1DataGrid;
            //dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //            e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //        }
            //    }
            //}));
        }

        /// <summary>
        /// 조회
        /// </summary>

        private void btnSearchModel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            SearchModel();
        }

        #endregion

        #endregion

        #region Method

        #region [BizCall]
        /// <summary>
        /// 창고유형
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo, C1ComboBox cbArea)
        {
            //if (cbArea.SelectedValue == null || cbArea.SelectedValue.ToString() == "SELECT") return;

            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, cbArea.SelectedValue.ToString(), null, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            DataTable dt = DataTableConverter.Convert(cbArea.ItemsSource);
            
            if (dt.Rows[cbArea.SelectedIndex]["AREA_TYPE_CODE"].ToString() == "A")
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
            else
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        /// <summary>
        /// 극성
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

        /// <summary>
        /// 창고
        /// </summary>
        /// <param name="mcb"></param>
        /// <param name="cbArea"></param>
        private void SetStockerCombo(MultiSelectionBox mcb, C1ComboBox cbArea)
        {
            try
            {
                //if (cbArea.SelectedValue == null) return;

                string stockerType = string.Empty;
                string electrodeType = string.Empty;

                if (((FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbStock"))
                {
                    stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
                    electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();
                }
                else
                {
                    stockerType = string.IsNullOrEmpty(cboStockerTypeModel.SelectedValue.GetString()) ? null : cboStockerTypeModel.SelectedValue.GetString();
                    electrodeType = string.IsNullOrEmpty(cboElectrodeTypeModel.SelectedValue.GetString()) ? null : cboElectrodeTypeModel.SelectedValue.GetString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cbArea.SelectedValue;
                dr["EQGRID"] = stockerType;
                dr["ELTR_TYPE_CODE"] = stockerType == "LWW" ? null : electrodeType;     // 라미대기 창고시 극성 Null
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            mcb.Check(i);
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchStock()
        {
            try
            {
                string bizRuleName = string.Empty;

                InitializeControls();

                if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    bizRuleName = "DA_MCS_SEL_WAREHOUSE_STOCK_LWW";
                }
                else
                {
                    if (cboSearchType.SelectedValue.ToString() == "M")
                        bizRuleName = "DA_MCS_SEL_WAREHOUSE_STOCK";
                    else
                        bizRuleName = "DA_MCS_SEL_WAREHOUSE_STOCK_USE";
                }

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    inTable.Columns.Add("EQGRID", typeof(string));
                }
                else
                {
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                }
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                else
                {
                    dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                }
                dr["EQPTID"] = cboStocker.SelectedItemsToString;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                    {
                        Util.GridSetData(dgStockListL, bizResult, null, true);
                    }
                    else
                    {
                        if (cboSearchType.SelectedValue.ToString() == "M")
                            Util.GridSetData(dgStockListE, bizResult, null, true);
                        else
                            Util.GridSetData(dgStockListU, bizResult, null, true);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SearchModel()
        {
            try
            {
                string bizRuleName = string.Empty;

                InitializeControls();

                if (cboStockerTypeModel.SelectedValue != null && cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    bizRuleName = "DA_MCS_SEL_WAREHOUSE_MODEL_LWW";
                }
                else
                {
                    if (cboSearchTypeModel.SelectedValue.ToString() == "M")
                        bizRuleName = "DA_MCS_SEL_WAREHOUSE_MODEL";
                    else
                        bizRuleName = "DA_MCS_SEL_WAREHOUSE_MODEL_USE";
                }

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                if (cboStockerTypeModel.SelectedValue != null && cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    inTable.Columns.Add("EQGRID", typeof(string));
                }
                else
                {
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                }
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaModel.SelectedValue;
                if (cboStockerTypeModel.SelectedValue != null && cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    dr["EQGRID"] = cboStockerTypeModel.SelectedValue;
                }
                else
                {
                    dr["ELTR_TYPE_CODE"] = cboElectrodeTypeModel.SelectedValue;
                }
                dr["EQPTID"] = cboStockerModel.SelectedItemsToString;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (cboStockerTypeModel.SelectedValue != null && cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                    {
                        Util.GridSetData(dgModelListL, bizResult, null, true);
                    }
                    else
                    {
                        if (cboSearchTypeModel.SelectedValue.ToString() == "M")
                            Util.GridSetData(dgModelListE, bizResult, null, true);
                        else
                            Util.GridSetData(dgModelListU, bizResult, null, true);
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

        #region [Validation]
        private bool ValidationSearch()
        {
            if (((System.Windows.FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbStock"))
            {
                if (cboStocker.SelectedItemsToString == string.Empty)
                {
                    // 창고를 먼저 선택해주세요.
                    Util.MessageValidation("SFU2961");
                    return false;
                }
            }
            else
            {
                if (cboStockerModel.SelectedItemsToString == string.Empty)
                {
                    // 창고를 먼저 선택해주세요.
                    Util.MessageValidation("SFU2961");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region [Function]

        /// <summary>
        /// 창고에 따른 조회 구분 설정
        /// </summary>
        private void SetSearchType()
        {
            if (((FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbStock"))
            {
                if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    cboSearchType.SelectedValue = "U";
                    cboSearchType.IsEnabled = false;
                }
                else
                {
                    cboSearchType.SelectedValue = "M";
                    cboSearchType.IsEnabled = true;
                }
            }
            else
            {
                if (cboStockerTypeModel.SelectedValue != null && cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    cboSearchTypeModel.SelectedValue = "U";
                    cboSearchTypeModel.IsEnabled = false;
                }
                else
                {
                    cboSearchTypeModel.SelectedValue = "M";
                    cboSearchTypeModel.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Lamination Wait Warehouse 인경우 극성 및 그리드 설정
        /// </summary>
        private void SetGridVisibility()
        {
            if (((FrameworkElement)tabStock.SelectedItem).Name.Equals("ctbStock"))
            {
                Util.gridClear(dgStockListE);
                Util.gridClear(dgStockListU);
                Util.gridClear(dgStockListL);
                dgStockListE.Visibility = Visibility.Collapsed;
                dgStockListU.Visibility = Visibility.Collapsed;
                dgStockListL.Visibility = Visibility.Collapsed;

                if (cboStockerType.SelectedValue != null && cboStockerType.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    cboElectrodeType.IsEnabled = false;
                    dgStockListL.Visibility = Visibility.Visible;
                }
                else
                {
                    cboElectrodeType.IsEnabled = true;
                    if (cboSearchType.SelectedValue.ToString() == "M")
                        dgStockListE.Visibility = Visibility.Visible;
                    else
                        dgStockListU.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Util.gridClear(dgModelListE);
                Util.gridClear(dgModelListU);
                Util.gridClear(dgModelListL);
                dgModelListE.Visibility = Visibility.Collapsed;
                dgModelListU.Visibility = Visibility.Collapsed;
                dgModelListL.Visibility = Visibility.Collapsed;

                if (cboStockerTypeModel.SelectedValue != null &&  cboStockerTypeModel.SelectedValue.ToString() == _LamiWaitWarehouse)
                {
                    cboElectrodeTypeModel.IsEnabled = false;
                    dgModelListL.Visibility = Visibility.Visible;
                }
                else
                {
                    cboElectrodeTypeModel.IsEnabled = true;
                    if (cboSearchTypeModel.SelectedValue.ToString() == "M")
                        dgModelListE.Visibility = Visibility.Visible;
                    else
                        dgModelListU.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetGridColumnWidth(C1DataGrid dg, string columnName)
        {
            if (columnName == "CNT_PROD_GOOD_LE1000" || columnName == "CNT_CA_PROD_GOOD")
            {
                for (int col = dg.Columns[columnName].Index; col < dg.Columns.Count; col++)
                {
                    dg.Columns[col].Width = new C1.WPF.DataGrid.DataGridLength(55);
                }
            }
            else if (columnName == "ALL_RATE_LOADING" || columnName == "GOOD_RATE_LOADING" || columnName == "EMPTY_RATE_LOADING")
            {
                dg.Columns[columnName].Width = new C1.WPF.DataGrid.DataGridLength(55);
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

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



        #endregion

        #endregion

    }
}
