/*************************************************************************************
 Created Date : 2021.03.24
      Creator : 신광희
   Decription : Roll Map 데이터 조회
--------------------------------------------------------------------------------------
 [Change History]
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
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
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_354 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly DataTable _dtEquipmentMeasurement = new DataTable();
        private readonly Util _util = new Util();

        private DataTable _dtRollMapCollectInfo = new DataTable();


        public COM001_354()
        {
            InitializeComponent();
            InitializeControl();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSelectRollMapCollect()) return;
                ClearControl();

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_ROLLMAP_COLLECT_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("ADJ_LOTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                string adjLotId, lotId;

                if(rdoAdjLot != null && (bool)rdoAdjLot.IsChecked)
                {
                    adjLotId = txtLotId.Text;
                    lotId = null;
                }
                else
                {
                    adjLotId = null;
                    lotId = txtLotId.Text;
                }

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ADJ_LOTID"] = adjLotId;
                dr["LOTID"] = lotId;
                dr["EQPT_MEASR_PSTN_ID"] = cboEquipmentMeasurement.SelectedValue;
                dr["DEL_FLAG"] = (chkIncludeDelete != null && chkIncludeDelete.IsChecked == true) ? null : "N";
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, result, null, true);
                        _dtRollMapCollectInfo = result.Copy();
                        SetEquipmentMeasurement(result);
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
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dgList.EndEdit();
            dgList.EndEditRow(true);

            if (!ValidationSave()) return;

            Util.MessageInfo("준비중 입니다.");
            return;

            const string bizRuleName = "XXXX";

            DataTable inTable = new DataTable("");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(string));
            inTable.Columns.Add("CLCT_SEQNO", typeof(string));

            inTable.Columns.Add("CLCT_DTTM", typeof(string));
            inTable.Columns.Add("LANE_NO", typeof(string));
            inTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inTable.Columns.Add("END_PSTN", typeof(decimal));
            inTable.Columns.Add("X_PSTN", typeof(decimal));

            inTable.Columns.Add("Y_PSTN", typeof(decimal));
            inTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inTable.Columns.Add("SCAN_OFFSET", typeof(decimal));
            inTable.Columns.Add("SCAN_COLRMAP", typeof(string));
            inTable.Columns.Add("SCAN_AVG_VALUE", typeof(decimal));

            inTable.Columns.Add("WND_DIRCTN", typeof(string));
            inTable.Columns.Add("WND_LEN", typeof(decimal));
            inTable.Columns.Add("INPUT_LOTID", typeof(string));
            inTable.Columns.Add("SCAN_COLR_SET_VALUE_HH", typeof(decimal));
            inTable.Columns.Add("SCAN_COLR_SET_VALUE_H", typeof(decimal));

            inTable.Columns.Add("SCAN_COLR_SET_VALUE_SV", typeof(decimal));
            inTable.Columns.Add("SCAN_COLR_SET_VALUE_L", typeof(decimal));
            inTable.Columns.Add("SCAN_COLR_SET_VALUE_LL", typeof(decimal));
            inTable.Columns.Add("TAG_AUTO_FLAG", typeof(string));
            inTable.Columns.Add("ADJ_LOTID", typeof(string));

            inTable.Columns.Add("ADJ_WIPSEQ", typeof(string));
            inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inTable.Columns.Add("ADJ_X_PSTN", typeof(decimal));
            inTable.Columns.Add("ADJ_Y_PSTN", typeof(decimal));

            inTable.Columns.Add("DEL_FLAG", typeof(string));
            inTable.Columns.Add("LOT_DFCT_NO", typeof(string));
            inTable.Columns.Add("STRT_TAG_PRT_STAT", typeof(string));
            inTable.Columns.Add("END_TAG_PRT_STAT", typeof(string));
            inTable.Columns.Add("ADJ_STRT_PSTN2", typeof(decimal));

            inTable.Columns.Add("ADJ_END_PSTN2", typeof(decimal));
            inTable.Columns.Add("ADJ_X_PSTN2", typeof(decimal));
            inTable.Columns.Add("UPDUSER", typeof(string));


            foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                    newRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetString();
                    newRow["CLCT_DTTM"] = DataTableConverter.GetValue(row.DataItem, "CLCT_DTTM").GetString();
                    newRow["LANE_NO"] = DataTableConverter.GetValue(row.DataItem, "LANE_NO").GetString();
                    newRow["STRT_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "STRT_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "STRT_PSTN").GetDecimal();
                    newRow["END_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "END_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "END_PSTN").GetDecimal();
                    newRow["X_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "X_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "X_PSTN").GetDecimal();
                    newRow["Y_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "Y_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "Y_PSTN").GetDecimal();
                    newRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRow["SCAN_OFFSET"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_OFFSET").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_OFFSET").GetDecimal();
                    newRow["SCAN_COLRMAP"] = DataTableConverter.GetValue(row.DataItem, "SCAN_COLRMAP").GetString();
                    newRow["SCAN_AVG_VALUE"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_AVG_VALUE").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_AVG_VALUE").GetDecimal();
                    newRow["WND_DIRCTN"] = DataTableConverter.GetValue(row.DataItem, "WND_DIRCTN").GetString();
                    newRow["WND_LEN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "WND_LEN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "WND_LEN").GetDecimal();
                    newRow["INPUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "INPUT_LOTID").GetString();
                    newRow["SCAN_COLR_SET_VALUE_HH"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_HH").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_HH").GetDecimal();
                    newRow["SCAN_COLR_SET_VALUE_H"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_H").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_H").GetDecimal();
                    newRow["SCAN_COLR_SET_VALUE_SV"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_SV").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_SV").GetDecimal();
                    newRow["SCAN_COLR_SET_VALUE_L"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_L").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_L").GetDecimal();
                    newRow["SCAN_COLR_SET_VALUE_LL"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_LL").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "SCAN_COLR_SET_VALUE_LL").GetDecimal();
                    newRow["TAG_AUTO_FLAG"] = DataTableConverter.GetValue(row.DataItem, "TAG_AUTO_FLAG").GetString();
                    newRow["ADJ_LOTID"] = DataTableConverter.GetValue(row.DataItem, "ADJ_LOTID").GetString();
                    newRow["ADJ_WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "ADJ_WIPSEQ").GetString();
                    newRow["ADJ_STRT_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRow["ADJ_END_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRow["ADJ_X_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_X_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_X_PSTN").GetDecimal();
                    newRow["ADJ_Y_PSTN"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_Y_PSTN").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_Y_PSTN").GetDecimal();
                    newRow["DEL_FLAG"] = DataTableConverter.GetValue(row.DataItem, "DEL_FLAG").GetString();
                    newRow["LOT_DFCT_NO"] = DataTableConverter.GetValue(row.DataItem, "LOT_DFCT_NO").GetString();
                    newRow["STRT_TAG_PRT_STAT"] = DataTableConverter.GetValue(row.DataItem, "STRT_TAG_PRT_STAT").GetString();
                    newRow["END_TAG_PRT_STAT"] = DataTableConverter.GetValue(row.DataItem, "END_TAG_PRT_STAT").GetString();

                    newRow["ADJ_STRT_PSTN2"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN2").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN2").GetDecimal();
                    newRow["ADJ_END_PSTN2"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN2").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN2").GetDecimal();
                    newRow["ADJ_X_PSTN2"] = string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ADJ_X_PSTN2").GetString()) ? (object)DBNull.Value : DataTableConverter.GetValue(row.DataItem, "ADJ_X_PSTN2").GetDecimal();
                    newRow["UPDUSER"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(inTable);
            string xml = ds.GetXml();


        }

        private void cboEquipmentMeasurement_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!CommonVerify.HasTableRow(_dtRollMapCollectInfo)) return;

            if (cboEquipmentMeasurement.SelectedValue == null)
            {
                Util.GridSetData(dgList, _dtRollMapCollectInfo.Copy(), null, true);
            }
            else
            {
                DataTable dtFiltering = _dtRollMapCollectInfo.AsEnumerable().Where(s => s.Field<string>("EQPT_MEASR_PSTN_ID") == (string)cboEquipmentMeasurement.SelectedValue).CopyToDataTable();
                Util.GridSetData(dgList, dtFiltering.Copy(), null, true);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgList);
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e?.Column == null) return;
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;
            // 체크박스에 선택된 대상만 수정 할 수 있으며, 선택된 셀의 값이 있는 경우에만 수정이 가능하도록 처리 함.
            if (e.Column.Name != "CHK" && drv["CHK"].GetString() != "True")
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = string.IsNullOrEmpty(drv[e.Column.Name].GetString());
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null || dg.CurrentCell.Row.IsMouseOver == false) return;

            if (dg.CurrentCell.Column.Name != "LOTID" && dg.CurrentCell.Column.Name != "EQPTID" &&
                dg.CurrentCell.Column.Name != "EQPT_MEASR_PSTN_ID" && dg.CurrentCell.Column.Name != "WIPSEQ" &&
                dg.CurrentCell.Column.Name != "CLCT_SEQNO" && dg.CurrentCell.Column.Name != "ADJ_LOTID")
                return;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;

            if (drv == null) return;

            string equipmentCode = drv["EQPTID"].GetString();
            string equipmentMeasurementPositionCode = drv["EQPT_MEASR_PSTN_ID"].GetString();
            string lotId = drv["LOTID"].GetString();
            string wipSeq = drv["WIPSEQ"].GetString();
            string collectSeq = drv["CLCT_SEQNO"].GetString();

            COM001_354_ROLLMAP_CLCT_INFO_HIST popupRollmapcollectInfoHistory = new COM001_354_ROLLMAP_CLCT_INFO_HIST { FrameOperation = FrameOperation };

            object[] parameters = new object[5];
            parameters[0] = equipmentCode;
            parameters[1] = equipmentMeasurementPositionCode;
            parameters[2] = lotId;
            parameters[3] = wipSeq;
            parameters[4] = collectSeq;
            C1WindowExtension.SetParameters(popupRollmapcollectInfoHistory, parameters);
            //popupEquipmentComment.Closed += new EventHandler(popupRollmapcollectInfoHistory_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupRollmapcollectInfoHistory.ShowModal()));
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "LOTID" || e.Cell.Column.Name == "EQPTID" || e.Cell.Column.Name == "EQPT_MEASR_PSTN_ID" || e.Cell.Column.Name == "WIPSEQ" || e.Cell.Column.Name == "CLCT_SEQNO" || e.Cell.Column.Name == "ADJ_LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #region Mehod

        private void InitializeControl()
        {
            _dtEquipmentMeasurement.Columns.Add("CBO_CODE", typeof(string));
            _dtEquipmentMeasurement.Columns.Add("CBO_NAME", typeof(string));

            SetEquipmentMeasurement(_dtEquipmentMeasurement);

            _dtRollMapCollectInfo = Util.MakeDataTable(dgList, false);
        }

        private void ClearControl()
        {
            SetDataGridCheckHeaderInitialize(dgList);
            _dtRollMapCollectInfo.Clear();
            _dtEquipmentMeasurement.Clear();
        }

        private void SetEquipmentMeasurement(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                DataRow newRow = _dtEquipmentMeasurement.NewRow();
                newRow["CBO_CODE"] = null;
                newRow["CBO_NAME"] = "-ALL-";

                _dtEquipmentMeasurement.Rows.InsertAt(newRow, 0);
                cboEquipmentMeasurement.ItemsSource = _dtEquipmentMeasurement.Copy().AsDataView();
                cboEquipmentMeasurement.SelectedIndex = 0;
            }
            else
            {
                var query = dt.AsEnumerable().GroupBy(x => new
                {
                    equipmentMeasurementPositionCode = x.Field<string>("EQPT_MEASR_PSTN_ID"),
                }).Select(g => new
                {
                    EquipmentMeasurementPositionCode = g.Key.equipmentMeasurementPositionCode,
                    EquipmentMeasurementPositionName = g.Key.equipmentMeasurementPositionCode,
                    Count = g.Count()
                }).ToList();

                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        DataRow dr = _dtEquipmentMeasurement.NewRow();
                        dr["CBO_CODE"] = item.EquipmentMeasurementPositionCode;
                        dr["CBO_NAME"] = item.EquipmentMeasurementPositionName;
                        _dtEquipmentMeasurement.Rows.Add(dr);
                    }

                    DataRow newRow = _dtEquipmentMeasurement.NewRow();
                    newRow["CBO_CODE"] = null;
                    newRow["CBO_NAME"] = "-ALL-";

                    _dtEquipmentMeasurement.Rows.InsertAt(newRow, 0);
                    cboEquipmentMeasurement.ItemsSource = _dtEquipmentMeasurement.Copy().AsDataView();
                    cboEquipmentMeasurement.SelectedIndex = 0;
                }
                else
                {
                    DataRow newRow = _dtEquipmentMeasurement.NewRow();
                    newRow["CBO_CODE"] = null;
                    newRow["CBO_NAME"] = "-ALL-";

                    _dtEquipmentMeasurement.Rows.InsertAt(newRow, 0);
                    cboEquipmentMeasurement.ItemsSource = _dtEquipmentMeasurement.Copy().AsDataView();
                    cboEquipmentMeasurement.SelectedIndex = 0;
                }
            }
        }

        private bool ValidationSelectRollMapCollect()
        {
            if (!string.IsNullOrEmpty(txtLotId.Text.Trim())) return true;

            Util.MessageValidation("SFU1366");
            return false;
        }

        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgList))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgList, "CHK", true) < 1)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }
            return true;
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }




        #endregion


    }
}