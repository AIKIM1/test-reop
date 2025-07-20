/*************************************************************************************
 Created Date : 2024.02.14
      Creator : 
   Decription : 버퍼 수동 초기화
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.14  이민영 : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_COATER_SLURRY_BUFFER_INIT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_COATER_SLURRY_BUFFER_INIT : C1Window, IWorkArea
    {

        #region Initialize
        private string _EQPTID = string.Empty;
        private string _AREAID = string.Empty;
        private string _BATCHID = string.Empty;
        private string _EQPTPSTNID = string.Empty;

        private bool iscboEqptPstnIdBeforeOpen = true;
        public CMM_ELEC_COATER_SLURRY_BUFFER_INIT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _EQPTID = Util.NVC(tmps[0]);
            _AREAID = Util.NVC(tmps[1]);

            if (_EQPTID.IsNullOrEmpty() || _AREAID.IsNullOrEmpty())
                this.DialogResult = MessageBoxResult.Cancel;

            txtEqptId.Text = _EQPTID;
        }

        private void btnInit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _EQPTPSTNID = string.Empty;
            _BATCHID = string.Empty;
            
            if (cboEqptPstnID.SelectedIndex < 0 || cboEqptPstnID.SelectedValue.GetString().IsNullOrEmpty())
            {   
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("버퍼위치ID")); //%1(을)를 선택하세요.
                return;
            }
            _EQPTPSTNID = cboEqptPstnID.SelectedValue.GetString();

            //전체초기화가 아닐경우 선택된 배치ID 확인
            //확인된 배치ID에 대해 Wip 체크, 버퍼 배치ID 체크
            if (chkAllEnd.IsChecked == false)
            {
                if (cboBatchId.SelectedIndex < 0)
                {
                    //선택된 아이템이 없고 입력된 값이 있을 경우 대문자 변환하여 할당한다.
                    //선택된 아이템이 없고 입력된 값이 없을 경우 return
                    if (cboBatchId.Text.IsNullOrEmpty())
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("배치ID")); //%1(을)를 선택하세요.
                        return;
                    }

                    _BATCHID = cboBatchId.Text.Trim().ToUpper();
                    cboBatchId.Text = _BATCHID;

                }
                else
                {
                    //선택된 아이템이 있고 value가 있으면 선택된 value를 할당한다.
                    //선택된 아이템이 있고 value가 NullOrEmpty면 return
                    if (cboBatchId.SelectedValue.GetString().IsNullOrEmpty())
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("배치ID")); //%1(을)를 선택하세요.
                        return;
                    }
                    _BATCHID = cboBatchId.SelectedValue.GetString();
                }

                if (ValidationWipBatch() == false)
                    return;

                ValidationBufferBatchInfo(false);
            }
            else //전체초기화시
            {
                _BATCHID = string.Empty;
                ValidationBufferBatchInfo(true);
            }
            
        }

        private void cboEqptPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEqptPstnID.SelectedIndex < 0 || cboEqptPstnID.SelectedValue.GetString().IsNullOrEmpty())
            {
                return;
            }

            if (iscboEqptPstnIdBeforeOpen == true)
                return;

            _EQPTPSTNID = cboEqptPstnID.SelectedValue.GetString();

            //대기 배치ID 콤보 조회
            SetBatchComboBox();
        }

        private void cboEqptPstnID_IsDropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (cboEqptPstnID.ItemsSource != null)
                return;
            //설비의 버퍼위치ID조회
            SetEqptPstnComboBox();

            //최초 조회시 배치ID자동 조회되지 않도록 flag처리
            if (iscboEqptPstnIdBeforeOpen == true)
            {
                cboEqptPstnID.SelectedIndex = -1;
                iscboEqptPstnIdBeforeOpen = false;
            }
        }

        private void chkAllEnd_Checked(object sender, RoutedEventArgs e)
        {
            if (chkAllEnd.IsChecked == true) //전체초기화 선택시 배치ID 비활성화
            {
                if (this.cboBatchId.ItemsSource != null && this.cboBatchId.SelectedIndex >= 0)
                {
                    this.cboBatchId.SelectedIndex = -1;
                }
                this.cboBatchId.Text = string.Empty;
                this.cboBatchId.IsEnabled = false;
            }
            else
            {
                this.cboBatchId.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        

        #endregion



        #region UserMethod
                
        /// <summary>
        /// 설비기준 버퍼위치 조회
        /// </summary>
        private void SetEqptPstnComboBox()
        {
            try
            {
                cboEqptPstnID.ItemsSource = null;
                const string bizRuleName = "DA_PRD_SEL_CT_SLURRY_MOUNT_PSTN_CBO_RM";
                string[] arrColumn = { "LANGID", "AREAID", "EQPTID" };
                string[] arrCondition = { LoginInfo.LANGID, _AREAID, _EQPTID };
                string selectedValueText = "CBO_CODE";
                string displayMemberText = "CBO_NAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cboEqptPstnID, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 설비, 버퍼위치 기준 대기중인 배치ID 조회
        /// </summary>
        /// <param name="EqptMountPstnId"></param>
        private void SetBatchComboBox()
        {
            try
            {
                cboBatchId.ItemsSource = null;
                const string bizRuleName = "DA_PRD_SEL_MIXER_TANK_CURR_MOUNT_MTRL_CBO_RM";
                string[] arrColumn = { "LANGID", "EQPTID", "EQPT_MOUNT_PSTN_ID" };
                string[] arrCondition = { LoginInfo.LANGID, _EQPTID, _EQPTPSTNID };
                string selectedValueText = "CBO_CODE";
                string displayMemberText = "CBO_NAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cboBatchId, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
                cboBatchId.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// WIP Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationWipBatch()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WIPSEQ", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = _BATCHID;
                dr["WIPSEQ"] = "1";
                dr["AREAID"] = _AREAID;

                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_CT_SLURRY_BUFFER_MANUAL_ACTION_RM", "INDATA", null, dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 버퍼 배치 정보 체크
        /// </summary>
        /// <returns></returns>
        private void ValidationBufferBatchInfo(bool isAllEnd)
        {
            //1. 전체초기화시 : 대기배치 존재여부 확인
            //2. 배치ID 선택시 : 설비/버퍼위치ID/배치ID기준 정보조회, 투입수량 체크
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dt.Columns.Add("INPUT_LOTID", typeof(string));
                dt.Columns.Add("INPUT_FLAG", typeof(string));
                
                DataRow dr = dt.NewRow();
                dr["EQPTID"] = _EQPTID;
                dr["EQPT_MOUNT_PSTN_ID"] = _EQPTPSTNID;
                
                if (isAllEnd) //전체초기화시 설비/버퍼위치ID기준 대기배치 존재여부 확인
                {
                    dr["INPUT_FLAG"] = "Y,N";
                }
                else // 배치ID 있을 경우 설비/버퍼위치ID/배치ID기준 정보조회
                {
                    dr["INPUT_LOTID"] = _BATCHID;
                    dr["INPUT_FLAG"] = "Y,N,E";
                }
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_MIXER_TANK_CURR_MOUNT_MTRL_RM", "RQSTDT", "RSLTDT", dt);

                if(CommonVerify.HasTableRow(result) == false)
                {
                    if(isAllEnd)
                    {
                        Util.MessageValidation("FM_ME_0240");  // 처리할 데이터가 없습니다.
                        return;
                    }
                    else
                    {
                        Util.MessageValidation("SFU1670");  // 설비 배치ID에 해당하는 데이터가 없습니다.
                        return;
                    }
                }

                if (isAllEnd)
                {
                    //전체 초기화 진행
                    RunBufferInitialize();
                    return;
                }
                    

                //투입수량 (INPUT_QTY)이 0인경우 진행여부 확인
                decimal inputQty = Util.NVC_Decimal(result.Rows[0]["INPUT_QTY"]);
                
                if(inputQty == 0 || "0.00000".Equals(result.Rows[0]["INPUT_QTY"] == null ? "0.00000" : result.Rows[0]["INPUT_QTY"].ToString()))
                {
                    // 투입수량이 0이라서 사용되지 않을 수 있습니다. 진행하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            //선택 배치기준 초기화 진행
                            RunBufferInitialize();
                            return;
                        }

                    });
                }
                else
                {
                    //선택 배치기준 초기화 진행
                    RunBufferInitialize();
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 초기화 처리
        /// </summary>
        private void RunBufferInitialize()
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataSet inData = new DataSet();
            //설비 정보
            DataTable inEqp = inData.Tables.Add("IN_EQP");
            inEqp.Columns.Add("SRCTYPE", typeof(string));  //소스Type
            inEqp.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
            inEqp.Columns.Add("EQPTID", typeof(string));   //설비
            inEqp.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));   //버퍼장착위치
            inEqp.Columns.Add("ACT_TYPE", typeof(string));   //처리구분
            inEqp.Columns.Add("USERID", typeof(string));

            DataRow row = null;
            row = inEqp.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["EQPTID"] = _EQPTID;
            row["EQPT_MOUNT_PSTN_ID"] = _EQPTPSTNID;
            row["ACT_TYPE"] = chkAllEnd.IsChecked == true ? "ALL_END" : "LAST_ONE_REMAIN";
            row["USERID"] = LoginInfo.USERID;
            inEqp.Rows.Add(row);

            //배치ID 정보
            DataTable inLot = inData.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID", typeof(string));//작업일지 항목

            row = inLot.NewRow();
            row["LOTID"] = chkAllEnd.IsChecked == true ? "" : _BATCHID;

            inLot.Rows.Add(row);

            string bizRuleName = "BR_PRD_REG_CT_SLURRY_BUFFER_MANUAL_ACTION_RM";

            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0239"); //처리가 완료되었습니다.
                        SetBatchComboBox();
                    }
                    
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, inData);
        }

        #endregion

        
    }
}
