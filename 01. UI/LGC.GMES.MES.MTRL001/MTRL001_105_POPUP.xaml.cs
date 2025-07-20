/*************************************************************************************
 Created Date : 2022.09.21
      Creator : 주동석
   Decription : 사전 자재 요청 팝업
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2022.12.01      주동석 :                           Initial Created.
  2024.08.13      정재홍 :    E20240626-000963       IWMS 출고 후 PDA Scan 대기 모니터링     

***************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions; 
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Controls.Primitives;



namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_105_POPUP : C1Window
    {
        #region #. Declaration & Constructor
        public MTRL001_105_POPUP()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; internal set; }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

            

        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            object[] tmps = C1WindowExtension.GetParameters(this);

            // CSR : E20240626-000963
            txtPlltID.Text = tmps[1].ToString();
            txtMTRL_LOTID.Text = tmps[2].ToString();

            SearchData(tmps);
        }

        private void init()
        {
            Util.gridClear(dgRMTRLList);
        }

        private void SearchData(object[] tmps)
        {
            try
            {
                init();
                string bizRule = string.Empty;
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                // CSR : E20240626-000963 기존 조회 조건 주석 처리
                //dtINDATA.Columns.Add("LANGID", typeof(string));
                //dtINDATA.Columns.Add("MTRLTYPE", typeof(string));
                //dtINDATA.Columns.Add("MTRLID", typeof(string));
                //dtINDATA.Columns.Add("SEARCHID", typeof(string));
                //dtINDATA.Columns.Add("INPUT_DTTM", typeof(string));

                //DataRow Indata = dtINDATA.NewRow();
                //Indata["LANGID"] = LoginInfo.LANGID;
                //Indata["MTRLTYPE"] = tmps[0].ToString();
                //Indata["MTRLID"] = tmps[1].ToString() == "" ? null : tmps[1].ToString();
                //Indata["SEARCHID"] = tmps[2].ToString() == "" ? null : tmps[2].ToString().Trim();
                //Indata["INPUT_DTTM"] = Convert.ToDateTime(tmps[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("PLLT_ID", typeof(string));
                dtINDATA.Columns.Add("MTRL_LOTID", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MTRLID"] = tmps[0].ToString();
                Indata["PLLT_ID"] = tmps[1].ToString().Trim();
                Indata["MTRL_LOTID"] = tmps[2].ToString().Trim();

                dtINDATA.Rows.Add(Indata);

                // CSR : E20240626-000963
                //bizRule = "BR_BAS_TB_SFC_RMTRL_INPUT_HIST_BY_RMTRL";
                bizRule = "DA_PRD_SEL_SFC_RMTRL_INPUT_HIST_CNT_MNG_DETAIL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "INDATA", "OUTDATA", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("101259", tmps[1].ToString());
                        this.Close();
                    }

                    Util.GridSetData(dgRMTRLList, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}