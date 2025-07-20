/*************************************************************************************
 Created Date :
      Creator : 
   Decription : Tray 정보조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29 DEV    Initial Coding
  2021.05.13 PSM    Cell Data Grid 범위설정 및 Cell ID 검색 하여 배경색 표시하는 로직 변경
                    설정 버튼 클릭하여 배경색이 바뀐 경우 새로운 Tray를 조회하거나, 설정버튼을 다시 누를 때 까지 바뀌지 않아야 함.   
  2021.09.30 PSM    rdoCurrA 추가 (powergrading, powercharging 선택 시 전류 단위는 A)
  2021.10.12 KDH    Auto Calibration Lot 표시 
  2022.01.28 KDH    O등급 표시 : O(공정검사의뢰) , O(PQC검사 의뢰)
  2022.03.03 PSM    특별등록 시 적재된 트레이 모두 특별 등록
  2022.05.26 JYD    Cell ID 입력후 엔터검색시 우측 Cell ID 검색안되는 현상 수정.목록 갯수에 따른 열 조정.
  2022.07.04 JYD    LOTID, CSTID 널체크에 "" 체크 추가. Cell 정보 기본 설정 추가
  2022.07.11 JYD    대기상태일때 Sample 출하 버튼 무조건 비활성화(김룡근)
  2022.08.19 LJM    변경이력 버튼 추가 
  2022.08.25 최도훈 Aging 입고 버튼에 Null 예외처리 추가
  2022.09.01 최도훈 과불량 변경 버튼 추가. 동작시 불량한계초과여부 변경(WIPATTR 내 DFCT_LIMIT_OVER_FLAG)
  2022.09.21 최도훈 BR_GET_LOAD_TRAY_INFO 호출시 파라미터 AREAID 추가
  2022.09.27 최도훈 Carrier의 비정상코드(ABNORM_TRF_RSN_CODE) 표시, 비정상 상태 해제 버튼 추가 (ESWA3 RTD팀 요청사항)
  2022.09.28 LJM    이력의 마지막 공정은 종료상태이지만 TRAY 공정상태는 작업중일 때 진행중인 공정 종료 확인 팝업 활성화되도록 수정 
  2022.10.07 최도훈 Crack 해제 기능 추가 (WipAttr 내 SCRP_TRAY_FLAG)
  2022.11.11 이정미 Tray 예약, Truoble 상태 미표기 오류 수정
  2025.01.10 scpark [E20241011-001270] 양극 lane 판정 추가에 따른 WINDER LANE 조회
  2025.05.17 이지은 샘플 등록 버튼 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_110.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_021 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        // ---------------------------------------------
        //          Line ID 상수
        // ---------------------------------------------
        public const string FORMATION_EQPTYPE = "1";
        public const string LOWAGING_EQPTYPE = "3";
        public const string HIGHAGING_EQPTYPE = "4";
        public const string OUTAGING_EQPTYPE = "7";
        public const string PREAGING_EQPTYPE = "9";
        public const string JUDGE_EQPTYPE = "B";
        public const string SELECTOR_EQPTYPE = "6";
        public const string GRADER_EQPTYPE = "5";
        public const string REPRINT_EQPTYPE = "K";
        public const string AGING_LINE_1 = "001";
        public const string AGING_LINE_2 = "002";
        public const string CHARGE = "11";
        public const string DISCHARGE = "12";


        public const string VENTID_DATATYPE = "VENT";
        public const string CANID_DATATYPE = "CAN";
        public const string OCV_DATATYPE = "O";
        public const string IMP_DATATYPE = "R";
        public const string CAPA_DATATYPE = "C";
        public const string CURR_DATATYPE = "I";
        public const string VOLT_DATATYPE = "V";
        public const string VOLT_AVG_DATATYPE = "V2";
        public const string POWER_DATATYPE = "PW";
        public const string FIRST_VOLT_DATATYPE = "FV";
        public const string TEMP_SUB_DATATYPE = "SV";
        public const string POWERGRADE_DATATYPE = "P";
        public const string OCV_SAMPLE_DATATYPE = "S";
        public const string IMP_SAMPLE_DATATYPE = "T";
        public const string CCVAL_DATATYPE = "CCV";
        public const string CCTIME_DATATYPE = "CCT";
        public const string CVVAL_DATATYPE = "CVV";
        public const string CVTIME_DATATYPE = "CVT";
        public const string PJCODE = "B";
        public const string JFTEMPER_DATATTYPE = "JT";  // 2015.04 ZHU ZIMING
        public const string JFPRESSURE_DATATTYPE = "JP"; // 2015.04 ZHU ZIMING
        public const string FORTEMPER_DATATTYPE = "FOR"; // 충방전/LCI 온도
        /* 2014.1.17 정종덕D // [CSR ID:2444504] 자동차 셀 전 모델 및 호기 W-code 및 비선형 mV/day 기준 정보 등록 
         * 저전압 데이터 타입추가(트레이 조회화면에서 사용)
         * */
        public const string LOWVOLTAGE_OCVJUDG_OCV_DATATYPE = "LOW_R";
        public const string LOWVOLTAGE_OCVJUDG_DOCV_DATATYPE = "LOW_E";

        //2014.02.26 강진구D | 2491135   | 대용량 방전기 전압값 표시 件 / 방전 전 전압, 방전 후 전압 상수 추가 POWERGRADING_STRAT_VOLT_DATATYPE, POWERGRADING_END_VOLT_DATATYPE
        public const string POWERGRADING_STRAT_VOLT_DATATYPE = "PWDS_V";
        public const string POWERGRADING_END_VOLT_DATATYPE = "PWDE_V";
        public const string IROCV_IMP = "IR";  // 2016.07.06 SMS IR-OCV추가로 수정함.
        public const string IROCV_VOLT = "IV"; // 2015.07.06 SMS IR-OCV추가로 수정함.
        public const string IROCV_JUDG = "IJ"; // 2015.07.06 SMS IR-OCV추가로 수정함.
        public const string VISION_GRADER = "VG"; // 2015.07.06 SMS IR-OCV추가로 수정함.
        public const string FITTEDCAPACITY_DATATYPE = "F";    //2020.03.10 lth PQA FITTED JUDGMENT
        public const string FITTEDGRDAE_DATATYPE = "G";     //2020.03.10 lth PQA FITTED JUDGMENT
        public const string XGRADE = "X";
        public const string DQDV_AVG = "DA";  //2020.03.12 KYZ DQDV_AVG
        public const string OD_GRADE = "OG";  //2021.06.10 LX 
        public const string OD_YN = "ODYN";   //2021.06.10 LX
        public const string DEFAULT_VEW_CELL = "DEFAULT_VEW_CELL";

        // 충방전 온도 TYPE
        public const int FORMATEMP_AVG = 2;
        public const int FORMATEMP_MIN = 3;
        public const int FORMATEMP_MAX = 4;

        //2025.01.10 scpark[E20241011 - 001270] 양극 lane 판정 추가에 따른 WINDER LANE 조회
        public const string WND_LOT = "WL";

        public bool _FinCheck = false;  // 충방전기 자동 Pin체크에서 넘어온 경우 체크
                                        // Tray 선택 화면에서 넘어 올 경우
        public bool _oldTray = false; //
        public bool _bTrayInfo = false;

        Util _Util = new Util();         //2021.10.12 Auto Calibration Lot 표시
        public bool bAtCalibUse = false; //2021.10.12 Auto Calibration Lot 표시

        private string _sTrayID; //TRAY_ID
        private string _sTrayNo; //TRAY_NO
        private string _sLotID; //LOTID
        private string _sTrayLine;
        private string _sTrayLineName;
        private string _sFinCD = "PROC";   //C:Default , P:FIN Check용
        private string _sModelID;
        private string _sCurrOper;
        private string _sCurrOperGrpID;
        private string _sCurrOperDetlTypeCD;
        private string _sCurrYN;
        private string _sOPStartTime;
        private string _sOPEndTime;
        private string _sEqpID;
        private string _sSpecial;
        private string _sShipmentYN;
        private string _sActYN = "N";   //다른 창에서 넘어오는지 체크 해서 Active Event 제어
        private int _AgingOutPriority = 0;
        private int iLastRow;  //공정이력 마지막줄 확인을 위함 //200420 KJE
        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;
        private string _sRdoBtnName;
        private object _object;
        private object _sRouteID;
        private string _sFrstTrayID; //TRAY_ID
        private DateTime _dAgingISSDTTM;
        private string _CellType;

        public const string OCVDATATYPE = "O";
        public const string IMPDATATYPE = "R";
        public const string CAPADATATYPE = "C";
        public const string CURRDATATYPE = "I";
        public const string VOLTDATATYPE = "V";

        private bool bsetRange = false; //범위설정
        private bool bsetColorRange = false; // 색조범위설정
        private bool bsetCellRange = false; //Cell ID 설정

        private DataSet dsRslt = new DataSet();

        // 2021-05-13
        // Cell data grid에서 범위설정, Cell ID 설정 시 해당 Cell ID 담기 위한 용도
        // Cell 범위 설정되어 Background Color가 바뀐 경우, 다른 공정의 측정값이 조회(좌측 공정 진행 grid 선택시 변경) 되더라도 유지되어야 함. (위치 정보 필요)
        private DataTable dtValueSublot = new DataTable();
        private DataTable dtSublotList = new DataTable();
        private DataTable dtWipCell = new DataTable();
        private DataTable dtCellList = new DataTable();
        private DataTable dtColorSet = new DataTable();
        private DataTable dtColorRange = new DataTable(); // 색조 
        private DataTable dtFDSList = new DataTable(); // 발열셀 LIST


        public string TrayID
        {
            set { this._sTrayID = value; }
            get { return this._sTrayID; }
        }

        public string TrayNO
        {
            set { this._sTrayNo = value; }
            get { return this._sTrayNo; }
        }

        public string FinCD
        {
            set { this._sFinCD = value; }
            get { return this._sFinCD; }
        }

        public bool FinCheck
        {
            set { this._FinCheck = value; }
            get { return this._FinCheck; }
        }

        public string EQPID
        {
            set { this._sEqpID = value; }
            get { return this._sEqpID; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }

        public FCS002_021()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTrayID.Text) || !string.IsNullOrEmpty(txtTrayNo.Text))
            {
                ClearControlValue();
            }

            try
            {
                btnRecipe.Visibility = Visibility.Collapsed;
                //btnAging.Visibility = Visibility.Collapsed;
                btnChgDfctLmtOverFlag.Visibility = Visibility.Collapsed;
                btnRlsTrfRsnCode.Visibility = Visibility.Collapsed;
                btnHistory.Visibility = Visibility.Collapsed;
                btnWGradeJudge.Visibility = Visibility.Collapsed;
                rdoManual.Visibility = Visibility.Collapsed;
                rdoAuto.Visibility = Visibility.Collapsed;
                rdoCanID.Visibility = Visibility.Collapsed;
                rdoVentID.Visibility = Visibility.Collapsed;

                // NFF 인경우
                switch (GetCellType())
                {
                    case "N":
                        rdoCanID.Visibility = Visibility.Visible;
                        rdoVentID.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }


                bAtCalibUse = _Util.IsAreaCommonCodeUse("ATCALIB_TYPE_CODE", "ATCALIB_TYPE_CODE");  //2021.10.12 Auto Calibration Lot 표시
                                                                                                    //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                dtValueSublot.Columns.Add("SUBLOTID", typeof(string));
                dtSublotList.Columns.Add("SUBLOTID", typeof(string));
                dtColorSet.Columns.Add("SUBLOTID", typeof(string));
                dtColorRange.Columns.Add("SUBLOTID", typeof(string));
                dtColorRange.Columns.Add("COLOR", typeof(string));

                //다른 화면에서 넘어온 경우
                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    ClearControlValue();
                    TrayID = Util.NVC(parameters[0]);
                    TrayNO = Util.NVC(parameters[1]);
                    FinCD = Util.NVC(parameters[2]);
                    FinCheck = Util.NVC(parameters[3]).Equals("true") ? true : false;
                    EQPID = Util.NVC(parameters[4]);
                    if (Util.NVC(parameters[5]).Equals("Y")) { chkHist.IsChecked = true; }
                    else { chkHist.IsChecked = false; } //PROCESS_HIST_YN
                    if (!string.IsNullOrEmpty(_sTrayNo)) { txtTrayNo.Text = _sTrayNo; }
                    if (!string.IsNullOrEmpty(_sTrayID)) { txtTrayID.Text = _sTrayID; }
                    GetTrayInfo();
                }
                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 기준정보 setting 및 쿼리 개발 필요.
        private void InitializeDataGrid(string sCSTID, C1DataGrid dg)
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = sCSTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_GRID_SET_INFO_MB", "INDATA", "OUTDATA", RQSTDT); // 20230207 개발필요.
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

                    //Grid Column 생성
                    for (int iCol = 0; iCol < iMaxCol; iCol++)
                    {
                        SetGridHeaderSingle(Convert.ToChar(iColName + iCol).ToString(), dg, iColWidth);
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

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel),
                IsReadOnly = true
            });
        }

        #endregion

        #region [Method]
        private void GetTrayInfo()
        {
            try
            {
                btnForcOutReset.IsEnabled = false;

                //CELL Data Grid 범위설정, Cell ID 설정 부분은 새로운 Tray 조회 될 때만 Clear 되어야 함.
                txtRange1.Text = string.Empty;
                txtRange2.Text = string.Empty;
                txtCellList.Text = string.Empty;
                dtValueSublot.Rows.Clear();
                dtSublotList.Rows.Clear();
                dtCellList.Rows.Clear();
                dtColorSet.Rows.Clear();
                dtColorRange.Rows.Clear();

                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSTAT", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("PROCESS_HIST_YN", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(txtTrayID.Text))
                {
                    dr["CSTID"] = Util.NVC(txtTrayID.Text);
                }
                else
                {
                    dr["CSTID"] = null;
                }

                if (!string.IsNullOrEmpty(txtTrayNo.Text))
                {
                    dr["LOTID"] = Util.NVC(txtTrayNo.Text);
                }
                else
                {
                    dr["LOTID"] = null;
                }

                if (!string.IsNullOrEmpty(_sEqpID))
                {
                    dr["EQPTID"] = _sEqpID;
                }
                else
                {
                    dr["EQPTID"] = null;
                }

                if ((bool)chkHist.IsChecked)
                {
                    dr["PROCESS_HIST_YN"] = "Y";
                }
                else
                {
                    dr["PROCESS_HIST_YN"] = "N";
                }

                if ((dr["CSTID"] == null || dr["CSTID"].ToString() == "") &&
                    (dr["LOTID"] == null || dr["LOTID"].ToString() == ""))
                {
                    Util.Alert("FM_ME_0069"); //Tray ID 또는 Tray No를 입력해주세요.
                    return;
                }

                if (!string.IsNullOrEmpty(dr["LOTID"].ToString()))
                {
                    dr["CSTID"] = null;
                }
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inDataTable.Rows.Add(dr);

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inDataTable);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_LOAD_TRAY_INFO_MB", "INDATA", "RET_TRAY_PROCESS,RET_DELTA_OCV,RET_TRAY_INFO,RET_TRAY_OP_STATUS,RET_OUT_OCV,RET_DEGAS_ESTIMATED,RET_W_LOW_VOLT,RET_LAST_OP,OUTDATA", inDataSet);

                if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("1"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("EMPTY_TRAY"); // 공 Tray
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("2"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("INFO_DEL") + " " + ObjectDic.Instance.GetObjectName("TRAY"); // 정보삭제 TRAY
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("3"))
                {
                    txtLotID.Text = ObjectDic.Instance.GetObjectName("HISTORY") + " " + ObjectDic.Instance.GetObjectName("TRAY"); // 이력 TRAY
                    txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    return;
                }
                else if (dsRslt.Tables["RET_TRAY_INFO"].Rows.Count == 0)
                {
                    Util.Alert("FM_ME_0078");  //Tray 정보가 존재하지 않습니다.
                    return;
                }

                DataTable dtForcCode = GetForcPortCode();




                for (int i = 0; i < dtForcCode.Rows.Count; i++)
                {
                    if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_FORC_MANL_PORT"].ToString().Equals(dtForcCode.Rows[i]["CMCODE"].ToString()))
                        btnForcOutReset.IsEnabled = true;
                }



                _sTrayLine = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQSGID"].ToString();
                _sTrayLineName = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQSGNAME"].ToString();
                _sCurrOper = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].ToString();
                _sCurrOperGrpID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROC_GR_ID"].ToString();
                _sCurrOperDetlTypeCD = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROC_DETL_TYPE_CD"].ToString();
                _sLotID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROD_LOTID"].ToString();
                _sTrayNo = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString();
                _sLotID = string.Empty;
                _sTrayID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString();
                _sFinCD = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTSTAT"].ToString();
                _sModelID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["MDLLOT_ID"].ToString();
                _AgingOutPriority = int.Parse(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString());
                _FinCheck = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ATCALIB_TYPE_CODE"].ToString().Equals("P") ? true : false;
                _sRouteID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ROUTID"].ToString();
                if (!string.IsNullOrEmpty(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString()))
                    _dAgingISSDTTM = Convert.ToDateTime(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString());
                _sFrstTrayID = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FRST_CSTID"].ToString();

                // 1번째 Row
                txtTrayID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString();
                if (string.IsNullOrEmpty(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CSTID"].ToString()))
                {
                    txtTrayID.Text = TrayID;
                }

                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["DUMMY_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Black);
                }

                txtTrayNo.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOTID"].ToString();
                txtROUTID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ROUTID"].ToString();
                txtROUTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtInCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["INPUT_SUBLOT_QTY"].ToString();
                txtInCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 2번째 Row
                txtLotID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROD_LOTID"].ToString();
                txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtPRODID.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PRODID"].ToString();
                txtPRODID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtDfctLmtOverFlag.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["DFCT_LIMIT_OVER_FLAG"].ToString();
                txtDfctLmtOverFlag.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNowCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIP_QTY"].ToString();
                txtNowCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 3번째 Row
                txtOper.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCNAME"].ToString();
                txtOper.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Tag = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].ToString();
                txtTrayOpStatus.Foreground = new SolidColorBrush(Colors.Black);
                switch (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString())
                {
                    case "PROC":
                        txtTrayOpStatus.Tag = "S";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("WORKING");  //작업중
                        break;
                    case "END":
                        txtTrayOpStatus.Tag = "END";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("완공");  //완공
                        break;
                    case "WAIT":
                        txtTrayOpStatus.Tag = "S";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("대기");  //대기
                        break;
                    case "TERM":
                        txtTrayOpStatus.Tag = "TERM";
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("재공종료"); //재공종료
                        chkHist.IsChecked = true;
                        break;
                    default:
                        txtTrayOpStatus.Tag = string.Empty;
                        txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("INFO_ERR");  //정보이상
                        break;
                }

                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ISS_RSV_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayOpStatus.Tag = "P";
                    txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("RESV");  //예약
                }

                //추가요청
                if (dsRslt.Tables["RET_TRAY_OP_STATUS"].Rows[0]["ABNORM_FLAG"].ToString().Equals("Y"))
                {
                    txtTrayOpStatus.Tag = "T";
                    txtTrayOpStatus.Text = ObjectDic.Instance.GetObjectName("TROUBLE");  //Trouble
                }

                //공정종료 상태이고 차기공정이 Aging인 Tray만 Aging 입고처리 버튼 활성화 조건에서 MENUAUTH 제외
                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString().Equals("WAIT") &&
                    (_sCurrOperGrpID.Equals("3") || _sCurrOperGrpID.Equals("4") || _sCurrOperGrpID.Equals("7") || _sCurrOperGrpID.Equals("9")))
                {
                    btnAging.IsEnabled = true;

                }
                else
                {
                    btnAging.IsEnabled = false;
                }
                txtNextOp.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["NEXT_OP_NAME"].ToString();
                txtNextOp.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNextOp.Tag = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["NEXT_OP_ID"].ToString();
                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString() == string.Empty)
                {
                    txtOpPlanTime.Text = string.Empty;
                }
                else
                {
                    txtOpPlanTime.Text = DateTime.Parse(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_SCHD_DTTM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                }
                txtOpPlanTime.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 4번째 Row
                txtAbnormTrfRsnCode.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ABNORM_TRF_RSN_CODE"].ToString();
                txtAbnormTrfRsnCode.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtLotWeek.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["LOT_WEEK"].ToString();
                txtLotWeek.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayLine.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQSGID"].ToString();
                txtTrayLine.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CST_CELL_QTY"].ToString();
                txtTrayCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 5번째 Row
                txtDecentration.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["CST_ECCEN_DISTANCE"].ToString();
                txtDecentration.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtEQPT_CUR.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["EQPT_CUR"].ToString();
                txtEQPT_CUR.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 안보이게 숨김 처리
                txtGoodCellCnt.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["GOOD_SUBLOT_QTY"].ToString();
                //txtCrackState.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SCRP_TRAY_FLAG"].ToString();
                //txtCrackState.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                //특별관리
                _sSpecial = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_FLAG"].ToString();
                _sShipmentYN = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["ISS_RSV_FLAG"].ToString();
                if (!string.IsNullOrEmpty(_sSpecial) && !_sSpecial.Equals("N"))
                {
                    if (string.IsNullOrEmpty(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_GR_ID"].ToString().Trim()))
                    {
                        txtSpc.Text = ObjectDic.Instance.GetObjectName("SPECIAL");
                        txtSpc.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    }
                    else
                    {
                        txtSpc.Text = dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_GR_ID"].ToString().Trim();
                        txtSpc.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_REL_SCHD_DTTM"])))
                    {
                        txtSpclRelDate.Text = Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["FORM_SPCL_REL_SCHD_DTTM"]);
                    }

                    txtSpc.Foreground = new SolidColorBrush(Colors.Red);
                    txtTrayID.Foreground = new SolidColorBrush(Colors.Red);
                    lblSpecial.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpSelect.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpDes.Foreground = new SolidColorBrush(Colors.Red);
                    txtSpclRelDate.Foreground = new SolidColorBrush(Colors.Red);

                    txtSpSelect.Text = Util.NVC(dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NAME"]);
                    txtSpSelect.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                    if (_sSpecial.Equals("I"))
                    {
                        txtSpc.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtTrayID.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        lblSpecial.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpSelect.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpDes.Foreground = new SolidColorBrush(Colors.DarkOrange);
                        txtSpclRelDate.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    }
                    txtSpDes.Text = " " + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NOTE"].ToString().Trim();
                }
                else
                {
                    txtSpc.Foreground = new SolidColorBrush(Colors.Black);
                    lblSpecial.Foreground = new SolidColorBrush(Colors.Black);

                    //특별관리 아닐때도 더미로 생성시 생성내역 표시
                    txtSpDes.Foreground = new SolidColorBrush(Colors.Black);
                    txtSpDes.Text = " " + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["SPCL_NOTE"].ToString().Trim();
                    txtSpDes.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                }


                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString().Equals("7"))
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("SAMPLE_SHIPPING") + "("
                        + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["UPDUSER"].ToString() + ")";
                    //sample 출고해제 버튼 
                    btnSampleOut.Content = ObjectDic.Instance.GetObjectName("SAMPLE_REL");  //Sample 출고해제
                }
                else if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["AGING_ISS_PRIORITY_NO"].ToString().Equals("9"))
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("FORCE_SHIPPING") + "("   //강제출고중
                       + dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["UPDUSER"].ToString() + ")";
                }
                else
                {
                    txtSample.Text = string.Empty;
                    txtSample.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                    //sample 출고해제 버튼 
                    btnSampleOut.Content = ObjectDic.Instance.GetObjectName("SAMPLE_SHIP");  //Sample 출고
                }

                if (_FinCheck || _sFinCD.Equals("P")) //충방전기 자동 Pin체크에서 넘어온경우 
                {
                    btnForceOut.IsEnabled = false;
                    btnManual.IsEnabled = false;
                    btnDOCV.IsEnabled = false;
                }
                else
                {
                    if (FrameOperation.AUTHORITY.Equals("W"))
                    {
                        btnForceOut.IsEnabled = true;
                        btnDOCV.IsEnabled = true;
                        btnManual.IsEnabled = true;
                    }
                    else
                    {
                        btnForceOut.IsEnabled = false;
                        btnDOCV.IsEnabled = false;
                        btnManual.IsEnabled = false;
                    }
                }

                //다음 공정이 LCI 일때 S등급 초기화 활성화
                if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["NEXT_OP_ID"].ToString().Equals("FF1A01"))
                {
                    btnResetS.IsEnabled = true;
                }

                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("TIME_OVER_YN");
                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("JUDG_OP_YN");
                dsRslt.Tables["RET_TRAY_PROCESS"].Columns.Add("PROFILE_YN");
                Util.GridSetData(dgProcess, dsRslt.Tables["RET_TRAY_PROCESS"], this.FrameOperation);
                iLastRow = -1;

                //마지막 공정 Row 저장(CURR_YN = 'Y'일 때만)
                if (dgProcess.Rows.Count > 0 && Util.NVC(DataTableConverter.GetValue(dgProcess.Rows.Count - 1, "CURR_YN")).Equals("Y"))
                {
                    iLastRow = dgProcess.GetRowCount() - 1;
                }

                Util.GridSetData(dgDOCV, dsRslt.Tables["RET_DELTA_OCV"], this.FrameOperation);
                Util.GridSetData(dgSampleOCV, dsRslt.Tables["RET_OUT_OCV"], this.FrameOperation);

                if (!_FinCheck) //"P"
                {
                    if (dsRslt.Tables["RET_DEGAS_ESTIMATED"] != null && dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows.Count > 0
                        && !string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["STARTTIME"]))
                        && !string.IsNullOrEmpty(Util.NVC(dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["ENDTIME"])))
                    {
                        DataGridRowAdd(dgProcess, 1);
                        int liRow = dgProcess.Rows.Count - 1;
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROCNAME", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["OP_NAME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WIPDTTM_ST", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["STARTTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WIPDTTM_ED", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["ENDTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "WORKTIME", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["WORKTIME"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "EQP_ID", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["EQP_ID"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROCID", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["OP_ID"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "JUDG_OP_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["JUDG_OP_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "PROFILE_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["PROFILE_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "IRR_HIST_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["IRR_HIST_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "CURR_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["CURR_YN"].ToString());
                        DataTableConverter.SetValue(dgProcess.Rows[liRow].DataItem, "TIME_OVER_YN", dsRslt.Tables["RET_DEGAS_ESTIMATED"].Rows[0]["TIME_OVER_YN"].ToString());
                    }
                }

                //viewpoint
                //진행공정목록 마지막 Row로 Scroll View Point 설정
                if (_sFinCD.Equals("D") || _sFinCD.Equals("H"))
                {
                    btnHistory.IsEnabled = false;
                }
                else
                {
                    //마지막 공정일 경우만 btnHistory 활성화 시킴
                    btnHistory.IsEnabled = false;
                    if (dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["PROCID"].Equals(dsRslt.Tables["RET_LAST_OP"].Rows[0]["PROCID"]))
                    {
                        btnHistory.IsEnabled = true;
                    }
                }

                if (_sCurrOperGrpID.Equals(LOWAGING_EQPTYPE) || _sCurrOperGrpID.Equals(OUTAGING_EQPTYPE) || _sCurrOperGrpID.Equals(PREAGING_EQPTYPE))
                {
                    if (_AgingOutPriority == 9)
                    {
                        btnForceOut.IsEnabled = false;
                        btnSampleOut.IsEnabled = false;
                    }
                    else
                    {
                        btnForceOut.IsEnabled = true;
                        btnSampleOut.IsEnabled = true;
                    }
                }
                else
                {
                    btnForceOut.IsEnabled = false;
                    btnSampleOut.IsEnabled = false;
                }

                //Sample 출고해제 추가
                if (_AgingOutPriority == 7)
                {
                    btnSampleOut.IsEnabled = true;
                }

                // 대기일때 무조건 비활성화(김룡근)
                if (_AgingOutPriority != 7 && dsRslt.Tables["RET_TRAY_INFO"].Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                {
                    btnSampleOut.IsEnabled = false;
                }

                if (string.IsNullOrEmpty(_sTrayID))
                {
                    InitializeDataGrid(_sFrstTrayID, dgCell);
                }
                else
                {
                    InitializeDataGrid(_sTrayID, dgCell);
                }
                SetRadioButton(_sCurrOperDetlTypeCD, 0);

                //발열셀 list 조회
                GetFDSInfo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetFDSInfo()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _sTrayNo;
                inDataTable.Rows.Add(dr);

                dtFDSList = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_SUBLOT_FDS_DFCT_BY_LOT_MB", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable ChkFDSCell()
        {
            DataTable ChkFDS = new DataTable();
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _sTrayNo;
                inDataTable.Rows.Add(dr);

                ChkFDS = new ClientProxy().ExecuteServiceSync("DA_SEL_FDS_DFCT_SUBLOT_BY_LOT_MB", "INDATA", "OUTDATA", inDataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return ChkFDS;
        }

        private void ClearControlValue()
        {
            try
            {
                _sTrayLine = string.Empty;
                _sLotID = string.Empty;
                _sFinCD = string.Empty;
                _sEqpID = string.Empty;
                _sModelID = string.Empty;
                _sTrayLineName = string.Empty;
                _sCurrOper = string.Empty;
                _sCurrOperGrpID = string.Empty;
                _sCurrOperDetlTypeCD = string.Empty;
                _sTrayNo = string.Empty;
                _sTrayID = string.Empty;
                _AgingOutPriority = 0;
                _FinCheck = false;
                _sRouteID = string.Empty;
                _sFrstTrayID = string.Empty;

                // 1번째 Row
                txtTrayID.Foreground = new SolidColorBrush(Colors.Black);
                txtROUTID.Text = string.Empty;
                txtROUTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtInCellCnt.Text = string.Empty;
                txtInCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 2번째 Row
                txtLotID.Text = string.Empty;
                txtLotID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtPRODID.Text = string.Empty;
                txtPRODID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtDfctLmtOverFlag.Text = string.Empty;
                txtDfctLmtOverFlag.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNowCellCnt.Text = string.Empty;
                txtNowCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 3번째 Row
                txtOper.Text = string.Empty;
                txtOper.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtOper.Tag = string.Empty;
                txtTrayOpStatus.Text = string.Empty;
                txtTrayOpStatus.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayOpStatus.Tag = "S";
                txtTrayOpStatus.Foreground = new SolidColorBrush(Colors.Black);
                txtNextOp.Text = string.Empty;
                txtNextOp.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtNextOp.Tag = string.Empty;
                txtOpPlanTime.Text = string.Empty;
                txtOpPlanTime.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 4번째 Row
                txtAbnormTrfRsnCode.Text = string.Empty;
                txtAbnormTrfRsnCode.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtLotWeek.Text = string.Empty;
                txtLotWeek.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayLine.Text = string.Empty;
                txtTrayLine.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                txtTrayCellCnt.Text = string.Empty;
                txtTrayCellCnt.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                txtEQPT_CUR.Text = string.Empty;
                txtEQPT_CUR.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                // 5번째 Row
                txtDecentration.Text = string.Empty;
                txtDecentration.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 안보이게 숨김 처리
                txtGoodCellCnt.Text = string.Empty;
                //txtCrackState.Text = string.Empty;
                //txtCrackState.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;

                // 화면 우측 TextBox
                txtSpc.Text = string.Empty;
                txtSpSelect.Text = string.Empty;
                txtSpDes.Text = string.Empty;
                lblSpecial.Foreground = new SolidColorBrush(Colors.Black);
                txtSpclRelDate.Text = string.Empty;

                txtSample.Text = string.Empty;

                //모든 라디오버튼 비활성화
                foreach (Control c in rdoGroup.Children)
                {
                    if (c.GetType().Equals(typeof(RadioButton)))
                    {
                        (c as RadioButton).Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;
                        (c as RadioButton).IsEnabled = false;
                    }
                }

                Util.gridClear(dgProcess);
                Util.gridClear(dgCell);
                Util.gridClear(dgDOCV);
                Util.gridClear(dgSampleOCV);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRadioButton(string pCurrOperDetlTypeCD, int pRow)
        {
            rdoCapa.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;           //용량
            rdoCurr.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;           //전류
            rdoOCV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;            //OCV
            rdoImp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;            //Imp
            rdoVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;           //종료전압
            rdoGrade.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;          //등급
            rdoPower.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;          //Power
            rdoCellID.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //Cell ID
            rdoVentID.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //Cell ID
            rdoCanID.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //Cell ID
            rdoPresure.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //압력
            rdoTemp.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;           //온도
            rdoBadChannel.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;     //Bad Ch.
            rdoIrOcvJudg.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;      //IR OCV판정
            rdoIrOcvIR.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //IR OCV저항
            rdoIrOcvV.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //IR OCV전압
            rdoVisionGrade.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;    //Vision등급
            rdoODGrade.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //X1등급
            rdoODYN.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;           //X1여부
            rdoAvgVolt.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //평균전압
            rdoFitCapa.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //Fit용량
            rdoXgrade.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //X등급
            rdoDQDVAVG.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;        //DQDV평균값
            rdoWndLot.Style = Application.Current.Resources["RadioButtonUnusedStyle"] as Style;         //와인더 lot

            foreach (Control c in rdoGroup.Children)
            {
                if (c.GetType().Equals(typeof(RadioButton)))
                {
                    (c as RadioButton).IsChecked = false;
                    (c as RadioButton).IsEnabled = false;
                }

            }

            rdoFitCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
            rdoFitCapa.IsEnabled = true;
            rdoXgrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
            rdoXgrade.IsEnabled = true;

            switch (pCurrOperDetlTypeCD)
            {
                // 13 : OCV, 81 : 전용 OCV, 88 : 출하 OCV, A1 : Delta OCV, A2 : K-Val, I1 : IR-OCV
                case "88":
                case "81":
                case "A1": //13: OCV, 81: 전용OCV, A1:DELTA OCV , J3: JIG OCV
                case "A2":
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;


                case "13":
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    // rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    // rdoTemp.IsEnabled = true;

                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;
                // 11 : Charge, 12 : DisCharge, 19 : CPF Charge

                case "1C":
                case "1B":
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPower.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPower.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoAvgVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoAvgVolt.IsEnabled = true;
                    //   rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    //   rdoTemp.IsEnabled = true;

                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;   // 11 : Charge, 12 : DisCharge, 19 : CPF Charge
                case "11":
                case "12":

                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPower.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPower.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoAvgVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoAvgVolt.IsEnabled = true;
                    rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoTemp.IsEnabled = true;

                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;
                // 방전용량대체
                case "A5":
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPower.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPower.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoAvgVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoAvgVolt.IsEnabled = true;
                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;

                case "1A":
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPower.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPower.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoAvgVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoAvgVolt.IsEnabled = true;
                    rdoDQDVAVG.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoDQDVAVG.IsEnabled = true;
                    // rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    //  rdoTemp.IsEnabled = true;

                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;

                // 14 : DC-Impendence, 15 : DC-Impendence Discharge, 16 : DC-Impendence Charge                
                case "14":
                case "15":
                case "16":  //41: AC-IMP, 14:DC-IMP, 15: 충전DC-IMP, 16: 방전DC-IMP
                    rdoImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoImp.IsEnabled = true;
                    rdoImp.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoImp, new RoutedEventArgs());
                    break;

                // 82 : AC-Impedance,  89 : 출하 AC-Impedance
                case "82":
                case "89":
                    rdoImp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoImp.IsEnabled = true;
                    rdoImp.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoImp, new RoutedEventArgs());
                    break;

                // 온도검사기 관련 공정 추가
                case "T1":
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoVolt.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoVolt, new RoutedEventArgs());
                    break;

                case "J1":   // 2015.04 ZHU ZIMING Jig Formation 충전 공정
                case "J2":   // Jig Formation 방전 공정
                    rdoCapa.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCapa.IsEnabled = true;
                    rdoCapa.IsChecked = true;
                    rdoCurr.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCurr.IsEnabled = true;
                    rdoVolt.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVolt.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPower.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPower.IsEnabled = true;
                    rdoPresure.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPresure.IsEnabled = true;
                    rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoTemp.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoCapa, new RoutedEventArgs());
                    break;

                case "J3":
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoPresure.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoPresure.IsEnabled = true;
                    rdoTemp.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoTemp.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;

                case "I1":  // IROCV 추가
                    rdoIrOcvIR.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvIR.IsEnabled = true;
                    rdoIrOcvIR.IsChecked = true;
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoIrOcvV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvV.IsEnabled = true;
                    rdoIrOcvJudg.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvJudg.IsEnabled = true;
                    rdo_Click(rdoIrOcvIR, new RoutedEventArgs());
                    break;

                case "G1":  // BOTTOMCHECK  EQP_KIND_CD
                case "51":  // EOL 추가
                case "V1":  // VISION GRADE 추가
                    rdoOCV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoOCV.IsEnabled = true;
                    rdoOCV.IsChecked = true;
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoBadChannel.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoBadChannel.IsEnabled = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdoIrOcvIR.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvIR.IsEnabled = true;
                    rdoIrOcvV.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvV.IsEnabled = true;
                    rdoIrOcvJudg.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoIrOcvJudg.IsEnabled = true;
                    rdoVisionGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoVisionGrade.IsEnabled = true;
                    rdoODGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoODGrade.IsEnabled = true;
                    rdoODYN.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoODYN.IsEnabled = true;
                    rdo_Click(rdoOCV, new RoutedEventArgs());
                    break;

                default:
                    rdoGrade.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoGrade.IsEnabled = true;
                    rdoGrade.IsChecked = true;
                    rdoCellID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                    rdoCellID.IsEnabled = true;
                    if (_CellType == "N")
                    {
                        rdoVentID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoVentID.IsEnabled = true;
                        rdoCanID.Style = Application.Current.Resources["SearchCondition_RadioButtonStyle"] as Style;
                        rdoCanID.IsEnabled = true;
                    }
                    rdo_Click(rdoGrade, new RoutedEventArgs());
                    break;
            }

        }

        private void SetRange()
        {
            dtValueSublot.Rows.Clear();
            dtColorSet.Rows.Clear();
            dtColorRange.Rows.Clear();
            if (string.IsNullOrEmpty(txtRange1.Text) || string.IsNullOrEmpty(txtRange2.Text))
            {
                return;
            }
            bsetRange = true;
            bsetColorRange = false;

            string range1 = txtRange1.Text.ToUpper();
            string range2 = txtRange2.Text.ToUpper();

            DataTable _TempT = DataTableConverter.Convert(dgCell.ItemsSource);

            for (int j = 0; j < dgCell.Columns.Count; j++)
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    if (_TempT.Rows[i][j].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                    {
                        continue;
                    }

                    string value = Util.NVC(dgCell.GetCell(i, j).Text).ToUpper();
                    string sCellId = Util.NVC(dtCellList.Rows[i][j].ToString());

                    if (rdoGrade.IsChecked == true) //범위가 등급일 경우 샘플등급(1)으로 인해 미처리 현상 수정 (24.03.05)
                    {
                        if (value.Equals(range1) || value.Equals(range2))
                        {
                            DataRow dr = dtValueSublot.NewRow();
                            dr["SUBLOTID"] = Util.NVC(value);
                            dtValueSublot.Rows.Add(dr);

                            DataRow drColor = dtColorSet.NewRow();
                            drColor["SUBLOTID"] = Util.NVC(sCellId);
                            dtColorSet.Rows.Add(drColor);
                        }
                    }
                    else if (IsNumeric(value)) //범위가 측정값일 경우
                    {
                        if (!IsNumeric(range1) || !IsNumeric(range2)) return;
                        if (double.Parse(value) >= double.Parse(range1) && double.Parse(value) <= double.Parse(range2))
                        {
                            DataRow dr = dtValueSublot.NewRow();
                            dr["SUBLOTID"] = Util.NVC(value);
                            dtValueSublot.Rows.Add(dr);

                            DataRow drColor = dtColorSet.NewRow();
                            drColor["SUBLOTID"] = Util.NVC(sCellId);
                            dtColorSet.Rows.Add(drColor);
                        }
                    }
                }
            }

            Util.GridSetData(dgCell, _TempT, this.FrameOperation);
        }

        private void SetCellRange()
        {
            dtSublotList.Rows.Clear();
            if (string.IsNullOrEmpty(txtCellList.Text)) return;
            bsetCellRange = true;

            string[] arrCell = txtCellList.Text.Split(',');

            DataTable _TempT = DataTableConverter.Convert(dgCell.ItemsSource);

            for (int j = 0; j < dgCell.Columns.Count; j++)
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    if (_TempT.Rows[i][j].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                    {
                        continue;
                    }

                    string curCell = Util.NVC(dgCell.GetCell(i, j).Text).ToUpper();
                    for (int k = 0; k < arrCell.Length; k++)
                    {

                        if (curCell.Equals(arrCell[k]))
                        {
                            DataRow dr = dtSublotList.NewRow();
                            dr["SUBLOTID"] = curCell;
                            dtSublotList.Rows.Add(dr);
                        }
                    }
                }
            }

            Util.GridSetData(dgCell, _TempT, this.FrameOperation);
        }

        private void GetTemp()
        {
            try
            {
                //DataTable inDataTable = new DataTable();
                //inDataTable.Columns.Add("LOTID", typeof(string));
                //inDataTable.Columns.Add("PROCID", typeof(string));

                //DataRow dr = inDataTable.NewRow();
                //dr["LOTID"] = _sTrayNo;
                //dr["PROCID"] = _sCurrOper;
                //inDataTable.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAYINFO_BY_TRAYNO_TEMP", "INDATA", "OUTDATA", inDataTable);
                //Util.gridClear(dgTemp);

                //if (dtRslt.Rows.Count > 0)
                //{
                //    DataGridRowAdd(dgTemp, 1);
                //}

                //foreach (DataRow drRslt in dtRslt.Rows)
                //{
                //    DataTableConverter.SetValue(dgTemp.Rows[0].DataItem, "TEMP" + drRslt[0].ToString(), drRslt[1].ToString());
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDataQuery(string pDataType)
        {
            DataSet InDataSet = new DataSet();
            DataSet OutDataSet = new DataSet();

            string sDataType = string.Empty;
            string sBizID = string.Empty;
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("OP_START_TIME", typeof(string));
                inDataTable.Columns.Add("OP_END_TIME", typeof(string));
                inDataTable.Columns.Add("MEAS_TYPE_CD", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _sTrayNo;
                dr["EQPTID"] = _sEqpID;
                dr["PROCID"] = _sCurrOper;
                dr["OP_START_TIME"] = _sOPStartTime;
                dr["OP_END_TIME"] = _sOPEndTime;
                dr["MEAS_TYPE_CD"] = pDataType;
                dr["ROUTID"] = _sRouteID;

                inDataTable.Rows.Add(dr);
                if (_sCurrYN != "N")
                {
                    switch (pDataType)
                    {
                        // 기본정보 Cell 보기
                        case DEFAULT_VEW_CELL: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_DEFAULT_VIEW_MB";
                            break;

                        // VENTID 보기
                        case VENTID_DATATYPE: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_VENTID_VIEW_MB";
                            break;

                        // CANID 보기
                        case CANID_DATATYPE: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_CANID_VIEW_MB";
                            break;

                        case CAPA_DATATYPE: //용량
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_CAPA_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CAPA";
                            break;

                        case CURR_DATATYPE: //전류
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_CURR_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CURR";
                            break;

                        case OCV_DATATYPE: //OCV
                            sBizID = "DA_SEL_LOAD_CELL_DATA_OCV_MB";
                            break;

                        case IMP_DATATYPE: //IMP
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IMP_MB";
                            break;

                        case VOLT_DATATYPE: //종료전압
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_VOLT_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_VOLT";
                            break;

                        case POWER_DATATYPE: //Power
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_POWER_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_POWER_MB";
                            break;

                        case JFPRESSURE_DATATTYPE: //압력
                            sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_PRESS_MB";
                            break;

                        case JFTEMPER_DATATTYPE: //온도
                            sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_TEMP_MB";
                            break;

                        case FORTEMPER_DATATTYPE: //온도(평균, 최소, 최대)
                            sBizID = "DA_SEL_LOAD_CELL_DATA_FORM_TEMP_MB";
                            break;

                        case IROCV_JUDG: //IR OCV 판정
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IROCV_JUDG_MB";
                            break;

                        case IROCV_IMP: //IR OCV저항
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IROCV_IMP_MB";
                            break;

                        case IROCV_VOLT: //IR OCV전압
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IROCV_VOLT_MB";
                            break;

                        case VISION_GRADER: //Vision등급
                            sBizID = "DA_SEL_LOAD_CELL_DATA_VISION_GRADE_MB";
                            break;

                        case OD_GRADE: //X1 등급
                            sBizID = "DA_SEL_LOAD_CELL_DATA_OD_GRADE_MB";
                            break;

                        case OD_YN: //X1 여부
                            sBizID = "DA_SEL_LOAD_CELL_DATA_ODYN_GRADE_MB";
                            break;

                        case VOLT_AVG_DATATYPE: //평균전압
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_AVG_VOLT_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_AVG_VOLT_MB";
                            break;

                        case FITTEDCAPACITY_DATATYPE: //Fit용량
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_FITCAPA_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FITCAPA";
                            break;

                        case XGRADE: //X등급
                            sBizID = "DA_SEL_LOAD_CELL_DATA_XGRADE_MB";
                            break;

                        case DQDV_AVG: //DQDV평균값

                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_DQDV_AVG_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_DQDV_AVG_MB";
                            break;

                        case WND_LOT: //2025.01.10 scpark [E20241011-001270] 양극 lane 판정 추가에 따른 WINDER LANE 조회
                            sBizID = "DA_SEL_LOAD_CELL_DATA_WND_LOT_MB";
                            break;

                        default:
                            if (_sFinCD == "C")
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_C_MB";
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_MB";
                            }
                            break;
                    }
                }
                else
                {
                    switch (pDataType)
                    {
                        // 기본정보 Cell 보기
                        case DEFAULT_VEW_CELL: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_DEFAULT_VIEW_MB";
                            break;

                        // VENTID 보기
                        case VENTID_DATATYPE: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_VENTID_VIEW_MB";
                            break;

                        // CANID 보기
                        case CANID_DATATYPE: //CELL ID
                            sBizID = "DA_SEL_LOAD_CELL_DATA_CANID_VIEW_MB";
                            break;

                        case CURR_DATATYPE: //전류
                        case VOLT_DATATYPE: //종료전압
                        case CAPA_DATATYPE: //용량
                        case POWER_DATATYPE: //Power
                        case FITTEDCAPACITY_DATATYPE: //Fit용량
                        case VOLT_AVG_DATATYPE: //평균전압
                        case DQDV_AVG: //DQDV평균값
                            if (_sCurrOperDetlTypeCD == "1A" || _sCurrOperDetlTypeCD == "1B" || _sCurrOperDetlTypeCD == "1C")
                                sBizID = "DA_SEL_LOAD_CELL_DATA_LCI_HIST_MB";
                            else
                                sBizID = "DA_SEL_LOAD_CELL_DATA_CHG_HIST_MB";
                            break;

                        case OCV_DATATYPE: //OCV
                            sBizID = "DA_SEL_LOAD_CELL_DATA_OCV_HIST_MB";
                            break;

                        case IMP_DATATYPE: //IMP
                            sBizID = "DA_SEL_LOAD_CELL_DATA_PW_HIST_MB";
                            break;

                        case JFPRESSURE_DATATTYPE: //압력
                        case JFTEMPER_DATATTYPE: //온도
                            sBizID = "DA_SEL_LOAD_CELL_DATA_JIGCNB_HIST_MB";
                            break;

                        case FORTEMPER_DATATTYPE: //온도(평균, 최소, 최대)
                            sBizID = "DA_SEL_LOAD_CELL_DATA_FORM_TEMP_HIST_MB";
                            break;

                        case IROCV_JUDG: //IR OCV 판정
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IROCV_JUDG_MB";
                            break;

                        case IROCV_IMP: //IR OCV저항
                        case IROCV_VOLT: //IR OCV전압
                            sBizID = "DA_SEL_LOAD_CELL_DATA_IROCV_HIST_MB";
                            break;

                        case VISION_GRADER: //Vision등급
                        case OD_GRADE: //X1 등급
                        case OD_YN: //X1 여부
                            sBizID = "DA_SEL_LOAD_CELL_DATA_VISION_HIST_MB";
                            break;

                        case XGRADE: //X등급
                            sBizID = "DA_SEL_LOAD_CELL_DATA_XGRADE_MB";
                            break;

                        case WND_LOT: //2025.01.10 scpark [E20241011-001270] 양극 lane 판정 추가에 따른 WINDER LANE 조회
                            sBizID = "DA_SEL_LOAD_CELL_DATA_WND_LOT_MB";
                            break;

                        default:
                            if (_sFinCD == "C")
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_C_MB";
                            }
                            else
                            {
                                sBizID = "DA_SEL_LOAD_CELL_DATA_FINCD_MB";
                            }
                            break;
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizID, "RQSTDT", "RSLTDT", inDataTable);


                if (pDataType != FORTEMPER_DATATTYPE)
                {
                    dtWipCell = dtRslt.Copy();
                }

                if (dtRslt.Rows.Count > 0)
                {
                    if (pDataType == FORTEMPER_DATATTYPE)
                    {
                        SetTempData(dtRslt);
                    }
                    else
                    {
                        SetCellData(dtRslt);
                    }
                }
                else
                {
                    Util.gridClear(dgCell);
                    if (string.IsNullOrEmpty(_sTrayID))
                    {
                        InitializeDataGrid(_sFrstTrayID, dgCell);
                    }
                    else
                    {
                        InitializeDataGrid(_sTrayID, dgCell);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public static Boolean IsNumeric(string pTarget)
        {
            double dNullable;
            return double.TryParse(pTarget, System.Globalization.NumberStyles.Any, null, out dNullable);
        }

        public static Boolean IsNumeric(object oTagraet)
        {
            return IsNumeric(oTagraet.ToString());
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int rowCount)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();

                int addRows = 0;
                if (Math.Abs(rowCount) > 0)
                {
                    if (rowCount + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else
                    {
                        addRows = rowCount;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetBadChannel(string sTrayNo, string sOperID, string sOperGr_Detl_ID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_DETL_CODE", typeof(string));
                dtRqst.Columns.Add("EQPKIND", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sTrayNo;
                dr["PROCID"] = sOperID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROC_GR_DETL_CODE"] = sOperGr_Detl_ID;
                dr["EQPKIND"] = _sCurrOperGrpID;

                if (sOperGr_Detl_ID == "1A")
                    dr["EQPKIND"] = "L";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_BAD_CHANNEL_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);

                SetCellData(dtRslt);


            }
            catch (System.Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCellData(DataTable dtRslt)
        {
            int iCellNo = 1;

            DataTable _TempT = DataTableConverter.Convert(dgCell.ItemsSource);
            dtCellList = DataTableConverter.Convert(dgCell.ItemsSource);

            for (int iCol = 0; iCol < _TempT.Columns.Count; iCol++)
            {
                for (int iRow = 0; iRow < _TempT.Rows.Count; iRow++)
                {
                    if (!_TempT.Rows[iRow][iCol].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                    {
                        DataRow[] dr = dtRslt.Select("CSTSLOT = '" + iCellNo.ToString() + "'");

                        if (dr.Length > 0)
                        {
                            _TempT.Rows[iRow][iCol] = Util.NVC(dr[0]["VALUE"]);
                            dtCellList.Rows[iRow][iCol] = Util.NVC(dr[0]["SUBLOTID"]);
                        }
                        iCellNo++;
                    }
                }
            }
            //dgCell.ItemsSource = DataTableConverter.Convert(_TempT);
            Util.GridSetData(dgCell, _TempT, FrameOperation, true);
        }


        private void SetTempData(DataTable dtRslt)
        {
            //SetTempGrid(dgCell);

            int tmp_point = 1;
            int tmp_type = 2;
            DataTable _TempT = DataTableConverter.Convert(dgCell.ItemsSource);
            dtCellList = DataTableConverter.Convert(dgCell.ItemsSource);
            string sData = "";
            string Type = "";
            for (int iCol = 0; iCol < _TempT.Columns.Count; iCol++)
            {
                for (int iRow = 0; iRow < _TempT.Rows.Count; iRow++)
                {
                    if (tmp_type == 5)
                    {
                        tmp_type = 2;
                        sData = "";
                        tmp_point++;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        DataRow[] dr = dtRslt.Select("TMPR_POINT = '" + tmp_point.ToString() + "' AND TMPR_TYPE_CODE = '" + tmp_type.ToString() + "'");

                        if (dr.Length > 0)
                        {
                            switch (tmp_type)
                            {
                                case FORMATEMP_AVG:
                                    Type = ObjectDic.Instance.GetObjectName("평균") + " : ";
                                    sData = Type + Util.NVC(dr[0]["TMPR_VALUE"]);
                                    break;
                                case FORMATEMP_MIN:
                                    Type = ObjectDic.Instance.GetObjectName("최소") + " : ";
                                    sData = sData + "\n" + Type + Util.NVC(dr[0]["TMPR_VALUE"]);
                                    break;
                                case FORMATEMP_MAX:
                                    Type = ObjectDic.Instance.GetObjectName("최대") + " : ";
                                    sData = sData + "\n" + Type + Util.NVC(dr[0]["TMPR_VALUE"]);
                                    break;
                            }


                        }
                        tmp_type++;
                    }

                    _TempT.Rows[iRow][iCol] = sData;
                }

            }

            Util.GridSetData(dgCell, _TempT, FrameOperation, true);



        }


        private void SetTempGrid(C1DataGrid dg)
        {
            int iMaxCol = 8;
            int iMaxRow = 6;
            int iColName = 65;
            List<string> rowList = new List<string>();

            int iColCount = dg.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                int index = (iColCount - i) - 1;
                dg.Columns.RemoveAt(index);
            }

            // iMaxRow = Convert.ToInt16(sRowCnt);
            // iMaxCol = Convert.ToInt16(sColCnt);

            List<DataTable> dtList = new List<DataTable>();

            double AAA = Math.Round((dg.ActualWidth - 250) / (iMaxCol - 1), 1);
            int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";

            //Grid Column 생성
            for (int iCol = 0; iCol < iMaxCol; iCol++)
            {
                SetGridHeaderSingle(Convert.ToChar(iColName + iCol).ToString(), dg, iColWidth);
                dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString(), typeof(string));


                if (iCol == 0)
                {
                    for (int iRow = 0; iRow < iMaxRow; iRow++)
                    {
                        DataRow row1 = dt.NewRow();

                        row1[Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                        dt.Rows.Add(row1);
                    }
                }
                else
                {
                    for (int iRow = 0; iRow < iMaxRow; iRow++)
                    {
                        dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                    }
                }
            }
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        private DataTable GetForcPortCode()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("CMCDTYPE", typeof(string));
            dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["CMCDTYPE"] = "FORM_FORC_MANL_PORT_CODE";
            dr["ATTRIBUTE1"] = "Y";

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTR_F_MB", "RQSTDT", "RSLTDT", dtRqst);


            return dtRslt;

        }

        #endregion

        #region [Event]
        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                chkColor.IsChecked = false;

                if (e.Key == Key.Enter && txtTrayID.Text.Length == 10)
                {
                    _sLotID = null;
                    _sTrayNo = null;
                    _sTrayID = txtTrayID.Text;
                    txtTrayNo.Text = null;
                    ClearControlValue();
                    if ((bool)chkHist.IsChecked)
                    {
                        Util.gridClear(dgCell);

                        FCS002_021_SEL_TRAY sel_tray = new FCS002_021_SEL_TRAY();
                        sel_tray.FrameOperation = FrameOperation;

                        object[] parameters = new object[1];
                        parameters[0] = Util.NVC(txtTrayID.Text);

                        C1WindowExtension.SetParameters(sel_tray, parameters);
                        sel_tray.Closed += new EventHandler(sel_tray_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => sel_tray.ShowModal()));
                        return;
                    }
                    ClearControlValue();
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void sel_tray_Closed(object sender, EventArgs e)
        {
            FCS002_021_SEL_TRAY window = sender as FCS002_021_SEL_TRAY;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                txtTrayNo.Text = _sTrayNo;
                txtTrayNo.Focus();
            }
            this.grdMain.Children.Remove(window);
        }

        private void txtTrayNo_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    chkColor.IsChecked = false;

                    _sTrayNo = txtTrayNo.Text;
                    _sLotID = string.Empty;
                    txtTrayID.Text = string.Empty;
                    ClearControlValue();
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_sTrayNo))
            {
                RadioButton rbClick = (RadioButton)sender;

                _object = rbClick;

                if (rbClick.Name.ToString().Equals("rdoCapa") && (rbClick.IsEnabled == true)) GetDataQuery(CAPA_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoCurr") && (rbClick.IsEnabled == true)) GetDataQuery(CURR_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoOCV") && (rbClick.IsEnabled == true)) GetDataQuery(OCV_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoImp") && (rbClick.IsEnabled == true)) GetDataQuery(IMP_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoVolt") && (rbClick.IsEnabled == true)) GetDataQuery(VOLT_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoGrade") && (rbClick.IsEnabled == true)) GetDataQuery("");
                if (rbClick.Name.ToString().Equals("rdoPower") && (rbClick.IsEnabled == true)) GetDataQuery(POWER_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoCellID") && (rbClick.IsEnabled == true)) GetDataQuery(DEFAULT_VEW_CELL);
                if (rbClick.Name.ToString().Equals("rdoVentID") && (rbClick.IsEnabled == true)) GetDataQuery(VENTID_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoCanID") && (rbClick.IsEnabled == true)) GetDataQuery(CANID_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoPresure") && (rbClick.IsEnabled == true)) GetDataQuery(JFPRESSURE_DATATTYPE);
                if (rbClick.Name.ToString().Equals("rdoTemp") && (rbClick.IsEnabled == true))
                    if (_sCurrOperDetlTypeCD == "11" || _sCurrOperDetlTypeCD == "12" ||
                         _sCurrOperDetlTypeCD == "13")//1a 11 12 13
                        GetDataQuery(FORTEMPER_DATATTYPE);
                    else
                        GetDataQuery(JFTEMPER_DATATTYPE);

                if (rbClick.Name.ToString().Equals("rdoBadChannel") && (rbClick.IsEnabled == true)) GetBadChannel(_sTrayNo, _sCurrOper, _sCurrOperDetlTypeCD);
                if (rbClick.Name.ToString().Equals("rdoIrOcvJudg") && (rbClick.IsEnabled == true)) GetDataQuery(IROCV_JUDG);
                if (rbClick.Name.ToString().Equals("rdoIrOcvIR") && (rbClick.IsEnabled == true)) GetDataQuery(IROCV_IMP);
                if (rbClick.Name.ToString().Equals("rdoIrOcvV") && (rbClick.IsEnabled == true)) GetDataQuery(IROCV_VOLT);
                if (rbClick.Name.ToString().Equals("rdoVisionGrade") && (rbClick.IsEnabled == true)) GetDataQuery(VISION_GRADER);
                if (rbClick.Name.ToString().Equals("rdoODGrade") && (rbClick.IsEnabled == true)) GetDataQuery(OD_GRADE);
                if (rbClick.Name.ToString().Equals("rdoODYN") && (rbClick.IsEnabled == true)) GetDataQuery(OD_YN);

                if (rbClick.Name.ToString().Equals("rdoAvgVolt") && (rbClick.IsEnabled == true)) GetDataQuery(VOLT_AVG_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoFitCapa") && (rbClick.IsEnabled == true)) GetDataQuery(FITTEDCAPACITY_DATATYPE);
                if (rbClick.Name.ToString().Equals("rdoXgrade") && (rbClick.IsEnabled == true)) GetDataQuery(XGRADE);
                if (rbClick.Name.ToString().Equals("rdoDQDVAVG") && (rbClick.IsEnabled == true)) GetDataQuery(DQDV_AVG);
                if (rbClick.Name.ToString().Equals("rdoWndLot") && (rbClick.IsEnabled == true)) GetDataQuery(WND_LOT);
            }
        }

        private void txtROUTID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //OpenRouteForm(_sTrayLine, _sModelID, txtRouteID.Text);
        }

        private void txtRange_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
                SetCellRange();
            }
        }

        #region [스프레드 이벤트]
        private void dgProcess_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////

                if (!FinCheck)
                {
                    //TH_TRAY_PROCESS_IRR_HIST (=>WipHistory) 데이터 일 경우
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CURR_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }

                    //비정상진행공정일 경우(수동 공정변경 등)
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "IRR_HIST_YN")).Equals("Y"))
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);

                    //특성공정작업 예상시간
                    if (e.Cell.Row.Index == dataGrid.Rows.Count - 1
                    && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PROCID")).Equals("000"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TIME_OVER_YN")).Equals("Y"))
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        else
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
                else
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                }
            }));
        }

        private void dgProcess_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgProcess.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;

            if (e.Row.Index == dataGrid.Rows.Count - 1 &&
                Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[dataGrid.Rows.Count - 1].DataItem, "PROCID")).Equals("000"))
            {
                tb.Text = ObjectDic.Instance.GetObjectName("FORECAST");
                dataGrid.Rows[e.Row.Index].HeaderPresenter.Content = tb;
            }
        }

        private void dgProcess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    int row = cell.Row.Index;
                    _sCurrOper = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID"));
                    _sCurrOperGrpID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROC_GR_CODE"));
                    _sCurrOperDetlTypeCD = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROC_DETL_TYPE_CD"));
                    _sRouteID = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "ROUTID"));
                    _sCurrYN = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "CURR_YN"));

                    if (_sCurrOper.Equals("000") || string.IsNullOrEmpty(_sCurrOper)) return;
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ST"))))
                    {
                        _sOPStartTime = "";
                    }
                    else
                    {
                        _sOPStartTime = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ST").ToString());
                        DateTime dateOP = DateTime.Parse(_sOPStartTime);
                        _sOPStartTime = dateOP.ToString("yyyyMMddHHmmss");
                    }

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED"))))
                    {
                        _sOPEndTime = "";
                    }
                    else
                    {
                        _sOPEndTime = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED").ToString());
                        DateTime dateOP = DateTime.Parse(_sOPEndTime).AddSeconds(1); // 00:00:00.XXX 초 단위가 같은경우 조회안됨 보정
                        _sOPEndTime = dateOP.ToString("yyyyMMddHHmmss");
                    }
                    SetRadioButton(_sCurrOperDetlTypeCD, row);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProcess_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                if (pnt == null) return;

                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);
                if (cell == null) return;
                C1.WPF.DataGrid.DataGridRow gridRow = cell.Row;

                if (gridRow != null)
                {
                    int row = cell.Row.Index;
                    bool bPossEnd = false;

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ST"))))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED"))))
                    {
                        bPossEnd = true;
                    }
                    else
                    {
                        //이력의 마지막 공정이 종료상태인데 TRAY 공정상태는 작업중일 때 공정 종료 가능하도록 함
                        if (iLastRow != -1 && row == iLastRow
                            && !string.IsNullOrEmpty(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "WIPDTTM_ED").ToString())
                            && txtTrayOpStatus.Tag.ToString().Equals("S"))
                            bPossEnd = true;
                    }

                    if (bPossEnd.Equals(false))
                    {
                        return;
                    }

                    if (bPossEnd.Equals(true))
                    {
                        //진행중인 공정을 종료하시겠습니까?
                        Util.MessageConfirm("FM_ME_0236", result =>
                        {
                            if (bPossEnd && result == MessageBoxResult.OK)
                            {
                                string CurrOP = string.Empty;
                                CurrOP = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID").ToString();

                                DataTable inDataTable = new DataTable();
                                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                                inDataTable.Columns.Add("IFMODE", typeof(string));
                                inDataTable.Columns.Add("AREAID", typeof(string)); //2021-05-10 AREAID 추가
                                inDataTable.Columns.Add("LOTID", typeof(string));
                                inDataTable.Columns.Add("PROCID", typeof(string));
                                inDataTable.Columns.Add("CURROP", typeof(string));
                                inDataTable.Columns.Add("ACTUSER", typeof(string));

                                DataRow dr = inDataTable.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["LOTID"] = _sTrayNo;
                                dr["PROCID"] = _sCurrOper;
                                dr["CURROP"] = _sCurrOperGrpID;
                                dr["ACTUSER"] = LoginInfo.USERID;
                                inDataTable.Rows.Add(dr);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_INFO_OP_END_MB", "INDATA", "OUTDATA", inDataTable);
                                if (dtRslt.Rows.Count == 0)
                                {
                                    Util.Alert("FM_ME_0112");  //공정종료에 실패하였습니다.
                                    return;
                                }
                                Util.Alert("FM_ME_0111");  //공정종료를 완료하였습니다.
                            }
                            GetTrayInfo();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDOCV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int row = cell.Row.Index;
                _sCurrOper = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROCID").ToString();
                _sCurrOperGrpID = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROC_GR_ID").ToString();
                _sCurrOperDetlTypeCD = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PROC_DETL_TYPE_CD").ToString();
                _sRouteID = DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "ROUTID").ToString();
                SetRadioButton(_sCurrOperDetlTypeCD, dgDOCV.CurrentRow.Index);
            }
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string[] NotUseRow = _sNotUseRowLIst.Split(',');
                    string[] NotUseCol = _sNotUseColLIst.Split(',');

                    if (NotUseRow.Contains(e.Cell.Row.Index.ToString()) && NotUseCol.Contains(e.Cell.Column.Index.ToString()))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                        DataTableConverter.SetValue(dgCell.Rows[e.Cell.Row.Index].DataItem, dgCell.Columns[e.Cell.Column.Index].Name, "NOT_USE");
                    }

                    // HIST 관련 쿼리 수정 전 임시 처리
                    if (dtWipCell.Rows.Count > 0 && chkHist.IsChecked == false)
                    {
                        string sCellId = Util.NVC(dtCellList.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString());
                        if (!string.IsNullOrEmpty(sCellId))
                        {
                            DataRow[] drSelect = dtWipCell.Select("SUBLOTID = '" + sCellId + "'");

                            if (drSelect.Length > 0)
                            {
                                if (drSelect[0]["SPLT_FLAG"].ToString() == "Y")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                }
                            }
                        }
                    }

                    // 발열셀 위치 표시용
                    if (dtFDSList.Rows.Count > 0)
                    {

                        string sCellId = Util.NVC(dtCellList.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString());
                        if (!string.IsNullOrEmpty(sCellId))
                        {
                            DataRow[] drSelect = dtFDSList.Select("SUBLOTID = '" + sCellId + "'");

                            if (drSelect.Length > 0)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        }

                    }


                    if (dtColorSet.Rows.Count > 0)
                    {
                        string sCellId = Util.NVC(dtCellList.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString());
                        if (!string.IsNullOrEmpty(sCellId))
                        {
                            DataRow[] drSelect = dtColorSet.Select("SUBLOTID = '" + sCellId + "'");
                            if (drSelect.Length > 0)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Tomato);
                            }
                        }
                    }

                    if (dtColorRange.Rows.Count > 0)
                    {
                        string sCellId = Util.NVC(dtCellList.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString());
                        if (!string.IsNullOrEmpty(sCellId))
                        {
                            DataRow[] drSelect = dtColorRange.Select("SUBLOTID = '" + sCellId + "'");
                            if (drSelect.Length > 0)
                            {
                                string scolor = drSelect[0]["COLOR"].ToString();
                                Brush color = (SolidColorBrush)new BrushConverter().ConvertFrom(scolor);
                                e.Cell.Presenter.Background = color;
                            }
                        }
                    }
                    if (dtSublotList.Rows.Count > 0)
                    {
                        string sCellId = Util.NVC(dtCellList.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString());
                        if (!string.IsNullOrEmpty(sCellId))
                        {
                            DataRow[] drSelect = dtSublotList.Select("SUBLOTID = '" + sCellId + "'");
                            if (drSelect.Length > 0)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            }
                        }
                    }
                }

                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength((dgCell.ActualWidth / dgCell.Columns.Count) - 2);
                if (chkSize.IsChecked == true)
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                }


                //if (dtSublotList.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtSublotList.Rows.Count; i++)
                //    {
                //        if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name)).Equals(Util.NVC(dtSublotList.Rows[i]["SUBLOTID"])))
                //        {
                //            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Tomato);
                //        }
                //    }
                //}

                //if (dtValueSublot.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtValueSublot.Rows.Count; i++)
                //    {
                //        if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name)).Equals(Util.NVC(dtValueSublot.Rows[i]["SUBLOTID"])))
                //        {
                //            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Tomato);
                //        }
                //    }
                //}
            }));
        }

        private void dgCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Value.ToString() != "X" && rdoGrade.IsChecked == true)
                {
                    //Open Cell Form
                    string sCellId = Util.NVC(dtCellList.Rows[cell.Row.Index][cell.Column.Index].ToString());
                    if (!string.IsNullOrEmpty(sCellId) && !sCellId.Equals("NOT_USE"))
                    {
                        FCS002_022 fcs022 = new FCS002_022();
                        fcs022.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(sCellId);
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenuFORM("FCS002_022", "FCS002_022", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);

                    }

                }




                else if (string.IsNullOrEmpty(cell.Value.ToString()) || cell.Value.ToString() == "X")
                    return;

                else if (!string.IsNullOrEmpty(DataTableConverter.GetValue(datagrid.Rows[cell.Row.Index].DataItem, cell.Column.Name).ToString())
                    || !string.IsNullOrEmpty(Util.NVC(datagrid.GetCell(cell.Row.Index, cell.Column.Index).Presenter.Tag.ToString())))
                {
                    //Open Cell Form
                    string sCellId = Util.NVC(dtCellList.Rows[cell.Row.Index][cell.Column.Index].ToString());
                    if (!string.IsNullOrEmpty(sCellId) && !sCellId.Equals("NOT_USE"))
                    {
                        FCS002_022 fcs022 = new FCS002_022();
                        fcs022.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(sCellId);
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenuFORM("FCS002_022", "FCS002_022", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);
                    }
                }
            }
        }

        private void txtROUTID_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtROUTID.Text)))
                {
                    string sModel = Util.NVC(_sModelID);
                    string sRoute = Util.NVC(txtROUTID.Text);

                    // 2023.11.08 수정 작업조건 Report →  Route 정보 조회(소형)
                    //// 프로그램 ID 확인 후 수정
                    //object[] parameters = new object[2];
                    //parameters[0] = sModel;
                    //parameters[1] = sRoute;
                    //this.FrameOperation.OpenMenu("SFU010730560", true, parameters);

                    string sLine = Util.NVC(txtTrayLine.Text);

                    // 프로그램 ID 확인 후 수정
                    object[] parameters = new object[3];
                    parameters[0] = sModel;
                    parameters[1] = sRoute;
                    parameters[2] = sLine;
                    this.FrameOperation.OpenMenu("SFU010745110", true, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion


        #region [버튼 이벤트]
        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
            //확인 메세지 팝업

            FCS002_021_SPECIAL_MANAGEMENT specialManagement = new FCS002_021_SPECIAL_MANAGEMENT();
            specialManagement.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = Util.NVC(txtTrayID.Text.Trim());
            parameters[1] = null; //적재된 Tray 특별관리 진행

            C1WindowExtension.SetParameters(specialManagement, parameters);
            specialManagement.Closed += new EventHandler(specialManagement_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => specialManagement.ShowModal()));
            specialManagement.BringToFront();
        }

        private void specialManagement_Closed(object sender, EventArgs e)
        {
            FCS002_021_SPECIAL_MANAGEMENT window = sender as FCS002_021_SPECIAL_MANAGEMENT;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSampleOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));



                if (_AgingOutPriority == 7)
                {
                    string TrayList = string.Empty;

                    DataTable inDataTable1 = new DataTable();
                    inDataTable1.Columns.Add("LOTID", typeof(string));
                    inDataTable1.Columns.Add("PROCID", typeof(string));

                    DataRow dr1 = inDataTable1.NewRow();
                    dr1["LOTID"] = _sTrayNo;
                    dr1["PROCID"] = _sCurrOper;
                    inDataTable1.Rows.Add(dr1);

                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INSIDE_EQP", "INDATA", "OUTDATA", inDataTable1);

                    if (dtRslt1.Rows.Count == 0) { Util.Alert("FM_ME_0549"); } //취소불가능한 tray가 있습니다.
                    else
                    {
                        for (int i = 0; i < dtRslt1.Rows.Count; i++)
                        {
                            DataRow dr = inDataTable.NewRow();
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["LOTID"] = dtRslt1.Rows[i]["LOTID"].ToString();
                            dr["USERID"] = LoginInfo.USERID;
                            inDataTable.Rows.Add(dr);

                            if (!TrayList.IsNullOrEmpty())
                            {
                                TrayList += "," + dtRslt1.Rows[i]["CSTID"].ToString();
                            }
                            else
                            {
                                TrayList = dtRslt1.Rows[i]["CSTID"].ToString();
                            }
                        }

                        if (!TrayList.IsNullOrEmpty())
                        {
                            DataTable dtInData1 = new DataTable();
                            dtInData1.Columns.Add("CSTID", typeof(string));


                            DataRow drIn1 = dtInData1.NewRow();
                            drIn1["CSTID"] = TrayList;

                            dtInData1.Rows.Add(drIn1);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SAMPLE_OUT_CHK_MB", "RQSTDT", "RSLTDT", dtInData1);

                            if (dtRslt.Rows.Count > 0)
                            {
                                Util.Alert("FM_ME_0549"); // Sample 출고 취소가 불가능한 Tray가 있습니다.
                                return;
                            }
                        }
                    }




                    Util.MessageConfirm("FM_ME_0009", (result) =>  //[Tray ID : {0}]를 Sample 해제하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_SAMPLE_IN_MB", "INDATA", "OUTDATA", inDataTable);

                                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0067");  //Sample 해제를 완료하였습니다.
                                }
                                else
                                {
                                    Util.Alert("ME_0068");  //Sample 해제에 실패하였습니다.
                                }
                                ClearControlValue();
                                GetTrayInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                    }, new string[] { _sTrayID });
                }
                else
                {
                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["LOTID"] = _sTrayNo;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    Util.MessageConfirm("FM_ME_0008", (result) =>  //[Tray ID : {0}]를 Sample 출고 하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            inDataSet.Tables.Add(inDataTable);
                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_SAMPLE_OUT_MB", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", inDataSet);
                            if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.MessageInfo("FM_ME_0065");  //Sample 출고 지시를 완료하였습니다.
                                ClearControlValue();
                                GetTrayInfo();
                                if (dsRslt.Tables["OUT_SAMPLE_PORT"].Rows.Count > 0)
                                {
                                    //TSK_116 tsk116 = new TSK_116();
                                    //tsk116.OUTPORT = dsRslt.Tables["OUT_SAMPLE_PORT"];

                                    //if (tsk116.ShowDialog(this) == DialogResult.Yes)
                                    //{
                                    //}
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0066");
                                }
                            }
                        }
                    }, new string[] { _sTrayID });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnForceOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_dAgingISSDTTM.ToString()))
                {
                    // 출고예정시간이 없는경우 강제출고가 불가합니다.
                    Util.Alert("FM_ME_0506");
                    return;
                }

                //else if (_dAgingISSDTTM < DateTime.Today)
                //{
                //    //출고예정시간이 미달인 경우 강제출고가 불가합니다.
                //    Util.Alert("FM_ME_0507");
                //    return;
                //}


                Util.MessageConfirm("FM_ME_0094", result => //강제출고요청을 하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel) return;

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LOTID"] = _sTrayNo;
                    dr["PROCID"] = _sCurrOper;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INSIDE_EQP", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows.Count == 0) { Util.Alert("FM_ME_0492"); } //Lot이 착공상태가 아니므로 강제출고 할 수 없습니다.
                    else
                    {
                        string sTray = string.Empty;
                        foreach (DataRow drTray in dtRslt.Rows)
                        {
                            sTray += drTray["CSTID"].ToString() + " ";
                        }

                        Util.MessageConfirm("FM_ME_0010", result2 => //[Tray ID : {0}]를 강제출고하시겠습니까?
                        {
                            if (result2 != MessageBoxResult.OK) return;

                            DataSet inDataSet = new DataSet();
                            DataTable inData = new DataTable();
                            inData.TableName = "INDATA";
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("IFMODE", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("LANGID", typeof(string));
                            inData.Columns.Add("AREAID", typeof(string));
                            DataRow dr2 = inData.NewRow();
                            dr2["SRCTYPE"] = "UI";
                            dr2["IFMODE"] = "OFF";
                            dr2["USERID"] = LoginInfo.USERID;
                            dr2["LANGID"] = LoginInfo.LANGID;
                            dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                            inData.Rows.Add(dr2);

                            DataTable inLot = new DataTable();
                            inLot.TableName = "INLOT";
                            inLot.Columns.Add("LOTID", typeof(string));
                            foreach (DataRow drTray in dtRslt.Rows)
                            {
                                DataRow lotRow = inLot.NewRow();
                                lotRow["LOTID"] = drTray["LOTID"];
                                inLot.Rows.Add(lotRow);
                            }
                            inDataSet.Tables.Add(inData);
                            inDataSet.Tables.Add(inLot);

                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_TRAY_FORCE_OUT_MULTI_MB", "INDATA,INLOT", "OUTDATA", inDataSet);

                            if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.MessageInfo("FM_ME_0092"); //강제출고 요청을 완료하였습니다.
                            }
                            else
                            {
                                Util.Alert("FM_ME_0091"); //강제출고 요청에 실패하였습니다.
                            }
                            GetTrayInfo();
                        }, new string[] { sTray });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtROUTID.Text))
            {
                Util.Alert("FM_ME_0100"); //공정경로 정보가 존재하지않습니다.
                return;
            }
            FCS002_021_SEL_JUDG_OP wndPopup = new FCS002_021_SEL_JUDG_OP();
            wndPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = txtTrayNo.Text;
            parameters[1] = txtROUTID.Text;

            C1WindowExtension.SetParameters(wndPopup, parameters);
            wndPopup.Closed += new EventHandler(wndPopup_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            wndPopup.BringToFront();
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_021_SEL_JUDG_OP window = sender as FCS002_021_SEL_JUDG_OP;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        //필요 시 추후 개발 기준정보에서 조회 기능
        private void btnRecipe_Click(object sender, RoutedEventArgs e)
        {


        }

        //필요 시 추후 개발 기준정보에서 조회 기능
        private void recipe_Closed(object sender, EventArgs e)
        {

        }

        private void btnKVAL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0468", result => //KValue재계산하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel) return;

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("CSTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["CSTID"] = _sTrayID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_KVALUE_CALC_MB", "INDATA", "OUTDATA", inDataTable);

                    if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        Util.MessageInfo("FM_ME_0466"); //K-Value OCV 계산 처리를 하였습니다.
                    }
                    else
                    {
                        Util.Alert("FM_ME_0467"); //K-Value OCV 계산 처리를 실패하였습니다.
                    }
                    GetTrayInfo();

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnGrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS002_021_GRADE_DISTRIBUTION grade_distribution = new FCS002_021_GRADE_DISTRIBUTION();
                grade_distribution.FrameOperation = FrameOperation;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(txtTrayID.Text.Trim());
                parameters[1] = Util.NVC(txtTrayNo.Text.Trim());

                C1WindowExtension.SetParameters(grade_distribution, parameters);
                grade_distribution.Closed += new EventHandler(grade_distribution_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => grade_distribution.ShowModal()));
                grade_distribution.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void grade_distribution_Closed(object sender, EventArgs e)
        {
            FCS002_021_GRADE_DISTRIBUTION window = sender as FCS002_021_GRADE_DISTRIBUTION;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            FCS002_024 FCS002_024 = new FCS002_024();
            FCS002_024.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = txtTrayID.Text;
            parameters[1] = txtTrayNo.Text;
            parameters[2] = null; // FINCD
            parameters[3] = "Y"; // ACT_YN
            this.FrameOperation.OpenMenuFORM("FCS002_024", "FCS002_024", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray별 Cell Data"), true, parameters);
        }

        private void btnDOCV_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID)) return;

            Util.MessageConfirm("FM_ME_0030", result => //Delta OCV를 계산하시겠습니까?
            {
                if (result != MessageBoxResult.OK) return;
                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("CSTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["CSTID"] = _sTrayID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_DOCV_CALC_MB", "INDATA", "OUTDATA", inDataTable);
                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        Util.MessageInfo("FM_ME_0219");  //정상적으로 계산하였습니다.
                    else
                        Util.Alert("FM_ME_0096");  //계산중 오류가 발생하였습니다.
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void btnALTVAL_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID)) return;

            Util.MessageConfirm("FM_ME_0546", result => //대체용량
            {
                if (result != MessageBoxResult.OK) return;
                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("CSTID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["CSTID"] = _sTrayID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_ALT_CAPA_CALC_MB", "INDATA", "OUTDATA", inDataTable);
                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        Util.MessageInfo("FM_ME_0219");  //정상적으로 계산하였습니다.
                    else
                        Util.Alert("FM_ME_0096");  //계산중 오류가 발생하였습니다.
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void btnChgHist_Click(object sender, RoutedEventArgs e)
        {

            FCS002_021_CHANGE_HIST wndRunStart = new FCS002_021_CHANGE_HIST();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _sTrayNo;

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
            }
        }

        private void btnCellJudge_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTrayNo.Text))
            {
                return;
            }
            FCS002_021_REL_JUDG wndPopup = new FCS002_021_REL_JUDG();
            wndPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = txtTrayID.Text;
            parameters[1] = txtTrayNo.Text;

            C1WindowExtension.SetParameters(wndPopup, parameters);
            wndPopup.Closed += new EventHandler(RelJudg_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            wndPopup.BringToFront();
        }

        private void RelJudg_Closed(object sender, EventArgs e)
        {
            FCS002_021_REL_JUDG window = sender as FCS002_021_REL_JUDG;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnAging_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTrayNo.Text)) return;

            string endTime = string.Empty;
            for (int i = dgProcess.GetRowCount() - 1; i >= 0; i--)
            {
                if (DataTableConverter.GetValue(dgProcess.Rows[i].DataItem, "CURR_YN").ToString().Equals("Y")
                    && DataTableConverter.GetValue(dgProcess.Rows[i].DataItem, "PROCID").ToString().Equals(txtOper.Tag.ToString()))
                {
                    //시작 공정일 때
                    if (dgProcess.GetRowCount() == 1) endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    else endTime = Util.NVC(DataTableConverter.GetValue(dgProcess.Rows[i - 1].DataItem, "WIPDTTM_ED"));
                }
            }

            FCS002_021_AGING_CARRY_IN AgingPopup = new FCS002_021_AGING_CARRY_IN();
            AgingPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = txtTrayNo.Text;
            parameters[1] = txtTrayID.Text;
            parameters[2] = endTime;

            C1WindowExtension.SetParameters(AgingPopup, parameters);
            AgingPopup.Closed += new EventHandler(AgingPopup_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => AgingPopup.ShowModal()));
            AgingPopup.BringToFront();
        }

        private void AgingPopup_Closed(object sender, EventArgs e)
        {
            FCS002_021_AGING_CARRY_IN window = sender as FCS002_021_AGING_CARRY_IN;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                ClearControlValue();
                GetTrayInfo();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnChgDfctLmtOverFlag_Click(object sender, RoutedEventArgs e)
        {
            //확인 메세지 팝업

            try
            {
                if (string.IsNullOrWhiteSpace(_sTrayNo)) return;

                Util.MessageConfirm("FM_ME_0079", (result) =>  //Tray 정보를 변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));
                        inDataTable.Columns.Add("UPDDTTM", typeof(string));
                        //inDataTable.Columns.Add("DFCT_OCCR_EQPTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = _sTrayNo;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); ;
                        //dr["DFCT_OCCR_EQPTID"] = "UI";
                        inDataTable.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TRAY_DFCT_LIMIT_OVER_FLAG_MB", "RQSTDT", "RSLTDT", inDataTable);

                        Util.MessageInfo("FM_ME_0074"); //Tray 정보 변경을 완료하였습니다.
                        ClearControlValue();
                        GetTrayInfo();
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                Util.MessageException(ex);
            }
        }

        private void btnRlsTrfRsnCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_sTrayID)) return;

                Util.MessageConfirm("FM_ME_0079", (result) =>  //Tray 정보를 변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("TRF_RSN_CODE", typeof(string));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));
                        inDataTable.Columns.Add("UPDDTTM", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow dr = inDataTable.NewRow();
                        dr["TRF_RSN_CODE"] = "N";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        dr["CSTID"] = _sTrayID;
                        inDataTable.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TRAY_ABNORM_TRF_RSN_CODE_MB", "RQSTDT", "RSLTDT", inDataTable);

                        Util.MessageInfo("FM_ME_0074"); //Tray 정보 변경을 완료하였습니다.
                        ClearControlValue();
                        GetTrayInfo();
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
                Util.MessageException(ex);
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID) || string.IsNullOrEmpty(_sTrayNo)) return;

            Util.MessageConfirm("FM_ME_0234", result => //종료하시겠습니까?
            {
                if (result == MessageBoxResult.No) return;
                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("TRAY_ID", typeof(string));
                    inDataTable.Columns.Add("TRAY_NO", typeof(string));
                    inDataTable.Columns.Add("WIPSTAT", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["TRAY_ID"] = _sTrayID;
                    dr["TRAY_NO"] = _sTrayNo;
                    dr["WIPSTAT"] = _sFinCD;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_MANUAL_HISTORY_MB", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        Util.Alert("ME_0221");  //정상종료하였습니다.
                        ClearControlValue();

                        _sFinCD = "H";
                        GetTrayInfo();
                    }
                    else
                        Util.Alert("ME_0220");  //정상종료 요청에 실패하였습니다.
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnWGradeJudge_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0087", result =>
            {
                if (result == MessageBoxResult.Cancel) return;

                try
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    inDataTable.Columns.Add("MANUAL_YN", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LOTID"] = _sTrayNo;
                    dr["MANUAL_YN"] = (bool)rdoManual.IsChecked ? "Y" : (bool)rdoAuto.IsChecked ? "N" : null;
                    //  dr["LOT_NO"] = _sLotID;
                    dr["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_W_MANUAL_LOT_MB", "INDATA", "OUTDATA", inDataTable);

                    if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
                    {
                        Util.MessageInfo("FM_ME_0086"); //W등급 재판정을 완료하였습니다.
                        ClearControlValue();
                        GetTrayInfo();
                    }
                    else
                    {
                        Util.Alert("FM_ME_0085");  //W등급 재판정에 실패하였습니다.
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnSetRange_Click(object sender, RoutedEventArgs e)
        {
            SetRange();
        }

        private void btnSetCell_Click(object sender, RoutedEventArgs e)
        {
            SetCellRange();
        }

        private void dgCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void btnResetS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 다음공정이 LCI가 아니면 
                //if (txtNextOp.Tag.ToString() != "FF1A01")
                {
                    //        Util.MessageInfo("FM_ME_0503");  
                    //    return;
                }
                //DataTable _TempT = null;

                //if (!string.IsNullOrEmpty(_sTrayID))
                //{
                //    rdoGrade.IsChecked = true;

                //    GetDataQuery("");

                //    _TempT = dtWipCell;

                // S와 X가 아닌것이 있으면 RETURN
                //for (int i = 0; i < _TempT.Rows.Count; i++)
                //    if (!(_TempT.Rows[i]["VALUE"].ToString() == "S" || _TempT.Rows[i]["VALUE"].ToString() == "X"))
                //    {
                //        Util.MessageInfo("FM_ME_0501");  

                //        return;
                //    }
                // 전체가 S이면 
                //    Util.MessageConfirm("FM_ME_0502", (result) => 
                //    {
                //        if (result != MessageBoxResult.OK)
                //        {
                //            return;
                //        }
                //        else
                //        {
                //            DataTable dtRqst = new DataTable();
                //            dtRqst.TableName = "RQSTDT";
                //            dtRqst.Columns.Add("SUBLOTID", typeof(string));
                //            dtRqst.Columns.Add("SUBLOTJUDGE", typeof(string));
                //            dtRqst.Columns.Add("USERID", typeof(string));

                //            for (int i = 0; i < _TempT.Rows.Count; i++)
                //            {
                //                DataRow dr = dtRqst.NewRow();
                //                // X cell 건너뛰기
                //                if (_TempT.Rows[i]["SUBLOTID"].ToString() == "0000000000")
                //                    continue;
                //                dr["SUBLOTID"] = _TempT.Rows[i]["SUBLOTID"].ToString();
                //                dr["SUBLOTJUDGE"] = "Z";
                //                dr["USERID"] = LoginInfo.USERID;

                //                if (string.IsNullOrEmpty(dr["SUBLOTJUDGE"].ToString()))
                //                    return;

                //                dtRqst.Rows.Add(dr);

                //            }

                //            //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CELL_GRADE_CHANGE_MB", "RQSTDT", "RSLTDT", dtRqst);

                //            Util.MessageInfo("FM_ME_0136"); //변경완료하였습니다.



                //        }
                //    });


                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoTemp_Unchecked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sTrayID))
            {
                InitializeDataGrid(_sFrstTrayID, dgCell);
            }
            else
            {
                InitializeDataGrid(_sTrayID, dgCell);
            }
        }

        private void chkSize_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_sTrayID) || !string.IsNullOrEmpty(_sFrstTrayID))
            {
                rdo_Click(_object, new RoutedEventArgs());
            }
        }

        private void chkSize_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_sTrayID) || !string.IsNullOrEmpty(_sFrstTrayID))
            {
                rdo_Click(_object, new RoutedEventArgs());
            }
        }

        private void btnTrayLoc_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("FROM_DATE", typeof(string));
            dtRqst.Columns.Add("TO_DATE", typeof(string));
            dtRqst.Columns.Add("CMCDTYPE", typeof(string));
            dtRqst.Columns.Add("CSTID", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CSTSTAT";
            dr["CSTID"] = Util.GetCondition(txtTrayID, sMsg: "FM_ME_0070"); //Tray ID를 입력해주세요.
            dr["USE_FLAG"] = "Y";
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_SEL_TRAY_POSITION_NOW_INFO_MB", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (result.Rows.Count > 0)
                {

                    //Tray ID: {0} \r\n 현재위치 : {1}
                    Util.MessageInfo("FM_ME_0516", new string[] { txtTrayID.Text, result.Rows[0]["EQPTNAME"].ToString() });


                }
                else
                {
                    Util.MessageInfo("FM_ME_0517");


                }


            });



        }

        private string GetCellType()
        {
            //DA_BAS_SEL_AREA_COM_CODE_USE

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_SPCL_FN_MB";
            dr["COM_CODE"] = "FORM_CELLID";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count == 0)
                _CellType = "D";
            else
                _CellType = dtRslt.Rows[0]["ATTR1"].ToString();


            return _CellType;
        }

        private void chkColor_Checked(object sender, RoutedEventArgs e)
        {
            txtRange1.Text = string.Empty;
            txtRange2.Text = string.Empty;
            // Tray 정보조회 색조
            GetColor();
        }

        private void GetColor()
        {
            // Tray 정보조회 색조

            dtValueSublot.Rows.Clear();
            dtColorSet.Rows.Clear();
            dtColorRange.Rows.Clear();

            DataTable _TempT = DataTableConverter.Convert(dgCell.ItemsSource);

            double min = 0;
            bool minCk = false;
            double max = 0;

            Double dVal = 0;

            // Mix/ Max 구하기
            for (int i = 0; i < dgCell.Rows.Count; i++)
                for (int j = 0; j < dgCell.Columns.Count; j++)
                {
                    string sVal = _TempT.Rows[i][j].ToString();


                    if (!string.IsNullOrEmpty(sVal) && !Double.TryParse(sVal, out dVal))
                        return;

                    if (minCk == false)
                    {
                        min = dVal;
                        minCk = true;
                    }
                    if (dVal < min)
                        min = dVal;
                    if (dVal > max)
                        max = dVal;
                }

            double range = max - min;

            bsetRange = false;
            bsetColorRange = true;

            for (int j = 0; j < dgCell.Columns.Count; j++)
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    if (_TempT.Rows[i][j].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                    {
                        continue;
                    }

                    string value = Util.NVC(dgCell.GetCell(i, j).Text).ToUpper();
                    string sCellId = Util.NVC(dtCellList.Rows[i][j].ToString());

                    if (string.IsNullOrEmpty(dgCell[i, j].Value.ToString()))
                        continue;
                    double Val = Double.Parse(dgCell[i, j].Value.ToString());
                    double gb = (max - Val) / range * 255;
                    Brush cellColor = new SolidColorBrush(Color.FromRgb(255, (byte)gb, (byte)gb));


                    DataRow dr = dtValueSublot.NewRow();
                    dr["SUBLOTID"] = Util.NVC(value);
                    dtValueSublot.Rows.Add(dr);

                    DataRow drColor = dtColorRange.NewRow();
                    drColor["SUBLOTID"] = Util.NVC(sCellId);
                    drColor["COLOR"] = cellColor;
                    dtColorRange.Rows.Add(drColor);


                }
            }

            Util.GridSetData(dgCell, _TempT, this.FrameOperation);
        }


        private void chkColor_Unchecked(object sender, RoutedEventArgs e)
        {

            dtColorRange.Rows.Clear();
        }

        private void btnForcOutReset_Click(object sender, RoutedEventArgs e)
        {


            //상태를 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("FORM_FORC_MANL_PORT_CODE", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("UPDUSER", typeof(string));
                        dtRqst.Columns.Add("UPDDTTM", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = Util.NVC(txtTrayNo.Text);
                        dr["CSTID"] = Util.NVC(txtTrayID.Text);
                        dr["FORM_FORC_MANL_PORT_CODE"] = "INIT";
                        // dr["EQPTID"] = ""; // Biz 정리후 테스트 필요
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_FORM_SET_FORM_FORC_MANL_PORT_CODE_MB", "INDATA", null, dtRqst);


                        Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                        GetTrayInfo();

                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });



        }

        private void btnRelFDS_Click(object sender, RoutedEventArgs e)
        {
            DataTable ChkFDS = ChkFDSCell();

            if (ChkFDS.Rows.Count == 0)
            {
                //SAMPLE 등록 가능한 발열셀이 없습니다.
                Util.MessageInfo("FM_ME_0551");
                return;
            }



            // 현재 TRAY의 발열셀을 SAMPLE등록하시겠습니까?
            Util.MessageConfirm("FM_ME_0550", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    string BizRuleID = "BR_SET_FDS_CELL_SMPL_MB";

                    DataTable dtIndata = new DataTable();
                    dtIndata.Columns.Add("LOTID", typeof(string));
                    dtIndata.Columns.Add("USERID", typeof(string));
                    dtIndata.Columns.Add("TD_FLAG", typeof(string));
                    dtIndata.Columns.Add("SPLT_FLAG", typeof(string));



                    DataRow InRow = dtIndata.NewRow();
                    InRow["LOTID"] = _sTrayNo;
                    InRow["USERID"] = LoginInfo.USERID;
                    InRow["TD_FLAG"] = "P"; // 샘플유형 -  생산 
                    InRow["SPLT_FLAG"] = "Y"; // 

                    dtIndata.Rows.Add(InRow);

                    new ClientProxy().ExecuteService(BizRuleID, "INDATA", "OUTDATA", dtIndata, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //처리가 완료되었습니다.
                                Util.MessageInfo("FM_ME_0239");
                            }
                            else if (bizResult.Rows[0]["RETVAL"].ToString().Equals("1"))
                            {
                                //SAMPLE 등록 가능한 발열셀이 없습니다.
                                Util.MessageInfo("FM_ME_0551");
                            }
                            else if (bizResult.Rows[0]["RETVAL"].ToString().Equals("2"))
                            {
                                //SAMPLE 등록 중 오류가 발생하였습니다.
                                Util.MessageInfo("FM_ME_0552");
                            }
                            else
                            {
                                //저장실패하였습니다.
                                Util.MessageInfo("FM_ME_0213");
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            GetTrayInfo();
                        }
                    });
                }
            });
        }


        private void btnCellSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkHist.IsChecked == true)
                {
                    Util.Alert("FM_ME_0591");  // 이력 조회시에는 Sample 처리할 수 없습니다.
                    return;
                }
                FCS002_021_SAMPLE spSample = new FCS002_021_SAMPLE();
                spSample.FrameOperation = FrameOperation;

                object[] Parameters = new object[2];
                Parameters[0] = _sTrayID;
                Parameters[1] = dtWipCell;

                C1WindowExtension.SetParameters(spSample, Parameters);

                spSample.Closed += new EventHandler(spSample_Closed);
                spSample.ShowModal();
                spSample.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void spSample_Closed(object sender, EventArgs e)
        {
            FCS002_021_SAMPLE runStartWindow = sender as FCS002_021_SAMPLE;

            runStartWindow.Closed -= new EventHandler(spSample_Closed);

            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                GetTrayInfo();
            }
        }


        #endregion

        //private void btnCrackChg_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string sCrackState = txtCrackState.Text;


        //        if (string.IsNullOrWhiteSpace(_sTrayNo)) return;

        //        if (sCrackState.Equals("N") || string.IsNullOrWhiteSpace(sCrackState))
        //        {
        //            Util.MessageInfo("FM_ME_0217");     // 정보 변경 가능한 Tray가 아닙니다.
        //            return;
        //        }


        //        Util.MessageConfirm("FM_ME_0079", (result) =>  //Tray 정보를 변경하시겠습니까?
        //        {

        //            if (result == MessageBoxResult.OK)
        //            {
        //                DataTable inDataTable = new DataTable();
        //                inDataTable.Columns.Add("LOTID", typeof(string));
        //                inDataTable.Columns.Add("FLAG", typeof(string));
        //                inDataTable.Columns.Add("UPDUSER", typeof(string));
        //                inDataTable.Columns.Add("UPDDTTM", typeof(string));

        //                DataRow dr = inDataTable.NewRow();
        //                dr["LOTID"] = _sTrayNo;
        //                dr["FLAG"] = (sCrackState.Equals("Y")) ? "C" : "N";   // 'Y'인 경우 현재 위치에서 가까운 수동포트 이동, 'C'인 경우 차기 공정 진행 (Flag : Y > C > N)
        //                dr["UPDUSER"] = LoginInfo.USERID;
        //                dr["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //                inDataTable.Rows.Add(dr);

        //                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_WIPATTR_SCRP_TRAY_FLAG", "RQSTDT", "RSLTDT", inDataTable);

        //                Util.MessageInfo("FM_ME_0074"); //Tray 정보 변경을 완료하였습니다.
        //                ClearControlValue();
        //                GetTrayInfo();
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert("FM_ME_0073");  //Tray 정보 변경에 실패하였습니다.
        //        Util.MessageException(ex);
        //    }
        //}


    }
    #endregion
}
