/*************************************************************************************
 Created Date : 2023.11.15
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.15  DEVELOPER : Initial Created.

 ***************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_382_LINK_PALLET : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string _boxid = string.Empty;
        string _palletbcd = string.Empty;
        string _rackid = string.Empty;

        bool _bPalletbcd = false;  // Pallet BCD Key Down Check
        bool _bPalletid = false;   // Pallet ID key Down Check

        public string BOXID
        {
            get { return _boxid; }
        }

        public string PALLETBCD
        {
            get { return _palletbcd; }
        }

        public COM001_382_LINK_PALLET()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _boxid = Util.NVC(tmps[0]);
                _palletbcd = Util.NVC(tmps[1]);
            }

            txtPalletBCD.Text = _palletbcd;
            txtPalletBCD.Focus();
            if (!string.Equals(txtPalletBCD.Text.Trim(), ""))
                getPalletInfo();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _Init();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (btnUnlink.IsEnabled == true && !string.Equals(txtLinkedPalletID.Text, ""))
                return;

            if (!_bPalletbcd || !_bPalletid)
                return;

            if (string.Equals(txtPalletBCD.Text.Trim(), ""))
            {
                // Pallet BCD를 입력하세요
                Util.MessageValidation("SFU8923", result =>
                {
                    txtPalletBCD.Focus();
                });

                return;
            }

            if (string.Equals(txtPalletID.Text.Trim(), ""))
            {
                // PALLETID를 입력해주세요
                Util.MessageValidation("SFU1411", result =>
                {
                    txtPalletID.Focus();
                });

                return;
            }

            // 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    savePallet();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnUnlink_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(txtLinkedPalletID.Text, ""))
                return;

            string sMSG = string.Equals(_rackid, "") ? "SFU8561" : "SFU9021";

            // SFU8561 : Empty 처리 하시겠습니까?, SFU9021 : Pallet [%1]가 Location [%2]에 입고되어 있습니다. Empty 처리 하시겠습니까?
            object[] _param = new object[] { string.Equals(_rackid, "") ? null : txtPalletBCD.Text, string.Equals(_rackid, "") ? null : _rackid };

            Util.MessageConfirm(sMSG, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    setUnlink();
                }
            }, _param);
        }

        private void txtPalletBCD_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _bPalletbcd = true;
                getPalletInfo();
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _bPalletid = true;
                string _valueToboxid = getBoxInfo();
                if (!string.Equals(_valueToboxid, ""))
                {
                    txtPalletID.IsReadOnly = true;
                }
                else
                {
                    txtPalletID.IsReadOnly = false;
                }
                txtPalletID.Text = _valueToboxid;
            }
        }

        private void _Init()
        {
            txtPalletBCD.Text = string.Empty;
            txtLinkedPalletID.Text = string.Empty;
            txtPalletID.Text = string.Empty;
            txtPalletBCD.IsReadOnly = false;
            btnUnlink.IsEnabled = true;
            txtPalletID.IsReadOnly = false;
            txtPalletBCD.Focus();
            _bPalletbcd = false;
            _bPalletid = false;
        }

        private void savePallet()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                INDATA_BOXID.TableName = "INDATA_BOXID";
                INDATA_BOXID.Columns.Add("CSTID", typeof(string));
                INDATA_BOXID.Columns.Add("BOXID", typeof(string));

                DataRow drbox = INDATA_BOXID.NewRow();
                drbox["CSTID"] = txtPalletBCD.Text;
                drbox["BOXID"] = txtPalletID.Text;
                INDATA_BOXID.Rows.Add(drbox);

                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PLLT_BCD_BOXID_MAPPING", "INDATA,INDATA_BOXID", null, inDataSet);

                // 정상 처리 되었습니다.
                Util.MessageInfo("SFU1275", msgresult =>
                {
                    _Init();
                });

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

        private void setUnlink()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("CSTID", typeof(string));
                dtIndata.Columns.Add("BOXID", typeof(string));
                dtIndata.Columns.Add("RACK_ID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow newRow = dtIndata.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["CSTID"] = txtPalletBCD.Text;
                newRow["BOXID"] = txtLinkedPalletID.Text;
                newRow["RACK_ID"] = _rackid;
                newRow["USERID"] = LoginInfo.USERID;


                dtIndata.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_PLLT_BCD_BOXID_EMPTY", "INDATA", "OUTDATA", dtIndata);

                // 정상 처리 되었습니다.
                Util.MessageInfo("SFU1275", msgresult =>
                {
                    _bPalletbcd = true; // Pallet BCD Key Down 체크
                    txtLinkedPalletID.Text = string.Empty;
                    txtPalletID.IsReadOnly = false;
                    btnUnlink.IsEnabled = false;
                    txtPalletID.Focus();
                });
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

        private void getPalletInfo()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = txtPalletBCD.Text;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PLLT_BCD_INF", "INDATA", "OUTDATA", INDATA);

                if (dtRslt?.Rows?.Count > 0)
                {
                    txtPalletBCD.IsReadOnly = true;
                    if (!string.Equals(Util.NVC(dtRslt.Rows[0]["BOXID"]), ""))
                    {
                        btnUnlink.IsEnabled = true;
                        txtPalletID.IsReadOnly = true;
                        txtPalletID.Text = string.Empty;
                        txtLinkedPalletID.Text = Util.NVC(dtRslt.Rows[0]["BOXID"]);
                        _rackid = Util.NVC(dtRslt.Rows[0]["RACK_ID"]);
                    }
                    else
                    {
                        btnUnlink.IsEnabled = false;
                        txtLinkedPalletID.Text = string.Empty;
                        _rackid = string.Empty;
                        txtPalletID.IsReadOnly = false;
                        txtPalletID.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    txtPalletBCD.Clear();
                    txtPalletBCD.Focus();
                    _bPalletbcd = false;
                });
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string getBoxInfo()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = txtPalletID.Text;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_BOXID_INF", "INDATA", "OUTDATA", INDATA);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["BOXID"]);
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    txtPalletID.Clear();
                    txtPalletID.Focus();
                    _bPalletid = false;
                });
                return "";
            }
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


    }
}
