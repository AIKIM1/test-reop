/*************************************************************************************
 Created Date : 2023.01.18
      Creator : KANG DONG HEE
   Decription : Aging 한계시간 초과 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.18  DEVELOPER : Initial Created.




 
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        public FCS002_014()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            SetWorkResetTime();

            InitCombo();

            InitControl();

            //dtpFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            //dtpToDate.SelectedDateTime = System.DateTime.Now;

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            string[] sFilter = { "3,4,9", null, "31,41,91", null };
            _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent, sFilter: sFilter);

            string[] sFilter1 = { "AGING_TIME_OVER_TYPE" };
            _combo.SetCombo(cboTimeType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgAgingLimit_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Color 변경
                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
                
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "DUMMY_FLAG")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DUMMY_FLAG")).Equals("Y"))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                    }

                    if (Util.NVC(e.Cell.Column.Name) == "SPECIAL_YN")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "SPECIAL_YN")).Equals("Y"))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.FontFamily = new FontFamily("맑은 고딕");
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.FontWeight = FontWeights.Bold;
                        }
                    }

                    if (Util.NVC(e.Cell.Column.Name) == "TIME_ALARM")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TIME_ALARM")).Equals("Y"))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Background = new SolidColorBrush(Colors.Pink);
                        }
                    }
                }
            }));
        }

        private void dgAgingLimit_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgAgingLimit.ItemsSource == null)
                    return;
                
                if (sender == null)
                    return;

                if (dgAgingLimit.CurrentRow != null && dgAgingLimit.CurrentColumn.Name.Equals("CSTID"))
                {
                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgAgingLimit_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("NDT", typeof(string));
                dtRqst.Columns.Add("AGING_YN", typeof(string));
                dtRqst.Columns.Add("FROM_MI", typeof(decimal));
                dtRqst.Columns.Add("TO_MI", typeof(decimal));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["AGING_YN"] = (chkAging.IsChecked == true ? "Y" : null);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:59");

                if (!string.IsNullOrEmpty(Util.GetCondition(cboTimeType)))
                {
                    if (Util.GetCondition(cboTimeType).Equals("1")) // 0~3hr
                    {
                        dr["FROM_MI"] = 0;
                        dr["TO_MI"] = 3 * 60 - 1;
                    }
                    else if (Util.GetCondition(cboTimeType).Equals("2")) //3~12hr
                    {
                        dr["FROM_MI"] = 3 * 60;
                        dr["TO_MI"] = 12 * 60 - 1;
                    }
                    else if (Util.GetCondition(cboTimeType).Equals("3")) //12~24hr
                    {
                        dr["FROM_MI"] = 12 * 60;
                        dr["TO_MI"] = 24 * 60 - 1;
                    }
                    else if (Util.GetCondition(cboTimeType).Equals("4")) //24hr~
                    {
                        dr["FROM_MI"] = 24 * 60;
                        dr["TO_MI"] = 99999999;
                    }
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_LIMIT_TRAY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgAgingLimit, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
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

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            dJobDate = dJobDate.AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        #endregion

    }
}
