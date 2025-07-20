/*************************************************************************************
 Created Date : 2015.09.01
      Creator : 홍길동
   Decription : 동에서 번쩍 서에서 번쩍할 때 필요한 알고리즘을 구현한 화면입니다.
--------------------------------------------------------------------------------------
 [Change History]
  2015.09.02 / 임꺽정 : 왔다갔다 할 필요없이 힘으로 찍어 누르는 방식으로 변경함.
  2015.09.03 / 일지매 : 가까이 가지 않고 멀리서 암기를 던지는 방식으로 변경함.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
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
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MainFrame.Controls
{
    public partial class ChangePWD : Window
    {
        /// <summary>
        /// ChangePWD 생성자
        /// </summary>
        public ChangePWD()
        {
            InitializeComponent();

            tbID.Focus();
        }

        public ChangePWD(string id)
        {
            InitializeComponent();

            tbID.Text = id;
            tbPW.Focus();
        }

        /// <summary>
        /// OK 버튼을 클릭하면 DialogResult에 OK를 기록하고 팝업을 닫는다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine(btnOK, Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            // validation
            if (!tbNewPW.Password.Equals(tbConfPW.Password))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Confirm password is different.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

            if (tbNewPW.Password.Length < 10)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("More than 10 digit with alpha-numeric characters.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

            if (tbID.Text.Equals(tbNewPW.Password) || tbPW.Password.Equals(tbNewPW.Password))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Should be different from current password and ID", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

            char[] pwchar = tbNewPW.Password.ToCharArray();
            for (int inx = 0; inx < pwchar.Length-3; inx++)
            {
                if (pwchar[inx].Equals(pwchar[inx + 1]))
                {
                    if (pwchar[inx].Equals(pwchar[inx + 2]))
                    {
                        if (pwchar[inx].Equals(pwchar[inx + 3]))
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Continuous repetition of more than 4 digit are not allowed", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            inx += 2;
                        }
                    }
                    else
                    {
                        inx++;
                    }
                }
            }

            if (tbNewPW.Password.Contains(tbID.Text))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Not allowed that 'ID' is included", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;

            DataTable chgPwdTable = new DataTable();
            chgPwdTable.Columns.Add("USERID", typeof(string));
            chgPwdTable.Columns.Add("USERPSWD", typeof(string));
            chgPwdTable.Columns.Add("NEWPWD", typeof(string));
            DataRow chgPwdRow = chgPwdTable.NewRow();
            chgPwdRow["USERID"] = tbID.Text;
            chgPwdRow["USERPSWD"] = tbPW.Password;
            chgPwdRow["NEWPWD"] = tbNewPW.Password;
            chgPwdTable.Rows.Add(chgPwdRow);

            new ClientProxy().ExecuteService("BR_COR_UPD_USER_PASSWORD", "INDATA", "OUTDATA", chgPwdTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {

                        if (result.Rows.Count > 0 && "Y".Equals(result.Rows[0]["CONFIRM"]))
                        {
                            this.DialogResult = true;
                        }
                        else
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show("Incorrect password", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex2)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            );

            Logger.Instance.WriteLine(btnOK, Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }

        /// <summary>
        /// Cacenl 버튼을 클릭하면 DialogResult에 Cancel을 기록하고 팝업을 닫는다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine(btnCancel, Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

            this.DialogResult = false;

            Logger.Instance.WriteLine(btnCancel, Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
        }
    }
}
