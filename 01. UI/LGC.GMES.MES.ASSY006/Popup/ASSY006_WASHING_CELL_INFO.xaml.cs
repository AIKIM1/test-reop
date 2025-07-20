/*************************************************************************************
 Created Date : 2022.01.27
      Creator : 신광희
   Decription : Cell 관리 위치조정(CMM_WASHING_WG_CELL_INFO 참조) 
              - 와싱공정진척 생산실적 - 생산반제품 Cell관리 버튼 호출 팝업(셀관리유형 : P)
                NFF 용도는 셀관리유형코드 C 밖에 없다고 함. 해당 파일은 사용되지 않지만 추후 조건 추가 시 필요한 경우를 위해 남겨둠.
--------------------------------------------------------------------------------------
 [Change History]
 2021.01.27  신광희 : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Input;
namespace LGC.GMES.MES.ASSY006
{
    public partial class ASSY006_WASHING_CELL_INFO : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();


        private DataTable _dtCellInfo = null;
        private DataTable _dtCellPosition = null;

        private int _xAxis;
        private int _yAxis;
        private bool _isStart = true;

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _lotId = string.Empty;
        private string _outlotId = string.Empty;
        private string _trayId = string.Empty;
        private string _trayTag = string.Empty;
        private string _workOrderId = string.Empty;
        private string _completeProduct = string.Empty;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation {get; set;}
        #endregion

        #region Initialize

        public ASSY006_WASHING_CELL_INFO()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY006_WASHING_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            _equipmentSegmentCode = tmps[1] as string;
            _equipmentCode = tmps[2] as string;
            _equipmentName = tmps[3] as string;
            _lotId = tmps[4] as string;
            _outlotId = tmps[5] as string;
            _trayId = tmps[6] as string;
            _trayTag = tmps[7] as string;
            _workOrderId = tmps[8] as string;
            _completeProduct = tmps[9] as string;

            if (string.IsNullOrEmpty(_workOrderId)) return;

            // 추후 Layout 변경에 따른 공통정보 관리가 필요 함.
            _xAxis = 8;
            _yAxis = 8;

            txtTrayId.Text = _trayId;

            // 생성 C, 수정 U, 조회 X
            if (_trayTag.Equals("C"))
            {
                Header = ObjectDic.Instance.GetObjectName("Tray생성");
                txtCellQty.Value = _xAxis * _yAxis;
                btnTurn.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtTrayId.IsEnabled = false;
                GetCellData();

                if (_trayTag.Equals("U"))
                {
                    Header = ObjectDic.Instance.GetObjectName("Tray수정");
                }
                else
                {
                    Header = ObjectDic.Instance.GetObjectName("Tray조회");
                    btnStart.Visibility = Visibility.Collapsed;
                    btnCheck.Visibility = Visibility.Collapsed;
                    btnUncheck.Visibility = Visibility.Collapsed;
                }
            }

            IsResizable = false;

            SetGridCell(false);
        }
        #endregion

        #region [Check]
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                Util.AlertInfo("SFU1651");
                return;
            }

            foreach (DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                    if (convertFromString != null)
                        cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount(false);
        }
        #endregion

        #region [UnCheck]
        private void btnUncheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                Util.AlertInfo("SFU1651");
                return;
            }

            foreach (DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount(false);
        }
        #endregion

        #region [UnCheck]
        private void btnTurn_Click(object sender, RoutedEventArgs e)
        {
            if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
            {
                btnTurn.Tag = "Y";
                SetGridCell(true);
                btnStart.IsEnabled = false;
                btnUncheck.IsEnabled = false;
                btnCheck.IsEnabled = false;

            }
            else
            {
                btnTurn.Tag = "N";
                SetGridCell(false);
                btnStart.IsEnabled = true;
                btnUncheck.IsEnabled = true;
                btnCheck.IsEnabled = true;
            }

        }
        #endregion

        #region [저장]
        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (!IsValid())
                return;
            else
            {
                if (_trayTag.Equals("C"))
                {
                    SaveData();
                }
                else
                {
                    ModifyData();
                }
            }
        }
        private void txtTrayId_KeyUp(object sender, KeyEventArgs e)
        {

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region DataGrid Event
        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (_dtCellPosition == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRow[] dr;

                    if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                    {
                        if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
                        {
                            var query = (from t in _dtCellPosition.AsEnumerable()
                                         where t.Field<string>("CSTSLOT") == (string)e.Cell.Value
                                         select t).ToList();

                            if (query.Any())
                            {
                                dr = _dtCellPosition.Select("CSTSLOT ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

                                if (dr.Length > 0)
                                {
                                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                        else
                        {
                            var query = (from t in _dtCellPosition.AsEnumerable()
                                         where t.Field<string>("CSTSLOT_F") == (string)e.Cell.Value
                                         select t).ToList();

                            if (query.Any())
                            {
                                dr = _dtCellPosition.Select("CSTSLOT_F ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

                                if (dr.Length > 0)
                                {
                                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                            }
                            else
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                    }
                    else
                    {
                        var query = (from t in _dtCellPosition.AsEnumerable()
                                     where t.Field<string>("CSTSLOT") == (string)e.Cell.Value
                                     select t).ToList();

                        if (query.Any())
                        {
                            dr = _dtCellPosition.Select("CSTSLOT ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

                            if (dr.Length > 0)
                            {
                                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                                if (convertFromString != null)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                        }
                        else
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFBDBDBD");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }));

        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetCellData()
        {
            try
            {
                DataSet indataSet = _bizDataSet.GetBR_PRD_GET_TRAY_INFO_WS();

                DataTable inDataTable = indataSet.Tables["INDATA"];

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PROD_LOTID"] = _lotId;
                newRow["TRAYID"] =txtTrayId.Text;
                newRow["OUT_LOTID"] = _outlotId;
                inDataTable.Rows.Add(newRow);

                //string xmlText = indataSet.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TRAY_INFO_WS", "INDATA", "OUTDATA,CST_PSTN", indataSet);

                _dtCellPosition = dsResult.Tables["CST_PSTN"];
                SetCellCount(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveData() //저장
        {

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();
                        const string bizRuleName = "BR_PRD_REG_START_OUT_LOT_WS";
                        DataSet inDataSet = _bizDataSet.GetBR_PRD_REG_START_OUT_LOT_WS();
                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];

                        DataRow drow = inDataTable.NewRow();
                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = _equipmentCode;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = _lotId;
                        drow["EQPT_LOTID"] = null;
                        drow["CSTID"] = txtTrayId.Text;
                        drow["OUTPUT_QTY"] = Convert.ToDecimal(txtCellQty.Value);

                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        DataTable inCST = inDataSet.Tables["IN_CST"];

                        for (int row = 0; row < dgCellInfo.Rows.Count; row++)
                        {
                            for (int col = 0; col < dgCellInfo.Columns.Count; col++)
                            {

                                if (!dgCellInfo.GetCell(row, col).Presenter.Background.ToString().Equals("#FFBDBDBD"))
                                {
                                    drow = inCST.NewRow();
                                    drow["CSTSLOT"] = dgCellInfo.GetCell(row, col).Value.ToString();
                                    inDataSet.Tables["IN_CST"].Rows.Add(drow);
                                }
                            }
                        }

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST,IN_INPUT", "OUTDATA", (Result, ex) =>
                        {
                            HiddenLoadingIndicator();
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            this.DialogResult = MessageBoxResult.OK;

                        }, inDataSet);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            });


        }

        private void ModifyData()
        {

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            ShowLoadingIndicator();

                            string bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WS";
                            if ("Y".Equals(_completeProduct))
                            {
                                bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WS_UI";
                            }
                            else
                            {
                                bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WS";
                            }

                            DataSet inDataSet = _bizDataSet.GetBR_PRD_REG_MODIFY_OUT_LOT_WS();
                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];

                            DataRow drow = inDataTable.NewRow();
                            drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drow["IFMODE"] = IFMODE.IFMODE_OFF;
                            drow["EQPTID"] = _equipmentCode;
                            drow["PROD_LOTID"] = _lotId;
                            drow["OUT_LOTID"] = _outlotId;
                            drow["TRAYID"] = txtTrayId.Text;
                            drow["CELLQTY"] = txtCellQty.Value;
                            drow["WO_DETL_ID"] = Util.NVC(_workOrderId);
                            drow["USERID"] = LoginInfo.USERID;
                            inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                            DataTable inCST = inDataSet.Tables["IN_CST"];

                            for (int row = 0; row < dgCellInfo.Rows.Count; row++)
                            {
                                for (int col = 0; col < dgCellInfo.Columns.Count; col++)
                                {

                                    if (!dgCellInfo.GetCell(row, col).Presenter.Background.ToString().Equals("#FFBDBDBD"))
                                    {
                                        drow = inCST.NewRow();
                                        drow["CSTSLOT"] = dgCellInfo.GetCell(row, col).Value.ToString();
                                        drow["CELL_FLAG"] = "Y";
                                        inDataSet.Tables["IN_CST"].Rows.Add(drow);
                                    }
                                }
                            }

                            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null, (Result, ex) =>
                            {
                                HiddenLoadingIndicator();
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                this.DialogResult = MessageBoxResult.OK;

                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    }
                }
            });
        }

        private bool IsValid()
        {
            if (txtTrayId.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU1430"); //TrayID를 입력하세요.
                return false;
            }

            if (txtTrayId.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675"); //TrayID는 10자리입니다.
                return false;
            }

            Regex regex = new Regex(@"^[0-9A-Z]{1,10}$");
            Boolean ismatch = regex.IsMatch(txtTrayId.Text);
            if (!ismatch)
            {
                txtTrayId.Focus();
                Util.MessageValidation("SFU3674"); //숫자와 영문대문자만 입력가능합니다.
                return false;
            }
            return true;
        }
        #endregion

        #region[[Validation]
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
        /// <summary>
        /// Cell 생성
        /// </summary>
        /// <param name="isTurn"></param>
        private void SetGridCell(bool isTurn)
        {
            int sum = _xAxis * _yAxis;
            int index = 1;

            // Column 생성
            _dtCellInfo = new DataTable();

            for (int col = 0; col < _xAxis; col++)
            {
                _dtCellInfo.Columns.Add(col.ToString());
            }

            // Row 생성
            if (isTurn)
            {
                //남경 소형5동
                if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                {
                    // 회전
                    for (int col = 0; col < _xAxis; col++)
                    {
                        for (int row = 0; row < _yAxis; row++)
                        {
                            if (col == 0)
                            {
                                _dtCellInfo.Rows.Add(index++);
                            }
                            else
                            {
                                _dtCellInfo.Rows[row][col] = index++;
                            }
                        }
                    }
                }
                else
                {
                    //오창 소형2동
                    for (int row = 0; row < _yAxis; row++)
                    {
                        DataRow dr = _dtCellInfo.NewRow();
                        for (int col = 0; col < _xAxis; col++)
                        {
                            if (col == 0)
                            {
                                _dtCellInfo.Rows.Add(index++);
                            }
                            else
                            {
                                _dtCellInfo.Rows[row][col] = index++;
                            }
                            
                        }
                    }
                }
            }
            else
            {
                // Load시
                index = sum;
                //남경 소형5동
                if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                {
                    for (int row = 0; row < _yAxis; row++)
                    {
                        DataRow dr = _dtCellInfo.NewRow();
                        for (int col = 0; col < _xAxis; col++)
                        {
                            dr[col.ToString()] = index--;
                        }
                        _dtCellInfo.Rows.Add(dr);
                    }
                }
                else
                {
                    //오창 소형2동
                    for (int col = _xAxis - 1; 0 <= col; col--)
                    {
                        for (int row = 0; row < _yAxis; row++)
                        {
                            if (col == _xAxis - 1)
                            {
                                DataRow dr = _dtCellInfo.NewRow();
                                dr[col.ToString()] = index--;
                                _dtCellInfo.Rows.Add(dr);
                            }
                            else
                            {
                                _dtCellInfo.Rows[row][col] = index--;
                            }
                        }
                    }
                }
            }

            dgCellInfo.ItemsSource = DataTableConverter.Convert(_dtCellInfo);

            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFC8294B");
            if (convertFromString != null)
                txtTurnColor.Background = new SolidColorBrush((Color)convertFromString);
            txtTurnColor.Height = dgCellInfo.ActualHeight / _yAxis;


            if (isTurn)
            {
                txtTurnColor.Margin = new Thickness(0, 0, 0, dgCellInfo.ActualHeight - txtTurnColor.ActualHeight);

            }
            else
            {
                // Y축 16기준으로 디자인
                //if (16 - _yAxis > 0)
                if (8 - _yAxis > 0)
                {
                    if (_isStart)
                    {
                        //txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _yAxis + 1) * txtTurnColor.ActualHeight) - ((16 - _yAxis + 1) * 22.5), 0, 0);
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((8 - _yAxis + 1) * txtTurnColor.ActualHeight) - ((8 - _yAxis + 1) * 22.5), 0, 0);
                    }
                    else
                    {
                        //txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _yAxis + 1) * txtTurnColor.ActualHeight) - ((16 - _yAxis + 1) * 20), 0, 0);
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((8 - _yAxis + 1) * txtTurnColor.ActualHeight) - ((8 - _yAxis + 1) * 20), 0, 0);
                    }

                    _isStart = false;
                }
                else
                {
                    txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - txtTurnColor.ActualHeight - 18, 0, 0);
                }
                    
            }
            for (int j = 0; j < dgCellInfo.Columns.Count; j++)
            {
                //dgCellInfo.Columns[j].Width = new DataGridLength(33);
                //dgCellInfo.Rows[j].Height = new DataGridLength(33);
                dgCellInfo.Columns[j].Width = new DataGridLength(66);
                dgCellInfo.Rows[j].Height = new DataGridLength(66);
                dgCellInfo.Columns[j].HorizontalAlignment = HorizontalAlignment.Center;
                
            }
        }

        /// <summary>
        /// Cell Count
        /// </summary>
        private void SetCellCount(bool bQuery)
        {
            int nSumCount = 0;
            int nRowCount = 0;

            if (bQuery)
            {
                DataRow[] dr = _dtCellPosition.Select("CELL_FLAG = 'N'");
                nRowCount = dr.Length;
            }
            else
            {
                for (int row = 0; row < dgCellInfo.Rows.Count; row++)
                {
                    for (int col = 0; col < dgCellInfo.Columns.Count; col++)
                    {
                        if (dgCellInfo.GetCell(row, col).Presenter.Background.ToString().Equals("#FFBDBDBD"))
                            nRowCount++;
                    }
                }
            }

            nSumCount = (_xAxis * _yAxis) - nRowCount;
            txtCellQty.Value = nSumCount;
        }
        #endregion

        #endregion

    }
}
