/*************************************************************************************
 Created Date : 2019.07.18
      Creator : 정문교
   Decription : 원자재관리 - 투입원자재 잔량 처리
--------------------------------------------------------------------------------------
 [Change History]


 
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
    public partial class MTRL001_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        private CheckBoxHeaderType _HeaderTypeCell;
        private CheckBoxHeaderType _HeaderTypeCellHis;

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_007()
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
            Util.gridClear(dgCurr);
            Util.gridClear(dgHistory);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //라인
            string[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);
        }

        private void SetControl(bool isVisibility = false)
        {
            _HeaderTypeCell = CheckBoxHeaderType.Zero;
            _HeaderTypeCellHis = CheckBoxHeaderType.Zero;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRemain);
            listAuth.Add(btnRemainHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
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

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgCurr;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCell)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCell)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCell = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCell = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void tbCheckHeaderAllHis_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgHistory;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCellHis)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCellHis)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCellHis = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCellHis = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void dgCurr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("REMAIN_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

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
                    if (e.Cell.Column.Name.Equals("REMAIN_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
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
        /// 설비에 장착된 자재에서 잔량 처리
        /// </summary>
        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemain(dgCurr))
                return;

            // 잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RemainProcess(dgCurr);
                }
            });
        }

        /// <summary>
        /// 설비 투입 이력에서 잔량 처리
        /// </summary>
        private void btnRemainHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemain(dgHistory))
                return;

            // 잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RemainProcess(dgHistory);
                }
            });
        }

        #endregion

        #region Mehod

        #region [BizCall]


        /// <summary>
        /// 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                // Clear
                InitializeGrid();
                SetControl();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_INPUT_CURR", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgCurr, bizResult, FrameOperation, true);

                        SearchHistory();
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
        /// 이력에서 조회
        /// </summary>
        private void SearchHistory()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_INPUT_HIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, null, true);
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
        /// 잔량처리
        /// </summary>
        private void RemainProcess(C1DataGrid dg)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inMLot = inDataSet.Tables.Add("INMLOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("MTRLID", typeof(string));
                inMLot.Columns.Add("REMAIN_QTY", typeof(Int16));

                /////////////////////////////////////////////////////////////////
                DataRow[] dr = DataTableConverter.Convert(dg.ItemsSource).Select("CHK = 1");

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = dr[0]["EQPTID"].ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                foreach (DataRow row in dr)
                {
                    newRow = inMLot.NewRow();
                    newRow["MLOTID"] = row["INPUT_LOTID"];
                    newRow["MTRLID"] = row["MTRLID"];
                    newRow["REMAIN_QTY"] = row["REMAIN_QTY"];
                    inMLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_MLOT_REMAIN", "INDATA,INMLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                        // 재조회
                        SearchProcess();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
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
            if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 라인을 선택해주세요
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 설비를 선택하세요
                Util.MessageValidation("SFU1153");
                return false;
            }

            return true;
        }

        private bool ValidationRemain(C1DataGrid dg)
        {
            if (dg == null || dg.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dg.ItemsSource).Select("CHK = 1");
            if (dr.Length ==  0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            foreach (DataRow row in dr)
            {
                int RemainQty = 0;
                double InputQty = 0;
                double UnitQty = 0;

                int.TryParse(row["REMAIN_QTY"].ToString(), out RemainQty);
                double.TryParse(row["INPUT_QTY"].ToString(), out InputQty);
                double.TryParse(row["UNIT_QTY"].ToString(), out UnitQty);

                if (RemainQty == 0)
                {
                    // 잔량은 1이상이어야 합니다.
                    Util.MessageValidation("SFU4207");
                    return false;
                }

                if (InputQty != 0 && RemainQty > InputQty)
                {
                    // 잔량은 투입수량보다 작아야 합니다.
                    Util.MessageValidation("SFU4053");
                    return false;
                }

                if (UnitQty != 0 && RemainQty > UnitQty)
                {
                    // 잔량은 단위수량보다 작아야 합니다.
                    Util.MessageValidation("SFU8019");
                    return false;
                }
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

        #endregion

        #endregion

        #endregion

    }
}
