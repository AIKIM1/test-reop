/*************************************************************************************
 Created Date : 2021.01.22
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.22  김길용           Initialize
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
    public partial class PACK003_006_CSTLOTINFO : C1Window, IWorkArea
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
        public PACK003_006_CSTLOTINFO()
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
            string carrierID = (Util.NVC(obj[0]));
            string palletid = (Util.NVC(obj[1]));
            this.SearchProcess(carrierID, palletid);
        }
        #endregion

        #region Method
        //Search - 상세 이력조회
        public void SearchProcess(string carrierID, string palletid)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                dtRQSTDT.Columns.Add("PLTID", typeof(string));
                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrierID;
                dr["PLTID"] = palletid;
                dtRQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_CELL_CSTPLT_MAPPINGINFO", dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, Util.NVC(dtResult.Rows.Count));
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
