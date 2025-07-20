/*************************************************************************************
 Created Date : 2023.09.23
      Creator : 백광영
   Decription : 조립 원자재 재고현황 - On Hand 자재 현황
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_390_ONHAND : C1Window, IWorkArea
    {
        string _MtrlPortID = string.Empty;
        string _ReqStatus = string.Empty;

        public COM001_390_ONHAND()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            _MtrlPortID = Util.NVC(tmps[0]);
            _ReqStatus = Util.NVC(tmps[1]);

            txtMtrlPortID.Text = _MtrlPortID;
            _getMtrlStock();
        }

        private void _getMtrlStock()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                RQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRL_PORT_ID"] = _MtrlPortID;
                dr["REQ_STAT_CODE"] = _ReqStatus;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROD_RACK_MTRL_BOX_STCK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    //Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                    return;
                }
                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
    }
}
