/*************************************************************************************
 Created Date : 2018.05.23
      Creator : 신광희
   Decription : 믹서이물 관리
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.23  신광희 : Initial Created.
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
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using UserControl = System.Windows.Controls.UserControl;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_034 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _collectSeqNo = string.Empty;

        public ELEC001_034()
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

        private void InitializeComboBox()
        {
            CommonCombo combo = new CommonCombo();

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
            List<Button> listAuth = new List<Button> { btnRegistration, btnSearch, btnDelete, btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            ImpurityCollectButtonIsEnable(false);

            InitializeComboBox();
            SetWarningTextBox();
            Loaded -= UserControl_Loaded;
        }

        private void SetWarningTextBox()
        {
            try
            {
                spnElapsedDays.Visibility = Visibility.Collapsed;

                DataTable resultDt = new ClientProxy().ExecuteServiceSync_Multi("BR_CUS_GET_SYSTIME", "INDATA", "OUTDATA", new DataSet()).Tables["OUTDATA"]; // 2024.10.06 김영국 - String indata를 넘겨주도록 수정함.
                DateTime curTime = Convert.ToDateTime(resultDt.Rows[0]["SYSTIME"]);

                //2019.09.12 김대근 - 수거일시 콤보박스의 Items.Count가 0이면 lastCollectTime을 가져올 수 없다.
                if (cboCollectTime.Items.Count == 0)
                    return;

                DateTime lastCollectTime = Convert.ToDateTime(((DataRowView)cboCollectTime.Items[0])["CLCT_DTTM"]);
                int elapsedDays = curTime.Subtract(lastCollectTime).Days;
                if(elapsedDays > 7)
                {
                    tblElapsedDays.Text = elapsedDays.GetString();
                    spnElapsedDays.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            //tbxWarning.Text
        }

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRegistration()) return;
            _collectSeqNo = string.Empty;

            ELEC001_034_IMPURITY_REGISTER popupInpurity = new ELEC001_034_IMPURITY_REGISTER { FrameOperation = FrameOperation };

            object[] parameters = new object[3];
            parameters[0] = cboArea.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            C1WindowExtension.SetParameters(popupInpurity, parameters);
            popupInpurity.Closed += popupInpurity_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupInpurity.ShowModal()));

        }

        private void popupInpurity_Closed(object sender, EventArgs e)
        {
            ELEC001_034_IMPURITY_REGISTER popup = sender as ELEC001_034_IMPURITY_REGISTER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _collectSeqNo = popup.CollectSeqNo;

                if (!string.IsNullOrEmpty(_collectSeqNo))
                {
                    GetCollectTime();
                }
                SelectImpurityCollect();
                //btnSearch_Click(btnSearch, null);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            SelectImpurityCollect();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;

            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveImpurityCollect();
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete()) return;

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteImpurityCollect();
                }
            });
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
                GetCollectTime();
        }

        private void dgSlurry_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                int idx = e.Cell.Row.Index;

                // [PPM 구하는 공식] = ①수거량 / (②생산량 합계)*1000
                DataTable dt = ((DataView)dgOutPut.ItemsSource).Table;
                // 생산량 합계
                double totalSlurry = dt.AsEnumerable().Sum(s => s.Field<double>("SLURRY_QTY"));
                // 수거량
                double collectQty = Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[idx].DataItem, "CLCT_QTY")).GetDouble();

                if (totalSlurry > 0)
                {
                    double ppmQty = collectQty / totalSlurry * 1000;
                    DataTableConverter.SetValue(dgSlurry.Rows[idx].DataItem, "PPM_QTY", ppmQty);
                    DataTableConverter.SetValue(dgSlurry.Rows[idx].DataItem, "PPB_QTY", ppmQty * 1000);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHopper_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

            try
            {
                if (e.Cell.Column.Name != "CLCT_QTY") return;

                int index = e.Cell.Row.Index;
                string hopperId = Util.NVC(DataTableConverter.GetValue(dgHopper.Rows[index].DataItem, "HOPPER_ID"));
                double collectQty = Util.NVC(DataTableConverter.GetValue(dgHopper.Rows[index].DataItem, "CLCT_QTY")).GetDouble();

                DataTable dt = ((DataView)dgHopper.ItemsSource).Table;
                double inputQty = dt.AsEnumerable().Where(w => w.Field<string>("HOPPER_ID") == hopperId).Sum(s => s.Field<double>("INPUT_QTY"));

                foreach (DataGridRow row in dgHopper.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (DataTableConverter.GetValue(row.DataItem, "HOPPER_ID").GetString() == hopperId)
                        {
                            DataTableConverter.SetValue(row.DataItem, "CLCT_QTY", collectQty);

                            if (inputQty != 0)
                            {
                                DataTableConverter.SetValue(row.DataItem, "PPM_QTY", DataTableConverter.GetValue(row.DataItem, "CLCT_QTY").GetDouble() / inputQty * 1000);
                                DataTableConverter.SetValue(row.DataItem, "PPB_QTY", DataTableConverter.GetValue(row.DataItem, "CLCT_QTY").GetDouble() / inputQty * 1000 * 1000);
                            }
                        }
                    }
                }
                dgHopper.EndEdit();
                dgHopper.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw;
            }

        }

        private void DgHopper_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg?.ItemsSource == null) return;

                //e.Merge(new DataGridCellsRange(dg.GetCell(0, dg.Columns["HOPPER_ID"].Index + 1), dg.GetCell(2, dg.Columns["HOPPER_ID"].Index + 1)));
                //e.Merge(new DataGridCellsRange(dg.GetCell(3, dg.Columns["HOPPER_ID"].Index + 1), dg.GetCell(3, dg.Columns["HOPPER_ID"].Index + 1)));

                DataTable dt = ((DataView) dg.ItemsSource).Table;
                var queryHopper = dt.AsEnumerable().GroupBy(x => new
                {
                    HopperId = x.Field<string>("HOPPER_ID")
                }).Select(g => new
                {
                    HopperCode = g.Key.HopperId,
                    Count = g.Count()
                }).ToList();

                string previewHopperCode = string.Empty;

                if (queryHopper.Any())
                {

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        foreach (var item in queryHopper)
                        {
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "HOPPER_ID").GetString() == item.HopperCode && previewHopperCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "HOPPER_ID").GetString())
                            {
                                int rowIndex = 0;
                                if (i != 0) rowIndex = i;

                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["HOPPER_ID"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["HOPPER_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["HOPPER_NAME"].Index),dg.GetCell(item.Count -1 + rowIndex, dg.Columns["HOPPER_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["CLCT_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["CLCT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PPM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PPM_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PPB_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PPB_QTY"].Index)));
                                previewHopperCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "HOPPER_ID").GetString();
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

        private void dgHopper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                if (e.Cell?.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    if (panel != null)
                    {
                        ContentPresenter presenter = panel.Children[0] as ContentPresenter;
                        if (e.Cell.Column.Index == dg.Columns["PPM_QTY"].Index)
                        {
                            if (e.Cell.Row.Index == dg.Rows.Count - 1)
                            {
                                if (presenter != null)
                                {
                                    presenter.Content = GetSumHopperppm();
                                }
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
                GetCollectTime();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboCollectTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboCollectTime.SelectedValue == null || cboCollectTime.ItemsSource == null)
            {
                ImpurityCollectButtonIsEnable(false);
            }

            //if (!string.IsNullOrEmpty(_collectSeqNo))
            //{
            //    btnSearch.IsEnabled = cboCollectTime.SelectedValue.GetString() == _collectSeqNo;
            //}
        }

        #endregion

        #region Mehod

        private void SelectImpurityCollect()
        {
            try
            {
                const string bizRuleName = "BR_BAS_SEL_IMPURITY_CLCT_HIST";
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_SEQNO", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CLCT_SEQNO"] = cboCollectTime.SelectedValue;
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

                        if (CommonVerify.HasTableInDataSet(bizResult) && CommonVerify.HasTableRow(bizResult.Tables["OUT_PRD"]))
                        {
                            Util.GridSetData(dgOutPut, bizResult.Tables["OUT_PRD"], FrameOperation, true);

                            if (CommonVerify.HasTableRow(bizResult.Tables["OUT_HOPPER"]))
                            {
                                dgHopper.ItemsSource = DataTableConverter.Convert(GetHopperppm(bizResult.Tables["OUT_HOPPER"]));
                            }
                            // 슬러리 합계량
                            double totalSlurryQty = bizResult.Tables["OUT_PRD"].AsEnumerable().Sum(s => s.Field<double>("SLURRY_QTY")); // 2024.10.24. 김영국 - DB Type형식 불일치로 인한 Type 변경. Decimal -> double
                            if (CommonVerify.HasTableRow(bizResult.Tables["OUT_PSTN"]))
                            {
                                dgSlurry.ItemsSource = DataTableConverter.Convert(GetSullyppm(bizResult.Tables["OUT_PSTN"], totalSlurryQty));
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

        private void DeleteImpurityCollect()
        {
            try
            {
                const string bizRuleName = "DA_BAS_DEL_TB_SFC_IMPURITY_CLCT_HIST";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_SEQNO", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CLCT_SEQNO"] = cboCollectTime.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        txtNote.Text = string.Empty;
                        GetCollectTime();
                        SelectImpurityCollect();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveImpurityCollect()
        {
            try
            {
                const string bizRuleName = "BR_BAS_REG_IMPURITY_CLCT_QTY";
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_SEQNO", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CLCT_SEQNO"] = cboCollectTime.SelectedValue;
                dr["NOTE"] = txtNote.Text;
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataTable inHopperTable = indataSet.Tables.Add("INDATA_HOPPER");
                inHopperTable.Columns.Add("HOPPER_ID", typeof(string));
                inHopperTable.Columns.Add("CLCT_QTY", typeof(double));

                DataTable dt = DataTableConverter.Convert(dgHopper.ItemsSource); //2019.10.17 이물관리 Hopper가 없는 경우에 null관련 에러 방지
                var query = (from t in dt.AsEnumerable()
                    select new { HopperId = t.Field<string>("HOPPER_ID"), HopperCollectQty = t.Field<double>("CLCT_QTY") }).Distinct().ToList();

                if(query.Any())
                    foreach (var item in query)
                    {
                        DataRow drHopper = inHopperTable.NewRow();
                        drHopper["HOPPER_ID"] = item.HopperId;
                        drHopper["CLCT_QTY"] = item.HopperCollectQty;
                        inHopperTable.Rows.Add(drHopper);
                    }

                DataTable inPositionTable = indataSet.Tables.Add("INDATA_PSTN");
                inPositionTable.Columns.Add("IMPURITY_CLCT_PSTN_CODE", typeof(string));
                inPositionTable.Columns.Add("CLCT_QTY", typeof(double));

                foreach (DataGridRow row in dgSlurry.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        DataRow drSlurry = inPositionTable.NewRow();
                        drSlurry["IMPURITY_CLCT_PSTN_CODE"] = DataTableConverter.GetValue(row.DataItem, "IMPURITY_CLCT_PSTN_CODE").GetString();
                        drSlurry["CLCT_QTY"] = DataTableConverter.GetValue(row.DataItem, "CLCT_QTY").GetDecimal();
                        inPositionTable.Rows.Add(drSlurry);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INDATA_HOPPER,INDATA_PSTN", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU3532");
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

                DataTable inTable = new DataTable {TableName = "RQSTDT"};
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

        private bool ValidationRegistration()
        {
            return true;
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

        private bool ValidationSave()
        {
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                // 비고를 입력 하세요.
                Util.MessageValidation("SFU1590");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (cboCollectTime.SelectedIndex < 0 || string.IsNullOrEmpty(cboCollectTime.SelectedValue.GetString()))
            {
                Util.MessageValidation("SUF4963");
                return false;
            }

            return true;
        }

        #region [Func]

        private void GetCollectTime()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_CLCT_SEQNO_COMBO";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("YEAR", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["YEAR"] = Convert.ToDateTime(dpYear.SelectedDateTime).ToString("yyyy");
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                cboCollectTime.DisplayMemberPath = "CLCT_DTTM";
                cboCollectTime.SelectedValuePath = "CLCT_SEQNO";

                cboCollectTime.ItemsSource = DataTableConverter.Convert(dtResult);

                //if (!CommonVerify.HasTableRow(dtResult))
                //    ImpurityCollectButtonIsEnable(false);

                if (CommonVerify.HasTableRow(dtResult))
                    ImpurityCollectButtonIsEnable(true);
                else
                    ImpurityCollectButtonIsEnable(false);


                var query = (from t in dtResult.AsEnumerable()
                    where t.Field<string>("CLCT_SEQNO") == _collectSeqNo
                    select t).FirstOrDefault();

                if (query != null)
                {
                    cboCollectTime.SelectedValue = _collectSeqNo;
                }
                else
                {
                    cboCollectTime.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static DataTable GetSullyppm(DataTable dt, decimal totalSullyQty)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                if (totalSullyQty > 0)
                {
                    var dtBinding = dt.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPM_QTY", DataType = typeof(decimal) });
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPB_QTY", DataType = typeof(decimal) });
                    // [PPM 구하는 공식] = ①수거량 / (②생산량 합계) * 1000
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["PPM_QTY"] = row["CLCT_QTY"].GetDecimal() / totalSullyQty * 1000;
                        row["PPB_QTY"] = row["CLCT_QTY"].GetDecimal() / totalSullyQty * 1000 * 1000;
                    }
                    dtBinding.AcceptChanges();
                    return dtBinding;
                }
                return null;
            }
            return null;
        }

        private static DataTable GetSullyppm(DataTable dt, double totalSullyQty)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                if (totalSullyQty > 0)
                {
                    var dtBinding = dt.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPM_QTY", DataType = typeof(decimal) });
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPB_QTY", DataType = typeof(decimal) });
                    // [PPM 구하는 공식] = ①수거량 / (②생산량 합계) * 1000
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["PPM_QTY"] = row["CLCT_QTY"].GetDouble() / totalSullyQty * 1000;
                        row["PPB_QTY"] = row["CLCT_QTY"].GetDouble() / totalSullyQty * 1000 * 1000;
                    }
                    dtBinding.AcceptChanges();
                    return dtBinding;
                }
                return null;
            }
            return null;
        }


        private static DataTable GetHopperppm(DataTable dt)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                //[PPM 구하는 공식] = ①수거량 / (②호퍼단위 투입량합계) * 1000
                var dtBinding = dt.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPM_QTY", DataType = typeof(decimal) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "PPB_QTY", DataType = typeof(decimal) });
                foreach (DataRow row in dtBinding.Rows)
                {
                    double inputQty = dt.AsEnumerable().Where(w => w.Field<string>("HOPPER_ID") == row["HOPPER_ID"].ToString()).Sum(s => s.Field<double>("INPUT_QTY"));
                    row["PPM_QTY"] = row["CLCT_QTY"].GetDouble() / inputQty * 1000;
                    row["PPB_QTY"] = row["CLCT_QTY"].GetDouble() / inputQty * 1000 * 1000;
                    //row["PPM_QTY"] = Math.Round(row["CLCT_QTY"].GetDecimal() / inputQty * 1000, 7);
                }
                dtBinding.AcceptChanges();
                return dtBinding;
            }
            return null;
        }

        private decimal GetSumHopperppm()
        {
            decimal sumHopperppm = 0;
            if (!CommonVerify.HasDataGridRow(dgHopper))
                return 0;

            DataTable dt = ((DataView)dgHopper.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                select new { HopperId = t.Field<string>("HOPPER_ID"), Hopperppm = t.Field<decimal>("PPM_QTY") }).Distinct().ToList();

            if(query.Any())
                sumHopperppm += query.Sum(item => Math.Round(item.Hopperppm, 7));

            return sumHopperppm;
        }

        private void ImpurityCollectButtonIsEnable(bool isEnable)
        {
            if (isEnable)
            {
                btnSearch.IsEnabled = true;
                btnSave.IsEnabled = true;
                btnDelete.IsEnabled = true;
            }
            else
            {
                btnSearch.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
        }

        private void ClearImpurityCollectControl()
        {
            Util.gridClear(dgOutPut);
            Util.gridClear(dgHopper);
            Util.gridClear(dgSlurry);
            txtNote.Text = string.Empty;
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        #endregion

        #endregion
    }
}
