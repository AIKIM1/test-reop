/*************************************************************************************
 Created Date : 2024.02.19
      Creator : 이정미
   Decription : PKG LOT HOLD 상세 현황
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.19  NAME : Initial Created
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_132_HOLD_DETAIL_SAVE : C1Window, IWorkArea
    {
        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        #region Declaration & Constructor
        private string EQSGID = string.Empty;

        #endregion
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_132_HOLD_DETAIL_SAVE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserID.Text = LoginInfo.USERID;
            txtUserName.Text = LoginInfo.USERNAME;

            Object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null) return;

            else
            {
                txtProdID.Text = tmps[0] as string;
                HOLD일자2.Text = tmps[1] as string;
                EQSGID = tmps[2] as string;
                HOLD일자.Text = tmps[3] as string;
            }

            InitCombo();
        }
        #endregion

        #region Event

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            string[] sFilter = { "FORM_HOLD_DEPART" };
            ComCombo.SetCombo(cboDepartment, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA_COMMON_CODE", sFilter : sFilter);
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetUserWindow();
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sUserID = txtUserID.Text;
                        string sUserName = txtUserName.Text;
                        if (string.IsNullOrEmpty(sUserID) || string.IsNullOrEmpty(sUserName))
                        {
                            //요청자를 입력해주세요.
                            Util.Alert("FM_ME_0355");
                            return;
                        }

                        string sDepartment = Util.GetCondition(cboDepartment, bAllNull: true);
                        if (string.IsNullOrEmpty(sDepartment) || sDepartment.Contains("SELECT"))
                        {
                            //부서를 선택해주세요
                            Util.Alert("FM_ME_0606"); //새로 만들기
                            return;
                        }

                        try
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "RQSTDT";
                            dtRqst.Columns.Add("WRK_DATE", typeof(string));
                            dtRqst.Columns.Add("EQSGID", typeof(string));
                            dtRqst.Columns.Add("PRODID", typeof(string));
                            dtRqst.Columns.Add("HOLD_DEPT", typeof(string));
                            dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                            dtRqst.Columns.Add("INSUSER", typeof(string));
                            dtRqst.Columns.Add("UPDUSER", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["WRK_DATE"] = HOLD일자.Text;
                            dr["EQSGID"] = EQSGID;
                            dr["PRODID"] = txtProdID.GetBindValue();
                            dr["HOLD_DEPT"] = Util.GetCondition(cboDepartment, bAllNull: true);
                            dr["REMARKS_CNTT"] = txtRemark.GetBindValue();
                            dr["INSUSER"] = sUserID;
                            dr["UPDUSER"] = sUserID;
                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_LOT_HOLD_DEPT_MDNG_UI", "RQSTDT", "RSLTDT", dtRqst);

                            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                            {
                                Util.MessageValidation("FM_ME_0215"); //저장하였습니다.
                                C1Window_Loaded(null, null);
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0213");  //저장실패하였습니다.
                            }
                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                    }
                });
            }

            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            } 
        }

        #endregion

        #region Method

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
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

        #endregion

    }
}
