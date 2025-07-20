/*************************************************************************************
 Created Date : 2018.10.05
      Creator : 신광희 차장
   Decription : CWA물류-전극 Foil 공급/중단 요청 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.05  신광희 차장 : Initial Created.      
**************************************************************************************/

using System;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_015_FOIL_SUPPLY_CHOICE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_015_FOIL_SUPPLY_CHOICE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        public bool IsUpdated;
        private string _selectedValue;
        private string _selectedRadioButtonValue;
        private readonly Util _util = new Util();
        
        #endregion

        public MCS001_015_FOIL_SUPPLY_CHOICE()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void InitializeControls()
        {

        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                dtpDateFrom.SelectedDateTime = Convert.ToDateTime(parameters[0]);
                dtpDateTo.SelectedDateTime = Convert.ToDateTime(parameters[1]);
                _selectedValue = parameters[2].GetString();
                _selectedRadioButtonValue = parameters[3].GetString();

                if (string.Equals("C", _selectedRadioButtonValue))
                    rdoAnode.IsChecked = true;
                else
                    rdoCathode.IsChecked = true;

                SelectSupplyChoice();
            }

            InitializeControls();
        }

        private void btnFoilSetUp_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSetUp()) return;

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_MTRL_SPLY_REQ_TARGET_FLAG";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inDataTable.Columns.Add("TARGET_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgMaterialSupplyRequest, "CHK", true);

                DataRow dr = inDataTable.NewRow();
                dr["MTRL_SPLY_REQ_ID"] = DataTableConverter.GetValue(dgMaterialSupplyRequest.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").GetString();
                dr["TARGET_FLAG"] = "Y";
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
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

                        btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaterialSupplyRequestChoice_Checked(object sender, RoutedEventArgs e)
        {

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
                dr["MTRL_ELTR_TYPE_CODE"] = rdoAnode.IsChecked != null && (bool)rdoAnode.IsChecked ? "C" : "A";
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
                    Util.GridSetData(dgMaterialSupplyRequest, bizResult, null, true);
                });
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
        #endregion

        #region Mehod


        private void SelectSupplyChoice()
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
                dr["MTRL_ELTR_TYPE_CODE"] = rdoAnode.IsChecked != null && (bool)rdoAnode.IsChecked ? "C" : "A";
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
                    Util.GridSetData(dgMaterialSupplyRequest, bizResult, null, true);

                    foreach (C1.WPF.DataGrid.DataGridRow row in dgMaterialSupplyRequest.Rows)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "MTRL_SPLY_REQ_ID")) == _selectedValue)
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", true);
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationSetUp()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialSupplyRequest, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgMaterialSupplyRequest))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
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
