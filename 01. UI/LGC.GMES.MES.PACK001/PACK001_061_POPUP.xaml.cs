/*************************************************************************************
 Created Date : 2020.06.08
      Creator : 강호운
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.08  강호운           Initialize
**************************************************************************************/


using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;

using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_044_POPUP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_061_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string strLotID = string.Empty;
        private string strInsp_seqs = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private DataTable isListTable = new DataTable();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK001_061_POPUP()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            string[] tmp = null;
            
            if (tmps != null && tmps.Length >= 1)
            {
                strLotID = Util.NVC(tmps[0]);
                strInsp_seqs = Util.NVC(tmps[1]);
            }
            else
            {
                strLotID = "";
                strInsp_seqs = "";
            }
            DetailList(strLotID, strInsp_seqs);
        }
        #endregion

        #region Method
        public void DetailList(string sLotid, string sInsp_seqs)
        {
            DataSet dsInput = new DataSet();
            DataTable INDATA = new DataTable();

            try
            {
                ShowLoadingIndicator();
                DoEvents();
                
                DataTable inTable = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("INSP_SEQS", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid.Trim();
                dr["PROCID"] = "P5400";
                INDATA.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_2ND_OCV_INSP_DATACOLLECT_DETAIL_LIST", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgExceptLotList, dtResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion
    }
}
