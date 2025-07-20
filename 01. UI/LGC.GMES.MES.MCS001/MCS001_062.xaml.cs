/*************************************************************************************
 Created Date : 2021.04.13
      Creator : 김대근
   Decription : STK 적재율 Report
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.13  김대근 사원 : Initial Created.    

**************************************************************************************/
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using System;
using C1.WPF.DataGrid;
using System.Windows.Media;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_062.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_062 : UserControl
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_062()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbArea.Text = LoginInfo.CFG_AREA_NAME;
            SetCombo(cboStockerType);
            SetCombo(cboElectrodeType);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            GetStockerStatus();
        }

        private void dgStk_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null || e == null || e.Cell == null || e.Cell.Presenter == null)
            {
                return;
            }

            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    int rowIndex = e.Cell.Row.Index;
                    DataRowView drv = dg.Rows[rowIndex].DataItem as DataRowView;

                    if (drv != null)
                    {
                        if (!e.Cell.Column.Name.Equals("STKGROUP")
                        && !e.Cell.Column.Name.Equals("STK_QTY")
                        && !e.Cell.Column.Name.Equals("STK_RACK_QTY"))
                        {
                            if (dg == null || e == null || e.Cell == null || e.Cell.Presenter == null)
                            {
                                return;
                            }

                            if (Util.NVC(DataTableConverter.GetValue(drv, "SUM_TYPE")).Equals("SUB"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Coral);
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(drv, "SUM_TYPE")).Equals("TOT"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }

        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }
        #endregion

        #region Mehod
        private void SetCombo(C1ComboBox cbo)
        {
            if (cbo == null)
            {
                return;
            }

            string bizRuleName = string.Empty;
            string[] arrColumn = null;
            string[] arrCondition = null;
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            string selectedValue = string.Empty;
            CommonCombo.ComboStatus status = CommonCombo.ComboStatus.ALL;

            switch (cbo.Name)
            {
                case "cboArea":
                    bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
                    arrColumn = new string[] { "LANGID", "AREAID" };
                    arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
                    selectedValue = LoginInfo.CFG_AREA_ID;
                    break;
                case "cboStockerType":
                    bizRuleName = "BR_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
                    arrColumn = new string[] { "LANGID", "AREAID", "ATTR1", "ATTR2", "CFG_AREA_ID" };
                    arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "Y", null, LoginInfo.CFG_AREA_ID };
                    break;
                case "cboElectrodeType":
                    bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
                    arrColumn = new string[] { "LANGID", "CMCDTYPE" };
                    arrCondition = new string[] { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
                    break;
                default:
                    break;
            }

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, status, selectedValueText, displayMemberText, selectedValue);
        }

        private DataTable GetSummaryAndAverage(DataTable dt)
        {
            if(dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            DataTable result = dt.Copy();
            result.Columns.Add("SUM_TYPE", typeof(string));

            int index = 0;
            while (true)
            {
                if(index == result.Rows.Count)
                {
                    break;
                }
                string stkGroup = result.Rows[index]["STKGROUP"].ToString();
                DataRow drCstUsing = result.NewRow();
                DataRow drCstEmpty = result.NewRow();
                DataRow drTotal = result.NewRow();

                for (int i = index; i < result.Rows.Count; i++)
                {
                    if (Util.NVC(result.Rows[i]["SUM_TYPE"]).Equals("SUB"))
                    {
                        continue;
                    }

                    int insertIdx = i;
                    if (Util.NVC(result.Rows[i]["EMPTY_FLAG"]).Equals("U"))
                    {
                        drCstUsing["TOTAL_QTY"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                        drCstUsing["MASS_QTY"] = Util.NVC_Int(drCstUsing["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                        drCstUsing["MASS_TYPE1"] = Util.NVC_Int(drCstUsing["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                        drCstUsing["MASS_TYPE2"] = Util.NVC_Int(drCstUsing["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                        drCstUsing["MASS_TYPE3"] = Util.NVC_Int(drCstUsing["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                        drCstUsing["OTHER_QTY"] = Util.NVC_Int(drCstUsing["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                        drCstUsing["OTHER_TYPE1"] = Util.NVC_Int(drCstUsing["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                        drCstUsing["OTHER_TYPE2"] = Util.NVC_Int(drCstUsing["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                        drCstUsing["OTHER_TYPE3"] = Util.NVC_Int(drCstUsing["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                        drCstUsing["HOLD_QTY"] = Util.NVC_Int(drCstUsing["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                        drCstUsing["HOLD_QA"] = Util.NVC_Int(drCstUsing["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                        drCstUsing["HOLD_TYPE1"] = Util.NVC_Int(drCstUsing["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                        drCstUsing["HOLD_TYPE2"] = Util.NVC_Int(drCstUsing["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                        drCstUsing["HOLD_TYPE3"] = Util.NVC_Int(drCstUsing["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);

                        if ((i + 1) == result.Rows.Count
                            || Util.NVC(result.Rows[i + 1]["EMPTY_FLAG"]).Equals("E")
                            || !Util.NVC(result.Rows[i + 1]["STKGROUP"]).Equals(stkGroup))
                        {
                            drCstUsing["STKGROUP"] = stkGroup;
                            drCstUsing["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                            drCstUsing["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                            drCstUsing["EMPTY_FLAG"] = "SUB_TOTAL";
                            drCstUsing["PRJT_NAME"] = string.Empty;
                            drCstUsing["ELTR_TYPE"] = string.Empty;

                            drCstUsing["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                            drCstUsing["MASS_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["MASS_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                            drCstUsing["OTHER_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["OTHER_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                            drCstUsing["HOLD_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["HOLD_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                            drCstUsing["SUM_TYPE"] = "SUB";

                            insertIdx += 1;
                            result.Rows.InsertAt(drCstUsing, insertIdx);
                        }
                    }

                    if (result.Rows[i]["EMPTY_FLAG"].ToString().Equals("E"))
                    {
                        drCstEmpty["TOTAL_QTY"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                        drCstEmpty["MASS_QTY"] = Util.NVC_Int(drCstEmpty["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                        drCstEmpty["MASS_TYPE1"] = Util.NVC_Int(drCstEmpty["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                        drCstEmpty["MASS_TYPE2"] = Util.NVC_Int(drCstEmpty["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                        drCstEmpty["MASS_TYPE3"] = Util.NVC_Int(drCstEmpty["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                        drCstEmpty["OTHER_QTY"] = Util.NVC_Int(drCstEmpty["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                        drCstEmpty["OTHER_TYPE1"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                        drCstEmpty["OTHER_TYPE2"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                        drCstEmpty["OTHER_TYPE3"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                        drCstEmpty["HOLD_QTY"] = Util.NVC_Int(drCstEmpty["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                        drCstEmpty["HOLD_QA"] = Util.NVC_Int(drCstEmpty["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                        drCstEmpty["HOLD_TYPE1"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                        drCstEmpty["HOLD_TYPE2"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                        drCstEmpty["HOLD_TYPE3"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);

                        if ((i + 1) == result.Rows.Count
                            || Util.NVC(result.Rows[i + 1]["EMPTY_FLAG"]).Equals("U")
                            || !Util.NVC(result.Rows[i + 1]["STKGROUP"]).Equals(stkGroup))
                        {
                            drCstEmpty["STKGROUP"] = stkGroup;
                            drCstEmpty["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                            drCstEmpty["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                            drCstEmpty["EMPTY_FLAG"] = "SUB_TOTAL";
                            drCstEmpty["PRJT_NAME"] = string.Empty;
                            drCstEmpty["ELTR_TYPE"] = string.Empty;

                            drCstEmpty["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                            drCstEmpty["MASS_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["MASS_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                            drCstEmpty["OTHER_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["OTHER_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                            drCstEmpty["HOLD_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["HOLD_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                            drCstEmpty["SUM_TYPE"] = "SUB";

                            insertIdx += 1;
                            result.Rows.InsertAt(drCstEmpty, insertIdx);
                        }
                    }

                    if (insertIdx + 1 == result.Rows.Count || !Util.NVC(result.Rows[insertIdx + 1]["STKGROUP"]).Equals(stkGroup))
                    {
                        drTotal["STKGROUP"] = stkGroup;
                        drTotal["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                        drTotal["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drTotal["EMPTY_FLAG"] = "TOTAL";
                        drTotal["PRJT_NAME"] = string.Empty;
                        drTotal["ELTR_TYPE"] = string.Empty;

                        drTotal["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drTotal["MASS_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["MASS_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                        drTotal["OTHER_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["OTHER_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                        drTotal["HOLD_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["HOLD_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                        drTotal["SUM_TYPE"] = "TOT";

                        insertIdx += 1;
                        result.Rows.InsertAt(drTotal, insertIdx);
                        index = insertIdx + 1;
                        break;
                    }
                    else
                    {
                        drTotal["TOTAL_QTY"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                        drTotal["MASS_QTY"] = Util.NVC_Int(drTotal["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                        drTotal["MASS_TYPE1"] = Util.NVC_Int(drTotal["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                        drTotal["MASS_TYPE2"] = Util.NVC_Int(drTotal["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                        drTotal["MASS_TYPE3"] = Util.NVC_Int(drTotal["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                        drTotal["OTHER_QTY"] = Util.NVC_Int(drTotal["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                        drTotal["OTHER_TYPE1"] = Util.NVC_Int(drTotal["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                        drTotal["OTHER_TYPE2"] = Util.NVC_Int(drTotal["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                        drTotal["OTHER_TYPE3"] = Util.NVC_Int(drTotal["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                        drTotal["HOLD_QTY"] = Util.NVC_Int(drTotal["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                        drTotal["HOLD_QA"] = Util.NVC_Int(drTotal["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                        drTotal["HOLD_TYPE1"] = Util.NVC_Int(drTotal["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                        drTotal["HOLD_TYPE2"] = Util.NVC_Int(drTotal["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                        drTotal["HOLD_TYPE3"] = Util.NVC_Int(drTotal["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);
                    }
                }
            }

            return result;
        }

        private DataTable GetCstUsingSumAndAvg(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            DataTable result = dt.Copy();

            DataRow drCstUsing = result.NewRow();
            for (int i = 0; i < result.Rows.Count; i++)
            {
                if (Util.NVC(result.Rows[i]["EMPTY_FLAG"]).Equals("U"))
                {
                    drCstUsing["TOTAL_QTY"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                    drCstUsing["MASS_QTY"] = Util.NVC_Int(drCstUsing["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                    drCstUsing["MASS_TYPE1"] = Util.NVC_Int(drCstUsing["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                    drCstUsing["MASS_TYPE2"] = Util.NVC_Int(drCstUsing["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                    drCstUsing["MASS_TYPE3"] = Util.NVC_Int(drCstUsing["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                    drCstUsing["OTHER_QTY"] = Util.NVC_Int(drCstUsing["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                    drCstUsing["OTHER_TYPE1"] = Util.NVC_Int(drCstUsing["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                    drCstUsing["OTHER_TYPE2"] = Util.NVC_Int(drCstUsing["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                    drCstUsing["OTHER_TYPE3"] = Util.NVC_Int(drCstUsing["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                    drCstUsing["HOLD_QTY"] = Util.NVC_Int(drCstUsing["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                    drCstUsing["HOLD_QA"] = Util.NVC_Int(drCstUsing["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                    drCstUsing["HOLD_TYPE1"] = Util.NVC_Int(drCstUsing["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                    drCstUsing["HOLD_TYPE2"] = Util.NVC_Int(drCstUsing["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                    drCstUsing["HOLD_TYPE3"] = Util.NVC_Int(drCstUsing["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);

                    if ((i + 1) == result.Rows.Count
                    || !Util.NVC(result.Rows[i]["EMPTY_FLAG"]).Equals(Util.NVC(result.Rows[i + 1]["EMPTY_FLAG"]))
                    || !Util.NVC(result.Rows[i]["STKGROUP"]).Equals(Util.NVC(result.Rows[i+1]["STKGROUP"])))
                    {
                        drCstUsing["STKGROUP"] = Util.NVC(result.Rows[i]["STKGROUP"]);
                        drCstUsing["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                        drCstUsing["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drCstUsing["EMPTY_FLAG"] = "Sub Total_" + Util.NVC(result.Rows[i]["TOTNUM"]).Replace("TOT", "") + "U";
                        drCstUsing["PRJT_NAME"] = string.Empty;
                        drCstUsing["ELTR_TYPE"] = string.Empty;
                        drCstUsing["TOTNUM"] = Util.NVC(result.Rows[i]["TOTNUM"]);

                        drCstUsing["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drCstUsing["MASS_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["MASS_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                        drCstUsing["OTHER_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["OTHER_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                        drCstUsing["HOLD_RATE"] = Util.NVC_Int(drCstUsing["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstUsing["HOLD_QTY"]) * 100 / Util.NVC_Int(drCstUsing["TOTAL_QTY"]);
                        drCstUsing["SUM_TYPE"] = "SUB";

                        result.Rows.InsertAt(drCstUsing, i + 1);
                        i += 1;
                        drCstUsing = result.NewRow();
                    }
                }
            }

            return result;
        }

        private DataTable GetCstEmptySumAndAvg(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            DataTable result = dt.Copy();
            DataRow drCstEmpty = result.NewRow();

            for (int i = 0; i < result.Rows.Count; i++)
            {
                if (Util.NVC(result.Rows[i]["EMPTY_FLAG"]).Equals("E"))
                {
                    drCstEmpty["TOTAL_QTY"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                    drCstEmpty["MASS_QTY"] = Util.NVC_Int(drCstEmpty["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                    drCstEmpty["MASS_TYPE1"] = Util.NVC_Int(drCstEmpty["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                    drCstEmpty["MASS_TYPE2"] = Util.NVC_Int(drCstEmpty["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                    drCstEmpty["MASS_TYPE3"] = Util.NVC_Int(drCstEmpty["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                    drCstEmpty["OTHER_QTY"] = Util.NVC_Int(drCstEmpty["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                    drCstEmpty["OTHER_TYPE1"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                    drCstEmpty["OTHER_TYPE2"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                    drCstEmpty["OTHER_TYPE3"] = Util.NVC_Int(drCstEmpty["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                    drCstEmpty["HOLD_QTY"] = Util.NVC_Int(drCstEmpty["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                    drCstEmpty["HOLD_QA"] = Util.NVC_Int(drCstEmpty["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                    drCstEmpty["HOLD_TYPE1"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                    drCstEmpty["HOLD_TYPE2"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                    drCstEmpty["HOLD_TYPE3"] = Util.NVC_Int(drCstEmpty["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);

                    if ((i + 1) == result.Rows.Count
                    || !Util.NVC(result.Rows[i]["EMPTY_FLAG"]).Equals(Util.NVC(result.Rows[i + 1]["EMPTY_FLAG"]))
                    || !Util.NVC(result.Rows[i]["STKGROUP"]).Equals(Util.NVC(result.Rows[i + 1]["STKGROUP"])))
                    {
                        drCstEmpty["STKGROUP"] = Util.NVC(result.Rows[i]["STKGROUP"]);
                        drCstEmpty["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                        drCstEmpty["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drCstEmpty["EMPTY_FLAG"] = "Sub Total_" + Util.NVC(result.Rows[i]["TOTNUM"]).Replace("TOT", "") + "E";
                        drCstEmpty["PRJT_NAME"] = string.Empty;
                        drCstEmpty["ELTR_TYPE"] = string.Empty;
                        drCstEmpty["TOTNUM"] = Util.NVC(result.Rows[i]["TOTNUM"]);

                        drCstEmpty["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                        drCstEmpty["MASS_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["MASS_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                        drCstEmpty["OTHER_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["OTHER_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                        drCstEmpty["HOLD_RATE"] = Util.NVC_Int(drCstEmpty["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drCstEmpty["HOLD_QTY"]) * 100 / Util.NVC_Int(drCstEmpty["TOTAL_QTY"]);
                        drCstEmpty["SUM_TYPE"] = "SUB";

                        result.Rows.InsertAt(drCstEmpty, i + 1);
                        i += 1;

                        drCstEmpty = result.NewRow();
                    }
                }
            }


            return result;
        }

        private DataTable GetTotalSumAndAvg(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            DataTable result = dt.Copy();
            DataRow drTotal = result.NewRow();

            for (int i = 0; i < result.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(result.Rows[i]["SUM_TYPE"])))
                {
                    drTotal["TOTAL_QTY"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) + Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);

                    drTotal["MASS_QTY"] = Util.NVC_Int(drTotal["MASS_QTY"]) + Util.NVC_Int(result.Rows[i]["MASS_QTY"]);
                    drTotal["MASS_TYPE1"] = Util.NVC_Int(drTotal["MASS_TYPE1"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE1"]);
                    drTotal["MASS_TYPE2"] = Util.NVC_Int(drTotal["MASS_TYPE2"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE2"]);
                    drTotal["MASS_TYPE3"] = Util.NVC_Int(drTotal["MASS_TYPE3"]) + Util.NVC_Int(result.Rows[i]["MASS_TYPE3"]);

                    drTotal["OTHER_QTY"] = Util.NVC_Int(drTotal["OTHER_QTY"]) + Util.NVC_Int(result.Rows[i]["OTHER_QTY"]);
                    drTotal["OTHER_TYPE1"] = Util.NVC_Int(drTotal["OTHER_TYPE1"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE1"]);
                    drTotal["OTHER_TYPE2"] = Util.NVC_Int(drTotal["OTHER_TYPE2"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE2"]);
                    drTotal["OTHER_TYPE3"] = Util.NVC_Int(drTotal["OTHER_TYPE3"]) + Util.NVC_Int(result.Rows[i]["OTHER_TYPE3"]);

                    drTotal["HOLD_QTY"] = Util.NVC_Int(drTotal["HOLD_QTY"]) + Util.NVC_Int(result.Rows[i]["HOLD_QTY"]);
                    drTotal["HOLD_QA"] = Util.NVC_Int(drTotal["HOLD_QA"]) + Util.NVC_Int(result.Rows[i]["HOLD_QA"]);
                    drTotal["HOLD_TYPE1"] = Util.NVC_Int(drTotal["HOLD_TYPE1"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE1"]);
                    drTotal["HOLD_TYPE2"] = Util.NVC_Int(drTotal["HOLD_TYPE2"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE2"]);
                    drTotal["HOLD_TYPE3"] = Util.NVC_Int(drTotal["HOLD_TYPE3"]) + Util.NVC_Int(result.Rows[i]["HOLD_TYPE3"]);
                }



                if (i + 1 == result.Rows.Count 
                    || !Util.NVC(result.Rows[i]["STKGROUP"]).Equals(Util.NVC(result.Rows[i+1]["STKGROUP"])))
                {
                    drTotal["STKGROUP"] = Util.NVC(result.Rows[i]["STKGROUP"]);
                    drTotal["STK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_QTY"]);
                    drTotal["STK_RACK_QTY"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                    drTotal["EMPTY_FLAG"] = "Total_" + Util.NVC(result.Rows[i]["TOTNUM"]).Replace("TOT", "");
                    drTotal["PRJT_NAME"] = "_";
                    drTotal["ELTR_TYPE"] = string.Empty;
                    drTotal["TOTNUM"] = Util.NVC(result.Rows[i]["TOTNUM"]);


                    drTotal["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                    drTotal["MASS_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["MASS_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                    drTotal["OTHER_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["OTHER_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                    drTotal["HOLD_RATE"] = Util.NVC_Int(drTotal["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(drTotal["HOLD_QTY"]) * 100 / Util.NVC_Int(drTotal["TOTAL_QTY"]);
                    drTotal["SUM_TYPE"] = "TOT";

                    result.Rows.InsertAt(drTotal, i + 1);
                    i += 1;
                    drTotal = result.NewRow();
                }
            }

            return result;

        }

        private DataTable GetRowSumAndAvg(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            DataTable result = dt.Copy();

            for (int i = 0; i < result.Rows.Count; i++)
            {
                result.Rows[i]["SUM_TYPE"] = string.Empty;
                result.Rows[i]["TOTAL_RATE"] = Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]) == 0 ? 0 : Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["STK_RACK_QTY"]);
                result.Rows[i]["MASS_RATE"] = Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(result.Rows[i]["MASS_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);
                result.Rows[i]["OTHER_RATE"] = Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(result.Rows[i]["OTHER_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);
                result.Rows[i]["HOLD_RATE"] = Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]) == 0 ? 0 : Util.NVC_Int(result.Rows[i]["HOLD_QTY"]) * 100 / Util.NVC_Int(result.Rows[i]["TOTAL_QTY"]);
            }

            return result;

        }

        private void GetStockerStatus()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQGRID", typeof(string));
                dtIndata.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dtIndata.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_MCS_SEL_WAREHOUSE_DAILY_FA", "INDATA", "RSLTDT", dtIndata, (result, bizError) =>
                {
                    if (bizError != null)
                    {
                        return;
                    }

                    try
                    {
                        if (result == null || result.Rows.Count == 0)
                        {
                        }
                        else
                        {
                            result.Columns.Add("SUM_TYPE", typeof(string));
                            DataTable dtComputed = GetRowSumAndAvg(result);
                            DataTable dtComputedUsing = GetCstUsingSumAndAvg(dtComputed);
                            DataTable dtComputedEmpty = GetCstEmptySumAndAvg(dtComputedUsing);
                            DataTable dtComputedTotal = GetTotalSumAndAvg(dtComputedEmpty);
                            dgStk.ItemsSource = dtComputedTotal.DefaultView;

                            new Util().SetDataGridMergeExtensionCol(dgStk, new string[] { "STKGROUP", "STK_QTY", "STK_RACK_QTY", "EMPTY_FLAG", "PRJT_NAME" }, DataGridMergeMode.VERTICAL);
                        }

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }
                });
            }
            catch(Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
