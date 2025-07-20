/*************************************************************************************
 Created Date : 2019.01.16
      Creator : 신광희 차장
   Decription : 자재 공급 요청 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.16  신광희 차장 : Initial Created.

**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Data;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_014.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        #endregion

        public MCS001_014()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_GET_MTRL_SPLY_REQ_LIST";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("MLOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["MTRL_SPLY_REQ_STAT_CODE"] = cboRequestType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["MTRLID"] = !string.IsNullOrEmpty(txtMaterialId.Text.Trim()) ? txtMaterialId.Text : null;
                dr["MLOTID"] = !string.IsNullOrEmpty(txtFoilId.Text.Trim()) ? txtFoilId.Text : null;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgLotList, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        #endregion

        #region Mehod
        private void InitializeCombo()
        {
            SetStockerCombo(cboStocker);

            //TODO 요청상태 콤보
            SetRequestStateCombo(cboRequestType);
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "COT", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetRequestStateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE"};
            string[] arrCondition = { LoginInfo.LANGID, "MCS_MTRL_SPLY_REQ_STAT_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
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
