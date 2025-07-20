/*************************************************************************************
 Created Date : 2019.05.28
      Creator : LG CNS 김대근
   Decription : ASSY004_COM_INPUT_LOT_END를 카피 후 재작업
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.28  LG CNS 김대근 : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_050_INPUT_LOT_END.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_INPUT_LOT_END : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _MountID = string.Empty;
        private string _InputLotID = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private int _WipQty = 0;
        private RadioButton rbAllOrRemain = null;
        private BizDataSet _Biz = new BizDataSet();

        public ASSY004_050_INPUT_LOT_END()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _MountID = Util.NVC(tmps[2]);
                _InputLotID = Util.NVC(tmps[3]);
                _ProdLotID = Util.NVC(tmps[4]);
                _WipQty = Util.NVC_Int(tmps[5]);
            }

            ApplyPermissions();
            SetInfo();
            _LDR_LOT_IDENT_BAS_CODE = "";

            if (!_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                tbCstID.Visibility = Visibility.Collapsed;
                txtCSTId.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbCstID.Visibility = Visibility.Visible;
                txtCSTId.Visibility = Visibility.Visible;
            }
        }

        private void rdoCpl_Checked(object sender, RoutedEventArgs e)
        {
            rbAllOrRemain = sender as RadioButton;
            if (txtChangeQty != null)
                txtChangeQty.IsReadOnly = true;
        }

        private void rdoRmn_Checked(object sender, RoutedEventArgs e)
        {
            rbAllOrRemain = sender as RadioButton;
            if (txtChangeQty != null)
                txtChangeQty.IsReadOnly = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rbAllOrRemain.Equals(rdoCpl))
            {
                SaveAll();
            }
            else if (rbAllOrRemain.Equals(rdoRmn))
            {
                SaveRemain();
            }
            else
            {
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [Mehod]
        private void SaveAll()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _MountID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = _InputLotID;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_IN_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveRemain()
        {
            try
            {
                if (!CanSave())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("INPUT_LOT_TYPE", typeof(string));


                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("WIPNOTE", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));
                inInput.Columns.Add("CSTID", typeof(string));


                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(newRow);
                newRow = null;

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _MountID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = _InputLotID;
                newRow["ACTQTY"] = Convert.ToInt32(txtChangeQty.Text);
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAIN_INPUT_IN_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Util Method]
        private bool CanSave()
        {
            bool bRet = false;

            if (txtChangeQty.Text.Trim().Equals("") || txtChangeQty.Text.Trim().Equals("0"))
            {
                //Util.Alert("잔량이 없습니다.");
                Util.MessageValidation("SFU1859");
                return bRet;
            }

            double dTot, dChg;
            double.TryParse(txtTotalQty.Text, out dTot);
            double.TryParse(txtChangeQty.Text, out dChg);

            if (dTot < dChg)
            {
                //Util.Alert("잔량이 총 수량보다 많습니다.");
                Util.MessageValidation("SFU1861");
                return bRet;
            }
            
            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (txtCSTId.Text.Trim().Length < 1)
                {
                    Util.MessageValidation("SFU6051");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        private void SetInfo()
        {
            txtLotId.Text = _InputLotID;
            txtTotalQty.Text = _WipQty.ToString();
        }
        #endregion
    }
}
