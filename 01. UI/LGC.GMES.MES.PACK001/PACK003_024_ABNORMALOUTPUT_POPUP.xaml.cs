/*************************************************************************************
 Created Date : 2022.02.21
      Creator : 김길용
   Decription :
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2022.02.21   김길용        SI           Initial Created.
  2022.06.22   김길용        SI           이동명령 수행시 팝업 수정
  2023.07.14   김길용        SM           E20230516-000486 [WA GMES Pack] 팩물류 연관 수동배출명령 시 MES 이력 추가를 위한 기능수정(개선방안)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Controls.Primitives;



namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_024_ABNORMALOUTPUT_POPUP : C1Window, IWorkArea
    {
        #region "Member Variable & Constructor"
        private StockerDetailDataHelper dataHelper = new StockerDetailDataHelper();
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        DataTable _requestOutStockerInfoTable;
        private string strDstManualPort;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            GetBizActorServerInfo();
        }
        public PACK003_024_ABNORMALOUTPUT_POPUP()
        {
            InitializeComponent();
            intCombo();
        }
        public void intCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //창고명 콤보박스
            # region 창고정보
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQGRID", typeof(string));
            RQSTDT.Columns.Add("EQPTLEVEL", typeof(string));
            RQSTDT.Columns.Add("STOCKERTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = null;
            dr["EQPTLEVEL"] = "M";
            dr["STOCKERTYPE"] = "OCV1_WAIT_STK,OCV_NG_STK,OCV2_WAIT_STK,OCV_WAIT_STK";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_STOCKER_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboStockerID.DisplayMemberPath = "EQPTNAME";
            cboStockerID.SelectedValuePath = "EQPTID";

            DataRow dataRow = dtResult.NewRow();
            dataRow["EQPTID"] = null;
            dataRow["EQPTNAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(dataRow, 0);

            cboStockerID.ItemsSource = dtResult.Copy().AsDataView();
            cboStockerID.SelectedIndex = 0;
            #endregion
        }
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
        #endregion

        #region "Member Function Lists..."
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
       
        private void InitializeCombo()
        {
            try
            {
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
        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "FP_MCS_AP_LOGIS_CONFIG";
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
        private void SelectSearch()
        {
            try
            {
                this.SearchAbnormal(null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SearchAbnormal(string strEqptid)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_CHK_MCS_STK_ERROR_LOT";

                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "IN_DATA";
                indata.Columns.Add("EQPTID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["EQPTID"] = strEqptid;
                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                DataSet resultSet = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync_Multi("BR_CHK_MCS_STK_ERROR_LOT", "IN_DATA", "OUT_LOT_LIST,OUT_LOT_SUM", ds);
                if (resultSet != null && resultSet.Tables["OUT_LOT_SUM"].Rows.Count > 0)
                {
                    // Form 로드시에는 처리버튼 비활성
                    if (strEqptid == null)
                    {
                        this.btnSavebtm.IsEnabled = false;
                        Util.GridSetData(dgList, resultSet.Tables["OUT_LOT_SUM"], FrameOperation, false);
                    }
                    else
                    {
                        btnSavebtm.IsEnabled = true;
                        _requestOutStockerInfoTable = resultSet.Tables["OUT_LOT_LIST"];
                        int rtxtCountQty = (int)(resultSet.Tables["OUT_LOT_SUM"].Rows[0].ItemArray)[1];
                        txtCountQty.Value = rtxtCountQty;
                    }
                }
                else
                {
                    btnSavebtm.IsEnabled = false;
                }
                
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetManualPortInfo(string strEqptid)
        {
            try
            {
                string bizRuleName = "DA_MCS_SEL_PORT_OPT";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                dtRQSTDT.Columns.Add("PORT_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["EQPTID"] = strEqptid;
                drRQSTDT["PORT_TYPE_CODE"] = "STK_MD_MGV";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    strDstManualPort = (dtRSLTDT.Rows[0]).ItemArray[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetManualOutPut_Order()
        {
            try
            {
            const string bizRuleName = "BR_PRD_REG_TRF_JOB_BY_MES_MANUAL";

            int Cnt = (int)txtCountQty.Value;
            int Position = 0;
            DateTime dtSystem = GetSystemTime();
            DataTable inTable = new DataTable("IN_DATA");
            inTable.Columns.Add("SRC_EQPTID", typeof(string));
            inTable.Columns.Add("SRC_LOCID", typeof(string));
            inTable.Columns.Add("DST_EQPTID", typeof(string));
            inTable.Columns.Add("DST_LOCID", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("DTTM", typeof(DateTime));
            inTable.Columns.Add("STK_ISS_TYPE", typeof(string));

                foreach (DataRow row in _requestOutStockerInfoTable.Rows)
                {
                    if (Position < Cnt)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SRC_EQPTID"] = _requestOutStockerInfoTable.Rows[Position].ItemArray[0].ToString();
                        newRow["SRC_LOCID"] = _requestOutStockerInfoTable.Rows[Position].ItemArray[3].ToString();
                        newRow["DST_EQPTID"] = cboStockerID.SelectedValue.ToString();
                        newRow["DST_LOCID"] = strDstManualPort.ToString();
                        newRow["CARRIERID"] = _requestOutStockerInfoTable.Rows[Position].ItemArray[2].ToString();
                        newRow["USER"] = LoginInfo.USERID;
                        newRow["DTTM"] = dtSystem;
                        newRow["STK_ISS_TYPE"] = "SHIP";
                        inTable.Rows.Add(newRow);
                        Position++;
                    }
                }
            new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_DATA", "OUT_DATA", inTable, (result, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
}
        private bool ValidationCheck()
        {
            bool returnValue = true;

            if (cboStockerID.SelectedIndex == 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                return false;
            }
            if (this.dgAbnormalList == null || this.dgList.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }
            
            if ((int)txtCountQty.Value == 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                return false;
            }

            return returnValue;
        }
        #endregion

        #region "Events..."
        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                string[] tmp = null;
                SelectSearch();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnSavebtm_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU2924", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (!this.ValidationCheck())
                    {
                        return;
                    }
                    this.GetManualOutPut_Order();
                }
            });
        }
        private void cboStockerID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (this.dgAbnormalList == null)
            {
                return;
            }

            if (this.cboStockerID.SelectedValue == null)
            {
                this.txtCountQty.IsEnabled = false;
                this.btnSavebtm.IsEnabled = false;
                return;
            }

            this.txtCountQty.IsEnabled = true;
            this.SearchAbnormal(this.cboStockerID.SelectedValue.ToString());
            this.GetManualPortInfo(this.cboStockerID.SelectedValue.ToString());

            //if (dgAbnormalList != null)
            //{
            //    if (cboStockerID.SelectedIndex > 0) txtCountQty.IsEnabled = true;
            //    this.SearchAbnormal(cboStockerID.SelectedValue.ToString());
            //    this.GetManualPortInfo(cboStockerID.SelectedValue.ToString());
            //}
        }
        #endregion


    }
}