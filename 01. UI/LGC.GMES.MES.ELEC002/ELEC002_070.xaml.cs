/*************************************************************************************
 Created Date : 2020.09.04
      Creator : 
   Decription : REWINDER 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.04  DEVELOPER : Initial Created.
  2023.01.04  정재홍    : C20221212-000050 - Rewinder UI Lot Search confirm[진행, 설비완공 조건 추가]

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Windows.Documents;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using System.Threading;


namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_070 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        CurrentLotInfo _CurrentLot = new CurrentLotInfo();
        string _Lane = string.Empty;
        string _Lane_Ptn_qty = string.Empty;
        string _InputQty = string.Empty;
        string _GoodQty_PC = string.Empty;
        string _DefectQty = string.Empty;
        string _LossQty = String.Empty;
        string _Shift = string.Empty;
        string _StartTime = string.Empty;
        string _EndTime = string.Empty;
        string _OperTime = string.Empty;
        string _Remark = string.Empty;
        string _Hold = string.Empty;
        string _EqptID = string.Empty;
        bool defectFlag = false;
        bool lossFlag = false;
        bool chargeFlag = false;
        bool remarkFlag = false;
        bool isResnCountUse = false;
        string _LotProcId = string.Empty;
        string _Wipstat = string.Empty;
        private bool isDupplicatePopup = false;
        decimal inputOverrate;

        private string WIPSTATUS { get; set; }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC002_070()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;

            chkRun.Checked += OnCheckBoxChecked;
            chkRun.Unchecked += OnCheckBoxChecked;
            chkEqpEnd.Checked += OnCheckBoxChecked;
            chkEqpEnd.Unchecked += OnCheckBoxChecked;

        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            initcombo();

            ApplyPermissions();
            //GetRewinderLot();

            string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));

            if (IsAreaCommonCodeUse("RESN_COUNT_USE_YN", Process.SLIT_REWINDING))
            {
                dgWipReason.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                dgWipReason.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboOperation.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return;
            }
            remove(dgWipInfo,"CHK");
            GetRewinderLot();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //재와인더 이동
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            //_Util.SetDataGridUncheck(dgWipInfo, "CHK", checkIndex);

            if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }


            if (!Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[checkIndex].DataItem, "PROCID")).Equals(Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK")].DataItem, "PROCID"))))
            {
                Util.MessageValidation("SFU1446");  //같은 공정이 아닙니다.
                DataTableConverter.SetValue(dgWipInfo.Rows[checkIndex].DataItem, "CHK", false);
                return;
            }
            
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            _Util.SetDataGridUncheck(dgRewinderInfo, "CHK", checkIndex);
           if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
            {
                Util.gridClear(dgWipReason);
                
                ClearData();
                return;
            }
            GetProcessDetail(checkIndex);
        }

        private void btnMoveStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                    return;
                }

                ELEC002_070_LOTSTART _LotStart = new ELEC002_070_LOTSTART();
                _LotStart.FrameOperation = FrameOperation;
                if (_LotStart != null)
                {
                    object[] parameters = new object[10];

                    DataTable dt = DataTableConverter.Convert(dgWipInfo.ItemsSource);       //투입Lot
                    dt = dt.Select("CHK = 'True'").CopyToDataTable();

                    parameters[0] = dt;
                    parameters[1] = Util.NVC(cboOperation.SelectedValue); ;
                    parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    parameters[3] = Util.NVC(cboEquipmentSegment.SelectedValue);

                    C1WindowExtension.SetParameters(_LotStart, parameters);

                    _LotStart.Closed += new EventHandler(OnCloseLotStart);
                    this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                    _LotStart.CenterOnScreen();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
          
        }
        private void OnCloseLotStart(object sender, EventArgs e)
        {
            ELEC002_070_LOTSTART window = sender as ELEC002_070_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                // Rewinder Lot 조회
                GetRewinderLot();
                // Rewinder 재공대상 삭제
                remove(dgWipInfo, "CHK");
            }

        }

        private void btnMoveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                    return;
                }
                // 선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelMoveMerge();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            defectFlag = true;
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (GetDefectSum() == false)
            {
                 if (dataGrid != null)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
                }
            }
            else
            {
                //20200701 오화백 태그수 입력시 TAG_CONV_RATE 컬럼값을 곱하여 수량정보 자동입력 되도록 수정
                //tag에 수량이 있는경우 일반 불량수량으로 입력해도 tag 수량으로 계산되어 입력되도록 수정
                if (e.Cell.Column.Name.Equals("DFCT_TAG_QTY") || Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY")) > 0)
                //if (e.Cell.Column.Name.Equals("DFCT_TAG_QTY"))
                {
                    if (!string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRCS_ITEM_CODE"), "OUT_LOT_QTY_INCR"))
                    {
                        string sTagQty = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY"));
                        string sTagRate = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAG_CONV_RATE"));

                        double dTagQty = 0;
                        double dTagRate = 0;

                        double.TryParse(sTagQty, out dTagQty);
                        double.TryParse(sTagRate, out dTagRate);
                        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", dTagQty * dTagRate);
                        dataGrid.UpdateLayout();
                    }
                    else
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
                        dataGrid.UpdateLayout();
                    }
                }
            }
        }


        /// <summary>
        /// 품질항목 스프레드 PreviewKeyDown 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }

        /// <summary>
        /// 품질항목 스프레드 LoadedCellPresenter 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                                C1.WPF.DataGrid.C1DataGrid grid;
                                grid = p.DataGrid;

                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
                                string sCSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSL"));
                                string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));

                                if (panel != null)
                                {
                                    if (string.Equals(sCode, "NUM"))
                                    {
                                        C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                        // 재설정
                                        if (string.Equals(txtUnit.Text, "EA"))
                                            numeric.Format = "F1";
                                        else
                                            numeric.Format = GetUnitFormatted();

                                        // SRS요청으로 동별 LIMIT값 설정 [2017-11-30]
                                        if (!string.IsNullOrEmpty(sLSL_Limit) && Util.NVC_Decimal(sLSL_Limit) > 0)
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            // 액셀 붙여넣기 기능으로 빈칸이 입력될 경우 Convert클래스 이용 시 오류 발생 문제로 체크용 Function 교체 [2019-01-28]
                                            if (!string.IsNullOrWhiteSpace(sValue) && !string.Equals(sValue, "NaN"))
                                            {
                                                //소수점Separator에 따라 분기(우크라이나 언어)
                                                if (sValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.CurrentCulture.NumberFormat);
                                                else
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture.NumberFormat);
                                            }

                                            if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }

                                            else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }
                                            else
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                            }
                                        }
                                        numeric.IsKeyboardFocusWithinChanged -= OnDataCollectGridFocusChanged;
                                        numeric.IsKeyboardFocusWithinChanged += OnDataCollectGridFocusChanged;
                                        numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.LostKeyboardFocus -= OnDataCollectGridGotKeyboardLost;
                                        numeric.LostKeyboardFocus += OnDataCollectGridGotKeyboardLost;
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }

                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                            if (e.Cell.Column.Index == dataGrid.Columns["CLSS_NAME1"].Index)
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                            }
                            else if (e.Cell.Column.Index == dataGrid.Columns["CLCTVAL01"].Index) // 측정값
                            {
                                if (presenter.HorizontalAlignment != HorizontalAlignment.Right)
                                    presenter.HorizontalAlignment = HorizontalAlignment.Right;

                                decimal sumValue = 0;
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                {
                                    if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非?字"))
                                    {
                                        foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));


                                        if (sumValue == 0)
                                            presenter.Content = 0;
                                        else
                                            presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dataGrid.Rows.Count - dataGrid.BottomRows.Count), "EA"));
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// 품질항목 스프레드 UnloadedCellPresenter 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }));
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboOperation.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (defectFlag == true)
            {
                Util.MessageValidation("SFU2900");  //불량/Loss/물청 정보를 저장하세요.
                return;
            }

            if (remarkFlag == true)
            {
                Util.MessageValidation("SFU2977"); //특이사항 정보를 저장하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            PreConfirmProcess();
        }

        private void btnSaveDefect_Click(object sender, RoutedEventArgs e)
        {
            //불량정보를 저장하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
              {
                  if (result == MessageBoxResult.OK)
                  {
                      SetDefect(dgWipReason);
                      //  SetDefect(dgDefect);
                  }
              });
            //  SetDefect(_CurrentLot.LOTID, dgDefect);
        }


        /// <summary>
        /// 품질항목 저장버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_Lane == string.Empty)
                return;
            try
            {
                SaveQuality(dgQuality);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        /// <summary>
        /// 품질항목 스프레드 수정시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            string sCode = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    // 자주검사 USL, LSL 체크
                    DataTable dataCollect = DataTableConverter.Convert(caller.ItemsSource);
                    int iHGCount1 = 0;  // H/G
                    int iHGCount2 = 0;  // M/S
                    int iHGCount3 = 0;  // 1차 H/G
                    int iHGCount4 = 0;  // 1차 M/S
                    decimal sumValue1 = 0;
                    decimal sumValue2 = 0;
                    decimal sumValue3 = 0;
                    decimal sumValue4 = 0;
                    foreach (DataRow row in dataCollect.Rows)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount3++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount4++;
                            }
                        }
                    }

                    if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);

                }
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Util.NVC(cboOperation.SelectedValue);
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }
        #endregion

        #region Mehod
        private void initcombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            String[] sFilter2 = { "REWINDING_PROCID" };
            C1ComboBox[] cboEquipmentChild = { cboEquipment };
            _combo.SetCombo(cboOperation, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild,  sFilter: sFilter2, sCase: "COMMCODE");

            C1ComboBox[] cboOperationParent = { cboEquipmentSegment, cboOperation };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboOperationParent);

            cboOperation.SelectedValue = LoginInfo.CFG_PROC_ID;
            if (cboOperation.SelectedIndex < 0)
            {
                cboOperation.SelectedIndex = 0;
            }
            cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cboEquipment.SelectedIndex < 0)
            {
                cboEquipment.SelectedIndex = 0;
            }
        }

        private void SetStatus()
        {
            // CSR : C20221212-000050 - Rewinder UI Lot Search confirm
            var status = new List<string>();

            if (chkRun.IsChecked == true)
                status.Add(Wip_State.PROC);

            if (chkEqpEnd.IsChecked == true)
                status.Add(Wip_State.EQPT_END);

            if (chkRun.IsChecked == false && chkEqpEnd.IsChecked == false)
                status.Add(Wip_State.WAIT);

            WIPSTATUS = string.Join(",", status);
        }

        private void GetRewinderLot()
        {
            try
            {
                ClearData();

                // CSR : C20221212-000050 - Rewinder UI Lot Search confirm
                SetStatus();

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Util.NVC(cboOperation.SelectedValue);
                // row["WIPSTAT"] = Wip_State.WAIT;
                row["WIPSTAT"] = WIPSTATUS; // CSR : C20221212 - 000050 - Rewinder UI Lot Search confirm
                row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_RW", "INDATA", "OUTDATA", dt);
                if (result.Rows.Count == 0)
                {
                    Util.gridClear(dgRewinderInfo);
                    return;
                }

                Util.GridSetData(dgRewinderInfo, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return Util.NVC(row["ATTRIBUTE1"]);
            }
            catch (Exception ex) { }

            return "";
        }
        private void SearchData()
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if(cboOperation.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                string lotid = txtLOTID.Text;
                if (txtLOTID.Text.Equals(""))
                {
                    Util.MessageValidation("SFU1366");  //LOT ID를 입력해주세요.
                    return;
                }

                DataTable dt = GetLotInfo(lotid);

                if (!ValidationWipInfo(dt))
                {
                    return;
                }

                AddRow(dgWipInfo, dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtLOTID.Text = "";
            }

        }
        private bool ValidationWipInfo(DataTable dt)
        {
            if (dt == null)
            {
                return false;
            }
            if (!_Wipstat.Equals(Wip_State.WAIT))
            {
                Util.MessageValidation("SFU1220");  //대기LOT이 아닙니다.
                return false;
            }

            for (int i = 0; i < dgWipInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "LOTID")).Equals(dt.Rows[0]["LOTID"]))
                {
                    Util.MessageValidation("SFU2014");  //해당 LOT이 이미 존재합니다.
                    return false;
                }
            }

            if (_Hold.Equals("Y"))
            {
                if (!GetHoldPassArea())
                {
                    Util.MessageValidation("SFU1761", new object[] { txtLOTID.Text });    //{0}이 HOLD상태 입니다.
                    txtLOTID.Text = "";
                    return false;
                }
            }

            if (!String.IsNullOrWhiteSpace(_EqptID))
            {
                Util.MessageValidation("101300", new object[] { _EqptID });    //{0} 설비에서 진행중인 Lot입니다. Lot정보조회에서 확인 후 작업하세요.
                txtLOTID.Text = "";
                return false;
            }
            //if ( string.Equals(Process.REWINDING, dt.Rows[0]["PROCID"]) || string.Equals(Process.SLIT_REWINDING, dt.Rows[0]["PROCID"]))
            //{
            //    Util.MessageValidation("SFU3200");//재와인더 공정으로 이동된 LOT입니다.
            //    txtLOTID.Text = "";
            //    return false;
            //}
            return true;
        }

        bool GetHoldPassArea()
        {
            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = txtLOTID.Text;
            indata["CMCDTYPE"] = "REWINDING_HOLD_PASS_AREA";
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTAREA_CHECK", "INDATA", "RSLTDT", inTable);

            if (_dt.Rows.Count > 0)
                return true;
            else
                return false;

        }

        private void AddRow(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }

            DataRow row = preTable.NewRow();
            row["CHK"] = Convert.ToBoolean(false);
            row["LOTID"] = Convert.ToString(dt.Rows[0]["LOTID"]);
            row["PROCNAME"] = Convert.ToString(dt.Rows[0]["PROCNAME"]);
            row["WIPSTAT"] = Convert.ToString(dt.Rows[0]["WIPSTAT"]);
            row["WIPSNAME"] = Convert.ToString(dt.Rows[0]["WIPSNAME"]);
            row["PRJT_NAME"] = Convert.ToString(dt.Rows[0]["PRJT_NAME"]);
            row["PRODID"] = Convert.ToString(dt.Rows[0]["PRODID"]);
            row["PRODNAME"] = Convert.ToString(dt.Rows[0]["PRODNAME"]);
            row["PROD_VER_CODE"] = Convert.ToString(dt.Rows[0]["PROD_VER_CODE"]);
            row["LANE_QTY"] = Convert.ToString(dt.Rows[0]["LANE_QTY"]);
            row["WIPQTY"] = Convert.ToString(dt.Rows[0]["WIPQTY"]);
            row["WIPQTY2"] = Convert.ToString(dt.Rows[0]["WIPQTY2"]);
            row["UNIT"] = Convert.ToString(dt.Rows[0]["UNIT"]);
            row["LANE_PTN_QTY"] = Convert.ToString(dt.Rows[0]["LANE_PTN_QTY"]);
            row["WIPSEQ"] = Convert.ToString(dt.Rows[0]["WIPSEQ"]);
            preTable.Rows.Add(row);

            Util.GridSetData(datagrid, preTable, FrameOperation, true);        
        }

        private void CancelMoveMerge()
        {
            try
            {
                DataSet indataSet = new DataSet();
                #region # IN_EQP
                DataTable IN_EQP = indataSet.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                IN_EQP.Columns.Add("IFMODE", typeof(string));
                IN_EQP.Columns.Add("EQPTID", typeof(string));
                IN_EQP.Columns.Add("USERID", typeof(string));

                DataRow row = IN_EQP.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                IN_EQP.Rows.Add(row);
                #endregion

                #region # IN_INPUT
                string _MERGE = string.Empty;
                DataTable IN_INPUT = indataSet.Tables.Add("IN_INPUT");
                IN_INPUT.Columns.Add("LOTID", typeof(string));
                IN_INPUT.Columns.Add("MERGE", typeof(string));

                for (int i = 0; i < dgRewinderInfo.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "ACTID")), "MERGE_LOT"))
                            _MERGE = "Y";
                        else
                            _MERGE = "N";

                        DataRow newRow = IN_INPUT.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "LOTID"));
                        newRow["MERGE"] = _MERGE;
                        IN_INPUT.Rows.Add(newRow);
                    }
                }
                #endregion

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_MOVE_MERGE_RW", "IN_EQP,IN_INPUT", null, indataSet);

                //정상처리되었습니다.
                Util.MessageInfo("SFU1275", (xresult) =>
                {
                    int rowIndex = _Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK");

                    string _LOTID = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[rowIndex].DataItem, "LOTID"));

                    DataTable dt = DataTableConverter.Convert(dgRewinderInfo.ItemsSource);
                    dt = dt.Select("LOTID = '" + _LOTID + "'").CopyToDataTable();

                    // Merge Lot
                    DataTable mergeTable = dt.DefaultView.ToTable(true);
                    mergeTable = GetLotInfo(Util.NVC(mergeTable.Rows[0]["LOTID"]));
                    AddRow(dgWipInfo, mergeTable);

                    // From Lot
                    DataTable fromTable = new DataTable();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(Util.NVC(dt.Rows[i]["FROM_LOTID"])))
                            continue;
                        fromTable = GetLotInfo(Util.NVC(dt.Rows[i]["FROM_LOTID"]));
                        AddRow(dgWipInfo, fromTable);
                    }
                    MERGEremove(dgRewinderInfo, _LOTID);
                    //remove(dgRewinderInfo, "CHK");
                    ClearData();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmProcess()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROD_VER_CODE", typeof(string));
                inData.Columns.Add("SHIFT", typeof(string));
                inData.Columns.Add("WRK_USER_NAME", typeof(string));
                inData.Columns.Add("WIPNOTE", typeof(string));
                inData.Columns.Add("LANE_QTY", typeof(string));
                inData.Columns.Add("LANE_PTN_QTY", typeof(string));


                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                row["PROCID"] = Util.NVC(cboOperation.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["PROD_VER_CODE"] = txtVersion.Text;
                row["SHIFT"] = txtShift.Tag;
                row["WRK_USER_NAME"] = txtWorker.Text;
                row["WIPNOTE"] = DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK");

                row["LANE_QTY"] = _Lane;
                row["LANE_PTN_QTY"] = _Lane_Ptn_qty;
                inData.Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("INPUTQTY", typeof(decimal));
                inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                inLot.Columns.Add("RESNQTY", typeof(decimal));

                row = inLot.NewRow();
                row["LOTID"] = _CurrentLot.LOTID;
                row["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
                row["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                row["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY"));
                inLot.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_END_LOT_RW", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.AlertInfo("SFU1924");  //착공완료 / 실적확정

                        GetRewinderLot();
                        ClearData();
                        SetLabelLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PreConfirmProcess(bool bRealWorkerSelFlag = false)
        {
            #region 작업자 실명관리 기능 추가
            if (!bRealWorkerSelFlag && CheckRealWorkerCheckFlag())
            {
                CMM001.CMM_COM_INPUT_USER wndRealWorker = new CMM001.CMM_COM_INPUT_USER();

                wndRealWorker.FrameOperation = FrameOperation;
                object[] Parameters2 = new object[0];
                //Parameters2[0] = "";

                C1WindowExtension.SetParameters(wndRealWorker, Parameters2);

                wndRealWorker.Closed -= new EventHandler(wndRealWorker_Closed);
                wndRealWorker.Closed += new EventHandler(wndRealWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndRealWorker.ShowModal()));

                return;
            }
            #endregion

            //실적 확정 하시겠습니까?
            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }

        private void SetLabelLot()
        {
            //DataTable LabelDT = new DataTable();
            txtEndLotID.Text = _CurrentLot.LOTID;

            if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, _CurrentLot.LOTID, Util.NVC(cboOperation.SelectedValue));

            //PrintLabel(_CurrentLot.LOTID);
        }

        private void SetDefect(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            try
            {
                GetTotalQtySum();

                defectFlag = false;
                if (datagrid.Rows.Count < 0) return;

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataRow["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["PROCID"] = Util.NVC(cboOperation.SelectedValue);
                inDataTable.Rows.Add(inDataRow);

                DataTable IndataTable = inDataSet.Tables.Add("INRESN");
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("ACTID", typeof(string));
                IndataTable.Columns.Add("RESNCODE", typeof(string));
                IndataTable.Columns.Add("RESNQTY", typeof(decimal));
                IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
                IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

                for (int i = 0; i < datagrid.GetRowCount(); i++)
                {

                    inDataRow = IndataTable.NewRow();

                    inDataRow["LOTID"] = _CurrentLot.LOTID;
                    inDataRow["WIPSEQ"] = _CurrentLot.WIPSEQ;
                    inDataRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "ACTID"));
                    inDataRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNCODE"));
                    inDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNQTY"));
                    if (datagrid.Columns["DFCT_TAG_QTY"].Visibility == Visibility.Visible)
                    {
                        inDataRow["DFCT_TAG_QTY"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "DFCT_TAG_QTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "DFCT_TAG_QTY")); ;
                    }
                    else
                    {
                        inDataRow["DFCT_TAG_QTY"] = 0;
                    }
                    inDataRow["LANE_QTY"] = txtLane.Text;
                    inDataRow["LANE_PTN_QTY"] = _Lane_Ptn_qty;
                    inDataRow["COST_CNTR_ID"] =  Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "COSTCENTERID"));
                    IndataTable.Rows.Add(inDataRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(bizException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                        GetDefectList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void remove(C1.WPF.DataGrid.C1DataGrid datagrid, string sCol)
        {      
            DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
            datagrid.ItemsSource = null;
            Util.GridSetData(datagrid, DataTableConverter.Convert(datagrid.ItemsSource), FrameOperation, true);
        }
        private void MERGEremove(C1.WPF.DataGrid.C1DataGrid datagrid, string _LOTID)
        {
            DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
            datagrid.ItemsSource = dt.Select("LOTID <> '" + _LOTID + "'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select("LOTID <> '" + _LOTID + "'").CopyToDataTable());
            Util.GridSetData(datagrid, DataTableConverter.Convert(datagrid.ItemsSource), FrameOperation, true);
        }

        private void GetProcessDetail(int index)
        {
            try
            {
                _CurrentLot.LOTID = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LOTID"));
                _CurrentLot.VERSION = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "PROD_VER_CODE"));
                _Lane = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY"));
                _Lane_Ptn_qty = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY"));
                _CurrentLot.INPUTQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.GOODQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPSEQ"));
                _LotProcId = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "PROCID"));

                txtVersion.Text = _CurrentLot.VERSION;
                txtLane.Text = _Lane;
                txtUnit.Text = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "UNIT"));

                // LOT GRID
                DataTable _dtLotInfo = new DataTable();
                _dtLotInfo.Columns.Add("LOTID", typeof(string));
                _dtLotInfo.Columns.Add("WIPSEQ", typeof(Int32));
                _dtLotInfo.Columns.Add("INPUTQTY", typeof(double));
                _dtLotInfo.Columns.Add("GOODQTY", typeof(double));
                _dtLotInfo.Columns.Add("GOODPTNQTY", typeof(double));
                _dtLotInfo.Columns.Add("LOSSQTY", typeof(double));
                _dtLotInfo.Columns.Add("DTL_DEFECT", typeof(double));
                _dtLotInfo.Columns.Add("DTL_LOSS", typeof(double));
                _dtLotInfo.Columns.Add("DTL_CHARGEPRD", typeof(double));

                DataRow dRow = _dtLotInfo.NewRow();
                dRow["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                dRow["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);
                dRow["INPUTQTY"] = Util.NVC(_CurrentLot.INPUTQTY);
                dRow["GOODQTY"] = Util.NVC(_CurrentLot.GOODQTY);
                dRow["GOODPTNQTY"] = Convert.ToDouble(Convert.ToDouble(_CurrentLot.GOODQTY) * Convert.ToDouble(_Lane));

                _dtLotInfo.Rows.Add(dRow);

                Util.GridSetData(dgLotInfo, _dtLotInfo, FrameOperation);

                GetDefectList();
                GetQualityList();

                DataTable dtCopy = _dtLotInfo.Copy();
                BindingWipNote(dtCopy);
            }
            catch (Exception ex)
            {
                DataTableConverter.SetValue(dgRewinderInfo.Rows[index].DataItem, "CHK", true);
                Util.MessageException(ex);
            }
        }

        private void GetDefectList()
        {
            try
            {
                Util.gridClear(dgWipReason);
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("RESNPOSITION", typeof(string));

                DataRow Indata = inDataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Util.NVC(cboOperation.SelectedValue);
                Indata["LOTID"] = _CurrentLot.LOTID;
                Indata["RESNPOSITION"] = null;


                inDataTable.Rows.Clear();
                inDataTable.Rows.Add(Indata);

                //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                Util.GridSetData(dgWipReason, dt, FrameOperation);

                GetDefectSum();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetDefectSum()
        {
            double ValueToDefect = 0F;
            double ValueToLoss = 0F;
            double ValueToCharge = 0F;
            double ValueToExceedLength = 0F; //길이초과수량
            decimal totalResnQty = 0;

            int laneqty = int.Parse(txtLane.Text);

            SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);
            totalResnQty = Convert.ToDecimal(ValueToDefect) + Convert.ToDecimal(ValueToLoss) + Convert.ToDecimal(ValueToCharge);

            // 투입량의 제한률 이상 초과하면 입력 금지
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
            decimal inputRateQty = Util.NVC_Decimal(inputQty * inputOverrate);

            if (inputRateQty > 0 && Util.NVC_Decimal(ValueToExceedLength) > inputRateQty)
            {
                Util.MessageValidation("SFU3195", new object[] { Util.NVC(inputOverrate * 100) + "%" });    //투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
                return false;
            }

            // 투입량 대비 제한이 없어서 해당 Validation 추가
            if ((inputQty + Convert.ToDecimal(ValueToExceedLength)) < totalResnQty)
            {
                Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                return false;
            }

            // SET LOT GRID
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", ValueToDefect + ValueToLoss + ValueToCharge);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", ValueToDefect);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_LOSS", ValueToLoss);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_CHARGEPRD", ValueToCharge);

            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY", ((double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge)) * laneqty);

            GetDefectSum_INCR();
            return true;
        }

        private void SumDefectTotalQty(ref double DefectSum, ref double LossSum, ref double ChargeSum, ref double ExceedLength)
        {
            DefectSum = 0;
            LossSum = 0;
            ChargeSum = 0;
            double p1 = 0F;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable defectDt = ((DataView)dgWipReason.ItemsSource).Table;

                foreach (DataRow dr in defectDt.Rows)
                {
                    //20221007 RESN QTY에 문자가 입력된 경우 0으로 변경
                    if ((!string.IsNullOrEmpty(dr["RESNQTY"].ToString())) && double.TryParse((dr["RESNQTY"]).ToString(), out p1))
                    {
                        if (string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "OUT_LOT_QTY_INCR") && Util.NVC_Decimal(dr["DFCT_TAG_QTY"]) > 0)
                        {
                            continue;
                        }
                        else
                        {
                            if (!string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "LENGTH_EXCEED"))
                            {
                                if (!string.Equals(Util.NVC(dr["RSLT_EXCL_FLAG"]), "Y"))
                                {
                                    if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                                        DefectSum += Convert.ToDouble(dr["RESNQTY"]);
                                    else if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                        LossSum += Convert.ToDouble(dr["RESNQTY"]);
                                    else if (string.Equals(Util.NVC(dr["ACTID"]), "CHARGE_PROD_LOT"))
                                        ChargeSum += Convert.ToDouble(dr["RESNQTY"]);
                                }
                            }
                            else
                            {
                                if (!string.Equals(Util.NVC(dr["RSLT_EXCL_FLAG"]), "Y"))
                                {
                                    if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                        ExceedLength = Convert.ToDouble(dr["RESNQTY"]);
                                }
                            }
                        }
                    }
                    else dr["RESNQTY"] =0;
                }
            }

        }

        private void GetDefectSum_INCR()
        {
            double DefectTagSumQty = 0F;
            double DefectBefTagSumQty = 0F;
            
            double DefectBefTagQty = 0F;
            double ValueToTag = 0F;
            
            int laneqty = int.Parse(txtLane.Text);

            SumDefectTotalQty_INCR(ref DefectTagSumQty);
            BeforeProcDefectSum(ref DefectBefTagQty, ref DefectBefTagSumQty);
            
            txtBefTag.Text = DefectBefTagQty.ToString();
            
            if (DefectTagSumQty == 0 && DefectBefTagSumQty==0 ) return;
            
            //이전 불량 태그 수량이 더 큰경우 LOSS엔 반영하지 않음(길이초과 처리 필요) 양품량에는 무조건 적용
            if (DefectBefTagSumQty - DefectTagSumQty > 0)
            {
                ValueToTag = DefectBefTagSumQty - DefectTagSumQty;
            }

            // SET LOT GRID
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY")))));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT"))) ));

            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY")))) + ValueToTag);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY", ((double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY"))) + ValueToTag) * laneqty));
        }

        private void SumDefectTotalQty_INCR(ref double DefectTagSumQty)
        {
            double ValueToDefectTag = 0F;
            double ValueTagConvertRate = 0F;
            

            DefectTagSumQty = 0F;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable defectDt = ((DataView)dgWipReason.ItemsSource).Table;

                foreach (DataRow dr in defectDt.Rows)
                {
                    if (Util.NVC_Decimal(dr["DFCT_TAG_QTY"]) > 0 && (string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "OUT_LOT_QTY_INCR")))
                    {
                        if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                        {
                            ValueToDefectTag = Convert.ToDouble(dr["DFCT_TAG_QTY"]);
                            ValueTagConvertRate = Convert.ToDouble(dr["TAG_CONV_RATE"]);
                            DefectTagSumQty += (ValueToDefectTag * ValueTagConvertRate);
                        }
                    }
                }
            }

        }

        private void BeforeProcDefectSum(ref double DefectBefTagQty, ref double DefectBefTagSumQty)
        {
            double ValueToDefectTag = 0F;
            double ValueTagConvertRate = 0F;
            DefectBefTagSumQty = 0F;
            DefectBefTagQty = 0F;

            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("ACTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["AREAID"] = LoginInfo.CFG_AREA_ID;

            for (int i = 0; i < dgRewinderInfo.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
                    indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "LOTID"));
            }

            indata["ACTID"] = "DEFECT_LOT";
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_TAG_QTY_TAG_CONV_RATE", "INDATA", "RSLTDT", inTable);

            foreach (DataRow dr in _dt.Rows)
            {
                if (Util.NVC_Decimal(dr["DFCT_TAG_QTY"]) > 0 )
                {
                    if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                    {
                        ValueToDefectTag = Convert.ToDouble(dr["DFCT_TAG_QTY"]);
                        ValueTagConvertRate = Convert.ToDouble(dr["TAG_CONV_RATE"]);
                        DefectBefTagQty += Convert.ToDouble(dr["DFCT_TAG_QTY"]);

                        DefectBefTagSumQty += (ValueToDefectTag * ValueTagConvertRate);
                    }
                }
            }
        }


        private void GetTotalQtySum()
        {

            double ValueToDefect = 0F;
            double ValueToLoss = 0F;
            double ValueToCharge = 0F;
            double ValueToExceedLength = 0F; //길이초과수량

            double ValueInput = 0F;             // 투입량
            double Valueoutput = 0F;            // 양품량

            double DefectTagSumQty = 0F;        // 현재공정 OUT_LOT_QTY_INCR 불량 수량 총합 -> sum(태그수 * 태그 변환률)

            double DefectBefTagQty = 0F;        // 이전공정 태그 총합
            double DefectBefTagSumQty = 0F;     // 이전공정 불량 수량 -> sum(이전공장 태그수 * 태그변환률)

            double ValueToTag = 0F;
            double p1 = 0F;

            SumDefectTotalQty_INCR(ref DefectTagSumQty);
            BeforeProcDefectSum(ref DefectBefTagQty, ref DefectBefTagSumQty);

            //SumDefectTotalQty_INCR() 함수 내에서 ValueToDefectTag 값이 0 인경우 적용 안되기 때문에 0으로 변경해줌
            if (DefectTagSumQty == 0 && DefectBefTagSumQty == 0)
            {
                ValueToTag = 0F;
                for (int i = 0; i < dgWipReason.Rows.Count; i++)
                {
                    if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                    {
                        DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                    }
                }
            }
            else
            {
                ValueToTag = DefectBefTagSumQty - DefectTagSumQty;
                if (ValueToTag < 0)
                {
                    for (int i = 0; i < dgWipReason.Rows.Count; i++)
                    {
                        if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                        {
                            DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                            DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", Math.Abs(ValueToTag));
                        }
                    }
                    ValueToTag = 0F;
                }
                else
                {
                    for (int i = 0; i < dgWipReason.Rows.Count; i++)
                    {
                        if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                        {
                            DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    }
                }
            }

            GetDefectSum();

            SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);
            ValueInput = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY")));//투입수량
            //양품량 + (특정 불량코드를 제외한 불량 수량 총합) - ((전공정에서 불량수량으로 반영된 불량태그총합 - A에 입력한 불량태그 총합) * 태그변환율)
            //INCR 적용이 안된경우 ValueToTag 수량이 0 이기 때문에 관계 없음
            //투입량과 상기 수식의 값이 안맞는 경우 길이초과나 길이부족 처리
            
            //20230206 불량수량 입력시 에러 발생하여 수량값 Decimal 변경 후 다시 double로 변경하여 처리
            Valueoutput = Convert.ToDouble((Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY"))) - Convert.ToDecimal(ValueToExceedLength) + Convert.ToDecimal((ValueToDefect + ValueToLoss + ValueToCharge))));

            double diffQty = Valueoutput - ValueInput;

            if (diffQty == 0)
            {
                for (int i = 0; i < dgWipReason.Rows.Count; i++)
                {
                    if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                    {
                        DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                    }
                }
                return;
            }
            else
            {
                if (diffQty > 0)
                {
                    //길이초과 처리
                    try
                    {
                        for (int i = 0; i < dgWipReason.Rows.Count; i++)
                        {
                            if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                            {
                                DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", Math.Abs(diffQty));
                            }
                            if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                            {
                                DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                            }
                        }
                        SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);
                        DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", ValueToDefect + ValueToLoss + ValueToCharge + DefectTagSumQty);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    Util.AlertInfo("SFU8451", Math.Abs(diffQty));
                }
                //else if (diffQty < 0)
                //{
                //    //길이부족 처리
                //    try
                //    {
                //        for (int i = 0; i < dgWipReason.Rows.Count; i++)
                //        {
                //            if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_LACK"))
                //            {
                //                DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", Math.Abs(diffQty));
                //            }
                //            if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), "LENGTH_EXCEED"))
                //            {
                //                DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNQTY", 0);
                //            }
                //        }
                //        SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);
                //        DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", ValueToDefect + ValueToLoss + ValueToCharge + DefectTagSumQty);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    Util.AlertInfo("SFU8450", Math.Abs(diffQty));
                //}
            }
            return;
        }


        bool IsExceptionDefectResult(string actId, string resnCode)
        {
            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("ACTID", typeof(string));
            inTable.Columns.Add("RESNCODE", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            indata["PROCID"] = Util.NVC(cboOperation.SelectedValue);
            indata["ACTID"] = actId;
            indata["RESNCODE"] = resnCode;
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_ELEC_DEFECT_EXCEPTION", "INDATA", "RSLTDT", inTable);

            if (_dt.Rows[0][0].ToString().Equals("1"))
                return true;
            else
                return false;
        }

        private void ClearData()
        {
            Util.gridClear(dgQuality);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgRemark);
            Util.gridClear(dgLotInfo);

            isDupplicatePopup = false;
            txtVersion.Text = "";
            txtLane.Text = "";

            txtUnit.Text = string.Empty;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                //if (Convert.ToDateTime(txtShiftEndTime.Text) < System.DateTime.Now)
                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Tag = window.USERID;
                txtWorker.Text = window.USERNAME;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMoveStart);
            listAuth.Add(btnMoveCancel);
            listAuth.Add(btnConfirm);

            listAuth.Add(btnSaveDefect);
            listAuth.Add(btnSaveRemark);
            listAuth.Add(btnShift);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private DataTable GetLotInfo(string lotid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["LOTID"] = lotid;
            row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_RW_INFO", "INDATA", "OUTDATA", dt);

            if (result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2025");  //해당하는 LOT정보가 없습니다.
                return null;
            }
            _LotProcId = Convert.ToString(result.Rows[0]["PROCID"]);
            _Wipstat = Convert.ToString(result.Rows[0]["WIPSTAT"]);
            _Hold = Convert.ToString(result.Rows[0]["WIPHOLD"]);
            return result;

        }

        #endregion
        private void SetWipNote()
        {
            try
            {
                if (dgRemark.GetRowCount() < 1)
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
                DataRow inData = null;
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    inData = inTable.NewRow();

                    inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);
                    inData["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(inData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        //Util.AlertByBiz("BR_PRD_REG_WIPHISTORY_NOTE", ex.Message, ex.ToString());
                        return;
                    }

                    Util.MessageInfo("SFU1275");        //정상 처리 되었습니다.
                    remarkFlag = false;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BindingWipNote(DataTable dt)
        {
            if (dgRemark.GetRowCount() > 0) return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = "ALL";
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                inDataRow["REMARK"] = GetWIPNOTE(Util.NVC(_row["LOTID"]));
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(dgRemark, dtRemark, FrameOperation);
        }
        private string GetWIPNOTE(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }

        private void dgRemart_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgRemark.Columns[e.Cell.Column.Index].Name)).Equals(""))
            {
                remarkFlag = true;
            }

            if (dgRemark.Rows.Count < 1) return;
            if (e.Cell.Row.Index != 0) return;

            string strAll = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[e.Cell.Row.Index].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));
            string strTmp = "";
            for (int i = 1; i < dgRemark.Rows.Count; i++)
            {
                strTmp = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));

                if (!string.IsNullOrEmpty(strTmp))
                    strTmp += " " + strAll;
                else
                    strTmp = strAll;

                DataTableConverter.SetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, strTmp);
            }
            DataTableConverter.SetValue(dgRemark.Rows[0].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, "");
        }
        private void dgRemark_Click(object sender, RoutedEventArgs e)
        {
            SetWipNote();
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
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

            // 라벨 발행 권한
            if (LoginInfo.CFG_LABEL_AUTO == "Y" && IsAreaCommonCodeUse("BARCODE_PRINT_PWD", Util.NVC(cboOperation.SelectedValue)))
            {
                ELEC002_070_BARCODE_AUTH authConfirm = new ELEC002_070_BARCODE_AUTH();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(cboOperation.SelectedValue);

                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Delete);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[1] = Util.NVC(cboOperation.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(LoginInfo.USERID);

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void OnCloseAuthConfirm_Delete(object sender, EventArgs e)
        {
            ELEC002_070_BARCODE_AUTH window = sender as ELEC002_070_BARCODE_AUTH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[1] = Util.NVC(cboOperation.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(window.UserName);

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }


        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Util.NVC(cboOperation.SelectedValue);
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                    PreConfirmProcess(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _CurrentLot.LOTID;
                //newRow["WIPSEQ"] = null;
                newRow["WORKER_NAME"] = sWrokerName;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
        
        #region  20200701 불량수량 변경가능여부에 따른 수정색깔 설정
        //20200701 오화백 불량수량변경가능여부에 따른  수정색깔 설정
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                // 수정 가능여부에 따른 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY_APPLY_FLAG")).Equals("Y"))
                    //{
                    //    //e.Cell.Column.IsReadOnly = true;
                    //    //dgWipReason.Columns["RESNQTY"][e.Cell.Row]
                    //    //dgWipReason. 
                    //}
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("DFCT_TAG_QTY") || e.Column.Name.Equals("COUNTQTY") || e.Column.Name.Equals("RESNQTY"))
            {
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").GetString()) && DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").Equals("Y"))
                {
                    e.Cancel = true;
                }
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG").GetString()) && DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG").Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

            if (string.Equals(e.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
            {
                e.Cancel = true;
            }

        }

        #endregion

        #region  20200701 품질정보탭 추가

        //품질항목 조회
        private void GetQualityList()
        {
            try
            {

                if (_CurrentLot.LOTID == string.Empty)
                    return;

                //if (string.Equals(_Wipstat, Wip_State.WAIT))
                //    return;

                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality };
                foreach (C1DataGrid dg in lst)
                {
                    IndataTable.Rows.Clear();

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _LotProcId;
                    Indata["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                    Indata["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                    {
                        Indata["VER_CODE"] = txtVersion.Text;
                        Indata["LANEQTY"] = txtLane.Text
                            ;
                    }
                    IndataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (dt.Rows.Count == 0)
                        dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (dg.Visibility == Visibility.Visible)
                    {
                        Util.GridSetData(dg, dt, FrameOperation, true);
                        _Util.SetDataGridMergeExtensionCol(dg, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        

        /// <summary>
        /// 품질항목 저장
        /// </summary>
        /// <param name="dg"></param>
        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);
            try
            {
                new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        /// <summary>
        /// 품질항목 저장 데이터 테이블
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;

            foreach (DataRow _iRow in dt.Rows)
            {
                inData = IndataTable.NewRow();

                inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inData["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                inData["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();

                inData["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);
                inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            }
            return IndataTable;
        }

        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridFocusChange 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(e.NewValue) == true)
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    int iRowIdx = p.Cell.Row.Index;
                    int iColIdx = p.Cell.Column.Index;
                    C1.WPF.DataGrid.C1DataGrid grid = p.DataGrid;

                    if (grid.CurrentCell.Column.Index != iColIdx)
                        grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridPreviewItmeKeyDown 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);

                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        // 액셀파일 PASTE시 공란PASS없이 전체 붙여넣기 추가 [2019-01-28]
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line.Trim());

                            iRowIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridGotKeyboardLost 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                isDupplicatePopup = true;
                int iRowIdx = 0;
                int iColIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;

                    grid = p.DataGrid;


                    C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);


                    string sLSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "LSL"));
                    string sCSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "CSL"));
                    string sUSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "USL"));


                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        if (sLSL != "" && Util.NVC_Decimal(item.Value) < Util.NVC_Decimal(sLSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        else if (sUSL != "" && Util.NVC_Decimal(item.Value) > Util.NVC_Decimal(sUSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            currentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                     }

                    if (!string.IsNullOrEmpty(Util.NVC(item.Value)) && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        DataTable dataCollect = DataTableConverter.Convert(grid.ItemsSource);
                        int iHGCount1 = 0;  // H/G
                        int iHGCount2 = 0;  // M/S
                        int iHGCount3 = 0;  // 1차 H/G
                        int iHGCount4 = 0;  // 1차 M/S
                        decimal sumValue1 = 0;
                        decimal sumValue2 = 0;
                        decimal sumValue3 = 0;
                        decimal sumValue4 = 0;
                        foreach (DataRow row in dataCollect.Rows)
                        {
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount3++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount4++;
                                    }
                                }
                            }
                        }

                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount3 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount4 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }

                        if (grid.BottomRows.Count > 0)
                            grid.BottomRows[0].Refresh(false);
                    }
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                isDupplicatePopup = false;
            }
        }


        private string GetUnitFormatted()
        {
            string sFormatted = "0";
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        private string GetUnitFormatted(object obj, string pattern)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (pattern)
            {
                case "KG":
                    sFormatted = "{0:###0.000}";
                    break;

                case "M":
                    sFormatted = "{0:###0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:###0.0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        #endregion

        private void dgRewinderInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgRewinderInfo.GetRowCount() == 0) return;

                List<System.Data.DataRow> list = DataTableConverter.Convert(dgRewinderInfo.ItemsSource).Select().ToList();
                List<int> arr = list.GroupBy(c => c["LOTID"]).Select(group => group.Count()).ToList();


                int p = 0;
                for (int j = 0; j < arr.Count; j++)
                {
                    for (int i = 0; i < dgRewinderInfo.Columns.Count; i++)
                    {
                        if (dgRewinderInfo.Columns[i].Name.Equals("FROM_LOTID") || dgRewinderInfo.Columns[i].Name.Equals("LOT_QTY"))
                        {
                            
                        }
                        else
                            e.Merge(new DataGridCellsRange(dgRewinderInfo.GetCell(p, i), dgRewinderInfo.GetCell((p + arr[j] - 1), i)));
                    }
                    p += arr[j];
                }
            }
            catch (Exception ex) { }
        }


        /// <summary>
        /// 팝업 - 대기 재공 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchWaitingWork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 라인을선택하세요
                if (Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223")).Equals("")) return;

                // 공정을선택하세요.
                if (Util.GetCondition(cboOperation, MessageDic.Instance.GetMessage("SFU1459")).Equals("")) return;

                // 설비를 선택하세요.
                if (Util.GetCondition(cboEquipment, MessageDic.Instance.GetMessage("SFU1673")).Equals("")) return;

                ELEC002_070_WAITWORK popWaitingWipLot = new ELEC002_070_WAITWORK();
                popWaitingWipLot.FrameOperation = FrameOperation;

                if (ValidationGridAdd(popWaitingWipLot.Name.ToString()) == false)
                    return;

                object[] Parameters = new object[1];
                Parameters[0] = cboEquipmentSegment.SelectedValue;

                C1WindowExtension.SetParameters(popWaitingWipLot, Parameters);

                popWaitingWipLot.Closed += new EventHandler(popWaitingWipLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popWaitingWipLot.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void popWaitingWipLot_Closed(object sender, EventArgs e)
        {
            try
            {
                ELEC002_070_WAITWORK popup = sender as ELEC002_070_WAITWORK;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    DataTable dtSearch = popup.dtSelect;
                    for (int i = 0; i < dtSearch.Rows.Count; i++)
                    {
                        txtLOTID.Text = dtSearch.Rows[i]["LOTID"].ToString();
                        txtLOTID_KeyDown(null, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            switch (cb.Name)
            {
                case "chkRun":
                case "chkEqpEnd":
                    if (cb.IsChecked == true)
                    {
                        chkRun.IsChecked = true;
                        chkEqpEnd.IsChecked = true;
                    }
                    else
                    {
                        chkRun.IsChecked = false;
                        chkEqpEnd.IsChecked = false;
                    }
                    break;
            }

            if (string.Equals(cb.Name, "chkEqpEnd")) // PROC, EQPT_END가 동시에 체크되기 떄문에 하나만 체크하도록 변경
                return;

            btnSearch_Click(null, null); //조회 실행
        }
    }
}
