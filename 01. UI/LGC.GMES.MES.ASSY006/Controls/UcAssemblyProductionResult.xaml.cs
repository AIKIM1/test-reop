/*************************************************************************************
 Created Date : 2021.12.16
      Creator : 신광희
   Decription : 소형 조립 공정진척(NFF) - 생산 실적  UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.16  신광희 : Initial Created.
 2022.05.03  배현우 : Tray 기준정보를 가져오는 GetTrayQty 함수 추가 및 Tray 등록시 함수 참조
 2022.11.03  배현우 : ZZS OutLot 발행시 체크하지않은 box도 발행되는 부분 수정
 2023.10.25  김용군 : 오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
 2023.12.05  배현우 : CELL 관리 팝업 파라미터 추가
 2024.03.15  남기운 : (NERP 대응 프로젝트)실적처리 허용비율 초과 입력 기능추가
 2024.05.02  백광영 : CCW 설비 재작업모드 조회 추가 (설비 재작업모드일 경우 Tray정보 호출 버튼 Visible, Tray정보 팝업 호출)
 2024.07.22  백광영 : CCW 설비 재작업모드 조회 삭제, 재작업 Tray 조회 Tab 추가
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using System.Linq;
using System.Reflection;
using LGC.GMES.MES.ASSY003;
 
namespace LGC.GMES.MES.ASSY006.Controls
{
    /// <summary>
    /// UcAssemblyProductionResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyProductionResult : UserControl
    {
        #region Declaration & Constructor
        
        public IFrameOperation FrameOperation{ get; set; }

        public UserControl UcParentControl;

        public DataTable DtEquipment { get; set; }
        public DataRowView DvProductLot { get; set; }
        public string EquipmentSegmentCode { get; set; }
        public string EquipmentSegmentName { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string ShiftId { get; set; }
        public string WorkerId { get; set; }
        public string WorkerName { get; set; }
        
        public bool IsProductLotRefreshFlag = false;

        public bool IsEquipmentTreeRefreshFlag = false;

        public bool IsSelectedAll = false;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private DataTable _dtCellManagement;

        private bool _isWindingSetAutoTime = false;
        private bool _isAutoSelectTime = false;

        private int _rowIndexCheck = 0;
        private Int32 _trayCheckSeq;

        private string _reLoadFlag = string.Empty;
        private string _equipmentConfigType = string.Empty;
        private string _cellManagementTypeCode;
        private string _cellManageGroup = string.Empty;

        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerOutWinding = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerOutWashing = new System.Windows.Threading.DispatcherTimer();

        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));
        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);

        private bool isMesAdmin = false;
        private DateTime _Min_Valid_Date;
        private string _Max_Pre_Proc_End_Day = string.Empty;
        private bool useInputCancel = false;
        private bool bCellTraceMode = false;

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
        CurrentLotInfo _CurrentLot = new CurrentLotInfo();
        string _Lane = string.Empty;
        bool _isChangeRemark = false;
        string _ProcWipnoteGroupUseFlag = string.Empty;
        string _HoldReasonValue = string.Empty;
        bool _isDupplicatePopup = false;
        bool _isChangeInputFocus = false;
        bool _isChangeWipReason = false;
        private decimal inputQtyOrg = 0;

        public bool bChangeQuality
        {
            get { return _isChangeQuality; }
        }

        bool _isChangeQuality = false;

        ASSY003_007_TRAY_MOVE wndTrayMove;

        private struct PreviewValues
        {
            public string PreviewTray;
            public string PreviewCurentInput;
            private string _previewOutBox;

            public PreviewValues(string tray, string currentInput, string box)
            {
                PreviewTray = tray;
                PreviewCurentInput = currentInput;
                this._previewOutBox = box;
            }
        }

        private PreviewValues _previewValues = new PreviewValues("", "", "");

        private struct PRV_VALUES
        {
            public string sPrvTray;

            public PRV_VALUES(string sTray)
            {
                sPrvTray = sTray;
            }
        }
        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("");

        private struct PreviousValues
        {
            public string PreviousTray;

            public PreviousValues(string tray)
            {
                PreviousTray = tray;
            }
        }
        private PreviousValues _previousValues = new PreviousValues("");

        public UcAssemblyProductionResult()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            SetIsMesAdmin();
            //SetControlVisibility();

        }

        #endregion

        #region Initialize

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeCombo();

            if (_dispatcherTimerOutWinding != null)
            {
                int second = 0;

                if (cboAutoSearchOutWinding?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearchOutWinding.SelectedValue.GetString()))
                    second = int.Parse(cboAutoSearchOutWinding.SelectedValue.ToString());

                _dispatcherTimerOutWinding.Tick += dispatcherTimerOutWinding_Tick;
                _dispatcherTimerOutWinding.Interval = new TimeSpan(0, 0, second);
            }

            if (_dispatcherTimerOutWashing != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut?.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                _dispatcherTimerOutWashing.Tick += dispatcherTimerOutWashing_Tick;
                _dispatcherTimerOutWashing.Interval = new TimeSpan(0, 0, iSec);
            }

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InitializeControls()
        {
            RegisterName("greenBrush", greenBrush);
            RegisterName("myAnimatedBrush", myAnimatedBrush);

            if (_dispatcherTimerOutWinding != null)
            {
                _dispatcherTimerOutWinding.Stop();
                cboAutoSearchOutWinding.SelectedValueChanged -= cboAutoSearchOutWinding_SelectedValueChanged;
                cboAutoSearchOutWinding.SelectedIndex = 0;
                cboAutoSearchOutWinding.SelectedValueChanged += cboAutoSearchOutWinding_SelectedValueChanged;
            }

            if (_dispatcherTimerOutWashing != null)
            {
                _dispatcherTimerOutWashing.Stop();
                cboAutoSearchOut.SelectedValueChanged -= cboAutoSearchOut_SelectedValueChanged;
                cboAutoSearchOut.SelectedIndex = 0;
                cboAutoSearchOut.SelectedValueChanged += cboAutoSearchOut_SelectedValueChanged;
            }
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "MOBILE_TRAY_INTERVAL" };
            combo.SetCombo(cboAutoSearchOutWinding, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");
            if (cboAutoSearchOutWinding?.Items != null && cboAutoSearchOutWinding.Items.Count > 0)
                cboAutoSearchOutWinding.SelectedIndex = 0;

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "MOBILE_TRAY_INTERVAL" };
            combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            if (cboAutoSearchOut?.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

            // 특별 TRAY  사유 Combo
            String[] sFilter3 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SpecialResonCodebyAreaCode");

            if (cboOutTraySplReason?.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;

            // 특별 TRAY  사유 Combo - ZZS
            String[] sFilter5 = { "SPCL_RSNCODE" };
            combo.SetCombo(cboOutLotSplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter5, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboOutLotSplReason != null && cboOutLotSplReason.Items != null && cboOutLotSplReason.Items.Count > 0)
                cboOutLotSplReason.SelectedIndex = 0;

            // 특별 TRAY  사유 Combo - PKG
            String[] sFilter6 = { "SPCL_RSNCODE" };
            combo.SetCombo(cboOutTraySplReasonPKG, CommonCombo.ComboStatus.SELECT, sFilter: sFilter5, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboOutTraySplReasonPKG != null && cboOutTraySplReasonPKG.Items != null && cboOutTraySplReasonPKG.Items.Count > 0)
                cboOutTraySplReasonPKG.SelectedIndex = 0;
        }

        private void SetButtons()
        {
            //ButtonProductionUpdate = btnProductionUpdate;
        }

        private void SetControl()
        {
            SetEquipmentDisplay();

            SetDataCollectColumnDisplay();

            SetDataGridColumn();

            _trayCheckSeq = 0;
            rdoLot.IsChecked = true;
        }

        private void SetIsMesAdmin()
        {
            ShowLoadingIndicator();

            DataTable dt = new DataTable();
            dt.Columns.Add("USERID");
            dt.Columns.Add("AUTHID");

            DataRow dr = dt.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["AUTHID"] = "MESADMIN";
            dt.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_BY_AUTH", "INDATA", "OUTDATA", dt, (result, exception) =>
            {
                try
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        isMesAdmin = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            });
        }

        private void SetEquipmentDisplay()
        {
            recEquipment.Fill = greenBrush;
            txtEquipment.Text = "[" + EquipmentCode + "] " + EquipmentName;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "greenBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void SetDataCollectColumnDisplay()
        {
            if(string.Equals(ProcessCode, Process.WASHING))
            {
                grdDataCollect.ColumnDefinitions[0].Width = new GridLength(0.35, GridUnitType.Star);
                grdDataCollect.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                grdDataCollect.ColumnDefinitions[2].Width = new GridLength(0.65, GridUnitType.Star);
            }
            else
            {
                grdDataCollect.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                grdDataCollect.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                grdDataCollect.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private void SetDataGridColumn()
        {
            dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["DEFECTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["REINPUTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Collapsed;

            tbPatternResult.Visibility = Visibility.Collapsed;
            txtPatternResultQty.Visibility = Visibility.Collapsed;

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            btnProductionUpdate.Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["EQPTQTY"].Header = "설비투입수량";

            if (string.Equals(ProcessCode, Process.WINDING))
            {
                dgProdCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;
                dgProdCellWinding.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;

                dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["RSLT_EXCL_FLAG"].Visibility = Visibility.Visible;
                dgDefect.Columns["INPUT_QTY_APPLY_TYPE_CODE_NAME"].Visibility = Visibility.Visible;

                //패턴초과 로직 추가 2020-09-01
                tbPatternResult.Visibility = Visibility.Visible;
                txtPatternResultQty.Visibility = Visibility.Visible;
                tbPatternResult.Text = ObjectDic.Instance.GetObjectName("패턴초과");

                tbAssyResult.Text = ObjectDic.Instance.GetObjectName("추가불량");
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["EQPTQTY_M_EA"].Visibility = Visibility.Visible;
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                tbAssyResult.Visibility = Visibility.Visible;
                txtAssyResultQty.Visibility = Visibility.Visible;
                tbAssyResult.Text = ObjectDic.Instance.GetObjectName("차이수량");
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY_M_EA"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["REINPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;
            }

            else if (string.Equals(ProcessCode, Process.WASHING))
            {
                dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                tbAssyResult.Visibility = Visibility.Collapsed;
                txtAssyResultQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;

                dgDefectDetail.Columns["EQPTQTY_M_EA"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;

                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
            }
            else if ((string.Equals(ProcessCode, Process.ZZS)) || (string.Equals(ProcessCode, Process.PACKAGING)))
            {
                btnSaveWipHistory.Visibility = Visibility.Collapsed;
                tbAssyResult.Visibility = Visibility.Collapsed;
                txtAssyResultQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;

                dgDefectDetail.Columns["EQPTQTY_M_EA"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_DEFECT"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_LOSS"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_CHARGEPRD"].Visibility = Visibility.Visible;

            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if ((string.Equals(ProcessCode, Process.ZTZ)))
            {
                btnSaveWipHistory.Visibility = Visibility.Collapsed;
                btnProductionUpdate.Visibility = Visibility.Visible;
                tbAssyResult.Visibility = Visibility.Collapsed;
                txtAssyResultQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;

                dgDefectDetail.Columns["EQPTQTY_M_EA"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["REINPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DEFECTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_DEFECT"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_LOSS"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["DTL_CHARGEPRD"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["EQPTQTY"].Header = "장비수량";
            }

            dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;
        }

        public void SetControlClear()
        {

            textLotId.Text = string.Empty;
            textWorkTime.Text = string.Empty;
            textProdId.Text = string.Empty;
            textProjectName.Text = string.Empty;
            textInputQty.Value = double.NaN;
            textGoodQty.Value = double.NaN;
            txtTrayIDInline.Text = string.Empty;
            txtProdVerCode.Text = string.Empty;

            txtTrayID.Text = string.Empty;
            _trayCheckSeq = 0;
            _cellManagementTypeCode = string.Empty;
            tbCellManagement.Text = string.Empty;
            //특별관리 화면 display 초기화 시작
            chkOutTraySpl.IsChecked = false;
            txtOutTrayReamrk.Text = string.Empty;
            txtRegPerson.Text = string.Empty;

            txtAssyResultQty.Text = string.Empty;
            txtPatternResultQty.Text = string.Empty;
            txtWipNote.Text = string.Empty;
            BoxQty.Value = 0;

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            txtWipstat.Text = string.Empty;
            txtUnit.Text = string.Empty;
            textInputQtyZtz.Value = double.NaN;
            txtParentQty.Value = 0;
            txtRemainQty.Value = 0;

            _isDupplicatePopup = false;
            _isChangeQuality = false;
            _isChangeInputFocus = false;
            _isChangeWipReason = false;

            if (cboOutTraySplReason.Items.Count > 0)
            {
                cboOutTraySplReason.SelectedIndex = 0;
            }

            if ((bool)chkOutTraySpl.IsChecked)
            {
                grdSpecialTrayMode.Visibility = Visibility.Visible;
                ColorAnimationInSpecialTray();
            }
            else
            {
                grdSpecialTrayMode.Visibility = Visibility.Collapsed;
            }
            //특별관리 화면 display 초기화 끝


            Util.gridClear(dgDefectDetail);
            // Grid DataCollect Left C1DataGrid
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgDefectReInput);
            Util.gridClear(dgBox);

            // Grid DataCollect Right C1DataGrid
            Util.gridClear(dgProdCellWinding);
            Util.gridClear(dgWashingResult);
            Util.gridClear(dgOut);

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            Util.gridClear(dgQualityZtz);
            Util.gridClear(dgRemark);
        }

        private void ClearLotInfo()
        {

        }

        private void SetBoxMountPstsIDCombo()
        {
            CommonCombo combo = new CommonCombo();
            //Packaging 자재 투입위치 코드 - 대기바구니 TAB
            String[] sFilter6 = { EquipmentCode, "PROD" };
            combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter6, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }

        private void SetControlVisibility()
        {
            SetTabVisibility();

            tbInputQty.Text = ObjectDic.Instance.GetObjectName("생산 수량");

            if (string.Equals(ProcessCode, Process.WASHING) || string.Equals(ProcessCode, Process.WINDING))
            {
                grdTrayLegend.Visibility = Visibility.Visible;

                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    grdTrayLegend.Margin = new Thickness(0, 10, 5, 0);
                    btnRefresh.Visibility = Visibility.Collapsed;
                    tbCellManagement.Visibility = Visibility.Collapsed;

                    bdTrayLegend1.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    bdTrayLegend2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    bdTrayLegend3.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                    tbTrayLegend1.Text = ObjectDic.Instance.GetObjectName("미확정");
                    tbTrayLegend2.Text = ObjectDic.Instance.GetObjectName("확정");
                    tbTrayLegend3.Text = ObjectDic.Instance.GetObjectName("배출완료");
                }
                else
                {
                    grdTrayLegend.Margin = new Thickness(0, 8, 5, 0);
                    btnRefresh.Visibility = Visibility.Visible;
                    tbCellManagement.Visibility = Visibility.Visible;

                    bdTrayLegend1.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                    bdTrayLegend2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    bdTrayLegend3.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                    tbTrayLegend1.Text = ObjectDic.Instance.GetObjectName("미확정");
                    tbTrayLegend2.Text = ObjectDic.Instance.GetObjectName("조립출고확정");
                    tbTrayLegend3.Text = ObjectDic.Instance.GetObjectName("활성화입고");
                }
            }
            else if(string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                grdTrayLegend.Visibility = Visibility.Collapsed;
                tbInputQty.Text = ObjectDic.Instance.GetObjectName("투입수량");
            }
            else if (string.Equals(ProcessCode, Process.ZZS))
            {
                grdTrayLegend.Margin = new Thickness(0, 8, 5, 0);
                grdTrayLegend.Visibility = Visibility.Visible;
                btnRefresh.Visibility = Visibility.Collapsed;
                tbCellManagement.Visibility = Visibility.Collapsed;

                bdTrayLegend1.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                bdTrayLegend2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                tbTrayLegend1.Text = ObjectDic.Instance.GetObjectName("발행"); 
                tbTrayLegend2.Text = ObjectDic.Instance.GetObjectName("미발행");

                bdTrayLegend3.Visibility = Visibility.Collapsed;
                tbTrayLegend3.Visibility = Visibility.Collapsed;
            }
            else if (string.Equals(ProcessCode, Process.PACKAGING))
            {
                grdTrayLegend.Margin = new Thickness(0, 8, 5, 0);
                grdTrayLegend.Visibility = Visibility.Visible;
                btnRefresh.Visibility = Visibility.Collapsed;
                tbCellManagement.Visibility = Visibility.Collapsed;

                bdTrayLegend1.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                bdTrayLegend2.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                bdTrayLegend3.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                tbTrayLegend1.Text = ObjectDic.Instance.GetObjectName("미확정");
                tbTrayLegend2.Text = ObjectDic.Instance.GetObjectName("조립출고확정");
                tbTrayLegend3.Text = ObjectDic.Instance.GetObjectName("활성화입고");
            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(ProcessCode, Process.ZTZ))
            {

                tbInputQtyZtz.Text = ObjectDic.Instance.GetObjectName("생산 수량");

                grdTrayLegend.Margin = new Thickness(0, 8, 5, 0);
                grdTrayLegend.Visibility = Visibility.Visible;
                btnRefresh.Visibility = Visibility.Collapsed;
                tbCellManagement.Visibility = Visibility.Collapsed;

                bdTrayLegend1.Visibility = Visibility.Collapsed;
                tbTrayLegend1.Visibility = Visibility.Collapsed;
                bdTrayLegend2.Visibility = Visibility.Collapsed;
                tbTrayLegend2.Visibility = Visibility.Collapsed;
                bdTrayLegend3.Visibility = Visibility.Collapsed;
                tbTrayLegend3.Visibility = Visibility.Collapsed;
            }
        }

        private void SetTabVisibility()
        {
            tabDefect.Visibility = Visibility.Collapsed;
            TabMaterialInput.Visibility = Visibility.Collapsed;
            tabDefectZZS.Visibility = Visibility.Collapsed;
            tabWaitBox.Visibility = Visibility.Collapsed;
            tabInBox.Visibility = Visibility.Collapsed;
            tabEquipmentFaulty.Visibility = Visibility.Collapsed;
            tabReInput.Visibility = Visibility.Collapsed;
            tabOutWindingProduct.Visibility = Visibility.Collapsed;
            tabOutZZSProduct.Visibility = Visibility.Collapsed;
            tabOutPKGProduct.Visibility = Visibility.Collapsed;
            tabWashingResult.Visibility = Visibility.Collapsed;
            tabOutWashingProduct.Visibility = Visibility.Collapsed;
            tabQualityInfo.Visibility = Visibility.Collapsed;
            tabBox.Visibility = Visibility.Collapsed;
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            tabDefectZtz.Visibility = Visibility.Collapsed;
            tiQuality.Visibility = Visibility.Collapsed;
            tiRemark.Visibility = Visibility.Collapsed;
            tabDefectZtz.Visibility = Visibility.Collapsed;
            tabCCWTray.Visibility = Visibility.Collapsed;

            if (ProcessCode == Process.WINDING)
            {
                tabDefect.Visibility = Visibility.Visible;
                tabEquipmentFaulty.Visibility = Visibility.Visible;
                tabOutWindingProduct.Visibility = Visibility.Visible;
                tabQualityInfo.Visibility = Visibility.Visible;
                TabMaterialInput.Visibility = Visibility.Visible;

            }
            else if (ProcessCode == Process.ASSEMBLY)
            {
                tabDefect.Visibility = Visibility.Visible;
                tabEquipmentFaulty.Visibility = Visibility.Visible;
                tabWashingResult.Visibility = Visibility.Visible;
                tabQualityInfo.Visibility = Visibility.Visible;
                tabCCWTray.Visibility = Visibility.Visible;

                if (GetReInputReasonApplyFlag(EquipmentCode) == "Y")
                {
                    tabReInput.Visibility = Visibility.Visible;
                }
                else
                {
                    tabReInput.Visibility = Visibility.Collapsed;
                }
            }
            else if (ProcessCode == Process.WASHING)
            {
                tabDefect.Visibility = Visibility.Visible;
                tabEquipmentFaulty.Visibility = Visibility.Visible;
                tabOutWashingProduct.Visibility = Visibility.Visible;
                tabBox.Visibility = Visibility.Visible;
            }
            else if (ProcessCode == Process.ZZS)
            {
                tabOutZZSProduct.Visibility = Visibility.Visible;
                tabDefectZZS.Visibility = Visibility.Visible;

            }
            else if (ProcessCode == Process.PACKAGING)
            {
                tabWaitBox.Visibility = Visibility.Visible;
                tabInBox.Visibility = Visibility.Visible;

                tabDefectZZS.Visibility = Visibility.Visible;
                tabOutPKGProduct.Visibility = Visibility.Visible;

            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (ProcessCode == Process.ZTZ)
            {                
                tabDefectZtz.Visibility = Visibility.Visible;
                tiQuality.Visibility = Visibility.Visible;
                tiRemark.Visibility = Visibility.Visible;
            }

        }

        private void SetDefect()
        {
            try
            {
                dgDefectZZS.EndEdit();

                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefectZZS.Rows.Count - dgDefectZZS.BottomRows.Count; i++)
                {
                    newRow = null;
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectZZS.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }
                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
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

                        GetDefectInfo(DvProductLot);
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        IsProductLotRefreshFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HiddenLoadingIndicator();
            }
        }

        #endregion




        #region Event

        private void dispatcherTimerOutWinding_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;
                    if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                    if (string.Equals(ProcessCode,Process.WINDING))
                    {
                        if (!CommonVerify.HasDataGridRow(dgProdCellWinding)) return;
                        GetProductCellList(dgProdCellWinding);
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

        private void dispatcherTimerOutWashing_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;
                    if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                    if (string.Equals(ProcessCode, Process.WASHING))
                    {
                        GetOutTraybyAsync();
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

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["REINPUTQTY"].Index)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["GOODQTY"].Index)
                    {
                        //if ((string.Equals(ProcessCode, Process.WINDING)) && !IsSmallType)
                        //{
                        //    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        //    if (convertFromString != null)
                        //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        //}
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }

                    if(string.Equals(ProcessCode, Process.WASHING))
                    {
                        if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_P"].Index)
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (e.Cell.Column.Index == dataGrid.Columns["ALPHAQTY_M"].Index)
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    if (e.Cell.Column.Name.Equals("GOODQTY"))
                    {
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double eqptqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTQTY").GetDouble();
                        double outputqty = goodqty + defectqty + lossqty + chargeprdqty;
                        double adddefectqty = 0;
                        adddefectqty = eqptqty - (goodqty + defectqty + lossqty + chargeprdqty);
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);

                        if (Math.Abs(adddefectqty) > 0)
                        {
                            txtAssyResultQty.Text = adddefectqty.ToString("##,###");
                            txtAssyResultQty.FontWeight = FontWeights.Bold;
                            txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            txtAssyResultQty.Text = "0";
                            txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (e.Cell.Column.Name.Equals("REINPUTQTY"))
                    {
                        double inputqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUTQTY").GetDouble();
                        double reinputqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "REINPUTQTY").GetDouble();
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double differenceqty = 0;
                        double boxqty = 0;

                        boxqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXQTY").GetDouble();

                        differenceqty = (inputqty + reinputqty) - (goodqty + defectqty + lossqty + chargeprdqty + boxqty).GetDouble();

                        if (Math.Abs(differenceqty) > 0)
                        {
                            txtAssyResultQty.Text = differenceqty.ToString("##,###");
                            txtAssyResultQty.FontWeight = FontWeights.Bold;
                            txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            txtAssyResultQty.Text = "0";
                            txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
                else
                {
                    double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                    double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                    double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                    double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                    double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);
                }
            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            try
            {
                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["RESNQTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumDefectQty().GetInt();
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
        }

        private void dgDefectZZS_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            try
            {
                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["RESNQTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumDefectQty().GetInt();
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
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgDefectZZS_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            // A2000,A3000,A4000 이벤트 동일 함.
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefectZZS_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            // A2000,A3000,A4000 이벤트 동일 함.
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            // A2000(UcAssyDataCollectInline) 이벤트 없음.
            // A3000(UcAssyDataCollect) 이벤트 없음.
            // A4000(UcAssyProduction) 에는 IsChangeDefect = true;

        }
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    //row 색 바꾸기
                    dgCCWTrayList.SelectedIndex = idx;
                }
            }
        }

        private void btnDfctCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgDefectDetail.ItemsSource);

                if (!CommonVerify.HasTableRow(dtTmp)) return;

                Dictionary<string, string> dList = new Dictionary<string, string>();
                dList.Add(Util.NVC(DvProductLot["LOTID"]), Util.NVC(DvProductLot["WIPSEQ"]));

                //foreach (DataRow dr in dtTmp.Rows)
                //{
                //    dList.Add(Util.NVC(dr["LOTID"]), Util.NVC(dr["WIPSEQ"]));
                //}

                Button bt = sender as Button;

                if (bt?.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG popupDefectCellInfo = new CMM_ASSY_DFCT_CELL_REG();
                popupDefectCellInfo.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = dList;
                parameters[1] = dList;
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = EquipmentCode;
                parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNCODE"));
                parameters[5] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTID"));
                parameters[6] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNNAME"));
                C1WindowExtension.SetParameters(popupDefectCellInfo, parameters);

                popupDefectCellInfo.Closed += new EventHandler(popupDefectCellInfo_Closed);
                Dispatcher.BeginInvoke(new Action(() => popupDefectCellInfo.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDefectCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.DataContext == null) return;

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) return;
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(button.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG popDefectCell = new CMM_ASSY_DFCT_CELL_REG();
                popDefectCell.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = DvProductLot["LOTID"].GetString();
                parameters[1] = DvProductLot["WIPSEQ"].GetString();
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = EquipmentCode;
                parameters[4] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "RESNCODE"));
                parameters[5] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "ACTID"));
                parameters[6] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "RESNNAME"));
                C1WindowExtension.SetParameters(popDefectCell, parameters);

                //popDefectCell.Closed += new EventHandler(popDefectCell_Closed);
                Dispatcher.BeginInvoke(new Action(() => popDefectCell.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupDefectCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DFCT_CELL_REG window = sender as CMM_ASSY_DFCT_CELL_REG;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                //GetEqpFaultyData();
            }
        }

        private void txtWaitBoxLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitBox();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchEqpFaulty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (sender == null) return;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("EQPT_DEFECTQTY_GR_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
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
                C1DataGrid dataGrid = (sender as C1DataGrid);
                if (sender == null) return;

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;
                
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_EQPT_DFCT_CELL_INFO popupDefectCellInfo = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
                            popupDefectCellInfo.FrameOperation = FrameOperation;

                            object[] parameters = new object[3];
                            parameters[0] = DvProductLot["LOTID"].GetString();
                            parameters[1] = EquipmentCode;
                            parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID"));
                            C1WindowExtension.SetParameters(popupDefectCellInfo, parameters);

                            //popupDefectCellInfo.Closed += new EventHandler(wndEqptDfctCell_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popupDefectCellInfo.ShowModal()));

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
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") || Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
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
            catch (Exception)
            {
                //Util.MessageException(ex);
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;
            SaveDefect();
        }

        private void btnDefectSaveZZS_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefectZZS()) return;
            SetDefect();
        }

        private void btnDefectReInputSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefectReInput()) return;
            SaveDefectReInput();
        }

        private void dgDefectReInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgDefectReInput_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void dgDefectReInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void tbBox_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetBoxList();
        }

        private void btnBoxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxCreate()) return;
            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateBox();
                }
            });
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            GetBoxList();
        }

        private void btnBoxInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxInput()) return;
            //투입 하시겠습니까?
            Util.MessageConfirm("SFU1248", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputBox();
                }
            });
        }

        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxDelete()) return;
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteBox();
                }
            });
        }

        private void btnBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxPrint()) return;
            //발행하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PrintBox();
                }
            });
        }

        private void dgBox_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgBox.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    string checkFlag = DataTableConverter.GetValue(drv, "CHK").GetString();
                    SetBoxButtonEnable(checkFlag == "True" ? e.Cell.Row : null);

                    int rowIndex = 0;
                    foreach (var item in dgBox.Rows)
                    {
                        if (drv["CHK"].GetString() == "True")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tcDataCollectRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1TabControl c1TabControl = sender as C1TabControl;
            if (c1TabControl.IsLoaded)
            {
                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();


                if (string.Equals(ProcessCode, Process.WASHING) || string.Equals(ProcessCode, Process.WINDING))
                {
                    if (string.Equals(tabItem, "tabQualityInfo"))
                        grdTrayLegend.Visibility = Visibility.Collapsed;
                    else
                        grdTrayLegend.Visibility = Visibility.Visible;
                }
                else
                {
                    grdTrayLegend.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetOutTraybyAsync();
        }

        private void btnSearchTray_Click(object sender, RoutedEventArgs e)
        {
            CMM_WINDING_TRAY_SEARCH popPanReplace = new CMM_WINDING_TRAY_SEARCH { FrameOperation = FrameOperation };
            popPanReplace.Show();
        }

        private void txtTrayIDInline_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (ValidationCreateTray() == true)
                    {
                        btnSave_ClickInline();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_ClickInline()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string sBizNAme = string.Empty;

                        sBizNAme = "BR_PRD_REG_EQPT_END_OUT_LOT_WN_CST";

                        DataSet inDataSet = new DataSet();
                        DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                        DataRow drow = inEQP.NewRow();
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();

                        inEQP.Columns.Add("SRCTYPE", typeof(string));
                        inEQP.Columns.Add("IFMODE", typeof(string));
                        inEQP.Columns.Add("EQPTID", typeof(string));
                        inEQP.Columns.Add("USERID", typeof(string));
                        inEQP.Columns.Add("PROD_LOTID", typeof(string));
                        inEQP.Columns.Add("EQPT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUT_LOTID", typeof(string));
                        inEQP.Columns.Add("OUTPUT_QTY", typeof(string));
                        inEQP.Columns.Add("CSTID", typeof(string));


                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = EquipmentCode;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                        drow["EQPT_LOTID"] = DvProductLot["LOTID"].GetString();
                        drow["OUT_LOTID"] = "";
                        drow["OUTPUT_QTY"] = GetTrayQty();
                        drow["CSTID"] = txtTrayIDInline.Text;

                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            GetProductCellList(dgProdCellWinding);
                            IsProductLotRefreshFlag = true;

                        }, inDataSet);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnCellWindingConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm(dgProdCellWinding)) return;

            try
            {
                _dispatcherTimerOutWinding?.Stop();
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTray(dgProdCellWinding);
                    }
                    else
                    {
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWindingConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirmCancel(dgProdCellWinding)) return;
            try
            {
                _dispatcherTimerOutWinding?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTrayCancel(dgProdCellWinding);
                    }
                    else
                    {
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDischarge(dgProdCellWinding)) return;
            try
            {
                _dispatcherTimerOutWinding?.Stop();
                //배출 하시겠습니까?
                Util.MessageConfirm("SFU3613", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DischargeTray(dgProdCellWinding);
                    }
                    else
                    {
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellWindingDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemoveTray(dgProdCellWinding))
                return;

            RemoveTray();
        }

        private void btnCellWindingSave_Click(object sender, RoutedEventArgs e)
        {
            SaveTrayWinding();
        }

        private void dgProdCellWinding_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    //위치정보 오류(#FFE400)
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_NG")).Equals("NG"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE400");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        // 배출완료(#E8F7C8)
                        if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else
                        {
                            // 확정(#E6F5FB)
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_TRAY_CNFM_FLAG").GetString() == "Y")
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                            else
                            {
                                // 미확정(#F8DAC0)
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                    }

                    if (e.Cell.Column.Name.Equals("btnModify"))
                    {
                        if (!string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_TRAY_CNFM_FLAG").GetString(), "Y") && string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTDTTM_OT").GetString()))
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                            ((ContentControl)e.Cell.Presenter.Content).Tag = "U";
                            ((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                            ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("조회");
                            ((ContentControl)e.Cell.Presenter.Content).Tag = string.Empty;
                            ((ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Black);
                            ((ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                            ((ContentControl)e.Cell.Presenter.Content).Tag = "X";
                        }
                    }
                }
            }));
        }

        private void dgProdCellWinding_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgProdCellWinding_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProdCellWinding.GetCellFromPoint(pnt);

            if (cell != null)
            {
                _rowIndexCheck = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK").GetString();
                decimal cellQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CELLQTY").GetDecimal();
                decimal cellCstQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CST_CELL_QTY").GetDecimal();


                dgProdCellWinding.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;

                if (string.Equals(moveStateCode, "EQPT_END") && cellQty <= cellCstQty)
                {
                    DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", false);

                    if (checkFlag.Equals("0"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", false);
                        btnCellWindingSave.IsEnabled = true;
                        if (cellQty > cellCstQty)
                        {
                            DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", true);
                        }
                        _reLoadFlag = string.Empty;
                    }

                    if (checkFlag.Equals("1"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", true);
                        if (btnCellWindingSave.IsEnabled == true)
                        {
                            btnCellWindingSave.IsEnabled = true;
                        }
                        else
                        {
                            btnCellWindingSave.IsEnabled = false;
                        }
                        if (cellQty > cellCstQty)
                        {
                            DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", true);
                        }
                        _reLoadFlag = string.Empty;
                    }
                }
                else
                {
                    if (checkFlag.Equals("0"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", false);
                        btnCellWindingSave.IsEnabled = false;
                        _reLoadFlag = string.Empty;
                    }

                    if (checkFlag.Equals("1"))
                    {
                        DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", true);
                        if (btnCellWindingSave.IsEnabled == false)
                        {
                            btnCellWindingSave.IsEnabled = true;
                        }
                        _reLoadFlag = string.Empty;
                    }
                }
            }
        }

        private void dgProdCellWinding_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                switch (Convert.ToString(e.Cell.Column.Name))
                {
                    case "CELLQTY":
                        SetParentQty();
                        break;
                }

                if (_reLoadFlag.Equals("Y"))
                {
                    GetProductCellList(dg, true);
                }
            }
        }
        
        private void btnSearchWashingResult_Click(object sender, RoutedEventArgs e)
        {
            if (DvProductLot == null || !string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                GetWashingResult();
        }

        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTraySearch()) return;
            _dispatcherTimerOutWashing?.Stop();
            CMM_ASSY_TRAY_INFO popTraySearch = new CMM_ASSY_TRAY_INFO { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = ProcessCode;
            parameters[1] = EquipmentSegmentCode;

            C1WindowExtension.SetParameters(popTraySearch, parameters);
            popTraySearch.Closed += popTraySearch_Closed;
            Dispatcher.BeginInvoke(new Action(() => popTraySearch.ShowModal()));
        }

        private void popTraySearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_INFO pop = sender as CMM_ASSY_TRAY_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void btnTrayMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayMove()) return;
            _dispatcherTimerOutWashing?.Stop();

            DataTable dt = DvProductLot.Row.Table.Copy();

            CMM_ASSY_TRAY_MOVE popTrayMove = new CMM_ASSY_TRAY_MOVE { FrameOperation = FrameOperation };
            object[] parameters = new object[4];
            parameters[0] = ProcessCode;
            parameters[1] = EquipmentSegmentCode;
            parameters[2] = EquipmentCode;
            parameters[3] = dt;

            C1WindowExtension.SetParameters(popTrayMove, parameters);
            popTrayMove.Closed += popTrayMove_Closed;

            Dispatcher.BeginInvoke(new Action(() => popTrayMove.ShowModal()));
        }

        private void popTrayMove_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_MOVE pop = sender as CMM_ASSY_TRAY_MOVE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();

        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationCreateTrayWashing()) return;
                    CreateTray();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayCreate()) return;
            _dispatcherTimerOutWashing?.Stop();
            
            // 수량관리
            if (string.Equals(_cellManagementTypeCode, "N"))
            {
                CreateTrayByQuantity();
            }

            else
            {
                // 위치관리
                CreateTrayByPosition();
            }
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("발행 후 삭제가 불가 합니다. 발행 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU4445", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    BoxIDPrint();
                }
            });
        }

        private void btnOutSaveZZS_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutBox())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    OutLotSpclSave();

                    SaveOutBox();
                }
            });
        }

        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayDelete()) return;

            try
            {
                _dispatcherTimerOutWashing?.Stop();
                string messageCode = "SFU1230";

                if (!string.IsNullOrEmpty(ValidationTrayCellQtyCode()))
                    messageCode = ValidationTrayCellQtyCode();

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteTray();
                    }
                    else
                    {
                        if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWashing.Start();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateBox())
                return;

            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutBox(GetNewOutLotid());
                }
            });
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeleteBox())
                return;

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteOutBox();
                }
            });
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!CanCreateBox())
                    return;

                //생성 하시겠습니까?
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOutBox(GetNewOutLotid());
                        txtBoxid.Focus();
                    }
                });
            }
        }
        
        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutProduct();
        }
        
        private void dgOutProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
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
            }));
        }

        private void dgOutProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void chkOutLotSpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutLotReamrk != null)
            {
                txtOutLotReamrk.Text = "";
            }
        }

        private void btnOutLotSplSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanOutLotSplSave())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("적용 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpecialLot();
                }
            });
        }

        private void btnOutConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirmCancel()) return;

            try
            {
                _dispatcherTimerOutWashing?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmCancelTray();
                    }
                    else
                    {
                        if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWashing.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm()) return;

            string cellManagementTypeCode = DvProductLot["CELL_MNGT_TYPE_CODE"].GetString();

            if (string.Equals(LoginInfo.CFG_SHOP_ID, "A010") && string.Equals(LoginInfo.CFG_AREA_ID, "M2") && string.Equals(cellManagementTypeCode, "C") && !ValidationCellMatched()) // 확정 대상 Tray 중 최소 1개 이상의 Tray에 대해서 1개 이상의 Cell 매칭검사 결과가 OK인지 여부를 체크
            {
                Util.Alert("SFU8362");
                return;
            }

            try
            {
                _dispatcherTimerOutWashing?.Stop();

                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmTray();
                    }
                    else
                    {
                        if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWashing.Start();
                    }
                });
                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellChange()) return;
            _dispatcherTimerOutWashing?.Stop();

            //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            string cellManagementTypeCode = DvProductLot["CELL_MNGT_TYPE_CODE"].GetString();

            // 수량관리
            if (string.Equals(cellManagementTypeCode, "N"))
            {
                CellByQuantity();
            }
            else if (string.Equals(cellManagementTypeCode, "C"))
            {
                WashingCellManagement();
            }
            else
            {
                // 위치관리
                CellByPosition();
            }
        }

        //CCW Cell 정보 조회
        private void btnCCWOutCell_Click(object sender, RoutedEventArgs e)
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgCCWTrayList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }
            _dispatcherTimerOutWashing?.Stop();
            AssemblyReworkCellInfo();
        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTraySave()) return;
            SaveTrayWashing();
        }

        private void dgOut_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PSTN_CHK").GetString() == "NG")
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {

                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CELL_QTY").GetDecimal() > DataTableConverter.GetValue(e.Cell.Row.DataItem, "CELLQTY").GetDecimal())
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                            if (convertFromString != null)
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }


                    if (e.Cell.Column.Index == dgOut.Columns["TRAYID"].Index && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRAYID").GetString(), "NOREAD"))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Violet);
                    }
                }
            }));
        }

        private void dgOut_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                DataRowView drv = e.Row.DataItem as DataRowView;
                if (e.Column.Name == "CHK") e.Cancel = true;

                if (e.Column.Name == "CELLQTY")
                {
                    if (drv != null && e.Row.Type == DataGridRowType.Item)
                    {
                        if ((drv["WIPSTAT"].GetString() == "EQPT_END" || drv["WIPSTAT"].GetString() == "END") && drv["CHK"].GetString() == "1" && _cellManagementTypeCode == "N")
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }

                else if (e.Column.Name == "CBO_SPCL" || e.Column.Name == "SPECIALDESC")
                {
                    if (drv != null && e.Row.Type == DataGridRowType.Item)
                    {
                        if ((drv["WIPSTAT"].GetString() == "EQPT_END" || drv["WIPSTAT"].GetString() == "END") && drv["CHK"].GetString() == "1")
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOut_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;

                if (e.Cell.Column.Name == "CELLQTY" || e.Cell.Column.Name == "CBO_SPCL" || e.Cell.Column.Name == "SPECIALDESC")
                {
                    if (DataTableConverter.GetValue(drv, "CELLQTY").GetString() == DataTableConverter.GetValue(drv, "CELLQTY_BASE").GetString()
                        &&
                        DataTableConverter.GetValue(drv, "SPECIALDESC").GetString() == DataTableConverter.GetValue(drv, "SPECIALDESC_BASE").GetString()
                        &&
                        DataTableConverter.GetValue(drv, "SPECIALYN").GetString() == DataTableConverter.GetValue(drv, "SPECIALYN_BASE").GetString()
                        )
                    {
                        DataTableConverter.SetValue(drv, "TransactionFlag", "N");
                    }
                    else
                    {
                        DataTableConverter.SetValue(drv, "TransactionFlag", "Y");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOut_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgOut.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int idx = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CHK").GetString();

                if (string.Equals(moveStateCode, "ASSY_OUT"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        string code = DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE").GetString();

                        if (checkFlag == "0")
                        {
                            if (code != "ASSY_OUT")
                            {
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (idx == i)
                                    DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", true);
                            }
                        }
                        else
                        {
                            if (idx == i)
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }

                else if (string.Equals(moveStateCode, "FORM_IN"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (checkFlag == "0")
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", idx == i);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }
                else if (string.Equals(moveStateCode, "WAIT"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        string code = DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE").GetString();

                        if (checkFlag == "0")
                        {
                            if (code != "WAIT")
                            {
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (idx == i)
                                    DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", true);
                            }

                        }
                        else
                        {
                            if (idx == i)
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }
            }
        }
        
        private void tbCheckHeaderAllWashing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsCheckedOnDataGridWashing())
            {
                chkHeaderAll_Checked(null, null);
            }
            else
            {
                Util.DataGridCheckAllUnChecked(dgOut);
            }
        }

        private void chkOutTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrk != null)
            {
                txtOutTrayReamrk.Text = string.Empty;
                txtRegPerson.Text = string.Empty;
            }
        }

        private void btnRegPerson_Click(object sender, RoutedEventArgs e)
        {
            CMM_PERSON popPerson = new CMM_PERSON();
            popPerson.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = "";
            C1WindowExtension.SetParameters(popPerson, parameters);

            popPerson.Closed += new EventHandler(popPerson_Closed);
            Dispatcher.BeginInvoke(new Action(() => popPerson.ShowModal()));
        }

        private void popPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRegPerson.Text = wndPerson.USERNAME;
            }
        }

        private void btnOutTraySplSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSpecialTraySave()) return;

            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSpecialTray();
                }
            });
        }

        private void rdoTraceUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
        }

        private void rdoTraceNotUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
        }

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerOutWashing != null)
                {
                    _dispatcherTimerOutWashing.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut?.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimerOutWashing.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimerOutWashing.Start();

                    if (_isAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        if (cboAutoSearchOut != null)
                            Util.MessageInfo("SFU1605", cboAutoSearchOut.SelectedValue.GetString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (!IsCheckedOnDataGrid())
            {
                chkHeaderAll_CheckedInline(null, null);
            }
            else
            {
                chkHeaderAll_UncheckedInline(null, null);
            }
        }

        private void chkHeaderAll_CheckedInline(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgProdCellWinding))
            {
                DataTable dt = ((DataView)dgProdCellWinding.ItemsSource).Table;
                var sortedTable = dt.Copy().AsEnumerable()
                    .OrderBy(r => r.Field<string>("LOTDTTM_CR"))
                    .Take(_trayCheckSeq).ToList();

                foreach (C1.WPF.DataGrid.DataGridRow row in dgProdCellWinding.Rows)
                {
                    if (row.Type != DataGridRowType.Item) continue;

                    decimal cellQty = DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();
                    decimal cellCstQty = DataTableConverter.GetValue(row.DataItem, "CST_CELL_QTY").GetDecimal();
                    string strStatCode = DataTableConverter.GetValue(row.DataItem, "STAT_CODE").GetString();

                    if (cellQty > 0)
                    {
                        if (sortedTable.Any())
                        {
                            if (sortedTable.AsQueryable().Any(x => x.Field<string>("TRAYID") == DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString()))
                            {
                                if (strStatCode.Equals("EQPT_END"))
                                {
                                    if (cellQty <= cellCstQty)
                                    {
                                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                                        btnCellWindingSave.IsEnabled = true;
                                    }
                                }

                            }
                        }
                    }
                }
                dgProdCellWinding.EndEdit();
                dgProdCellWinding.EndEditRow(true);
            }
        }

        private void cboAutoSearchOutWinding_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerOutWinding != null)
                {
                    _dispatcherTimerOutWinding.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutWinding?.SelectedValue != null && !cboAutoSearchOutWinding.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutWinding.SelectedValue.ToString());

                    if (iSec == 0 && _isWindingSetAutoTime)
                    {
                        _dispatcherTimerOutWinding.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isWindingSetAutoTime = true;
                        return;
                    }

                    _dispatcherTimerOutWinding.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimerOutWinding.Start();

                    if (_isWindingSetAutoTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        if (cboAutoSearchOutWinding != null)
                            Util.MessageValidation("SFU1605", cboAutoSearchOutWinding.SelectedValue.GetString());
                    }

                    _isWindingSetAutoTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgOut))
            {
                DataTable dt = ((DataView)dgOut.ItemsSource).Table;
                var sortedTable = dt.Copy().AsEnumerable()
                    .Where(x => x.Field<string>("WIPSTAT") == "EQPT_END" || x.Field<string>("WIPSTAT") == "END")
                    .Where(x => x.Field<string>("TransactionFlag") == "N")
                    .Where(x => x.Field<decimal>("CELLQTY") > 0)
                    .Where(x => x.Field<string>("FORM_MOVE_STAT_CODE") == "WAIT")
                    .OrderBy(r => r.Field<string>("LOTDTTM_CR"))
                    .Take(_trayCheckSeq).ToList();

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (row.Type != DataGridRowType.Item) continue;

                    string transactionFlag = DataTableConverter.GetValue(row.DataItem, "TransactionFlag").GetString();
                    string fromMoveStateCode = DataTableConverter.GetValue(row.DataItem, "FORM_MOVE_STAT_CODE").GetString();
                    string wipStat = DataTableConverter.GetValue(row.DataItem, "WIPSTAT").GetString();
                    decimal cellQty = DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();

                    if (transactionFlag == "N" && cellQty > 0 && (wipStat == "EQPT_END" || wipStat == "END") && fromMoveStateCode == "WAIT")
                    {
                        if (sortedTable.Any())
                        {
                            if (sortedTable.AsQueryable().Any(x => x.Field<string>("TRAYID") == DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString()))
                            {
                                DataTableConverter.SetValue(row.DataItem, "CHK", true);
                            }
                        }
                    }
                }
                dgOut.EndEdit();
                dgOut.EndEditRow(true);
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProdCellWinding);
        }

        private void chkHeaderAll_UncheckedInline(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProdCellWinding);
        }

        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SetTimeControl();
                GetQualityInfoList();
            }
        }

        private void rdoTime_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SetTimeControl();
                GetQualityInfoList();
            }
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualitySave()) return;
            SaveQuality();
        }

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
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

        private void dgQualityInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL01", StringComparison.Ordinal) >= 0)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));            
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1DataGrid grid = null;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    if (n != null)
                    {
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                        if (p != null)
                        {
                            rIdx = p.Cell.Row.Index;
                            cIdx = p.Cell.Column.Index;
                            grid = p.DataGrid;
                        }
                    }
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    StackPanel panel = n?.Parent as StackPanel;
                    DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                    if (p != null)
                    {
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                }
                else
                    return;


                if (grid.GetRowCount() > ++rIdx)
                {
                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid?.ScrollIntoView(rIdx + 1, cIdx);
                    }

                    if (grid != null)
                    {
                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
                        StackPanel panel = p.Content as StackPanel;

                        if (panel != null)
                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                    panel.Children[cnt].Focus();
                            }
                    }
                }
                else if (CommonVerify.HasDataGridRow(grid) && grid.GetRowCount() == rIdx)
                {
                    //btnQualitySave.Focus();
                }
            }
        }

        private void CLCTVAL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid = null;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }
                    else
                        return;

                    if (grid.GetRowCount() > ++rIdx)
                    {
                        // Null 오류 Scroll 추가
                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }

                        if (grid != null)
                        {
                            DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                            if (p == null) return;
                            StackPanel panel = p.Content as StackPanel;

                            if (panel != null)
                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid = null;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n?.Parent as StackPanel;
                        DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                        if (p != null)
                        {
                            rIdx = p.Cell.Row.Index;
                            cIdx = p.Cell.Column.Index;
                            grid = p.DataGrid;
                        }
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        if (n != null)
                        {
                            StackPanel panel = n.Parent as StackPanel;
                            DataGridCellPresenter p = panel?.Parent as DataGridCellPresenter;
                            if (p != null)
                            {
                                rIdx = p.Cell.Row.Index;
                                cIdx = p.Cell.Column.Index;
                                grid = p.DataGrid;
                            }
                        }
                    }
                    else
                        return;

                    if (grid.GetRowCount() > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        // Null 오류 Scroll 추가
                        if (rIdx >= 0)
                        {
                            if (rIdx == 0)
                                grid.ScrollIntoView(rIdx, cIdx);
                            else
                                grid.ScrollIntoView(rIdx - 1, cIdx);
                        }
                        else
                        {
                            return;
                        }

                        if (grid != null)
                        {
                            DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                            if (p == null) return;
                            StackPanel panel = p.Content as StackPanel;

                            if (panel != null)
                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
                if (drv == null) return;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);

                    string usl = drv["USL"].GetString();
                    string lsl = drv["LSL"].GetString();

                    if (n != null)
                    {
                        if (!string.IsNullOrEmpty(usl) && !string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal() || n.Value.GetDecimal() < lsl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(usl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() < usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                    }
                }
                else //if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_BOXCARD_PRINT popup = sender as CMM_ASSY_BOXCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void popTrayCreateQuantity_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_CREATE_CELL_QTY pop = sender as CMM_ASSY_TRAY_CREATE_CELL_QTY;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }

            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();

        }

        private void popTrayCreatePosition_Closed(object sender, EventArgs e)
        {
            CMM_WASHING_WG_CELL_INFO pop = sender as CMM_WASHING_WG_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }

            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void popCellPosition_Closed(object sender, EventArgs e)
        {
            //CMM_WASHING_WG_CELL_INFO pop = sender as CMM_WASHING_WG_CELL_INFO;
            ASSY006_WASHING_CELL_INFO pop = sender as ASSY006_WASHING_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void popCellManagement_Closed(object sender, EventArgs e)
        {
            ASSY006_WASHING_CELL_MANAGEMENT pop = sender as ASSY006_WASHING_CELL_MANAGEMENT;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void popReworkCellManagement_Closed(object sender, EventArgs e)
        {
            ASSY006_ASSEMBLY_CELL_INFO pop = sender as ASSY006_ASSEMBLY_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void cboTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetQualityInfoList();
        }

        private void dgWaitBox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    if (e.Cell != null &&
                        e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null)
                    {
                        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                        if (chk != null)
                        {
                            switch (Convert.ToString(e.Cell.Column.Name))
                            {
                                case "CHK":
                                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitBox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = null;
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgWaitBox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnWaitBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitBoxInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitBoxInPut())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BasketInput(false, -1);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputBox))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputCancel2();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayIDPKG_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    CreateTrayPKG();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayIDPKG_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtTrayIDPKG == null) return;
                InputMethod.SetPreferredImeConversionMode(txtTrayIDPKG, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayQtyPKG_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtTrayQtyPKG.Text, 0))
                {
                    txtTrayQtyPKG.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    CreateTrayPKG();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutDelPKG_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayDelete())
                return;

            //string sMsg = "삭제 하시겠습니까?";
            string messageCode = "SFU1230";
            string sCellQty = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "CELLQTY"));
            double dCellQty = 0;

            if (!sCellQty.Equals(""))
                double.TryParse(sCellQty, out dCellQty);

            if (!sCellQty.Equals("") && dCellQty != 0)
            {
                //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                messageCode = "SFU1320";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayDelete();
                }
            });
        }

        private void btnOutConfirmCancelPKG_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirmCancel())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetTrayConfirmCancel();
                }
            });
        }

        private void btnOutConfirmPKG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgOutPKG.EndEdit();

                if (!CanTrayConfirm())
                    return;

                TrayConfirmProcess();

                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCellPKG_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            ChangeCellInfo();
        }

        private void btnOutSavePKG_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveTray())
                return;

            SaveTray();
        }

        private void btnOutMove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            if (wndTrayMove != null)
                wndTrayMove = null;

            wndTrayMove = new ASSY003_007_TRAY_MOVE();
            wndTrayMove.FrameOperation = FrameOperation;

            if (wndTrayMove != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = EquipmentSegmentCode;
                Parameters[1] = EquipmentCode;
                Parameters[2] = DvProductLot["LOTID"].GetString();
                Parameters[3] = DvProductLot["WIPSEQ"].GetString();
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "TRAYID")).Replace("\0", "");
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "OUT_LOTID"));


                C1WindowExtension.SetParameters(wndTrayMove, Parameters);

                wndTrayMove.Closed += new EventHandler(wndTrayMove_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndTrayMove.ShowModal()));
            }
        }

        private void wndTrayMove_Closed(object sender, EventArgs e)
        {
            wndTrayMove = null;
            ASSY003_007_TRAY_MOVE window = sender as ASSY003_007_TRAY_MOVE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetOutTray();
            }
        }

        private void dgOutPKG_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        // 이전 값 저장.
                                        _PRV_VLAUES.sPrvTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));

                                        SetOutTrayButtonEnablePKG(e.Cell.Row);

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;

                                        _PRV_VLAUES.sPrvTray = "";

                                        // 확정 시 저장, 삭제 버튼 비활성화
                                        SetOutTrayButtonEnablePKG(null);
                                    }
                                }
                                break;
                        }

                        if (dgOutPKG.CurrentCell != null)
                            dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.CurrentCell.Row.Index, dgOutPKG.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0)
                            dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.Rows.Count, dgOutPKG.Columns.Count - 1);

                    }
 
                }
            }));
        }

        private void dgOutPKG_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgOutPKG_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgOutPKG_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
                    //dgOut.BeginEdit(e.Row.Index, 4);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutPKG_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Row == null || e.Cell.Row.DataItem == null || e.Cell.Column == null)
                    return;

                if (e.Cell.Column.Name.Equals("CELLQTY") ||
                    e.Cell.Column.Name.Equals("SPECIALDESC") ||
                    e.Cell.Column.Name.Equals("CBO_SPCL")
                   )
                {
                    C1DataGrid dg = e.Cell.DataGrid;

                    int iColIdx = dg.Columns["CHK"].Index;
                    C1.WPF.DataGrid.DataGridCell dgTmpCell = dg.GetCell(e.Cell.Row.Index, iColIdx); // Checkbox

                    CheckBox chk = dgTmpCell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        if (dgTmpCell.Presenter != null &&
                            dgTmpCell.Presenter.Content != null &&
                            (dgTmpCell.Presenter.Content as CheckBox) != null &&
                            (dgTmpCell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                            !(bool)(dgTmpCell.Presenter.Content as CheckBox).IsChecked)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk.IsChecked = true;

                                // 이전 값 저장.
                                _PRV_VLAUES.sPrvTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));

                                SetOutTrayButtonEnablePKG(e.Cell.Row);

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, iColIdx).Presenter != null &&
                                            dg.GetCell(idx, iColIdx).Presenter.Content != null &&
                                            (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["OUT_LOTID"].Index);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns["OUT_LOTID"].Index);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutPKG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (dgOutPKG.CurrentCell.Column.Name.Equals("FORM_MOVE_STAT_CODE_NAME"))
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgOutPKG.CurrentCell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        if (AuthCheck())
                        {
                            Util.MessageConfirm("SFU1243", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    SetTrayConfirmCancelAdmin(Util.NVC(DataTableConverter.GetValue(dgOutPKG.CurrentCell.Row.DataItem, "OUT_LOTID")),
                                        Util.NVC(DataTableConverter.GetValue(dgOutPKG.CurrentCell.Row.DataItem, "TRAYID")));
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkOutTraySplPKG_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrkPKG != null)
            {
                txtOutTrayReamrkPKG.Text = "";
            }
        }

        private void btnOutTraySplSavePKG_Click(object sender, RoutedEventArgs e)
        {
            if (!CanOutTraySplSave())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("적용 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpecialTray();
                }
            });
        }
        
        private void dgMaterialInput_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;
                                        _previewValues.PreviewCurentInput = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                            _previewValues.PreviewCurentInput = string.Empty;
                                        }


                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtMaterialInputLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!ValidationAutoInputLot())
                        return;

                    string positionId = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string positionName = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                    
                    object[] parameters = new object[2];
                    parameters[0] = positionName;
                    parameters[1] = txtMaterialInputLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            InputAutoLotWinding(txtMaterialInputLotID.Text.Trim(), positionId);

                            txtMaterialInputLotID.Text = string.Empty;
                        }
                    }, parameters);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialInputReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMaterialInputReplace()) return;

                PopupReplaceWinding();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEqptRemainUnmount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationEqptRemainUnmount())
                    return;

                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN")) == "Y")
                        {
                            EqptRemainInputCancel();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




















        #endregion

        #region Mehod

        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                //btnSecCutOff,
                //btnSaveFaulty,
                //btnSaveRemark
            };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductionResult()
        {
            SetControl();
            SetControlClear();
            SetControlVisibility();

            if (string.Equals(ProcessCode, Process.WINDING))
            {
                SetProductionResult();
                GetDefectInfo(DvProductLot);

                GetQualityInfoList();
                //와인딩 생산반제품 조회
                GetProductCellList(dgProdCellWinding);

                //자재투입/잔량처리
                GetMaterialInputList();
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                SetProductionResult();
                GetDefectInfo(DvProductLot);
                CalculateDefectQty();

                GetQualityInfoList();
                //재투입 탭, Washing 실적 조회
                GetDefectReInputList();
                GetWashingResult();
                // CCW 재작업 Tray List
                GetCCWTrayList();
            }
            else if (string.Equals(ProcessCode, Process.ZZS))
            {
                SetProductionResult();
                GetDefectInfoZZS(DvProductLot);
                
                GetQualityInfoList();

                //ZZS 생산반제품 조회
                GetOutMagazine();
                CalculateDefectQty();

            }
            else if (string.Equals(ProcessCode, Process.PACKAGING))
            {
                SetProductionResult();
                
                SetBoxMountPstsIDCombo();
                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls();
                //불량 조회
                GetDefectInfoZZS(DvProductLot);
                //투입바구니 조회
                GetInBoxList();
                //대기바구니 조회
                GetWaitBox();
                GetCellTraceFlag(DvProductLot["PRODID"].GetString());
                //생산반제품 조회
                GetOutTrayPKG();
                CalculateDefectQty();

            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(ProcessCode, Process.ZTZ))
            {
                SetProductionResult();
                GetDefectInfoZtz(DvProductLot);
                GetQualityInfoZtzList();
                SelectRemark();
            }
            else
            {
                SetProductionResult();
                GetDefectInfo(DvProductLot);
                //CalculateDefectQty();

                GetCellManagementInfo();
                _cellManagementTypeCode = DvProductLot["CELL_MNGT_TYPE_CODE"].GetString();
                ChangeCellManagementType(_cellManagementTypeCode);
                GetTrayCheckCount();
                GetBoxList();
                GetOutTraybyAsync();
                GetSpecialTrayInfo();
            }

            // 설비불량정보 A2000, A3000, A4000 전공정에서 호출 함.
            GetEquipmentFaultyData();
            
        }

        public void GetMaterialInputList()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_WNS";
                
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgMaterialInput, searchResult, FrameOperation);

                        if (!_previewValues.PreviewCurentInput.Equals(""))
                        {
                            int idx = _util.GetDataGridRowIndex(dgMaterialInput, "EQPT_MOUNT_PSTN_ID", _previewValues.PreviewCurentInput);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgMaterialInput.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgMaterialInput.SelectedIndex = idx;
                                dgMaterialInput.ScrollIntoView(idx, dgMaterialInput.Columns["CHK"].Index);
                            }
                        }

                        dgMaterialInput.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                        dgMaterialInput.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
                        dgMaterialInput.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;

                        if (dgMaterialInput.CurrentCell != null)
                            dgMaterialInput.CurrentCell = dgMaterialInput.GetCell(dgMaterialInput.CurrentCell.Row.Index, dgMaterialInput.Columns.Count - 1);
                        else if (dgMaterialInput.Rows.Count > 0 && dgMaterialInput.GetCell(dgMaterialInput.Rows.Count, dgMaterialInput.Columns.Count - 1) != null)
                            dgMaterialInput.CurrentCell = dgMaterialInput.GetCell(dgMaterialInput.Rows.Count, dgMaterialInput.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCellTraceFlag(string sProdID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PRODID"] = sProdID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELL_ID_MNGT_FLAG_CL", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CELL_ID_MNGT_FLAG") && Util.NVC(dtRslt.Rows[0]["CELL_ID_MNGT_FLAG"]).Equals("Y"))
                    {
                        bCellTraceMode = true;

                        if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                            dgOut.Columns["CELLQTY"].IsReadOnly = true;

                        txtTraceMode.Text = "(Trace mode)";

                        btnOutCell.IsEnabled = true;
                    }
                    else
                    {
                        bCellTraceMode = false;

                        if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                            dgOut.Columns["CELLQTY"].IsReadOnly = false;

                        txtTraceMode.Text = "(Not Trace mode)";

                        btnOutCell.IsEnabled = false;
                    }
                }
                else
                {
                    bCellTraceMode = true;

                    if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                        dgOut.Columns["CELLQTY"].IsReadOnly = true;

                    txtTraceMode.Text = "(Trace mode)";
                    btnOutCell.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                bCellTraceMode = true;

                if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                    dgOut.Columns["CELLQTY"].IsReadOnly = true;

                txtTraceMode.Text = "(Trace mode)";
                btnOutCell.IsEnabled = true;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetProductionResult()
        {
            if (DvProductLot == null) return;

            // 생산량, 양품량은 바인딩 이전에 재 조회하여 바인딩  함.
            //GetInputQoodQty();

            SelectLotInfo();            

            SetProductionResultDetailGrid();

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응          
            SetParentQtyZtz();

        }

        private void SelectLotInfo()
        {
            textLotId.Text = DvProductLot["LOTID"].GetString();
            textWorkTime.Text = DvProductLot["WIPDTTM_ST"].GetString() + " ~ " + DvProductLot["WIPDTTM_ED"].GetString();
            textProdId.Text = DvProductLot["PRODID"].GetString();
            textProjectName.Text = DvProductLot["PRJT_NAME"].GetString();
            txtProdVerCode.Text = DvProductLot["PROD_VER_CODE"].GetString();

            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                tbWipstat.Visibility = Visibility.Visible;
                txtWipstat.Visibility = Visibility.Visible;
                tbUnit.Visibility = Visibility.Visible;
                txtUnit.Visibility = Visibility.Visible;
                tbParentQty.Visibility = Visibility.Visible;
                txtParentQty.Visibility = Visibility.Visible;
                tbRemainQty.Visibility = Visibility.Visible;
                txtRemainQty.Visibility = Visibility.Visible;
                tbInputQtyZtz.Visibility = Visibility.Visible;
                textInputQtyZtz.Visibility = Visibility.Visible;
                tbLane.Visibility = Visibility.Collapsed;
                txtLaneQty.Visibility = Visibility.Collapsed;

                tbInputQty.Visibility = Visibility.Collapsed;
                textInputQty.Visibility = Visibility.Collapsed;
                tbGoodQty.Visibility = Visibility.Collapsed;
                textGoodQty.Visibility = Visibility.Collapsed;
                textTmpProdQty.Visibility = Visibility.Collapsed;                

                txtWipstat.Text = DvProductLot["WIPSTAT_NAME"].GetString();
                txtUnit.Text = DvProductLot["UNIT_CODE"].GetString();
                txtParentQty.Value = Convert.ToDouble(DvProductLot["INPUTQTY"]).GetDouble();
                txtLaneQty.Value = Convert.ToDouble(DvProductLot["LANE_QTY"]).GetDouble();
                textProjectName.Text = DvProductLot["PROD_VER_CODE"].GetString();
                textTmpProdQty.Value = Convert.ToDouble(DvProductLot["TMP_PROD_QTY"]).GetDouble();
                
            }
            else
            {
                tbWipstat.Visibility = Visibility.Collapsed;
                txtWipstat.Visibility = Visibility.Collapsed;
                tbUnit.Visibility = Visibility.Collapsed;
                txtUnit.Visibility = Visibility.Collapsed;
                tbParentQty.Visibility = Visibility.Collapsed;
                txtParentQty.Visibility = Visibility.Collapsed;
                tbRemainQty.Visibility = Visibility.Collapsed;
                txtRemainQty.Visibility = Visibility.Collapsed;

                tbInputQtyZtz.Visibility = Visibility.Collapsed;
                textInputQtyZtz.Visibility = Visibility.Collapsed;
                tbLane.Visibility = Visibility.Collapsed;
                txtLaneQty.Visibility = Visibility.Collapsed;
                tbGoodQty.Visibility = Visibility.Visible;
                textGoodQty.Visibility = Visibility.Visible;
                tbInputQty.Visibility = Visibility.Visible;
                textInputQty.Visibility = Visibility.Visible;

            }            

            if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                if(string.IsNullOrEmpty(txtProdVerCode.Text))
                {
                    DataTable dtVersion = GetProcessVersion(DvProductLot["LOTID"].GetString(), DvProductLot["PRODID"].GetString());

                    if(CommonVerify.HasTableRow(dtVersion))
                    {
                        txtProdVerCode.Text = dtVersion.Rows[0]["PROD_VER_CODE"].GetString();
                    }
                }
            }

            textInputQty.Value = double.NaN;
            textGoodQty.Value = double.NaN;
        }

        private void GetInputQoodQty()
        {

        }

        public void SetProductionResultDetailGrid()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("EQPTQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));
            inDataTable.Columns.Add("REINPUTQTY", typeof(int));
            inDataTable.Columns.Add("BOXQTY", typeof(int));
            inDataTable.Columns.Add("EQPTQTY_M_EA", typeof(string));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["EQPTQTY"] = "0";
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;
            dtRow["REINPUTQTY"] = 0;
            dtRow["BOXQTY"] = 0;
            dtRow["EQPTQTY_M_EA"] = "0";

            inDataTable.Rows.Add(dtRow);
            dgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        public void GetDefectInfo(DataRowView rowview)
        {
            if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;
            const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";

            try
            {
                //ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }

                if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    CalculateDefectQty();
                }
                else if(string.Equals(ProcessCode, Process.WASHING))
                {
                    GetOutTraybyAsync();
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetDefectInfoZZS(DataRowView rowview)
        {
            if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;
            const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT";

            try
            {
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    dgDefectZZS.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        protected virtual void CalculateDefectQty()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("CalculateDefectQty");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private double GetSumDefectQty()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
                return 0;

            DataTable dt = ((DataView)dgDefect.ItemsSource).Table;
            double defectqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("DEFECT_LOT") && !w.Field<string>("RSLT_EXCL_FLAG").Equals("Y")).Sum(s => s.Field<double>("RESNQTY"));
            double lossqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("LOSS_LOT")).Sum(s => s.Field<double>("RESNQTY"));
            double chargeprdqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("CHARGE_PROD_LOT")).Sum(s => s.Field<double>("RESNQTY"));

            return defectqty + lossqty + chargeprdqty;
        }

        private decimal GetBoxQty()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                return 0;

            const string bizRuleName = "DA_PRD_SEL_WAIT_BOX_LIST_WS";
            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = indataTable.NewRow();
            dr["LOTID"] = DvProductLot["LOTID"].GetString();
            indataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", indataTable);
            decimal returnBoxQty = CommonVerify.HasTableRow(searchResult) ? searchResult.AsEnumerable().Sum(s => s.Field<decimal>("WIPQTY")) : 0;
            return returnBoxQty;
        }

        private decimal GetGoodQty()
        {
            decimal returnGoodQty;

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                return 0;

            const string bizRuleName = "DA_PRD_SEL_WASHING_LOT_RSLT";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            returnGoodQty = CommonVerify.HasTableRow(searchResult) ? searchResult.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY_INPUT")) : 0;
            return returnGoodQty;
        }

        private decimal GetInputQty()
        {
            decimal returnInputQty;

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                return 0;

            string bizRuleName = string.Equals(ProcessCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
            const string materialType = "PROD";

            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LANGID", typeof(string));
            indataTable.Columns.Add("LOTID", typeof(string));
            indataTable.Columns.Add("MTRLTYPE", typeof(string));
            indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            indataTable.Columns.Add("EQPTID", typeof(string));
            indataTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = indataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = string.Empty;
            dr["MTRLTYPE"] = materialType;
            dr["EQPT_MOUNT_PSTN_ID"] = null;
            dr["EQPTID"] = EquipmentCode;
            dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
            indataTable.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

            if (CommonVerify.HasTableRow(dt))
            {
                returnInputQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("INPUT_QTY"));
            }
            else
            {
                returnInputQty = 0;
            }
            return returnInputQty;
        }

        private decimal GetReInputQty()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                return 0;

            if (tabReInput.Visibility == Visibility.Visible)
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    return CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]) ? dsResult.Tables["OUTDATA"].AsEnumerable().Sum(s => s.Field<decimal>("RESNQTY")) : 0;
                }
                return 0;
            }
            else
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_DIFF_QTY";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = DvProductLot["LOTID"].GetString();
                dr["WIPSEQ"] = DvProductLot["WIPSEQ"].GetInt();
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                return CommonVerify.HasTableRow(searchResult) ? searchResult.Rows[0]["INPUT_DIFF_QTY"].GetDecimal() : 0;
            }
        }

        private void SaveDefect()
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                // TODO A4000 와싱공정의 경우 IsChangeDefect = false; 추가 됨.
                GetDefectInfo(DvProductLot);
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SaveDefectBeforeConfirm()
        {
            try
            {
                dgDefect.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_MOBILE";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                //IsChangeDefect = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetEquipmentFaultyData()
        {
            try
            {
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode; //"N3ALAMM05";
                newRow["LOTID"] = DvProductLot["LOTID"].GetString(); //"UTKB31LCT3" 
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                inTable.Rows.Add(newRow);
                //"10.61.7.62", "tcp", "7865","SERVICE", "0"
                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);

                        dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                        dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                        if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                        else
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        IsSelectedAll = true;
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetParentQty()
        {
            string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "STAT_CODE"));
            string checkFlag = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK").GetString();
            decimal cellQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CELLQTY").GetDecimal();
            decimal cellCstQty = DataTableConverter.GetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CST_CELL_QTY").GetDecimal();

            if (cellQty > cellCstQty)
            {
                Util.MessageValidation("SFU1500", txtTrayIDInline.Text.ToUpper());
                btnCellWindingSave.IsEnabled = false;
                DataTableConverter.SetValue(dgProdCellWinding.Rows[_rowIndexCheck].DataItem, "CHK", false);
                _reLoadFlag = "Y";
            }
            else
            {
                _reLoadFlag = "";
            }
        }

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
        private void SetParentRemainQty()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;
            inputQtyOrg = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = Util.NVC_Decimal(textInputQtyZtz.Value);

            txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty));
            textInputQtyZtz.Value = 0;
            inputQtyOrg = inputQty;            
        }

        private string GetReInputReasonApplyFlag(string equipmentCode)
        {
            const string bizRuleName = "DA_PRD_SEL_REINPUT_RSN_APPLY_FLAG";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            return CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0][0].GetString() : string.Empty;
        }

        private void GetDefectReInputList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefectReInput.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefectReInput.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectReInput()
        {
            try
            {
                dgDefectReInput.EndEdit();

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_FOR_REINPUT";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefectReInput.Rows.Count - dgDefectReInput.BottomRows.Count; i++)
                {
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "RESNNOTE"));
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectReInput.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDefectLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, indataSet);

                GetDefectReInputList();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetBoxList()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WAIT_BOX_LIST_WS";
                ShowLoadingIndicator();

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LOTID"] = DvProductLot["LOTID"].GetString();
                indataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        dgBox.ItemsSource = DataTableConverter.Convert(Util.CheckBoxColumnAddTable(searchResult, false));
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

        private void CreateBox()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CREATE_BOX_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CELL_QTY", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                dr["USERID"] = LoginInfo.USERID;
                dr["WIPNOTE"] = txtWipNote.Text;
                dr["SHIFT"] = ShiftId;
                dr["WRK_USERID"] = WorkerId;
                dr["WRK_USER_NAME"] = WorkerName;
                dr["CELL_QTY"] = BoxQty.Value;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    IsProductLotRefreshFlag = true;
                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    txtWipNote.Text = string.Empty;
                    BoxQty.Value = 0;
                    GetBoxList();
                    CalculateDefectQty();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InputBox()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_INPUT_BOX_LOT_WS";
                DataTable dtEquipment = new DataTable("IN_EQP");
                dtEquipment.Columns.Add("SRCTYPE", typeof(string));
                dtEquipment.Columns.Add("IFMODE", typeof(string));
                dtEquipment.Columns.Add("EQPTID", typeof(string));
                dtEquipment.Columns.Add("USERID", typeof(string));
                dtEquipment.Columns.Add("PROD_LOTID", typeof(string));
                dtEquipment.Columns.Add("EQPT_LOTID", typeof(string));
                DataRow dr = dtEquipment.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                dr["EQPT_LOTID"] = string.Empty;
                dtEquipment.Rows.Add(dr);

                DataTable dtInput = new DataTable("IN_INPUT");
                dtInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                dtInput.Columns.Add("PRODID", typeof(string));
                dtInput.Columns.Add("INPUT_LOTID", typeof(string));
                dtInput.Columns.Add("INPUT_QTY", typeof(decimal));
                DataRow dataRow = dtInput.NewRow();
                dataRow["EQPT_MOUNT_PSTN_ID"] = string.Empty;
                dataRow["EQPT_MOUNT_PSTN_STATE"] = string.Empty;
                dataRow["PRODID"] = string.Empty;
                dataRow["INPUT_LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<string>("LOTID").GetString();
                dataRow["INPUT_QTY"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<decimal>("WIPQTY").GetDecimal();
                dtInput.Rows.Add(dataRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(dtEquipment);
                ds.Tables.Add(dtInput);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetBoxList();
                    IsProductLotRefreshFlag = true;
                    Util.MessageInfo("SFU1275");
                    CalculateDefectQty();
                }, ds);

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void DeleteBox()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DELETE_BOX_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<string>("LOTID").GetString();
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    IsProductLotRefreshFlag = true;
                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    txtWipNote.Text = string.Empty;
                    BoxQty.Value = 0;
                    GetBoxList();
                    CalculateDefectQty();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PrintBox()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_BOX_RUNCARD_DATA_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = _util.GetDataGridFirstRowBycheck(dgBox, "CHK", true).Field<string>("LOTID").GetString();
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    CMM_ASSY_BOXCARD_PRINT poopupHistoryCard = new CMM_ASSY_BOXCARD_PRINT { FrameOperation = this.FrameOperation };
                    object[] parameters = new object[1];
                    parameters[0] = result;
                    C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                    poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);
                    Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetBoxButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    btnBoxInput.IsEnabled = true;
                    btnBoxDelete.IsEnabled = true;

                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "MODE")).Equals("DEL"))
                    {
                        btnBoxInput.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "MODE")).Equals("INS"))
                    {
                        btnBoxDelete.IsEnabled = false;
                    }
                    else
                    {
                        btnBoxInput.IsEnabled = true;
                        btnBoxDelete.IsEnabled = true;
                    }
                }
                else
                {
                    btnBoxInput.IsEnabled = true;
                    btnBoxDelete.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWashingResult()
        {
            try
            {
                Util.gridClear(dgWashingResult);
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WASHING_LOT_RSLT";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                dr["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWashingResult, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCCWTrayList()
        {
            try
            {
                Util.gridClear(dgCCWTrayList);
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_INPUT_MATERIAL_CCW";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCCWTrayList, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetProductCellList(C1DataGrid dg, bool isAsync = true)
        {
            try
            {
                if (isAsync)
                {
                    ShowLoadingIndicator();
                }

                SetDataGridCheckHeaderInitialize(dg);
                string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WN_CST";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["EQPTID"] = EquipmentCode;
                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", indataTable, (result, ex) =>
                {
                    if (isAsync)
                    {
                        HiddenLoadingIndicator();
                    }

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }

                    Util.GridSetData(dg, result, null, true);

                    //TODO : UcAssyDataCollectInline 에선 불량수량 계산하는 로직이 있음.
                    CalculateDefectQty();
                    _trayCheckSeq = 0;

                    if (dg.Rows.Count > 0)
                        _trayCheckSeq = dg.Rows.Count;
                    dg.GetCell(dg.Rows.Count, 1);

                });
            }
            catch (Exception ex)
            {
                if (isAsync)
                {
                    HiddenLoadingIndicator();
                }

                Util.MessageException(ex);
            }

        }

        private void GetOutMagazine()
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_MAGAZINE_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PR_LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutProduct, searchResult, FrameOperation, false);

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOutProduct.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                // 특별 Tray 정보 조회.
                GetSpecialLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetSpecialLotInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(EquipmentCode)) return null;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    chkOutLotSpl.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    txtOutLotReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutLotSplReason != null && cboOutLotSplReason.Items != null && cboOutLotSplReason.Items.Count > 0 && cboOutLotSplReason.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboOutLotSplReason.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
                                {
                                    cboOutLotSplReason.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutLotSplReason.Items.Count > 0)
                                cboOutLotSplReason.SelectedIndex = 0;
                        }
                    }

                    if ((bool)chkOutLotSpl.IsChecked)
                    {
                        grdSpecialLotMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialLot();
                    }
                    else
                    {
                        grdSpecialLotMode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutLotSpl.IsChecked = false;
                    txtOutLotReamrk.Text = "";

                    if (cboOutLotSplReason.Items.Count > 0)
                        cboOutLotSplReason.SelectedIndex = 0;

                    if ((bool)chkOutLotSpl.IsChecked)
                    {
                        grdSpecialLotMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialLot();
                    }
                    else
                    {
                        grdSpecialLotMode.Visibility = Visibility.Collapsed;
                    }
                }

                //HiddenLoadingIndicator();

                return dtResult;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void ColorAnimationInSpecialLot()
        {
            recSpcLot.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void GetTrayInfo(C1DataGrid dg, out string returnMessage, out string messageCode)
        {
            try
            {
                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WN_CST";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["EQPTID"] = EquipmentCode;
                indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TRAYID"));
                indataTable.Rows.Add(indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", indataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    //if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    if (dtResult.Rows[0]["PROC_TRAY_CNFM_FLAG"].GetString() != "Y")
                    {
                        returnMessage = "OK";
                        messageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        messageCode = "SFU1431";
                    }
                }
                else
                {
                    returnMessage = "NG";
                    //sMsg = "존재하지 않습니다.";
                    messageCode = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnMessage = "EXCEPTION";
                messageCode = ex.Message;
            }
        }

        private void ConfirmTray(C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_WNS";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        //ClearDataCollectControl();
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductCellList(dgProdCellWinding);
                        IsProductLotRefreshFlag = true;
                        //GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();

                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmTrayCancel(C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();
                //int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                const string bizRuleName = "BR_PRD_TRAY_CONFIRM_CANCEL_WNS";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }
                        
                        //ClearDataCollectControl();
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductCellList(dgProdCellWinding);
                        IsProductLotRefreshFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DischargeTray(C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_CHK_CONFIRM_TRAY_WN_CST";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("INLOT");
                inInput.Columns.Add("SEQ", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                inInput.Columns.Add("PROD_LOTID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                int seq = 1;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["SEQ"] = seq.GetString();
                        row["CSTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "TRAYID").GetString();
                        row["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                        inInput.Rows.Add(row);
                        seq = seq + 1;
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,INLOT", "OUT_EQP", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductCellList(dgProdCellWinding);
                        IsProductLotRefreshFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RemoveTray()
        {
            C1DataGrid dg = dgProdCellWinding;
            if (!ValidationTrayDelete(dg)) return;

            try
            {
                _dispatcherTimerOutWinding?.Stop();

                //string sMsg = "삭제 하시겠습니까?";
                string messageCode = "SFU1230";
                double dCellQty = 0;

                string cellQty = Util.NVC(DataTableConverter.GetValue(dg.Rows[_util.GetDataGridCheckFirstRowIndex(dg, "CHK")].DataItem, "CELLQTY"));

                if (!string.IsNullOrEmpty(cellQty))
                    double.TryParse(cellQty, out dCellQty);

                if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                {
                    //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                    messageCode = "SFU1320";
                }

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteTray(dg);
                    }
                    else
                    {
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteTray(C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName;

                if (ProcessCode.Equals(Process.WINDING) || ProcessCode.Equals(Process.WINDING_POUCH))
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WNS";
                else
                    bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS";


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("OUT_LOTID", typeof(string));
                inInput.Columns.Add("TRAYID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = EquipmentCode;
                row["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);


                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["OUT_LOTID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "OUT_LOTID").GetString();
                        row["TRAYID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "TRAYID").GetString();
                        inInput.Rows.Add(row);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //정상 처리 되었습니다.
                        //Util.MessageInfo("SFU1275");
                        //ClearDataCollectControl();
                        //GetWorkOrder();
                        //GetProductLot();
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetProductCellList(dgProdCellWinding);
                        IsProductLotRefreshFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        
                        if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                            _dispatcherTimerOutWinding.Start();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveTrayWinding()
        {
            try
            {
                _dispatcherTimerOutWinding?.Stop();
                dgProdCellWinding.EndEdit();

                const string bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WN_CST";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                for (int i = 0; i < dgProdCellWinding.Rows.Count - dgProdCellWinding.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgProdCellWinding, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["USERID"] = LoginInfo.USERID;

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "TRAYID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProdCellWinding.Rows[i].DataItem, "CELLQTY")));

                    inDataTable.Rows.Add(newRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_DATA", null, indataSet);
                    inDataTable.Rows.Remove(newRow);

                }

                //HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
                if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWinding.Start();

                GetProductCellList(dgProdCellWinding);
                IsProductLotRefreshFlag = true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                if (_dispatcherTimerOutWinding != null && _dispatcherTimerOutWinding.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWinding.Start();
            }
        }

        private void SaveTrayWashing()
        {
            try
            {
                ShowLoadingIndicator();
                _dispatcherTimerOutWashing?.Stop();
                dgOut.EndEdit();
                const string bizRuleName = "BR_PRD_REG_UPD_OUT_LOT_WS";
                string specialyn = null;
                string specialReasonCode = null;

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_UPD_OUT_LOT_WS();
                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(newRow);

                    // Tray 정보 DataTable             
                    DataRow dr = inLot.NewRow();
                    dr["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "OUT_LOTID"));
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "TRAYID"));
                    dr["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")));
                    inLot.Rows.Add(dr);

                    // 특별 Tray DataTable                
                    DataRow dataRow = inSpcl.NewRow();
                    dataRow["SPCL_CST_GNRT_FLAG"] = specialyn;
                    dataRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "SPECIALDESC"));
                    dataRow["SPCL_CST_RSNCODE"] = specialReasonCode;
                    inSpcl.Rows.Add(dataRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT,IN_SPCL", null, indataSet);
                    inDataTable.Rows.Remove(newRow);
                    inLot.Rows.Remove(dr);
                    inSpcl.Rows.Remove(dataRow);
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();

                //GetProductLot();
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();
            }
        }

        private void GetOutTray()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);

                //if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0) return;
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();
                //SetDataGridCheckHeaderInitialize(dgOut);
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = ProcessCode;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOut, GetOutTrayAddColumn(searchResult), FrameOperation, true);

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                var dataGridComboBoxColumn = dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dt.Copy());


                if (!_previousValues.PreviousTray.Equals(""))
                {
                    int idx = _util.GetDataGridRowIndex(dgOut, "OUT_LOTID", _previousValues.PreviousTray);

                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgOut.Rows[idx].DataItem, "CHK", true);

                        dgOut.ScrollIntoView(idx, dgOut.Columns["CHK"].Index);

                        // Tray 관련 버튼 처리.
                        SetOutTrayButtonEnable(dgOut.Rows[idx]);

                        dgOut.CurrentCell = dgOut.GetCell(idx, dgOut.Columns.Count - 1);
                    }
                    else
                    {
                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    }
                }
                else
                {
                    if (dgOut.CurrentCell != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutTraybyAsync()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = ProcessCode;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    HiddenLoadingIndicator();
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgOut, GetOutTrayAddColumn(result), FrameOperation, true);

                    // TODO : UcAssyProduction.DgDefectDetail 불량 데이터 계산로직 확인 필요.
                    CalculateDefectQty();
                    IsProductLotRefreshFlag = true;

                    //특별TRAY 콤보
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");
                    dt.Rows.Add("N", "N");
                    dt.Rows.Add("Y", "Y");

                    var dataGridComboBoxColumn = dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                    if (dataGridComboBoxColumn != null)
                        dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dt.Copy());


                    if (!string.IsNullOrEmpty(_previousValues.PreviousTray))
                    {
                        int idx = _util.GetDataGridRowIndex(dgOut, "OUT_LOTID", _previousValues.PreviousTray);

                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgOut.Rows[idx].DataItem, "CHK", true);
                            dgOut.ScrollIntoView(idx, dgOut.Columns["CHK"].Index);

                            // Tray 관련 버튼 처리.
                            SetOutTrayButtonEnable(dgOut.Rows[idx]);
                            dgOut.CurrentCell = dgOut.GetCell(idx, dgOut.Columns.Count - 1);
                        }
                        else
                        {
                            if (dgOut.CurrentCell != null)
                                dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                            else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                                dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                        }
                    }
                    else
                    {
                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetSpecialTrayInfo()
        {
            try
            {
                //if (ComboEquipment?.SelectedValue == null) return null;

                //if (DgProductLot == null || DgProductLot.Rows.Count <= 2)
                //{
                //    return null;
                //}

                if (string.IsNullOrEmpty(EquipmentCode)) return null;
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"]?.GetString())) return null;

                string productLot = DvProductLot["LOTID"].GetString();
                if (string.IsNullOrEmpty(productLot))
                {
                    Util.MessageInfo("SFU1364");    //LOT ID가 선택되지 않았습니다.
                    return null;
                }

                txtSpecialLotGradeCode.Text = string.Empty;
                string selectedProductLotCode = string.Empty;

                const string bizRuleName = "DA_BAS_SEL_WIPATTR_SPCL_LOT_WS";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = productLot;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    //if (rowIndex >= 0)
                    //{
                    //    selectedProductLotCode = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID").GetString();
                    //}

                    selectedProductLotCode = DvProductLot["LOTID"].GetString();

                    if (!string.IsNullOrEmpty(selectedProductLotCode))
                    {
                        if (string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]), "Y") &&
                            string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]), selectedProductLotCode))
                        {
                            chkOutTraySpl.IsChecked = true;
                            txtOutTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);
                            txtSpecialLotGradeCode.Text = dtResult.Rows[0]["SPCL_LOT_GR_CODE"].GetString();
                        }
                        else
                        {
                            cboOutTraySplReason.SelectedIndex = 0;
                            chkOutTraySpl.IsChecked = false;
                            txtOutTrayReamrk.Text = string.Empty;
                            txtRegPerson.Text = string.Empty;
                            txtSpecialLotGradeCode.Text = string.Empty;
                        }
                    }
                    else
                    {
                        cboOutTraySplReason.SelectedIndex = 0;
                        chkOutTraySpl.IsChecked = false;
                        txtOutTrayReamrk.Text = string.Empty;
                        txtRegPerson.Text = string.Empty;
                        txtSpecialLotGradeCode.Text = string.Empty;
                    }

                    if (cboOutTraySplReason?.Items != null && cboOutTraySplReason.Items.Count > 0 && cboOutTraySplReason.Items.CurrentItem != null)
                    {
                        var dataRowView = cboOutTraySplReason.Items.CurrentItem as DataRowView;
                        DataView dtview = dataRowView?.DataView;
                        if (dtview?.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])) && string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]), selectedProductLotCode))
                                {
                                    cboOutTraySplReason.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutTraySplReason.Items.Count > 0)
                                cboOutTraySplReason.SelectedIndex = 0;
                        }
                    }

                    if (chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutTraySpl.IsChecked = false;
                    txtOutTrayReamrk.Text = string.Empty;
                    txtRegPerson.Text = string.Empty;

                    if (cboOutTraySplReason.Items.Count > 0)
                        cboOutTraySplReason.SelectedIndex = 0;

                    if ((bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }

                if (isFixedSpclTrayByDemandType())
                {
                    chkOutTraySpl.IsEnabled = false;
                    cboOutTraySplReason.IsEnabled = false;
                    txtOutTrayReamrk.IsReadOnly = true;
                    btnOutTraySplSave.IsEnabled = false;
                }
                else
                {
                    chkOutTraySpl.IsEnabled = true;
                    cboOutTraySplReason.IsEnabled = true;
                    txtOutTrayReamrk.IsReadOnly = false;
                    btnOutTraySplSave.IsEnabled = true;
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SaveSpecialTray()
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    Util.MessageInfo("SFU1364");    //LOT ID가 선택되지 않았습니다.
                    return;
                }

                //string productLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
                //if (String.IsNullOrEmpty(productLot))
                //{
                //    Util.MessageInfo("SFU1364");    //LOT ID가 선택되지 않았습니다.
                //    return;
                //}

                txtSpecialLotGradeCode.Text = string.Empty;

                const string bizRuleName = "BR_PRD_REG_EIOATTR_SPCL_CST_WSH";
                string sRsnCode = cboOutTraySplReason.SelectedValue?.ToString() ?? "";

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = Process.PACKAGING;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_LOT_GNRT_FLAG"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtRegPerson.Text + " " + txtOutTrayReamrk.Text;  //C20190415_74474 추가
                //newRow["SPCL_PROD_LOTID"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID")) : "";
                newRow["SPCL_PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            txtSpecialLotGradeCode.Text = bizResult.Rows[0]["SPCL_LOT_GR_CODE"].GetString();
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfo();

                        if (chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked)
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTray();
                        }
                        else
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ChangeCellManagementType(string type)
        {
            if (type == "N")
            {
                btnOutCell.IsEnabled = false;
            }
            else if (type == "P")
            {
                btnOutCell.IsEnabled = true;
            }

            if (type == "N" || type == "P")
            {
                btnTrayMove.IsEnabled = true;
            }
            else
            {
                btnTrayMove.IsEnabled = false;
            }

            DisplayCellManagementType(DvProductLot);
        }

        private void GetCellManagementInfo()
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_MNGT_TYPE_CODE";
            inTable.Rows.Add(dr);
            _dtCellManagement = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", inTable);

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            DataRow dataRow = inDataTable.NewRow();
            dataRow["LANGID"] = LoginInfo.LANGID;
            dataRow["CMCDTYPE"] = "CMCDTYPE";
            dataRow["CMCODE"] = "CELL_MNGT_TYPE_CODE";
            inDataTable.Rows.Add(dataRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
                _cellManageGroup = dtResult.Rows[0]["CMCDNAME"].GetString();
        }

        private void DisplayCellManagementType(DataRowView rowview)
        {
            if (rowview == null)
            {
                tbCellManagement.Text = string.Empty;
            }
            else
            {
                string cellType = string.Empty;

                var query = (from t in _dtCellManagement.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == _cellManagementTypeCode
                             select new { cellType = t.Field<string>("CBO_NAME") }).FirstOrDefault();

                if (query != null)
                    cellType = query.cellType;

                tbCellManagement.Text = "[" + _cellManageGroup + "  : " + cellType + "]";
            }
        }

        private void GetTrayCheckCount()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TRAY_CHECK_QTY";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQSGID"] = EquipmentSegmentCode;
                inTable.Rows.Add(dr);
                DataTable resultTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (CommonVerify.HasTableRow(resultTable))
                    _trayCheckSeq = Convert.ToInt32(resultTable.Rows[0]["CHECK_QTY"]);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateTrayByQuantity()
        {
            CMM_ASSY_TRAY_CREATE_CELL_QTY popTrayCreateQuantity = new CMM_ASSY_TRAY_CREATE_CELL_QTY { FrameOperation = FrameOperation };

            string cellQty = "0";
            string trayId = string.Empty;
            int idx = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");
            if (idx >= 0)
            {
                cellQty = string.IsNullOrEmpty(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY").GetString()) ? "0" : DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY").GetInt().GetString();
                trayId = DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID").GetString();
            }
            object[] parameters = new object[8];
            parameters[0] = EquipmentCode;
            parameters[1] = DvProductLot["LOTID"].GetString();
            parameters[2] = string.Empty;
            parameters[3] = "N";
            parameters[4] = string.Empty;
            parameters[5] = cellQty;
            parameters[6] = trayId;
            parameters[7] = "C"; // 상태값에 따라 화면내용이 변경
            C1WindowExtension.SetParameters(popTrayCreateQuantity, parameters);
            popTrayCreateQuantity.Closed += popTrayCreateQuantity_Closed;
            Dispatcher.BeginInvoke(new Action(() => popTrayCreateQuantity.ShowModal()));
        }

        private void CreateTrayByPosition()
        {
            CMM_WASHING_WG_CELL_INFO popTrayCreatePosition = new CMM_WASHING_WG_CELL_INFO { FrameOperation = FrameOperation };

            object[] parameters = new object[10];
            parameters[0] = ProcessCode;
            parameters[1] = EquipmentSegmentCode;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = DvProductLot["LOTID"].GetString();
            parameters[5] = string.Empty;
            parameters[6] = string.Empty;
            parameters[7] = "C";
            parameters[8] = DvProductLot["WO_DETL_ID"].GetString();
            parameters[9] = "N"; //생산LOT 확정 후 수정 여부

            C1WindowExtension.SetParameters(popTrayCreatePosition, parameters);
            popTrayCreatePosition.Closed += popTrayCreatePosition_Closed;

            Dispatcher.BeginInvoke(new Action(() => popTrayCreatePosition.ShowModal()));
        }

        private void CreateTray()
        {
            try
            {
                _dispatcherTimerOutWashing?.Stop();

                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_START_OUT_LOT_WS";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                DataTable incst = indataSet.Tables.Add("IN_CST");
                incst.Columns.Add("CSTSLOT", typeof(string));
                incst.Columns.Add("CSTSLOT_F", typeof(string));
                incst.Columns.Add("SUBLOTID", typeof(string));

                DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
                inInputLot.Columns.Add("MTRLID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode; //ComboEquipment.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString(); //DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID").GetString();
                dr["EQPT_LOTID"] = string.Empty;
                dr["CSTID"] = txtTrayID.Text.Trim();
                dr["OUTPUT_QTY"] = 0;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST,IN_INPUT", "RSLTDT", (bizResult, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutWashing.Start();

                    //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    GetOutTraybyAsync();
                    txtTrayID.Text = string.Empty;
                    txtTrayID.Focus();
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();
                Util.MessageException(ex);
            }
        }

        private void DeleteTray()
        {
            try
            {
                ShowLoadingIndicator();

                // 원각/ 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS";
                DataTable indataTable = _bizDataSet.GetBR_PRD_REG_DELETE_OUT_LOT_WS();
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                        {
                            DataRow dr = indataTable.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["IFMODE"] = IFMODE.IFMODE_OFF;
                            dr["EQPTID"] = EquipmentCode;
                            dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                            dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                            dr["TRAYID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                            dr["WO_DETL_ID"] = DvProductLot["WO_DETL_ID"].GetString();
                            dr["USERID"] = LoginInfo.USERID;
                            indataTable.Rows.Add(dr);

                            new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP", null, ds);
                            indataTable.Rows.Remove(dr);
                        }
                    }
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();

                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();
            }
        }

        private void ConfirmCancelTray()
        {
            try
            {
                ShowLoadingIndicator();
                // 원각/ 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS();
                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataTable inCstTable = indataSet.Tables["IN_CST"];

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;

                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = EquipmentCode;
                    dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                    dr["USERID"] = LoginInfo.USERID;
                    indataTable.Rows.Add(dr);

                    DataRow newRow = inCstTable.NewRow();
                    newRow["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                    newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                    inCstTable.Rows.Add(newRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_CST", null, indataSet);
                    indataTable.Rows.Remove(dr);
                    inCstTable.Rows.Remove(newRow);
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                //Util.MessageInfo("SFU1275");
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();

                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();
            }
        }

        private void ConfirmTray()
        {
            try
            {
                ShowLoadingIndicator();
                //원각 초소형 공통사용 BizRule
                const string bizRuleName = "BR_PRD_REG_END_OUT_LOT_WS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_ALL_OUT_WS();
                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataTable inCstTable = indataSet.Tables["IN_CST"];


                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;

                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = EquipmentCode;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                    dr["EQPT_LOTID"] = string.Empty;
                    indataTable.Rows.Add(dr);

                    DataRow newRow = inCstTable.NewRow();
                    newRow["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                    newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                    newRow["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetString()) ? 0 : DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();
                    inCstTable.Rows.Add(newRow);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_CST", null, indataSet);
                    indataTable.Rows.Remove(dr);
                    inCstTable.Rows.Remove(newRow);
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                //Util.MessageInfo("SFU1275");
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();

                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                    _dispatcherTimerOutWashing.Start();
            }
        }

        private void GetTrayInfo(out string returnMessage, out string messageCode)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["EQPTID"] = EquipmentCode;
                indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "TRAYID"));
                indataTable.Rows.Add(indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", indataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        returnMessage = "OK";
                        messageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        messageCode = "SFU1431";
                    }
                }
                else
                {
                    returnMessage = "NG";
                    //sMsg = "존재하지 않습니다.";
                    messageCode = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnMessage = "EXCEPTION";
                messageCode = ex.Message;
            }
        }

        private void CellByQuantity()
        {
            CMM_ASSY_TRAY_CREATE_CELL_QTY popCellQuantity = new CMM_ASSY_TRAY_CREATE_CELL_QTY { FrameOperation = FrameOperation };

            string cellQty = "0";
            string trayId = string.Empty;

            int idx = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");
            if (idx >= 0)
            {
                cellQty = string.IsNullOrEmpty(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY").GetString()) ? "0" : DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY").GetInt().GetString();
                trayId = DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID").GetString();
            }


            object[] parameters = new object[8];
            parameters[0] = EquipmentCode;
            parameters[1] = DvProductLot["LOTID"].GetString();
            parameters[2] = string.Empty;
            parameters[3] = "Y";
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
            parameters[5] = cellQty;
            parameters[6] = trayId;
            //상태가 미확정일 경우만 수정가능  그외 조회
            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) == "WAIT")
            {
                parameters[7] = "U";
            }
            else
            {
                parameters[7] = "R";
            }

            C1WindowExtension.SetParameters(popCellQuantity, parameters);
            popCellQuantity.Closed += popCellQuantity_Closed;

            Dispatcher.BeginInvoke(new Action(() => popCellQuantity.ShowModal()));
        }

        private void popCellQuantity_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_CREATE_CELL_QTY pop = sender as CMM_ASSY_TRAY_CREATE_CELL_QTY;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            if (_dispatcherTimerOutWashing != null && _dispatcherTimerOutWashing.Interval.TotalSeconds > 0)
                _dispatcherTimerOutWashing.Start();
        }

        private void CellByPosition()
        {
            //CMM_WASHING_WG_CELL_INFO popCellPosition = new CMM_WASHING_WG_CELL_INFO { FrameOperation = FrameOperation };
            ASSY006_WASHING_CELL_INFO popCellPosition = new ASSY006_WASHING_CELL_INFO { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");

            object[] parameters = new object[10];
            parameters[0] = ProcessCode;
            parameters[1] = EquipmentSegmentCode;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = DvProductLot["LOTID"].GetString();
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
            parameters[6] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID"));

            //상태가 미확정일 경우만 수정가능  그외 조회
            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) == "WAIT")
            {
                parameters[7] = "U";
            }
            else
            {
                parameters[7] = "R";
            }
            parameters[8] = DvProductLot["WO_DETL_ID"].GetString();
            parameters[9] = "N"; //생산LOT 확정 후 수정 여부

            C1WindowExtension.SetParameters(popCellPosition, parameters);
            popCellPosition.Closed += popCellPosition_Closed;

            Dispatcher.BeginInvoke(new Action(() => popCellPosition.ShowModal()));
        }
        
        private void WashingCellManagement()
        {
            if (!ValidationCellChange()) return;

            ASSY006_WASHING_CELL_MANAGEMENT popCellManagement = new ASSY006_WASHING_CELL_MANAGEMENT { FrameOperation = FrameOperation };
            //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            object[] parameters = new object[9];
            parameters[0] = DvProductLot["LOTID"].GetString();
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
            parameters[3] = EquipmentCode;
            //상태가 미확정일 경우에만 저장/삭제가 가능하다 - 그 외 나머지 상태는 조회만 가능
            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) != "WAIT")
            {
                //Read 모드
                parameters[4] = "R";
            }
            else
            {   //Write 모드
                parameters[4] = "W";
            }
            parameters[5] = "N";    //completeProd 여부
            parameters[6] = WorkerId; //UcAssyShift.TextWorker.Tag;     // 작업자 ID
            parameters[7] = idx > 0 ? Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx-1].DataItem, "TRAYID")) : "";
            parameters[8] = idx < dgOut.GetRowCount()-1 ? Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx+1].DataItem, "TRAYID")) : "";

            C1WindowExtension.SetParameters(popCellManagement, parameters);
            popCellManagement.Closed += popCellManagement_Closed;

            Dispatcher.BeginInvoke(new Action(() => popCellManagement.ShowModal()));
        }

        private void AssemblyReworkCellInfo()
        {
            ASSY006_ASSEMBLY_CELL_INFO popCellManagement = new ASSY006_ASSEMBLY_CELL_INFO { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgCCWTrayList, "CHK");

            object[] parameters = new object[9];
            parameters[0] = DvProductLot["LOTID"].GetString();
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCCWTrayList.Rows[idx].DataItem, "CSTID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCCWTrayList.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = EquipmentCode;

            C1WindowExtension.SetParameters(popCellManagement, parameters);
            popCellManagement.Closed += popReworkCellManagement_Closed;

            Dispatcher.BeginInvoke(new Action(() => popCellManagement.ShowModal()));
        }

        private static DataTable GetOutTrayAddColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "CELLQTY_BASE", DataType = typeof(decimal) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "SPECIALYN_BASE", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "SPECIALDESC_BASE", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "TransactionFlag", DataType = typeof(string) });

            foreach (DataRow row in dtBinding.Rows)
            {
                row["CELLQTY_BASE"] = row["CELLQTY"];
                row["SPECIALYN_BASE"] = row["SPECIALYN"];
                row["SPECIALDESC_BASE"] = row["SPECIALDESC"];
                row["TransactionFlag"] = "N";
            }
            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private DataTable GetProcessVersion(string sLotID, string sProdID)
        {
            // VERSION을 룰에 따라 가져옴
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCSTATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["EQSGID"] = EquipmentSegmentCode;
                indata["PROCID"] = ProcessCode;
                indata["EQPTID"] = EquipmentCode;
                indata["LOTID"] = sLotID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PRODID"] = sProdID;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_ASSY", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = true;
                        if (_cellManagementTypeCode != "N") btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        if (_cellManagementTypeCode != "N") btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN")) // 활성화입고
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = false;
                        if (_cellManagementTypeCode != "N") btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        if (_cellManagementTypeCode != "N") btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutCreate.IsEnabled = true;
                    btnOutDel.IsEnabled = true;
                    btnOutConfirmCancel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    if (_cellManagementTypeCode != "N") btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetOutTrayButtonEnablePKG(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDelPKG.IsEnabled = true;
                        btnOutConfirmCancelPKG.IsEnabled = false;
                        btnOutConfirmPKG.IsEnabled = true;
                        btnOutCellPKG.IsEnabled = true;
                        btnOutSavePKG.IsEnabled = true;
                        btnOutMovePKG.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDelPKG.IsEnabled = false;
                        btnOutConfirmCancelPKG.IsEnabled = true;
                        btnOutConfirmPKG.IsEnabled = false;
                        btnOutCellPKG.IsEnabled = true;
                        btnOutSavePKG.IsEnabled = false;
                        btnOutMovePKG.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDelPKG.IsEnabled = false;
                        btnOutConfirmCancelPKG.IsEnabled = false;
                        btnOutConfirmPKG.IsEnabled = false;
                        btnOutCellPKG.IsEnabled = true;
                        btnOutSavePKG.IsEnabled = false;
                        btnOutMovePKG.IsEnabled = false;
                    }
                    else
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDelPKG.IsEnabled = true;
                        btnOutConfirmCancelPKG.IsEnabled = true;
                        btnOutConfirmPKG.IsEnabled = true;
                        btnOutCellPKG.IsEnabled = true;
                        btnOutSavePKG.IsEnabled = true;
                        btnOutMovePKG.IsEnabled = true;
                    }
                }
                else
                {
                    //btnOutCreate.IsEnabled = true;
                    btnOutDelPKG.IsEnabled = true;
                    btnOutConfirmCancelPKG.IsEnabled = true;
                    btnOutConfirmPKG.IsEnabled = true;
                    btnOutCellPKG.IsEnabled = true;
                    btnOutSavePKG.IsEnabled = true;
                    btnOutMovePKG.IsEnabled = true;
                }

                // Cell 추적 여부에 따른 Cell ID 버튼 활성/비활성화
                if (bCellTraceMode)
                {
                    btnOutCellPKG.IsEnabled = true;
                }
                else
                {
                    btnOutCellPKG.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ColorAnimationInSpecialTray()
        {
            recSpcTray.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private void SetTimeControl()
        {
            if (rdoTime.IsChecked == true)
            {
                txtTime.Visibility = Visibility.Visible;
                cboTime.Visibility = Visibility.Visible;
            }
            else
            {
                txtTime.Visibility = Visibility.Collapsed;
                cboTime.Visibility = Visibility.Collapsed;
            }
        }

        private void GetQualityInfoList()
        {
            try
            {
                Util.gridClear(dgQuality);
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                string bizRuleName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
                if (rdoTime.IsChecked == true)
                {
                    bizRuleName = "BR_QCA_SEL_SELF_INSP_CLCTITEM_TIME";
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_BAS_CODE", typeof(string));
                inTable.Columns.Add("CLCT_ITVL", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = ProcessCode;
                dr["LOTID"] = DvProductLot["LOTID"].GetString();
                dr["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                dr["EQPTID"] = EquipmentCode;
                dr["CLCT_BAS_CODE"] = "LOT";

                if(string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if(!string.IsNullOrEmpty(DvProductLot["PROD_VER_CODE"].GetString()))
                        dr["PROD_VER_CODE"] = DvProductLot["PROD_VER_CODE"].GetString();
                }

                if (rdoTime.IsChecked == true)
                {
                    dr["CLCT_BAS_CODE"] = "TIME";
                }
                if (rdoTime.IsChecked == true && cboTime.Items.Count > 0)
                {
                    dr["CLCT_ITVL"] = cboTime.SelectedValue;
                }
                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    Util.GridSetData(dgQuality, result, FrameOperation, true);

                    if (rdoTime.IsChecked == true && cboTime.Items.Count < 1)
                    {
                        MakeTimeCombo(result.Copy());
                    }
                    _util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "CLSS_NAME1", "CLSS_NAME2", "CLSS_NAME3" }, DataGridMergeMode.VERTICALHIERARCHI);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MakeTimeCombo(DataTable dt)
        {
            var query = dt.Copy().AsEnumerable()
                .OrderBy(o => o.Field<Int32>("CLCT_ITVL"))
                .GroupBy(g => new { clctVal = g.Field<Int32>("CLCT_ITVL") })
                .Select(s => s.Key.clctVal).ToList();

            DataTable dtTime = new DataTable();
            dtTime.Columns.Add("CODE", typeof(string));
            dtTime.Columns.Add("CODENAME", typeof(string));

            foreach (var item in query)
            {
                DataRow dr = dtTime.NewRow();
                dr["CODE"] = item.GetString();
                dr["CODENAME"] = item.GetString();
                dtTime.Rows.Add(dr);
            }

            DataRow newRow = dtTime.NewRow();
            newRow["CODE"] = null;
            newRow["CODENAME"] = "-ALL-";
            dtTime.Rows.InsertAt(newRow, 0);

            cboTime.DisplayMemberPath = "CODENAME";
            cboTime.SelectedValue = "CODE";
            cboTime.ItemsSource = dtTime.Copy().AsDataView();
            cboTime.SelectedIndex = 0;

            cboTime.SelectedValueChanged += cboTime_SelectedValueChanged;
        }

        private void SaveQuality()
        {
            try
            {
                dgQuality.EndEdit();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Int16));
                inTable.Columns.Add("CLCTSEQ", typeof(Int16));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("CLCTMAX", typeof(string));
                inTable.Columns.Add("CLCTMIN", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTSEQ_ORG", typeof(Int16));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgQuality.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = DvProductLot["LOTID"].GetString();
                    dr["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    if (rdoLot.IsChecked == true)
                    {
                        dr["CLCTSEQ"] = 1;
                    }
                    dr["CLCTITEM"] = DataTableConverter.GetValue(row.DataItem, "CLCTITEM").GetString();
                    dr["CLCTVAL01"] = DataTableConverter.GetValue(row.DataItem, "CLCTVAL01").GetString();

                    if (DataTableConverter.GetValue(row.DataItem, "MAND_INSP_ITEM_FLAG").GetString().ToUpper() == "Y")
                    {
                        if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                        {
                            object[] parameters = new object[1];
                            string clssName1 = DataTableConverter.GetValue(row.DataItem, "CLSS_NAME1").GetString();
                            string clssName2 = DataTableConverter.GetValue(row.DataItem, "CLSS_NAME2").GetString();

                            if (string.IsNullOrEmpty(clssName2) == true)
                            {
                                parameters[0] = (clssName1);
                            }
                            else
                            {
                                parameters[0] = (clssName1 + " - " + clssName2);
                            }
                            //parameters[0] = (dr["CLSS_NAME1"].GetString() + dr["CLSS_NAME2"].GetString()).Length > 0 ? " - " + dr["CLSS_NAME2"].GetString(): string.Empty;
                            Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.
                            return;
                        }
                    }
                    dr["CLCTMAX"] = DataTableConverter.GetValue(row.DataItem, "USL");
                    dr["CLCTMIN"] = DataTableConverter.GetValue(row.DataItem, "LSL");
                    dr["EQPTID"] = EquipmentCode;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CLCTSEQ_ORG"] = DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    inTable.Rows.Add(dr);
                }

                string serviceName = "BR_QCA_REG_WIP_DATA_CLCT";
                if (rdoTime.IsChecked == true)
                {
                    serviceName = "BR_QCA_REG_WIP_DATA_CLCT_TIME_FOR_LOT";
                }

                if (inTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync(serviceName, "INDATA", null, inTable);
                    if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        Util.MessageInfo("SFU1270");      //저장되었습니다.
                        GetQualityInfoList();
                    }
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetWaitBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["WO_DETL_ID"] = DvProductLot["WO_DETL_ID"].GetString(); 
                newRow["LOTID"] = txtWaitBoxLot.Text.Trim();
                //newRow["LOTID"] = DvProductLot["LOTID"].GetString(); ;

                inTable.Rows.Add(newRow);

                string bizRuleName = ProcessCode == Process.SSC_FOLDED_BICELL ? "DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD" : "DA_PRD_SEL_WAIT_LOT_LIST_CL_S";

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }
                        
                        Util.GridSetData(dgWaitBox, searchResult, FrameOperation);

                        if (dgWaitBox.CurrentCell != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.CurrentCell.Row.Index, dgWaitBox.Columns.Count - 1);
                        else if (dgWaitBox.Rows.Count > 0 && dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1) != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }
        
        private void SaveOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MODIFY_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")));
                    newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));

                    outLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_LOT_WIPQTY", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        //GetProductLot();
                        GetOutMagazine();
                        IsProductLotRefreshFlag = true;

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void OutLotSpclSave()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inEQP = indataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("PROD_LOTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));

                DataTable inLOT = indataSet.Tables.Add("IN_LOT");
                inLOT.Columns.Add("OUT_LOTID", typeof(string));
                inLOT.Columns.Add("CSTID", typeof(string));
                inLOT.Columns.Add("WIPQTY", typeof(int));
                inLOT.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                inLOT.Columns.Add("SPCL_CST_NOTE", typeof(string));
                inLOT.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                DataRow newRow = inEQP.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inEQP.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = inLOT.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "CSTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                    newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALYN"));
                    newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALDESC"));
                    newRow["SPCL_CST_RSNCODE"] = "";

                    inLOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UPD_OUT_LOT_SPCL", "IN_EQP,IN_LOT", null, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CanSaveOutBox()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }

                string specYN = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALYN"));
                string SpecDesc = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALDESC"));
                if (specYN.Equals("Y"))
                {
                    if (SpecDesc == "")
                    {
                        //Util.Alert("특별관리내역을 입력하세요.");
                        Util.MessageValidation("SFU1990");
                        return bRet;
                    }
                }
                else if (specYN.Equals("N"))
                {
                    if (SpecDesc != "")
                    {
                        //Util.Alert("특별관리내역을 삭제하세요.");
                        Util.MessageValidation("SFU1989");
                        return bRet;
                    }
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanDeleteBox()
        {
            bool bRet = false;

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }

                //사용자권한 MESADMIN인경우 삭제가능
                if (isMesAdmin)
                {
                    // Dispatch 된 경우 삭제처리 불가.
                    if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "DISPATCH_YN")).Equals("Y"))
                    {
                        Util.MessageValidation("SFU4444", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));  // %1 은 재공이 다음 공정으로 이동 되어 삭제할 수 없습니다.
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DELETE_OUT_LOT_STP();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_STP", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        //GetProductLot();
                        GetOutMagazine();
                        IsProductLotRefreshFlag = true;

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool GetErpSendInfo(string sLotID, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // P : ERP 전송 중 , Y : ERP 전송 완료
                    {
                        bRet = true;
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
                HiddenLoadingIndicator();
            }
        }

        private string GetNewOutLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_GET_NEW_OUT_LOT_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_OUT_LOTID_STP", "INDATA", "OUTDATA", inTable);

                string sNewLot = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dtResult.Rows[0]["OUT_LOTID"]);
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void CreateOutBox(string sNewOutLot)
        {
            try
            {
                if (sNewOutLot.Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_CREATE_OUT_LOT_STP();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["OUT_LOTID"] = sNewOutLot;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WO_DETL_ID"] = null;
                newRow["INPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                newRow["RESNQTY"] = 0;
                newRow["SHIFT"] = null;
                newRow["WIPNOTE"] = null;
                newRow["WRK_USER_NAME"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = txtBoxid.Text.Trim().ToUpper();

                inTable.Rows.Add(newRow);
                //BR_PRD_REG_CREATE_OUT_LOT_STP -> BR_PRD_REG_CREATE_OUT_LOT_ZZS (2022-07-27)
                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_ZZS", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                            BoxIDPrint(sNewOutLot, Convert.ToDecimal(txtOutBoxQty.Text));


                        //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        //GetProductLot();
                        GetOutMagazine();
                        IsProductLotRefreshFlag = true;

                        int idx = _util.GetDataGridRowIndex(dgOutProduct, "LOTID", sNewOutLot);
                        if (idx >= 0)
                            DataTableConverter.SetValue(dgOutProduct.Rows[idx].DataItem, "CHK", true);

                        txtBoxid.Text = "";
                        txtBoxid.Focus();
                        //Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));  //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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
                    //dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    //if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    //{
                    //    dicParam.Add("MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(winWorkOrder.dgWorkOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(winWorkOrder.dgWorkOrder, "CHK")].DataItem, "MKT_TYPE_CODE")));
                    //    dicParam.Add("CSTID", Util.NVC(dtRslt.Rows[0]["CSTID"]));
                    //}

                    dicList.Add(dicParam);


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.ZZS;
                        Parameters[2] = EquipmentSegmentCode;
                        Parameters[3] = EquipmentCode;
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
                }
                else
                {
                    for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                    {
                        DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));

                        if (dtRslt == null || dtRslt.Rows.Count < 1) continue;
                        if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "CHK")).ToString() == "0") continue;


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
                        dicParam.Add("EQPTNO", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALDESC")));
                        //dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                        dicParam.Add("TITLEX", "BASKET ID");

                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                        dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                        if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                        {
                            dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                            dicParam.Add("CSTID", Util.NVC(dtRslt.Rows[0]["CSTID"]));
                        }

                        dicList.Add(dicParam);
                    }


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD();
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.ZZS;
                        Parameters[2] = EquipmentSegmentCode;
                        Parameters[3] = EquipmentCode;
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
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

        private void GetOutProduct()
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutProduct, searchResult, FrameOperation);

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOutProduct.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                // 특별 Tray 정보 조회.
                GetSpecialLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CanCreateBox()
        {
            bool bRet = false;

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) < 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) < 1)
            {
                //Util.Alert("수량은 0보다 커야 합니다.");
                Util.MessageValidation("SFU1683");
                txtOutBoxQty.SelectAll();
                return false;
            }


            bRet = true;
            return bRet;
        }

        private void SetSpecialLot()
        {
            try
            {
                string sRsnCode = cboOutLotSplReason.SelectedValue == null ? "" : cboOutLotSplReason.SelectedValue.ToString();

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = Process.ZZS;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)chkOutLotSpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)chkOutLotSpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutLotReamrk.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)chkOutLotSpl.IsChecked ? DvProductLot["LOTID"].GetString() : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialLotInfo();

                        if ((bool)chkOutLotSpl.IsChecked)
                        {
                            grdSpecialLotMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialLot();
                        }
                        else
                        {
                            grdSpecialLotMode.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanOutLotSplSave()
        {
            bool bRet = false;

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            if (chkOutLotSpl.IsChecked.HasValue && (bool)chkOutLotSpl.IsChecked)
            {
                if (cboOutLotSplReason.SelectedValue == null || cboOutLotSplReason.SelectedValue == null || cboOutLotSplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtOutLotReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return bRet;
                }
            }
            else
            {
                if (!txtOutLotReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private decimal GetTrayQty()  // MMD 설비관리 / 동별/Trayout 기준정보에서 캐리어 적재 수량 반환  2022-05-03
        {

            DataSet dsResult;
            DataTable dtResult;
            const string bizRuleName = "BR_GET_TB_MMD_AREA_PROC_CST_LAYOUT";
            DataSet inDataSet = new DataSet();
            DataTable inData = inDataSet.Tables.Add("INDATA");
            inData.Columns.Add("PROD_LOTID", typeof(string));

            DataRow dr = inData.NewRow();
            dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
            inData.Rows.Add(dr);

            dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA", inDataSet);

            dtResult = dsResult.Tables["OUTDATA"];

            if (dtResult.Rows.Count > 0)
            {
                return dtResult.Rows[0]["CST_LOAD_QTY"].SafeToDecimal();
            }
            else
                return 60;
        }

        private string GetNowRunProdLot()
        {
            try
            {
                ShowLoadingIndicator();

                string sNowLot = "";
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_NOW_PROD_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_NOW_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sNowLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                }

                return sNowLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void BasketInput(bool bAuto, int iRow)
        {
            try
            {

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_BASKET_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (bAuto)
                {
                    if (iRow < 0)
                        return;

                    newRow = null;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "LOTID"));
                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "PRODID"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                else
                {
                    newRow = null;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];

                    for (int i = 0; i < dgWaitBox.Rows.Count - dgWaitBox.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgWaitBox, "CHK", i)) continue;
                        newRow = inMtrlTable.NewRow();
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")));

                        inMtrlTable.Rows.Add(newRow);

                        break;
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_CL_S", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot();
                        GetWaitBox();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetInputHistButtonControls()
        {
            try
            {
                bool bRet = false;
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("ATTRIBUTE1", typeof(string));
                dt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_LOT_CANCEL_TERM_USE";
                dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
                dr["ATTRIBUTE2"] = ProcessCode;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", dt);

                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE5"]).Trim().Equals("Y"))
                {
                    btnInputBoxCancel.Visibility = Visibility.Visible;
                    useInputCancel = true;
                }
                else
                {
                    btnInputBoxCancel.Visibility = Visibility.Collapsed;
                    useInputCancel = false;
                }
                
                if (ProcessCode.Equals(Process.PACKAGING))
                {
                    SetControlButton_PACKAGING();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControlButton_PACKAGING()
        {
            try
            {
                btnInputBoxCancel.Visibility = Visibility.Collapsed; //투입바구니->투입취소
                
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("USERID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH", "INDATA", "OUTDATA", dt);

                if (dtResult?.Rows?.Count > 0)
                {
                    DataRow[] searchedRow = dtResult.Select("(AUTHID = 'MESADMIN' AND USE_FLAG = 'Y') OR (AUTHID = 'PROD_RSLT_MGMT_NJ' AND USE_FLAG = 'Y')");
                    if (searchedRow.Length > 0)
                    {
                        if (useInputCancel == true)
                        {
                            btnInputBoxCancel.Visibility = Visibility.Visible; //투입바구니->투입취소
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void BoxInputCancel2()
        {
            try
            {
                string bizRuleName = string.Empty;

                bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT_S";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputBox.Rows.Count - dgInputBox.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgInputBox, "CHK", i)) continue;
                    newRow = null;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")));
                    newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetInBoxList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        SetInputHistButtonControls();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInBoxList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_IN_BOX_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString(); 
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString().Equals("") ? 1 : Convert.ToDecimal(DvProductLot["WIPSEQ"].GetString());

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputBox.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputBox, searchResult, FrameOperation);

                        //if (dgInputBox.CurrentCell != null)
                        //    dgInputBox.CurrentCell = dgInputBox.GetCell(dgInputBox.CurrentCell.Row.Index, dgInputBox.Columns.Count - 1);
                        //else if (dgInputBox.Rows.Count > 0)
                        //    dgInputBox.CurrentCell = dgInputBox.GetCell(dgInputBox.Rows.Count, dgInputBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CreateTrayPKG()
        {
            try
            {
                if (!CanCreateTrayByTrayID())
                    return;

                string sCreateTrayID, sCreateTrayQty, sCreateOutLot;
                if (CreateTray(out sCreateOutLot, out sCreateTrayID, out sCreateTrayQty))
                {
                    //셀 추적용
                    //if (bCellTraceMode)
                    //{
                    //    ShowCellInfoPopup(sCreateTrayID, sCreateOutLot, sCreateTrayQty);
                    //}
                    txtTrayID.Text = "";
                }

                //GetProductLot();
                GetOutTrayPKG();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CreateTray(out string sCreateOutLot, out string sCreateTrayID, out string sCreateTrayQty)
        {
            sCreateOutLot = "";
            sCreateTrayID = "";
            sCreateTrayQty = "";

            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_CREATE_TRAY_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = ""; // TRAY MAPPING LOT
                newRow["TRAYID"] = txtTrayIDPKG.Text;
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;
                if (!bCellTraceMode)
                {
                    decimal dTmp = 0;
                    decimal.TryParse(txtTrayQtyPKG.Text, out dTmp);
                    newRow["CELL_QTY"] = dTmp;
                }

                inTable.Rows.Add(newRow);

                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL_S", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);

                if (dsRslt != null && dsRslt.Tables.Contains("OUT_LOT") && dsRslt.Tables["OUT_LOT"].Rows.Count > 0)
                {
                    if (dsRslt.Tables["OUT_LOT"].Columns.Contains("OUT_LOTID"))
                        sCreateOutLot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]);
                    else
                        sCreateOutLot = "";

                    if (dsRslt.Tables["OUT_LOT"].Columns.Contains("CST_CAPA_QTY"))
                        sCreateTrayQty = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["CST_CAPA_QTY"]);
                    else
                        sCreateTrayQty = "";

                    sCreateTrayID = txtTrayIDPKG.Text;
                }

                HiddenLoadingIndicator();

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }
        
        private void TrayDelete()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_TRAY_DEL_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "OUT_LOTID"));
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "TRAYID"));
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_DELETE_OUT_LOT_CL_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        //GetProductLot();
                        GetOutTrayPKG();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetTrayConfirmCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_CONFIRM_CNL_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString(); 
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "TRAYID"));

                inCst.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot();
                        GetOutTrayPKG();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfoPKG(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString(); 
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "TRAYID"));

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        sMsg = "SFU3045";   // TRAY가 미확정 상태가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
        }

        private void TrayConfirmProcess()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");

            if (idx < 0)
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2044", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayConfirm();
                }
            });

        }

        private void TrayConfirm()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_CONFIRM_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString(); 
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "OUT_LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CELLQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "TRAYID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] = "";

                inCst.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot();
                        GetOutTrayPKG();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();

                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ChangeCellInfo()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");

            if (idx < 0)
                return;

            string sTrayQty = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CST_CELL_QTY"));//cboTrayType.SelectedValue == null ? "25" : cboTrayType.SelectedValue.ToString();
            string trayID = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "TRAYID")).Replace("\0", "");
            string outLOTID = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "OUT_LOTID"));

            ShowCellInfoPopup(trayID, outLOTID, sTrayQty);

        }

        private void SaveTray()
        {
            try
            {
                dgOut.EndEdit();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");

                string specYN = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "SPECIALYN"));
                string SpecDesc = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "SPECIALDESC"));
                if (specYN.Equals("Y"))
                {
                    if (SpecDesc == "")
                    {
                        //Util.Alert("특별관리내역을 입력하세요.");
                        Util.MessageValidation("SFU1990");
                        return;
                    }
                }
                else if (specYN.Equals("N"))
                {
                    if (SpecDesc != "")
                    {
                        //Util.Alert("특별관리내역을 삭제하세요.");
                        Util.MessageValidation("SFU1989");
                        return;
                    }
                }

                ShowLoadingIndicator();

                string sBizName = "BR_PRD_REG_UPD_OUT_LOT_CL_S";// bCellTraceMode ? "BR_PRD_REG_UPD_OUT_LOT_CL" : "BR_PRD_REG_UPD_OUT_LOT_CL_S";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_SAVE_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                for (int i = 0; i < dgOutPKG.Rows.Count - dgOutPKG.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutPKG, "CHK", i)) continue;
                    // Tray 정보 DataTable             
                    newRow = inLot.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "TRAYID"));
                    if (!bCellTraceMode) // if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                        newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CELLQTY")));


                    inLot.Rows.Add(newRow);
                    newRow = null;

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specYN;
                    newRow["SPCL_CST_NOTE"] = SpecDesc;
                    newRow["SPCL_CST_RSNCODE"] = "";

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot();
                        GetOutTrayPKG();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool AuthCheck()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetTrayConfirmCancelAdmin(string sOutLot, string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inCst = indataSet.Tables.Add("IN_CST");
                inCst.Columns.Add("OUT_LOTID", typeof(string));
                inCst.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                newRow = inCst.NewRow();
                newRow["OUT_LOTID"] = sOutLot;
                newRow["CSTID"] = sTrayID;

                inCst.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL_ADMIN_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot();
                        GetOutTrayPKG();
                        IsProductLotRefreshFlag = true;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetSpecialTray()
        {
            try
            {
                string sRsnCode = cboOutTraySplReasonPKG.SelectedValue == null ? "" : cboOutTraySplReasonPKG.SelectedValue.ToString();

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = Process.PACKAGING;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)chkOutTraySplPKG.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)chkOutTraySplPKG.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrkPKG.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)chkOutTraySplPKG.IsChecked ? DvProductLot["LOTID"].GetString() : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfoPKG();

                        if ((bool)chkOutTraySplPKG.IsChecked)
                        {
                            grdSpecialLotModePKG.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTrayPKG();
                        }
                        else
                        {
                            grdSpecialLotModePKG.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetSpecialTrayInfoPKG()
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    chkOutTraySplPKG.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    txtOutTrayReamrkPKG.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutTraySplReasonPKG != null && cboOutTraySplReasonPKG.Items != null && cboOutTraySplReasonPKG.Items.Count > 0 && cboOutTraySplReasonPKG.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboOutTraySplReasonPKG.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
                                {
                                    cboOutTraySplReasonPKG.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutTraySplReasonPKG.Items.Count > 0)
                                cboOutTraySplReasonPKG.SelectedIndex = 0;
                        }
                    }

                    //cboOutTraySplReason.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);

                    if ((bool)chkOutTraySplPKG.IsChecked)
                    {
                        grdSpecialLotModePKG.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTrayPKG();
                    }
                    else
                    {
                        grdSpecialLotModePKG.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutTraySplPKG.IsChecked = false;
                    txtOutTrayReamrkPKG.Text = "";

                    if (cboOutTraySplReasonPKG.Items.Count > 0)
                        cboOutTraySplReasonPKG.SelectedIndex = 0;

                    if ((bool)chkOutTraySplPKG.IsChecked)
                    {
                        grdSpecialLotModePKG.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTrayPKG();
                    }
                    else
                    {
                        grdSpecialLotModePKG.Visibility = Visibility.Collapsed;
                    }
                }

                //HiddenLoadingIndicator();

                return dtResult;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void ColorAnimationInSpecialTrayPKG()
        {
            recSpcLotPKG.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void GetOutTrayPKG()
        {
            try
            {
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnablePKG(null);

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.
                if (!bCellTraceMode)
                {
                    newRow["CELL_TRACE_FLAG"] = "N";
                }

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL_NJ", "INDATA", "OUTDATA", inTable);

                //dgOut.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgOutPKG, searchResult, FrameOperation, true);

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOutPKG.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                if (!_PRV_VLAUES.sPrvTray.Equals(""))
                {
                    int idx = _util.GetDataGridRowIndex(dgOutPKG, "OUT_LOTID", _PRV_VLAUES.sPrvTray);

                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgOutPKG.Rows[idx].DataItem, "CHK", true);

                        dgOut.ScrollIntoView(idx, dgOutPKG.Columns["CHK"].Index);

                        // Tray 관련 버튼 처리.
                        SetOutTrayButtonEnablePKG(dgOutPKG.Rows[idx]);

                        dgOutPKG.CurrentCell = dgOutPKG.GetCell(idx, dgOutPKG.Columns.Count - 1);
                    }
                    else
                    {
                        if (dgOutPKG.CurrentCell != null)
                            dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.CurrentCell.Row.Index, dgOutPKG.Columns.Count - 1);
                        else if (dgOutPKG.Rows.Count > 0 && dgOutPKG.GetCell(dgOutPKG.Rows.Count, dgOutPKG.Columns.Count - 1) != null)
                            dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.Rows.Count, dgOutPKG.Columns.Count - 1);
                    }
                }
                else
                {
                    if (dgOutPKG.CurrentCell != null)
                        dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.CurrentCell.Row.Index, dgOutPKG.Columns.Count - 1);
                    else if (dgOutPKG.Rows.Count > 0 && dgOutPKG.GetCell(dgOutPKG.Rows.Count, dgOutPKG.Columns.Count - 1) != null)
                        dgOutPKG.CurrentCell = dgOutPKG.GetCell(dgOutPKG.Rows.Count, dgOutPKG.Columns.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }





















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




































        #region [Cell Info Pop up..(Memory leak)]
        #region [Declaration & Constructor]
        private string _TrayID = string.Empty;
        private string _TrayQty = string.Empty;
        private string _OutLotID = string.Empty;
        private bool _ELUseYN = false;
        private Brush[,] originColorMap = null;


        private bool _ShowSlotNo = false;
        private bool _OnlyView = false;

        // 주액 USL, LSL 기준정보
        private string _EL_WEIGHT_LSL = string.Empty;
        private string _EL_WEIGHT_USL = string.Empty;
        private string _EL_AFTER_WEIGHT_LSL = string.Empty;
        private string _EL_AFTER_WEIGHT_USL = string.Empty;
        private string _EL_BEFORE_WEIGHT_LSL = string.Empty;
        private string _EL_BEFORE_WEIGHT_USL = string.Empty;


        ASSY003_007_CELLID_RULE wndCellIDRule = null;

        private static class TRAY_SHAPE
        {
            public static string CELL_TYPE = string.Empty;  // CELL TYPE
            public static int ROW_NUM = 0;  // 총 ROW 수
            public static int COL_NUM = 0;  // 총 COL 수
            public static bool EMPTY_SLOT = false;  // 빈 슬롯 존재 여부
            public static bool ZIGZAG = false;  // COL 별 지그재그 배치 여부
            public static string[] EMPTY_SLOT_LIST = null;  // 빈 슬롯 컬럼 LIST
            public static int MERGE_START_COL_NUM = 0; // 머지 시작 컬럼 넘버
            public static string[] DISPLAY_LIST = null; // Cell 영역에 표시할 Data List
            public static char[] DISP_SEPARATOR; // 표시 영역 구분자
        }

        public bool bExistLayOutInfo = false;
        private bool bViewAll = false;

        #endregion

        #region [Initialize]
        private void InitializeControlsPKG()
        {
            int iTrayQty = 0;
            if (!_TrayQty.Equals("") && int.TryParse(_TrayQty, out iTrayQty))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = 0; i < iTrayQty; i++)
                {
                    dt.Rows.Add((i + 1).ToString(), (i + 1).ToString());
                }

                cboTrayLocation.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboTrayLocation.SelectedIndex = 0;
            }

            //rdoAuto.IsChecked = true;
            cboTrayLocation.IsEnabled = false;

            // 주액 관려 여부에 따른 컨트롤 Hidden 처리
            if (_ELUseYN)
            {
                //rdoManual.IsEnabled = true;
                tblEl.Visibility = Visibility.Visible;
                txtEl.Visibility = Visibility.Visible;
                //tblElMinMax.Visibility = Visibility.Visible;
                tblBeforeWeight.Visibility = Visibility.Visible;
                txtBeforeWeight.Visibility = Visibility.Visible;
                //tblBeforeWeightMinMax.Visibility = Visibility.Visible;
                tblAfterWeight.Visibility = Visibility.Visible;
                txtAfterWeight.Visibility = Visibility.Visible;
                //tblAfterWeightMinMax.Visibility = Visibility.Visible;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Visible;
                txtPosition.Visibility = Visibility.Visible;
                tblJudge.Visibility = Visibility.Visible;
                txtJudge.Visibility = Visibility.Visible;

                btnCheckElJudge.Visibility = Visibility.Visible;
            }
            else
            {
                //rdoManual.IsEnabled = false;
                tblEl.Visibility = Visibility.Collapsed;
                txtEl.Visibility = Visibility.Collapsed;
                tblElMinMax.Visibility = Visibility.Collapsed;
                tblBeforeWeight.Visibility = Visibility.Collapsed;
                txtBeforeWeight.Visibility = Visibility.Collapsed;
                tblBeforeWeightMinMax.Visibility = Visibility.Collapsed;
                tblAfterWeight.Visibility = Visibility.Collapsed;
                txtAfterWeight.Visibility = Visibility.Collapsed;
                tblAfterWeightMinMax.Visibility = Visibility.Collapsed;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Collapsed;
                txtPosition.Visibility = Visibility.Collapsed;
                tblJudge.Visibility = Visibility.Collapsed;
                txtJudge.Visibility = Visibility.Collapsed;

                btnCheckElJudge.Visibility = Visibility.Collapsed;
            }


            // View Mode 인 경우 모두 Disable 처리.
            if (_OnlyView)
            {
                txtCellId.IsReadOnly = true;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                dgDupList.IsReadOnly = true;

                //rdoManual.IsEnabled = true;

                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;

                btnCheckElJudge.Visibility = Visibility.Collapsed;
                btnOutRangeDelAll.Visibility = Visibility.Collapsed;

                this.tbTitle.Text = ObjectDic.Instance.GetObjectName("TRAY별CELLID관리") + " (Read Only)";
            }
        }

        private void InitializeGrid()
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                DataTable dtTemp = new DataTable();

                if (TRAY_SHAPE.ZIGZAG) // zigzag 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 80;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 115;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);


                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // 빈 Cell 정보 Set.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // alternating row color 삭제
                    dgCell.RowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
                    dgCell.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);

                    SetZigZagGridInfo();

                    dgCell.MergingCells -= dgCell_MergingCells;
                    dgCell.MergingCells += dgCell_MergingCells;

                    //MergingCells();
                }
                else // 정상 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 95;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 100;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);

                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // Row Add.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtRow["NO"] = (i + 1).ToString();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // LOCATION 정보 SET.
                    int iLocIdx = 1;    // 실제 Cell Location Number 변수.
                    int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        for (int i = 0; i < dgCell.Rows.Count; i++)
                        {
                            // 빈 슬롯 번호 확인.
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                            {
                                if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                                {
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_JUDGE"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_JUDGE", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_CELLID"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_CELLID", "");
                                }
                                else
                                {
                                    if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                                }
                            }
                            else
                            {
                                if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // Location 정보 설정
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_LOC") >= 0 &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                            {
                                // Cell 에 Location 값 처리.
                                string sOrgView = Util.NVC(dgCell.Columns[j].Name).Replace("_LOC", "");
                                if (dgCell.Columns.Contains(sOrgView))
                                {
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView, iLocIdx.ToString());
                                }

                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, iLocIdx.ToString());
                                iLocIdx++;


                                // Location 정보 Set (View 용)
                                if (dgCell.Columns.Contains(sOrgView + "_SLOTNO") &&
                                    dgCell.Columns.Contains(sOrgView + "_LOC"))
                                {
                                    string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, sOrgView + "_LOC"));

                                    if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView + "_SLOTNO", sTmpLocValue);
                                }
                            }
                            else if (Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                            {

                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // View 컬럼 기준으로 슬롯 넘버링 처리.
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                                iTmpIdx++;

                            //// 25 ROW 넘어가는 경우 ROW HEIGHT 조정
                            //if (dgCell.Rows.Count > 25)
                            //    dgCell.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(22);
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

        #region [Event]
        private void rdoCellID_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    // Display Set.
                    SetTrayDisplayList(new string[] { "CELLID" });

                    // 조회
                    SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void dgCell_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dg.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if ((Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 || Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) && !Util.NVC(dg.Columns[idxCol].Name).Equals("NO"))
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                        }
                    }

                    for (int idxRow = 0; idxRow < dg.Rows.Count; idxRow++)
                    {
                        // Row Number 설정
                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") >= 0)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                            }
                            else
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    //iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Cell Merge 처리.
                                if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                                else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                            }
                            else
                            {
                                // View Column Cell Merge 처리.
                                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 ||
                                    Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) // View 용 slot no 머지 처리.
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                }
                            }
                        }

                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0)
                        {
                            iTmpIdx++;
                        }

                        dg.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        // 자동차동..
                        if (!LoginInfo.CFG_AREA_ID.StartsWith("A"))
                            return;

                        // ReadOnly
                        if (_OnlyView) return;

                        ShowLoadingIndicator();

                        C1DataGrid dgTray = null;

                        dgTray = dgCell;

                        if (dgTray == null)
                            return;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (txtLotId.Text.Length == 10)
                            {
                                string sTmpCellID = txtLotId.Text.Substring(3, 5) + txtLotId.Text.Substring(9, 1);
                                int iRow = 0;
                                int iLocation = 0;
                                int iColCnt = 0;    // 컬럼 수

                                // 해당 LOT의 MAX SEQ 조회.
                                DataTable inTmpTable = new DataTable();
                                inTmpTable.Columns.Add("LOTID", typeof(string));
                                inTmpTable.Columns.Add("OUT_LOTID", typeof(string));
                                inTmpTable.Columns.Add("TRAYID", typeof(string));
                                inTmpTable.Columns.Add("CELLID", typeof(string));

                                DataRow newTmpRow = inTmpTable.NewRow();
                                newTmpRow["LOTID"] = DvProductLot["LOTID"].GetString();

                                inTmpTable.Rows.Add(newTmpRow);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CELL_SEQ_IN_TRAY", "INDATA", "OUTDATA", inTmpTable);

                                if (dtRslt != null && dtRslt.Rows.Count > 0)
                                    iRow = Util.NVC(dtRslt.Rows[0]["MAXSEQ"]).Equals("") ? 0 : Convert.ToInt32(Util.NVC(dtRslt.Rows[0]["MAXSEQ"]));

                                for (int i = 0; i < dgTray.Columns.Count; i++)
                                {
                                    if (!dgTray.Columns[i].Name.Equals("NO") &&
                                        !dgTray.Columns[i].Name.EndsWith("_JUDGE") &&
                                        !dgTray.Columns[i].Name.EndsWith("_LOC"))
                                    {
                                        // 여러 컬럼인 경우 계산.
                                        iLocation = dgTray.Rows.Count * iColCnt;
                                        iColCnt = iColCnt + 1;

                                        for (int j = 0; j < dgTray.Rows.Count; j++)
                                        {
                                            iLocation = iLocation + 1;
                                            iRow = iRow + 1;

                                            string sTmpCell = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name));
                                            string sTmpLoc = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_LOC"));

                                            if ((sTmpCell.Equals("") || sTmpCell.Equals(sTmpLoc)) &&
                                                !Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_JUDGE")).Equals("EMPT_SLOT"))
                                            {
                                                try
                                                {
                                                    DataSet indataSet = _bizDataSet.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

                                                    DataTable inTable = indataSet.Tables["IN_EQP"];

                                                    DataRow newRow = inTable.NewRow();
                                                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                                    newRow["EQPTID"] = EquipmentCode;
                                                    newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString(); 
                                                    newRow["OUT_LOTID"] = _OutLotID;
                                                    newRow["CSTID"] = _TrayID;
                                                    newRow["USERID"] = LoginInfo.USERID;

                                                    inTable.Rows.Add(newRow);
                                                    newRow = null;

                                                    DataTable inSublotTable = indataSet.Tables["IN_CST"];
                                                    newRow = inSublotTable.NewRow();
                                                    newRow["SUBLOTID"] = sTmpCellID + iRow.ToString("0000");
                                                    newRow["CSTSLOT"] = iLocation.ToString();

                                                    inSublotTable.Rows.Add(newRow);

                                                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S", "IN_EQP,IN_CST,IN_EL", null, indataSet);

                                                    System.Threading.Thread.Sleep(300);
                                                }
                                                catch (Exception ex)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }

                                SearchTrayWindow();
                                GetTrayInfo();
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }));
        }

        private void tbxCellId_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbxCellId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                string[] colNameArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                ClearControl();

                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    for (int j = 2; j < dgCell.Columns.Count; j++)
                    {
                        if (dgCell[i, j].Presenter != null)
                        {
                            //dgCell[i, j].Presenter.Background = originColorMap[i, j];
                            dgCell[i, j].Presenter.IsSelected = false;
                            for (int k = 0; k < colNameArr.Length; k++)
                            {
                                if (tbxCellId.Text == dgCell[i, j].Text &&
                                    dgCell.Columns[j].Name == colNameArr[k] && dgCell[i, j].Text == dgCell[i, j + 3].Text)
                                {
                                    dgCell.CurrentCell = dgCell[i, j];
                                    dgCell[i, j].Presenter.IsSelected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void chkViewSlotNo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;

                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    //    (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
                    //}
                    ShowSlotNoColumns(bShowSlotNo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkViewSlotNo_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;

                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    //    (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
                    //}
                    ShowSlotNoColumns(bShowSlotNo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCheckElJudge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EL_SPEC_CHK_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.PACKAGING;

                //if (Util.NVC(_EL_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_USL).Length > 0 )
                //    newRow["EL_SPEC_CHK_FLAG"] = "Y";
                //else
                newRow["EL_SPEC_CHK_FLAG"] = "N";

                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PKG_CELL_CALC_EL_SPEC", "IN_EQP", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        btnSearchCellInfo.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnSearchCellInfo_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayWindow(bChgNexRow: false);

            GetTrayInfo();

            GetOutRangeCellList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = MessageBoxResult.Cancel;
            try
            {
                this.ClearAllCellInfo();

                grCellList.Visibility = Visibility.Collapsed;

                if (this.Parent != null && this.Parent.GetType() == typeof(ContentControl) &&
                    (this.Parent as ContentControl).Parent != null && (this.Parent as ContentControl).Parent.GetType() == typeof(Grid))
                {
                    UIElementCollection uiec = ((this.Parent as ContentControl).Parent as Grid).Children;

                    if (uiec != null)
                    {
                        foreach (System.Windows.UIElement child in uiec)
                        {
                            if (child.GetType() == typeof(Button))
                            {
                                if ((child as Button).Name.ToUpper().Equals("BTNCLOSE"))
                                {
                                    child.Visibility = Visibility.Visible;
                                }
                                break;
                            }
                        }
                    }
                }

                //GetWorkOrder(); // 작지 생산수량 정보 재조회.

                //GetProductLot();
                //GetOutTraybyAsync();
                IsProductLotRefreshFlag = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCell_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    if (e.Cell.Row.Index >= 0 && e.Cell.Column.Index > 0)
                    {
                        if (Util.NVC(e.Cell.Column.Name).IndexOf("_") >= 0 || Util.NVC(e.Cell.Column.Name).IndexOf("NO") >= 0) // Hidden Column 인 경우.
                            return;

                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_JUDGE")).Equals("EMPT_SLOT")) // 빈 슬롯인 경우..
                            return;

                        string sCell = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_CELLID")); //dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value == null ? "" : dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_LOC"));


                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetCellInfo(sCell.Equals(sLoc) ? "" : sCell, e.Cell.Row.Index, e.Cell.Column.Index, sLoc);
                    }
                }
            }));
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("NO"))
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        return;
                    }

                    if (e.Cell.Column.Name.IndexOf("_SLOTNO") > 0)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        string sOrgView = Util.NVC(e.Cell.Column.Name).Replace("_SLOTNO", "");
                        if (dgCell.Columns.Contains(sOrgView + "_JUDGE"))
                        {
                            string sTmpJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sOrgView + "_JUDGE"));

                            if (sTmpJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }

                        return;
                    }

                    if (!dataGrid.Columns.Contains(e.Cell.Column.Name + "_JUDGE"))
                        return;

                    string sJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_JUDGE"));

                    if (sJudge.Equals("SC")) // SC : 특수문자 포함 (Include Special Character) => Cell ID 형식 오류
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NR")) // NR : 읽을 수 없음 (No Read)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("DL")) // DL : 자리수 상이 (Different ID Length)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("ID")) // ID : ID 중복 (ID Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("PD")) // PD : Tray Location 중복 (Position Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NI")) // NI : 주액량 정보 없음 (No Information)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        // 아무것도 없는 경우에는 기본 Base 폰트 색 변경.
                        if (e.Cell.Column.Name.IndexOf("_") < 0 && dataGrid.Columns.Contains(e.Cell.Column.Name + "_LOC") && !e.Cell.Column.Name.Equals("NO"))
                        {
                            string sLocVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_LOC"));
                            string sCellVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name));

                            if (!sLocVal.Equals("EMPT_SLOT") && sCellVal.Trim().Equals(sLocVal)) //sCellVal.Trim().Equals(""))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#B0B0B0"));
                                //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                            }
                            else if (sLocVal.Equals("EMPT_SLOT"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }
                    }
                }
            }));
        }

        private void dgCell_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                }
            }));
        }

        private void dgCell_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.D)
                {
                    C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
                    if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null &&
                        !grd.CurrentCell.Column.Name.Equals("NO") &&
                        grd.CurrentCell.Column.Name.IndexOf("_") < 0)
                    {
                        //남경 로직.. OP 사용 로직인지 확인 필요..
                        //DeleteBtnCall();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Released &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                        //{
                        bool bShowSlotNo = chkViewSlotNo.IsChecked.HasValue && (bool)chkViewSlotNo.IsChecked ? true : false;
                        ShowHideAllColumns(bShowSlotNo);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }));
        }

        private void tbCellID_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    txtCellId.IsReadOnly = false;
                    txtEl.IsReadOnly = false;
                    txtBeforeWeight.IsReadOnly = false;
                    txtAfterWeight.IsReadOnly = false;
                    txtHeader.IsReadOnly = false;
                    txtPosition.IsReadOnly = false;
                    txtJudge.IsReadOnly = true;
                    cboTrayLocation.IsEnabled = true;

                    btnSave.IsEnabled = true;
                    btnDelete.IsEnabled = true;

                    btnCheckElJudge.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoELWeight_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    // Display Set.
                    SetTrayDisplayList(new string[] { "EL_WEIGHT" });

                    // 조회
                    SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                    GetTrayInfo();


                    //rdoManual.IsEnabled = true;
                    //tblEl.Visibility = Visibility.Visible;
                    //txtEl.Visibility = Visibility.Visible;
                    //tblBeforeWeight.Visibility = Visibility.Visible;
                    //txtBeforeWeight.Visibility = Visibility.Visible;
                    //tblAfterWeight.Visibility = Visibility.Visible;
                    //txtAfterWeight.Visibility = Visibility.Visible;
                    //tblHeader.Visibility = Visibility.Visible;
                    //txtHeader.Visibility = Visibility.Visible;
                    //tblPosition.Visibility = Visibility.Visible;
                    //txtPosition.Visibility = Visibility.Visible;
                    //tblJudge.Visibility = Visibility.Visible;
                    //txtJudge.Visibility = Visibility.Visible;
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbCellIDRuleTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    wndCellIDRule = new ASSY003_007_CELLID_RULE();

                    wndCellIDRule.FrameOperation = FrameOperation;

                    if (wndCellIDRule != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = DvProductLot["LOTID"].GetString();

                        C1WindowExtension.SetParameters(wndCellIDRule, Parameters);

                        wndCellIDRule.Closed += new EventHandler(wndCellIDRule_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndCellIDRule.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellId_KeyUp(object sender, KeyEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            // 권한 없으면 Skip.
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                return;

            if (e.Key == Key.Enter) // && rdoAuto.IsChecked.HasValue && (bool)rdoAuto.IsChecked)
            {
                if (!CanCellScan())
                    return;


                if (SaveCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
        }

        private void txtCellId_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCellId == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCellId, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtEl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtEl.Text, false, -1))
                {
                    txtEl.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBeforeWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtBeforeWeight.Text, false, -1))
                {
                    txtBeforeWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtAfterWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtAfterWeight.Text, false, -1))
                {
                    txtAfterWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHeader_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtHeader.Text, false, -1))
                {
                    txtHeader.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPosition_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtJudge_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            if (!CanCellModify())
                return;

            if (SaveCell())
            {
                SearchTrayWindow();

                GetTrayInfo();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            if (!CanDelete())
                return;

            if (DeleteCell())
            {
                SearchTrayWindow();

                GetTrayInfo();
            }
        }

        private void btnDupDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDelete())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgDupList == null || dgDupList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                SearchTrayWindow(false, true);

                                GetTrayInfo();

                                GetDupLocList(Util.NVC(dtRow["CSTSLOT"]));
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutRangeDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                GetTrayInfo();
                                GetOutRangeCellList();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutRangeDelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        for (int i = 0; i < dgOutRangeList.Rows.Count; i++)
                        {
                            DeleteCell(Util.NVC(DataTableConverter.GetValue(dgOutRangeList.Rows[i].DataItem, "SUBLOTID")));
                        }

                        GetTrayInfo();
                        GetOutRangeCellList();

                        Util.MessageInfo("SFU1273");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Method]
        #region [Func]
        private void ShowCellInfoPopup(string sTrayID, string sOutLOTID, string sTrayQty)
        {
            try
            {
                this.ClearAllCellInfo();

                if (sTrayID.Equals("")) return;
                if (sOutLOTID.Equals("")) return;

                _TrayID = sTrayID;
                _TrayQty = sTrayQty;
                _OutLotID = sOutLOTID;

                grCellList.Visibility = Visibility.Visible;

                if (this.Parent != null && this.Parent.GetType() == typeof(ContentControl) &&
                    (this.Parent as ContentControl).Parent != null && (this.Parent as ContentControl).Parent.GetType() == typeof(Grid))
                {
                    UIElementCollection uiec = ((this.Parent as ContentControl).Parent as Grid).Children;

                    if (uiec != null)
                    {
                        foreach (System.Windows.UIElement child in uiec)
                        {
                            if (child.GetType() == typeof(Button))
                            {
                                if ((child as Button).Name.ToUpper().Equals("BTNCLOSE"))
                                {
                                    child.Visibility = Visibility.Collapsed;
                                }
                                break;
                            }
                        }
                    }
                }

                grdDupList.Visibility = Visibility.Collapsed;
                grdOutRangeList.Visibility = Visibility.Collapsed;

                // Slot No. 표시 처리
                //_ShowSlotNo = true;
                //if (chkViewSlotNo != null && chkViewSlotNo.IsChecked.HasValue)
                //    chkViewSlotNo.IsChecked = true; 

                chkViewSlotNo.Visibility = Visibility.Visible;


                ApplyPermissions();

                // 기본 Display 설정            
                if (rdoCellID != null && rdoCellID.IsChecked != null)
                {
                    rdoCellID.Checked -= rdoCellID_Checked;
                    rdoCellID.IsChecked = true;
                    rdoCellID.Checked += rdoCellID_Checked;
                }

                // 주액 DATA 관리 여부
                GetElDataMngtFlag();

                InitializeControlsPKG();

                SetTrayWindow();
                //setTryLoction();
                SetBasicInfo();

                ChangeMode("ALL");  // 컨트롤 모두 View 처리...


                // Tray Grid Set.
                InitializeGrid();

                SetCellInfo(true, false, true);

                if (_ShowSlotNo)
                {
                    ShowSlotNoColumns(true);
                }
                else
                    ShowSlotNoColumns(false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearAllCellInfo()
        {
            try
            {
                // 내부 사용 변수
                _TrayID = string.Empty;
                _TrayQty = string.Empty;
                _OutLotID = string.Empty;

                _ELUseYN = false;
                originColorMap = null;

                _ShowSlotNo = false;
                _OnlyView = false;

                // 주액 USL, LSL 기준정보
                _EL_WEIGHT_LSL = string.Empty;
                _EL_WEIGHT_USL = string.Empty;
                _EL_AFTER_WEIGHT_LSL = string.Empty;
                _EL_AFTER_WEIGHT_USL = string.Empty;
                _EL_BEFORE_WEIGHT_LSL = string.Empty;
                _EL_BEFORE_WEIGHT_USL = string.Empty;

                bExistLayOutInfo = false;
                bViewAll = false;

                TRAY_SHAPE.CELL_TYPE = string.Empty;
                TRAY_SHAPE.ROW_NUM = 0;
                TRAY_SHAPE.COL_NUM = 0;
                TRAY_SHAPE.EMPTY_SLOT = false;
                TRAY_SHAPE.ZIGZAG = false;
                TRAY_SHAPE.EMPTY_SLOT_LIST = null;
                TRAY_SHAPE.MERGE_START_COL_NUM = 0;
                TRAY_SHAPE.DISPLAY_LIST = null;
                TRAY_SHAPE.DISP_SEPARATOR = null;

                // 화면 Display Control
                Util.gridClear(dgCell);
                Util.gridClear(dgDupList);
                Util.gridClear(dgOutRangeList);

                tbxCellId.Text = "";
                txtLotId.Text = "";
                txtTrayId.Text = "";
                txtCellCnt.Text = "";
                txtDefaultWeight.Text = "";
                txtCellId.Text = "";
                txtEl.Text = "";
                tblElMinMax.Text = "";
                txtBeforeWeight.Text = "";
                tblBeforeWeightMinMax.Text = "";
                txtAfterWeight.Text = "";
                tblAfterWeightMinMax.Text = "";
                txtHeader.Text = "";
                txtPosition.Text = "";
                txtJudge.Text = "";


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrayWindow()
        {
            // Tray Layout 정보 조회.
            GetTrayLayoutInfo();
        }

        private void SetZigZagGridInfo()
        {
            try
            {
                if (dgCell == null)
                    return;

                int iRowNum = 1;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dgCell.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                            //iMergIdx++;
                        }
                    }

                    for (int idxRow = 0; idxRow < dgCell.Rows.Count; idxRow++)
                    {
                        // 빈 슬롯 번호 확인하여 Empty Slot 설정.
                        if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE", "EMPT_SLOT");
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC", "EMPT_SLOT");
                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                    !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                                {
                                    // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                        )
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                }
                            }
                        }
                        else
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                            {
                                // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                    Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                   )
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                            }
                        }


                        // Row Number 설정
                        if (Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))  // Empty slot
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), "");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), (iRowNum).ToString());

                                if (idxRow % 2 != 0 && idxRow != dgCell.Rows.Count - 1)
                                {
                                    iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                            else
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)  // A Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }


                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1) // B Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                        }


                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            iTmpIdx++;
                        }

                        dgCell.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetTrayDisplayList(string[] strList)
        {
            TRAY_SHAPE.DISPLAY_LIST = strList == null ? new string[] { "CELLID" } : strList;
        }

        public void SetCellInfo(bool bLoad, bool bSameLoc, bool bChgNexRow)
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                ShowLoadingIndicator();

                int iRow = 0;
                int iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;

                if (!bLoad)
                    GetNextCellPos(out iRow, out iCol, bSameLoc: bSameLoc, bChgNexRow: bChgNexRow);

                ClearCellInfo();

                // Cell List 조회.
                DataTable dtResult = GetTrayCellList();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (!Util.NVC(dtResult.Rows[i]["LOCATION"]).Equals(""))
                        {
                            int iTmpLoc = 0;
                            if (int.TryParse(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iTmpLoc))
                            {
                                // Grid 내에 해당 Location 좌표 조회
                                int iFndRow, iFndCol;
                                string sViewColName;

                                //FindLocXY(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);
                                FindLocXYByLinq(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);

                                if (!sViewColName.Equals("") &&
                                    dgCell.Columns.Contains(sViewColName) &&
                                    dgCell.Columns.Contains(sViewColName + "_JUDGE") &&
                                    iFndRow > -1)
                                {
                                    // OK 가 아닌 경우에는 DATA SET 후 화면 색 표시 처리.
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("") ||
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("OK"))
                                    {
                                        string sTmpDisp = ""; // Util.NVC(dtResult.Rows[i]["CELLID"]); 


                                        if (TRAY_SHAPE.DISPLAY_LIST != null)
                                        {
                                            for (int iDsp = 0; iDsp < TRAY_SHAPE.DISPLAY_LIST.Length; iDsp++)
                                            {
                                                if (dtResult.Columns.Contains(Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])))
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                }
                                                else
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (dtResult.Columns.Contains("CELLID"))
                                                sTmpDisp = Util.NVC(dtResult.Rows[i]["CELLID"]);
                                        }

                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName, sTmpDisp);
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > iFndRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName, sTmpDisp);
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!bLoad)
                {
                    DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();
                }

                if (dgCell.Rows.Count > iRow && dgCell.Columns.Count > iCol && iRow > -1 && iCol > -1)
                {
                    Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                    dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);
                    dgCell.ScrollIntoView(iRow, iCol);

                    if (Util.NVC(dgCell.Columns[iCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[iCol].Name).Equals("NO") &&
                        dgCell.Columns.Contains(Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"))
                    {
                        //loadcellpresenter 콜을 위해 itemsouce 다시 set 시 current cell 오류로.. index로 직접 콜 하도록 변경.
                        string sCell = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_CELLID")); //dgCell.GetCell(iRow, iCol).Value == null ? "" : dgCell.GetCell(iRow, iCol).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"));

                        // 첫 Row 가 Empty slot 이면 Current Cell 1번 설정.
                        if (iRow == 0 && sLoc.Equals("EMPT_SLOT"))
                        {
                            int iFirstRow = Util.gridFindDataRow(ref dgCell, dgCell.Columns[iCol].Name + "_SLOTNO", "1", true);

                            if (iFirstRow > 0)
                            {
                                iRow = iFirstRow;
                                Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                                dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);
                                dgCell.ScrollIntoView(iRow, iCol);
                            }
                        }

                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetCellInfo(sCell.Equals(sLoc) ? "" : sCell, iRow, iCol, sLoc);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetNextCellPos(out int iRow, out int iCol, bool bSameLoc = false, bool bChgNexRow = true)
        {
            if (dgCell.CurrentCell != null)
            {
                if (dgCell.CurrentCell.Row != null)
                {
                    if (bSameLoc)   // 동일 로케이션 
                    {
                        iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index;
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    }
                    else
                    {
                        string sTmpColName = Util.NVC(dgCell.CurrentCell.Column.Name);

                        if (sTmpColName.IndexOf("_") < 0 && !sTmpColName.Equals("NO") && dgCell.Columns.Contains(sTmpColName + "_LOC"))
                        {
                            string sPrvLoc = Util.NVC(DataTableConverter.GetValue(dgCell.CurrentCell.Row.DataItem, sTmpColName + "_LOC"));

                            int iTmp = 0;
                            //int iFndRow, iFndCol;
                            string sViewColName;

                            int.TryParse(sPrvLoc, out iTmp);

                            //FindLocXY((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);
                            FindLocXYByLinq((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);

                            if (!sViewColName.Equals(""))
                                iCol = dgCell.Columns[sViewColName].Index < 0 ? iCol : dgCell.Columns[sViewColName].Index;

                            // 동일 Row 유지 확인
                            if (!bChgNexRow)
                            {
                                int iTmpPrvRow = 0;
                                int iTmpPrvCol = 0;
                                string sTmpViewColName = "";

                                FindLocXYByLinq((iTmp).ToString(), out iTmpPrvRow, out iTmpPrvCol, out sTmpViewColName);

                                iRow = iTmpPrvRow;
                            }
                        }
                        else
                        {
                            iRow = 0;
                            iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;
                        }
                    }
                }
                else
                {
                    iRow = 0;

                    if (dgCell.CurrentCell.Column != null)
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    else
                        iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
                }
            }
            else
            {
                iRow = 0;
                iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
            }
        }

        private void ClearCellInfo()
        {
            try
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                        {
                            if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC") &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")));
                            else
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_JUDGE") >= 0)  // 판정 컬럼 초기화
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name))).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_CELLID") >= 0)  // CELL ID 컬럼 초기화
                        {
                            DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                    }
                }

                //ParentClearInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBasicInfo()
        {
            txtLotId.Text = DvProductLot["LOTID"].GetString();
            txtTrayId.Text = _TrayID;

            GetTrayInfo();

            GetOutRangeCellList();

            // 주액 MIN MAX 관리 안하기로 하여 주석..
            //GetElJudgeInfo();
        }

        private void SetOriginColorMap()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (dgCell == null) return;

                //C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                originColorMap = new Brush[dgCell.Rows.Count, dgCell.Columns.Count];

                for (int i = 0; i < dgCell.Rows.Count; i++)
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                        if (dgCell[i, j].Presenter != null)
                            originColorMap[i, j] = dgCell[i, j].Presenter.Background;
            })
            );
        }

        private void FindLocXYByLinq(string sFindText, out int iFndRow, out int iFndCol, out string sViewColName)
        {
            iFndRow = -1;
            iFndCol = -1;
            sViewColName = "";

            DataTable dt = ((DataView)dgCell.ItemsSource).Table;
            DataRow row;

            for (int col = 0; col < dgCell.Columns.Count; col++)
            {
                if (Util.NVC(dgCell.Columns[col].Name).IndexOf("_LOC") < 0) continue;

                row = (from t in dt.AsEnumerable()
                       where (t.Field<string>(Util.NVC(dgCell.Columns[col].Name)) == sFindText)
                       select t).FirstOrDefault();

                if (row != null)
                {
                    //idx = dt.Rows.IndexOf(row) + 1;
                    iFndRow = dt.Rows.IndexOf(row);
                    iFndCol = dgCell.Columns[col].Index;

                    sViewColName = Util.NVC(dgCell.Columns[col].Name).Replace("_LOC", "");
                    return;
                }
            }
        }

        private void ChangeMode(string sMode)
        {
            if (sMode.Equals("ALL"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = false;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;
            }
            else if (sMode.Equals("AUTO"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = true;
            }
            else
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;
            }


            if (!bExistLayOutInfo)
            {
                SetLayoutNone();
            }
            else
            {
                //txtCellId.IsReadOnly = false;
                //txtEl.IsReadOnly = false;
                //txtBeforeWeight.IsReadOnly = false;
                //txtAfterWeight.IsReadOnly = false;
                //txtHeader.IsReadOnly = false;
                //txtPosition.IsReadOnly = false;
                //txtJudge.IsReadOnly = true;
                //cboTrayLocation.IsEnabled = true;

                btnSave.IsEnabled = true;
                btnDelete.IsEnabled = true;

                btnCheckElJudge.IsEnabled = true;
            }
        }

        public void SetLayoutNone()
        {
            try
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;

                btnSave.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnOutRangeDelAll.IsEnabled = false;

                btnCheckElJudge.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckLocationDup()
        {
            try
            {
                if (cboTrayLocation == null || cboTrayLocation.SelectedValue == null)
                    return;

                DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

                if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0") || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("1"))
                {
                    grdDupList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdDupList.Visibility = Visibility.Visible;
                    grdOutRangeList.Visibility = Visibility.Collapsed;

                    GetDupLocList(cboTrayLocation.SelectedValue.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchTrayWindow(bool bLoad = false, bool bSameLoc = false, bool bChgNexRow = true)
        {
            SetCellInfo(bLoad, bSameLoc, bChgNexRow);
        }

        private void ClearControl()
        {
            txtDefaultWeight.Text = "";
            txtCellId.Text = "";
            txtEl.Text = "";
            txtBeforeWeight.Text = "";
            txtAfterWeight.Text = "";
        }

        public void ShowSlotNoColumns(bool bShowSlotNo)
        {
            try
            {
                if (dgCell == null)
                    return;

                bool bFirsNo = true;
                for (int i = 0; i < dgCell.Columns.Count; i++)
                {
                    if (dgCell.Columns[i].Name.IndexOf("_SLOTNO") >= 0)
                    {
                        if (bFirsNo)
                        {
                            dgCell.Columns[i].Visibility = Visibility.Visible;
                            bFirsNo = false;
                        }
                        else
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns[i].Visibility = Visibility.Visible;

                                if (dgCell.Columns.Contains("NO"))
                                    dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;

                                if (dgCell.Columns.Contains("NO"))
                                    dgCell.Columns["NO"].Visibility = Visibility.Collapsed; //Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowHideAllColumns(bool bShowSlotNo)
        {
            if (dgCell == null)
                return;

            if (bViewAll)
                bViewAll = false;
            else
                bViewAll = true;

            for (int i = 0; i < dgCell.Columns.Count; i++)
            {
                if (bViewAll)
                {
                    dgCell.Columns[i].Visibility = Visibility.Visible;
                }
                else
                {
                    if (Util.NVC(dgCell.Columns[i].Name).Length > 2)
                    {
                        if (dgCell.Columns.Contains("NO"))
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Visible;
                            }
                        }

                        if (dgCell.Columns[i].Name.IndexOf("_SLOTNO") >= 0)
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            if (dgCell.Columns[i].Visibility == Visibility.Visible)
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            else if (dgCell.Columns[i].Visibility == Visibility.Collapsed)
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void wndCellIDRule_Closed(object sender, EventArgs e)
        {
            ASSY003_007_CELLID_RULE window = sender as ASSY003_007_CELLID_RULE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            wndCellIDRule = null;
        }

        private bool ChkDouble(string str, bool bUseMin, double dMinValue)
        {
            try
            {
                bool bRet = false;

                if (str.Trim().Equals(""))
                    return bRet;

                if (str.Trim().Equals("-"))
                    return true;

                double value;
                if (!double.TryParse(str, out value))
                {
                    //숫자필드에 부적절한 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2914");
                    return bRet;
                }
                if (bUseMin && value < dMinValue)
                {
                    //숫자필드에 허용되지 않는 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2915");
                    return bRet;
                }

                bRet = true;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public static void SetTrayShape(string sCellType, int iRowCnt, int iColCnt,
                                        bool bEmptySlot, bool bZigZag, string[] emptySlotList,
                                        int iMergeStartCol = 0, string[] displayList = null, char[] dispSeparator = null)
        {
            TRAY_SHAPE.CELL_TYPE = sCellType;  // CELL TYPE
            TRAY_SHAPE.ROW_NUM = bZigZag ? iRowCnt * 2 : iRowCnt;  // 총 ROW 수 (zigzag이면 Merge를 위해 2배로 생성)
            TRAY_SHAPE.COL_NUM = iColCnt;  // 총 COL 수
            TRAY_SHAPE.EMPTY_SLOT = bEmptySlot;  // 빈 슬롯 존재 여부
            TRAY_SHAPE.ZIGZAG = bZigZag;  // COL 별 지그재그 배치 여부
            TRAY_SHAPE.EMPTY_SLOT_LIST = emptySlotList;  // 빈 슬롯 번호 LIST
            TRAY_SHAPE.MERGE_START_COL_NUM = iMergeStartCol;    // 머지 시작 컬럼 번호.
            TRAY_SHAPE.DISPLAY_LIST = displayList == null ? new string[] { "CELLID" } : displayList;  // Cell 영역에 표시할 Data List
            TRAY_SHAPE.DISP_SEPARATOR = dispSeparator == null ? new char[] { ',' } : dispSeparator; // 표시 영역 구분자
        }
        #endregion

        #region [BizRule]
        private void GetTrayLayoutInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TrayID.Length > 4 ? _TrayID.Substring(0, 4) : _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CST_LAYOUT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bExistLayOutInfo = true;

                    int iRowNum = Util.NVC(dtRslt.Rows[0]["ROW_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_NUM"]));
                    int iColNum = Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]));
                    string sRowTypeCode = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]);
                    string[] sEmptySlotList = Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    int iRowMergeStrtCol = Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]));
                    string[] sDispInfo = Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    char[] cDispDelimeter = Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).Equals("") ? null : Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).ToCharArray();
                    bool bEmptySlot = sEmptySlotList != null && sEmptySlotList.Length > 0 ? true : false;
                    bool bZigZag = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]).Equals("Z") ? true : false;

                    SetTrayShape(Util.NVC(dtRslt.Rows[0]["CST_CELL_QTY"]), iRowNum, iColNum, bEmptySlot, bZigZag, sEmptySlotList, iMergeStartCol: iRowMergeStrtCol, displayList: sDispInfo, dispSeparator: cDispDelimeter);
                }
                else
                {
                    // 남경인 경우 AREA 로 없으면 AREA 없이 CST_TYPE_CODE 코드로만 조회 후 첫번째 LAYOUT 표시 요청에 의한 코드 추가.
                    GetTrayLayoutInfoWithArea();

                    //bExistLayOutInfo = false;

                    //SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                    //// 데이터 오류 [캐리어 레이아웃 기준정보 누락 - PI팀에 데이터 확인 요청]
                    //Util.MessageValidation("SFU3630");                    
                }
            }
            catch (Exception ex)
            {
                bExistLayOutInfo = false;

                SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTrayLayoutInfoWithArea()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CST_TYPE_CODE"] = _TrayID.Length > 4 ? _TrayID.Substring(0, 4) : _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CST_LAYOUT_INFO_WITHOUT_AREAID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bExistLayOutInfo = true;

                    int iRowNum = Util.NVC(dtRslt.Rows[0]["ROW_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_NUM"]));
                    int iColNum = Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]));
                    string sRowTypeCode = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]);
                    string[] sEmptySlotList = Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    int iRowMergeStrtCol = Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]));
                    string[] sDispInfo = Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    char[] cDispDelimeter = Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).Equals("") ? null : Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).ToCharArray();
                    bool bEmptySlot = sEmptySlotList != null && sEmptySlotList.Length > 0 ? true : false;
                    bool bZigZag = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]).Equals("Z") ? true : false;

                    SetTrayShape(Util.NVC(dtRslt.Rows[0]["CST_CELL_QTY"]), iRowNum, iColNum, bEmptySlot, bZigZag, sEmptySlotList, iMergeStartCol: iRowMergeStrtCol, displayList: sDispInfo, dispSeparator: cDispDelimeter);
                }
                else
                {
                    bExistLayOutInfo = false;

                    SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                    // 데이터 오류 [캐리어 레이아웃 기준정보 누락 - PI팀에 데이터 확인 요청]
                    Util.MessageValidation("SFU3630");
                }
            }
            catch (Exception ex)
            {
                bExistLayOutInfo = false;

                SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetElDataMngtFlag()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EL_DATA_MNGT_FLAG_CL", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("EL_DATA_USE_YN") && Util.NVC(dtRslt.Rows[0]["EL_DATA_USE_YN"]).Equals("Y"))
                    {
                        _ELUseYN = true;
                    }
                    else
                    {
                        _ELUseYN = false;
                    }
                }
                else
                {
                    _ELUseYN = false;
                }
            }
            catch (Exception ex)
            {
                _ELUseYN = false;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        public DataTable GetTrayCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_TRAY_CELL_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_LIST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTrayInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            txtCellCnt.Text = Double.Parse(Util.NVC(searchResult.Rows[0]["CELLQTY"])).ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetOutRangeCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TrayID.Substring(0, 4);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_OF_RANGE_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutRangeList.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Collapsed;
                        }

                        //SetOriginColorMap();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetCellInfo(string sCellID, int iRow, int iCol, string sLoc = "")
        {
            try
            {
                ShowLoadingIndicator();

                txtCellId.Text = sCellID;

                // 주액 정보
                txtEl.Text = "";
                txtBeforeWeight.Text = "";
                txtAfterWeight.Text = "";
                txtHeader.Text = "";
                txtPosition.Text = "";
                txtJudge.Text = "";

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CELL_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;
                newRow["CELLID"] = sCellID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_INFO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtEl.Text = dtResult.Columns.Contains("EL_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_WEIGHT"]) : "";
                    txtBeforeWeight.Text = dtResult.Columns.Contains("EL_PRE_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_PRE_WEIGHT"]) : "";
                    txtAfterWeight.Text = dtResult.Columns.Contains("EL_AFTER_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_AFTER_WEIGHT"]) : "";
                    txtHeader.Text = dtResult.Columns.Contains("HEADER") ? Util.NVC(dtResult.Rows[0]["HEADER"]) : "";
                    txtPosition.Text = dtResult.Columns.Contains("EL_PSTN") ? Util.NVC(dtResult.Rows[0]["EL_PSTN"]) : "";
                    txtJudge.Text = dtResult.Columns.Contains("EL_JUDG_VALUE") ? Util.NVC(dtResult.Rows[0]["EL_JUDG_VALUE"]) : "";

                    int iLoc = 0;
                    int.TryParse(Util.NVC(dtResult.Rows[0]["LOCATION"]), out iLoc);

                    //if(cboTrayLocation.Items.Contains(iLoc))
                    //    cboTrayLocation.SelectedValue = iLoc;
                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        cboTrayLocation.SelectedIndex = iLoc - 1;

                        //if (winTray25 == null)
                        //    return;

                        //winTray25.SetNexPos(iLoc - 1);
                    }


                    // 해당 위치에 Cell 정보 조회 후 중복건인경우 중복 List View 처리.
                    CheckLocationDup();
                }
                else
                {
                    //if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    int iLoc = 0;

                    int.TryParse(sLoc, out iLoc);

                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        if (iLoc == 0)
                            cboTrayLocation.SelectedValue = 1;
                        else
                            cboTrayLocation.SelectedValue = iLoc;

                        //if (winTray25 == null)
                        //    return;

                        //winTray25.SetNexPos(iLoc);
                    }
                    //}
                    //else
                    //{
                    //    int iMaxRow = 25;

                    //    int iLoc = 0;
                    //    iLoc = iRow + (iCol > 1 ? iMaxRow : 0);

                    //    if (cboTrayLocation.Items.Count >= iLoc)
                    //    {
                    //        cboTrayLocation.SelectedIndex = iLoc;

                    //        //if (winTray25 == null)
                    //        //    return;

                    //        //winTray25.SetNexPos(iLoc);
                    //    }
                    //}

                    grdDupList.Visibility = Visibility.Collapsed;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetCellLocCount(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_SEL_TRAY_LOCATION_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_LOCATION_CNT", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                return dtRslt;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetDupLocList(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_SEL_DUP_TRAY_LOCATION_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DUP_TRAY_LOCATION_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgDupList.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool SaveCell()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

                inSublotTable.Rows.Add(newRow);

                if (_ELUseYN)
                {
                    DataTable inElTable = indataSet.Tables["IN_EL"];
                    newRow = inElTable.NewRow();
                    newRow["SUBLOTID"] = txtCellId.Text.Trim();
                    newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                    newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                    newRow["EL_WEIGHT"] = txtEl.Text.Trim();
                    newRow["EL_PSTN"] = txtPosition.Text.Trim();
                    newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();

                    inElTable.Rows.Add(newRow);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S_UI", "IN_EQP,IN_CST,IN_EL", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sRet = string.Empty;
                    string sMsg = string.Empty;

                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                    }
                    else
                    {
                        sRet = "NG";

                        if (dsRslt.Tables["OUTDATA"].Columns.Contains("NG_MSG"))
                            sMsg = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG"]).Trim();
                        else
                            sMsg = "";

                        //if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("SC"))
                        //{
                        //    sMsg = "SFU3049";  // 특수문자가 포함되어 있습니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NR"))
                        //{
                        //    sMsg = "SFU3050";    // 읽을 수 없습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("DL"))
                        //{
                        //    sMsg = "SFU3051";    // CELL ID 자리수가 잘못 되었습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("ID"))
                        //{
                        //    sMsg = "SFU3052";  // CELL ID 가 이미 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("PD"))
                        //{
                        //    sMsg = "SFU3053";   // 동일한 위치에 이미 CELL ID가 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NI"))
                        //{
                        //    sMsg = "SFU3054";  // 주액 정보가 없습니다.
                        //}
                        //else
                        //{
                        //    sMsg = "";
                        //} 

                    }

                    if (sRet.Equals("NG"))
                    {
                        //Util.Alert(sMsg);
                        Util.MessageValidation(sMsg);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool ChkTrayStatWait()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool DeleteCell(string sDelCell = "")
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DELETE_SUBLOT_CL();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["CSTID"] = _TrayID;
                newRow["SUBLOTID"] = sDelCell.Equals("") ? txtCellId.Text.Trim() : sDelCell;

                inSublotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);

                HiddenLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private void InputAutoLotWinding(string inputLot, string positionId)
        {
            try
            {
               const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;
                inInputTable.Rows.Add(newRow);

                //string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                //GetProductLot();
                GetMaterialInputList();
                ResetEquipmentTree();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PopupReplaceWinding()
        {
            CMM_WINDING_PAN_REPLACE popPanReplace = new CMM_WINDING_PAN_REPLACE { FrameOperation = FrameOperation };

            int idx = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");

            object[] parameters = new object[8];
            parameters[0] = EquipmentSegmentCode;
            parameters[1] = EquipmentCode;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "WIPSEQ"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_QTY"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
            parameters[6] = ProcessCode;
            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
            C1WindowExtension.SetParameters(popPanReplace, parameters);

            popPanReplace.Closed += popWindingReplace_Closed;

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Add(popPanReplace);
                    popPanReplace.BringToFront();
                    break;
                }
            }
        }

        private void popWindingReplace_Closed(object sender, EventArgs e)
        {
            CMM_WINDING_PAN_REPLACE pop = sender as CMM_WINDING_PAN_REPLACE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                GetMaterialInputList();
                ResetEquipmentTree();
            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }

            }
        }

        private void EqptRemainInputCancel()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inEqpTable = indataSet.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("SRCTYPE", typeof(string));
                inEqpTable.Columns.Add("IFMODE", typeof(string));
                inEqpTable.Columns.Add("EQPTID", typeof(string));
                inEqpTable.Columns.Add("USERID", typeof(string));
                inEqpTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("EQPT_REMAIN_PSTN", typeof(string));

                DataRow newRow = inEqpTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                if (!string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString(); 
                }
                inEqpTable.Rows.Add(newRow);

                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "INPUT_LOTID"));
                newRow["EQPT_REMAIN_PSTN"] = Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN"));
                inInputTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_EQPT_REMAIN_LOT_WN", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetMaterialInputList();
                    ResetEquipmentTree();

                    Util.MessageInfo("SFU1275");

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        protected virtual void ResetEquipmentTree()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetEquipmentTree");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



















        #endregion

        #region [Validation]
        private bool CanCellScan()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            //if (txtCellId.Text.Trim().Length != 10)
            //{
            //    //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
            //    Util.MessageValidation("SFU1318");
            //    return bRet;
            //}

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // 주액 저장시 신규 Cell ID 인지 수정 Cell ID 인지 구분 불가로 해당 validation 저장 Biz에서 처리 하도록 변경.
            //// Cell ID 특수문자 등 체크.            
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetSubLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    //Util.Alert(sMsg);
            //    Util.MessageValidation(sMsg);
            //    return bRet;
            //}
            //else if (sRet.Equals("EXCEPTION"))
            //    return bRet;

            //// 해당 Location 존재 여부 체크.            
            //DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            //if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            //{
            //    //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
            //    Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
            //    return bRet;
            //}


            bRet = true;
            return bRet;
        }

        private bool CanCellModify()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            //if (txtCellId.Text.Trim().Length != 10)
            //{
            //    //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
            //    Util.MessageValidation("SFU1318");
            //    return bRet;
            //}

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // 주액 저장시 신규 Cell ID 인지 수정 Cell ID 인지 구분 불가로 해당 validation 저장 Biz에서 처리 하도록 변경.
            //// Cell ID 특수문자 등 체크.            
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetSubLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    //Util.Alert(sMsg);
            //    Util.MessageValidation(sMsg);
            //    return bRet;
            //}
            //else if (sRet.Equals("EXCEPTION"))
            //    return bRet;

            //// 해당 Location 존재 여부 체크.            
            //DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            //if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            //{
            //    //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
            //    Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
            //    return bRet;
            //}


            // 주액 정보 Min ~ Max 값 확인
            if (_ELUseYN)
            {
                double dTmpLSL, dTmpUSL, dTmpInput;

                if (double.TryParse(_EL_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtEl.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4280"); // 입력한 주액 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }

                if (double.TryParse(_EL_BEFORE_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_BEFORE_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtBeforeWeight.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4281"); // 입력한 주액전 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }

                if (double.TryParse(_EL_AFTER_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_AFTER_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtAfterWeight.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4282"); // 입력한 주액후 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }


                // 주액량 정보입력 여부 체크.
                if (txtEl.Text.Trim().Equals("") || (double.TryParse(txtEl.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4451"); // 주액량 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }

                if (txtBeforeWeight.Text.Trim().Equals("") || (double.TryParse(txtBeforeWeight.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4452"); // 주액전 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }

                if (txtAfterWeight.Text.Trim().Equals("") || (double.TryParse(txtAfterWeight.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4453"); // 주액후 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }


                //Header 길이 체크.
                if (txtPosition.Text.Trim().Equals("") || txtPosition.Text.Trim().Length > 1)
                {
                    Util.MessageValidation("SFU4450"); // 해더 정보는 1자리로 입력하세요.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanDelete()
        {
            bool bRet = false;

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // CELL 확정 여부 체크.

            // 해당 Location 존재 여부 체크.            
            DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            {
                //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 없습니다.", cboTrayLocation.SelectedValue.ToString());
                Util.MessageValidation("SFU2031", cboTrayLocation.SelectedValue.ToString());
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanDeleteOutRange()
        {
            bool bRet = false;

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #endregion
        #endregion













        #region[[Validation]

        private bool ValidationMaterialInputReplace()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            //C20210913-000199
            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN")) == "Y")
            {
                Util.MessageValidation("SFU3805");
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return false;
            }

            if(DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()) )
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefectZZS()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectZZS))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return false;
            }

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefectReInput()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectReInput))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Lot 정보가 없습니다
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }

        private bool ValidationCreateTray()
        {
            if (string.IsNullOrEmpty(txtTrayIDInline.Text.Trim()) || txtTrayIDInline.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675");
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtTrayIDInline.Text.ToUpper(), @"[^a-zA-Z0-9가-힣]");

            if (chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtTrayIDInline.Text.ToUpper());
                return false;
            }

            return true;
        }

        private bool ValidationCreateTrayWashing()
        {
            if(DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(txtTrayID.Text.Trim()) || txtTrayID.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675");
                txtTrayID.SelectAll();
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtTrayID.Text.ToUpper(), @"^[a-zA-Z0-9]+$");
            if (!chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtTrayID.Text.ToUpper());
                txtTrayID.SelectAll();
                return false;
            }

            return true;
        }

        private bool ValidationTrayConfirm(C1DataGrid dg)
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (CommonVerify.HasDataGridRow(dg))
            {
                DataTable dt = ((DataView)dg.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int64>("CHK") == 1
                                 select t).ToList();

                if (queryEdit.Any())
                {
                    foreach (var item in queryEdit)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                        {
                            Util.MessageValidation("SFU3616");
                            return false;
                        }

                        // 확정 여부 확인
                        if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                        {
                            Util.MessageValidation("SFU1235");
                            return false;
                        }

                        if (item["LOCATION_NG"].GetString() == "NG")
                        {
                            Util.MessageValidation("SFU3638");
                            return false;
                        }

                        double dTmp;
                        if (double.TryParse(Util.NVC(item["CELLQTY"]), out dTmp))
                        {
                            if (dTmp.Equals(0))
                            {
                                //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                                Util.MessageValidation("SFU1685");
                                return false;
                            }
                        }
                        else
                        {
                            //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                            Util.MessageValidation("SFU1687");
                            return false;
                        }

                        string returnMessage;
                        string messageCode;

                        // Tray 현재 작업중인지 여부 확인.
                        GetTrayInfo(dg, out returnMessage, out messageCode);

                        if (returnMessage.Equals("NG"))
                        {
                            Util.MessageValidation(messageCode);
                            return false;
                        }
                        else if (returnMessage.Equals("EXCEPTION"))
                            return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidationConfirmCancel(C1DataGrid dg)
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    //"선택된 작업대상이 없습니다."
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }


                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int64>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 확정취소 할 수 없습니다.
                                Util.MessageValidation("SFU3617");
                                return false;
                            }

                            // 확정 여부 확인
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정 상태만 확정 취소할 수 있습니다.
                                Util.MessageValidation("SFU3618");
                                return false;
                            }
                        }
                    }
                    return true;
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

        private bool ValidationDischarge(C1DataGrid dg)
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    //"선택된 작업대상이 없습니다."
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int64>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 Tray만 배출가능 합니다.
                                Util.MessageValidation("SFU3614");
                                return false;
                            }

                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건이 존재합니다.
                                Util.MessageValidation("SFU3620");
                                return false;
                            }

                            if (!string.Equals(Util.NVC(item["LOCATION_NG"]).GetString(), "OK"))
                            {
                                //투입위치 정보를 확인 하세요.
                                Util.MessageValidation("SFU1980");
                                return false;
                            }
                        }
                    }
                    return true;
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

        private bool ValidationRemoveTray(C1DataGrid dg)
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    //"선택된 작업대상이 없습니다."
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int64>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 삭제할 수 없습니다.
                                Util.MessageValidation("SFU3619");
                                return false;
                            }

                            //// 확정 여부 확인
                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 건은 삭제하실 수 없습니다.
                                Util.MessageValidation("SFU3621");
                                return false;
                            }
                        }
                    }
                    return true;
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

        private bool ValidationTrayDelete(C1DataGrid dg)
        {

            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
                {
                    //"선택된 작업대상이 없습니다."
                    Util.MessageValidation("SFU1645");
                    return false;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");

                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }
                // TODO 선택된 W/O가 없습니다.
                //DataRow selectedWorkOrderRow = GetSelectWorkOrderRow();
                //if (selectedWorkOrderRow == null)
                //{
                //    //Util.Alert("선택된 W/O가 없습니다.");
                //    Util.MessageValidation("SFU1635");
                //    return false;
                //}

                if (CommonVerify.HasDataGridRow(dg))
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<Int64>("CHK") == 1
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        foreach (var item in queryEdit)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(item["LOTDTTM_OT"]).GetString()))
                            {
                                //배출완료된 건은 삭제할 수 없습니다.
                                Util.MessageValidation("SFU3619");
                                return false;
                            }

                            if (string.Equals(Util.NVC(item["PROC_TRAY_CNFM_FLAG"]).GetString(), "Y"))
                            {
                                //확정된 건은 삭제하실 수 없습니다
                                Util.MessageValidation("SFU3621");
                                return false;
                            }
                        }
                    }
                    return true;
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

        private bool ValidationSpecialTraySave()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (chkOutTraySpl.IsChecked.HasValue && (bool)chkOutTraySpl.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return false;
                }

                if (txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return false;
                }

                ///[C20190415_74474] 추가
                if (LoginInfo.CFG_SHOP_ID.Equals("A010") && txtRegPerson.Text.Trim().Equals(""))
                {
                    //Util.Alert("특별Tray 담당자를 선택하셔야 합니다.");
                    Util.MessageValidation("SFU6042");
                    return false;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationTraySearch()
        {
            
            if (string.IsNullOrEmpty(EquipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationTrayMove()
        {
            
            if (string.IsNullOrEmpty(EquipmentSegmentCode))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }
            
            if (string.IsNullOrEmpty(EquipmentCode))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }
            
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationTrayCreate()
        {

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayDelete()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayConfirmCancel()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayConfirm()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }


            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            //TODO : 작업자
            //if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            //{
            //    //작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1842");
            //    return false;
            //}
            if(string.IsNullOrEmpty(WorkerId))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }


            DataTable dt = ((DataView)dgOut.ItemsSource).Table;
            var queryEdit = (from t in dt.AsEnumerable()
                             where t.Field<Int64>("CHK") == 1
                             select t).ToList();
            if (queryEdit.Any())
            {
                foreach (var item in queryEdit)
                {
                    if (item["TransactionFlag"].GetString() == "Y")
                    {
                        //변경된 데이터가 존재합니다.\r\n먼저 저장한 후 다시 시도하세요.
                        Util.MessageValidation("SFU4038");
                        return false;
                    }
                }
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")), out dTmp))
            {
                if (dTmp.Equals(0))
                {
                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                    Util.MessageValidation("SFU1685");
                    return false;
                }
            }
            else
            {
                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                Util.MessageValidation("SFU1687");
                return false;
            }

            string returnMessage;
            string messageCode;

            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfo(out returnMessage, out messageCode);

            if (returnMessage.Equals("NG"))
            {
                Util.MessageValidation(messageCode);
                return false;
            }
            else if (returnMessage.Equals("EXCEPTION"))
                return false;

            return true;
        }

        private bool ValidationCellChange()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            if (_util.GetDataGridCheckCnt(dgOut, "CHK") > 1)
            {
                Util.MessageValidation("SFU3719", ObjectDic.Instance.GetObjectName("CELL관리"));
                return false;
            }

            if(string.IsNullOrEmpty(WorkerId))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            //TODO : 작업자 입력 Validation
            //if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            //{
            //    //작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1845");
            //    return false;
            //}

            return true;
        }

        private bool ValidationTraySave()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationCellMatched()
        {
            DataSet dsResult;
            DataTable dtResult;
            bool matchFlag = false;

            const string bizRuleName = "BR_PRD_CHK_TRAY_CELL_MATCH";

            DataSet inDataSet = new DataSet();

            DataTable inData = inDataSet.Tables.Add("INDATA");
            inData.Columns.Add("PROD_LOTID", typeof(string));
            inData.Columns.Add("OUT_LOTID", typeof(string));
            inData.Columns.Add("CSTID", typeof(string));
            inData.Columns.Add("OUTPUT_QTY", typeof(decimal));

            foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;

                DataRow dr = inData.NewRow();

                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "OUT_LOTID").GetString();
                dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "TRAYID").GetString();
                dr["OUTPUT_QTY"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetString()) ? 0 : DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetDecimal();

                inData.Rows.Add(dr);
            }

            dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA", inDataSet);

            dtResult = dsResult.Tables["OUTDATA"];

            if (dtResult.Rows.Count > 0)
            {
                if (dtResult.Rows[0]["MATCHED"].ToString() == "Y")
                {
                    matchFlag = true;
                }
            }

            if (matchFlag)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ValidationTrayCellQtyCode()
        {
            double dCellQty = 0;
            string returnmessageCode = string.Empty;
            foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                {
                    string cellQty = DataTableConverter.GetValue(row.DataItem, "CELLQTY").GetString();
                    if (!string.IsNullOrEmpty(cellQty))
                        double.TryParse(cellQty, out dCellQty);

                    if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                    {
                        return "SFU1320";
                    }
                }
            }

            return returnmessageCode;
        }

        private bool ValidationQualitySave()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgQuality))
            {
                //Data가 존재하지 않습니다.
                Util.MessageValidation("SFU1331");
                return false;
            }

            return true;
        }

        private bool ValidationBoxCreate()
        {
            if(DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(WorkerId))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(ShiftId))
            {
                //작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }

            return true;
        }

        private bool ValidationBoxInput()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK", true);
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgBox.Rows[rowIndex].DataItem, "LOTID").GetString()))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationBoxDelete()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgBox))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationBoxPrint()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgBox, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgBox))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }
        
        private bool isFixedSpclTrayByDemandType()
        {
            //int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            //string demandType = DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "DEMAND_TYPE").ToString();
            string demandType = DvProductLot["DEMAND_TYPE"].GetString();

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "SPCL_TRAY_DEMAND_TYPE";
            inTable.Rows.Add(dr);
            DataTable resultTable = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            return resultTable.AsEnumerable().Any(row => demandType.Equals(row.Field<String>("CBO_CODE")));
        }

        private bool IsCheckedOnDataGrid()
        {
            if (CommonVerify.HasDataGridRow(dgProdCellWinding))
            {
                DataTable dt = ((DataView)dgProdCellWinding.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int32>("CHK") == 1
                                 select t).ToList();
                if (queryEdit.Any())
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private bool IsCheckedOnDataGridWashing()
        {
            if (CommonVerify.HasDataGridRow(dgOut))
            {
                DataTable dt = ((DataView)dgOut.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int32>("CHK") == 1
                                 select t).ToList();
                if (queryEdit.Any())
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

        private bool CanWaitBoxInPut()
        {
            bool bRet = false;

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return bRet;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgWaitBox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 패키지 바구니 투입 시 복수개의 PROD가 존재하는 경우 처리 되도록 변경.
            if (cboBoxMountPstsID == null || cboBoxMountPstsID.SelectedValue == null || cboBoxMountPstsID.SelectedIndex < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            //int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboBoxMountPstsID.SelectedValue.ToString());
            //if (iRow < 0)
            //{
            //    //Util.Alert("자재 투입 위치에 존재하지 않는 투입 위치 입니다.");
            //    Util.MessageValidation("SFU1819");
            //    return bRet;
            //}

            //if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            //{
            //    //Util.Alert("[{0}] 위치에 RUN 상태의 바구니가 존재 합니다.\n완료 처리 후 투입 하십시오.", cboBoxMountPstsID.Text);
            //    Util.MessageValidation("SFU1281", cboBoxMountPstsID.Text);
            //    return bRet;
            //}

            // 패키지 공정인 경우 최근 LOT에만 투입 처리 가능
            // 마지막 PROD LOT에만 투입 가능하도록 처리.
            if (string.Equals(ProcessCode, Process.PACKAGING))
            {
                string sSelProd = DvProductLot["LOTID"].GetString();
                string sNowProd = GetNowRunProdLot();
                if (!sNowProd.Equals("") && sSelProd != sNowProd)
                {
                    //Util.Alert("선택한 조립LOT({0})은 마지막 작업중인 LOT이 아닙니다.\n마지막 작업중인 LOT({1})에만 투입할 수 있습니다.", sSelProd, sNowProd);
                    object[] parameters = new object[2];
                    parameters[0] = sSelProd;
                    parameters[1] = sNowProd;
                    Util.MessageValidation("SFU1666", parameters);
                    return false;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanInBoxInputCancel(C1.WPF.DataGrid.C1DataGrid dg)
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool CanCreateTrayByTrayID()
        {
            bool bRet = false;

            string trayID = txtTrayIDPKG.Text.ToUpper();
            //정규식표현 false=>영문과숫자이외문자열비허용
            bool chk = System.Text.RegularExpressions.Regex.IsMatch(trayID, @"^[a-zA-Z0-9]+$");

            if (!chk)
            {
                //Util.Alert("입력한 ID ({0}) 에 특수문자가 존재하여 생성할 수 없습니다.", trayID);
                Util.MessageValidation("SFU1811", trayID);
                txtTrayID.Text = "";
                return bRet;
            }
            if (!bCellTraceMode && (txtTrayQtyPKG.Text.Equals("0") || txtTrayQtyPKG.Text.Trim().Length < 1))
            {
                //Util.Alert("수량을 입력하세요.");
                Util.MessageValidation("SFU1684");
                txtTrayQtyPKG.Focus();
                return bRet;
            }

            if (chkOutTraySpl.IsChecked.HasValue && (bool)chkOutTraySpl.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("특이사항을 입력 하세요.");
                    Util.MessageValidation("SFU1992");
                    return bRet;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("특이사항을 삭제 하세요.");
                    Util.MessageValidation("SFU1991");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayDelete()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanConfirmCancel()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayConfirm()
        {
            bool bRet = false;

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");

            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return bRet;
            }

            //if (((DataRowView)dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem).Row.RowState == DataRowState.Modified)
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "CELLQTY")).Equals(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "HIDDEN_CELLQTY"))))
            {
                Util.MessageValidation("SFU4038");   // 변경된 데이터가 존재합니다. 먼저 저장한 후 다시 시도하세요.
                return bRet;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "CELLQTY")), out dTmp))
            {
                if (dTmp == 0)
                {
                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                    Util.MessageValidation("SFU1685");
                    return bRet;
                }
            }
            else
            {
                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                Util.MessageValidation("SFU1687");
                return bRet;
            }

            string sRet = string.Empty;
            string sMsg = string.Empty;
            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfoPKG(out sRet, out sMsg);
            if (sRet.Equals("NG"))
            {
                Util.MessageValidation(sMsg);
                return bRet;
            }
            else if (sRet.Equals("EXCEPTION"))
                return bRet;

            bRet = true;
            return bRet;
        }

        private bool CanChangeCell()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSaveTray()
        {
            bool bRet = false;

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayMove()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                Util.MessageValidation("SFU3721"); // Tray 이동이 가능한 상태가 아닙니다.
                return bRet;
            }

            //if (((DataRowView)dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem).Row.RowState == DataRowState.Modified)
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "CELLQTY")).Equals(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "HIDDEN_CELLQTY"))))
            {
                Util.MessageValidation("SFU4038");   // 변경된 데이터가 존재합니다. 먼저 저장한 후 다시 시도하세요.
                return bRet;
            }

            double iCellCnt = 0;
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOutPKG.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPKG, "CHK")].DataItem, "CELLQTY")), out iCellCnt);

            if (iCellCnt < 1)
            {
                Util.MessageValidation("SFU3063"); // 수량이 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanOutTraySplSave()
        {
            bool bRet = false;

            if (chkOutTraySplPKG.IsChecked.HasValue && (bool)chkOutTraySplPKG.IsChecked)
            {
                if (cboOutTraySplReasonPKG.SelectedValue == null || cboOutTraySplReasonPKG.SelectedValue == null || cboOutTraySplReasonPKG.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtOutTrayReamrkPKG.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return bRet;
                }
            }
            else
            {
                if (!txtOutTrayReamrkPKG.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool ValidationAutoInputLot()
        {
            if (string.IsNullOrEmpty(txtMaterialInputLotID.Text.Trim()))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return false;
            }

            for (int i = 0; i < dgMaterialInput.Rows.Count - dgMaterialInput.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtMaterialInputLotID.Text.Trim()))
                    {
                        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
                        return false;
                    }
                }
            }

            //C20210913-000199
            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN")) == "Y")
            {
                Util.MessageValidation("SFU3805");
                return false;
            }

            return true;
        }

        private bool ValidationEqptRemainUnmount()
        {
            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridFirstRowIndexByCheck(dgMaterialInput, "CHK")].DataItem, "EQPT_REMAIN_PSTN")) != "Y")
            {
                Util.MessageValidation("SFU3807");  //설비잔량투입이 아닙니다.
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[_util.GetDataGridCheckFirstRowIndex(dgMaterialInput, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                Util.MessageValidation("SFU1945");  //투입 LOT이 없습니다.
                return false;
            }

            return true;
        }

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_Start        
        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveQuality(dgQualityZtz);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

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
                inData["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inData["EQPTID"] = Util.NVC(EquipmentCode);
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();

                inData["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            }
            return IndataTable;
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemark(dgRemark)) return;

            DataTable dt = DataTableConverter.Convert(dgRemark.ItemsSource);
            if (dt != null)
            {
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                        SaveWipNote(dgRemark);
                    else
                        return;
                });
            }
            else
            {
                SaveWipNote(dgRemark);
            }
        }

        private bool ValidationRemark(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3552");     // 저장 할 DATA가 없습니다.
                return false;
            }

            return true;
        }
        

        private bool IsCommoncodeUse(string sCodeType, string sCmCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = sCmCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void dgQualityZtz_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            
            C1DataGrid dg = sender as C1DataGrid;

            if (dg != null)
            {
                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
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
                                //string sCSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSL"));
                                //string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                //string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));

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

                                        if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
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
                                    }
                                }
                            }
                            else if (string.Equals(e.Cell.Column.Name, "MEAN"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }     

                        //dg.Columns["MEAN"].Visibility = Visibility.Visible;
                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                            if (e.Cell.Column.Index == dg.Columns["CLSS_NAME1"].Index)
                            {
                                if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                            }
                            else if (e.Cell.Column.Index == dg.Columns["CLCTVAL01"].Index) // 측정값
                            {
                                decimal sumValue = 0;
                                if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                                    if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非数字"))
                                    {
                                        foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));

                                        if (sumValue == 0)
                                            presenter.Content = 0;
                                        else
                                            presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dg.Rows.Count - dg.BottomRows.Count), "EA"));
                                    }
                            }
                        }
                    }
                }));
            }
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

        private void dgQualityZtz_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void SaveWipNote(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow inData = null;
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    inData = inTable.NewRow();

                    inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                    if (dg.Rows[0].Visibility == Visibility.Visible)
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                    else
                        inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                    inData["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(inData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    _isChangeRemark = false;
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }           

        }

        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 3)
            {
                Grid grid = e.Cell.Presenter.Content as Grid;

                if (grid != null)
                {
                    TextBox remarkText = grid.Children[0] as TextBox;

                    if (remarkText != null)
                    {
                        remarkText.LostKeyboardFocus -= OnRemarkLostKeyboardFocus;
                        remarkText.LostKeyboardFocus += OnRemarkLostKeyboardFocus;
                    }
                }
            }
            else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 3)
            {
                Grid grid = e.Cell.Presenter.Content as Grid;

                if (grid != null)
                {
                    TextBox remarkText = grid.Children[0] as TextBox;

                    if (remarkText != null)
                    {
                        remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                        remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;

                        if (Util.NVC(_ProcWipnoteGroupUseFlag).Equals("Y"))
                        {
                            if (Util.NVC(Convert.ToString(dgRemark.GetCell(e.Cell.Row.Index, dgRemark.Columns["REMARK_TYPE"].Index).Value)).Equals("ETC"))
                            {
                                remarkText.IsReadOnly = false;
                            }
                            else
                            {
                                remarkText.IsReadOnly = true;
                            }
                        }

                    }
                }
            }
        }

        private void SetParentQtyZtz()
        {
            if (string.Equals(ProcessCode, Process.ZTZ))
            {
                txtParentQty.Value = Convert.ToDouble(DvProductLot["INPUTQTY"]).GetDouble();
                SetParentRemainQty();
            }
        }

        private void btnProductionUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductionUpdate()) return;

            if (textInputQtyZtz.Value >= 0)
            {
                if (Convert.ToDouble(textInputQtyZtz.Value) > Convert.ToDouble(txtParentQty.Value))
                {
                    Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                    return;
                }                

                SaveProductionUpdate();
                CalculateDefectQty();
                SetDefectDetailZtz();
            }
            else
            {
                //수량은 0 보다 커야 합니다.
                Util.MessageValidation("100057");
                return;
            }
        }

        private void SetDefectDetailZtz()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;
            decimal defectQty = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = inputQtyOrg; //Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectDetail.Rows[2].DataItem, "OUTPUTQTY"));
            defectQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectDetail.Rows[2].DataItem, "DEFECTQTY"));

            DataTableConverter.SetValue(dgDefectDetail.Rows[2].DataItem, "OUTPUTQTY", inputQty);
            DataTableConverter.SetValue(dgDefectDetail.Rows[2].DataItem, "GOODQTY", inputQty - defectQty);

            txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty));
        }


        private void SaveProductionUpdate()
        {
            try
            {
                // 작업조, 작업자
                DataRow[] drShift = DtEquipment.Select("EQPTID = '" + EquipmentCode + "' And SEQ = 2");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                inTable.Columns.Add("LANE_QTY", typeof(decimal));
                inTable.Columns.Add("PROD_QTY", typeof(decimal));
                inTable.Columns.Add("SRS1QTY", typeof(decimal));
                inTable.Columns.Add("SRS2QTY", typeof(decimal));
                inTable.Columns.Add("SRS3QTY", typeof(decimal));
                inTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["PROD_VER_CODE"] = textProjectName != null ? Util.NVC(textProjectName.Text) : null;
                newRow["SHIFT"] = string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()) ? null : drShift[0]["SHFT_ID"].ToString();
                newRow["WIPNOTE"] = null;
                newRow["WRK_USER_NAME"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["VAL002"].ToString();
                newRow["WRK_USERID"] = string.IsNullOrWhiteSpace(drShift[0]["WRK_USERID"].ToString()) ? null : drShift[0]["WRK_USERID"].ToString();
                newRow["PROD_QTY"] = inputQtyOrg;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SaveDefect(dgDefectZtz, true);

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
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

        public void SaveDefect(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);
                
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable InResn = inDataSet.Tables.Add("INRESN");
                InResn.Columns.Add("LOTID", typeof(string));
                InResn.Columns.Add("WIPSEQ", typeof(Int32));
                InResn.Columns.Add("ACTID", typeof(string));
                InResn.Columns.Add("RESNCODE", typeof(string));
                InResn.Columns.Add("RESNQTY", typeof(double));
                InResn.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                InResn.Columns.Add("COST_CNTR_ID", typeof(string));
                InResn.Columns.Add("WRK_COUNT", typeof(Int16));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable dtDefect = DataTableConverter.Convert(dg.ItemsSource);

                foreach (DataRow row in dtDefect.Rows)
                {
                    newRow = InResn.NewRow();
                    newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                    newRow["WIPSEQ"] = Util.NVC(DvProductLot["WIPSEQ"]);
                    newRow["ACTID"] = row["ACTID"];
                    newRow["RESNCODE"] = row["RESNCODE"];
                    newRow["RESNQTY"] = row["RESNQTY"].ToString().Equals("") ? 0 : row["RESNQTY"];
                    //newRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(row["DFCT_TAG_QTY"])) ? 0 : row["DFCT_TAG_QTY"];
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = txtLaneQty.Value;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;
                    //newRow["COST_CNTR_ID"] = row["COSTCENTERID"];
                    newRow["COST_CNTR_ID"] = null;
                    //newRow["WRK_COUNT"] = row["COUNTQTY"].ToString() == "" ? DBNull.Value : row["COUNTQTY"];
                    newRow["WRK_COUNT"] = 0;

                    InResn.Rows.Add(newRow);
                }

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                    dg.EndEdit(true);

                    _isChangeWipReason = false;
                }
                catch (Exception ex) { Util.MessageException(ex); }

                if (!bAllSave)
                    Util.MessageInfo("SFU3532");     // 저장 되었습니다
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool CheckConfirmLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && !string.Equals(ProcessCode, dtResult.Rows[0]["PROCID"]) && (string.Equals(INOUT_TYPE.IN, dtResult.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dtResult.Rows[0]["WIP_TYPE_CODE"])))
                {
                    Util.MessageValidation("SFU5066");     // 이미 실적 확정 된 LOT입니다.
                    return false;
                }

            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        private bool ValidationProductionUpdate()
        {
            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (CheckConfirmLot() == false) return false;

            return true;
        }

        private string GetProcWipnoteGroupUseFlag()
        {

            string sProcWipnoteGroupUseFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = { ProcessCode };

            sCodeType = "PROC_WIPNOTE_GROUP_USE_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sProcWipnoteGroupUseFlag = "Y";  // Util.NVC(dtResult.Rows[0]["ATTR1"].ToString())
                }
                else
                {
                    sProcWipnoteGroupUseFlag = "N";
                }

                return sProcWipnoteGroupUseFlag;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return sProcWipnoteGroupUseFlag;
            }
        }

        private void SetWipNoteHoldReason()
        {
            _ProcWipnoteGroupUseFlag = GetProcWipnoteGroupUseFlag();

            if (Util.NVC(_ProcWipnoteGroupUseFlag).Equals("Y"))
            {
                SetdgRemarkGridCboItem(dgRemark.Columns["REMARK_TYPE"]);  // Hold Reason Remark Combo Setting 

                dgRemark.Columns["REMARK_TYPE"].Visibility = Visibility.Visible;
                dgRemark.BeginningEdit += dgRemark_BeginningEdit;
            }
        }

        private void dgRemark_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (Util.NVC(_ProcWipnoteGroupUseFlag).Equals("Y"))
            {
                if (e.Column == this.dgRemark.Columns["REMARK_TYPE"])
                {
                    _HoldReasonValue = Convert.ToString(dgRemark.CurrentCell.Value);
                }
            }
        }

        private void SetdgRemarkGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                string sClassId = "PROC_STD_REMARK_ELTR";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");

                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }
                else
                {
                    AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
            {
                _isChangeRemark = true;
            }
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1) return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                _isChangeRemark = true;
        }

        private void dgRemark_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null)
                _isChangeRemark = true;

            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (e.Cell.Column.Name.Contains("POST_HOLD"))
                {
                    if (e.Cell.Row.Index.Equals(0))
                    {
                        DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
                        DataRow dr = dt.Select().FirstOrDefault();
                        string sColumnName = e.Cell.Column.Name;
                        bool hold = Convert.ToBoolean(dr[sColumnName]);
                        if (dr != null)
                        {
                            foreach (DataRow dRow in dt.Rows)
                            {
                                dRow[sColumnName] = hold;
                            }
                        }
                        Util.GridSetData(dgRemark, dt, FrameOperation);
                    }
                }

                if (Util.NVC(_ProcWipnoteGroupUseFlag).Equals("Y"))
                {

                    if (e.Cell.Column.Name.Contains("REMARK_TYPE"))
                    {
                        string sHoldReason = "";
                        string sProcStdRemark = "";

                        sHoldReason = Convert.ToString(dgRemark.CurrentCell.Value);

                        if (!string.Equals(_HoldReasonValue, sHoldReason))
                        {
                            switch (sHoldReason)
                            {
                                case "SELECT":
                                    dgRemark.Refresh();
                                    break;

                                case "ETC":
                                    DataTableConverter.SetValue(dgRemark.Rows[dgRemark.CurrentCell.Row.Index].DataItem, "REMARK", string.Empty);
                                    dgRemark.Refresh();
                                    break;

                                default:
                                    sProcStdRemark = GetProcStdRemarkEltr(sHoldReason);
                                    DataTableConverter.SetValue(dgRemark.Rows[dgRemark.CurrentCell.Row.Index].DataItem, "REMARK", sProcStdRemark);
                                    dgRemark.Refresh();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private string GetProcStdRemarkEltr(string sCmCode)
        {

            string sProcStdRemarkEltr = "";
            string sCodeType;
            string[] sAttribute = null;

            sCodeType = "PROC_STD_REMARK_ELTR";

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCmCode;
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sProcStdRemarkEltr = Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());

                }
                else
                {
                    sProcStdRemarkEltr = "";
                }

                return sProcStdRemarkEltr;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return sProcStdRemarkEltr;
            }
        }

        private void dgDefectZtz_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null) return;

            try
            {
                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["RESNQTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumDefectZtzQty().GetInt();
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
        }

        private void dgDefectZtz_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgDefectZtz_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private decimal GetSumDefectZtzQty()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectZtz))
                return 0;

            DataTable dt = ((DataView)dgDefectZtz.ItemsSource).Table;
            decimal defectqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("DEFECT_LOT") && !w.Field<string>("RSLT_EXCL_FLAG").Equals("Y")).Sum(s => s.Field<decimal>("RESNQTY"));
            decimal lossqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("LOSS_LOT")).Sum(s => s.Field<decimal>("RESNQTY"));
            decimal chargeprdqty = dt.AsEnumerable().Where(w => w.Field<string>("ACTID").Equals("CHARGE_PROD_LOT")).Sum(s => s.Field<decimal>("RESNQTY"));

            return defectqty + lossqty + chargeprdqty;
        }

        public void GetDefectInfoZtz(DataRowView rowview)
        {
            if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;
            const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";

            try
            {
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    dgDefectZtz.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }

                if (string.Equals(ProcessCode, Process.ZTZ))
                {
                    CalculateDefectQty();

                    //잔량
                    decimal parentQty = Convert.ToDecimal(DvProductLot["INPUTQTY"]);
                    decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectDetail.Rows[2].DataItem, "OUTPUTQTY"));
                    txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty));
                }
                else if (string.Equals(ProcessCode, Process.WASHING))
                {
                    GetOutTraybyAsync();
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetQualityInfoZtzList()
        {
            try
            {
                DataTable _topDT = new DataTable();
                DataTable _backDT = new DataTable();

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                Util.gridClear(dgQualityZtz);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = ProcessCode;
                Indata["LOTID"] = DvProductLot["LOTID"].GetString();
                Indata["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                Indata["CLCT_PONT_CODE"] = ProcessCode.Equals(Process.COATING) || ProcessCode.Equals(Process.INS_COATING) ? "T" : null;

                IndataTable.Rows.Add(Indata);

                _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                if (_topDT.Rows.Count == 0)
                {
                    _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(dgQualityZtz, _topDT, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgQualityZtz, _topDT, FrameOperation, true);
                }                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dgDefectZtz_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            // 이벤트 없음
        }

        private void SelectRemark()
        {
            try
            {
                DataTable dtRemark = new DataTable();
                dtRemark.Columns.Add("LOTID", typeof(String));
                dtRemark.Columns.Add("REMARK", typeof(String));

                DataRow newRow = dtRemark.NewRow();
                newRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

                string[] WipNote = GetRemarkData(Util.NVC(DvProductLot["LOTID"])).Split('|');
                if (WipNote.Length > 1)
                    newRow["REMARK"] = WipNote[1];

                dtRemark.Rows.Add(newRow);

                newRow = dtRemark.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["REMARK"] = GetRemarkData(Util.NVC(DvProductLot["LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(newRow);

                Util.GridSetData(dgRemark, dtRemark, FrameOperation);

                dgRemark.Rows[0].Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetRemarkData(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = sLotID;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dtResult.Rows.Count > 0)
            {
                return Util.NVC(dtResult.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }

        private void btnDefectSaveZtz_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefectZtz()) return;
            SetDefectZtz();
        }

        private bool ValidationSaveDefectZtz()
        {
            if (!CommonVerify.HasDataGridRow(dgDefectZtz))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return false;
            }

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool IsFinalProcess()
        {
            // 현재 작업중인 LOT이 마지막 공정인지 체크
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = Util.NVC(DvProductLot["LOTID_PR"]);
            indata["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
            indata["PROCID"] = ProcessCode;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                return true;

            return false;
        }

        private void textInputQtyZtz_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isChangeInputFocus == false && textInputQtyZtz.Value > 0)
                textInputQtyZtz_KeyDown(textInputQtyZtz, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        private void textInputQtyZtz_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _isChangeInputFocus = true;

                if (Convert.ToDouble(textInputQtyZtz.Value) > Convert.ToDouble(txtParentQty.Value))
                {
                    Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                    return;
                }
                else if (Convert.ToDouble(textInputQtyZtz.Value) < 0)
                {
                    //수량은 0 보다 커야 합니다.
                    Util.MessageValidation("100057");
                    return;
                }
                else
                {
                    SetInputQty();
                }
                _isChangeInputFocus = false;
            }
        }

        private void SetInputQty()
        {
            decimal inputQty = Util.NVC_Decimal(textInputQtyZtz.Value);
            decimal lossQty = 0;
     
            for (int i = 0 + dgDefectDetail.TopRows.Count; i < (dgDefectDetail.Rows.Count - dgDefectDetail.BottomRows.Count); i++)
            {
                lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectDetail.Rows[i].DataItem, "DEFECTQTY"));

                DataTableConverter.SetValue(dgDefectDetail.Rows[i].DataItem, "OUTPUTQTY", inputQty);
                DataTableConverter.SetValue(dgDefectDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);                
            }
            SetParentRemainQty();
        }

        private void SetDefectZtz()
        {
            try
            {
                dgDefectZtz.EndEdit();

                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefectZtz.Rows.Count - dgDefectZtz.BottomRows.Count; i++)
                {
                    newRow = null;
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectZtz.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }
                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
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

                        GetDefectInfoZtz(DvProductLot);
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        IsProductLotRefreshFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HiddenLoadingIndicator();
            }
        }
        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_End
        #endregion

        #region [팝업]

        #endregion

        #endregion


    }
}
