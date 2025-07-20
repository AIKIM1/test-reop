/*************************************************************************************
 Created Date : 2021.04.12
      Creator : 서동현
   Decription : Pallet 수동 반송 이력
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.12  서동현 책임 : Initial Created.    

**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_061 : UserControl, IWorkArea
	{
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

		private string _bizRuleIp;
		private string _bizRuleProtocol;
		private string _bizRulePort;
		private string _bizRuleServiceMode;
		private string _bizRuleServiceIndex;

		/// <summary>
		/// Frame과 상호작용하기 위한 객체
		/// </summary>
		public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_061()
        {
            InitializeComponent();
        }

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeControls();
			InitializeCombo();
		}

        private void InitializeControls()
        {
			dtpDateFrom.SelectedDateTime = DateTime.Now.Date.AddDays(-1);
			dtpDateTo.SelectedDateTime = DateTime.Now.Date;
		}

		private void InitializeCombo()
		{
			// Area 콤보박스
			CommonCombo _combo = new CommonCombo();
			string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
			_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODEATTRS");

			// CstStat 콤보박스
			string[] sFilter2 = { "CSTSTAT" };
			_combo.SetCombo(cboCstStat, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

			// CstProd 콤보박스
			string[] sFilter3 = { "CSTPROD", "PT" };
			_combo.SetCombo(cboCstProd, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODEATTRS");

			//SetDataGridCommonCombo(dgTransferHistory.Columns["PRODID"], "CBO_CODE", CommonCombo.ComboStatus.NONE, "Y");
		}

		private void GetBizActorServerInfo()
		{
			DataTable inTable = new DataTable("RQSTDT");
			inTable.Columns.Add("KEYGROUPID", typeof(string));
			DataRow dr = inTable.NewRow();
			dr["KEYGROUPID"] = "FP_MCS_AP_CONFIG";
			inTable.Rows.Add(dr);

			DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

			if (CommonVerify.HasTableRow(dtResult))
			{
				foreach (DataRow newRow in dtResult.Rows)
				{
					if (newRow["KEYID"].ToString() == "BizActorIP")
						_bizRuleIp = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorPort")
						_bizRulePort = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorProtocol")
						_bizRuleProtocol = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
						_bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
					else
						_bizRuleServiceMode = newRow["KEYVALUE"].ToString();
				}
			}
		}
		#endregion

		#region Event
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            Loaded -= UserControl_Loaded;
        }

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				SelectTransferHistory();
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void SelectTransferHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_SEL_MCS_REQ_MANUAL_TRF_HISTORY_BY_MES_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("AREAID", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("CSTSTAT", typeof(string));
				inDataTable.Columns.Add("CSTPROD", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
				inData["AREAID"] = LoginInfo.CFG_AREA_ID;
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["CSTSTAT"] = string.IsNullOrEmpty(this.cboCstStat.SelectedValue.ToString()) ? null : this.cboCstStat.SelectedValue.ToString();
				inData["CSTPROD"] = string.IsNullOrEmpty(this.cboCstProd.SelectedValue.ToString()) ? null : this.cboCstProd.SelectedValue.ToString();
				inDataTable.Rows.Add(inData);

				new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

					if (result != null && result.DataSet != null && result.DataSet.Tables.Count > 0)
					{
                        DataTable dtProdid = result.AsDataView().ToTable(true, "PRODID");

                        // Area 콤보박스
                        //CommonCombo _combo = new CommonCombo();
                        //string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
                        //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "PRJT_NAME");
                        //
                        string strProdIdList = string.Empty;

						foreach(DataRow dr in dtProdid.Rows)
						{
							strProdIdList += dr["PRODID"].ToString() + ",";
						}

						SetDataGridCommonCombo(dgTransferHistory.Columns["PRODID"], "CBO_CODE", strProdIdList, CommonCombo.ComboStatus.NONE, "Y");
					}

					Util.GridSetData(dgTransferHistory, result, FrameOperation, true);
				});
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

		private static void SetDataGridCommonCombo(C1.WPF.DataGrid.DataGridColumn dgcol, string codeType, string prodIdList, CommonCombo.ComboStatus status, string useFlag = null)
		{
			const string bizRuleName = "DA_BAS_SEL_PRODUCT_BY_PRODID_LIST_CBO";
			string[] arrColumn = { "PRODID_LIST" };
			string[] arrCondition = { prodIdList };
			string selectedValueText = dgcol.SelectedValuePath();
			string displayMemberText = dgcol.DisplayMemberPath();
			CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
		}

		private void ClearControls()
        {
            Util.gridClear(dgTransferHistory);
        }
		#endregion

		private void dgTransferHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					//if (string.Equals(e.Cell.Column.Name, "REQUEST_COMP_FLAG") &&
					//	Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQUEST_COMP_FLAG")) == "Y")
					//{
					//	e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
					//	e.Cell.Presenter.FontWeight = FontWeights.Bold;
					//}
					//else
					//{
						e.Cell.Presenter.FontWeight = FontWeights.Normal;
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					//}
				}
			}));
		}

		private void dgTransferHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			if (e.Cell.Presenter != null)
			{
				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					e.Cell.Presenter.FontWeight = FontWeights.Normal;
					//e.Cell.Presenter.Background = null;
				}
			}
		}
	}
}