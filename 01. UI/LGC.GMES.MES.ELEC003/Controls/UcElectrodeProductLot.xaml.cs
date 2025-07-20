/*************************************************************************************
 Created Date : 2020.09.14
      Creator : 정문교
   Decription : 전극 공정진척 - 생산 LOT List 
--------------------------------------------------------------------------------------
 [Change History]
 2020.09.09  정문교 : Initial Created.
 2021.03.31  정문교 : 대기 LOT 조회시 HOLD는 빨간색 표시
                      자동 조회시 대기 체크인 경우 대기 리스트 조회
 2021.07.01  조영대 : Slitting, 절연 Coter, 표면검사, Taping, Heat Treatment 추가
 2021.10.15  김지은 : 롤프레스 대기 무지부방향 추가
 2021.12.15  강동희 : 2차 Slitting 공정진척DRB 화면 개발
 2023.07.26  김태우 : NFF DAM믹서 추가
 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경
 2024.02.20  정재홍 : [E20240130-000387] - Slitting 1st sample roll color
 2024.06.28  김도형 : [E20240426-001921] [ESWA PI] Re-request of CSR E20240125-001691 Electrode Process Progress-Mixer Column Changes(설비명표시)
 2024.11.29  이동주 : E20240904-000991 [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건
 2025.04.25  이민형 : [HD_OSS_0285] 롤프레스 대기 Lot 조회 그리드 컬럼 추가
 2025.06.25  이민형 : [MI2_OSS_0373] Slitting 조회 시 에러 수정
 ***************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using C1.WPF;
using System.Reflection;
using System.Linq;

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeProductLot.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeProductLot : UserControl
    {
        #region Declaration
        public event CommandButtonClickEventHandler CommandButtonClick;
        public delegate void CommandButtonClickEventHandler(object sender, string buttonName);

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public DataTable DtEquipment { get; set; }

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string CoatSide { get; set; }
        public string ReWindingProcess { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public string UnldrLotIdentBasCode { get; set; }

        public C1DataGrid DgProductLot { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;
        private string _clickLotID = string.Empty;
        DataTable _dProductLotColumnVisible;
        DataTable _dwipColorLegnd;
        DataTable _dProductLot;

        List<string> buttonPermissions = null;

        public UcElectrodeProductLot()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            SetDataGridContextMenu();
            //SetControlVisibility();
            InitColumnsList();
            //SetComboBox();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            chkWoProduct.Visibility = Visibility.Collapsed;
            chkHoldInclude.Visibility = Visibility.Collapsed;
            Util.gridClear(dgProductLot);
        }

        private void SetControl()
        {
            DgProductLot = dgProductLot;
        }
        private void SetButtons()
        {
        }
        private void SetDataGridContextMenu()
        {
            for (int row = 0; row < dgProductLot.ContextMenu.Items.Count; row++)
            {
                MenuItem item = dgProductLot.ContextMenu.Items[row] as MenuItem;
                if (item == null) continue;

                switch (item.Name.ToString())
                {
                    case "cmnuLotHistory":
                        item.Tag = dgProductLot;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        item.Click += cmnuLotHistory_Click;
                        break;

                    case "cmnuLotHoldHistory":
                        item.Tag = dgProductLot;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        item.Click += cmnuLotHoldHistory_Click;
                        break;
                }
            }

        }

        private void InitColumnsList()
        {
            _dProductLotColumnVisible = new DataTable();
            _dProductLotColumnVisible.Columns.Add("COLUMN");
            _dProductLotColumnVisible.Columns.Add("PROCESS");
            _dProductLotColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dProductLotColumnVisible.Rows.Add("WIPSTAT_IMAGES", null, Process.REWINDING + ',' +                                // 상태(기호)
                                                                       Process.SLIT_REWINDING);

            _dProductLotColumnVisible.Rows.Add("LOTID_LARGE", Process.COATING, null);                                           // 대LOT
            _dProductLotColumnVisible.Rows.Add("CUT", Process.COATING, null);                                                   // CUT

            _dProductLotColumnVisible.Rows.Add("OUT_CSTID", Process.COATING + ',' +                                             // 배출CarrierID
                                                            Process.ROLL_PRESSING + ',' +
                                                            Process.REWINDING + ',' +
                                                            Process.SLIT_REWINDING, null);
            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 Start
            //_dProductLotColumnVisible.Rows.Add("PRJT_NAME", Process.MIXING + ',' +
            //                                                Process.COATING + ',' +
            //                                                Process.HALF_SLITTING + "," +
            //                                                Process.ROLL_PRESSING + "," +
            //                                                Process.TAPING + ',' +
            //                                                Process.HEAT_TREATMENT + ',' +
            //                                                Process.INS_COATING + ',' +
            //                                                Process.REWINDER + ',' +
            //                                                Process.REWINDING + ',' +
            //                                                Process.SLIT_REWINDING, null);                                      // PRJT_NAME
            //_dProductLotColumnVisible.Rows.Add("COATER_PRJT_NAME", Process.MIXING + "," +
            //                                                       Process.PRE_MIXING, null);                                   // 코터PJT
            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 End

            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 Start
            _dProductLotColumnVisible.Rows.Add("PRJT_NAME_NEW", Process.MIXING + ',' +
                                                            Process.COATING + ',' +
                                                            Process.HALF_SLITTING + "," +
                                                            Process.ROLL_PRESSING + "," +
                                                            Process.TAPING + ',' +
                                                            Process.HEAT_TREATMENT + ',' +
                                                            Process.INS_COATING + ',' +
                                                            Process.REWINDER + ',' +
                                                            Process.REWINDING + ',' +
                                                            Process.SLIT_REWINDING, null);                                      // PRJT_NAME
            _dProductLotColumnVisible.Rows.Add("COATER_PRJT_NAME", Process.PRE_MIXING, null);                                   // 코터PJT
            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 End

            _dProductLotColumnVisible.Rows.Add("COATERVER", Process.MIXING + "," +
                                                            Process.PRE_MIXING + "," +
                                                            Process.BS + "," +
                                                            Process.DAM_MIXING + "," +
                                                            Process.CMC, null);                                                 // 버전
            _dProductLotColumnVisible.Rows.Add("PROD_VER_CODE", Process.COATING + ',' +
                                                                Process.HALF_SLITTING + "," +
                                                                Process.ROLL_PRESSING + "," +
                                                                Process.TAPING + ',' +
                                                                Process.HEAT_TREATMENT + ',' +
                                                                Process.INS_COATING + ',' +
                                                                Process.REWINDER + ',' +
                                                                Process.REWINDING + ',' +
                                                                Process.SLIT_REWINDING, null);                                  // 버전
            _dProductLotColumnVisible.Rows.Add("LOTID_PR", Process.COATING + ',' +
                                                           Process.HALF_SLITTING + ',' +
                                                           Process.ROLL_PRESSING + ',' +
                                                           Process.TAPING + ',' +
                                                           Process.HEAT_TREATMENT + ',' +
                                                           Process.REWINDER + ',' +
                                                           Process.TWO_SLITTING + ',' +
                                                           Process.SLITTING + ',' +                                           // 2021.12.29. Slitting 로직 개선 - Slitting 추가
                                                           Process.INS_COATING, null);                                        // 투입 LOT //20211215 2차 Slitting 공정진척DRB 화면 개발
            _dProductLotColumnVisible.Rows.Add("CSTID", Process.ROLL_PRESSING, null);                                           // CarrierID
            _dProductLotColumnVisible.Rows.Add("INPUTQTY", Process.ROLL_PRESSING + "," +
                                                           Process.TAPING + ',' +
                                                           Process.HEAT_TREATMENT + ',' +
                                                           Process.INS_COATING + ',' +
                                                           Process.REWINDER + ',' +
                                                           Process.HALF_SLITTING, null);                                        // 투입량
            _dProductLotColumnVisible.Rows.Add("INPUT_BACK_QTY", Process.COATING, null);                                        // 투입량(BACK)
            _dProductLotColumnVisible.Rows.Add("INPUT_TOP_QTY", Process.COATING, null);                                         // 투입량(TOP)
            _dProductLotColumnVisible.Rows.Add("WIPQTY", Process.REWINDING + ',' +
                                                         Process.SLIT_REWINDING, null);                                         // 양품량(C/Roll)
            _dProductLotColumnVisible.Rows.Add("LANE_QTY", Process.REWINDING + ',' +
                                                           Process.SLIT_REWINDING, null);                                       // Lane수
            //_dProductLotColumnVisible.Rows.Add("WIPSTAT_NAME", null, Process.REWINDING + ',' +                                  // 상태
            //                                                           Process.SLIT_REWINDING);
            _dProductLotColumnVisible.Rows.Add("ROLLPRESS_SEQNO", Process.ROLL_PRESSING, null);                                 // 압연차수
            //_dProductLotColumnVisible.Rows.Add("WIPDTTM_ST", Process.MIXING + ',' +
            //                                                 Process.PRE_MIXING + ',' +
            //                                                 Process.BS + ',' +
            //                                                 Process.CMC + ',' +
            //                                                 Process.InsulationMixing + ',' +
            //                                                 Process.COATING + ',' +
            //                                                 Process.HALF_SLITTING + ',' +
            //                                                 Process.ROLL_PRESSING, null);                                      // 시작시간
            //_dProductLotColumnVisible.Rows.Add("WIPDTTM_ED", Process.MIXING + ',' +
            //                                                 Process.PRE_MIXING + ',' +
            //                                                 Process.BS + ',' +
            //                                                 Process.CMC + ',' +
            //                                                 Process.InsulationMixing + ',' +
            //                                                 Process.COATING + ',' +
            //                                                 Process.HALF_SLITTING + ',' +
            //                                                 Process.ROLL_PRESSING, null);                                      // 완료시간
            _dProductLotColumnVisible.Rows.Add("WOID", Process.MIXING + ',' +
                                                       Process.PRE_MIXING + ',' +
                                                       Process.BS + ',' +
                                                       Process.CMC + ',' +
                                                       Process.InsulationMixing + ',' +
                                                       Process.DAM_MIXING + ',' +
                                                       Process.COATING + ',' +
                                                       Process.HALF_SLITTING + ',' +
                                                       Process.ROLL_PRESSING + ',' +
                                                       Process.TAPING + ',' +
                                                       Process.HEAT_TREATMENT + ',' +
                                                       Process.REWINDER + ',' +
                                                       Process.INS_COATING, null);                                            // WO
        }

        private void SetComboBox()
        {
            GetButtonPermissionGroup();

            //// 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
            if (ProcessCode.Equals(Process.COATING) ||
                ProcessCode.Equals(Process.INS_COATING) ||
                ProcessCode.Equals(Process.ROLL_PRESSING) ||
                ProcessCode.Equals(Process.REWINDER) ||
                ProcessCode.Equals(Process.SLITTING) ||
                ProcessCode.Equals(Process.TWO_SLITTING) || //20211215 2차 Slitting 공정진척DRB 화면 개발
                ProcessCode.Equals(Process.HALF_SLITTING) ||
                ProcessCode.Equals(Process.TAPING) ||
                ProcessCode.Equals(Process.HEAT_TREATMENT) ||
                ProcessCode.Equals(Process.REWINDING) ||
                ProcessCode.Equals(Process.SLIT_REWINDING))
            {
                SetWipColorLegendCombo();
            }
        }

        public void SetControlVisibility()
        {
            chkWait.Visibility = Visibility.Collapsed;
            //chkWoProduct.Visibility = Visibility.Collapsed;
            cboColor.Visibility = Visibility.Collapsed;
            tbWorkHalfSlittingSide.Visibility = Visibility.Collapsed;
            txtWorkHalfSlittingSide.Visibility = Visibility.Collapsed;

            if (ProcessCode == Process.COATING)
            {
                cboColor.Visibility = Visibility.Visible;
            }
            //else if (ProcessCode == Process.ROLL_PRESSING ||
            //         ProcessCode == Process.TAPING ||
            //         ProcessCode == Process.HEAT_TREATMENT ||
            //         ProcessCode == Process.REWINDER ||
            //         ProcessCode == Process.SLITTING ||
            //         ProcessCode == Process.INS_COATING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            else if (ProcessCode == Process.ROLL_PRESSING ||
                     ProcessCode == Process.TAPING ||
                     ProcessCode == Process.HEAT_TREATMENT ||
                     ProcessCode == Process.REWINDER ||
                     ProcessCode == Process.SLITTING ||
                     ProcessCode == Process.TWO_SLITTING ||
                     ProcessCode == Process.INS_COATING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                chkWait.Visibility = Visibility.Visible;
                //chkWoProduct.Visibility = Visibility.Visible;
                cboColor.Visibility = Visibility.Visible;
                tbWorkHalfSlittingSide.Visibility = Visibility.Visible;
                txtWorkHalfSlittingSide.Visibility = Visibility.Visible;
            }
            else if (ProcessCode == Process.REWINDING ||
                     ProcessCode == Process.SLIT_REWINDING)
            {
                cboColor.Visibility = Visibility.Visible;
                tbWorkHalfSlittingSide.Visibility = Visibility.Visible;
                txtWorkHalfSlittingSide.Visibility = Visibility.Visible;
            }
            else if (ProcessCode == Process.HALF_SLITTING)
            {
                chkWait.Visibility = Visibility.Visible;
                tbWorkHalfSlittingSide.Visibility = Visibility.Visible;
                txtWorkHalfSlittingSide.Visibility = Visibility.Visible;
            }

            //SetGridColumnVisibility(dgProductLot, ProcessCode);
            SetGridColumnVisibilityException();
        }

        #endregion

        #region Event

        /// <summary>
        /// RollPressing 대기 LOT 조회
        /// </summary>
        public void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            SetGridColumnVisibilityException();                    

            if (chkWait.IsChecked == true)
            {
                chkHoldInclude.Checked -= chkHoldInclude_Checked;
                chkHoldInclude.Unchecked -= chkHoldInclude_Checked;

                chkWoProduct.IsChecked = true;
                chkHoldInclude.IsChecked = true;

                chkHoldInclude.Checked += chkHoldInclude_Checked;
                chkHoldInclude.Unchecked += chkHoldInclude_Checked;                
              
                GetProductListWait();
            }
            else
            {
                GetProductList(txtSelectLot.Text);
               
            }

        }

        /// <summary>
        /// W/O선택제품 체크
        /// </summary>
        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (_dProductLot == null) return;

            Util.gridClear(dgProductLot);

            if (chkWoProduct.IsChecked == true)
            {
                if (_dProductLot.Rows.Count == 0) return;

                if (DtEquipment == null) return;

                // 설비 리스트에 있는 WO전부
                //DataRow[] drWO = DtEquipment.Select("EQPTID = '" + txtSelectEquipment.Text + "' And SEQ = 11");

                //if (drWO.Length > 0)
                //{
                //    DataRow[] dr = _dProductLot.Select("PRODID = '" + drWO[0]["VAL007"].ToString() + "'"); 
                //    if (dr.Length == 0)
                //        dgProductLot.ItemsSource = null;
                //    else 
                //        Util.GridSetData(dgProductLot, dr.CopyToDataTable(), null, true);
                //}

                DataTable dtDistinct = DtEquipment.DefaultView.ToTable(true, "VAL007");
                // WO 제품이 없는 경우 삭제
                dtDistinct.Select("VAL007 = '' or VAL007 is null").ToList<DataRow>().ForEach(row => row.Delete());

                DataTable dt = _dProductLot.Clone();

                foreach (DataRow dr in dtDistinct.Rows)
                {
                    DataRow[] drIns = _dProductLot.Select("PRODID = '" + dr["VAL007"].ToString() + "'");

                    for (int row = 0; row < drIns.Length; row++)
                    {
                        dt.ImportRow(drIns[row]);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    dt.AcceptChanges();
                    Util.GridSetData(dgProductLot, dt, null, true);
                }
                else
                {
                    dgProductLot.ItemsSource = null;
                }
            }
            else
            {
                if (_dProductLot == null || _dProductLot.Rows.Count == 0) return;

                Util.GridSetData(dgProductLot, _dProductLot, null, true);
            }

        }

        /// <summary>
        /// Hold 포함 체크 
        /// </summary>
        private void chkHoldInclude_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgProductLot);

            GetProductListWait();
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //string WipstatImages = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT_IMAGES"));
                    string EqptID = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID"));
                    string Lot = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID"));
                    string Wipstat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT"));
                    string WipHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD"));
                    string QmsHold = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMS_LOT_INSP_JUDG_HOLD_FLAG"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "WIPSTAT_IMAGES")
                    {
                        e.Cell.Presenter.FontSize = 16;

                        if (Wipstat == "WAIT")
                        {
                            if (WipHold == "Y")
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            else
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (WipHold == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        //else if (QmsHold == "Y")
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Orange);
                        //}
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                        }

                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (e.Cell.Column.Name.ToString() == "EQPTID" && !string.IsNullOrWhiteSpace(EqptID))
                    {
                        // 선택된 설비인 경우 
                        if (EqptID == txtSelectEquipment.Text)
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (e.Cell.Column.Name.ToString() == "LOTID_LARGE")
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    //else if (ProcessCode.Equals(ElectrodeProcesses.SLITTING) && e.Cell.Column.Name.ToString() == "CUT_ID") //20211215 2차 Slitting 공정진척DRB 화면 개발
                    else if ((ProcessCode.Equals(ElectrodeProcesses.SLITTING) || ProcessCode.Equals(ElectrodeProcesses.TWO_SLITTING)) && e.Cell.Column.Name.ToString() == "CUT_ID") //20211215 2차 Slitting 공정진척DRB 화면 개발
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;

                        if (_load)
                        {
                            _load = false;
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(e.Cell.Column.ActualWidth + 1);
                        }
                    }
                    //else if (!ProcessCode.Equals(ElectrodeProcesses.SLITTING) && e.Cell.Column.Name.ToString() == "LOTID") //20211215 2차 Slitting 공정진척DRB 화면 개발
                    else if ((!ProcessCode.Equals(ElectrodeProcesses.SLITTING) && !ProcessCode.Equals(ElectrodeProcesses.TWO_SLITTING)) && e.Cell.Column.Name.ToString() == "LOTID") //20211215 2차 Slitting 공정진척DRB 화면 개발
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;

                        if (_load)
                        {
                            _load = false;
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(e.Cell.Column.ActualWidth + 1);
                        }
                    }
                    //else if (e.Cell.Column.Name.ToString() == "WIPQTY" && ReWindingProcess == "Y")
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //    e.Cell.Presenter.Cursor = Cursors.Hand;
                    //}
                    else if (e.Cell.Column.Name.ToString() == "WIPDTTM_ST")
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    else if (e.Cell.Column.Name.ToString() == "WIPDTTM_ED")
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    else if (e.Cell.Column.Name.ToString() == "WOID")
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }

                    // 설비완공, 완공 배경색 주석처리
                    //if (Wipstat == "EQPT_END" || Wipstat == "END")
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFBEF0F8"));
                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    //}

                    // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완 
                    if (_dwipColorLegnd == null)
                        return;

                    ///////////////////////////////////////////////////////////////////////////////////////////

                    // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                    SolidColorBrush scbZVersionBack = new SolidColorBrush();
                    SolidColorBrush scbZVersionFore = new SolidColorBrush();

                    SolidColorBrush scbCutBack = new SolidColorBrush();
                    SolidColorBrush scbCutFore = new SolidColorBrush();

                    foreach (DataRow dr in _dwipColorLegnd.Rows)
                    {
                        if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                        {
                            continue;
                        }

                        if (dr["CODE"].ToString().Equals("Z_VER"))
                        {
                            scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                            scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                        }
                        else if (dr["CODE"].ToString().Equals("CUT"))
                        {
                            scbCutBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                            scbCutFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                        }
                    }

                    if (ProcessCode == Process.COATING)
                    {
                        if ((string.Equals(e.Cell.Column.Name, "LOTID") && string.Equals(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CUT"), "1"))
                        || (string.Equals(e.Cell.Column.Name, "LOTID") && string.Equals(GetCutLotSampleQAFalg(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID")), Process.COATING), "Y"))) //[E20230228-000007] CSR - Marking logic in GMES
                        {
                            e.Cell.Presenter.Background = scbCutBack;
                            e.Cell.Presenter.Foreground = scbCutFore;
                        }
                        else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                        {
                            e.Cell.Presenter.Background = scbZVersionBack;
                            e.Cell.Presenter.Foreground = scbZVersionFore;
                        }
                        //else
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        //}
                    }
                    //else if (ProcessCode == Process.ROLL_PRESSING ||
                    //         ProcessCode == Process.TAPING ||
                    //         ProcessCode == Process.HEAT_TREATMENT ||
                    //         ProcessCode == Process.REWINDER ||
                    //         ProcessCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                    else if (ProcessCode == Process.ROLL_PRESSING ||
                             ProcessCode == Process.TAPING ||
                             ProcessCode == Process.HEAT_TREATMENT ||
                             ProcessCode == Process.REWINDER ||
                             //ProcessCode == Process.SLITTING ||
                             ProcessCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                    {
                        if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).IsNullOrEmpty() &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = scbCutBack;
                            e.Cell.Presenter.Foreground = scbCutFore;
                        }
                        else if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                            ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                        {
                            e.Cell.Presenter.Background = scbZVersionBack;
                            e.Cell.Presenter.Foreground = scbZVersionFore;
                        }
                        else if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                            ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                        {
                            e.Cell.Presenter.Background = scbZVersionBack;
                            e.Cell.Presenter.Foreground = scbZVersionFore;
                        }
                        //else
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        //}
                    }
                    // CSR : E20240130-000387
                    else if (ProcessCode == Process.SLITTING)
                    {
                        // 20201221 슬리터 샘플링 컷 범례 표시

                        if (e.Cell.Column.Name.Equals("CUT_ID") 
                        && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["SLIT_QA_INSP_TRGT_FLAG"] != null
                        && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SLIT_QA_INSP_TRGT_FLAG")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = scbCutBack;
                            e.Cell.Presenter.Foreground = scbCutFore;
                        }
                        else if (e.Cell.Column.Name.Equals("LOTID_PR") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                    ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                        {
                            e.Cell.Presenter.Background = scbZVersionBack;
                            e.Cell.Presenter.Foreground = scbZVersionFore;
                        }
                        else if (e.Cell.Column.Name.Equals("CUT_ID") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals(Wip_State.WAIT) &&
                                    ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                        {
                            e.Cell.Presenter.Background = scbZVersionBack;
                            e.Cell.Presenter.Foreground = scbZVersionFore;
                        }
                    }

                }
            }));

        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                //C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)(((System.Windows.FrameworkElement)sender).Parent)).DataGrid as C1DataGrid;
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                SetUserControlProductLotSelect(rb);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductLot.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 Row 위치
            int rowIdx = cell.Row.Index;
            DataRowView dv = DgProductLot.Rows[rowIdx].DataItem as DataRowView;

            if (dv == null) return;

            if (string.IsNullOrWhiteSpace(dv["LOTID"].ToString()))
                _clickLotID = dv["LOTID_PR"].ToString();
            else
                _clickLotID = dv["LOTID"].ToString();

            if (dv["WIPHOLD"].ToString() == "Y")
            {
                DataGridContextMenuItemEnabled("cmnuLotHoldHistory", true);
            }
            else
            {
                DataGridContextMenuItemEnabled("cmnuLotHoldHistory", false);
            }

        }

        private void cmnuLotHistory_Click(object sender, RoutedEventArgs e)
        {
            PopupLotChangeHistory(_clickLotID);
        }

        private void cmnuLotHoldHistory_Click(object sender, RoutedEventArgs e)
        {
            PopupLotHoldHistory(_clickLotID);
        }

        #endregion

        #region Mehod

        #region [외부 호출]
        public void SetApplyPermissions()
        {
            // 추가작성 필요~~~~~~~~~
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductList(string LotID, string ReWindingProcess)
        {
            SetComboBox();
                      
            if (ProcessCode == Process.MIXING ||
                ProcessCode == Process.PRE_MIXING ||
                ProcessCode == Process.BS ||
                ProcessCode == Process.CMC ||
                ProcessCode == Process.InsulationMixing ||
                ProcessCode == Process.DAM_MIXING)
            {
                // Mixing, 선분산 Mixing, Binder Solution, CMC Solution
                GetProductListMix(LotID);
            }
            else if (ProcessCode == Process.COATING ||
                     ProcessCode == Process.HALF_SLITTING)
            {
                GetProductList(LotID);
            }
            //else if (ProcessCode == Process.ROLL_PRESSING ||
            //         ProcessCode == Process.TAPING ||
            //         ProcessCode == Process.HEAT_TREATMENT ||
            //         ProcessCode == Process.REWINDER ||
            //         ProcessCode == Process.INS_COATING ||
            //         ProcessCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            else if (ProcessCode == Process.ROLL_PRESSING ||
                     ProcessCode == Process.TAPING ||
                     ProcessCode == Process.HEAT_TREATMENT ||
                     ProcessCode == Process.REWINDER ||
                     ProcessCode == Process.INS_COATING ||
                     ProcessCode == Process.SLITTING ||
                     ProcessCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                if (chkWait.IsChecked.Equals(true))
                    GetProductListWait();
                else
                    GetProductList(LotID);
            }
            else if (ReWindingProcess == "Y")
            {
                GetProductListReWinding(LotID);
            }
        }

        #endregion

        #region [BizCall]

        private void SetWipColorLegendCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            newRow["PROCID"] = ProcessCode;

            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            _dwipColorLegnd = dtResult;
        }

        private void GetProductListMix(string LotID = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_MX_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, bizResult, null, true);

                        // 라디오 버튼 체크
                        SetGridSelectRow(LotID);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void GetProductList(string LotID = null)
        {
            try
            {
                _load = true;
                Util.gridClear(DgProductLot);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID_LARGE", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("COATSIDE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["COATSIDE"] = CoatSide;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_ELEC_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        chkWoProduct.Visibility = Visibility.Collapsed;
                        chkHoldInclude.Visibility = Visibility.Collapsed;

                        Util.GridSetData(dgProductLot, bizResult, null, true);

                        // 라디오 버튼 체크
                        SetGridSelectRow(LotID);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetProductListWait()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                //newRow["EQPTID"] = EquipmentCode;
                newRow["WIPSTAT"] = chkWait.Tag.ToString();
                newRow["WIPHOLD"] = (bool)chkHoldInclude.IsChecked ? null : "N";
                inTable.Rows.Add(newRow);
                                
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_ELEC_WAIT_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _dProductLot = bizResult.Copy();

                        Util.GridSetData(dgProductLot, bizResult, null, true);

                        chkWoProduct.Visibility = Visibility.Visible;
                        chkHoldInclude.Visibility = Visibility.Visible;

                        ////////////////////////////////////////////////////////
                        ////if (!string.IsNullOrWhiteSpace(txtSelectEquipment.Text))
                        OnCheckBoxChecked(null, null);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetProductListReWinding(string LotID = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["WIPSTAT"] = Wip_State.PROC + "," + Wip_State.EQPT_END;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_GET_REWINDER_LOT_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, bizResult, null, true);

                        chkWoProduct.Visibility = Visibility.Collapsed;
                        chkHoldInclude.Visibility = Visibility.Collapsed;

                        // 라디오 버튼 체크
                        SetGridSelectRow(LotID);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        // [E20230228-000007] CSR - Marking logic in GMES
        private string GetCutLotSampleQAFalg(string sLotID, string sProcID)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SAMPLE_TARGET_CT", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["SAMPLE_FLAG"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }

        #endregion

        #region [Func]
        private void DataGridContextMenuItemEnabled(string ItemName, bool isEnabled)
        {
            for (int row = 0; row < dgProductLot.ContextMenu.Items.Count; row++)
            {
                MenuItem item = dgProductLot.ContextMenu.Items[row] as MenuItem;

                if (item.Name.ToString() == ItemName)
                {
                    item.IsEnabled = isEnabled;
                    break;
                }
            }
        }

        private void SetGridColumnVisibility(C1DataGrid dg, string ProcID)
        {
            dg.UpdateLayout();

            foreach (DataRow dr in _dProductLotColumnVisible.Rows)
            {
                string[] sProcess = dr["PROCESS"].ToString().Split(',');
                string[] sExceptionProcess = dr["EXCEPTION_PROCESS"].ToString().Split(',');
                int nExceptionindex;
                int nindex = -1;

                // 예외 공정 검색 
                nExceptionindex = Array.IndexOf(sExceptionProcess, ProcID);
                if (nExceptionindex >= 0)
                {
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // 대상공정 여부 체크
                    nindex = Array.IndexOf(sProcess, ProcID);

                    // 공정별 칼럼 Visibility
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        // 공정에 NULL인 경우는 예외공정을 제외한 공정은 다 보여준다.
                        if (string.IsNullOrWhiteSpace(sProcess[0]) || nindex >= 0)
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Visible;
                        else
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SetGridColumnVisibilityException()
        {
            SetGridColumnVisibility(dgProductLot, ProcessCode);
            dgProductLot.Columns["PRODUCTION_TYPE_NAME"].Visibility = Visibility.Collapsed;
            // [E20240426-001921] [ESWA PI] Re-request of CSR E20240125-001691 Electrode Process Progress-Mixer Column Changes
            if (ProcessCode == Process.MIXING)
            {
                DgProductLot.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            }
            else
            {
                DgProductLot.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
            }

            if (chkWait.IsChecked == true)
            {
                //DgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
                DgProductLot.Columns["WIPDTTM_ST"].Visibility = Visibility.Collapsed;
                DgProductLot.Columns["WIPDTTM_ED"].Visibility = Visibility.Collapsed;
                DgProductLot.Columns["WOID"].Visibility = Visibility.Collapsed;
              
                if (ProcessCode == Process.ROLL_PRESSING || ProcessCode == Process.SLITTING)
                {
                    DgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Visible;
                    DgProductLot.Columns["RACK_EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    DgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Collapsed;
                    DgProductLot.Columns["RACK_EQPTNAME"].Visibility = Visibility.Collapsed;
                }

                if (ProcessCode == Process.ROLL_PRESSING)
                {
                    dgProductLot.Columns["PRODUCTION_TYPE_NAME"].Visibility = Visibility.Visible;
                }              
            }
            else
            {
                DgProductLot.Columns["SLIT_SIDE_WINDING_DIRCTN"].Visibility = Visibility.Collapsed;
                DgProductLot.Columns["RACK_EQPTNAME"].Visibility = Visibility.Collapsed;
                switch (LdrLotIdentBasCode)
                {
                    case "CST_ID":
                    case "RF_ID":
                        DgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                        break;
                    default:
                        DgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                        break;
                }

                switch (UnldrLotIdentBasCode)
                {
                    case "CST_ID":
                    case "RF_ID":
                        DgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                        break;
                    default:
                        DgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;
                        break;
                }
            }

            // 칼럼 명칭 변경
            if (ReWindingProcess == "Y")
            {
                //DgProductLot.Columns["WIPQTY"].Header = ObjectDic.Instance.GetObjectName("수량");
                DgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }
            else
            {
                //DgProductLot.Columns["WIPQTY"].Header = ObjectDic.Instance.GetObjectName("양품량(C/Roll)");
            }

            // Shop 별  컬럼 설정
            SetGridColumnVisibilityByProcess();
        }

        private void SetGridColumnVisibilityByProcess()
        {
            switch (ProcessCode)
            {
                //case ElectrodeProcesses.SLITTING:
                //DgProductLot.Columns["ROLLPRESS_SEQNO"].Visibility = Visibility.Collapsed;
                //DgProductLot.Columns["CUT_ID"].Visibility = Visibility.Visible;
                //DgProductLot.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("투입 LOT");
                //break;
                //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                case ElectrodeProcesses.SLITTING: // 2021.12.29. Slitting 로직 개선 - 2차 Slitting과 조건 동일하게
                case ElectrodeProcesses.TWO_SLITTING:
                    DgProductLot.Columns["ROLLPRESS_SEQNO"].Visibility = Visibility.Collapsed;
                    DgProductLot.Columns["CUT_ID"].Visibility = Visibility.Visible;
                    DgProductLot.Columns["LOTID"].Visibility = Visibility.Hidden;
                    break;
                //20211215 2차 Slitting 공정진척DRB 화면 개발 END
                default:
                    DgProductLot.Columns["ROLLPRESS_SEQNO"].Visibility = Visibility.Collapsed;
                    DgProductLot.Columns["CUT_ID"].Visibility = Visibility.Collapsed;
                    DgProductLot.Columns["LOTID"].Visibility = Visibility.Visible; // 2021.12.29. Slitting 로직 개선 - Slitting, 2차 Slitting 이 아닌 경우 LOTID를 보여주도록 수정
                    //DgProductLot.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("LOTID");
                    break;
            }
        }

        protected virtual void SetUserControlProductLotSelect(RadioButton rb)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetProductLotSelect");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = rb;

                methodInfo.Invoke(UcParentControl, parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridSelectRow(string LotID)
        {
            //////////////////////////////////////////////////// 라디오 버튼 체크
            if (!string.IsNullOrWhiteSpace(LotID))
            {
                int idx = -1;
                for (int row = 0; row < dgProductLot.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID")) == LotID)
                    {
                        idx = row;
                        break;
                    }
                }

                if (idx < 0)
                {
                    txtSelectLot.Text = string.Empty;
                    return;
                }

                dgProductLot.GetCell(idx, dgProductLot.Columns["CHK"].Index);
                RadioButton rb = dgProductLot.GetCell(idx, dgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

                if (rb?.DataContext == null) return;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }
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

        #endregion;

        #region 팝업
        private void PopupLotChangeHistory(string sLotID)
        {
            CMM_LOT_CHANGE_HISTORY_DRB popupLotChangeHistory = new CMM_LOT_CHANGE_HISTORY_DRB();
            popupLotChangeHistory.FrameOperation = FrameOperation;

            if (popupLotChangeHistory != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sLotID;

                C1WindowExtension.SetParameters(popupLotChangeHistory, Parameters);

                popupLotChangeHistory.Closed += new EventHandler(PopupLotChangeHistory_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupLotChangeHistory.ShowModal()));
                popupLotChangeHistory.CenterOnScreen();
            }
        }

        private void PopupLotChangeHistory_Closed(object sender, EventArgs e)
        {
            CMM_LOT_CHANGE_HISTORY_DRB popup = sender as CMM_LOT_CHANGE_HISTORY_DRB;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void PopupLotHoldHistory(string sLotID)
        {
            CMM_LOT_HOLD_HISTORY_DRB popupLotHoldHistory = new CMM_LOT_HOLD_HISTORY_DRB();
            popupLotHoldHistory.FrameOperation = FrameOperation;

            if (popupLotHoldHistory != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sLotID;

                C1WindowExtension.SetParameters(popupLotHoldHistory, Parameters);

                popupLotHoldHistory.Closed += new EventHandler(PopupLotHoldHistory_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupLotHoldHistory.ShowModal()));
                popupLotHoldHistory.CenterOnScreen();
            }
        }

        private void PopupLotHoldHistory_Closed(object sender, EventArgs e)
        {
            CMM_LOT_HOLD_HISTORY_DRB popup = sender as CMM_LOT_HOLD_HISTORY_DRB;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        #endregion

        #endregion

        private void GetButtonPermissionGroup()
        {
            try
            {
                if (buttonPermissions == null) buttonPermissions = new List<string>();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        buttonPermissions.Clear();
                        foreach (DataRow dr in dtRslt.Rows)
                        {
                            buttonPermissions.Add(Util.NVC(dr["BTN_PMS_GRP_CODE"]));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_LoadedRowDetailsPresenter(object sender, C1.WPF.DataGrid.DataGridRowDetailsEventArgs e)
        {
            if (e.Row.DetailsVisibility == Visibility.Visible)
            {
                var actionCommand = e.DetailsElement as UcActionCommand;

                if (actionCommand != null)
                {
                    actionCommand.FrameOperation = FrameOperation;

                    e.Row.DetailsPresenter.HorizontalGridLineBrush = Brushes.LightGray;
                    e.Row.DetailsPresenter.HorizontalGridLineVisibility = Visibility.Visible;

                    SetCommandButton(actionCommand);
                }
            }
        }

        private void SetCommandButton(UcActionCommand actionCommand)
        {
            if (buttonPermissions == null) buttonPermissions = new List<string>();

            if (ProcessCode == Process.MIXING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelFCut", "LOT종료취소");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnInvoiceMaterial", "투입요청서");
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnMixConfirm", "자주검사등록");
                actionCommand.AddButton("btnSlurryConf", "Slurry 물성정보");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ProcessCode == Process.PRE_MIXING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelFCut", "LOT종료취소");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnInvoiceMaterial", "투입요청서");
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnMixConfirm", "자주검사등록");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing || ProcessCode == Process.DAM_MIXING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnMixConfirm", "자주검사등록");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ProcessCode == Process.COATING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCleanLot", "대LOT삭제");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnMixConfirm", "자주검사등록");
                actionCommand.AddButton("btnMixerTankInfo", "Slurry정보");
                actionCommand.AddButton("btnReservation", "W/O 예약");
                actionCommand.AddButton("btnStartCoaterCut", "코터 임의 Cut 생성");
                actionCommand.AddButton("btnLogisStat", "물류반송현황");
                actionCommand.AddButton("btnScrapLot", "Scrap Lot 재생성");
                actionCommand.AddButton("btnWebBreak", "단선추가");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
                actionCommand.AddButton("btnSlurryManualOutput", "슬러리수동배출");    //nathan 2023.12.20 믹서 코터 배치연계 
                actionCommand.AddButton("btnSlurryManualInput", "슬러리수동재투입");   //nathan 2023.12.20 믹서 코터 배치연계 

            }
            else if (ProcessCode == Process.HALF_SLITTING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnCut", "Cut");
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
                actionCommand.AddButton("btnMove", "이동");
                actionCommand.AddButton("btnMoveCancel", "이동취소");
            }
            //else if (ProcessCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            else if (ProcessCode == Process.SLITTING || ProcessCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ProcessCode == Process.ROLL_PRESSING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnCut", "Cut");
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnProcReturn", "R/P 대기 변경");
                actionCommand.AddButton("btnWorkHalfSlitSide", "무지부 방향설정");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ProcessCode == Process.TAPING)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
            }
            else if (ProcessCode == Process.HEAT_TREATMENT)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
            }
            else if (ProcessCode == Process.REWINDER)
            {
                actionCommand.AddButton("btnEqptIssue", "설비특이사항");
                actionCommand.AddButton("btnCancelDelete", "CANCEL_DELETE_LOT");// 삭제Lot생성
                actionCommand.AddButton("btnEqptCond", "작업조건등록");
                actionCommand.AddButton("btnPilotProdMode", "시생산설정/해제");
            }
            else if (ReWindingProcess == "Y")
            {
                actionCommand.AddButton("btnMove", "이동");
                actionCommand.AddButton("btnMoveCancel", "이동취소");
            }

            if (buttonPermissions.Contains("LOTSTART_W")) actionCommand.AddButton("btnStart", "작업시작");
            if (buttonPermissions.Contains("CANCEL_LOTSTART_W")) actionCommand.AddButton("btnCancel", "시작취소");

            actionCommand.ButtonLoadedPresenter += ActionCommand_ButtonLoadedPresenter;
            actionCommand.ButtonClick += ActionCommand_ButtonClick;
        }

        private void ActionCommand_ButtonLoadedPresenter(object sender, Button button)
        {
        }

        private void ActionCommand_ButtonClick(object sender, string buttonName)
        {
            CommandButtonClick.Invoke(sender, buttonName);
        }

    }
}
