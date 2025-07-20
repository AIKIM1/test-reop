/*************************************************************************************
 Created Date : 2020.10.23
      Creator : 
   Decription : 공 Tray 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.23  DEVELOPER : Initial Created.
  2021.04.21 KDH : 위치정보 그룹화 대응
  2022.05.25 이정미 : EQP_LOC_GRP_CD 동별 공통코드로 변경
  2022.07-07 조영대 : 공 Tray 수동배출 팝업 신규 추가
  2022.07.21 최도훈 : 공 Tray 수동배출 팝업 ESWA RTD용으로 신규 추가
  2022.11.25 이정미 : 폐기 TRAY 조회 Tab 수정 - 조회 INDATA 추가
  2022.12.07 조영대 : UserControl_Loaded 이벤트 제거
  2022.12.14 형준우 : 공 Tray 수동배출 팝업 신규 추가
  2023.02.24 권혜정 : 공 Tray 상온/출하 Aging 신규 추가
  2023.05.24 권혜정 : 공 Tray 상온/출하 Aging 보관 컬럼(목적지) 추가
  2023.05.26 권혜정 : 공 Tray 상온/출하 Aging 보관 설비 ID 수정(KEY-IN 변경)
  2023.05.30 권혜정 : 공 Tray 수동배출 적재 대표 carrier만 명령 생성(2단적재 트레이인 경우 1개의 명령만 필요)
  2023.08.28 이의철 : Aging Type combo 추가
  2023.10.03 조영대 : 수동OFF, 상태변경 팝업창 변경없이 X 로 닫을시 조회안함
  2023.10.03 이정미 : 조회 Tab 위치 콤보박스 수정 및 예약 출고 예약 여부 컬럼 추가 
  2023.10.23 조영대 : 수동OFF 사용자 권한 설정, 권한 등록된 사용자만 보임(동별공통코드:FORM_EMPTY_TRAY_MANUAL_USE), 없으면 전체 보임.
  2023.11.02 조영대 : 상태 콤보박스 기본 설정(공통코드 COMBO_TRAY_STATUS - ATTRIBUTE2 에 Y 설정시 기본 적용)
  2023.11.24 조영대 : 청소 구분에 맞게 인수 수정.
  2023.12.30 손동혁 : 공 Tray 관리 복사 후 붙여넣기 Tray 다중 조회 수정 및 폐기 트레이 조회 시 날짜 검색조건 선택적 사용 추가.
  2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
  2024.05.20 지광현 : E20240226-000399 공 Tray 목록 조회 시 위치가 EMP_STOCK_nF인 경우 실제 설비명이 EMP_STOCK_nF인 데이터만 조회되도록 수정
  2024.05.21 양강주 : TRAY 자동 세척 기능을 사용하는 경우, MMD 동별 공통코드(CARRIER_CLEAN_USE_COUNT_MNGT_PROC) 규칙을 따르도록 기능 수정
                      AS-IS : Y(세척 필요), N(세척 완료) , TO-BE : Q(세척 필요), Y(세척 완료), N(초기 상태)
  2024.06.10 지광현 : E20240226-000399 수정 건 롤백 처리
  2024.06.17 지광현 : E20240226-000399 (폴란드 4동만) 공 Tray 목록 조회 시 위치가 EMP_STOCK_nF인 경우 실제 설비명이 EMP_STOCK_nF인 데이터만 조회되도록 수정
  2024.09.10 남형희 : E20240729-000157 조회조건 상태(폐기) 추가 및 정상 조회시 폐기 출력 제거
  2024.08.23 조영대 : E20240820-000578 상태 비정상(U) 추가
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sOnOff;
        private string _sTrayType;
        private string _sLocGrp;
        private string _sBldgCd;
        private string _sActYN = "N";
        private string _sEqpId; //Aging Type combo 추가
        private string _sCMCODE;
        private string _sEQPTYPE = "N";

        bool bEqpMachineUseFlag = false;

        Util _Util = new Util();
        bool bEnableSippingControlFlag = false; // 2023.11.24 출고 제어 기능 추가

        private readonly Util _util = new Util();
        private bool _useTrayCleanOp = false;   // 2024.05.21 TRAY 자동 세척 기능을 사용하는 경우 true, AS-IS 방식의 경우(MMD 등록하지 않은 경우) false

        private bool EMPTY_TRAY_EQUIP_USE_YN = false; //Aging Type combo 추가

        public string ON_OFF
        {
            set { this._sOnOff = value; }
        }

        public string TRAY_TYPE
        {
            set { this._sTrayType = value; }
        }

        public string LOC_GRP
        {
            set { this._sLocGrp = value; }
        }

        public string BLDG_CD
        {
            set { this._sBldgCd = value; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }

        public FCS001_026()
        {
            InitializeComponent();
            InitCombo();
            initDefault();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region 활성화 공정 화면 추가 제어
            {
                _useTrayCleanOp = (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") == true && _util.IsAreaCommoncodeAttrUse("CARRIER_CLEAN_USE_COUNT_MNGT_PROC", "FORM_TRAY", new string[] { "" }) == true);

                #region [ 활성화 Tray 세척 기능 사용여부에 따른 숨김 처리]
                if (_useTrayCleanOp == false)
                {
                    dgEmptyTray.Columns["CLEAN_BAS_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["USE_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["ACCU_USE_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["LAST_CLEAN_TIME"].Visibility = Visibility.Hidden;
                }
                #endregion
            }
            #endregion

            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                _sOnOff = Util.NVC(parameters[0]);
                _sTrayType = Util.NVC(parameters[1]);
                _sLocGrp = Util.NVC(parameters[2]);
                _sBldgCd = Util.NVC(parameters[3]);
                _sActYN = Util.NVC(parameters[4]);

                //Aging Type combo 추가
                EMPTY_TRAY_EQUIP_USE_YN = false;

                if (parameters.Length > 5)
                {
                    _sEqpId = Util.NVC(parameters[5]);
                    _sCMCODE = Util.NVC(parameters[6]);
                    _sEQPTYPE = Util.NVC(parameters[7]);

                    if (_sEQPTYPE.Equals("Y"))
                    {
                        EMPTY_TRAY_EQUIP_USE_YN = true;
                    }
                }

                InitCombo();
                initDefault();
                GetList();
            }
            //  SetEvent();

            Loaded -= UserControl_Loaded;
        }

        private void initDefault()
        {
            rdoOn.Checked -= rdoOn_Checked;
            rdoOff.Checked -= rdoOff_Checked;
            if (!string.IsNullOrEmpty(_sOnOff))
            {
                if (_sOnOff.Equals("ON"))
                    rdoOn.IsChecked = true;
                else if (_sOnOff.Equals("OFF"))
                    rdoOff.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(_sTrayType))
                cboTrayType.SelectedValue = _sTrayType;
            if (!string.IsNullOrEmpty(_sLocGrp))
                cboLoc.SelectedValue = _sLocGrp;

            rdoOn.Checked += rdoOn_Checked;
            rdoOff.Checked += rdoOff_Checked;
            rdoOn_Checked(null, null);
            rdoOnScrap_Checked(null, null);

            //Aging Type combo 추가
            if (!string.IsNullOrEmpty(_sCMCODE))
            {
                cboAgingType.SelectedValue = _sCMCODE;
            }

            if (!string.IsNullOrEmpty(_sEqpId))
            {
                cboEqp.SelectedValue = _sEqpId;
            }

            if (EMPTY_TRAY_EQUIP_USE_YN.Equals(true))
            {
                this.cboAgingType.Visibility = Visibility.Visible;
                this.cboEqp.Visibility = Visibility.Visible;
                this.lblEQP_FLAG.Visibility = Visibility.Visible;
                this.lblEQP.Visibility = Visibility.Visible;
            }
            else
            {
                this.cboAgingType.Visibility = Visibility.Hidden;
                this.cboEqp.Visibility = Visibility.Hidden;
                this.lblEQP_FLAG.Visibility = Visibility.Hidden;
                this.lblEQP.Visibility = Visibility.Hidden;
            }

            // 2023.11.24 출고 제어 기능 추가
            bEnableSippingControlFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_026_SHIPPING_CONTROL");
            if (bEnableSippingControlFlag == true)
            {
                this.btnShippingCtl.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnShippingCtl.Visibility = Visibility.Hidden;
            }

            //cboEqp.SelectedIndexChanged += cboEqp_SelectedIndexChanged;

            // 수동 OFF 사용권한 설정
            SetManualOffButtonVisible();

        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            #region 조회   
            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.NONE, sCase: "TRAYTYPE");

            cboState.SetCommonCode("COMBO_TRAY_STATUS", "ATTR1='1'", CommonCombo.ComboStatus.ALL, false);
            DataView dvState = cboState.ItemsSource as DataView;
            DataTable dtState = dvState.ToTable();
            DataRow drDefault = dtState.AsEnumerable().Where(w => w.Field<string>("ATTR2") == "Y").FirstOrDefault();
            if (drDefault != null)
            {
                cboState.SelectedValue = drDefault["CBO_CODE"].ToString();
            }

            //2023.10.03 위치 콤보박스 변경 START
            string[] sFilter = { "EQP_LOC_GRP_CD" };
            SetEqptLoc(cboLoc, sFilter: sFilter);
            //2023.10.03 위치 콤보박스 변경 END

            //Aging Type combo 추가
            C1ComboBox[] cboAgingTypeChild = { cboEqp };
            string[] sFilterAging = { "FORM_AGING_TYPE_CODE", "N" };
            _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilterAging, cbChild: cboAgingTypeChild);

            C1ComboBox[] cboEqpParent = { cboAgingType };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "SCEQPID", cbParent: cboEqpParent);
            #endregion

            #region 폐기 Tray 조회  
            _combo.SetCombo(cboTrayTypeScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE");
            //2021.04.21 위치정보 그룹화 대응 START
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "ETRAY_LOC");
            string[] sFilterSrp = { "EQP_LOC_GRP_CD" };
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterSrp);
            //2021.04.21 위치정보 그룹화 대응 END
            _combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilterSrp);
            #endregion

            #region 상온/출하 Aging 조회          
            _combo.SetCombo(cboTrayTypeEmpty, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE");
            string[] sFilterUse = { "USE_FLAG" };
            _combo.SetCombo(cboUseFlag, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilterUse);
            SetAreaCombo(cboArea);
            SetEqptIdName(cboEqptID);
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            SetGridCboItem(dgEmptyTrayStorage.Columns["USE_FLAG"], "USE_FLAG");
            //SetGridCboItem(dgEmptyTrayStorage.Columns["EQPTID"], "EQPTID");
            SetGridCboItem(dgEmptyTrayStorage.Columns["EQPT_GR_TYPE_CODE"], "EQPT_GR_TYPE_CODE");
            #endregion
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                //Aging Type combo 추가
                string sBiz = "";
                if (EMPTY_TRAY_EQUIP_USE_YN.Equals(true))
                {
                    if ((bool)rdoOn.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_TRAY_UI";
                    else sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY_UI";
                }
                else
                {
                    if ((bool)rdoOn.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_TRAY";
                    else sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY";
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                inDataTable.Columns.Add("TRAY_STATUS_CD", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                //inDataTable.Columns.Add("S01", typeof(string));      // Main Eqp //2021.04.21 위치정보 그룹화 대응
                inDataTable.Columns.Add("EQPTSHORTNAME", typeof(string)); // 위치 그룹 정보 //2021.04.21 위치정보 그룹화 대응
                inDataTable.Columns.Add("EQGRID", typeof(string));   // 설비군
                inDataTable.Columns.Add("S06", typeof(string));      // 설비 층 구분
                inDataTable.Columns.Add("ETC", typeof(string));    //ETC 미포함 설비

                inDataTable.Columns.Add("AREAID", typeof(string)); //Aging Type combo 추가
                inDataTable.Columns.Add("CMCODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("EQPTID_IS_NULL", typeof(string));
                inDataTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType);

                if (!string.IsNullOrEmpty(Util.GetCondition(cboState, bAllNull: true)))
                {
                    String toClean = (_useTrayCleanOp == true) ? "Q" : "Y";
                    String cleanOK = (_useTrayCleanOp == true) ? "Y,N" : "N"; //롤백

                    if (Util.GetCondition(cboState).Equals("ONCLEAN"))
                    {
                        dr["CST_CLEAN_FLAG"] = toClean;
                    }
                    else if (Util.GetCondition(cboState).Equals("NORMAL"))
                    {
                        dr["CST_CLEAN_FLAG"] = cleanOK;
                    }

                    dr["ABNORM_TRF_RSN_CODE"] = "INNERDETECT;CELLDETECT;U".IndexOf(Util.GetCondition(cboState)) >= 0 ? Util.GetCondition(cboState) : " ";
                }

                if (!string.IsNullOrEmpty(txtTrayID.Text)) dr["CSTID"] = txtTrayID.Text;

                string s = Util.GetCondition(cboLoc, bAllNull: true);
                if (string.IsNullOrEmpty(s)) { }
                /* else if (s.Equals("EMP_STOCK_1F"))
                     *
                 {
                     dr["EQGRID"] = "STO";
                     dr["S06"] = "1";
                 }
                 else if (s.Equals("EMP_STOCK_3F"))
                 {
                     dr["EQGRID"] = "STO";
                     dr["S06"] = "3";
                 }*/
                else if (s.ToUpper().Contains("STOCK") && LoginInfo.CFG_AREA_ID != "AC")
                {
                    dr["EQGRID"] = "STO";
                    string[] floor = s.Split('_');
                    dr["S06"] = floor[floor.Length - 1].Substring(0, 1);
                }
                else if (s.Equals("ETC"))
                {
                    dr["ETC"] = "Y";
                }
                else
                {
                    //dr["S01"] = s; //2021.04.21 위치정보 그룹화 대응
                    dr["EQPTSHORTNAME"] = s; //2021.04.21 위치정보 그룹화 대응
                }

                //Aging Type combo 추가          
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCODE"] = Util.GetCondition(cboAgingType, bAllNull: true);


                if (_sEQPTYPE.Equals("Y"))
                {
                    dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                }

                //2024.09.10 남형희 : E20240729-000157 조회조건 상태(폐기) 추가 및 정상 조회시 폐기 출력 제거
                if (Util.GetCondition(cboState).Equals("ONCLEAN"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "I";
                }
                else if (Util.GetCondition(cboState).Equals("NORMAL"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "I";
                }
                else if (Util.GetCondition(cboState).Equals("DISUSE"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "S";
                }

                inDataTable.Rows.Add(dr);

                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = false;

                //백그라운드 처리, 비즈 실행 후 dgEmptyTray_ExecuteDataModify, dgEmptyTray_ExecuteDataCompleted 실행.
                dgEmptyTray.ExecuteService(sBiz, "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
            }
        }

        private void GetList(string sTrayList)
        {
            try
            {
                string sBiz = "";
                if ((bool)rdoOn.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_TRAY";
                else sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CSTID"] = sTrayList;
                inDataTable.Rows.Add(dr);

                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = false;

                // 백그라운드 처리, 비즈 실행 후 dgEmptyTray_ExecuteDataModify, dgEmptyTray_ExecuteDataCompleted 실행.
                dgEmptyTray.ExecuteService(sBiz, "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
            }
        }

        private void dgEmptyTray_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtRslt = e.ResultData as DataTable;
            if (dtRslt.Rows.Count > 0) dtRslt.Columns.Add("CHK", typeof(bool));
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                dtRslt.Rows[i]["CHK"] = false;
            }
        }

        private void dgEmptyTray_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            // 조회 완료
            btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
        }

        private void GetListScrap()
        {
            string sBiz = "";
            if ((bool)rdoOnScrap.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_TRAY";
            else sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY";
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("TRAY_TYPE_CD", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("BLDG_CD", typeof(string));
            inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
            //inDataTable.Columns.Add("S01", typeof(string));      // Main Eqp //2021.04.21 위치정보 그룹화 대응
            inDataTable.Columns.Add("EQPTSHORTNAME", typeof(string)); // 위치 그룹 정보 //2021.04.21 위치정보 그룹화 대응
            inDataTable.Columns.Add("EQGRID", typeof(string));   // 설비군
            inDataTable.Columns.Add("S06", typeof(string));      // 설비 층 구분
            inDataTable.Columns.Add("ETC", typeof(string));      //ETC 미포함플래그
            inDataTable.Columns.Add("FROM_TIME", typeof(string));      //ETC 미포함플래그
            inDataTable.Columns.Add("TO_TIME", typeof(string));      //ETC 미포함플래그
            inDataTable.Columns.Add("AREAID", typeof(string)); //Aging Type combo 추가


            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TRAY_TYPE_CD"] = Util.GetCondition(cboTrayTypeScrap, bAllNull: true);
            string s = Util.GetCondition(cboLocScrap, bAllNull: true);
            if (string.IsNullOrEmpty(s)) { }
            else if (s.Equals("EMP_STOCK_1F"))
            {
                dr["EQGRID"] = "STO";
                dr["S06"] = "1";
            }
            else if (s.Equals("EMP_STOCK_3F"))
            {
                dr["EQGRID"] = "STO";
                dr["S06"] = "3";
            }
            else if (s.Equals("ETC"))
            {
                dr["ETC"] = "Y";
            }
            else
            {
                //dr["S01"] = s; //2021.04.21 위치정보 그룹화 대응
                dr["EQPTSHORTNAME"] = s; //2021.04.21 위치정보 그룹화 대응
            }
            if (!string.IsNullOrEmpty(txtTrayIDScrap.Text)) dr["CSTID"] = txtTrayIDScrap.Text;
            dr["CST_MNGT_STAT_CODE"] = "S"; //폐기
            if ((bool)chkUse.IsChecked)
            {
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd ") + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd ") + dtpToTime.DateTime.Value.ToString("HH:mm:00");
            }
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            inDataTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
            if (dtRslt.Rows.Count > 0)
            {
                dtRslt.Columns.Add("CHK", typeof(bool));
            }
            //dgScrapTray.ItemsSource = DataTableConverter.Convert(dtRslt);
            Util.GridSetData(dgScrapTray, dtRslt, this.FrameOperation);
        }
        
        private void GetListEmpty()
        {
            try
            {
                string sBiz = "DA_SEL_STORAGE_EMPTY_TRAY";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CSTTYPE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQPTID"] = Util.GetCondition(cboEqptID, bAllNull: true);
                dr["CSTTYPE"] = Util.GetCondition(cboTrayTypeEmpty, bAllNull: true);
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);

                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Columns.Add("FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["CHK"] = false;
                        dtRslt.Rows[i]["FLAG"] = "N";
                    }

                    Util.GridSetData(dgEmptyTrayStorage, dtRslt, this.FrameOperation);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrayEnd(string sTray, int iRow)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("TRAY_ID", typeof(string));
                inDataTable.Columns.Add("MDF_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["TRAY_ID"] = sTray;
                dr["MDF_ID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_TRAY_END", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetEqptIdName(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_EMPTYTRAY", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "] " + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private void EmptyStorageDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                DataRow dr = RQSTDT.NewRow();
                RQSTDT.TableName = "RQSTDT";
                string bizRuleName = "";

                if (sClassId == "USE_FLAG")
                {
                    bizRuleName = "DA_BAS_SEL_CMN_CBO";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = sClassId;
                }
                /*else if (sClassId == "EQPTID")
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_EMPTYTRAY";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                }*/
                else if (sClassId == "EQPT_GR_TYPE_CODE")
                {
                    bizRuleName = "DA_BAS_SEL_CMN_CBO";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = sClassId;
                    dr["CMCODE_LIST"] = "3, 7"; // 3: 상온Aging, 7:출하Aging
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - dg.TopRows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        //2023.10.03 위치 콤보박스 변경 START 
        private void SetEqptLoc(C1ComboBox cbo, String[] sFilter, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2023.10.03 위치 콤보박스 변경 END 

        private void SetManualOffButtonVisible()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = "FORM_EMPTY_TRAY_MANUAL_USE";
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_USER_AUTH_INFO_FOR_BTN", "RQSTDT", "RSLTDT", inTable);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    // 공통코드가 등록되어 있으면 들어옴.
                    bool isAllowButton = false; // 기본 사용금지로 설정
                    foreach (DataRow drAuth in dtResult.Rows)
                    {
                        if (Util.NVC(drAuth["AUTH_USE_YN"]).Equals("Y"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(drAuth["AUTHID"]))) isAllowButton = true;
                        }
                        else if (Util.NVC(drAuth["AUTH_USE_YN"]).Equals("N"))
                        {
                            if (string.IsNullOrEmpty(Util.NVC(drAuth["AUTHID"]))) isAllowButton = true;
                        }
                    }

                    if (isAllowButton)
                    {
                        btnManualOff.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnManualOff.Visibility = Visibility.Collapsed;
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

        #region [조회 Tab]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void rdoOn_Checked(object sender, RoutedEventArgs e)
        {
            /*   if (cboLoc != null)
               {
                   CommonCombo_Form _combo = new CommonCombo_Form();
                   _combo.SetCombo(cboLoc, CommonCombo_Form.ComboStatus.ALL, sCase: "CONV_MAIN");
               }*/
        }

        private void rdoOff_Checked(object sender, RoutedEventArgs e)
        {
            /*if (sender != null)
            {
                CommonCombo_Form _combo = new CommonCombo_Form();
                _combo.SetCombo(cboLoc, CommonCombo_Form.ComboStatus.ALL, sCase: "PORT_MAIN");
            }*/
        }

        private void btnStatusChange_Click(object sender, RoutedEventArgs e)
        {

            FCS001_026_STATUS_CHANGE wndPopup = new FCS001_026_STATUS_CHANGE();
            wndPopup.FrameOperation = FrameOperation;

            string sTrayList = string.Empty;

            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS001_026_STATUS_CHANGE fcs001_026_status_change = new FCS001_026_STATUS_CHANGE();
            fcs001_026_status_change.FrameOperation = FrameOperation;
            if (fcs001_026_status_change != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(fcs001_026_status_change, Parameters);

                fcs001_026_status_change.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => fcs001_026_status_change.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_026_STATUS_CHANGE window = sender as FCS001_026_STATUS_CHANGE;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOut_Click(object sender, RoutedEventArgs e)
        {
            if (IsCnvrLocationOutLocUse())
            {
                FCS001_026_MANUAL_OUT_LOC manualOutLoc = new FCS001_026_MANUAL_OUT_LOC();
                manualOutLoc.FrameOperation = FrameOperation;

                if (manualOutLoc != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")) + "|";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[1];
                    Parameters[0] = sTrayList;

                    C1WindowExtension.SetParameters(manualOutLoc, Parameters);

                    manualOutLoc.Closed += new EventHandler(ManualOutLoc_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOutLoc.ShowModal()));
                }
            }
            else if (IsBothPortIdUse())
            {
                FCS001_026_MANUAL_OUT_PORT manualOutPort = new FCS001_026_MANUAL_OUT_PORT();
                manualOutPort.FrameOperation = FrameOperation;

                if (manualOutPort != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")) + "|";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[1];
                    Parameters[0] = sTrayList;

                    C1WindowExtension.SetParameters(manualOutPort, Parameters);

                    manualOutPort.Closed += new EventHandler(ManualOutPort_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOutPort.ShowModal()));
                }
            }
            else if (IsCnvDestinationOutUse())
            {
                FCS001_026_MANUAL_OUT_TRAY manualOutTray = new FCS001_026_MANUAL_OUT_TRAY();
                manualOutTray.FrameOperation = FrameOperation;

                if (manualOutTray != null)
                {
                    string sTrayList = string.Empty;

                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")) + "|";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = sTrayList;
                    Parameters[1] = cboTrayType.SelectedValue;

                    C1WindowExtension.SetParameters(manualOutTray, Parameters);

                    manualOutTray.Closed += new EventHandler(ManualOutTray_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOutTray.ShowModal()));
                }
            }
            else
            {
                FCS001_026_MANUAL_OUT manualOut = new FCS001_026_MANUAL_OUT();
                manualOut.FrameOperation = FrameOperation;

                if (manualOut != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")) + ",";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[1];
                    Parameters[0] = sTrayList;

                    C1WindowExtension.SetParameters(manualOut, Parameters);

                    manualOut.Closed += new EventHandler(ManualOut_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOut.ShowModal()));
                }
            }
        }

        private void ManualOut_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT window = sender as FCS001_026_MANUAL_OUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutLoc_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_LOC window = sender as FCS001_026_MANUAL_OUT_LOC;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutPort_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_PORT window = sender as FCS001_026_MANUAL_OUT_PORT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutTray_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_TRAY window = sender as FCS001_026_MANUAL_OUT_TRAY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOff_Click(object sender, RoutedEventArgs e)
        {
            string sTrayList = string.Empty;
            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + "|";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS001_026_MANUAL_OFF fcs001_026_manual_off = new FCS001_026_MANUAL_OFF();
            fcs001_026_manual_off.FrameOperation = FrameOperation;
            if (fcs001_026_manual_off != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(fcs001_026_manual_off, Parameters);

                fcs001_026_manual_off.Closed += new EventHandler(fcs001_026_manual_off_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => fcs001_026_manual_off.ShowModal()));
            }
        }

        private void fcs001_026_manual_off_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OFF window = sender as FCS001_026_MANUAL_OFF;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnShippingCtl_Click(object sender, EventArgs e)
        {
            // 2023.11.13 SHIPPING OUTPUT CONTROL ADD
            FCS001_026_SHIPPING_OUTPUT_CONTROL shippingOutputControl = new FCS001_026_SHIPPING_OUTPUT_CONTROL();
            shippingOutputControl.FrameOperation = FrameOperation;

            this.Dispatcher.BeginInvoke(new Action(() => shippingOutputControl.ShowModal()));

            shippingOutputControl.Closed += new EventHandler(btnShippingCtl_Closed);
        }

        private void btnShippingCtl_Closed(object sender, EventArgs e)
        {
            FCS001_026_SHIPPING_OUTPUT_CONTROL window = sender as FCS001_026_SHIPPING_OUTPUT_CONTROL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnWorkEnd_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0236", (result) => //진행중인 공정을 종료하시겠습니까?
            {
                if (result != MessageBoxResult.OK)
                {
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (_util.GetDataGridCheckValue(dgEmptyTray, "CHK", i))
                        {
                            SetTrayEnd(Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "TRAY_ID")), i);
                        }
                    }
                    Util.MessageInfo("FM_ME_0111");  //공정종료를 완료하였습니다.
                    GetList();
                }
            });
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < numCnt.Value && i < dgEmptyTray.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", true);
            }
        }

        private void dgEmptyTray_ButtonClicked(object sender, RoutedEventArgs e)
        {
            //   if(dgEmptyTray.GetValue(e.Rows.DateItem,"RACK_YN"))
        }

        /* private void dgEmptyTray_BeganEdit(object sender, DataGridBeganEditEventArgs e)
         {
             try
             {
                 if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "LOAD_REP_CSTID"))))
                 {
                     //동일한 설비 체크하기
                     bool bCheck = (bool)DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "CHK");
                     string sEqpID = string.Empty;
                     sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "LOAD_REP_CSTID"));

                     if (!string.IsNullOrEmpty(sEqpID)) {

                         for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                         {
                             if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID))
                             {
                                 DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", !bCheck);
                             }
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 Util.MessageException(ex);
             }
         }*/

        /// <summary>
        /// Excel 업로드
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            GetList(GetExcelData());

        }

        private string GetExcelData()
        {
            string sColData = string.Empty;

            Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    sColData = LoadExcel(stream, (int)0);
                }
            }
            return sColData;
        }

        private string LoadExcel(Stream excelFileStream, int sheetNo)
        {
            string sColData = string.Empty;
            try
            {
                string sSeparator = "|";
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                }

                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null)
                    {
                        sColData += sheet.GetCell(rowInx, 0).Text + sSeparator;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sColData;
        }

        // 전체 선택, 해제
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgEmptyTray);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgEmptyTray);

        }
        #endregion

        #region [폐기 Tray 조회 Tab]
        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            GetListScrap();
        }

        private void rdoOnScrap_Checked(object sender, RoutedEventArgs e)
        {
            /*  if (cboTrayLocScrap == null)
                  return;
              if (sender != null)
              {
                  CommonCombo_Form _combo = new CommonCombo_Form();
                  _combo.SetCombo(cboTrayLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "CONV_MAIN");
              }*/
        }

        private void rdoOffScrap_Checked(object sender, RoutedEventArgs e)
        {
            /*if (cboTrayLocScrap == null)
                return;
            if (sender != null)
            {
                CommonCombo_Form _combo = new CommonCombo_Form();
                _combo.SetCombo(cboTrayLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "PORT_MAIN");
            }*/
        }

        //전체선택 , 전체선택 해제
        private void chkHeaderAllScrap_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgScrapTray);
        }

        private void chkHeaderAllScrap_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgScrapTray);
        }
        #endregion

        private void dgEmptyTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //동일한 설비 체크하기
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEmptyTray.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                bool bCheck = !(bool)DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "CHK");
                string sEqpID = string.Empty;
                sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "LOAD_REP_CSTID"));

                if (string.IsNullOrEmpty(sEqpID)) return;
                for (int i = 0; i < this.dgEmptyTray.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID)
                        && i != cell.Row.Index)
                    {
                        DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", bCheck);
                    }
                }
            }
        }

        private void dgEmptyTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgEmptyTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgScrapTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgScrapTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private bool IsCnvrLocationOutLocUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CNVR_LOCATION_OUT_LOC_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
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
            }
            return false;
        }

        private bool IsBothPortIdUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "BOTH_PORT_ID_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
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
            }
            return false;
        }

        private bool IsCnvDestinationOutUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CNVR_DESTINATION_OUT_USE";
                dr["COM_CODE"] = "CNVR_DESTINATION_OUT_USE";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
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
            }
            return false;
        }

        #region [상온/출하 Aging 보관 Tab]

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //편집중인 내역 Commit.
                dgEmptyTrayStorage.EndEdit(true);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("NOTE", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = null;
                for (int i = 0; i < dgEmptyTrayStorage.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "FLAG")).Equals("Y") &&
                        Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {
                        dr = dtRqst.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "EQPTID"));
                        dr["CSTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "TRAY_TYPE"));
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "USE_FLAG"));
                        dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "ROUTID"));
                        dr["EQPT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "EQPT_GR_TYPE_CODE"));
                        dr["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[i].DataItem, "NOTE"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        if (string.IsNullOrWhiteSpace(Util.NVC(dr["EQPTID"])))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("EQPTID"));
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(Util.NVC(dr["CSTTYPE"])))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("TRAY_TYPE"));
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(Util.NVC(dr["ROUTID"])))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("ROUTID"));
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(Util.NVC(dr["EQPT_GR_TYPE_CODE"])))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("EQPT_GR_TYPE_CODE"));
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(Util.NVC(dr["NOTE"])))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("NOTE"));
                            return;
                        }
                    }
                }

                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_TRAY_UI", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
                        {
                            Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                        }

                        GetListEmpty();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmptyTrayStorage.Rows.Count > 1)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgEmptyTrayStorage.Rows[dgEmptyTrayStorage.Rows.Count - 1].DataItem, "FLAG"));
                if (flag.Equals("Y")) DataGridRowRemove(dgEmptyTrayStorage);
                dgEmptyTrayStorage.ScrollIntoView(dgEmptyTrayStorage.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }

        private void btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            EmptyStorageDataGridRowAdd(dgEmptyTrayStorage, 1);
            SetGridCboItem(dgEmptyTrayStorage.Columns["USE_FLAG"], "USE_FLAG");
            //SetGridCboItem(dgEmptyTrayStorage.Columns["EQPTID"], "EQPTID");
            SetGridCboItem(dgEmptyTrayStorage.Columns["EQPT_GR_TYPE_CODE"], "EQPT_GR_TYPE_CODE");
            DataTableConverter.SetValue(dgEmptyTrayStorage.Rows[dgEmptyTrayStorage.Rows.Count - 1].DataItem, "FLAG", "Y");
            if (dgEmptyTrayStorage.Rows.Count > 0)
            {
                dgEmptyTrayStorage.ScrollIntoView(dgEmptyTrayStorage.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEqptIdName(cboEqptID);
        }
        private void btnSearchEmpty_Click(object sender, RoutedEventArgs e)
        {
            GetListEmpty();
        }

        //전체선택 , 전체선택 해제
        private void chkHeaderAllEmpty_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgEmptyTrayStorage);
        }

        private void chkHeaderAllEmpty_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgEmptyTrayStorage);
        }

        private void dgEmptyTrayStorage_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        #endregion

        #endregion

        private void TrayID_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            if (text.Contains(",")) btnSearch.PerformClick();
        }

        private void chkUse_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
            dtpToDate.IsEnabled = true;
            dtpToTime.IsEnabled = true;
        }

        private void chkUse_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
            dtpToDate.IsEnabled = false;
            dtpToTime.IsEnabled = false;
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private object GetValue(object obj, string property)
        {
            object oValue = null;
            if (obj != null)
            {
                if (obj is DataRowView)
                {

                    try
                    {
                        DataRowView drv = obj as DataRowView;
                        oValue = drv[property] == DBNull.Value ? null : drv[property];
                    }
                    catch
                    { }
                }
            }

            return oValue;
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void btnSaveEmptyTray_Click(object sender, RoutedEventArgs e)
        {
            CreateEmptyTrayInfomaion();
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void CreateEmptyTrayInfomaion()
        {
            try
            {
                Util.MessageConfirm("SFU1621", (result) => //생성 하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("CSTID", typeof(string));

                    for (int i = 0; i < dgInputList.Rows.Count; i++)
                    {
                        if (Util.NVC(GetValue(dgInputList.Rows[i].DataItem, "TRAY_ID")) != "")
                        {
                            DataRow dr = inData.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["USERID"] = LoginInfo.USERID;
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["CSTID"] = Util.NVC(GetValue(dgInputList.Rows[i].DataItem, "TRAY_ID"));
                            inData.Rows.Add(dr);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_SET_CREATE_EMPTY_TRAY_UI", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //생성완료하였습니다.
                                Util.MessageInfo("FM_ME_0160");

                                // 생성 완료후 Grid Data Clear & 초기화
                                this.ClearValidation();
                                dgInputList.ClearRows();
                                EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
                            }
                            else
                            {
                                if (bizResult.Tables["OUTDATA"].Rows[0]["ERR_CODE"].ToString() == "NG_DUP")
                                {
                                    // 중복된 Tray 정보가 존재합니다.
                                    Util.MessageInfo("SFU3682");
                                }
                                //생성실패하였습니다. (Result Code : {0})
                                Util.MessageInfo("ME_0159");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                        }
                    }, inDataSet);

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void EqpDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                //DataTable dt = new DataTable();

                //foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                //{
                //    dt.Columns.Add(Convert.ToString(col.Name));
                //}

                //dt = dg.GetDataTable().Copy();

                //for (int i = dg.Rows.Count; i < iRowcount; i++)
                //{
                //    DataRow dr = dt.NewRow();
                //    dt.Rows.Add(dr);
                //    dg.BeginEdit();
                //    dg.ItemsSource = DataTableConverter.Convert(dt);
                //    dg.EndEdit();
                //}

                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void btnSearchEmptyTray_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayType();
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void SearchTrayType()
        {
            try
            {
                string cstList = string.Empty;
                int iTrayLength = 0;
                DataTable dt = DataTableConverter.Convert(dgInputList.ItemsSource);

                iTrayLength = GetTrayIDLength();

                #region MyRegion
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    if (dt.Rows[row]["TRAY_ID"].ToString().Equals(string.Empty))
                    {
                        continue;
                    }

                    if (cstList.Length == 0)
                    {
                        cstList += dt.Rows[row]["TRAY_ID"].ToString();
                    }
                    else
                    {
                        cstList += "," + dt.Rows[row]["TRAY_ID"].ToString();
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("TRAY_ID_LIST");
                RQSTDT.Columns.Add("AREAID");

                DataRow dr = RQSTDT.NewRow();
                dr["TRAY_ID_LIST"] = cstList;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_TYPE", "INDATA", "OUTDATA", RQSTDT);

                if (dsResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgInputList, dsResult, FrameOperation, true);
                    EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));

                    for (int row = 0; row < dsResult.Rows.Count; row++)
                    {
                        if (dsResult.Rows[row]["TRAY_TYPE"].ToString().Equals(string.Empty))
                        {

                            Util.MessageInfo("50047", dsResult.Rows[row]["TRAY_ID"].ToString());
                            break;
                        }

                        if (dsResult.Rows[row]["TRAY_ID"].ToString().Length != iTrayLength)
                        {
                            Util.MessageInfo("50066", dsResult.Rows[row]["TRAY_ID"].ToString());
                            break;
                        }
                    }
                }
                #endregion                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private int GetTrayIDLength()
        {
            int iLength = 0;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_TRAYID_SIZE_VALIDATION";
                dr["CMCODE"] = "10";
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    int.TryParse(Convert.ToString(dtRslt.Rows[0]["CMCODE"]), out iLength);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return iLength;
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList.ClearRows();
            //DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
            EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void Loc_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList.Rows.Count > 0)
            {
                EqpDataGridRowAdd(dgInputList, 1);
                //DataTableConverter.SetValue(dgInputList.Rows[dgInputList.Rows.Count - 1].DataItem, "FLAG", "Y");
                dgInputList.ScrollIntoView(dgInputList.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void Loc_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputList.Rows.Count > 0)
            {
                //string flag = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[dgInputList.Rows.Count - 1].DataItem, "FLAG"));
                //if (flag.Equals("Y")) DataGridRowRemove(dgInputList);
                if (dgInputList.Rows.Count - 1 == 0)
                {
                    Util.MessageInfo("SFU1121");
                    return;
                }
                DataGridRowRemove(dgInputList);
                dgInputList.ScrollIntoView(dgInputList.Rows.Count - 1, 1); //스크롤 하단 고정
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void btnExcelImport_Click(object sender, RoutedEventArgs e)
        {
            bool rslt = ImportExcel(dgInputList, "TRAY_ID");

            if (!rslt)
            {
                Util.MessageValidation("SFU1497");
            }

            EqpDataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private Boolean ImportExcel(C1.WPF.DataGrid.C1DataGrid dataGrid, string AddDatacol)
        {
            Boolean rslt = false;

            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    if (!dtInfo.Columns.Contains(AddDatacol))
                    {
                        dtInfo.Columns.Add(AddDatacol, typeof(string));
                    }

                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        if (sheet.GetCell(rowInx, 0) != null)
                        {
                            DataRow dr = dtInfo.NewRow();
                            dr[AddDatacol] = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            dtInfo.Rows.Add(dr);
                        }
                    }

                    Util.GridSetData(dataGrid, dtInfo, FrameOperation, true);

                    rslt = true;
                }
            }
            return rslt;
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (e.Cell.Column.Name.ToString().Equals("TRAY_TYPE"))
                                {
                                    if (Util.NVC(GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TRAY_TYPE")).Length == 0)
                                    {
                                        dataGrid.Rows[e.Cell.Row.Index].Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    }
                                }


                                if (e.Cell.Column.Name.ToString().Equals("TRAY_ID"))
                                {
                                    if (Util.NVC(GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TRAY_ID")).Length != 10)
                                    {
                                        dataGrid.Rows[e.Cell.Row.Index].Presenter.Foreground = new SolidColorBrush(Colors.HotPink);
                                    }
                                }
                            }
                        }
                    }
                }));
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void dgInputList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            try
            {

                //if (e.Cell.Column.Name.ToString().Equals("CHK"))
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")) != "N")
                //    {
                //        if (e.Cell.Presenter != null)
                //        {
                //            if (e.Cell.Row.Type == DataGridRowType.Item)
                //            {
                //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                //                e.Cell.Presenter.Background = null;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2024.03.26 남형희 : E20240123-000627 공 Tray 수동 생성 Tab 추가 및 엑셀 Import, Tray Type 조회, Validation 추가
        private void dgInputList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        }

    }
}


