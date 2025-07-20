/*************************************************************************************
 Created Date : 2022.12.14
      Creator : Kang Dong Hee
   Decription : Aging S/C 호기별 Tray 수량
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  NAME : Initial Created
**************************************************************************************/
#define SAMPLE_DEV

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
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_077 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        Util _Util = new Util();

        public FCS002_077()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            string[] sFilter = { "FORM_AGING_TYPE_CODE" };
            ComCombo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter);

            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "MODEL");
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
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

        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAgingType.SelectedIndex > -1)
            {
                GetCommonCode();
            }

        }

        private void dgAgingTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                Util.gridClear(dgAgingTray);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrEmpty(Util.GetCondition(cboAgingType, bAllNull: true)))
                {
                    dr["S70"] = EQPT_GR_TYPE_CODE;
                    dr["S71"] = LANE_ID;
                }
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["CMCDTYPE"] = "EQPT_GR_TYPE_CODE";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CNT_BY_AGING_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingTray, dtRslt, FrameOperation, true);

                //string[] sColumnName = new string[] { "EQP_NAME" };
                //_Util.SetDataGridMergeExtensionCol(dgAgingTray, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }
        #endregion

    }
}
