/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 강동희
   Decription : Aging Rack 사용률 Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  강동희 : Initial Created
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
using System.Collections;
namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_130 : UserControl
    {
        #region [Declaration & Constructor]

        #endregion

        #region [Initialize]
        ArrayList alSumCol = new ArrayList();
        Util _Util = new Util();
        public FCS002_130()
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
            //Combo Setting
            InitCombo();

            this.Loaded -= UserControl_Loaded; // 화면간 이동 시 초기화 현상 제거
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();     
            ComCombo.SetCombo(cboBldg, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");  
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgAgingReport);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RECIPE_REPORT_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingReport, dtRslt, FrameOperation, true);
                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "STD_TIME" , "S_CRANE", "SC_CNT" }; //바인딩 값으로 변경하기 
                    _Util.SetDataGridMergeExtensionCol(dgAgingReport, sColumnName, DataGridMergeMode.VERTICAL);
                }
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

        private void SetLaneCombo(C1ComboBox cbo) 
        {
            const string bizRuleName = "DA_BAS_SEL_LINE_DFLT_LANE_CBO";
            string[] arrColumn = { "LANGID" , "AREAID"};
            string[] arrCondition = { LoginInfo.LANGID, Util.GetCondition(cboBldg, bAllNull: true) };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form_MB.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form_MB.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
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

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void cboBldg_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            cboLane.Text = string.Empty;
            SetLaneCombo(cboLane);
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
