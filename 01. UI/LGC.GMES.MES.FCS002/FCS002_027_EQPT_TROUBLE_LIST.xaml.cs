/*************************************************************************************
 Created Date : 2023.01.25
      Creator : 
   Decription : 설비Trouble List
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.25  DEVELOPER : Initial Created.
  2025.05.15  이준영    :  MES2.0 전환 사용자 확인 BR 변경  

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
    public partial class FCS002_027_EQPT_TROUBLE_LIST : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        Util _Util = new Util();
        public FCS002_027_EQPT_TROUBLE_LIST()
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

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLaneChild = { cboEqpKind };
            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilter);

            C1ComboBox[] cboEqpKindParent = { cboLane };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbParent: cboEqpKindParent);

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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = this.FrameOperation.Parameters;

                SetWorkResetTime();
                InitControl();
                InitCombo();


                dtpFromDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[0]));
                dtpFromTime.DateTime = Util.StringToDateTime(dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[1]), "yyyyMMdd HHmmss");
                dtpToDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[2]));
                dtpToTime.DateTime = Util.StringToDateTime(dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[3]), "yyyyMMdd HHmmss");


                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("S70", typeof(string));
                inDataTable.Columns.Add("S71", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                newRow["S71"] = Util.GetCondition(cboLane, bAllNull: true);
                newRow["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                newRow["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                newRow["TO_DATE"] = Util.GetCondition(dtpToDate);

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_TROUBLE_LIST_MB", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count != 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                }
                Util.GridSetData(dgEqpTroubleList, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void BtnUserConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQPTID");
                dtRqst.Columns.Add("EQPT_ALARM_CODE");
                dtRqst.Columns.Add("USERID");

                for (int i = 0; i < dgEqpTroubleList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                        && !Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "WRKR_CHK_FLAG")).Equals("Y"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "EQPTID"));
                        dr["EQPT_ALARM_CODE"] = Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "EQPT_ALARM_CODE"));
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }
                }
                if (dtRqst.Rows.Count == 0) Util.MessageInfo("FM_ME_0165"); //선택된 데이터가 없습니다.
                else
                {
                    Util.MessageConfirm("FM_ME_0282", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TC_TROUBLE", "RQSTDT", null, dtRqst);
                            GetList();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpTroubleList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            if (e.Cell == null) return;

            C1DataGrid dataGrid = e.Cell.DataGrid;

            if (e.Cell.Column.Name.Equals("CHK"))
            {
                string userC = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRKR_CHK_FLAG"));
                if (userC.Equals("Y"))
                {
                    dataGrid.Rows[e.Cell.Row.Index].Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgEqpTroubleList.Rows)
            {
                string wrkrChkFlag = "";
                wrkrChkFlag = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WRKR_CHK_FLAG"));
                if (wrkrChkFlag.Equals("N"))
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgEqpTroubleList.Rows)
            {
                string wrkrChkFlag = "";
                wrkrChkFlag = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WRKR_CHK_FLAG"));
                if (wrkrChkFlag.Equals("N"))
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
            }

        }

        private void dgEqpTroubleList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("CHK"))
            {
                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRKR_CHK_FLAG")).Equals("Y"))
                {
                    e.Cancel = true;
                }

            }
        }

        private void dgEqpTroubleList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboEqpKindParent = { cboLane };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbParent: cboEqpKindParent);
        }

        private void cboEqpKind_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
            string[] sFilter1 = { null, "M" };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboEqpParent, sFilter: sFilter1);
        }
    }
}

