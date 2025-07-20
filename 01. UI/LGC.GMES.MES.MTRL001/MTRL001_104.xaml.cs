/*********************************************************************************************************************************
 Created Date : 2022.04.28
      Creator : 김광오
   Decription : 분리막 방치관리
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2022.04.25        C20220221-000333       김광오           Initial Created.
  2022.04.25        C20220221-000333       김광오           NEW-1 bldg sepa interlock logic (4 h)
***********************************************************************************************************************************/

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
using C1.WPF;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_104 : UserControl, IWorkArea
    {
        #region Declaration
        private CommonCombo combo = new CommonCombo();
        private Util _Util = new Util();

        private string CSTStatus = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        public MTRL001_104()
        {
            InitializeComponent();

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        #region [ Event ] - dtpDateFrom
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
        #endregion

        #region [ Event ] - Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }

            if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return;
            }

            SearchData();
        }
        #endregion

        #region [ Event ] - Grid
        private void dgSearch_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    try
            //    {
            //        if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

            //        if (pre == null) return;

            //        if (string.IsNullOrEmpty(e.Column.Name) == false)
            //        {
            //            if (e.Column.Name.Equals("CHK"))
            //            {
            //                pre.Content = chkAll;
            //                e.Column.HeaderPresenter.Content = pre;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //    }
            //}));
        }

        private void dgSearch_UnloadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    try
            //    {
            //        if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

            //        if (pre == null) return;

            //        if (string.IsNullOrEmpty(e.Column.Name) == false)
            //        {
            //            if (e.Column.Name.Equals("CHK"))
            //            {
            //                pre.Content = chkAll;
            //                e.Column.HeaderPresenter.Content = null;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //    }
            //}));
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

                if (e.Cell.Column.Name.Equals("SUPPLIER_LOTID"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK_VLD")).Equals("N"))
                    {
                        //#FFA500"
                        //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                    }
                    else
                    {
                        //"#5AD26B"
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5AD26B"));
                    }
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

        #region [ Method ] - Search
        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo combo = new CommonCombo();
            //string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            String[] sFilter = { LoginInfo.CFG_AREA_ID, string.Format("{0},{1}", Process.LAMINATION, Process.STACKING_FOLDING) };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_PROC");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            String[] sFilter2 = { string.Format("{0},{1}", Process.LAMINATION, Process.STACKING_FOLDING) };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sFilter: sFilter2, sCase: "PROCESS_BY_PROCID");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            cboEquipment.SelectedIndex = 0;
        }

        private void init()
        {
            Util.gridClear(dgSearch);
        }


        private void SearchData()
        {
            try
            {
                init();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                //IndataTable.Columns.Add("MTRLID", typeof(string));
                //IndataTable.Columns.Add("MTRLLOTID", typeof(string));
                //IndataTable.Columns.Add("INTLOCK_FLAG", typeof(string));
                IndataTable.Columns.Add("CHK_VLD", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                //Indata["MTRLID"] = string.Equals(Util.NVC(txtMaterial.Text), string.Empty) ? null : Util.NVC(txtMaterial.Text);
                //Indata["MTRLLOTID"] = string.Equals(Util.NVC(txtSupplyLotID.Text), string.Empty) ? null : Util.NVC(txtSupplyLotID.Text);
                Indata["CHK_VLD"] = ((bool)chkVldMtrlFlag.IsChecked && (bool)chkNotVldMtrlFlag.IsChecked) ||
                                     (!(bool)chkVldMtrlFlag.IsChecked && !(bool)chkNotVldMtrlFlag.IsChecked) ? null : (bool)chkVldMtrlFlag.IsChecked ? "Y" : (bool)chkNotVldMtrlFlag.IsChecked ? "N" : null;

                Indata["EQPTID"] = "".Equals(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                Indata["PROCID"] = string.Format("{0},{1}", Process.LAMINATION, Process.STACKING_FOLDING);
                IndataTable.Rows.Add(Indata);

                string bizRule = "DA_PRD_SEL_PALLET_FOR_SEPA";

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", IndataTable, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        return;
                    }

                    Util.GridSetData(dgSearch, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ Util ]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion
    }
}

