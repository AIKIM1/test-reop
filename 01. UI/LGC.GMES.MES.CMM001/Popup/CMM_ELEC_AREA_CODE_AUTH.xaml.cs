/*************************************************************************************
 Created Date : 2022.07.29
      Creator : 정재홍
   Decription : 동별코드 기준정보 권한 ChecK
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.29  DEVELOPER : Initial Created.
   
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// 권한체크
    /// </summary>
    public partial class CMM_ELEC_AREA_CODE_AUTH : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _COMTYPE = string.Empty;
        private string _COMCODE = string.Empty;
        private string _ATTR1 = string.Empty;
        private string _ATTR2 = string.Empty;
        private string _ATTR3 = string.Empty;
        private string _ATTR4 = string.Empty;
        private string _ATTR5 = string.Empty;
        public string UserName { get; set; }

        Util _Util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_AREA_CODE_AUTH()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length > 0)
            {
                _COMTYPE = Util.NVC(tmps[0]);
                _COMCODE = Util.NVC(tmps[1]);
            }

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

                txtPassWord.Focus();
            }
        }

        private void txtPassWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //if (string.IsNullOrEmpty(txtPassWord.Password))
                //{
                //    Util.MessageValidation("SFU1156"); // 비밀번호를 입력하세요.
                //    return;
                //}

                //string[] sAttr = { txtPassWord.Password };
                //if (!IsValidPassword(sAttr))
                //{
                //    Util.MessageValidation("SFU3188");  //비밀번호를 확인하세요.
                //    return;
                //}

               this.Dispatcher.BeginInvoke(new Action(() => btnConfirm_Click(null, null)));
            }
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
            {
                Util.MessageValidation("SFU4983");  // 사용자를 확인 하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtPassWord.Password))
            {
                Util.MessageValidation("SFU1156"); // 비밀번호를 입력하세요.
                return;
            }

            string[] sAttr = { txtPassWord.Password };
            if (!IsValidPassword(sAttr))
            {
                Util.MessageValidation("SFU3188");  //비밀번호를 확인하세요.
                return;
            }

            UserName = txtUserID.Text;
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private bool IsValidPassword(string[] sAttribute)
        {
            //동별 공통코드로 변경
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
                dr["COM_TYPE_CODE"] = _COMTYPE;
                dr["COM_CODE"] = (_COMCODE == string.Empty ? null : _COMCODE);
                dr["USE_FLAG"] = "Y";
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return true;
                }

                return false;

            }
            catch (Exception ex) { }

            return false;
        }
        #endregion
    }
}
