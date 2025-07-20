/*************************************************************************************
 Created Date : 2021.01.25
      Creator : 조영대
   Decription : 반송요청현황 및 이력 - 반송 요청 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.27  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_054_ADD_REQUEST : C1Window, IWorkArea
    {
        #region Declaration

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize


        public MCS001_054_ADD_REQUEST()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            CommonCombo comboSet = new CommonCombo();

            // 처리유형
            cboProcessType.SetCommonCode("MHS_PRCS_TYPE_CODE", CommonCombo.ComboStatus.SELECT);

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                //요청처리 고정하고 비활성화
                cboProcessType.SelectedValue = Util.NVC(parameters[0]);
                cboProcessType.IsEnabled = false;
            }
           

            // 요청 반송유형
            cboReqReturnType.SetCommonCode("MHS_PORT_TYPE_CODE", CommonCombo.ComboStatus.SELECT);

            //동
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 상태
            cboCstState.SetCommonCode("CSTSTAT", CommonCombo.ComboStatus.SELECT);
            
        }

        #endregion

        #region Event
        private void cboProcessType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboSystemType.Items.Count.Equals(2))
            {
                cboSystemType.SelectedIndex = 1;
            }
            else
            {
                cboSystemType.SelectedIndex = 0;
            }
            cboReqEqptGroup.SelectedIndex = 0;
            cboReqEqpt.SelectedIndex = 0;
            cboReqPort.SelectedIndex = 0;
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;

            SetInputControlState();
        }

        private void cboReqReturnType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboSystemType.Items.Count.Equals(2))
            {
                cboSystemType.SelectedIndex = 1;
            }
            else
            {
                cboSystemType.SelectedIndex = 0;
            }
            cboReqEqptGroup.SelectedIndex = 0;
            cboReqEqpt.SelectedIndex = 0;
            cboReqPort.SelectedIndex = 0;
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;

            SetInputControlState();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboSystemType.SelectedIndex = 0;
            cboReqEqptGroup.SelectedIndex = 0;
            cboReqEqpt.SelectedIndex = 0;
            cboReqPort.SelectedIndex = 0;
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;

            SetSystemTypeCombo();
            SetReqEqptGroupCombo();
        }

        private void cboReqEqptGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboReqEqpt.SelectedIndex = 0;
            cboReqPort.SelectedIndex = 0;
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;

            SetReqEqptCombo();         
        }

        private void cboReqEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboReqPort.SelectedIndex = 0;
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;

            SetReqPortCombo();
        }

        private void cboReqPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            cboCstState.SelectedIndex = 0;
            cboRuleId.SelectedIndex = 0;
            cboFilterType.SelectedIndex = 0;
            cboSortType.SelectedIndex = 0;
        }

        private void txtCarrierId_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(System.Windows.Input.Key.Enter) ||
                e.Key.Equals(System.Windows.Input.Key.Tab))
            {
                if (FindLotId())
                {
                    SetRuleIdCombo("RULE");
                    SetRuleIdCombo("FLTR");
                    SetRuleIdCombo("SORT");
                }
            }
        }

        private void cboCstState_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboCstState.IsEnabled)
            {
                SetRuleIdCombo("RULE");
                SetRuleIdCombo("FLTR");
                SetRuleIdCombo("SORT");
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ChkValidation()) return;

                // %1 (을)를 등록 하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4330", ""), null,
                    "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            AddRequest();
                        }
                    });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region Method

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

        private void SetInputControlState()
        {
            if (Util.NVC(cboProcessType.SelectedValue).Equals("EQPT_REQ") ||
                Util.NVC(cboProcessType.SelectedValue).Equals("EQPT_PRC"))
            {
                switch (Util.NVC(cboReqReturnType.SelectedValue))
                {
                    case "LR":
                        txtCarrierId.Text = string.Empty;
                        txtCarrierId.IsEnabled = false;
                        cboCstState.IsEnabled = true;
                        cboCstState.SelectedIndex = 0;
                        break;
                    case "UR":
                        txtCarrierId.Text = string.Empty;
                        txtCarrierId.IsEnabled = true;
                        cboCstState.IsEnabled = false;
                        cboCstState.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                txtCarrierId.Text = string.Empty;
                txtCarrierId.IsEnabled = false;
                cboCstState.IsEnabled = true;
                cboCstState.SelectedIndex = 0;
            }
        }

        private void SetSystemTypeCombo()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drNewRow = dtRQSTDT.NewRow();
                drNewRow["LANGID"] = LoginInfo.LANGID;
                drNewRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dtRQSTDT.Rows.Add(drNewRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_REQ_SYSTEM_TYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                
                cboSystemType.DisplayMemberPath = "CBO_NAME";
                cboSystemType.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow[cboSystemType.SelectedValuePath] = string.Empty;
                newRow[cboSystemType.DisplayMemberPath] = "SELECT";
                dtResult.Rows.InsertAt(newRow, 0);

                cboSystemType.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count.Equals(2))
                {
                    cboSystemType.SelectedIndex = 1;
                }
                else
                {
                    cboSystemType.SelectedIndex = 0;
                }                    
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReqEqptGroupCombo()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drInRow = dtRQSTDT.NewRow();
                drInRow["LANGID"] = LoginInfo.LANGID;
                drInRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dtRQSTDT.Rows.Add(drInRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_EQUIPMENTGROUP_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                      
                cboReqEqptGroup.DisplayMemberPath = "CBO_NAME";
                cboReqEqptGroup.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow[cboReqEqptGroup.SelectedValuePath] = string.Empty;
                newRow[cboReqEqptGroup.DisplayMemberPath] = "SELECT";
                dtResult.Rows.InsertAt(newRow, 0);

                cboReqEqptGroup.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count.Equals(2))
                {
                    cboReqEqptGroup.SelectedIndex = 1;
                }
                else
                {
                    cboReqEqptGroup.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReqEqptCombo()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQGRID", typeof(string));

                DataRow drInRow = dtRQSTDT.NewRow();
                drInRow["LANGID"] = LoginInfo.LANGID;
                drInRow["AREAID"] = cboArea.GetBindValue();
                drInRow["EQGRID"] = cboReqEqptGroup.GetBindValue();
                dtRQSTDT.Rows.Add(drInRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_FOR_MHS", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboReqEqpt.DisplayMemberPath = "CBO_NAME";
                cboReqEqpt.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow[cboReqEqpt.SelectedValuePath] = string.Empty;
                newRow[cboReqEqpt.DisplayMemberPath] = "SELECT";
                dtResult.Rows.InsertAt(newRow, 0);

                cboReqEqpt.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count.Equals(2))
                {
                    cboReqEqpt.SelectedIndex = 1;
                }
                else
                {
                    cboReqEqpt.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReqPortCombo()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("IS_REP_PORT", typeof(string));

                DataRow drInRow = dtRQSTDT.NewRow();
                drInRow["LANGID"] = LoginInfo.LANGID;
                drInRow["EQPTID"] = Util.NVC(cboReqEqpt.SelectedValue);
                if (Util.NVC(cboReqEqptGroup.SelectedValue).Equals("STO"))
                {
                    drInRow["IS_REP_PORT"] = "Y";
                }
                dtRQSTDT.Rows.Add(drInRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MCS_PORT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboReqPort.DisplayMemberPath = "CBO_NAME";
                cboReqPort.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow[cboReqPort.SelectedValuePath] = string.Empty;
                newRow[cboReqPort.DisplayMemberPath] = "SELECT";
                dtResult.Rows.InsertAt(newRow, 0);

                cboReqPort.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count.Equals(2))
                {
                    cboReqPort.SelectedIndex = 1;
                }
                else
                {
                    cboReqPort.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRuleIdCombo(string workType)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("TYPE", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("MES_SYSTEM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("EQGRID", typeof(string));                
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("PORTID", typeof(string));
                dtRQSTDT.Columns.Add("TRF_EQPT_GR_ID", typeof(string));
                dtRQSTDT.Columns.Add("TRF_MODE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("RULE_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("CSTSTAT", typeof(string));

                DataRow drInRow = dtRQSTDT.NewRow();
                drInRow["LANGID"] = LoginInfo.LANGID;
                drInRow["TYPE"] = workType;
                drInRow["AREAID"] = cboArea.GetBindValue();
                drInRow["MES_SYSTEM_TYPE_CODE"] = cboSystemType.GetBindValue();
                drInRow["EQGRID"] = cboReqEqptGroup.GetBindValue();
                drInRow["EQPTID"] = cboReqEqpt.GetBindValue();
                drInRow["PORTID"] = cboReqPort.GetBindValue();
                drInRow["TRF_EQPT_GR_ID"] = cboReqEqpt.GetBindValue("EQPT_GR_TYPE_CODE");
                drInRow["TRF_MODE_CODE"] = "AUTO";
                drInRow["RULE_TYPE_CODE"] = cboReqReturnType.GetBindValue();
                drInRow["CSTSTAT"] = cboCstState.GetBindValue();

                dtRQSTDT.Rows.Add(drInRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MMD_RTD_TRF_EQPT_GR_TRF_RULE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                
                C1ComboBox cbo = cboRuleId;
                switch (workType)
                {
                    case "RULE":
                        cbo = cboRuleId;
                        break;
                    case "FLTR":
                        cbo = cboFilterType;
                        break;
                    case "SORT":
                        cbo = cboSortType;
                        break;
                }

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow newRow = dtResult.NewRow();
                newRow[cbo.SelectedValuePath] = string.Empty;
                newRow[cbo.DisplayMemberPath] = "SELECT";
                dtResult.Rows.InsertAt(newRow, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool FindLotId()
        {
            try
            {
                if (Util.IsNVC(txtCarrierId.Text.Trim()))
                {
                    txtLotId.Text = string.Empty;
                    return true;
                }
                
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow drInRow = dtRQSTDT.NewRow();
                drInRow["CSTID"] = txtCarrierId.Text.Trim();
                dtRQSTDT.Rows.Add(drInRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_CARRIER_FIND_CURRENT_LOT", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtLotId.Text = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    cboCstState.SelectedValue = Util.NVC(dtResult.Rows[0]["CSTSTAT"]);
                    return true;
                }
                else
                {
                    txtLotId.Text = string.Empty;

                    if (txtCarrierId.IsEnabled && !Util.IsNVC(txtCarrierId.Text))
                    // 등록되지 않은 CARRIER ID입니다.
                    Util.MessageValidation("SFU8237");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }
        
        private bool ChkValidation()
        {
            if (cboProcessType.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblProcessType.Text + "]");
                return false;
            }

            if (cboReqReturnType.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblReqReturnType.Text + "]");
                return false;
            }

            if (cboArea.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblArea.Text + "]");
                return false;
            }

            if (cboSystemType.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblSystemType.Text + "]");
                return false;
            }
            
            if (cboReqEqptGroup.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblReqEqptGroup.Text + "]");
                return false;
            }

            if (cboReqEqpt.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblReqEqpt.Text + "]");
                return false;
            }

            if (cboReqPort.GetBindValue() == null)
            {
                // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                Util.MessageValidation("SFU5070", "[" + lblReqPort.Text + "]");
                return false;
            }

            if (txtCarrierId.IsEnabled)
            {
                if (txtCarrierId.GetBindValue() == null)
                {
                    // %1 (을)를 입력해 주세요.
                    Util.MessageValidation("SFU8275", "[" + lblCarrierId.Text + "]");
                    return false;
                }
                else
                {
                    if (!FindLotId()) return false;
                }
            }

            if (cboCstState.IsEnabled && cboCstState.GetBindValue() == null)
            {
                // %1 (을)를 입력해 주세요.
                Util.MessageValidation("SFU8275", "[" + lblCstState.Text + "]");
                return false;
            }

            if (cboRuleId.GetBindValue() == null)
            {
                // %1 (을)를 입력해 주세요.
                Util.MessageValidation("SFU8275", "[" + lblRuleId.Text + "]");
                return false;
            }

            return true;
        }


        private void AddRequest()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("REQ_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRCS_TYPE_CODE", typeof(string));
                inTable.Columns.Add("RTD_RULE_ID", typeof(string));
                inTable.Columns.Add("FLTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("SORT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));

                DataRow inRow = inTable.NewRow();
                inRow["PRCS_TYPE_CODE"] = cboProcessType.GetBindValue();
                inRow["REQ_TYPE_CODE"] = cboReqReturnType.GetBindValue();
                inRow["EQPTID"] = cboReqEqpt.GetBindValue();
                inRow["PORT_ID"] = cboReqPort.GetBindValue();
                inRow["CSTID"] = txtCarrierId.Text.Trim();
                inRow["CSTSTAT"] = cboCstState.GetBindValue();
                inRow["LOTID"] = txtLotId.Text.Trim();                
                inRow["RTD_RULE_ID"] = cboRuleId.GetBindValue();
                inRow["FLTR_TYPE_CODE"] = cboFilterType.GetBindValue();
                inRow["SORT_TYPE_CODE"] = cboSortType.GetBindValue();
                inRow["USER_ID"] = LoginInfo.USERID;
                inTable.Rows.Add(inRow);

                new ClientProxy().ExecuteService("BR_MHS_REG_TRF_REQ_MANUAL_UI", "INDATA", null, inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // 등록하였습니다.
                        Util.MessageValidation("SFU1518");

                        DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }




        #endregion


    }
}
