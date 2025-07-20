/*************************************************************************************
 Created Date : 2020.09.22
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.22  김길용           Initialize
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
    public partial class PACK003_003_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sMtrlid = string.Empty;
        private string sMtrlname = string.Empty;
        private string sPlanqty = string.Empty;
        private string sFlagYN = string.Empty;
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
        public PACK003_003_POPUP()
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
                sMtrlid = Util.NVC(tmps[0]);
                sMtrlname = Util.NVC(tmps[1]);
                sPlanqty = Util.NVC(tmps[2]);
                sFlagYN = Util.NVC(tmps[3]);

                txtCellProdid.Text = Util.NVC(sMtrlname.ToString());
                txtTotalQty.Text = Util.NVC(sPlanqty.ToString());

                DetailList(sMtrlid, sFlagYN);
            }
            else
            {
            }
            
        }
        #endregion

        #region Method
        public void DetailList(string sMtrlid, string sFlagYN)
        {
            DataSet dsInput = new DataSet();

            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("LOGIS_FLAG", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MTRLID"] = sMtrlid;
                dr["LOGIS_FLAG"] = sFlagYN;

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CELL_LOGIS_PLAN_DETL", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgExceptLotList, dtResult, FrameOperation);
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
