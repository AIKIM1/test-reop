/*************************************************************************************
 Created Date : 2023.09.23
      Creator : 백광영
   Decription : 조립 원자재 관리 KANBAN
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using System.Threading;
using System.Linq;
using System.Collections;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_390: UserControl, IWorkArea
    {

        public COM001_390()
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
            DateTime firstOfDay = DateTime.Now.AddDays(-7);
            DateTime endOfDay = DateTime.Now;

            dtpDateFrom.IsNullInitValue = true;
            dtpDateTo.IsNullInitValue = true;

            dtpDateFrom.SelectedDateTime = firstOfDay;
            dtpDateTo.SelectedDateTime = endOfDay;

            InitCombo();
            // 라인
            SetEquipmentSegment(cboEquipmentSegment);
            // 자재분류
            SetMtrlClass(cboMtrlClass);
            Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 자동공급유무
            string[] sFilter2 = { "AUTO_SPLY_FLAG" };
            _combo.SetCombo(cboAutoSupply, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 반품사유
            string[] sFilter3 = { "KANBAN_REMAIN_REASON_CODE" };
            _combo.SetCombo(cboReason, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter3);

            // 공정
            SetProcess(cboProcess);
            SetProcess(cboHisProcess);
            // 요청유형
            setComboRequest();
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgConsume);
            Util.gridClear(dgReturn);
            txtKanbanID.Clear();
            txtReturnKanbanID.Clear();
            cboReason.SelectedIndex = 0;
            txtNote.Clear();
        }
        
        private void InitHisGrid()
        {
            Util.gridClear(dgListInventoryHistory);
        }

        private void SetEquipmentSegment(MultiSelectionBox _cb)
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                    _cb.CheckAll();
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetHisEquipmentSegment()
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboHisEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
                cboHisEquipmentSegment.CheckAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess(C1ComboBox _cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PCSGID"] = "A";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.DisplayMemberPath = "CBO_NAME";
                _cb.SelectedValuePath = "CBO_CODE";
                _cb.ItemsSource = AddStatus(dtResult, "CBO_CODE", "CBO_NAME", "ALL").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    _cb.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (_cb.SelectedIndex < 0)
                        _cb.SelectedIndex = 0;
                }
                else
                {
                    if (_cb.Items.Count > 0)
                        _cb.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment(C1ComboBox _cb)
        {
            try
            {
                string sProc = _cb.Name.Equals("cboEquipment") ? Util.GetCondition(cboProcess) : Util.GetCondition(cboHisProcess);

                string sEquipmentSegment = _cb.Name.Equals("cboEquipment") ? cboEquipmentSegment.SelectedItemsToString : cboHisEquipmentSegment.SelectedItemsToString;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = string.IsNullOrWhiteSpace(sProc) ? null : sProc;   
                dr["PCSGID"] = "A";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.DisplayMemberPath = "CBO_NAME";
                _cb.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                _cb.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    _cb.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (_cb.SelectedIndex < 0)
                        _cb.SelectedIndex = 0;
                }
                else
                {
                    _cb.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setComboRequest()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "KANBAN_REQ_STAT_CODE";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_KANBAN_GROUP_STATUS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboReqStatus.DisplayMemberPath = "CBO_NAME";
                cboReqStatus.SelectedValuePath = "CBO_CODE";
                cboReqStatus.ItemsSource = AddStatus(result2, "CBO_CODE", "CBO_NAME", "ALL").Copy().AsDataView();

                cboReqStatus.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetMtrlClass(MultiSelectionBox _cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ASSY_MTRL_CLSS2_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.ItemsSource = DataTableConverter.Convert(dtResult);
                _cb.CheckAll();
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            InitGrid();
            getStockList(); 
        }

        private void btnSearchResult_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

            //if (timeSpan.Days > 7)
            //{
            //    //dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.Date.AddDays(+7);
            //    HiddenLoadingIndicator();
            //    Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.                    
            //    return;
            //}

            if (Util.NVC(cboHisEquipmentSegment.SelectedItemsToString) == "")
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return;
            }
            if (Util.NVC(cboHisMtrlClass.SelectedItemsToString) == "")
            {
                // 자재분류를 선택하세요.
                Util.MessageValidation("SFU8676");
                return;
            }

            InitHisGrid();

            getStockHistoryList();
        }

        private void btnMaterialRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_390_REQUEST_MTRL reqpopup = new COM001_390_REQUEST_MTRL();
                reqpopup.FrameOperation = this.FrameOperation;

                if (reqpopup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = Util.NVC(cboProcess.SelectedValue.ToString());
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue.ToString());
                    Parameters[3] = Util.NVC(cboMtrlClass.SelectedItemsToString);
                    C1WindowExtension.SetParameters(reqpopup, Parameters);

                    reqpopup.Closed -= reqpopup_Closed;
                    reqpopup.Closed += reqpopup_Closed;

                    reqpopup.ShowModal();
                    reqpopup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;
                bool bSply = (bool)((System.Windows.Controls.Primitives.ToggleButton)e.OriginalSource).IsChecked;

                string sMSG = bSply == true ? "SUF9005" : "SUF9006";

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgList.Rows[idx].DataItem;

                if (objRowIdx != null)
                {
                    // SUF9005 : 해당 랙/자재는 소진시 자동요청 되도록 설정 하시겠습니까?, SUF9006 : 해당 랙/자재는 소진시 자동요청 되지 않도록 설정 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMSG), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result.ToString().Equals("OK"))
                        {
                            DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "AUTO_SPLY_FLAG", bSply);

                            _setAutoSplyFlag(idx, bSply);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "AUTO_SPLY_FLAG", bSply == true ? false : true);
                        }
                    }
                    );

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name == null)
                    return;

                string _col = e.Cell.Column.Name.ToString();

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null)
                    {
                        // on Hand
                        decimal _onhandqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ON_HAND"));
                        // Delivering
                        decimal _delieryqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DELIVERING"));
                        // 주의수량
                        decimal _catnqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CATN_STCK_QTY"));
                        // 위험수량
                        decimal _dngrqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DNGR_STCK_QTY"));
                        if (_col == "ON_HAND" || _col == "DELIVERING" || _col == "REQUEST")
                        {
                            if ((_onhandqty + _delieryqty) > _catnqty)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050"));
                            }
                            else if ((_onhandqty + _delieryqty) > _dngrqty)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000"));
                            }
                            else
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                            }
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                    }
                }
            }));
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.GetRowCount() > 0)
            {
                string _mtrlPortID = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "MTRL_PORT_ID"));
                string _mtrlClsscode = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "MTRL_CLSS_CODE"));
                int _request = Util.NVC_Int(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQUEST"));
                int _onhand = Util.NVC_Int(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "ON_HAND"));
                int _delivering = Util.NVC_Int(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "DELIVERING"));

                // Request : 자재요청 취소
                if (dgList.CurrentColumn.Name.Equals("REQUEST") && _request > 0)
                {
                    // 자재 요청 취소 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU9006"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result.ToString().Equals("OK"))
                        {
                            _CancelMaterial(_mtrlPortID);
                        }
                    });
                }
                // On Hand 상세정보 조회
                if (dgList.CurrentColumn.Name.Equals("ON_HAND") && _onhand > 0)
                {
                    try
                    {
                        COM001_390_ONHAND onhandopup = new COM001_390_ONHAND();
                        onhandopup.FrameOperation = this.FrameOperation;

                        if (onhandopup != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = _mtrlPortID;
                            Parameters[1] = "COMPLETE";

                            C1WindowExtension.SetParameters(onhandopup, Parameters);

                            onhandopup.ShowModal();
                            onhandopup.CenterOnScreen();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                // Delivering 상세정보 조회
                if (dgList.CurrentColumn.Name.Equals("DELIVERING") && _delivering > 0)
                {
                    try
                    {
                        COM001_390_DELIVERING onhandopup = new COM001_390_DELIVERING();
                        onhandopup.FrameOperation = this.FrameOperation;

                        if (onhandopup != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = _mtrlClsscode;
                            Parameters[1] = "DELIVERING";
                            Parameters[2] = _mtrlPortID;

                            C1WindowExtension.SetParameters(onhandopup, Parameters);

                            onhandopup.Closed -= deliveringpopup_Closed;
                            onhandopup.Closed += deliveringpopup_Closed;

                            onhandopup.ShowModal();
                            onhandopup.CenterOnScreen();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            }
        }

        private void btnRowDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgConsume.ItemsSource == null || dgConsume.Rows.Count < 0)
                return;

            DataTable dt = (dgConsume.ItemsSource as DataView).Table;

            _GridDelete(dt, dgConsume);
        }

        private void btnConsume_Click(object sender, RoutedEventArgs e)
        {
            if (dgConsume.ItemsSource == null || dgConsume.Rows.Count < 0)
                return;

            try
            {
                DataTable dt = ((DataView)dgConsume.ItemsSource).Table;

                var query = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true));
                if (query.Count() <= 0)
                {
                    Util.MessageValidation("SFU3538");    //선택된 데이터가 없습니다
                    return;
                }

                var querydelivery = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true) && x.Field<string>("REQ_STAT_CODE").Equals("DELIVERING"));
                if (querydelivery.Count() > 0)
                {
                    Util.MessageValidation("SFU9009");    //Complete 처리 후 진행 하세요
                    return;
                }

                // 소진처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU9004"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        _ConsumeMaterial();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRowClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgConsume);
            txtKanbanID.Clear();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (dgReturn.ItemsSource == null || dgReturn.Rows.Count < 0)
                return;

            try
            {
                DataTable dt = ((DataView)dgReturn.ItemsSource).Table;

                var query = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true));
                if (query.Count() <= 0)
                {
                    Util.MessageValidation("SFU1651");    //선택된 데이터가 없습니다
                    return;
                }

                string _reason = Util.GetCondition(cboReason, "SFU1593"); //사유를 선택하세요..
                if (_reason.Equals(""))
                {
                    return;
                }

                string _note = Util.GetCondition(txtNote, "SFU1590"); //비고를 입력 하세요.
                if (_note.Equals(""))
                {
                    return;
                }

                var querydelivery = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true) && x.Field<string>("REQ_STAT_CODE").Equals("DELIVERING"));
                if (querydelivery.Count() > 0)
                {
                    Util.MessageValidation("SFU9009");    //Complete 처리 후 진행 하세요
                    return;
                }

                // 반품처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU9005"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        _ReturnMaterial();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReturnRowDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgReturn.ItemsSource == null || dgReturn.Rows.Count < 0)
                return;

            DataTable dt = (dgReturn.ItemsSource as DataView).Table;

            _GridDelete(dt, dgReturn);
        }

        private void btnReturnRowClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgReturn);
            txtReturnKanbanID.Clear();
            cboReason.SelectedIndex = 0;
            txtNote.Clear();
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment(cboEquipment);
            }
        }

        private void cboHisProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboHisProcess.Items.Count > 0 && cboHisProcess.SelectedValue != null && !cboHisProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment(cboHisEquipment);
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            //{
            //    SetProcess(cboProcess);
            //}
        }

        private void cboHisEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboHisEquipmentSegment.Items.Count > 0 && cboHisEquipmentSegment.SelectedValue != null && !cboHisEquipmentSegment.SelectedValue.Equals("SELECT"))
            //{
            //    SetProcess(cboHisProcess);
            //}
        }

        private void txtKanbanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtKanbanID.Text.Equals("") || txtKanbanID.Text == null) return;

                getKanban(txtKanbanID.Text, txtKanbanID, dgConsume, "TERM");
            }
        }

        private void txtReturnKanbanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtReturnKanbanID.Text.Equals("") || txtReturnKanbanID.Text == null) return;

                getKanban(txtReturnKanbanID.Text, txtReturnKanbanID, dgReturn, "RETURN");
            }
        }

        void reqpopup_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_390_REQUEST_MTRL condpopup = sender as COM001_390_REQUEST_MTRL;
                if (condpopup.DialogResult == MessageBoxResult.Cancel)
                {
                    InitGrid();
                    getStockList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void deliveringpopup_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_390_DELIVERING condpopup = sender as COM001_390_DELIVERING;
                if (condpopup.DialogResult == MessageBoxResult.Cancel)
                {
                    InitGrid();
                    getStockList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getStockList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                if (Util.NVC(cboEquipmentSegment.SelectedItemsToString) == "")
                {
                    // 라인을 선택하세요.
                    Util.MessageValidation("SFU1223");
                    return;
                }
                if (Util.NVC(cboMtrlClass.SelectedItemsToString) == "")
                {
                    // 자재분류를 선택하세요.
                    Util.MessageValidation("SFU8676");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("CLSS2_CODE", typeof(string));
                RQSTDT.Columns.Add("AUTO_SPLY_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["CLSS2_CODE"] = Util.NVC(cboMtrlClass.SelectedItemsToString);
                dr["AUTO_SPLY_FLAG"] = Util.GetCondition(cboAutoSupply, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_PORT_MTRL_INVENTORY", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    //Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                    return;
                }
                Util.GridSetData(dgList, dtRslt, FrameOperation, false);
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

        private void getStockHistoryList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FR_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MTRL_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("KANBAN_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FR_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; 
                dr["PROCID"] = Util.GetCondition(cboHisProcess, bAllNull: true);
                dr["EQSGID"] = Util.NVC(cboHisEquipmentSegment.SelectedItemsToString);
                dr["EQPTID"] = Util.GetCondition(cboHisEquipment, bAllNull: true);
                dr["MTRL_CLSS_CODE"] = Util.NVC(cboHisMtrlClass.SelectedItemsToString);
                dr["REQ_STAT_CODE"] = Util.GetCondition(cboReqStatus, bAllNull: true);
                dr["MTRLID"] = Util.ConvertEmptyToNull(txtHisMaterial.Text); 
                dr["KANBAN_ID"] = Util.ConvertEmptyToNull(txtHisKanbanID.Text);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_PORT_MTRL_INVENTORYHISTORY", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    //Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                    return;
                }
                Util.GridSetData(dgListInventoryHistory, dtRslt, FrameOperation, true);
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

        private void getKanban(string _kanban, TextBox _tb, C1DataGrid _dg, string _proctype)
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("KANBANID", typeof(string));
                INDATA.Columns.Add("PROC_TYPE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["KANBANID"] = _kanban;
                dr["PROC_TYPE"] = _proctype;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_MTRL_CHK_MTRL_KANBAN", "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {

                        Util.MessageException(Exception, (msgresult) =>
                        {
                            _tb.Clear();
                            _tb.Focus();
                        });

                        return;
                    }

                    if (_dg.GetRowCount() == 0)
                    {
                        Util.GridSetData(_dg, result, FrameOperation, true);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(_dg.ItemsSource);
                        if (result.Rows.Count != 0)
                        {
                            // 중복체크
                            if (dtInfo.Select("KANBAN_ID = '" + Convert.ToString(result.Rows[0]["KANBAN_ID"]) + "'").Count() == 0)
                            {
                                dtInfo.Merge(result);
                                Util.GridSetData(_dg, dtInfo, FrameOperation, true);
                            }
                        }
                    }
                    _tb.Clear();
                    _tb.Focus();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    _tb.Clear();
                    _tb.Focus();
                });
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void _ConsumeMaterial()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dt = ((DataView)dgConsume.ItemsSource).Table;

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("REQ_NO", typeof(string));
                dtIndata.Columns.Add("REQ_QTY", typeof(Int32));
                dtIndata.Columns.Add("UPDUSER", typeof(string));
                dtIndata.Columns.Add("EQPTID", typeof(string));

                DataRow dr = null;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<Boolean>("CHK") == true
                             select new
                             {
                                 _reqno = t.Field<string>("REQ_NO"),
                                 _reqqty = t.Field<Int32>("ISS_QTY"),
                                 _eqptid = t.Field<string>("EQPTID")
                             }).ToList();

                foreach (var x in query)
                {
                    dr = dtIndata.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["REQ_NO"] = x._reqno;
                    dr["REQ_QTY"] = x._reqqty;
                    dr["UPDUSER"] = LoginInfo.USERID;
                    dr["EQPTID"] = x._eqptid;
                    dtIndata.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_TERM_BOX_STCK_KANBAN", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");

                InitGrid();
                getStockList();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void _ReturnMaterial()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dt = ((DataView)dgReturn.ItemsSource).Table;

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("REQ_NO", typeof(string));
                dtIndata.Columns.Add("REQ_QTY", typeof(Int32));
                dtIndata.Columns.Add("RESNCODE", typeof(string));
                dtIndata.Columns.Add("NOTE", typeof(string));
                dtIndata.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = null;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<Boolean>("CHK") == true
                             select new
                             {
                                 _reqno = t.Field<string>("REQ_NO"),
                                 _reqqty = t.Field<Int32>("ISS_QTY")
                             }).ToList();

                foreach (var x in query)
                {
                    dr = dtIndata.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["REQ_NO"] = x._reqno;
                    dr["REQ_QTY"] = x._reqqty;
                    dr["NOTE"] = Util.NVC(txtNote.Text);
                    dr["RESNCODE"] = Util.GetCondition(cboReason);
                    dr["UPDUSER"] = LoginInfo.USERID;

                    dtIndata.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_REMAIN_BOX_STCK_KANBAN", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");
                InitGrid();
                getStockList();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void _CancelMaterial(string _mtrlPortID)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRL_PORT_ID"] = _mtrlPortID;
                dr["USERID"] = LoginInfo.USERID;

                dtIndata.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_CANCEL_BOX_STCK_KANBAN", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");
                InitGrid();
                getStockList();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void _setAutoSplyFlag(int idx, bool apy)
        {
            try
            {
                if (dgList == null)
                {
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                string sMTRL_PORT_ID = string.Empty;

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("MTRL_PORT_ID", typeof(string));
                INDATA.Columns.Add("AUTO_SPLY_FLAG", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                sMTRL_PORT_ID = Util.NVC(dgList.GetCell(idx, dgList.Columns["MTRL_PORT_ID"].Index).Value);

                DataRow dr = INDATA.NewRow();
                dr["MTRL_PORT_ID"] = sMTRL_PORT_ID;
                dr["AUTO_SPLY_FLAG"] = apy == true ? "Y" : "N";
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_MTRL_UPD_PORT_AUTO_INFO", "INDATA", null, INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (apy)
                        Util.MessageInfo("SUF9016"); //해당 랙/자재는 소진시 자동요청 되도록 설정 되었습니다.
                    else
                        Util.MessageInfo("SUF9004"); //해당랙/자재는 소진시 자동요청 되지 않도록 설정 되었습니다.
                });
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

        private void _GridDelete(DataTable _dt, C1DataGrid _dg)
        {
            var query = _dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 데이터가 없습니다
                return;
            }
            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                bool _chk;
                if (Boolean.TryParse(Util.NVC(_dt.Rows[i]["CHK"]), out _chk))
                {
                    if (_chk)
                        _dt.Rows.RemoveAt(i);
                }
            }
            Util.GridSetData(_dg, _dt, FrameOperation, true);
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

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment(cboEquipment);
        }

        private void cboHisEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment(cboHisEquipment);
        }

        private void btnConf_Click(object sender, RoutedEventArgs e)
        {
            SetEquipmentSegment(cboHisEquipmentSegment);
        }

        private void tabInventoryHistory_Loaded(object sender, RoutedEventArgs e)
        {
            //SetEquipmentSegment(cboHisEquipmentSegment);
            //SetMtrlClass(cboHisMtrlClass);
        }

        private void cboHisEquipmentSegment_Loaded(object sender, RoutedEventArgs e)
        {
            if (cboHisEquipmentSegment.SelectedItems.Count == 0)
            {
                SetEquipmentSegment(cboHisEquipmentSegment);
            }
            if (cboHisMtrlClass.SelectedItems.Count == 0)
            {
                SetMtrlClass(cboHisMtrlClass);
            }
        }
    }
}