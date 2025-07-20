/*************************************************************************************
 Created Date : 2018.10.05
      Creator : 신광희 차장
   Decription : CWA물류 - 자재공급 요청 / 요청 취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.05  신광희 차장 : Initial Created.        
**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_015_FOIL_SUPPLY_REQUEST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_015_FOIL_SUPPLY_REQUEST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        private bool _isLoaded;
        public bool IsUpdated;
        private readonly Util _util = new Util();
        private string _materialElectrodeType = string.Empty;
        #endregion

        public MCS001_015_FOIL_SUPPLY_REQUEST()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void InitializeControls()
        {
            dgMaterialSupply.Columns["CHK"].Visibility = Visibility.Collapsed;
            InitializeComboBox();
            GetUserInfo();
        }

        private void InitializeComboBox()
        {
            SetStockerCombo(cboEquipment);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {

            }

            InitializeControls();
            _isLoaded = true;
        }

        private void btnMaterialSupplyRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_GET_MTRL_SPLY_REQ_LIST";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("MLOTID", typeof(string));
                inDataTable.Columns.Add("QRY_TYPE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["MTRL_ELTR_TYPE_CODE"] = rdoAnode.IsChecked != null && (bool) rdoAnode.IsChecked ? "C" : "A";
                dr["MTRL_SPLY_REQ_STAT_CODE"] = null;
                dr["EQPTID"] = null;
                dr["MTRLID"] = null;
                dr["MLOTID"] = null;
                dr["QRY_TYPE"] = "REQ";
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgMaterialSupply, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_isLoaded) return;

                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();
                if (tabItem == "TabMaterialSupply")
                {
                    dgMaterialSupply.Columns["CHK"].Visibility = Visibility.Collapsed;
                    btnRequestCancel.Visibility = Visibility.Collapsed;
                    btnCarrierRequest.Visibility = Visibility.Visible;
                    btnMaterialRequest.Visibility = Visibility.Visible;
                }
                else
                {
                    dgMaterialSupply.Columns["CHK"].Visibility = Visibility.Visible;
                    btnRequestCancel.Visibility = Visibility.Visible;
                    btnCarrierRequest.Visibility = Visibility.Collapsed;
                    btnMaterialRequest.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnCarrierRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCarrierRequest()) return;

            try
            {
                SaveMaterialSupply("E");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMaterialRequest()) return;

            try
            {
                SaveMaterialSupply("M");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRequestCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRequestCancel()) return;

            try
            {
                SaveMaterialSupply("C");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment?.SelectedValue == null) return;

            txtWorkOrderDetail.Text = string.Empty;
            txtFoilId.Text = string.Empty;
            txtFoilName.Text = string.Empty;
            _materialElectrodeType = string.Empty;

            GetFoilMaterialInfo();
        }
        #endregion

        #region Mehod

        private void GetFoilMaterialInfo()
        {

            try
            {
                string bizRuleName = "BR_MCS_GET_MTRL_INFO_BY_EQPTID";
                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                inTable.Rows.Add(dr);
                //string xml = ds.GetXml();
                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA,OUT_WOID", ds);

                if (CommonVerify.HasTableInDataSet(dsResult) && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    bizRuleName = "DA_MCS_SEL_FOIL_MATERIAL_BY_EQPTID";

                    DataTable inDataTable = new DataTable("RQSTDT");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = cboEquipment.SelectedValue;
                    inDataTable.Rows.Add(newRow);

                    DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

                    if (CommonVerify.HasTableRow(searchResult))
                    {
                        txtFoilId.Text = searchResult.Rows[0]["MTRLID"].GetString();
                        txtFoilName.Text = searchResult.Rows[0]["MTRLDESC"].GetString();
                        txtWorkOrderDetail.Text = searchResult.Rows[0]["WO_DETL_ID"].GetString();
                        _materialElectrodeType = searchResult.Rows[0]["PRDT_CLSS_CODE"].GetString();
                    }
                }
            }
            catch (Exception)
            {
                
            }


        }

        private void SaveMaterialSupply(string requestType)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_MTRL_SPLY_REQ_BY_COATER";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("REQ_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["REQ_TYPE"] = requestType;
                newRow["USERID"] = requestType.Equals("C") ? txtUserName.Tag : LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrl = ds.Tables.Add("INMTRL");
                inMtrl.Columns.Add("EQPTID", typeof(string));
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                inMtrl.Columns.Add("WO_DETL_ID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inMtrl.Columns.Add("NOTE", typeof(string));

                switch (requestType)
                {
                    case "M":
                    {
                        DataRow dr = inMtrl.NewRow();
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["MTRLID"] = txtFoilId.Text;
                        dr["MTRL_ELTR_TYPE_CODE"] = _materialElectrodeType;
                        dr["WO_DETL_ID"] = txtWorkOrderDetail.Text;
                        inMtrl.Rows.Add(dr);
                        break;
                    }
                    case "E":
                    {
                        DataRow dr = inMtrl.NewRow();
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["MTRLID"] = txtFoilId.Text;
                        dr["MTRL_ELTR_TYPE_CODE"] = _materialElectrodeType;
                        inMtrl.Rows.Add(dr);
                        break;
                    }
                    default:
                        foreach (C1.WPF.DataGrid.DataGridRow row in dgMaterialSupply.Rows)
                        {
                            if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                            {
                                DataRow dataRow = inMtrl.NewRow();

                                dataRow["MTRL_SPLY_REQ_ID"] = DataTableConverter.GetValue(row.DataItem, "MTRL_SPLY_REQ_ID").GetString();
                                dataRow["NOTE"] = txtReason.Text;
                                inMtrl.Rows.Add(dataRow);
                            }
                        }
                        break;
                }

                //string xml = ds.GetXml();

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

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);
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

        private void PopupUser()
        {
            CMM_PERSON popPerson = new CMM_PERSON {FrameOperation = FrameOperation};

            object[] parameters = new object[1];
            parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popPerson, parameters);

            popPerson.Closed += popupUser_Closed;
            Dispatcher.BeginInvoke(new Action(() => popPerson.ShowModal()));
        }

        private void popupUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;
            }
        }

        private void GetUserInfo()
        {
            txtUserName.Text = LoginInfo.USERNAME;
            txtUserName.Tag = LoginInfo.USERID;
        }

        private bool ValidationCarrierRequest()
        {
            //if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialSupply, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgMaterialSupply))
            //{
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (cboEquipment?.SelectedValue == null)
            {
                Util.MessageValidation("SFU1153");
                return false;
            }

            if (string.IsNullOrEmpty(txtFoilId.Text))
            {
                Util.MessageValidation("SFU1335");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialRequest()
        {
            if (cboEquipment?.SelectedValue == null)
            {
                Util.MessageValidation("SFU1153");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorkOrderDetail.Text))
            {
                Util.MessageValidation("SFU1849");
                return false;
            }

            if (string.IsNullOrEmpty(txtFoilId.Text))
            {
                Util.MessageValidation("SFU1335");
                return false;
            }

            return true;
        }

        private bool ValidationRequestCancel()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialSupply, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgMaterialSupply))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                //취소자를 입력 해 주세요.
                Util.MessageValidation("SFU6010");
                return false;
            }

            if (string.IsNullOrEmpty(txtReason.Text.Trim()))
            {
                //사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            return true;
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "COT", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }





        #endregion


    }
}
