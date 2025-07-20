/*************************************************************************************
 Created Date : 2022.12.15
      Creator : 이윤중
   Decription : Route 정보 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.15  이윤중 : Initial Created. (MMD - [Route 관리] 화면 기반)
  2023.10.17  주훈   : LGC.GMES.MES.FROM001.FROM001_ROUTE_MMD_RouteGradeMJudgSet 초기 복사
**************************************************************************************/
using System;
using System.Windows;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_ROUTE_MMD_RouteGradeMJudgSet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_ROUTE_MMD_RouteGradeMJudgSet : C1Window, IWorkArea
    {
        #region Declaration & Constructor

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
        public FCS002_ROUTE_MMD_RouteGradeMJudgSet()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= C1Window_Loaded;

            object[] param = C1WindowExtension.GetParameters(this);

            string sAREAID = Util.NVC(param[0]);
            string sAREANAME = Util.NVC(param[1]);

            string sEQSGID = Util.NVC(param[2]);
            string sEQSGNAME = Util.NVC(param[3]);

            string sMODLID = Util.NVC(param[4]);
            string sMODLNAME = Util.NVC(param[5]);

            string sROUTID = Util.NVC(param[6]);
            string sROUTNAME = Util.NVC(param[7]);

            string sROUTID_TYPE_ID = Util.NVC(param[8]);
            string sROUT_TYPE_NAME = Util.NVC(param[9]);


            txtAREA.Text = sAREANAME;
            txtAREA.Tag = sAREAID;

            txtEQSG.Text = sEQSGNAME;
            txtEQSG.Tag = sEQSGID;

            txtMODL.Text = sMODLNAME;
            txtMODL.Tag = sMODLID;

            txtROUT.Text = sROUTNAME;
            txtROUT.Tag = sROUTID;

            txtROUT_TYPE.Text = sROUT_TYPE_NAME;
            txtROUT_TYPE.Tag = sROUTID_TYPE_ID;

            InitializeGrid();

            //SetCellInfo(true, false, true);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void dgDeltaOCV_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //string[] Col = { "ROUTID", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgDeltaOCV, Col);
        }

        private void dgDeltaOCV_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgDeltaOCV);
        }

        #endregion

        #region Mehod

        private void InitializeGrid()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgCjudg);

                const string bizRuleName = "MMD_SEL_ROUT_GRD_MJUDG_SET_LIST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["USERID"] = LoginInfo.USERID;
                inData["AREAID"] = txtAREA.Tag;
                inData["ROUTID"] = txtROUT.Tag;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgCjudg.ItemsSource = DataTableConverter.Convert(result);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}
