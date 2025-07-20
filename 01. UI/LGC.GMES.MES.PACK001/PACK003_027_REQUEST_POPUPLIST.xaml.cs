/*************************************************************************************
 Created Date : 2021.09.20
      Creator : 김길용
  Description : 모델/라인/일자별 수동요청 팝업
--------------------------------------------------------------------------------------
 [Change History]
    Date       Author       CSR          Description...
  2021.09.20   김길용        SI          Initial Created.
  2021.11.04   김길용        SI          ESWA PACK3 적용을 위한 하드코딩 제거
  2021.11.11   김길용        SI          Pack3동 공통화 수정,하드코딩 제거 및 샘플링,AREAID 컬럼 추가
  2023.07.14   김길용        SM          E20230516-000486 [WA GMES Pack] 팩물류 연관 수동배출명령 시 MES 이력 추가를 위한 기능수정(개선방안)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_027_REQUEST_POPUPLIST : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private PACK003_027_REQUEST_POPUPLIST_DataHelper dataHelper = new PACK003_027_REQUEST_POPUPLIST_DataHelper();
        private string productID = string.Empty;
        private string equipmentSegmentID = string.Empty;
        private string fromEOLDate = string.Empty;
        private string toEOLDate = string.Empty;
        private string firstGroupCode = string.Empty;
        private string secondGroupCode = string.Empty;
        private string logisPackTypeCode = string.Empty;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_027_REQUEST_POPUPLIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void SelectLotList()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgLOTList.ItemsSource);
                if (dt.Rows.Count < (double)this.txtCountQty.Value)
                {
                    Util.MessageValidation("SFU4418");  // 입력수량이 재공수량보다 클 수 없습니다.
                    return;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][0] = false;
                }

                for (int i = 0; i < Convert.ToInt32(this.txtCountQty.Value); i++)
                {
                    dt.Rows[i][0] = true;
                }

                this.dgLOTList.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Initialize()
        {
            object[] arrParameters = C1WindowExtension.GetParameters(this);
            if (arrParameters == null || arrParameters.Length <= 0)
            {
                return;
            }

            try
            {
                this.productID = Util.NVC(arrParameters[0]);
                this.equipmentSegmentID = Util.NVC(arrParameters[1]);
                this.fromEOLDate = Util.NVC(arrParameters[2]);
                this.toEOLDate = Util.NVC(arrParameters[3]);
                this.firstGroupCode = Util.NVC(arrParameters[4]);
                this.secondGroupCode = Util.NVC(arrParameters[5]);
                this.logisPackTypeCode = Util.NVC(arrParameters[6]);

                InitializeCombo();
                if (firstGroupCode == "TRF_AVA")
                {
                    this.pnlPackEquipment.Visibility = Visibility.Visible;
                    this.cboChangeRoute.Visibility = Visibility.Visible;
                    this.ucPersonInfo.Visibility = Visibility.Visible;
                    this.btnRequest.Visibility = Visibility.Visible;
                }
                else
                {
                    this.pnlPackEquipment.Visibility = Visibility.Collapsed;
                    this.cboChangeRoute.Visibility = Visibility.Collapsed;
                    this.ucPersonInfo.Visibility = Visibility.Collapsed;
                    this.btnRequest.Visibility = Visibility.Collapsed;
                }
                this.dgLOTList.Columns["SMPL_HOLD"].Visibility = LoginInfo.CFG_AREA_ID.Equals("PA") ? Visibility.Visible : Visibility.Collapsed;

                this.SearchProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //Combo 상태정보 : ALL, N/A, SELECT
        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
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
                    dr[sValue] = "-SELECT-";
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

        private void InitializeCombo()
        {
            try
            {
                // 하단포장기 콤보
                SetBinding_BoxEqptList();
                object[] arrFramOperationParameters = FrameOperation.Parameters;
                if (arrFramOperationParameters == null || arrFramOperationParameters.Length <= 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBinding_BoxEqptList()
        {
            //라인
            string[] filter = new string[] { LoginInfo.CFG_AREA_ID };
            SetInput_BoxEqptList(cboChangeRoute, CommonCombo.ComboStatus.SELECT, filter);
        }

        private void SetInput_BoxEqptList(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("VIEW_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = DBNull.Value;
                dr["VIEW_FLAG"] = DBNull.Value;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MCS_GET_LOGIS_MANUAL_PORT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "PORT_NAME";
                cbo.SelectedValuePath = "PORT_ID";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "PORT_NAME", "PORT_ID").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidationCheck()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgLOTList.ItemsSource);

                // 선택된거 없으면 Interlock
                var checkedData = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));
                if (checkedData.Count() <= 0)
                {
                    Util.MessageInfo("SFU1651");    // 선택된 항목이 없습니다.
                    this.dgLOTList.Focus();
                    return false;
                }
                if (string.IsNullOrEmpty(this.cboChangeRoute.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU1223");
                    this.cboChangeRoute.Focus();
                    return false;
                }
                if (ucPersonInfo.UserID == null || string.IsNullOrEmpty(ucPersonInfo.UserID.ToString()))
                {
                    Util.MessageInfo("SFU1843");
                    this.ucPersonInfo.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // BizRule 호출 - MCS BizActor 접속정보 가져오기
        private DataTable GetMCSBizActorServerInfo()
        {
            string bizRuleName = "DA_MCS_SEL_MCS_CONFIG_INFO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            dtRQSTDT.Columns.Add("KEYGROUPID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["KEYGROUPID"] = "FP_MCS_AP_LOGIS_CONFIG";
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            if (!CommonVerify.HasTableRow(dtRSLTDT))
            {
                return null;
            }

            var query = dtRSLTDT.AsEnumerable().GroupBy(x => x.Field<string>("KEYGROUPID")).Select(grp => new
            {
                BIZACTORIP = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORIP")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPORT = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPORT")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPROTOCOL = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPROTOCOL")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEINDEX = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEINDEX")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEMODE = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEMODE")) ? x.Field<string>("KEYVALUE") : string.Empty)
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // BizRule 호출 - DB Time 가져오기
        private DateTime GetSystemTime()
        {
            string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";
            DateTime dteReturn = DateTime.Now;

            try
            {
                string inDataTableNameList = string.Empty;
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    foreach (DataTable dt in dsOUTDATA.Tables.OfType<DataTable>().Where(x => x.TableName.Equals(outDataSetName)))
                    {
                        if (CommonVerify.HasTableRow(dt))
                        {
                            foreach (DataRowView drv in dt.AsDataView())
                            {
                                dteReturn = Convert.ToDateTime(drv["SYSTIME"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dteReturn;
            }

            return dteReturn;
        }

        // BizRule 호출 - 수동포트출고예약
        public bool LineChangeProcess(DataTable dt, string destEquipmentID, string destPortID)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_TRF_JOB_BY_MES_MANUAL";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUT_DATA";
            DateTime dateTime = this.GetSystemTime();
            DataTable dtMCSBizActorServerInfo = this.GetMCSBizActorServerInfo();

            DataTable dtIN_DATA = new DataTable("IN_DATA");
            dtIN_DATA.Columns.Add("SRC_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("SRC_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("DST_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("DST_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIERID", typeof(string));
            dtIN_DATA.Columns.Add("USER", typeof(string));
            dtIN_DATA.Columns.Add("DTTM", typeof(DateTime));
            dtIN_DATA.Columns.Add("PRODID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIER_STRUCT", typeof(string));
            dtIN_DATA.Columns.Add("MDL_TP", typeof(string));
            dtIN_DATA.Columns.Add("STK_ISS_TYPE", typeof(string));

            foreach (DataRowView drv in dt.AsDataView())
            {
                DataRow drIN_DATA = dtIN_DATA.NewRow();
                drIN_DATA["CARRIERID"] = drv["LOTID"].ToString();
                drIN_DATA["SRC_EQPTID"] = drv["STOCKER_ID"].ToString();
                drIN_DATA["SRC_LOCID"] = drv["RACK_ID"].ToString();
                drIN_DATA["DST_EQPTID"] = destEquipmentID;
                drIN_DATA["DST_LOCID"] = destPortID;
                drIN_DATA["USER"] = LoginInfo.USERID;
                drIN_DATA["DTTM"] = dateTime;
                drIN_DATA["PRODID"] = drv["PRODID"].ToString();
                drIN_DATA["CARRIER_STRUCT"] = null;
                drIN_DATA["MDL_TP"] = null;
                drIN_DATA["STK_ISS_TYPE"] = null;
                dtIN_DATA.Rows.Add(drIN_DATA);
            }
            dsINDATA.Tables.Add(dtIN_DATA);

            string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
            try
            {
                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());

                    dsOUTDATA = clientProxy.ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                    if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                    {
                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다
                        returnValue = true;
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 조회
        private void SearchProcess()
        {
            PackCommon.SearchRowCount(ref this.txtGridRowCount, 0);
            Util.gridClear(this.dgLOTList);

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                DataTable dt = this.dataHelper.GetStockerLOTByEOLDateDetail(this.productID, this.equipmentSegmentID, this.fromEOLDate, this.toEOLDate, this.txtLOTID.Text, this.firstGroupCode, this.secondGroupCode);

                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgLOTList, dt, FrameOperation);
                    PackCommon.SearchRowCount(ref this.txtGridRowCount, dt.Rows.Count);
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

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            this.SearchProcess();
        }

        // Validation Check
        private bool ValidationCheckRequest()
        {
            bool returnValue = true;

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                //this.ucPersonInfo.Focus();
                return false;
            }
            if (this.dgLOTList == null || this.dgLOTList.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }

            if (this.dgLOTList.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgLOTList.ItemsSource);
            var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));
            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                //this.dgStockerLoadedList.Focus();
                return false;
            }

            // Manual Port 선택 여부
            if (this.cboChangeRoute.SelectedValue == null || string.IsNullOrEmpty(this.cboChangeRoute.SelectedValue.ToString()) || this.cboChangeRoute.SelectedValue.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("포장기")); // %1(을)를 선택하세요.
                //this.cboChangeRoute.Focus();
                return false;
            }

            if (!this.SetBoxEqptOperModeChk())
            {
                Util.MessageValidation("SFU5170", ObjectDic.Instance.GetObjectName("MANUAL")); // %1(을)를 선택하세요.
                //this.cboChangeRoute.Focus();
                return false;
            }

            return returnValue;
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            // 변경하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    if (ValidationCheckRequest())
                    {
                        DataTable dt = DataTableConverter.Convert(this.dgLOTList.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
                        DataTable dtPortInfo = DataTableConverter.Convert(this.cboChangeRoute.ItemsSource);

                        string selectedPortID = this.cboChangeRoute.SelectedValue.ToString();
                        string selectedEqptID = ((System.Data.DataRowView)cboChangeRoute.SelectedItem).Row.ItemArray[0].ToString();

                        this.SetLOTSendOutToManualPortorPacking(dt, selectedEqptID, selectedPortID);        // 수동포트출고예약
                    }
                }
            });
        }

        // BizRule 호출 - 수동포트출고예약
        private bool SetLOTSendOutToManualPortorPacking(DataTable dt, string destEquipmentID, string destPortID)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_TRF_JOB_BY_MES_MANUAL";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUT_DATA";
            DateTime dateTime = this.GetSystemTime();
            DataTable dtMCSBizActorServerInfo = this.GetMCSBizActorServerInfo();

            DataTable dtIN_DATA = new DataTable("IN_DATA");
            dtIN_DATA.Columns.Add("SRC_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("SRC_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("DST_EQPTID", typeof(string));
            dtIN_DATA.Columns.Add("DST_LOCID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIERID", typeof(string));
            dtIN_DATA.Columns.Add("USER", typeof(string));
            dtIN_DATA.Columns.Add("DTTM", typeof(DateTime));
            dtIN_DATA.Columns.Add("PRODID", typeof(string));
            dtIN_DATA.Columns.Add("CARRIER_STRUCT", typeof(string));
            dtIN_DATA.Columns.Add("MDL_TP", typeof(string));
            dtIN_DATA.Columns.Add("STK_ISS_TYPE", typeof(string));

            foreach (DataRowView drv in dt.AsDataView())
            {
                DataRow drIN_DATA = dtIN_DATA.NewRow();
                drIN_DATA["CARRIERID"] = drv["LOTID"].ToString();
                drIN_DATA["SRC_EQPTID"] = drv["LOGIS_EQPTID"].ToString();
                drIN_DATA["SRC_LOCID"] = drv["RACK_ID"].ToString();
                drIN_DATA["DST_EQPTID"] = destEquipmentID;
                drIN_DATA["DST_LOCID"] = destPortID;//  "P8CNV105G009";
                drIN_DATA["USER"] = LoginInfo.USERID;
                drIN_DATA["DTTM"] = dateTime;
                drIN_DATA["PRODID"] = drv["PRODID"].ToString();
                drIN_DATA["CARRIER_STRUCT"] = null;
                drIN_DATA["MDL_TP"] = null;
                drIN_DATA["STK_ISS_TYPE"] = null;
                dtIN_DATA.Rows.Add(drIN_DATA);
            }
            dsINDATA.Tables.Add(dtIN_DATA);

            string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
            try
            {
                foreach (DataRowView drvMCSBizActorServerInfo in dtMCSBizActorServerInfo.AsDataView())
                {
                    ClientProxy clientProxy = new ClientProxy(drvMCSBizActorServerInfo["BIZACTORIP"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPROTOCOL"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORPORT"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEMODE"].ToString()
                                                            , drvMCSBizActorServerInfo["BIZACTORSERVICEINDEX"].ToString());

                    dsOUTDATA = clientProxy.ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                    if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                    {
                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다
                        returnValue = true;
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        private bool SetBoxEqptOperModeChk()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("PORT_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["PORT_ID"] = this.cboChangeRoute.SelectedValue.ToString();
                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOGIS_PORT_EQPT_OPER_MODE", "RQSTDT", "RSLTDT", INDATA);

                if (dsResult.Rows.Count == 0 || dsResult.Rows[0]["EQPT_OPER_MODE"].Equals("MANUAL"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgLOTList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgLOTList);
        }

        private void txtCountQty_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SelectLotList();
        }
        #endregion
    }

    internal class PACK003_027_REQUEST_POPUPLIST_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        public PACK003_027_REQUEST_POPUPLIST_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - Pack Line
        internal DataTable GetPackLineInfo()
        {
            string bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));  // 반송여부(물류타는라인)
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));         // MEB 라인 여부
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));         // 자동 포장 라인 여부

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = "Y";
                drRQSTDT["PACK_BOX_LINE_FLAG"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Stocker 상세 현황 조회
        internal DataTable GetStockerLOTByEOLDateDetail(string productID, string equipmentSegmentID, string fromDate, string toDate, string LOTID, string firstGroupCode, string secondGroupCode)
        {
            string bizRuleName = "BR_PRD_SEL_LOGIS_STK_LOT_BY_EOLDATE_DETL";
            DataTable dtINDATA = new DataTable("INDATA");
            DataTable dtOUTDATA = new DataTable("OUTDATA");

            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("AREAID", typeof(string));
            dtINDATA.Columns.Add("PROD_LIST", typeof(string));
            dtINDATA.Columns.Add("EQSG_LIST", typeof(string));
            dtINDATA.Columns.Add("FROM_DAY", typeof(string));
            dtINDATA.Columns.Add("TO_DAY", typeof(string));
            dtINDATA.Columns.Add("LOTID", typeof(string));
            dtINDATA.Columns.Add("FIRSTGROUPCODE", typeof(string));
            dtINDATA.Columns.Add("SECONDGROUPCODE", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
            drINDATA["PROD_LIST"] = string.IsNullOrEmpty(productID) ? null : productID;
            drINDATA["EQSG_LIST"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
            drINDATA["FROM_DAY"] = string.IsNullOrEmpty(fromDate) ? null : fromDate;
            drINDATA["TO_DAY"] = string.IsNullOrEmpty(toDate) ? null : toDate;
            drINDATA["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
            drINDATA["FIRSTGROUPCODE"] = string.IsNullOrEmpty(firstGroupCode) ? null : firstGroupCode;
            drINDATA["SECONDGROUPCODE"] = string.IsNullOrEmpty(secondGroupCode) ? null : secondGroupCode;

            dtINDATA.Rows.Add(drINDATA);

            dtOUTDATA = new ClientProxy().ExecuteServiceSync(bizRuleName, dtINDATA.TableName, dtOUTDATA.TableName, dtINDATA);

            return dtOUTDATA;
        }
        #endregion
    }
}