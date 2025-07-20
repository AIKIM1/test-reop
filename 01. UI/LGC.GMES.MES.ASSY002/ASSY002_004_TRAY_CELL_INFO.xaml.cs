/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : Washing 초소형 공정 착공
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_004_TRAY_CELL_INFO : C1Window, IWorkArea
    {
        #region Declaration
        BizDataSet _bizRule = new BizDataSet();

        private DataTable dtCell = null;
        private DataTable dtCellData = null;

        private int _nX;
        private int _nY;
        private bool bStart = true;

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
        public ASSY002_004_TRAY_CELL_INFO()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Load Event
        private void ASSY002_004_TRAY_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
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
            //workOrder = tmps[8] as DataRow;
            woDetlId = tmps[8] as string;

            //if (workOrder == null)
            //    return;

            _nX = 16;
            _nY = 16;

            txtTrayId.Text = trayID;

            // 생성 C, 수정 U, 조회 X
            if (trayTag.Equals("C"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Tray생성");
                txtCellQty.Text = (_nX * _nY).ToString();
            }
            else
            {
                txtTrayId.IsEnabled = false;
                GetCellData();

                if (trayTag.Equals("U"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("Tray수정");
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("Tray조회");
                    btnStart.IsEnabled = false;
                }
            }

            this.IsResizable = false;

            SetGridCell(false);
        }
        #endregion

        #region Button Event
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                Util.AlertInfo("선택된 항목이 없습니다.");
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount(false);
        }

        private void btnUncheck_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellInfo.Selection.SelectedCells.Count == 0)
            {
                Util.AlertInfo("선택된 항목이 없습니다.");
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
            {
                if (cell.Presenter != null)
                {
                    cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }

            dgCellInfo.Selection.Clear();
            SetCellCount(false);
        }

        private void btnTurn_Click(object sender, RoutedEventArgs e)
        {
            if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
            {
                btnTurn.Tag = "Y";
                SetGridCell(true);
            }
            else
            {
                btnTurn.Tag = "N";
                SetGridCell(false);
            }

        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1112"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string sBizNAme = trayTag.Equals("C") ? "BR_PRD_REG_START_OUT_LOT_WNM" : "BR_PRD_REG_MODIFY_OUT_LOT_WNM";

                        DataSet inDataSet = _bizRule.GetBR_PRD_REG_START_OUT_LOT_WS();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];

                        DataRow drow = inDataTable.NewRow();
                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = eqptID;
                        drow["PROD_LOTID"] = lotID;

                        // 수정일 때만 OUT_LOTID
                        if (trayTag.Equals("U"))
                            drow["OUT_LOTID"] = outlotID;

                        drow["TRAYID"] = txtTrayId.Text;
                        drow["CELLQTY"] = Util.NVC_Int(txtCellQty.Text);
                        drow["WO_DETL_ID"] = Util.NVC(woDetlId);
                        //drow["WO_DETL_ID"] = Util.NVC(workOrder["WO_DETL_ID"]);
                        drow["USERID"] = LoginInfo.USERID;
                        inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                        DataTable inCST = inDataSet.Tables["IN_CST"];

                        for (int row = 0; row < dgCellInfo.Rows.Count; row++)
                        {
                            for (int col = 0; col < dgCellInfo.Columns.Count; col++)
                            {
                                drow = inCST.NewRow();
                                drow["CSTSLOT"] = dgCellInfo.GetCell(row, col).Value.ToString();
                                if (dgCellInfo.GetCell(row, col).Presenter.Background.ToString().Equals("#FFFF0000"))
                                    drow["CELL_FLAG"] = "N";
                                else
                                    drow["CELL_FLAG"] = "Y";

                                inDataSet.Tables["IN_CST"].Rows.Add(drow);
                            }
                        }

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
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            });

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region DataGrid Event
        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (dtCellData == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRow[] dr;

                    if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
                        dr = dtCellData.Select("CSTSLOT ='" + e.Cell.Value.ToString() + "' And CELL_FLAG = 'N'");
                    else
                        dr = dtCellData.Select("CSTSLOT_F ='" + e.Cell.Value.ToString() + "' And CELL_FLAG = 'N'");

                    if (dr.Length > 0)
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);

                }
            }));

        }
        #endregion

        #region User Method
        /// <summary>
        /// Tray Cell 조회
        /// </summary>
        /// <param name="bTurn"></param>
        /// 
        private void GetCellData()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizRule.GetBR_PRD_GET_TRAY_INFO_WS();

                DataTable inDataTable = indataSet.Tables["INDATA"];

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PROD_LOTID"] = lotID;
                newRow["TRAYID"] = trayTag.Equals("C") ? null : txtTrayId.Text;
                inDataTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TRAY_INFO_WS", "INDATA", "OUTDATA,CST_PSTN", indataSet);

                dtCellData = dsResult.Tables["CST_PSTN"];
                SetCellCount(true);
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

        /// <summary>
        /// Cell 생성
        /// </summary>
        /// <param name="bTurn"></param>
        private void SetGridCell(bool bTurn)
        {
            int sum = _nX * _nY;
            int index = 1;

            // Column 생성
            dtCell = new DataTable();

            for (int col = 0; col < _nX; col++)
            {
                dtCell.Columns.Add(col.ToString());
            }

            // Row 생성
            if (bTurn)
            {
                // 회전
                for (int col = 0; col < _nX; col++)
                {
                    for (int row = 0; row < _nY; row++)
                    {
                        if (col == 0)
                        {
                            dtCell.Rows.Add(index++);
                        }
                        else
                        {
                            dtCell.Rows[row][col] = index++;
                        }
                    }
                }
            }
            else
            {
                // Load시
                index = sum;
                for (int row = 0; row < _nY; row++)
                {
                    DataRow dr = dtCell.NewRow();
                    for (int col = 0; col < _nX; col++)
                    {
                        dr[col.ToString()] = index--;
                    }
                    dtCell.Rows.Add(dr);
                }
            }

            ////Util.gridClear(dgCellInfo);
            dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCell);

            txtTurnColor.Background = new SolidColorBrush(Colors.Red);
            txtTurnColor.Height = dgCellInfo.ActualHeight / _nY;

            if (bTurn)
            {
                txtTurnColor.Margin = new Thickness(0, 0, 0, dgCellInfo.ActualHeight - txtTurnColor.ActualHeight);
            }
            else
            {
                // Y축 16기준으로 디자인
                if (16 - _nY > 0)
                {
                    if (bStart)
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _nY + 1) * txtTurnColor.ActualHeight) - ((16 - _nY + 1) * 22.5), 0, 0);
                    else
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _nY + 1) * txtTurnColor.ActualHeight) - ((16 - _nY + 1) * 20), 0, 0);

                    bStart = false;
                }
                else
                    txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - txtTurnColor.ActualHeight - 18, 0, 0);
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
                DataRow[] dr = dtCellData.Select("CELL_FLAG = 'N'");
                nRowCount = dr.Length;
            }
            else
            {
                for (int row = 0; row < dgCellInfo.Rows.Count; row++)
                {
                    for (int col = 0; col < dgCellInfo.Columns.Count; col++)
                    {
                        if (dgCellInfo.GetCell(row, col).Presenter.Background.ToString().Equals("#FFFF0000"))
                            nRowCount++;
                    }
                }
            }

            nSumCount = (_nX * _nY) - nRowCount;
            txtCellQty.Text = nSumCount.ToString();
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

        #endregion


    }
}
