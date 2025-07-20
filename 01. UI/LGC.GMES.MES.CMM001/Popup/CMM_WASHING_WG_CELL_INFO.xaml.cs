/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 신광희C
   Decription : Cell 관리 위치조정
--------------------------------------------------------------------------------------
 [Change History]
   2017.08.25   신광희 C : DataGrid Cell Style 적용 및 남경 오창 구분에 의한 로직 변경 및 수정
   2018.12.05   신광희 C : 남경 소형3동(M5) 셀관리 시작위치 변경으로 인한 수정
   2019.07.22   이상준 C : 남경 소형6동(M8) 셀관리 시작위치수정
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
using System.Linq;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Input;
namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_WASHING_WG_CELL_INFO : C1Window, IWorkArea
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
        //private DataRow workOrder;
        private string workOrder = string.Empty;
        private string completeProd = string.Empty;

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

        public CMM_WASHING_WG_CELL_INFO()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void CMM_WASHING_WG_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
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
            workOrder = tmps[8] as string;
            completeProd = tmps[9] as string;

            if (string.IsNullOrEmpty(workOrder))
                return;

            _nX = 16;
            _nY = 16;

            txtTrayId.Text = trayID;

            // 생성 C, 수정 U, 조회 X
            if (trayTag.Equals("C"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Tray생성");
                txtCellQty.Value = _nX * _nY;
                btnTurn.Visibility = Visibility.Collapsed;
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
                    btnStart.Visibility = Visibility.Collapsed;
                    btnCheck.Visibility = Visibility.Collapsed;
                    btnUncheck.Visibility = Visibility.Collapsed;
                }
            }

            this.IsResizable = false;

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

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
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
                if (trayTag.Equals("C"))
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
            //try
            //{
            //    if (txtTrayId.Text != string.Empty)
            //    {
            //        Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            //        Boolean ismatch = regex.IsMatch(txtTrayId.Text);
            //        if (!ismatch)
            //        {
            //            txtTrayId.Text = string.Empty;
            //            txtTrayId.Focus();
            //            Util.MessageValidation("숫자와 영문대문자만 입력가능합니다(글자제한10).");
            //            return;
            //        }
            //    }


            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
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
            if (dtCellData == null)
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

                    //if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
                    //    dr = dtCellData.Select("CSTSLOT ='" + e.Cell.Value.ToString() + "' And CELL_FLAG = 'N'");
                    //else
                    //    dr = dtCellData.Select("CSTSLOT_F ='" + e.Cell.Value.ToString() + "' And CELL_FLAG = 'N'");


                    if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
                    {
                        if (btnTurn.Tag == null || btnTurn.Tag.Equals("N"))
                        {
                            var query = (from t in dtCellData.AsEnumerable()
                                         where t.Field<string>("CSTSLOT") == (string)e.Cell.Value
                                         select t).ToList();

                            if (query.Any())
                            {
                                dr = dtCellData.Select("CSTSLOT ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

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
                            var query = (from t in dtCellData.AsEnumerable()
                                         where t.Field<string>("CSTSLOT_F") == (string)e.Cell.Value
                                         select t).ToList();

                            if (query.Any())
                            {
                                dr = dtCellData.Select("CSTSLOT_F ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

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
                        var query = (from t in dtCellData.AsEnumerable()
                                     where t.Field<string>("CSTSLOT") == (string)e.Cell.Value
                                     select t).ToList();

                        if (query.Any())
                        {
                            dr = dtCellData.Select("CSTSLOT ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

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

                    /*
                    var query = (from t in dtCellData.AsEnumerable()
                        where t.Field<string>("CSTSLOT") == (string) e.Cell.Value
                        select t).ToList();

                    if (query.Any())
                    {
                        dr = dtCellData.Select("CSTSLOT ='" + e.Cell.Value + "' And CELL_FLAG = 'N'");

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
                    */
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
                ShowLoadingIndicator();

                DataSet indataSet = _bizRule.GetBR_PRD_GET_TRAY_INFO_WS();

                DataTable inDataTable = indataSet.Tables["INDATA"];

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PROD_LOTID"] = lotID;
                newRow["TRAYID"] =txtTrayId.Text;
                newRow["OUT_LOTID"] = outlotID;
                inDataTable.Rows.Add(newRow);

                string xmlText = indataSet.GetXml();

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
                        DataSet inDataSet = _bizRule.GetBR_PRD_REG_START_OUT_LOT_WS();
                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];

                        DataRow drow = inDataTable.NewRow();
                        drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drow["IFMODE"] = IFMODE.IFMODE_OFF;
                        drow["EQPTID"] = eqptID;
                        drow["USERID"] = LoginInfo.USERID;
                        drow["PROD_LOTID"] = lotID;
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

        private void ModifyData()//수정
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
                            if ("Y".Equals(completeProd))
                            {
                                bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WS_UI";
                            }
                            else
                            {
                                bizRuleName = "BR_PRD_REG_MODIFY_OUT_LOT_WS";
                            }

                            DataSet inDataSet = _bizRule.GetBR_PRD_REG_MODIFY_OUT_LOT_WS();
                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];

                            DataRow drow = inDataTable.NewRow();
                            drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drow["IFMODE"] = IFMODE.IFMODE_OFF;
                            drow["EQPTID"] = eqptID;
                            drow["PROD_LOTID"] = lotID;
                            drow["OUT_LOTID"] = outlotID;
                            drow["TRAYID"] = txtTrayId.Text;
                            drow["CELLQTY"] = txtCellQty.Value;
                            drow["WO_DETL_ID"] = Util.NVC(workOrder);
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

            Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
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
                //남경 소형5동
                if (string.Equals(LoginInfo.CFG_AREA_ID, "M7") || string.Equals(LoginInfo.CFG_AREA_ID, "M3") || string.Equals(LoginInfo.CFG_AREA_ID, "M5") || string.Equals(LoginInfo.CFG_AREA_ID, "M8"))
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
                    //오창 소형2동
                    for (int row = 0; row < _nY; row++)
                    {
                        DataRow dr = dtCell.NewRow();
                        for (int col = 0; col < _nX; col++)
                        {
                            if (col == 0)
                            {
                                dtCell.Rows.Add(index++);
                            }
                            else
                            {
                                dtCell.Rows[row][col] = index++;
                            }

                            //dtCell.Rows[row][col] = index++;
                            
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
                else
                {
                    //오창 소형2동
                    for (int col = _nX - 1; 0 <= col; col--)
                    {
                        for (int row = 0; row < _nY; row++)
                        {
                            if (col == _nX - 1)
                            {
                                DataRow dr = dtCell.NewRow();
                                dr[col.ToString()] = index--;
                                dtCell.Rows.Add(dr);
                            }
                            else
                            {
                                dtCell.Rows[row][col] = index--;
                            }
                        }
                    }


                    //for (int col = _nX - 1; -1 < col; col--)
                    //{
                    //    for (int row = 0; row < _nY; row++)
                    //    {
                    //        dtCell.Rows.Add(sum--);
                    //        //if (col == 0)
                    //        //{
                    //        //    dtCell.Rows.Add(index++);
                    //        //}
                    //        //else
                    //        //{
                    //        //    dtCell.Rows[row][col] = index++;
                    //        //}
                    //    }
                    //}
                }
            }

            ////Util.gridClear(dgCellInfo);
            dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCell);

            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFC8294B");
            if (convertFromString != null)
                txtTurnColor.Background = new SolidColorBrush((Color)convertFromString);
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
                    {
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _nY + 1) * txtTurnColor.ActualHeight) - ((16 - _nY + 1) * 22.5), 0, 0);
                    }
                    else
                    {
                        txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - ((16 - _nY + 1) * txtTurnColor.ActualHeight) - ((16 - _nY + 1) * 20), 0, 0);
                    }

                    bStart = false;
                }
                else
                {
                    txtTurnColor.Margin = new Thickness(0, dgCellInfo.ActualHeight - txtTurnColor.ActualHeight - 18, 0, 0);
                }
                    
            }
            for (int j = 0; j < dgCellInfo.Columns.Count; j++)
            {
                dgCellInfo.Columns[j].Width = new C1.WPF.DataGrid.DataGridLength(33);
                dgCellInfo.Rows[j].Height = new C1.WPF.DataGrid.DataGridLength(33);
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
                DataRow[] dr = dtCellData.Select("CELL_FLAG = 'N'");
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

            nSumCount = (_nX * _nY) - nRowCount;
            txtCellQty.Value = nSumCount;
        }
        #endregion

        #endregion

 

    }
}
