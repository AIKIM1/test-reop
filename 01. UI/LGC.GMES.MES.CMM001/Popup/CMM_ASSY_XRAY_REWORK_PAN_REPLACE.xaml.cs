/*************************************************************************************
 Created Date : 2017.10.12
      Creator : 신광희C
   Decription : X-Ray 재작업 잔량처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
   2017.10.12   신광희C : Initial Created.
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_XRAY_REWORK_PAN_REPLACE : C1Window, IWorkArea
    {
        private string _equipmentCode = string.Empty;
        private string _inputLotId = string.Empty;
        private string _equipmentPositionId = string.Empty;
        private string _processCode = string.Empty;
        private string _inputTypeCode = string.Empty;
        private bool _isRework;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_XRAY_REWORK_PAN_REPLACE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _equipmentCode = Util.NVC(tmps[0]);         // 설비코드
                _inputLotId = Util.NVC(tmps[1]);            // 투입LOT
                _equipmentPositionId = Util.NVC(tmps[2]);   // 위치
                _processCode = Util.NVC(tmps[3]);           // Process
                _inputTypeCode = Util.NVC(tmps[4]);         // 투입제품타입
                _isRework = (bool) tmps[5];
            }

            //if(!_isRework)
            //    this.Header = ObjectDic.Instance.GetObjectName("Washing 잔량처리");

            ApplyPermissions();

            DataTable dt = new DataTable();
            GetRemainInfo(ref dt);
            if (dt != null)
                SetInfo(dt); // SetData

            InitControl(); // Control 초기화
        }

        private void GetRemainInfo(ref DataTable selectDt)
        {
            if (selectDt == null) throw new ArgumentNullException(nameof(selectDt));

            const string bizRuleName = "DA_PRD_SEL_INPUT_LOT_REMAIN_INFO_ASSY_XR";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["EQPTID"] = _equipmentCode;
            inData["EQPT_MOUNT_PSTN_ID"] = _equipmentPositionId;
            inData["INPUT_LOTID"] = _inputLotId;

            inDataTable.Rows.Add(inData);

            selectDt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
        }

        private void SetInfo(DataTable dt)
        {
            if (CommonVerify.HasTableRow(dt)) // 조회했을 때 데이터가있을경우 조회해온 dt에서 값 바인딩
            {
                txtLotId.Text = Util.NVC(dt.Rows[0]["PROD_LOTID"]); // Lot ID
                txtInputLotId.Text = Util.NVC(dt.Rows[0]["INPUT_LOTID"]); // Winding Lot ID
                txtWipQty.Value = dt.Rows[0]["INPUT_QTY"].GetInt(); // 투입수량
            }
            else
            {
                txtLotId.Text = _inputLotId;
            }

        }

        private void InitControl()
        {
            txtChangeQty.IsReadOnly = false;    // ea 직접 입력 가능하도록 기능 변경 요청으로 수정.

            if (!_inputTypeCode.Equals("PROD"))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;
                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);
                grdContents.RowDefinitions[6].Height = new GridLength(0);
                grdContents.RowDefinitions[7].Height = new GridLength(0);
                grdContents.RowDefinitions[8].Height = new GridLength(0);
                grdContents.RowDefinitions[9].Height = new GridLength(0);
                //grdContents.RowDefinitions[10].Height = new GridLength(0);
                //grdContents.RowDefinitions[11].Height = new GridLength(0);
                //grdContents.RowDefinitions[12].Height = new GridLength(0);
                //grdContents.RowDefinitions[13].Height = new GridLength(0);
            }
            else
            {
                grdContents.RowDefinitions[10].Height = new GridLength(0);
                grdContents.RowDefinitions[11].Height = new GridLength(0);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_inputTypeCode.Equals("PROD"))
            {
                if (!ValidationSave())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("잔량처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1862", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            else
            {
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool ValidationSave()
        {
            if (txtChangeQty.Value.Equals(0) || string.IsNullOrEmpty(txtChangeQty.Value.GetString()))
            {
                //Util.Alert("잔량이 없습니다.");
                Util.MessageValidation("SFU1859");
                return false;
            }
            return true;
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT_ASSY_XR";

                DataSet inDataSet = new DataSet();
                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("INPUT_LOT_TYPE", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUT_LOT_TYPE"] = Util.NVC(_inputTypeCode);
                inData.Rows.Add(newRow);

                DataTable inInputLot = inDataSet.Tables.Add("IN_INPUT");
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
                inInputLot.Columns.Add("WIPNOTE", typeof(string));
                inInputLot.Columns.Add("ACTQTY", typeof(Decimal));

                newRow = inInputLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(_equipmentPositionId);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = _inputLotId;
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                newRow["ACTQTY"] = txtChangeQty.Value;
                inInputLot.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, inDataSet);

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
                HiddenLoadingIndicator();
            }
        }

        private void TxtUseQty_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            string useQty = txtUseQty.Value.GetString();
            foreach (char c in useQty)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }

            if (txtUseQty.Value.GetDecimal() > txtWipQty.Value.GetDecimal())
            {
                Util.MessageInfo("SFU4052", (result) =>
                {
                    txtUseQty.Value = 0;
                    txtChangeQty.Value = 0;
                    txtUseQty.Focus();
                });
                return;
            }
            else
            {
                txtChangeQty.Value = txtWipQty.Value - txtUseQty.Value;
            }
        }

        private void TxtChangeQty_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            string changeQty = txtChangeQty.Value.GetString();
            foreach (char c in changeQty)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }

            if (txtChangeQty.Value.GetDecimal() > txtWipQty.Value.GetDecimal())
            {
                Util.MessageInfo("SFU4053", (result) =>
                {
                    txtUseQty.Value = 0;
                    txtChangeQty.Value = 0;
                    txtChangeQty.Focus();
                });
                return;
            }
            else
            {
                txtUseQty.Value = txtWipQty.Value - txtChangeQty.Value;
            }
        }
    }
}
