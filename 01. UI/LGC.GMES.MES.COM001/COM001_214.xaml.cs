/*************************************************************************************
 Created Date : 2018.01.25
      Creator : J.S HONG
   Decription : 재고 조사(소형파우치) < 특이작업
                1. 수정시 COM001_011-재고조사, COM001_125-재고조사(활성화), COM001_214-재고조사(소형파우치)화면을 고려해 주세요.
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.25  J.S HONG : Initial Created.
  2021.05.03  장희만     C20220501-000014        조회옵션 PROCESS, LINE 선택사항 조건에 누락되어 추가
  2024.06.10  윤지해     NERP 대응 프로젝트      차수마감취소(1차만) 버튼 추가 및 차수 추가(2차 이후) 시 ERP 실적 마감 FLAG 확인 후 생성 가능하도록 변경
  2024.08.02  Addumairi  NERP 대응 프로젝트      NERP 회계 마감여부에 따라 SNAP 재고 수정 제한, 차수추가 제한, 마감취소 제한 (GDC)
  2024.09.04  윤지해     NERP 대응 프로젝트      차수마감 취소 시 NERP 회계마감 여부 재확인
  2025.05.27  김영택     NERP 대응               [NERP 대응]: 제외/제외취소/선택재고변경 시 차수마감여부 재조회 하도록 수정 (2025.06.23 git 머지)
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
using System.Linq;

namespace LGC.GMES.MES.COM001
{
	public partial class COM001_214 : UserControl, IWorkArea
	{
		#region Declaration & Constructor 
		Util _Util = new Util();

		public COM001_214()
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
		private string _sDistType = string.Empty;
		private string _sPrjtName = string.Empty;
		private string _sNerpCloseFlag = string.Empty;  // 2024.06.10 윤지해 NERP 회계 마감 체크
		private string _sMaxSeq = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 조회
		private string _sMaxSeqCmplFlag = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 마감상태 조회
		private string _sNerpApplyFlag = string.Empty;  // 2024.06.10 윤지해 NERP 적용 동 조회

        // 조회 조건 저장 용도 (전산재고)
        //  E20250310-000144      NERP 대응 프로젝트 : 제외/제외취소/선택재고변경 시 차수마감여부 재조회 하도록 수정 
        private string _sDgNerpCloseFlagShot = string.Empty;
        private string _sDgMesAreaShot = string.Empty;
        private string _sDgMesSeqShot = string.Empty;
        private string _sDgMesYMShot = string.Empty;

        // 조회조건 저장 용도 (실사재고) 
        private string _sDgMesAreaStock = string.Empty;
        private string _sDgMesSeqStock = string.Empty;
        private string _sDgMesYMStock = string.Empty;

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

			// 물류구분
			String[] sFilterDistType = { "", "DIST_TYPE_CODE" };
			_combo.SetCombo(cboDistTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterDistType, sCase: "COMMCODES");
			_combo.SetCombo(cboDistTypeUpload, CommonCombo.ComboStatus.ALL, sFilter: sFilterDistType, sCase: "COMMCODES");
			_combo.SetCombo(cboDistTypeCompare, CommonCombo.ComboStatus.ALL, sFilter: sFilterDistType, sCase: "COMMCODES");

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

            if(_sNerpCloseFlag.Equals("N"))
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
					COM001_011_STOCKCNT_START wndSTOCKCNT_START = new COM001_011_STOCKCNT_START();
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
				COM001_011_STOCKCNT_START window = sender as COM001_011_STOCKCNT_START;
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

					SetListShot();
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
			SetListShot();
		}
		#endregion

		#region 재고조사 조회
		private void btnSearchStock_Click(object sender, RoutedEventArgs e)
		{
			SetListStock();
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
			_sDistType = Util.ConvertEmptyToNull(Util.GetCondition(cboDistTypeCompare));
			_sPrjtName = Util.ConvertEmptyToNull(txtPrjtNameCompare.Text);

			//Summary 조회
			SetListCompare(_sProdID, _sProcID, _sDistType, _sEqsgID, _sPrjtName);

			//Detail 조회
			SetListCompareDetail(_sProdID, _sProcID, null, _sDistType, _sEqsgID, _sPrjtName);
			chkDetailAll.IsChecked = true;
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

		private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton rb = sender as RadioButton;

			//최초 체크시에만 로직 타도록 구현
			if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
			{
				string sPRODID = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID"));
				string sPROCID = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PROCID"));
				string sMKT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MKT_TYPE_CODE"));

				//체크시 처리될 로직
				SetListCompareDetail(sPRODID, sPROCID, sMKT_TYPE_CODE, _sDistType, _sEqsgID, _sPrjtName);
				chkDetailAll.IsChecked = false;

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
					if (e.Cell.Row.Type == DataGridRowType.Item)
					{
						string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));
						if (e.Cell.Presenter != null && sCheck.Equals("NG"))
						{
							string[] Col = { "SNAP_CNT", "SNAP_SUM", "REAL_CNT", "REAL_SUM", "DIFF_CNT", "DIFF_SUM" };
							foreach (string column in Col)
							{
								if (column == e.Cell.Column.Name)
								{
									e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
									break;
								}
							}
						}


						//if (e.Cell.Presenter != null && sCheck.Equals("NG"))
						//{
						//    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["REAL_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["REAL_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["DIFF_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["DIFF_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
						//}
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
					if (e.Cell.Row.Type == DataGridRowType.Item)
					{
						//전산재고와 실물 수량이 맞지않으면 Yellow
						string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));

						if (e.Cell.Presenter != null && sCheck.Equals("NG"))
						{
							string[] Col = { "SNAP_PROCNAME", "SNAP_WIPQTY", "REAL_PROCNAME", "REAL_WIPQTY", "DIFF_WIPQTY" };
							foreach (string column in Col)
							{
								if (column == e.Cell.Column.Name)
								{
									e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
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

		private void dgListCompareDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			if (sender == null)
				return;

			C1DataGrid dataGrid = sender as C1DataGrid;

			dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter != null)
				{
					if (e.Cell.Row.Type == DataGridRowType.Item)
					{
						e.Cell.Presenter.Background = null;
						e.Cell.Presenter.FontWeight = FontWeights.Normal;
						e.Cell.Presenter.Foreground = dgListCompareDetail.Foreground;
					}
				}
			}));
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
				// 동일한 물류단위만 전체 선택 가능하도록
				if (dgListShot.GetRowCount() > 0)
				{
					if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("DIST_TYPE_CODE <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "DIST_TYPE_CODE")) + "'").Length >= 1)
					{
						Util.MessageValidation("SFU4547"); //동일한 물류단위만 전체선택이 가능합니다.
						chkAll.IsChecked = false;
						return;
					}

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
					if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("CHK = 'True' AND DIST_TYPE_CODE <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[idx].DataItem, "DIST_TYPE_CODE")) + "'").Length >= 1)
					{
						Util.MessageValidation("SFU4548"); //동일한 물류단위만 선택이 가능합니다.
						DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", false);
						return;
					}

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
			int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

			if (iRow < 0)
			{
				Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
				return;
			}

            // 2024.08.02 Addumairi Validation for NERP Accounting Closed so can't modify SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정

            string[] sAttrbute = { "Y" };
            // ChkNerpFlag(ldpMonthShot);

            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
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
		#endregion

		#region 전산재고 재공제외취소
		private void btnExclude_SNAP_Cancel_Click(object sender, RoutedEventArgs e)
		{
			int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

			if (iRow < 0)
			{
				Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
				return;
			}

            // 2024.08.02 Addumairi Validation for NERP Accounting Closed so can't cancel modify/reset SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
            //if (_sSTCK_CNT_CMPL_FLAG.Equals("Y")) 
            string[] sAttrbute = { "Y" };

            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
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
		#endregion

		#region 전산재고 선택재고변경
		private void btnRowReSet_Click(object sender, RoutedEventArgs e)
		{
			int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");
            if (iRow < 0)
			{
				Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
				return;
			}

            // 2024.08.02 Addumairi Validation for NERP Accounting Closed so can't reset SNAP Inventory
            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
            string[] sAttrbute = { "Y" };

            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
				return;
			}

            if(ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
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
		#endregion

		#region 재고실사 재공제외
		private void btnExclude_RSLT_Click(object sender, RoutedEventArgs e)
		{
			int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

			if (iRow < 0)
			{
				Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
				return;
			}

            // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
            if (ChkCmplFlag(_sDgMesYMStock, _sDgMesAreaStock, _sDgMesSeqStock))
            {
				Util.MessageValidation("SFU3499"); //마감된 차수입니다.
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
		#endregion

		#region 재고실사 재공 정보변경
		private void btnStckCntRslt_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

				if (iRow < 0)
				{
					Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
					return;
				}

                // 2024.06.10 윤지해 차수마감일 경우 제외 안되도록 수정
                if (ChkCmplFlag(_sDgMesYMStock, _sDgMesAreaStock, _sDgMesSeqStock))
                {
					Util.MessageValidation("SFU3499"); //마감된 차수입니다.
					return;
				}

				COM001_011_STOCKCNT_RSLT wndSTOCKCNT_RSLT = new COM001_011_STOCKCNT_RSLT();
				wndSTOCKCNT_RSLT.FrameOperation = FrameOperation;

				if (wndSTOCKCNT_RSLT != null)
				{
					DataTable dtRSLT = DataTableConverter.Convert(dgListStock.ItemsSource);
					DataRow[] drRSLT = dtRSLT.Select(" CHK = 'True' ");

					object[] Parameters = new object[5];
					Parameters[0] = "MCP";
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

		private void wndSTOCKCNT_RSLT_Closed(object sender, EventArgs e)
		{
			try
			{
				COM001_011_STOCKCNT_RSLT window = sender as COM001_011_STOCKCNT_RSLT;
				if (window.DialogResult == MessageBoxResult.OK)
				{
					SetListStock();
					Util.MessageInfo("SFU1275");//정상처리 되었습니다.
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 전산재고 ScanidLable 
		private void cboDistTypeShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			if (cboDistTypeShot.ItemsSource != null)
			{
				string[] sDistType = cboDistTypeShot.Text.Split(':');

				if (sDistType.Length > 1)
				{
					tblLotIdShot.Text = string.Format("{0} {1}", sDistType[1].Trim(), "ID");
				}
				else
				{
					tblLotIdShot.Text = string.Format("{0} {1}", "SCAN", "ID");
				}
			}
		}
		#endregion

		#region 재고실사 ScanidLable
		private void cboDistTypeUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			if (cboDistTypeUpload.ItemsSource != null)
			{
				string[] sDistType = cboDistTypeUpload.Text.Split(':');

				if (sDistType.Length > 1)
				{
					tblLotIdUpload.Text = string.Format("{0} {1}", sDistType[1].Trim(), "ID");
				}
				else
				{
					tblLotIdUpload.Text = string.Format("{0} {1}", "SCAN", "ID");
				}
			}

		}
		#endregion

		#region Inbox List 조회 선택
		private void dgListShotChoice_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton rb = sender as RadioButton;

			//최초 체크시에만 로직 타도록 구현
			if (DataTableConverter.GetValue(rb.DataContext, "CHO").Equals("False"))
			{
				//Inbox List
				SetListShotCartDetail(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "LOTID")));

				//선택값 셋팅
				foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
				{
					DataTableConverter.SetValue(row.DataItem, "CHO", "False");
				}

				DataTableConverter.SetValue(rb.DataContext, "CHO", "True");
				//row 색 바꾸기
				((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

			}
		}
		#endregion

		#region 재고비교 - 재고비교 Detail 전체표시
		private void chkDetailAll_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				dgListCompareDetail.Columns["AREANAME"].Visibility = Visibility.Visible;
				dgListCompareDetail.Columns["EQSGNAME"].Visibility = Visibility.Visible;
				dgListCompareDetail.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
				dgListCompareDetail.Columns["PRODID"].Visibility = Visibility.Visible;
				dgListCompareDetail.Columns["MKT_TYPE_NAME"].Visibility = Visibility.Visible;
			}
			catch
			{ }
		}

		private void chkDetailAll_Unchecked(object sender, RoutedEventArgs e)
		{
			try
			{
				dgListCompareDetail.Columns["AREANAME"].Visibility = Visibility.Collapsed;
				dgListCompareDetail.Columns["EQSGNAME"].Visibility = Visibility.Collapsed;
				dgListCompareDetail.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
				dgListCompareDetail.Columns["PRODID"].Visibility = Visibility.Collapsed;
				dgListCompareDetail.Columns["MKT_TYPE_NAME"].Visibility = Visibility.Collapsed;
			}
			catch
			{ }
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
		private void SetListShot()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;
				Util.gridClear(dgListShotCartDetail);

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("DIST_TYPE_CODE", typeof(string));

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

				dr["DIST_TYPE_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboDistTypeShot));
				if (string.IsNullOrEmpty(txtLotIdShot.Text.Trim()))
				{
					int iEqsgShotItemCnt = (cboEqsgShot.ItemsSource == null ? 0 : ((DataView)cboEqsgShot.ItemsSource).Count);
					int iEqsgShotSelectedCnt = cboEqsgShot.SelectedItemsToString.Split(',').Length;
					int iProcShotItemCnt = (cboProcShot.ItemsSource == null ? 0 : ((DataView)cboProcShot.ItemsSource).Count);
					int iProcShotSelectedCnt = cboProcShot.SelectedItemsToString.Split(',').Length;

					dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgShot.SelectedItemsToString);
					dr["PROCID"] = Util.ConvertEmptyToNull(cboProcShot.SelectedItemsToString);
					dr["PRODID"] = Util.ConvertEmptyToNull(txtProdShot.Text);
					dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameShot.Text);
					dr["STCK_CNT_EXCL_FLAG"] = Util.ConvertEmptyToNull(Util.GetCondition(cboExclFlagShot));
				}
				else
				{
					dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdShot.Text);
				}

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MESSTOCK_MCP", "RQSTDT", "RSLTDT", RQSTDT);
				Util.GridSetData(dgListShot, dtRslt, FrameOperation);

                // 2025.03.10 윤지해 NERP 회계마감여부 체크
                _sDgNerpCloseFlagShot = _sNerpCloseFlag;

                // 2025.03.10 윤지해 조회한 데이터의 동/기준월/차수 데이터 저장
                _sDgMesAreaShot = Util.GetCondition(cboAreaShot);
                _sDgMesYMShot = Util.GetCondition(ldpMonthShot);
                _sDgMesSeqShot = Util.GetCondition(cboStockSeqShot);
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

		#region 전산재고 조회(대차상세정보)
		private void SetListShotCartDetail(string sCART_ID)
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("CTNR_ID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["CTNR_ID"] = sCART_ID;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "RQSTDT", "RSLTDT", RQSTDT);
				Util.GridSetData(dgListShotCartDetail, dtRslt, FrameOperation);
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
		private void SetListStock()
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
				RQSTDT.Columns.Add("SCAN_TRGT_TYPE", typeof(string));
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

				dr["SCAN_TRGT_TYPE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboDistTypeUpload));
				if (string.IsNullOrEmpty(txtLotIdUpload.Text.Trim()))
				{
					int iEqsgUploadItemCnt = (cboEqsgUpload.ItemsSource == null ? 0 : ((DataView)cboEqsgUpload.ItemsSource).Count);
					int iEqsgUploadSelectedCnt = cboEqsgUpload.SelectedItemsToString.Split(',').Length;
					int iProcUploadItemCnt = (cboProcUpload.ItemsSource == null ? 0 : ((DataView)cboProcUpload.ItemsSource).Count);
					int iProcUploadSelectedCnt = cboProcUpload.SelectedItemsToString.Split(',').Length;

					dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgUpload.SelectedItemsToString);
					dr["PROCID"] = Util.ConvertEmptyToNull(cboProcUpload.SelectedItemsToString);
					dr["PRODID"] = Util.ConvertEmptyToNull(txtProdUpload.Text);
					dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameUpload.Text);
				}
				else
				{
					dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdUpload.Text);
				}

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INFO_MCP", "RQSTDT", "RSLTDT", RQSTDT);
				Util.GridSetData(dgListStock, dtRslt, FrameOperation);

                // 2025.03.10 윤지해 조회한 데이터의 동/기준월/차수 데이터 저장
                _sDgMesAreaStock = Util.GetCondition(cboAreaUpload);
                _sDgMesYMStock = Util.GetCondition(ldpMonthUpload);
                _sDgMesSeqStock = Util.GetCondition(cboStockSeqUpload);
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

		#region 재고비교 SUMMAY 조회
		private void SetListCompare(string sProdID, string sProcID, string sDistType = null, string sEqsgID = null, string sPrjtName = null)
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
				RQSTDT.Columns.Add("DIST_TYPE_CODE", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("PRODID", typeof(string));
				RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
				RQSTDT.Columns.Add("SCAN_TRGT_TYPE", typeof(string));

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

				dr["DIST_TYPE_CODE"] = sDistType;
				dr["EQSGID"] = sEqsgID;
				dr["PROCID"] = sProcID;
				dr["PRODID"] = sProdID;
				dr["PRJT_NAME"] = sPrjtName;
				dr["SCAN_TRGT_TYPE"] = sDistType;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE_MCP", "RQSTDT", "RSLTDT", RQSTDT);
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

		private void SetListCompareDetail(string sProdID, string sProcID, string sMKT_TYPE_CODE, string sDistType = null, string sEqsgID = null, string sPrjtName = null)
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
				RQSTDT.Columns.Add("DIST_TYPE_CODE", typeof(string));
				RQSTDT.Columns.Add("MKT_TYPE_CODE", typeof(string));

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

				dr["EQSGID"] = Util.ConvertEmptyToNull(sEqsgID);
				dr["PROCID"] = Util.ConvertEmptyToNull(sProcID);
				dr["PRODID"] = sProdID;
				dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(sDistType);
				dr["PRJT_NAME"] = Util.ConvertEmptyToNull(sPrjtName);
				dr["DIST_TYPE_CODE"] = Util.ConvertEmptyToNull(sDistType);
				dr["MKT_TYPE_CODE"] = sMKT_TYPE_CODE;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE_DETAIL_MCP", "RQSTDT", "RSLTDT", RQSTDT);
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
				if (dgListStock.GetRowCount() < 1)
				{
					Util.MessageValidation("SFU2946"); //재고조사 파일을 먼저 선택 해 주세요.
					return;
				}

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

				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
				RQSTDT.Columns.Add("LOTID", typeof(string));
				RQSTDT.Columns.Add("NOTE", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));

				DataRowView rowview = null;

				foreach (C1.WPF.DataGrid.DataGridRow row in dgListStock.Rows)
				{
					rowview = row.DataItem as DataRowView;

					if (!String.IsNullOrEmpty(rowview["LOTID"].ToString()))
					{
						DataRow dr = RQSTDT.NewRow();
						dr["LANGID"] = LoginInfo.LANGID;
						dr["STCK_CNT_YM"] = sMonth;
						dr["AREAID"] = sArea;
						dr["STCK_CNT_SEQNO"] = iCnt;
						dr["NOTE"] = "SFU";
						dr["LOTID"] = rowview["LOTID"].ToString();
						dr["USERID"] = LoginInfo.USERID;
						dr["SCAN_TYPE"] = Util.GetCondition(cboDistTypeShot);

						RQSTDT.Rows.Add(dr);
					}
				}

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_PDA_SCAN_MCP", "RQSTDT", "RSLTDT", RQSTDT);
				//DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EXCEL_UPLOAD", "RQSTDT", "RSLTDT", RQSTDT);

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

				int iSTCK_CNT_SNAP_CTNR_CNT = dtRSLT.Select("CHK = 'True' AND DIST_TYPE_CODE = 'CART'").Length;
				string sbizRuleID = iSTCK_CNT_SNAP_CTNR_CNT >= 1 ? "DA_PRD_UPD_STCK_CNT_SNAP_CTNR" : "DA_PRD_UPD_STCK_CNT_SNAP";

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

				new ClientProxy().ExecuteService_Multi(sbizRuleID, "RQSTDT", null, (bizResult, bizException) =>
				{
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");//정상처리되었습니다.

						SetListShot();
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
				RQSTDT.Columns.Add("PROCID", typeof(string));
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
						dr["PROCID"] = dtRSLT.Rows[i]["PROCID"].ToString();
						dr["LOTID"] = dtRSLT.Rows[i]["SCAN_ID"].ToString();
						dr["REAL_WIP_QTY"] = string.IsNullOrEmpty(dtRSLT.Rows[i]["WIP_QTY"].ToString()) ? 0 : Convert.ToDecimal(dtRSLT.Rows[i]["WIP_QTY"].ToString());
						dr["USERID"] = LoginInfo.USERID;
						dr["USE_FLAG"] = "N";

						RQSTDT.Rows.Add(dr);
					}
				}

				loadingIndicator.Visibility = Visibility.Visible;

				new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_RSLT_MCP", "RQSTDT", null, (bizResult, bizException) =>
				{
					try
					{
						if (bizException != null)
						{
							Util.MessageException(bizException);
							return;
						}

						Util.MessageInfo("SFU1275");//정상처리되었습니다.

						SetListStock();
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
				RQSTDT.Columns.Add("CTNR_ID", typeof(string));
				RQSTDT.Columns.Add("USERID", typeof(string));
				string sLOTID = string.Empty;

				for (int i = 0; i < dtRSLT.Rows.Count; i++)
				{
					if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
					{
						sLOTID = dtRSLT.Rows[i]["DIST_TYPE_CODE"].ToString().Equals("CART") ? "CTNR_ID" : "LOTID";
						DataRow dr = RQSTDT.NewRow();
						dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
						dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
						dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
						dr[sLOTID] = dtRSLT.Rows[i]["LOTID"].ToString();
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

						SetListShot();
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

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

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

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_EQSG", "RQSTDT", "RSLTDT", RQSTDT);

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
				// 2024.08.02 Addumairi Change to reduce minus 1 month for input in DA
				DateTime prevMonth = DateTime.Now.AddMonths(-1);

				if (ldpMonthShot.SelectedDateTime.Year > DateTime.MinValue.Year || ldpMonthShot.SelectedDateTime.Month > DateTime.MinValue.Month)
				{
					prevMonth = ldpMonthShot.SelectedDateTime.AddMonths(-1);
				}

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("CLOSE_YM", typeof(string));
				RQSTDT.Columns.Add("SHOPID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
				// dr["CLOSE_YM"] = Util.GetCondition(ldpMonthShot);
				dr["CLOSE_YM"] = prevMonth.ToString("yyyyMM");

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

        #region ChkCmplFlag : 마감여부 조회 로직 
        private bool ChkCmplFlag(string sStckYm, string sStckArea, string sSeqno)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = sStckYm;
                dr["AREAID"] = sStckArea;
                dr["STCK_CNT_SEQNO"] = sSeqno;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_ORD", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Rows[0]["STCK_CNT_CMPL_FLAG"].ToString().Equals("Y"))
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

        #endregion

        private void tabStckCnt_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				//if(tabStckCnt.SelectedIndex == 0)
				//{ SetProcessCombo(cboProcShot, cboAreaShot, cboStckCntGrShot); }
				//else if(tabStckCnt.SelectedIndex == 1)
				//{ SetProcessCombo(cboProcUpload, cboAreaUpload, cboStckCntGrUpload); }
				//else if(tabStckCnt.SelectedIndex == 2)
				//{ SetProcessCombo(cboProcCompare, cboAreaCompare, cboStckCntGrCompare); }
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}


	}
}
