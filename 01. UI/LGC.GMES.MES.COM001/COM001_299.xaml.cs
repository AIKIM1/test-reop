/*************************************************************************************
 Created Date : 2019.10.15
      Creator : 정문교
   Decription : 공정별 생산 실적수정(조립) - 특이작업 (작업자 사용) 
--------------------------------------------------------------------------------------
 [Change History]
 2019.10.15   정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
 2019.11.01   정문교   1.좌측하단 자공정 LOSS (공급부) Text 박스 추가
                       2.자공정 LOSS (공급부) 저장 Parameters 추가
 2019.11.05   정문교 : 칼람 추가 : WipAttr 작업방식(재검사,재작업) 칼럼 추가
 2019.11.13   정문교 : 재투입수 칼람위치 작업조 앞으로 변경
                       미확인LOSS 산출에서 재투입 제외
 2019.11.19   정문교 : 설비불량명, 불량그룹명 좌측 정렬
 2019.11.26   정문교 : 전공정LOSS 칼럼 제거
 2019.12.05   정문교 : 작업타입을 작업유형으로 명칭 변경
 2019.12.11   정문교 : 재투입수 수정 불가 변경
 2019.12.13   정문교 : 조회 조건에 생산 구분 콤보 추가
 2020.06.16   김동일   C20200527-000121 NND 양품 수량 0 저장 안되도록 변경
 2020.07.02   김동일   C20200625-000271 기준정보를 통한 투입관련 Tab의 투입 및 투입취소버튼 제어기능 추가
 2021.06.23   이상훈   C20210623-000317 공정별 생산실적 수정(조립) 화면 조회시 다국어 처리
 2021.07.15   김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
 2022.09.01   신광희 : LOT 리스트 데이터 그리드 생산량 컬럼 수정 및 실적 확인 영역 투입 TextBlock 생산으로 변경
 2023.09.06   주동석 : 모든 경우에 다음공정 투입시 수정 못하도록 Blocking 하도록 수정
 2023.10.30   강성묵 : Add editable remarks column for dgSublot and add button to add additional remark (Muhammad Fajar)
 2024.04.08   김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
 2025.03.11   이민형 : 완성 탭의 수량 변경 방지 추가
 2025.03.14   이민형 : 완성 탭의 수량 변경 방지 추가 원복
 2025.03.18   천진수 : 완성LOT 수량변경
 2025.04.14   박성진 : [E20240704-001494] 생산실적 수정시 로직 중복 수행 방지(CatchUp)
 2025.05.16   천진수 : 2512_015(MES_IT_A_0057) 생산실적 완성LOT수정 GRID 체크박스 선택 개선
 * **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
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
    public partial class COM001_299 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private DataTable _dtCellManagement;
        private string _cellManageGroup = string.Empty;
        private System.DateTime dtCaldate;

        private const string _assembly = "ASSY";

        DataTable _dtLotListHeader;
        DataTable _dtLotListColumnVisible;
        DataTable _dtDefectColumnVisible;
        DataTable _dtSubLotColumnVisible;
        DataTable _dtEqptCondColumnVisible;

        DataRow _drSelectRow;

        bool _bLoad;
        bool _bLoadHeader = true;  // C20210623-000317 초기 화면 구성 시 체크 추가

        public COM001_299()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        #region # Combo Setting
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //ESMI-A4동 1~5Line 제외처리
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "ESMI_A4_FIRST_PRIORITY_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }

            //공정
            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //생산구분
            string[] sFilter = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
        }
        #endregion

        #region # dgLotList의 Header Text, Column Visibility
        private void InitColumnsList()
        {
            #region [ dgLotList의 칼럼 Header ]
            _dtLotListHeader = new DataTable();
            _dtLotListHeader.Columns.Add("COLUMN");
            _dtLotListHeader.Columns.Add("PROCESS");
            _dtLotListHeader.Columns.Add("TITLE");

            //_dtLotListHeader.Rows.Add("CURR_PROC_LOSS_QTY", _assembly, ObjectDic.Instance.GetObjectName("공급") + "," + ObjectDic.Instance.GetObjectName("자공정LOSS"));
            //_dtLotListHeader.Rows.Add("CURR_PROC_LOSS_QTY", Process.NOTCHING, ObjectDic.Instance.GetObjectName("공급") + "," + ObjectDic.Instance.GetObjectName("자공정LOSS(공급부)"));

            #endregion

            #region [ 불량/Loss/물품청구 칼럼 Visibility ]
            _dtDefectColumnVisible = new DataTable();
            _dtDefectColumnVisible.Columns.Add("COLUMN");
            _dtDefectColumnVisible.Columns.Add("PROCESS");
            _dtDefectColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtDefectColumnVisible.Rows.Add("EQP_DFCT_QTY", _assembly, Process.NOTCHING);                                                            // 장비불량수량
            #endregion

            #region [ dgLotList의 칼럼 Visibility ]
            _dtLotListColumnVisible = new DataTable();
            _dtLotListColumnVisible.Columns.Add("COLUMN");
            _dtLotListColumnVisible.Columns.Add("PROCESS");
            _dtLotListColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtLotListColumnVisible.Rows.Add("PR_LOTID", Process.NOTCHING + "," + Process.VD_LMN, null);                                                                       // 투입LOT
            _dtLotListColumnVisible.Rows.Add("EQPT_END_PSTN_ID", Process.NOTCHING, null);                                                                                      // REWINDER
            _dtLotListColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.VD_LMN + "," + Process.PACKAGING, null);                                   // 전공정LOSS
            _dtLotListColumnVisible.Rows.Add("FIX_LOSS_QTY", Process.NOTCHING + "," + Process.VD_LMN + "," + Process.LAMINATION, null);                                        // 고정LOSS
            _dtLotListColumnVisible.Rows.Add("CURR_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.STACKING_FOLDING, null);                        // 자공정LOSS
            //_dtLotListColumnVisible.Rows.Add("RE_INPUT_QTY", Process.PACKAGING, null);                                                                                         // 재투입
            _dtLotListColumnVisible.Rows.Add("EQPT_UNMNT_TYPE_CODE", Process.NOTCHING, null);                                                                                  // 잔량처리

            _dtLotListColumnVisible.Rows.Add("RWK_TYPE_NAME", Process.RWK_LNS, null);                                                                                          // 작업방식
            _dtLotListColumnVisible.Rows.Add("WOID", _assembly, Process.RWK_LNS);                                                                                              // W/O
            _dtLotListColumnVisible.Rows.Add("WO_DETL_ID", _assembly, Process.RWK_LNS);                                                                                        // W/O상세

            #endregion

            #region [ dgSubLot 칼럼 Visibility ]
            _dtSubLotColumnVisible = new DataTable();
            _dtSubLotColumnVisible.Columns.Add("COLUMN");
            _dtSubLotColumnVisible.Columns.Add("PROCESS");
            _dtSubLotColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtSubLotColumnVisible.Rows.Add("CSTID", _assembly, Process.NOTCHING);                                                                    // Carrier ID
            _dtSubLotColumnVisible.Rows.Add("SPECIALYN", Process.PACKAGING, null);                                                                    // 특이
            _dtSubLotColumnVisible.Rows.Add("SPECIALDESC", Process.PACKAGING, null);                                                                  // 특이사항
            _dtSubLotColumnVisible.Rows.Add("FORM_MOVE_STAT_CODE_NAME", Process.PACKAGING, null);                                                     // 상태
            _dtSubLotColumnVisible.Rows.Add("LOTID", _assembly, Process.PACKAGING);                                                                   // 완성ID
            _dtSubLotColumnVisible.Rows.Add("PRINT_YN", _assembly, Process.PACKAGING);                                                                // 발행
            _dtSubLotColumnVisible.Rows.Add("DISPATCH_YN", _assembly, Process.PACKAGING);                                                             // DISPATCH
            _dtSubLotColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", _assembly, Process.NOTCHING + "," + Process.PACKAGING);                              // 전공정 LOSS
            #endregion

            #region [ 설비작업조건 칼럼 Visibility ]
            _dtEqptCondColumnVisible = new DataTable();
            _dtEqptCondColumnVisible.Columns.Add("COLUMN");
            _dtEqptCondColumnVisible.Columns.Add("PROCESS");

            _dtSubLotColumnVisible.Rows.Add("UNIT_EQPTNAME", Process.PACKAGING);                                                                      // 설비UNIT
            #endregion

        }
        #endregion

        #region # Tab, TextBox, TextBlock, ComboBox Visibility.Collapsed
        private void InitTabControlVisible()
        {
            InitializeDataGridAttribute();

            // Tab
            cTabHalf.Visibility = Visibility.Collapsed;                        // 완성LOT
            cTabInBox.Visibility = Visibility.Collapsed;                       // 투입바구니

            // TextBlock
            tbPreSectionQty.Visibility = Visibility.Collapsed;                // 구간잔량(검사전)
            tbAfterSectionQty.Visibility = Visibility.Collapsed;              // 구간잔량(검사후)
            tbReInputQty.Visibility = Visibility.Collapsed;                   // 재투입수량
            tbCurrProcLossQty.Visibility = Visibility.Collapsed;              // 자공정 LOSS (공급부)
            tbM.Visibility = Visibility.Collapsed;


            // C1NumericBox
            txtInputQty.IsEnabled = true;
            txtPreSectionQty.Visibility = Visibility.Collapsed;
            txtAfterSectionQty.Visibility = Visibility.Collapsed;
            txtReInputQty.Visibility = Visibility.Collapsed;
            txtCurrProcLossQty.Visibility = Visibility.Collapsed;
            txtOutQty.IsEnabled = false;

            // Button
            btnOutDel.Visibility = Visibility.Collapsed;

            // 완성LOT 탭의 수량 Column
            dgSubLot.Columns["WIPQTY"].IsReadOnly = false;
        }

        private void InitializeDataGridAttribute()
        {

            dgLotList.Columns["INPUT_TAB_COUNT"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["END_TAB_COUNT"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["FIX_LOSS_QTY_VD"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["WRK_TYPE"].Visibility = Visibility.Collapsed;

            dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Visible;
            dgLotList.Columns["IRREGL_PROD_LOT_TYPE_NAME"].Visibility = Visibility.Visible;
            dgLotList.Columns["WOID"].Visibility = Visibility.Visible;
            dgLotList.Columns["WO_DETL_ID"].Visibility = Visibility.Visible;
            dgLotList.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;

            // C20210623-000317  다국어 처리
            if (_bLoadHeader == false)
            {
                dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("생산량") };
                dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("양품량") };
                dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("불량량") };
                dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("LOSS량") };
                dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("물품청구") };
            }

            tbWorkOrder.Visibility = Visibility.Visible;
            txtWorkorder.Visibility = Visibility.Visible;
        }

        #endregion

        #region # Control Clear
        private void InitUsrControl()
        {
            SetNumericBoxValueChangedEvent(false);

            txtSelectLot.Text = string.Empty;
            txtWorkorder.Text = string.Empty;
            txtWorkorderDetail.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtShift.Tag = string.Empty;
            txtStartTime.Text = string.Empty;
            txtWorker.Text = string.Empty;
            txtWorker.Tag = string.Empty;
            txtEndTime.Text = string.Empty;
            txtLotStatus.Text = string.Empty;

            txtInputQty.Value = 0;
            txtOutQty.Value = 0;
            txtPreSectionQty.Value = 0;
            txtAfterSectionQty.Value = 0;
            txtDefectQty.Value = 0;
            txtLossQty.Value = 0;
            txtPrdtReqQty.Value = 0;
            txtCurrProcLossQty.Value = 0;
            txtReInputQty.Value = 0;
            txtUnidentifiedQty.Value = 0;

            //cboIrreglProdLotType.SelectedIndex = 0;
            txtIrreglProdLotType.Text = string.Empty;
            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;

            txtNote.Text = string.Empty;
            txtReqNote.Text = string.Empty;

            tbCellManagement.Text = string.Empty;     // 완성LOT 탭

            Util.gridClear(dgDefect);
            Util.gridClear(dgSubLot);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgInputBox);
            Util.gridClear(dgEqptCond);

            _drSelectRow = null;

            SetNumericBoxValueChangedEvent(true);
        }
        #endregion

        private void SetControl()
        {
            dgDefect.AlternatingRowBackground = null;
        }

        #endregion

        #region Event

        #region # Form Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_bLoad)
            {
                // 탭 전환시 재조회
                Util.gridClear(dgLotList);
                InitUsrControl();
                //GetLotList();
            }
            else
            {
                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSave);
                listAuth.Add(btnSubLotSave);
                listAuth.Add(btnEqptCondSave);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //여기까지 사용자 권한별로 버튼 숨기기

                InitCombo();
                InitColumnsList();
                SetControl();

                SetControlVisibility(cboProcess.SelectedValue.ToString());

                GetCellManagementInfo();
                dtpCaldate.SelectedDataTimeChanged += dtpCaldate_SelectedDataTimeChanged;
            }

            _bLoad = true;
        }
        #endregion

        #region # 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetLotList();
        }
        #endregion

        #region # 조회조건 : 공정 변경
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.gridClear(dgLotList);
                InitUsrControl();
                SetControlVisibility(cboProcess.SelectedValue.ToString());
            }
        }
        #endregion

        #region # 조회조건 : LOT KeyDown
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region # 생산 Lot 선택
        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 칼럼 Width 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "WRK_USER_NAME")
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }
                }
            }));
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            //if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0) || DataTableConverter.GetValue(rb.DataContext, "CHK").ToString() == "0") // 2024.11.12. 김영국 - DataType의 값이 long으로 들어와서 String 비교함.
            {
                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);

                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                InitUsrControl();

                // 선택 Row 
                _drSelectRow = _util.GetDataGridFirstRowBycheck(dgLotList, "CHK", false);

                SetValue();

                // 불량/Loss/물품청구 조회
                GetDefectInfo();

                // 설비불량정보 탭 조회
                GetEqpFaultyData();

                // 완성LOT 탭 조회
                if (!Util.NVC(_drSelectRow["PROCID"]).Equals(Process.NOTCHING))
                {
                    GetSubLot();
                }

                // 투입바구니 탭 조회
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.PACKAGING))
                {
                    GetInBox(Util.NVC(_drSelectRow["LOTID"]), int.Parse(Util.NVC(_drSelectRow["WIPSEQ"])));
                }

                // 설비작업조건 탭 조회
                GetEqptCond();

                SetControlVisibility(cboProcess.SelectedValue.ToString());

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(Util.NVC(_drSelectRow["PROCID"]));
            }
        }

        #endregion

        #region # 실적확인 : W/O 선택
        private void btnWorkorder_Click(object sender, RoutedEventArgs e)
        {
            popupWO();
        }
        #endregion

        #region # 실적확인 : 작업일자 변경
        private void dtpCaldate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                else
                    dtPik.Focus();
            }));
        }
        #endregion

        #region # 실적확인 : 작업조선택(팝업)
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            popupShift();
        }
        #endregion

        #region # 실적확인 : 작업자(팝업)
        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            popupWorker();
        }
        #endregion

        #region # 실적확인 : 요청자(팝업)
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            popupUser();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popupUser();
            }
        }
        #endregion

        #region # 실적확인 : 투입수,양품수, 구간잔량(전,후), 재투입 변경
        private void txtQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            C1NumericBox nbox = sender as C1NumericBox;

            //if (nbox.Value.ToString() == "NaN") return;

            CalculateQty();
        }

        #endregion

        #region # 불량/Loss/물품청구 탭 Grid(dgDefect) : 불량,로스,물청 수량변경
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                #region 미확인 Loss 음수 여부 Validation
                //if (e == null || e.Cell == null)
                //    return;

                //if (sender == null) return;

                //C1DataGrid grd = sender as C1DataGrid;

                //int idx = _util.GetDataGridRowIndex(grd, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");

                //if (idx >= 0)
                //{
                //    DataTable dtDefect = DataTableConverter.Convert(dgDefect.ItemsSource);

                //    if (dtDefect == null || dtDefect.Rows.Count == 0) return;

                //    double dDefectQty = double.Parse(Util.NVC(dtDefect.Compute("sum(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'")));

                //    // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량(전+후) + 재투입수
                //    double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - dDefectQty  - (txtPrdtReqQty.Value + txtAfterSectionQty.Value) + txtReInputQty.Value;

                //    if (dUnidentifiedQty < 0)
                //    {
                //        // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
                //        Util.MessageValidation("SFU6035");

                //        grd.EndEdit();
                //        grd.EndEditRow(true);

                //        DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);

                //        grd.UpdateLayout();
                //    }
                //}
                #endregion

                SumDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    // 입력 불가
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }

                    // 미확인LOSS
                    if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PRCS_ITEM_CODE")).Equals("UNIDENTIFIED_QTY"))
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (!e.Cell.Column.Name.Equals("ACTNAME"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        //if (sFlag == "Y" || Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "PRCS_ITEM_CODE")).Equals("UNIDENTIFIED_QTY"))
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        //}
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        #endregion

        #region # 설비불량정보 탭
        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupEqptDfctCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpFaulty_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgEqpFaulty.TopRows.Count; i < dgEqpFaulty.Rows.Count; i++)
                {
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") ||
                           Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비작업조건탭 : 저장
        private void btnEqptCondSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptCondSave())
                return;

            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetEqptCond();
                }
            });
        }
        #endregion

        #region # 완성Lot 탭 : Geid Event
        private void dgSubLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                // 수정 가능여부에 따른 CHK 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODIFY_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                }
            }));

        }

        private void dgSubLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("CHK") || e.Column.Name.Equals("WIPQTY"))
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "MODIFY_YN").Equals("N"))
                {
                    e.Cancel = true;
                }
            }

            // 2023.10.27 Muhammad Fajar : Add editable remarks column for dgSublot and add button to add additional remark for azs
            if (e.Column.Name.Equals("colAZSRemark"))
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").ToString() == "0")
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgSubLot_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name.Equals("WIPQTY") || e.Cell.Column.Name.Equals("CHK"))
                {
                    (dgSubLot.GetCell(e.Cell.Row.Index, dgSubLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;

                    if (e.Cell.Column.Name.Equals("WIPQTY"))
                    {
                        DataTable dtSublot = DataTableConverter.Convert(dgSubLot.ItemsSource);

                        if (dtSublot == null || dtSublot.Rows.Count == 0) return;

                        double dOutQty = double.Parse(Util.NVC(dtSublot.Compute("sum(WIPQTY)", string.Empty)));

                        txtOutQty.Value = dOutQty;

                        // 미확인 Loss 산출
                        CalculateQty();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSubLot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgSubLot.GetCellFromPoint(pnt);

            // 2023.10.27 Muhammad Fajar : Add editable remarks column for dgSublot
            if (cell != null && cell.Column.Name != "colAZSRemark") // -> if selected cell's column is not "REMARK", check the checkbox
            {
                int idx = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgSubLot.Rows[idx].DataItem, "CHK").GetString();

                for (int i = 0; i < dgSubLot.GetRowCount(); i++)
                {
                    if (checkFlag == "0")
                    {
                        DataTableConverter.SetValue(dgSubLot.Rows[i].DataItem, "CHK", idx == i);

                    }
                    else
                    {
                        DataTableConverter.SetValue(dgSubLot.Rows[i].DataItem, "CHK", false);
                    }
                }
                SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
            }

        }

        private void dgSubLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CSTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupAssyCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "LOTID")),
                                          Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "CSTID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성Lot 탭 : Tray 삭제
        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayDelete())
                return;

            try
            {
                string messageCode = "SFU1230";

                if (!string.IsNullOrEmpty(ValidationTrayCellQtyCode()))
                    messageCode = ValidationTrayCellQtyCode();

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteTray();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성Lot 탭 : 저장 -> Visibility="Collapsed"
        private void btnSubLotSave_Click(object sender, RoutedEventArgs e)
        {
            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSubLot();
                }
            });
        }
        #endregion

        #region # 투입 바구니 탭 : 투입취소
        private void btnInputLotCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputLotCancel())
                return;

            // 투입취소 하시겠습니까?
            Util.MessageConfirm("SFU1988", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputHalfProductCancel(dgInputBox);
                }
            });
        }
        #endregion

        #region # 투입 바구니 탭 : 투입

        private void btnInputBox_Click(object sender, RoutedEventArgs e)
        {
            popupModifyPKGBox();
        }
        #endregion

        #region # 저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgDefect.EndEditRow(true);

                if (!ValidationSave())
                    return;

                string messageCode = txtUnidentifiedQty.Value < 0 ? "SFU3746" : "SFU1241";

                // 저장하시겠습니까?
                Util.MessageConfirm(messageCode, result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (Util.NVC(_drSelectRow["WIPSTAT"]).Equals("EQPT_END"))
                        {
                            SaveWO();
                        }
                        else
                        {
                            Save();
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #endregion

        #region Mehod

        private void GetCellManagementInfo()
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_MNGT_TYPE_CODE";
            inTable.Rows.Add(dr);
            _dtCellManagement = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", inTable);

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            DataRow dataRow = inDataTable.NewRow();
            dataRow["LANGID"] = LoginInfo.LANGID;
            dataRow["CMCDTYPE"] = "CMCDTYPE";
            dataRow["CMCODE"] = "CELL_MNGT_TYPE_CODE";
            inDataTable.Rows.Add(dataRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
                _cellManageGroup = dtResult.Rows[0]["CMCDNAME"].GetString();
        }

        #region # 생산 Lot 리스트 조회
        public void GetLotList(string ProdLotID = null)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_EDIT_LOT_LIST_L";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                //inTable.Columns.Add("NORMAL", typeof(string));
                //inTable.Columns.Add("PILOT", typeof(string));
                inTable.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtLotId.Text))
                {
                    dr["AREAID"] = cboArea.SelectedValue.ToString();
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                    dr["PROCID"] = cboProcess.SelectedValue.ToString();
                    dr["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();

                    // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                    //if (cboProductDiv.SelectedValue.ToString() == "P")
                    //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                    //else if (cboProductDiv.SelectedValue.ToString() == "X")
                    //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                    dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["PROCID"] = cboProcess.SelectedValue.ToString();
                    dr["LOTID"] = txtLotId.Text;
                }

                inTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    // Clear
                    InitUsrControl();

                    Util.GridSetData(dgLotList, bizResult, FrameOperation, true);

                    if (bizResult.Rows.Count > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(ProdLotID))
                        {
                            // 투입바구니 탭 : 투입취소후 다시 조회, 완성Lot 탭 : Tray 삭제후 다시 조회
                            int idx = _util.GetDataGridRowIndex(dgLotList, "LOTID", ProdLotID);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", 1);

                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                dgLotList.CurrentCell = dgLotList.GetCell(idx, dgLotList.Columns.Count - 1);

                                _drSelectRow = _util.GetDataGridFirstRowBycheck(dgLotList, "CHK");
                                SetValue();
                            }
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 불량/Loss/물품청구 탭 조회
        private void GetDefectInfo()
        {
            try
            {
                string BizNAme = "DA_QCA_SEL_WIPRESONCOLLECT_L";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = Util.NVC(_drSelectRow["AREAID"]);
                newRow["PROCID"] = Util.NVC(_drSelectRow["PROCID"]);
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(BizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgDefect, searchResult, null);

                    SumDefectQty();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비불량정보 탭 조회 
        private void GetEqpFaultyData() //string sLot, string sWipSeq)
        {
            try
            {
                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgEqpFaulty, searchResult, null);

                    dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                    dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                    if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                    else
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;

                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성 LOT 탭 조회
        private void GetSubLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                inTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EDIT_SUBLOT_LIST_SM_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    // 2023.10.27 Muhammad Fajar : Add editable remarks column for dgSublot
                    // ADD COLUMN "PRE-REMARK"
                    searchResult.Columns.Add("PRE_REMARK");

                    foreach (DataRow rw in searchResult.Rows)
                    {
                        rw["PRE_REMARK"] = rw["REMARK"];
                    }

                    searchResult.AcceptChanges();

                    Util.GridSetData(dgSubLot, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비작업조건 탭 : 조회
        private void GetEqptCond()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("BEFORE_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Util.NVC(_drSelectRow["PROCID"]);
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["EQSGID"] = Util.NVC(_drSelectRow["EQSGID"]);
                newRow["BEFORE_LOTID"] = "TEMP_LOT"; // NULL 또는 공백으로 BIZ 호출 시 TIMEOUT 으로 인해 임의 값 설정.
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgEqptCond, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 투입바구니 탭 조회
        private void GetInBox(string lotid, int wipseq)
        {
            try
            {

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_BOX_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = lotid;
                newRow["WIPSEQ"] = wipseq.Equals("") ? 1 : Convert.ToDecimal(wipseq);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgInputBox, searchResult, null);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비작업조건 탭 : 저장
        private void SetEqptCond()
        {
            try
            {
                ShowLoadingIndicator();

                dgEqptCond.EndEdit();

                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.PACKAGING))
                {
                    DataSet inDataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = inDataSet.Tables["IN_EQP"];

                    DataRow newRow = null;
                    DataTable in_Data = inDataSet.Tables["IN_DATA"];

                    // Biz Core Multi 처리 없으므로 임시로 Unit 단위로 비즈 호출 처리 함.
                    // 추후 Multi Biz 생성 시 처리 방법 변경 필요.
                    string sUnitID = "";
                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        string sTmp = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "UNIT_EQPTID"));

                        if (i == 0)
                        {
                            sUnitID = sTmp;

                            newRow = null;

                            newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                            newRow["UNIT_EQPTID"] = sUnitID;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                            newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                            inTable.Rows.Add(newRow);

                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);
                        }
                        else
                        {
                            if (sUnitID.Equals(sTmp))
                            {
                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                            else
                            {
                                // data 존재 시 biz call
                                if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                                {
                                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, inDataSet);

                                    inTable.Rows.Clear();
                                    in_Data.Rows.Clear();
                                }

                                sUnitID = sTmp;

                                newRow = null;

                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                                newRow["UNIT_EQPTID"] = sUnitID;
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                                newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                                inTable.Rows.Add(newRow);

                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                        }
                    }

                    // 마지막 Unit 처리.
                    if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, inDataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                    }
                }
                else
                {
                    DataSet inDataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = inDataSet.Tables["IN_EQP"];

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                    newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                    inTable.Rows.Add(newRow);

                    DataTable in_Data = inDataSet.Tables["IN_DATA"];

                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        newRow = null;

                        newRow = in_Data.NewRow();
                        newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                        newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                        in_Data.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, inDataSet);

                    inTable.Rows.Clear();
                    in_Data.Rows.Clear();

                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                }

                GetEqptCond();
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
        #endregion

        #region # 완성Lot 탭 : Tray 삭제
        private void DeleteTray()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS_UI";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                int rowidx = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");

                foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                        {
                            DataRow dr = inTable.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["IFMODE"] = IFMODE.IFMODE_OFF;
                            dr["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                            dr["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                            dr["OUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                            dr["TRAYID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            dr["WO_DETL_ID"] = txtWorkorder.Text;
                            dr["USERID"] = LoginInfo.USERID;
                            inTable.Rows.Add(dr);

                            new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP", null, ds);
                            inTable.Rows.Remove(dr);
                        }
                    }
                }

                HiddenLoadingIndicator();
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                GetLotList(Util.NVC(_drSelectRow["LOTID"]));
                GetSubLot();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성LOT 탭 : 저장 -> Visibility="Collapsed" 
        private bool SaveSubLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = GetSaveDataSet();
                DataTable inTable = inDataSet.Tables["INDATA"];

                DataTable dtSubLot = DataTableConverter.Convert(dgSubLot.ItemsSource);

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                foreach (DataRow dr in dtSubLot.Rows)
                {
                    if (Util.NVC(dr["CHK"]).Equals("True") || Util.NVC(dr["CHK"]).Equals("1"))
                    {
                        inTable.Clear();
                        DataRow drIn = inTable.NewRow();

                        drIn["LOTID"] = dr["LOTID"];
                        //drIn["WIPSEQ"] = dr["WIPSEQ"];    
                        drIn["WIPSEQ"] = int.Parse(Util.NVC(_drSelectRow["WIPSEQ"]));   // 250317 천진수
                        drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");

                        drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                        drIn["WOID"] = dr["WOID"];
                        drIn["WIPQTY_ED"] = dr["WIPQTY"];
                        drIn["WIPQTY2_ED"] = dr["WIPQTY"];
                        drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["SHIFT"] = txtShift.Tag;
                        drIn["WRK_USERID"] = txtWorker.Tag;
                        drIn["WRK_USER_NAME"] = txtWorker.Text;
                        drIn["NOTE"] = SetWipNote();

                        /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            //drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                            2023.09.20 고해선 책임 요청으로 원복
                         */
                        drIn["FORCE_FLAG"] = "Y";               // Y이면 다음 공정 투입 여부 체크 안함

                        drIn["REQ_NOTE"] = txtReqNote.Text;
                        drIn["PROD_LOT_YN"] = "N";               // 생산LOT여부(UI에서 생산LOT정보 일때 Y, 완성LOT정보일때 N)
                        drIn["CHANGE_WIPQTY_FLAG"] = "Y";       // 250317 천진수 CHANGE_WIPQTY_FLAG 항목 추가 
                        inTable.Rows.Add(drIn);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MODIFY_LOT_L", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", null, inDataSet);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region # 투입바구니 탭 : 취소
        private void InputHalfProductCancel(C1DataGrid grid)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT_CL";

                DataSet inDataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET2_CL();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["PROCID"] = Util.NVC(_drSelectRow["PROCID"]);

                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = inDataSet.Tables["INLOT"];

                for (int i = 0; i < grid.Rows.Count - grid.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(grid, "CHK", i)) continue;

                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(grid.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    GetLotList(Util.NVC(_drSelectRow["LOTID"]));
                    GetInBox(Util.NVC(_drSelectRow["LOTID"]), Convert.ToInt16(Util.NVC(_drSelectRow["WIPSEQ"])));
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # WO 저장 
        private void SaveWO()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow drIn = inTable.NewRow();
                drIn["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                drIn["WOID"] = Util.GetCondition(txtWorkorder);
                drIn["NOTE"] = Util.GetCondition(txtNote);
                drIn["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(drIn);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_MODIFY_LOT_WOID", "INDATA", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    GetLotList();

                    // 저장되었습니다.
                    Util.MessageInfo("SFU1270");
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 실적확인 : 저장
        private void Save()
        {
            try
            {
                if (!SaveSubLot())
                    return;

                DataSet inDataSet = GetSaveDataSet();

                #region [INDATA]

                DataTable inDataTable = inDataSet.Tables["INDATA"];

                DataRow drIn = inDataTable.NewRow();
                drIn["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                drIn["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                drIn["CALDATE"] = dtTime.ToString("yyyy-MM-dd");
                drIn["WIPDTTM_ED"] = dtTime.ToString("yyyy-MM-dd");
                drIn["SHIFT"] = txtShift.Tag;
                drIn["WRK_USERID"] = txtWorker.Tag;
                drIn["WRK_USER_NAME"] = txtWorker.Text;
                drIn["WOID"] = txtWorkorder.Text;

                // Null인 경우 "NaN" 또는 "非数字"로 발생
                double dOutQty;
                double dLossQty;
                double dDefectQty;
                double dPrdtReqQty;
                double dInputQty;
                double dReInputQty;

                if (txtOutQty.Value.ToString() == double.NaN.ToString())
                    dOutQty = 0;
                else
                    dOutQty = txtOutQty.Value;

                if (txtLossQty.Value.ToString() == double.NaN.ToString())
                    dLossQty = 0;
                else
                    dLossQty = txtLossQty.Value;

                if (txtDefectQty.Value.ToString() == double.NaN.ToString())
                    dDefectQty = 0;
                else
                    dDefectQty = txtDefectQty.Value;

                if (txtPrdtReqQty.Value.ToString() == double.NaN.ToString())
                    dPrdtReqQty = 0;
                else
                    dPrdtReqQty = txtPrdtReqQty.Value;

                if (txtInputQty.Value.ToString() == double.NaN.ToString())
                    dInputQty = 0;
                else
                    dInputQty = txtInputQty.Value;

                if (txtReInputQty.Value.ToString() == double.NaN.ToString())
                    dReInputQty = 0;
                else
                    dReInputQty = txtReInputQty.Value;

                drIn["WIPQTY_ED"] = dOutQty;
                drIn["WIPQTY2_ED"] = dOutQty;
                drIn["LOSS_QTY"] = dLossQty;
                drIn["LOSS_QTY2"] = dLossQty;
                drIn["DFCT_QTY"] = dDefectQty;
                drIn["DFCT_QTY2"] = dDefectQty;
                drIn["PRDT_REQ_QTY"] = dPrdtReqQty;
                drIn["PRDT_REQ_QTY2"] = dPrdtReqQty;

                drIn["NOTE"] = SetWipNote();
                drIn["REQ_USERID"] = txtUserName.Tag.ToString();
                drIn["USERID"] = LoginInfo.USERID;
                drIn["CHANGE_WIPQTY_FLAG"] = "Y";

                /*
                            2023.09.06 주동석 수정
                            E20230829-000518
                            요청자 : 장인천 책임
                            목적 : 생산실적 수정화면에서 다음공정 투입시에는 수정 안되도록 BLOCKING
	
                            '- 실적 수정 메뉴는 특이작업과 시스템관리 하위에 존재함
                            . 특이작업 하위의 실적수정 메뉴는 다음 공정 투입시 실적수정 막고 있음
                            . 시스템관리 하위의 실적수정 메뉴는 다음공정 투입시에도 수정 가능함
                            => 시스템관리 하위의 실적 수정 메뉴에서도 다음 공정 투입시에 수정 못하
                            도록 BLOCKG 하도록 수정함. (UI에서 FORCE_FLAG = 'Y' 제거)

                            if (double.Parse(_drSelectRow["WIPQTY_ED"].ToString()) == dOutQty)
                            {
                                // 양품수량 변경 없는 경우 다음 공정 투입 이력 있어도 수정 가능하게 FORCE_FLAG : Y 로 변경
                                drIn["FORCE_FLAG"] = "Y";
                            }
                            else
                            {
                                drIn["FORCE_FLAG"] = "N";
                            }

                            2023.09.20 고해선 책임 요청으로 원복
                */

                if (double.Parse(_drSelectRow["WIPQTY_ED"].ToString()) == dOutQty)
                {
                    // 양품수량 변경 없는 경우 다음 공정 투입 이력 있어도 수정 가능하게 FORCE_FLAG : Y 로 변경
                    drIn["FORCE_FLAG"] = "Y";
                }
                else
                {
                    drIn["FORCE_FLAG"] = "N";
                }

                drIn["REQ_NOTE"] = txtReqNote.Text;
                ////drIn["INPUT_QTY"] = txtInputQty.Value.ToString().Equals("NaN") ? 0 : txtInputQty.Value;
                ////drIn["RE_INPUT_QTY"] = txtReInputQty.Value.ToString().Equals("NaN") ? 0 : txtReInputQty.Value;

                drIn["INPUT_QTY"] = dInputQty;
                drIn["RE_INPUT_QTY"] = dReInputQty;

                //drIn["PRE_SECTION_QTY"] = txtPreSectionQty.Value.ToString().Equals("NaN") ? 0 : txtPreSectionQty.Value;
                //drIn["AFTER_SECTION_QTY"] = txtAfterSectionQty.Value.ToString().Equals("NaN") ? 0 : txtAfterSectionQty.Value;
                drIn["PROD_LOT_YN"] = "Y";               // 생산LOT여부(UI에서 생산LOT정보 일때 Y, 완성LOT정보일때 N)

                // 자공정 LOSS (공급부)
                if (Util.NVC(_drSelectRow["PROCID"]) == "A5000")
                {
                    double dCurrProcLossQty;

                    if (txtCurrProcLossQty.Value.ToString() == double.NaN.ToString())
                        dCurrProcLossQty = 0;
                    else
                        dCurrProcLossQty = txtCurrProcLossQty.Value;

                    drIn["CURR_PROC_LOSS_QTY"] = dCurrProcLossQty;
                }

                inDataTable.Rows.Add(drIn);

                #endregion

                #region [IN_LOSS]
                DataTable inDataLoss = inDataSet.Tables["IN_LOSS"];

                DataTable dtdefect = DataTableConverter.Convert(dgDefect.ItemsSource);
                DataRow[] drLoss = dtdefect.Select("ACTID ='LOSS_LOT'");

                foreach (DataRow dr in drLoss)
                {
                    DataRow drInLoss = inDataLoss.NewRow();

                    drInLoss["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                    drInLoss["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                    drInLoss["RESNCODE"] = dr["RESNCODE"];
                    if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                    {
                        drInLoss["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInLoss["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInLoss["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                    }
                    else
                    {
                        drInLoss["RESNQTY"] = 0;
                        drInLoss["RESNQTY2"] = 0;
                        drInLoss["DFCT_TAG_QTY"] = 0;
                    }
                    drInLoss["RESNCODE_CAUSE"] = "";
                    drInLoss["PROCID_CAUSE"] = "";

                    inDataLoss.Rows.Add(drInLoss);
                }

                #endregion

                #region [IN_DFCT]

                DataTable inDataDfct = inDataSet.Tables["IN_DFCT"];

                DataRow[] drDefect = dtdefect.Select("ACTID ='DEFECT_LOT'");
                foreach (DataRow dr in drDefect)
                {

                    DataRow drInDfct = inDataDfct.NewRow();

                    drInDfct["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                    drInDfct["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                    drInDfct["RESNCODE"] = dr["RESNCODE"];
                    if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                    {
                        drInDfct["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInDfct["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInDfct["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                    }
                    else
                    {
                        drInDfct["RESNQTY"] = 0;
                        drInDfct["RESNQTY2"] = 0;
                        drInDfct["DFCT_TAG_QTY"] = 0;
                    }
                    drInDfct["RESNCODE_CAUSE"] = "";
                    drInDfct["PROCID_CAUSE"] = "";

                    inDataDfct.Rows.Add(drInDfct);
                }

                #endregion

                #region [IN_PRDT_REQ]

                DataTable inDataPrdtReq = inDataSet.Tables["IN_PRDT_REQ"];

                DataRow[] drCharge = dtdefect.Select("ACTID ='CHARGE_PROD_LOT'");
                foreach (DataRow dr in drCharge)
                {
                    DataRow drInPrdtReq = inDataPrdtReq.NewRow();

                    drInPrdtReq["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                    drInPrdtReq["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                    drInPrdtReq["RESNCODE"] = dr["RESNCODE"];
                    if (!Util.NVC(dr["RESNQTY"]).Equals(""))
                    {
                        drInPrdtReq["RESNQTY"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInPrdtReq["RESNQTY2"] = Convert.ToDecimal(dr["RESNQTY"]);
                        drInPrdtReq["DFCT_TAG_QTY"] = dr.Table.Columns.Contains("TAG_QTY") ? Convert.ToDecimal(dr["TAG_QTY"]) : 0;
                    }
                    else
                    {
                        drInPrdtReq["RESNQTY"] = 0;
                        drInPrdtReq["RESNQTY2"] = 0;
                        drInPrdtReq["DFCT_TAG_QTY"] = 0;
                    }
                    drInPrdtReq["RESNCODE_CAUSE"] = "";
                    drInPrdtReq["PROCID_CAUSE"] = "";
                    drInPrdtReq["RESNNOTE"] = dr["RESNNOTE"];
                    drInPrdtReq["COST_CNTR_ID"] = dr["COST_CNTR_ID"];

                    inDataPrdtReq.Rows.Add(drInPrdtReq);
                }
                #endregion

                ShowLoadingIndicator();

                #region [E20240704-001494][CAT_UP_0542]
                // 기존 비동기 방식으로 수행 되던 비즈 동기방식으로 수행되도록 수정
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MODIFY_LOT_L", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", null, inDataSet);
                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_LOT_L", "INDATA,IN_LOSS,IN_DFCT,IN_PRDT_REQ", null, (bizResult, bizException) =>
                //{
                //    HiddenLoadingIndicator();

                //    if (bizException != null)
                //    {
                //        HiddenLoadingIndicator();
                //        Util.MessageException(bizException);
                //        return;
                //    }

                GetLotList();

                // 저장되었습니다.
                Util.MessageInfo("SFU1270");
                //    }, inDataSet);
                #endregion [E20240704-001494][CAT_UP_0542]
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region # 투입관련 버튼 제어
        private void SetInputHistButtonControls(string sProcID)
        {
            try
            {
                bool bRet = false;
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("ATTRIBUTE1", typeof(string));
                dt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_LOT_CANCEL_TERM_USE";
                dr["ATTRIBUTE1"] = Util.NVC(cboArea.SelectedValue);
                dr["ATTRIBUTE2"] = sProcID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", dt);
                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE4"]).Trim().Equals("Y"))
                {
                    btnInputBox.Visibility = Visibility.Visible;                    // 투입바구니 Tab : 바구니투입
                    btnInputLotCancel.Visibility = Visibility.Visible;              // 투입바구니 Tab : 투입취소
                }
                else
                {
                    btnInputBox.Visibility = Visibility.Collapsed;                    // 투입바구니 Tab : 바구니투입
                    btnInputLotCancel.Visibility = Visibility.Collapsed;              // 투입바구니 Tab : 투입취소
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch(bool isLot = false)
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (!isLot)
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
                }

                if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 라인을선택하세요.
                    Util.MessageValidation("SFU1223");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationInputLotCancel()
        {
            if (Util.NVC(_drSelectRow["ERP_CLOSE"]).Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgInputBox, "CHK");
            if (iRow < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //  "투입 LOT이 없습니다."
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            if (_drSelectRow == null || Util.NVC(_drSelectRow["LOTID"]).Equals(""))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            //if (txtUnidentifiedQty.Value < 0)
            //{
            //    // 입력오류 : 입력한 불량으로 인해 미확인LOSS가 음수가 됩니다.
            //    Util.MessageValidation("SFU6035");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(txtReqNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            #region [E20240704-001494][CAT_UP_0542]
            DataTable dtResult = ApprReqChk();
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                object[] parameters = new object[1];
                parameters[0] = Util.NVC(dtResult.Rows[0]["APPR_BIZ_NAME"]);
                Util.MessageInfo("SFU9934", parameters);
                return false;
            }
            #endregion [E20240704-001494][CAT_UP_0542]

            if (Util.NVC(_drSelectRow["ERP_CLOSE"]).Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            //if (!Util.NVC(_drSelectRow["WIPSTAT"]).Equals("EQPT_END"))
            //{
            //    if (cboIrreglProdLotType.SelectedValue == null || cboIrreglProdLotType.SelectedValue.ToString().Equals("SELECT"))
            //    {
            //        // %1(을)를 선택하세요.
            //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("작업타입"));
            //        return false;
            //    }
            //}

            if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[_util.GetDataGridCheckFirstRowIndex(dgLotList, "CHK")].DataItem, "PROCID")).Equals(Process.NOTCHING))
            {
                double dOutQty;
                if (txtOutQty.Value.ToString() == double.NaN.ToString())
                    dOutQty = 0;
                else
                    dOutQty = txtOutQty.Value;

                if (dOutQty < 1)
                {
                    Util.MessageValidation("SFU3753"); // 입력오류 : 양품수량은 0보다 커야 합니다.
                    return false;
                }
            }

            return true;
        }

        private string ValidationTrayCellQtyCode()
        {
            double dCellQty = 0;
            string returnmessageCode = string.Empty;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgSubLot.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                {
                    string cellQty = DataTableConverter.GetValue(row.DataItem, "WIPQTY").GetString();

                    if (!string.IsNullOrEmpty(cellQty))
                        double.TryParse(cellQty, out dCellQty);

                    if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                    {
                        return "SFU1320";
                    }
                }
            }

            return returnmessageCode;
        }

        private bool ValidationTrayDelete()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(_drSelectRow["ERP_CLOSE"]).Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        private bool ValidationEqptCondSave()
        {
            if (_drSelectRow == null || Util.NVC(_drSelectRow["LOTID"]).Equals(""))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgSubLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPopupShift()
        {
            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            return true;
        }

        private bool ValidationPopupWorker()
        {
            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 선택된 작업조가 없습니다.
                Util.MessageValidation("SFU1646");
                return false;
            }

            return true;
        }

        private bool ValidationPopupWO()
        {
            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            return true;
        }

        private bool ValidationPopupModifyPKGBox()
        {
            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
            {
                // LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return false;
            }

            if (Util.NVC(_drSelectRow["ERP_CLOSE"]).Equals("CLOSE"))
            {
                // ERP 생산실적이 마감 되었습니다.
                Util.MessageValidation("SFU3494");
                return false;
            }

            return true;
        }

        #endregion

        #region [Popup]

        #region # W/O 팝업
        private void popupWO()
        {
            if (!ValidationPopupWO())
                return;

            CMM_WORKORDER popWO = new CMM_WORKORDER();
            popWO.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popWO.Name.ToString()) == false)
                return;

            object[] Parameters = new object[6];
            Parameters[0] = LoginInfo.CFG_SHOP_ID;
            Parameters[1] = Util.NVC(_drSelectRow["AREAID"]);
            Parameters[2] = Util.NVC(_drSelectRow["EQSGID"]);
            Parameters[3] = Util.NVC(_drSelectRow["PROCID"]);
            Parameters[4] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[5] = txtWorkorderDetail.Text;
            C1WindowExtension.SetParameters(popWO, Parameters);

            popWO.Closed += new EventHandler(popWO_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            grdMain.Children.Add(popWO);
            popWO.BringToFront();
        }
        private void popWO_Closed(object sender, EventArgs e)
        {
            CMM_WORKORDER popup = sender as CMM_WORKORDER;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtWorkorder.Text = popup.WOID;
                txtWorkorderDetail.Text = popup.WOIDDETAIL;
            }
        }
        #endregion

        #region # 작업조 팝업

        private void popupShift()
        {
            if (!ValidationPopupShift())
                return;

            CMM_SHIFT popShift = new CMM_SHIFT();
            popShift.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popShift.Name.ToString()) == false)
                return;

            object[] Parameters = new object[4];
            Parameters[0] = LoginInfo.CFG_SHOP_ID;
            Parameters[1] = Util.NVC(_drSelectRow["AREAID"]);
            Parameters[2] = Util.NVC(_drSelectRow["EQSGID"]);
            Parameters[3] = Util.NVC(_drSelectRow["PROCID"]);
            C1WindowExtension.SetParameters(popShift, Parameters);

            popShift.Closed += new EventHandler(popShift_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            grdMain.Children.Add(popShift);
            popShift.BringToFront();
        }

        private void popShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT popup = sender as CMM_SHIFT;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (!txtShift.Tag.Equals(popup.SHIFTCODE))
                {
                }
                txtShift.Tag = popup.SHIFTCODE;
                txtShift.Text = popup.SHIFTNAME;
            }
            grdMain.Children.Remove(popup);
        }
        #endregion

        #region # 작업자 팝업
        private void popupWorker()
        {
            if (!ValidationPopupWorker())
                return;

            CMM_SHIFT_USER2 popWorker = new CMM_SHIFT_USER2();
            popWorker.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popWorker.Name.ToString()) == false)
                return;

            object[] Parameters = new object[8];
            Parameters[0] = LoginInfo.CFG_SHOP_ID;
            Parameters[1] = LoginInfo.CFG_AREA_ID;
            Parameters[2] = Util.NVC(_drSelectRow["EQSGID"]);
            Parameters[3] = Util.NVC(_drSelectRow["PROCID"]);
            Parameters[4] = Util.NVC(txtShift.Tag);
            Parameters[5] = Util.NVC(txtWorker.Tag);
            Parameters[6] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.

            C1WindowExtension.SetParameters(popWorker, Parameters);

            popWorker.Closed += new EventHandler(popWorker_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            grdMain.Children.Add(popWorker);
            popWorker.BringToFront();
        }
        private void popWorker_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(popup.SHIFTNAME);
                txtShift.Tag = Util.NVC(popup.SHIFTCODE);
                txtWorker.Text = Util.NVC(popup.USERNAME);
                txtWorker.Tag = Util.NVC(popup.USERID);
            }
            grdMain.Children.Remove(popup);
        }
        #endregion

        #region # 요청자 팝업
        private void popupUser()
        {
            CMM_PERSON popUser = new CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popUser.Name.ToString()) == false)
                return;

            object[] Parameters = new object[1];
            Parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);
            grdMain.Children.Add(popUser);
            popUser.BringToFront();
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;

                txtReqNote.Focus();
            }
            grdMain.Children.Remove(popup);
        }
        #endregion

        #region # 설비불량정보 조회 팝업
        private void popupEqptDfctCell(string sPortID)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popEqptDfctCell = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
            popEqptDfctCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popEqptDfctCell.Name.ToString()) == false)
                return;

            object[] Parameters = new object[3];
            Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[1] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[2] = sPortID;
            C1WindowExtension.SetParameters(popEqptDfctCell, Parameters);

            popEqptDfctCell.Closed += new EventHandler(popEqptDfctCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popEqptDfctCell.ShowModal()));
        }

        private void popEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popup = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        #endregion

        #region # 완성LOT Cell 조회 팝업
        private void popupAssyCell(string sLotID, string CstID)
        {
            COM001_302_CELL popAssyCell = new COM001_302_CELL();
            popAssyCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popAssyCell.Name.ToString()) == false)
                return;

            object[] Parameters = new object[4];
            Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[1] = sLotID;
            Parameters[2] = CstID;

            C1WindowExtension.SetParameters(popAssyCell, Parameters);

            popAssyCell.Closed += new EventHandler(popAssyCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popAssyCell.ShowModal()));
        }

        private void popAssyCell_Closed(object sender, EventArgs e)
        {
            COM001_302_CELL popup = sender as COM001_302_CELL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        #endregion

        #region # 투입 바구니 팝업
        private void popupModifyPKGBox()
        {
            if (!ValidationPopupModifyPKGBox()) return;

            COM001_305_MODIFY_PKG_BOX popModifyPKGBox = new COM001_305_MODIFY_PKG_BOX();
            popModifyPKGBox.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popModifyPKGBox.Name.ToString()) == false)
                return;

            object[] Parameters = new object[7];

            Parameters[0] = Util.NVC(_drSelectRow["EQSGID"]);
            Parameters[1] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[2] = Util.NVC(_drSelectRow["WO_DETL_ID"]);
            Parameters[3] = Util.NVC(_drSelectRow["PROCID"]);
            Parameters[4] = Util.NVC(_drSelectRow["SHIFT"]);
            Parameters[5] = Util.NVC(_drSelectRow["CALDATE"]);
            Parameters[6] = Util.NVC(_drSelectRow["LOTID"]);
            C1WindowExtension.SetParameters(popModifyPKGBox, Parameters);

            popModifyPKGBox.Closed += new EventHandler(popModifyPKGBox_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => popModifyPKGBox.ShowModal()));
        }

        private void popModifyPKGBox_Closed(object sender, EventArgs e)
        {
            COM001_305_MODIFY_PKG_BOX popup = sender as COM001_305_MODIFY_PKG_BOX;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                string lotid = Util.NVC(_drSelectRow["LOTID"]);
                int wipseq = int.Parse(Util.NVC(_drSelectRow["WIPSEQ"]));

                GetLotList();

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")).Equals(lotid))
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "CHK", true);
                        break;
                    }
                }

                GetInBox(lotid, wipseq);
            }
        }

        #endregion


        #endregion

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        #region ## 그리드, 탭, Text Header Text 및 칼럼 Visibility
        private void SetControlVisibility(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                // Visibility.Collapsed
                InitTabControlVisible();

                #region ## 공정별 Header Text 변경
                DataRow[] drHeader;

                ////////////////////////////////////// dgLotList
                // 조립 전체
                drHeader = _dtLotListHeader.Select("PROCESS = '" + _assembly + "'");

                foreach (DataRow dr in drHeader)
                {
                    if (dgLotList.Columns.Contains(dr["COLUMN"].ToString()))
                        dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["TITLE"].ToString().Split(',').ToList<string>();
                }

                // 공정별 
                drHeader = _dtLotListHeader.Select("PROCESS = '" + sProcID + "'");
                foreach (DataRow dr in drHeader)
                {
                    if (dgLotList.Columns.Contains(dr["COLUMN"].ToString()))
                        dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["TITLE"].ToString().Split(',').ToList<string>();
                }
                _bLoadHeader = false; //C20210623-000317  다국어 처리
                #endregion

                #region ## 공정별 칼럼 Visibility
                ////////////////////////////////////// dgLotList
                SetGridVisibility(_dtLotListColumnVisible, dgLotList, sProcID);

                ////////////////////////////////////// 불량/Loss/물품청구 탭 : dgDefect
                SetGridVisibility(_dtDefectColumnVisible, dgDefect, sProcID);

                ////////////////////////////////////// 완성LOT 탭 : dgSubLot
                SetGridVisibility(_dtSubLotColumnVisible, dgSubLot, sProcID);

                ////////////////////////////////////// 설비작업조건 탭 : dgEqptCond
                SetGridVisibility(_dtEqptCondColumnVisible, dgEqptCond, sProcID);

                #endregion

                tbInputQty.Text = ObjectDic.Instance.GetObjectName("생산");

                // 2023.10.27 Muhammad Fajar : Visibility for column Remark AZS Stacking and button Additional Lot Remark
                btnAddLotRemark.Visibility = Visibility.Collapsed;
                dgSubLot.Columns["colAZSRemark"].Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.PACKAGING))
                {
                    // 완성LOT 탭
                    cTabHalf.Visibility = Visibility.Visible;

                    // 투입바구니 탭
                    cTabInBox.Visibility = Visibility.Visible;

                    // 재투입
                    tbReInputQty.Visibility = Visibility.Visible;
                    txtReInputQty.Visibility = Visibility.Visible;

                    // 완성LOT 탭의 수량 수정 불가, TRAY삭제 버튼
                    dgSubLot.Columns["WIPQTY"].IsReadOnly = true;

                    btnOutDel.Visibility = Visibility.Visible;
                }
                else
                {
                    if (sProcID.Equals(Process.NOTCHING))
                    {
                        txtOutQty.IsEnabled = true;
                        tbCurrProcLossQty.Visibility = Visibility.Visible;
                        tbM.Visibility = Visibility.Visible;
                        txtCurrProcLossQty.Visibility = Visibility.Visible;

                        //// 잔량 처리시 투입수 변경 못함
                        //if (_drSelectRow != null)
                        //{ 
                        //    if (Util.NVC(_drSelectRow["EQPT_UNMNT_TYPE_CODE"]).Equals("Y"))
                        //        txtInputQty.IsEnabled = false;
                        //}

                        // 투입수 수정 못하게 처리
                        txtInputQty.IsEnabled = false;
                    }
                    else if (sProcID.Equals(Process.VD_LMN))
                    {
                        dgLotList.Columns["INPUT_TAB_COUNT"].Visibility = Visibility.Visible;
                        dgLotList.Columns["END_TAB_COUNT"].Visibility = Visibility.Visible;
                        dgLotList.Columns["FIX_LOSS_QTY_VD"].Visibility = Visibility.Visible;

                        dgLotList.Columns["WRK_TYPE"].Visibility = Visibility.Visible;
                        dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["IRREGL_PROD_LOT_TYPE_NAME"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WOID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WO_DETL_ID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;

                        //dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { "투입량", "투입량" };
                        //dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { "양품량", "양품량" };
                        //dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { "불량량", "불량량" };
                        //dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { "LOSS량", "LOSS량" };
                        //dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { "물품청구", "물품청구" };

                        // C20210623-000317  다국어 처리
                        dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("투입량") };
                        dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("양품량"), ObjectDic.Instance.GetObjectName("양품량") };
                        dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("불량량"), ObjectDic.Instance.GetObjectName("불량량") };
                        dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("LOSS량"), ObjectDic.Instance.GetObjectName("LOSS량") };
                        dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("물품청구"), ObjectDic.Instance.GetObjectName("물품청구") };

                        txtOutQty.IsEnabled = true;
                        txtInputQty.IsEnabled = false;
                        tbWorkOrder.Visibility = Visibility.Collapsed;
                        txtWorkorder.Visibility = Visibility.Collapsed;
                        cTabHalf.Visibility = Visibility.Collapsed;
                        cTabEqptCond.Visibility = Visibility.Collapsed;
                        tbInputQty.Text = ObjectDic.Instance.GetObjectName("투입");
                    }
                    else
                    {
                        // 완성LOT 탭
                        cTabHalf.Visibility = Visibility.Visible;

                        // 2023.10.27 Muhammad Fajar : Add editable remarks column for dgSublot => 강성묵 추가 수정
                        SetOutLotRemarkUse(sProcID);
                        //// Visibility for column Remark AZS Stacking and button Additional Lot Remark
                        //if (sProcID.Equals(Process.AZS_STACKING))
                        //{
                        //    btnAddLotRemark.Visibility = Visibility.Visible;
                        //    dgSubLot.Columns["colAZSRemark"].Visibility = Visibility.Visible;
                        //    //
                        //}
                    }
                }

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls(sProcID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridVisibility(DataTable dt, C1DataGrid dg, string ProcID)
        {
            foreach (DataRow dr in dt.Rows)
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

                    // 검색 공정이 없으면 조립 구분자로 다시 검색
                    if (nindex < 0)
                    {
                        nindex = Array.IndexOf(sProcess, _assembly);
                    }

                    // 공정별 칼럼 Visibility
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (nindex < 0)
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                        else
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Visible;
                    }

                }
            }
        }
        #endregion

        #region # 선택 생산 Lot의 실적 확인(textBiox) Setting
        private void SetValue()
        {
            SetNumericBoxValueChangedEvent(false);

            txtSelectLot.Text = Util.NVC(_drSelectRow["LOTID"]);
            txtWorkorder.Text = Util.NVC(_drSelectRow["WOID"]);
            txtWorkorderDetail.Text = Util.NVC(_drSelectRow["WO_DETL_ID"]);
            txtLotStatus.Text = Util.NVC(_drSelectRow["WIPSNAME"]);

            dtpCaldate.Text = Util.NVC(_drSelectRow["CALDATE"]);
            dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(_drSelectRow["CALDATE"]));
            dtCaldate = Convert.ToDateTime(Util.NVC(_drSelectRow["CALDATE"]));

            txtShift.Text = Util.NVC(_drSelectRow["SHFT_NAME"]);
            txtShift.Tag = Util.NVC(_drSelectRow["SHIFT"]);
            txtStartTime.Text = Util.NVC(_drSelectRow["STARTDTTM"]);
            txtWorker.Text = Util.NVC(_drSelectRow["WRK_USER_NAME"]);
            txtWorker.Tag = Util.NVC(_drSelectRow["WRK_USERID"]);
            txtEndTime.Text = Util.NVC(_drSelectRow["ENDDTTM"]);
            txtInputQty.Value = Double.Parse(Util.NVC(_drSelectRow["INPUT_QTY"]));
            txtOutQty.Value = Double.Parse(Util.NVC(_drSelectRow["WIPQTY_ED"]));
            txtDefectQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_DFCT_QTY"]));
            txtLossQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_LOSS_QTY"]));
            txtPrdtReqQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_PRDT_REQ_QTY"]));
            txtPreSectionQty.Value = Double.Parse(Util.NVC(_drSelectRow["PRE_SECTION_QTY"]));
            txtAfterSectionQty.Value = Double.Parse(Util.NVC(_drSelectRow["AFTER_SECTION_QTY"]));

            //// 작업 타입 
            //if (string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["IRREGL_PROD_LOT_TYPE_CODE"])))
            //    cboIrreglProdLotType.SelectedIndex = 0;
            //else
            //    cboIrreglProdLotType.SelectedValue = Util.NVC(_drSelectRow["IRREGL_PROD_LOT_TYPE_CODE"]);
            txtIrreglProdLotType.Text = Util.NVC(_drSelectRow["IRREGL_PROD_LOT_TYPE_NAME"]);

            txtCurrProcLossQty.Value = Double.Parse(Util.NVC(_drSelectRow["CURR_PROC_LOSS_QTY"]));
            txtReInputQty.Value = Double.Parse(Util.NVC(_drSelectRow["RE_INPUT_QTY"]));
            txtNote.Text = GetWipNote();

            CalculateQty();
            DisplayCellManagementType(dgLotList);

            if (Util.NVC(_drSelectRow["WIPSTAT"]).Equals("EQPT_END"))
            {
                btnWorkorder.Visibility = Visibility.Visible;
            }
            else
            {
                btnWorkorder.Visibility = Visibility.Collapsed;
            }

            if (Util.NVC(_drSelectRow["WIPHOLD"]).Equals("Y"))
            {
                btnSave.IsEnabled = false;
            }
            else
            {
                btnSave.IsEnabled = true;
            }

            SetNumericBoxValueChangedEvent(true);

            if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.NOTCHING))
                txtOutQty.BorderBrush = new SolidColorBrush(Colors.Red);
            else
                txtOutQty.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC3C3C3"));
        }
        #endregion

        #region # 저장 Biz DataSet 생성
        private DataSet GetSaveDataSet()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            //inDataTable.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataTable.Columns.Add("WIPSEQ", typeof(int)); // 2024.11.13. 김영국 - WIP_SEQ 값 String으로 변경.,
            inDataTable.Columns.Add("CALDATE", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WIPQTY_ED", typeof(Decimal));
            inDataTable.Columns.Add("WIPQTY2_ED", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("REQ_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CHANGE_WIPQTY_FLAG", typeof(string));
            inDataTable.Columns.Add("FORCE_FLAG", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("INPUT_QTY", typeof(Decimal));
            inDataTable.Columns.Add("RE_INPUT_QTY", typeof(Decimal));
            inDataTable.Columns.Add("PRE_SECTION_QTY", typeof(Decimal));
            inDataTable.Columns.Add("AFTER_SECTION_QTY", typeof(string));
            inDataTable.Columns.Add("PROD_LOT_YN", typeof(string));
            inDataTable.Columns.Add("CURR_PROC_LOSS_QTY", typeof(Decimal));

            // IN_LOSS 
            DataTable inDataLoss = indataSet.Tables.Add("IN_LOSS");
            inDataLoss.Columns.Add("LOTID", typeof(string));
            //inDataLoss.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataLoss.Columns.Add("WIPSEQ", typeof(int)); // 2024.11.13. 김영국 - WIP_SEQ 값 String으로 변경.,
            inDataLoss.Columns.Add("RESNCODE", typeof(string));
            inDataLoss.Columns.Add("RESNQTY", typeof(Decimal));
            inDataLoss.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataLoss.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataLoss.Columns.Add("RESNNOTE", typeof(string));
            inDataLoss.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));
            //inDataLoss.Columns.Add("DFCT_TAG_QTY", typeof(Decimal));
            inDataLoss.Columns.Add("DFCT_TAG_QTY", typeof(int));
            // IN_DFCT
            DataTable inDataDfct = indataSet.Tables.Add("IN_DFCT");
            inDataDfct.Columns.Add("LOTID", typeof(string));
            //inDataDfct.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataDfct.Columns.Add("WIPSEQ", typeof(int)); // 2024.11.13. 김영국 - WIP_SEQ 값 String으로 변경.,
            inDataDfct.Columns.Add("RESNCODE", typeof(string));
            inDataDfct.Columns.Add("RESNQTY", typeof(Decimal));
            inDataDfct.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataDfct.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataDfct.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataDfct.Columns.Add("RESNNOTE", typeof(string));
            //inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(Decimal));
            inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(int));
            inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(Decimal));
            inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(Decimal));
            inDataDfct.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));

            // IN_PRDT_REQ
            DataTable inDataPrdtReq = indataSet.Tables.Add("IN_PRDT_REQ");
            inDataPrdtReq.Columns.Add("LOTID", typeof(string));
            //inDataPrdtReq.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataPrdtReq.Columns.Add("WIPSEQ", typeof(int)); // 2024.11.13. 김영국 - WIP_SEQ 값 String으로 변경.,
            inDataPrdtReq.Columns.Add("RESNCODE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataPrdtReq.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNNOTE", typeof(string));
            inDataPrdtReq.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataPrdtReq.Columns.Add("DFCT_QTY_DDT_RATE", typeof(Decimal));
            //inDataPrdtReq.Columns.Add("DFCT_TAG_QTY", typeof(Decimal));
            inDataPrdtReq.Columns.Add("DFCT_TAG_QTY", typeof(int));
            return indataSet;
        }

        #endregion

        #region # 특이사항 Split
        private string GetWipNote()
        {
            string sReturn;
            string[] sWipNote = Util.NVC(_drSelectRow["WIP_NOTE"]).Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = Util.NVC(_drSelectRow["WIP_NOTE"]);
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }

        private string SetWipNote()
        {
            string sReturn;
            string[] sWipNote = Util.NVC(_drSelectRow["WIP_NOTE"]).Split('|');

            sReturn = txtNote.Text + "|";

            for (int nlen = 1; nlen < sWipNote.Length; nlen++)
            {
                sReturn += sWipNote[nlen] + "|";
            }

            return sReturn.Substring(0, sReturn.Length - 1);
        }

        #endregion

        #region # 불량 합산
        private void SumDefectQty()
        {
            try
            {
                DataTable dtDefect = DataTableConverter.Convert(dgDefect.ItemsSource);

                double dDefect = 0;
                double dLoss = 0;
                double dChargeProd = 0;

                foreach (DataRow dr in dtDefect.Rows)
                {
                    if (dr["RSLT_EXCL_FLAG"].Equals("N") && !dr["PRCS_ITEM_CODE"].Equals("UNIDENTIFIED_QTY"))
                    {
                        if (dr["ACTID"].Equals("DEFECT_LOT"))
                            dDefect += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else if (dr["ACTID"].Equals("LOSS_LOT"))
                            dLoss += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else
                            dChargeProd += double.Parse(Util.NVC(dr["RESNQTY"]));
                    }
                }

                txtDefectQty.Value = dDefect;
                txtLossQty.Value = dLoss;
                txtPrdtReqQty.Value = dChargeProd;

                CalculateQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalculateQty()
        {
            try
            {
                // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량(전+후) + 재투입수
                ////double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - txtDefectQty.Value - txtLossQty.Value - txtPrdtReqQty.Value - (txtPreSectionQty.Value + txtAfterSectionQty.Value) + txtReInputQty.Value;
                //double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - txtDefectQty.Value - txtLossQty.Value - txtPrdtReqQty.Value + txtReInputQty.Value;

                double dInputQty;
                double dOutQty;
                double dDefectQty;
                double dLossQty;
                double dPrdtReqQty;
                double dReInputQty;

                // Null인 경우 "NaN" 또는 "非数字"로 발생
                if (txtInputQty.Value.ToString() == double.NaN.ToString())
                    dInputQty = 0;
                else
                    dInputQty = txtInputQty.Value;

                if (txtOutQty.Value.ToString() == double.NaN.ToString())
                    dOutQty = 0;
                else
                    dOutQty = txtOutQty.Value;

                if (txtDefectQty.Value.ToString() == double.NaN.ToString())
                    dDefectQty = 0;
                else
                    dDefectQty = txtDefectQty.Value;

                if (txtLossQty.Value.ToString() == double.NaN.ToString())
                    dLossQty = 0;
                else
                    dLossQty = txtLossQty.Value;

                if (txtPrdtReqQty.Value.ToString() == double.NaN.ToString())
                    dPrdtReqQty = 0;
                else
                    dPrdtReqQty = txtPrdtReqQty.Value;

                if (txtReInputQty.Value.ToString() == double.NaN.ToString())
                    dReInputQty = 0;
                else
                    dReInputQty = txtReInputQty.Value;

                //// 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) + 재투입수
                //double dUnidentifiedQty = dInputQty - dOutQty - dDefectQty - dLossQty - dPrdtReqQty + dReInputQty;
                // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외)
                double dUnidentifiedQty = dInputQty - dOutQty - dDefectQty - dLossQty - dPrdtReqQty;

                txtUnidentifiedQty.Value = dUnidentifiedQty;

                // 불량/Loss/물품청구 그리드에 미확인 Loss Update
                int idx = _util.GetDataGridRowIndex(dgDefect, "PRCS_ITEM_CODE", "UNIDENTIFIED_QTY");
                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgDefect.Rows[idx].DataItem, "RESNQTY", dUnidentifiedQty);
                    dgDefect.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성LOT 탭의 Tray 삭제 IsEnabled
        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutDel.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutDel.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN")) // 활성화입고
                    {
                        btnOutDel.IsEnabled = true;
                    }
                    else
                    {
                        btnOutDel.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutDel.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성LOT 탭의 CELL_MNGT_TYPE_CODE Display 
        private void DisplayCellManagementType(C1DataGrid dg)
        {
            if (!CommonVerify.HasDataGridRow(dg) || !CommonVerify.HasTableRow(_dtCellManagement))
            {
                tbCellManagement.Text = string.Empty;
            }
            else
            {
                string cellType = string.Empty;

                var query = (from t in _dtCellManagement.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == Util.NVC(_drSelectRow["CELL_MNGT_TYPE_CODE"])
                             select new { cellType = t.Field<string>("CBO_NAME") }).FirstOrDefault();

                if (query != null)
                    cellType = query.cellType;

                tbCellManagement.Text = "[" + _cellManageGroup + "  : " + cellType + "]";
            }
        }

        #endregion

        #region # 수량 변경 TextBox Event(ValueChanged) 설정,해제
        private void SetNumericBoxValueChangedEvent(bool isSet)
        {
            if (isSet)
            {
                txtInputQty.ValueChanged += txtQty_ValueChanged;
                txtOutQty.ValueChanged += txtQty_ValueChanged;
                txtPreSectionQty.ValueChanged += txtQty_ValueChanged;
                txtAfterSectionQty.ValueChanged += txtQty_ValueChanged;
                txtReInputQty.ValueChanged += txtQty_ValueChanged;
            }
            else
            {
                txtInputQty.ValueChanged -= txtQty_ValueChanged;
                txtOutQty.ValueChanged -= txtQty_ValueChanged;
                txtPreSectionQty.ValueChanged -= txtQty_ValueChanged;
                txtAfterSectionQty.ValueChanged -= txtQty_ValueChanged;
                txtReInputQty.ValueChanged -= txtQty_ValueChanged;
            }
        }
        #endregion

        #endregion

        #endregion


        #region Editable remarks column for dgSublot and button "Additional Lot Remark"
        /// <summary>
        /// Action on click "Additional Lot Remark" button
        /// </summary>
        /// <param/>
        /// <returns/>
        private void btnAddLotRemark_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveCompletedLotRemark();
                }
            });
        }

        /// <summary>
        /// When confirm to update remark, run this function
        /// </summary>
        /// <param/>
        /// <returns/>
        private void SaveCompletedLotRemark()
        {

            try
            {
                ShowLoadingIndicator();

                // check if main LOT ID selected
                if (_drSelectRow == null)
                {
                    Util.MessageInfo("SFU1381");
                    return;
                }

                DataSet dtIndataSet = new DataSet();

                DataTable dtInDataTable = dtIndataSet.Tables.Add("IN_EQP");
                dtInDataTable.Columns.Add("SRCTYPE", typeof(string));
                dtInDataTable.Columns.Add("IFMODE", typeof(string));
                dtInDataTable.Columns.Add("EQPTID", typeof(string));
                dtInDataTable.Columns.Add("PROD_LOTID", typeof(string));
                dtInDataTable.Columns.Add("USERID", typeof(string));

                DataTable dtInLot = dtIndataSet.Tables.Add("IN_LOT");
                dtInLot.Columns.Add("OUT_LOTID", typeof(string));
                dtInLot.Columns.Add("CSTID", typeof(string));
                dtInLot.Columns.Add("WIPQTY", typeof(int));
                dtInLot.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                dtInLot.Columns.Add("SPCL_CST_NOTE", typeof(string));
                dtInLot.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                DataTable dtInTable = dtIndataSet.Tables["IN_EQP"];
                DataRow drNewRow = dtInTable.NewRow();
                drNewRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drNewRow["IFMODE"] = IFMODE.IFMODE_OFF;
                drNewRow["EQPTID"] = _drSelectRow["EQPTID"].GetString();
                drNewRow["PROD_LOTID"] = _drSelectRow["LOTID"].GetString();
                drNewRow["USERID"] = LoginInfo.USERID;
                dtInTable.Rows.Add(drNewRow);
                drNewRow = null;

                for (int iIdx = 0; iIdx < dgSubLot.Rows.Count - dgSubLot.BottomRows.Count; iIdx++)
                {
                    // 2023.11.13 강성묵 추가 수정 체크박스 체크상태일 경우만 데이터 저장
                    if (_util.GetDataGridCheckValue(dgSubLot, "CHK", iIdx) == false)
                    {
                        continue;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iIdx].DataItem, "PRE_REMARK")) == Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iIdx].DataItem, "REMARK")))
                    {
                        continue;
                    }

                    drNewRow = dtInLot.NewRow();
                    drNewRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iIdx].DataItem, "LOTID"));
                    drNewRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iIdx].DataItem, "CSTID"));
                    drNewRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgSubLot.Rows[iIdx].DataItem, "REMARK"));

                    dtInLot.Rows.Add(drNewRow);
                }

                if (dtInLot.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1566");
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_SPCL", "IN_EQP,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }


                        Util.MessageInfo("SFU1275");

                        // re-get sublot data
                        GetSubLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, dtIndataSet);


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //ESMI 1동(A4) 6 Line 만 조회
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetOutLotRemarkUse(string sProcId)
        {
            btnAddLotRemark.Visibility = Visibility.Collapsed;
            dgSubLot.Columns["colAZSRemark"].Visibility = Visibility.Collapsed;

            DataTable dtInTable = new DataTable("RQSTDT");
            dtInTable.Columns.Add("AREAID", typeof(string));
            dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtInTable.Columns.Add("COM_CODE", typeof(string));

            DataRow drNewRow = dtInTable.NewRow();
            drNewRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            drNewRow["COM_TYPE_CODE"] = "OUTLOT_REMARK_COL_USE";
            drNewRow["COM_CODE"] = sProcId;
            dtInTable.Rows.Add(drNewRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtInTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                btnAddLotRemark.Visibility = Visibility.Visible;
                dgSubLot.Columns["colAZSRemark"].Visibility = Visibility.Visible;
            }
        }
        #region [E20240704-001494][CAT_UP_0542]
        private DataTable ApprReqChk()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPR_REQ", "RQSTDT", "RSLTDT", inTable);

            return dtResult;
        }
        #endregion [E20240704-001494][CAT_UP_0542]
        #endregion
    }
}
