/*************************************************************************************
 Created Date : 2020.04.23
      Creator : INS 김동일K
   Decription : 수불정보 이상 Data [C20200406-000377]
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.23  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_135.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_135 : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        private int idx = 0;

        public COM001_135()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearch);
                listAuth.Add(btnComment);
                listAuth.Add(btnSave);
                
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                {
                    //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "7");
                    return;
                }

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_135_HELP wndHelp = new COM001_135_HELP();
                wndHelp.FrameOperation = FrameOperation;

                if (wndHelp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = LoginInfo.CFG_AREA_ID;

                    C1WindowExtension.SetParameters(wndHelp, Parameters);

                    wndHelp.Closed += new EventHandler(wndHelp_Closed);
                    
                    this.Dispatcher.BeginInvoke(new Action(() => wndHelp.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                COM001_135_USER wndUser = new COM001_135_USER();
                wndUser.FrameOperation = FrameOperation;

                if (wndUser != null)
                {
                    object[] Parameters = new object[1];

                    C1WindowExtension.SetParameters(wndUser, Parameters);

                    wndUser.Closed += new EventHandler(wndUser_Closed);
                    
                    this.Dispatcher.BeginInvoke(new Action(() => wndUser.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }

                SaveProcess();                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //라인
            string[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboEqsgChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgChild, sFilter: sFilter);

            //공정        
            //C1ComboBox[] cboProcParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcChild, sFilter: sFilter, sCase: "PROCESS_BY_SBL_ABNORM_BAS");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            string[] sFilter4 = { "SBL_VERIF_FINL_RSLT" };
            _combo.SetCombo(cboGbn, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");

            cboGbn.SelectedValue = "N";
        }

        private void GetList()
        {
            try
            {
                string sProcID = Util.GetCondition(cboProcess, "SFU1459");
                if (sProcID.Equals("")) return;
                
                DataTable inTable = null;
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("SBL_VERIF_STATUS", typeof(string));
                
                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = sProcID;
                newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                newRow["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                newRow["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                newRow["SBL_VERIF_STATUS"] = Util.GetCondition(cboGbn, bAllNull: true);

                inTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_SBL_ABNORM_VERIF_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (sProcID.Equals(Process.NOTCHING))
                        {
                            dgList.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;

                            dgList.Columns["USE_QTY_U"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_M"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_L"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_AT_M"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_CT_H"].Visibility = Visibility.Collapsed;

                            dgList.Columns["CONV_INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_END_QTY"].Visibility = Visibility.Visible;
                        }
                        else if (sProcID.Equals(Process.LAMINATION))
                        {
                            dgList.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;

                            dgList.Columns["USE_QTY_U"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_M"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_L"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_AT_M"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_CT_H"].Visibility = Visibility.Collapsed;

                            dgList.Columns["CONV_INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                        }
                        else if (sProcID.Equals(Process.STACKING_FOLDING))
                        {
                            dgList.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;

                            dgList.Columns["USE_QTY_U"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_M"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_L"].Visibility = Visibility.Collapsed;
                            dgList.Columns["USE_QTY_AT_M"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_CT_H"].Visibility = Visibility.Visible;

                            dgList.Columns["CONV_INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            dgList.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;

                            dgList.Columns["USE_QTY_U"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_M"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_L"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_AT_M"].Visibility = Visibility.Visible;
                            dgList.Columns["USE_QTY_CT_H"].Visibility = Visibility.Visible;

                            dgList.Columns["CONV_INPUT_QTY"].Visibility = Visibility.Visible;
                            dgList.Columns["EQPT_END_QTY"].Visibility = Visibility.Visible;
                        }

                        Util.GridSetData(dgList, searchResult, FrameOperation, true);

                        //최종결과 콤보
                        DataTable dt = GetResultCombo();

                        if (dt != null)
                            (dgList.Columns["CBO_SBL_VERIF_FINL_RSLT"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                        tbTotCount.Text = searchResult.Rows.Count.ToString("#,##0");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private DataTable GetResultCombo()
        {
            try
            {
                DataTable inTable = null;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CMCDTYPE", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE1", typeof(string));

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "SBL_VERIF_FINL_RSLT";
                //newRow["ATTRIBUTE1"] = "Y";
                
                inTable.Rows.Add(newRow);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {                
                Util.MessageException(ex);
                return null;
            }
        }

        private void SaveProcess()
        {
            try
            {
                if (dgList == null) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("SBL_ABNORM_CODE", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("PROD_CHARGE_USERID", typeof(string));
                inTable.Columns.Add("PROD_CHARGE_NOTE", typeof(string));
                inTable.Columns.Add("SYSTEM_CHARGE_USERID", typeof(string));
                inTable.Columns.Add("SYSTEM_CHARGE_NOTE", typeof(string));
                inTable.Columns.Add("SBL_VERIF_FINL_RSLT", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    //if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SBL_VERIF_FINL_RSLT")).Equals(""))
                    //{
                    //    Util.MessageValidation(""); // 입력오류 : 최종결과를 선택 하세요.
                    //    return;
                    //}

                    if ((Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERID")).Equals("") && !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_NOTE")).Trim().Equals(""))
                        ||
                        (!Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERID")).Equals("") && Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_NOTE")).Trim().Equals("")))
                    {
                        Util.MessageValidation("SFU3750"); // 입력오류 : 담당자와 Comment 정보는 어느 한 정보만 입력할 수 없습니다.
                        return;
                    }

                    if ((Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERID")).Equals("") && !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_NOTE")).Trim().Equals(""))
                        ||
                        (!Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERID")).Equals("") && Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_NOTE")).Trim().Equals("")))
                    {
                        Util.MessageValidation("SFU3750"); // 입력오류 : 담당자와 Comment 정보는 어느 한 정보만 입력할 수 없습니다.
                        return;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERID")).Equals("") &&
                        Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERID")).Equals(""))
                    {
                        Util.MessageValidation("SFU3751"); // 입력오류 : 생산 담당자 또는 시스템 담당자를 입력 하세요.
                        return;
                    }

                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ABNORM_LOT"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPSEQ"));
                    newRow["SBL_ABNORM_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SBL_ABNORM_CODE"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    newRow["PROD_CHARGE_USERID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERID"));
                    newRow["PROD_CHARGE_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_NOTE"));
                    newRow["SYSTEM_CHARGE_USERID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERID"));
                    newRow["SYSTEM_CHARGE_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_NOTE"));
                    newRow["SBL_VERIF_FINL_RSLT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SBL_VERIF_STATUS"));
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                if (inTable.Rows.Count < 1) return;

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_REG_SBL_ABNORM_VERIF", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1270");

                        GetList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void wndHelp_Closed(object sender, EventArgs e)
        {
            COM001_135_HELP window = sender as COM001_135_HELP;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            COM001_135_USER window = sender as COM001_135_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    if (!string.IsNullOrWhiteSpace(window.USER_NAME()) && !string.IsNullOrWhiteSpace(window.USER_ID()))
                    {
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERNAME", window.USER_NAME());
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_USERID", window.USER_ID());
                    }

                    if (!string.IsNullOrWhiteSpace(window.COMMENT()))
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "PROD_CHARGE_NOTE", window.COMMENT());

                    if (!string.IsNullOrWhiteSpace(window.STATE()) && !Util.NVC(window.STATE()).Trim().Equals("SELECT"))
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "SBL_VERIF_STATUS", window.STATE());


                    if (!string.IsNullOrWhiteSpace(window.USER_NAME_SYS()) && !string.IsNullOrWhiteSpace(window.USER_ID_SYS()))
                    {
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERNAME", window.USER_NAME_SYS());
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_USERID", window.USER_ID_SYS());
                    }

                    if (!string.IsNullOrWhiteSpace(window.COMMENT_SYS()))
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "SYSTEM_CHARGE_NOTE", window.COMMENT_SYS());

                    if (!string.IsNullOrWhiteSpace(window.STATE_SYS()) && !Util.NVC(window.STATE_SYS()).Trim().Equals("SELECT"))
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "SBL_VERIF_STATUS", window.STATE_SYS());
                }

                dgList.UpdateLayout();
                dgList.Refresh();
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("WIP_TYPE_CODE") && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WIP_TYPE_CODE")).Equals("PROD"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
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

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (Util.NVC(cell.Column.Name).Equals("WIP_TYPE_CODE"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, cell.Column.Name)).Equals("PROD"))
                        {
                            COM001_135_INPUT_HIST wndHist = new COM001_135_INPUT_HIST();
                            wndHist.FrameOperation = FrameOperation;

                            if (wndHist != null)
                            {
                                object[] Parameters = new object[3];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, "LOTID"));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, "WIPSEQ"));
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, "PROCID"));

                                C1WindowExtension.SetParameters(wndHist, Parameters);

                                wndHist.Closed += new EventHandler(wndHist_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            COM001_135_INPUT_HIST window = sender as COM001_135_INPUT_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                if (Util.NVC(e.Cell.Column.Name).Equals("PROD_CHARGE_USERNAME"))
                {
                    idx = e.Cell.Row.Index;
                    CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                    popUser.FrameOperation = FrameOperation;

                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_CHARGE_USERNAME"));
                    C1WindowExtension.SetParameters(popUser, Parameters);

                    popUser.Closed += new EventHandler(popUser1_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));

                    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "PROD_CHARGE_USERID", "");
                }
                else if (Util.NVC(e.Cell.Column.Name).Equals("SYSTEM_CHARGE_USERNAME"))
                {
                    idx = e.Cell.Row.Index;
                    CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                    popUser.FrameOperation = FrameOperation;

                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SYSTEM_CHARGE_USERNAME"));
                    C1WindowExtension.SetParameters(popUser, Parameters);

                    popUser.Closed += new EventHandler(popUser2_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));

                    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "SYSTEM_CHARGE_USERID", "");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popUser1_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "PROD_CHARGE_USERNAME", popup.USERNAME);
                DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "PROD_CHARGE_USERID", popup.USERID);
            }
            //else
            //{
            //    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "PROD_CHARGE_USERNAME", "");
            //    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "PROD_CHARGE_USERID", "");
            //}
        }

        private void popUser2_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "SYSTEM_CHARGE_USERNAME", popup.USERNAME);
                DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "SYSTEM_CHARGE_USERID", popup.USERID);
            }
            //else
            //{
            //    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "SYSTEM_CHARGE_USERNAME", "");
            //    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "SYSTEM_CHARGE_USERID", "");
            //}
        }
    }
}

