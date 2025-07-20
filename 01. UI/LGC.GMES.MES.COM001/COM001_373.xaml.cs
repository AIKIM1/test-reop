/*************************************************************************************
 Created Date : 2022.10.24
      Creator : 김용준
   Decription : 전지 5MEGA-GMES - 보류재고 정보 관리
--------------------------------------------------------------------------------------
2023.07.20  김동훈    : TOP_PRODID 제품ID 추가
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
    public partial class COM001_373 : UserControl, IWorkArea
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
        public COM001_373()
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

            string[] sFilter1 = { "HOLD_TRGT_CODE_FORM" };

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
                RQSTDT.Columns.Add("TOP_PRODID");


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
                //dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;

                if (rdProdid.IsChecked == true)
                {
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                }
                else
                {
                    dr["TOP_PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;


                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_STCK_HIST_F", "RQSTDT", "OUTDATA", RQSTDT);

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
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
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
                        ToolTip toolTip = new ToolTip();
                        Size size = new Size(100, 100);
                        toolTip.PlacementRectangle = new Rect(pnt, size);
                        toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        toolTip.Content = sToolTipText;
                        toolTip.IsOpen = true;

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
                return;
            }

            string sHoldGRid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_GR_ID"].Index).Value);
            
            if (datagrid.CurrentColumn.Name == "PROD_QTY")
            {
                COM001_373_LOT_LIST popUp = new COM001_373_LOT_LIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sHoldGRid;
                    Parameters[1] = datagrid.CurrentColumn.Name;

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackNote_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            COM001.COM001_373_LOT_LIST wndPopup = sender as COM001.COM001_373_LOT_LIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
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

                COM001_373_UPDATE puSave = new COM001_373_UPDATE();
                puSave.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = drList.CopyToDataTable();
                Parameters[1] = pAREAID;
                Parameters[2] = pEQSGID;

                C1WindowExtension.SetParameters(puSave, Parameters);

                puSave.Closed += new EventHandler(puSave_Closed);

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
            COM001_373_UPDATE window = sender as COM001_373_UPDATE;
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
