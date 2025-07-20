/************************************************************************************* 
 Created Date : 2023.10.23
      Creator : Chi Woo
   Decription : 공 Tray 보관/공급 관리
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.23  조영대 : Initial Created.
  2023.12.09  손동혁 : 오프라인 처리 메시지 변경 
  2023.12.25  이의철 : 입고 강제, 출고 강제 체크 박스 수정
  2023.12.28  이의철 : 입고 강제, 출고 강제 체크 박스 미선택시 오류 수정
  2023.12.28  이의철 : btnEmptyTrayOutNEW  추가
  2024.01.16  손동혁 : ESNA 법인 요청 FCS001_165_BTNEMPTYTRAYOUTNEW 속성 2 , 3 으로 버튼 사용 유무 변경 수정 요청
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_165 : UserControl, IWorkArea
    {
        #region Declaration & Constructor         

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private int saveTrayListIndex = 0;
        private string saveTrayListPopupType = string.Empty;
        private string saveTrayListWorkType = string.Empty;
        Util _Util = new Util();
        private bool bFCS001_165_btnEmptyTrayOutNEW = false;  //btnEmptyTrayOutNEW  추가

        public FCS001_165()
        {
            InitializeComponent();
        }

        #endregion Declaration & Constructor 


        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        
            InitCombo();
            IsButtonUse();
            if (IsMesAdmin())
            {
                dgListSupply.Columns["CNVR_LOCATION_ID"].Visibility = Visibility.Visible;
                dgListStorage.Columns["EQPTID"].Visibility = Visibility.Visible;
                dgListPriority.Columns["CNVR_LOCATION_ID"].Visibility = Visibility.Visible;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            // 설비군
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn1 = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR2" };
            string[] arrCondition1 = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORMLGS_EMPTY_CST_EQPT_GR_CODE_UI", "Y" };
            cboEqptTypeSupply.SetDataComboItem(bizRuleName, arrColumn1, arrCondition1, CommonCombo.ComboStatus.ALL, true);

            string[] arrColumn2 = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR1" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORMLGS_EMPTY_CST_EQPT_GR_CODE_UI", "Y" };
            cboEqptTypeStorage.SetDataComboItem(bizRuleName, arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, true);

            string[] sFilter2 = { "FLOOR_CODE" };
            _combo.SetCombo(cboFloorInfoSupply, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter2);
            _combo.SetCombo(cboFloorInfoStorage, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter2);
        }
        #endregion Initialize

        #region Event

        private void btnSearchSupply_Click(object sender, RoutedEventArgs e)
        {
            GetListSupply();
        }

        private void btnSearchStorage_Click(object sender, RoutedEventArgs e)
        {
            GetListStorage();
        }

        private void btnForcedRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_165_FORCED_RELEASE popForcedRelease = new FCS001_165_FORCED_RELEASE();
                popForcedRelease.FrameOperation = FrameOperation;

                if (popForcedRelease != null)
                {
                    int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(cboEqptTypeStorage.SelectedValue);
                    Parameters[1] = Util.NVC(dgListStorage.GetValue(rowIndex, "TRAY_TYPE_CODE"));
                    Parameters[2] = Util.NVC(dgListStorage.GetValue(rowIndex, "EQPTID"));
                    Parameters[3] = Util.NVC(dgListStorage.GetValue(rowIndex, "EQPTNAME"));

                    C1WindowExtension.SetParameters(popForcedRelease, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => { popForcedRelease.ShowModal(); }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEmptyTrayOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_165_TRAY_OUT popTrayOut = new FCS001_165_TRAY_OUT();
                popTrayOut.FrameOperation = FrameOperation;

                if (popTrayOut != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "";

                    C1WindowExtension.SetParameters(popTrayOut, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => { popTrayOut.ShowModal(); }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEmptyTrayOutNEW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_165_TRAY_OUT_NEW popTrayOut = new FCS001_165_TRAY_OUT_NEW();
                popTrayOut.FrameOperation = FrameOperation;

                if (popTrayOut != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "";

                    C1WindowExtension.SetParameters(popTrayOut, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => { popTrayOut.ShowModal(); }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListSupply_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                    if (datagrid.CurrentRow.Type != DataGridRowType.Item) return;

                    switch (dgListSupply.CurrentColumn.Name)
                    {
                        case "ON_SUPPLY_CNT":
                        case "AUTO_ISS_CNT":
                        case "MANUAL_ISS_CNT":
                        case "OTHERS_CNT":

                            if (gdTrayList.ActualWidth < 50)
                            {
                                gdTrayList.Width = new GridLength(600, GridUnitType.Pixel);
                            }

                            string sWORK_TYPE = string.Empty;

                            if (dgListSupply.CurrentColumn.Name.Equals("ON_SUPPLY_CNT")) sWORK_TYPE = "ON_SUPPLY";
                            else if (dgListSupply.CurrentColumn.Name.Equals("AUTO_ISS_CNT")) sWORK_TYPE = "AUTO_ISS";
                            else if (dgListSupply.CurrentColumn.Name.Equals("MANUAL_ISS_CNT")) sWORK_TYPE = "MANUAL_ISS";
                            else if (dgListSupply.CurrentColumn.Name.Equals("OTHERS_CNT")) sWORK_TYPE = "OTHERS";

                            GetTrayList(cell.Row.Index, "SUPPLY", sWORK_TYPE);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListSupply_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Column.Name.Equals("ON_SUPPLY_CNT") || e.Cell.Column.Name.Equals("AUTO_ISS_CNT") ||
                        e.Cell.Column.Name.Equals("MANUAL_ISS_CNT") || e.Cell.Column.Name.Equals("OTHERS_CNT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListSupply_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                btnSearchSupply.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListStorage_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                    if (datagrid.CurrentRow.Type != DataGridRowType.Item) return;

                    switch (dgListStorage.CurrentColumn.Name)
                    {
                        case "IN_STORAGE_CNT":
                        case "AUTO_ISS_CNT":
                        case "MANUAL_ISS_CNT":
                        case "OTHERS_CNT":

                            if (gdTrayList.ActualWidth < 50)
                            {
                                gdTrayList.Width = new GridLength(600, GridUnitType.Pixel);
                            }

                            string sWORK_TYPE = string.Empty;

                            if (dgListStorage.CurrentColumn.Name.Equals("IN_STORAGE_CNT")) sWORK_TYPE = "IN_STORAGE";
                            else if (dgListStorage.CurrentColumn.Name.Equals("AUTO_ISS_CNT")) sWORK_TYPE = "AUTO_ISS";
                            else if (dgListStorage.CurrentColumn.Name.Equals("MANUAL_ISS_CNT")) sWORK_TYPE = "MANUAL_ISS";
                            else if (dgListStorage.CurrentColumn.Name.Equals("OTHERS_CNT")) sWORK_TYPE = "OTHERS";

                            GetTrayList(cell.Row.Index, "STORAGE", sWORK_TYPE);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListStorage_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Column.Name.Equals("IN_STORAGE_CNT") || e.Cell.Column.Name.Equals("AUTO_ISS_CNT") ||
                        e.Cell.Column.Name.Equals("MANUAL_ISS_CNT") || e.Cell.Column.Name.Equals("OTHERS_CNT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListStorage_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                btnSearchStorage.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListPriority_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (Util.NVC(dgListPriority.GetValue(e.Row.Index, e.Column.Name)).Equals("Y"))
            {
                //e.Cancel = true;
                //return;
            }
        }

        private void dgListPriority_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            string floorCode = Util.NVC(dgListPriority.GetValue(e.Cell.Row.Index, "FLOOR_CODE"));
            string locationId = Util.NVC(dgListPriority.GetValue(e.Cell.Row.Index, "CNVR_LOCATION_ID"));

            string CHECK = Util.NVC(dgListPriority.GetValue(e.Cell.Row.Index, e.Cell.Column.Name)); //False/True

            if (e.Cell.Column.Name == "FORCE_IN_YN")
            {
                List<DataRow> rowList = dgListPriority.GetDataTable(false).AsEnumerable().Where(w => w.Field<string>("FLOOR_CODE").Equals(floorCode)).ToList();
                foreach (DataRow dr in rowList)
                {
                    if (Util.NVC(dr["CNVR_LOCATION_ID"]).Equals(locationId))
                    {
                        if (dr[e.Cell.Column.Name].Equals("True"))
                        {
                            dr[e.Cell.Column.Name] = "Y";
                        }
                        if (dr[e.Cell.Column.Name].Equals("False"))
                        {
                            dr[e.Cell.Column.Name] = "N";
                        }
                    }
                    else
                    {
                        dr[e.Cell.Column.Name] = "N";
                    }
                }
            }

            if (e.Cell.Column.Name == "FORCE_OUT_YN")
            {
                List<DataRow> rowList = dgListPriority.GetDataTable(false).AsEnumerable().Where(w => w.Field<string>("FLOOR_CODE").Equals(floorCode)).ToList();
                foreach (DataRow dr in rowList)
                {
                    if (Util.NVC(dr["CNVR_LOCATION_ID"]).Equals(locationId))
                    {

                        if (dr[e.Cell.Column.Name].Equals("True"))
                        {
                            dr[e.Cell.Column.Name] = "Y";
                        }
                        if (dr[e.Cell.Column.Name].Equals("False"))
                        {
                            dr[e.Cell.Column.Name] = "N";
                        }
                    }
                    else
                    {
                        dr[e.Cell.Column.Name] = "N";
                    }
                }
            }

        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (dataGrid.GetValue(e.Cell.Row.Index, "EXPIRED_FLAG").Nvc().Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSlateGray);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayList_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;
            if (!dtResult.Columns.Contains("EXPIRED_FLAG"))
            {
                dtResult.Columns.Add("EXPIRED_FLAG");
            }
        }

        private void dgListPriority_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                btnSearchPriority.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnTrayListView_Click(object sender, RoutedEventArgs e)
        {
            gdTrayList.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void btnMaxBufferChange_Click(object sender, RoutedEventArgs e)
        {
            dgListSupply.EndEdit(true);

            // 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                        dtRqst.Columns.Add("CNVR_BUF_LEN_VALUE", typeof(int));
                        dtRqst.Columns.Add("CNVR_BUF_OFFSET", typeof(int));

                        DataRow drRqst = dtRqst.NewRow();
                        drRqst["USERID"] = LoginInfo.USERID;
                        drRqst["CNVR_LOCATION_ID"] = Util.NVC(dgListSupply.GetValue(rowIndex, "CNVR_LOCATION_ID"));
                        drRqst["CNVR_BUF_LEN_VALUE"] = Util.NVC_Int(dgListSupply.GetValue(rowIndex, "CNVR_BUF_LEN_VALUE"));
                        drRqst["CNVR_BUF_OFFSET"] = Util.NVC_Int(dgListSupply.GetValue(rowIndex, "CNVR_BUF_OFFSET_VALUE"));
                        dtRqst.Rows.Add(drRqst);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_UPD_CNVR_LOCATION_BUF_INFO", "RQSTDT", "RSLTDT", dtRqst);

                        Util.AlertInfo("SFU1166");  //변경되었습니다.

                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnSearchPriority_Click(object sender, RoutedEventArgs e)
        {
            GetListPriority();
        }

        private void btnSavePriority_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> checkFloorCode = new List<string>();
                DataTable dtPriority = dgListPriority.GetDataTable();
                foreach(DataRow drChk in dtPriority.AsEnumerable()
                    .Where(w => w.Field<string>("FORCE_IN_YN").Equals("Y") || w.Field<string>("FORCE_OUT_YN").Equals("Y")
                                || w.Field<string>("FORCE_IN_YN").Equals("True") || w.Field<string>("FORCE_OUT_YN").Equals("True")
                                || w.Field<string>("FORCE_IN_YN").Equals("N") || w.Field<string>("FORCE_OUT_YN").Equals("N")
                                || w.Field<string>("FORCE_IN_YN").Equals("False") || w.Field<string>("FORCE_OUT_YN").Equals("False")
                                ))
                {
                    if (!checkFloorCode.Contains(drChk["FLOOR_NAME"].Nvc())) checkFloorCode.Add(drChk["FLOOR_NAME"].Nvc());
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CODE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("FORCE_IN_YN", typeof(string));
                dtRqst.Columns.Add("FORCE_OUT_YN", typeof(string));

                foreach (string floorName in checkFloorCode)
                {
                    List<DataRow> rowList = dtPriority.AsEnumerable()
                        .Where(w => w.Field<string>("FLOOR_NAME").Equals(floorName)).ToList();

                    foreach (DataRow dr in rowList)
                    {
                        DataRow drRqst = dtRqst.NewRow();
                        drRqst["USERID"] = LoginInfo.USERID;
                        drRqst["FLOOR_CODE"] = Util.NVC(dr["FLOOR_CODE"]);
                        drRqst["CNVR_LOCATION_ID"] = Util.NVC(dr["CNVR_LOCATION_ID"]);
                        drRqst["FORCE_IN_YN"] = Util.NVC(dr["FORCE_IN_YN"]).Replace("True","Y").Replace("False","N");
                        drRqst["FORCE_OUT_YN"] = Util.NVC(dr["FORCE_OUT_YN"]).Replace("True", "Y").Replace("False", "N");
                        dtRqst.Rows.Add(drRqst);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_LOCATION_PRIORITY_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["RETVAL"]).Equals("1"))
                    {
                        //Util.AlertInfo("SFU2964");  //처리하였습니다.
                    }
                    else
                    {
                        string msg = "[*]" + MessageDic.Instance.GetMessage("SFU2964") + " - " + Util.NVC(dtResult.Rows[0]["RSLT_MSG"]);
                        Util.AlertInfo(msg);  //처리되지 않았습니다.
                        return;
                    }
                }

                GetListPriority();

                Util.AlertInfo("SFU1166");  //변경되었습니다.

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOffLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable chkTable = dgTrayList.GetDataTable();

                if (!dgTrayList.IsCheckedRow("CHK"))
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                //처리 하시겠습니까?
                Util.MessageConfirm("SFU1925", (result) =>
                {
                    try
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("CSTID", typeof(string));
                            dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                            dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                            
                            foreach (DataRow chkRow in dgTrayList.GetCheckedDataRow("CHK"))
                            {
                                DataRow drRqst = dtRqst.NewRow();
                                drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;
                                drRqst["USERID"] = LoginInfo.USERID;
                                drRqst["CSTID"] = chkRow["CSTID"].Nvc();
                                drRqst["TRAY_TYPE_CODE"] = chkRow["TRAY_TYPE_CODE"].Nvc();
                                drRqst["CNVR_LOCATION_ID"] = chkRow["CNVR_LOCATION_ID"].Nvc();
                                dtRqst.Rows.Add(drRqst);
                            }

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_CST_CNVR_LOCATION_INIT", "INDATA", "OUTDATA", dtRqst);
                            if (dtResult != null && dtResult.Rows.Count > 0)
                            {
                                if (Util.NVC(dtResult.Rows[0]["RETVAL"]).Equals("1"))
                                {
                                    Util.AlertInfo("SFU1275");  //정상처리되었습니다..

                                    GetTrayList(saveTrayListIndex, saveTrayListPopupType, saveTrayListWorkType);
                                }
                                else
                                {
                                    string msg = "[*]" + MessageDic.Instance.GetMessage("SFU2964") + " - " + Util.NVC(dtResult.Rows[0]["RSLT_MSG"]);
                                    Util.AlertInfo(msg);  //처리되지 않았습니다.
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Event


        #region Method

        private void GetListSupply()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CODE", typeof(string));                
                dtRqst.Columns.Add("EQPT_GR_CODE", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FLOOR_CODE"] = cboFloorInfoSupply.GetBindValue();
                dr["EQPT_GR_CODE"] = cboEqptTypeSupply.GetBindValue();
                dr["TRAY_TYPE_CODE"] = txtTrayTypeSupply.GetBindValue();
                dtRqst.Rows.Add(dr);

                btnSearchSupply.IsEnabled = false;

                // 백그라운드 실행. 
                dgListSupply.ExecuteService("DA_SEL_EMPTY_CST_SUPPLY_STATE", "RQSTDT", "RSLTDT", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListStorage()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FLOOR_CODE", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_CODE", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FLOOR_CODE"] = cboFloorInfoStorage.GetBindValue();
                dr["EQPT_GR_CODE"] = cboEqptTypeStorage.GetBindValue();
                dr["TRAY_TYPE_CODE"] = txtTrayTypeStorage.GetBindValue();
                dtRqst.Rows.Add(dr);

                btnSearchStorage.IsEnabled = false;

                // 백그라운드 실행. 
                dgListStorage.ExecuteService("DA_SEL_EMPTY_CST_STORAGE_STATE", "INDATA", "OUTDATA", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListPriority()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                btnSearchPriority.IsEnabled = false;

                // 백그라운드 실행. 
                dgListPriority.ExecuteService("DA_SEL_CNVR_LOCATION_PRIORITY_INFO", "RQSTDT", "RSLTDT", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayList(int rowIndex, string sPOPUP_TYPE, string sWORK_TYPE)
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("SUPPLY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("STORAGE_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                string bizName = string.Empty;

                switch (sPOPUP_TYPE)
                {
                    case "SUPPLY":
                        bizName = "DA_SEL_EMPTY_CST_SUPPLY_DETAIL";

                        dr["SUPPLY_TYPE_CODE"] = sWORK_TYPE;
                        dr["CNVR_LOCATION_ID"] = Util.NVC(dgListSupply.GetValue(rowIndex, "CNVR_LOCATION_ID"));
                        dr["TRAY_TYPE_CODE"] = Util.NVC(dgListSupply.GetValue(rowIndex, "TRAY_TYPE_CODE"));
                        break;
                    case "STORAGE":
                        bizName = "DA_SEL_EMPTY_CST_STORAGE_DETAIL";

                        dr["STORAGE_TYPE_CODE"] = sWORK_TYPE;
                        dr["EQPTID"] = Util.NVC(dgListStorage.GetValue(rowIndex, "EQPTID"));
                        dr["TRAY_TYPE_CODE"] = Util.NVC(dgListStorage.GetValue(rowIndex, "TRAY_TYPE_CODE"));
                        break;
                    default:
                        return;
                }

                dtRqst.Rows.Add(dr);

                saveTrayListIndex = rowIndex;
                saveTrayListPopupType = sPOPUP_TYPE;
                saveTrayListWorkType = sWORK_TYPE;

                dgTrayList.ExecuteService(bizName, "RQSTDT", "RSLTDT", dtRqst, true, true, dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool IsMesAdmin()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("USERID");
            dt.Columns.Add("AUTHID");

            DataRow dr = dt.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["AUTHID"] = "MESDEV";
            dt.Rows.Add(dr);

            DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "INDATA", "OUTDATA", dt);
            if (dtAuth != null &&  dtAuth.Rows.Count > 0 )
            {
                return true;
            }

            return false;
        }

        #endregion Method


        private void IsButtonUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "PROGRAM_BY_FUNC_USE_FLAG";
                dr["COM_CODE"] = "FCS001_165_BTNEMPTYTRAYOUTNEW";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR1"].ToString().Equals("Y") && dtResult.Rows[0]["ATTR2"].ToString().Equals("Y"))
                {
                    btnEmptyTrayOut.Visibility = Visibility.Visible;
                }
                else
                {
                    btnEmptyTrayOut.Visibility = Visibility.Collapsed;
                }
                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR1"].ToString().Equals("Y") && dtResult.Rows[0]["ATTR3"].ToString().Equals("Y"))
                {
                    btnEmptyTrayOutNEW.Visibility = Visibility.Visible;
                }
                else
                {
                    btnEmptyTrayOutNEW.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

    }
}
