/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : Gripper 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2022.01.24 강동희 : 불량등급 리스트에 전체선택 기능 추가
  2022.03.08 강동희 : 설비리스트출력 조건 수정
  2022.05.17 이정미 : 충방전기 Gripper 관리 화면에서만 열연단 조건을 확인하도록 변경, 
                      JIG BOX별 작업수량 조회 추가  
  2022.09.01  김령호 : NB1동 VOC Formation issue 62번 요구사항 적용 (조회 결과값 정상적으로 보이지 않음)
  2022.09.08  김령호 : NB1동 VOC Formation issue 62번 추가 요구사항 적용 
                          1. 불량 수량 합계가 디스플레이
                          2. 불량 수량 1 -> Blue / 2이상 -> Red
                          3. 불량 PIN NO Double 클릭 시, 해당 불량 내역 조회
  2022.11.14  조영대 : 불량수량 관련 수정
  2022 12.23  이정미 : 불량 수량 수정 
  2023.10.12  주훈   : _sProcKind (Process Group Code) 추가
                       불량 위치 정보 수정(Biz) (불량 위치 -> 불량 위치-불량 수량)
                       채널 조회-불량 범레에 불량 코드 추가(Xml)
                        LCI, IR-OCV PIIN 관리 추가 (EqpKind : L, I)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_029 : UserControl, IWorkArea
    {
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        private string _sEqpKind = string.Empty;
        private string _sLaneX = string.Empty;
        private string _sSelType = string.Empty;

        DataTable _dt = new DataTable();
        DataTable _dtc = new DataTable();
        DataTable _dtch = new DataTable();
        DataTable dtTempRslt = new DataTable();

        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;

        // 2023.10.12 추가
        // Process Group Code
        private string _sProcKind = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_029()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sEqpKind))
                _sEqpKind = GetLaneIDForMenu(LoginInfo.CFG_MENUID);

            // 2023.10.12 : 설비 그룹과 공정 그룹간의 연관 정보가 필요함
            _sProcKind = _sEqpKind;

            switch (_sEqpKind)
            {
                case "1": // 충방전기                    
                case "L": // LCI : 2023.10.12 추가
                    // 2023.10.12 : LCI의 공정 그룹이 Formation으로 등록되어 있음
                    if (_sEqpKind.Equals("L")) _sProcKind = "1";

                    //tbBoxPos1.Visibility = Visibility.Visible;
                    tbBoxPos2.Visibility = Visibility.Visible;
                    spBoxPos.Visibility = Visibility.Visible;
                    chkAllMap.Visibility = Visibility.Visible;

                    spGripper1.Visibility = Visibility.Visible;
                    spGripper2.Visibility = Visibility.Visible;
                    chkAllGripper.Visibility = Visibility.Visible;

                    cboEqpMap.Visibility = Visibility.Collapsed;
                    tblEqpMap.Visibility = Visibility.Collapsed;

                    tblEqpt.Visibility = Visibility.Collapsed;
                    cboEqpGripper.Visibility = Visibility.Collapsed;
                    break;

                case "8": // OCV
                case "I": // IROCV : 2023.10.12 추가
                    //tbBoxPos1.Visibility = Visibility.Hidden;
                    tbBoxPos2.Visibility = Visibility.Collapsed;
                    spBoxPos.Visibility = Visibility.Collapsed;
                    chkAllMap.Visibility = Visibility.Collapsed;

                    spGripper1.Visibility = Visibility.Collapsed;
                    spGripper2.Visibility = Visibility.Collapsed;
                    chkAllGripper.Visibility = Visibility.Collapsed;

                    _sLaneX = "ALL";
                    break;

                case "J": //JIG
                    //tbBoxPos1.Visibility = Visibility.Hidden;
                    tbBoxPos2.Visibility = Visibility.Collapsed;
                    spBoxPos.Visibility = Visibility.Collapsed;
                    chkAllMap.Visibility = Visibility.Collapsed;

                    spGripper1.Visibility = Visibility.Collapsed;
                    spGripper2.Visibility = Visibility.Collapsed;
                    chkAllGripper.Visibility = Visibility.Collapsed;

                    TabRepairInfo.Visibility = Visibility.Collapsed;

                    _sLaneX = "ALL";

                    break;
            }

            //dtpFromDateMap.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            //dtpRepairFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);



            InitCombo();
            InitControl();
            InitTime();
            InitLegendMap();
            InitLegendChDfct();
            InitLegendCh();
            this.Loaded -= UserControl_Loaded;
        }

        private string GetLaneIDForMenu(string sMenuID)
        {
            string sLaneID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_CHARGE_MENU_ID";
                dr["CMCODE"] = sMenuID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sLaneID;
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

        private void InitCombo()
        {
            #region [설비 Map 조회 Tabpage]

            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", _sEqpKind };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterEqpType);

            C1ComboBox[] cboLaneMapChild = { cboRowMap, cboColMap, cboStgMap, cboEqpMap };

            // 2023.10.12 추가
            // _sEqpKind : LCI(L), IRODV(I)
            //if (_sEqpKind.Equals("8"))
            if (_sEqpKind.Equals("8") || _sEqpKind.Equals("L") || _sEqpKind.Equals("I"))
            {
                string[] sFilterLane = { _sEqpKind };
                _combo.SetCombo(cboLaneMap, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilterLane, cbChild: cboLaneMapChild);
            }
            else
            {
                string[] sFilterLane = { _sLaneX };
                _combo.SetCombo(cboLaneMap, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE", sFilter: sFilterLane, cbChild: cboLaneMapChild);
            }

            C1ComboBox[] cboRouteChild = { cboOpMap };
            string[] sRouteMap = { null, null, null, null, null };
            _combo.SetCombo(cboRouteMap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", sFilter: sRouteMap, cbChild: cboRouteChild);

            // 2023.10.12 추가
            // _sEqpKind : LCI(L)
            if (_sEqpKind.Equals("1"))
            {
                C1ComboBox[] cboOperMapParent = { cboRouteMap };
                // 2023.10.12 수정
                // _sEqpKind -> _sProcKind
                //string[] sFilterOperMap = { _sEqpKind, null, "11,12" };
                string[] sFilterOperMap = { _sProcKind, null, "11,12,13" };    //  (PROC_GR_CODE= Formation), (Charge, Discharge, OCV)
                _combo.SetCombo(cboOpMap, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperMapParent, sFilter: sFilterOperMap);
            }
            else if (_sEqpKind.Equals("L"))
            {
                C1ComboBox[] cboOperMapParent = { cboRouteMap };
                string[] sFilterOperMap = { _sProcKind, null, "1A,1C" };    //  (PROC_GR_CODE= Formation), (LowCurrent Inspection, PreCharge)
                _combo.SetCombo(cboOpMap, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperMapParent, sFilter: sFilterOperMap);
            }
            else
            {
                C1ComboBox[] cboOperMapParent = { cboRouteMap };
                // 2023.10.12 수정
                // _sEqpKind -> _sProcKind
                //string[] sFilterOperMap = { _sEqpKind, null, null };
                string[] sFilterOperMap = { _sProcKind, null, null };
                _combo.SetCombo(cboOpMap, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperMapParent, sFilter: sFilterOperMap);
            }


            C1ComboBox[] cboRowMapParent = { cboEqpKind, cboLaneMap };
            _combo.SetCombo(cboRowMap, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", cbParent: cboRowMapParent);
            _combo.SetCombo(cboColMap, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COL", cbParent: cboRowMapParent);
            _combo.SetCombo(cboStgMap, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "STG", cbParent: cboRowMapParent);

            C1ComboBox[] cboRowMapParent1 = { cboLaneMap, cboEqpKind }; //20220308_설비리스트출력 조건 수정
            if (_sEqpKind.Equals("J"))
            {
                _combo.SetCombo(cboEqpMap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEJIG", cbParent: cboRowMapParent1);
            }
            else
            {
                string[] sFilterLevel = { null, "M,C" };
                _combo.SetCombo(cboEqpMap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboRowMapParent1, sFilter: sFilterLevel);
            }

            #endregion

            #region [Gripper 수리정보 Tabpage]

            C1ComboBox[] cboLaneGripperChild = { cboRowGripper, cboColGripper, cboStgGripper, cboEqpGripper };


            // 2023.10.12 추가
            // _sEqpKind : LCI(L), IRODV(I)
            //if (_sEqpKind.Equals("8"))
            if (_sEqpKind.Equals("8") || _sEqpKind.Equals("L") || _sEqpKind.Equals("I"))
            {
                string[] sFilterLane = { _sEqpKind };
                _combo.SetCombo(cboLaneGripper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE_MB", cbChild: cboLaneGripperChild, sFilter: sFilterLane);
            }
            else
            {
                string[] sFilterLane = { _sLaneX };
                _combo.SetCombo(cboLaneGripper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneGripperChild, sFilter: sFilterLane);
            }


            C1ComboBox[] cboParentGripper = { cboEqpKind, cboLaneGripper };
            _combo.SetCombo(cboRowGripper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", cbParent: cboParentGripper);
            _combo.SetCombo(cboColGripper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COL", cbParent: cboParentGripper);
            _combo.SetCombo(cboStgGripper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "STG", cbParent: cboParentGripper);

            if (cboRowGripper.Items.Count == 0)
                cboRowGripper.Visibility = Visibility.Collapsed;

            C1ComboBox[] cboEqpGripperParent = { cboLaneGripper, cboEqpKind };

            if (_sEqpKind.Equals("J"))
            {
                _combo.SetCombo(cboEqpGripper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEJIG", cbParent: cboEqpGripperParent);
            }
            else
            {
                string[] sFilterLevel = { null, "M,C" };
                _combo.SetCombo(cboEqpGripper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboEqpGripperParent, sFilter: sFilterLevel);
            }
            #endregion
        }

        private void InitLegendChDfct()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COM_TYPE_CODE"] = "FORM_CH_DFCT_CODE";
                RQSTDT.Rows.Add(dr);

                _dtch = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgChDfctColor, _dtch, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void InitLegendMap()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_EQP_MAP_LEGEND";
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMapColor, dtResult, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void InitLegendCh()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_CH_LEGEND";
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgChColor, dtResult, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void InitTime()
        {

            SetWorkResetTime();

            // Util 에 해당 함수 추가 필요.
            dtpFromDateMap.SelectedDateTime = GetJobDateFrom();
            dtpRepairFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDateMap.SelectedDateTime = GetJobDateTo();
            dtpRepairToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }

        private void InitControl()
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
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOTJUDGE_CBO", "RQSTDT", "RSLTDT", RQSTDT, menuid: LoginInfo.CFG_MENUID);

                foreach (DataRow drResult in dtResult.Rows)
                {
                    CheckBox cbChk = new CheckBox();
                    cbChk.Tag = drResult["CBO_CODE"];
                    cbChk.Content = drResult["CBO_NAME"];
                    lbDefectGrade.Items.Add(cbChk);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListMap();
        }

        private void GetListMap()
        {
            try
            {
                dgEqpMap.ItemsSource = null;

                int iColCount = dgEqpMap.Columns.Count;
                for (int i = iColCount - 1; i > -1; i--)
                {
                    if (!dgEqpMap.Columns[i].Name.Equals("EQPTID") && !dgEqpMap.Columns[i].Name.Equals("EQPTNAME") && !dgEqpMap.Columns[i].Name.Equals("CST_CELL_TYPE_CODE"))
                    {
                        dgEqpMap.Columns.RemoveAt(i);
                    }
                }

                //SetHeader();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));
                dtRqst.Columns.Add("GRADE", typeof(string));
                dtRqst.Columns.Add("ROW", typeof(string));
                dtRqst.Columns.Add("COL", typeof(string));
                dtRqst.Columns.Add("STG", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = Util.GetCondition(cboLaneMap, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRouteMap, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOpMap, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                if (string.IsNullOrEmpty(dr["PROCID"].ToString())) return;


                dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmmss");
                dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmmss");

                //dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.AddHours(dtpFromTime.DateTime.Value.Hour).AddMinutes(dtpFromTime.DateTime.Value.Minute).ToString("yyyy-MM-dd HH:mm:00.000");
                //dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.AddHours(dtpToTime.DateTime.Value.Hour).AddMinutes(dtpToTime.DateTime.Value.Minute).AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:59.997");

                if (!string.IsNullOrEmpty(GetCheckdGrade()))
                    dr["GRADE_CD"] = GetCheckdGrade();

                dr["EQP_KIND_CD"] = _sEqpKind;
                if (!chkAllMap.IsChecked == true)
                {
                    // 2023.10.12 추가
                    // _sEqpKind : LCI(L)
                    //dr["ROW"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboRowMap, bAllNull: true) : null;
                    //dr["COL"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboColMap, bAllNull: true) : null;
                    //dr["STG"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboStgMap, bAllNull: true) : null;

                    dr["ROW"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboRowMap, bAllNull: true) : null;
                    dr["COL"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboColMap, bAllNull: true) : null;
                    dr["STG"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboStgMap, bAllNull: true) : null;
                }

                // 2023.10.12 추가
                // _sEqpKind : IRODV(I)
                //dr["EQPTID"] = _sEqpKind.Equals("8") || _sEqpKind.Equals("J") ? Util.GetCondition(cboEqpMap, bAllNull: true) : null;
                dr["EQPTID"] = _sEqpKind.Equals("8") || _sEqpKind.Equals("J") || _sEqpKind.Equals("I") ? Util.GetCondition(cboEqpMap, bAllNull: true) : null;
                dtRqst.Rows.Add(dr);

                string sBiz = "DA_SEL_FORMATION_PIN_BAD_RATE3_MB";
                //if (Util.GetCondition(cboOpMap).Substring(2, 2).Equals("17"))
                //    sBiz = "DA_SEL_FORMATION_PIN_BAD_RATE4_MB";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    return;
                }

                string sEqpid = string.Empty;
                int iCol = 0;
                int iRow = -1;
                Double dBadRate = 0;
                string sBgColor = string.Empty;
                string sForeColor = string.Empty;
                //------------------------------------------------------
                //BInding용 Datatable 만들기 
                DataTable dt = new DataView(dtRslt).ToTable(true, new string[] { "EQPTID", "CST_CELL_TYPE_CODE", "EQPTNAME" });
                for (int i = 1; i < 300; i++)
                {
                    dt.Columns.Add("TRAY" + i.ToString(), typeof(string));
                }
                dt.Columns.Add("BAD_POS", typeof(string));
                dt.Columns.Add("BAD_RATE", typeof(string));

                dt.Clear();

                _dt = dt.Copy();
                _dtc = _dt.Copy(); //For Color
                //------------------------------------------------------

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    //299개 limit
                    if (i > 299)
                        continue;
                    if (!sEqpid.Equals(dtRslt.Rows[i]["EQPTID"].ToString()))
                    {
                        iRow++;

                        DataRow d = dt.NewRow();
                        d["EQPTID"] = dtRslt.Rows[i]["EQPTID"].ToString();
                        d["EQPTNAME"] = dtRslt.Rows[i]["EQPTNAME"].ToString();
                        d["CST_CELL_TYPE_CODE"] = dtRslt.Rows[i]["CST_CELL_TYPE_CODE"].ToString();
                        dt.Rows.Add(d);

                        DataRow _d = _dt.NewRow();
                        _d["EQPTID"] = dtRslt.Rows[i]["EQPTID"].ToString();
                        _d["EQPTNAME"] = dtRslt.Rows[i]["EQPTNAME"].ToString();
                        _d["CST_CELL_TYPE_CODE"] = dtRslt.Rows[i]["CST_CELL_TYPE_CODE"].ToString();
                        _dt.Rows.Add(_d);

                        DataRow _dc = _dtc.NewRow();
                        _dc["EQPTID"] = dtRslt.Rows[i]["EQPTID"].ToString();
                        _dc["EQPTNAME"] = dtRslt.Rows[i]["EQPTNAME"].ToString();
                        _dc["CST_CELL_TYPE_CODE"] = dtRslt.Rows[i]["CST_CELL_TYPE_CODE"].ToString();
                        _dtc.Rows.Add(_dc);

                        iCol = dt.Columns["EQPTNAME"].Ordinal + 1;
                    }

                    if (iCol == dt.Columns["EQPTNAME"].Ordinal + 1)
                    {
                        dt.Rows[iRow][dt.Columns["BAD_POS"].Ordinal] = dtRslt.Rows[i]["BAD_POS"].ToString();
                    }
                    else
                    {
                        string[] Bad_Pos = dtRslt.Rows[i]["BAD_POS"].ToString().Split(',');

                        for (int j = 0; j < Bad_Pos.Length; j++)
                        {
                            if (!dt.Rows[iRow][dt.Columns["BAD_POS"].Ordinal].ToString().Split(',').Contains<string>(Bad_Pos[j].ToString()))
                                dt.Rows[iRow][dt.Columns["BAD_POS"].Ordinal] = dt.Rows[iRow][dt.Columns["BAD_POS"].Ordinal].ToString() + "," + Bad_Pos[j].ToString();
                        }
                    }

                    dt.Rows[iRow][iCol] = dtRslt.Rows[i]["BAD_CNT"].ToString();
                    dt.Rows[iRow][dt.Columns["BAD_RATE"].Ordinal] = dtRslt.Rows[i]["BAD_RATE"].ToString();

                    _dt.Rows[iRow][iCol] = dtRslt.Rows[i]["CSTID"].ToString() + "_" + dtRslt.Rows[i]["PROCID"].ToString() + "_" + dtRslt.Rows[i]["PROC_STRT_DTTM"].ToString() + "_" + dtRslt.Rows[i]["LOTID"].ToString() + "_" + dtRslt.Rows[i]["PROCNAME"].ToString() + "_" + dtRslt.Rows[i]["EQPTID"].ToString();

                    dBadRate = Convert.ToDouble(dtRslt.Rows[i]["BAD_CNT"].ToString());

                    //Color Setting
                    sForeColor = "Black";

                    if (dBadRate == 0)
                    {
                        sBgColor = "White";
                    }
                    else if (dBadRate > 0 && dBadRate <= 0.5)
                    {
                        sBgColor = "Lime";
                    }
                    else if (dBadRate > 0.5 && dBadRate <= 1)
                    {
                        sBgColor = "Yellow";
                    }
                    else if (dBadRate > 1 && dBadRate <= 2)
                    {
                        sBgColor = "Cyan";
                    }
                    else if (dBadRate > 2 && dBadRate <= 3)
                    {
                        sBgColor = "Fuchsia";
                    }
                    else if (dBadRate > 3 && dBadRate <= 5)
                    {
                        sBgColor = "Red";
                    }
                    else if (dBadRate > 5 && dBadRate <= 10)
                    {
                        sBgColor = "DarkBlue";
                        sForeColor = "White";
                    }
                    else if (dBadRate > 10)
                    {
                        sBgColor = "Black";
                        sForeColor = "White";
                    }

                    _dtc.Rows[iRow][iCol] = sBgColor + "_" + sForeColor;

                    iCol++;
                    sEqpid = dtRslt.Rows[i]["EQPTID"].ToString();
                }




                for (int i = int.Parse(dt.Columns["TRAY1"].Ordinal.ToString()); i < int.Parse(dt.Columns["TRAY299"].Ordinal.ToString()); i++)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[j][i].ToString()))
                        {
                            string sColName = "TRAY" + (i - 2).ToString();
                            string sHeader = "[*]" + (i - 2).ToString();
                            //데이터가 있는 경우 컬럼을 생성한다.
                            dgEqpMap.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                            {
                                Name = sColName,
                                Header = sHeader,
                                Binding = new Binding()
                                {
                                    Path = new PropertyPath(sColName),
                                    Mode = BindingMode.TwoWay
                                },
                                TextWrapping = TextWrapping.Wrap,
                                IsReadOnly = true
                            });

                            break;
                        }
                    }
                }

                SetGridHeaderSingleEqpMap("BAD_POS", dgEqpMap, 10);
                SetGridHeaderSingleEqpMap("BAD_RATE", dgEqpMap, 10);

                //Data Binding
                Util.GridSetData(dgEqpMap, dt, FrameOperation, true);

                dgEqpMap.Columns["EQPTID"].Visibility = Visibility.Collapsed;
                dgEqpMap.Columns["CST_CELL_TYPE_CODE"].Visibility = Visibility.Collapsed;
                dgEqpMap.Columns["BAD_RATE"].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridHeaderSingleEqpMap(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel),
                IsReadOnly = true,
                CanUserResize = false
            });
        }

        private void SetGridHeaderSingleChannel(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Star),
                IsReadOnly = true,
                CanUserResize = false
            });
        }

        private void InitializeDataGrid(string sComCode, C1DataGrid dg)
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CST_CELL_TYPE_CODE";
                dr["COM_CODE"] = sComCode;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dg);

                if (dtRslt.Rows.Count > 0)
                {
                    int iColName = 65;
                    string sRowCnt = dtRslt.Rows[0]["ATTR1"].ToString();
                    string sColCnt = dtRslt.Rows[0]["ATTR2"].ToString();
                    _sNotUseRowLIst = dtRslt.Rows[0]["ATTR3"].ToString();
                    _sNotUseColLIst = dtRslt.Rows[0]["ATTR4"].ToString();

                    #region Grid 초기화
                    int iMaxCol;
                    int iMaxRow;
                    List<string> rowList = new List<string>();

                    int iColCount = dg.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        int index = (iColCount - i) - 1;
                        dg.Columns.RemoveAt(index);
                    }

                    iMaxRow = Convert.ToInt16(sRowCnt);
                    iMaxCol = Convert.ToInt16(sColCnt);

                    List<DataTable> dtList = new List<DataTable>();

                    double AAA = Math.Round((dg.ActualWidth - 70) / (iMaxCol - 1), 1);
                    int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

                    int iSeq = 1;
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";

                    for (int iCol = 0; iCol < iMaxCol; iCol++)
                    {
                        SetGridHeaderSingleChannel(Convert.ToChar(iColName + iCol).ToString(), dg, iColWidth);
                        dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString(), typeof(string));

                        if (iCol == 0)
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                DataRow row1 = dt.NewRow();

                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                                dt.Rows.Add(row1);
                            }
                        }
                        else
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                            }
                        }
                    }

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrayType(C1DataGrid dg, string sPalletTypeCd)
        {
            if (sPalletTypeCd.Equals("1"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, true);

                dg.GetCell(21, 0).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 2).Presenter.Background = new SolidColorBrush(Colors.DarkGray);

            }
            else if (sPalletTypeCd.Equals("2"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, true);

                dg.GetCell(21, 1).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 3).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
            }
            else if (sPalletTypeCd.Equals("3"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //24개 ROW 생성
                for (int i = 0; i < 24; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, true);
            }
            else if (sPalletTypeCd.Equals("4"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //16개 ROW 생성
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, true);
            }
            else if (sPalletTypeCd.Equals("5"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E", "F", "G" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, true);

                dg.GetCell(21, 1).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 3).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 5).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
            }
        }

        private string GetCheckdGrade()
        {
            string sCheck = "";
            foreach (CheckBox itemChecked in lbDefectGrade.Items)
            {
                if (itemChecked.IsChecked == true)
                {
                    if (sCheck != "")
                        sCheck += "," + itemChecked.Tag.ToString();
                    else
                        sCheck = itemChecked.Tag.ToString();
                }
            }

            return sCheck;
        }

        private void btnSearchGripper_Click(object sender, RoutedEventArgs e)
        {
            GetListGripper();
        }

        private void GetListGripper()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("ROW", typeof(string));
                dtRqst.Columns.Add("COL", typeof(string));
                dtRqst.Columns.Add("STG", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = Util.GetCondition(cboLaneGripper, bAllNull: true);
                dr["FROM_DATE"] = dtpRepairFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpRepairToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQP_KIND_CD"] = _sEqpKind;
                if (!chkAllGripper.IsChecked == true)
                {
                    // 2023.10.12 추가
                    // _sEqpKind : LCI(L)
                    //dr["ROW"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboRowGripper, bAllNull: true) : null;
                    //dr["COL"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboColGripper, bAllNull: true) : null;
                    //dr["STG"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboStgGripper, bAllNull: true) : null;

                    dr["ROW"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboRowGripper, bAllNull: true) : null;
                    dr["COL"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboColGripper, bAllNull: true) : null;
                    dr["STG"] = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboStgGripper, bAllNull: true) : null;
                }

                // 2023.10.12 추가
                // _sEqpKind : IRODV(I)
                //dr["EQP_ID"] = _sEqpKind.Equals("8") ? Util.GetCondition(cboEqpGripper, bAllNull: true) : null;
                dr["EQP_ID"] = _sEqpKind.Equals("8") || _sEqpKind.Equals("I") ? Util.GetCondition(cboEqpGripper, bAllNull: true) : null;
                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PIN_MAINT_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgRepair, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ProcCheck()
        {
            bool bCheck = true;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboOpMap);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_PROCID_F_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (!dtRslt.Rows[0]["PROC_DETL_TYPE_CODE"].ToString().Equals("17"))
                        bCheck = false;
                }
                else
                {
                    bCheck = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }


        private void btnAddGripper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS002_029_REPAIR spRepair = new FCS002_029_REPAIR();
                spRepair.FrameOperation = FrameOperation;

                object[] Parameters = new object[1];
                Parameters[0] = _sEqpKind;

                C1WindowExtension.SetParameters(spRepair, Parameters);

                spRepair.Closed += new EventHandler(spRepair_Closed);
                spRepair.ShowModal();
                spRepair.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
            }
        }

        private void spRepair_Closed(object sender, EventArgs e)
        {
            FCS002_029_REPAIR runStartWindow = sender as FCS002_029_REPAIR;

            runStartWindow.Closed -= new EventHandler(spRepair_Closed);

            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                //btnSearch_Click(null, null);
            }
        }

        private void dgEqpMap_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null) return;


                if (cell.Value == null)
                    return;

                if (cell != null && datagrid.CurrentRow != null)
                {
                    if (dgEqpMap.CurrentColumn.Name.Equals("EQPTID") ||
                        dgEqpMap.CurrentColumn.Name.Equals("BAD_POS") ||
                        dgEqpMap.CurrentColumn.Name.Equals("BAD_RATE"))
                    {
                        return;
                    }

                    _sSelType = string.Empty;

                    //채널조회 탭으로 이동
                    TabChannel.IsSelected = true;

                    Util.gridClear(dgChannel);
                    txtEqpNameChannel.Text = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[dgEqpMap.CurrentRow.Index].DataItem, "EQPTNAME"));
                    txtOpName.Text = string.Empty;
                    txtStatus.Text = string.Empty;
                    txtOpStartDate.Text = string.Empty;
                    txtTrayId.Text = string.Empty;
                    txtBadPos.Text = string.Empty;

                    string sBiz = string.Empty;

                    InitializeDataGrid(Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[dgEqpMap.CurrentRow.Index].DataItem, "CST_CELL_TYPE_CODE")), dgChannel);

                    DataTable dtChannel = DataTableConverter.Convert(dgChannel.ItemsSource);

                    if (dgEqpMap.CurrentColumn.Name.Equals("EQPTNAME"))
                    {
                        _sSelType = "EQPTNAME";
                        ChannelLegend.Visibility = Visibility.Visible;
                        ChannelDfctLegend.Visibility = Visibility.Collapsed;

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("FROM_DATE", typeof(string));
                        dtRqst.Columns.Add("TO_DATE", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[dgEqpMap.CurrentRow.Index].DataItem, "EQPTID"));
                        dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                        dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:59");

                        //dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.AddHours(dtpFromTime.DateTime.Value.Hour).AddMinutes(dtpFromTime.DateTime.Value.Minute).ToString("yyyy-MM-dd HH:mm:00.000");
                        //dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.AddHours(dtpToTime.DateTime.Value.Hour).AddMinutes(dtpToTime.DateTime.Value.Minute).AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:59.997");
                        dtRqst.Rows.Add(dr);

                        //  2023.10.12 일시 표시 추가
                        txtOpStartDate.Text = Convert.ToDateTime(dr["FROM_DATE"]).ToString("yyyy-MM-dd HH:mm") + " ~ "
                                            + Convert.ToDateTime(dr["TO_DATE"]).ToString("yyyy-MM-dd HH:mm");

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_PIN_BAD_TOTAL_COUNT_NJ_MB", "RQSTDT", "RSLTDT", dtRqst);

                        int iCellNo = 1;
                        int iBadCnt = 0;

                        DataTable dt = DataTableConverter.Convert(dgChannel.ItemsSource);
                        for (int iCol = 0; iCol < dgChannel.Columns.Count; iCol++)
                        {
                            for (int iRow = 0; iRow < dgChannel.Rows.Count; iRow++)
                            {
                                if (!dt.Rows[iRow][iCol].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    DataRow[] dr1 = dtRslt.Select("CELL_NO = '" + iCellNo.ToString() + "'");

                                    if (dr1.Length > 0)
                                    {
                                        dt.Rows[iRow][iCol] = Util.NVC(dr1[0]["BAD_CNT"]);
                                        iBadCnt = Convert.ToInt32(dr1[0]["BAD_CNT"].ToString());
                                    }
                                    iCellNo++;
                                }
                            }
                        }
                        Util.GridSetData(dgChannel, dt, FrameOperation, false);
                    }
                    else
                    {
                        _sSelType = "TRAY";

                        // 2023.10.12 ChannelDfctLegend 항시 보기게 수정
                        //if (_sEqpKind.Equals("8"))
                        //{
                        //    ChannelDfctLegend.Visibility = Visibility.Collapsed;
                        //}
                        //else
                        //{
                        //    ChannelDfctLegend.Visibility = Visibility.Visible;
                        //}
                        ChannelLegend.Visibility = Visibility.Collapsed;
                        ChannelDfctLegend.Visibility = Visibility.Visible;

                        string sParam = _dt.Rows[dgEqpMap.CurrentRow.Index][dgEqpMap.CurrentColumn.Index].ToString();
                        string[] sParamArr = sParam.Split('_');

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("EQPKIND", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("PROCID", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        dtRqst.Columns.Add("PROC_STRT_DTTM", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["EQPKIND"] = _sEqpKind;
                        dr["CSTID"] = sParamArr[0];
                        dr["EQPTID"] = sParamArr[5];
                        dr["PROCID"] = sParamArr[1];
                        dr["LOTID"] = sParamArr[3];
                        dr["PROC_STRT_DTTM"] = sParamArr[2];
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CELL_BAD_TYPE_CD_MB", "RQSTDT", "RSLTDT", dtRqst);

                        if (dtRslt.Rows.Count > 0)
                        {
                            txtTrayId.Text = sParamArr[0];
                            txtStatus.Text = dtRslt.Rows[0]["WIPSNAME"].ToString();
                            txtOpStartDate.Text = sParamArr[2];
                            txtOpName.Text = sParamArr[4];
                        }

                        int iCellNo = 1;

                        DataTable dt = DataTableConverter.Convert(dgChannel.ItemsSource);
                        for (int iCol = 0; iCol < dgChannel.Columns.Count; iCol++)
                        {
                            for (int iRow = 0; iRow < dgChannel.Rows.Count; iRow++)
                            {
                                if (!dt.Rows[iRow][iCol].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    DataRow[] dr1 = dtRslt.Select("CELL_NO = '" + iCellNo.ToString() + "'");

                                    if (dr1.Length > 0)
                                    {
                                        dt.Rows[iRow][iCol] = Util.NVC(dr1[0]["DFCT_CODE"]);
                                        txtBadPos.Text = txtBadPos.Text + ", " + dr1[0]["BAD_POS"].ToString();
                                    }
                                    iCellNo++;
                                }
                            }
                        }

                        Util.GridSetData(dgChannel, dt, FrameOperation, false);

                        if (txtBadPos.Text.Length > 0)
                        {
                            txtBadPos.Text = txtBadPos.Text.Substring(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpMap_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                int iRowCht = dgEqpMap.GetRowCount();


                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                    e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("Black") as SolidColorBrush;

                    // 불량위치 칸 줄어듦 방지
                    if (e.Cell.Column.Name.Equals("BAD_POS"))
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    if (!e.Cell.Column.Name.Equals("EQPTID") && !e.Cell.Column.Name.Equals("EQPTNAME") && !e.Cell.Column.Name.Equals("CST_CELL_TYPE_CODE") &&
                        !e.Cell.Column.Name.Equals("BAD_POS") && !e.Cell.Column.Name.Equals("BAD_RATE"))
                    {
                        string sParam = _dtc.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString();
                        if (!string.IsNullOrEmpty(sParam))
                        {
                            string[] sParamArr = sParam.Split('_');

                            string sBgColor = sParamArr[0];
                            string sForeColor = sParamArr[1];

                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(sBgColor) as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(sForeColor) as SolidColorBrush;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("Black") as SolidColorBrush;
                        }
                    }
                }
            }));
        }

        //20220124_불량등급 리스트에 전체선택 기능 추가 START
        private void chkAllDef_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox itemChecked in lbDefectGrade.Items)
            {
                itemChecked.IsChecked = true;
            }

        }
        //20220124_불량등급 리스트에 전체선택 기능 추가 END

        //20220124_불량등급 리스트에 전체선택 기능 추가 START
        private void chkAllDef_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox itemChecked in lbDefectGrade.Items)
            {
                itemChecked.IsChecked = false;
            }
        }

        private void dgChDfctColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    //{
                    //    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTR1").ToString()) as SolidColorBrush;

                    //    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()))
                    //    {
                    //        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()) as SolidColorBrush;
                    //    }
                    //}

                    if ((Util.NVC(e.Cell.Column.Name) == "CBO_CODE") || (Util.NVC(e.Cell.Column.Name) == "CBO_NAME"))
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTR1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR2")).ToString()) as SolidColorBrush;
                        }

                    }
                }
            }));
        }

        private void dgMapColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()) as SolidColorBrush;
                        }
                    }
                }
            }));
        }

        private void dgChColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()) as SolidColorBrush;
                        }
                    }
                }
            }));
        }

        private void dgChannel_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //20220704_검색조건(조회기간) 추가 START
                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////
                //20220704_검색조건(조회기간) 추가 END


                if (_sSelType.Equals("EQPTNAME"))
                {
                    if (!e.Cell.Column.Name.Equals("EQPTID") & !e.Cell.Column.Name.Equals("EQPTNAME") & !e.Cell.Column.Name.Equals("CST_CELL_TYPE_CODE") &
                        !e.Cell.Column.Name.Equals("BAD_POS") & !e.Cell.Column.Name.Equals("BAD_RATE"))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString()));
                        if (!string.IsNullOrEmpty(sValue) && !sValue.Equals("NOT_USE"))
                        {
                            if (int.Parse(sValue) <= 10)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else if (int.Parse(sValue) > 10 && int.Parse(sValue) <= 20)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Silver);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else if (int.Parse(sValue) > 20 && int.Parse(sValue) <= 30)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else if (int.Parse(sValue) > 30 && int.Parse(sValue) <= 40)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.DimGray);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else if (int.Parse(sValue) > 40)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                        }
                    }
                }
                else
                {
                    if (!e.Cell.Column.Name.Equals("EQPTID") & !e.Cell.Column.Name.Equals("EQPTNAME") & !e.Cell.Column.Name.Equals("CST_CELL_TYPE_CODE") &
                        !e.Cell.Column.Name.Equals("BAD_POS") & !e.Cell.Column.Name.Equals("BAD_RATE"))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString()));
                        if (!string.IsNullOrEmpty(sValue) && !sValue.Equals("NOT_USE"))
                        {
                            //채널 불량코드별 색상 Mapping
                            DataRow[] dr2 = _dtch.Select("CBO_CODE = '" + sValue + "'");

                            if (dr2.Length > 0)
                            {
                                e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(dr2[0]["ATTR1"].ToString()) as SolidColorBrush;
                                if (string.IsNullOrEmpty(dr2[0]["ATTR2"].ToString()))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(dr2[0]["ATTR2"].ToString()) as SolidColorBrush;
                                }
                            }
                        }
                    }
                }
            }));
        }

        private void chkAllGripper_Checked(object sender, RoutedEventArgs e)
        {
            cboRowGripper.IsEnabled = false;
            cboColGripper.IsEnabled = false;
            cboStgGripper.IsEnabled = false;
        }

        private void chkAllGripper_Unchecked(object sender, RoutedEventArgs e)
        {
            cboRowGripper.IsEnabled = true;
            cboColGripper.IsEnabled = true;
            cboStgGripper.IsEnabled = true;
        }

        private void dgEqpMap_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgChannel_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgRepair_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            dJobDate = dJobDate.AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
    }
}
