/*************************************************************************************
 Created Date : 2022.02.08
      Creator : 신광희
   Decription : Washing 공정진척 Cell 관리 팝업(CMM_WASHING_CELL_MANAGEMENT 참조)
--------------------------------------------------------------------------------------
 [Change History]
   2022.02.08   신광희 : Initial Created.
   2023.06.03   배현우 :  ESNJ 9동 CELL 위치 조회 변경 및 모따기 위치 색지정
   2023.12.05   배현우 : 캔등록 팝업 및 CAN, VENT ID 매칭검사 기능 추가
   2023.12.28   배현우 : NFF 양산라인 CELL 위치 조회 변경 추가
   2024.05.20   배현우 : 오창 2산단 NFF 양산라인의 경우 엑셀 다운로드시 CANID, VENTID 컬럼추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Configuration;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// ASSY006_WASHING_CELL_MANAGEMENT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_072_WASHING_CELL_MANAGEMENT : C1Window, IWorkArea
    {
        private readonly Util _util = new Util();

        // DataSet, DataTable
        private DataTable _dtBaseCell = null;           // 그리드 Cell Data
        private DataTable _dtSubLotId = null;           // SUBLOTID 정보        
        private DataTable _dtTrayInfo = null;           // dsResult 에 담긴 OUTDATA
        private DataTable _dtSubLotInfo = null;         // dsResult 에 담긴 CST_PSTN
        private DataTable _dtTrayLocation = null;       // TrayLocation 정보
        private DataTable _dtSubLotCheckCode = null;    // SUBLOT_CHK_CODE 정보
        private DataTable _dtCellIdCheckCode = null;    // Cell ID 매칭검사 결과 정보

        // Main에서 넘겨받은 파라미터
        private string _prodLotId = string.Empty;       // LOTID
        private string _trayId = string.Empty;          // TRAYID
        private string _outLotId = string.Empty;        // OUTLOTID
        private string _equipmentId = string.Empty;     // 설비코드
        private string _workerid = string.Empty;        //작업자 ID

        // 전역 변수
        private string _cellId = string.Empty;                // CELLID 텍스트박스에 수정되기전 값
        private string _lotId = string.Empty;                 // 원래 데이터에서 StartIndex : 4, EndIndex : 10 자른 7자리 LOTID
        private string _subCellId = string.Empty;             // 원래 데이터에서 StartIndex : 3, EndIndex : 9 자른 7자리 CELLID
        private string _selectedTrayLocation = string.Empty;  // 선택한 Tray 위치
        private string _trayTag = string.Empty;               // R: 읽기 / W: 쓰기 모드
        private string _completeProd = string.Empty;          // 생산LOT 완료여부
        private bool bSave = false;
        private int _cellLength = 12;                         // Cell ID 자릿수
        private string _canId = string.Empty;                 // CANID
        private string _trayId_f = string.Empty;              // 먼저 생성된 Tray
        private string _trayId_b = string.Empty;              // 후에 생성된 Tray

        // 그리드 Cell 갯수 : 8 by 8
        private int _xAxis;
        private int _yAxis;

        private int sRow = -1; //CELL 검색 기능
        private int sCol = -1; //CELL 검색 기능
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_072_WASHING_CELL_MANAGEMENT()
        {
            InitializeComponent();
        }

        private void COM001_072_WASHING_CELL_MANAGEMENT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
        
            _prodLotId = parameters[0] as string;
            _trayId = parameters[1] as string;
            _outLotId = parameters[2] as string;
            _equipmentId = parameters[3] as string;
            _trayTag = parameters[4] as string;
            _completeProd = parameters[5] as string;
            _workerid = parameters[6] as string;
            _trayId_b = parameters[7] as string;
            _trayId_f = parameters[8] as string;

            //읽기 모드
            if (_trayTag != null && _trayTag.Equals("R"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Cell조회");
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnCell.Visibility = Visibility.Collapsed;
                btnCan.Visibility = Visibility.Collapsed;
                btnCopy.Visibility = Visibility.Collapsed;
                cboTagetTray.Visibility = Visibility.Collapsed;
                fontA.Visibility = Visibility.Collapsed;
                fBlue.Visibility = Visibility.Collapsed;
                fontB.Visibility = Visibility.Collapsed;
                FRed.Visibility = Visibility.Collapsed;
            }
            else //쓰기모드
            {
                this.Header = ObjectDic.Instance.GetObjectName("Cell관리");
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
                btnCell.Visibility = Visibility.Visible;
                btnCan.Visibility = Visibility.Visible;
                //TRAY 복사기능 오창에만 적용되도록 분기처리 (A010)
                if (!string.Equals(LoginInfo.CFG_SHOP_ID, "A010"))
                {
                    btnCopy.Visibility = Visibility.Collapsed;
                    cboTagetTray.Visibility = Visibility.Collapsed;
                    fontA.Visibility = Visibility.Collapsed;
                    fBlue.Visibility = Visibility.Collapsed;
                    fontB.Visibility = Visibility.Collapsed;
                    FRed.Visibility = Visibility.Collapsed;
                }
            }
            
            if (LoginInfo.CFG_AREA_ID.Equals("MB"))
            {
                _xAxis = 16;
                _yAxis = 16;
                btnCan.Visibility = Visibility.Collapsed;
                txtCellSearch.Visibility = Visibility.Collapsed;
                tbCellSearch.Visibility = Visibility.Collapsed;


            }
            else
            {
                if (LoginInfo.CFG_AREA_ID.Equals("MC"))
                {
                    btnCan.Visibility = Visibility.Collapsed;
                    btnCanVent.Visibility = Visibility.Visible;
                    tbVentId.Visibility = Visibility.Visible;
                    txtVentId.Visibility = Visibility.Visible;
                    btnCell.Visibility = Visibility.Collapsed;
                    txtCellSearch.Visibility = Visibility.Visible;
                    tbCellSearch.Visibility = Visibility.Visible;
                    txtCellId.IsReadOnly = true;
                    _xAxis = 12;
                    _yAxis = 12;
                }
                else
                {
                    btnCanVent.Visibility = Visibility.Collapsed;
                    txtVentId.Visibility = Visibility.Collapsed;
                    tbVentId.Visibility = Visibility.Collapsed;
                    txtCellSearch.Visibility = Visibility.Collapsed;
                    tbCellSearch.Visibility = Visibility.Collapsed;

                    _xAxis = 8;
                    _yAxis = 8;
                }

                
            }

            SetCellInfo();
            InitControl();
        }

        private void InitControl()
        {
            // 컨트롤 초기화
            txtLotId.Text = string.Empty;         // LOTID
            txtTrayId.Text = string.Empty;        // Cell수량
            txtWipQty.Text = string.Empty;        // Cell수량
            txtCellId.Text = string.Empty;        // CELLID
            txtTrayLocation.Text = string.Empty;  // Tray 위치
            txtCanId.Text = string.Empty;
            txtVentId.Text = string.Empty;
            sRow = -1; //검색된 Cell 위치 초기화
            sCol = -1; 
            //if (_dtSubLotInfo == null || _dtSubLotInfo.Rows.Count <= 0)
            if(!CommonVerify.HasTableRow(_dtSubLotInfo))
            {
                /*
                iCount = 1;
                dtTrayLocationClone = dtCell.Copy();

                for (int row = 1; row < dtCell.Rows.Count; row++)
                {
                    for (int col = 1; col < dtCell.Columns.Count; col++)
                    {
                        dtTrayLocationClone.Rows[col][row.ToString()] = iCount++;
                    }
                }
                */
            }

            // 텍스트박스 바인딩
            if (_dtTrayInfo != null)
            {
                //int sLotIDLength = 0;
                txtLotId.Text = Util.NVC(_dtTrayInfo.Rows[0]["PROD_LOTID"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["PROD_LOTID"]) : string.Empty;
                _lotId = Util.NVC(txtLotId.Text) != string.Empty ? Util.NVC(txtLotId.Text) : string.Empty;
                _lotId = _lotId != string.Empty ? _lotId.Substring(3, 7) : string.Empty;
                txtTrayId.Text = Util.NVC(_dtTrayInfo.Rows[0]["TRAYID"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["TRAYID"]) : string.Empty;
                txtWipQty.Text = Util.NVC(_dtTrayInfo.Rows[0]["WIPQTY"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["WIPQTY"]) : string.Empty;
            }

            // Cell수량 뒤에 소수점 제거와 숫자 표현 형식 (1,000 : 3자리마다 , 출력)
            int iWipQty = Util.NVC_Int(txtWipQty.Text);
            string wipQty = iWipQty != 0 ? $"{iWipQty:#,###}" : string.Empty;
            txtWipQty.Text = Util.NVC(wipQty);

            // CELLID
            _cellId = txtCellId.Text.Trim();
            SetComboBox(cboTagetTray, CommonCombo.ComboStatus.NONE);
        }

        private void SetComboBox(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROD_LOTID", typeof(string));
                RQSTDT.Columns.Add("TRAYID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROD_LOTID"] = Util.NVC(_dtTrayInfo.Rows[0]["PROD_LOTID"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["PROD_LOTID"]) : string.Empty;
                dr["TRAYID"] = Util.NVC(_dtTrayInfo.Rows[0]["TRAYID"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["TRAYID"]) : string.Empty;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_BY_PRDLOTID", "RQSTDT", "RSLTDT", RQSTDT);


                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                cbo.DisplayMemberPath = "CSTID";
                cbo.SelectedValuePath = "LOTID";
                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //SetGridCell(false);
            SetCellInfo();
            InitControl();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = bSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            //Cell 입력
            try
            {
                bSave = true;
                COM001_072_WASHING_INPUT_CELL popInputCell = new COM001_072_WASHING_INPUT_CELL { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = txtLotId.Text;
                parameters[1] = txtTrayId.Text;
                parameters[2] = _equipmentId;
                parameters[3] = _outLotId;
                parameters[4] = string.Empty;
                parameters[5] = _completeProd;
                C1WindowExtension.SetParameters(popInputCell, parameters);

                popInputCell.Closed += new EventHandler(popInputCell_Closed);
                popInputCell.ShowModal();
                popInputCell.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCan_Click(object sender, RoutedEventArgs e)
        {
            //Cell 입력
            try
            {
                bSave = true;
                COM001_072_WASHING_INPUT_CAN popInputCell = new COM001_072_WASHING_INPUT_CAN { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = txtLotId.Text;
                parameters[1] = txtTrayId.Text;
                parameters[2] = _equipmentId;
                parameters[3] = _outLotId;
                parameters[4] = string.Empty;
                parameters[5] = _completeProd;
                C1WindowExtension.SetParameters(popInputCell, parameters);

                popInputCell.Closed += new EventHandler(popInputCan_Closed);
                popInputCell.ShowModal();
                popInputCell.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCanVent_Click(object sender, RoutedEventArgs e)
        {
            //Cell 입력
            try
            {
                bSave = true;
                COM001_072_WASHING_INPUT_CAN_VENT popInputCell = new COM001_072_WASHING_INPUT_CAN_VENT { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = txtLotId.Text;
                parameters[1] = txtTrayId.Text;
                parameters[2] = _equipmentId;
                parameters[3] = _outLotId;
                parameters[4] = string.Empty;
                C1WindowExtension.SetParameters(popInputCell, parameters);

                popInputCell.Closed += new EventHandler(popInputCanVent_Closed);
                popInputCell.ShowModal();
                popInputCell.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 저장 버튼 클릭 시 삭제 BizRule 실행 후 저장 BizRule 실행
            if (!ValidationSave())
                return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    bSave = true;
                    Delete(false); // Cell 삭제
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDelete())
                return;

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    bSave = true;
                    Delete(true); // Cell 삭제
                }
            });
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidateCopy())
                return;

            // 복사 버튼 클릭 시 복사 BizRule 실행
            Util.MessageConfirm("SFU8025", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    bSave = true;
                    CellCopy(); // Cell 삭제
                }
            }, cboTagetTray.Text);



        }

        private void popInputCell_Closed(object sender, EventArgs e)
        {
            COM001_072_WASHING_INPUT_CELL popup = sender as COM001_072_WASHING_INPUT_CELL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //SetGridCell(false);
                SetCellInfo();
                InitControl();
            }

        }

        private void popInputCan_Closed(object sender, EventArgs e)
        {
            COM001_072_WASHING_INPUT_CAN popup = sender as COM001_072_WASHING_INPUT_CAN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //SetGridCell(false);
                SetCellInfo();
                InitControl();
            }

        }

        private void popInputCanVent_Closed(object sender, EventArgs e)
        {
            COM001_072_WASHING_INPUT_CAN_VENT popup = sender as COM001_072_WASHING_INPUT_CAN_VENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //SetGridCell(false);
                SetCellInfo();
                InitControl();
            }

        }
        private void popMatchCell_Closed(object sender, EventArgs e)
        {

            COM001_072_WASHING_CELL_MATCH popup = sender as COM001_072_WASHING_CELL_MATCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //SetGridCell(false);
                SetCellInfo();
                InitControl();
            }

        }
        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (_dtBaseCell != null)
                    {

                        // 가로 Header 색상 적용
                        if (e.Cell.Row.Index == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE"));
                        }
                        // 세로 Header 색상 적용
                        if (e.Cell.Column.Index == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE"));
                        }

                        if ((LoginInfo.CFG_AREA_ID == "MB" || LoginInfo.CFG_AREA_ID == "MC") && e.Cell.Row.Index == 0 && e.Cell.Column.Index == 0)   // ESNJ 9동의 경우 TRAY 모따기 모양 표시  2023-06-03 배현우
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        }
                        if (_dtSubLotCheckCode != null)
                        {
                            for (int row = 1; row < _dtSubLotCheckCode.Rows.Count; row++)
                            {
                                for (int col = 1; col < _dtSubLotCheckCode.Columns.Count; col++)
                                {
                                    if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.IsNullOrEmpty(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()])))
                                    {
                                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE");
                                        if (convertFromString != null)
                                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                    }
                                }
                            }

                            for (int row = 1; row < _dtSubLotCheckCode.Rows.Count; row++)
                            {
                                for (int col = 1; col < _dtSubLotCheckCode.Columns.Count; col++)
                                {
                                    // NR : 읽을 수 없음(No Read)
                                    if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdNR.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdNR.Background;
                                    }
                                    // DL : 자리수 상이 (Different ID Length) 
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdDL.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdDL.Background;
                                    }
                                    // ID : ID 중복 (ID Duplication)
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdID.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdID.Background;
                                    }
                                    // SC : 특수문자 포함 (Include Special Character) 
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdSC.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdSC.Background;
                                    }
                                    // PD : Tray Location 중복 (Position Duplication)
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdPD.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdPD.Background;
                                    }
                                    // NI : 주액량 정보 없음 (No Information) -> Empty로 변경 예정
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtSubLotCheckCode.Rows[row][col.ToString()]), bdNI.Tag))
                                    {
                                        e.Cell.Presenter.Background = bdNI.Background;
                                    }
                                    //else
                                    //{
                                    //    e.Cell.Presenter.Background = bdNI.Background;
                                    //}
                                }
                            }

                        }
                        // TODO
                        if (_dtCellIdCheckCode != null)
                        {
                            for (int row = 1; row < _dtCellIdCheckCode.Rows.Count; row++)   
                            {
                                for (int col = 1; col < _dtCellIdCheckCode.Columns.Count; col++)
                                {
                                    if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.IsNullOrEmpty(Util.NVC(_dtCellIdCheckCode.Rows[row][col.ToString()])))
                                    {
                                        //    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE");
                                        //   if (convertFromString != null)
                                        //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                    }
                                }
                            }
                            for (int row = 1; row < _dtCellIdCheckCode.Rows.Count; row++)
                            {
                                for (int col = 1; col < _dtCellIdCheckCode.Columns.Count; col++)
                                {
                                    // cell 매칭 검사 OK
                                    if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtCellIdCheckCode.Rows[row][col.ToString()]), fontA.Tag))
                                    {
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        e.Cell.Presenter.Foreground = fontA.Background;
                                    }
                                    // cell 매칭 검사 NG
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtCellIdCheckCode.Rows[row][col.ToString()]), fontB.Tag))
                                    {
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        e.Cell.Presenter.Foreground = fontB.Background;
                                    }
                                    // cell 매칭 검사 : 미검사
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(_dtCellIdCheckCode.Rows[row][col.ToString()]), "N"))
                                    {
                                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                        //e.Cell.Presenter.Foreground = fontB.Background;
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                }
                            }
                        }
                    }
                }
            }));

        }

        private void dgCellInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgCellInfo.CurrentRow == null || dgCellInfo.CurrentColumn == null) return;

                // 선택한 셀의 위치
                int rowIdx = dgCellInfo.CurrentRow.Index;
                int colIdx = dgCellInfo.CurrentColumn.Index;
                
                txtTrayLocation.Text = string.Empty;
                txtCellId.Text = string.Empty;
                txtCanId.Text = string.Empty;
                txtVentId.Text = string.Empty;

                if (rowIdx == 0 || colIdx == 0) return;

                if (_dtSubLotId == null)
                {
                    string selectedTrayLocation = Util.NVC(_dtTrayLocation.Rows[rowIdx][colIdx.ToString()]); // 메인에서 수량 0인 항목을 선택했을 경우 Tray 위치 정보
                    txtTrayLocation.Text = Util.NVC(selectedTrayLocation);
                    _cellId = txtCellId.Text.Trim();
                    _selectedTrayLocation = txtTrayLocation.Text.Trim();
                }
                else
                {
                    string selectedSubLot = Util.NVC(_dtSubLotId.Rows[rowIdx][colIdx.ToString()]); // 선택한 Cell의 SUBLOTID
                    txtTrayLocation.Text = Util.NVC(_dtTrayLocation.Rows[rowIdx][colIdx.ToString()]); // Tray위치
                    if (CommonVerify.HasTableRow(_dtSubLotInfo) && CommonVerify.HasTableRow(_dtTrayInfo))
                    {
                        var query = (from t in _dtSubLotInfo.AsEnumerable()
                                     where t.Field<string>("SUBLOTID") == selectedSubLot
                                     select new
                                     {
                                         CarrierSlot = t.Field<Int32>("CSTSLOT").GetString(),    // Carrier 슬롯번호
                                         SubLotId = t.Field<string>("SUBLOTID"),                 // CELLID
                                         CanId = t.Field<string>("CANID"), 
                                         VentId = t.Field<string>("VENTID"),// CANID
                                     }).FirstOrDefault();

                        if (query != null)
                        {
                            _subCellId = query.SubLotId ?? string.Empty;
                            txtCellId.Text = query.SubLotId ?? string.Empty;         // CELLID
                            txtCanId.Text = query.CanId ?? string.Empty;
                            txtVentId.Text = query.VentId ?? string.Empty;
                            DataRow[] dr = null;
                            if (_dtSubLotInfo.Columns.Contains("CSTSLOT"))
                            {
                                dr = _dtSubLotInfo.Select("CSTSLOT ='" + query.CarrierSlot + "'");
                            }
                            if (dr != null && dr.Length > 1)
                            {
                                CMM_ASSY_DUPCELLDELETE popDupcelldelete = new CMM_ASSY_DUPCELLDELETE { FrameOperation = FrameOperation };

                                object[] parameters = new object[7];
                                parameters[0] = dr;             //중복된 Cell ID
                                parameters[1] = _equipmentId;   //설비정보
                                parameters[2] = txtLotId.Text;  //생산LOT ID
                                parameters[3] = _outLotId;      //완성LOT ID
                                parameters[4] = txtTrayId.Text; //Tray ID
                                parameters[5] = _trayTag;       //읽기 : R, 쓰기 : W
                                parameters[6] = _completeProd;  //생산LOT 완료 여부
                                C1WindowExtension.SetParameters(popDupcelldelete, parameters);

                                popDupcelldelete.Closed += new EventHandler(DupCellLoad_Closed);
                                popDupcelldelete.ShowModal();
                                popDupcelldelete.CenterOnScreen();
                            }
                        }
                        else
                        {
                            txtCellId.Text = string.Empty; // CELLID
                            _subCellId = string.Empty;
                        }
                    }
                    _cellId = txtCellId.Text.Trim();
                    _selectedTrayLocation = txtTrayLocation.Text.Trim();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayLocation_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _selectedTrayLocation = txtTrayLocation.Text;
                if (!Util.CheckDecimal(_selectedTrayLocation, 0)) // 입력된 데이터형 판단
                {
                    txtTrayLocation.Text = Util.NVC(_selectedTrayLocation);
                    return;
                }

                if (e.Key == Key.Back)
                    return;
                if (e.Key == Key.Delete)
                    return;

                //txtWipQty.Text = Util.NVC(txtChangeQty.Text) != string.Empty ? Util.NVC($"{Util.NVC_Int(txtWipQty.Text):#,###}") : Util.NVC(string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DupCellLoad_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DUPCELLDELETE popup = sender as CMM_ASSY_DUPCELLDELETE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
                //SetCellInfo();
            }

        }

        private bool ValidateDelete()
        {
            if (string.IsNullOrEmpty(Util.NVC(txtCellId.Text))) // 삭제할 항목이 없습니다.
            {
                Util.MessageValidation("SFU1597");
                return false;
            }
            if (txtTrayLocation.Text == "0")
            {
                //Util.MessageValidation("SFU3636");
                Util.MessageValidation("SFU3681"); //Tray 정보는 0 이상이어야 합니다.
                return false;
            }
            if (txtTrayLocation.Text.Length == 0)
            {
                //Tray 위치정보를 입력하세요
                Util.MessageValidation("SFU4278");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            if (txtLotId.Text.Length != 12)
            {
                //LOTID는 12자리여야 합니다.
                Util.MessageValidation("SFU3634");
                return false;
            }

            if (txtCellId.Text.Length != _cellLength)
            {
                //Util.Alert("CELLID는 XX자리여야 합니다."); 
                object[] parameters1 = new object[1];
                parameters1[0] = _cellLength.ToString();
                Util.MessageValidation("SFU8115", parameters1);
                return false;
            }

            /*
            if (_util.Left(txtCellId.Text.Trim(), iPrefix_Len) != txtPREFIX.Text)
            {
                object[] parameters = new object[3];
                parameters[0] = txtCellId.Text;
                parameters[1] = txtPREFIX.Text;
                parameters[2] = iPrefix_Len.ToString();
                Util.MessageValidation("SFU8112", parameters);  //앞 xx자리가 Prefix와 일치하지 않습니다. 
                return false;
            }
            */

            if (!CheckNumber(_util.Right(txtCellId.Text.Trim(), 6)))
            {
                Util.MessageValidation("SFU4916", txtCellId.Text.Trim()); //Cell ID는 뒤6자리만 입력 가능합니다. 
                return false;
            }

            if (txtTrayLocation.Text == "0")
            {
                //Util.MessageValidation("SFU3636");
                Util.MessageValidation("SFU3683"); //Tray 정보는 0 이상이어야 합니다.
                return false;
            }
            if (txtTrayLocation.Text.Length == 0)
            {
                Util.MessageValidation("SFU4278"); //Tray 위치정보를 입력하세요
                return false;
            }
            return true;
        }
        private bool ValidateCopy()
        {
            if (string.IsNullOrEmpty(Util.NVC(cboTagetTray.Text))) // 트레이가 없습니다.
            {
                Util.MessageValidation("SFU8105");
                return false;
            }
            return true;
        }

        private void Delete(bool deleteType)
        {
            try
            {
                if (dgCellInfo.Selection.SelectedCells.Count == 0)
                {
                    return;
                }
                ShowLoadingIndicator();
                if (_subCellId != string.Empty)
                {
                    int iDeletSubLotCount = 0;

                    string bizRuleName = _completeProd == "Y" ? "BR_PRD_REG_DELETE_SUBLOT_WS_CELLID_UI" : "BR_PRD_REG_DELETE_SUBLOT_WS_CELLID";
                    // DATA SET
                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("PROD_LOTID", typeof(string));
                    inData.Columns.Add("OUT_LOTID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataTable inCst = inDataSet.Tables.Add("IN_CST");
                    inCst.Columns.Add("CSTID", typeof(string));
                    inCst.Columns.Add("SUBLOTID", typeof(string));

                    DataRow newRow = inData.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(_equipmentId);
                    newRow["PROD_LOTID"] = Util.NVC(_prodLotId);
                    newRow["OUT_LOTID"] = Util.NVC(_outLotId);
                    newRow["USERID"] = LoginInfo.USERID;
                    inData.Rows.Add(newRow);

                    foreach (DataGridCell cell in dgCellInfo.Selection.SelectedCells)
                    {
                        if (cell.Presenter != null)
                        {
                            string selectedSubLot = Util.NVC(_dtSubLotId.Rows[cell.Row.Index][cell.Column.Index.ToString()]); // 선택한 Cell의 SUBLOTID
                            if (!selectedSubLot.Trim().Equals(""))
                            {
                                newRow = inCst.NewRow();
                                newRow["CSTID"] = Util.NVC(txtTrayId.Text);
                                newRow["SUBLOTID"] = Util.NVC(selectedSubLot);
                                inCst.Rows.Add(newRow);
                                iDeletSubLotCount++;
                            }
                        }
                    }
                    dgCellInfo.Selection.Clear();
                    if (iDeletSubLotCount == 0)
                    {
                        Util.MessageValidation("SFU1597");
                        return;
                    }

                    //string xml = inDataSet.GetXml();

                    new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_CST", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (deleteType)
                            {
                                SetCellInfo();
                                InitControl();
                            }
                            else
                            {
                                Save();
                                SetCellInfo();
                                InitControl();
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }
                else
                {
                    Save();
                    SetCellInfo();
                    InitControl();
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
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = _completeProd == "Y" ? "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS_CELLID_UI" : "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS_CELLID";

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inEqp = inDataSet.Tables.Add("IN_EQP");
                inEqp.Columns.Add("SRCTYPE", typeof(string));
                inEqp.Columns.Add("IFMODE", typeof(string));
                inEqp.Columns.Add("EQPTID", typeof(string));
                inEqp.Columns.Add("USERID", typeof(string));
                inEqp.Columns.Add("PROD_LOTID", typeof(string));
                inEqp.Columns.Add("EQPT_LOTID", typeof(string));
                inEqp.Columns.Add("OUT_LOTID", typeof(string));
                inEqp.Columns.Add("CSTID", typeof(string));

                DataTable inCst = inDataSet.Tables.Add("IN_CST");
                inCst.Columns.Add("SUBLOTID", typeof(string));
                inCst.Columns.Add("CSTSLOT", typeof(string));
                inCst.Columns.Add("CSTSLOT_F", typeof(string));

                DataRow newRow = inEqp.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentId);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(txtLotId.Text);
                newRow["EQPT_LOTID"] = string.Empty;
                newRow["OUT_LOTID"] = Util.NVC(_outLotId);
                newRow["CSTID"] = Util.NVC(txtTrayId.Text);
                inEqp.Rows.Add(newRow);

                newRow = inCst.NewRow();
                newRow["SUBLOTID"] = Util.NVC(txtCellId.Text);
                newRow["CSTSLOT"] = Util.NVC(txtTrayLocation.Text);
                newRow["CSTSLOT_F"] = string.Empty;
                inCst.Rows.Add(newRow);

                //string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", "OUT_CST", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            if (CommonVerify.HasTableInDataSet(bizResult))
                            {
                                DataTable dtResult = bizResult.Tables["OUT_CST"];
                                if (dtResult.Rows[0]["CHK_RESULT"].GetString() == "2")
                                {
                                    ControlsLibrary.MessageBox.Show(dtResult.Rows[0]["CHK_MESSAGE"].GetString(), "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                }
                                else
                                {
                                    Util.MessageInfo("SFU1889");
                                }
                            }
                            SetCellInfo();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
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

        //TRAY 복사기능 구현 20190522 조영빈
        private void CellCopy()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("TO_CSTID", typeof(string));
                inDataTable.Columns.Add("FROM_LOTID", typeof(string));
                inDataTable.Columns.Add("TO_LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["TO_CSTID"] = cboTagetTray.Text;
                newRow["FROM_LOTID"] = Util.NVC(_dtTrayInfo.Rows[0]["OUT_LOTID"]) != null ? Util.NVC(_dtTrayInfo.Rows[0]["OUT_LOTID"]) : string.Empty;
                newRow["TO_LOTID"] = cboTagetTray.SelectedValue;
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_COPY_TRAY", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);


                DialogResult = bSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PrepareGridCell()
        {
            // No.1 8 by 8 컬럼 생성
            DataTable dtBase = new DataTable();

            DataRow dr = dtBase.NewRow();

            int xAxis = 0;
            int yAxis = 0;

            for (int col = 0; col < _xAxis + 1; col++)
            {
                dtBase.Columns.Add(col.GetString());
                dr[col.GetString()] = xAxis++;
            }
            dtBase.Rows.Add(dr);

            for (int row = 0; row < _yAxis + 1; row++)
            {
                dtBase.Rows[row]["0"] = yAxis++;
                if (dtBase.Rows.Count == _yAxis + 1) break;
                dtBase.Rows.Add();
            }

            _dtBaseCell = dtBase.Copy();
        }

        private void SetCellInfo()
        {
            try
            {
                PrepareGridCell();

                int sum = _xAxis * _yAxis;

                _dtTrayLocation = _dtBaseCell.Copy();
                _dtSubLotId = _dtBaseCell.Copy();
                _dtSubLotCheckCode = _dtBaseCell.Copy();
                _dtCellIdCheckCode = _dtBaseCell.Copy();

                //_dtTrayLocation = new DataTable();
                //_dtTrayLocation = _dtBaseCell.Copy();

                //_dtSubLotId = new DataTable();
                //_dtSubLotId = _dtBaseCell.Copy();

                //_dtSubLotCheckCode = new DataTable();
                //_dtSubLotCheckCode = _dtBaseCell.Copy();

                //_dtCellIdCheckCode = new DataTable();
                //_dtCellIdCheckCode = _dtBaseCell.Copy();

                GetData();

                if (!CommonVerify.HasTableRow(_dtSubLotInfo))
                {
                    dgCellInfo.ItemsSource = DataTableConverter.Convert(_dtBaseCell);
                }
                else
                {
                    //var querySubLot = (from t in _dtSubLotInfo.AsEnumerable()
                    //                   orderby t.Field<Int32>("CSTSLOT") ascending
                    //                   select t).CopyToDataTable();

                    //int idx = 1;
                    //for (int i = 1; i < _xAxis + 1; i++)
                    //{
                    //    for (int j = 1; j < _yAxis + 1; j++)
                    //    {
                    //        _dtTrayLocation.Rows[i][j] = idx;
                    //        idx++;
                    //    }
                    //}

                    var querySubLotGroup = _dtSubLotInfo.AsEnumerable().GroupBy(g => g.Field<Int32>("CSTSLOT"))
                        .Select(s => s.OrderByDescending(o => o.Field<Int32>("CSTSLOT")).Last()).CopyToDataTable();

                    DataRow newRow = null;

                    if(!sum.Equals(0))
                    {
                        for(int k = 0; k < sum; k++)
                        {
                            var query = (from t in _dtSubLotInfo.AsEnumerable()
                                         where t.Field<Int32>("CSTSLOT") == k + 1
                                         select t).ToList();

                            if(!query.Any())
                            {
                                newRow = querySubLotGroup.NewRow();
                                newRow["CSTSLOT"] = k + 1;
                                newRow["SUBLOTID"] = string.Empty;
                                newRow["SUBLOT_CHK_CODE"] = string.Empty;
                                newRow["SCAN_RSLT_CODE"] = string.Empty;
                                querySubLotGroup.Rows.Add(newRow);
                            }
                        }
                        querySubLotGroup.AcceptChanges();

                        var querySubLot = (from t in querySubLotGroup.AsEnumerable()
                                           orderby t.Field<Int32>("CSTSLOT") ascending
                                           select t).CopyToDataTable();

                        int index = 0;
                        int subLotLength = 0;
                        int trayIndex = 1;
                        string subLotId = string.Empty;
                        string subLotCheckCode = string.Empty;
                        string cellCheckCode = string.Empty;

                        if (LoginInfo.CFG_AREA_ID.Equals("MB")) // ESNJ 9동 Cell 위치 수정 2023-06-03
                        {
                            for (int col = _dtBaseCell.Columns.Count-1 ; col > 0; col--)
                            {
                                for (int row = 1 ; row  < _dtBaseCell.Rows.Count; row++)
                                {
                                    subLotLength = querySubLot.Rows[index]["SUBLOTID"].ToString().Length != 0 ? querySubLot.Rows[index]["SUBLOTID"].ToString().Length - 1 : 0;
                                    subLotId = subLotLength > 0 ? Util.NVC(querySubLot.Rows[index]["SUBLOTID"]).Substring(subLotLength - 5) : string.Empty;
                                    _dtBaseCell.Rows[row][col] = subLotId;
                                    _dtSubLotId.Rows[row][col] = querySubLot.Rows[index]["SUBLOTID"];
                                    _dtTrayLocation.Rows[row][col] = trayIndex++;
                                    subLotCheckCode = Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                    _dtSubLotCheckCode.Rows[row][col] = subLotCheckCode;
                                    cellCheckCode = Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) : string.Empty;
                                    _dtCellIdCheckCode.Rows[row][col] = Util.NVC(cellCheckCode);

                                    index++;
                                }
                            }
                        }
                        else if (LoginInfo.CFG_AREA_ID.Equals("MC")) // NFF 양산 라인 Cell 위치 수정
                        {
                            for (int col = 1; col < _dtBaseCell.Columns.Count; col++)
                            {
                                for (int row = 1; row < _dtBaseCell.Rows.Count; row++)
                                {
                                    subLotLength = querySubLot.Rows[index]["SUBLOTID"].ToString().Length != 0 ? querySubLot.Rows[index]["SUBLOTID"].ToString().Length - 1 : 0;
                                    subLotId = subLotLength > 0 ? Util.NVC(querySubLot.Rows[index]["SUBLOTID"]).Substring(subLotLength - 5) : string.Empty;
                                    _dtBaseCell.Rows[row][col] = subLotId;
                                    _dtSubLotId.Rows[row][col] = querySubLot.Rows[index]["SUBLOTID"];
                                    _dtTrayLocation.Rows[row][col] = trayIndex++;
                                    subLotCheckCode = Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                    _dtSubLotCheckCode.Rows[row][col] = subLotCheckCode;
                                    cellCheckCode = Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) : string.Empty;
                                    _dtCellIdCheckCode.Rows[row][col] = Util.NVC(cellCheckCode);

                                    index++;
                                }
                            }
                        }
                        else
                        {
                            for (int row = 1; row < _dtBaseCell.Rows.Count; row++)
                            {
                                for (int col = 1; col < _dtBaseCell.Columns.Count; col++)
                                {
                                    subLotLength = querySubLot.Rows[index]["SUBLOTID"].ToString().Length != 0 ? querySubLot.Rows[index]["SUBLOTID"].ToString().Length - 1 : 0;
                                    subLotId = subLotLength > 0 ? Util.NVC(querySubLot.Rows[index]["SUBLOTID"]).Substring(subLotLength - 5) : string.Empty;
                                    _dtBaseCell.Rows[row][col] = subLotId;
                                    _dtSubLotId.Rows[row][col] = querySubLot.Rows[index]["SUBLOTID"];
                                    _dtTrayLocation.Rows[row][col] = trayIndex++;
                                    subLotCheckCode = Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                    _dtSubLotCheckCode.Rows[row][col] = subLotCheckCode;
                                    cellCheckCode = Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["SCAN_RSLT_CODE"]) : string.Empty;
                                    _dtCellIdCheckCode.Rows[row][col] = Util.NVC(cellCheckCode);

                                    index++;
                                }
                            }
                        }
                     }

                    dgCellInfo.ItemsSource = DataTableConverter.Convert(_dtBaseCell);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


            /*
            try
            {
                int sum = _xAxis * _yAxis;
                DataTable dtBaseTable = new DataTable();

                // C20190611_14694 소형 1동 해더 부분 출력값 설정 (16->1) 
                // 설정 불가능함 해더 명칭으로 데이터 검증하는 로직이 있어 임으로 변경이 불가능함

                DataTable dtCellHeader = new DataTable();
                DataTable dtCellRows = new DataTable();
                dtCellRows.Columns.Add("A");

                for (int row = 0; row < _yAxis; row++)
                {
                    dtCellRows.Rows.Add();
                }

                if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                {
                    for (int col = _xAxis; col > 0; col--)
                    {
                        dtCellHeader.Columns.Add(col.ToString());
                    }

                    for (int row = _yAxis-1; row >= 0; row--)
                    {
                        dtCellRows.Rows[row]["A"] = (_yAxis - row).ToString();
                    }
                }
                else if (string.Equals(LoginInfo.CFG_AREA_ID, "M1"))
                {
                    for (int col = _xAxis; col > 0; col--)
                    {
                        dtCellHeader.Columns.Add(col.ToString());
                    }
                    for (int row = 0; row < _yAxis; row++)
                    {
                        dtCellRows.Rows[row]["A"] = (row + 1).ToString();
                    }
                }
                else
                {
                    for (int col = 1; col <= _xAxis; col++)
                    {
                        dtCellHeader.Columns.Add(col.ToString());
                    }

                    for (int row = _yAxis - 1; row >= 0; row--)
                    {
                        dtCellRows.Rows[row]["A"] = (_yAxis - row).ToString();
                    }
                }

                for (int col = 0; col < _xAxis; col++)
                {
                    dtBaseTable.Columns.Add(col.ToString());
                }

                for (int row = 0; row < _yAxis; row++)
                {
                    dtBaseTable.Rows.Add();
                }

                _dtTrayLocation = dtBaseTable.Copy();  // Tray 위치
                dtSubLotChkCode = dtBaseTable.Copy(); // SUBLOT 점검 코드
                dtSubLotID = dtBaseTable.Copy();
                dtCellIDChkCode = dtBaseTable.Copy(); // Cell ID 매칭검사 결과

                int idx = sum;

                if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5")  || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                {
                    for (int row = 0; row < _yAxis; row++)
                    {
                        for (int col = 0; col < _xAxis; col++)
                        {
                            _dtTrayLocation.Rows[row][col] = Util.NVC_Int(idx);
                            idx--;
                        }
                    }
                }
                //C20190224_31462 화면 출력 방향 변경(소형1동만 적용)
                else if (string.Equals(LoginInfo.CFG_AREA_ID, "M1"))
                {
                    for (int row = _yAxis - 1; 0 <= row; row--)
                    {
                        for (int col = 0; col < _xAxis; col++)
                        {
                            _dtTrayLocation.Rows[row][col] = Util.NVC_Int(idx);
                            idx--;
                        }
                    }
                }
                else
                {
                    for (int col = _xAxis - 1; 0 <= col; col--)
                    {
                        for (int row = 0; row < _yAxis; row++)
                        {
                            _dtTrayLocation.Rows[row][col] = Util.NVC_Int(idx);
                            idx--;
                        }
                    }
                }

                _dtBaseCell = dtBaseTable.Copy();
                //txtTurnColor.Background = null;
                txtTrayLocation.Text = string.Empty;
                txtCellId.Text = string.Empty;

                GetData();

                if (!CommonVerify.HasTableRow(_dtSubLotInfo))
                {
                    dgCellInfo.ItemsSource = DataTableConverter.Convert(dtBaseTable);
                    //txtTurnColor.Background = null;
                }
                else
                {
                    var query = _dtSubLotInfo.AsEnumerable().GroupBy(g => g.Field<Int32>("CSTSLOT"))
                        .Select(s => s.OrderByDescending(o => o.Field<Int32>("CSTSLOT")).Last()).CopyToDataTable();

                    if (query.Rows.Count < sum)
                    {
                        for (int j = 0; j < sum; j++)
                        {
                            //if (!query.AsEnumerable().Select(s => s.Field<Int32>("CSTSLOT") == j + 1).Any())
                            if (query.AsEnumerable().All(a => a.Field<Int32>("CSTSLOT") != j + 1))
                            {
                                var dr = query.NewRow();

                                dr["CSTSLOT"] = j + 1;
                                dr["SUBLOTID"] = string.Empty;
                                dr["SUBLOT_CHK_CODE"] = string.Empty;
                                query.Rows.Add(dr);
                            }
                        }

                        query.DefaultView.Sort = "CSTSLOT ASC";
                        query = query.DefaultView.ToTable();
                        query.AcceptChanges();
                    }

                    int index = sum;
                    if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                    {
                        for (int row = 0; row < _yAxis; row++)
                        {
                            //C20190611_14694 원복 DataRow dr = dtCell.NewRow();
                            for (int col = 0; col < _xAxis; col++)
                            {
                                var subLotIdLength = Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) != 0 ? Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) - 1 : 0;
                                _subLotId = subLotIdLength > 0 ? Util.NVC(query.Rows[index - 1]["SUBLOTID"]).Substring(subLotIdLength - 5) : string.Empty; // 뒤에6자리 자른 SUBLOTID
                                _subLotIdClone = Util.NVC(query.Rows[index - 1]["SUBLOTID"]);
                                string subLotChkCode = Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보

                                //dr[col.ToString()] = _subLotId;

                                _dtBaseCell.Rows[row][col] = _subLotId;
                                dtSubLotID.Rows[row][col] = Util.NVC(_subLotIdClone);
                                dtSubLotChkCode.Rows[row][col] = Util.NVC(subLotChkCode);
                                //dtTrayLocation.Rows[row][col] = Util.NVC_Int(index);

                                index--;

                            }
                            //dtCell.Rows.Add(dr);
                        }
                    }
                    //C20190224_31462 화면 출력 방향 변경(소형1동만 적용)
                    else if (string.Equals(LoginInfo.CFG_AREA_ID, "M1"))
                    {
                        for (int row = _yAxis - 1; 0 <= row; row--)
                        {
                            //for (int col = _nX - 1; 0 <= col; col--) [20191018-01 출력 순서 변경]
                            for (int col = 0; col < _xAxis; col++)
                            {
                                var subLotIdLength = Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) != 0 ? Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) - 1 : 0;
                                _subLotId = subLotIdLength > 0 ? Util.NVC(query.Rows[index - 1]["SUBLOTID"]).Substring(subLotIdLength - 5) : string.Empty; // 뒤에6자리 자른 SUBLOTID
                                _subLotIdClone = Util.NVC(query.Rows[index - 1]["SUBLOTID"]);
                                string subLotChkCode = Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                string cellChkCode = Util.NVC(query.Rows[index - 1]["SCAN_RSLT_CODE"]) != string.Empty ? Util.NVC(query.Rows[index - 1]["SCAN_RSLT_CODE"]) : string.Empty;

                                _dtBaseCell.Rows[row][col] = _subLotId;
                                dtSubLotID.Rows[row][col] = Util.NVC(_subLotIdClone);
                                dtSubLotChkCode.Rows[row][col] = Util.NVC(subLotChkCode);
                                dtCellIDChkCode.Rows[row][col] = Util.NVC(cellChkCode);
                                index--;

                            }
                        }
                    }
                    else
                    {
                        for (int col = _xAxis - 1; 0 <= col; col--)
                        {
                            for (int row = 0; row < _yAxis; row++)
                            {
                                var subLotIdLength = Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) != 0 ? Util.NVC_Int(query.Rows[index - 1]["SUBLOTID"].ToString().Length) - 1 : 0;
                                _subLotId = subLotIdLength > 0 ? Util.NVC(query.Rows[index - 1]["SUBLOTID"]).Substring(subLotIdLength - 5) : string.Empty; // 뒤에6자리 자른 SUBLOTID
                                _subLotIdClone = Util.NVC(query.Rows[index - 1]["SUBLOTID"]);
                                string subLotChkCode = Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(query.Rows[index - 1]["SUBLOT_CHK_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                string cellChkCode = Util.NVC(query.Rows[index - 1]["SCAN_RSLT_CODE"]) != string.Empty ? Util.NVC(query.Rows[index - 1]["SCAN_RSLT_CODE"]) : string.Empty;

                                _dtBaseCell.Rows[row][col] = _subLotId;

                                dtSubLotID.Rows[row][col] = Util.NVC(_subLotIdClone);
                                dtSubLotChkCode.Rows[row][col] = Util.NVC(subLotChkCode);
                                dtCellIDChkCode.Rows[row][col] = Util.NVC(cellChkCode);
                                index--;
                            }
                        }
                    }

                    dgCellInfo.ItemsSource = DataTableConverter.Convert(_dtBaseCell);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            */
        }

        private void GetData()
        {
            try
            {

                const string bizRuleName = "BR_PRD_GET_TRAY_INFO_WS_CELLID";

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("TRAYID", typeof(string));
                inData.Columns.Add("OUT_LOTID", typeof(string));

                DataRow inDataRow = inData.NewRow();
                inDataRow["PROD_LOTID"] = Util.NVC(_prodLotId);
                inDataRow["TRAYID"] = Util.NVC(_trayId);
                inDataRow["OUT_LOTID"] = Util.NVC(_outLotId);
                inData.Rows.Add(inDataRow);

                //string xml = inDataSet.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA,CST_PSTN", inDataSet);

                _dtTrayInfo = dsResult.Tables["OUTDATA"];
                _dtSubLotInfo = dsResult.Tables["CST_PSTN"].Clone();
                _dtSubLotInfo.Columns["CSTSLOT"].DataType = Type.GetType("System.Int32");

                //_dtSubLotInfoClone = _dtSubLotInfo.Clone();
                //_dtSubLotInfoClone.Columns["CSTSLOT"].DataType = Type.GetType("System.Int32");

                foreach (DataRow dr in dsResult.Tables["CST_PSTN"].Rows)
                {
                    _dtSubLotInfo.ImportRow(dr);
                }

                _dtSubLotInfo.DefaultView.Sort = "CSTSLOT ASC";
                _dtSubLotInfo = _dtSubLotInfo.DefaultView.ToTable();
                _dtSubLotInfo.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 엑셀 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "CIRCULAR_CELL_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    if (LoginInfo.CFG_AREA_ID.Equals("MC"))
                    {

                        sheet[0, 0].Value = "CELLID";
                        sheet[0, 1].Value = "CANID";
                        sheet[0, 2].Value = "VENTID";
                        sheet[0, 3].Value = "Tray 위치";



                        sheet[0, 0].Style = sheet[0, 1].Style = sheet[0, 2].Style = sheet[0, 3].Style = styel;


                        sheet.Columns[0].Width = 3000;
                        sheet.Columns[1].Width = 3000;
                        sheet.Columns[2].Width = 3000;
                        sheet.Columns[3].Width = 1500;

                        for (int i = 1; i < _dtSubLotInfo.Rows.Count + 1; i++)
                        {
                            sheet[i, 0].Value = _dtSubLotInfo.Rows[i - 1]["SUBLOTID"];
                            sheet[i, 1].Value = _dtSubLotInfo.Rows[i - 1]["CANID"];
                            sheet[i, 2].Value = _dtSubLotInfo.Rows[i - 1]["VENTID"];
                            sheet[i, 3].Value = _dtSubLotInfo.Rows[i - 1]["CSTSLOT"];
                        }
                    }
                    else
                    {
                        sheet[0, 0].Value = "CELLID";

                        sheet[0, 1].Value = "Tray 위치";



                        sheet[0, 0].Style = sheet[0, 1].Style = styel;


                        sheet.Columns[0].Width = 3000;
                        sheet.Columns[1].Width = 1500;


                        for (int i = 1; i < _dtSubLotInfo.Rows.Count + 1; i++)
                        {
                            sheet[i, 0].Value = _dtSubLotInfo.Rows[i - 1]["SUBLOTID"];
                            sheet[i, 1].Value = _dtSubLotInfo.Rows[i - 1]["CSTSLOT"];
                        }
                    }
                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static bool CheckNumber(string letter)
        {
            Int32 numchk = 0;
            bool isCheck = int.TryParse(letter, out numchk);
            return isCheck;
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
        //2020/09/17 추가 CELL ID 매칭기능 추가
        private void btnMatch_Click(object sender, RoutedEventArgs e)
        {
            if (txtCellId.Text == null || txtCellId.Text == "")
            {
                // SFU4579 비교할 DATA가 없습니다.
                Util.MessageValidation("SFU4579");
                return;
            }
            try
            {
                bSave = true;
                COM001_072_WASHING_CELL_MATCH popMatchCell = new COM001_072_WASHING_CELL_MATCH { FrameOperation = FrameOperation };

                object[] parameters = new object[7];

                parameters[0] = txtCanId.Text;
                parameters[1] = txtVentId.Text;
                parameters[2] = _trayId;
                parameters[3] = _trayId_f;
                parameters[4] = _trayId_b;
                //parameters[0] = txtLotId.Text; //와싱 생산lot
                //parameters[1] = txtTrayId.Text; //CSTID
                //parameters[2] = _equipmentId; // EQPTID
                //parameters[3] = _outLotId; //TRAYID
                //parameters[4] = _selectedTrayLocation; //CELL위치
                //parameters[5] = _cellId; //CELL ID
                //parameters[6] = _workerid; //작업자 ID
                C1WindowExtension.SetParameters(popMatchCell, parameters);

                popMatchCell.Closed += new EventHandler(popMatchCell_Closed);
                popMatchCell.ShowModal();
                popMatchCell.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtCellSearch.Text.Trim()))
                {
                    string cellId = txtCellSearch.Text.Trim();
                    
                    if (CommonVerify.HasTableRow(_dtSubLotInfo) && CommonVerify.HasTableRow(_dtTrayInfo))
                    {
                        var query = (from t in _dtSubLotInfo.AsEnumerable()
                                     where t.Field<string>("SUBLOTID") == cellId || t.Field<string>("CANID") == cellId || t.Field<string>("VENTID") == cellId
                                     select new
                                     {
                                         CarrierSlot = t.Field<Int32>("CSTSLOT"),    // Carrier 슬롯번호
                                         SubLotId = t.Field<string>("SUBLOTID"),                 // CELLID
                                         CanId = t.Field<string>("CANID"),
                                         VentId = t.Field<string>("VENTID"),// CANID       
                                     }).FirstOrDefault();

                        if(query != null)
                        {
                            if (sRow != -1 && sCol != -1)
                            {
                                dgCellInfo.CurrentCell = dgCellInfo.GetCell(sRow, sCol);
                                dgCellInfo.CurrentCell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            }
                            sRow = query.CarrierSlot % _xAxis == 0 ? _xAxis : query.CarrierSlot % _xAxis;
                            sCol = query.CarrierSlot % _yAxis == 0 ? query.CarrierSlot / _yAxis : query.CarrierSlot / _yAxis + 1;
                            dgCellInfo.CurrentCell = dgCellInfo.GetCell(sRow, sCol);
                            dgCellInfo.CurrentCell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE603"));
                            txtCellId.Text = query.SubLotId ?? string.Empty;         // CELLID
                            txtCanId.Text = query.CanId ?? string.Empty;
                            txtVentId.Text = query.VentId ?? string.Empty;
                            txtTrayLocation.Text = query.CarrierSlot.GetString();
                        }
                        else
                        {
                            Util.MessageValidation("SFU1209");
                            return;
                        }
                    }
                }
               
            }
        }
    }
}
