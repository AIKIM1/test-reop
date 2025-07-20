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
  2023.05.03  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.08.17  손동혁 : NA 1동 요청 모델별 검색 조건 생성 모델을 선택하면 해당 모델에 관련된 공정경로 필터링 설정 (모델은 조회조건 아님), COMBOBOX CASE : ROUTE -> LANEMODELROUTE ,
  2023.10.12  김최일 : E20231011-000910 - 투입량/불량/불량률 값이 없는 machine는 보이지 않게 요청
  2025.07.07  전상진 : MI_LS_OSS_0233 : 조회 쿼리 일자 기준 상이 / 시간 조건이 상이
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_029 : UserControl, IWorkArea
    {
        private string _sEqpKind = string.Empty;
        private string _sLaneX = string.Empty;
        Util _Util = new Util();
        bool bUseFlag = false; //2023.08.15 NA1동 모델 필터링 조건 추가

        public IFrameOperation FrameOperation {
            get;
            set;
        }

        public FCS001_029()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sEqpKind))
                _sEqpKind = GetLaneIDForMenu(LoginInfo.CFG_MENUID);

            switch (_sEqpKind)
            {
                case "1": //충방전기
                    _sEqpKind = "1";
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

                case "8": //OCV
                    _sEqpKind = "8";
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
                    _sEqpKind = "J";
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

            dtpFromDateMap.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            dtpRepairFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            ///2023.08.17 
            ///NA 1동 요청으로 인해 모델 별 필터링 조건 추가

            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {

                tbModel.Visibility = Visibility;
                cboModel.Visibility = Visibility;

            }
            else
            {
                tbModel.Visibility = Visibility.Collapsed;
                cboModel.Visibility = System.Windows.Visibility.Collapsed;

            }


            InitCombo();
            InitControl();
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

            CommonCombo_Form _combo = new CommonCombo_Form();
            
            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", _sEqpKind };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterEqpType);



            ///2023.08.17 NA 1동 요청으로 인해 모델 별 조건 추가 NA 1동만 보이게 추가
            ///string[] sFilterLane = { _sLaneX };
            ///C1ComboBox[] cboLaneMapChild = { cboRowMap, cboColMap, cboStgMap, cboEqpMap};
            ///_combo.SetCombo(cboLaneMap, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", sFilter: sFilterLane, cbChild: cboLaneMapChild);
            
            string[] sFilterLane = { _sLaneX };
            C1ComboBox[] cboLaneMapChild = { cboRowMap, cboColMap, cboStgMap, cboEqpMap, cboModel, cboRouteMap };
            _combo.SetCombo(cboLaneMap, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", sFilter: sFilterLane, cbChild: cboLaneMapChild);
             


            C1ComboBox[] cboModelChild = { cboRouteMap };
            C1ComboBox[] cboModelParent = { cboLaneMap };
            string[] sFilter = { null };

            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LANEMODEL", cbParent: cboModelParent, cbChild: cboModelChild, sFilter: sFilter);

            C1ComboBox[] cboRouteParent = { cboLaneMap, cboModel };
            C1ComboBox[] cboRouteChild = { cboOpMap };
            string[] sRouteMap = { null, null, null, null, null };

            _combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, sCase: "LANEMODELROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);
            
            ///2023.08.17 NA 1동 요청으로 인해 모델 별 조건 추가 NA 1동만 보이게 추가
            /// 기존 COMBOBOX CASE : ROUTE ,  COMBOBOX CASE : ROUTE -> LANEMODELROUTE , LANEMODEL 추가
            ///string[] sRouteMap = { null, null, null, null, null };
            ///C1ComboBox[] cboRouteChild = { cboOpMap };
            ///_combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", sFilter: sRouteMap, cbChild: cboRouteChild);



            C1ComboBox[] cboOperMapParent = { cboRouteMap };
            string[] sFilterOperMap = { _sEqpKind, null, null };
            _combo.SetCombo(cboOpMap, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperMapParent, sFilter: sFilterOperMap);

            C1ComboBox[] cboRowMapParent = { cboEqpKind, cboLaneMap };
            _combo.SetCombo(cboRowMap, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW", cbParent: cboRowMapParent);
            _combo.SetCombo(cboColMap, CommonCombo_Form.ComboStatus.NONE, sCase: "COL", cbParent: cboRowMapParent);
            _combo.SetCombo(cboStgMap, CommonCombo_Form.ComboStatus.NONE, sCase: "STG", cbParent: cboRowMapParent);

            C1ComboBox[] cboRowMapParent1 = { cboLaneMap, cboEqpKind }; //20220308_설비리스트출력 조건 수정
            if (_sEqpKind.Equals("J"))
            {
                _combo.SetCombo(cboEqpMap, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPIDBYLANEJIG", cbParent: cboRowMapParent1);
            }
            else
            {
                _combo.SetCombo(cboEqpMap, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboRowMapParent1);
            }

            #endregion

            #region [Gripper 수리정보 Tabpage]

            C1ComboBox[] cboLaneGripperChild = { cboRowGripper, cboColGripper, cboStgGripper, cboEqpGripper };
            _combo.SetCombo(cboLaneGripper, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneGripperChild, sFilter: sFilterLane);

            if (cboLaneGripper.Items.Count > 0)
            {
                cboLaneGripper.SelectedValue = _sLaneX;
            }

            C1ComboBox[] cboParentGripper = { cboEqpKind, cboLaneGripper };
            _combo.SetCombo(cboRowGripper, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW", cbParent: cboParentGripper);
            _combo.SetCombo(cboColGripper, CommonCombo_Form.ComboStatus.NONE, sCase: "COL", cbParent: cboParentGripper);
            _combo.SetCombo(cboStgGripper, CommonCombo_Form.ComboStatus.NONE, sCase: "STG", cbParent: cboParentGripper);

            if (cboRowGripper.Items.Count == 0)
                cboRowGripper.Visibility = Visibility.Collapsed;

            C1ComboBox[] cboEqpGripperParent = { cboLaneGripper, cboEqpKind };

            if (_sEqpKind.Equals("J"))
            {
                _combo.SetCombo(cboEqpGripper, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPIDBYLANEJIG", cbParent: cboEqpGripperParent);
            }
            else
            {
                _combo.SetCombo(cboEqpGripper, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboEqpGripperParent);
            }

         
            #endregion
        }

        private void InitControl()
        {
            try
            {
                //dtpFromDateMap.Value = Util.GetJobDateFrom();
                //dtpFromTimeMap.Value = Util.GetJobDateFrom();
                //dtpToDateMap.Value = Util.GetJobDateTo();
                //dtpToTimeMap.Value = Util.GetJobDateTo();

                //dtpFromDateGripper.Value = Util.GetJobDateFrom();
                //dtpToDateGripper.Value = Util.GetJobDateTo();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOTJUDGE_CBO", "RQSTDT", "RSLTDT", RQSTDT, menuid: LoginInfo.CFG_MENUID);

                //lbDefectGrade.ItemsSource = DataTableConverter.Convert(dtResult);

                //lbDefectGrade.Items.Clear();
                foreach (DataRow drResult in dtResult.Rows)
                {
                    CheckBox cbChk = new CheckBox();
                    cbChk.Tag = drResult["CBO_CODE"];
                    cbChk.Content = drResult["CBO_NAME"];
                    lbDefectGrade.Items.Add(cbChk);
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearchClick(object sender, RoutedEventArgs e)
        {
            GetListMap();
        }

        private void GetListMap()
        {
            try
            {
                //2022.09.01  김령호 : NB1동 VOC Formation issue 62번 요구사항
                Util.gridClear(dgEqpMap);
                for (int i = dgEqpMap.Columns["PIN1"].Index; i <= dgEqpMap.Columns["PIN72"].Index; i++)
                {
                    dgEqpMap.Columns[i].Visibility = Visibility.Visible;
                }
                dgEqpMap.Refresh();


                //SetHeader();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("ROUTE_ID", typeof(string));
                dtRqst.Columns.Add("OP_ID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(DateTime));
                dtRqst.Columns.Add("TO_TIME", typeof(DateTime));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));
                dtRqst.Columns.Add("GRADE", typeof(string));
                dtRqst.Columns.Add("ROW", typeof(string));
                dtRqst.Columns.Add("COL", typeof(string));
                dtRqst.Columns.Add("STG", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = Util.GetCondition(cboLaneMap, bAllNull: true);
                dr["ROUTE_ID"] = Util.GetCondition(cboRouteMap, bAllNull: true);
                dr["OP_ID"] = Util.GetCondition(cboOpMap, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                if (string.IsNullOrEmpty(dr["OP_ID"].ToString())) return;
                
                //dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.AddHours(dtpFromTime.DateTime.Value.Hour).AddMinutes(dtpFromTime.DateTime.Value.Minute).ToString("yyyy-MM-dd HH:mm:00.000");
                //dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.AddHours(dtpToTime.DateTime.Value.Hour).AddMinutes(dtpToTime.DateTime.Value.Minute).AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:59.997");

                // MI_LS_OSS_0233
                dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm") + ":00";
                dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm") + ":00";

                if (!string.IsNullOrEmpty(GetCheckdGrade()))
                    dr["GRADE_CD"] = GetCheckdGrade();

                dr["EQP_KIND_CD"] = _sEqpKind;
                if (!chkAllMap.IsChecked == true)
                {
                    dr["ROW"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboRowMap, bAllNull: true) : null;
                    dr["COL"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboColMap, bAllNull: true) : null;
                    dr["STG"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboStgMap, bAllNull: true) : null;
                }
                dr["EQP_ID"] = _sEqpKind.Equals("8") || _sEqpKind.Equals("J") ? Util.GetCondition(cboEqpMap, bAllNull: true) : null;
                dtRqst.Rows.Add(dr);

                string sBiz = "DA_SEL_FORMATION_PIN_BAD_RATE1";
                if (Util.GetCondition(cboOpMap).Substring(2, 2).Equals("17"))
                    sBiz = "DA_SEL_FORMATION_PIN_BAD_RATE2";

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_TRAY_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                //pivot 처리
                //pivot 의 row 만들기용 distinct
                DataTable dtPivot = new DataView(dtRslt).ToTable(true, new string[] { "EQP_ID", "TRAY_LOC", "EQP_NAME", "LOC_NAME" });

                for (int i = 1; i < 73; i++) //최대 Cell No.
                {
                    dtPivot.Columns.Add("PIN" + i.ToString(), typeof(Int32));
                }
                dtPivot.Columns.Add("INPUT_CNT", typeof(Int32));
                dtPivot.Columns.Add("BAD_CNT", typeof(Int32));
                dtPivot.Columns.Add("BAD_RATE", typeof(double));

                //PK 지정
                dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["EQP_ID"], dtPivot.Columns["TRAY_LOC"] };

                object[] oKeyVal = new object[2];

                DataView dvPivotData = new DataView(dtRslt);
                dvPivotData.RowFilter = "PIN_NO IS NOT NULL";

                DataTable dtPivotData = dvPivotData.ToTable();
                for (int i = 0; i < dtPivotData.Rows.Count; i++)
                {
                    // Set the values of the keys to find.
                    oKeyVal[0] = dtPivotData.Rows[i]["EQP_ID"].ToString();
                    oKeyVal[1] = dtPivotData.Rows[i]["TRAY_LOC"].ToString();

                    DataRow drPivot = dtPivot.Rows.Find(oKeyVal);
                    drPivot["PIN" + dtPivotData.Rows[i]["PIN_NO"]] = dtPivotData.Rows[i]["BAD_CNT"];
                }

                DataTable dtRqstInput = new DataTable();
                dtRqstInput.TableName = "INDATA";
                dtRqstInput.Columns.Add("LANE_ID", typeof(string));
                dtRqstInput.Columns.Add("FROM_TIME", typeof(string));
                dtRqstInput.Columns.Add("TO_TIME", typeof(string));
                dtRqstInput.Columns.Add("ROW", typeof(string));
                dtRqstInput.Columns.Add("COL", typeof(string));
                dtRqstInput.Columns.Add("STG", typeof(string));
                dtRqstInput.Columns.Add("OP_ID", typeof(string));

                DataRow dr1 = dtRqstInput.NewRow();
                dr1["LANE_ID"] = Util.GetCondition(cboLaneMap, bAllNull: true);
                //dr1["FROM_TIME"] = Util.GetDatenTimeCondition(dtpFromDateMap, dtpFromTimeMap, dtFormat: "yyyyMMddHHmm00");
                //dr1["TO_TIME"] = Util.GetDatenTimeCondition(dtpToDateMap, dtpToTimeMap, dtFormat: "yyyyMMddHHmm59");

                //dr1["FROM_TIME"] = dtpFromDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:ss");
                //dr1["TO_TIME"] = dtpToDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:ss");

                // MI_LS_OSS_0233
                dr1["FROM_TIME"] = dtpFromDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm") + ":00";
                dr1["TO_TIME"] = dtpToDateMap.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm") + ":00";

                if (_sEqpKind.Equals("1"))
                {
                    if (!chkAllMap.IsChecked == true)
                    {
                        if(!string.IsNullOrEmpty(Util.GetCondition(cboRowMap, bAllNull: true)))
                            dr1["ROW"] = Util.GetCondition(cboRowMap, bAllNull: true);
                        if (!string.IsNullOrEmpty(Util.GetCondition(cboColMap, bAllNull: true)))
                            dr1["COL"] = Util.GetCondition(cboColMap, bAllNull: true);
                        if (!string.IsNullOrEmpty(Util.GetCondition(cboStgMap, bAllNull: true)))
                            dr1["STG"] = Util.GetCondition(cboStgMap, bAllNull: true);
                    }
                }

                dr1["OP_ID"] = Util.GetCondition(cboOpMap);
                dtRqstInput.Rows.Add(dr1);

                string sCellBiz = "DA_SEL_BOX_CELL_JOBCNT";
                if (_sEqpKind.Equals("8"))
                    sCellBiz = "DA_SEL_BOX_CELL_JOBCNT_OCV";
                if (_sEqpKind.Equals("J"))
                    sCellBiz = "DA_SEL_JF_CELL_JOBCNT";

                DataTable dtRsltInput = new ClientProxy().ExecuteServiceSync(sCellBiz, "RQSTDT", "RSLTDT", dtRqstInput);

                for (int i = 0; i < dtRsltInput.Rows.Count; i++)
                {
                    // Set the values of the keys to find.
                    oKeyVal[0] = dtRsltInput.Rows[i]["EQP_ID"].ToString();
                    oKeyVal[1] = dtRsltInput.Rows[i]["TRAY_LOC"].ToString();

                    DataRow drPivot = dtPivot.Rows.Find(oKeyVal);
                    if (drPivot != null)
                    {
                        drPivot["INPUT_CNT"] = dtRsltInput.Rows[i]["CELL_CNT"];
                    }
                }

                Util.GridSetData(dgEqpMap, dtPivot, FrameOperation, true);

                // 투입수량, 불량률 계산
                string sTemp1, sTemp2;
                for (int i = 0; i < dgEqpMap.GetRowCount(); i++)
                {
                    sTemp1 = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[i].DataItem, "INPUT_CNT"));
                    if (sTemp1.Equals(""))
                        continue;

                    double dTot = 0, dRatio = 0;
                    for (int j = dgEqpMap.Columns["PIN1"].Index; j <= dgEqpMap.Columns["PIN72"].Index; j++)
                    {
                        sTemp2 = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[i].DataItem, dgEqpMap.Columns[j].Name));
                        if (sTemp2.Equals(""))
                            continue;

                        dTot += Convert.ToDouble(sTemp2);
                    }
                    dRatio = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[i].DataItem, "INPUT_CNT")));
                    dRatio = dTot / dRatio * 100;

                    DataTableConverter.SetValue(dgEqpMap.Rows[i].DataItem, "BAD_CNT", dTot.ToString());
                    DataTableConverter.SetValue(dgEqpMap.Rows[i].DataItem, "BAD_RATE", dRatio.ToString("0.##"));
                }

                // 핀별 값이 있는 칼럼만 보이도록
                for (int i = dgEqpMap.Columns["PIN1"].Index; i <= dgEqpMap.Columns["PIN72"].Index; i++)
                {
                    //2022.09.01  김령호 : NB1동 VOC Formation issue 62번 요구사항
                    dgEqpMap.Columns[i].Visibility = Visibility.Collapsed;
                    for (int j = 0; j < dgEqpMap.GetRowCount(); j++)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[j].DataItem, dgEqpMap.Columns[i].Name))))
                        {
                            dgEqpMap.Columns[i].Visibility = Visibility.Visible;
                            break;
                        }
                    }
                }

                //E20231011-000910
                ((System.Data.DataView)dgEqpMap.ItemsSource).Table.DefaultView.Sort = "INPUT_CNT"; //투입량 정렬
                if (((System.Data.DataView)dgEqpMap.ItemsSource).Table.Select("INPUT_CNT > 0 ").Length > 0) // 이부분에서 정렬이 되긴 함(SORT 안해도)
                    Util.GridSetData(dgEqpMap, ((System.Data.DataView)dgEqpMap.ItemsSource).Table.Select("INPUT_CNT > 0 ").CopyToDataTable(), FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void btnSearchGripperClick(object sender, RoutedEventArgs e)
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

                //dr["FROM_DATE"] = Util.GetCondition(dtpFromDateGripper);
                //dr["TO_DATE"] = Util.GetCondition(dtpToDateGripper);

                dr["FROM_DATE"] = dtpRepairFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpRepairToDate.SelectedDateTime.ToString("yyyyMMdd");

                dr["EQP_KIND_CD"] = _sEqpKind;
                if (!chkAllGripper.IsChecked == true)
                {
                    dr["ROW"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboRowGripper, bAllNull: true) : null;
                    dr["COL"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboColGripper, bAllNull: true) : null;
                    dr["STG"] = _sEqpKind.Equals("1") ? Util.GetCondition(cboStgGripper, bAllNull: true) : null;
                }
                dr["EQP_ID"] = _sEqpKind.Equals("8") ? Util.GetCondition(cboEqpGripper, bAllNull: true) : null;
                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PIN_MAINT", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgRepair, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAddGripperClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_029_REPAIR spRepair = new FCS001_029_REPAIR();
                spRepair.FrameOperation = FrameOperation;

                object[] Parameters = new object[1];
                Parameters[0] = _sEqpKind;

                C1WindowExtension.SetParameters(spRepair, Parameters);

                spRepair.Closed += new EventHandler(spRepair_Closed);
                spRepair.ShowModal();
                spRepair.CenterOnScreen();

                //Point pnt = e.GetPosition(null);
                //C1.WPF.DataGrid.DataGridCell cell = dgLowVoltage.GetCellFromPoint(pnt);

                //if (cell != null)
                //{
                //    if (!cell.Column.Name.Equals("LOT_ID"))
                //    {
                //        return;
                //    }

                //    FCS001_019_TRAY_SEL _xlsTraySel = new FCS001_019_TRAY_SEL();
                //    _xlsTraySel.FrameOperation = FrameOperation;

                //    if (_xlsTraySel != null)
                //    {
                //        object[] Parameters = new object[1];
                //        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLowVoltage.Rows[cell.Row.Index].DataItem, "LOT_ID")).ToString();

                //        C1WindowExtension.SetParameters(_xlsTraySel, Parameters);

                //        _xlsTraySel.Closed += new EventHandler(_xlsTraySel_Closed);
                //        _xlsTraySel.ShowModal();
                //        _xlsTraySel.CenterOnScreen();
                //    }

                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void spRepair_Closed(object sender, EventArgs e)
        {
            FCS001_029_REPAIR runStartWindow = sender as FCS001_029_REPAIR;

            runStartWindow.Closed -= new EventHandler(spRepair_Closed);

            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                //btnSearch_Click(null, null);
            }
        }

        private void dgEqpMapDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (dgEqpMap.CurrentColumn.Index < 3) return;
            //if (dgEqpMap.Columns["PIN50"].Index < dgEqpMap.CurrentColumn.Index) return;

            if (dgEqpMap.CurrentColumn.Name.Equals("EQP_NAME") ||
                dgEqpMap.CurrentColumn.Name.Equals("INPUT_CNT") ||
                dgEqpMap.CurrentColumn.Name.Equals("BAD_CNT") ||
                dgEqpMap.CurrentColumn.Name.Equals("BAD_RATE")) return;

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            //채널조회 탭으로 이동
            TabChannel.IsSelected = true;

            Util.gridClear(dgChannel);
            Util.gridClear(dgChannelDetail);

            txtEqpNameChannel.Text = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[dgEqpMap.CurrentRow.Index].DataItem, "EQP_NAME"));

            string sBiz = string.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("PIN_NO", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));
                dtRqst.Columns.Add("OP_ID", typeof(string));
                dtRqst.Columns.Add("TRAY_LOC", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQP_ID"] = Util.NVC(DataTableConverter.GetValue(dgEqpMap.Rows[dgEqpMap.CurrentRow.Index].DataItem, "EQP_ID"));
                //dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmmss");
                //dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmmss");

                // MI_LS_OSS_0233
                dr["FROM_DATE"] = dtpFromDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm") + "00";
                dr["TO_DATE"] = dtpToDateMap.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm") + "00";
                dr["GRADE_CD"] = GetCheckdGrade();
                dr["OP_ID"] = Util.GetCondition(cboOpMap);
                dr["TRAY_LOC"] = Util.NVC(dgEqpMap.GetValue("TRAY_LOC"));
                dtRqst.Rows.Add(dr);

                if (dgEqpMap.CurrentColumn.Name.Equals("LOC_NAME"))  //전체
                {
                    if (ProcCheck())
                    {
                        sBiz = "DA_SEL_FORMATION_PIN_GRADE_PG";
                    }
                    else
                    {
                        sBiz = "DA_SEL_FORMATION_PIN_GRADE";
                    }
                }
                else if (dgEqpMap.CurrentColumn.Index >= dgEqpMap.Columns["PIN1"].Index && dgEqpMap.CurrentColumn.Index <= dgEqpMap.Columns["PIN72"].Index) //pin 번호별
                {
                    ////  2022.09.08  김령호 : NB1동 VOC Formation issue 62번 추가 요구사항 적용 (3번)
                    dr["PIN_NO"] = dgEqpMap.Columns[dgEqpMap.CurrentColumn.Index].Name.Replace("PIN", "");
                    //dr["PIN_NO"] = dgEqpMap.Columns[dgEqpMap.SelectedIndex].Name.Replace("PIN", "");

                    if (ProcCheck())
                    {
                        sBiz = "DA_SEL_FORMATION_PIN_GRADE_PG";
                    }
                    else
                    {
                        sBiz = "DA_SEL_FORMATION_PIN_GRADE";
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);

                var groupQuery = (from table in dtRslt.AsEnumerable()
                                  group table by new { TIME = table["TIME"], GRADE_CD = table["GRADE_CD"] }
                                  into groupedTable
                                  select new
                                  {
                                      x = groupedTable.Key,
                                      y = groupedTable.Count()
                                  }).OrderBy(item => item.x.TIME.ToString() + item.x.GRADE_CD.ToString());

                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("TIME", typeof(string));
                dt.Columns.Add("GRADE_CD", typeof(string));
                dt.Columns.Add("CNT", typeof(string));

                foreach (var item in groupQuery)
                {
                    DataRow d = dt.NewRow();
                    d["TIME"] = item.x.TIME;
                    d["GRADE_CD"] = item.x.GRADE_CD;
                    d["CNT"] = item.y;
                    dt.Rows.Add(d);
                }
                Util.GridSetData(dgChannel, dt, FrameOperation, true);
                Util.GridSetData(dgChannelDetail, dtRslt, FrameOperation, true);
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
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_PROCID_F", "RQSTDT", "RSLTDT", dtRqst);

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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.ToString().StartsWith("PIN"))
                    {
                        string sTemp = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            //2022.09.08  김령호: NB1동 VOC Formation issue 62번 추가 요구사항 적용(2번)
                            if (Convert.ToInt16(sTemp) == 1)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Blue);
                                //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Blue);
                            }
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

        //20220124_불량등급 리스트에 전체선택 기능 추가 END


        /// 2023.08.17 NA 1동 요청 Model 값 수정 시 모델에 해당하는 작업공정 필터
        private void cboModel_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();


            string[] sRouteMap = { null, Util.GetCondition(cboModel), null, null, null };

            C1ComboBox[] cboRouteParent = { cboLaneMap, cboModel };

            C1ComboBox[] cboRouteChild = { cboOpMap };
            //string[] sRouteMap = { null, null, null, null, null };
            //_combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);
            // _combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", sFilter: sRouteMap);
            //_combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", sFilter: sRouteMap, cbChild: cboRouteChild);

         //   C1ComboBox[] cboRouteParent = { cboStockerType, cboLine, cboModel };
         //   _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANEMODELROUTE", cbParent: cboRouteParent);
            _combo.SetCombo(cboRouteMap, CommonCombo_Form.ComboStatus.ALL, cbChild: cboRouteChild , cbParent: cboRouteParent, sFilter: sRouteMap , sCase: "LANEMODELROUTE" );
        }

       


    }
}
