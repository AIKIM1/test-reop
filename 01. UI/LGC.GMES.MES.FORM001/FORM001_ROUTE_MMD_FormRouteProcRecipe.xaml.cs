using System;
using System.Windows;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_ROUTE_MMD_FormRouteProcRecipe.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_ROUTE_MMD_FormRouteProcRecipe : C1Window, IWorkArea
    {
        
        
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_ROUTE_MMD_FormRouteProcRecipe()
        {
            InitializeComponent();
        }

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

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void dgProcRecipe_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            string[] Col = { "ROUTID", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgProcRecipe, Col);
        }

        private void dgProcRecipe_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgProcRecipe);
        }

        #endregion

        #region Mehod

        private void InitializeGrid()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgProcRecipe);
                //SetDataGridCheckHeaderInitialize(dgProcRecipe);

                const string bizRuleName = "MMD_SEL_FORM_ROUTE_PROC_RECIPE_LIST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
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

                    dgProcRecipe.ItemsSource = DataTableConverter.Convert(result);

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
