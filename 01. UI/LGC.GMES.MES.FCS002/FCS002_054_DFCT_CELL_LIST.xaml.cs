/*************************************************************************************
 Created Date : 2021.04.14
      Creator : PSM
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.14  DEVELOPER : Initial Created.





 
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

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_112.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_054_DFCT_CELL_LIST : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string WORK_DATE = string.Empty;
        private string SHFT_ID = string.Empty;
        private string MDLLOT_ID = string.Empty;
        private string EQSGID = string.Empty;
        private string EQPTID = string.Empty;
        private string PROD_LOTID = string.Empty;
        private string LOTTYPE = string.Empty;
        private string DFCT_CODE = string.Empty;
        private string FORM_RSLT_SUM_GR_CODE = string.Empty;
        private string RWK_FLAG = string.Empty;
        private string ROUT_TYPE_CODE = string.Empty;
        private string LOT_COMNET = string.Empty;

        public FCS002_054_DFCT_CELL_LIST()
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
                FORM_RSLT_SUM_GR_CODE = Util.NVC(tmps[8]);
                RWK_FLAG = Util.NVC(tmps[9]);
                ROUT_TYPE_CODE = Util.NVC(tmps[10]);
                LOT_COMNET = Util.NVC(tmps[11]);
                GetList();
            }
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                string BizRuleID = "DA_SEL_DFCT_CELL_LIST";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID");
                dtRqst.Columns.Add("AREAID");
                dtRqst.Columns.Add("WORK_DATE"); //필수
                dtRqst.Columns.Add("SHFT_ID"); //필수
                dtRqst.Columns.Add("FORM_RSLT_SUM_GR_CODE"); //필수
                dtRqst.Columns.Add("EQSGID");
                dtRqst.Columns.Add("EQPTID");
                dtRqst.Columns.Add("PROD_LOTID");
                dtRqst.Columns.Add("LOTTYPE");
                dtRqst.Columns.Add("DFCT_CODE");
                dtRqst.Columns.Add("RWK_FLAG");
                dtRqst.Columns.Add("ROUT_TYPE_CODE");
                dtRqst.Columns.Add("PRE_DFCT_CODE");
                dtRqst.Columns.Add("DEGAS_FRST");
                dtRqst.Columns.Add("DEGAS_RWK");
                dtRqst.Columns.Add("EOL_FRST");
                dtRqst.Columns.Add("EOL_RWK");

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WORK_DATE"] = WORK_DATE;
                dr["SHFT_ID"] = SHFT_ID;
                dr["FORM_RSLT_SUM_GR_CODE"] = FORM_RSLT_SUM_GR_CODE;

                if (FORM_RSLT_SUM_GR_CODE.Contains("DEGAS") || FORM_RSLT_SUM_GR_CODE.Contains("EOL"))
                {
                    BizRuleID = "DA_SEL_DFCT_CELL_LIST_DE";
                }
                if (FORM_RSLT_SUM_GR_CODE.Contains("DEGAS_FRST")) dr["DEGAS_FRST"] = "Y";
                else if (FORM_RSLT_SUM_GR_CODE.Contains("DEGAS_RWK")) dr["DEGAS_RWK"] = "Y";
                else if (FORM_RSLT_SUM_GR_CODE.Contains("EOL_FRST")) dr["EOL_FRST"] = "Y";
                else if (FORM_RSLT_SUM_GR_CODE.Contains("EOL_RWK")) dr["EOL_RWK"] = "Y";
                
                if(!string.IsNullOrEmpty(DFCT_CODE)) dr["DFCT_CODE"] = DFCT_CODE;
                if(!string.IsNullOrEmpty(EQSGID)) dr["EQSGID"] = EQSGID;
                if(!string.IsNullOrEmpty(EQPTID)) dr["EQPTID"] = EQPTID;
                if(!string.IsNullOrEmpty(PROD_LOTID)) dr["PROD_LOTID"] = PROD_LOTID;
                if(!string.IsNullOrEmpty(LOTTYPE)) dr["LOTTYPE"] = LOTTYPE;
                if(!string.IsNullOrEmpty(RWK_FLAG)) dr["RWK_FLAG"] = RWK_FLAG;
                if(!string.IsNullOrEmpty(ROUT_TYPE_CODE)) dr["ROUT_TYPE_CODE"] = ROUT_TYPE_CODE;
                if (!string.IsNullOrEmpty(LOT_COMNET)) dr["PRE_DFCT_CODE"] = LOT_COMNET;
                //if (!string.IsNullOrEmpty()) dr["PRE_DFCT_CODE"] = LOT_COMNET;
                dtRqst.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizRuleID, "INDATA", "OUTDATA", dtRqst);
                dgCellList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        #endregion

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
    }
}
