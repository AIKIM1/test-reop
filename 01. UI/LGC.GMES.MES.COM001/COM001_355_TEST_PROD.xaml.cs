/*************************************************************************************
 Created Date : 2017.10.07
      Creator : 주건태
   Decription : 활성화 재공 현황 재고 특이사항 일괄 변경
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.17  최초착성.
 
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_355_TEST_PROD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataRow[] _LOT_ID_LISTS = null;

        int cnt = 0;

        public COM001_355_TEST_PROD()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataRow[] LOT_ID_LISTS
        {
            get { return _LOT_ID_LISTS; }
        }

        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LOT_ID_LISTS = (DataRow[])tmps[0];

            setInit();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateSave())
                {
                    return;
                }

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SavePilotProdInfo();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SavePilotProdInfo()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_WIPHISTORYATTR_PILOT_PROD";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PILOT_PROD_CHARGE_USERNAME", typeof(string));
                inTable.Columns.Add("PILOT_PROD_DETL_CNTT", typeof(string));

                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PILOT_PROD_CHARGE_USERNAME"] = txtPilotProdUserList.Text.ToString();
                newRow["PILOT_PROD_DETL_CNTT"] = txtPilotProdDetl.Text.ToString();

                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(decimal));

                newRow = inLot.NewRow();
                newRow["LOTID"] = _LOT_ID_LISTS[0]["LOTID"];
                newRow["WIPSEQ"] = decimal.Parse(_LOT_ID_LISTS[0]["WIPSEQ"].ToString());

                inLot.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", "", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                       //정상 처리 되었습니다
                       Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /*
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveWipRemarks(TRANSACTION_TYPE_DELETE);
                }
            });
        }
        */
        #endregion


        #region Mehod

        private void setInit()
        {
            if (_LOT_ID_LISTS.Length == 1)
            {
                txtPilotProdUserList.Text = _LOT_ID_LISTS[0]["PILOT_PROD_CHARGE_USERNAME"].ToString();
                txtPilotProdDetl.Text = _LOT_ID_LISTS[0]["PILOT_PROD_DETL_CNTT"].ToString();
            }
        }

        private bool ValidateSave()
        {
            /*if (txtPilotProdUserList.Text == null || string.IsNullOrEmpty(txtPilotProdUserList.Text))
            {
                //담당자를 입력하세요.
                Util.MessageValidation("SFU4011");
                return false;
            }*/

            /*
            if (txtTestProdInfo.Text == null || string.IsNullOrEmpty(txtTestProdInfo.Text))
            {
                //특이사항을 입력하세요.
                Util.MessageValidation("SFU1993");
                return false;
            }*/

            if (_LOT_ID_LISTS == null || _LOT_ID_LISTS.Length <= 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }


        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                txtPilotProdUserList.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTestProdWrkSearch_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
            txtPilotProdUserSearch.Text = "";
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtPilotProdUserSearch.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                MatchCollection matches = Regex.Matches(txtPilotProdUserList.Text.ToString(), ",");

                if (matches.Count == 2)
                {
                    Util.Alert("SFU8340"); // 인원은 최대 3명까지 등록가능합니다.
                    return;
                }


                if (string.IsNullOrEmpty(txtPilotProdUserList.Text.ToString()))
                {
                    txtPilotProdUserList.Text = wndPerson.USERNAME + "(" + wndPerson.USERID + ")";
                }
                else
                {
                    txtPilotProdUserList.Text += "," + wndPerson.USERNAME + "(" + wndPerson.USERID + ")";
                }
                //txtTestProdWrkSearch.Text = wndPerson.USERNAME;
                //txtTestProdWrkSearch.Tag = wndPerson.USERID;
            }
        }

        private void txtPilotProdUserSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
                txtPilotProdUserSearch.Text = "";
            }
        }

    }
}