/*************************************************************************************
 Created Date : 2021.01.14
      Creator : 
   Decription : Trouble 상세내역
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_066_TROUBLE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        string _EQPTID = "";
        string _TIME = "";
        int _V_SECOND;
        string _WRK_DATE;

        int _MAX_SEQNO = 0;

        DataTable _dtBeforeSet = new DataTable();
        public FCS001_066_TROUBLE()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _EQPTID = Util.NVC(tmps[0]);
                _TIME = Util.NVC(tmps[1]);
                _V_SECOND = Convert.ToInt32(tmps[2]);
                _WRK_DATE = Util.NVC(tmps[3]);

            }
            SetTrouble();
        }
       
        private void SetTrouble()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
               // dtRqst.Columns.Add("V_SECOND", typeof(Int32));
                dtRqst.Columns.Add("STRT_DTTM", typeof(string));
                dtRqst.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _EQPTID.Substring(1);
              //  dr["V_SECOND"] = _V_SECOND;
                dr["STRT_DTTM"] = _TIME;
                dr["WRK_DATE"] = _WRK_DATE;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_TRBL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    lblEqptID.Content = dtRslt.Rows[0]["EQPTNAME"].ToString();
                    lblEioStat.Content = Convert.ToString(dtRslt.Rows[0]["EIOSTAT"]);
                    lblCode.Content = dtRslt.Rows[0]["TRBL_CODE"].ToString();
                    lblName.Content = dtRslt.Rows[0]["TRBL_NAME"].ToString();
                    lblStrtDttm.Content = dtRslt.Rows[0]["STRT_DTTM"].ToString();
                    lblEndDttm.Content = dtRslt.Rows[0]["END_DTTM"].ToString();
                    lblTermSec.Content = dtRslt.Rows[0]["SECOND"].ToString();
                    lblTerm.Content = dtRslt.Rows[0]["MINUTE"].ToString();
                    lblLossCode.Content = dtRslt.Rows[0]["LOSS_NAME"].ToString();
                    lblLossDetlCode.Content = dtRslt.Rows[0]["LOSS_DETL_NAME"].ToString();
                    lblRemark.Content = dtRslt.Rows[0]["LOSS_NOTE"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}
