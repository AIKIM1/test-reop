/*************************************************************************************
 Created Date : 2023.10.26
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.26  성민식 : Initial Created.
  2023.11.08  성민식 : 날짜 형식 24시간제로 변경
 
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
using System.Linq;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private readonly System.Windows.Threading.DispatcherTimer dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        public MON001_018()
        {
            InitializeComponent();

            InitCombo();

            InitTimer();

            ClearValue();
            GetResult();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            string[] items = new string[]{ "ALL", "EQ", "UI"};
            cboType.ItemsSource = items;
            cboType.SelectedValue = "EQ";
        }
        private void InitTimer()
        {
            if (dispatcherMainTimer != null)
            {
                int sec = 0;

                if (chkAuto.IsChecked == true)
                    sec = 60;

                dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                dispatcherMainTimer.Interval = new TimeSpan(0, 0, sec);
            }
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void txtMin_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckDecimal(txtMin.Text.Trim(), 1, 100))
                {
                    txtMin.Text = "5";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }
        private void chkAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dispatcherMainTimer != null)
                {
                    dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (chkAuto.IsChecked == true)
                        iSec = 60;

                    if (iSec == 0)
                    {
                        dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherMainTimer.Start();
                    
                    //자동조회 %1초로 변경 되었습니다.
                    Util.MessageValidation("SFU5127", Convert.ToString(iSec));
              
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    C1DataGrid dg = sender as C1DataGrid;

                    if (dg == null) return;

                    C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                    if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                    if (Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "DB_NAME")).Equals("")
                    || string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "DB_NAME"))))
                        return;

                    if (dg.CurrentColumn.Name == "C1" || dg.CurrentColumn.Name == "C2" || dg.CurrentColumn.Name == "C3")
                    {
                        string sDBName = string.Empty;
                        string sFromDate = string.Empty;
                        string sToDate = string.Empty;

                        int min = Convert.ToInt32(Util.GetCondition(txtMin));
                        DateTime basDttm = Convert.ToDateTime(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "BAS_DTTM"));

                        sDBName = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "DB_NAME"));

                        if (dg.CurrentColumn.Name == "C1")
                        {
                            sFromDate = (basDttm.AddMinutes(min * -1)).ToString("yyyy/MM/dd HH:mm:ss");
                            sToDate = basDttm.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        else if (dg.CurrentColumn.Name == "C2")
                        {
                            sFromDate = (basDttm.AddMinutes(min * -2)).ToString("yyyy/MM/dd HH:mm:ss");
                            sToDate = (basDttm.AddMinutes(min * -1)).ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        else if (dg.CurrentColumn.Name == "C3")
                        {
                            sFromDate = (basDttm.AddMinutes(min * -3)).ToString("yyyy/MM/dd HH:mm:ss");
                            sToDate = (basDttm.AddMinutes(min * -2)).ToString("yyyy/MM/dd HH:mm:ss");
                        }

                        Util.gridClear(dgDetail);
                        GetDetail(sDBName, sFromDate, sToDate);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    ClearValue();
                    GetResult();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }
        #endregion

        #region Mehod
        private void GetResult()
        {
            try
            {
                Util.gridClear(dgResult);
                string sBizName = "BR_COM_MES_MONT_TIMEOUT";
                DataSet dsRqst = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("INTERVAL", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = Util.GetCondition(cboType) == "ALL" ? null : Util.GetCondition(cboType);
                dr["INTERVAL"] = Util.GetCondition(txtMin);

                dtRqst.Rows.Add(dr);
                dsRqst.Tables.Add(dtRqst);
                
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTDATA,OUTLIST", dsRqst);
                dgResult.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);

                SetTime();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetDetail(string dbName, string fDate, string tDate)
        {
            try
            {
                Util.gridClear(dgDetail);
                string sBizName = "BR_COM_MES_MONT_TIMEOUT_DETL";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("DB_NAME", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DB_NAME"] = dbName;
                dr["FROM_DATE"] = fDate;
                dr["TO_DATE"] = tDate;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgDetail, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private bool CheckDecimal(string txt, int minValue, int maxValue)
        {
            bool bReturn = false;

            //공백 체크
            if(String.IsNullOrEmpty(txt))
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2581"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bReturn;
            }

            //1 ~ 100 사이의 정수만 입력 가능
            decimal value;
            if (!decimal.TryParse(txt, out value))
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2581"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bReturn;
            }
            if(minValue > value || value > maxValue)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2581"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bReturn;
            }

            bReturn = true;
            return bReturn;
        }
        private void ClearValue()
        {
            Util.gridClear(dgResult);
            Util.gridClear(dgDetail);
        }
        private void SetTime()
        {
            try
            {
                DateTime time = new DateTime();
                DataTable dt = ((DataView)dgResult.ItemsSource).Table;

                foreach(DataRow dr in dt.Rows)
                {
                    time = Convert.ToDateTime(Util.NVC(dr["BAS_DTTM"]));
                    break;
                }

                int min = Convert.ToInt32(Util.GetCondition(txtMin));

                dgResult.Columns["C1"].Header = (time.AddMinutes(min * -1)).ToString("HH:mm:ss") + " ~ " + time.ToString("HH:mm:ss");
                dgResult.Columns["C2"].Header = (time.AddMinutes(min * -2)).ToString("HH:mm:ss") + " ~ " + (time.AddMinutes(min * -1)).ToString("HH:mm:ss");
                dgResult.Columns["C3"].Header = (time.AddMinutes(min * -3)).ToString("HH:mm:ss") + " ~ " + (time.AddMinutes(min * -2)).ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                    if (dg == null) return;
                    if (e.Cell.Presenter == null) return;
                    
                    int chk = 5;
                    
                    if(e.Cell.Column.Name != null && e.Cell.Value != null)
                    {
                        if(e.Cell.Column.Name.Contains("C1") || e.Cell.Column.Name.Contains("C2") || e.Cell.Column.Name.Contains("C3"))
                        {
                            if (!String.IsNullOrEmpty(e.Cell.Value.ToString()))
                            {
                                if(Int16.Parse(e.Cell.Value.ToString()) >= chk)
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }));
        }
        #endregion
    }
}
