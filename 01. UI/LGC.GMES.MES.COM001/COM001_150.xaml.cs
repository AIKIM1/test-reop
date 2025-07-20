/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.13  이제섭    : CNB 법인은 활성화 불량분석(Selector) 컬럼 보이고 나머지 법인은 숨김처리.
  2020.07.21  이제섭    : UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2020.10.13            : 변경집합 번호 34689, 2020-08-03일 버젼으로 롤백처리함
  2023.04.06  성민식    : 특정 Tray 조회 하여 수동배출 / Tray 그룹 해제 시 해당 TrayID 조회하여 출력
  2023.04.25  성민식    : 간헐적으로 수동배출 안 되는 현상으로 인한 주석처리
  2023.04.26  성민식    : 특정 Tray 조회 하여 수동배출 / Tray 그룹 해제 시 해당 TrayID 조회하여 출력 - 수정
  2023.07.05  성민식    : 수동배출 / Tray 그룹 해제 시 CSTID, 입력 TrayID 비교 로직 수정
  2023.11.13  이병윤    : 검색조건 라인 삭제, 현황 라인추가, GetDetail 메소드 sEqsgId 추가
**************************************************************************************/

using C1.WPF;
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using System.Linq;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid.Summaries;



namespace LGC.GMES.MES.COM001
{
    public partial class COM001_150 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private C1ComboBox cboOperation = new C1ComboBox(); // LOT
        string sLogis_Zone = string.Empty;
        string sTrayStat = string.Empty;
        string sEqsgId = string.Empty;
        public COM001_150()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public MessageBoxResult DialogResult { get; private set; }

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>


        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE);

            //라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            ////C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent);

        }
        #endregion


        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            btnGroupdel.IsEnabled = false;                
            btnEmptyTray.IsEnabled = false;
            btnUseTray.IsEnabled = false;

        }
        


        private void dgTray_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv != null)
            {
                //if (dgTray.Columns["SUM_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["SUM_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["SUM_QTY2"]).Equals(Convert.ToDouble(drv["SUM_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgTray.Columns["HOLD_LOT_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["HOLD_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(drv["HOLD_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgTray.Columns["MOVING_LOT_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["MOVING_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(drv["MOVING_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }
        private void dgTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                //Grid Data Binding 이용한 Background 색 변경

                //string sLogis_Zone = string.Empty;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {                 
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if ( ((e.Cell.Column.Name == "TOTAL_QTY" || e.Cell.Column.Name == "USE_QTY" || e.Cell.Column.Name == "EMPTY_QTY"))&&
                          (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOGIS_ZONE"))))
                        )
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }


                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{

                    //    sLogis_Zone = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOGIS_ZONE"));

                    //    if (sLogis_Zone =="" || sLogis_Zone == " ")
                    //    {
                    //        e.Cell.Presenter.Background = null;
                    //        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    //        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    //    }
                    //    //else
                    //    //{
                    //    //    e.Cell.Presenter.Background = null;
                    //    //    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //    //}
                    //}

                    //Grid Data Binding 이용한 Background 색 변경
                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{
                    //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_STCK_FLAG")).Equals("N"))
                    //    {
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    //        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    //    }
                    //    else
                    //    {
                    //        e.Cell.Presenter.Background = null;
                    //        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //    }
                    //}

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }        

        private void dgTrayDetail_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv != null)
            {
                //if (dgTray.Columns["SUM_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["SUM_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["SUM_QTY2"]).Equals(Convert.ToDouble(drv["SUM_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgTray.Columns["HOLD_LOT_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["HOLD_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(drv["HOLD_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgTray.Columns["MOVING_LOT_QTY2"].Visibility == Visibility.Visible && dgTray.Columns["MOVING_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(drv["MOVING_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }
        private void dgTrayDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                //Grid Data Binding 이용한 Background 색 변경

                //string sLogis_Zone = string.Empty;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "CSTID" || e.Cell.Column.Name == "CURR_LOTID")                        
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region []
        //private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //    Util.gridClear(dgTray);
        //    Util.gridClear(dgTrayDetail);

        //    //string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);
        //    //if (sEqsgid == "" || sEqsgid == "SELECT")
        //    //{
        //    //    sEqsgid = "";
        //    //}
        //    //_sPalletID = string.Empty;

        //    ////범례 
        //    //if (sEqsgid == "A7A24")
        //    //{
        //    //    Color.Visibility = Visibility.Visible;
        //    //}
        //    //else
        //    //{
        //    //    Color.Visibility = Visibility.Collapsed;
        //    //}
        //    //GetPilotProdMode();
        //    //String[] sFilter = { sEqsgid, Process.CELL_BOXING };
        //    //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter:sFilter);
        //}
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();

            btnGroupdel.IsEnabled = false;
            btnEmptyTray.IsEnabled = false;
            btnUseTray.IsEnabled = false;
        }
        #endregion

        #region [### 현황 조회 ###]
        public void GetLotList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("AREAID", typeof(string));
                //dtRqst.Columns.Add("EQSGID", typeof(string));               

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);  //Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;
                                         
                dtRqst.Rows.Add(dr);

                Util.gridClear(dgTray);
                Util.gridClear(dgTrayDetail);
                txtTrayId.Text = string.Empty;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_CYCLE_MON", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);

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
        #endregion

        #region [### 상세 조회 ###]
        private void dgTray_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    C1DataGrid dg = sender as C1DataGrid;

                    if (dg == null) return;

                    C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                    if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                    if (Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "LOGIS_ZONE")).Equals("")
                    || string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "LOGIS_ZONE"))))
                        return;

                    if (dg.CurrentColumn.Name == "TOTAL_QTY" || dg.CurrentColumn.Name == "USE_QTY" || dg.CurrentColumn.Name == "EMPTY_QTY")
                    {
                        sLogis_Zone = string.Empty;
                        sTrayStat = string.Empty;
                        sEqsgId = string.Empty;

                        sLogis_Zone = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "LOGIS_ZONE"));
                        sEqsgId = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "EQSGID"));

                        if (dg.CurrentColumn.Name == "TOTAL_QTY")
                        {
                            //newRow["CSTSTAT"] = string.Empty;
                            sTrayStat = null;
                        }
                        else if (dg.CurrentColumn.Name == "USE_QTY")
                        {
                            sTrayStat  = "U";
                        }
                        else if (dg.CurrentColumn.Name == "EMPTY_QTY")
                        {
                            sTrayStat = "E";
                        }


                        if (sLogis_Zone == "D")
                        {
                            btnGroupdel.IsEnabled = true;

                            if (dg.CurrentColumn.Name == "USE_QTY")
                            {
                                btnEmptyTray.IsEnabled = true;
                                btnUseTray.IsEnabled = false;
                            }
                            else if (dg.CurrentColumn.Name == "EMPTY_QTY")
                            {
                                btnEmptyTray.IsEnabled = false;
                                btnUseTray.IsEnabled = true;
                            }
                            else
                            {
                                btnEmptyTray.IsEnabled = false;
                                btnUseTray.IsEnabled = false;
                            }                                                       
                        }
                        else
                        {
                            btnGroupdel.IsEnabled = false;
                            btnEmptyTray.IsEnabled = false;
                            btnUseTray.IsEnabled = false;
                        }


                        Util.gridClear(dgTrayDetail);
                        txtTrayId.Text = string.Empty;
                        GetDetail(sLogis_Zone, sTrayStat, sEqsgId);
                    }
                    else
                    {
                        return;
                    }


                    
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));

        }        

        private void GetDetail(string logisZone, string trayStat, string eqsgid = null)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOGIS_ZONE", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                

                DataRow newRow = inTable.NewRow();
                //newRow["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue); //Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                newRow["EQSGID"] = eqsgid;
                newRow["LOGIS_ZONE"] = logisZone;
                newRow["CSTSTAT"] = trayStat;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_CYCLE_MON_DETAIL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgTrayDetail, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region [Tray id 조회]
        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                Util.gridClear(dgTrayDetail);
                GetDetail2();
            }
        }
        private void GetDetail2()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));                
                inTable.Columns.Add("CSTID", typeof(string));
                
                DataRow newRow = inTable.NewRow();

                //newRow["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue); //Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));

                if (Util.GetCondition(txtTrayId).Equals("")) return;
                newRow["CSTID"] = Util.GetCondition(txtTrayId);
                newRow["EQSGID"] = sEqsgId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_CYCLE_MON_DETAIL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgTrayDetail, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }
        

        #region [Tray id 처리 상세 이력 조회]
        private void dgTrayDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                //datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
                return;
            }

            string sCstId = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["CSTID"].Index).Value);
            string sLotId = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["CURR_LOTID"].Index).Value);

            if (datagrid.CurrentColumn.Name == "CSTID")
            {
                COM001_150_CSTID_HIST popUp = new COM001_150_CSTID_HIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = sCstId;                    

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndTrayHist_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            else if (datagrid.CurrentColumn.Name == "CURR_LOTID")
            {
                COM001_150_LOTID_HIST popUp = new COM001_150_LOTID_HIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sLotId.Equals("") ? sCstId : sLotId;
                    Parameters[1] = sLotId.Equals("") ? "BOX" : "LOT";

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndLotHist_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndTrayHist_Closed(object sender, EventArgs e)
        {
            COM001.COM001_150_CSTID_HIST wndPopup = sender as COM001.COM001_150_CSTID_HIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndLotHist_Closed(object sender, EventArgs e)
        {
            COM001.COM001_150_LOTID_HIST wndPopup = sender as COM001.COM001_150_LOTID_HIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(wndPopup);
        }
        #endregion

        #region [조립투입금지처리]
        private void btnAssyNotInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            try
            {

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;

                string sLoad_Rep_Cstid = string.Empty;                

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("PRHB_FLAG", typeof(string));


                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOAD_REP_CSTID"]).Equals(""))
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["LOAD_REP_CSTID"]);                            
                            break;
                        }
                    }
                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = sLoad_Rep_Cstid;
                newRow["PRHB_FLAG"] = "Y";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_ASSY_IN_PRHB_TRAY", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        // 라인추가
                        GetDetail(sLogis_Zone, sTrayStat, sEqsgId);

                        this.DialogResult = MessageBoxResult.OK;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [조립투입금지 취소처리]
        private void btnAssyNotInput_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            try
            {

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;

                string sLoad_Rep_Cstid = string.Empty;

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("PRHB_FLAG", typeof(string));


                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOAD_REP_CSTID"]).Equals(""))
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["LOAD_REP_CSTID"]);
                            break;
                        }
                    }
                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = sLoad_Rep_Cstid;
                newRow["PRHB_FLAG"] = "N";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_ASSY_IN_PRHB_TRAY", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        GetDetail(sLogis_Zone, sTrayStat, sEqsgId);

                        this.DialogResult = MessageBoxResult.OK;
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [수동배출처리]
        private void btnMan_Out_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            try
            {
                bool isDetail = false;
                string sTxt_Trayid = txtTrayId.Text;

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;
                DataTable dt2 = ((DataView)dgTrayDetail.ItemsSource).Table.Copy();

                string sLoad_Rep_Cstid = string.Empty;

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("OUT_IN_GUBUN", typeof(string));
                inTable.Columns.Add("USER_FLAG", typeof(string));


                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (Util.NVC(row["LOAD_REP_CSTID"]).Equals(""))
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["CSTID"]);
                            break;
                        }
                        else
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["LOAD_REP_CSTID"]);
                            break;
                        }
                    }
                }
                
                if (!String.IsNullOrEmpty(sTxt_Trayid))
                {
                    if (sTxt_Trayid == sLoad_Rep_Cstid)
                    {
                        isDetail = true;
                    }
                    else
                    {
                        foreach (DataRow row in dt2.Rows)
                        {
                            if (Convert.ToBoolean(row["CHK"]))
                            {
                                if (Util.NVC(row["CSTID"]).Equals(sTxt_Trayid))
                                {
                                    isDetail = true;
                                    break;
                                }
                            }
                        }
                    }
                }




                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = sLoad_Rep_Cstid;
                //newRow["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                newRow["OUT_IN_GUBUN"] = "O";
                newRow["USER_FLAG"] = "U";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_MANUAL_OUT_TRAY", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        
                        if (isDetail)
                        {
                            GetDetail2();
                        }
                        else
                        {
                            GetDetail("D", sTrayStat, sEqsgId);
                        }

                        this.DialogResult = MessageBoxResult.OK;

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [수동배출처리취소]
        private void btnMan_Out_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            try
            {

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;

                string sLoad_Rep_Cstid = string.Empty;

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("OUT_IN_GUBUN", typeof(string));
                inTable.Columns.Add("USER_FLAG", typeof(string));


                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (Util.NVC(row["LOAD_REP_CSTID"]).Equals(""))
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["CSTID"]);
                            break;
                        }
                        else
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["LOAD_REP_CSTID"]);
                            break;
                        }
                    }
                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = sLoad_Rep_Cstid;
                //newRow["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                newRow["OUT_IN_GUBUN"] = "C";
                newRow["USER_FLAG"] = "U";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_MANUAL_OUT_TRAY", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        GetDetail("C", sTrayStat, sEqsgId);

                        this.DialogResult = MessageBoxResult.OK;

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [엔지니어 Tray 강제 배출처리]
        private void btnManModify_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            try
            {

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;

                string sLoad_Rep_Cstid = string.Empty;

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("OUT_IN_GUBUN", typeof(string));
                inTable.Columns.Add("USER_FLAG", typeof(string));


                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (Util.NVC(row["LOAD_REP_CSTID"]).Equals(""))
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["CSTID"]);
                            break;
                        } else 
                        {
                            sLoad_Rep_Cstid = Util.NVC(row["LOAD_REP_CSTID"]);
                            break;
                        }
                    }
                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CSTID"] = sLoad_Rep_Cstid;
                //newRow["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                newRow["OUT_IN_GUBUN"] = "O";
                newRow["USER_FLAG"] = "M";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_MANUAL_OUT_TRAY", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        GetDetail("D", sTrayStat, sEqsgId);

                        this.DialogResult = MessageBoxResult.OK;

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [엔지니어 Tray 그룹해제, 공Tray로 변경, 실Tray로 변경]
        private void btnTGroupInit_Click(object sender, RoutedEventArgs e)
        {
            btnTrayModify(sender, e, "A");
        }

        private void btnTrayEmpty_Click(object sender, RoutedEventArgs e)
        {
            btnTrayModify(sender, e, "B");
        }

        private void btnTrayUse_Click(object sender, RoutedEventArgs e)
        {
            btnTrayModify(sender, e, "C");
        }

        private void btnTrayModify(object sender, RoutedEventArgs e, string sChg_flag)
        {
            if (!ValidationCreate()) return;

            try
            {
                bool isDetail = false;
                string sTxt_Trayid = txtTrayId.Text;

                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgTrayDetail.ItemsSource).Table;

                string sLoad_Rep_Cstid = string.Empty;

                DataTable inTable = inDataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                //inTable.Columns.Add("CSTID", typeof(string));                
                inTable.Columns.Add("CHG_FLAG", typeof(string));
            

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                //newRow["CSTID"] = sLoad_Rep_Cstid;                
                newRow["CHG_FLAG"] = sChg_flag;
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < dgTrayDetail.GetRowCount(); i++)
                {                
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayDetail.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow newRow2 = inLot.NewRow();
                        newRow2["CSTID"] = DataTableConverter.GetValue(dgTrayDetail.Rows[i].DataItem, "CSTID").ToString();

                        inDataSet.Tables["IN_INPUT"].Rows.Add(newRow2);
                        
                        if (Util.NVC(dt.Rows[i]["LOAD_REP_CSTID"]) == sTxt_Trayid || Util.NVC(dt.Rows[i]["CSTID"]) == sTxt_Trayid)
                        {
                            isDetail = true;
                        }
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_TRAY_CHG_INFO", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.gridClear(dgTrayDetail);
                        //txtTrayId.Text = string.Empty;
                        
                        if (isDetail)
                        {
                            GetDetail2();
                        }
                        else
                        {
                            GetDetail("D", sTrayStat, sEqsgId);
                        }

                        this.DialogResult = MessageBoxResult.OK;

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private bool ValidationCreate()
        {
            if (dgTrayDetail.GetRowCount() == 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return false;
            }
            return true;
        }

        private void dgTrayDetail_Checked(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //if (chkGroupSelect.IsChecked == true)
                //{
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                if (drv["LOAD_REP_CSTID"].ToString().Equals("") && drv["CSTID"].ToString().Equals("")) return;

                foreach (DataRowView item in dgTrayDetail.ItemsSource)
                {
                    if (drv["CSTID"].ToString().Equals(item["CSTID"].ToString()) && drv["CSTID"].ToString() != "")
                    {
                        item["CHK"] = true;
                    }
                    else if (drv["LOAD_REP_CSTID"].ToString().Equals(item["LOAD_REP_CSTID"].ToString()) && drv["LOAD_REP_CSTID"].ToString() != "")
                    {
                        item["CHK"] = true;
                    }
                    else
                    {
                        item["CHK"] = false;
                    }                  
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (chkGroupSelect.IsChecked == true)
                //{
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;               

                DataRowView drv = cb.DataContext as DataRowView;

                if (drv["LOAD_REP_CSTID"].ToString().Equals("")) return;

                foreach (DataRowView item in dgTrayDetail.ItemsSource)
                {
                    if (drv["LOAD_REP_CSTID"].ToString().Equals(item["LOAD_REP_CSTID"].ToString()) )
                    {
                        item["CHK"] = false;
                    }                   
                }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [Func]
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
    #endregion
}
#endregion