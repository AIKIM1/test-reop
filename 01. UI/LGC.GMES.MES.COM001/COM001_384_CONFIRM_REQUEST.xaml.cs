/*************************************************************************************
 Created Date : 2023.06.16
      Creator : 
   Decription : 포장 PALLET 생산 출고 요청 - CONFIRM_REQUEST
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_384_CONFIRM_REQUEST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string _sCELL_SPLY_REQ_ID = string.Empty;

        public COM001_384_CONFIRM_REQUEST()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _sCELL_SPLY_REQ_ID = Util.NVC(tmps[0]);

            InitCombo();

            this.Loaded -= C1Window_Loaded;
        }


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

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }


 
        public void SaveProcess()
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["CELL_SPLY_REQ_ID"] = _sCELL_SPLY_REQ_ID;
            dr["NOTE"] = txtNote.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            INDATA.Rows.Add(dr);

            try
            {

                ShowLoadingIndicator();
                string _sbizName = "BR_PRD_REG_CELL_PLLT_SHIP_REQ_CFM";
                new ClientProxy().ExecuteService_Multi(_sbizName, "INDATA", null, (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }
                    
                    Util.MessageInfo("SFU1275"); //정상처리되었습니다. 
                    this.DialogResult = MessageBoxResult.OK;

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                this.DialogResult = MessageBoxResult.Cancel;
            }

        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        #endregion


        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtConfirmor.Text = wndPerson.USERNAME;
                txtConfirmor.Tag = wndPerson.USERID;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtConfirmor.Text))
            {
                Util.MessageValidation("SFU8568");  // 확정자를 확인 하세요.
                txtConfirmor.Focus();
                return;
            }

            if (txtConfirmor.Tag == null)
            {
                Util.MessageValidation("SFU8568");  // 확정자를 확인 하세요.
                txtConfirmor.Focus();
                txtConfirmor.Text = string.Empty;
                return;
            }

            if (txtConfirmor.Tag.ToString() == "")
            {
                Util.MessageValidation("SFU8568");  // 확정자를 확인 하세요.
                txtConfirmor.Focus();
                txtConfirmor.Text = string.Empty;
                return;
            }

            Util.MessageConfirm("SFU8569", result =>   // 확정 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });

        }

        private void txtConfirmor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnConfirmor_Click(sender, e);
            }

        }

        private void btnConfirmor_Click(object sender, RoutedEventArgs e)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtConfirmor.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);


                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));


            }

        }
    }
}
