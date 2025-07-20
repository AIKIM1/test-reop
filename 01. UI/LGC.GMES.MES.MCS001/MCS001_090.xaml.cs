/*************************************************************************************
 Created Date : 2023.12.13
      Creator : 서동현
   Decription : Carrier 현황 조회(Form)
--------------------------------------------------------------------------------------
 [Change History]
  2012.12.13  서동현 책임 : Initial Created.
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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001;


namespace LGC.GMES.MES.MCS001
{
	/// <summary>
	/// MCS001_090.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MCS001_090 : UserControl, IWorkArea
	{
		#region Declaration & Constructor 

		public IFrameOperation FrameOperation
		{
			get;
			set;
		}

		private readonly Util _util = new Util();
		private string _selectedCarrierTypeCode;
		private string _selectedCarrierStateCode;
		private string _selectedCarrierProductCode;
		private string _selectedAreaCode;

        private string _ClearChk = "Y"; // csttype의 이벤트에 따라 초기화가 되지만 cstid, lotid로 조회시에는 초기화가 되지않아야 됨 그래서 구분자 변수지정

		public MCS001_090()
		{
			InitializeComponent();
		}
		#endregion

		#region Event

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			List<Button> listAuth = new List<Button> { btnSearch, btnSave };
			Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

			InitializeCombo();

			this.rdoDefect.IsChecked = true;
			this.grdMapping.Visibility = Visibility.Collapsed;

			Loaded -= UserControl_Loaded;
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		private void cboCarrierType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			// Carrier Prod 콤보박스
			SetCarrierProdCombo(cboCarrierProd, cboCarrierType);

			// Carrier Summary Grid 설정
			SetGridSummaryColumnChange();

			// Carrier 상세 정보 BOBBIN_ID 칼럼 명칭 설정
			SetGridDetailColumnText();
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			ClearControl();

			this.grdMapping.Visibility = Visibility.Collapsed;

			if (TabItemCarrierInfo.IsSelected)
			{
				SetDataGridCheckHeaderInitialize(dgCarrierDetail);

				if (!string.IsNullOrEmpty(txtCarrierId.Text)|| !string.IsNullOrEmpty(txtCarrierId2.Text))
				{
					SelectCarrierDetailList(true, false);
				}
				else if (!string.IsNullOrEmpty(txtLotId.Text))
				{
					SelectCarrierDetailList(false, true);
				}
				else
				{
					if (!ValidationSearch()) return;

					SelecCarrierSummary();
				}
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSaveCarrierState()) return;
			SaveCarrierState();
		}

		private void dgCarrierSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null)
				{
					return;
				}

				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					//if (Convert.ToString(e.Cell.Column.Name) == "CNT")
					//{
					//    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CNT").GetInt() > 0)
					//    {
					//        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
					//        e.Cell.Presenter.FontWeight = FontWeights.Bold;
					//    }
					//    else
					//    {
					//        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					//    }
					//}
					if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD_NAME")), ObjectDic.Instance.GetObjectName("합계")))
					{
						var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
						if (convertFromString != null)
							e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
					}
					else
					{
						var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
						if (convertFromString != null)
							e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
					}
				}
			}));
		}

		private void dgCarrierSummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null) return;

			C1DataGrid dataGrid = sender as C1DataGrid;
			if (e.Cell.Presenter != null)
			{
				if (e.Cell.Row.Type == DataGridRowType.Item)
				{
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
					e.Cell.Presenter.FontWeight = FontWeights.Normal;
					e.Cell.Presenter.Background = null;
				}
			}
		}

		private void dgCarrierSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (sender == null) return;
				C1DataGrid dataGrid = sender as C1DataGrid;

				if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

				Point pnt = e.GetPosition(null);
				C1.WPF.DataGrid.DataGridCell cell = dgCarrierSummary.GetCellFromPoint(pnt);

				if (cell == null) return;

				// 선택한 셀의 위치
				int rowIdx = cell.Row.Index;
				DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
				if (drv == null) return;

				if (cell.Column.Name == "CSTPROD_NAME" || cell.Column.Name == "T_CNT")
				{
					_selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
					_selectedCarrierProductCode = null;
					_selectedCarrierStateCode = null;

					if (DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString() != ObjectDic.Instance.GetObjectName("합계"))
					{
						_selectedCarrierProductCode = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
					}
				}
				else
				{
					if (cell.Column.Name == "U_CNT")
					{
						_selectedCarrierStateCode = "U";
					}
					else if (cell.Column.Name == "E_CNT")
					{
						_selectedCarrierStateCode = "E";
					}
					else if (cell.Column.Name == "ET_CNT")
					{
						_selectedCarrierStateCode = "T";
					}
					else if (cell.Column.Name == "D_CNT")
					{
						_selectedCarrierStateCode = "D";
					}
					else if (cell.Column.Name == "ABNORM_CNT")
					{
						_selectedCarrierStateCode = "AB";
					}
					else if (cell.Column.Name == "UN_CNT")
					{
						_selectedCarrierStateCode = "UN";
					}
					else
						_selectedCarrierStateCode = null;

					_selectedCarrierTypeCode = DataTableConverter.GetValue(drv, "CSTTYPE").GetString();
					_selectedCarrierProductCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "CSTPROD").GetString()) ? null : DataTableConverter.GetValue(drv, "CSTPROD").GetString();
				}

				SelectCarrierDetailList(false, false);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void dgCarrierDetail_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
		{

		}

		private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
		{
			C1DataGrid dg = dgCarrierDetail;

			int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");

			foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
			{
				if (idx > -1)
				{
					string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_CUR").GetString();
					if (DataTableConverter.GetValue(row.DataItem, "EQPT_CUR").GetString() == selectedequipmentCode)
					{
						DataTableConverter.SetValue(row.DataItem, "CHK", 1);
					}
					else
					{
						DataTableConverter.SetValue(row.DataItem, "CHK", 0);
					}
				}
				else
				{
					DataTableConverter.SetValue(row.DataItem, "CHK", 1);
				}
			}
			dg.EndEdit();
			dg.EndEditRow(true);
		}

		private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
		{
			//Util.DataGridCheckAllUnChecked(dgCarrierDetail);
			C1DataGrid dg = dgCarrierDetail;

			foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
			{
				DataTableConverter.SetValue(row.DataItem, "CHK", 0);
			}

			dg.EndEdit();
			dg.EndEditRow(true);
		}

		private void txtCarrierId_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (string.IsNullOrEmpty(txtCarrierId.Text))
			{
				cboAreaGroup.IsEnabled = true;
				cboCarrierType.IsEnabled = true;
				txtLotId.IsEnabled = true;
                _ClearChk = "Y";
            }
			else
			{
				cboAreaGroup.IsEnabled = false;
				cboCarrierType.IsEnabled = false;
				txtLotId.IsEnabled = false;
			}
		}

		private void cboCarrierState_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			//상태가 불량인 경우 불량사유 콤보박스, 비고 텍스트박스 활성화
			if (cboCarrierState.SelectedValue?.GetString() == "Y")
			{
				cboDefectReason.IsEnabled = true;
				cboDefectReason.SelectedIndex = 0;
				txtNote.IsReadOnly = false;
			}
			else
			{
				cboDefectReason.IsEnabled = false;
				cboDefectReason.SelectedIndex = -1;
				txtNote.IsReadOnly = true;
				txtNote.Text = string.Empty;
			}
		}

		private void cboAreaGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			SetAreaCombo(cboArea);
		}

		private void btnAbnormalSave_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSaveAbnormalState()) return;
			SaveAbnormal();
		}

		private void btnScrapSave_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSaveScrap()) return;
			SaveScrap();
		}

		private void cboAbnormalState_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			//상태가 비정상인 경우 비정상사유 콤보박스 활성화
			if (cboAbnormalState.SelectedValue?.GetString() == "Y")
			{
				cboAbnormalReason.IsEnabled = true;
				cboAbnormalReason.SelectedIndex = 0;
			}
			else
			{
				cboAbnormalReason.IsEnabled = false;
				cboAbnormalReason.SelectedIndex = -1;
			}
		}

		private void rdoSaveType_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton radioButton = sender as RadioButton;
			if (radioButton == null) return;

			BottomDefectArea.Visibility = Visibility.Collapsed;
			BottomAbnormalArea.Visibility = Visibility.Collapsed;
			BottomScrapArea.Visibility = Visibility.Collapsed;

			switch (radioButton.Name)
			{
				case "rdoDefect":
					BottomDefectArea.Visibility = Visibility.Visible;
					break;
				case "rdoAbnormal":
					BottomAbnormalArea.Visibility = Visibility.Visible;
					break;
				case "rdoScrap":
					BottomScrapArea.Visibility = Visibility.Visible;
					break;
				default:
					break;
			}
		}
		#endregion

		#region Method
		private void SelecCarrierSummary()
		{
			//const string bizRuleName = "DA_MCS_SEL_CARRIER_AREA_SUMMARY";
			const string bizRuleName = "BR_MCS_GET_BOBBIN_SUMMARY_INFO_FORM";
			try
			{
				ShowLoadingIndicator();
				DataTable inTable = new DataTable("INDATA");
				inTable.Columns.Add("LANGID", typeof(string));
				inTable.Columns.Add("CSTTYPE", typeof(string));
				inTable.Columns.Add("CSTPROD", typeof(string));
				inTable.Columns.Add("AREAID", typeof(string));
				inTable.Columns.Add("GR_AREAID", typeof(string));
				inTable.Columns.Add("CSTOWNER", typeof(string));

				DataRow dr = inTable.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["CSTTYPE"] = cboCarrierType.SelectedValue;
				dr["CSTPROD"] = cboCarrierProd.SelectedValue;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["GR_AREAID"] = cboAreaGroup.SelectedValue;
                dr["CSTOWNER"] = null;

				inTable.Rows.Add(dr);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
				{
					HiddenLoadingIndicator();
					if (bizException != null)
					{
						Util.MessageException(bizException);
						return;
					}

					var query = bizResult.AsEnumerable().GroupBy(x => new
					{ }).Select(g => new
					{
						CarrierTypeCode = cboCarrierType.SelectedValue,
						CarrierProductCode = string.Empty,
						CarrierProductName = ObjectDic.Instance.GetObjectName("합계"),
						CarrierTotalCount = g.Sum(x => x.Field<Int32>("T_CNT")),
						CarrierUseCount = g.Sum(x => x.Field<Int32>("U_CNT")),
						CarrierEmptyCount = g.Sum(x => x.Field<Int32>("E_CNT")),
						CarrierEmptyTrayCount = g.Sum(x => x.Field<Int32>("ET_CNT")),
						CarrierDefectCount = g.Sum(x => x.Field<Int32>("D_CNT")),
						CarrierAbnormalCount = g.Sum(x => x.Field<Int32>("ABNORM_CNT")),
						CarrierNonUseCount = g.Sum(x => x.Field<Int32>("UN_CNT"))
					}).FirstOrDefault();

					if (query != null)
					{
						DataRow newRow = bizResult.NewRow();
						newRow["CSTTYPE"] = query.CarrierTypeCode;
						newRow["CSTPROD"] = query.CarrierProductCode;
						newRow["CSTPROD_NAME"] = query.CarrierProductName;
						newRow["T_CNT"] = query.CarrierTotalCount;
						newRow["U_CNT"] = query.CarrierUseCount;
						newRow["E_CNT"] = query.CarrierEmptyCount;
						newRow["ET_CNT"] = query.CarrierEmptyTrayCount;
						newRow["D_CNT"] = query.CarrierDefectCount;
						newRow["ABNORM_CNT"] = query.CarrierAbnormalCount;
						newRow["UN_CNT"] = query.CarrierNonUseCount;
						bizResult.Rows.Add(newRow);
					}

					Util.GridSetData(dgCarrierSummary, bizResult, null, true);
				});
			}
			catch (Exception ex)
			{
				HiddenLoadingIndicator();
				Util.MessageException(ex);
			}
		}

		private void SelectCarrierDetailList(bool isCarrierIdOnly, bool isLotOnly)
		{
			SetDataGridCheckHeaderInitialize(dgCarrierDetail);
			//const string bizRuleName = "DA_MCS_SEL_CARRIER_AREA_SUMMARY_LIST";
			const string bizRuleName = "BR_MCS_GET_BOBBIN_INFO_FORM";
			try
			{
				ShowLoadingIndicator();
				DataTable inTable = new DataTable("RQSTDT");
				inTable.Columns.Add("LANGID", typeof(string));
				inTable.Columns.Add("CSTTYPE", typeof(string));
				inTable.Columns.Add("CSTPROD", typeof(string));
				//inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("CSTID_LIST", typeof(string));
				inTable.Columns.Add("AREAID", typeof(string));
				inTable.Columns.Add("GR_AREAID", typeof(string));
				inTable.Columns.Add("CSTSTAT", typeof(string));
				inTable.Columns.Add("CSTOWNER", typeof(string));
				//inTable.Columns.Add("LOTID", typeof(string));
				inTable.Columns.Add("LOTID_LIST", typeof(string));

				DataRow dr = inTable.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["GR_AREAID"] = cboAreaGroup.SelectedValue;
				dr["CSTOWNER"] = cboArea.SelectedValue;

				if (isCarrierIdOnly)
				{
                    dr["CSTID_LIST"] = !string.IsNullOrEmpty(txtCarrierId.Text.Trim()) ? ConvertData(txtCarrierId.Text.Trim()) : null;
                }
				else if (isLotOnly)
				{
                    dr["LOTID_LIST"] = !string.IsNullOrEmpty(txtLotId.Text.Trim()) ? ConvertData(txtLotId.Text.Trim()) : null;
                    dr["CSTTYPE"] = cboCarrierType.SelectedValue.ToString();
                }
				else
				{
					dr["CSTTYPE"] = _selectedCarrierTypeCode;
					dr["CSTPROD"] = _selectedCarrierProductCode;
					dr["CSTSTAT"] = _selectedCarrierStateCode;
					//dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
				}
				inTable.Rows.Add(dr);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
				{
					HiddenLoadingIndicator();
					if (bizException != null)
					{
						Util.MessageException(bizException);
						return;
					}

					Util.GridSetData(dgCarrierDetail, bizResult, null, true);
                    _ClearChk = "Y";
                    if (isCarrierIdOnly || isLotOnly)
					{
						if (dgCarrierDetail.Rows.Count > 2)
						{
							if(bizResult.DefaultView.ToTable(true, "CSTTYPE").Rows.Count == 1)
							{
								string strCstType = DataTableConverter.GetValue(dgCarrierDetail.Rows[2].DataItem, "CSTTYPE").GetString();

                                _ClearChk = "N";
                                this.cboCarrierType.SelectedValue = strCstType;

								if (cboCarrierType.SelectedValue.ToString() == "PT")
								{
									// 데이터가 1개면
									if (dgCarrierDetail.Rows.Count == 3)
									{
										grdMapping.Visibility = Visibility.Visible;

										string strCstStat = DataTableConverter.GetValue(dgCarrierDetail.Rows[2].DataItem, "CSTSTAT").GetString();

										if (strCstStat.Equals("U"))
										{
											this.btnMapping.Visibility = Visibility.Collapsed;
											this.lblPalletId.Visibility = Visibility.Collapsed;
											this.txtPalletId.Visibility = Visibility.Collapsed;
											this.btnUnmapping.Visibility = Visibility.Visible;
										}
										else
										{
											this.btnMapping.Visibility = Visibility.Visible;
											this.lblPalletId.Visibility = Visibility.Visible;
											this.txtPalletId.Visibility = Visibility.Visible;
											this.btnUnmapping.Visibility = Visibility.Collapsed;
										}
									}
									else
									{
										grdMapping.Visibility = Visibility.Collapsed;
									}
								}
								else
								{
									rdoDefect.IsChecked = true;
									grdMapping.Visibility = Visibility.Collapsed;
								}
							}
							else
							{
								this.cboCarrierType.SelectedValue = "SELECT";

								grdMapping.Visibility = Visibility.Collapsed;
							}
						}
						else
						{
							this.cboCarrierType.SelectedValue = "SELECT";
						}
					}
                });
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void SelectCarrierStateList(C1DataGrid dg)
		{
			const string bizRuleName = "DA_MCS_SEL_CARRIER_AREA_SUMMARY_LIST";
			try
			{
				ShowLoadingIndicator();
				DataTable inTable = new DataTable("RQSTDT");
				inTable.Columns.Add("LANGID", typeof(string));
				inTable.Columns.Add("AREAID", typeof(string));
				inTable.Columns.Add("CSTTYPE", typeof(string));
				inTable.Columns.Add("CSTPROD", typeof(string));
				inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("CSTOWNER", typeof(string));
				inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));

				DataRow dr = inTable.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				//dr["AREAID"] = cboAreaGroup.SelectedValue;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["CSTTYPE"] = string.Equals(dg.Name, "dgCarrierChange") ? cboCarrierType.SelectedValue : "SD";
				dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
				dr["CST_DFCT_FLAG"] = string.Equals(dg.Name, "dgCarrierChange") ? "Y" : null;
				inTable.Rows.Add(dr);

				new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
				{
					HiddenLoadingIndicator();
					if (bizException != null)
					{
						Util.MessageException(bizException);
						return;
					}

					Util.GridSetData(dg, bizResult, null, true);
				});
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void SaveCarrierState()
		{
			try
			{
				ShowLoadingIndicator();
				const string bizRuleName = "BR_MCS_REG_CARRIER_DEFECT";

				DataSet ds = new DataSet();
				DataTable inTable = ds.Tables.Add("INDATA");
				inTable.Columns.Add("SRCTYPE", typeof(string));
				inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));
				inTable.Columns.Add("CST_DFCT_RESNCODE", typeof(string));
				inTable.Columns.Add("NOTE", typeof(string));
				inTable.Columns.Add("USERID", typeof(string));

				DataRow newRow = inTable.NewRow();
				newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
				newRow["CST_DFCT_FLAG"] = cboCarrierState.SelectedValue;

				if (cboCarrierState.SelectedValue?.GetString() == "Y")
				{
					newRow["CST_DFCT_RESNCODE"] = cboDefectReason.SelectedValue;
					newRow["NOTE"] = !string.IsNullOrEmpty(txtNote.Text) ? txtNote.Text : null;
				}

				newRow["USERID"] = LoginInfo.USERID;
				inTable.Rows.Add(newRow);

				DataTable inCarrierTable = ds.Tables.Add("INCST");
				inCarrierTable.Columns.Add("CSTID", typeof(string));

				//foreach (C1.WPF.DataGrid.DataGridRow row in dgCarrierDetail.Rows)
				//{
				//    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
				//    {
				//        DataRow dr = inCarrierTable.NewRow();
				//        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
				//        inCarrierTable.Rows.Add(dr);
				//    }
				//}

				DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
				foreach (DataRow row in drSelect)
				{
					DataRow dr = inCarrierTable.NewRow();
					dr["CSTID"] = row["BOBBIN_ID"];
					inCarrierTable.Rows.Add(dr);
				}

				new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCST", null, (bizResult, bizException) =>
				{
					try
					{
						HiddenLoadingIndicator();

						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");    //정상처리되었습니다.
						SelectCarrierDetailList(false, false);
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

		private void InitializeCombo()
		{
			// 동그룹 콤보박스
			SetAreaGroupCombo(cboAreaGroup);

			// 동 콤보박스
			SetAreaCombo(cboArea);

			// Carrier Type 콤보박스
			SetCarrierTypeCombo(cboCarrierType);

			// Carrier Prod 콤보박스
			SetCarrierProdCombo(cboCarrierProd, cboCarrierType);

			// Carrier 상태 콤보박스
			SetCarrierStateCombo(cboCarrierState);

            // 불량 사유 
            SetDefectReasonCombo(cboDefectReason);

            // 비정상 상태 콤보박스
            SetAbnomalStateCombo(cboAbnormalState);

			// 비정상 사유 코드 콤보박스
			SetAbnomalReasonCombo(cboAbnormalReason);

			// 폐기 상태 콤보박스
			SetScrapStateCombo(cboScrapState);

			cboCarrierType.SelectedItemChanged += cboCarrierType_SelectedItemChanged;
		}

		private void ClearControl()
		{
			_selectedCarrierTypeCode = string.Empty;
			_selectedCarrierProductCode = string.Empty;
			_selectedCarrierStateCode = string.Empty;

            Util.gridClear(dgCarrierSummary);
			Util.gridClear(dgCarrierDetail);
		}

		private bool ValidationSaveCarrierState()
		{
			C1DataGrid dg = dgCarrierDetail;

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

			if (cboCarrierState.SelectedIndex < 0 || cboCarrierState.SelectedValue == null)
			{
				Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
				return false;
			}

			return true;
		}

		private bool ValidationSaveMapping()
		{
			C1DataGrid dg = dgCarrierDetail;

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

			if (string.IsNullOrEmpty(txtPalletId.Text.Trim()))
			{
				Util.MessageValidation("10013", (result) =>// Pallet ID를 입력하세요.
				{
					if (result == MessageBoxResult.OK)
					{
						txtPalletId.SelectAll();
						txtPalletId.Focus();
					}
				}, this.lblPalletId.Text);

				return false;
			}

			return true;
		}

		private bool ValidationSaveUnmapping()
		{
			C1DataGrid dg = dgCarrierDetail;

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

			return true;
		}

		private static void SetAreaGroupCombo(C1ComboBox cbo)
		{
			//const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
			//string[] arrColumn = { "LANGID", "SHOPID", "SYSTEM_ID", "USERID" };
			//string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.SYSID, LoginInfo.USERID };
			const string bizRuleName = "DA_MCS_SEL_LOGIS_AREA_GR_CODE_CBO";
			string[] arrColumn = { "LANGID", "AREAID" };
			string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
		}

		private void SetAreaCombo(C1ComboBox cbo)
		{
			string areaGroup = string.IsNullOrEmpty(cboAreaGroup.SelectedValue.GetString()) ? null : cboAreaGroup.SelectedValue.GetString();

			const string bizRuleName = "DA_BAS_SEL_COMMONCODE_BY_ATTR1";
			string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
			string[] arrCondition = { LoginInfo.LANGID, "LOGIS_TRF_SECTION_CODE", areaGroup };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
		}

		private  void SetCarrierTypeCombo(C1ComboBox cbo)
		{
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };
            string[] arrCondition = { LoginInfo.LANGID, "CSTTYPE", null, null, null, null, "CWA3-FORM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

		private static void SetCarrierProdCombo(C1ComboBox cbo, C1ComboBox cboParent)
		{
			if (cboParent.SelectedValue == null) return;

			const string bizRuleName = "DA_MCS_SEL_CARRIER_PROD_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
			string[] arrCondition = { LoginInfo.LANGID, "CSTPROD", cboParent.SelectedValue.ToString() };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
		}

		private static void SetCarrierStateCombo(C1ComboBox cbo)
		{
			const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE" };
			string[] arrCondition = { LoginInfo.LANGID, "CST_DFCT_FLAG" };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
		}

        private void SetDefectReasonCombo(C1ComboBox cbo)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("RESNGRID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "DEFECT_CST";
                dr["RESNGRID"] = string.Empty;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITIREASON_RESNGRID_CBO", "RQSTDT", "RSLTDT", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    cbo.DisplayMemberPath = "CBO_NAME";
                    cbo.SelectedValuePath = "CBO_CODE";
                    cbo.ItemsSource = dtResult.Copy().AsDataView();
                    cbo.SelectedIndex = 0;
                }
                //(cboIssuePort.SelectedItem as C1ComboBoxItem).Tag.GetString()
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static void SetAbnomalStateCombo(C1ComboBox cbo)
		{
			const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE" };
			string[] arrCondition = { LoginInfo.LANGID, "CST_ABNORM_FLAG" };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
		}

		private static void SetAbnomalReasonCombo(C1ComboBox cbo)
		{
			const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE" };
			string[] arrCondition = { LoginInfo.LANGID, "ABNORM_TRF_RSN_CODE" };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
		}

		private static void SetScrapStateCombo(C1ComboBox cbo)
		{
			const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
			string[] arrColumn = { "LANGID", "CMCDTYPE" };
			string[] arrCondition = { LoginInfo.LANGID, "CST_SCRAP_FLAG" };
			string selectedValueText = cbo.SelectedValuePath;
			string displayMemberText = cbo.DisplayMemberPath;

			CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
		}

		private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
		{
			C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
			StackPanel allPanel = allColumn?.Header as StackPanel;
			CheckBox allCheck = allPanel?.Children[0] as CheckBox;
			if (allCheck?.IsChecked == true)
			{
				allCheck.Unchecked -= chkHeaderAll_Unchecked;
				allCheck.IsChecked = false;
				allCheck.Unchecked += chkHeaderAll_Unchecked;
			}
		}

		private static void DoEvents()
		{
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
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

		private bool ValidationSearch()
		{
			if (TabItemCarrierInfo.IsSelected)
			{
				if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT")
				{
					// Carrier Type을 선택하세요.
					Util.MessageValidation("SFU4523");
					return false;
				}
			}

			return true;
		}

		private void SetGridSummaryColumnChange()
		{
			if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT") return;

			dgCarrierSummary.Columns["ET_CNT"].Visibility = Visibility.Collapsed;
			dgCarrierSummary.Columns["ABNORM_CNT"].Visibility = Visibility.Collapsed;

			switch (cboCarrierType.SelectedValue.ToString())
			{
				case "PT":
					dgCarrierSummary.Columns["ET_CNT"].Visibility = Visibility.Visible;
					dgCarrierSummary.Columns["ABNORM_CNT"].Visibility = Visibility.Visible;
					break;
                default:
                    break;
			}
		}

		private void SetGridDetailColumnText()
		{
			if (cboCarrierType.SelectedValue == null || cboCarrierType.SelectedValue.ToString() == "SELECT") return;

			dgCarrierDetail.Columns["CARRIERID"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["BOBBIN_ID"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["CST_CLEAN_NAME"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["PET_WINDING_CMPL_NAME"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["CURR_LOCID"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["CURR_LOC_NAME"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["CST_DFCT_FLAG_NAME"].Visibility = Visibility.Visible;
            dgCarrierDetail.Columns["CSTSTAT_NAME"].Visibility = Visibility.Visible;

            btnSave.Visibility = Visibility.Visible;
            txtNote.IsReadOnly = false;
            txtCarrier.Visibility = Visibility.Visible;
            txtCarrier2.Visibility = Visibility.Collapsed;
            txtCarrierId.Visibility = Visibility.Visible;
            txtCarrierId2.Visibility = Visibility.Collapsed;
            //this.cboDefectReason.IsDropDownOpen = true;
            //this.cboCarrierState.IsDropDownOpen = true;

            switch (cboCarrierType.SelectedValue.ToString())
			{
				case "PT":
					dgCarrierDetail.Columns["CARRIERID"].Visibility = Visibility.Collapsed;
					break;
                default:
                    break;
            }
        }

		private bool ValidationSaveAbnormalState()
		{
			C1DataGrid dg = dgCarrierDetail;

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

			if (cboAbnormalState.SelectedIndex < 0 || cboAbnormalState.SelectedValue == null)
			{
				Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
				return false;
			}

			foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
			{
				if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
				{
					//DataRow newRow = inTable.NewRow();
					//newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
					//newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
					//newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
					//newRow["DST_EQPTID"] = strOutPortTagList[0];
					//newRow["DST_LOCID"] = strOutPortTagList[1];
					//newRow["USER"] = LoginInfo.USERID;
					//newRow["DTTM"] = dtSystem;
					//newRow["PRODID"] = IsProdIdNull ? null : DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();
					//newRow["CARRIER_STRUCT"] = IsCstStatNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
					//newRow["MDL_TP"] = IsTrayTypeNull ? null : DataTableConverter.GetValue(row.DataItem, "CSTPROD").GetString();
					//inTable.Rows.Add(newRow);
				}
			}

			return true;
		}

		private void SaveAbnormal()
		{
			try
			{
				ShowLoadingIndicator();

				if (cboAbnormalState.SelectedValue.ToString() == "N")
				{
					//정상일경우
					const string bizRuleName = "BR_MCS_REG_CARRIER_ABNORM_RELEASE_LIST";

					DataTable inTable = new DataTable("INDATA");
					inTable.Columns.Add("SRCTYPE", typeof(string));
					inTable.Columns.Add("EQPTID", typeof(string));
					inTable.Columns.Add("CSTID", typeof(string));
					inTable.Columns.Add("USERID", typeof(string));

					DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
					foreach (DataRow row in drSelect)
					{
						DataRow newRow = inTable.NewRow();
						newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
						newRow["EQPTID"] = null;
						newRow["CSTID"] = row["BOBBIN_ID"];
						newRow["USERID"] = LoginInfo.USERID;
						inTable.Rows.Add(newRow);
					}

					new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
					{
						try
						{
							HiddenLoadingIndicator();

							if (bizException != null)
							{
								Util.MessageException(bizException);
								return;
							}

							Util.MessageInfo("SFU1275");    //정상처리되었습니다.
						SelectCarrierDetailList(false, false);
						}
						catch (Exception ex)
						{
							HiddenLoadingIndicator();
							Util.MessageException(ex);
						}
					});
				}
				else
				{
					//비정상일 경우
					const string bizRuleName = "BR_MCS_REG_CARRIER_ABNORM_LIST";

					DataTable inTable = new DataTable("INDATA");
					inTable.Columns.Add("SRCTYPE", typeof(string));
					inTable.Columns.Add("EQPTID", typeof(string));
					inTable.Columns.Add("CSTID", typeof(string));
					inTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
					inTable.Columns.Add("USERID", typeof(string));

					DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
					foreach (DataRow row in drSelect)
					{
						DataRow newRow = inTable.NewRow();
						newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
						newRow["EQPTID"] = null;
						newRow["CSTID"] = row["BOBBIN_ID"];
						newRow["ABNORM_TRF_RSN_CODE"] = (cboAbnormalState.SelectedValue.ToString() == "N") ? "" : cboAbnormalReason.SelectedValue.ToString();
						newRow["USERID"] = LoginInfo.USERID;
						inTable.Rows.Add(newRow);
					}

					new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
					{
						try
						{
							HiddenLoadingIndicator();

							if (bizException != null)
							{
								Util.MessageException(bizException);
								return;
							}

							Util.MessageInfo("SFU1275");    //정상처리되었습니다.
							SelectCarrierDetailList(false, false);
						}
						catch (Exception ex)
						{
							HiddenLoadingIndicator();
							Util.MessageException(ex);
						}
					});
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private bool ValidationSaveScrap()
		{
			C1DataGrid dg = dgCarrierDetail;

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

			if (cboScrapState.SelectedIndex < 0 || cboScrapState.SelectedValue == null)
			{
				Util.MessageValidation("MCS0005", ObjectDic.Instance.GetObjectName("상태"));
				return false;
			}

			return true;
		}

		private void SaveScrap()
		{
			try
			{
				ShowLoadingIndicator();
				const string bizRuleName = "BR_MCS_REG_CARRIER_SCRAP_LIST";

				DataTable inTable = new DataTable("INDATA");
				inTable.Columns.Add("SRCTYPE", typeof(string));
				inTable.Columns.Add("EQPTID", typeof(string));
				inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
				inTable.Columns.Add("USERID", typeof(string));

				DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
				foreach (DataRow row in drSelect)
				{
					DataRow newRow = inTable.NewRow();
					newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
					newRow["EQPTID"] = null;
					newRow["CSTID"] = row["BOBBIN_ID"];
					newRow["CST_MNGT_STAT_CODE"] = (cboScrapState.SelectedValue.ToString() == "N") ? "I" : "S";
					newRow["USERID"] = LoginInfo.USERID;
					inTable.Rows.Add(newRow);
				}

				new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
				{
					try
					{
						HiddenLoadingIndicator();

						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");    //정상처리되었습니다.
						SelectCarrierDetailList(false, false);
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

		private void SaveMapping()
		{
			try
			{
				ShowLoadingIndicator();
				const string bizRuleName = "BR_PRD_REG_MAPPING_CSTID_PLTID";

				DataTable inTable = new DataTable("INDATA");
				inTable.Columns.Add("USERID", typeof(string));
				inTable.Columns.Add("SRCTYPE", typeof(string));
				inTable.Columns.Add("TAGID", typeof(string));
				inTable.Columns.Add("BOXID", typeof(string));

				DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
				foreach (DataRow row in drSelect)
				{
					DataRow newRow = inTable.NewRow();
					newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
					newRow["TAGID"] = row["BOBBIN_ID"];
					newRow["USERID"] = LoginInfo.USERID;
					newRow["BOXID"] = txtPalletId.Text.Trim();

					inTable.Rows.Add(newRow);
				}

				new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
				{
					try
					{
						HiddenLoadingIndicator();

						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");    //정상처리되었습니다.
						SelectCarrierDetailList(false, false);
						this.txtPalletId.Text = string.Empty;
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

		private void SaveUnmapping()
		{
			try
			{
				ShowLoadingIndicator();
				const string bizRuleName = "BR_PRD_REG_UNMAPPING_CSTID_PLTID";

				DataTable inTable = new DataTable("INDATA");
				inTable.Columns.Add("USERID", typeof(string));
				inTable.Columns.Add("SRCTYPE", typeof(string));
				inTable.Columns.Add("TAGID", typeof(string));

				DataRow[] drSelect = DataTableConverter.Convert(dgCarrierDetail.ItemsSource).Select("CHK = 1");
				foreach (DataRow row in drSelect)
				{
					DataRow newRow = inTable.NewRow();
					newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
					newRow["TAGID"] = row["BOBBIN_ID"];
					newRow["USERID"] = LoginInfo.USERID;
					inTable.Rows.Add(newRow);
				}

				new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
				{
					try
					{
						HiddenLoadingIndicator();

						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");    //정상처리되었습니다.
						SelectCarrierDetailList(false, false);
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
		#endregion

		private void txtLotId_PreviewKeyUp(object sender, KeyEventArgs e)
		{
            if (string.IsNullOrEmpty(txtLotId.Text))
            {
                cboAreaGroup.IsEnabled = true;
                cboCarrierType.IsEnabled = true;
                txtCarrierId.IsEnabled = true;
            }
            else
            {
                cboAreaGroup.IsEnabled = false;
                cboCarrierType.IsEnabled = false;
                txtCarrierId.IsEnabled = false;
            }
		}

		private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				this.btnSearch_Click(null, null);
			}
		}

		private void txtCarrierId_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				try
				{
					string[] stringSeparators = new string[] { "\r\n" };
					string sPasteString = Clipboard.GetText();
					string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

					string strData = string.Empty;

					for (int i = 0; i < sPasteStrings.Length; i++)
					{
						if (sPasteStrings[i].Trim() == "") continue;
						strData += sPasteStrings[i].Trim() + ",";
					}

					if (strData.Length > 0)
						strData = strData.Substring(0, strData.Length - 1);

					this.txtCarrierId.Text = strData;
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
					return;
				}

				e.Handled = true;
			}
		}

		private void txtLotId_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				this.btnSearch_Click(null, null);
			}
		}

		private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				try
				{
					string[] stringSeparators = new string[] { "\r\n" };
					string sPasteString = Clipboard.GetText();
					string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

					string strData = string.Empty;

					for (int i = 0; i < sPasteStrings.Length; i++)
					{
						if (sPasteStrings[i].Trim() == "") continue;
						strData += sPasteStrings[i].Trim() + ",";
					}

					if (strData.Length > 0)
						strData = strData.Substring(0, strData.Length - 1);

					this.txtLotId.Text = strData;
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
					return;
				}

				e.Handled = true;
			}
		}

		private string ConvertData(string data)
		{
			string strReturn = string.Empty;

			string[] list = data.Split(',');
			foreach (string alist in list)
			{
				strReturn += string.Format("'{0}',", alist);
			}

			if (strReturn.Length > 0)
				strReturn = strReturn.Substring(0, strReturn.Length - 1);

			return strReturn;
		}

		private void btnMapping_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSaveMapping()) return;

			Util.MessageConfirm("SFU1241", (result) =>
			{
				if (result == MessageBoxResult.OK)
				{
					this.SaveMapping();
				}
			});
		}

		private void btnUnmapping_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSaveUnmapping()) return;

			Util.MessageConfirm("SFU1241", (result) =>
			{
				if (result == MessageBoxResult.OK)
				{
					this.SaveUnmapping();
				}
			});
		}

		private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				this.btnMapping_Click(null, null);
			}
		}

        private void cboCarrierType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_ClearChk == "Y")
            {
                ClearControl();
            }
        }
    }
}
