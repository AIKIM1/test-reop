/*************************************************************************************
 Created Date :
      Creator : 주건태
   Decription : 설비 Loss 등록 팝업 
   원래 설비 Loss 화면은 PACK 의 경우 별로 로직이 존재함. 이 팝업 화면은 PACK 별도로직 제외함. PACK 사용 필요시 화면 수정 필요.
   소형 원각의 요청으로 개발됨. 소형 원각 기준으로 개발되었으며 향후 타 모듈 확장시 소스 확인 필요함.
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.04  이병윤 : E20240215-001102 부동내역에 돋보기 추가 FCR팝업호출 및 리턴값처리  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;


namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_EQPT_LOSS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_EQPT_LOSS : C1Window
    {
        #region Declaration

        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqsgID = string.Empty;

        //Machine 설비 Loss 수정 가능여부 Flag :    2023.03.16 오화백
        string MachineEqptChk = string.Empty;
        string occurEqptFlag = string.Empty;

        bool bPack;
        bool isCauseEquipment = false;


        DataTable dtRemarkMandatory;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        public CMM_COM_EQPT_LOSS()
        {
            InitializeComponent();
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            try
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _EqsgID;
                dr["PROCID"] = _ProcID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(_EqptID))
                {
                    cbo.SelectedValue = _EqptID;

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length == 3)
                {
                    _EqptID = Util.NVC(tmps[0]);
                    _ProcID = Util.NVC(tmps[1]);
                    _EqsgID = Util.NVC(tmps[2]);
                }

                //C20210723 - 000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                if (string.IsNullOrWhiteSpace(_EqsgID) || string.IsNullOrEmpty(_EqsgID))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                if (string.IsNullOrWhiteSpace(_ProcID) || string.IsNullOrEmpty(_ProcID))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }
                if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
                {
                    bPack = false;
                }
                else
                {
                    bPack = true;
                }

                

                InitInsertCombo();

                SelectRemarkMandatory();    //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화

                GetEqptLossDetailList();

                SetTroubleUnitColumnDisplay();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectRemarkMandatory()
        {
            try
            {

                dtRemarkMandatory = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "EQPT_LOSS_REMARK_MANDATORY";
                row["EQSGID"] = _EqsgID;
                row["PROCID"] = _ProcID;

                dt.Rows.Add(row);

                dtRemarkMandatory = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_REMARK_MANDATORY", "INDATA", "RSLT", dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void InitInsertCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                SetEquipmentCombo(cboEquipment);

                //원인설비
                C1ComboBox[] cboOccurEqptParent = { cboEquipment };
                if (string.Equals(GetAreaType(), "P"))
                {
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cboOccurEqptParent);
                }
                else
                {
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                }

                //현상코드
                String[] sFilterFailure = { "F" };
                _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

                //원인코드
                String[] sFilterCause = { "C" };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

                //조치코드
                String[] sFilterResolution = { "R" };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");

                //LOSS분류
                if (LoginInfo.CFG_AREA_ID == "P1" || LoginInfo.CFG_AREA_ID == "P2" || LoginInfo.CFG_AREA_ID == "P6")
                {
                    C1ComboBox[] cboLossChild = { cboLossDetl };
                    string[] sFilterLoss = { LoginInfo.CFG_AREA_ID, _ProcID, Convert.ToString(cboEquipment.SelectedValue), _EqsgID };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProcPack");
                }
                else
                {
                    C1ComboBox[] cboLossChild = { cboLossDetl };
                    string[] sFilterLoss = { LoginInfo.CFG_AREA_ID, _ProcID, Convert.ToString(cboEquipment.SelectedValue) };
                    _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
                }

                //부동내용
                C1ComboBox[] cboLossDetlParent = { cboLoss, cboEquipment };
                string[] sFilter = { LoginInfo.CFG_AREA_ID, _ProcID };
                _combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);

                //최근등록
                C1ComboBox[] cboLastLossParent = { cboEquipment };
                _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);

                //동-라인-공정별 로스 맵핑
                string[] sFilter1 = { _EqsgID, _ProcID };
                _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                {
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
                }
                    
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return "";
        }

        private void cboLossEqsgProc_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboLastLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetComponent();
        }

        private void ResetComponent()
        {
            txtStart.Text = "";
            txtEnd.Text = "";
            //txtStartHidn.Text = "";
            //txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            cboLossDetl.SelectedIndex = 0;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;
            cboLossEqsgProc.SelectedIndex = 0;

            chkT.IsChecked = false;
            chkW.IsChecked = false;
            chkU.IsChecked = false;
        }

        private void SetComponent(C1.WPF.DataGrid.DataGridRow row = null)
        {
            try
            {
                if (row != null)
                {
                    txtStart.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "START_TIME"));
                    txtEnd.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "END_TIME"));
                    //txtStartHidn.Text = "";
                    //txtEndHidn.Text = "";
                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                    {
                        cboOccurEqpt.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                    {
                        cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                    {
                        cboLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                    {
                        cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                    {
                        cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                    {
                        cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                    {
                        new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                    }

                    cboLastLoss.SelectedIndex = 0;
                    cboLossEqsgProc.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveEqptLoss();
        }

        private bool ValidationCommon()
        {
            try
            {
                string sEqptId = Util.GetCondition(cboEquipment);
                if (sEqptId.Equals(""))
                {
                    Util.MessageValidation("SFU1153"); //설비를 선택하세요
                    return false;
                }

                if (cboOccurEqpt.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageValidation("9041"); //원인설비을 선택하여주십시오
                    return false;
                }

                string sLosscode = Util.GetCondition(cboLoss, "SFU3513");
                if (sLosscode.Equals(""))
                {
                    Util.MessageValidation("SFU3513"); //LOSS는필수항목입니다
                    return false;
                }

                string sLossDetlCode = Util.GetCondition(cboLossDetl);
                if (sLossDetlCode.Equals(""))
                {
                    if (cboLossDetl.Items.Count > 1)
                    {
                        Util.MessageValidation("SFU3631");  //부동내용을 입력하세요.
                        return false;
                    }
                }

                //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
                {

                    DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLosscode + "' AND ATTRIBUTE4 = '" + sLossDetlCode + "'");

                    if (rows.Length > 0)
                    {
                        int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
                        string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

                        //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
                        if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
                        {
                            Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
                            rtbLossNote.Focus();
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
            }
        }

        private void SaveEqptLoss()
        {
            try
            {
                Util util = new Util();

                if (util.GetDataGridCheckCnt(dgDetail, "CHK") > 0)
                {
                    Util.MessageValidation("SFU3490");  //하나의 부동내역을 저장 할 경우 check box선택을 모두 해제 후 \r\n 한개의 행만 더블클릭  해주세요
                    return;
                }

                if(ValidationCommon() == false)
                {
                    return;
                }

                int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
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
                //RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
                //RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
                //RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
                RQSTDT.Columns.Add("CHKW", typeof(string));
                RQSTDT.Columns.Add("CHKT", typeof(string));
                RQSTDT.Columns.Add("CHKU", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "WRK_DATE"));
                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "HIDDEN_START"));
                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "HIDDEN_END"));
                dr["LOSS_CODE"] = Util.GetCondition(cboLoss);
                dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();
                dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
                dr["USERID"] = LoginInfo.USERID;

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
                }

                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_EACH", "RQSTDT", "RSLTDT", RQSTDT);
                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS", "RQSTDT", "RSLTDT", RQSTDT);
                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                }

                //UPDATE 처리후 재조회
                GetEqptLossDetailList();

                dgDetail.ScrollIntoView(idx, 0);

                Util.AlertInfo("SFU1270");  //저장되었습니다.
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

        private void btnTotalSave_Click(object sender, RoutedEventArgs e)
        {
            TotalSaveEqptLoss();
        }

        private void TotalSaveEqptLoss()
        {
            try
            {
                Util util = new Util();

                if (util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3486");  //선택된 부동내역이 없습니다.
                    return;
                }

                if (util.GetDataGridCheckCnt(dgDetail, "CHK") == 1)
                {
                    Util.MessageValidation("SFU3487");  //일괄등록의 경우 한개 이상의 부동내역을 선택해주세요
                    return;
                }

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                {
                    Util.MessageValidation("SFU3489");  //개별등록일 경우 일괄저장 기능 사용 불가
                    return;

                }

                if (ValidationCommon() == false)
                {
                    return;
                }

                int idx = util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

                //해당Loss를 일괄로 저장하시겠습니까?
                Util.MessageConfirm("SFU3488", (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        DataSet ds = new DataSet();
                        DataTable RQSTDT = ds.Tables.Add("INDATA");
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
                        RQSTDT.Columns.Add("USERID", typeof(string));

                        DataRow dr = null;

                        for (int i = 0; i < dgDetail.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                dr = RQSTDT.NewRow();
                                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                                dr["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WRK_DATE"));
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                                dr["LOSS_CODE"] = Util.GetCondition(cboLoss);
                                dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                                dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();
                                dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                                dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                                dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                                dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
                                dr["USERID"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(dr);
                            }
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL", "RQSTDT", null, ds);
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                        //UPDATE 처리후 재조회
                        GetEqptLossDetailList();

                        dgDetail.ScrollIntoView(idx, 0);

                        Util.MessageInfo("SFU1270");    //저장되었습니다.
                    }
                });

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

        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgDetail.CurrentRow != null)
                {
                    if (dgDetail.CurrentColumn != null
                        && dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE")
                        && Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "CHECK_DELETE")).Equals("DELETE")
                        && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("N")) //삭제 더블클릭시에 실행
                    {
                        DeleteEqptLoss();
                    }
                    else
                    {
                        //ResetComponent();
                        //SetComponent(dgDetail.CurrentRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDetail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgDetail.CurrentRow != null)
                {
                    if (dgDetail.CurrentColumn != null)
                    {
                        ResetComponent();
                        SetComponent(dgDetail.CurrentRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteEqptLoss()
        {
            try
            {
                int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

                string eqptId = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                if (eqptId.Equals(""))
                {
                    return;
                }

                //삭제 하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {

                    if (result.ToString().Equals("OK"))
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("EQPTID", typeof(string));
                        RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                        RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                        RQSTDT.Columns.Add("END_DTTM", typeof(string));
                        RQSTDT.Columns.Add("USERID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["EQPTID"] = eqptId;
                        dr["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "WRK_DATE"));
                        dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                        dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);

                        ShowLoadingIndicator();

                        DataTable dtR1 = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_RESET", "RQSTDT", "RSLTDT", RQSTDT);
                        DataTable dtR2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                        //UPDATE 처리후 재조회
                        GetEqptLossDetailList();
                        if (dgDetail.GetRowCount() != 0)
                        {
                            dgDetail.ScrollIntoView(idx, 0);
                        }
                    }
                }
                );
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

        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
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
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(150, 150, 150);
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        }
                        else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
                        }
                        else
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 127, 127);
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqptLossDetailList();
        }

        private void GetEqptLossDetailList()
        {
            try
            {
                string sEqpt = Util.GetCondition(cboEquipment, "SFU1153");  //설비를 선택하세요
                if (sEqpt.Equals(""))
                {
                    return;
                }

                ResetComponent();
                ClearGrid();

                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["WRK_DATE_FROM"] = (DateTime.Now.AddDays(-1)).ToString("yyyyMMdd");
                dr["WRK_DATE_TO"] = DateTime.Now.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_FROM_TO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);


                txtRequire.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtWriteEnd.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
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

        private void ClearGrid()
        {
            Util.gridClear(dgDetail);
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }

            CMM_COM_EQPT_LOSS_DETL_FCR wndLossDetl = new CMM_COM_EQPT_LOSS_DETL_FCR();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                occurEqptFlag = !bPack && isCauseEquipment ? "Y" : "N";
                #region 
                /* 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                 * 2022.12.20 C20221216-000583 설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회
                 */
                object[] Parameters = new object[6];
                
                Parameters[0] = Convert.ToString(LoginInfo.CFG_AREA_ID);
                Parameters[1] = Convert.ToString(_ProcID);
                

                Parameters[2] = Convert.ToString(_EqptID);
                Parameters[3] = (cboLoss.SelectedValue.IsNullOrEmpty() || cboLoss.SelectedValue.ToString().Equals("SELECT")) ? "" : cboLoss.SelectedValue.ToString();
                Parameters[4] = occurEqptFlag;
                Parameters[5] = (cboOccurEqpt.SelectedValue.IsNullOrEmpty() || cboOccurEqpt.SelectedValue.ToString().Equals("SELECT")) ? string.Empty : cboOccurEqpt.SelectedValue.ToString();
                #endregion
                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_LOSS_DETL_FCR window = sender as CMM_COM_EQPT_LOSS_DETL_FCR;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                cboLossDetl.SelectedValue = window._LOSS_DETL_CODE;
                cboFailure.SelectedValue = window._FAIL_CODE;
                cboCause.SelectedValue = window._CAUSE_CODE;
                cboResolution.SelectedValue = window._RESOL_CODE;
            }
        }

        private bool event_valridtion()
        {
            //Machine 설비 사용 체크 by 오화백 2023 02 20
            //if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            //{
            //    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
            //    {
            //        if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
            //        {
            //            // 질문1 조회된 데이터가 없습니다.
            //            Util.MessageValidation("SFU1905");
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(Util.GetCondition(cboEquipment_Machine)) || Util.GetCondition(cboEquipment_Machine).Equals(""))
            //        {
            //            // 질문1 조회된 데이터가 없습니다.
            //            Util.MessageValidation("SFU1905");
            //            return false;
            //        }

            //    }
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
            //    {
            //        // 질문1 조회된 데이터가 없습니다.
            //        Util.MessageValidation("SFU1905");
            //        return false;
            //    }
            //}

            return true;
        }

        private bool ValidateFCR()
        {
            //if (cboArea.SelectedValue == null || cboArea.SelectedValue.Equals("") || cboArea.SelectedValue.Equals("SELECT"))
            //{
            //    Util.MessageValidation("SFU3206"); //동을 선택해주세요
            //    return false;
            //}

            //if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.Equals("") || cboProcess.SelectedValue.Equals("SELECT"))
            //{
            //    Util.MessageValidation("SFU3207"); //공정을 선택해주세요
            //    return false;
            //}
            return true;
        }

        private void SetTroubleUnitColumnDisplay()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = _ProcID;
                dr["EQSGID"] = _EqsgID;
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
    }

}