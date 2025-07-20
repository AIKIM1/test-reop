/*************************************************************************************
 Created Date : 2020.07.13
      Creator : 
   Decription : 원자재 특채 처리
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.13  : Initial Created.
  2022.12.14  정재홍 : C20221122-000646 - Request for adding check box for used foil lot
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_103 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string CSTStatus = string.Empty;

        public MTRL001_103()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SetEvent();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void init()
        {
            txtUserName.Text = string.Empty;
            Util.gridClear(dgSearch);
            Util.gridClear(dgSearchList);
        }
        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            if (dtVldDate != null)
                dtVldDate.SelectedDateTime = (dtVldDate.SelectedDateTime).AddDays(90);

            string[] sFilter1 = { "DMD_EL_OUT" };
            combo.SetCombo(cboMtgr, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        #region Event

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }

            SearchData();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearchList, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }
            if (string.IsNullOrEmpty(Util.NVC(txtUserName.Text)) || string.IsNullOrEmpty(Util.NVC(txtUserName.Tag)))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return;
            }
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    savePallet();
                }
            });
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dgSearch_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgSearch_UnloadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                    dtTo.Columns.Add("PLLT_ID", typeof(string));
                    dtTo.Columns.Add("SUPPLIER_LOTID", typeof(string));
                    dtTo.Columns.Add("MTRLID", typeof(string));
                    dtTo.Columns.Add("MTRLNAME", typeof(string));
                }

                if (dtTo.Select("PLLT_ID = '" + DataTableConverter.GetValue(cb.DataContext, "PLLT_ID") + "' AND " +
                                "SUPPLIER_LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "SUPPLIER_LOTID") + "'").Length > 0) //중복조건 체크
                {
                    return;
                }

                DataRow dr = dtTo.NewRow();
                foreach (DataColumn dc in dtTo.Columns)
                {
                    if (dc.DataType.Equals(typeof(Boolean)))
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    else
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                    }
                }

                dtTo.Rows.Add(dr);
                dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);
                DataRow[] dr = dtTo.Select();

                if (dr.Length > 0)
                {
                    dtTo.Rows.Remove(dr[0]);
                }
                dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkVldFlag_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkVldFlag.IsChecked)
            {
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
            }
            else
            {
                dtpDateFrom.IsEnabled = true;
                dtpDateTo.IsEnabled = true;
            }
        }

        private void dgSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgSearch.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("SUPPLIER_LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK_VLD")).Equals("N"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    e.Cell.Presenter.Foreground = dgSearch.Foreground;
                    e.Cell.Presenter.FontWeight = dgSearch.FontWeight;
                    e.Cell.Presenter.FontSize = dgSearch.FontSize;
                }
            }));
        }
        #endregion

        #region Funct
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dgSearch.Selection.Clear();

                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                if (dgSearch.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgSearch.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    row["CHK"] = true;
                }
                dgSearch.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                if (dgSearch.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgSearch.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    row["CHK"] = false;
                }
                dgSearch.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchData()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                init();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("MTRLLOTID", typeof(string));
                IndataTable.Columns.Add("INTLOCK_FLAG", typeof(string));
                IndataTable.Columns.Add("CHK_VLD", typeof(string));
                IndataTable.Columns.Add("CHK_FOIL", typeof(string));
                

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FDATE"] = chkVldFlag.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = chkVldFlag.IsChecked == true ? null : dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["MTRLID"] = string.Equals(Util.NVC(txtMaterial.Text), string.Empty) ? null : Util.NVC(txtMaterial.Text);
                Indata["MTRLLOTID"] = string.Equals(Util.NVC(txtSupplyLotID.Text), string.Empty) ? null : Util.NVC(txtSupplyLotID.Text);
                Indata["INTLOCK_FLAG"] = chkVldFlag.IsChecked == true ? "Y" : null;
                Indata["CHK_VLD"] = chkVldDate.IsChecked == true ? "Y" : null;
                Indata["CHK_FOIL"] = chkFoilLot.IsChecked == true ? "Y" : null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_FOR_SPCL", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgSearch, dtMain, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }

        private void savePallet()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                string strIssueDate = dtVldDate.SelectedDateTime.ToString("yyyyMMdd");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PLLT_ID", typeof(string));
                inTable.Columns.Add("SUPPLIER_LOTID", typeof(string));
                inTable.Columns.Add("VLD_DATE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgSearchList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgSearchList, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["PLLT_ID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "PLLT_ID"));
                    newRow["SUPPLIER_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchList.Rows[i].DataItem, "SUPPLIER_LOTID"));
                    newRow["VLD_DATE"] = strIssueDate;
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_UPD_PALLET_FOR_SPCL", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                        //this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                    finally { loadingIndicator.Visibility = Visibility.Collapsed; }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }
        #endregion
    }
}

