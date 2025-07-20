/*************************************************************************************
 Created Date : 2020.11.12
      Creator : 서동현
   Decription : 세정기 Buffer 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.12  서동현 책임 : Initial Created.    

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
    public partial class MCS001_052 : UserControl, IWorkArea
	{
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
		private bool _bStartSetting = false;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_052()
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
			//dtpDateFrom.SelectedDateTime = DateTime.Now.Date.AddDays(-1);
			//dtpDateTo.SelectedDateTime = DateTime.Now.Date;
		}

		private void InitializeCombo()
		{
			// Area 콤보박스
			CommonCombo _combo = new CommonCombo();
			string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
			_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");

			// Stocker 콤보박스
			SetEquipmentCombo(cboEquipment, cboArea);

			// Area 콤보박스
			CommonCombo _combo2 = new CommonCombo();
			string[] sFilter2 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
			_combo2.SetCombo(cboHistoryArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODEATTRS");

			// Stocker 콤보박스
			SetEquipmentCombo(cboHistoryEquipment, cboHistoryArea);

			SetDataGridCommonCombo(dgCleannerBuffer.Columns["CHANGE_CSTPROD"], "CSTPROD", CommonCombo.ComboStatus.NA, "Y");
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
				SelectCleannerBuffer();
				SelectCleannerPort();
			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnHistorySearch_Click(btnHistorySearch, null);
            }
        }

		private void dgCleannerBuffer_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					e.Cell.Presenter.FontWeight = FontWeights.Normal;
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
				}
			}));
		}

		private void dgCleannerBuffer_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgCleannerPort_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					e.Cell.Presenter.FontWeight = FontWeights.Normal;
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
				}
			}));
		}

		private void dgCleannerPort_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			ClearControls("Status");
		}

		private void btnBufferChange_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationBuffer()) return;

			// 변경하시겠습니까?
			Util.MessageConfirm("SFU2875", (result) =>
			{
				if (result == MessageBoxResult.OK)
				{
					this.SaveBuffer();
				}
			});
		}

		private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			ClearControls("Status");

			//SetDataGridCommonCombo2(dgCleannerBuffer.Columns["CHANGE_CSTPROD"], cboArea.SelectedValue.ToString(), CommonCombo.ComboStatus.NA, "Y");

			cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
			SetEquipmentCombo(cboEquipment, cboArea);
			cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
		}

		private void dgCleannerBuffer_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
		{
			try
			{
				/*
				if (e?.Row?.DataItem == null || e.Column == null)
					return;

				C1DataGrid dg = dgCleannerBuffer;

				DataRowView drv = e.Row.DataItem as DataRowView;

				if (DataTableConverter.GetValue(drv, "E_PLT_CNT").GetString() == "0")
				{
					e.Cancel = true;
					return;
				}
				else if (DataTableConverter.GetValue(drv, "CST_REQ_MCS_GNRT_FLAG").GetString() == "Y")
				{
					e.Cancel = true;
					return;
				}
				*/

				Util.DataGridRowEditByCheckBoxColumn(e, null);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void dgCleannerHistory_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
		{
			try
			{
				/*
				if (e?.Row?.DataItem == null || e.Column == null)
					return;

				C1DataGrid dg = dgCleannerBuffer;

				DataRowView drv = e.Row.DataItem as DataRowView;

				if (DataTableConverter.GetValue(drv, "E_PLT_CNT").GetString() == "0")
				{
					e.Cancel = true;
					return;
				}
				else if (DataTableConverter.GetValue(drv, "CST_REQ_MCS_GNRT_FLAG").GetString() == "Y")
				{
					e.Cancel = true;
					return;
				}
				*/

				Util.DataGridRowEditByCheckBoxColumn(e, null);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void dgCleannerHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					e.Cell.Presenter.FontWeight = FontWeights.Normal;
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
				}
			}));
		}

		private void dgCleannerHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void btnHistorySearch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SelectCleannerHistory();
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void cboHistoryEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			ClearControls("History");
		}

		private void cboHistoryArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			ClearControls("History");

			cboHistoryEquipment.SelectedValueChanged -= cboHistoryEquipment_SelectedValueChanged;
			SetEquipmentCombo(cboHistoryEquipment, cboHistoryArea);
			cboHistoryEquipment.SelectedValueChanged += cboHistoryEquipment_SelectedValueChanged;
		}
		private void dtpDateFrom_Loaded(object sender, RoutedEventArgs e)
		{
			// Tab이 가려질경우 초기값 세팅이 안되는 문제대응
			if (this.tabHistory.IsSelected && !_bStartSetting)
			{
				_bStartSetting = true;
				dtpDateFrom.SelectedDateTime = DateTime.Now.Date.AddDays(-1);
				dtpDateTo.SelectedDateTime = DateTime.Now.Date;
			}
		}
		#endregion

		#region Mehod
		private void SetEquipmentCombo(C1ComboBox cbo, C1ComboBox cbo1)
		{
			const string bizRuleName = "DA_MCS_SEL_FORMATION_EQPT_CBO";

			DataTable dtRQSTDT = new DataTable();
			dtRQSTDT.TableName = "RQSTDT";
			dtRQSTDT.Columns.Add("LANGID", typeof(string));
			dtRQSTDT.Columns.Add("BLDGCODE", typeof(string));
			dtRQSTDT.Columns.Add("EQGRID", typeof(string));

			DataRow drNewrow = dtRQSTDT.NewRow();
			drNewrow["LANGID"] = LoginInfo.LANGID;
			drNewrow["BLDGCODE"] = cbo1.SelectedValue.ToString();
			drNewrow["EQGRID"] = "CLN";

			dtRQSTDT.Rows.Add(drNewrow);

			new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
			{
				if (Exception != null)
				{
					Util.AlertByBiz(bizRuleName, Exception.Message, Exception.ToString());
					return;
				}

				DataTable dtTemp = new DataTable();
				dtTemp = result.Copy();
				//cbo.ItemsSource = DataTableConverter.Convert(dtTemp);

				//ComboStatus cs = ComboStatus
				cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "EQPTID", "EQPTNAME").Copy().AsDataView();
				cbo.SelectedIndex = 0;
			}
			);
		}

		private static void SetDataGridCommonCombo(C1.WPF.DataGrid.DataGridColumn dgcol, string codeType, CommonCombo.ComboStatus status, string useFlag = null)
		{
			const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1", "ATTRIBUTE2" };
			string[] arrCondition = { LoginInfo.LANGID, codeType, "PT", null };
			string selectedValueText = dgcol.SelectedValuePath();
			string displayMemberText = dgcol.DisplayMemberPath();
			CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
		}

		private static void SetDataGridCommonCombo2(C1.WPF.DataGrid.DataGridColumn dgcol, string bldgCode, CommonCombo.ComboStatus status, string useFlag = null)
		{
			//const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
			//string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1", "ATTRIBUTE2" };
			//string[] arrCondition = { LoginInfo.LANGID, codeType, "PT", null };
			//string selectedValueText = dgcol.SelectedValuePath();
			//string displayMemberText = dgcol.DisplayMemberPath();
			//CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);

			const string bizRuleName = "DA_MCS_SEL_FORMATION_CLEANNER_BUFFER_CSTPROD_COMBO";
			string[] arrColumn = { "LANGID", "BLDGCODE", };
			string[] arrCondition = { LoginInfo.LANGID, bldgCode, "PT", null };
			string selectedValueText = dgcol.SelectedValuePath();
			string displayMemberText = dgcol.DisplayMemberPath();
			CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
		}

		private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
		{
			DataRow dr = dt.NewRow();

			switch (cs)
			{
				case CommonCombo.ComboStatus.ALL:
					dr[sDisplay] = "-ALL-";
					dr[sValue] = "";
					dt.Rows.InsertAt(dr, 0);
					break;

				case CommonCombo.ComboStatus.SELECT:
					dr[sDisplay] = "-SELECT-";
					dr[sValue] = "SELECT";
					dt.Rows.InsertAt(dr, 0);
					break;

				case CommonCombo.ComboStatus.NA:
					dr[sDisplay] = "-N/A-";
					dr[sValue] = "";
					dt.Rows.InsertAt(dr, 0);
					break;
			}

			return dt;
		}

		private void SelectCleannerBuffer()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_MCS_SEL_FORMATION_CLEANNER_BUFFER_INFO";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("EQPTID", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["EQPTID"] = string.IsNullOrEmpty(this.cboEquipment.SelectedValue.ToString()) ? null : this.cboEquipment.SelectedValue.ToString();

				inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

					Util.GridSetData(dgCleannerBuffer, Util.CheckBoxColumnAddTable(result, false), FrameOperation, true);
				});
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

		private void SelectCleannerPort()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_MCS_SEL_FORMATION_CLEANNER_PORT_INFO";

				DataTable inDataTable = new DataTable("RQSTDT");
				inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("EQPTID", typeof(string));

				DataRow inData = inDataTable.NewRow();
				inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["EQPTID"] = string.IsNullOrEmpty(this.cboEquipment.SelectedValue.ToString()) ? null : this.cboEquipment.SelectedValue.ToString();

				inDataTable.Rows.Add(inData);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					Util.GridSetData(dgCleannerPort, result, FrameOperation, true);
				});
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}

		private void SelectCleannerHistory()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "DA_MCS_SEL_FORMATION_CLEANNER_HISTORY";

				DataTable inDataTable = new DataTable("RQSTDT");
				inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("EQPTID", typeof(string));
				inDataTable.Columns.Add("FROM_DT", typeof(DateTime));
				inDataTable.Columns.Add("TO_DT", typeof(DateTime));
				inDataTable.Columns.Add("CSTID", typeof(string));

				DataRow inData = inDataTable.NewRow();
				inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboHistoryArea.SelectedValue.ToString()) ? null : this.cboHistoryArea.SelectedValue.ToString();
				inData["EQPTID"] = string.IsNullOrEmpty(this.cboHistoryEquipment.SelectedValue.ToString()) ? null : this.cboHistoryEquipment.SelectedValue.ToString();
				inData["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
				inData["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
				inData["CSTID"] = string.IsNullOrEmpty(this.txtCarrierId.Text.Trim()) ? null : this.txtCarrierId.Text.Trim();

				inDataTable.Rows.Add(inData);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					Util.GridSetData(dgCleannerHistory, Util.CheckBoxColumnAddTable(result, false), FrameOperation, true);
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

		private void ClearControls(string type)
        {
			if (type.Equals("Status"))
			{
				Util.gridClear(dgCleannerBuffer);
				Util.gridClear(dgCleannerPort);
			}
			else
			{
				Util.gridClear(dgCleannerHistory);
			}
        }

		private bool ValidationBuffer()
		{
			C1DataGrid dg = dgCleannerBuffer;

			if (!CommonVerify.HasDataGridRow(dg))
			{
				Util.MessageValidation("SFU1636");
				return false;
			}

			if (_util.GetDataGridRowCountByCheck(dg, "CHK", true) < 1)
			{
				Util.MessageValidation("SFU1636");
				return false;
			}

			for (int i = 0; i < dg.Rows.Count; i++)
			{
				C1.WPF.DataGrid.DataGridRow iRow = dg.Rows[i];
				if (Util.NVC(DataTableConverter.GetValue(iRow.DataItem, "CHK")).GetString() != "True") continue;

				if (DataTableConverter.GetValue(iRow.DataItem, "REQ_CSTPROD").GetString() != "" &&
					DataTableConverter.GetValue(iRow.DataItem, "CST_REQ_MCS_GNRT_FLAG").GetString() == "Y")
				{
					// 이미 요청된 상태입니다.
					Util.MessageValidation("SFU8271", DataTableConverter.GetValue(iRow.DataItem, "PORTSEQ").GetString());
					return false;
				}
			}

			for (int i = 0; i < dg.Rows.Count; i++)
			{
				C1.WPF.DataGrid.DataGridRow iRow = dg.Rows[i];
				if (Util.NVC(DataTableConverter.GetValue(iRow.DataItem, "CHK")).GetString() != "True") continue;

				for (int j = 0; j < dg.Rows.Count; j++)
				{
					if (i == j) continue;
					
					C1.WPF.DataGrid.DataGridRow jRow = dg.Rows[j];

					string strA = string.Empty;
					string strAEqptid = string.Empty;

					string strB = string.Empty;
					string strBEqptid = string.Empty;

					strAEqptid = DataTableConverter.GetValue(iRow.DataItem, "EQPTID").GetString();
					strBEqptid = DataTableConverter.GetValue(jRow.DataItem, "EQPTID").GetString();

					if (Util.NVC(DataTableConverter.GetValue(iRow.DataItem, "CHK")).GetString() == "True")
					{
						strA = DataTableConverter.GetValue(iRow.DataItem, "CHANGE_CSTPROD").GetString();
					}
					else
					{
						strA = DataTableConverter.GetValue(iRow.DataItem, "REQ_CSTPROD").GetString();
					}

					if (Util.NVC(DataTableConverter.GetValue(jRow.DataItem, "CHK")).GetString() == "True")
					{
						strB = DataTableConverter.GetValue(jRow.DataItem, "CHANGE_CSTPROD").GetString();
					}
					else
					{
						strB = DataTableConverter.GetValue(jRow.DataItem, "REQ_CSTPROD").GetString();
					}

					if (strA == "" || strB == "") continue;

					if(strAEqptid.Equals(strBEqptid) &&
					   strA.Equals(strB))
					{
						Util.MessageValidation("SFU8270", DataTableConverter.GetValue(iRow.DataItem, "EQPTNAME") + " [" + DataTableConverter.GetValue(iRow.DataItem, "PORTSEQ").GetString() + "]");
						return false;
					}
				}
			}
			return true;
		}

		private void SaveBuffer()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;
				const string bizRuleName = "BR_MCS_REG_STK_FORMATION_CLEANNER_BUFFER";

				//DateTime dtSystem = GetSystemTime();

				DataTable inTable = new DataTable("INDATA");
				inTable.Columns.Add("PORTID", typeof(string));
				inTable.Columns.Add("CSTPROD", typeof(string));
				inTable.Columns.Add("USERID", typeof(string));
				//inTable.Columns.Add("UPDDTTM", typeof(DateTime));

				C1DataGrid dg = dgCleannerBuffer;

				foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
				{
					if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
					{
						DataRow newRow = inTable.NewRow();
						newRow["PORTID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID").GetString();
						newRow["CSTPROD"] = DataTableConverter.GetValue(row.DataItem, "CHANGE_CSTPROD").GetString();
						newRow["USERID"] = LoginInfo.USERID;
						//newRow["UPDDTTM"] = dtSystem;

						inTable.Rows.Add(newRow);
					}
				}

				new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
						SelectCleannerBuffer();
						SelectCleannerPort();
					}
					catch (Exception ex)
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
						Util.MessageException(ex);
					}
				});
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}
		#endregion
	}
}
