/*************************************************************************************
 Created Date : 2019.05.22
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 투입자재 Tab - 투입LOT배출
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.22  INS 김동일K : Initial Created.
  2019.09.02  INS 김동일K : 투입배출 시 투입량 계산 변경(설비 계산에서 MES 계산)으로 인한 수정
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
    /// ASSY004_COM_INPUT_LOT_END.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_COM_INPUT_LOT_END : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private double _WipQty = 0;
        private string _Position = string.Empty;
        private string _ProcID = string.Empty;
        private string _Input_type_code = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _CSTID = string.Empty;
        private string _AUTO_STOP_FLAG = string.Empty;
        private string _Prod_LotID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY004_COM_INPUT_LOT_END()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 12)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _WipQty = Util.NVC(tmps[4]).Equals("") ? 0 : double.Parse(Util.NVC(tmps[4]));
                _Position = Util.NVC(tmps[5]);
                _ProcID = Util.NVC(tmps[6]);
                _Input_type_code = Util.NVC(tmps[7]);

                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[8]);
                _CSTID = Util.NVC(tmps[9]);
                _AUTO_STOP_FLAG = Util.NVC(tmps[10]);
                _Prod_LotID = Util.NVC(tmps[11]);
            }

            ApplyPermissions();
            SetInfo();

            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow dr = dt.NewRow();
            dr["CODE"] = "N";
            dr["NAME"] = "N";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CODE"] = "H";
            dr["NAME"] = "Y"; // Hold 배출
            dt.Rows.Add(dr);

            cboHold.DisplayMemberPath = "NAME";
            cboHold.SelectedValuePath = "CODE";
            cboHold.ItemsSource = dt.Copy().AsDataView();

            cboHold.SelectedIndex = 1;
                        
            if (!_Input_type_code.Equals("PROD"))
            {
                txtChangeQty.IsEnabled = false;

                rdoCpl.IsEnabled = false;
                rdoRmn.IsEnabled = false;
                txtLotId.IsEnabled = false;
                txtTotalQty.IsEnabled = false;
                txtCSTId.IsEnabled = false;
                cboHold.IsEnabled = false;
            }

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
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Input_type_code.Equals("PROD"))
            {
                if (!CanSave())
                    return;

                // 처리 하시겠습니까?
                Util.MessageConfirm("SFU1925", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            else
            {
                // 처리 하시겠습니까?
                Util.MessageConfirm("SFU1925", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtChangeQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtChangeQty.Text, 0))
                {
                    txtChangeQty.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTId_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCSTId == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCSTId, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoCpl_Checked(object sender, RoutedEventArgs e)
        {
            if (cboHold == null) return;

            cboHold.IsEnabled = false;
            txtChangeQty.IsEnabled = false;
            txtCSTId.IsEnabled = false;

            txtChangeQty.Text = "";
            //txtCSTId.Text = "";
        }

        private void rdoRmn_Checked(object sender, RoutedEventArgs e)
        {
            if (cboHold == null) return;

            cboHold.IsEnabled = true;
            txtChangeQty.IsEnabled = true;
            txtCSTId.IsEnabled = true;

            //txtCSTId.Text = _CSTID;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));                

                DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
                inInputLot.Columns.Add("CSTID", typeof(string));
                inInputLot.Columns.Add("UNMOUNT_TYPE", typeof(string));
                inInputLot.Columns.Add("REMAIN_TYPE", typeof(string));
                inInputLot.Columns.Add("REMAIN_QTY", typeof(int));
                inInputLot.Columns.Add("LOSS_PRE", typeof(int));
                inInputLot.Columns.Add("LOSS_CON", typeof(int));
                inInputLot.Columns.Add("LOSS_SELF", typeof(int));
                inInputLot.Columns.Add("AUTO_SPLICING_MODE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOT_CNT", typeof(int));
                
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _Prod_LotID;

                inDataTable.Rows.Add(newRow);
                newRow = null;
                
                newRow = inInputLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _Position;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = txtLotId.Text;
                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    newRow["CSTID"] = txtCSTId.Text.Trim();

                newRow["UNMOUNT_TYPE"] = (bool)rdoCpl.IsChecked ? "C" : "R";
                if ((bool)rdoRmn.IsChecked)
                    newRow["REMAIN_TYPE"] = cboHold.SelectedValue.ToString();

                int iRmn = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : Convert.ToInt32(txtChangeQty.Text);
                newRow["REMAIN_QTY"] = iRmn;
                newRow["LOSS_PRE"] = 0;
                newRow["LOSS_CON"] = 0;
                newRow["LOSS_SELF"] = 0;
                newRow["AUTO_SPLICING_MODE"] = "N";
                newRow["INPUT_LOT_CNT"] = (_WipQty - iRmn) < 0 ? 0 : (_WipQty - iRmn);

                inInputLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNMOUNT_INPUT_IN_LOT_L", "IN_EQP,IN_INPUT", "OUTPUT", indataSet);
                
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            bool bRet = false;

            if ((bool)rdoRmn.IsChecked)
            {
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

                // 자동 Change 모드인 경우 투입취소 불가.
                if (_ProcID.Equals(Process.LAMINATION) &&
                    _AUTO_STOP_FLAG.Equals("Y"))
                {
                    // 잔량처리 불가 : 설비 자동 Change 모드로 투입 완료처리된 LOT은 처리 불가.
                    Util.MessageValidation("SFU6038");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]

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
            txtLotId.Text = _LotID;
            txtTotalQty.Text = _WipQty.ToString();

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                txtCSTId.Text = _CSTID;

                //if (!CheckCSTState())
                //{
                //    txtCSTId.Text = "";
                //    txtCSTId.Focus();
                //}
            }
        }

        #endregion

        #endregion

        
    }
}
