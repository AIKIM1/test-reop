/*************************************************************************************
 Created Date : 2020.12.17
      Creator : 정용석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.17  정용석           Initialize
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_002_REQUEST_HISTORY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string strReturnID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private DataTable isListTable = new DataTable();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_002_REQUEST_HISTORY()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        #endregion

        #region Event
        //최초 Load
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] obj = C1WindowExtension.GetParameters(this);
            if (obj == null || obj.Length <= 0)
            {
                return;
            }
            string equipmentSegmentID = (Util.NVC(obj[0]));
            string productID = (Util.NVC(obj[1]));
            this.SearchProcess(equipmentSegmentID, productID);
        }
        #endregion

        #region Method
        //Search - 상세 이력조회
        public void SearchProcess(string equipmentSegmentID, string productID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = equipmentSegmentID;
                dr["PRODID"] = productID;
                dr["PRJT_NAME"] = null;
                dtRQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_REQ_QTY", dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(this.dgRequestList, dtResult, FrameOperation);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
