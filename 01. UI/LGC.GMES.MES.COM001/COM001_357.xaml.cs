/*COM001_045(생산실적 조회(생산Lot 기준))를 복사하여 사용*/
/*************************************************************************************
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.24  이병윤 : E20230718-000630 : "안정화 간 비가동시간[SBLZ_DWTM_TIME]" 추가(정보 수동 입력하는 칸)
 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Data;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_357 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;
        private readonly Util _util = new Util();
        string _EQGRID = string.Empty;


        private BizDataSet _Biz = new BizDataSet();
        public COM001_357()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_ASSY", cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS_ASSY");

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT_ASSY", cbParent: cboEquipmentParent);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;

            cboProcess.IsEnabled = false;


            SetGridColumnCombo(dgModlChg, "MODL_CHG_LVDF_TYPE_CODE", "MODL_CHG_LVDF_TYPE_CODE");
            SetGridColumnCombo(dgModlChg, "DEL_FLAG", "MODL_CHG_DEL_FLAG");

            setComboBox(cboModlChgLvdfTypeCode, CommonCombo.ComboStatus.ALL, "MODL_CHG_LVDF_TYPE_CODE");
            setComboBox(cboDelFlag, CommonCombo.ComboStatus.ALL, "MODL_CHG_DEL_FLAG");

            //// 모델 AutoComplete
            //GetModel();

        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {          
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;



            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetModlChgHist();
            SetReport();
        }  
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
                //cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                //SetProcess();
                //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                //SetEquipment();
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgModlChg);

            }
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;


                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }


            }
        }
        #endregion

        #endregion


        #region Mehod

        #region [BizCall]

        #region [### 작업대상 조회 ###]
        public void GetModlChgHist()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRE_PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("MODL_CHG_LVDF_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DEL_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                string sEqptID = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;


                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtPrePrjtName.Text))
                    dr["PRE_PRJT_NAME"] = txtPrePrjtName.Text.ToString();

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text.ToString();

                if (!string.IsNullOrWhiteSpace(txtWoId.Text))
                    dr["WOID"] = txtWoId.Text.ToString();

                string sModlChgLvdfType = Util.GetCondition(cboModlChgLvdfTypeCode);
                if (!string.IsNullOrWhiteSpace(sModlChgLvdfType))
                    dr["MODL_CHG_LVDF_TYPE_CODE"] = sModlChgLvdfType;

                string sDelFlag = Util.GetCondition(cboDelFlag);
                if (!string.IsNullOrWhiteSpace(sDelFlag))
                    dr["DEL_FLAG"] = sDelFlag;

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;

                sBizName = "DA_PRD_SEL_MODL_CHG_HIST";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgModlChg, dtRslt, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetReport()
        {                  
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;


                sBizName = "DA_PRE_SEL_MODL_CHG_REPORT";


                DataTable dtData = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                sBizName = "DA_PRE_SEL_MODL_CHG_REPORT_COLUMN";

                DataTable dtColumn = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                if (dtData.Rows.Count == 0 || dtColumn.Rows.Count == 0)
                    return;

                SetDgModlChgReport(dtColumn, dtData);

                _util.SetDataGridMergeExtensionCol(dgModlChgReport, new string[] { "MODL_CHG_LVDF_TYPE_CODE" }, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetDgModlChgReport(DataTable dtCol,DataTable dtData)
        {
            #region Report Grid Column Setting

            dgModlChgReport.Columns.Clear();

            dgModlChgReport.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "MODL_CHG_LVDF_TYPE_CODE",
                Header = ObjectDic.Instance.GetObjectName("난이도"),
                Binding = new Binding()
                {
                    Path = new PropertyPath("MODL_CHG_LVDF_TYPE_CODE"),
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Visible,
                IsReadOnly = true
            });

            dgModlChgReport.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "EQPTID",
                Header = ObjectDic.Instance.GetObjectName("설비ID"),
                Binding = new Binding()
                {
                    Path = new PropertyPath("EQPTID"),
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Collapsed,
                IsReadOnly = true
            });

            dgModlChgReport.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = "EQPTNAME",
                Header = ObjectDic.Instance.GetObjectName("설비명"),
                Binding = new Binding()
                {
                    Path = new PropertyPath("EQPTNAME"),
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Visible,
                IsReadOnly = true
            });


            Util.GridSetData(dgModlChgReport, dtCol, FrameOperation, true);

            DateTime minDt = new DateTime(9999, 12, 01);
            DateTime maxDt = new DateTime(2000, 01, 01);


            for (int i = 0; i < dtData.Rows.Count; i++){
                DateTime tmp = new DateTime(Int32.Parse(dtData.Rows[i]["YEAR"].ToString()), Int32.Parse(dtData.Rows[i]["MONTH"].ToString()), 1);

                int minResult = DateTime.Compare(minDt, tmp);
                int maxResult = DateTime.Compare(maxDt, tmp);

                if (minResult > 0)
                    minDt = tmp;
                if (maxResult < 0)
                    maxDt = tmp; 
            }

            int diffYear = 0;
            int diffMonth = 0;

            if(maxDt.Year > minDt.Year)
            {
                diffYear = maxDt.Year - minDt.Year;
            }

            diffMonth = maxDt.Month + diffYear * 12 - minDt.Month + 1;

            int strtYear = minDt.Year;
            int strtMonth = minDt.Month;

            DataTable tmpDt = new DataTable();
            tmpDt = ((DataView)dgModlChgReport.ItemsSource).ToTable();

            for (int i = 0; i < diffMonth; i++)
            {
                if(i == 0 || strtMonth.ToString() == "1")
                {
                    dgModlChgReport.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = strtYear.ToString(),
                        Header = ObjectDic.Instance.GetObjectName(strtYear + "년 평균"),
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(strtYear.ToString()),
                        },
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Visibility = Visibility.Visible,
                        IsReadOnly = false
                    });

                    tmpDt.Columns.Add(strtYear.ToString(), typeof(string));

                }
                dgModlChgReport.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = strtYear.ToString() + strtMonth.ToString(),
                    Header = ObjectDic.Instance.GetObjectName(strtYear + "년 " + strtMonth + "월"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath(strtYear.ToString() + strtMonth.ToString()),
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Visibility = Visibility.Visible,
                    IsReadOnly = false
                });

                tmpDt.Columns.Add(strtYear.ToString() + strtMonth.ToString(), typeof(string));

                strtMonth++;
                if (strtMonth == 13) {
                    strtMonth = 1;
                    strtYear++;
                }

            }
            #endregion

            for (int i = 0; i < tmpDt.Columns.Count; i++)
            {

                if (tmpDt.Columns[i].ColumnName == "MODL_CHG_LVDF_TYPE_CODE" || tmpDt.Columns[i].ColumnName == "EQPTID" || tmpDt.Columns[i].ColumnName == "EQPTNAME")
                {
                    continue;
                }

                for (int j = 0; j < tmpDt.Rows.Count; j++)
                {
                    tmpDt.Rows[j][i] = 0;
                }
            }

            dgModlChgReport.BeginEdit();
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                for(int j = 0; j < tmpDt.Rows.Count; j++)
                {                 
                    if (dtData.Rows[i]["MODL_CHG_LVDF_TYPE_CODE"].ToString() == tmpDt.Rows[j]["MODL_CHG_LVDF_TYPE_CODE"].ToString()
                        && dtData.Rows[i]["EQPTID"].ToString() == tmpDt.Rows[j]["EQPTID"].ToString())
                    {
                        string colName = dtData.Rows[i]["YEAR"].ToString() + dtData.Rows[i]["MONTH"].ToString();
                        tmpDt.Rows[j][colName] = dtData.Rows[i]["COUNT"].ToString();
                    }
                }
            }

            int sumDiff = 0;
            int sumEasy = 0;

            DataRow drDiff = tmpDt.NewRow();
            DataRow drEasy = tmpDt.NewRow();
            DataRow drAvg = tmpDt.NewRow();

            drDiff["MODL_CHG_LVDF_TYPE_CODE"] = "DIFFICULTY";
            drEasy["MODL_CHG_LVDF_TYPE_CODE"] = "EASY";
            drAvg["MODL_CHG_LVDF_TYPE_CODE"] = "TOTAL";

            drDiff["EQPTID"] = "SUM";
            drEasy["EQPTID"] = "SUM";
            drAvg["EQPTID"] = "TOTAL";


            drDiff["EQPTNAME"] = "소계";
            drEasy["EQPTNAME"] = "소계";
            drAvg["EQPTNAME"] = "TOTAL";


            int cntDiff = 0;

            for(int i = 0; i < tmpDt.Rows.Count; i++)
            {
                if(tmpDt.Rows[i]["MODL_CHG_LVDF_TYPE_CODE"].ToString() == "DIFFICULTY")
                {
                    cntDiff++;
                }
            }
            
            // 월별 소계
            for (int i = 0; i < tmpDt.Columns.Count; i++)
            {
                sumDiff = 0;
                sumEasy = 0;

                if(tmpDt.Columns[i].ColumnName == "MODL_CHG_LVDF_TYPE_CODE" ||tmpDt.Columns[i].ColumnName == "EQPTID" || tmpDt.Columns[i].ColumnName == "EQPTNAME")
                {
                    continue;
                }

                for (int j = 0; j < tmpDt.Rows.Count; j++)
                {
                    if(tmpDt.Rows[j]["MODL_CHG_LVDF_TYPE_CODE"].ToString() == "DIFFICULTY")
                    {
                        if (!string.IsNullOrEmpty(tmpDt.Rows[j][i].ToString())) {
                            sumDiff += Int32.Parse(tmpDt.Rows[j][i].ToString());
                        }
                    }
                    else if(tmpDt.Rows[j]["MODL_CHG_LVDF_TYPE_CODE"].ToString() == "EASY")
                    {
                        if (!string.IsNullOrEmpty(tmpDt.Rows[j][i].ToString()))
                        {
                            sumEasy += Int32.Parse(tmpDt.Rows[j][i].ToString());
                        }
                    }
                }

                drDiff[tmpDt.Columns[i].ColumnName] = sumDiff;
                drEasy[tmpDt.Columns[i].ColumnName] = sumEasy;
                drAvg[tmpDt.Columns[i].ColumnName] = sumDiff + sumEasy;

            }

            tmpDt.Rows.InsertAt(drDiff,cntDiff);
            tmpDt.Rows.InsertAt(drEasy,tmpDt.Rows.Count);
            tmpDt.Rows.InsertAt(drAvg, tmpDt.Rows.Count);


            int yearIndex;
            Double yearSum = 0;
            int yearStrt = minDt.Year;

            //연평균 계산
            for (int i = 0; i < tmpDt.Rows.Count; i++)
            {
                yearIndex = 0;
                yearSum = 0;
                yearStrt = minDt.Year;

                for (int j = 0; j < tmpDt.Columns.Count; j++)
                {
                    string sColName = tmpDt.Columns[j].ColumnName;
                    if (sColName == "MODL_CHG_LVDF_TYPE_CODE" || sColName == "EQPTID" || sColName == "EQPTNAME")
                    {
                        continue;
                    }
                    if (sColName.Length == 4) // 연평균 컬럼
                    {
                        if(sColName != yearStrt.ToString())
                        {
                            tmpDt.Rows[i][yearIndex] = Math.Round(yearSum / 12.0, 3);
                            yearStrt++;
                            yearSum = 0;                           
                        }
                        yearIndex = j;
                        continue;
                    }

                    if (sColName.Contains(yearStrt.ToString()))
                    {
                        if (!string.IsNullOrEmpty(tmpDt.Rows[i][j].ToString())) {
                            yearSum += Double.Parse(tmpDt.Rows[i][j].ToString());
                        }
                    }
                }

                if(yearSum != 0)
                {
                    tmpDt.Rows[i][yearIndex] = Math.Round(yearSum / 12.0, 3);
                }

            }

            dgModlChgReport.ItemsSource = DataTableConverter.Convert(tmpDt);
            dgModlChgReport.EndEdit();
        }
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [Func]
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


        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion


        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_COM_WIPHIST_HIST window = sender as CMM001.Popup.CMM_COM_WIPHIST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnPilotProdInfo_Click(object sender, RoutedEventArgs e)
        {

        }


        private void dgModlChgChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgModlChg.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;


            }
        }


        private void SetGridColumnCombo(C1.WPF.DataGrid.C1DataGrid grid, string columnName, string sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                (grid.Columns[columnName] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string lotId = "";
                decimal ppmValue = 0;
                string lvdf_type = "";
                decimal dayProdQty = 0;
                DateTime modlChgStrtDttm;
                DateTime modlChgEndDttm;
                string delFlag = "";
                decimal dwtmTime = 0;
                decimal sblzDwtmTime = 0;

                if (dgModlChg.ItemsSource == null)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataRow[] drSelect = DataTableConverter.Convert(dgModlChg.ItemsSource).Select("CHK = 1");

                if (drSelect.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                if (drSelect[0]["CNFM_FLAG"].ToString() == "Y")
                {
                    Util.MessageValidation("SFU1235");
                    return;
                }


                DataSet inData = new DataSet();

                DataTable inModlChgTable = inData.Tables.Add("INMODLCHG");
                inModlChgTable.Columns.Add("LOTID", typeof(string));
                inModlChgTable.Columns.Add("PPM_VALUE", typeof(decimal));
                inModlChgTable.Columns.Add("MODL_CHG_LVDF_TYPE_CODE", typeof(string));
                inModlChgTable.Columns.Add("DAY_PROD_QTY", typeof(decimal));
                inModlChgTable.Columns.Add("MODL_CHG_STRT_DTTM", typeof(DateTime));
                inModlChgTable.Columns.Add("MODL_CHG_END_DTTM", typeof(DateTime));
                inModlChgTable.Columns.Add("DEL_FLAG", typeof(string));
                inModlChgTable.Columns.Add("DWTM_TIME", typeof(decimal));
                inModlChgTable.Columns.Add("SBLZ_DWTM_TIME", typeof(decimal));

                DataRow row = null;

                row = inModlChgTable.NewRow();

                lotId = drSelect[0]["LOTID"].ToString();
                row["LOTID"] = lotId;

                ppmValue = decimal.Parse(drSelect[0]["PPM_VALUE"].ToString());
                row["PPM_VALUE"] = ppmValue;

                lvdf_type = drSelect[0]["MODL_CHG_LVDF_TYPE_CODE"].ToString();
                row["MODL_CHG_LVDF_TYPE_CODE"] = lvdf_type;

                dayProdQty = decimal.Parse(drSelect[0]["DAY_PROD_QTY"].ToString());
                row["DAY_PROD_QTY"] = dayProdQty;

                if (!string.IsNullOrEmpty(drSelect[0]["MODL_CHG_STRT_DTTM"].ToString()))
                {
                    modlChgStrtDttm = DateTime.Parse(drSelect[0]["MODL_CHG_STRT_DTTM"].ToString());
                    row["MODL_CHG_STRT_DTTM"] = modlChgStrtDttm;
                }


                if (!string.IsNullOrEmpty(drSelect[0]["MODL_CHG_END_DTTM"].ToString()))
                {
                    modlChgEndDttm = DateTime.Parse(drSelect[0]["MODL_CHG_END_DTTM"].ToString());
                    row["MODL_CHG_END_DTTM"] = modlChgEndDttm;
                }

                delFlag = drSelect[0]["DEL_FLAG"].ToString();
                row["DEL_FLAG"] = delFlag;

                dwtmTime = decimal.Parse(drSelect[0]["DWTM_TIME"].ToString());
                row["DWTM_TIME"] = dwtmTime;

                // 안정화 간 비가동시간 추가
                sblzDwtmTime = decimal.Parse(drSelect[0]["SBLZ_DWTM_TIME"].ToString());
                row["SBLZ_DWTM_TIME"] = sblzDwtmTime;

                inModlChgTable.Rows.Add(row);

                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));

                row = inDataTable.NewRow();
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                try
                {
                    new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_TB_SFC_MODL_CHG_HIST", "INMODLCHG,INDATA", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        GetModlChgHist();

                    }, inData);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCreateModlChgHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_357_CREATE_HIST popupCreateHist = new COM001_357_CREATE_HIST();
                popupCreateHist.FrameOperation = this.FrameOperation;

                popupCreateHist.Closed += new EventHandler(popupCreateHist_Closed);

                object[] parameters = new object[1];

                C1WindowExtension.SetParameters(popupCreateHist, parameters);
                grdMain.Children.Add(popupCreateHist);
                popupCreateHist.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popupCreateHist_Closed(object sender, EventArgs e)
        {
            COM001_357_CREATE_HIST window = sender as COM001_357_CREATE_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetModlChgHist();
            }
        }

        private void setComboBox(C1ComboBox cbo, ComboStatus cs, string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;


        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {

            if (dgModlChg.ItemsSource == null)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgModlChg.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (drSelect[0]["CNFM_FLAG"].ToString() == "Y")
            {
                Util.MessageValidation("SFU1235");
                return;
            }

            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");

            inDataTable.Columns.Add("CNFM_USERID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["CNFM_USERID"] = LoginInfo.USERID;
            row["LOTID"] = drSelect[0]["LOTID"].ToString();
            inDataTable.Rows.Add(row);

            try
            {
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_MODL_CHG_HIST_CNFM", "INDATA", null, (Result, ex) =>
                        {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        GetModlChgHist();

                        }, inData);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
    }
}