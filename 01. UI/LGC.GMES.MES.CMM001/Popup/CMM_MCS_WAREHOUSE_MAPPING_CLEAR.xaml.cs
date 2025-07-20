/*************************************************************************************
 Created Date : 2019.04.11
      Creator : 신광희
   Decription : 창고 매핑 데이터 해제
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.11  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System;
using System.Data;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_MCS_WAREHOUSE_MAPPING_CLEAR.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_MCS_WAREHOUSE_MAPPING_CLEAR : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        private DataTable _dtLotInfo;
        private readonly Util _util = new Util();
        private string _tempLotId = string.Empty;
        public bool IsUpdated;

        #endregion


        public CMM_MCS_WAREHOUSE_MAPPING_CLEAR()
        {
            InitializeComponent();
        }

        #region Initialize
        private void InitializeLotInfo()
        {
            _dtLotInfo = new DataTable();
            _dtLotInfo.Columns.Add("CHK", typeof(bool));
            _dtLotInfo.Columns.Add("NO", typeof(int));
            _dtLotInfo.Columns.Add("LOTID", typeof(string));
            _dtLotInfo.Columns.Add("PRODID", typeof(string));
            _dtLotInfo.Columns.Add("EQPTID", typeof(string));
            _dtLotInfo.Columns.Add("RACK_ID", typeof(string));
            _dtLotInfo.Columns.Add("POSITION", typeof(string));
            _dtLotInfo.Columns.Add("WH_RCV_ISS_CODE", typeof(string));
            _dtLotInfo.Columns.Add("WH_RCV_DTTM", typeof(string));
            _dtLotInfo.Columns.Add("X_PSTN", typeof(string));
            _dtLotInfo.Columns.Add("Y_PSTN", typeof(string));
            _dtLotInfo.Columns.Add("Z_PSTN", typeof(string));
            _dtLotInfo.Columns.Add("VisibilityButton", typeof(string));

            Util.GridSetData(dgLotList, _dtLotInfo, null);
        }
        #endregion

        public IFrameOperation FrameOperation { get; set; }

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLotId.Focus();
            InitializeLotInfo();

        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {

                if (string.IsNullOrEmpty(txtLotId.Text))
                {
                    // LOT ID를 입력해주세요
                    Util.MessageValidation("SFU1366");
                    return;
                }

                _tempLotId = txtLotId.Text;
                txtLotId.Text = string.Empty;

                if (!ValidationDuplicationLot(_tempLotId))
                {
                    Util.MessageConfirmByWarning("SFU2051", result =>
                    {
                        if (result != MessageBoxResult.OK) return;
                        txtLotId.Focus();
                    }, _tempLotId);
                    return;
                }

                GetLotInfo(_tempLotId);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotInfo(txtLotId.Text.Trim());
        }

        private void btnMappingClear_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMappingClear()) return;

            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_WIPATTR_FOR_WH_OUT";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLotList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True")
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        dr["UPDUSER"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        Util.gridClear(dgLotList);
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotList == null || dgLotList.Rows.Count < 1)
                    return;

                Button dg = sender as Button;
                if ((dg?.DataContext as DataRowView)?.Row != null)
                {
                    DataRow dtRow = ((DataRowView)dg.DataContext).Row;
                    DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                    DataRow[] selectedRow = dt.Select("LOTID = '" + Util.NVC(dtRow["LOTID"]) + "'");

                    int seqno = 0;
                    foreach (DataRow row in selectedRow)
                    {
                        seqno = Convert.ToInt16(row["NO"]);
                        dt.Rows.Remove(row);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToInt16(row["NO"]) > seqno)
                        {
                            row["NO"] = Convert.ToInt16(row["NO"]) - 1;
                        }
                    }

                    dt.AcceptChanges();
                    dgLotList.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        #endregion


        #region User Method

        private bool ValidationDuplicationLot(string lotId)
        {
            if (CommonVerify.HasDataGridRow(dgLotList))
            {
                DataTable dt = ((DataView)dgLotList.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                    where t.Field<string>("LOTID") == lotId
                    select t).ToList();

                if (queryEdit.Any())
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidationMappingClear()
        {
            if (!CommonVerify.HasDataGridRow(dgLotList))
            {
                Util.MessageValidation("SFU5069");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgLotList, "CHK", true) < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void GetLotInfo(string lotId = "")
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_MAPPING_LOT";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LOTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LOTID"] = lotId;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    int maxSeq = 1;
                    DataTable dt = ((DataView)dgLotList.ItemsSource).Table;
                    if (CommonVerify.HasTableRow(dt))
                    {
                        maxSeq = Convert.ToInt32(dt.Compute("max([NO])", string.Empty)) + 1;
                    }

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["CHK"] = true;
                        newRow["NO"] = maxSeq;
                        newRow["LOTID"] = dtResult.Rows[i]["LOTID"];
                        newRow["PRODID"] = dtResult.Rows[i]["PRODID"];
                        newRow["EQPTID"] = dtResult.Rows[i]["EQPTID"];
                        newRow["RACK_ID"] = dtResult.Rows[i]["RACK_ID"];
                        newRow["POSITION"] = dtResult.Rows[i]["POSITION"];
                        newRow["WH_RCV_ISS_CODE"] = dtResult.Rows[i]["WH_RCV_ISS_CODE"];
                        newRow["WH_RCV_DTTM"] = dtResult.Rows[i]["WH_RCV_DTTM"];
                        newRow["X_PSTN"] = dtResult.Rows[i]["X_PSTN"];
                        newRow["Y_PSTN"] = dtResult.Rows[i]["Y_PSTN"];
                        newRow["Z_PSTN"] = dtResult.Rows[i]["Z_PSTN"];
                        newRow["VisibilityButton"] = "Visible";
                        dt.Rows.Add(newRow);
                        maxSeq++;
                    }
                    Util.GridSetData(dgLotList, dt, null, true);
                }
                else
                {
                    Util.MessageInfo("SFU1195");
                }

                txtLotId.Focus();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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


    }
}
