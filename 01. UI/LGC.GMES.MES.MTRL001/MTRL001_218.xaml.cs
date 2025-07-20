/*************************************************************************************
 Created Date : 2024.09.12
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.12  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using C1.WPF.Excel;
using System.IO;
using System.Configuration;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid.Summaries;
using Microsoft.Win32;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_218 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        private string _dst_eqptID;

        public MTRL001_218()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion


        #region Initialize
        private void InitCombo()
        {
            // 창고유형 콤보박스
            SetStockerTypeCombo(cboWarehouseType);
            //창고 콤보박스 
            SetStockerCombo(cboWarehouse);

        }
        #endregion

        #region Event

        #region 창고유형 콤보 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고 유형  콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboWarehouseType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
           SetStockerCombo(cboWarehouse);
        }
        #endregion

        #region 조회버튼 이벤트 : btnSearch()
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            PM_MtrlList();
        }
        #endregion

        #region Spread 체크박스 이벤트 : dgMtrlList_BeginningEdit()

        private void dgMtrlList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                SelectWareHousePortInfo(dgMtrlList, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 출고버튼 이벤트 : btnOutput_Click()

        //출고처리
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
           if (!ValidationManualIssue()) return;

            SaveManualIssueByEsnb();
        }
        #endregion

        #region 목적지 콤보박스 이벤트 : cboIssuePort_SelectedIndexChanged()
        /// <summary>
        /// 목적지 콤보박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboOutputPort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;

            try
            {
                if (cboOutputPort != null && cboOutputPort.SelectedItem != null)
                {
                    int previousRowIndex = e.OldValue;
                    int currentRowIndex = e.NewValue;

                    string transferStateCode = ((ContentControl)(cboOutputPort.Items[currentRowIndex])).Name.GetString();
                    _dst_eqptID = ((ContentControl)(cboOutputPort.Items[currentRowIndex])).DataContext.GetString();

                    if (transferStateCode == "OUT_OF_SERVICE")
                    {
                        Util.MessageInfo("SFU8137");
                        cboOutputPort.SelectedIndex = previousRowIndex;
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

        #region Mehod

        #region 창고유형 콤보박스 조회 : SetStockerTypeCombo()

        /// <summary>
        /// 창고 유형 콤보박스 조회
        /// </summary>
        /// <param name="cbo"></param>

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, "AREA_EQUIPMENT_MTRL_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);


        }

        #endregion
        
        #region 창고 콤보박스 조회 : SetStockerCombo()

        /// <summary>
        /// 창고 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.Empty;

            stockerType = string.IsNullOrEmpty(cboWarehouseType.SelectedValue.GetString()) ? null : cboWarehouseType.SelectedValue.GetString();

            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region PM 대상 자재 정보 조회 : PM_MtrlList()

        /// <summary>
        /// PM 대상 자재 정보 조회
        /// </summary>
        private void PM_MtrlList()
        {
            const string bizRuleName = "DA_INV_SEL_MANUAL_SHIP";
            try
            {
                DataTable dtResult = new DataTable();

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQUIPMENT_ID", typeof(string));
            

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQUIPMENT_ID"] = cboWarehouse.SelectedValue == null ? null : cboWarehouse.SelectedValue.ToString();
               
                inTable.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgMtrlList, bizResult, null, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    dgMtrlList.MergingCells -= dgMtrlList_MergingCells;
                    dgMtrlList.MergingCells += dgMtrlList_MergingCells;
                }
                else
                {
                    Util.gridClear(dgMtrlList);
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion


        /// <summary>
        /// // 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMtrlList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {

            try
            {
                if (dgMtrlList.Rows.Count > 0)
                {

                    for (int idx = 0; idx < dgMtrlList.Rows.Count;)
                    {
                        string strPortId = Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[idx].DataItem, "DURABLE_ID"));

                        int nRowMergeCnt = 0;

                        for (int jdx = idx; jdx < dgMtrlList.Rows.Count; jdx++)
                        {
                            if (strPortId.Equals(Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[jdx].DataItem, "DURABLE_ID"))))
                            {
                                nRowMergeCnt++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["CHK"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["CHK"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["EQPTNAME"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["EQPTNAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["RACK_ID"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["RACK_ID"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["PRJT_NAME"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["PRJT_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["MODLID"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["MODLID"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["DURABLE_ID"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["DURABLE_ID"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["EQPT_CUR"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["EQPT_CUR"].Index)));
                        e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idx, dgMtrlList.Columns["PORT_CUR"].Index), dgMtrlList.GetCell(idx + nRowMergeCnt - 1, dgMtrlList.Columns["PORT_CUR"].Index)));

                        idx = idx + nRowMergeCnt;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        

        #region 수동출고 Validation  : ValidationManualIssue()
        /// <summary>
        /// 기본 Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationManualIssue()
        {
                     
            if (!CommonVerify.HasDataGridRow(dgMtrlList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_Util.GetDataGridRowCountByCheck(dgMtrlList, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboOutputPort.SelectedItem == null || string.IsNullOrEmpty((cboOutputPort.SelectedItem as C1ComboBoxItem).Tag.GetString()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            if ((cboOutputPort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            {
                Util.MessageInfo("SFU8137");
                return false;
            }

            return true;
        }





        #endregion

        #region PORT 리스트 및 목적지 콤보박스 조회  : SelectWareHousePortInfo()
        /// <summary>
        /// PORT 리스트 및 목적지 콤보박스 조회
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="e"></param>
        private void SelectWareHousePortInfo(C1DataGrid dg, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idx = _Util.GetDataGridFirstRowIndexByCheck(dg, "CHK");
                string currentequipmentCode = DataTableConverter.GetValue(e.Row.DataItem, "EQPT_CUR").GetString();
                string checkValue = DataTableConverter.GetValue(e.Row.DataItem, "CHK").GetString();
                string carrier = DataTableConverter.GetValue(e.Row.DataItem, "DURABLE_ID").GetString();
                if (idx > -1)
                {
                    string selectedequipmentCode = DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_CUR").GetString();
                    if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EQPT_CUR")).Equals(selectedequipmentCode))
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                DataTable dt = ((DataView)dg.ItemsSource).Table;
                int chkCount = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["CHK"].ToString() == "1")
                    {
                        chkCount = chkCount + 1;
                    }
                }

                if (checkValue == "0")
                {
                    if (chkCount < 1)
                    {

                        SetIssuePort(cboOutputPort, currentequipmentCode);
                    }
                }
                else
                {
                    if (chkCount <= 1)
                    {
                         if (cboOutputPort.SelectedItem != null && cboOutputPort.Items.Count > 0)
                        {
                            SetIssuePort(cboOutputPort, string.Empty);
                        }
                    }
                }
            }
        }

        #endregion

        #region 출고 PORT 콤보 조회 : SetIssuePort()
        private void SetIssuePort(C1ComboBox cbo, string equipmentCode)
        {
            try
            {
                cboOutputPort.SelectedIndexChanged -= cboOutputPort_SelectedIndexChanged;
                if (cbo.Items.Count > 0)
                {
                    for (int i = 0; i < cbo.Items.Count; i++)
                    {
                        cbo.Items.RemoveAt(i);
                        i--;
                    }
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;

                inTable.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_STO_PORT_LIST", "RQSTDT", "RSLTDT", inTable);
                //cbo.ItemsSource = dtResult.AsEnumerable();
                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["PORT_ID"].GetString();
                    comboBoxItem.Name = row["TRF_STAT_CODE"].GetString();

                    comboBoxItem.DataContext = row["DST_EQPTID"].GetString();

                    if (row["TRF_STAT_CODE"].GetString() == "OUT_OF_SERVICE")
                    {
                        comboBoxItem.Foreground = new SolidColorBrush(Colors.Red);
                        comboBoxItem.FontWeight = FontWeights.Bold;
                    }
                    cbo.Items.Add(comboBoxItem);
                }
                cboOutputPort.SelectedIndexChanged += cboOutputPort_SelectedIndexChanged;
                if (cbo.Items != null && cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 수동 출고 처리 : SaveManualIssueByEsnb()
        private void SaveManualIssueByEsnb()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_GDS_PLT_ORDER_LIST_UI";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("TO_PORT", typeof(string));
                inDataTable.Columns.Add("USER_ID", typeof(string));
                DataTable inInput = indataSet.Tables.Add("DURABLE_LIST");
                inInput.Columns.Add("DURABLE_ID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["TO_PORT"] = (cboOutputPort.SelectedItem as C1ComboBoxItem).Tag.GetString();
                row["USER_ID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                C1DataGrid dg = dgMtrlList;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inInput.NewRow();
                        row["DURABLE_ID"] = DataTableConverter.GetValue(dg.Rows[i].DataItem, "DURABLE_ID").GetString();
                        inInput.Rows.Add(row);
                      
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,INLOT", "OUT_EQP", (bizResult, bizException) =>
                {

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다
                        PM_MtrlList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationTransferCancel()) return;

                //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;
                C1DataGrid dg = dgMtrlList;
             
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_TRF_CMD_CANCEL";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));


                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "DURABLE_ID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                        PM_MtrlList();

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 수동출고 취소 Validation : ValidationTransferCancel()
        private bool ValidationTransferCancel()
        {
            //C1DataGrid dg = string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER") ? dgIssueTargetInfoByEmptyCarrier : dgIssueTargetInfo;

            C1DataGrid dg;
            dg = dgMtrlList;

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_Util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (!ValidationTransferCancelByEsnb(dg))
            {
                return false;
            }

            return true;


        }

        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
               
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "DURABLE_ID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_CHK_SEL_TRF_CMD_CANCEL_BY_UI", "IN_DATA", "OUT_DATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["RETVAL"].GetString() != "0")
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        private void dgMtrlList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);
                if (cell == null) return;
             
                if(cell.Column.Name.Equals("DURABLE_ID") && cell != null)
                {
                    int rowIdx = cell.Row.Index;
                    DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                    if (drv == null) return;

                    // 팝업 호출
                    MTRL001_218_LOTLIST popupHoldLotlist = new MTRL001_218_LOTLIST { FrameOperation = FrameOperation };
                    object[] parameters = new object[1];
                    parameters[0] = DataTableConverter.GetValue(drv, "DURABLE_ID").GetString();
                    C1WindowExtension.SetParameters(popupHoldLotlist, parameters);

                    popupHoldLotlist.Closed += popupHoldLotlist_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupHoldLotlist.ShowModal()));
                }
              

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void popupHoldLotlist_Closed(object sender, EventArgs e)
        {
            MTRL001_218_LOTLIST popup = sender as MTRL001_218_LOTLIST;
            if (popup != null)
            {

            }
        }
        private void dgMtrlList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "DURABLE_ID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.Cursor = Cursors.Hand;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
              
                }
            }));
        }

        private void dgMtrlList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
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
    }
}
