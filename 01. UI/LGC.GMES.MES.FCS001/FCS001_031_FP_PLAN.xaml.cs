/*************************************************************************************
 Created Date : 2021.01.13
      Creator : kang Dong Hee
   Decription : 상대판정 Cell List
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  NAME : Initial Created
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Globalization;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_031_FP_PLAN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sAREAID;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        public FCS001_031_FP_PLAN()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                sAREAID = Util.NVC(tmps[0]);
            }

            SetCalendar(DateTime.Now);
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA");
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPlan();
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SetCalendar(dtpDateFrom.SelectedDateTime);
        }
        private void dtpDateTo_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;

            if (dtPik == null) return;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                SetCalendar(dtpDateFrom.SelectedDateTime);
                return;
            }
        }
        #endregion

        #region Mehod

        private void SetCalendar(DateTime _datetime)
        {
            DateTime _Today = _datetime; 
            var _culture = new System.Globalization.CultureInfo("pl-PL");

            DayOfWeek _dwFirst = _culture.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek _dwToday = _culture.Calendar.GetDayOfWeek(_Today);

            int idiff = _dwToday - _dwFirst;
            DateTime _Firstday = _Today.AddDays(-idiff);
            DateTime _Lastday = _Firstday.AddDays(6);

            dtpDateFrom.SelectedDateTime = _Today;
            dtpDateTo.SelectedDateTime = _Lastday;

        }
        private void GetPlan()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SDTTM", typeof(DateTime));
                dtRqst.Columns.Add("EDTTM", typeof(DateTime));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["EDTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SFC_FP_DAILY_FORM_PLAN", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgPlan, dtRslt, FrameOperation, false);
                HiddenLoadingIndicator();
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
        #endregion
    }
}
