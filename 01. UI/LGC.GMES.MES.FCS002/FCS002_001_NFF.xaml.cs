/*************************************************************************************
 Created Date : 2023.01.04
      Creator : Dooly
   Decription : 충방전기 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.04  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.IO;
using System.Windows.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_001_NFF : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LANEID = string.Empty;
        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private string _MENUID = string.Empty;
        private bool _GridSize = false; // Grid 크기 변경 여부
        private double rowHeight = 0;
        private Point prevRowPos = new Point(0, 0);
        private Point prevColPos = new Point(0, 0);

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        Util Util = new Util();

        public FCS002_001_NFF()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// Frame과 상호작용하기 위한 객체
        ///// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string sLaneID = string.Empty;

                if (string.IsNullOrEmpty(_MENUID))
                {
                    _MENUID = LoginInfo.CFG_MENUID;
                }

                sLaneID = GetLaneIDForMenu(_MENUID);

                if (string.IsNullOrEmpty(sLaneID))
                {
                    _LANEID = "X11";
                }
                else
                {
                    _LANEID = sLaneID;
                }
                              
                InitCombo();
                InitLegend();
                InitProc();
                InitWRKLoc();

                rdoTrayId.IsChecked = true;
                rdoTrayInfo.IsChecked = true;

                btnSearch_Click(null, null);

                _timer = new DispatcherTimer();

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (chkTimer.IsChecked == true)
            {
                chkTimer.IsChecked = false;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 10)
            {
                btnSearch_Click(null, null);
                sec = 0;
            }
        }

        #endregion

        #region Initialize
        //화면내 combo 셋팅
        private void InitCombo()
        {
            try
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                //string[] filter = { "1", _LANEID };
                //_combo.SetCombo(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", sFilter: filter);

                //cboRow.IsEnabled = false;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            ClearALL();
            GetNonOperBoxSummary(); //부동율
            GetFormationStatus();
            //GetTrayTypeCnt();       //가동율
            GetFormationRatioRoute();
            boxStatusView();

            if (chkTimer.IsChecked == true)
            {
                _timer.Start();
            }
        }

        private void dgColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE2").ToString()) as SolidColorBrush;
                    }
                }
            }));
        }

        private void dgTemp_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{

            //    if (dgTemp.GetCell(0, 0) != null)
            //        dgTemp.GetCell(0, 0).Presenter. = new SolidColorBrush(Colors.LightGray);
            //}));
        }
        private void cboRow_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //btnSearch_Click(null, null);
        }

        private void dgFormation_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    //ROW Header 설정
                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "1")).Equals(string.Empty) || e.Cell.Column.Index == 0)
                    if (e.Cell.Row.Index == 0 || e.Cell.Row.Index == _dtCopy.Rows.Count / 2 || e.Cell.Column.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = Brushes.Black;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.FontSize = 14;

                        string ROWNUM = _dtCopy.Rows[e.Cell.Row.Index][(e.Cell.Column.Index + 1).ToString()].ToString();

                        if (!string.IsNullOrEmpty(ROWNUM))
                        {
                            string BCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "BCOLOR"));
                            string FCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "FCOLOR"));
                            string TEXT = Util.NVC(GetDtRowValue(ROWNUM, "TEXT"));
                            string BOLD = Util.NVC(GetDtRowValue(ROWNUM, "BOLD"));

                            if (!string.IsNullOrEmpty(BCOLOR))
                                e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR) as SolidColorBrush;

                            if (!string.IsNullOrEmpty(FCOLOR))
                                e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;

                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), TEXT);

                            if (BOLD.Equals("Y"))
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            else if (BOLD.Equals("N"))
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            //설비없음
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), ObjectDic.Instance.GetObjectName("NO_EQP"));
                        }

                        e.Cell.Presenter.Padding = new Thickness(0);
                        e.Cell.Presenter.Margin = new Thickness(0);
                        e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                        e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

      
        private void dgFormation_MouseUp(object sender, MouseButtonEventArgs e)
        {

            try
            {

                dgTemp.ItemsSource = null;
                dgTemp.Refresh();



                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgFormation.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    // dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter.Background = Brushes.LightGray;
                    //  dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter.Background = Brushes.LightGray;

                    if (!(cell.Row.Index == 0) && !(cell.Row.Index == _dtCopy.Rows.Count / 2))
                    {
                        if (dgFormation.GetCell(cell.Row.Index, 0).Value == null)
                            return;

                        if (!string.IsNullOrEmpty(dgFormation.GetCell(cell.Row.Index, 0).Value.ToString()))
                        {
                            if (dgFormation.GetCell(cell.Row.Index, 0).Value.Equals(dgFormation.GetCell(cell.Row.Index - 1, 0).Value))
                            {
                                prevRowPos = new Point(cell.Row.Index - 1, 0);
                            }
                            else
                            {
                                prevRowPos = new Point(cell.Row.Index, 0);
                            }
                            //dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter.Background = Brushes.White;

                            for (int row = cell.Row.Index; row >= 0; row--)
                            {
                                //if (dgFormation.GetValue(row, "1").Equals(string.Empty))
                                if (row == 0 || row == _dtCopy.Rows.Count / 2)
                                {
                                    prevColPos = new Point(row, cell.Column.Index);
                                    break;
                                }
                            }
                            //dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter.Background = Brushes.White;
                        }
                    }

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    if (string.IsNullOrEmpty(ROWNUM)) return;

                    int i = 0;
                    if (!int.TryParse(ROWNUM, out i)) return;

                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));

                    if (!string.IsNullOrEmpty(EQPTID))
                    {
                        if (chkTemp.IsChecked == true)
                        {
                            GetTempData(EQPTID);
                        }
                        ClearControl();
                        txtSelStg.Text = int.Parse(STG).ToString();
                        txtSelCol.Text = int.Parse(COL).ToString();

                        DataSet ds = new DataSet();
                        DataTable dtRqst = ds.Tables.Add("INDATA");
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["CSTID"] = CSTID;
                        dr["EQPTID"] = EQPTID;
                        dtRqst.Rows.Add(dr);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_TRAY_EQP_MAINT_MB", "INDATA", "TRAY,EQPMAINT,MAINT", ds);

                        // Box 상태정보
                        if (dsRslt.Tables["TRAY"].Rows.Count > 0 && !FORMSTATUS.Equals("16")) //Trouble이 아닐경우
                        {
                            if (string.IsNullOrEmpty(CSTID))
                            {
                                txtStatus.Text = string.Empty;
                            }
                            else
                            {
                                txtStatus.Text = dsRslt.Tables["TRAY"].Rows[0]["CSTID"].ToString() + "\r\n"
                                        + dsRslt.Tables["TRAY"].Rows[0]["PROCID"].ToString() + "\r\n"
                                        + dsRslt.Tables["TRAY"].Rows[0]["WIPDTTM_ST"].ToString();
                            }

                        }
                        else if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0 && FORMSTATUS.Equals("16")) //Trouble
                        {
                            txtStatus.Text = ObjectDic.Instance.GetObjectName("TROUBLE");
                        }
                        else
                        {
                            txtStatus.Text = string.Empty;
                        }

                        // Box Trouble 정보
                        if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0 && (FORMSTATUS.Equals("16") || FORMSTATUS.Equals("17"))) //Trouble 또는 일시정지
                        {
                            txtTroubleName.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_NAME"].ToString());
                            txtTroubleRepairWay.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString());
                            txtCH_INFO.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["CH_INFO"].ToString());
                        }
                        else if (dsRslt.Tables["MAINT"].Rows.Count > 0)
                        {
                            txtTroubleRepairWay.Text = Util.NVC(dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString());
                            try
                            {
                                /* 2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가 
                                 * 부동전환시 박스 표시 변경, 조건 변경
                                 * */
                                //if (fpsFormation.ActiveSheet.Cells[piRow, piCol].Text.Trim().Substring(0, MDF_NAME.Length) == MDF_NAME)
                                if (dsRslt.Tables["MAINT"].Rows[0]["CF_YN"].ToString().Equals("C"))
                                {
                                    txtRemark.Text = Util.NVC(dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_NAME"].ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0)
                            {
                                txtTroubleName.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_NAME"].ToString());
                                txtTroubleRepairWay.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString());
                                txtCH_INFO.Text = Util.NVC(dsRslt.Tables["EQPMAINT"].Rows[0]["CH_INFO"].ToString());
                            }
                            else
                            {
                                txtTroubleName.Text = string.Empty;
                                txtTroubleRepairWay.Text = string.Empty;
                                txtCH_INFO.Text = string.Empty;
                            }

                        }
                    }


                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

    
        private void dgFormation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    //20220420_에러수정(입력 문자열의 형식이 잘못되었습니다.) START
                    if (string.IsNullOrEmpty(Util.NVC(ROWNUM)))
                    {
                        return;
                    }
                    if (cell.Row.Index == 0 || cell.Row.Index == _dtCopy.Rows.Count / 2 || cell.Column.Index == 0)
                    {
                        return;
                    }
                    //20220420_에러수정(입력 문자열의 형식이 잘못되었습니다.) END
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string ROW = Util.NVC(GetDtRowValue(ROWNUM, "ROW"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string LOTID = Util.NVC(GetDtRowValue(ROWNUM, "LOTID"));
                    string PRE_LOTID = Util.NVC(GetDtRowValue(ROWNUM, "PRE_LOTID"));

                    if (rdoTrayInfo.IsChecked == true)
                    {
                        if (string.IsNullOrEmpty(CSTID) && string.IsNullOrEmpty(PRE_LOTID))
                        {
                            return;
                        }

                        FCS002_021 wndRunStart = new FCS002_021();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = CSTID;
                            Parameters[1] = !LOTID.Equals(string.Empty) ? LOTID : PRE_LOTID;

                            this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                        }
                    }
                    else if (rdoEqpControl.IsChecked == true)
                    {
                        FCS002_001_DETAIL wndRunStart = new FCS002_001_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = _LANEID;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }
                    }
                    else if (rdoTempInfo.IsChecked == true)
                    {
                        //FCS002_049
                        FCS002_049 wndRunStart = new FCS002_049();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = _LANEID;
                            Parameters[1] = EQPTID;

                            this.FrameOperation.OpenMenu("SFU010715300", true, Parameters);
                        }
                    }
                    else if (rdoTrayHistory.IsChecked == true)
                    {
                        //FCS002_001_TRAY_HIST
                        FCS002_001_TRAY_HISTORY wndRunStart = new FCS002_001_TRAY_HISTORY();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = "1";
                            Parameters[5] = String.Empty;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }

                    }
                    else if (rdoUseTime.IsChecked == true)
                    {
                        //FCS002_072 - Box 유지보수
                        FCS002_072 wndRunStart = new FCS002_072();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = _LANEID;

                            //this.FrameOperation.OpenMenu("SFU010730580", true, Parameters);
                            this.FrameOperation.OpenMenuFORM("FCS002_072", "FCS002_072", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Box 유지보수"), true, Parameters);
                        }
                    }
                    else if (rdoBoxModel.IsChecked == true)
                    {
                        FCS002_001_BOX_MODEL wndRunStart = new FCS002_001_BOX_MODEL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }
                    }

                    else if (rdoPinCnt.IsChecked == true)
                    {
                        FCS002_223 wndRunStart = new FCS002_223();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[4];
                            Parameters[0] = "FOR";
                            Parameters[1] = _LANEID;
                            Parameters[2] = EQPTID;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkTimer_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        private void chkTimer_Unchecked(object sender, RoutedEventArgs e)
        {
            _timer.Tick -= new EventHandler(timer_Tick);
            _timer.Stop();
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbClick = (RadioButton)sender;
            btnSearch_Click(null, null);
        }
        #endregion

        #region Method

        private string GetLaneIDForMenu(string sMenuID)
        {
            string sLaneID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_CHARGE_MENU_ID";
                dr["CMCODE"] = _MENUID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sLaneID;
        }

        private void InitLegend()
        {
            try
            {
                C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("- LEGEND -") };
               // cboColorLegend.Items.Add(cbItemTiTle);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_FORMSTATUS_MCC";
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                _dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in _dtColor.Rows)
                {
                    if (row["ATTRIBUTE1"].ToString().IsNullOrEmpty())
                    {
                        continue;
                    }

                    C1ComboBoxItem cbItem = new C1ComboBoxItem
                    {
                        Content = row["CBO_NAME"].ToString(),
                        Background = new BrushConverter().ConvertFromString(row["ATTRIBUTE1"].ToString()) as SolidColorBrush,
                        Foreground = new BrushConverter().ConvertFromString(row["ATTRIBUTE2"].ToString()) as SolidColorBrush
                    };
                    //cboColorLegend.Items.Add(cbItem);
                }


                // cboColorLegend.SelectedIndex = 0;

                //-----------------------------------------------------
                //  CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                //  string[] filter = { "1", _LANEID };
                //  _combo.SetCombo(cboOperLegend, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "FORMATION_STATUS_NEXT_PROC","11,12,13,14,B1,31,41,51" });

                //------------------------------------------------------



                // 색 범례 
                SetColorGrid(_dtColor);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void InitProc()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORMATION_STATUS_NEXT_PROC";
                dr["CMCODE"] = "11,12,13,14,B1,31,41,51";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

                //ShowLoadingIndicator();

                SetProcrGrid(dtResult);
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetColorGrid(DataTable dt)
        {

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Color";
                textColumn1.Binding = new Binding("CBO_NAME");
                textColumn1.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn1.IsReadOnly = true;
                textColumn1.Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel);

                C1.WPF.DataGrid.DataGridTextColumn textColumn2 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn2.Header = "Color";
                textColumn2.Binding = new Binding("ATTRIBUTE1");
                textColumn2.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn2.IsReadOnly = true;
                textColumn2.Visibility = Visibility.Collapsed;

                C1.WPF.DataGrid.DataGridTextColumn textColumn3 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn3.Header = "Color";
                textColumn3.Binding = new Binding("ATTRIBUTE2");
                textColumn3.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn3.IsReadOnly = true;
                textColumn3.Visibility = Visibility.Collapsed;

                dgNew.Columns.Add(textColumn1);
                dgNew.Columns.Add(textColumn2);
                dgNew.Columns.Add(textColumn3);

                // dgNew.IsEnabled = false;
                // dgNew.IsReadOnly = true;
                dgNew.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
                dgNew.FrozenColumnCount = 0;
                dgNew.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                dgNew.LoadedCellPresenter += dgColor_LoadedCellPresenter;
                dgNew.SelectedBackground = null;


                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                dgColor.Children.Add(dgNew); 

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("CBO_NAME", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow drRow = dtRow.NewRow();
                drRow["CBO_NAME"] = dt.Rows[i]["CBO_NAME"];
                drRow["ATTRIBUTE1"] = dt.Rows[i]["ATTRIBUTE1"];
                drRow["ATTRIBUTE2"] = dt.Rows[i]["ATTRIBUTE2"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, false);
            }
        }
        private void SetProcrGrid(DataTable dt)
        {

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Proc";
                textColumn1.Binding = new Binding("CBO_NAME");
                textColumn1.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn1.IsReadOnly = true;
                textColumn1.Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel);

                dgNew.Columns.Add(textColumn1);

                dgNew.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
                dgNew.FrozenColumnCount = 0;
                dgNew.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                dgNew.SelectedBackground = null;


                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                dgProc.Children.Add(dgNew);

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("CBO_NAME", typeof(string));
            
                DataRow drRow = dtRow.NewRow();
                drRow["CBO_NAME"] = dt.Rows[i]["CBO_NAME"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, true);
            }
        }

        // 통로위치 
        private void InitWRKLoc()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_WORK_LOC_MB";
                dr["CMCODE"] = _LANEID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_F_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    switch (dtResult.Rows[0]["ATTRIBUTE1"].ToString())
                    {
                        case "TOP":
                            LocTop.Visibility = Visibility.Visible;
                            LocBottom.Visibility = Visibility.Collapsed;
                            break;
                        case "BOTTOM":
                            LocTop.Visibility = Visibility.Collapsed;
                            LocBottom.Visibility = Visibility.Visible;
                            break;
                        default:
                            LocTop.Visibility = Visibility.Collapsed;
                            LocBottom.Visibility = Visibility.Collapsed;
                            break;

                    }
                }
                
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetNonOperBoxSummary()
        {
            try
            {
                dgLoss.ItemsSource = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_NONBOX_RATE_BY_LANE_MB", "RQSTDT", "RSLTDT", dtRqst);
                dgLoss.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetFormationStatus()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = _LANEID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                _dtDATA = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_VIEW_BY_LANE_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (_dtDATA.Rows.Count == 0)
                {
                    //조회된 값이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0232"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }

                dgFormation.ItemsSource = null;

                SetFpsFormationData(_dtDATA);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //private void GetTrayTypeCnt()
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("LANE_ID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANE_ID"] = _LANEID;
        //        dtRqst.Rows.Add(dr);

        //        ShowLoadingIndicator();
        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHARGE_TRAY_TYPE_CNT_MB", "RQSTDT", "RSLTDT", dtRqst);
        //        Util.GridSetData(dgTrayShare, dtRslt, FrameOperation, true);

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //    finally
        //    {
        //        HiddenLoadingIndicator();
        //    }
        //}

        private void SetFpsFormationData(DataTable dtRslt)
        {
            try
            {
                // 스프레드 초기화용 변수
                #region 스프레드 초기화용 변수
                int iMaxCol;
                int iMaxStg;
                int iRowCount;
                int iColumnCount;
                double dColumnWidth;
                double dRowHeight;
                int tempCol = 0;
                int tempStg = 0;
                #endregion

                // 충방전기 데이터용 변수
                #region 충방전기 데이터용 변수
                string sStatus = string.Empty;
                TimeSpan tTimeSpan = new TimeSpan();
                bool bBold = false;

                string EQPTID = string.Empty;
                int iROW;
                int iCOL;
                int iSTG;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string EIOSTAT = string.Empty;
                string EQP_OP_STATUS_CD = string.Empty;
                string RUN_MODE_CD = string.Empty;
                string PROCID = string.Empty;
                string PROCNAME = string.Empty;
                string NEXT_PROCID = string.Empty;
                string RCV_ISS_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string EQPTIUSE = string.Empty;
                string NEXT_PROCNAME = string.Empty;
                string PROD_LOTID = string.Empty;
                string JOB_TIME = string.Empty;
                string ROUTID = string.Empty;
                string DUMMY_FLAG = string.Empty;
                string SPECIAL_YN = string.Empty;
                string LAST_RUN_TIME = string.Empty;
                string JIG_AVG_TEMP = string.Empty;
                string POW_AVG_TEMP = string.Empty;
                string MEGA_DCHG_FUNC_YN = string.Empty;
                string MEGA_CHG_FUNC_YN = string.Empty;
                string NOW_TIME = string.Empty;
                string PROC_DETL_TYPE_CODE = string.Empty;
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                string START_TIME = string.Empty;
                string MIN_TEMP = string.Empty;
                string MAX_TEMP = string.Empty;
                string TRANS_CSTID = string.Empty;
                string WIPSTAT = string.Empty;
                #endregion

                //충방전기 열 갯수 확인
                DataView view = dtRslt.DefaultView;
                DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
                List<string> rowList = new List<string>();
                foreach (DataRow dr in distRowTable.Rows)
                {
                    rowList.Add(dr["ROW"].ToString());
                }

                // 충방전기 Row 갯수에 따라 로직을 나눔
                if (rowList.Count > 1)
                {
                    #region Grid 초기화
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", "ROW = " + rowList[row]));
                        iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", "ROW = " + rowList[row]));

                        if (iMaxCol > tempCol) tempCol = iMaxCol;
                        if (iMaxStg > tempStg) tempStg = iMaxStg;
                    }

                    //iRowCount = tempStg * 2;
                    //iColumnCount = tempCol + 1;
                    iRowCount = tempStg;
                    iColumnCount = (tempCol + 1) / 2 + 1;   //단 추가.

                    //Grid Column Width get
                    dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

                    //Grid Row Height get
                    //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
                    dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2)  - 2.3;

                    //충방전기 Row별 Datatable 정의
                    List<DataTable> dtList = new List<DataTable>();

                    //Grid Column 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataTable dt = new DataTable();

                        for (int i = 0; i < iColumnCount; i++)
                        {
                            if (row == 0)
                            {
                                //GRID Column Create
                                if (i == 0)
                                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50);
                                else
                                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth);
                            }

                            dt.Columns.Add((i + 1).ToString(), typeof(string));
                        }

                        dtList.Add(dt);
                    }

                    //Grid Row 생성
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        for (int i = 0; i < iRowCount; i++)
                        {
                            DataRow drRow = dtList[row].NewRow();
                            dtList[row].Rows.Add(drRow);
                        }

                    }

                    //Grid Row Header 생성 (갯수에 따라 만들어야 하는데...)
                    List<DataRow> drListHeader = new List<DataRow>();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        DataRow drRowHeader = dtList[row].NewRow();
                        for (int i = 0; i < dtList[row].Columns.Count; i++)
                        {
                            if (i == 0)
                                drRowHeader[i] = int.Parse(rowList[row]).ToString() + ObjectDic.Instance.GetObjectName("열");
                            else
                                drRowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
                        }
                        drListHeader.Add(drRowHeader);
                    }
                    #endregion

                    #region Data Setting
                    //dtRslt Column Add
                    dtRslt.Columns.Add("BCOLOR", typeof(string));
                    dtRslt.Columns.Add("FCOLOR", typeof(string));
                    dtRslt.Columns.Add("TEXT", typeof(string));
                    dtRslt.Columns.Add("BOLD", typeof(string));

                    for (int k = 0; k < dtRslt.Rows.Count; k++)
                    {
                        iROW = int.Parse(dtRslt.Rows[k]["ROW"].ToString());
                        iCOL = int.Parse(dtRslt.Rows[k]["COL"].ToString());
                        iSTG = int.Parse(dtRslt.Rows[k]["STG"].ToString());

                        EQPTID = Util.NVC(dtRslt.Rows[k]["EQPTID"].ToString());
                        CSTID = Util.NVC(dtRslt.Rows[k]["CSTID"].ToString());
                        LOTID = Util.NVC(dtRslt.Rows[k]["LOTID"].ToString());
                        EIOSTAT = Util.NVC(dtRslt.Rows[k]["EIOSTAT"].ToString());
                        EQP_OP_STATUS_CD = Util.NVC(dtRslt.Rows[k]["EQP_OP_STATUS_CD"].ToString());
                        RUN_MODE_CD = Util.NVC(dtRslt.Rows[k]["RUN_MODE_CD"].ToString());
                        PROCID = Util.NVC(dtRslt.Rows[k]["PROCID"].ToString());
                        PROCNAME = Util.NVC(dtRslt.Rows[k]["PROCNAME"].ToString());
                        NEXT_PROCID = Util.NVC(dtRslt.Rows[k]["NEXT_PROCID"].ToString()); //임시
                        RCV_ISS_ID = Util.NVC(dtRslt.Rows[k]["RCV_ISS_ID"].ToString());
                        FORMSTATUS = Util.NVC(dtRslt.Rows[k]["FORMSTATUS"].ToString());
                        EQPTIUSE = Util.NVC(dtRslt.Rows[k]["EQPTIUSE"].ToString());
                        NEXT_PROCNAME = Util.NVC(dtRslt.Rows[k]["NEXT_PROCNAME"].ToString());
                        PROD_LOTID = Util.NVC(dtRslt.Rows[k]["PROD_LOTID"].ToString());
                        JOB_TIME = Util.NVC(dtRslt.Rows[k]["JOB_TIME"].ToString());
                        ROUTID = Util.NVC(dtRslt.Rows[k]["ROUTID"].ToString());
                        DUMMY_FLAG = Util.NVC(dtRslt.Rows[k]["DUMMY_FLAG"].ToString());
                        SPECIAL_YN = Util.NVC(dtRslt.Rows[k]["SPECIAL_YN"].ToString());
                        LAST_RUN_TIME = Util.NVC(dtRslt.Rows[k]["LAST_RUN_TIME"].ToString());
                        JIG_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["JIG_AVG_TEMP"].ToString());
                        POW_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["POW_AVG_TEMP"].ToString());
                        MEGA_DCHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_DCHG_FUNC_YN"].ToString());
                        MEGA_CHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_CHG_FUNC_YN"].ToString());
                        NOW_TIME = Util.NVC(dtRslt.Rows[k]["NOW_TIME"].ToString());
                        PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["PROC_DETL_TYPE_CODE"].ToString());
                        NEXT_PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["NEXT_PROC_DETL_TYPE_CODE"].ToString());
                        ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가

                        START_TIME = Util.NVC(dtRslt.Rows[k]["OP_START_TIME"].ToString());
                        MIN_TEMP = Util.NVC(dtRslt.Rows[k]["MIN_TEMP"].ToString());
                        MAX_TEMP = Util.NVC(dtRslt.Rows[k]["MAX_TEMP"].ToString());
                        TRANS_CSTID = Util.NVC(dtRslt.Rows[k]["TRANS_CST"].ToString());
                        WIPSTAT = Util.NVC(dtRslt.Rows[k]["WIPSTAT"].ToString());

                        for (int row = 0; row < rowList.Count; row++)
                        {
                            if (int.Parse(rowList[row]) == iROW)
                            {
                                dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                                dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL] = k.ToString();
                            }
                        }

                        #region MyRegion
                        string BCOLOR = "Black";
                        string FCOLOR = "White";
                        string TEXT = string.Empty;
                        sStatus = string.Empty;

                        DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                        if (drColor.Length > 0)
                        {
                            BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                            FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                        }

                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (rdoOpStart.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if (rdoTime.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (rdoMaxTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MAX_TEMP) ? string.Empty : double.Parse(MAX_TEMP).ToString("00.0"));
                            }
                            else if (rdoMinTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MIN_TEMP) ? string.Empty : double.Parse(MIN_TEMP).ToString("00.0"));
                            }
                            else if (rdoAvgTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (rdoTrayId.IsChecked == true)
                            {
                                if (WIPSTAT.Equals("WAIT"))
                                {
                                    sStatus = CSTID + " [" + PROC_DETL_TYPE_CODE + "]";
                                }
                                else
                                {
                                    sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (rdoLotId.IsChecked == true)
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (rdoTime.IsChecked == true)
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (rdoOpStart.IsChecked == true)
                            {
                                sStatus = START_TIME;
                            }
                            else if (rdoRouteNextOp.IsChecked == true)
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (rdoMaxTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MAX_TEMP) ? string.Empty : double.Parse(MAX_TEMP).ToString("00.0"));
                            }
                            else if (rdoMinTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MIN_TEMP) ? string.Empty : double.Parse(MIN_TEMP).ToString("00.0"));
                            }
                            else if (rdoAvgTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }

                            if (DUMMY_FLAG.Equals("Y"))
                                FCOLOR = (BCOLOR == "Blue") ? "RoyalBlue" : "Blue";

                            if (SPECIAL_YN.Equals("Y"))
                            {
                                FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
                                bBold = false;
                            }
                            else if (SPECIAL_YN.Equals("I"))
                            {
                                FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
                                bBold = true;
                            }
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

                                if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
                                    BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

                                if (rdoTime.IsChecked == true)
                                    sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + TRANS_CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                                break;
                            case "25": //수리중
                                       //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
                                sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + getMaintName(EQPTID, LAST_RUN_TIME);
                                break;
                            case "22": //사용안함
                                sStatus = ObjectDic.Instance.GetObjectName("USE_N");
                                break;
                            case "27": //화재
                                sStatus = ObjectDic.Instance.GetObjectName("FIRE");
                                break;
                            case "": //MANUAL - NULL
                                BCOLOR = "White";
                                FCOLOR = "Black";
                                sStatus = ObjectDic.Instance.GetObjectName("MANUAL");
                                break;
                        }

                        dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
                        dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
                        dtRslt.Rows[k]["TEXT"] = sStatus;
                        dtRslt.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";
                        #endregion

                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtList[row].Rows.InsertAt(drListHeader[row], 0);
                    }

                    //상,하 Merge
                    DataTable dtTotal = new DataTable();
                    for (int row = 0; row < rowList.Count; row++)
                    {
                        dtTotal.Merge(dtList[row], false, MissingSchemaAction.Add);
                    }
                    _dtCopy = dtTotal.Copy();

                    dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                    dgFormation.ItemsSource = DataTableConverter.Convert(dtTotal);
                    dgFormation.UpdateLayout();

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgFormation.Columns)
                        dgc.VerticalAlignment = VerticalAlignment.Center;

                    string[] sColumnName = new string[] { "1" };
                    Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    #endregion

                }
                else
                {
                    #region Grid 초기화
                    iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
                    iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

                    //iRowCount = iMaxStg * 2;
                    iRowCount = iMaxStg;
                    iColumnCount = (iMaxCol + 1) / 2 + 1;   //단 추가.

                    //Column Width get
                    dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

                    //Row Height get
                    //dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2) - 2.3;
                    if (rowHeight == 0)
                        rowHeight = dgFormation.ActualHeight;
                    //dRowHeight = rowHeight / (iRowCount * 2) - 2.3; // TempData 크기증가로 인한 사이즈 조절
                    dRowHeight = rowHeight / ((iRowCount + 1) * 2) - 1; // TempData 크기증가로 인한 사이즈 조절

                    //상단 Datatable, 하단 Datatable 정의
                    DataTable Udt = new DataTable();
                    DataTable Ddt = new DataTable();

                    //Column 생성
                    for (int i = 0; i < iColumnCount; i++)
                    {
                        //GRID Column Create
                        if (i == 0)
                            SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50);
                        else
                            SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth);

                        Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                    }

                    //Row 생성
                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow Urow = Udt.NewRow();
                        Udt.Rows.Add(Urow);

                        DataRow Drow = Ddt.NewRow();
                        Ddt.Rows.Add(Drow);
                    }

                    //Row Header 생성
                    DataRow UrowHeader = Udt.NewRow();
                    for (int i = 0; i < Udt.Columns.Count; i++)
                    {
                        if (i == 0)
                            UrowHeader[i] = string.Empty;
                        else
                            UrowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
                    }

                    //Row Header 생성
                    DataRow DrowHeader = Ddt.NewRow();
                    for (int i = 0; i < Ddt.Columns.Count; i++)
                    {
                        if (i == 0)
                            DrowHeader[i] = string.Empty;
                        else
                            DrowHeader[i] = (i + Ddt.Columns.Count - 1).ToString() + ObjectDic.Instance.GetObjectName("연");
                    }
                    #endregion

                    #region Data Setting
                    //dtRslt Column Add
                    dtRslt.Columns.Add("BCOLOR", typeof(string));
                    dtRslt.Columns.Add("FCOLOR", typeof(string));
                    dtRslt.Columns.Add("TEXT", typeof(string));
                    dtRslt.Columns.Add("BOLD", typeof(string));
                    
                    for (int k = 0; k < dtRslt.Rows.Count; k++)
                    {
                        iROW = int.Parse(dtRslt.Rows[k]["ROW"].ToString());
                        iCOL = int.Parse(dtRslt.Rows[k]["COL"].ToString());
                        iSTG = int.Parse(dtRslt.Rows[k]["STG"].ToString());

                        EQPTID = Util.NVC(dtRslt.Rows[k]["EQPTID"].ToString());
                        CSTID = Util.NVC(dtRslt.Rows[k]["CSTID"].ToString());
                        LOTID = Util.NVC(dtRslt.Rows[k]["LOTID"].ToString());
                        EIOSTAT = Util.NVC(dtRslt.Rows[k]["EIOSTAT"].ToString());
                        EQP_OP_STATUS_CD = Util.NVC(dtRslt.Rows[k]["EQP_OP_STATUS_CD"].ToString());
                        RUN_MODE_CD = Util.NVC(dtRslt.Rows[k]["RUN_MODE_CD"].ToString());
                        PROCID = Util.NVC(dtRslt.Rows[k]["PROCID"].ToString());
                        PROCNAME = Util.NVC(dtRslt.Rows[k]["PROCNAME"].ToString());
                        NEXT_PROCID = Util.NVC(dtRslt.Rows[k]["NEXT_PROCID"].ToString()); //임시
                        RCV_ISS_ID = Util.NVC(dtRslt.Rows[k]["RCV_ISS_ID"].ToString());
                        FORMSTATUS = Util.NVC(dtRslt.Rows[k]["FORMSTATUS"].ToString());
                        EQPTIUSE = Util.NVC(dtRslt.Rows[k]["EQPTIUSE"].ToString());
                        NEXT_PROCNAME = Util.NVC(dtRslt.Rows[k]["NEXT_PROCNAME"].ToString());
                        PROD_LOTID = Util.NVC(dtRslt.Rows[k]["PROD_LOTID"].ToString());
                        JOB_TIME = Util.NVC(dtRslt.Rows[k]["JOB_TIME"].ToString());
                        ROUTID = Util.NVC(dtRslt.Rows[k]["ROUTID"].ToString());
                        DUMMY_FLAG = Util.NVC(dtRslt.Rows[k]["DUMMY_FLAG"].ToString());
                        SPECIAL_YN = Util.NVC(dtRslt.Rows[k]["SPECIAL_YN"].ToString());
                        LAST_RUN_TIME = Util.NVC(dtRslt.Rows[k]["LAST_RUN_TIME"].ToString());
                        JIG_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["JIG_AVG_TEMP"].ToString());
                        POW_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["POW_AVG_TEMP"].ToString());
                        MEGA_DCHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_DCHG_FUNC_YN"].ToString());
                        MEGA_CHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_CHG_FUNC_YN"].ToString());
                        NOW_TIME = Util.NVC(dtRslt.Rows[k]["NOW_TIME"].ToString());
                        PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["PROC_DETL_TYPE_CODE"].ToString());
                        NEXT_PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["NEXT_PROC_DETL_TYPE_CODE"].ToString());
                        ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가

                        START_TIME = Util.NVC(dtRslt.Rows[k]["OP_START_TIME"].ToString());
                        MIN_TEMP = Util.NVC(dtRslt.Rows[k]["MIN_TEMP"].ToString());
                        MAX_TEMP = Util.NVC(dtRslt.Rows[k]["MAX_TEMP"].ToString());
                        TRANS_CSTID = Util.NVC(dtRslt.Rows[k]["TRANS_CST"].ToString());
                        WIPSTAT = Util.NVC(dtRslt.Rows[k]["WIPSTAT"].ToString());

                        if (iCOL <= iColumnCount - 1)
                        {
                            Udt.Rows[iRowCount - iSTG][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                            Udt.Rows[iRowCount - iSTG][iCOL] = k.ToString();
                        }
                        else
                        {
                            Ddt.Rows[iRowCount - iSTG][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                            Ddt.Rows[iRowCount - iSTG][iCOL - iColumnCount + 1] = k.ToString();
                        }

                        #region MyRegion
                        string BCOLOR = "Black";
                        string FCOLOR = "White";
                        string TEXT = string.Empty;
                        sStatus = string.Empty;

                        DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                        if (drColor.Length > 0)
                        {
                            BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                            FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                        }

                        if (string.IsNullOrEmpty(CSTID))
                        {
                            sStatus = string.Empty;

                            if (rdoOpStart.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                                {
                                    sStatus = "T )";
                                    sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                                }
                            }
                            else if (rdoTime.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                            {
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            }
                            else if (rdoMaxTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MAX_TEMP) ? string.Empty : double.Parse(MAX_TEMP).ToString("00.0"));
                            }
                            else if (rdoMinTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MIN_TEMP) ? string.Empty : double.Parse(MIN_TEMP).ToString("00.0"));
                            }
                            else if (rdoAvgTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }
                        }
                        else
                        {
                            if (rdoTrayId.IsChecked == true)
                            {
                                if (WIPSTAT.Equals("WAIT"))
                                {
                                    sStatus = CSTID + " [" + PROC_DETL_TYPE_CODE + "]";
                                }
                                else
                                {
                                    if(string.IsNullOrEmpty(NEXT_PROC_DETL_TYPE_CODE))
                                        sStatus = CSTID ;
                                    else
                                        sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (rdoLotId.IsChecked == true)
                            {
                                if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.Equals("Y"))
                                {
                                    sStatus = PROD_LOTID + " [Auto Calib]";
                                }
                                else
                                {
                                    sStatus = PROD_LOTID;
                                }
                            }
                            else if (rdoTime.IsChecked == true)
                            {
                                if (!string.IsNullOrEmpty(JOB_TIME))
                                {
                                    tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                                    sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                    sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                                }
                            }
                            else if (rdoOpStart.IsChecked == true)
                            {
                                sStatus = START_TIME;
                            }
                            else if (rdoRouteNextOp.IsChecked == true)
                            {
                                sStatus = ROUTID + " [" + NEXT_PROCID + "]";
                            }
                            else if (rdoMaxTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MAX_TEMP) ? string.Empty : double.Parse(MAX_TEMP).ToString("00.0"));
                            }
                            else if (rdoMinTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(MIN_TEMP) ? string.Empty : double.Parse(MIN_TEMP).ToString("00.0"));
                            }
                            else if (rdoAvgTemp.IsChecked == true)
                            {
                                sStatus = (string.IsNullOrEmpty(JIG_AVG_TEMP) ? string.Empty : double.Parse(JIG_AVG_TEMP).ToString("00.0")) + " / " + (string.IsNullOrEmpty(POW_AVG_TEMP) ? string.Empty : double.Parse(POW_AVG_TEMP).ToString("00.0"));
                            }

                            if (DUMMY_FLAG.Equals("Y"))
                                FCOLOR = (BCOLOR == "Blue") ? "RoyalBlue" : "Blue";

                            if (SPECIAL_YN.Equals("Y"))
                            {
                                FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
                                bBold = false;
                            }
                            else if (SPECIAL_YN.Equals("I"))
                            {
                                FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
                                bBold = true;
                            }
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

                                if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
                                    BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

                                if (rdoTime.IsChecked == true)
                                    sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + TRANS_CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                                if (!DUMMY_FLAG.Equals("Y") && !SPECIAL_YN.Equals("Y") && !SPECIAL_YN.Equals("I"))
                                    FCOLOR = "Black";
                                BCOLOR = "White";
                                break;
                            case "25": //수리중
                                       //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
                                sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + getMaintName(EQPTID, LAST_RUN_TIME);
                                BCOLOR = "White";
                                if (!DUMMY_FLAG.Equals("Y") && !SPECIAL_YN.Equals("Y") && !SPECIAL_YN.Equals("I"))
                                    FCOLOR = "Black";
                                break;
                            case "22": //사용안함
                                sStatus = ObjectDic.Instance.GetObjectName("USE_N");
                                break;
                            case "27": //화재
                                sStatus = ObjectDic.Instance.GetObjectName("FIRE");
                                break;
                            case "": //MANUAL - NULL
                                BCOLOR = "White";
                                FCOLOR = "Black";
                                sStatus = ObjectDic.Instance.GetObjectName("MANUAL");
                                break;
                        }
                      if(!string.IsNullOrEmpty(CSTID) && 
                         DBNull.Value != dtRslt.Rows[k]["EQPT_CTRL_STAT_CODE"] &&
                         dtRslt.Rows[k]["EQPT_CTRL_STAT_CODE"].ToString() == string.Empty)
                            sStatus = ObjectDic.Instance.GetObjectName("작업 제어 거부");

                        dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
                        dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
                        dtRslt.Rows[k]["TEXT"] = sStatus;
                        dtRslt.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";
                        #endregion
                    }
                    #endregion

                    #region Grid 조합
                    //DataTable Header Insert
                    Udt.Rows.InsertAt(UrowHeader, 0);
                    Ddt.Rows.InsertAt(DrowHeader, 0);

                    //상,하 Merge
                    Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                    _dtCopy = Udt.Copy();

                    dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                    dgFormation.ItemsSource = DataTableConverter.Convert(Udt);
                    dgFormation.UpdateLayout();

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgFormation.Columns)
                        dgc.VerticalAlignment = VerticalAlignment.Center;

                    string[] sColumnName = new string[] { "1" };
                    Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
                
            });
        }

        private void ClearALL()
        {
            _dtCopy = null;
            _dtDATA = null;
            dgFormation.ItemsSource = null;
            dgFormation.Columns.Clear();
            dgFormation.Refresh();

            ClearControl();
        }

        private void ClearControl()
        {
            txtSelCol.Text = string.Empty;
            txtSelStg.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtTroubleName.Text = string.Empty;
            txtTroubleRepairWay.Text = string.Empty;
            txtRemark.Text = string.Empty;
            txtCH_INFO.Text = string.Empty;
        }

        private string getMaintName(string sEqpId, string sLastRunTime)// 작업자 이름 가져오기, 부동유형 가져오기
        {
            string sReturn = string.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqpId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TRAY_EQP_MAINT_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                    sReturn = ObjectDic.Instance.GetObjectName("MANUAL") + " " + sLastRunTime;  //수동
                else
                    sReturn = dtRslt.Rows[0]["TROUBLE_REPAIR_CD2"].ToString() + " " + dtRslt.Rows[0]["TROUBLE_REPAIR_TIME"].ToString(); //TROUBLE_REPAIR_TIME 와 TROUBLE_REPAIR_TIME2가 동일함(Format만 틀림)
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sReturn;
        }

        private void GetTempData(string sEqpId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = sEqpId;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TEMP_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return;

                DataTable dtTemp = new DataTable();

                dtTemp.Columns.Add("TEMP01", typeof(string));
                dtTemp.Columns.Add("TEMP02", typeof(string));
                dtTemp.Columns.Add("TEMP03", typeof(string));
                dtTemp.Columns.Add("TEMP04", typeof(string));
                dtTemp.Columns.Add("TEMP05", typeof(string));
                dtTemp.Columns.Add("TEMP06", typeof(string));
                dtTemp.Columns.Add("TEMP07", typeof(string));
                dtTemp.Columns.Add("TEMP08", typeof(string));
                dtTemp.Columns.Add("TEMP09", typeof(string));
                dtTemp.Columns.Add("TEMP10", typeof(string));
                dtTemp.Columns.Add("TEMP11", typeof(string));
                dtTemp.Columns.Add("TEMP12", typeof(string));

                for (int iRow = 0; iRow < 12; iRow++)
                {
                    DataRow dr1 = dtTemp.NewRow();
                    dtTemp.Rows.Add(dr1);
                }

                // TEMP01
                int dtCol = 8;

                for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
                {
                    for (int iRow = 0; iRow < dtTemp.Rows.Count; iRow++)
                    {
                        if (dtCol != dtRslt.Columns.Count)
                        {
                            dtTemp.Rows[iRow][iCol] = dtRslt.Rows[0][dtCol];

                            dtCol++;
                        }
                    }
                }






                //    for (int iRow = 0; iRow<12;iRow++)
                //{
                //    DataRow dr1 = dtTemp.NewRow();

                //    for (int iCol=0;iCol<dtTemp.Columns.Count; iCol++)
                //    {

                //        if (dtCol != dtRslt.Columns.Count)
                //        {
                //            dr1[iCol] = dtRslt.Rows[0][dtCol];

                //            dtCol++;
                //        }
                //    }

                //    dtTemp.Rows.Add(dr1);
                //}





                Util.GridSetData(dgTemp, dtTemp, FrameOperation, true);

                
 //               dgTemp.GetCell(0,0).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                //if (_GridSize == false)
                //{
                //    btnSearch_Click(null, null);
                //    _GridSize = true;
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetDtVAlue(string sCstID, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                DataRow[] dr = _dtDATA.Select("CSTID = '" + sCstID + "'");

                foreach (var row in dr)
                {
                    sRtnValue = row[sFindCol] as string;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
        }

        private string GetDtRowValue(string iRowNum, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                if (int.Parse(iRowNum) >= _dtDATA.Rows.Count)
                    return sRtnValue;

                sRtnValue = _dtDATA.Rows[int.Parse(iRowNum)][sFindCol].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            //RadioButton rdo = (RadioButton)sender;
            //if (rdo.IsChecked == true)
            //{
            //    btnSearch_Click(null, null);
            //}
        }

        private string GetCstID(string sCellValue)
        {
            string sCstID = string.Empty;
            try
            {
                int start = sCellValue.IndexOf("[");

                if (start > 0)
                    sCstID = sCellValue.Substring(0, start - 1);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sCstID;
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

        private void GetFormationRatioRoute()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TOP_INFO_JDA_MB", "RQSTDT", "RSLTDT", dtRqst);
                dgQtybyRoute.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void boxStatusView()
        {
            try
            {
                txtRate.Text = string.Empty;
                txtRateABLE.Text = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_RATIO_LANE_JDA_MB", "RQSTDT", "RSLTDT", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    txtRate.Text = dtRslt.Rows[i]["USE_COUNT"].ToString();
                    txtRateABLE.Text = dtRslt.Rows[i]["ABLE_COUNT"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            dgTemp.ItemsSource = null;
            dgTemp.Refresh();
        }

        private void dgFormation_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void chkTemp_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTemp.ItemsSource = null;
            dgTemp.Refresh();
        }

        private void btnRsv_Click(object sender, RoutedEventArgs e)
        {
            //확인 메세지 팝업

            FCS002_001_RESERVE Rsv_List = new FCS002_001_RESERVE();
            Rsv_List.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = LoginInfo.LANGID;   //LANGID
            parameters[1] = _LANEID; // LANEID
         
            C1WindowExtension.SetParameters(Rsv_List, parameters);
            Rsv_List.Closed += new EventHandler(Rsv_List_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => Rsv_List.ShowModal()));
            Rsv_List.BringToFront();
        }

        private void Rsv_List_Closed(object sender, EventArgs e)
        {
            FCS002_001_RESERVE window = sender as FCS002_001_RESERVE;

            this.grdMain.Children.Remove(window);
        }

    }
}
