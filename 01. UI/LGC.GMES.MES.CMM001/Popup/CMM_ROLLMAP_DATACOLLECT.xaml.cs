/*************************************************************************************
 Created Date : 2021.03.06
      Creator : 
   Decription : 전극 롤맵 불량정보
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.06  DEVELOPER : Initial Created.
    
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
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ROLLMAP_DATACOLLECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ROLLMAP_DATACOLLECT : C1Window, IWorkArea
    {
        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _RUNLOT = string.Empty;
        private string _WIPSEQ = string.Empty;
        private decimal _LANEQTY = 0;

        private Util _Util = new Util();
        private DataTable _dtWipReason2;
        public bool IsUpdated { get; set; }

        public CMM_ROLLMAP_DATACOLLECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Loaded Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _PROCID = Util.NVC(tmps[0]);
                _EQPTID = Util.NVC(tmps[1]);
                _RUNLOT = Util.NVC(tmps[2]);
                _WIPSEQ = Util.NVC(tmps[3]);
                _LANEQTY = Util.NVC_Decimal(tmps[4]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            InitClearControl();
            GetDefectList();
        }
        #endregion

        #region Event Method
        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDefect(dgWipReason);

                if (dgWipReason2.Visibility == Visibility.Visible)
                    SaveDefect(dgWipReason2);

                // 보정로직 호출
                // adjDefect();

                SaveDefectForRollMap();
                Util.MessageInfo("SFU1270");    //저장되었습니다.
                IsUpdated = true;
                GetDefectList();
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                #region # RollMap ActivityReason
                                if (string.Equals(_PROCID, Process.COATING))
                                {
                                    // 미확정-미확정-기타 (Top) : 수정불가 배경색 적용
                                    if (string.Equals(e.Cell.Column.Name, "RESNQTY") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "MZZ01Z01E61"))
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                }
                                else if (string.Equals(_PROCID, Process.ROLL_PRESSING))
                                {
                                    // 미확정-미확정-기타(MZZ01Z01999), Sample-자주검사(PL08L29), Sample-QA검사(PS01S04) 아닌경우 수정불가
                                    if (string.Equals(e.Cell.Column.Name, "RESNQTY") && 
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "MZZ01Z01999")) ||
                                         string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL08L29") ||
                                         string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PS01S04"))
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                                    else if (string.Equals(e.Cell.Column.Name, "RESNQTY"))
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                }
                                #endregion
                            }
                        }
                    }
                }));
            }
        }

        private void dgWipReason_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }
        }

        private void dgWipReason_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (string.Equals(e.Column.Name, "COUNTQTY") &&
                    !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    e.Cancel = true;

                if ((string.Equals(e.Column.Name, "COUNTQTY") || string.Equals(e.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Column.Name, "RESNQTY")) &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    e.Cancel = true;

                if (string.Equals(e.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    e.Cancel = true;

                #region # ActivityReason 
                // Coater
                if (string.Equals(_PROCID, Process.COATING))
                {
                    // 미확정-미확정-기타 (Top), 초기조건조정-폭/로딩/미스매치(Top), 재조건조정-(Back)-Loading : 수정불가
                    if (string.Equals(e.Column.Name, "RESNQTY") &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "MZZ01Z01E61") ||
                         string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "PL18LD3") ||
                         string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "PL19LA1")))
                    {
                        e.Cancel = true;
                    }
                }
                else if (string.Equals(_PROCID, Process.ROLL_PRESSING))
                {
                    // 미확정-미확정-기타(MZZ01Z01999), Sample-자주검사(PL08L29), Sample-QA검사(PS01S04) 아닌경우 수정불가
                    if (string.Equals(e.Column.Name, "RESNQTY") &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "MZZ01Z01999") ||
                         string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "PL08L29") ||
                         string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNCODE"), "PS01S04")))
                        e.Cancel = false;
                    else
                        e.Cancel = true;
                }
                #endregion
            }
        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                            if (e.Cell.Row.Index != i)
                                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                        }
                    }
                    #region # Coater Back
                    if (string.Equals(_PROCID, Process.COATING))
                    {
                        // 기타 BACK 집계된 수량보다 작은경우 집계수량 적용
                        DataRow[] dr = _dtWipReason2.Select("RESNCODE ='MZZ01Z01E60'");
                        if (dr?.Length > 0)
                        {
                            if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "MZZ01Z01E60"))
                            {
                                if (Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY")) < Util.NVC_Decimal(dr[0]["RESNQTY"]))
                                {
                                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", Util.NVC_Decimal(dr[0]["RESNQTY"]));
                                    //Util.MessageValidation("SFU1500");
                                }
                            }
                        }
                        //
                        decimal _sResnSumqty = 0;
                        //DataRow[] drLoad = (dgWipReason2.ItemsSource as DataView).Table.Select("RESNCODE = 'PL19LA1'"); // 재조건조정 Loading 
                        DataRow[] drCond = (dgWipReason2.ItemsSource as DataView).Table.Select("RESNCODE IN ('PL19LA2','PL19LD5','PL19LD7','PL19LD9','PL19LE1')");

                        foreach (DataRow _iRow in drCond)
                            _sResnSumqty += Util.NVC_Decimal(_iRow["RESNQTY"]);

                        // 재조건조정-(Back)-Loading(PL19LA1) 적용 : 재조건조정-(Back)-Mismatch, 재조건조정-표면(Back), 재조건조정-접힘(Back), 재조건조정-절연(Back), 재조건조정-폭(Back)
                        if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL19LA2") ||
                            string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL19LD5") ||
                            string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL19LD7") ||
                            string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL19LD9") ||
                            string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"), "PL19LE1"))
                        {
                            DataRow[] dr1 = _dtWipReason2.Select("RESNCODE = 'PL19LA1'"); // 재조건조정 Loading 
                            if (dr1?.Length > 0)
                            {
                                for (int i = 0; i < dgWipReason2.Rows.Count; i++)
                                {
                                    if (string.Equals(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "RESNCODE"), "PL19LA1"))
                                    {
                                        DataTableConverter.SetValue(dgWipReason2.Rows[i].DataItem, "RESNQTY", _sResnSumqty + Util.NVC_Decimal(dr1[0]["RESNQTY"]));
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region # RollPress

                    #endregion
                }
            }
        }

        private void dgWipReason_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        //DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);

                    Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                }
            }
        }

        private void dgWipReason2_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                        }
                                    }
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESN_TOT_CHK", false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                        }
                    }
                }
            }));
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Method
        private void InitClearControl()
        {
            if (string.Equals(_PROCID, Process.COATING))
            {
                lblBack.Visibility = Visibility.Visible;
                dgWipReason2.Visibility = Visibility.Visible;
            }                
            else
            {
                lblBack.Visibility = Visibility.Collapsed;
                dgWipReason.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgWipReason2.Visibility = Visibility.Collapsed;
            }

            Util.gridClear(dgWipReason);
            Util.gridClear(dgWipReason2);
        }

        private void GetDefectList()
        {
            try
            {
                Util.gridClear(dgWipReason);
                Util.gridClear(dgWipReason2);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("RESNPOSITION", typeof(string));

                List<C1DataGrid> lst;
                if (string.Equals(_PROCID, Process.COATING))
                    lst = new List<C1DataGrid> { dgWipReason, dgWipReason2 };
                else
                    lst = new List<C1DataGrid> { dgWipReason };
                foreach (C1DataGrid dg in lst)
                {
                    inDataTable.Rows.Clear();

                    DataRow Indata = inDataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _PROCID;
                    Indata["LOTID"] = _RUNLOT;
                    if (string.Equals(_PROCID, Process.COATING))
                        Indata["RESNPOSITION"] = string.Equals(dg.Name, "dgWipReason") ? "DEFECT_TOP" : "DEFECT_BACK";
                    inDataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                    if (dg.Visibility == Visibility.Visible)
                        Util.GridSetData(dg, dt, FrameOperation, true);
                }
                // Back 불량 보관
                _dtWipReason2 = Util.MakeDataTable(dgWipReason2, true); 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = _PROCID;
            inDataTable.Rows.Add(inDataRow);

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            inDataRow = null;
            DataTable dtTop = (dg.ItemsSource as DataView).Table;

            foreach (DataRow dataRow in dtTop.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = _RUNLOT;
                inDataRow["WIPSEQ"] = _WIPSEQ;
                inDataRow["ACTID"] = dataRow["ACTID"];
                inDataRow["RESNCODE"] = dataRow["RESNCODE"];
                inDataRow["RESNQTY"] = dataRow["RESNQTY"].ToString().Equals("") ? 0 : dataRow["RESNQTY"];
                inDataRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(dataRow["DFCT_TAG_QTY"])) ? 0 : dataRow["DFCT_TAG_QTY"];
                inDataRow["LANE_QTY"] = _LANEQTY;
                inDataRow["LANE_PTN_QTY"] = 1;
                inDataRow["COST_CNTR_ID"] = dataRow["COSTCENTERID"];
                inDataRow["WRK_COUNT"] = dataRow["COUNTQTY"].ToString() == "" ? DBNull.Value : dataRow["COUNTQTY"];

                IndataTable.Rows.Add(inDataRow);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void adjDefect()
        {
            string _bizRule = string.Equals(_PROCID, Process.COATING) ? "DA_PRD_REG_ADJ_LOGIC_01_CT_RM" : "DA_PRD_REG_ADJ_LOGIC_01_RP_UI_RM";
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["LOTID"] = _RUNLOT;
            inDataRow["WIPSEQ"] = _WIPSEQ;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi(_bizRule, "INDATA", null, inDataSet);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SaveDefectForRollMap()
        {
            try
            {
                string bizRuleName = string.Equals(_PROCID, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";
                DataSet inDataSet = new DataSet();

                DataTable dtInEquipment = inDataSet.Tables.Add("IN_EQP");
                dtInEquipment.Columns.Add("SRCTYPE", typeof(string));
                dtInEquipment.Columns.Add("IFMODE", typeof(string));
                dtInEquipment.Columns.Add("EQPTID", typeof(string));
                dtInEquipment.Columns.Add("USERID", typeof(string));
                DataRow dr = dtInEquipment.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                dtInEquipment.Rows.Add(dr);

                DataTable dtInLot = inDataSet.Tables.Add("IN_LOT");
                dtInLot.Columns.Add("LOTID", typeof(string));
                dtInLot.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow newRow = dtInLot.NewRow();
                newRow["LOTID"] = _RUNLOT;
                newRow["WIPSEQ"] = _WIPSEQ;
                dtInLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
