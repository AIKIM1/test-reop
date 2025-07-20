/*************************************************************************************
 Created Date : 2021.04.14
      Creator : PSM
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.14  DEVELOPER : Initial Created.
  2023.01.26  권혜정 : 저전압 실적관리 상세 팝업 : 직행/재작업 구분(RWK_FLAG) 추가
  2023.07.13  이정미 : 생산실적레포트 상세 팝업 - 작업조, 일자 추가(FROM_DATE, TO_DATE 인수 있을 경우 보여짐)
  2023.11.02  최석준 : MDLLOT_ID 조회조건에 추가
  2025.02.26  이지은 : 실적 집계 수량 상세 조회쿼리 분리(집계방식이 달라 EOL,DGS Biz 분리 필요)



**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_112.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_054_DFCT_CELL_LIST : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string WORK_DATE = string.Empty;
        private string SHFT_ID = string.Empty;
        private string MDLLOT_ID = string.Empty;
        private string EQSGID = string.Empty;
        private string EQPTID = string.Empty;
        private string PROD_LOTID = string.Empty;
        private string DIGIT8_LOTID = string.Empty;
        private string DIGIT10_LOTID = string.Empty;
        private string LOTTYPE = string.Empty;
        private string DFCT_CODE = string.Empty;
        private string FORM_RSLT_SUM_GR_CODE = string.Empty;
        private string RWK_FLAG = string.Empty;
        private string ROUT_TYPE_CODE = string.Empty;
        private string LOT_COMNET = string.Empty;
        private string FROM_DATE = string.Empty;
        private string TO_DATE = string.Empty;
        private string WRKLOG_TYPE_CODE = string.Empty;
        private string WRK_TYPE_CODE = string.Empty;

        public FCS001_054_DFCT_CELL_LIST()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null) return;
            else
            {
                WORK_DATE = Util.NVC(tmps[0]);
                SHFT_ID = Util.NVC(tmps[1]);
                MDLLOT_ID = Util.NVC(tmps[2]);
                EQSGID = Util.NVC(tmps[3]);
                EQPTID = Util.NVC(tmps[4]);
                PROD_LOTID = Util.NVC(tmps[5]);
                LOTTYPE = Util.NVC(tmps[6]);
                DFCT_CODE = Util.NVC(tmps[7]);
                WRKLOG_TYPE_CODE = Util.NVC(tmps[8]);
                WRK_TYPE_CODE = Util.NVC(tmps[9]);
                ROUT_TYPE_CODE = Util.NVC(tmps[10]);
                LOT_COMNET = Util.NVC(tmps[11]);
                FROM_DATE = Util.NVC(tmps[12]);
                TO_DATE = Util.NVC(tmps[13]);

                if (tmps.Length >= 16)
                {
                    DIGIT8_LOTID = Util.NVC(tmps[14]);
                    DIGIT10_LOTID = Util.NVC(tmps[15]);

                    if (!string.IsNullOrEmpty(DIGIT8_LOTID) || !string.IsNullOrEmpty(DIGIT10_LOTID))
                    {
                        PROD_LOTID = string.Empty;
                    }
                }

                GetList();
            }
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                string BizRuleID;

                //2025.02.26 Degas 와 Eol은 불량 집계방식이 달라 Bizrule 분리 필요
                if (WRKLOG_TYPE_CODE.Equals("D")|| WRKLOG_TYPE_CODE.Equals("Q"))
                {
                    BizRuleID = "DA_SEL_DFCT_CELL_LIST_DE";
                }
                else
                {
                    BizRuleID = "DA_SEL_DFCT_CELL_LIST";
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID");
                dtRqst.Columns.Add("AREAID"); //(*) 필수
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE"); //(*) 필수
                dtRqst.Columns.Add("WRK_TYPE"); //(*) 필수
                dtRqst.Columns.Add("DFCT_CODE");
                if (!string.IsNullOrEmpty(WORK_DATE)) dtRqst.Columns.Add("WORK_DATE");  
                if (!string.IsNullOrEmpty(FROM_DATE)) dtRqst.Columns.Add("FROM_DATE");
                if (!string.IsNullOrEmpty(TO_DATE)) dtRqst.Columns.Add("TO_DATE");
                if (!string.IsNullOrEmpty(SHFT_ID)) dtRqst.Columns.Add("SHFT_ID"); 
                dtRqst.Columns.Add("EQSGID");
                dtRqst.Columns.Add("MDLLOT_ID");
                dtRqst.Columns.Add("EQPTID");
                dtRqst.Columns.Add("PROD_LOTID");
                dtRqst.Columns.Add("DIGIT8_LOTID");
                dtRqst.Columns.Add("DIGIT10_LOTID");
                dtRqst.Columns.Add("LOTTYPE");
                dtRqst.Columns.Add("ROUT_TYPE_CODE");
                dtRqst.Columns.Add("PRE_DFCT_CODE");

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WRKLOG_TYPE_CODE"] = WRKLOG_TYPE_CODE;
                dr["WRK_TYPE"] = WRK_TYPE_CODE; 
                if (!string.IsNullOrEmpty(DFCT_CODE)) dr["DFCT_CODE"] = DFCT_CODE;
                if (!string.IsNullOrEmpty(WORK_DATE)) dr["WORK_DATE"] = WORK_DATE;
                if (!string.IsNullOrEmpty(FROM_DATE)) dr["FROM_DATE"] = FROM_DATE;
                if (!string.IsNullOrEmpty(TO_DATE)) dr["TO_DATE"] = TO_DATE;
                if (!string.IsNullOrEmpty(SHFT_ID)) dr["SHFT_ID"] = SHFT_ID;
                if (!string.IsNullOrEmpty(EQSGID)) dr["EQSGID"] = EQSGID;
                if (!string.IsNullOrEmpty(MDLLOT_ID)) dr["MDLLOT_ID"] = MDLLOT_ID;
                if (!string.IsNullOrEmpty(EQPTID)) dr["EQPTID"] = EQPTID;
                if (!string.IsNullOrEmpty(PROD_LOTID)) dr["PROD_LOTID"] = PROD_LOTID;
                if (!string.IsNullOrEmpty(DIGIT8_LOTID)) dr["DIGIT8_LOTID"] = DIGIT8_LOTID;
                if (!string.IsNullOrEmpty(DIGIT10_LOTID)) dr["DIGIT10_LOTID"] = DIGIT10_LOTID;
                if (!string.IsNullOrEmpty(LOTTYPE)) dr["LOTTYPE"] = LOTTYPE;
                if (!string.IsNullOrEmpty(ROUT_TYPE_CODE)) dr["ROUT_TYPE_CODE"] = ROUT_TYPE_CODE;
                if (!string.IsNullOrEmpty(LOT_COMNET)) dr["PRE_DFCT_CODE"] = LOT_COMNET;

                dtRqst.Rows.Add(dr);

                // 백그라운드 작업 적용. 쿼리 후 ExecuteDataCompleted 이벤트 실행됨.
                dgCellList.ExecuteService(BizRuleID, "INDATA", "OUTDATA", dtRqst, autoWidth: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]

        private void dgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;

        }

        private void dgCellList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (!string.IsNullOrEmpty(FROM_DATE) || !string.IsNullOrEmpty(TO_DATE))
            {
                dgCellList.Columns["CALDATE"].Visibility = Visibility.Visible;
                dgCellList.Columns["SHFT_ID"].Visibility = Visibility.Visible;
            }

            double dWidthSum = 0;

            foreach (C1.WPF.DataGrid.DataGridColumn item in dgCellList.Columns)
            {
                if (item.Visibility != Visibility.Visible) continue;

                dWidthSum += item.ActualWidth;
            }
            this.Width = dWidthSum + 100;
        }
        #endregion

    }
}
