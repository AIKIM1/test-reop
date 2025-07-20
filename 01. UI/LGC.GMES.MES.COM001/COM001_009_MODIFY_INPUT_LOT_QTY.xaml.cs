/*************************************************************************************
 Created Date : 2017.08.28
      Creator : 신광희
   Decription : 투입 완료 LOT 투입 수량 수정
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.28  신광희 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_009_MODIFY_INPUT_LOT_QTY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _equipmentCode = string.Empty;   
        private string _prodLotId = string.Empty;
        private string _inputLotId = string.Empty;
        private string _inputLotSeqNo = string.Empty;
        private string _inputQty = string.Empty;
        private string _processCode = string.Empty;

        public COM001_009_MODIFY_INPUT_LOT_QTY()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                _equipmentCode = Util.NVC(tmps[0]);
                _prodLotId = Util.NVC(tmps[1]);
                _inputLotId = Util.NVC(tmps[2]);
                _inputLotSeqNo = Util.NVC(tmps[3]);
                _inputQty = Util.NVC(tmps[4]);
                _processCode = Util.NVC(tmps[5]);
                txtInputLotId.Text = _inputLotId;
            }

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (_processCode.Equals(Process.ASSEMBLY))
                            SaveInputLotQty();
                        else
                            SaveInputLotQtyWS();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        #endregion

        #region Mehod


        private void SaveInputLotQty()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName  = "BR_PRD_REG_MODIFY_INPUT_LOT_QTY_AS";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_SEQNO", typeof(string));
                inDataTable.Columns.Add("INPUT_QTY", typeof(Decimal));
                inDataTable.Columns.Add("MODIFY_QTY", typeof(Decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["PROD_LOTID"] = _prodLotId;
                dr["INPUT_LOTID"] = txtInputLotId.Text;
                dr["INPUT_SEQNO"] = _inputLotSeqNo;
                dr["INPUT_QTY"] = _inputQty.GetDecimal();
                dr["MODIFY_QTY"] = txtInputQty.Value.GetDecimal();
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Hidden;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                }, ds);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Hidden;
                Util.MessageException(ex);
            }
        }

        private void SaveInputLotQtyWS()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_PRD_UPD_TB_SFC_WIP_INPUT_MTRL_HIST_INPUT_QTY_AS";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("INPUT_SEQNO", typeof(string));
                inDataTable.Columns.Add("INPUT_QTY", typeof(Decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("DATE", typeof(DateTime));

                DataRow dr = inDataTable.NewRow();
                dr["INPUT_SEQNO"] = _inputLotSeqNo;
                dr["INPUT_QTY"] = txtInputQty.Value.GetDecimal();
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTID"] = _prodLotId;
                dr["INPUT_LOTID"] = txtInputLotId.Text;

                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (searchResult, searchException) =>
                {
                    if (searchException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                }
                );
                loadingIndicator.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Hidden;
                Util.MessageException(ex);
            }
        }

        private bool Validation()
        {
            //수량은 0보다 커야 합니다.
            if (txtInputQty.Value.Equals(0))
            {
                Util.MessageValidation("SFU1683"); 
                return false;
            }

            if (string.IsNullOrEmpty(txtInputLotId.Text))
            {
                //LOT ID 가 없습니다.
                Util.MessageValidation("SFU1361");
                return false;
            }
          
            return true;
        }

        #endregion

       
    }
}
