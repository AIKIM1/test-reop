/*************************************************************************************
 Created Date : 2022.06.29
      Creator : 오화백
   Decription : 창고 수동출고 
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.29  오화백 과장 : Initial Created.    
  2023.07.31  강성묵      : 공Carrier, 정보불일치 합계 클릭시 ELTR_TYPE_NAME 적용
  2023.08.31  오화백      : MES HOLD, QMS HOLD,  유효일자 체크후 색깔표시
  2023.10.10  김태우      : NFF 보빈ID, 캐리어ID 대신 대표랏(REP_LOT)사용 하는경우 적용
  2023.11.13  김태우      : NFF에서 WWW 창고 사용시 대표랏ID 컬럼 해더명 변경
  2023.11.15  오화백      : BR_MHS_REG_TRF_CMD_BY_UI_2에 LANGID 파라미터 추가
  2023.11.17  양영재   [E20231023-000302] - MES 생산창고수동창고 예약시 무지부/권취방향 Validation 추가
  2024.08.26  김도형      : [E20240809-001478] ESHM Improvement Adding column QMS HOLD at storage type ROL-SLT Jumbo roll warehouse
  2025.04.23  이민형      : Gird 순서 변경 및 Carrierid 조회조건 추가
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
    /// MCS001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_081 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private double _maxcheckCount = 0;
        private string _userId;
        private string _selectedRadioButtonValue;
        private string _projectName;
        private string _productVersion;
        private string _halfSlitterSideCode;
        private string _productCode;
        private string _holdCode;
        private string _pastDay;
        private string _lotId;
        private string _faultyType;
        private string _skidType;


        private string _emptybobbinState;
        private string _equipmentCode;
        private string _electrodeTypeCode;
        private string _cstTypeCode;
        private string _abNormalReasonCode;
        private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedQmsHold_ETC;
        private string _Skid_Use_Chk;
        //권취
        private string _em_section_roll_dirctn;
        private string _eltr_type_code_lot;

        //private string _dst_eqptID;
        //private string _dst_portID;

        private DataTable _requestTransferInfoTable;
        private bool _isAdminAuthority;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private string _FastTrackLot;
        private string _FastFlag;

        //[E20231023-000302]
        DataSet inDataSet = null;
        private bool ischecked = false;

        public MCS001_081()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 권한
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeRequestTransferTable();
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);
        }

        /// <summary>
        /// 화면 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitializeCombo();
            rdoCarrier.IsChecked = true;
            rdoPort.IsChecked = true;
            SelectReleaseCount();
            Chk_Skid_Use();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

        private void InitializeRequestTransferTable()
        {
            _requestTransferInfoTable = new DataTable();
            _requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRF_STAT", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRFID", typeof(string));
            _requestTransferInfoTable.Columns.Add("SRC_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("DST_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("JOBID", typeof(string));
        }
        /// <summary>
        ///  그리드 셋팅
        /// </summary>
        private void InitializeGrid()
        {
            dgStore.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgStore.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);
            //FastTrack 적용여부 체크
            if (ChkFastTrackOWNER())
            {
                dgIssueTargetInfo.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                btnFastTrack.Visibility = Visibility.Visible;
            }

        }
        /// <summary>
        /// 콤보 셋팅
        /// </summary>
        private void InitializeCombo()
        {
            // 창고 유형 콤보박스
            SetStockerTypeCombo(cboStockerType);

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // HOLD 사유 콤보박스
            SetHoldCombo(cboHoldReason);

            // QA불량유형
            SetFalutyTypeCombo(cboFaultyType);


        }
        #endregion

        #region Event

        #region 창고 유형 콤보 이벤트  : cboStockerType_SelectedValueChanged()

        /// <summary>
        /// 창고유형
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //조건출고 / FIFO 집계
            dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
            dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
            dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;

            dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Collapsed;
            dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Collapsed;

            //공Carrier 집계
            dgStoreByEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

            //조건출고 / FIFO LIST
            dgIssueTargetInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
            dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
            dgIssueTargetInfo.Columns["ROLL_DIRECTION"].Visibility = Visibility.Visible;


            dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Collapsed;

            dgIssueTargetInfoByEmptyCarrier.Columns["REP_CSTID"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Columns["CSTID"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Columns["LOAD_LOC_CODE"].Visibility = Visibility.Collapsed;

            dgStoreByEmptyCarrier.Columns["REP_CST_QTY"].Visibility = Visibility.Collapsed;
            dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Visible;
            dgStoreByEmptyCarrier.Columns["BBN_QTY"].Header = ObjectDic.Instance.GetObjectName("CST수");

            //rdoProcEqpt.Visibility = Visibility.Collapsed;
            if (IsProcEqptPortVisibility(cboStockerType.SelectedValue.GetString()) && rdoCarrier.IsChecked == true)
            {
                rdoProcEqpt.Visibility = Visibility.Visible;
            }
            else
            {
                rdoProcEqpt.Visibility = Visibility.Collapsed;
            }

            if (cboStockerType.SelectedValue.GetString() == "JRW")
            {
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;
                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }

                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }

            }
            else if (cboStockerType.SelectedValue.GetString() == "PCW")
            {
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;

                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }

                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;

                // 스태킹 완성 창고일 경우 리스트 변경
                if (dgIssueTargetInfo_SWW.IsVisible == true)
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                }
                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }


                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }

            }

            else if (cboStockerType.SelectedValue.GetString() == "NPW")
            {
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;

                // 스태킹 완성 창고일 경우 리스트 변경
                if (dgIssueTargetInfo_SWW.IsVisible == true)
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                }
                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }

                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Visible;

                // 스태킹 완성 창고일 경우 리스트 변경
                if (dgIssueTargetInfo_SWW.IsVisible == true)
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                }

                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }

                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "SWW")
            {
                //조건출고 / FIFO 집계
                dgStore.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                dgStore.Columns["HALF_ROLL"].Visibility = Visibility.Collapsed;
                //공Carrier 집계
                dgStoreByEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;

                //조건출고 / FIFO LIST
                dgIssueTargetInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfo.Columns["ROLL_DIRECTION"].Visibility = Visibility.Collapsed;

                // 스태킹 완성 창고일 경우 리스트 변경
                if (dgIssueTargetInfo.IsVisible == true)
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Collapsed;
                }

                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }


                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }

            }

            else if (cboStockerType.SelectedValue.GetString() == "JTW")
            {
                dgStoreByEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgStoreByEmptyCarrier.Columns["REP_CST_QTY"].Visibility = Visibility.Visible;
                dgIssueTargetInfoByEmptyCarrier.Columns["REP_CSTID"].Visibility = Visibility.Visible;
                dgIssueTargetInfoByEmptyCarrier.Columns["CSTID"].Visibility = Visibility.Visible;
                dgIssueTargetInfoByEmptyCarrier.Columns["LOAD_LOC_CODE"].Visibility = Visibility.Visible;
                dgIssueTargetInfoByEmptyCarrier.Columns["SKID_ID"].Visibility = Visibility.Collapsed;
                dgIssueTargetInfoByEmptyCarrier.Columns["BOBBIN_ID"].Visibility = Visibility.Collapsed;

                dgStoreByEmptyCarrier.Columns["EMPTY_BBN_YN"].Visibility = Visibility.Collapsed;
                dgStoreByEmptyCarrier.Columns["BBN_QTY"].Header = ObjectDic.Instance.GetObjectName("CARRIER 수");
            }

            else if (cboStockerType.SelectedValue.GetString() == "ROW") // [E20240809-001478] ESHM Improvement Adding column QMS HOLD at storage type ROL-SLT Jumbo roll warehouse
            {
                if (rdoCarrier.IsChecked == true)
                {
                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }

                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgIssueTargetInfo.Columns["QMS_HOLD_FLAG_OLD"].Visibility = Visibility.Visible;
                    }
                }

            }

            RepLotUseForm(); //캐리어ID 사용 안하고 대표랏 사용할경우 화면 컬럼 변경

            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        #endregion

        #region 극성 콤보 이벤트  : cboElectrodeType_SelectedValueChanged()
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        #endregion

        #region 창고 콤보 이벤트 : cboStocker_SelectedValueChanged()
        /// <summary>
        /// 창고 콤보
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }

        #endregion

        #region 조회 버튼 : btnSearch_Click()
        /// <summary>
        /// 조회 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            // 공캐리어 조회
            if (rdoEmptyCarrier.IsChecked == true)
            {
                SelectWareHouseEmptyCarrier();
            }
            //오류 Rack 조회
            else if (rdoNoReadCarrier.IsChecked == true)
            {
                SelectWareHouseNoReadCarrier();
            }
            //정보 불일치 조회
            else if (rdoAbNormalCarrier.IsChecked == true)
            {
                SelectWareHouseAbNormalCarrier();
            }
            //FIFO, 조건출고 조회
            else
            {
                SelectManualOutInventory();
            }
        }

        #endregion

        #region 수동출고 버튼 : btnManualIssue_Click()
        /// <summary>
        /// 수동출고 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            if(rdoProcEqpt.IsChecked == true)
            {
                // [E20231023-000302] 무지부/권취방향 Validation 추가
                if (IsAreaCommonCodeUse("EQPT_WIND_DIRECTION_CHK_USE"))
                {
                    ChkEqptWindDirection();
                }
                else
                {
                    SaveProcManuallssue();
                }
            }
            else
            {
                SaveManualIssueByEsnb();
            }


        }

        #endregion

        #region 수동출고 취소버튼 : btnTransferCancel_Click()
        /// <summary>
        /// 수동출고 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;

            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;
            C1DataGrid dg;
            string _Carrierid = string.Empty;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
                _Carrierid = "SKID_ID";
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
                _Carrierid = "CARRIERID";
            }
            else
            {
                if (cboStockerType.SelectedValue.ToString() == "SWW")
                {
                    dg = dgIssueTargetInfo_SWW;
                    _Carrierid = "SKID_ID";
                }
                else
                {
                    dg = dgIssueTargetInfo;
                    _Carrierid = "SKID_ID";
                }

            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("RequestTransferId", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["RequestTransferId"] = DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString();
                    newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                    inTable.Rows.Add(newRow);
                }
            }

            CMM_MHS_TRANSFER_CANCEL popupTransferCancel = new CMM_MHS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = inTable;
            if (rdoProcEqpt.IsChecked == true)
            {
                parameters[1] = "proc";
            }
            else
            {
                parameters[1] = "";
            }

            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        #endregion

        #region 수동출고 취소 팝업 닫기 : popupTransferCancel_Closed()
        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MHS_TRANSFER_CANCEL popup = sender as CMM_MHS_TRANSFER_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    SelectWareHouseAbNormalCarrier();
                    SelectWareHouseAbNormalCarrierList();
                }
                else
                {
                    SelectManualOutInventory();
                    SelectManualOutInventoryList(true);
                }
            }
        }


        #endregion

        #region 예약순서변경 : btnChangePriorityNo_Click()
        /// <summary>
        /// 예약순서변경 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangePriorityNo_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReservChange()) return;
            //DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table.Select("ISS_RSV_FLAG = 'Y'").CopyToDataTable(); ;
            MCS001_081_TRANS_RESERVE popupTransfer = new MCS001_081_TRANS_RESERVE { FrameOperation = FrameOperation };
            object[] parameters = new object[20];
            parameters[0] = _selectedRadioButtonValue;
            parameters[1] = cboStockerType.SelectedValue.GetString();
            parameters[2] = _equipmentCode;
            parameters[3] = _productVersion;
            parameters[4] = _productCode;
            parameters[5] = _projectName;
            parameters[6] = _selectedWipHold;
            parameters[7] = _lotId;
            parameters[8] = _halfSlitterSideCode;
            parameters[9] = _em_section_roll_dirctn;
            parameters[10] = _holdCode;
            parameters[11] = _pastDay;
            parameters[12] = _electrodeTypeCode;
            parameters[13] = _eltr_type_code_lot;
            parameters[14] = _selectedQmsHold;
            parameters[15] = _faultyType;
            parameters[16] = _selectedQmsHold_ETC;
            parameters[17] = ((ContentControl)(cboIssueProcEqpt.Items[cboIssueProcEqpt.SelectedIndex])).Tag.ToString();

            foreach (var item in dgIssueTargetInfo.Rows)
            {
                //if ((int)item.DataItem.GetValue("chk") == 1)
                if (item.DataItem.GetValue("CHK").GetInt() == 1)
                {
                    parameters[18] = item.DataItem.GetValue("SKID_ID");
                    parameters[19] = item.DataItem.GetValue("EQPTNAME");
                    break;
                }
            }

            if (parameters[18] == null)
                return;


            C1WindowExtension.SetParameters(popupTransfer, parameters);
            popupTransfer.Closed += popupTransfer_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransfer.ShowModal()));
        }

        #endregion

        #region 예약순서변경 팝업 닫기 : ChangePriorityNo_Closed()
        private void popupTransfer_Closed(object sender, EventArgs e)
        {
            MCS001_081_TRANS_RESERVE popup = sender as MCS001_081_TRANS_RESERVE;
            if (popup != null && popup.IsUpdated)
            {
                SelectManualOutInventory();
                SelectManualOutInventoryList(true);
            }
        }


        #endregion




        #region 유형 Radio 버튼 클릭 : rdoRelease_Checked()
        /// <summary>
        /// 유형 Radio 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoRelease_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;


            spFiFo.Visibility = Visibility.Collapsed;
            spCondition.Visibility = Visibility.Collapsed;
            dgStore.Visibility = Visibility.Collapsed;
            dgStoreByEmptyCarrier.Visibility = Visibility.Collapsed;
            dgStoreByNoReadCarrier.Visibility = Visibility.Collapsed;
            dgStoreByAbNormalCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByNoReadCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByAbNormalCarrier.Visibility = Visibility.Collapsed;
            dgIssueTargetInfo_SWW.Visibility = Visibility.Collapsed;

            dgIssueTargetInfo.Columns["ISS_RSV_FLAG"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["ISS_RSV_PRIORITY_NO"].Visibility = Visibility.Collapsed;
            dgIssueTargetInfo.Columns["PORT_NAME"].Visibility = Visibility.Collapsed;

            dgStore.Columns["LOT_HOLD_QTY"].Visibility = Visibility.Collapsed;
            dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Collapsed;
            dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Collapsed;
            dgStore.Columns["LOT_TOTAL_QTY"].Visibility = Visibility.Collapsed;

            txtReturnSrc.Visibility = Visibility.Visible;
            rdoPort.Visibility = Visibility.Visible;
            rdoWareHouse.Visibility = Visibility.Visible;
            rdoProcEqpt.Visibility = Visibility.Collapsed;
            txtSrc.Visibility = Visibility.Visible;
            cboIssuePort.Visibility = Visibility.Visible;
            btnManualIssue.Visibility = Visibility.Visible;
            btnTransferCancel.Visibility = Visibility.Visible;


            Left.Visibility = Visibility.Visible;
            Left_NoRead.Visibility = Visibility.Collapsed;

            switch (radioButton.Name)
            {
                case "rdoCarrier":
                    //spFiFo.Visibility = Visibility.Visible;
                    spCondition.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                    dgStore.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NORMAL";
                    dgStore.Columns["LOT_HOLD_QTY"].Visibility = Visibility.Visible;
                    dgStore.Columns["LOT_TOTAL_QTY"].Visibility = Visibility.Visible;

                    //EQGR별 QMS 체크할 BLOCK_TYPE_CODE 체크
                    if (ChkQmsBlock_Type_code(cboStockerType.SelectedValue.GetString()))
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgStore.Columns["LOT_HOLD_QMS_QTY_ETC"].Visibility = Visibility.Visible;
                    }


                    if (IsProcEqptPortVisibility(cboStockerType.SelectedValue.GetString()) && rdoProcEqpt.IsChecked == true)
                    {
                        rdoProcEqpt.Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["ISS_RSV_FLAG"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["ISS_RSV_PRIORITY_NO"].Visibility = Visibility.Visible;
                        dgIssueTargetInfo.Columns["PORT_NAME"].Visibility = Visibility.Visible;
                    }

                    break;
                case "rdoEmptyCarrier":
                    dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Visible;
                    dgStoreByEmptyCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "EMPTYCARRIER";

                    break;
                case "rdoNoReadCarrier":
                    dgStoreByNoReadCarrier.Visibility = Visibility.Visible;
                    dgIssueTargetInfoByNoReadCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "NOREADCARRIER";
                    // if (_isAdminAuthority) btnDataIssue.Visibility = Visibility.Visible;

                    btnManualIssue.Visibility = Visibility.Collapsed;
                    btnTransferCancel.Visibility = Visibility.Collapsed;
                    txtReturnSrc.Visibility = Visibility.Collapsed;
                    rdoPort.Visibility = Visibility.Collapsed;
                    rdoWareHouse.Visibility = Visibility.Collapsed;
                    txtSrc.Visibility = Visibility.Collapsed;
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    cboIssueWareHouse.Visibility = Visibility.Collapsed;
                    cboIssueProcEqpt.Visibility = Visibility.Collapsed;
                    Left.Visibility = Visibility.Collapsed;
                    Left_NoRead.Visibility = Visibility.Visible;

                    break;
                case "rdoAbNormalCarrier":
                    dgStoreByAbNormalCarrier.Visibility = Visibility.Visible;
                    dgIssueTargetInfoByAbNormalCarrier.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "ABNORMALCARRIER";

                    break;
            }

            ClearControl();
            //cboStockerType.SelectedValueChanged -= cboStockerType_SelectedValueChanged;
            SetStockerTypeCombo(cboStockerType);
            //cboStockerType.SelectedValueChanged += cboStockerType_SelectedValueChanged;
            SetStockerCombo(cboStocker);

            if (string.Equals(_selectedRadioButtonValue, "NORMAL"))
            {
                if (cboStockerType.SelectedValue.GetString() == "JRW")
                {
                    dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                    dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgStore.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    dgStore.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }

                if (cboStockerType.SelectedValue.GetString() == "SWW")
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgIssueTargetInfo_SWW.Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region FIFO 출고 선택시  출고 수량  이벤트  : rowCount_LostFocus(), rowCount_ValueChanged()

        private void rowCount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_maxcheckCount < rowCount.Value)
            {
                rowCount.Value = _maxcheckCount;
            }

            //SetCheckedIssueTargetInfoGrid();
        }

        private void rowCount_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (_maxcheckCount < rowCount.Value)
                rowCount.Value = _maxcheckCount;

            //SetCheckedIssueTargetInfoGrid();
        }

        #endregion

        #region FIFO 출고, 조건출고  집계 리스트 이벤트 : dgStore_LoadedCellPresenter(), dgStore_UnloadedCellPresenter(),dgStore_MouseLeftButtonUp()

        /// <summary>
        ///  리스트 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStore_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    string[] columnStrings = new[] { "LOT_TOTAL_QTY", "LOT_QTY", "LOT_HOLD_QTY", "LOT_HOLD_QMS_QTY", "LOT_HOLD_QMS_QTY_ETC" };

                    if (columnStrings.Contains(e.Cell.Column.Name))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRJT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
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
        ///  리스트 색깔 처리
        private void dgStore_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        /// 창고 재고 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStore_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgStore == null || dgStore.CurrentCell == null || dgStore.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgStore.GetCellFromPoint(pnt);

                if (cell == null) return;


                // 선택한 셀의 Row 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dgStore.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;
                if (DataTableConverter.GetValue(drv, "LOT_TOTAL_QTY").GetString() == "0") return;


                _halfSlitterSideCode = null;
                _productVersion = null;
                _productCode = null;
                _holdCode = null;
                _pastDay = null;
                _lotId = null;
                _faultyType = null;
                _selectedWipHold = null;
                _selectedQmsHold = null;
                _selectedQmsHold_ETC = null;
                _equipmentCode = null;
                _electrodeTypeCode = null;
                _em_section_roll_dirctn = null;
                _eltr_type_code_lot = null;

                _projectName = string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "PRJT_NAME").GetString()) ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();

                if (!string.Equals(ObjectDic.Instance.GetObjectName("합계"), DataTableConverter.GetValue(drv, "PRJT_NAME").GetString()))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _halfSlitterSideCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "HALF_SLIT_SIDE").GetString()) ? null : DataTableConverter.GetValue(drv, "HALF_SLIT_SIDE").GetString();
                    _em_section_roll_dirctn = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "EM_SECTION_ROLL_DIRCTN").GetString()) ? null : DataTableConverter.GetValue(drv, "EM_SECTION_ROLL_DIRCTN").GetString();
                    if (rdoProcEqpt.IsChecked == true)
                    {
                        btnChangePriorityNo.IsEnabled = true;
                    }
                }
                else
                {
                    _equipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.GetString()) ? null : cboStocker.SelectedValue.GetString();

                    if (rdoProcEqpt.IsChecked == true)
                    {
                        btnChangePriorityNo.IsEnabled = false;
                    }

                }

                if (cell.Column.Name.Equals("LOT_QTY"))
                {
                    _selectedWipHold = "N";
                    _selectedQmsHold = "N";
                    _selectedQmsHold_ETC = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY"))
                {
                    _selectedWipHold = "Y";
                    _selectedQmsHold = null;
                    _selectedQmsHold_ETC = null;
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY"))
                {
                    _selectedQmsHold = "Y";
                    _selectedWipHold = "N";
                    _selectedQmsHold_ETC = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY_ETC"))
                {
                    _selectedQmsHold = "N";
                    _selectedWipHold = "N";
                    _selectedQmsHold_ETC = "Y";
                }

                _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();
                _productVersion = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "PROD_VER_CODE").GetString()) ? null : DataTableConverter.GetValue(drv, "PROD_VER_CODE").GetString();
                _productCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "PRODID").GetString()) ? null : DataTableConverter.GetValue(drv, "PRODID").GetString();
                _eltr_type_code_lot = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE_LOT").GetString()) ? null : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE_LOT").GetString();

                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                txtEquipmentName.Text = string.Empty;
                SelectManualOutInventoryList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgIssueTargetInfo_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgIssueTargetInfo.ItemsSource == null) return;

                SelectPortInfo(string.Empty);
                txtEquipmentName.Text = string.Empty;
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);
                SetProcEqpt(cboIssueProcEqpt, string.Empty);
                DataTable dt = ((DataView)dgIssueTargetInfo.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 공 Carrier 출고 집계 리스트 이벤트 : dgStoreByEmptyCarrier_LoadedCellPresenter(), dgStoreByEmptyCarrier_UnloadedCellPresenter(), dgStoreByEmptyCarrier_MouseLeftButtonUp()

        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByEmptyCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "BBN_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "BBN_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    if (cboStockerType.SelectedValue.ToString() == "SWW" || cboStockerType.SelectedValue.ToString() == "JTW")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD")), ObjectDic.Instance.GetObjectName("합계")))
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
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
                    else
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
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

                }
            }));
        }
        /// <summary>
        /// 리스트 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByEmptyCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        ///  창고 재고 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByEmptyCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _skidType = string.Empty;
                _emptybobbinState = string.Empty;
                _electrodeTypeCode = string.Empty;

                //_skidID = string.Empty;
                //_bobbinID = string.Empty;


                if (cboStockerType.SelectedValue.ToString() == "SWW")
                {
                    if (DataTableConverter.GetValue(drv, "CSTPROD").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                    {
                        _electrodeTypeCode = null;
                        _skidType = null;
                        _emptybobbinState = null;
                        //_skidID = null;
                        //_bobbinID = null;
                    }
                    else
                    {
                        if (cell.Column.Name.Equals("CSTPROD"))
                        {
                            _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                            _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                        else
                        {
                            _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                            _emptybobbinState = DataTableConverter.GetValue(drv, "EMPTY_BBN_YN").GetString();
                            _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                    }
                }
                else if(cboStockerType.SelectedValue.ToString() == "JTW")
                {
                    if (DataTableConverter.GetValue(drv, "CSTPROD").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                    {
                        _electrodeTypeCode = null;
                        _skidType = null;
                        _emptybobbinState = null;
                        //_skidID = null;
                        //_bobbinID = null;
                    }
                    else
                    {
                        _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                        _electrodeTypeCode = null;
                        _emptybobbinState = null;
                        
                    }
                }
                else
                {
                    if (DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                    {
                        // 2023.07.31 강성묵 공Carrier, 정보불일치 합계 클릭시 ELTR_TYPE_NAME 적용
                        //_electrodeTypeCode = null;
                        _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();
                        _skidType = null;
                        _emptybobbinState = null;

                    }
                    else
                    {
                        if (cell.Column.Name.Equals("ELTR_TYPE_NAME"))
                        {
                            _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                            _skidType = null;
                            _emptybobbinState = null;
                        }
                        else if (cell.Column.Name.Equals("CSTPROD"))
                        {
                            _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                            _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                        else
                        {
                            _skidType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                            _emptybobbinState = DataTableConverter.GetValue(drv, "EMPTY_BBN_YN").GetString();
                            _electrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                    }
                }


                txtEquipmentName.Text = string.Empty;
                SelectWareHouseEmptyCarrierList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 정보 불일치  집계 리스트 이벤트 : dgStoreByAbNormalCarrier_LoadedCellPresenter(), dgStoreByAbNormalCarrier_UnloadedCellPresenter(), dgStoreByAbNormalCarrier_MouseLeftButtonUp()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByAbNormalCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
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

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByAbNormalCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        /// 창고데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByAbNormalCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 위치
            int rowIdx = cell.Row.Index;
            DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            _equipmentCode = null;
            _abNormalReasonCode = null;

            if (DataTableConverter.GetValue(drv, "EQPTNAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
            {
                // 2023.07.31 강성묵 공Carrier, 정보불일치 합계 클릭시 ELTR_TYPE_NAME 적용
                _electrodeTypeCode = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();
            }
            else
            {
                // 2023.07.31 강성묵 공Carrier, 정보불일치 합계 클릭시 ELTR_TYPE_NAME 적용
                _electrodeTypeCode = null;

                if (cell.Column.Name.Equals("EQPTNAME"))
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                }
                else
                {
                    _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _abNormalReasonCode = DataTableConverter.GetValue(drv, "ABNORM_TRF_RSN_CODE").GetString();
                }
            }

            txtEquipmentName.Text = string.Empty;
            SelectWareHouseAbNormalCarrierList();
        }

        #endregion

        #region 비정상 RACK 집계 리스트 이벤트 : dgStoreByNoReadCarrier_LoadedCellPresenter(), dgStoreByNoReadCarrier_UnloadedCellPresenter(), dgStoreByNoReadCarrier_MouseLeftButtonUp()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByNoReadCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
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

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByNoReadCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        /// <summary>
        /// 창고데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStoreByNoReadCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 위치
            int rowIdx = cell.Row.Index;
            DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            _equipmentCode = null;
            _cstTypeCode = null;

            if (DataTableConverter.GetValue(drv, "EQPTNAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
            {
                _equipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                _cstTypeCode = DataTableConverter.GetValue(drv, "CST_TYPE_CODE").GetString();
            }

            txtEquipmentName.Text = string.Empty;
            SelectWareHouseNoReadCarrierList();

        }

        #endregion

        #region  조건출고 리스트 조회 조건 : cboHoldReason_SelectedValueChanged(), txtLotId_KeyDown(), txtPastDay_KeyDown(), cboFaultyType_SelectedValueChanged()

        /// <summary>
        ///  HOLD 사유
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboHoldReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectManualOutInventoryList();
            }
        }
        /// <summary>
        /// 랏 아이디
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {

                try
                {

                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {


                        for (int i = 0; i < dgIssueTargetInfo.Rows.Count; i++)
                        {
                            if (txtLotId.Text.ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "LOTID")).GetString())
                            {
                                DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 1);

                                dgIssueTargetInfo.EndEdit();
                                dgIssueTargetInfo.EndEditRow(true);
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "SKID_ID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else if (rdoWareHouse.IsChecked == true)
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }
                            else
                            {
                                SetProcEqpt(cboIssueProcEqpt, carrier);
                            }

                        }

                        txtLotId.Text = string.Empty;
                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        txtLotId.Text = string.Empty;
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;


            }
        }
        /// <summary>
        /// 경과일수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPastDay_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(_projectName))
                {
                    _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                    _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                    _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                    _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                    SelectManualOutInventoryList();
                }
            }
        }
        /// <summary>
        /// QA불량유형
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboFaultyType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                _holdCode = string.IsNullOrEmpty(cboHoldReason.SelectedValue.GetString()) ? null : cboHoldReason.SelectedValue.GetString();
                _pastDay = string.IsNullOrEmpty(txtPastDay.Text) ? null : txtPastDay.Text;
                _lotId = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                _faultyType = string.IsNullOrEmpty(cboFaultyType.SelectedValue.GetString()) ? null : cboFaultyType.SelectedValue.GetString();

                SelectManualOutInventoryList();
            }
        }

        #endregion

        #region  FIFO 출고, 조건 출고 리스트 이벤트  : dgIssueTargetInfo_LoadedCellPresenter(), dgIssueTargetInfo_UnloadedCellPresenter(),dgIssueTargetInfo_BeginningEdit(), dgIssueTargetInfo_MergingCells(), dgIssueTargetInfo_MouseDoubleClick()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                            if (convertFromString != null)
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                            if (convertFromString != null)
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                    else
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (Convert.ToString(e.Cell.Column.Name) == "HOLD_NAME")
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(243, 165, 165));
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                        else if (Convert.ToString(e.Cell.Column.Name) == "FINL_JUDG_NOTE")
                        {

                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMS_HOLD_FLAG")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(243, 165, 165));
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }

                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                            if (convertFromString != null)
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RACK_STAT_CODE")).Equals("DISABLE"))
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "RACK_STAT_NAME")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                    }
                    else
                    {
                        if (Convert.ToString(e.Cell.Column.Name) == "RACK_STAT_NAME")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            e.Cell.Presenter.Background = null;
                        }

                    }

                }

            }));
        }

        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        /// <summary>
        /// 리스트 내 체크박스 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfo;


                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 랏 클릭시 정보 조회 화면 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        #endregion

        #region  FIFO 출고, 조건 출고 리스트 이벤트(스태킹 완성창고)  : dgIssueTargetInfo_SWW_LoadedCellPresenter(), dgIssueTargetInfo-SWW_UnloadedCellPresenter(),dgIssueTargetInfo_SWW_BeginningEdit(), dgIssueTargetInfo_SWW_MergingCells(), dgIssueTargetInfo_SWW_MouseDoubleClick()
        /// <summary>
        /// FIFO 출고, 조건출고 리스트 머지 (스태킹 완성 창고)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_SWW_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfo_SWW.TopRows.Count; i < dgIssueTargetInfo_SWW.Rows.Count; i++)
                {

                    if (dgIssueTargetInfo_SWW.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo_SWW.Rows[i].DataItem, "SKID_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo_SWW.Rows[i].DataItem, "SKID_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfo_SWW.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CHK"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CMD_STAT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["ROW_NUM"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["ROW_NUM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CSTINDTTM"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CSTINDTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["PAST_DAY"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["PAST_DAY"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["RACK_NAME"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["RACK_NAME"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CHK"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CMD_STAT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["ROW_NUM"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["ROW_NUM"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["CSTINDTTM"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["CSTINDTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["PAST_DAY"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["PAST_DAY"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfo_SWW.GetCell(idxS, dgIssueTargetInfo_SWW.Columns["RACK_NAME"].Index), dgIssueTargetInfo_SWW.GetCell(idxE, dgIssueTargetInfo_SWW.Columns["RACK_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo_SWW.Rows[i].DataItem, "SKID_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 색깔처리 (스태킹 완성 창고)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_SWW_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    else if (Convert.ToString(e.Cell.Column.Name) == "HOLD_NAME")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(243, 165, 165));
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }

            }));
        }

        /// <summary>
        /// 색깔처리 (스태킹 완성 창고)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_SWW_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        /// <summary>
        /// 리스트 내 체크박스 체크 (스태킹 완성 창고)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_SWW_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfo_SWW;


                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 랏 클릭시 정보 조회 화면 이동 (스태킹 완성 창고)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfo_SWW_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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


        private void dgIssueTargetInfo_SWW_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            try
            {
                if (dgIssueTargetInfo_SWW.ItemsSource == null) return;

                SelectPortInfo(string.Empty);
                txtEquipmentName.Text = string.Empty;
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);
                SetProcEqpt(cboIssueProcEqpt, string.Empty);
                DataTable dt = ((DataView)dgIssueTargetInfo_SWW.ItemsSource).Table;
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 공 Carrier 출고 리스트 이벤트 : dgIssueTargetInfoByEmptyCarrier_BeginningEdit(), dgIssueTargetInfoByEmptyCarrier_MergingCells()
        /// <summary>
        /// 공 Carrier 출고 첵크 박스 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfoByEmptyCarrier_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfoByEmptyCarrier;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 공캐리어 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfoByEmptyCarrier_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfoByEmptyCarrier.TopRows.Count; i < dgIssueTargetInfoByEmptyCarrier.Rows.Count; i++)
                {

                    if (dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "SKID_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "SKID_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfoByEmptyCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "SKID_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        private void dgIssueTargetInfoByEmptyCarrierJTW_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfoByEmptyCarrier.TopRows.Count; i < dgIssueTargetInfoByEmptyCarrier.Rows.Count; i++)
                {

                    if (dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_CSTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_CSTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfoByEmptyCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByEmptyCarrier.GetCell(idxS, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index), dgIssueTargetInfoByEmptyCarrier.GetCell(idxE, dgIssueTargetInfoByEmptyCarrier.Columns["CMD_STAT_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[i].DataItem, "REP_CSTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region 정보 불일치 리스트 이벤트 : dgIssueTargetInfoByAbNormalCarrier_BeginningEdit(), dgIssueTargetInfoByAbNormalCarrier_MergingCells()
        /// <summary>
        ///  정보불일치 첵크 박스 체크시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfoByAbNormalCarrier_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                C1DataGrid dg = dgIssueTargetInfoByAbNormalCarrier;
                SelectWareHousePortInfo(dg, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 정보불일치 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgIssueTargetInfoByAbNormalCarrier_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgIssueTargetInfoByAbNormalCarrier.TopRows.Count; i < dgIssueTargetInfoByAbNormalCarrier.Rows.Count; i++)
                {

                    if (dgIssueTargetInfoByAbNormalCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[i].DataItem, "CSTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[i].DataItem, "CSTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgIssueTargetInfoByAbNormalCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByAbNormalCarrier.GetCell(idxS, dgIssueTargetInfoByAbNormalCarrier.Columns["CHK"].Index), dgIssueTargetInfoByAbNormalCarrier.GetCell(idxE, dgIssueTargetInfoByAbNormalCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgIssueTargetInfoByAbNormalCarrier.GetCell(idxS, dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRF_STAT"].Index), dgIssueTargetInfoByAbNormalCarrier.GetCell(idxE, dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRF_STAT"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByAbNormalCarrier.GetCell(idxS, dgIssueTargetInfoByAbNormalCarrier.Columns["CHK"].Index), dgIssueTargetInfoByAbNormalCarrier.GetCell(idxE, dgIssueTargetInfoByAbNormalCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgIssueTargetInfoByAbNormalCarrier.GetCell(idxS, dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRF_STAT"].Index), dgIssueTargetInfoByAbNormalCarrier.GetCell(idxE, dgIssueTargetInfoByAbNormalCarrier.Columns["REQ_TRF_STAT"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[i].DataItem, "CSTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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



        #endregion

        #region Port 리스트 이벤트 : dgPortInfo_LoadedCellPresenter(), dgPortInfo_UnloadedCellPresenter()
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPortInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_STAT_CODE").GetString() == "OUT_OF_SERVICE")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#BDBDBD");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }
        /// <summary>
        /// 색깔처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPortInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region 목적지 콤보박스 이벤트 : cboIssuePort_SelectedIndexChanged()
        /// <summary>
        /// 목적지 콤보박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboIssuePort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;

            C1ComboBox cb = sender as C1ComboBox;
            if (cb == null)
                return;

            try
            {
                if (rdoPort.IsChecked == true && cb == cboIssuePort && cboIssuePort != null && cboIssuePort.SelectedItem != null)
                {
                    int previousRowIndex = e.OldValue;
                    int currentRowIndex = e.NewValue;

                    string transferStateCode = ((ContentControl)(cboIssuePort.Items[currentRowIndex])).Name.GetString();

                    if (transferStateCode == "OUT_OF_SERVICE")
                    {
                        Util.MessageInfo("SFU8137");
                        cboIssuePort.SelectedIndex = previousRowIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Timer 셋팅 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// 타이머 셋 이벤트
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

        #region PORT 리스트 및 목적지 콤보박스 조회  : SelectWareHousePortInfo()
        /// <summary>
        /// PORT 리스트 및 목적지 콤보박스 조회
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="e"></param>
        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPTID").GetString();
                string currentequipmentName = DataTableConverter.GetValue(e.Row.DataItem, string.Equals(dg.Name, "dgIssueTargetInfoByEmptyCarrier") ? "EQPTNAME" : "EQPTNAME").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrier = DataTableConverter.GetValue(e.Row.DataItem, "CARRIERID").GetString();
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPTID").GetString();
                    if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EQPTID")).Equals(selectedequipmentCode))
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                //if (rdoProcEqpt.IsChecked == true && Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RACK_STAT_CODE")).Equals("DISABLE"))
                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RACK_STAT_CODE")).Equals("DISABLE"))
                {
                    e.Cancel = true;
                    return;
                }

                var query = (from t in ((DataView)dg.ItemsSource).Table.AsEnumerable()
                             where t.Field<Int64>("CHK") == 1   // 2024.10.23. 김영국 - Type형식이 맞지 않아 로직 수정 int - > int64
                             select t).ToList();

                if (checkValue == "0")
                {
                    if (query.Count() < 1)
                    {

                        if (rdoPort.IsChecked == true)
                        {
                            SetIssuePort(cboIssuePort, currentequipmentCode);
                            SelectPortInfo(currentequipmentCode);
                            txtEquipmentName.Text = currentequipmentName;
                        }
                        else if (rdoWareHouse.IsChecked == true)
                        {

                            SetWareHouse(cboIssueWareHouse, carrier);
                        }
                        else
                        {
                            SetProcEqpt(cboIssueProcEqpt, carrier);
                        }



                    }
                }
                else
                {
                    if (query.Count() <= 1)
                    {
                        SelectPortInfo(string.Empty);
                        txtEquipmentName.Text = string.Empty;
                        if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                        }
                        if (cboIssueWareHouse.SelectedItem != null && cboIssueWareHouse.Items.Count > 0)
                        {
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                        }
                        if (cboIssueProcEqpt.SelectedItem != null && cboIssueProcEqpt.Items.Count > 0)
                        {
                            SetProcEqpt(cboIssueProcEqpt, string.Empty);
                        }
                    }
                }
            }
        }

        #endregion

        #region LOTID 텍스트 박스 이벤트 : txtLotId_PreviewKeyDown()
        /// <summary>
        /// 랏 아이디 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {
                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            for (int j = 0; j < dgIssueTargetInfo.Rows.Count; j++)
                            {
                                if (sPasteStrings[i].ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[j].DataItem, "LOTID")).GetString())
                                {
                                    DataTableConverter.SetValue(dgIssueTargetInfo.Rows[j].DataItem, "CHK", 1);

                                    dgIssueTargetInfo.EndEdit();
                                    dgIssueTargetInfo.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "SKID_ID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else if (rdoWareHouse.IsChecked == true)
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }
                            else
                            {
                                SetProcEqpt(cboIssueProcEqpt, string.Empty);
                            }

                        }

                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }

        }

        #endregion

        #region FastTrack 버튼 이벤트 : btnFastTrack_Click()
        /// <summary>
        /// 20210903 오화백 PastTrack 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFastTrack_Click(object sender, RoutedEventArgs e)
        {

            foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    _FastTrackLot = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    _FastFlag = DataTableConverter.GetValue(row.DataItem, "FAST_TRACK_FLAG").GetString();
                }
            }
            if (!ValidationFastTrack()) return;

            if (_FastFlag == "Y")
            {
                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7355", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(false);
                    }
                });
            }
            else
            {

                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7354", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(true);
                    }
                });
            }

        }



        #endregion

        #region 반송 목적지 Radio 버튼 이벤트 : rdoType_Checked()
        private void rdoType_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            string selectedequipmentCode = string.Empty;
            string carrier = string.Empty;
            switch (radioButton.Name)
            {

                case "rdoPort":
                    cboIssuePort.Visibility = Visibility.Visible;
                    cboIssueWareHouse.Visibility = Visibility.Collapsed;
                    cboIssueProcEqpt.Visibility = Visibility.Collapsed;
                    btnManualIssue.Content = ObjectDic.Instance.GetObjectName("수동출고");
                    btnTransferCancel.Content = ObjectDic.Instance.GetObjectName("출고취소");
                    dgIssueTargetInfo.Columns["ISS_RSV_FLAG"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["ISS_RSV_PRIORITY_NO"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["PORT_NAME"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Visibility = Visibility.Visible;
                    btnChangePriorityNo.Visibility = Visibility.Collapsed;
                    int lastIdx = -1;
                    if (rdoCarrier.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");
                    }
                    else if (rdoEmptyCarrier.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByEmptyCarrier, "CHK");
                    }
                    else if (rdoAbNormalCarrier.IsChecked == true)
                    {
                        lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByAbNormalCarrier, "CHK");
                    }

                    if (lastIdx >= 0)
                    {

                        if (rdoCarrier.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                        }
                        else if (rdoEmptyCarrier.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx].DataItem, "EQPTNAME").GetString();

                        }
                        else if (rdoAbNormalCarrier.IsChecked == true)
                        {
                            selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx].DataItem, "EQPTNAME").GetString();

                        }

                        SetIssuePort(cboIssuePort, selectedequipmentCode);
                        SelectPortInfo(selectedequipmentCode);
                    }
                    else
                    {
                        SetIssuePort(cboIssuePort, selectedequipmentCode);
                        SelectPortInfo(selectedequipmentCode);
                    }

                    break;
                case "rdoWareHouse":
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    cboIssueWareHouse.Visibility = Visibility.Visible;
                    cboIssueProcEqpt.Visibility = Visibility.Collapsed;
                    btnManualIssue.Content = ObjectDic.Instance.GetObjectName("수동출고");
                    btnTransferCancel.Content = ObjectDic.Instance.GetObjectName("출고취소");
                    Util.gridClear(dgPortInfo);
                    txtEquipmentName.Text = string.Empty;
                    dgIssueTargetInfo.Columns["ISS_RSV_FLAG"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["ISS_RSV_PRIORITY_NO"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["PORT_NAME"].Visibility = Visibility.Collapsed;
                    dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Visibility = Visibility.Visible;
                    btnChangePriorityNo.Visibility = Visibility.Collapsed;
                    int lastIdx1 = -1;

                    if (rdoCarrier.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");
                    }
                    else if (rdoEmptyCarrier.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByEmptyCarrier, "CHK");
                    }
                    else if (rdoAbNormalCarrier.IsChecked == true)
                    {
                        lastIdx1 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfoByAbNormalCarrier, "CHK");
                    }



                    if (lastIdx1 >= 0)
                    {
                        if (rdoCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx1].DataItem, "SKID_ID").GetString();

                        }
                        else if (rdoEmptyCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfoByEmptyCarrier.Rows[lastIdx1].DataItem, "SKID_ID").GetString();

                        }
                        else if (rdoAbNormalCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfoByAbNormalCarrier.Rows[lastIdx1].DataItem, "CSTID").GetString();

                        }
                        SetWareHouse(cboIssueWareHouse, carrier);
                    }
                    else
                    {
                        SetWareHouse(cboIssueWareHouse, carrier);
                    }

                    break;

                case "rdoProcEqpt":
                    cboIssuePort.Visibility = Visibility.Collapsed;
                    cboIssueWareHouse.Visibility = Visibility.Collapsed;
                    cboIssueProcEqpt.Visibility = Visibility.Visible;
                    btnManualIssue.Content = ObjectDic.Instance.GetObjectName("공정반송예약");
                    btnTransferCancel.Content = ObjectDic.Instance.GetObjectName("공정반송예약취소");
                    Util.gridClear(dgPortInfo);
                    dgIssueTargetInfo.Columns["ISS_RSV_FLAG"].Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Columns["ISS_RSV_PRIORITY_NO"].Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Columns["PORT_NAME"].Visibility = Visibility.Visible;
                    dgIssueTargetInfo.Columns["CMD_STAT_NAME"].Visibility = Visibility.Collapsed;
                    btnChangePriorityNo.Visibility = Visibility.Visible;
                    txtEquipmentName.Text = string.Empty;
                    int lastIdx2 = -1;

                    if (rdoCarrier.IsChecked == true)
                    {
                        for (int i = 0; i < dgIssueTargetInfo.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "RACK_STAT_CODE")) == "DISABLE")
                            {
                                DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 0);
                            }
                        }
                        lastIdx2 = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");
                    }

                    if (lastIdx2 >= 0)
                    {

                        //string carrier = string.Empty;
                        if (rdoCarrier.IsChecked == true)
                        {
                            carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx2].DataItem, "SKID_ID").GetString();

                        }
                        SetProcEqpt(cboIssueProcEqpt, carrier);
                    }
                    else
                    {
                        SetProcEqpt(cboIssueProcEqpt, carrier);
                    }
                    break;

            }

        }

        #endregion


        #region Method

        #region FIFO 출고, 조건출고  집계 및 리스트 조회 : SelectManualOutInventory(), SelectManualOutInventoryList()

        /// <summary>
        /// FIFO 출고, 조건출고 집계 조회
        /// </summary>
        private void SelectManualOutInventory()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));
              
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["FIFO"] = _selectedRadioButtonValue;
        
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    decimal LotTotalCount = 0;
                    decimal LotCount = 0;
                    decimal LotHoldCount = 0;
                    decimal LotHoldQMSCount = 0;
                    decimal LotHoldQMSCount_ETC = 0;
                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            LotTotalCount = LotTotalCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_TOTAL_QTY"]);
                            LotCount = LotCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_QTY"]);
                            LotHoldCount = LotHoldCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_HOLD_QTY"]);
                            LotHoldQMSCount = LotHoldQMSCount + Convert.ToDecimal(bizResult.Rows[i]["LOT_HOLD_QMS_QTY"]);
                            LotHoldQMSCount_ETC = LotHoldQMSCount_ETC + Convert.ToDecimal(bizResult.Rows[i]["LOT_HOLD_QMS_QTY_ETC"]);
                        }
                        DataRow newRow = bizResult.NewRow();
                        newRow["PRJT_NAME"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["PROD_VER_CODE"] = string.Empty;
                        newRow["ELTR_TYPE_CODE_LOT"] = string.Empty;
                        newRow["ELTR_TYPE_NAME"] = string.Empty;
                        newRow["EQPTID"] = string.Empty;
                        newRow["EQPTNAME"] = string.Empty;
                        newRow["PRODID"] = string.Empty;
                        newRow["HALF_ROLL"] = string.Empty;
                        newRow["HALF_SLIT_SIDE"] = string.Empty;
                        newRow["EM_SECTION_ROLL_DIRCTN"] = string.Empty;
                        newRow["LOT_TOTAL_QTY"] = LotTotalCount;
                        newRow["LOT_QTY"] = LotCount;
                        newRow["LOT_HOLD_QTY"] = LotHoldCount;
                        newRow["LOT_HOLD_QMS_QTY"] = LotHoldQMSCount;
                        newRow["LOT_HOLD_QMS_QTY_ETC"] = LotHoldQMSCount_ETC;

                        bizResult.Rows.Add(newRow);

                    }


                    Util.GridSetData(dgStore, bizResult, null, true);



                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        /// <summary>
        ///  FIFO 출고, 조건출고 리스트 조회
        /// </summary>
        /// <param name="isRefresh"></param>
        private void SelectManualOutInventoryList(bool isRefresh = false)
        {
            Util.gridClear(dgPortInfo);          
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("PAST_DAY", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE_LOT", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("QA_INSP_JUDG_VALUE", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG_ETC", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FIFO"] = _selectedRadioButtonValue;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _equipmentCode;
                dr["PROD_VER_CODE"] = _productVersion;
                dr["PRODID"] = _productCode;
                dr["PRJT_NAME"] = _projectName;
                dr["WIPHOLD"] = _selectedWipHold;
                dr["LOTID"] = _lotId;
                dr["HALF_SLIT_SIDE"] = _halfSlitterSideCode;
                dr["EM_SECTION_ROLL_DIRCTN"] = _em_section_roll_dirctn;
                dr["HOLD_CODE"] = _holdCode;
                dr["PAST_DAY"] = _pastDay;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["ELTR_TYPE_CODE_LOT"] = _eltr_type_code_lot;
                dr["QMS_HOLD_FLAG"] = _selectedQmsHold;
                dr["QA_INSP_JUDG_VALUE"] = _faultyType;
                dr["QMS_HOLD_FLAG_ETC"] = _selectedQmsHold_ETC;
                dr["CSTID"] = txtCarrierId.Text;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataView dv = bizResult.AsDataView();
                    dv.Sort = "ISS_RSV_TRGT_PORT_ID,ISS_RSV_PRIORITY_NO";
                    bizResult = dv.ToTable();

                    if (dgIssueTargetInfo.IsVisible == true)
                    {
                        Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
                    }
                    else
                    {

                        if (bizResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < bizResult.Rows.Count; i++)
                            {
                                string _SkidID = string.Empty;
                                string _Indate = string.Empty;
                                string _Rownum = string.Empty;

                                if (bizResult.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[i]["BOBBIN_ID"].ToString())
                                {
                                    _SkidID = bizResult.Rows[i]["SKID_ID"].ToString();
                                    _Indate = bizResult.Rows[i]["CSTINDTTM"].ToString();
                                    _Rownum = bizResult.Rows[i]["ROW_NUM"].ToString();

                                    for (int j = 0; j < bizResult.Rows.Count; j++)
                                    {
                                        if (bizResult.Rows[j]["SKID_ID"].ToString() == _SkidID)
                                        {
                                            bizResult.Rows[j]["CSTINDTTM"] = Convert.ToDateTime(_Indate);
                                            bizResult.Rows[j]["ROW_NUM"] = _Rownum;
                                        }


                                    }
                                }
                            }

                        }

                        Util.GridSetData(dgIssueTargetInfo_SWW, bizResult, null, true);
                    }



                    if (!isRefresh)
                    {
                        if (CommonVerify.HasTableRow(bizResult) && string.Equals(_selectedRadioButtonValue, "NORMAL"))
                        {
                            rowCount_ValueChanged(rowCount, null);
                        }
                        else
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                            SetProcEqpt(cboIssueProcEqpt, string.Empty);
                        }
                    }

                    if (dgIssueTargetInfo.IsVisible == true)
                    {
                        Util.GridSetData(dgIssueTargetInfo, bizResult, null, true);
                    }
                    else
                    {
                        if (bizResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < bizResult.Rows.Count; i++)
                            {
                                string _SkidID = string.Empty;
                                string _Indate = string.Empty;
                                string _Rownum = string.Empty;
                                if (bizResult.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[i]["BOBBIN_ID"].ToString())
                                {
                                    _SkidID = bizResult.Rows[i]["SKID_ID"].ToString();
                                    _Indate = bizResult.Rows[i]["CSTINDTTM"].ToString();
                                    _Rownum = bizResult.Rows[i]["ROW_NUM"].ToString();

                                    for (int j = 0; j < bizResult.Rows.Count; j++)
                                    {
                                        if (bizResult.Rows[j]["SKID_ID"].ToString() == _SkidID)
                                        {
                                            bizResult.Rows[j]["CSTINDTTM"] = Convert.ToDateTime(_Indate);
                                            bizResult.Rows[j]["ROW_NUM"] = _Rownum;
                                        }


                                    }
                                }
                            }

                        }
                        Util.GridSetData(dgIssueTargetInfo_SWW, bizResult, null, true);
                    }



                    if (!isRefresh)
                    {
                        if (CommonVerify.HasTableRow(bizResult) && string.Equals(_selectedRadioButtonValue, "NORMAL"))
                        {
                            rowCount_ValueChanged(rowCount, null);
                        }
                        else
                        {
                            SetIssuePort(cboIssuePort, string.Empty);
                            SetWareHouse(cboIssueWareHouse, string.Empty);
                            SetProcEqpt(cboIssueProcEqpt, string.Empty);
                        }
                    }
                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {
                        dgIssueTargetInfo_SWW.MergingCells -= dgIssueTargetInfo_SWW_MergingCells;
                        dgIssueTargetInfo_SWW.MergingCells += dgIssueTargetInfo_SWW_MergingCells;
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region 공캐리어 집계 및 리스트 조회 : SelectWareHouseEmptyCarrier(), SelectWareHouseEmptyCarrierList()

        /// <summary>
        /// 공캐리어 집계 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        ElectrodeTypeCode = string.Empty,
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        CarrierProductCode = string.Empty,
                        BobinCount = g.Sum(x => x.Field<double>("BBN_QTY")), // 2024.10.24. 김영국 - DB Data Type 형식 불일치로 인한 로직 수정. (int32 -> long)
                        EmptyBobinYn = string.Empty,
                        RepCstCount = g.Sum(x => x.Field<double>("REP_CST_QTY")), // 2024.10.24. 김영국 - DB Data Type 형식 불일치로 인한 로직 수정. (int32 -> long)
                        Count = g.Count()
                        
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        // 스태킹 완성 창고일 경우
                        if (cboStockerType.SelectedValue.ToString() == "SWW" || cboStockerType.SelectedValue.ToString() == "JTW")
                        {
                            newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                            newRow["ELTR_TYPE_NAME"] = query.ElectrodeTypeCode;
                            newRow["CSTPROD"] = query.ElectrodeTypeName;
                            newRow["BBN_QTY"] = query.BobinCount;
                            newRow["EMPTY_BBN_YN"] = query.EmptyBobinYn;
                            newRow["REP_CST_QTY"] = query.RepCstCount;
                        }
                        
                        else
                        {
                            newRow["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                            newRow["ELTR_TYPE_NAME"] = query.ElectrodeTypeName;
                            newRow["CSTPROD"] = query.CarrierProductCode;
                            newRow["BBN_QTY"] = query.BobinCount;
                            newRow["EMPTY_BBN_YN"] = query.EmptyBobinYn;
                        }

                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByEmptyCarrier, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 공캐리어 리스트 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrierList()
        {

            Util.gridClear(dgPortInfo);
            string carrierState;

            if (!string.IsNullOrEmpty(_emptybobbinState))
            {
                if (_Skid_Use_Chk == "Y")
                {
                    if (_emptybobbinState == "Y")
                        carrierState = "E";
                    else
                        carrierState = "U";
                }
                else
                {
                    if (_emptybobbinState == "Y")
                        carrierState = "U";
                    else
                        carrierState = "E";
                }


            }
            else
            {
                carrierState = null;
            }


            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;
                dr["EQPTID"] = _selectedRadioButtonValue.Equals("EMPTYCARRIER") ? cboStocker.SelectedValue : _equipmentCode;
                dr["CSTPROD"] = _skidType;
                dr["CSTSTAT"] = carrierState; //string.Equals(_emptybobbinState, "Y") ? "U" : "E";


                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);

                    SetIssuePort(cboIssuePort, string.Empty);
                    SetWareHouse(cboIssueWareHouse, string.Empty);
                    SetProcEqpt(cboIssueProcEqpt, string.Empty);

                    //if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                    //{
                    //    bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                    //    bizResult.Columns.Add("CARRIERID", typeof(string));

                    //}

                    //if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    //{
                    //    foreach (DataRow row in bizResult.Rows)
                    //    {
                    //        //창고유형 = “JRW” 이면 BOBBIN_ID (MES) = CARRIERID (MCS)
                    //        if (cboStockerType?.SelectedValue.GetString() == "JWR")
                    //        {
                    //            var query = (from t in _requestTransferInfoTable.AsEnumerable()
                    //                         where t.Field<string>("CARRIERID") == row["BOBBIN_ID"].GetString()
                    //                         select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                    //            if (query != null)
                    //            {
                    //                row["REQ_TRF_STAT"] = query.RequestTransferState;
                    //                row["CARRIERID"] = query.CarrierId;
                    //                row["REQ_TRFID"] = query.RequestTransferId;
                    //            }
                    //        }
                    //        else //창고유형 != “JRW” 이면SKID_ID (MES) = CARRIERID (MCS) 
                    //        {
                    //            var query = (from t in _requestTransferInfoTable.AsEnumerable()
                    //                         where t.Field<string>("CARRIERID") == row["SKID_ID"].GetString()
                    //                         select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                    //            if (query != null)
                    //            {
                    //                row["REQ_TRF_STAT"] = query.RequestTransferState;
                    //                row["CARRIERID"] = query.CarrierId;
                    //                row["REQ_TRFID"] = query.RequestTransferId;
                    //            }
                    //        }
                    //    }
                    //    bizResult.AcceptChanges();
                    //}

                    Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);
                    SetIssuePort(cboIssuePort, string.Empty);
                    SetWareHouse(cboIssueWareHouse, string.Empty);
                    SetProcEqpt(cboIssueProcEqpt, string.Empty);
                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {
                        dgIssueTargetInfoByEmptyCarrier.MergingCells -= dgIssueTargetInfoByEmptyCarrier_MergingCells;
                        dgIssueTargetInfoByEmptyCarrier.MergingCells += dgIssueTargetInfoByEmptyCarrier_MergingCells;
                    }
                    else if (cboStockerType.SelectedValue.ToString() == "JTW")
                    {
                        dgIssueTargetInfoByEmptyCarrier.MergingCells -= dgIssueTargetInfoByEmptyCarrierJTW_MergingCells;
                        dgIssueTargetInfoByEmptyCarrier.MergingCells += dgIssueTargetInfoByEmptyCarrierJTW_MergingCells;
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

        #region 비정상 Rack 집계 및 리스트 조회 : SelectWareHouseNoReadCarrier(),   SelectWareHouseNoReadCarrierList()

        /// <summary>
        /// 비정상 Rack 집계 조회
        /// </summary>
        private void SelectWareHouseNoReadCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_ABNORM_RACK";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EquipmentCode = string.Empty,
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        //SkidCount = g.Sum(x => x.Field<Int32>("CST_QTY")),
                        SkidCount = g.Sum(x => x.Field<long>("CST_QTY")), // 2024.10.25. 김영국 - DB Type이 맞지않아 Type변경. Int32 -> Long
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPTNAME"] = query.EquipmentName;
                        newRow["CST_QTY"] = query.SkidCount;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByNoReadCarrier, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 비정상 Rack 리스트 조회
        /// </summary>
        private void SelectWareHouseNoReadCarrierList()
        {
            ShowLoadingIndicator();
            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_RACK";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("CST_TYPE_CODE", typeof(string));


            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = cboStockerType.SelectedValue;
            dr["EQPTID"] = _equipmentCode;
            dr["CST_TYPE_CODE"] = _cstTypeCode;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();

                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                Util.GridSetData(dgIssueTargetInfoByNoReadCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);

            });

        }


        #endregion

        #region 정보 불일치 집계 및 리스트 조회 : SelectWareHouseAbNormalCarrier(), SelectWareHouseAbNormalCarrierList()

        /// <summary>
        /// 정보불일치 집계 조회
        /// </summary>
        private void SelectWareHouseAbNormalCarrier()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_ABNORM_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EquipmentCode = string.Empty,
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        //SkidCount = g.Sum(x => x.Field<Int32>("CST_QTY")),
                        SkidCount = g.Sum(x => x.Field<double>("CST_QTY")), // 2024.10.25. 김영국 - DB Type이 맞지않아 Type변경. Int32 -> Long
                        Count = g.Count()
                    }).FirstOrDefault();


                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = query.EquipmentCode;
                        newRow["EQPTNAME"] = query.EquipmentName;
                        newRow["CST_QTY"] = query.SkidCount;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByAbNormalCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }



        /// <summary>
        /// 정보 불일치 리스트 조회
        /// </summary>
        private void SelectWareHouseAbNormalCarrierList()
        {
            ShowLoadingIndicator();

            Util.gridClear(dgPortInfo);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_CST";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = cboStockerType.SelectedValue;
            dr["EQPTID"] = _equipmentCode;
            dr["ABNORM_TRF_RSN_CODE"] = _abNormalReasonCode;

            // 2023.07.31 강성묵 공Carrier, 정보불일치 합계 클릭시 ELTR_TYPE_NAME 적용
            dr["ELTR_TYPE_CODE"] = _electrodeTypeCode;

            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();

                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);
                SetProcEqpt(cboIssueProcEqpt, string.Empty);

                if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                {
                    bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                    bizResult.Columns.Add("CARRIERID", typeof(string));
                    bizResult.Columns.Add("REQ_TRFID", typeof(string));
                }

                if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                {
                    foreach (DataRow row in bizResult.Rows)
                    {

                        var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                     where t.Field<string>("CARRIERID") == row["CARRIERID"].GetString()
                                     select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                        if (query != null)
                        {
                            row["REQ_TRF_STAT"] = query.RequestTransferState;
                            row["CARRIERID"] = query.CarrierId;
                            row["REQ_TRFID"] = query.RequestTransferId;
                        }
                    }
                    bizResult.AcceptChanges();
                }

                Util.GridSetData(dgIssueTargetInfoByAbNormalCarrier, bizResult, null, true);
                SetIssuePort(cboIssuePort, string.Empty);
                SetWareHouse(cboIssueWareHouse, string.Empty);
                SetProcEqpt(cboIssueProcEqpt, string.Empty);
                if (cboStockerType.SelectedValue.ToString() == "SWW")
                {
                    dgIssueTargetInfoByAbNormalCarrier.MergingCells -= dgIssueTargetInfoByAbNormalCarrier_MergingCells;
                    dgIssueTargetInfoByAbNormalCarrier.MergingCells += dgIssueTargetInfoByAbNormalCarrier_MergingCells;
                }


            });
        }
        #endregion

        #region 출고 수량 셋팅 : SelectReleaseCount()
        /// <summary>
        /// 출고 수량 셋팅  공통코드 관리
        /// </summary>
        private void SelectReleaseCount()
        {
            const string bizRuleName = "DA_MCS_SEL_COMMCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CMCDTYPE"] = "CWA_JRW_FIFO_DEFAULT_CNT";
            dr["CMCODE"] = "DEFAULT_VAL";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ATTRIBUTE1"].GetInt() > 3 || dtResult.Rows[0]["ATTRIBUTE1"].GetInt() < 1)
                {
                    _maxcheckCount = 1;
                    rowCount.Maximum = 1;
                    rowCount.Value = 1;
                }
                else
                {
                    _maxcheckCount = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                    rowCount.Maximum = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                    rowCount.Value = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                }
            }
        }
        #endregion

        #region PORT 리스트 조회  : SelectPortInfo()
        private void SelectPortInfo(string equipmentCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgPortInfo, result, null, true);
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
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 수동 출고 처리 : SaveManualIssueByEsnb()
        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI_2";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("SRC_PORTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                C1DataGrid dg;
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                    _Carrierid = "SKID_ID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    dg = dgIssueTargetInfoByAbNormalCarrier;
                    _Carrierid = "CARRIERID";
                }
                else
                {
                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {
                        dg = dgIssueTargetInfo_SWW;
                        _Carrierid = "SKID_ID";
                    }
                    else
                    {
                        dg = dgIssueTargetInfo;
                        _Carrierid = "SKID_ID";
                    }

                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_EQPTID").GetString();
                        newRow["SRC_PORTID"] = DataTableConverter.GetValue(row.DataItem, "SRC_PORTID").GetString();

                        //if (rdoPort.IsChecked == true)
                        //{
                        //    newRow["DST_EQPTID"] = _dst_eqptID;
                        //    newRow["DST_PORTID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                        //}
                        //else
                        //{
                        //    newRow["DST_EQPTID"] = (cboIssueWareHouse.SelectedItem as C1ComboBoxItem).Name.GetString();
                        //    newRow["DST_PORTID"] = _dst_portID;
                        //}
                        if (rdoPort.IsChecked == true)
                        {
                            newRow["DST_EQPTID"] = ((ContentControl)(cboIssuePort.Items[cboIssuePort.SelectedIndex])).DataContext.GetString();
                            newRow["DST_PORTID"] = (cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                        }
                        else
                        {
                            newRow["DST_EQPTID"] = (cboIssueWareHouse.SelectedItem as C1ComboBoxItem).Name.GetString();
                            newRow["DST_PORTID"] = ((ContentControl)(cboIssueWareHouse.Items[cboIssueWareHouse.SelectedIndex])).DataContext.GetString();
                        }
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {

                            SelectWareHouseEmptyCarrierList();
                        }
                        else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                        {
                            SelectWareHouseAbNormalCarrierList();
                        }
                        else
                        {
                            SelectManualOutInventoryList(true);
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
                Util.MessageException(ex);
            }
        }

        #endregion


        #region 공정설비반송예약 : SaveProcManuallssue()
        private void SaveProcManuallssue()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_CST_RSV_BY_UI";

                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_PORTID", typeof(string));
                inTable.Columns.Add("PRIORITY_NO", typeof(Decimal));
                inTable.Columns.Add("UPDUSER", typeof(string));


                C1DataGrid dg;
                string _Carrierid = string.Empty;

                dg = dgIssueTargetInfo;
                _Carrierid = "SKID_ID";

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                        newRow["DST_EQPTID"] = (cboIssueProcEqpt.SelectedItem as C1ComboBoxItem).Name.GetString();
                        newRow["DST_PORTID"] = ((ContentControl)(cboIssueProcEqpt.Items[cboIssueProcEqpt.SelectedIndex])).DataContext.GetString();
                        newRow["PRIORITY_NO"] = 1;
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        SelectManualOutInventoryList(true);
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
                Util.MessageException(ex);
            }
        }

        #endregion



        #region 사용자 권한 조회 : IsAdminAuthorityByUserId()
        /// <summary>
        /// 사용자 권한
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private bool IsAdminAuthorityByUserId(string userId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["USERID"] = userId;
                dr["AUTHID"] = "MESADMIN,MESDEV,LOGIS_MANA";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", inTable);

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

        #endregion

        #region FastTrack 적용 공장 체크 : ChkFastTrackOWNER()
        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        #endregion

        #region  초기화 : ClearControl()
        private void ClearControl()
        {
            _projectName = string.Empty;
            _productVersion = string.Empty;
            _halfSlitterSideCode = string.Empty;
            _productCode = string.Empty;
            _holdCode = string.Empty;
            _pastDay = string.Empty;
            _lotId = string.Empty;
            _faultyType = string.Empty;
            _skidType = string.Empty;
            _emptybobbinState = string.Empty;
            _equipmentCode = string.Empty;
            _electrodeTypeCode = string.Empty;
            _cstTypeCode = string.Empty;
            _abNormalReasonCode = string.Empty;
            txtEquipmentName.Text = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedQmsHold_ETC = string.Empty;
            _FastTrackLot = string.Empty;
            _FastFlag = string.Empty;

            _eltr_type_code_lot = string.Empty;
            _em_section_roll_dirctn = string.Empty;
            Util.gridClear(dgStore);
            Util.gridClear(dgStoreByEmptyCarrier);
            Util.gridClear(dgStoreByNoReadCarrier);
            Util.gridClear(dgStoreByAbNormalCarrier);
            Util.gridClear(dgIssueTargetInfo);
            Util.gridClear(dgIssueTargetInfo_SWW);
            Util.gridClear(dgIssueTargetInfoByEmptyCarrier);
            Util.gridClear(dgIssueTargetInfoByNoReadCarrier);
            Util.gridClear(dgIssueTargetInfoByAbNormalCarrier);
            Util.gridClear(dgPortInfo);

            _requestTransferInfoTable.Clear();

            if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
            {
                SetIssuePort(cboIssuePort, string.Empty);
            }
            if (cboIssueWareHouse.SelectedItem != null && cboIssueWareHouse.Items.Count > 0)
            {
                SetWareHouse(cboIssueWareHouse, string.Empty);
            }
            if (cboIssueProcEqpt.SelectedItem != null && cboIssueProcEqpt.Items.Count > 0)
            {
                SetProcEqpt(cboIssueProcEqpt, string.Empty);
            }
        }

        #endregion

        #region FIFO 출고 선택시 출고수량에 따라 선택 :  SetCheckedIssueTargetInfoGrid()
        private void SetCheckedIssueTargetInfoGrid()
        {
            try
            {
                if (Math.Abs(rowCount.Value) >= 0)
                {
                    int selectedCheckCount = (int)rowCount.Value;
                    int i = 0;

                    if (CommonVerify.HasDataGridRow(dgIssueTargetInfo))
                    {
                        if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") >= rowCount.Value) return;

                        foreach (C1.WPF.DataGrid.DataGridRow row in dgIssueTargetInfo.Rows)
                        {
                            if (row.Type == DataGridRowType.Item)
                            {
                                if (i < selectedCheckCount)
                                {
                                    if (!DataTableConverter.GetValue(row.DataItem, "RACK_STAT_CODE").Equals("DISABLE"))
                                    {
                                        DataTableConverter.SetValue(row.DataItem, "CHK", 1);
                                        i++;
                                        dgIssueTargetInfo.EndEdit();
                                        dgIssueTargetInfo.EndEditRow(true);
                                    }
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "SKID_ID").GetString();

                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else if (rdoWareHouse.IsChecked == true)
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }
                            else
                            {
                                SetProcEqpt(cboIssueProcEqpt, carrier);
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

        #endregion

        #region 수동출고 Validation  : ValidationManualIssue(), ValidationTransferCancelByEsnb()
        /// <summary>
        /// 기본 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationManualIssue()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
            }
            else
            {
                if (cboStockerType.SelectedValue.ToString() == "SWW")
                {
                    dg = dgIssueTargetInfo_SWW;

                }
                else
                {
                    dg = dgIssueTargetInfo;
                }

            }

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (rdoPort.IsChecked == true)
            {
                if (cboIssuePort.SelectedItem == null || string.IsNullOrEmpty((cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
                {
                    Util.MessageValidation("MCS1004");
                    return false;
                }

                if ((cboIssuePort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
                {
                    Util.MessageInfo("SFU8137");
                    return false;
                }
            }
            else if (rdoWareHouse.IsChecked == true)
            {
                if (cboIssueWareHouse.SelectedItem == null || string.IsNullOrEmpty((cboIssueWareHouse.SelectedItem as C1ComboBoxItem).Tag.GetString()))
                {
                    Util.MessageValidation("MCS1004");
                    return false;
                }
            }
            else
            {
                if (cboIssueProcEqpt.SelectedItem == null || string.IsNullOrEmpty((cboIssueProcEqpt.SelectedItem as C1ComboBoxItem).Tag.GetString()))
                {
                    Util.MessageValidation("SFU7024");
                    return false;
                }
            }


            return true;
        }


        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
                string _Carrierid = string.Empty;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    _Carrierid = "SKID_ID";
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    _Carrierid = "CARRIERID";
                }
                else
                {
                    _Carrierid = "SKID_ID";
                }
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, _Carrierid).GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_CHK_SEL_TRF_CMD_CANCEL_BY_UI", "IN_DATA", "OUT_DATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["RETVAL"].GetString() != "0")
                    {
                        return false;
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


        #endregion

        #region 수동출고 취소 Validation : ValidationTransferCancel()
        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
            {
                dg = dgIssueTargetInfoByAbNormalCarrier;
            }
            else
            {
                if (cboStockerType.SelectedValue.ToString() == "SWW")
                {
                    dg = dgIssueTargetInfo_SWW;

                }
                else
                {
                    dg = dgIssueTargetInfo;
                }
            }

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }
            if (rdoProcEqpt.IsChecked == false)
            {
                if (!ValidationTransferCancelByEsnb(dg))
                {
                    return false;
                }
            }


            return true;


        }

        #endregion


        #region 예약순서변경 Validation : ValidationReservChange()
        private bool ValidationReservChange()
        {
            if (!CommonVerify.HasDataGridRow(dgIssueTargetInfo))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            int rowCount;
            rowCount = (from C1.WPF.DataGrid.DataGridRow rows in dgIssueTargetInfo.Rows
                        where rows.DataItem != null
                              && rows.Visibility == Visibility.Visible
                              && rows.Type == DataGridRowType.Item
                              && DataTableConverter.GetValue(rows.DataItem, "ISS_RSV_FLAG").GetString() == "Y"
                        select rows).Count();

            if (rowCount < 1)
            {
                //반송예약정보가 없습니다.
                Util.MessageValidation("SFU8898");
                return false;
            }
            if (cboIssueProcEqpt.SelectedItem == null || string.IsNullOrEmpty((cboIssueProcEqpt.SelectedItem as C1ComboBoxItem).Tag.GetString()))
            {
                Util.MessageValidation("SFU7024");
                return false;
            }
            return true;
        }

        #endregion


        #region 창고유형 콤보 조회 : SetStockerTypeCombo()
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            string attribute1, attribute2;

            if (_selectedRadioButtonValue == "NORMAL" || _selectedRadioButtonValue == "NOREADCARRIER")
            {
                attribute1 = "Y";
                attribute2 = null;
            }
            else
            {
                attribute1 = null;
                attribute2 = "Y";
            }

            const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, attribute1, attribute2, "AREA_EQUIPMENT_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region 창고 콤보 조회 : SetStockerCombo()
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "DA_MHS_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region 극성 콤보 조회 : SetElectrodeTypeCombo()
        private static void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region HOLD 사유 콤보 조회 : SetHoldCombo()
        private static void SetHoldCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ACTIVITIREASON_CBO";
            string[] arrColumn = { "LANGID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, "HOLD_LOT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region QA 불량 유형 콤보 조회 : SetFalutyTypeCombo()
        private static void SetFalutyTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "VD_RESN_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);


        }

        #endregion

        #region 출고 PORT 콤보 조회 : SetIssuePort()
        private void SetIssuePort(C1ComboBox cbo, string equipmentCode)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;

                if (cbo.Items.Count > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);
                //cbo.ItemsSource = dtResult.AsEnumerable();
                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["PORT_ID"].GetString();
                    comboBoxItem.Name = row["TRF_STAT_CODE"].GetString();

                    comboBoxItem.DataContext = row["DST_EQPTID"].GetString();



                    if (row["TRF_STAT_CODE"].GetString() == "OUT_OF_SERVICE")
                    {
                        comboBoxItem.Foreground = new SolidColorBrush(Colors.Red);
                        comboBoxItem.FontWeight = FontWeights.Bold;
                    }
                    cbo.Items.Add(comboBoxItem);
                }

                cboIssuePort.SelectedIndexChanged += cboIssuePort_SelectedIndexChanged;

                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 대상 창고 콤보 조회 : SetWareHouse()
        private void SetWareHouse(C1ComboBox cbo, string carrier)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;
                if (cbo.Items.Count > 0)
                {
                    cbo.ItemsSource = null;
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                    cbo.Items.Clear();
                    cbo.SelectedValue = null;
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("IS_EMPTY", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrier;
                dr["IS_EMPTY"] = _selectedRadioButtonValue == "EMPTYCARRIER" ? "Y" : "N";

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_LIST", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["DST_PORTNAME"].GetString();
                    comboBoxItem.Tag = row["DST_PORTID"].GetString();
                    comboBoxItem.Name = row["DST_EQPTID"].GetString();
                    comboBoxItem.DataContext = row["DST_PORTID"].GetString();

                    cbo.Items.Add(comboBoxItem);
                }
                cboIssuePort.SelectedIndexChanged += cboIssuePort_SelectedIndexChanged;
                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 공정설비포트 콤보 조회 : SetProcEqpt()
        private void SetProcEqpt(C1ComboBox cbo, string carrier)
        {
            try
            {
                cboIssuePort.SelectedIndexChanged -= cboIssuePort_SelectedIndexChanged;
                if (cbo.Items.Count > 0)
                {
                    cbo.ItemsSource = null;
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                    cbo.Items.Clear();
                    cbo.SelectedValue = null;
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrier;

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_PROC_PORT_LIST", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["DST_PORTID"].GetString();
                    comboBoxItem.Name = row["DST_EQPTID"].GetString();
                    comboBoxItem.DataContext = row["DST_PORTID"].GetString();

                    cbo.Items.Add(comboBoxItem);
                }
                cboIssuePort.SelectedIndexChanged += cboIssuePort_SelectedIndexChanged;
                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Timer 관련 : TimerSetting(), _dispatcherTimer_Tick()

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }


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

                    btnSearch_Click(null, null);
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }


        #endregion

        #region 프로그래스 바  : ShowLoadingIndicator()
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

        #region FastTrack 설정  : SetFastTrace()

        /// <summary>
        /// FastTrack 설정
        /// </summary>
        /// <param name="fasttrackFlag"></param>
        private void SetFastTrace(bool fasttrackFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FAST_TRACK_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LOTID"] = _FastTrackLot;
                if (fasttrackFlag == true)
                {
                    dr["FAST_TRACK_FLAG"] = "Y";
                }
                else
                {
                    dr["FAST_TRACK_FLAG"] = string.Empty;
                }
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_FAST_TRACK_LOT", "INDATA", null, dtRqst);

                if (fasttrackFlag)
                {
                    Util.MessageInfo("SFU1518");
                }
                else
                {
                    Util.MessageInfo("SFU1937");
                }

                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "NOREADCARRIER"))
                {
                    SelectWareHouseNoReadCarrier();
                    SelectWareHouseNoReadCarrierList();
                }
                else if (string.Equals(_selectedRadioButtonValue, "ABNORMALCARRIER"))
                {
                    SelectWareHouseAbNormalCarrier();
                    SelectWareHouseAbNormalCarrierList();
                }
                else
                {
                    SelectManualOutInventory();
                    SelectManualOutInventoryList(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region FastTrack Validation : ValidationFastTrack()
        private bool ValidationFastTrack()
        {

            if (!CommonVerify.HasDataGridRow(dgIssueTargetInfo))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgIssueTargetInfo, "CHK") > 1)
            {
                Util.MessageValidation("SFU4159");
                return false;
            }

            if (_FastTrackLot != string.Empty && _FastTrackLot.Substring(8, 2) != "C1")
            {
                Util.MessageValidation("SFU7356");
                return false;
            }


            return true;
        }

        #endregion

        #region 스키드 사용 체크 : Chk_Skid_Use()
        /// <summary>
        /// 스키드 사용 체크
        /// </summary>
        private void Chk_Skid_Use()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));
            dt.Columns.Add("ATTRIBUTE1", typeof(string));


            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "MHS_UNCHECK_SKID_BOBBIN_MAPPING";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dr["ATTRIBUTE1"] = "Y";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                _Skid_Use_Chk = "Y";
            }
            else
            {
                _Skid_Use_Chk = "N";

            }



        }

        #endregion

        #region 공정설비포트 사용여부 : IsTabStatusbyWorkorderVisibility()
        /// <summary>
        /// 공정설비포트 사용여부 
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        private bool IsProcEqptPortVisibility(string stockerTypeCode)
        {

            if (string.IsNullOrEmpty(stockerTypeCode)) return false;

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AREA_EQUIPMENT_GROUP_UI";
                dr["COM_CODE"] = stockerTypeCode;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["ATTR3"].GetString() == "Y")
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
        #endregion

        #region EQGR별 QMS 체크할 BLOCK_TYPE_CODE : ChkQmsBlock_Type_code()
        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkQmsBlock_Type_code(string StockerType)
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "BLOCK_TYPE_BY_EQGR";
            dr["CBO_CODE"] = StockerType;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count > 0)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        #endregion

        #region NFF
        /// <summary>
        /// NFF PCW 에는 보빈이 없다.
        /// </summary>
        private void RepLotUseForm()
        {

            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", LoginInfo.CFG_AREA_ID) && (cboStockerType.SelectedValue.GetString() == "PCW" || cboStockerType.SelectedValue.GetString() == "WWW"))  //NFF 추가
            {
                dgIssueTargetInfo.Columns["BOBBIN_ID"].Visibility = Visibility.Collapsed;   //보빈ID
                dgIssueTargetInfoByEmptyCarrier.Columns["CST_CLEAN_NAME"].Visibility = Visibility.Collapsed;     //케리어 세정 여부
                dgIssueTargetInfoByEmptyCarrier.Columns["BOBBIN_ID"].Visibility = Visibility.Collapsed;     //보빈ID


                dgIssueTargetInfo.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                dgIssueTargetInfoByEmptyCarrier.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                dgIssueTargetInfoByAbNormalCarrier.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");

            }
            else
            {
                dgIssueTargetInfo.Columns["BOBBIN_ID"].Visibility = Visibility.Visible;   //보빈ID
                dgIssueTargetInfoByEmptyCarrier.Columns["CST_CLEAN_NAME"].Visibility = Visibility.Visible;     //케리어 세정 여부
                if (cboStockerType.SelectedValue.GetString() != "JTW")
                    dgIssueTargetInfoByEmptyCarrier.Columns["BOBBIN_ID"].Visibility = Visibility.Visible;     //보빈ID     
                dgIssueTargetInfo.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("캐리어ID");
                dgIssueTargetInfoByEmptyCarrier.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("캐리어ID");
                dgIssueTargetInfoByAbNormalCarrier.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("캐리어ID");
            }
        }
        #endregion
        // [E20231023-000302] 무지부/권취방향 Validation 추가
        #region 동별 공통코드 확인 : IsAreaCommonCodeUse
        private bool IsAreaCommonCodeUse(string sComeCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                //RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                //dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion

        // [E20231023-000302] 무지부/권취방향 Validation 추가
        #region 목적 공정설비와 체크 랏의 무지부/권취방향 Validation : ChkEqptWindDirection
        private void ChkEqptWindDirection()
        {
            bool iserror = false;

            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["EQPTID"] = ((ContentControl)(cboIssueProcEqpt.Items[cboIssueProcEqpt.SelectedIndex])).Name.ToString();

            inDataTable.Rows.Add(inDataRow);

            DataTable inDataTable1 = inDataSet.Tables.Add("INLOT");
            inDataTable1.Columns.Add("LOTID", typeof(string));

            C1DataGrid dg;
            string _Lotid = string.Empty;

            dg = dgIssueTargetInfo;
            _Lotid = "LOTID";

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    DataRow newRow = inDataTable1.NewRow();
                    newRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, _Lotid).GetString();
                    inDataTable1.Rows.Add(newRow);
                }
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_EQPT_WIND_DIRECTION", "INDATA,INLOT", null, (result, ex) =>
            {
                try
                {
                    ShowLoadingIndicator();

                    if (ex != null)
                    {
                        iserror = true;
                        Util.MessageException(ex);
                        return;
                    }
                }
                catch (Exception ErrEx)
                {
                    iserror = true;
                    HiddenLoadingIndicator();
                    Util.MessageException(ErrEx);

                }
                finally
                {
                    if (!iserror)
                    {
                        SaveProcManuallssue();
                    }
                    HiddenLoadingIndicator();
                }

            }, inDataSet);
        }

        #endregion

        #endregion

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {


                        for (int i = 0; i < dgIssueTargetInfo.Rows.Count; i++)
                        {
                            if (txtLotId.Text.ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[i].DataItem, "SKID_ID")).GetString())
                            {
                                DataTableConverter.SetValue(dgIssueTargetInfo.Rows[i].DataItem, "CHK", 1);

                                dgIssueTargetInfo.EndEdit();
                                dgIssueTargetInfo.EndEditRow(true);
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "SKID_ID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else if (rdoWareHouse.IsChecked == true)
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }
                            else
                            {
                                SetProcEqpt(cboIssueProcEqpt, carrier);
                            }

                        }

                        txtLotId.Text = string.Empty;
                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        txtLotId.Text = string.Empty;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCarrierId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    if (dgIssueTargetInfo.Rows.Count > 0)
                    {
                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            for (int j = 0; j < dgIssueTargetInfo.Rows.Count; j++)
                            {
                                if (sPasteStrings[i].ToString() == Util.NVC(DataTableConverter.GetValue(dgIssueTargetInfo.Rows[j].DataItem, "SKID_ID")).GetString())
                                {
                                    DataTableConverter.SetValue(dgIssueTargetInfo.Rows[j].DataItem, "CHK", 1);

                                    dgIssueTargetInfo.EndEdit();
                                    dgIssueTargetInfo.EndEditRow(true);
                                }
                            }
                        }

                        int lastIdx = _util.GetDataGridLastRowIndexByCheck(dgIssueTargetInfo, "CHK");

                        if (lastIdx >= 0)
                        {
                            string selectedequipmentCode = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTID").GetString();
                            string carrier = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "SKID_ID").GetString();
                            if (rdoPort.IsChecked == true)
                            {
                                txtEquipmentName.Text = DataTableConverter.GetValue(dgIssueTargetInfo.Rows[lastIdx].DataItem, "EQPTNAME").GetString();
                                SetIssuePort(cboIssuePort, selectedequipmentCode);
                                SelectPortInfo(selectedequipmentCode);
                            }
                            else if (rdoWareHouse.IsChecked == true)
                            {

                                SetWareHouse(cboIssueWareHouse, carrier);
                            }
                            else
                            {
                                SetProcEqpt(cboIssueProcEqpt, string.Empty);
                            }

                        }

                    }
                    else
                    {
                        Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
    }
}