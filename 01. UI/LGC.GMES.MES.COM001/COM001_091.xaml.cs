/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 김동일K INS
   Decription : GMES 패키지 TRAY 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  김동일 : Initial Created.
   
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_091.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_091 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        public COM001_091()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            DateTime dtNowTime = System.DateTime.Now;
            if (dtpDateFrom != null)
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            if (tmedtFrom != null)
                tmedtFrom.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);

            if (dtpDateTo != null)
                dtpDateTo.SelectedDateTime = dtNowTime;
            if (tmedtTo != null)
                tmedtTo.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter1 = { Process.PACKAGING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1);
        }
        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
            InitializeControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HiddenLoadingIndicator();
                return;
            }

            GetTrayList();
        }

        private void dgTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    //Grid Data Binding 이용한 Background 색 변경
            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dgTray_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;                

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region Mehod        

        #region [BizCall]
        private void GetTrayList()
        {
            try
            {
                Util.gridClear(dgTray);

                ShowLoadingIndicator();

                //DoEvents();

                DateTime dtFromTime;
                DateTime dtToTime;
                TimeSpan spn;
                if (tmedtFrom.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtFrom.Value);
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);
                }
                
                if (tmedtTo.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtTo.Value);
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                }
                
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STDTTM", typeof(DateTime));
                inTable.Columns.Add("EDDTTM", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;                
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["STDTTM"] = dtFromTime;
                newRow["EDDTTM"] = dtToTime;


                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_BY_CRDTTM_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgProductLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgTray, searchResult, FrameOperation, false);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            // 조회 날짜 최대 체크.
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 15)
            {
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "15");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        #endregion

    }
}
