/*************************************************************************************
 Created Date : 2018.01.13
      Creator : Lee. D. R
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.13  Lee. D. R  : Initial Created.

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_069 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        private string sEmpty_Lot = string.Empty;
        private string sType = string.Empty;

        DataTable dtList = new DataTable();
        DataTable dtType = new DataTable();

        public COM001_069()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SetEvent();
        }

        private void ApplyPermissions()
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            //string[] sFilter = { "MKT_TYPE_CODE" };
            //combo.SetCombo(cboType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);

            txtLotid.Focus();
        }

        private void SetEvent()
        {
            dtType = CommonCodeS("MKT_TYPE_CODE");

            if (dtType != null && dtType.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtType.Rows[i]["CBO_CODE"].ToString(), dtType.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }
        }

        private DataTable CommonCodeS(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }
        #endregion

        #region [Button Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //Save();
                }
            });
        }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgList.IsReadOnly = false;
                    dgList.RemoveRow(index);
                    dgList.IsReadOnly = true;

                    txtLotid.Focus();
                }
            });
        }
        #endregion

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
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
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }

        private void Clear()
        {
            txtNote.Text = string.Empty;
            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;
            txtCode.Text = string.Empty;
            Util.gridClear(dgList);
            sEmpty_Lot = "";
            sType = "";
            dtList = new DataTable();
            txtLotid.Focus();
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        public void GetLotList()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLotid.Text))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageInfo("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotid.Text;
                
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_MKT_TYPE_CODE", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3588", txtLotid.Text);  // 입력한 LOTID[% 1] 정보가 없습니다.
                    return;
                }

                if (dgList.GetRowCount() == 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation);

                    sType = dtResult.Rows[0]["MKT_TYPE_CODE"].ToString();
                }
                else
                {
                    if (sType.Equals(dtResult.Rows[0]["MKT_TYPE_CODE"].ToString()))
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgList.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgList, dtInfo, FrameOperation);
                    }
                    else
                    {
                        Util.MessageValidation("SFU4271");   //동일한 시장유형이 아닙니다.
                        return;
                    }
                }
                
                dtList = DataTableConverter.Convert(dgList.GetCurrentItems());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Process(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (sEmpty_Lot != "")
                    {
                        Util.MessageValidation("SFU3588", sEmpty_Lot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        sEmpty_Lot = "";
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        public bool Multi_Process(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_MKT_TYPE_CODE", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_Lot == "")
                        sEmpty_Lot += sLotid;
                    else
                        sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                }

                if (dgList.GetRowCount() == 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation);

                    if (dtResult.Rows.Count != 0)
                        sType = dtResult.Rows[0]["MKT_TYPE_CODE"].ToString();

                    if (sType == "D")
                        txtCode.Text = dtType.Rows[0]["CBO_NAME"].ToString();
                    else
                        txtCode.Text = dtType.Rows[1]["CBO_NAME"].ToString();

                    //if (sType == "D")
                    //    cboType.SelectedItem = "E";
                    //else
                    //    cboType.SelectedItem = "D";
                }
                else
                {
                    if (sType.Equals(dtResult.Rows[0]["MKT_TYPE_CODE"].ToString()))
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgList.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgList, dtInfo, FrameOperation);
                    }
                    else
                    {
                        Util.MessageValidation("SFU4271");   //동일한 시장유형이 아닙니다.
                        return false;
                    }
                }

                dtList = DataTableConverter.Convert(dgList.GetCurrentItems());

                return true;
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
