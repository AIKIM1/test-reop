/*************************************************************************************
 Created Date : 2021.01.04
      Creator : 서동현 책임
   Decription : Pack2동 반송 요청 현황
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.04  서동현 책임 : Initial Created.    

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
    public partial class MCS001_053 : UserControl, IWorkArea
	{
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
		private string _selectedType = string.Empty;
		private string _selectedProdId = string.Empty;
		private string _selectedTransferType = string.Empty;

		/// <summary>
		/// Frame과 상호작용하기 위한 객체
		/// </summary>
		public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_053()
        {
            InitializeComponent();
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
				ClearControl();
				SelectRequest();
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

		private void ClearControl()
		{
			_selectedType = string.Empty;
			_selectedProdId = string.Empty;
			_selectedTransferType = string.Empty;

			Util.gridClear(dgRequest);
			Util.gridClear(dgTransferRequestDetail);
			Util.gridClear(dgTransferDetail);
		}

		private void SelectRequest()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_PRD_SEL_CWA2_LOGIS_REQ_BY_ASSY3";

				DataTable inDataTable = new DataTable("RQSTDT");
				inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));

				DataRow inData = inDataTable.NewRow();
				inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);

				inDataTable.Rows.Add(inData);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					var query = result.AsEnumerable().GroupBy(x => new
					{ }).Select(g => new
					{
						Division = ObjectDic.Instance.GetObjectName("합계"),
						ReqQty = g.Sum(x => x.Field<Decimal>("REQ_QTY")),
						TransferPltQty = g.Sum(x => x.Field<Int32>("TRANSFER_PLT_QTY")),
						OnRoutePltQty = g.Sum(x => x.Field<Int32>("ON_ROUTE_PLT_QTY")),
						TransferCellQty = g.Sum(x => x.Field<Decimal>("TRANSFER_CELL_QTY")),
						OnRouteCellQty = g.Sum(x => x.Field<Decimal>("ON_ROUTE_CELL_QTY")),
						Count = g.Count()
					}).FirstOrDefault();

					if (query != null)
					{
						DataRow newRow = result.NewRow();
						newRow["DIVISION"] = query.Division;
						newRow["REQ_QTY"] = query.ReqQty;
                        newRow["TRANSFER_PLT_QTY"] = query.TransferPltQty;
						newRow["ON_ROUTE_PLT_QTY"] = query.OnRoutePltQty;
						newRow["TRANSFER_CELL_QTY"] = query.TransferCellQty;
						newRow["ON_ROUTE_CELL_QTY"] = query.OnRouteCellQty;
						result.Rows.Add(newRow);
					}

					Util.GridSetData(dgRequest, result, FrameOperation, true);
				});
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}
		#endregion

		#region Mehod
		private void SelectTransferRequestDetail()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_PRD_SEL_CWA2_LOGIS_REQ_DETAIL_BY_ASSY3";

				DataTable inDataTable = new DataTable("RQSTDT");
				inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("TYPE", typeof(string));
				inDataTable.Columns.Add("PRODID", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));
				inDataTable.Columns.Add("TRF_CST_STAT_CODE", typeof(string));

				DataRow inData = inDataTable.NewRow();
				inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["TYPE"] = _selectedType;
				inData["PRODID"] = _selectedProdId;
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
				inData["TRF_CST_STAT_CODE"] = _selectedTransferType;      // RECEIVED_LOGIS, RECEIVING_LOGIS

				inDataTable.Rows.Add(inData);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					Util.GridSetData(dgTransferRequestDetail, result, FrameOperation, true);
				});
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}

		private void SelectTransferDetail()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_PRD_SEL_CWA2_LOGIS_DETAIL_BY_ASSY3";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("TYPE", typeof(string));
				inDataTable.Columns.Add("PRODID", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));
				inDataTable.Columns.Add("TRF_CST_STAT_CODE", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["TYPE"] = _selectedType;
				inData["PRODID"] = _selectedProdId;
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
				inData["TRF_CST_STAT_CODE"] = _selectedTransferType;      // RECEIVED_LOGIS, RECEIVING_LOGIS

				inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

					Util.GridSetData(dgTransferDetail, result, FrameOperation, true);
				});
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

		private void ClearControls()
        {
			Util.gridClear(dgTransferRequestDetail);
			Util.gridClear(dgTransferDetail);
        }
		#endregion


		private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "REQ_QTY") || 
					    string.Equals(e.Cell.Column.Name, "TRANSFER_PLT_QTY") ||
						string.Equals(e.Cell.Column.Name, "ON_ROUTE_PLT_QTY") ||
						string.Equals(e.Cell.Column.Name, "TRANSFER_CELL_QTY") ||
						string.Equals(e.Cell.Column.Name, "ON_ROUTE_CELL_QTY"))
					{
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					else
					{
						e.Cell.Presenter.FontWeight = FontWeights.Normal;
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					}

					if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIVISION")), ObjectDic.Instance.GetObjectName("합계")))
					{
						var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
						if (convertFromString != null)
							e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
					}
				}
			}));
		}

		private void dgRequest_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgRequest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				C1DataGrid dg = sender as C1DataGrid;
				if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

				Point pnt = e.GetPosition(null);
				C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

				if (cell == null) return;

				int rowIdx = cell.Row.Index;
				DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
				if (drv == null) return;

				_selectedType = null;
				_selectedProdId = null;
				_selectedTransferType = null;

				if (DataTableConverter.GetValue(drv, "DIVISION").GetString() != ObjectDic.Instance.GetObjectName("합계"))
				{
					_selectedType = DataTableConverter.GetValue(drv, "DIVISION").GetString();
					_selectedProdId = DataTableConverter.GetValue(drv, "PRODID").GetString();
				}

				if (string.Equals(cell.Column.Name, "TRANSFER_PLT_QTY") ||
					string.Equals(cell.Column.Name, "TRANSFER_CELL_QTY"))
				{
					_selectedTransferType = "RECEIVED_LOGIS";
				}
				else if (string.Equals(cell.Column.Name, "ON_ROUTE_PLT_QTY") ||
					     string.Equals(cell.Column.Name, "ON_ROUTE_CELL_QTY"))
				{
					_selectedTransferType = "RECEIVING_LOGIS";
				}

				Util.gridClear(dgTransferRequestDetail);
				Util.gridClear(dgTransferDetail);

				if (string.Equals(cell.Column.Name, "REQ_QTY"))
				{
					tabItemReqList.IsSelected = true;
					SelectTransferRequestDetail();
				}
				else
				{
					tabItemDetailList.IsSelected = true;
					SelectTransferDetail();
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void dgTransferRequestDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					//if (string.Equals(e.Cell.Column.Name, "TOTAL_QTY"))
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

		private void dgTransferRequestDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgTransferDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					//if (string.Equals(e.Cell.Column.Name, "TOTAL_QTY"))
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

		private void dgTransferDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
