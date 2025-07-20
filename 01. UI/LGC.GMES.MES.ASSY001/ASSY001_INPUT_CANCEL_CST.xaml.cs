/*************************************************************************************
 Created Date : 2018.02.21
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - LOT ID MERGE 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.02.21  INS 김동일K : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_INPUT_CANCEL_CST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_INPUT_CANCEL_CST : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _Procid = string.Empty;
        private string _PstnID = string.Empty;
        private string _Type = string.Empty;
        private string _InpuqSeq = string.Empty;
        private string _prodLotID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_INPUT_CANCEL_CST()
        {
            InitializeComponent();
        }

        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 13)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);
                
                txtPstnID.Text = Util.NVC(tmps[3]);
                txtLotID.Text = Util.NVC(tmps[4]);
                txtCstID.Text = Util.NVC(tmps[5]);
                double dTmp = 0;
                double.TryParse(Util.NVC(tmps[6]), out dTmp);
                txtInputQty.Text = dTmp.ToString();
                txtProdID.Text = Util.NVC(tmps[7]);
                txtInputDttm.Text = Util.NVC(tmps[8]);
                _PstnID = Util.NVC(tmps[9]);

                _Type = Util.NVC(tmps[10]);
                _InpuqSeq = Util.NVC(tmps[11]);

                _prodLotID = Util.NVC(tmps[12]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _Procid = "";

                txtPstnID.Text = "";
                txtLotID.Text = "";
                txtCstID.Text = "";
                txtInputQty.Text = "";
                txtProdID.Text = "";
                txtInputDttm.Text = "";
                _PstnID = "";
                _Type = "";
                _InpuqSeq = "";
                _prodLotID = "";
            }

            //if (!CheckCSTState())
            //{
            //    txtCstID.Text = "";
            //    txtCstID.Focus();
            //}
            
            ApplyPermissions();

            if (_Type.Equals("HIST"))
            {
                tbPstnName.Visibility = Visibility.Collapsed;
                txtPstnID.Visibility = Visibility.Collapsed;
            }
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave()) return;

                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (_Type.Equals("HIST"))
                            CancelHistProcess();
                        else
                            CancelProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
              
        #endregion

        #region Mehod

        #region [BizCall]
        private void CancelProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("WIPNOTE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                //inInputTable.Columns.Add("ACTQTY", typeof(int));
                inInputTable.Columns.Add("INPUT_SEQNO", typeof(Int64));
                inInputTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _Procid.Equals(Process.PACKAGING) ? "" : _prodLotID; // biz에서 찾음.

                inDataTable.Rows.Add(newRow);

                newRow = inInputTable.NewRow();
                newRow["WIPNOTE"] = "";
                newRow["INPUT_LOTID"] = txtLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = _PstnID;
                //newRow["INPUT_SEQNO"] = null;
                newRow["CSTID"] = txtCstID.Text;

                inInputTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1275");
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CancelHistProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                newRow = inMtrlTable.NewRow();
                newRow["INPUT_SEQNO"] = _InpuqSeq.Equals("") ? 0 : Convert.ToDecimal(_InpuqSeq);
                newRow["LOTID"] = txtLotID.Text;
                newRow["WIPQTY"] = Util.NVC(txtInputQty.Text).Equals("") ? 0 : Convert.ToDecimal(txtInputQty.Text);
                newRow["CSTID"] = txtCstID.Text;

                inMtrlTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CheckCSTState()
        {
            try
            {
                bool bRet = false;
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("CSTID", typeof(string));

                DataRow dtRow = inData.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CSTID"] = txtCstID.Text;
                inData.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_BASIC_INFO_RFID", "INDATA", "OUTDATA", inData);

                if (dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("CSTSTAT"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["CSTSTAT"]).Equals("E"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #region [Validation]
        private bool CanSave()
        {
            bool bRet = false;

            if (txtLotID.Text.Equals(""))
            {
                Util.MessageValidation("SFU1361");
                return bRet;
            }

            //if (txtCstID.Text.Equals(""))
            //{
            //    Util.MessageValidation("SFU1244");
            //    return bRet;
            //}

            bRet = true;

            return bRet;
        }
        #endregion

        #endregion

        private void txtCstID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCstID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCstID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
