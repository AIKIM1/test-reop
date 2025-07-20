/*************************************************************************************
 Created Date : 2020.08.19
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.19  DEVELOPER : Initial Created.
  2020.09.03  이종원    : 파일명 변경(FORM001_102 -> COM001_331), DA수정에 따른 컬럼 수정
  2020.09.08  이종원    : "공정" 조회조건 필수선택으로 수정
  2020.09.10  이종원    : 조회조건 중 "SHOPID" 포함
  2021.07.15  김지은    : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
  2022.10.06  이주홍    : 검색시 Lot ID를 7자리 이상 필수 입력 추가, RISK RANGE 컬럼 추가
                         검색시 판정전체보기 체크박스 체크시에만 RISK RANGE 컬럼 표시
  2022.10.21  이주홍    : LOT별 재공 탭에서만 Lot ID Validation을 체크하도록 수정
                         이동재공 기간별목록 탭에서만 기간 Validation을 체크하도록 수정
  2022.10.24  이주홍    : LOT별 재공 탭에서 전극일 경우 Lot ID 필수 입력 제외
                         전극이고 판정 전체보기 체크시 Lot ID 3자리 이상 입력하도록 수정
  2022.10.26  이주홍    : 전극일 경우에만 DataTable에 컬럼 추가, TERM값 저장
  2022.11.22  이주홍    : (전극 제외) Lot ID 8자리 이상 입력시 다른 Validation을 체크하지 않게 수정
  2022.12.16  이주홍    : 조회시 차단 유형도 일치하는 것만 조회하게 수정
  2023.05.22  이주홍    : GQMS 검사결과 탭 추가 및 분기문 수정
  2023.05.30  이주홍    : GQMS 검사결과 탭 조회 조건 그리드 분리 및 이벤트 수정
  2023.06.01  이주홍    : Lot Tracking 탭 조회 조건 그리드 분리 및 이벤트 수정
  2023.06.15  이주홍    : 이동재공 기간별목록 탭을 전극에서만 보이게 수정
  2023.10.12  이준영    : [E20231013-001347] LOT별 GQMS LOT_ID 조회_ 다수 LOTID 조회
  2023.10.18  이준영    : Clipborad 내역 수정
  2024.04.24  최경아    : 조회조건 CELL ID추가 
                          LOTID 검색 시 해당 LOTID에 관계된 SUBLOTID의 RISK RANGE HOLD인 경우 RISK RANGE CELL 컬럼에 표기. (24.07.30 배포예정)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_331.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_331 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _util = new Util();

		public COM001_331()
        {
            InitializeComponent();            
        }
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitControl();
			SetcboProcess(); // 공정 정보 설정

			// 2022.11.22 이주홍
			try
			{
				object[] tmps = FrameOperation.Parameters; // 이전 화면에서 넘겨준 파라미터                

                if (tmps.Length > 0) // 파라미터가 있음
				{
                    if (tmps[0].ToString().Equals("COM001_331_Tab_GQMSRESULT"))
                    {                        
                        tbcList.SelectedItem = GQMSRESULT;
                        txtLotID2.Text = tmps[1].ToString();
                        if (!string.IsNullOrEmpty(txtLotID2.Text))
                        {                             
                            dgGQMSInspectionResultSearch(txtLotID2.Text);
                            tmps[0] = "";
                        }
                    }
                    else
                    {
                        txtLotID.Text = tmps[0].ToString(); // LOTID 셋팅
                        ChkValidation(); // 해당 LOTID로 조회
                    }

				}

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK)) // 2024.12.16. 김영국 - PACK인경우는 극정 콤보가 보이지 않도록 한다. (남도우 책임)
                {
                    tbElecType.Visibility = Visibility.Collapsed;
                    cboElecType.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
			{
				Util.MessageException(ex);
			}
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            SetAuthShow();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            InitCombo();
            InitbPeriod();
            InitJdgChk();
        }

        /// <summary>
        /// 권한별 UI 설정
        /// </summary>
        //private void InitAuthShow()
        //{
        //    // 현재(2020.09.17) 이 화면은 [AuthorityMenu]테이블에 MES개발자(MESDEV)만 등록되어 있음
        //    // 
        //    if (FrameOperation.AUTHORITY.Equals("R") || FrameOperation.AUTHORITY.Equals("W"))             
        //    {
        //        TERM.Visibility = Visibility.Visible;
        //        TERM.IsEnabled = true;
        //    }
        //    else
        //    {
        //        TERM.Visibility = Visibility.Collapsed;
        //        TERM.IsEnabled = false;
        //    }
        //}
        private void SetAuthShow()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("USERID");
            dt.Columns.Add("AUTHID");

            DataRow dr = dt.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["AUTHID"] = "MESDEV";
            dt.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", dt, (result, exception) =>
            {
                try
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

					// 2023.06.15 이동재공 기간별목록 탭을 전극일 때만 보이게 함
					if (result.Rows.Count > 0 && LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E"))
                    {
                        TERM.Visibility = Visibility.Visible;
                        TERM.IsEnabled = true;
                    }
                    else
                    {
                        TERM.Visibility = Visibility.Collapsed;
                        TERM.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent); // 2022.11.28 ALL로 수정
            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //극성
            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            //생산구분
            string[] sFilter2 = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
            if (cboProductDiv.Items.Count > 1) cboProductDiv.SelectedIndex = 1;

            // GMES Hold여부
            string[] sFilter4 = { "WIPHOLD" };
            _combo.SetCombo(cboGmesHold, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter4);

            // QMS InterLock여부
            string[] sFilter5 = { "WIPHOLD" };
            _combo.SetCombo(cboQmsIntlock, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter5);

            // 차단 유형 코드
            string[] sFilter3 = { "BLOCK_TYPE_CODE" };
            _combo.SetCombo(cboBlockType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);
            //if (cboBlockType.Items.Count > 0) cboBlockType.SelectedIndex = 3;

            // 사용유무
            string[] sFilter6 = { "IUSE" };
            _combo.SetCombo(cboUseFlag, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter6);

            // RISK RANGE 여부
            string[] sFilter7 = { "IUSE" };
            _combo.SetCombo(cboRiskRangeFlag, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter7);

            //GQMS OPEN 적용시 기준 BLOCK CODE 선택
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "BLOCK_TYPE_CODE_PLANT";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
                cboBlockType.SelectedValue = dtResult.Rows[0]["ATTRIBUTE1"].ToString();
            else
                cboBlockType.SelectedIndex = 3;
            
		}

		private void InitbPeriod()
		{
			// Lot Tracking 탭은 조립, Pack일 때만 보이게 함
			LOTTRACKING.Visibility = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("A") || LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P") ? Visibility.Visible : Visibility.Collapsed;

			// 2023.06.15 이동재공 기간별목록 탭은 전극일 때만 보이게 함
			TERM.Visibility = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") ? Visibility.Visible: Visibility.Collapsed;

			// 2023.06.02 이주홍
			// Lot Tracking 탭 추가되어 분기 수정
			if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) // LOT별 재공 탭
            {
				grid1.Visibility = Visibility.Visible;
				grid2.Visibility = Visibility.Collapsed;
				grid3.Visibility = Visibility.Collapsed;

				DateDisabled();
			}
            else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM")) // 이동재공 기간별목록 탭
			{
				grid1.Visibility = Visibility.Visible;
				grid2.Visibility = Visibility.Collapsed;
				grid3.Visibility = Visibility.Collapsed;

				DateEnabled();
			}
			else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("GQMSRESULT")) // GQMS 검사결과 탭
			{
				grid1.Visibility = Visibility.Collapsed;
				grid2.Visibility = Visibility.Visible;
				grid3.Visibility = Visibility.Collapsed;

				DateDisabled();
			}
			else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("LOTTRACKING")) // Lot Tracking 탭
			{
				grid1.Visibility = Visibility.Collapsed;
				grid2.Visibility = Visibility.Collapsed;
				grid3.Visibility = Visibility.Visible;

				DateDisabled();
			}
		}

        private void InitJdgChk()
        {
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) // 이동재공 기간별목록 탭
            {
                chkRptType.Visibility = Visibility.Visible; // 판정전체보기 활성화
                chkRptType.IsEnabled = true;
            }
			else
			{
                chkRptType.Visibility = Visibility.Collapsed; // 판정전체보기 비활성화
                chkRptType.IsEnabled = false;
            }
        }

		// 2023.06.02 이주홍
		private void DateEnabled() // 기간 조건 활성화
		{
			tbPeriod.Visibility = Visibility.Visible;
			ldpDateFrom.Visibility = Visibility.Visible;
			Tilt.Visibility = Visibility.Visible;
			ldpDateTo.Visibility = Visibility.Visible;

			tbPeriod.IsEnabled = true;
			ldpDateFrom.IsEnabled = true;
			Tilt.IsEnabled = true;
			ldpDateTo.IsEnabled = true;
		}

		private void DateDisabled() // 기간 조건 비활성화
		{
			tbPeriod.Visibility = Visibility.Collapsed;
			ldpDateFrom.Visibility = Visibility.Collapsed;
			Tilt.Visibility = Visibility.Collapsed;
			ldpDateTo.Visibility = Visibility.Collapsed;

			tbPeriod.IsEnabled = false;
			ldpDateFrom.IsEnabled = false;
			Tilt.IsEnabled = false;
			ldpDateTo.IsEnabled = false;
		}
		#endregion

		#region Event
		/// <summary>
		/// Initializing 이후에 FormLoad시 Event를 생성.
		/// </summary>
		private void btnSearch_Click(object sender, RoutedEventArgs e) // LOT별 재공 탭, 이동재공 기간별목록 탭
		{
			ChkValidation(); // 2022.10.12 이주홍
		}

		// 2023.05.30 이주홍
		private void btnSearch2_Click(object sender, RoutedEventArgs e) // GQMS 검사결과 탭
		{
            if (txtLotID2.Text.Trim().Length < 7) // Lot ID 7자리 이상 필수 입력 적용
			{
				Util.MessageValidation("SFU8522"); // Lot ID를 7자리 이상 입력하세요.
				return;
			}

            dgGQMSInspectionResultSearch(txtLotID2.Text);            
		}

		// 2023.06.01 이주홍
		private void btnSearch3_Click(object sender, RoutedEventArgs e) // Lot Tracking 탭
		{
			if (txtLotID3.Text.Trim().Length < 7 && txtInputLotID.Text.Trim().Length < 7) // Lot ID 또는 Input Lot ID 7자리 이상 필수 입력 적용
			{
				Util.MessageValidation("SFU8549"); // Lot ID 또는 투입 Lot ID를 7자리 이상 입력하세요.
				return;
			}

			Search();
		}

		#endregion

		#region Method
		// 2022.10.12 이주홍
		private void ChkValidation()
		{

            // 2024.04.24 최경아
            if (!LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") && txtSublotID.Text.Trim().Length > 0 && txtLotID.Text.Trim().Length > 0) // 전극이 아님 AND SUBLOTID 입력 AND LOTID 입력
            {
                Util.MessageValidation("SFU4655"); // Lot ID, Cell ID 중 한가지만 조회 가능합니다.
                return;
            }

            // 2022.11.22 이주홍
            if (!LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") && txtLotID.Text.Trim().Length >= 8) // 전극이 아님 AND LOTID 8자리 이상 입력
			{
				Search(); // 나머지 Validation을 체크하지 않고 검색
				return;
			}

            // 2024.04.24 최경아
            if (!LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") && txtSublotID.Text.Trim().Length >= 10) // 전극이 아님 AND SUBLOTID 10자리 이상 입력
            {
                Search_Cell(); // 나머지 Validation을 체크하지 않고 검색
                return;
            }

            // 2022.10.21 이주홍
            if (((FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM")) // 이동재공 기간별목록 탭
			{
				if ((ldpDateTo.SelectedDateTime - ldpDateFrom.SelectedDateTime).TotalDays > 31)
				{
					Util.MessageValidation("SFU2042", "31"); // 기간은 31일 이내로 조회
					return;
				}
			}

			if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
			{
				Util.MessageValidation("SFU3203"); // 동은필수입니다.
				return;
			}

			if (Util.NVC(cboProcess.SelectedItemsToString) == "")
			{
				Util.MessageValidation("SFU1459"); // 공정을 선택하세요.
				return;
			}

			// 2022.10.24 이주홍
			if (((FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) // LOT별 재공 탭
			{
				if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E")) // 전극
				{
					if (chkRptType.IsChecked == true && txtLotID.Text.Trim().Length < 3) // 체크박스 체크시 Lot ID 3자리 이상 필수 입력 적용
					{
						Util.MessageValidation("SFU8524"); // Lot ID를 3자리 이상 입력하세요.
						return;
					}
				}
				else // 전극이 아님
				{
					if (txtLotID.Text.Trim().Length < 7) // Lot ID 7자리 이상 필수 입력 적용
					{
						Util.MessageValidation("SFU8522"); // Lot ID를 7자리 이상 입력하세요.
						return;
					}
				}
			}

			Search();
		}

		/// <summary>
		/// 조회
		/// </summary>
        private void dgGQMSInspectionResultSearch(String lotID2)
        {
            try
            {
                if (lotID2.Length < 7) // Lot ID 7자리 이상 필수 입력 적용
                {
                    Util.MessageValidation("SFU8522"); // Lot ID를 7자리 이상 입력하세요.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));             
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("MULTI_LOTID", typeof(string)); // 투입 Lot ID
                RQSTDT.Columns.Add("USE_FLAG", typeof(string)); // 사용 여부
                RQSTDT.Columns.Add("RISK_RANGE_FLAG", typeof(string)); // 리스크 범위 여부               

                DataRow dr = RQSTDT.NewRow();

                // INDATA를 LANGID, USE_FLAG, RISK_RANGE_FLAG, LOTID(GQMS 검사결과)만 보냄
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag) == "" ? null : Util.GetCondition(cboUseFlag);
                dr["RISK_RANGE_FLAG"] = Util.GetCondition(cboRiskRangeFlag) == "" ? null : Util.GetCondition(cboRiskRangeFlag);
                if (lotID2.ToString().Contains(","))
                {
                  dr["MULTI_LOTID"] = lotID2;
                }
                else
                {
                  dr["LOTID"] = lotID2;    
                }             
                
                RQSTDT.Rows.Add(dr);
                string bizName = string.Empty;
     
                bizName = "DA_PRD_SEL_LOT_QMS_INSP_RSLT_HIST";

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.gridClear(dgGQMSInspectionResult);
                        Util.GridSetData(dgGQMSInspectionResult, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
                                                                               
                Clipboard.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void Search()
        {
            try
            {
                // 2023.10.12  | 이준영S | [E20231013-001347] LOT별 GQMS LOT_ID 조회_ 다수 LOTID 조회   


                

                string[] stringSeparators = new string[] { "\r\n" };
                string sPasteString = Clipboard.GetText();
                //string sPasteStringLot = txtLotID.Text.ToString();
                //string sPasteStringQms = txtLotID2.Text.ToString();

                //string[] sPasteStringsLot = sPasteStringLot.Split(stringSeparators, StringSplitOptions.None);
                //string[] sPasteStringsQms = sPasteStringQms.Split(stringSeparators, StringSplitOptions.None);


                string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                if (sPasteStrings.Length>400 && sPasteStrings.Length > 400)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0516"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            return;
                        }
                    });
                   
                }
                // LOT 별 제공 탭
                string Temp_CellList = sPasteString.Replace("\r", ","); // 기본
                string Temp_CellList1 = sPasteString.Replace("\r", ","); // gqms 검사 탭

                // string Temp_CellList2 = sPasteString.Replace("\r", ","); // lot별 tracking 

                string CellList = Temp_CellList.Replace("\n", "");
                string CellList1 = Temp_CellList1.Replace("\n", "");  

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));                   // SHOP(법인) - 20200910 추가
                RQSTDT.Columns.Add("AREAID", typeof(string));                   // 동
                RQSTDT.Columns.Add("EQSGID", typeof(string));                   // 라인
                RQSTDT.Columns.Add("PROCID", typeof(string));                   // 공정
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("RPT_TYPE", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("WIPHOLD", typeof(string));
                RQSTDT.Columns.Add("QMS_HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_TYPE_CODE", typeof(string)); // 2023.04.10 이제섭 공정 구분 신규 추가
				RQSTDT.Columns.Add("USE_FLAG", typeof(string)); // 사용 여부
				RQSTDT.Columns.Add("RISK_RANGE_FLAG", typeof(string)); // 리스크 범위 여부
				RQSTDT.Columns.Add("INPUT_LOTID", typeof(string)); // 투입 Lot ID

				// 2022.11.25 이주홍
				// Validation 변경으로 LOTID 8자리 이상 입력시 다른 Validation을 체크하지 않음(동, 라인, 공정 null 가능)
				DataRow dr = RQSTDT.NewRow();

				// 2023.06.01 이주홍 Lot Tracking 탭 추가되어 분기 수정
				if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("LOTTRACKING")) // Lot Tracking 탭
				{
					// INDATA를 LANGID, LOTID(Lot Tracking), INPUT_LOTID만 보냄
					dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.GetCondition(txtLotID3) == "" ? null : Util.GetCondition(txtLotID3);                  
                    dr["INPUT_LOTID"] = Util.GetCondition(txtInputLotID) == "" ? null : Util.GetCondition(txtInputLotID);
				}
				else if (!LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") && txtLotID.Text.Trim().Length >= 8) // 전극이 아님 AND LOTID 8자리 이상 입력
				{
					// INDATA를 LANGID, SHOPID, LOTID, BLOCK_TYPE_CODE만 보냄
					dr["LANGID"] = LoginInfo.LANGID;
					dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["LOTID"] = Util.GetCondition(txtLotID);
                    if (Clipboard.ContainsText())
                    {
                        dr["LOTID"] = Util.NVC(CellList) == "" ? null : Util.NVC(CellList);
                    }
                    dr["BLOCK_TYPE_CODE"] = Util.GetCondition(cboBlockType) == "" ? null : Util.GetCondition(cboBlockType); // 2022.12.16
                    dr["SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE; // 2023.04.10 이제섭 공정 구분 신규 추가
                }
				else
				{
					dr["LANGID"] = LoginInfo.LANGID;
					dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
					dr["AREAID"] = cboArea.SelectedValue.ToString();
					dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
					dr["PROCID"] = cboProcess.SelectedItemsToString;
					dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType) == "" ? null : Util.GetCondition(cboElecType);
					dr["LOTTYPE"] = Util.GetCondition(cboProductDiv) == "" ? null : Util.GetCondition(cboProductDiv);
					dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName) == "" ? null : Util.GetCondition(txtPrjtName);
					dr["MODLID"] = Util.GetCondition(txtModlId) == "" ? null : Util.GetCondition(txtModlId);
					dr["PRODID"] = Util.GetCondition(txtProdId) == "" ? null : Util.GetCondition(txtProdId);
                    dr["LOTID"] = Util.GetCondition(txtLotID) == "" ? null : Util.GetCondition(txtLotID);                   
                    dr["BLOCK_TYPE_CODE"] = Util.GetCondition(cboBlockType) == "" ? null : Util.GetCondition(cboBlockType);
                    dr["SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE; // 2023.04.10 이제섭 공정 구분 신규 추가
				}

				// 2023.06.01 이주홍
				// Lot Tracking 탭 추가되어 조건 수정(전극이고 LOT별 재공 탭 or 이동재공 기간별목록 탭)
				if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E") &&
					((((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) ||
					((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM")))
				{
					RQSTDT.Columns.Add("WIPSTAT", typeof(string)); // 전극일때만 DataTable에 컬럼 추가
					dr["WIPSTAT"] = "TERM"; // 전극일때만 쿼리에 WIPSTAT <> 'TERM' 조건이 추가됨
                }

				if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) // LOT별 재공 탭
                {
                    //if (chkRptType.IsChecked.ToString().Equals("True"))
                    if (chkRptType.IsChecked == true)
                        dr["RPT_TYPE"] = "HIST";
                    else
                        dr["RPT_TYPE"] = "FINL";
                }
				else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM")) // 이동재공 기간별목록 탭
				{
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                }

                dr["WIPHOLD"] = Util.GetCondition(cboGmesHold) == "" ? null : Util.GetCondition(cboGmesHold);
                dr["QMS_HOLD_FLAG"] = Util.GetCondition(cboQmsIntlock) == "" ? null : Util.GetCondition(cboQmsIntlock);

                RQSTDT.Rows.Add(dr);
                string bizName = string.Empty;

				if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("WIPSTATE")) // LOT별 재공 탭
				{
					//bizName = "DA_PRD_SEL_LOT_QMS_INSP_HIST";
					bizName = "BR_PRD_GET_LOT_QMS_INSP_HIST"; // 2023.04.10 이제섭 비즈명 변경

					ShowLoadingIndicator();

					//new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
					new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) => // 2023.04.10 이제섭 비즈명 변경으로 인한 IN/OUT 데이터 테이블 변경
					{
						HiddenLoadingIndicator();
						try
						{
							if (searchException != null)
							{
								Util.MessageException(searchException);
								return;
							}

							string[] sColumnName = null;
							Util.gridClear(dgSearchResult);
							Util.GridSetData(dgSearchResult, searchResult, FrameOperation, true);
							//if (chkRptType.IsChecked.ToString().Equals("True"))
							//{
							//    sColumnName = new string[] { "LOTID", "PRODID" , "BLOCK_TYPE_NAME"};                                
							//}
							//else
							//{ 
							sColumnName = new string[] { "LOTID", "PRODID", "PRJT_NAME", "BLOCK_TYPE_CODE", "BLOCK_TYPE_NAME" };
							//}
							_util.SetDataGridMergeExtensionCol(dgSearchResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);
							if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("A")) //조립인 경우
							{
								dgSearchResult.Columns["COATING_LOTDTTM_CR"].Visibility = Visibility.Collapsed;
								dgSearchResult.Columns["ASSYLOT_INSDTTM"].Visibility = Visibility.Visible;
								dgSearchResult.Columns["PAT_REL_FLAG"].Visibility = Visibility.Visible;
								dgSearchResult.Columns["PAT_REL_DATE"].Visibility = Visibility.Visible;
								dgSearchResult.Columns["PAT_REL_NOTE"].Visibility = Visibility.Visible;
								dgSearchResult.Columns["PAT_REL_USERID"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["RISK_RANGE_CELL"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["RISK_RANGE_INSP_ID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["SUBLOT_INSDTTM"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["SUBLOTID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["LOTID"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["LOTDTTM_CR"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["HOLD_STD_TYPE_NAME"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["LOT_VLD_DATE"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["REV_NO"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["INSP_MED_CLSS_CODE"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["INSP_MED_CLSS_NAME"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["QMS_JUDG_USERID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["JUDG_USERID"].Visibility = Visibility.Visible;
                            }
							else //조립이 아닌경우
							{
								dgSearchResult.Columns["COATING_LOTDTTM_CR"].Visibility = Visibility.Visible;
								dgSearchResult.Columns["ASSYLOT_INSDTTM"].Visibility = Visibility.Collapsed;
								dgSearchResult.Columns["PAT_REL_FLAG"].Visibility = Visibility.Collapsed;
								dgSearchResult.Columns["PAT_REL_DATE"].Visibility = Visibility.Collapsed;
								dgSearchResult.Columns["PAT_REL_NOTE"].Visibility = Visibility.Collapsed;
								dgSearchResult.Columns["PAT_REL_USERID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["RISK_RANGE_CELL"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["RISK_RANGE_INSP_ID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["SUBLOT_INSDTTM"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["SUBLOTID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["LOTID"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["LOTDTTM_CR"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["HOLD_STD_TYPE_NAME"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["LOT_VLD_DATE"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["REV_NO"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["INSP_MED_CLSS_CODE"].Visibility = Visibility.Visible; 
                                dgSearchResult.Columns["INSP_MED_CLSS_NAME"].Visibility = Visibility.Visible;
                                dgSearchResult.Columns["QMS_JUDG_USERID"].Visibility = Visibility.Collapsed;
                                dgSearchResult.Columns["JUDG_USERID"].Visibility = Visibility.Visible;
                            }
						}
						catch (Exception ex)
						{
							Util.MessageException(ex);
						}
					});
				}
				else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("TERM")) // 이동재공 기간별목록 탭
				{
					bizName = "DA_PRD_SEL_LOT_QMS_INSP_HIST_TERM";

					ShowLoadingIndicator();

					new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
					{
						HiddenLoadingIndicator();
						try
						{
							if (searchException != null)
							{
								Util.MessageException(searchException);
								return;
							}

							Util.gridClear(dgTermSearchResult);
							Util.GridSetData(dgTermSearchResult, searchResult, FrameOperation, true);
							string[] sColumnName = new string[] { "LOTID", "PRODID", "PRJT_NAME", "BLOCK_TYPE_CODE", "BLOCK_TYPE_NAME" };
							_util.SetDataGridMergeExtensionCol(dgTermSearchResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);

							if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("A")) //조립인 경우
							{
								dgTermSearchResult.Columns["COATING_LOTDTTM_CR"].Visibility = Visibility.Collapsed;
								dgTermSearchResult.Columns["ASSYLOT_INSDTTM"].Visibility = Visibility.Visible;
							}
							else //조립이 아닌경우
							{
								dgTermSearchResult.Columns["COATING_LOTDTTM_CR"].Visibility = Visibility.Visible;
								dgTermSearchResult.Columns["ASSYLOT_INSDTTM"].Visibility = Visibility.Collapsed;
							}
						}
						catch (Exception ex)
						{
							Util.MessageException(ex);
						}
					});
				}
				// 2023.06.01 이주홍 Lot Tracking 탭 추가되어 분기 수정
				else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("LOTTRACKING")) // Lot Tracking 탭
				{
					bizName = "DA_PRD_SEL_LOT_TRACKING";

					ShowLoadingIndicator();

					new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
					{
						HiddenLoadingIndicator();
						try
						{
							if (searchException != null)
							{
								Util.MessageException(searchException);
								return;
							}
							Util.gridClear(dgLotTracking);
							Util.GridSetData(dgLotTracking, searchResult, FrameOperation, true);
						}
						catch (Exception ex)
						{
							Util.MessageException(ex);
						}
					});
				}

                Clipboard.Clear();
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }



        private void Search_Cell()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));                   // SHOP(법인)
                RQSTDT.Columns.Add("AREAID", typeof(string));                   // 동
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("RPT_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["SUBLOTID"] = Util.GetCondition(txtSublotID) == "" ? null : Util.GetCondition(txtSublotID);
                dr["BLOCK_TYPE_CODE"] = Util.GetCondition(cboBlockType) == "" ? null : Util.GetCondition(cboBlockType);
                

                if (chkRptType.IsChecked == true)
                    dr["RPT_TYPE"] = "HIST";
                else
                    dr["RPT_TYPE"] = "FINL";

                RQSTDT.Rows.Add(dr);

                string bizName = string.Empty;
                bizName = "BR_GET_LOT_QMS_INSP_HIST_BY_SUBLOT";

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgSearchResult);
                        Util.GridSetData(dgSearchResult, searchResult, FrameOperation, true);


                        dgSearchResult.Columns["SUBLOTID"].Visibility = Visibility.Visible;
                        dgSearchResult.Columns["SUBLOT_INSDTTM"].Visibility = Visibility.Visible;
                        dgSearchResult.Columns["RISK_RANGE_INSP_ID"].Visibility = Visibility.Visible;
                        dgSearchResult.Columns["RISK_RANGE_CELL"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["QMS_JUDG_USERID"].Visibility = Visibility.Visible;

                        dgSearchResult.Columns["LOTID"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["LOTDTTM_CR"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["COATING_LOTDTTM_CR"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["ASSYLOT_INSDTTM"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["HOLD_STD_TYPE_CODE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["HOLD_STD_TYPE_NAME"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["PK_LOTID"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["SEARCH_COND"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["SEARCH_COND2"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["IDVD_JUDG_CODE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["JUDG_DATE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["LOT_VLD_DATE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["REV_NO"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["PAT_REL_FLAG"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["PAT_REL_DATE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["PAT_REL_NOTE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["PAT_REL_USERID"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["INSP_MED_CLSS_CODE"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["INSP_MED_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        dgSearchResult.Columns["JUDG_USERID"].Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
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



        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitbPeriod();
            InitJdgChk();
        }


        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetcboProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
				RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProcess.Check(i);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

		// 2022.10.06 이주홍
		// 판정 전체보기 체크박스 체크시
		private void chkSelHist_Checked(object sender, RoutedEventArgs e)
        {
			ChkValidation();

			dgSearchResult.Columns["SEQNO"].Visibility = Visibility.Visible;
			dgSearchResult.Columns["RISK_RANGE"].Visibility = Visibility.Visible; // RISK_RANGE 컬럼 보이게 하기

			// 2022.11.22 이주홍
			dgSearchResult.Columns["PK_LOTID"].Visibility = Visibility.Visible; // 검사 LOT 컬럼 보이게 하기
			dgSearchResult.Columns["SEARCH_COND"].Visibility = Visibility.Visible; // 판정 조건 1 컬럼 보이게 하기
			dgSearchResult.Columns["SEARCH_COND2"].Visibility = Visibility.Visible; // 판정 조건 2 컬럼 보이게 하기
			dgSearchResult.Columns["IDVD_JUDG_CODE"].Visibility = Visibility.Visible; // 판정 결과 컬럼 보이게 하기
		}

		// 2022.10.06 이주홍
		// 판정 전체보기 체크박스 해제시
		private void chkSelHist_Unchecked(object sender, RoutedEventArgs e)
        {
			ChkValidation();

			dgSearchResult.Columns["SEQNO"].Visibility = Visibility.Collapsed;
			dgSearchResult.Columns["RISK_RANGE"].Visibility = Visibility.Collapsed; // RISK_RANGE 컬럼 안 보이게 하기

			// 2022.11.22 이주홍
			dgSearchResult.Columns["PK_LOTID"].Visibility = Visibility.Collapsed; // 검사 LOT 컬럼 안 보이게 하기
			dgSearchResult.Columns["SEARCH_COND"].Visibility = Visibility.Collapsed; // 판정 조건 1 컬럼 안 보이게 하기
			dgSearchResult.Columns["SEARCH_COND2"].Visibility = Visibility.Collapsed; // 판정 조건 2 컬럼 안 보이게 하기
			dgSearchResult.Columns["IDVD_JUDG_CODE"].Visibility = Visibility.Collapsed; // 판정 결과 컬럼 안 보이게 하기
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

        /// <summary>
        ///  Clear Grid
        /// </summary>
        /// <param name="dataGrid"></param>
        public static void gridClear(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            DataTable dtClear = DataTableConverter.Convert(dataGrid.ItemsSource);
            if (dtClear != null && dtClear.Rows.Count > 0)
            {
                dtClear.Rows.Clear();
                dataGrid.ItemsSource = DataTableConverter.Convert(dtClear);
                dataGrid.Refresh();
            }
        }


        private void txtLotID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgGQMSInspectionResultSearch(txtLotID2.Text);
            }
        }

        private void txtLotID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                    dgGQMSInspectionResultSearch(lotList);
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
