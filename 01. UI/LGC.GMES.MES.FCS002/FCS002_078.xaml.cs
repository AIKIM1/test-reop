/*************************************************************************************
 Created Date : 2022.12.14
      Creator : Kang Dong Hee
   Decription : FCS-FP 계획연동 Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  NAME : Initial Created
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_078 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        private string sEqpKind = string.Empty;

        public FCS002_078()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            //Combo Setting
            InitCombo();
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
            C1ComboBox[] cboPlantChild = { cboLine };
            ComCombo.SetCombo(cboPlant, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "PLANT", cbChild: cboPlantChild);

            C1ComboBox[] cboLineChild = { cboModel };
            C1ComboBox[] cboLineParent = { cboPlant };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE_SHOPID", cbChild: cboLineChild, cbParent: cboLineParent);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            GetFpProcId();

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", sEqpKind };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterEqpType);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
        }

        // 공통함수로 뺄지 확인 필요 START
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
        // 공통함수로 뺄지 확인 필요 END

        private void GetFpProcId()
        {
            try
            {
                sEqpKind = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FCS_FP_PROCID";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (row["ATTRIBUTE2"].ToString().Contains(LoginInfo.CFG_AREA_ID)) continue;

                    sEqpKind += row["ATTRIBUTE1"].ToString() + ",";
                }

                sEqpKind = sEqpKind.TrimEnd(',');
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgFP_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!e.Cell.Column.Name.Equals("AREAID") && !e.Cell.Column.Name.Equals("PROD_PROCID") && !e.Cell.Column.Name.Equals("EQSGNAME"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MDL_CHG_FLAG")).Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SHOPID"] = Util.GetCondition(cboPlant, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FP_FCS_WORKPLAN_CONNECTION_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgFP, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count > 0)
                {
                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "AREAID", "PROD_PROCID", "EQSGNAME" };
                    _Util.SetDataGridMergeExtensionCol(dgFP, sColumnName, DataGridMergeMode.VERTICAL);

                    dgFP.GroupBy(dgFP.Columns["AREAID"], DataGridSortDirection.None);
                    dgFP.GroupRowPosition = DataGridGroupRowPosition.AboveData;
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
