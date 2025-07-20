/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 관리
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.08.23 윤지해 CSR ID C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
  2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
  2023.03.07 윤지해 CSR ID E20230220-000068	FCR 초기화, Loss코드, 부동내용 필수 등록으로 validation 수정
  2023.06.05 윤지해 CSR ID E20230330-001442 LOSS Lv2, Lv3 선택 시 조건에 따라 원인설비 및 원인설비별 FCR 코드 매핑 기능 변경
  2023.06.08 김도형 CSR ID [E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
  2023.07.03 이윤중 CSR ID E20230627-000461 2023-07-01 Loss 체계 개선 이전 날짜 선택 불가 로직 추가 (임시)
  2023.07.06 이윤중 CSR ID E20230627-000461 설비 Loss 변경 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
  2023.07.07 김도형 CSR ID [E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
  2023.07.18 윤지해 CSR ID E20230703-000158 COM001_014_LOSS_DETL_FCR 화면으로 부동내용 팝업 변경
  2023.07.20 김도형 CSR ID [E20230707-001616] 원인 설비별 LOSS 관리 <> 'Y' 일 경우 원인설비공정-> 메인설비공정으로 변경
  2023.07.17 김대현 CSR ID E20230711-000645  MES 시스템의 설비 Loss 수정 승인 요청 기능을 위한 신규 개발/기능 변경
  2023.08.10 김대현 CSR ID E20230711-000645  MES 시스템의 설비 Loss 수정 승인 요청 기능을 위한 신규 개발/기능 변경(Pack)
  2023.11.07 김대현 ValidateEqptLossAppr() 파라미터에서 STRT_DTTM, END_DTTM 제거
  2023.12.19 김대현 E20231208-001776 설비 Loss 등록, 수정 화면 통합
  2024.05.31 안유수 CSR ID E20240527-000288 설비 LOSS 데이터 저장 조건 변경 -> 기존 STRT_DTTM_YMDHMS 조건으로 업데이트 처리하는 조건에서,
                           PRE_LOSS_SEQNO 값 기준으로 처리 되도록 수정, 화면을 REFLASH 처리하지 않은 상태에서 수정된 LOSS 이력에 대한 VALIDATION 추가
  2024.07.23 안유수 CSR ID E20240704-001632 설비 LOSS SaveDataChk() 매서드 내 체크 대상 loss 이력 데이터 조회 조건 중 누락된 EQPTID 조회 조건 추가, 저장, 일괄저장 구분에 따라 설비 ID 변경되도록 수정
  2025.07.03 김선영 ESHG 수정, 작업조 조회 2.0 전환 DA로 변경, MIN_SECONDS 파라메터 추가 
  2025.07.04 김선영 ESHG 수정, SPLIT 파라메터 변경 : 설비LOSS 등록 화면과 동일하게 맞춤
  **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_239 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Hashtable hash_loss_color = new Hashtable();

        DataTable dtMainList = new DataTable();

        DataTable dtBeforeList = new DataTable();

        DataTable AreaTime;

        DataTable dtShift;

        //String sMainEqptID;
       // String sEqptID;
        String sEqptLevel;
        DataSet dsEqptTimeList = null;
        Util _Util = new Util();
        Hashtable org_set;

        List<string> liProcId;

        int iEqptCnt;

        string RunSplit; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분

        // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        string _fTypeCode = "F";
        string _cTypeCode = "C";
        string _rTypeCode = "R";

        // 2023.05.23 윤지해 CSR ID E20230330-001442 원인설비별 LOSS 등록 여부 확인
        bool isCauseEquipment = false;
        string occurEqptFlag = string.Empty;
        bool bMobile = false;
        string _grid_proc;
        string _grid_eqpt;

        string strAttr1 = string.Empty;
        string strAttr2 = string.Empty;
        string sNowDay = string.Empty;
        bool bUseEqptLossAppr = false; // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가
        string sSearchDay = "";

        string _wrk_date = string.Empty;
        string _areaid = string.Empty;
        string _eqptid = string.Empty;

        DateTime dBaseDate = new DateTime();

        public COM001_239()
        {
            InitializeComponent();

            InitCombo();

            InitGrid();

            GetLossColor();

            if (string.Equals(GetAreaType(), "E"))
            {
                lotTname.Visibility = Visibility.Visible;
            }


            GetEqptLossApprAuth();  // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            // 2023.06.05 윤지해 CSR ID E20230330-001442 소형 조립 확인용 추가
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("M"))
                bMobile = true;
            else
                bMobile = false;

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboShift };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboShift, cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: "cboEquipmentEqptLoss");

            //작업조
            C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent);

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboColor.Items.Add(cbItemTitle);

            C1ComboBoxItem cbItemRun = new C1ComboBoxItem();
            cbItemRun.Content = "Run";
            cbItemRun.Background = ColorToBrush(GridBackColor.R);
            cboColor.Items.Add(cbItemRun);

            C1ComboBoxItem cbItemWait = new C1ComboBoxItem();
            cbItemWait.Content = "Wait";
            cbItemWait.Background = ColorToBrush(GridBackColor.W);
            cboColor.Items.Add(cbItemWait);

            C1ComboBoxItem cbItemTrouble = new C1ComboBoxItem();
            cbItemTrouble.Content = "Trouble";
            cbItemTrouble.Background = ColorToBrush(GridBackColor.T);
            cboColor.Items.Add(cbItemTrouble);

            C1ComboBoxItem cbItemOff = new C1ComboBoxItem();
            cbItemOff.Content = "OFF";
            cbItemOff.Background = ColorToBrush(GridBackColor.F);
            cboColor.Items.Add(cbItemOff);

            C1ComboBoxItem cbItemUserStop = new C1ComboBoxItem();
            cbItemUserStop.Content = "UserStop";
            cbItemUserStop.Background = ColorToBrush(GridBackColor.U);
            cboColor.Items.Add(cbItemUserStop);

            cboColor.SelectedIndex = 0;

            //작업조 Default 셋팅

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));


                DataRow row = dt.NewRow();
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SET_SHIFT", "RQSTDT", "RSLTDT", dt);

                if (dtRslt.Rows.Count != 0)
                {
                    cboShift.SelectedValue = Convert.ToString(dtRslt.Rows[0]["SHFT_ID"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            // 2023.05.23 윤지해 CSR ID E20230330-001442 추가
            ChkProcessEquipmentSegment();

            //2023-07-03 - 설비 Loss 체계 개편 관련 제한 로직 추가 - yjlee
            ldpDatePicker.SelectedDataTimeChanged += ldpDatePicker_SelectedDataTimeChanged;
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
        }

        private void InitInsertCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //loss 코드
            ////C1ComboBox[] cboLossChild = { cboLossDetl };
            ////_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild);

            //C1ComboBox[] cboLossChild = { cboLossDetl };
            //string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
            //_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
            if (occurEqptFlag.Equals("Y"))
            {
                string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), occurEqptFlag, string.Empty };
                _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
            }
            else
            {
                if (bUseEqptLossAppr)
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeAll");
                }
                else
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
                }
            }

            //부동코드
            // 2023.06.05 윤지해 CSR ID E20230330-001442 부동내용 콤보박스 세팅 로직 변경
            string occurEqptValue = (string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
            C1ComboBox[] cboLossDetlParent = { cboLoss, cboEquipment };
            string[] sFilter = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
            //_combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);
            string[] sFilter3 = { cboLoss.SelectedValue.ToString(), cboEquipment.SelectedValue.ToString(), occurEqptValue };
            if (occurEqptFlag.Equals("Y") && !(string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
            {
                _combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "cboOccrLossDetl");
            }
            else
            {
                _combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);
            }

            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경_주석처리
            ////현상코드
            //String[] sFilterFailure = { "F" };
            //_combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

            ////원인코드
            //String[] sFilterCause = { "C" };
            //_combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

            ////조치코드
            //String[] sFilterResolution = { "R" };
            //_combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");
            #endregion

            //원인설비
            // 2023.06.05 윤지해 CSR ID E20230330-001442 원인설비 콤보박스 세팅 로직 변경
            C1ComboBox[] cboOccurEqptParent = { cboEquipment };
            String[] sFilterEquipment = { Util.GetCondition(cboEquipment) };
            //_combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
            if (!bMobile && isCauseEquipment)  // 2023.05.28 윤지해 CSR ID E20230330-001442 PACK, 소형이 아니고 원인설비별 LOSS 등록을 하는 곳일 경우 등록된 원인설비로 LIST-UP && 이벤트 추가
            {
                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, sFilter: sFilterEquipment, sCase: "cboOccurFcrEqpt");
                cboOccurEqpt.SelectedValueChanged += cboOccurEqpt_SelectedItemChanged;
            }
            else
            {
                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                cboOccurEqpt.SelectedValueChanged -= cboOccurEqpt_SelectedItemChanged;
            }

            //최근
            if (bUseEqptLossAppr)
            {
                string[] sFilterLoss = { Convert.ToString(cboEquipment.SelectedValue) };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "LAST_LOSS_ALL");
            }
            else
            {
                C1ComboBox[] cboLastLossParent = { cboEquipment };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);
            }

            //동-라인-공정별 로스 맵핑
            string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
            _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
        }

        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {
                int gridRowCount = 220;

                _grid.Width = 3000;

                _grid.Height = gridRowCount * 15;

                for (int i = 0; i < 361; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = GridLength.Auto;
                    }
                    else { gridCol1.Width = new GridLength(3); }

                    _grid.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < gridRowCount; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow1);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GetLossColor()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtRslt);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["LOSS_NAME"];
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["DISPCOLOR"].ToString()));
                    cboColor.Items.Add(cbItem);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }



        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기



            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToShortDateString();
            dt.Rows.Add(row);

            AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (AreaTime.Rows.Count == 0) { }
            if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return;
            }
            TimeSpan tmp = DateTime.Parse(System.DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;//DateTime.Parse("06:59:59").TimeOfDay;//

            if (tmp < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
            {
                ldpDatePicker.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            }




        }

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                //cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                SetEquipment();
                SetShift();
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
            }
        }
        #endregion

        #region [설비] - 조회 조건
        private void CboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (cboEquipment.SelectedIndex == 0)
                cboProcess.SelectedIndex = 0;
            else
                cboProcess.SelectedValue = liProcId[cboEquipment.SelectedIndex - 1].ToString();

        }
        #endregion

        #region [작업일] - 조회 조건

        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //2023.07.06 - 설비 Loss 등록 제한 기준 날짜 공통코드 추가 (EQPT_LOSS_CODE_APPLY_AREA - ATTRIBUTE5 - 2023-07-01 00:00:00.000)
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);

            if (dBaseDate != null
                && ldpDatePicker.SelectedDateTime < dBaseDate)
            {
                Util.MessageValidation("SFU9040", dBaseDate.ToString("yyyy-MM-dd")); // 설비Loss 체계 개편에 따라, 7월 이전 설비Loss 등록이 불가합니다. 
                ldpDatePicker.SelectedDateTime = dBaseDate;
                return;
            }

            if (ldpDatePicker.SelectedDateTime.Year > 1 && ldpDatePicker.SelectedDateTime.Year > 1)
            {
                SetShift();
            }
        }
        #endregion

        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 2023.05.23 윤지해 CSR ID E20230330-001442 추가
            ChkProcessEquipmentSegment();

            string sEqpt = Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요
            if (sEqpt.Equals("")) return;

            DataTable dt = new DataTable();
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["EQPTID"] = sEqpt;
            dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLOSS_CBO", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                sEqptLevel = Convert.ToString(dtResult.Rows[0]["EQPTLEVEL"]);
            }
            
            // 2023.05.23 윤지해 CSR ID E20230330-001442 추가
            _grid_proc = Convert.ToString(cboProcess.SelectedValue);
            _grid_eqpt = sEqpt;

            // 2023.06.05 윤지해 CSR ID E20230330-001442 추가
            occurEqptFlag = !bMobile && isCauseEquipment ? "Y" : "N";

            //초기화
            InitInsertCombo();

            ClearGrid();

            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            //txtLossNote.Text = "";
            txtFCRCode.Text = "";

            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            cboLossDetl.SelectedIndex = 0;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

            SelectLossRunArea();
            indataset();

            GetEqptLossRawList();

            SelectProcess();
            GetEqptLossDetailList();

            sSearchDay = ldpDatePicker.SelectedDateTime.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 색지도 내에서 클릭시 발생
        /// </summary>
        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            Border aa = sender as Border;

            org_set = aa.Tag as Hashtable;

            if (e.ChangedButton == MouseButton.Right)
            {
                if (!org_set["STATUS"].ToString().Equals("R"))
                {
                    ContextMenu menu = this.FindResource("_gridMenu") as ContextMenu;
                    menu.PlacementTarget = sender as Border;
                    menu.IsOpen = true;

                    for (int i = 0; i < menu.Items.Count; i++)
                    {
                        MenuItem item = menu.Items[i] as MenuItem;

                        switch (item.Name.ToString())
                        {
                            case "LossDetail":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss내역보기");
                                item.Click -= lossDetail_Click;
                                item.Click += lossDetail_Click;
                                break;

                            case "LossSplit":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss분할");
                                item.Click -= lossSplit_Click;
                                item.Click += lossSplit_Click;
                                break;
                        }
                    }

                }
                if (RunSplit.Equals("Y"))
                {
                    if (org_set["STATUS"].ToString().Equals("R")) //추가
                    {
                        string startTime = txtStartHidn.Text;
                        string endTime = txtEndHidn.Text;
                        if (startTime.Equals("") || endTime.Equals(""))
                        {
                            return;
                        }

                        DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime  + "and EQPTID = '"+ org_set["EQPTID"].ToString().Substring(1) + "'").CopyToDataTable();
                        if (dt.Select("EIOSTAT <> 'R'").Count() > 0)
                        {
                            Util.MessageValidation("SFU3204"); //운영설비 사이에 Loss가 존재합니다.
                            btnReset_Click(null, null);
                            return;
                        }

                        COM001_239_RUN_SPLIT wndPopup = new COM001_239_RUN_SPLIT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[10];
                            Parameters[0] = org_set["EQPTID"].ToString();
                            Parameters[1] = org_set["TIME"].ToString();
                            Parameters[2] = startTime;
                            Parameters[3] = endTime;
                            Parameters[4] = cboArea.SelectedValue.ToString();
                            Parameters[5] = Util.GetCondition(ldpDatePicker); //ldpDatePicker.SelectedDateTime.ToShortDateString();
                            Parameters[6] = this;
                            Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[8] = cboProcess.SelectedValue.ToString();
                            Parameters[9] = !bMobile && isCauseEquipment ? "Y" : "N";  // 2023.06.05 윤지해 CSR ID E20230330-001442 추가

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }

            }
            else
            {

                if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
                {
                    btnReset_Click(null, null);
                }
                else
                {
                    if (!(dtMainList.Select("CHK = 1").Count() == 0))
                    {
                        if (!(dtMainList.Select("CHK = 1")[0]["EQPTID"]).ToString().Equals(org_set["EQPTID"].ToString().Substring(1)))
                        {
                            btnReset_Click(null, null);
                        }
                    }

                    setMapColor(org_set["EQPTID"].ToString(), org_set["TIME"].ToString(), "MAP");


                }
            }
        }

        private void lossDetail_Click(object sender, RoutedEventArgs e)
        {
            COM001_014_TROUBLE wndPopup = new COM001_014_TROUBLE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = org_set["EQPTID"].ToString();
                Parameters[1] = org_set["TIME"].ToString();
                Parameters[2] = cboShift.SelectedValue.ToString().Equals("") ? 20 : 10;
                Parameters[3] = Util.GetCondition(ldpDatePicker);//ldpDatePicker.SelectedDateTime.ToShortDateString();


                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.Show()));

            }
        }

        private void lossSplit_Click(object sender, RoutedEventArgs e)
        {
            if (!org_set["STATUS"].ToString().Equals("R")) //추가
            {
                string startTime = txtStartHidn.Text;
                string endTime = txtEndHidn.Text;
                if (startTime.Equals("") || endTime.Equals(""))
                {
                    return;
                }

                DataTable tmp = new DataTable();
                tmp.Columns.Add("EQPTID", typeof(string));
                tmp.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));

                DataRow dr = tmp.NewRow();
                dr["EQPTID"] = org_set["EQPTID"].ToString().Substring(1);
                dr["STRT_DTTM_YMDHMS"] = startTime;
                tmp.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_END_DTTM", "RQSTDT", "RSLTDT", tmp);
                if (result.Rows.Count != 0)
                {
                    if (Convert.ToString(result.Rows[0]["END_DTTM"]).Equals(""))
                    {
                        Util.MessageValidation("SFU4244"); // 부동이 끝나지 않아 데이터를 분할 할 수 없습니다.
                        return;
                    }
                }

                DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime + "and EQPTID = '"+ org_set["EQPTID"].ToString().Substring(1) +"'").CopyToDataTable();
                if (dt.Select("EIOSTAT = 'R'").Count() > 0)
                {
                    Util.MessageValidation("SFU3511"); //Run상태가 존재합니다
                    btnReset_Click(null, null);
                    return;
                }

                if (dt.Select().Count() > 1)
                {
                    Util.MessageValidation("SFU3512"); //하나의 부동상태만 선택해주세요
                    btnReset_Click(null, null);
                    return;
                }

                COM001_239_LOSS_SPLIT wndPopup = new COM001_239_LOSS_SPLIT();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = org_set["EQPTID"].ToString();
                    Parameters[1] = org_set["TIME"].ToString();
                    Parameters[2] = startTime;
                    Parameters[3] = endTime;
                    Parameters[4] = cboArea.SelectedValue.ToString();
                    Parameters[5] = Util.GetCondition(ldpDatePicker);
                    Parameters[6] = this;
                    Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[8] = cboProcess.SelectedValue.ToString();
                    Parameters[9] = !bMobile && isCauseEquipment ? "Y" : "N";  // 2023.06.05 윤지해 CSR ID E20230330-001442 추가

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopup_Closed2);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        /// <summary>
        /// 초기화 버튼 클릭시
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            resetMapColor();
            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            //txtLossNote.Text = "";
            txtFCRCode.Text = "";

            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            cboLossDetl.SelectedIndex = 0;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return;
            }

            if (!ValidateEqptLossAppr("SAVE"))
            {
                return;
            }

            if (!SaveDataChk(GetPreLossSeqnoForSave(),"SAVE"))
            {
                Util.MessageInfo("SUF9018"); // 업데이트된 LOSS DATA가 존재합니다.  화면을 다시 조회해주세요.
                return;
            }

            if (new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim().Length > 1000)
            {
                Util.MessageValidation("SFU5182");  //비고는 최대 1000자 까지 가능합니다.
                rtbLossNote.Focus();
                return;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") > 0)
            {
                Util.MessageValidation("SFU3490"); //하나의 부동내역을 저장 할 경우 check box선택을 모두 해제 후 \r\n 한개의 행만 더블클릭  해주세요
                return;
            }

            ValidateNonRegisterLoss("ONE");

            if (bUseEqptLossAppr)
            {
                // 당일 수정건이 아닌 경우
                if (ValidationChkDDay().Equals("AUTH_ONLY"))
                {
                    string lossCode = Util.GetCondition(cboLoss);
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOSS_CODE"] = lossCode;
                    RQSTDT.Rows.Add(dr);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                    if (result.Rows.Count != 0)
                    {
                        string upprLossCode = Convert.ToString(result.Rows[0]["UPPR_LOSS_CODE"]);
                        // 수정할 Loss가 비조업인 경우
                        if (upprLossCode.Equals("10000"))
                        {
                            ApprovalProcess();
                        }
                        else
                        {
                            SaveProcess();
                        }
                    }
                }
                else
                {
                    SaveProcess();
                }
            }
            else
            {
                SaveProcess();
            }
        }

        private void SaveProcess()
        {
            String sEqptid = dtMainList.Select("CHK = 1").Count() == 0 ? cboEquipment.SelectedValue.ToString() : (dtMainList.Select("CHK = 1")[0]["EQPTID"]).ToString();
            //DataRow[] dtRow = dtMainList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_END <= '" + txtEndHidn.Text + "' and EQPTID = '" + sEqptid + "'");

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("PRE_LOSS_SEQNO", typeof(string));
            RQSTDT.Columns.Add("SAVETYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석처리
            //string msg = "";

            //// new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
            //if (!(new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Equals("")) && cboLoss.Text.Equals("-SELECT-") && cboLossDetl.Text.Equals("-SELECT-"))
            //{
            //    msg = "SFU3441";//"해당 Loss의 비고 항목만 저장하시겠습니까?";

            //    dr["EQPTID"] = sEqptid;//Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요

            //    if (dr["EQPTID"].Equals("")) return;
            //    dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            //    dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            //    dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
            //    dr["LOSS_CODE"] = null;
            //    //if (dr["LOSS_CODE"].Equals("")) return;
            //    dr["LOSS_DETL_CODE"] = null;
            //    dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
            //    dr["SYMP_CODE"] = null;
            //    dr["CAUSE_CODE"] = null;
            //    dr["REPAIR_CODE"] = null;
            //    dr["OCCR_EQPTID"] = null;
            //    dr["USERID"] = LoginInfo.USERID;
            //    RQSTDT.Rows.Add(dr);

            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //    {

            //        if (result.ToString().Equals("OK"))
            //        {

            //            DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK", "RQSTDT", "RSLTDT", RQSTDT);
            //            btnSearch_Click(null, null);
            //            chkT.IsChecked = false;
            //            chkW.IsChecked = false;
            //            chkU.IsChecked = false;

            //            Util.AlertInfo("SFU1270");  //저장되었습니다.
            //        }
            //    });
            //}
            //else
            //{
            #endregion
            dr["AREAID"] = _areaid;
            dr["EQPTID"] = sEqptid; //Util.GetCondition(cboEquipment, "SFU1153"); // 설비를 선택하세요
            if (dr["EQPTID"].Equals("")) return;
            dr["WRK_DATE"] = _wrk_date;
            dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
            dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); //LOSS는필수항목입니다
            if (dr["LOSS_CODE"].Equals("")) return;

            dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
            if (dr["LOSS_DETL_CODE"].Equals(""))
            {
                if (cboLossDetl.Items.Count > 1)
                {
                    // 부동내용을 입력하세요.
                    Util.MessageValidation("SFU3631");
                    return;
                }
            }

            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
            dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
            dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
            dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
            dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
            dr["USERID"] = LoginInfo.USERID;
            dr["PRE_LOSS_SEQNO"] = GetPreLossSeqnoForSave();
            //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건 
            //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
            string sEqptLossFcrCheck = GetprodEqptLossFcrGroupChkResult(sEqptid,                          // 메인설비
                                                                        Util.GetCondition(cboOccurEqpt),  // 원인설비
                                                                        Util.GetCondition(cboLossDetl),
                                                                        Util.GetCondition(cboLoss),
                                                                        Util.GetCondition(cboFailure),
                                                                        Util.GetCondition(cboCause),
                                                                        Util.GetCondition(cboResolution)
                                                                       );
            if (!(string.Equals(sEqptLossFcrCheck, "Y")))
            {
                switch (sEqptLossFcrCheck)
                {
                    case "N1":
                        Util.MessageInfo("SFU3212"); //  현상을 선택해주세요
                        break;
                    case "N2":
                        Util.MessageInfo("SFU3213"); // 원인을 선택해주세요
                        break;
                    case "N3":
                        Util.MessageInfo("SFU3214"); // 조치를 선택해주세요
                        break;
                    default:
                        Util.MessageInfo("SFU9202"); // FCR 그룹이 등록되지 않은 내역입니다.(현상/원인/조치)
                        break;

                }
                return;
            }

            try
            {
                RQSTDT.Rows.Add(dr);

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true) //일괄등록이 하나라도 체크되어 있으면 Run 은 살린 상태로 개별 저장
                {
                    if (chkT.IsChecked == true)
                    {
                        dr["CHKT"] = "T";
                    }
                    else
                    {
                        dr["CHKT"] = "";
                    }

                    if (chkW.IsChecked == true)
                    {
                        dr["CHKW"] = "W";
                    }
                    else
                    {
                        dr["CHKW"] = "";
                    }

                    if (chkU.IsChecked == true)
                    {
                        dr["CHKU"] = "U";
                    }
                    else
                    {
                        dr["CHKU"] = "";
                    }

                    DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_EACH_V02", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    //DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS", "RQSTDT", "RSLTDT", RQSTDT);
                    new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_V02", "RQSTDT", "RSLTDT", RQSTDT);
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                if (dgDetail.Rows.Count != 0)
                {
                    dgDetail.ScrollIntoView(idx, 0);
                }

                Util.AlertInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //}
        }

        private void ApprovalProcess()
        {
            string eqptId = Util.GetCondition(cboEquipment);
            string wrkDate = Util.GetCondition(ldpDatePicker);
            string strtDttm = Util.GetCondition(txtStartHidn);
            string endDttm = Util.GetCondition(txtEndHidn);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();
            string lossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;

            //결재 요청 Popup 창 Open
            COM001_014_APPR_ASSIGN popup = new COM001_014_APPR_ASSIGN();
            popup.FrameOperation = FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.GetString();
                Parameters[1] = eqptId;
                Parameters[2] = wrkDate;
                Parameters[3] = strtDttm;
                Parameters[4] = endDttm;
                Parameters[5] = lossCode;
                Parameters[6] = lossDetlCode;
                Parameters[7] = lossNote;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(Popup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            COM001_014_APPR_ASSIGN window = sender as COM001_014_APPR_ASSIGN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private string VadliationERPEnd()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("WRKDATE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboArea.SelectedValue.ToString();
            dr["WRKDATE"] = Util.GetCondition(ldpDatePicker);
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_CLOSE", "RQSTDT", "RSLT", dt);

            if (result.Rows.Count != 0)
            {
                return Convert.ToString(result.Rows[0]["ERP_CLOSING_FLAG"]);
            }

            return "OPEN";
        }

        /// <summary>
        /// 삭제 더블클릭시
        /// 데이터 원복
        /// </summary>
        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            svMap.ScrollToVerticalOffset(10);
            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            if (dgDetail.CurrentRow != null)
            {
                if (RunSplit.Equals("Y"))
                {
                    if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("Y"))
                    {
                        if (!ValidateEqptLossAppr("CLICK"))
                        {
                            return;
                        }
                        //가동 데이터를 분할 한 데이터이므로 추가된 Loss가 초기화 됩니다. 그래도 삭제하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3205"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {

                            if (result.ToString().Equals("OK"))
                            {
                                try
                                {
                                    DataSet ds = new DataSet();
                                    DataTable dt = ds.Tables.Add("INDATA");

                                    dt.Columns.Add("STRT_DTTM", typeof(DateTime));
                                    dt.Columns.Add("END_DTTM", typeof(DateTime));
                                    dt.Columns.Add("EQPTID", typeof(string));
                                    dt.Columns.Add("WRK_DATE", typeof(string));
                                    dt.Columns.Add("AREAID", typeof(string));
                                    dt.Columns.Add("USERID", typeof(string));
                                    dt.Columns.Add("START_DTTM_YMDHMS", typeof(string));

                                    DataRow row = dt.NewRow();
                                    row["STRT_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["END_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                    row["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "WRK_DATE"));
                                    row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                                    row["USERID"] = LoginInfo.USERID;
                                    row["START_DTTM_YMDHMS"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                    dt.Rows.Add(row);

                                    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RUN_SPLT_RESET", "INDATA", null, ds);

                                    btnSearch_Click(null, null);

                                    if (dgDetail.GetRowCount() != 0)
                                    {
                                        dgDetail.ScrollIntoView(idx, 0);
                                    }


                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }
                        }
                        );
                    }
                }

                if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "CHECK_DELETE")).Equals("DELETE") && dgDetail.CurrentColumn != null && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("N")) //삭제 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }
                    //삭제하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("EQPTID", typeof(string));
                                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                                RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                                RQSTDT.Columns.Add("END_DTTM", typeof(string));
                                RQSTDT.Columns.Add("USERID", typeof(string));

                                DataRow dr = RQSTDT.NewRow();
                                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;

                                //if (dr["EQPTID"].Equals("") || dr["LOSS_CODE"].Equals("")) return;

                                RQSTDT.Rows.Add(dr);
                                DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_RESET", "RQSTDT", "RSLTDT", RQSTDT);

                                //UPDATE 처리후 재조회
                                btnSearch_Click(null, null);
                                if (dgDetail.GetRowCount() != 0)
                                {
                                    dgDetail.ScrollIntoView(idx, 0);
                                }


                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("SPLIT") && dgDetail.CurrentColumn != null) //분할 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }
                    //분할하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3120"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            COM001_014_SPLIT wndPopup = new COM001_014_SPLIT();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                //2025.07.04 김선영, SPLIT 파라메터 변경 : 설비LOSS 등록 화면과 동일하게 맞춤, START 
                                object[] Parameters = new object[6];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO"));
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                Parameters[4] = Convert.ToString(cboArea.SelectedValue);
                                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "TRANSACTION_SERIAL_NO"));

                                //object[] Parameters = new object[4];
                                //Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                //Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO"));
                                //Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                //Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                //2025.07.04 김선영, SPLIT 파라메터 변경 : 설비LOSS 등록 화면과 동일하게 맞춤, END 

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("MERGE") && dgDetail.CurrentColumn != null) //병합 더블클릭시에 실행
                {
                    if (!ValidateEqptLossAppr("CLICK"))
                    {
                        return;
                    }
                    //병합하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2876"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet dsData = new DataSet();

                                DataTable dtIn = dsData.Tables.Add("INDATA");
                                dtIn.Columns.Add("EQPTID", typeof(string));
                                dtIn.Columns.Add("FROM_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("TO_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("USERID", typeof(string));

                                DataRow row = null;
                                row = dtIn.NewRow();
                                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                row["FROM_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100;
                                row["TO_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100 + 99;
                                row["USERID"] = LoginInfo.USERID;

                                dtIn.Rows.Add(row);



                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_SPLIT_RESET", "INDATA", "OUTDATA", dsData);


                                if (Convert.ToInt16(dsRslt.Tables["OUTDATA"].Rows[0]["CNT"]) > 0)
                                {
                                    Util.AlertInfo("SFU1516");  //등록된 데이터를 지우고 병합해주세요.
                                    return;
                                }
                                else
                                {
                                    //UPDATE 처리후 재조회
                                    btnSearch_Click(null, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else //선택처리
                {
                    btnReset_Click(null, null);
                    setMapColor("A" + Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);

                }
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_014_SPLIT window = sender as COM001_014_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void wndPopup_Closed2(object sender, EventArgs e)
        {
            COM001_239_LOSS_SPLIT window = sender as COM001_239_LOSS_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        /// <summary>
        /// 상세 데이터 색변환하기
        /// </summary>
        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
                    string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
                    string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));

                    if (sCheck.Equals("DELETE"))
                    {
                        System.Drawing.Color color = GridBackColor.Color4;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                    else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
                    }
                    else
                    {
                        System.Drawing.Color color = GridBackColor.Color6;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)); ;
                    }

                }

                //link 색변경
                if (e.Cell.Column.Name != null)
                {
                    if (e.Cell.Column.Name.Equals("CHECK_DELETE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name.Equals("SPLIT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }

                // 비고 칼럼 사이즈
                if (e.Cell.Column.Name.Equals("txtNote"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                }

            }));

        }

        private void cboLossEqsgProc_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLossEqsgProc.SelectedValue.Equals("SELECT"))
            {
                string[] loss = cboLossEqsgProc.SelectedValue.ToString().Split('-');

                cboLoss.SelectedValue = loss[0];

                if (!loss[1].Equals(""))
                {
                    cboLossDetl.SelectedValue = loss[1];
                }
            }
        }

        #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        private void cboLossDetl_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 부동내용이 바뀌면 현상, 원인, 조치 전체 초기화
            if (!cboLossDetl.Text.Equals("SELECT"))
            {
                InitFCRCode();
            }
        }

        private void cboFailure_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            /*
             * <현상 변경>
             * 1. N/A 선택될 경우              : 현상 콤보박스 초기화 O
             *                                   원인, 조치 콤보박스 초기화 X
             * 2. N/A 가 아닌 값이 선택될 경우 : 현상 콤보박스 초기화 X
             *                                   원인 - 선택된 값이 없을 경우 현상, 조치 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             *                                   조치 - 선택된 값이 없을 경우 현상, 원인 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             */
            if (!cboLossDetl.SelectedValue.IsNullOrEmpty())
            {
                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboFailure.SelectedValue.IsNullOrEmpty())
                {
                    cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                    String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                    cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                }else
                {
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboCause_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLossDetl.SelectedValue.IsNullOrEmpty())
            {

                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboCause.SelectedValue.IsNullOrEmpty())
                {
                    cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                    String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                    cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                }else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboResolution_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLossDetl.SelectedValue.IsNullOrEmpty())
            {

                CommonCombo _combo = new CommonCombo();

                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                if (cboResolution.SelectedValue.IsNullOrEmpty())
                {
                    cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                    String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                    _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                    cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                }else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                }
            }
        }
        #endregion

        #region 2023.06.05 윤지해 CSR ID E20230330-001442 원인설비 변경 이벤트
        private void cboOccurEqpt_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // LOSS LV2 변경
            CommonCombo _combo = new CommonCombo();
            // PACK, 소형이 아니고 원인설비별 LOSS 등록 여부 체크
            if (occurEqptFlag.Equals("Y"))
            {
                if (!(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))   // 원인설비 선택했을 경우 : BM/PD만 LIST-UP
                {
                    string[] sFilterLoss = { Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboOccurEqpt.SelectedValue), string.Empty };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeOccurEqpt");
                }
                else // BM/PD 제외 LIST-UP
                {
                    string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), occurEqptFlag, string.Empty };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPart");
                }
            }

            // 최근등록 콤보박스 변경
            C1ComboBox[] cboLastLossParent = { cboEquipment };
            if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
            {
                string[] sFilterOccurEqpt = { occurEqptFlag };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent, sFilter: sFilterOccurEqpt, sCase: "cboLastLossPart");
            }
            else
            {
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);
            }

            // 동-라인-공정별 로스 매핑 콤보박스 변경
            if (occurEqptFlag.Equals("Y") && !(cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
            {
                string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue), occurEqptFlag };
                _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "cboLossEqsgProcPart");
            }
            else
            {
                string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
                _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
            }
        }
        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAreaId = cboArea.SelectedValue.ToString();
            dBaseDate = Util.LossDataUnalterable_BaseDate(sAreaId);
            if (dBaseDate != null
                && ldpDatePicker.SelectedDateTime < dBaseDate)
            {
                ldpDatePicker.SelectedDateTime = dBaseDate;
            }

            GetEqptLossApprAuth();
        }

        #endregion

        #region Mehod

        /// <summary>
        /// 색지도초기화
        /// </summary>
        private void ClearGrid()
        {
            try
            {
                foreach (Border _border in _grid.Children.OfType<Border>())
                {
                    _grid.UnregisterName(_border.Name);
                }

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다
                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_UNIT", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_UNIT", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게 
                dtBeforeList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN_UNIT", "RQSTDT", "RSLTDT", RQSTDT);//저장 버튼 클릭 시점에서 업데이트된 LOSS DATA와 비교할 DataTable
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 색지도 처리
        /// </summary>
        private void SelectProcess()
        {
            try
            {
                string sEqptID = Util.GetCondition(cboEquipment);
                string sEqptType = sEqptLevel.Equals("M") ? "A" : "M"; //chkMain.IsChecked.Equals(true)) ? "M" : "A";
                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);



                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();


                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sEqptID);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                //spdMList.ActiveSheet.RowCount = (hash_title.Count) + 1;

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정                    
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                //for (int i = 0; i < 1000; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);


                        _grid.Children.Add(_lable);
                    }

                    //spdMList.ActiveSheet.Cells[nRow, nCol].HorizontalAlignment = CellHorizontalAlignment.Left;

                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];
                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정


                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;


                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);

                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                        if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        {
                            //spdMList.ActiveSheet.RowCount = spdMList.ActiveSheet.RowCount + (hash_title.Count) + 1;
                        }
                    }

                }

                ////--- 위에서 시간 중복 Hastable 처리
                //foreach (DictionaryEntry de in hash_Merge)
                //{
                //    int nRow1 = int.Parse(de.Key.ToString());
                //    spdMList.ActiveSheet.SetRowMerge(nRow1, FarPoint.Win.Spread.Model.MergePolicy.Always);
                //    FarPoint.Win.LineBorder bevelbrdr = new FarPoint.Win.LineBorder(GridBackColor.Color3, 1);
                //    spdMList.ActiveSheet.Rows[nRow1].Border = bevelbrdr;
                //}

                //spdMList.ActiveSheet.Protect = true;
                //spdMList.ResumeLayout();

                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 부동 내역 조회
        /// - 시간 차이가 180초 이상 인 경우
        /// - OP Key-In 인 경우
        /// - LOSS CODE ( 38000)  자재교체인 경우
        /// 색깔 구분
        /// - 분홍색 (2)
        ///   : 180초 이상 
        ///   : 기준정보 기준시간 초과인 경우 ( 시작시간이 0 인  기준정보 )
        /// - 회색 (1)
        ///   : OP Key-In 
        ///   : 기준정보 기준시간 이내인 경우 ( 시작시간이 0 인  기준정보 )
        /// </summary>
        private void GetEqptLossDetailList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));       //2025.07.03 김선영 ESHG 수정,  MIN_SECONDS 파라메터 추가 

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["ASC"] = (bool)chkLossSort.IsChecked ? null : "Y";
                dr["REVERSE_CHECK"] = (bool)chkLossSort.IsChecked ? "Y" : null;
                dr["MIN_SECONDS"] = 180;            //2025.07.03 김선영 ESHG 수정, MIN_SECONDS 파라메터 추가 
                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL", "RQSTDT", "RSLTDT", RQSTDT);


                if (!cboShift.SelectedValue.ToString().Equals(""))
                {
                    DateTime dJobDate_st = new DateTime();
                    DateTime dJobDate_ed = new DateTime();

                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift_st = drShift[0]["SHFT_STRT_HMS"].ToString();
                        String sShift_ed = drShift[0]["SHFT_END_HMS"].ToString();

                        dJobDate_st = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_st.Substring(0, 2) + ":" + sShift_st.Substring(2, 2) + ":" + sShift_st.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        dJobDate_ed = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                        //작업조의 end시간이 기준시간 보다 작을때
                        if (TimeSpan.Parse(sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2)) < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
                        {
                            dJobDate_ed = DateTime.ParseExact(ldpDatePicker.SelectedDateTime.AddDays(1).ToString("yyyyMMdd") + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        }
                    }

                    try
                    {
                        RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").CopyToDataTable();
                        if (chkLossSort.IsChecked == true)
                        {
                            RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").Reverse().CopyToDataTable();
                        }

                    }
                    catch (Exception ex)
                    {
                        DataTable dt = new DataTable();
                        foreach (DataColumn col in RSLTDT.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.ColumnName));
                        }

                        RSLTDT = dt;
                    }

                }



                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);


                txtRequire.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtWriteEnd.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 시간 목록
        /// </summary>
        private DataSet GetEqptTimeList(string sJobDate, string sShiftCode)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = sJobDate;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0) { }
            if (Convert.ToString(result.Rows[0]["JOBDATE_YYYYMMDD"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return null;
            }




            DataSet ds = new DataSet();

            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                RQSTDT.Rows.Add(dr);

                dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT", "RQSTDT", "RSLTDT", RQSTDT);


                DataTable RSLTDT = new DataTable("RSLTDT");

                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();

                if (sShiftCode.Equals(""))
                {
                    // String sShift = dtShift.Compute("MIN(SHFT_STRT_HMS)", "").ToString();
                    String sShift = dtShift.Rows[0]["SHFT_STRT_HMS"].ToString();
                    if (sShift.Length > 0)
                    {
                        // dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {


                        dJobDate = DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }
                    iTerm = 20;
                    iIncrease = 20;
                }
                else
                {
                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift = drShift[0]["SHFT_STRT_HMS"].ToString();
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {
                        dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];//DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }

                    if (drShift[0]["SHFT_GR_CODE"].ToString().Equals(""))
                    {
                        Util.MessageValidation("SFU3442"); //  "해당 조의 교대 수가 없습니다.
                                                           // ds.Tables. // = null;

                        return ds;
                    }

                    iTerm = int.Parse(drShift[0]["SHFT_GR_CODE"].ToString()) * 10;
                    iIncrease = 10;
                }



                DataTable dtGetDate = new ClientProxy().ExecuteServiceSync("COR_SEL_GETDATE", null, "RSLTDT", null);

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));

                }//

                DataTable RSLTDT1 = RSLTDT.Select("STARTTIME <=" + Convert.ToDateTime(dtGetDate.Rows[0]["SYSTIME"]).ToString("yyyyMMddHHmmss"), "").CopyToDataTable();


                ds.Tables.Add(RSLTDT1);

                ds.Tables[0].TableName = "RSLTDT";
                return ds;
            }
            catch (Exception ex)
            {
                //---commMessage.Show(ex.Message);                
                return ds;
            }
        }

        /// <summary>
        /// 설비명 가져오기
        /// </summary>
        private DataTable GetEqptName(string sEqptID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE_UNIT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        /// <summary>
        /// 시간별 상태 목록 가져오기
        /// </summary>
        private DataTable GetEqptLossList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));



                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;

        }

        /// <summary>
        /// 최초 상태 가져오기
        /// </summary>
        private DataTable GetEqptLossFirstList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Hashtable hash_rs = new Hashtable();
                    for (int j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        hash_rs.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                    }
                    hash_return.Add(dt.Rows[i]["STARTTIME"].ToString(), hash_rs);
                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                hash_return = null;
            }
            return hash_return;
        }

        /// <summary>
        /// 색지도 클릭시 색상 초기화
        /// </summary>
        private void resetMapColor()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            DataRow[] dtRow = dtMainList.Select("CHK = '1'", "");

            foreach (DataRow dr in dtRow)
            {
                dr["CHK"] = "0";
            }

        }

        /// <summary>
        /// 색지도 클릭시 색깔 칠하기
        /// </summary>
        private void setMapColor(String sEqptid, String sTime, String sType, C1.WPF.DataGrid.DataGridRow row = null)
        {

            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END > '" + sTime + "' and EQPTID = '" + sEqptid.Substring(1) +"'");
            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");



            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            try
            {
                if (dtRow.Length > 0)
                {
                    dtRow[0]["CHK"] = "1";

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                    {
                        if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                        }
                        else
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRow[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                        }
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

                        if (row != null)
                        {
                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            CommonCombo _combo = new CommonCombo();

                            // 2023.05.23 윤지해 CSR ID E20230330-001442 procId 전역변수로 변경 로직 변경
                            // 항상 존재하는 값
                            string lossCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            string lossDetlCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));

                            // Optional 값
                            string failCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            string causeCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            string resolCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            #endregion
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME")).Equals(""))
                            {
                                txtEqptName.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                            {
                                cboOccurEqpt.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                            }

                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            //{
                            //    cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            //{
                            //    cboLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                            //{
                            //    cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                            //{
                            //    cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                            //{
                            //    cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            //}
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            {
                                cboLoss.SelectedValue = lossCode;
                            }

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            {
                                cboLossDetl.SelectedValue = lossDetlCode;
                            }

                            // 2023.06.05 윤지해 CSR ID E20230330-001442 원인설비, FCR 변경
                            string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                            // 현상
                            String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");

                            // 원인
                            String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");

                            // 조치
                            String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                            _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                            #endregion

                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                            {
                                new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                                //txtLossNote.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                            }
                        }

                    }

                    Border borderS = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("S" + sEqptid.Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("S" + sEqptid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        //  dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;  (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString();
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("S" + sEqptid.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    Hashtable hashStart = borderS.Tag as Hashtable;
                    Hashtable hashEnd = borderE.Tag as Hashtable;

                    DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                    //색칠해야할 셀갯수 =  row 차이 / (설비갯수 + 시간디스플레이) * 컬럼수 + 종료컬럼 - 시작컬럼
                    int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));


                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = new SolidColorBrush(Colors.Blue);
                    }

                    //마지막 칸 정리
                    Border borderEndMinusOne = _grid.FindName("S" + sEqptid.Replace("-", "_") + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                    Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                    if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                    {
                        borderE.Background = new SolidColorBrush(Colors.Blue);
                    }

                    int iRow = Grid.GetRow(borderS);



                    txtEqptName.Text = (GetEqptName(sEqptid.Substring(1)).Rows[0]["EQPTNAME"]).ToString();
                    if (row == null) 
                    {
                        cboOccurEqpt.SelectedValue = sEqptid.Substring(1);
                    }

                    if (sType.Equals("LIST"))
                    {
                        svMap.ScrollToVerticalOffset((15 * iRow - 8));
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        #region 2023.05.23 윤지해 CSR ID E20230330-001442 원인설비 사용조건 확인
        private void ChkProcessEquipmentSegment()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("CAUSE_EQPT_LOSS_MNGT_FLAG"))
                {
                    if (Util.NVC(dtResult.Rows[0]["CAUSE_EQPT_LOSS_MNGT_FLAG"]).Equals("Y"))
                    {
                        isCauseEquipment = true;
                    }
                    else
                    {
                        isCauseEquipment = false;
                    }
                }
                else
                {
                    isCauseEquipment = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion

        #region [Biz]

        #region [### 설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegment;
                dr["PROCID"] = Util.GetCondition(cboProcess);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";


                liProcId = new List<string>();

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    liProcId.Add(dtResult.Rows[i]["PROCID"].ToString());
                }

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();


                cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                if (cboEquipment.SelectedIndex < 0)
                    cboEquipment.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Shift 정보 가져오기]
        private void SetShift()
        {
            try
            {
                //2025.07.03 김선영 ESHG 수정, 작업조 조회 MES2.0 DA로 변경, START 
                const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cboShift.DisplayMemberPath = "CBO_NAME";
                cboShift.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow["CBO_CODE"] = "";
                newRow["CBO_NAME"] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);
                cboShift.ItemsSource = dtResult.Copy().AsDataView();
                cboShift.SelectedIndex = 0;


                //string sArea = Util.GetCondition(cboArea);
                //if (string.IsNullOrWhiteSpace(sArea))
                //    return;

                //string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                //if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                //    return;

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                ////RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("FROMDATE", typeof(string));
                //RQSTDT.Columns.Add("TODATE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = sArea;
                //dr["EQSGID"] = sEquipmentSegment;
                //dr["FROMDATE"] = Util.GetCondition(ldpDatePicker);
                //dr["TODATE"] = Util.GetCondition(ldpDatePicker);
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                //cboShift.DisplayMemberPath = "SHFT_NAME";
                //cboShift.SelectedValuePath = "SHFT_ID";

                //DataRow drIns = dtResult.NewRow();
                //drIns["SHFT_NAME"] = "-ALL-";
                //drIns["SHFT_ID"] = "";
                //dtResult.Rows.InsertAt(drIns, 0);

                //cboShift.ItemsSource = dtResult.Copy().AsDataView();

                //if (cboShift.SelectedIndex < 0)
                //    cboShift.SelectedIndex = 0;

                //2025.07.03 김선영 ESHG 수정, 작업조 조회 MES2.0 DA로 변경, END 

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion


        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);


        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType.Equals(""))
                        {
                            color = System.Drawing.Color.White;
                        }
                        else
                        {
                            //color = System.Drawing.Color.White;
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                color = System.Drawing.Color.White;
            }
            return color;
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        private void cboLastLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLastLoss.SelectedValue.Equals("SELECT"))
            {
                string[] sLastLoss = cboLastLoss.SelectedValue.ToString().Split('-');

                cboLoss.SelectedValue = sLastLoss[0];

                if (!sLastLoss[1].Equals(""))
                {
                    cboLossDetl.SelectedValue = sLastLoss[1];
                }
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

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
            Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
            Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);

            IndataTable.Rows.Add(Indata);

            string sShiftID = string.Empty;
            string sShiftNM = string.Empty;
            string sWorkerID = string.Empty;
            string sWorkerNM = string.Empty;

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt.Rows.Count > 0)
                {
                    sShiftID = Util.NVC(dtRslt.Rows[0]["SHFT_ID"]);
                    sShiftNM = Util.NVC(dtRslt.Rows[0]["SHFT_NAME"]);
                    sWorkerID = Util.NVC(dtRslt.Rows[0]["WRK_USERID"]);
                    sWorkerNM = Util.NVC(dtRslt.Rows[0]["WRK_USERNAME"]);
                }
            }
            catch
            {
            }

            if (string.Equals(GetAreaType(), "E"))
            {
                COM001_014_EQPCOMMNET wndEqpComment = new COM001.COM001_014_EQPCOMMNET();
                wndEqpComment.FrameOperation = FrameOperation;

                //if (wndEqpComment != null)
                //{
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = cboEquipment.Text;
                Parameters[6] = sShiftNM; // 작업조명
                Parameters[7] = sShiftID; // 작업조코드
                Parameters[8] = sWorkerNM; // 작업자명
                Parameters[9] = sWorkerID; // 작업자 ID
                                           //C1WindowExtension.SetParameters(wndEqpComment, Parameters);
                this.FrameOperation.OpenMenu("SFU010130090", true, Parameters);

                //wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                //_grid.Children.Add(wndEqpComment);
                //wndEqpComment.BringToFront();
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT();
                wndEqpComment.FrameOperation = FrameOperation;

                if (wndEqpComment != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                    Parameters[3] = "";
                    Parameters[4] = "";
                    Parameters[5] = cboEquipment.Text;
                    Parameters[6] = sShiftNM; // 작업조명
                    Parameters[7] = sShiftID; // 작업조코드
                    Parameters[8] = sWorkerNM; // 작업자명
                    Parameters[9] = sWorkerID; // 작업자 ID
                    C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                    wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                    //_grid.Children.Add(wndEqpComment);
                    wndEqpComment.BringToFront();
                }
            }

        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            if (string.Equals(GetAreaType(), "E"))
            {
                //COM001_014_EQPCOMMNET window = sender as COM001_014_EQPCOMMNET;
                //if (window.DialogResult == MessageBoxResult.OK)
                //{
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                }
            }

        }

        private void btnRegiFcr_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFCR())
            {
                return;
            }
            COM001_014_FCR wndFCR = new COM001_014_FCR();
            wndFCR.FrameOperation = FrameOperation;

            if (wndFCR != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = Convert.ToString(cboEquipmentSegment.SelectedValue);

                C1WindowExtension.SetParameters(wndFCR, Parameters);

                wndFCR.Closed += new EventHandler(wndFCR_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCR.ShowModal()));
                wndFCR.BringToFront();

            }

        }

        private void wndFCR_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR window = sender as COM001_014_FCR;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnSearchLossCode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFCR())
            {
                return;
            }

            COM001_014_FCR_LIST wndFCRList = new COM001_014_FCR_LIST();
            wndFCRList.FrameOperation = FrameOperation;

            if (wndFCRList != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = Convert.ToString(cboEquipmentSegment.SelectedValue);

                C1WindowExtension.SetParameters(wndFCRList, Parameters);

                wndFCRList.Closed += new EventHandler(wndFCRList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCRList.ShowModal()));
                wndFCRList.BringToFront();

            }

        }

        private void wndFCRList_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR_LIST window = sender as COM001_014_FCR_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                #region 2023.05.23 윤지해 CSR ID E20230330-001442 FCR 그룹 선택 시 FCR 콤보박스 리스트 변경
                CommonCombo _combo = new CommonCombo();

                // 항상 존재하는 값
                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();

                // Optional 값
                string failCode = window.F_CODE;
                string causeCode = window.C_CODE;
                string resolCode = window.R_CODE;
                string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                // 현상
                cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

                // 원인
                cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                // 조치
                cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                #endregion

                txtFCRCode.Text = window.FCR_GR_CODE;
            }
        }

        private bool ValidateFCR()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.Equals("") || cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.Equals("") || cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3207"); //공정을 선택해주세요
                return false;
            }
            return true;
        }

        private void txtLossCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchFCRCode();
            }
        }

        private void SearchFCRCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FCR_GR_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["FCR_GR_CODE"] = txtFCRCode.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_FCR_GR", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3208"); //해당 FCR그룹코드가 없습니다. 등록해주세요

                    txtFCRCode.Text = string.Empty;
                    cboFailure.SelectedIndex = 0;
                    cboCause.SelectedIndex = 0;
                    cboResolution.SelectedIndex = 0;

                    return;
                }

                cboFailure.SelectedValue = result.Rows[0]["F_CODE"];
                cboCause.SelectedValue = result.Rows[0]["C_CODE"];
                cboResolution.SelectedValue = result.Rows[0]["R_CODE"];
            }
            catch (Exception ex)
            {

            }
        }

        //운영설비도 Split되는지 확인
        private void SelectLossRunArea()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "LOSS_RUN_AREA";
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_LOSSRUNAREA", "INDATA", "RSLT", dt);

                RunSplit = result.Rows.Count == 0 ? "N" : "Y";

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFCR())
            {
                return;
            }
            // 2023.07.18 윤지해 CSR ID E20230703-000158 - COM001_014_LOSS_DETL_FCR로 변경
            //COM001_014_LOSS_DETL wndLossDetl = new COM001_014_LOSS_DETL();
            COM001_014_LOSS_DETL_FCR wndLossDetl = new COM001_014_LOSS_DETL_FCR();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                #region 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(cboArea.SelectedValue);
                Parameters[1] = Convert.ToString(cboProcess.SelectedValue);
                Parameters[2] = Convert.ToString(cboEquipment.SelectedValue);
                // 2022.08.23 YJH 자동차/ESS 조립 공정, PD(품질부동,EL014)만 적용
                //if (cboLoss.SelectedValue != null && string.Equals(GetAreaType(), "A") && (LoginInfo.CFG_AREA_ID.ToString().StartsWith("A") || LoginInfo.CFG_AREA_ID.ToString().StartsWith("S")))
                //{
                //    Parameters[3] = cboLoss.SelectedValue.ToString().Equals("EL014") ? cboLoss.SelectedValue.ToString() : "";
                //}
                //else
                //{
                //    Parameters[3] = "";
                //}
                #endregion
                // 2023.06.05 윤지해 CSR ID E20230330-001442 
                Parameters[3] = (cboLoss.SelectedValue.IsNullOrEmpty() || cboLoss.SelectedValue.ToString().Equals("SELECT")) ? "" : cboLoss.SelectedValue.ToString();
                Parameters[4] = occurEqptFlag;
                Parameters[5] = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            // 2023.07.18 윤지해 CSR ID E20230703-000158 - COM001_014_LOSS_DETL_FCR로 변경
            //COM001_014_LOSS_DETL window = sender as COM001_014_LOSS_DETL;
            COM001_014_LOSS_DETL_FCR window = sender as COM001_014_LOSS_DETL_FCR;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                cboLossDetl.SelectedValue = window._LOSS_DETL_CODE;
                cboFailure.SelectedValue = window._FAIL_CODE;
                cboCause.SelectedValue = window._CAUSE_CODE;
                cboResolution.SelectedValue = window._RESOL_CODE;
            }
        }

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(0);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);

            grUp.RowDefinitions[1].Height = new GridLength(0);
        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(300);
            grUp.RowDefinitions[1].Height = new GridLength(8);
            grUp.RowDefinitions[2].Height = GridLength.Auto;
            grUp.RowDefinitions[3].Height = new GridLength(8);
            grUp.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        }

        private void btnTotalSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgDetail.GetRowCount() > 0 && !ValidateEqptLossAppr("TOTAL"))
            {
                return;
            }

            if (cboLoss.Text.ToString().Equals("-SELECT-") && cboLossDetl.Text.ToString().Equals("-SELECT-") && new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Equals(""))
            {
                Util.MessageValidation("SFU3485"); //저장내역을 입력해주세요
                return;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") == 1)
            {
                Util.MessageValidation("SFU3487"); //일괄등록의 경우 한개 이상의 부동내역을 선택해주세요
                return;
            }

            if (new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim().Length > 1000)
            {
                Util.MessageValidation("SFU5182");  //비고는 최대 1000자 까지 가능합니다.
                rtbLossNote.Focus();
                return;
            }

            ValidateNonRegisterLoss("TOTAL");

            if (!SaveDataChk(GetPreLossSeqnoForSaveALL(),"ALL"))
            {
                Util.MessageInfo("SUF9018"); // 업데이트된 LOSS DATA가 존재합니다.  화면을 다시 조회해주세요.
                return;
            }

            if (bUseEqptLossAppr)
            {
                // 당일 수정건이 아닌 경우
                if (ValidationChkDDay().Equals("AUTH_ONLY"))
                {
                    string lossCode = Util.GetCondition(cboLoss);
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOSS_CODE"] = lossCode;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtUpprLoss = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                    if (dtUpprLoss.Rows.Count != 0)
                    {
                        string upprLossCode = Convert.ToString(dtUpprLoss.Rows[0]["UPPR_LOSS_CODE"]);
                        // 수정할 Loss가 비조업인 경우
                        if (upprLossCode.Equals("10000"))
                        {
                            TotalApprovalProcess();
                        }
                        else
                        {
                            this.TotalSaveProcess();
                        }
                    }
                }
                else
                {
                    this.TotalSaveProcess();
                }
            }
            else
            {
                this.TotalSaveProcess();
            }
        }

        private void TotalSaveProcess()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

            //해당Loss를 일괄로 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {

                    DataSet ds = new DataSet();
                    DataTable RQSTDT = ds.Tables.Add("INDATA");
                    //RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                    //RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                    //RQSTDT.Columns.Add("END_DTTM", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
                    RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CHKW", typeof(string));
                    RQSTDT.Columns.Add("CHKT", typeof(string));
                    RQSTDT.Columns.Add("CHKU", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    RQSTDT.Columns.Add("PRE_LOSS_SEQNO", typeof(string));
                    RQSTDT.Columns.Add("SAVETYPE", typeof(string));
                    DataRow dr = null;

                    #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석처리
                    //string msg = "";

                    //if (!(new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Equals("")) && cboLoss.Text.Equals("-SELECT-") && cboLossDetl.Text.Equals("-SELECT-"))
                    //{
                    //    msg = "SFU3441";

                    //    for (int i = 0; i < dgDetail.GetRowCount(); i++)
                    //    {
                    //        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                    //        {
                    //            dr = RQSTDT.NewRow();

                    //            dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                    //            if (dr["EQPTID"].Equals("")) return;
                    //            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    //            dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                    //            dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    //            dr["LOSS_CODE"] = null;
                    //            dr["LOSS_DETL_CODE"] = null;
                    //            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
                    //            dr["SYMP_CODE"] = null;
                    //            dr["CAUSE_CODE"] = null;
                    //            dr["REPAIR_CODE"] = null;
                    //            dr["OCCR_EQPTID"] = null;
                    //            dr["USERID"] = LoginInfo.USERID;
                    //            RQSTDT.Rows.Add(dr);
                    //        }
                    //    }



                    //    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL", "INDATA", null, ds);
                    //    btnSearch_Click(null, null);
                    //    chkT.IsChecked = false;
                    //    chkW.IsChecked = false;
                    //    chkU.IsChecked = false;

                    //    dgDetail.ScrollIntoView(idx, 0);

                    //    Util.MessageInfo("SFU1270");  //저장되었습니다.

                    //}
                    //else
                    //{
                    //dr = null;
                    #endregion

                    //for (int i = 0; i < dgDetail.GetRowCount(); i++)
                    //{
                    //    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                    //    {
                            dr = RQSTDT.NewRow();

                            dr["AREAID"] = _areaid;
                            dr["EQPTID"] = _eqptid;

                            if (dr["EQPTID"].Equals("")) return;
                            dr["WRK_DATE"] = _wrk_date;
                            //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                            //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                            dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
                            if (dr["LOSS_CODE"].Equals("")) return;

                            dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                            if (dr["LOSS_DETL_CODE"].Equals(""))
                            {
                                if (cboLossDetl.Items.Count > 1)
                                {
                                    // 부동내용을 입력하세요.
                                    Util.MessageValidation("SFU3631");
                                    return;
                                }
                            }

                            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
                            dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                            dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                            dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                            dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
                            dr["USERID"] = LoginInfo.USERID;
                            dr["PRE_LOSS_SEQNO"] = GetPreLossSeqnoForSaveALL();
                            dr["SAVETYPE"] = "ALL";

                    RQSTDT.Rows.Add(dr);
                     //   }
                   // }

                    //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
                    //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
                    string sEqptLossFcrCheck = GetprodEqptLossFcrGroupChkResult(Util.GetCondition(cboEquipment),  // 메인설비
                                                                                Util.GetCondition(cboOccurEqpt), // 원인설비
                                                                                Util.GetCondition(cboLossDetl),
                                                                                Util.GetCondition(cboLoss),
                                                                                Util.GetCondition(cboFailure),
                                                                                Util.GetCondition(cboCause),
                                                                                Util.GetCondition(cboResolution)
                                                                               );
                    if (!(string.Equals(sEqptLossFcrCheck, "Y")))
                    {
                        switch (sEqptLossFcrCheck)
                        {
                            case "N1":
                                Util.MessageInfo("SFU3212"); //  현상을 선택해주세요
                                break;
                            case "N2":
                                Util.MessageInfo("SFU3213"); // 원인을 선택해주세요
                                break;
                            case "N3":
                                Util.MessageInfo("SFU3214"); // 조치를 선택해주세요
                                break;
                            default:
                                Util.MessageInfo("SFU9202"); // FCR 그룹이 등록되지 않은 내역입니다.(현상/원인/조치)
                                break;

                        }
                        return;
                    }

                    try
                    {

                        if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                        {
                            Util.MessageValidation("SFU3489");//개별등록일 경우 일괄저장 기능 사용 불가
                            return;

                        }
                        else
                        {
                            new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL_V03", "INDATA", null, ds);
                        }

                        //UPDATE 처리후 재조회
                        btnSearch_Click(null, null);
                        chkT.IsChecked = false;
                        chkW.IsChecked = false;
                        chkU.IsChecked = false;

                        dgDetail.ScrollIntoView(idx, 0);

                        Util.MessageInfo("SFU1270");  //저장되었습니다.
                    }

                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    //}
                }
            });
        }
        private string GetPreLossSeqnoForSaveALL()
        {
            string pre_loss_seqno = string.Empty;

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (i != dgDetail.GetRowCount() - 1)
                    {
                        pre_loss_seqno += Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO")) + ",";
                    }
                    else
                    {
                        pre_loss_seqno += Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    }
                }
            }

            return pre_loss_seqno;
        }

        private void TotalApprovalProcess()
        {
            string wrkDate = Util.GetCondition(ldpDatePicker);
            string strtDttm = Util.GetCondition(txtStartHidn);
            string endDttm = Util.GetCondition(txtEndHidn);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = cboLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLossDetl.SelectedValue.ToString();
            string lossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;

            DataTable dtLossData = new DataTable();
            dtLossData.Columns.Add("EQPTID", typeof(string));
            dtLossData.Columns.Add("STRT_DTTM", typeof(string));
            dtLossData.Columns.Add("END_DTTM", typeof(string));
            dtLossData.Columns.Add("WRK_DATE", typeof(string));
            dtLossData.Columns.Add("LOSS_SEQNO", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_CODE", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_DETL_CODE", typeof(string));
            dtLossData.Columns.Add("APPR_REQ_LOSS_CNTT", typeof(string));
            dtLossData.Columns.Add("USERID", typeof(string));
            dtLossData.Columns.Add("APPR_USERID", typeof(string));
            dtLossData.Columns.Add("LOSS_CODE", typeof(string));
            dtLossData.Columns.Add("LOSS_DETL_CODE", typeof(string));
            dtLossData.Columns.Add("LOTID", typeof(string));
            dtLossData.Columns.Add("TRBL_CODE", typeof(string));
            dtLossData.Columns.Add("EIOSTAT", typeof(string));

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = dtLossData.NewRow();
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EQPTID"));
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    dr["WRK_DATE"] = wrkDate;
                    dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    dr["APPR_REQ_LOSS_CODE"] = lossCode;
                    dr["APPR_REQ_LOSS_DETL_CODE"] = lossDetlCode;
                    dr["APPR_REQ_LOSS_CNTT"] = lossNote;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["APPR_USERID"] = string.Empty;
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_CODE"));
                    dr["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_DETL_CODE"));
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    dr["TRBL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "TRBL_CODE"));
                    dr["EIOSTAT"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EIOSTAT"));
                    dtLossData.Rows.Add(dr);
                }
            }

            //결재 요청 Popup 창 Open
            COM001_014_TOTAL_APPR_ASSIGN popup = new COM001_014_TOTAL_APPR_ASSIGN();
            popup.FrameOperation = FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.GetString();
                Parameters[1] = dtLossData;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(Popup_Closed2);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void Popup_Closed2(object sender, EventArgs e)
        {
            COM001_014_TOTAL_APPR_ASSIGN window = sender as COM001_014_TOTAL_APPR_ASSIGN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void ValidateNonRegisterLoss(string saveType)
        {
            try
            {
                int idx = -1;
                if (saveType.Equals("TOTAL"))
                {
                    //제일 마지막에 클릭한 index찾기
                    for (int i = dgDetail.GetRowCount() - 1; i > 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            idx = i;
                            break;
                        }
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) : ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + txtStart.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSETAIL_VALID", "RQSTDT", "RSLT", dt);

                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU3515", Convert.ToString(result.Rows[0]["STRT_DTTM"]));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string ValidationChkDDay()
        {
            string bDDay = string.Empty;
            DateTime dtNowDay = DateTime.ParseExact(GetNowDate(), "yyyyMMdd", null);
            DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);

            if ((dtNowDay - dtSearchDay).TotalDays >= 0 && (dtNowDay - dtSearchDay).TotalDays < Convert.ToDouble(strAttr1))
            {
                bDDay = "ALL";
            }
            else if ((dtNowDay - dtSearchDay).TotalDays >= Convert.ToDouble(strAttr1) && (dtNowDay - dtSearchDay).TotalDays <= Convert.ToDouble(strAttr2))
            {
                bDDay = "AUTH_ONLY";
            }
            else
            {
                bDDay = "NO_REG";
            }

            return bDDay;
        }

        private bool ValidateEqptLossAppr(string validateType)
        {
            // 설비 LOSS 수정 화면 추가에 따른 Validation 추가, 설비 Loss 수정 화면을 사용할 경우 확인
            if (bUseEqptLossAppr)
            {
                DataTable RQSTDT;
                DataRow dr;
                DataTable result;

                //sSearchDay와 현재시간이 D+1 일 경우 Validation(비조업) 2023.07.19 김대현
                if (ValidationChkDDay().Equals("NO_REG"))
                {
                    string strParam = (Convert.ToDouble(strAttr2) + 1).ToString();
                    Util.MessageValidation("SFU5180", ObjectDic.Instance.GetObjectName(strParam));  // strAttr2 + 일이전 설비 Loss는 등록 불가합니다.
                    return false;
                }

                if (ValidationChkDDay().Equals("AUTH_ONLY") && LoginInfo.USERTYPE.Equals("P"))
                {
                    Util.MessageValidation("SFU5179"); // 공용 PC 사용자권한으로는 Loss 등록이 불가합니다. \n개인권한 사용자로 로그인하여 등록해주시기 바랍니다. 
                    return false;
                }

                // 선택한 Loss건이 비조업에 해당하면 설비 Loss 수정 화면 사용 여부 확인하여 사용중이면 return (저장할 경우에만 체크)
                //if (ValidationChkDDay().Equals("AUTH_ONLY"))
                //{
                //    if (validateType.Equals("SAVE") || validateType.Equals("TOTAL"))
                //    {
                //        string lossCode = Util.GetCondition(cboLoss);
                //        RQSTDT = new DataTable();
                //        RQSTDT.Columns.Add("LOSS_CODE", typeof(string));

                //        dr = RQSTDT.NewRow();
                //        dr["LOSS_CODE"] = lossCode;
                //        RQSTDT.Rows.Add(dr);

                //        result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_LOSSCODE_CHK_UPPR_LOSE_CODE", "RQSTDT", "RSLT", RQSTDT);
                //        if (result.Rows.Count != 0)
                //        {
                //            string upprLossCode = Convert.ToString(result.Rows[0]["UPPR_LOSS_CODE"]);
                //            if (upprLossCode.Equals("10000"))
                //            {
                //                Util.MessageValidation("SFU5177"); // 비조업에 해당하는 Loss 분류는 '설비 Loss 수정' 화면을 통해 수정가능합니다. MSG 수정
                //                return false;
                //            }
                //        }
                //    }
                //}

                // 선택한 Loss 건 중 승인 대기중인 건이 있으면 return
                // 저장(SAVE), 일괄저장(TOTAL), RowMouseDoubleClick(CLICK), Split(SPLIT)
                dr = null;
                result = null;

                RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOSS_SEQNO", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("END_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("APPR_STAT", typeof(string));

                switch (validateType)
                {
                    case "SAVE":
                        if (string.IsNullOrEmpty(txtStartHidn.Text) && string.IsNullOrEmpty(txtEndHidn.Text))
                        {
                            Util.MessageValidation("SFU3538");  //선택된 데이터가 없습니다
                            return false;
                        }
                        dr = RQSTDT.NewRow();
                        dr["EQPTID"] = Util.GetCondition(cboEquipment);
                        dr["WRK_DATE"] = sSearchDay;
                        dr["STRT_DTTM"] = DateTime.ParseExact(txtStartHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["END_DTTM"] = DateTime.ParseExact(txtEndHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                    case "TOTAL":

                        if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
                        {
                            Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                            return false;
                        }

                        for (int i = 0; i < dgDetail.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dr = RQSTDT.NewRow();
                                dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EQPTID"));
                                dr["WRK_DATE"] = sSearchDay;
                                //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "STRT_DTTM"));
                                //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "END_DTTM"));
                                dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                                dr["APPR_STAT"] = "W";
                                RQSTDT.Rows.Add(dr);
                            }
                        }
                        break;
                    case "SPLIT":
                        dr = RQSTDT.NewRow();
                        dr["EQPTID"] = Util.GetCondition(cboEquipment);
                        dr["WRK_DATE"] = sSearchDay;
                        dr["STRT_DTTM"] = DateTime.ParseExact(txtStartHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["END_DTTM"] = DateTime.ParseExact(txtEndHidn.Text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                    case "CLICK":
                        dr = RQSTDT.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "EQPTID"));
                        dr["WRK_DATE"] = sSearchDay;
                        //dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "STRT_DTTM"));
                        //dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "END_DTTM"));
                        dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "PRE_LOSS_SEQNO"));
                        dr["APPR_STAT"] = "W";
                        RQSTDT.Rows.Add(dr);

                        break;
                }

                result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_CHK_APPR", "RQSTDT", "RSLT", RQSTDT);
                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU5176"); // 설비 LOSS 수정을 통한 승인 대기 중인 설비 LOSS 건이 있습니다.
                    return false;
                }
            }


            return true;
        }

        private void GetEqptLossApprAuth()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = Util.NVC(cboArea.SelectedValue);
                drRQSTDT["COM_TYPE_CODE"] = "EQPT_LOSS_CHG_APPR_USE_SYSTEM";    // 해당 동, 시스템의 '설비 Loss 수정' 화면 사용 여부 확인
                drRQSTDT["COM_CODE"] = LoginInfo.SYSID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (dtRSLTDT != null && dtRSLTDT.Rows.Count > 0)
                {
                    bUseEqptLossAppr = true;

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR1"])))
                    {
                        strAttr1 = Util.NVC(dtRSLTDT.Rows[0]["ATTR1"]);
                    }
                    else
                    {
                        strAttr1 = "1";
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR2"])))
                    {
                        strAttr2 = Util.NVC(dtRSLTDT.Rows[0]["ATTR2"]);
                    }
                    else
                    {
                        strAttr2 = "7";
                    }
                }
                else
                {
                    bUseEqptLossAppr = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetNowDate()
        {
            string nowDate = string.Empty;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

            nowDate = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

            return nowDate;
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }

        private void chkLossSort_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }

        }

        private void chkLossSort_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }
        }

        // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        private void cboLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLoss.Text.Equals("-SELECT-"))
            {
                //부동코드
                // 2023.06.05 윤지해 CSR ID E20230330-001442 부동내용 콤보박스 세팅 로직 변경
                CommonCombo _combo = new CommonCombo();

                string occurEqptValue = (string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboEquipment };
                string[] sFilter = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
                //_combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);
                string[] sFilter3 = { Util.GetCondition(cboLoss), cboEquipment.SelectedValue.ToString(), occurEqptValue };
                if (occurEqptFlag.Equals("Y") && !(string.IsNullOrEmpty(cboOccurEqpt.Text) || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")))
                {
                    _combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "cboOccrLossDetl");
                }
                else
                {
                    _combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);
                }

                InitFCRCode();
            }
        }

        //[E20230601-001177] [전극/조립MES팀] 설비Loss 등록 시, FCR 필수 입력 Validation 요청의 건
        //[E20230707-001616] BM/PD LOSS FCR코드 벨리데이션 요청 件
        private string GetprodEqptLossFcrGroupChkResult(string sMainEqptID, string sEqptID, string sLossDetlCode, string sLossCode, string sSympCode, string sCauseCode, string sRepairCode)
        {
            string sChkResult = "N";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MAIN_EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MAIN_EQPTID"] = sMainEqptID;  // 메인설비
                dr["EQPTID"]      = sEqptID;      // 원인설비
                dr["LOSS_DETL_CODE"] = sLossDetlCode;
                dr["LOSS_CODE"] = sLossCode;
                dr["SYMP_CODE"] = sSympCode;
                dr["CAUSE_CODE"] = sCauseCode;
                dr["REPAIR_CODE"] = sRepairCode;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_FCR_MAPP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sChkResult = Util.NVC(dtResult.Rows[0][0].ToString());
                }
                else
                {
                    sChkResult = "";
                }

                return sChkResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return "";
            }
        }

        private bool SaveDataChk(string prelossseqno, string type = null)
        {
            //UPDATE하려는 LOSS DATA에 변경된 이력이 있는지 체크
            string _endnullseqno = string.Empty;
            string eqptid = string.Empty;
            string _prelossseqno = string.Empty;
            DataTable dtAfterList = new DataTable();

            eqptid = type == "SAVE" ? dtMainList.Select("CHK = 1").Count() == 0 ? _eqptid : (dtMainList.Select("CHK = 1")[0]["EQPTID"]).ToString() : _eqptid;

            _prelossseqno = prelossseqno;

            if (string.IsNullOrEmpty(_prelossseqno))
                return true;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQPTID"] = eqptid;
            dr["WRK_DATE"] = _wrk_date;

            RQSTDT.Rows.Add(dr);

            dtAfterList = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDATA_RUN_UNIT", "RQSTDT", "RSLTDT", RQSTDT);

            DataRow[] a = dtBeforeList.Select("EQPTID = " + "'" + eqptid + "'" + "AND PRE_LOSS_SEQNO IN (" + _prelossseqno + ")" + "AND END_TIME IS NULL");
            if (a.Count() != 0)
            {
                _endnullseqno = a[0]["PRE_LOSS_SEQNO"].ToString();
            }

            object[] oldloss = !string.IsNullOrEmpty(_endnullseqno) ? dtBeforeList.Select("PRE_LOSS_SEQNO IN (" + _prelossseqno + ")" + "AND PRE_LOSS_SEQNO <>" + _endnullseqno + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["UPDDTTM"]).ToArray() :
                                                                      dtBeforeList.Select("PRE_LOSS_SEQNO IN (" + _prelossseqno + ")" + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["UPDDTTM"]).ToArray();
            object[] newloss = !string.IsNullOrEmpty(_endnullseqno) ? dtAfterList.Select("PRE_LOSS_SEQNO IN (" + _prelossseqno + ")" + "AND PRE_LOSS_SEQNO <>" + _endnullseqno + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["UPDDTTM"]).ToArray() :
                                                                      dtAfterList.Select("PRE_LOSS_SEQNO IN (" + _prelossseqno + ")" + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["UPDDTTM"]).ToArray();
            object[] oldendnullloss = !string.IsNullOrEmpty(_endnullseqno) ? dtBeforeList.Select("PRE_LOSS_SEQNO =" + _endnullseqno + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["PRE_LOSS_SEQNO"]).ToArray() : null;
            object[] newendnullloss = !string.IsNullOrEmpty(_endnullseqno) ? dtAfterList.Select("PRE_LOSS_SEQNO =" + _endnullseqno + "AND EQPTID =" + "'" + eqptid + "'").Select(x => x["PRE_LOSS_SEQNO"]).ToArray() : null;
            if (newloss.Except(oldloss).Count() > 0 || oldloss.Count() != newloss.Count())
            {
                return false;
            }
            else if (oldendnullloss != null)
            {
                if (oldendnullloss.Count() != newendnullloss.Count() || !string.IsNullOrEmpty(_endnullseqno) && newendnullloss.Length == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void indataset()
        {
            _wrk_date = Util.GetCondition(ldpDatePicker);
            _areaid = Util.GetCondition(cboArea);
            _eqptid = Util.GetCondition(cboEquipment);
        }

        private string GetPreLossSeqnoForSave()
        {
            string pre_loss_seqno = string.Empty;
            string eqptid = string.Empty;
            string eiostat = string.Empty;

            if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
            {
                if (chkT.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'T'";
                    }
                    else
                    {
                        eiostat += ",'T'";
                    }
                }

                if (chkW.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'W'";
                    }
                    else
                    {
                        eiostat += ",'W'";
                    }
                }

                if (chkU.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(eiostat))
                    {
                        eiostat += "'U'";
                    }
                    else
                    {
                        eiostat += ",'U'";
                    }
                }
            }

            eqptid = dtMainList.Select("CHK = 1").Count() == 0 ? _eqptid : (dtMainList.Select("CHK = 1")[0]["EQPTID"]).ToString();

            object[] prelossseqno = string.IsNullOrEmpty(eiostat) ? dtBeforeList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_START < '" + txtEndHidn.Text + "'" + "AND EQPTID =" + "'" + eqptid + "'", "").Select(x => x["PRE_LOSS_SEQNO"]).ToArray() :
                 dtBeforeList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_START < '" + txtEndHidn.Text + "'" + "AND EQPTID =" + "'" + eqptid + "'" + "AND EIOSTAT IN (" + eiostat + ")", "").Select(x => x["PRE_LOSS_SEQNO"]).ToArray();
                
            for (int i = 0; i < prelossseqno.Count(); i++)
            {
                if (i != prelossseqno.Count() - 1)
                {
                    pre_loss_seqno += prelossseqno[i] + ",";
                }
                else
                {
                    pre_loss_seqno += prelossseqno[i];
                }
            }

            return pre_loss_seqno;
        }

        #region 2023.05.23 윤지해 CSR ID E20230330-001442 부동내용 바뀌면 원인설비, FCR 전체 초기화
        // 2023.06.05 윤지해 CSR ID E20230330-001442
        private void InitFCRCode()
        {
            CommonCombo _combo = new CommonCombo();

            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = Util.GetCondition(cboLossDetl);

            string failCode = string.Empty;
            string causeCode = string.Empty;
            string resolCode = string.Empty;
            string selectedText = string.Empty;
            string causeEqptid = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();

            cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
            String[] sFilterFailure = { _grid_proc, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
            cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

            if(cboFailure.Items.Count == 2)
            {
                cboFailure.SelectedIndex = 1;
                cboFailure_SelectedItemChanged(null, null);
            }
            else
            {
                cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                String[] sFilterCause = { _grid_proc, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                String[] sFilterResolution = { _grid_proc, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, selectedText, occurEqptFlag, _grid_eqpt, causeEqptid };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
            }
        }
        #endregion
    }
}
