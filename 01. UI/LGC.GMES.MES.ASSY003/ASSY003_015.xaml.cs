/*************************************************************************************
 Created Date : 2017.09.28
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Tray, Cell  정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017. 09. 28  Lee. D. R : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_015 : UserControl, IWorkArea
    {        
        #region Declaration & Constructor
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        #region Popup 처리 로직 변경
        //ASSY003_015_TRAYID wndTray;
        //ASSY003_015_CELLID wndCell;
        #endregion
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_015()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboEquipment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

                //라인
                string sProcID = string.Empty;
                if (LoginInfo.CFG_EQSG_ID == "M9CP1") //NFF 는 Washing 이 마지막 공정
                    sProcID = Process.WASHING;
                else
                    sProcID = Process.PACKAGING;

                String[] sFilter = { null, sProcID };
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "cboEquipmentSegmentAssy");

                //설비
                String[] sFilter1 = { Process.PACKAGING };
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_MAIN_LEVEL");


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetEvent();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region Button Event        
        private void btnTray_Click(object sender, RoutedEventArgs e)
        {
            ASSY003_015_TRAYID wndTrayCreate = new ASSY003_015_TRAYID();
            wndTrayCreate.FrameOperation = FrameOperation;

            if (wndTrayCreate != null)
            {
                //object[] Parameters = new object[3];
                //Parameters[0] = EQPTSEGMENT;
                //Parameters[1] = EQPTID;
                //Parameters[2] = PROD_WOID;
                //C1WindowExtension.SetParameters(wndMAZCreate, Parameters);

                wndTrayCreate.Closed += new EventHandler(wndTrayCreate_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                //{
                //    if (tmp.Name == "grdMain")
                //    {
                //        tmp.Children.Add(wndTrayCreate);
                //        wndTrayCreate.BringToFront();
                //        break;
                //    }
                //}
            }
        }

        private void wndTrayCreate_Closed(object sender, EventArgs e)
        {
            ASSY003_015_TRAYID window = sender as ASSY003_015_TRAYID;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            ASSY003_015_CELLID wndCellCreate = new ASSY003_015_CELLID();
            wndCellCreate.FrameOperation = FrameOperation;

            if (wndCellCreate != null)
            {
                //object[] Parameters = new object[3];
                //Parameters[0] = EQPTSEGMENT;
                //Parameters[1] = EQPTID;
                //Parameters[2] = PROD_WOID;
                //C1WindowExtension.SetParameters(wndMAZCreate, Parameters);

                wndCellCreate.Closed += new EventHandler(wndCellCreate_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndCellCreate.ShowModal()));
                //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                //{
                //    if (tmp.Name == "grdMain")
                //    {
                //        tmp.Children.Add(wndCellCreate);
                //        wndCellCreate.BringToFront();
                //        break;
                //    }
                //}
            }
        }

        private void wndCellCreate_Closed(object sender, EventArgs e)
        {
            ASSY003_015_CELLID window = sender as ASSY003_015_CELLID;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            {
                // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "7");
                return;
            }

            try
            {
                Util.gridClear(dgLotList);
                Util.gridClear(dgTray);
                Util.gridClear(dgCell);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);

                if (LoginInfo.CFG_EQSG_ID == "M9CP1")
                    dr["PROCID"] = Process.WASHING;
                else
                    dr["PROCID"] = Process.PACKAGING;

                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                //Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_PKG_INFO_LOT_LIST", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgLotList, bizResult, FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                // 조회 버튼 클릭시로 변경
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                {
                    // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "7");

                    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                    if (LGCdp.Name.Equals("dtpDateTo"))
                        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+6);
                    else
                        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);

                    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                    return;
                }

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
            */
        }
        #endregion

        #region Grid Click Event
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgLotList.SelectedIndex = idx;

                Search_Tray_List(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID").ToString());
            }
        }

        private void Search_Tray_List(string sTrayID)
        {
            try
            {
                Util.gridClear(dgTray);
                Util.gridClear(dgCell);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PR_LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PR_LOTID"] = sTrayID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_OUTLOT_LIST", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgTray, DetailResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgTray.SelectedIndex = idx;

                Search_Cell_List(DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "LOTID").ToString());
            }
        }

        private void Search_Cell_List(string sCellID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("OUT_LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["OUT_LOTID"] = sCellID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_SUBLOT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgCell, DetailResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

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

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //try
            //{
            //    CommonCombo _combo = new CommonCombo();

            //    String[] sFilter = { Util.NVC(cboArea.SelectedValue), null, Process.PACKAGING };
            //    C1ComboBox[] cboLineChild = { cboEquipment };
            //    _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
    }
}
