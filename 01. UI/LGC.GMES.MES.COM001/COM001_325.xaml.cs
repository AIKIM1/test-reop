/*************************************************************************************
 Created Date : 2020.04.01
      Creator : 
   Decription : 공정 품질승인 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.01  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
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
using C1.WPF.DataGrid;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_325 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        bool bCheck = false;
        Util _Util = new Util();
        DataTable dtTemp = new DataTable();
        DataTable dtColor = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_325()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;
            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;
            ApplyPermissions();
            InitCombo();
            InitGrid();
            dtColor = getCommoncode("RESOURCE_QUALITY_CONFIRM");
            bCheck = true;
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetGridDate();
            }));
        }

        private void SetEquipment()
        {
            try
            {
                //// 동을 선택하세요.
                //string sArea = Util.GetCondition(cboArea);
                //if (string.IsNullOrWhiteSpace(sArea))
                //    return;

                //string sEqsg = Util.GetCondition(cboEquipmentSegment);
                //if (string.IsNullOrWhiteSpace(sEqsg))
                //{
                //    cboEquipment.ItemsSource = null;
                //    return;
                //}

                //    string sProcess = Util.GetCondition(cboProcess);

                //    DataTable RQSTDT = new DataTable();
                //    RQSTDT.TableName = "RQSTDT";
                //    RQSTDT.Columns.Add("LANGID", typeof(string));
                //    RQSTDT.Columns.Add("AREAID", typeof(string));
                //    RQSTDT.Columns.Add("EQSGID", typeof(string));
                //    RQSTDT.Columns.Add("PROCID", typeof(string));

                //    DataRow dr = RQSTDT.NewRow();
                //    dr["LANGID"] = LoginInfo.LANGID;
                //    dr["AREAID"] = sArea;
                //    dr["EQSGID"] = sEqsg;
                //    dr["PROCID"] = string.IsNullOrWhiteSpace(sProcess) ? null : sProcess; 
                //    RQSTDT.Rows.Add(dr);

                //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //    cboEquipment.DisplayMemberPath = "CBO_NAME";
                //    cboEquipment.SelectedValuePath = "CBO_CODE";

                //    DataRow drIns = dtResult.NewRow();
                //    drIns["CBO_NAME"] = "-ALL-";
                //    drIns["CBO_CODE"] = "";
                //    dtResult.Rows.InsertAt(drIns, 0);

                //    cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                //    if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                //    {
                //        cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                //        if (cboEquipment.SelectedIndex < 0)
                //            cboEquipment.SelectedIndex = 0;
                //    }
                //    else
                //    {
                //        cboEquipment.SelectedIndex = 0;
                //    }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCboEquipmentSegment()
        {
            try
            {

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(result);
                cboEquipmentSegment.CheckAll();
            }
            catch (Exception ex)
            {

            }
        }

        private void SetCboProcess()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREAID_PCSG_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(result);
                cboProcess.CheckAll();
            }
            catch (Exception ex) { }
        }

        private void SetCboEquipment()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString).Equals("") ? null : Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                drnewrow["PROCID"] = Util.NVC(cboProcess.SelectedItemsToString).Equals("") ? null : Util.NVC(cboProcess.SelectedItemsToString);
                dtRQSTDT.Rows.Add(drnewrow);

                
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_EQSG_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboEquipment.ItemsSource = DataTableConverter.Convert(result);
                cboEquipment.CheckAll();
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Event
        private void cboArea_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipmentSegment();
                    SetCboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboProcess_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!validationGrid(dgList))
                return;

            callCreate();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!validationGrid(dgList))
                return;

            callUpdate();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1499");  //동을 선택하세요
                return;
            }

            if (Util.NVC(cboEquipmentSegment.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요
                return;
            }

            if (Util.NVC(cboProcess.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return;
            }

            if (Util.NVC(cboEquipment.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }


            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("tCalendar"))
                SearchData();
            else
                SearchDataHistory();
        }

        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (bCheck)
                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                string _Day = string.Empty;
                string _colName = string.Empty;
                string _colPName = string.Empty;
                string _dayofWeek = string.Empty;
                string _date = string.Empty;
                string ValueToCode = string.Empty;
                string ValueToColor = string.Empty;

                int iYear;
                int iMon;
                int iDay;
                DateTime DateValue;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Top && e.Cell.Column.Name.Contains("P_"))
                    {
                        string[] splitColHeader = e.Cell.Column.ActualGroupHeader.ToString().Split(',');
                        string[] splitYearMonth = splitColHeader[0].Split('.');

                        iYear = int.Parse(splitYearMonth[0].Trim().Replace("[#]", ""));
                        iMon = int.Parse(splitYearMonth[1].Trim().Replace("[#]", "").Replace("(Today)", ""));
                        iDay = int.Parse(splitColHeader[2].Trim().Replace("[#]", ""));
                        DateValue = new DateTime(iYear, iMon, iDay);

                        _dayofWeek = Convert.ToString((int)DateValue.DayOfWeek);

                        if (e.Cell.Presenter.Content.GetType() == typeof(C1.WPF.DataGrid.DataGridColumnHeaderPresenter))
                        {
                            System.Windows.Controls.ContentControl cc = (e.Cell.Presenter.Content as System.Windows.Controls.ContentControl);
                            switch (_dayofWeek.Trim())
                            {
                                case "6": //토
                                    cc.Foreground = new SolidColorBrush(Colors.Blue);
                                    break;
                                case "0": //일
                                    cc.Foreground = new SolidColorBrush(Colors.Red);
                                    break;
                                default:
                                    cc.Foreground = new SolidColorBrush(Colors.Black);
                                    break;
                            }
                        }
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        int Startcol = dgList.Columns["DOWNTIME"].Index;
                        for (int i = Startcol - 1; i < dgList.Columns.Count; i++)
                        {
                            if (e.Cell.Column.Name.Contains("P_"))
                            {
                                string[] splitDay = e.Cell.Column.Name.Split('_');
                                _Day = splitDay[1];
                                _colPName = "P_" + _Day;

                                DataRow[] dr = dtTemp.Select("AREAID = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "AREAID")) + "' AND " +
                                                             "EQSGID = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "EQSGID")) + "' AND " +
                                                             "PROCID = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "PROCID")) + "' AND " +
                                                             "EQPTID = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "EQPTID")) + "' AND " +
                                                             _colPName + " = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, _colPName)) + "'");

                                if (dr.Length > 0)
                                {
                                    foreach (DataRow row in dr)
                                    {
                                        _colName = "PR_" + _Day;
                                        string[] _sValueToFind = Util.NVC(row[_colName]).Split(':');

                                        if (_sValueToFind.Length > 1)
                                        {
                                            foreach (DataRow dRow in dtColor.Rows)
                                                if (string.Equals(_sValueToFind[0], dRow["CBO_CODE"]))
                                                    ValueToColor = Util.NVC(dRow["ATTRIBUTE1"]);

                                            if (_sValueToFind[0].Equals("Y"))       // 공통코드 수정으로 기존Data 'Y'  Pass로 처리
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                            else if (_sValueToFind[0].Equals("F"))  // 공통코드 수정으로 기존Data 'F'  Non Pass로 처리
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                            else if (!ValueToColor.Equals(string.Empty))
                                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(ValueToColor));

                                            if (!string.IsNullOrEmpty(_sValueToFind[1]))
                                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(_sValueToFind[1]));
                                            else
                                                e.Cell.Presenter.Background = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.Rows.Count <= 3)
                return;
            
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);
            if (cell.Row.Type == DataGridRowType.Top) { return; }

            if (cell != null && Util.NVC(cell.Column.Name).StartsWith("P_"))
            {
                if (!string.IsNullOrEmpty(cell.Text))
                {
                    foreach (DataRow dRow in getCommoncode("RESOURCE_QUALITY_PROCESS_STEP").Rows)
                    {
                        if (string.Equals(cell.Text, dRow["CBO_CODE"]))
                        {
                            callUpdate();
                            return;
                        }                            
                    }
                }
                callCreate();
            }
        }

        private void dgList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

            if (cell == null || (cell.Row.Type == DataGridRowType.Top)) { return; }

            if (cell != null && Util.NVC(cell.Column.Name).StartsWith("P_"))
            {
                if (!string.IsNullOrEmpty(cell.Text))
                {
                    foreach (DataRow dRow in getCommoncode("RESOURCE_QUALITY_PROCESS_STEP").Rows)
                    {
                        if (string.Equals(cell.Text, dRow["CBO_CODE"]))
                        {
                            btnUpdate.IsEnabled = true;
                            return;
                        }                        
                    }
                }
                btnUpdate.IsEnabled = false;
            }
        }
        #endregion

        #region Mehod

        private DataTable getCommoncode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType; 
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }
        private bool validationGrid(C1DataGrid dg)
        {
            try
            {
                bool bRet = false;

                if (dg.Rows.Count <= 3)
                    return bRet;

                if (dg.CurrentRow == null)
                    return bRet;

                if (!dg.CurrentColumn.Name.Contains("P_"))
                    return bRet;

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void callCreate()
        {
            COM001.COM001_325_CREATE _QCreate = new COM001.COM001_325_CREATE();

            _QCreate.FrameOperation = FrameOperation;

            if (_QCreate != null)
            {
                object[] Parameters = new object[8];
                string _ValueToDate = string.Empty;
                string _Year = string.Empty;
                string _Month = string.Empty;
                string _Day = string.Empty;
                string _ValuetoPlanDate = string.Empty;

                string[] splitColHeader = dgList.CurrentColumn.ActualGroupHeader.ToString().Split(',');
                string[] splitYearMonth = splitColHeader[0].Split('.');
                _Year = splitYearMonth[0].Trim().Replace("[#]", "").Trim();
                _Month = string.Concat('0', splitYearMonth[1].Trim());
                _Day = string.Concat('0', splitColHeader[2].Trim());

                _ValueToDate = _Year + "-" + _Month.Substring(_Month.Length - 2) + "-" + _Day.Substring(_Day.Length - 2);
                _ValuetoPlanDate = _Year + _Month.Substring(_Month.Length - 2) + _Day.Substring(_Day.Length - 2);

                Parameters[0] = Util.GetCondition(cboArea);
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQSGID"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQSGNAME"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQPTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQPTNAME"));
                Parameters[5] = _ValueToDate;
                Parameters[6] = _ValuetoPlanDate;
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PROCID"));

                C1WindowExtension.SetParameters(_QCreate, Parameters);

                _QCreate.Closed += new EventHandler(QCreate_Closed);
                _QCreate.ShowModal();
                _QCreate.CenterOnScreen();
            }
        }

        private void callUpdate()
        {
            COM001.COM001_325_UPDATE _QUpdate = new COM001.COM001_325_UPDATE();

            _QUpdate.FrameOperation = FrameOperation;

            if (_QUpdate != null)
            {
                object[] Parameters = new object[9];
                string _ValueToDate = string.Empty;
                string _Year = string.Empty;
                string _Month = string.Empty;
                string _Day = string.Empty;
                string _colName = string.Empty;

                string[] splitColHeader = dgList.CurrentColumn.ActualGroupHeader.ToString().Split(',');
                string[] splitYearMonth = splitColHeader[0].Split('.');
                _Year = splitYearMonth[0].Trim().Replace("[#]", "").Trim();
                _Month = string.Concat('0', splitYearMonth[1].Trim());
                _Day = string.Concat('0', splitColHeader[2].Trim());

                _ValueToDate = _Year + "-" + _Month.Substring(_Month.Length - 2) + "-" + _Day.Substring(_Day.Length - 2);
                _colName = "P_" + _Util.Right(_Day,2);

                Parameters[0] = Util.GetCondition(cboArea);
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQSGID"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQSGNAME"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQPTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "EQPTNAME"));
                Parameters[5] = _ValueToDate;
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, _colName));
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRODID"));
                Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRJT_NAME"));

                C1WindowExtension.SetParameters(_QUpdate, Parameters);

                _QUpdate.Closed += new EventHandler(QUpdate_Closed);
                _QUpdate.ShowModal();
                _QUpdate.CenterOnScreen();
            }
        }

        private void QCreate_Closed(object sender, EventArgs e)
        {
            COM001_325_CREATE Window = sender as COM001_325_CREATE;
            if (Window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
        }

        private void QUpdate_Closed(object sender, EventArgs e)
        {
            COM001_325_UPDATE Window = sender as COM001_325_UPDATE;
            if (Window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
        }

        private void SetGridDate()
        {
            System.DateTime dtNow = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, 1); 

            for (int i = 1; i <= dtNow.AddMonths(1).AddDays(-1).Day; i++)
            {
                string dayColumnName = string.Empty;

                if (i < 10)
                {
                    dayColumnName = "P_0" + i.ToString();
                }
                else
                {
                    dayColumnName = "P_" + i.ToString();
                }

                List<string> sHeader = new List<string>() { dtNow.Year.ToString() + "." + dtNow.Month.ToString(), dtNow.AddDays(i - 1).ToString("ddd"), i.ToString() };

                if (DateTime.Now.ToString("yyyyMMdd") == dtNow.Year.ToString() + dtNow.Month.ToString("00") + dtNow.AddDays(i - 1).ToString("dd")) 
                {
                    List<string> sHeader_Today = new List<string>() { dtNow.Year.ToString() + "." + dtNow.Month.ToString(), dtNow.AddDays(i - 1).ToString("ddd") + " (Today)", i.ToString() };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, 70, HorizontalAlignment.Center, Visibility.Visible);
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, 70, HorizontalAlignment.Center, Visibility.Visible);
                }
            }
        }

        private void SearchData()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                for (int i = dgList.Columns.Count; i-- > 13;)
                    dgList.Columns.RemoveAt(i);

                SetGridDate();   

                string _ValueToMonth = string.Format("{0:yyyy-MM}", dtpDateMonth.SelectedDateTime) + "-01";

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = _ValueToMonth;
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                Indata["PROCID"] = cboProcess.SelectedItemsToString;
                Indata["EQPTID"] = cboEquipment.SelectedItemsToString;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROD_EQPT_QLTY_ARRP", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    if (result != null && result.Rows.Count > 0)
                    {
                        dtTemp = result.Select().CopyToDataTable();
                        Util.GridSetData(dgList, result, FrameOperation);

                        string[] sColumnName = new string[] {"EQSGID", "EQSGNAME", "PROCID", "PROCNAME", "GRP_CODE1", "EQPTID", "EQPTNAME", "GRP_CODE2", "PRJT_NAME", "GRP_CODE3", "PRODID", "GRP_CODE4" };
                        _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    }

                    for (int i = 0; i < dgList.GetRowCount(); i++)
                    {
                        Scrolling_Into_Today_Col();
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SearchDataHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                string _ValueToMonth = string.Format("{0:yyyy-MM}", dtpDateMonth.SelectedDateTime) + "-01";

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = _ValueToMonth;
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                Indata["PROCID"] = cboProcess.SelectedItemsToString;
                Indata["EQPTID"] = cboEquipment.SelectedItemsToString;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROD_EQPT_QLTY_ARRP_HIST", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgHistory, result, FrameOperation);

                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void Scrolling_Into_Today_Col()
        {
            string today_Col_Name = "P_" + DateTime.Now.ToString("dd");

            if (dgList.Columns[today_Col_Name] != null)
            {
                if (DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime) ||
                    DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime))
                {
                    dgList.ScrollIntoView(2, dgList.Columns[today_Col_Name].Index);
                }
            }
        }
        #endregion
    }
}
