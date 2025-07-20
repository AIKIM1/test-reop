/****************************************************************************************************************************
 Created Date : 2024.09.05
      Creator : 김용준
   Decription : 충방전기 전체현황
----------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2024.09.05  김용준 : Initial Created.
*****************************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Controls;

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Linq;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_173 : UserControl, IWorkArea
    {
        #region Declaration & Constructor         

        private DataTable _dtFormation = new DataTable();

        private DataTable _dtFormLane= new DataTable();

        private UcBaseRadioButton saveRadioDisplay = null;        

        Util Util = new Util();
        private bool bUseFlag = false;
        private bool cUseFlag = false , dUseFlag =false;
        private string FirstRow , SecondRow;

        // 속도 향상을 위해 반복문 내부 비즈 및 다국어 밖으로 뺌.
        private DataTable dtRepair = null;
        private string langYun = string.Empty;
        private string langYul = string.Empty;
        private string langDan = string.Empty;
        private string COMM_LOSS = string.Empty;
        private string RESV = string.Empty;
        private string MAINTENANCE = string.Empty;
        private string REPAIR = string.Empty;
        private string USE_N = string.Empty;
        private string FIRE = string.Empty;
        private string NO_EQP = string.Empty;
        private string MANUAL = string.Empty;
        private string PRE = string.Empty;

        System.ComponentModel.BackgroundWorker bgWorker = null;

        public FCS001_173()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                // 창을 3개이상 오픈시 Radio 버튼 클리어 현상으로 우회 처리.
                // 탭 재선택시 보관된 Radio 버튼과 체크가 없어질 경우 복원
                // this.Loaded -= UserControl_Loaded; 이벤트 넣지 말것.

                if (saveRadioDisplay == null)
                {
                    LoadedProcess();

                    saveRadioDisplay = rdoTrayId;
                }
                else
                {
                    if (saveRadioDisplay.IsChecked != true) saveRadioDisplay.SetCheckStatus(true, false);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        
        private void rdoDisplay_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                saveRadioDisplay = sender as UcBaseRadioButton;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            // Grid 초기화
            dgFormation.Columns.Clear();
            dgFormation.ItemsSource = null;

            // Grid의 Column Header 생성을 위하여 Area 기준 Max Col 값을 조회
            string sMaxCol =  GetFormationMaxCol();

            
            int iMaxCol = Convert.ToInt16(sMaxCol) + 2;
            double dColumnWidth = (dgFormation.ActualWidth - 160) / (iMaxCol - 1);

            //Grid Column Header 생성
            for (int i = 0; i < iMaxCol; i++)
            {
                //GRID Column Header 생성
                if (i == 0)
                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, 200, VerticalAlignment.Center);
                else if (i == 1)
                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50, VerticalAlignment.Center);
                else
                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth, VerticalAlignment.Center);
            }
            
            // 충방전기 조회 Lane 목록 조회
            _dtFormLane = GetFormationLane();            

            string sDisplayType = string.Empty;

            if (rdoTrayId.IsChecked == true)
            {
                sDisplayType = "TRAY";
            }
            else if (rdoLotId.IsChecked == true)
            {
                sDisplayType = "LOT";
            }
            else if (rdoRouteNextOp.IsChecked == true)
            {
                sDisplayType = "ROUTE";
            }
            else if (rdoTime.IsChecked == true)
            {
                sDisplayType = "TIME";
            }
            else if (rdoAvgTemp.IsChecked == true)
            {
                sDisplayType = "TEMP";
            }

            object[] argument = new object[1] { sDisplayType };

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                btnSearch.IsEnabled = false;

                bgWorker.RunWorkerAsync(argument);
            }
        }  

        private void dgFormation_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                if (_dtFormation.Rows.Count == 0)
                {
                    //조회된 값이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0232"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = GetList(e.Argument);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            xProgress.Percent = e.ProgressPercentage;
            xProgress.ProgressText = e.UserState == null ? "" : e.UserState.ToString();
        }

        private void BgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
                else if (e.Result != null && e.Result is DataTable)
                {

                    DataTable dtData = (DataTable)e.Result;                    

                    if (dtData != null)
                    {
                        // 각 충방전기 현황 데이터를 Grid에 매핑
                        if (dgFormation.ItemsSource == null) dgFormation.ItemsSource = DataTableConverter.Convert(dtData);

                        // Lane Name, 단 정보 Column Merge
                        string[] sColumnName = new string[] { "1", "2" };
                        Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);                        

                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;

            btnSearch.IsEnabled = true;
        }
        

        #endregion

        #region Method

        private void LoadedProcess()
        {
            try
            {

                bUseFlag = Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_001_NA"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

                
                dgFormation.FontSize = 10;
                

                rdoTrayId.IsChecked = true;

                // 속도 향상을 위해 반복문 내부 다국어 밖으로 뺌.
                langYun = ObjectDic.Instance.GetObjectName("연");
                langYul = ObjectDic.Instance.GetObjectName("열");
                langDan = ObjectDic.Instance.GetObjectName("단");
                COMM_LOSS = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                RESV = ObjectDic.Instance.GetObjectName("RESV");
                MAINTENANCE = ObjectDic.Instance.GetObjectName("MAINTENANCE");
                REPAIR = ObjectDic.Instance.GetObjectName("REPAIR");
                USE_N = ObjectDic.Instance.GetObjectName("USE_N");
                FIRE = ObjectDic.Instance.GetObjectName("FIRE");
                NO_EQP = ObjectDic.Instance.GetObjectName("NO_EQP");
                MANUAL = ObjectDic.Instance.GetObjectName("MANUAL");
                PRE = ObjectDic.Instance.GetObjectName("FORMATION_RESERV");               
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private object GetList(object arg)
        {
            try
            {           

                object[] arguments = (object[])arg;

                string sDisplayType = arguments[0].Nvc();

                int workCount = 0; //실행 Count

                int totalCount = _dtFormLane.Rows.Count;

                string updateResultText = string.Empty;

                double dNumberOfProcessingCnt = 0.0;

                ClearALL();

                foreach (DataRow dr in _dtFormLane.Rows)
                {
                    string sLane = dr["LANE_PARM"].ToString();
                    string sLaneName = dr["LANE_NAME"].ToString();

                    // NA 1동 사용 체크
                    if (bUseFlag)
                    {
                        IsFormationUse(sLane);
                    }

                    GetFormationStatusForLane(sLane, sLaneName, sDisplayType);

                    workCount++;

                    dNumberOfProcessingCnt = ((double)workCount / (double)totalCount) * 100;

                    bgWorker.ReportProgress(Convert.ToInt16((double)workCount / (double)totalCount * 100), Math.Truncate(dNumberOfProcessingCnt).Nvc() + "%");
                }

                return _dtFormation;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void SetFpsFormationData(DataTable dtRslt, string LaneName, string DisplayType)
        {
            try
            {
                // 스프레드 초기화용 변수
                #region 스프레드 초기화용 변수
                int iMaxCol;
                int iMaxStg;
                int iRowCount;
                int iColumnCount;
                int tempCol = 0;
                int tempStg = 0;
                #endregion

                // 충방전기 데이터용 변수
                #region 충방전기 데이터용 변수
                string sStatus = string.Empty;
                TimeSpan tTimeSpan = new TimeSpan();

                string EQPTID = string.Empty;
                int iROW;
                int iCOL;
                int iSTG;
                int iCST_LOAD_LOCATION_CODE;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string EIOSTAT = string.Empty;
                string EQP_OP_STATUS_CD = string.Empty;
                string RUN_MODE_CD = string.Empty;
                string PROCID = string.Empty;
                string PROCNAME = string.Empty;
                string NEXT_PROCID = string.Empty;
                string RCV_ISS_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string EQPTIUSE = string.Empty;
                string NEXT_PROCNAME = string.Empty;
                string PROD_LOTID = string.Empty;
                string JOB_TIME = string.Empty;
                string ROUTID = string.Empty;
                string DUMMY_FLAG = string.Empty;
                string SPECIAL_YN = string.Empty;
                string LAST_RUN_TIME = string.Empty;
                string JIG_AVG_TEMP = string.Empty;
                string POW_AVG_TEMP = string.Empty;
                string MEGA_DCHG_FUNC_YN = string.Empty;
                string MEGA_CHG_FUNC_YN = string.Empty;
                string NOW_TIME = string.Empty;
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty;
                string PreFlag = string.Empty;
                #endregion

                //충방전기 열 갯수 확인
                DataView view = dtRslt.DefaultView;
                DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
                List<string> rowList = new List<string>();
                foreach (DataRow dr in distRowTable.Rows)
                {
                    rowList.Add(dr["ROW"].ToString());
                }

                // 충방전기 Row 갯수에 따라 로직을 나눔
                if (rowList.Count > 1)
                {
                    #region Grid 초기화
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", "ROW = " + rowList[row]));

                        iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", "ROW = " + rowList[row]));

                        if (iMaxCol > tempCol) tempCol = iMaxCol;
                        if (iMaxStg > tempStg) tempStg = iMaxStg;
                    }

                    iRowCount = tempStg * 2;
                    iColumnCount = tempCol + 2;  // 단 추가. Lane 추가
                    

                    //충방전기 Row별 Datatable 정의
                    List<DataTable> dtList = new List<DataTable>();

                    ////Grid Column 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataTable dt = new DataTable();

                        for (int i = 0; i < iColumnCount; i++)
                        {
                            dt.Columns.Add((i + 1).ToString(), typeof(string));
                        }

                        dtList.Add(dt);
                    }

                    //Grid Row 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        for (int i = 0; i < iRowCount; i++)
                        {
                            DataRow drRow = dtList[row].NewRow();
                            
                            for (int j = 0; j < dtList[row].Columns.Count; j++)
                            {
                                if (j == 0)
                                    drRow[j] = LaneName;

                                if (j > 1)
                                {
                                    drRow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                                }
                            }                            

                            dtList[row].Rows.Add(drRow);
                        }

                    }                    

                    //Grid Row Header 생성
                    List<DataRow> drListHeader = new List<DataRow>();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataRow drRowHeader = dtList[row].NewRow();
                        for (int i = 0; i < dtList[row].Columns.Count; i++)
                        {
                            if (i == 0)
                                drRowHeader[i] = LaneName;
                            else if (i == 1)
                                drRowHeader[i] = rowList[row].Nvc() + langYul;
                            else
                                drRowHeader[i] = (i-1).ToString() + langYun;
                        }
                        drListHeader.Add(drRowHeader);
                    }
                    #endregion

                    #region Data Setting                    

                    int k = -1;

                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        k++;

                        iROW = dr["ROW"].NvcInt();
                        iCOL = dr["COL"].NvcInt();
                        iSTG = dr["STG"].NvcInt();
                        iCST_LOAD_LOCATION_CODE = dr["CST_LOAD_LOCATION_CODE"].NvcInt();

                        EQPTID = dr["EQPTID"].Nvc();
                        CSTID = dr["CSTID"].Nvc();
                        LOTID = dr["LOTID"].Nvc();
                        EIOSTAT = dr["EIOSTAT"].Nvc();
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].Nvc();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc();
                        PreFlag = dr["PRE"].Nvc();                        

                        #region MyRegion         
                         

                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (DisplayType.Equals("TIME") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if( DisplayType.Equals("ROUTE") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (DisplayType.Equals("TRAY"))
                            {
                                sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                            }
                            else if (DisplayType.Equals("LOT"))
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.ToString().Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (DisplayType.Equals("TIME"))
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (DisplayType.Equals("ROUTE"))
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                            
                        }                        

                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID;
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);                                

                                if (DisplayType.Equals("TIME"))
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME;
                                break;
                            case "25": //수리중
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                break;
                            case "22": //사용안함
                                sStatus = USE_N;
                                break;
                            case "27": //화재
                                sStatus = FIRE;
                                break;
                            case "99": //설비없음
                                sStatus = NO_EQP;
                                break;
                        }

                        for (int row = 0; row < rowList.Count; row++)
                        {
                            if (rowList[row].NvcInt() == iROW)
                            {
                                if (iCST_LOAD_LOCATION_CODE > 1)
                                {
                                    dtList[row].Rows[iRowCount - (iSTG * 2)][0] = LaneName; //Lane
                                    dtList[row].Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                    dtList[row].Rows[iRowCount - (iSTG * 2)][iCOL+1] = sStatus;
                                }
                                else
                                {
                                    dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName; //Lane
                                    dtList[row].Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                    dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL+1] = sStatus;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtList[row].Rows.InsertAt(drListHeader[row], 0);
                    }

                    //상,하 Merge
                    DataTable dtTotal = new DataTable();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtTotal.Merge(dtList[row], false, MissingSchemaAction.Add);
                    }

                    _dtFormation.Merge(dtTotal, false, MissingSchemaAction.Add);

                    #endregion

                }
                else
                {
                    #region Grid 초기화
                    iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));

                    iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

                    iRowCount = iMaxStg * 2;
                    iColumnCount = (iMaxCol + 1) / 2 + 2;   //단 추가. Lane 추가                    

                    //상단 Datatable, 하단 Datatable 정의
                    DataTable Udt = new DataTable();
                    DataTable Ddt = new DataTable();

                    //Column 생성
                    for (int i = 0; i < iColumnCount; i++)
                    {
                        Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                    }

                    //Row 생성
                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow Urow = Udt.NewRow();

                        for (int j = 0; j < Udt.Columns.Count; j++)
                        {

                            if (j == 0)
                                Urow[j] = LaneName;

                            if (j > 1)
                            {
                                Urow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        Udt.Rows.Add(Urow);

                        DataRow Drow = Ddt.NewRow();

                        for (int j = 0; j < Ddt.Columns.Count; j++)
                        {
                            if (j == 0)
                                Drow[j] = LaneName;

                            if (j > 1)
                            {
                                Drow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        Ddt.Rows.Add(Drow);
                    }
                    

                    //Row Header 생성
                    DataRow UrowHeader = Udt.NewRow();
                    for (int i = 0; i < Udt.Columns.Count; i++)
                    {
                        if (i == 0)
                            UrowHeader[i] = LaneName;
                        else if (i == 1)
                            UrowHeader[i] = string.Empty;
                        else
                            UrowHeader[i] = (i-1).ToString() + langYun;
                    }

                    //Row Header 생성
                    DataRow DrowHeader = Ddt.NewRow();
                    for (int i = 0; i < Ddt.Columns.Count; i++)
                    {
                        if (i == 0)
                            DrowHeader[i] = LaneName;
                        else if (i == 1)
                            DrowHeader[i] = string.Empty;
                        else
                            DrowHeader[i] = ((i-1) + Ddt.Columns.Count - 1).ToString() + langYun;
                    }
                    #endregion

                    #region Data Setting                    

                    int k = -1;
                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        k++;

                        iROW = dr["ROW"].NvcInt();
                        iCOL = dr["COL"].NvcInt();
                        iSTG = dr["STG"].NvcInt();
                        iCST_LOAD_LOCATION_CODE = dr["CST_LOAD_LOCATION_CODE"].NvcInt();

                        EQPTID = dr["EQPTID"].Nvc();
                        CSTID = dr["CSTID"].Nvc();
                        LOTID = dr["LOTID"].Nvc();
                        EIOSTAT = dr["EIOSTAT"].Nvc();
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].Nvc();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc();
                        PreFlag = dr["PRE"].Nvc();

                        #region MyRegion                                               

                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (DisplayType.Equals("TIME")&& !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if (DisplayType.Equals("ROUTE") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (DisplayType.Equals("TRAY"))
                            {
                                sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                            }
                            else if (DisplayType.Equals("LOT"))
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.ToString().Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (DisplayType.Equals("TIME"))
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (DisplayType.Equals("ROUTE"))
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }                            
                        }
                        
                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID;
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);                                

                                if (DisplayType.Equals("TIME"))
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME;
                                break;
                            case "25": //수리중
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                break;
                            case "22": //사용안함
                                sStatus = USE_N;
                                break;
                            case "27": //화재
                                sStatus = FIRE;
                                break;
                            case "99": //설비없음
                                sStatus = NO_EQP;
                                break;
                        }
                                                
                        if (iCOL <= iColumnCount - 2)
                        {
                            if (iCST_LOAD_LOCATION_CODE > 1)
                            {
                                Udt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                Udt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                Udt.Rows[iRowCount - (iSTG * 2)][iCOL + 1] = sStatus;
                            }
                            else
                            {
                                Udt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                Udt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                Udt.Rows[iRowCount - (iSTG * 2) + 1][iCOL + 1] = sStatus;
                            }
                        }
                        else
                        {
                            if (iCST_LOAD_LOCATION_CODE > 1)
                            {
                                Ddt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                Ddt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                Ddt.Rows[iRowCount - (iSTG * 2)][(iCOL + 1) - iColumnCount + 2] = sStatus;
                            }
                            else
                            {
                                Ddt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                Ddt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                Ddt.Rows[iRowCount - (iSTG * 2) + 1][(iCOL + 1) - iColumnCount + 2] = sStatus;
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    Udt.Rows.InsertAt(UrowHeader, 0);
                    Ddt.Rows.InsertAt(DrowHeader, 0);

                    //상,하 Merge
                    Udt.Merge(Ddt, false, MissingSchemaAction.Add);

                    // 각 Lane 충방전기 현황을 _dtFormation에 Merge -> 최종 완료된 이후에 Grid에 매핑
                    _dtFormation.Merge(Udt, false, MissingSchemaAction.Add);
                    
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFpsFormationDataNA(DataTable dtRslt, string LaneName, string DisplayType)
        {
            try
            {
                // 스프레드 초기화용 변수
                #region 스프레드 초기화용 변수
                int iMaxCol;
                int iMaxStg;
                int iRowCount;
                int iColumnCount;
                
                int tempCol = 0;
                int tempStg = 0;
                
                #endregion

                // 충방전기 데이터용 변수
                #region 충방전기 데이터용 변수
                string sStatus = string.Empty;
                TimeSpan tTimeSpan = new TimeSpan();
                int iROWGRP;
                string EQPTID = string.Empty;
                int iROW;
                int iCOL;
                int iSTG;
                int iCST_LOAD_LOCATION_CODE;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string EIOSTAT = string.Empty;
                string EQP_OP_STATUS_CD = string.Empty;
                string RUN_MODE_CD = string.Empty;
                string PROCID = string.Empty;
                string PROCNAME = string.Empty;
                string NEXT_PROCID = string.Empty;
                string RCV_ISS_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string EQPTIUSE = string.Empty;
                string NEXT_PROCNAME = string.Empty;
                string PROD_LOTID = string.Empty;
                string JOB_TIME = string.Empty;
                string ROUTID = string.Empty;
                string DUMMY_FLAG = string.Empty;
                string SPECIAL_YN = string.Empty;
                string LAST_RUN_TIME = string.Empty;
                string JIG_AVG_TEMP = string.Empty;
                string POW_AVG_TEMP = string.Empty;
                string MEGA_DCHG_FUNC_YN = string.Empty;
                string MEGA_CHG_FUNC_YN = string.Empty;
                string NOW_TIME = string.Empty;
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty;
                string PreFlag = string.Empty;
                #endregion

                //충방전기 열 갯수 확인
                DataView view = dtRslt.DefaultView;
                DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
                List<string> rowList = new List<string>();
                foreach (DataRow dr in distRowTable.Rows)
                {
                    rowList.Add(dr["ROW"].ToString());
                }

                // 충방전기 Row 갯수에 따라 로직을 나눔
                if (rowList.Count > 1)
                {
                    #region Grid 초기화
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", "ROW = " + rowList[row]));
                        iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", "ROW = " + rowList[row]));

                        if (iMaxCol > tempCol) tempCol = iMaxCol;
                        if (iMaxStg > tempStg) tempStg = iMaxStg;
                    }

                    iRowCount = tempStg * 2;
                    ////////////////내부적으로 7-2연 컬럼 카운트
                    iColumnCount = 13;


                    //충방전기 Row별 Datatable 정의
                    List<DataTable> dtList = new List<DataTable>();

                    //Grid Column 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataTable dt = new DataTable();

                        for (int i = 0; i < iColumnCount; i++)
                        {
                            dt.Columns.Add((i + 1).ToString(), typeof(string));
                        }

                        dtList.Add(dt);
                    }

                    //Grid Row 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        for (int i = 0; i < iRowCount; i++)
                        {
                            DataRow drRow = dtList[row].NewRow();

                            for (int j = 0; j < dtList[row].Columns.Count; j++)
                            {
                                if (j == 0)
                                    drRow[j] = LaneName;

                                if (j > 1)
                                {
                                    drRow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                                }
                            }

                            dtList[row].Rows.Add(drRow);
                        }

                    }                    

                    //Grid Row Header 생성
                    List<DataRow> drListHeader = new List<DataRow>();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataRow drRowHeader = dtList[row].NewRow();
                        for (int i = 0; i < dtList[row].Columns.Count; i++)
                        {
                            if (i == 0)
                                drRowHeader[i] = LaneName;
                            else if (i == 1)
                                drRowHeader[i] = rowList[row].Nvc() + langYul;
                            else
                            {
                                if (row == 0)
                                {
                                    drRowHeader[i] = ((i-1) + 9).ToString() + langYun;
                                }
                                else
                                {
                                    drRowHeader[i] = (i-1).ToString() + langYun;
                                }

                            }
                        }
                        drListHeader.Add(drRowHeader);
                    }
                    #endregion

                    #region Data Setting

                    int k = -1;
                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        k++;

                        iROW = dr["ROW"].NvcInt();
                        iCOL = dr["COL"].NvcInt();
                        iSTG = dr["STG"].NvcInt();
                        iCST_LOAD_LOCATION_CODE = dr["CST_LOAD_LOCATION_CODE"].NvcInt();

                        EQPTID = dr["EQPTID"].Nvc();
                        CSTID = dr["CSTID"].Nvc();
                        LOTID = dr["LOTID"].Nvc();
                        EIOSTAT = dr["EIOSTAT"].Nvc();
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].ToString();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc();
                        PreFlag = dr["PRE"].Nvc(); 
                        

                        #region MyRegion                    

                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (DisplayType.Equals("TIME") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if (DisplayType.Equals("ROUTE")&& !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (DisplayType.Equals("TRAY"))
                            {
                                sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                            }
                            else if (DisplayType.Equals("LOT"))
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.ToString().Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (DisplayType.Equals("TIME"))
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (DisplayType.Equals("ROUTE"))
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                            
                        }
                        
                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID;
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);                                

                                if (DisplayType.Equals("TIME"))
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME;
                                break;
                            case "25": //수리중
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                if (bUseFlag)
                                {
                                    sStatus = "BM " + GetMaintName(EQPTID, LAST_RUN_TIME);
                                }
                                break;
                            case "22": //사용안함
                                sStatus = USE_N;
                                break;
                            case "27": //화재
                                sStatus = FIRE;
                                break;
                            case "99": //설비없음
                                sStatus = NO_EQP;
                                break;
                        }
                        

                        for (int row = 0; row < rowList.Count; row++)
                        {
                            if (rowList[row].NvcInt() == iROW)
                            {
                                if (row == 0)
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        //////////////내부적으로 
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][0] = LaneName; //Lane
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][iCOL - 9 + 1] = sStatus;
                                    }
                                    else
                                    {
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName; //Lane
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL - 9 + 1] = sStatus;
                                    }
                                else if (row == 1)
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][0] = LaneName; //Lane
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        dtList[row].Rows[iRowCount - (iSTG * 2)][iCOL + 1] = sStatus;
                                    }
                                    else
                                    {
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName; //Lane
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL + 1] = sStatus;
                                    }

                            }
                        }
                        
                        #endregion
                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtList[row].Rows.InsertAt(drListHeader[row], 0);
                    }

                    //상,하 Merge
                    DataTable dtTotal = new DataTable();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtTotal.Merge(dtList[row], false, MissingSchemaAction.Add);
                    }

                    _dtFormation.Merge(dtTotal, false, MissingSchemaAction.Add);

                    #endregion

                }
                else
                {
                    #region Grid 초기화
                    iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
                    iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));
                    
                    // 1단 충방전기 여부 확인
                    if(cUseFlag)
                    {
                        iRowCount = iMaxStg;
                    }
                    else
                    {
                        iRowCount = iMaxStg * 2;
                    }
                    
                    iColumnCount = (iMaxCol + 1) / 2 + 2;   //단 추가. Lane 추가

                    //상단 Datatable, 하단 Datatable 정의
                    DataTable Udt = new DataTable();
                    DataTable Ddt = new DataTable();

                    DataTable NAUdt = new DataTable();
                    DataTable NADdt = new DataTable();

                    //Column 생성
                    for (int i = 0; i < iColumnCount; i++)
                    {

                        Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        Ddt.Columns.Add((i + 1).ToString(), typeof(string));

                        NAUdt.Columns.Add((i + 1).ToString(), typeof(string));
                        NADdt.Columns.Add((i + 1).ToString(), typeof(string));
                    }

                    //Row 생성
                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow Urow = Udt.NewRow();

                        for (int j = 0; j < Udt.Columns.Count; j++)
                        {

                            if (j == 0)
                                Urow[j] = LaneName;

                            if (j > 1)
                            {
                                Urow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        Udt.Rows.Add(Urow);

                        DataRow Drow = Ddt.NewRow();

                        for (int j = 0; j < Ddt.Columns.Count; j++)
                        {
                            if (j == 0)
                                Drow[j] = LaneName;

                            if (j > 1)
                            {
                                Drow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        Ddt.Rows.Add(Drow);

                        DataRow NADrow = NAUdt.NewRow();

                        for (int j = 0; j < NAUdt.Columns.Count; j++)
                        {
                            if (j == 0)
                                NADrow[j] = LaneName;

                            if (j > 1)
                            {
                                NADrow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        NAUdt.Rows.Add(NADrow);

                        DataRow NAUrow = NADdt.NewRow();

                        for (int j = 0; j < NADdt.Columns.Count; j++)
                        {
                            if (j == 0)
                                NAUrow[j] = LaneName;

                            if (j > 1)
                            {
                                NAUrow[j] = NO_EQP; // Box 상태 '설비없음'으로  초기화
                            }
                        }

                        NADdt.Rows.Add(NAUrow);

                    }

                    //Row Header 생성
                    DataRow UrowHeader = Udt.NewRow();
                    for (int i = 0; i < Udt.Columns.Count; i++)
                    {
                        if (i == 0)
                            UrowHeader[i] = LaneName;
                        else if (i == 1)
                            UrowHeader[i] = string.Empty;
                        else
                            UrowHeader[i] = (i-1).ToString() + langYun;
                    }

                    //Row Header 생성
                    DataRow DrowHeader = Ddt.NewRow();
                    for (int i = 0; i < Ddt.Columns.Count; i++)
                    {
                        if (i == 0)
                            DrowHeader[i] = LaneName;
                        else if (i == 1)
                            DrowHeader[i] = string.Empty;
                        else
                            DrowHeader[i] = ((i-2) + Ddt.Columns.Count - 1).ToString() + langYun;
                    }

                    DataRow NADrowHeader = NADdt.NewRow();
                    DataRow NAUrowHeader = NAUdt.NewRow();

                    if (dUseFlag)
                    {

                        for (int i = 0; i < NAUdt.Columns.Count; i++)
                        {
                            if (i == 0)
                                NAUrowHeader[i] = LaneName;
                            else if (i == 1)
                                NAUrowHeader[i] = string.Empty;
                            else
                                NAUrowHeader[i] = (i-1).ToString() + langYun;
                        }

                        for (int i = 0; i < NADdt.Columns.Count; i++)
                        {
                            if (i == 0)
                                NADrowHeader[i] = LaneName;
                            else if (i == 1)
                                NADrowHeader[i] = string.Empty;
                            else
                                NADrowHeader[i] = ((i-2) + NADdt.Columns.Count - 1).ToString() + langYun;
                        }

                    }
                    #endregion

                    #region Data Setting

                    int k = -1;
                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        k++;

                        iROW = dr["ROW"].NvcInt();
                        iCOL = dr["COL"].NvcInt();
                        iSTG = dr["STG"].NvcInt();
                        iCST_LOAD_LOCATION_CODE = dr["CST_LOAD_LOCATION_CODE"].NvcInt();
                        iROWGRP = dr["ROW_GRP"].NvcInt();
                        EQPTID = dr["EQPTID"].Nvc();
                        CSTID = dr["CSTID"].Nvc();
                        LOTID = dr["LOTID"].Nvc();
                        EIOSTAT = dr["EIOSTAT"].Nvc();
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].Nvc();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc();
                        PreFlag = dr["PRE"].Nvc();                        

                        #region MyRegion
                        
                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (DisplayType.Equals("TIME") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if (DisplayType.Equals("ROUTE") && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (DisplayType.Equals("TRAY"))
                            {
                                sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                            }
                            else if (DisplayType.Equals("LOT"))
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.ToString().Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (DisplayType.Equals("TIME"))
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (DisplayType.Equals("ROUTE"))
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (DisplayType.Equals("TEMP"))
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }                        

                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID;
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

                                if (DisplayType.Equals("TIME"))
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME;
                                break;
                            case "25": //수리중
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                if (bUseFlag)
                                {
                                    sStatus =  "BM " + GetMaintName(EQPTID, LAST_RUN_TIME);
                                }
                                    break;
                            case "22": //사용안함
                                sStatus = USE_N;
                                break;
                            case "27": //화재
                                sStatus = FIRE;
                                break;
                            case "99": //설비없음
                                sStatus = NO_EQP;
                                break;
                        }

                        /// 충방전기 1단만 사용 여부 체크
                        if (cUseFlag)
                        {
                            if (iCOL <= iColumnCount - 2)
                            {

                                if (iCST_LOAD_LOCATION_CODE == 1) // 1단 Status만 표시.
                                {
                                    Udt.Rows[iRowCount - (iSTG)][0] = LaneName;  //Lane
                                    Udt.Rows[iRowCount - (iSTG)][1] = iSTG.ToString() + langDan;  //단
                                    Udt.Rows[iRowCount - (iSTG)][iCOL + 1] = sStatus;

                                }
                            }
                            else
                            {
                                if (iCST_LOAD_LOCATION_CODE == 1) // 1단 Status만 표시.
                                {
                                    Ddt.Rows[iRowCount - (iSTG)][0] = LaneName;  //Lane
                                    Ddt.Rows[iRowCount - (iSTG)][1] = iSTG.ToString() + langDan;  //단
                                    Ddt.Rows[iRowCount - (iSTG)][iCOL - iColumnCount + 3] = sStatus;

                                }                                
                            }
                        }
                        /// 충방전기 멀티레인 사용 여부 체크
                        else if (dUseFlag)
                        {
                            if (iROWGRP > 1)
                            {
                                if (iCOL <= iColumnCount - 2)
                                {
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        NAUdt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                        NAUdt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        NAUdt.Rows[iRowCount - (iSTG * 2)][iCOL + 1] = sStatus;
                                    }
                                    else
                                    {
                                        NAUdt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                        NAUdt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        NAUdt.Rows[iRowCount - (iSTG * 2) + 1][iCOL + 1] = sStatus;
                                    }
                                }
                                else
                                {
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        NADdt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                        NADdt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        NADdt.Rows[iRowCount - (iSTG * 2)][iCOL - iColumnCount + 3] = sStatus;
                                    }
                                    else
                                    {
                                        NADdt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                        NADdt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        NADdt.Rows[iRowCount - (iSTG * 2) + 1][iCOL - iColumnCount + 3] = sStatus;
                                    }
                                }

                            }
                            else
                            {
                                if (iCOL <= iColumnCount - 2)
                                {
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        Udt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                        Udt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        Udt.Rows[iRowCount - (iSTG * 2)][iCOL + 1] = sStatus;

                                    }
                                    else
                                    {
                                        Udt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                        Udt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        Udt.Rows[iRowCount - (iSTG * 2) + 1][iCOL + 1] = sStatus;

                                    }
                                }
                                else
                                {
                                    if (iCST_LOAD_LOCATION_CODE > 1)
                                    {
                                        Ddt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                        Ddt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                        Ddt.Rows[iRowCount - (iSTG * 2)][iCOL - iColumnCount + 3] = sStatus;

                                    }
                                    else
                                    {
                                        Ddt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                        Ddt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                        Ddt.Rows[iRowCount - (iSTG * 2) + 1][iCOL - iColumnCount + 3] = sStatus;

                                    }
                                }

                            }
                        }
                        else
                        {
                            if (iCOL <= iColumnCount - 2)
                            {
                                if (iCST_LOAD_LOCATION_CODE > 1)
                                {
                                    Udt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                    Udt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                    Udt.Rows[iRowCount - (iSTG * 2)][iCOL + 1] = sStatus;
                                }
                                else
                                {
                                    Udt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                    Udt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                    Udt.Rows[iRowCount - (iSTG * 2) + 1][iCOL + 1] = sStatus;
                                }
                            }
                            else
                            {
                                if (iCST_LOAD_LOCATION_CODE > 1)
                                {
                                    Ddt.Rows[iRowCount - (iSTG * 2)][0] = LaneName;  //Lane
                                    Ddt.Rows[iRowCount - (iSTG * 2)][1] = iSTG.ToString() + langDan;  //단
                                    Ddt.Rows[iRowCount - (iSTG * 2)][iCOL - iColumnCount + 3] = sStatus;
                                }
                                else
                                {
                                    Ddt.Rows[iRowCount - (iSTG * 2) + 1][0] = LaneName;  //Lane
                                    Ddt.Rows[iRowCount - (iSTG * 2) + 1][1] = iSTG.ToString() + langDan;  //단
                                    Ddt.Rows[iRowCount - (iSTG * 2) + 1][iCOL - iColumnCount + 3] = sStatus;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    DataTable[] FirstDt = new DataTable[] { Udt, Ddt };
                    DataTable[] SecondDt = new DataTable[] { NAUdt, NADdt };
                    Udt.Rows.InsertAt(UrowHeader, 0);
                    Ddt.Rows.InsertAt(DrowHeader, 0);

                    if (dUseFlag)
                    {
                        NAUdt.Rows.InsertAt(NAUrowHeader, 0);
                        NADdt.Rows.InsertAt(NADrowHeader, 0);

                        FirstDt[Convert.ToInt32(FirstRow)].Merge(SecondDt[Convert.ToInt32(SecondRow)], false, MissingSchemaAction.Add);
                        Udt = FirstDt[Convert.ToInt32(FirstRow)].Copy();

                    }
                    else
                    {
                        Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                    }

                    _dtFormation.Merge(Udt, false, MissingSchemaAction.Add);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth, VerticalAlignment vertical)
        {
            if (dg.Columns.Contains(sColName)) return;

            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = vertical,
                TextWrapping = TextWrapping.NoWrap,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)

            });
        }

        private void ClearALL()
        {
            _dtFormation.Clear();
        }
        
        private void IsFormationUse(string LaneID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_FORMEQPT_COND_USE_FLAG";
                dr["COM_CODE"] = LaneID;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                cUseFlag = false;
                dUseFlag = false;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    cUseFlag = true;
                    dUseFlag = false;
                }
                else if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR2"].ToString().Equals("Y"))
                {
                    cUseFlag = false;
                    dUseFlag = true;
                    FirstRow = dtResult.Rows[0]["ATTR3"].ToString();
                    SecondRow = dtResult.Rows[0]["ATTR4"].ToString();
                }
                else
                {
                    cUseFlag = false;
                    dUseFlag = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetMaintData(string LaneID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = LaneID;
                dtRqst.Rows.Add(dr);

                dtRepair = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TRAY_EQP_REPAIR", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetMaintName(string sEqpId, string sLastRunTime)
        {
            string sReturn = string.Empty;
            try
            {
                if (dtRepair == null || dtRepair.Rows.Count == 0) return sReturn;

                DataRow drEqpt = dtRepair.AsEnumerable().Where(w => w.Field<string>("EQPTID").Equals(sEqpId)).FirstOrDefault();
                if (drEqpt == null)
                {
                    sReturn = MANUAL + " " + sLastRunTime;  //수동
                }
                else
                {
                    sReturn = drEqpt["TROUBLE_REPAIR_CD2"].ToString() + " " + drEqpt["TROUBLE_REPAIR_TIME"].ToString();
                    if (bUseFlag)
                    {
                        sReturn = Convert.ToDateTime(Util.NVC(drEqpt["TROUBLE_REPAIR_TIME"])).ToString("MM-dd HH:mm"); 
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sReturn;
        }

        private void GetFormationStatusForLane(string LaneID, string LaneName, string DisplayType)
        {
            try
            {
                // 각 Lane 충방전기 정보 조회
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = LaneID;
                dtRqst.Rows.Add(dr);

                DataTable dtResultHc = new ClientProxy().ExecuteServiceSync("BR_SEL_FORMATION_VIEW_BY_LANE", "RQSTDT", "RSTDT", dtRqst);                

                if(dtResultHc != null && dtResultHc.Rows.Count > 0)
                {
                    GetMaintData(LaneID);

                    // NA 1동 사용 체크
                    if (bUseFlag)
                    {
                        SetFpsFormationDataNA(dtResultHc, LaneName, DisplayType);
                    }
                    else
                    {
                        SetFpsFormationData(dtResultHc, LaneName, DisplayType);
                    }
                }
                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private DataTable GetFormationLane()
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_LANE_FOR_FORM_STATE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return null;
            }
        }
        
        private string GetFormationMaxCol()
        {
            try
            {
                string sRet = "0";
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_FORM_MAX_COL", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    sRet = dtResult.Rows[0]["MAX_COL_NUM"].ToString();
                }

                return sRet;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return "0";
            }
        }
        
        #endregion

    }
}

