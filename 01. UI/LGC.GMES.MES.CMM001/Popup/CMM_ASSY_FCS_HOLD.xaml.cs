/*************************************************************************************
 Created Date : 2018.08.17
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 활성화 출하 HOLD
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.17  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_FCS_HOLD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_FCS_HOLD : C1Window, IWorkArea
    {

        private string _ProcID = string.Empty;
        private string _LotID = string.Empty;
        private string _HOLD_FLAG = string.Empty;
        private string _HOLD_ID = string.Empty;

        public CMM_ASSY_FCS_HOLD()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 2)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _LotID = Util.NVC(tmps[1]);
                    txtLotID.Text = _LotID;               
                }
                else
                {
                    _ProcID = "";
                    _LotID = "";
                }

                ApplyPermissions();

                GetFCSHoldInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetUserWindow();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                string sMsg = string.Empty;

                //if (DDDD.ToUpper().Equals("Y"))
                //    sMsg = "SFU4046";
                //else
                    sMsg = "SFU1345";

                Util.MessageConfirm(sMsg, (result) =>// %1 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //if (_HOLD_FLAG.ToUpper().Equals("Y"))
                        //    UnHoldProcess();
                        //else
                            HoldProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (string.IsNullOrWhiteSpace(_LotID))
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return bRet;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(txtUser.Text)))
            {
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                return bRet;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(txtUser.Tag)))
            {
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                return bRet;
            }

            if (string.IsNullOrWhiteSpace(txtRemark.Text))
            {
                Util.MessageValidation("SFU1590"); // 비고 정보를 입력하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void HoldProcess()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("HOLD_FLAG", typeof(string));
                inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable in_DATA = indataSet.Tables.Add("INHOLD");
                in_DATA.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["HOLD_FLAG"] = "Y";
                newRow["HOLD_NOTE"] = Util.NVC(txtRemark.Text);
                newRow["USERID"] = txtUser.Tag;

                inDataTable.Rows.Add(newRow);

                newRow = null;

                newRow = in_DATA.NewRow();
                newRow["LOTID"] = _LotID;

                in_DATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPCL_MGT_HOLD", "INDATA,INHOLD", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275", (msgResult) =>
                        {
                            //if (msgResult == MessageBoxResult.OK)
                            //{
                                this.DialogResult = MessageBoxResult.OK;
                            //}                            
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void UnHoldProcess()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("UNHOLD_NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable in_DATA = indataSet.Tables.Add("INHOLD");
                in_DATA.Columns.Add("HOLD_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["UNHOLD_NOTE"] = Util.NVC(txtRemark.Text);
                newRow["USERID"] = txtUser.Tag;

                inDataTable.Rows.Add(newRow);

                newRow = null;

                newRow = in_DATA.NewRow();
                newRow["HOLD_ID"] = _HOLD_ID;

                in_DATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPCL_MGT_UNHOLD", "INDATA,INHOLD", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275", (msgResult) =>
                        {
                            //if (msgResult == MessageBoxResult.OK)
                            //{
                            this.DialogResult = MessageBoxResult.OK;
                            //}                            
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetFCSHoldInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("ASSY_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ASSY_LOTID"] = _LotID;

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_CL", "INDATA", "OUTDATA", dtRqst, (dtRslt, bizError) =>
                {
                    try
                    {
                        if (bizError != null)
                        {
                            Util.MessageException(bizError);
                            return;
                        }

                        if (dtRslt?.Rows?.Count > 0 && dtRslt.Columns.Contains("HOLD_FLAG") && dtRslt.Columns.Contains("HOLD_ID"))
                        {
                            _HOLD_FLAG = Util.NVC(dtRslt.Rows[0]["HOLD_FLAG"]);
                            _HOLD_ID = Util.NVC(dtRslt.Rows[0]["HOLD_ID"]);
                            //cboHold.SelectedValue = _HOLD_FLAG;
                            //txtHoldYN.Text = _HOLD_FLAG;

                            if (_HOLD_FLAG.ToUpper().Equals("Y"))
                            {
                                //btnSave.Content = ObjectDic.Instance.GetObjectName("출하HOLD취소");
                                btnSave.IsEnabled = false;
                                btnSaveCancel.IsEnabled = true;
                            }
                            else
                            {
                                //btnSave.Content = ObjectDic.Instance.GetObjectName("출하HOLD");
                                btnSave.IsEnabled = true;
                                btnSaveCancel.IsEnabled = false;
                            }
                        }
                        else
                        {
                            _HOLD_FLAG = "N";
                            _HOLD_ID = "";
                            //cboHold.SelectedValue = _HOLD_FLAG;
                            //txtHoldYN.Text = _HOLD_FLAG;

                            //btnSave.Content = ObjectDic.Instance.GetObjectName("출하HOLD");
                            btnSave.IsEnabled = true;
                            btnSaveCancel.IsEnabled = false;
                        }

                        SettxtHoldYN();

                        //this.Header = btnSave.Content;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SettxtHoldYN()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "HOLD_YN";
                dr["CMCODE"] = _HOLD_FLAG;

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", dtRqst, (dtRslt, bizError) =>
                {
                    try
                    {
                        if (bizError != null)
                        {
                            Util.MessageException(bizError);
                            txtHoldYN.Text = _HOLD_FLAG;
                            return;
                        }

                        if (dtRslt?.Rows?.Count > 0 && dtRslt.Columns.Contains("CMCODE") && dtRslt.Columns.Contains("CMCDNAME"))
                        {
                            txtHoldYN.Text = Util.NVC(dtRslt.Rows[0]["CMCODE"]) + " : " + Util.NVC(dtRslt.Rows[0]["CMCDNAME"]);
                        }
                        else
                        {
                            txtHoldYN.Text = _HOLD_FLAG;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        txtHoldYN.Text = _HOLD_FLAG;
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
                txtHoldYN.Text = _HOLD_FLAG;
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    GetUserWindow();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetUserWindow()
        {
            Popup.CMM_PERSON wndPerson = new Popup.CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));                
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            Popup.CMM_PERSON wndPerson = sender as Popup.CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }
        }

        private void btnSaveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                string sMsg = string.Empty;

                //if (_HOLD_FLAG.ToUpper().Equals("Y"))
                    sMsg = "SFU4046";
                //else
                //    sMsg = "SFU1345";

                Util.MessageConfirm(sMsg, (result) =>// %1 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //if (_HOLD_FLAG.ToUpper().Equals("Y"))
                            UnHoldProcess();
                        //else
                        //    HoldProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
