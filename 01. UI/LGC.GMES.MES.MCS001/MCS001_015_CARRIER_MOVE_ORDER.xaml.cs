/*************************************************************************************
 Created Date : 2018.10.03
      Creator : 신광희 차장
   Decription : CWA물류-전극 Carrier 이동 지시 신규 생성
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.03  신광희 차장 : Initial Created.      
**************************************************************************************/

using System;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_015_CARRIER_MOVE_ORDER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_015_CARRIER_MOVE_ORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        public bool IsUpdated;
        private string _fromPortCode;
        #endregion

        public MCS001_015_CARRIER_MOVE_ORDER()
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
            SetDestinationCombo(cboDestination);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _fromPortCode = Util.NVC(parameters[0]);
                txtPortId.Text = _fromPortCode;
                DataRow drPortInfo = parameters[1] as DataRow;

                if (drPortInfo != null)
                    SetPortInfo(drPortInfo);
            }

            InitializeControls();
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove()) return;

            Util.MessageConfirm("SFU6008", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveCarrierMove();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion



        #region Mehod

        private void SaveCarrierMove()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_FP_TO_TP";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("FROM_PORT_ID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("MCS_CST_ID", typeof(string));
                inLot.Columns.Add("MLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["FROM_PORT_ID"] = txtPortId.Text;
                newRow["TO_PORT_ID"] = cboDestination.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow dr = inLot.NewRow();
                dr["MCS_CST_ID"] = txtCarrierId.Text;
                dr["MLOTID"] = txtxtMaterialLotId.Text;
                inLot.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");//정상처리되었습니다.
                        IsUpdated = true;

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);

                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPortInfo(DataRow drPortInfo)
        {
            txtPortId.Text = drPortInfo["PORT_ID"].GetString();
            txtPortName.Text = drPortInfo["PORT_NAME"].GetString();
            txtCarrierId.Text = drPortInfo["MCS_CST_ID"].GetString();
            txtElectrodeType.Text = drPortInfo["ELTR_TYPE_CODE"].GetString();
            txtMaterialId.Text = drPortInfo["MTRLID"].GetString();
            txtMaterialName.Text = drPortInfo["MTRLDESC"].GetString();
            txtxtMaterialLotId.Text = drPortInfo["MLOTID"].GetString();
            txtCurrentQty.Text = drPortInfo["MLOTQTY_CUR"].GetInt().GetString();
            txtStockedQty.Text = drPortInfo["MLOTQTY_STOCKED"].GetInt().GetString();
            txtcalculateDate.Text = drPortInfo["CALDATE"].GetString();
            txtStockedDate.Text = drPortInfo["MLOTDTTM_STOCKED"].GetString();
        }

        private bool ValidationMove()
        {
            if (string.IsNullOrEmpty(cboDestination?.SelectedValue?.GetString()))
            {   //PORT가 설정되지 않았습니다.
                Util.MessageValidation("SFU5061");
                return false;
            }

            return true;
        }

        private void SetDestinationCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_RELATION_FOIL_CBO";
            string[] arrColumn = { "LANGID", "FROM_PORT_ID" };
            string[] arrCondition = { LoginInfo.LANGID, _fromPortCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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
