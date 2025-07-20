/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 대기창고 관리 - 이력조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.03  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
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

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_051_HISTORY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_051_HISTORY : C1Window, IWorkArea
    {        
        #region Declaration & Constructor

        string _ProcID = string.Empty;
        string _DateFrom = string.Empty;
        string _DateTo = string.Empty;
        string _WarehouseID = string.Empty;
        string _LineID = string.Empty;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        #endregion        

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_051_HISTORY()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 5)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _DateFrom = Util.NVC(tmps[1]);
                    _DateTo = Util.NVC(tmps[2]);
                    _WarehouseID = Util.NVC(tmps[3]);
                    _LineID = Util.NVC(tmps[4]);
                }
                else
                {
                    _ProcID = string.Empty;
                    _DateFrom = string.Empty;
                    _DateTo = string.Empty;
                    _WarehouseID = string.Empty;
                    _LineID = string.Empty;
                }

                InitCombo();
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            DateTime dateFrom = Convert.ToDateTime(_DateFrom);
            dtpDateFrom.SelectedDateTime = dateFrom;
            teTimeFrom.Value = new TimeSpan(dateFrom.Hour, 0, 0);

            DateTime dateTo = Convert.ToDateTime(_DateTo);
            dtpDateTo.SelectedDateTime = dateTo;
            teTimeTo.Value = new TimeSpan(dateTo.Hour, 0, 0);

            CommonCombo _combo = new CommonCombo();

            String[] sEquipmentSegmentFilter = { LoginInfo.CFG_AREA_ID };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sEquipmentSegmentFilter);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sEquipmentSegmentFilter, sCase: "cboEquipmentSegmentForm");
            cboEquipmentSegment.SelectedValue = _LineID;


            String[] sTrayStatFilter = { LoginInfo.LANGID, "WH_TRAY_STATUS" };
            _combo.SetCombo(cboTrayStat, CommonCombo.ComboStatus.ALL, sFilter: sTrayStatFilter, sCase: "COMMCODES");
        }

        #region [EVENT]

        #region [Main Window]

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);
                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                string dateFrom = string.Format("{0}{1:0#}", dtPik.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, true))
                {
                    dtPik.Text = dtpDateTo.SelectedDateTime.ToLongDateString();
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    teTimeFrom.Value = new TimeSpan(teTimeTo.Value.Value.Ticks);
                    return;
                }
            }));
        }
        private void teTimeFrom_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                C1.WPF.DateTimeEditors.C1TimeEditor teTimePik = (sender as C1.WPF.DateTimeEditors.C1TimeEditor);
                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimePik.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, true))
                {
                    teTimePik.Value = new TimeSpan(teTimeTo.Value.Value.Ticks);
                    return;
                }
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);
                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtPik.SelectedDateTime.ToString("yyyyMMdd"), teTimeTo.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, false))
                {
                    dtPik.Text = dtpDateFrom.SelectedDateTime.ToLongDateString();
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    teTimeTo.Value = new TimeSpan(teTimeFrom.Value.Value.Ticks);
                    return;
                }
            }));
        }
        private void teTimeTo_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                C1.WPF.DateTimeEditors.C1TimeEditor teTimePik = (sender as C1.WPF.DateTimeEditors.C1TimeEditor);
                string dateFrom = string.Format("{0}{1:0#}", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"), teTimeFrom.Value.Value.Hours);
                string dateTo = string.Format("{0}{1:0#}", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"), teTimePik.Value.Value.Hours);
                if (!CanSearchDateTime(dateFrom, dateTo, false))
                {
                    teTimePik.Value = new TimeSpan(teTimeFrom.Value.Value.Ticks);
                    return;
                }
            }));
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GetHistory();
        }

        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GetHistory();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcelSave_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion [Main Window]

        #endregion [EVENT]

        #region [Biz]

        private void GetHistory()
        {
            try
            {
                ShowLoadingIndicator();

                //DataTable inTable = _Biz.GetDA_PRD_SEL_INOUT_HISTORY_STOCK();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DATE_FROM", typeof(string));
                inTable.Columns.Add("DATE_TO", typeof(string));
                inTable.Columns.Add("LINEID", typeof(string));
                inTable.Columns.Add("WH_ID", typeof(string));
                inTable.Columns.Add("INOUT", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _ProcID;
                dr["DATE_FROM"] = string.Format("{0} {1:0#}:00:00", dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeFrom.Value.Value.Hours);
                dr["DATE_TO"] = string.Format("{0} {1:0#}:00:00", dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd"), teTimeTo.Value.Value.Hours);
                dr["WH_ID"] = _WarehouseID;

                dr["LINEID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                if (Util.NVC(dr["LINEID"]).Length < 1 || Util.NVC(dr["LINEID"]).ToUpper().Equals("ALL") || Util.NVC(dr["LINEID"]).ToUpper().Equals("SELECT"))
                    dr["LINEID"] = DBNull.Value;

                dr["INOUT"] = Util.NVC(cboTrayStat.SelectedValue);
                if (Util.NVC(dr["INOUT"]).Length < 1 || Util.NVC(dr["INOUT"]).ToUpper().Equals("ALL") || Util.NVC(dr["INOUT"]).ToUpper().Equals("SELECT"))
                    dr["INOUT"] = DBNull.Value;

                dr["TRAYID"] = txtTrayID.Text;
                dr["PRODID"] = txtProdID.Text;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INOUT_HISTORY_STOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, true);
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

        #endregion [Biz]


        #region [Validation]

        private bool CanSearchDateTime(string dateFrom, string dateTo, bool byFromDate)
        {
            bool bRet = false;

            if (Convert.ToDecimal(dateFrom) > Convert.ToDecimal(dateTo))
            {
                Util.MessageValidation(byFromDate ? "SFU3231" : "SFU3231"); // [SFU3231:종료시간이 시작시간보다 이전입니다.], [SFU1698:시작일자 이전 날짜는 선택할 수 없습니다.]
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [FUNC]

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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSearch);
            listAuth.Add(btnExcelSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion [FUNC]
    }
}
