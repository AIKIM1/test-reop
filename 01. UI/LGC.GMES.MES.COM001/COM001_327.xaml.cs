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
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_327 : UserControl, IWorkArea
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
        public COM001_327()
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

            if (bNcrHoldUser())
            {
                btnHoldStockRelease_Ncr.Visibility = Visibility.Visible;
            }
            else
            {
                btnHoldStockRelease_Ncr.Visibility = Visibility.Collapsed;
            }
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
            _combo.SetCombo(cboArea_Ncr, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

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

            _combo.SetCombo(cboLotType_Ncr, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType_Ncr.SelectedIndex = 1;
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {

            txtTotalSQty.Text = "0";
            txtChoiceQty.Text = "0";
            txtTotalSQty2.Text = "0";
            txtChoiceQty2.Text = "0";
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

        private bool bNcrHoldUser()
        {
            bool user_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "NCR_HOLD_RELEASE_AUTHORITY";
                dr["ATTRIBUTE2"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && LoginInfo.USERID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    user_chk = true;
                }
                else
                {
                    user_chk = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return user_chk;
        }

        private void cboArea_Ncr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea_Ncr.SelectedValue);
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
            _combo2.SetCombo(cboEquipmentSegment_Ncr, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

        }

        private void dtpDateFrom_Ncr_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Ncr.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Ncr.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Ncr_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Ncr.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Ncr.SelectedDateTime;
                return;
            }
        }

        private void btnSearch_Ncr_Click(object sender, RoutedEventArgs e)
        {
            Search_Ncr();
        }

        private void btnHoldRelease_Ncr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);

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

                // SFU8131 보류재고를 해제하시겠습니까?
                Util.MessageConfirm("SFU8131", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });

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

        private void Save()
        {
            try
            {

                // DataTable dr = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);

                // DATA SET 
                DataSet inDataSet = new DataSet();
                //DataTable inTable = inDataSet.Tables.Add("INDATA");
                DataTable inTable = new DataTable("INDATA");

                inTable.Columns.Add("HOLD_GR_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRowView rowview = null;
                /////////////////////////////////////////////////////////////////                
                foreach (C1.WPF.DataGrid.DataGridRow row in dgSearchResult_Ncr.Rows)
                {
                    rowview = row.DataItem as DataRowView;

                    //if (!String.IsNullOrEmpty(rowview["HOLD_GR_ID"].ToString()))
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        DataRow dr = inTable.NewRow();
                        dr["HOLD_GR_ID"] = DataTableConverter.GetValue(row.DataItem, "HOLD_GR_ID"); ;
                        dr["UPDUSER"] = LoginInfo.USERID;

                        inTable.Rows.Add(dr);

                        //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ASSY_NCR_HOLD", "INDATA", "OUTDATA", RQSTDT);
                        new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_SFC_ASSY_LOT_HOLD_STCK_RELEASE", "INDATA", null, inTable);
                        inTable.Rows.Clear();
                    }
                }
                Util.AlertInfo("SFU1270");  //저장되었습니다.
                //dgHold.ItemsSource = null;
                Search_Ncr();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Search_Ncr()
        {
            try
            {
                if (cboLotType_Ncr.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("LOT타입"));
                    return;
                }

                txtTotalSQty.Text = "0";
                txtChoiceQty.Text = "0";

                sEQSGID = Util.NVC(cboEquipmentSegment_Ncr.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE"); //2019.04.19 이제섭 HOLD_TYPE_CODE 추가
                RQSTDT.Columns.Add("SEARCH_GUBUN"); // 탭 조회 구분 ->  춣하HOLD등록 탭 : 'G'  NCR HOLD등록 탭 : 'Q'
                RQSTDT.Columns.Add("HOLD_GR_ID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["HOLD_TYPE_CODE"] = "QA_HOLD"; //2019.04.19 이제섭 HOLD_TYPE_CODE 추가   2020.04.13 조영빈 소형 전용 조회를 위한 추가             
                dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID_Ncr.Text) ? null : txtLotID_Ncr.Text;
                dr["FROM_HOLD_DTTM"] = dtpDateFrom_Ncr.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_HOLD_DTTM"] = dtpDateTo_Ncr.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? null : sAREAID;
                dr["EQSGID"] = string.IsNullOrEmpty(sEQSGID) ? null : sEQSGID;
                dr["HOLD_TRGT_CODE"] = (string)cboLotType_Ncr.SelectedValue;
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID_Ncr.Text) ? null : txtCellID_Ncr.Text;
                dr["PRODID"] = string.IsNullOrEmpty(txtProdID_Ncr.Text) ? null : txtProdID_Ncr.Text;
                dr["SEARCH_GUBUN"] = "S";
                dr["HOLD_GR_ID"] = string.IsNullOrEmpty(txtHold_GR_ID_Ncr.Text) ? null : txtHold_GR_ID_Ncr.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                txtTotalSQty.Text = Convert.ToString(dtResult.Rows.Count);

                Util.GridSetData(dgSearchResult_Ncr, dtResult, FrameOperation, true);
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
        void checkAll_Ncr_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Ncr_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        private void dgSearchResult_Ncr_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Ncr_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Ncr_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Ncr_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Ncr_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgSearchResult_Ncr_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult_Ncr.GetCellFromPoint(pnt);
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

        private void dgSearchResult_Ncr_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_STCK_FLAG")).Equals("Y"))
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
        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            COM001.COM001_327_LOT_LIST wndPopup = sender as COM001.COM001_327_LOT_LIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResult_Ncr_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
                */
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                {
                    if (item["HOLD_STCK_FLAG"].Equals("Y"))
                    {
                        // item["CHK"] = false;
                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";

                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1")
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                        && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")
                        )
                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }


                txtChoiceQty.Text = Convert.ToString(sChoiceQty);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Ncr_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            //  item["CHK"] = false;
                        }

                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";

                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1")
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                         && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")
                        )

                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }

                txtChoiceQty.Text = Convert.ToString(sChoiceQty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Ncr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
                */
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                {
                    if (item["HOLD_STCK_FLAG"].Equals("Y"))
                    {
                        // item["CHK"] = false;
                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1")
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                         && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")
                        )
                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }
                txtChoiceQty.Text = Convert.ToString(sChoiceQty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotHold_Ncr_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            registeHold_Ncr("NCR");
        }

        private void btnLotHold_Excel_Click(object sender, RoutedEventArgs e)
        {
            registeHold_Ncr("EXCEL");
        }

        private bool CanChangeCell()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgSearchResult_Ncr, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void registeHold_Ncr(string PGubun)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);
                COM001_327_NCR_HOLD puNcrHold = new COM001_327_NCR_HOLD();
                if (PGubun == "NCR")
                {
                    List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                    if (drList.Count <= 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }



                    puNcrHold.FrameOperation = FrameOperation;

                    object[] Parameters = new object[2];
                    Parameters[0] = drList.CopyToDataTable();
                    Parameters[1] = PGubun;
                    C1WindowExtension.SetParameters(puNcrHold, Parameters);
                }
                else
                {

                    puNcrHold.FrameOperation = FrameOperation;

                    object[] Parameters = new object[2];
                    Parameters[0] = "";
                    Parameters[1] = PGubun;
                    C1WindowExtension.SetParameters(puNcrHold, Parameters);
                }
                //Parameters[1] = PGubun;                

                //puNcrHold.Closed += new EventHandler(puUnHold_Closed);
                puNcrHold.Closed += new EventHandler(puNcrHold_Closed);

                grdMain.Children.Add(puNcrHold);
                puNcrHold.BringToFront();
                Search_Ncr();

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

        private void puNcrHold_Closed(object sender, EventArgs e)
        {
            COM001_327_NCR_HOLD window = sender as COM001_327_NCR_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search_Ncr();
            }
            this.grdMain.Children.Remove(window);
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
                txtTotalSQty2.Text = "0";
                txtChoiceQty2.Text = "0";

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
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_STCK_HIST_S", "RQSTDT", "OUTDATA", RQSTDT);
                }

                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
                Util.GridAllColumnWidthAuto(ref dgSearchResult);

                if (dtResult.Rows.Count >= 1)
                {
                    pAREAID = string.IsNullOrEmpty(dtResult.Rows[0]["AREAID"].ToString()) ? null : dtResult.Rows[0]["AREAID"].ToString();
                    pEQSGID = string.IsNullOrEmpty(dtResult.Rows[0]["EQSGID"].ToString()) ? null : dtResult.Rows[0]["EQSGID"].ToString();
                }else
                {
                    pAREAID = sAREAID;
                    pEQSGID = sEQSGID;
                }
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
                            COM001_327_LOT_LIST popUp = new COM001_327_LOT_LIST();
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
                COM001_327_LOT_LIST popUp = new COM001_327_LOT_LIST();
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

                COM001_327_UPDATE puSave = new COM001_327_UPDATE();
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
            COM001_327_UPDATE window = sender as COM001_327_UPDATE;
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
            catch (Exception ex)
            {
                return false;
                Util.MessageException(ex);
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
