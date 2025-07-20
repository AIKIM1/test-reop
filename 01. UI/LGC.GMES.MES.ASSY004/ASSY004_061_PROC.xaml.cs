/*************************************************************************************
 Created Date : 2021.12.14
      Creator : 오화백
   Decription : FOL,STK Rework 공정 진척 (ESWA)
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.02  박성진  E20231219-001011   반제품 생성시 호출되는 Tables[0] 이 ‘OUTDATA’가 아니라 ‘CALLCOUNT_INFO’를 가져오는 부분 수정
  2024.07.04  이동주  E20240703-000930   FOL/STK 재작업 설비는 MAIN만 조회되도록 변경
  2024.08.27  안유수  E20240810-001560  dgOutLot_MouseDoubleClick 이벤트 제거
  2025.01.16  안민호  설비 완공 LOT은 [불량/LOSS/물품청구 TAP] 불량/LOSS/물품청구 변경 못하도록 함 - 메시지 박스 다국어 처리
                      설비 완공 LOT은 [생산반제품 TAP] 생성 못하도록 함 - 메시지 박스 다국어 처리
  2025.02.12  이민형  김선영 부장님 요청으로 저장버튼 안보이게 처리 생산수량 변경 불가 처리
  2025.02.12  이민형  김광희 책임님 요청으로 저장버튼 및 발행 버튼 제거
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
    public partial class ASSY004_061_PROC : UserControl, IWorkArea
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

        private string _PJT = string.Empty;  // 선택한 PJT 정보
        private string _SELECT_PRODID = string.Empty; //선택한 PRODID

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public ASSY004_061_PROC(UserControl parent)
        {
            _UCParent = parent;
            _Biz = new BizDataSet();
            _Util = new Util();

            InitializeComponent();
            this.Dispatcher.BeginInvoke
          (
              System.Windows.Threading.DispatcherPriority.Input, (System.Threading.ThreadStart)(() =>
              {
                  SetChangeDatePlan();
              }
          ));
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
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_MAIN_LEVEL"); // 2024.07.04 DJLEE FOL/STK 재작업 설비는 MAIN만 조회되도록 변경

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;

            string[] sFilter5 = { cboEquipment.SelectedValue.ToString(), null }; // 자재,제품 전체
            _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

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
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgCurrIn);
                Util.gridClear(dgInputHist);
            }
            else
            {
                CommonCombo _combo = new CommonCombo();
                string[] sFilter5 = { cboEquipment.SelectedValue.ToString(), null }; // 자재,제품 전체
                _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                SearchEquipment();
                SetWoinfo();
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
                GetCurrInList();
                GetInputHistory();
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
                            //btnOutPrint.IsEnabled = false;

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
                            //btnOutPrint.IsEnabled = true;
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

                        GetCurrInList();
                        GetInputHistory();
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
        /// 설비 EqptType
        /// <summary>
        private void SetWoinfo(bool isSelectFlag = true)
        {
            try
            {
                string sPrvWODTL = string.Empty;

                if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                    if (idx >= 0)
                    {
                        sPrvWODTL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"));

                        //if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                        //    btnSelectCancel.IsEnabled = true;
                    }
                }

                // 취소인 경우에는 선택 없애도록..
                if (!isSelectFlag)
                {
                    sPrvWODTL = "";
                    _PJT = string.Empty;
                    _SELECT_PRODID = string.Empty;
                }


                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["STDT"] = (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMM") + "01")); //dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FOL_STK_WORKORDER_LIST", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgWorkOrder, dtResult, FrameOperation, true);

                // 현 작업지시 정보 Top Row 처리 및 고정..
                if (dtResult.Rows.Count > 0)
                {
                    if (!Util.NVC(dtResult.Rows[0]["EIO_WO_SEL_STAT"]).Equals(""))
                        dgWorkOrder.FrozenTopRowsCount = 1;
                    else
                        dgWorkOrder.FrozenTopRowsCount = 0;
                }

          

                // 이전 선택 작지 선택
                if (!sPrvWODTL.Equals(""))
                {
                    int idx = _Util.GetDataGridRowIndex(dgWorkOrder, "EIO_WO_SEL_STAT", sPrvWODTL);

                    if (idx >= 0)
                    {
                        for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                            if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                            else
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);

                        DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                        // 재조회 처리.
                        ReChecked(idx);

            
                    }
                }
                else // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
                {
                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            dgWorkOrder.SelectedIndex = i;
                             break;
                        }
                    }
                }

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                    if (idx != -1)
                    {
                        string workState = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"));
                        btnSelectCancel.IsEnabled = workState == "Y";

                        _PJT = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRJT_NAME"));
                        _SELECT_PRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                    }
                }

              


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = iRow;

                //_PJT = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "PRJT_NAME"));
                //_SELECT_PRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "PRODID"));
                // 선택 작지 수량 설정
                //SetWorkOrderQtyInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);

                // 실적 조회 호출..
                //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                //SearchParentProductInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);
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

                                //string sNewOutLot = searchResult.Tables[0].Rows[0]["LOTID"].ToString();
                                string sNewOutLot = searchResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();

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
            if(_SELECT_PRODID == string.Empty)
            {
                // 제품을 선택하세요
                Util.MessageValidation("SFU1895");
                return false;
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

            // 설비 완공 상태에서 생성할 수 없도록 한다.
            if (string.Equals(_drSelectRow["WIPSTAT"], "EQPT_END"))
            {
                // 설비 완공 상태에서는 생성할 수 없습니다
                Util.MessageValidation("SFU9957");
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

            // 설비 완공 상태에서 불량//LOSS/물품청구 정보를 수정 할 수 없도록 한다.
            if(string.Equals(_drSelectRow["WIPSTAT"], "EQPT_END"))
            {
                // 설비 완공 상태에서는 수정할 수 없습니다
                Util.MessageValidation("SFU9955");
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
            ASSY004_061_RUNSTART popupRunStart = new ASSY004_061_RUNSTART();
            popupRunStart.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popupRunStart.Name.ToString()) == false) 
                return;

            object[] Parameters = new object[5];
            Parameters[0] = cboArea.SelectedValue.ToString();
            Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
            Parameters[2] = cboEquipment.SelectedValue.ToString();
            Parameters[3] = _PJT; //PJT
            Parameters[4] = _SELECT_PRODID; //제품ID

            C1WindowExtension.SetParameters(popupRunStart, Parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY004_061_RUNSTART popup = sender as ASSY004_061_RUNSTART;
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
            ASSY004_061_EQPTEND popupEqptEnd = new ASSY004_061_EQPTEND();
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
            ASSY004_061_EQPTEND popup = sender as ASSY004_061_EQPTEND;
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
            ASSY004_061_CONFIRM popupEnd = new ASSY004_061_CONFIRM();
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
            ASSY004_061_CONFIRM popup = sender as ASSY004_061_CONFIRM;
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
            //listAuth.Add(btnSaveOutLot);

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

                    //grdOutTranPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutLot, false);
                }
                else if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgOutLot.Columns.Contains("CSTID"))
                        dgOutLot.Columns["CSTID"].Visibility = Visibility.Visible;

                    tbCarrier.Visibility = Visibility.Visible;
                    txtCarrierID.Visibility = Visibility.Visible;

                    //grdOutTranPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutLot, true);
                }
                else
                {
                    if (dgOutLot.Columns.Contains("CSTID"))
                        dgOutLot.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    tbCarrier.Visibility = Visibility.Collapsed;
                    txtCarrierID.Visibility = Visibility.Collapsed;

                    //grdOutTranPrint.Visibility = Visibility.Visible;

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

                //btnOutPrint.IsEnabled = false;

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
                //btnOutPrint.IsEnabled = true;
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
                                    //grdOutTranPrint.Visibility = Visibility.Visible;
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
                //grdOutTranPrint.Visibility = Visibility.Collapsed;

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


        private void dgWorkOrder_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;
                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content is RadioButton)
                            {
                                RadioButton rdoButton = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

                                if (rdoButton != null)
                                {
                                    if (rdoButton.DataContext == null)
                                        return;

                                    // 부모 조회 없으므로 로직 수정..
                                    if (!(bool)rdoButton.IsChecked && (rdoButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                                    {
                                        DataRow dtRow = (rdoButton.DataContext as DataRowView).Row;

                                        for (int i = 0; i < dg.Rows.Count; i++)
                                            if (e.Cell.Row.Index == i)   // Mode = OneWay 이므로 Set 처리.
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                                            else
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                        //row 색 바꾸기
                                        dgWorkOrder.SelectedIndex = e.Cell.Row.Index;
              
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = idx;

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "EIO_WO_SEL_STAT").GetString();
                    btnSelectCancel.IsEnabled = workState == "Y";
                }
            }
        }



        private void SetChangeDatePlan(bool isInitFlag = true)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (isInitFlag)
            {
                if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateFrom.Tag = "CHANGE";

                    dtpDateTo.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateTo.Tag = "CHANGE";
                }
            }
            else
            {
                if (Util.NVC_Decimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate;
                    dtpDateFrom.Tag = "CHANGE";
                }

                if (Util.NVC_Decimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateTo.SelectedDateTime = currDate;
                    dtpDateTo.Tag = "CHANGE";
                }
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }



        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

            if (idx < 0)
                return;

            DataRow dtRow = (dgWorkOrder.Rows[idx].DataItem as DataRowView).Row;
          
            if (!CanChangeWorkOrder(dtRow))
                return;
            //변경하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(dtRow);
                    //SetWorkOrderQtyInfo(sWorkOrder);
                }
            });
        }



        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipment.SelectedIndex == 0 || cboEquipmentSegment.SelectedIndex == 0 || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;

                DataRowView drv = dgWorkOrder.Rows[idx].DataItem as DataRowView;
                if (drv != null)
                {
                    DataRow dr = drv.Row;
                   
                    // 취소 하시겠습니까?
                    Util.MessageConfirm("SFU4616", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SelectWorkInProcessStatus(cboEquipment.SelectedValue.ToString(), Process.RWK_LNS, cboEquipmentSegment.SelectedValue.ToString(), (table, ex) =>
                            {
                                if (CommonVerify.HasTableRow(table))
                                {
                                    if (table.Rows[0]["WIPSTAT"].GetString() == "PROC")
                                    {
                                        Util.MessageValidation("SFU1917");
                                        return;
                                    }
                                }
                                else
                                {
                                    WorkOrderChange(dr, false);
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private static void SelectWorkInProcessStatus(string eqptCode, string processCode, string eqptSegmentCode, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = eqptCode;
                inData["PROCID"] = processCode;
                inData["EQSGID"] = eqptSegmentCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    actionCompleted?.Invoke(result, null);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        /// <summary>
        /// 작업지시 선택 가능 Validation 처리
        /// </summary>
        /// <param name="iRow">선택한 작업지시 정보 Row Number</param>
        /// <returns></returns>
        private bool CanChangeWorkOrder(DataRow dtRow)
        {
            bool bRet = false;

            if (dtRow == null)
                return bRet;

            if (cboEquipment.SelectedIndex ==0 || cboEquipmentSegment.SelectedIndex == 0)
                return bRet;


            if (SelectProcStateLot())
            {
                Util.MessageValidation("SFU1917");
                return bRet;
            }
            bRet = true;
            return bRet;
        }

        private bool SelectProcStateLot()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inData["PROCID"] = Process.RWK_LNS;
                inData["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("WIPSTAT"))
                {
                    if (dtRslt.Rows[0]["WIPSTAT"].Equals("PROC"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        private void WorkOrderChange(DataRow dr, bool isSelectFlag = true)
        {
            if (dr == null) return;

            SetWorkOrderSelect(dr, isSelectFlag);

            SetWoinfo(isSelectFlag);
          
        }
        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="dr">선택 한 작업지시 정보 DataRow</param>
        /// <param name="isSelectFlag">선택 처리:true 선택 취소:fals</param>
        private void SetWorkOrderSelect(DataRow dr, bool isSelectFlag = true)
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(dr["WO_DETL_ID"]) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                if (isSelectFlag)
                    Util.MessageInfo("SFU1166");    //변경 되었습니다.
                else
                    Util.MessageInfo("SFU1937");    //취소 되었습니다..
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private struct PRV_VALUES
        {
            public string sPrvOutTray;
            public string sPrvCurrIn;
            public string sPrvOutBox;

            public PRV_VALUES(string sTray, string sIn, string sBox)
            {
                this.sPrvOutTray = sTray;
                this.sPrvCurrIn = sIn;
                this.sPrvOutBox = sBox;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("", "", "");

        public void GetCurrInList()
        {
            try
            {
             
                //if (_drSelectRow == null)
                //{
                //   btnCurrInEnd.IsEnabled = false;
                //}
                //else
                //{
                //   btnCurrInEnd.IsEnabled = true;
                //}
                string sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_L";
               
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = _drSelectRow == null ? null : Util.NVC(_drSelectRow["LOTID"]);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation, true);

                        if (!_PRV_VLAUES.sPrvCurrIn.Equals(""))
                        {
                            int idx = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", _PRV_VLAUES.sPrvCurrIn);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgCurrIn.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgCurrIn.SelectedIndex = idx;

                                dgCurrIn.ScrollIntoView(idx, dgCurrIn.Columns["CHK"].Index);
                            }
                        }
                        dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                        //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        SetElecTypeCount();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                  
                }
                );
            }
            catch (Exception ex)
            {
           
                Util.MessageException(ex);
            }
        }
        private void SetElecTypeCount()
        {
            int iCtype = 0;
            int iAtype = 0;

            if (dgCurrIn.ItemsSource == null || dgCurrIn.Rows.Count < 1)
                return;

            for (int i = 0; i < dgCurrIn.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("CT")) iCtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("AT")) iAtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("MC")) iCtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("HC")) iAtype++;
                }
            }
            tbATypeCnt.Text = iAtype.ToString();
        
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCurrIn.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgCurrIn.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    //if (Util.NVC(row["DISPATCH_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                    //{
                    row["CHK"] = true;
                    //}
                }
                dgCurrIn.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCurrIn.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgCurrIn.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    row["CHK"] = false;
                }
                dgCurrIn.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private bool CanCurrAutoInputLot()
        {
            bool bRet = false;

            if (txtCurrInLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");
                return bRet;
            }

            if (_drSelectRow == null)
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return bRet;
            }
            if(Util.NVC(_drSelectRow["WIPSTAT"]) != "PROC")
            {
                // 작업중인 LOT이 아닙니다.
                Util.MessageValidation("SFU1846");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID")).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return bRet;
            }

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1957");  // 투입 위치를 선택하세요.
                return bRet;
            }
            else if (drTmp.Length > 1)
            {
                Util.MessageValidation("SUF4961");  // 하나의 투입 위치만 선택하세요.
                return bRet;
            }
         
            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                {
                    Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // %1 에 이미 투입되었습니다.
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }
   
        public void OnCurrAutoInputLot(string sInputLot, string sPstnID)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return ;

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["WIPSTAT"]) == "PROC" ? Util.NVC(_drSelectRow["LOTID"]) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];
                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = sInputLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_RWK_ST_L", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                SearchProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            
            }
        }

        private void txtCurrInLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!CanCurrAutoInputLot())
                    {
                        txtCurrInLotID.Text = "";
                        return;
                    }

                    if (_drSelectRow == null) return;

                    string sInPos = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string sInPosName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

                        object[] parameters = new object[2];
                        parameters[0] = sInPosName;
                        parameters[1] = txtCurrInLotID.Text.Trim();

                        Util.MessageConfirm("SFU1291", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos);
                                txtCurrInLotID.Text = "";
                            }
                        }, parameters);
             
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtCurrInLotID.Text = "";
            }
        }



        private bool CanCurrInCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }
          

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                {
                    //Util.Alert("투입 LOT이 없습니다.");
                    Util.MessageValidation("SFU1945");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }
        /// <summary>
        /// 자재투입취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

                    ASSY004_INPUT_CANCEL_CST wndCancel = new ASSY004_INPUT_CANCEL_CST();
                    wndCancel.FrameOperation = FrameOperation;

                    if (wndCancel != null)
                    {
                        int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                         //자재랏의 PROD LOT 정보가 필요함
                         string PROD_LOT = string.Empty;

                        if (_drSelectRow != null && Util.NVC(_drSelectRow["LOTID"]) == GetMtrllotID(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID")), cboEquipment.SelectedValue.ToString()))
                        {
                          PROD_LOT = Util.NVC(_drSelectRow["LOTID"]);
                         }
                        else
                        {
                         PROD_LOT = GetMtrllotID(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID")), cboEquipment.SelectedValue.ToString());
                        }
                        object[] Parameters = new object[13];
                        Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[1] = cboEquipment.SelectedValue.ToString();
                        Parameters[2] = Process.RWK_LNS;
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                        Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MTRLID"));
                        Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_STAT_CHG_DTTM"));
                        Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        Parameters[10] = "CURR";
                        Parameters[11] = "";
                        Parameters[12] = PROD_LOT;

                        C1WindowExtension.SetParameters(wndCancel, Parameters);

                        wndCancel.Closed += new EventHandler(wndCancel_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndCancel.ShowModal()));
                    }
                
              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void wndCancel_Closed(object sender, EventArgs e)
        {
            ASSY004_INPUT_CANCEL_CST window = sender as ASSY004_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetCurrInList();
                GetInputHistory();
            }
        }


        //투입자재랏 PROD LOT 조회
        private string GetMtrllotID(string MtrlLot, string Eqptid)
        {
            try
            {
                //ShowLoadingIndicator();

                string sProdLot = "";

                DataTable inTable = new DataTable();

                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
                inTable.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["INPUT_LOTID"] = MtrlLot;
                newRow["EQPTID"] = Eqptid;
              
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_BY_EQPTID_WIP_TYPE_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 )
                {
                    sProdLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                }

                return sProdLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                
            }
            catch (Exception ex)
            {
              
                return "";
            }

        }




        private void dgCurrIn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                            {
                                pre.Content = chkAll;
                                e.Column.HeaderPresenter.Content = pre;
                                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            }
                            else
                            {
                                pre.Content = chkAll;
                                e.Column.HeaderPresenter.Content = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgCurrIn_UnloadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }


        private bool CanCurrInReplace()
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }
            else if (drTmp.Length > 1)
            {
                Util.MessageValidation("SUF4961");  // 하나의 투입 위치만 선택하세요.
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanUnmtWaitMtrl()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_PSTN_GR_CODE")).Equals("S")
                && _Util.IsCommonCodeUse("INPUT_MTRL_WAIT_WIP_AREA", LoginInfo.CFG_AREA_ID))
            {
                try
                {
                    // 탈착처리 하시겠습니까?
                    Util.MessageConfirm("SFU5047", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            UnmtWaitMtrl();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        private void UnmtWaitMtrl()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]); 
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
                if (idx < 0)
                {
                  return;
                }

                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                newRow["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_PSTN_STAT_CODE"));
                newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNMOUNT_MTRL_LOT_WAIT", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        SearchProductLot();

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                  
                }, indataSet
                );
            }
            catch (Exception ex)
            {
      
                Util.MessageException(ex);
            }
        }

        private void btnCurrInEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInReplace())
                    return;

                if (CanUnmtWaitMtrl())
                    return;


                ASSY004_COM_INPUT_LOT_END wndEnd = new ASSY004_COM_INPUT_LOT_END();
                wndEnd.FrameOperation = FrameOperation;

                if (wndEnd != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                    string PROD_LOT = string.Empty;

                    if (_drSelectRow != null && Util.NVC(_drSelectRow["LOTID"]) == GetMtrllotID(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID")), cboEquipment.SelectedValue.ToString()))
                    {
                        PROD_LOT = Util.NVC(_drSelectRow["LOTID"]);
                    }
                    else
                    {
                        PROD_LOT = GetMtrllotID(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID")), cboEquipment.SelectedValue.ToString());
                    }

                    object[] Parameters = new object[12];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    Parameters[6] = Process.RWK_LNS;
                    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE")) == string.Empty ? "PROD" : Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
                    Parameters[8] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                    Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "AUTO_STOP_FLAG"));
                    Parameters[11] = PROD_LOT; 
                    C1WindowExtension.SetParameters(wndEnd, Parameters);

                    wndEnd.Closed += new EventHandler(wndEnd_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEnd.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_INPUT_LOT_END window = sender as ASSY004_COM_INPUT_LOT_END;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SearchProductLot();
            }
        }



        //투입이력
        private void GetInputHistory()
        {
            try
            {
                if (_drSelectRow == null)
                    return;
                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]); ;
                newRow["PROD_WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]).Equals("") ? 1 : Convert.ToDecimal(Util.NVC(_drSelectRow["WIPSEQ"]));
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);

                        if (dgInputHist.CurrentCell != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                       
                        txtHistLotID.Text = "";
                    }
                }
                );
            }
            catch (Exception ex)
            {
             
                Util.MessageException(ex);
            }
        }

        private void cboHistMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHistLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInputHistory();
            }
        }

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHistory();
        }

        private void txtHistLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHistLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtHistLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY004_061_WAITLOT wndWaitLot = new ASSY004_061_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                  C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            }
        }
        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY004_061_WAITLOT window = sender as ASSY004_061_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnInputLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY004_061_INPUT_HF_CELL wndInputLot = new ASSY004_061_INPUT_HF_CELL();
            wndInputLot.FrameOperation = FrameOperation;

            if (wndInputLot != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                C1WindowExtension.SetParameters(wndInputLot, Parameters);

                wndInputLot.Closed += new EventHandler(wndInputLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndInputLot.ShowModal()));
            }
        }
        private void wndInputLot_Closed(object sender, EventArgs e)
        {
            ASSY004_061_INPUT_HF_CELL window = sender as ASSY004_061_INPUT_HF_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
    }
}
