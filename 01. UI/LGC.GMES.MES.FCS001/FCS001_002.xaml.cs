/*************************************************************************************
 Created Date : 2020.12.02
      Creator : Dooly
   Decription : JIG 충방전기 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.02  DEVELOPER : Initial Created.
  2021.10.18  KDH : Auto Calibration Lot표시 추가
  2022.04.19  KDH : 에러 수정(개체 참조가 개체의 인스턴스 로 설정되지 않았습니다.)  
  2022.05.24  조영대 : 데이터그리드 셀 라인 표시 및 범례 전경색 수정
  2022.11.07  이정미 : JIG LD/ULD Station 오류 수정
  2022.03.23  조영대 : 표시방식 차기공정 Route 추가
  2023.06.14  최도훈 : 화면 이동 또는 종료시 AUTO_TIMER 해제되도록 수정
  2023.06.29  최도훈 : JIG LD/ULD Station 비즈 수정
  2023.09.21  손동혁 : NA 1동 J/F 설비명 표시 수정
  2023.11.24  손동혁 : 색 범례 정비중 표기오류 원복
  2023.12.01  주훈종 : 화재알람 추가
  2023.12.28  이정미 : 설비 없음 표기 추가 
  2024.03.11  조영대 : 상세보기 유형 - 수리내역 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Controls;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _LANEID = string.Empty;
        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private string _MENUID = string.Empty;
        private bool _LoadChk = false;
        private string NO_EQP = string.Empty;

        private UcBaseRadioButton saveRadioDisplay = null;
        private UcBaseRadioButton saveRadioDetail = null;

        DispatcherTimer _timer = new DispatcherTimer();

        private bool bUseFlag = false; //2023.09.21 NA1동 JGF 설비명 설비 설명 표시
        public FCS001_002()
        {
            InitializeComponent();
        }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 창을 3개이상 오픈시 Radio 버튼 클리어 현상으로 우회 처리.
                // 탭 재선택시 보관된 Radio 버튼과 체크가 없어질 경우 복원
                // this.Loaded -= UserControl_Loaded; 이벤트 넣지 말것.

                if (saveRadioDisplay == null)
                {
                    LoadedProcess();

                    saveRadioDisplay = rdoTrayId;
                    saveRadioDetail = rdoTrayInfo;
                }
                else
                {
                    if (saveRadioDisplay.IsChecked != true) saveRadioDisplay.SetCheckStatus(true, false);
                    if (saveRadioDetail.IsChecked != true) saveRadioDetail.SetCheckStatus(true, false);
                }
                
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterLane = { "", "1" };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilterLane);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            btnSearch_Click(null, null);
        }

        #endregion

        #region Event

        private void cboLane_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgFormation.ItemsSource = null;

            btnSearch_Click(null, null);
        }

        private void chkEnlargeJig_Checked(object sender, RoutedEventArgs e)
        {
            chkEnlargeStation.IsChecked = false;
            this.R02.Height = new GridLength(0);

            //dgFormation.FontFamily = new FontFamily("Arial");
            //dgFormation.FontStretch = FontStretches.UltraExpanded;
            //dgFormation.FontSize = 10;

            //for (int i = 0; i < dgFormation.Rows.Count; i++)
            //{
            //    dgFormation.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(18); ;
            //}

        }

        private void chkEnlargeJig_Unchecked(object sender, RoutedEventArgs e)
        {
            this.R01.Height = new GridLength(0.7, GridUnitType.Star);
            this.R02.Height = new GridLength(0.3, GridUnitType.Star);
        }

        private void chkEnlargeStation_Checked(object sender, RoutedEventArgs e)
        {
            chkEnlargeJig.IsChecked = false;
            this.R01.Height = new GridLength(0);
        }

        private void chkEnlargeStation_Unchecked(object sender, RoutedEventArgs e)
        {
            this.R01.Height = new GridLength(0.7, GridUnitType.Star);
            this.R02.Height = new GridLength(0.3, GridUnitType.Star);
        }

        private void rdoDisplay_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                saveRadioDisplay = sender as UcBaseRadioButton;

                dgFormation.ItemsSource = null;

                btnSearch_Click(null, null);
            }
        }

        private void rdoDetail_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked) saveRadioDetail = sender as UcBaseRadioButton;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!_LoadChk) return;

            btnSearch.IsEnabled = false;

            ClearALL();
            GetFormationStatus();
            GetFormationULDStatus();
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
                    // 값이 아니라 헤더 영역인 경우 종료
                    if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0)) return;

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    if (string.IsNullOrEmpty(ROWNUM)) return;
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string ROW = Util.NVC(GetDtRowValue(ROWNUM, "ROW"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string COLHEADER = _dtCopy.Rows[0][cell.Column.Index].ToString();
                    string ROWHEADER = _dtCopy.Rows[cell.Row.Index][0].ToString();
                    string LANEID = Util.NVC(GetDtRowValue(ROWNUM, "LANE_ID"));
                    //if (string.IsNullOrEmpty(CSTID))
                    //    return;

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

                            this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                        }
                    }
                    else if (rdoEqpControl.IsChecked == true)
                    {
                        if (string.IsNullOrEmpty(EQPTID))
                            return;

                        FCS001_002_DETAIL wndRunStart = new FCS001_002_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = _LANEID;
                            Parameters[5] = COLHEADER;
                            Parameters[6] = ROWHEADER;

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
                            object[] Parameters = new object[2];
                            Parameters[0] = _LANEID;
                            if (bUseFlag) Parameters[0] = LANEID;                            
                            Parameters[1] = "J";
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

        private void dgFormation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgFormation.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    ClearControl();

                    //설비 정보 가져오기
                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    if (string.IsNullOrEmpty(ROWNUM)) return;

                    int i = 0;
                    if (!int.TryParse(ROWNUM, out i)) return;

                    string ROW_ID = Util.NVC(GetDtRowValue(ROWNUM, "ROW_ID"));
                    string COL_ID = Util.NVC(GetDtRowValue(ROWNUM, "COL_ID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));

                    //TextBox Set
                    txtSelUnit.Text = ROW_ID;
                    txtSelNum.Text = COL_ID;

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = EQPTID;
                    dr["CSTID"] = CSTID;
                    dtRqst.Rows.Add(dr);

                    DataSet dsRqst = new DataSet();
                    dsRqst.Tables.Add(dtRqst);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_TRAY_EQP_MAINT", "INDATA", "TRAY,EQPMAINT,MAINT", dsRqst);

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
                    else if (cell.Text.Length > 0)
                    {
                        txtStatus.Text = cell.Text;
                    }
                    else
                    {
                        txtStatus.Text = string.Empty;
                    }

                    // Box Trouble 정보
                    if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0 && (FORMSTATUS.Equals("16") || FORMSTATUS.Equals("17"))) //Trouble 또는 일시정지
                    {
                        txtTroubleName.Text = dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_NAME"].ToString();
                        txtTroubleRepairWay.Text = dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString();
                    }
                    else if (dsRslt.Tables["MAINT"].Rows.Count > 0)
                    {
                        txtTroubleRepairWay.Text = dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString();

                        /* 2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가 
                             * 부동전환시 박스 표시 변경, 조건 변경
                             * */
                        if (dsRslt.Tables["MAINT"].Rows[0]["MAINT_STAT_CODE"].ToString().Equals("C"))
                        {
                            txtRemark.Text = dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_NAME"].ToString();
                        }
                    }
                    else
                    {
                        txtTroubleName.Text = string.Empty;
                        txtTroubleRepairWay.Text = string.Empty;
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

                if (e.Cell.Presenter == null) return;

                if (!_dtCopy.Columns.Contains(Util.NVC(e.Cell.Column.Index + 1))) return;

                bool bRH = false;
                bool bCH = false;

                #region Header Setting
                //ROW Header 설정
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "1")).Equals(string.Empty))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    bRH = true;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    if (e.Cell.Column.Index != 0 && e.Cell.Row.Index != 0)
                    {
                        //설비없음
                        e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                        e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                        e.Cell.Presenter.ToolTip = NO_EQP;
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), NO_EQP);
                    }

                    bRH = false;
                }

                //Column Header 설정
                if (e.Cell.Column.Index == 0)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                    e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("Black") as SolidColorBrush;
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    bCH = true;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    bCH = false;
                }
                #endregion

                if (bRH.Equals(true) || bCH.Equals(true))
                {
                    return;
                }
                else
                {
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

                        string toolTipLegend = _dtColor.AsEnumerable().Where(w => w["ATTRIBUTE1"].Nvc() == BCOLOR).Select(s => s["CBO_NAME"].Nvc()).FirstOrDefault();
                        if (!string.IsNullOrEmpty(toolTipLegend))
                        {
                            e.Cell.Presenter.ToolTip = toolTipLegend;
                        }
                        else
                        {
                            e.Cell.Presenter.ToolTip = null;
                        }
                    }

                    e.Cell.Presenter.Padding = new Thickness(0);
                    e.Cell.Presenter.Margin = new Thickness(0);
                    e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 1, 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgFormation_ExecuteCustomBinding(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            _dtDATA = e.ResultData as DataTable;

            SetfpsFormationData(_dtDATA);
        }

        private void dgFormation_ExecuteDataCompleted(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (chkTimer.IsChecked.Equals(true)) _timer.Start();

            btnSearch.IsEnabled = true;
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

        private void dgStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    string BCOLOR_MAIN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BCOLOR_MAIN"));
                    string FCOLOR_MAIN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FCOLOR_MAIN"));
                    string BCOLOR_ST = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BCOLOR_ST"));
                    string FCOLOR_ST = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FCOLOR_ST"));

                    if (e.Cell.Column.Name.Equals("MAIN_EQP_STATUS"))
                    {
                        if (!string.IsNullOrEmpty(BCOLOR_MAIN))
                        {
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR_MAIN) as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                            
                        if (!string.IsNullOrEmpty(FCOLOR_MAIN))
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR_MAIN) as SolidColorBrush;
                    }

                    if (e.Cell.Column.Name.Equals("ST_STATUS"))
                    {
                        if (!string.IsNullOrEmpty(BCOLOR_ST))
                        {
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR_ST) as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                            
                        if (!string.IsNullOrEmpty(FCOLOR_ST))
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR_ST) as SolidColorBrush;
                    }

                    if (e.Cell.Column.Name.Equals("LANE_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgStatus_ExecuteCustomBinding(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtRslt = e.ResultData as DataTable;

            SetfpsFormationULDData(dtRslt);
        }

        private void dgStatus_ExecuteDataCompleted(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void chkTimer_Checked(object sender, RoutedEventArgs e)
        {
            nuTimerSec.IsEnabled = false;

            _timer.Interval = TimeSpan.FromSeconds(nuTimerSec.Value);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        private void chkTimer_Unchecked(object sender, RoutedEventArgs e)
        {
            nuTimerSec.IsEnabled = true;

            _timer.Tick -= new EventHandler(timer_Tick);
            _timer.Stop();
        }

        #endregion

        #region Method

        private void LoadedProcess()
        {
            try
            {
                _LoadChk = false;
                string sLaneID = string.Empty;

                if (string.IsNullOrEmpty(_MENUID))
                    _MENUID = LoginInfo.CFG_MENUID;

                sLaneID = GetLaneIDForMenu(_MENUID);

                if (string.IsNullOrEmpty(sLaneID))
                    _LANEID = "1";
                else
                    _LANEID = sLaneID;

                //Combo Setting
                InitCombo();
                //Legend Setting
                InitLegend();

                chkEnlargeJig.Checked += chkEnlargeJig_Checked;
                chkEnlargeJig.Unchecked += chkEnlargeJig_Unchecked;
                chkEnlargeStation.Checked += chkEnlargeStation_Checked;
                chkEnlargeStation.Unchecked += chkEnlargeStation_Unchecked;

                cboLane.SelectedValue = _LANEID;

                bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_002_JGF_NAME"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

                _LoadChk = true;
                btnSearch_Click(null, null);

                NO_EQP = ObjectDic.Instance.GetObjectName("NO_EQP");

                cboLane.SelectedValueChanged += cboLane_SelectedValueChanged;

                _timer = new DispatcherTimer();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                dr["CMCDTYPE"] = "FORM_JIG_CHARGE_MENU_ID";
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
                Util.MessageException(ex);
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
                dr["CMCDTYPE"] = "FORM_JIGFORMSTATUS";
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
                        Foreground = new BrushConverter().ConvertFromString(row["ATTRIBUTE2"].ToString()) as SolidColorBrush,
                    };
                    cboColorLegend.Items.Add(cbItem);
                }

                cboColorLegend.SelectedIndex = 0;

                //-----------------------------------------------------
                CommonCombo_Form _combo = new CommonCombo_Form();

                _combo.SetCombo(cboOperLegend, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "JIGFORMATION_STATUS_NEXT_PROC" });

                //------------------------------------------------------
                //Util.GridSetData(dgColor, _dtColor, FrameOperation, true);
                dgColor.ItemsSource = DataTableConverter.Convert(_dtColor);

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

                //20220419_에러 수정(개체 참조가 개체의 인스턴스로 설정되지 않았습니다.) START
                //if (!string.IsNullOrEmpty(cboLane.SelectedValue.ToString()))
                //    dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                if (!string.IsNullOrEmpty(Util.NVC(cboLane.SelectedValue)))
                {
                    dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                }
                else
                {
                    dr["LANE_ID"] = DBNull.Value;
                }
                //20220419_에러 수정(개체 참조가 개체의 인스턴스로 설정되지 않았습니다.) END

                dtRqst.Rows.Add(dr);

                // 백그라운드 실행
                dgFormation.ExecuteService("DA_SEL_JIG_FORMATION_VIEW", "RQSTDT", "RSLTDT", dtRqst);

                //if (_dtDATA.Rows.Count == 0)
                //{
                //    ////임시
                //    ////GetTestData1(ref _dtDATA);
                //    ////조회된 값이 없습니다.
                //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0232"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    //{
                //    //    if (result == MessageBoxResult.OK)
                //    //    {
                //    //    }
                //    //});
                //    //return;

                //    //Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                //}
                //else
                //{
                //    dgFormation.Columns.Clear();
                //    dgFormation.Refresh();

                //    SetfpsFormationData(_dtDATA);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetFormationULDStatus()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                // 백그라운드 실행
                dgStatus.ExecuteService("BR_SEL_JIG_FORMATION_ST_VIEW", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetfpsFormationULDData(DataTable dtRslt)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANE_ID", typeof(string));
                dt.Columns.Add("MAIN_EQP", typeof(string));
                dt.Columns.Add("MAIN_EQP_STATUS", typeof(string));
                dt.Columns.Add("ST_NAME", typeof(string));
                dt.Columns.Add("ST_STATUS", typeof(string));
                dt.Columns.Add("BCOLOR_MAIN", typeof(string));
                dt.Columns.Add("FCOLOR_MAIN", typeof(string));
                dt.Columns.Add("BCOLOR_ST", typeof(string));
                dt.Columns.Add("FCOLOR_ST", typeof(string));

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    DataRow row = dt.NewRow();
                    row["LANE_ID"] = Util.NVC(dtRslt.Rows[i]["LANE_ID"].ToString());
                    row["MAIN_EQP_STATUS"] = $"COMM[{dtRslt.Rows[i]["COMM_STATUS"].ToString()}] / " +
                                             $"STAT[{dtRslt.Rows[i]["EQPT_STATUS"].ToString()}] / " +
                                             $"MAN[{dtRslt.Rows[i]["MANUAL"].ToString()}]";
                    row["ST_NAME"] = Util.NVC(dtRslt.Rows[i]["PORT_NAME"].ToString());
                    row["ST_STATUS"] = Util.NVC(dtRslt.Rows[i]["PORT_STATUS"].ToString());

                    if (i == 0)
                    {
                        row["MAIN_EQP"] = Util.NVC(dtRslt.Rows[i]["EQP_STG"].ToString());
                    }
                    else
                    {
                        if (Util.NVC(dtRslt.Rows[i]["EQPTID"].ToString()) == Util.NVC(dtRslt.Rows[i - 1]["EQPTID"].ToString()))
                        {
                            row["MAIN_EQP"] = Util.NVC(dt.Rows[i - 1]["MAIN_EQP"].ToString());
                        }
                        else
                        {
                            row["MAIN_EQP"] = Util.NVC(dtRslt.Rows[i]["EQP_STG"].ToString());
                        }
                    }

                    //MAIN_EQP_STATUS
                    if (row["MAIN_EQP_STATUS"].ToString().Contains("NG"))
                    {
                        row["BCOLOR_MAIN"] = "Red";
                        row["FCOLOR_MAIN"] = "White";
                    }
                    else
                    {
                        row["BCOLOR_MAIN"] = "LightGreen";
                    }

                    //ST_STATUS
                    if (row["ST_STATUS"].ToString().Contains("NG"))
                    {
                        row["BCOLOR_ST"] = "Red";
                        row["FCOLOR_ST"] = "White";
                    }
                    else
                    {
                        row["BCOLOR_ST"] = "LightGreen";
                    }
                    
                    dt.Rows.Add(row);
                }

                dgStatus.SetItemsSource(dt, FrameOperation);

                string[] sColumnName = new string[] { "LANE_ID", "MAIN_EQP", "MAIN_EQP_STATUS" };
                _Util.SetDataGridMergeExtensionCol(dgStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetfpsFormationData(DataTable dtRslt)
        {
            try
            {
                dgFormation.ItemsSource = null;

                DataTable dt = new DataTable();
                DataTable dtCol = new DataTable();
                dtCol.TableName = "RQSTDT";
                dtCol.Columns.Add("COL_ID", typeof(string));
                dtCol.Columns.Add("INDEX", typeof(string));
                int iCOLINDEX = 0;
                string sCOL_ID = string.Empty;
                string sStatus = string.Empty;
                bool bBold = false;

                //GRID에 Binding할 Datatable Setting하기
                //MAX ROW

                int iMaxRow = Convert.ToInt32(dtRslt.Compute("MAX(ROW_IDX)", string.Empty));
                
                //Row Height get
                double dRowHeight = (dgFormation.ActualHeight) / iMaxRow - 1.6;

                //MAX COL
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    sCOL_ID = dtRslt.Rows[i]["COL_ID"].ToString();

                    if (i == 0)
                    {
                        DataRow row = dtCol.NewRow();
                        row["COL_ID"] = sCOL_ID;
                        row["INDEX"] = iCOLINDEX;
                        dtCol.Rows.Add(row);

                        iCOLINDEX++;
                    }
                    else
                    {
                        if (sCOL_ID != dtRslt.Rows[i - 1]["COL_ID"].ToString())
                        {
                            DataRow row = dtCol.NewRow();
                            row["COL_ID"] = sCOL_ID;
                            row["INDEX"] = iCOLINDEX;
                            dtCol.Rows.Add(row);

                            iCOLINDEX++;
                        }
                    }
                }

                //MAX COL
                int iMaxCol = dtCol.Rows.Count + 1; //Header 포함
                if (iMaxCol != dgFormation.Columns.Count)
                {
                    dgFormation.Columns.Clear();
                    dgFormation.ItemsSource = null;
                }

                //Column 생성
                for (int i = 0; i < iMaxCol; i++)
                {
                    double dWidth;

                    if (i == 0)
                        dWidth = 100;
                    else
                        dWidth = 180;

                    //GRID Column Create
                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dWidth);

                    dt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for (int i = 0; i < iMaxRow; i++)
                {
                    DataRow row = dt.NewRow();
                    dt.Rows.Add(row);
                }

                //Column Header 생성
                DataRow Header = dt.NewRow();
                for (int i = 0; i < dtCol.Rows.Count + 1; i++)
                {
                    if (i != 0)
                    {
                        Header[i] = dtCol.Rows[i - 1]["COL_ID"].ToString();
                    }
                }

                //Row Header 
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string j = string.Empty;
                    string k = "0";
                    dgFormation.Refresh();

                    if (i < 9)
                    {
                        k = k + (i + 1);
                        j = k;
                    }
                    else
                    {
                        j = Convert.ToString(i + 1);
                    }
                    DataRow[] dr = dtRslt.Select("ROW_IDX = '" + j + "'");

                    if (dr.Length > 0)
                    {
                        /// NA 1동에서 J/F 설비명이 맞지 않아 설비 설명으로 대체 표시
                        if (bUseFlag)
                        {
                            if (dr[0]["EQPTDESC"] != null)
                            {
                                string[] LaneId = dr[0]["EQPTDESC"].ToString().Split(' ');
                                if (LaneId.Length < 3)
                                {
                                    dt.Rows[i]["1"] = LaneId[0].ToString();
                                }
                                else
                                {
                                    dt.Rows[i]["1"] = LaneId[2].ToString();

                                }
                            }
                        }else
                        {
                            dt.Rows[i]["1"] = dr[0]["ROW_ID"].ToString();
                        }
                    }
                }
                
                //dtRslt Column Add
                dtRslt.Columns.Add("BCOLOR", typeof(string));
                dtRslt.Columns.Add("FCOLOR", typeof(string));
                dtRslt.Columns.Add("TEXT", typeof(string));
                dtRslt.Columns.Add("BOLD", typeof(string));

                int COLPOS;
                int iROW;
                int iCOL;
                int iSTG;
                int iROW_IDX;
                string COL_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string PROD_LOTID = string.Empty; //2021.06.03 추가
                string OP_START_TIME = string.Empty;
                string OP_OVER_TIME = string.Empty;
                string TEMP_VAL = string.Empty;
                string PRESS_VAL = string.Empty;
                string LAST_RUN_TIME = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                string ROUTID = string.Empty;
                string NEXT_PROCID = string.Empty;

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    iROW = int.Parse(dtRslt.Rows[i]["ROW"].ToString());
                    iCOL = int.Parse(dtRslt.Rows[i]["COL"].ToString());
                    iSTG = int.Parse(dtRslt.Rows[i]["STG"].ToString());
                    iROW_IDX = int.Parse(dtRslt.Rows[i]["ROW_IDX"].ToString());
                    COL_ID = dtRslt.Rows[i]["COL_ID"].ToString();
                    FORMSTATUS = Util.NVC(dtRslt.Rows[i]["FORMSTATUS"].ToString());
                    CSTID = Util.NVC(dtRslt.Rows[i]["CSTID"].ToString());
                    LOTID = Util.NVC(dtRslt.Rows[i]["LOTID"].ToString());
                    PROD_LOTID = Util.NVC(dtRslt.Rows[i]["PROD_LOTID"]);
                    OP_START_TIME = Util.NVC(dtRslt.Rows[i]["OP_START_TIME"].ToString());
                    OP_OVER_TIME = Util.NVC(dtRslt.Rows[i]["OP_OVER_TIME"].ToString());
                    TEMP_VAL = Util.NVC(dtRslt.Rows[i]["TEMP_VAL"].ToString());
                    PRESS_VAL = Util.NVC(dtRslt.Rows[i]["PRESS_VAL"].ToString());
                    LAST_RUN_TIME = Util.NVC(dtRslt.Rows[i]["LAST_RUN_TIME"].ToString());
                    ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[i]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가
                    ROUTID = Util.NVC(dtRslt.Rows[i]["ROUTID"].ToString());
                    NEXT_PROCID = Util.NVC(dtRslt.Rows[i]["NEXT_PROCID"].ToString());

                    COLPOS = GetColPos(COL_ID, dtCol);

                    dt.Rows[iROW_IDX - 1][COLPOS + 1] = i;

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                    DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                    if (drColor.Length > 0)
                    {
                        BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                        FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                    }

                    if (rdoTrayId.IsChecked == true)
                        sStatus = CSTID;
                    else if (rdoLotId.IsChecked == true)
                    {
                        //20211018 Auto Calibration Lot표시 추가 START
                        //sStatus = PROD_LOTID; //2021.06.03 PSM : TRAY LOT ID -> PKG LOT ID로 변경
                        if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE))
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
                        sStatus = OP_OVER_TIME;
                    else if (rdoOpStart.IsChecked == true)
                        sStatus = OP_START_TIME;
                    else if (rdoTempPress.IsChecked == true)
                        sStatus = string.Format("{0} / {1}", string.IsNullOrEmpty(TEMP_VAL) ? "" : (Convert.ToDouble(TEMP_VAL) / 10).ToString("F1"), PRESS_VAL);
                    else if (rdoRouteNextOp.IsChecked == true)
                        sStatus = ROUTID + (NEXT_PROCID.Equals(string.Empty) ? "" : " [" + NEXT_PROCID + "]");

                    switch (FORMSTATUS)
                    {
                        case "01": // 통신두절
                            sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                            break;
                        case "19": //Power Off
                            sStatus = "Power Off";
                            break;
                        case "21": //정비중
                            sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                            break;
                        case "25": //수리중
                            sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + LAST_RUN_TIME; //200611 KJE : 수리중 시간 추가
                            break;
                        case "26": //임시예약
                            sStatus = ObjectDic.Instance.GetObjectName("TMP_RESV");
                            break;
                        case "36": //화재
                            sStatus = ObjectDic.Instance.GetObjectName("FIRE");
                            break;
                    }

                        dtRslt.Rows[i]["BCOLOR"] = BCOLOR;
                        dtRslt.Rows[i]["FCOLOR"] = FCOLOR;
                        dtRslt.Rows[i]["TEXT"] = sStatus;
                        dtRslt.Rows[i]["BOLD"] = (bBold == true) ? "Y" : "N";

                }

                //DataTable Header Insert
                dt.Rows.InsertAt(Header, 0);

                //DataTable Copy
                _dtCopy = dt.Copy();

                if (dgFormation.ItemsSource == null) dgFormation.ItemsSource = DataTableConverter.Convert(dt);

                dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string getMaintName(string sEqpId, string sLastRunTime) //작업자 이름 가져오기, 부동유형 가져오기
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
                dr["EQP_ID"] = sEqpId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TRAY_EQP_MAINT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                    sReturn = ObjectDic.Instance.GetObjectName("MANUAL") + " " + sLastRunTime;  //수동
                else
                    sReturn = dtRslt.Rows[0]["TROUBLE_REPAIR_CD2"].ToString() + " " + dtRslt.Rows[0]["TROUBLE_REPAIR_TIME"].ToString();
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sReturn;
        }

        private int GetColPos(string sVal, DataTable dt)
        {
            int iRtnVal = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["COL_ID"].ToString() == sVal)
                    {
                        iRtnVal = int.Parse(dt.Rows[i]["INDEX"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return iRtnVal;
        }

        private void ClearALL()
        {
            //dgFormation.ItemsSource = null;
            //dgFormation.Columns.Clear();
            //dgFormation.Refresh();
            _dtCopy = null;
            _dtDATA = null;

            ClearControl();
        }

        private void ClearControl()
        {
            txtSelNum.Text = string.Empty;
            txtSelUnit.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtTroubleName.Text = string.Empty;
            txtTroubleRepairWay.Text = string.Empty;
            txtRemark.Text = string.Empty;
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            if (dg.Columns.Contains(sColName)) return;

            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
                
            });
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
                Util.MessageException(ex);
            }
            return sRtnValue;
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

        private void GetTestData1(ref DataTable dt)
        {
            #region TEST Data
            DataRow row1 = dt.NewRow();
            row1["EQPTID"] = "X11JF10701";
            row1["ROW"] = "01";
            row1["COL"] = "07";
            row1["STG"] = "01";
            row1["CSTID"] = "";
            row1["LOTID"] = "";
            row1["EIOSTAT"] = "W";
            row1["PORT_STAT_CODE"] = "WAIT";
            row1["RUN_MODE_CD"] = "R";
            row1["PROCID"] = "";
            row1["PROCNAME"] = "";
            row1["ISS_RSV_FLAG"] = "";
            row1["WIPSTAT"] = "";
            row1["NEXT_PROCID"] = "";
            row1["FORMSTATUS"] = "11";
            row1["EQP_MAINT_DISPLAY"] = "N";
            row1["USE_YN"] = "Y";
            row1["PROD_LOTID"] = "";
            row1["OP_START_TIME"] = "";
            row1["OP_OVER_TIME"] = "";
            row1["TEMP_VAL"] = "679";
            row1["PRESS_VAL"] = "0";
            row1["LAST_RUN_TIME"] = "2020-11-24 07:22:36";
            row1["ROW_IDX"] = "1";
            row1["COL_ID"] = "자동차 1-1호 Lane 1호";
            row1["ROW_ID"] = "PP_01";
            dt.Rows.Add(row1);

            DataRow row2 = dt.NewRow();
            row2["EQPTID"] = "X11JF10702";
            row2["ROW"] = "01";
            row2["COL"] = "07";
            row2["STG"] = "02";
            row2["CSTID"] = "";
            row2["LOTID"] = "";
            row2["EIOSTAT"] = "W";
            row2["PORT_STAT_CODE"] = "WAIT";
            row2["RUN_MODE_CD"] = "R";
            row2["PROCID"] = "";
            row2["PROCNAME"] = "";
            row2["ISS_RSV_FLAG"] = "";
            row2["WIPSTAT"] = "";
            row2["NEXT_PROCID"] = "";
            row2["FORMSTATUS"] = "11";
            row2["EQP_MAINT_DISPLAY"] = "N";
            row2["USE_YN"] = "Y";
            row2["PROD_LOTID"] = "";
            row2["OP_START_TIME"] = "";
            row2["OP_OVER_TIME"] = "";
            row2["TEMP_VAL"] = "679";
            row2["PRESS_VAL"] = "0";
            row2["LAST_RUN_TIME"] = "2020-11-24 07:22:37";
            row2["ROW_IDX"] = "2";
            row2["COL_ID"] = "자동차 1-1호 Lane 1호";
            row2["ROW_ID"] = "PP_02";
            dt.Rows.Add(row2);

            DataRow row3 = dt.NewRow();
            row3["EQPTID"] = "X11JF10703";
            row3["ROW"] = "01";
            row3["COL"] = "07";
            row3["STG"] = "03";
            row3["CSTID"] = "LJIG101036";
            row3["LOTID"] = "5258286";
            row3["EIOSTAT"] = "W";
            row3["PORT_STAT_CODE"] = "WAIT";
            row3["RUN_MODE_CD"] = "R";
            row3["PROCID"] = "J71";
            row3["PROCNAME"] = "J/F PP #01";
            row3["ISS_RSV_FLAG"] = "E";
            row3["WIPSTAT"] = "WAIT";
            row3["NEXT_PROCID"] = "J31";
            row3["FORMSTATUS"] = "11";
            row3["EQP_MAINT_DISPLAY"] = "N";
            row3["USE_YN"] = "Y";
            row3["PROD_LOTID"] = "UAJTK302C1";
            row3["OP_START_TIME"] = "2020-11-24 07:22:36";
            row3["OP_OVER_TIME"] = "00-00 00:13";
            row3["TEMP_VAL"] = "700";
            row3["PRESS_VAL"] = "0";
            row3["LAST_RUN_TIME"] = "2020-11-24 07:22:38";
            row3["ROW_IDX"] = "3";
            row3["COL_ID"] = "자동차 1-1호 Lane 1호";
            row3["ROW_ID"] = "PP_03";
            dt.Rows.Add(row3);

            DataRow row4 = dt.NewRow();
            row4["EQPTID"] = "X11JF10704";
            row4["ROW"] = "01";
            row4["COL"] = "07";
            row4["STG"] = "04";
            row4["CSTID"] = "LJIG101024";
            row4["LOTID"] = "5258359";
            row4["EIOSTAT"] = "R";
            row4["PORT_STAT_CODE"] = "WAIT";
            row4["RUN_MODE_CD"] = "R";
            row4["PROCID"] = "J71";
            row4["PROCNAME"] = "J/F PP #01";
            row4["ISS_RSV_FLAG"] = "S";
            row4["WIPSTAT"] = "PROC";
            row4["NEXT_PROCID"] = "J31";
            row4["FORMSTATUS"] = "J7";
            row4["EQP_MAINT_DISPLAY"] = "N";
            row4["USE_YN"] = "Y";
            row4["PROD_LOTID"] = "UAJTK302C1";
            row4["OP_START_TIME"] = "2020-11-24 07:22:37";
            row4["OP_OVER_TIME"] = "00-00 00:04";
            row4["TEMP_VAL"] = "699";
            row4["PRESS_VAL"] = "0";
            row4["LAST_RUN_TIME"] = "2020-11-24 07:22:39";
            row4["ROW_IDX"] = "4";
            row4["COL_ID"] = "자동차 1-1호 Lane 1호";
            row4["ROW_ID"] = "PP_04";
            dt.Rows.Add(row4);

            DataRow row5 = dt.NewRow();
            row5["EQPTID"] = "X11JF10502";
            row5["ROW"] = "01";
            row5["COL"] = "05";
            row5["STG"] = "02";
            row5["CSTID"] = "LJIG101041";
            row5["LOTID"] = "5257428";
            row5["EIOSTAT"] = "R";
            row5["PORT_STAT_CODE"] = "WAIT";
            row5["RUN_MODE_CD"] = "R";
            row5["PROCID"] = "J13";
            row5["PROCNAME"] = "J/F Charge #03";
            row5["ISS_RSV_FLAG"] = "S";
            row5["WIPSTAT"] = "PROC";
            row5["NEXT_PROCID"] = "J32";
            row5["FORMSTATUS"] = "J1";
            row5["EQP_MAINT_DISPLAY"] = "N";
            row5["USE_YN"] = "Y";
            row5["PROD_LOTID"] = "UAJTK302C1";
            row5["OP_START_TIME"] = "2020-11-24 07:22:38";
            row5["OP_OVER_TIME"] = "00-00 00:20";
            row5["TEMP_VAL"] = "526";
            row5["PRESS_VAL"] = "1465";
            row5["LAST_RUN_TIME"] = "2020-11-24 07:22:40";
            row5["ROW_IDX"] = "6";
            row5["COL_ID"] = "자동차 1-1호 Lane 1호";
            row5["ROW_ID"] = "HOT_02";
            dt.Rows.Add(row5);

            DataRow row6 = dt.NewRow();
            row6["EQPTID"] = "X11JF10503";
            row6["ROW"] = "01";
            row6["COL"] = "05";
            row6["STG"] = "03";
            row6["CSTID"] = "LJIG101050";
            row6["LOTID"] = "5258043";
            row6["EIOSTAT"] = "R";
            row6["PORT_STAT_CODE"] = "WAIT";
            row6["RUN_MODE_CD"] = "R";
            row6["PROCID"] = "J12";
            row6["PROCNAME"] = "J/F Charge #02";
            row6["ISS_RSV_FLAG"] = "S";
            row6["WIPSTAT"] = "PROC";
            row6["NEXT_PROCID"] = "BJ3";
            row6["FORMSTATUS"] = "J1";
            row6["EQP_MAINT_DISPLAY"] = "N";
            row6["USE_YN"] = "Y";
            row6["PROD_LOTID"] = "UAJTK302C1";
            row6["OP_START_TIME"] = "2020-11-24 07:22:39";
            row6["OP_OVER_TIME"] = "00-00 00:10";
            row6["TEMP_VAL"] = "529";
            row6["PRESS_VAL"] = "157";
            row6["LAST_RUN_TIME"] = "2020-11-24 07:22:41";
            row6["ROW_IDX"] = "7";
            row6["COL_ID"] = "자동차 1-1호 Lane 1호";
            row6["ROW_ID"] = "HOT_03";
            dt.Rows.Add(row6);

            DataRow row7 = dt.NewRow();
            row7["EQPTID"] = "X11JF10504";
            row7["ROW"] = "01";
            row7["COL"] = "05";
            row7["STG"] = "04";
            row7["CSTID"] = "LJIG101031";
            row7["LOTID"] = "5258168";
            row7["EIOSTAT"] = "R";
            row7["PORT_STAT_CODE"] = "WAIT";
            row7["RUN_MODE_CD"] = "R";
            row7["PROCID"] = "J12";
            row7["PROCNAME"] = "J/F Charge #02";
            row7["ISS_RSV_FLAG"] = "S";
            row7["WIPSTAT"] = "PROC";
            row7["NEXT_PROCID"] = "BJ3";
            row7["FORMSTATUS"] = "J1";
            row7["EQP_MAINT_DISPLAY"] = "N";
            row7["USE_YN"] = "Y";
            row7["PROD_LOTID"] = "UAJTK302C1";
            row7["OP_START_TIME"] = "2020-11-24 07:22:40";
            row7["OP_OVER_TIME"] = "00-00 00:02";
            row7["TEMP_VAL"] = "513";
            row7["PRESS_VAL"] = "139";
            row7["LAST_RUN_TIME"] = "2020-11-24 07:22:42";
            row7["ROW_IDX"] = "8";
            row7["COL_ID"] = "자동차 1-1호 Lane 1호";
            row7["ROW_ID"] = "HOT_04";
            dt.Rows.Add(row7);

            DataRow row8 = dt.NewRow();
            row8["EQPTID"] = "X11JF10505";
            row8["ROW"] = "01";
            row8["COL"] = "05";
            row8["STG"] = "05";
            row8["CSTID"] = "LJIG101049";
            row8["LOTID"] = "5257659";
            row8["EIOSTAT"] = "R";
            row8["PORT_STAT_CODE"] = "WAIT";
            row8["RUN_MODE_CD"] = "R";
            row8["PROCID"] = "J13";
            row8["PROCNAME"] = "J/F Charge #03";
            row8["ISS_RSV_FLAG"] = "S";
            row8["WIPSTAT"] = "PROC";
            row8["NEXT_PROCID"] = "J32";
            row8["FORMSTATUS"] = "J1";
            row8["EQP_MAINT_DISPLAY"] = "N";
            row8["USE_YN"] = "Y";
            row8["PROD_LOTID"] = "UAJTK302C1";
            row8["OP_START_TIME"] = "2020-11-24 07:22:41";
            row8["OP_OVER_TIME"] = "00-00 00:09";
            row8["TEMP_VAL"] = "530";
            row8["PRESS_VAL"] = "1465";
            row8["LAST_RUN_TIME"] = "2020-11-24 07:22:43";
            row8["ROW_IDX"] = "9";
            row8["COL_ID"] = "자동차 1-1호 Lane 1호";
            row8["ROW_ID"] = "HOT_05";
            dt.Rows.Add(row8);

            DataRow row9 = dt.NewRow();
            row9["EQPTID"] = "X11JF10506";
            row9["ROW"] = "01";
            row9["COL"] = "05";
            row9["STG"] = "06";
            row9["CSTID"] = "LJIG101010";
            row9["LOTID"] = "5257571";
            row9["EIOSTAT"] = "R";
            row9["PORT_STAT_CODE"] = "WAIT";
            row9["RUN_MODE_CD"] = "R";
            row9["PROCID"] = "J13";
            row9["PROCNAME"] = "J/F Charge #03";
            row9["ISS_RSV_FLAG"] = "S";
            row9["WIPSTAT"] = "PROC";
            row9["NEXT_PROCID"] = "J32";
            row9["FORMSTATUS"] = "J1";
            row9["EQP_MAINT_DISPLAY"] = "N";
            row9["USE_YN"] = "Y";
            row9["PROD_LOTID"] = "UAJTK302C1";
            row9["OP_START_TIME"] = "2020-11-24 07:22:42";
            row9["OP_OVER_TIME"] = "00-00 00:16";
            row9["TEMP_VAL"] = "524";
            row9["PRESS_VAL"] = "1466";
            row9["LAST_RUN_TIME"] = "2020-11-24 07:22:44";
            row9["ROW_IDX"] = "10";
            row9["COL_ID"] = "자동차 1-1호 Lane 1호";
            row9["ROW_ID"] = "HOT_06";
            dt.Rows.Add(row9);

            DataRow row10 = dt.NewRow();
            row10["EQPTID"] = "X11JF10507";
            row10["ROW"] = "01";
            row10["COL"] = "05";
            row10["STG"] = "07";
            row10["CSTID"] = "LJIG101035";
            row10["LOTID"] = "5257958";
            row10["EIOSTAT"] = "R";
            row10["PORT_STAT_CODE"] = "WAIT";
            row10["RUN_MODE_CD"] = "R";
            row10["PROCID"] = "J13";
            row10["PROCNAME"] = "J/F Charge #03";
            row10["ISS_RSV_FLAG"] = "S";
            row10["WIPSTAT"] = "PROC";
            row10["NEXT_PROCID"] = "J32";
            row10["FORMSTATUS"] = "J1";
            row10["EQP_MAINT_DISPLAY"] = "N";
            row10["USE_YN"] = "Y";
            row10["PROD_LOTID"] = "UAJTK302C1";
            row10["OP_START_TIME"] = "2020-11-24 07:22:43";
            row10["OP_OVER_TIME"] = "00-00 00:03";
            row10["TEMP_VAL"] = "530";
            row10["PRESS_VAL"] = "152";
            row10["LAST_RUN_TIME"] = "2020-11-24 07:22:45";
            row10["ROW_IDX"] = "11";
            row10["COL_ID"] = "자동차 1-1호 Lane 1호";
            row10["ROW_ID"] = "HOT_07";
            dt.Rows.Add(row10);

            DataRow row11 = dt.NewRow();
            row11["EQPTID"] = "X11JF10508";
            row11["ROW"] = "01";
            row11["COL"] = "05";
            row11["STG"] = "08";
            row11["CSTID"] = "";
            row11["LOTID"] = "";
            row11["EIOSTAT"] = "W";
            row11["PORT_STAT_CODE"] = "WAIT";
            row11["RUN_MODE_CD"] = "R";
            row11["PROCID"] = "";
            row11["PROCNAME"] = "";
            row11["ISS_RSV_FLAG"] = "";
            row11["WIPSTAT"] = "";
            row11["NEXT_PROCID"] = "";
            row11["FORMSTATUS"] = "11";
            row11["EQP_MAINT_DISPLAY"] = "N";
            row11["USE_YN"] = "Y";
            row11["PROD_LOTID"] = "";
            row11["OP_START_TIME"] = "";
            row11["OP_OVER_TIME"] = "";
            row11["TEMP_VAL"] = "520";
            row11["PRESS_VAL"] = "0";
            row11["LAST_RUN_TIME"] = "2020-11-24 07:22:46";
            row11["ROW_IDX"] = "12";
            row11["COL_ID"] = "자동차 1-1호 Lane 1호";
            row11["ROW_ID"] = "HOT_08";
            dt.Rows.Add(row11);

            DataRow row12 = dt.NewRow();
            row12["EQPTID"] = "X11JF10509";
            row12["ROW"] = "01";
            row12["COL"] = "05";
            row12["STG"] = "09";
            row12["CSTID"] = "";
            row12["LOTID"] = "";
            row12["EIOSTAT"] = "W";
            row12["PORT_STAT_CODE"] = "WAIT";
            row12["RUN_MODE_CD"] = "R";
            row12["PROCID"] = "";
            row12["PROCNAME"] = "";
            row12["ISS_RSV_FLAG"] = "";
            row12["WIPSTAT"] = "";
            row12["NEXT_PROCID"] = "";
            row12["FORMSTATUS"] = "11";
            row12["EQP_MAINT_DISPLAY"] = "N";
            row12["USE_YN"] = "Y";
            row12["PROD_LOTID"] = "";
            row12["OP_START_TIME"] = "";
            row12["OP_OVER_TIME"] = "";
            row12["TEMP_VAL"] = "519";
            row12["PRESS_VAL"] = "0";
            row12["LAST_RUN_TIME"] = "2020-11-24 07:22:47";
            row12["ROW_IDX"] = "13";
            row12["COL_ID"] = "자동차 1-1호 Lane 1호";
            row12["ROW_ID"] = "HOT_09";
            dt.Rows.Add(row12);

            DataRow row13 = dt.NewRow();
            row13["EQPTID"] = "X11JF10510";
            row13["ROW"] = "01";
            row13["COL"] = "05";
            row13["STG"] = "10";
            row13["CSTID"] = "";
            row13["LOTID"] = "";
            row13["EIOSTAT"] = "W";
            row13["PORT_STAT_CODE"] = "WAIT";
            row13["RUN_MODE_CD"] = "R";
            row13["PROCID"] = "";
            row13["PROCNAME"] = "";
            row13["ISS_RSV_FLAG"] = "";
            row13["WIPSTAT"] = "";
            row13["NEXT_PROCID"] = "";
            row13["FORMSTATUS"] = "11";
            row13["EQP_MAINT_DISPLAY"] = "N";
            row13["USE_YN"] = "Y";
            row13["PROD_LOTID"] = "";
            row13["OP_START_TIME"] = "";
            row13["OP_OVER_TIME"] = "";
            row13["TEMP_VAL"] = "519";
            row13["PRESS_VAL"] = "0";
            row13["LAST_RUN_TIME"] = "2020-11-24 07:22:48";
            row13["ROW_IDX"] = "14";
            row13["COL_ID"] = "자동차 1-1호 Lane 1호";
            row13["ROW_ID"] = "HOT_10";
            dt.Rows.Add(row13);

            DataRow row14 = dt.NewRow();
            row14["EQPTID"] = "X11JF10511";
            row14["ROW"] = "01";
            row14["COL"] = "05";
            row14["STG"] = "11";
            row14["CSTID"] = "";
            row14["LOTID"] = "";
            row14["EIOSTAT"] = "W";
            row14["PORT_STAT_CODE"] = "WAIT";
            row14["RUN_MODE_CD"] = "R";
            row14["PROCID"] = "";
            row14["PROCNAME"] = "";
            row14["ISS_RSV_FLAG"] = "";
            row14["WIPSTAT"] = "";
            row14["NEXT_PROCID"] = "";
            row14["FORMSTATUS"] = "11";
            row14["EQP_MAINT_DISPLAY"] = "N";
            row14["USE_YN"] = "Y";
            row14["PROD_LOTID"] = "";
            row14["OP_START_TIME"] = "";
            row14["OP_OVER_TIME"] = "";
            row14["TEMP_VAL"] = "519";
            row14["PRESS_VAL"] = "0";
            row14["LAST_RUN_TIME"] = "2020-11-24 07:22:49";
            row14["ROW_IDX"] = "15";
            row14["COL_ID"] = "자동차 1-1호 Lane 1호";
            row14["ROW_ID"] = "HOT_11";
            dt.Rows.Add(row14);

            DataRow row15 = dt.NewRow();
            row15["EQPTID"] = "X11JF10512";
            row15["ROW"] = "01";
            row15["COL"] = "05";
            row15["STG"] = "12";
            row15["CSTID"] = "";
            row15["LOTID"] = "";
            row15["EIOSTAT"] = "W";
            row15["PORT_STAT_CODE"] = "WAIT";
            row15["RUN_MODE_CD"] = "R";
            row15["PROCID"] = "";
            row15["PROCNAME"] = "";
            row15["ISS_RSV_FLAG"] = "";
            row15["WIPSTAT"] = "";
            row15["NEXT_PROCID"] = "";
            row15["FORMSTATUS"] = "11";
            row15["EQP_MAINT_DISPLAY"] = "N";
            row15["USE_YN"] = "Y";
            row15["PROD_LOTID"] = "";
            row15["OP_START_TIME"] = "";
            row15["OP_OVER_TIME"] = "";
            row15["TEMP_VAL"] = "520";
            row15["PRESS_VAL"] = "0";
            row15["LAST_RUN_TIME"] = "2020-11-24 07:22:50";
            row15["ROW_IDX"] = "16";
            row15["COL_ID"] = "자동차 1-1호 Lane 1호";
            row15["ROW_ID"] = "HOT_12";
            dt.Rows.Add(row15);

            DataRow row16 = dt.NewRow();
            row16["EQPTID"] = "X11JF10513";
            row16["ROW"] = "01";
            row16["COL"] = "05";
            row16["STG"] = "13";
            row16["CSTID"] = "";
            row16["LOTID"] = "";
            row16["EIOSTAT"] = "W";
            row16["PORT_STAT_CODE"] = "WAIT";
            row16["RUN_MODE_CD"] = "R";
            row16["PROCID"] = "";
            row16["PROCNAME"] = "";
            row16["ISS_RSV_FLAG"] = "";
            row16["WIPSTAT"] = "";
            row16["NEXT_PROCID"] = "";
            row16["FORMSTATUS"] = "11";
            row16["EQP_MAINT_DISPLAY"] = "N";
            row16["USE_YN"] = "Y";
            row16["PROD_LOTID"] = "";
            row16["OP_START_TIME"] = "";
            row16["OP_OVER_TIME"] = "";
            row16["TEMP_VAL"] = "519";
            row16["PRESS_VAL"] = "0";
            row16["LAST_RUN_TIME"] = "2020-11-24 07:22:51";
            row16["ROW_IDX"] = "17";
            row16["COL_ID"] = "자동차 1-1호 Lane 1호";
            row16["ROW_ID"] = "HOT_13";
            dt.Rows.Add(row16);

            DataRow row17 = dt.NewRow();
            row17["EQPTID"] = "X11JF20701";
            row17["ROW"] = "02";
            row17["COL"] = "07";
            row17["STG"] = "01";
            row17["CSTID"] = "LJIG102001";
            row17["LOTID"] = "5258453";
            row17["EIOSTAT"] = "R";
            row17["PORT_STAT_CODE"] = "WAIT";
            row17["RUN_MODE_CD"] = "R";
            row17["PROCID"] = "J71";
            row17["PROCNAME"] = "J/F PP #01";
            row17["ISS_RSV_FLAG"] = "S";
            row17["WIPSTAT"] = "PROC";
            row17["NEXT_PROCID"] = "J31";
            row17["FORMSTATUS"] = "J7";
            row17["EQP_MAINT_DISPLAY"] = "N";
            row17["USE_YN"] = "Y";
            row17["PROD_LOTID"] = "UAJTK302C1";
            row17["OP_START_TIME"] = "2020-11-24 07:22:36";
            row17["OP_OVER_TIME"] = "00-00 00:04";
            row17["TEMP_VAL"] = "702";
            row17["PRESS_VAL"] = "1167";
            row17["LAST_RUN_TIME"] = "2020-11-24 07:22:52";
            row17["ROW_IDX"] = "1";
            row17["COL_ID"] = "자동차 1-1호 Lane 2호";
            row17["ROW_ID"] = "PP_01";
            dt.Rows.Add(row17);

            DataRow row18 = dt.NewRow();
            row18["EQPTID"] = "X11JF20702";
            row18["ROW"] = "02";
            row18["COL"] = "07";
            row18["STG"] = "02";
            row18["CSTID"] = "LJIG102048";
            row18["LOTID"] = "5258532";
            row18["EIOSTAT"] = "R";
            row18["PORT_STAT_CODE"] = "WAIT";
            row18["RUN_MODE_CD"] = "R";
            row18["PROCID"] = "J71";
            row18["PROCNAME"] = "J/F PP #01";
            row18["ISS_RSV_FLAG"] = "S";
            row18["WIPSTAT"] = "PROC";
            row18["NEXT_PROCID"] = "J31";
            row18["FORMSTATUS"] = "J7";
            row18["EQP_MAINT_DISPLAY"] = "N";
            row18["USE_YN"] = "Y";
            row18["PROD_LOTID"] = "UAJTK302C1";
            row18["OP_START_TIME"] = "2020-11-24 07:22:36";
            row18["OP_OVER_TIME"] = "00-00 00:01";
            row18["TEMP_VAL"] = "698";
            row18["PRESS_VAL"] = "978";
            row18["LAST_RUN_TIME"] = "2020-11-24 07:22:53";
            row18["ROW_IDX"] = "2";
            row18["COL_ID"] = "자동차 1-1호 Lane 2호";
            row18["ROW_ID"] = "PP_02";
            dt.Rows.Add(row18);

            DataRow row19 = dt.NewRow();
            row19["EQPTID"] = "X11JF20703";
            row19["ROW"] = "02";
            row19["COL"] = "07";
            row19["STG"] = "03";
            row19["CSTID"] = "";
            row19["LOTID"] = "";
            row19["EIOSTAT"] = "W";
            row19["PORT_STAT_CODE"] = "WAIT";
            row19["RUN_MODE_CD"] = "R";
            row19["PROCID"] = "";
            row19["PROCNAME"] = "";
            row19["ISS_RSV_FLAG"] = "";
            row19["WIPSTAT"] = "";
            row19["NEXT_PROCID"] = "";
            row19["FORMSTATUS"] = "11";
            row19["EQP_MAINT_DISPLAY"] = "N";
            row19["USE_YN"] = "Y";
            row19["PROD_LOTID"] = "";
            row19["OP_START_TIME"] = "";
            row19["OP_OVER_TIME"] = "";
            row19["TEMP_VAL"] = "700";
            row19["PRESS_VAL"] = "1166";
            row19["LAST_RUN_TIME"] = "2020-11-24 07:22:54";
            row19["ROW_IDX"] = "3";
            row19["COL_ID"] = "자동차 1-1호 Lane 2호";
            row19["ROW_ID"] = "PP_03";
            dt.Rows.Add(row19);

            DataRow row20 = dt.NewRow();
            row20["EQPTID"] = "X11JF20501";
            row20["ROW"] = "02";
            row20["COL"] = "05";
            row20["STG"] = "01";
            row20["CSTID"] = "LJIG102016";
            row20["LOTID"] = "5257615";
            row20["EIOSTAT"] = "R";
            row20["PORT_STAT_CODE"] = "WAIT";
            row20["RUN_MODE_CD"] = "R";
            row20["PROCID"] = "J13";
            row20["PROCNAME"] = "J/F Charge #03";
            row20["ISS_RSV_FLAG"] = "S";
            row20["WIPSTAT"] = "PROC";
            row20["NEXT_PROCID"] = "J32";
            row20["FORMSTATUS"] = "J1";
            row20["EQP_MAINT_DISPLAY"] = "N";
            row20["USE_YN"] = "Y";
            row20["PROD_LOTID"] = "UAJTK302C1";
            row20["OP_START_TIME"] = "2020-11-24 07:22:36";
            row20["OP_OVER_TIME"] = "00-00 00:25";
            row20["TEMP_VAL"] = "533";
            row20["PRESS_VAL"] = "1466";
            row20["LAST_RUN_TIME"] = "2020-11-24 07:22:55";
            row20["ROW_IDX"] = "5";
            row20["COL_ID"] = "자동차 1-1호 Lane 2호";
            row20["ROW_ID"] = "HOT_01";
            dt.Rows.Add(row20);

            DataRow row21 = dt.NewRow();
            row21["EQPTID"] = "X11JF20502";
            row21["ROW"] = "02";
            row21["COL"] = "05";
            row21["STG"] = "02";
            row21["CSTID"] = "LJIG102002";
            row21["LOTID"] = "5258294";
            row21["EIOSTAT"] = "R";
            row21["PORT_STAT_CODE"] = "WAIT";
            row21["RUN_MODE_CD"] = "R";
            row21["PROCID"] = "J11";
            row21["PROCNAME"] = "J/F Charge #01";
            row21["ISS_RSV_FLAG"] = "S";
            row21["WIPSTAT"] = "PROC";
            row21["NEXT_PROCID"] = "BJ2";
            row21["FORMSTATUS"] = "J1";
            row21["EQP_MAINT_DISPLAY"] = "N";
            row21["USE_YN"] = "Y";
            row21["PROD_LOTID"] = "UAJTK302C1";
            row21["OP_START_TIME"] = "2020-11-24 07:22:37";
            row21["OP_OVER_TIME"] = "00-00 00:03";
            row21["TEMP_VAL"] = "531";
            row21["PRESS_VAL"] = "0";
            row21["LAST_RUN_TIME"] = "2020-11-25 07:22:01";
            row21["ROW_IDX"] = "6";
            row21["COL_ID"] = "자동차 1-1호 Lane 2호";
            row21["ROW_ID"] = "HOT_02";
            dt.Rows.Add(row21);

            DataRow row22 = dt.NewRow();
            row22["EQPTID"] = "X11JF20503";
            row22["ROW"] = "02";
            row22["COL"] = "05";
            row22["STG"] = "03";
            row22["CSTID"] = "LJIG102030";
            row22["LOTID"] = "5258373";
            row22["EIOSTAT"] = "R";
            row22["PORT_STAT_CODE"] = "WAIT";
            row22["RUN_MODE_CD"] = "R";
            row22["PROCID"] = "J71";
            row22["PROCNAME"] = "J/F PP #01";
            row22["ISS_RSV_FLAG"] = "E";
            row22["WIPSTAT"] = "WAIT";
            row22["NEXT_PROCID"] = "J31";
            row22["FORMSTATUS"] = "J7";
            row22["EQP_MAINT_DISPLAY"] = "N";
            row22["USE_YN"] = "Y";
            row22["PROD_LOTID"] = "UAJTK302C1";
            row22["OP_START_TIME"] = "2020-11-24 07:22:38";
            row22["OP_OVER_TIME"] = "00-00 00:08";
            row22["TEMP_VAL"] = "532";
            row22["PRESS_VAL"] = "18";
            row22["LAST_RUN_TIME"] = "2020-11-25 07:22:02";
            row22["ROW_IDX"] = "7";
            row22["COL_ID"] = "자동차 1-1호 Lane 2호";
            row22["ROW_ID"] = "HOT_03";
            dt.Rows.Add(row22);

            DataRow row23 = dt.NewRow();
            row23["EQPTID"] = "X11JF20504";
            row23["ROW"] = "02";
            row23["COL"] = "05";
            row23["STG"] = "04";
            row23["CSTID"] = "LJIG102014";
            row23["LOTID"] = "5257673";
            row23["EIOSTAT"] = "R";
            row23["PORT_STAT_CODE"] = "WAIT";
            row23["RUN_MODE_CD"] = "R";
            row23["PROCID"] = "J13";
            row23["PROCNAME"] = "J/F Charge #03";
            row23["ISS_RSV_FLAG"] = "S";
            row23["WIPSTAT"] = "PROC";
            row23["NEXT_PROCID"] = "J32";
            row23["FORMSTATUS"] = "J1";
            row23["EQP_MAINT_DISPLAY"] = "N";
            row23["USE_YN"] = "Y";
            row23["PROD_LOTID"] = "UAJTK302C1";
            row23["OP_START_TIME"] = "2020-11-24 07:22:39";
            row23["OP_OVER_TIME"] = "00-00 00:22";
            row23["TEMP_VAL"] = "536";
            row23["PRESS_VAL"] = "1467";
            row23["LAST_RUN_TIME"] = "2020-11-25 07:22:03";
            row23["ROW_IDX"] = "8";
            row23["COL_ID"] = "자동차 1-1호 Lane 2호";
            row23["ROW_ID"] = "HOT_04";
            dt.Rows.Add(row23);

            DataRow row24 = dt.NewRow();
            row24["EQPTID"] = "X11JF20505";
            row24["ROW"] = "02";
            row24["COL"] = "05";
            row24["STG"] = "05";
            row24["CSTID"] = "LJIG102022";
            row24["LOTID"] = "5257739";
            row24["EIOSTAT"] = "R";
            row24["PORT_STAT_CODE"] = "WAIT";
            row24["RUN_MODE_CD"] = "R";
            row24["PROCID"] = "J13";
            row24["PROCNAME"] = "J/F Charge #03";
            row24["ISS_RSV_FLAG"] = "S";
            row24["WIPSTAT"] = "PROC";
            row24["NEXT_PROCID"] = "J32";
            row24["FORMSTATUS"] = "J1";
            row24["EQP_MAINT_DISPLAY"] = "N";
            row24["USE_YN"] = "Y";
            row24["PROD_LOTID"] = "UAJTK302C1";
            row24["OP_START_TIME"] = "2020-11-24 07:22:40";
            row24["OP_OVER_TIME"] = "00-00 00:18";
            row24["TEMP_VAL"] = "527";
            row24["PRESS_VAL"] = "1469";
            row24["LAST_RUN_TIME"] = "2020-11-25 07:22:04";
            row24["ROW_IDX"] = "9";
            row24["COL_ID"] = "자동차 1-1호 Lane 2호";
            row24["ROW_ID"] = "HOT_05";
            dt.Rows.Add(row24);

            DataRow row25 = dt.NewRow();
            row25["EQPTID"] = "X11JF20506";
            row25["ROW"] = "02";
            row25["COL"] = "05";
            row25["STG"] = "06";
            row25["CSTID"] = "LJIG102028";
            row25["LOTID"] = "5257872";
            row25["EIOSTAT"] = "R";
            row25["PORT_STAT_CODE"] = "WAIT";
            row25["RUN_MODE_CD"] = "R";
            row25["PROCID"] = "J13";
            row25["PROCNAME"] = "J/F Charge #03";
            row25["ISS_RSV_FLAG"] = "S";
            row25["WIPSTAT"] = "PROC";
            row25["NEXT_PROCID"] = "J32";
            row25["FORMSTATUS"] = "J1";
            row25["EQP_MAINT_DISPLAY"] = "N";
            row25["USE_YN"] = "Y";
            row25["PROD_LOTID"] = "UAJTK302C1";
            row25["OP_START_TIME"] = "2020-11-24 07:22:41";
            row25["OP_OVER_TIME"] = "00-00 00:10";
            row25["TEMP_VAL"] = "538";
            row25["PRESS_VAL"] = "1468";
            row25["LAST_RUN_TIME"] = "2020-11-25 07:22:05";
            row25["ROW_IDX"] = "10";
            row25["COL_ID"] = "자동차 1-1호 Lane 2호";
            row25["ROW_ID"] = "HOT_06";
            dt.Rows.Add(row25);

            DataRow row26 = dt.NewRow();
            row26["EQPTID"] = "X11JF20507";
            row26["ROW"] = "02";
            row26["COL"] = "05";
            row26["STG"] = "07";
            row26["CSTID"] = "LJIG102011";
            row26["LOTID"] = "5258151";
            row26["EIOSTAT"] = "R";
            row26["PORT_STAT_CODE"] = "WAIT";
            row26["RUN_MODE_CD"] = "R";
            row26["PROCID"] = "J12";
            row26["PROCNAME"] = "J/F Charge #02";
            row26["ISS_RSV_FLAG"] = "S";
            row26["WIPSTAT"] = "PROC";
            row26["NEXT_PROCID"] = "BJ3";
            row26["FORMSTATUS"] = "J1";
            row26["EQP_MAINT_DISPLAY"] = "N";
            row26["USE_YN"] = "Y";
            row26["PROD_LOTID"] = "UAJTK302C1";
            row26["OP_START_TIME"] = "2020-11-24 07:22:42";
            row26["OP_OVER_TIME"] = "00-00 00:05";
            row26["TEMP_VAL"] = "535";
            row26["PRESS_VAL"] = "216";
            row26["LAST_RUN_TIME"] = "2020-11-25 07:22:06";
            row26["ROW_IDX"] = "11";
            row26["COL_ID"] = "자동차 1-1호 Lane 2호";
            row26["ROW_ID"] = "HOT_07";
            dt.Rows.Add(row26);

            DataRow row27 = dt.NewRow();
            row27["EQPTID"] = "X11JF20508";
            row27["ROW"] = "02";
            row27["COL"] = "05";
            row27["STG"] = "08";
            row27["CSTID"] = "LJIG102005";
            row27["LOTID"] = "5257928";
            row27["EIOSTAT"] = "R";
            row27["PORT_STAT_CODE"] = "WAIT";
            row27["RUN_MODE_CD"] = "R";
            row27["PROCID"] = "J13";
            row27["PROCNAME"] = "J/F Charge #03";
            row27["ISS_RSV_FLAG"] = "S";
            row27["WIPSTAT"] = "PROC";
            row27["NEXT_PROCID"] = "J32";
            row27["FORMSTATUS"] = "J1";
            row27["EQP_MAINT_DISPLAY"] = "N";
            row27["USE_YN"] = "Y";
            row27["PROD_LOTID"] = "UAJTK302C1";
            row27["OP_START_TIME"] = "2020-11-24 07:22:43";
            row27["OP_OVER_TIME"] = "00-00 00:04";
            row27["TEMP_VAL"] = "549";
            row27["PRESS_VAL"] = "288";
            row27["LAST_RUN_TIME"] = "2020-11-25 07:22:07";
            row27["ROW_IDX"] = "12";
            row27["COL_ID"] = "자동차 1-1호 Lane 2호";
            row27["ROW_ID"] = "HOT_08";
            dt.Rows.Add(row27);

            DataRow row28 = dt.NewRow();
            row28["EQPTID"] = "X11JF20509";
            row28["ROW"] = "02";
            row28["COL"] = "05";
            row28["STG"] = "09";
            row28["CSTID"] = "LJIG102021";
            row28["LOTID"] = "5258001";
            row28["EIOSTAT"] = "R";
            row28["PORT_STAT_CODE"] = "WAIT";
            row28["RUN_MODE_CD"] = "R";
            row28["PROCID"] = "J13";
            row28["PROCNAME"] = "J/F Charge #03";
            row28["ISS_RSV_FLAG"] = "S";
            row28["WIPSTAT"] = "PROC";
            row28["NEXT_PROCID"] = "J32";
            row28["FORMSTATUS"] = "J1";
            row28["EQP_MAINT_DISPLAY"] = "N";
            row28["USE_YN"] = "Y";
            row28["PROD_LOTID"] = "UAJTK302C1";
            row28["OP_START_TIME"] = "2020-11-24 07:22:44";
            row28["OP_OVER_TIME"] = "00-00 00:02";
            row28["TEMP_VAL"] = "541";
            row28["PRESS_VAL"] = "164";
            row28["LAST_RUN_TIME"] = "2020-11-25 07:22:08";
            row28["ROW_IDX"] = "13";
            row28["COL_ID"] = "자동차 1-1호 Lane 2호";
            row28["ROW_ID"] = "HOT_09";
            dt.Rows.Add(row28);

            DataRow row29 = dt.NewRow();
            row29["EQPTID"] = "X11JF20510";
            row29["ROW"] = "02";
            row29["COL"] = "05";
            row29["STG"] = "10";
            row29["CSTID"] = "LJIG102027";
            row29["LOTID"] = "5257546";
            row29["EIOSTAT"] = "W";
            row29["PORT_STAT_CODE"] = "WAIT";
            row29["RUN_MODE_CD"] = "R";
            row29["PROCID"] = "BJ4";
            row29["PROCNAME"] = "J/F 判定 #04";
            row29["ISS_RSV_FLAG"] = "E";
            row29["WIPSTAT"] = "WAIT";
            row29["NEXT_PROCID"] = "J99";
            row29["FORMSTATUS"] = "11";
            row29["EQP_MAINT_DISPLAY"] = "N";
            row29["USE_YN"] = "Y";
            row29["PROD_LOTID"] = "UAJTK302C1";
            row29["OP_START_TIME"] = "2020-11-24 07:22:45";
            row29["OP_OVER_TIME"] = "00-00 00:01";
            row29["TEMP_VAL"] = "530";
            row29["PRESS_VAL"] = "1468";
            row29["LAST_RUN_TIME"] = "2020-11-25 07:22:09";
            row29["ROW_IDX"] = "14";
            row29["COL_ID"] = "자동차 1-1호 Lane 2호";
            row29["ROW_ID"] = "HOT_10";
            dt.Rows.Add(row29);

            DataRow row30 = dt.NewRow();
            row30["EQPTID"] = "X11JF20511";
            row30["ROW"] = "02";
            row30["COL"] = "05";
            row30["STG"] = "11";
            row30["CSTID"] = "LJIG102029";
            row30["LOTID"] = "5257815";
            row30["EIOSTAT"] = "R";
            row30["PORT_STAT_CODE"] = "WAIT";
            row30["RUN_MODE_CD"] = "R";
            row30["PROCID"] = "J13";
            row30["PROCNAME"] = "J/F Charge #03";
            row30["ISS_RSV_FLAG"] = "S";
            row30["WIPSTAT"] = "PROC";
            row30["NEXT_PROCID"] = "J32";
            row30["FORMSTATUS"] = "J1";
            row30["EQP_MAINT_DISPLAY"] = "N";
            row30["USE_YN"] = "Y";
            row30["PROD_LOTID"] = "UAJTK302C1";
            row30["OP_START_TIME"] = "2020-11-24 07:22:46";
            row30["OP_OVER_TIME"] = "00-00 00:13";
            row30["TEMP_VAL"] = "530";
            row30["PRESS_VAL"] = "1467";
            row30["LAST_RUN_TIME"] = "2020-11-25 07:22:10";
            row30["ROW_IDX"] = "15";
            row30["COL_ID"] = "자동차 1-1호 Lane 2호";
            row30["ROW_ID"] = "HOT_11";
            dt.Rows.Add(row30);

            DataRow row31 = dt.NewRow();
            row31["EQPTID"] = "X11JF20512";
            row31["ROW"] = "02";
            row31["COL"] = "05";
            row31["STG"] = "12";
            row31["CSTID"] = "LJIG102009";
            row31["LOTID"] = "5258070";
            row31["EIOSTAT"] = "R";
            row31["PORT_STAT_CODE"] = "WAIT";
            row31["RUN_MODE_CD"] = "R";
            row31["PROCID"] = "J12";
            row31["PROCNAME"] = "J/F Charge #02";
            row31["ISS_RSV_FLAG"] = "S";
            row31["WIPSTAT"] = "PROC";
            row31["NEXT_PROCID"] = "BJ3";
            row31["FORMSTATUS"] = "J1";
            row31["EQP_MAINT_DISPLAY"] = "N";
            row31["USE_YN"] = "Y";
            row31["PROD_LOTID"] = "UAJTK302C1";
            row31["OP_START_TIME"] = "2020-11-24 07:22:47";
            row31["OP_OVER_TIME"] = "00-00 00:10";
            row31["TEMP_VAL"] = "537";
            row31["PRESS_VAL"] = "166";
            row31["LAST_RUN_TIME"] = "2020-11-25 07:22:11";
            row31["ROW_IDX"] = "16";
            row31["COL_ID"] = "자동차 1-1호 Lane 2호";
            row31["ROW_ID"] = "HOT_12";
            dt.Rows.Add(row31);

            DataRow row32 = dt.NewRow();
            row32["EQPTID"] = "X11JF20513";
            row32["ROW"] = "02";
            row32["COL"] = "05";
            row32["STG"] = "13";
            row32["CSTID"] = "LJIG102013";
            row32["LOTID"] = "5258231";
            row32["EIOSTAT"] = "R";
            row32["PORT_STAT_CODE"] = "WAIT";
            row32["RUN_MODE_CD"] = "R";
            row32["PROCID"] = "J12";
            row32["PROCNAME"] = "J/F Charge #02";
            row32["ISS_RSV_FLAG"] = "S";
            row32["WIPSTAT"] = "PROC";
            row32["NEXT_PROCID"] = "BJ3";
            row32["FORMSTATUS"] = "J1";
            row32["EQP_MAINT_DISPLAY"] = "N";
            row32["USE_YN"] = "Y";
            row32["PROD_LOTID"] = "UAJTK302C1";
            row32["OP_START_TIME"] = "2020-11-24 07:22:48";
            row32["OP_OVER_TIME"] = "00-00 00:03";
            row32["TEMP_VAL"] = "529";
            row32["PRESS_VAL"] = "242";
            row32["LAST_RUN_TIME"] = "2020-11-25 07:22:12";
            row32["ROW_IDX"] = "17";
            row32["COL_ID"] = "자동차 1-1호 Lane 2호";
            row32["ROW_ID"] = "HOT_13";
            dt.Rows.Add(row32);

            DataRow row33 = dt.NewRow();
            row33["EQPTID"] = "X11JF30701";
            row33["ROW"] = "03";
            row33["COL"] = "07";
            row33["STG"] = "01";
            row33["CSTID"] = "";
            row33["LOTID"] = "";
            row33["EIOSTAT"] = "R";
            row33["PORT_STAT_CODE"] = "WAIT";
            row33["RUN_MODE_CD"] = "R";
            row33["PROCID"] = "";
            row33["PROCNAME"] = "";
            row33["ISS_RSV_FLAG"] = "";
            row33["WIPSTAT"] = "";
            row33["NEXT_PROCID"] = "";
            row33["FORMSTATUS"] = "0";
            row33["EQP_MAINT_DISPLAY"] = "N";
            row33["USE_YN"] = "Y";
            row33["PROD_LOTID"] = "";
            row33["OP_START_TIME"] = "";
            row33["OP_OVER_TIME"] = "";
            row33["TEMP_VAL"] = "679";
            row33["PRESS_VAL"] = "0";
            row33["LAST_RUN_TIME"] = "2020-11-25 07:22:13";
            row33["ROW_IDX"] = "1";
            row33["COL_ID"] = "자동차 1-1호 Lane 3호";
            row33["ROW_ID"] = "PP_01";
            dt.Rows.Add(row33);

            DataRow row34 = dt.NewRow();
            row34["EQPTID"] = "X11JF30702";
            row34["ROW"] = "03";
            row34["COL"] = "07";
            row34["STG"] = "02";
            row34["CSTID"] = "LJIG103040";
            row34["LOTID"] = "5258416";
            row34["EIOSTAT"] = "W";
            row34["PORT_STAT_CODE"] = "WAIT";
            row34["RUN_MODE_CD"] = "R";
            row34["PROCID"] = "J71";
            row34["PROCNAME"] = "J/F PP #01";
            row34["ISS_RSV_FLAG"] = "E";
            row34["WIPSTAT"] = "WAIT";
            row34["NEXT_PROCID"] = "J31";
            row34["FORMSTATUS"] = "11";
            row34["EQP_MAINT_DISPLAY"] = "N";
            row34["USE_YN"] = "Y";
            row34["PROD_LOTID"] = "UAATK301C3";
            row34["OP_START_TIME"] = "2020-11-24 07:22:48";
            row34["OP_OVER_TIME"] = "00-00 00:08";
            row34["TEMP_VAL"] = "681";
            row34["PRESS_VAL"] = "2136";
            row34["LAST_RUN_TIME"] = "2020-11-25 07:22:14";
            row34["ROW_IDX"] = "2";
            row34["COL_ID"] = "자동차 1-1호 Lane 3호";
            row34["ROW_ID"] = "PP_02";
            dt.Rows.Add(row34);

            DataRow row35 = dt.NewRow();
            row35["EQPTID"] = "X11JF30703";
            row35["ROW"] = "03";
            row35["COL"] = "07";
            row35["STG"] = "03";
            row35["CSTID"] = "LJIG103010";
            row35["LOTID"] = "5258475";
            row35["EIOSTAT"] = "R";
            row35["PORT_STAT_CODE"] = "WAIT";
            row35["RUN_MODE_CD"] = "R";
            row35["PROCID"] = "J71";
            row35["PROCNAME"] = "J/F PP #01";
            row35["ISS_RSV_FLAG"] = "S";
            row35["WIPSTAT"] = "PROC";
            row35["NEXT_PROCID"] = "J31";
            row35["FORMSTATUS"] = "J7";
            row35["EQP_MAINT_DISPLAY"] = "N";
            row35["USE_YN"] = "Y";
            row35["PROD_LOTID"] = "UAATK301C3";
            row35["OP_START_TIME"] = "2020-11-24 07:22:49";
            row35["OP_OVER_TIME"] = "00-00 00:01";
            row35["TEMP_VAL"] = "679";
            row35["PRESS_VAL"] = "0";
            row35["LAST_RUN_TIME"] = "2020-11-25 07:22:15";
            row35["ROW_IDX"] = "3";
            row35["COL_ID"] = "자동차 1-1호 Lane 3호";
            row35["ROW_ID"] = "PP_03";
            dt.Rows.Add(row35);

            DataRow row36 = dt.NewRow();
            row36["EQPTID"] = "X11JF30501";
            row36["ROW"] = "03";
            row36["COL"] = "05";
            row36["STG"] = "01";
            row36["CSTID"] = "LJIG103017";
            row36["LOTID"] = "5257641";
            row36["EIOSTAT"] = "R";
            row36["PORT_STAT_CODE"] = "WAIT";
            row36["RUN_MODE_CD"] = "R";
            row36["PROCID"] = "J13";
            row36["PROCNAME"] = "J/F Charge #03";
            row36["ISS_RSV_FLAG"] = "S";
            row36["WIPSTAT"] = "PROC";
            row36["NEXT_PROCID"] = "J32";
            row36["FORMSTATUS"] = "J1";
            row36["EQP_MAINT_DISPLAY"] = "N";
            row36["USE_YN"] = "Y";
            row36["PROD_LOTID"] = "UAATK301C3";
            row36["OP_START_TIME"] = "2020-11-24 07:22:50";
            row36["OP_OVER_TIME"] = "00-00 00:25";
            row36["TEMP_VAL"] = "541";
            row36["PRESS_VAL"] = "388";
            row36["LAST_RUN_TIME"] = "2020-11-25 07:22:16";
            row36["ROW_IDX"] = "5";
            row36["COL_ID"] = "자동차 1-1호 Lane 3호";
            row36["ROW_ID"] = "HOT_01";
            dt.Rows.Add(row36);

            DataRow row37 = dt.NewRow();
            row37["EQPTID"] = "X11JF30502";
            row37["ROW"] = "03";
            row37["COL"] = "05";
            row37["STG"] = "02";
            row37["CSTID"] = "LJIG103005";
            row37["LOTID"] = "5258205";
            row37["EIOSTAT"] = "R";
            row37["PORT_STAT_CODE"] = "WAIT";
            row37["RUN_MODE_CD"] = "R";
            row37["PROCID"] = "J12";
            row37["PROCNAME"] = "J/F Charge #02";
            row37["ISS_RSV_FLAG"] = "S";
            row37["WIPSTAT"] = "PROC";
            row37["NEXT_PROCID"] = "BJ2";
            row37["FORMSTATUS"] = "J1";
            row37["EQP_MAINT_DISPLAY"] = "N";
            row37["USE_YN"] = "Y";
            row37["PROD_LOTID"] = "UAATK301C3";
            row37["OP_START_TIME"] = "2020-11-24 07:22:51";
            row37["OP_OVER_TIME"] = "00-00 00:05";
            row37["TEMP_VAL"] = "536";
            row37["PRESS_VAL"] = "191";
            row37["LAST_RUN_TIME"] = "2020-11-25 07:22:17";
            row37["ROW_IDX"] = "6";
            row37["COL_ID"] = "자동차 1-1호 Lane 3호";
            row37["ROW_ID"] = "HOT_02";
            dt.Rows.Add(row37);

            DataRow row38 = dt.NewRow();
            row38["EQPTID"] = "X11JF30503";
            row38["ROW"] = "03";
            row38["COL"] = "05";
            row38["STG"] = "03";
            row38["CSTID"] = "LJIG103044";
            row38["LOTID"] = "5257706";
            row38["EIOSTAT"] = "R";
            row38["PORT_STAT_CODE"] = "WAIT";
            row38["RUN_MODE_CD"] = "R";
            row38["PROCID"] = "J13";
            row38["PROCNAME"] = "J/F Charge #03";
            row38["ISS_RSV_FLAG"] = "S";
            row38["WIPSTAT"] = "PROC";
            row38["NEXT_PROCID"] = "J32";
            row38["FORMSTATUS"] = "J1";
            row38["EQP_MAINT_DISPLAY"] = "N";
            row38["USE_YN"] = "Y";
            row38["PROD_LOTID"] = "UAATK301C3";
            row38["OP_START_TIME"] = "2020-11-24 07:22:52";
            row38["OP_OVER_TIME"] = "00-00 00:23";
            row38["TEMP_VAL"] = "536";
            row38["PRESS_VAL"] = "386";
            row38["LAST_RUN_TIME"] = "2020-11-25 07:22:18";
            row38["ROW_IDX"] = "7";
            row38["COL_ID"] = "자동차 1-1호 Lane 3호";
            row38["ROW_ID"] = "HOT_03";
            dt.Rows.Add(row38);

            DataRow row39 = dt.NewRow();
            row39["EQPTID"] = "X11JF30504";
            row39["ROW"] = "03";
            row39["COL"] = "05";
            row39["STG"] = "04";
            row39["CSTID"] = "LJIG103019";
            row39["LOTID"] = "5257767";
            row39["EIOSTAT"] = "R";
            row39["PORT_STAT_CODE"] = "WAIT";
            row39["RUN_MODE_CD"] = "R";
            row39["PROCID"] = "J13";
            row39["PROCNAME"] = "J/F Charge #03";
            row39["ISS_RSV_FLAG"] = "S";
            row39["WIPSTAT"] = "PROC";
            row39["NEXT_PROCID"] = "J32";
            row39["FORMSTATUS"] = "J1";
            row39["EQP_MAINT_DISPLAY"] = "N";
            row39["USE_YN"] = "Y";
            row39["PROD_LOTID"] = "UAATK301C3";
            row39["OP_START_TIME"] = "2020-11-24 07:22:53";
            row39["OP_OVER_TIME"] = "00-00 00:19";
            row39["TEMP_VAL"] = "533";
            row39["PRESS_VAL"] = "388";
            row39["LAST_RUN_TIME"] = "2020-11-25 07:22:19";
            row39["ROW_IDX"] = "8";
            row39["COL_ID"] = "자동차 1-1호 Lane 3호";
            row39["ROW_ID"] = "HOT_04";
            dt.Rows.Add(row39);

            DataRow row40 = dt.NewRow();
            row40["EQPTID"] = "X11JF30505";
            row40["ROW"] = "03";
            row40["COL"] = "05";
            row40["STG"] = "05";
            row40["CSTID"] = "LJIG103003";
            row40["LOTID"] = "5258153";
            row40["EIOSTAT"] = "R";
            row40["PORT_STAT_CODE"] = "WAIT";
            row40["RUN_MODE_CD"] = "R";
            row40["PROCID"] = "J12";
            row40["PROCNAME"] = "J/F Charge #02";
            row40["ISS_RSV_FLAG"] = "S";
            row40["WIPSTAT"] = "PROC";
            row40["NEXT_PROCID"] = "BJ2";
            row40["FORMSTATUS"] = "J1";
            row40["EQP_MAINT_DISPLAY"] = "N";
            row40["USE_YN"] = "Y";
            row40["PROD_LOTID"] = "UAATK301C3";
            row40["OP_START_TIME"] = "2020-11-24 07:22:54";
            row40["OP_OVER_TIME"] = "00-00 00:07";
            row40["TEMP_VAL"] = "535";
            row40["PRESS_VAL"] = "199";
            row40["LAST_RUN_TIME"] = "2020-11-25 07:22:20";
            row40["ROW_IDX"] = "9";
            row40["COL_ID"] = "자동차 1-1호 Lane 3호";
            row40["ROW_ID"] = "HOT_05";
            dt.Rows.Add(row40);

            DataRow row41 = dt.NewRow();
            row41["EQPTID"] = "X11JF30506";
            row41["ROW"] = "03";
            row41["COL"] = "05";
            row41["STG"] = "06";
            row41["CSTID"] = "LJIG103031";
            row41["LOTID"] = "5258253";
            row41["EIOSTAT"] = "R";
            row41["PORT_STAT_CODE"] = "WAIT";
            row41["RUN_MODE_CD"] = "R";
            row41["PROCID"] = "J12";
            row41["PROCNAME"] = "J/F Charge #02";
            row41["ISS_RSV_FLAG"] = "S";
            row41["WIPSTAT"] = "PROC";
            row41["NEXT_PROCID"] = "BJ2";
            row41["FORMSTATUS"] = "J1";
            row41["EQP_MAINT_DISPLAY"] = "N";
            row41["USE_YN"] = "Y";
            row41["PROD_LOTID"] = "UAATK301C3";
            row41["OP_START_TIME"] = "2020-11-24 07:22:55";
            row41["OP_OVER_TIME"] = "00-00 00:03";
            row41["TEMP_VAL"] = "529";
            row41["PRESS_VAL"] = "223";
            row41["LAST_RUN_TIME"] = "2020-11-25 07:22:21";
            row41["ROW_IDX"] = "10";
            row41["COL_ID"] = "자동차 1-1호 Lane 3호";
            row41["ROW_ID"] = "HOT_06";
            dt.Rows.Add(row41);

            DataRow row42 = dt.NewRow();
            row42["EQPTID"] = "X11JF30507";
            row42["ROW"] = "03";
            row42["COL"] = "05";
            row42["STG"] = "07";
            row42["CSTID"] = "LJIG103006";
            row42["LOTID"] = "5258301";
            row42["EIOSTAT"] = "R";
            row42["PORT_STAT_CODE"] = "WAIT";
            row42["RUN_MODE_CD"] = "R";
            row42["PROCID"] = "J71";
            row42["PROCNAME"] = "J/F PP #01";
            row42["ISS_RSV_FLAG"] = "E";
            row42["WIPSTAT"] = "WAIT";
            row42["NEXT_PROCID"] = "J31";
            row42["FORMSTATUS"] = "J7";
            row42["EQP_MAINT_DISPLAY"] = "N";
            row42["USE_YN"] = "Y";
            row42["PROD_LOTID"] = "UAATK301C3";
            row42["OP_START_TIME"] = "2020-11-24 07:22:56";
            row42["OP_OVER_TIME"] = "00-00 00:10";
            row42["TEMP_VAL"] = "520";
            row42["PRESS_VAL"] = "0";
            row42["LAST_RUN_TIME"] = "2020-11-25 07:22:22";
            row42["ROW_IDX"] = "11";
            row42["COL_ID"] = "자동차 1-1호 Lane 3호";
            row42["ROW_ID"] = "HOT_07";
            dt.Rows.Add(row42);

            DataRow row43 = dt.NewRow();
            row43["EQPTID"] = "X11JF30508";
            row43["ROW"] = "03";
            row43["COL"] = "05";
            row43["STG"] = "08";
            row43["CSTID"] = "";
            row43["LOTID"] = "";
            row43["EIOSTAT"] = "W";
            row43["PORT_STAT_CODE"] = "WAIT";
            row43["RUN_MODE_CD"] = "R";
            row43["PROCID"] = "";
            row43["PROCNAME"] = "";
            row43["ISS_RSV_FLAG"] = "";
            row43["WIPSTAT"] = "";
            row43["NEXT_PROCID"] = "";
            row43["FORMSTATUS"] = "11";
            row43["EQP_MAINT_DISPLAY"] = "N";
            row43["USE_YN"] = "Y";
            row43["PROD_LOTID"] = "";
            row43["OP_START_TIME"] = "";
            row43["OP_OVER_TIME"] = "";
            row43["TEMP_VAL"] = "520";
            row43["PRESS_VAL"] = "0";
            row43["LAST_RUN_TIME"] = "2020-11-25 07:22:23";
            row43["ROW_IDX"] = "12";
            row43["COL_ID"] = "자동차 1-1호 Lane 3호";
            row43["ROW_ID"] = "HOT_08";
            dt.Rows.Add(row43);

            DataRow row44 = dt.NewRow();
            row44["EQPTID"] = "X11JF30509";
            row44["ROW"] = "03";
            row44["COL"] = "05";
            row44["STG"] = "09";
            row44["CSTID"] = "";
            row44["LOTID"] = "";
            row44["EIOSTAT"] = "W";
            row44["PORT_STAT_CODE"] = "WAIT";
            row44["RUN_MODE_CD"] = "R";
            row44["PROCID"] = "";
            row44["PROCNAME"] = "";
            row44["ISS_RSV_FLAG"] = "";
            row44["WIPSTAT"] = "";
            row44["NEXT_PROCID"] = "";
            row44["FORMSTATUS"] = "11";
            row44["EQP_MAINT_DISPLAY"] = "N";
            row44["USE_YN"] = "Y";
            row44["PROD_LOTID"] = "";
            row44["OP_START_TIME"] = "";
            row44["OP_OVER_TIME"] = "";
            row44["TEMP_VAL"] = "520";
            row44["PRESS_VAL"] = "0";
            row44["LAST_RUN_TIME"] = "2020-11-25 07:22:24";
            row44["ROW_IDX"] = "13";
            row44["COL_ID"] = "자동차 1-1호 Lane 3호";
            row44["ROW_ID"] = "HOT_09";
            dt.Rows.Add(row44);

            DataRow row45 = dt.NewRow();
            row45["EQPTID"] = "X11JF30510";
            row45["ROW"] = "03";
            row45["COL"] = "05";
            row45["STG"] = "10";
            row45["CSTID"] = "";
            row45["LOTID"] = "";
            row45["EIOSTAT"] = "W";
            row45["PORT_STAT_CODE"] = "WAIT";
            row45["RUN_MODE_CD"] = "R";
            row45["PROCID"] = "";
            row45["PROCNAME"] = "";
            row45["ISS_RSV_FLAG"] = "";
            row45["WIPSTAT"] = "";
            row45["NEXT_PROCID"] = "";
            row45["FORMSTATUS"] = "11";
            row45["EQP_MAINT_DISPLAY"] = "N";
            row45["USE_YN"] = "Y";
            row45["PROD_LOTID"] = "";
            row45["OP_START_TIME"] = "";
            row45["OP_OVER_TIME"] = "";
            row45["TEMP_VAL"] = "520";
            row45["PRESS_VAL"] = "0";
            row45["LAST_RUN_TIME"] = "2020-11-25 07:22:25";
            row45["ROW_IDX"] = "14";
            row45["COL_ID"] = "자동차 1-1호 Lane 3호";
            row45["ROW_ID"] = "HOT_10";
            dt.Rows.Add(row45);

            DataRow row46 = dt.NewRow();
            row46["EQPTID"] = "X11JF30511";
            row46["ROW"] = "03";
            row46["COL"] = "05";
            row46["STG"] = "11";
            row46["CSTID"] = "";
            row46["LOTID"] = "";
            row46["EIOSTAT"] = "W";
            row46["PORT_STAT_CODE"] = "WAIT";
            row46["RUN_MODE_CD"] = "R";
            row46["PROCID"] = "";
            row46["PROCNAME"] = "";
            row46["ISS_RSV_FLAG"] = "";
            row46["WIPSTAT"] = "";
            row46["NEXT_PROCID"] = "";
            row46["FORMSTATUS"] = "11";
            row46["EQP_MAINT_DISPLAY"] = "N";
            row46["USE_YN"] = "Y";
            row46["PROD_LOTID"] = "";
            row46["OP_START_TIME"] = "";
            row46["OP_OVER_TIME"] = "";
            row46["TEMP_VAL"] = "520";
            row46["PRESS_VAL"] = "0";
            row46["LAST_RUN_TIME"] = "2020-11-25 07:22:26";
            row46["ROW_IDX"] = "15";
            row46["COL_ID"] = "자동차 1-1호 Lane 3호";
            row46["ROW_ID"] = "HOT_11";
            dt.Rows.Add(row46);

            DataRow row47 = dt.NewRow();
            row47["EQPTID"] = "X11JF30512";
            row47["ROW"] = "03";
            row47["COL"] = "05";
            row47["STG"] = "12";
            row47["CSTID"] = "";
            row47["LOTID"] = "";
            row47["EIOSTAT"] = "W";
            row47["PORT_STAT_CODE"] = "WAIT";
            row47["RUN_MODE_CD"] = "R";
            row47["PROCID"] = "";
            row47["PROCNAME"] = "";
            row47["ISS_RSV_FLAG"] = "";
            row47["WIPSTAT"] = "";
            row47["NEXT_PROCID"] = "";
            row47["FORMSTATUS"] = "11";
            row47["EQP_MAINT_DISPLAY"] = "N";
            row47["USE_YN"] = "Y";
            row47["PROD_LOTID"] = "";
            row47["OP_START_TIME"] = "";
            row47["OP_OVER_TIME"] = "";
            row47["TEMP_VAL"] = "520";
            row47["PRESS_VAL"] = "0";
            row47["LAST_RUN_TIME"] = "2020-11-25 07:22:27";
            row47["ROW_IDX"] = "16";
            row47["COL_ID"] = "자동차 1-1호 Lane 3호";
            row47["ROW_ID"] = "HOT_12";
            dt.Rows.Add(row47);

            DataRow row48 = dt.NewRow();
            row48["EQPTID"] = "X11JF30513";
            row48["ROW"] = "03";
            row48["COL"] = "05";
            row48["STG"] = "13";
            row48["CSTID"] = "";
            row48["LOTID"] = "";
            row48["EIOSTAT"] = "W";
            row48["PORT_STAT_CODE"] = "WAIT";
            row48["RUN_MODE_CD"] = "R";
            row48["PROCID"] = "";
            row48["PROCNAME"] = "";
            row48["ISS_RSV_FLAG"] = "";
            row48["WIPSTAT"] = "";
            row48["NEXT_PROCID"] = "";
            row48["FORMSTATUS"] = "11";
            row48["EQP_MAINT_DISPLAY"] = "N";
            row48["USE_YN"] = "Y";
            row48["PROD_LOTID"] = "";
            row48["OP_START_TIME"] = "";
            row48["OP_OVER_TIME"] = "";
            row48["TEMP_VAL"] = "520";
            row48["PRESS_VAL"] = "0";
            row48["LAST_RUN_TIME"] = "2020-11-25 07:22:28";
            row48["ROW_IDX"] = "17";
            row48["COL_ID"] = "자동차 1-1호 Lane 3호";
            row48["ROW_ID"] = "HOT_13";
            dt.Rows.Add(row48); 
            #endregion
        }

        private void GetTestData2(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            dt.Columns.Add("LANE_ID", typeof(string));
            dt.Columns.Add("MAIN_EQP", typeof(string));
            dt.Columns.Add("EQP", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("TO_ROW", typeof(string));
            dt.Columns.Add("TO_CEL", typeof(string));
            dt.Columns.Add("TO_STG", typeof(string));
            dt.Columns.Add("OP_STATUS", typeof(string));
            dt.Columns.Add("COMM_STATUS", typeof(string));
            dt.Columns.Add("EQP_STATUS", typeof(string));
            dt.Columns.Add("MANUAL", typeof(string));

            #region TEST Data
            DataRow row1 = dt.NewRow();
            row1["LANE_ID"] = "X11";
            row1["MAIN_EQP"] = "X11JT10101";
            row1["EQP"] = "X11JT10201";
            row1["NAME"] = "REAL INPUT";
            row1["TO_ROW"] = "00";
            row1["TO_CEL"] = "00";
            row1["TO_STG"] = "11";
            row1["OP_STATUS"] = "G(NG)";
            row1["COMM_STATUS"] = "Y(OK)";
            row1["EQP_STATUS"] = "I(OK)";
            row1["MANUAL"] = "C(OK)";
            dt.Rows.Add(row1);

            DataRow row2 = dt.NewRow();
            row2["LANE_ID"] = "X11";
            row2["MAIN_EQP"] = "X11JT10101";
            row2["EQP"] = "X11JT10202";
            row2["NAME"] = "EMPTY OUTPUT";
            row2["TO_ROW"] = "00";
            row2["TO_CEL"] = "00";
            row2["TO_STG"] = "12";
            row2["OP_STATUS"] = "G(NG)";
            row2["COMM_STATUS"] = "Y(OK)";
            row2["EQP_STATUS"] = "I(OK)";
            row2["MANUAL"] = "C(OK)";
            dt.Rows.Add(row2);

            DataRow row3 = dt.NewRow();
            row3["LANE_ID"] = "X11";
            row3["MAIN_EQP"] = "X11JT10101";
            row3["EQP"] = "X11JT10203";
            row3["NAME"] = "EMPTY INPUT";
            row3["TO_ROW"] = "00";
            row3["TO_CEL"] = "00";
            row3["TO_STG"] = "13";
            row3["OP_STATUS"] = "G(NG)";
            row3["COMM_STATUS"] = "Y(OK)";
            row3["EQP_STATUS"] = "I(OK)";
            row3["MANUAL"] = "C(OK)";
            dt.Rows.Add(row3);

            DataRow row4 = dt.NewRow();
            row4["LANE_ID"] = "X11";
            row4["MAIN_EQP"] = "X11JT10101";
            row4["EQP"] = "X11JT10204";
            row4["NAME"] = "REAL OUTPUT";
            row4["TO_ROW"] = "00";
            row4["TO_CEL"] = "00";
            row4["TO_STG"] = "14";
            row4["OP_STATUS"] = "G(NG)";
            row4["COMM_STATUS"] = "Y(OK)";
            row4["EQP_STATUS"] = "I(OK)";
            row4["MANUAL"] = "C(OK)";
            dt.Rows.Add(row4);

            DataRow row5 = dt.NewRow();
            row5["LANE_ID"] = "X11";
            row5["MAIN_EQP"] = "X11JT20101";
            row5["EQP"] = "X11JT20201";
            row5["NAME"] = "REAL INPUT";
            row5["TO_ROW"] = "00";
            row5["TO_CEL"] = "00";
            row5["TO_STG"] = "21";
            row5["OP_STATUS"] = "G(NG)";
            row5["COMM_STATUS"] = "Y(OK)";
            row5["EQP_STATUS"] = "I(OK)";
            row5["MANUAL"] = "C(OK)";
            dt.Rows.Add(row5);

            DataRow row6 = dt.NewRow();
            row6["LANE_ID"] = "X11";
            row6["MAIN_EQP"] = "X11JT20101";
            row6["EQP"] = "X11JT20202";
            row6["NAME"] = "EMPTY OUTPUT";
            row6["TO_ROW"] = "00";
            row6["TO_CEL"] = "00";
            row6["TO_STG"] = "22";
            row6["OP_STATUS"] = "G(NG)";
            row6["COMM_STATUS"] = "Y(OK)";
            row6["EQP_STATUS"] = "I(OK)";
            row6["MANUAL"] = "C(OK)";
            dt.Rows.Add(row6);

            DataRow row7 = dt.NewRow();
            row7["LANE_ID"] = "X11";
            row7["MAIN_EQP"] = "X11JT20101";
            row7["EQP"] = "X11JT20203";
            row7["NAME"] = "EMPTY INPUT";
            row7["TO_ROW"] = "00";
            row7["TO_CEL"] = "00";
            row7["TO_STG"] = "23";
            row7["OP_STATUS"] = "G(NG)";
            row7["COMM_STATUS"] = "Y(OK)";
            row7["EQP_STATUS"] = "I(OK)";
            row7["MANUAL"] = "C(OK)";
            dt.Rows.Add(row7);

            DataRow row8 = dt.NewRow();
            row8["LANE_ID"] = "X11";
            row8["MAIN_EQP"] = "X11JT20101";
            row8["EQP"] = "X11JT20204";
            row8["NAME"] = "REAL OUTPUT";
            row8["TO_ROW"] = "00";
            row8["TO_CEL"] = "00";
            row8["TO_STG"] = "24";
            row8["OP_STATUS"] = "G(NG)";
            row8["COMM_STATUS"] = "Y(OK)";
            row8["EQP_STATUS"] = "I(OK)";
            row8["MANUAL"] = "C(OK)";
            dt.Rows.Add(row8);

            DataRow row9 = dt.NewRow();
            row9["LANE_ID"] = "X11";
            row9["MAIN_EQP"] = "X11JT30101";
            row9["EQP"] = "X11JT30201";
            row9["NAME"] = "REAL INPUT";
            row9["TO_ROW"] = "00";
            row9["TO_CEL"] = "00";
            row9["TO_STG"] = "31";
            row9["OP_STATUS"] = "L(OK)";
            row9["COMM_STATUS"] = "Y(OK)";
            row9["EQP_STATUS"] = "I(OK)";
            row9["MANUAL"] = "C(OK)";
            dt.Rows.Add(row9);

            DataRow row10 = dt.NewRow();
            row10["LANE_ID"] = "X11";
            row10["MAIN_EQP"] = "X11JT30101";
            row10["EQP"] = "X11JT30202";
            row10["NAME"] = "EMPTY OUTPUT";
            row10["TO_ROW"] = "00";
            row10["TO_CEL"] = "00";
            row10["TO_STG"] = "32";
            row10["OP_STATUS"] = "G(NG)";
            row10["COMM_STATUS"] = "Y(OK)";
            row10["EQP_STATUS"] = "I(OK)";
            row10["MANUAL"] = "C(OK)";
            dt.Rows.Add(row10);

            DataRow row11 = dt.NewRow();
            row11["LANE_ID"] = "X11";
            row11["MAIN_EQP"] = "X11JT30101";
            row11["EQP"] = "X11JT30203";
            row11["NAME"] = "EMPTY INPUT";
            row11["TO_ROW"] = "00";
            row11["TO_CEL"] = "00";
            row11["TO_STG"] = "33";
            row11["OP_STATUS"] = "G(NG)";
            row11["COMM_STATUS"] = "Y(OK)";
            row11["EQP_STATUS"] = "I(OK)";
            row11["MANUAL"] = "C(OK)";
            dt.Rows.Add(row11);

            DataRow row12 = dt.NewRow();
            row12["LANE_ID"] = "X11";
            row12["MAIN_EQP"] = "X11JT30101";
            row12["EQP"] = "X11JT30204";
            row12["NAME"] = "REAL OUTPUT";
            row12["TO_ROW"] = "00";
            row12["TO_CEL"] = "00";
            row12["TO_STG"] = "34";
            row12["OP_STATUS"] = "G(NG)";
            row12["COMM_STATUS"] = "Y(OK)";
            row12["EQP_STATUS"] = "I(OK)";
            row12["MANUAL"] = "C(OK)";
            dt.Rows.Add(row12);

            #endregion
        }
        #endregion
    }
}
