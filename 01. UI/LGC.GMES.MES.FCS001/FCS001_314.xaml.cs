/*************************************************************************************
 Created Date : 2023.01.04
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 대용량 충방전기 현황
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
    public partial class FCS001_314 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LANEID = string.Empty;


        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private DataTable _dtLegend;
        private DataTable _dtBackgroundCopy, _dtForeColorCopy;
        private string _MENUID = string.Empty;

        private Point prevRowPos = new Point(0, 0);
        private Point prevColPos = new Point(0, 0);

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        Util Util = new Util();
        private bool bUseFlag = false; //2023.08.15 NA1동 외부 대용량 충 방전기 그리드 추가
        private bool cUseFlag = false , dUseFlag =false; //2023.08.15 NA1동 충방전기 1단 라인 표시 및 한 화면에서 다른 레인 표시 구분
        private string FirstRow , SecondRow;

        // 속도 향상을 위해 반복문 내부 비즈 및 다국어 밖으로 뺌.
        private DataTable dtRepair = null;
        private string langYun = string.Empty;
        private string langDCIR = string.Empty;
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

        public FCS001_314()
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

                if (string.IsNullOrEmpty(sLaneID))
                    _LANEID = "X11";
                else
                    _LANEID = sLaneID;

                /// 2023.08.15

                bUseFlag = Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_001_NA"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if (bUseFlag)
                {

                    dgFormation.FontSize = 10;
                    IsFormationUse();
                    OuterMgformGrid.Visibility = Visibility.Visible;
                    OuterMgformGrid.UpdateLayout();
                    if (cUseFlag)
                        dgFormation.FontSize = 12;
                   
                     
                }
                else
                {
                    OuterMgformGrid.Visibility = Visibility.Collapsed;
                    OuterMgformGrid.UpdateLayout();
                }

                InitCombo();
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
                //langDCIR = ObjectDic.Instance.GetObjectName("고전압 대용량 방전기");
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

        #region Initialize
        //화면내 combo 셋팅
        private void InitCombo()
        {
            try
            {
                CommonCombo_Form _combo = new CommonCombo_Form();

                string[] filter = { "1", _LANEID };
                _combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW", sFilter: filter);

                //cboRow.Visibility = Visibility.Hidden;
                cboRow.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void dgLegend_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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


                    if (e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Value.ToString().Equals("CBO_NAME"))

                    {

                        if (e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value == null)
                        {
                            return;
                        }

                        if (e.Cell.Column.Index > 0)
                        {

                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new BrushConverter().ConvertFromString(_dtBackgroundCopy.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString()) as SolidColorBrush;
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new BrushConverter().ConvertFromString(_dtForeColorCopy.Rows[e.Cell.Row.Index][e.Cell.Column.Index].ToString()) as SolidColorBrush;

                        }

                    }
                }
            }));
        }

        private void cboRow_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //btnSearch_Click(null, null);
        }

        ///2023.08.15  손동혁
        private void btnBufferListEmpty_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;
           
                int clickedIndex = presenter.Row.Index;

               /* if (!String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgOuterMgform.Rows[clickedIndex].DataItem, "FROM_EQP_ID"))))
                    return;*/

                Util.MessageConfirm("FM_ME_0079", (result) =>  //Tray 정보를 변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {


                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("PORT_ID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        string a = e.GetString();
                        dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOuterMgform.Rows[clickedIndex].DataItem, "CSTID"));
                        dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgOuterMgform.Rows[clickedIndex].DataItem, "PORT_ID"));



                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_FORMATION_BUFFER_LANE", "RQSTDT", "RSLTDT", dtRqst);
                        Util.MessageInfo("10022"); //삭제가 완료되었습니다
                        GetOuterMgFormationStatus();

                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void dgFormation_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            if (sender == null)
                return;
            if (cUseFlag)
            {
                C1DataGrid dg = sender as C1DataGrid;

                for (int row = 0; row < dg.Rows.Count; row++)
                {
                    for (int col = 0; col < dg.Columns.Count; col++)
                    {
                        string ROWNUM = _dtCopy.Rows[row][(col + 1).ToString()].ToString();
                        int result = 0;

                        if (int.TryParse(ROWNUM, out result))
                        {
                            if (row == (_dtCopy.Rows.Count / 2) + 1 || row == 1)
                            {
                                C1.WPF.DataGrid.DataGridCell cell1 = dgFormation.GetCell(row - 1, col);
                                if (cell1.Presenter != null)
                                {
                                    cell1.Presenter.FontSize = 7;
                                }
                                continue;
                            }

                            if (string.IsNullOrEmpty(ROWNUM))
                            {
                                C1.WPF.DataGrid.DataGridCell cell1 = dgFormation.GetCell(row - 1, col);
                                C1.WPF.DataGrid.DataGridCell cell2 = dgFormation.GetCell(row, col);
                                DataGridCellsRange range = new DataGridCellsRange(cell1, cell2);
                                dgFormation.MergeCells(cell1, cell2);
                           
                            }
                            else
                            {

                                int Seq = Util.NVC_Int(GetDtRowValue(ROWNUM, "FORMSTATUS"));

                                if (Seq == 99 || Seq == 22)

                                {
                                    C1.WPF.DataGrid.DataGridCell cell1 = dgFormation.GetCell(row - 1, col);
                                    C1.WPF.DataGrid.DataGridCell cell2 = dgFormation.GetCell(row, col);
                                    DataGridCellsRange range = new DataGridCellsRange(cell1, cell2);
                                    dgFormation.MergeCells(cell1, cell2);
                                }
                            }
                        }
                    }
                }
                double dRowHeight = (dgFormation.ActualHeight) / ((dg.Rows.Count));
                dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
            }
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
                            txtTroubleRepairWay.Text = Util.NVC(dsRslt.Tables["EQUIPMENT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString());
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
                            txtTroubleName.Text = string.Empty;
                            txtTroubleRepairWay.Text = string.Empty;
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
                    if (bUseFlag)
                    {
                        LANEID = Util.NVC(GetDtRowValue(ROWNUM, "LANE_ID"));

                    }

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
                        FCS001_001_DETAIL wndRunStart = new FCS001_001_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = _LANEID;
                            if (bUseFlag)
                            {
                                Parameters[4] = LANEID;
                            }

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
                            if (bUseFlag)
                            {
                                Parameters[0] = LANEID;
                            }

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
                    else if (rdoUseTime.IsChecked == true)
                    {
                        //FCS001_072 - Box 유지보수
                        FCS001_072 wndRunStart = new FCS001_072();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = _LANEID;

                            if (bUseFlag)
                            {
                                Parameters[0] = LANEID;
                            }
                            this.FrameOperation.OpenMenu("SFU010730050", true, Parameters);
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

            GetMaintData();
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
                dr["CMCDTYPE"] = "FORM_FORMSTATUS";
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

                //-----------------------------------------------------
                CommonCombo_Form _combo = new CommonCombo_Form();

                string[] filter = { "1", _LANEID };
                _combo.SetCombo(cboOperLegend, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "FORMATION_STATUS_NEXT_PROC" });



                //------------------------------------------------------
                Util.GridSetData(dgColor, _dtColor, FrameOperation, true);
                ///2023.08.15  손동혁
                if (bUseFlag)
                {
                    RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                    dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "FORMATION_STATUS_NEXT_PROC";
                    RQSTDT.Rows.Add(dr);

                    _dtLegend = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    foreach (DataRow row in _dtLegend.Rows)
                    {
                        C1ComboBoxItem cbItem = new C1ComboBoxItem
                        {
                            Content = row["CBO_CODE"].ToString() + " : " + row["CBO_NAME"].ToString(),

                        };

                    }

                    DataTable mergeTable = new DataTable();
                    mergeTable = MergeTablesByIndex(_dtColor, _dtLegend);
                    SetLegendData(mergeTable);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        ///2023.08.15  손동혁
        public static DataTable MergeTablesByIndex(DataTable dt1, DataTable dt2)
        {

            DataTable dt3 = new DataTable();  // first add columns from table1
                                              /*   dt3=dt1.Clone();
                                                 dt3.Merge(dt2);
                                                 dt1.Merge(dt2);*/
            int Rows = 0, DiffCnt = dt1.Rows.Count - dt2.Rows.Count;

            if (DiffCnt > 0)
            {
                Rows = dt1.Rows.Count;
                for (int Cnt = 0; Cnt < Math.Abs(DiffCnt); Cnt++)
                    dt2.Rows.Add();
            }else
            {
                Rows = dt2.Rows.Count;
                for (int Cnt = 0; Cnt < Math.Abs(DiffCnt); Cnt++)
                    dt1.Rows.Add();
            }

            dt3.Columns.Add("CBO_NAME", typeof(string));
            dt3.Columns.Add("ATTRIBUTE1", typeof(string));
            dt3.Columns.Add("ATTRIBUTE2", typeof(string));
            dt3.Columns.Add("CBO_CODE", typeof(string));


            for (int Cnt = 0; Cnt < Rows; Cnt++)
            {

                DataRow row1 = dt3.NewRow();
                string name = dt1.Rows[Cnt].Field<string>("CBO_NAME");
                row1["CBO_NAME"] = dt1.Rows[Cnt].Field<string>("CBO_NAME");
                row1["ATTRIBUTE1"] = dt1.Rows[Cnt].Field<string>("ATTRIBUTE1");
                row1["ATTRIBUTE2"] = dt1.Rows[Cnt].Field<string>("ATTRIBUTE2");
                row1["CBO_CODE"] = (dt2.Rows[Cnt].Field<string>("CBO_CODE") + "  " + dt2.Rows[Cnt].Field<string>("CBO_NAME"));

                dt3.Rows.Add(row1);
            }




            return dt3;


        }
        
        private void GetNonOperBoxSummary()
        {
            try
            {

                dgLoss.ItemsSource = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("LANE_ID_2", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (dUseFlag)
                {

                    string[] LaneId = _LANEID.Split(',');
                    dr["LANE_ID"] = LaneId[0];
                    dr["LANE_ID_2"] = LaneId[1];

                }
                else
                {
                    dr["LANE_ID"] = _LANEID;
                }
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_NONBOX_RATE_BY_LANE", "RQSTDT", "RSLTDT", dtRqst);

                //Util.GridSetData(dgLoss, dtRslt, FrameOperation, true);
                dgLoss.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
             
        private void GetTrayTypeCnt()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("LANE_ID_2", typeof(string));
                dtRqst.Columns.Add("INCLUDE_REQ_SHIP", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (dUseFlag)
                {
                    string[] LaneId = _LANEID.Split(',');
                    dr["LANE_ID"] = LaneId[0];
                    dr["LANE_ID_2"] = LaneId[1];
                }
                else
                {
                    dr["LANE_ID"] = _LANEID;
                }
                dr["INCLUDE_REQ_SHIP"] = chkReqShip.IsChecked.Equals(true) ? "Y" : "N";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHARGE_TRAY_TYPE_CNT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayShare, dtRslt, FrameOperation, true);

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
                int iCST_LOAD_LOCATION_CODE;
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
                string NEXT_PROC_DETL_TYPE_CODE = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                string PreFlag = string.Empty;
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

                    iRowCount = tempStg * 2;
                    iColumnCount = tempCol + 1;

                    //Grid Column Width get
                    dColumnWidth = (dgFormation.ActualWidth - 70) / (iColumnCount - 1);

                    //Grid Row Height get
                    //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
                    //dRowHeight = (dgFormation.ActualHeight) / ((iRowCount * 2) + rowList.Count) - 1.3;
                    //2023.08.13 LJE Rowlist count도 포함하여 한 화면에 스크롤 없이 표시되록 수정
                    dRowHeight = (dgFormation.ActualHeight) / ((iRowCount * 2) + rowList.Count) - 1.3;

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
                                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, 50, VerticalAlignment.Center);
                                else
                                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dColumnWidth, VerticalAlignment.Center);
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
                                drRowHeader[i] = rowList[row].Nvc() + langYul;
                            else
                                //drRowHeader[i] = (i).ToString() + langDCIR;
                                drRowHeader[i] = (i).ToString() + langYun;
                        }
                        drListHeader.Add(drRowHeader);
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
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc(); //임시
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].Nvc();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc(); //20211018 Auto Calibration Lot표시 추가
                        PreFlag = dr["PRE"].Nvc();

                        for (int row = 0; row < rowList.Count; row++)
                        {
                            if (rowList[row].NvcInt() == iROW)
                            {
                                if (iCST_LOAD_LOCATION_CODE > 1)
                                {
                                    dtList[row].Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + langDan;  //단
                                    dtList[row].Rows[iRowCount - (iSTG * 2)][iCOL] = k.ToString();
                                }
                                else
                                {
                                    dtList[row].Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + langDan;  //단
                                    dtList[row].Rows[iRowCount - (iSTG * 2) + 1][iCOL] = k.ToString();
                                }

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

                        //2023.06.07 추가
                        if (MEGA_DCHG_FUNC_YN.Equals("Y") && MEGA_CHG_FUNC_YN.Equals("Y"))
                        {
                            CCOLOR = "Red";
                        }

                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID; //231229 SDH : 예약중 상태 추가
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

                                if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
                                    BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

                                if (rdoTime.IsChecked == true)
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                                break;
                            case "25": //수리중
                                       //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                break;
                            case "22": //사용안함
                                sStatus = USE_N;
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
                        dr["CCOLOR"] = CCOLOR; ////////////////수정!!!!!!!!!!!!!!!!!!!!!!
                        dr["TEXT"] = sStatus;
                        dr["BOLD"] = (bBold == true) ? "Y" : "N";
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

                    string[] sColumnName = new string[] { "1" };
                    Util.SetDataGridMergeExtensionCol(dgFormation, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    #endregion

                }
                else
                {
                    #region Grid 초기화
                    iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(COL)", string.Empty));
                    iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(STG)", string.Empty));

                    iRowCount = iMaxStg * 2;
                    iColumnCount = (iMaxCol + 1) / 2 + 1;   //단 추가.

                    //Column Width get
                    dColumnWidth = (dgFormation.ActualWidth - 1100) / (iColumnCount - 1);

                    //Row Height get
                    //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
                    dRowHeight = (dgFormation.ActualHeight) / (iRowCount * 2) - 100;

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
                            //UrowHeader[i] = (i).ToString() + langDCIR;
                            UrowHeader[i] = (i).ToString() + langYun;
                    }

                    //Row Header 생성
                    DataRow DrowHeader = Ddt.NewRow();
                    for (int i = 0; i < Ddt.Columns.Count; i++)
                    {
                        if (i == 0)
                            DrowHeader[i] = string.Empty;
                        else
                            //DrowHeader[i] = (i + Ddt.Columns.Count - 1).ToString() + langDCIR;
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
                        EQP_OP_STATUS_CD = dr["EQP_OP_STATUS_CD"].Nvc();
                        RUN_MODE_CD = dr["RUN_MODE_CD"].Nvc();
                        PROCID = dr["PROCID"].Nvc();
                        PROCNAME = dr["PROCNAME"].Nvc();
                        NEXT_PROCID = dr["NEXT_PROCID"].Nvc();
                        RCV_ISS_ID = dr["RCV_ISS_ID"].Nvc();
                        FORMSTATUS = dr["FORMSTATUS"].Nvc();
                        EQPTIUSE = dr["EQPTIUSE"].Nvc();
                        NEXT_PROCNAME = dr["NEXT_PROCNAME"].Nvc();
                        PROD_LOTID = dr["PROD_LOTID"].Nvc();
                        JOB_TIME = dr["JOB_TIME"].Nvc();
                        ROUTID = dr["ROUTID"].Nvc();
                        DUMMY_FLAG = dr["DUMMY_FLAG"].Nvc();
                        SPECIAL_YN = dr["SPECIAL_YN"].Nvc();
                        LAST_RUN_TIME = dr["LAST_RUN_TIME"].Nvc();
                        JIG_AVG_TEMP = dr["JIG_AVG_TEMP"].Nvc();
                        POW_AVG_TEMP = dr["POW_AVG_TEMP"].Nvc();
                        MEGA_DCHG_FUNC_YN = dr["MEGA_DCHG_FUNC_YN"].Nvc();
                        MEGA_CHG_FUNC_YN = dr["MEGA_CHG_FUNC_YN"].Nvc();
                        NOW_TIME = dr["NOW_TIME"].Nvc();
                        NEXT_PROC_DETL_TYPE_CODE = dr["NEXT_PROC_DETL_TYPE_CODE"].Nvc();
                        ATCALIB_TYPE_CODE = dr["ATCALIB_TYPE_CODE"].Nvc(); //20211018 Auto Calibration Lot표시 추가
                        PreFlag = dr["PRE"].Nvc();

                        if (iCOL <= iColumnCount - 1)
                        {
                            if (iCST_LOAD_LOCATION_CODE > 1)
                            {
                                Udt.Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + langDan;  //단
                                Udt.Rows[iRowCount - (iSTG * 2)][iCOL] = k.ToString();
                            }
                            else
                            {
                                Udt.Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + langDan;  //단
                                Udt.Rows[iRowCount - (iSTG * 2) + 1][iCOL] = k.ToString();
                            }
                        }
                        else
                        {
                            if (iCST_LOAD_LOCATION_CODE > 1)
                            {
                                Ddt.Rows[iRowCount - (iSTG * 2)][0] = iSTG.ToString() + langDan;  //단
                                Ddt.Rows[iRowCount - (iSTG * 2)][iCOL - iColumnCount + 1] = k.ToString();
                            }
                            else
                            {
                                Ddt.Rows[iRowCount - (iSTG * 2) + 1][0] = iSTG.ToString() + langDan;  //단
                                Ddt.Rows[iRowCount - (iSTG * 2) + 1][iCOL - iColumnCount + 1] = k.ToString();
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

                        if (MEGA_DCHG_FUNC_YN.Equals("Y") && MEGA_CHG_FUNC_YN.Equals("Y"))
                        {
                            CCOLOR = "Red";
                        }

                        if (PreFlag.Equals("Y"))
                        {
                            sStatus = PRE + ") " + CSTID; //231229 SDH : 예약중 상태 추가
                        }

                        switch (FORMSTATUS)
                        {
                            case "01": // 통신두절
                                sStatus = COMM_LOSS;
                                break;
                            case "10": //예약요청
                                tTimeSpan = DateTime.Parse(NOW_TIME) - DateTime.Parse(LAST_RUN_TIME);

                                if (tTimeSpan.TotalMinutes >= Convert.ToInt16(drColor[0]["ATTRIBUTE4"]))   //예약요청 한계시간 초과시 배경색 변경
                                    BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();

                                if (rdoTime.IsChecked == true)
                                    sStatus = RESV + ")" + tTimeSpan.Days.ToString() + "day " + tTimeSpan.Hours.ToString("00") + ":" + tTimeSpan.Minutes.ToString("00");
                                else
                                    sStatus = RESV + ")" + CSTID;
                                break;
                            case "19": //Power Off
                                sStatus = "Power Off";
                                break;
                            case "21": //정비중
                                sStatus = MAINTENANCE + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                                break;
                            case "25": //수리중
                                       //2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가, 부동전환시 박스 표시 변경
                                sStatus = REPAIR + ")" + GetMaintName(EQPTID, LAST_RUN_TIME);
                                break;
                            case "22": //사용안함
                                sStatus = USE_N;
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
                        dr["CCOLOR"] = CCOLOR; ////////////////수정!!!!!!!!!!!!!!!!!!!!!!
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
                }
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
            txtTroubleRepairWay.Text = string.Empty;
            txtRemark.Text = string.Empty;
        }

        private void IsFormationUse()
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
                dr["COM_TYPE_CODE"] = "FORM_FORMEQPT_COND_USE_FLAG";
                dr["COM_CODE"] = _LANEID;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    cUseFlag = true;
                    dUseFlag = false;
                }
                else if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null && dtResult.Rows[0]["ATTR2"].ToString().Equals("Y"))
                {
                    cUseFlag = false;
                    dUseFlag = true;
                    FirstRow = dtResult.Rows[0]["ATTR3"].ToString();
                    SecondRow = dtResult.Rows[0]["ATTR4"].ToString();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetMaintData()
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

                dtRepair = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TRAY_EQP_REPAIR", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetMaintName(string sEqpId, string sLastRunTime)
        {
            string sReturn = string.Empty;
            try
            {
                if (dtRepair == null || dtRepair.Rows.Count == 0) return sReturn;

                DataRow drEqpt = dtRepair.AsEnumerable().Where(w => w.Field<string>("EQPTID").Equals(sEqpId)).FirstOrDefault();
                if (drEqpt == null)
                {
                    sReturn = MANUAL + " " + sLastRunTime;  //수동
                }
                else
                {
                    sReturn = drEqpt["TROUBLE_REPAIR_CD2"].ToString() + " " + drEqpt["TROUBLE_REPAIR_TIME"].ToString(); //TROUBLE_REPAIR_TIME 와 TROUBLE_REPAIR_TIME2가 동일함(Format만 틀림)
                    if (bUseFlag)
                    {
                        //ESNA 요청 년도 및 형식 수정 요청 ex) BM MM-DD HH:mm
                        //sReturn = drEqpt["TROUBLE_REPAIR_CD2"].ToString() + " " + Convert.ToDateTime(Util.NVC(drEqpt["TROUBLE_REPAIR_TIME"])).ToString("MM-dd HH:mm:ss"); //TROUBLE_REPAIR_TIME 와 TROUBLE_REPAIR_TIME2가 동일함(Format만 틀림)
                        sReturn = Convert.ToDateTime(Util.NVC(drEqpt["TROUBLE_REPAIR_TIME"])).ToString("MM-dd HH:mm"); 
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = sEqpId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TEMP", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0) return;

                DataTable dtRsltChg = new DataTable();
                dtRsltChg.Columns.Add("POINT", typeof(string));
                dtRsltChg.Columns.Add("JIG_L", typeof(Decimal));
                dtRsltChg.Columns.Add("JIG_U", typeof(Decimal));
                dtRsltChg.Columns.Add("POWER_L", typeof(Decimal));
                dtRsltChg.Columns.Add("POWER_U", typeof(Decimal));
                for (int j = 1; j < dtRslt.Columns.Count; j++)
                {
                    if (!dtRslt.Columns[j].Caption.Contains("TEMP"))
                        continue;

                    DataRow drr = dtRsltChg.NewRow();
                    drr["POINT"] = dtRslt.Columns[j].Caption.Substring(4);
                    drr["JIG_L"] = dtRslt.Rows[0][j];
                    drr["JIG_U"] = dtRslt.Rows[1][j];
                    drr["POWER_L"] = dtRslt.Rows[2][j];
                    drr["POWER_U"] = dtRslt.Rows[3][j];
                    dtRsltChg.Rows.Add(drr);
                }
                Util.GridSetData(dgTemp, dtRsltChg, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        ///2023.08.15  손동혁
        private void GetOuterMgFormationStatus()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;
                dr["LANGID"] = LoginInfo.LANGID;
                if (dUseFlag)
                {
                    string[] LaneId = _LANEID.Split(',');
                    dr["LANE_ID"] = LaneId[0];
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_BUFFER_LANE", "RQSTDT", "RSLTDT", dtRqst);

            

                if (dtRslt.Rows.Count > 0)
                { 

                    Util.GridSetData(dgOuterMgform, dtRslt, FrameOperation, true);
                }

                dgOuterMgform.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
             
        ///2023.08.15  손동혁
        private void SetLegendData(DataTable dtRslt)
        {
            try
            {
                DataTable dst = new DataTable();
                DataRow dr;
                DataColumn dc;
                int idx, Count, ROW;
                Count = 8;
                idx = 1;

                dc = dst.Columns.Add("CODE");
               

                for (int r = 0; r < (Count + 1); r++)

                {
                    dc = dst.Columns.Add("Legend_" + r, typeof(string));

                }


                for (int c = 0; c < dtRslt.Columns.Count; c++)

                {
                    ROW = 0;
                    dr = dst.NewRow();
                    dr[0] = dtRslt.Columns[c].ColumnName;

                    for (int r = 0; r < dtRslt.Rows.Count; r++)
                    {

                        if (r > 0 && r % Count == 0)
                        {
                            dst.Rows.Add(dr);
                            dr = dst.NewRow();
                            dr[0] = dtRslt.Columns[c].ColumnName;

                            ROW++;


                        }
                        string a = dtRslt.Rows[r][c].ToString();
                        dr[r + idx - (ROW * Count)] = dtRslt.Rows[r][c];

                    }

                    dst.Rows.Add(dr);

                }
                ////_dtBackgroundCopy, _dtForeColorCopy
                dst.AcceptChanges();
                _dtBackgroundCopy = dst.Select("CODE = 'ATTRIBUTE1'").CopyToDataTable();
                _dtForeColorCopy = dst.Select("CODE = 'ATTRIBUTE2'").CopyToDataTable();
                foreach (DataRow row in dst.Rows)
                {
                    if (row["CODE"].ToString() == "ATTRIBUTE1" || row["CODE"].ToString() == "ATTRIBUTE2" || String.IsNullOrWhiteSpace(row["Legend_1"].ToString()))
                    {
                        row.Delete();

                    }
                }
                dst.AcceptChanges();

                dgLegend.ItemsSource = DataTableConverter.Convert(dst);
                dgLegend.UpdateLayout();
                Util.GridSetData(dgLegend, dst, FrameOperation, true);
                double dColumnWidth = (dgLegend.ActualWidth - 70) / (Count - 1);
                double dRowHeight = (dgLegend.ActualHeight) / ((dst.Rows.Count)) - 1.3;
                dgLegend.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
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

        private void chkReqShip_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_LANEID)) return;

            GetListFormation();
        }

        private void GetListFormation()
        {
            try
            {
                btnSearch.IsEnabled = false;
                gdDisplayType.IsEnabled = false;
                gdDetailType.IsEnabled = false;


                ClearALL();
                GetNonOperBoxSummary();
                GetTrayTypeCnt();
                ///2023.08.15  손동혁
                if (bUseFlag)
                {
                    GetOuterMgFormationStatus();
                }

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
                //dtRqst.Columns.Add("ROW", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = _LANEID;
                dtRqst.Rows.Add(dr);

                // 멀티 스레드 실행, 다음은 순서대로 dgFormation_ExecuteCustomBinding, dgFormation_ExecuteDataCompleted 실행 됨.
                dgFormation.ExecuteService("BR_SEL_FORMATION_VIEW_BY_LANE", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

    }
}

