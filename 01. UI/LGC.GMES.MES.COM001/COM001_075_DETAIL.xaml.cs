/*************************************************************************************
 Created Date : 2024.04.17
      Creator : 김도형
   Decription : 특이사항 ->전극 생산일별 특이사항(NEW)-> 물류 일별 노트 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.17  김도형 : Initial Created.
  2024.04.17  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건
  2024.04.20  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건( PRODID_BY_PJT)
  2024.06.03  김도형 : [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Forms.VisualStyles;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_075_DETAIL Log Biz Rule 상세조회
    /// </summary>
    public partial class COM001_075_DETAIL : C1Window, IWorkArea
    {        
        private string _HistSEQ = string.Empty;                
        private readonly BizDataSet _bizDataSet = new BizDataSet();   

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_075_DETAIL()
        {
            InitializeComponent();
         
        }       

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _HistSEQ = tmps[0].GetString();                    
                }

                DetailLog();         
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }      
                    
        private void DetailLog()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PROD_SEL_BZRULE_ERR_DATASET_INFO";

                DataTable inDataTable = new DataTable("RQSTDT");

                inDataTable.Columns.Add("HIST_SEQNO", typeof(string));

                DataRow dr = inDataTable.NewRow();

                dr["HIST_SEQNO"] = _HistSEQ;
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtLogDetail.Text = dtResult.Rows[0]["DATASET"].ToString();
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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
    }
}
