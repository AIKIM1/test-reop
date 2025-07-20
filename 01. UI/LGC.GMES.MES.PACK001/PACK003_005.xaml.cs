/*************************************************************************************
 Created Date : 2020.11.16
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR         Description...
  2020.11.16    담당자       SI             Initial Created (MCS001_006 : 라미대기창고 모니터링 참조 화면 구성)
  2021.01.22    김길용A      SI             보관일수에 따른 색상변경, 창고재고 조회조건 추가 및 컬럼추가, 출고대상 Carrier정보 컬럼추가, Storker정보 수정 및 추가
  2021.02.04    정용석       SI             범례 추가 및 Stocker Layout 부분 레이아웃 변경. 기타 수정 내역 반영
  2021.03.18    김길용       SI             수동출고 예약취소 추가 (JOB_FAIL 일 때만 취소 가능) - 출고Carrier 예약 정보 체크 시 표출
  2021.04.02    김길용       SI             PKG전극설비정보 항목 추가 (PKGSHORTNAME)
  2021.10.22    김길용       SI             Pack3동 동기화
  2022.03.31    김길용       SI             ESWA Pack2동 Phase2 3창고를 위한 수동출고포트 정보 수정
  2023.07.14    김길용       SM            E20230516-000486 [WA GMES Pack] 팩물류 연관 수동배출명령 시 MES 이력 추가를 위한 기능수정(개선방안)
  2024.09.09    권성혁       E20240822-001209  [E20240822-001209] EV2020 87 (E129A) 셀 입고기간 혼입 최소화_Module 
  2024.10.22    권성혁       E20241021-000318  [PACK] MES CelL Stocker 관리 화면 오류 수정의 건.. 언어 변경시 Rack색상 설명 내용 겹침
  2024.11.20    권성혁       E20241115-001277   팩 Cell Stocker 관리 화면 오류수정건
  2024.12.25    이헌수       SI             MI CELL TARY 물류 대응을 위한 수정    
  2025.04.14    김선준       SI            ReTray('R')/Return('B') 상단에 표시   
**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        // 모니터링용 타이머 관련
        DispatcherTimer dispatcherTimer = new DispatcherTimer();    // Timer
        //Rack 기본 셋팅
        private int MAX_VAR = 0;
        private int MAX_ROW = 0;
        private int MAX_COL = 0;
        //UserControl 
        private Rack_CheckBox AssmRack = new Rack_CheckBox();
        // Type : PORT 인지 RACK 인지
        private static string sType = string.Empty;
        //유저 컨트롤 버튼에 대한 정보 셋팅
        Button btnClick = new Button();
        //설비
        private string sW1ANPW101 = string.Empty;
        //비즈 Config
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        //긴급출고여부
        private bool Emerg_Flag = false;
        private DataTable _requestOutRackInfoTable;

        //창고재고 체크박스
        string Strtot_Line = string.Empty;
        string Strtot_Prod = string.Empty;

        //Storker재고변수
        int Inup_One;
        int InBot_One;

        //편차 시간
        string limit_gap_date = null;
        DataTable dtCommon;

        //분기 변수 : True : MI/ST RTD BIZ 호출 // False : 폴란드 MCS BIZ 호출 
        bool _DiffusionSiteFlag = false;

        public PACK003_005()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btn_OutPut);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            sType = string.Empty;
            this.setTextBlock();
            if (this.txtCycle != null)
            {
                this.timerSetting(this.txtCycle);
            }
            InitCombo();
            Refresh(true);
            Limit_Gap_Time();

            this.Loaded -= UserControl_Loaded;
        }

        private void timerSetting(C1NumericBox newmericBox)
        {
            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
                this.dispatcherTimer.Tick -= dispatcherTimer_Tick;
                int intervalMinute = Convert.ToInt32(newmericBox.Value);
                this.dispatcherTimer.Tick += dispatcherTimer_Tick;
                this.dispatcherTimer.Interval = new TimeSpan(0, intervalMinute, 0);
                this.dispatcherTimer.Start();
            }
        }

        private void ExecuteProcess()
        {
            this.dispatcherTimer.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (this.dispatcherTimer != null)
                    {
                        this.dispatcherTimer.Stop();
                    }
                    if (this.dispatcherTimer.Interval.TotalSeconds == 0)
                    {
                        return;
                    }
                    this.Refresh(true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (this.dispatcherTimer != null && this.dispatcherTimer.Interval.TotalSeconds > 0)
                    {
                        this.dispatcherTimer.Start();
                    }
                }
            }));
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            # region 창고재고정보 Combo -라인
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_STK_LINE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboEqsgid.DisplayMemberPath = "CBO_NAME";
            cboEqsgid.SelectedValuePath = "CBO_CODE";

            DataRow dataRow = dtResult.NewRow();
            dataRow["CBO_CODE"] = null;
            dataRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(dataRow, 0);

            cboEqsgid.ItemsSource = dtResult.Copy().AsDataView();
            cboEqsgid.SelectedIndex = 0;
            #endregion

            # region 창고재고정보 Combo -제품
            DataTable RQSTDT2 = new DataTable();
            RQSTDT2.TableName = "RQSTDT";
            RQSTDT2.Columns.Add("LANGID", typeof(string));
            RQSTDT2.Columns.Add("AREAID", typeof(string));

            DataRow dr2 = RQSTDT2.NewRow();
            dr2["LANGID"] = LoginInfo.LANGID;
            dr2["AREAID"] = LoginInfo.CFG_AREA_ID;

            RQSTDT2.Rows.Add(dr2);

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOGIS_WH_PACK", "RQSTDT", "RSLTDT", RQSTDT2);
            cboWhInfo.DisplayMemberPath = "CBO_NAME";
            cboWhInfo.SelectedValuePath = "CBO_CODE";

            DataRow dataRow2 = dtResult2.NewRow();
            dataRow2["CBO_CODE"] = null;
            dataRow2["CBO_NAME"] = "-ALL-";
            dtResult2.Rows.InsertAt(dataRow2, 0);

            cboWhInfo.ItemsSource = dtResult2.Copy().AsDataView();
            cboWhInfo.SelectedIndex = 0;
            #endregion

            #region 분기 변수 초기화 : True : MI/ST RTD BIZ 호출 // False : 폴란드 MCS BIZ 호출 
            DataTable dtDiffusionSite = new DataTable();
            dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "AUTO_LOGIS");

            string shop_id = string.Empty;

            if (dtDiffusionSite.Rows.Count > 0)
            {
                shop_id = dtDiffusionSite.Rows[0]["ATTRIBUTE1"].ToString();

                if (shop_id.Contains(LoginInfo.CFG_SHOP_ID))
                {
                    _DiffusionSiteFlag = true; //[공통코드 - DIFFUSION_SITE - AUTO_LOGIS - ATTR1]에 등록된 Shop 만 Ex) MI ST
                }
            }
            #endregion

            GetintoAreaEqpt();
            GetintoAreaOutPort(cboStkid.SelectedValue.ToString());
        }

        private void GetintoAreaOutPort(string strEqptid)
        {
            #region 출고포트 조회
            DataTable RQSTDT3 = new DataTable();
            RQSTDT3.TableName = "RQSTDT";
            RQSTDT3.Columns.Add("LANGID", typeof(string));
            RQSTDT3.Columns.Add("AREAID", typeof(string));
            RQSTDT3.Columns.Add("EQPTID", typeof(string));
            RQSTDT3.Columns.Add("PORT_TYPE_CODE", typeof(string));

            DataRow dr3 = RQSTDT3.NewRow();
            dr3["LANGID"] = LoginInfo.LANGID;
            dr3["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr3["EQPTID"] = strEqptid == null ? null : strEqptid;
            dr3["PORT_TYPE_CODE"] = "STK_PLT_MGV";
            RQSTDT3.Rows.Add(dr3);

            DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_PORT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT3);
            cboOutPort.DisplayMemberPath = "CBO_NAME";
            cboOutPort.SelectedValuePath = "CBO_CODE";

            DataRow dataRow3 = dtResult3.NewRow();
            dataRow3["CBO_CODE"] = string.Empty;
            dataRow3["CBO_NAME"] = "-SELECT-";
            dtResult3.Rows.InsertAt(dataRow3, 0);

            cboOutPort.ItemsSource = dtResult3.Copy().AsDataView();
            cboOutPort.SelectedIndex = 0;
            #endregion
        }
        private void GetintoAreaEqpt()
        {
            #region Cell STK 콤보조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PACK_LOGIS_CELLSTOCKER_INFO";
            dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1", "RQSTDT", "RSLTDT", RQSTDT);
            cboStkid.DisplayMemberPath = "CBO_NAME";
            cboStkid.SelectedValuePath = "CBO_CODE";

            cboStkid.ItemsSource = dtResult.Copy().AsDataView();
            cboStkid.SelectedIndex = 0;
            #endregion

            sW1ANPW101 = dtResult.Rows[0]["CBO_CODE"].ToString();
            MAX_VAR = GetVarCount(sW1ANPW101);
            MAX_ROW = int.Parse(dtResult.Rows[0]["ATTRIBUTE2"].ToString());
            MAX_COL = int.Parse(dtResult.Rows[0]["ATTRIBUTE3"].ToString());
            txtEqptID_1.Text = dtResult.Rows[0]["CBO_NAME"].ToString();
        }
        //Stocker  위치정보에 대한 기준정보
        private void GetintoAreaEqpt_Change(string strEqptid)
        {
            #region Cell STK 콤보조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CBO_CODE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PACK_LOGIS_CELLSTOCKER_INFO";
            dr["CBO_CODE"] = strEqptid == null ? null : strEqptid;
            dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);
            cboStkid.DisplayMemberPath = "CBO_NAME";
            cboStkid.SelectedValuePath = "CBO_CODE";


            sW1ANPW101 = dtResult.Rows[0]["CBO_CODE"].ToString();
            MAX_VAR = GetVarCount(sW1ANPW101);
            MAX_ROW = int.Parse(dtResult.Rows[0]["ATTRIBUTE2"].ToString());
            MAX_COL = int.Parse(dtResult.Rows[0]["ATTRIBUTE3"].ToString());
            txtEqptID_1.Text = dtResult.Rows[0]["CBO_NAME"].ToString();
            #endregion

        }
        #region 수동출고 비즈 CONFIG 설정
        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            //dr["KEYGROUPID"] = "MCS_AP_TEST_CONFIG";    // TEST
            dr["KEYGROUPID"] = "FP_MCS_AP_CONFIG";      // 운영


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
        #endregion
        #region 시간동기화
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
        #endregion

        // RACK 색상 설명
        private void setTextBlock()
        {
            string abnormalText = ObjectDic.Instance.GetObjectName("비정상");
            txtAbnomal1.Text = "[" + abnormalText + "] " + ObjectDic.Instance.GetObjectName("D+3미만");
            txtAbnomal2.Text = "[" + abnormalText + "] " + ObjectDic.Instance.GetObjectName("D+3이상");
            txtOcvAbnomal1.Text = "[" + abnormalText + "] " + ObjectDic.Instance.GetObjectName("편차일 값 없음");
            txtOcvAbnomal2.Text = "[" + abnormalText + "] " + ObjectDic.Instance.GetObjectName("편차일 설정기준 초과");
        }



        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //조회조건에 LOT이 있으면 LOT을 조회해서 선택
            //아니면 전체 초기화
            if (txtLotS.Text == string.Empty)
            {
                Refresh(true);
            }
            else
            {
                SelectLOT();
            }
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region 출고대상조회 버튼 : btnSearch_OutPut_Click()
        /// <summary>
        /// 출고대상 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_OutPut_Click(object sender, RoutedEventArgs e)
        {
            SelectOutRackList(true, "U", null, null);
        }
        #endregion

        #region 창고정보조회(제품콤보박스)  : cboWhInfo_SelectedValueChanged()
        /// <summary>
        /// 창고정보 조회 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboWhInfo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            WareHouseBind();
            Chkfresh(true);
        }
        #endregion

        #region 창고정보조회(라인콤보박스)  : cboEqsgid_SelectedValueChanged()
        private void cboEqsgid_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            WareHouseBind();
            Chkfresh(true);
        }
        #endregion

        #region  창고정보조회(마우스더블클릭: 입고LOT 조회팝업) : dgWhereHose_MouseDoubleClick()  
        private void popupInputLot_Closed(object sender, EventArgs e)
        {
            PACK003_005_INPUT_INFO popup = sender as PACK003_005_INPUT_INFO;
            if (popup != null)
            {
            }
        }
        #endregion

        #region 창고정보조회(글자색) : dgWhereHose_LoadedCellPresenter()
        /// <summary>
        /// 글자색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWareHouse_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgWareHouse.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Brown);
                }
            }));
        }
        #endregion

        #region  LOT 조회조건 Key Down : txtLotS_KeyDown()
        /// <summary>
        /// LOT Text box KeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectLOT();
            }
        }
        #endregion

        #region Timer 관리 : cboReset_SelectedValueChanged(), OnUnloaded(), OnMonitorTimer()

        private void OnMonitorTimer(object sender, EventArgs e)
        {
            try
            {
                this.Refresh(false);
            }
            catch (Exception ex)
            {
                Util.Alert("Timer_Err" + ex.ToString());
            }
        }

        #endregion

        #region RACK 선택 : assmRack_Checked()
        /// <summary>
        /// RACK 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assmRack_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //Thread.Sleep(2000);
                Rack_CheckBox assmRack = sender as Rack_CheckBox;
                if (assmRack == null) return;
                if (assmRack.Wip_Remarks.ToString() == "N") return;
                if (assmRack.IsChecked)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACKID", typeof(string));
                    RQSTDT.Columns.Add("LANGID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RACKID"] = assmRack.RackId.ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    RQSTDT.Rows.Add(dr);
                    //RACK으로 LOT정보 조회
                    new ClientProxy().ExecuteService("DA_PRD_SEL_PACK_LOGIS_STK_INPUTLOT", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (result.Rows.Count > 0)
                        {
                            if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우 LOT ADD
                            {
                                try
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                                    DataRow newRow = null;
                                    newRow = dtSource.NewRow();
                                    newRow["RACK_ID"] = result.Rows[0]["RACK_ID_2"];
                                    newRow["DIV"] = result.Rows[0]["DIV"];
                                    newRow["CHK"] = 0;
                                    newRow["RACK_ID"] = result.Rows[0]["RACK_ID"];
                                    newRow["RACK_NAME"] = result.Rows[0]["RACK_NAME"];
                                    newRow["EQPTID"] = result.Rows[0]["EQPTID"];
                                    newRow["EQPTNAME"] = result.Rows[0]["EQPTNAME"];
                                    newRow["X_PSTN"] = result.Rows[0]["X_PSTN"];
                                    newRow["Y_PSTN"] = result.Rows[0]["Y_PSTN"];
                                    newRow["Z_PSTN"] = result.Rows[0]["Z_PSTN"];
                                    newRow["ZONE_ID"] = result.Rows[0]["ZONE_ID"];
                                    newRow["PRODID"] = result.Rows[0]["PRODID"];
                                    newRow["PRJT_NAME"] = result.Rows[0]["PRJT_NAME"];
                                    newRow["MODLID"] = result.Rows[0]["MODLID"];
                                    newRow["CSTID"] = result.Rows[0]["CSTID"];
                                    newRow["CSTPROD"] = result.Rows[0]["CSTPROD"];
                                    newRow["CSTSTAT"] = result.Rows[0]["CSTSTAT"];
                                    newRow["CSTPROD_NAME"] = result.Rows[0]["CSTPROD_NAME"];
                                    newRow["CSTSTAT_NAME"] = result.Rows[0]["CSTSTAT_NAME"];
                                    newRow["PLTID"] = result.Rows[0]["PLTID"];
                                    newRow["LOTCNT"] = result.Rows[0]["LOTCNT"];
                                    newRow["WIPHOLD"] = result.Rows[0]["WIPHOLD"];
                                    newRow["RACK_STAT_CODE"] = result.Rows[0]["RACK_STAT_CODE"];
                                    newRow["EQSGID"] = result.Rows[0]["EQSGID"];
                                    newRow["EQSGNAME"] = result.Rows[0]["EQSGNAME"];
                                    newRow["STORE_DAY_GAP"] = result.Rows[0]["STORE_DAY_GAP"];
                                    newRow["BLDG_CODE"] = result.Rows[0]["BLDG_CODE"];
                                    newRow["BLDG_NAME"] = result.Rows[0]["BLDG_NAME"];
                                    newRow["COTSHORTNAME"] = result.Rows[0]["COTSHORTNAME"];
                                    newRow["PKGSHORTNAME"] = result.Rows[0]["PKGSHORTNAME"];

                                    dtSource.Rows.Add(newRow);

                                    Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);
                                }
                                catch (Exception ex1)
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex1), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                }

                            }
                            else
                            {
                                DataTable dt = new DataTable();
                                dt.Columns.Add("RACK_ID_2", typeof(string));
                                dt.Columns.Add("DIV", typeof(string));
                                dt.Columns.Add("CHK", typeof(string));
                                dt.Columns.Add("RACK_ID", typeof(string));
                                dt.Columns.Add("RACK_NAME", typeof(string));
                                dt.Columns.Add("EQPTID", typeof(string));
                                dt.Columns.Add("EQPTNAME", typeof(string));
                                dt.Columns.Add("X_PSTN", typeof(string));
                                dt.Columns.Add("Y_PSTN", typeof(string));
                                dt.Columns.Add("Z_PSTN", typeof(string));
                                dt.Columns.Add("ZONE_ID", typeof(string));
                                dt.Columns.Add("PRODID", typeof(string));
                                dt.Columns.Add("PRJT_NAME", typeof(string));
                                dt.Columns.Add("MODLID", typeof(string));
                                dt.Columns.Add("CSTID", typeof(string));
                                dt.Columns.Add("CSTPROD", typeof(string));
                                dt.Columns.Add("CSTSTAT", typeof(string));
                                dt.Columns.Add("CSTPROD_NAME", typeof(string));
                                dt.Columns.Add("CSTSTAT_NAME", typeof(string));
                                dt.Columns.Add("PLTID", typeof(string));
                                dt.Columns.Add("LOTCNT", typeof(string));
                                dt.Columns.Add("WIPHOLD", typeof(string));
                                dt.Columns.Add("RACK_STAT_CODE", typeof(string));
                                dt.Columns.Add("EQSGID", typeof(string));
                                dt.Columns.Add("EQSGNAME", typeof(string));
                                dt.Columns.Add("STORE_DAY_GAP", typeof(string));
                                dt.Columns.Add("BLDG_CODE", typeof(string));
                                dt.Columns.Add("BLDG_NAME", typeof(string));
                                dt.Columns.Add("COTSHORTNAME", typeof(string));
                                dt.Columns.Add("PKGSHORTNAME", typeof(string));
                                DataRow newRow = null;
                                newRow = dt.NewRow();

                                newRow["RACK_ID"] = result.Rows[0]["RACK_ID_2"];
                                newRow["DIV"] = result.Rows[0]["DIV"];
                                newRow["CHK"] = 0;
                                newRow["RACK_ID"] = result.Rows[0]["RACK_ID"];
                                newRow["RACK_NAME"] = result.Rows[0]["RACK_NAME"];
                                newRow["EQPTID"] = result.Rows[0]["EQPTID"];
                                newRow["EQPTNAME"] = result.Rows[0]["EQPTNAME"];
                                newRow["X_PSTN"] = result.Rows[0]["X_PSTN"];
                                newRow["Y_PSTN"] = result.Rows[0]["Y_PSTN"];
                                newRow["Z_PSTN"] = result.Rows[0]["Z_PSTN"];
                                newRow["ZONE_ID"] = result.Rows[0]["ZONE_ID"];
                                newRow["PRODID"] = result.Rows[0]["PRODID"];
                                newRow["PRJT_NAME"] = result.Rows[0]["PRJT_NAME"];
                                newRow["MODLID"] = result.Rows[0]["MODLID"];
                                newRow["CSTID"] = result.Rows[0]["CSTID"];
                                newRow["CSTPROD"] = result.Rows[0]["CSTPROD"];
                                newRow["CSTSTAT"] = result.Rows[0]["CSTSTAT"];
                                newRow["CSTPROD_NAME"] = result.Rows[0]["CSTPROD_NAME"];
                                newRow["CSTSTAT_NAME"] = result.Rows[0]["CSTSTAT_NAME"];
                                newRow["PLTID"] = result.Rows[0]["PLTID"];
                                newRow["LOTCNT"] = result.Rows[0]["LOTCNT"];
                                newRow["WIPHOLD"] = result.Rows[0]["WIPHOLD"];
                                newRow["RACK_STAT_CODE"] = result.Rows[0]["RACK_STAT_CODE"];
                                newRow["EQSGID"] = result.Rows[0]["EQSGID"];
                                newRow["EQSGNAME"] = result.Rows[0]["EQSGNAME"];
                                newRow["STORE_DAY_GAP"] = result.Rows[0]["STORE_DAY_GAP"];
                                newRow["BLDG_CODE"] = result.Rows[0]["BLDG_CODE"];
                                newRow["BLDG_NAME"] = result.Rows[0]["BLDG_NAME"];
                                newRow["COTSHORTNAME"] = result.Rows[0]["COTSHORTNAME"];
                                newRow["PKGSHORTNAME"] = result.Rows[0]["PKGSHORTNAME"];
                                dt.Rows.Add(newRow);

                                Util.GridSetData(dgLotInfo, dt, FrameOperation, false);

                                //축소
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);

                                btnBottomExpandFrame.IsChecked = true;

                            }
                        }


                        loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    DataTable _dtRackInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + assmRack.RackId + "'");

                    foreach (DataRow row in selectedRow)
                    {
                        _dtRackInfo.Rows.Remove(row);
                    }
                    Util.GridSetData(dgLotInfo, _dtRackInfo, FrameOperation, false);

                    if (dgLotInfo.Rows.Count == 0)
                    {
                        //확장
                        Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                        Monitoring.RowDefinitions[1].Height = new GridLength(8);
                        Monitoring.RowDefinitions[2].Height = new GridLength(8.3, GridUnitType.Star);
                        Monitoring.RowDefinitions[3].Height = new GridLength(8);
                        Monitoring.RowDefinitions[4].Height = new GridLength(31);

                        btnBottomExpandFrame.IsChecked = false;
                    }



                    loadingIndicator.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        //창고재고체크 시 그리드 출력을 위한 동작
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgWareHouseStock as C1DataGrid;
                Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, "0");
                for (int i = 2; i < dg.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        //LINE,PROD BINDING
                        string e_line = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[i].DataItem, "EQSGID")) + ",";
                        string e_prod = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[i].DataItem, "PRODID")) + ",";
                        Strtot_Line = Strtot_Line + e_line;
                        Strtot_Prod = Strtot_Prod + e_prod;
                    }
                }
                int checkedIndex = Util.gridFindDataRow(ref this.dgWareHouseStock, "CHK", "True", false);
                if (checkedIndex < 2)
                {
                    return;
                }
                Chkfresh(true);
            }
            catch
            {

            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgWareHouseStock as C1DataGrid;

                Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, "0");
                for (int i = 2; i < dg.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        string e_line = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[i].DataItem, "EQSGID")) + ",";
                        string e_prod = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[i].DataItem, "PRODID")) + ",";
                        Strtot_Line = Strtot_Line + e_line;
                        Strtot_Prod = Strtot_Prod + e_prod;
                    }
                }
                Chkfresh(true);
            }
            catch
            {
            }
        }

        #endregion

        #region 출고대상Carrier정보(USING상태일경우만 수동출고가능) : dgLotInfo_BeginningEdit()
        private void dgLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgLotInfo.CurrentCell != null && dgLotInfo.SelectedIndex > -1)
            {
                if (dgLotInfo.CurrentCell.Column.Name == "CHK")
                {
                    string sStatCode = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["RACK_STAT_CODE"].Index).Value);
                    if (sStatCode == "USING")
                    {
                        e.Cancel = false;   // Editing 가능

                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }
        }

        #endregion

        #region 수동출고예약 : btn_OutPut_Click()
        /// <summary>
        /// 수동출고예약
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OutPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationReturn())
                    return;
                string port = (string)cboOutPort.SelectedValue;
                if (chkEship.IsChecked == true)
                {
                    Emerg_Flag = true;
                }
                //ShowLoadingIndicator();

                #region 분기 전 필요 변수 초기화 
                string bizRuleName = string.Empty;
                DataTable inTable = new DataTable("IN_DATA");
                string outData = (_DiffusionSiteFlag) ? "" : "OUT_DATA";
                ClientProxy ClientProxy = (_DiffusionSiteFlag) ? new ClientProxy() : new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex);
                #endregion

                if (_DiffusionSiteFlag)
                { //[공통코드 - DIFFUSION_SITE - AUTO_LOGIS - ATTR1]에 등록된 Shop 만 Ex) MI ST
                    bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI_2";

                    DateTime dtSystem = GetSystemTime();

                    inTable.Columns.Add("SRC_EQPTID", typeof(string));
                    inTable.Columns.Add("SRC_PORTID", typeof(string));
                    inTable.Columns.Add("DST_EQPTID", typeof(string));
                    inTable.Columns.Add("DST_PORTID", typeof(string));
                    inTable.Columns.Add("PRIORITY_NO", typeof(string));
                    inTable.Columns.Add("CARRIERID", typeof(string));
                    inTable.Columns.Add("UPDUSER", typeof(string));
                    inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                    inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));

                    string strOutportLoc = ((DataRowView)cboOutPort.SelectedItem).Row.ItemArray[0].ToString();
                    string strOutportEqpt = ((DataRowView)cboOutPort.SelectedItem).Row.ItemArray[2].ToString();
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                        {
                            DataRow newRow = inTable.NewRow();

                            newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                            newRow["SRC_PORTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString() + "-SN0";
                            newRow["DST_EQPTID"] = strOutportEqpt;
                            newRow["DST_PORTID"] = strOutportLoc;
                            newRow["UPDUSER"] = LoginInfo.USERID;
                            newRow["LANGID"] = LoginInfo.LANGID;
                            //newRow["DTTM"] = dtSystem;
                            //newRow["PRODID"] = DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();
                            //newRow["CARRIER_STRUCT"] = DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
                            //newRow["MDL_TP"] = DataTableConverter.GetValue(row.DataItem, "CSTPROD").GetString();
                            //newRow["STK_ISS_TYPE"] = Emerg_Flag == true ? "EML" : "SHIP";

                            inTable.Rows.Add(newRow);
                        }
                    }
                }
                else
                {
                    bizRuleName = "BR_PRD_REG_TRF_JOB_BY_MES_MANUAL";

                    DateTime dtSystem = GetSystemTime();

                    inTable.Columns.Add("SRC_EQPTID", typeof(string));
                    inTable.Columns.Add("SRC_LOCID", typeof(string));
                    inTable.Columns.Add("DST_EQPTID", typeof(string));
                    inTable.Columns.Add("DST_LOCID", typeof(string));
                    inTable.Columns.Add("CARRIERID", typeof(string));
                    inTable.Columns.Add("USER", typeof(string));
                    inTable.Columns.Add("DTTM", typeof(DateTime));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("CARRIER_STRUCT", typeof(string));
                    inTable.Columns.Add("MDL_TP", typeof(string));
                    inTable.Columns.Add("STK_ISS_TYPE", typeof(string));

                    string strOutportLoc = ((DataRowView)cboOutPort.SelectedItem).Row.ItemArray[0].ToString();
                    string strOutportEqpt = ((DataRowView)cboOutPort.SelectedItem).Row.ItemArray[2].ToString();
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                            newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                            newRow["DST_EQPTID"] = strOutportEqpt;
                            newRow["DST_LOCID"] = strOutportLoc;
                            newRow["USER"] = LoginInfo.USERID;
                            newRow["DTTM"] = dtSystem;
                            newRow["PRODID"] = DataTableConverter.GetValue(row.DataItem, "PRODID").GetString();
                            newRow["CARRIER_STRUCT"] = DataTableConverter.GetValue(row.DataItem, "CSTSTAT").GetString();
                            newRow["MDL_TP"] = DataTableConverter.GetValue(row.DataItem, "CSTPROD").GetString();
                            newRow["STK_ISS_TYPE"] = Emerg_Flag == true ? "EML" : "SHIP";

                            inTable.Rows.Add(newRow);

                        }
                    }

                }

                ClientProxy.ExecuteService(bizRuleName, "IN_DATA", outData, inTable, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            Emerg_Flag = false;
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다
                        Refresh(true);
                        //확장
                        Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                        Monitoring.RowDefinitions[1].Height = new GridLength(8);
                        Monitoring.RowDefinitions[2].Height = new GridLength(8.3, GridUnitType.Star);
                        Monitoring.RowDefinitions[3].Height = new GridLength(8);
                        Monitoring.RowDefinitions[4].Height = new GridLength(31);
                        btnBottomExpandFrame.IsChecked = false;
                        Emerg_Flag = false;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        Emerg_Flag = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                Emerg_Flag = false;
            }
        }

        #endregion

        #region =============== 팝업 및 화면 이동 ==============

        #region  Rack정보조회  : BtnUserControl_Click(),assmRack_DoubleClick()
        private void BtnUserControl_Click(object sender, RoutedEventArgs e)
        {
        }
        /// <summary>
        /// RACK 더블클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assmRack_DoubleClick(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                Rack_CheckBox assmRack = sender as Rack_CheckBox;
                if (string.IsNullOrEmpty(assmRack.RackId.ToString()) || string.IsNullOrEmpty(assmRack.PalletID.ToString()))
                {
                    return;
                }

                sType = "RACK";

                PACK003_005_LOT_INFO popupLotInfo = new PACK003_005_LOT_INFO();
                popupLotInfo.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];
                parameters[0] = assmRack.RackId.ToString(); //RACKID
                parameters[1] = sType;

                C1WindowExtension.SetParameters(popupLotInfo, parameters);

                popupLotInfo.Closed += new EventHandler(popupLotInfo_Closed);
                popupLotInfo.Show();
                popupLotInfo.CenterOnScreen();
                loadingIndicator.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///  Rack정보조회 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupLotInfo_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            PACK003_005_LOT_INFO popupLotInfo = sender as PACK003_005_LOT_INFO;
            if (popupLotInfo.DialogResult == MessageBoxResult.OK)
            {
                Refresh(false);
            }
            this.grdMain.Children.Remove(popupLotInfo);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #endregion

        #region ============= 확장/축소 ===============

        #region 좌측축소 : btnLeftExpandFrame_Checked()
        /// <summary>
        ///  좌측축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            WhereHose.ColumnDefinitions[0].Width = new GridLength(2.7, GridUnitType.Star);
            WhereHose.ColumnDefinitions[1].Width = new GridLength(8);
            WhereHose.ColumnDefinitions[2].Width = new GridLength(7.3, GridUnitType.Star);
        }
        #endregion

        #region 좌측확장 : btnLeftExpandFrame_Unchecked()
        /// <summary>
        /// 좌측확장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            // 확장
            WhereHose.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            WhereHose.ColumnDefinitions[1].Width = new GridLength(0);
            WhereHose.ColumnDefinitions[2].Width = new GridLength(10.0, GridUnitType.Star);
        }

        #endregion

        #region 하단축소 : btnBottomExpandFrame_Checked()
        /// <summary>
        /// 하단 축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBottomExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            //축소
            Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
            Monitoring.RowDefinitions[1].Height = new GridLength(8);
            Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
            Monitoring.RowDefinitions[3].Height = new GridLength(8);
            Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
        }
        #endregion

        #region 하단확장 : btnBottomExpandFrame_Unchecked()
        /// <summary>
        /// 하단 확장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBottomExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            //확장
            Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
            Monitoring.RowDefinitions[1].Height = new GridLength(8);
            Monitoring.RowDefinitions[2].Height = new GridLength(8.3, GridUnitType.Star);
            Monitoring.RowDefinitions[3].Height = new GridLength(8);
            Monitoring.RowDefinitions[4].Height = new GridLength(31);

        }
        #endregion

        #endregion

        #endregion

        #region Method

        #region MAX POSITION : MAX_POSITION()
        /// <summary>
        /// MAX POSITION
        /// </summary>
        /// <returns></returns>
        private DataTable MAX_POSITION(string Eqpt, string Zone)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = Eqpt;
            dr["ZONE_ID"] = Zone;
            dtRqst.Rows.Add(dr);
            return new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_XYPOSITION_LAMI", "INDATA", "OUTDATA", dtRqst);
        }
        #endregion

        #region 재조회  : Refresh()
        //체크동작으로 인한 조회
        private void Chkfresh(bool bButton)
        {
            try
            {
                if (bButton == true)
                {
                    Inup_One = 0;
                    InBot_One = 0;
                    setGrid(MAX_VAR, bButton);
                }
            }
            catch
            {
            }
        }

        private void Refresh(bool bButton)
        {
            try
            {
                //버튼 조회시
                if (bButton == true)
                {
                    //초기화
                    Clear();
                    Inup_One = 0;
                    InBot_One = 0;
                    setGrid(MAX_VAR, bButton);
                    WareHouseBind(); //창고재고 정보
                    this.SelectOutRackList(true, "U", null, null);
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Time 셋팅 : TimerSetting()
        #endregion

        #region 설비명조회 : EqptName()
        private string EqptName(string EqptID)
        {
            string ReturnEqptName = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID.ToString();
            dr["EQPTID"] = EqptID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EQPTNAME", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult.Rows.Count > 0)
            {
                ReturnEqptName = dtResult.Rows[0]["EQPTNAME"].ToString();
            }
            return ReturnEqptName;

        }
        #endregion

        #region  초기화 :  Clear()
        /// <summary>
        /// 초기화
        /// </summary>
        private void Clear()
        {
            //상단 Clear
            Util.gridClear(dgLotInfo);
            //좌측 Clear
            Util.gridClear(dgWareHouse);
            Util.gridClear(dgWareHouseStock);
            //하단 Clear
            Util.gridClear(dgOutputInfo);
            Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, "0");
            sType = string.Empty;
            btnClick.Content = null;
            Strtot_Line = null;
            Strtot_Prod = null;
        }

        #endregion

        #region ========   RACK, 연, 단 셋팅  ============

        #region  1호기 C_ZONE RACK 셋팅 : PrepareLayoutNoScroll()
        /// <summary>
        /// 1호기 RACK 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll(string StrStkLocation, int varNum)
        {
            Grid gGrid = getGrid(varNum);

            gGrid.Children.Clear();
            // 행/열 전체 삭제
            if (gGrid.ColumnDefinitions.Count > 0)
            {
                gGrid.ColumnDefinitions.Clear();
            }
            if (gGrid.RowDefinitions.Count > 0)
            {
                gGrid.RowDefinitions.Clear();
            }
            //DataTable dtMaxPosition = MAX_POSITION(StrStkLocation, "A");
            //if (dtMaxPosition.Rows[0]["X_LOCATION"].ToString() == string.Empty || dtMaxPosition.Rows[0]["Y_LOCATION"].ToString() == string.Empty)
            //    return;
            //
            //// X축 값 설정
            //MAX_ROW_C1 = Convert.ToInt16(dtMaxPosition.Rows[0]["X_LOCATION"].ToString());
            //// Y축 값 설정
            //MAX_COL_C1 = Convert.ToInt16(dtMaxPosition.Rows[0]["Y_LOCATION"].ToString());

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(80);
                gGrid.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(100);
                gGrid.RowDefinitions.Add(rowdef);
            }
            // BOARDER 추가
            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < MAX_ROW; i++)
            {
                Border border = new Border();
                if (i == MAX_ROW - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                gGrid.Children.Add(border);
            }
            for (int i = 0; i < MAX_COL; i++)
            {
                Border border = new Border();
                if (i == MAX_COL - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                gGrid.Children.Add(border);
            }
        }
        #endregion

        #region  1호기 C_ZONE 연 셋팅 : PrepareLayoutNoScroll_Top()
        /// <summary>
        /// 1호기 연 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_Top(int varNum)
        {
            Grid gGrid = getGrid(varNum, "Top");

            gGrid.Children.Clear();
            // 행/열 전체 삭제
            if (gGrid.ColumnDefinitions.Count > 0)
            {
                gGrid.ColumnDefinitions.Clear();
            }
            if (gGrid.RowDefinitions.Count > 0)
            {
                gGrid.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(80);
                gGrid.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(15);
            gGrid.RowDefinitions.Add(rowdef);

            for (int i = 0; i < MAX_COL; i++)
            {
                var tbC_TopC01 = new TextBlock() { Text = i + 1 + ObjectDic.Instance.GetObjectName("연"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                //tbC_TopC01.SetValue(Grid.RowProperty, 3);
                tbC_TopC01.SetValue(Grid.ColumnProperty, i);
                tbC_TopC01.HorizontalAlignment = HorizontalAlignment.Center;
                gGrid.Children.Add(tbC_TopC01);

            }
        }
        #endregion

        #region  1호기 C_ZONE 단 셋팅 : PrepareLayoutNoScroll_LeftC1()
        /// <summary>
        /// 1호기 단 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_Left(int varNum)
        {
            Grid gGrid = getGrid(varNum, "Left");

            gGrid.Children.Clear();
            // 행/열 전체 삭제
            if (gGrid.ColumnDefinitions.Count > 0)
            {
                gGrid.ColumnDefinitions.Clear();
            }
            if (gGrid.RowDefinitions.Count > 0)
            {
                gGrid.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(29.5);
            gGrid.ColumnDefinitions.Add(coldef);

            // Row 값 정의
            RowDefinition rowdef = null;

            //for (int i = 0; i < MAX_ROW_C1; i++)
            //{
            //    rowdef = new RowDefinition();
            //    rowdef.Height = new GridLength(29.5);
            //    stair_LeftC01.RowDefinitions.Add(rowdef);
            //}
            for (int i = 0; i < MAX_ROW; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(100);
                gGrid.RowDefinitions.Add(rowdef);

                var tbC_LeftC01 = new TextBlock() { Text = MAX_ROW - i + ObjectDic.Instance.GetObjectName("단"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_LeftC01.SetValue(Grid.RowProperty, i);
                tbC_LeftC01.HorizontalAlignment = HorizontalAlignment.Center;
                gGrid.Children.Add(tbC_LeftC01);
            }
        }
        #endregion
        #endregion

        #region ======== LOT 정보, PORT 정보, 반송정보, 창고정보, 출고대상정보 BIND ==========
        //창고 숫자Groupby
        private void MainGridCnt(DataTable dt, int G_Num)
        {
            //line groupby 추출
            var list = dt.AsEnumerable().GroupBy(r => new
            {
                PRODGROUP = r.Field<string>("MCS_CST_ID")
            }).Select(g => g.First());

            DataTable dtLineList = list.CopyToDataTable();
            if (dtLineList == null)
            {
                return;
            }
            if (G_Num == 1) Inup_One = dtLineList.Rows.Count;
            if (G_Num != 1) InBot_One = dtLineList.Rows.Count;

        }

        #region 1호기 C_ZONE LOT 정보 Bind : DataBind_C1()
        /// <summary>
        /// 1호기 상단 정보 Bind
        /// </summary>
        private void DataBind(bool button, int varNum)
        {
            Grid gGrid = getGrid(varNum);

            loadingIndicator.Visibility = Visibility.Visible;
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("ZONE_ID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("X_PSTN", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQPTID"] = sW1ANPW101;
            dr["ZONE_ID"] = "A";
            dr["LANGID"] = LoginInfo.LANGID;
            dr["X_PSTN"] = varNum;
            dr["EQSGID"] = Strtot_Line == "" ? null : Strtot_Line;
            dr["PRODID"] = Strtot_Prod == "" ? null : Strtot_Prod;
            RQSTDT.Rows.Add(dr);

            //시간체크떄문에
            //Stopwatch sw = new Stopwatch();
            new ClientProxy().ExecuteService("DA_PRD_LOGIS_PLT_POSITION_PACK", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
            {
                try
                {
                    if (result.Rows.Count > 0)
                    {
                        MainGridCnt(result, varNum);
                    }
                    if (InBot_One != 0)
                    {
                        int One = Inup_One + InBot_One;
                        if (One == 0)
                        {
                            Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, "0");
                        }
                        else
                        {
                            Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, Util.NVC(One));
                        }
                    }
                    else
                    {
                        Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, Util.NVC(Inup_One));
                    }
                    Strtot_Line = null;
                    Strtot_Prod = null;
                    if (ex != null)
                    {
                        Util.SetTextBlockText_DataGridRowCount(txoneeqptcnt, "0");
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (gGrid.ColumnDefinitions.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            if (result.Rows[i]["DIV"].ToString().Equals("RACK"))
                            {
                                Rack_CheckBox assmRack = new Rack_CheckBox();
                                //assmRack.Name = result.Rows[i]["RACK_ID"].ToString();
                                assmRack.RackId = result.Rows[i]["RACK_ID"].ToString();                //RACK_ID
                                assmRack.Check = result.Rows[i]["DIV"].ToString();                     //RACK인지 PORT인지
                                assmRack.PalletID = result.Rows[i]["PALLETID"].ToString();             //PALLETID
                                assmRack.ProdID = result.Rows[i]["PRJT_NAME"].ToString();              //제품명->PJT 변경 21.01.19
                                assmRack.LineID = result.Rows[i]["EQSG_SHORT_NAME"].ToString();        //라인약어
                                assmRack.CSTID = result.Rows[i]["MCS_CST_ID"].ToString();              //구루마ID
                                assmRack.RackStateCode = result.Rows[i]["RACK_STAT_CODE"].ToString();  //상태
                                assmRack.Spcl_Flag = result.Rows[i]["ABNORMAL_YN"].ToString();         //비정상Flag
                                assmRack.ElapseDay = Convert.ToInt32(result.Rows[i]["STORE_DAY_GAP"]); //입고경과일
                                assmRack.Wip_Remarks = result.Rows[i]["WIP_REMARKS"].ToString();       //WIP REMAKRKS
                                assmRack.CAbbr_Code = result.Rows[i]["COT_ABBR_CODE"].ToString();      //전극설비정보
                                assmRack.PAbbr_Code = result.Rows[i]["PKG_ABBR_CODE"].ToString();      //조립설비정보
                                string gap_date = result.Rows[i]["GAP_DATE"].ToString();
                                DataRow[] comprodid = dtCommon.Select("CBO_CODE = '" + result.Rows[i]["PRODID"].ToString() + "'");
                                if (comprodid != null && comprodid.Count() > 0)
                                {
                                    limit_gap_date = comprodid[0]["ATTRIBUTE2"].ToString();
                                    if (!string.IsNullOrWhiteSpace(limit_gap_date)) // 변경 : 데이터 테이블에 제품ID 확인 후 값이 존재하면 limited_gap_date 값 설정 아래로직은 그대로?
                                    {
                                        assmRack.Gap_Date = string.IsNullOrWhiteSpace(gap_date) ? gap_date : (Convert.ToInt32(limit_gap_date) > Convert.ToInt32(gap_date)).ToString();
                                    }
                                }
                                assmRack.WipHold = result.Rows[i]["WIPHOLD"].ToString();//HOLD관리
                                assmRack.RETRAY_DISP = result.Rows[i]["RETRAY_DISP"].ToString();//Retray여부

                                assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                                assmRack.Checked += assmRack_Checked; //체크박스 체크
                                Grid.SetRow(assmRack, MAX_ROW - Convert.ToInt16(result.Rows[i]["X_LOCATION"].ToString()));
                                Grid.SetColumn(assmRack, Convert.ToInt16(result.Rows[i]["Y_LOCATION"].ToString()) - 1);

                                gGrid.Children.Add(assmRack);
                            }
                        }

                        if (button == false)
                        {
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                catch (Exception)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                }
            });
        }
        #endregion

        #region 창고 정보 BIND : WareHouseBind()
        /// <summary>
        /// 창고정보 Bind
        /// </summary>
        private void WareHouseBind()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("EQSGID", typeof(string));
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            if (cboEqsgid.SelectedValue == null)
            {
                dr["EQSGID"] = null;
            }
            else
            {
                dr["EQSGID"] = cboEqsgid.SelectedValue.ToString() == "ALL" ? null : cboEqsgid.SelectedValue.ToString();
            }
            if (cboWhInfo.SelectedValue == null)
            {
                dr["PRODID"] = null;
            }
            else
            {
                dr["PRODID"] = cboWhInfo.SelectedValue.ToString() == "ALL" ? null : cboWhInfo.SelectedValue.ToString();
            }
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);
            new ClientProxy().ExecuteService("DA_PRD_LOGIS_SEL_WAREHOUSE_INFO_STK", "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                if (result == null || result.Rows.Count <= 0)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    // STOCKER 적재율 데이터와, 제품별 가용/비가용 CST 숫자 데이터 분리
                    if (!(result.AsEnumerable().Where(x => x.Field<string>("CODE").Equals("Y")).Count().Equals(0)))
                    {
                        DataTable dtWareHouseStock = result.AsEnumerable().Where(x => x.Field<string>("CODE").Equals("Y")).CopyToDataTable();
                        Util.GridSetData(dgWareHouseStock, dtWareHouseStock, FrameOperation, true);
                    }
                    if (!(result.AsEnumerable().Where(x => x.Field<string>("CODE").Equals("N")).Count().Equals(0)))
                    {
                        DataTable dtRackLoadRate = result.AsEnumerable().Where(x => x.Field<string>("CODE").Equals("N")).CopyToDataTable();
                        Util.GridSetData(dgWareHouse, dtRackLoadRate, FrameOperation, true);
                    }
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region 입고/출고 예약 정보 Bind : OutputBind()
        /// <summary>
        /// 입고/출고 예약 정보 Bind
        /// </summary>
        private void OutputBind()
        {
        }

        private void SelectOutRackList(bool IsBinding, string cstStat, string cstProd, string prodId)
        {
            #region 분기 필요한 변수 초기화 
            // True : [공통코드 - DIFFUSION_SITE - AUTO_LOGIS - ATTR1]에 등록된 Shop 만 Ex) MI ST 
            // False : 그외 Ex) WA
            string bizRuleName = (_DiffusionSiteFlag) ? "DA_SEL_MCS_MANUAL_ORD_INFO_FOR_PACK_RTD" : "DA_SEL_MCS_MANUAL_ORD_INFO_FOR_PACK_MES";
            ClientProxy ClientProxy = (_DiffusionSiteFlag) ? new ClientProxy() : new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex);
            #endregion
            DataTable inDataTable = new DataTable("RQSTDT");

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("BLDG_CODE", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            if (LoginInfo.CFG_AREA_ID.Equals("P8"))
            {
                dr["BLDG_CODE"] = "WP2";
            }
            else if (LoginInfo.CFG_AREA_ID.Equals("PA"))
            {
                dr["BLDG_CODE"] = "WP3";
            }
            else
            {
                dr["BLDG_CODE"] = LoginInfo.CFG_AREA_ID;
            }

            inDataTable.Rows.Add(dr);

            _requestOutRackInfoTable = ClientProxy.ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (IsBinding) Util.GridSetData(dgOutputInfo, _requestOutRackInfoTable, FrameOperation, true);
        }
        #endregion
        #endregion

        #region  Carrier ID를 조회조건으로 Rack 조회 : DataBind_S()
        /// <summary>
        /// Carrier ID를 조회조건으로 Rack 조회
        /// </summary>
        private DataTable DataBind_S(string LotID)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("ZONE_ID", typeof(string));
            RQSTDT.Columns.Add("PLTID", typeof(string));
            RQSTDT.Columns.Add("X_PSTN", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["PLTID"] = LotID;
            dr["ZONE_ID"] = "A";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_LOGIS_PLT_POSITION_PACK", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult.Rows.Count > 0)
            {

                if (dtResult.Rows[0]["PALLETID"] != null)
                {
                    dtResult.Rows[0]["PALLETID"] = dtResult.Rows[0]["PALLETID"].ToString();
                }
            }
            return dtResult;
        }
        #endregion

        #region LOT을 조회조건으로 RACK의 체크박스 선택 : SelectLOT()
        /// <summary>
        /// Carrier ID를 조회조건으로 재조회
        /// </summary>
        private void SelectLOT()
        {
            if (txtLotS.Text != string.Empty)
            {
                //LOT으로 Rack정보 및 LOT정보 조회
                DataTable dtRackLot = DataBind_S(txtLotS.Text);
                #region Rack 정보
                //Rack 정보
                if (dtRackLot.Rows.Count > 0)
                {
                    for (int i = 1; i <= MAX_VAR; i++)
                    {
                        foreach (Rack_CheckBox control in Util.FindVisualChildren<Rack_CheckBox>(getGrid(i)))
                        {
                            if (control != null)
                            {
                                if (control.RackId.ToString() == dtRackLot.Rows[0]["RACK_ID"].ToString())
                                {
                                    //Rack에 체크박스 체크
                                    if (control.IsChecked == true) return;

                                    control.IsChecked = true;
                                    OutPutLotBind(control);
                                    //확장
                                    Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                    Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                    Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                    Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                    Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
                                }
                            }
                        }
                    }

                    txtLotS.Text = string.Empty;
                    txtLotS.Focus();
                }
                else
                {
                    Util.MessageInfo("SFU3394");
                    txtLotS.Text = string.Empty;
                    txtLotS.Focus();
                }
                #endregion
            }
        }
        #endregion

        #region LOT을 조회조건으로 출고대상 Pacncake 스프레드 Bind  : OutPutLotBind()
        /// <summary>
        /// PALLET으로 조회시 PALLET 출고대상 스프레드
        /// </summary>
        private void OutPutLotBind(Rack_CheckBox Control)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PLTID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PLTID"] = Control.PalletID.ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PACK_LOGIS_STK_INPUTLOT", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우
                        {

                            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "RACK_ID").ToString() == Control.RackId)
                                {
                                    return;
                                }
                            }
                            DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                            DataRow newRow = null;
                            newRow = dtSource.NewRow();
                            #region Data Bind
                            newRow["RACK_ID"] = result.Rows[0]["RACK_ID_2"];
                            newRow["DIV"] = result.Rows[0]["DIV"];
                            newRow["CHK"] = result.Rows[0]["CHK"];
                            newRow["RACK_ID"] = result.Rows[0]["RACK_ID"];
                            newRow["RACK_NAME"] = result.Rows[0]["RACK_NAME"];
                            newRow["EQPTID"] = result.Rows[0]["EQPTID"];
                            newRow["EQPTNAME"] = result.Rows[0]["EQPTNAME"];
                            newRow["X_PSTN"] = result.Rows[0]["X_PSTN"];
                            newRow["Y_PSTN"] = result.Rows[0]["Y_PSTN"];
                            newRow["Z_PSTN"] = result.Rows[0]["Z_PSTN"];
                            newRow["ZONE_ID"] = result.Rows[0]["ZONE_ID"];
                            newRow["PRODID"] = result.Rows[0]["PRODID"];
                            newRow["PRJT_NAME"] = result.Rows[0]["PRJT_NAME"];
                            newRow["MODLID"] = result.Rows[0]["MODLID"];
                            newRow["CSTID"] = result.Rows[0]["CSTID"];
                            newRow["CSTPROD"] = result.Rows[0]["CSTPROD"];
                            newRow["CSTSTAT"] = result.Rows[0]["CSTSTAT"];
                            newRow["CSTPROD_NAME"] = result.Rows[0]["CSTPROD_NAME"];
                            newRow["CSTSTAT_NAME"] = result.Rows[0]["CSTSTAT_NAME"];
                            newRow["PLTID"] = result.Rows[0]["PLTID"];
                            newRow["LOTCNT"] = result.Rows[0]["LOTCNT"];
                            newRow["WIPHOLD"] = result.Rows[0]["WIPHOLD"];
                            newRow["RACK_STAT_CODE"] = result.Rows[0]["RACK_STAT_CODE"];
                            newRow["EQSGID"] = result.Rows[0]["EQSGID"];
                            newRow["EQSGNAME"] = result.Rows[0]["EQSGNAME"];
                            newRow["STORE_DAY_GAP"] = result.Rows[0]["STORE_DAY_GAP"];
                            newRow["BLDG_CODE"] = result.Rows[0]["BLDG_CODE"];
                            newRow["BLDG_NAME"] = result.Rows[0]["BLDG_NAME"];
                            newRow["COTSHORTNAME"] = result.Rows[0]["COTSHORTNAME"];
                            newRow["PKGSHORTNAME"] = result.Rows[0]["PKGSHORTNAME"];
                            #endregion //Data Bind
                            dtSource.Rows.Add(newRow);

                            Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);
                        }
                        else
                        {
                            DataTable drt = new DataTable();
                            #region 컬럼선언
                            drt.Columns.Add("RACK_ID_2", typeof(string));
                            drt.Columns.Add("DIV", typeof(string));
                            drt.Columns.Add("CHK", typeof(string));
                            drt.Columns.Add("RACK_ID", typeof(string));
                            drt.Columns.Add("RACK_NAME", typeof(string));
                            drt.Columns.Add("EQPTID", typeof(string));
                            drt.Columns.Add("EQPTNAME", typeof(string));
                            drt.Columns.Add("X_PSTN", typeof(string));
                            drt.Columns.Add("Y_PSTN", typeof(string));
                            drt.Columns.Add("Z_PSTN", typeof(string));
                            drt.Columns.Add("ZONE_ID", typeof(string));
                            drt.Columns.Add("PRODID", typeof(string));
                            drt.Columns.Add("PRJT_NAME", typeof(string));
                            drt.Columns.Add("MODLID", typeof(string));
                            drt.Columns.Add("CSTID", typeof(string));
                            drt.Columns.Add("CSTPROD", typeof(string));
                            drt.Columns.Add("CSTSTAT", typeof(string));
                            drt.Columns.Add("CSTPROD_NAME", typeof(string));
                            drt.Columns.Add("CSTSTAT_NAME", typeof(string));
                            drt.Columns.Add("PLTID", typeof(string));
                            drt.Columns.Add("LOTCNT", typeof(string));
                            drt.Columns.Add("WIPHOLD", typeof(string));
                            drt.Columns.Add("RACK_STAT_CODE", typeof(string));
                            drt.Columns.Add("EQSGID", typeof(string));
                            drt.Columns.Add("EQSGNAME", typeof(string));
                            drt.Columns.Add("STORE_DAY_GAP", typeof(string));
                            drt.Columns.Add("BLDG_CODE", typeof(string));
                            drt.Columns.Add("BLDG_NAME", typeof(string));
                            drt.Columns.Add("COTSHORTNAME", typeof(string));
                            drt.Columns.Add("PKGSHORTNAME", typeof(string));
                            #endregion //컬럼선언

                            DataRow newRow = null;
                            newRow = drt.NewRow();
                            #region DataBind
                            newRow["RACK_ID"] = result.Rows[0]["RACK_ID_2"];
                            newRow["DIV"] = result.Rows[0]["DIV"];
                            newRow["CHK"] = result.Rows[0]["CHK"];
                            newRow["RACK_ID"] = result.Rows[0]["RACK_ID"];
                            newRow["RACK_NAME"] = result.Rows[0]["RACK_NAME"];
                            newRow["EQPTID"] = result.Rows[0]["EQPTID"];
                            newRow["EQPTNAME"] = result.Rows[0]["EQPTNAME"];
                            newRow["X_PSTN"] = result.Rows[0]["X_PSTN"];
                            newRow["Y_PSTN"] = result.Rows[0]["Y_PSTN"];
                            newRow["Z_PSTN"] = result.Rows[0]["Z_PSTN"];
                            newRow["ZONE_ID"] = result.Rows[0]["ZONE_ID"];
                            newRow["PRODID"] = result.Rows[0]["PRODID"];
                            newRow["PRJT_NAME"] = result.Rows[0]["PRJT_NAME"];
                            newRow["MODLID"] = result.Rows[0]["MODLID"];
                            newRow["CSTID"] = result.Rows[0]["CSTID"];
                            newRow["CSTPROD"] = result.Rows[0]["CSTPROD"];
                            newRow["CSTSTAT"] = result.Rows[0]["CSTSTAT"];
                            newRow["CSTPROD_NAME"] = result.Rows[0]["CSTPROD_NAME"];
                            newRow["CSTSTAT_NAME"] = result.Rows[0]["CSTSTAT_NAME"];
                            newRow["PLTID"] = result.Rows[0]["PLTID"];
                            newRow["LOTCNT"] = result.Rows[0]["LOTCNT"];
                            newRow["WIPHOLD"] = result.Rows[0]["WIPHOLD"];
                            newRow["RACK_STAT_CODE"] = result.Rows[0]["RACK_STAT_CODE"];
                            newRow["EQSGID"] = result.Rows[0]["EQSGID"];
                            newRow["EQSGNAME"] = result.Rows[0]["EQSGNAME"];
                            newRow["STORE_DAY_GAP"] = result.Rows[0]["STORE_DAY_GAP"];
                            newRow["BLDG_CODE"] = result.Rows[0]["BLDG_CODE"];
                            newRow["BLDG_NAME"] = result.Rows[0]["BLDG_NAME"];
                            newRow["COTSHORTNAME"] = result.Rows[0]["COTSHORTNAME"];
                            newRow["PKGSHORTNAME"] = result.Rows[0]["PKGSHORTNAME"];
                            #endregion //DataBind
                            drt.Rows.Add(newRow);
                            Util.GridSetData(dgLotInfo, drt, FrameOperation, false);
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 수동출고예약 Valldation :  ValidationReturn()
        /// <summary>
        /// 수동출고예약 Valldation
        /// </summary>
        /// <returns></returns>
        private bool ValidationReturn()
        {
            if (dgLotInfo.Rows.Count == 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고할 RACK"));
                return false;
            }

            if (cboOutPort.SelectedIndex <= 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고포트"));
                return false;

            }
            int checkcount = 0;
            foreach (DataRow row in ((System.Data.DataView)dgLotInfo.ItemsSource).Table.Rows)
            {
                if (row["CHK"].ToString() == "True")
                { checkcount++; }
            }

            if (checkcount < 1)
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고대상 Carrier정보"));
                return false;
            }

            return true;
        }
        #endregion

        #region 타이머로 재조회시 기존 선택된 LOT 정보 갱신해서 출고대상 Pancake 스프레드 Bind : ReCheckLotInf()
        /// <summary>
        /// 타이머로 재조회시 선택된 LOT 정보 조회
        /// </summary>
        private void ReCheckLotInf(string _lotid)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _lotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "RQSTDT", "OUTDATA", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우
                        {

                            try
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                                DataRow newRow = null;
                                for (int i = 0; i < result.Rows.Count; i++)
                                {
                                    newRow = dtSource.NewRow();
                                    #region DataBind
                                    newRow["RACK_ID_2"] = result.Rows[i]["RACK_ID_2"];
                                    newRow["CHK"] = 0;
                                    newRow["RACK_ID"] = result.Rows[i]["RACK_ID"];
                                    newRow["PRJT_NAME"] = result.Rows[i]["PRJT_NAME"];
                                    newRow["PRODID"] = result.Rows[i]["PRODID"];
                                    newRow["WH_RCV_DTTM"] = result.Rows[i]["WH_RCV_DTTM"];
                                    newRow["WIPDTTM_ED"] = result.Rows[i]["WIPDTTM_ED"];
                                    newRow["LOTID"] = result.Rows[i]["LOTID"];
                                    newRow["WIP_QTY"] = result.Rows[i]["WIP_QTY"];
                                    newRow["VLD_DATE"] = result.Rows[i]["VLD_DATE"];
                                    newRow["SPCL_FLAG"] = result.Rows[i]["SPCL_FLAG"];
                                    newRow["TMP1"] = result.Rows[i]["TMP1"];
                                    newRow["SPCL_RSNCODE"] = result.Rows[i]["SPCL_RSNCODE"];
                                    newRow["TMP2"] = result.Rows[i]["TMP2"];
                                    newRow["WIP_REMARKS"] = result.Rows[i]["WIP_REMARKS"];
                                    newRow["TMP3"] = result.Rows[i]["TMP3"];
                                    newRow["WIPHOLD"] = result.Rows[i]["WIPHOLD"];
                                    newRow["RACK_STAT_CODE"] = result.Rows[i]["RACK_STAT_CODE"];
                                    newRow["EQPTID"] = result.Rows[i]["EQPTID"];
                                    newRow["EQPTNAME"] = result.Rows[i]["EQPTNAME"];
                                    newRow["X_PSTN"] = result.Rows[i]["X_PSTN"];
                                    newRow["Y_PSTN"] = result.Rows[i]["Y_PSTN"];
                                    newRow["Z_PSTN"] = result.Rows[i]["Z_PSTN"];
                                    newRow["ID_SEQ"] = result.Rows[i]["ID_SEQ"];
                                    newRow["WOID"] = result.Rows[i]["WOID"];
                                    newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                                    newRow["MODLID"] = result.Rows[i]["MODLID"];
                                    newRow["MCS_CST_ID"] = result.Rows[i]["MCS_CST_ID"];
                                    newRow["PRODDESC"] = result.Rows[i]["PRODDESC"];
                                    newRow["SAVE_YN"] = result.Rows[i]["SAVE_YN"];
                                    newRow["EQSGID"] = result.Rows[i]["EQSGID"];
                                    newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                                    newRow["TMP4"] = result.Rows[i]["TMP4"];
                                    newRow["TMP5"] = result.Rows[i]["TMP5"];
                                    newRow["TMP6"] = result.Rows[i]["TMP6"];
                                    newRow["TMP7"] = result.Rows[i]["TMP7"];
                                    newRow["TMP8"] = result.Rows[i]["TMP8"];
                                    newRow["TMP9"] = result.Rows[i]["TMP9"];
                                    #endregion //DataBind
                                    dtSource.Rows.Add(newRow);
                                }
                                Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);

                            }
                            catch (Exception ex1)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex1), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            Util.GridSetData(dgLotInfo, result, FrameOperation, false);
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Rack 하단 PORT 배경색 설정 : ButtonPort_Color()
        /// <summary>
        /// Rack 하단 PORT 배경색 설정
        /// </summary>
        private void ButtonPort_Color(Button Button, string remark)
        {
            if (remark == "N")
            {
                Button.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF558ED5"));
            }
            else
            {
                Button.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC3D69B"));
                this.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
        #endregion
        #endregion

        private void popupChangeLine_Closed(object sender, EventArgs e)
        {
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK003_005_RESERVATION popup = new PACK003_005_RESERVATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgWareHouseStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = this.dgWareHouseStock.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "TOT_CELL_QTY" || cell.Column.Name == "AVA_CELL_QTY" || cell.Column.Name == "HOLD_CELL_QTY")
                    {
                        string sHold = string.Empty;
                        string sProd = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[cell.Row.Index].DataItem, "PRODID"));
                        string eqsgID = Util.NVC(DataTableConverter.GetValue(dgWareHouseStock.Rows[cell.Row.Index].DataItem, "EQSGID"));
                        switch (cell.Column.Name)
                        {
                            case "TOT_CELL_QTY":
                                sHold = "";
                                break;
                            case "AVA_CELL_QTY":
                                sHold = "N";
                                break;
                            case "HOLD_CELL_QTY":
                                sHold = "Y";
                                break;
                        }
                        PACK003_005_INPUT_INFO popupInputLot = new PACK003_005_INPUT_INFO();
                        popupInputLot.FrameOperation = this.FrameOperation;
                        object[] parameters = new object[3];
                        parameters[0] = sProd;
                        parameters[1] = sHold;
                        parameters[2] = eqsgID;
                        C1WindowExtension.SetParameters(popupInputLot, parameters);
                        popupInputLot.Closed += new EventHandler(popupInputLot_Closed);
                        popupInputLot.Show();
                        popupInputLot.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void dgWareHouseStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgWareHouseStock.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("TOT_CELL_QTY") || e.Cell.Column.Name.Equals("AVA_CELL_QTY") || e.Cell.Column.Name.Equals("HOLD_CELL_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }

            }));
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
        }

        private void txtCycle_ValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<double> e)
        {
            C1NumericBox newmericBox = (C1NumericBox)sender;
            if (newmericBox == null)
            {
                return;
            }

            this.timerSetting(newmericBox);
        }

        private void btn_OutputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region 분기 필요한 변수 초기화 
                // True : [공통코드 - DIFFUSION_SITE - AUTO_LOGIS - ATTR1]에 등록된 Shop 만 Ex) MI ST 
                // False : 그외 Ex) WA
                string bizRuleName = (_DiffusionSiteFlag) ? "BR_MHS_REG_CANCEL_CMD" : "BR_PRD_REG_TRF_JOB_CANCEL_BY_MES";
                string outData = (_DiffusionSiteFlag) ? "" : "OUT_DATA";
                ClientProxy ClientProxy = (_DiffusionSiteFlag) ? new ClientProxy() : new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex);
                #endregion


                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_CANCEL_INFO");
                inTable.Columns.Add("ORDID", typeof(string));
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOutputInfo.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        if ((DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() != "JOB_FAIL") && !_DiffusionSiteFlag)
                        {
                            Util.MessageInfo("SFU8337"); //Order 상태가 JOB_FAIL인것만 취소 가능합니다.
                            return;
                        }
                        if ((DataTableConverter.GetValue(row.DataItem, "ORD_STAT").GetString() == "MOVING") && _DiffusionSiteFlag) //MHS BIZ 호출시 ORD_STAT가 MOVING이면 취소 불가 
                        {
                            Util.MessageInfo("SFU5183", "MOVING"); //[MOVING] 상태에서는 요청 취소가 불가능합니다.
                            return;
                        }
                        DataRow newRow = inTable.NewRow();
                        newRow["ORDID"] = DataTableConverter.GetValue(row.DataItem, "ORDID").GetString();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }
                ClientProxy.ExecuteService(bizRuleName, "IN_CANCEL_INFO", outData, inTable, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1930"); //출고 취소 완료
                        Refresh(true);

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

        private void chkCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            CheckBox cb = sender as CheckBox;

            if (cb.DataContext == null)
                return;
            if (dgOutputInfo.CurrentCell.Column.Name == "CHK")
            {
                if (dgOutputInfo.CurrentCell.IsSelectable.ToString() == "True")
                {
                    btn_OutputCancel.Visibility = Visibility.Visible;
                }
            }
        }

        private void cboStkid_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(sW1ANPW101))
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Clear();

                sW1ANPW101 = cboStkid.SelectedValue.ToString();
                txtEqptID_1.Text = cboStkid.Text;
                GetintoAreaEqpt_Change(cboStkid.SelectedValue.ToString());
                GetintoAreaOutPort(cboStkid.SelectedValue.ToString());
                Refresh(true);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        //공통코드에서 편차(GAP 일수) 시간 가져옴
        private void Limit_Gap_Time()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_CELL_STK_INTERLOCK";

                RQSTDT.Rows.Add(dr);

                dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }

        private void setGrid(int MAX_VAR, bool bButton) //그리드 초기화및 재구성 메소드 
        {
            for (int i = 1; i <= 4; i++)
            {
                Grid gGrid = RackGrid.FindChild<Grid>($"grid_{i:D2}");
                gGrid.Visibility = (i <= MAX_VAR) ? Visibility.Visible : Visibility.Collapsed;

                if (i <= MAX_VAR)
                {
                    PrepareLayoutNoScroll(sW1ANPW101, i);
                    PrepareLayoutNoScroll_Top(i);
                    PrepareLayoutNoScroll_Left(i);
                    DataBind(bButton, i);
                }
            }

        }

        private Grid getGrid(int varNum, string type = "")  //XAML상 그리드 객체 받아오는  메소드
        {
            Grid gGrid = RackGrid.FindChild<Grid>($"grid_{varNum:D2}").FindChild<Grid>($"stair_{type}{varNum:D2}");
            return gGrid;
        }

        private int GetVarCount(string EQPTID) //전달 받은 STK의 X_PSTN 개수 조회 메소드
        {
            try
            {
                int iVarCount = 0;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = EQPTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RACK_MAX_XYZ", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    iVarCount = int.Parse(dtResult.Rows[0]["X_ROW"].GetString());
                else
                    return 0;

                return iVarCount;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 0;
        }
    }

}
