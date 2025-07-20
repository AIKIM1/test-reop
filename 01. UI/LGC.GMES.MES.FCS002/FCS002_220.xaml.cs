/*************************************************************************************
 Created Date : 2023.05.26
      Creator : 
   Decription : IROCV 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.03.31  DEVELOPER : Initial Created.
 
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
    public partial class FCS002_220 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LANEID = string.Empty;
        private string _CELLTYPE = string.Empty;
        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private string _MENUID = string.Empty;
        private bool _GridSize = false;
        private double rowHeight = 0;

        private DataTable _dtNewData;
        
        private Point prevRowPos = new Point(0, 0);
        private Point prevColPos = new Point(0, 0);

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        Util Util = new Util();

        public FCS002_220()
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
                    _MENUID = LoginInfo.CFG_MENUID;

                sLaneID = GetLaneIDForMenu(_MENUID);

                if (string.IsNullOrEmpty(sLaneID))
                    _LANEID = "X11";
                else
                    _LANEID = sLaneID;

                switch (GetCellType())
                {
                    case "P":
                        SetTempGridMCP();
                        break;
                    case "N":
                        SetTempGridNFF();
                        break;
                    default: // D
                        //    SetTempGridMCC();
                        //    break;
                        break;
                }

                InitLegend();
                GeColorLegend();
                InitProc();

                rdoTrayId.Checked += rdo_CheckedChanged;
                rdoTrayId.Unchecked += rdo_CheckedChanged;
                rdoLotId.Checked += rdo_CheckedChanged;
                rdoLotId.Unchecked += rdo_CheckedChanged;
                rdoTime.Checked += rdo_CheckedChanged;
                rdoTime.Unchecked += rdo_CheckedChanged;

                rdoOpStart.Checked += rdo_CheckedChanged;
                rdoOpStart.Unchecked += rdo_CheckedChanged;
                rdoMinTemp.Checked += rdo_CheckedChanged;
                rdoMinTemp.Unchecked += rdo_CheckedChanged;
                rdoMaxTemp.Checked += rdo_CheckedChanged;
                rdoMaxTemp.Unchecked += rdo_CheckedChanged;

                btnSearch_Click(null, null);

                _timer.Tick += new EventHandler(timer_Tick);

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

        #region Event
        private void dgColorLegend_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name) == "CMCDNAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()) as SolidColorBrush;
                        }
                    }
                }

            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            ClearALL();
            GetFormationStatus();

            if (chkTimer.IsChecked == true)
            {
                _timer.Start();
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
                    if (e.Cell.Row.Index == 0 || e.Cell.Row.Index == _dtCopy.Rows.Count  || e.Cell.Column.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = Brushes.Black;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;

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
                    //dgFormation.GetCell((int)prevRowPos.X, (int)prevRowPos.Y).Presenter.Background = Brushes.LightGray;
                    //dgFormation.GetCell((int)prevColPos.X, (int)prevColPos.Y).Presenter.Background = Brushes.LightGray;

                    if (!(cell.Row.Index == 0) && !(cell.Row.Index == _dtCopy.Rows.Count / 2))
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
                        if (_CELLTYPE == "N")
                            GetTempNFF(EQPTID);  // 각 point Row로 출력
                        else
                            GetTempData(EQPTID); // 각 point 컬럼으로 출력

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
                        }
                        else
                        {
                            txtTroubleName.Text = string.Empty;
                            txtTroubleRepairWay.Text = string.Empty;
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
                    if (cell.Row.Index == 0 || cell.Row.Index == _dtCopy.Rows.Count  || cell.Column.Index == 0)
                    {
                        return;
                    }
                    //20220420_에러수정(입력 문자열의 형식이 잘못되었습니다.) END
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string ROW = Util.NVC(GetDtRowValue(ROWNUM, "ROW"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string EQPTNAME = Util.NVC(GetDtRowValue(ROWNUM, "EQPTNAME"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string LOTID = Util.NVC(GetDtRowValue(ROWNUM, "LOTID"));
                    string PRE_LOTID = Util.NVC(GetDtRowValue(ROWNUM, "PRE_LOTID"));
                    string LANE_ID = Util.NVC(GetDtRowValue(ROWNUM, "LANE_ID"));

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
                        FCS002_220_DETAIL wndRunStart = new FCS002_220_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = LANE_ID;
                            Parameters[5] = EQPTNAME;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
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
                            Parameters[4] = "I";
                            Parameters[5] = EQPTNAME;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }

                    }
                    else if (rdoPinCnt.IsChecked == true)
                    {
                        FCS002_223 wndPin = new FCS002_223();
                        wndPin.FrameOperation = FrameOperation;

                        if (wndPin != null)
                        {
                            object[] Parameters = new object[4];
                            Parameters[0] = "ORV";
                            Parameters[1] = _LANEID;
                            Parameters[2] = EQPTID;

                            C1WindowExtension.SetParameters(wndPin, Parameters);

                            wndPin.ShowModal();
                        }
                    }
                    //else if (rdoTempInfo.IsChecked == true)
                    //{
                    //    FCS002_212 wndRunStart = new FCS002_212();
                    //    wndRunStart.FrameOperation = FrameOperation;

                    //    if (wndRunStart != null)
                    //    {
                    //        object[] Parameters = new object[2];
                    //        Parameters[0] = _LANEID;
                    //        Parameters[1] = EQPTID;

                    //        this.FrameOperation.OpenMenu("SFU010715310", true, Parameters);
                    //    }
                    //}
                    //BOXMODEL 설정은 MMD BOX/MODEL 설정 화면으로 이관
                    //else if (rdoBoxModel.IsChecked == true)
                    //{
                    //    FCS002_200_CPF_BOXMODEL wndRunStart = new FCS002_200_CPF_BOXMODEL();
                    //    wndRunStart.FrameOperation = FrameOperation;

                    //    if (wndRunStart != null)
                    //    {
                    //        object[] Parameters = new object[5];
                    //        Parameters[0] = ROW;
                    //        Parameters[1] = COL;
                    //        Parameters[2] = STG;
                    //        Parameters[3] = EQPTID;

                    //        C1WindowExtension.SetParameters(wndRunStart, Parameters);

                    //        wndRunStart.ShowModal();
                    //    }
                    //}
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
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_OCV_MENU_ID";
                dr["COM_CODE"] = _MENUID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTR1"].ToString();
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
                cboColorLegend.Items.Add(cbItemTiTle);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_IROCVSTATUS_MCC";
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
                    cboColorLegend.Items.Add(cbItem);
                }

                cboColorLegend.SelectedIndex = 0;

                //-----------------------------------------------------
                //CommonCombo_Form _combo = new CommonCombo_Form();

                //string[] filter = { "1", _LANEID };
                //_combo.SetCombo(cboOperLegend, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "FORMATION_STATUS_NEXT_PROC" });

                //------------------------------------------------------

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

        private void GeColorLegend()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_IROCVSTATUS_MCC";

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTR_F_MB", "RQSTDT", "RSLTDT", dtRqst);

                SetColorGrid(dt);
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

        private void SetColorGrid(DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Color";
                textColumn1.Binding = new Binding("CMCDNAME");
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
                dgNew.LoadedCellPresenter += dgColorLegend_LoadedCellPresenter;
                dgNew.SelectedBackground = null;


                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                dgColor.Children.Add(dgNew);

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("CMCDNAME", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow drRow = dtRow.NewRow();
                drRow["CMCDNAME"] = dt.Rows[i]["CMCDNAME"];
                drRow["ATTRIBUTE1"] = dt.Rows[i]["ATTRIBUTE1"];
                drRow["ATTRIBUTE2"] = dt.Rows[i]["ATTRIBUTE2"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, false);
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
                dr["LANE_ID"] =_LANEID; 
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                _dtDATA = new ClientProxy().ExecuteServiceSync("DA_SEL_IROCV_VIEW_MB", "RQSTDT", "RSLTDT", dtRqst);

                //GetTestData(ref _dtDATA);
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
                dr["CMCDTYPE"] = "FORM_IROCV_STATUS_NEXT_PROC_MB";

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



        // 데이터 이상여부 확인
        private int CheckData(DataTable dt)
        {
            string PreEQPT = string.Empty;
            string EQPT = string.Empty;
            int Cnt = 0;
            string[] rows = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                EQPT = Util.NVC(dt.Rows[i]["EQPTID"].ToString());

                if (PreEQPT == EQPT)
                    Cnt++;
            }
            return Cnt;



        }

        #region MyRegion
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

                #region 충방전기 데이터용 변수
                string sStatus = string.Empty;
                TimeSpan tTimeSpan = new TimeSpan();
                bool bBold = false;

                // 중복데이터 갯수
                int TBLcnt = CheckData(dtRslt);

                string EQPTID = string.Empty;
                int iROW;
                int iCOL = 0;
                int iSTG;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string EIOSTAT = string.Empty;
                string EQP_OP_STATUS_CD = string.Empty;
                string RUN_MODE_CD = string.Empty;
                string PROCID = string.Empty;
                string PROCNAME = string.Empty;
                string NEXT_PROCID = string.Empty;
                //string RCV_ISS_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                //string EQPTIUSE = string.Empty;
                //string NEXT_PROCNAME = string.Empty;
                string PROD_LOTID = string.Empty;
                string JOB_TIME = string.Empty;
                //string ROUTID = string.Empty;
                string DUMMY_FLAG = string.Empty;
                //string SPECIAL_YN = string.Empty;
                //string LAST_RUN_TIME = string.Empty;
                //string JIG_AVG_TEMP = string.Empty;
                //string POW_AVG_TEMP = string.Empty;
                //string MEGA_DCHG_FUNC_YN = string.Empty;
                //string MEGA_CHG_FUNC_YN = string.Empty;
                string NOW_TIME = string.Empty;
                string PROC_DETL_TYPE_CODE = string.Empty;
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                //string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                string START_TIME = string.Empty;
                string MIN_TEMP = string.Empty;
                string MAX_TEMP = string.Empty;
                string TRANS_CSTID = string.Empty;
                string WIPSTAT = string.Empty;
                #endregion

                int iEQPcnt = 0;
                string sEQPTID = string.Empty;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {

                    if (sEQPTID != dtRslt.Rows[i]["EQPTID"].ToString())
                    {
                        sEQPTID = dtRslt.Rows[i]["EQPTID"].ToString();
                        iEQPcnt++;
                    }

                }

                string[] sEqpt = new string[iEQPcnt];
                int idx = 0;
                //충방전기 열 갯수 확인
                DataView view = dtRslt.DefaultView;
                DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
                List<string> rowList = new List<string>();
                foreach (DataRow dr in distRowTable.Rows)
                {
                    rowList.Add(dr["ROW"].ToString());
                  
                }

                sEQPTID = string.Empty;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (sEQPTID != dtRslt.Rows[i]["EQPTID"].ToString())
                    {
                        sEqpt[idx] = dtRslt.Rows[i]["EQPTID"].ToString();
                        sEQPTID = sEqpt[idx];
                        idx++;
                    }
                }
                #region Grid 초기화
                iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
                iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

                //iRowCount = iMaxStg * 2;
                iRowCount = 1;// iMaxStg;
                iColumnCount = (sEqpt.Length + 1) ;   //단 추가.

                //Column Width get
                dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1)/2;
                if (rowHeight == 0)
                    rowHeight = dgFormation.ActualHeight;
                //Row Height get
                if (iRowCount == 1)
                    dRowHeight = 65;
                else
                    dRowHeight = rowHeight / (iRowCount * 2) / 4 - 1.3;
                //dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2) / 4 - 1.3;

                //상단 Datatable, 하단 Datatable 정의
                DataTable Udt = new DataTable();
         //     DataTable Ddt = new DataTable();

                //Column 생성
                for (int i = 0; i < iColumnCount; i++)
                {
                    //GRID Column Create
                    if (i == 0)
                        SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50);
                    else
                        SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth);

                    Udt.Columns.Add((i + 1).ToString(), typeof(string));
       //           Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for (int i = 0; i < iRowCount; i++)
                {
                    DataRow Urow = Udt.NewRow();
                    Udt.Rows.Add(Urow);
       //           DataRow Drow = Ddt.NewRow();
       //           Ddt.Rows.Add(Drow);
                }

                //Row Header 생성
                int iMakeCol = 0;
                idx = 0;
                DataRow UrowHeader = Udt.NewRow();
                for (int i = 0; i < Udt.Columns.Count; i++)
                {
                    if (i == 0)
                        UrowHeader[i] = string.Empty;
                    else
                    {
                        iMakeCol++;

                        DataRow[] drRow = dtRslt.Select("EQPTID = '" + sEqpt[idx].ToString() + "'");
                       UrowHeader[i] = ObjectDic.Instance.GetObjectName("[*]"+drRow[0]["EQPTNAME"].ToString());
                        idx++;
                    }
                }

                //Row Header 생성
                //DataRow DrowHeader = Ddt.NewRow();
                //for (int i = 0; i < Ddt.Columns.Count; i++)
                //{
                //    if (i == 0)
                //        DrowHeader[i] = string.Empty;
                //    else
                //    {
                //        //DrowHeader[i] = (i + Ddt.Columns.Count - 1).ToString() + ObjectDic.Instance.GetObjectName("연");
                //        iMakeCol++;

                //        // DataRow[] drRow = dtRslt.Select("STG = '01' AND COL = '" + iMakeCol.ToString("00") + "'");
                //        DataRow[] drRow = dtRslt.Select("EQPTID = '" + sEqpt[idx].ToString() + "'");
                //        DrowHeader[i] =  (idx+1) + ObjectDic.Instance.GetObjectName("호 OCV");
                //        idx++; 
                //    }
                //}
                #endregion

                #region Data Setting
                //dtRslt Column Add
                dtRslt.Columns.Add("BCOLOR", typeof(string));
                dtRslt.Columns.Add("FCOLOR", typeof(string));
                dtRslt.Columns.Add("TEXT", typeof(string));
                dtRslt.Columns.Add("BOLD", typeof(string));

                TBLcnt = 0;
                string PreEQPTID = string.Empty;
                for (int k = 0; k < dtRslt.Rows.Count; k++)
                {
                    #region MyRegion



                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;
                    sStatus = string.Empty;

                    EQPTID = Util.NVC(dtRslt.Rows[k]["EQPTID"].ToString());

                    if (PreEQPTID == EQPTID && k != 0)
                    {
                        TBLcnt++;

                        // 데이터 중복시 정보이상처리
                        FORMSTATUS = "0";

                        DataRow[] drColor1 = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                        if (drColor1.Length > 0)
                        {
                            BCOLOR = drColor1[0]["ATTRIBUTE1"].ToString();
                            FCOLOR = drColor1[0]["ATTRIBUTE2"].ToString();
                        }
                        dtRslt.Rows[k - 1]["BCOLOR"] = BCOLOR;
                        dtRslt.Rows[k - 1]["FCOLOR"] = FCOLOR;
                        dtRslt.Rows[k - 1]["TEXT"] = ObjectDic.Instance.GetObjectName("MCS 정보이상");
                        dtRslt.Rows[k - 1]["BOLD"] = "Y";

                        PreEQPTID = EQPTID;

                        continue;
                    }


                    //열연단 구분X
                    iROW = 1;//int.Parse(dtRslt.Rows[k]["ROW"].ToString());
                    iCOL = iCOL + 1;//int.Parse(dtRslt.Rows[k]["COL"].ToString());
                    iSTG = 1;// int.Parse(dtRslt.Rows[k]["STG"].ToString());

                    CSTID = Util.NVC(dtRslt.Rows[k]["CSTID"].ToString());
                    //LOTID = Util.NVC(dtRslt.Rows[k]["LOTID"].ToString());
                    EIOSTAT = Util.NVC(dtRslt.Rows[k]["EIOSTAT"].ToString());
                    EQP_OP_STATUS_CD = Util.NVC(dtRslt.Rows[k]["EQP_OP_STATUS_CD"].ToString());
                    RUN_MODE_CD = Util.NVC(dtRslt.Rows[k]["RUN_MODE_CD"].ToString());
                    PROCID = Util.NVC(dtRslt.Rows[k]["PROCID"].ToString());
                    PROCNAME = Util.NVC(dtRslt.Rows[k]["PROCNAME"].ToString());
                    NEXT_PROCID = Util.NVC(dtRslt.Rows[k]["NEXT_PROCID"].ToString()); //임시
                                                                                      //RCV_ISS_ID = Util.NVC(dtRslt.Rows[k]["RCV_ISS_ID"].ToString());
                    FORMSTATUS = Util.NVC(dtRslt.Rows[k]["FORMSTATUS"].ToString());
                    //EQPTIUSE = Util.NVC(dtRslt.Rows[k]["EQPTIUSE"].ToString());
                    //NEXT_PROCNAME = Util.NVC(dtRslt.Rows[k]["NEXT_PROCNAME"].ToString());
                    PROD_LOTID = Util.NVC(dtRslt.Rows[k]["PROD_LOTID"].ToString());
                    JOB_TIME = Util.NVC(dtRslt.Rows[k]["JOB_TIME"].ToString());
                    //ROUTID = Util.NVC(dtRslt.Rows[k]["ROUTID"].ToString());
                    DUMMY_FLAG = Util.NVC(dtRslt.Rows[k]["DUMMY_FLAG"].ToString());
                    //SPECIAL_YN = Util.NVC(dtRslt.Rows[k]["SPECIAL_YN"].ToString());
                    //LAST_RUN_TIME = Util.NVC(dtRslt.Rows[k]["LAST_RUN_TIME"].ToString());
                    //JIG_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["JIG_AVG_TEMP"].ToString());
                    //POW_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["POW_AVG_TEMP"].ToString());
                    //MEGA_DCHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_DCHG_FUNC_YN"].ToString());
                    //MEGA_CHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_CHG_FUNC_YN"].ToString());
                    NOW_TIME = Util.NVC(dtRslt.Rows[k]["NOW_TIME"].ToString());
                    PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["PROC_DETL_TYPE_CODE"].ToString());
                    NEXT_PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["NEXT_PROC_DETL_TYPE_CODE"].ToString());
                    //ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가

                    START_TIME = Util.NVC(dtRslt.Rows[k]["OP_START_TIME"].ToString());
                    MIN_TEMP = Util.NVC(dtRslt.Rows[k]["MIN_TEMP"].ToString());
                    MAX_TEMP = Util.NVC(dtRslt.Rows[k]["MAX_TEMP"].ToString());
                    TRANS_CSTID = Util.NVC(dtRslt.Rows[k]["TRANS_CST"].ToString());
                    WIPSTAT = Util.NVC(dtRslt.Rows[k]["WIPSTAT"].ToString());
                    #endregion

                    Udt.Rows[iRowCount - iSTG][iCOL] = k.ToString();

                    #region MyRegion
                  
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
                            sStatus = PROD_LOTID;
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
                        else if (rdoMaxTemp.IsChecked == true)
                        {
                            sStatus = (string.IsNullOrEmpty(MAX_TEMP) ? string.Empty : double.Parse(MAX_TEMP).ToString("00.0"));
                        }
                        else if (rdoMinTemp.IsChecked == true)
                        {
                            sStatus = (string.IsNullOrEmpty(MIN_TEMP) ? string.Empty : double.Parse(MIN_TEMP).ToString("00.0"));
                        }


                        if (DUMMY_FLAG.Equals("Y"))
                            FCOLOR = (BCOLOR == "Blue") ? "RoyalBlue" : "Blue";

                    }

                    switch (FORMSTATUS)
                    {
                        case "01": // 통신두절
                            sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                            break;
                        case "10": //예약요청

                            if (rdoTime.IsChecked == true)
                                sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                            else
                                sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + TRANS_CSTID;
                            break;
                        case "19": //Power Off
                            sStatus = "Power Off";
                            break;
                        case "21": //정비중
                            BCOLOR = "White";
                            FCOLOR = "Black";
                            sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")"; //200611 KJE : 정비중 시간 추가
                            break;
                        case "25": //수리중
                            BCOLOR = "White";
                            FCOLOR = "Black";
                            //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
                            sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")";
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
                            //수동
                            sStatus = ObjectDic.Instance.GetObjectName("MANUAL");
                            break;
                    }
                    if (!string.IsNullOrEmpty(CSTID) &&
                        DBNull.Value != dtRslt.Rows[k]["EQPT_CTRL_STAT_CODE"] &&
                        dtRslt.Rows[k]["EQPT_CTRL_STAT_CODE"].ToString() == string.Empty)
                           sStatus = ObjectDic.Instance.GetObjectName("작업 제어 거부");

                    dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
                    dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
                    dtRslt.Rows[k]["TEXT"] = sStatus;
                    dtRslt.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";


                    PreEQPTID = EQPTID;
                    #endregion
                }
                #endregion


                PreEQPTID = EQPTID;

                #region Grid 조합
                //DataTable Header Insert
                Udt.Rows.InsertAt(UrowHeader, 0);
                //Ddt.Rows.InsertAt(DrowHeader, 0);

                //상,하 Merge
                //Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                _dtCopy = Udt.Copy();

                dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                dgFormation.ItemsSource = DataTableConverter.Convert(Udt);
                dgFormation.UpdateLayout();

                Util.GridSetData(dgFormation, Udt, FrameOperation, true);

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgFormation.Columns)
                    dgc.VerticalAlignment = VerticalAlignment.Center;

                string[] sColumnName = new string[] { "1" };
                Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                #endregion
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        //private void SetFpsFormationDataAllRow(DataTable dtRslt)
        //{
        //    try
        //    {
        //        // 스프레드 초기화용 변수
        //        #region 스프레드 초기화용 변수
        //        int iMaxCol;
        //        int iMaxStg;
        //        int iRowCount;
        //        int iColumnCount;
        //        double dColumnWidth;
        //        double dRowHeight;
        //        int tempCol = 0;
        //        int tempStg = 0;
        //        #endregion

        //        // 충방전기 데이터용 변수
        //        #region 충방전기 데이터용 변수
        //        string sStatus = string.Empty;
        //        TimeSpan tTimeSpan = new TimeSpan();
        //        bool bBold = false;

        //        string EQPTID = string.Empty;
        //        int iROW;
        //        int iCOL;
        //        int iSTG;
        //        int iCST_LOAD_LOCATION_CODE;
        //        string CSTID = string.Empty;
        //        string LOTID = string.Empty;
        //        string EIOSTAT = string.Empty;
        //        string EQP_OP_STATUS_CD = string.Empty;
        //        string RUN_MODE_CD = string.Empty;
        //        string PROCID = string.Empty;
        //        string PROCNAME = string.Empty;
        //        string NEXT_PROCID = string.Empty;
        //        string RCV_ISS_ID = string.Empty;
        //        string FORMSTATUS = string.Empty;
        //        string EQPTIUSE = string.Empty;
        //        string NEXT_PROCNAME = string.Empty;
        //        string PROD_LOTID = string.Empty;
        //        string JOB_TIME = string.Empty;
        //        string ROUTID = string.Empty;
        //        string DUMMY_FLAG = string.Empty;
        //        string SPECIAL_YN = string.Empty;
        //        string LAST_RUN_TIME = string.Empty;
        //        string JIG_AVG_TEMP = string.Empty;
        //        string POW_AVG_TEMP = string.Empty;
        //        string MEGA_DCHG_FUNC_YN = string.Empty;
        //        string MEGA_CHG_FUNC_YN = string.Empty;
        //        string NOW_TIME = string.Empty;
        //        string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
        //        string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
        //        #endregion

        //        //충방전기 열 갯수 확인
        //        DataView view = dtRslt.DefaultView;
        //        DataTable distRowTable = view.ToTable(true, new string[] { "ROW" });
        //        List<string> rowList = new List<string>();
        //        foreach (DataRow dr in distRowTable.Rows)
        //        {
        //            rowList.Add(dr["ROW"].ToString());
        //        }

        //        // 충방전기 Row 갯수에 따라 로직을 나눔
        //        if (rowList.Count > 1)
        //        {
        //            #region Grid 초기화
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", "ROW = " + rowList[row]));
        //                iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", "ROW = " + rowList[row]));

        //                if (iMaxCol > tempCol) tempCol = iMaxCol;
        //                if (iMaxStg > tempStg) tempStg = iMaxStg;
        //            }

        //            iRowCount = tempStg * 2;
        //            iColumnCount = tempCol + 1;

        //            //Grid Column Width get
        //            dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

        //            //Grid Row Height get
        //            //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
        //            dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2) - 1.3;

        //            //충방전기 Row별 Datatable 정의
        //            List<DataTable> dtList = new List<DataTable>();

        //            //Grid Column 생성
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                DataTable dt = new DataTable();

        //                for (int i = 0; i < iColumnCount; i++)
        //                {
        //                    if (row == 0)
        //                    {
        //                        //GRID Column Create
        //                        if (i == 0)
        //                            SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50);
        //                        else
        //                            SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth);
        //                    }

        //                    dt.Columns.Add((i + 1).ToString(), typeof(string));
        //                }

        //                dtList.Add(dt);
        //            }

        //            //Grid Row 생성
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                for (int i = 0; i < iRowCount; i++)
        //                {
        //                    DataRow drRow = dtList[row].NewRow();
        //                    dtList[row].Rows.Add(drRow);
        //                }

        //            }

        //            //Grid Row Header 생성
        //            List<DataRow> drListHeader = new List<DataRow>();
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                DataRow drRowHeader = dtList[row].NewRow();
        //                for (int i = 0; i < dtList[row].Columns.Count; i++)
        //                {
        //                    if (i == 0)
        //                        drRowHeader[i] = int.Parse(rowList[row]).ToString() + ObjectDic.Instance.GetObjectName("열");
        //                    else if (i == 1)
        //                        drRowHeader[i] = "[" + rowList[row] + "열]  " + (i).ToString() + ObjectDic.Instance.GetObjectName("연");
        //                    else
        //                        drRowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
        //                }
        //                drListHeader.Add(drRowHeader);
        //            }
        //            #endregion

        //            #region Data Setting
        //            //dtRslt Column Add
        //            dtRslt.Columns.Add("BCOLOR", typeof(string));
        //            dtRslt.Columns.Add("FCOLOR", typeof(string));
        //            dtRslt.Columns.Add("TEXT", typeof(string));
        //            dtRslt.Columns.Add("BOLD", typeof(string));

        //            for (int k = 0; k < dtRslt.Rows.Count; k++)
        //            {
        //                iROW = int.Parse(dtRslt.Rows[k]["ROW"].ToString());
        //                iCOL = int.Parse(dtRslt.Rows[k]["COL"].ToString());
        //                iSTG = int.Parse(dtRslt.Rows[k]["STG"].ToString());
        //                iCST_LOAD_LOCATION_CODE = int.Parse(dtRslt.Rows[k]["CST_LOAD_LOCATION_CODE"].ToString());

        //                EQPTID = Util.NVC(dtRslt.Rows[k]["EQPTID"].ToString());
        //                CSTID = Util.NVC(dtRslt.Rows[k]["CSTID"].ToString());
        //                LOTID = Util.NVC(dtRslt.Rows[k]["LOTID"].ToString());
        //                EIOSTAT = Util.NVC(dtRslt.Rows[k]["EIOSTAT"].ToString());
        //                EQP_OP_STATUS_CD = Util.NVC(dtRslt.Rows[k]["EQP_OP_STATUS_CD"].ToString());
        //                RUN_MODE_CD = Util.NVC(dtRslt.Rows[k]["RUN_MODE_CD"].ToString());
        //                PROCID = Util.NVC(dtRslt.Rows[k]["PROCID"].ToString());
        //                PROCNAME = Util.NVC(dtRslt.Rows[k]["PROCNAME"].ToString());
        //                NEXT_PROCID = Util.NVC(dtRslt.Rows[k]["NEXT_PROCID"].ToString()); //임시
        //                RCV_ISS_ID = Util.NVC(dtRslt.Rows[k]["RCV_ISS_ID"].ToString());
        //                FORMSTATUS = Util.NVC(dtRslt.Rows[k]["FORMSTATUS"].ToString());
        //                EQPTIUSE = Util.NVC(dtRslt.Rows[k]["EQPTIUSE"].ToString());
        //                NEXT_PROCNAME = Util.NVC(dtRslt.Rows[k]["NEXT_PROCNAME"].ToString());
        //                PROD_LOTID = Util.NVC(dtRslt.Rows[k]["PROD_LOTID"].ToString());
        //                JOB_TIME = Util.NVC(dtRslt.Rows[k]["JOB_TIME"].ToString());
        //                ROUTID = Util.NVC(dtRslt.Rows[k]["ROUTID"].ToString());
        //                DUMMY_FLAG = Util.NVC(dtRslt.Rows[k]["DUMMY_FLAG"].ToString());
        //                SPECIAL_YN = Util.NVC(dtRslt.Rows[k]["SPECIAL_YN"].ToString());
        //                LAST_RUN_TIME = Util.NVC(dtRslt.Rows[k]["LAST_RUN_TIME"].ToString());
        //                JIG_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["JIG_AVG_TEMP"].ToString());
        //                POW_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["POW_AVG_TEMP"].ToString());
        //                MEGA_DCHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_DCHG_FUNC_YN"].ToString());
        //                MEGA_CHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_CHG_FUNC_YN"].ToString());
        //                NOW_TIME = Util.NVC(dtRslt.Rows[k]["NOW_TIME"].ToString());
        //                NEXT_PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["NEXT_PROC_DETL_TYPE_CODE"].ToString());
        //                ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가

        //                for (int row = 0; row < rowList.Count; row++)
        //                {
        //                    if (int.Parse(rowList[row]) == iROW)
        //                    {
        //                        if (iCST_LOAD_LOCATION_CODE > 1)
        //                        {
        //                            dtList[row].Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                            dtList[row].Rows[iRowCount - (iSTG * 2)][iCOL] = k.ToString();
        //                        }
        //                        else
        //                        {
        //                            dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                            dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL] = k.ToString();
        //                        }

        //                    }
        //                }

        //                #region MyRegion
        //                string BCOLOR = "Black";
        //                string FCOLOR = "White";
        //                string TEXT = string.Empty;
        //                sStatus = string.Empty;

        //                DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

        //                if (drColor.Length > 0)
        //                {
        //                    BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
        //                    FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
        //                }

        //                if (string.IsNullOrEmpty(CSTID))
        //                {
        //                    sStatus = string.Empty;

        //                    if (rdoTime.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
        //                    {
        //                        if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
        //                        {
        //                            sStatus = "T )";
        //                            sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (rdoTrayId.IsChecked == true)
        //                    {
        //                        sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
        //                    }
        //                    else if (rdoLotId.IsChecked == true)
        //                    {
        //                        //20211018 Auto Calibration Lot표시 추가 START
        //                        //sStatus = PROD_LOTID;
        //                        if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE))
        //                        {
        //                            sStatus = PROD_LOTID + " [Auto Calib]";
        //                        }
        //                        else
        //                        {
        //                            sStatus = PROD_LOTID;
        //                        }
        //                        //20211018 Auto Calibration Lot표시 추가 END
        //                    }
        //                    else if (rdoTime.IsChecked == true)
        //                    {
        //                        if (!string.IsNullOrEmpty(JOB_TIME))
        //                        {
        //                            tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
        //                            sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
        //                            sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
        //                        }
        //                    }

        //                    if (DUMMY_FLAG.Equals("Y"))
        //                        FCOLOR = (BCOLOR == "Blue") ? "RoyalBlue" : "Blue";

        //                    if (SPECIAL_YN.Equals("Y"))
        //                    {
        //                        FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
        //                        bBold = false;
        //                    }
        //                    else if (SPECIAL_YN.Equals("I"))
        //                    {
        //                        FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
        //                        bBold = true;
        //                    }
        //                }

        //                switch (FORMSTATUS)
        //                {
        //                    case "01": // 통신두절
        //                        sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
        //                        break;
        //                    case "10": //예약요청
        //                        tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

        //                        if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
        //                            BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

        //                        if (rdoTime.IsChecked == true)
        //                            sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
        //                        else
        //                            sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + CSTID;
        //                        break;
        //                    case "19": //Power Off
        //                        sStatus = "Power Off";
        //                        break;
        //                    case "21": //정비중
        //                        sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
        //                        break;
        //                    case "25": //수리중
        //                               //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
        //                        sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + getMaintName(EQPTID, LAST_RUN_TIME);
        //                        break;
        //                    case "22": //사용안함
        //                        sStatus = ObjectDic.Instance.GetObjectName("USE_N");
        //                        break;
        //                    case "27": //화재
        //                        sStatus = ObjectDic.Instance.GetObjectName("FIRE");
        //                        break;
        //                    case "": //MANUAL - NULL
        //                        BCOLOR = "White";
        //                        FCOLOR = "Black";
        //                        
        //                          수동
        //                          sStatus = ObjectDic.Instance.GetObjectName("MANUAL");

        //                        break;
        //                }

        //                dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
        //                dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
        //                dtRslt.Rows[k]["TEXT"] = sStatus;
        //                dtRslt.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";
        //                #endregion

        //            }
        //            #endregion

        //            #region Grid 조합
        //            //DataTable Header Insert
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                dtList[row].Rows.InsertAt(drListHeader[row], 0);
        //            }

        //            //상,하 Merge
        //            DataTable dtTotal = new DataTable();
        //            for (int row = 0; row < rowList.Count; row++)
        //            {
        //                dtTotal.Merge(dtList[row], false, MissingSchemaAction.Add);
        //            }
        //            _dtCopy = dtTotal.Copy();

        //            dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
        //            dgFormation.ItemsSource = DataTableConverter.Convert(dtTotal);
        //            dgFormation.UpdateLayout();

        //            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgFormation.Columns)
        //                dgc.VerticalAlignment = VerticalAlignment.Center;

        //            string[] sColumnName = new string[] { "1" };
        //            Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        //            #endregion

        //        }
        //        else
        //        {
        //            #region Grid 초기화
        //            iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
        //            iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

        //            iRowCount = iMaxStg * 2;
        //            iColumnCount = (iMaxCol + 1) / 2 + 1;   //단 추가.

        //            //Column Width get
        //            dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

        //            //Row Height get
        //            //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
        //            dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2) - 1.3;

        //            //상단 Datatable, 하단 Datatable 정의
        //            DataTable Udt = new DataTable();
        //            DataTable Ddt = new DataTable();

        //            //Column 생성
        //            for (int i = 0; i < iColumnCount; i++)
        //            {
        //                //GRID Column Create
        //                if (i == 0)
        //                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50);
        //                else
        //                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth);

        //                Udt.Columns.Add((i + 1).ToString(), typeof(string));
        //                Ddt.Columns.Add((i + 1).ToString(), typeof(string));
        //            }

        //            //Row 생성
        //            for (int i = 0; i < iRowCount; i++)
        //            {
        //                DataRow Urow = Udt.NewRow();
        //                Udt.Rows.Add(Urow);

        //                DataRow Drow = Ddt.NewRow();
        //                Ddt.Rows.Add(Drow);
        //            }

        //            //Row Header 생성
        //            DataRow UrowHeader = Udt.NewRow();
        //            for (int i = 0; i < Udt.Columns.Count; i++)
        //            {
        //                if (i == 0)
        //                    UrowHeader[i] = string.Empty;
        //                else
        //                    UrowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
        //            }

        //            //Row Header 생성
        //            DataRow DrowHeader = Ddt.NewRow();
        //            for (int i = 0; i < Ddt.Columns.Count; i++)
        //            {
        //                if (i == 0)
        //                    DrowHeader[i] = string.Empty;
        //                else
        //                    DrowHeader[i] = (i + Ddt.Columns.Count - 1).ToString() + ObjectDic.Instance.GetObjectName("연");
        //            }
        //            #endregion

        //            #region Data Setting
        //            //dtRslt Column Add
        //            dtRslt.Columns.Add("BCOLOR", typeof(string));
        //            dtRslt.Columns.Add("FCOLOR", typeof(string));
        //            dtRslt.Columns.Add("TEXT", typeof(string));
        //            dtRslt.Columns.Add("BOLD", typeof(string));

        //            for (int k = 0; k < dtRslt.Rows.Count; k++)
        //            {
        //                iROW = int.Parse(dtRslt.Rows[k]["ROW"].ToString());
        //                iCOL = int.Parse(dtRslt.Rows[k]["COL"].ToString());
        //                iSTG = int.Parse(dtRslt.Rows[k]["STG"].ToString());
        //                iCST_LOAD_LOCATION_CODE = int.Parse(dtRslt.Rows[k]["CST_LOAD_LOCATION_CODE"].ToString());

        //                EQPTID = Util.NVC(dtRslt.Rows[k]["EQPTID"].ToString());
        //                CSTID = Util.NVC(dtRslt.Rows[k]["CSTID"].ToString());
        //                LOTID = Util.NVC(dtRslt.Rows[k]["LOTID"].ToString());
        //                EIOSTAT = Util.NVC(dtRslt.Rows[k]["EIOSTAT"].ToString());
        //                EQP_OP_STATUS_CD = Util.NVC(dtRslt.Rows[k]["EQP_OP_STATUS_CD"].ToString());
        //                RUN_MODE_CD = Util.NVC(dtRslt.Rows[k]["RUN_MODE_CD"].ToString());
        //                PROCID = Util.NVC(dtRslt.Rows[k]["PROCID"].ToString());
        //                PROCNAME = Util.NVC(dtRslt.Rows[k]["PROCNAME"].ToString());
        //                NEXT_PROCID = Util.NVC(dtRslt.Rows[k]["NEXT_PROCID"].ToString()); //임시
        //                RCV_ISS_ID = Util.NVC(dtRslt.Rows[k]["RCV_ISS_ID"].ToString());
        //                FORMSTATUS = Util.NVC(dtRslt.Rows[k]["FORMSTATUS"].ToString());
        //                EQPTIUSE = Util.NVC(dtRslt.Rows[k]["EQPTIUSE"].ToString());
        //                NEXT_PROCNAME = Util.NVC(dtRslt.Rows[k]["NEXT_PROCNAME"].ToString());
        //                PROD_LOTID = Util.NVC(dtRslt.Rows[k]["PROD_LOTID"].ToString());
        //                JOB_TIME = Util.NVC(dtRslt.Rows[k]["JOB_TIME"].ToString());
        //                ROUTID = Util.NVC(dtRslt.Rows[k]["ROUTID"].ToString());
        //                DUMMY_FLAG = Util.NVC(dtRslt.Rows[k]["DUMMY_FLAG"].ToString());
        //                SPECIAL_YN = Util.NVC(dtRslt.Rows[k]["SPECIAL_YN"].ToString());
        //                LAST_RUN_TIME = Util.NVC(dtRslt.Rows[k]["LAST_RUN_TIME"].ToString());
        //                JIG_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["JIG_AVG_TEMP"].ToString());
        //                POW_AVG_TEMP = Util.NVC(dtRslt.Rows[k]["POW_AVG_TEMP"].ToString());
        //                MEGA_DCHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_DCHG_FUNC_YN"].ToString());
        //                MEGA_CHG_FUNC_YN = Util.NVC(dtRslt.Rows[k]["MEGA_CHG_FUNC_YN"].ToString());
        //                NOW_TIME = Util.NVC(dtRslt.Rows[k]["NOW_TIME"].ToString());
        //                NEXT_PROC_DETL_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["NEXT_PROC_DETL_TYPE_CODE"].ToString());
        //                ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[k]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가

        //                if (iCOL <= iColumnCount - 1)
        //                {
        //                    if (iCST_LOAD_LOCATION_CODE > 1)
        //                    {
        //                        Udt.Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                        Udt.Rows[iRowCount - (iSTG * 2)][iCOL] = k.ToString();
        //                    }
        //                    else
        //                    {
        //                        Udt.Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                        Udt.Rows[iRowCount - (iSTG * 2) + 1][iCOL] = k.ToString();
        //                    }
        //                }
        //                else
        //                {
        //                    if (iCST_LOAD_LOCATION_CODE > 1)
        //                    {
        //                        Ddt.Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                        Ddt.Rows[iRowCount - (iSTG * 2)][iCOL - iColumnCount + 1] = k.ToString();
        //                    }
        //                    else
        //                    {
        //                        Ddt.Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
        //                        Ddt.Rows[iRowCount - (iSTG * 2) + 1][iCOL - iColumnCount + 1] = k.ToString();
        //                    }
        //                }

        //                #region MyRegion
        //                string BCOLOR = "Black";
        //                string FCOLOR = "White";
        //                string TEXT = string.Empty;
        //                sStatus = string.Empty;

        //                DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

        //                if (drColor.Length > 0)
        //                {
        //                    BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
        //                    FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
        //                }

        //                if (string.IsNullOrEmpty(CSTID))
        //                {
        //                    sStatus = string.Empty;

        //                    if (rdoTime.IsChecked == true && !string.IsNullOrEmpty(JOB_TIME))
        //                    {
        //                        if (EIOSTAT.Equals("T") || EIOSTAT.Equals("S"))
        //                        {
        //                            sStatus = "T )";
        //                            sStatus = sStatus + (DateTime.Parse(JOB_TIME)).ToString("MM-dd HH:mm");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (rdoTrayId.IsChecked == true)
        //                    {
        //                        sStatus = CSTID + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
        //                    }
        //                    else if (rdoLotId.IsChecked == true)
        //                    {
        //                        //20211018 Auto Calibration Lot표시 추가 START
        //                        //sStatus = PROD_LOTID;
        //                        if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE))
        //                        {
        //                            sStatus = PROD_LOTID + " [Auto Calib]";
        //                        }
        //                        else
        //                        {
        //                            sStatus = PROD_LOTID;
        //                        }
        //                        //20211018 Auto Calibration Lot표시 추가 END
        //                    }
        //                    else if (rdoTime.IsChecked == true)
        //                    {
        //                        if (!string.IsNullOrEmpty(JOB_TIME))
        //                        {
        //                            tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(JOB_TIME);
        //                            sStatus = tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
        //                            sStatus = sStatus + " [" + NEXT_PROC_DETL_TYPE_CODE + "]";
        //                        }
        //                    }

        //                    if (DUMMY_FLAG.Equals("Y"))
        //                        FCOLOR = (BCOLOR == "Blue") ? "RoyalBlue" : "Blue";

        //                    if (SPECIAL_YN.Equals("Y"))
        //                    {
        //                        FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
        //                        bBold = false;
        //                    }
        //                    else if (SPECIAL_YN.Equals("I"))
        //                    {
        //                        FCOLOR = (BCOLOR == "Red") ? "Crimson" : "Red";
        //                        bBold = true;
        //                    }
        //                }

        //                switch (FORMSTATUS)
        //                {
        //                    case "01": // 통신두절
        //                        sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
        //                        break;
        //                    case "10": //예약요청
        //                        tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

        //                        if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
        //                            BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

        //                        if (rdoTime.IsChecked == true)
        //                            sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
        //                        else
        //                            sStatus = ObjectDic.Instance.GetObjectName("RESV") + ")" + CSTID;
        //                        break;
        //                    case "19": //Power Off
        //                        sStatus = "Power Off";
        //                        break;
        //                    case "21": //정비중
        //                        sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
        //                        break;
        //                    case "25": //수리중
        //                               //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
        //                        sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + getMaintName(EQPTID, LAST_RUN_TIME);
        //                        break;
        //                    case "22": //사용안함
        //                        sStatus = ObjectDic.Instance.GetObjectName("USE_N");
        //                        break;
        //                    case "27": //화재
        //                        sStatus = ObjectDic.Instance.GetObjectName("FIRE");
        //                        break;
        //                    case "": //MANUAL - NULL
        //                        BCOLOR = "White";
        //                        FCOLOR = "Black";
        //                        
        //                      수동
        //                      sStatus = ObjectDic.Instance.GetObjectName("MANUAL"); 
        //                        break;
        //                }

        //                dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
        //                dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
        //                dtRslt.Rows[k]["TEXT"] = sStatus;
        //                dtRslt.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";
        //                #endregion
        //            }
        //            #endregion

        //            #region Grid 조합
        //            //DataTable Header Insert
        //            Udt.Rows.InsertAt(UrowHeader, 0);
        //            Ddt.Rows.InsertAt(DrowHeader, 0);

        //            //상,하 Merge
        //            Udt.Merge(Ddt, false, MissingSchemaAction.Add);
        //            _dtCopy = Udt.Copy();

        //            dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
        //            dgFormation.ItemsSource = DataTableConverter.Convert(Udt);
        //            dgFormation.UpdateLayout();

        //            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgFormation.Columns)
        //                dgc.VerticalAlignment = VerticalAlignment.Center;

        //            string[] sColumnName = new string[] { "1" };
        //            Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        //            #endregion
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

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
                dgTemp.ItemsSource = null;
                //dgTemp.Columns.Clear();
                dgTemp.Refresh();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("TMPR_PSTN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqpId;
                dr["TMPR_PSTN"] = "1";  //기구부
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CPF_TEMP_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return;


                int PointCnt = 0;
                float TempTotal = 0;
                float temp = 0;
                for (int i = 8; i < dtRslt.Columns.Count; i++)
                {
                    //PointCnt++;
                    //dgTemp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    //{
                    //    Name = PointCnt.ToString()+"_Point",
                    //    Header = PointCnt.ToString() + "_Point",
                    //    Binding = new Binding()
                    //    {
                    //        Path = new PropertyPath(dtRslt.Columns[i].ColumnName.ToString()),
                    //        Mode = BindingMode.TwoWay
                    //    },
                    //    TextWrapping = TextWrapping.Wrap,
                    //    IsReadOnly = true,
                    //});
                    if (!string.IsNullOrEmpty(dtRslt.Rows[0][i].ToString()))
                    {
                        PointCnt++;
                        temp = float.Parse(dtRslt.Rows[0][i].ToString());
                        TempTotal = TempTotal + temp;
                    }
                }

                //dgTemp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                //{
                //    Name = "AVG_VALUE",
                //    Header = "AVG_VALUE",
                //    Binding = new Binding()
                //    {
                //        Path = new PropertyPath("AVG_VALUE"),
                //        Mode = BindingMode.TwoWay
                //    },
                //    TextWrapping = TextWrapping.Wrap,
                //    IsReadOnly = true,
                //});
                dtRslt.Columns.Add("AVG_VALUE");
                dtRslt.Rows[0]["AVG_VALUE"] = (TempTotal/PointCnt).ToString("00.0");

                Util.GridSetData(dgTemp, dtRslt, FrameOperation, true);
                
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
            RadioButton rdo = (RadioButton)sender;
            if (rdo.IsChecked == true)
            {
                btnSearch_Click(null, null);
            }
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

        private DataTable GetTestData(ref DataTable dt)
        {
            //_dtDATA
            dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("ROW", typeof(int));
            dt.Columns.Add("COL", typeof(int));
            dt.Columns.Add("STG", typeof(int));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("DUMMY_FLAG", typeof(string));
            dt.Columns.Add("EIOSTAT", typeof(string));
            dt.Columns.Add("EQP_OP_STATUS_CD", typeof(string));
            dt.Columns.Add("RUN_MODE_CD", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("PROCNAME", typeof(string));
            dt.Columns.Add("NEXT_PROCID", typeof(string));
            dt.Columns.Add("FORMSTATUS", typeof(string));
            dt.Columns.Add("PROD_LOTID", typeof(string));
            dt.Columns.Add("OP_START_TIME", typeof(string));
            dt.Columns.Add("JOB_TIME", typeof(string));
            dt.Columns.Add("NOW_TIME", typeof(string));
            dt.Columns.Add("MAX_TEMP", typeof(string));
            dt.Columns.Add("MIN_TEMP", typeof(string));
            dt.Columns.Add("NEXT_PROC_DETL_TYPE_CODE", typeof(string));

            DataRow row1 = dt.NewRow(); row1["EQPTID"] = "FB11020010"; row1["ROW"] = "11"; row1["COL"] = "1"; row1["STG"] = "2"; row1["CSTID"] = "CFHA533470"; row1["LOTID"] = "509957336"; row1["DUMMY_FLAG"] = "N"; row1["EIOSTAT"] = "R"; row1["EQP_OP_STATUS_CD"] = ""; row1["RUN_MODE_CD"] = "C"; row1["PROCID"] = "191"; row1["PROCNAME"] = "Pre-Charge #1"; row1["NEXT_PROCID"] = "B11"; row1["FORMSTATUS"] = "12"; row1["PROD_LOTID"] = "H05CD10BN2"; row1["OP_START_TIME"] = "2023-04-10 13:48"; row1["JOB_TIME"] = "2023-04-10 13:48"; row1["NOW_TIME"] = "2023-04-10 13:48"; row1["MAX_TEMP"] = "32.5"; row1["MIN_TEMP"] = "29.5"; row1["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow(); row2["EQPTID"] = "FB11010010"; row2["ROW"] = "11"; row2["COL"] = "1"; row2["STG"] = "1"; row2["CSTID"] = ""; row2["LOTID"] = ""; row2["DUMMY_FLAG"] = ""; row2["EIOSTAT"] = "I"; row2["EQP_OP_STATUS_CD"] = "L"; row2["RUN_MODE_CD"] = "C"; row2["PROCID"] = ""; row2["PROCNAME"] = ""; row2["NEXT_PROCID"] = ""; row2["FORMSTATUS"] = "11"; row2["PROD_LOTID"] = ""; row2["OP_START_TIME"] = "2023-04-10 13:48"; row2["JOB_TIME"] = ""; row2["NOW_TIME"] = "2023-04-10 13:48"; row2["MAX_TEMP"] = "30"; row2["MIN_TEMP"] = "27"; row2["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow(); row3["EQPTID"] = "FC11020010"; row3["ROW"] = "12"; row3["COL"] = "2"; row3["STG"] = "2"; row3["CSTID"] = "CFHA523979"; row3["LOTID"] = "509957329"; row3["DUMMY_FLAG"] = "N"; row3["EIOSTAT"] = "R"; row3["EQP_OP_STATUS_CD"] = ""; row3["RUN_MODE_CD"] = "C"; row3["PROCID"] = "191"; row3["PROCNAME"] = "Pre-Charge #1"; row3["NEXT_PROCID"] = "B11"; row3["FORMSTATUS"] = "12"; row3["PROD_LOTID"] = "H05CD10CN2"; row3["OP_START_TIME"] = "2023-04-10 13:48"; row3["JOB_TIME"] = "2023-04-10 13:48"; row3["NOW_TIME"] = "2023-04-10 13:48"; row3["MAX_TEMP"] = "31.5"; row3["MIN_TEMP"] = "29.5"; row3["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow(); row4["EQPTID"] = "FC11010010"; row4["ROW"] = "12"; row4["COL"] = "2"; row4["STG"] = "1"; row4["CSTID"] = "CFHA509204"; row4["LOTID"] = "509957340"; row4["DUMMY_FLAG"] = "N"; row4["EIOSTAT"] = "R"; row4["EQP_OP_STATUS_CD"] = ""; row4["RUN_MODE_CD"] = "C"; row4["PROCID"] = "191"; row4["PROCNAME"] = "Pre-Charge #1"; row4["NEXT_PROCID"] = "B11"; row4["FORMSTATUS"] = "12"; row4["PROD_LOTID"] = "H05CD10CN2"; row4["OP_START_TIME"] = "2023-04-10 13:48"; row4["JOB_TIME"] = "2023-04-10 13:48"; row4["NOW_TIME"] = "2023-04-10 13:48"; row4["MAX_TEMP"] = "30"; row4["MIN_TEMP"] = "28"; row4["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow(); row5["EQPTID"] = "FD11020010"; row5["ROW"] = "13"; row5["COL"] = "3"; row5["STG"] = "2"; row5["CSTID"] = "CFHA349653"; row5["LOTID"] = "509957337"; row5["DUMMY_FLAG"] = "N"; row5["EIOSTAT"] = "R"; row5["EQP_OP_STATUS_CD"] = ""; row5["RUN_MODE_CD"] = "C"; row5["PROCID"] = "191"; row5["PROCNAME"] = "Pre-Charge #1"; row5["NEXT_PROCID"] = "B11"; row5["FORMSTATUS"] = "12"; row5["PROD_LOTID"] = "H05CD10DN2"; row5["OP_START_TIME"] = "2023-04-10 13:48"; row5["JOB_TIME"] = "2023-04-10 13:48"; row5["NOW_TIME"] = "2023-04-10 13:48"; row5["MAX_TEMP"] = "33"; row5["MIN_TEMP"] = "31"; row5["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow(); row6["EQPTID"] = "FD11010010"; row6["ROW"] = "13"; row6["COL"] = "3"; row6["STG"] = "1"; row6["CSTID"] = ""; row6["LOTID"] = ""; row6["DUMMY_FLAG"] = ""; row6["EIOSTAT"] = "I"; row6["EQP_OP_STATUS_CD"] = "L"; row6["RUN_MODE_CD"] = "C"; row6["PROCID"] = ""; row6["PROCNAME"] = ""; row6["NEXT_PROCID"] = ""; row6["FORMSTATUS"] = "11"; row6["PROD_LOTID"] = ""; row6["OP_START_TIME"] = "2023-04-10 13:48"; row6["JOB_TIME"] = ""; row6["NOW_TIME"] = "2023-04-10 13:48"; row6["MAX_TEMP"] = "34"; row6["MIN_TEMP"] = "32"; row6["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow(); row7["EQPTID"] = "FE11020010"; row7["ROW"] = "14"; row7["COL"] = "4"; row7["STG"] = "2"; row7["CSTID"] = ""; row7["LOTID"] = ""; row7["DUMMY_FLAG"] = ""; row7["EIOSTAT"] = "I"; row7["EQP_OP_STATUS_CD"] = "L"; row7["RUN_MODE_CD"] = "C"; row7["PROCID"] = ""; row7["PROCNAME"] = ""; row7["NEXT_PROCID"] = ""; row7["FORMSTATUS"] = "11"; row7["PROD_LOTID"] = ""; row7["OP_START_TIME"] = "2023-04-10 13:48"; row7["JOB_TIME"] = ""; row7["NOW_TIME"] = "2023-04-10 13:48"; row7["MAX_TEMP"] = "28"; row7["MIN_TEMP"] = "27.5"; row7["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow(); row8["EQPTID"] = "FE11010010"; row8["ROW"] = "14"; row8["COL"] = "4"; row8["STG"] = "1"; row8["CSTID"] = ""; row8["LOTID"] = ""; row8["DUMMY_FLAG"] = ""; row8["EIOSTAT"] = "I"; row8["EQP_OP_STATUS_CD"] = "L"; row8["RUN_MODE_CD"] = "C"; row8["PROCID"] = ""; row8["PROCNAME"] = ""; row8["NEXT_PROCID"] = ""; row8["FORMSTATUS"] = "11"; row8["PROD_LOTID"] = ""; row8["OP_START_TIME"] = "2023-04-10 13:48"; row8["JOB_TIME"] = ""; row8["NOW_TIME"] = "2023-04-10 13:48"; row8["MAX_TEMP"] = "28.5"; row8["MIN_TEMP"] = "27.5"; row8["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow(); row9["EQPTID"] = "FF11080010"; row9["ROW"] = "15"; row9["COL"] = "5"; row9["STG"] = "8"; row9["CSTID"] = "CFHA365843"; row9["LOTID"] = "509957310"; row9["DUMMY_FLAG"] = "N"; row9["EIOSTAT"] = "R"; row9["EQP_OP_STATUS_CD"] = ""; row9["RUN_MODE_CD"] = "C"; row9["PROCID"] = "171"; row9["PROCNAME"] = "低?流??#1"; row9["NEXT_PROCID"] = "191"; row9["FORMSTATUS"] = "15"; row9["PROD_LOTID"] = "H05CD10FN2"; row9["OP_START_TIME"] = "2023-04-10 13:48"; row9["JOB_TIME"] = "2023-04-10 13:48"; row9["NOW_TIME"] = "2023-04-10 13:48"; row9["MAX_TEMP"] = "29"; row9["MIN_TEMP"] = "28.5"; row9["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow(); row10["EQPTID"] = "FF11070010"; row10["ROW"] = "15"; row10["COL"] = "5"; row10["STG"] = "7"; row10["CSTID"] = "CFHA557554"; row10["LOTID"] = "509957296"; row10["DUMMY_FLAG"] = "N"; row10["EIOSTAT"] = "R"; row10["EQP_OP_STATUS_CD"] = ""; row10["RUN_MODE_CD"] = "C"; row10["PROCID"] = "171"; row10["PROCNAME"] = "低?流??#1"; row10["NEXT_PROCID"] = "191"; row10["FORMSTATUS"] = "15"; row10["PROD_LOTID"] = "H05CD10FN2"; row10["OP_START_TIME"] = "2023-04-10 13:48"; row10["JOB_TIME"] = "2023-04-10 13:48"; row10["NOW_TIME"] = "2023-04-10 13:48"; row10["MAX_TEMP"] = "29"; row10["MIN_TEMP"] = "28.5"; row10["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow(); row11["EQPTID"] = "FF11060010"; row11["ROW"] = "15"; row11["COL"] = "5"; row11["STG"] = "6"; row11["CSTID"] = "CFHA430288"; row11["LOTID"] = "509957323"; row11["DUMMY_FLAG"] = "N"; row11["EIOSTAT"] = "R"; row11["EQP_OP_STATUS_CD"] = ""; row11["RUN_MODE_CD"] = "C"; row11["PROCID"] = "171"; row11["PROCNAME"] = "低?流??#1"; row11["NEXT_PROCID"] = "191"; row11["FORMSTATUS"] = "15"; row11["PROD_LOTID"] = "H05CD10FN2"; row11["OP_START_TIME"] = "2023-04-10 13:48"; row11["JOB_TIME"] = "2023-04-10 13:48"; row11["NOW_TIME"] = "2023-04-10 13:48"; row11["MAX_TEMP"] = "29"; row11["MIN_TEMP"] = "28.5"; row11["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow(); row12["EQPTID"] = "FF11050010"; row12["ROW"] = "15"; row12["COL"] = "5"; row12["STG"] = "5"; row12["CSTID"] = "CFHA546069"; row12["LOTID"] = "509957272"; row12["DUMMY_FLAG"] = "N"; row12["EIOSTAT"] = "R"; row12["EQP_OP_STATUS_CD"] = ""; row12["RUN_MODE_CD"] = "C"; row12["PROCID"] = "171"; row12["PROCNAME"] = "低?流??#1"; row12["NEXT_PROCID"] = "191"; row12["FORMSTATUS"] = "15"; row12["PROD_LOTID"] = "H05CD10FN2"; row12["OP_START_TIME"] = "2023-04-10 13:48"; row12["JOB_TIME"] = "2023-04-10 13:48"; row12["NOW_TIME"] = "2023-04-10 13:48"; row12["MAX_TEMP"] = "29"; row12["MIN_TEMP"] = "28.5"; row12["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow(); row13["EQPTID"] = "FF11040010"; row13["ROW"] = "15"; row13["COL"] = "5"; row13["STG"] = "4"; row13["CSTID"] = ""; row13["LOTID"] = ""; row13["DUMMY_FLAG"] = ""; row13["EIOSTAT"] = "I"; row13["EQP_OP_STATUS_CD"] = "L"; row13["RUN_MODE_CD"] = "C"; row13["PROCID"] = ""; row13["PROCNAME"] = ""; row13["NEXT_PROCID"] = ""; row13["FORMSTATUS"] = "11"; row13["PROD_LOTID"] = ""; row13["OP_START_TIME"] = "2023-04-10 13:48"; row13["JOB_TIME"] = ""; row13["NOW_TIME"] = "2023-04-10 13:48"; row13["MAX_TEMP"] = "29"; row13["MIN_TEMP"] = "28.5"; row13["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow(); row14["EQPTID"] = "FF11030010"; row14["ROW"] = "15"; row14["COL"] = "5"; row14["STG"] = "3"; row14["CSTID"] = "CFHA355607"; row14["LOTID"] = "509957345"; row14["DUMMY_FLAG"] = "N"; row14["EIOSTAT"] = "I"; row14["EQP_OP_STATUS_CD"] = "R"; row14["RUN_MODE_CD"] = "C"; row14["PROCID"] = "0"; row14["PROCNAME"] = "Start"; row14["NEXT_PROCID"] = "171"; row14["FORMSTATUS"] = "10"; row14["PROD_LOTID"] = "H05CD10FN2"; row14["OP_START_TIME"] = "2023-04-10 13:48"; row14["JOB_TIME"] = "2023-04-10 13:48"; row14["NOW_TIME"] = "2023-04-10 13:48"; row14["MAX_TEMP"] = "29"; row14["MIN_TEMP"] = "29"; row14["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow(); row15["EQPTID"] = "FF11020010"; row15["ROW"] = "15"; row15["COL"] = "5"; row15["STG"] = "2"; row15["CSTID"] = "CFHA463375"; row15["LOTID"] = "509957283"; row15["DUMMY_FLAG"] = "N"; row15["EIOSTAT"] = "R"; row15["EQP_OP_STATUS_CD"] = ""; row15["RUN_MODE_CD"] = "C"; row15["PROCID"] = "171"; row15["PROCNAME"] = "低?流??#1"; row15["NEXT_PROCID"] = "191"; row15["FORMSTATUS"] = "15"; row15["PROD_LOTID"] = "H05CD10FN2"; row15["OP_START_TIME"] = "2023-04-10 13:48"; row15["JOB_TIME"] = "2023-04-10 13:48"; row15["NOW_TIME"] = "2023-04-10 13:48"; row15["MAX_TEMP"] = "28.5"; row15["MIN_TEMP"] = "28.5"; row15["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow(); row16["EQPTID"] = "FF11010010"; row16["ROW"] = "15"; row16["COL"] = "5"; row16["STG"] = "1"; row16["CSTID"] = "CFHA429193"; row16["LOTID"] = "509957259"; row16["DUMMY_FLAG"] = "N"; row16["EIOSTAT"] = "R"; row16["EQP_OP_STATUS_CD"] = ""; row16["RUN_MODE_CD"] = "C"; row16["PROCID"] = "191"; row16["PROCNAME"] = "Pre-Charge #1"; row16["NEXT_PROCID"] = "B11"; row16["FORMSTATUS"] = "12"; row16["PROD_LOTID"] = "H05CD10FN2"; row16["OP_START_TIME"] = "2023-04-10 13:48"; row16["JOB_TIME"] = "2023-04-10 13:48"; row16["NOW_TIME"] = "2023-04-10 13:48"; row16["MAX_TEMP"] = "28.5"; row16["MIN_TEMP"] = "28.5"; row16["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow(); row17["EQPTID"] = "FG11080010"; row17["ROW"] = "16"; row17["COL"] = "6"; row17["STG"] = "8"; row17["CSTID"] = "CFHA471657"; row17["LOTID"] = "509957274"; row17["DUMMY_FLAG"] = "N"; row17["EIOSTAT"] = "R"; row17["EQP_OP_STATUS_CD"] = ""; row17["RUN_MODE_CD"] = "C"; row17["PROCID"] = "171"; row17["PROCNAME"] = "低?流??#1"; row17["NEXT_PROCID"] = "191"; row17["FORMSTATUS"] = "15"; row17["PROD_LOTID"] = "H05CD10GN2"; row17["OP_START_TIME"] = "2023-04-10 13:48"; row17["JOB_TIME"] = "2023-04-10 13:48"; row17["NOW_TIME"] = "2023-04-10 13:48"; row17["MAX_TEMP"] = "29"; row17["MIN_TEMP"] = "28"; row17["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow(); row18["EQPTID"] = "FG11070010"; row18["ROW"] = "16"; row18["COL"] = "6"; row18["STG"] = "7"; row18["CSTID"] = "CFHA367727"; row18["LOTID"] = "509957306"; row18["DUMMY_FLAG"] = "N"; row18["EIOSTAT"] = "R"; row18["EQP_OP_STATUS_CD"] = ""; row18["RUN_MODE_CD"] = "C"; row18["PROCID"] = "171"; row18["PROCNAME"] = "低?流??#1"; row18["NEXT_PROCID"] = "191"; row18["FORMSTATUS"] = "15"; row18["PROD_LOTID"] = "H05CD10GN2"; row18["OP_START_TIME"] = "2023-04-10 13:48"; row18["JOB_TIME"] = "2023-04-10 13:48"; row18["NOW_TIME"] = "2023-04-10 13:48"; row18["MAX_TEMP"] = "29"; row18["MIN_TEMP"] = "28"; row18["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow(); row19["EQPTID"] = "FG11060010"; row19["ROW"] = "16"; row19["COL"] = "6"; row19["STG"] = "6"; row19["CSTID"] = "CFHA587002"; row19["LOTID"] = "509957293"; row19["DUMMY_FLAG"] = "N"; row19["EIOSTAT"] = "R"; row19["EQP_OP_STATUS_CD"] = ""; row19["RUN_MODE_CD"] = "C"; row19["PROCID"] = "171"; row19["PROCNAME"] = "低?流??#1"; row19["NEXT_PROCID"] = "191"; row19["FORMSTATUS"] = "15"; row19["PROD_LOTID"] = "H05CD10GN2"; row19["OP_START_TIME"] = "2023-04-10 13:48"; row19["JOB_TIME"] = "2023-04-10 13:48"; row19["NOW_TIME"] = "2023-04-10 13:48"; row19["MAX_TEMP"] = "28.5"; row19["MIN_TEMP"] = "28"; row19["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow(); row20["EQPTID"] = "FG11050010"; row20["ROW"] = "16"; row20["COL"] = "6"; row20["STG"] = "5"; row20["CSTID"] = "CFHA482854"; row20["LOTID"] = "509957236"; row20["DUMMY_FLAG"] = "N"; row20["EIOSTAT"] = "R"; row20["EQP_OP_STATUS_CD"] = ""; row20["RUN_MODE_CD"] = "C"; row20["PROCID"] = "191"; row20["PROCNAME"] = "Pre-Charge #1"; row20["NEXT_PROCID"] = "B11"; row20["FORMSTATUS"] = "12"; row20["PROD_LOTID"] = "H05CD10GN2"; row20["OP_START_TIME"] = "2023-04-10 13:48"; row20["JOB_TIME"] = "2023-04-10 13:48"; row20["NOW_TIME"] = "2023-04-10 13:48"; row20["MAX_TEMP"] = "28"; row20["MIN_TEMP"] = "27.5"; row20["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow(); row21["EQPTID"] = "FG11040010"; row21["ROW"] = "16"; row21["COL"] = "6"; row21["STG"] = "4"; row21["CSTID"] = "CFHA330275"; row21["LOTID"] = "509957285"; row21["DUMMY_FLAG"] = "N"; row21["EIOSTAT"] = "R"; row21["EQP_OP_STATUS_CD"] = ""; row21["RUN_MODE_CD"] = "C"; row21["PROCID"] = "171"; row21["PROCNAME"] = "低?流??#1"; row21["NEXT_PROCID"] = "191"; row21["FORMSTATUS"] = "15"; row21["PROD_LOTID"] = "H05CD10GN2"; row21["OP_START_TIME"] = "2023-04-10 13:48"; row21["JOB_TIME"] = "2023-04-10 13:48"; row21["NOW_TIME"] = "2023-04-10 13:48"; row21["MAX_TEMP"] = "28.5"; row21["MIN_TEMP"] = "28"; row21["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow(); row22["EQPTID"] = "FG11030010"; row22["ROW"] = "16"; row22["COL"] = "6"; row22["STG"] = "3"; row22["CSTID"] = "CFHA578737"; row22["LOTID"] = "509957268"; row22["DUMMY_FLAG"] = "N"; row22["EIOSTAT"] = "R"; row22["EQP_OP_STATUS_CD"] = ""; row22["RUN_MODE_CD"] = "C"; row22["PROCID"] = "171"; row22["PROCNAME"] = "低?流??#1"; row22["NEXT_PROCID"] = "191"; row22["FORMSTATUS"] = "15"; row22["PROD_LOTID"] = "H05CD10GN2"; row22["OP_START_TIME"] = "2023-04-10 13:48"; row22["JOB_TIME"] = "2023-04-10 13:48"; row22["NOW_TIME"] = "2023-04-10 13:48"; row22["MAX_TEMP"] = "28.5"; row22["MIN_TEMP"] = "28.5"; row22["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow(); row23["EQPTID"] = "FG11020010"; row23["ROW"] = "16"; row23["COL"] = "6"; row23["STG"] = "2"; row23["CSTID"] = "CFHA475532"; row23["LOTID"] = "509957223"; row23["DUMMY_FLAG"] = "N"; row23["EIOSTAT"] = "I"; row23["EQP_OP_STATUS_CD"] = "U"; row23["RUN_MODE_CD"] = "C"; row23["PROCID"] = "B11"; row23["PROCNAME"] = "判定 #1"; row23["NEXT_PROCID"] = "6C1"; row23["FORMSTATUS"] = "18"; row23["PROD_LOTID"] = "H05CD10GN2"; row23["OP_START_TIME"] = "2023-04-10 13:48"; row23["JOB_TIME"] = "2023-04-10 13:48"; row23["NOW_TIME"] = "2023-04-10 13:48"; row23["MAX_TEMP"] = "28"; row23["MIN_TEMP"] = "27.5"; row23["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow(); row24["EQPTID"] = "FG11010010"; row24["ROW"] = "16"; row24["COL"] = "6"; row24["STG"] = "1"; row24["CSTID"] = "CFHA501290"; row24["LOTID"] = "509957317"; row24["DUMMY_FLAG"] = "N"; row24["EIOSTAT"] = "I"; row24["EQP_OP_STATUS_CD"] = "R"; row24["RUN_MODE_CD"] = "C"; row24["PROCID"] = "0"; row24["PROCNAME"] = "Start"; row24["NEXT_PROCID"] = "171"; row24["FORMSTATUS"] = "10"; row24["PROD_LOTID"] = "H05CD10GN2"; row24["OP_START_TIME"] = "2023-04-10 13:48"; row24["JOB_TIME"] = "2023-04-10 13:48"; row24["NOW_TIME"] = "2023-04-10 13:48"; row24["MAX_TEMP"] = "28"; row24["MIN_TEMP"] = "27.5"; row24["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow(); row25["EQPTID"] = "FH11080010"; row25["ROW"] = "17"; row25["COL"] = "7"; row25["STG"] = "8"; row25["CSTID"] = ""; row25["LOTID"] = ""; row25["DUMMY_FLAG"] = ""; row25["EIOSTAT"] = "I"; row25["EQP_OP_STATUS_CD"] = "L"; row25["RUN_MODE_CD"] = "C"; row25["PROCID"] = ""; row25["PROCNAME"] = ""; row25["NEXT_PROCID"] = ""; row25["FORMSTATUS"] = "11"; row25["PROD_LOTID"] = ""; row25["OP_START_TIME"] = "2023-04-10 13:48"; row25["JOB_TIME"] = ""; row25["NOW_TIME"] = "2023-04-10 13:48"; row25["MAX_TEMP"] = "27"; row25["MIN_TEMP"] = "26.5"; row25["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow(); row26["EQPTID"] = "FH11070010"; row26["ROW"] = "17"; row26["COL"] = "7"; row26["STG"] = "7"; row26["CSTID"] = ""; row26["LOTID"] = ""; row26["DUMMY_FLAG"] = ""; row26["EIOSTAT"] = "I"; row26["EQP_OP_STATUS_CD"] = "L"; row26["RUN_MODE_CD"] = "C"; row26["PROCID"] = ""; row26["PROCNAME"] = ""; row26["NEXT_PROCID"] = ""; row26["FORMSTATUS"] = "11"; row26["PROD_LOTID"] = ""; row26["OP_START_TIME"] = "2023-04-10 13:48"; row26["JOB_TIME"] = ""; row26["NOW_TIME"] = "2023-04-10 13:48"; row26["MAX_TEMP"] = "26.5"; row26["MIN_TEMP"] = "26.5"; row26["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow(); row27["EQPTID"] = "FH11060010"; row27["ROW"] = "17"; row27["COL"] = "7"; row27["STG"] = "6"; row27["CSTID"] = ""; row27["LOTID"] = ""; row27["DUMMY_FLAG"] = ""; row27["EIOSTAT"] = "I"; row27["EQP_OP_STATUS_CD"] = "L"; row27["RUN_MODE_CD"] = "C"; row27["PROCID"] = ""; row27["PROCNAME"] = ""; row27["NEXT_PROCID"] = ""; row27["FORMSTATUS"] = "11"; row27["PROD_LOTID"] = ""; row27["OP_START_TIME"] = "2023-04-10 13:48"; row27["JOB_TIME"] = ""; row27["NOW_TIME"] = "2023-04-10 13:48"; row27["MAX_TEMP"] = "27"; row27["MIN_TEMP"] = "26.5"; row27["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow(); row28["EQPTID"] = "FH11050010"; row28["ROW"] = "17"; row28["COL"] = "7"; row28["STG"] = "5"; row28["CSTID"] = ""; row28["LOTID"] = ""; row28["DUMMY_FLAG"] = ""; row28["EIOSTAT"] = "I"; row28["EQP_OP_STATUS_CD"] = "L"; row28["RUN_MODE_CD"] = "C"; row28["PROCID"] = ""; row28["PROCNAME"] = ""; row28["NEXT_PROCID"] = ""; row28["FORMSTATUS"] = "11"; row28["PROD_LOTID"] = ""; row28["OP_START_TIME"] = "2023-04-10 13:48"; row28["JOB_TIME"] = ""; row28["NOW_TIME"] = "2023-04-10 13:48"; row28["MAX_TEMP"] = "27"; row28["MIN_TEMP"] = "26.5"; row28["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow(); row29["EQPTID"] = "FH11040010"; row29["ROW"] = "17"; row29["COL"] = "7"; row29["STG"] = "4"; row29["CSTID"] = ""; row29["LOTID"] = ""; row29["DUMMY_FLAG"] = ""; row29["EIOSTAT"] = "I"; row29["EQP_OP_STATUS_CD"] = "L"; row29["RUN_MODE_CD"] = "C"; row29["PROCID"] = ""; row29["PROCNAME"] = ""; row29["NEXT_PROCID"] = ""; row29["FORMSTATUS"] = "11"; row29["PROD_LOTID"] = ""; row29["OP_START_TIME"] = "2023-04-10 13:48"; row29["JOB_TIME"] = ""; row29["NOW_TIME"] = "2023-04-10 13:48"; row29["MAX_TEMP"] = "26.5"; row29["MIN_TEMP"] = "26.5"; row29["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow(); row30["EQPTID"] = "FH11030010"; row30["ROW"] = "17"; row30["COL"] = "7"; row30["STG"] = "3"; row30["CSTID"] = ""; row30["LOTID"] = ""; row30["DUMMY_FLAG"] = ""; row30["EIOSTAT"] = "I"; row30["EQP_OP_STATUS_CD"] = "L"; row30["RUN_MODE_CD"] = "C"; row30["PROCID"] = ""; row30["PROCNAME"] = ""; row30["NEXT_PROCID"] = ""; row30["FORMSTATUS"] = "11"; row30["PROD_LOTID"] = ""; row30["OP_START_TIME"] = "2023-04-10 13:48"; row30["JOB_TIME"] = ""; row30["NOW_TIME"] = "2023-04-10 13:48"; row30["MAX_TEMP"] = "26.5"; row30["MIN_TEMP"] = "26.5"; row30["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow(); row31["EQPTID"] = "FH11020010"; row31["ROW"] = "17"; row31["COL"] = "7"; row31["STG"] = "2"; row31["CSTID"] = ""; row31["LOTID"] = ""; row31["DUMMY_FLAG"] = ""; row31["EIOSTAT"] = "I"; row31["EQP_OP_STATUS_CD"] = "L"; row31["RUN_MODE_CD"] = "C"; row31["PROCID"] = ""; row31["PROCNAME"] = ""; row31["NEXT_PROCID"] = ""; row31["FORMSTATUS"] = "11"; row31["PROD_LOTID"] = ""; row31["OP_START_TIME"] = "2023-04-10 13:48"; row31["JOB_TIME"] = ""; row31["NOW_TIME"] = "2023-04-10 13:48"; row31["MAX_TEMP"] = "26.5"; row31["MIN_TEMP"] = "26"; row31["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow(); row32["EQPTID"] = "FH11010010"; row32["ROW"] = "17"; row32["COL"] = "7"; row32["STG"] = "1"; row32["CSTID"] = ""; row32["LOTID"] = ""; row32["DUMMY_FLAG"] = ""; row32["EIOSTAT"] = "I"; row32["EQP_OP_STATUS_CD"] = "L"; row32["RUN_MODE_CD"] = "C"; row32["PROCID"] = ""; row32["PROCNAME"] = ""; row32["NEXT_PROCID"] = ""; row32["FORMSTATUS"] = "11"; row32["PROD_LOTID"] = ""; row32["OP_START_TIME"] = "2023-04-10 13:48"; row32["JOB_TIME"] = ""; row32["NOW_TIME"] = "2023-04-10 13:48"; row32["MAX_TEMP"] = "26"; row32["MIN_TEMP"] = "26"; row32["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow(); row33["EQPTID"] = "FI11080010"; row33["ROW"] = "18"; row33["COL"] = "8"; row33["STG"] = "8"; row33["CSTID"] = ""; row33["LOTID"] = ""; row33["DUMMY_FLAG"] = ""; row33["EIOSTAT"] = "I"; row33["EQP_OP_STATUS_CD"] = "L"; row33["RUN_MODE_CD"] = "C"; row33["PROCID"] = ""; row33["PROCNAME"] = ""; row33["NEXT_PROCID"] = ""; row33["FORMSTATUS"] = "11"; row33["PROD_LOTID"] = ""; row33["OP_START_TIME"] = "2023-04-10 13:48"; row33["JOB_TIME"] = ""; row33["NOW_TIME"] = "2023-04-10 13:48"; row33["MAX_TEMP"] = "28"; row33["MIN_TEMP"] = "27.5"; row33["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow(); row34["EQPTID"] = "FI11070010"; row34["ROW"] = "18"; row34["COL"] = "8"; row34["STG"] = "7"; row34["CSTID"] = ""; row34["LOTID"] = ""; row34["DUMMY_FLAG"] = ""; row34["EIOSTAT"] = "I"; row34["EQP_OP_STATUS_CD"] = "L"; row34["RUN_MODE_CD"] = "C"; row34["PROCID"] = ""; row34["PROCNAME"] = ""; row34["NEXT_PROCID"] = ""; row34["FORMSTATUS"] = "11"; row34["PROD_LOTID"] = ""; row34["OP_START_TIME"] = "2023-04-10 13:48"; row34["JOB_TIME"] = ""; row34["NOW_TIME"] = "2023-04-10 13:48"; row34["MAX_TEMP"] = "28.5"; row34["MIN_TEMP"] = "28"; row34["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow(); row35["EQPTID"] = "FI11060010"; row35["ROW"] = "18"; row35["COL"] = "8"; row35["STG"] = "6"; row35["CSTID"] = ""; row35["LOTID"] = ""; row35["DUMMY_FLAG"] = ""; row35["EIOSTAT"] = "I"; row35["EQP_OP_STATUS_CD"] = "L"; row35["RUN_MODE_CD"] = "C"; row35["PROCID"] = ""; row35["PROCNAME"] = ""; row35["NEXT_PROCID"] = ""; row35["FORMSTATUS"] = "11"; row35["PROD_LOTID"] = ""; row35["OP_START_TIME"] = "2023-04-10 13:48"; row35["JOB_TIME"] = ""; row35["NOW_TIME"] = "2023-04-10 13:48"; row35["MAX_TEMP"] = "28"; row35["MIN_TEMP"] = "27.5"; row35["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row35);
            DataRow row36 = dt.NewRow(); row36["EQPTID"] = "FI11050010"; row36["ROW"] = "18"; row36["COL"] = "8"; row36["STG"] = "5"; row36["CSTID"] = ""; row36["LOTID"] = ""; row36["DUMMY_FLAG"] = ""; row36["EIOSTAT"] = "I"; row36["EQP_OP_STATUS_CD"] = "L"; row36["RUN_MODE_CD"] = "C"; row36["PROCID"] = ""; row36["PROCNAME"] = ""; row36["NEXT_PROCID"] = ""; row36["FORMSTATUS"] = "11"; row36["PROD_LOTID"] = ""; row36["OP_START_TIME"] = "2023-04-10 13:48"; row36["JOB_TIME"] = ""; row36["NOW_TIME"] = "2023-04-10 13:48"; row36["MAX_TEMP"] = "29"; row36["MIN_TEMP"] = "29"; row36["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row36);
            DataRow row37 = dt.NewRow(); row37["EQPTID"] = "FI11040010"; row37["ROW"] = "18"; row37["COL"] = "8"; row37["STG"] = "4"; row37["CSTID"] = ""; row37["LOTID"] = ""; row37["DUMMY_FLAG"] = ""; row37["EIOSTAT"] = "I"; row37["EQP_OP_STATUS_CD"] = "L"; row37["RUN_MODE_CD"] = "C"; row37["PROCID"] = ""; row37["PROCNAME"] = ""; row37["NEXT_PROCID"] = ""; row37["FORMSTATUS"] = "11"; row37["PROD_LOTID"] = ""; row37["OP_START_TIME"] = "2023-04-10 13:48"; row37["JOB_TIME"] = ""; row37["NOW_TIME"] = "2023-04-10 13:48"; row37["MAX_TEMP"] = "27.5"; row37["MIN_TEMP"] = "27"; row37["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row37);
            DataRow row38 = dt.NewRow(); row38["EQPTID"] = "FI11030010"; row38["ROW"] = "18"; row38["COL"] = "8"; row38["STG"] = "3"; row38["CSTID"] = ""; row38["LOTID"] = ""; row38["DUMMY_FLAG"] = ""; row38["EIOSTAT"] = "I"; row38["EQP_OP_STATUS_CD"] = "L"; row38["RUN_MODE_CD"] = "C"; row38["PROCID"] = ""; row38["PROCNAME"] = ""; row38["NEXT_PROCID"] = ""; row38["FORMSTATUS"] = "11"; row38["PROD_LOTID"] = ""; row38["OP_START_TIME"] = "2023-04-10 13:48"; row38["JOB_TIME"] = ""; row38["NOW_TIME"] = "2023-04-10 13:48"; row38["MAX_TEMP"] = "28.5"; row38["MIN_TEMP"] = "28"; row38["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row38);
            DataRow row39 = dt.NewRow(); row39["EQPTID"] = "FI11020010"; row39["ROW"] = "18"; row39["COL"] = "8"; row39["STG"] = "2"; row39["CSTID"] = "CFHA381400"; row39["LOTID"] = "509957294"; row39["DUMMY_FLAG"] = "N"; row39["EIOSTAT"] = "R"; row39["EQP_OP_STATUS_CD"] = ""; row39["RUN_MODE_CD"] = "C"; row39["PROCID"] = "171"; row39["PROCNAME"] = "低?流??#1"; row39["NEXT_PROCID"] = "191"; row39["FORMSTATUS"] = "15"; row39["PROD_LOTID"] = "H05CD10IN2"; row39["OP_START_TIME"] = "2023-04-10 13:48"; row39["JOB_TIME"] = "2023-04-10 13:48"; row39["NOW_TIME"] = "2023-04-10 13:48"; row39["MAX_TEMP"] = "27.5"; row39["MIN_TEMP"] = "27"; row39["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row39);
            DataRow row40 = dt.NewRow(); row40["EQPTID"] = "FI11010010"; row40["ROW"] = "18"; row40["COL"] = "8"; row40["STG"] = "1"; row40["CSTID"] = "CFHA585283"; row40["LOTID"] = "509957341"; row40["DUMMY_FLAG"] = "N"; row40["EIOSTAT"] = "R"; row40["EQP_OP_STATUS_CD"] = ""; row40["RUN_MODE_CD"] = "C"; row40["PROCID"] = "171"; row40["PROCNAME"] = "低?流??#1"; row40["NEXT_PROCID"] = "191"; row40["FORMSTATUS"] = "15"; row40["PROD_LOTID"] = "H05CD10IN2"; row40["OP_START_TIME"] = "2023-04-10 13:48"; row40["JOB_TIME"] = "2023-04-10 13:48"; row40["NOW_TIME"] = "2023-04-10 13:48"; row40["MAX_TEMP"] = "27.5"; row40["MIN_TEMP"] = "27.5"; row40["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row40);
            DataRow row41 = dt.NewRow(); row41["EQPTID"] = "FJ11080010"; row41["ROW"] = "19"; row41["COL"] = "9"; row41["STG"] = "8"; row41["CSTID"] = "CFHA566881"; row41["LOTID"] = "509957305"; row41["DUMMY_FLAG"] = "N"; row41["EIOSTAT"] = "R"; row41["EQP_OP_STATUS_CD"] = ""; row41["RUN_MODE_CD"] = "C"; row41["PROCID"] = "171"; row41["PROCNAME"] = "低?流??#1"; row41["NEXT_PROCID"] = "191"; row41["FORMSTATUS"] = "15"; row41["PROD_LOTID"] = "H05CD10JN1"; row41["OP_START_TIME"] = "2023-04-10 13:48"; row41["JOB_TIME"] = "2023-04-10 13:48"; row41["NOW_TIME"] = "2023-04-10 13:48"; row41["MAX_TEMP"] = "28.5"; row41["MIN_TEMP"] = "28"; row41["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row41);
            DataRow row42 = dt.NewRow(); row42["EQPTID"] = "FJ11070010"; row42["ROW"] = "19"; row42["COL"] = "9"; row42["STG"] = "7"; row42["CSTID"] = "CFHA527738"; row42["LOTID"] = "509957291"; row42["DUMMY_FLAG"] = "N"; row42["EIOSTAT"] = "R"; row42["EQP_OP_STATUS_CD"] = ""; row42["RUN_MODE_CD"] = "C"; row42["PROCID"] = "171"; row42["PROCNAME"] = "低?流??#1"; row42["NEXT_PROCID"] = "191"; row42["FORMSTATUS"] = "15"; row42["PROD_LOTID"] = "H05CD10JN1"; row42["OP_START_TIME"] = "2023-04-10 13:48"; row42["JOB_TIME"] = "2023-04-10 13:48"; row42["NOW_TIME"] = "2023-04-10 13:48"; row42["MAX_TEMP"] = "28.5"; row42["MIN_TEMP"] = "28"; row42["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row42);
            DataRow row43 = dt.NewRow(); row43["EQPTID"] = "FJ11060010"; row43["ROW"] = "19"; row43["COL"] = "9"; row43["STG"] = "6"; row43["CSTID"] = "CFHA361362"; row43["LOTID"] = "509957279"; row43["DUMMY_FLAG"] = "N"; row43["EIOSTAT"] = "R"; row43["EQP_OP_STATUS_CD"] = ""; row43["RUN_MODE_CD"] = "C"; row43["PROCID"] = "171"; row43["PROCNAME"] = "低?流??#1"; row43["NEXT_PROCID"] = "191"; row43["FORMSTATUS"] = "15"; row43["PROD_LOTID"] = "H05CD10JN1"; row43["OP_START_TIME"] = "2023-04-10 13:48"; row43["JOB_TIME"] = "2023-04-10 13:48"; row43["NOW_TIME"] = "2023-04-10 13:48"; row43["MAX_TEMP"] = "29"; row43["MIN_TEMP"] = "28.5"; row43["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row43);
            DataRow row44 = dt.NewRow(); row44["EQPTID"] = "FJ11050010"; row44["ROW"] = "19"; row44["COL"] = "9"; row44["STG"] = "5"; row44["CSTID"] = "CFHA345230"; row44["LOTID"] = "509957269"; row44["DUMMY_FLAG"] = "N"; row44["EIOSTAT"] = "R"; row44["EQP_OP_STATUS_CD"] = ""; row44["RUN_MODE_CD"] = "C"; row44["PROCID"] = "191"; row44["PROCNAME"] = "Pre-Charge #1"; row44["NEXT_PROCID"] = "B11"; row44["FORMSTATUS"] = "12"; row44["PROD_LOTID"] = "H05CD10JN1"; row44["OP_START_TIME"] = "2023-04-10 13:48"; row44["JOB_TIME"] = "2023-04-10 13:48"; row44["NOW_TIME"] = "2023-04-10 13:48"; row44["MAX_TEMP"] = "28.5"; row44["MIN_TEMP"] = "28"; row44["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row44);
            DataRow row45 = dt.NewRow(); row45["EQPTID"] = "FJ11040010"; row45["ROW"] = "19"; row45["COL"] = "9"; row45["STG"] = "4"; row45["CSTID"] = ""; row45["LOTID"] = ""; row45["DUMMY_FLAG"] = ""; row45["EIOSTAT"] = "I"; row45["EQP_OP_STATUS_CD"] = "L"; row45["RUN_MODE_CD"] = "C"; row45["PROCID"] = ""; row45["PROCNAME"] = ""; row45["NEXT_PROCID"] = ""; row45["FORMSTATUS"] = "11"; row45["PROD_LOTID"] = ""; row45["OP_START_TIME"] = "2023-04-10 13:48"; row45["JOB_TIME"] = ""; row45["NOW_TIME"] = "2023-04-10 13:48"; row45["MAX_TEMP"] = "27.5"; row45["MIN_TEMP"] = "27"; row45["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row45);
            DataRow row46 = dt.NewRow(); row46["EQPTID"] = "FJ11030010"; row46["ROW"] = "19"; row46["COL"] = "9"; row46["STG"] = "3"; row46["CSTID"] = ""; row46["LOTID"] = ""; row46["DUMMY_FLAG"] = ""; row46["EIOSTAT"] = "I"; row46["EQP_OP_STATUS_CD"] = "L"; row46["RUN_MODE_CD"] = "C"; row46["PROCID"] = ""; row46["PROCNAME"] = ""; row46["NEXT_PROCID"] = ""; row46["FORMSTATUS"] = "11"; row46["PROD_LOTID"] = ""; row46["OP_START_TIME"] = "2023-04-10 13:48"; row46["JOB_TIME"] = ""; row46["NOW_TIME"] = "2023-04-10 13:48"; row46["MAX_TEMP"] = "28"; row46["MIN_TEMP"] = "27.5"; row46["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row46);
            DataRow row47 = dt.NewRow(); row47["EQPTID"] = "FJ11020010"; row47["ROW"] = "19"; row47["COL"] = "9"; row47["STG"] = "2"; row47["CSTID"] = "CFHA329963"; row47["LOTID"] = "509957257"; row47["DUMMY_FLAG"] = "N"; row47["EIOSTAT"] = "R"; row47["EQP_OP_STATUS_CD"] = ""; row47["RUN_MODE_CD"] = "C"; row47["PROCID"] = "191"; row47["PROCNAME"] = "Pre-Charge #1"; row47["NEXT_PROCID"] = "B11"; row47["FORMSTATUS"] = "12"; row47["PROD_LOTID"] = "H05CD10JN1"; row47["OP_START_TIME"] = "2023-04-10 13:48"; row47["JOB_TIME"] = "2023-04-10 13:48"; row47["NOW_TIME"] = "2023-04-10 13:48"; row47["MAX_TEMP"] = "27.5"; row47["MIN_TEMP"] = "27"; row47["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row47);
            DataRow row48 = dt.NewRow(); row48["EQPTID"] = "FJ11010010"; row48["ROW"] = "19"; row48["COL"] = "9"; row48["STG"] = "1"; row48["CSTID"] = ""; row48["LOTID"] = ""; row48["DUMMY_FLAG"] = ""; row48["EIOSTAT"] = "I"; row48["EQP_OP_STATUS_CD"] = "L"; row48["RUN_MODE_CD"] = "C"; row48["PROCID"] = ""; row48["PROCNAME"] = ""; row48["NEXT_PROCID"] = ""; row48["FORMSTATUS"] = "11"; row48["PROD_LOTID"] = ""; row48["OP_START_TIME"] = "2023-04-10 13:48"; row48["JOB_TIME"] = ""; row48["NOW_TIME"] = "2023-04-10 13:48"; row48["MAX_TEMP"] = "28"; row48["MIN_TEMP"] = "27"; row48["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row48);
            DataRow row49 = dt.NewRow(); row49["EQPTID"] = "FK11080010"; row49["ROW"] = "20"; row49["COL"] = "10"; row49["STG"] = "8"; row49["CSTID"] = "CFHA545243"; row49["LOTID"] = "509957289"; row49["DUMMY_FLAG"] = "N"; row49["EIOSTAT"] = "R"; row49["EQP_OP_STATUS_CD"] = ""; row49["RUN_MODE_CD"] = "C"; row49["PROCID"] = "171"; row49["PROCNAME"] = "低?流??#1"; row49["NEXT_PROCID"] = "191"; row49["FORMSTATUS"] = "15"; row49["PROD_LOTID"] = "H05CD10KN1"; row49["OP_START_TIME"] = "2023-04-10 13:48"; row49["JOB_TIME"] = "2023-04-10 13:48"; row49["NOW_TIME"] = "2023-04-10 13:48"; row49["MAX_TEMP"] = "30.5"; row49["MIN_TEMP"] = "30"; row49["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row49);
            DataRow row50 = dt.NewRow(); row50["EQPTID"] = "FK11070010"; row50["ROW"] = "20"; row50["COL"] = "10"; row50["STG"] = "7"; row50["CSTID"] = "CFHA213380"; row50["LOTID"] = "509957263"; row50["DUMMY_FLAG"] = "N"; row50["EIOSTAT"] = "I"; row50["EQP_OP_STATUS_CD"] = "U"; row50["RUN_MODE_CD"] = "C"; row50["PROCID"] = "B11"; row50["PROCNAME"] = "判定 #1"; row50["NEXT_PROCID"] = "6C1"; row50["FORMSTATUS"] = "18"; row50["PROD_LOTID"] = "H05CD10KN1"; row50["OP_START_TIME"] = "2023-04-10 13:48"; row50["JOB_TIME"] = "2023-04-10 13:48"; row50["NOW_TIME"] = "2023-04-10 13:48"; row50["MAX_TEMP"] = "30"; row50["MIN_TEMP"] = "29.5"; row50["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row50);
            DataRow row51 = dt.NewRow(); row51["EQPTID"] = "FK11060010"; row51["ROW"] = "20"; row51["COL"] = "10"; row51["STG"] = "6"; row51["CSTID"] = "CFHA469351"; row51["LOTID"] = "509957350"; row51["DUMMY_FLAG"] = "N"; row51["EIOSTAT"] = "I"; row51["EQP_OP_STATUS_CD"] = "R"; row51["RUN_MODE_CD"] = "C"; row51["PROCID"] = "0"; row51["PROCNAME"] = "Start"; row51["NEXT_PROCID"] = "171"; row51["FORMSTATUS"] = "10"; row51["PROD_LOTID"] = "H05CD10KN1"; row51["OP_START_TIME"] = "2023-04-10 13:48"; row51["JOB_TIME"] = "2023-04-10 13:48"; row51["NOW_TIME"] = "2023-04-10 13:48"; row51["MAX_TEMP"] = "30.5"; row51["MIN_TEMP"] = "29.5"; row51["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row51);
            DataRow row52 = dt.NewRow(); row52["EQPTID"] = "FK11050010"; row52["ROW"] = "20"; row52["COL"] = "10"; row52["STG"] = "5"; row52["CSTID"] = "CFHA494315"; row52["LOTID"] = "509957338"; row52["DUMMY_FLAG"] = "N"; row52["EIOSTAT"] = "R"; row52["EQP_OP_STATUS_CD"] = ""; row52["RUN_MODE_CD"] = "C"; row52["PROCID"] = "171"; row52["PROCNAME"] = "低?流??#1"; row52["NEXT_PROCID"] = "191"; row52["FORMSTATUS"] = "15"; row52["PROD_LOTID"] = "H05CD10KN1"; row52["OP_START_TIME"] = "2023-04-10 13:48"; row52["JOB_TIME"] = "2023-04-10 13:48"; row52["NOW_TIME"] = "2023-04-10 13:48"; row52["MAX_TEMP"] = "30.5"; row52["MIN_TEMP"] = "30"; row52["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row52);
            DataRow row53 = dt.NewRow(); row53["EQPTID"] = "FK11040010"; row53["ROW"] = "20"; row53["COL"] = "10"; row53["STG"] = "4"; row53["CSTID"] = "CFHA387903"; row53["LOTID"] = "509957314"; row53["DUMMY_FLAG"] = "N"; row53["EIOSTAT"] = "R"; row53["EQP_OP_STATUS_CD"] = ""; row53["RUN_MODE_CD"] = "C"; row53["PROCID"] = "171"; row53["PROCNAME"] = "低?流??#1"; row53["NEXT_PROCID"] = "191"; row53["FORMSTATUS"] = "15"; row53["PROD_LOTID"] = "H05CD10KN1"; row53["OP_START_TIME"] = "2023-04-10 13:48"; row53["JOB_TIME"] = "2023-04-10 13:48"; row53["NOW_TIME"] = "2023-04-10 13:48"; row53["MAX_TEMP"] = "30.5"; row53["MIN_TEMP"] = "30"; row53["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row53);
            DataRow row54 = dt.NewRow(); row54["EQPTID"] = "FK11030010"; row54["ROW"] = "20"; row54["COL"] = "10"; row54["STG"] = "3"; row54["CSTID"] = "CFHA473577"; row54["LOTID"] = "509957303"; row54["DUMMY_FLAG"] = "N"; row54["EIOSTAT"] = "R"; row54["EQP_OP_STATUS_CD"] = ""; row54["RUN_MODE_CD"] = "C"; row54["PROCID"] = "171"; row54["PROCNAME"] = "低?流??#1"; row54["NEXT_PROCID"] = "191"; row54["FORMSTATUS"] = "15"; row54["PROD_LOTID"] = "H05CD10KN1"; row54["OP_START_TIME"] = "2023-04-10 13:48"; row54["JOB_TIME"] = "2023-04-10 13:48"; row54["NOW_TIME"] = "2023-04-10 13:48"; row54["MAX_TEMP"] = "30"; row54["MIN_TEMP"] = "29"; row54["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row54);
            DataRow row55 = dt.NewRow(); row55["EQPTID"] = "FK11020010"; row55["ROW"] = "20"; row55["COL"] = "10"; row55["STG"] = "2"; row55["CSTID"] = "CFHA404986"; row55["LOTID"] = "509957276"; row55["DUMMY_FLAG"] = "N"; row55["EIOSTAT"] = "R"; row55["EQP_OP_STATUS_CD"] = ""; row55["RUN_MODE_CD"] = "C"; row55["PROCID"] = "171"; row55["PROCNAME"] = "低?流??#1"; row55["NEXT_PROCID"] = "191"; row55["FORMSTATUS"] = "15"; row55["PROD_LOTID"] = "H05CD10KN1"; row55["OP_START_TIME"] = "2023-04-10 13:48"; row55["JOB_TIME"] = "2023-04-10 13:48"; row55["NOW_TIME"] = "2023-04-10 13:48"; row55["MAX_TEMP"] = "30"; row55["MIN_TEMP"] = "29"; row55["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row55);
            DataRow row56 = dt.NewRow(); row56["EQPTID"] = "FK11010010"; row56["ROW"] = "20"; row56["COL"] = "10"; row56["STG"] = "1"; row56["CSTID"] = "CFHA552081"; row56["LOTID"] = "509957327"; row56["DUMMY_FLAG"] = "N"; row56["EIOSTAT"] = "R"; row56["EQP_OP_STATUS_CD"] = ""; row56["RUN_MODE_CD"] = "C"; row56["PROCID"] = "171"; row56["PROCNAME"] = "低?流??#1"; row56["NEXT_PROCID"] = "191"; row56["FORMSTATUS"] = "15"; row56["PROD_LOTID"] = "H05CD10KN1"; row56["OP_START_TIME"] = "2023-04-10 13:48"; row56["JOB_TIME"] = "2023-04-10 13:48"; row56["NOW_TIME"] = "2023-04-10 13:48"; row56["MAX_TEMP"] = "30.5"; row56["MIN_TEMP"] = "30"; row56["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row56);
            DataRow row57 = dt.NewRow(); row57["EQPTID"] = "FL11080010"; row57["ROW"] = "21"; row57["COL"] = "11"; row57["STG"] = "8"; row57["CSTID"] = "CFHA376023"; row57["LOTID"] = "509957290"; row57["DUMMY_FLAG"] = "N"; row57["EIOSTAT"] = "R"; row57["EQP_OP_STATUS_CD"] = ""; row57["RUN_MODE_CD"] = "C"; row57["PROCID"] = "171"; row57["PROCNAME"] = "低?流??#1"; row57["NEXT_PROCID"] = "191"; row57["FORMSTATUS"] = "15"; row57["PROD_LOTID"] = "H05CD10LN1"; row57["OP_START_TIME"] = "2023-04-10 13:48"; row57["JOB_TIME"] = "2023-04-10 13:48"; row57["NOW_TIME"] = "2023-04-10 13:48"; row57["MAX_TEMP"] = "30.5"; row57["MIN_TEMP"] = "30.5"; row57["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row57);
            DataRow row58 = dt.NewRow(); row58["EQPTID"] = "FL11070010"; row58["ROW"] = "21"; row58["COL"] = "11"; row58["STG"] = "7"; row58["CSTID"] = "CFHA404014"; row58["LOTID"] = "509957304"; row58["DUMMY_FLAG"] = "N"; row58["EIOSTAT"] = "R"; row58["EQP_OP_STATUS_CD"] = ""; row58["RUN_MODE_CD"] = "C"; row58["PROCID"] = "171"; row58["PROCNAME"] = "低?流??#1"; row58["NEXT_PROCID"] = "191"; row58["FORMSTATUS"] = "15"; row58["PROD_LOTID"] = "H05CD10LN1"; row58["OP_START_TIME"] = "2023-04-10 13:48"; row58["JOB_TIME"] = "2023-04-10 13:48"; row58["NOW_TIME"] = "2023-04-10 13:48"; row58["MAX_TEMP"] = "30.5"; row58["MIN_TEMP"] = "30"; row58["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row58);
            DataRow row59 = dt.NewRow(); row59["EQPTID"] = "FL11060010"; row59["ROW"] = "21"; row59["COL"] = "11"; row59["STG"] = "6"; row59["CSTID"] = "CFHA350414"; row59["LOTID"] = "509957278"; row59["DUMMY_FLAG"] = "N"; row59["EIOSTAT"] = "R"; row59["EQP_OP_STATUS_CD"] = ""; row59["RUN_MODE_CD"] = "C"; row59["PROCID"] = "171"; row59["PROCNAME"] = "低?流??#1"; row59["NEXT_PROCID"] = "191"; row59["FORMSTATUS"] = "15"; row59["PROD_LOTID"] = "H05CD10LN1"; row59["OP_START_TIME"] = "2023-04-10 13:48"; row59["JOB_TIME"] = "2023-04-10 13:48"; row59["NOW_TIME"] = "2023-04-10 13:48"; row59["MAX_TEMP"] = "30.5"; row59["MIN_TEMP"] = "30.5"; row59["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row59);
            DataRow row60 = dt.NewRow(); row60["EQPTID"] = "FL11050010"; row60["ROW"] = "21"; row60["COL"] = "11"; row60["STG"] = "5"; row60["CSTID"] = "CFHA384165"; row60["LOTID"] = "509957265"; row60["DUMMY_FLAG"] = "N"; row60["EIOSTAT"] = "R"; row60["EQP_OP_STATUS_CD"] = ""; row60["RUN_MODE_CD"] = "C"; row60["PROCID"] = "191"; row60["PROCNAME"] = "Pre-Charge #1"; row60["NEXT_PROCID"] = "B11"; row60["FORMSTATUS"] = "12"; row60["PROD_LOTID"] = "H05CD10LN1"; row60["OP_START_TIME"] = "2023-04-10 13:48"; row60["JOB_TIME"] = "2023-04-10 13:48"; row60["NOW_TIME"] = "2023-04-10 13:48"; row60["MAX_TEMP"] = "30.5"; row60["MIN_TEMP"] = "30"; row60["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row60);
            DataRow row61 = dt.NewRow(); row61["EQPTID"] = "FL11040010"; row61["ROW"] = "21"; row61["COL"] = "11"; row61["STG"] = "4"; row61["CSTID"] = ""; row61["LOTID"] = ""; row61["DUMMY_FLAG"] = ""; row61["EIOSTAT"] = "I"; row61["EQP_OP_STATUS_CD"] = "L"; row61["RUN_MODE_CD"] = "C"; row61["PROCID"] = ""; row61["PROCNAME"] = ""; row61["NEXT_PROCID"] = ""; row61["FORMSTATUS"] = "11"; row61["PROD_LOTID"] = ""; row61["OP_START_TIME"] = "2023-04-10 13:48"; row61["JOB_TIME"] = "2023-04-10 13:48"; row61["NOW_TIME"] = "2023-04-10 13:48"; row61["MAX_TEMP"] = "31"; row61["MIN_TEMP"] = "30"; row61["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row61);
            DataRow row62 = dt.NewRow(); row62["EQPTID"] = "FL11030010"; row62["ROW"] = "21"; row62["COL"] = "11"; row62["STG"] = "3"; row62["CSTID"] = "CFHA539574"; row62["LOTID"] = "509957346"; row62["DUMMY_FLAG"] = "N"; row62["EIOSTAT"] = "I"; row62["EQP_OP_STATUS_CD"] = "R"; row62["RUN_MODE_CD"] = "C"; row62["PROCID"] = "0"; row62["PROCNAME"] = "Start"; row62["NEXT_PROCID"] = "171"; row62["FORMSTATUS"] = "10"; row62["PROD_LOTID"] = "H05CD10LN1"; row62["OP_START_TIME"] = "2023-04-10 13:48"; row62["JOB_TIME"] = "2023-04-10 13:48"; row62["NOW_TIME"] = "2023-04-10 13:48"; row62["MAX_TEMP"] = "31"; row62["MIN_TEMP"] = "30.5"; row62["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row62);
            DataRow row63 = dt.NewRow(); row63["EQPTID"] = "FL11020010"; row63["ROW"] = "21"; row63["COL"] = "11"; row63["STG"] = "2"; row63["CSTID"] = "CFHA466694"; row63["LOTID"] = "509957331"; row63["DUMMY_FLAG"] = "N"; row63["EIOSTAT"] = "R"; row63["EQP_OP_STATUS_CD"] = ""; row63["RUN_MODE_CD"] = "C"; row63["PROCID"] = "171"; row63["PROCNAME"] = "低?流??#1"; row63["NEXT_PROCID"] = "191"; row63["FORMSTATUS"] = "15"; row63["PROD_LOTID"] = "H05CD10LN1"; row63["OP_START_TIME"] = "2023-04-10 13:48"; row63["JOB_TIME"] = "2023-04-10 13:48"; row63["NOW_TIME"] = "2023-04-10 13:48"; row63["MAX_TEMP"] = "30.5"; row63["MIN_TEMP"] = "30.5"; row63["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row63);
            DataRow row64 = dt.NewRow(); row64["EQPTID"] = "FL11010010"; row64["ROW"] = "21"; row64["COL"] = "11"; row64["STG"] = "1"; row64["CSTID"] = "CFHA469046"; row64["LOTID"] = "509957320"; row64["DUMMY_FLAG"] = "N"; row64["EIOSTAT"] = "R"; row64["EQP_OP_STATUS_CD"] = ""; row64["RUN_MODE_CD"] = "C"; row64["PROCID"] = "171"; row64["PROCNAME"] = "低?流??#1"; row64["NEXT_PROCID"] = "191"; row64["FORMSTATUS"] = "15"; row64["PROD_LOTID"] = "H05CD10LN1"; row64["OP_START_TIME"] = "2023-04-10 13:48"; row64["JOB_TIME"] = "2023-04-10 13:48"; row64["NOW_TIME"] = "2023-04-10 13:48"; row64["MAX_TEMP"] = "30.5"; row64["MIN_TEMP"] = "30"; row64["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row64);
            DataRow row65 = dt.NewRow(); row65["EQPTID"] = "FM11080010"; row65["ROW"] = "22"; row65["COL"] = "12"; row65["STG"] = "8"; row65["CSTID"] = ""; row65["LOTID"] = ""; row65["DUMMY_FLAG"] = ""; row65["EIOSTAT"] = "I"; row65["EQP_OP_STATUS_CD"] = "L"; row65["RUN_MODE_CD"] = "C"; row65["PROCID"] = ""; row65["PROCNAME"] = ""; row65["NEXT_PROCID"] = ""; row65["FORMSTATUS"] = "11"; row65["PROD_LOTID"] = ""; row65["OP_START_TIME"] = "2023-04-10 13:48"; row65["JOB_TIME"] = "2023-04-10 13:48"; row65["NOW_TIME"] = "2023-04-10 13:48"; row65["MAX_TEMP"] = "30.5"; row65["MIN_TEMP"] = "30.5"; row65["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row65);
            DataRow row66 = dt.NewRow(); row66["EQPTID"] = "FM11070010"; row66["ROW"] = "22"; row66["COL"] = "12"; row66["STG"] = "7"; row66["CSTID"] = "CFHA486484"; row66["LOTID"] = "509957342"; row66["DUMMY_FLAG"] = "N"; row66["EIOSTAT"] = "I"; row66["EQP_OP_STATUS_CD"] = "R"; row66["RUN_MODE_CD"] = "C"; row66["PROCID"] = "0"; row66["PROCNAME"] = "Start"; row66["NEXT_PROCID"] = "171"; row66["FORMSTATUS"] = "10"; row66["PROD_LOTID"] = "H05CD10MN1"; row66["OP_START_TIME"] = "2023-04-10 13:48"; row66["JOB_TIME"] = "2023-04-10 13:48"; row66["NOW_TIME"] = "2023-04-10 13:48"; row66["MAX_TEMP"] = "30.5"; row66["MIN_TEMP"] = "30"; row66["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row66);
            DataRow row67 = dt.NewRow(); row67["EQPTID"] = "FM11060010"; row67["ROW"] = "22"; row67["COL"] = "12"; row67["STG"] = "6"; row67["CSTID"] = "CFHA559695"; row67["LOTID"] = "509957330"; row67["DUMMY_FLAG"] = "N"; row67["EIOSTAT"] = "R"; row67["EQP_OP_STATUS_CD"] = ""; row67["RUN_MODE_CD"] = "C"; row67["PROCID"] = "171"; row67["PROCNAME"] = "低?流??#1"; row67["NEXT_PROCID"] = "191"; row67["FORMSTATUS"] = "15"; row67["PROD_LOTID"] = "H05CD10MN1"; row67["OP_START_TIME"] = "2023-04-10 13:48"; row67["JOB_TIME"] = "2023-04-10 13:48"; row67["NOW_TIME"] = "2023-04-10 13:48"; row67["MAX_TEMP"] = "31"; row67["MIN_TEMP"] = "31"; row67["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row67);
            DataRow row68 = dt.NewRow(); row68["EQPTID"] = "FM11050010"; row68["ROW"] = "22"; row68["COL"] = "12"; row68["STG"] = "5"; row68["CSTID"] = "CFHA424928"; row68["LOTID"] = "509957318"; row68["DUMMY_FLAG"] = "N"; row68["EIOSTAT"] = "R"; row68["EQP_OP_STATUS_CD"] = ""; row68["RUN_MODE_CD"] = "C"; row68["PROCID"] = "171"; row68["PROCNAME"] = "低?流??#1"; row68["NEXT_PROCID"] = "191"; row68["FORMSTATUS"] = "15"; row68["PROD_LOTID"] = "H05CD10MN1"; row68["OP_START_TIME"] = "2023-04-10 13:48"; row68["JOB_TIME"] = "2023-04-10 13:48"; row68["NOW_TIME"] = "2023-04-10 13:48"; row68["MAX_TEMP"] = "30.5"; row68["MIN_TEMP"] = "30"; row68["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row68);
            DataRow row69 = dt.NewRow(); row69["EQPTID"] = "FM11040010"; row69["ROW"] = "22"; row69["COL"] = "12"; row69["STG"] = "4"; row69["CSTID"] = "CFHA426592"; row69["LOTID"] = "509957307"; row69["DUMMY_FLAG"] = "N"; row69["EIOSTAT"] = "R"; row69["EQP_OP_STATUS_CD"] = ""; row69["RUN_MODE_CD"] = "C"; row69["PROCID"] = "171"; row69["PROCNAME"] = "低?流??#1"; row69["NEXT_PROCID"] = "191"; row69["FORMSTATUS"] = "15"; row69["PROD_LOTID"] = "H05CD10MN1"; row69["OP_START_TIME"] = "2023-04-10 13:48"; row69["JOB_TIME"] = "2023-04-10 13:48"; row69["NOW_TIME"] = "2023-04-10 13:48"; row69["MAX_TEMP"] = "31"; row69["MIN_TEMP"] = "30"; row69["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row69);
            DataRow row70 = dt.NewRow(); row70["EQPTID"] = "FM11030010"; row70["ROW"] = "22"; row70["COL"] = "12"; row70["STG"] = "3"; row70["CSTID"] = "CFHA368993"; row70["LOTID"] = "509957292"; row70["DUMMY_FLAG"] = "N"; row70["EIOSTAT"] = "R"; row70["EQP_OP_STATUS_CD"] = ""; row70["RUN_MODE_CD"] = "C"; row70["PROCID"] = "171"; row70["PROCNAME"] = "低?流??#1"; row70["NEXT_PROCID"] = "191"; row70["FORMSTATUS"] = "15"; row70["PROD_LOTID"] = "H05CD10MN1"; row70["OP_START_TIME"] = "2023-04-10 13:48"; row70["JOB_TIME"] = "2023-04-10 13:48"; row70["NOW_TIME"] = "2023-04-10 13:48"; row70["MAX_TEMP"] = "30.5"; row70["MIN_TEMP"] = "30"; row70["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row70);
            DataRow row71 = dt.NewRow(); row71["EQPTID"] = "FM11020010"; row71["ROW"] = "22"; row71["COL"] = "12"; row71["STG"] = "2"; row71["CSTID"] = "CFHA559812"; row71["LOTID"] = "509957267"; row71["DUMMY_FLAG"] = "N"; row71["EIOSTAT"] = "R"; row71["EQP_OP_STATUS_CD"] = ""; row71["RUN_MODE_CD"] = "C"; row71["PROCID"] = "191"; row71["PROCNAME"] = "Pre-Charge #1"; row71["NEXT_PROCID"] = "B11"; row71["FORMSTATUS"] = "12"; row71["PROD_LOTID"] = "H05CD10MN1"; row71["OP_START_TIME"] = "2023-04-10 13:48"; row71["JOB_TIME"] = "2023-04-10 13:48"; row71["NOW_TIME"] = "2023-04-10 13:48"; row71["MAX_TEMP"] = "30.5"; row71["MIN_TEMP"] = "29.5"; row71["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row71);
            DataRow row72 = dt.NewRow(); row72["EQPTID"] = "FM11010010"; row72["ROW"] = "22"; row72["COL"] = "12"; row72["STG"] = "1"; row72["CSTID"] = "CFHA445966"; row72["LOTID"] = "509957280"; row72["DUMMY_FLAG"] = "N"; row72["EIOSTAT"] = "R"; row72["EQP_OP_STATUS_CD"] = ""; row72["RUN_MODE_CD"] = "C"; row72["PROCID"] = "171"; row72["PROCNAME"] = "低?流??#1"; row72["NEXT_PROCID"] = "191"; row72["FORMSTATUS"] = "15"; row72["PROD_LOTID"] = "H05CD10MN1"; row72["OP_START_TIME"] = "2023-04-10 13:48"; row72["JOB_TIME"] = "2023-04-10 13:48"; row72["NOW_TIME"] = "2023-04-10 13:48"; row72["MAX_TEMP"] = "30.5"; row72["MIN_TEMP"] = "30.5"; row72["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row72);
            DataRow row73 = dt.NewRow(); row73["EQPTID"] = "FN11080010"; row73["ROW"] = "23"; row73["COL"] = "13"; row73["STG"] = "8"; row73["CSTID"] = "CFHA452201"; row73["LOTID"] = "509957275"; row73["DUMMY_FLAG"] = "N"; row73["EIOSTAT"] = "R"; row73["EQP_OP_STATUS_CD"] = ""; row73["RUN_MODE_CD"] = "C"; row73["PROCID"] = "191"; row73["PROCNAME"] = "Pre-Charge #1"; row73["NEXT_PROCID"] = "B11"; row73["FORMSTATUS"] = "12"; row73["PROD_LOTID"] = "H05CD10NN1"; row73["OP_START_TIME"] = "2023-04-10 13:48"; row73["JOB_TIME"] = "2023-04-10 13:48"; row73["NOW_TIME"] = "2023-04-10 13:48"; row73["MAX_TEMP"] = "31"; row73["MIN_TEMP"] = "30.5"; row73["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row73);
            DataRow row74 = dt.NewRow(); row74["EQPTID"] = "FN11070010"; row74["ROW"] = "23"; row74["COL"] = "13"; row74["STG"] = "7"; row74["CSTID"] = "CFHA418243"; row74["LOTID"] = "509957254"; row74["DUMMY_FLAG"] = "N"; row74["EIOSTAT"] = "I"; row74["EQP_OP_STATUS_CD"] = ""; row74["RUN_MODE_CD"] = "C"; row74["PROCID"] = "B11"; row74["PROCNAME"] = "判定 #1"; row74["NEXT_PROCID"] = "6C1"; row74["FORMSTATUS"] = "11"; row74["PROD_LOTID"] = "H05CD10NN1"; row74["OP_START_TIME"] = "2023-04-10 13:48"; row74["JOB_TIME"] = "2023-04-10 13:48"; row74["NOW_TIME"] = "2023-04-10 13:48"; row74["MAX_TEMP"] = "30.5"; row74["MIN_TEMP"] = "30.5"; row74["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row74);
            DataRow row75 = dt.NewRow(); row75["EQPTID"] = "FN11060010"; row75["ROW"] = "23"; row75["COL"] = "13"; row75["STG"] = "6"; row75["CSTID"] = "CFHA503556"; row75["LOTID"] = "509957339"; row75["DUMMY_FLAG"] = "N"; row75["EIOSTAT"] = "R"; row75["EQP_OP_STATUS_CD"] = ""; row75["RUN_MODE_CD"] = "C"; row75["PROCID"] = "171"; row75["PROCNAME"] = "低?流??#1"; row75["NEXT_PROCID"] = "191"; row75["FORMSTATUS"] = "15"; row75["PROD_LOTID"] = "H05CD10NN1"; row75["OP_START_TIME"] = "2023-04-10 13:48"; row75["JOB_TIME"] = "2023-04-10 13:48"; row75["NOW_TIME"] = "2023-04-10 13:48"; row75["MAX_TEMP"] = "31"; row75["MIN_TEMP"] = "30"; row75["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row75);
            DataRow row76 = dt.NewRow(); row76["EQPTID"] = "FN11050010"; row76["ROW"] = "23"; row76["COL"] = "13"; row76["STG"] = "5"; row76["CSTID"] = "CFHA315353"; row76["LOTID"] = "509957328"; row76["DUMMY_FLAG"] = "N"; row76["EIOSTAT"] = "R"; row76["EQP_OP_STATUS_CD"] = ""; row76["RUN_MODE_CD"] = "C"; row76["PROCID"] = "171"; row76["PROCNAME"] = "低?流??#1"; row76["NEXT_PROCID"] = "191"; row76["FORMSTATUS"] = "15"; row76["PROD_LOTID"] = "H05CD10NN1"; row76["OP_START_TIME"] = "2023-04-10 13:48"; row76["JOB_TIME"] = "2023-04-10 13:48"; row76["NOW_TIME"] = "2023-04-10 13:48"; row76["MAX_TEMP"] = "30.5"; row76["MIN_TEMP"] = "30"; row76["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row76);
            DataRow row77 = dt.NewRow(); row77["EQPTID"] = "FN11040010"; row77["ROW"] = "23"; row77["COL"] = "13"; row77["STG"] = "4"; row77["CSTID"] = "CFHA319269"; row77["LOTID"] = "509957302"; row77["DUMMY_FLAG"] = "N"; row77["EIOSTAT"] = "R"; row77["EQP_OP_STATUS_CD"] = ""; row77["RUN_MODE_CD"] = "C"; row77["PROCID"] = "171"; row77["PROCNAME"] = "低?流??#1"; row77["NEXT_PROCID"] = "191"; row77["FORMSTATUS"] = "15"; row77["PROD_LOTID"] = "H05CD10NN1"; row77["OP_START_TIME"] = "2023-04-10 13:48"; row77["JOB_TIME"] = "2023-04-10 13:48"; row77["NOW_TIME"] = "2023-04-10 13:48"; row77["MAX_TEMP"] = "29.5"; row77["MIN_TEMP"] = "29"; row77["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row77);
            DataRow row78 = dt.NewRow(); row78["EQPTID"] = "FN11030010"; row78["ROW"] = "23"; row78["COL"] = "13"; row78["STG"] = "3"; row78["CSTID"] = "CFHA312071"; row78["LOTID"] = "509957288"; row78["DUMMY_FLAG"] = "N"; row78["EIOSTAT"] = "R"; row78["EQP_OP_STATUS_CD"] = ""; row78["RUN_MODE_CD"] = "C"; row78["PROCID"] = "171"; row78["PROCNAME"] = "低?流??#1"; row78["NEXT_PROCID"] = "191"; row78["FORMSTATUS"] = "15"; row78["PROD_LOTID"] = "H05CD10NN1"; row78["OP_START_TIME"] = "2023-04-10 13:48"; row78["JOB_TIME"] = "2023-04-10 13:48"; row78["NOW_TIME"] = "2023-04-10 13:48"; row78["MAX_TEMP"] = "30.5"; row78["MIN_TEMP"] = "30"; row78["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row78);
            DataRow row79 = dt.NewRow(); row79["EQPTID"] = "FN11020010"; row79["ROW"] = "23"; row79["COL"] = "13"; row79["STG"] = "2"; row79["CSTID"] = "CFHA455037"; row79["LOTID"] = "509957351"; row79["DUMMY_FLAG"] = "N"; row79["EIOSTAT"] = "I"; row79["EQP_OP_STATUS_CD"] = "R"; row79["RUN_MODE_CD"] = "C"; row79["PROCID"] = "0"; row79["PROCNAME"] = "Start"; row79["NEXT_PROCID"] = "171"; row79["FORMSTATUS"] = "10"; row79["PROD_LOTID"] = "H05CD10NN1"; row79["OP_START_TIME"] = "2023-04-10 13:48"; row79["JOB_TIME"] = "2023-04-10 13:48"; row79["NOW_TIME"] = "2023-04-10 13:48"; row79["MAX_TEMP"] = "30"; row79["MIN_TEMP"] = "29.5"; row79["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row79);
            DataRow row80 = dt.NewRow(); row80["EQPTID"] = "FN11010010"; row80["ROW"] = "23"; row80["COL"] = "13"; row80["STG"] = "1"; row80["CSTID"] = "CFHA471992"; row80["LOTID"] = "509957315"; row80["DUMMY_FLAG"] = "N"; row80["EIOSTAT"] = "R"; row80["EQP_OP_STATUS_CD"] = ""; row80["RUN_MODE_CD"] = "C"; row80["PROCID"] = "171"; row80["PROCNAME"] = "低?流??#1"; row80["NEXT_PROCID"] = "191"; row80["FORMSTATUS"] = "15"; row80["PROD_LOTID"] = "H05CD10NN1"; row80["OP_START_TIME"] = "2023-04-10 13:48"; row80["JOB_TIME"] = "2023-04-10 13:48"; row80["NOW_TIME"] = "2023-04-10 13:48"; row80["MAX_TEMP"] = "30"; row80["MIN_TEMP"] = "29.5"; row80["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row80);
            DataRow row81 = dt.NewRow(); row81["EQPTID"] = "FO11080010"; row81["ROW"] = "24"; row81["COL"] = "14"; row81["STG"] = "8"; row81["CSTID"] = "CFHA488758"; row81["LOTID"] = "509957311"; row81["DUMMY_FLAG"] = "N"; row81["EIOSTAT"] = "R"; row81["EQP_OP_STATUS_CD"] = ""; row81["RUN_MODE_CD"] = "C"; row81["PROCID"] = "171"; row81["PROCNAME"] = "低?流??#1"; row81["NEXT_PROCID"] = "191"; row81["FORMSTATUS"] = "15"; row81["PROD_LOTID"] = "H05CD10ON2"; row81["OP_START_TIME"] = "2023-04-10 13:48"; row81["JOB_TIME"] = "2023-04-10 13:48"; row81["NOW_TIME"] = "2023-04-10 13:48"; row81["MAX_TEMP"] = "30"; row81["MIN_TEMP"] = "29.5"; row81["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row81);
            DataRow row82 = dt.NewRow(); row82["EQPTID"] = "FO11070010"; row82["ROW"] = "24"; row82["COL"] = "14"; row82["STG"] = "7"; row82["CSTID"] = "CFHA516580"; row82["LOTID"] = "509957297"; row82["DUMMY_FLAG"] = "N"; row82["EIOSTAT"] = "R"; row82["EQP_OP_STATUS_CD"] = ""; row82["RUN_MODE_CD"] = "C"; row82["PROCID"] = "171"; row82["PROCNAME"] = "低?流??#1"; row82["NEXT_PROCID"] = "191"; row82["FORMSTATUS"] = "15"; row82["PROD_LOTID"] = "H05CD10ON2"; row82["OP_START_TIME"] = "2023-04-10 13:48"; row82["JOB_TIME"] = "2023-04-10 13:48"; row82["NOW_TIME"] = "2023-04-10 13:48"; row82["MAX_TEMP"] = "30"; row82["MIN_TEMP"] = "29.5"; row82["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row82);
            DataRow row83 = dt.NewRow(); row83["EQPTID"] = "FO11060010"; row83["ROW"] = "24"; row83["COL"] = "14"; row83["STG"] = "6"; row83["CSTID"] = "CFHA592217"; row83["LOTID"] = "509957284"; row83["DUMMY_FLAG"] = "N"; row83["EIOSTAT"] = "R"; row83["EQP_OP_STATUS_CD"] = ""; row83["RUN_MODE_CD"] = "C"; row83["PROCID"] = "171"; row83["PROCNAME"] = "低?流??#1"; row83["NEXT_PROCID"] = "191"; row83["FORMSTATUS"] = "15"; row83["PROD_LOTID"] = "H05CD10ON2"; row83["OP_START_TIME"] = "2023-04-10 13:48"; row83["JOB_TIME"] = "2023-04-10 13:48"; row83["NOW_TIME"] = "2023-04-10 13:48"; row83["MAX_TEMP"] = "30.5"; row83["MIN_TEMP"] = "30"; row83["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row83);
            DataRow row84 = dt.NewRow(); row84["EQPTID"] = "FO11050010"; row84["ROW"] = "24"; row84["COL"] = "14"; row84["STG"] = "5"; row84["CSTID"] = "CFHA330346"; row84["LOTID"] = "509957321"; row84["DUMMY_FLAG"] = "N"; row84["EIOSTAT"] = "R"; row84["EQP_OP_STATUS_CD"] = ""; row84["RUN_MODE_CD"] = "C"; row84["PROCID"] = "171"; row84["PROCNAME"] = "低?流??#1"; row84["NEXT_PROCID"] = "191"; row84["FORMSTATUS"] = "15"; row84["PROD_LOTID"] = "H05CD10ON2"; row84["OP_START_TIME"] = "2023-04-10 13:48"; row84["JOB_TIME"] = "2023-04-10 13:48"; row84["NOW_TIME"] = "2023-04-10 13:48"; row84["MAX_TEMP"] = "30.5"; row84["MIN_TEMP"] = "30"; row84["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row84);
            DataRow row85 = dt.NewRow(); row85["EQPTID"] = "FO11040010"; row85["ROW"] = "24"; row85["COL"] = "14"; row85["STG"] = "4"; row85["CSTID"] = "CFHA546690"; row85["LOTID"] = "509957347"; row85["DUMMY_FLAG"] = "N"; row85["EIOSTAT"] = "I"; row85["EQP_OP_STATUS_CD"] = "R"; row85["RUN_MODE_CD"] = "C"; row85["PROCID"] = "0"; row85["PROCNAME"] = "Start"; row85["NEXT_PROCID"] = "171"; row85["FORMSTATUS"] = "10"; row85["PROD_LOTID"] = "H05CD10ON2"; row85["OP_START_TIME"] = "2023-04-10 13:48"; row85["JOB_TIME"] = "2023-04-10 13:48"; row85["NOW_TIME"] = "2023-04-10 13:48"; row85["MAX_TEMP"] = "30"; row85["MIN_TEMP"] = "29.5"; row85["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row85);
            DataRow row86 = dt.NewRow(); row86["EQPTID"] = "FO11030010"; row86["ROW"] = "24"; row86["COL"] = "14"; row86["STG"] = "3"; row86["CSTID"] = "CFHA422458"; row86["LOTID"] = "509957334"; row86["DUMMY_FLAG"] = "N"; row86["EIOSTAT"] = "R"; row86["EQP_OP_STATUS_CD"] = ""; row86["RUN_MODE_CD"] = "C"; row86["PROCID"] = "171"; row86["PROCNAME"] = "低?流??#1"; row86["NEXT_PROCID"] = "191"; row86["FORMSTATUS"] = "15"; row86["PROD_LOTID"] = "H05CD10ON2"; row86["OP_START_TIME"] = "2023-04-10 13:48"; row86["JOB_TIME"] = "2023-04-10 13:48"; row86["NOW_TIME"] = "2023-04-10 13:48"; row86["MAX_TEMP"] = "30"; row86["MIN_TEMP"] = "29.5"; row86["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row86);
            DataRow row87 = dt.NewRow(); row87["EQPTID"] = "FO11020010"; row87["ROW"] = "24"; row87["COL"] = "14"; row87["STG"] = "2"; row87["CSTID"] = "CFHA397237"; row87["LOTID"] = "509957271"; row87["DUMMY_FLAG"] = "N"; row87["EIOSTAT"] = "R"; row87["EQP_OP_STATUS_CD"] = ""; row87["RUN_MODE_CD"] = "C"; row87["PROCID"] = "191"; row87["PROCNAME"] = "Pre-Charge #1"; row87["NEXT_PROCID"] = "B11"; row87["FORMSTATUS"] = "12"; row87["PROD_LOTID"] = "H05CD10ON2"; row87["OP_START_TIME"] = "2023-04-10 13:48"; row87["JOB_TIME"] = "2023-04-10 13:48"; row87["NOW_TIME"] = "2023-04-10 13:48"; row87["MAX_TEMP"] = "30"; row87["MIN_TEMP"] = "29.5"; row87["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row87);
            DataRow row88 = dt.NewRow(); row88["EQPTID"] = "FO11010010"; row88["ROW"] = "24"; row88["COL"] = "14"; row88["STG"] = "1"; row88["CSTID"] = "CFHA464428"; row88["LOTID"] = "509957260"; row88["DUMMY_FLAG"] = "N"; row88["EIOSTAT"] = "I"; row88["EQP_OP_STATUS_CD"] = "U"; row88["RUN_MODE_CD"] = "C"; row88["PROCID"] = "B11"; row88["PROCNAME"] = "判定 #1"; row88["NEXT_PROCID"] = "6C1"; row88["FORMSTATUS"] = "18"; row88["PROD_LOTID"] = "H05CD10ON2"; row88["OP_START_TIME"] = "2023-04-10 13:48"; row88["JOB_TIME"] = "2023-04-10 13:48"; row88["NOW_TIME"] = "2023-04-10 13:48"; row88["MAX_TEMP"] = "29.5"; row88["MIN_TEMP"] = "29.5"; row88["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row88);
            DataRow row89 = dt.NewRow(); row89["EQPTID"] = "FP11080010"; row89["ROW"] = "25"; row89["COL"] = "15"; row89["STG"] = "8"; row89["CSTID"] = ""; row89["LOTID"] = ""; row89["DUMMY_FLAG"] = ""; row89["EIOSTAT"] = "I"; row89["EQP_OP_STATUS_CD"] = "L"; row89["RUN_MODE_CD"] = "C"; row89["PROCID"] = ""; row89["PROCNAME"] = ""; row89["NEXT_PROCID"] = ""; row89["FORMSTATUS"] = "11"; row89["PROD_LOTID"] = ""; row89["OP_START_TIME"] = "2023-04-10 13:48"; row89["JOB_TIME"] = "2023-04-10 13:48"; row89["NOW_TIME"] = "2023-04-10 13:48"; row89["MAX_TEMP"] = "26"; row89["MIN_TEMP"] = "26"; row89["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row89);
            DataRow row90 = dt.NewRow(); row90["EQPTID"] = "FP11070010"; row90["ROW"] = "25"; row90["COL"] = "15"; row90["STG"] = "7"; row90["CSTID"] = ""; row90["LOTID"] = ""; row90["DUMMY_FLAG"] = ""; row90["EIOSTAT"] = "I"; row90["EQP_OP_STATUS_CD"] = "L"; row90["RUN_MODE_CD"] = "C"; row90["PROCID"] = ""; row90["PROCNAME"] = ""; row90["NEXT_PROCID"] = ""; row90["FORMSTATUS"] = "11"; row90["PROD_LOTID"] = ""; row90["OP_START_TIME"] = "2023-04-10 13:48"; row90["JOB_TIME"] = "2023-04-10 13:48"; row90["NOW_TIME"] = "2023-04-10 13:48"; row90["MAX_TEMP"] = "26.5"; row90["MIN_TEMP"] = "26"; row90["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row90);
            DataRow row91 = dt.NewRow(); row91["EQPTID"] = "FP11060010"; row91["ROW"] = "25"; row91["COL"] = "15"; row91["STG"] = "6"; row91["CSTID"] = ""; row91["LOTID"] = ""; row91["DUMMY_FLAG"] = ""; row91["EIOSTAT"] = "I"; row91["EQP_OP_STATUS_CD"] = "L"; row91["RUN_MODE_CD"] = "C"; row91["PROCID"] = ""; row91["PROCNAME"] = ""; row91["NEXT_PROCID"] = ""; row91["FORMSTATUS"] = "11"; row91["PROD_LOTID"] = ""; row91["OP_START_TIME"] = "2023-04-10 13:48"; row91["JOB_TIME"] = "2023-04-10 13:48"; row91["NOW_TIME"] = "2023-04-10 13:48"; row91["MAX_TEMP"] = "25.5"; row91["MIN_TEMP"] = "24.5"; row91["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row91);
            DataRow row92 = dt.NewRow(); row92["EQPTID"] = "FP11050010"; row92["ROW"] = "25"; row92["COL"] = "15"; row92["STG"] = "5"; row92["CSTID"] = ""; row92["LOTID"] = ""; row92["DUMMY_FLAG"] = ""; row92["EIOSTAT"] = "I"; row92["EQP_OP_STATUS_CD"] = "L"; row92["RUN_MODE_CD"] = "C"; row92["PROCID"] = ""; row92["PROCNAME"] = ""; row92["NEXT_PROCID"] = ""; row92["FORMSTATUS"] = "11"; row92["PROD_LOTID"] = ""; row92["OP_START_TIME"] = "2023-04-10 13:48"; row92["JOB_TIME"] = "2023-04-10 13:48"; row92["NOW_TIME"] = "2023-04-10 13:48"; row92["MAX_TEMP"] = "25.5"; row92["MIN_TEMP"] = "25"; row92["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row92);
            DataRow row93 = dt.NewRow(); row93["EQPTID"] = "FP11040010"; row93["ROW"] = "25"; row93["COL"] = "15"; row93["STG"] = "4"; row93["CSTID"] = ""; row93["LOTID"] = ""; row93["DUMMY_FLAG"] = ""; row93["EIOSTAT"] = "I"; row93["EQP_OP_STATUS_CD"] = "L"; row93["RUN_MODE_CD"] = "C"; row93["PROCID"] = ""; row93["PROCNAME"] = ""; row93["NEXT_PROCID"] = ""; row93["FORMSTATUS"] = "11"; row93["PROD_LOTID"] = ""; row93["OP_START_TIME"] = "2023-04-10 13:48"; row93["JOB_TIME"] = "2023-04-10 13:48"; row93["NOW_TIME"] = "2023-04-10 13:48"; row93["MAX_TEMP"] = "24.5"; row93["MIN_TEMP"] = "24"; row93["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row93);
            DataRow row94 = dt.NewRow(); row94["EQPTID"] = "FP11030010"; row94["ROW"] = "25"; row94["COL"] = "15"; row94["STG"] = "3"; row94["CSTID"] = ""; row94["LOTID"] = ""; row94["DUMMY_FLAG"] = ""; row94["EIOSTAT"] = "I"; row94["EQP_OP_STATUS_CD"] = "L"; row94["RUN_MODE_CD"] = "C"; row94["PROCID"] = ""; row94["PROCNAME"] = ""; row94["NEXT_PROCID"] = ""; row94["FORMSTATUS"] = "11"; row94["PROD_LOTID"] = ""; row94["OP_START_TIME"] = "2023-04-10 13:48"; row94["JOB_TIME"] = "2023-04-10 13:48"; row94["NOW_TIME"] = "2023-04-10 13:48"; row94["MAX_TEMP"] = "25"; row94["MIN_TEMP"] = "24.5"; row94["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row94);
            DataRow row95 = dt.NewRow(); row95["EQPTID"] = "FP11020010"; row95["ROW"] = "25"; row95["COL"] = "15"; row95["STG"] = "2"; row95["CSTID"] = ""; row95["LOTID"] = ""; row95["DUMMY_FLAG"] = ""; row95["EIOSTAT"] = "I"; row95["EQP_OP_STATUS_CD"] = "L"; row95["RUN_MODE_CD"] = "C"; row95["PROCID"] = ""; row95["PROCNAME"] = ""; row95["NEXT_PROCID"] = ""; row95["FORMSTATUS"] = "11"; row95["PROD_LOTID"] = ""; row95["OP_START_TIME"] = "2023-04-10 13:48"; row95["JOB_TIME"] = "2023-04-10 13:48"; row95["NOW_TIME"] = "2023-04-10 13:48"; row95["MAX_TEMP"] = "25"; row95["MIN_TEMP"] = "24.5"; row95["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row95);
            DataRow row96 = dt.NewRow(); row96["EQPTID"] = "FP11010010"; row96["ROW"] = "25"; row96["COL"] = "15"; row96["STG"] = "1"; row96["CSTID"] = ""; row96["LOTID"] = ""; row96["DUMMY_FLAG"] = ""; row96["EIOSTAT"] = "I"; row96["EQP_OP_STATUS_CD"] = "L"; row96["RUN_MODE_CD"] = "C"; row96["PROCID"] = ""; row96["PROCNAME"] = ""; row96["NEXT_PROCID"] = ""; row96["FORMSTATUS"] = "11"; row96["PROD_LOTID"] = ""; row96["OP_START_TIME"] = "2023-04-10 13:48"; row96["JOB_TIME"] = "2023-04-10 13:48"; row96["NOW_TIME"] = "2023-04-10 13:48"; row96["MAX_TEMP"] = "26"; row96["MIN_TEMP"] = "26"; row96["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row96);
            DataRow row97 = dt.NewRow(); row97["EQPTID"] = "FQ11080010"; row97["ROW"] = "26"; row97["COL"] = "16"; row97["STG"] = "8"; row97["CSTID"] = ""; row97["LOTID"] = ""; row97["DUMMY_FLAG"] = ""; row97["EIOSTAT"] = "I"; row97["EQP_OP_STATUS_CD"] = "L"; row97["RUN_MODE_CD"] = "C"; row97["PROCID"] = ""; row97["PROCNAME"] = ""; row97["NEXT_PROCID"] = ""; row97["FORMSTATUS"] = "11"; row97["PROD_LOTID"] = ""; row97["OP_START_TIME"] = "2023-04-10 13:48"; row97["JOB_TIME"] = "2023-04-10 13:48"; row97["NOW_TIME"] = "2023-04-10 13:48"; row97["MAX_TEMP"] = "25"; row97["MIN_TEMP"] = "25"; row97["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row97);
            DataRow row98 = dt.NewRow(); row98["EQPTID"] = "FQ11070010"; row98["ROW"] = "26"; row98["COL"] = "16"; row98["STG"] = "7"; row98["CSTID"] = ""; row98["LOTID"] = ""; row98["DUMMY_FLAG"] = ""; row98["EIOSTAT"] = "I"; row98["EQP_OP_STATUS_CD"] = "L"; row98["RUN_MODE_CD"] = "C"; row98["PROCID"] = ""; row98["PROCNAME"] = ""; row98["NEXT_PROCID"] = ""; row98["FORMSTATUS"] = "11"; row98["PROD_LOTID"] = ""; row98["OP_START_TIME"] = "2023-04-10 13:48"; row98["JOB_TIME"] = "2023-04-10 13:48"; row98["NOW_TIME"] = "2023-04-10 13:48"; row98["MAX_TEMP"] = "25.5"; row98["MIN_TEMP"] = "25"; row98["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row98);
            DataRow row99 = dt.NewRow(); row99["EQPTID"] = "FQ11060010"; row99["ROW"] = "26"; row99["COL"] = "16"; row99["STG"] = "6"; row99["CSTID"] = ""; row99["LOTID"] = ""; row99["DUMMY_FLAG"] = ""; row99["EIOSTAT"] = "I"; row99["EQP_OP_STATUS_CD"] = "L"; row99["RUN_MODE_CD"] = "C"; row99["PROCID"] = ""; row99["PROCNAME"] = ""; row99["NEXT_PROCID"] = ""; row99["FORMSTATUS"] = "11"; row99["PROD_LOTID"] = ""; row99["OP_START_TIME"] = "2023-04-10 13:48"; row99["JOB_TIME"] = "2023-04-10 13:48"; row99["NOW_TIME"] = "2023-04-10 13:48"; row99["MAX_TEMP"] = "24.5"; row99["MIN_TEMP"] = "24"; row99["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row99);
            DataRow row100 = dt.NewRow(); row100["EQPTID"] = "FQ11050010"; row100["ROW"] = "26"; row100["COL"] = "16"; row100["STG"] = "5"; row100["CSTID"] = ""; row100["LOTID"] = ""; row100["DUMMY_FLAG"] = ""; row100["EIOSTAT"] = "I"; row100["EQP_OP_STATUS_CD"] = "L"; row100["RUN_MODE_CD"] = "C"; row100["PROCID"] = ""; row100["PROCNAME"] = ""; row100["NEXT_PROCID"] = ""; row100["FORMSTATUS"] = "11"; row100["PROD_LOTID"] = ""; row100["OP_START_TIME"] = "2023-04-10 13:48"; row100["JOB_TIME"] = "2023-04-10 13:48"; row100["NOW_TIME"] = "2023-04-10 13:48"; row100["MAX_TEMP"] = "24.5"; row100["MIN_TEMP"] = "24"; row100["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row100);
            DataRow row101 = dt.NewRow(); row101["EQPTID"] = "FQ11040010"; row101["ROW"] = "26"; row101["COL"] = "16"; row101["STG"] = "4"; row101["CSTID"] = ""; row101["LOTID"] = ""; row101["DUMMY_FLAG"] = ""; row101["EIOSTAT"] = "I"; row101["EQP_OP_STATUS_CD"] = "L"; row101["RUN_MODE_CD"] = "C"; row101["PROCID"] = ""; row101["PROCNAME"] = ""; row101["NEXT_PROCID"] = ""; row101["FORMSTATUS"] = "11"; row101["PROD_LOTID"] = ""; row101["OP_START_TIME"] = "2023-04-10 13:48"; row101["JOB_TIME"] = "2023-04-10 13:48"; row101["NOW_TIME"] = "2023-04-10 13:48"; row101["MAX_TEMP"] = "23.5"; row101["MIN_TEMP"] = "23"; row101["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row101);
            DataRow row102 = dt.NewRow(); row102["EQPTID"] = "FQ11030010"; row102["ROW"] = "26"; row102["COL"] = "16"; row102["STG"] = "3"; row102["CSTID"] = ""; row102["LOTID"] = ""; row102["DUMMY_FLAG"] = ""; row102["EIOSTAT"] = "I"; row102["EQP_OP_STATUS_CD"] = "L"; row102["RUN_MODE_CD"] = "C"; row102["PROCID"] = ""; row102["PROCNAME"] = ""; row102["NEXT_PROCID"] = ""; row102["FORMSTATUS"] = "11"; row102["PROD_LOTID"] = ""; row102["OP_START_TIME"] = "2023-04-10 13:48"; row102["JOB_TIME"] = "2023-04-10 13:48"; row102["NOW_TIME"] = "2023-04-10 13:48"; row102["MAX_TEMP"] = "24"; row102["MIN_TEMP"] = "23.5"; row102["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row102);
            DataRow row103 = dt.NewRow(); row103["EQPTID"] = "FQ11020010"; row103["ROW"] = "26"; row103["COL"] = "16"; row103["STG"] = "2"; row103["CSTID"] = ""; row103["LOTID"] = ""; row103["DUMMY_FLAG"] = ""; row103["EIOSTAT"] = "I"; row103["EQP_OP_STATUS_CD"] = "L"; row103["RUN_MODE_CD"] = "C"; row103["PROCID"] = ""; row103["PROCNAME"] = ""; row103["NEXT_PROCID"] = ""; row103["FORMSTATUS"] = "11"; row103["PROD_LOTID"] = ""; row103["OP_START_TIME"] = "2023-04-10 13:48"; row103["JOB_TIME"] = "2023-04-10 13:48"; row103["NOW_TIME"] = "2023-04-10 13:48"; row103["MAX_TEMP"] = "24"; row103["MIN_TEMP"] = "23.5"; row103["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row103);
            DataRow row104 = dt.NewRow(); row104["EQPTID"] = "FQ11010010"; row104["ROW"] = "26"; row104["COL"] = "16"; row104["STG"] = "1"; row104["CSTID"] = ""; row104["LOTID"] = ""; row104["DUMMY_FLAG"] = ""; row104["EIOSTAT"] = "I"; row104["EQP_OP_STATUS_CD"] = "L"; row104["RUN_MODE_CD"] = "C"; row104["PROCID"] = ""; row104["PROCNAME"] = ""; row104["NEXT_PROCID"] = ""; row104["FORMSTATUS"] = "11"; row104["PROD_LOTID"] = ""; row104["OP_START_TIME"] = "2023-04-10 13:48"; row104["JOB_TIME"] = "2023-04-10 13:48"; row104["NOW_TIME"] = "2023-04-10 13:48"; row104["MAX_TEMP"] = "25"; row104["MIN_TEMP"] = "24.5"; row104["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row104);
            DataRow row105 = dt.NewRow(); row105["EQPTID"] = "FR11080010"; row105["ROW"] = "27"; row105["COL"] = "17"; row105["STG"] = "8"; row105["CSTID"] = ""; row105["LOTID"] = ""; row105["DUMMY_FLAG"] = ""; row105["EIOSTAT"] = "I"; row105["EQP_OP_STATUS_CD"] = "L"; row105["RUN_MODE_CD"] = "C"; row105["PROCID"] = ""; row105["PROCNAME"] = ""; row105["NEXT_PROCID"] = ""; row105["FORMSTATUS"] = "11"; row105["PROD_LOTID"] = ""; row105["OP_START_TIME"] = "2023-04-10 13:48"; row105["JOB_TIME"] = "2023-04-10 13:48"; row105["NOW_TIME"] = "2023-04-10 13:48"; row105["MAX_TEMP"] = "28.5"; row105["MIN_TEMP"] = "28"; row105["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row105);
            DataRow row106 = dt.NewRow(); row106["EQPTID"] = "FR11070010"; row106["ROW"] = "27"; row106["COL"] = "17"; row106["STG"] = "7"; row106["CSTID"] = ""; row106["LOTID"] = ""; row106["DUMMY_FLAG"] = ""; row106["EIOSTAT"] = "I"; row106["EQP_OP_STATUS_CD"] = "L"; row106["RUN_MODE_CD"] = "C"; row106["PROCID"] = ""; row106["PROCNAME"] = ""; row106["NEXT_PROCID"] = ""; row106["FORMSTATUS"] = "11"; row106["PROD_LOTID"] = ""; row106["OP_START_TIME"] = "2023-04-10 13:48"; row106["JOB_TIME"] = "2023-04-10 13:48"; row106["NOW_TIME"] = "2023-04-10 13:48"; row106["MAX_TEMP"] = "28.5"; row106["MIN_TEMP"] = "28"; row106["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row106);
            DataRow row107 = dt.NewRow(); row107["EQPTID"] = "FR11060010"; row107["ROW"] = "27"; row107["COL"] = "17"; row107["STG"] = "6"; row107["CSTID"] = "CFHA326033"; row107["LOTID"] = "509957309"; row107["DUMMY_FLAG"] = "N"; row107["EIOSTAT"] = "R"; row107["EQP_OP_STATUS_CD"] = ""; row107["RUN_MODE_CD"] = "C"; row107["PROCID"] = "171"; row107["PROCNAME"] = "低?流??#1"; row107["NEXT_PROCID"] = "191"; row107["FORMSTATUS"] = "15"; row107["PROD_LOTID"] = "H05CD10RN1"; row107["OP_START_TIME"] = "2023-04-10 13:48"; row107["JOB_TIME"] = "2023-04-10 13:48"; row107["NOW_TIME"] = "2023-04-10 13:48"; row107["MAX_TEMP"] = "28.5"; row107["MIN_TEMP"] = "28"; row107["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row107);
            DataRow row108 = dt.NewRow(); row108["EQPTID"] = "FR11050010"; row108["ROW"] = "27"; row108["COL"] = "17"; row108["STG"] = "5"; row108["CSTID"] = "CFHA560604"; row108["LOTID"] = "509957300"; row108["DUMMY_FLAG"] = "N"; row108["EIOSTAT"] = "R"; row108["EQP_OP_STATUS_CD"] = ""; row108["RUN_MODE_CD"] = "C"; row108["PROCID"] = "171"; row108["PROCNAME"] = "低?流??#1"; row108["NEXT_PROCID"] = "191"; row108["FORMSTATUS"] = "15"; row108["PROD_LOTID"] = "H05CD10RN1"; row108["OP_START_TIME"] = "2023-04-10 13:48"; row108["JOB_TIME"] = "2023-04-10 13:48"; row108["NOW_TIME"] = "2023-04-10 13:48"; row108["MAX_TEMP"] = "28.5"; row108["MIN_TEMP"] = "28"; row108["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row108);
            DataRow row109 = dt.NewRow(); row109["EQPTID"] = "FR11040010"; row109["ROW"] = "27"; row109["COL"] = "17"; row109["STG"] = "4"; row109["CSTID"] = "CFHA411876"; row109["LOTID"] = "509957282"; row109["DUMMY_FLAG"] = "N"; row109["EIOSTAT"] = "R"; row109["EQP_OP_STATUS_CD"] = ""; row109["RUN_MODE_CD"] = "C"; row109["PROCID"] = "171"; row109["PROCNAME"] = "低?流??#1"; row109["NEXT_PROCID"] = "191"; row109["FORMSTATUS"] = "15"; row109["PROD_LOTID"] = "H05CD10RN1"; row109["OP_START_TIME"] = "2023-04-10 13:48"; row109["JOB_TIME"] = "2023-04-10 13:48"; row109["NOW_TIME"] = "2023-04-10 13:48"; row109["MAX_TEMP"] = "27.5"; row109["MIN_TEMP"] = "27"; row109["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row109);
            DataRow row110 = dt.NewRow(); row110["EQPTID"] = "FR11030010"; row110["ROW"] = "27"; row110["COL"] = "17"; row110["STG"] = "3"; row110["CSTID"] = "CFHA343713"; row110["LOTID"] = "509957261"; row110["DUMMY_FLAG"] = "N"; row110["EIOSTAT"] = "I"; row110["EQP_OP_STATUS_CD"] = "U"; row110["RUN_MODE_CD"] = "C"; row110["PROCID"] = "B11"; row110["PROCNAME"] = "判定 #1"; row110["NEXT_PROCID"] = "6C1"; row110["FORMSTATUS"] = "18"; row110["PROD_LOTID"] = "H05CD10RN1"; row110["OP_START_TIME"] = "2023-04-10 13:48"; row110["JOB_TIME"] = "2023-04-10 13:48"; row110["NOW_TIME"] = "2023-04-10 13:48"; row110["MAX_TEMP"] = "27.5"; row110["MIN_TEMP"] = "27.5"; row110["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row110);
            DataRow row111 = dt.NewRow(); row111["EQPTID"] = "FR11020010"; row111["ROW"] = "27"; row111["COL"] = "17"; row111["STG"] = "2"; row111["CSTID"] = "CFHA433337"; row111["LOTID"] = "509957344"; row111["DUMMY_FLAG"] = "N"; row111["EIOSTAT"] = "I"; row111["EQP_OP_STATUS_CD"] = "R"; row111["RUN_MODE_CD"] = "C"; row111["PROCID"] = "0"; row111["PROCNAME"] = "Start"; row111["NEXT_PROCID"] = "171"; row111["FORMSTATUS"] = "10"; row111["PROD_LOTID"] = "H05CD10RN1"; row111["OP_START_TIME"] = "2023-04-10 13:48"; row111["JOB_TIME"] = "2023-04-10 13:48"; row111["NOW_TIME"] = "2023-04-10 13:48"; row111["MAX_TEMP"] = "28"; row111["MIN_TEMP"] = "27.5"; row111["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row111);
            DataRow row112 = dt.NewRow(); row112["EQPTID"] = "FR11010010"; row112["ROW"] = "27"; row112["COL"] = "17"; row112["STG"] = "1"; row112["CSTID"] = "CFHA450765"; row112["LOTID"] = "509957332"; row112["DUMMY_FLAG"] = "N"; row112["EIOSTAT"] = "R"; row112["EQP_OP_STATUS_CD"] = ""; row112["RUN_MODE_CD"] = "C"; row112["PROCID"] = "171"; row112["PROCNAME"] = "低?流??#1"; row112["NEXT_PROCID"] = "191"; row112["FORMSTATUS"] = "15"; row112["PROD_LOTID"] = "H05CD10RN1"; row112["OP_START_TIME"] = "2023-04-10 13:48"; row112["JOB_TIME"] = "2023-04-10 13:48"; row112["NOW_TIME"] = "2023-04-10 13:48"; row112["MAX_TEMP"] = "27.5"; row112["MIN_TEMP"] = "27"; row112["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row112);
            DataRow row113 = dt.NewRow(); row113["EQPTID"] = "FS11080010"; row113["ROW"] = "28"; row113["COL"] = "18"; row113["STG"] = "8"; row113["CSTID"] = ""; row113["LOTID"] = ""; row113["DUMMY_FLAG"] = ""; row113["EIOSTAT"] = "I"; row113["EQP_OP_STATUS_CD"] = "L"; row113["RUN_MODE_CD"] = "C"; row113["PROCID"] = ""; row113["PROCNAME"] = ""; row113["NEXT_PROCID"] = ""; row113["FORMSTATUS"] = "11"; row113["PROD_LOTID"] = ""; row113["OP_START_TIME"] = "2023-04-10 13:48"; row113["JOB_TIME"] = "2023-04-10 13:48"; row113["NOW_TIME"] = "2023-04-10 13:48"; row113["MAX_TEMP"] = "27.5"; row113["MIN_TEMP"] = "27.5"; row113["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row113);
            DataRow row114 = dt.NewRow(); row114["EQPTID"] = "FS11070010"; row114["ROW"] = "28"; row114["COL"] = "18"; row114["STG"] = "7"; row114["CSTID"] = "CFHA493338"; row114["LOTID"] = "509957298"; row114["DUMMY_FLAG"] = "N"; row114["EIOSTAT"] = "R"; row114["EQP_OP_STATUS_CD"] = ""; row114["RUN_MODE_CD"] = "C"; row114["PROCID"] = "171"; row114["PROCNAME"] = "低?流??#1"; row114["NEXT_PROCID"] = "191"; row114["FORMSTATUS"] = "15"; row114["PROD_LOTID"] = "H05CD10SN1"; row114["OP_START_TIME"] = "2023-04-10 13:48"; row114["JOB_TIME"] = "2023-04-10 13:48"; row114["NOW_TIME"] = "2023-04-10 13:48"; row114["MAX_TEMP"] = "29.5"; row114["MIN_TEMP"] = "28.5"; row114["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row114);
            DataRow row115 = dt.NewRow(); row115["EQPTID"] = "FS11060010"; row115["ROW"] = "28"; row115["COL"] = "18"; row115["STG"] = "6"; row115["CSTID"] = ""; row115["LOTID"] = ""; row115["DUMMY_FLAG"] = ""; row115["EIOSTAT"] = "I"; row115["EQP_OP_STATUS_CD"] = "L"; row115["RUN_MODE_CD"] = "C"; row115["PROCID"] = ""; row115["PROCNAME"] = ""; row115["NEXT_PROCID"] = ""; row115["FORMSTATUS"] = "11"; row115["PROD_LOTID"] = ""; row115["OP_START_TIME"] = "2023-04-10 13:48"; row115["JOB_TIME"] = "2023-04-10 13:48"; row115["NOW_TIME"] = "2023-04-10 13:48"; row115["MAX_TEMP"] = "27.5"; row115["MIN_TEMP"] = "27"; row115["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row115);
            DataRow row116 = dt.NewRow(); row116["EQPTID"] = "FS11050010"; row116["ROW"] = "28"; row116["COL"] = "18"; row116["STG"] = "5"; row116["CSTID"] = "CFHA393619"; row116["LOTID"] = "509957349"; row116["DUMMY_FLAG"] = "N"; row116["EIOSTAT"] = "I"; row116["EQP_OP_STATUS_CD"] = "R"; row116["RUN_MODE_CD"] = "C"; row116["PROCID"] = "0"; row116["PROCNAME"] = "Start"; row116["NEXT_PROCID"] = "171"; row116["FORMSTATUS"] = "10"; row116["PROD_LOTID"] = "H05CD10SN1"; row116["OP_START_TIME"] = "2023-04-10 13:48"; row116["JOB_TIME"] = "2023-04-10 13:48"; row116["NOW_TIME"] = "2023-04-10 13:48"; row116["MAX_TEMP"] = "27.5"; row116["MIN_TEMP"] = "27"; row116["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row116);
            DataRow row117 = dt.NewRow(); row117["EQPTID"] = "FS11040010"; row117["ROW"] = "28"; row117["COL"] = "18"; row117["STG"] = "4"; row117["CSTID"] = "CFHA359163"; row117["LOTID"] = "509957324"; row117["DUMMY_FLAG"] = "N"; row117["EIOSTAT"] = "R"; row117["EQP_OP_STATUS_CD"] = ""; row117["RUN_MODE_CD"] = "C"; row117["PROCID"] = "171"; row117["PROCNAME"] = "低?流??#1"; row117["NEXT_PROCID"] = "191"; row117["FORMSTATUS"] = "15"; row117["PROD_LOTID"] = "H05CD10SN1"; row117["OP_START_TIME"] = "2023-04-10 13:48"; row117["JOB_TIME"] = "2023-04-10 13:48"; row117["NOW_TIME"] = "2023-04-10 13:48"; row117["MAX_TEMP"] = "26.5"; row117["MIN_TEMP"] = "26.5"; row117["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row117);
            DataRow row118 = dt.NewRow(); row118["EQPTID"] = "FS11030010"; row118["ROW"] = "28"; row118["COL"] = "18"; row118["STG"] = "3"; row118["CSTID"] = "CFHA465451"; row118["LOTID"] = "509957270"; row118["DUMMY_FLAG"] = "N"; row118["EIOSTAT"] = "R"; row118["EQP_OP_STATUS_CD"] = ""; row118["RUN_MODE_CD"] = "C"; row118["PROCID"] = "191"; row118["PROCNAME"] = "Pre-Charge #1"; row118["NEXT_PROCID"] = "B11"; row118["FORMSTATUS"] = "12"; row118["PROD_LOTID"] = "H05CD10SN1"; row118["OP_START_TIME"] = "2023-04-10 13:48"; row118["JOB_TIME"] = "2023-04-10 13:48"; row118["NOW_TIME"] = "2023-04-10 13:48"; row118["MAX_TEMP"] = "28"; row118["MIN_TEMP"] = "27.5"; row118["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row118);
            DataRow row119 = dt.NewRow(); row119["EQPTID"] = "FS11020010"; row119["ROW"] = "28"; row119["COL"] = "18"; row119["STG"] = "2"; row119["CSTID"] = "CFHA484687"; row119["LOTID"] = "509957335"; row119["DUMMY_FLAG"] = "N"; row119["EIOSTAT"] = "R"; row119["EQP_OP_STATUS_CD"] = ""; row119["RUN_MODE_CD"] = "C"; row119["PROCID"] = "171"; row119["PROCNAME"] = "低?流??#1"; row119["NEXT_PROCID"] = "191"; row119["FORMSTATUS"] = "15"; row119["PROD_LOTID"] = "H05CD10SN1"; row119["OP_START_TIME"] = "2023-04-10 13:48"; row119["JOB_TIME"] = "2023-04-10 13:48"; row119["NOW_TIME"] = "2023-04-10 13:48"; row119["MAX_TEMP"] = "26.5"; row119["MIN_TEMP"] = "26"; row119["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row119);
            DataRow row120 = dt.NewRow(); row120["EQPTID"] = "FS11010010"; row120["ROW"] = "28"; row120["COL"] = "18"; row120["STG"] = "1"; row120["CSTID"] = "CFHA435571"; row120["LOTID"] = "509957312"; row120["DUMMY_FLAG"] = "N"; row120["EIOSTAT"] = "R"; row120["EQP_OP_STATUS_CD"] = ""; row120["RUN_MODE_CD"] = "C"; row120["PROCID"] = "171"; row120["PROCNAME"] = "低?流??#1"; row120["NEXT_PROCID"] = "191"; row120["FORMSTATUS"] = "15"; row120["PROD_LOTID"] = "H05CD10SN1"; row120["OP_START_TIME"] = "2023-04-10 13:48"; row120["JOB_TIME"] = "2023-04-10 13:48"; row120["NOW_TIME"] = "2023-04-10 13:48"; row120["MAX_TEMP"] = "28"; row120["MIN_TEMP"] = "27.5"; row120["NEXT_PROC_DETL_TYPE_CODE"] = "A"; dt.Rows.Add(row120);

            return dt;
        }


        private void SetTempGridMCP()
        {
            // 파우치
            dgTemp.Columns["TMPR_PSTN_NAME"].Visibility = Visibility.Visible;
            dgTemp.Columns["TEMP09"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP10"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP11"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP12"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP13"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP14"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP15"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP16"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP17"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP18"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP19"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP20"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP21"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP22"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP23"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP24"].Visibility = Visibility.Collapsed;
            //     dgTemp.Height = 105;

        }

        private void SetTempGridNFF()
        {

            // nff
            dgTemp.Columns["TMPR_PSTN_NAME"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP09"].Visibility = Visibility.Visible;
            dgTemp.Columns["TEMP10"].Visibility = Visibility.Visible;
            dgTemp.Columns["TEMP11"].Visibility = Visibility.Visible;
            dgTemp.Columns["TEMP12"].Visibility = Visibility.Visible;
            dgTemp.Columns["TEMP13"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP14"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP15"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP16"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP17"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP18"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP19"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP20"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP21"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP22"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP23"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["TEMP24"].Visibility = Visibility.Collapsed;
            dgTemp.Columns["AVG_VALUE"].Visibility = Visibility.Collapsed;
            //    dgTemp.Height = 400;

        }



        private void GetTempNFF(string sEqpId)
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
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CPF_TEMP_MB", "RQSTDT", "RSLTDT", dtRqst);

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



                //int dtCol = 8;
                //for (int iRow = 0; iRow < 12; iRow++)
                //{
                //    DataRow dr1 = dtTemp.NewRow();

                //    for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
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
            //-------------------------------------------------------------------------
            // NFF Temp Data (12*12)

        }


        private string GetCellType()
        {
            //DA_BAS_SEL_AREA_COM_CODE_USE

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_SPCL_FN_MB";
            dr["COM_CODE"] = "FORM_TEMP";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count == 0)
                _CELLTYPE = "D";

            else
                _CELLTYPE = dtRslt.Rows[0]["ATTR1"].ToString();


            return _CELLTYPE;
        }
        #endregion

    }
}
