/*************************************************************************************
 Created Date : 2022.02.08
      Creator : 신광희
   Decription : Assembly 공정진척 Rework Cell 관리 팝업(CMM_WASHING_CELL_MANAGEMENT 참조)
--------------------------------------------------------------------------------------
 [Change History]
   2022.02.08   신광희 : Initial Created.
   2024.03.05   백광영 : NFF Assembly Rework CELL 위치 조회
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
using LGC.GMES.MES.ASSY006.Popup;

namespace LGC.GMES.MES.ASSY006
{
    /// <summary>
    /// ASSY006_WASHING_CELL_MANAGEMENT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY006_ASSEMBLY_CELL_INFO : C1Window, IWorkArea
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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY006_ASSEMBLY_CELL_INFO()
        {
            InitializeComponent();
        }

        private void ASSY006_ASSEMBLY_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
        
            _prodLotId = parameters[0] as string;
            _trayId = parameters[1] as string;
            _outLotId = parameters[2] as string;
            _equipmentId = parameters[3] as string;
            _trayTag = "R";
            //_completeProd = parameters[5] as string;
            //_workerid = parameters[6] as string;
            //_trayId_b = parameters[7] as string;
            //_trayId_f = parameters[8] as string;

            this.Header = ObjectDic.Instance.GetObjectName("Cell조회");
            
            if (LoginInfo.CFG_AREA_ID.Equals("MC"))
            {
                _xAxis = 8;
                _yAxis = 8;
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
            if (_dtTrayInfo != null && _dtTrayInfo.Rows.Count > 0)
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
                txtNGCode.Text = string.Empty;

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
                                         NgCode = t.Field<string>("NG_CODE"),                    // NG Code
                                     }).FirstOrDefault();

                        if (query != null)
                        {
                            _subCellId = query.SubLotId ?? string.Empty;
                            txtCellId.Text = query.SubLotId ?? string.Empty;         // CELLID
                            txtNGCode.Text = query.NgCode ?? string.Empty;           // NG Code
                            DataRow[] dr = null;
                            if (_dtSubLotInfo.Columns.Contains("CSTSLOT"))
                            {
                                dr = _dtSubLotInfo.Select("CSTSLOT ='" + query.CarrierSlot + "'");
                            }
                        }
                        else
                        {
                            txtCellId.Text = string.Empty; // CELLID
                            txtNGCode.Text = string.Empty;  
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

                GetData();

                if (!CommonVerify.HasTableRow(_dtSubLotInfo))
                {
                    dgCellInfo.ItemsSource = DataTableConverter.Convert(_dtBaseCell);
                }
                else
                {

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

                        for (int col = 1; col < _dtBaseCell.Columns.Count; col++)
                        {
                            for (int row = 1; row < _dtBaseCell.Rows.Count; row++)
                            {
                                subLotLength = querySubLot.Rows[index]["SUBLOTID"].ToString().Length != 0 ? querySubLot.Rows[index]["SUBLOTID"].ToString().Length - 1 : 0;
                                subLotId = subLotLength > 0 ? Util.NVC(querySubLot.Rows[index]["SUBLOTID"]).Substring(subLotLength - 5) : string.Empty;
                                _dtBaseCell.Rows[row][col] = subLotId;
                                _dtSubLotId.Rows[row][col] = querySubLot.Rows[index]["SUBLOTID"];
                                _dtTrayLocation.Rows[row][col] = trayIndex++;
                                subLotCheckCode = Util.NVC(querySubLot.Rows[index]["NG_CODE"]) != string.Empty ? Util.NVC(querySubLot.Rows[index]["NG_CODE"]) : string.Empty; // SUBLOT_CHK_CODE 정보
                                _dtSubLotCheckCode.Rows[row][col] = subLotCheckCode;

                                index++;
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


        }

        private void GetData()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_TRAY_INFO_ASS_CELLID";

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


                    sheet[0, 0].Value = "CELLID";
                    sheet[0, 1].Value = "Tray 위치";

                    

                    sheet[0, 0].Style = sheet[0, 1].Style = styel;


                    sheet.Columns[0].Width = 3000;
                    sheet.Columns[1].Width = 1500;


                    for (int i = 1; i < _dtSubLotInfo.Rows.Count+1 ; i++)
                    {
                        sheet[i, 0].Value = _dtSubLotInfo.Rows[i-1]["SUBLOTID"];
                        sheet[i, 1].Value = _dtSubLotInfo.Rows[i-1]["CSTSLOT"];
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
    }
}
