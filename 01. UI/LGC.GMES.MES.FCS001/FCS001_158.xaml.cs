/*************************************************************************************
 Created Date : 2023.05.08
      Creator : 
   Decription : 예약 이력관리
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.08  DEVELOPER : Initial Created.
  2023.05.15  이지은      폴란드 GMES 역전개 프로젝트 신규화면 개발
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

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_158.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_158 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        public FCS001_158()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            SetAreaComCode(cboRsvType);
        }

        private void SetAreaComCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORMLGS_RSV_TYPE";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = drRslt["CBO_NAME"].ToString() + " (" + drRslt["CBO_CODE"].ToString() + ")";
                }
            }
            return dt;
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetWorkResetTime();
                InitCombo();

                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    DateTime nowDate = DateTime.Now;

                    //이전 Form으로 부터 넘겨받은 parameter
                    txtPgmName.Text = Util.NVC(parameters[0]);
                    cboRsvType.SelectedValue = Util.NVC(parameters[1]);

                    dtpFromDate.SelectedDateTime = DateTime.Parse(parameters[2].ToString());
                    dtpFromTime.DateTime = DateTime.Parse(parameters[2].ToString());
                    dtpToDate.SelectedDateTime = DateTime.Parse(parameters[2].ToString());
                    dtpToTime.DateTime = DateTime.Parse(parameters[2].ToString()).AddMinutes(1);

                    //InitControl();
                    GetList();
                }
                else
                {
                    InitControl();
                }
            }
             catch (Exception ex)
             {
                 Util.MessageException(ex);
             }
            this.Loaded -= UserControl_Loaded;
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
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EXEC_TYPE", typeof(string));
                inDataTable.Columns.Add("FROM_TIME", typeof(DateTime));
                inDataTable.Columns.Add("EXEC_PGM_NAME", typeof(string));
                inDataTable.Columns.Add("TO_TIME", typeof(DateTime));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EXEC_TYPE"] = Util.GetCondition(cboRsvType, bAllNull: true);
                newRow["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                if (!string.IsNullOrEmpty(txtPgmName.Text)) newRow["EXEC_PGM_NAME"] = Util.NVC(txtPgmName.Text);
                newRow["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORMLGS_RSV_EXEC_HIST_DETAIL_UI", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count != 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                }
                Util.GridSetData(dgRsvList, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }

        private void dgRsvList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
            foreach (C1.WPF.DataGrid.DataGridRow row in dgRsvList.Rows)
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
            foreach (C1.WPF.DataGrid.DataGridRow row in dgRsvList.Rows)
            {
                string wrkrChkFlag = "";
                wrkrChkFlag = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WRKR_CHK_FLAG"));
                if (wrkrChkFlag.Equals("N"))
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
            }

        }

        private void dgRsvList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
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

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                //sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
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
                //sJobDate = DateTime.Now.ToString("yyyyMMdd");
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
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

