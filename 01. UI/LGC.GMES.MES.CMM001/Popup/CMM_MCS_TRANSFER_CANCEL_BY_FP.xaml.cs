/*************************************************************************************
 Created Date : 2020.09.10
      Creator : 서동현 책임
   Decription : 활성화 창고 수동반송 예약 취소 (Formation <-> Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.10  서동현 : Initial Created.
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_MCS_TRANSFER_CANCEL_BY_FP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_MCS_TRANSFER_CANCEL_BY_FP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public bool IsUpdated;
        private DataTable _dtTransferCancel;
		private DataTable _dtPalletList;
		private string _BldgCode;
        private string _PortId;
		private string _DstType;

		private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        private readonly Util _util = new Util();

		private bool _isFAAuthority;

		public CMM_MCS_TRANSFER_CANCEL_BY_FP()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        private void Initialize()
        {
            ApplyPermissions();
            InitializeControls();
            InitializeCombo();

			if(_isFAAuthority)
			{
				txtDstType.Visibility = Visibility.Visible;
				cboDstType.Visibility = Visibility.Visible;
			}
        }

        private void InitializeControls()
        {
            DateTime systemDateTime = GetSystemTime();

            if (dtpDateFrom != null)
                dtpDateFrom.SelectedDateTime = systemDateTime;

            if (dtpDateTo != null)
                dtpDateTo.SelectedDateTime = systemDateTime.AddDays(+1);
        }

        private void InitializeCombo()
        {
            SetCommonCombo(cboEquipmentType, "EQPT_TP");

            cboEquipmentType.SelectedValue = "STKC";

            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);

			cboDstType.SelectedValueChanged -= cboDstType_SelectedValueChanged;
			CommonCombo _combo = new CommonCombo();
			string[] sFilter3 = { "STK_DST_TYPE" };
			_combo.SetCombo(cboDstType, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
			

			if (_DstType != null)
				cboDstType.SelectedValue = _DstType;
			
			SetPortCombo(_BldgCode);

			if (_PortId != null)
                cboPort.SelectedValue = _PortId;

			cboDstType.SelectedValueChanged += cboDstType_SelectedValueChanged;
		}
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            GetBizActorServerInfo();
			_isFAAuthority = IsFAAuthorityByUserId(LoginInfo.USERID);
		}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _dtTransferCancel = (tmps[0] == null) ? null : tmps[0] as DataTable;
                _BldgCode = (tmps[1] == null) ? null : tmps[1].ToString();
                _PortId = (tmps[2] == null) ? null : tmps[2].ToString();
				_DstType = (tmps[3] == null) ? null : tmps[3].ToString();

				Loaded -= C1Window_Loaded;
                Initialize();

                if (_dtTransferCancel != null && CommonVerify.HasTableRow(_dtTransferCancel))
                {
                    SelectFirstManualTransferCancelList();
                }
                else if (_PortId != null)
                {
                    this.btnSearch_Click(this.btnSearch, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDataGridCheckHeaderInitialize(dgManualTransferCancel);
                SelectManualTransferCancelList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string equipmentType = string.IsNullOrEmpty(cboEquipmentType?.SelectedValue.GetString()) ? null : cboEquipmentType?.SelectedValue.GetString();
            SetEquipmentCombo(cboEquipment, equipmentType);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", "1");
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgManualTransferCancel;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", "0");
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void btnManualTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualTransferCancel()) return;

            // 수동출고 예약 취소하시겠습니까?
            Util.MessageConfirm("SFU4544", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.SaveManualTransferCancel();
                }
            });
        }
        #endregion

        #region Mehod
        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "FP_MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }
        }

		private bool IsFAAuthorityByUserId(string userId)
		{
			try
			{
				DataTable inTable = new DataTable("RQSTDT");
				inTable.Columns.Add("USERID", typeof(string));
				inTable.Columns.Add("AUTHID", typeof(string));

				DataRow dr = inTable.NewRow();
				dr["USERID"] = userId;
				dr["AUTHID"] = "MESADMIN,FA_MNGR";
				inTable.Rows.Add(dr);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", inTable);

				if (CommonVerify.HasTableRow(dtResult))
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
				return false;
			}
		}

		private void SetPortCombo(string bldgCode)
        {
            try
            {
				if (cboDstType.SelectedValue.ToString().ToUpper().Equals("PORT"))
				{
					const string bizRuleName = "DA_SEL_MCS_LOC_PORT_INFO_EQPT";

					DataTable inTable = new DataTable("RQSTDT");
					inTable.Columns.Add("LANGID", typeof(string));
					inTable.Columns.Add("BLDGCODE", typeof(string));

					DataRow dr = inTable.NewRow();
					dr["LANGID"] = LoginInfo.LANGID;
					dr["BLDGCODE"] = bldgCode;

					inTable.Rows.Add(dr);

					new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
					{
						try
						{
							if (bizException != null)
							{
								Util.MessageException(bizException);
								return;
							}

							//Util.GridSetData(dgPortInfo, result, null, true);

							DataRow newRow = result.NewRow();
							newRow[cboPort.SelectedValuePath.GetString()] = null;
							newRow[cboPort.DisplayMemberPath.GetString()] = "-ALL-";
							result.Rows.InsertAt(newRow, 0);

							cboPort.ItemsSource = result.Copy().AsDataView();
							if (cboPort.SelectedIndex < 0) cboPort.SelectedIndex = 0;
						}
						catch (Exception ex)
						{
							Util.MessageException(ex);
						}
					});
				}
				else
				{
					DataTable inTable = new DataTable("RQSTDT");
					inTable.Columns.Add("LANGID", typeof(string));
					//inTable.Columns.Add("SRC_LOCID", typeof(string));
					inTable.Columns.Add("SRC_BLDGCODE", typeof(string));
					//inTable.Columns.Add("CSTSTAT", typeof(string));

					DataRow dr = inTable.NewRow();
					dr["LANGID"] = LoginInfo.LANGID;
					//dr["SRC_LOCID"] = "";
					dr["SRC_BLDGCODE"] = _BldgCode;
					//dr["SRC_EQPTID"] = equipmentCode;
					//dr["CSTSTAT"] = cstStat == "" ? null : cstStat;

					inTable.Rows.Add(dr);

					DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("DA_SEL_EQP_DST_INFO_BY_MANUAL_UI", "RQSTDT", "RSLTDT", inTable);

					dtResult.Columns["DST_LOC_GROUPID"].ColumnName = "PORT_ID";
					dtResult.Columns["DST_LOC_GROUP_NAME"].ColumnName = "PORT_NAME";
					dtResult.AcceptChanges();

					DataRow newRow = dtResult.NewRow();
					//newRow["DST_LOC_GROUPID"] = null;
					//newRow["DST_LOC_GROUP_NAME"] = "-ALL-";
					newRow[cboPort.SelectedValuePath.GetString()] = null;
					newRow[cboPort.DisplayMemberPath.GetString()] = "-ALL-";
					dtResult.Rows.InsertAt(newRow, 0);

					cboPort.ItemsSource = dtResult.Copy().AsDataView();
					if (cboPort.SelectedIndex < 0) cboPort.SelectedIndex = 0;
				}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectFirstManualTransferCancelList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                //const string bizRuleName = "DA_SEL_MCS_REQ_TRF_GUI";
                const string bizRuleName = "DA_SEL_MCS_REQ_TRF_INFO_BY_MES_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("REQ_TRFID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("BLDG_CODE", typeof(string));
				//inDataTable.Columns.Add("DST_LOCID", typeof(string));
				inDataTable.Columns.Add("DSTTYPE", typeof(string));

				DateTime systemDateTime = GetSystemTime();

                foreach (DataRow row in _dtTransferCancel.Rows)
                {
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["REQ_TRFID"] = row["ORDID"].ToString();
                    newRow["FROM_DATE"] = systemDateTime.AddMonths(-1).ToString("yyyyMMdd");
                    newRow["TO_DATE"] = systemDateTime.ToString("yyyyMMdd");
                    newRow["BLDG_CODE"] = _BldgCode;
					//newRow["DST_LOCID"] = string.IsNullOrEmpty(cboPort.SelectedValue.GetString()) ? null : cboPort.SelectedValue.GetString();
					newRow["DSTTYPE"] = cboDstType.SelectedValue;
					inDataTable.Rows.Add(newRow);
                }

                new ClientProxy(
                    _bizRuleIp
                    , _bizRuleProtocol
                    , _bizRulePort
                    , _bizRuleServiceMode
                    , _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

						//DataTable dtProid = result.DefaultView.ToTable(true, "PRODID");

						//string strProdidList = string.Empty;

						//foreach (DataRow dr in dtProid.Rows)
						//{
						//	strProdidList += dr["PRODID"].ToString() + ",";
						//}

						//if (strProdidList.Length > 0)
						//	strProdidList = strProdidList.Substring(0, strProdidList.Length - 1);

						string strCstIdList = string.Empty;

						foreach (DataRow dr in result.Rows)
						{
							strCstIdList += dr["CARRIERID"].ToString() + ",";
						}

						if (strCstIdList.Length > 0)
							strCstIdList = strCstIdList.Substring(0, strCstIdList.Length - 1);

						this.SelectProjectList(ConvertData(strCstIdList));

						result.Columns.Add("REQ_TRF_STAT_NAME", typeof(string));
						result.Columns.Add("PRJT_NAME", typeof(string));
						result.Columns.Add("BOXID", typeof(string));

						foreach (DataRow dr in result.Rows)
						{
							string strProjectName = string.Empty;
							string strPalletId = string.Empty;

							DataRow[] drs = _dtPalletList.Select("CSTID = '" + dr["CARRIERID"].ToString() + "'");

							if (drs.Length > 0)
							{
								strProjectName = drs[0]["PRJT_NAME"].ToString();
								strPalletId = drs[0]["BOXID"].ToString();
							}

							dr["REQ_TRF_STAT_NAME"] = ObjectDic.Instance.GetObjectName(dr["REQ_TRF_STAT"].ToString());
							dr["PRJT_NAME"] = strProjectName;
							dr["BOXID"] = strPalletId;
						}

                        Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);

                        if (CommonVerify.HasTableRow(result))
                        {
                            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgManualTransferCancel.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                            StackPanel allPanel = allColumn?.Header as StackPanel;
                            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                            allCheck.IsChecked = true;
                        }
                    });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

		private void SelectProjectList(string cstIdList)
		{
			const string bizRuleName = "DA_MCS_SEL_PALLET_BY_CSTID";

			DataTable inDataTable = new DataTable("RQSTDT");

			inDataTable.Columns.Add("CSTID_LIST", typeof(string));

			DataRow dr = inDataTable.NewRow();
			dr["CSTID_LIST"] = cstIdList;

			inDataTable.Rows.Add(dr);

			_dtPalletList = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
		}

		private void SelectManualTransferCancelList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgManualTransferCancel);

                //const string bizRuleName = "DA_SEL_MCS_REQ_TRF_GUI";
                const string bizRuleName = "DA_SEL_MCS_REQ_TRF_INFO_BY_MES_GUI";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("REQ_TRFID", typeof(string));
                inDataTable.Columns.Add("CARRIERID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("EQPT_TP", typeof(string));
                inDataTable.Columns.Add("BLDG_CODE", typeof(string));
                inDataTable.Columns.Add("DST_LOCID", typeof(string));
				inDataTable.Columns.Add("DSTTYPE", typeof(string));

				DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                inData["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inData["REQ_TRFID"] = !string.IsNullOrEmpty(txtRequestTransferId.Text.Trim()) ? txtRequestTransferId.Text : null;
                inData["CARRIERID"] = !string.IsNullOrEmpty(txtCarrierId.Text.Trim()) ? txtCarrierId.Text : null;
                inData["EQPTID"] = cboEquipment.SelectedValue;
                inData["EQPT_TP"] = cboEquipmentType.SelectedValue;
                inData["BLDG_CODE"] = _BldgCode;
                inData["DST_LOCID"] = string.IsNullOrEmpty(cboPort.SelectedValue.GetString()) ? null : cboPort.SelectedValue.GetString();
				inData["DSTTYPE"] = cboDstType.SelectedValue;

				inDataTable.Rows.Add(inData);

                new ClientProxy(
                    _bizRuleIp
                    , _bizRuleProtocol
                    , _bizRulePort
                    , _bizRuleServiceMode
                    , _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

						//DataTable dtProid = result.DefaultView.ToTable(true, "PRODID");

						//string strProdidList = string.Empty;

						//foreach(DataRow dr in dtProid.Rows)
						//{
						//	strProdidList += dr["PRODID"].ToString() + ",";
						//}

						//if (strProdidList.Length > 0)
						//	strProdidList = strProdidList.Substring(0, strProdidList.Length - 1);

						string strCstIdList = string.Empty;

						foreach (DataRow dr in result.Rows)
						{
							strCstIdList += dr["CARRIERID"].ToString() + ",";
						}

						if (strCstIdList.Length > 0)
							strCstIdList = strCstIdList.Substring(0, strCstIdList.Length - 1);

						this.SelectProjectList(ConvertData(strCstIdList));

						result.Columns.Add("REQ_TRF_STAT_NAME", typeof(string));
						result.Columns.Add("PRJT_NAME", typeof(string));
						result.Columns.Add("BOXID", typeof(string));

						foreach (DataRow dr in result.Rows)
                        {
							string strProjectName = string.Empty;
							string strPalletId = string.Empty;

							DataRow[] drs = _dtPalletList.Select("CSTID = '" + dr["CARRIERID"].ToString() + "'");

							if (drs.Length > 0)
							{
								strProjectName = drs[0]["PRJT_NAME"].ToString();
								strPalletId = drs[0]["BOXID"].ToString();
							}
							
                            dr["REQ_TRF_STAT_NAME"] = ObjectDic.Instance.GetObjectName(dr["REQ_TRF_STAT"].ToString());
							dr["PRJT_NAME"] = strProjectName;
							dr["BOXID"] = strPalletId;
						}

                        Util.GridSetData(dgManualTransferCancel, result, FrameOperation, true);
                    });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SaveManualTransferCancel()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                //const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER_CHG_STAT";
                const string bizRuleName = "BR_GUI_REG_TRF_JOB_CANCEL_BY_MES_GUI";

                DataSet ds = new DataSet();
                DataTable requestTransferInfoTable = ds.Tables.Add("IN_CANCEL_INFO");
                requestTransferInfoTable.Columns.Add("ORDID", typeof(string));
                requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
                requestTransferInfoTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgManualTransferCancel.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = requestTransferInfoTable.NewRow();
                        newRow["ORDID"] = DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        requestTransferInfoTable.Rows.Add(newRow);
                    }
                }

                //DataTable changeStateTable = ds.Tables.Add("IN_CHG_STAT");
                //changeStateTable.Columns.Add("CHG_STAT", typeof(string));
                //DataRow dr = changeStateTable.NewRow();
                //dr["CHG_STAT"] = "CANCEL";
                //changeStateTable.Rows.Add(dr);

                new ClientProxy(
                    _bizRuleIp
                    , _bizRuleProtocol
                    , _bizRulePort
                    , _bizRuleServiceMode
                    , _bizRuleServiceIndex).ExecuteService_Multi(bizRuleName, "IN_CANCEL_INFO", "OUT_DATA", (bizResult, bizException) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            IsUpdated = true;
                            Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                            btnSearch_Click(btnSearch, null);
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

        private DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_COR_SEL_SYSTIME_G";
            DataTable inDataTable = new DataTable("INDATA");

            DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private bool ValidationManualTransferCancel()
        {
            C1DataGrid dg = dgManualTransferCancel;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private void SetCommonCombo(C1ComboBox cbo, string codeType)
        {
            try
            {
                const string bizRuleName = "DA_SEL_MMD_MCS_COMMONCODE_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CMCDIUSE"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentCombo(C1ComboBox cbo, string equipmentType = null)
        {
            try
            {
                const string bizRuleName = "DA_SEL_MMD_EQPT_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPT_TP", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_TP"] = equipmentType;
                dr["USE_FLAG"] = "Y";

                inTable.Rows.Add(dr);
                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                DataRow newRow = dtResult.NewRow();
                newRow[cbo.SelectedValuePath.GetString()] = null;
                newRow[cbo.DisplayMemberPath.GetString()] = "-ALL-";
                dtResult.Rows.InsertAt(newRow, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnManualTransferCancel };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
		#endregion

		private void cboDstType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
		{
			try
			{
				SetPortCombo(_BldgCode);
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
	}
}