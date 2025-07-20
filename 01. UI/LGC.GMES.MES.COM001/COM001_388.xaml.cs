/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2023.08.03 성민식 : 최초생성
  2023.10.19 성민식 : SBC 방화벽 문제로 하이퍼링크 제거 및 링크 복사하여 접속 할 수 있도록 유도
  2023.10.26 이병윤 : 공통그룹코드[GMES_NOTICE]에 등록된 공지사항 가져오기 변경
  2023.11.02 이병윤 : 공지내용이 없을경우 팝업을 띄우지 않고, 공지내용 조회 변경 처리 
  2023.12.11 국유채 : 플랜트/동 권한 체크 후 권한 여부에 따른 공지 팝업 분기 추가
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_388 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string sSeqNo = null;
        string parameter = string.Empty;
        bool isInitAuthority = false;

        public COM001_388()
        {
            InitializeComponent();
            //InitCombo();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Loaded(object sender, EventArgs e)
        {
            //권한유무체크 (23-12-11)
            CheckAuth();
            
            SetNotice();

            parameter = C1WindowExtension.GetParameter(this);
        }
        #endregion

        #region Event
        private void SetNotice()
        {
            this.Loaded -= UserControl_Loaded;

            
            GetText(LoginInfo.LANGID);
        }
        #endregion

        #region Method
        private void CheckAuth()
        {
            Logger.Instance.WriteLine("[FRAME] Initial Authority", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            DataTable loginAuthTable = new DataTable();
            loginAuthTable.Columns.Add("USERID", typeof(string));
            DataRow authIndata = loginAuthTable.NewRow();
            authIndata["USERID"] = LoginInfo.USERID;
            loginAuthTable.Rows.Add(authIndata);

            DataTable dtAuthority = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AREA_AUTH_CONN", "INDATA", "OUTDATA", loginAuthTable);

            isInitAuthority = false;
            if (dtAuthority == null || dtAuthority.Rows.Count == 0)
                isInitAuthority = true;

            if (isInitAuthority == true)
            {
                DataTable dtComCode = new ClientProxy().ExecuteServiceSync("CUS_SEL_LANGID", null, "OUTDATA", null);

                if (dtComCode != null && dtComCode.Rows.Count > 0 && !string.IsNullOrEmpty(Convert.ToString(dtComCode.Rows[0]["LANGID"])))
                    LoginInfo.LANGID = Convert.ToString(dtComCode.Rows[0]["LANGID"]);
            }

            Logger.Instance.WriteLine("[FRAME] Initial Authority", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        private void GetText(string sLangid)
        {
            
            //string sHyperLink = "itsm.lgensol.com/cm/service/ssologin.ncd?gbn=AM&mid=XAMM2099";

            //Hyperlink hl = new Hyperlink(new Run(sHyperLink));
            //hl.Inlines.Clear();
            //if (String.Equals(sLangid, "ko-KR"))
            //{
            //    hl.Inlines.Add("권한관리시스템 접속링크");
            //}
            //else if (String.Equals(sLangid, "zh-CN"))
            //{
            //    hl.Inlines.Add("权限管理系统访问链接");
            //}
            //else
            //{
            //    hl.Inlines.Add("Permission management system URL");
            //}
            //hl.NavigateUri = new Uri("http://" + sHyperLink);
            //hl.RequestNavigate += (sender, e) =>
            //{
            //    System.Diagnostics.Process.Start(e.Uri.ToString());
            //    e.Handled = true;
            //};

            //if(String.Equals(sLangid, "ko-KR"))
            //{
            //    txtNotice.Text = "GMES/MMD 권한신청 가이드\n\n전사 권한관리시스템이 오픈했습니다.\n\nGMES / MMD 권한 신청은\n1) IT Help Desk 에서 권한신청 CSR 신청시 해당 시스템 선택하면 권한관리시스템으로 이동\n2) 공지사항 URL 선택시 권한신청 화면으로 이동\n\n기존 : IT Help Desk를 통한 접속 1) IT HelpDesk - IT서비스 - IT 서비스 요청 - ID/권한신청 - MES/MMD 시스템 검색후 클릭\n     - 이동 팝업 Yes 선택 - IT 시스템 권한 신청 기능 화면으로 자동 이동\n\nitsm.lgensol.com/cm/service/ssologin.ncd?gbn=AM&mid=XAMM2099\nEnPortal 로그인 상태에서 위 링크를 복사하여 접속";
            //}
            //else if(String.Equals(sLangid, "zh-CN"))
            //{
            //    txtNotice.Text = "全公司权限管理系统开通。\n\nGMES / MMD权限申请\n1) 在IT帮助台申请权限申请CSR时选择相关系统移动到权限管理系统\n2) 选择通知URL时移动到许可申请屏幕\n\n现有：通过 IT Help Desk 访问 1) IT HelpDesk - IT 服务 - IT 服务请求 - ID/权限请求 - 搜索 MES/MMD 系统并单击\n- 在移动弹出窗口中选择是 - 自动移动到IT系统权限申请功能屏幕\n\nitsm.lgensol.com/cm/service/ssologin.ncd?gbn=AM&mid=XAMM2099\n登录 EnPortal 后复制并访问上述链接。";
            //}
            //else
            //{
            //    txtNotice.Text = "The company-wide rights management system has been opened.\n\nGMES / MMD authority application\n1) Applying for authority at the IT help desk When applying for CSR, select the relevant system to move to the authority management system\n2) Move to the permission application screen when selecting the notice URL\n\nExisting: Access through IT Help Desk 1) IT HelpDesk - IT Service - IT Service Request - ID/Authority Request - Search for MES/MMD system and click\n- Select Yes in the move pop-up - Automatically move to the IT system permission application function screen\n\nitsm.lgensol.com/cm/service/ssologin.ncd?gbn=AM&mid=XAMM2099\nCopy and access the above link while logged in to EnPortal.";
            //}

            //txtNotice.Inlines.Add(hl);
            try
            {
                if (isInitAuthority)
                {
                    DataTable RQSTDT = new DataTable("RQSTDT");
                    RQSTDT.Columns.Add("CMCDTYPE");
                    RQSTDT.Columns.Add("AREAID");


                    DataRow dr = RQSTDT.NewRow();
                    dr["CMCDTYPE"] = "GMES_NOTICE";
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_NOTICE", "RQSTDT", "RSLTDT", RQSTDT);
                    string sNotice = string.Empty;
                    if (dtRslt.Rows.Count > 0)
                    {
                        if ((Util.IsNVC(dtRslt.Rows[0]["NOTICE"])))
                        {
                            this.DialogResult = MessageBoxResult.Cancel;
                            return;
                        }
                        sNotice = dtRslt.Rows[0]["NOTICE"].ToString().Replace("\\n", "\r\n");
                        txtNotice.Text = sNotice + "\r\n\r\n" + "=========" + "\r\n\r\n" + MessageDic.Instance.GetMessage("SFU10010");
                    }
                    else
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }
                else
                {
                    DataTable RQSTDT = new DataTable("RQSTDT");
                    RQSTDT.Columns.Add("CMCDTYPE");
                    RQSTDT.Columns.Add("AREAID");


                    DataRow dr = RQSTDT.NewRow();
                    dr["CMCDTYPE"] = "GMES_NOTICE";
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_NOTICE", "RQSTDT", "RSLTDT", RQSTDT);
                    string sNotice = string.Empty;
                    if (dtRslt.Rows.Count > 0)
                    {
                        if ((Util.IsNVC(dtRslt.Rows[0]["NOTICE"])))
                        {
                            this.DialogResult = MessageBoxResult.Cancel;
                            return;
                        }
                        sNotice = dtRslt.Rows[0]["NOTICE"].ToString().Replace("\\n", "\r\n");
                        txtNotice.Text = sNotice;
                    }
                    else
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }
                
            }
            catch
            {
                // DA가 배포되지 않을 경우 처리
                this.DialogResult = MessageBoxResult.Cancel;
            }


            
        }
        #endregion
    }
}