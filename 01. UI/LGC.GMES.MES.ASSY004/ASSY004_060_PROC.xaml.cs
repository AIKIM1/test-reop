/*************************************************************************************
 Created Date : 2019.10.28
      Creator : 정문교
   Decription : FOL,STK Rework 공정 진척
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.28  정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
                                          ASSY004_050_PROC Copy ASSY004_060_PROC

  2019.11.09  정문교 : 동 콤보 조회시 Setting에 설정된 동 정보가 아닌 CNB에 해당하는 동정보 전부 갖고오게 수정
  2019.11.27  정문교 : 공급량을 작업자가 입력하는 것에서 양품, 불량~물청 수량 등록 시 실시간으로 공급량 업데이트로 변경
  2019.12.02  정문교 : 시장유형 칼럼 추가
  2021.03.18  안인효 : CSR : C20210316-000472
                       발행 처리 추가
  2024.08.27  안유수 : E20240810-001560 dgOutLot_MouseDoubleClick 이벤트 제거
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060_PROC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_060_PROC : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private BizDataSet _Biz;
        private UserControl _UCParent;
        private Util _Util;
        private UC_IN_OUTPUT winInput = null;

        private DataRow _drSelectRow;
        private bool bSetAutoSelTime = false;
        private bool bMagzinePrintVisible = false;
        private string _EqptType = string.Empty;                          
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;            // 로더 LOT 식별 기준 코드
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;          // 언로더 LOT 식별 기준 코드
        private string _LABEL_PRT_RESTRCT_FLAG = string.Empty;            // 라벨 인쇄 제한 여부

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public ASSY004_060_PROC(UserControl parent)
        {
            _UCParent = parent;
            _Biz = new BizDataSet();
            _Util = new Util();

            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            _drSelectRow = null;

            Util.gridClear(dgProductLot);
            Util.gridClear(dgOutLot);
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpFaulty);

            txtOutBoxQty.Text = string.Empty;
            txtCarrierID.Text = string.Empty;
            txtUserNameCr.Text = string.Empty;
            txtUserNameCr.Tag = string.Empty;
        }

        private void SetControl()
        {
            // 생산 반제품 조회 Timer
            if (dispatcherTimer != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }

        }


        private void CheckAutoPrintUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_AUTO_PRINT_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    chkAutoPrint.Visibility = Visibility.Visible;
                else
                    chkAutoPrint.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "cboAreaAll");

            String[] sFilter = { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "PROCESSEQUIPMENTSEGMENT");

            String[] sFilter2 = { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_PROCID");

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeUserControls();
            SetControl();
            InitCombo();
            CheckAutoPrintUse();
            //Reload방지
            this.Loaded -= UserControl_Loaded;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
            {
                if (Util.NVC((cboEquipmentSegment.Items[cboEquipmentSegment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                {
                    // 기준정보 조회
                    GetLotIdentBasCode();
                    SetInOutCtrlByLotIdentBasCode();    // Lot 식별 기준 코드에 따른 컨트롤 변경.
                    SetUserCheckFlag();
                }
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _EqptType = string.Empty;

            if ((sender as C1ComboBox).SelectedIndex == -1 || e.NewValue.ToString().Trim().Equals("SELECT") || (sender as C1ComboBox).SelectedIndex == 0)
            {
                InitializeUserControls();
            }
            else
            {
                SearchEquipment();
                btnSearch_Click(null, null);
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "EQPT_INPUT_QTY")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
            }));
        }

        private void rbProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            //rb클릭시 Row선택한 것으로 되도록 설정
            //클릭한 Row의 PRODID를 가져옴
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if (rb.IsChecked.HasValue && rb.IsChecked.Value)
            {
                //rb.Parent는 부모가 보는 선택된 한 줄을 의미한다. 따라서 부모가 봤을 때는 선택된 줄이 몇번째인지 알 수 있다.
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                dgProductLot.SelectedIndex = idx;

                #region [버튼 권한 적용에 따른 처리]
                GetButtonPermissionGroup();
                #endregion

                // 선택 Row 
                _drSelectRow = dtRow;

                SearchOutLot();
                SearchDefectInfo();
                SearchDefectEqpt();
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            SearchProductLot();
        }

        /// <summary>
        /// 작업시작
        /// </summary>
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart()) return;

            PopupRunStart();
        }

        /// <summary>
        /// 시작취소
        /// </summary>
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunCancel())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveRunCancel();
                }
            });
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEnd()) return;

            if (_EqptType == "D")
            {
                // 작업 설비가 가상설비이면 팝업 없이 장비완료 처리만 
                SaveEqptEnd();
            }
            else
            {
                PopupEqptEnd();
            }
        }

        /// <summary>
        /// 장비완료 취소
        /// </summary>
        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEndCancel())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveEqptEndCancel();
                }
            });
        }

        /// <summary>
        /// 실적 확정
        /// </summary>
        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEnd()) return;

            PopupEnd();
        }

        #region ##생산반제품 탭

        private void dgOutLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (bMagzinePrintVisible)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                }
            }));
        }

        private void dgOutLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }
        
        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetAutoSelTime = true;
                        return;
                    }

                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageValidation("SFU1605", cboAutoSearchOut.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void txtOutBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutBoxQty.Text, 0))
                {
                    txtOutBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtCarrierID.Visibility == Visibility.Visible)
                        txtCarrierID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCarrierID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.SetPreferredImeConversionMode(txtCarrierID, ImeConversionModeValues.Alphanumeric);
        }

        private void txtCarrierID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtCarrierID.Text = string.Empty;
                            txtCarrierID.Focus();
                        });

                        return;
                    }

                    CreateOutProdWithOutMsg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        /// <summary>
        /// 생산반제품 생성
        /// </summary>
        private void btnCreateOutLot_Click(object sender, RoutedEventArgs e)
        {
            CreateOutProdWithOutMsg();
        }

        /// <summary>
        /// 생산반제품 삭제
        /// </summary>
        private void btnDeleteOutLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteOutLot())
                return;

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        DeleteOutLot();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }

        /// <summary>
        /// 생산반제품 저장
        /// </summary>
        private void btnSaveOutLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveOutLot())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        SaveOutLot();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }
        #endregion

        #region ##불량/Loss/물품청구 탭

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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!e.Cell.Column.Name.Equals("ACTNAME"))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        //}
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?            
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect();
                }
            });
        }
        #endregion

        #region ##설비불량정보 탭

        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            PopupEqptDeftCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgEqpFaulty.TopRows.Count; i < dgEqpFaulty.Rows.Count; i++)
                {
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") ||
                           Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion


        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 20.10.22 3) 고정식BCR 사용 시 라벨 출력 항목 수정
                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        try
                        {
                            btnOutPrint.IsEnabled = false;

                            DataTable dtTmp = null;

                            for (int i = 0; i < dgOutLot.Rows.Count - dgOutLot.BottomRows.Count; i++)
                            {
                                if (!_Util.GetDataGridCheckValue(dgOutLot, "CHK", i)) continue;


                                DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "LOTID")));

                                if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                    dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "DISPATCH_YN")).Equals("Y") ? "Y" : "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                    dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (dtTmp == null)
                                    dtTmp = dtRslt.Copy();
                                else
                                    dtTmp.Merge(dtRslt);
                            }

                            if (dtTmp == null) return;

                            using (ThermalPrint thmrPrt = new ThermalPrint())
                            {
                                THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;

                                thmrPrt.Print(sEqsgID: Util.NVC(cboEquipmentSegment.SelectedValue),
                                              sEqptID: Util.NVC(cboEquipment.SelectedValue),
                                              sProcID: Process.STACKING_FOLDING,
                                              inData: dtTmp,
                                              iType: type,
                                              iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                              bSavePrtHist: true,
                                              bDispatch: true);

                                GetOutProduct();

                                //if (chkAll != null && chkAll.IsChecked.HasValue)
                                //    chkAll.IsChecked = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            btnOutPrint.IsEnabled = true;
                        }
                    }
                    else
                        BoxIDPrint();
                }
            });
        }


        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutProduct();
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 생산LOT 조회
        /// </summary>
        private void SearchProductLot()
        {
            try
            {
                string SelectLot = string.Empty;

                if (_drSelectRow != null)
                {
                    SelectLot = Util.NVC(_drSelectRow["LOTID"]);
                }

                InitializeUserControls();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("PROCID");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = Process.RWK_LNS;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_FOL_STK_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        Util.GridSetData(dgProductLot, searchResult, this.FrameOperation);

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            // 선택 LOT이 있으면 자동 선택
                            int rowindex = -1;
                            DataRow[] dr = searchResult.Select("LOTID = '" + SelectLot + "'");

                            // Proc상태이면 자동 선택
                            if (dr.Length == 0)
                            {
                                dr = searchResult.Select("WIPSTAT = 'PROC'");
                            }

                            // 첫번째 ROW 자동 선택
                            if (dr.Length == 0)
                            {
                                rowindex = 0;
                            }
                            else
                            {
                                rowindex = searchResult.Rows.IndexOf(dr[0]);
                            }

                            if (rowindex >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[rowindex + dgProductLot.TopRows.Count].DataItem, "CHK", false);
                                DataTableConverter.SetValue(dgProductLot.Rows[rowindex + dgProductLot.TopRows.Count].DataItem, "CHK", true);

                                dgProductLot.SelectedIndex = rowindex + dgProductLot.TopRows.Count;
                                _drSelectRow = searchResult.Rows[rowindex];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 시작 취소
        /// </summary>
        private void SaveRunCancel()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_CL", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 장비 완료
        /// </summary>
        private void SaveEqptEnd()
        {
            try
            {
                /////////////////////////////////////////////////////////////////////// DataSet
                DataSet indataSet = new DataSet();

                DataTable inEQP = indataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("PROD_LOTID", typeof(string));
                inEQP.Columns.Add("LOT_MODE", typeof(string));
                inEQP.Columns.Add("AN_LOT_TYPE", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));

                DataTable inOutput = indataSet.Tables.Add("IN_OUTPUT");
                inOutput.Columns.Add("INPUT_QTY", typeof(int));
                inOutput.Columns.Add("OUTPUT_QTY", typeof(int));
                inOutput.Columns.Add("DFCT_QTY", typeof(int));
                inOutput.Columns.Add("BTN_QTY_PRE", typeof(int));
                inOutput.Columns.Add("BTN_QTY_AFT", typeof(int));
                inOutput.Columns.Add("REINPUT_QTY", typeof(int));

                DataTable inDefect = indataSet.Tables.Add("IN_DEFECT");
                inDefect.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                inDefect.Columns.Add("DFCT_QTY", typeof(int));
                inDefect.Columns.Add("PORT_ID", typeof(string));

                /////////////////////////////////////////////////////////////////////// 바인딩
                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["LOT_MODE"] = "L";
                newRow["AN_LOT_TYPE"] = Util.NVC(_drSelectRow["IRREGL_PROD_LOT_TYPE_CODE"]);
                newRow["USERID"] = LoginInfo.USERID;
                inEQP.Rows.Add(newRow);

                newRow = inOutput.NewRow();
                newRow["INPUT_QTY"] = _drSelectRow["EQPT_INPUT_QTY"];
                newRow["DFCT_QTY"] = _drSelectRow["DFCTQTY"];
                newRow["OUTPUT_QTY"] = _drSelectRow["WIPQTY"];
                inOutput.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_PROD_LOT_RWK_FOL_STK_L", "IN_EQP,IN_INPUT,IN_OUTPUT,IN_DEFECT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 장비완료 취소
        /// </summary>
        private void SaveEqptEndCancel()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 EqptType
        /// <summary>
        private void SearchEquipment()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _EqptType = dtResult.Rows[0]["EQPTTYPE"].ToString();
                }
                else
                {
                    _EqptType = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = string.Empty;
                _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtResult.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtResult.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtResult.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetUserCheckFlag()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LABEL_FLAG", "RQSTDT", "RSLDT", inTable);

                if (dtRslt.Rows[0]["LABEL_PRT_RESTRCT_FLAG"].ToString().Trim().ToUpper().Equals("Y"))
                {
                    txtCreater.Visibility = Visibility.Visible;
                    txtUserNameCr.Visibility = Visibility.Visible;
                    btnUserCr.Visibility = Visibility.Visible;

                    _LABEL_PRT_RESTRCT_FLAG = "Y";
                }
                else
                {
                    //grdCreater.Visibility = Visibility.Collapsed;
                    txtCreater.Visibility = Visibility.Collapsed;
                    txtUserNameCr.Visibility = Visibility.Collapsed;
                    btnUserCr.Visibility = Visibility.Collapsed;

                    _LABEL_PRT_RESTRCT_FLAG = "N";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkOutCstCnt()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    HideLoadingIndicator();

                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        #region ## 생산반제품
        /// <summary>
        /// 생산반제품 조회
        /// </summary>
        private void SearchOutLot()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_FD_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutLot.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            double dQty = Convert.ToDouble((searchResult.Rows[0]["WIPQTY"].ToString()));
                            txtOutBoxQty.Text = Convert.ToString(dQty);
                        }
                        else
                        {
                            txtOutBoxQty.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 생산반제품 생성
        /// </summary>
        private void CreateOutLot()
        {
            try
            {
                DataTable inTabl = new DataTable();
                inTabl.Columns.Add("SRCTYPE", typeof(string));
                inTabl.Columns.Add("IFMODE", typeof(String));
                inTabl.Columns.Add("EQPTID", typeof(String));
                inTabl.Columns.Add("USERID", typeof(String));
                inTabl.Columns.Add("PROD_LOTID", typeof(String));

                DataRow dr = inTabl.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["USERID"] = _LABEL_PRT_RESTRCT_FLAG == "Y" ? txtUserNameCr.Tag.ToString() : LoginInfo.USERID;
                dr["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                inTabl.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOT_LABEL_PRT_RESTRCT", "INDATA", null, inTabl, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException, (msgResult) =>
                            {
                                if (txtCarrierID.Visibility == Visibility.Visible)
                                {
                                    txtCarrierID.Text = string.Empty;
                                    txtCarrierID.Focus();
                                }
                            });

                            return;
                        }

                        DataSet indataSet = new DataSet();

                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROD_LOTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("WIPQTY", typeof(int));
                        inInput.Columns.Add("CSTID", typeof(string));

                        DataRow newRow = inData.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                        newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                        newRow["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(newRow);

                        newRow = inInput.NewRow();
                        newRow["WIPQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                        newRow["CSTID"] = txtCarrierID.Text;
                        inInput.Rows.Add(newRow);

                        ShowLoadingIndicator();




                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
                        {
                            try
                            {
                                HideLoadingIndicator();

                                if (searchException != null)
                                {
                                    Util.MessageException(searchException, (msgResult) =>
                                    {
                                        if (txtCarrierID.Visibility == Visibility.Visible)
                                        {
                                            txtCarrierID.Text = string.Empty;
                                            txtCarrierID.Focus();
                                        }
                                    });

                                    return;
                                }
                                string sNewOutLot = searchResult.Tables[0].Rows[0]["LOTID"].ToString();

                                if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                                {
                                    BoxIDPrint(sNewOutLot, Convert.ToDecimal(txtOutBoxQty.Text));
                                }


                                SearchProductLot();



                                Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.
                                txtCarrierID.Text = string.Empty;

                                if (txtCarrierID.Visibility == Visibility.Visible)
                                    txtCarrierID.Focus();
                            }
                            catch (Exception ex)
                            {
                                HideLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, indataSet
                        );

                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 생산반제품 삭제
        /// </summary>
        private void DeleteOutLot()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("WIPQTY_ED", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgOutLot.ItemsSource).Select("CHK = 1");

                foreach (DataRow row in dr)
                {
                    newRow = inInput.NewRow();
                    newRow["LOTID"] = Util.NVC(row["LOTID"]);
                    inInput.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 생산반제품 저장
        /// </summary>
        private void SaveOutLot()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("WIPQTY_ED", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgOutLot.ItemsSource).Select("CHK = 1");
                foreach (DataRow row in dr)
                {
                    newRow = inInput.NewRow();
                    newRow["LOTID"] = Util.NVC(row["LOTID"]);
                    newRow["WIPQTY_ED"] = Util.NVC(row["WIPQTY"]);
                    inInput.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2017.06.28 Add by Kim Joonphil
        /// CST 사용 가능 확인 (폴란드에서 사용)
        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable inEQP = IndataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("CSTID", typeof(string));
                inEQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["CSTID"] = Util.NVC(txtCarrierID.Text.Trim());
                newRow["WIP_TYPE_CODE"] = "OUT";
                inEQP.Rows.Add(newRow);

                DataTable inINPUT = IndataSet.Tables.Add("IN_INPUT");
                inINPUT.Columns.Add("LANGID", typeof(string));
                inINPUT.Columns.Add("PROCID", typeof(string));
                inINPUT.Columns.Add("EQSGID", typeof(string));

                newRow = inINPUT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                inINPUT.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckFirstOutProduction()
        {
            try
            {
                bool bRet = false;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT_BY_PR_LOTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("OUT_PROD_CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["OUT_PROD_CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt < 1)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private bool GetErpSendInfo(string sLotID, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // S : ERP 전송 중 , P : ERP 전송 대기, Y : ERP 전송 완료
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region ## 불량/Loss/물품청구
        /// <summary>
        /// 불량/Loss/물품청구 조회
        /// </summary>
        private void SearchDefectInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]); 
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        /// <summary>
        /// 불량/Loss/물품청구 저장
        /// </summary>
        private void SaveDefect()
        {
            try
            {
                dgDefect.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inData = indataSet.Tables["INDATA"];

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);

                DataTable inResn = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inResn.NewRow();
                    newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                    newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]); ;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inResn.Rows.Add(newRow);
                }

                if (inResn.Rows.Count < 1)
                {
                    return;
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.

                        SearchProductLot();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        #endregion

        #region ## 설비 불량
        /// <summary>
        /// 설비 불량 정보
        /// </summary>
        private void SearchDefectEqpt() 
        {
            try
            {
                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HideLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgEqpFaulty, searchResult, null);

                    dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                    dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                    if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                    else
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;

                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1153");
                return false;
            }

           return true;
        }

        private bool ValidationRunStart()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (dgProductLot?.ItemsSource != null)
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductLot.ItemsSource).Select("WIPSTAT = 'PROC'");

                if (dr.Length > 0)
                {
                    // 장비에 진행 중 인 LOT이 존재 합니다.
                    Util.MessageValidation("SFU1863");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationRunCancel()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!Util.NVC(_drSelectRow["WIPSTAT"]).Equals("PROC"))
            {
                // 작업중인 LOT이 아닙니다.
                Util.MessageValidation("SFU1846");
                return false;
            }

            // 완성 이력 정보 존재여부 확인
            if (ChkOutCstCnt())
            {
                // 완성LOT이 존재하여 작업취소가 불가합니다.
                Util.MessageValidation("SFU4422");   
                return false;
            }

            return true;
        }

        private bool ValidationEqptEnd()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!_drSelectRow["WIPSTAT"].ToString().Equals("PROC"))
            {
                // 장비완료 할 수 있는 LOT상태가 아닙니다.
                Util.MessageValidation("SFU1866");
                return false;
            }

            // 공급 = 양품+불량/LOSS/물청 체크
            decimal InputQty = Util.NVC_Decimal(_drSelectRow["EQPT_INPUT_QTY"].ToString());
            decimal GoodQty = Util.NVC_Decimal(_drSelectRow["WIPQTY"].ToString());
            decimal DefectQty = Util.NVC_Decimal(_drSelectRow["DFCTQTY"].ToString());

            if (InputQty == 0)
            {
                // %1 에 입력된 수량이 없습니다.
                Util.MessageValidation("SFU1289", ObjectDic.Instance.GetObjectName("공급"));
                return false;
            }

            if (InputQty != (GoodQty + DefectQty))
            {
                // 양품수량과 불량수량의 합이 투입수량과 맞지 않습니다.
                Util.MessageValidation("SFU1723");
                return false;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return false;
                }
            }

            return true;
        }

        private bool ValidationEqptEndCancel()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!Util.NVC(_drSelectRow["WIPSTAT"]).Equals("EQPT_END"))
            {
                // 장비완료 상태의 LOT이 아닙니다.
                Util.MessageValidation("SFU1864");  // 
                return false;
            }

            return true;
        }
        private bool ValidationEnd()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!_drSelectRow["WIPSTAT"].ToString().Equals("EQPT_END"))
            {
                // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                Util.MessageValidation("SFU3194");
                return false;
            }

            // 공급 = 양품+불량/LOSS/물청 체크
            decimal InputQty = Util.NVC_Decimal(_drSelectRow["EQPT_INPUT_QTY"].ToString());
            decimal GoodQty = Util.NVC_Decimal(_drSelectRow["WIPQTY"].ToString());
            decimal DefectQty = Util.NVC_Decimal(_drSelectRow["DFCTQTY"].ToString());

            if (InputQty != (GoodQty + DefectQty))
            {
                // 양품수량과 불량수량의 합이 투입수량과 맞지 않습니다.
                Util.MessageValidation("SFU1723");
                return false;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return false;
                }
            }

            return true;
        }

        private bool ValidationCreateOutLot()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutBoxQty.Text.Trim()))
            {
                // 수량을 입력 하세요.
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return false;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                // 수량이 0보다 작습니다.
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return false;
            }

            if (!CheckedUseCassette())  //Cassette 중복여부 체크. 2017.06.28 Add by Kim Joonphil
            {
                txtCarrierID.SelectAll();
                return false;
            }

            if (_LABEL_PRT_RESTRCT_FLAG == "Y")
            {
                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    txtUserNameCr.Focus();
                    return false;
                }
            }

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                // 카세트ID 10자리 확인 [2018.03.14]
                if (txtCarrierID.Text.Length != 10)
                {
                    Util.MessageValidation("SFU4571");
                    txtCarrierID.Text = string.Empty;
                    txtCarrierID.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDeleteOutLot()
        {
            DataRow[] dr = DataTableConverter.Convert(dgOutLot.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            // ERP 전송 여부 확인.
            foreach (DataRow row in dr)
            {
                if (!GetErpSendInfo(Util.NVC(row["LOTID"]), Util.NVC(row["WIPSEQ"])))
                {
                    // [{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.
                    Util.MessageValidation("SFU1283", Util.NVC(row["LOTID"]));
                    return false;
                }
            }

            return true;
        }

        private bool ValidationSaveOutLot()
        {
            DataRow[] dr = DataTableConverter.Convert(dgOutLot.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 수량 체크
            foreach (DataRow row in dr)
            {
                if (Util.NVC_Decimal(Util.NVC(row["WIPQTY"])) < 0)
                {
                    // 수량은 0보다 커야 합니다.
                    Util.MessageValidation("SFU1683");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (_drSelectRow == null || string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                // 불량 항목이 없습니다.
                Util.MessageValidation("SFU1578");      
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]

        /// <summary>
        /// 작업시작 
        /// </summary>
        private void PopupRunStart()
        {
            ASSY004_060_RUNSTART popupRunStart = new ASSY004_060_RUNSTART();
            popupRunStart.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popupRunStart.Name.ToString()) == false) 
                return;

            object[] Parameters = new object[3];
            Parameters[0] = cboArea.SelectedValue.ToString();
            Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
            Parameters[2] = cboEquipment.SelectedValue.ToString();

            C1WindowExtension.SetParameters(popupRunStart, Parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY004_060_RUNSTART popup = sender as ASSY004_060_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                _drSelectRow = null;
                SearchProductLot();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEnd()
        {
            ASSY004_060_EQPTEND popupEqptEnd = new ASSY004_060_EQPTEND();
            popupEqptEnd.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popupEqptEnd.Name.ToString()) == false)
                return;

            object[] Parameters = new object[10];
            Parameters[0] = Process.RWK_LNS;
            Parameters[1] = cboEquipmentSegment.SelectedValue;
            Parameters[2] = cboEquipment.SelectedValue;
            Parameters[3] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[4] = Util.NVC(_drSelectRow["WIPSEQ"]);
            Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
            Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
            Parameters[7] = true;
            Parameters[8] = "N";
            Parameters[9] = _drSelectRow["EQPT_INPUT_QTY"];

            C1WindowExtension.SetParameters(popupEqptEnd, Parameters);

            popupEqptEnd.Closed += new EventHandler(popupEqptEnd_Closed);
            grdMain.Children.Add(popupEqptEnd);
            popupEqptEnd.BringToFront();
        }

        private void popupEqptEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_060_EQPTEND popup = sender as ASSY004_060_EQPTEND;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                SearchProductLot();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 실적확정
        /// </summary>
        private void PopupEnd()
        {
            ASSY004_060_CONFIRM popupEnd = new ASSY004_060_CONFIRM();
            popupEnd.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popupEnd.Name.ToString()) == false)
                return;

            object[] Parameters = new object[14];
            Parameters[0] = Process.RWK_LNS;
            Parameters[1] = cboEquipmentSegment.SelectedValue;
            Parameters[2] = cboEquipment.SelectedValue;

            Parameters[3] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[4] = Util.NVC(_drSelectRow["WIPSEQ"]);
            Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
            Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;

            Parameters[7] = null;                 //Util.NVC(txtShift.Text);
            Parameters[8] = null;                 //Util.NVC(txtShift.Tag);
            Parameters[9] = null;                 //Util.NVC(txtWorker.Text);
            Parameters[10] = null;                //Util.NVC(txtWorker.Tag);
            Parameters[11] = true;
            Parameters[12] = "N";
            Parameters[13] = _drSelectRow["EQPT_INPUT_QTY"];

            C1WindowExtension.SetParameters(popupEnd, Parameters);

            popupEnd.Closed += new EventHandler(popupEnd_Closed);
            grdMain.Children.Add(popupEnd);
            popupEnd.BringToFront();
        }

        private void popupEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_060_CONFIRM popup = sender as ASSY004_060_CONFIRM;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                SearchProductLot();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 반제품Cell
        /// </summary>
        /// <param name="portID"></param>
        private void PopupOutCell(string lotID, string cstID)
        {
            CMM_ASSY_CELL_INFO popupCell = new CMM_ASSY_CELL_INFO();
            popupCell.FrameOperation = FrameOperation;

            object[] Parameters = new object[2];
            Parameters[0] = lotID;
            Parameters[1] = cstID;

            C1WindowExtension.SetParameters(popupCell, Parameters);

            popupCell.Closed += new EventHandler(popupCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popupCell.ShowModal()));
        }

        private void popupCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO popup = sender as CMM_ASSY_CELL_INFO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 불량Cell
        /// </summary>
        /// <param name="portID"></param>
        private void PopupEqptDeftCell(string portID)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popupEqptDfctCell = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
            popupEqptDfctCell.FrameOperation = FrameOperation;

            object[] Parameters = new object[3];

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            if (idx < 0) return;

            Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[1] = cboEquipment.SelectedValue;
            Parameters[2] = portID;

            C1WindowExtension.SetParameters(popupEqptDfctCell, Parameters);

            popupEqptDfctCell.Closed += new EventHandler(popupEqptDfctCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popupEqptDfctCell.ShowModal()));
        }

        private void popupEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popup = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 사용자 팝업
        /// </summary>
        private void PopupUser()
        {
            CMM_PERSON popupUser = new CMM_PERSON();
            popupUser.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];
            Parameters[0] = txtUserNameCr.Text;
            C1WindowExtension.SetParameters(popupUser, Parameters);

            popupUser.Closed += new EventHandler(popupUser_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupUser.ShowModal()));
        }

        private void popupUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameCr.Text = popup.USERNAME;
                txtUserNameCr.Tag = popup.USERID;
            }
        }

        #endregion

        #region [Function]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRunStart);
            listAuth.Add(btnEqptEnd);
            listAuth.Add(btnEnd);

            listAuth.Add(btnCreateOutLot);
            listAuth.Add(btnDeleteOutLot);
            listAuth.Add(btnSaveOutLot);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (dgProductLot == null || dgProductLot.GetRowCount() < 1)
                        return;

                    SearchOutLot();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void SetInOutCtrlByLotIdentBasCode()
        {
            try
            {
                // Unloader                
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                {
                    if (dgOutLot.Columns.Contains("CSTID"))
                        dgOutLot.Columns["CSTID"].Visibility = Visibility.Visible;

                    tbCarrier.Visibility = Visibility.Visible;
                    txtCarrierID.Visibility = Visibility.Visible;

                    grdOutTranPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutLot, false);
                }
                else if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgOutLot.Columns.Contains("CSTID"))
                        dgOutLot.Columns["CSTID"].Visibility = Visibility.Visible;

                    tbCarrier.Visibility = Visibility.Visible;
                    txtCarrierID.Visibility = Visibility.Visible;

                    grdOutTranPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutLot, true);
                }
                else
                {
                    if (dgOutLot.Columns.Contains("CSTID"))
                        dgOutLot.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    tbCarrier.Visibility = Visibility.Collapsed;
                    txtCarrierID.Visibility = Visibility.Collapsed;

                    grdOutTranPrint.Visibility = Visibility.Visible;

                    SetGridData(dgOutLot, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetGridData(C1DataGrid dg, bool bRfid)
        {
            try
            {
                if (bRfid)
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 4;
                    dg.Columns["PRODID"].DisplayIndex = 5;

                    dg.Columns["WIPQTY"].DisplayIndex = 6;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 7;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN"].DisplayIndex = 9;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 10;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 11;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 12;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 13;
                    dg.Columns["CSTID"].DisplayIndex = 3;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;
                }
                else
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 3;
                    dg.Columns["PRODID"].DisplayIndex = 4;

                    dg.Columns["WIPQTY"].DisplayIndex = 5;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 6;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 7;
                    dg.Columns["PRINT_YN"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 9;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 10;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 11;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 12;
                    dg.Columns["CSTID"].DisplayIndex = 13;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateOutProdWithOutMsg()
        {
            if (!ValidationCreateOutLot())
                return;

            if (CheckFirstOutProduction())
            {
                //"생산 수량 %1 개로 생성 하시겠습니까?"
                Util.MessageConfirm("SFU4888", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOutLot();
                    }
                }, txtOutBoxQty.Text);
            }
            else
            {
                //"생성 하시겠습니까?"
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOutLot();
                    }
                });
            }
        }

        #endregion


        private bool CanPrint()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutLot.Rows.Count - dgOutLot.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutLot, "CHK", i)) continue;

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWORK", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetOutProduct()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_ST_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutLot, searchResult, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void BoxIDPrint(string sBoxID = "", decimal dQty = 0)
        {
            try
            {
                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnOutPrint.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                if (!sBoxID.Equals(""))
                {
                    // 발행..
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩
                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    dicList.Add(dicParam);
                }
                else
                {
                    for (int i = 0; i < dgOutLot.Rows.Count - dgOutLot.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgOutLot, "CHK", i)) continue;

                        DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "LOTID")));

                        if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                        Dictionary<string, string> dicParam = new Dictionary<string, string>();

                        //폴딩
                        dicParam.Add("reportName", "Fold");
                        dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                        dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                        dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                        dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                        dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                        dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                        dicParam.Add("TITLEX", "BASKET ID");

                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                        dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutLot.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                        dicList.Add(dicParam);
                    }
                }

                CMM_THERMAL_PRINT_FOLD print = new CMM_THERMAL_PRINT_FOLD();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[3] = cboEquipment.SelectedValue.ToString();
                    Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "N";   // 디스패치 처리.

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.Show();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnOutPrint.IsEnabled = true;
            }
        }

        private void GetButtonPermissionGroup()
        {
            try
            {
                InitializeButtonPermissionGroup();

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = this.GetType().Name;    // 화면ID

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtRslt.Rows)
                        {
                            if (drTmp == null) continue;

                            switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                            {
                                //case "INPUT_W": // 투입 사용 권한
                                //case "WAIT_W": // 대기 사용 권한
                                //case "INPUTHIST_W": // 투입이력 사용 권한
                                //    SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                //    break;
                                //case "OUTPUT_W": // 생산반제품 사용 권한
                                //    grdOutTranBtn.Visibility = Visibility.Visible;
                                //    break;
                                //case "LOTSTART_W": // 작업시작 사용 권한
                                //    btnRunStart.Visibility = Visibility.Visible;
                                //    break;
                                //case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                //    btnCancelConfirm.Visibility = Visibility.Visible;
                                //    break;
                                case "OUTPUT_W_C1":         // 발행 사용권한
                                    bMagzinePrintVisible = true;
                                    grdOutTranPrint.Visibility = Visibility.Visible;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                // 재작업 Lot 이면 생산 반제품 기능 제공.
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx >= 0)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "IRREGL_PROD_LOT_TYPE_CODE")).Equals("R"))
                    {
                        grdOutTranBtn.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HideLoadingIndicator();
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                grdOutTranBtn.Visibility = Visibility.Collapsed;
                //btnRunStart.Visibility = Visibility.Collapsed;
                //btnCancelConfirm.Visibility = Visibility.Collapsed;
                grdOutTranPrint.Visibility = Visibility.Collapsed;

                //if (winInput == null)
                //    return;

                //winInput.InitializePermissionPerInputButton();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPermissionPerInputButton(string sBtnPermissionGrpCode)
        {
            if (winInput == null)
                return;

            winInput.SetPermissionPerButton(sBtnPermissionGrpCode);
        }


        #endregion
    }
}
