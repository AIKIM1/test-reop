/*************************************************************************************
 Created Date : 2020.12.07
      Creator : 이정미
   Decription : Aging Rack 사용률 Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.05.30  이정미 : Initial Created
  2023.09.11  조영대 : IWorkArea 추가
  2023.10.02  조영대 : Merge 바인딩값으로 변경
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_130 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        #endregion

        #region [Initialize]
        ArrayList alSumCol = new ArrayList();
        Util _Util = new Util();
        public FCS001_130()
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
            CommonCombo_Form ComCombo = new CommonCombo_Form();     
            ComCombo.SetCombo(cboBldg, CommonCombo_Form.ComboStatus.NONE, sCase: "ALLAREA");  
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
                dtRqst.Columns.Add("AREAID", typeof(string)); //2021.04.09 조회조건 AREAID 추가
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2021.04.09 조회조건 AREAID 추가
                //dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                //dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RECIPE_REPORT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingReport, dtRslt, FrameOperation, true);
                if (dtRslt.Rows.Count > 0)
                {
                   //////////////합계 추가하기 ////////////////////////////
                    //alSumCol.Add("Grp")
                    
                    string[] sColumnName = new string[] { "ROUTID", "PROCNAME", "PROCID" }; //바인딩 값으로 변경하기 
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

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
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

    }
}
