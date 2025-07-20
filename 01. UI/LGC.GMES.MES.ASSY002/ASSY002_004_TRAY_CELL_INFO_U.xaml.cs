/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : Washing 초소형 공정 착공
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_004_TRAY_CELL_INFO_U : C1Window, IWorkArea
    {
        #region Declaration        
        BizDataSet _bizRule = new BizDataSet();
        
        private DataTable dtCell = null;
        private DataTable dtCellData = null;
        private DataTable dtOutData = null;
        private DataTable dtCellTemp = null;

        private int _nX;
        private int _nY;

        private string procID = string.Empty;
        private string lineID = string.Empty;
        private string eqptID = string.Empty;
        private string eqptName = string.Empty;
        private string lotID = string.Empty;
        private string outlotID = string.Empty;
        private string trayID = string.Empty;
        private string trayTag = string.Empty;
        private string woDetlId = string.Empty;
        //private DataRow workOrder;
        private string wipqty2 = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public ASSY002_004_TRAY_CELL_INFO_U()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_004_TRAY_CELL_INFO_U_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
            lineID = tmps[1] as string;
            eqptID = tmps[2] as string;
            eqptName = tmps[3] as string;
            lotID = tmps[4] as string;
            outlotID = tmps[5] as string;
            trayID = tmps[6] as string;
            trayTag = tmps[7] as string;

            // SET WORKORDER
            woDetlId = tmps[8] as string;
            //workOrder = tmps[8] as DataRow;
            wipqty2 = tmps[9] as string;

            txtTrayId.Text = trayID;
            txtWipqty2.Value = Util.StringToDouble(wipqty2); // 재공수량

            // 생성 C, 수정 U, 조회 X
            if (trayTag.Equals("C"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Tray생성");

                _nX = 32;
                _nY = 32;

                SetGridCell(true);
            }
            else
            {
                txtTrayId.IsEnabled = false;
                GetCellData();

                if (trayTag.Equals("U"))
                    this.Header = ObjectDic.Instance.GetObjectName("Tray수정");
                else
                    this.Header = ObjectDic.Instance.GetObjectName("Tray조회");

                this.IsResizable = false;
                
                SetGridCell(false);
            }
        }
        #endregion

        #region [Check]
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    if (cell.Row.Index == 0 || cell.Column.Index == 0)
                    {
                        Util.AlertInfo("선택한 영역이 잘못되었습니다.");
                        return;
                    }
                    int colIndex = cell.Column.Index;
                    int rowIndex = cell.Row.Index;

                    for (int row = 1; row < dtCell.Rows.Count; row++)
                    {
                        for (int col = 1; col < dtCell.Columns.Count; col++)
                        {
                            if (rowIndex == row && colIndex == col && colIndex.ToString() != "0")
                            {
                                dtCell.Rows[row][col.ToString()] = 0;
                            }
                        }
                    }

                    cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#708090"));
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount();
        }
        #endregion

        #region [UnCheck]
        private void btnUncheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    if (cell.Row.Index == 0 || cell.Column.Index == 0)
                    {
                        Util.AlertInfo("선택한 영역이 잘못되었습니다.");
                        return;
                    }
                    int colIndex = cell.Column.Index;
                    int rowIndex = cell.Row.Index;

                    for (int row = 1; row < dtCell.Rows.Count; row++)
                    {
                        for (int col = 1; col < dtCell.Columns.Count; col++)
                        {
                            if (rowIndex == row && colIndex == col && colIndex.ToString() != "0")
                            {
                                dtCell.Rows[row][col.ToString()] = 1;
                            }
                        }
                    }
                    cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#fffafa"));
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount();
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
            //Util.MessageConfirm("SFU1112", (result) =>
            Util.MessageConfirm("저장 하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //string sBizNAme = trayTag.Equals("C") ? "BR_PRD_REG_START_OUT_LOT_WNS" : "BR_PRD_REG_MODIFY_CST_CELL_PSTN_WNS";
                        string sBizNAme = trayTag.Equals("C") ? "BR_PRD_REG_CREATE_OUT_LOT_WMS" : "BR_PRD_REG_MODIFY_CST_CELL_PSTN_WNS";
                        
                        if (trayTag.Equals("C") && string.IsNullOrEmpty(txtTrayId.Text))
                        {
                            Util.MessageInfo("TRAY ID를 입력하십시오.");
                            return;
                        }

                        DataSet inDataSet = new DataSet();
                        DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                        DataTable inInputLot = inDataSet.Tables.Add("IN_CST");
                        DataRow drow = inDataTable.NewRow();
                        StringBuilder sb = new StringBuilder();

                        if (string.Equals(sBizNAme, "BR_PRD_REG_CREATE_OUT_LOT_WMS"))
                        {
                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("LANGID", typeof(string));
                            inDataTable.Columns.Add("IFMODE", typeof(string));
                            inDataTable.Columns.Add("EQPTID", typeof(string));
                            inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                            inDataTable.Columns.Add("OUT_LOTID", typeof(string));                            
                            inDataTable.Columns.Add("CSTID", typeof(string));
                            inDataTable.Columns.Add("INPUTQTY", typeof(Decimal));
                            inDataTable.Columns.Add("OUTPUTQTY", typeof(Decimal));
                            inDataTable.Columns.Add("RESNQTY", typeof(Decimal));
                            inDataTable.Columns.Add("SHIFT", typeof(string));
                            inDataTable.Columns.Add("WIPNOTE", typeof(string));
                            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));

                            inInputLot.Columns.Add("ROW_NUM", typeof(Decimal));
                            inInputLot.Columns.Add("LOCATION_BIT", typeof(string));
                            inInputLot.Columns.Add("LOCATION_NUM", typeof(Decimal));

                            drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drow["LANGID"] = LoginInfo.LANGID;
                            drow["IFMODE"] = IFMODE.IFMODE_OFF;
                            drow["EQPTID"] = eqptID;
                            drow["WO_DETL_ID"] = null;
                            drow["PROD_LOTID"] = Util.NVC(lotID);
                            drow["OUT_LOTID"] = string.Empty;
                            drow["CSTID"] = txtTrayId.Text;                            
                            drow["INPUTQTY"] = Convert.ToDecimal(txtCellQty.Value);
                            drow["OUTPUTQTY"] = Convert.ToDecimal(txtCellQty.Value);
                            drow["RESNQTY"] = 0;
                            drow["SHIFT"] = string.Empty;
                            drow["WIPNOTE"] = string.Empty;
                            drow["WRK_USER_NAME"] = string.Empty;
                            drow["USERID"] = LoginInfo.USERID;
                            inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                            string sLocationBit = string.Empty;

                            for (int row = 1; row < dgCellInfo.Rows.Count; row++)
                            {
                                drow = inInputLot.NewRow();
                                drow["ROW_NUM"] = dtCell.Rows[row]["0"];
                                for (int col = 1; col < dgCellInfo.Columns.Count; col++)
                                {
                                    if (string.Equals(dtCell.Rows[col][row.ToString()].ToString(), "1"))
                                        sb.Append("1");
                                    else
                                        sb.Append("0");
                                }
                                drow["LOCATION_BIT"] = sb.ToString().Substring(0, 32);
                                drow["LOCATION_NUM"] = 0;
                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                                sb.Clear();
                            }
                            new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,IN_CST,IN_INPUT", null, (Result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                this.DialogResult = MessageBoxResult.OK;

                            }, inDataSet);
                        }
                        else
                        {
                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("IFMODE", typeof(string));
                            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                            inDataTable.Columns.Add("CSTID", typeof(string));
                            inDataTable.Columns.Add("CELLQTY", typeof(Int32));
                            inDataTable.Columns.Add("USERID", typeof(string));

                            inInputLot.Columns.Add("ROW_NUM", typeof(Int32));
                            inInputLot.Columns.Add("LOCATION_BIT", typeof(string));

                            drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drow["IFMODE"] = IFMODE.IFMODE_OFF;
                            //drow["OUT_LOTID"] = lotID;
                            drow["OUT_LOTID"] = outlotID;
                            drow["CSTID"] = txtTrayId.Text;
                            drow["CELLQTY"] = txtCellQty.Value;
                            drow["USERID"] = LoginInfo.USERID;
                            inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                            string sLocationBit = string.Empty;

                            for (int row = 1; row < dgCellInfo.Rows.Count; row++)
                            {
                                //drow = inCST.NewRow();
                                drow = inInputLot.NewRow();
                                drow["ROW_NUM"] = dtCell.Rows[row]["0"];
                                for (int col = 1; col < dgCellInfo.Columns.Count; col++)
                                {
                                    if (string.Equals(dtCell.Rows[col][row.ToString()].ToString(), "1"))
                                        sb.Append("1");
                                    else
                                        sb.Append("0");
                                }
                                drow["LOCATION_BIT"] = sb.ToString().Substring(0, 32);
                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                                sb.Clear();
                            }

                            string xmlText = inDataSet.GetXml();

                            new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,IN_CST", null, (Result, ex) =>
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                this.DialogResult = MessageBoxResult.OK;

                            }, inDataSet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
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
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (dtCell != null)
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
                        // dtCell의 Cell의 갯수만큼 돌려서 데이터 "0"이 있는지 찾고 "0"인 dtCell의 위치와 e.Cell의 위치를 비교해서 색상적용
                        for (int i = 0; i < dtCell.Rows.Count; i++)
                        {
                            for (int j = 0; j < dtCell.Columns.Count; j++)
                            {
                                if (dtCell.Rows[i][j.ToString()].ToString() == "0")
                                {
                                    if (e.Cell.Row.Index == i && e.Cell.Column.Index == j && e.Cell.Column.Index.ToString() != "0")
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#708090"));
                                    }
                                }
                            }
                        }
                        // 8번째(Row, Column)마다 Cell 테두리 진한 검은색 적용
                        if (e.Cell.Row.Index == 8 || e.Cell.Row.Index == 16 || e.Cell.Row.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 0, 1);
                        }
                        if (e.Cell.Column.Index == 8 || e.Cell.Column.Index == 16 || e.Cell.Column.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 0);
                        }
                    }
                }
                HiddenLoadingIndicator();
            }));
        }

        private void dgCellInfo_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            // 선택한 셀의 위치
            int rowIdx = dgCellInfo.CurrentRow.Index;
            int colIdx = dgCellInfo.CurrentColumn.Index;
            int selectColIdx = dgCellInfo.Selection.SelectedColumns.Count();
            int selectRowIdx = dgCellInfo.Selection.SelectedRows.Count();
            int startIdx = 1;
            int dgCellRowCount = dgCellInfo.Rows.Count - 1;
            int dgCellColumnCount = dgCellInfo.Columns.Count - 1;

            // 다건
            if (selectColIdx > 1 || selectRowIdx > 1)
            {
                int iRowIdx = 0;
                int iColIdx = 0;

                foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
                {
                    iRowIdx = cell.Row.Index;
                    iColIdx = cell.Column.Index;

                    // 전체 선택
                    if (iRowIdx == 0 && iColIdx == 0)
                    {
                        dgCellInfo.Selection.SelectAll();
                        dgCellInfo.Selection.Remove(dgCellInfo[0, 0], dgCellInfo[0, dgCellColumnCount]);
                        dgCellInfo.Selection.Remove(dgCellInfo[0, 0], dgCellInfo[dgCellRowCount, 0]);
                    }
                    // 상단 선택
                    if (iRowIdx == 0 && iColIdx <= 32 && iColIdx != 0)
                    {
                        dgCellInfo.Selection.Add(dgCellInfo[startIdx, iColIdx], dgCellInfo[dgCellRowCount, iColIdx]);
                        dgCellInfo.Selection.Remove(dgCellInfo[0, iColIdx]);
                    }
                    // 좌측 선택
                    if (iRowIdx <= 32 && iColIdx == 0 && iRowIdx != 0)
                    {
                        dgCellInfo.Selection.Add(dgCellInfo[iRowIdx, startIdx], dgCellInfo[iRowIdx, dgCellColumnCount]);
                        dgCellInfo.Selection.Remove(dgCellInfo[iRowIdx, 0]);
                    }
                }
            }
            else // 단건
            {
                // 전체 선택
                if (rowIdx == 0 && colIdx == 0)
                {
                    dgCellInfo.Selection.SelectAll();
                    dgCellInfo.Selection.Remove(dgCellInfo[0, 0], dgCellInfo[0, dgCellColumnCount]);
                    dgCellInfo.Selection.Remove(dgCellInfo[0, 0], dgCellInfo[dgCellRowCount, 0]);
                }
                // 상단 선택
                if (rowIdx == 0 && colIdx <= 32 && colIdx != 0)
                {
                    dgCellInfo.Selection.Add(dgCellInfo[startIdx, colIdx], dgCellInfo[dgCellRowCount, colIdx]);
                    dgCellInfo.Selection.Remove(dgCellInfo[0, colIdx]);
                }
                // 좌측 선택
                if (rowIdx <= 32 && colIdx == 0 && rowIdx != 0)
                {
                    dgCellInfo.Selection.Add(dgCellInfo[rowIdx, startIdx], dgCellInfo[rowIdx, dgCellColumnCount]);
                    dgCellInfo.Selection.Remove(dgCellInfo[rowIdx, 0]);
                }
            }
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetCellData()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                //inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));

                inDataTable = indataSet.Tables["INDATA"];

                DataRow newRow = inDataTable.NewRow();
                //newRow["CSTID"] = trayID;
                newRow["OUT_LOTID"] = outlotID;
                //newRow["OUT_LOTID"] = lotID;
                inDataTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TRAY_INFO_WNS", "INDATA", "OUTDATA,CST_PSTN", indataSet);

                dtCellData = dsResult.Tables["CST_PSTN"];
                dtOutData = dsResult.Tables["OUTDATA"];
                if (dtOutData != null)
                {
                    _nX = Convert.ToInt32(dtOutData.Rows[0]["X"]);
                    _nY = Convert.ToInt32(dtOutData.Rows[0]["Y"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
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
        /// <param name="bTurn"></param>
        private void SetGridCell(bool bTurn)
        {
            int sum = _nX * _nY;
            int iTopHeader = 0;
            int iLeftHeader = 0;
            int iTopHeaderTemp = 0;
            int iLeftHeaderTemp = 1;

            // Column 생성
            dtCell = new DataTable();

            DataRow dr = dtCell.NewRow();

            // 상단 0 ~ 32 까지 칼럼 생성
            for (int col = 0; col < _nX + 1; col++)
            {
                dtCell.Columns.Add(col.ToString());
                dr[col.ToString()] = iTopHeader++;
            }
            dtCell.Rows.Add(dr);

            // 좌측 0 ~ 32 까지 행 추가
            for (int row = 0; row < _nY + 1; row++)
            {
                dtCell.Rows[row]["0"] = iLeftHeader++;
                if (dtCell.Rows.Count == _nY + 1)
                    break;
                dtCell.Rows.Add();
            }

            // dtCellTemp 생성
            dtCellTemp = new DataTable();

            // 상단 0 ~ 32 까지 칼럼 생성
            dtCellTemp = dtCell.Clone();

            DataRow drTemp = dtCellTemp.NewRow();

            for (int col = 0; col < _nX + 1; col++)
            {
                drTemp[col.ToString()] = iTopHeaderTemp++;
            }
            //drTemp[0] = string.Empty;

            // 상단 0 ~ 32 까지 칼럼 생성
            //for (int col = 0; col < _nX + 1; col++)
            //{
            //    dtCellTemp.Columns.Add(col.ToString());
            //    drTemp[col.ToString()] = iTopHeaderTemp++;
            //}
            //drTemp[0] = string.Empty;
            
            dtCellTemp.Rows.Add(drTemp);

            // 좌측 0 ~ 32 까지 행 추가
            for (int row = 0; row < _nY + 1; row++)
            {
                if (row == 0)
                    dtCellTemp.Rows[row]["0"] = string.Empty;
                else
                    dtCellTemp.Rows[row]["0"] = iLeftHeaderTemp++;

                if (dtCellTemp.Rows.Count == _nY + 1)
                    break;
                dtCellTemp.Rows.Add();
            }

            // 수정
            if (!bTurn)
            {
                string sLocationBit = string.Empty;
                string sLocationBitResult = string.Empty;
                int iLocationBitLength = 0;

                for (int i = 0; i < dtCellData.Rows.Count; i++)
                {
                    sLocationBit = dtCellData.Rows[i]["LOCATION_BIT"].ToString();
                    iLocationBitLength = sLocationBit.Length;

                    for (int j = 1; j < iLocationBitLength + 1; j++)
                    {
                        sLocationBitResult = sLocationBit.Substring(j - 1, 1);
                        dtCell.Rows[j][(i + 1).ToString()] = sLocationBitResult;
                    }
                }
            }// 생성
            else
            {
                for (int row = 1; row < dtCell.Rows.Count; row++)
                {
                    for (int col = 1; col < dtCell.Columns.Count; col++)
                    {
                        dtCell.Rows[row][col.ToString()] = 1;
                    }
                }
            }

            dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCellTemp);

            SetCellCount();
        }

        /// <summary>
        /// Cell Count
        /// </summary>
        private void SetCellCount()
        {
            int iCellQuantity = 0;

            for (int i = 1; i < dtCell.Rows.Count; i++)
            {
                for (int j = 1; j < dtCell.Columns.Count; j++)
                {
                    if (string.Equals(dtCell.Rows[i][j.ToString()].ToString(), "1"))
                    {
                        iCellQuantity++;
                    }
                }
            }

            if (trayTag.Equals("C") && !string.IsNullOrEmpty(lotID)) // 생산반제품 탭의 생성 버튼 클릭 후 호출 하는 경우
            {
                txtCellQty.Value = iCellQuantity; // J/R 수량
                //txtWipqty2.Value = Util.StringToDouble(wipqty2);
                txtWipqty2.Value = iCellQuantity; // 재공수량
            }
            else if (trayTag.Equals("U") && !string.IsNullOrEmpty(lotID)) // 생산반제품 탭의 수정 버튼 클릭 후 호출 하는 경우
            {
                tbWipqty2.Visibility = Visibility.Hidden;
                txtWipqty2.Visibility = Visibility.Hidden;
                txtCellQty.Value = iCellQuantity; // J/R 수량
            }
            else if (trayTag.Equals("X") && !string.IsNullOrEmpty(lotID)) // 생산반제품 탭의 조회 버튼 클릭 후 호출 하는 경우
            {
                btnCheck.Visibility = Visibility.Hidden;
                btnUncheck.Visibility = Visibility.Hidden;
                btnSave.Visibility = Visibility.Hidden;
                txtCellQty.Value = iCellQuantity; // J/R 수량
                txtWipqty2.Value = Util.StringToDouble(wipqty2); // 재공수량
            }
            else // 메뉴에서 호출하는 경우
            {
                txtCellQty.Value = iCellQuantity;                // J/R 수량
                txtWipqty2.Value = Util.StringToDouble(wipqty2); // 재공수량
                if (txtCellQty.Value == txtWipqty2.Value)
                    btnSave.IsEnabled = true;
                else
                    btnSave.IsEnabled = false;
            }
        }


        #endregion

        #endregion
    }
}
