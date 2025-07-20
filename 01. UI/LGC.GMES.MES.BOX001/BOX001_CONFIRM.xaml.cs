/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public BOX001_CONFIRM()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            txtLoginID.Text = LoginInfo.USERID;
            tbPW.Focus();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.Loaded -= Window_Loaded;

        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            passWord_Confirm();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void passWord_Confirm()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("USERID", typeof(String));
                dtIndata.Columns.Add("USERPSWD", typeof(String));

                DataRow dr = dtIndata.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["USERPSWD"] = tbPW.Password;

                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_COR_SEL_USERS_PASSWORD_CHK", "INDATA", "OUTDATA", dtIndata);

                if (dtResult.Rows[0]["VERIFY"].ToString() == "Y")
                {
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
                else
                {
                    //사용자 암호가 인증되지 않았습니다.
                    Util.MessageInfo("SFU3269");
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3269"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    tbPW.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                tbPW.Focus();
                return;
            }
        }

        #endregion

        #region Mehod

        #endregion

        private void tbPW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                passWord_Confirm();
            }
        }
    }
}
