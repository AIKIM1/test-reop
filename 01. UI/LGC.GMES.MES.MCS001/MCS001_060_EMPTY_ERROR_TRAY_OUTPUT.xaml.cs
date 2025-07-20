/*************************************************************************************
 Created Date : 2021.05.11
      Creator : 오화백
   Decription : 고공 CNV 현황조회 - 공Tray, 오류Tray 출고예약 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.11  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_002_CHANGE_PORT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_060_EMPTY_ERROR_TRAY_OUTPUT : C1Window, IWorkArea
    {
        #region Initialize
        private DataTable dtRack = null;
        private string _EqptID = string.Empty;// 설비정보
        private bool _load = true;
   
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }
        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_060_EMPTY_ERROR_TRAY_OUTPUT()
        {
            InitializeComponent();
        }
      
        public IFrameOperation FrameOperation { get; set; }

        /// <summary>
        /// 화면로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_load)
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps == null)
                    return;
                dtRack = tmps[0] as DataTable;
                _EqptID = tmps[1] as string;
                this.InitCombo();

                _load = false;
            }
          
        }

        /// <summary>
        /// 콤보박스 조회
        /// </summary>
        private void InitCombo()
        {
            SetOutPutPort(cboOutputPort);                                        // 출고포트 
        }

        #endregion

        #region Event

        #region 출고예약 버튼처리 : btnOutPut_Click()
        private void btnOutPut_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidationReturn())
                return;

            // 출고예약 하시겠습니까?
            Util.MessageConfirm("SFU8360", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    OutPut();
                }
            });
        }
        #endregion

        #region 닫기 : btnClose_Click()

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #endregion

        #region Method

        #region 출고 Port 팝업 조회 : SetMachineCombo()
        /// <summary>
        /// 출고 PORT 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetOutPutPort(C1ComboBox cbo)
        {
           
            const string bizRuleName = "BR_GUI_GET_GET_TRF_DEST";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
       
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = _EqptID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataTable dtBinding = dtResult.Copy();
            DataRow newRow = dtBinding.NewRow();

            newRow[cbo.SelectedValuePath] = null;
            newRow[cbo.DisplayMemberPath] = "-SELECT-";
            dtBinding.Rows.InsertAt(newRow, 0);

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
        #endregion
        
        #region 출고예약 처리  : OutPut()
        /// <summary>
        /// Port 변경
        /// </summary>

        private void OutPut()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));



                for (int i = 0; i < dtRack.Rows.Count; i++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["CARRIERID"] = GetFirstCstid(_EqptID, dtRack.Rows[i]["RACK"].ToString());
                    newRow["SRC_LOCID"] = dtRack.Rows[i]["RACK"].ToString();
                    newRow["DST_LOCID"] = cboOutputPort.SelectedValue.ToString();
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["DTTM"] = dtSystem;
                    inTable.Rows.Add(newRow);

                }


                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", "OUT_REQ_TRF_INFO", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

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

        #endregion

        #region 출고예약 Validation : ValidationReturn()
        private bool ValidationReturn()
        {
            if (cboOutputPort.SelectedIndex <= 0)
            {
                //출고port를 선택하세요
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고Port"));
                return false;
            }

            return true;
        }

        #endregion

        #region MCS 비즈 접속 정보 : GetBizActorServerInfo()
        /// <summary>
        /// MCS 비즈 접속 정보
        /// </summary>
        private void GetBizActorServerInfo()
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

        }



        #endregion

        #region LoadingIndicator : ShowLoadingIndicator(),HiddenLoadingIndicator()

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

        #region 출고예약 시  등록일자 셋팅 : GetSystemTime()
        /// <summary>
        /// 등록일자 셋팅
        /// </summary>
        /// <returns></returns>
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
        #endregion


        #region 출고예약 시 해당 RACK의 첫번째 CSTID 값 셋팅 : GetFirstCstid()
        /// <summary>
        /// CSTID 조회
        /// </summary>
        /// <returns></returns>
        private string GetFirstCstid(string Eqptid, string RackID)
        {
            string FirstCstid = string.Empty;

            const string bizRuleName = "DA_GMES_CELL_TRAY_BUNDLE_INFO_BY_RACKID";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("RACK_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = Eqptid;
            dr["RACK_ID"] = RackID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                FirstCstid = dtResult.Rows[0]["CSTID"].ToString();
            }

            return FirstCstid;
        }

        #endregion







        #endregion


    }
}
