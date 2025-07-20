/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 이대영D
   Decription : Winding(초소형) Tray 생성
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.03   INS 이대영D : Initial Created.
   2021.08.30   윤세진 선임 : [버튼셀] 모델에 따른 Tray Size 지정(메소드 : SetCellSize() 추가)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_TRAY_CELL_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_TRAY_CELL_INFO : C1Window, IWorkArea
    {
        BizDataSet _bizRule = new BizDataSet();

        private DataTable dtCell = null;
        private DataTable dtCellData = null;
        private DataTable dtOutData = null;
        private DataTable dtCellTemp = null;

        private int _nX = 0;
        private int _nY = 0;

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
        private string prodID = string.Empty;
        private bool btnCell = false;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_TRAY_CELL_INFO()
        {
            InitializeComponent();
        }

        private void CMM_TRAY_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
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

            SetProdId();
            SetBtnCell();
            // 생성 C, 수정 U, 조회 X, 확정 후 생성 I,
            if (trayTag.Equals("C") || trayTag.Equals("I"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Tray생성");

                SetCellSize();

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
                        Util.MessageValidation("SFU3625"); // 선택한 영역이 잘못되었습니다.
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
                        Util.MessageValidation("SFU3625"); // 선택한 영역이 잘못되었습니다.
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
        
        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //Util.MessageConfirm("SFU1112", (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //string sBizNAme = trayTag.Equals("C") ? "BR_PRD_REG_START_OUT_LOT_WNS" : "BR_PRD_REG_MODIFY_CST_CELL_PSTN_WNS";
                        //string sBizNAme = trayTag.Equals("C")  ? "BR_PRD_REG_CREATE_OUT_LOT_WMS" : "BR_PRD_REG_MODIFY_CST_CELL_PSTN_WNS";
                        string sBizNAme = string.Empty;

                        if (trayTag.Equals("C"))
                        {
                            sBizNAme = "BR_PRD_REG_CREATE_OUT_LOT_WMS";
                        }
                        else if (trayTag.Equals("I"))
                        {
                            sBizNAme = "BR_PRD_REG_CREATE_OUT_LOT_WNS_UI";
                        }
                        else
                        {
                            sBizNAme = "BR_PRD_REG_MODIFY_CST_CELL_PSTN_WNS";
                        }

                        if ((trayTag.Equals("C") || trayTag.Equals("I")) && string.IsNullOrEmpty(txtTrayId.Text))
                        {
                            Util.MessageValidation("SFU3623"); // TRAYID는 3자리 이상 입력하세요.
                            return;
                        }

                        DataSet inDataSet = new DataSet();
                        DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                        DataTable inCST = inDataSet.Tables.Add("IN_CST");
                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        DataRow drow = inEQP.NewRow();
                        StringBuilder sb = new StringBuilder();

                        if (string.Equals(sBizNAme, "BR_PRD_REG_CREATE_OUT_LOT_WMS") || string.Equals(sBizNAme, "BR_PRD_REG_CREATE_OUT_LOT_WNS_UI"))
                        {
                            inEQP.Columns.Add("SRCTYPE", typeof(string));
                            inEQP.Columns.Add("LANGID", typeof(string));
                            inEQP.Columns.Add("IFMODE", typeof(string));
                            inEQP.Columns.Add("EQPTID", typeof(string));
                            inEQP.Columns.Add("WO_DETL_ID", typeof(string));
                            inEQP.Columns.Add("PROD_LOTID", typeof(string));
                            //inEQP.Columns.Add("OUT_LOTID", typeof(string));
                            inEQP.Columns.Add("CSTID", typeof(string));
                            inEQP.Columns.Add("INPUTQTY", typeof(Decimal));
                            inEQP.Columns.Add("OUTPUTQTY", typeof(Decimal));
                            inEQP.Columns.Add("RESNQTY", typeof(Decimal));
                            inEQP.Columns.Add("SHIFT", typeof(string));
                            inEQP.Columns.Add("WIPNOTE", typeof(string));
                            inEQP.Columns.Add("WRK_USER_NAME", typeof(string));
                            inEQP.Columns.Add("USERID", typeof(string));

                            inCST.Columns.Add("ROW_NUM", typeof(Decimal));
                            inCST.Columns.Add("LOCATION_BIT", typeof(string));
                            inCST.Columns.Add("LOCATION_NUM", typeof(Decimal));

                            inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                            inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                            inInput.Columns.Add("INPUT_LOTID", typeof(string));

                            drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drow["LANGID"] = LoginInfo.LANGID;
                            drow["IFMODE"] = IFMODE.IFMODE_OFF;
                            drow["EQPTID"] = eqptID;
                            drow["WO_DETL_ID"] = null;
                            drow["PROD_LOTID"] = Util.NVC(lotID);
                            //drow["OUT_LOTID"] = string.Empty;
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
                                drow = inCST.NewRow();
                                drow["ROW_NUM"] = dtCell.Rows[row]["0"];
                                for (int col = 1; col < dgCellInfo.Columns.Count; col++)
                                {
                                    if (string.Equals(dtCell.Rows[col][row.ToString()].ToString(), "1"))
                                        sb.Append("1");
                                    else
                                        sb.Append("0");
                                }
                                drow["LOCATION_BIT"] = sb.ToString().Substring(0, _nY);
                                drow["LOCATION_NUM"] = 0;
                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                                sb.Clear();
                            }

                            drow = inInput.NewRow();
                            drow["EQPT_MOUNT_PSTN_ID"] = string.Empty;
                            drow["EQPT_MOUNT_PSTN_STATE"] = string.Empty;
                            drow["INPUT_LOTID"] = string.Empty;
                            inDataSet.Tables["IN_INPUT"].Rows.Add(drow);
                            
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
                            inEQP.Columns.Add("SRCTYPE", typeof(string));
                            inEQP.Columns.Add("IFMODE", typeof(string));
                            inEQP.Columns.Add("OUT_LOTID", typeof(string));
                            inEQP.Columns.Add("CSTID", typeof(string));
                            inEQP.Columns.Add("CELLQTY", typeof(Int32));
                            inEQP.Columns.Add("USERID", typeof(string));

                            inCST.Columns.Add("ROW_NUM", typeof(Int32));
                            inCST.Columns.Add("LOCATION_BIT", typeof(string));

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
                                drow = inCST.NewRow();
                                drow["ROW_NUM"] = dtCell.Rows[row]["0"];
                                for (int col = 1; col < dgCellInfo.Columns.Count; col++)
                                {
                                    if (string.Equals(dtCell.Rows[col][row.ToString()].ToString(), "1"))
                                        sb.Append("1");
                                    else
                                        sb.Append("0");
                                }
                                drow["LOCATION_BIT"] = sb.ToString().Substring(0, _nY);
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
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
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
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 0.1, 1);
                        }
                        if (e.Cell.Column.Index == 8 || e.Cell.Column.Index == 16 || e.Cell.Column.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 0.1);
                        }
                        if (e.Cell.Row.Index == 8 && e.Cell.Column.Index == 8)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 8 && e.Cell.Column.Index == 16)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 16 && e.Cell.Column.Index == 8)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 8 && e.Cell.Column.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 16 && e.Cell.Column.Index == 16)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 16 && e.Cell.Column.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 16 && e.Cell.Column.Index == 8)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 24 && e.Cell.Column.Index == 8)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 24 && e.Cell.Column.Index == 16)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
                        }
                        if (e.Cell.Row.Index == 24 && e.Cell.Column.Index == 24)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.BorderThickness = new Thickness(0.1, 0.1, 1, 1);
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
                inDataTable.Columns.Add("CONFIRM_FLAG", typeof(string));

                inDataTable = indataSet.Tables["INDATA"];

                DataRow newRow = inDataTable.NewRow();
                //newRow["CSTID"] = trayID;
                newRow["OUT_LOTID"] = outlotID;
                //newRow["OUT_LOTID"] = lotID;
                if (trayTag.Equals("Y"))
                {
                    newRow["CONFIRM_FLAG"] = "Y";
                }
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
            if (btnCell)
            {
                dgCellInfo.RowHeight = new C1.WPF.DataGrid.DataGridLength(20, DataGridUnitType.Pixel);
                dgCellInfo.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(23, DataGridUnitType.Pixel);

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

            if ((trayTag.Equals("C") || trayTag.Equals("I")) && !string.IsNullOrEmpty(lotID)) // 생산반제품 탭의 생성 버튼 클릭 후 호출 하는 경우
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
            else if ((trayTag.Equals("X") || trayTag.Equals("Y")) && !string.IsNullOrEmpty(lotID)) // 생산반제품 탭의 조회 버튼 클릭 후 호출 하는 경우
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

        private void SetCellSize()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("RQSTDT");
                //inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));

                inDataTable = indataSet.Tables["RQSTDT"];

                DataRow newRow = inDataTable.NewRow();
                //newRow["CSTID"] = trayID;
                newRow["EQPTID"] = eqptID;
                newRow["PROCID"] = procID;
                newRow["PRODID"] = prodID;

                inDataTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_WINDING_TRAY_TYPE_BY_EQPTID", "RQSTDT", "RSLTDT", indataSet);

                DataTable outDataTable  = dsResult.Tables["RSLTDT"];
                if (outDataTable.Rows.Count > 0)
                {
                    _nY = Convert.ToInt32(outDataTable.Rows[0]["ROW_NUM"]);
                    _nX = Convert.ToInt32(outDataTable.Rows[0]["COL_NUM"]);
                    
                }

                if(_nY == 0 && _nX == 0)
                {
                    _nY = 32;
                    _nX = 32;
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

        private void SetProdId()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));

                inDataTable = indataSet.Tables["RQSTDT"];

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = lotID;

                inDataTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", indataSet);

                DataTable outDataTable = dsResult.Tables["RSLTDT"];
                if (outDataTable.Rows.Count > 0)
                {
                    prodID = outDataTable.Rows[0]["PRODID"].ToString();
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

        private void SetBtnCell()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("RQSTDT");
                inDataTable.Columns.Add("PRODID", typeof(string));

                inDataTable = indataSet.Tables["RQSTDT"];

                DataRow newRow = inDataTable.NewRow();
                newRow["PRODID"] = prodID;

                inDataTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("DA_BAS_SEL_VW_PRODUCT_MODEL_INFO", "RQSTDT", "RSLTDT", indataSet);

                DataTable outDataTable = dsResult.Tables["RSLTDT"];

                if (outDataTable.Rows.Count > 0)
                {
                    if(outDataTable.Rows[0]["PRODUCT_LEVEL3_CODE"].ToString() == "B")
                        btnCell = true;
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
    }
}
