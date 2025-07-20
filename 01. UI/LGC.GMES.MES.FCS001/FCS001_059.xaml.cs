/********************************************************************************************************************************
 Created Date : 2020.11.27
      Creator : 박준규
   Decription : 특성기 불량 셀 등록
---------------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.11.27  DEVELOPER : 박준규
  2022.06.30  조영대 : Cell ID 색상 변경, Excell 업로드
  2022.07.02  조영대 : 불량 등록 비즈, 불량 복구 비즈 수정.
  2022.07.13  조영대 : 조회 탭 수정
  2022.07.27  이정미 : 조회 Tab 선택 복구 호출 Biz 변경, 등록 Tab 조회 시간 변경
  2022.07.27  이형대 : 복구이력 탭 수정
  2022.08.04  이정미 : 등록 Tab 불량 Cell 등록 조건 추가 
  2022.08.05  이정미 : 복구 Tab 불량 Cell 복구 INDATA 오류 수정
  2022.08.10  이정미 : CalDate 함수 오류 수정
  2022.08.19  이정미 : 복구이력조회 Tab 조회 INDATA 오류 수정, 복구 Tab Cell ID 입력 불가 오류 및 조회 오류 수정,
                       복구 Tab 불량 Cell 복구 시 등록 Tab 셀 복구되는 오류 수정, 등록 Tab 및 복구 Tab 등록 조건 추가, 
                       알림 메시지 추가, 디자인 수정
  2022.08.26  이제섭 : 불량 등록 시, Cell의 투입일자와 UI에서 선택한 일자 비교 로직 추가
  2022.09.01  이정미 : 불량 등록 등록 전기일자 선택 시 마감 Validation 추가 기존 날짜 조건 모두 Disable
  2022.12.06  조영대 : 복구탭 불량 Cell 복구 시 여러개가 아닌 1개씩 실행하게 수정.
  2022.12.12  이정미 : 복구 Tab 불량명 추가 
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.02  형준우 : 조회 버튼에 대한 DA문의 LANGID 추가
  2023.01.12  형준우 : Cell 복구 시, 비즈 에러나는 부분에 대해서 에러메세지 출력하도록 수정
  2023.02.17  이정미 : 등록 Tab 생성자 입력 추가, 복구 Tab 생성자 입력 추가 및 팝업창 확인 버튼 클릭시 복구 가능하도록 수정,
                       조회Tab 생성자 조회 조건 수정, 복구 이력 Tab 생성자 조회 조건 수정 및 생성자, 생성일시 컬럼 추가 
  2023.05.09  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.05.24  주훈종 : ESGM 요청으로 DEGAS 불량 셀도 같은 복구탭을 이용하여 처리할 수 있도록 수정
  2023.08.10  김동훈 : BizWF 로직 추가
  2024.07.30  Ivan P : E20240613-001552 Add capability to search after paste in txtSearchCellId (GDC)
  2024.08.08  김수용 : E20240716-000683 SSO Login Info Check logic 삭제
  2024.08.12  주경호 : E20240422-000378 Julia -Form#1/2/3 /4-> Add easy possibility to change Vision Apperance defect function
  2024.11.09  복현수 : MES 리빌딩 PJT : datetime -> string 타입 변경
  2025.01.08  배준호 : MES 리빌딩 PJT : 조회 탭에서 복구처리할때, 다건으로 처리되도록 변경(btnSearchRecover_Click)
*********************************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Text;
using System.Linq;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS001_059 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string gsProcessType = string.Empty;
        private string gsEqpKind = string.Empty;
        private string sCalTime = string.Empty;
        private DateTime SelCaldate = Convert.ToDateTime("9999-01-01 00:00:00");
        private string sShift = string.Empty;
        private string sRowCount = string.Empty; // 2024.07.23 Ivan P Add sRowCount to store max number of cell can be inputted in Cell ID textbox

        private Boolean isCellChgLossTab = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();

        public FCS001_059()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gsEqpKind))
                gsEqpKind = GetLaneIDForMenu(LoginInfo.CFG_MENUID);

            switch (gsEqpKind)
            {
                case "D": //DEGAS
                    //gsEqpKind = "D";
                    gsProcessType = "B";
                    break;

                case "5": //특성기
                    //gsEqpKind = "5";
                    gsProcessType = "5";
                    break;

                case "6": //SELECTOR
                    //gsEqpKind = "6";
                    gsProcessType = "A,C";
                    break;

                case "J": //JIG
                    //gsEqpKind = "J";
                    gsProcessType = "E";
                    break;

                default:
                    gsProcessType = "B";
                    gsEqpKind = "D";
                    break;
            }

            // 2024.07.23 Ivan P Add max number of cell can be inputted in the Cell ID textbox
            sRowCount = GetCommonCode("SELECT_ROW_COUNT", "SEARCH_DEFECT_CELL");
            InitControl();

            txtUserID.Text = LoginInfo.USERID;
            txtUserName.Text = LoginInfo.USERNAME;
            txtRestoreUserID.Text = LoginInfo.USERID;
            txtRestoreUserName.Text = LoginInfo.USERNAME;

            Loaded -= UserControl_Loaded;
        }

        private void InitControl()
        {
            dtpFromTimeT1.DateTime = DateTime.Now.AddDays(-1);
            dgSearch.AddCheckAll();

            isCellChgLossTab = GetCellChgLossTab();

            if (isCellChgLossTab == false)
            {
                Chg_Loss.Visibility = Visibility.Hidden;
            }

            InitCombo();
        }

        private void InitCombo()
        {
            string[] sFilterEqp = { gsEqpKind, null };
            string[] sFilterSearchShift = { "STM", null, null, null, null, null };
            string[] sFilterDefect = { gsEqpKind };
            string[] sFilterSearchDefect = { null, gsEqpKind };
            string[] sFilterOp = { "CLO", "A,B,C,D" };
            string[] sFilterEqpInput = { "EQP_INPUT_YN" };

            CommonCombo_Form _combo = new CommonCombo_Form();

            //C1ComboBox[] cboLineChild = { cboModel };

            if (gsEqpKind.Equals("5") || gsEqpKind.Equals("D"))   //Degas EOL만 설비 Display
            {
                C1ComboBox[] cboSearchLaneChild = { cboSearchEqp };
                _combo.SetCombo(cboSearchLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchLaneChild, cbChild: cboSearchLaneChild, sCase: "LANE");

                C1ComboBox[] cboSearchEqpParent = { cboSearchLane };
                _combo.SetCombo(cboSearchEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");

                C1ComboBox[] cboHistLaneChild = { cboHistEqp };
                _combo.SetCombo(cboHistLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistLaneChild, cbChild: cboHistLaneChild, sCase: "LANE");

                C1ComboBox[] cboHistEqpParent = { cboHistLane };
                _combo.SetCombo(cboHistEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");
            }

            C1ComboBox[] cboDefectParent = { cboInsertGroup };
            _combo.SetCombo(cboInsertDefect, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "CELL_DEFECT", cbParent: cboDefectParent);
            C1ComboBox[] cboGroupChild = { cboInsertDefect };
            _combo.SetCombo(cboInsertGroup, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "DEFECT_KIND", cbChild: cboGroupChild);

            _combo.SetCombo(cboSearchModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");

            string[] arrColumnShift = { "LANGID", "AREAID" };
            string[] arrConditionShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboSearchShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnShift, arrConditionShift, CommonCombo.ComboStatus.ALL, true);

            string[] arrColumn = { "LANGID", "USE_FLAG", "DFCT_TYPE_CODE", "DFCT_GR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, "Y", null, gsEqpKind };
            cboSearchDefect.SelectedValuePath = "DFCT_CODE"; cboSearchDefect.DisplayMemberPath = "DFCT_NAME";
            cboSearchDefect.SetDataComboItem("DA_BAS_SEL_COMBO_TM_CELL_DEFECT", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);

            cboSearchOp.SetCommonCode("FORM_DFCT_GR_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);
            cboEqpInputYN.SetCommonCode("EQPT_INPUT_AVAIL_FLAG", CommonCombo.ComboStatus.ALL, true);

            _combo.SetCombo(cboHistShift, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterSearchShift, sCase: "CMN_WITH_OPTION");
            _combo.SetCombo(cboHistModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");
            _combo.SetCombo(cboHistDefect, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterSearchDefect, sCase: "CELL_DEFECT");
            //_combo.SetCombo(cboHistOp, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterOp, sCase: "CMN");

            //string[] sFilterYN = { "USE_YN" };
            //_combo.SetCombo(cboSelectYN, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilterYN, sCase: "CMN", bCodeDisplay: true);

            // 복구 이력 탭의 콤보박스 설정
            cboHistOp.SetCommonCode("FORM_DFCT_GR_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);

            string[] arrColumnHistShift = { "LANGID", "AREAID" };
            string[] arrConditionHistShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboHistShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnHistShift, arrConditionHistShift, CommonCombo.ComboStatus.ALL, true);

            // 신규 탭 추가 관련 default 값 설정
            C1ComboBox[] cboGroupChild1 = { cboInsertDefect1 };
            _combo.SetCombo(cboInsertGroup1, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "DEFECT_KIND", cbChild: cboGroupChild1);

            C1ComboBox[] cboDefectParent1 = { cboInsertGroup1 };
            _combo.SetCombo(cboInsertDefect1, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "CELL_DEFECT", cbParent: cboDefectParent1);

            // 1번째가 외관불량으로 강제 세팅 후 disable 하여 선택 금지함
            cboInsertGroup1.SelectedIndex = 2;
        }

        private string GetLaneIDForMenu(string sMenuID)
        {
            string sLaneID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_CHARGE_MENU_ID";
                dr["CMCODE"] = sMenuID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
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
            return sLaneID;
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //fpsInsert.ActiveSheet.Columns["CELL_ID"].Locked = true;
            GetInsertCellDataFromSpread(dgInputList, dgInputList, dgInputList.Columns["SUBLOTID"].Index);
        }

        private void GetInsertCellDataFromSpread(C1DataGrid fromSpread, C1DataGrid toSpread, int indexCellCol)
        {
            string sCellID = string.Empty;

            for (int iRow = 0; iRow < fromSpread.GetRowCount(); iRow++)
            {

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(fromSpread.Rows[iRow + 2].DataItem, "SUBLOTID"))))
                    continue;
                sCellID += fromSpread.GetCell(iRow + 2, indexCellCol).Text.ToString() + ",";

                #region BizWF 
                int RetVal = BizWF_Check(fromSpread.GetCell(iRow + 2, indexCellCol).Text.ToString());

                if (RetVal != 0)
                {
                    //ShowLoadingIndicator();
                    //loadingIndicator.Visibility = Visibility.Hidden;
                    return;
                }
                #endregion
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("EQP_KIND", typeof(string));
            dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["SUBLOTID"] = sCellID;
            dr["EQP_KIND"] = gsEqpKind;
            dr["PROCESS_TYPE"] = gsProcessType;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_INSERT_LOSS", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
            Util.GridSetData(toSpread, dtRslt, FrameOperation, false);

            txtBadInsertRow.Text = dtRslt.Select("CHK = '0'").Length.ToString();

            for (int i = 0; i < toSpread.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(toSpread.Rows[i + 2].DataItem, "CHK")).Equals("True"))
                {
                    toSpread.Rows[i + 2].Presenter.Tag = "U";
                    //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("CHK")].Locked = false;

                    if (gsEqpKind.Equals("D") || gsEqpKind.Equals("5"))
                    {
                        //FarPoint.Win.Spread.CellType.ComboBoxCellType cellType = new FarPoint.Win.Spread.CellType.ComboBoxCellType();
                        //cellType.AutoCompleteSource = AutoCompleteSource.ListItems;
                        //cellType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        //cellType.Editable = true;

                        DataTable dtCombo = GetLossEqp(Util.NVC(DataTableConverter.GetValue(toSpread.Rows[i + 2].DataItem, "S71")), gsEqpKind);
                        DataView dvCombo = dtCombo.DefaultView;

                        string[] valueList = new string[dvCombo.Count];
                        string[] displayList = new string[dvCombo.Count];

                        for (int k = 0; k < dvCombo.Count; k++)
                        {
                            valueList[k] = dvCombo[k]["CBO_CODE"].ToString(); // ConvertUtil.ToString(dvCombo[k]["CBO_CODE"]);
                            displayList[k] = dvCombo[k]["CBO_NAME"].ToString(); // ConvertUtil.ToString(dvCombo[k]["CBO_NAME"]);
                        }

                        //cellType.Items = displayList;
                        //cellType.ItemData = valueList;
                        //cellType.EditorValue = FarPoint.Win.Spread.CellType.EditorValue.ItemData;

                        //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("EQP_NAME")].CellType = cellType;
                        //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("EQP_NAME")].Value = toSpread.GetValue(i, toSpread.GetColumnIndex("EQP_ID"));

                        DataTableConverter.SetValue(toSpread.Rows[i + 2].DataItem, "EQPTNAME", Util.NVC(DataTableConverter.GetValue(toSpread.Rows[i + 2].DataItem, "EQPTID")));

                        //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("EQP_NAME")].Locked = false;
                    }
                }
                else
                {
                    //toSpread.ActiveSheet.Rows[i].Tag = null;
                    //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("CHK")].Locked = true;
                }
            }
            txtInsertCellCnt.Text = dtRslt.Rows.Count.ToString();
            toSpread.Refresh();
        }

        public static DataTable GetLossEqp(string sLaneId, string sEqpType, bool bCodeDisplay = false)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sLaneId;
                dr["EQPTTYPE"] = sEqpType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_EQPT_IN_EQPTTYPE", "RQSTDT", "RSLTDT", RQSTDT);

                return SetCodeDisplay(dtResult, bCodeDisplay);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }
        private void btnBadCellReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgInputList.ItemsSource);
                if (dt.Columns.Count < 2)
                {
                    //조회된 값이 없습니다.
                    Util.MessageValidation("FM_ME_0232");
                    return;
                }
                if (!dgInputList.IsCheckedRow("CHK"))
                {
                    //선택된 Cell ID가 없습니다.
                    Util.MessageValidation("FM_ME_0161");
                    return;
                }

                DateTime dateReg = new DateTime(dtpFromDateT1.SelectedDateTime.Year,
                                                dtpFromDateT1.SelectedDateTime.Month,
                                                dtpFromDateT1.SelectedDateTime.Day,
                                                dtpFromTimeT1.DateTime.Value.Hour,
                                                dtpFromTimeT1.DateTime.Value.Minute,
                                                dtpFromTimeT1.DateTime.Value.Second);

                //ERP 마감여부 체크
                if (!ChkErpClosingFlag(dateReg))
                    return;

                if (DateTime.Now < dateReg)
                {
                    //등록일 기준 미래일자는 등록할 수 없습니다. 
                    Util.MessageValidation("FM_ME_0443");
                    return;
                }

                for (int i = 2; i < dgInputList.Rows.Count; i++)
                {
                    // Cell의 투입 시간 존재 시
                    if (!string.IsNullOrWhiteSpace(dgInputList.GetStringValue(i, "WORK_DTTM").ToString()))
                    {
                        DateTime WorkDate = Convert.ToDateTime(dgInputList.GetStringValue(i, "WORK_DTTM"));

                        // Cell의 투입 시간보다 선택한 일자가 과거 날짜라면
                        if (WorkDate > dateReg)
                        {
                            //마지막 투입 시간 이전 시간으로 등록 불가합니다.
                            Util.MessageValidation("SFU8512");
                            return;
                        }
                    }

                    //if (dateReg.AddDays(-1) > Convert.ToDateTime(dgInputList.GetStringValue(i, "WORK_DATE")) ||
                    //    dateReg.AddDays(1) < Convert.ToDateTime(dgInputList.GetStringValue(i, "WORK_DATE")))
                    // {
                    //    //등록하려는 작업일자가 %1의 작업일자와 하루 이상 차이나서 등록불가.
                    //    Util.MessageValidation("FM_ME_0438", dgInputList.GetStringValue(i, "SUBLOTID"));
                    //    return;
                    // }

                    if (dgInputList.GetValue(i, "FINL_JUDG_CODE") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "FINL_JUDG_CODE").ToString())
                        || dgInputList.GetValue(i, "EQPTID") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "EQPTID").ToString())
                        || dgInputList.GetValue(i, "PROCNAME") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "PROCNAME").ToString()))
                    {
                        //데이터를 조회하십시오.
                        Util.Alert("9059");
                        return;
                    }
                }

                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sLossCode = Util.GetCondition(cboInsertDefect, bAllNull: true);
                        if (string.IsNullOrEmpty(sLossCode) || sLossCode.Contains("SELECT"))
                        {
                            Util.Alert("FM_ME_0149"); //불량코드를 선택해주세요.
                            return;
                        }
                        string sDefectProcType = Util.GetCondition(cboInsertGroup);
                        if (string.IsNullOrEmpty(sLossCode)) return;

                        if (dgInputList.GetRowCount() <= 0)
                        {
                            //등록할 대상이 존재하지 않습니다.
                            Util.MessageValidation("FM_ME_0125");
                            return;
                        }

                        DataSet dsInDataSet = new DataSet();

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));
                        dtINDATA.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                        dtINDATA.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtINDATA.Columns.Add("CALDATE", typeof(DateTime));
                        dtINDATA.Columns.Add("MENUID", typeof(string));
                        dtINDATA.Columns.Add("USER_IP", typeof(string));
                        dtINDATA.Columns.Add("PC_NAME", typeof(string));
                        dsInDataSet.Tables.Add(dtINDATA);

                        DataRow drInData = dtINDATA.NewRow();
                        drInData["SRCTYPE"] = "UI";
                        drInData["IFMODE"] = "OFF";
                        drInData["USERID"] = txtUserID.Text;
                        drInData["LOT_DETL_TYPE_CODE"] = 'N';
                        drInData["REMARKS_CNTT"] = txtRemark.Text;
                        drInData["CALDATE"] = dateReg;
                        drInData["MENUID"] = LoginInfo.CFG_MENUID;
                        drInData["USER_IP"] = LoginInfo.USER_IP;
                        drInData["PC_NAME"] = LoginInfo.PC_NAME;
                        dtINDATA.Rows.Add(drInData);

                        DataTable dtIN_SUBLOT = new DataTable();
                        dtIN_SUBLOT.TableName = "IN_SUBLOT";
                        dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                        dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                        dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                        dsInDataSet.Tables.Add(dtIN_SUBLOT);

                        for (int i = 2; i < dgInputList.Rows.Count; i++)
                        {
                            if (dgInputList.IsCheckedRow("CHK", i - 2)
                                && string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "DENY_DESC"))))
                            {

                                dtIN_SUBLOT.Rows.Clear();

                                DataRow drInSublot = dtIN_SUBLOT.NewRow();
                                drInSublot["SUBLOTID"] = dgInputList.GetValue(i, "SUBLOTID");
                                drInSublot["DFCT_GR_TYPE_CODE"] = gsEqpKind;
                                drInSublot["DFCT_CODE"] = sLossCode;
                                dtIN_SUBLOT.Rows.Add(drInSublot);

                                try
                                {
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet, FrameOperation.MENUID);

                                    if (dsResult.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0"))
                                    {
                                        dgInputList.SetValue(i, "DENY_DESC", "SUCCESS");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    dgInputList.SetValue(i, "DENY_DESC", "FAIL");
                                }
                            }
                        }
                        // 비고란을 통해 성공여부를 확인할 수 있습니다.
                        Util.MessageInfo("FM_ME_0448");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();

                DataTable dtRslt = new DataTable();
                for (int iRow = 0; iRow < dgRecover.GetRowCount(); iRow++)
                {
                    int indexCellCol = dgRecover.Columns["SUBLOTID"].Index;
                    string sCellID = string.Empty;
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[iRow].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID = sTemp;

                    //스프레드에 있는지 확인
                    if (iRow != 0)
                    {
                        for (int i = 0; i <= iRow - 1; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID")).Equals(sCellID))
                            {
                                Util.MessageInfo("FM_ME_0287", new string[] { sCellID });  //[CELL ID : {0}]목록에 기존재하는 Cell 입니다.
                                continue;
                            }
                        }
                    }

                    #region BizWF 
                    int RetVal = BizWF_Check(sCellID);

                    if (RetVal != 0)
                    {
                        //ShowLoadingIndicator();
                        loadingIndicator.Visibility = Visibility.Hidden;
                        return;
                    }
                    #endregion

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                    dr["PROCESS_TYPE"] = gsProcessType;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);

                    //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_CELLID_FOR_RECOVER", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                    //Degas Cell 도 복구처리 할 수 있도록 수정, 2023.05.24
                    dtRqst.Clear();
                    dr.Delete();
                    dr["SUBLOTID"] = sCellID;
                    if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;
                    dr["PROCESS_TYPE"] = 'D';
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_CELLID_FOR_RECOVER", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                    if (dtRslt1.Rows.Count > 0)
                    {
                        dtRslt.Merge(dtRslt1);
                    }
                    else if (dtRslt2.Rows.Count > 0)
                    {
                        dtRslt.Merge(dtRslt2);
                    }
                    else
                    {
                        cellIDs.Append(sTemp + ", ");
                    }
                }

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dgRecover.ClearRows();
                }
                else
                {
                    Util.GridSetData(dgRecover, dtRslt, FrameOperation, true);
                }

                if (cellIDs.Length > 0)
                {
                    Util.MessageInfo(MessageDic.Instance.GetMessage("SFU1585") + "\r\n\r\n" + cellIDs.ToString(0, cellIDs.Length - 2));  //불량정보가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            dgRecover.ClearRows();
            DataGridRowAdd(dgRecover, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Math.Abs(iRowcount) > 0)
                {
                    if (iRowcount > 900)
                    {
                        //최대 ROW수는 900입니다.
                        Util.MessageValidation("SFU4648", "900");
                        return;
                    }
                }

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

        private void btnBadCellResotre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRecover.GetRowCount() == 0)
                {
                    Util.Alert("FM_ME_0139");  //복구 대상이 없습니다.
                    return;
                }

                for (int j = 0; j < dgRecover.GetRowCount(); j++)
                {
                    if (dgRecover.GetValue(j, "SUBLOTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "SUBLOTID").ToString()))
                    {
                        //복구 대상이 없습니다.
                        Util.Alert("FM_ME_0139");
                        return;
                    }

                    if (dgRecover.GetValue(j, "ROUTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "ROUTID").ToString())
                        || dgRecover.GetValue(j, "PROD_LOTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "PROD_LOTID").ToString()))
                    {
                        //데이터를 조회하십시오.
                        Util.Alert("9059");
                        return;
                    }
                }

                Util.MessageConfirm("FM_ME_0147", (result) => //불량 Cell 복구를 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("SUBLOTID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));

                            string errorString = string.Empty;
                            for (int i = 0; i < dgRecover.GetRowCount(); i++)
                            {
                                dtRqst.Rows.Clear();

                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["SUBLOTID"] = dgRecover.GetValue(i, "SUBLOTID");
                                dr["USERID"] = txtRestoreUserID.Text;
                                dtRqst.Rows.Add(dr);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SUBLOT_DFCT_RESTORE", "INDATA", "OUTDATA", dtRqst);
                                if (!string.IsNullOrEmpty(dtRslt.Rows[0]["NO_SUBLOTID"].ToString()))
                                {
                                    errorString += dtRslt.Rows[0]["NO_SUBLOTID"].ToString() + ",";
                                }
                            }

                            if (!string.IsNullOrEmpty(errorString))
                            {
                                //NO_SUBLOTID String 불량항목 없어서 복구되지 않은 SUBLOTID LIST, 리턴 값 예시) AK21E12955,AK21E12956,AK21E12957
                                Util.MessageInfo("FM_ME_0366", errorString);  //{0}은 Cell의 등급이력이 없어 복구할 수 없습니다.
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0140");  //저장하였습니다.
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        btnRefresh2_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2024.07.23 Ivan P add new event to directly search after paste in txtSearchCellId
        private void txtSearchCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            btnSearch3.PerformClick();
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSearchOp.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                //2024.07.23 Ivan P add max limit for cell input to be 900 cells
                if (txtSearchCellId.Text.Count(c => c == ',') + 1 > Convert.ToInt32(sRowCount))
                {
                    Util.Alert("FM_ME_0619");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                if (rdoReg.IsChecked == true)
                {
                    dtRqst.Columns.Add("FROM_REG_DATE", typeof(string)); //MES 리빌딩 PJT : datetime -> string 타입 변경
                    dtRqst.Columns.Add("TO_REG_DATE", typeof(string)); //MES 리빌딩 PJT : datetime -> string 타입 변경
                }
                else if (rdoWork.IsChecked == true)
                {
                    dtRqst.Columns.Add("FROM_WORK_DATE", typeof(string)); //MES 리빌딩 PJT : datetime -> string 타입 변경
                    dtRqst.Columns.Add("TO_WORK_DATE", typeof(string)); //MES 리빌딩 PJT : datetime -> string 타입 변경
                }

                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("EQPT_INPUT_AVAIL_FLAG", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (rdoReg.IsChecked == true)
                {
                    dr["FROM_REG_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                    dr["TO_REG_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:00");
                }
                else if (rdoWork.IsChecked == true)
                {
                    dr["FROM_WORK_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_WORK_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                }
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHFT_ID"] = cboSearchShift.GetBindValue();
                dr["DFCT_GR_TYPE_CODE"] = cboSearchOp.GetBindValue();
                dr["DFCT_CODE"] = cboSearchDefect.GetBindValue();
                dr["SUBLOTID"] = txtSearchCellId.GetBindValue();
                if (cboSearchEqp.Visibility == Visibility.Visible)
                {
                    dr["S71"] = cboSearchLane.GetBindValue();
                    dr["EQPTID"] = cboSearchEqp.GetBindValue();
                }
                dr["MDLLOT_ID"] = cboSearchModel.GetBindValue();
                dr["PROD_LOTID"] = txtSearchLotId.GetBindValue();
                dr["EQPT_INPUT_AVAIL_FLAG"] = cboEqpInputYN.GetBindValue();
                dr["REMARKS_CNTT"] = txtHoldDesc.GetBindValue();
                dr["INSUSER"] = txtSearchRegUser.GetBindValue();
                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_LOSS", "RQSTDT", "RSLTDT", dtRqst, gsEqpKind); //INSUSER 로 수정하기 비즈 수정 소스 수정!!!!!!!!

                // CHK 추가
                if (!dtRslt.Columns.Contains("CHK"))
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Select().ToList().ForEach(row => row["CHK"] = false);
                    dtRslt.AcceptChanges();
                }

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

                if (!gsEqpKind.Equals("6")) // Selector
                {
                    for (int i = 0; i < dgSearch.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG")).Equals("N"))
                        {
                            DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", true);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchRecover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                //불량 Cell 복구를 하시겠습니까?
                Util.MessageConfirm("FM_ME_0147", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        bool bNGRetval = false;

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("SUBLOTID", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgSearch.Rows.Count; i++)
                        {
                            if (dgSearch.IsCheckedRow("CHK", i))
                            {
                                dtINDATA.Rows.Clear();

                                DataRow drInSublot = dtINDATA.NewRow();
                                drInSublot["SRCTYPE"] = "UI";
                                drInSublot["IFMODE"] = "OFF";
                                drInSublot["SUBLOTID"] = dgSearch.GetValue(i, "SUBLOTID");
                                drInSublot["USERID"] = LoginInfo.USERID;
                                dtINDATA.Rows.Add(drInSublot);

                                try
                                {
                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SUBLOT_DFCT_RESTORE", "INDATA", "OUTDATA", dtINDATA);

                                    if (!dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                                    {
                                        bNGRetval = true;
                                        Util.MessageValidation("FM_ME_0311");  //저장 실패하였습니다.
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }
                        }
                        if (!bNGRetval)
                        {
                            Util.MessageValidation("FM_ME_0140");  //저장하였습니다.
                        }
                        btnSearch3_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboHistOp.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("PRE_DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRE_DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpHistFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["TO_DATE"] = dtpHistToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistToTime.DateTime.Value.ToString(" HH:mm:00");

                dr["SHFT_ID"] = cboHistShift.GetBindValue();
                dr["PRE_DFCT_GR_TYPE_CODE"] = cboHistOp.GetBindValue();
                dr["PRE_DFCT_CODE"] = cboHistDefect.GetBindValue();
                dr["SUBLOTID"] = txtHistCellId.GetBindValue();
                if (cboHistEqp.Visibility == Visibility.Visible)
                {
                    dr["S71"] = cboHistLane.GetBindValue();
                    dr["EQPTID"] = cboHistEqp.GetBindValue();
                }
                dr["MDLLOT_ID"] = cboHistModel.GetBindValue();
                dr["PROD_LOTID"] = txtHistLotId.GetBindValue();
                dr["REMARKS_CNTT"] = txtHistHoldDesc.GetBindValue();
                dr["INSUSER"] = txtHistUser.GetBindValue();

                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_LOSS_DEL", "RQSTDT", "RSLTDT", dtRqst, menuid: Tag.ToString());

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_LOSS_RSLT_HIST", "RQSTDT", "RSLTDT", dtRqst, gsEqpKind);

                Util.GridSetData(dgHistList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoReg_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoReg.IsChecked == true)
            {
                dtpFromTime.IsEnabled = true;
                dtpToTime.IsEnabled = true;
            }
            else
            {
                dtpFromTime.IsEnabled = false;
                dtpToTime.IsEnabled = false;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgInputList);
        }


        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgRecover);
        }


        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgInputList, true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgInputList, false);
        }

        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DENY_DESC"))
                {
                    if (dataGrid[e.Cell.Row.Index, 0].Presenter != null)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                        {
                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            if (cb != null) cb.Visibility = Visibility.Hidden;
                        }
                        else
                        {

                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            cb.Visibility = Visibility.Visible;
                        }
                    }
                }

                if (e.Cell.Row.Index == 0 && e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    C1.WPF.DataGrid.DataGridColumnHeaderPresenter dgcp =
                    (C1.WPF.DataGrid.DataGridColumnHeaderPresenter)e.Cell.Presenter.MergedRange.TopLeftCell.Presenter.Content;
                    dgcp.Foreground = Brushes.Red;
                }
            }));
        }

        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
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
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dataGrid, dataTable, FrameOperation);
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            dgRecover.RemoveRow(idx);
        }

        private void dgRecover_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private bool ChkErpClosingFlag(DateTime RegDttm)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("REG_DTTM", typeof(DateTime));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REG_DTTM"] = RegDttm;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_CHK_ERP_CLOSING_FLAG", "INDATA", null, dtRqst);

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetUserWindow();
        }

        private void txtRestoreUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetRestoreUserWindow();
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnRestoreSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetRestoreUserWindow();
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

        private void GetRestoreUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtRestoreUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndRestoreUser_Closed);
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

        private void wndRestoreUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRestoreUserName.Text = wndPerson.USERNAME;
                txtRestoreUserID.Text = wndPerson.USERID;
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = window.USERNAME;
                txtUserID.Text = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void wndRestoreShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtRestoreUserName.Text = window.USERNAME;
                txtRestoreUserID.Text = window.USERID;
            }
            grdMain.Children.Remove(window);
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

        // 2024.07.23 Ivan P add a method to retrieve common code value for max number of cell can be entered in Cell ID texbox
        private string GetCommonCode(string sComTypeCode, string sComCode)
        {
            string sValue = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sComTypeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ALL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (row["CBO_CODE"].ToString().Equals(sComCode))
                    {
                        sValue = row["ATTR1"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return sValue;
        }

        private void btnChgLoss_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgChgLossList.ItemsSource);
            if (dt.Columns.Count < 2)
            {
                //조회된 값이 없습니다.
                Util.MessageValidation("FM_ME_0232");
                return;
            }

            if (!dgChgLossList.IsCheckedRow("CHK"))
            {
                //선택된 Cell ID가 없습니다.
                Util.MessageValidation("FM_ME_0161");
                return;
            }

            string sLossCode = Util.GetCondition(cboInsertDefect1, bAllNull: true);
            if (string.IsNullOrEmpty(sLossCode) || sLossCode.Contains("SELECT"))
            {
                Util.Alert("FM_ME_0149"); //불량코드를 선택해주세요.
                return;
            }

            Util.MessageConfirm("SFU10023", (result) => //불량 Cell 복구를 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    int k = 0;

                    DataSet dsInDataSet = new DataSet();

                    // 복구 data set 구성
                    DataTable restoreInData = new DataTable();
                    restoreInData.TableName = "RES_INDATA";
                    restoreInData.Columns.Add("SRCTYPE", typeof(string));
                    restoreInData.Columns.Add("IFMODE", typeof(string));
                    restoreInData.Columns.Add("SUBLOTID", typeof(string));
                    restoreInData.Columns.Add("USERID", typeof(string));
                    dsInDataSet.Tables.Add(restoreInData);

                    // 등록 data set 구성
                    DataTable registerInData = new DataTable();
                    registerInData.TableName = "REG_INDATA";
                    registerInData.Columns.Add("SRCTYPE", typeof(string));
                    registerInData.Columns.Add("IFMODE", typeof(string));
                    registerInData.Columns.Add("USERID", typeof(string));
                    registerInData.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                    registerInData.Columns.Add("REMARKS_CNTT", typeof(string));
                    registerInData.Columns.Add("CALDATE", typeof(DateTime));
                    registerInData.Columns.Add("MENUID", typeof(string));
                    registerInData.Columns.Add("USER_IP", typeof(string));
                    registerInData.Columns.Add("PC_NAME", typeof(string));
                    dsInDataSet.Tables.Add(registerInData);

                    DataRow drInData = registerInData.NewRow();
                    drInData["SRCTYPE"] = "UI";
                    drInData["IFMODE"] = "OFF";
                    drInData["USERID"] = LoginInfo.USERID;
                    drInData["LOT_DETL_TYPE_CODE"] = 'N';
                    drInData["REMARKS_CNTT"] = txtRemark1.Text;
                    drInData["CALDATE"] = DateTime.Now;
                    drInData["MENUID"] = LoginInfo.CFG_MENUID;
                    drInData["USER_IP"] = LoginInfo.USER_IP;
                    drInData["PC_NAME"] = LoginInfo.PC_NAME;
                    registerInData.Rows.Add(drInData);


                    DataTable dtIN_SUBLOT = new DataTable();
                    dtIN_SUBLOT.TableName = "REG_IN_SUBLOT";
                    dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                    dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                    dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                    dsInDataSet.Tables.Add(dtIN_SUBLOT);

                    string errorString = string.Empty;
                    string[] resultString = new string[dgChgLossList.GetRowCount()];

                    for (int i = 2; i < dgChgLossList.Rows.Count; i++)
                    {
                        // check 한 대상만 포함
                        if(dgChgLossList.GetValue(i, "CHK").ToString().Equals("0"))
                        {
                            // check 하지 않은 데이터는 비고에 ""
                            resultString[i - 2] = "";
                            break;
                        }

                        try
                        {
                            restoreInData.Rows.Clear();
                            k = i;
                            DataRow dr1 = restoreInData.NewRow();
                            dr1["SRCTYPE"] = "UI";
                            dr1["IFMODE"] = "OFF";
                            dr1["SUBLOTID"] = dgChgLossList.GetValue(i, "SUBLOTID");
                            dr1["USERID"] = LoginInfo.USERID;
                            restoreInData.Rows.Add(dr1);

                            dtIN_SUBLOT.Rows.Clear();

                            DataRow drInSublot = dtIN_SUBLOT.NewRow();
                            drInSublot["SUBLOTID"] = dgChgLossList.GetValue(i, "SUBLOTID");
                            drInSublot["DFCT_GR_TYPE_CODE"] = "5";
                            drInSublot["DFCT_CODE"] = sLossCode;
                            dtIN_SUBLOT.Rows.Add(drInSublot);

                            DataSet dtRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CELL_CHG_LOSS_UI", "RES_INDATA,REG_INDATA,REG_IN_SUBLOT", "RES_OUTDATA,REG_OUTDATA", dsInDataSet, FrameOperation.MENUID);

                            // RES_OUTDATA : 복구 시 RETURN 값
                            if (!string.IsNullOrEmpty(dtRslt.Tables[0].Rows[0]["NO_SUBLOTID"].ToString()))
                            {
                                //NO_SUBLOTID String 불량항목 없어서 복구되지 않은 SUBLOTID
                                //Util.MessageInfo("FM_ME_0366", dtRslt.Tables[0].Rows[0]["NO_SUBLOTID"].ToString());
                                resultString[i - 2] = "FAIL";
                                continue;
                            }

                            // REG_OUTDATA : 등록 시 RETURN 값
                            if (dtRslt.Tables[1].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //dgChgLossList.SetValue(i, "DENY_DESC", "SUCCESS");
                                // 정상 변경
                                resultString[i - 2] = "SUCCESS";
                            }
                        }
                        catch (Exception ex)
                        {
                            //dgChgLossList.SetValue(k, "DENY_DESC", "FAIL");
                            resultString[i - 2] = "FAIL";
                            continue;
                        }
                        
                    }

                    // 비고란을 통해 성공여부를 확인할 수 있습니다.
                    Util.MessageInfo("FM_ME_0448");

                    //정상 종료시 데이터 변경을 위해서 재 조회
                    btnSearch5_Click(null, null);

                    // 조회 이후에 비고 입력함
                    for(int i = 0; i < resultString.Length; i++)
                    {
                        dgChgLossList.SetValue(i+2, "DENY_DESC", resultString[i]);
                    }

                    txtBadInsertRow1.Text = resultString.Where(s => s == "FAIL").Count().ToString();
                    txtInsertCellCnt1.Text = resultString.Where(s => s == "SUCCESS").Count().ToString();
                }
            });
        }

        private void btnRefresh3_Click(object sender, RoutedEventArgs e)
        {
            dgChgLossList.ClearRows();
            DataGridRowAdd(dgChgLossList, Convert.ToInt32(txtRowCntInsertCell1.Text));
        }

        private void btnExcel3_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgChgLossList);
        }

        private void btnSearch5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();
                DataTable dtRslt = new DataTable();
                string sCellID = string.Empty;

                for (int iRow = 0; iRow < dgChgLossList.GetRowCount(); iRow++)
                {
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dgChgLossList.Rows[iRow + 2].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID += sTemp + ",";

                    #region BizWF 
                    int RetVal = BizWF_Check(sTemp);

                    if (RetVal != 0)
                    {
                        return;
                    }
                    #endregion
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellID;
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_DFCT_INFO", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                Util.GridSetData(dgChgLossList, dtRslt1, FrameOperation, false);

                txtInsertCellCnt1.Text = dtRslt1.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgChgLossList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private void dgChgLossList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DENY_DESC"))
                {
                    if (dataGrid[e.Cell.Row.Index, 0].Presenter != null)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                        {
                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            if (cb != null)
                            {
                                cb.Visibility = Visibility.Hidden;
                                cb.IsChecked = false;
                            }
                        }
                        else
                        {

                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            cb.Visibility = Visibility.Visible;
                        }
                    }
                }

                if (e.Cell.Row.Index == 0 && e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    C1.WPF.DataGrid.DataGridColumnHeaderPresenter dgcp =
                    (C1.WPF.DataGrid.DataGridColumnHeaderPresenter)e.Cell.Presenter.MergedRange.TopLeftCell.Presenter.Content;
                    dgcp.Foreground = Brushes.Red;
                }
            }));
        }

        // [E20240422-000378] 건으로 폴란드 활성화만 반영하기 위해서 함수 생성
        private bool GetCellChgLossTab()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_CELL_CHG_LOSS_TAB";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
