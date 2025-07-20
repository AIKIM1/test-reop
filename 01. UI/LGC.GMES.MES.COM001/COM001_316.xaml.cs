/*************************************************************************************
 Created Date : 2019.11.11
      Creator : 최상민
   Decription : 전지 5MEGA-GMES - 보류재고 정보 관리
--------------------------------------------------------------------------------------
 [Change History]
 -  2019-11-18 최상민 : GMES Hold, QMS Hold건 보류재고등록 정보 관리
                     C20191104-000168 + GMES 상 보류재고 관리 시스템 개발(9월 요청 건) +  2200
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_316 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo2 = new CMM001.Class.CommonCombo();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sEQSGID = string.Empty;

        string pAREAID = string.Empty;
        string pEQSGID = string.Empty;

        string sLotType = string.Empty;
        string sStckType = string.Empty;
        string sStckFlag = string.Empty;

        string sHoldGrId = string.Empty;
        string sHoldGrIdSeqNo = string.Empty;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        public COM001_316()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnModify);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

            string[] sFilter1 = { "HOLD_TRGT_CODE" };

            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType.SelectedIndex = 1;

            string[] sFilter2 = { "HOLD_STCK_TYPE" };

            _combo.SetCombo(cboHold_Stck_Type, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE_WITHOUT_CODE");
            //cboHold_Stck_Type.SelectedIndex = 1;
            /*
                        _combo.SetCombo(cboHold_Stck_Type2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE_WITHOUT_CODE");
                        cboHold_Stck_Type2.SelectedIndex = 1;
            */

            string[] sFilter3 = { "HOLD_STCK_FLAG" };

            _combo.SetCombo(cboHold_Stck_Flag, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");
            cboHold_Stck_Flag.SelectedIndex = 1;
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {

            txtTotalSQty.Text = "0";
            txtChoiceQty.Text = "0";
            /*
                        cboHold_Stck_Type2.SelectedValue = "SELECT";
                        txtOccr_Proc.Text = string.Empty;
                        txtOccrCause_Cntt.Text = string.Empty;
                        txtPrcs_Mthd_Cntt.Text = string.Empty;
                        txtProg_Stat_Cntt.Text = string.Empty;
                        txtUser.Text = string.Empty;
                        txtDept.Text = string.Empty;
            */
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            _combo2.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            cboCheck();
            Search();
        }
        #endregion

        private bool cboCheck()
        {
            bool bRet = false;

            string sTemp1 = Util.NVC(cboLotType.SelectedValue);
            string sTemp2 = Util.NVC(cboHold_Stck_Type.SelectedValue);
            string sTemp3 = Util.NVC(cboHold_Stck_Flag.SelectedValue);

            if (sTemp1 == "" || sTemp1 == "SELECT")
            {
                sLotType = "SELECT";
            }
            else
            {
                sLotType = sTemp1;
            }

            if (sTemp2 == "" || sTemp2 == "SELECT")
            {
                sStckType = "";
            }
            else
            {
                sStckType = sTemp2;
            }

            if (sTemp3 == "" || sTemp3 == "SELECT")
            {
                sStckFlag = "";
            }
            else
            {
                sStckFlag = sTemp3;
            }

            bRet = true;
            return bRet;
        }


        #region Method
        private void Search()
        {
            try
            {
                if (cboLotType.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("LOT타입"));
                    return;
                }

                if (cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1499", ObjectDic.Instance.GetObjectName("동"));
                    return;
                }

                txtTotalSQty.Text = "0";
                txtChoiceQty.Text = "0";

                sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("HOLD_STCK_TYPE_CODE");
                RQSTDT.Columns.Add("HOLD_STCK_FLAG");
                RQSTDT.Columns.Add("HOLD_GR_ID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("PRODID");


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? null : sAREAID;
                dr["EQSGID"] = string.IsNullOrEmpty(sEQSGID) ? null : sEQSGID;

                dr["HOLD_TRGT_CODE"] = string.IsNullOrEmpty(sLotType) ? null : sLotType;
                dr["HOLD_STCK_TYPE_CODE"] = string.IsNullOrEmpty(sStckType) ? null : sStckType;
                dr["HOLD_STCK_FLAG"] = string.IsNullOrEmpty(sStckFlag) ? null : sStckFlag;

                dr["HOLD_GR_ID"] = string.IsNullOrEmpty(txtHold_GR_ID.Text) ? null : txtHold_GR_ID.Text;
                dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text) ? null : txtCellID.Text;
                dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;

                if (sLotType.Equals("TRAY"))
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_TRAY_HOLD_STCK_HIST", "RQSTDT", "OUTDATA", RQSTDT);
                }
                else
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_STCK_HIST", "RQSTDT", "OUTDATA", RQSTDT);
                }

                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
                dgSearchResult.AllColumnsWidthAuto();
                //Util.GridAllColumnWidthAuto(ref dgSearchResult);
                pAREAID = sAREAID;
                pEQSGID = sEQSGID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion
        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        /*
                private void btnUser_Click(object sender, RoutedEventArgs e)
                {
                    try
                    {
                        GetUserWindow();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

                private void GetUserWindow()
                {
                    CMM_PERSON wndPerson = new CMM_PERSON();
                    wndPerson.FrameOperation = FrameOperation;

                    if (wndPerson != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = txtUser.Text;
                        C1WindowExtension.SetParameters(wndPerson, Parameters);

                        wndPerson.Closed += new EventHandler(wndUser_Closed);
                        //grdMain.Children.Add(wndPerson);
                        //wndPerson.BringToFront();

                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(wndPerson);
                                wndPerson.BringToFront();
                                break;
                            }
                        }
                    }
                }

                private void wndUser_Closed(object sender, EventArgs e)
                {
                    CMM_PERSON wndPerson = sender as CMM_PERSON;
                    if (wndPerson.DialogResult == MessageBoxResult.OK)
                    {

                        txtUser.Text = wndPerson.USERNAME;
                        txtUser.Tag = wndPerson.USERID;
                        txtDept.Text = wndPerson.DEPTNAME;
                        txtDept.Tag = wndPerson.DEPTID;

                    }

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Remove(wndPerson);
                            break;
                        }
                    }
                }
        */
        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOT_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);

                    }
                    else if (e.Cell.Column.Name == "PROD_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_STCK_FLAG")).Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgSearchResult_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                if (cell != null)
                {
                    if (cell.Row.Index < 0)
                        return;

                    if (cell.Column.Name == "LOT_CNT")
                    {
                        //setDataGridCellToolTip(sender, e);             

                        string sHoldGRid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_GR_ID"].Index).Value);

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["HOLD_GR_ID"] = sHoldGRid;
                        RQSTDT.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_GR_LIST", "RQSTDT", "OUTDATA", RQSTDT);

                        //txtTotalSQty.Text = Convert.ToString(dtResult.Rows.Count);
                        //Util.GridSetData(dgLotList, dtResult, FrameOperation, true);
                        if (dtResult.Rows.Count <= 0)
                        {
                            return;
                        }

                        string sToolTipText = "";
                        int lsCount = 0;

                        for (lsCount = 0; lsCount < dtResult.Rows.Count; lsCount++)
                        {
                            if (lsCount == 0)
                            {
                                if (Util.NVC(dtResult.Rows[lsCount]["HOLD_TRGT_CODE"]) == "SUBLOT")
                                {
                                    sToolTipText = Util.NVC(dtResult.Rows[lsCount]["STRT_SUBLOTID"]);
                                }
                                else
                                {
                                    sToolTipText = Util.NVC(dtResult.Rows[lsCount]["ASSY_LOTID"]);
                                }

                            }
                            else
                            {
                                if (Util.NVC(dtResult.Rows[lsCount]["HOLD_TRGT_CODE"]) == "SUBLOT")
                                {
                                    sToolTipText += "\n" + Util.NVC(dtResult.Rows[lsCount]["STRT_SUBLOTID"]);
                                }
                                else
                                {
                                    sToolTipText += "\n" + Util.NVC(dtResult.Rows[lsCount]["ASSY_LOTID"]);
                                }

                            }

                        }
                        /*
                            Size size = new Size(100, 100);
                            ToolTipService.SetPlacementRectangle(cell.Presenter , new Rect(pnt, size));
                            //ToolTipService.SetPlacement(cell.Presenter, System.Windows.Controls.Primitives.PlacementMode.Relative);
                            ToolTipService.SetInitialShowDelay(cell.Presenter, 1);
                            ToolTipService.SetToolTip(cell.Presenter, sToolTipText);
                        */
                        ToolTip toolTip = new ToolTip();
                        Size size = new Size(100, 100);
                        toolTip.PlacementRectangle = new Rect(pnt, size);
                        toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        toolTip.Content = sToolTipText;
                        toolTip.IsOpen = true;
                        //ToolTip = toolTip;

                        DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 3), IsEnabled = true };
                        timer.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
                        {
                            if (toolTip != null)
                            {
                                toolTip.IsOpen = false;
                            }
                            toolTip = null;
                            timer = null;
                        });
                    }
                }
                //ToolTipService.SetToolTip(e.Cell.Presenter, "테스트테스트테스트테스트테스트테스트테스트테스트,테스트테스트테스트테스트테스트테스트\n테스트테스트\n");                       
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                //datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
                return;
            }

            string sHoldGRid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_GR_ID"].Index).Value);
            /*
                        if (datagrid.CurrentColumn.Name == "LOT_CNT")
                        {
                            COM001_316_LOT_LIST popUp = new COM001_316_LOT_LIST();
                            popUp.FrameOperation = this.FrameOperation;

                            if (popUp != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = sHoldGRid;
                                Parameters[1] = datagrid.CurrentColumn.Name;

                                C1WindowExtension.SetParameters(popUp, Parameters);

                                popUp.Closed += new EventHandler(wndPackNote_Closed);
                                // 팝업 화면 숨겨지는 문제 수정.
                                //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                                grdMain.Children.Add(popUp);
                                popUp.BringToFront();
                            }
                        }
             */
            if (datagrid.CurrentColumn.Name == "PROD_QTY")
            {
                COM001_316_LOT_LIST popUp = new COM001_316_LOT_LIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sHoldGRid;
                    Parameters[1] = datagrid.CurrentColumn.Name;

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackNote_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            COM001.COM001_316_LOT_LIST wndPopup = sender as COM001.COM001_316_LOT_LIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["HOLD_STCK_FLAG"]) == "N")
                    {
                        DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", true);
                    }

                }
                */
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                if (rowIndex != -1)
                {
                    if (dgSearchResult.GetCell(rowIndex, dgSearchResult.Columns["CHK"].Index).Presenter != null
                        && (dgSearchResult.GetCell(rowIndex, dgSearchResult.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue
                        && Util.NVC(dgSearchResult.GetCell(rowIndex, dgSearchResult.Columns["HOLD_STCK_FLAG"].Index).Value) == "N")
                    {
                        (dgSearchResult.GetCell(rowIndex, dgSearchResult.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 보류재고정보 수정
        /// </summary>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            HoldInformSave("SAVE");
            /* radio 버튼 기능시 로직 -> check button으로 변경하면서 주석처리함
            if (!ValidationModify())
                return;

            // 수정 하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyProcess();
                }
            });
            */
        }

        private void HoldInformSave(string TrgtCode)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                COM001_316_UPDATE puSave = new COM001_316_UPDATE();
                puSave.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = drList.CopyToDataTable();
                Parameters[1] = pAREAID;
                Parameters[2] = pEQSGID;

                C1WindowExtension.SetParameters(puSave, Parameters);

                puSave.Closed += new EventHandler(puSave_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => puSave.ShowModal()));

                grdMain.Children.Add(puSave);
                puSave.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puSave_Closed(object sender, EventArgs e)
        {
            COM001_316_UPDATE window = sender as COM001_316_UPDATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }

        private bool ValidationModify()
        {
            try
            {
                List<int> list = _util.GetDataGridCheckRowIndex(dgSearchResult, "CHK");
                if (list.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }
                /*
                                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgSearchResult, "CHK", true);

                                if (rowIndex <= 0)
                                {
                                    // 선택된 항목이 없습니다.
                                    Util.MessageValidation("SFU1651");
                                    return false;
                                }
                */
                /*
                if (cboHold_Stck_Type2.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("HOLD재고구분"));
                    return false;
                }            

                if (string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    // 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtDept.Text))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("담당부서"));
                    return false;
                }           
                */
                return true;
            }
            catch 
            {
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ModifyProcess()
        {
            try
            {
                /*
                DataRow dr = _util.GetDataGridFirstRowBycheck(dgSearchResult, "CHK");

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("HOLD_GR_ID", typeof(string));
                inTable.Columns.Add("HOLD_GR_ID_SEQNO", typeof(string));
                inTable.Columns.Add("HOLD_STCK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("OCCR_PROCID", typeof(string));
                inTable.Columns.Add("OCCR_CAUSE_CNTT", typeof(string));
                inTable.Columns.Add("PRCS_MTHD_CNTT", typeof(string));
                inTable.Columns.Add("PROG_STAT_CNTT", typeof(string));
                inTable.Columns.Add("CHARGE_DEPT_NAME", typeof(string));
                inTable.Columns.Add("CHARGE_USERID", typeof(string));                
                inTable.Columns.Add("UPDUSER", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["HOLD_GR_ID"] = sHoldGrId;
                newRow["HOLD_GR_ID_SEQNO"] = sHoldGrIdSeqNo;
                newRow["HOLD_STCK_TYPE_CODE"] = cboHold_Stck_Type2.SelectedValue.ToString();
                newRow["OCCR_PROCID"] = string.IsNullOrEmpty(txtOccr_Proc.Text.Trim()) ? null : txtOccr_Proc.Text.Trim();
                newRow["OCCR_CAUSE_CNTT"] = string.IsNullOrEmpty(txtOccrCause_Cntt.Text.Trim()) ? null : txtOccrCause_Cntt.Text.Trim(); 
                newRow["PRCS_MTHD_CNTT"] = string.IsNullOrEmpty(txtPrcs_Mthd_Cntt.Text.Trim()) ? null : txtPrcs_Mthd_Cntt.Text.Trim(); 
                newRow["PROG_STAT_CNTT"] = string.IsNullOrEmpty(txtProg_Stat_Cntt.Text.Trim()) ? null : txtProg_Stat_Cntt.Text.Trim();
                newRow["CHARGE_DEPT_NAME"] = string.IsNullOrEmpty(txtDept.Text.Trim()) ? null : txtDept.Text.Trim();
                newRow["CHARGE_USERID"] = string.IsNullOrEmpty(txtUser.Text.Trim()) ? null : txtUser.Text.Trim();
                newRow["UPDUSER"] = LoginInfo.USERID;                
                inTable.Rows.Add(newRow);                

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_UPD_TB_SFC_ASSY_LOT_HOLD_STCK_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        cboCheck();
                        Search();
                        InitControl();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
                */
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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


    }
}
