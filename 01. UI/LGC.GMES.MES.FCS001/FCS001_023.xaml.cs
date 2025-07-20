/**************************************************************************************************************************************************
Created Date : 2020.10.15
     Creator : 
  Decription : Tray 정보생성
-----------------------------------------------------------------------------------------------------------------------------------------------
[Change History]
 2020.10.07  NAME : Initial Created
 2021.04.26  박수미 : 라인 자동선택 체크여부에 따른 라인 콤보박스 활성화 설정
 2021.09.02  강동희 : Lot 혼입 시 Lot ID 8자리 체크로직 추가
 2022.01.20  강동희 : 입력한 Cell ID가 대문자로 입력되도록 대응
 2022.05.26  조영대 : 엑셀에서 복사 후 Cell ID 붙여넣기 시 중복메세지 제거
 2022.07.17  조영대 : Cell ID 입력시 가장 마지막 로우로 스크롤.
 2022.07.19  조영대 : Cell ID 입력시 메세지 자동 닫힘처리 및 클리어.
 2022.08.23  조영대 : cboLane 이벤트변경, (IndexChanged 이벤트사용시 선택할때 이전값을 가져옴, SelectionCommitted 로 변경)
 2022.08.24  김진섭 : NB2동 요청사항 - 전 Cell 불량 Tray에 대해서 Selector 공정 완료 상태로 변경하는 기능 추가
 2022.09.21  이정미 : 작업 공정 변경 Tab 초기화 이벤트 추가
 2022.12.29  형준우 : ESWA#1 특정등급에 대해서는 트레이 생성 불가하도록 추가 ( 추후 동별공통코드 분리 )
 2022.01.31  조영대 : Validation 추가
 2023.02.03  이정미 : A등급 금지 체크박스 이벤트 메시지 수정
 2023.02.13  이정미 : 요청자 미입력시 트레이 생성 불가하도록 수정 
 2023.02.17  염수혁 : 투입 Cell 수량 Validation 추가
 2023.02.17  조영대 : 셀 입력시 Lot 상세유형 혼입 방지 Validation 추가
 2023.02.20  염수혁 : 투입 Cell 수량 Validation 메시지 추가
 2023.02.22  이정미 : Dummy Tray 생성 오류 수정 - 첫번째 입려된 CelI ID가 가상인 경우(0000000000) 두번째 Cell ID 입력 시 발생하는 공정경로 오류 수정
 2023.03.13  이정미 : A등급 혼재 선택 시 재작업 Rote에 해당하는 재작업 등급 외 A등급 혼재 가능하도록 수정 
 2023.03.22  이정미 : Dummy Tray 구성 시 Tray Type 확인하여 자동으로 1번과 36번 Cell 위치에 빈셀이 입력되도록 기능 추가
 2023.03.23  이해령 : Lot 혼재(모델기준) 추가
 2023.04.04  이정미 : 저장라우트 조회 오류 수정
 2023.05.03  최도훈 : 인도네시아 예상해제일 지정시 오류나는 현상 수정
 2023.05.10  이정미 : 전 Cell 불량 Tray 기능 추가 - Tray 생성 시 불량 실적 생성 후 Dummy Tray 생성 
 2023.08.09  김동훈 : BizWF 기능 추가 (CellScan)
 2023.09.23  이의철 : 투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
 2023.10.24  최석준 : 재작업 Dummy 생성시 Cell의 W,E,C,V 등급 체크에 대해서 Validation 추가
 2023.11.28  손동혁 : Cell을 투입할 시 현업에서 금지하는 Cell 등급은 투입 불가 요청하여 및 팝업 기능 추가
 2023.12.08  이의철 : 투입 Cell HPCD 공정 포함, 투입 Cell T 등급 체크 추가
 2023.12.10  손동혁 : CELL 조회 시 ROUTID 조회 추가
 2023.12.10  김용식 : CNV Tray Create 기능 추가
 2023.12.12  이의철 : Degase 전 재작업 Route 만 hpcd 공정 체크 추가
 2023.12.14  이의철 : Z 등급 재작업에서 라우트 리스트 필터링
 2023.12.15  이의철 : 기존 Cell이 Charge 공정 진행했는지 확인 변경
 2023.12.22  이정미 : LOT혼재(앞 8자리만 동일) 자동 선택 기능 추가
 2023.12.28  이의철 : CheckValidationCellChargeProc 수정
 2023.12.29  형준우 : Tray 정보 생성 불가 등급 체크에 대하여 하드코딩을 동별공통코드로 수정
 2023.01.02  이정미 : 입력한 Tray ID가 대문자로 입력되도록 수정
 2024.01.18  형준우 : 디가스 폐기대기는 Tray 정보 생성 불가능하도록 수정
 2024.02.01  남형희 : E20240124-001208 ESNA 요청자 및 검색 기능 불가 변경 / Login 사용자 정보로 요청자 및 해당 정보 자동 입력
 2024.03.12  남형희 : E20240218-001674 ESNA Cell Qty 입력 값 / Cell 스캔한 값 Validation 기능 추가
 2024.05.09  양강주 : E20240311-000080 TRAY 세척기능 적용에 따른 TRAY 상태 세분화 응답 코드 정의
 2024.05.09  복현수 : 고온에이징을 완료하지 않은 셀을 더미생성에 사용할 때, 선택가능한 라우트 목록에서 고온에이징 미포함 라우트 제거 (고온에이징 미진행 셀의 human 에러 보완 로직)
 2024.05.31  권용섭 : [E20240531-001747] xaml 변경 >> 용어변경 (특별관리내용 - >특별관리유형) 및 관리내역 (특수문자 제외+대문자만+스페이스 입력 -> 제약없이 모두 입력)
 2024.06.21  최석준 : 사외반품 Cell Validation 추가
 2024.06.25  최도훈 : 재작업 불가등급 점검하는 부분 하드코딩 제거(공통코드 'FORM_DUMMY_BAN_GRADE', 동별 공통코드 'FORM_DUMMY_JUDGE_BAN' 사용)
 2024.07.23  양강주 : mv/Day 판정 공정 등록된 재작업 ROUTE에 DUMMARY 생성(CELL 등록) 시에 CELL 등급이 L 등급인지 식별 
 2024.11.11  복현수 : MES 리빌딩 PJT : 활성화 물류 사용 로직 제거
 2025.02.04  복현수 : MES 리빌딩 PJT : 수동물류 대응 건 추가
 2025.02.12  복현수 : MES 리빌딩 PJT : 수동물류 대응 건 공통코드 속성 수정에 따른 로직 수정
 2025.04.01  이현승 : Catch-Up [E20240911-001061] 등급변경 기능 있는 화면 적용 UI 추적성 향상을 위한 MENUID 추가
***************************************************************************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_023 : UserControl, IWorkArea
    {

        private bool isAOK = false;  //200330 KJE : A등급 Dummy 금지 Validation
        private bool isReworkRouteChk = true;
        private bool IsTrayTypeChk = false;
        private bool bIsLoaded = false;
        private bool bCheck = false;
        private bool bDefcCheck = false;
        bool bUseFlag = false; //

        //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
        //private bool bCellHPCDCheck = false;  
        Util _Util = new Util();
        string cellGrade = ""; // T CELL 등급 관리
        private const string CELLGRADE_L = "L"; //L 등급
        private const string CELLGRADE_T = "T"; //T 등급
        private const string CELLGRADE_Z = "Z";//Z 등급

        //투입 Cell HPCD 공정 포함과 T 등급 체크 분리
        private bool bFCS001_023_CellHPCDCheck = false;  //투입 Cell HPCD 공정 포함체크
        //private bool bFCS001_023_CelGradeT = false;  //투입 Cell T 등급 체크
        
        private bool bFCS001_023_Enable_CNVTrayCRT = false;  //CNV Tray 기능 사용 유무

        //LOT혼재(앞 8자리만 동일) 자동선택 체크
        private bool bFCS001_023_LotMixCeck = false;

        //수동물류 대응 건
        public string sManualUseFlag_Pre = null;
        public string sAgingEqptID_Pre = null;
        public string sManualUseFlag_High = null;
        public string sAgingEqptID_High = null;
        public string sManualUseFlag_Normal = null;
        public string sAgingEqptID_Normal = null;
        public string sManualUseFlag_Ship = null;
        public string sAgingEqptID_Ship = null;

        public FCS001_023()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //동별공통코드의 ATTR 전부 들고오는 DA 추가
        public DataTable GetAreaCommonCodeUse_AllAttr(string sComeCodeType, string sComeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_EGN_CODE_ITEM_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                return null;
            }
        }

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
            //bCellHPCDCheck = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_032_CELL_HPCD_CHECK");

            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_CELL_INPUT_PRHB"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            bFCS001_023_CellHPCDCheck = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_CELLHPCDCHECK");  //Z 등급 재작업에서 라우트 리스트 필터링
            //bFCS001_023_CelGradeT = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_CELGRADET"); //투입 Cell T 등급 체크

            bFCS001_023_Enable_CNVTrayCRT = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_Enable_CNVTrayCRT"); // 탭 여부 확인
            //폐기/청소상태 공 TRAY 체크 추가

            //LOT혼재(앞 8자리만 동일) 자동선택 체크
            bFCS001_023_LotMixCeck = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_LOT_MIX_CHECK"); 

            //라인 자동 선택 체크
            chkLineAuto.IsChecked = true;
            cboDummyLineID.IsEnabled = false;

            btnStartOp.Visibility = Visibility.Hidden;

            txtInputDummyTrayID.SelectAll();
            txtInputDummyTrayID.Focus();

            //E20240218-001674 Cell 수량 Display Default
            CellCount.Visibility = Visibility.Hidden;
            txtCellCount.Visibility = Visibility.Hidden;
            
            //E20240124-001208
            if (LoginInfo.CFG_AREA_ID.ToString() == "A3")
            {
                txtUserName.Text = LoginInfo.USERNAME.ToString();
                txtUserID.Text = LoginInfo.USERID.ToString();

                txtUserName.IsEnabled = false;
                txtUserID.IsEnabled = false;
                btnSearchUser.IsEnabled = false;                
            }

            //E20240218-001674 Cell 수량 Display 동별공통코드 확인
            if (_Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_CELL_QTY_COUNT"))
            {                
                CellCount.Visibility = Visibility.Visible;
                txtCellCount.Visibility = Visibility.Visible;
            }

            //A등급 금지 기본 설정
            bIsLoaded = true;
            chkABan.IsChecked = true;
            bIsLoaded = false;

            //충방전기 수동예약 기본 설정
            chkReservation.IsChecked = false;
            cboLane.IsEnabled = false;
            cboRow.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboStg.IsEnabled = false;

            // 2023.12.10 동별코드로 탭 보이는 유무 판단
            // 2023.12.11 자동처리로 변경
            /*
            if (bFCS001_023_Enable_CNVTrayCRT == true)
            {
                CnvTrayReg.Visibility = Visibility.Visible;
            }
            else
            {
                CnvTrayReg.Visibility = Visibility.Hidden;
            }
            */
            if(bFCS001_023_LotMixCeck)
            {
                chkLotMix.IsChecked = true;
            }

            //수동물류 대응 건
            TrayAssyConfirm.Visibility = Visibility.Collapsed;
            TrayAgingStartEnd.Visibility = Visibility.Collapsed;

            if (GetAreaCommonCodeUse_AllAttr("MANUAL_LOGISTICS_USE_FLAG", null).Rows.Count > 0)
            {
                TrayAssyConfirm.Visibility = Visibility.Visible;
                TrayAgingStartEnd.Visibility = Visibility.Visible;

                DataTable dtComCode = GetAreaCommonCodeUse_AllAttr("MANUAL_LOGISTICS_USE_FLAG", null) as DataTable;

                //sManualUseFlag_Pre    = Util.NVC(dtComCode.Rows[0]["ATTR1"].ToString().ToUpper().Trim()); //Pre-Aging 수동여부
                sAgingEqptID_Pre      = Util.NVC(dtComCode.Rows[0]["ATTR2"].ToString().ToUpper().Trim()); //Pre-Aging 1호기 설비 ID
                //sManualUseFlag_High   = Util.NVC(dtComCode.Rows[0]["ATTR3"].ToString().ToUpper().Trim()); //High-Aging 수동여부
                sAgingEqptID_High     = Util.NVC(dtComCode.Rows[0]["ATTR4"].ToString().ToUpper().Trim()); //High-Aging 1호기 설비 ID
                //sManualUseFlag_Normal = Util.NVC(dtComCode.Rows[0]["ATTR5"].ToString().ToUpper().Trim()); //Normal-Aging 수동여부
                sAgingEqptID_Normal   = Util.NVC(dtComCode.Rows[0]["ATTR6"].ToString().ToUpper().Trim()); //Normal-Aging 1호기 설비 ID
                //sManualUseFlag_Ship   = Util.NVC(dtComCode.Rows[0]["ATTR7"].ToString().ToUpper().Trim()); //Ship-Aging 수동여부
                sAgingEqptID_Ship     = Util.NVC(dtComCode.Rows[0]["ATTR8"].ToString().ToUpper().Trim()); //Ship-Aging 1호기 설비 ID
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void txtInputDummyTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInputDummyTrayID.Text.Trim() == string.Empty)
                    return;
                Receive_ScanMsg(txtInputDummyTrayID.Text.ToUpper().Trim());

                txtCellInput.Clear();
                txtCellInput.Focus();
            }
        }

        private void txtCellInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCellInput.Text.Trim() == string.Empty)
                    return;

                ScanCell(txtCellInput.Text.Trim());
             
                dgCell.SelectRow(dgCell.Rows.Count - 1);
                txtCellInput.Clear();
                txtCellInput.Focus();
            }
        }

        private void txtCellInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    string _ValueToFind = string.Empty;

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings.Length == 0 || sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        ScanCell(sPasteStrings[i].ToString().Trim());
                    }

                    dgCell.SelectRow(dgCell.Rows.Count - 1);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        txtCellInput.Text = string.Empty;
                        txtCellInput.Focus();
                    }));
                    
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    txtCellInput.SelectAll();
                    txtCellInput.Focus();
                }
            }
        }

        private void btnNoneCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetEmptyCellRow();
                
                dgCell.SelectRow(dgCell.Rows.Count - 1);
                txtCellInput.Clear();
                txtCellInput.Focus();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                txtCellInput.SelectAll();
                txtCellInput.Focus();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dgCell.ItemsSource = null;
            lstRoute.ItemsSource = null;

            txtDummyTrayID.Text = string.Empty;
            txtDummyCellCnt.Text = string.Empty;
            txtDummyModel.Text = string.Empty;
            txtDummyLotID.Text = string.Empty;
            txtDummyProdCD.Text = string.Empty;

            txtSpecial.Text = string.Empty;

            //E20240124-001208
            //txtUserName.Text = string.Empty;
            //txtUserID.Text = string.Empty;
            if (LoginInfo.CFG_AREA_ID.ToString() != "A3")
            {
                txtUserName.Text = string.Empty;
                txtUserID.Text = string.Empty;
            }

            txtCellCount.Text = string.Empty;

            chkLotMix.IsChecked = false;
            chkLotMix_Model.IsChecked = false;
            chkStorageR.IsChecked = false;
            chkLowVoltRoute.IsChecked = false;
            chkDegasOnly.IsChecked = false;
            chkJigRework.IsChecked = false;
        }

        private void btnDeleteCell_Click(object sender, RoutedEventArgs e)
        {

            dgCell.ItemsSource = null;
            lstRoute.ItemsSource = null;

            txtDummyModel.Text = string.Empty;
            txtDummyLotID.Text = string.Empty;
            txtDummyProdCD.Text = string.Empty;

            txtSpecial.Text = string.Empty;

            //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
            cellGrade = "";
        }

        private void btnCellRemove_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);
            dt.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

            //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
            //셀 삭제시 시트에 T등급 셀 없으면 다른 등급 셀 등록 가능하도록 남아있는 T등급 정보 삭제
            if (!string.IsNullOrEmpty(cellGrade))
            {
                bool flag = false;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if(dt.Rows[i]["FINL_JUDG_CODE"].ToString().Equals(CELLGRADE_T))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    cellGrade = "";
                }
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetTrayInfo(true);
        }

        private void btnChangeOp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();
                if (txtTrayID.GetBindValue() == null)
                {
                    txtTrayID.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0071", tbTrayID.Text));
                    return;
                }

                if (cboOp.GetBindValue() == null)
                {
                    cboOp.SetValidation(MessageDic.Instance.GetMessage("SFU8275", tbOp.Text));
                    return;
                }

                //Tray 정보를 변경하시겠습니까?
                Util.MessageConfirm("FM_ME_0079", (resultMessage) =>
                {
                    if (resultMessage == MessageBoxResult.OK)
                    {
                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("INDATA");

                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("IFMODE", typeof(string));
                        dt.Columns.Add("CSTID", typeof(string));
                        dt.Columns.Add("PROCID", typeof(string));
                        dt.Columns.Add("LANE_ID", typeof(string));
                        dt.Columns.Add("EQP_ROW_LOC", typeof(string));
                        dt.Columns.Add("EQP_COL_LOC", typeof(string));
                        dt.Columns.Add("EQP_STG_LOC", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["CSTID"] = Util.GetCondition(txtTrayID).ToUpper().Trim();
                        dr["PROCID"] = Util.GetCondition(cboOp);
                        if (chkReservation.IsChecked == true)
                        {
                            dr["LANE_ID"] = Util.GetCondition(cboLane);
                            dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow);
                            dr["EQP_COL_LOC"] = Util.GetCondition(cboCol);
                            dr["EQP_STG_LOC"] = Util.GetCondition(cboStg);
                        }
                        dr["USERID"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_CHANGE_TRAY_OP", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                switch (bizResult.Tables[0].Rows[0]["RESULT"].ToString())
                                {
                                    case "0":
                                        //변경완료하였습니다
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }


                                        });
                                        break;

                                    case "1":
                                        //이전 작업 공정 종료, Tray 정보 화면 내 정상종료를 실행해 주세요.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0195"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "2":
                                        //작업 불가 공정 입니다.(델타, 판정)
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0198"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "3":
                                        //충방전기 열,단 정보가 없습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0243"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "4":
                                        //충방전기작업과 맞지않습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0244"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "5":
                                        //현재 예약된 Tray와 공정 정보가 맞지 않습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0264"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    default:
                                        //Tray 정보 변경 중 오류가 발생하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0072"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                }
                                //GetEqpCurTray(); //MES 리빌딩 PJT : 활성화 물류 사용 로직 제거

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);
                        #region MyRegion


                        //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_CHANGE_TRAY_OP", "INDATA", "OUTDATA", ds, null);

                        //switch (dsResult.Tables[0].Rows[0]["RESULT"].ToString())
                        //{
                        //    case "0":
                        //        //변경완료하였습니다
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    case "1":
                        //        //이전 작업 공정 종료, Tray 정보 화면 내 정상종료를 실행해 주세요.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0195"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    case "2":
                        //        //작업 불가 공정 입니다.(델타, 판정)
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0198"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    case "3":
                        //        //충방전기 열,단 정보가 없습니다.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0243"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    case "4":
                        //        //충방전기작업과 맞지않습니다.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0244"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    case "5":
                        //        //현재 예약된 Tray와 공정 정보가 맞지 않습니다.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0264"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //    default:
                        //        //Tray 정보 변경 중 오류가 발생하였습니다.
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0072"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            if (result == MessageBoxResult.OK)
                        //            {
                        //                GetTrayInfo(false);
                        //            }
                        //        });
                        //        break;

                        //}
                        //GetEqpCurTray(); 
                        #endregion
                    }
                });


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetTrayInfo(true);
            }
        }

        private void chkReservation_Checked(object sender, RoutedEventArgs e)
        {
            cboLane.IsEnabled = true;
            cboStg.IsEnabled = true;
            cboCol.IsEnabled = true;
            cboRow.IsEnabled = true;
        }

        private void chkReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            cboLane.IsEnabled = false;
            cboStg.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboRow.IsEnabled = false;
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COLOR"))))
                {
                    //e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "COLOR").ToString()) as SolidColorBrush;
                }
            }));
        }

        private void btnCreateDummy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCurCnt = 0;
                bool bCellQtyCountUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_023_CELL_QTY_COUNT"); //E20240218-001674

                if (bCellQtyCountUseFlag) //E20240218-001674
                {
                    int tryValue = 0;
                    if (int.TryParse(txtCellCount.Text, out tryValue))
                    {
                        if (tryValue < 1)
                        {
                            //txtCellCount.Text = string.Empty;
                            txtCellCount.SetValidation("FCS001_023_CELL_COUNT");
                            txtCellCount.Focus();
                            Util.MessageInfo("FCS001_023_CELL_COUNT");
                            return;
                        }

                        if (dgCell.Rows.Count != tryValue)
                        {
                            txtCellCount.SetValidation("FCS001_023_CELL");
                            txtCellCount.Focus();
                            Util.MessageInfo("FCS001_023_CELL");
                            return;
                        }
                    }
                    else
                    {
                        //txtCellCount.Text = string.Empty;
                        txtCellCount.SetValidation("SFU2877");
                        Util.MessageInfo("SFU2877");
                        return;
                    }
                }

                if (chkJigRework.IsChecked == true)
                {
                    //JIG 재작업이 체크되어있습니다. 이대로 진행하시겠습니까?
                    Util.MessageConfirm("FM_ME_0330", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            chkJigRework.IsChecked = false;
                        }
                    });
                }

                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("MENUID", typeof(string)); //2025.04.02 이현승 : MENUID 추가 
                dt.Columns.Add("LANGID", typeof(string)); //추가
                dt.Columns.Add("PROD_LOTID", typeof(string));
                dt.Columns.Add("ROUTID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("CELL_CNT", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("SPCL_FLAG", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("SPCL_DESC", typeof(string));
                dt.Columns.Add("JIG_REWORK_YN", typeof(string));
                dt.Columns.Add("REQ_USERID", typeof(string));
                dt.Columns.Add("STORAGE_YN", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                //특별 예상 해제일 추가 2021.06.30 PSM
                dt.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(DateTime));

                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                if (string.IsNullOrEmpty(Util.GetCondition(txtUserID)))
                {
                    Util.Alert("FM_ME_0355"); //요청자를 입력해주세요.
                    return;
                }
                else
                {
                    dr["USERID"] = Util.GetCondition(txtUserID); ;
                }
                dr["MENUID"] = LoginInfo.CFG_MENUID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = Util.GetCondition(txtDummyLotID, sMsg: "FM_ME_0049");  //Lot ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["PROD_LOTID"].ToString())) return;

                if (lstRoute.SelectedItem == null)
                {   //공정경로를 선택해주세요.
                    Util.MessageInfo("FM_ME_0106");
                    return;
                }
                else
                {
                    dr["ROUTID"] = lstRoute.SelectedValue.ToString();
                }

                dr["CSTID"] = Util.GetCondition(txtDummyTrayID, sMsg: "FM_ME_0070").ToUpper().Trim();  //Tray ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["CSTID"].ToString())) return;

                dr["CELL_CNT"] = iCurCnt;

                dr["EQSGID"] = Util.GetCondition(cboDummyLineID, sMsg: "FM_ME_0044");  //Line ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["EQSGID"].ToString())) return;

                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial);
                dr["PRODID"] = Util.GetCondition(txtDummyProdCD);

                if (!Util.NVC(cboSpecial.SelectedValue).Equals("N") && string.IsNullOrEmpty(txtSpecial.Text))
                {
                    Util.Alert("FM_ME_0113");  //관리내역을 입력해주세요.
                    return;
                }
                dr["SPCL_DESC"] = txtSpecial.Text;
                //  if (string.IsNullOrEmpty(dr["SPCL_DESC"].ToString())) return;

                dr["JIG_REWORK_YN"] = (!chkStorageR.IsChecked == true && chkJigRework.IsChecked == true) ? "Y" : "N";

                if (!dr["SPCL_FLAG"].Equals("N"))
                {
                    dr["REQ_USERID"] = Util.GetCondition(txtUserID, sMsg: "FM_ME_0355"); //요청자를 입력해주세요.
                    if (string.IsNullOrEmpty(dr["REQ_USERID"].ToString())) return;
                }

                //200326 KJE : 저장Route 추가
                //저장Route 선택 시
                dr["STORAGE_YN"] = chkStorageR.IsChecked == true ? "Y" : "N";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if ((bool)chkReleaseDate.IsChecked)   //특별 예상 해제일 추가 2021.06.30 PSM
                {
                    dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                }

                dt.Rows.Add(dr);

                DataTable dtCell = ds.Tables.Add("IN_CELL");
                dtCell.Columns.Add("SUBLOTID", typeof(string));
                dtCell.Columns.Add("CSTSLOT", typeof(string));

                DataRow drCell = null;

                // Tray Type이 1&36번 Cell Block 해야 하는 유형인지 확인 (WA 설비 문제로 요청)
                if (IsTrayTypeChk == true)
                {
                    if (Convert.ToInt32(txtDummyCellCnt.Text) - 1 < dgCell.Rows.Count)  
                    {
                        Util.MessageConfirm("FM_ME_0478", (result) => //이 Tray의 경우에는 35번까지 입력 가능하며 36번은 등록할 수 없습니다.\r\n확인을 누르실 경우 36번은 자동으로 빈셀이 입력됩니다.\r\n계속 진행하시겠습니까?
                        {
                            if (result == MessageBoxResult.OK || result == MessageBoxResult.None)
                            {
                                for (int i = dgCell.Rows.Count - 1; i >= Convert.ToInt32(txtDummyCellCnt.Text) - 1; i--)
                                {
                                    DataTable dtDgCell = DataTableConverter.Convert(dgCell.ItemsSource);
                                    dtDgCell.Rows.RemoveAt(i);
                                    Util.GridSetData(dgCell, dtDgCell, this.FrameOperation);
                                } 

                                btnNoneCell_Click(null, null); 
                            }
                        }); return;
                    }

                    else if (Convert.ToInt32(txtDummyCellCnt.Text) - 1 == dgCell.Rows.Count) 
                    {
                        Util.MessageConfirm("FM_ME_0477", (result) => //36번에는 빈셀이 입력됩니다.
                        {
                            if (result == MessageBoxResult.OK || result == MessageBoxResult.None)
                            {
                                btnNoneCell_Click(null, null); 
                            }
                        });
                    }
                }

                else
                {
                    if (Convert.ToInt32(txtDummyCellCnt.Text) < dgCell.Rows.Count)
                    {
                        Util.MessageConfirm("FM_ME_0377", (result) => //투입 가능 Cell 개수를 초과하였습니다.
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                for (int i = dgCell.Rows.Count; i >= Convert.ToInt32(txtDummyCellCnt.Text); i--)
                                {
                                    DataTable dtDgCell = DataTableConverter.Convert(dgCell.ItemsSource);
                                    dtDgCell.Rows.RemoveAt(i);
                                    Util.GridSetData(dgCell, dtDgCell, this.FrameOperation);
                                }
                            }
                        });

                        return;
                    }
                }

                //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
                string sROUTE_ID = lstRoute.SelectedValue.ToString();



                // 2024-07-23 : mv/Day 판정 공정 등록된 재작업 ROUTE에 DUMMARY 생성(CELL 등록) 시에 CELL 등급이 L 등급인지 식별 
                {
                    int sublotCount = 0;
                    string sublotList = string.Empty;

                    //투입 Cell L 등급 체크
                    for (int i = 0; i < dgCell.Rows.Count; i++)
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID")).Equals("0000000000"))
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).Equals(CELLGRADE_L))
                            {
                                sublotList += ((sublotCount > 0) ? "," : string.Empty) + Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID"));
                                sublotCount++;
                            }
                        }
                    }

                    if (sublotCount > 0)
                    {

                        DataSet gradeDS = new DataSet();

                        DataTable gradeDT = gradeDS.Tables.Add("RQSTDT");
                        DataRow gradeDR = gradeDT.NewRow();

                        gradeDT.Columns.Add("SUBLOTJUDGE", typeof(string));
                        gradeDT.Columns.Add("ROUTID", typeof(string));
                        gradeDT.Columns.Add("SUBLOT_LIST", typeof(string));

                        gradeDR["SUBLOTJUDGE"] = CELLGRADE_L;
                        gradeDR["ROUTID"] = sROUTE_ID;

                        gradeDR["SUBLOT_LIST"] = sublotList;
                        gradeDT.Rows.Add(gradeDR);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_GET_NOT_MATCH_MV_DAY_GRADE_CELL", "RQSTDT", "RSLTDT", gradeDS);

                        if (dsRslt.Tables["RSLTDT"].Rows.Count > 0)
                        {
                            // 공정 진행중인 CELL은 mv/Day 재작업 ROUTE Tray로 생성할 수 없습니다.
                            if (string.IsNullOrEmpty(dsRslt.Tables["RSLTDT"].Rows[0]["WIP_START_CELLS"].ToString()) == false 
                                && dsRslt.Tables["RSLTDT"].Rows[0]["WIP_START_CELLS"].ToString().Equals("0") == false)
                            {
                                Util.MessageValidation("FM_ME_0618", CELLGRADE_L);   // 공정 진행중인 CELL은 mv/Day 재작업 ROUTE Tray로 생성할 수 없습니다.
                                return;
                            }

                            // 선택된 CELL은 mv/Day 재작업 ROUTE 대상 등급( %1 ) 이 아닙니다.
                            if (string.IsNullOrEmpty(dsRslt.Tables["RSLTDT"].Rows[0]["OTHER_GRADE_CELLS"].ToString()) == false 
                                && dsRslt.Tables["RSLTDT"].Rows[0]["OTHER_GRADE_CELLS"].ToString().Equals("0") == false)
                            {
                                Util.MessageValidation("FM_ME_0617", CELLGRADE_L);   // 선택된 CELL은 mv/Day 재작업 ROUTE 대상 등급( %1 ) 이 아닙니다.
                                return;
                            }

                            // CELL을 %1 등급으로 변경해야 합니다.
                            if (string.IsNullOrEmpty(dsRslt.Tables["RSLTDT"].Rows[0]["CHANGE_GRADE_CELLS"].ToString()) == false 
                                && dsRslt.Tables["RSLTDT"].Rows[0]["CHANGE_GRADE_CELLS"].ToString().Equals("0") == false)
                            {
                                Util.MessageValidation("FM_ME_0616", CELLGRADE_L);   // CELL을 %1 등급으로 변경해야 합니다.
                                return;
                            }
                        }
                    }
                }


                int nCell_Grade_T = 0;
                int nCell_Grade_Other = 0;

                //투입 Cell T 등급 체크
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID")).Equals("0000000000"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).Equals(CELLGRADE_T))
                        {
                            nCell_Grade_Other++;
                        }

                        if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).Equals(CELLGRADE_T))
                        {
                            nCell_Grade_T++;
                        }
                    }
                }


                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    drCell = dtCell.NewRow();
                    drCell["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID"));
                    //drCell["CSTSLOT"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CSTSLOT"));
                    drCell["CSTSLOT"] = (i + 1).ToString();

                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID")) == "0000000000")
                        iCurCnt++;

                    dtCell.Rows.Add(drCell);
                }
                ShowLoadingIndicator();

                //전셀 불량 Dummy Tray 생성 
                if (chkDfct.IsChecked == true)
                {
                    new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_ALL_LOSS_CELL_CREATE_DUMMY_TRAY_UI", "INDATA,IN_CELL", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("0"))
                            {
                                //생성완료하였습니다.
                                Util.MessageInfo("FM_ME_0160");
                                btnClear_Click(null, null);
                                txtTrayID.Text = dt.Rows[0]["CSTID"].ToString();
                                btnSearch_Click(null, null);
                            }
                            else
                            {
                                //생성실패하였습니다. 
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0012", bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                    }
                                });
                                return;
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
                    }, ds);
                }
                else
                {
                    new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_CREATE_DUMMY_TRAY", "INDATA,IN_CELL", "OUTDATA", (bizResult, bizException) =>
                   {
                       try
                       {
                           if (bizException != null)
                           {
                               Util.MessageException(bizException);
                               return;
                           }

                           if (bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("0"))
                           {
                               //생성완료하였습니다.
                               Util.MessageInfo("FM_ME_0160");
                               btnClear_Click(null, null);
                               txtTrayID.Text = dt.Rows[0]["CSTID"].ToString();
                               btnSearch_Click(null, null);
                           }
                           else
                           {
                               //생성실패하였습니다. (Result Code : {0})
                               LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0012", bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                               {
                                   if (result == MessageBoxResult.OK)
                                   {
                                   }
                               });
                               return;
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
                   }, ds);

               }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchFormationBoxList(Util.GetCondition(cboLane));
        }

        private void chkDfct_CheckedChanged(object sender, CMM001.Controls.UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            if (chkDfct.IsChecked.Equals(true))
            {
                //전 Cell불량 Tray의 구성을 편리하게 진행할 수 있는 기능입니다.\r\n이 항목을 선택할 경우에는 다른 선택항목은 모두 해제됩니다.\r\n계속 진행하시겠습니까?
                Util.MessageConfirm("FM_ME_0491", (result) => 
                {
                    if (result == MessageBoxResult.OK)
                    {
                        chkStorageR.IsChecked = false;
                        chkStorageR.IsEnabled = false;
                        chkStorageR.Opacity = 0.5;

                        chkABan.IsChecked = true;
                        chkABan.IsEnabled = false;

                        chkLotMix.IsChecked = false;
                        chkLotMix.IsEnabled = false;
                        chkLotMix.Opacity = 0.5;

                        chkLotMix_Model.IsChecked = false;
                        chkLotMix_Model.IsEnabled = false;
                        chkLotMix_Model.Opacity = 0.5;

                        chkLowVoltRoute.IsChecked = false;
                        chkLowVoltRoute.IsEnabled = false;
                        chkLowVoltRoute.Opacity = 0.5;

                        chkDegasOnly.IsChecked = false;
                        chkDegasOnly.IsEnabled = false;
                        chkDegasOnly.Opacity = 0.5;

                        chkJigRework.IsChecked = false;
                        chkJigRework.IsEnabled = false;
                        chkJigRework.Opacity = 0.5;

                        txtCellInput.IsEnabled = false; 
                        btnNoneCell.IsEnabled = false;
                        btnDeleteCell.IsEnabled = false;

                        chkLineAuto.IsChecked = true;
                        chkLineAuto.IsEnabled = false;
                        chkLineAuto.Opacity = 0.5;

                        chkReRoute.IsChecked = true;
                        chkReRoute.IsEnabled = false;
                        chkReRoute.Opacity = 0.5;

                        bDefcCheck = true;
                    }

                    if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                    {
                        chkDfct.IsChecked = false;
                        bDefcCheck = false;
                        return;

                    }

                });
            }

            else
            {
                chkStorageR.IsEnabled = true;
                chkStorageR.Opacity = 1.0;

                chkABan.IsEnabled = true;

                chkLotMix.IsEnabled = true;
                chkLotMix.Opacity = 1.0;

                chkLotMix_Model.IsEnabled = true;
                chkLotMix_Model.Opacity = 1.0;

                chkLowVoltRoute.IsEnabled = true;
                chkLowVoltRoute.Opacity = 1.0;

                chkDegasOnly.IsEnabled = true;
                chkDegasOnly.Opacity = 1.0;

                chkJigRework.IsEnabled = true;
                chkJigRework.Opacity = 1.0;

                txtCellInput.IsEnabled = true;
                btnNoneCell.IsEnabled = true;
                btnDeleteCell.IsEnabled = true;

                chkLineAuto.IsEnabled = true;
                chkLineAuto.Opacity = 1.0;

                chkReRoute.IsEnabled = true;
                chkReRoute.Opacity = 1.0;

                bDefcCheck = false;
            }

        }

        // Tab 3 : Create CV Carrier Info 
        // 2023.12.11 자동실행 방식으로 변경하여 탭 제거
        /*
        private void btnbtnCreateCnvCarrierInfo_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("CNV Tray Info Create?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("SCRTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("PORTID", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["SCRTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["USERID"] = LoginInfo.USERID;

                        dr["EQPTID"] = Util.GetCondition(txtEqptId, sMsg: "FM_ME_0275");  //Eqpt ID를 입력해주세요.
                        if (string.IsNullOrEmpty(dr["EQPTID"].ToString())) return;

                        dr["PORTID"] = txtProtId.Text;

                        dr["CSTID"] = Util.GetCondition(txtTrayIDCnv, sMsg: "FM_ME_0070");  //Tray ID를 입력해주세요.
                        if (string.IsNullOrEmpty(dr["CSTID"].ToString())) return;

                        dtRqst.Rows.Add(dr);

                        ShowLoadingIndicator();

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_CMD_CREATE_EMPTY_TRAY_DECIDE_ON_DESTINATION_UI", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0160"); //생성완료하였습니다.
                            btnInitCnvTab_Click(null, null);
                        }
                        else
                        {
                            //생성실패하였습니다. (Result Code : {0})
                            Util.MessageInfo("FM_ME_0159", dtRslt.Rows[0]["RETVAL"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }
            });
        }

        private void btnInitCnvTab_Click(object sender, RoutedEventArgs e)
        {
            txtTrayIDCnv.Text = string.Empty;
            txtEqptId.Text = string.Empty;
            txtProtId.Text = string.Empty;
        }
        */



        #endregion

        #region Method

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //동
            C1ComboBox[] cboAreaChild = { cboDummyLineID };
            _combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, cbChild: cboAreaChild);

            //Login 한 AREA Setting
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //라인
            //ComCombo.SetCombo(cboDummyLIneID, ComboStatus.NONE, sCase: "LINE");

            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboDummyLineID, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbParent: cboLineParent);

            string[] sFilter = { "SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "CMN");

            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE");
        }

        private void SetSearchFormationBoxList(string sLaneID)
        {
            try
            {
                cboRow.Text = string.Empty;
                cboCol.Text = string.Empty;
                cboStg.Text = string.Empty;

                CommonCombo_Form _combo = new CommonCombo_Form();

                string[] sFilter = { "1", sLaneID };
                _combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "ROW");
                _combo.SetCombo(cboCol, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "COL");
                _combo.SetCombo(cboStg, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "STG");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkABan_CheckedChanged(object sender, EventArgs e)
        {
            if (chkABan == null) return;

            bool beforeChk = chkABan.IsChecked.Equals(true);

            if (bIsLoaded.Equals(true)) return; 

            else
            {
                if (chkABan.IsChecked.Equals(false))
                {
                    //A등급 Cell을 Dummy Tray에 추가 허용하시겠습니까?
                    Util.MessageConfirm("FM_ME_0360", (firstResult) =>
                    {
                        if (firstResult != MessageBoxResult.OK)
                        {
                            chkABan.CheckedChanged -= chkABan_CheckedChanged;
                            chkABan.IsChecked = true;
                            chkABan.CheckedChanged += chkABan_CheckedChanged;
                        }

                        if (beforeChk == chkABan.IsChecked.Equals(true))
                        {
                            //작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
                            Util.MessageConfirm("FM_ME_0203", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    btnClear_Click(null, null);
                                }
                                else
                                {
                                    chkABan.CheckedChanged -= chkABan_CheckedChanged;
                                    chkABan.IsChecked = !beforeChk;
                                    chkABan.CheckedChanged += chkABan_CheckedChanged;
                                }
                            });
                        }
                    });
                }

                //작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
                if (chkABan.IsChecked.Equals(true))
                {
                    Util.MessageConfirm("FM_ME_0203", (chkResult) =>
                    {
                        if (chkResult == MessageBoxResult.OK)
                        {
                            btnClear_Click(null, null);
                        }
                    });
                }

                isAOK = chkABan.IsChecked.Equals(true) ? false : true;
            }
        }

    private void chkStorageR_Checked(object sender, EventArgs e)
    {
        //작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
        Util.MessageConfirm("FM_ME_0203", (result) =>
        {
            if(result == MessageBoxResult.OK)
            {
                btnClear_Click(null, null);
                chkJigRework.IsChecked = false;
            }

            else
            {

                chkStorageR.Unchecked -= chkStorageR_UnChecked;
                chkStorageR.IsChecked = false;
                chkStorageR.Unchecked += chkStorageR_UnChecked;  
            }
        });
    }
        private void chkStorageR_UnChecked(object sender, EventArgs e)
        {
            //작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
            Util.MessageConfirm("FM_ME_0203", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    btnClear_Click(null, null);
                }

                else
                {
                    chkStorageR.Unchecked -= chkStorageR_UnChecked;
                    chkStorageR.IsChecked = true;
                    chkStorageR.Unchecked += chkStorageR_UnChecked;
                }
            });
        }

        private void Receive_ScanMsg(string sScan)
        {
            string sResultMsg = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sScan) || sScan.Length < 10)
                {
                    //잘못된 ID입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtInputDummyTrayID.SelectAll();
                        this.txtInputDummyTrayID.Focus();
                    });
                    return;
                }

                //10자리 넘을 경우 10자리로 자름
                if (sScan.Length > 10)
                    sScan = sScan.Substring(0, 10);

                DataTable OutTable;

                if (chkDfct.IsChecked == true)
                {
                    //공 Tray 확인
                    GetCheckEmptyTray(sScan);
                    if (bCheck == true)
                    {
                        //Tray [%1]은 공 Tray 처리 할 수 없는 Tray 입니다.
                        Util.AlertInfo("FM_ME_0451", new object[] { sScan });
                        txtInputDummyTrayID.Clear();
                        return;
                    }
 
                    else
                    {
                        //전셀 불량인지 체크  
                        if (CheckExistWipCell(sScan) == false)
                        {
                            //전 Cell 불량 Tray가 아닙니다. 
                            Util.AlertInfo("FM_ME_0500", new object[]
                        { Util.GetCondition(txtTrayID, bAllNull: true)});
                            txtInputDummyTrayID.Clear();
                            HiddenLoadingIndicator();
                            return;
                        }
                    }
                }

                else
                {

                    //직전 전Cell불량 Tray인지 확인
                    if (GetCheckLossTray(sScan, out OutTable))
                    {
                        Util.gridClear(dgCell);
                        txtDummyTrayID.Text = sScan;
                        GetCellCnt(txtDummyTrayID.Text);

                        for (int i = 0; i < OutTable.Rows.Count; i++)
                        {
                            if (OutTable.Rows[i]["SUBLOTID"].ToString().Equals("0000000000"))
                            {
                                SetEmptyCellRow();
                                continue;
                            }
                            //모든 체크 끝나면 스프레드에 CELL ID 쓰기
                            Receive_ScanMsg(OutTable.Rows[i]["SUBLOTID"].ToString());
                        }
                    }
                    //재공유무 및 Tray상태 확인(청소,폐기는 Dummy 생성 불가)
                    else if (GetCheckTrayStatus(sScan, out sResultMsg))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sResultMsg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                this.txtInputDummyTrayID.SelectAll();
                                this.txtInputDummyTrayID.Focus();
                            }
                        });
                        return;
                    }

                    //DataTable EmtpyDt =  CheckValidationEmptyTrayStatusByCSTID(txtDummyTrayID.Text);
                    //string sCST_CLEAN_FLAG = string.Empty;
                    //string sCST_MNGT_STAT_CODE = string.Empty;

                    //if (EmtpyDt.Rows.Count > 0)
                    //{                    
                    //    sCST_CLEAN_FLAG = Util.NVC(EmtpyDt.Rows[0]["CST_CLEAN_FLAG"]).ToString();
                    //    sCST_MNGT_STAT_CODE = Util.NVC(EmtpyDt.Rows[0]["CST_MNGT_STAT_CODE"]).ToString();

                    //    //ONCLEAN
                    //    if (sCST_CLEAN_FLAG.Equals("Y"))
                    //    {
                    //        Util.MessageInfo("SUF9022"); //청소중인 공 tray 입니다.
                    //        return;

                    //    }

                    //    //DISUSE
                    //    if (sCST_CLEAN_FLAG.Equals("N") && sCST_MNGT_STAT_CODE.Equals("S"))
                    //    {
                    //        Util.MessageInfo("SUF9023"); //폐기된 공 tray 입니다.
                    //        return;
                    //    }
                    //}
                }
                //Tray Type으로 Cell 자동생성 유무 확인
                if (GetChekBlockTrayType(sScan) && GetChekBlockTrayType(sScan))
                {
                    IsTrayTypeChk = true;
                    btnNoneCell_Click(null, null);
                }
                else IsTrayTypeChk = false;

                txtDummyTrayID.Text = sScan.Trim();
                GetCellCnt(txtDummyTrayID.Text);

                //Tray에 담겨있는 Cell ID 확인 후 입력 
                if (chkDfct.IsChecked == true && CheckExistWipCell(sScan) == true)
                {
                    SetTrayCell(sScan);
                }

                txtDummyTrayID.Text = sScan.Trim();
                GetCellCnt(txtDummyTrayID.Text);

                this.txtInputDummyTrayID.Clear();
                this.txtInputDummyTrayID.Focus();

                return;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool GetCheckLossTray(string sTray, out DataTable OutTable)
        {
            bool bCheck = false;
            OutTable = null;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CSTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CSTID"] = sTray;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_TRAY_ALL_LOSS_CELL", "INDATA", "OUTDATA,CELLS", inDataSet);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0 && dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    OutTable = dsRslt.Tables["CELLS"];

                    //모든 Cell이 불량인 Tray입니다.\r\nTray 정보를 생성하시겠습니까?
                    Util.MessageConfirm("FM_ME_0131", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            bCheck = true;
                        }
                    });
                }

                return bCheck;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }

        private bool GetChekBlockTrayType(string TrayID)
        {
            bool bTrayType = false;
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["COM_TYPE_CODE"] = "TRAY_TYPE_AUTO_CREATE_SUBLOT"; 
                dr["CSTID"] = TrayID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dtRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_CHECK_TRAY_TYPE_AUTO_CREATE_SUBLOT_UI", "INDATA", "OUTDATA", ds);


                //Util.Alert("FM");
                if (dtRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    bTrayType = true;
                }
                else bTrayType = false;
            }

            
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bTrayType;
        }

        private void GetCellCnt(string TrayID)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = TrayID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_TM_TRAY_TYPE", "INDATA", "OUTDATA", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    txtDummyCellCnt.Text = dsRslt.Tables["OUTDATA"].Rows[0]["CST_CELL_QTY"].ToString();
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void ScanCell(string sCellID)
        {
            string sWipStat = string.Empty;
            string sWipSName = string.Empty;
            string sLotDetlTypeCode = string.Empty;
            string sColor = string.Empty;
            string sTrayType = string.Empty;
            string sCstsLOT = string.Empty;
            string sLotDetlTypeName = string.Empty;

            string sRetval = string.Empty;
            string sSublotID = string.Empty;
            string sTypeCode = string.Empty;
            string sOCOPRtnFlag = string.Empty;
            string sPassFlag = string.Empty;

            #region BizWF 
            int RetVal = BizWF_Check(sCellID);

            if (RetVal != 0)
            {
                //ShowLoadingIndicator();
                //loadingIndicator.Visibility = Visibility.Hidden;
                return;
            }
            #endregion

            try
            {
                if (string.IsNullOrEmpty(txtDummyTrayID.Text))
                {
                    this.txtInputDummyTrayID.Clear();
                    //Tray를 먼저 입력해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                //Tray Type Get
                GetTrayType(txtDummyTrayID.Text.Trim(), ref sTrayType);

                //셀 빈것 처리
                if (sCellID.Equals("0000000000"))
                {
                    SetEmptyCellRow();
                    return;
                }

                if (string.IsNullOrEmpty(sCellID) || sCellID.Length < 10)
                {
                    //잘못된 ID입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                //이미 스캔한 CELL ID인지 Check
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID").ToString() == sCellID)
                    {
                        //이미 스캔한 ID 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0193"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return;
                    }
                }

                //CELL 공정 완료 체크
                // - 요고는 SKIP하기로 함. 
                // - Transaction을 하나로 합치기 위해, 저장할때 BIZ에서 처리하도록 함.
                //if (!GetCellCheck(sCellID))
                //{
                //    //공정 진행중인 Cell 입니다.\r\n종료 후 추가하시겠습니까?
                //    Util.MessageConfirm("FM_ME_0097", (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            UpdDummySelectYN(sCellID);
                //        }
                //    });
                //}

                //BR 배포 전 임시 주석처리 2024.06.25 복현수
                //GetOCOPReturnCellCheck(sCellID, ref sRetval, ref sSublotID, ref sTypeCode, ref sOCOPRtnFlag, ref sPassFlag);

                if (sOCOPRtnFlag == "Y")
                {
                    if (string.IsNullOrEmpty(sTypeCode))
                    {
                        // 처리유형 정보가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0614", sSublotID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);

                        return;
                    }
                    else if (sPassFlag == "N")
                    {
                        // 해당 처리유형은 작업할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0613", sSublotID, sTypeCode), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);

                        return;
                    }
                }

                GetCellCheck(sCellID, ref sWipStat, ref sWipSName, ref sLotDetlTypeCode, ref sLotDetlTypeName, ref sColor, ref sCstsLOT);
            
                if (sWipStat == "TERM")
                {
                    //이미 종료된 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0401"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                //조립등록 여부 체크
                if (!GetCellAssyCheck(sCellID))
                {
                    //조립미등록 Cell입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0228"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                //첫번째 셀이 가상인지 체크 
                string firstCellId = string.Empty;
                int cellRow = 0;
                for (int row = 0; row < dgCell.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID")).Equals("0000000000"))
                    {
                        continue;
                    }
                    else
                    {
                        firstCellId = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID"));
                        cellRow = row + 1;
                        break;
                    }
                }

                // 첫번째 셀이 아니고, 기존 조회된 Route정보가 여러개라면
                if (dgCell.GetRowCount() > 0 && !string.IsNullOrEmpty(firstCellId) && lstRoute.Items.Count > 1)
                {
                    isReworkRouteChk = false;
                }

                //등급/출하여부 체크
                DataTable dtGrade = new DataTable();
                if (!GetBadCellCheck(sCellID, ref dtGrade))
                    return;

                //디가스 NG 체크 2011.09.26 정종덕S : 오창에서만 사용
                //외관 검사기에서 측정한 두께 데이터중 DEGAS Vision 검사 결과 NG이면 추가못함
                //TC_CELL_THICK_MEAS.DEGAS_JUDG
                //디가스 NG 체크박스 선택되었을때만 가능
                //if ( chkDegasVisionCheck)
                //{
                //    if (!GetCellDegasJudgCheck(sScan))
                //        return;
                //}
                // 재작업 라우트 자동선택 체크시  
                // 첫번째 셀이 아니면 스캔한 셀 공정 체크
                if (isReworkRouteChk && chkReRoute.IsChecked == true && dgCell.Rows.Count - cellRow > 0 && !string.IsNullOrEmpty(firstCellId))
                {
                    string sRouteId = string.Empty;

                    if (lstRoute.Items.Count == 0)
                    {                        
                        //Tray의 공정경로가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0082"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return;
                    }
                    else
                    {
                        lstRoute.SelectedIndex = 0;
                        sRouteId = lstRoute.SelectedValue.ToString();
                    }

                    if (!GetReWorkCheck(sCellID, sRouteId, sTrayType))
                    {
                        //공정경로가 동일하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0101"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return;
                    }
                }

                //최초 CELL 일경우 처리
                if (string.IsNullOrEmpty(firstCellId))  //Todo : 로직수정해야됨, 최초셀이지만 이전에 None True Cell 만 있을경우에도 최초True셀이 될수있음
                {
                    if (!GetFirstCellCheck(sCellID, sTrayType))
                        return;

                    if (lstRoute.Items.Count == 0)
                    {
                        //공정경로가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0102"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    Boolean RouteHigtchik = true;

                    #region 고온에이징 진행여부 Validation 2024.05.09
                    ////전체 공정 진행이력 중 고온에이징 공정이력이 없는 셀이면
                    if (!CheckProcByCell("4", sCellID))
                    {
                        DataTable dtListRoute = DataTableConverter.Convert(lstRoute.ItemsSource);

                        for (int i = lstRoute.Items.Count - 1; i >= 0; i--)
                        {
                            var TargetRow = lstRoute.Items[i] as DataRowView;
                            string sTargetRouteID = TargetRow["ROUTID"].ToString();

                            //디전라우트가 아니면서
                            if (!CheckProcByRoute("D", null, sTargetRouteID))
                            {
                                //고온에이징 공정이 포함되지 않은 라우트이면 필터링
                                if (!CheckProcByRoute(null, "4", sTargetRouteID))
                                {
                                    dtListRoute.Rows.RemoveAt(i);
                                    RouteHigtchik = false;
                                }
                            }
                        }

                        if (RouteHigtchik == false)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0632"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                            //고온 Aging 공정 이력이 없는 셀은 Dummy 진행할 수 없습니다. 
                        }



                        //필터링된 데이터 재적용
                        lstRoute.ItemsSource = dtListRoute.AsDataView();
                        lstRoute.SelectedValuePath = "ROUTID";
                        lstRoute.DisplayMemberPath = "CMCDNAME";

                        if (lstRoute.Items.Count > 0)
                            lstRoute.SelectedIndex = 0;
                    }
                    #endregion 고온에이징 진행여부 Validation 2024.05.09

                    /***********************************************************************************************************************************
                     등급 재작업 라우터 필터링 로직 (NA 특화 Z )
                     사유 : HPCD 설비 다운 ->  공정 재시작 하지 않고 -> Tray 꺼내서 재작업 시도 -> Z 등급(미판정)
                        1.Z 등급 재작업시
                        2.디가스 전 TRAY TYPE 확인
                        3.기존 Cell이 HPCD 공정 진행했는지 확인
                        4.기존 Cell이 Charge 공정 진행했는지 확인
                        5.기존 HPCD 공정 진행 이력 없으면 HPCD 없는 공정은 필터링
                        6.기존 Charge 공정 진행 이력 있으면 HPCD 있는 공정은 필터링

                        HPCD 다음공정은 Charge #1 이기 때문에 Charge 공정 유무 체크
                    ***********************************************************************************************************************************/
                    bool bHPCDProc = false;
                    bool bChargeProc = false;
                    //Z 등급 재작업에서 라우트 리스트 필터링
                    if (bFCS001_023_CellHPCDCheck.Equals(true))
                    {
                        if (dtGrade.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("Z"))
                        {
                            //디가스 전 TRAY TYPE 확인
                            //DataTable dtR = CheckValidationRoutGRTypeByCell(sCellID);
                            DataTable dtR = CheckValidationCstTypeCode(sTrayType);
                            if (dtR.Rows.Count > 0)
                            {
                                if (dtR.Rows[0]["CST_ROUT_TYPE_CODE"].ToString().Equals("D"))
                                {
                                    //cell hpcd 이력이 있으면
                                    if (CheckValidationCellHpcdProc(sCellID,"U"))
                                    {
                                        bHPCDProc = true;                                        
                                    }

                                    //기존 Cell이 Charge 공정 진행했는지 확인
                                    //if (CheckValidationCellHpcdProc(sCellID, "1"))
                                    if(CheckValidationCellChargeProc(sCellID, "1","FF1101"))
                                    {
                                        bChargeProc = true;
                                    }


                                    DataTable dt = DataTableConverter.Convert(lstRoute.ItemsSource);                                    
                                    
                                    for (int i = lstRoute.Items.Count -1; i >= 0; i--)
                                    {
                                        var CompRow = lstRoute.Items[i] as DataRowView;
                                        string sRoute = CompRow["ROUTID"].ToString();
                                        
                                        if (bHPCDProc.Equals(false))
                                        {
                                            //기존 HPCD 공정 진행 이력 없으면 HPCD 없는 공정은 필터링
                                            if (!CheckValidationRouteHPCD(sRoute))
                                            {
                                                //list 에서 제거                                                
                                                dt.Rows.RemoveAt(i);
                                            }
                                                                                     
                                        }

                                        if (bChargeProc.Equals(true))
                                        {
                                            //기존 Charge 공정 진행 이력 있으면 HPCD 있는 공정은 필터링
                                            if (CheckValidationRouteHPCD(sRoute))
                                            {
                                                //list 에서 제거                                                
                                                dt.Rows.RemoveAt(i);
                                            }
                                        }
                                    }

                                    //필터링된 데이터 재적용
                                    lstRoute.ItemsSource = dt.AsDataView();
                                    lstRoute.SelectedValuePath = "ROUTID";
                                    lstRoute.DisplayMemberPath = "CMCDNAME";

                                    if (lstRoute.Items.Count > 0)
                                        lstRoute.SelectedIndex = 0;
                                }                                
                                
                            }
                            
                        }
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(firstCellId))
                    {
                        if (!chkLotMix.IsChecked == true && !GetSecondLotCheck(sCellID) && !chkLotMix_Model.IsChecked == true)
                        {
                            //이전 입력된 Cell과 Lot ID가 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0194"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }
                        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 START
                        else if (chkLotMix.IsChecked == true && !GetSecondLotCheckDayGrLotID(sCellID))
                        {
                            //이전 입력된 Cell과 Lot ID가 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0194"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }
                        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 END
                        // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가 START
                        else if (chkLotMix_Model.IsChecked == true && !GetSecondLotCheckModelLotID(sCellID))
                        {
                            //이전 입력된 Cell과 Lot ID가 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0194"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }
                        // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가 END

                        //LOTTYPE Validation 추가
                        if (!GetLotTypeCheck(sCellID))
                        {
                            //이전 입력된 Cell과 LOTTYPE이 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0430"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }

                        //LOT_DETL_TYPE_CODE Validation 추가
                        if (!GetLotDetlTypeCheck(sCellID))
                        {
                            //이전 입력된 Cell과 Lot 상세 유형이 다릅니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0470"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }

                        #region 고온에이징 진행여부 Validation 2024.05.09
                        bool isFirstCellProceedHTA = new bool(); //첫번째 셀의 고온에이징 진행여부
                        bool isCompareCellProceedHTA = new bool(); //두번째 이후 셀의 고온에이징 진행여부

                        //첫번째 셀이 고온에이징 공정을 진행했는지 체크
                        isFirstCellProceedHTA = CheckProcByCell("4", firstCellId) ? true : false;

                        //두번째 이후 셀이 고온에이징 공정을 진행했는지 체크
                        isCompareCellProceedHTA = CheckProcByCell("4", sCellID) ? true : false;

                        if (isFirstCellProceedHTA != isCompareCellProceedHTA)
                        {
                            //고온에이징 진행 셀과 미진행 셀은 혼재할 수 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0612"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return;
                        }
                        #endregion 고온에이징 진행여부 Validation 2024.05.09
                    }
                }

                //불량 폐기 셀 등록 여부 체크(TC_CELL_LOSS)
                //복구하는 로직이 있으므로 가장 마지막에 체크함
                //2020-06-30 KJE : CWA 3동 신규 실적로직 반영으로 인하여 더미생성시 불량복구 안함
                //if (!LoginInfo.AREAID.ToString().Substring(0, 3).Equals("CWA"))
                //{
                if (!GetLossCellCheck(sCellID))
                    return;
                //}

                //Binding
                if (dgCell.GetRowCount() == 0)
                {
                    DataTable dt = new DataTable();
                    InitDT(ref dt);

                    DataRow dr = dt.NewRow();
                    //dr["SUBLOTID"] = sCellID; //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["SUBLOTID"] = sCellID.ToUpper(); //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["FINL_JUDG_CODE"] = Util.NVC(dtGrade.Rows[0]["FINL_JUDG_CODE"]);
                    dr["WIPSTAT"] = sWipStat;
                    dr["WIPSNAME"] = sWipSName;
                    dr["LOT_DETL_TYPE_CODE"] = sLotDetlTypeCode;
                    dr["LOT_DETL_TYPE_NAME"] = sLotDetlTypeName;
                    dr["COLOR"] = sColor;
                    dr["CSTSLOT"] = sCstsLOT;
                    dr["ROUTID"] = Util.NVC(dtGrade.Rows[0]["ROUTID"]);
                    
                    dt.Rows.Add(dr);
                    Util.GridSetData(dgCell, dt, FrameOperation, true);
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);

                    DataRow dr = dt.NewRow();
                    //dr["SUBLOTID"] = sCellID; //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["SUBLOTID"] = sCellID.ToUpper(); //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["FINL_JUDG_CODE"] = dtGrade.Rows[0]["FINL_JUDG_CODE"].ToString();
                    dr["WIPSTAT"] = sWipStat;
                    dr["WIPSNAME"] = sWipSName;
                    dr["LOT_DETL_TYPE_CODE"] = sLotDetlTypeCode;
                    dr["LOT_DETL_TYPE_NAME"] = sLotDetlTypeName;
                    dr["COLOR"] = sColor;
                    dr["CSTSLOT"] = sCstsLOT;
                    dr["ROUTID"] = Util.NVC(dtGrade.Rows[0]["ROUTID"]);

                    dt.Rows.Add(dr);
                    Util.GridSetData(dgCell, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DfctScanCell(string sCellID)
        {
            string sWipStat = string.Empty;
            string sWipSName = string.Empty;
            string sLotDetlTypeCode = string.Empty;
            string sColor = string.Empty;
            string sTrayType = string.Empty;
            string sCstsLOT = string.Empty;
            string sLotDetlTypeName = string.Empty;

            try
            {
                //Tray Type Get
                GetTrayType(txtDummyTrayID.Text.Trim(), ref sTrayType);

                GetCellCheck(sCellID, ref sWipStat, ref sWipSName, ref sLotDetlTypeCode, ref sLotDetlTypeName, ref sColor, ref sCstsLOT);

                if (sWipStat == "TERM")
                {
                    //이미 종료된 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0401"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                // 첫번째 셀이 아니고, 기존 조회된 Route정보가 여러개라면
                if (dgCell.GetRowCount() > 0 && lstRoute.Items.Count > 1)
                {
                    isReworkRouteChk = false;
                }

                //등급/출하여부 체크
                DataTable dtGrade = new DataTable();
                if (!GetBadCellCheck(sCellID, ref dtGrade))
                    return;

                // NG 체크 2011.09.26 정종덕S : 오창에서만 사용
                //외관 검사기에서 측정한 두께 데이터중 DEGAS Vision 검사 결과 NG이면 추가못함
                //TC_CELL_THICK_MEAS.DEGAS_JUDG
                //디가스 NG 체크박스 선택되었을때만 가능
                //if ( chkDegasVisionCheck)
                //{
                //    if (!GetCellDegasJudgCheck(sScan))
                //        return;
                //}
                // 재작업 라우트 자동선택 체크시  
                // 첫번째 셀이 아니면 스캔한 셀 공정 체크
                if (isReworkRouteChk && chkReRoute.IsChecked == true && dgCell.Rows.Count > 0)
                {
                    string sRouteId = string.Empty;

                    if (lstRoute.Items.Count == 0)
                    {
                        //Tray의 공정경로가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0082"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return;
                    }
                    else
                    {
                        lstRoute.SelectedIndex = 0;
                        sRouteId = lstRoute.SelectedValue.ToString();
                    }
                }

                //최초 CELL 일경우 처리
                if (dgCell.GetRowCount() == 0)  //Todo : 로직수정해야됨, 최초셀이지만 이전에 None True Cell 만 있을경우에도 최초True셀이 될수있음
                {
                    if (!GetFirstCellCheck(sCellID, sTrayType))
                        return;

                    if (lstRoute.Items.Count == 0)
                    {
                        //공정경로가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0102"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return;
                    }
                }

                //Binding
                if (dgCell.GetRowCount() == 0)
                {
                    DataTable dt = new DataTable();
                    InitDT(ref dt);

                    DataRow dr = dt.NewRow();
                    //dr["SUBLOTID"] = sCellID; //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["SUBLOTID"] = sCellID.ToUpper(); //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["FINL_JUDG_CODE"] = Util.NVC(dtGrade.Rows[0]["FINL_JUDG_CODE"]);
                    dr["WIPSTAT"] = sWipStat;
                    dr["WIPSNAME"] = sWipSName;
                    dr["LOT_DETL_TYPE_CODE"] = sLotDetlTypeCode;
                    dr["LOT_DETL_TYPE_NAME"] = sLotDetlTypeName;
                    dr["COLOR"] = sColor;
                    dr["CSTSLOT"] = sCstsLOT;
                    dr["ROUTID"] = Util.NVC(dtGrade.Rows[0]["ROUTID"]);

                    dt.Rows.Add(dr);
                    Util.GridSetData(dgCell, dt, FrameOperation, true);

                    //전셀 불량일 경우 숨김처리
                    if (chkDfct.IsChecked == true)
                    {
                        dgCell.Columns[9].Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);

                    DataRow dr = dt.NewRow();
                    //dr["SUBLOTID"] = sCellID; //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["SUBLOTID"] = sCellID.ToUpper(); //20220120_입력한 Cell ID가 대문자로 입력되도록 대응
                    dr["FINL_JUDG_CODE"] = dtGrade.Rows[0]["FINL_JUDG_CODE"].ToString();
                    dr["WIPSTAT"] = sWipStat;
                    dr["WIPSNAME"] = sWipSName;
                    dr["LOT_DETL_TYPE_CODE"] = sLotDetlTypeCode;
                    dr["LOT_DETL_TYPE_NAME"] = sLotDetlTypeName;
                    dr["COLOR"] = sColor;
                    dr["CSTSLOT"] = sCstsLOT;
                    dr["ROUTID"] = Util.NVC(dtGrade.Rows[0]["ROUTID"]);

                    dt.Rows.Add(dr);
                    Util.GridSetData(dgCell, dt, FrameOperation, true);

                    //전셀 불량일 경우 숨김처리
                    if (chkDfct.IsChecked == true)
                    {
                        dgCell.Columns[9].Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitDT(ref DataTable dt)
        {
            dt.Columns.Add("SUBLOTID");
            dt.Columns.Add("FINL_JUDG_CODE");
            dt.Columns.Add("WIPSTAT");
            dt.Columns.Add("WIPSNAME");
            dt.Columns.Add("LOT_DETL_TYPE_CODE");
            dt.Columns.Add("LOT_DETL_TYPE_NAME");
            dt.Columns.Add("COLOR");
            dt.Columns.Add("CSTSLOT");
            dt.Columns.Add("ROUTID");
        }

        private bool GetLossCellCheck(string sCellId)
        {
            bool bFlag = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_LOSS_YN", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    #region 디가스 불량 Cell 체크 확인
                    if (IsDummyDegasCellBanCheck())
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("SUBLOTLIST", typeof(string));

                        DataRow newrow = RQSTDT.NewRow();
                        newrow["SUBLOTLIST"] = sCellId;
                        RQSTDT.Rows.Add(newrow);

                        DataTable dtLotDetlTypeCode = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_LOT_DETL_TYPE_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtLotDetlTypeCode.Rows.Count > 0)
                        {
                            if (dtRslt.Rows[0]["DFCT_GR_TYPE_CODE"].ToString().Trim().Equals("D") && dtLotDetlTypeCode.Rows[0]["LOT_DETL_TYPE_CODE"].ToString().Trim().Equals("N"))
                            {
                                //[%]Cell은 디가스 폐기대기 상태이므로 Cell 복구 이후 다시 진행해주세요
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0562", sCellId), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtCellInput.SelectAll();
                                        txtCellInput.Focus();
                                        btnDeleteCell_Click(null, null);
                                    }
                                });
                                return false;
                            }
                        }
                    }
                    #endregion

                    if (dtRslt.Rows[0]["EQPT_INPUT_AVAIL_FLAG"].ToString().Equals("T")) //복구불가 셀일경우
                    {
                        //복구불가 Cell은 Dummy 진행 할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0390"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        btnDeleteCell_Click(null, null);
                        return false;
                    }
                    else
                    {
                        //Cell ID : {0}는 불량 셀에 등록된 셀입니다. 복구 후 진행하시겠습니까?
                        Util.MessageConfirm("FM_ME_0357", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                bFlag = true;
                            }
                            else
                            {
                                //불량 셀 등록 Cell은 Dummy 진행 할 수 없습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0358"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                                btnDeleteCell_Click(null, null);
                                bFlag = false;
                            }
                        }, new string[] { sCellId });
                    }
                }
                return bFlag;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

        private void GetTrayType(string sTray, ref string sTrayType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = sTray;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_F", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    sTrayType = SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetEmptyCellRow()
        {
            try
            {
                DataTable dt;
                DataRow dr;

                if (dgCell.GetRowCount() == 0)
                {
                    InitGrid();

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgCell.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["SUBLOTID"] = "0000000000";
                    temp.AcceptChanges();

                    dgCell.ItemsSource = DataTableConverter.Convert(temp);
                }
                else
                {
                    dt = DataTableConverter.Convert(dgCell.ItemsSource);
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dgCell.ItemsSource = DataTableConverter.Convert(dt);

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgCell.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["SUBLOTID"] = "0000000000";
                    temp.AcceptChanges();

                    dgCell.ItemsSource = DataTableConverter.Convert(temp);
                }

                // 스프레드 스크롤 하단으로 이동
                dgCell.ScrollIntoView(dgCell.GetRowCount() - 1, 1);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool GetCheckTrayStatus(string sTray, out string sResultMsg)
        {
            bool bCheck = false;
            sResultMsg = string.Empty;

            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sTray;
                dr["WIPSTAT"] = "WAIT,PROC";
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CHK_TRAY_STATUS", "INDATA", "OUTDATA", ds);

                switch (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString())
                {
                    case "0":   //정상
                        bCheck = false;
                        break;

                    case "1":   //재공있음
                        bCheck = true;
                        sResultMsg = "FM_ME_0207";  //재공이 존재하는 Tray입니다.
                        break;

                    case "2":   //Tray 정상상태 아님
                        bCheck = true;
                        sResultMsg = "FM_ME_0379";  //Tray상태가 정상이 아닙니다.
                        break;

                    case "3":   //Tray 정보없음 - New Tray는 Dummy 생성 가능
                        bCheck = false;
                        // 2023.12.11 자동생성으로 변경
                        if (bFCS001_023_Enable_CNVTrayCRT == true)
                        {
                            CreateCNVTrayInfo(sTray, "N1FCNV281", "");
                        }
                        break;

                    /*
                       E20240311-000080 TRAY 세척기능 적용에 따른 TRAY 상태 세분화 응답 코드 정의 
                       4 : 세척 대상 TRAY 입니다.
                       5 : 폐기된 공 TRAY 입니다.
                    */
                    case "4":   //세척 대상 TRAY 입니다.
                        bCheck = true;
                        sResultMsg = "FM_ME_0608";  //세척 대상 TRAY 입니다.
                        break;

                    case "5":   //폐기된 공 TRAY 입니다.
                        bCheck = true;
                        sResultMsg = "FM_ME_0609";  //폐기된 공 TRAY 입니다.
                        break;

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }

        private void GetCheckEmptyTray(string sTray)  //공트레인지 확인
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["CSTID"] = sTray;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_DUMMY", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                if (dtRslt.Rows.Count == 0)
                {
                    bCheck = true;
                }
                else
                {
                    bCheck = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void InitGrid()
        {
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("SUBLOTID", typeof(string));
            dtTable.Columns.Add("FINL_JUDG_CODE", typeof(string));
            dtTable.Columns.Add("WIPSTAT", typeof(string));
            dtTable.Columns.Add("WIPSNAME", typeof(string));
            dtTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
            dtTable.Columns.Add("LOT_DETL_TYPE_NAME", typeof(string));
            dtTable.Columns.Add("COLOR", typeof(string));
            dtTable.Columns.Add("CSTSLOT", typeof(string));
            dtTable.Columns.Add("ROUTID", typeof(string));

            DataRow dr = dtTable.NewRow();

            dtTable.Rows.Add(dr);

            dgCell.ItemsSource = DataTableConverter.Convert(dtTable);
        }

        private bool GetCellCheck(string sCellId)
        {
            bool bCheck = true;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bCheck = false;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }

        private void GetCellCheck(string sCellId, ref string sWipStat, ref string sWipSName, ref string sLotDetlTypeCode, ref string sLotDetlTypeName, ref string sColor, ref string sCstsLOT)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    sWipStat = SearchResult.Rows[0]["WIPSTAT"].ToString();
                    sWipSName = SearchResult.Rows[0]["WIPSNAME"].ToString();
                    sLotDetlTypeCode = SearchResult.Rows[0]["LOT_DETL_TYPE_CODE"].ToString();
                    sLotDetlTypeName = SearchResult.Rows[0]["LOT_DETL_TYPE_NAME"].ToString();
                    sColor = SearchResult.Rows[0]["COLOR"].ToString();
                    sCstsLOT = SearchResult.Rows[0]["CSTSLOT"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void UpdDummySelectYN(string sCellId)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CELL_ID", typeof(string));
                INDATA.Columns.Add("MDF_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CELL_ID"] = sCellId;
                dr["MDF_ID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_DUMMY_SELECT_Y", "INDATA", "OUTDATA", inDataSet, null);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool GetCellAssyCheck(string sCellId)
        {
            try
            {
                //TC_MLB_CELL 존재여부 확인
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_MLB_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return false;
        }

        private bool GetBadCellCheck(string sCellId, ref DataTable dtGrade)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                #region 삭제
                //TC_MLB_CELL_DEFECT을 사용하지 않으므로, 삭제 - 정종덕 2020/12/14
                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_BAD", "RQSTDT", "RSLTDT", RQSTDT);

                //if (SearchResult.Rows.Count > 0)
                //{
                //    //[%] 불량 Cell은 추가할 수 없습니다.
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0012", sCellId), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtCellInput.SelectAll();
                //            txtCellInput.Focus();
                //        }
                //    });
                //    return false;
                //} 
                #endregion

                //TC_CELL_SCRAP테이블 삭제하고, SUBLOT테이블의 SUBLOTSCRAP을 일단 사용하는걸로.. 2020/12/14 WITH 정종덕
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_F", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    if (SearchResult.Rows[0]["SUBLOTSCRAP"].ToString() == "Y")
                    {
                        //폐기 Cell은 추가할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0388"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellInput.SelectAll();
                                txtCellInput.Focus();
                            }
                        });
                        return false;
                    }
                }

                SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_A", "RQSTDT", "RSLTDT", RQSTDT);

                dtGrade = SearchResult;

                #region 트레이 생성 불가 대응 ( 동별공통코드 'FORM_DUMMY_JUDGE_BAN' 사용 )
                if (IsDummyJudgeBanCheck())
                {
                    DataTable SearchResult2 = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL", "RQSTDT", "RSLTDT", RQSTDT);

                    if(SearchResult.Rows.Count > 0 && SearchResult2.Rows.Count > 0)
                    {
                        //속성1 : 디가스 전, 속성2 : 디가스 후
                        string[] sAttribute = null;

                        if (SearchResult2.Rows[0]["ROUT_GR_CODE"].ToString().Trim().Equals("D"))
                            sAttribute = new string[] { "Y" };

                        if (SearchResult2.Rows[0]["ROUT_GR_CODE"].ToString().Trim().Equals("E"))
                            sAttribute = new string[] { null, "Y" };

                        
                        if (IsDummyJudgeBan("FORM_DUMMY_JUDGE_BAN", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim(), sAttribute) ||
                            IsDummyJudgeBan("FORM_DUMMY_JUDGE_BAN", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim(), sAttribute) )
                        {
                            //[%]등급 Cell은 추가할 수 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0361", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellInput.SelectAll();
                                    txtCellInput.Focus();
                                }
                            });
                            return false;
                        }
                    }                    
                }
                #endregion
                                

                //if (SearchResult.Rows.Count > 0 && (SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("W")
                //                                    || SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("E")
                //                                    || SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("C")
                //                                    || SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("V")))
                if (SearchResult.Rows.Count > 0 && IsDummyBanGrade(SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim(), "CMN"))
                {
                    if (!IsDummyCellGradeCheck(SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()))
                    {
                        //[%]등급 Cell은 추가할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0361", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellInput.SelectAll();
                                txtCellInput.Focus();
                            }
                        });
                        return false;
                    }
                }

                //200330 KJE : A등급 Dummy 금지 Validation
                //else if (!isAOK && SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("A"))
                else if (!isAOK && SearchResult.Rows.Count > 0 && IsDummyBanGrade(SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim(), "GOOD"))
                {
                    //[%]등급 Cell은 추가할 수 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0361", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }

                if (SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["MVDAY_JUDG"].ToString().Trim().Equals("NG"))
                {
                    //mv/Day NG Cell은 추가할 수 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0052"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    //Util.SetTextBoxReadOnly(txtTrayCellID, string.Empty); -> 이기 머꼬?
                    return false;
                }

                //Degas 전 T등급 추가 시 반드시 재작업 Route(자동) 체크되어있어야 함
                //Degas 후 저전압 T등급이 있어서 A5는 제외 처리
                //하드코딩 제거 후, 디가스 전만 조회되도록 수정
                //if (SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("T"))
                if (SearchResult.Rows.Count > 0 && IsDummyBanGrade(SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim(), "RWK_ROUTE_AUTO"))
                {
                    DataTable dtReq = new DataTable();
                    dtReq.TableName = "RQSTDT";
                    dtReq.Columns.Add("SUBLOTID", typeof(string));
                    dtReq.Columns.Add("DEGAS_B_A", typeof(string));

                    DataRow dr1 = dtReq.NewRow();
                    dr1["SUBLOTID"] = sCellId;
                    dr1["DEGAS_B_A"] = "D"; //기본라우트
                    dtReq.Rows.Add(dr1);

                    ShowLoadingIndicator();
                    DataTable dtDegasBA = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHK_DEGAS_B_A", "RQSTDT", "RSLTDT", dtReq);

                    if (dtDegasBA.Rows.Count > 0)
                    {
                        isReworkRouteChk = true;
                        chkDegasOnly.IsChecked = false; //모든 Route 체크 해제
                        if (chkReRoute.IsChecked == false)
                        {
                            //[%]등급 Cell 추가 시 반드시 재작업 Route(자동)에 체크해주세요.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0389", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellInput.SelectAll();
                                    txtCellInput.Focus();
                                }
                            });
                            return false;
                        }
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

        private bool IsDummyCellGradeCheck(string strCellGrade)
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
                dr["COM_TYPE_CODE"] = "FORM_DMY_CELL_GRD_PSBL_UI";
                dr["COM_CODE"] = strCellGrade;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
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

        private bool IsDummyDegasCellBanCheck()
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
                dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                dr["COM_CODE"] = "FORM_DUMMY_DEGAS_BAN_CHECK_YN";
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

        private bool IsDummyJudgeBanCheck()
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
                dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                dr["COM_CODE"] = "FORM_DUMMY_JUDGE_BAN_CHECK_YN";
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

        private bool IsDummyJudgeBan(string sCodeType, string sCodeName, string[] sAttribute)
        {
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
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsDummyBanGrade(string sFinlJudgCode, string sChkCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_DUMMY_BAN_GRADE";
                dr["CMCDIUSE"] = 'Y';
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    int iRstCnt = dtResult.Select($"ATTRIBUTE1 = '{ sChkCode }' AND CMCODE = '{ sFinlJudgCode }'").Count();

                    if (iRstCnt > 0)
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

        private bool GetReWorkCheck(string sCellId, string sRouteId, string sTrayType)
        {
            bool isSuccess = false;
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SUBLOTID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["ROUTID"] = sRouteId;
                dr["CST_TYPE_CODE"] = sTrayType;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CHK_REWORK_ROUTE", "INDATA", "OUTDATA", inDataSet, null);

                if (dsResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    //정상 CELL   
                    isSuccess = true;
                }
                else
                {
                    // 불량 CELL                    
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return isSuccess;
        }

        private bool GetFirstCellCheck(string sCellId, string sTrayType)
        {
            bool isSuccess = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    if (chkLineAuto.IsChecked == true)
                    {
                        cboDummyLineID.SelectedValue = SearchResult.Rows[0]["EQSGID"].ToString().Trim();
                    }

                    //200326 KJE : 저장Route 추가
                    if (chkStorageR.IsChecked == true)
                    {
                        SetStorageRoute(sCellId, sTrayType);
                    }
                    // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가
                    else if (chkLotMix_Model.IsChecked == true)
                    {
                        SetLotCheckMlodelLotRoute(sCellId, sTrayType);
                    }
                    //  2011.10.07 디가스 전후 라우트 구분
                    else if ((chkLotMix.IsChecked == true || chkDegasOnly.IsChecked == true) && !chkLowVoltRoute.IsChecked == true)
                    {
                        //SetlstRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString().Trim());
                        SetlstRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), sTrayType);
                    }
                    // 2013.06.19 W재작업 라우트 기준정보 조회
                    else if (chkLowVoltRoute.IsChecked == true)
                    {
                        if (chkLotMix.IsChecked == true)
                            //SetIsWReWorkRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString().Trim());
                            SetIsWReWorkRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), sTrayType);
                        else
                        {
                            //Lot 혼입 체크박스 비활성화 상태 입니다.\r\n활성화 후 재진행하여주십시요.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0051"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellInput.SelectAll();
                                    txtCellInput.Focus();
                                }
                            });
                            isSuccess = false;
                        }
                    }
                    // 2012.08.27 재작업 라우트 기준정보 조회
                    else if (chkReRoute.IsChecked == true)
                    {
                        //기준정보 없다면 데이터 삭제
                        //if (!SetIsReWorkRoute(sCellId, txtDummyTrayID.Text.ToString(), SearchResult.Rows[0]["LOTID"].ToString().Trim(), SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString().Trim()))
                        if (!SetIsReWorkRoute(sCellId, txtDummyTrayID.Text.ToString(), SearchResult.Rows[0]["LOTID"].ToString().Trim(), sTrayType))
                        {
                            SearchResult.Clear();
                            isSuccess = false;
                        }
                        

                    }
                    else
                    {
                        //SetlstRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString().Trim(), SearchResult.Rows[0]["ROUT_TYPE_CODE"].ToString().Trim());
                        SetlstRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), sTrayType, SearchResult.Rows[0]["ROUT_TYPE_CODE"].ToString().Trim());
                    }

                    // [CSR ID:2361359] LOTID 비교 완료후 LOTID 입력하기 위하여 정합성 확인 후로 수정함. //정상 
                    if (SearchResult.Rows.Count != 0)
                    {
                        txtDummyModel.Text = SearchResult.Rows[0]["MDLLOT_ID"].ToString();
                        txtDummyLotID.Text = SearchResult.Rows[0]["LOTID"].ToString().Trim();

                        //CA_LOT_NO, AN_LOT_NO 삭제
                        //if (string.IsNullOrEmpty(SearchResult.Rows[0]["CA_LOT_ID"].ToString()))
                        //{
                        //    txtDummyCaLot.Text = SearchResult.Rows[0]["LOTID"].ToString().Trim();
                        //}
                        //else
                        //{
                        //    txtDummyCaLot.Text = SearchResult.Rows[0]["CA_LOT_ID"].ToString().Trim();
                        //}

                        //if (string.IsNullOrEmpty(SearchResult.Rows[0]["AN_LOT_ID"].ToString()))
                        //{
                        //    txtDummyAnLot.Text = SearchResult.Rows[0]["LOTID"].ToString().Trim();
                        //}
                        //else
                        //{
                        //    txtDummyAnLot.Text = SearchResult.Rows[0]["AN_LOT_ID"].ToString().Trim();
                        //}

                        txtDummyProdCD.Text = SearchResult.Rows[0]["PRODID"].ToString().Trim();

                        if (string.IsNullOrEmpty(SearchResult.Rows[0]["RWK_ROUTID"].ToString()))  //TM_REWORK_ROUTE 정보로 기본 선택하기
                        {
                            if (lstRoute.Items.Count > 0)
                                lstRoute.SelectedValue = SearchResult.Rows[0]["RWK_ROUTID"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                isSuccess = false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return isSuccess;
        }

        private bool SetStorageRoute(string sCellId, string sTrayType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["CST_TYPE_CODE"] = sTrayType;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_STORAGE_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                //저장 Route정보가 없다면 알람 후 종료
                if (SearchResult.Rows.Count == 0)
                {
                    //해당 Cell에 대한 저장 Route가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0359"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }
                else
                {
                    lstRoute.ItemsSource = SearchResult.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMCDNAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return true;
        }

        // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가
        // LOT 혼재(모델기준)는 Degas 후 Tray만 생성할 수 있음 (다른 Tray와 LOT 혼입 방지)
        private bool SetLotCheckMlodelLotRoute(string sCellId, string sTrayTypeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["CST_TYPE_CODE"] = sTrayTypeCode;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_REWORK_ROUTE_BY_CELL_GRADE_AFTER_DEGAS", "RQSTDT", "RSLTDT", RQSTDT);

                //Degas 후 Route정보가 없다면 알람 후 종료
                if (SearchResult.Rows.Count == 0)
                {
                    //해당 Cell에 대한 Degas 후 Route가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0475"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }
                else
                {
                    lstRoute.ItemsSource = SearchResult.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMCDNAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return true;
        }

        private void SetlstRoute(string sLotId, string sTrayId, string sTrayTypeCode, string sRouteTypeCd = null)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotId;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dr["ROUT_TYPE_CODE"] = sRouteTypeCd;
                dr["CST_TYPE_CODE"] = sTrayTypeCode;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RETRIEVE_DUMMY_LOT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    lstRoute.ItemsSource = dtRslt.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMCDNAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetIsWReWorkRoute(string sLotID, string sTrayID, string sTrayTypeCode)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dr["CST_TYPE_CODE"] = sTrayTypeCode;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_SEARCH_W_REWORK_ROUTE", "RQSTDT", "RSLTDT", dtRqst);

                //W등급 재작업 기준정보가 없다면 알람 후 종료
                if (dtRslt.Rows.Count == 0)
                {
                    //저전압 재작업 라우트 기준정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0216"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return;
                }
                else
                {
                    lstRoute.ItemsSource = dtRslt.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMN_CD_NAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool SetIsReWorkRoute(string sCellId, string sTrayId, string sLotId, string sTrayType)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SUBLOTID", typeof(string));
                dt.Columns.Add("CST_TYPE_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["CST_TYPE_CODE"] = sTrayType;
                dr["LOTID"] = sLotId;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_REWORK_ROUTE", "INDATA", "OUTDATA", ds, null);

                //재작업 기준정보가 없다면 알람 후 종료
                if (dsResult.Tables["OUTDATA"].Rows.Count == 0)
                {
                    //재작업 라우트 기준정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0209"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }
                else if (dsResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("-1"))
                {
                    isReworkRouteChk = false;
                }

                lstRoute.ItemsSource = dsResult.Tables["OUTDATA"].AsDataView();
                lstRoute.SelectedValuePath = "ROUTID";
                lstRoute.DisplayMemberPath = "CMCDNAME";

                if (lstRoute.Items.Count > 0)
                    lstRoute.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return true;
        }

        private bool GetSecondLotCheck(string sCellId)
        {
            bool isSuccess = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 ||
                    dtRslt.Rows[0]["LOTID"].ToString().Length < 9 ||
                    txtDummyLotID.Text.Length < 9 ||
                    !dtRslt.Rows[0]["LOTID"].ToString().Substring(0, 9).Equals(txtDummyLotID.Text.Substring(0, 9)))
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return isSuccess;
        }

        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 START
        private bool GetSecondLotCheckDayGrLotID(string sCellId)
        {
            bool isSuccess = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 || !dtRslt.Rows[0]["DAY_GR_LOTID"].ToString().Equals(txtDummyLotID.Text.Substring(0, 8)))
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return isSuccess;
        }
        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 END

        // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가
        private bool GetSecondLotCheckModelLotID(string sCellId)
        {
            bool isSuccess = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 || !dtRslt.Rows[0]["MDLLOT_ID"].ToString().Equals(txtDummyLotID.Text.Substring(0, 3)))
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return isSuccess;
        }

        //LOTTYPE이 다를경우 Dummy 생성 불가
        private bool GetLotTypeCheck(string sCellID)
        {
            try
            {
                // 기존 첫번째 Row 와 비교시 첫번째가 Dummy 일때 비교불가하여 더미가 아닌 로우중 첫번째를 찾아 비교.
                string firstCellId = string.Empty;
                for (int row = 0; row < dgCell.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID")) == "0000000000") continue;

                    firstCellId = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID"));
                    break;
                }
                if (string.IsNullOrEmpty(firstCellId)) return true;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTLIST"] = firstCellId + "," + sCellID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_LOTTYPE", "RQSTDT", "RSLTDT", dtRqst);

                if(dtRslt.Rows.Count != 2) return true;
                else
                {
                    if (dtRslt.Rows[0]["LOTTYPE"].ToString().Equals(dtRslt.Rows[1]["LOTTYPE"].ToString()))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

        private void GetOCOPReturnCellCheck(string sCellID, ref string sRetval, ref string sSublotID, ref string sTypeCode, ref string sOCOPRtnFlag, ref string sPassFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_CHK_OCOP_RTN_CELL", "INDATA", "RSLTDT", dtRqst);

                sRetval = dtRslt.Rows[0]["RETVAL"].ToString();
                sSublotID = dtRslt.Rows[0]["SUBLOTID"].ToString();
                sTypeCode = dtRslt.Rows[0]["INSP_TYPE_CODE"].ToString();
                sOCOPRtnFlag = dtRslt.Rows[0]["OCOP_RTN_FLAG"].ToString();
                sPassFlag = dtRslt.Rows[0]["PASS_FLAG"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool GetLotDetlTypeCheck(string sCellID)
        {
            try
            {
                // 임시로 NB 만 적용 NB, GM, WA 공통이 확인될경우 제거.
                if (!LoginInfo.SYSID.Equals("GMES-F-NB")) return true;

                // 기존 첫번째 Row 와 비교시 첫번째가 Dummy 일때 비교불가하여 더미가 아닌 로우중 첫번째를 찾아 비교.
                string firstCellId = string.Empty;
                for (int row = 0; row < dgCell.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID")) == "0000000000") continue;

                    firstCellId = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, "SUBLOTID"));
                    break;
                }
                if (string.IsNullOrEmpty(firstCellId)) return true;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTLIST"] = firstCellId + "," + sCellID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_LOT_DETL_TYPE_CODE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 2) return true;
                else
                {
                    if (dtRslt.Rows[0]["LOT_DETL_TYPE_CODE"].ToString().Equals(dtRslt.Rows[1]["LOT_DETL_TYPE_CODE"].ToString()))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

        private void GetTrayInfo(bool pOpCheck)
        {
            try
            {
                this.ClearValidation();
                if (string.IsNullOrEmpty(txtTrayID.Text) || txtTrayID.Text.Length < 10)
                {
                    txtTrayID.SelectAll();
                    txtTrayID.Focus();
                    //Tray ID를 정확히 입력해주세요.
                    txtTrayID.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0071"));
                    return;
                }

                //if (string.IsNullOrEmpty(txtSTrayID.Text))
                //{
                //    //Tray를 먼저 입력해주세요.
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtTrayID.SelectAll();
                //            txtTrayID.Focus();
                //        }
                //    });
                //    return;
                //}

                if (CheckExistWipCell(txtTrayID.Text))
                {
                    btnStartOp.Visibility = Visibility.Visible;

                    //모든 Cell이 불량인 Tray입니다. Selector 공정 이력을 남기려면 변경 버튼을 눌러주세요.
                    Util.Alert("FM_ME_0450");  

                    return;
                }  

                btnStartOp.Visibility = Visibility.Hidden;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = txtTrayID.Text;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_RETRIEVE", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count == 0)
                {
                    //Tray 정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0078"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayID.SelectAll();
                            txtTrayID.Focus();
                        }
                    });
                    return;
                }
                txtSTrayID.Text = dtRslt.Rows[0]["CSTID"].ToString();
                txtOp.Text = dtRslt.Rows[0]["PROCNAME"].ToString();
                //txtNextOp.Text = dtRslt.Rows[0]["NEXT_PROCNAME"].ToString();
                txtStatus.Text = dtRslt.Rows[0]["WIPSNAME"].ToString();
                //출고예약상태가 분리됨.
                txtIssRsvFlag.Text = dtRslt.Rows[0]["ISS_RSV_FLAG"].ToString();
                txtTrayID.Foreground = Brushes.Black;

                if (dtRslt.Rows[0]["DUMMY_FLAG"].ToString().Equals("Y"))
                    txtTrayID.Foreground = Brushes.Blue;
                if (dtRslt.Rows[0]["SPCL_TYPE_CODE"].ToString().Equals("Y"))
                    txtTrayID.Foreground = Brushes.Red;
                else if (dtRslt.Rows[0]["SPCL_TYPE_CODE"].ToString().Equals("I"))
                    txtTrayID.Foreground = Brushes.DarkOrange;

                CommonCombo_Form _combo = new CommonCombo_Form();

                string[] sFilter = { dtRslt.Rows[0]["ROUTID"].ToString(), null, null, null }; //ROUTE_ID, EQP_KIND_CD, BYPASS_OP_YN,DOCV_FLAG
                _combo.SetCombo(cboOp, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "ROUTE_OP");
                cboOp.SelectedValue = dtRslt.Rows[0]["PROCID"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckExistWipCell(string pTrayID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = pTrayID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_CHK_EXIST_WIP_CELL", "RQSTDT", "RSLTDT", dtRqst);


                if (dtRslt.Rows.Count == 0) return false;

                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            { 
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        private void GetEqpCurTray()
        {
            try
            {
                txtLowerTray.Text = string.Empty;
                txtUpperTray.Text = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("ROW_LOC", typeof(string));
                dtRqst.Columns.Add("COL_LOC", typeof(string));
                dtRqst.Columns.Add("STG_LOC", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = Util.GetCondition(cboLane);
                dr["ROW_LOC"] = Util.GetCondition(cboRow);
                dr["COL_LOC"] = Util.GetCondition(cboCol);
                dr["STG_LOC"] = Util.GetCondition(cboStg);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CURRENT_TRAY", "RQSTDT", "RSLTDT", dtRqst);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    if (drRslt["CST_LOAD_LOCATION_CODE"].ToString().Equals("1"))
                    {
                        txtLowerTray.Text = drRslt["CSTID"].ToString();
                    }
                    else
                    {
                        txtUpperTray.Text = drRslt["CSTID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserID.Text = wndPerson.USERID;
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

        //2021-04-26 추가
        private void chkLineAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = false;
        }

        private void chkLineAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = true;
        }

        private string GetTrayCell(string sTray)  //Tray에 들어있는 Cell 리스트
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["CSTID"] = sTray;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_CELL_DATA_UI", "RQSTDT", "RSLTDT", dtRqst);
            
            string sCellID = string.Empty;

            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["SUBLOTID"]))) continue;

                sCellID += Util.NVC(dtRslt.Rows[i]["SUBLOTID"]);
                sCellID += "|";
            }

            return sCellID;
        }

        private void SetTrayCell(string sTray)  //Cell List 화면에 입력   
        {
            string[] sCellID = GetTrayCell(sTray).Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < sCellID.Length; i++)
            {
                DfctScanCell(sCellID[i]);
            }
        }


        private void CreateCNVTrayInfo(string sTray, string sEqpId, string sPortId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SCRTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PORTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SCRTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["CSTID"] = sTray;
                dr["EQPTID"] = sEqpId;
                dr["PORTID"] = sPortId;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_CMD_CREATE_EMPTY_TRAY_DECIDE_ON_DESTINATION_UI", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    //Util.MessageInfo("FM_ME_0160"); //생성완료하였습니다.
                }
                else
                {
                    //생성실패하였습니다. (Result Code : {0})
                    Util.MessageInfo("FM_ME_0159", dtRslt.Rows[0]["RETVAL"].ToString());
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion
        //2021-05-04 RowHeader 추가 - 생산팀 요청
        private void dgCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void chkReleaseDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
        }

        private void chkReleaseDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
        }

        private void cboSpecial_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(e.NewValue).Equals("N"))
            {
                txtSpecial.IsEnabled = false;
            }
            else
            {
                txtSpecial.IsEnabled = true;
            }
        }

        private void btnStartOp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = txtTrayID.Text.ToUpper().Trim();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SELECTOR_TRAY_MANUAL_START_END", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    Util.Alert("FM_ME_0215");  //저장하였습니다.

                    btnStartOp.Visibility = Visibility.Hidden;
                }
                else
                {
                    Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                }


            }
            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            txtTrayID.Text = string.Empty;
            txtSTrayID.Text = string.Empty;
            txtOp.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtIssRsvFlag.Text = string.Empty;
            cboOp.Text = string.Empty;
            chkReservation.IsChecked = false;
            cboLane.SelectedIndex = 0;
            cboRow.Text = string.Empty;
            cboCol.Text = string.Empty;
            cboStg.Text = string.Empty;
            txtUpperTray.Text = string.Empty;
            txtLowerTray.Text = string.Empty;


        }

        // 2023.03.23  이해령: Lot 혼입(Model 기준) 시 Lot ID 3자리 체크로직 추가
        private void chkLotMix_Checked(object sender, RoutedEventArgs e)
        {
            chkLotMix_Model.IsChecked = false;
        }

        private void chkLotMixModel_Checked(object sender, RoutedEventArgs e)
        {
            chkLotMix.IsChecked = false;

            Util.MessageConfirm("FM_ME_0476", (result) => { if (result == MessageBoxResult.Cancel) { chkLotMixModel_Cancel(); } });

        }

        private void chkLotMixModel_Cancel()
        {
            chkLotMix_Model.IsChecked = false;
        }

        private int BizWF_Check(string SubLotID)
        {
            int RetVal = -1;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TD_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = SubLotID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TD_FLAG"] = "Z";
                dtRqst.Rows.Add(dr);

                //ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SMPL_COM_CHK", "INDATA", "OUTDATA", dtRqst);

                RetVal = Convert.ToInt16(dtRslt.Rows[0]["RETVAL"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return RetVal;
            }

            return RetVal;
        }


        //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
        private bool CheckValidationRouteHPCD(string routeId)
        {
            // T등급 CELL 로 DUMMY TRAY 생성시 ROUTE상에 반드시 가압검사공정(HPCD) 존재해야함
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                DataRow dr = dtRqst.NewRow();                
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = routeId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_ROUTE_OP_HPCD_YN", "INDATA", "OUTDATA", dtRqst);

                if(dtRslt.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                
                return false;
            }

            return false;
        }

        //private bool CheckValidationRouteType(string routeId)
        //{
        //    // rout_type_code 조회, degas 전, 후
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "INDATA";
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("ROUTID", typeof(string));
               

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["ROUTID"] = routeId;
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_ROUTE_TYPE_CODE", "INDATA", "OUTDATA", dtRqst);

        //        if (dtRslt.Rows.Count > 0)
        //        {
        //            // R:DEGAS 전 재작업 ROUTE, T:DEGAS 전 TEST ROUTE
        //            if(Util.NVC(dtRslt.Rows[0]["ROUT_TYPE_CODE"]).ToString().Equals("R") )//|| Util.NVC(dtRslt.Rows[0]["ROUT_TYPE_CODE"]).ToString().Equals("T"))
        //            {
        //                return true;
        //            }                    
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);

        //        return false;
        //    }

        //    return false;
        //}

        //투입 Cell HPCD 공정 포함된 ROUTE Validation 기능 추가
        //private bool CheckChargeCell()
        //{
        //    //충전된 cell Check - Z,T 판정 외 N,J... 등 충전된 판정 Check

        //    bool IsChargeCell = false;
        //    for (int i = 0; i < dgCell.Rows.Count; i++)
        //    {
        //        if (!(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).ToString().Equals(CELLGRADE_T) ||
        //            Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).ToString().Equals(CELLGRADE_Z)) &&
        //            !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "FINL_JUDG_CODE")).ToString())
        //            )
        //        {
        //            IsChargeCell = true;
        //        }
        //    }
        //    return IsChargeCell;
        //}


        private DataTable CheckValidationEmptyTrayStatusByCSTID(string cstid)
        {
            //폐기 / 청소상태 공 TRAY 체크 추가

            DataTable dtRslt = new DataTable();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = cstid;
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EMPTY_TRAY_STATUS_BY_CSTID", "INDATA", "OUTDATA", dtRqst);

                return dtRslt;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool CheckValidationCellHpcdProc(string sublotid, string proc_gr_code)
        {
            //cell hpcd proc 확인
            bool bChecked = false;
            DataTable dtRslt = new DataTable();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sublotid;
                dr["PROC_GR_CODE"] = proc_gr_code;
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_CELL_HPCD_PROC", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    for(int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        //HPCD 공정 완료가 있으면 true
                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["WRK_STRT_DTTM"])) && !string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["WRK_END_DTTM"])))
                        {
                            bChecked = true;
                        }                        
                    }
                   
                }

                return bChecked;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool CheckValidationCellChargeProc(string sublotid, string proc_gr_code, string procid)
        {
            //cell charge proc 확인
            bool bChecked = false;
            DataTable dtRslt = new DataTable();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sublotid;
                dr["PROC_GR_CODE"] = proc_gr_code;
                if(!string.IsNullOrEmpty(procid))
                {
                    dr["PROCID"] = procid;
                }
                
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_CELL_CHARGE_PROC", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        //CHARGE 공정 시작 또는 완료가 있으면 true
                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["WRK_STRT_DTTM"])) || !string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["WRK_END_DTTM"])))
                        {
                            bChecked = true;
                        }
                    }

                }

                return bChecked;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable CheckValidationRoutGRTypeByCell(string sublotid)
        {
            
            //디가스 전후 확인을 위한 ROUT_GR_TYPE 조회
            DataTable dtRslt = new DataTable();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sublotid;
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_ROUT_GR_TYPE_BY_CELLID", "INDATA", "OUTDATA", dtRqst);

                return dtRslt;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        private DataTable CheckValidationCstTypeCode(string cst_type_code)
        {
            //DA_BAS_SEL_TB_MMD_FCS_CST_TYPE_F
            //디가스 전후 확인을 위한 ROUT_GR_TYPE 조회
            DataTable dtRslt = new DataTable();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CST_TYPE_CODE"] = cst_type_code;
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHECK_CST_TYPE_CODE", "INDATA", "OUTDATA", dtRqst);

                return dtRslt;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 고온에이징이 포함된 용량 라우트 Validation 2024.05.09

        private bool CheckProcByCell(string PROC_GR_CODE, string SUBLOTID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROC_GR_CODE"] = PROC_GR_CODE;
                dr["SUBLOTID"] = SUBLOTID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROC_HIST_BY_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return false;
            }

            return false;
        }

        private bool CheckProcByRoute(string ROUT_GR_CODE, string PROC_GR_CODE, string ROUTID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("ROUT_GR_CODE", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUT_GR_CODE"] = ROUT_GR_CODE;
                dr["PROC_GR_CODE"] = PROC_GR_CODE;
                dr["ROUTID"] = ROUTID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROC_BY_ROUTE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                
                return false;
            }
            
            return false;
        }

        #endregion 고온에이징이 포함된 용량 라우트 Validation 2024.05.09

        #region 활성화 입고확정 및 에이징 착공/완공 임시 추가 (수동물류 대응 건)

        #region 활성화 입고확정
        private void txtTrayAssyConfirm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sTrayID = txtTrayAssyConfirm.Text.ToUpper().Trim();

                    if (string.IsNullOrEmpty(sTrayID))
                        return;

                    if (sTrayID.Length != 10)
                    {
                        //잘못된 ID입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    if (dgTrayAssyConfirm.GetRowCount() >= 5)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Up to 5 cases can be processed at a time", null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    for (int i = 0; i < dgTrayAssyConfirm.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgTrayAssyConfirm.Rows[i].DataItem, "TRAY_ID").ToString().ToUpper().Trim() == sTrayID)
                        {
                            //이미 스캔한 ID 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0193"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                            return;
                        }
                    }

                    if (dgTrayAssyConfirm.GetRowCount() == 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("TRAY_ID");

                        DataRow dr = dt.NewRow();
                        dr["TRAY_ID"] = txtTrayAssyConfirm.Text.Trim();

                        dt.Rows.Add(dr);

                        Util.GridSetData(dgTrayAssyConfirm, dt, FrameOperation, true);
                    }
                    else
                    {
                        DataTable dt_origin = DataTableConverter.Convert(dgTrayAssyConfirm.ItemsSource);

                        DataTable dt = new DataTable();
                        dt.Columns.Add("TRAY_ID");

                        DataRow dr = dt.NewRow();
                        dr["TRAY_ID"] = txtTrayAssyConfirm.Text.Trim();

                        dt.Rows.Add(dr);

                        dt_origin.Merge(dt, true, MissingSchemaAction.Ignore);
                        Util.GridSetData(dgTrayAssyConfirm, dt_origin, FrameOperation, true);
                    }

                    dgTrayAssyConfirm.SelectRow(dgTrayAssyConfirm.Rows.Count - 1);
                    txtTrayAssyConfirm.Clear();
                    txtTrayAssyConfirm.Focus();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtTrayAssyConfirm_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnTrayAssyConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrayAssyConfirm.GetRowCount() == 0)
            {
                //Tray를 먼저 입력해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                return;
            }

            //선택된 TRAY를 확정하시겠습니까?
            Util.MessageConfirm("SFU3246", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int cnt_OK = 0;
                        int cnt_origin = dgTrayAssyConfirm.GetRowCount();

                        for (int i = 0; i < dgTrayAssyConfirm.GetRowCount(); i++)
                        {
                            try
                            {
                                string sPackageEqptID = null;

                                #region 조립 설비 ID 조회
                                DataTable dtRqst1 = new DataTable();
                                dtRqst1.TableName = "INDATA";
                                dtRqst1.Columns.Add("CSTID", typeof(string));

                                DataRow drRqst1 = dtRqst1.NewRow();
                                drRqst1["CSTID"] = DataTableConverter.GetValue(dgTrayAssyConfirm.Rows[i].DataItem, "TRAY_ID").ToString().ToUpper().Trim();

                                dtRqst1.Rows.Add(drRqst1);

                                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FORM_INPUT_LOT_HIST_CSTID", "RQSTDT", "RSLTDT", dtRqst1);

                                if (dtRslt1.Rows.Count > 0)
                                {
                                    sPackageEqptID = dtRslt1.Rows[0]["EQPTID"].ToString().ToUpper().Trim();
                                }
                                #endregion

                                if (!string.IsNullOrEmpty(sPackageEqptID))
                                {
                                    DataSet dsRqst = new DataSet();

                                    DataTable dtINDATA = new DataTable();
                                    dtINDATA.TableName = "INDATA";
                                    dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                                    dtINDATA.Columns.Add("IFMODE", typeof(string));
                                    dtINDATA.Columns.Add("EQPTID", typeof(string));
                                    dtINDATA.Columns.Add("USERID", typeof(string));

                                    DataRow drINDATA = dtINDATA.NewRow();
                                    drINDATA["SRCTYPE"] = "UI";
                                    drINDATA["IFMODE"] = "OFF";
                                    drINDATA["EQPTID"] = sPackageEqptID; //조립 패키지 설비 ID
                                    drINDATA["USERID"] = LoginInfo.USERID;

                                    dtINDATA.Rows.Add(drINDATA);
                                    dsRqst.Tables.Add(dtINDATA);

                                    DataTable dtIN_CST = new DataTable();
                                    dtIN_CST.TableName = "IN_CST";
                                    dtIN_CST.Columns.Add("CSTID", typeof(string));

                                    DataRow drIN_CST = dtIN_CST.NewRow();
                                    drIN_CST["CSTID"] = DataTableConverter.GetValue(dgTrayAssyConfirm.Rows[i].DataItem, "TRAY_ID").ToString().ToUpper().Trim();

                                    dtIN_CST.Rows.Add(drIN_CST);
                                    dsRqst.Tables.Add(dtIN_CST);

                                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_ASSY_OUT_TRAY_INPUT", "INDATA, IN_CST", "OUTDATA", dsRqst);

                                    if (dsRslt.Tables.Count > 0 && dsRslt.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0")) //정상 처리된 Tray는 리스트에서 제거.
                                    {
                                        DataTable dt = DataTableConverter.Convert(dgTrayAssyConfirm.ItemsSource);
                                        dt.Rows.RemoveAt(i);
                                        Util.GridSetData(dgTrayAssyConfirm, dt, this.FrameOperation);

                                        i--; //트레이 1개 제거되었기 때문에 인덱스 조정
                                        cnt_OK++;
                                    }
                                }
                            }
                            catch (Exception ex2)
                            {
                                continue;
                            }
                        }

                        if (cnt_OK == cnt_origin)
                        {
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                        }
                        else
                        {
                            Util.Alert("SFU11000"); //정상 처리된 Tray는 리스트에서 제거되었습니다.\r\n리스트에 남은 Tray의 정보를 확인해 주세요.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnTrayAssyConfirmClear_Click(object sender, RoutedEventArgs e)
        {
            //초기화 하시겠습니까?
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                dgTrayAssyConfirm.ItemsSource = null;
            });
        }

        private void btnTrayAssyConfirmRemove_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dt = DataTableConverter.Convert(dgTrayAssyConfirm.ItemsSource);
            dt.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);
        }

        private void dgTrayAssyConfirm_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTrayAssyConfirm_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

        }
        #endregion

        #region Aging 착공
        private void txtTrayAgingStart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sTrayID = txtTrayAgingStart.Text.ToUpper().Trim();

                    if (string.IsNullOrEmpty(sTrayID))
                        return;

                    if (sTrayID.Length != 10)
                    {
                        //잘못된 ID입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    if (dgTrayAgingStart.GetRowCount() >= 20)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Up to 20 cases can be processed at a time", null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    for (int i = 0; i < dgTrayAgingStart.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgTrayAgingStart.Rows[i].DataItem, "CSTID").ToString().ToUpper().Trim() == sTrayID)
                        {
                            //이미 스캔한 ID 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0193"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                            return;
                        }
                    }

                    DataTable dt = new DataTable();
                    dt.TableName = "INDATA";
                    dt.Columns.Add("CSTID");

                    DataRow dr = dt.NewRow();
                    dr["CSTID"] = txtTrayAgingStart.Text.Trim();

                    dt.Rows.Add(dr);

                    DataSet inDataSet = new DataSet();
                    inDataSet.Tables.Add(dt);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_LOAD_TRAY_INFO", "INDATA", "RET_TRAY_PROCESS,RET_DELTA_OCV,RET_TRAY_INFO", inDataSet);

                    if (dsRslt == null || !dsRslt.Tables.Contains("RET_TRAY_INFO") || dsRslt.Tables["RET_TRAY_INFO"].Rows.Count == 0)
                    {
                        Util.Alert("FM_ME_0078"); //Tray 정보가 존재하지 않습니다.
                        return;
                    }
                    
                    DataTable dtRslt = dsRslt.Tables["RET_TRAY_INFO"] as DataTable;

                    string sProc_Type_Code = dtRslt.Rows[0]["PROCID"].ToString().Substring(2, 1);
                    if (!sProc_Type_Code.Equals("9")
                     && !sProc_Type_Code.Equals("4")
                     && !sProc_Type_Code.Equals("3")
                     && !sProc_Type_Code.Equals("7"))
                    {
                        Util.Alert("FM_ME_0015"); //Aging 공정이 아닌 Tray가 있습니다.
                        return;
                    }

                    if (!dtRslt.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        Util.Alert("SFU2063"); //재공상태를 확인해주세요.
                        return;
                    }
                    else
                    {
                        dtRslt.Rows[0]["WIPSTAT"] = ObjectDic.Instance.GetObjectName("대기");

                        if (dgTrayAgingStart.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgTrayAgingStart, dtRslt, FrameOperation, false);
                        }
                        else
                        {
                            DataTable dt_origin = DataTableConverter.Convert(dgTrayAgingStart.ItemsSource);

                            //첫 행의 공정ID와 입력한 Tray의 공정ID가 같은지 확인
                            if (dt_origin.Rows[0]["PROCID"].ToString() != dtRslt.Rows[0]["PROCID"].ToString())
                            {
                                Util.Alert("SFU1446"); //같은 공정이 아닙니다.
                                return;
                            }

                            dt_origin.Merge(dtRslt, true, MissingSchemaAction.Ignore);
                            Util.GridSetData(dgTrayAgingStart, dt_origin, FrameOperation, false);
                        }
                    }

                    dgTrayAgingStart.SelectRow(dgTrayAgingStart.Rows.Count - 1);
                    txtTrayAgingStart.Clear();
                    txtTrayAgingStart.Focus();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtTrayAgingStart_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnTrayAgingStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrayAgingStart.GetRowCount() == 0)
            {
                //Tray를 먼저 입력해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                return;
            }

            //Aging 입고처리 하시겠습니까?
            Util.MessageConfirm("FM_ME_0326", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int cnt_OK = 0;
                        int cnt_origin = dgTrayAgingStart.GetRowCount();

                        for (int i = 0; i < dgTrayAgingStart.GetRowCount(); i++)
                        {
                            try
                            {
                                string strEqptID = null;
                                switch (DataTableConverter.GetValue(dgTrayAgingStart.Rows[i].DataItem, "PROCID").ToString().Substring(2, 1)) //공정 그룹 코드에 맞게 설비ID 지정
                                {
                                    case "9":
                                        strEqptID = !string.IsNullOrEmpty(sAgingEqptID_Pre) ? sAgingEqptID_Pre : "J1FSTO11301"; //Pre Aging 설비 ID
                                        break;
                                    case "4":
                                        strEqptID = !string.IsNullOrEmpty(sAgingEqptID_High) ? sAgingEqptID_High : "J1FSTO11311"; //High Aging 설비 ID
                                        break;
                                    case "3":
                                        strEqptID = !string.IsNullOrEmpty(sAgingEqptID_Normal) ? sAgingEqptID_Normal : "J1FSTO11315"; //Normal Aging 설비 ID
                                        break;
                                    case "7":
                                        strEqptID = !string.IsNullOrEmpty(sAgingEqptID_Ship) ? sAgingEqptID_Ship : "J1FSTO12307"; //Ship Aging 설비 ID
                                        break;
                                    default:
                                        strEqptID = null;
                                        break;
                                }

                                DataTable dtRqst = new DataTable();
                                dtRqst.TableName = "INDATA";
                                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                                dtRqst.Columns.Add("IFMODE", typeof(string));
                                dtRqst.Columns.Add("AREAID", typeof(string));
                                dtRqst.Columns.Add("CSTID", typeof(string));
                                dtRqst.Columns.Add("EQPTID", typeof(string));
                                dtRqst.Columns.Add("USERID", typeof(string));
                                dtRqst.Columns.Add("RACK_ID", typeof(string));
                                dtRqst.Columns.Add("PROCID", typeof(string));
                                dtRqst.Columns.Add("MANUAL_IN_TIME", typeof(DateTime));

                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["CSTID"] = DataTableConverter.GetValue(dgTrayAgingStart.Rows[i].DataItem, "CSTID").ToString().ToUpper().Trim();
                                dr["EQPTID"] = strEqptID;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["RACK_ID"] = null;
                                dr["PROCID"] = DataTableConverter.GetValue(dgTrayAgingStart.Rows[i].DataItem, "PROCID").ToString().ToUpper().Trim();
                                dr["MANUAL_IN_TIME"] = DateTime.Now;
                                dtRqst.Rows.Add(dr);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_AGING_MANUAL_INPUT_UI", "INDATA", "OUTDATA", dtRqst);

                                if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //정상 처리된 Tray는 리스트에서 제거.
                                {
                                    DataTable dt = DataTableConverter.Convert(dgTrayAgingStart.ItemsSource);
                                    dt.Rows.RemoveAt(i);
                                    Util.GridSetData(dgTrayAgingStart, dt, this.FrameOperation);

                                    i--; //트레이 1개 제거되었기 때문에 인덱스 조정
                                    cnt_OK++;
                                }
                            }
                            catch (Exception ex2)
                            {
                                continue;
                            }
                        }

                        if (cnt_OK == cnt_origin)
                        {
                            Util.AlertInfo("FM_ME_0327"); //Aging 입고처리 완료하였습니다.
                        }
                        else
                        {
                            Util.Alert("SFU11000"); //정상 처리된 Tray는 리스트에서 제거되었습니다.\r\n리스트에 남은 Tray의 정보를 확인해 주세요.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnTrayAgingStartClear_Click(object sender, RoutedEventArgs e)
        {
            //초기화 하시겠습니까?
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                dgTrayAgingStart.ItemsSource = null;
            });
        }

        private void btnTrayAgingStartRemove_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dt = DataTableConverter.Convert(dgTrayAgingStart.ItemsSource);
            dt.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);
        }

        private void dgTrayAgingStart_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTrayAgingStart_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

        }
        #endregion

        #region Aging 완공
        private void txtTrayAgingEnd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sTrayID = txtTrayAgingEnd.Text.ToUpper().Trim();

                    if (string.IsNullOrEmpty(sTrayID))
                        return;

                    if (sTrayID.Length != 10)
                    {
                        //잘못된 ID입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    if (dgTrayAgingEnd.GetRowCount() >= 20)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Up to 20 cases can be processed at a time", null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                        return;
                    }

                    for (int i = 0; i < dgTrayAgingEnd.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgTrayAgingEnd.Rows[i].DataItem, "CSTID").ToString().ToUpper().Trim() == sTrayID)
                        {
                            //이미 스캔한 ID 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0193"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                            return;
                        }
                    }

                    DataTable dt = new DataTable();
                    dt.TableName = "INDATA";
                    dt.Columns.Add("CSTID");

                    DataRow dr = dt.NewRow();
                    dr["CSTID"] = txtTrayAgingEnd.Text.Trim();

                    dt.Rows.Add(dr);

                    DataSet inDataSet = new DataSet();
                    inDataSet.Tables.Add(dt);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_LOAD_TRAY_INFO", "INDATA", "RET_TRAY_PROCESS,RET_DELTA_OCV,RET_TRAY_INFO", inDataSet);

                    if (dsRslt == null || !dsRslt.Tables.Contains("RET_TRAY_INFO") || dsRslt.Tables["RET_TRAY_INFO"].Rows.Count == 0)
                    {
                        Util.Alert("FM_ME_0078"); //Tray 정보가 존재하지 않습니다.
                        return;
                    }

                    DataTable dtRslt = dsRslt.Tables["RET_TRAY_INFO"] as DataTable;

                    string sProc_Type_Code = dtRslt.Rows[0]["PROCID"].ToString().Substring(2, 1);
                    if (!sProc_Type_Code.Equals("9")
                     && !sProc_Type_Code.Equals("4")
                     && !sProc_Type_Code.Equals("3")
                     && !sProc_Type_Code.Equals("7"))
                    {
                        Util.Alert("FM_ME_0015"); //Aging 공정이 아닌 Tray가 있습니다.
                        return;
                    }

                    if (!dtRslt.Rows[0]["WIPSTAT"].ToString().Equals("PROC"))
                    {
                        Util.Alert("SFU2063"); //재공상태를 확인해주세요.
                        return;
                    }
                    else
                    {
                        dtRslt.Rows[0]["WIPSTAT"] = ObjectDic.Instance.GetObjectName("WORKING");

                        if (dgTrayAgingEnd.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgTrayAgingEnd, dtRslt, FrameOperation, false);
                        }
                        else
                        {
                            DataTable dt_origin = DataTableConverter.Convert(dgTrayAgingEnd.ItemsSource);

                            //첫 행의 공정ID와 입력한 Tray의 공정ID가 같은지 확인
                            if (dt_origin.Rows[0]["PROCID"].ToString() != dtRslt.Rows[0]["PROCID"].ToString())
                            {
                                Util.Alert("SFU1446"); //같은 공정이 아닙니다.
                                return;
                            }

                            dt_origin.Merge(dtRslt, true, MissingSchemaAction.Ignore);
                            Util.GridSetData(dgTrayAgingEnd, dt_origin, FrameOperation, false);
                        }
                    }

                    dgTrayAgingEnd.SelectRow(dgTrayAgingEnd.Rows.Count - 1);
                    txtTrayAgingEnd.Clear();
                    txtTrayAgingEnd.Focus();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtTrayAgingEnd_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnTrayAgingEnd_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrayAgingEnd.GetRowCount() == 0)
            {
                //Tray를 먼저 입력해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                return;
            }

            //완공처리 하시겠습니까?
            Util.MessageConfirm("SFU1744", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int cnt_OK = 0;
                        int cnt_origin = dgTrayAgingEnd.GetRowCount();

                        for (int i = 0; i < dgTrayAgingEnd.GetRowCount(); i++)
                        {
                            try
                            {
                                string strEqptID = null;
                                switch (DataTableConverter.GetValue(dgTrayAgingEnd.Rows[i].DataItem, "PROCID").ToString().Substring(2, 1)) //공정 그룹 코드에 맞게 설비ID 지정
                                {
                                    case "9":
                                        strEqptID = sAgingEqptID_Pre;
                                        break;
                                    case "4":
                                        strEqptID = sAgingEqptID_High;
                                        break;
                                    case "3":
                                        strEqptID = sAgingEqptID_Normal;
                                        break;
                                    case "7":
                                        strEqptID = sAgingEqptID_Ship;
                                        break;
                                    default:
                                        strEqptID = null;
                                        break;
                                }

                                DataTable dtRqst = new DataTable();
                                dtRqst.TableName = "INDATA";
                                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                                dtRqst.Columns.Add("IFMODE", typeof(string));
                                dtRqst.Columns.Add("AREAID", typeof(string));
                                dtRqst.Columns.Add("CSTID", typeof(string));
                                dtRqst.Columns.Add("EQPTID", typeof(string));
                                dtRqst.Columns.Add("USERID", typeof(string));
                                dtRqst.Columns.Add("RACK_ID", typeof(string));
                                dtRqst.Columns.Add("PROCID", typeof(string));

                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["CSTID"] = DataTableConverter.GetValue(dgTrayAgingEnd.Rows[i].DataItem, "CSTID").ToString().ToUpper().Trim();
                                dr["EQPTID"] = strEqptID;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["RACK_ID"] = null; //RACK_ID;
                                dr["PROCID"] = DataTableConverter.GetValue(dgTrayAgingEnd.Rows[i].DataItem, "PROCID").ToString().ToUpper().Trim();
                                dtRqst.Rows.Add(dr);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_AGING_MANUAL_OUTPUT_UI", "INDATA", "OUTDATA", dtRqst);

                                if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //정상 처리된 Tray는 리스트에서 제거.
                                {
                                    DataTable dt = DataTableConverter.Convert(dgTrayAgingEnd.ItemsSource);
                                    dt.Rows.RemoveAt(i);
                                    Util.GridSetData(dgTrayAgingEnd, dt, this.FrameOperation);

                                    i--; //트레이 1개 제거되었기 때문에 인덱스 조정
                                    cnt_OK++;
                                }
                            }
                            catch (Exception ex2)
                            {
                                continue;
                            }
                        }

                        if (cnt_OK == cnt_origin)
                        {
                            Util.AlertInfo("SFU1742"); //완공되었습니다.
                        }
                        else
                        {
                            Util.Alert("SFU11000"); //정상 처리된 Tray는 리스트에서 제거되었습니다.\r\n리스트에 남은 Tray의 정보를 확인해 주세요.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnTrayAgingEndClear_Click(object sender, RoutedEventArgs e)
        {
            //초기화 하시겠습니까?
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                dgTrayAgingEnd.ItemsSource = null;
            });
        }

        private void btnTrayAgingEndRemove_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dt = DataTableConverter.Convert(dgTrayAgingEnd.ItemsSource);
            dt.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);
        }

        private void dgTrayAgingEnd_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTrayAgingEnd_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {

        }
        #endregion

        #endregion
    }
}
