/*************************************************************************************
 Created Date : 2023.01.04
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 High Chamber 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.04  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Data;
using System.Windows.Threading;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_316 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LANEID = string.Empty;


        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private string _MENUID = string.Empty;

        private Point prevRowPos = new Point(0, 0);
        private Point prevColPos = new Point(0, 0);

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        Util Util = new Util();

        // 속도 향상을 위해 반복문 내부 비즈 및 다국어 밖으로 뺌.
        //private DataTable dtRepair = null;
        private string langYun = string.Empty;
        private string langYul = string.Empty;
        private string langDan = string.Empty;
        private string COMM_LOSS = string.Empty;
        private string RESV = string.Empty;
        private string MAINTENANCE = string.Empty;
        private string REPAIR = string.Empty;
        private string USE_N = string.Empty;
        private string FIRE = string.Empty;
        private string NO_EQP = string.Empty;
        private string MANUAL = string.Empty;
        private string PRE = string.Empty;

        public FCS001_316()
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
            try
            {
                string sLaneID = string.Empty;

                if (string.IsNullOrEmpty(_MENUID))
                    _MENUID = LoginInfo.CFG_MENUID;

                sLaneID = GetLaneIDForMenu(_MENUID);
                
                _LANEID = sLaneID;

                InitLegend();

                rdoTrayId.IsChecked = true;
                rdoTrayInfo.IsChecked = true;

                rdoTrayId.Checked += rdo_CheckedChanged;
                rdoTrayId.Unchecked += rdo_CheckedChanged;
                rdoLotId.Checked += rdo_CheckedChanged;
                rdoLotId.Unchecked += rdo_CheckedChanged;
                rdoRouteNextOp.Checked += rdo_CheckedChanged;
                rdoRouteNextOp.Unchecked += rdo_CheckedChanged;
                rdoTime.Checked += rdo_CheckedChanged;
                rdoTime.Unchecked += rdo_CheckedChanged;
                rdoAvgTemp.Checked += rdo_CheckedChanged;
                rdoAvgTemp.Unchecked += rdo_CheckedChanged;

                // 속도 향상을 위해 반복문 내부 다국어 밖으로 뺌.
                langYun = ObjectDic.Instance.GetObjectName("연");
                langYul = ObjectDic.Instance.GetObjectName("열");
                langDan = ObjectDic.Instance.GetObjectName("단");
                COMM_LOSS = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                RESV = ObjectDic.Instance.GetObjectName("RESV");
                MAINTENANCE = ObjectDic.Instance.GetObjectName("MAINTENANCE");
                REPAIR = ObjectDic.Instance.GetObjectName("REPAIR");
                USE_N = ObjectDic.Instance.GetObjectName("USE_N");
                FIRE = ObjectDic.Instance.GetObjectName("FIRE");
                NO_EQP = ObjectDic.Instance.GetObjectName("NO_EQP");
                MANUAL = ObjectDic.Instance.GetObjectName("MANUAL");
                PRE = ObjectDic.Instance.GetObjectName("FORMATION_RESERV");

                GetListFormation();

                _timer = new DispatcherTimer();

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                GetListFormation();
                sec = 0;
            }
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListFormation();
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
                    }
                }
            }));
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


        private void dgFormation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgFormation.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter != null)
                        dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter.Background = Brushes.LightGray;
                    if (dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter != null)
                        dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter.Background = Brushes.LightGray;

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    int i = 0;
                    //if (!dgFormation.GetValue(cell.Row.Index, "1").Equals(string.Empty))
                    if (int.TryParse(ROWNUM, out i))
                    {
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
                            dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter.Background = Brushes.White;

                            for (int row = cell.Row.Index; row >= 0; row--)
                            {
                                //if (dgFormation.GetValue(row, "1").Equals(string.Empty))
                                if (!int.TryParse(ROWNUM, out i))
                                {
                                    prevColPos = new Point(row, cell.Column.Index);
                                    break;
                                }
                            }
                            if (dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter != null)
                                dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter.Background = Brushes.White;
                        }
                    }

                    if (string.IsNullOrEmpty(ROWNUM)) return;
                    if (!int.TryParse(ROWNUM, out i)) return;

                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string CST_LOAD_LOCATION_CODE = Util.NVC(GetDtRowValue(ROWNUM, "CST_LOAD_LOCATION_CODE"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));

                    if (!string.IsNullOrEmpty(EQPTID))
                    {
                        GetTempData(EQPTID);

                        ClearControl();
                        txtSelStg.Text = STG;
                        txtSelCol.Text = COL;

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

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_TRAY_EQP_MAINT", "INDATA", "TRAY,EQUIPMENT,MAINT", ds);

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
                        else if (dsRslt.Tables["EQUIPMENT"].Rows.Count > 0 && FORMSTATUS.Equals("16")) //Trouble
                        {
                            txtStatus.Text = ObjectDic.Instance.GetObjectName("TROUBLE");
                        }
                        else
                        {
                            txtStatus.Text = string.Empty;
                        }

                        // Box Trouble 정보
                        if (dsRslt.Tables["EQUIPMENT"].Rows.Count > 0 && (FORMSTATUS.Equals("16") || FORMSTATUS.Equals("17"))) //Trouble 또는 일시정지
                        {
                            txtTroubleName.Text = Util.NVC(dsRslt.Tables["EQUIPMENT"].Rows[0]["TROUBLE_NAME"].ToString());
                        }
                        else if (dsRslt.Tables["MAINT"].Rows.Count > 0)
                        {
                            try
                            {
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
                            txtTroubleName.Text = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                    int result = 0;
                    //20220420_에러수정(입력 문자열의 형식이 잘못되었습니다.) START
                    if (string.IsNullOrEmpty(Util.NVC(ROWNUM)))
                    {
                        return;
                    }
                    if (!int.TryParse(ROWNUM, out result))
                    {
                        return;
                    }
                    //20220420_에러수정(입력 문자열의 형식이 잘못되었습니다.) END
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string ROW = Util.NVC(GetDtRowValue(ROWNUM, "ROW"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string CST_LOAD_LOCATION_CODE = Util.NVC(GetDtRowValue(ROWNUM, "CST_LOAD_LOCATION_CODE"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string LOTID = Util.NVC(GetDtRowValue(ROWNUM, "LOTID"));
                    string LANEID = _LANEID;
                    //if (bUseFlag)
                    //{
                    //    LANEID = Util.NVC(GetDtRowValue(ROWNUM, "LANE_ID"));

                    //}

                    if (rdoTrayInfo.IsChecked == true)
                    {
                        if (string.IsNullOrEmpty(CSTID))
                            return;

                        FCS001_021 wndRunStart = new FCS001_021();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = CSTID;
                            Parameters[1] = LOTID;

                            this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                        }
                    }
                    else if (rdoEqpControl.IsChecked == true)
                    {
                        FCS001_316_DETAIL wndRunStart = new FCS001_316_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = _LANEID;
                            //if (bUseFlag)
                            //{
                            //    Parameters[4] = LANEID;
                            //}

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }
                    }
                    else if (rdoTempInfo.IsChecked == true)
                    {
                        //FCS001_049
                        FCS001_049 wndRunStart = new FCS001_049();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = _LANEID;
                            //if (bUseFlag)
                            //{
                            //    Parameters[0] = LANEID;
                            //}

                            Parameters[1] = EQPTID;

                            this.FrameOperation.OpenMenu("SFU010715110", true, Parameters);
                        }
                    }
                    else if (rdoTrayHistory.IsChecked == true)
                    {
                        //FCS001_001_TRAY_HIST
                        FCS001_001_TRAY_HISTORY wndRunStart = new FCS001_001_TRAY_HISTORY();
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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                    string ROWNUM = _dtCopy.Rows[e.Cell.Row.Index][(e.Cell.Column.Index + 1).ToString()].ToString();
                    int result = 0;

                    if (!int.TryParse(ROWNUM, out result))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = Brushes.Black;
                        //2023.06.07 추가
                        e.Cell.Presenter.Padding = new Thickness(0);
                        e.Cell.Presenter.Margin = new Thickness(0);
                        e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                        e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                        
                        if(string.IsNullOrEmpty(ROWNUM) && e.Cell.Column.Index != 0 && e.Cell.Row.Index != 0)
                        {
                            //설비없음
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), NO_EQP);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;

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
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), NO_EQP);
                        }

                        if (!string.IsNullOrEmpty(ROWNUM))
                        {
                            string CCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "CCOLOR"));

                            if (!string.IsNullOrEmpty(CCOLOR) && CCOLOR.Equals("Red"))
                            {
                                e.Cell.Presenter.BorderBrush = new BrushConverter().ConvertFromString(CCOLOR) as SolidColorBrush;
                                e.Cell.Presenter.BorderThickness = new Thickness(2, 2, 2, 2);
                            }

                            else
                            {
                                e.Cell.Presenter.Padding = new Thickness(0);
                                e.Cell.Presenter.Margin = new Thickness(0);
                                e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                                e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                            }

                        }

                    }

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
         
        private void dgFormation_ExecuteCustomBinding(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            _dtDATA = e.ResultData as DataTable;

                SetFpsFormationData(_dtDATA);

        }

        private void dgFormation_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
                gdDisplayType.IsEnabled = true;
                gdDetailType.IsEnabled = true;
            }
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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sLaneID;
        }

        private void InitLegend()
        {
            try
            {
                C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("-LEGEND-") };
                cboColorLegend.Items.Add(cbItemTiTle);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "HIGHCHAMBER_STATUS";
                RQSTDT.Rows.Add(dr);

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
                    cboColorLegend.Items.Add(cbItem);
                }

                cboColorLegend.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                int iCST_LOAD_LOCATION_CODE;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string EIOSTAT = string.Empty;
                string RUN_MODE_CD = string.Empty;
                string PROCID = string.Empty;
                string PROCNAME = string.Empty;
                string NEXT_PROCID = string.Empty;
                string RCV_ISS_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string EQPTIUSE = string.Empty;
                string PROD_LOTID = string.Empty;
                string JOB_TIME = string.Empty;
                string ROUTID = string.Empty;
                string DUMMY_FLAG = string.Empty;
                string SPECIAL_YN = string.Empty;
                string JIG_AVG_TEMP = string.Empty;
                string POW_AVG_TEMP = string.Empty;
                string NOW_TIME = string.Empty;
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                #endregion

                //충방전기 열 갯수 확인
                DataView view = dtRslt.DefaultView;
                DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
                List<string> rowList = new List<string>();
                foreach (DataRow dr in distRowTable.Rows)
                {
                    rowList.Add(dr["ROW"].ToString());
                }

                #region Grid 초기화
                iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
                iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

                iRowCount = iMaxStg * 4;
                iColumnCount = (iMaxCol + 1);   //단 추가.

                //Column Width get
                dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

                //Row Height get
                dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 1.3);

                //상단 Datatable, 하단 Datatable 정의
                DataTable Udt = new DataTable();
                DataTable Ddt = new DataTable();

                //Column 생성
                for (int i = 0; i < iColumnCount; i++)
                {
                    //GRID Column Create
                    if (i == 0)
                        SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50, VerticalAlignment.Center);
                    else
                        SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth, VerticalAlignment.Center);

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
                        UrowHeader[i] = (i).ToString() + langYun;
                }

                //Row Header 생성
                DataRow DrowHeader = Ddt.NewRow();
                for (int i = 0; i < Ddt.Columns.Count; i++)
                {
                    if (i == 0)
                        DrowHeader[i] = string.Empty;
                    else
                        DrowHeader[i] = (i + Ddt.Columns.Count - 1).ToString() + langYun;
                }
                #endregion

                #region Data Setting
                //dtRslt Column Add
                dtRslt.Columns.Add("BCOLOR", typeof(string));
                dtRslt.Columns.Add("FCOLOR", typeof(string));
                dtRslt.Columns.Add("CCOLOR", typeof(string));
                dtRslt.Columns.Add("TEXT", typeof(string));
                dtRslt.Columns.Add("BOLD", typeof(string));

                int k = -1;
                foreach (DataRow dr in dtRslt.Rows)
                {
                    k++;

                    iROW = dr["ROW"].NvcInt();
                    iCOL = dr["COL"].NvcInt();
                    iSTG = dr["STG"].NvcInt();
                    iCST_LOAD_LOCATION_CODE = dr["CST_LOAD_LOCATION_CODE"].NvcInt();

                    EQPTID = dr["EQPTID"].Nvc();
                    CSTID = dr["CSTID"].Nvc();
                    LOTID = dr["LOTID"].Nvc();
                    EIOSTAT = dr["EIOSTAT"].Nvc();
                    RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                    PROCID = dr["PROCID"].Nvc();
                    PROCNAME = dr["PROCNAME"].Nvc();
                    NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                    RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                    FORMSTATUS = dr["FORMSTATUS"].Nvc();
                    EQPTIUSE = dr["EQPTIUSE"].Nvc();
                    PROD_LOTID = dr["PROD_LOTID"].Nvc();
                    JOB_TIME = dr["JOB_TIME"].Nvc();
                    ROUTID = dr["ROUTID"].Nvc();
                    DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                    SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                    JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                    POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                    NOW_TIME = dr["NOW_TIME"].Nvc();
                    NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                    ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc(); //20211018 Auto Calibration Lot표시 추가

                    if (iCOL <= iColumnCount - 1)
                    {
                        if (iCST_LOAD_LOCATION_CODE == 1)
                        {
                            Udt.Rows[iRowCount - (iSTG * 4) + 3][0] = iSTG.ToString() + langDan;  //단
                            Udt.Rows[iRowCount - (iSTG * 4) + 3][iCOL] = k.ToString();
                        }
                        else if (iCST_LOAD_LOCATION_CODE == 2)
                        {
                            Udt.Rows[iRowCount - (iSTG * 4) + 2][0] = iSTG.ToString() + langDan;  //단
                            Udt.Rows[iRowCount - (iSTG * 4) + 2][iCOL] = k.ToString();
                        }
                        else if (iCST_LOAD_LOCATION_CODE == 3)
                        {
                            Udt.Rows[iRowCount - (iSTG * 4) + 1][0] = iSTG.ToString() + langDan;  //단
                            Udt.Rows[iRowCount - (iSTG * 4) + 1][iCOL] = k.ToString();
                        }
                        else
                        {
                            Udt.Rows[iRowCount - (iSTG * 4)][0] = iSTG.ToString() + langDan;  //단
                            Udt.Rows[iRowCount - (iSTG * 4)][iCOL] = k.ToString();
                        }
                    }
                    else
                    {
                        if (iCST_LOAD_LOCATION_CODE > 1)
                        {
                            Ddt.Rows[iRowCount - (iSTG * 4)][0] = iSTG.ToString() + langDan;  //단
                            Ddt.Rows[iRowCount - (iSTG * 4)][iCOL - iColumnCount + 1] = k.ToString();
                        }
                        else
                        {
                            Ddt.Rows[iRowCount - (iSTG * 4) + 1][0] = iSTG.ToString() + langDan;  //단
                            Ddt.Rows[iRowCount - (iSTG * 4) + 1][iCOL - iColumnCount + 1] = k.ToString();
                        }
                    }

                    #region MyRegion
                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string CCOLOR = "Black";
                    string TEXT = string.Empty;

                    DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                    if (drColor.Length > 0)
                    {
                        BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                        FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                    }

                    if (string.IsNullOrEmpty(CSTID))
                    {
                        sStatus = string.Empty;

                        if (rdoTime.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                        {
                            if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
                            {
                                sStatus = "T )";
                                sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
                            }
                        }
                        else if (rdoRouteNextOp.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
                        {
                            tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
                            sStatus = tTimeSpan.Days.ToString("000") + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
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
                            sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
                        }
                        else if (rdoLotId.IsChecked == true)
                        {
                            //20211018 Auto Calibration Lot표시 추가 START
                            //sStatus = PROD_LOTID;
                            if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE) && ATCALIB_TYPE_CODE.ToString().Equals("Y"))
                            {
                                sStatus = PROD_LOTID + " [Auto Calib]";
                            }
                            else
                            {
                                sStatus = PROD_LOTID;
                            }
                            //20211018 Auto Calibration Lot표시 추가 END
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
                        else if (rdoRouteNextOp.IsChecked == true)
                        {
                            sStatus = ROUTID + " [" + NEXT_PROCID + "]";
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
                            sStatus = COMM_LOSS;
                            break;
                        case "19": //Power Off
                            sStatus = "Power Off";
                            break;
                        case "21": //정비중
                            sStatus = MAINTENANCE; // + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                            break;
                        case "27": //화재
                            sStatus = FIRE;
                            break;
                        case "99": //설비없음
                            BCOLOR = "Gray";
                            FCOLOR = "White";
                            sStatus = NO_EQP;
                            break;
                    }

                    dr["BCOLOR"] = BCOLOR;
                    dr["FCOLOR"] = FCOLOR;
                    dr["CCOLOR"] = CCOLOR;
                    dr["TEXT"] = sStatus;
                    dr["BOLD"] = (bBold == true) ? "Y" : "N";
                    #endregion
                }
                #endregion

                #region Grid 조합
                //DataTable Header Insert
                Udt.Rows.InsertAt(UrowHeader, 0);
                //Ddt.Rows.InsertAt(DrowHeader, 0);

                //상,하 Merge
                //Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                _dtCopy = Udt.Copy();

                dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                dgFormation.ItemsSource = DataTableConverter.Convert(Udt);

                string[] sColumnName = new string[] { "1" };
                Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                #endregion
            //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
}


        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth, VerticalAlignment vertical)
        {
            if (dg.Columns.Contains(sColName)) return;

            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = vertical,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)

            });
        }

        private void ClearALL()
        {
            _dtCopy = null;
            _dtDATA = null;
            
            ClearControl();
        }

        private void ClearControl()
        {
            txtSelCol.Text = string.Empty;
            txtSelStg.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtTroubleName.Text = string.Empty;
            txtRemark.Text = string.Empty;
        }

        private void GetTempData(string sEqpId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = sEqpId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGHCHAMBER_TEMP_HVF", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return;

                DataTable dtRsltChg = new DataTable();
                dtRsltChg.Columns.Add("SET_TEMP", typeof(Decimal));
                dtRsltChg.Columns.Add("SET_TEMP_MEASURE_TEMP", typeof(Decimal));
                dtRsltChg.Columns.Add("LOWER_MEASURE_TEMP", typeof(Decimal));
                dtRsltChg.Columns.Add("UPPER_MEASURE_TEMP", typeof(Decimal));

                DataRow drr = dtRsltChg.NewRow();
                drr["SET_TEMP"] = dtRslt.Rows[0]["SV_TEMP_01"].ToString();
                drr["SET_TEMP_MEASURE_TEMP"] =  dtRslt.Rows[0]["SV_PV_TEMP"].ToString();
                drr["LOWER_MEASURE_TEMP"] = dtRslt.Rows[0]["PV_TEMP_LOWER"].ToString();
                drr["UPPER_MEASURE_TEMP"] =  dtRslt.Rows[0]["PV_TEMP_UPPER"].ToString();
                dtRsltChg.Rows.Add(drr);

                Util.GridSetData(dgTemp, dtRsltChg, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetDtRowValue(string iRowNum, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                if (iRowNum.NvcInt() >= _dtDATA.Rows.Count)
                    return sRtnValue;

                sRtnValue = _dtDATA.Rows[iRowNum.NvcInt()][sFindCol].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sRtnValue;
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = (RadioButton)sender;
            if (rdo.IsChecked == true)
            {
                GetListFormation();
            }
        }

        private void GetListFormation()
        {
            try
            {
                btnSearch.IsEnabled = false;
                gdDisplayType.IsEnabled = false;
                gdDetailType.IsEnabled = false;

                ClearALL();

                GetFormationStatus();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetFormationStatus()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;
                dtRqst.Rows.Add(dr);

                // 멀티 스레드 실행, 다음은 순서대로 dgFormation_ExecuteCustomBinding, dgFormation_ExecuteDataCompleted 실행 됨.
                dgFormation.ExecuteService("DA_SEL_HIGHCHAMBER_VIEW_BY_LANE_HVF", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //고온챔버 수동입고 버튼
        private void btnChamberStart_Click(object sender, RoutedEventArgs e)
        {

            if (dgFormation.Selection.SelectedCells.Count == 0)
            {
                Util.MessageValidation("FM_ME_0570"); // 챔버를 먼저 선택해주세요.
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgFormation.Selection.SelectedCells)
            {
                if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                {
                    Util.MessageValidation("FM_ME_0577"); //챔버 정보가 없습니다.
                    return;
                }
                else
                {
                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();

                    string CST_LOAD_LOCATION_CODE = Util.NVC(GetDtRowValue(ROWNUM, "CST_LOAD_LOCATION_CODE"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANE_ID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANE_ID"] = _LANEID;
                    dr["EQPTID"] = EQPTID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGHCHAMBER_VIEW_BY_LANE_HVF", "RQSTDT", "RSLTDT", dtRqst);

                    // 고전압 고온챔버는 User Stop이 없어 설비작업제어(ECMD)시 EIOSTAT이 U가 아닌 W로 변경. 따라서 아래와 같이 일시정지를 정의
                    // 일시정지(17) 상태 -> 설비상태가 대기중이고(EIOSTAT = W) 해당 설비의 설비 작업 정지 여부가 Y 일때(EIOATTR TABLE의 EQPT_WRK_STOP_FLAG = Y)
                    // 일시정지(17) 상태 -> 설비상태가 대기중이고(EIOSTAT = W) 해당 설비에 TRAY ID가 존재하고(CSTID != NULL) TRAY가 작업 중 일때 (WIPSTAT = PROC)
                    // 고온챔버 작업중인 상태(28) 상태 -> 설비상태가 작업중이고(EIOSTAT = R) 해당 설비에 TRAY ID가 존재하고(CSTID != NULL) TRAY가 작업 중 일때 (WIPSTAT = PROC)
                    // 설비가 작업중(챔버 안에 Tray가 존재한다면)이면 새로운 Tray 추가로 착공 안됨.

                    if (((!string.IsNullOrEmpty(dtRslt.Rows[0]["CSTID"].ToString()) && dtRslt.Rows[0]["FORMSTATUS"].ToString().Equals("17")) ||
                         (!string.IsNullOrEmpty(dtRslt.Rows[1]["CSTID"].ToString()) && dtRslt.Rows[1]["FORMSTATUS"].ToString().Equals("17")) ||
                         (!string.IsNullOrEmpty(dtRslt.Rows[2]["CSTID"].ToString()) && dtRslt.Rows[2]["FORMSTATUS"].ToString().Equals("17")) ||
                         (!string.IsNullOrEmpty(dtRslt.Rows[3]["CSTID"].ToString()) && dtRslt.Rows[3]["FORMSTATUS"].ToString().Equals("17")))
                         ||
                        (dtRslt.Rows[0]["FORMSTATUS"].ToString().Equals("28") ||
                         dtRslt.Rows[1]["FORMSTATUS"].ToString().Equals("28") ||
                         dtRslt.Rows[2]["FORMSTATUS"].ToString().Equals("28") ||
                         dtRslt.Rows[3]["FORMSTATUS"].ToString().Equals("28"))
                       )
                    //if (!string.IsNullOrEmpty(dtRslt.Rows[0]["CSTID"].ToString()) || !string.IsNullOrEmpty(dtRslt.Rows[1]["CSTID"].ToString()) || !string.IsNullOrEmpty(dtRslt.Rows[2]["CSTID"].ToString()) || !string.IsNullOrEmpty(dtRslt.Rows[3]["CSTID"].ToString()))
                    {
                        Util.MessageValidation("FM_ME_0578"); //작업중인 TRAY가 존재합니다.\n\n작업중에는 추가로 TRAY 투입이 불가능합니다.
                        return;
                    }

                    //if (!dtRslt.Rows[0]["FORMSTATUS"].ToString().Equals("11") && !dtRslt.Rows[1]["FORMSTATUS"].ToString().Equals("11") && !dtRslt.Rows[2]["FORMSTATUS"].ToString().Equals("11") && !dtRslt.Rows[3]["FORMSTATUS"].ToString().Equals("11")
                    //    )
                    //{
                    //    Util.MessageValidation("FM_ME_0571"); //입고 불가능한 챔버 입니다.
                    //    return;
                    //}

                    if (dtRslt.Rows.Count > 1)
                    {
                        if (cell != null)
                        {
                            FCS001_316_MANUAL_START HighChamberStartPopup = new FCS001_316_MANUAL_START();
                            HighChamberStartPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[2];

                            parameters[0] = EQPTID; //dtRslt.Rows[0]["EQPTID"].ToString()
                            parameters[1] = dtRslt.Rows[0]["EQPTNAME"].ToString();
                            //parameters[2] = CST_LOAD_LOCATION_CODE;

                            C1WindowExtension.SetParameters(HighChamberStartPopup, parameters);
                            HighChamberStartPopup.Closed += new EventHandler(HighChamberStartPopup_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => HighChamberStartPopup.ShowModal()));
                            HighChamberStartPopup.BringToFront();
                        }
                    }

                    else
                    {
                        Util.MessageValidation("FM_ME_0571"); //입고 불가능한 챔버 입니다.
                        return;
                    }
                }
            }
        }

        //고온챔버 수동출고 버튼
        private void btnChamberEnd_Click(object sender, RoutedEventArgs e)
        {

            if (dgFormation.Selection.SelectedCells.Count == 0)
            {
                Util.MessageValidation("FM_ME_0570"); // 챔버를 먼저 선택해주세요.
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgFormation.Selection.SelectedCells)
            {
                if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                {
                    Util.MessageValidation("FM_ME_0577"); //챔버 정보가 없습니다.
                    return;
                }
                else
                {
                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();

                    string CST_LOAD_LOCATION_CODE = Util.NVC(GetDtRowValue(ROWNUM, "CST_LOAD_LOCATION_CODE"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANE_ID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANE_ID"] = _LANEID;
                    dr["EQPTID"] = EQPTID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGHCHAMBER_VIEW_BY_LANE_HVF", "RQSTDT", "RSLTDT", dtRqst);

                    //if (((string.IsNullOrEmpty(dtRslt.Rows[0]["CSTID"].ToString()) && dtRslt.Rows[0]["FORMSTATUS"].ToString().Equals("17")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[1]["CSTID"].ToString()) && dtRslt.Rows[1]["FORMSTATUS"].ToString().Equals("17")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[2]["CSTID"].ToString()) && dtRslt.Rows[2]["FORMSTATUS"].ToString().Equals("17")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[3]["CSTID"].ToString()) && dtRslt.Rows[3]["FORMSTATUS"].ToString().Equals("17")))
                    //     ||
                    //    ((string.IsNullOrEmpty(dtRslt.Rows[0]["CSTID"].ToString()) && dtRslt.Rows[0]["FORMSTATUS"].ToString().Equals("28")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[1]["CSTID"].ToString()) && dtRslt.Rows[1]["FORMSTATUS"].ToString().Equals("28")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[2]["CSTID"].ToString()) && dtRslt.Rows[2]["FORMSTATUS"].ToString().Equals("28")) &&
                    //     (string.IsNullOrEmpty(dtRslt.Rows[3]["CSTID"].ToString()) && dtRslt.Rows[3]["FORMSTATUS"].ToString().Equals("28")))
                    //   )
                    if (string.IsNullOrEmpty(dtRslt.Rows[0]["CSTID"].ToString()) && string.IsNullOrEmpty(dtRslt.Rows[1]["CSTID"].ToString()) && string.IsNullOrEmpty(dtRslt.Rows[2]["CSTID"].ToString()) && string.IsNullOrEmpty(dtRslt.Rows[3]["CSTID"].ToString()))
                    {
                        Util.MessageValidation("FM_ME_0563");  //진행중인 TRAY가 존재하지 않습니다.
                        return;
                    }

                    if (dtRslt.Rows.Count > 1)
                    {
                        if (cell != null)
                        {
                            FCS001_316_MANUAL_END HighChamberEndPopup = new FCS001_316_MANUAL_END();
                            HighChamberEndPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[6];

                            parameters[0] = EQPTID; //dtRslt.Rows[0]["EQPTID"].ToString()
                            parameters[1] = dtRslt.Rows[0]["EQPTNAME"].ToString();
                            parameters[2] = dtRslt.Rows[0]["CSTID"].ToString().Trim();
                            parameters[3] = dtRslt.Rows[1]["CSTID"].ToString().Trim();
                            parameters[4] = dtRslt.Rows[2]["CSTID"].ToString().Trim();
                            parameters[5] = dtRslt.Rows[3]["CSTID"].ToString().Trim();


                            C1WindowExtension.SetParameters(HighChamberEndPopup, parameters);
                            HighChamberEndPopup.Closed += new EventHandler(HighChamberEndPopup_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => HighChamberEndPopup.ShowModal()));
                            HighChamberEndPopup.BringToFront();
                        }
                    }

                    else
                    {
                        Util.MessageValidation("FM_ME_0579"); //출고 가능한 상태가 아닙니다.
                        return;
                    }
                }
            }
        }

        private void HighChamberStartPopup_Closed(object sender, EventArgs e)
        {
            FCS001_316_MANUAL_START window = sender as FCS001_316_MANUAL_START;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetListFormation();
            }
            this.grdMain.Children.Remove(window);
        }

        private void HighChamberEndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_316_MANUAL_END window = sender as FCS001_316_MANUAL_END;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetListFormation();
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

    }
}

