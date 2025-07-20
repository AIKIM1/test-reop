/*************************************************************************************
 Created Date : 2020.12.28
      Creator : KANG DONG HEE
   Decription : 수동실적현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.28 : Initial Created.
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using System.Linq;
using System.Windows.Threading;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_076 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private readonly string COLUMNS_LIST = "WRK_DATE,SHFT_ID,WRK_DATE1,EQP_NAME,LOT_TYPE,LOT_COMMENT,PROD_LOTID,LOT_ATTR,INPUT_QTY,GOOD_QTY,DFCT_QTY";
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        public FCS002_076()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            //Combo Setting
            InitCombo();
            //Control Setting
            InitControl();

            AddDefectHeader();
            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            C1ComboBox[] cbChild = { cboEqp, cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LINE", cbChild: cbChild);

            object[] oFilter = { "D,5", cboLine };
            ComCombo.SetComboObjParent(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPID", objParent: oFilter);

            string[] sFilterShift = { "COMBO_SHIFT" };
            ComCombo.SetCombo(cboShift, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilterShift);

            string[] sFilterFlag = { "WRKLOG_TYPE_CODE" };
            ComCombo.SetCombo(cboFlag, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterFlag);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

        }
        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
        }

        // 공통함수로 뺄지 확인 필요 START
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

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
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
        // 공통함수로 뺄지 확인 필요 END

        #endregion

        #region Event
        private void cboFlag_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            AddDefectHeader();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                dr["TO_DATE"] = Util.GetCondition(dtpToDate);
                dr["EQSGID"] = Util.GetCondition(cboLine);
                dr["SHFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["WRKLOG_TYPE_CODE"] = Util.GetCondition(cboFlag);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WORKSHEET_BY_WORKDETAIL_TOTAL_MB", "RQSTDT", "RSLTDT", dtRqst);

                //충방의 경우 설비 구분없이 라인별로만 입력 됨
                if (Util.GetCondition(cboFlag).Equals("A") || Util.GetCondition(cboFlag).Equals("B"))
                {
                    dgMaintPf.Columns["EQP_NAME"].Visibility = Visibility.Collapsed;

                    //pivot 처리
                    //pivot 의 row 만들기용 distinct
                    DataTable dtPivot = new DataView(dtRslt).ToTable(true, new string[] { "WRK_DATE", "SHFT_ID", "LOT_TYPE", "LOT_COMMENT", "PROD_LOTID", "LOT_ATTR", "INPUT_QTY", "GOOD_QTY" });

                    for (int i = dgMaintPf.Columns["DFCT_QTY"].Index + 1; i < dgMaintPf.Columns.Count; i++) //스프레드 data_field 사용하기
                    {
                        dtPivot.Columns.Add(dgMaintPf.Columns[i].Name, typeof(Int32));
                    }

                    //pk 지정
                    dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["WRK_DATE"], dtPivot.Columns["SHFT_ID"], dtPivot.Columns["LOT_TYPE"], dtPivot.Columns["LOT_COMMENT"], dtPivot.Columns["PROD_LOTID"], dtPivot.Columns["LOT_ATTR"], dtPivot.Columns["INPUT_QTY"], dtPivot.Columns["GOOD_QTY"] };

                    object[] oKeyVal = new object[8];

                    DataView dvPivotData = new DataView(dtRslt);
                    dvPivotData.RowFilter = "PROD_LOTID IS NOT NULL";

                    DataTable dtPivotData = dvPivotData.ToTable();

                    for (int i = 0; i < dtPivotData.Rows.Count; i++)
                    {
                        // Set the values of the keys to find.
                        oKeyVal[0] = dtPivotData.Rows[i]["WRK_DATE"].ToString();
                        oKeyVal[1] = dtPivotData.Rows[i]["SHFT_ID"].ToString();
                        oKeyVal[2] = dtPivotData.Rows[i]["LOT_TYPE"].ToString();
                        oKeyVal[3] = dtPivotData.Rows[i]["LOT_COMMENT"].ToString();
                        oKeyVal[4] = dtPivotData.Rows[i]["PROD_LOTID"].ToString();
                        oKeyVal[5] = dtPivotData.Rows[i]["LOT_ATTR"].ToString();
                        oKeyVal[6] = dtPivotData.Rows[i]["INPUT_QTY"].ToString();
                        oKeyVal[7] = dtPivotData.Rows[i]["GOOD_QTY"].ToString();

                        DataRow drPivot = dtPivot.Rows.Find(oKeyVal);

                        if (dtPivot.Columns.Contains("DEFECT_" + dtPivotData.Rows[i]["DFCT_CODE"]))
                        {
                            drPivot["DEFECT_" + dtPivotData.Rows[i]["DFCT_CODE"]] = dtPivotData.Rows[i]["DFCT_QTY"];
                        }
                    }

                    dtPivot.Columns["WRK_DATE"].AllowDBNull = true;
                    dtPivot.Columns["SHFT_ID"].AllowDBNull = true;
                    dtPivot.Columns["LOT_TYPE"].AllowDBNull = true;
                    dtPivot.Columns["LOT_COMMENT"].AllowDBNull = true;
                    dtPivot.Columns["PROD_LOTID"].AllowDBNull = true;
                    dtPivot.Columns["LOT_ATTR"].AllowDBNull = true;
                    dtPivot.Columns["INPUT_QTY"].AllowDBNull = true;
                    dtPivot.Columns["GOOD_QTY"].AllowDBNull = true;

                    dtPivot.PrimaryKey = null;
                    dtPivot.Constraints.Clear();

                    Util.GridSetData(dgMaintPf, dtPivot, FrameOperation, true);

                    //fpsMaintPf.SumBottom("WRK_DATE1", 1, ObjectDic.GetObjectName("TOTAL"), sSumHeaderCol: "LOT_ATTR");
                }
                else
                {
                    dgMaintPf.Columns["EQP_NAME"].Visibility = Visibility.Visible;

                    //pivot 처리
                    //pivot 의 row 만들기용 distinct
                    DataTable dtPivot = new DataView(dtRslt).ToTable(true, new string[] { "WRK_DATE", "SHFT_ID", "EQP_NAME", "LOT_TYPE", "LOT_COMMENT", "PROD_LOTID", "LOT_ATTR", "INPUT_QTY", "GOOD_QTY" });

                    for (int i = dgMaintPf.Columns["DFCT_QTY"].Index + 1; i < dgMaintPf.Columns.Count; i++) //스프레드 data_field 사용하기
                    {
                        dtPivot.Columns.Add(dgMaintPf.Columns[i].Name, typeof(Int32));
                    }

                    //pk 지정
                    dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["WRK_DATE"], dtPivot.Columns["SHFT_ID"], dtPivot.Columns["EQP_NAME"], dtPivot.Columns["LOT_TYPE"], dtPivot.Columns["LOT_COMMENT"], dtPivot.Columns["PROD_LOTID"], dtPivot.Columns["LOT_ATTR"], dtPivot.Columns["INPUT_QTY"], dtPivot.Columns["GOOD_QTY"] };

                    object[] oKeyVal = new object[9];

                    DataView dvPivotData = new DataView(dtRslt);
                    dvPivotData.RowFilter = "PROD_LOTID IS NOT NULL";

                    DataTable dtPivotData = dvPivotData.ToTable();

                    for (int i = 0; i < dtPivotData.Rows.Count; i++)
                    {
                        // Set the values of the keys to find.
                        oKeyVal[0] = dtPivotData.Rows[i]["WRK_DATE"].ToString();
                        oKeyVal[1] = dtPivotData.Rows[i]["SHFT_ID"].ToString();
                        oKeyVal[2] = dtPivotData.Rows[i]["EQP_NAME"].ToString();
                        oKeyVal[3] = dtPivotData.Rows[i]["LOT_TYPE"].ToString();
                        oKeyVal[4] = dtPivotData.Rows[i]["LOT_COMMENT"].ToString();
                        oKeyVal[5] = dtPivotData.Rows[i]["PROD_LOTID"].ToString();
                        oKeyVal[6] = dtPivotData.Rows[i]["LOT_ATTR"].ToString();
                        oKeyVal[7] = dtPivotData.Rows[i]["INPUT_QTY"].ToString();
                        oKeyVal[8] = dtPivotData.Rows[i]["GOOD_QTY"].ToString();

                        DataRow drPivot = dtPivot.Rows.Find(oKeyVal);

                        if (dtPivot.Columns.Contains("DEFECT_" + dtPivotData.Rows[i]["DFCT_CODE"]))
                        {
                            drPivot["DEFECT_" + dtPivotData.Rows[i]["DFCT_CODE"]] = dtPivotData.Rows[i]["DFCT_QTY"];
                        }
                    }

                    dtPivot.Columns["WRK_DATE"].AllowDBNull = true;
                    dtPivot.Columns["SHFT_ID"].AllowDBNull = true;
                    dtPivot.Columns["EQP_NAME"].AllowDBNull = true;
                    dtPivot.Columns["LOT_TYPE"].AllowDBNull = true;
                    dtPivot.Columns["LOT_COMMENT"].AllowDBNull = true;
                    dtPivot.Columns["PROD_LOTID"].AllowDBNull = true;
                    dtPivot.Columns["LOT_ATTR"].AllowDBNull = true;
                    dtPivot.Columns["INPUT_QTY"].AllowDBNull = true;
                    dtPivot.Columns["GOOD_QTY"].AllowDBNull = true;

                    dtPivot.PrimaryKey = null;
                    dtPivot.Constraints.Clear();

                    Util.GridSetData(dgMaintPf, dtPivot, FrameOperation, true);
                    //fpsMaintPf.SumBottom("WRK_DATE1", 1, ObjectDic.GetObjectName("TOTAL"), sSumHeaderCol: "LOT_ATTR");
                }

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "WRK_DATE", "SHFT_ID" };
                    _Util.SetDataGridMergeExtensionCol(dgMaintPf, sColumnName, DataGridMergeMode.VERTICAL);

                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["WRK_DATE"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["SHFT_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["WRK_DATE1"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["EQP_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["LOT_TYPE"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["LOT_COMMENT"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["PROD_LOTID"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["LOT_ATTR"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["GOOD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
                    DataGridAggregate.SetAggregateFunctions(dgMaintPf.Columns["DFCT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum()});
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
        }

        private void AddDefectHeader()
        {
            Util.gridClear(dgMaintPf);

            string sColNameList = string.Empty;
                ;
            for (int i = 0; i < dgMaintPf.Columns.Count; i++)
            {
                sColNameList = sColNameList + Util.NVC(dgMaintPf.Columns[i].Name) + ",";
            }

            sColNameList = sColNameList.Remove(sColNameList.LastIndexOf(","));

            string[] sColList = sColNameList.Split(',');

            for (int i = 0; i < sColList.Length; i++)
            {
                if (!COLUMNS_LIST.Contains(sColList[i].ToString()))
                {
                    dgMaintPf.Columns.RemoveAt(dgMaintPf.Columns[sColList[i].ToString()].Index);
                }
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("USE_FLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["DFCT_GR_TYPE_CODE"] = Util.GetCondition(cboFlag);
            dr["USE_FLAG"] = "Y";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_CELL_DEFECT_MB", "RQSTDT", "RSLTDT", dtRqst);

            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                string sDefCode = dtRslt.Rows[i]["DFCT_CODE"].ToString();
                string sGrpName = dtRslt.Rows[i]["GROUP_NAME"].ToString();
                string SDefName = dtRslt.Rows[i]["DFCT_NAME"].ToString();

                dgMaintPf.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "DEFECT_" + sDefCode,
                    Header = new string[] { sGrpName , SDefName }.ToList<string>(),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("DEFECT_" + sDefCode),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Format = "#,##0"
                });

                //string sColName = dgMaintPf.Columns[i].ActualGroupHeader.ToString();
                //string[] COLNAME = sColName.Replace("[#]", "").Split(',');
                //dgMaintPf.Columns[i].Header = COLNAME[0] + "," + COLNAME[1];

            }

//            for (int k = 0; k < dgMaintPf.TopRows.Count; k++)
//            {

//                for (int i = 0; i < dgMaintPf.Columns.Count; i++)
//                {
//                    string sColName = dgMaintPf.Columns[i].ActualGroupHeader.ToString();
//                    string[] COLNAME = sColName.Replace("[#]", "").Split(',');

//                    dgMaintPf.TopRows[k].DataGrid.Columns[i].Header = COLNAME[0] + "," + COLNAME[1];


////                    dgMaintPf.Columns[i].Header = COLNAME[0] + "," + COLNAME[1];
//                }
//            }

            if (dtRslt.Rows.Count > 0)
            {
                //fp.ActiveSheet.ReferenceStyle = FarPoint.Win.Spread.Model.ReferenceStyle.R1C1;
                //fp.ActiveSheet.SetFormula(fp.ActiveSheet.RowCount - 1, fp.GetColumnIndex("GOOD_QTY"), "RC[-1] - RC[1]");
                //fp.ActiveSheet.SetFormula(fp.ActiveSheet.RowCount - 1, fp.GetColumnIndex("DFCT_QTY"), "SUM(RC[1]:RC[" + dtRslt.Rows.Count + "])");
            }
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
        #endregion

        private void dgMaintPf_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {

                e.Column.HeaderPresenter.Content = null;

                TextBlock tb = new TextBlock();

                tb.Text = tb.Text.Remove(0).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Column.HeaderPresenter.Content = tb;
            }));
        }

    }
}
