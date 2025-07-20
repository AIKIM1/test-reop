/*************************************************************************************
 Created Date : 2018.11.09
      Creator : 신광희 차장
   Decription : CWA 전극 RACK 정보 수정 팝업
--------------------------------------------------------------------------------------
  [Change History]
   2019.01.02   신광희C  : Rack 정보 변경 및 수정 시 AD 인증 처리 로직 추가
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001_PANCAKE_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_RACK_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }
        public bool IsUpdated;
        private string _equipmentCode;
        private string _rackStateCode;
        private string _carrierCode;
        private const string AuthGroup = "LOGIS_MANA";// 물류관리자
        private string _userId;
        #endregion

        public MCS001_011_RACK_INFO()
        {
            InitializeComponent();
        }

        #region Initialize 

        private void InitializeControls()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Stocker 콤보박스
            SetStockerCombo(cboStocker);
            // 상태 콤보박스
            SetRackState(cboRackState);
        }


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();

            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataRow drRackInfo = parameters[1] as DataRow;

                if (drRackInfo != null)
                    SetRackInfo(drRackInfo);

            }

            // Stocker 정보 Setting
            if (!string.IsNullOrEmpty(_equipmentCode))
            {
                for (int i = 0; i < cboStocker.Items.Count; i++)
                {
                    if (string.Equals(_equipmentCode, ((DataRowView) cboStocker.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboStocker.SelectedIndex = i;
                        cboStocker.IsEnabled = false;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_rackStateCode))
            {
                for (int i = 0; i < cboRackState.Items.Count; i++)
                {
                    if (string.Equals(_rackStateCode, ((DataRowView)cboRackState.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboRackState.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void SetRackInfo(DataRow drRackInfo)
        {
            txtRackId.Text = drRackInfo["RACK_ID"].GetString();
            txtProjectName.Text = drRackInfo["PRJT_NAME"].GetString();
            txtProductId.Text = drRackInfo["PRODID"].GetString();
            txtProductName.Text = drRackInfo["PRODNAME"].GetString();
            txtStartTime.Text = drRackInfo["WH_RCV_DTTM"].GetString();
            txtLotId.Text = drRackInfo["LOTID"].GetString();
            txtQty.Text = drRackInfo["WIPQTY"].GetInt().GetString();
            txtvalidDt.Text = drRackInfo["VLD_DATE"].GetString();
            txtPastDay.Text = drRackInfo["PAST_DAY"].GetString();
            txtWipHold.Text = drRackInfo["WIPHOLD"].GetString();
            _rackStateCode = drRackInfo["RACK_STAT_CODE"].GetString();
            _carrierCode = drRackInfo["MCS_CST_ID"].GetString();
        }

        private void txtChangeLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtChangeLotId.Text.Trim()))
                    GetLotInfo();
            }
        }

        private void btnDeleteLotInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationDeleteLotInfo()) return;

                CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM {FrameOperation = FrameOperation};

                object[] parameters = new object[1];
                parameters[0] = AuthGroup;
                C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

                popupAuthConfirm.Closed += popupDeleteLotInfo_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupDeleteLotInfo_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
                DeleteLotInfo();
            }
        }

        private void btnChangeLotInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeLotInfo()) return;

            CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = AuthGroup;
            C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

            popupAuthConfirm.Closed += popupChangeLotInfo_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));

        }

        private void popupChangeLotInfo_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
                ChangeLotInfo();
            }
        }

        private void btnChangeRackState_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeRackState()) return;

            CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = AuthGroup;
            C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

            popupAuthConfirm.Closed += popupChangeRackState_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
        }

        private void popupChangeRackState_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
                ChangeRackState();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        #endregion

        #region Mehod

        private void GetLotInfo()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "DA_PRD_SEL_LOT_STATUS";

            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = txtChangeLotId.Text.Trim();
            inDataTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();
                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                Util.GridSetData(dgRackInfo, bizResult, null, true);
            });


        }

        private void DeleteLotInfo()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOT_INFO_ON_RACK";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("ACT_TYPE", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["RACK_ID"] = txtRackId.Text;
                newRow["UPDUSER"] = _userId;
                newRow["ACT_TYPE"] = "D";   //D / C (DELETE / CHANGE)
                inTable.Rows.Add(newRow);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inLot.Columns.Add("REEL_TYPE_CODE", typeof(string));
                DataRow dr = inLot.NewRow();
                dr["LOTID"] = txtLotId.Text;
                inLot.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        _userId = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeLotInfo()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOT_INFO_ON_RACK";
                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("ACT_TYPE", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["RACK_ID"] = txtRackId.Text;
                newRow["UPDUSER"] = _userId;
                newRow["ACT_TYPE"] = "C";   //D / C (DELETE / CHANGE)
                inTable.Rows.Add(newRow);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inLot.Columns.Add("REEL_TYPE_CODE", typeof(string));
                DataRow dr = inLot.NewRow();
                dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "LOTID"));
                dr["EMPTY_REEL_TYPE_CODE"] = null;
                dr["REEL_TYPE_CODE"] = null;
                inLot.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        _userId = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeRackState()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_RACK_INFO_CHANGE";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("RACK_ID", typeof(string));
                inDataTable.Columns.Add("RACK_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("REEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["RACK_ID"] = txtRackId.Text;
                dr["RACK_STAT_CODE"] = cboRackState.SelectedValue;
                dr["EMPTY_REEL_TYPE_CODE"] = null;
                dr["REEL_TYPE_CODE"] = null;
                dr["MCS_CST_ID"] = !string.IsNullOrEmpty(_carrierCode) ? _carrierCode : null;
                dr["UPDUSER"] = _userId;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    IsUpdated = true;
                    _userId = string.Empty;
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationDeleteLotInfo()
        {
            if (string.IsNullOrEmpty(txtLotId.Text))
            {   //LOT ID 가 없습니다.
                Util.MessageValidation("SFU1361");
                return false;
            }

            return true;
        }

        private bool ValidationChangeLotInfo()
        {
            if (dgRackInfo.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "LOTID"))))
            {
                Util.MessageValidation("SFU1361");
                return false;
            }

            return true;
        }

        private bool ValidationChangeRackState()
        {
            if (cboRackState?.SelectedValue == null || cboRackState.Items.Count < 1)
            {
                Util.MessageValidation("SFU5059");
                return false;
            }

            return true;
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "JRW", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private static void SetRackState(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "CMCDTYPE", "ATTRIBUTE1"};
            string[] arrCondition = { "MCS_RACK_STAT_CODE", "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
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


    }
}
