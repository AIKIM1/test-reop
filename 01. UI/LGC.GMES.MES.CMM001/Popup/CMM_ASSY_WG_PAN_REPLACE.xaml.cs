/*************************************************************************************
 Created Date : 2017.06.30
      Creator : 이대영D
   Decription : Assembly 잔량처리(원각)
--------------------------------------------------------------------------------------
 [Change History]
   2017.06.30   INS 이대영D : Initial Created.

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
    public partial class CMM_ASSY_WG_PAN_REPLACE : C1Window, IWorkArea
    {
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private double _WipQty = 0;
        private string _Position = string.Empty;
        private string _ProcID = string.Empty;
        private string _Input_type_code = string.Empty;

        private BizDataSet _Biz = new BizDataSet();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_WG_PAN_REPLACE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 8)
            {
                _LineID = Util.NVC(tmps[0]);                                                    // Line
                _EqptID = Util.NVC(tmps[1]);                                                    // 설비코드
                _LotID = Util.NVC(tmps[2]);                                                     // 투입LOT
                _WipSeq = Util.NVC(tmps[3]);                                                    // 재공 일련번호
                _WipQty = Util.NVC(tmps[4]).Equals("") ? 0 : double.Parse(Util.NVC(tmps[4]));   // 투입량
                _Position = Util.NVC(tmps[5]);                                                  // 위치
                _ProcID = Util.NVC(tmps[6]);                                                    // Process
                _Input_type_code = Util.NVC(tmps[7]);                                           // 투입제품타입
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _WipQty = 0;
                _Position = "";
                _ProcID = "";
                _Input_type_code = "";
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
            string sBizName = string.Empty;
            sBizName = "DA_PRD_SEL_WINDING_REMAIN_INFO_AS";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["EQPTID"] = _EqptID;
            inData["EQPT_MOUNT_PSTN_ID"] = _Position;
            inData["INPUT_LOTID"] = _LotID;

            inDataTable.Rows.Add(inData);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inDataTable);
            //string sTextXml = ds.GetXml();

            selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

            return selectDt;
        }

        private void SetInfo(DataTable dt)
        {
            int iWipQty = 0;                         // 투입수량
            int iChangeQty = 0;                      // 잔량
            string sWipQty = string.Empty;           // 투입수량 콤마 삽입을 위한 변수
            string sChangeQty = string.Empty;        // 잔량 콤마 삽입을 위한 변수

            if (dt != null && dt.Rows.Count != 0)  // 조회했을 때 데이터가있을경우 조회해온 dt에서 값 바인딩
            {
                txtLotId.Text = Util.NVC(dt.Rows[0]["PROD_LOTID"]);             // Lot ID
                txtWindingLotId.Text = Util.NVC(dt.Rows[0]["INPUT_LOTID"]);     // Winding Lot ID
                iWipQty = Util.NVC_Int(dt.Rows[0]["INPUT_QTY"]);                // 투입수량
                //txtRuncardId.Text = Util.NVC(dt.Rows[0]["WINDING_RUNCARD_ID"]); // 이력카드 ID
            }
            else                                  // 조회했을 때 데이터가없을경우 메인에서 넘겨받은 파라미터 값 바인딩
            {
                txtLotId.Text = Util.NVC(_LotID); // Lot ID
            }
            
            sWipQty = $"{iWipQty:#,###}";
            txtWipQty.Text = iWipQty != 0 ? Util.NVC(sWipQty) : Util.NVC(0);
            sChangeQty = $"{iChangeQty:#,###}";
            txtChangeQty.Text = iChangeQty != 0 ? Util.NVC(sChangeQty) : Util.NVC(0);
        }

        private void InitControl()
        {
            // HOLD 사유 코드
            CommonCombo _combo = new CommonCombo();

            // HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            txtChangeQty.IsReadOnly = false;    // ea 직접 입력 가능하도록 기능 변경 요청으로 수정.

            if (!_ProcID.Equals(Process.ASSEMBLY))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);
                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;
            }

            if (!_Input_type_code.Equals("PROD"))
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
            if (_Input_type_code.Equals("PROD"))
            {
                if (!CanSave())
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
            //listAuth.Add(btnConvert);
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (txtChangeQty.Text.Trim().Equals("") || txtChangeQty.Text.Trim().Equals("0"))
            {
                //Util.Alert("잔량이 없습니다.");
                Util.MessageValidation("SFU1859");
                return bRet;
            }
            if (txtChangeReason.Text.Trim().Equals(""))
            {
                //Util.Alert("특이사항을 입력하세요.");
                Util.MessageValidation("SFU1992");
                return bRet;
            }

            double dTot, dChg;
            double.TryParse(txtChangeQty.Text, out dChg);

            if (rdoHold.IsChecked.HasValue && !(bool)rdoHold.IsChecked &&
                rdoWait.IsChecked.HasValue && !(bool)rdoWait.IsChecked)
            {
                //Util.Alert("상태변경 여부를 선택 하세요.");
                Util.MessageValidation("SFU1600");
                return bRet;
            }


            if (rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
            {
                if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("HOLD 사유를 선택 하세요.");
                    Util.MessageValidation("SFU1342");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT_AS";

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
                newRow["EQPTID"] = Util.NVC(_EqptID);
                newRow["USERID"] = LoginInfo.USERID;
                //newRow["PROD_LOTID"] = Util.NVC(_LotID);
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUT_LOT_TYPE"] = Util.NVC(_Input_type_code);

                inData.Rows.Add(newRow);
                newRow = null;

                DataTable inInputLot = inDataSet.Tables.Add("IN_INPUT");
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
                inInputLot.Columns.Add("WIPNOTE", typeof(string));
                inInputLot.Columns.Add("ACTQTY", typeof(Decimal));

                newRow = inInputLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(_Position);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = Util.NVC(_LotID);
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                newRow["ACTQTY"] = Util.NVC(txtChangeQty.Text).Equals(string.Empty) ? 0 : Convert.ToDecimal(txtChangeQty.Text);

                inInputLot.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, inDataSet);

                if (_Input_type_code.Equals("PROD") && rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
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
                ShowLoadingIndicator();

                string sBizName = "BR_PRD_REG_HOLD_LOT";

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
                inRow = null;

                inRow = inLot.NewRow();
                //inRow["LOTID"] = txtLotId.Text;
                inRow["LOTID"] = _LotID;
                inRow["HOLD_NOTE"] = txtChangeReason.Text;
                inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                inLot.Rows.Add(inRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inDataSet);

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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
