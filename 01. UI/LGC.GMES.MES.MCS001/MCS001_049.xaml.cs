/*************************************************************************************
 Created Date : 2020.10.15
      Creator : 서동현
   Decription : EOL 생산 정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  서동현 책임 : Initial Created.    

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
    public partial class MCS001_049 : UserControl, IWorkArea
	{
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_049()
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
				SelectEolHistory();
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
                btnSearch_Click(btnSearch, null);
            }
        }
        #endregion

        #region Mehod
        private void SelectEolHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_MCS_SEL_FORMATION_EOL_INFO";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
				inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				inDataTable.Columns.Add("AREAID", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
				inData["BLDG_CODE"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
				inData["AREAID"] = LoginInfo.CFG_AREA_ID;

				inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

					Util.GridSetData(dgEolHistory, result, FrameOperation, true);

					string[] columnName = new string[] { "AREA", "LINE" };
					_util.SetDataGridMergeExtensionCol(dgEolHistory, columnName, DataGridMergeMode.VERTICAL);
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
            Util.gridClear(dgEolHistory);
        }
		#endregion

		private void dgEolHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null) return;

				//PORT_ID
				//REQUEST_COMP_FLAG
				//e.Cell.Presenter.Background = null;

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					/*
					일단 보류
					if (string.Equals(e.Cell.Column.Name, "AREA")){
						if (e.Cell.Row.Index == 0 || DataTableConverter.GetValue(e.Cell.Row.DataItem, "AREA") == DataTableConverter.GetValue(dataGrid.GetCell(e.Cell.Row.Index - 1, e.Cell.Column.Index).Row.DataItem, "AREA"))
						{ 
							e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
						}
						else
						{
							var bgColor = new BrushConverter();
							e.Cell.Presenter.Background = (Brush)bgColor.ConvertFrom("#fff8f8f8");
						}
					}
					*/
					/*
					if (string.Equals(e.Cell.Column.Name, "LINE"))
					{
						if (e.Cell.Row.Index == 0 || DataTableConverter.GetValue(e.Cell.Row.DataItem, "LINE") == DataTableConverter.GetValue(dataGrid.GetCell(e.Cell.Row.Index - 1, e.Cell.Column.Index).Row.DataItem, "LINE"))
						{
							e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
						}
						else
						{
							var bgColor = new BrushConverter();
							e.Cell.Presenter.Background = (Brush)bgColor.ConvertFrom("#fff8f8f8");
						}
					}
					*/

					if (string.Equals(e.Cell.Column.Name, "REQUEST_COMP_FLAG") &&
						Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQUEST_COMP_FLAG")) == "Y")
					{
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					else if (string.Equals(e.Cell.Column.Name, "MES_EOL_PORT_STAT_CHG_DTTM") &&
							 Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MES_EOL_PORT_STAT")) == "LR" &&
							 Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MES_EOL_PORT_STAT_CHG_WAITTIME"))) >= 10)
					{
						var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFCD12");
						if (convertFromString != null)
							e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
					}
					else
					{
						e.Cell.Presenter.FontWeight = FontWeights.Normal;
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					}
				}
			}));
		}

		private void dgEolHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

		private void dgEolHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				Point pnt = e.GetPosition(null);
				
				C1DataGrid dg = sender as C1DataGrid;
				C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

				if (cell != null && cell.Column.Name.Equals("REQUEST_COMP_FLAG"))
				{
					string strPortId = Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "PORT_ID"));
					string strPortName = Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "PORT_NAME"));
					string strFlag = Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "REQUEST_COMP_FLAG"));

					if (strPortId == "") return;
					if (strFlag != "Y") return;

					string strMessage = string.Format("{0}[{1}]정보를 N으로 초기화 시키겠습니까?", strPortName, strPortId);

					// 선택된 반송정보를 취소하시겠습니까?
					Util.MessageConfirm(strMessage, (result) =>
					{
						if (result == MessageBoxResult.OK)
						{
							UpdatePort(strPortId);
							SelectEolHistory();
						}
					});
				}
				
				/*
				if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("REQUEST_COMP_FLAG"))
				{
					string strPortId = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PORT_ID"));
					string strPortName = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PORT_NAME"));
					string strFlag = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQUEST_COMP_FLAG"));

					if (strPortId == "") return;
					if (strFlag != "Y") return;

					string strMessage = string.Format("{0}[{1}]정보를 N으로 초기화 시키겠습니까?", strPortName, strPortId);

					// 선택된 반송정보를 취소하시겠습니까?
					Util.MessageConfirm(strMessage, (result) =>
					{
						if (result == MessageBoxResult.OK)
						{
							UpdatePort(strPortId);
							SelectEolHistory();
						}
					});
				}
				*/
			}
		}

		private void dgEolHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
		{
			try
			{
				C1DataGrid dg = dgEolHistory;

				string columnNameBase = "LINE";
				//int columnIdxBase = dg.Columns[columnNameBase].Index;

				string[] columnName = new string[] { "EQPTNAME", "EIOSTAT", "MDLLOT_ID", "PRJT_NAME", "PRODID", "CSTPROD", "QUICKSHIP_YN" };
				int[] columnIdx = new int[columnName.Length];

				for (int i = 0; i < columnName.Length; i++)
				{
					columnIdx[i] = dg.Columns[columnName[i]].Index;
				}

				int j = 0;
				if (dg.GetRowCount() > 0)
				{
					for (int i = 0; i <= dg.GetRowCount() - 1; i++)
					{
						for (j = 1; j < dg.GetRowCount() - i; j++)
						{
							if ((Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, columnNameBase))) != (Util.NVC(DataTableConverter.GetValue(dg.Rows[i + j].DataItem, columnNameBase))))
							{
								break;
							}
						}
						j--;

						for (int x = 0; x < columnName.Length; x++)
						{
							int iTemp = columnIdx[x];
							e.Merge(new C1.WPF.DataGrid.DataGridCellsRange(dg.GetCell(i, iTemp), dg.GetCell(i + j, iTemp)));
						}

						i = i + j;
					}
				}
			}
			catch (Exception ex)
			{
				Util.MessageInfo(ex.Message.ToString());
			}
		}

		private void btnQuickShipSet_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MCS001_049_QUICK_SHIP_SETTING popup = new MCS001_049_QUICK_SHIP_SETTING { FrameOperation = FrameOperation };
				popup.Closed += Popup_Closed;
				object[] parameters = new object[1];
				parameters[0] = cboArea.SelectedValue.ToString();
				C1WindowExtension.SetParameters(popup, parameters);
				Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
			}
			catch (Exception ex)
			{
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
			}
			finally
			{

			}
		}

		private void Popup_Closed(object sender, EventArgs e)
		{
			MCS001_049_QUICK_SHIP_SETTING popup = sender as MCS001_049_QUICK_SHIP_SETTING;
			if (popup != null && popup.DialogResult == MessageBoxResult.OK)
			{
				btnSearch_Click(null, null);
			}
		}

		private void btnHandOver_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MCS001_049_HAND_OVER popup = new MCS001_049_HAND_OVER { FrameOperation = FrameOperation };
				//popup.Closed += HandOverPopup_Closed;
				//object[] parameters = new object[1];
				//parameters[0] = cboArea.SelectedValue.ToString();
				//C1WindowExtension.SetParameters(popup, parameters);
				Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
			}
			catch (Exception ex)
			{
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
			}
			finally
			{

			}
		}

		private void HandOverPopup_Closed(object sender, EventArgs e)
		{
			//MCS001_049_HAND_OVER popup = sender as MCS001_049_HAND_OVER;
			//if (popup != null && popup.DialogResult == MessageBoxResult.OK)
			//{
			//	btnSearch_Click(null, null);
			//}
		}
	}
}
