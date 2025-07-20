/*******************************************************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : 활성화 Lot 관리
-------------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME   : Initial Created
  2021.03.31  KDH    : 변경사유 추가
  2021.04.01  KDH    : CommonCombo 참조에서 CommonCombo_Form 참조로 변경
  2021.04.06  KDH    : Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID)
  2021.04.18  KDH    : 검색조건 제거(작업일) 및 컬럼 추가(CST ID, 재공상태)
  2021.04.20  KDH    : 공통 Line에 대해 공정 List Setting되게 로직 추가
  2021.04.26  KDH    : 검색조건 추가 및 자동 체크 기능 추가 (Cell ID)
  2022.05.27  이정미 : 동 콤보박스 수정 
  2022.07.25  KDH    : C20220603-000198_버튼별 권한 체크 로직 추가
  2022.08.09  조영대 : 설비콤보 기본설정(Setting) 안되게 수정.
  2022.08.26  조영대 : Cell ID 입력 조회 시 선택된 Lot 을 최상위로 이동  
  2022.08.31  강동희 : Cell ID 입력/조회 후 Cell ID Block 설정
  2022.09.01  조영대 : 공정콤보박스 ALL 추가
  2022.09.02  김령호 : Aging 공정시에 Line 조건 무시하도록 변경
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2022.12.22  이정미 : Cell ID가 0개인 경우 발생하는 오류 수정
  2023.01.08  형준우 : 불량그룹유형에 따른 불량그룹이 나오도록 수정
  2023.01.31  조영대 : Empty Validation 추가
  2023.03.14  이정미 : 실물 폐기 처리  Validation 삭제  - 실물 폐기 처리 시 전체 Cell 폐기해야 하는 조건 삭제 
  2023.06.22  최도훈 : 화면 처음 열리는 속도 개선
  2023.06.23  최도훈 : Lot ID, Cell ID 조회 속도 개선
  2024.01.10  최도훈 : 1.불량코드 선택시 불량명도 함께 보여주도록 수정. 2.'순번' 컬럼 정렬시 오류가 있어 Row로 변경. 3.선택된 Cell 수량 표시
  2024.01.18  형준우 : 실물 폐기 이벤트 (SaveScrapLotProcess) AREAID 추가
  2024.03.19  남형희 : E20240304-000395 ESNA 요청 공정그룹 ALL 추가
  2024.06.11  남형희 : E20240514-000705 ESNA N Lot을 등록한 작업자가 조회되도록 컬럼 추가
********************************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_096 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private DataTable dtTemp = new DataTable();
        private DataTable dtTemp2 = new DataTable();
        Util _Util = new Util();
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private string LOTID = string.Empty;
        private string LOTTYPE = string.Empty;

        private bool IsLoading = true;
        private bool isChkAllHdr = false;
        #endregion

        #region [Initialize]
        public FCS001_096()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReCheckProc);
            listAuth.Add(btnScrapStandbyProc);
            listAuth.Add(btnScrapProc);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            SetWorkResetTime();
            //Combo Setting            
            InitCombo();
            //Control Setting
            InitControl();

            //다른 화면에서 넘어온 경우
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                txtGrpLotID.Text = Util.NVC(parameters[0]);
                GetLotInfo();
            }

            IsLoading = false;
            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "ALLAREA");

            // 라인
            SetEquipmentSegmentCombo(cboLine);

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.

            // 공정
            SetProcessCombo(cboProcess);

            // 설비 
            SetEquipmentCombo(cboEquipment);

            // Lot 유형
            string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
            ComCombo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilter1);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            //dtpFromDate.SelectedDateTime = GetJobDateFrom(); //2021.04.18 검색조건 제거(작업일)
            //dtpToDate.SelectedDateTime = GetJobDateTo(); //2021.04.18 검색조건 제거(작업일)
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgReCheckNGLotList);
                Util.gridClear(dgCellIDDetail);
                btnReCheckProc.IsEnabled = false;
                btnScrapStandbyProc.IsEnabled = false;
                btnScrapProc.IsEnabled = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCGRID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FROM_WIPDTTM_ST", typeof(string));
                inTable.Columns.Add("TO_WIPDTTM_ST", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.GetBindValue();
                //상온, 고온, 출하 Aging 선택인 경우 무시.
                if (cboProcGrpCode.GetStringValue() == "3" || cboProcGrpCode.GetStringValue() == "4" || cboProcGrpCode.GetStringValue() == "7" || cboProcGrpCode.GetStringValue() == "9")
                {
                    newRow["EQSGID"] = null;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboLine);
                }
                //newRow["PROCGRID"] = cboProcGrpCode.GetBindValue(); //2024.03.19 E20240304-000395 ESNA 요청 공정그룹 ALL 조건 추가
                newRow["PROCGRID"] = Util.NVC(cboProcGrpCode.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboProcGrpCode);
                newRow["PROCID"] = cboProcess.GetBindValue();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboEquipment);
                newRow["PROD_LOTID"] = txtPkgLotID.Text == string.Empty ? null : txtPkgLotID.Text;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(cboLotType.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboLotType);
                newRow["PRODID"] = txtProd.Text == string.Empty ? null : txtProd.Text;
                //newRow["FROM_WIPDTTM_ST"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd"); //2021.04.18 검색조건 제거(작업일)
                //newRow["TO_WIPDTTM_ST"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd"); //2021.04.18 검색조건 제거(작업일)

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RECHECK_NG_DRB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            #region 선택된 로우 최상단으로 이동
                            if (!string.IsNullOrEmpty(txtGrpLotID.Text.Trim()))
                            {
                                List<DataRow> rows = result.AsEnumerable().Where(r => r["LOTID"].Equals(Util.NVC(txtGrpLotID.Text))).ToList();
                                if (rows.Count > 0)
                                {
                                    //rows[0]["CHK"] = true;
                                    rows[0]["CHK"] = 1;
                                    result.DefaultView.Sort = "CHK DESC";
                                    result = result.DefaultView.ToTable();
                                }
                            }
                            #endregion

                            Util.GridSetData(dgReCheckNGLotList, result, FrameOperation, true);
                            if (result.Rows.Count == 1)
                            {
                                string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[0].DataItem, "LOTID"));
                                dgReCheckNGLotList.SelectedIndex = 0;
                            }

                            if (!string.IsNullOrEmpty(txtSubLotID.Text))
                            {
                                txtSubLotID.SelectAll();
                                txtSubLotID.Focus();
                            }                                                        
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
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

        private void GetDetailInfo(string LOTID)
        {
            try
            {
                Util.gridClear(dgCellIDDetail);
                txtResnNoteSubLot.Text = string.Empty;

                SetGridCboItem_CommonCode(dgCellIDDetail.Columns["DFCT_GR_TYPE_CODE"], "FORM_DFCT_GR_TYPE_CODE");
                //SetGridCboItem_CommonCode(dgCellIDDetail.Columns["DEFECT_KIND"], "FORM_DFCT_TYPE_CODE");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = Util.NVC(LOTID);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RN_CELL_DETAIL_DRB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgCellIDDetail, result, FrameOperation, true);                            
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        chkCellQty.Text = GetCheckedCellQty();
                        isChkAllHdr = false;

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

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        /// <summary>
        /// 공정그룹
        /// </summary>
        private void SetProcessGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(), cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(), "PROC_GR_CODE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            //2024.03.19  E20240304-000395 남형희: ESNA 요청 공정그룹 ALL 추가
            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.ALL, selectedValueText, displayMemberText, null);
            
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            bool bAging = false;
            //상온, 고온, 출하 Aging 선택인 경우 무시.
            if (cboProcGrpCode.GetStringValue() == "3" || cboProcGrpCode.GetStringValue() == "4" || cboProcGrpCode.GetStringValue() == "7" || cboProcGrpCode.GetStringValue() == "9")
            {
                bAging = true;
            }
            else
            {
                bAging = false;
            }
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(),
                                      cboLine.SelectedValue == null || bAging == true ? null : cboLine.SelectedValue.ToString(),
                                      cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };

            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.ALL, selectedValueText, displayMemberText, null);

            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 START
            DataTable dtcbo = DataTableConverter.Convert(cbo.ItemsSource);
            if (dtcbo == null || dtcbo.Rows.Count == 0)
            {
                const string bizRuleName1 = "DA_BAS_SEL_ALL_OP_CBO";
                string[] arrColumn1 = { "LANGID", "PROC_GR_CODE" };
                string[] arrCondition1 = { LoginInfo.LANGID, cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
                string selectedValueText1 = "CBO_CODE";
                string displayMemberText1 = "CBO_NAME";

                CommonCombo_Form.CommonBaseCombo(bizRuleName1, cbo, arrColumn1, arrCondition1, CommonCombo_Form.ComboStatus.NONE, selectedValueText1, displayMemberText1, null);
            }
            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 END
        }

        /// <summary>
        /// 설비
        /// </summary>
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            string saveEqptId = cboEquipment.GetStringValue();

            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_EQP_BY_PROC";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCGRID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,
                cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(),
                cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode),
                cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString() };

            cboEquipment.SetDataComboItem(bizRuleName, arrColumn, arrCondition, string.Empty, CommonCombo.ComboStatus.ALL, true, saveEqptId);
        }

        // 공통함수로 뺄지 확인 필요 START
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        // 공통함수로 뺄지 확인 필요 END

        private void SaveScrapSubLotProcess(string sLotDetlTypeCode)
        {
            try
            {
                if (dgCellIDDetail.ItemsSource == null || dgCellIDDetail.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // DATA SET                                
                DataTable dtScrap = DataTableConverter.Convert(dgCellIDDetail.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtScrap == null || dtScrap.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string)); //20210331 변경사유 추가
                inTable.Columns.Add("REMARKS_CNTT", typeof(string));
                inTable.Columns.Add("MENUID", typeof(string));
                inTable.Columns.Add("USER_IP", typeof(string));
                inTable.Columns.Add("PC_NAME", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(sLotDetlTypeCode);
                newRow["REMARKS_CNTT"] = Util.NVC(txtResnNoteSubLot.Text); //20210331 변경사유 추가
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                newRow["USER_IP"] = LoginInfo.USER_IP;
                newRow["PC_NAME"] = LoginInfo.PC_NAME; 

                inTable.Rows.Add(newRow);

                DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_CODE", typeof(string));

                for (int i = 0; i < dgCellIDDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow newRowSubLot = inSubLot.NewRow();

                        newRowSubLot["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "SUBLOTID"));
                        newRowSubLot["DFCT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DFCT_GR_TYPE_CODE"));
                        newRowSubLot["DFCT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DEFECT_ID"));
                        inSubLot.Rows.Add(newRowSubLot);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        GetDetailInfo(LOTID);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet, menuid: FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveScrapLotProcess()
        {
            try
            {
                if (dgReCheckNGLotList.ItemsSource == null || dgReCheckNGLotList.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // DATA SET 
                DataTable dtScrap = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtScrap == null || dtScrap.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataTable dtScrapLot = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                DataTable dtDetalALLCell = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);

                DataTable dtDetalChkCell = DataTableConverter.Convert(dgCellIDDetail.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtDetalChkCell == null || dtDetalChkCell.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MENUID", typeof(string));
                inTable.Columns.Add("USER_IP", typeof(string));
                inTable.Columns.Add("PC_NAME", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID"));
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                newRow["USER_IP"] = LoginInfo.USER_IP;
                newRow["PC_NAME"] = LoginInfo.PC_NAME;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgCellIDDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inSubLot.NewRow();
                        dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "SUBLOTID"));
                        inSubLot.Rows.Add(dr);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_SET_SCRAP_SUBLOT", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        GetList();

                        DataTable dt = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                        dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
                        dt.AcceptChanges();
                        Util.GridSetData(dgReCheckNGLotList, dt, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem_CommonCode(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetDfctCode(string sDfctGrpTypeCode, string sDfctTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                dr["DFCT_TYPE_CODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_FORM_DFCT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetDfctKind(string sDfctGrpTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = "Y";
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEFECT_KIND", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetLotInfo()
        {
            try
            {
                Util.gridClear(dgReCheckNGLotList);
                Util.gridClear(dgCellIDDetail);
                btnReCheckProc.IsEnabled = false;
                btnScrapStandbyProc.IsEnabled = false;
                btnScrapProc.IsEnabled = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtGrpLotID.Text == string.Empty ? null : Util.NVC(txtGrpLotID.Text);

                string sGrpLotID = txtGrpLotID.Text; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                inTable.Rows.Add(newRow);

                //ShowLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                new ClientProxy().ExecuteService("DA_SEL_GROUP_LOT_INFO", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            // 2023.06.23 동 코드가 현재와 다른경우만 변경
                            string sArea = Util.NVC(result.Rows[0]["AREAID"]);

                            if (!string.IsNullOrWhiteSpace(sArea) && !sArea.Equals(cboArea.SelectedValue))
                            {
                                cboArea.SelectedValue = sArea;
                            }
                            cboLine.SelectedValue = Util.NVC(result.Rows[0]["EQSGID"]);
                            cboProcGrpCode.SelectedValue = Util.NVC(result.Rows[0]["PROC_GRP"]);
                            cboProcess.SelectedValue = Util.NVC(result.Rows[0]["PROCID"]);
                            //dtpFromDate.SelectedDateTime = DateTime.Parse(Util.NVC(result.Rows[0]["CALDATE"])); //2021.04.18 검색조건 제거(작업일)
                            txtGrpLotID.Text = sGrpLotID; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                            GetList();
                        }
                    }
                    catch (Exception ex)
                    {
                        //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                    }
                });

            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                Util.MessageException(ex);
            }
        }

        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
        private void GetSubLotInfo()
        {
            try
            {
                Util.gridClear(dgReCheckNGLotList);
                Util.gridClear(dgCellIDDetail);
                btnReCheckProc.IsEnabled = false;
                btnScrapStandbyProc.IsEnabled = false;
                btnScrapProc.IsEnabled = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SUBLOTID"] = txtSubLotID.Text == string.Empty ? null : Util.NVC(txtSubLotID.Text);

                string sSubLotID = txtSubLotID.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_SEL_SUBLOT_INFO", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            // 2023.06.23 동 코드가 현재와 다른경우만 변경
                            string sArea = Util.NVC(result.Rows[0]["AREAID"]);

                            if (!string.IsNullOrWhiteSpace(sArea) && !sArea.Equals(cboArea.SelectedValue))
                            {
                                cboArea.SelectedValue = sArea;
                            }
                            cboLine.SelectedValue = Util.NVC(result.Rows[0]["EQSGID"]);
                            cboProcGrpCode.SelectedValue = Util.NVC(result.Rows[0]["PROC_GRP"]);
                            cboProcess.SelectedValue = Util.NVC(result.Rows[0]["PROCID"]);
                            txtGrpLotID.Text = Util.NVC(result.Rows[0]["LOTID"]);
                            txtSubLotID.Text = Util.NVC(sSubLotID);

                            GetList();
                        }
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
        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END

        #endregion

        #region [Event]
        // 체크박스 관련
        #region
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
        private void dgReCheckNGLotList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgReCheckNGLotList.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgReCheckNGLotList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgReCheckNGLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        #endregion


        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (IsLoading) return;

            //cboLine.SelectedValueChanged -= cboLine_SelectedValueChanged;

            SetEquipmentSegmentCombo(cboLine);
            //cboLine.SelectedValueChanged += cboLine_SelectedValueChanged;
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (IsLoading) return;
            
            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.
            SetProcessCombo(cboProcess);
            SetEquipmentCombo(cboEquipment);

            // Clear
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void cboProcGrpCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (IsLoading) return;

            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;
            // 공정 
            SetProcessCombo(cboProcess);
            SetEquipmentCombo(cboEquipment);

            // Clear
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (IsLoading) return;

            // 설비 
            SetEquipmentCombo(cboEquipment);
            // Clear
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
        }

        private void txtPkgLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtPkgLotID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!IsLoading)
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtProd_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtProd.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        //2021.04.06 Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID) START
        private void txtGrpLotID_KeyDown(object sender, KeyEventArgs e)
        {
            //if ((!string.IsNullOrEmpty(txtGrpLotID.Text)) && (e.Key == Key.Enter))
            //{
            //    GetLotInfo();
            //}
        }
        //2021.04.06 Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID) END

        private void txtGrpLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtGrpLotID.Text)) && (e.Key == Key.Enter))
            {
                GetLotInfo();
            }
        }

        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
        private void txtSubLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtSubLotID.Text)) && (e.Key == Key.Enter))
            {
                GetSubLotInfo();
            }
        }
        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            if (!string.IsNullOrEmpty(txtSubLotID.Text))
            {
                GetSubLotInfo();
            }
            else if (!string.IsNullOrEmpty(txtGrpLotID.Text))
            {
                GetLotInfo();
            }
            else
            {
                GetList();
            }
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util _Util = new Util();

                if (dgReCheckNGLotList.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }

                int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                if (iRow < 0)
                {
                    Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOT_DETL_TYPE_CODE")).Equals("G"))
                {
                    string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID")).ToString();

                    // 프로그램 ID 확인 후 수정
                    object[] parameters = new object[6];
                    parameters[0] = string.Empty;
                    parameters[1] = sTrayNo;
                    parameters[2] = string.Empty;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    parameters[5] = "Y";
                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회 연계
                }
                else
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID"));   //TRAYNO
                    this.FrameOperation.OpenMenu("SFU010710021", true, parameters);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgReCheckNGLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReCheckNGLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("LOTID"))
                    {
                        string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.CurrentRow.DataItem, "LOTID"));

                        DataTable dt = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr["LOTID"].Equals(sLotID))
                            {
                                dr["CHK"] = true;
                            }
                            else
                            {
                                dr["CHK"] = false;
                            }
                        }
                        dgReCheckNGLotList.ItemsSource = DataTableConverter.Convert(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void dgReCheckNGLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;

                        int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                        if (iRow < 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[e.Cell.Row.Index].DataItem, "LOTID")).Equals(Util.NVC(txtGrpLotID.Text)))
                            {
                                DataTableConverter.SetValue(dgReCheckNGLotList.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }
                }
            }));
        }

        private void dgReCheckNGLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            LOTID = string.Empty;
            LOTTYPE = string.Empty;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                LOTID = Util.NVC((rb.DataContext as DataRowView).Row["LOTID"]);
                LOTTYPE = Util.NVC((rb.DataContext as DataRowView).Row["LOT_DETL_TYPE_CODE"]);
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                if (dg != null)
                {

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }

                dgReCheckNGLotList.SelectedIndex = idx;
                GetDetailInfo(LOTID);

                if (LOTTYPE.Equals("G"))
                {
                    btnReCheckProc.IsEnabled = true;
                    btnScrapStandbyProc.IsEnabled = true;
                    btnScrapProc.IsEnabled = false;
                }
                else if (LOTTYPE.Equals("R"))
                {
                    btnReCheckProc.IsEnabled = false;
                    btnScrapStandbyProc.IsEnabled = true;
                    btnScrapProc.IsEnabled = false;
                }
                else if (LOTTYPE.Equals("N"))
                {
                    btnReCheckProc.IsEnabled = true;
                    btnScrapStandbyProc.IsEnabled = false;
                    btnScrapProc.IsEnabled = true;
                }
                else if (LOTTYPE.Equals("H"))
                {
                    btnReCheckProc.IsEnabled = false;
                    btnScrapStandbyProc.IsEnabled = true;
                    btnScrapProc.IsEnabled = false;
                }
                else
                {
                    btnReCheckProc.IsEnabled = false;
                    btnScrapStandbyProc.IsEnabled = false;
                    btnScrapProc.IsEnabled = false;
                }

            }
        }

        private void dgCellIDDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;

                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        private void dgCellIDDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "SUBLOTID")
                    {
                        int iRow = _Util.GetDataGridRowIndex(dgCellIDDetail, "CHK", "True");

                        if (iRow < 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[e.Cell.Row.Index].DataItem, "SUBLOTID")).Equals(Util.NVC(txtSubLotID.Text)))
                            {
                                DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }
                    //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END

                    if (e.Cell.Column.Name.Equals("DFCT_GR_TYPE_CODE") || e.Cell.Column.Name.Equals("DEFECT_KIND") || e.Cell.Column.Name.Equals("DEFECT_ID"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                }
            }));
        }

        private void dgCellIDDetail_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;

            if (cbo != null)
            {
                //2023.01.03 추가
                //if (e.Column.Name == "DFCT_GR_TYPE_CODE")
                //{
                //    string sDfctGrpTypeCode = Util.NVC(cbo.SelectedValue, "DFCT_GR_TYPE_CODE");

                //    if (string.IsNullOrEmpty(sDfctGrpTypeCode))
                //    {
                //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_GR_TYPE_CODE"));
                //        return;
                //    }

                //    dtTemp2 = null;
                //    DataTable dt = SetDfctKind(sDfctGrpTypeCode);
                //    dtTemp2 = dt.Copy();

                //    dt.AcceptChanges();

                //    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                //    cbo.SelectedIndex = -1;

                //    DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Row.Index].DataItem, "DEFECT_KIND", string.Empty);
                //}

                if (e.Column.Name == "DEFECT_ID")
                {
                    string sDfctGrpTypeCode = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_GR_TYPE_CODE"));
                    string sDfctTypeCode = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DEFECT_KIND"));

                    if (string.IsNullOrEmpty(sDfctGrpTypeCode))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_GR_TYPE_CODE"));
                        return;
                    }

                    if (string.IsNullOrEmpty(sDfctTypeCode))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DEFECT_KIND"));
                        return;
                    }
                    dtTemp = null;
                    DataTable dt = SetDfctCode(sDfctGrpTypeCode, sDfctTypeCode);
                    dtTemp = dt.Copy();

                    dt.AcceptChanges();

                    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                    cbo.SelectedIndex = -1;

                    DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Row.Index].DataItem, "SUBLOT_GRD_CODE", string.Empty);
                }
            }
        }

        private void dgCellIDDetail_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                C1ComboBox cbo = e.EditingElement as C1ComboBox;

                //20230106 추가
                if (e.Column.Name == "DFCT_GR_TYPE_CODE")
                {
                    string sDfctGrpTypeCode = Util.NVC(cbo.SelectedValue, "DFCT_GR_TYPE_CODE");

                    if (string.IsNullOrEmpty(sDfctGrpTypeCode))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_GR_TYPE_CODE"));
                        return;
                    }

                    DataTable dt = SetDfctKind(sDfctGrpTypeCode);
                    dtTemp2 = dt.Copy();

                    dt.AcceptChanges();

                    DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Row.Index].DataItem, "DEFECT_KIND", string.Empty);
                }

                if (dg.CurrentColumn.Name == "DEFECT_ID")
                {
                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        string sDfctID = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "DEFECT_ID"));
                        DataRow[] dr = dtTemp.Select("CBO_CODE = '" + sDfctID + "'");

                        if (dr.Length > 0)
                        {
                            string sDfctName = Util.NVC(dr[0]["DFCT_NAME"]);
                            DataTableConverter.SetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "DFCT_NAME", sDfctName);

                            string sValue = Util.NVC(dr[0]["SUBLOT_GRD_CODE"]);
                            DataTableConverter.SetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "SUBLOT_GRD_CODE", sValue);

                            //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 START
                            //if (dg.CurrentRow.Index == (dg.Rows.Count - 1))
                            //{
                            //    dg.SelectRow(dg.CurrentRow.Index - 1);
                            //}
                            //else
                            //{
                            //    dg.SelectRow(dg.CurrentRow.Index + 1);
                            //}
                            //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 END
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgCellIDDetail.ItemsSource = DataTableConverter.Convert(dt);

            chkCellQty.Text = GetCheckedCellQty();
            isChkAllHdr = true;
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgCellIDDetail.ItemsSource = DataTableConverter.Convert(dt);

            chkCellQty.Text = GetCheckedCellQty();
            isChkAllHdr = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            chkCellQty.Text = GetCheckedCellQty();
        }

        private void btnReCheckProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapSubLotProcess("R");
                    }
                }, ObjectDic.Instance.GetObjectName("RECHECK_PROC"));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnScrapStandbyProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapSubLotProcess("N");
                    }
                }, ObjectDic.Instance.GetObjectName("SCRAP_STANDBY_PROC"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnScrapProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
                // 1. 해당 동 적용 여부 체크
                // 2. 적용 동 버튼 적용 여부 체크
                string[] sAttrbute = { null, "FCS001_096" };

                if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_FORM", "", sAttrbute))
                    if (!CheckButtonPermissionGroupByBtnGroupID("SCRAP_PROC_W", "FCS001_096")) return;
                // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapLotProcess(); //임시로 막아둠 2021.04.16
                    }
                }, ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL"));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnAllCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sCheck = string.Empty;
                string sDfctGrpTypeCode = string.Empty;
                string sDfctTypeCode = string.Empty;
                string sDfctCode = string.Empty;
                string sDfctName = string.Empty;
                string sSubLotGrdCode = string.Empty;

                for (int idx = 0; idx < dgCellIDDetail.Rows.Count; idx++)
                {
                    sCheck = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "CHK"));
                    sDfctGrpTypeCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DFCT_GR_TYPE_CODE"));
                    sDfctTypeCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DEFECT_KIND"));
                    sDfctCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DEFECT_ID"));
                    sDfctName = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DFCT_NAME"));
                    sSubLotGrdCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "SUBLOT_GRD_CODE"));

                    if (sCheck.Equals("True") || sCheck.Equals("1"))
                    {
                        break;
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
                foreach (DataRow dr in dt.Rows)
                {
                    if (Util.NVC(dr["CHK"]).Equals("True") || Util.NVC(dr["CHK"]).Equals("1"))
                    {
                        dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                        dr["DEFECT_KIND"] = sDfctTypeCode;
                        dr["DEFECT_ID"] = sDfctCode;
                        dr["DFCT_NAME"] = sDfctName;
                        dr["SUBLOT_GRD_CODE"] = sSubLotGrdCode;
                    }
                }
                Util.GridSetData(dgCellIDDetail, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                throw ex;
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


        #endregion

        private void btnScrapByCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                FCS001_113 fcs001_113 = new FCS001_113();
                fcs001_113.FrameOperation = FrameOperation;
                this.FrameOperation.OpenMenuFORM("SFU010705225", "FCS001_113", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL_SUBLOT"), true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void dgReCheckNGLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 START
        private void dgCellIDDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                //20230106 추가
                if (dg.CurrentColumn.Name == "DFCT_GR_TYPE_CODE")
                {
                    if (dtTemp2 != null && dtTemp2.Rows.Count > 0)
                    {
                        (dg.Columns["DEFECT_KIND"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtTemp2);
                    }
                }

                if (dg.CurrentColumn.Name == "DEFECT_ID")
                {
                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        string sDfctID = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "DEFECT_ID"));
                        DataRow[] dr = dtTemp.Select("CBO_CODE = '" + sDfctID + "'");

                        if (dr.Length > 0)
                        {
                            string sValue = Util.NVC(dr[0]["SUBLOT_GRD_CODE"]);
                            DataTableConverter.SetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "SUBLOT_GRD_CODE", sValue);
                            if (dg.CurrentRow.Index == (dg.Rows.Count - 1))
                            {
                                if (dg.CurrentRow.Index == 0)
                                {
                                   dg.CurrentRow.Refresh();
                                }

                                else
                                {
                                    dg.SelectRow(dg.CurrentRow.Index - 1);
                                }  
                            }

                            else
                            {
                                dg.SelectRow(dg.CurrentRow.Index + 1);
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
        //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 END

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
        private bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
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
        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID, string sFormID)
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
                dtRow["FORMID"] = sFormID;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "SCRAP_PROC_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL");

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private string GetCheckedCellQty(bool isChkAll = false, bool isUnChkAll = false)
        {
            int checkedQty = 0;
            int totalQty = dgCellIDDetail.GetRowCount();
            string tagName = chkCellQty.Text.Split(':')[0].Trim();

            if (isChkAll)
            {
                checkedQty = totalQty;
            }
            else if (!isUnChkAll)
            {
                for (int i = 0; i < totalQty; i++)
                {
                    if (DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        checkedQty++;
                    }
                }
            }            

            return $"{tagName} : {checkedQty} / {totalQty}";
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (isChkAllHdr) return;

            if (!string.IsNullOrWhiteSpace(txtSubLotID.Text))
            {
                chkCellQty.Text = GetCheckedCellQty();
            }
        }

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END

    }
}
