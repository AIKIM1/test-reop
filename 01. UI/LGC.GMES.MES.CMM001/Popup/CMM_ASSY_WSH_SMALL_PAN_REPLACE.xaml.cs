/*************************************************************************************
 Created Date : 2017.07.27
      Creator : 신광희C
   Decription : Washing 재작업 잔량처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.27   신광희C : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_WINDING_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_WSH_SMALL_PAN_REPLACE : C1Window, IWorkArea
    {
        private string _equipmentCode = string.Empty;
        private string _inputLotId = string.Empty;
        private string _equipmentPositionId = string.Empty;
        private string _processCode = string.Empty;
        private string _inputTypeCode = string.Empty;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_WSH_SMALL_PAN_REPLACE()
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
            }

            ApplyPermissions();

            DataTable dt = new DataTable();
            GetRemainInfo(ref dt);
            if (dt != null)
                SetInfo(dt); // SetData

            InitControl(); // Control 초기화
        }

        private DataTable GetRemainInfo(ref DataTable selectDt)
        {
            const string bizRuleName = "DA_PRD_SEL_INPUT_LOT_REMAIN_INFO_WS";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["EQPTID"] = _equipmentCode;
            inData["EQPT_MOUNT_PSTN_ID"] = _equipmentPositionId;
            inData["INPUT_LOTID"] = _inputLotId;

            inDataTable.Rows.Add(inData);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inDataTable);
            //string sTextXml = ds.GetXml();

            selectDt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            return selectDt;
        }

        private void SetInfo(DataTable dt)
        {
            int iWipQty = 0;                         // 투입수량
            int iChangeQty = 0;                      // 잔량

            if (CommonVerify.HasTableRow(dt))  // 조회했을 때 데이터가있을경우 조회해온 dt에서 값 바인딩
            {
                txtLotId.Text = Util.NVC(dt.Rows[0]["PROD_LOTID"]);             // Lot ID
                txtWindingLotId.Text = Util.NVC(dt.Rows[0]["INPUT_LOTID"]);     // Winding Lot ID
                iWipQty = Util.NVC_Int(dt.Rows[0]["INPUT_QTY"]);                // 투입수량
            }
            else                                  // 조회했을 때 데이터가없을경우 메인에서 넘겨받은 파라미터 값 바인딩
            {
                txtLotId.Text = Util.NVC(_inputLotId); // Lot ID
            }

            string sWipQty = $"{iWipQty:#,###}";
            txtWipQty.Text = iWipQty != 0 ? Util.NVC(sWipQty) : Util.NVC(0);

            string sChangeQty = $"{iChangeQty:#,###}";
            txtChangeQty.Text = iChangeQty != 0 ? Util.NVC(sChangeQty) : Util.NVC(0);
        }

        private void InitControl()
        {
            // HOLD 사유 코드
            CommonCombo combo = new CommonCombo();

            // HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            txtChangeQty.IsReadOnly = false;    // ea 직접 입력 가능하도록 기능 변경 요청으로 수정.

            if (!_processCode.Equals(Process.ASSEMBLY))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);
                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;
            }

            if (!_inputTypeCode.Equals("PROD"))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;

                grdStats.Visibility = Visibility.Collapsed;

                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);
                grdContents.RowDefinitions[6].Height = new GridLength(0);
                grdContents.RowDefinitions[7].Height = new GridLength(0);
                grdContents.RowDefinitions[8].Height = new GridLength(0);
                grdContents.RowDefinitions[9].Height = new GridLength(0);
                grdContents.RowDefinitions[10].Height = new GridLength(0);
                grdContents.RowDefinitions[11].Height = new GridLength(0);
                grdContents.RowDefinitions[12].Height = new GridLength(0);
                grdContents.RowDefinitions[13].Height = new GridLength(0);
            }
        }


        private void rdoHold_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (cboHoldReason != null)
                    cboHoldReason.IsEnabled = true;

                if (dtExpected != null)
                    dtExpected.IsEnabled = true;
            }
        }

        private void rdoWait_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (cboHoldReason != null)
                    cboHoldReason.IsEnabled = false;

                if (dtExpected != null)
                    dtExpected.IsEnabled = false;
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
            if (txtChangeQty.Text.Trim().Equals("") || txtChangeQty.Text.Trim().Equals("0"))
            {
                //Util.Alert("잔량이 없습니다.");
                Util.MessageValidation("SFU1859");
                return false;
            }
            if (txtChangeReason.Text.Trim().Equals(""))
            {
                //Util.Alert("특이사항을 입력하세요.");
                Util.MessageValidation("SFU1992");
                return false;
            }

            double dChg;
            double.TryParse(txtChangeQty.Text, out dChg);

            if (rdoHold.IsChecked.HasValue && !(bool)rdoHold.IsChecked &&
                rdoWait.IsChecked.HasValue && !(bool)rdoWait.IsChecked)
            {
                //Util.Alert("상태변경 여부를 선택 하세요.");
                Util.MessageValidation("SFU1600");
                return false;
            }


            if (rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
            {
                if (cboHoldReason?.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("HOLD 사유를 선택 하세요.");
                    Util.MessageValidation("SFU1342");
                    return false;
                }
            }
            return true;
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT";

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
                //newRow["PROD_LOTID"] = Util.NVC(_inputLotId);
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
                newRow["INPUT_LOTID"] = txtLotId.Text;
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                newRow["ACTQTY"] = Util.NVC(txtChangeQty.Text).Equals(string.Empty) ? 0 : Convert.ToDecimal(txtChangeQty.Text);

                inInputLot.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, inDataSet);

                if (_inputTypeCode.Equals("PROD") && rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
                {
                    if (!HoldProcess())
                    {
                        return;
                    }
                }

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

        private bool HoldProcess()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_HOLD_LOT";

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("ACTION_USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                DataRow inRow = inData.NewRow();
                inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inRow["LANGID"] = LoginInfo.LANGID;
                inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inRow["USERID"] = LoginInfo.USERID;
                inRow["ACTION_USERID"] = string.Empty;

                inData.Rows.Add(inRow);

                inRow = inLot.NewRow();
                inRow["LOTID"] = txtLotId.Text;
                inRow["HOLD_NOTE"] = txtChangeReason.Text;
                inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                inLot.Rows.Add(inRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null, inDataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void txtChangeQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}
