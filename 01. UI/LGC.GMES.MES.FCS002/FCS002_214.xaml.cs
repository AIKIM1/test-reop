/*************************************************************************************
 Created Date : 2022.12.07
      Creator : 
   Decription : Grader 실적관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.07  DEVELOPER : Initial Created.
 
**************************************************************************************/
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
    public partial class FCS002_214 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        Util _Util = new Util();

        public FCS002_214()
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel, cboRoute };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

         // C1ComboBox[] cboLaneChild = { cboLane };
         // string[] sFilter = { "1" };   //EQPT_GR_TYPE_CODE 참고. (1:Formation)
         // _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilter);
            string[] sFilter = { "5", null,"M" };
            _combo.SetCombo(cboEQP, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);

         // C1ComboBox[] cboEqpParent = { cboLane };
         // _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboEqpParent);
          
            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent, cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter2 = { "COMBO_PROC_INFO_BY_DATE_SEARCH_CONDITION" }; //E07
            _combo.SetCombo(cboSearch, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { "FORM_SEARCH_WIPSTATE" };//FIN_CD
            _combo.SetCombo(cboFinCD, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);

            string[] sFilter4 = { "FORM_SEARCH_ORDERBY" };  //E09
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter4);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();

                SetWorkResetTime();

                dtpFromDate.SelectedDateTime = GetJobDateFrom();
                dtpFromTime.DateTime = GetJobDateFrom();
                dtpToDate.SelectedDateTime = GetJobDateTo();
                dtpToTime.DateTime = GetJobDateTo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            
        }

        private void dgList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null)
                    return;

                if (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["LOTID"]).Equals("Y") || Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["LOTID"]).Equals("N"))
                {
                    e.Row.DataGrid.RowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    e.Row.DataGrid.RowForeground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

                if (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["WIPSTAT"]).Equals("PROC"))
                {
                    e.Row.DataGrid.RowBackground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                
            }));
        }

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgList.ItemsSource == null)
                    return;

                if (sender == null)
                    return;

                if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("CSTID"))
                {
                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    //Tray 정보조회
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkMORE_Checked(object sender, RoutedEventArgs e)
        {
            dgList.Columns["OTHER_F11"].Visibility = Visibility.Visible;
            dgList.Columns["OTHER_DIS"].Visibility = Visibility.Visible;
            
        }
        private void chkMORE_Unchecked(object sender, RoutedEventArgs e)
        {
            dgList.Columns["OTHER_F11"].Visibility = Visibility.Collapsed;
            dgList.Columns["OTHER_DIS"].Visibility = Visibility.Collapsed;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                Util.gridClear(dgList);

                DataTable dtRslt = new DataTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LANEID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("MDL_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboSearch), typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboFinCD), typeof(string));
                dtRqst.Columns.Add(Util.GetCondition(cboOrder), typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GR_TYPE_CODE"] = "5"; // 특성 측정기
                dr["EQPTID"] = Util.GetCondition(cboEQP, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
             // dr["LANEID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");

                if (!string.IsNullOrEmpty(txtProdLot.Text))
                    dr["PROD_LOTID"] = Util.GetCondition(txtProdLot, bAllNull: true);

                dr["MDL_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);

                dr[Util.GetCondition(cboSearch)] = "Y";
                dr[Util.GetCondition(cboFinCD)] = "Y";
                dr[Util.GetCondition(cboOrder)] = "Y";

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                if (chkMORE.IsChecked == true)
                {
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_NEW_MB", "RQSTDT", "RSLTDT", dtRqst);
                }
                else
                {
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_EQP_RESULT_MB", "RQSTDT", "RSLTDT", dtRqst);
                }

                if (dtRslt.Rows.Count > 0)
                {
                    //행 추가(합계, 비율)
                    CreateSumRow(dtRslt);
                }
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

        private void CreateSumRow(DataTable dt)
        {
            try
            {
                //DataTable preTable = DataTableConverter.Convert(dgList.ItemsSource);
                //합계 Row용
                DataTable dtSum = dt.Clone();
                DataRow drSum = dtSum.NewRow();
                drSum["LOTID"] = "Y";
                drSum["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");

                //비율 Row용
                DataTable dtRate = dt.Clone();
                DataRow drRate = dtRate.NewRow();
                drRate["LOTID"] = "N";
                drRate["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("비율");

                //합계
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].DataType == System.Type.GetType("System.Decimal"))
                    {
                        drSum[dt.Columns[i].ColumnName.ToString()] = Convert.ToDecimal(dt.Compute("Sum(" + dt.Columns[i].ColumnName.ToString() + ")", ""));

                        if (Convert.ToDecimal(dt.Compute("Sum(" + dt.Columns[i].ColumnName.ToString() + ")", "")) == 0)
                        {
                            dgList.Columns[dt.Columns[i].ColumnName.ToString()].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                //row 추가
                dtSum.Rows.Add(drSum);

                //비율
                for (int i = dtSum.Columns["INPUT_SUBLOT_QTY"].Ordinal; i < dtSum.Columns.Count; i++)
                {
                    if (dtSum.Columns[i].DataType == System.Type.GetType("System.Decimal"))
                    {
                        drRate[dtSum.Columns[i].ColumnName.ToString()] = string.Format("{0:0.00} ", Convert.ToDecimal(Convert.ToDecimal(dtSum.Rows[0][i].ToString()) / Convert.ToDecimal(dtSum.Rows[0]["INPUT_SUBLOT_QTY"].ToString())) * 100);
                    }
                }

                dtRate.Rows.Add(drRate);

                dt.Merge(dtSum);
                dt.Merge(dtRate);

                dgList.ItemsSource = DataTableConverter.Convert(dt);
            
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
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