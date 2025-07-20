/*************************************************************************************
 Created Date : 2022.12.13
      Creator : Kang Dong Hee
   Decription : Box 유지보수
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.13  NAME : Initial Created
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
    public partial class FCS002_072 : UserControl, IWorkArea
    {
        private string _LANEID = string.Empty;

        public IFrameOperation FrameOperation {
            get;
            set;
        }

        public FCS002_072()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;
            if (tmps != null && tmps.Length >= 1)
            {
                _LANEID = Util.NVC(tmps[0]);
            }

            InitCombo();
            InitControl();

            if (!string.IsNullOrEmpty(_LANEID))
            {
                cboLane.SelectedValue = _LANEID;
                GetList();
            }

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string[] sFilter1 = { "FORM_BOX_GR_MB","Y" };
                _combo.SetCombo(cboEQPGR, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter1, sCase: "SYSTEM_AREA_COMMON_CODE");
                                
                string[] sFilter = { "FORMEQPT_MAINT_STAT_CODE" };
                _combo.SetCombo(cboFlag, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "CMN");                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            try
            {
                dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetLaneCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_BAS_SEL_LANE_CBO_MB";
            //string[] arrColumn = { "LANGID", "EQGRID_LIST", "ONLY_COMMON", "S70_LIST" };
            //string[] arrCondition = { LoginInfo.LANGID, cboEQPGR.SelectedValue.ToString(), null, null };
            //string selectedValueText = "CBO_CODE";
            //string displayMemberText = "CBO_NAME";
            //CommonCombo_Form_MB.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form_MB.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);

            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();


            C1ComboBox[] cboLineParent = { cboEQPGR };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "LANE_BY_EQGR", cbParent: cboLineParent);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("MAINT_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DAY", typeof(string));
                dtRqst.Columns.Add("TO_DAY", typeof(string));
                dtRqst.Columns.Add("S67", typeof(string));
                dtRqst.Columns.Add("S68", typeof(string));
                dtRqst.Columns.Add("EQGRID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S71"] = Util.GetCondition(cboLane);
                dr["MAINT_STAT_CODE"] = Util.GetCondition(cboFlag, bAllNull: true);
                dr["FROM_DAY"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DAY"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["S67"] = Util.GetCondition(cboCol, bAllNull: true);
                dr["S68"] = Util.GetCondition(cboStg, bAllNull: true);
                dr["EQGRID"] = Util.GetCondition(cboEQPGR, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_MNT_DAY_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                Util _Util = new Util();
                string[] sColumnName = new string[] { "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICAL);

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

        public void callOther(string sLaneId, string sCol, string sStg)
        {
            cboLane.SelectedValue = sLaneId;
            cboCol.SelectedValue = sCol;
            cboStg.SelectedValue = sStg;

            btnSearch_Click(null, null);
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

        private void cboLane_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            object[] oParent = { "1,L", cboLane };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROW", objParent: oParent);
            _combo.SetComboObjParent(cboCol, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COL", objParent: oParent);
            _combo.SetComboObjParent(cboStg, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "STG", objParent: oParent);
        }

        private void cboEQPGR_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetLaneCombo(cboLane);
        }
    }
}
