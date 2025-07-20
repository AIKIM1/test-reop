/*************************************************************************************
 Created Date : 2019.10.15
      Creator : INS 김동일K
   Decription : CWA 전극 생산실적 자동화 - 슬리터 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.15  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.DateTimeEditors;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Models;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ELEC002
{
    /// <summary>
    /// ELEC002_004.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC002_004 : UserControl, IWorkArea
    {
        #region Properties
        string _PROCID = string.Empty;

        string _ValueWOID = string.Empty;
        string _LOTID = string.Empty;
        string _EQPTID = string.Empty;
        string _LOTIDPR = string.Empty;
        string _CUTID = string.Empty;
        string _WIPSTAT = string.Empty;
        string _INPUTQTY = string.Empty;
        string _OUTPUTQTY = string.Empty;
        string _CTRLQTY = string.Empty;
        string _GOODQTY = string.Empty;
        string _LOSSQTY = string.Empty;
        string _CHARGEQTY = string.Empty;
        string _WORKORDER = string.Empty;
        string _WORKDATE = string.Empty;
        string _VERSION = string.Empty;
        string _PRODID = string.Empty;
        string _WIPDTTM_ST = string.Empty;
        string _WIPDTTM_ED = string.Empty;
        //string _REMARK = string.Empty;
        string _CONFIRMUSER = string.Empty;
        string _FINALCUT = string.Empty;
        string _WIPSTAT_NAME = string.Empty;
        string _LANEQTY = string.Empty;
        string sEQPTID = string.Empty;
        string cut = string.Empty;
        string _PTNQTY = string.Empty;
        string _WIPSEQ = string.Empty;
        string _CLCTSEQ = string.Empty;
        string _WRKDTTM_ST = string.Empty;
        string _WRKDTTM_ED = string.Empty;
        string _HOLDYN_CHK = string.Empty;
        string _CSTID = string.Empty;
        string _CSTID_CHK = string.Empty;

        decimal inputOverrate;
        decimal exceedLengthQty;
        decimal convRate;

        bool isChangeColorTag = false;
        bool isChangeQuality = false;
        bool isChangeMaterial = false;
        bool isChagneDefectTag = false;
        bool isChangeRemark = false;
        bool isChangeCotton = false;
        bool isChangeInputFocus = false;
        bool isDupplicatePopup = false;
        bool isResnCountUse = false;
        bool isDefectLevel = false;
        bool isConfirm = false;
        
        bool isManualWorkMode = false;
        
        TextBox txtWipNote;
        
        C1NumericBox txtLanePatternQty = new C1NumericBox();
        
        DataSet inDataSet;
        DataTable procResnDt;

        DataTable dtWoAll;
        DataTable dtWoCheck;

        DataTable _DT_OUT_PRODUCT;
        DataTable _dtDEFECTLANENOT = null; // 전수불량 Lane
        DataRow[] _DEFECTLANELOT = null;  // 전수불량 Lane

        int dgLVIndex1 = 0;
        int dgLVIndex2 = 0;
        int dgLVIndex3 = 0;

        GridLength ExpandFrame;

        private DataTable dtWipReasonBak;       // WIPREASONCOLLECT의 이전값을 보관하기 위한 DataTable(C20190404_67447) [2019-04-11]

        private Util _Util = new Util();

        bool isRefresh;
        public bool REFRESH
        {
            get
            {
                return isRefresh;
            }
            set
            {
                isRefresh = value;

                if (isRefresh)
                {
                    if (isConfirm && _DT_OUT_PRODUCT.Rows.Count > 0)
                    {
                        txtEndLotId.Text = Util.NVC(_DT_OUT_PRODUCT.Rows[0]["LOTID"]);
                        txtEndLotId.Tag = _CUTID;

                        // INOUT LOT이 수량 연계되는 문제가 발생할 여지가 있어서 저장된 수량 초기화 [2017-04-22]
                        SetInitProductQty();

                        string confirmMsg = string.Empty;   // 실적확정 INFORM MESSAGE                   
                        confirmMsg += string.IsNullOrEmpty(confirmMsg) ? MessageDic.Instance.GetMessage("SFU1275") : ("\n\n" + MessageDic.Instance.GetMessage("SFU1275"));

                        #region [샘플링 출하거래처 추가]
                        // SAMPLING용 발행 매수 추가
                        int iSamplingCount;

                        if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                        {
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                foreach (DataRow _iRow in _DT_OUT_PRODUCT.Rows)
                                {
                                    iSamplingCount = 0;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), _PROCID, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                        }
                        #endregion                        

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(confirmMsg, null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                        {
                            if (vResult == MessageBoxResult.OK)
                            {
                                if (!string.Equals(_PROCID, Process.PRE_MIXING))
                                {
                                    if (string.Equals(LoginInfo.CFG_CARD_POPUP, "Y") || string.Equals(LoginInfo.CFG_CARD_AUTO, "Y"))
                                        OnClickHistoryCard(null, null);
                                }
                            }
                        });
                        isConfirm = false;
                    }
                    if (chkWait.Visibility == Visibility.Collapsed || WIPSTATUS.Equals(Wip_State.WAIT))
                        chkRun.IsChecked = true;

                    Thread.Sleep(500);

                    GetProductLot();

                    // SHIFT_CODE, SHIFT_USER 자동 SET
                    GetWrkShftUser();                    
                }
            }
        }

        private const string ITEM_CODE_LEN_LACK = "LENGTH_LACK";
        private const string ITEM_CODE_LEN_EXCEED = "LENGTH_EXCEED";
        private const string ITEM_CODE_PROD_QTY_INCR = "PROD_QTY_INCR";

        public string LOTID { get; set; }
        public string PRLOTID { get; set; }
        public string WO_DETL_ID { get; set; }
        
        public string WIPSTATUS { get; set; }
        
        public C1DataGrid WORKORDER_GRID { get; set; }
        

        public string _LDR_LOT_IDENT_BAS_CODE { get; set; }
        public string _UNLDR_LOT_IDENT_BAS_CODE { get; set; }
        #endregion

        #region Initialize
        public ELEC002_004()
        {
            InitializeComponent();

            _PROCID = Process.SLITTING;

            InitControls();
        }

        void InitControls()
        {
            SetButtons();
            SetCheckBox();

            _DT_OUT_PRODUCT = new DataTable();
            _DT_OUT_PRODUCT.Columns.Add("LOTID", typeof(string));
            _DT_OUT_PRODUCT.Columns.Add("OUT_CSTID", typeof(string));
            _DT_OUT_PRODUCT.Columns.Add("PR_LOTID", typeof(string));
            _DT_OUT_PRODUCT.Columns.Add("CSTID", typeof(string));
            _DT_OUT_PRODUCT.Columns.Add("CUT_ID", typeof(string));
            _DT_OUT_PRODUCT.Columns.Add("LANE_QTY", typeof(decimal));
            _DT_OUT_PRODUCT.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            _DT_OUT_PRODUCT.Columns.Add("LANE_NUM", typeof(decimal));
        }

        public IFrameOperation FrameOperation { get; set; }
        public DataTable WIPCOLORLEGEND { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            #region 초기화
            SetComboBox();
            SetGrids();
            SetTabItems();
            //SetResultDetail();
            SetTextBox();
            SetEvents();
            SetStatus();
            //SetSingleCoaterControl();
            SetVisible();
            WipReasonGridColumnAdd();
            SetIdentInfo();

            grdWorkOrder.Children.Clear();
            grdWorkOrder.Children.Add(new UC_WORKORDER_CWA());

            // 슬리터 공정 Port별 Skid Type 설정 팝업추가
            // 적용대상 동은 CNB 전극1동, CWA 전극2동
            if (LoginInfo.CFG_AREA_ID.Equals("EC") || LoginInfo.CFG_AREA_ID.Equals("ED"))
            {
                btnSkidTypeSettingByPort.Visibility = Visibility.Visible;
            }

            // 하프슬리터도 변경 못하게 추가 [2017-05-23]
            // 해당 건 CWA요청으로 주석 [2018-12-20]
            //if (string.Equals(procId, Process.HALF_SLITTING))                    
            txtInputQty.IsEnabled = false;

            btnSamplingProdT1.Visibility = Visibility.Visible;

            chkWoProduct.Visibility = Visibility.Collapsed;

            // 투입량의 초과입력률 체크하기 위하여 추가
            string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));

            // 변환률 기본값 설정
            convRate = 1;

            // 롤프레스 평균값
            if (!string.Equals(_PROCID, Process.ROLL_PRESSING))
                dgQuality.BottomRows.Clear();

            // 슬리터 기본 설정 (소형 체크로직을 Setting에서 불러와서 사용)
            if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                if (LoginInfo.CFG_ETC != null && LoginInfo.CFG_ETC.Rows.Count > 0)
                    chkSum.IsChecked = Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_SMALLTYPE]);

            #endregion

            SetApplyPermissions();
        }

        private void InitClear()
        {
            // INIT VARIABLE
            _ValueWOID = string.Empty;
            _LOTID = string.Empty;
            _EQPTID = string.Empty;
            _LOTIDPR = string.Empty;
            _CUTID = string.Empty;
            _WIPSTAT = string.Empty;
            _INPUTQTY = string.Empty;
            _OUTPUTQTY = string.Empty;
            _CTRLQTY = string.Empty;
            _GOODQTY = string.Empty;
            _LOSSQTY = string.Empty;
            _WORKORDER = string.Empty;
            _WORKDATE = string.Empty;
            _VERSION = string.Empty;
            _PRODID = string.Empty;
            _WIPDTTM_ST = string.Empty;
            _WIPDTTM_ED = string.Empty;
            //_REMARK = string.Empty;
            _CONFIRMUSER = string.Empty;
            sEQPTID = string.Empty;
            _FINALCUT = string.Empty;
            _WIPSTAT_NAME = string.Empty;
            _LANEQTY = string.Empty;
            _PTNQTY = string.Empty;
            exceedLengthQty = 0;
            convRate = 1;
            cut = "Y";  // Cut

            InitTextBox();

            isChangeQuality = false;
            isChangeMaterial = false;
            isChangeColorTag = false;
            isChagneDefectTag = false;
            isChangeRemark = false;
            isChangeCotton = false;

            cboLaneNum.Items.Clear();

            _DT_OUT_PRODUCT.Clear();


            btnSaveWipReason.IsEnabled = false;
            btnPublicWipSave.IsEnabled = false;
            btnSaveRegDefectLane.IsEnabled = false;
            //btnProcResn.IsEnabled = false;
        }

        private void InitWorkOrderCheck()
        {
            chkWoProduct.IsChecked = false;
            dtWoAll = null;
            dtWoCheck = null;
        }

        private void InitTextBox()
        {
            txtUnit.Text = string.Empty;
            txtInputQty.Value = 0;

            if (txtParentQty != null)
            {
                txtParentQty.Value = 0;
                txtRemainQty.Value = 0;
            }

            if (txtVersion != null)
                txtVersion.Text = string.Empty;

            if (txtLaneQty != null)
                txtLaneQty.Value = 0;
            #region # 전수불량 Lane  등록
            if (txtCurLaneQty != null)
                txtCurLaneQty.Value = 0;
            #endregion
            if (txtLanePatternQty != null)
                txtLanePatternQty.Value = 0;

            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;

            if (txtWorkDate != null)
                txtWorkDate.Text = string.Empty;

            if (txtWipNote != null)
                txtWipNote.Text = string.Empty;

            txtMergeInputLot.Text = string.Empty;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

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
        #endregion

        #region Event
        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            switch (cb.Name)
            {
                case "chkWait":
                    if (cb.IsChecked == true)
                    {
                        chkWoProduct.Visibility = Visibility.Visible;

                        chkRun.IsChecked = false;
                        chkEqpEnd.IsChecked = false;
                        chkConfirm.IsChecked = false;
                    }
                    else
                    {
                        chkWoProduct.Visibility = Visibility.Collapsed;

                        chkRun.IsChecked = true;
                        chkEqpEnd.IsChecked = true;
                        chkConfirm.IsChecked = true;
                    }
                    break;

                case "chkRun":
                case "chkEqpEnd":
                case "chkConfirm":
                    if (cb.IsChecked == true)
                    {
                        chkWoProduct.Visibility = Visibility.Collapsed;

                        chkWait.IsChecked = false;
                        chkRun.IsChecked = true;
                        chkEqpEnd.IsChecked = true;
                        chkConfirm.IsChecked = true;
                    }
                    else
                    {
                        chkWoProduct.Visibility = Visibility.Visible;

                        chkWait.IsChecked = true;
                        chkRun.IsChecked = false;
                        chkEqpEnd.IsChecked = false;
                        chkConfirm.IsChecked = false;
                    }
                    break;

                case "chkWoProduct":
                    if (cb.IsChecked == true)
                    {
                        if (dgProductLot.Rows.Count < 1)
                            return;

                        try
                        {
                            string prodId = (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).PRODID;
                            
                            dtWoAll = (dgProductLot.ItemsSource as DataView).Table;
                            dtWoCheck = (dgProductLot.ItemsSource as DataView).Table.Select("PRODID='" + prodId + "'").CopyToDataTable();
                            Util.GridSetData(dgProductLot, dtWoCheck, FrameOperation, true);
                            
                        }
                        catch
                        {
                            dgProductLot.ItemsSource = null;                            
                        }
                    }
                    else
                    {
                        if (dtWoAll != null)
                        {
                            Util.GridSetData(dgProductLot, dtWoAll, FrameOperation, true);
                        }
                    }
                    break;
            }

            if (!cb.Name.Equals("chkWoProduct"))
            {
                SetStatus();

                if (string.Equals(cb.Name, "chkEqpEnd"))    // PROC, EQPT_END가 동시에 체크되기 떄문에 하나만 체크하도록 변경
                    return;

                if (((string.Equals(WIPSTATUS, Wip_State.WAIT) || string.Equals(WIPSTATUS, Wip_State.PROC + "," + Wip_State.EQPT_END + "," + Wip_State.END)) && cb.IsChecked == true))
                {
                    OnClickSearch(btnSearch, null);
                }
            }

            if (cb.Name.Equals("chkWait"))
                chkWoProduct.IsChecked = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkOrder_chk())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _PROCID);
            dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            dicParam.Add("EQSGID", Util.NVC(cboEquipmentSegment.SelectedValue));
            if (new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") != -1)
                dicParam.Add("RUNLOT", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID_PR")));
            dicParam.Add("COAT_SIDE_TYPE", "");

            ELEC002_LOTSTART _LotStart = new ELEC002_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;

            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC002_LOTSTART _LotStart = sender as ELEC002_LOTSTART;

            if (_LotStart.DialogResult == MessageBoxResult.OK)
                RunProcess(_LotStart._ReturnLotID);
        }

        private void OnClickSearch(object sender, RoutedEventArgs e)
        {
            try
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

                if (string.IsNullOrEmpty(WIPSTATUS))
                {
                    Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                InitClear();
                InitWorkOrderCheck();

                loadingIndicator.Visibility = Visibility.Visible;

                GetWorkOrder();

                GetProductLot();

                // WAIT 상태 일때는 수량 입력 금지
                if (string.Equals(WIPSTATUS, Wip_State.WAIT))
                {
                    chkWoProduct.IsChecked = true;
                }

                // SHIFT_CODE, SHIFT_USER 자동 SET
                GetWrkShftUser();

                // 자동선택 추가
                SetLotAutoSelected();
            }
            catch (Exception ex) { Util.MessageException(ex); }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void OnClickVersion(object sender, EventArgs e)
        {
            CMM_ELECRECIPE wndPopup = new CMM_ELECRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null && dgProductLot.SelectedIndex > -1 && _DT_OUT_PRODUCT.Rows.Count > 0)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "PRODID"));
                Parameters[1] = _PROCID;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "LOTID"));
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseVersion);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseVersion(object sender, EventArgs e)
        {
            CMM_ELECRECIPE window = sender as CMM_ELECRECIPE;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (Util.NVC_Decimal(txtLaneQty.Value) != Util.NVC_Decimal(window._ReturnLaneQty))
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(window._ReturnLaneQty);
                    txtLanePatternQty.Value = Convert.ToDouble(window._ReturnPtnQty);

                    if (dgWipReason.Rows.Count > 0)
                        SaveDefect(dgWipReason);

                    _LANEQTY = Util.NVC(txtLaneQty.Value);

                    #region #전수불량 Lane등록
                    txtCurLaneQty.Value = getCurrLaneQty(_LOTID, _PROCID);
                    #endregion
                }
                else
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(window._ReturnLaneQty);
                    txtLanePatternQty.Value = Convert.ToDouble(window._ReturnPtnQty);
                }

                if (_DT_OUT_PRODUCT.Rows.Count > 0)
                {
                    for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                    {
                        _DT_OUT_PRODUCT.Rows[i]["LANE_QTY"] = txtLaneQty.Value;
                        _DT_OUT_PRODUCT.Rows[i]["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                    }
                }
                GetSumDefectQty();
            }
        }

        private void OnKeyDownLaneQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_DT_OUT_PRODUCT.Rows.Count > 0)
                {
                    if (Util.NVC_Decimal(_LANEQTY) != Util.NVC_Decimal(txtLaneQty.Value))
                    {
                        if (dgWipReason.Rows.Count > 0)
                            SaveDefect(dgWipReason);                        

                        _LANEQTY = Util.NVC(txtLaneQty.Value);

                        for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                            _DT_OUT_PRODUCT.Rows[i]["LANE_QTY"] = txtLaneQty.Value;

                        GetSumDefectQty();
                    }
                }
            }
        }

        private void OnKeyLostFocusLaneQty(object sender, RoutedEventArgs e)
        {
            if (_DT_OUT_PRODUCT.Rows.Count > 0)
            {
                if (Util.NVC_Decimal(_LANEQTY) != Util.NVC_Decimal(txtLaneQty.Value))
                {
                    if (dgWipReason.Rows.Count > 0)
                        SaveDefect(dgWipReason);
                    
                    _LANEQTY = Util.NVC(txtLaneQty.Value);

                    for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                        _DT_OUT_PRODUCT.Rows[i]["LANE_QTY"] = txtLaneQty.Value;

                    GetSumDefectQty();
                }
            }
        }

        private void OnClickSaveColorTag(object sender, EventArgs e)
        {
            string status = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

            if (status.Equals(Wip_State.PROC) || status.Equals(Wip_State.EQPT_END) || status.Equals(Wip_State.END))
            {
                SaveQuality(dgColor);
                isChangeColorTag = false;
            }
        }

        private void OnExtraMouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void OnSelectedItemChangedEquipmentCombo(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataRowView drv = e.NewValue as DataRowView;

                if (drv != null)
                {
                    // 요청으로 설비 선택 시 초기화 추가 [2017-03-17]
                    ClearControls();
                    InitWorkOrderCheck();

                    Util.gridClear(((UC_WORKORDER_CWA)grdWorkOrder.Children[0]).dgWorkOrder);
                    
                    Util.gridClear(dgProductLot);

                    loadingIndicator.Visibility = Visibility.Visible;

                    // ON SEARCH
                    GetWorkOrder();
                    GetProductLot();

                    // SHIFT_CODE, SHIFT_USER 자동 SET
                    GetWrkShftUser();

                    SetIdentInfo();
                   
                    // 자동선택 추가
                    SetLotAutoSelected();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void OnGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {                
                case "dgProductLot":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
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

                                            ClearDetailControls();

                                            if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                                return;

                                            WIPSTATUS = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT"));
                                            LOTID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                                            if (_PROCID.Equals(Process.HALF_SLITTING))
                                                PRLOTID = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID_PR"));

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            if (!WIPSTATUS.Equals("WAIT"))
                                            {
                                                GetLotInfo(dgProductLot.Rows[e.Cell.Row.Index].DataItem);

                                                if (_DT_OUT_PRODUCT.Rows.Count == 1)
                                                    GetDefectList();
                                                else if (_DT_OUT_PRODUCT.Rows.Count > 1)
                                                    GetDefectListMultiLane();

                                                GetQualityList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);

                                                if (_PROCID.Equals(Process.SLITTING))
                                                {
                                                    GetCottonList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                                }

                                                // 코터 이후 설비에서는 BOTTOM ROW 추가 [2017-06-13]
                                                if (!string.Equals(_PROCID, Process.PRE_MIXING) && !_PROCID.Equals(Process.BS) && !_PROCID.Equals(Process.CMC) && !string.Equals(_PROCID, Process.MIXING) && !string.Equals(_PROCID, Process.SRS_MIXING))
                                                    if (string.Equals(txtUnit.Text, "EA") && dgLotInfo.BottomRows.Count == 0)
                                                        for (int i = dgLotInfo.TopRows.Count; i < (dgLotInfo.Rows.Count - dgLotInfo.BottomRows.Count); i++)
                                                            if (dgLotInfo.Rows[i].Visibility == Visibility.Visible)
                                                                dgLotInfo.BottomRows.Add(new C1.WPF.DataGrid.DataGridRow());
                                            }
                                            
                                            if (!_PROCID.Equals(Process.MIXING) && !_PROCID.Equals(Process.PRE_MIXING) && !_PROCID.Equals(Process.BS) && !_PROCID.Equals(Process.CMC) && !_PROCID.Equals(Process.SRS_MIXING))
                                                GetRemarkHistory(e.Cell.Row.Index);

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            ClearDefectLV();

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            ClearDetailControls();

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);

                                            ClearDefectLV();

                                        }
                                        break;
                                }

                                if (dgProductLot.CurrentCell != null)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                                else if (dgProductLot.Rows.Count > 0)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }

        private void OnClickDefetectFilter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            //Grid grdDefectLVFilter = grdDefectLVFilter as Grid;

            if (_DT_OUT_PRODUCT.Rows.Count < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                //CWA 불량등록 필터 그리드
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }
        }

        private void OnLaneChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                if (string.Equals(checkBox.Tag, "ALL"))
                {
                    foreach (CheckBox _checkBox in cboLaneNum.Items)
                        _checkBox.IsChecked = checkBox.IsChecked;
                }
                else
                {
                    SetVisibilityWipReasonGrid(Util.NVC(checkBox.Tag), checkBox.IsChecked);
                }
            }
        }

        private void OnLoadedDefectLaneCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            var _defectLane = new List<string>();
            foreach (DataRow row in _DEFECTLANELOT)
            {
                _defectLane.Add(Util.NVC(row["LOTID"]));
                _defectLane.Add(Util.NVC(row["LOTID"]) + "NUM");
                _defectLane.Add(Util.NVC(row["LOTID"]) + "CNT");
                _defectLane.Add(Util.NVC(row["LOTID"]) + "RESN_TOT_CHK");
                _defectLane.Add(Util.NVC(row["LOTID"]) + "FRST_AUTO_RSLT_RESNQTY");
            }
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME") && !Util.NVC(e.Cell.Column.Name).Equals("RESNNAME"))
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                    }
                                    else
                                    {
                                        if (_defectLane.Contains(e.Cell.Column.Name) || _defectLane.Contains(e.Cell.Column.Name + "NUM") || _defectLane.Contains(e.Cell.Column.Name + "CNT"))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                        }
                                        else if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT") &&
                                                 Util.NVC(e.Cell.Column.Name).EndsWith("RESN_TOT_CHK"))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                            //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                        }
                                        else
                                        {
                                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                            //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                        }
                                    }

                                    // 길이부족 차감 색상표시 추가 [2019-12-09]
                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRCS_ITEM_CODE")).Equals("PROD_QTY_INCR"))
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D6606D"));

                                }
                            }
                            // 전체불량의 경우 ALL Column ReadOnly
                            if (_dtDEFECTLANENOT.Rows.Count == _DEFECTLANELOT.Length)
                                if (string.Equals(e.Cell.Column.Name, "ALL"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                        }
                    }
                }));
            }
        }

        private void OnClickBarcode(object sender, RoutedEventArgs e)
        {
            // 상태 관계없이 OUTLOT 기준으로 BARCODE 발행 [2017-01-31]
            if (_DT_OUT_PRODUCT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1559");  //발행할 LOT을 선택하십시오.
                return;
            }
            
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                return;
            }

            DataTable printDT = GetPrintCount(_LOTID, _PROCID);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            #region [샘플링 출하거래처 추가]
                            // SAMPLING용 발행 매수 추가
                            int iSamplingCount;
                            
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                foreach (DataRow _iRow in _DT_OUT_PRODUCT.Rows)
                                {
                                    iSamplingCount = 0;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), _PROCID, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            #endregion
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    // S/L공정은 Default 라벨 발행 매수 포함해서 출력해달라고 자동차2동 요청 [2018-07-10]
                    if (string.Equals(_PROCID, Process.SLITTING))
                    {
                        #region [샘플링 출하거래처 추가]
                        // SAMPLING용 발행 매수 추가
                        int iSamplingCount;

                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            foreach (DataRow _iRow in _DT_OUT_PRODUCT.Rows)
                            {
                                iSamplingCount = 0;
                                string[] sCompany = null;
                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                {
                                    iSamplingCount = Util.NVC_Int(items.Key);
                                    sCompany = Util.NVC(items.Value).Split(',');
                                }

                                for (int i = 0; i < iSamplingCount; i++)
                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), _PROCID, i > sCompany.Length - 1 ? "" : sCompany[i]);
                            }
                        #endregion
                    }
                    else
                    {
                        foreach (DataRow _iRow in _DT_OUT_PRODUCT.Rows)
                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), _PROCID);
                    }
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void OnClickStartCancel(object sender, RoutedEventArgs e)
        {
            DataRow[] dr = Util.gridGetChecked(ref dgProductLot, "CHK");

            if (dr == null || dr.Length < 1 || dgProductLot == null)
            {
                Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                return;
            }

            StartCancelProcess();
        }

        private void OnClickEqptEnd(object sender, RoutedEventArgs e)
        {

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (_DT_OUT_PRODUCT.Rows.Count< 1)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPT_END();
            wndEqpComment.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
            {
                if (!Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]).Equals(""))  //_Shift
                {
                    endLotID = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]) + "," + endLotID;
                }
            }

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = cboEquipment.SelectedValue.ToString();
                Parameters[1] = _PROCID;
                Parameters[2] = endLotID; /*iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[iRow].DataItem, "LOTID"));*/
                Parameters[3] = txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(txtStartDateTime.Text);    // 시작시간 추가
                Parameters[5] = _CUTID;
                Parameters[6] = Util.NVC(txtParentQty.Value);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(OnCloseEqptEnd);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        private void OnCloseEqptEnd(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END window = sender as LGC.GMES.MES.CMM001.CMM_COM_EQPT_END;
            if (window.DialogResult == MessageBoxResult.OK)
                REFRESH = true;
        }

        private void OnClickEqptEndCancel(object sender, RoutedEventArgs e)
        {
            DataRow[] dr = Util.gridGetChecked(ref dgProductLot, "CHK");

            if (dr == null || dr.Length < 1 || dgProductLot == null)
            {
                Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                return;
            }

            EqptEndCancelProcess();
        }

        // 실적확정취소 기능 추가 [2019-12-13]
        private void OnClickEndCancel(object sender, RoutedEventArgs e)
        {
            DataRow[] dr = Util.gridGetChecked(ref dgProductLot, "CHK");

            if (dr == null || dr.Length < 1 || dgProductLot == null)
            {
                Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                return;
            }
            EndCancelProcess();
        }

        private void btnDispatch_Click(object sender, RoutedEventArgs e)
        {
            //if (txtInputQty.Value > 0)
            //    return;

            if (_DT_OUT_PRODUCT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 확정 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_WIPSTAT, Wip_State.END))
            {
                Util.MessageValidation("SFU5131");  //실적확정 Lot 선택 오류 [선택한 Lot이 완공상태 인지 확인 후 처리]
                return;
            }

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            if (!ValidateConfirmSlitter())
                return;

            try
            {
                if (_PROCID.Equals(Process.SLITTING) && IsAbnormalLot())
                {

                    #region #전수불량 Lane 등록
                    CMM_ELEC_DFCT_ACTION_CHK wndDefectLaneChk = new CMM_ELEC_DFCT_ACTION_CHK();
                    wndDefectLaneChk.FrameOperation = FrameOperation;

                    if (wndDefectLaneChk != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CUT_ID"));

                        C1WindowExtension.SetParameters(wndDefectLaneChk, Parameters);

                        wndDefectLaneChk.Closed += new EventHandler(wndDefectLaneChk_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => wndDefectLaneChk.ShowModal()));

                    }
                    #endregion
                }
                else
                {
                    ConfirmDispatcher();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void wndDefectLaneChk_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_DFCT_ACTION_CHK window = sender as CMM_ELEC_DFCT_ACTION_CHK;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                ConfirmDispatcher();
            }
        }

        private void OnClickHistoryCard(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup2 = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup2.FrameOperation = FrameOperation;

            if (wndPopup2 != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[4];
                Parameters[0] = txtEndLotId.Text; //LOT ID
                Parameters[1] = _PROCID; //PROCESS ID

                // SKIDID
                if (string.Equals(_PROCID, Process.SLITTING) && !string.IsNullOrEmpty(Util.NVC(txtEndLotId.Tag)))
                    Parameters[2] = Util.NVC(txtEndLotId.Tag);
                else
                    Parameters[2] = string.Empty;

                Parameters[3] = "Y";    // 실적확정 여부

                C1WindowExtension.SetParameters(wndPopup2, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup2.ShowModal()));
            }
        }

        private void OnClickEqptIssue(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = _PROCID;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;
                Parameters[6] = Util.NVC(txtShift.Text);
                Parameters[7] = Util.NVC(txtShift.Tag);
                Parameters[8] = Util.NVC(txtWorker.Text);
                Parameters[9] = Util.NVC(txtWorker.Tag);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        private void OnClickEqptCond(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            CMM_ELEC_EQPT_COND wndPopup = new CMM_ELEC_EQPT_COND();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipment.SelectedValue;
                Parameters[1] = cboEquipment.Text;
                Parameters[2] = _PROCID;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnClickPrintLabel(object sender, RoutedEventArgs e)
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

            CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = _PROCID;
                Parameters[2] = cboEquipment.SelectedValue;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnClickSamplingProdT1(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = _PROCID;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnClickSkidTypeSettingByPort(object sender, RoutedEventArgs e)
        {
            CMM_ELEC_SKIDTYPE_SETTING_PORT popSkidTypeSettingByPort = new CMM_ELEC_SKIDTYPE_SETTING_PORT { FrameOperation = FrameOperation };
            object[] parameters = new object[6];
            parameters[0] = LoginInfo.CFG_AREA_ID;
            C1WindowExtension.SetParameters(popSkidTypeSettingByPort, parameters);

            popSkidTypeSettingByPort.Closed += popSkidTypeSettingByPort_Closed;
            Dispatcher.BeginInvoke(new Action(() => popSkidTypeSettingByPort.ShowModal()));
        }

        private void OnClickManualWorkMode(object sender, RoutedEventArgs e)
        {
            List<Button> buttons = new List<Button>();
            buttons.Add(btnStart);
            buttons.Add(btnCancel);
            buttons.Add(btnEqptEnd);
            buttons.Add(btnEndCancel);

            if (isManualWorkMode)
                foreach (Button button in buttons)
                    button.Visibility = Visibility.Collapsed;
            else
                SetManualMode(buttons);
           
            //isManualWorkMode = !isManualWorkMode;

            btnExtra.IsDropDownOpen = false;

            if (btnStart.Visibility != Visibility.Visible)
            {
                Util.MessageValidation("SFU5142");  //수작업 모드를 진행할 권한이 없습니다.(엔지니어에게 문의 바랍니다.)
                return;
            }
        }

        private void popSkidTypeSettingByPort_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_SKIDTYPE_SETTING_PORT popup = sender as CMM_ELEC_SKIDTYPE_SETTING_PORT;
            if (popup != null && popup.IsUpdated)
            {

            }
        }

        protected void OnDefectCurrentChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
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
                                            dgLVIndex1 = e.Cell.Row.Index;

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }                                                    
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
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
                                            if (dgLVIndex1 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex1 = e.Cell.Row.Index;
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel1.CurrentCell != null)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.CurrentCell.Row.Index, dgLevel1.Columns.Count - 1);
                                else if (dgLevel1.Rows.Count > 0)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.Rows.Count, dgLevel1.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
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
                                            dgLVIndex2 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }                                                    
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();

                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
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
                                            if (dgLVIndex2 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex2 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel2.CurrentCell != null)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.CurrentCell.Row.Index, dgLevel2.Columns.Count - 1);
                                else if (dgLevel2.Rows.Count > 0)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.Rows.Count, dgLevel2.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
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
                                            dgLVIndex3 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
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
                                            if (dgLVIndex3 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex3 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel3.CurrentCell != null)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.CurrentCell.Row.Index, dgLevel3.Columns.Count - 1);
                                else if (dgLevel3.Rows.Count > 0)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.Rows.Count, dgLevel3.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }

        private void OnLoadedDefectCellPresenter(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {

                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex1 = 0;
                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel1.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex2 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel2.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex3 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel3.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;
            }
        }

        private void OnCancelDeleteLot(object sender, RoutedEventArgs e)
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

            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = _PROCID; //PROCESS ID
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(OnCloseCancelDeleteLot);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseCancelDeleteLot(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
                REFRESH = true;
        }

        private void OnKeyDownInputQty(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (dgLotInfo.GetRowCount() < 1)
                        return;

                    SetResultInputQTY();
                }
            }
            catch (Exception ex)
            {
                txtInputQty.Value = 0;
                Util.MessageException(ex);
            }
        }

        private void OnKeyLostInputQty(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isChangeInputFocus == false && txtInputQty.Value > 0)
                OnKeyDownInputQty(txtInputQty, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        private void OnClickWIPHistory(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                // 진행LOT이 실적 확정 완료이면 저장 전체 방어
                if (ValidConfirmLotCheck() == false)
                {
                    Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                    return;
                }

                if (_DT_OUT_PRODUCT.Rows.Count > 0)
                {
                    if (txtInputQty.Value <= 0)
                    {
                        if (IsCoaterProdVersion() == true && !string.Equals(GetCoaterMaxVersion(), txtVersion.Text))
                        {
                            // 작업지시 최신 Version과 상이합니다! 그래도 저장하시겠습니까?
                            Util.MessageConfirm("SFU4462", (sResult) =>
                            {
                                if (sResult == MessageBoxResult.OK)
                                {
                                    SaveWIPHistory();
                                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                                }
                            }, new object[] { GetCoaterMaxVersion(), txtVersion.Text });
                        }
                        else
                        {
                            SaveWIPHistory();
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                            Util.MessageInfo("SFU1270");    //저장되었습니다.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnClickSaveCarrier(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnClickRegDefectLane(object sender, RoutedEventArgs e)
        {
            try
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

                if (_DT_OUT_PRODUCT.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                    return;
                }

                if (!(string.Equals(_WIPSTAT, Wip_State.EQPT_END) || string.Equals(_WIPSTAT, Wip_State.END)))
                {
                    Util.MessageValidation("SFU3723");  //작업 가능한 상태가 아닙니다.
                    return;
                }

                LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[6];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CUT_ID"));

                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE_PANCAKE;

            #region 전수불량 Lot 불량 0 처리
            DataTable dtTmp = getDefectLaneLotList(_CUTID);
            DataRow[] dtRows = dtTmp.Select("CHK = 1");
            SaveDefectByDefectLane(dtRows);
            #endregion

            //if (window.DialogResult == MessageBoxResult.OK)
                REFRESH = true;
        }

        private void OnLoadedLotInfoCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (!string.Equals(_PROCID, Process.PRE_MIXING) && !string.Equals(_PROCID, Process.MIXING) && !string.Equals(_PROCID, Process.SRS_MIXING))
                                {
                                    if (dataGrid.GetRowCount() > 0)
                                    {
                                        if (e.Cell.Column.Visibility == Visibility.Visible)
                                        {
                                            TextBlock sContents = e.Cell.Presenter.Content as TextBlock;

                                            int iSourceIdx = e.Cell.Row.Index - (dataGrid.Rows.Count - dataGrid.BottomRows.Count) + dataGrid.TopRows.Count;
                                            if (DataTableConverter.Convert(dataGrid.ItemsSource).Columns.Contains(e.Cell.Column.Name))
                                            {

                                                string sValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[iSourceIdx].DataItem, e.Cell.Column.Name));

                                                if (string.Equals(e.Cell.Column.Name, "LANE_QTY"))
                                                {
                                                    sContents.Text = sValue;
                                                }
                                                else
                                                {
                                                    if (e.Cell.Column.GetType() == typeof(DataGridNumericColumn))
                                                        sContents.Text = GetUnitFormatted(Convert.ToDouble(Util.NVC_Decimal(string.IsNullOrEmpty(sValue) ? "0" : sValue) * convRate), "EA");
                                                    else
                                                        sContents.Text = sValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Tag, "N"))
                            {
                                if ((e.Cell.Row.Index - dataGrid.TopRows.Count) > 0)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Transparent);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }

                            if (dataGrid.Columns["INPUT_VALUE_TYPE"].Index < e.Cell.Column.Index &&
                                dataGrid.Columns["EQPT_END_QTY"].Index > e.Cell.Column.Index && ((e.Cell.Row.Index - dataGrid.TopRows.Count)) == 2)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name,
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) -
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)));

                                if (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) !=
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void OnUnLoadedLotInfoCellPresenter(object sender, DataGridCellEventArgs e)
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
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }        

        private void OnDataCollectGridCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (_DT_OUT_PRODUCT.Rows.Count > 0)
            {
                C1DataGrid caller = sender as C1DataGrid;

                if (ValidateDefect(sender as C1DataGrid))
                {
                    if (_DT_OUT_PRODUCT.Rows.Count > 1)
                    {
                        DefectChange(caller, caller.CurrentCell.Row.Index, caller.CurrentCell.Column.Index);
                    }
                    else
                    {
                        // 청주1동 특화 M -> P 변환 로직 [2017-05-15]
                        if (string.Equals(caller.CurrentCell.Column.Name, "CONVRESNQTY") && string.Equals(txtUnit.Text, "EA"))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY",
                                Convert.ToDouble(GetIntFormatted(Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CONVRESNQTY")) / convRate)));

                        GetSumDefectQty();
                    }

                    // 불량량 입력 시 전체 선택 Row 존재 하는 경우 해제 및 수량 초기화.
                    string sResnColName = Util.NVC(e.Cell.Column.Name).Replace("NUM", "").Replace("CNT", "").Replace("RESN_TOT_CHK", "").Replace("FRST_AUTO_RSLT_RESNQTY", "");
                    if (caller.Columns.Contains(sResnColName) && 
                        Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, sResnColName)) > 0 &&
                        !Util.NVC(e.Cell.Column.Name).EndsWith("NUM") &&
                        !Util.NVC(e.Cell.Column.Name).EndsWith("CNT") &&
                        !Util.NVC(e.Cell.Column.Name).EndsWith("RESN_TOT_CHK") &&
                        !Util.NVC(e.Cell.Column.Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                    {
                        if (e.Cell.Column.Index == caller.Columns["ALL"].Index)
                        {
                            for (int iCol = caller.Columns["ALL"].Index; iCol < caller.Columns["COSTCENTERID"].Index; iCol++)
                            {
                                if (!Util.NVC(caller.Columns[iCol].Name).Equals("ALL") &&
                                    !Util.NVC(caller.Columns[iCol].Name).EndsWith("NUM") &&
                                    !Util.NVC(caller.Columns[iCol].Name).EndsWith("CNT") &&
                                    !Util.NVC(caller.Columns[iCol].Name).EndsWith("RESN_TOT_CHK") &&
                                    !Util.NVC(caller.Columns[iCol].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                {
                                    for (int iRow = 0; iRow < caller.GetRowCount(); iRow++)
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, Util.NVC(caller.Columns[iCol].Name) + "RESN_TOT_CHK")).ToUpper().Equals("TRUE") ||
                                            Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, Util.NVC(caller.Columns[iCol].Name) + "RESN_TOT_CHK")).Equals("1"))
                                        {
                                            DataTableConverter.SetValue(caller.Rows[iRow].DataItem, Util.NVC(caller.Columns[iCol].Name) + "RESN_TOT_CHK", false);

                                            // 길이초과 초기화 제외.
                                            if (!Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_LEN_EXCEED))
                                            {
                                                DataTableConverter.SetValue(caller.Rows[iRow].DataItem, Util.NVC(caller.Columns[iCol].Name), 0);
                                                DefectChange(caller, iRow, iCol);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int iRow = 0; iRow < caller.GetRowCount(); iRow++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, sResnColName + "RESN_TOT_CHK")).ToUpper().Equals("TRUE") ||
                                    Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, sResnColName + "RESN_TOT_CHK")).Equals("1"))
                                {
                                    DataTableConverter.SetValue(caller.Rows[iRow].DataItem, sResnColName + "RESN_TOT_CHK", false);

                                    // 길이초과 초기화 제외.
                                    if (!Util.NVC(DataTableConverter.GetValue(caller.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_LEN_EXCEED))
                                    {
                                        DataTableConverter.SetValue(caller.Rows[iRow].DataItem, sResnColName, 0);
                                        DefectChange(caller, iRow, caller.Columns[sResnColName].Index);
                                    }
                                }
                            }
                        }                        
                    }

                    SetExceedLength();
                    dgLotInfo.Refresh(false);
                    _LOSSQTY = string.Empty;
                    _GOODQTY = string.Empty;
                    _CHARGEQTY = string.Empty;
                }
            }
        }

        private void OnDataCollectGridBeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            
            if (dataGrid != null)
            {
                if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y"))
                    e.Cancel = true;


                if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                {
                    if (dataGrid.Columns["ALL"].Index < e.Column.Index && dataGrid.Columns["COSTCENTERID"].Index > e.Column.Index)
                    {
                        if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_LACK) ||
                            string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
                        {
                            e.Cancel = true;
                            dataGrid.BeginEdit(e.Row.Index, dataGrid.Columns["ALL"].Index);
                        }
                    }
                }
            }
                        
            if (isResnCountUse == true && (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING)))
            {
                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "DEFECT_LOT") ||
                    string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "LOSS_LOT"))
                {
                    if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y") &&
                        string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "LINK_DETL_RSN_CODE_TYPE"))))
                        return;
                }

                if (e.Column.Name.Length == 13 && e.Column.Name.Contains("NUM") == true)
                    e.Cancel = true;
            }

            // 물품청구, 길이초과, 길이부족은 전체 선택 불가.
            if (dataGrid != null && Util.NVC(e?.Column?.Name).EndsWith("RESN_TOT_CHK"))
            {
                if (string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_LACK) ||
                    string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED) ||
                    string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void OnDataCollectGridBeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    string sDfctColName = Util.NVC(e.Column.Name).Replace("RESN_TOT_CHK", "");

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                    // 입력 최대 수량.
                                    decimal maxSetValue = 0;
                                    ArrayList aList = new ArrayList();

                                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR))
                                    {
                                        // 복수개 설정 가능.
                                        decimal maxProdIncrValue = 0;
                                        for (int iRow = dataGrid.TopRows.Count; iRow < dataGrid.GetRowCount(); iRow++)
                                        {                                            
                                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR))
                                            {
                                                if (iRow != idx)
                                                {
                                                    maxProdIncrValue += Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[iRow].DataItem, sDfctColName + "FRST_AUTO_RSLT_RESNQTY"));
                                                    aList.Add(iRow);
                                                }
                                            }
                                        }

                                        int iLenLackRow = Util.gridFindDataRow(ref dataGrid, "PRCS_ITEM_CODE", ITEM_CODE_LEN_LACK, false);

                                        if (iLenLackRow >= 0)
                                            maxSetValue = maxProdIncrValue +
                                                          Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, sDfctColName + "FRST_AUTO_RSLT_RESNQTY")) +
                                                          Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[iLenLackRow].DataItem, sDfctColName + "FRST_AUTO_RSLT_RESNQTY"));
                                    }
                                    else
                                        maxSetValue = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);


                                    DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, sDfctColName, maxSetValue);

                                    foreach (int iFndRow in aList)
                                    {
                                        DataTableConverter.SetValue(dataGrid.Rows[iFndRow].DataItem, sDfctColName, 0);
                                        GetSumCutDefectQty(dataGrid, iFndRow, dgWipReason.Columns[sDfctColName].Index);
                                    }

                                    DefectChange(dataGrid, e.Row.Index, e.Column.Index - 1);

                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, Util.NVC(e.Column.Name), false);

                                            // 길이초과 초기화 제외.
                                            if (!Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_LEN_EXCEED))
                                            {
                                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, sDfctColName, 0);
                                                DefectChange(dataGrid, i, e.Column.Index - 1);
                                            }
                                        }
                                    }
                                    GetSumDefectQty();
                                    dgLotInfo.Refresh(false);
                                    dgWipReason.Refresh(false);
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, Util.NVC(e.Column.Name), false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, sDfctColName, 0);
                            DefectChange(dataGrid, idx, e.Column.Index - 1);

                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
                            dgWipReason.Refresh(false);
                        }
                    }
                }
            }));
        }

        private void OnDataCollectGridPreviewKeyDown(object sender, KeyEventArgs e)
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

        private void OnClickCellMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void OnLoadedRemarkCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 1)
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
                else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;
                        }
                    }
                }
            }
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
            {
                isChangeRemark = true;
            }
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }

        private void OnDataCollectGridQualityCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02"));
                //sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-').Count() == 3)
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0] + "-" +
                        Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[1];
                }
                else
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                }
                sCLCNAME = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
            }
            else
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            }
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
                    isChangeQuality = true;
                }                
            }
            else if (e.Cell.Presenter != null)
            {
            }
        }

        private void OnLoadedDgQualitynCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
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
                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
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
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit, CultureInfo.InvariantCulture);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit, CultureInfo.InvariantCulture);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrEmpty(Util.NVC(numeric.Value)) && numeric.Value != 0 && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            if (!string.IsNullOrEmpty(sValue) && !string.Equals(sValue, "NaN"))
                                            //numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture);
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
                    }
                }));
            }
        }

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

                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line);

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
                        isChangeQuality = true;
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

                    isChangeQuality = true;
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

        private void OnUnLoadedDgQualitynCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                e.Cell.Presenter.Background = null;

                                if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void OnDataCollectGridCottonTagCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgCotton.Columns[e.Cell.Column.Index].Name)).Equals(""))
            {
                isChangeCotton = true;
            }
        }

        private void OnClickSaveWipReason(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgWipReason.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                    return;
                }

                SaveDefect(dgWipReason);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void OnClickProcReason(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_FOR_AUTO_RSLT wndPopup = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_FOR_AUTO_RSLT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = _PROCID;
                Parameters[2] = _CUTID;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[4] = GetUnitFormatted();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseProcReason);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseProcReason(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_FOR_AUTO_RSLT window = sender as LGC.GMES.MES.CMM001.Popup.CMM_ELEC_PROC_RESN_FOR_AUTO_RSLT;
            if (window.DialogResult == MessageBoxResult.OK)
                SetProcWipReasonData();
        }

        private void OnClickSaveQuality(object sender, RoutedEventArgs e)
        {
            // 품질정보 SLITTER, SRS SLITTER는 CUT단위로 관리 [2017-02-01]
            if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING))
            {
                SaveCutQuality(dgQuality);
            }
            else
            {
                SaveQuality(dgQuality);
            }
        }

        private void OnClickSaveRemark(object sender, RoutedEventArgs e)
        {
            SetWipNote();
        }

        private void OnClickPublicSave(object sender, RoutedEventArgs e)
        {

            try
            {
                SaveDefect(dgWipReason);

            }
            catch (Exception ex) { Util.MessageException(ex); }

            SavePublicCutQuality(dgQuality);
            SetWipNote();
        }

        private void OnClickCottonSave(object sender, RoutedEventArgs e)
        {
            if (dgCotton.GetRowCount() < 1) return;

            SaveCotton(dgCotton);
        }
        
        private void OnSumReasonGridChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb != null && _DT_OUT_PRODUCT.Rows.Count > 0)
            {
                dgWipReason.Refresh(false);

                if (_DT_OUT_PRODUCT.Rows.Count == 1)
                    GetDefectList();
                else if (_DT_OUT_PRODUCT.Rows.Count > 1)
                    GetDefectListMultiLane();
            }
        }

        private void OnLaneSelectionItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboLaneNum.Text = ObjectDic.Instance.GetObjectName("Lane선택");

        }

        private void OnLoadedWipReasonCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null && e != null && e.Cell != null && e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                    if (panel == null && panel.Children == null && panel.Children.Count < 1) return;

                    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    if (e.Cell.Column.Index == dataGrid.Columns["ACTNAME"].Index)
                    {
                        if (e.Cell.Row.Index == (dataGrid.Rows.Count - 3))
                            presenter.Content = ObjectDic.Instance.GetObjectName("불량수량");
                        else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                            presenter.Content = ObjectDic.Instance.GetObjectName("양품수량");
                        else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                            presenter.Content = ObjectDic.Instance.GetObjectName("투입수량");
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["RESNTOTQTY"].Index)
                    {
                        if (_DT_OUT_PRODUCT.Rows.Count > 0 && dgLotInfo.GetRowCount() > 0)
                        {
                            if (e.Cell.Row.Index == (dataGrid.Rows.Count - 3))
                                presenter.Content = GetUnitFormatted((Convert.ToDecimal(presenter.Content) - (exceedLengthQty * _DT_OUT_PRODUCT.Rows.Count)));
                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                                presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY"))) - (Convert.ToDecimal(presenter.Content) - (exceedLengthQty * _DT_OUT_PRODUCT.Rows.Count)));
                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY"))));
                        }
                    }
                    else
                    {
                        if (_DT_OUT_PRODUCT.Rows.Count > 0 && dgLotInfo.GetRowCount() > 0)
                        {
                            if (e.Cell.Row.Index == (dataGrid.Rows.Count - 3))
                                presenter.Content = GetUnitFormatted(Convert.ToDecimal(presenter.Content) - exceedLengthQty);
                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 2))
                                presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count)) - (Convert.ToDecimal(presenter.Content) - exceedLengthQty));
                            else if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                presenter.Content = GetUnitFormatted((Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count)));
                        }
                    }
                }
            }
        }

        private void OnKeyDownMesualQty(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_DT_OUT_PRODUCT.Rows.Count < 1)
                    return;

                C1NumericBox numericBox = sender as C1NumericBox;
                if (numericBox != null)
                {
                    for (int i = 0; i < dgQuality.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgQuality.Rows[i].DataItem, "CLCTVAL01", numericBox.Value);

                        decimal iUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgQuality.Rows[i].DataItem, "USL"));
                        decimal iLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgQuality.Rows[i].DataItem, "LSL"));

                        if (iLSL == 0 && iUSL == 0)
                            continue;

                        if (dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter != null)
                        {
                            if (iLSL > Util.NVC_Decimal(numericBox.Value) || (iUSL > 0 && iUSL < Util.NVC_Decimal(numericBox.Value)))
                            {
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgQuality.GetCell(i, dgQuality.Columns["CLCTVAL01"].Index).Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    isChangeQuality = true;
                    numericBox.Value = 0;
                }
            }
        }

        private void OnKeyLostMesualQty(object sender, System.Windows.RoutedEventArgs e)
        {
            C1NumericBox numericBox = sender as C1NumericBox;

            if (numericBox != null)
                if (numericBox.Value > 0)
                    OnKeyDownMesualQty(numericBox, new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
        }

        private void OnClickShift(object sender, EventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = _PROCID;
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

        private void OnClickbtnExpandFrame(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualHeight != 0)
                ExpandFrame = Content.RowDefinitions[1].Height;
            if (btnExpandFrame.IsChecked == true)
            {
                Content.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                Content.RowDefinitions[1].Height = ExpandFrame;
            }
        }

        private void OnClickbtnLeftExpandFrame(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualWidth != 0)
                ExpandFrame = Content.ColumnDefinitions[0].Width;
            if (btnLeftExpandFrame.IsChecked == true)
            {
                Content.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Content.ColumnDefinitions[0].Width = ExpandFrame;
            }
        }

        private void OnTabSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiWipReason"))
                    {
                        dgWipReason.EndEdit(true);
                    }
                }
            }
        }

        private void OnClickSearchMergeData(object sender, RoutedEventArgs e)
        {
            if (_DT_OUT_PRODUCT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1381");    //Lot을 선택 하세요
                return;
            }

            GetMergeList();
            GetMergeEndList();
        }

        private void OnClickSaveMergeData(object sender, RoutedEventArgs e)
        {
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));
            dt2.Columns.Add("PR_LOTID", typeof(string));

            DataRow dataRow2 = dt2.NewRow();
            dataRow2["LOTID"] = Util.NVC(txtMergeInputLot.Text);

            dt2.Rows.Add(dataRow2);

            DataTable prodLotresult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt2);

            DataTable dt = ((DataView)dgWipMerge.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["CHK"].ToString().Equals("1"))
                {
                    if (prodLotresult.Rows[0]["LANE_QTY"].ToString() != dt.Rows[i]["LANE_QTY"].ToString())
                    {
                        Util.MessageInfo("SFU5081");
                        return;
                    }
                    if (prodLotresult.Rows[0]["MKT_TYPE_CODE"].ToString() != dt.Rows[i]["MKT_TYPE_CODE"].ToString())
                    {
                        Util.MessageInfo("SFU4271");
                        return;
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[i]["WH_ID"].ToString()))
                    {
                        Util.MessageInfo("SFU2963");
                        return;
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[i]["ABNORM_FLAG"].ToString()) && dt.Rows[i]["ABNORM_FLAG"].ToString().Equals("Y"))
                    {
                        Util.MessageInfo("SFU7029");
                        return;
                    }
                }

                DataTable prodLotresult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt2);
                if (!string.IsNullOrEmpty(prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString()) && prodLotresult2.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                {
                    Util.MessageValidation("SFU7029");  //전수불량레인이 존재하여 합권취 불가합니다.
                    return;
                }

                if (_DT_OUT_PRODUCT.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1381");   //Lot을 선택 하세요
                    return;
                }


                if (!string.Equals(_WIPSTAT, Wip_State.PROC))
                {
                    Util.MessageValidation("SFU3627");  //합권취는 진행 상태에서만 가능합니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtMergeInputLot.Text))
                {
                    Util.MessageValidation("SFU1945");  //투입 LOT이 없습니다.
                    return;
                }

                C1DataGrid dg = dgWipMerge;
                if (Util.gridGetChecked(ref dg, "CHK").Length <= 0)
                {
                    Util.MessageValidation("SFU3628");  //합권취 진행할 대상 Lot들이 선택되지 않았습니다.
                    return;
                }
            }
            SaveMergeData();
        }

        #endregion

        #region BizCall
        private void SetManualMode(List<Button> buttons)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(row["USERTYPE"]), "G"))
                        {
                            foreach (Button button in buttons)
                                button.Visibility = Visibility.Visible;

                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        public void RunProcess(string ValueToLotID)
        {
            try
            {
                Util.MessageConfirm("SFU1240", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        #region MESSAGE SET
                        DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");
                        inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inLotDataTable.Columns.Add("IFMODE", typeof(string));
                        inLotDataTable.Columns.Add("EQPTID", typeof(string));
                        inLotDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inLotDataRow = null;
                        inLotDataRow = inLotDataTable.NewRow();
                        inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inLotDataRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                        inLotDataRow["USERID"] = LoginInfo.USERID;
                        inLotDataTable.Rows.Add(inLotDataRow);

                        DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                        InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                        DataRow inMtrlDataRow = null;

                        inMtrlDataRow = InMtrldataTable.NewRow();
                        inMtrlDataRow["INPUT_LOTID"] = ValueToLotID;
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(cboEquipment.SelectedValue.ToString());
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                        InMtrldataTable.Rows.Add(inMtrlDataRow);
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_SL", "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            #region SAVE DEFECT
            inDataSet = new DataSet();

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
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = _PROCID;
            inDataTable.Rows.Add(inDataRow);

            DataTable inDefectLot = _DT_OUT_PRODUCT.Rows.Count > 1 ? dtDataCollectOfChildrenDefect(dg) : dtDataCollectOfChildDefect(dg);
            #endregion
            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "IINDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveDefectByDefectLane(DataRow[] dfctLot)
        {
            if (dgWipReason.GetRowCount() <= 0)
            {
                //Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }
            
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
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = _PROCID;
            inDataTable.Rows.Add(inDataRow);
            
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            if (isResnCountUse == true)
                IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            for (int i = 0; i < dfctLot.Length; i++)
            {
                if (!dgWipReason.Columns.Contains(Util.NVC(dfctLot[i]["LOTID"]))) continue;

                string sublotid = Util.NVC(dfctLot[i]["LOTID"]);
                foreach (DataRow _iRow in ((dgWipReason.ItemsSource as DataView).Table).Rows)
                {
                    inDataRow = IndataTable.NewRow();

                    inDataRow["LOTID"] = sublotid;
                    inDataRow["WIPSEQ"] = _DT_OUT_PRODUCT.Rows[0]["WIPSEQ"];
                    inDataRow["ACTID"] = _iRow["ACTID"];
                    inDataRow["RESNCODE"] = _iRow["RESNCODE"];
                    inDataRow["RESNQTY"] = 0;// _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inDataRow["DFCT_TAG_QTY"] = 0;// _iRow[sublotid + "CNT"].ToString().Equals("") ? 0 : _iRow[sublotid + "CNT"];
                    inDataRow["LANE_QTY"] = 1;
                    inDataRow["LANE_PTN_QTY"] = 1;
                    inDataRow["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

                    if (isResnCountUse == true)
                        inDataRow["WRK_COUNT"] = 0;// string.IsNullOrEmpty(_iRow[sublotid + "NUM"].ToString()) ? 0 : _iRow[sublotid + "NUM"];

                    IndataTable.Rows.Add(inDataRow);
                }
            }

            if (IndataTable.Rows.Count < 1)
                return;

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private Int32 getCurrLaneQty(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { }

            return 0;
        }

        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

            // TOP/BACK을 동시 적용하기 위하여 해당 방식으로 변경 (SPLUNK문제로 CMI요청사항) [2018-05-24]
            if (dgQuality2.Visibility == Visibility.Visible)
                inEDCLot.Merge(dtDataCollectOfChildQuality(dgQuality2));

            new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (!dg.Name.Equals("dgColor") && !dg.Name.Equals("dgDefectTag"))
                    isChangeQuality = false;

                if (dg.Name.Equals("dgColor"))
                    Util.MessageInfo("SFU3272");    //색지정보가 저장되었습니다.
                else if (dg.Name.Equals("dgDefectTag"))
                    Util.MessageInfo("SFU3271");    //불량태그정보가 저장되었습니다.
                else
                    Util.MessageInfo("SFU1998");    //품질 정보가 저장되었습니다.                
            });
        }

        private void GetWipSeq(string sLotID, string sCLCTITEM)
        {
            _WIPSEQ = string.Empty;
            _CLCTSEQ = string.Empty;

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = _PROCID;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_SL", "INDATA", "RSLTDT", IndataTable);

            if (dtResult.Rows.Count == 0)
            {
                _WIPSEQ = string.Empty;
                _CLCTSEQ = string.Empty;
            }
            else
            {
                _WIPSEQ = dtResult.Rows[0]["WIPSEQ"].ToString();
                _CLCTSEQ = dtResult.Rows[0]["CLCTSEQ"].ToString();
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

        private void GetProductLot(DataRowView drv = null)
        {
            try
            {
                string ValueToCondition = string.Empty;
                var sCond = new List<string>();

                SetStatus();

                if (string.IsNullOrEmpty(WIPSTATUS))
                {
                    Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                // SLITTER CUT기준으로 변경으로 인하여 LOT ID -> CUT ID로 표기 ( 2017-01-21 ) CR-53
                if ((string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING)) && !string.Equals(WIPSTATUS, Wip_State.WAIT))
                {
                    dgProductLot.Columns["CUT_ID"].Visibility = Visibility.Visible;
                    dgProductLot.Columns["LOTID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgProductLot.Columns["CUT_ID"].Visibility = Visibility.Collapsed;
                    dgProductLot.Columns["LOTID"].Visibility = Visibility.Visible;
                }

                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                    return;

                string sPrvLot = string.Empty;

                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                Util.gridClear(dgProductLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("WIPSTAT", typeof(string));
                IndataTable.Columns.Add("LOTID_LARGE", typeof(string));
                IndataTable.Columns.Add("COATSIDE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);  // 자동차 전극은 EQSG가 2개라서 변경 가능하기 때문에 선택된 걸로 변경 ( 2017-01-20 )
                Indata["PROCID"] = _PROCID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["WIPSTAT"] = WIPSTATUS;

                if (drv != null)
                    Indata["LOTID_LARGE"] = drv["LOTID_LARGE"].ToString();
                
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_ELEC", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgProductLot, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWrkShftUser()
        {
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
            Indata["PROCID"] = _PROCID;
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
            });
        }

        private void SetIdentInfo()
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    _LDR_LOT_IDENT_BAS_CODE = string.Empty;
                    _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = _PROCID;
                row["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _UNLDR_LOT_IDENT_BAS_CODE = result.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();

                    switch (_LDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }

                    switch (_UNLDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Visible;

                            if (string.Equals(_PROCID, Process.COATING))
                            {
                                dgLotInfo.Columns["OUT_CSTID"].IsReadOnly = false;
                                btnSaveCarrier.Visibility = Visibility.Visible;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            LOTID = Util.NVC(rowview["LOTID"]);
            _LOTID = Util.NVC(rowview["LOTID"]);
            _LOTIDPR = Util.NVC(rowview["LOTID_PR"]);
            _WIPSTAT = Util.NVC(rowview["WIPSTAT"]);
            _WIPDTTM_ST = Util.NVC(rowview["WIPDTTM_ST"]);
            _WIPDTTM_ED = Util.NVC(rowview["WIPDTTM_ED"]);
            //_REMARK = Util.NVC(rowview["REMARK"]);
            _PRODID = Util.NVC(rowview["PRODID"]);
            _EQPTID = Util.NVC(rowview["EQPTID"]);
            _WORKORDER = Util.NVC(rowview["WOID"]);
            _LANEQTY = "0"; // LANE수 디폴트 0 SET
            _PTNQTY = "1"; // 패턴 사용을 안하지만 다른곳에서 패턴을 사용할 것을 대비하여 1로 SET
            _CSTID = (DataTableConverter.Convert(rowview.DataView).Columns["CSTID"] == null) ? "" : Util.NVC(rowview["CSTID"]);

            _CUTID = (_PROCID.Equals(Process.INS_COATING) || _PROCID.Equals(Process.INS_SLIT_COATING)) ? "1" : Util.NVC(rowview["CUT_ID"]);

            // 버전 공통으로 입력 하는 부분 추가 [2017-02-17]
            DataTable versionDt = GetProcessVersion(_LOTID, _PRODID);
            if (versionDt.Rows.Count > 0)
            {
                _VERSION = Util.NVC(versionDt.Rows[0]["PROD_VER_CODE"]);
                _LANEQTY = string.IsNullOrEmpty(Util.NVC(versionDt.Rows[0]["LANE_QTY"])) ? "0" : Util.NVC(versionDt.Rows[0]["LANE_QTY"]);
            }

            if (_WIPSTAT != "WAIT")
            {
                txtVersion.Text = _VERSION;
                txtLaneQty.Value = string.IsNullOrEmpty(_LANEQTY) ? 0 : Convert.ToDouble(_LANEQTY);
                txtLanePatternQty.Value = string.IsNullOrEmpty(_PTNQTY) ? 0 : Convert.ToDouble(_PTNQTY);

                txtStartDateTime.Text = Convert.ToDateTime(_WIPDTTM_ST).ToString("yyyy-MM-dd HH:mm");

                if (string.IsNullOrEmpty(_WIPDTTM_ED))
                    txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                else
                    txtEndDateTime.Text = Convert.ToDateTime(_WIPDTTM_ED).ToString("yyyy-MM-dd HH:mm");

                WIPSTATUS = _WIPSTAT;
                _WORKDATE = Util.NVC(rowview["WORKDATE"]);

                if (txtWorkDate != null)
                    SetCalDate();

                if (!_PROCID.Equals(Process.PRE_MIXING) && !_PROCID.Equals(Process.BS) && !_PROCID.Equals(Process.CMC) && !_PROCID.Equals(Process.MIXING) && !_PROCID.Equals(Process.SRS_MIXING))
                    LOTID = WIPSTATUS == "EQPT_END" ? _LOTIDPR : _LOTID;

                txtUnit.Text = rowview["UNIT_CODE"].ToString();

                // 합권취용 투입 LOT SET
                txtMergeInputLot.Text = string.IsNullOrEmpty(_LOTIDPR) ? _LOTID : _LOTIDPR;

                // 청주 소형전극에서만 패턴에서만 변환해서 값 입력 [COATER에서만 사용]                
                if (string.Equals(txtUnit.Text, "EA") && (!string.Equals(_PROCID, Process.PRE_MIXING) && !string.Equals(_PROCID, Process.MIXING) && !string.Equals(_PROCID, Process.SRS_MIXING)))
                    convRate = GetPatternLength(_PRODID);


                SetUnitFormatted(); // UNIT별로 FORMAT을 별도로 해달라는 요청이 있어서 해당 기능 적용 [2017-02-21]
                GetOutProductList();
                GetResultInfo();
                                
                #region # 전수불량 Lane
                //현재 Lane 수량
                txtCurLaneQty.Value = getCurrLaneQty(_LOTID, _PROCID);

                if (getDefectLane(LoginInfo.CFG_EQSG_ID, _PROCID))
                    btnSaveRegDefectLane.Visibility = Visibility.Visible;

                #endregion


                // 완공 상태일 경우만 불량/Loss 저장 가능
                if (string.Equals(rowview["WIPSTAT"], Wip_State.END))
                {
                    btnSaveWipReason.IsEnabled = true;
                    btnPublicWipSave.IsEnabled = true;
                    //btnSaveRegDefectLane.IsEnabled = true;
                    //btnProcResn.IsEnabled = true;

                    if (string.Equals(_PROCID, Process.SLITTING) && (txtCurLaneQty.Value == txtLaneQty.Value))
                        btnSaveRegDefectLane.IsEnabled = false;
                    else
                        btnSaveRegDefectLane.IsEnabled = true;
                }
            }
        }

        private DataTable GetProcessVersion(string sLotID, string sModlID)
        {
            // VERSION, LANE수를 룰에 따라 가져옴 [2017-02-17]
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MODLID", typeof(string));
                inTable.Columns.Add("PROCSTATE", typeof(string));
                inTable.Columns.Add("TOPLOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                indata["PROCID"] = _PROCID;
                indata["EQPTID"] = _EQPTID;
                indata["LOTID"] = sLotID;
                indata["MODLID"] = sModlID;
                indata["PROCSTATE"] = "Y";
                
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_V01", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private void SetCalDate()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = cboEquipment.SelectedValue;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", dt);

                if (result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["CALDATE"])))
                {
                    txtWorkDate.Text = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtWorkDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = prodID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                        if (string.Equals(row["PROD_VER_CODE"], txtVersion.Text) && !string.IsNullOrEmpty(Util.NVC(row["PTN_LEN"])))
                            return Util.NVC_Decimal(row["PTN_LEN"]);

                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private void GetResultInfo()
        {
            try
            {
                Util.gridClear(dgLotInfo);

                DataRowView rowView = dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem as DataRowView;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CUT_ID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow indata = inTable.NewRow();

                indata["LANGID"] = LoginInfo.LANGID;
                indata["CUT_ID"] = Util.NVC(DataTableConverter.GetValue(rowView, "CUT_ID"));
                indata["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(rowView, "WIPSEQ")).Equals("") ? 0 : DataTableConverter.GetValue(rowView, "WIPSEQ");
                
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_INFO_CUT", "INDATA", "RSLTDT", inTable);

                Util.GridSetData(dgLotInfo, dt, FrameOperation, true);
                
                // 모랏 수량 설정.
                SetParentQty(_LOTID, WIPSTATUS); // OUTLOT 기준으로 변경 (CR54) 

                // Roll Press, Slitter 특이사항 Setting
                BindingWipNote();


                // 해당 설비 완공 시점에서는 설비완공 시점에서 투입량을 수량으로 변경한다 [2017-02-14]
                if (string.Equals(WIPSTATUS, Wip_State.EQPT_END) || string.Equals(WIPSTATUS, Wip_State.END))
                    txtInputQty.Value = Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "EQPT_END_QTY"));

                // 절연코터, BACK WINDER는 자동 입력 후 수정 못하게 변경 (믹서는 투입자재 총수량 = 생산량)
                // 믹서 공정 다시 설비완공수량을 생산량으로 자동입력하게 변경, 또한 표면검사는 투입량 -> 생산량 자동입력 및 수정 가능하게 변경 요청
                // 백와인더, INS슬리터 코터는 모LOT 투입 기준 수정X, 나머지 공정들은 모LOT 투입 기준 수정 O                
                //if (txtInputQty.IsReadOnly == true || string.Equals(_PROCID, Process.REWINDER) || string.Equals(_PROCID, Process.ROLL_PRESSING) || string.Equals(_PROCID, Process.INS_COATING) ||
                //    string.Equals(_PROCID, Process.HALF_SLITTING) || string.Equals(_PROCID, Process.TAPING))
                //    txtInputQty.Value = txtParentQty.Value;

                // 저장되어 있는 수량이 있으면 그 수량을 최선책으로 지정 [2017-04-21]
                SetSaveProductQty();

                SetInputQty();
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetOutProductList()
        {
            try
            {
                _DT_OUT_PRODUCT.Clear();
                Util.gridClear(dgOutProduct);

                DataRowView rowView = dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem as DataRowView;

                string lotId = Util.NVC(DataTableConverter.GetValue(rowView, "LOTID"));

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("CUT_ID", typeof(string));
                
                DataRow indata = inTable.NewRow();

                indata["CUT_ID"] = _CUTID;
                indata["LOTID_PR"] = _LOTIDPR;
                indata["LOTID"] = null;
                indata["WIPSTAT"] = WIPSTATUS;
                indata["LANGID"] = LoginInfo.LANGID;
                indata["PROCID"] = _PROCID;

                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RUNLOT_SL", "INDATA", "RSLTDT", inTable);

                _DT_OUT_PRODUCT = dt.Copy();
                Util.GridSetData(dgOutProduct, dt, FrameOperation, true);                
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetParentQty(string lotid, string status)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = lotid;

                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRLOT_QTY_L", "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    //if (status.Equals(Wip_State.EQPT_END))
                    //    txtInputQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_OUT"]);

                    txtParentQty.Value = Convert.ToDouble(dt.Rows[0]["WIPQTY_IN"].ToString());
                    SetParentRemainQty();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
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

        private void SetSaveProductQty()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = _LOTID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR_FOR_PROD_QTY", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                    if (Util.NVC_Decimal(result.Rows[0]["PROD_QTY"]) > 0)
                        txtInputQty.Value = Convert.ToDouble(result.Rows[0]["PROD_QTY"]);
            }
            catch (Exception ex) { }
        }

        private bool getDefectLane(string sEQSGID, string sPROCID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "SCRIBE_DEFECT_LANE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(Util.NVC(row["CBO_CODE"]), sEQSGID + ":" + sPROCID))
                        return true;
                }
            }
            return false;
        }

        private void GetDefectLevel()
        {
            try
            {
                string[] Level = { "LV1", "LV2", "LV3" };

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LV_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();


                DataTable dtAddAll = new DataTable();
                dtAddAll.Columns.Add("CHK", typeof(string));
                dtAddAll.Columns.Add("LV_NAME", typeof(string));
                dtAddAll.Columns.Add("LV_CODE", typeof(string));

                DataRow AddData = dtAddAll.NewRow();

                for (int i = 0; i < Level.Count(); i++)
                {
                    AddData["CHK"] = 0;
                    AddData["LV_NAME"] = "ALL";
                    AddData["LV_CODE"] = "ALL";
                    dtAddAll.Rows.Add(AddData);

                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _PROCID;
                    Indata["LV_CODE"] = Level[i];

                    IndataTable.Rows.Add(Indata);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC_LEVEL", "RQSTDT", "RSLTDT", IndataTable);

                    dtAddAll.Merge(dtResult);

                    if (i == 0)
                        Util.GridSetData(dgLevel1, dtAddAll, FrameOperation, true);
                    else if (i == 1)
                        Util.GridSetData(dgLevel2, dtAddAll, FrameOperation, true);
                    else if (i == 2)
                        Util.GridSetData(dgLevel3, dtAddAll, FrameOperation, true);

                    IndataTable.Clear();
                    dtAddAll.Clear();
                }
            }
            catch (Exception ex)
            {
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
                inDataTable.Columns.Add("CODE", typeof(string));

                //Modify 2016.12.19 물청 TOP/BACK 구분 없음(CHARGE2_GRID 삭제) *************************************************************
                List<C1DataGrid> lst = new List<C1DataGrid> { dgWipReason };
                //**************************************************************************************************************************
                foreach (C1DataGrid dg in lst)
                {
                    DataRow Indata = inDataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _PROCID;
                    Indata["LOTID"] = _LOTID; // LOTID -> _LOTID로 변경, 아니면 위에 MIXER일때 EQPT_END시점에서 PR_LOTID를 LOTID로 넣어주는것때문에 제대로 조회 불가 ( 2017-01-26 )

                    Indata["RESNPOSITION"] = null;

                    if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                        Indata["CODE"] = "BAS";

                    inDataTable.Rows.Clear();
                    inDataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                    if (dg.Visibility == Visibility.Visible)
                        Util.GridSetData(dg, dt, FrameOperation, true);                    
                }
                GetSumDefectQty();
                SetCauseTitle();
                SetExceedLength();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectListMultiLane()
        {
            try
            {
                string childLotId;

                // 소형/자동차 전극 사용을 체크박스로 분리 [2017-01-23] CR-55
                bool isElecProdType = chkSum.IsChecked == true ? true : false;

                Util.gridClear(dgWipReason);

                // SET LANE COMBO 설정
                cboLaneNum.Items.Clear();

                CheckBox allCheck = new CheckBox();
                allCheck.IsChecked = true;
                allCheck.Content = Util.NVC("-ALL-");
                allCheck.Tag = "ALL";
                allCheck.Checked += OnLaneChecked;
                allCheck.Unchecked += OnLaneChecked;
                cboLaneNum.Items.Add(allCheck);

                for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.IsChecked = true;
                    checkBox.Content = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LANE_NUM"]);
                    checkBox.Tag = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);
                    checkBox.Checked += OnLaneChecked;
                    checkBox.Unchecked += OnLaneChecked;
                    cboLaneNum.Items.Add(checkBox);
                }
                cboLaneNum.Text = ObjectDic.Instance.GetObjectName("Lane선택");

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = _PROCID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable totalWipReason = searchResult.DataSet.Tables["OUTDATA"];
                        DataTable dsWipReason = totalWipReason.DefaultView.ToTable(false, "RESNCODE", "ACTID", "ACTNAME", "RESNNAME", "PRCS_ITEM_CODE", "RSLT_EXCL_FLAG", "RESNTOTQTY", "PARTNAME", "TAG_CONV_RATE");

                        Util.GridSetData(dgWipReason, dsWipReason, FrameOperation);

                        if (dgWipReason.Rows.Count > 0)
                        {
                            if (_DT_OUT_PRODUCT.Rows.Count > 0)
                            {
                                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                                //int iCount = isResnCountUse == true ? 1 : 0;

                                // CONV_RATE기준으로 반영된 컬럼 삭제 [2017-02-15]
                                for (int i = dgWipReason.Columns.Count - 1; i > dgWipReason.Columns["TAG_CONV_RATE"].Index; i--)
                                    dgWipReason.Columns.RemoveAt(i);

                                if (_DT_OUT_PRODUCT.Rows.Count > 0)
                                {
                                    string sMessageDic = ObjectDic.Instance.GetObjectName("태그수");
                                    string sMessageNum = ObjectDic.Instance.GetObjectName("횟수");
                                    string sObjDicAll = ObjectDic.Instance.GetObjectName("전체");
                                    double defectQty = 0;
                                    double lossQty = 0;
                                    double chargeQty = 0;

                                    for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                                    {
                                        childLotId = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);

                                        // 소형 ( SUM, 태그수 X ), 자동차 ( ALL, 태그수 O ) (2017-01-23) CR-54
                                        if (i == 0)
                                        {
                                            if (isElecProdType)
                                                Util.SetGridColumnNumeric(dgWipReason, "ALL", null, "SUM", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                            else
                                                Util.SetGridColumnNumeric(dgWipReason, "ALL", null, "ALL", true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);
                                        }
                                        
                                        if (isResnCountUse == true)
                                        {
                                            // 횟수
                                            Util.SetGridColumnNumeric(dgWipReason, childLotId + "NUM", null, sMessageNum,
                                                true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);
                                        }

                                        // 태그수
                                        Util.SetGridColumnNumeric(dgWipReason, childLotId + "CNT", null, sMessageDic,
                                            true, true, false, false, 60, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed, "F0", false, false);

                                        // Lane
                                        Util.SetGridColumnNumeric(dgWipReason, childLotId, null, Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LANE_NUM"]),
                                                true, true, false, false, 50, HorizontalAlignment.Right, Visibility.Visible, GetUnitFormatted(), false, false);

                                        // 전체
                                        Util.SetGridColumnCheckbox(dgWipReason, childLotId + "RESN_TOT_CHK", null, sObjDicAll,
                                                true, false, false, false, 50, HorizontalAlignment.Right, isElecProdType == false ? Visibility.Visible : Visibility.Collapsed);

                                        // 최초 자동 실적 활동사유수량
                                        Util.SetGridColumnNumeric(dgWipReason, childLotId + "FRST_AUTO_RSLT_RESNQTY", null, "FRST_AUTO_RSLT_RESNQTY",
                                                false, false, false, true, 50, HorizontalAlignment.Right, Visibility.Collapsed, GetUnitFormatted(), false, false);

                                        // Lane Dfct Sum
                                        DataGridAggregate.SetAggregateFunctions(dgWipReason.Columns[childLotId], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});

                                        if (dgWipReason.Rows.Count == 0)
                                            continue;

                                        DataTable dt = GetDefectDataByLot(childLotId);

                                        if (dt != null)
                                        {
                                            for (int j = 0; j < dt.Rows.Count; j++)
                                            {
                                                // 횟수
                                                if (isResnCountUse == true)
                                                {
                                                    if (dt.Rows[j]["COUNTQTY"].ToString().Equals(""))
                                                        BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 4, null);
                                                    else
                                                        BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 4, dt.Rows[j]["COUNTQTY"]);
                                                }

                                                // Tag
                                                if (dt.Rows[j]["DFCT_TAG_QTY"].ToString().Equals(""))
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 3, 0);
                                                else
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 3, dt.Rows[j]["DFCT_TAG_QTY"]);

                                                // Lane
                                                if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 2, 0);
                                                else
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 2, dt.Rows[j]["RESNQTY"]);

                                                // 전체
                                                if (Util.NVC(dt.Rows[j]["RESN_TOT_CHK"]).Equals("1"))
                                                {
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 1, 1);
                                                    DataTableConverter.SetValue(dgWipReason.Rows[j].DataItem, childLotId + "RESN_TOT_CHK", true);
                                                }
                                                else
                                                {
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count - 1, 0);
                                                    DataTableConverter.SetValue(dgWipReason.Rows[j].DataItem, childLotId + "RESN_TOT_CHK", false);
                                                }

                                                // 최초 자동 실적 활동사유수량
                                                if (dt.Rows[j]["FRST_AUTO_RSLT_RESNQTY"].ToString().Equals(""))
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count, 0);
                                                else
                                                    BindingDataGrid(dgWipReason, j, dgWipReason.Columns.Count, dt.Rows[j]["FRST_AUTO_RSLT_RESNQTY"]);
                                            }
                                        }

                                        // SLITTER는 CUT단위로 변경되면서 CUT단위는 합산수량, 그 외는 개별로 처리 [2017-01-21] CR-53
                                        DataTable distinctDt = DataTableConverter.Convert(dgWipReason.ItemsSource).DefaultView.ToTable(true, "ACTID");
                                        foreach (DataRow _row in distinctDt.Rows)
                                        {
                                            if (string.Equals(_row["ACTID"], "DEFECT_LOT"))
                                                defectQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "LOSS_LOT"))
                                                lossQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                            else if (string.Equals(_row["ACTID"], "CHARGE_PROD_LOT"))
                                                chargeQty += SumDefectQty(dgWipReason, i, childLotId, Util.NVC(_row["ACTID"]));
                                        }

                                        //if (i == (_DT_OUT_PRODUCT.Rows.Count - 1))
                                        //{
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_DEFECT", defectQty);
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_LOSS", lossQty);
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_CHARGEPRD", chargeQty);
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_DEFECT_SUM", (defectQty + lossQty + chargeQty));
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY", (Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "INPUTQTY")) * LOTINFO_GRID.GetRowCount()) - Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "DTL_DEFECT_SUM")));
                                        //    DataTableConverter.SetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODPTNQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "GOODQTY")) * Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[LOTINFO_GRID.TopRows.Count].DataItem, "LANE_QTY")));
                                        //}
                                    }
                                }

                                // COST CENTER 컬럼 맨 뒤로 오게하는 요청으로 해당 컬럼 제일 하위에서 생성 [2017-02-07]
                                // 전 공정 횟수 관리를 위하여 컬럼 추가 (C20190416_75868 ) [2019-04-17]
                                Util.SetGridColumnText(dgWipReason, "COSTCENTERID", null, "COSTCENTERID", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "COSTCENTER", null, "COSTCENTER", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "TAG_ALL_APPLY_FLAG", null, "TAG_ALL_APPLY_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "DFCT_QTY_CHG_BLOCK_FLAG", null, "DFCT_QTY_CHG_BLOCK_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "AREA_RESN_CLSS_NAME1", null, "AREA_RESN_CLSS_NAME1", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "AREA_RESN_CLSS_NAME2", null, "AREA_RESN_CLSS_NAME2", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                Util.SetGridColumnText(dgWipReason, "AREA_RESN_CLSS_NAME3", null, "AREA_RESN_CLSS_NAME3", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);

                                if (isResnCountUse == true)
                                {
                                    Util.SetGridColumnText(dgWipReason, "WRK_COUNT_MNGT_FLAG", null, "WRK_COUNT_MNGT_FLAG", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                    Util.SetGridColumnText(dgWipReason, "LINK_DETL_RSN_CODE_TYPE", null, "LINK_DETL_RSN_CODE_TYPE", true, true, true, true, C1.WPF.DataGrid.DataGridLength.Auto, HorizontalAlignment.Center, Visibility.Collapsed);
                                }

                                for (int i = 0; i < totalWipReason.Rows.Count; i++)
                                {
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["COSTCENTERID"].Index + 1, totalWipReason.Rows[i]["COSTCENTERID"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["COSTCENTER"].Index + 1, totalWipReason.Rows[i]["COSTCENTER"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["TAG_ALL_APPLY_FLAG"].Index + 1, totalWipReason.Rows[i]["TAG_ALL_APPLY_FLAG"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["DFCT_QTY_CHG_BLOCK_FLAG"].Index + 1, totalWipReason.Rows[i]["DFCT_QTY_CHG_BLOCK_FLAG"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["AREA_RESN_CLSS_NAME1"].Index + 1, totalWipReason.Rows[i]["AREA_RESN_CLSS_NAME1"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["AREA_RESN_CLSS_NAME2"].Index + 1, totalWipReason.Rows[i]["AREA_RESN_CLSS_NAME2"]);
                                    BindingDataGrid(dgWipReason, i, dgWipReason.Columns["AREA_RESN_CLSS_NAME3"].Index + 1, totalWipReason.Rows[i]["AREA_RESN_CLSS_NAME3"]);

                                    if (isResnCountUse == true)
                                    {
                                        BindingDataGrid(dgWipReason, i, dgWipReason.Columns["WRK_COUNT_MNGT_FLAG"].Index + 1, totalWipReason.Rows[i]["WRK_COUNT_MNGT_FLAG"]);
                                        BindingDataGrid(dgWipReason, i, dgWipReason.Columns["LINK_DETL_RSN_CODE_TYPE"].Index + 1, totalWipReason.Rows[i]["LINK_DETL_RSN_CODE_TYPE"]);
                                    }
                                }

                                // ROW별 합산 수량 반영 [2017-02-15]
                                double rowSum = 0;
                                for (int i = 0; i < dgWipReason.Rows.Count; i++)
                                {
                                    rowSum = 0;
                                    for (int j = dgWipReason.Columns["ALL"].Index; j < dgWipReason.Columns["COSTCENTERID"].Index; j++)
                                        if (!Util.NVC(dgWipReason.Columns[j].Name).Equals("ALL") &&
                                            !Util.NVC(dgWipReason.Columns[j].Name).EndsWith("NUM") &&
                                            !Util.NVC(dgWipReason.Columns[j].Name).EndsWith("CNT") &&
                                            !Util.NVC(dgWipReason.Columns[j].Name).EndsWith("RESN_TOT_CHK") &&
                                            !Util.NVC(dgWipReason.Columns[j].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                            rowSum += Convert.ToDouble(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[j].Name));

                                    DataTableConverter.SetValue(dgWipReason.Rows[i].DataItem, "RESNTOTQTY", rowSum);
                                }
                                Util.GridSetData(dgWipReason, DataTableConverter.Convert(dgWipReason.ItemsSource), FrameOperation, true);
                                SetExceedLength();
                                dgWipReason.Refresh(false);
                                dgWipReason.FrozenColumnCount = dgWipReason.Columns["ALL"].Index + 1;

                                // S/L공정의 경우 TAG 불량/LOSS 자동 반영을 위하여 하기와 같이 이전 값을 DataTable로 보관(C20190404_67447)  [2019-04-11]
                                dtWipReasonBak = DataTableConverter.Convert(dgWipReason.ItemsSource);
                            }

                            #region #전수불량 Lane 등록
                            _dtDEFECTLANENOT = getDefectLaneLotList(_CUTID);
                            _DEFECTLANELOT = _dtDEFECTLANENOT.Select("CHK = 1");
                            SetDisableWipReasonGrid(_DEFECTLANELOT);
                            #endregion
                        }
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetQualityList(object SelectedItem)
        {
            try
            {
                DataTable _topDT = new DataTable();
                DataTable _backDT = new DataTable();

                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(dgQuality);

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
                Indata["PROCID"] = _PROCID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
                Indata["CLCT_PONT_CODE"] = _PROCID.Equals(Process.COATING) || _PROCID.Equals(Process.INS_COATING) ? "T" : null;

                if (!string.IsNullOrEmpty(txtVersion.Text))
                    Indata["VER_CODE"] = txtVersion.Text;

                if (txtCurLaneQty != null && txtCurLaneQty.Value > 0)
                    Indata["LANEQTY"] = txtCurLaneQty.Value;
                IndataTable.Rows.Add(Indata);

                _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                if (_topDT.Rows.Count == 0)
                {
                    _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(dgQuality, _topDT, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgQuality, _topDT, FrameOperation, true);
                }
                _Util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);

                if (dgQuality2.Visibility == Visibility.Visible)
                {
                    Util.gridClear(dgQuality2);

                    DataTable backTable = new DataTable();
                    backTable.Columns.Add("LANGID", typeof(string));
                    backTable.Columns.Add("AREAID", typeof(string));
                    backTable.Columns.Add("PROCID", typeof(string));
                    backTable.Columns.Add("LOTID", typeof(string));
                    backTable.Columns.Add("WIPSEQ", typeof(string));
                    backTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    backTable.Columns.Add("VER_CODE", typeof(string));
                    backTable.Columns.Add("LANEQTY", typeof(Int16));

                    DataRow backdata = backTable.NewRow();
                    backdata["LANGID"] = LoginInfo.LANGID;
                    backdata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    backdata["PROCID"] = _PROCID;
                    backdata["LOTID"] = rowview["LOTID"].ToString();
                    backdata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
                    backdata["CLCT_PONT_CODE"] = _PROCID.Equals(Process.COATING) || _PROCID.Equals(Process.INS_COATING) ? "B" : null;

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                        backdata["VER_CODE"] = txtVersion.Text;

                    if (_LANEQTY != null && Convert.ToInt16(_LANEQTY) > 0)
                        backdata["LANEQTY"] = _LANEQTY;

                    backTable.Rows.Add(backdata);

                    _backDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", backTable);

                    if (_backDT.Rows.Count == 0)
                    {
                        _backDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", backTable);
                    } 

                    Util.GridSetData(dgQuality2, _backDT, FrameOperation, true);
                    _Util.SetDataGridMergeExtensionCol(dgQuality2, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("RESNPOSITION", typeof(string));
                IndataTable.Columns.Add("CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = LotId;
                Indata["RESNPOSITION"] = null;

                if (string.Equals(LoginInfo.CFG_AREA_ID, "E6"))
                    Indata["CODE"] = "BAS";

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", IndataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetCottonList(object SelectedItem)
        {
            DataTable _cottonDT = new DataTable();

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(dgCotton);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("CUT_ID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["CUT_ID"] = rowview["CUT_ID"].ToString();
            Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();
            IndataTable.Rows.Add(Indata);

            _cottonDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_CHK_DATA", "INDATA", "RSLTDT", IndataTable);
            Util.GridSetData(dgCotton, _cottonDT, FrameOperation, true);
        }

        private void GetRemarkHistory(int iRow)
        {
            try
            {
                Util.gridClear(dgRemarkHistory);

                String sLotID = String.Empty;
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID_PR")).Equals(""))
                    sLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                else
                    sLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID_PR"));

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["LOTID"] = sLotID;
                inDataTable.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_HISTORY_WIPNOTE", "INDATA", "RSLTDT", inDataTable);

                // 필요정보 변환
                System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                foreach (DataRow row in dtResult.Rows)
                {
                    strBuilder.Clear();
                    string[] wipNotes = Util.NVC(row["WIPNOTE"]).Split('|');

                    for (int i = 0; i < wipNotes.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(wipNotes[i]))
                        {
                            if (i == 0)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                            else if (i == 1)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                            else if (i == 2)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                            else if (i == 3)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                            else if (i == 4)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                            else if (i == 5)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                            strBuilder.Append("\n");
                        }
                    }
                    row["WIPNOTE"] = strBuilder.ToString();
                }
                Util.GridSetData(dgRemarkHistory, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private DataTable getDefectLaneLotList(string sCUTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUT_ID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CUT_ID"] = sCUTID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DFCT_LANE_SL", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            if ((string.Equals(_PROCID, Process.ROLL_PRESSING) || (string.Equals(_PROCID, Process.SLITTING))) && string.Equals(getQAInspectFlag(sLotID), "Y"))
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };
            }

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private string getQAInspectFlag(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(result.Rows[0]["QA_INSP_TRGT_FLAG"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private void StartCancelProcess()
        {
            try
            {
                if (_WIPSTAT.Equals("WAIT"))
                    return;

                if (_WIPSTAT.Equals("EQPT_END"))
                {
                    Util.MessageValidation("SFU1671");  //설비 완공 LOT은 취소할 수 없습니다.
                    return;
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        #region Lot Info
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow inDataRow = null;

                        for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                        {
                            inDataRow = null;
                            inDataRow = inDataTable.NewRow();

                            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inDataRow["EQPTID"] = _EQPTID;
                            inDataRow["LOTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);
                            if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                if (_DT_OUT_PRODUCT.Columns["OUT_CSTID"] != null)
                                {
                                    inDataRow["OUT_CSTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["OUT_CSTID"]);
                                }
                            }

                            inDataRow["USERID"] = LoginInfo.USERID;
                            inDataRow["INPUT_LOTID"] = _LOTIDPR;
                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                if (_DT_OUT_PRODUCT.Columns["CSTID"] != null)
                                {
                                    inDataRow["CSTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["CSTID"]);
                                }
                            }
                            inDataTable.Rows.Add(inDataRow);
                        }
                        #endregion

                        // Add : 2016.12.10, Slitter, SRS Slitter 작업시작 취소 추가 *******************************
                        if (_PROCID.Equals(Process.SLITTING) || _PROCID.Equals(Process.SRS_SLITTING))
                        {
                            #region Parent Lot
                            DataRow inLotDataRow = null;

                            DataTable InLotdataTable = inDataSet.Tables.Add("IN_PRLOT");
                            InLotdataTable.Columns.Add("PR_LOTID", typeof(string));
                            InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                            InLotdataTable.Columns.Add("CSTID", typeof(string));

                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["PR_LOTID"] = _LOTIDPR;
                            inLotDataRow["CUT_ID"] = _CUTID;

                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                            {
                                inLotDataRow["CSTID"] = _CSTID;
                            }
                            InLotdataTable.Rows.Add(inLotDataRow);
                            #endregion
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_SL", _PROCID.Equals(Process.SLITTING) || _PROCID.Equals(Process.SRS_SLITTING) ? "INDATA,IN_PRLOT" : "INDATA", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetInitProductQty()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));

            DataRow inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["LOTID"] = _LOTID;
            inLotDetailDataRow["PROD_QTY"] = DBNull.Value;
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("DA_BAS_UPD_WIPATTR_FOR_PROD_QTY", "INDATA", null, inDataTable, (result, Returnex) =>
            {
                try
                {
                    if (Returnex != null)
                        return;
                }
                catch (Exception ex) { }
            });
        }

        private void EqptEndCancelProcess()
        {
            try
            {
                if (_WIPSTAT.Equals("WAIT"))
                    return;

                if (_WIPSTAT.Equals("PROC"))
                {
                    Util.MessageValidation("SFU3464");  //진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                    return;
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["PROCID"] = _PROCID;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                        InLotdataTable.Columns.Add("WIPNOTE", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();

                        if (_PROCID.Equals(Process.SLITTING) || _PROCID.Equals(Process.SRS_SLITTING) || _PROCID.Equals(Process.HALF_SLITTING))
                            inLotDataRow["CUT_ID"] = _CUTID;
                        else
                            inLotDataRow["LOTID"] = _LOTID;

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void EndCancelProcess()
        {
            try
            {
                if (!string.Equals(_WIPSTAT, "END"))
                {
                    Util.MessageValidation("SFU5146", new object[] { _CUTID });  //해당 Lot[%1]은 실적확정 상태가 아니라 취소 할 수 없습니다.
                    return;
                }

                if (_Util.IsCommonCodeUse("ELEC_CNFM_CANCEL_USER", LoginInfo.USERID) == false)
                {
                    Util.MessageValidation("SFU5148", new object[] { LoginInfo.USERID });  //해당 USER[%1]는 실적취소할 권한이 없습니다. (시스템 담당자에게 문의 바랍니다.)
                    return;
                }

                // 해당 확정 처리 된 Lot을 실적취소하시겠습니까?
                Util.MessageConfirm("SFU5147", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = _EQPTID;
                        inDataRow["PROCID"] = _PROCID;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();

                        if (_PROCID.Equals(Process.SLITTING) || _PROCID.Equals(Process.SRS_SLITTING) || _PROCID.Equals(Process.HALF_SLITTING))
                            inLotDataRow["CUT_ID"] = _CUTID;
                        else
                            inLotDataRow["LOTID"] = _LOTID;

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            REFRESH = true;
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool IsAbnormalLot()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            DataRow dr = dt.NewRow();
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "LOTID_PR"));
            dt.Rows.Add(dr);
            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", dt);

            if (!string.IsNullOrEmpty(result.Rows[0]["ABNORM_FLAG"].ToString()) && result.Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
            {
                return true;
            }
            else
                return false;


        }
        
        private void ConfirmDispatcher(bool bRealWorkerSelFlag = false)
        {
            // Remark 취합
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU4257"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

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

            // 다음 공정으로 이송 하시겠습니까?
            Util.MessageConfirm("SFU4257", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        // DEFECT 자동 저장
                        SaveDefect(dgWipReason);

                        #region 전수불량 Lot 불량
                        DataTable dtLaneDfct = getDefectLaneLotList(_CUTID);
                        #endregion

                        // IN_DATA
                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("IN_DATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WIP_NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("REPROC_YN", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        row["PROCID"] = _PROCID;
                        row["SHIFT"] = txtShift.Tag;
                        row["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                        row["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                        row["WIP_NOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "LOTID"))];
                        row["USERID"] = LoginInfo.USERID;
                        //row["REPROC_YN"] = chkExtraPress.IsChecked == true ? "Y" : "N";
                        inDataSet.Tables["IN_DATA"].Rows.Add(row);

                        // IN_LOT
                        DataTable InLot = inDataSet.Tables.Add("IN_LOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(decimal));
                        InLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLot.Columns.Add("RESNQTY", typeof(decimal));
                        InLot.Columns.Add("HOLD_YN", typeof(string));

                        for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                        {
                            string sLotID = string.Empty;
                            double laneQty = 0;
                            double totInputQty = 0;
                            double dfctQty = 0;

                            sLotID = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);
                            totInputQty = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")));
                            laneQty = _DT_OUT_PRODUCT.Rows.Count;
                            
                            // Lane별 불량 sum
                            if (dgWipReason.GetRowCount() > 0)
                                for (int k = 0; k < dgWipReason.GetRowCount(); k++)
                                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[k].DataItem, sLotID))))
                                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[k].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                                            dfctQty += Convert.ToDouble(DataTableConverter.GetValue(dgWipReason.Rows[k].DataItem, sLotID));
                            

                            row = InLot.NewRow();
                            row["LOTID"] = sLotID;
                            row["INPUTQTY"] = totInputQty / laneQty;
                            row["OUTPUTQTY"] = (totInputQty / laneQty) - dfctQty;
                            
                            DataRow[] drDfctLane = dtLaneDfct.Select("CHK = 1 AND LOTID = '" + sLotID + "'");
                            if (drDfctLane?.Length > 0)
                                row["RESNQTY"] = 0;
                            else
                                row["RESNQTY"] = dfctQty;
                                               //+ Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_LEN_LACK")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_LEN_LACK")));
                            //row["HOLD_YN"] = _IS_POSTING_HOLD;

                            inDataSet.Tables["IN_LOT"].Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NEXT_PROC_MOVE", "IN_DATA,IN_LOT", null, (result, searchException) =>
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            isConfirm = true;
                            REFRESH = true;
                        }, inDataSet);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            });
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

        private DataTable GetMergeInfo(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_HIST", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }
        
        private string GetEqptCurrentMtrl(string sEqptID)
        {
            try
            {
                string MountMTRL = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                    return "";

                MountMTRL = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                return MountMTRL;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private bool IsFinalProcess()
        {
            // 현재 작업중인 LOT이 마지막 공정인지 체크 [2017-02-16]
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = _LOTIDPR;
            indata["LOTID"] = _LOTID;
            indata["PROCID"] = _PROCID;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                return true;

            return false;
        }

        private string GetCoaterMaxVersion()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "PRODID"));
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_MAX_VERSION", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return Util.NVC(dtMain.Rows[0][0]);

            }
            catch (Exception ex) { }

            return "";
        }

        private void SaveWIPHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS1QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS2QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS3QTY", typeof(decimal));
            inDataTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            //RFID 관련 추가
            inDataTable.Columns.Add("OUT_CSTID", typeof(string));

            for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
            {
                DataRow inLotDetailDataRow = null;
                inLotDetailDataRow = inDataTable.NewRow();
                inLotDetailDataRow["LOTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);
                inLotDetailDataRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                inLotDetailDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                inLotDetailDataRow["WIPNOTE"] = txtWipNote != null ? Util.NVC(txtWipNote.Text) : null;
                inLotDetailDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                inLotDetailDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);

                //if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                //    inLotDetailDataRow["PROD_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY"));
                //else
                //    inLotDetailDataRow["PROD_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY"));

                inLotDetailDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inLotDetailDataRow);

            }

            new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inDataTable, (result, Returnex) =>
            {
                try
                {
                    if (Returnex != null)
                    {
                        Util.MessageException(Returnex);
                        return;
                    }
                    
                    SaveDefect(dgWipReason);                    
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void SetProcWipReasonData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CUTID", typeof(string));
                dt.Columns.Add("WIPSEQ", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["CUTID"] = _CUTID;

                int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    dataRow["WIPSEQ"] = 1;
                else
                    dataRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_WIPREASON", "INDATA", "RSLTDT", dt);

                if (procResnDt == null)
                    procResnDt = new DataTable();

                procResnDt.Clear();
                procResnDt = result.Copy();

                if (result != null && result.Rows.Count > 0)
                {
                    // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                    //int iCount = isResnCountUse == true ? 1 : 0;

                    for (int i = dgWipReason.Columns["ALL"].Index; i < dgWipReason.Columns["COSTCENTERID"].Index; i++)
                    {
                        if (Util.NVC(dgWipReason.Columns[i].Name).Equals("ALL") ||
                            Util.NVC(dgWipReason.Columns[i].Name).EndsWith("NUM") ||
                            Util.NVC(dgWipReason.Columns[i].Name).EndsWith("CNT") ||
                            Util.NVC(dgWipReason.Columns[i].Name).EndsWith("RESN_TOT_CHK") ||
                            Util.NVC(dgWipReason.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY")) continue;

                        DataRow[] rows = result.Select("LOTID = '" + dgWipReason.Columns[i].Name + "'");
                        for (int j = 0; j < dgWipReason.Rows.Count; j++)
                        {
                            if (rows.Length == 0)
                                break;

                            foreach (DataRow row in rows)
                            {
                                if (string.Equals(dgWipReason.Columns[i].Name, row["LOTID"]) && string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "RESNCODE"), row["RESNCODE"]))
                                {
                                    DataTableConverter.SetValue(dgWipReason.Rows[j].DataItem, dgWipReason.Columns[i].Name, row["RESNQTY"]);
                                    GetSumCutDefectQty(dgWipReason, j, i);
                                    continue;
                                }
                            }
                        }
                    }
                    dgWipReason.Refresh(false);
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveCutQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataSet inCollectLot = dtDataCollectOfChildrenQuality(dg);

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPDATACOLLECT_CUTID", "INDATA,IN_DATA", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1998");    //품질 정보가 저장되었습니다.
                isChangeQuality = false;
            }, inCollectLot);
        }

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

                    if (dgRemark.Rows[0].Visibility == Visibility.Visible)
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
                    isChangeRemark = false;
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SavePublicCutQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataSet inCollectLot = dtDataCollectOfChildrenQuality(dg);

            new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPDATACOLLECT_CUTID", "INDATA,IN_DATA", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                isChangeQuality = false;
            }, inCollectLot);
        }

        private void SaveCotton(C1DataGrid dg)
        {
            string[] SLIT_CHK = new string[5];
            SLIT_CHK[0] = "SLIT_CHK_001";
            SLIT_CHK[1] = "SLIT_CHK_002";
            SLIT_CHK[2] = "SLIT_CHK_003";
            SLIT_CHK[3] = "SLIT_CHK_004";
            SLIT_CHK[4] = "SLIT_CHK_005";

            DataSet inDataSet = new DataSet();
            DataTable dtCotton = (dg.ItemsSource as DataView).Table;

            DataRow inDataRow = null;
            DataTable IndataTable = inDataSet.Tables.Add("INDATA");
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("NOTE", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            inDataRow = IndataTable.NewRow();
            inDataRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[0].DataItem, "WIPSEQ"));
            inDataRow["NOTE"] = "";
            inDataRow["USERID"] = LoginInfo.USERID;

            IndataTable.Rows.Add(inDataRow);

            DataRow inDataRow2 = null;
            DataTable IndataTable2 = inDataSet.Tables.Add("IN_LOT");
            IndataTable2.Columns.Add("LOTID", typeof(string));
            IndataTable2.Columns.Add("SLIT_CHK_ITEM_CODE", typeof(string));
            IndataTable2.Columns.Add("CHK_RSLT", typeof(string));

            for (int k = 0; k < dtCotton.Rows.Count; k++)
            {
                for (int i = 0; i < SLIT_CHK.Length; i++)
                {
                    inDataRow2 = IndataTable2.NewRow();
                    string schk = dtCotton.Rows[k][SLIT_CHK[i]].ToString();
                    if (schk.Equals("True"))
                    {
                        schk = "Y";
                    }
                    else
                    {
                        if (schk.Equals("False"))
                        {
                            schk = "";
                        }
                    }

                    inDataRow2["LOTID"] = dtCotton.Rows[k]["LOTID"].ToString();
                    inDataRow2["SLIT_CHK_ITEM_CODE"] = SLIT_CHK[i].ToString();
                    inDataRow2["CHK_RSLT"] = schk;
                    IndataTable2.Rows.Add(inDataRow2);
                }
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SLIT_CHK_DATA", "INDATA,IN_LOT", null, inDataSet);
                dgCotton.EndEdit(true);
                isChangeCotton = false;
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private bool ValidConfirmLotCheck()
        {
            if (_DT_OUT_PRODUCT.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                {
                    DataRow Indata = IndataTable.NewRow();
                    Indata["LOTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);

                    IndataTable.Rows.Add(Indata);
                }

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", IndataTable);

                DataRow[] dtRows = dt?.Select("PROCID <> '" + _PROCID + "' AND WIP_TYPE_CODE IN ('IN', 'INOUT')");

                if (dtRows?.Length > 0)
                    return false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        private void GetMergeList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dataRow["PROCID"] = _PROCID;
                dataRow["PRODID"] = _PRODID;
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_LIST_V01", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgWipMerge, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetMergeEndList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dataRow["PROCID"] = _PROCID;
                dataRow["LOTID"] = txtMergeInputLot.Text;
                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_END_LIST", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgWipMerge2, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveMergeData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["LOTID"] = Util.NVC(txtMergeInputLot.Text);
                        row["NOTE"] = string.Empty;
                        row["USERID"] = LoginInfo.USERID;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable formLotID = indataSet.Tables.Add("IN_FROMLOT");
                        formLotID.Columns.Add("FROM_LOTID", typeof(string));

                        DataTable dt = ((DataView)dgWipMerge.ItemsSource).Table;
                        decimal iAddSumQty = 0;

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                row = formLotID.NewRow();

                                iAddSumQty += Util.NVC_Decimal(inRow["WIPQTY"]);
                                row["FROM_LOTID"] = Util.NVC(inRow["LOTID"]);
                                indataSet.Tables["IN_FROMLOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MERGE_LOT", "INDATA,IN_FROMLOT", null, indataSet);

                        Util.MessageInfo("SFU2009");    //합권되었습니다.
                        REFRESH = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        #endregion

        #region Function

        public void SetApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnStart);
            listAuth.Add(btnCancel);
            listAuth.Add(btnEqptEnd);
            listAuth.Add(btnEndCancel);
            //listAuth.Add(btnConfirm);
            listAuth.Add(btnDispatch);

            listAuth.Add(btnProcResn);
            listAuth.Add(btnSaveWipReason);
            listAuth.Add(btnSaveQuality);
            listAuth.Add(btnSaveMaterial);
            listAuth.Add(btnDeleteMaterial);
            listAuth.Add(btnAddMaterial);
            listAuth.Add(btnRemoveMaterial);
            listAuth.Add(btnSaveRemark);
            listAuth.Add(btnSaveColor);
            listAuth.Add(btnSaveWipHistory);
            listAuth.Add(btnShift);

            listAuth.Add(btnCancelDelete);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetButtons()
        {
        }

        private void SetCheckBox()
        {
            chkWait.Checked += OnCheckBoxChecked;
            chkWait.Unchecked += OnCheckBoxChecked;
            
            chkRun.Checked += OnCheckBoxChecked;
            chkRun.Unchecked += OnCheckBoxChecked;

            chkEqpEnd.Checked += OnCheckBoxChecked;
            chkEqpEnd.Unchecked += OnCheckBoxChecked;

            chkConfirm.Checked += OnCheckBoxChecked;
            chkConfirm.Unchecked += OnCheckBoxChecked;

            chkWoProduct.Checked += OnCheckBoxChecked;
            chkWoProduct.Unchecked += OnCheckBoxChecked;
        }

        private void SetComboBox()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { _PROCID, null, "A" };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

            spSingleCoater.Visibility = Visibility.Collapsed;
            cboCoatType.Visibility = Visibility.Collapsed;

            // 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
            SetWipColorLegendCombo();
        }

        private void SetWipColorLegendCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow inRow = inTable.NewRow();
            inRow["LANGID"] = LoginInfo.LANGID;
            inRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            inRow["PROCID"] = Process.SLITTING;

            inTable.Rows.Add(inRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            WIPCOLORLEGEND = dtResult;
        }

        private void SetGrids()
        {
            dgQuality2.Visibility = Visibility.Collapsed;
            gridWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
        }

        private void SetTabItems()
        {
            tiSlurry.Visibility = Visibility.Collapsed;
            tiInputMaterial.Visibility = Visibility.Collapsed;            
            lblQualityTop.Visibility = Visibility.Collapsed;
            lblQualityBack.Visibility = Visibility.Collapsed;
            tiCotton.Visibility = Visibility.Collapsed;
        }

        private void SetResultDetail()
        {
            int elementCountPerRow = 3;
            int totalRowCount = 0;
            int rowIndex = 0;
            int colIndex = 0;

            List<ResultElement> elemList = new List<ResultElement>();

            #region #전수불량 Lane 등록
            //elemList = ResultElementList.SlitterList(elementCountPerRow);
            elemList = ResultElementList.DefectLaneSlitterList(elementCountPerRow);
            #endregion

            totalRowCount = elemList.Count + 2;

            Grid grdElement = ResultDetail;
            grdElement.Children.Clear();

            int mod = totalRowCount / elementCountPerRow;
            int rem = totalRowCount % elementCountPerRow;

            for (int i = 0; i < mod; i++)
            {
                var rowDef = new RowDefinition();

                if (elemList[elemList.Count - 2].Control.Name.Equals("dgColorTag") || elemList[elemList.Count - 1].Control.Name.Equals("dgColorTag") || elemList[elemList.Count - 1].Control.Name.Equals("txtNote"))
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                else
                    rowDef.Height = new GridLength(30);

                grdElement.RowDefinitions.Add(rowDef);
            }

            if (rem > 0 && elemList[elemList.Count - 1].SpaceInCharge > 1)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = elemList[elemList.Count - 1].SpaceInCharge > 1 ? new GridLength(1, GridUnitType.Star) : new GridLength(30);
                grdElement.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < elementCountPerRow; i++)
            {
                var colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                grdElement.ColumnDefinitions.Add(colDef);
            }

            foreach (ResultElement re in elemList)
            {
                if (re.Control.Name.Equals("txtVersion"))
                {
                    txtVersion = re.Control as TextBox;

                    if (re.PopupButton != null)
                        re.PopupButton.Click += OnClickVersion;
                }
                else if (re.Control.Name.Equals("txtLaneQty"))
                {
                    txtLaneQty = re.Control as C1NumericBox;

                    if (txtLaneQty.IsEnabled)
                    {
                        txtLaneQty.KeyDown += OnKeyDownLaneQty;
                        txtLaneQty.LostFocus += OnKeyLostFocusLaneQty;
                    }
                }
                #region #전수불량 Lane 등록
                else if (re.Control.Name.Equals("txtCurLaneQty"))
                {
                    txtCurLaneQty = re.Control as C1NumericBox;
                }
                #endregion                
                else if (re.Control.Name.Equals("txtWorkDate"))
                {
                    txtWorkDate = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtNote"))
                {
                    txtWipNote = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtStartDateTime"))
                {
                    txtStartDateTime = re.Control as TextBox;
                }
                else if (re.Control.Name.Equals("txtEndDateTime"))
                {
                    txtEndDateTime = re.Control as TextBox;
                }                

                if (re.Control is C1NumericBox)
                    re.Control.Style = Application.Current.Resources["C1NumericBoxStyle"] as Style;

                if (re.PopupButton != null)
                {
                    if (re.Control.Name.Equals("dgColorTag"))
                    {
                        //COLORTAG_GRID = re.Control as C1DataGrid;
                        re.PopupButton.Click += OnClickSaveColorTag;
                    }

                    re.PopupButton.Style = re.Control.Name.Equals("dgColorTag") ? Application.Current.Resources["Content_SaveButtonStyle"] as Style : Application.Current.Resources["Content_SearchButtonStyle"] as Style;
                    re.PopupButton.SetValue(Grid.ColumnProperty, 2);
                }

                var panel = new Grid();

                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(70);
                panel.ColumnDefinitions.Add(cd);

                cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                panel.ColumnDefinitions.Add(cd);

                cd = new ColumnDefinition();
                cd.Width = new GridLength(23);
                panel.ColumnDefinitions.Add(cd);

                TextBlock title = new TextBlock { Text = ObjectDic.Instance.GetObjectName(re.Title), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                title.SetValue(Grid.ColumnProperty, 0);
                panel.Children.Add(title);

                re.Control.SetValue(Grid.ColumnProperty, 1);
                panel.Children.Add(re.Control);

                if (re.PopupButton != null)
                    panel.Children.Add(re.PopupButton);

                if (re.Control.Name.Equals("dgColorTag"))
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    panel.RowDefinitions.Add(rd);

                    (re.Control as C1DataGrid).Height = 60;
                    (re.Control as C1DataGrid).VerticalAlignment = VerticalAlignment.Top;
                    (re.Control as C1DataGrid).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
                else if (re.Control.Name.Equals("txtNote"))
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1, GridUnitType.Star);
                    panel.RowDefinitions.Add(rd);

                    (re.Control as TextBox).TextWrapping = TextWrapping.WrapWithOverflow;
                    (re.Control as TextBox).Height = 60;
                    (re.Control as TextBox).AcceptsReturn = true;
                    (re.Control as TextBox).VerticalAlignment = VerticalAlignment.Top;
                    (re.Control as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                if (re.Control is C1DateTimePicker)
                {
                    (re.Control as C1DateTimePicker).TimeFormat = C1TimeEditorFormat.Custom;
                    (re.Control as C1DateTimePicker).DateFormat = C1DatePickerFormat.Short;
                    (re.Control as C1DateTimePicker).CustomDateFormat = "yyyy-MM-dd";
                    (re.Control as C1DateTimePicker).CustomTimeFormat = "HH:mm";
                    (re.Control as C1DateTimePicker).EditMode = C1DateTimePickerEditMode.Date;
                    (re.Control as C1DateTimePicker).MinDate = DateTime.Now.AddDays(-1);
                    (re.Control as C1DateTimePicker).MaxDate = DateTime.Now.AddDays(1);
                    (re.Control as C1DateTimePicker).Margin = new Thickness(3, 0, 0, 0);
                }

                if (re.Control is C1NumericBox || re.Control is CheckBox)
                    re.Control.Margin = new Thickness(3, 0, 0, 0);

                if (re.SpaceInCharge.Equals(elementCountPerRow))
                {
                    if (!colIndex.Equals(0))
                        rowIndex++;

                    colIndex = 0;
                    panel.SetValue(Grid.ColumnSpanProperty, grdElement.ColumnDefinitions.Count);
                }

                panel.SetValue(Grid.RowProperty, rowIndex);
                panel.SetValue(Grid.ColumnProperty, colIndex);

                grdElement.Children.Add(panel);

                colIndex += re.SpaceInCharge;

                if (colIndex % elementCountPerRow == 0)
                    rowIndex++;

                colIndex = colIndex % elementCountPerRow == 0 ? 0 : colIndex;
            }
        }

        private void SetTextBox()
        {
            chkDefectFilter.Visibility = Visibility.Visible;
            txtLVFilter.Visibility = Visibility.Visible;
        }

        private void SetEvents()
        {
            btnStart.Click += btnStart_Click;
            btnSearch.Click += OnClickSearch;
            btnBarcodeLabel.Click += OnClickBarcode;
            btnCancel.Click += OnClickStartCancel;
            btnEqptEnd.Click += OnClickEqptEnd;
            btnEqptEndCancel.Click += OnClickEqptEndCancel;
            btnEndCancel.Click += OnClickEndCancel;
            btnDispatch.Click += btnDispatch_Click;
            btnEqptIssue.Click += OnClickEqptIssue;
            btnEqptCond.Click += OnClickEqptCond;
            btnPrintLabel.Click += OnClickPrintLabel;

            #region [Sampling]
            btnSamplingProdT1.Click += OnClickSamplingProdT1;
            #endregion

            btnExtra.MouseLeave += OnExtraMouseLeave;

            // Port별 Skid Type 설정 Evnet
            btnSkidTypeSettingByPort.Click += OnClickSkidTypeSettingByPort;
            btnManualWorkMode.Click += OnClickManualWorkMode;

            cboEquipment.SelectedItemChanged += OnSelectedItemChangedEquipmentCombo;

            dgProductLot.CurrentCellChanged += OnGridCurrentCellChanged;
            dgProductLot.LoadedCellPresenter += OnLoadedProdLotCellPresenter;
            dgLevel1.CurrentCellChanged += OnDefectCurrentChanged;
            dgLevel2.CurrentCellChanged += OnDefectCurrentChanged;
            dgLevel3.CurrentCellChanged += OnDefectCurrentChanged;

            dgLevel1.LoadedCellPresenter += OnLoadedDefectCellPresenter;
            dgLevel2.LoadedCellPresenter += OnLoadedDefectCellPresenter;
            dgLevel3.LoadedCellPresenter += OnLoadedDefectCellPresenter;

            #region [Cancel Delete Lot]
            btnCancelDelete.Click += OnCancelDeleteLot;
            #endregion

            txtInputQty.KeyDown += OnKeyDownInputQty;
            txtInputQty.LostFocus += OnKeyLostInputQty;
            btnSaveWipHistory.Click += OnClickWIPHistory;
            btnSaveCarrier.Click += OnClickSaveCarrier;

            #region # 전수불량Lane등록
            btnSaveRegDefectLane.Click += OnClickRegDefectLane;
            #endregion

            dgLotInfo.LoadedCellPresenter += OnLoadedLotInfoCellPresenter;
            dgLotInfo.UnloadedCellPresenter += OnUnLoadedLotInfoCellPresenter;
            dgWipReason.CommittedEdit += OnDataCollectGridCommittedEdit;
            dgWipReason.BeginningEdit += OnDataCollectGridBeginningEdit;
            dgWipReason.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            dgWipReason.MouseDoubleClick += OnClickCellMouseDoubleClick;
            dgWipReason.BeganEdit += OnDataCollectGridBeganEdit;
            dgWipReason.LoadedCellPresenter += OnLoadedWipReasonCellPresenter;

            dgRemark.LoadedCellPresenter += OnLoadedRemarkCellPresenter;
            dgQuality.CommittedEdit += OnDataCollectGridQualityCommittedEdit;
            dgQuality.PreviewKeyDown += OnDataCollectGridPreviewKeyDown;
            dgQuality.LoadedCellPresenter += OnLoadedDgQualitynCellPresenter;
            dgQuality.UnloadedCellPresenter += OnUnLoadedDgQualitynCellPresenter;

            dgCotton.CommittedEdit += OnDataCollectGridCottonTagCommittedEdit;
            btnSaveWipReason.Click += OnClickSaveWipReason;
            btnProcResn.Click += OnClickProcReason;
            btnSaveQuality.Click += OnClickSaveQuality;

            btnSaveRemark.Click += OnClickSaveRemark;

            //전체저장 2018-01-09
            btnPublicWipSave.Click += OnClickPublicSave;

            //면상태일지 2018-04-06
            btnSaveCotton.Click += OnClickCottonSave;

            // MULTI LANE 소형/자동차를 나눠서 기능을 사용하기 위하여 추가 [2017-01-24] -CR-56-
            chkSum.Click += OnSumReasonGridChecked;

            cboLaneNum.SelectedItemChanged += OnLaneSelectionItemChanged;

            txtMesualQty.KeyDown += OnKeyDownMesualQty;
            txtMesualQty.LostFocus += OnKeyLostMesualQty;

            // SHIFT USER, WORKER 정보를 사용하기 위하여 BUTTON 이벤트 추가 [2017-02-24]
            btnShift.Click += OnClickShift;

            // 공정진척 창확장 버튼 EVENT추가 [2017-03-24]
            btnExpandFrame.Click += OnClickbtnExpandFrame;
            btnLeftExpandFrame.Click += OnClickbtnLeftExpandFrame;

            // 합권취 버튼 이벤트 추가 [2017-07-06]
            btnSearchMerge.Click += OnClickSearchMergeData;
            btnSaveMerge.Click += OnClickSaveMergeData;

            // CNA에서 불량 실적 반영안되는 괴현상 발생으로 불량값 입력 하다 TAB이동 후 다시 돌아오면 소수점 클리어되는 현상으로 하기 이벤트 추가 [2017-09-03]
            tcDataCollect.SelectionChanged += OnTabSelectionChange;

            //CWA 불량정보 필터링 이벤트 추가
            chkDefectFilter.Click += OnClickDefetectFilter;
        }

        private void OnLoadedProdLotCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                                return;

                            // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완
                            if (WIPCOLORLEGEND == null)
                                return;

                            // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                            SolidColorBrush scbZVersionBack = new SolidColorBrush();
                            SolidColorBrush scbZVersionFore = new SolidColorBrush();

                            foreach (DataRow dr in WIPCOLORLEGEND.Rows)
                            {
                                if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                                {
                                    continue;
                                }

                                if (dr["CODE"].ToString().Equals("Z_VER"))
                                {
                                    scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                                    scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                                }
                            }

                            if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                            {
                                e.Cell.Presenter.Background = scbZVersionBack;
                                e.Cell.Presenter.Foreground = scbZVersionFore;
                            }
                            else if (e.Cell.Column.Name.Equals("CUT_ID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                        ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                            {
                                e.Cell.Presenter.Background = scbZVersionBack;
                                e.Cell.Presenter.Foreground = scbZVersionFore;
                            }
                        }
                    }
                }));
            }
        }

        private void SetStatus()
        {
            var status = new List<string>();

            if (chkWait.IsChecked == true)
                status.Add(chkWait.Tag.ToString());

            if (chkRun.IsChecked == true)
                status.Add(chkRun.Tag.ToString());

            if (chkEqpEnd.IsChecked == true)
                status.Add(chkEqpEnd.Tag.ToString());

            if (chkConfirm.IsChecked == true)
                status.Add(chkConfirm.Tag.ToString());

            WIPSTATUS = string.Join(",", status);
        }

        private void SetVisible()
        {
            // 횟수를 COMMONCODE로 관리하도록 변경 (C20190416_75868) [2019-04-16]
            isResnCountUse = IsAreaCommonCodeUse("RESN_COUNT_USE_YN", _PROCID);            

            // MULTI LANE은 수동으로 ALL/SUM 구분하여 사용할 수 있게 변경 [2017-01-21] -CR-56-
            dgWipReason.Columns["RESNTOTQTY"].Visibility = Visibility.Visible;

            if (Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_SMALLTYPE]) == true)
                chkSum.Visibility = Visibility.Visible;

            cboLaneNum.Visibility = Visibility.Visible;

            lblMesualQty.Visibility = Visibility.Visible;
            txtMesualQty.Visibility = Visibility.Visible;

            DataTableConverter.SetValue(dgWipReason, "FrozenBottomRowsCount", 3);

            // SLITTING 공정만 생산 중 불량 기능 추가 [2017-07-18]
            //btnProcResn.Visibility = Visibility.Visible;


            btnStart.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            btnEqptEnd.Visibility = Visibility.Collapsed;
            btnEqptEndCancel.Visibility = Visibility.Collapsed;
            btnEndCancel.Visibility = Visibility.Collapsed;
        }

        private void WipReasonGridColumnAdd()
        {
            //// 동적으로 AREA_RESN_CLSS_NAME1,2,3 컬럼 생성
            //WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            //{
            //    Name = "AREA_RESN_CLSS_NAME1",
            //    Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME1"),
            //    Binding = new System.Windows.Data.Binding()
            //    {
            //        Path = new PropertyPath("AREA_RESN_CLSS_NAME1"),
            //        Mode = BindingMode.TwoWay
            //    },
            //    IsReadOnly = true,
            //    Visibility = Visibility.Collapsed
            //});

            //WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            //{
            //    Name = "AREA_RESN_CLSS_NAME2",
            //    Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME2"),
            //    Binding = new System.Windows.Data.Binding()
            //    {
            //        Path = new PropertyPath("AREA_RESN_CLSS_NAME2"),
            //        Mode = BindingMode.TwoWay
            //    },
            //    IsReadOnly = true,
            //    Visibility = Visibility.Collapsed
            //});

            //WIPREASON_GRID.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            //{
            //    Name = "AREA_RESN_CLSS_NAME3",
            //    Header = ObjectDic.Instance.GetObjectName("AREA_RESN_CLSS_NAME3"),
            //    Binding = new System.Windows.Data.Binding()
            //    {
            //        Path = new PropertyPath("AREA_RESN_CLSS_NAME3"),
            //        Mode = BindingMode.TwoWay
            //    },
            //    IsReadOnly = true,
            //    Visibility = Visibility.Collapsed
            //});

            //WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME1"].DisplayIndex = 7;
            //WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME2"].DisplayIndex = 8;
            //WIPREASON_GRID.Columns["AREA_RESN_CLSS_NAME3"].DisplayIndex = 9;

        }

        private bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailControls();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private void ClearDetailControls()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgQuality);
            Util.gridClear(dgQuality2);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgSearchSlurry);
            Util.gridClear(dgSlurry);
            Util.gridClear(dgColor);
            Util.gridClear(dgDefectTag);
            Util.gridClear(dgMaterialList);
            Util.gridClear(dgRemark);
            Util.gridClear(dgRemarkHistory);
            Util.gridClear(dgWipMerge);
            Util.gridClear(dgWipMerge2);
            Util.gridClear(dgCotton);
            dgLotInfo.BottomRows.Clear();
            
            if (procResnDt != null)
                procResnDt.Clear();

            InitClear();
        }

        private void GetWorkOrder()
        {
            UC_WORKORDER_CWA wo1;

            ClearControls();

            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                wo1 = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                wo1.FrameOperation = FrameOperation;
                //wo1._UCElec_CWA = this;
                wo1.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
                wo1.EQPTID = cboEquipment.SelectedValue.ToString();
                wo1.PROCID = _PROCID;

                wo1.GetWorkOrder();
            }

            WORKORDER_GRID = (grdWorkOrder.Children[0] as UC_WORKORDER_CWA).dgWorkOrder;
        }

        private void GetSumDefectQty()
        {
            if (!string.Equals(_WIPSTAT, Wip_State.END))
                return;

            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            if (_DT_OUT_PRODUCT.Rows.Count == 1)
            {
                defectQty = SumDefectQty("DEFECT_LOT");
                LossQty = SumDefectQty("LOSS_LOT");
                chargeQty = SumDefectQty("CHARGE_PROD_LOT");

                totalSum = defectQty + LossQty + chargeQty;

                //DataTable dt = (LOTINFO_GRID.ItemsSource as DataView).Table;

                for (int i = dgLotInfo.TopRows.Count; i < (dgLotInfo.Rows.Count - dgLotInfo.BottomRows.Count); i++)
                {
                    if (i != dgLotInfo.TopRows.Count + 1) continue;

                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_LOSS", LossQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_DEFECT_SUM", totalSum);

                    ////이병렬책임 요청 [2019-07-09]
                    //if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count) - totalSum);
                    //else if (Util.NVC(txtInputQty.Tag).Equals("C"))
                    //    //DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY")) + totalSum);
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count) - totalSum);
                    //else
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY")) * (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count) + totalSum);

                    //if (txtLaneQty != null && !string.Equals(_PROCID, Process.HALF_SLITTING))
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY2", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")) - totalSum);
                    //else
                        DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY2", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")) - totalSum);
                }

                SetParentRemainQty();
            }
        }

        private double SumDefectQty(string actId)
        {
            double sum = 0;

            if (dgWipReason.Rows.Count > 0)
                for (int i = 0; i < dgWipReason.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNQTY")) * 1);

            return sum;
        }

        private double SumDefectQty(C1DataGrid dg, string lotId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)) * 1);

            return sum;
        }

        private double SumDefectQty(C1DataGrid dg, int index, string lotId, string actId)
        {
            double sum = 0;

            if (dg.Rows.Count > 0)
                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID").ToString().Equals(actId))
                                sum += (Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[i].DataItem, lotId)) * 1);

            return sum;
        }

        private void SetParentRemainQty()
        {
            decimal parentQty = 0;
            decimal inputQty = 0;

            parentQty = Util.NVC_Decimal(string.IsNullOrEmpty(txtParentQty.Value.ToString()) ? "0" : txtParentQty.Value.ToString());
            inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);

            txtRemainQty.Value = Convert.ToDouble(parentQty - (inputQty - exceedLengthQty));

            txtInputQty.Value = 0;
        }

        private DataTable dtDataCollectOfChildrenDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            if (isResnCountUse == true)
                IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            //int iCount = isResnCountUse == true ? 1 : 0;

            //DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;
            int iLotCount = 0;

            for (int iCol = dg.Columns["ALL"].Index; iCol < dg.Columns["COSTCENTERID"].Index; iCol++)
            {
                if (Util.NVC(dg.Columns[iCol].Name).Equals("ALL") ||
                    Util.NVC(dg.Columns[iCol].Name).EndsWith("NUM") ||
                    Util.NVC(dg.Columns[iCol].Name).EndsWith("CNT") ||
                    Util.NVC(dg.Columns[iCol].Name).EndsWith("RESN_TOT_CHK") ||
                    Util.NVC(dg.Columns[iCol].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY")) continue;

                string sublotid = dg.Columns[iCol].Name;

                foreach (DataRow _iRow in ((dg.ItemsSource as DataView).Table).Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["LOTID"] = sublotid;
                    inData["WIPSEQ"] = _DT_OUT_PRODUCT.Rows[0]["WIPSEQ"];
                    inData["ACTID"] = _iRow["ACTID"];
                    inData["RESNCODE"] = _iRow["RESNCODE"];
                    inData["RESNQTY"] = _iRow[sublotid].ToString().Equals("") ? 0 : _iRow[sublotid];
                    inData["DFCT_TAG_QTY"] = _iRow[sublotid + "CNT"].ToString().Equals("") ? 0 : _iRow[sublotid + "CNT"];

                    // SLITTING 공정에서 Deffec 저장 시 LANE수량 1로 변경 요청 [2017-01-17]
                    if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING))
                    {
                        inData["LANE_QTY"] = 1; //txtLaneQty.Value;
                        inData["LANE_PTN_QTY"] = 1; //txtLanePatternQty.Value;
                    }
                    else if (string.Equals(_PROCID, Process.HALF_SLITTING))
                    {
                        inData["LANE_QTY"] = Util.NVC(_DT_OUT_PRODUCT.Rows[iLotCount]["LANE_QTY"]);
                        inData["LANE_PTN_QTY"] = Util.NVC(_DT_OUT_PRODUCT.Rows[iLotCount]["LANE_PTN_QTY"]);
                    }
                    else
                    {
                        inData["LANE_QTY"] = txtLaneQty.Value;
                        inData["LANE_PTN_QTY"] = txtLanePatternQty.Value;
                    }
                    inData["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

                    //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                    //    inData["WRK_COUNT"] = _iRow["COUNTQTY"].ToString() == "" ? DBNull.Value : _iRow["COUNTQTY"];
                    if (isResnCountUse == true)
                        inData["WRK_COUNT"] = string.IsNullOrEmpty(_iRow[sublotid + "NUM"].ToString()) ? 0 : _iRow[sublotid + "NUM"];

                    IndataTable.Rows.Add(inData);
                }
                iLotCount++;
            }
            return IndataTable;
        }

        private DataTable dtDataCollectOfChildDefect(C1DataGrid dg)
        {
            DataTable IndataTable = inDataSet.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            // if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
            // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
            if (isResnCountUse == true && (!string.Equals(_PROCID, Process.SLITTING) && !string.Equals(_PROCID, Process.SRS_SLITTING) && !string.Equals(_PROCID, Process.HALF_SLITTING)))
                IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            DataRow inDataRow = null;
            DataTable dtTop = (dg.ItemsSource as DataView).Table;

            foreach (DataRow _iRow in dtTop.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = _LOTID;
                inDataRow["WIPSEQ"] = _DT_OUT_PRODUCT.Rows[0]["WIPSEQ"];
                inDataRow["ACTID"] = _iRow["ACTID"];
                inDataRow["RESNCODE"] = _iRow["RESNCODE"];
                inDataRow["RESNQTY"] = _iRow["RESNQTY"].ToString().Equals("") ? 0 : _iRow["RESNQTY"];
                inDataRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(_iRow["DFCT_TAG_QTY"])) ? 0 : _iRow["DFCT_TAG_QTY"];

                // SLITTING 공정에서 Deffec 저장 시 LANE수량 1로 변경 요청 [2017-01-17]
                if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING))
                {
                    inDataRow["LANE_QTY"] = 1; //txtLaneQty.Value;
                    inDataRow["LANE_PTN_QTY"] = 1; //txtLanePatternQty.Value;
                }
                else
                {
                    inDataRow["LANE_QTY"] = txtLaneQty == null ? 0 : txtLaneQty.Value;
                    inDataRow["LANE_PTN_QTY"] = txtLanePatternQty == null ? 0 : txtLanePatternQty.Value;
                }
                inDataRow["COST_CNTR_ID"] = _iRow["COSTCENTERID"];

                //if (string.Equals(procId, Process.COATING) || string.Equals(procId, Process.BACK_COATING))
                // 횟수 전 공정 추가로 수정 (C20190416_75868) [2019-04-16]
                if (isResnCountUse == true && (!string.Equals(_PROCID, Process.SLITTING) && !string.Equals(_PROCID, Process.SRS_SLITTING) && !string.Equals(_PROCID, Process.HALF_SLITTING)))
                    inDataRow["WRK_COUNT"] = _iRow["COUNTQTY"].ToString() == "" ? DBNull.Value : _iRow["COUNTQTY"];

                IndataTable.Rows.Add(inDataRow);
            }
            return IndataTable;
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

            if (dg.Name.Equals("dgColorTag"))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    GetWipSeq(_LOTID, dg.Columns[i].Name);
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = _LOTID;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = dg.Columns[i].Name;
                    inData["CLCTVAL01"] = string.IsNullOrEmpty(dt.Rows[0][dg.Columns[i].Name].ToString()) ? 0 : Convert.ToDouble(dt.Rows[0][dg.Columns[i].Name].ToString());
                    inData["WIPSEQ"] = string.IsNullOrEmpty(_WIPSEQ) ? null : _WIPSEQ;
                    inData["CLCTSEQ"] = string.IsNullOrEmpty(_CLCTSEQ) ? null : _CLCTSEQ;
                    IndataTable.Rows.Add(inData);
                }
            }
            else
            {
                GetWipSeq(_LOTID, string.Empty);

                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inData["LOTID"] = _LOTID;
                    inData["EQPTID"] = _EQPTID;
                    inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];
                    decimal tmp;
                    if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                        inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    else
                        inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(_WIPSEQ) ? null : _WIPSEQ;
                    inData["CLCTSEQ"] = 1;
                    IndataTable.Rows.Add(inData);
                }
            }
            return IndataTable;
        }

        private void SetLotAutoSelected()
        {
            // 롤플레스 대기, 설비완공 자동으로 선택해주게 변경 2018-01-06
            if (dgProductLot.Visibility == Visibility.Visible)
            {
                if (dgProductLot != null && dgProductLot.Rows.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridCell currCell = dgProductLot.GetCell(0, dgProductLot.Columns["CHK"].Index);

                    if (currCell != null && currCell.Presenter != null && currCell.Presenter.Content != null)
                    {
                        dgProductLot.SelectedIndex = currCell.Row.Index;
                        dgProductLot.CurrentCell = currCell;

                    }
                }
            }

            if (dgProductLot != null && dgProductLot.Rows.Count > 0)
            {
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    if (string.Equals(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT"), Wip_State.PROC))
                    {
                        C1.WPF.DataGrid.DataGridCell currCell = dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index);

                        if (currCell != null && currCell.Presenter != null && currCell.Presenter.Content != null)
                        {
                            dgProductLot.SelectedIndex = currCell.Row.Index;
                            dgProductLot.CurrentCell = currCell;
                        }
                        break;
                    }
                }
            }
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택 하세요.
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택 하세요.
                return bRet;
            }
            bRet = true;
            return bRet;
        }

        private void ClearDefectLV()
        {
            if (chkDefectFilter.IsChecked == true)
            {
                isDefectLevel = true;
                OnClickDefetectFilter(chkDefectFilter, null);
                isDefectLevel = false;
            }
        }

        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            DataRowView drv = dataitem.DataItem as DataRowView;
            string sInputLot;
            string sChildSeq;
            string sLot;

            try
            {
                sInputLot = drv["LOTID_PR"].ToString().Equals(string.Empty) ? drv["LOTID"].ToString() : drv["LOTID_PR"].ToString();
            }
            catch
            {
                sInputLot = string.Empty;
            }

            try
            {
                sChildSeq = string.IsNullOrEmpty(drv["CUT_ID"].ToString()) ? "1" : drv["CUT_ID"].ToString();
            }
            catch
            {
                sChildSeq = "1";
            }

            try
            {
                sLot = drv["LOTID"].ToString();
            }
            catch
            {
                sLot = string.Empty;
            }

            if (!string.IsNullOrEmpty(sInputLot) && !string.IsNullOrEmpty(sChildSeq))
            {
                // 모두 Uncheck 처리 및 동일 자LOT의 경우는 Check 처리.
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    if (dataitem.Index != i)
                    {
                        if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID_PR")) &&
                            sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CUT_ID")))
                        {
                            if (sInputLot.Equals(""))
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                    dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (bUncheckAll)
                                {
                                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                    dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                }
                                else
                                {
                                    // CNA에 같은 대LOT에 같은 GR_SEQ로 COATER LOT이 2개씩 생성되어 SLITTER계열만 동일 선택 처리 [2017-07-05]
                                    if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                                    {
                                        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                            dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;

                                        DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", true);
                                    }
                                    else if (!string.Equals(sLot, Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"))))
                                    {
                                        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                            dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                        DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            return true;
        }

        private void DefectVisibleLVAll()
        {
            DataTable dt = (dgWipReason.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgWipReason.Rows[i].Visibility = Visibility.Visible;
            }
        }

        private void SetUnitFormatted()
        {
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                string sFormatted = string.Empty;
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

                txtInputQty.Format = sFormatted;

                txtParentQty.Format = sFormatted;
                txtRemainQty.Format = sFormatted;

                if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                {
                    if (string.Equals(txtUnit.Text, "EA"))
                        txtMesualQty.Format = "F1";
                    else
                        txtMesualQty.Format = sFormatted;
                }

                if (dgLotInfo.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgLotInfo.Columns.Count; i++)
                        if (dgLotInfo.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgLotInfo.Columns[i].Tag, "N"))
                            // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                            if (string.Equals(txtUnit.Text, "EA") && string.Equals(_PROCID, Process.COATING))
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = sFormatted;

                if (dgWipReason.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgWipReason.Columns.Count; i++)
                        if (dgWipReason.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReason.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)dgWipReason.Columns[i]).Format = sFormatted;

                if (dgQuality.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgQuality.Columns.Count; i++)
                        if (dgQuality.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQuality.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgQuality.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgQuality.Columns[i]).Format = sFormatted;

                if (dgQuality2.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgQuality2.Columns.Count; i++)
                        if (dgQuality2.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQuality2.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgQuality2.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgQuality2.Columns[i]).Format = sFormatted;

                if (dgMaterial.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgMaterial.Columns.Count; i++)
                        if (dgMaterial.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgMaterial.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = sFormatted;

                if (dgMaterialList.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgMaterialList.Columns.Count; i++)
                        if (dgMaterialList.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgMaterialList.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgMaterialList.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgMaterialList.Columns[i]).Format = sFormatted;

                // NISSAN용 입력용도로 추가
                if (dgDefectTag.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgDefectTag.Columns.Count; i++)
                        if (dgDefectTag.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgDefectTag.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)dgDefectTag.Columns[i]).Format = sFormatted;
            }
        }

        private void BindingWipNote()
        {
            if (dgRemark.GetRowCount() > 0)
                return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

            if (_DT_OUT_PRODUCT.Rows.Count > 0)
            {
                string[] sWipNote = GetWIPNOTE(Util.NVC(_DT_OUT_PRODUCT.Rows[0]["LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    inDataRow["REMARK"] = sWipNote[1];
            }
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in _DT_OUT_PRODUCT.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                inDataRow["REMARK"] = GetWIPNOTE(Util.NVC(_row["LOTID"])).Split('|')[0];
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(dgRemark, dtRemark, FrameOperation);            
        }

        private void SetInputQty()
        {
            if (dgLotInfo.GetRowCount() < 1)
                return;

            decimal inputQty = Util.NVC_Decimal(txtInputQty.Value);
            decimal lossQty = 0;
            int laneqty = 0;

            lossQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT_SUM"));
            laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LANE_QTY"));

            //DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY", inputQty);
            //DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (inputQty * dgLotInfo.GetRowCount()) - lossQty);

            SetParentRemainQty();

        }

        private void SetCauseTitle()
        {
            int causeqty = 0;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                for (int i = dgWipReason.TopRows.Count; i < dt.Rows.Count + dgWipReason.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }                
            }
        }

        private void SetExceedLength()
        {
            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            int iCount = isResnCountUse == true ? 1 : 0;

            //for (int i = 0; i < dgWipReason.Rows.Count; i++)
            //{
            //    if (string.Equals(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
            //    {
            //        if (_DT_OUT_PRODUCT.Rows.Count == 1)
            //            exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNQTY"));
            //        else
            //            exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, dgWipReason.Columns[dgWipReason.Columns["ALL"].Index + (2 + iCount)].Name));
            //        break;
            //    }
            //}

            int iLenLackRow = Util.gridFindDataRow(ref dgWipReason, "PRCS_ITEM_CODE", ITEM_CODE_LEN_EXCEED, false);

            if (iLenLackRow >= 0)
            {
                if (_DT_OUT_PRODUCT.Rows.Count == 1)
                {
                    exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iLenLackRow].DataItem, "RESNQTY"));
                }
                else
                {
                    //for (int iCol = dgWipReason.Columns["ALL"].Index; iCol < dgWipReason.Columns["COSTCENTERID"].Index; iCol++)
                    //{
                    //    if (!Util.NVC(dgWipReason.Columns[iCol].Name).Equals("ALL") &&
                    //        !Util.NVC(dgWipReason.Columns[iCol].Name).EndsWith("NUM") &&
                    //        !Util.NVC(dgWipReason.Columns[iCol].Name).EndsWith("CNT") &&
                    //        !Util.NVC(dgWipReason.Columns[iCol].Name).EndsWith("RESN_TOT_CHK") &&
                    //        !Util.NVC(dgWipReason.Columns[iCol].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                    //    {
                    //        exceedLengthQty += Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iLenLackRow].DataItem, Util.NVC(dgWipReason.Columns[iCol].Name)));
                    //    }
                    //}
                    exceedLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iLenLackRow].DataItem, dgWipReason.Columns[dgWipReason.Columns["ALL"].Index + (2 + iCount)].Name));
                }
            }
            
            if (exceedLengthQty >= 0)
            {
                //decimal inputQty = Util.NVC_Decimal(txtParentQty.Value);
                //decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);

                //if (prodQty > 0)
                //    txtRemainQty.Value = Convert.ToDouble(inputQty - (prodQty - Util.NVC_Decimal(exceedLengthQty)));
            }
            
            txtRemainQty.Value = Convert.ToDouble(0);
        }

        private void SetVisibilityWipReasonGrid(string sLotID, bool? isVisibility)
        {
            for (int i = dgWipReason.Columns["TAG_CONV_RATE"].Index; i < dgWipReason.Columns.Count; i++)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                if (string.Equals(sLotID, dgWipReason.Columns[i].Name) ||
                    (chkSum.IsChecked == false &&
                        (string.Equals(sLotID + "NUM", dgWipReason.Columns[i].Name) || 
                         string.Equals(sLotID + "CNT", dgWipReason.Columns[i].Name) ||
                         string.Equals(sLotID + "RESN_TOT_CHK", dgWipReason.Columns[i].Name))))
                    dgWipReason.Columns[i].Visibility = isVisibility == true ? Visibility.Visible : Visibility.Collapsed;
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

        private string GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (txtUnit.Text)
            {
                case "KG":
                    sFormatted = "{0:#,##0.000}";
                    break;

                case "M":
                    sFormatted = "{0:#,##0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:#,##0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
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

        private string GetIntFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = "{0:#,##0}";
            double dFormat = 0;

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private void BindingDataGrid(C1DataGrid dg, int iRow, int iCol, object sValue)
        {
            try
            {
                if (dg.ItemsSource == null)
                {
                    return;
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                    if (dt.Columns.Count < dg.Columns.Count)
                        for (int i = dt.Columns.Count; i < dg.Columns.Count; i++)
                            dt.Columns.Add(dg.Columns[i].Name);

                    if (sValue.Equals("") || sValue.Equals(null))
                        sValue = 0;

                    dt.Rows[iRow][iCol - 1] = sValue;

                    dg.BeginEdit();
                    Util.GridSetData(dg, dt, FrameOperation, false);
                    dg.EndEdit();
                }
            }
            catch { }
        }

        private void SetDisableWipReasonGrid(DataRow[] laneRow)
        {
            for (int j = 0; j < laneRow.Length; j++)
            {
                for (int i = dgWipReason.Columns["TAG_CONV_RATE"].Index; i < dgWipReason.Columns.Count; i++)
                {
                    if (string.Equals(laneRow[j]["LOTID"], dgWipReason.Columns[i].Name) || 
                        string.Equals(laneRow[j]["LOTID"] + "NUM", dgWipReason.Columns[i].Name) || 
                        string.Equals(laneRow[j]["LOTID"] + "CNT", dgWipReason.Columns[i].Name) ||
                        string.Equals(laneRow[j]["LOTID"] + "RESN_TOT_CHK", dgWipReason.Columns[i].Name) ||
                        string.Equals(laneRow[j]["LOTID"] + "FRST_AUTO_RSLT_RESNQTY", dgWipReason.Columns[i].Name))
                    {
                        dgWipReason.Columns[i].IsReadOnly = true;

                        //// 전수불량인 경우 불량 모두 0으로 처리.
                        //if (string.Equals(laneRow[j]["LOTID"], dgWipReason.Columns[i].Name))
                        //{
                        //    for (int iRow = dgWipReason.TopRows.Count; iRow < dgWipReason.GetRowCount(); iRow++)
                        //    {
                        //        DataTableConverter.SetValue(dgWipReason.Rows[iRow].DataItem, dgWipReason.Columns[i].Name, 0);
                        //    }
                        //}
                    }
                }
            }
            // 전체불량의 경우 ALL Column ReadOnly
            if (_dtDEFECTLANENOT.Rows.Count == laneRow.Length)
                dgWipReason.Columns[dgWipReason.Columns["ALL"].Index].IsReadOnly = true;

            dgWipReason.LoadedCellPresenter -= OnLoadedDefectLaneCellPresenter;
            dgWipReason.LoadedCellPresenter += OnLoadedDefectLaneCellPresenter;
        }
        
        private Dictionary<string, string> GetRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (dgRemark.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < dgRemark.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[0].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 3. 조정횟수
                    if (dgWipReason.Visibility == Visibility.Visible && dgWipReason.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < dgWipReason.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) + ",");
                                        
                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 4. 압연횟수
                    if (string.Equals(_PROCID, Process.ROLL_PRESSING))
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "PRESSCOUNT")));
                    sRemark.Append("|");

                    // 5.색지정보
                    if (string.Equals(_PROCID, Process.ROLL_PRESSING))
                        for (int j = 0; j < dgColor.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01"))) &&
                                Util.NVC_Decimal(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(dgColor.Rows[j].DataItem, "CLCTVAL01")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID")), _PROCID);
                    if (mergeInfo.Rows.Count > 0)
                        foreach (DataRow row in mergeInfo.Rows)
                            sRemark.Append(Util.NVC(row["LOTID"]) + " : " + GetUnitFormatted(row["LOT_QTY"]) + txtUnit.Text + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        private bool SetLossLot(C1DataGrid dg, string sItemCode, decimal iLossQty)
        {
            bool isLossValid = false;
            DataTable dt = (dg.ItemsSource as DataView).Table;
            if (_DT_OUT_PRODUCT.Rows.Count > 1)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                //int iCount = isResnCountUse == true ? 1 : 0;

                for (int iCol = dg.Columns["ALL"].Index; iCol < dg.Columns["COSTCENTERID"].Index; iCol++)
                {
                    if (Util.NVC(dg.Columns[iCol].Name).Equals("ALL") ||
                        Util.NVC(dg.Columns[iCol].Name).EndsWith("NUM") ||
                        Util.NVC(dg.Columns[iCol].Name).EndsWith("CNT") ||
                        Util.NVC(dg.Columns[iCol].Name).EndsWith("RESN_TOT_CHK") ||
                        Util.NVC(dg.Columns[iCol].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY")) continue;

                    string sublotid = dg.Columns[iCol + 1].Name;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                        {
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, sublotid, iLossQty);
                            DefectChange(dg, i, iCol + 1);
                            isLossValid = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (string.Equals(dt.Rows[i]["ACTID"], "LOSS_LOT") && string.Equals(dt.Rows[i]["PRCS_ITEM_CODE"], sItemCode))
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, "RESNQTY", iLossQty);
                        GetSumDefectQty(dg);
                        isLossValid = true;
                        break;
                    }
                }
            }

            if (isLossValid == false)
                Util.MessageValidation("SFU3196", new object[] { string.Equals(sItemCode, ITEM_CODE_LEN_LACK) ?
                    ObjectDic.Instance.GetObjectName("길이부족") : ObjectDic.Instance.GetObjectName("길이초과") }); //해당 MMD에 {%1}에 관련된 속성이 지정되지 않아 자동Loss를 등록할 수 없습니다.

            return isLossValid;
        }

        private void GetSumDefectQty(C1DataGrid dg)
        {
            double defectQty = 0;
            double LossQty = 0;
            double chargeQty = 0;
            double totalSum = 0;
            double laneqty = 0;

            if (_DT_OUT_PRODUCT.Rows.Count == 1)
            {
                defectQty = SumDefectQty("DEFECT_LOT");
                LossQty = SumDefectQty("LOSS_LOT");
                chargeQty = SumDefectQty("CHARGE_PROD_LOT");

                totalSum = defectQty + LossQty + chargeQty;

                //DataTable dt = (LOTINFO_GRID.ItemsSource as DataView).Table;

                for (int i = dgLotInfo.TopRows.Count; i < (dgLotInfo.Rows.Count - dgLotInfo.BottomRows.Count); i++)
                {
                    if (i != dgLotInfo.TopRows.Count + 1) continue;

                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_DEFECT", defectQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_LOSS", LossQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_CHARGEPRD", chargeQty);
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "DTL_DEFECT_SUM", totalSum);

                    //if (string.IsNullOrEmpty(Util.NVC(txtInputQty.Tag)))
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")) - totalSum);
                    //else
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY")) + totalSum);

                    //laneqty = Util.NVC_Int(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LANE_QTY"));

                    //if (txtLaneQty != null && !string.Equals(_PROCID, Process.HALF_SLITTING))
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY2", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY")) * txtLaneQty.Value);
                    //else
                    //    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY2", Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY")) * laneqty);
                }
            }
        }

        private void DefectChange(C1DataGrid dg, int iRow, int iCol)
        {
            if (iCol == dg.Columns["ALL"].Index)
            {
                // 소형 전극은 SUM으로 분배 [2017-01-24]
                int iLaneQty = 0;

                for (int i = 1; i < cboLaneNum.Items.Count; i++)
                    if (((CheckBox)cboLaneNum.Items[i]).IsChecked == true)
                        iLaneQty++;

                if (iLaneQty == 0)
                    return;

                decimal iTarget = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                decimal iLastCol = iTarget % iLaneQty;

                for (int i = dg.Columns["ALL"].Index; i < dg.Columns["COSTCENTERID"].Index; i++)
                {
                    // Lane 값 1/N 처리.
                    if (!Util.NVC(dg.Columns[i].Name).Equals("ALL") &&
                        !Util.NVC(dg.Columns[i].Name).EndsWith("NUM") && 
                        !Util.NVC(dg.Columns[i].Name).EndsWith("CNT") && 
                        !Util.NVC(dg.Columns[i].Name).EndsWith("RESN_TOT_CHK") && 
                        !Util.NVC(dg.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                    {
                        if (dg.Columns[i].Visibility == Visibility.Collapsed)
                            continue;

                        string _ValueToFind = string.Empty;

                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR))
                        {
                            SetCalcLackQty(iRow, Util.NVC(dg.Columns[i].Name), true);
                        }
                        else
                        {
                            // 길이초과, 길이부족의 경우는 SUM도 분배가 아닌 일괄 등록
                            if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_LACK) ||
                                string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
                            {
                                _ValueToFind = Util.NVC(iTarget);
                            }
                            else
                            {
                                if (string.Equals(dg.Columns["ALL"].GetColumnText(), ObjectDic.Instance.GetObjectName("SUM")))
                                    _ValueToFind = Util.NVC(Math.Truncate(iTarget / iLaneQty) + (iLastCol > 1 ? 1 : iLastCol));
                                else
                                    _ValueToFind = Util.NVC(iTarget);
                            }

                            // 공정 진행 중 불량은 ALL/SUM SETUP시 포함하여 저장 [2017-07-21]
                            if (procResnDt != null && procResnDt.Rows.Count > 0)
                            {
                                DataRow[] rows = procResnDt.Select("LOTID = '" + dg.Columns[i].Name + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "RESNCODE")) + "'");
                                if (rows != null && rows.Length > 0)
                                    _ValueToFind = Util.NVC(Util.NVC_Decimal(_ValueToFind) + Util.NVC_Decimal(rows[0]["RESNQTY"]));
                            }
                            #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                            if (!dg.Columns[i].IsReadOnly)
                                DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name, _ValueToFind);
                            #endregion
                            bool isDefectValid = true;
                            isDefectValid = GetSumCutDefectQty(dg, iRow, i);

                            if (isDefectValid == false)
                                continue;

                            iLastCol = iLastCol > 1 ? iLastCol - 1 : 0;
                        }
                    }
                }
            }
            // Lane Column
            else if (iCol >= dg.Columns["ALL"].Index + 1 &&
                     !Util.NVC(dg.Columns[iCol].Name).EndsWith("NUM") &&
                     !Util.NVC(dg.Columns[iCol].Name).EndsWith("CNT") &&
                     !Util.NVC(dg.Columns[iCol].Name).EndsWith("RESN_TOT_CHK") &&
                     !Util.NVC(dg.Columns[iCol].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
            {
                SetCalcLackQty(iRow, Util.NVC(dg.Columns[iCol].Name));
                GetSumCutDefectQty(dg, iRow, iCol);
            }
            // 태그수 Column
            else if (iCol >= dg.Columns["ALL"].Index + 1 &&
                     Util.NVC(dg.Columns[iCol].Name).EndsWith("CNT"))
            {
                // 태그 수 불량/LOSS 자동 반영 로직 적용(C20190404_67447) [2019-04-11]
                if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR))
                {
                    decimal dConvertValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_CONV_RATE"));
                    if (dConvertValue > 0)
                    {
                        decimal dInputTagValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol].Name));
                        decimal dInputDefectValue = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol + 1].Name));
                        decimal dPreInputDefectValue = Util.NVC_Decimal(dtWipReasonBak.Rows[iRow][dg.Columns[iCol].Name]) * dConvertValue;
                        //bool isSumValue = dInputDefectValue == 0 ? true : false;
                        bool isSumValue = true;

                        if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_ALL_APPLY_FLAG"), "Y"))
                        {
                            for (int i = dg.Columns["ALL"].Index; i < dg.Columns["COSTCENTERID"].Index; i++)
                            {
                                if (!Util.NVC(dg.Columns[i].Name).Equals("ALL") &&
                                    !Util.NVC(dg.Columns[i].Name).EndsWith("NUM") && 
                                    !Util.NVC(dg.Columns[i].Name).EndsWith("CNT") && 
                                    !Util.NVC(dg.Columns[i].Name).EndsWith("RESN_TOT_CHK") &&
                                    !Util.NVC(dg.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                {
                                    if (Util.NVC_Decimal(Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name))) != 0 &&
                                        Util.NVC_Decimal(Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name))) != dPreInputDefectValue)
                                    {
                                        isSumValue = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dInputDefectValue != 0 && dInputDefectValue != dPreInputDefectValue)
                                isSumValue = false;
                        }

                        if (isSumValue == true)
                        {
                            if (string.Equals(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, "TAG_ALL_APPLY_FLAG"), "Y"))
                            {
                                for (int i = dg.Columns["ALL"].Index; i < dg.Columns["COSTCENTERID"].Index; i++)
                                {
                                    if (!Util.NVC(dg.Columns[i].Name).Equals("ALL") &&
                                        !Util.NVC(dg.Columns[i].Name).EndsWith("NUM") && 
                                        !Util.NVC(dg.Columns[i].Name).EndsWith("CNT") && 
                                        !Util.NVC(dg.Columns[i].Name).EndsWith("RESN_TOT_CHK") &&
                                        !Util.NVC(dg.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                    {
                                        #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                                        if (!dg.Columns[i].IsReadOnly)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i].Name, (dInputTagValue * dConvertValue));
                                            DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[i - 1].Name, (dInputTagValue));

                                        }
                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                #region #전수불량 Lane 등록; 전수불량 등록Lane 불량수량 미적용
                                if (!dg.Columns[iCol + 1].IsReadOnly)
                                    DataTableConverter.SetValue(dg.Rows[iRow].DataItem, dg.Columns[iCol + 1].Name, (dInputTagValue * dConvertValue));
                                #endregion
                            }
                            GetSumCutDefectQty(dg, iRow, iCol + 1);
                        }
                    }
                }
            }            
            //SetParentRemainQty();

            // 값 변환때마다 갱신(C20190404_67447) [2019-04-11]
            if (string.Equals(_PROCID, Process.SLITTING))
                dtWipReasonBak = DataTableConverter.Convert(dg.ItemsSource);
        }

        private void SetDfctChkAll(C1DataGrid dg, string sColName, int iRow)
        {
            if (dg == null) return;

            if (iRow < 0 || iRow > dg.GetRowCount()) return;

            string sLaneName = sColName.Replace("RESN_TOT_CHK", "");

            if (!dg.Columns.Contains(sLaneName)) return;

            for (int i = 0; i < dg.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dg.Rows[i].DataItem, sLaneName, 0);
                DataTableConverter.SetValue(dg.Rows[i].DataItem, sColName, 0);
            }

            DataTableConverter.SetValue(dg.Rows[iRow].DataItem, sLaneName, 0);
        }

        private bool GetSumCutDefectQty(C1DataGrid dg, int rowIdx, int colIdx)
        {
            if (!string.Equals(_WIPSTAT, Wip_State.END))
                return false;

            string actId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ACTID"));

            if (actId.Equals("")) return false;

            if (dgLotInfo.Rows.Count <= dgLotInfo.TopRows.Count) return false;

            double inputQty = 0;
            double actSum = 0;
            double totalSum = 0;
            double rowSum = 0;
            double laneQty = 0;

            // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
            //int iCount = isResnCountUse == true ? 1 : 0;

            laneQty = _DT_OUT_PRODUCT.Rows.Count;
            inputQty = Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);

            for (int i = dg.Columns["ALL"].Index; i < dg.Columns["COSTCENTERID"].Index; i++)
            {
                if (Util.NVC(dg.Columns[i].Name).Equals("ALL") ||
                    Util.NVC(dg.Columns[i].Name).EndsWith("NUM") || 
                    Util.NVC(dg.Columns[i].Name).EndsWith("CNT") || 
                    Util.NVC(dg.Columns[i].Name).EndsWith("RESN_TOT_CHK") ||
                    Util.NVC(dg.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY")) continue;
                /*
                if (inputQty < SumDefectQty(dg, dg.Columns[i].Name))
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1608"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //생산량 보다 불량이 크게 입력될 수 없습니다.
                    DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, dg.Columns[colIdx].Name, null);
                    return false;
                }
                */

                rowSum += Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, dg.Columns[i].Name));
                actSum += SumDefectQty(dg, i, dg.Columns[i].Name, actId);
            }

            totalSum = actSum;
            if (!string.Equals(actId, "DEFECT_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_DEFECT"));
            if (!string.Equals(actId, "LOSS_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_LOSS"));
            if (!string.Equals(actId, "CHARGE_PROD_LOT"))
                totalSum += Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_CHARGEPRD"));

            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, string.Equals(actId, "DEFECT_LOT") ? "DTL_DEFECT" : string.Equals(actId, "LOSS_LOT") ? "DTL_LOSS" : "DTL_CHARGEPRD", actSum);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "DTL_DEFECT_SUM", totalSum);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count + 1].DataItem, "GOODQTY2", (inputQty * laneQty) - totalSum);

            DataTableConverter.SetValue(dg.Rows[rowIdx].DataItem, "RESNTOTQTY", rowSum);

            return true;
        }
        
        private void DefectVisibleLV(DataTable dt, int LV, bool chk)
        {
            if (LV == 1)
            {
                DefectVisibleLV1(dt, chk);
            }
            else if (LV == 2)
            {
                DefectVisibleLV2(dt, chk);
            }
            else if (LV == 3)
            {
                DefectVisibleLV3(dt, chk);
            }
        }

        private void DefectVisibleLV1(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        private void DefectVisibleLV2(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV3(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }

        }

        private void SetResultInputQTY()
        {
            isChangeInputFocus = true;
            if (IsFinalProcess())
            {
                if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                {
                    decimal diffQty = Math.Abs(Util.NVC_Decimal(txtParentQty.Value) - Util.NVC_Decimal(txtInputQty.Value));

                    // 투입량의 제한률 이상 초과하면 입력 금지, 단 INPUT_OVER_RATE가 등록되어있지 않으면 SKIP [2017-03-02]
                    decimal inputRateQty = Util.NVC_Decimal(Util.NVC_Decimal(txtParentQty.Value) * inputOverrate);

                    if (inputRateQty > 0 && diffQty > inputRateQty)
                    {
                        Util.MessageValidation("SFU3195", new object[] { Util.NVC(inputOverrate * 100) + "%" });    //투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
                        return;
                    }

                    //  차이수량(생산량-투입량) %1 만큼 길이초과로 등록 하시겠습니까?
                    Util.MessageConfirm("SFU1921", (vResult) =>
                    {
                        if (vResult == MessageBoxResult.OK)
                        {
                            if (SetLossLot(dgWipReason, ITEM_CODE_LEN_EXCEED, diffQty) == false)
                                return;

                            exceedLengthQty = diffQty;
                                
                            SetInputQty();
                            dgWipReason.Refresh();
                            dgLotInfo.Refresh(false);
                        }
                    }, new object[] { diffQty + txtUnit.Text });
                }
                else
                {
                    SetInputQty();
                }
            }
            else
            {
                if (Convert.ToDouble(txtInputQty.Value) > Convert.ToDouble(txtParentQty.Value))
                {
                    Util.MessageValidation("SFU1614");  //생산량이 투입량을 초과할 수 없습니다.
                    return;
                }
                else
                {
                    SetInputQty();
                }
            }
            isChangeInputFocus = false; // FOCUS 초기화


            if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                dgWipReason.Refresh(false);

            dgLotInfo.Refresh(false);
        }

        private DataSet dtDataCollectOfChildrenQuality(C1DataGrid dg)
        {
            // 사용안하는 메서드라 SLITTER CUT의 BINDING메서드로 용도 변경 사용 [2017-02-01]
            DataSet inDataSet = new DataSet();

            DataTable IndataTable = inDataSet.Tables.Add("INDATA");
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("CUT_ID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = IndataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["PROCID"] = _PROCID;
            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            inDataRow["CUT_ID"] = Util.NVC(_CUTID);
            inDataRow["USERID"] = LoginInfo.USERID;
            IndataTable.Rows.Add(inDataRow);

            DataTable IndataDetailTable = inDataSet.Tables.Add("IN_DATA");
            IndataDetailTable.Columns.Add("CLCTITEM", typeof(string));
            IndataDetailTable.Columns.Add("VERSION", typeof(string));
            IndataDetailTable.Columns.Add("CLCTVAL01", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            foreach (DataRow _iRow in dt.Rows)
            {
                DataRow inDetailDataRow = null;
                inDetailDataRow = IndataDetailTable.NewRow();
                inDetailDataRow["CLCTITEM"] = _iRow["CLCTITEM"];
                inDetailDataRow["VERSION"] = 0;
                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();
                //inDetailDataRow["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]);

                IndataDetailTable.Rows.Add(inDetailDataRow);
            }
            return inDataSet;
        }

        public bool WorkOrder_chk()
        {
            bool _Woder = true;
            if (new Util().GetDataGridCheckFirstRowIndex(WORKORDER_GRID, "CHK") != -1)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(WORKORDER_GRID, "CHK");

                if (Util.NVC(DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                {
                    if (Util.NVC(DataTableConverter.GetValue(WORKORDER_GRID.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(WORKORDER_GRID.Rows[idx].DataItem, "WOID")))
                    {
                        Util.MessageValidation("SFU1436");
                        _Woder = false;
                    }
                    else
                    {
                        _Woder = true;
                    }
                }
            }
            return _Woder;
        }

        private bool ValidateConfirmSlitter()
        {
            if (dgLotInfo.GetRowCount() < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1702"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);    //실적 Lot을 선택 해 주세요.
                return false;
            }

            if (!ValidateProductQty())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (!ValidVersion())
                return false;

            if (!ValidLaneQty())
                return false;

            if (!ValidCutOverProdQty())
                return false;

            if (!ValidConfirmQty())
                return false;

            if (!ValidQualityRequired())
                return false;

            if (!ValidQualitySpecRequired())
                return false;

            if (!ValidDataCollect())
                return false;

            return true;
        }

        private bool ValidateProductQty()
        {
            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            if (Util.NVC_Decimal(dt.Rows[0]["INPUT_QTY"]) <= 0)
            {
                Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                return false;
            }

            return true;
        }

        private bool ValidShift()
        {
            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력하세요.
                return false;
            }

            return true;
        }

        private bool ValidOperator()
        {
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }

            return true;
        }

        private bool ValidVersion()
        {
            if (string.IsNullOrEmpty(txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
            return true;
        }

        private bool ValidLaneQty()
        {
            if (string.IsNullOrEmpty(Util.NVC(txtLaneQty.Value)) || txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");  //Lane 정보가 없습니다
                return false;
            }
            return true;
        }

        private bool ValidCutOverProdQty()
        {
            if (_DT_OUT_PRODUCT.Rows.Count > 1)
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                //int iCount = isResnCountUse == true ? 1 : 0;

                double inputQty = Convert.ToDouble(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);

                for (int i = dgWipReason.Columns["ALL"].Index; i < dgWipReason.Columns["COSTCENTERID"].Index; i++)
                {
                    if (Util.NVC(dgWipReason.Columns[i].Name).Equals("ALL") ||
                        Util.NVC(dgWipReason.Columns[i].Name).EndsWith("NUM") ||
                        Util.NVC(dgWipReason.Columns[i].Name).EndsWith("CNT") ||
                        Util.NVC(dgWipReason.Columns[i].Name).EndsWith("RESN_TOT_CHK") ||
                        Util.NVC(dgWipReason.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY")) continue;

                    if (inputQty < SumDefectQty(dgWipReason, dgWipReason.Columns[i].Name))
                    {
                        Util.MessageValidation("SFU3236");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidConfirmQty()
        {
            //decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);
            
            //if (exceedLengthQty > 0)
            //{
            //    // 투입량 <> (생산량 - 길이초과) 여야 확정 가능
            //    if (Util.NVC_Decimal(txtParentQty.Value) != (inputQty - exceedLengthQty))
            //    {
            //        Util.MessageValidation("SFU3417");  //길이초과 입력 시 잔량이 0이어야 합니다.
            //        return false;
            //    }
            //}

            return true;
        }

        private bool ValidQualityRequired()
        {
            if (dgQuality.Visibility == Visibility.Visible && dgQuality2.Visibility != Visibility.Visible && dgQuality.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dgQuality.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    bool isValid = false;
                    DataRow[] filterRows = DataTableConverter.Convert(dgQuality.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (!string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) && !string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (isValid == false)
                    {
                        Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                        return false;
                    }
                }
            }

            if (dgQuality2.Visibility == Visibility.Visible && dgQuality2.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dgQuality2.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    bool isValid = false;
                    DataRow[] filterRows = DataTableConverter.Convert(dgQuality2.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (!string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) && !string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (isValid == false)
                    {
                        Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidQualitySpecRequired()
        {
            if (dgQuality.Visibility == Visibility.Visible && dgQuality2.Visibility != Visibility.Visible && dgQuality.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dgQuality.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(dgQuality.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }

            if (dgQuality2.Visibility == Visibility.Visible && dgQuality2.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dgQuality2.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    string itemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(dgQuality2.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidDataCollect()
        {
            if (isChangeQuality)
            {
                Util.MessageValidation("SFU1999");  //품질 정보를 저장하세요.
                return false;
            }

            if (isChangeMaterial)
            {
                Util.MessageValidation("SFU1818");  //자재 정보를 저장하세요.
                return false;
            }

            if (isChagneDefectTag)
            {
                Util.MessageValidation("SFU3409");  //불량태그 정보를 저장하세요.
                return false;
            }

            if (isChangeColorTag)
            {
                Util.MessageValidation("SFU3410");  //색지 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }

            if (isChangeCotton)
            {
                Util.MessageValidation("SFU4913");  //면상태일지 정보를 저장하세요.
                return false;
            }
            return true;
        }

        private bool ValidLaneWrongQty()
        {
            //if (!_PROCID.Equals(Process.PRE_MIXING) && !_PROCID.Equals(Process.BS) && !_PROCID.Equals(Process.CMC) && !_PROCID.Equals(Process.MIXING) && !_PROCID.Equals(Process.SLITTING))
            //{
            //    if (_PROCID.Equals(Process.HALF_SLITTING))
            //    {
            //        for (int i = LOTINFO_GRID.TopRows.Count; i < (LOTINFO_GRID.Rows.Count - LOTINFO_GRID.BottomRows.Count); i++)
            //            if (Util.NVC_Decimal(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "LANE_QTY")) == 1)
            //                return false;
            //    }
            //    else
            //    {
            //        if (txtLaneQty.Value == 1)
            //            return false;
            //    }
            //}
            return true;
        }

        private bool IsCoaterProdVersion()
        {
            // 1. 공정체크
            if (!string.Equals(_PROCID, Process.COATING))
                return false;

            // 2. 입력된 VERSION 체크
            if (string.IsNullOrEmpty(txtVersion.Text))
                return false;

            // 3. 양산버전 이외는 체크 안함
            System.Text.RegularExpressions.Regex engRegex = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]");
            if (engRegex.IsMatch(txtVersion.Text.Substring(0, 1)) == true)
                return false;

            // 4. 1번 CUT인지 확인
            string sCut = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CUT"));
            if (string.IsNullOrEmpty(sCut) || !string.Equals(sCut, "1"))
                return false;

            return true;
        }

        private bool ValidateDefect(C1DataGrid datagrid)
        {
            if (datagrid.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return false;
            }
            
            // 길이초과 입력 시 반영 안해줌
            if (string.Equals(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "PRCS_ITEM_CODE"), ITEM_CODE_LEN_EXCEED))
            {
                // 전 공정 횟수 관리를 위하여 로직 변경 (C20190416_75868 ) [2019-04-17]
                //int iCount = isResnCountUse == true ? 1 : 0;

                decimal inputQty = 0;
                decimal inputLengthQty = 0;

                if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                    inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "ALL"));
                else
                    inputLengthQty = Util.NVC_Decimal(DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "RESNQTY"));

                if (inputLengthQty > 0)
                {
                    inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_QTY")) / (_DT_OUT_PRODUCT.Rows.Count < 1 ? 1 : _DT_OUT_PRODUCT.Rows.Count);

                    if (Util.NVC_Decimal(txtParentQty.Value) > inputQty)
                    {
                        Util.MessageValidation("SFU3424");  // FINAL CUT이 아닌 경우 길이초과 입력 불가
                        DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);

                        if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                            for (int i = dgWipReason.Columns["ALL"].Index; i < dgWipReason.Columns["COSTCENTERID"].Index; i++)
                                if (!Util.NVC(datagrid.Columns[i].Name).Equals("ALL") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("NUM") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("CNT") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("RESN_TOT_CHK") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                    DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.Columns[i].Name, 0);

                        exceedLengthQty = 0;
                        return false;
                    }

                    if (inputLengthQty > (inputQty - Util.NVC_Decimal(txtParentQty.Value)))
                    {
                        Util.MessageValidation("SFU3422", (inputQty - Util.NVC_Decimal(txtParentQty.Value)) + txtUnit.Text);    // 길이초과수량을 초과하였습니다.[현재 실적에서 길이초과는 %1까지 입력 가능합니다.] 
                        DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.CurrentCell.Column.Name, null);

                        if (string.Equals(_PROCID, Process.SLITTING) || string.Equals(_PROCID, Process.SRS_SLITTING) || string.Equals(_PROCID, Process.HALF_SLITTING))
                            for (int i = dgWipReason.Columns["ALL"].Index; i < dgWipReason.Columns["COSTCENTERID"].Index; i++)
                                if (!Util.NVC(datagrid.Columns[i].Name).Equals("ALL") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("NUM") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("CNT") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("RESN_TOT_CHK") &&
                                    !Util.NVC(datagrid.Columns[i].Name).EndsWith("FRST_AUTO_RSLT_RESNQTY"))
                                    DataTableConverter.SetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, datagrid.Columns[i].Name, 0);

                        exceedLengthQty = 0;
                        return false;
                    }
                }
            }

            return true;
        }

        private void SetCalcLackQty(int iChgRow, string sChgColName, bool bAllColYN = false)
        {            
            if (dgWipReason?.Rows?.Count < 1 || iChgRow < dgWipReason?.TopRows?.Count) return;
            if (!Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[iChgRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR)) return;

            string sColName = sChgColName.Replace("NUM", "").Replace("CNT", "").Replace("RESN_TOT_CHK", "").Replace("FRST_AUTO_RSLT_RESNQTY", "");

            if (!dgWipReason.Columns.Contains(sColName)) return;
            if (dgWipReason.Columns[sColName].IsReadOnly) return;

            decimal IncrQtySum = 0;         // 생산수량 증가 합계
            decimal InputQty = 0;           // 입력 수량.
            decimal LengthLack = 0;         // 길이부족 수량.
            decimal maxLenLack = 0;         // 최대 수량.
            decimal AnotherIncrQtySum = 0;  // 다른 생산수량 증가 입력값 합계 

            ArrayList aList = new ArrayList();


            // 1. 입력 수량이 길이초과 수량 보다 큰 경우 (자동실적 확정수량의 합계 수량 모두 등록

            int iLenLackRow = Util.gridFindDataRow(ref dgWipReason, "PRCS_ITEM_CODE", ITEM_CODE_LEN_LACK, false);

            if (iLenLackRow < 0) return;

            LengthLack = Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iLenLackRow].DataItem, sColName + "FRST_AUTO_RSLT_RESNQTY"));
            InputQty = bAllColYN ? Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iChgRow].DataItem, "ALL")) : Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iChgRow].DataItem, sColName));

            for (int iRow = dgWipReason.TopRows.Count; iRow < dgWipReason.GetRowCount(); iRow++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[iRow].DataItem, "PRCS_ITEM_CODE")).Equals(ITEM_CODE_PROD_QTY_INCR))
                {
                    if (iRow != dgWipReason.SelectedIndex)
                    {
                        IncrQtySum += Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iRow].DataItem, sColName + "FRST_AUTO_RSLT_RESNQTY"));
                        AnotherIncrQtySum += Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iRow].DataItem, sColName));

                        aList.Add(iRow);
                    }
                }
            }
            
            maxLenLack = IncrQtySum +
                         Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[iChgRow].DataItem, sColName + "FRST_AUTO_RSLT_RESNQTY")) +
                         LengthLack;
            
            if (InputQty > maxLenLack)
            {
                //Util.MessageValidation("SFU5132", DataTableConverter.GetValue(datagrid.Rows[datagrid.SelectedIndex].DataItem, "RESNNAME"));  // %1에 길이부족 수량보다 크게 입력할 수 없습니다.

                //if (bAllColYN)
                //    DataTableConverter.SetValue(dgWipReason.Rows[iChgRow].DataItem, "ALL", maxLenLack);

                DataTableConverter.SetValue(dgWipReason.Rows[iChgRow].DataItem, sColName, maxLenLack);

                // 길이부족 수량 설정.
                DataTableConverter.SetValue(dgWipReason.Rows[iLenLackRow].DataItem, sColName, 0);

                GetSumCutDefectQty(dgWipReason, iChgRow, dgWipReason.Columns[sColName].Index);
                GetSumCutDefectQty(dgWipReason, iLenLackRow, dgWipReason.Columns[sColName].Index);

                // 생산수량 증가 항목 초기화.
                foreach (int iFndRow in aList)
                {
                    DataTableConverter.SetValue(dgWipReason.Rows[iFndRow].DataItem, sColName, 0);

                    GetSumCutDefectQty(dgWipReason, iFndRow, dgWipReason.Columns[sColName].Index);
                }
            }
            else
            {
                decimal calQty = 0;                 // 길이부족 수량 계산 수량.

                calQty = maxLenLack - InputQty - AnotherIncrQtySum;
                if (calQty < 0)
                {
                    //if (bAllColYN)
                    //    DataTableConverter.SetValue(dgWipReason.Rows[iChgRow].DataItem, "ALL", maxLenLack - AnotherIncrQtySum);

                    DataTableConverter.SetValue(dgWipReason.Rows[iChgRow].DataItem, sColName, maxLenLack - AnotherIncrQtySum);
                    DataTableConverter.SetValue(dgWipReason.Rows[iLenLackRow].DataItem, sColName, 0);

                    GetSumCutDefectQty(dgWipReason, iChgRow, dgWipReason.Columns[sColName].Index);
                    GetSumCutDefectQty(dgWipReason, iLenLackRow, dgWipReason.Columns[sColName].Index);
                }
                else
                {
                    DataTableConverter.SetValue(dgWipReason.Rows[iChgRow].DataItem, sColName, InputQty);
                    DataTableConverter.SetValue(dgWipReason.Rows[iLenLackRow].DataItem, sColName, calQty);

                    GetSumCutDefectQty(dgWipReason, iChgRow, dgWipReason.Columns[sColName].Index);
                    GetSumCutDefectQty(dgWipReason, iLenLackRow, dgWipReason.Columns[sColName].Index);
                }
            }
        }
        #endregion

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
                dtRow["PROCID"] = _PROCID;
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

                    ConfirmDispatcher(true);
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

                for (int i = 0; i < _DT_OUT_PRODUCT.Rows.Count; i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(_DT_OUT_PRODUCT.Rows[i]["LOTID"]);
                    //newRow["WIPSEQ"] = null;
                    newRow["WORKER_NAME"] = sWrokerName;
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }
                
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
    }
}
