/*************************************************************************************
 Created Date : 2022.11.15
      Creator : 조영대
   Decription : Master Sample Cell 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.15                              조영대 : Initial Created
  2024.08.08     [E20240304-000404]       주경호 : Form#1/2/3 -> Lack of possibility to change status for Master Sample from Y to N
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_120 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private bool isPasteMode = false;

        private bool isMasterCellChgYn = false;

        public FCS001_120()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitControl()
        {
            ApplyPermissions();

            cboUseFlag.SetCommonCode("USE_FLAG", CommonCombo.ComboStatus.ALL, true);
            cboUseFlag.SelectedValue = "Y";

            isMasterCellChgYn = GetMasCellChgYn();

            GetList();
        }
        
        #endregion

        #region [Event]
        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (!txtCellId.Text.Trim().Equals(string.Empty) && e.Key.Equals(Key.Enter))
            {
                btnSearch.PerformClick();
            }
        }

        private void dgCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    // Default Color
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                    if (string.IsNullOrEmpty(Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "USE_FLAG"))))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);

                        if (e.Cell.Column.Name.Equals("SUBLOTID"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                    else
                    {
                        if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "ROW_NUM")).Equals("-1"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);

                            if (e.Cell.Column.Name.Equals("SUBLOTID"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;
                            }
                        }
                        else if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "ROW_NUM")).Equals("0"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);

                            if (e.Cell.Column.Name.Equals("SUBLOTID"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                            }
                            else
                            {
                                if (e.Cell.Column.Name.Equals("MEMO"))
                                {
                                    if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "SUBLOTID")).Equals(string.Empty))
                                    {
                                        e.Cell.Presenter.Background = null;
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                                    }
                                }

                                if (e.Cell.Column.Name.Equals("USE_FLAG"))
                                {
                                    if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "SUBLOTID")).Equals(string.Empty))
                                    {
                                        e.Cell.Presenter.Background = null;
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                                    }

                                    if (!Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "LOT_DETL_TYPE_CODE")).Equals("N"))
                                    {
                                        e.Cell.Presenter.Background = null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                            if (e.Cell.Column.Name.Equals("MEMO"))
                            {
                                if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "SUBLOTID")).Equals(string.Empty) ||
                                    Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "ROW_NUM")).Equals("-1"))
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                                }
                            }

                            if (e.Cell.Column.Name.Equals("USE_FLAG"))
                            {
                                if (Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "SUBLOTID")).Equals(string.Empty) ||
                                    Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "ROW_NUM")).Equals("-1"))
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 224));
                                }

                                if (!Util.NVC(dataGrid.GetValue(e.Cell.Row.Index, "LOT_DETL_TYPE_CODE")).Equals("N"))
                                {
                                    if(isMasterCellChgYn== false)
                                    {
                                        e.Cell.Presenter.Background = null;
                                    }
                                }
                            }
                        }
                    }

                }
            }));
        }

        private void dgCellList_ClipboardPasted(object sender, DataObjectPastingEventArgs e)
        {
            isPasteMode = true;

            C1DataGrid dg = sender as C1DataGrid;

            if (dg != null && dg.CurrentColumn != null)
            {
                switch (dg.CurrentColumn.Name)
                {
                    case "SUBLOTID":
                        string cellString = Clipboard.GetText().ToUpper().Replace(" ", "").Replace("\r", ",").Replace("\n", ",");
                        string[] cells = cellString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        cellString = string.Join(",", cells);

                        if (cellString.Contains(","))
                        {
                            dg.EndEditRow(false);

                            cells = cellString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (cells.Length > 0)
                            {
                                DataTable dtCellInfo = GetCellInfo(string.Join(",", cells));
                                SetCellInfo(dtCellInfo, cells);
                            }
                        }
                        else
                        {
                            if (!dg.CurrentCell.IsEditing)
                            {
                                dg.SetValue(dg.GetCurrentRowIndex(), "SUBLOTID", cellString);
                                GetCellInfo(dg.CurrentRow.Index);
                            }
                        }
                        break;
                    case "MEMO":
                        string memoString = Clipboard.GetText();
                        dg.SetValue(dg.GetCurrentRowIndex(), "MEMO", memoString);
                        dgCellList.SetValue(dg.GetCurrentRowIndex(), "CHK", 1);
                        break;
                }
            }

            isPasteMode = false;
        }

        private void dgCellList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Column.Name.Equals("SUBLOTID") && 
                !Util.NVC(dg.GetValue(e.Row.Index, "ROW_NUM")).Equals("0") &&
                !Util.NVC(dg.GetValue(e.Row.Index, "ROW_NUM")).Equals("-1"))
            {
                e.Cancel = true;
                return;
            }

            if (e.Column.Name.Equals("MEMO") && Util.NVC(dg.GetValue(e.Row.Index, "ROW_NUM")).Equals("-1"))
            {
                e.Cancel = true;
                return;
            }

            if (e.Column.Name.Equals("USE_FLAG") && Util.NVC(dg.GetValue(e.Row.Index, "ROW_NUM")).Equals("-1"))
            {
                e.Cancel = true;
                return;
            }

            if (e.Column.Name.Equals("USE_FLAG") && 
                !Util.NVC(dg.GetValue(e.Row.Index, "LOT_DETL_TYPE_CODE")).Equals("N"))
            {
                if (isMasterCellChgYn == false)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (e.Column.Name.Equals("SUBLOTID"))
            {
                dgCellList.SetValue(e.Row.Index, "LOTID", string.Empty);
                dgCellList.SetValue(e.Row.Index, "SUBLOTJUDGE", string.Empty);
                dgCellList.SetValue(e.Row.Index, "PROC_NAME", string.Empty);
                dgCellList.SetValue(e.Row.Index, "EQSG_NAME", string.Empty);
                dgCellList.SetValue(e.Row.Index, "MODEL_NAME", string.Empty);
                dgCellList.SetValue(e.Row.Index, "ROUTID", string.Empty);
                dgCellList.SetValue(e.Row.Index, "MEMO", string.Empty);
                dgCellList.SetValue(e.Row.Index, "USE_FLAG", string.Empty);
                e.Row.Refresh();
            }

            if (e.Column.Name.Equals("CHK") &&
                string.IsNullOrEmpty(Util.NVC(dg.GetValue(e.Row.Index, "USE_FLAG"))))
            {
                e.Cancel = true;
                return;
            }
        }

        private void dgCellList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (isPasteMode) return;
            if (e.Cell.Column.Name.Equals("CHK")) return;

            switch (e.Cell.Column.Name)
            {
                case "SUBLOTID":
                    GetCellInfo(e.Cell.Row.Index);
                    break;
                case "MEMO":
                case "USE_FLAG":
                    dgCellList.SetValue(e.Cell.Row.Index, "CHK", 1);
                    dgCellList.Refresh();
                    break;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgCellList.EndEditRow(false);

            GetList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellList.CurrentColumn.Name.Equals("SUBLOTID"))
            {
                dgCellList.EndEditRow(false);
            }
            else
            {
                dgCellList.EndEditRow(true);
            }
            dgCellList.CurrentRow.Refresh();

            SaveList();
        }

        private void dgCellList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (e.Exception == null)
            {
                dgCellList.AddRows(1);
                dgCellList.SetValue(dgCellList.Rows.Count - 1, "ROW_NUM", "-1");

                C1.WPF.DataGrid.DataGridColumn col = dgCellList.Columns["USE_FLAG"];
                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;
                if (cboColumn.ItemsSource == null)
                {
                    dgCellList.SetGridColumnCommonCombo("USE_FLAG", "USE_FLAG", isInCode: true, isInBlank: false);
                }
            }

            btnSearch.IsEnabled = true;
            btnSave.IsEnabled = true;
        }

        #endregion

        #region [Method]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private DataTable GetCellInfo(string cells)
        {
            try
            {
                if (string.IsNullOrEmpty(cells)) return null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_OPT", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_OPT"] = "CELL_INFO";
                dr["SUBLOTID"] = cells;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SEL_TB_SFC_FORM_MST_SMPL_SUBLOT_MNGT", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private void SetCellInfo(DataTable dtCellInfo, string[] cells)
        {
            if (dtCellInfo == null) return;

            int insertRow = dgCellList.Rows.Count - 1;
            foreach (string cell in cells)
            {
                // 중복 체크
                List<DataRow> findList = dgCellList.FindDataRow("SUBLOTID", cell);
                if (findList.Count == 0)
                {
                    dgCellList.SetValue(insertRow, "SUBLOTID", cell);

                    DataRow[] findCells = dtCellInfo.Select("SUBLOTID = '" + cell + "'");
                    if (findCells.Length > 0)
                    {
                        DataRow drInfo = findCells[0];
                        if (drInfo["LOT_DETL_TYPE_CODE"].Equals("N"))
                        {
                            dgCellList.SetValue(insertRow, "LOTID", drInfo["LOTID"]);
                            dgCellList.SetValue(insertRow, "SUBLOTJUDGE", drInfo["SUBLOTJUDGE"]);
                            dgCellList.SetValue(insertRow, "PROC_NAME", drInfo["PROC_NAME"]);
                            dgCellList.SetValue(insertRow, "EQSG_NAME", drInfo["EQSG_NAME"]);
                            dgCellList.SetValue(insertRow, "MODEL_NAME", drInfo["MODEL_NAME"]);
                            dgCellList.SetValue(insertRow, "ROUTID", drInfo["ROUTID"]);
                            dgCellList.SetValue(insertRow, "USE_FLAG", "Y");
                            dgCellList.SetValue(insertRow, "CHK", 1);
                            dgCellList.SetValue(dgCellList.Rows.Count - 1, "ROW_NUM", "0");
                        }
                        else
                        {
                            dgCellList.SetValue(insertRow, "LOTID", MessageDic.Instance.GetMessage("FM_ME_0460")); // // 폐기 대기 LOT 만 등록 가능 합니다.
                            dgCellList.SetValue(insertRow, "ROW_NUM", "-1");
                        }
                    }
                    else
                    {
                        dgCellList.SetValue(insertRow, "LOTID", MessageDic.Instance.GetMessage("SFU1209")); // Cell 정보가 없습니다.
                        dgCellList.SetValue(insertRow, "ROW_NUM", "-1");
                    }
                }
                else
                {
                    dgCellList.SetValue(insertRow, "SUBLOTID", cell);
                    dgCellList.SetValue(insertRow, "LOTID", MessageDic.Instance.GetMessage("SFU4384")); // 중복된 Cell 정보가 존재합니다.
                    dgCellList.SetValue(insertRow, "ROW_NUM", "-1");                    
                }

                dgCellList.AddRows(1);
                dgCellList.SetValue(dgCellList.Rows.Count - 1, "ROW_NUM", "-1");

                insertRow++;
            }

            dgCellList.SelectRow(insertRow);
        }

        private void GetCellInfo(int row)
        {
            try
            {
                string cellId = Util.NVC(dgCellList.GetValue(row, "SUBLOTID")).Trim().ToUpper();
                if (string.IsNullOrEmpty(cellId)) return;

                if (dgCellList.ExistsCheck("SUBLOTID", cellId) > 1)
                {
                    // 중복된 Cell 정보가 존재합니다.
                    Util.MessageValidation("SFU4384");
                    dgCellList.SetValue(row, "SUBLOTID", string.Empty);
                    dgCellList.Rows[row].Refresh();
                    return;
                }

                dgCellList.EndEditRow(false);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_OPT", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_OPT"] = "EXISTS_CHECK";
                dr["SUBLOTID"] = cellId;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SEL_TB_SFC_FORM_MST_SMPL_SUBLOT_MNGT", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    // 이미 등록된 Cell[% 1] 입니다.
                    Util.MessageValidation("101062", cellId);
                    dgCellList.SetValue(row, "SUBLOTID", string.Empty);
                    dgCellList.SetValue(row, "CHK", 0);
                    return;
                }

                dr["WRK_OPT"] = "CELL_INFO";
                dtResult = new ClientProxy().ExecuteServiceSync("BR_SEL_TB_SFC_FORM_MST_SMPL_SUBLOT_MNGT", "INDATA", "OUTDATA", dtRqst);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow drInfo = dtResult.Rows[0];
                    if (!drInfo["LOT_DETL_TYPE_CODE"].Equals("N"))
                    {
                        // 폐기 대기 LOT 만 등록 가능 합니다.
                        Util.MessageValidation("FM_ME_0460", cellId);
                        dgCellList.SetValue(row, "SUBLOTID", string.Empty);
                        dgCellList.SetValue(row, "CHK", 0);
                        return;
                    }

                    dgCellList.SetValue(row, "SUBLOTID", drInfo["SUBLOTID"]);
                    dgCellList.SetValue(row, "LOTID", drInfo["LOTID"]);
                    dgCellList.SetValue(row, "SUBLOTJUDGE", drInfo["SUBLOTJUDGE"]);
                    dgCellList.SetValue(row, "PROC_NAME", drInfo["PROC_NAME"]);
                    dgCellList.SetValue(row, "EQSG_NAME", drInfo["EQSG_NAME"]);
                    dgCellList.SetValue(row, "MODEL_NAME", drInfo["MODEL_NAME"]);
                    dgCellList.SetValue(row, "ROUTID", drInfo["ROUTID"]);
                    dgCellList.SetValue(row, "USE_FLAG", "Y");
                    dgCellList.SetValue(row, "ROW_NUM", "0");
                    dgCellList.SetValue(row, "CHK", 1);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(dgCellList.GetValue(dgCellList.Rows.Count - 1, "SUBLOTID"))))
                        {
                            dgCellList.AddRows(1);
                            dgCellList.SetValue(dgCellList.Rows.Count - 1, "ROW_NUM", "-1");
                            dgCellList.SelectCell(dgCellList.Rows.Count - 1, 1);
                        }
                    }));
                }
                else
                {
                    // Cell 정보가 없습니다.
                    Util.MessageValidation("SFU1209");
                    dgCellList.SetValue(row, "SUBLOTID", string.Empty);
                    dgCellList.SetValue(row, "LOTID", string.Empty);
                    dgCellList.SetValue(row, "CHK", 0);
                }
                dgCellList.Rows[row].Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetList()
        {
            try
            {
                btnSearch.IsEnabled = false;
                btnSave.IsEnabled = false;

                dgCellList.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WRK_OPT", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WRK_OPT"] = "SEARCH";
                dr["SUBLOTID"] = txtCellId.GetBindValue();
                dr["USE_FLAG"] = cboUseFlag.GetBindValue();
                dtRqst.Rows.Add(dr);

                // Background 처리 완료시 dgCellList_ExecuteDataCompleted 이벤트 호출
                dgCellList.ExecuteService("BR_SEL_TB_SFC_FORM_MST_SMPL_SUBLOT_MNGT", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveList()
        {
            try
            {
                btnSearch.IsEnabled = false;
                btnSave.IsEnabled = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("MEMO", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                foreach (DataRow dr in dgCellList.GetCheckedDataRow("CHK"))
                {
                    if (Util.NVC(dr["SUBLOTID"]).Equals(string.Empty)) continue;

                    DataRow drNew = dtRqst.NewRow();
                    drNew["LANGID"] = LoginInfo.LANGID;
                    drNew["SUBLOTID"] = dr["SUBLOTID"];
                    drNew["MEMO"] = dr["MEMO"];
                    drNew["USE_FLAG"] = dr["USE_FLAG"];
                    drNew["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(drNew);
                }

                if (dtRqst.Rows.Count == 0)
                {
                    btnSearch.IsEnabled = true;
                    btnSave.IsEnabled = true;

                    // 처리할 데이터가 없습니다.
                    Util.MessageValidation("FM_ME_0240");

                    return;
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_SET_MASTER_SAMPLE_CELL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        Util.MessageInfo("SFU3532");    //저장 되었습니다.

                        GetList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        // [E20240304-000404] 건으로 폴란드 활성화 1,2,3동만 반영하기 위해서 함수 생성
        private bool GetMasCellChgYn()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_MAS_CELL_ALL_CHG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}


