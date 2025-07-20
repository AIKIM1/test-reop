/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
                   1. 수정시 FCS002_332-재고조사, FCS002_332-재고조사(활성화), COM001_214-재고조사(소형파우치)화면을 고려해 주세요.
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.15  DEVELOPER : Initial Created.
                          FCS002_332 에서 분할 처리/활성화 후공정에 필요한 항목이 추가 되어 별도 구성함.
  2021.05.03  장희만      C20220501-000014        조회옵션 PROCESS, LINE 선택사항 조건에 누락되어 추가
  2023.03.31  이홍주      SI                      소형활성화 MES.  COM001_125 --> FCS002_332
  2024.06.10  윤지해      NERP 대응 프로젝트      차수마감취소(1차만) 버튼 추가 및 차수 추가(2차 이후) 시 ERP 실적 마감 FLAG 확인 후 생성 가능하도록 변경
  2024.08.02  rifkyaditya NERP 대응 프로젝트      NERP 회계 마감여부에 따라 차수추가 제한, 마감취소 제한 (GDC)
  2024.09.04  윤지해      NERP 대응 프로젝트      차수마감 취소 시 NERP 회계마감 여부 재확인
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.Windows.Input;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
	public partial class FCS002_332 : UserControl, IWorkArea
	{
		#region Declaration & Constructor 
		Util _Util = new Util();

		public FCS002_332()
		{

			InitializeComponent();

			InitCombo();

		}

		public IFrameOperation FrameOperation
		{
			get;
			set;
		}

		CommonCombo _combo = new CommonCombo();

		private string _sEqsgID = string.Empty;
		private string _sProcID = string.Empty;
		private string _sProdID = string.Empty;
		private string _sElecType = string.Empty;
		private string _sPrjtName = string.Empty;
		private string _sNerpCloseFlag = string.Empty;  // 2024.06.10 윤지해 NERP 회계 마감 체크
		private string _sMaxSeq = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 조회
		private string _sMaxSeqCmplFlag = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 마감상태 조회
		private string _sNerpApplyFlag = string.Empty;  // 2024.06.10 윤지해 NERP 적용 동 조회

		private const string _sLOTID = "LOTID";
		private const string _sBOXID = "BOXID";

		DataView _dvSTCKCNT { get; set; }

		string _sSTCK_CNT_CMPL_FLAG = string.Empty;

		#endregion

		#region Initialize
		/// <summary>
		/// 화면내 combo 셋팅
		/// </summary>
		private void InitCombo()
		{

			//동,라인,공정,설비 셋팅
			//CommonCombo _combo = new CommonCombo();


			//동
			C1ComboBox[] cboAreaShotChild = { cboStockSeqShot };
			_combo.SetCombo(cboAreaShot, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaShotChild);

			C1ComboBox[] cboAreaUploadChild = { cboStockSeqUpload };
			_combo.SetCombo(cboAreaUpload, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaUploadChild);

			C1ComboBox[] cboAreaCompareChild = { cboStockSeqCompare };
			_combo.SetCombo(cboAreaCompare, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaCompareChild);

			//극성
			String[] sFilterElectype = { "", "ELEC_TYPE" };
			_combo.SetCombo(cboElecTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");
			_combo.SetCombo(cboElecTypeUpload, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");
			_combo.SetCombo(cboElecTypeCompare, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");

			//재고실사 제외 여부
			String[] sFilterExclFlag = { "", "STCK_CNT_EXCL_FLAG" };
			_combo.SetCombo(cboExclFlagShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterExclFlag, sCase: "COMMCODES");


			if (cboExclFlagShot.Items.Count > 0) cboExclFlagShot.SelectedIndex = 1;

			object[] objStockSeqShotParent = { cboAreaShot, ldpMonthShot };
			String[] sFilterAll = { "" };
			_combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterAll);

			object[] cboStockSeqUploadParent = { cboAreaUpload, ldpMonthUpload };
			_combo.SetComboObjParent(cboStockSeqUpload, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboStockSeqUploadParent, sFilter: sFilterAll);

			object[] cboStockSeqCompareParent = { cboAreaCompare, ldpMonthCompare };
			_combo.SetComboObjParent(cboStockSeqCompare, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboStockSeqCompareParent, sFilter: sFilterAll);
		}
		#endregion

		#region Event
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			SetProcessCombo(cboProcShot, cboAreaShot, cboStckCntGrShot);
			SetProcessCombo(cboProcUpload, cboAreaUpload, cboStckCntGrUpload);
			SetProcessCombo(cboProcCompare, cboAreaCompare, cboStckCntGrCompare);

			//사용자 권한별로 버튼 숨기기
			List<Button> listAuth = new List<Button>();
			listAuth.Add(btnExclude_SNAP);
			listAuth.Add(btnExclude_RSLT);
			Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
			//여기까지 사용자 권한별로 버튼 숨기기

			// 2024.06.10 윤지해 차수 마감 취소, 차수마감 버튼 VISIBLE 처리
			ChkNerpApplyFlag();

			ShowBtnCloseCancel(ldpMonthShot);
		}

		#region 차수마감
		private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
		{
			if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
			{
				Util.MessageValidation("SFU3499"); //마감된 차수입니다.
				return;
			}

			//마감하시겠습니까?
			LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
			{
				if (result.ToString().Equals("OK"))
				{
					DegreeClose();
				}
			}
			);
		}
		#endregion

		#region 차수마감 취소
		// 2024.06.10 윤지해 차수마감 취소 버튼 추가
		// 1차수만 ERP에서 관리하여 1차수 이후에는 마감취소 불가
		private void btnDegreeCloseCancel_Click(object sender, RoutedEventArgs e)
		{
            ChkNerpFlag(ldpMonthShot);

            if (_sNerpCloseFlag.Equals("N"))
            {
                // 마감 취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3685"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        DegreeCloseCancel();
                    }
                }
                );
            }
            else
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                ShowBtnCloseCancel(ldpMonthShot);
                return;
            }
        }
		#endregion

		#region 차수추가
		private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// 2024.06.10 윤지해 NERP 회계 마감여부 체크 추가
				ChkNerpApplyFlag();
				ChkNerpFlag(ldpMonthShot);

				string[] sAttrbute = { "Y" };

				if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sNerpCloseFlag.Equals("N"))
				{
					Util.MessageValidation("SFU3686");  // NERP 회계 마감기간 중 차수 추가를 할 수 없습니다.
					return;
				}
				else
				{
					FCS002_332_STOCKCNT_START wndSTOCKCNT_START = new FCS002_332_STOCKCNT_START();
					wndSTOCKCNT_START.FrameOperation = FrameOperation;

					if (wndSTOCKCNT_START != null)
					{
						object[] Parameters = new object[6];
						Parameters[0] = Convert.ToString(cboAreaShot.SelectedValue);
						Parameters[1] = ldpMonthShot.SelectedDateTime;

						C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

						wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

						// 팝업 화면 숨겨지는 문제 수정.
						this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_START.ShowModal()));
						wndSTOCKCNT_START.BringToFront();
					}
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
		{
			try
			{
				FCS002_332_STOCKCNT_START window = sender as FCS002_332_STOCKCNT_START;
				if (window.DialogResult == MessageBoxResult.OK)
				{
					//CommonCombo _combo = new CommonCombo();
					_combo.SetCombo(cboStockSeqShot);
					_combo.SetCombo(cboStockSeqUpload);
					_combo.SetCombo(cboStockSeqCompare);

					Util.gridClear(dgListShot);
					Util.gridClear(dgListStock);
					Util.gridClear(dgListCompare);
					Util.gridClear(dgListCompareDetail);

					SetListShot(true);
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 전산재고 조회
		private void btnSearchShot_Click(object sender, RoutedEventArgs e)
		{
			SetListShot(true);
		}
		#endregion

		#region 재고조사 조회
		private void btnSearchStock_Click(object sender, RoutedEventArgs e)
		{
			SetListStock(true);
		}
		#endregion

		#region 재고비교 조회
		private void btnSearchCompare_Click(object sender, RoutedEventArgs e)
		{
			int iEqsgCompareItemCnt = (cboEqsgCompare.ItemsSource == null ? 0 : ((DataView)cboEqsgCompare.ItemsSource).Count);
			int iEqsgCompareSelectedCnt = cboEqsgCompare.SelectedItemsToString.Split(',').Length;
			int iProcCompareItemCnt = (cboProcCompare.ItemsSource == null ? 0 : ((DataView)cboProcCompare.ItemsSource).Count);
			int iProcCompareSelectedCnt = cboProcCompare.SelectedItemsToString.Split(',').Length;

			_sEqsgID = Util.ConvertEmptyToNull(cboEqsgCompare.SelectedItemsToString);
			_sProcID = Util.ConvertEmptyToNull(cboProcCompare.SelectedItemsToString);
			_sProdID = Util.ConvertEmptyToNull(txtProdCompare.Text);
			_sElecType = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeCompare));
			_sPrjtName = Util.ConvertEmptyToNull(txtPrjtNameCompare.Text);

			//Summary 조회
			SetListCompare(_sProdID, _sEqsgID, _sProcID, _sElecType, _sPrjtName);

			//Detail 조회
			SetListCompareDetail(_sProdID, _sEqsgID, _sProcID, _sElecType, _sPrjtName);
		}
		#endregion

		private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
		{
			//CommonCombo _combo = new CommonCombo();
			_combo.SetCombo(cboStockSeqShot);

			// 2024.06.10 윤지해 차수 마감 취소, 차수마감 버튼 VISIBLE 처리
			ShowBtnCloseCancel(ldpMonthShot);
		}

		private void ldpMonthUpload_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
		{
			//CommonCombo _combo = new CommonCombo();
			_combo.SetCombo(cboStockSeqUpload);
		}

		private void ldpMonthCompare_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
		{
			//CommonCombo _combo = new CommonCombo();
			_combo.SetCombo(cboStockSeqCompare);
		}

		//private void dgListCompare_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		//{
		//    if (dgListCompare.CurrentColumn.Name.Equals("PRODID") && dgListCompare.CurrentRow !=null)
		//    {
		//        SetListCompareDetail(Util.NVC(DataTableConverter.GetValue(dgListCompare.CurrentRow.DataItem, "PRODID")));
		//    }
		//}

		private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton rb = sender as RadioButton;

			//최초 체크시에만 로직 타도록 구현
			if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
			{
				//체크시 처리될 로직
				SetListCompareDetail(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID")), _sEqsgID, _sProcID, _sElecType, _sPrjtName);


				//여기까지 체크시 처리될 로직
				//선택값 셋팅
				foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
				{
					DataTableConverter.SetValue(row.DataItem, "CHK", 0);
				}

				DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
				//row 색 바꾸기
				((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

			}
		}

		#region 재고비교 컬러설정
		private void dgListCompare_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
		{
			try
			{
				C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

				dataGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					if (e.Cell.Presenter == null)
					{
						return;
					}
					//link 색변경
					//if (e.Cell.Column.Name.Equals("PRODID"))
					//{
					//    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
					//}
					//else
					//{
					//    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
					//}

					//틀린색변경
					if (e.Cell.Column.Name.Equals("SNAP_CNT"))
					{
						string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));
						if (e.Cell.Presenter != null && sCheck.Equals("NG"))
						{
							//e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

							e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
							e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
							e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["REAL_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
							e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["REAL_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						}
					}
				}));
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}


		private void dgListCompareDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
		{
			try
			{
				C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

				dataGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					//전산재고와 실물 수량이 맞지않으면 Yellow
					string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));

					if (e.Cell.Presenter != null && sCheck.Equals("NG"))
					{
						string[] Col = { "SNAP_LOTID", "REAL_LOTID", "SNAP_QTY", "REAL_QTY", "MOVE_ORD_STAT", "REAL_STCK_CNT_DTTM", "SNAP_ASSY_LOT", "REAL_ASSY_LOT" };
						foreach (string column in Col)
						{
							if (column == e.Cell.Column.Name)
							{
								var bgColor = new BrushConverter();
								e.Cell.Presenter.Background = (Brush)bgColor.ConvertFrom("#ffffff00");
								break;
							}
						}

						//실물만 있는 LOTID면 Red
						DataRowView dr = (DataRowView)e.Cell.Row.DataItem;
						string sSNAP_LOTID = Util.NVC(dr.Row["SNAP_LOTID"]);
						string sREAL_LOTID = Util.NVC(dr.Row["REAL_LOTID"]);

						if (String.IsNullOrEmpty(sSNAP_LOTID) && !String.IsNullOrEmpty(sREAL_LOTID))
						{
							string[] Col1 = { "REAL_LOTID", "REAL_QTY", "REAL_STCK_CNT_DTTM", "REAL_ASSY_LOT" };
							foreach (string column in Col1)
							{
								if (column == e.Cell.Column.Name)
								{
									var bgColor = new BrushConverter();
									e.Cell.Presenter.Background = (Brush)bgColor.ConvertFrom("#ffff0000");
									break;
								}
							}
						}
					}
				}));
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 재고조사 엑셀업로드
		private void btnUploadFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();

			if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
			{
				fd.InitialDirectory = @"\\Client\C$";
			}

			fd.Filter = "Excel Files (.xlsx)|*.xlsx";
			if (fd.ShowDialog() == true)
			{
				using (Stream stream = fd.OpenFile())
				{
					C1XLBook book = new C1XLBook();
					book.Load(stream, FileFormat.OpenXml);
					XLSheet sheet = book.Sheets[0];

					DataTable dataTable = new DataTable();
					dataTable.Columns.Add("LOTID", typeof(string));
					dataTable.Columns.Add("CHK", typeof(bool));
					dataTable.Columns.Add("WIP_QTY", typeof(decimal));
					//for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
					//{
					//    dataTable.Columns.Add(getExcelColumnName(colInx), typeof(string));
					//}
					for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
					{
						//DataRow dataRow = dataTable.NewRow();
						//for (int colInx = 0; colInx < sheet.Rows.Count; colInx++)
						//{
						//    XLCell cell = sheet.GetCell(rowInx, colInx);
						//    if (cell != null)
						//    {
						//        dataRow[getExcelColumnName(colInx)] = cell.Text;
						//    }
						//}
						DataRow dataRow = dataTable.NewRow();
						XLCell cell = sheet.GetCell(rowInx, 0);
						if (cell != null)
						{
							dataRow["LOTID"] = cell.Text;
							dataRow["CHK"] = true;
						}

						dataTable.Rows.Add(dataRow);
					}
					dataTable.AcceptChanges();

					dgListStock.ItemsSource = DataTableConverter.Convert(dataTable);
				}
			}
		}

		private void btnUploadSave_Click(object sender, RoutedEventArgs e)
		{
			int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

			if (iRow < 0)
			{
				Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
				return;
			}

			//저장 하시겠습니까?
			LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
			{
				if (result.ToString().Equals("OK"))
				{
					SaveLotList();
				}
			}
			);
		}
		#endregion

		#region 재고비교 엑셀저장
		private void btnExport_Click(object sender, RoutedEventArgs e)
		{

			C1DataGrid[] dataGridArray = new C1DataGrid[2];
			dataGridArray[0] = dgListCompare;
			dataGridArray[1] = dgListCompareDetail;
			string[] excelTabNameArray = new string[2] { "Summary", "Detail" };

			new LGC.GMES.MES.Common.ExcelExporter().Export(dataGridArray, excelTabNameArray);
		}
		#endregion

		#region 동/기준월/차수별  Note설정
		private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			_dvSTCKCNT = cboStockSeqShot.ItemsSource as DataView;
			txtStckCntCmplFlagShot.Text = string.Empty;

			string sStckCntSeq = cboStockSeqShot.Text;
			if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
			{
				_dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
				txtStckCntCmplFlagShot.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
				_sSTCK_CNT_CMPL_FLAG = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();

				_dvSTCKCNT.RowFilter = null;
			}

			// 2024.06.10 윤지해 추가
			ShowBtnCloseCancel(ldpMonthShot);
		}

		private void cboStockSeqUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			_dvSTCKCNT = cboStockSeqUpload.ItemsSource as DataView;
			txtStckCntCmplFlagUpload.Text = string.Empty;

			string sStckCntSeq = cboStockSeqUpload.Text;
			if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
			{
				_dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
				txtStckCntCmplFlagUpload.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
				_dvSTCKCNT.RowFilter = null;
			}
		}

		private void cboStockSeqCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			_dvSTCKCNT = cboStockSeqCompare.ItemsSource as DataView;
			txtStckCntCmplFlagCompare.Text = string.Empty;

			string sStckCntSeq = cboStockSeqCompare.Text;
			if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
			{
				_dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
				txtStckCntCmplFlagCompare.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
				_dvSTCKCNT.RowFilter = null;
			}
		}
		#endregion

		#region 전산재고 전체선택 & 전체해제
		C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
		{
			Background = new SolidColorBrush(Colors.Transparent),
			MouseOverBrush = new SolidColorBrush(Colors.Transparent),
		};

		CheckBox chkAll = new CheckBox()
		{
			IsChecked = false,
			Background = new SolidColorBrush(Colors.Transparent),
			VerticalAlignment = System.Windows.VerticalAlignment.Center,
			HorizontalAlignment = System.Windows.HorizontalAlignment.Center
		};

		private void dgListShot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
		{
			this.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (string.IsNullOrEmpty(e.Column.Name) == false)
				{
					if (e.Column.Name.Equals("CHK"))
					{
						pre.Content = chkAll;
						e.Column.HeaderPresenter.Content = pre;
						chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
						chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
						chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
						chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
					}
				}
			}));
		}

		void checkAll_Checked(object sender, RoutedEventArgs e)
		{
			if ((bool)chkAll.IsChecked)
			{
				if (dgListShot.GetRowCount() > 0)
				{
					if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
					{
						Util.MessageValidation("SFU4550"); //동일한 재고실사 제외여부만 전체선택이 가능합니다.
						chkAll.IsChecked = false;
						return;
					}
				}

				for (int inx = 0; inx < dgListShot.GetRowCount(); inx++)
				{
					DataTableConverter.SetValue(dgListShot.Rows[inx].DataItem, "CHK", true);
				}

				//전산재고 제외/제외취소 버튼 Display
				SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")));
			}
		}

		void checkAll_Unchecked(object sender, RoutedEventArgs e)
		{
			for (int inx = 0; inx < dgListShot.GetRowCount(); inx++)
			{
				DataTableConverter.SetValue(dgListShot.Rows[inx].DataItem, "CHK", false);
			}
		}


		private void chkHeader_SNAP_Click(object sender, RoutedEventArgs e)
		{
			if (sender == null) return;

			int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
			object objRowIdx = dgListShot.Rows[idx].DataItem;

			if (objRowIdx != null)
			{
				if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
				{
					if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("CHK = 'True' AND STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[idx].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
					{
						Util.MessageValidation("SFU4549"); //동일한 재고실사 제외여부만 선택이 가능합니다.
						DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", false);
						return;
					}

					DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", true);

					//전산재고 제외/제외취소 버튼 Display
					SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(objRowIdx, "STCK_CNT_EXCL_FLAG")));
				}
			}
		}
		#endregion

		#region 재고실사 전체선택 & 전체해제
		private void chkHeaderAll_RSLT_Checked(object sender, RoutedEventArgs e)
		{
			Util.DataGridCheckAllChecked(dgListStock);
		}
		private void chkHeaderAll_RSLT_Unchecked(object sender, RoutedEventArgs e)
		{
			Util.DataGridCheckAllUnChecked(dgListStock);
		}
		#endregion

		#region 전산재고 재공제외
		private void btnExclude_SNAP_Click(object sender, RoutedEventArgs e)
		{
            // 2024.08.02 rifkyaditya  Validation for NERP Accounting Closed so can't reset SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정

            // if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }
            else
            {
				int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

				if (iRow < 0)
				{
					Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
					return;
				}

				if (string.IsNullOrEmpty(txtExcludeNote_SNAP.Text.Trim()))
				{
					Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
					return;
				}

				//전산재고 LOTID를 제외 하시겠습니까?
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
				{
					if (result.ToString().Equals("OK"))
					{
						string sSTCK_CNT_EXCL_FLAG = "Y";
						Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
					}
				}
				);
			}
		}
		#endregion

		#region 전산재고 재공제외취소
		private void btnExclude_SNAP_Cancel_Click(object sender, RoutedEventArgs e)
		{
            // 2024.08.02 rifkyaditya  Validation for NERP Accounting Closed so can't modify SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정

            // if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }
            else
            {
				int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

				if (iRow < 0)
				{
					Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
					return;
				}

				//전산재고 LOTID를 제외 취소 하시겠습니까?
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4551"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
				{
					if (result.ToString().Equals("OK"))
					{
						string sSTCK_CNT_EXCL_FLAG = "N";
						Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
					}
				}
				);
			}
		}
		#endregion

		#region 전산재고 선택재고변경
		private void btnRowReSet_Click(object sender, RoutedEventArgs e)
		{
            // 2024.08.02 rifkyaditya  Validation for NERP Accounting Closed so can't modify SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정

            // if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }
            else
            {
				int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

				if (iRow < 0)
				{
					Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
					return;
				}

				//전산재고 재공정보를 현재 재공정보로 변경 하시겠습니까?
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4588"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
				{
					if (result.ToString().Equals("OK"))
					{
						SetRowLotUpdate_SNAP();
					}
				}
				);
			}
		}
		#endregion

		#region 재고실사 재공제외
		private void btnExclude_RSLT_Click(object sender, RoutedEventArgs e)
		{
			// 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
			if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
			{
				Util.MessageValidation("SFU3499"); //마감된 차수입니다.
				return;
			}
			else
			{
				int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

				if (iRow < 0)
				{
					Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
					return;
				}

				//재고실사 LOTID를 제외 하시겠습니까?
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4213"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
				{
					if (result.ToString().Equals("OK"))
					{
						Exclude_RSLT();
					}
				}
				);
			}
		}
		#endregion

		#region 재고실사 재공 정보변경
		private void btnStckCntRslt_Click(object sender, RoutedEventArgs e)
		{
			// 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
			if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
			{
				Util.MessageValidation("SFU3499"); //마감된 차수입니다.
				return;
			}
			else
			{
				try
				{
					int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

					if (iRow < 0)
					{
						Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
						return;
					}

					FCS002_332_STOCKCNT_RSLT wndSTOCKCNT_RSLT = new FCS002_332_STOCKCNT_RSLT();
					wndSTOCKCNT_RSLT.FrameOperation = FrameOperation;

					if (wndSTOCKCNT_RSLT != null)
					{
						DataTable dtRSLT = DataTableConverter.Convert(dgListStock.ItemsSource);
						DataRow[] drRSLT = dtRSLT.Select(" CHK = 'True' ");

						object[] Parameters = new object[5];
						Parameters[0] = "COMMON";
						Parameters[1] = Convert.ToString(cboAreaUpload.SelectedValue);
						Parameters[2] = ldpMonthUpload.SelectedDateTime;
						Parameters[3] = Convert.ToString(cboStockSeqUpload.SelectedValue);
						Parameters[4] = drRSLT;

						C1WindowExtension.SetParameters(wndSTOCKCNT_RSLT, Parameters);

						wndSTOCKCNT_RSLT.Closed += new EventHandler(wndSTOCKCNT_RSLT_Closed);

						// 팝업 화면 숨겨지는 문제 수정.
						this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_RSLT.ShowModal()));
						wndSTOCKCNT_RSLT.BringToFront();
					}
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
				}
			}
		}

		private void wndSTOCKCNT_RSLT_Closed(object sender, EventArgs e)
		{
			try
			{
				FCS002_332_STOCKCNT_RSLT window = sender as FCS002_332_STOCKCNT_RSLT;
				if (window.DialogResult == MessageBoxResult.OK)
				{
					SetListStock(true);
					Util.MessageInfo("SFU1275");//정상처리 되었습니다.
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#endregion

		#region Mehod

		#region 차수마감
		private void DegreeClose()
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
				dr["USERID"] = LoginInfo.USERID;

				if (dr["AREAID"].Equals("") || dr["STCK_CNT_SEQNO"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				//DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCKCNT", "RQSTDT", "RSLTDT", RQSTDT);
				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);

				//CommonCombo _combo = new CommonCombo();
				_combo.SetCombo(cboStockSeqShot);
				_combo.SetCombo(cboStockSeqUpload);
				_combo.SetCombo(cboStockSeqCompare);

				// 2024.06.10 윤지해 추가
				ShowBtnCloseCancel(ldpMonthShot);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 전산재고 조회
		private void SetListShot(bool bSingleInput = true, string sLotID = "")
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

				dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
				if (dr["AREAID"].Equals("")) return;

				if (string.IsNullOrEmpty(txtLotIdShot.Text.Trim()) && sLotID.Equals(""))
				{
					int iEqsgShotItemCnt = (cboEqsgShot.ItemsSource == null ? 0 : ((DataView)cboEqsgShot.ItemsSource).Count);
					int iEqsgShotSelectedCnt = cboEqsgShot.SelectedItemsToString.Split(',').Length;
					int iProcShotItemCnt = (cboProcShot.ItemsSource == null ? 0 : ((DataView)cboProcShot.ItemsSource).Count);
					int iProcShotSelectedCnt = cboProcShot.SelectedItemsToString.Split(',').Length;

					dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgShot.SelectedItemsToString);
					dr["PROCID"] = Util.ConvertEmptyToNull(cboProcShot.SelectedItemsToString);
					dr["PRODID"] = Util.ConvertEmptyToNull(txtProdShot.Text);
					//dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeShot));
					dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameShot.Text);
					dr["STCK_CNT_EXCL_FLAG"] = Util.ConvertEmptyToNull(Util.GetCondition(cboExclFlagShot));
				}
				else
				{
					if (bSingleInput == true)
					{
						dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdShot.Text);
					}
					else
					{
						dr["LOTID"] = sLotID;
					}
				}

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MESSTOCK_FCS", "RQSTDT", "RSLTDT", RQSTDT);

				if (dtRslt.Rows.Count == 0 && bSingleInput == true)
				{
					Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
					Util.GridSetData(dgListShot, dtRslt, FrameOperation);
				}
				else if (dtRslt.Rows.Count > 0 && bSingleInput == true)
				{
					Util.GridSetData(dgListShot, dtRslt, FrameOperation);
				}
				else if (dtRslt.Rows.Count > 0 && bSingleInput == false)
				{
					DataTable dtSource = DataTableConverter.Convert(dgListShot.ItemsSource);
					dtSource.Merge(dtRslt);
					Util.gridClear(dgListShot);
					Util.GridSetData(dgListShot, dtSource, FrameOperation, true);
				}

			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
			finally
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
		}
		#endregion

		#region 재고조사 조회
		private void SetListStock(bool bSingleInput = true, string sLotID = "")
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthUpload);
				if (!Util.GetCondition(cboStockSeqUpload, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqUpload));
				}
				if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

				dr["AREAID"] = Util.GetCondition(cboAreaUpload, "SFU3203"); //동은필수입니다.
				if (dr["AREAID"].Equals("")) return;

				if (string.IsNullOrEmpty(txtLotIdUpload.Text.Trim()) && sLotID.Equals(""))
				{
					int iEqsgUploadItemCnt = (cboEqsgUpload.ItemsSource == null ? 0 : ((DataView)cboEqsgUpload.ItemsSource).Count);
					int iEqsgUploadSelectedCnt = cboEqsgUpload.SelectedItemsToString.Split(',').Length;
					int iProcUploadItemCnt = (cboProcUpload.ItemsSource == null ? 0 : ((DataView)cboProcUpload.ItemsSource).Count);
					int iProcUploadSelectedCnt = cboProcUpload.SelectedItemsToString.Split(',').Length;

					dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgUpload.SelectedItemsToString);
					dr["PROCID"] = Util.ConvertEmptyToNull(cboProcUpload.SelectedItemsToString);
					dr["PRODID"] = Util.ConvertEmptyToNull(txtProdUpload.Text);
					dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeUpload));
					dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameUpload.Text);
				}
				else
				{
					if (bSingleInput == true)
					{
						dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdUpload.Text);
					}
					else
					{
						dr["LOTID"] = sLotID;
					}
				}

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INFO_FCS", "RQSTDT", "RSLTDT", RQSTDT);

				if (dtRslt.Rows.Count == 0 && bSingleInput == true)
				{
					Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
					Util.GridSetData(dgListStock, dtRslt, FrameOperation);
				}
				else if (dtRslt.Rows.Count > 0 && bSingleInput == true)
				{
					Util.GridSetData(dgListStock, dtRslt, FrameOperation);
				}
				else if (dtRslt.Rows.Count > 0 && bSingleInput == false)
				{
					DataTable dtSource = DataTableConverter.Convert(dgListStock.ItemsSource);
					dtSource.Merge(dtRslt);
					Util.gridClear(dgListStock);
					Util.GridSetData(dgListStock, dtSource, FrameOperation);
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
			finally
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
		}
		#endregion

		#region 재고비교 조회
		private void SetListCompare(string sProdID = null, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null)
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthCompare);
				if (!Util.GetCondition(cboStockSeqCompare, "차수는필수입니다.").Equals(""))
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqCompare));
				}
				if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

				dr["AREAID"] = Util.GetCondition(cboAreaCompare, "SFU3203"); //동은필수입니다.
				if (dr["AREAID"].Equals("")) return;

				dr["EQSGID"] = sEqsgID;
				dr["PROCID"] = sProcID;
				dr["PRODID"] = sProdID;
				dr["PRDT_CLSS_CODE"] = sElecType;
				dr["PRJT_NAME"] = sPrjtName;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE_FCS", "RQSTDT", "RSLTDT", RQSTDT);
				Util.GridSetData(dgListCompare, dtRslt, FrameOperation);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
			finally
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
		}

		private void SetListCompareDetail(string sProdID, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null)
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthCompare);
				if (!Util.GetCondition(cboStockSeqCompare, "차수는필수입니다.").Equals(""))
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqCompare));
				}
				if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

				dr["AREAID"] = Util.GetCondition(cboAreaCompare, "SFU3203"); //동은필수입니다.
				if (dr["AREAID"].Equals("")) return;

				dr["EQSGID"] = sEqsgID;
				dr["PROCID"] = sProcID;
				dr["PRODID"] = sProdID;
				dr["PRDT_CLSS_CODE"] = sElecType;
				dr["PRJT_NAME"] = sPrjtName;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE_DETAIL_FCS", "RQSTDT", "RSLTDT", RQSTDT);
				Util.GridSetData(dgListCompareDetail, dtRslt, FrameOperation);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
			finally
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
		}
		#endregion

		#region 재고조사 엑셀업로드
		private void SaveLotList()
		{
			try
			{
				//if (dgListStock.GetRowCount() < 1)
				//{
				//    Util.MessageValidation("SFU2946"); //재고조사 파일을 먼저 선택 해 주세요.
				//    return;
				//}

				string sMonth = Util.GetCondition(ldpMonthUpload);
				string sArea = Util.GetCondition(cboAreaUpload, "동은 필수입니다.");
				if (sArea.Equals("")) return;
				Int16 iCnt = 0;
				if (!Util.GetCondition(cboStockSeqUpload, "차수는 필수입니다.").Equals(""))
				{
					iCnt = Convert.ToInt16(Util.GetCondition(cboStockSeqUpload));
				}

				if (iCnt.Equals(0)) return;

				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("SCAN_TYPE", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("NOTE", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));
				RQSTDT.Columns.Add("WIP_QTY", typeof(decimal));
				RQSTDT.Columns.Add("WIP_QTY2", typeof(decimal));

				DataRowView rowview = null;

				foreach (C1.WPF.DataGrid.DataGridRow row in dgListStock.Rows)
				{
					rowview = row.DataItem as DataRowView;

					if (!String.IsNullOrEmpty(rowview["LOTID"].ToString()) && "True".Equals(rowview["CHK"].ToString()))
					{
						DataRow dr = RQSTDT.NewRow();
						dr["LANGID"] = LoginInfo.LANGID;
						dr["STCK_CNT_YM"] = sMonth;
						dr["AREAID"] = sArea;
						dr["STCK_CNT_SEQNO"] = iCnt;
						dr["NOTE"] = "SFU";
						dr["LOTID"] = rowview["LOTID"].ToString();
						dr["USERID"] = LoginInfo.USERID;

						if (rdoBox.IsChecked == true)
						{ dr["SCAN_TYPE"] = _sBOXID; }
						else
						{ dr["SCAN_TYPE"] = _sLOTID; }

						if (!String.IsNullOrEmpty(rowview["WIP_QTY"].ToString()))
						{
							dr["WIP_QTY"] = rowview["WIP_QTY"].ToString();
							dr["WIP_QTY2"] = rowview["WIP_QTY"].ToString();
						}

						RQSTDT.Rows.Add(dr);
					}
				}

				//DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);
				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EXCEL_UPLOAD", "RQSTDT", "RSLTDT", RQSTDT);

				Util.AlertInfo("SFU1270");  //저장되었습니다.
				dgListStock.ItemsSource = null;
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
			finally
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
		}
		#endregion

		#region 전산재고 제외/제외취소 버튼 Display
		private void SetExcludeDisplay(string sSTCK_CNT_EXCL_FLAG)
		{
			if (sSTCK_CNT_EXCL_FLAG.Equals("N"))
			{
				tblExcludeNote_SNAP.Visibility = Visibility.Visible;
				txtExcludeNote_SNAP.Visibility = Visibility.Visible;
				btnExclude_SNAP.Visibility = Visibility.Visible;
				btnExclude_SNAP_Cancel.Visibility = Visibility.Collapsed;
			}
			else
			{
				tblExcludeNote_SNAP.Visibility = Visibility.Collapsed;
				txtExcludeNote_SNAP.Visibility = Visibility.Collapsed;
				btnExclude_SNAP.Visibility = Visibility.Collapsed;
				btnExclude_SNAP_Cancel.Visibility = Visibility.Visible;
			}
		}
		#endregion

		#region 전산재고 제외처리/제외취소처리
		private void Exclude_SNAP(string sSTCK_CNT_EXCL_FLAG)
		{
			try
			{
				this.dgListShot.EndEdit();
				this.dgListShot.EndEditRow(true);

				DataTable dtRSLT = ((DataView)dgListShot.ItemsSource).Table;

				//RQSTDT
				DataSet inData = new DataSet();
				DataTable RQSTDT = inData.Tables.Add("RQSTDT");
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_EXCL_USERID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_EXCL_NOTE", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));

				for (int i = 0; i < dtRSLT.Rows.Count; i++)
				{
					if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
					{
						DataRow dr = RQSTDT.NewRow();
						dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
						dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
						dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
						dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
						dr["STCK_CNT_EXCL_FLAG"] = sSTCK_CNT_EXCL_FLAG;
						dr["STCK_CNT_EXCL_USERID"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? LoginInfo.USERID : "";
						dr["STCK_CNT_EXCL_NOTE"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? txtExcludeNote_SNAP.Text : "";
						dr["USERID"] = LoginInfo.USERID;

						RQSTDT.Rows.Add(dr);
					}
				}

				loadingIndicator.Visibility = Visibility.Visible;

				new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP", "RQSTDT", null, (bizResult, bizException) =>
				{
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");//정상처리되었습니다.

						SetListShot(true);
						txtExcludeNote_SNAP.Text = string.Empty;
					}
					catch (Exception ex)
					{
						Util.MessageException(ex);
					}
					finally
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
					}

				}, inData);
			}
			catch (Exception ex)
			{
				Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_SNAP", ex.Message, ex.ToString());
			}
		}
		#endregion

		#region 전산재고 선택 LOTID 현상태 반영
		private void SetRowLotUpdate_SNAP()
		{
			try
			{

				this.dgListShot.EndEdit();
				this.dgListShot.EndEditRow(true);
				DataTable dtRSLT = ((DataView)dgListShot.ItemsSource).Table;

				//RQSTDT
				DataSet inData = new DataSet();
				DataTable RQSTDT = inData.Tables.Add("RQSTDT");
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));

				for (int i = 0; i < dtRSLT.Rows.Count; i++)
				{
					if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
					{
						DataRow dr = RQSTDT.NewRow();
						dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
						dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
						dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
						dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
						dr["USERID"] = LoginInfo.USERID;

						RQSTDT.Rows.Add(dr);
					}
				}

				loadingIndicator.Visibility = Visibility.Visible;

				new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP_LOTID", "RQSTDT", null, (bizResult, bizException) =>
				{
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");//정상처리되었습니다.

						SetListShot(true);
					}
					catch (Exception ex)
					{
						Util.MessageException(ex);
					}
					finally
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
					}

				}, inData);
			}
			catch (Exception ex)
			{
				Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_SNAP_LOTID", ex.Message, ex.ToString());
			}
		}
		#endregion

		#region 재고실사 제외처리
		private void Exclude_RSLT()
		{
			try
			{
				//INDATA
				this.dgListStock.EndEdit();
				this.dgListStock.EndEditRow(true);

				DataSet inData = new DataSet();
				DataTable RQSTDT = inData.Tables.Add("RQSTDT");
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("REAL_WIP_QTY", typeof(decimal));
				RQSTDT.Columns.Add("USERID", typeof(string));
				RQSTDT.Columns.Add("USE_FLAG", typeof(string));

				DataTable dtRSLT = ((DataView)dgListStock.ItemsSource).Table;
				for (int i = 0; i < dtRSLT.Rows.Count; i++)
				{
					if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
					{
						DataRow dr = RQSTDT.NewRow();
						dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
						dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
						dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
						dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
						dr["REAL_WIP_QTY"] = dtRSLT.Rows[i]["WIP_QTY"].ToString();
						dr["USERID"] = LoginInfo.USERID;
						dr["USE_FLAG"] = "N";

						RQSTDT.Rows.Add(dr);
					}
				}

				loadingIndicator.Visibility = Visibility.Visible;

				new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_RSLT", "RQSTDT", null, (bizResult, bizException) =>
				{
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");//정상처리되었습니다.

						SetListStock(true);
					}
					catch (Exception ex)
					{
						Util.MessageException(ex);
					}
					finally
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
					}

				}, inData);
			}
			catch (Exception ex)
			{
				Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_RSLT", ex.Message, ex.ToString());
			}
		}


		#endregion

		#region 차수마감 취소
		// 2024.06.10 윤지해 차수마감 취소 기능 추가
		private void DegreeCloseCancel()
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
				dr["USERID"] = LoginInfo.USERID;

				if (dr["AREAID"].Equals("") || dr["STCK_CNT_SEQNO"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT_CANCEL", "INDATA", null, RQSTDT);

				_combo.SetCombo(cboStockSeqShot);
				_combo.SetCombo(cboStockSeqUpload);
				_combo.SetCombo(cboStockSeqCompare);

				// 2024.06.10 윤지해 추가
				ShowBtnCloseCancel(ldpMonthShot);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 차수마감 취소 버튼 VISIBLE 처리
		// 2024.06.10 윤지해 차수마감 취소 버튼 VISIBLE 처리
		private void ShowBtnCloseCancel(LGCDatePicker ldpMonthShot)
		{
			ChkNerpApplyFlag();

			string[] sAttrbute = { "Y" };

			if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y"))
			{
				ChkNerpFlag(ldpMonthShot);
				ChkMaxSeq(ldpMonthShot);

				if (_sNerpCloseFlag.Equals("Y"))
				{
					// NERP 회계 마감이 완료되면 차수마감 취소 불가
					btnDegreeClose.Visibility = Visibility.Visible;
					btnDegreeCloseCancel.Visibility = Visibility.Collapsed;
				}
				else if (!_sMaxSeq.Equals("1") || (_sMaxSeq.Equals("1") && _sMaxSeqCmplFlag.Equals("N")))
				{
					// 회계마감이 되지 않고 2차수 이상이거나, 현재 최고차수가 1차수이고 차수마감이 되지 않은 경우 차수마감 취소 버튼 Collapsed
					btnDegreeClose.Visibility = Visibility.Visible;
					btnDegreeCloseCancel.Visibility = Visibility.Collapsed;
				}
				else
				{
					// 현재 최고 차수가 1차수이고 차수마감이 된 경우(NERP 회계 마감 완료되지 않았을 경우)
					btnDegreeClose.Visibility = Visibility.Collapsed;
					btnDegreeCloseCancel.Visibility = Visibility.Visible;
				}
			}
			else
			{
				btnDegreeClose.Visibility = Visibility.Visible;
				btnDegreeCloseCancel.Visibility = Visibility.Collapsed;
			}
		}
		#endregion

		#region 동별공통코드 조회
		// 2024.06.10 윤지해 동별공통코드 조회
		private bool ChkAreaComCode(string sCodeType, string sCodeName, string[] sAttribute)
		{
			try
			{
				string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
				RQSTDT.Columns.Add("COM_CODE", typeof(string));
				RQSTDT.Columns.Add("USE_FLAG", typeof(string));
				for (int i = 0; i < sColumnArr.Length; i++)
					RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["AREAID"] = Util.GetCondition(cboAreaShot);
				dr["COM_TYPE_CODE"] = sCodeType;
				dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
				dr["USE_FLAG"] = 'Y';
				for (int i = 0; i < sAttribute.Length; i++)
				{
					dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
				}

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
				return false;
			}
		}
		#endregion

		#region NERP 적용여부 확인
		// 2024.06.10 윤지해 NERP 적용여부 확인
		private void ChkNerpApplyFlag()
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["SYSTEM_ID"] = LoginInfo.SYSID;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_NERP_APPLY_FLAG", "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					_sNerpApplyFlag = dtRslt.Rows[0]["NERP_APPLY_FLAG"].ToString();
				}
				else
				{
					_sNerpApplyFlag = "N";
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region NERP 회계 마감여부 확인
		// 2024.06.10 윤지해 NERP 회계 마감여부 확인
		private void ChkNerpFlag(LGCDatePicker ldpMonthShot)
		{
			try
			{
				// 2024.08.02 rifkyaditya Change to reduce minus 1 month for input in DA
				DateTime LastMonth = DateTime.Now;

				if (ldpMonthShot.SelectedDateTime.Year > DateTime.MinValue.Year || ldpMonthShot.SelectedDateTime.Month > DateTime.MinValue.Month)
				{
					LastMonth = ldpMonthShot.SelectedDateTime.AddMonths(-1);
				}

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("CLOSE_YM", typeof(string));
				RQSTDT.Columns.Add("SHOPID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
				// dr["CLOSE_YM"] = Util.GetCondition(ldpMonthShot);
				dr["CLOSE_YM"] = LastMonth.ToString("yyyyMM");

				if (dr["CLOSE_YM"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_NERP_ACTG_CLOSE", "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					_sNerpCloseFlag = dtRslt.Rows[0]["CLOSE_FLAG"].ToString();
				}
				else
				{
					_sNerpCloseFlag = "N";
				}

			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 선택한 재고실사 년월에 따른 마지막 차수 및 차수 마감여부 조회
		// 2024.06.10 윤지해 선택한 재고실사 년월에 따른 마지막 차수 및 차수 마감여부 조회
		private void ChkMaxSeq(LGCDatePicker ldpMonthShot)
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
				dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.

				if (dr["STCK_CNT_YM"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_MAX_SEQ_TOP", "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					_sMaxSeq = dtRslt.Rows[0]["STCK_CNT_SEQNO"].ToString();
					_sMaxSeqCmplFlag = dtRslt.Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();
				}
				else
				{
					_sMaxSeq = "0";
					_sMaxSeqCmplFlag = "N";
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#endregion

		#region Area - SelectedValueChanged
		private void cboAreaShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			try
			{
				this.Dispatcher.BeginInvoke(new System.Action(() =>
				{
					SetStckCntGrShotCombo(cboStckCntGrShot, cboAreaShot);
				}));

				// 2024.06.10 윤지해 추가
				ShowBtnCloseCancel(ldpMonthShot);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void cboAreaUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			try
			{
				this.Dispatcher.BeginInvoke(new System.Action(() =>
				{
					SetStckCntGrShotCombo(cboStckCntGrUpload, cboAreaUpload);
				}));
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void cboAreaCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			try
			{
				this.Dispatcher.BeginInvoke(new System.Action(() =>
				{
					SetStckCntGrShotCombo(cboStckCntGrCompare, cboAreaCompare);
				}));
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region 그룹선택 Combo 조회
		private void SetStckCntGrShotCombo(C1ComboBox cboStckCntGr, C1ComboBox cboArea)
		{
			try
			{
				const string bizRuleName = "DA_PRD_SEL_STOCK_CNT_GR_CBO";
				string[] arrColumn = { "LANGID", "AREAID" };
				string[] arrCondition = { LoginInfo.LANGID, (string)cboArea.SelectedValue ?? null };
				string selectedValueText = cboStckCntGr.SelectedValuePath;
				string displayMemberText = cboStckCntGr.DisplayMemberPath;

				CommonCombo.CommonBaseCombo(bizRuleName, cboStckCntGr, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);

				int index = cboStckCntGr.Items.Count == 2 ? 1 : 0;
				cboStckCntGr.SelectedIndex = index;
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 그룹선택 - SelectedValueChanged
		private void cboStckCntGrShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetProcessCombo(cboProcShot, cboAreaShot, cboStckCntGrShot);
			}));
		}

		private void cboStckCntGrUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetProcessCombo(cboProcUpload, cboAreaUpload, cboStckCntGrUpload);
			}));
		}

		private void cboStckCntGrCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetProcessCombo(cboProcCompare, cboAreaCompare, cboStckCntGrCompare);
			}));
		}
		#endregion

		#region 공정 Combo 조회
		private void SetProcessCombo(MultiSelectionBox cboProc, C1ComboBox cboArea, C1ComboBox cboStckCntGr)
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_GR_CODE", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["AREAID"] = cboArea.SelectedValue ?? null;
				dr["STCK_CNT_GR_CODE"] = cboStckCntGr.SelectedValue ?? null;
				RQSTDT.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

				cboProc.ItemsSource = DataTableConverter.Convert(dtResult);

				if (dtResult.Rows.Count > 0)
				{
					cboProc.CheckAll();
				}
			}
			catch (Exception ex)
			{
				//Util.MessageException(ex);
			}
		}


		#endregion

		#region 공정 선택 - SelectionChanged
		private void cboProcShot_SelectionChanged(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetEqsgCombo(cboEqsgShot, cboProcShot, cboAreaShot);
			}));
		}

		private void cboProcUpload_SelectionChanged(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetEqsgCombo(cboEqsgUpload, cboProcUpload, cboAreaUpload);
			}));
		}

		private void cboProcCompare_SelectionChanged(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				SetEqsgCombo(cboEqsgCompare, cboProcCompare, cboAreaCompare);
			}));
		}
		#endregion

		#region 라인 Combo 조회
		private void SetEqsgCombo(MultiSelectionBox cboEqsg, MultiSelectionBox cboProc, C1ComboBox cboArea)
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["PROCID"] = cboProc.SelectedItemsToString ?? null;
				dr["AREAID"] = cboArea.SelectedValue ?? null;
				RQSTDT.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_EQSG_FCS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

				cboEqsg.ItemsSource = DataTableConverter.Convert(dtResult);

				if (dtResult.Rows.Count > 0)
				{
					cboEqsg.CheckAll();
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		#endregion

		private void btnAddCompare_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				FCS002_332_COMPARE_ADD wndCOMPARE_ADD = new FCS002_332_COMPARE_ADD();
				wndCOMPARE_ADD.FrameOperation = FrameOperation;

				if (wndCOMPARE_ADD != null)
				{
					object[] Parameters = new object[5];
					Parameters[0] = "";
					Parameters[1] = Convert.ToString(cboAreaCompare.SelectedValue);
					Parameters[2] = ldpMonthCompare.SelectedDateTime;
					Parameters[3] = Convert.ToString(cboStockSeqCompare.SelectedValue);

					C1WindowExtension.SetParameters(wndCOMPARE_ADD, Parameters);

					wndCOMPARE_ADD.Closed += new EventHandler(wndCOMPARE_ADD_Closed);

					// 팝업 화면 숨겨지는 문제 수정.
					this.Dispatcher.BeginInvoke(new Action(() => wndCOMPARE_ADD.ShowModal()));
					wndCOMPARE_ADD.BringToFront();
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}

		}

		private void wndCOMPARE_ADD_Closed(object sender, EventArgs e)
		{
			try
			{
				FCS002_332_COMPARE_ADD window = sender as FCS002_332_COMPARE_ADD;
				if (window.DialogResult == MessageBoxResult.OK)
				{
					//Summary 조회
					//Detail 조회
					btnSearchCompare_Click(null, null);

					Util.MessageInfo("SFU1275");//정상처리 되었습니다.
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void dgListStock_CommittedEdit(object sender, DataGridCellEventArgs e)
		{
			if (dgListStock.Rows[e.Cell.Row.Index] == null)
				return;

			if (e.Cell.Column.IsReadOnly == false)
			{
				DataTableConverter.SetValue(dgListStock.Rows[e.Cell.Row.Index].DataItem, "CHK", true);

				//row 색 바꾸기
				dgListStock.SelectedIndex = e.Cell.Row.Index;

			}
		}

		private void txtLotIdShot_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				try
				{
					string[] stringSeparators = new string[] { "\r\n" };
					string sPasteString = Clipboard.GetText();
					string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

					if (sPasteStrings.Count() > 100)
					{
						Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
						return;
					}

					Util.gridClear(dgListShot);

					for (int i = 0; i < sPasteStrings.Length; i++)
					{
						ShowLoadingIndicator();

						if (string.IsNullOrEmpty(sPasteStrings[i]))
						{
							break;
						}

						if (dgListShot.GetRowCount() > 0)
						{
							for (int idx = 0; idx < dgListShot.GetRowCount(); idx++)
							{
								if (DataTableConverter.GetValue(dgListShot.Rows[idx].DataItem, "LOTID").ToString() == sPasteStrings[i])
								{
									dgListShot.ScrollIntoView(idx, dgListShot.Columns["CHK"].Index);
									dgListShot.SelectedIndex = idx;
									txtLotIdShot.Focus();
									txtLotIdShot.Text = string.Empty;
									return;
								}
							}
						}

						SetListShot(false, sPasteStrings[i]);
						System.Windows.Forms.Application.DoEvents();
					}

					txtLotIdShot.Focus();
					txtLotIdShot.Text = string.Empty;

					if (dgListShot.GetRowCount() <= 0)
					{
						Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
					}
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
				}
				finally
				{
					HiddenLoadingIndicator();

					e.Handled = true;
				}
			}
		}

		private void txtLotIdUpload_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				try
				{
					string[] stringSeparators = new string[] { "\r\n" };
					string sPasteString = Clipboard.GetText();
					string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

					if (sPasteStrings.Count() > 100)
					{
						Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
						return;
					}

					Util.gridClear(dgListStock);

					for (int i = 0; i < sPasteStrings.Length; i++)
					{
						ShowLoadingIndicator();

						if (string.IsNullOrEmpty(sPasteStrings[i]))
						{
							break;
						}

						if (dgListStock.GetRowCount() > 0)
						{
							for (int idx = 0; idx < dgListStock.GetRowCount(); idx++)
							{
								if (DataTableConverter.GetValue(dgListStock.Rows[idx].DataItem, "LOTID").ToString() == sPasteStrings[i])
								{
									dgListStock.ScrollIntoView(idx, dgListStock.Columns["CHK"].Index);
									dgListStock.SelectedIndex = idx;
									txtLotIdUpload.Focus();
									txtLotIdUpload.Text = string.Empty;
									return;
								}
							}
						}

						SetListStock(false, sPasteStrings[i]);
						System.Windows.Forms.Application.DoEvents();
					}

					txtLotIdUpload.Focus();
					txtLotIdUpload.Text = string.Empty;

					if (dgListStock.GetRowCount() <= 0)
					{
						Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
					}
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
				}
				finally
				{
					HiddenLoadingIndicator();

					e.Handled = true;
				}
			}
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



	}
}
