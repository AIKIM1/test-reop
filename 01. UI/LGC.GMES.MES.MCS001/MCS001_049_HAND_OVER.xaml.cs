/*************************************************************************************
 Created Date : 2021.01.11
      Creator : 서동현
   Decription : 특이사항 관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.11  서동현 책임 : Initial Created.    

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
using System.Text;

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_049_HAND_OVER : C1Window, IWorkArea
	{
		#region Declaration & Constructor 
		public bool IsUpdated = false;
		private string _AREA = string.Empty;        //동
		private int m_LimitCount = 70;

		private readonly Util _util = new Util();

		public MCS001_049_HAND_OVER()
		{
			InitializeComponent();
		}

		public IFrameOperation FrameOperation { get; set; }

		private void C1Window_Loaded(object sender, RoutedEventArgs e)
		{
			ApplyPermissions();
			InitializeControls();
			InitCombo();
			SetRemarkLimitCount();

			//object[] parameters = C1WindowExtension.GetParameters(this);
			//_AREA = parameters[0] as string; //동
			//cboArea.SelectedValue = _AREA;

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
			Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
		}

		/// <summary>
		/// 컨트롤 초기화
		/// </summary>
		private void InitializeControls()
		{
			dtpDateFrom.SelectedDateTime = DateTime.Now.Date.AddDays(-1);
			dtpDateTo.SelectedDateTime = DateTime.Now.Date;
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
				_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");

				this.SetLevelCombo(cboLevel);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void SetRemarkLimitCount()
		{
			const string bizRuleName = "DA_MCS_SEL_COMMCODE";
			DataTable inTable = new DataTable("RQSTDT");
			inTable.Columns.Add("CMCDTYPE", typeof(string));
			inTable.Columns.Add("CMCODE", typeof(string));

			DataRow dr = inTable.NewRow();
			dr["CMCDTYPE"] = "REMARK_LIMIT_COUNT";
			dr["CMCODE"] = "LIMIT_VAL";
			inTable.Rows.Add(dr);

			DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

			if (CommonVerify.HasTableRow(dtResult))
			{
				m_LimitCount = Convert.ToInt32(dtResult.Rows[0]["ATTRIBUTE1"]);
			}
			else
			{
				m_LimitCount = 70;
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

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			// 초기화 하시겠습니까?
			Util.MessageConfirm("SFU3440", (result) =>
			{
				if (result == MessageBoxResult.OK)
				{
					this.txtRemark.Text = string.Empty;
					this.SaveData();
				}
			});
		}
		#endregion

		#region Mehod
		private void SetLevelCombo(C1ComboBox cbo)
		{
			const string bizRuleName = "DA_PRD_SEL_FORMATION_LEVEL_CBO";

			DataTable dtRQSTDT = new DataTable();
			dtRQSTDT.TableName = "RQSTDT";
			dtRQSTDT.Columns.Add("LANGID", typeof(string));
			dtRQSTDT.Columns.Add("EQGRID", typeof(string));

			DataRow drNewrow = dtRQSTDT.NewRow();
			drNewrow["LANGID"] = LoginInfo.LANGID;
			drNewrow["EQGRID"] = "FCW";

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
				cbo.ItemsSource = DataTableConverter.Convert(dtTemp);

				//ComboStatus cs = ComboStatus
				//cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "EQPTID", "EQPTNAME").Copy().AsDataView();
				cbo.SelectedIndex = 0;
			}
			);
		}

		/// <summary>
		/// 조회
		/// </summary>
		private void SeachData()
		{
			loadingIndicator.Visibility = Visibility.Visible;
			DataTable RQSTDT = new DataTable("RQSTDT");
			RQSTDT.Columns.Add("LANGID", typeof(string));
			RQSTDT.Columns.Add("BLDG_CODE", typeof(string));
			RQSTDT.Columns.Add("LEVEL", typeof(string));
			RQSTDT.Columns.Add("FROM_DT", typeof(DateTime));
			RQSTDT.Columns.Add("TO_DT", typeof(DateTime));

			DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
			dr["BLDG_CODE"] = (cboArea.SelectedValue == null || string.IsNullOrEmpty(cboArea.SelectedValue.ToString())) ? null : cboArea.SelectedValue.ToString();
			dr["LEVEL"] = (cboLevel.SelectedValue == null || string.IsNullOrEmpty(cboLevel.SelectedValue.ToString())) ? null : cboLevel.SelectedValue.ToString();
			dr["FROM_DT"] = dtpDateFrom.SelectedDateTime.Date;
			dr["TO_DT"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);

			RQSTDT.Rows.Add(dr);

			new ClientProxy().ExecuteService("DA_MCS_SEL_FORMATION_EQP_NOTE_LIST", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) =>
			{
				try
				{
					if (exception != null)
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
			});
		}

		private bool ValidationSave()
		{
			C1DataGrid dg = dgList;

			//if (!CommonVerify.HasDataGridRow(dg))
			//{
			//	Util.MessageValidation("SFU1636");
			//	return false;
			//}

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

				const string bizRuleName = "BR_MCS_REG_FORMATION_EQP_REMARK";

				DataSet ds = new DataSet();
				DataTable inData = ds.Tables.Add("INDATA");
				inData.Columns.Add("BLDG_CODE", typeof(string));
				inData.Columns.Add("LEVEL", typeof(string));
				inData.Columns.Add("INSUSERID", typeof(string));
				inData.Columns.Add("REMARK", typeof(string));

				string strRemark = this.txtRemark.Text.Trim();

				StringBuilder sb = new StringBuilder();

				string[] sptReamrk = strRemark.Split(new string[] { "\r\n" }, StringSplitOptions.None);

				for (int i = 0; i < sptReamrk.Length; i++)
				{
					string aRemark = sptReamrk[i];

					if (aRemark.Length > this.m_LimitCount)
					{
						sb.Append(SplitText(aRemark, this.m_LimitCount));
					}
					else
					{
						sb.Append(aRemark);
					}

					if(i != sptReamrk.Length - 1)
					{
						sb.Append("\r\n");
					}
				}

				strRemark = sb.ToString();

				if (strRemark.Length > 4000)
					strRemark = strRemark.Substring(0, 4000);

				DataRow newRow = inData.NewRow();
				newRow["BLDG_CODE"] = (cboArea.SelectedValue == null || string.IsNullOrEmpty(cboArea.SelectedValue.ToString())) ? null : cboArea.SelectedValue.ToString();
				newRow["LEVEL"] = (cboLevel.SelectedValue == null || string.IsNullOrEmpty(cboLevel.SelectedValue.ToString())) ? null : cboLevel.SelectedValue.ToString();
				newRow["INSUSERID"] = LoginInfo.USERID;
				newRow["REMARK"] = strRemark;

				inData.Rows.Add(newRow);

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
						//DialogResult = MessageBoxResult.OK;
						//Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
						//btnSearch_Click(btnSearch, null);

						Util.MessageInfo("SFU1275", (action) =>
						{
							SeachData(); 
						});
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

		private string SplitText(string s, int length)
		{
			string strReturnData = string.Empty;

			var split = s.Select((c, index) => new { c, index })
				.GroupBy(x => x.index / length)
				.Select(group => group.Select(elem => elem.c))
				.Select(chars => new string(chars.ToArray()));

			foreach (var str in split)
			{
				strReturnData += str + "\r\n";
			}

			if(strReturnData.Length > 0)
			{
				strReturnData = strReturnData.Substring(0, strReturnData.Length - 2);
			}

			return strReturnData;
		}
		#endregion
	}
}