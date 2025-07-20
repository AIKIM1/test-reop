/*************************************************************************************
 Created Date : 2020.10.05
      Creator : 신광희
   Decription : CWA3동 증설 - V/D공정진척(Redry / Rewinding) 화면 (ASSY0004.ASSY004_002 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.05  신광희 : Initial Created.
  2020.11.03  오화백 : 추가기능 버튼에 Lamie 대기이동 추가
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using System.Linq;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_008.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private string sCaldate = string.Empty;
        private DateTime dtCaldate;
        private bool bTestMode = false;

        private string _Unit = string.Empty;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private readonly Util _util = new Util();
        private BizDataSet _bizDataSet = new BizDataSet();
        CurrentLotInfo _currentLotInfo = new CurrentLotInfo();

        //private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();

        //2019.02.27 오화백 RF_ID 투입부, 배출부 RFID  
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A7" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY004_008()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            /*
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { Process.VD_LMN, LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboVDProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEqptParent = { cboVDProcess, cboEquipmentSegment  };
            C1ComboBox[] cboEqptChild = { cboMountPstsID };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEqptParent, cbChild: cboEqptChild);

            // 투입위치 코드
            String[] sFilter4 = { "PROD" };
            C1ComboBox[] cboMountPstParent = { cboEquipment };
            _combo.SetCombo(cboMountPstsID, CommonCombo.ComboStatus.NONE, cbParent: cboMountPstParent, sFilter: sFilter4, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_L");
            */
        }

        private void InitializeControl()
        {
            rdoRedry.IsChecked = true;
            rdoRedry.Checked += rdoRewinding_Checked;
            rdoRewinding.Checked += rdoRewinding_Checked;
        }

        private void InitializeCombo()
        {
            // 라인 콤보박스
            SetEquipmentSegmentCombo(cboEquipmentSegment);
            SetEquipmentCombo(cboEquipment);

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //InitCombo();
            InitializeControl();
            InitializeCombo();

            //RegisterName("redBrush", redBrush);
            //HideTestMode();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //GetPilotProdMode();

            if (!ValidationSearch())
            {
                HideLoadingIndicator();
                return;
            }
            GetLotIdentBasCode();

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") && _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                //dgDetail.Columns["CSTID"].Visibility = Visibility.Visible;
                //dgProductLot.Columns["UW_CSTID"].Visibility = Visibility.Visible;
                //dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                //dgDetail.Columns["CSTID"].Visibility = Visibility.Collapsed;
                //dgProductLot.Columns["UW_CSTID"].Visibility = Visibility.Collapsed;
                //dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            GetWorkTarget();
            //GetWorkOrder();
            GetProductLot();
            GetWrokCalander();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStartRun()) return;

            ASSY004_008_RUNSTART popRunstart = new ASSY004_008_RUNSTART();
            popRunstart.FrameOperation = FrameOperation;

            if (popRunstart != null)
            {
                int idx = _util.GetDataGridCheckFirstRowIndex(dgWorkTarget, "CHK");

                object[] parameters = new object[10];
                parameters[0] = cboEquipmentSegment.SelectedValue;
                parameters[1] = cboEquipment.SelectedValue;
                parameters[2] = Process.VD_LMN;
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgWorkTarget.Rows[idx].DataItem, "LOTID"));
                parameters[4] = Util.NVC(DataTableConverter.GetValue(dgWorkTarget.Rows[idx].DataItem, "CSTID"));
                parameters[5] = cboMountPstsID.SelectedValue;
                parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                parameters[7] = Util.NVC(DataTableConverter.GetValue(dgWorkTarget.Rows[idx].DataItem, "PRODID")); ;
                parameters[8] = Util.NVC(DataTableConverter.GetValue(dgWorkTarget.Rows[idx].DataItem, "WIPQTY")).Equals("0") ? "0" : double.Parse(Util.NVC(DataTableConverter.GetValue(dgWorkTarget.Rows[idx].DataItem, "WIPQTY"))).ToString();
                parameters[9] = rdoRewinding != null && rdoRewinding.IsChecked == true ? "W" : "D";

                C1WindowExtension.SetParameters(popRunstart, parameters);
                popRunstart.Closed += new EventHandler(popRunStart_Closed);
                Dispatcher.BeginInvoke(new Action(() => popRunstart.ShowModal()));
            }
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });

        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunComplete()) return;

            ASSY004_008_EQPTEND popEquipmentEnd = new ASSY004_008_EQPTEND();
            popEquipmentEnd.FrameOperation = FrameOperation;

            if (popEquipmentEnd != null)
            {

                int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                object[] parameters = new object[10];
                parameters[0] = Process.VD_LMN;
                parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[2] = cboEquipment.SelectedValue.ToString();
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                parameters[7] = cboMountPstsID != null && cboMountPstsID.SelectedValue != null ? cboMountPstsID.SelectedValue.ToString() : "";
                parameters[8] = CheckProdQtyChgPermission();
                parameters[9] = string.IsNullOrEmpty(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WRK_TYPE").GetString()) ? null : DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WRK_TYPE").GetString().Substring(2,1).ToUpper(); 

                C1WindowExtension.SetParameters(popEquipmentEnd, parameters);
                popEquipmentEnd.Closed += new EventHandler(popEquipmentEnd_Closed);

                Dispatcher.BeginInvoke(new Action(() => popEquipmentEnd.ShowModal()));
            }
        }

        private void popEquipmentEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_008_EQPTEND window = sender as ASSY004_008_EQPTEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }


        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCompleteCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunCompleteCancel();
                }
            });
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirm()) return;

            Util.MessageConfirm("SFU1706", (result) =>
            {

                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }

            });
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrintPopup())
                return;

            //인쇄할 항목이 없는 경우 발행 팝업 출력.
            ASSY004_001_HIST wndPrint = new ASSY004_001_HIST();
            wndPrint.FrameOperation = FrameOperation;

            if (wndPrint != null)
            {
                object[] parameters = new object[4];
                parameters[0] = Process.VD_LMN;
                parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[2] = cboEquipment.SelectedValue.ToString();
                //_UNLDR코드를 wndPrint로 보낸다.
                parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndPrint, parameters);

                wndPrint.Closed += new EventHandler(wndPrint_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
            }
        }

        private void btnManualWaitMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualWaitMove()) return;


            ASSY004_008_MANUAL_WAIT popManualWait = new ASSY004_008_MANUAL_WAIT();
            popManualWait.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = Process.VD_LMN;
            parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
            parameters[2] = cboEquipment.SelectedValue.ToString();

            //_UNLDR코드를 wndPrint로 보낸다.
            parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;
            C1WindowExtension.SetParameters(popManualWait, parameters);

            popManualWait.Closed += new EventHandler(popManualWait_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popManualWait.ShowModal()));

        }

        private void popManualWait_Closed(object sender, EventArgs e)
        {
            ASSY004_008_MANUAL_WAIT popup = sender as ASSY004_008_MANUAL_WAIT;
            if (popup.DialogResult == MessageBoxResult.OK)
            {

            }

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //GetTestMode();
                //GetPilotProdMode();

                SetEquipmentMountPosition(cboMountPstsID);
                GetEquipmentWorkMode();
                ClearControls();

                txtWorker.Text = string.Empty;
                txtWorker.Tag = string.Empty;
                txtShift.Text = string.Empty;
                txtShift.Tag = string.Empty;
                txtShiftStartTime.Text = string.Empty;
                txtShiftEndTime.Text = string.Empty;
                txtShiftDateTime.Text = string.Empty;
                txtLossCnt.Text = string.Empty;
                txtLossCnt.Background = new SolidColorBrush(Colors.WhiteSmoke);

                // 설비 선택 시 자동 조회 처리
                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        ShowLoadingIndicator();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btnSearch_Click(null, null);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY004_COM_WAITLOT wndWait = new ASSY004_COM_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] parameters = new object[4];
                parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[1] = cboEquipment.SelectedValue.ToString();
                parameters[2] = Process.VD_LMN;
                parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndWait, parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
            }
        }

        private void btnRework_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return;
            }

            ASSY004_002_REWORK wndRwk = new ASSY004_002_REWORK();
            wndRwk.FrameOperation = FrameOperation;

            if (wndRwk != null)
            {
                object[] parameters = new object[4];
                parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[1] = cboEquipment.SelectedValue.ToString();
                parameters[2] = Process.VD_LMN;
                parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndRwk, parameters);

                wndRwk.Closed += new EventHandler(wndRwk_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRwk.ShowModal()));
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            SetEquipmentCombo(cboEquipment);
            
            //2019.02.27 오화백 RF_ID 투입부, 배출부 여부 체크
            GetLotIdentBasCode();

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") && _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                //dgDetail.Columns["CSTID"].Visibility = Visibility.Visible;
                //dgProductLot.Columns["UW_CSTID"].Visibility = Visibility.Visible;
                //dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                //dgDetail.Columns["CSTID"].Visibility = Visibility.Collapsed;
                //dgProductLot.Columns["UW_CSTID"].Visibility = Visibility.Collapsed;
                //dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
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
                    SetDefect();
                }
            });
        }

        private void rdoRewinding_Checked(object sender, RoutedEventArgs e)
        {

            GetWorkTarget();
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals("")) return;

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    // 선택할 수 없습니다.
                    Util.MessageValidation("SFU1669");
                    //e.Handled = false;
                    return;
                }
            }));
        }

        private void dgWorkTargetChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgWorkTarget.SelectedIndex = idx;

            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
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

            C1DataGrid dataGrid = sender as C1DataGrid;

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

        private void btnDfctCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx < 0) return;

                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG wndDfctCellReg = new CMM_ASSY_DFCT_CELL_REG();
                wndDfctCellReg.FrameOperation = FrameOperation;

                if (wndDfctCellReg != null)
                {
                    object[] parameters = new object[7];

                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                    parameters[2] = cboEquipmentSegment.SelectedValue;
                    parameters[3] = cboEquipment.SelectedValue;
                    parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNCODE"));
                    parameters[5] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTID"));
                    parameters[6] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNNAME"));

                    C1WindowExtension.SetParameters(wndDfctCellReg, parameters);

                    wndDfctCellReg.Closed += new EventHandler(wndDfctCellReg_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndDfctCellReg.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 라미이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLamiMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualWaitMove()) return;


            ASSY004_008_LAMI_MOVE popLamiMove = new ASSY004_008_LAMI_MOVE();
            popLamiMove.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = LoginInfo.CFG_AREA_ID;

            C1WindowExtension.SetParameters(popLamiMove, parameters);

            popLamiMove.Closed += new EventHandler(popLamiMove_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popLamiMove.ShowModal()));
        }

        #endregion

        #region [작업대상]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgProductLot.SelectedIndex = idx;

                // 상세 정보 조회
                ProdListClickedProcess(idx);
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell.Presenter == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    /*
                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("X"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2424"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #endregion

        #region [실적확인]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[8];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                parameters[3] = Process.VD_LMN;
                parameters[4] = Util.NVC(txtShift.Tag);
                parameters[5] = Util.NVC(txtWorker.Tag);
                parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        #endregion

        #region [특이사항]
        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            //저장하시겠습니까
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemark();
                }
            });
        }
        
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true)
        {
            try
            {
                if (!ValidationSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                _currentLotInfo.resetCurrentLotInfo();

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.VD_LMN;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["WIPSTAT"] = "PROC,EQPT_END";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_VD_L", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HideLoadingIndicator();

                    try
                    {
                        if (searchException != null)
                        {
                            HideLoadingIndicator();
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);
                                
                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);                                
                            }
                        }
                        else
                        {
                            // 임시 주석.
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)  // 진행중인 경우에만 자동 체크 처리.
                                {

                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);
                                    
                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
                );

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetDetailData()
        {

            const string bizRuleName = "DA_PRD_SEL_LOT_DETL_INFO_VD_PANCAKE_L";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.VD_LMN;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDetail, searchResult, FrameOperation, false);

                        if (CommonVerify.HasTableRow(searchResult))
                        {
                            txtInCarrierId.Text = searchResult.Rows[0]["IN_CSTID"].GetString();
                            txtOutCarrierId.Text = searchResult.Rows[0]["OUT_CSTID"].GetString();
                            txtInputTabCount.Text = searchResult.Rows[0]["INPUT_TAB_COUNT"].GetInt().GetString();
                            txtEndTabCount.Text = searchResult.Rows[0]["END_TAB_COUNT"].GetInt().GetString();
                            txtElectrodeType.Text = searchResult.Rows[0]["ELEC_TYPE"].GetString();
                            txtProjectName.Text = searchResult.Rows[0]["PRJT_NAME"].GetString();
                            txtModelId.Text = searchResult.Rows[0]["MODLID"].GetString();
                            txtProductId.Text = searchResult.Rows[0]["PRODID"].GetString();
                            txtWipHold.Text = searchResult.Rows[0]["WIPHOLD"].GetString();
                            txtQaHold.Text = searchResult.Rows[0]["QMS_HOLD_FLAG"].GetString();
                            txtValidTime.Text = searchResult.Rows[0]["VLD_DATE"].GetString();
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

        private void GetDefectInfo(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                ShowLoadingIndicator();

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
                newRow["PROCID"] = Process.VD_LMN;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, false);
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
                Util.MessageException(ex);

                HideLoadingIndicator();
            }

        }

        private void SetDefect()
        {
            try
            {
                dgDefect.EndEdit();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx < 0) return;

                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
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
                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
            }
        }

        private void GetRemark(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                ShowLoadingIndicator();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

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
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetRemark()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_UPD_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                newRow["WIP_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("저장 되었습니다.");
                        Util.MessageInfo("SFU1270");
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
        
        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.VD_LMN;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness            
                newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE

                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "VD PANCAKE";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
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

        private void AutoPrint(bool bQAFlag = false)
        {
            try
            {
                ShowLoadingIndicator();
                // ZPL 파일 저장 여부 
                string _EQPT_CELL_PRINT_FLAG = string.Empty;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PRINT_YN();

                    DataRow newRow = inTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

                    inTable.Rows.Add(newRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_PRT_CHK", "INDATA", "OUTDATA", inTable);


                    //  파일 저장 분기 처리                    
                    DataTable inTable2 = new DataTable();
                    inTable2.Columns.Add("EQPTID", typeof(string));

                    DataRow newRow2 = inTable2.NewRow();
                    newRow2["EQPTID"] = cboEquipment.SelectedValue;

                    inTable2.Rows.Add(newRow2);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_CELL_ID_PRT_FLAG", "INDATA", "OUTDATA", inTable2);

                    if (dtResult?.Rows?.Count > 0)
                        _EQPT_CELL_PRINT_FLAG = dtResult.Rows[0]["CELL_ID_PRT_FLAG"].ToString();


                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {

                        if (!Util.NVC(dtRslt.Rows[0]["PROC_LABEL_PRT_FLAG"]).Equals("Y"))
                        {
                            // 프린터 정보 조회
                            string sPrt = string.Empty;
                            string sRes = string.Empty;
                            string sCopy = string.Empty;
                            string sXpos = string.Empty;
                            string sYpos = string.Empty;
                            string sDark = string.Empty;
                            string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                            string sEqpt = string.Empty;
                            DataRow drPrtInfo = null;

                            // 2017-07-04 Lee. D. R
                            // Line별 라벨 독립 발행 기능
                            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                                return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                            {
                                if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                    return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                            {
                                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                                {
                                    if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(cboEquipment.SelectedValue.ToString()))
                                    {
                                        sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                        sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                        sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                        sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                        sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                        sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                        sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                        drPrtInfo = dr;
                                    }
                                }

                                if (sEqpt.Equals(""))
                                {
                                    Util.MessageValidation("SFU3615"); //프린터 환경설정에 설비 정보를 확인하세요.
                                    return;
                                }
                            }

                            if (bQAFlag)
                                sCopy = "2";

                            string zplText = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                            string lotId = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));


                            if (string.IsNullOrEmpty(zplText) )
                            {
                                Util.MessageValidation("SFU1498");
                                return;
                            }

                            if (_EQPT_CELL_PRINT_FLAG.Equals("Y"))
                            {
                                Util.SendZplBarcode(lotId, zplText);
                            }
                            else
                            {


                                if (zplText.StartsWith("0,"))  // ZPL 정상 코드 확인.
                                {
                                    if (PrintLabel(zplText.Substring(2), drPrtInfo))
                                        SetLabelPrtHist(zplText.Substring(2), drPrtInfo, Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sLBCD);
                                }
                                else
                                {
                                    Util.Alert(zplText.Substring(2));
                                }
                            }
                        }
                    }
                }
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

        private void GetWrokCalander()
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.VD_LMN;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                    txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                    txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                    txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                    txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                    txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                    txtShiftDateTime.Text = Util.NVC(result.Rows[0]["WRK_DTTM_FR_TO"]);

                    #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                    if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
                        btnShift.Visibility = Visibility.Collapsed;
                    #endregion
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;

                    #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                    if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
                    {
                        btnShift.Visibility = Visibility.Visible;
                        GetEqptWrkInfo();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        HideTestMode();
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

        private void CancelRun()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_VD_R2R_L";

                ShowLoadingIndicator();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HideLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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

        private void EqpEnd()
        {
            try
            {
                ShowLoadingIndicator();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0) return;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_OUTPUT");
                inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_END_PSTN_ID", typeof(string));

                #region IN_EQP
                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.GetCondition(cboEquipment);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                #endregion

                #region IN_INPUT
                string sMountPstnID = GetMountPstnID(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID")));
                
                inTable = indataSet.Tables["IN_INPUT"];
                newRow = inTable.NewRow();             
                if (!sMountPstnID.Equals(""))
                {
                    newRow["EQPT_MOUNT_PSTN_ID"] = sMountPstnID;
                }
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "UW_CSTID"));

                inTable.Rows.Add(newRow);
                #endregion

                #region IN_OUTPUT
                inTable = indataSet.Tables["IN_OUTPUT"];
                newRow = inTable.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["EQPT_END_PSTN_ID"] = null;

                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    newRow["OUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "CSTID"));

                inTable.Rows.Add(newRow);
                #endregion

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_VD_R2R_L", "IN_EQP,IN_INPUT,IN_OUTPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunCompleteCancel()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT";

                ShowLoadingIndicator();

                int iRow = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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

        private void ConfirmProcess()
        {
            try
            {
                ASSY004_008_CONFIRM popConfirm = new ASSY004_008_CONFIRM();

                popConfirm.FrameOperation = FrameOperation;

                if (popConfirm != null)
                {
                    int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx < 0) return;

                    object[] Parameters = new object[14];
                    Parameters[0] = Process.VD_LMN;
                    Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[2] = cboEquipment.SelectedValue.ToString();
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                    Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                    Parameters[7] = cboMountPstsID != null && cboMountPstsID.SelectedValue != null ? cboMountPstsID.SelectedValue.ToString() : "";
                    Parameters[8] = Util.NVC(txtShift.Text);
                    Parameters[9] = Util.NVC(txtShift.Tag);
                    Parameters[10] = Util.NVC(txtWorker.Text);
                    Parameters[11] = Util.NVC(txtWorker.Tag);
                    Parameters[12] = CheckProdQtyChgPermission();
                    Parameters[13] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "QA_INSP_TRGT_FLAG"));

                    C1WindowExtension.SetParameters(popConfirm, Parameters);

                    popConfirm.Closed -= new EventHandler(popConfirm_Closed);
                    popConfirm.Closed += new EventHandler(popConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popConfirm_Closed(object sender, EventArgs e)
        {
            ASSY004_008_CONFIRM window = sender as ASSY004_008_CONFIRM;

            try
            {
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    // 발행 여부 체크 및 미발행 시 자동 발행 처리
                    AutoPrint(window.CHECKED);

                    GetWorkTarget();
                    GetProductLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.VD_LMN;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if(CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
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

        private string GetMountPstnID(string sInputLot)
        {
            try
            {
                string sRet = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = Util.GetCondition(cboEquipment);
                newRow["INPUT_LOTID"] = sInputLot;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_INPUT_LOTID_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["EQPT_MOUNT_PSTN_ID"]);
                }

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.VD_LMN;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetEquipmentWorkMode()
        {
            const string bizRuleName = "DA_BAS_SEL_EIOATTR_EQPT_WRK_MODE";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["EQPTID"] = cboEquipment.SelectedValue;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (CommonVerify.HasTableRow(searchResult))
            {
                if (searchResult.Rows[0]["EQPT_WRK_MODE"].GetString() == "W")
                {
                    TextBlockWorkMode.Text = ObjectDic.Instance.GetObjectName("Rewinding");
                }
                else
                {
                    TextBlockWorkMode.Text = string.Empty;
                }
            }
            else
            {
                TextBlockWorkMode.Text = string.Empty;
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private static void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FOR_VD";

            string[] arrColumn = { "LANGID", "PROCID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, Process.VD_LMN, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_FOR_VD";

            string[] arrColumn = { "LANGID", "PROCID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, Process.VD_LMN, cboEquipmentSegment.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetEquipmentMountPosition(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_L";

            string[] arrColumn = { "LANGID", "EQPTID", "MOUNT_MTRL_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipment.SelectedValue.GetString(), "PROD" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private bool CheckProdQtyChgPermission()
        {
            try
            {
                bool bRet = false;

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
                                case "PROD_QTY_CHG_W": // 수량 변경 권한
                                    bRet = true;
                                    break;
                                default:
                                    break;
                            }
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
        }

        #endregion

        #region [Validation]

        private bool ValidationSearch()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }
            
            return true;
        }

        private bool ValidationRunComplete()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            string wipState = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK", true).Field<string>("WIPSTAT").GetString();

            if (!wipState.Equals("PROC"))
            {
                Util.MessageValidation("SFU1866");
                return false;
            }

             //장비완료 시 체크로직 여부 확인 후 반영예정
            
            if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
            {
                //WorkCalander 체크.
                GetWrokCalander();

                if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
                {
                    Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                    return false;
                }

                if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
                {
                    DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                    DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                    DateTime systemDateTime = GetSystemTime();

                    int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                    int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                    if (prevCheck < 0 || nextCheck > 0)
                    {
                        Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                        return false;
                    }
                }

                return true;
            }

            return true;
        }

        private bool ValidationPrintPopup()
        {
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationCompleteCancelRun()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                return false;
            }
            return true;
        }

        private bool ValidationConfirm()
        {
            if(string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                //Util.Alert("작업조를 선택 하세요.");
                //Util.MessageValidation("SFU1844");
                //return false;
            }

            //if (txtWorker.Text.Trim().Equals(""))
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                //Util.Alert("작업자를 선택 하세요.");
                //Util.MessageValidation("SFU1842");
                //return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택 된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return false;

            }

            if(!_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK",true).Field<string>("WIPSTAT").Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
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

            //if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            //{
            //    DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
            //    DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
            //    DateTime systemDateTime = GetSystemTime();

            //    int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
            //    int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

            //    if (prevCheck < 0 || nextCheck > 0)
            //    {
            //        Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
            //        txtWorker.Text = string.Empty;
            //        txtWorker.Tag = string.Empty;
            //        txtShift.Text = string.Empty;
            //        txtShift.Tag = string.Empty;
            //        txtShiftStartTime.Text = string.Empty;
            //        txtShiftEndTime.Text = string.Empty;
            //        txtShiftDateTime.Text = string.Empty;
            //        return false;
            //    }
            //}

            return true;

        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationStartRun()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            int idx = _util.GetDataGridFirstRowIndexByCheck(dgWorkTarget, "CHK", true);
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            /*
            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                Util.MessageValidation("SFU1543");
                return false;
            }
            */

            return true;
        }

        private bool ValidationCancelRun()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            return true;
        }

        private bool ValidationManualWaitMove()
        {
            return true;
        }
        #endregion

        #region [PopUp Event]

        private void popRunStart_Closed(object sender, EventArgs e)
        {
            ASSY004_008_RUNSTART popup = sender as ASSY004_008_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetWorkTarget();
                GetProductLot();
            }
        }

        private void wndPrint_Closed(object sender, EventArgs e)
        {
            ASSY004_001_HIST window = sender as ASSY004_001_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_WAITLOT window = sender as ASSY004_COM_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndRwk_Closed(object sender, EventArgs e)
        {
            ASSY004_002_REWORK window = sender as ASSY004_002_REWORK;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
        }

        private void wndDfctCellReg_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DFCT_CELL_REG window = sender as CMM_ASSY_DFCT_CELL_REG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                //txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                //txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                //txtWorker.Tag = Util.NVC(wndPopup.USERID);
                //txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                //txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                //txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);

                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        private void wndEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO window = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }


        /// <summary>
        /// 라미이동 화면 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popLamiMove_Closed(object sender, EventArgs e)
        {
            ASSY004_008_LAMI_MOVE window = sender as ASSY004_008_LAMI_MOVE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetWorkTarget();
                GetProductLot();
            }
        }

        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRunStart);
            listAuth.Add(btnConfirm);
            listAuth.Add(btnRunComplete);
            listAuth.Add(btnPrint);
            //listAuth.Add(btnWaitLot);
            listAuth.Add(btnDefectSave);
            listAuth.Add(btnSaveRemark);
            //listAuth.Add(btnRework);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetWorkTarget()
        {
            const string bizRuleName = "DA_PRD_SEL_WRK_TRGT_VD_L";

            try
            {
                ShowLoadingIndicator();    

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WRK_TYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WRK_TYPE"] = rdoRewinding != null && rdoRewinding.IsChecked == true ? "W" : "D";
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HideLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgWorkTarget, bizResult, null, true);
                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailData();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }
        
        private void ClearLotInfo()
        {
            sCaldate = "";

            txtInCarrierId.Text = string.Empty;
            txtOutCarrierId.Text = string.Empty;
            txtInputTabCount.Text = string.Empty;
            txtEndTabCount.Text = string.Empty;
            txtElectrodeType.Text = string.Empty;
            txtProjectName.Text = string.Empty;

            txtModelId.Text = string.Empty;
            txtProductId.Text = string.Empty;
            txtWipHold.Text = string.Empty;
            txtQaHold.Text = string.Empty;
            txtValidTime.Text = string.Empty;
            //txtEndTime.Text = string.Empty;
        }

        private void ClearDetailData()
        {
            Util.gridClear(dgDetail);
            Util.gridClear(dgDefect);

            ClearLotInfo();

            rtxRemark.Document.Blocks.Clear();            
        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            ClearDetailData();

            if (!_util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);
        }

        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
            {
                //Util.Alert("LOT 상세정보가 잘못 되어 있습니다.");
                Util.MessageValidation("SFU1215");
                return;
            }

            _currentLotInfo.LOTID = Util.NVC(rowview["LOTID"]);
            _currentLotInfo.INPUTQTY = Util.NVC(rowview["WIPQTY"]);
            //_currentLotInfo.WORKORDER = Util.NVC(rowview["WOID"]); // 장비완료 process 추가로 수정.
            _currentLotInfo.STATUS = Util.NVC(rowview["WIPSTAT"]);
            //_currentLotInfo.STATUSNAME = Util.NVC(rowview["WIPSNAME"]);
            //_currentLotInfo.PRODID = Util.NVC(rowview["PRODID"]);
            //_currentLotInfo.MODELID = Util.NVC(rowview["MODLID"]);


            if (Util.NVC(rowview["CALDATE_LOT"]).Trim().Equals(""))
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));

                sCaldate = Util.NVC(rowview["NOW_CALDATE_YMD"]);
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));
            }
            else
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));

                sCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToString("yyyyMMdd");
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));
            }

            if (!_currentLotInfo.STATUS.Equals("WAIT"))
            {
                FillLotInfo();
                GetDetailData();
                GetDefectInfo(rowview);
            }

            GetRemark(rowview);
        }

        private void FillLotInfo()
        {
            /******************** 상세 정보 Set... ******************/
            //txtWorkorder.Text = _currentLotInfo.WORKORDER;
            //txtLotStatus.Text = _currentLotInfo.STATUSNAME;

            /*
            txtElectrodeType.Text = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("ELEC_TYPE").GetString();
            txtModelId.Text = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("MODLID").GetString();

            txtStartTime.Text = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<DateTime>("WIPDTTM_ST").GetString();   //Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "STARTTIME"));
            txtEndTime.Text = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<DateTime>("WIPDTTM_ED").GetString();     //Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "ENDTIME"));
            */


            /*
            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "STARTTIME")).Equals("") && !Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "ENDTIME")).Equals(""))
            {
                DateTime dTmpEnd;
                DateTime dTmpStart;

                //if (DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "ENDTIME")), out dTmpEnd) && DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "STARTTIME")), out dTmpStart))
                //    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString();
            }
            */
        }
        
        private bool PrintLabel(string zplText, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zplText);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(zplText, cboEquipment.SelectedValue.ToString());
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zplText);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zplText);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rec)
        {
            rec.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInredRectangle(recTestMode);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void GetLossCnt()
        {
            try
            {
                DataTable dtEqpLossCnt = Util.Get_EqpLossCnt(cboEquipment.SelectedValue.ToString());

                if (dtEqpLossCnt != null && dtEqpLossCnt.Rows.Count > 0)
                {
                    txtLossCnt.Text = Util.NVC(dtEqpLossCnt.Rows[0]["CNT"]);

                    if (Util.NVC_Int(dtEqpLossCnt.Rows[0]["CNT"]) > 0)
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.LightPink);
                    }
                    else
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.VD_LMN;
                sEqsg = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }

        }
        #endregion

        #endregion

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPilotProdMode()) return;

            string sMsg = string.Empty;
            bool bMode = GetPilotProdMode();

            if (bMode)
                sMsg = "SFU2875";
            else
                sMsg = "SFU2875";

            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetPilotProdMode(!bMode);
                    GetPilotProdMode();

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
            });
        }

        private bool CanPilotProdMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //string sProcLotID = string.Empty;
            //if (CheckProcWip(out sProcLotID))
            //{
            //    Util.MessageValidation("SFU3199", sProcLotID); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CheckProcWip(out string sProcLotID)
        {
            sProcLotID = "";

            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dtRow["WIPSTAT"] = Wip_State.PROC;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sProcLotID = Util.NVC(dtRslt.Rows[0]["LOTID"]);
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

        private bool GetPilotProdMode()
        {
            try
            {
                bool bRet = false;

                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || Util.NVC(cboEquipment.SelectedValue).Trim().Equals("SELECT"))
                {
                    HidePilotProdMode();
                    return bRet;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["PILOT_PROD_MODE"]).Equals("Y"))
                    {
                        ShowPilotProdMode();
                        bRet = true;
                    }
                    else
                    {
                        HidePilotProdMode();
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SetPilotProdMode(bool bMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PILOT_PRDC_MODE"] = bMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
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

        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void ShowPilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
