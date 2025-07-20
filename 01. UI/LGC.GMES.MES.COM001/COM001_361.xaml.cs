/*************************************************************************************
 Created Date : 2021.04.20
      Creator : 오광택
   Decription : 믹서 원자재 불출요청서 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.20          : Initial Created.

**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using SAP.Middleware.Connector;
using System.Linq;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// </summary>
    public partial class COM001_361 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }
        RfcDestination dest;

        public COM001_361()
        {
            InitializeComponent();
            SetComboBox(cboArea);
            SetComboBox(cboLine);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ERPDestinationConfig cfg = new ERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
            //chkFinlwip.IsChecked = true;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetStocks();
            }

            catch (Exception ex) { }
        }

        void SetComboBox(C1ComboBox cbo)
        {
            switch (cbo.Name)
            {
                case "cboArea":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_AREA_CBO", cbo, new string[] { "LANGID", "SHOPID", "USERID", "SYSTEM_ID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID, LoginInfo.SYSID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
                case "cboLine":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", cbo, new string[] { "LANGID", "AREAID" }, new string[] {  LoginInfo.LANGID, cboArea.SelectedValue as string}, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;

                case "cboStockLocation":
                    if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("ALL"))
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID" }, new string[] { LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    else
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID", "AREAID" }, new string[] { LoginInfo.CFG_SHOP_ID, cboArea.SelectedValue as string }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
            }
        }



        void GetStocks()
        {
            #region Initialize Grid
            dgStockList.ItemsSource = null;
            #endregion Initialize Grid


            DataTable dt = new DataTable();

            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("STRT_DATE", typeof(string));
            dt.Columns.Add("END_DATE", typeof(string));

            DataRow dtRow = dt.NewRow();
            dtRow["EQSGID"] = cboLine.SelectedValue;
            dtRow["STRT_DATE"] = ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            dtRow["END_DATE"] = ldpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
            dt.Rows.Add(dtRow);

            // 불출요청서 조회 비즈
            DataTable returndt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_REQUEST", "INDATA", "OUTDATA", dt);

            Util.GridSetData(dgStockList, returndt, FrameOperation);
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SetComboBox(cboStockLocation);
        }
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetComboBox(cboLine);
            //SetComboBox(cboStockLocation);
        }

        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }
    }
}