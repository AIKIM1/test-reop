using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// MNT001_017.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_038 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        #region Declare
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        #endregion

        #region FrameOperation
        /// <summary>
        /// 
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ELEC001_038()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
            ComboInitialize();
        }
        #endregion

        #region Combobox Initialize
        /// <summary>
        /// 
        /// </summary>
        private void ComboInitialize()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent);
        }
        #endregion

        #endregion

        #region Mehod

        #region SearchData
        /// <summary>
        /// Data를 조회하여 Grid에 바인드 한다
        /// </summary>
        private void SearchData()
        {
            try
            {
                dgLotInfo.ItemsSource = null;

                if (dtpDateFrom.SelectedDateTime.Date > DateTime.Now.Date)
                {
                    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("SUM_DATE", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("LANGID", typeof(string));


                DataRow dr = dt.NewRow();
                dr["SUM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue) == "" ? null : Util.NVC(cboProcess.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["LANGID"] = LoginInfo.LANGID;

                dt.Rows.Add(dr);
                
                DataTable result = SummaryData(new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_END_QTY_SUM_ELTR", "RQSTDT", "RSLTDT", dt));

                if (result.Rows.Count > 0)
                    Util.GridSetData(dgLotInfo, result, FrameOperation, true);
                else
                    throw new Exception("no data.");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region SummaryData
        /// <summary>
        /// DB에서 조회한 데이터를 Pivot처리한다
        /// </summary>
        /// <param name="srcDt"></param>
        /// <returns></returns>
        private DataTable SummaryData(DataTable srcDt)
        {
            DataTable newDt = new DataTable();

            Dictionary<string, string> dEltrColInfo = new Dictionary<string, string>();
            Dictionary<string, string> dEqptColInfo = new Dictionary<string, string>();
            Dictionary<string, string> wrkDttm = new Dictionary<string, string>();

            bool EltrCheck = false;


            foreach (DataRow dr in srcDt.Rows)
            {
                if (!dEltrColInfo.ContainsKey(dr["ELTR_CODE"].ToString()))
                {
                    dEltrColInfo.Add(dr["ELTR_CODE"].ToString(), dr["ELTR_TYPE"].ToString());
                }

                if (!dEqptColInfo.ContainsKey(dr["EQPTID"].ToString()))
                {
                    dEqptColInfo.Add(dr["EQPTID"].ToString(), dr["EQPTNAME"].ToString());
                }

                if (!wrkDttm.ContainsKey(dr["END_SUM_DTTM"].ToString()))
                {
                    wrkDttm.Add(dr["END_SUM_DTTM"].ToString(), dr["END_SUM_DTTM"].ToString());
                }
            }

            newDt.Columns.Add("WRK_DATE");
            newDt.Columns.Add("END_SUM_DTTM");
            newDt.Columns.Add("EQSGID");
            newDt.Columns.Add("PROCID");
            newDt.Columns.Add("PROCNAME");


            if (dEltrColInfo.Keys.Count > 1)
            {
                EltrCheck = true;
                foreach (string dEltr in dEltrColInfo.Keys)
                {
                    newDt.Columns.Add("ELTR_CODE_" + dEltrColInfo[dEltr]);
                }
                foreach (string dEqpt in dEqptColInfo.Keys)
                {
                    newDt.Columns.Add("LEFT_" + dEqptColInfo[dEqpt]);
                    newDt.Columns.Add("RIGHT_" + dEqptColInfo[dEqpt]);
                }
            }
            else
            {
                EltrCheck = false;
                foreach (string dEqpt in dEqptColInfo.Keys)
                {
                    newDt.Columns.Add("LEFT_" + dEqptColInfo[dEqpt]);
                    newDt.Columns.Add("RIGHT_" + dEqptColInfo[dEqpt]);
                }
            }

            SetGridColumn(newDt.Columns, EltrCheck);

            foreach (string dEqpt in dEqptColInfo.Keys)
            {
                if (!dEqpt.Contains("SUM"))
                {
                    newDt.Columns["LEFT_" + dEqptColInfo[dEqpt]].ColumnName = "LEFT_" + dEqptColInfo[dEqpt].Substring(0, dEqptColInfo[dEqpt].Length - 2);
                    newDt.Columns["RIGHT_" + dEqptColInfo[dEqpt]].ColumnName = "RIGHT_" + dEqptColInfo[dEqpt].Substring(0, dEqptColInfo[dEqpt].Length - 2);
                }
            }

            foreach (string wTime in wrkDttm.Keys)
            {
                DataTable tempdt = srcDt.Select("END_SUM_DTTM='" + wTime + "'").CopyToDataTable();

                DataRow newdr = newDt.NewRow();
                newdr["WRK_DATE"] = tempdt.Rows[0]["WRK_DATE"];
                newdr["END_SUM_DTTM"] = tempdt.Rows[0]["END_SUM_DTTM"];
                newdr["EQSGID"] = tempdt.Rows[0]["EQSGID"];
                newdr["PROCID"] = tempdt.Rows[0]["PROCID"];
                newdr["PROCNAME"] = tempdt.Rows[0]["PROCNAME"];


                if (EltrCheck == true)
                {
                    foreach (string dEltr in dEltrColInfo.Keys)
                    {
                        newdr["ELTR_CODE_" + dEltrColInfo[dEltr]] = dEltrColInfo[dEltr];
                    }
                }
                foreach (DataRow dr in tempdt.Rows)
                {
                    if (dr["EQPTNAME"].ToString().Contains("SUM"))
                    {
                        newdr["LEFT_" + dr["EQPTNAME"].ToString()] = radRoll.IsChecked == true ? dr["HOURLY_PLAN_QTY"] : dr["HOURLY_PLAN_QTY2"];
                        newdr["RIGHT_" + dr["EQPTNAME"].ToString()] = radRoll.IsChecked == true ? dr["SUM_END_QTY"] : dr["SUM_END_QTY2"];
                    }
                    else
                    {
                        newdr["LEFT_" + dr["EQPTNAME"].ToString().Substring(0, dr["EQPTNAME"].ToString().Length - 2)] = radRoll.IsChecked == true ? dr["HOURLY_PLAN_QTY"] : dr["HOURLY_PLAN_QTY2"];
                        newdr["RIGHT_" + dr["EQPTNAME"].ToString().Substring(0, dr["EQPTNAME"].ToString().Length - 2)] = radRoll.IsChecked == true ? dr["SUM_END_QTY"] : dr["SUM_END_QTY2"];
                    }
                }

                newDt.Rows.Add(newdr);

            }


            return newDt;
        }
        #endregion

        #region Set Grid Columns
        /// <summary>
        /// Grid의 Column 동적 세팅
        /// </summary>
        /// <param name="dcc"></param>
        private void SetGridColumn(DataColumnCollection dcc, bool EltrCheck)
        {
            dgLotInfo.Columns.Clear();

            if (EltrCheck == true)
            {
                foreach (DataColumn dc in dcc)
                {
                    C1.WPF.DataGrid.DataGridTextColumn TextCol = new C1.WPF.DataGrid.DataGridTextColumn();

                    if (dc.ColumnName.Contains("ELTR_CODE_"))
                    {
                        List<string> args;
                        if (dc.ColumnName.Substring(dc.ColumnName.Length - 2, 1).ToUpper() == "C" || dc.ColumnName.Substring(dc.ColumnName.Length - 2, 1).ToUpper() == "A")
                        {
                            foreach (DataColumn dr in dcc)
                            {


                                C1.WPF.DataGrid.DataGridNumericColumn NumericCol = new C1.WPF.DataGrid.DataGridNumericColumn();
                                if (dr.ColumnName.Contains("LEFT_") && (dr.ColumnName.Substring(dr.ColumnName.Length - 1, 1).ToUpper() == dc.ColumnName.Substring(dc.ColumnName.Length - 2, 1).ToUpper()))
                                {
                                    args = new List<string>();
                                    args.Add(dc.ColumnName.Replace("ELTR_CODE_", "").ToUpper());
                                    args.Add(dr.ColumnName.Substring(0, dr.ColumnName.Length - 2).Replace("LEFT_", "").ToUpper());
                                    args.Add("PLAN_QTY");
                                    NumericCol.Header = args;
                                    NumericCol.HorizontalAlignment = HorizontalAlignment.Right;
                                    NumericCol.Format = "#,##0";
                                    NumericCol.MaxWidth = 80;
                                    if (dr.ColumnName.Contains("SUM"))
                                    {
                                        NumericCol.Binding = new System.Windows.Data.Binding(dr.ColumnName);
                                    }
                                    else
                                    {
                                        NumericCol.Binding = new System.Windows.Data.Binding(dr.ColumnName.Substring(0, dr.ColumnName.Length - 2));
                                    }

                                    dgLotInfo.Columns.Add(NumericCol);
                                }
                                if (dr.ColumnName.Contains("RIGHT_") && (dr.ColumnName.Substring(dr.ColumnName.Length - 1, 1).ToUpper() == dc.ColumnName.Substring(dc.ColumnName.Length - 2, 1).ToUpper()))
                                {
                                    args = new List<string>();
                                    args.Add(dc.ColumnName.Replace("ELTR_CODE_", "").ToUpper());
                                    args.Add(dr.ColumnName.Substring(0, dr.ColumnName.Length - 2).Replace("RIGHT_", "").ToUpper());
                                    args.Add("END_QTY");
                                    NumericCol.Header = args;
                                    NumericCol.HorizontalAlignment = HorizontalAlignment.Right;
                                    NumericCol.Format = "#,##0";
                                    NumericCol.MaxWidth = 80;
                                    if (dr.ColumnName.Contains("SUM"))
                                    {
                                        NumericCol.Binding = new System.Windows.Data.Binding(dr.ColumnName);
                                    }
                                    else
                                    {
                                        NumericCol.Binding = new System.Windows.Data.Binding(dr.ColumnName.Substring(0, dr.ColumnName.Length - 2));
                                    }
                                    dgLotInfo.Columns.Add(NumericCol);
                                }
                            }
                        }
                    }
                    else if (dc.ColumnName.Contains("WRK_DATE") || dc.ColumnName.Contains("EQSGID") || dc.ColumnName.Contains("PROCID") || dc.ColumnName.Contains("PROCNAME"))
                    {
                        TextCol.Header = dc.ColumnName;
                        TextCol.HorizontalAlignment = HorizontalAlignment.Left;
                        TextCol.Visibility = Visibility.Collapsed;
                        TextCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        dgLotInfo.Columns.Add(TextCol);
                    }
                    else if (!dc.ColumnName.Contains("ELTR_CODE_") && !dc.ColumnName.Contains("LEFT_") && !dc.ColumnName.Contains("RIGHT_"))
                    {
                        TextCol.Header = dc.ColumnName;
                        TextCol.HorizontalAlignment = HorizontalAlignment.Left;
                        TextCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        dgLotInfo.Columns.Add(TextCol);
                    }

                    if (dgLotInfo.TopRows.Count <= 2)
                    {
                        dgLotInfo.TopRows.Add(new C1.WPF.DataGrid.DataGridColumnHeaderRow());
                    }
                    dgLotInfo.Refresh();
                }
            }
            else
            {
                foreach (DataColumn dc in dcc)
                {
                    C1.WPF.DataGrid.DataGridTextColumn TextCol = new C1.WPF.DataGrid.DataGridTextColumn();
                    C1.WPF.DataGrid.DataGridNumericColumn NumericCol = new C1.WPF.DataGrid.DataGridNumericColumn();
                    if (dc.ColumnName.Contains("LEFT_"))
                    {
                        List<string> args = new List<string>();
                        args.Add(dc.ColumnName.Substring(0, dc.ColumnName.Length - 2).Replace("LEFT_", "").ToUpper());
                        args.Add("PLAN_QTY");
                        NumericCol.Header = args;
                        NumericCol.HorizontalAlignment = HorizontalAlignment.Right;
                        NumericCol.Format = "#,##0";
                        NumericCol.MaxWidth = 80;
                        if (dc.ColumnName.Contains("SUM"))
                        {
                            NumericCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        }
                        else
                        {
                            NumericCol.Binding = new System.Windows.Data.Binding(dc.ColumnName.Substring(0, dc.ColumnName.Length - 2).ToUpper());
                        }
                        dgLotInfo.Columns.Add(NumericCol);
                    }
                    else if (dc.ColumnName.Contains("RIGHT_"))
                    {
                        List<string> args = new List<string>();
                        args.Add(dc.ColumnName.Substring(0, dc.ColumnName.Length - 2).Replace("RIGHT_", "").ToUpper());
                        args.Add("END_QTY");
                        NumericCol.Header = args;
                        NumericCol.HorizontalAlignment = HorizontalAlignment.Right;
                        NumericCol.Format = "#,##0";
                        NumericCol.MaxWidth = 80;
                        if (dc.ColumnName.Contains("SUM"))
                        {
                            NumericCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        }
                        else
                        {
                            NumericCol.Binding = new System.Windows.Data.Binding(dc.ColumnName.Substring(0, dc.ColumnName.Length - 2).ToUpper());
                        }
                        dgLotInfo.Columns.Add(NumericCol);
                    }
                    else if (dc.ColumnName.Contains("WRK_DATE") || dc.ColumnName.Contains("EQSGID"))
                    {
                        TextCol.Header = dc.ColumnName;
                        TextCol.HorizontalAlignment = HorizontalAlignment.Left;
                        TextCol.Visibility = Visibility.Collapsed;
                        TextCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        dgLotInfo.Columns.Add(TextCol);
                    }
                    else
                    {
                        TextCol.Header = dc.ColumnName;
                        TextCol.HorizontalAlignment = HorizontalAlignment.Left;
                        TextCol.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                        dgLotInfo.Columns.Add(TextCol);
                    }

                    if (dgLotInfo.TopRows.Count == 3)
                    {
                        dgLotInfo.TopRows.Remove(dgLotInfo.TopRows[dgLotInfo.TopRows.Count - 1]);
                    }
                    dgLotInfo.Refresh();

                }
            }
        }
        #endregion

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return; ;

            SearchData();
        }

        private void radRoll_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null || dgLotInfo == null)
                return;

            SearchData();
        }
        #endregion
    }
}
