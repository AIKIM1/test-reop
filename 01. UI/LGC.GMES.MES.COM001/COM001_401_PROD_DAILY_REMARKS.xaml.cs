/*************************************************************************************
 Created Date : 2024.04.17
      Creator : 김도형
   Decription : 특이사항 ->전극 생산일별 특이사항(NEW)-> 생산 일별 노트 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.17  김도형 : Initial Created.
  2024.04.17  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건
  2024.04.20  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건( PRODID_BY_PJT)
  2024.06.03  김도형 : [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Forms.VisualStyles;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_401_PROD_DAILY_REMARKS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_401_PROD_DAILY_REMARKS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _SaveMode = string.Empty;
        private string _INPUTSEQNO = string.Empty;
        private string _selectedAreaCode = string.Empty;
        private string _selectedEquipmentSegmentCode = string.Empty;
        private string _selectedProcess = string.Empty;
        private string _selectedEquipment = string.Empty;
        private string _CLSS_TYPE = string.Empty;



        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private Util _util = new Util();
        #endregion

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_401_PROD_DAILY_REMARKS()
        {
            InitializeComponent();
        }
         

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
          // try
          // {
          //     InitCombo();
          // }
          // catch (Exception ex)
          // {
          //     Util.MessageException(ex);
          // }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _SaveMode = tmps[0].GetString();
                    _INPUTSEQNO = tmps[1].GetString();
                    _selectedAreaCode = tmps[2].GetString();
                    _selectedEquipmentSegmentCode = tmps[3].GetString();
                    _selectedProcess = tmps[4].GetString();
                    _selectedEquipment = tmps[5].GetString();
                    _CLSS_TYPE = tmps[6].GetString();
                }

                ApplyPermissions();
                InitializeControls();
                InitCombo();

                SetControlsValue();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #region Initialize 
        private void InitializeControls()
        {
            if (dpAct != null)
                dpAct.SelectedDateTime = GetSystemTime();

            if (teAct != null)
                teAct.Value = new TimeSpan(GetSystemTime().Hour, GetSystemTime().Minute, GetSystemTime().Second);
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS");

            //설비
            C1ComboBox[] cboEquipmentChild = { cboProdid, cboPreProdid };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, cbParent: cboEquipmentParent, sCase: "EQUIPMENT");

          //생산모델
           C1ComboBox[] cboMountPstParent1 = { cboEquipmentSegment, cboProcess, cboEquipment };
           _combo.SetCombo(cboProdid, CommonCombo.ComboStatus.SELECT, cbParent: cboMountPstParent1, sCase: "cboProductsPjt");
         
           //변경전모델
           C1ComboBox[] cboMountPstParent2 = { cboEquipmentSegment, cboProcess, cboEquipment };
           _combo.SetCombo(cboPreProdid, CommonCombo.ComboStatus.SELECT, cbParent: cboMountPstParent2, sCase: "cboProductsPjt");

            String[] sFilter = { "MDL_CHG_FLAG" };
            String[] sFilter1 = { "USE_FLAG" };

            // 모델변경여부
            _combo.SetCombo(cboMdlChgFlag, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            //구분
            _combo.SetCombo(cboUseFlag, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE"); 
        }

        private void SetControlsValue()
        {
            if(_SaveMode.Equals("INS")) //신규
            {
                cboArea.SelectedValue = _selectedAreaCode;
                cboEquipmentSegment.SelectedValue = _selectedEquipmentSegmentCode ;
                cboProcess.SelectedValue = _selectedProcess ;
                cboEquipment.SelectedValue = _selectedEquipment;
            }else
            {
                if (!GetCheckProcessChk(_INPUTSEQNO))
                {
                    Util.MessageInfo("SFU9212");  // 확인 처리 되었습니다. 수정 불가
                    this.DialogResult = MessageBoxResult.OK;
                    return;
                }

                GetPrdtDailyRemarksInfo(_INPUTSEQNO);
            }
        }
        #endregion Initialize

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            if (_SaveMode.Equals("INS"))
            {
                Util.MessageConfirm("SFU1241", (result) =>  // 저장하시겠습니까
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveAdd();
                    }
                }); 
            }
            else
            {
                if(!GetCheckProcessChk(_INPUTSEQNO))
                {
                    Util.MessageInfo("SFU9212");  // 확인 처리 되었습니다. 수정 불가
                    this.DialogResult = MessageBoxResult.OK;
                    return;
                }
                Util.MessageConfirm("SFU4340", (result) =>  // 수정하시겠습니까
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveMod();
                    }
                }); 
            }
        }
         
        #endregion  Event

        #region Mehod


        #region [BizCall]
        private void SaveAdd()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_INS_TB_SFC_PRDT_DAILY_REMARKS";

                // DateTime dtActTime;
                // if (teAct.Value.HasValue)
                // {
                //     var spn = (TimeSpan)teAct.Value;
                //     dtActTime = new DateTime(dpAct.SelectedDateTime.Year, dpAct.SelectedDateTime.Month, dpAct.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                // }
                // else
                // {
                //     dtActTime = new DateTime(dpAct.SelectedDateTime.Year, dpAct.SelectedDateTime.Month, dpAct.SelectedDateTime.Day);
                // }

                string sDtActTime = dpAct.SelectedDateTime.Year.ToString() + "-" + dpAct.SelectedDateTime.Month.ToString() + "-" + dpAct.SelectedDateTime.Day + " " + Convert.ToString(teAct.Value);


                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("ACTDTTM", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CLSS_TYPE", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("MDL_CHG_FLAG", typeof(string));
                inDataTable.Columns.Add("PRE_PRODID", typeof(string));
                inDataTable.Columns.Add("EQPT_REMARKS", typeof(string));
                inDataTable.Columns.Add("PROD_CATN", typeof(string));
                inDataTable.Columns.Add("REMARKS", typeof(string)); 
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea) == "" ? (object)DBNull.Value : Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? (object)DBNull.Value : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess) == "" ? (object)DBNull.Value : Util.GetCondition(cboProcess);
                dr["ACTDTTM"] = sDtActTime;
                dr["EQPTID"] = Util.GetCondition(cboEquipment) == "" ? (object)DBNull.Value : Util.GetCondition(cboEquipment);
                dr["CLSS_TYPE"] = _CLSS_TYPE;
                dr["PRODID"] = Util.GetCondition(cboProdid) == "" ? (object)DBNull.Value : Util.GetCondition(cboProdid);
                dr["MDL_CHG_FLAG"] = Util.GetCondition(cboMdlChgFlag) == "" ? (object)DBNull.Value : Util.GetCondition(cboMdlChgFlag);
                dr["PRE_PRODID"] = Util.GetCondition(cboPreProdid) == "" ? (object)DBNull.Value : Util.GetCondition(cboPreProdid);
                dr["EQPT_REMARKS"] = txtEqptRemarks.Text.ToString(); // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["PROD_CATN"] = txtProdCatn.Text.ToString();      // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["REMARKS"] = txtRemarks.Text.ToString();         // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag); ;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU3532");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
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

        private void SaveMod()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_UPD_TB_SFC_PRDT_DAILY_REMARKS";

                // DateTime dtActTime;
                // if (teAct.Value.HasValue)
                // {
                //     var spn = (TimeSpan)teAct.Value;
                //     dtActTime = new DateTime(dpAct.SelectedDateTime.Year, dpAct.SelectedDateTime.Month, dpAct.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                // }
                // else
                // {
                //     dtActTime = new DateTime(dpAct.SelectedDateTime.Year, dpAct.SelectedDateTime.Month, dpAct.SelectedDateTime.Day);
                // }

                string sDtActTime = dpAct.SelectedDateTime.Year.ToString() + "-" + dpAct.SelectedDateTime.Month.ToString() + "-" + dpAct.SelectedDateTime.Day + " " + Convert.ToString(teAct.Value);

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("INPUTSEQNO", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("ACTDTTM", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CLSS_TYPE", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("MDL_CHG_FLAG", typeof(string));
                inDataTable.Columns.Add("PRE_PRODID", typeof(string));
                inDataTable.Columns.Add("EQPT_REMARKS", typeof(string));
                inDataTable.Columns.Add("PROD_CATN", typeof(string));
                inDataTable.Columns.Add("REMARKS", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["INPUTSEQNO"] = _INPUTSEQNO;
                dr["AREAID"] = Util.GetCondition(cboArea) == "" ? (object)DBNull.Value : Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? (object)DBNull.Value : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess) == "" ? (object)DBNull.Value : Util.GetCondition(cboProcess);
                dr["ACTDTTM"] = sDtActTime;
                dr["EQPTID"] = Util.GetCondition(cboEquipment) == "" ? (object)DBNull.Value : Util.GetCondition(cboEquipment);
                dr["CLSS_TYPE"] = _CLSS_TYPE;
                dr["PRODID"] = Util.GetCondition(cboProdid) == "" ? (object)DBNull.Value : Util.GetCondition(cboProdid);
                dr["MDL_CHG_FLAG"] = Util.GetCondition(cboMdlChgFlag);
                dr["PRE_PRODID"] = Util.GetCondition(cboPreProdid) == "" ? (object)DBNull.Value : Util.GetCondition(cboPreProdid);
                dr["EQPT_REMARKS"] = txtEqptRemarks.Text.ToString();  // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["PROD_CATN"] = txtProdCatn.Text.ToString();        // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["REMARKS"] = txtRemarks.Text.ToString();           // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag); ;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU3532");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
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
        
        private bool GetCheckProcessChk(string sInputSeqno)
        {
            bool bChkResult = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPUTSEQNO", typeof(string)); 

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUTSEQNO"] = sInputSeqno; 

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_DAILY_REMARKS_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if(Util.NVC(dtResult.Rows[0]["CHKUSER"].ToString()).Equals(""))
                    {
                        bChkResult = true;
                    }
                    else
                    {
                        bChkResult = false;
                    }                   
                }
                else
                {
                    bChkResult = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bChkResult = true;
            }

            return bChkResult;
        }
       
        private void GetPrdtDailyRemarksInfo(string sInputSeqno)
        {
            DateTime systemDateTime = new DateTime();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPUTSEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUTSEQNO"] = sInputSeqno;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_DAILY_REMARKS_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    cboArea.SelectedValue = Util.NVC(dtResult.Rows[0]["AREAID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["AREAID"].ToString());
                    cboEquipmentSegment.SelectedValue = Util.NVC(dtResult.Rows[0]["EQSGID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["EQSGID"].ToString());
                    cboProcess.SelectedValue = Util.NVC(dtResult.Rows[0]["PROCID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["PROCID"].ToString());
                    cboEquipment.SelectedValue = Util.NVC(dtResult.Rows[0]["EQPTID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["EQPTID"].ToString());
                    cboProdid.SelectedValue = Util.NVC(dtResult.Rows[0]["PRODID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["PRODID"].ToString());
                    cboMdlChgFlag.SelectedValue = Util.NVC(dtResult.Rows[0]["MDL_CHG_FLAG"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["MDL_CHG_FLAG"].ToString()); 
                    cboPreProdid.SelectedValue = Util.NVC(dtResult.Rows[0]["PRE_PRODID"].ToString()) == "" ? "SELECT" : Util.NVC(dtResult.Rows[0]["PRE_PRODID"].ToString()); 
                                        
                    systemDateTime = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["ACTDTTM"].ToString()));
                    dpAct.SelectedDateTime = systemDateTime;
                    teAct.Value = new TimeSpan(systemDateTime.Hour, systemDateTime.Minute, systemDateTime.Second);

                    this.txtEqptRemarks.Text = Util.NVC(dtResult.Rows[0]["EQPT_REMARKS"].ToString());
                    this.txtProdCatn.Text = Util.NVC(dtResult.Rows[0]["PROD_CATN"].ToString());
                    this.txtRemarks.Text = Util.NVC(dtResult.Rows[0]["REMARKS"].ToString());

                    cboUseFlag.SelectedValue = Util.NVC(dtResult.Rows[0]["USE_FLAG"].ToString());
                }
                else
                {
                    Util.MessageInfo("SFU1498"); //  데이터가 없습니다.
                    this.DialogResult = MessageBoxResult.Cancel; 
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex); 
            }

            return ;
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_CUS_GET_SYSTIME";

            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        #endregion [BizCall]

        #region [Validation]

        private bool ValidationSave()
        { 
          // 우선은 제약 조건 없이 저장 및 수정 가능
           if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
           {
               //동을 선택하세요.
               Util.MessageValidation("SFU1499");
               return false;
           }
           if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
           {
               //라인을 선택 하세요.
               Util.MessageValidation("SFU1223");
               return false;
           }
           if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
           {
               //공정을선택하세요.
               Util.MessageValidation("SFU1459");
               return false;
           } 
           if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
           {
               // 설비를 선택하세요.
               Util.MessageValidation("SFU1673");
               return false;
           }
              
            return true;
        }
        #endregion  [Validation]

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> {btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion [Func]

        #endregion Mehod


    }
}
