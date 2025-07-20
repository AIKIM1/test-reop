using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainFrame.ConfigWindows.Common_sub
{
    /// <summary>
    /// UserCheck.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserCheck : Window
    {
        public UserCheck()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(UserCheck_Loaded);
        }

        void UserCheck_Loaded(object sender, RoutedEventArgs e)
        {
            tbID.Text = LoginInfo.USERID;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DataTable loginIndataTable = new DataTable();
            loginIndataTable.Columns.Add("USERID", typeof(string));
            loginIndataTable.Columns.Add("USERPSWD", typeof(string));
            loginIndataTable.Columns.Add("LANGID", typeof(string));
            DataRow loginIndata = loginIndataTable.NewRow();
            loginIndata["USERID"] = tbID.Text;
            loginIndata["USERPSWD"] = tbPW.Password;
            loginIndata["LANGID"] = LoginInfo.LANGID;
            loginIndataTable.Rows.Add(loginIndata);
            new ClientProxy().ExecuteService("BR_COR_SEL_USERS_PASSWORD_CHK_G", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
                {
                    if (loginException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(loginException.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (loginOutdata.Rows.Count < 1)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("COM0002"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataRow userInfo = loginOutdata.Rows[0];
                    if (!userInfo["VERIFY"].ToString().Equals("Y"))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("COM0002"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DialogResult = true;
                }
            );
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
