/*************************************************************************************
 Created Date : 2020.09.21
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.21  담당자           Initialize
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
    public partial class PACK003_002_HISTORY_POPUP : C1Window, IWorkArea
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
        public PACK003_002_HISTORY_POPUP()
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
            this.txtRequestNo.Text = Util.NVC(obj[0]);  // 요청번호
            this.txtRequestLOTQty.Text = string.Format("{0:#,0}", Convert.ToDecimal(Util.NVC(obj[1]))); // 요청 Cell 수량
            this.txtMovePalletQty.Text = string.Format("{0:#,0}", Convert.ToDecimal(Util.NVC(obj[4]))); // 이동중 PLT 수량
            this.txtMoveLOTQty.Text = string.Format("{0:#,0}", Convert.ToDecimal(Util.NVC(obj[5])));    // 이동중 Cell 수량
            this.txtAssyarea.Text = Util.NVC(obj[6]);  // 조립동
            this.txtAssyline.Text = Util.NVC(obj[7]);  // 조립LINE
            this.txtElecline.Text = Util.NVC(obj[8]);  // 전극LINE
            this.txtUser.Text = Util.NVC(obj[9]);  // 요청자
            this.txtDttm.Text = Util.NVC(obj[10]);  // 요청일시

            this.cboRequestStatus.isAllUsed = true;
            cboRequestStatus.ApplyTemplate();
            this.SetMultiSelectionBoxRequestStatus(this.cboRequestStatus);

            this.SearchProcess(this.txtRequestNo.Text);
        }
        #endregion

        #region Method
        private void SetMultiSelectionBoxRequestStatus(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_LOGIS_TRF_REQ_STAT_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            cboMulti.Check(i);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //Search - 상세 이력조회
        public void SearchProcess(string requestNo)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
                dtRQSTDT.Columns.Add("TRF_CST_STAT_CODE", typeof(string));
                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRF_REQ_NO"] = requestNo.ToString();
                dr["TRF_CST_STAT_CODE"] = this.cboRequestStatus.SelectedItemsToString;
                dtRQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_REQ_DATA_DETL", dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgComfhist, dtResult, FrameOperation);
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess(this.txtRequestNo.Text);
        }
        #endregion
    }
}
