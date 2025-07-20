/*************************************************************************************
 Created Date : 2020.10.15
      Creator : 서동현
   Decription : 긴급 출하 설정
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  서동현 책임 : Initial Created.    

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_049_QUICK_SHIP_SETTING : C1Window, IWorkArea
	{
		#region Declaration & Constructor 
		public bool IsUpdated = false;
		private string _AREA = string.Empty;        //동
		private int _maxCount = 2;

		private readonly Util _util = new Util();

		public MCS001_049_QUICK_SHIP_SETTING()
		{
			InitializeComponent();
		}

		public IFrameOperation FrameOperation { get; set; }

		private void C1Window_Loaded(object sender, RoutedEventArgs e)
		{
			ApplyPermissions();
            InitCombo();
			SetLineMaxCount();

			object[] parameters = C1WindowExtension.GetParameters(this);
			_AREA = parameters[0] as string; //동
			cboArea.SelectedValue = _AREA;

			SeachData();
            this.Loaded -= C1Window_Loaded;
        }

		/// <summary>
		/// 화면내 버튼 권한 처리
		/// </summary>
		private void ApplyPermissions()
		{
			List<Button> listAuth = new List<Button>();
			//listAuth.Add(btnInReplace);
			Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}

        /// <summary>
        /// 콤보박스 
        /// </summary>
        private void InitCombo()
        {
            try
            {
				// Area 콤보박스
				CommonCombo _combo = new CommonCombo();
				string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
				_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODEATTRS");
			}
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
			this.Close();
        }

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			SeachData();
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationSave()) return;

			// 저장하시겠습니까?
			Util.MessageConfirm("SFU1241", (result) =>
			{
				if (result == MessageBoxResult.OK)
				{
					this.SaveData();
				}
			});
		}

		private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{

		}
		#endregion

		#region Mehod
		/// <summary>
		/// 조회
		/// </summary>
		private void SeachData()
		{
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable RQSTDT = new DataTable( "RQSTDT" );
			RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("BLDG_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
			dr["BLDG_CODE"] = (cboArea.SelectedValue == null || string.IsNullOrEmpty(cboArea.SelectedValue.ToString())) ? null : cboArea.SelectedValue.ToString();

			RQSTDT.Rows.Add(dr);

			new ClientProxy().ExecuteService("DA_MCS_SEL_FORMATION_EOL_QUICK_SHIP_INFO", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) =>
			{
				try
				{
					if(exception != null)
					{
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(exception);
						return;
					}

					Util.GridSetData(dgList, result, FrameOperation, true);
				}
				catch (Exception ex)
				{
					Util.MessageException(ex);
				}
				finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

			} );
		}
		private void SetLineMaxCount()
		{
			const string bizRuleName = "DA_MCS_SEL_COMMCODE";
			DataTable inTable = new DataTable("RQSTDT");
			inTable.Columns.Add("CMCDTYPE", typeof(string));
			inTable.Columns.Add("CMCODE", typeof(string));

			DataRow dr = inTable.NewRow();
			dr["CMCDTYPE"] = "CWA_EMRG_SHIP_LINE_MAX_CNT";
			dr["CMCODE"] = "MAX_VAL";
			inTable.Rows.Add(dr);

			DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

			if (CommonVerify.HasTableRow(dtResult))
			{
				_maxCount = Convert.ToInt32(dtResult.Rows[0]["ATTRIBUTE1"]);
			}
			else
			{
				_maxCount = 2;		// 기본값 2
			}
		}

		private bool ValidationSave()
		{
			C1DataGrid dg = dgList;

			if (!CommonVerify.HasDataGridRow(dg))
			{
				Util.MessageValidation("SFU1636");
				return false;
			}

			//if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
			//{
			//	Util.MessageValidation("SFU1636");
			//	return false;
			//}

			return true;
		}

		private void SaveData()
		{
			try
			{
				loadingIndicator.Visibility = Visibility.Visible;

				const string bizRuleName = "BR_MCS_REG_FORMATION_EOL_QUICKSHIP";

				DataSet ds = new DataSet();
				DataTable inData = ds.Tables.Add("INDATA");
				inData.Columns.Add("EQPTID", typeof(string));
				inData.Columns.Add("QUICKSHIP_YN", typeof(string));
				inData.Columns.Add("UPDUSER", typeof(string));

				foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
				{
					DataRow newRow = inData.NewRow();
					newRow["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();

					if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
					{
						newRow["QUICKSHIP_YN"] = "Y";
					}
					else
					{
						newRow["QUICKSHIP_YN"] = "N";
					}

					newRow["UPDUSER"] = LoginInfo.USERID;

					inData.Rows.Add(newRow);
				}

				new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (result, exception) => 
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					try
					{
						if (exception != null)
						{
							Util.MessageException(exception);
							return;
						}

						IsUpdated = true;
						DialogResult = MessageBoxResult.OK;
						Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
						//btnSearch_Click(btnSearch, null);
					}
					catch (Exception ex)
					{
						loadingIndicator.Visibility = Visibility.Collapsed;
						Util.MessageException(ex);
					}
				}, ds);
			}
			catch (Exception ex)
			{
				loadingIndicator.Visibility = Visibility.Collapsed;
				Util.MessageException(ex);
			}
		}
		#endregion

		private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
		{
			try
			{
				if (e?.Row?.DataItem == null || e.Column == null)
					return;

				C1DataGrid dg = dgList;

				if (e.Column.Name == "CHK")
				{
					int idx = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
					string cuurentArea = DataTableConverter.GetValue(e.Row.DataItem, "AREA").GetString();
					string currentFloor = DataTableConverter.GetValue(e.Row.DataItem, "FLOOR").GetString();
					bool currentChk = DataTableConverter.GetValue(e.Row.DataItem, "CHK").ToString() == "1" ? true : false;
					
					
					var query = (from t in ((DataView)dg.ItemsSource).Table.AsEnumerable()
								 where t.Field<int>("CHK") == 1
									&& t.Field<string>("AREA") == cuurentArea
									&& t.Field<string>("FLOOR") == currentFloor
								 select t).ToList();
					int iCount = query.Count;

					if (currentChk) iCount--;

					if (_maxCount <= iCount)
					{
						e.Cancel = true;
						Util.MessageValidation("SFU8262", _maxCount.ToString());
						return;
					}
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
	}
}