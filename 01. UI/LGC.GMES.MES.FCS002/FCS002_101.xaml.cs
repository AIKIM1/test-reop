/*************************************************************************************
 Created Date : 2021.01.13
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 활성화 : 재고 실사
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  조영대 : Initial Created.
  2021.04.01  조영대 : 공정그룹 콤보박스 추가
  2024.06.10  윤지해 : NERP 대응 프로젝트      차수마감취소(1차만) 버튼 추가 및 차수 추가(2차 이후) 시 ERP 실적 마감 FLAG 확인 후 생성 가능하도록 변경
  2024.08.02  Faiz   : NERP 대응 프로젝트      NERP 회계 마감여부에 따라 차수추가 제한, 마감취소 제한 (GDC)
  2024.09.04  윤지해 : NERP 대응 프로젝트      차수마감 취소 시 NERP 회계마감 여부 재확인
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;

namespace LGC.GMES.MES.FCS002
{
	public partial class FCS002_101 : UserControl, IWorkArea
	{
		#region Declaration & Constructor 
		Util _Util = new Util();

		public FCS002_101()
		{
			InitializeComponent();
		}

		public IFrameOperation FrameOperation
		{
			get;
			set;
		}

		CommonCombo _combo = new CommonCombo();

		DataView _dvSTCKCNT { get; set; }

		string _sSTCK_CNT_CMPL_FLAG = string.Empty;
		string _sNerpCloseFlag = string.Empty;  // 2024.06.10 윤지해 NERP 회계 마감 체크
		string _sMaxSeq = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 조회
		string _sMaxSeqCmplFlag = string.Empty;  // 2024.06.10 윤지해 재고실사 년월, 동에 따른 마지막 차수 마감상태 조회
		string _sNerpApplyFlag = string.Empty;  // 2024.06.10 윤지해 NERP 적용 동 조회

		SolidColorBrush captionBrush = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
		SolidColorBrush allBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 153));
		SolidColorBrush surveyBrush = new SolidColorBrush(Color.FromArgb(255, 225, 255, 225));
		SolidColorBrush realBrush = new SolidColorBrush(Colors.Gray);
		SolidColorBrush infoBrush = new SolidColorBrush(Colors.White);

		#endregion

		#region Initialize
		private void InitControls()
		{
			InitCombo();

			InitGrid();

			ClearControls();

			rdoProcessStd.IsChecked = true;

			// 2024.06.10 윤지해 차수 마감 취소, 차수마감 버튼 VISIBLE 처리
			ChkNerpApplyFlag();

			ShowBtnCloseCancel(DateTime.Today.ToString("yyyyMM"));
		}

		/// <summary>
		/// 화면내 combo 셋팅
		/// </summary>
		private void InitCombo()
		{

			//String[] sFilterStock = { LoginInfo.CFG_AREA_ID, DateTime.Today.ToString("yyyyMM"), "N" };
			String[] sFilterStock = { LoginInfo.CFG_AREA_ID, DateTime.Today.ToString("yyyyMM"), null };    // 2024.06.10  윤지해 차수마감 취소 기능 추가로 마감되지 않은 차수만 보여주는 부분 삭제. 공백으로 오류 발생함
			_combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", sFilter: sFilterStock);

			SetLineCombo();

			cboProcGroup.SetCommonCode("PROC_GR_CODE", CommonCombo.ComboStatus.ALL, false);

			SetProcessCombo();

			SetStorageLocCombo();
		}

		private void InitGrid()
		{
			DataTable dtSummary = new DataTable();
			dtSummary.Columns.Add("ALL", typeof(string));
			dtSummary.Columns.Add("ALL_LOT", typeof(string));
			dtSummary.Columns.Add("ALL_CNT", typeof(string));
			dtSummary.Columns.Add("COMPUTE", typeof(string));
			dtSummary.Columns.Add("COMPUTE_LOT", typeof(string));
			dtSummary.Columns.Add("COMPUTE_CNT", typeof(string));
			dtSummary.Columns.Add("PDA", typeof(string));
			dtSummary.Columns.Add("PDA_LOT", typeof(string));
			dtSummary.Columns.Add("PDA_CNT", typeof(string));
			dtSummary.Columns.Add("REAL", typeof(string));
			dtSummary.Columns.Add("REAL_LOT", typeof(string));
			dtSummary.Columns.Add("REAL_CNT", typeof(string));
			dtSummary.Columns.Add("INFO", typeof(string));
			dtSummary.Columns.Add("INFO_LOT", typeof(string));
			dtSummary.Columns.Add("INFO_CNT", typeof(string));

			DataRow drNew = dtSummary.NewRow();
			drNew["ALL"] = ObjectDic.Instance.GetObjectName("ALL_DUE_DILIGENCE_TARGET");
			drNew["ALL_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
			drNew["ALL_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
			drNew["COMPUTE"] = ObjectDic.Instance.GetObjectName("COMPUTE_AUTO_DUE_DILIGENCE");
			drNew["COMPUTE_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
			drNew["COMPUTE_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
			drNew["PDA"] = ObjectDic.Instance.GetObjectName("PDA");
			drNew["PDA_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
			drNew["PDA_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
			drNew["REAL"] = ObjectDic.Instance.GetObjectName("REAL_UNCONFIRMED");
			drNew["REAL_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
			drNew["REAL_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
			drNew["INFO"] = ObjectDic.Instance.GetObjectName("INFO_UNCONFIRMED");
			drNew["INFO_LOT"] = ObjectDic.Instance.GetObjectName("LOT_CNT");
			drNew["INFO_CNT"] = ObjectDic.Instance.GetObjectName("QTY");
			dtSummary.Rows.Add(drNew);

			drNew = dtSummary.NewRow();
			drNew["ALL"] = ObjectDic.Instance.GetObjectName("ALL_DUE_DILIGENCE_TARGET");
			drNew["ALL_LOT"] = "-";
			drNew["ALL_CNT"] = "-";
			drNew["COMPUTE"] = ObjectDic.Instance.GetObjectName("COMPUTE_AUTO_DUE_DILIGENCE");
			drNew["COMPUTE_LOT"] = "-";
			drNew["COMPUTE_CNT"] = "-";
			drNew["PDA"] = ObjectDic.Instance.GetObjectName("PDA");
			drNew["PDA_LOT"] = "-";
			drNew["PDA_CNT"] = "-";
			drNew["REAL"] = ObjectDic.Instance.GetObjectName("REAL_UNCONFIRMED");
			drNew["REAL_LOT"] = "-";
			drNew["REAL_CNT"] = "-";
			drNew["INFO"] = ObjectDic.Instance.GetObjectName("INFO_UNCONFIRMED");
			drNew["INFO_LOT"] = "-";
			drNew["INFO_CNT"] = "-";
			dtSummary.Rows.Add(drNew);

			dgSumShot.SetItemsSource(dtSummary, FrameOperation, false);

		}
		#endregion

		#region Event
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this)) return;

			InitControls();

			ApplyPermissions();

			Loaded -= new RoutedEventHandler(UserControl_Loaded);
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
            ChkNerpFlag(DateTime.Today.ToString("yyyyMM"));

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
                ShowBtnCloseCancel(DateTime.Today.ToString("yyyyMM"));
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
				ChkNerpFlag(DateTime.Today.ToString("yyyyMM"));

				string[] sAttrbute = { "Y" };

				if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sNerpCloseFlag.Equals("N"))
				{
					Util.MessageValidation("SFU3686");  // NERP 회계 마감기간 중 차수 추가를 할 수 없습니다.
					return;
				}
				else
				{
					FCS002_101_STOCKCNT_START wndSTOCKCNT_START = new FCS002_101_STOCKCNT_START();
					wndSTOCKCNT_START.FrameOperation = FrameOperation;

					if (wndSTOCKCNT_START != null)
					{
						object[] Parameters = new object[6];
						Parameters[0] = LoginInfo.CFG_AREA_ID;
						Parameters[1] = DateTime.Today;

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
				FCS002_101_STOCKCNT_START window = sender as FCS002_101_STOCKCNT_START;
				if (window.DialogResult == MessageBoxResult.OK)
				{
					_combo.SetCombo(cboStockSeqShot);

					Util.gridClear(dgListShot);

					SetListShot(false);
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
			SetListShot(false);
		}
		#endregion


		#region 동/기준월/차수별  Note설정
		private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			_dvSTCKCNT = cboStockSeqShot.ItemsSource as DataView;

			string sStckCntSeq = cboStockSeqShot.Text;
			if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
			{
				_dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
				_sSTCK_CNT_CMPL_FLAG = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();

				_dvSTCKCNT.RowFilter = null;
			}

			// 2024.06.10 윤지해 추가
			ShowBtnCloseCancel(DateTime.Today.ToString("yyyyMM"));
		}
		#endregion

		private void cboProcGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			SetProcessCombo();
		}

		private void cboProcShot_SelectionChanged(object sender, EventArgs e)
		{
			ClearControls();
		}

		private void cboEqsgShot_SelectionChanged(object sender, EventArgs e)
		{
			ClearControls();

			SetProcessCombo();
		}

		private void cboStorageLoc_SelectionChanged(object sender, EventArgs e)
		{
			ClearControls();
		}

		private void dgSumShot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			dgSumShot.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
				{
					return;
				}

				if (e.Cell.Row.Index == 0 &&
					(e.Cell.Column.Name.Contains("_LOT") || e.Cell.Column.Name.Contains("_CNT")))
				{
					e.Cell.Presenter.Background = captionBrush;
					e.Cell.Presenter.FontWeight = FontWeights.Bold;
				}
				else
				{
					if (e.Cell.Column.Name.Contains("ALL"))
					{
						e.Cell.Presenter.Background = allBrush;
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					if (e.Cell.Column.Name.Contains("COMPUTE"))
					{
						e.Cell.Presenter.Background = surveyBrush;
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					if (e.Cell.Column.Name.Contains("PDA"))
					{
						e.Cell.Presenter.Background = surveyBrush;
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					if (e.Cell.Column.Name.Contains("REAL"))
					{
						e.Cell.Presenter.Background = realBrush;
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}
					if (e.Cell.Column.Name.Contains("INFO"))
					{
						e.Cell.Presenter.Background = infoBrush;
						e.Cell.Presenter.FontWeight = FontWeights.Bold;
					}

					if (e.Cell.Column.Name.Contains("_LOT") || e.Cell.Column.Name.Contains("_CNT"))
					{
						e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
					}
				}
			}));
		}

		private void dgSummaryShot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			dgSummaryShot.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
				{
					return;
				}

				switch (e.Cell.Column.Name)
				{
					case "PROCNAME":
					case "EQSGNAME":
					case "LOT_CNT":
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
						break;
					default:
						e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
						break;
				}
			}));
		}

		private void dgListShot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
		{
			dgSummaryShot.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
				{
					return;
				}

				if (e.Cell.Column.Name.Equals("IS_HISTORY"))
				{
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
				}
				else
				{
					e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
				}


				if (Util.IsNVC(dgListShot.GetValue(e.Cell.Row.Index, "WORK_SURVEY_TYPE")))
				{
					e.Cell.Presenter.Background = realBrush;
				}
				else
				{
					e.Cell.Presenter.Background = surveyBrush;
				}
			}));
		}

		private void dgSummaryShot_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (dgSummaryShot.CurrentColumn == null || dgSummaryShot.CurrentRow == null ||
				dgSummaryShot.CurrentRow.Index < 0) return;

			object value1, value2;
			switch (dgSummaryShot.CurrentColumn.Name)
			{
				case "PROCNAME":
					value1 = dgSummaryShot.GetValue(dgSummaryShot.CurrentRow.Index, "PROCID");
					cboProcShot.UncheckAll();
					cboProcShot.Check(value1);
					break;
				case "EQSGNAME":
					value2 = dgSummaryShot.GetValue(dgSummaryShot.CurrentRow.Index, "EQSGID");
					cboEqsgShot.UncheckAll();
					cboEqsgShot.Check(value2);
					break;
				case "LOT_CNT":
					value1 = dgSummaryShot.GetValue(dgSummaryShot.CurrentRow.Index, "EQSGID");
					value2 = dgSummaryShot.GetValue(dgSummaryShot.CurrentRow.Index, "PROCID");

					if (Util.IsNVC(value1))
					{
						cboEqsgShot.CheckAll();
					}
					else
					{
						cboEqsgShot.UncheckAll();
						cboEqsgShot.Check(value1);
					}

					if (Util.IsNVC(value2))
					{
						cboProcShot.CheckAll();
					}
					else
					{
						cboProcShot.UncheckAll();
						cboProcShot.Check(value2);
					}

					break;
			}
			SetListShot(true);
		}

		private void dgListShot_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				if (!dgListShot.CurrentColumn.Name.Equals("IS_HISTORY")) return;
				if (!Util.NVC(dgListShot.GetValue(dgListShot.CurrentRow.Index, "IS_HISTORY")).Equals("Y")) return;

				loadingIndicator.Visibility = Visibility.Visible;

				if (dgListShot.CurrentRow != null)
				{
					switch (LoginInfo.CFG_SYSTEM_TYPE_CODE)
					{
						case "F":
							FCS002_021 wndTRAY = new FCS002_021();
							wndTRAY.FrameOperation = FrameOperation;

							object[] Parameters = new object[10];
							Parameters[1] = Util.NVC(dgListShot.GetValue(dgListShot.CurrentRow.Index, "LOTID")); //Tray No
							this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
							break;
						default:
							COM001.COM001_016 wndLotInfo = new COM001.COM001_016();
							wndLotInfo.FrameOperation = FrameOperation;

							object[] parameters = new object[1];
							parameters[0] = Util.NVC(dgListShot.GetValue(dgListShot.CurrentRow.Index, "LOTID"));
							FrameOperation.OpenMenu("SFU010745050", true, parameters);
							break;
					}
				}
				loadingIndicator.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}

		private void btnAutoStck_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("USERID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = DateTime.Today.ToString("yyyyMM");
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["USERID"] = LoginInfo.USERID;

				RQSTDT.Rows.Add(dr);

				new ClientProxy().ExecuteService("DA_SEL_STOCK_WORKING_SURVEY_AUTO", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
				{
					if (ex != null)
					{
						Util.MessageException(ex);
						return;
					}

					if (result != null && result.Rows.Count == 1 && Util.NVC(result.Rows[0]["RESULT"]).Equals("OK"))
					{
						SetListShot(false);
					}

					loadingIndicator.Visibility = Visibility.Collapsed;
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

		private void ApplyPermissions()
		{
			List<Button> listAuth = new List<Button>
			{
				btnDegreeAdd,
				btnDegreeClose,
				btnAutoStck
			};

			Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
		}

		private void ClearControls()
		{
			dgSummaryShot.ClearRows();
			dgListShot.ClearRows();

			dgSumShot.SetValue(1, "ALL_LOT", "-");
			dgSumShot.SetValue(1, "ALL_CNT", "-");
			dgSumShot.SetValue(1, "COMPUTE_LOT", "-");
			dgSumShot.SetValue(1, "COMPUTE_CNT", "-");
			dgSumShot.SetValue(1, "PDA_LOT", "-");
			dgSumShot.SetValue(1, "PDA_CNT", "-");
			dgSumShot.SetValue(1, "REAL_LOT", "-");
			dgSumShot.SetValue(1, "REAL_CNT", "-");
			dgSumShot.SetValue(1, "INFO_LOT", "-");

			txtSnapMakeDate.Text = "★ " + ObjectDic.Instance.GetObjectName("CREATE_INVENTORY_SNAPSHOT") + " : ";
			txtActualRate.Text = "0 %";
		}

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
				dr["STCK_CNT_YM"] = DateTime.Today.ToString("yyyyMM");
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["USERID"] = LoginInfo.USERID;

				if (dr["STCK_CNT_SEQNO"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);

				_combo.SetCombo(cboStockSeqShot);

				// 2024.06.10 윤지해 추가
				ShowBtnCloseCancel(DateTime.Today.ToString("yyyyMM"));
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 재고 조회
		private void SetListShot(bool isDoubleClick)
		{
			try
			{
				//차수는필수입니다.
				if (Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) return;

				int iEqsgShotItemCnt = (cboEqsgShot.ItemsSource == null ? 0 : ((DataView)cboEqsgShot.ItemsSource).Count);
				int iEqsgShotSelectedCnt = (cboEqsgShot.ItemsSource == null ? 0 : cboEqsgShot.SelectedItemsToString.Split(',').Length);
				int iProcShotItemCnt = (cboProcShot.ItemsSource == null ? 0 : ((DataView)cboProcShot.ItemsSource).Count);
				int iProcShotSelectedCnt = (cboProcShot.ItemsSource == null ? 0 : cboProcShot.SelectedItemsToString.Split(',').Length);
				int iStorageLocItemCnt = (cboStorageLoc.ItemsSource == null ? 0 : ((DataView)cboStorageLoc.ItemsSource).Count);
				int iStorageLocSelectedCnt = (cboStorageLoc.ItemsSource == null ? 0 : cboStorageLoc.SelectedItemsToString.Split(',').Length);

				string bizRuleName = "DA_SEL_STOCK_WORKING_SURVEY_SUMMARY";
				if (!isDoubleClick &&
					iEqsgShotItemCnt.Equals(iEqsgShotSelectedCnt) && iProcShotItemCnt.Equals(iProcShotSelectedCnt))
				{
					dgSummaryShot.Visibility = Visibility.Visible;
					dgListShot.Visibility = Visibility.Collapsed;

					bizRuleName = "DA_SEL_STOCK_WORKING_SURVEY_SUMMARY";
				}
				else
				{
					dgSummaryShot.Visibility = Visibility.Collapsed;
					dgListShot.Visibility = Visibility.Visible;

					bizRuleName = "DA_SEL_STOCK_WORKING_SURVEY_DETAIL";
				}

				loadingIndicator.Visibility = Visibility.Visible;

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
				RQSTDT.Columns.Add("POSITN", typeof(string));
				RQSTDT.Columns.Add("IS_NO_CONFIRM", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["STCK_CNT_YM"] = DateTime.Today.ToString("yyyyMM");
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;


				dr["EQSGID"] = (iEqsgShotItemCnt == iEqsgShotSelectedCnt ? null : Util.ConvertEmptyToNull(cboEqsgShot.SelectedItemsToString));
				dr["PROCID"] = (iProcShotItemCnt == iProcShotSelectedCnt ? null : Util.ConvertEmptyToNull(cboProcShot.SelectedItemsToString));
				dr["POSITN"] = (iStorageLocItemCnt == iStorageLocSelectedCnt ? null : Util.ConvertEmptyToNull(cboStorageLoc.SelectedItemsToString));
				dr["IS_NO_CONFIRM"] = chkUnconfirmedSearch.IsChecked.Equals(true) ? "Y" : null;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STOCK_WORKING_SURVEY", "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					DataRow selectRow = dtRslt.Rows[0];
					dgSumShot.SetValue(1, "ALL_LOT", Util.NVC_Int(selectRow["LOT_ALL_CNT"]));
					dgSumShot.SetValue(1, "ALL_CNT", Util.NVC_Int(selectRow["SUM_ALL_QTY"]));
					dgSumShot.SetValue(1, "COMPUTE_LOT", Util.NVC_Int(selectRow["LOT_AUTO_CNT"]));
					dgSumShot.SetValue(1, "COMPUTE_CNT", Util.NVC_Int(selectRow["SUM_AUTO_QTY"]));
					dgSumShot.SetValue(1, "PDA_LOT", Util.NVC_Int(selectRow["LOT_PDA_CNT"]));
					dgSumShot.SetValue(1, "PDA_CNT", Util.NVC_Int(selectRow["SUM_PDA_QTY"]));
					dgSumShot.SetValue(1, "REAL_LOT", Util.NVC_Int(selectRow["LOT_REAL_CNT"]));
					dgSumShot.SetValue(1, "REAL_CNT", Util.NVC_Int(selectRow["SUM_REAL_QTY"]));
					dgSumShot.SetValue(1, "INFO_LOT", Util.NVC_Int(selectRow["LOT_INFO_CNT"]));

					if (Util.NVC_Int(selectRow["LOT_ALL_CNT"]) > 0)
					{
						decimal workingSubey = Util.NVC_Decimal(selectRow["LOT_AUTO_CNT"]) / Util.NVC_Decimal(selectRow["LOT_ALL_CNT"]) * 100;
						txtActualRate.Text = workingSubey.ToString("#,##0.00") + " %";
					}
					else
					{
						txtActualRate.Text = "0 %";
					}
					txtSnapMakeDate.Text = "★ " + ObjectDic.Instance.GetObjectName("CREATE_INVENTORY_SNAPSHOT") + " : " + Util.NVC(selectRow["SNAP_DTTM"]);
				}
				else
				{
					txtActualRate.Text = string.Empty;
					txtSnapMakeDate.Text = "★ " + ObjectDic.Instance.GetObjectName("CREATE_INVENTORY_SNAPSHOT") + " : ";
				}

				dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
				if (dtRslt != null && dtRslt.Rows.Count > 0)
				{
					if (bizRuleName.Equals("DA_SEL_STOCK_WORKING_SURVEY_SUMMARY"))
					{
						if (rdoProcessStd.IsChecked.Equals(true))
						{
							dgSummaryShot.Columns["POSITN_NAME1"].Visibility = Visibility.Collapsed;
							dgSummaryShot.Columns["POSITN_NAME2"].Visibility = Visibility.Visible;
						}
						else
						{
							dgSummaryShot.Columns["POSITN_NAME1"].Visibility = Visibility.Visible;
							dgSummaryShot.Columns["POSITN_NAME2"].Visibility = Visibility.Collapsed;
						}
						dgSummaryShot.SetItemsSource(dtRslt, FrameOperation, false);
					}
					else
					{
						if (rdoProcessStd.IsChecked.Equals(true))
						{
							dgListShot.Columns["POSITN_NAME1"].Visibility = Visibility.Collapsed;
							dgListShot.Columns["POSITN_NAME2"].Visibility = Visibility.Visible;
						}
						else
						{
							dgListShot.Columns["POSITN_NAME1"].Visibility = Visibility.Visible;
							dgListShot.Columns["POSITN_NAME2"].Visibility = Visibility.Collapsed;
						}
						dgListShot.SetItemsSource(dtRslt, FrameOperation, true);
					}
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

		#region 공정 Combo
		private void SetProcessCombo()
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("EQSGID", typeof(string));
				RQSTDT.Columns.Add("PROCGRP", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["EQSGID"] = cboEqsgShot.SelectedItemsToString;
				dr["PROCGRP"] = cboProcGroup.GetBindValue();
				RQSTDT.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_MULTI_WITH_EQSG", "RQSTDT", "RSLTDT", RQSTDT);

				cboProcShot.ItemsSource = DataTableConverter.Convert(dtResult);

				if (dtResult.Rows.Count > 0)
				{
					cboProcShot.CheckAll();
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 라인 Combo
		private void SetLineComboBack()
		{
			try
			{

				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				RQSTDT.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

				cboEqsgShot.ItemsSource = DataTableConverter.Convert(dtResult);

				if (dtResult.Rows.Count > 0)
				{
					cboEqsgShot.CheckAll();
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}

		private void SetLineCombo()
		{
			string[] arrColumn = { "LANGID", "AREAID" };

			DataTable RQSTDT = new DataTable();
			RQSTDT.TableName = "RQSTDT";
			RQSTDT.Columns.Add("LANGID", typeof(string));
			RQSTDT.Columns.Add("AREAID", typeof(string));

			DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
			dr["AREAID"] = LoginInfo.CFG_AREA_ID;

			RQSTDT.Rows.Add(dr);

			DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_LINE", "RQSTDT", "RSLTDT", RQSTDT);

			cboEqsgShot.ItemsSource = DataTableConverter.Convert(dtResult);

			if (dtResult.Rows.Count > 0)
			{
				cboEqsgShot.CheckAll();
			}
		}
		#endregion

		#region 보관위치 Combo
		private void SetStorageLocCombo()
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("LANGID", typeof(string));
				RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["LANGID"] = LoginInfo.LANGID;
				dr["CMCDTYPE"] = "STORAGE_LOCATION";
				RQSTDT.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

				cboStorageLoc.ItemsSource = DataTableConverter.Convert(dtResult);

				if (dtResult.Rows.Count > 0)
				{
					cboStorageLoc.CheckAll();
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
				dr["STCK_CNT_YM"] = DateTime.Today.ToString("yyyyMM");
				if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
				{
					dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
				}
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["USERID"] = LoginInfo.USERID;

				if (dr["STCK_CNT_SEQNO"].Equals("")) return;

				RQSTDT.Rows.Add(dr);

				DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT_CANCEL", "INDATA", null, RQSTDT);

				_combo.SetCombo(cboStockSeqShot);

				// 2024.06.10 윤지해 추가
				ShowBtnCloseCancel(DateTime.Today.ToString("yyyyMM"));
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
		#endregion

		#region 차수마감 취소 버튼 VISIBLE 처리
		// 2024.06.10 윤지해 차수마감 취소 버튼 VISIBLE 처리
		private void ShowBtnCloseCancel(string stckCntYm)
		{
			ChkNerpApplyFlag();

			string[] sAttrbute = { "Y" };

			if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y"))
			{
				ChkNerpFlag(stckCntYm);
				ChkMaxSeq(stckCntYm);

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
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
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
		private void ChkNerpFlag(string stckCntYm)
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("CLOSE_YM", typeof(string));
				RQSTDT.Columns.Add("SHOPID", typeof(string));

				// 2024.08.02 Faiz Change to reduce minus 1 month for input in DA
				DateTime prevMonth = DateTime.ParseExact(stckCntYm, "yyyyMM", null);
				prevMonth = prevMonth.AddMonths(-1);

				DataRow dr = RQSTDT.NewRow();
				dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
				//dr["CLOSE_YM"] = stckCntYm;
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
		private void ChkMaxSeq(string stckCntYm)
		{
			try
			{
				DataTable RQSTDT = new DataTable();
				RQSTDT.TableName = "RQSTDT";
				RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
				RQSTDT.Columns.Add("AREAID", typeof(string));

				DataRow dr = RQSTDT.NewRow();
				dr["STCK_CNT_YM"] = stckCntYm;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;

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


	}
}
