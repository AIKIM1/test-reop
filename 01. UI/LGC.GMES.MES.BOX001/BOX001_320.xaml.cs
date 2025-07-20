/*************************************************************************************
 Created Date : 2021.12.21
      Creator : 오화백
   Decription : SORTING Cell 등록 및 이력
--------------------------------------------------------------------------------------
 [Change History]
 2023.06.27  이원열       SORTING CELL 저장 및 이력조회 화면 - BarcodeID 추가 및 조회 가능하도록 수정
 2024.02.24  서동현       Sorting 측정 이력 조회 탭 추가
 2024.07.23  최석준       반품여부 컬럼 추가 (2025년 적용예정, 수정 시 연락부탁드립니다)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_320 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private CheckBoxHeaderType _HeaderTypeCell;
        private CheckBoxHeaderType _HeaderTypeHistory;
        DataRow _drSelectRow;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public BOX001_320()
        {
            InitializeComponent();

            Initialize();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHoldStock_Excel);
            listAuth.Add(btnDeleteSubLot);
            listAuth.Add(btnSearchHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            TimerSetting();
            _isLoaded = true;
            InitializeUserControls();
            InitCombo();
            SetControl();

            // 팔레트바코드 표시여부
            isVisibleBCD(LoginInfo.CFG_AREA_ID);

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 조회 수량
        /// </summary>
        private void InitializeUserControls()
        {
            if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbSortingCell"))
            {
                txtScanQty.Value = 0;
            }
        }

        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgDefectSubLot.Columns["RTN_FLAG"].Visibility = Visibility.Visible;        //Sorting Cell 등록
                dgHistoryCell.Columns["RTN_FLAG"].Visibility = Visibility.Visible;         //Sorting 이력 조회
                dgSortingMeasrHistory.Columns["RTN_FLAG"].Visibility = Visibility.Visible; //Sorting 측정 이력 조회
            }

        }

        /// <summary>
        /// 컨트롤 초기화
        /// </summary>
        private void SetControl()
        {
            _HeaderTypeCell = CheckBoxHeaderType.Zero;
            _HeaderTypeHistory = CheckBoxHeaderType.Zero;

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboMeasrEquipmentSegment.SelectedItemChanged += cboMeasrEquipmentSegment_SelectedItemChanged;
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            SetEquipmentSegment();
            SetEquipment();

            SetMeasrEquipmentSegment();
            SetMeasrEquipment();
        }
        #endregion

        #region Event

        #region [Sorting Cell 등록]

        #region  Cell ID로 조회  : txtSortingSubLot_KeyDown()
        /// <summary>
        /// Cell ID 조회조건으로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSortingSubLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (txtSortingSubLot.Text == string.Empty)
                    return;

                SearchSortingCell();
            }
        }
        #endregion

        #region 전체선택 이벤트 : tbCheckHeaderAllDefect_MouseLeftButtonDown()
        /// <summary>
        /// 전체 선택 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbCheckHeaderAllDefect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectSubLot;
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

        #endregion

        #region 전체 및 개별 삭제 : btnDeleteSubLot_Click()

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDeleteSubLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteSubLot())
                return;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource).Select("CHK = True").CopyToDataTable(); ;

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("INSDTTM", typeof(DateTime));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("UPDDTTM", typeof(DateTime));
                inTable.Columns.Add("TYPE", typeof(string));
                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["INSUSER"] = LoginInfo.USERID; ;
                newRow["INSDTTM"] = System.DateTime.Now; ;
                newRow["UPDUSER"] = LoginInfo.USERID; ;
                newRow["UPDDTTM"] = System.DateTime.Now; ;
                newRow["TYPE"] = "DEL";
                inTable.Rows.Add(newRow);

                // XML string 500건당 1개 생성
                int rowCount = 0;
                string sXML = string.Empty;

                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    if (dt.Rows[row]["CHK"].ToString() == "1")
                    {
                        if (row == 0 || row % 500 == 0)
                        {
                            sXML = "<root>";
                        }
                        sXML += "<DT><SUBLOTID>" + dt.Rows[row]["SUBLOTID"] + "</SUBLOTID></DT>";
                        if ((row + 1) % 500 == 0 || row + 1 == dt.Rows.Count)
                        {
                            sXML += "</root>";

                            newRow = inSubLot.NewRow();
                            newRow["SUBLOTID"] = sXML;
                            inSubLot.Rows.Add(newRow);
                        }

                        rowCount = rowCount + 1;
                    }
                }
                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SORTING_CELL_INS", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                        SearchSortingCell();
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
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 엑셀 업로드 버튼 :  btnLotHold_Excel_Click()
        /// <summary>
        /// 엑셀 업로드 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLotHold_Excel_Click(object sender, RoutedEventArgs e)
        {
            SortingCellUPLOD();
        }
        #endregion

        #region Cell 정보 조회 버튼 : btnSearchCell_Click()
        /// <summary>
        /// Cell 정보 조회 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchCell_Click(object sender, RoutedEventArgs e)
        {
            SearchSortingCell();
        }
        #endregion

        #region Pallet 조회  : txtSortingPallt_KeyDown()
        /// <summary>
        /// Pallet ID로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSortingPallt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (txtSortingPallt.Text == string.Empty)
                    return;
                SearchSortingCell();
            }
        }
        #endregion

        #region txtBox Focus : text_GotFocus()
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region 타이머 콤보박스 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// 타이머 콤보박스 이벤트
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

        #endregion

        #region [Sorting Cell 이력]

        #region Pallet ID로 이력 조회 : txtHis_KeyDown()
        /// <summary>
        /// Pallet ID, Pallet BCD 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchHistoryProcess();
            }
        }
        #endregion

        #region 이력조회- 버튼  : btnSearchHis_Click()
        /// <summary>
        /// 이력조회- 버튼 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryProcess();
        }
        #endregion

        #region 라인 이벤트 : cboEquipmentSegment_SelectedItemChanged()
        /// <summary>
        /// 라인이벤트 :
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipment();
        }
        #endregion

        #region Pallet 정보 선택 : dgHistoryChoice_Checked()
        /// <summary>
        /// 이력 Pallet 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dgHistoryChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgHistory.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                // 선택 Row 
                _drSelectRow = _Util.GetDataGridFirstRowBycheck(dgHistory, "CHK");

                //소팅Cell 이력 조회
                SearchCellHistory(Util.NVC(_drSelectRow["FROM_PALLETID"]));
            }
        }
        #endregion

        #endregion

        #region [Sorting 측정 이력 조회]

        #region Pallet ID로 이력 조회 : txtMeasrPallet_KeyDown()
        /// <summary>
        /// Pallet ID, Pallet BCD 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMeasrPallet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchMeasrHistory();
            }
        }
        #endregion

        #region 이력조회- 버튼  : btnSearchMeasrHistory_Click()
        /// <summary>
        /// 측정 이력조회- 버튼 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchMeasrHistory_Click(object sender, RoutedEventArgs e)
        {
            SearchMeasrHistory();
        }
        #endregion

        #region 라인 이벤트 : cboMeasrEquipmentSegment_SelectedItemChanged()
        /// <summary>
        /// 라인이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboMeasrEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMeasrEquipment();
        }
        #endregion

        #endregion

        #endregion

        #region Mehod

        #region [Sorting Cell 등록]

        #region SORTING 대상 Cell 조회 : SearchSortingCell()

        /// <summary>
        /// SORTING 대상 Cell 조회
        /// </summary>
        private void SearchSortingCell()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("OUTER_BOXID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SUBLOTID"] = txtSortingSubLot.Text == string.Empty ? null : txtSortingSubLot.Text;

                //2023.06.27
                //newRow["OUTER_BOXID"] = txtSortingPallt.Text == string.Empty ? null : txtSortingPallt.Text;
                newRow["OUTER_BOXID"] = txtSortingPallt.Text == string.Empty ? null : getPalletBCD(txtSortingPallt.Text); ;


                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SUBLOTID_SORTING", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        txtScanQty.Value = bizResult.Rows.Count;
                        Util.GridSetData(dgDefectSubLot, bizResult, null, false);

                        if (txtSortingSubLot.Text != string.Empty)
                        {
                            txtSortingSubLot.Text = string.Empty;
                        }
                        if (txtSortingPallt.Text != string.Empty)
                        {
                            txtSortingPallt.Text = string.Empty;
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
        #endregion

        #region 타이머 관련 : TimerSetting(), _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 기준정보 조회
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 1;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();
            }
        }

        /// <summary>
        /// 타이머 실행
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

                    if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbSortingCell"))
                    {
                        SearchSortingCell();
                    }
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

        #region 삭제 Validation  :  ValidationDeleteSubLot()
        /// <summary>
        /// 삭제 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationDeleteSubLot()
        {
            int rowIndex = 0;
            DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["CHK"].ToString() == "1")
                {
                    rowIndex++;
                }
            }

            if (rowIndex == 0 || dt.Rows.Count == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        #endregion

        #region 엑셀 업로드 : SortingCellUPLOD(),PuCell_Closed() 
        /// <summary>
        /// 엑셀 업로드 팝업 
        /// </summary>
        private void SortingCellUPLOD()
        {
            try
            {
                BOX001_320_SORTING_CELL_UPLOAD PuCell = new BOX001_320_SORTING_CELL_UPLOAD();
                PuCell.FrameOperation = FrameOperation;

                PuCell.Closed += new EventHandler(PuCell_Closed);

                grdMain.Children.Add(PuCell);
                PuCell.BringToFront();
                //Search_Ncr();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 엑셀 업로드 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PuCell_Closed(object sender, EventArgs e)
        {
            BOX001_320_SORTING_CELL_UPLOAD window = sender as BOX001_320_SORTING_CELL_UPLOAD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SearchSortingCell();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #endregion

        #region [Sorting Cell 이력]

        #region 소팅 Pallet 이력 조회 : SearchHistoryProcess()
        /// <summary>
        /// SORTING Pallet 이력 조회
        /// </summary>
        private void SearchHistoryProcess()
        {
            try
            {
                if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }
                if (cboEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("PALLET_BCD", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString(); //"W1AWRPM01"
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PALLETID"] = txtPallet.Text == string.Empty ? null : txtPallet.Text;
                newRow["PALLET_BCD"] = txtPalletBCD.Text == string.Empty ? null : txtPalletBCD.Text;

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_PALLET_HISTORY_SORTING_BY_BARCODE", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.gridClear(dgHistoryCell);
                        Util.GridSetData(dgHistory, bizResult, null, false);

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

        #region 팔레트바코드ID -> BoxID : 2023.06.27
        /// <summary>
        /// 팔레트바코드ID -> BoxID
        /// </summary>
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        #region 파레트 바코드 표시 설정 : 2023.06.27
        /// <summary>
        /// 팔레트 바코드 항목 표시 여부
        /// </summary>
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgDefectSubLot.Columns.Contains("PLLT_BCD_ID"))
                    dgDefectSubLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                if (dgHistory.Columns.Contains("FROM_BARCODEID"))
                    dgHistory.Columns["FROM_BARCODEID"].Visibility = Visibility.Visible;
                if (dgHistory.Columns.Contains("TO_BARCODEID"))
                    dgHistory.Columns["TO_BARCODEID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgDefectSubLot.Columns.Contains("PLLT_BCD_ID"))
                    dgDefectSubLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                if (dgHistory.Columns.Contains("FROM_BARCODEID"))
                    dgHistory.Columns["FROM_BARCODEID"].Visibility = Visibility.Collapsed;
                if (dgHistory.Columns.Contains("TO_BARCODEID"))
                    dgHistory.Columns["TO_BARCODEID"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 소팅 Cell 이력 조회 : SearchCellHistory()
        /// <summary>
        /// 소팅 Cell 이력 조회
        /// </summary>
        private void SearchCellHistory(string Palletid)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PALLETID"] = Palletid;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_CELL_HISTORY_SORTING", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryCell, bizResult, FrameOperation, false);
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

        #region 라인정보조회 : SetEquipmentSegment()
        /// <summary>
        /// 라인 정보 조회
        /// </summary>
        private void SetEquipmentSegment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
         
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SORTING_LINE_ID";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CMCDNAME";
                cboEquipmentSegment.SelectedValuePath = "CMCODE";
               
                DataRow drIns = dtResult.NewRow();
                drIns["CMCDNAME"] = "-SELECT-";
                drIns["CMCODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();

                cboEquipmentSegment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 설비조회 : SetEquipment()

        private void SetEquipment()
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SORTING_EQUIPMENT_ID";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);
           
                cboEquipment.DisplayMemberPath = "CMCDNAME";
                cboEquipment.SelectedValuePath = "CMCODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CMCDNAME"] = "-SELECT-";
                drIns["CMCODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);
            
                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region [Sorting 측정 이력 조회]

        #region 소팅 측정 이력 조회 : SearchMeasrHistory()
        /// <summary>
        /// SORTING 측정 이력 조회
        /// </summary>
        private void SearchMeasrHistory()
        {
            try
            {
                if ((dtpDateToMeasrHis.SelectedDateTime - dtpDateFromMeasrHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (cboMeasrEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (cboMeasrEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboMeasrEquipment.SelectedValue.ToString();      //"W3FWRPS1101"
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromMeasrHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToMeasrHis);
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PALLETID"] = txtMeasrPallet.Text == string.Empty ? null : txtMeasrPallet.Text;

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SORTING_MEASR_HISTORY", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgSortingMeasrHistory, bizResult, null, false);

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

        #region 측정 라인정보조회 : SetMeasrEquipmentSegment()
        /// <summary>
        /// 라인 정보 조회
        /// </summary>
        private void SetMeasrEquipmentSegment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SORTING_LINE_ID";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                cboMeasrEquipmentSegment.DisplayMemberPath = "CMCDNAME";
                cboMeasrEquipmentSegment.SelectedValuePath = "CMCODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CMCDNAME"] = "-SELECT-";
                drIns["CMCODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboMeasrEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();

                cboMeasrEquipmentSegment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 측정 설비조회 : SetMeasrEquipment()

        private void SetMeasrEquipment()
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboMeasrEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SORTING_EQUIPMENT_ID";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                cboMeasrEquipment.DisplayMemberPath = "CMCDNAME";
                cboMeasrEquipment.SelectedValuePath = "CMCODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CMCDNAME"] = "-SELECT-";
                drIns["CMCODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboMeasrEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboMeasrEquipment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

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

        /// <summary>
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
    }
}
