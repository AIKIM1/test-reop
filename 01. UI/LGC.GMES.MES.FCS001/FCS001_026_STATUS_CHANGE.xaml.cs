/*************************************************************************************
 Created Date : 2020.10.23
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.23  DEVELOPER : Initial Created.
  2022.01.31  조영대    : Validation 추가
  2023.11.16  이정미    : 정상, 청소, 폐기 상태로 변경 시 초기화 데이터 추가 
  2024.01.08  이해령    : CNVR_ROUT_PATH_FIX_FLAG inDataTable 컬럼 추가
  2024.05.21  양강주    : TRAY 자동 세척 기능을 사용하는 경우, MMD 동별 공통코드(CARRIER_CLEAN_USE_COUNT_MNGT_PROC) 규칙을 따르도록 기능 수정
  2024.08.22  조영대    : 비정상(U) 상태 추가 및 업데이트 적용
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026_STATUS_CHANGE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sTrayID = string.Empty;
        private readonly Util _util = new Util();
        private bool _useTrayCleanOp = false;


        public FCS001_026_STATUS_CHANGE()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter = { "COMBO_TRAY_STATUS" };
            _combo.SetCombo(cboStatus, CommonCombo_Form.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            #region 활성화 공정 화면 추가 제어
            {
                _useTrayCleanOp = (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") == true && _util.IsAreaCommoncodeAttrUse("CARRIER_CLEAN_USE_COUNT_MNGT_PROC", "FORM_TRAY", new string[] { "" }) == true);
            }
            #endregion
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _sTrayID = Util.NVC(tmps[0]);
            }
            else
            {
                _sTrayID = "";
            }

            InitCombo();

        }
        #endregion

        #region [Method]
        private void TryaStatusChange()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
            inDataTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REMARK_SCRAP", typeof(string));
            inDataTable.Columns.Add("CNVR_ROUT_PATH_FIX_FLAG", typeof(string));
            inDataTable.Columns.Add("SET_USE_COUNT", typeof(Int64));
            inDataTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["CSTID"] = _sTrayID;
            string state = Util.GetCondition(cboStatus, bAllNull: true);
            if (string.IsNullOrEmpty(state)) { Util.Alert("FM_ME_0137"); return; }

            String toClean = (_useTrayCleanOp == true) ? "Q" : "Y";
            String cleanOK = (_useTrayCleanOp == true) ? "Y" : "N";

            switch (state)
            {
                case "ONCLEAN":
                    dr["CST_CLEAN_FLAG"] = toClean;
                    dr["CNVR_ROUT_PATH_FIX_FLAG"] = "N";
                    break;
                case "NORMAL":
                    dr["CST_CLEAN_FLAG"] = cleanOK;
                    dr["CST_MNGT_STAT_CODE"] = "I";
                    dr["CNVR_ROUT_PATH_FIX_FLAG"] = "N";
                    dr["SET_USE_COUNT"] = 0;
                    break;
                case "DISUSE":
                    dr["CST_CLEAN_FLAG"] = cleanOK;
                    dr["CST_MNGT_STAT_CODE"] = "S";
                    dr["CNVR_ROUT_PATH_FIX_FLAG"] = "N";
                    break;
                case "U":
                    dr["ABNORM_TRF_RSN_CODE"] = state;
                    break;
                default: break;
            }

            dr["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(dr);

            // 폐기 Tray일 경우 REMARK 필수
            if (cboStatus.SelectedValue.ToString().Equals("DISUSE"))
            {
                dr["REMARK_SCRAP"] = Util.GetCondition(txtScrapRemark);
                if (string.IsNullOrEmpty(dr["REMARK_SCRAP"].ToString()))
                {
                    Util.Alert("ME_0381"); //폐기사유를 입력해주세요.
                    return;
                }
            }
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CHANGE_TRAY_STATUS", "INDATA", "OUTDATA", inDataTable);

            this.DialogResult = MessageBoxResult.No;
            Util.MessageInfo("FM_ME_0136");  //변경완료하였습니다.
        }
        #endregion

        #region [Event]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            if (cboStatus.GetBindValue() == null)
            {
                // 변경할 상태를 선택해 주세요.
                cboStatus.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0137"));
                return;
            }

            Util.MessageConfirm("FM_ME_0337", (result) => //상태를 변경하시겠습니까?
             {
                 if (result == MessageBoxResult.OK)
                 {
                     try
                     {
                         TryaStatusChange();
                     }
                     catch (Exception ex)
                     {
                         Util.MessageException(ex);
                     }
                     Close();

                 }
                 else { return; }
             });
        }
        private void cboStatus_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null || e == null) return;
            else
            {
                if (e.NewValue.Equals("DISUSE"))
                {
                    txtScrapRemark.Style = Application.Current.Resources["Content_InputForm_MandatoryTextBoxStyle"] as Style;
                }
                else
                {
                    txtScrapRemark.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                }
            }
        }

        #endregion
    }
}
