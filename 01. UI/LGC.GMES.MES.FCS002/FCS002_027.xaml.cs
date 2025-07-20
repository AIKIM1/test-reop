/*************************************************************************************
 Created Date : 2023.01.25
      Creator : 강동희
   Decription : Trouble 분석
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.25  강동희 : Initial Created
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_027 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_027()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            InitControl();
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLaneChild = { cboEqpKind, cboEqp };
            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilter);

            C1ComboBox[] cboEqpKindParent = { cboLane };
            C1ComboBox[] cboEqpKinChild = { cboEqp };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbParent: cboEqpKindParent, cbChild: cboEqpKinChild);

            C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
            string[] sFilter1 = { null, "M" };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboEqpParent, sFilter: sFilter1);
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
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

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        #endregion

        #region Event
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTroubleList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("EQPT_ALARM_NAME") || e.Cell.Column.Name.Equals("EQPT_ALARM_CODE"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgTroubleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            if (dgTroubleList == null || dgTroubleList.CurrentRow == null || (!(dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_NAME") || dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_CODE"))))
                return;

            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                string sLANE_ID = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "LANE_ID")));
                string sS70 = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPTKINDCD")));
                string sEqpID = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPTID")));
                string sTROUBLE_CD = "";

                DateTime sFROM_DATE = dtpFromDate.SelectedDateTime;
                string sFROM_TIME = dtpFromTime.DateTime.Value.ToString("HHmm00");
                DateTime sTO_DATE = dtpToDate.SelectedDateTime;
                string sTO_TIME = dtpToTime.DateTime.Value.ToString("HHmm59");

                if (dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_CODE") || dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_NAME"))
                {
                    sTROUBLE_CD = Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPT_ALARM_CODE"));
                }

                Load_FCS002_027_TROUBLE_DETAIL_LIST(sLANE_ID, sS70, sTROUBLE_CD, sFROM_DATE, sFROM_TIME, sTO_DATE, sTO_TIME, sEqpID);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Load_FCS002_027_TROUBLE_DETAIL_LIST(string sLANE_ID, string sS70,
                                 string sTROUBLE_CD, DateTime sFROM_DATE, string sFROM_TIME, DateTime sTO_DATE, string sTO_TIME, string sEQPTID)
        {
            FCS002_027_TROUBLE_DETAIL_LIST TroubleDetailList = new FCS002_027_TROUBLE_DETAIL_LIST();
            TroubleDetailList.FrameOperation = FrameOperation;

            if (TroubleDetailList != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = sLANE_ID; //sLANE_ID
                Parameters[1] = sS70; //sS70
                Parameters[2] = sTROUBLE_CD; //sTROUBLE_CD
                Parameters[3] = sFROM_DATE; //sFROM_DATE
                Parameters[4] = sFROM_TIME; //sFROM_TIME
                Parameters[5] = sTO_DATE; //sTO_DATE
                Parameters[6] = sTO_TIME; //sTO_TIME
                Parameters[7] = sEQPTID; //sEQPTID
                this.FrameOperation.OpenMenuFORM("FCS002_027_TROUBLE_DETAIL_LIST", "FCS002_027_TROUBLE_DETAIL_LIST", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Alarm 상세 List"), true, Parameters);
            }

        }

        private void btnTrouble_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            try
            {

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
              
                DateTime sFROM_DATE = dtpFromDate.SelectedDateTime;
                string sFROM_TIME = dtpFromTime.DateTime.Value.ToString("HHmm00");
                DateTime sTO_DATE = dtpToDate.SelectedDateTime;
                string sTO_TIME = dtpToTime.DateTime.Value.ToString("HHmm59");

                Load_FCS002_027_EQPT_TROUBLE_LIST(sFROM_DATE, sFROM_TIME, sTO_DATE, sTO_TIME);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void Load_FCS002_027_EQPT_TROUBLE_LIST(DateTime sFROM_DATE, string sFROM_TIME, DateTime sTO_DATE, string sTO_TIME)
        {
            FCS002_027_EQPT_TROUBLE_LIST EqptTroubleList = new FCS002_027_EQPT_TROUBLE_LIST();
            EqptTroubleList.FrameOperation = FrameOperation;
      

            if (EqptTroubleList != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = sFROM_DATE; //sFROM_DATE
                Parameters[1] = sFROM_TIME; //sFROM_TIME
                Parameters[2] = sTO_DATE; //sTO_DATE    
                Parameters[3] = sTO_TIME; //sTO_TIME
             
                this.FrameOperation.OpenMenuFORM("FCS002_027_EQPT_TROUBLE_LIST", "FCS002_027_EQPT_TROUBLE_LIST", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("설비 Alarm List"), true, Parameters);
            }

            
        }

        private void dgTroubleList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANE_ID", typeof(string));
                inDataTable.Columns.Add("S70", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                newRow["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                newRow["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:59");

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_TROUBLE_MB", "INDATA", "OUTDATA", inDataTable);
                //dgTroubleList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgTroubleList, dtRslt, this.FrameOperation, true);
                string[] sColumnName = new string[] { "LANE_NAME", "EQPTKIND", "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgTroubleList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
