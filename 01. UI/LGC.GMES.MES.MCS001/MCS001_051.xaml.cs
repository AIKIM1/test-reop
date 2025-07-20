/*************************************************************************************
 Created Date : 2020.11.05
      Creator : 서동현
   Decription : OQC 검사대상 Pallet 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.05  서동현 책임 : Initial Created.    

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
    public partial class MCS001_051 : UserControl, IWorkArea
	{
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
		private string _selectedLine = string.Empty;

		/// <summary>
		/// Frame과 상호작용하기 위한 객체
		/// </summary>
		public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_051()
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
			_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");
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
				SelectOqcLine();
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

		private void ClearControl()
		{
			_selectedLine = string.Empty;

			Util.gridClear(dgLine);
			Util.gridClear(dgOqcPallet);
		}

		private void SelectOqcLine()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_MCS_SEL_FORMATION_OQC_INFO";

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
						LineName = ObjectDic.Instance.GetObjectName("합계"),
						TotalQty = g.Sum(x => x.Field<Int32>("TOTAL_QTY")),
                        OqcCmplTotalQty = g.Sum(x => x.Field<Int32>("OQC_CMPL_TOTAL_QTY")),
						Count = g.Count()
					}).FirstOrDefault();

					if (query != null)
					{
						DataRow newRow = result.NewRow();
						newRow["LINE_NAME"] = query.LineName;
						newRow["TOTAL_QTY"] = query.TotalQty;
                        newRow["OQC_CMPL_TOTAL_QTY"] = query.OqcCmplTotalQty;
						result.Rows.Add(newRow);
					}

					Util.GridSetData(dgLine, result, FrameOperation, true);
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
        private void SelectOqcList(bool bOQCCmplYN)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_MCS_SEL_FORMATION_OQC_LIST";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("EQSGID", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));
                inDataTable.Columns.Add("OQC_CMPL_FLAG", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["EQSGID"] = _selectedLine;
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
                if (bOQCCmplYN)
                    inData["OQC_CMPL_FLAG"] = "Y";

				inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

					Util.GridSetData(dgOqcPallet, result, FrameOperation, true);
				});
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

		private void UpdatePort(string portId)
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_MCS_UPD_FORMATION_EOL_CST_REQ_MCS_GNRT_FLAG";

				DataTable inDataTable = new DataTable("RQSTDT");
				inDataTable.Columns.Add("PORTID", typeof(string));
				inDataTable.Columns.Add("USERID", typeof(string));

				DataRow inData = inDataTable.NewRow();
				inData["PORTID"] = portId;
				inData["USERID"] = LoginInfo.USERID;

				inDataTable.Rows.Add(inData);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, ex) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
					//btnSearch_Click(null, null);
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
            Util.gridClear(dgOqcPallet);
        }
		#endregion

		private void dgOqcPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgOqcPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgLine_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "TOTAL_QTY") || string.Equals(e.Cell.Column.Name, "OQC_CMPL_TOTAL_QTY"))
					{
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					else
					{
						e.Cell.Presenter.FontWeight = FontWeights.Normal;
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					}

					if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LINE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
					{
						var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
						if (convertFromString != null)
							e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
					}
				}
			}));
		}

		private void dgLine_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgLine_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

				_selectedLine = null;

				if (DataTableConverter.GetValue(drv, "LINE_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
				{
					_selectedLine = DataTableConverter.GetValue(drv, "LINE").GetString();
				}

                bool bOQCCmplYN = Util.NVC(cell.Column.Name).Equals("OQC_CMPL_TOTAL_QTY") ? true : false;

				Util.gridClear(dgOqcPallet);
				SelectOqcList(bOQCCmplYN);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
	}
}
