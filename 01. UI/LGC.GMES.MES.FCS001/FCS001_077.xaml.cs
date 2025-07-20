/*************************************************************************************
 Created Date : 2020.12.29
      Creator : Kang Dong Hee
   Decription : Aging S/C 호기별 Tray 수량
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.29  NAME : Initial Created
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2022.02.23  KDH : 조회조건 오류성 수정
  2022.05.23  이정미 : EQP_NAME 컬럼 그룹화 
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_077 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        Util _Util = new Util();

        public FCS001_077()
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
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            string[] sFilter = { "FORM_AGING_TYPE_CODE" };
            ComCombo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter);

            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "MODEL");
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

        //20220223_조회조건 오류성 수정 START
        //private void cboAgingType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        //{
        //    if (cboAgingType.SelectedIndex > -1)
        //    {
        //        GetCommonCode();
        //    }
        //}
        //20220223_조회조건 오류성 수정 END

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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CNT_BY_AGING", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingTray, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "EQP_NAME" };
                _Util.SetDataGridMergeExtensionCol(dgAgingTray, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }
        #endregion

        //20220223_조회조건 오류성 수정 START
        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAgingType.SelectedIndex > -1)
            {
                GetCommonCode();
            }

        }
        //20220223_조회조건 오류성 수정 END
    }
}
