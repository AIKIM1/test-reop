/*************************************************************************************
 Created Date : 2018.11.21
      Creator : 고현영
   Decription : 믹서이물 관리조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.21  고현영 : Initial Created.
  2024.06.11  유명환 : [E20240523-001361]Slurry 이물 그리드 PPM_QTY컬럼 TOTAL_PPM_QTY과 동일하게 Font Color적용
  2024.07.05  배현우 : [E20240708-000018] 호퍼 이물 , Slurry 이물 탭에 ppb 컬럼 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;

using System.Collections;
using System.Windows.Media;
using C1.WPF.Excel;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_255 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public COM001_255()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        CommonCombo combo = new CommonCombo();
        bool isSlurryFont = false;
        private void InitializeComboBox()
        {
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeComboBox();
            InitializeTextBlock();
            Loaded -= UserControl_Loaded;
        }

        private void InitializeTextBlock()
        {
            try
            {
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                dt.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "IMPURITY_SPEC_INFO";
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", dt);
                DataRow[] resultRowArr = null;

                resultRowArr = dtResult.Select("COM_CODE = 'IMP_AMTRL_0001'");
                tblHopperUSL.Text = resultRowArr[0]["ATTR1"].ToString();
                tblHopperUCL.Text = resultRowArr[0]["ATTR2"].ToString();

                resultRowArr = dtResult.Select("COM_CODE = 'IMP_SLURRY_0001'");
                tblSlurryUSL.Text = resultRowArr[0]["ATTR1"].ToString();
                tblSlurryUCL.Text = resultRowArr[0]["ATTR2"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            isSlurryFont = IsAreaCommonCodeUse("SLURRY_PPM_QTY_FONT_COLOR_USE", "USE_YN");

            if (!ValidationSearch()) return;
            SelectImpurityCollect();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearImpurityCollectControl();
            SetEquipment();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(cboEquipment?.SelectedValue.GetString()) || cboEquipment?.SelectedValue.GetString().GetString() != "SELECT")
                ClearImpurityCollectControl();
        }

        private void dgHopper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                if (e.Cell?.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dg.Columns["TOTAL_PPM_QTY"].Index)
                    {
                        decimal ppmValue = e.Cell.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(e.Cell.Text);
                        decimal hopperUCL = tblHopperUCL.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(tblHopperUCL.Text);
                        decimal hopperUSL = tblHopperUSL.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(tblHopperUSL.Text);

                        if (hopperUCL <= ppmValue)
                        {
                            if (hopperUSL <= ppmValue)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dpYear_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearImpurityCollectControl();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private bool IsAreaCommonCodeUse(string codeType, string codeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = codeType;
                dr["COM_CODE"] = codeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }

        private void SelectImpurityCollect()
        {
            try
            {
                const string bizRuleName = "BR_BAS_SEL_IMPURITY_CLCT_HIST_V02";
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("START_DTTM", typeof(string));
                //inTable.Columns.Add("END_DTTM", typeof(string));
                inTable.Columns.Add("START_DTTM", typeof(DateTime)); // 2024.10.21. 김영국 - BR호출 시 해당 Date가 DateTime형식으로 Type변경.
                inTable.Columns.Add("END_DTTM", typeof(DateTime));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                //inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["START_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["END_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["PRODID"] = tbxProdId.Text.IsEmpty() ? null : tbxProdId.Text.ToString();
                dr["PRJT_NAME"] = tbxPjtName.Text.IsEmpty() ? null : tbxPjtName.Text.ToString();
                //dr["LOTID"] = tbx
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_PRD,OUT_HOPPER,OUT_PSTN", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgOutPut);
                        Util.gridClear(dgHopper);
                        Util.gridClear(dgSlurry);

                        if (CommonVerify.HasTableInDataSet(bizResult))
                        {
                            Util.GridSetData(dgOutPut, bizResult.Tables["OUT_PRD"], FrameOperation, true);

                            if (CommonVerify.HasTableRow(bizResult.Tables["OUT_HOPPER"]))
                            {
                                dgHopper.ItemsSource = DataTableConverter.Convert(GetHopperppm(bizResult.Tables["OUT_HOPPER"]));
                            }
                            if (CommonVerify.HasTableRow(bizResult.Tables["OUT_PSTN"]))
                            {
                                dgSlurry.ItemsSource = DataTableConverter.Convert(GetSullyppm(bizResult.Tables["OUT_PSTN"]));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                Util.MessageException(ex);
            }
        }

        private void SetEquipment()
        {
            try
            {
                cboEquipment.ItemsSource = null;

                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["PROCID"] = Process.MIXING;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);

                var query = (from t in dtResult.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == LoginInfo.CFG_EQPT_ID
                             select t).FirstOrDefault();
                if (query != null)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
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

        private bool ValidationSearch()
        {
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        #region [Func]

        private DataTable GetSullyppm(DataTable dt)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                var dtBinding = dt.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPM_QTY", DataType = typeof(decimal) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOTAL_PPM_QTY", DataType = typeof(decimal) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOTAL_PPB_QTY", DataType = typeof(decimal) });
                // [PPM 구하는 공식] = ①수거량 / (②생산량 합계) * 1000
                foreach (DataRow row in dtBinding.Rows)
                {
                    if (row["SLURRY_QTY"].GetDecimal() <= 0)
                        row["PPM_QTY"] = 0;
                    else
                        row["PPM_QTY"] = row["CLCT_QTY"].GetDecimal() / row["SLURRY_QTY"].GetDecimal() * 1000;

                }

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["TOTAL_PPM_QTY"] = dtBinding.AsEnumerable().Where(s => s.Field<DateTime>("CLCT_DTTM") == Convert.ToDateTime(row["CLCT_DTTM"])).Sum(s => s.Field<decimal>("PPM_QTY"));
                    row["TOTAL_PPB_QTY"] = row["TOTAL_PPM_QTY"].GetDecimal() * 1000;
                }
                dtBinding.AcceptChanges();

                return dtBinding;
            }
            return null;
        }

        private DataTable GetHopperppm(DataTable dt)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                //[PPM 구하는 공식] = ①수거량 / (②호퍼단위 투입량합계) * 1000
                var dtBinding = dt.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPM_QTY", DataType = typeof(decimal) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOTAL_PPM_QTY", DataType = typeof(decimal) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOTAL_PPB_QTY", DataType = typeof(decimal) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    decimal inputQty = dt.AsEnumerable().Where(s => s.Field<DateTime>("CLCT_DTTM") == Convert.ToDateTime(row["CLCT_DTTM"])
                                                                 && s.Field<string>("HOPPER_ID") == row["HOPPER_ID"].ToString()).Sum(s => s.Field<decimal>("INPUT_QTY"));
                    row["PPM_QTY"] = row["CLCT_QTY"].GetDecimal() / inputQty * 1000;

                    if (row["INPUT_QTY"].GetDecimal() == 0)
                        row["PPM_QTY"] = 0;
                    else
                        row["PPM_QTY"] = row["CLCT_QTY"].GetDecimal() / inputQty * 1000;
                }

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["TOTAL_PPM_QTY"] = dtBinding.AsEnumerable().Where(w => w.Field<DateTime>("CLCT_DTTM") == Convert.ToDateTime(row["CLCT_DTTM"]))
                                                                   .GroupBy(g => new
                                                                   {
                                                                       CLCT_DTTM = g.Field<DateTime>("CLCT_DTTM"),
                                                                       HOPPER_ID = g.Field<string>("HOPPER_ID"),
                                                                       PPM_QTY = g.Field<decimal>("PPM_QTY")
                                                                   }).Sum(s => s.Key.PPM_QTY);
                    row["TOTAL_PPB_QTY"] = row["TOTAL_PPM_QTY"].GetDecimal() * 1000;
                }
                dtBinding.AcceptChanges();

                return dtBinding;
            }
            return null;
        }

        private void ClearImpurityCollectControl()
        {
            Util.gridClear(dgOutPut);
            Util.gridClear(dgHopper);
            Util.gridClear(dgSlurry);
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

        private void dgHopper_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                //투입일
                int columnIndex = dg.Columns["CLCT_DTTM"].Index;
                int fromRowIndex = 0;
                int toRowIndex = 0;
                while (fromRowIndex < dg.Rows.Count - dg.FrozenBottomRowsCount)
                {
                    DateTime fromDate = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM"));
                    DateTime toDate = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    while (fromDate == toDate)
                    {
                        toRowIndex++;
                        if (toRowIndex >= dg.Rows.Count - dg.FrozenBottomRowsCount)
                            break;
                        toDate = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    }
                    e.Merge(new DataGridCellsRange(dg.GetCell(fromRowIndex, columnIndex), dg.GetCell(toRowIndex - 1, columnIndex)));
                    fromRowIndex = toRowIndex;
                }

                //호퍼,수거량,PPM,총합PPM MERGE
                fromRowIndex = 0;
                toRowIndex = 0;
                while (fromRowIndex < dg.Rows.Count)
                {
                    string fromHopperId = DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "HOPPER_ID").ToString();
                    string toHopperId = DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "HOPPER_ID").ToString();
                    while (fromHopperId == toHopperId)
                    {
                        toRowIndex++;
                        if (toRowIndex >= dg.Rows.Count - dg.FrozenBottomRowsCount)
                            break;
                        if (Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM")) !=
                            Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM")))
                            break;
                        toHopperId = DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "HOPPER_ID").ToString();
                    }

                    foreach (string column in new string[] { "HOPPER_ID", "HOPPER_NAME", "CLCT_QTY", "PPM_QTY" })
                        e.Merge(new DataGridCellsRange(dg.GetCell(fromRowIndex, dg.Columns[column].Index), dg.GetCell(toRowIndex - 1, dg.Columns[column].Index)));
                    fromRowIndex = toRowIndex;
                }

                //호퍼 총합PPM MERGE
                fromRowIndex = 0;
                toRowIndex = 0;
                while (fromRowIndex < dg.Rows.Count)
                {
                    DateTime fromClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM"));
                    DateTime toClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    while (fromClctDttm == toClctDttm)
                    {
                        toRowIndex++;
                        if (toRowIndex >= dg.Rows.Count - dg.FrozenBottomRowsCount)
                            break;
                        if (Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM")) !=
                            Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM")))
                            break;
                        toClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    }

                    foreach (string column in new string[] { "TOTAL_PPM_QTY", "TOTAL_PPB_QTY" })
                        e.Merge(new DataGridCellsRange(dg.GetCell(fromRowIndex, dg.Columns[column].Index), dg.GetCell(toRowIndex - 1, dg.Columns[column].Index)));
                    fromRowIndex = toRowIndex;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSlurry_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                //슬러리 총합PPM MERGE
                int fromRowIndex = 0;
                int toRowIndex = 0;
                while (fromRowIndex < dg.Rows.Count)
                {
                    DateTime fromClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM"));
                    DateTime toClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    while (fromClctDttm == toClctDttm)
                    {
                        toRowIndex++;
                        if (toRowIndex >= dg.Rows.Count - dg.FrozenBottomRowsCount)
                            break;
                        if (Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[fromRowIndex].DataItem, "CLCT_DTTM")) !=
                            Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM")))
                            break;
                        toClctDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.Rows[toRowIndex].DataItem, "CLCT_DTTM"));
                    }

                    foreach (string column in new string[] { "CLCT_DTTM","TOTAL_PPM_QTY", "TOTAL_PPB_QTY" })
                        e.Merge(new DataGridCellsRange(dg.GetCell(fromRowIndex, dg.Columns[column].Index), dg.GetCell(toRowIndex - 1, dg.Columns[column].Index)));
                    fromRowIndex = toRowIndex;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnCellExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelExporter excelExporter = new LGC.GMES.MES.Common.ExcelExporter();

                C1DataGrid[] dataGridArr = new C1DataGrid[] { dgOutPut, dgHopper, dgSlurry };
                string[] tabNameArr = { "Sheet1", "Sheet2", "Sheet3" };
                excelExporter.Export(dataGridArr, tabNameArr);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgSlurry_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                if (e.Cell?.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dg.Columns["TOTAL_PPM_QTY"].Index || (e.Cell.Column.Index == dg.Columns["PPM_QTY"].Index && isSlurryFont))
                    {
                        decimal ppmValue = e.Cell.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(e.Cell.Text);
                        decimal slurryUCL = tblSlurryUCL.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(tblSlurryUCL.Text);
                        decimal slurryUSL = tblSlurryUSL.Text.IsNullOrEmpty() ? 0 : Convert.ToDecimal(tblSlurryUSL.Text);

                        if (slurryUCL <= ppmValue)
                        {
                            if (slurryUSL <= ppmValue)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void dgSlurry_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            e.Cell.Presenter.FontWeight = FontWeights.Normal;
        }

        private void dgHopper_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            e.Cell.Presenter.FontWeight = FontWeights.Normal;
        }

        private void btnTrend_Click(object sender, RoutedEventArgs e)
        {
            DataTable hopperDt = DataTableConverter.Convert(dgHopper.ItemsSource);
            DataTable slurryDt = DataTableConverter.Convert(dgSlurry.ItemsSource);

            COM001.COM001_255_IMPURITY_TREND TrendPopup = new COM001.COM001_255_IMPURITY_TREND();
            TrendPopup.FrameOperation = FrameOperation;

            if (TrendPopup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(Convert.ToDecimal(tblHopperUSL.Text.GetString())); //호퍼USL
                Parameters[1] = Util.NVC(Convert.ToDecimal(tblHopperUCL.Text.GetString())); //호퍼UCL
                Parameters[2] = (DataTableConverter.Convert(dgHopper.ItemsSource)) == null ? new DataTable(): DataTableConverter.Convert(dgHopper.ItemsSource); //호퍼DATATABLE
                Parameters[3] = Util.NVC(Convert.ToDecimal(tblSlurryUSL.Text.GetString())); //슬러리USL
                Parameters[4] = Util.NVC(Convert.ToDecimal(tblSlurryUCL.Text.GetString())); //슬러리UCL
                Parameters[5] = (DataTableConverter.Convert(dgSlurry.ItemsSource)) == null ? new DataTable() : DataTableConverter.Convert(dgSlurry.ItemsSource); //슬러리DATATABLE

                C1WindowExtension.SetParameters(TrendPopup, Parameters);

                TrendPopup.Closed += new EventHandler(TrendPopup_Closed);
                TrendPopup.ShowModal();
            }
        }

        private void TrendPopup_Closed(object sender, EventArgs e)
        {
            COM001_255_IMPURITY_TREND Window = sender as COM001_255_IMPURITY_TREND;
            if (Window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

      
    }
}
