/*************************************************************************************
 Created Date : 2022.04.20
      Creator : 
   Decription : 권한 CHECK(동별 공통코드 사용)
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.20  DEVELOPER : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Web.Hosting;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_AUTH_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string DOMAIN_NAME = "lgcgmes.net";

        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _RUNLOT = string.Empty;
        private string _PRODID = string.Empty;
        public string UserID { get; set; }

        Util _Util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private string sComTypeCode = string.Empty;

        public MCS001_011_AUTH_CONFIRM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            sComTypeCode = Util.NVC(tmps[0]);

            if (string.IsNullOrEmpty(sComTypeCode))
                return;

            // SGC AD, 화학 AD 구분을 위하여 추가
            if (tmps.Length == 2)
                DOMAIN_NAME = Util.NVC(tmps[1]);

            txtUserID.Focus();
            
        }
        private void txtUserID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
                {
                    Util.MessageValidation("SFU4983");  // 사용자를 확인 하세요.
                    return;
                }

                if (IsAuthValidChk(txtUserID.Text.Trim()) == false)
                {
                    Util.MessageValidation("SFU4983");  // 사용자를 확인 하세요.
                    return;
                }

                txtPassWord.Focus();
            }
        }
        private void txtPassWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (string.IsNullOrEmpty(txtPassWord.Password))
                {
                    Util.MessageValidation("SFU1156"); // 비밀번호를 입력하세요.
                    return;
                }

               this.Dispatcher.BeginInvoke(new Action(() => btnConfirm_Click(null, null)));
            }
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
            {
                Util.MessageValidation("SFU1155");  // 사용자 ID를 입력하세요. 
                return;
            }

            if (!IsAuthValidChk(txtUserID.Text.Trim()))
            {
                Util.MessageValidation("SFU4983");  // 사용자를 확인 하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtPassWord.Password))
            {
                Util.MessageValidation("SFU1156"); // 비밀번호를 입력하세요.
                return;
            }

            if (!IsValidPassword(txtUserID.Text.Trim(), txtPassWord.Password))
            {
                Util.MessageValidation("SFU3188");  //비밀번호를 확인하세요.
                return;
            }

            UserID = txtUserID.Text;
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private bool IsAuthValidChk(string sUserid)
        {
            //동별 공통코드로
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComTypeCode;
                dr["COM_CODE"] = sUserid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

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

        private bool IsValidPassword(string sUserid,  string sPWD)
        {
            //동별 공통코드로
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComTypeCode;
                dr["COM_CODE"] = sUserid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return false;

                string pwd = Util.NVC(dtResult.Rows[0]["ATTR1"]);
                if (sPWD.Equals(pwd))
                    return true;
                
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion
    }
}
